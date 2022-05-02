using CapyCSS.Controls.BaseControls;
using CapyCSS.Script;
using CapyCSS.Controls;
using CbVS;
using CbVS.Script;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Xml;
using System.Xml.Serialization;

namespace CapyCSS.Controls.BaseControls
{
    public interface IHaveCommandCanvas
    {
        CommandCanvas OwnerCommandCanvas { get; set; }
    }


    public interface IAsset
    {
        int AssetId { get; set; }
    }
    public class AssetIdProvider
    {
        private static Dictionary<int, object> AssetList = new Dictionary<int, object>();
        private static int _assetId = 0;
        private int assetId = ++_assetId;    // 初期化時に _pointID をインクリメントする
        public int AssetId
        {
            get => assetId;
            set
            {
                if (value >= _assetId)
                {
                    // _pointID は常に最大にする

                    _assetId = value + 1;
                }
                assetId = value;
            }
        }
        public AssetIdProvider(object owner)
        {
            if (owner is null)
                new NotImplementedException();
            AssetList.Add(AssetId, owner);
        }
        ~AssetIdProvider()
        {
            AssetList.Remove(AssetId);
        }
    }

    /// <summary>
    /// CommandCanvas.xaml の相互作用ロジック
    /// </summary>
    public partial class CommandCanvas 
        : UserControl
        , IAsset
        , IDisposable
    {
        public static readonly int DATA_VERSION = 3;

        #region ID管理
        private AssetIdProvider assetIdProvider = null;
        public int AssetId
        {
            get => assetIdProvider.AssetId;
            set { assetIdProvider.AssetId = value; }
        }
        #endregion

        #region XML定義
        [XmlRoot(nameof(CommandCanvas))]
        public class _AssetXML<OwnerClass> : IDisposable
            where OwnerClass : CommandCanvas
        {
            [XmlIgnore]
            public Action WriteAction = null;
            [XmlIgnore]
            public Action<OwnerClass> ReadAction = null;
            private bool disposedValue;

            public _AssetXML()
            {
                ReadAction = (self) =>
                {
                    self.AssetId = AssetId;
                    self._inportNameSpaceModule = ImportNameSpaceModule;
                    self._inportDllModule = ImportDllModule;
                    self._inportNuGetModule = ImportNuGetModule;

                    {// 不要なモジュールを削除する
                        List<string> importModules = new List<string>();
                        foreach (var module in ImportNameSpaceModule)
                        {
                            importModules.Add(ModuleControler.HEADER_NAMESPACE + module);
                        }
                        foreach (var module in ImportDllModule)
                        {
                            importModules.Add(ModuleControler.HEADER_DLL + module);
                        }
                        foreach (var module in ImportNuGetModule)
                        {
                            importModules.Add(ModuleControler.HEADER_NUGET + module);
                        }

                        // 基本的なインポートモジュールを追加
                        importModules.AddRange(self.ApiImporter.GetBaseImportList());
                        self.ApiImporter.ClearModule(new List<string>(importModules.Distinct()));
                    }

                    if (self.ImportModule())
                    {
                        SetupCanvas(self);
                    }

                    // 次回の為の初期化
                    self.AssetXML = new _AssetXML<CommandCanvas>(self);
                };
            }

            private void SetupCanvas(OwnerClass self)
            {
                self.WorkCanvas.OwnerCommandCanvas = self;
                self.WorkCanvas.AssetXML = WorkCanvas;
                self.WorkCanvas.AssetXML.ReadAction?.Invoke(self.WorkCanvas);

                self.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (DataVersion != DATA_VERSION)
                    {
                        ControlTools.ShowErrorMessage(CapyCSS.Language.Instance["Help:DataVersionError"]);
                        return;
                    }

                    if (WorkStack != null)
                    {
                        self.WorkStack.OwnerCommandCanvas = self;
                        self.WorkStack.AssetXML = WorkStack;
                        self.WorkStack.AssetXML.ReadAction?.Invoke(self.WorkStack);
                    }
                    foreach (var node in WorkCanvasAssetList)
                    {
                        try
                        {
                            Movable movableNode = new Movable(this);
                            self.WorkCanvas.Add(movableNode);

                            movableNode.OwnerCommandCanvas = self;
                            movableNode.AssetXML = node;
                            movableNode.AssetXML.ReadAction?.Invoke(movableNode);

                            // レイヤー設定
                            if (movableNode.ControlObject is IDisplayPriority dp)
                            {
                                Canvas.SetZIndex(movableNode, dp.Priority);
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine(nameof(CommandCanvas) + "._AssetXML(ReadAction): " + ex.Message);
                        }
                    }

                }), DispatcherPriority.ApplicationIdle);
            }

            public _AssetXML(OwnerClass self)
            {
                WriteAction = () =>
                {
                    AssetId = self.AssetId;
                    DataVersion = DATA_VERSION;
                    ImportNameSpaceModule = self.ApiImporter.NameSpaceModuleList;
                    ImportDllModule = self.ApiImporter.DllModulePathList;
                    ImportNuGetModule = self.ApiImporter.NuGetModuleList;
                    self.WorkCanvas.AssetXML.WriteAction?.Invoke();
                    WorkCanvas = self.WorkCanvas.AssetXML;
                    self.WorkStack.AssetXML.WriteAction?.Invoke();
                    WorkStack = self.WorkStack.AssetXML;
                    List<Movable._AssetXML<Movable>> workList = new List<Movable._AssetXML<Movable>>();
                    foreach (var node in self.WorkCanvas)
                    {
                        if (node is Movable target)
                        {
                            target.AssetXML.WriteAction?.Invoke();
                            workList.Add(target.AssetXML);
                        }
                    }
                    WorkCanvasAssetList = workList;
                };
            }
            [XmlAttribute("Id")]
            public int AssetId { get; set; } = 0;
            #region 固有定義
            public int DataVersion { get; set; } = 0;
            public BaseWorkCanvas._AssetXML<BaseWorkCanvas> WorkCanvas { get; set; } = null;
            public List<string> ImportNameSpaceModule { get; set; } = null;
            public List<string> ImportDllModule { get; set; } = null;
            public List<string> ImportNuGetModule { get; set; } = null;
            public Stack._AssetXML<Stack> WorkStack { get; set; } = null;
            [XmlArrayItem("Asset")]
            public List<Movable._AssetXML<Movable>> WorkCanvasAssetList { get; set; } = null;

            protected virtual void Dispose(bool disposing)
            {
                if (!disposedValue)
                {
                    if (disposing)
                    {
                        WriteAction = null;
                        ReadAction = null;

                        // 以下、固有定義開放
                        WorkCanvas?.Dispose();
                        WorkCanvas = null;
                        ImportNameSpaceModule?.Clear();
                        ImportNameSpaceModule = null;
                        ImportDllModule?.Clear();
                        ImportDllModule = null;
                        ImportNuGetModule?.Clear();
                        ImportNuGetModule = null;
                        WorkStack?.Dispose();
                        WorkStack = null;
                        CbSTUtils.ForeachDispose(WorkCanvasAssetList);
                        WorkCanvasAssetList = null;
                    }
                    disposedValue = true;
                }
            }

