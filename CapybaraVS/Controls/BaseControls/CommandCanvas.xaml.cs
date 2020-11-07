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
    public partial class CommandCanvas : UserControl, IAsset
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
                    self.WorkCanvas.AssetXML = WorkCanvas;
                    self.WorkCanvas.AssetXML.ReadAction?.Invoke(self.WorkCanvas);
                    if (WorkStack != null)
                    {
                        self.WorkStack.AssetXML = WorkStack;
                        self.WorkStack.AssetXML.ReadAction?.Invoke(self.WorkStack);
                    }
                    foreach (var node in WorkCanvasAssetList)
                    {
                        try
                        {
                            Movable movableNode = new Movable();
                            self.WorkCanvas.Add(movableNode);

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

                    // 次回の為の初期化
                    self.AssetXML = new _AssetXML<CommandCanvas>(self);
                };
            }
            public _AssetXML(OwnerClass self)
            {
                WriteAction = () =>
                {
                    AssetId = self.AssetId;
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
            public Stack._AssetXML<Stack> WorkStack { get; set; } = null;
            [XmlArrayItem("Asset")]
            public List<Movable._AssetXML<Movable>> WorkCanvasAssetList { get; set; } = null;
            #endregion
        }
        public _AssetXML<CommandCanvas> AssetXML { get; set; } = null;
        #endregion

        public static ObservableCollection<TreeMenuNode> AssetTreeData { get; set; } = null;
        public CommandCanvas()
        {
            InitializeComponent();
            assetIdProvider = new AssetIdProvider(this);
            AssetXML = new _AssetXML<CommandCanvas>(this);

            ScriptCommandCanvas = this;
            ScriptWorkCanvas = WorkCanvas;
            ScriptWorkStack = WorkStack;
            AssetTreeData ??= new ObservableCollection<TreeMenuNode>();

            CommandWindow.TreeViewCommand.AssetTreeData ??= new ObservableCollection<TreeMenuNode>();
            AddTreeCommandAsset(CommandWindow.TreeViewCommand);

            Dispatcher.BeginInvoke(new Action(() =>
            {
                // アイドル状態になってから戻す

                if (App.EntryLoadFile != null)
                {
                    // 起動時にコマンドライン引数から渡されたファイルを読み込む

                    LoadXML(System.IO.Path.GetFullPath(App.EntryLoadFile));
                }
            }), DispatcherPriority.ApplicationIdle);
        }

        public void HideWorkStack()
        {
            WorkStack.Visibility = Visibility.Collapsed;
        }

        //----------------------------------------------------------------------
        #region 広域処理 TODO インスタンス化する

        public static TreeViewCommand TreeViewCommand = new TreeViewCommand();
        public static CommandCanvas ScriptCommandCanvas = null;
        public static BaseWorkCanvas ScriptWorkCanvas = null;
        public static Stack ScriptWorkStack = null;
        public static Func<object> ScriptWorkClickEvent = null;
        public static Action ClickEntryEvent = new Action(() =>
        {
            ScriptWorkCanvas.Cursor = null;
            TreeViewCommand.Cursor = null;
        });
        public static Action ClickExitEvent = new Action(() =>
        {
            CommandWindow.CloseWindow();
            ScriptWorkCanvas.Cursor = Cursors.Hand;
            TreeViewCommand.Cursor = Cursors.Hand;
        });
        public static HoldActionManager<UIParam> UIParamHoldAction = new HoldActionManager<UIParam>();
        public static HoldActionManager<StackGroup> StackGroupHoldAction = new HoldActionManager<StackGroup>();
        public static HoldActionManager<PlotWindow> PlotWindowHoldAction = new HoldActionManager<PlotWindow>();
        public static HoldActionManager<LinkConnectorList> LinkConnectorListHoldAction = new HoldActionManager<LinkConnectorList>();
        public static bool EnabledScriptHoldActionMode
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
        public static Func<object> ClickEvent
        {
            get => ScriptWorkClickEvent;
            set
            {
                if (value is null)
                {
                    // クリックイベントを無効にする

                    ClickEntryEvent?.Invoke();

                    // コマンドツリー上でのコマンドキャンセルイベントを消す
                    TreeViewCommand.MouseRightButtonDown -= (s, e) => ScriptWorkCanvas.ResetCommand();
                }
                else
                {
                    // クリックイベントを有効にする

                    ClickExitEvent?.Invoke();

                    // コマンドツリー上でのコマンドキャンセルイベントを登録する
                    TreeViewCommand.MouseRightButtonDown += (s, e) => ScriptWorkCanvas.ResetCommand();
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
        public static TreeMenuNodeCommand CreateEventCanvasCommand(string path, Func<object> action, TreeMenuNode vm = null)
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
        public static TreeMenuNodeCommand CreateImmediateExecutionCanvasCommand(Action action, TreeMenuNode vm = null)
        {
            return new TreeMenuNodeCommand(vm, (a) =>
                {
                    if (MainWindow.Instance.Cursor == Cursors.Wait)
                        return; // 処理中は禁止

                    CommandWindow.CloseWindow();

                    action?.Invoke();
                }
            );
        }

        #endregion

        //----------------------------------------------------------------------
        #region アセットリストを実装

        private static void AddTreeCommandAsset(TreeViewCommand treeViewCommand)
        {
            // コマンドを追加
            {
                var commandNode = new TreeMenuNode("Command");
                commandNode.AddChild(new TreeMenuNode("Clear(Ctrl+N)", CreateImmediateExecutionCanvasCommand(() =>
                {
                    if (CommandCanvas.ScriptWorkCanvas.Count != 0 &&
                        MessageBox.Show(CapybaraVS.Language.GetInstance["ConfirmationAllDelete"],
                            CapybaraVS.Language.GetInstance["Confirmation"],
                            MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                    {
                        MainWindow.Instance.Cursor = Cursors.Wait;
                        ScriptWorkCanvas.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            WorkCanvasClear();
                            ScriptCommandCanvas.Dispatcher.BeginInvoke(new Action(() =>
                            {
                                // アイドル状態になってから戻す

                                GC.Collect();
                                MainWindow.Instance.Cursor = Cursors.Arrow;

                            }), DispatcherPriority.ApplicationIdle);
                        }), DispatcherPriority.ApplicationIdle);
                    }
                })));
                commandNode.AddChild(new TreeMenuNode("Toggle ShowMouseInfo", CreateImmediateExecutionCanvasCommand(() => ScriptWorkCanvas.EnableInfo = ScriptWorkCanvas.EnableInfo ? false : true)));
                commandNode.AddChild(new TreeMenuNode("Toggle ShowGridLine(G)", CreateImmediateExecutionCanvasCommand(() => ScriptCommandCanvas.ToggleGridLine())));
                commandNode.AddChild(new TreeMenuNode("Save(Ctrl+S)", CreateImmediateExecutionCanvasCommand(() => SaveXML())));
                commandNode.AddChild(new TreeMenuNode("Load(Ctrl+O)", CreateImmediateExecutionCanvasCommand(() => LoadXML())));
                treeViewCommand.AssetTreeData.Add(commandNode);
            }

            // 飾りを追加
            {
                var decorationNode = new TreeMenuNode("Decoration");
                decorationNode.AddChild(new TreeMenuNode("Text Area", CreateEventCanvasCommand(decorationNode.Name + ".Text Area", () => new GroupArea() { Name = "test", Width = 150, Height = 150 })));
                treeViewCommand.AssetTreeData.Add(decorationNode);
            }

            // 基本的なアセットを追加
            new ImplementBaseAsset();

            // デバッグ用アセットを追加
            AddTestTreeAsset(treeViewCommand);
        }

        [Conditional("DEBUG")]
        private static void AddTestTreeAsset(TreeViewCommand treeViewCommand)
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

        /// <summary>
        /// キャンバスの作業をxml化して表示します。
        /// </summary>
        private static void OutputControlXML()
        {
            var outputWindow = new OutputWindow();
            outputWindow.Title = "Contorl List <Output[" + ScriptWorkCanvas.Name + "]>";
            outputWindow.Owner = MainWindow.Instance;
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
        private static void OverwriteSaveXML()
        {
            if (MainWindow.Instance.Cursor == Cursors.Wait)
                return;

            if (MainWindow.Instance.OpenFileName == "")
            {
                SaveXML();
                return;
            }

            SaveXml(MainWindow.Instance.OpenFileName);
        }

        /// <summary>
        /// キャンバスの作業を保存します。
        /// </summary>
        public static void SaveXML()
        {
            if (MainWindow.Instance.Cursor == Cursors.Wait)
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
        private static void SaveXml(string path)
        {
            if (path is null)
                return;

            if (MainWindow.Instance.Cursor == Cursors.Wait)
                return;

            MainWindow.Instance.Cursor = Cursors.Wait;

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

                MainWindow.Instance.OpenFileName = path;
                MainWindow.Instance.MainLog.OutLine("System", $"Save...\"{path}.xml\"");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            
            MainWindow.Instance.Cursor = Cursors.Arrow;
        }

        /// <summary>
        /// キャンバスの作業を読み込みます。
        /// </summary>
        private static void LoadXML()
        {
            if (MainWindow.Instance.Cursor == Cursors.Wait)
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
        public static void LoadXML(string path)
        {
            if (MainWindow.Instance.Cursor == Cursors.Wait)
                return;

            MainWindow.Instance.Cursor = Cursors.Wait;
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
                WorkCanvasClear(false);
                GC.Collect();

                PointIdProvider.InitCheckRequest();
                ScriptCommandCanvas.AssetXML.ReadAction(ScriptCommandCanvas);

                ScriptCommandCanvas.Dispatcher.BeginInvoke(new Action(() =>
                {
                    // アイドル状態になってから戻す

                    GC.Collect();
                    MainWindow.Instance.Cursor = Cursors.Arrow;
                    if (App.IsAutoExecute)
                    {
                        MainWindow.CallPublicExecuteEntryPoint();
                        App.IsAutoExecute = false;
                    }
                    else
                    {
                        App.IsAutoExit = false;
                    }

                    MainWindow.Instance.OpenFileName = path;

                }), DispatcherPriority.ApplicationIdle);
            }), DispatcherPriority.ApplicationIdle);
        }

        /// <summary>
        /// キャンバスの作業をクリアします。
        /// </summary>
        private static void WorkCanvasClear(bool full = true)
        {
            MainWindow.ClearPublicExecuteEntryPoint();
            ScriptWorkCanvas.Clear();
            ScriptWorkStack.Clear();
            ScriptCommandCanvas.HideWorkStack();
            if (full)
            {
                MainWindow.Instance.OpenFileName = "";
            }
        }

        /// <summary>
        /// 保存用ファイル選択ダイアログを表示します。
        /// </summary>
        /// <returns></returns>
        public static string ShowSaveDialog()
        {
            var dialog = new SaveFileDialog();

            // ファイルの種類を設定
            dialog.Filter = "CBSファイル (*.cbs)|*.cbs|全てのファイル (*.*)|*.*";

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
        public static string ShowLoadDialog()
        {
            var dialog = new OpenFileDialog();

            // ファイルの種類を設定
            dialog.Filter = "CBSファイル (*.cbs, *.xml)|*.cbs;*.xml|全てのファイル (*.*)|*.*";

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
            if ((Keyboard.GetKeyStates(Key.LeftCtrl) & KeyStates.Down) > 0 ||
                   (Keyboard.GetKeyStates(Key.RightCtrl) & KeyStates.Down) > 0)
            {
                // Ctrl + key

                switch (e.Key)
                {
                    case Key.S:
                        OverwriteSaveXML();
                        break;

                    case Key.O:
                        LoadXML();
                        break;

                    case Key.N:
                        if (CommandCanvas.ScriptWorkCanvas.Count != 0 &&
                                MessageBox.Show(CapybaraVS.Language.GetInstance["ConfirmationDelete"],
                                    CapybaraVS.Language.GetInstance["Confirmation"],
                                    MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                        {
                            MainWindow.Instance.Cursor = Cursors.Wait;
                            ScriptWorkCanvas.Dispatcher.BeginInvoke(new Action(() =>
                            {
                                WorkCanvasClear();
                                ScriptCommandCanvas.Dispatcher.BeginInvoke(new Action(() =>
                                {
                                    // アイドル状態になってから戻す

                                    GC.Collect();
                                    MainWindow.Instance.Cursor = Cursors.Arrow;

                                }), DispatcherPriority.ApplicationIdle);
                            }), DispatcherPriority.ApplicationIdle);
                        }
                        break;

                        // BaseWorkCanvas で使用
                    case Key.C:
                    case Key.V:
                        break;
                }
            }
            else if ((Keyboard.GetKeyStates(Key.LeftShift) & KeyStates.Down) > 0 ||
                        (Keyboard.GetKeyStates(Key.RightShift) & KeyStates.Down) > 0)
            {
                // Shift + key



            }
            else
            {
                switch (e.Key)
                {
                    case Key.G:
                        ToggleGridLine();
                        break;

                        // BaseWorkCanvas で使用
                    case Key.Delete:
                        break;
                }
            }
        }
    }
}
