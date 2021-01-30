using CapybaraVS.Control.BaseControls;
using CapybaraVS.Script;
using CapyCSS.Controls;
using CapyCSS.Controls.BaseControls;
using CbVS;
using MathNet.Numerics.RootFinding;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
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

namespace CapybaraVS.Controls.BaseControls
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
        public class _AssetXML<OwnerClass>
            where OwnerClass : CommandCanvas
        {
            [XmlIgnore]
            public Action WriteAction = null;
            [XmlIgnore]
            public Action<OwnerClass> ReadAction = null;
            public _AssetXML()
            {
                ReadAction = (self) =>
                {
                    self.AssetId = AssetId;
                    self._inportModule = ImportModule;
                    self._inportDllModule = ImportDllModule;
                    self.ImportModule();
                    SetupCanvas(self);

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
                    // モジュールのインポートが終わるのを待つ

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
                            Movable movableNode = new Movable();
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
                    ImportModule = self.ApiImporter.ModuleList;
                    ImportDllModule = self.ApiImporter.ModulePathList;
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
            public BaseWorkCanvas._AssetXML<BaseWorkCanvas> WorkCanvas { get; set; } = null;
            public List<string> ImportModule { get; set; } = null;
            public List<string> ImportDllModule { get; set; } = null;
            public Stack._AssetXML<Stack> WorkStack { get; set; } = null;
            [XmlArrayItem("Asset")]
            public List<Movable._AssetXML<Movable>> WorkCanvasAssetList { get; set; } = null;
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

            CommandMenuWindow = CommandWindow.Create();
            CommandMenuWindow.treeViewCommand.OwnerCommandCanvas = this;
            CommandMenuWindow.treeViewCommand.AssetTreeData = new ObservableCollection<TreeMenuNode>();
            AddTreeCommandAsset(CommandMenuWindow.treeViewCommand);

            ClickEntryEvent = new Action(() =>
            {
                ScriptWorkCanvas.Cursor = null;
                CommandMenu.Cursor = null;
            });

            ClickExitEvent = new Action(() =>
            {
                CommandMenuWindow.CloseWindow();
                ScriptWorkCanvas.Cursor = Cursors.Hand;
                CommandMenu.Cursor = Cursors.Hand;
            });
        }

        ~CommandCanvas()
        {
            Dispose();
        }

        //----------------------------------------------------------------------
        #region スクリプト内共有
        public List<string> _inportModule = null;
        public List<string> _inportDllModule = null;
        public ApiImporter ApiImporter = null;
        public CommandWindow CommandMenuWindow = null;
        public CommandCanvasList CommandCanvasControl = null;
        public TreeViewCommand CommandMenu => CommandMenuWindow.treeViewCommand;
        public CommandCanvas ScriptCommandCanvas = null;
        public BaseWorkCanvas ScriptWorkCanvas = null;
        public Stack ScriptWorkStack = null;
        public Func<object> ScriptWorkClickEvent = null;
        public Action ClickEntryEvent = null;
        public Action ClickExitEvent = null;
        public HoldActionManager<UIParam> UIParamHoldAction = new HoldActionManager<UIParam>();
        public HoldActionManager<StackGroup> StackGroupHoldAction = new HoldActionManager<StackGroup>();
        public HoldActionManager<PlotWindow> PlotWindowHoldAction = new HoldActionManager<PlotWindow>();
        public HoldActionManager<LinkConnectorList> LinkConnectorListHoldAction = new HoldActionManager<LinkConnectorList>();
        public bool EnabledScriptHoldActionMode
        {
            set
            {
                UIParamHoldAction.Enabled = value;
                StackGroupHoldAction.Enabled = value;
                PlotWindowHoldAction.Enabled = value;
                LinkConnectorListHoldAction.Enabled = value;
            }
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
        public TreeMenuNodeCommand CreateEventCanvasCommand(string path, Func<object> action, TreeMenuNode vm = null)
        {
            return new TreeMenuNodeCommand(vm, (a) =>
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
        public TreeMenuNodeCommand CreateImmediateExecutionCanvasCommand(Action action, TreeMenuNode vm = null)
        {
            return new TreeMenuNodeCommand(vm, (a) =>
                {
                    if (CommandCanvasList.OwnerWindow.Cursor == Cursors.Wait)
                        return; // 処理中は禁止

                    CommandMenuWindow.CloseWindow();

                    action?.Invoke();
                }
            );
        }

        #endregion

        //----------------------------------------------------------------------
        #region アセットリストを実装

        private void AddTreeCommandAsset(TreeViewCommand treeViewCommand)
        {
            // コマンドを追加
            {
                var commandNode = new TreeMenuNode("Command");
                commandNode.AddChild(new TreeMenuNode("Clear(Ctrl+Shift+N)", CreateImmediateExecutionCanvasCommand(() => ClearWorkCanvasWithConfirmation())));
                commandNode.AddChild(new TreeMenuNode("Toggle ShowMouseInfo", CreateImmediateExecutionCanvasCommand(() => ScriptWorkCanvas.EnableInfo = ScriptWorkCanvas.EnableInfo ? false : true)));
                commandNode.AddChild(new TreeMenuNode("Toggle ShowGridLine(Ctrl+G)", CreateImmediateExecutionCanvasCommand(() => ScriptCommandCanvas.ToggleGridLine())));
                commandNode.AddChild(new TreeMenuNode("Save(Ctrl+S)", CreateImmediateExecutionCanvasCommand(() => CommandCanvasControl.SaveCbsFile())));
                commandNode.AddChild(new TreeMenuNode("Load(Ctrl+O)", CreateImmediateExecutionCanvasCommand(() => CommandCanvasControl.LoadCbsFile())));
                treeViewCommand.AssetTreeData.Add(commandNode);
            }

            // 飾りを追加
            {
                var decorationNode = new TreeMenuNode("Decoration");
                decorationNode.AddChild(new TreeMenuNode("Text Area", CreateEventCanvasCommand(decorationNode.Name + ".Text Area", () => new GroupArea() { Name = "test", Width = 150, Height = 150 })));
                treeViewCommand.AssetTreeData.Add(decorationNode);
            }

            // 基本的なアセットを追加
            ApiImporter = new ApiImporter(this);
            moduleView.Content = new ModuleControler(ApiImporter);
            ImportModule();

            // デバッグ用アセットを追加
            AddTestTreeAsset(treeViewCommand);
        }

        /// <summary>
        /// モジュールを読み込みます。
        /// </summary>
        public void ImportModule()
        {
            ApiImporter.ClearModule();
            if (_inportModule != null)
            {
                // モジュールインポートの復元

                foreach (var imp in _inportModule)
                {
                    ApiImporter.SetModule(imp, null);
                }
                _inportModule = null;
            }
            if (_inportDllModule != null)
            {
                // DLL インポートの復元

                foreach (var imp in _inportDllModule)
                {
                    ApiImporter.LoadDll(imp, null);
                }
                _inportModule = null;
            }
        }

        [Conditional("DEBUG")]
        private void AddTestTreeAsset(TreeViewCommand treeViewCommand)
        {
            {
                var testCommandNode = new TreeMenuNode("TestCommand");
                testCommandNode.AddChild(new TreeMenuNode("OutputControlXML()", CreateImmediateExecutionCanvasCommand(() => OutputControlXML())));
                treeViewCommand.AssetTreeData.Add(testCommandNode);
            }
            #region Connector
#if false// 今は動かない
            {
                var connectorNode = new TreeMenuNode("Connector");
                connectorNode.AddChild(new TreeMenuNode("Node List", CreateEventCanvasCommand(
                    () =>
                    {
                        var ret = new RunableControl();
                        ret.AssetFunctionType = RunableFunctionType.Execute;
                        return ret;
                    }
                    )));
                connectorNode.AddChild(new TreeMenuNode("SingleRootConnector", CreateEventCanvasCommand(() => CbScript.MakeSingleRootConnector())));
                connectorNode.AddChild(new TreeMenuNode("MakeMultiRootConnector", CreateEventCanvasCommand(() => CbScript.MakeMultiRootConnector())));
                connectorNode.AddChild(new TreeMenuNode("SingleLinkConnector", CreateEventCanvasCommand(() => CbScript.MakeSingleLinkConnector())));
                connectorNode.AddChild(new TreeMenuNode("MultiLinkConnector", CreateEventCanvasCommand(() => CbScript.MakeMultiLinkConnector())));
                treeViewCommand.AssetTreeData.Add(connectorNode);
            }
#endif
            #endregion
            {
                var testAssetNode = new TreeMenuNode("TestAsset");
                testAssetNode.AddChild(new TreeMenuNode("Rectangle", CreateEventCanvasCommand(testAssetNode.Name + ".Rectangle", () => new Rectangle() { Fill = Brushes.Red, Width = 50, Height = 50 })));
                treeViewCommand.AssetTreeData.Add(testAssetNode);
            }
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
            if (CommandCanvasList.OwnerWindow.Cursor == Cursors.Wait)
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
            CommandCanvasControl.RequestSetTitle(openFileName);
        }

        /// <summary>
        /// キャンバスの作業を保存します。
        /// </summary>
        public void SaveXML()
        {
            if (CommandCanvasList.OwnerWindow.Cursor == Cursors.Wait)
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

            if (CommandCanvasList.OwnerWindow.Cursor == Cursors.Wait)
                return;

            CommandCanvasList.OwnerWindow.Cursor = Cursors.Wait;

            try
            {
                var writer = new StringWriter();
                var serializer = new XmlSerializer(ScriptCommandCanvas.AssetXML.GetType());
                var namespaces = new XmlSerializerNamespaces();
                namespaces.Add(string.Empty, string.Empty);
                ScriptCommandCanvas.AssetXML.WriteAction();
                serializer.Serialize(writer, ScriptCommandCanvas.AssetXML, namespaces);
                StreamWriter swriter = new StreamWriter(path, false);
                swriter.WriteLine(writer.ToString());
                swriter.Close();

                OpenFileName = path;
                CommandCanvasList.OutPut.OutLine("System", $"Save...\"{path}.xml\"");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            CommandCanvasList.OwnerWindow.Cursor = Cursors.Arrow;
        }

        /// <summary>
        /// キャンバスの作業を読み込みます。
        /// </summary>
        public void LoadXML()
        {
            if (CommandCanvasList.OwnerWindow.Cursor == Cursors.Wait)
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
            if (CommandCanvasList.OwnerWindow.Cursor == Cursors.Wait)
                return;

            OpenFileName = path;
            CommandCanvasList.OwnerWindow.Cursor = Cursors.Wait;
            ScriptWorkCanvas.Dispatcher.BeginInvoke(new Action(() =>
            {
                StreamReader reader = null;
                try
                {
                    reader = new StreamReader(path);
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
                    MessageBox.Show(ex.Message);
                }
                finally
                {
                    reader?.Close();
                }
                ClearWorkCanvas(false);
                GC.Collect();

                PointIdProvider.InitCheckRequest();
                ScriptCommandCanvas.AssetXML.ReadAction(ScriptCommandCanvas);

                ScriptCommandCanvas.Dispatcher.BeginInvoke(new Action(() =>
                {
                    // アイドル状態になってから戻す

                    GC.Collect();
                    CommandCanvasList.OwnerWindow.Cursor = Cursors.Arrow;
                    if (CommandCanvasControl.IsAutoExecute)
                    {
                        CommandCanvasControl.CallPublicExecuteEntryPoint();
                        CommandCanvasControl.IsAutoExecute = false;
                    }
                    else
                    {
                        CommandCanvasControl.IsAutoExit = false;
                    }
                }), DispatcherPriority.ApplicationIdle);
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
            ScriptCommandCanvas.HideWorkStack();
            if (full)
            {
                OpenFileName = "";
            }
        }

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

            // ダイアログを表示する
            if (dialog.ShowDialog() == true)
            {
                return dialog.FileName;
            }
            return null;
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
            if (ScriptWorkCanvas.Count != 0 &&
                    MessageBox.Show(CapybaraVS.Language.GetInstance["ConfirmationDelete"],
                        CapybaraVS.Language.GetInstance["Confirmation"],
                        MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {
                CommandCanvasList.OwnerWindow.Cursor = Cursors.Wait;
                ScriptWorkCanvas.Dispatcher.BeginInvoke(new Action(() =>
                {
                    ClearWorkCanvas();
                    ScriptCommandCanvas.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        // アイドル状態になってから戻す

                        GC.Collect();
                        CommandCanvasList.OwnerWindow.Cursor = Cursors.Arrow;

                    }), DispatcherPriority.ApplicationIdle);
                }), DispatcherPriority.ApplicationIdle);
            }
        }

        /// <summary>
        /// コマンドメニューを表示します。
        /// </summary>
        /// <param name="pos">表示位置</param>
        public void ShowCommandMenu(Point? pos = null)
        {
            CommandMenuWindow.SetPos(pos);
            CommandMenuWindow.ShowDialog();
        }

        public void Dispose()
        {
            if (CommandMenuWindow is null)
                return;

            CommandMenuWindow.Dispose();
            CommandMenuWindow = null;
        }
    }
}