            public void Dispose()
            {
                Dispose(disposing: true);
                GC.SuppressFinalize(this);
            }
            #endregion
        }
        public _AssetXML<CommandCanvas> AssetXML { get; set; } = null;
        #endregion

        public CommandCanvas(CommandCanvasList commandCanvasList)
        {
            InitializeComponent();
            assetIdProvider = new AssetIdProvider(this);
            CommandCanvasControl = commandCanvasList;
            WorkCanvas.OwnerCommandCanvas = this;
            WorkStack.OwnerCommandCanvas = this;
            AssetXML = new _AssetXML<CommandCanvas>(this);

            ScriptCommandCanvas = this;
            ScriptWorkCanvas = WorkCanvas;
            ScriptWorkStack = WorkStack;

            CommandCanvasList.SetOwnerCursor(Cursors.Wait);

            TypeMenuWindow = CommandWindow.Create();
            TypeMenuWindow.Title = "Type";
            TypeMenuWindow.treeViewCommand.OwnerCommandCanvas = this;
            TypeMenuWindow.treeViewCommand.AssetTreeData = new ObservableCollection<TreeMenuNode>();
            MakeTypeMenu(TypeMenuWindow.treeViewCommand);

            CommandMenuWindow = CommandWindow.Create();
            CommandMenuWindow.treeViewCommand.OwnerCommandCanvas = this;
            CommandMenuWindow.treeViewCommand.AssetTreeData = new ObservableCollection<TreeMenuNode>();
            MakeCommandMenu(CommandMenuWindow.treeViewCommand);

            ClickEntryEvent = new Action(() =>
            {
                CommandCanvasList.SetOwnerCursor(null);
            });

            ClickExitEvent = new Action(() =>
            {
                CommandMenuWindow.CloseWindow();
                CommandCanvasList.SetOwnerCursor(Cursors.Hand);
            });

            DEBUG_Check();
            
            CommandCanvasList.SetOwnerCursor(null);
        }

        ~CommandCanvas()
        {
            Dispose();
        }

        //----------------------------------------------------------------------
        #region DEBUG
        [Conditional("DEBUG")]
        private void DEBUG_Check()
        {
            Console.Write(nameof(CommandCanvas) + $": check...");

            Type[] valueTypes = new Type[]
            {
                typeof(Byte),
                typeof(SByte),
                typeof(Int16),
                typeof(Int32),
                typeof(Int64),
                typeof(UInt16),
                typeof(UInt32),
                typeof(UInt64),
                typeof(Char),
                typeof(Single),
                typeof(Double),
                typeof(Decimal),
            };

            // 代入チェックのチェック
            Debug.Assert(CbSTUtils.IsAssignment(typeof(Action), typeof(Action)));
            Debug.Assert(CbSTUtils.IsAssignment(typeof(Action<int>), typeof(Action<int>)));
            Debug.Assert(CbSTUtils.IsAssignment(typeof(Func<int>), typeof(Func<int>)));
            Debug.Assert(CbSTUtils.IsAssignment(typeof(Func<Type>), typeof(Func<Type>)));
            Debug.Assert(CbSTUtils.IsAssignment(typeof(Action), typeof(int)));
            Debug.Assert(CbSTUtils.IsAssignment(typeof(Action<string>), typeof(int)));
            Debug.Assert(CbSTUtils.IsAssignment(typeof(Func<int>), typeof(int)));
            Debug.Assert(CbSTUtils.IsAssignment(typeof(Func<Type>), typeof(Type)));
            Debug.Assert(CbSTUtils.IsAssignment(typeof(Func<Func<int>>), typeof(Func<int>)));

            foreach (var valueType in valueTypes)
                Debug.Assert(CbSTUtils.IsAssignment(valueType, valueType));
            Debug.Assert(CbSTUtils.IsAssignment(typeof(object), typeof(object)));
            foreach (var valueType in valueTypes)
                Debug.Assert(CbSTUtils.IsAssignment(typeof(object), valueType));
            Debug.Assert(CbSTUtils.IsAssignment(typeof(Action), typeof(CbVoid)));
            Debug.Assert(!CbSTUtils.IsAssignment(typeof(object), typeof(CbVoid)));
            Debug.Assert(CbSTUtils.IsAssignment(typeof(CbVoid), typeof(CbVoid)));
            Debug.Assert(CbSTUtils.IsAssignment(typeof(CbVoid), typeof(object)));
            Debug.Assert(CbSTUtils.IsAssignment(typeof(List<int>), typeof(List<int>)));
            Debug.Assert(CbSTUtils.IsAssignment(typeof(IEnumerable<int>), typeof(List<int>)));
            Debug.Assert(CbSTUtils.IsAssignment(typeof(ICollection<int>), typeof(List<int>)));
            Debug.Assert(CbSTUtils.IsAssignment(typeof(IList<int>), typeof(List<int>)));
            Debug.Assert(CbSTUtils.IsAssignment(typeof(IEnumerable<int>), typeof(IList<int>)));
            Debug.Assert(CbSTUtils.IsAssignment(typeof(int), typeof(int)));

            // Void は object と Action 以外に代入できない
            Debug.Assert(!CbSTUtils.IsAssignment(typeof(bool), typeof(CbVoid)));
            Debug.Assert(!CbSTUtils.IsAssignment(typeof(string), typeof(CbVoid)));
            Debug.Assert(!CbSTUtils.IsAssignment(typeof(Type), typeof(CbVoid)));
            Debug.Assert(!CbSTUtils.IsAssignment(typeof(List<int>), typeof(CbVoid)));
            foreach (var valueType in valueTypes)
                Debug.Assert(!CbSTUtils.IsAssignment(valueType, typeof(CbVoid)));

            foreach (var valueType in valueTypes)
                Debug.Assert(!CbSTUtils.IsAssignment(typeof(string), valueType));

            // object と Void に対する代入チェック
            foreach (var valueType in valueTypes)
            {
                Debug.Assert(CbSTUtils.IsAssignment(typeof(object), valueType));
                Debug.Assert(CbSTUtils.IsAssignment(valueType, typeof(object), true));
                Debug.Assert(CbSTUtils.IsAssignment(typeof(CbVoid), valueType));
            }
            Debug.Assert(CbSTUtils.IsAssignment(typeof(CbVoid), typeof(object)));
            Debug.Assert(!CbSTUtils.IsAssignment(typeof(object), typeof(CbVoid)));
            Debug.Assert(CbSTUtils.IsAssignment(typeof(CbVoid), typeof(CbVoid)));

            // 代入のサンプリングチェック
            Debug.Assert(CbSTUtils.IsAssignment(typeof(object), typeof(List<int>)));
            Debug.Assert(CbSTUtils.IsAssignment(typeof(CbVoid), typeof(List<int>)));

            // キャストのサンプリングチェック
            Debug.Assert(CbSTUtils.IsAssignment(typeof(long), typeof(int), true));
            Debug.Assert(CbSTUtils.IsAssignment(typeof(float), typeof(int), true));
            Debug.Assert(CbSTUtils.IsAssignment(typeof(double), typeof(int), true));
            Debug.Assert(CbSTUtils.IsAssignment(typeof(decimal), typeof(int), true));
            Debug.Assert(CbSTUtils.IsAssignment(typeof(ushort), typeof(char), true));
            Debug.Assert(CbSTUtils.IsAssignment(typeof(long), typeof(char), true));
            Debug.Assert(CbSTUtils.IsAssignment(typeof(float), typeof(char), true));
            Debug.Assert(CbSTUtils.IsAssignment(typeof(short), typeof(byte), true));
            Debug.Assert(CbSTUtils.IsAssignment(typeof(ushort), typeof(byte), true));
            Debug.Assert(CbSTUtils.IsAssignment(typeof(uint), typeof(byte), true));
            Debug.Assert(CbSTUtils.IsAssignment(typeof(ulong), typeof(byte), true));
            Debug.Assert(CbSTUtils.IsAssignment(typeof(double), typeof(float), true));
            Debug.Assert(CbSTUtils.IsAssignment(typeof(string), typeof(object), true));
            Debug.Assert(CbSTUtils.IsAssignment(typeof(Type), typeof(object), true));
            Debug.Assert(CbSTUtils.IsAssignment(typeof(List<int>), typeof(object), true));

            // 型名変換チェック
            Debug.Assert(CbSTUtils.GetTypeName(typeof(int)) == "int");
            Debug.Assert(CbSTUtils.GetTypeName(typeof(Int32)) == "int");
            Debug.Assert(CbSTUtils.GetTypeName(typeof(Int64?)) == "long?");
            Debug.Assert(CbSTUtils.GetTypeName(typeof(Type)) == "Type");
            Debug.Assert(CbSTUtils.GetTypeName(typeof(Rect)) == "Rect");
            Debug.Assert(CbSTUtils.GetTypeName(typeof(Rect?)) == "Rect?");
            Debug.Assert(CbSTUtils.GetTypeName(typeof(Action<bool>)) == "Action<bool>");
            Debug.Assert(CbSTUtils.GetTypeName(typeof(List<int>)) == "List<int>");
            Debug.Assert(CbSTUtils.GetTypeName(typeof(IDictionary<int, short?>)) == "IDictionary<int,short?>");
            Debug.Assert(CbSTUtils.GetTypeName(typeof(IDictionary<int, Nullable<short>>)) == "IDictionary<int,short?>");
            Debug.Assert(CbSTUtils.GetTypeName(typeof(System.PlatformID)) == "PlatformID");

            Debug.Assert(CbSTUtils.GetTypeFullName(typeof(int)) == "System.Int32");
            Debug.Assert(CbSTUtils.GetTypeFullName(typeof(Rect)) == "System.Windows.Rect");
            Debug.Assert(CbSTUtils.GetTypeFullName(typeof(Rect?)) == "System.Nullable<System.Windows.Rect>");
            Debug.Assert(CbSTUtils.GetTypeFullName(typeof(IDictionary<int, Nullable<short>>)) == "System.Collections.Generic.IDictionary<System.Int32,System.Nullable<System.Int16>>");
            Debug.Assert(CbSTUtils.GetTypeFullName(typeof(System.PlatformID)) == "System.PlatformID");

            // ジェネリック型名変換チェック
            Debug.Assert(CbSTUtils.GetGenericTypeName(typeof(List<>)) == "List<T>");
            Debug.Assert(CbSTUtils.StripParamater(CbSTUtils.GetGenericTypeName(typeof(List<>))) == "List<>");
            Debug.Assert(CbSTUtils.GetGenericTypeName(typeof(Dictionary<,>)) == "Dictionary<TKey, TValue>");
            Debug.Assert(CbSTUtils.GetGenericTypeName(typeof(Dictionary<string, bool>)) == "Dictionary<string, bool>");
            Debug.Assert(CbSTUtils.GetGenericTypeName(typeof(List<Dictionary<int, double>>)) == "List<Dictionary<int, double>>");
            Debug.Assert(CbSTUtils.StripParamater(CbSTUtils.GetGenericTypeName(typeof(Dictionary<string, bool>))) == "Dictionary<,>");

            // 型生成チェック
            void CheckCreateCbType(Type type, Type ifc = null)
            {
                var cbType = CbST.CbCreate(type);
                Debug.Assert(cbType != null);
                if (cbType.OriginalType == typeof(CbGeneMethArg))
                    return;
                Debug.Assert(cbType.OriginalType == type);
                if (ifc != null)
                    Debug.Assert(ifc.IsAssignableFrom(cbType.GetType()));
            }

            foreach (var valueType in valueTypes)
                CheckCreateCbType(valueType, typeof(ICbValueClass<>).MakeGenericType(new Type[] { valueType }));
            CheckCreateCbType(typeof(bool), typeof(ICbValueClass<bool>));
            CheckCreateCbType(typeof(string), typeof(ICbValueClass<string>));
            CheckCreateCbType(typeof(object), typeof(ICbValueClass<object>));
            
            CheckCreateCbType(typeof(Type), typeof(ICbClass));
            CheckCreateCbType(typeof(Rect), typeof(ICbStruct));
            
            CheckCreateCbType(typeof(Action), typeof(ICbEvent));
            CheckCreateCbType(typeof(Action<bool>), typeof(ICbEvent));
            CheckCreateCbType(typeof(Action<ushort, Type>), typeof(ICbEvent));
            CheckCreateCbType(typeof(Func<object>), typeof(ICbEvent));
            CheckCreateCbType(typeof(Func<sbyte, decimal>), typeof(ICbEvent));
            CheckCreateCbType(typeof(Converter<double, int>), typeof(ICbEvent));
            CheckCreateCbType(typeof(Predicate<double>), typeof(ICbEvent));

            CheckCreateCbType(typeof(List<byte>), typeof(ICbList));
            CheckCreateCbType(typeof(IList<byte>), typeof(ICbList));
            CheckCreateCbType(typeof(IEnumerable<float>), typeof(ICbList));
            CheckCreateCbType(typeof(List<Dictionary<int, double>>), typeof(ICbList));
            CheckCreateCbType(typeof(IEnumerable<Dictionary<int, double>>), typeof(ICbList));
            
            CheckCreateCbType(typeof(Dictionary<short, char>));

            // 型生成時ジェネリックパラメータの置き換えチェック
            void CheckGenericType(ICbValue cbType)
            {
                Debug.Assert(cbType != null);
                Debug.Assert(cbType.MyType != null);
                foreach (var arg in cbType.MyType.GetGenericArguments())
                {
                    Debug.Assert(arg != null);
                    Debug.Assert(arg == typeof(CbGeneMethArg));
                }
            }

            CheckCreateCbType(typeof(Func<>), typeof(ICbEvent));
            CheckCreateCbType(typeof(Func<,>), typeof(ICbEvent));
            CheckCreateCbType(typeof(Converter<,>), typeof(ICbEvent));
            CheckGenericType(CbST.CbCreate(typeof(List<>)));
            CheckGenericType(CbST.CbCreate(typeof(IEnumerable<>)));
            CheckGenericType(CbST.CbCreate(typeof(Dictionary<,>)));

            // Nullable<T>型生成チェック
            void CheckNullable(ICbValue cbType)
            {
                Debug.Assert(cbType.IsNullable);
            }
            CheckNullable(CbST.CbCreate(typeof(int?)));
            CheckNullable(CbST.CbCreate(typeof(DateTime?)));
            foreach (var valueType in valueTypes)
                CheckNullable(CbST.CbCreate(typeof(Nullable<>).MakeGenericType(new Type[] { valueType })));

            Console.WriteLine("ok");
        }
