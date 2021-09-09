using CapybaraVS;
using CapybaraVS.Control.BaseControls;
using CapybaraVS.Controls.BaseControls;
using CapybaraVS.Script;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Xml;
using System.Xml.Serialization;
using Path = System.IO.Path;

namespace CapyCSS.Controls
{
    /// <summary>
    /// CommandCanvasList.xaml の相互作用ロジック
    /// </summary>
    public partial class CommandCanvasList
        : UserControl
        , IDisposable
    {
        public static readonly int DATA_VERSION = 1;

        #region XML定義
        [XmlRoot("CapyCSS")]
        public class _AssetXML<OwnerClass> : IDisposable
            where OwnerClass : CommandCanvasList
        {
            [XmlIgnore]
            public Action WriteAction = null;
            [XmlIgnore]
            public Action<OwnerClass> ReadAction = null;
            public _AssetXML()
            {
                ReadAction = (self) =>
                {
                    CapybaraVS.Language.Instance.LanguageType = Language;

                    if (BackGroundImagePath != null)
                    {
                        // 作業領域の背景イメージのパスをセットする

                        BaseWorkCanvas.BackGrountImagePath = BackGroundImagePath;
                    }

                    while (ScriptWorkRecentList.Count >= 10)
                    {
                        ScriptWorkRecentList.RemoveAt(0);
                    }
                    self.scriptWorkRecentList = ScriptWorkRecentList;

                    // 次回の為の初期化
                    self.AssetXML = new _AssetXML<CommandCanvasList>(self);
                };
            }
            public _AssetXML(OwnerClass self)
            {
                WriteAction = () =>
                {
                    DataVersion = DATA_VERSION;

                    Language = CapybaraVS.Language.Instance.LanguageType;

                    BackGroundImagePath = BaseWorkCanvas.BackGrountImagePath;

                    ScriptWorkRecentList = self.scriptWorkRecentList;
                };
            }
            #region 固有定義
            public int DataVersion { get; set; } = 0;
            public string Language { get; set; } = "ja-JP";
            public string BackGroundImagePath { get; set; } = null;

            public List<string> ScriptWorkRecentList = null;    // 意味無し？
            private bool disposedValue;

            protected virtual void Dispose(bool disposing)
            {
                if (!disposedValue)
                {
                    if (disposing)
                    {
                        WriteAction = null;
                        ReadAction = null;

                        // 以下、固有定義開放
                        Language = null;
                        BackGroundImagePath = null;
                        ScriptWorkRecentList?.Clear();
                        ScriptWorkRecentList = null;
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
        public _AssetXML<CommandCanvasList> AssetXML { get; set; } = null;
        #endregion

        public ObservableCollection<CommandCanvas> CanvasData { get; set; } = new ObservableCollection<CommandCanvas>();

        public static CommandCanvasList Instance = null;
        public static OutPutLog OutPut => Instance.MainLog;

        public static string CAPYCSS_WORK_PATH = null;
        public static string CAPYCSS_INFO_PATH = @"CapyCSS.xml";
        public static string CAPYCSS_DLL_DIR_PATH = @"dll"; // DLL保存用ディレクトリ
        public static string CAPYCSS_PACKAGE_DIR_PATH = @"package"; // NuGet保存用ディレクトリ

        public bool IsAutoExecute = false;   // スクリプトの自動実行
        public bool IsAutoExit = false;      // スクリプトの自動実行後自動終了

        public static string ErrorLog = "";

        public Action<string> SetTitleFunc = null;

        public static Action CallClosing = null;

        public string DllDir = null;

        public string PackageDir = null;

        private int CurrentTabIndex = -1;

        private List<string> scriptWorkRecentList = new List<string>();

        private static Window ownerWindow = null;

        public static Window OwnerWindow => ownerWindow;

        private TabItem CurrentTabItem
        {
            get
            {
                if (CurrentTabIndex == -1)
                    return null;
                return Tab.Items[CurrentTabIndex] as TabItem;
            }
        }

        private CommandCanvas CurrentScriptCanvas = null;
        public CommandCanvasList()
        {
            InitializeComponent();
            AssetXML = new _AssetXML<CommandCanvasList>(this);
            ClearPublicExecuteEntryPoint(null);
            Instance = this;

            Tab.Items.Clear();
            Dispatcher.BeginInvoke(new Action(() =>
            {
                // アイドル状態になってから新規シートを作成する

                AddNewContents();
                if (reserveLoadCbsFilePath != null)
                {
                    // 起動読み込みをキックする（タイミングが不安…）

                    KickLoadCbsFile();
                }
            }), DispatcherPriority.ApplicationIdle);
        }

        /// <summary>
        /// セットアップ処理です。
        /// </summary>
        /// <param name="owner">オーナーウインドウ</param>
        /// <param name="settingFile">セッティングファイルパス</param>
        /// <param name="setTitleFunc">タイトル表示処理（func(filename)）</param>
        /// <param name="closingFunc">終了処理</param>
        /// <param name="autoLoadCbsFile">起動時読み込みcbsファイル</param>
        /// <param name="isAutoExecute">起動時実行</param>
        /// <param name="isAutoExit">自動終了</param>
        public void Setup(
            Window owner,
            string path,
            Action<string> setTitleFunc = null,
            Action closingFunc = null,
            string autoLoadCbsFile = null,
            bool isAutoExecute = false,
            bool isAutoExit = false)
        {
            ownerWindow = owner;
            SetTitleFunc = setTitleFunc;

            CAPYCSS_WORK_PATH = path;
            CAPYCSS_INFO_PATH = Path.Combine(CAPYCSS_WORK_PATH, CAPYCSS_INFO_PATH);
            string dllDir = Path.Combine(CAPYCSS_WORK_PATH, CAPYCSS_DLL_DIR_PATH);
            string packageDir = Path.Combine(CAPYCSS_WORK_PATH, CAPYCSS_PACKAGE_DIR_PATH);
            if (!Directory.Exists(dllDir))
            {
                Directory.CreateDirectory(dllDir);
            }
            if (!Directory.Exists(packageDir))
            {
                Directory.CreateDirectory(packageDir);
            }

            DllDir = dllDir;
            PackageDir = packageDir;
            CallClosing = closingFunc;
            SetTitleFunc(null);
            if (autoLoadCbsFile != null)
            {
                SetLoadCbsFile(autoLoadCbsFile);
            }
            if (!File.Exists(CAPYCSS_INFO_PATH))
            {
                SaveInfo();
            }
            LoadInfo(CAPYCSS_INFO_PATH);
            IsAutoExecute = isAutoExecute;
            IsAutoExit = isAutoExit;
            ShowSystemErrorLog();
        }

        /// <summary>
        /// 全体的な情報を保存します。
        /// </summary>
        public void SaveInfo()
        {
            try
            {
                var writer = new StringWriter();
                var serializer = new XmlSerializer(AssetXML.GetType());
                var namespaces = new XmlSerializerNamespaces();
                namespaces.Add(string.Empty, string.Empty);
                AssetXML.WriteAction();
                serializer.Serialize(writer, AssetXML, namespaces);
                StreamWriter swriter = new StreamWriter(CAPYCSS_INFO_PATH, false);
                swriter.WriteLine(writer.ToString());
                swriter.Close();
            }
            catch (Exception ex)
            {
                ControlTools.ShowErrorMessage(ex.Message);
            }
        }

        /// <summary>
        /// 全体的な情報を読み込みます。
        /// </summary>
        /// <param name="filename">保存ファイル名</param>
        public void LoadInfo(string filename)
        {
            try
            {
                StreamReader reader = new StreamReader(filename);
                XmlSerializer serializer = new XmlSerializer(AssetXML.GetType());

                XmlDocument doc = new XmlDocument();
                doc.PreserveWhitespace = true;
                doc.Load(reader);
                XmlNodeReader nodeReader = new XmlNodeReader(doc.DocumentElement);

                object data = (CommandCanvasList._AssetXML<CommandCanvasList>)serializer.Deserialize(nodeReader);
                AssetXML = (CommandCanvasList._AssetXML<CommandCanvasList>)data;
                reader.Close();
                AssetXML.ReadAction(this);
            }
            catch (Exception ex)
            {
                ControlTools.ShowErrorMessage(ex.Message);
            }
        }

        /// <summary>
        /// CBS ファイルを保存します。
        /// </summary>
        public void SaveCbsFile()
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                CurrentScriptCanvas.SaveXML();
                {
                    CurrentTabItem.Header = System.IO.Path.GetFileNameWithoutExtension(CurrentScriptCanvas.OpenFileName);
                }
            }), DispatcherPriority.ApplicationIdle);
        }

        /// <summary>
        /// CBS ファイルを上書き保存します。
        /// </summary>
        public void OverwriteCbsFile()
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                CurrentScriptCanvas.OverwriteSaveXML();
                if (CurrentScriptCanvas.OpenFileName != null && CurrentScriptCanvas.OpenFileName != "")
                {
                    CurrentTabItem.Header = System.IO.Path.GetFileNameWithoutExtension(CurrentScriptCanvas.OpenFileName);
                }
            }), DispatcherPriority.ApplicationIdle);
        }

        /// <summary>
        /// 読み込み予約cbsファイルです。
        /// </summary>
        private string reserveLoadCbsFilePath = null;

        /// <summary>
        /// CBS ファイル読み込みを予約します。
        /// </summary>
        /// <param name="path">CBSファイルのパス</param>
        public void SetLoadCbsFile(string path = null)
        {
            reserveLoadCbsFilePath = path;
        }

        /// <summary>
        /// CBS ファイルを読み込み
        /// </summary>
        /// <param name="path">CBSファイルのパス</param>
        public void LoadCbsFile(string path = null)
        {
            reserveLoadCbsFilePath = path;
            KickLoadCbsFile();
        }

        /// <summary>
        /// 予約された CBS ファイルを読み込みます。
        /// </summary>
        public void KickLoadCbsFile()
        {
            CurrentScriptCanvas?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (reserveLoadCbsFilePath is null)
                {
                    CurrentScriptCanvas.LoadXML();
                    if (CurrentScriptCanvas.OpenFileName != null && CurrentScriptCanvas.OpenFileName != "")
                    {
                        CurrentTabItem.Header = System.IO.Path.GetFileNameWithoutExtension(CurrentScriptCanvas.OpenFileName);
                    }
                }
                else
                {
                    CurrentScriptCanvas.LoadXML(System.IO.Path.GetFullPath(reserveLoadCbsFilePath));
                    if (CurrentScriptCanvas.OpenFileName != null && CurrentScriptCanvas.OpenFileName != "")
                    {
                        CurrentTabItem.Header = System.IO.Path.GetFileNameWithoutExtension(reserveLoadCbsFilePath);
                    }
                }
                reserveLoadCbsFilePath = null;
            }), DispatcherPriority.ApplicationIdle);
        }

        private static int newNumbering = 1;

        /// <summary>
        /// 新規スクリプト作業領域を作成します。
        /// </summary>
        private void AddNewContents()
        {
            string newName = $"New{newNumbering++}";
            CurrentScriptCanvas = CreateCanvas();
            Tab.Items.Add(
                new TabItem()
                {
                    Header = newName,
                    Content = CurrentScriptCanvas
                }
                );
            if (CurrentTabIndex == -1)
            {
                CurrentTabIndex = 0;
            }
            SetupScriptCommandRecent(CurrentScriptCanvas);
            Dispatcher.BeginInvoke(new Action(() =>
            {
                // アイドル状態になって反映する

                Tab.SelectedIndex = Tab.Items.Count - 1;
            }), DispatcherPriority.ApplicationIdle);
        }

        ~CommandCanvasList()
        {
            Dispose();
        }

        /// <summary>
        /// スクリプト用キャンバスを作成します。
        /// </summary>
        /// <returns></returns>
        private CommandCanvas CreateCanvas()
        {
            var canvas = new CommandCanvas(this);
            CanvasData.Add(canvas);
            return canvas;
        }

        private bool backupIsEnebleExecuteButton = false;
        private bool backupIsEnebleExecuteAllButton = false;

        /// <summary>
        /// 実行ボタンの有効か無効かを制御します。
        /// </summary>
        /// <param name="enable">true = 有効</param>
        public void SetExecuteButtonEnable(bool enable)
        {
            if (enable)
            {
                // 実行解除時
                // ※ 復元として動作する

                ExecuteButton.IsEnabled = backupIsEnebleExecuteButton;
                ExecuteAllButton.IsEnabled = backupIsEnebleExecuteAllButton;
            }
            else
            {
                // 実行時

                backupIsEnebleExecuteButton = ExecuteButton.IsEnabled;
                backupIsEnebleExecuteAllButton = ExecuteAllButton.IsEnabled;
                ExecuteButton.IsEnabled = false;
                ExecuteAllButton.IsEnabled = false;
            }
            LoadButton.IsEnabled = enable;
            SaveButton.IsEnabled = enable;
            CommandMenuButton.IsEnabled = enable;
            AddButton.IsEnabled = enable;
            DeleteButton.IsEnabled = enable;
        }

        public bool IsLockExecute = false;

        class EntryPoint
        {
            public object owner = null;
            public Action action = null;
            public EntryPoint(object _owner, Action _action)
            {
                owner = _owner;
                action = _action;
            }
        }

        /// <summary>
        /// 全スクリプトエントリーポイントEnableリスト
        /// </summary>
        private List<Action<bool>> AllExecuteEntryPointList = new List<Action<bool>>();

        /// <summary>
        /// スクリプト実行用公開エントリーポイントリスト
        /// </summary>
        private List<EntryPoint> PublicExecuteEntryPointList = new List<EntryPoint>();

        /// <summary>
        /// スクリプト実行用公開エントリーポイントリストをクリアします。
        /// </summary>
        public void ClearPublicExecuteEntryPoint(object owner)
        {
            AllExecuteEntryPointList.Clear();
            AddAllExecuteEntryPointEnable(SetExecuteButtonEnable);
            if (owner is null)
            {
                PublicExecuteEntryPointList.Clear();
            }
            else
            {
                PublicExecuteEntryPointList.RemoveAll(s => s.owner == owner);
            }
            UpdateButtonEnable();
        }

        /// <summary>
        /// 全スクリプトエントリーポイントEnableリストにEnable制御を追加します。
        /// </summary>
        public void AddAllExecuteEntryPointEnable(Action<bool> func)
        {
            AllExecuteEntryPointList.Add(func);
        }

        /// <summary>
        /// 全スクリプトエントリーポイントリストからエントリーポイントを削除します。
        /// </summary>
        public void RemoveAllExecuteEntryPointEnable(Action<bool> func)
        {
            AllExecuteEntryPointList.Remove(func);
        }

        /// <summary>
        /// 全スクリプトエントリーポイントリストを使ってすべての Enable を制御します。
        /// </summary>
        public void CallAllExecuteEntryPointEnable(bool enable)
        {
            foreach (var act in AllExecuteEntryPointList)
            {
                act?.Invoke(enable);
            }
        }

        /// <summary>
        /// スクリプト実行用公開エントリーポイントリストにエントリーポイントを追加します。
        /// </summary>
        public void AddPublicExecuteEntryPoint(object owner, Action func)
        {
            PublicExecuteEntryPointList.Add(new EntryPoint(owner, func));
            UpdateButtonEnable();
        }

        /// <summary>
        /// ボタンの有効無効を一括設定します。
        /// </summary>
        private void UpdateButtonEnable()
        {
            ExecuteButton.IsEnabled = PublicExecuteEntryPointList.Any(s => s.owner == CurrentScriptCanvas);
            ExecuteAllButton.IsEnabled = PublicExecuteEntryPointList.Count != 0;
            LoadButton.IsEnabled = Tab.Items.Count != 0;
            SaveButton.IsEnabled = Tab.Items.Count != 0;
            DeleteButton.IsEnabled = Tab.Items.Count != 0;
            CommandMenuButton.IsEnabled = Tab.Items.Count != 0;
        }

        /// <summary>
        /// スクリプト実行用公開エントリーポイントリストからエントリーポイントを削除します。
        /// </summary>
        public void RemovePublicExecuteEntryPoint(Action func)
        {
            PublicExecuteEntryPointList.RemoveAll(s => s.action == func);
            UpdateButtonEnable();
        }

        /// <summary>
        /// スクリプト実行用公開エントリーポイントリストからエントリーポイントをまとめて順次呼び出しします。
        /// </summary>
        public void CallPublicExecuteEntryPoint(object owner = null)
        {
            if (IsLockExecute)
                return; // 再入を禁止する

            foreach (var act in PublicExecuteEntryPointList)
            {
                if (owner is null)
                {
                    act.action?.Invoke();
                }
                else
                {
                    if (act.owner == owner)
                    {
                        act.action?.Invoke();
                    }
                }
            }
            Dispatcher.BeginInvoke(new Action(() =>
            {
                // アイドル状態になってから戻す

                if (IsAutoExit)
                {
                    // スクリプト実行後自動終了

                    CallClosing?.Invoke();
                }
            }), DispatcherPriority.ApplicationIdle);
        }

        /// <summary>
        /// スクリプト読み込みボタンの処理です。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            LoadCbsFile();
        }

        /// <summary>
        /// スクリプト保存ボタンの処理です。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            SaveCbsFile();
        }

        /// <summary>
        /// 追加ボタンの処理です。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            AddNewContents();
        }

        /// <summary>
        /// スクリプト作業領域を削除するボタンの処理です。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            CurrentScriptCanvas.ClearWorkCanvas();
            CanvasData.Remove(CurrentScriptCanvas);
            CurrentScriptCanvas.Dispose();
            Tab.Items.Remove(CurrentTabItem);
            if (Tab.Items.Count == 0)
            {
                CurrentScriptCanvas = null;
                RequestSetTitle();
            }
            UpdateButtonEnable();
        }

        /// <summary>
        /// コマンドメニューを表示ボタンの処理です。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CommandMenuButton_Click(object sender, RoutedEventArgs e)
        {
            CurrentScriptCanvas.ShowCommandMenu(new Point());
        }

        /// <summary>
        /// 全スクリプト実行ボタンの処理です。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExecuteAllButton_Click(object sender, RoutedEventArgs e)
        {
            CallPublicExecuteEntryPoint();
        }

        /// <summary>
        /// スクリプト実行ボタンの処理です。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExecuteButton_Click(object sender, RoutedEventArgs e)
        {
            CallPublicExecuteEntryPoint(CurrentScriptCanvas);
        }

        /// <summary>
        /// キーボードによる操作を定義します。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Grid_KeyDown(object sender, KeyEventArgs e)
        {
            if ((Keyboard.GetKeyStates(Key.LeftCtrl) & KeyStates.Down) > 0 ||
                (Keyboard.GetKeyStates(Key.RightCtrl) & KeyStates.Down) > 0)
            {
                // Ctrl + key

                switch (e.Key)
                {
                    case Key.S: // 保存
                        OverwriteCbsFile();
                        e.Handled = true;
                        break;

                    case Key.O: // 読み込み
                        LoadCbsFile();
                        e.Handled = true;
                        break;

                    case Key.N: // 新規項目の追加
                        AddNewContents();
                        e.Handled = true;
                        break;

                    case Key.F5:    // 全スクリプト実行
                        CallPublicExecuteEntryPoint();
                        e.Handled = true;
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
                    case Key.F5:    // スクリプト実行
                        CallPublicExecuteEntryPoint(CurrentScriptCanvas);
                        e.Handled = true;
                        break;
                }
            }
        }

        /// <summary>
        /// タブを切り替えた時の処理です。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Tab_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is TabControl && e.AddedItems.Count != 0)
            {
                TabItem selectedTab = e.AddedItems[0] as TabItem;
                int index = 0;
                //CurrentTabIndex = -1;
                foreach (var node in Tab.Items)
                {
                    if (node == selectedTab)
                    {
                        CurrentTabIndex = index;
                        SetCurrentScriptCanvas(index);
                        break;
                    }
                    index++;
                }
            }
        }

        /// <summary>
        /// タイトルのセットを依頼します。
        /// </summary>
        /// <param name="filename">ファイル名</param>
        public void RequestSetTitle(string filename = null)
        {
            SetTitleFunc?.Invoke(filename);
        }

        /// <summary>
        /// カレントのスクリプト作業領域を切り替えます。
        /// </summary>
        /// <param name="index">スクリプト作業領域のインデックス番号</param>
        private void SetCurrentScriptCanvas(int index)
        {
            CurrentScriptCanvas = CanvasData[index];
            CurrentScriptCanvas.SetupTitle();
            UpdateButtonEnable();
        }

        /// <summary>
        /// 最近使ったスクリプトノードに記録します。
        /// </summary>
        public void AddScriptCommandRecent(string name)
        {
            if (!scriptWorkRecentList.Contains(name))
            {
                while (scriptWorkRecentList.Count >= 10)
                {
                    scriptWorkRecentList.RemoveAt(0);
                }
                scriptWorkRecentList.Add(name);
            }

            foreach (var node in CanvasData)
            {
                node.WorkCanvas.AddScriptCommandRecent(name);
            }
        }

        /// <summary>
        /// 最近使ったスクリプトノードをまとめてセットアップします。
        /// </summary>
        private void SetupScriptCommandRecent(CommandCanvas commandCanvas)
        {
            foreach (var recent in scriptWorkRecentList)
            {
                commandCanvas.WorkCanvas.AddScriptCommandRecent(recent);
            }
        }

        private void HelpButton_Click(object sender, RoutedEventArgs e)
        {
            ShowHelpWindow();
        }

        /// <summary>
        /// ヘルプウィンドウを表示します。
        /// </summary>
        private static void ShowHelpWindow()
        {
            var helpWindow = HelpWindow.Create();
            helpWindow.ShowDialog();
        }

        [ScriptMethod("System.Application")]
        static public void SetWorkCanvasBG(string path, Stretch stretch = Stretch.UniformToFill, bool overwriteSettings = false)
        {
            if (overwriteSettings)
            {
                BaseWorkCanvas.BackGrountImagePath = path;
            }

            foreach (var canvas in CommandCanvasList.Instance.CanvasData)
            {
                SetTargetWorkCanvasBB(canvas, path, stretch);
            }
        }

        private static void SetTargetWorkCanvasBB(CommandCanvas canvas, string path, Stretch stretch = Stretch.UniformToFill)
        {
            canvas.ScriptWorkCanvas.BGImage.Stretch = stretch;
            canvas.ScriptWorkCanvas.BGImage.Source = new BitmapImage(new Uri(path));
        }


        /// <summary>
        /// システムエラーを出力します。
        /// </summary>
        public static void ShowSystemErrorLog()
        {
            if (ErrorLog.Length != 0)
            {
                OutputWindow outputWindow = new OutputWindow();
                outputWindow.Title = "SystemError";
                outputWindow.Owner = CommandCanvasList.OwnerWindow;
                outputWindow.Show();

                outputWindow.AddBindText = ErrorLog;
            }
        }

        public void Dispose()
        {
            CbSTUtils.ForeachDispose(CanvasData);
            CanvasData = null;
            CapybaraVS.Language.Instance.Dispose();
            ToolExec.KillProcess();

            GC.SuppressFinalize(this);
        }
    }
}