#endregion

        //----------------------------------------------------------------------
#region スクリプト内共有
        private List<string> _inportNameSpaceModule = null;
        private List<string> _inportDllModule = null;
        private List<string> _inportNuGetModule = null;
        public ApiImporter ApiImporter = null;
        private ModuleControler moduleControler = null;
        public CommandWindow CommandMenuWindow = null;
        public CommandWindow TypeMenuWindow = null;
        public static String SelectType = null;
        public CommandCanvasList CommandCanvasControl = null;
        public TreeViewCommand TypeMenu => TypeMenuWindow.treeViewCommand;
        public TreeViewCommand CommandMenu => CommandMenuWindow.treeViewCommand;
        public CommandCanvas ScriptCommandCanvas = null;
        public BaseWorkCanvas ScriptWorkCanvas = null;
        /// <summary>
        /// カーブ用キャンバスを参照します。
        /// </summary>
        public Canvas CurveCanvas => ScriptWorkCanvas.CurveCanvas;
        public Stack ScriptWorkStack = null;
        public Func<object> ScriptWorkClickEvent = null;
        public Action ClickEntryEvent = null;
        public Action ClickExitEvent = null;
        public HoldActionQueue<UIParam> UIParamHoldAction = new HoldActionQueue<UIParam>();
        public HoldActionQueue<StackGroup> StackGroupHoldAction = new HoldActionQueue<StackGroup>();
        public HoldActionQueue<PlotWindow> PlotWindowHoldAction = new HoldActionQueue<PlotWindow>();
        public HoldActionQueue<LinkConnectorList> LinkConnectorListHoldAction = new HoldActionQueue<LinkConnectorList>();
        private bool enabledScriptHoldActionMode = false;
        public bool EnabledScriptHoldActionMode
        {
            set
            {
                enabledScriptHoldActionMode = value;
                UIParamHoldAction.Enabled = value;
                StackGroupHoldAction.Enabled = value;
                PlotWindowHoldAction.Enabled = value;
                LinkConnectorListHoldAction.Enabled = value;
            }
            get => enabledScriptHoldActionMode; // true ならスクリプト実行中
        }

        /// <summary>
        /// クリック実行呼び出し処理用イベントを参照します。
        /// </summary>
        public Func<object> ClickEvent
        {
            get => ScriptWorkClickEvent;
            set
            {
                if (value is null)
                {
                    // クリックイベントを無効にする

                    ClickEntryEvent?.Invoke();

                    // コマンドツリー上でのコマンドキャンセルイベントを消す
                    CommandMenu.MouseRightButtonDown -= (s, e) => ScriptWorkCanvas.ResetCommand();
                }
                else
                {
                    // クリックイベントを有効にする

                    ClickExitEvent?.Invoke();

                    // コマンドツリー上でのコマンドキャンセルイベントを登録する
                    CommandMenu.MouseRightButtonDown += (s, e) => ScriptWorkCanvas.ResetCommand();
                }
                ScriptWorkClickEvent = value;
            }
        }

        /// <summary>
        /// キャンバス登録用のコマンドを作成します。
        /// </summary>
        /// <param name="path">コマンドの正式な名前</param>
        /// <param name="action">実行されるイベント</param>
        /// <param name="vm"></param>
        /// <returns>コマンド</returns>
        public TreeMenuNodeCommand CreateEventCanvasCommand(string path, Func<object> action)
        {
            return new TreeMenuNodeCommand((a) =>
                {
                    ClickEvent = action;
                    ScriptWorkCanvas.ObjectSetCommand = ClickEvent;
                    ScriptWorkCanvas.ObjectSetCommandName = path;
                    ScriptWorkCanvas.ObjectSetExitCommand = new Action(() => ClickEvent = null);
                }
            );
        }

        /// <summary>
        /// 即時実行用コマンドを作成します。
        /// </summary>
        /// <param name="vm"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public TreeMenuNodeCommand CreateImmediateExecutionCanvasCommand(Action action)
        {
            return new TreeMenuNodeCommand((a) =>
                {
                    if (CommandCanvasList.GetOwnerCursor() == Cursors.Wait)
                        return; // 処理中は禁止

                    CommandMenuWindow.CloseWindow();

                    action?.Invoke();
                }
            );
        }

#endregion

        //----------------------------------------------------------------------
#region アセットリストを実装

        private void MakeCommandMenu(TreeViewCommand treeViewCommand)
        {
            // コマンドを追加
            {
                var commandNode = new TreeMenuNode("Command");
                commandNode.AddChild(new TreeMenuNode("Clear(Ctrl+Shift+N)", CreateImmediateExecutionCanvasCommand(() => ClearWorkCanvasWithConfirmation())));
                commandNode.AddChild(new TreeMenuNode("Toggle ShowMouseInfo", CreateImmediateExecutionCanvasCommand(() => ScriptWorkCanvas.EnableInfo = ScriptWorkCanvas.EnableInfo ? false : true)));
                commandNode.AddChild(new TreeMenuNode("Toggle ShowGridLine(Ctrl+G)", CreateImmediateExecutionCanvasCommand(() => ScriptCommandCanvas.ToggleGridLine())));
                commandNode.AddChild(new TreeMenuNode("Save(Ctrl+S)", CreateImmediateExecutionCanvasCommand(() => CommandCanvasControl.SaveCbsFile())));
                commandNode.AddChild(new TreeMenuNode("Load(Ctrl+O)", CreateImmediateExecutionCanvasCommand(() => CommandCanvasControl.LoadCbsFile())));
                commandNode.AddChild(new TreeMenuNode("Convert C#", CreateImmediateExecutionCanvasCommand(() => CommandCanvasList.Instance?.BuildScriptAndOut())));
                treeViewCommand.AssetTreeData.Add(commandNode);
            }

            // 飾りを追加
            {
                var decorationNode = new TreeMenuNode("Decoration");
                decorationNode.AddChild(new TreeMenuNode("Text Area", CreateEventCanvasCommand(decorationNode.Name + ".Text Area", () => new GroupArea() { Name = "test", Width = 150, Height = 150 })));
                treeViewCommand.AssetTreeData.Add(decorationNode);
            }

            // 基本的なネームスペースを追加
            ApiImporter = new ApiImporter(this);
            MakeModuleControler(new List<ShortCutCommand>()
            {
                // ショートカットを追加：
                // BaseWorkCanvas.AddScriptCommandRecent(string name)
                // にブレークポイントを張ると name の内容でショートカットコードがわかります。

                new ShortCutCommand() { Name = "Create Literal/Instance", Command = "Program.Literal/Local.Literal/Local : T" },
                new ShortCutCommand() { Name = "Create Variable", Command = "Program.Variable.Create Variable : T" },
                new ShortCutCommand() { Name = "Get Variable", Command = "Program.Variable.Get Variable" },
                new ShortCutCommand() { Name = "Set Variable", Command = "Program.Variable.Set Variable" },
                new ShortCutCommand() { Name = "Reference Dummy Arguments", Command = "Program.Function.f(x).DummyArguments<T> : T" },
                new ShortCutCommand() { Name = "Void Sequence", Command = "Program.VoidSequence" },
                new ShortCutCommand() { Name = "Result Sequence", Command = "Program.ResultSequence(T) : T" },
            });

            ApiImporter.ImportBaseModule();
            ImportModule(); // 起動時に外部からインポートモジュールを設定される可能性がある
        }

        struct ShortCutCommand
        {
            public string Name { get; set; }
            public string Command { get; set; }
        }

        private void MakeModuleControler(IEnumerable<ShortCutCommand> shortCutCommands)
        {
            moduleControler = new ModuleControler(
                ApiImporter,
                CommandCanvasControl.DllDir,
                CommandCanvasControl.PackageDir
            );

            var stackPanel = new StackPanel();
            stackPanel.Orientation = Orientation.Vertical;
            stackPanel.Children.Add(moduleControler);
            foreach (var shortCutCommand in shortCutCommands)
            {
                var shortcutButton = new Button()
                {
                    Content = shortCutCommand.Name,
                    Height = 24,
                    Background = (Brush)Application.Current.FindResource("CommandShortcutButtonBackgroundBrush"),
                    BorderBrush = (Brush)Application.Current.FindResource("CommandShortcutButtonBorderBrushBrush"),
                    Margin = new Thickness( 0, 0, 0, 2),
                };
                shortcutButton.Click += (o, e) => CommandMenu.ExecuteFindCommand(shortCutCommand.Command);
                stackPanel.Children.Add(shortcutButton);
            }
            stackPanel.Margin = new Thickness(6, 4, 0, 6);
            moduleView.Content = stackPanel;
        }

        /// <summary>
        /// モジュールを読み込みます。
        /// </summary>
        /// <returns>成功したらtrue</returns>
        public bool ImportModule()
        {
            if (_inportNameSpaceModule != null)
            {
                // ネームスペースインポートの復元

                foreach (var imp in _inportNameSpaceModule)
                {
                    ApiImporter.ImportNameSpace(imp);
                }
                _inportNameSpaceModule = null;
            }
            if (_inportNuGetModule != null)
            {
                // NuGetインポートの復元

                foreach (var imp in _inportNuGetModule)
                {
                    ApiImporter.ImportNuGet(CommandCanvasControl.PackageDir, imp);
                }
                _inportNuGetModule = null;
            }
            if (_inportDllModule != null)
            {
                // DLL インポートの復元

                foreach (var imp in _inportDllModule)
                {
                    if (!moduleControler.CheckImportable(imp))
                    {
                        // dll のインポートに失敗

                        return false;
                    }
                    ApiImporter.ImportDll(imp, null);
                }
                _inportDllModule = null;
            }
            return true;
        }

        public List<string> ScriptControlRecent
        {
            get
            {
                var recentNode = CommandMenuWindow.treeViewCommand.GetRecent();
                if (recentNode.Child.Count != 0)
                {
                    // 最近使ったスクリプトノードを記録する

                    var Recent = new List<string>();
                    foreach (var node in recentNode.Child)
                    {
                        Recent.Add(node.Name);
                    }
                    return Recent;
                }
                return null;
            }
            set
            {
                if (value != null)
                {
                    // 最近使ったスクリプトノードを復元する

                    var recentNode = CommandMenu.GetRecent();
                    foreach (var node in value)
                    {
                        recentNode.AddChild(new TreeMenuNode(node, CreateImmediateExecutionCanvasCommand(() =>
                        {
                            CommandMenu.ExecuteFindCommand(node);
                        })));
                    }
                }
            }
        }

        public void HideWorkStack()
        {
            WorkStack.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// キャンバスの作業をxml化して表示します。
        /// </summary>
        private void OutputControlXML()
        {
            var outputWindow = new OutputWindow();
            outputWindow.Title = "Contorl List <Output[" + ScriptWorkCanvas.Name + "]>";
            outputWindow.Owner = CommandCanvasList.OwnerWindow;
            outputWindow.Show();

            var writer = new StringWriter();
            var serializer = new XmlSerializer(ScriptCommandCanvas.AssetXML.GetType());
            var namespaces = new XmlSerializerNamespaces();
            namespaces.Add(string.Empty, string.Empty);
            ScriptCommandCanvas.AssetXML.WriteAction();
            serializer.Serialize(writer, ScriptCommandCanvas.AssetXML, namespaces);
            outputWindow.AddBindText = writer.ToString();
        }

        /// <summary>
        /// キャンバスの作業を上書き保存します。
        /// </summary>
        public void OverwriteSaveXML()
        {
            if (CommandCanvasList.GetOwnerCursor() == Cursors.Wait)
                return;

            if (OpenFileName == "")
            {
                SaveXML();
                return;
            }

            SaveXml(OpenFileName);
        }

        private string openFileName = "";

        /// <summary>
        /// 開いているファイルを参照します。
        /// </summary>
        public string OpenFileName
        {
            get => openFileName;
            set
            {
                openFileName = value;
                SetupTitle();
            }
        }

        /// <summary>
        /// タイトルのセットを依頼します。
        /// </summary>
        public void SetupTitle()
        {
            CommandCanvasControl.RequestSetTitle(System.IO.Path.GetFileName(openFileName));
        }

        /// <summary>
        /// キャンバスの作業を保存します。
        /// </summary>
        public void SaveXML()
        {
            if (CommandCanvasList.GetOwnerCursor() == Cursors.Wait)
                return;

            string path = ShowSaveDialog();

            if (path is null)
                return;

            SaveXml(path);
        }

        /// <summary>
        /// キャンバスの作業を保存します。
        /// </summary>
        /// <param name="path">ファイルのパス</param>
        private void SaveXml(string path)
        {
            if (path is null)
                return;

            if (CommandCanvasList.GetOwnerCursor() == Cursors.Wait)
                return;

            CommandCanvasList.SetOwnerCursor(Cursors.Wait);

            try
            {
                var writer = new StringWriter();
                var serializer = new XmlSerializer(ScriptCommandCanvas.AssetXML.GetType());
                var namespaces = new XmlSerializerNamespaces();
                namespaces.Add(string.Empty, string.Empty);
                ScriptCommandCanvas.AssetXML.WriteAction();
                serializer.Serialize(writer, ScriptCommandCanvas.AssetXML, namespaces);
                using (StreamWriter swriter = new StreamWriter(path, false))
                {
                    swriter.WriteLine(writer.ToString());
                }
                OpenFileName = path;

                string script = CommandCanvasList.Instance.BuildScript();
                if (!string.IsNullOrWhiteSpace(script))
                {
                    File.WriteAllText(System.IO.Path.ChangeExtension(OpenFileName, ".css"), script);
                }

                Console.WriteLine($"Save...\"{path}.xml\"");
            }
            catch (Exception ex)
            {
                ControlTools.ShowErrorMessage(ex.Message);
            }

            CommandCanvasList.SetOwnerCursor(null);
        }

        /// <summary>
        /// キャンバスの作業を読み込みます。
        /// </summary>
        public void LoadXML()
        {
            if (CommandCanvasList.GetOwnerCursor() == Cursors.Wait)
                return;

            string path = ShowLoadDialog();
            if (path is null)
                return;

            LoadXML(path);
        }

        /// <summary>
        /// キャンバスの作業を読み込みます。
        /// </summary>
        /// <param name="path">ファイルのパス</param>
        public void LoadXML(string path)
        {
            if (CommandCanvasList.GetOwnerCursor() == Cursors.Wait)
                return;

            OpenFileName = path;
            CommandCanvasList.SetOwnerCursor(Cursors.Wait);
            ScriptWorkCanvas.Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    string text = File.ReadAllText(path);
                    if (!text.Contains("<CommandCanvas Id=") || !text.Contains("<DataVersion>"))
                    {
                        CommandCanvasList.SetOwnerCursor(null);
                        ControlTools.ShowErrorMessage(CapyCSS.Language.Instance["SYSTEM_Error_DataFormat"]);
                        return;
                    }
                }
                catch (Exception ex)
                {
                    ControlTools.ShowErrorMessage(ex.Message);
                }

                using (StreamReader reader = new StreamReader(path))
                {
                    try
                    {
                        File.ReadAllText(path).Contains("<CommandCanvas Id=");

                        XmlSerializer serializer = new XmlSerializer(ScriptCommandCanvas.AssetXML.GetType());

                        XmlDocument doc = new XmlDocument();
                        doc.PreserveWhitespace = true;
                        doc.Load(reader);
                        XmlNodeReader nodeReader = new XmlNodeReader(doc.DocumentElement);

                        object data = (CommandCanvas._AssetXML<CommandCanvas>)serializer.Deserialize(nodeReader);
                        ScriptCommandCanvas.AssetXML = (CommandCanvas._AssetXML<CommandCanvas>)data;
                    }
                    catch (Exception ex)
                    {
                        ControlTools.ShowErrorMessage(ex.Message);
                    }
                }
                ClearWorkCanvas(false);
                GC.Collect();
                CommandCanvasControl.MainLog.TryAutoClear();

                PointIdProvider.InitCheckRequest();
                ScriptCommandCanvas.AssetXML.ReadAction(ScriptCommandCanvas);   // 優先順位 Background まで使われる 

                ScriptCommandCanvas.Dispatcher.BeginInvoke(new Action(() =>
                {
                    // アイドル状態になってから戻す

                    GC.Collect();
                    CommandCanvasList.SetOwnerCursor(null);
                    if (CommandCanvasControl.IsAutoExecute)
                    {
                        // 起動時自動実行

                        CommandCanvasControl.CallPublicExecuteEntryPoint(false);
                        CommandCanvasControl.IsAutoExecute = false;
                    }
                    else
                    {
                        CommandCanvasControl.IsAutoExit = false;
                    }
                }), DispatcherPriority.SystemIdle); // ApplicationIdle でも良いと思うけども…
            }), DispatcherPriority.ApplicationIdle);
        }

        /// <summary>
        /// キャンバスの作業をクリアします。
        /// </summary>
        public void ClearWorkCanvas(bool full = true)
        {
            CommandCanvasControl.ClearPublicExecuteEntryPoint(this);
            ScriptWorkCanvas.Clear();
            ScriptWorkStack.Clear();
            WorkStack.Clear();
            UIParamHoldAction.Clear();
            StackGroupHoldAction.Clear();
            PlotWindowHoldAction.Clear();
            LinkConnectorListHoldAction.Clear();
            ScriptCommandCanvas.HideWorkStack();
            InstalledMultiRootConnector = null;

            if (full)
            {
                OpenFileName = "";
            }
            OutDebugCreateList("leak clear");
        }

#region デバッグ用クリア管理
        //※参照カウンターは勘案しない

        public static Dictionary<string, int> DebugCreateNames = new Dictionary<string, int>();

        [Conditional("DEBUG")]
        public void OutDebugCreateList(string title)
        {
            bool isFirst = true;
            foreach (var node in DebugCreateNames)
            {
                if (node.Value == 0)
                    continue;
                if (isFirst)
                {
                    Console.WriteLine($"---------------------- {title}");
                    isFirst = false;
                }
                Console.WriteLine($"{node.Key} : {node.Value}");
            }
        }

        [Conditional("DEBUG")]
        public static void DebugMakeCreateTitle(ref string title, object self, object coller, string methodName, string ext)
        {
            if (methodName == "")
            {
                methodName = "(node)";
            }
            if (coller is null)
            {
                title = $"{self.GetType().Name} => {methodName}";
            }
            else if (coller is string str)
            {
                title = $"{self.GetType().Name} => {str}::{methodName}";
            }
            else
            {
                string collerName;
                if (coller.GetType().IsGenericType)
                {
                    collerName = CbSTUtils.GetGenericTypeName(coller.GetType());
                }
                else
                {
                    collerName = coller.GetType().Name;
                }
                title = $"{self.GetType().Name} => {collerName}::{methodName}";
            }
            if (ext != "")
            {
                title += $"[{ext}]";
            }
        }

        [Conditional("DEBUG")]
        public static void SetDebugCreateList(ref string title, object self, object coller = null, string methodName = "", string ext = "")
        {
            DebugMakeCreateTitle(ref title, self, coller, methodName, ext);
            if (!CommandCanvas.DebugCreateNames.ContainsKey(title))
            {
                CommandCanvas.DebugCreateNames.Add(title, 1);
            }
            else
            {
                CommandCanvas.DebugCreateNames[title]++;
            }
        }

        [Conditional("DEBUG")]
        public static void RemoveDebugCreateList(string name)
        {
            if (!CommandCanvas.DebugCreateNames.ContainsKey(name))
            {
                Debug.Assert(false);
            }
            else
            {
                CommandCanvas.DebugCreateNames[name]--;
            }
        }
#endregion

        /// <summary>
        /// 保存用ファイル選択ダイアログを表示します。
        /// </summary>
        /// <returns></returns>
        public string ShowSaveDialog()
        {
            var dialog = new SaveFileDialog();

            // ディレクトリを設定
            dialog.InitialDirectory = System.IO.Path.GetDirectoryName(OpenFileName);

            // ファイル名を設定
            dialog.FileName = System.IO.Path.GetFileNameWithoutExtension(OpenFileName);

            // ファイルの種類を設定
            dialog.Filter = "CBS files (*.cbs)|*.cbs|all (*.*)|*.*";

            // ダイアログを表示する
            if (dialog.ShowDialog() == true)
            {
                return dialog.FileName;
            }
            return null;
        }

        /// <summary>
        /// 読み込み用ファイル選択ダイアログを表示します。
        /// </summary>
        /// <returns></returns>
        public string ShowLoadDialog()
        {
            var dialog = new OpenFileDialog();

            // ファイルの種類を設定
            dialog.Filter = "CBS files (*.cbs, *.xml)|*.cbs;*.xml|all (*.*)|*.*";

            if (!initFlg)
            {
                // 初期ディレクトリをサンプルのあるディレクトリにする
                
                var sampleDir = GetSamplePath();
                if (sampleDir != null)
                {
                    dialog.InitialDirectory = GetSamplePath();
                }
                initFlg = true;
            }

            // ダイアログを表示する
            if (dialog.ShowDialog() == true)
            {
                return dialog.FileName;
            }
            return null;
        }
        static bool initFlg = false;

        /// <summary>
        /// sample ディレクトリのフルパスを取得します。
        /// </summary>
        /// <returns>sampleディレクトリのフルパス</returns>
        public static string GetSamplePath()
        {
            string exexPath = System.Environment.CommandLine;
            if (exexPath == null)
            {
                return Environment.CurrentDirectory;
            }
            System.IO.FileInfo fi = new System.IO.FileInfo(exexPath.Replace("\"", ""));
            string startupPath = fi.Directory.FullName;
            var resultPath = System.IO.Path.Combine(startupPath, "Sample");
            if (!Directory.Exists(resultPath))
            {
                return Environment.CurrentDirectory;
            }
            return System.IO.Path.Combine(startupPath, "Sample");
        }
#endregion

#region タイプリストを実装
        private TreeMenuNode typeWindow_classMenu = null;
        private TreeMenuNode typeWindow_enumMenu = null;
        private TreeMenuNode typeWindow_structMenu = null;
        private TreeMenuNode typeWindow_interfaceMenu = null;
        private TreeMenuNode typeWindow_import = null;

        /// <summary>
        /// 組み込み型の型情報を型メニューにセットします。
        /// </summary>
        /// <param name="treeViewCommand">登録先</param>
        private void MakeTypeMenu(TreeViewCommand treeViewCommand)
        {
            // コマンドを追加
            {
                var builtInGroup = new TreeMenuNode("Built-in type");
                foreach (var typeName in CbSTUtils.BuiltInTypeList)
                {
                    TreeViewCommand.AddGroupedMenu(
                        builtInGroup,
                        typeName.Value,
                        null,
                        (p) =>
                        {
                            SelectType = typeName.Key;
                            TypeMenuWindow.Close();
                        },
                        (p) =>
                        {
                            return CanTypeMenuExecuteEvent(CbST.GetTypeEx(typeName.Key));
                        }
                        );

                }
                treeViewCommand.AssetTreeData.Add(builtInGroup);
                treeViewCommand.AssetTreeData.Add(typeWindow_classMenu = new TreeMenuNode(CbSTUtils.CLASS_STR));
                treeViewCommand.AssetTreeData.Add(typeWindow_interfaceMenu = new TreeMenuNode(CbSTUtils.INTERFACE_STR));
                treeViewCommand.AssetTreeData.Add(typeWindow_structMenu = new TreeMenuNode(CbSTUtils.STRUCT_STR));
                treeViewCommand.AssetTreeData.Add(typeWindow_enumMenu = new TreeMenuNode(CbSTUtils.ENUM_STR));
                treeViewCommand.AssetTreeData.Add(typeWindow_import = new TreeMenuNode(ApiImporter.MENU_TITLE_IMPORT));
            }
        }

        /// <summary>
        /// 型情報を型メニューに取り込みます。
        /// </summary>
        /// <param name="type">型情報</param>
        public void AddTypeMenu(Type type)
        {
            TreeMenuNode targetNode = null;

            if (type.IsEnum)
            {
                targetNode = typeWindow_enumMenu;
            }
            else if (type.IsInterface)
            {
                targetNode = typeWindow_interfaceMenu;
            }
            else if (CbStruct.IsStruct(type))
            {
                targetNode = typeWindow_structMenu;
            }
            else if (type.IsClass)
            {
                targetNode = typeWindow_classMenu;
            }
            if (targetNode is null)
            {
                return;
            }
            TreeViewCommand.AddGroupedMenu(
                targetNode,
                CbSTUtils.MakeGroupedTypeName(type),
                null,
                (p) =>
                {
                    CommandCanvas.SelectType = type.FullName;
                    TypeMenuWindow.Close();
                },
                (p) =>
                {
                    return CanTypeMenuExecuteEvent(type);
                }
                );
        }

        private string backupImportingName = "";
        private TreeMenuNode importingMenu = null;

        /// <summary>
        /// 型情報を型メニューインポートします。
        /// </summary>
        /// <param name="type">型情報</param>
        public void AddImportTypeMenu(Type type)
        {
            string group = CbSTUtils.GetTypeGroupName(type);
            if (group is null)
            {
                return;
            }
            if (ScriptImplement.ImportingName != backupImportingName)
            {
                backupImportingName = ScriptImplement.ImportingName;
                importingMenu = ImplementAsset.CreateGroup(typeWindow_import, backupImportingName);
            }
            Debug.Assert(importingMenu != null);

            TreeViewCommand.AddGroupedMenu(
                importingMenu,
                group + "." + CbSTUtils.MakeGroupedTypeName(type),
                null,
                (p) =>
                {
                    CommandCanvas.SelectType = type.FullName;
                    TypeMenuWindow.Close();
                },
                (p) =>
                {
                    return CanTypeMenuExecuteEvent(type);
                }
                );
        }

        public struct TypeRequest
        {
            public string Name;
            public Type InitType;
            public Func<Type, bool> IsAccept;
            public TypeRequest(Func<Type, bool> isAccept, string name = "")
            {
                Name = name;
                InitType = null;
                IsAccept = isAccept;
            }
            public TypeRequest(Type initTypeName, Func<Type, bool> isAccept, string name = "")
            {
                Name = name;
                InitType = initTypeName;
                IsAccept = isAccept;
            }
            public TypeRequest(Type initTypeName, string name = "")
            {
                Name = name;
                InitType = initTypeName;
                IsAccept = null;
            }
        }

        /// <summary>
        /// ユーザーに複数の型の指定を要求します。
        /// </summary>
        /// <param name="typeRequests">型の要求リスト</param>
        /// <param name="title">タイトル</param>
        /// <returns>型名リスト</returns>
        public List<string> RequestTypeName(in IList<TypeRequest> typeRequests, string title = "")
        {
            var result = new List<string>();
            bool positionSet = true;
            foreach (var typeRequest in typeRequests)
            {
                string typeName;
                if (typeRequest.InitType is null)
                {
                    // フィルタリングされた任意の型

                    typeName = RequestTypeName(typeRequest.IsAccept, title, positionSet);
                }
                else if (typeRequest.InitType.IsGenericType && typeRequest.InitType.GenericTypeArguments.Length == 0)
                {
                    // ジェネリックでかつ型が完成していない

                    typeName = RequestGenericTypeName(typeRequest.InitType.FullName, typeRequest.IsAccept, title, positionSet);
                }
                else if (typeRequest.InitType == CbSTUtils.ARRAY_TYPE)
                {
                    // 配列型

                    typeName = RequestTypeName(typeRequest.IsAccept, title, positionSet);
                    typeName += "[]";
                }
                else
                {
                    // 指定された型

                    typeName = typeRequest.InitType.FullName;
                }
                if (typeName is null)
                {
                    return null;
                }
                result.Add(typeName);
                positionSet = false;
            }
            return result;
        }

        /// <summary>
        /// ユーザーに型の指定を要求します。
        /// </summary>
        /// <returns>型名</returns>
        public string RequestTypeName(Func<Type, bool> isAccept, string title = "", bool positionSet = true)
        {
            TypeMenuWindow.Message = title;
            _CanTypeMenuExecuteEvent = new Func<Type, bool>[] { isAccept };
            _CanTypeMenuExecuteEventIndex = 0;
            TypeMenu.RefreshItem();
            string ret = null;
            try
            {
                Type type = RequestType(true, isAccept, positionSet);
                if (type is null)
                {
                    return null;
                }
                ret = type.FullName;
            }
            catch (Exception ex)
            {
                Console.WriteLine(nameof(CommandCanvas) + ":" + ex.Message);
            }
            return ret;
        }

        /// <summary>
        /// ユーザーにジェネリック型の引数の型の指定を要求します。
        /// </summary>
        /// <param name="genericTypeName">ジェネリック型の型名</param>
        /// <returns>型名（ジェネリック型でない場合はそのままの型名）</returns>
        public string RequestGenericTypeName(string genericTypeName, Func<Type, bool> isAccept, string title = "", bool positionSet = true)
        {
            _CanTypeMenuExecuteEvent = new Func<Type, bool>[] { isAccept };
            _CanTypeMenuExecuteEventIndex = 0;
            TypeMenu.RefreshItem();
            string ret = null;
            try
            {
                ret = RequestGenericTypeName(CbST.GetTypeEx(genericTypeName), isAccept, title, positionSet);
            }
            catch (Exception ex)
            {
                Console.WriteLine(nameof(CommandCanvas) + ":" + ex.Message);
            }
            return ret;
        }

        /// <summary>
        /// メニューの有効無効判定イベントを登録します。
        /// </summary>
        private Func<Type, bool>[] _CanTypeMenuExecuteEvent = null;
        private int _CanTypeMenuExecuteEventIndex = 0;
        private Func<Type, bool> GetCanTypeMenuExecuteEvent()
        {
            if (_CanTypeMenuExecuteEventIndex >= _CanTypeMenuExecuteEvent.Length || _CanTypeMenuExecuteEventIndex < 0)
            {
                return (n) => true;
            }
            return _CanTypeMenuExecuteEvent[_CanTypeMenuExecuteEventIndex];
        }

        /// <summary>
        /// メニューの有効無効判定イベントを呼び出します。
        /// </summary>
        private bool CanTypeMenuExecuteEvent(Type type)
        {
            if (_CanTypeMenuExecuteEvent is null)
                return true;
            return GetCanTypeMenuExecuteEvent()(type);
        }

        /// <summary>
        /// ユーザーに型の指定を要求します。
        /// </summary>
        /// <returns>型情報</returns>
        private Type RequestType(bool checkType, Func<Type, bool> isAccept, bool positionSet = true)
        {
            SelectType = null;
            if (positionSet && _CanTypeMenuExecuteEventIndex == 0)
            {
                ControlTools.SetWindowPos(TypeMenuWindow, new Point(Mouse.GetPosition(this).X, Mouse.GetPosition(this).Y));
            }
            TypeMenuWindow.ShowDialog();
            if (SelectType is null)
            {
                return null;
            }
            if (checkType)
            {
                _CanTypeMenuExecuteEventIndex++;
                TypeMenu.RefreshItem();
            }
            else
            {
                var backup = _CanTypeMenuExecuteEventIndex;
                _CanTypeMenuExecuteEventIndex = -1;
                TypeMenu.RefreshItem();
                _CanTypeMenuExecuteEventIndex = backup;
            }

            Type type = CbST.GetTypeEx(SelectType);
            if (type is null)
            {
                Console.WriteLine(nameof(CommandCanvas) + $": {SelectType} was an unsupportable type.");
                return null;
            }

            string name = type.Name.Split('`')[0];
            if (CbSTUtils.CbTypeNameList.ContainsKey(name))
            {
                TypeMenuWindow.Message += CbSTUtils.CbTypeNameList[name];
            }
            else
            {
                TypeMenuWindow.Message += name;
            }
            return RequestGenericType(type, isAccept, false);
        }

        /// <summary>
        /// ユーザーにジェネリック型の引数の型の指定を要求します。
        /// </summary>
        /// <param name="genericType">ジェネリック型の型情報</param>
        /// <returns>型名（ジェネリック型でない場合はそのままの型名）</returns>
        private string RequestGenericTypeName(Type genericType, Func<Type, bool> isAccept, string title = "", bool positionSet = true)
        {
            if (genericType is null)
            {
                return null;
            }

            var token = genericType.Name.Split('`');
            string name = token[0];

            string param = "<";
            int argCount = int.Parse(token[1]);
            if (argCount - 1 > 0)
            {
                param += new string(',', argCount - 1);
            }
            param += '>';

            if (CbSTUtils.CbTypeNameList.ContainsKey(name))
            {
                name = CbSTUtils.CbTypeNameList[name];
            }

            TypeMenuWindow.Message = $"{title}{name}{param} : {name}";

            Type type = RequestGenericType(genericType, isAccept, true, positionSet);
            if (type is null)
            {
                return null;
            }
            return type.FullName;
        }

        /// <summary>
        /// ユーザーにジェネリック型の引数の型の指定を要求します。
        /// </summary>
        /// <param name="genericType">ジェネリック型の型情報</param>
        /// <returns>型情報（ジェネリック型でない場合はそのままの型情報）</returns>
        private Type RequestGenericType(Type genericType, Func<Type, bool> isAccept, bool checkType, bool positionSet = true)
        {
            if (genericType is null)
            {
                return null;
            }
            if (genericType == CbSTUtils.ARRAY_TYPE)
            {
                bool andPositionSet = _CanTypeMenuExecuteEventIndex == 0;
                if (_CanTypeMenuExecuteEventIndex != 0)
                {
                    _CanTypeMenuExecuteEventIndex--;
                }
                _CanTypeMenuExecuteEvent[_CanTypeMenuExecuteEventIndex] = t => CbScript.AcceptAll(t);
                TypeMenu.RefreshItem();
                Type result = RequestType(checkType, isAccept, positionSet && andPositionSet);
                if (result is null)
                    return null;
                return result.MakeArrayType();
            }
            if (genericType.IsGenericType)
            {
                TypeMenuWindow.Message += "<";

                var args = new List<Type>();

                string cmc = genericType.FullName.Split('`')[1];
                if (cmc.Contains('['))
                {
                    cmc = cmc.Split('[')[0];
                }
                int argCount = Int32.Parse(cmc);
                for (int i = 0; i < argCount; ++i)
                {
                    var argType = genericType.GetGenericArguments()[i];

                    // ジェネリック用パラメータ型制限用フィルターに更新する
                    if (_CanTypeMenuExecuteEventIndex != 0)
                    {
                        _CanTypeMenuExecuteEventIndex--;
                    }
                    Func<Type, bool> _isAccept = isAccept != null ? isAccept : (t) => true;
                    _CanTypeMenuExecuteEvent[_CanTypeMenuExecuteEventIndex] = (t) => _isAccept(t) && ScriptImplement.MakeParameterConstraintAccepter(argType)(t);
                    TypeMenu.RefreshItem();

                    Type arg = RequestType(checkType, isAccept, positionSet);

                    if (arg is null)
                    {
                        return null;
                    }
                    args.Add(arg);

                    if (i < argCount - 1)
                    {
                        TypeMenuWindow.Message += ", ";
                    }
                }

                TypeMenuWindow.Message += ">";
                return genericType.MakeGenericType(args.ToArray());
            }
            return genericType;
        }

        /// <summary>
        /// インポートされている型情報を削除します。
        /// </summary>
        public void ClearTypeImportMenu(string name)
        {
            if (typeWindow_import is null)
            {
                return;
            }

            TreeMenuNode targetNode = null;
            foreach (var node in typeWindow_import.Child)
            {
                if (node.Name == name)
                {
                    targetNode = node;
                    break;
                }
            }
            if (targetNode != null)
            {
                typeWindow_import.Child.Remove(targetNode);
            }
        }
#endregion

        public void ToggleGridLine()
        {
            ScriptWorkCanvas.EnabelGridLine = ScriptWorkCanvas.EnabelGridLine ? false : true;
        }

        private void Grid_KeyDown(object sender, KeyEventArgs e)
        {
            bool isCtrlButton = (Keyboard.GetKeyStates(Key.LeftCtrl) & KeyStates.Down) > 0 ||
                   (Keyboard.GetKeyStates(Key.RightCtrl) & KeyStates.Down) > 0;
            bool isShiftButton = (Keyboard.GetKeyStates(Key.LeftShift) & KeyStates.Down) > 0 ||
                        (Keyboard.GetKeyStates(Key.RightShift) & KeyStates.Down) > 0;

            if (isCtrlButton && isShiftButton)
            {
                // Ctrl + Shift + key

                switch (e.Key)
                {
                    // CommandCanvasList で使用
                    case Key.N: // 全クリア
                        ClearWorkCanvasWithConfirmation();
                        break;
                }
            }
            else if (isCtrlButton)
            {
                // Ctrl + key

                switch (e.Key)
                {
                    // CommandCanvasList で使用
                    case Key.S:
                    case Key.O:
                    case Key.N:
                        break;

                    case Key.G:
                        ToggleGridLine();
                        e.Handled = true;
                        break;

                        // BaseWorkCanvas で使用
                    case Key.C:
                    case Key.V:
                        break;
                }
            }
            else if (isShiftButton)
            {
                // Shift + key
            }
            else
            {
                switch (e.Key)
                {
                    // CommandCanvasList で使用
                    case Key.F5:
                        break;

                    // BaseWorkCanvas で使用
                    case Key.Delete:
                        break;
                }
            }
        }

        private void ClearWorkCanvasWithConfirmation()
        {
            if (ControlTools.ShowSelectMessage(
                        CapyCSS.Language.Instance["SYSTEM_ConfirmationDelete"],
                        CapyCSS.Language.Instance["SYSTEM_Confirmation"],
                        MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {
                CommandCanvasList.SetOwnerCursor(Cursors.Wait);
                ScriptWorkCanvas.Dispatcher.BeginInvoke(new Action(() =>
                {
                    ApiImporter.ClearModule(ApiImporter.GetBaseImportList());
                    ClearWorkCanvas();
                    ScriptCommandCanvas.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        // アイドル状態になってから戻す

                        GC.Collect();
                        CommandCanvasList.SetOwnerCursor(null);

                    }), DispatcherPriority.ApplicationIdle);
                }), DispatcherPriority.ApplicationIdle);
            }
        }

#region ROOT_VALUE_TYPE
        private Type rootConnectorValueType = null;
        /// <summary>
        /// 接続操作されている RootConnector の型を参照します。
        /// </summary>
        public Type RootConnectorValueType
        {
            get => rootConnectorValueType;
            set
            {
                rootConnectorValueType = value;
            }
        }

        /// <summary>
        /// 接続操作されている RootConnector の型との一致を判定します。
        /// </summary>
        /// <param name="typeName"></param>
        /// <returns></returns>
        public bool IsEqualRootConnectorValueType(string typeName)
        {
            if (rootConnectorValueType is null)
                return false;
            bool ret = (rootConnectorValueType.Namespace + "." + rootConnectorValueType.Name) == typeName;
            if (!ret)
            {
                RootConnectorValueType = null;
            }
            return ret;
        }

        /// <summary>
        /// 接続操作されている RootConnector の型リクエスト情報をコピーします。
        /// </summary>
        /// <param name="col"></param>
        /// <returns></returns>
        public string[] SetRootConnectorValueType(MultiRootConnector col)
        {
            List<string> typeNames = new List<string>();
            for (int i = 0; i < rootConnectorValueType.GetGenericArguments().Length; ++i)
            {
                var type = rootConnectorValueType.GetGenericArguments()[i];
                col.SelectedVariableType[i] = type;
                typeNames.Add(type.FullName);
            }
            RootConnectorValueType = null;
            return typeNames.ToArray();
        }

        /// <summary>
        /// 最後に WorkCanvas に置かれた MultiRootConnector を参照します。
        /// </summary>
        public MultiRootConnector installedMultiRootConnector = null;
        public MultiRootConnector InstalledMultiRootConnector
        {
            get
            {
                if (installedMultiRootConnector != null && !ScriptWorkCanvas.Contains(installedMultiRootConnector))
                {
                    installedMultiRootConnector = null;
                }
                return installedMultiRootConnector;
            }
            set
            {
                installedMultiRootConnector = value;
            }
        }

        /// <summary>
        /// 最後に WorkCanvas に置かれた MultiRootConnector の持つ引数 LinkConnector の内から型の一致するものを返します。
        /// </summary>
        /// <param name="type">要求する型</param>
        /// <returns>一致する最初に見つかった LinkConnector</returns>
        public LinkConnector GetLinkConnectorFromInstalledMultiRootConnector(Type type)
        {
            if (InstalledMultiRootConnector is null ||
                InstalledMultiRootConnector.LinkConnectorControl is null)
                return null;
            return InstalledMultiRootConnector.LinkConnectorControl.GetLinkConnector(type);
        }
#endregion

        /// <summary>
        /// WorkCanvas に登録されたコマンドを実行します。
        /// </summary>
        /// <param name="setPos">場所</param>
        public void ProcessCommand(Point setPos)
        {
            ScriptWorkCanvas.ProcessCommand(setPos);
        }

        /// <summary>
        /// コマンドメニューを表示します。
        /// </summary>
        /// <param name="pos">表示位置</param>
        public void ShowCommandMenu(Point? pos = null, string filterString = null)
        {
            CommandMenuWindow.SetPos(pos);
            CommandMenuWindow.FilterString = filterString;
            CommandMenuWindow.ShowDialog();
        }

#region IDisposable Support
        private bool disposedValue = false; // 重複する呼び出しを検出するには

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    CommandMenuWindow.Dispose();
                    CommandMenuWindow = null;
                    TypeMenuWindow.Dispose();
                    TypeMenuWindow = null;

                    _inportNameSpaceModule = null;
                    _inportDllModule = null;
                    _inportNuGetModule = null;

                    ApiImporter = null;
                    moduleControler = null;
                    CommandCanvasControl = null;
                    ScriptCommandCanvas = null;
                    ScriptWorkCanvas?.Dispose();
                    ScriptWorkCanvas = null;
                    ScriptWorkStack?.Dispose();
                    ScriptWorkStack = null;
                    ScriptWorkClickEvent = null;
                    ClickEntryEvent = null;
                    ClickExitEvent = null;

                    UIParamHoldAction?.Dispose();
                    StackGroupHoldAction?.Dispose();
                    PlotWindowHoldAction?.Dispose();
                    LinkConnectorListHoldAction?.Dispose();

                    UIParamHoldAction = null;
                    StackGroupHoldAction = null;
                    PlotWindowHoldAction = null;
                    LinkConnectorListHoldAction = null;

                    typeWindow_classMenu = null;
                    typeWindow_enumMenu = null;
                    typeWindow_structMenu = null;
                    typeWindow_interfaceMenu = null;
                    typeWindow_import = null;
                    InstalledMultiRootConnector = null;

                    WorkStack.Dispose();

                    AssetXML?.Dispose();
                    AssetXML = null;
                }
                disposedValue = true;
            }
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
#endregion

    }
}
