using CapyCSS;
using CapyCSS.Controls.BaseControls;
using CapyCSS.Script;
using CapyCSSattribute;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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
    [ScriptClass]
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
                    CapyCSS.Language.Instance.LanguageType = Language;

                    if (DataVersion != DATA_VERSION)
                    {
                        ControlTools.ShowErrorMessage(CapyCSS.Language.Instance["Help:DataVersionError"]);
                        return;
                    }

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

                    Language = CapyCSS.Language.Instance.LanguageType;

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

        public static CommandCanvasList Instance = null;
        public static OutPutLog OutPut => Instance.MainLog;

        public const string CBSPROJ_FILTER = "CBS Project files (*.cbsproj)|*.cbsproj";
        public const string CBS_FILTER = "CBS files (*.cbs)|*.cbs";

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

        private List<string> scriptWorkRecentList = new List<string>();

        private static Window ownerWindow = null;

        public static Window OwnerWindow => ownerWindow;

        private static int cursorOverwriteCounter = 0;
        public static void SetOwnerCursor(Cursor cursor)
        {
            if (cursor != null)
            {
                cursorOverwriteCounter++;
                if (OwnerWindow.Cursor == Cursors.Wait)
                {
                    // 待機中カーソルには上書きしない

                    return;
                }
                OwnerWindow.Cursor = cursor;
            }
            else
            {
                cursorOverwriteCounter--;
                if (cursorOverwriteCounter == 0)
                {
                    OwnerWindow.Cursor = cursor;
                }
                Debug.Assert(cursorOverwriteCounter >= 0);
            }
        }

        public static Cursor GetOwnerCursor()
        {
            return OwnerWindow.Cursor;
        }

        private TabItem CurrentTabItem
        {
            get
            {
                if (Tab.SelectedIndex == -1)
                {
                    return null;
                }
                return Tab.Items[Tab.SelectedIndex] as TabItem;
            }
        }

        private CommandCanvas CurrentScriptCanvas
        {
            get
            {
                if (CurrentTabItem is null)
                {
                    return null;
                }
                return GetCommandCanvas(CurrentTabItem);
            }
        }

        public string CurrentScriptTitle
        {
            get
            {
                if (CurrentTabItem is null)
                {
                    return null;
                }
                return System.IO.Path.GetFileNameWithoutExtension((CurrentTabItem.Header as RemovableLabel).Title);
            }
        }

        /// <summary>
        /// タブタイトルからスクリプトキャンバスを探す。
        /// </summary>
        /// <param name="title">タイトル</param>
        /// <returns>スクリプトキャンバス</returns>
        private CommandCanvas FindCommandCanvas(string title)
        {
            CommandCanvas result = null;
            foreach (TabItem tabItem in Tab.Items)
            {
                string tabTitle = (tabItem.Header as RemovableLabel).Title;
                tabTitle = System.IO.Path.GetFileNameWithoutExtension(tabTitle);
                if (tabTitle == title)
                {
                    result = tabItem.Content as CommandCanvas;
                    break;
                }
            }
            return result;
        }

        /// <summary>
        /// パスからタブを探す。
        /// </summary>
        /// <param name="path"></param>
        /// <returns>タブ</returns>
        private TabItem FindTabFromPath(string path)
        {
            TabItem result = null;
            foreach (TabItem tabItem in Tab.Items)
            {
                string tabTitle = (tabItem.Header as RemovableLabel).Title;
                if (tabTitle == path)
                {
                    result = tabItem;
                    break;
                }
            }
            return result;
        }

        /// <summary>
        /// タブタイトルからスクリプトキャンバスを探す。
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private CommandCanvas FindCommandCanvasFromPath(string path)
        {
            TabItem tab = FindTabFromPath(path);
            if (tab == null)
            {
                return null;
            }
            return tab.Content as CommandCanvas;
        }

        public CommandCanvasList()
        {
            InitializeComponent();
            AssetXML = new _AssetXML<CommandCanvasList>(this);
            ClearPublicExecuteEntryPoint(null);
            Instance = this;

            // Consoleの出力先を変更する
            Console.SetOut(new SystemTextWriter());

            Dispatcher.BeginInvoke(new Action(() =>
            {
                // アイドル状態になってから新規シートを作成する

                if (reserveLoadCbsFilePath != null)
                {
                    // 起動読み込みをキックする

                    AddLoadContents(reserveLoadCbsFilePath);
                    reserveLoadCbsFilePath = null;
                }
            }), DispatcherPriority.ApplicationIdle);
        }

        /// <summary>
        /// セットアップ処理です。
        /// </summary>
        /// <param name="owner">オーナーウインドウ</param>
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
            OwnerWindow.ForceCursor = true;
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
        /// カーソルがロックされているか判定します。
        /// </summary>
        /// <returns>true==ロックされている</returns>
        public static bool IsCursorLock()
        {
            return GetOwnerCursor() == Cursors.Wait;
        }

        /// <summary>
        /// カーソルをロックします。
        /// ※UIが操作される処理用
        /// </summary>
        public static void CursorLock()
        {
            SetOwnerCursor(Cursors.Wait);
            foreach (var item in Instance.Tab.Items)
            {
                (item as TabItem).IsEnabled = false;
            }
        }

        /// <summary>
        /// カーソルをアンロックします。
        /// ※UIが操作される処理用
        /// </summary>
        public static void CursorUnlock()
        {
            SetOwnerCursor(null);
            foreach (var item in Instance.Tab.Items)
            {
                (item as TabItem).IsEnabled = true;
            }
        }

        /// <summary>
        /// カーソルをロックして処理を呼び出します。
        /// ※UIが操作される処理用
        /// </summary>
        /// <param name="action">処理</param>
        public static void CursorLock(Action action, DispatcherObject dispatcherObject = null)
        {
            if (action != null)
            {
                CursorLock();
                CursorAfterUnlock(action, dispatcherObject);
            }
        }

        /// <summary>
        /// カーソルをロックして処理を呼び出します。
        /// ※UIが操作される処理用
        /// </summary>
        /// <param name="action">処理</param>
        /// <returns>true==処理を呼び出した</returns>
        public static bool TryCursorLock(Action action, DispatcherObject dispatcherObject = null)
        {
            if (action != null && !IsCursorLock())
            {
                CursorLock();
                CursorAfterUnlock(action, dispatcherObject);
                return true;
            }
            return false;
        }

        /// <summary>
        /// カーソルをロックして処理を呼び出します。
        /// ただし、処理後にロックを解除しません。
        /// ※UIが操作される処理用
        /// </summary>
        /// <param name="action">処理</param>
        /// <returns>true==処理を呼び出した</returns>
        public static bool TryCursorLockNotUnlock(Action action)
        {
            if (action != null && !IsCursorLock())
            {
                CursorLock();
                action();
                return true;
            }
            return false;
        }

        /// <summary>
        /// 処理を呼び出した後にカーソルをアンロックします。
        /// ※UIが操作される処理用
        /// </summary>
        /// <param name="action">処理</param>
        public static void CursorAfterUnlock(Action action, DispatcherObject dispatcherObject = null)
        {
            Debug.Assert(action != null);
            action();
            if (dispatcherObject is null)
            {
                dispatcherObject = Instance;
            }
            Instance.Dispatcher.BeginInvoke(new Action(() =>
                {
                    CursorUnlock();
                }), DispatcherPriority.ApplicationIdle);
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
        /// キャンバスの作業を上書き保存します。
        /// </summary>
        public void OverwriteSaveXML()
        {
            if (IsCursorLock())
            {
                return;
            }

            string path;
            if (CurrentScriptCanvas.OpenFileName == "")
            {
                path = ShowSaveDialog(CBS_FILTER, CurrentScriptCanvas.OpenFileName);

                if (path is null)
                {
                    return;
                }
            }
            else
            {
                path = CurrentScriptCanvas.OpenFileName;
            }

            CurrentScriptCanvas.SaveXML(path);
        }

        /// <summary>
        /// CBS ファイルを保存します。
        /// </summary>
        public void SaveCbsFile()
        {
            if (IsCursorLock())
            {
                return;
            }

            string path = ShowSaveDialog(CBS_FILTER, CurrentScriptCanvas.OpenFileName);
            if (path is null)
            {
                return;
            }

            CurrentScriptCanvas.SaveXML(path);
            SetCurrentTabName(CurrentScriptCanvas.OpenFileName);
        }

        /// <summary>
        /// 保存用ファイル選択ダイアログを表示します。
        /// </summary>
        /// <returns>保存するCBSファイルのパス</returns>
        public static string ShowSaveDialog(string filter, string currentPath)
        {
            var dialog = new Microsoft.Win32.SaveFileDialog();

            // ディレクトリを設定
            dialog.InitialDirectory = System.IO.Path.GetDirectoryName(currentPath);

            // ファイル名を設定
            dialog.FileName = System.IO.Path.GetFileNameWithoutExtension(currentPath);

            // ファイルの種類を設定
            dialog.Filter = filter;

            // ダイアログを表示する
            if (dialog.ShowDialog() == true)
            {
                return dialog.FileName;
            }
            return null;
        }

        /// <summary>
        /// CBS ファイルを上書き保存します。
        /// </summary>
        public void OverwriteCbsFile()
        {
            Dispatcher.BeginInvoke(new Action(() =>
                {
                    OverwriteSaveXML();
                    if (CurrentScriptCanvas.OpenFileName != null && CurrentScriptCanvas.OpenFileName != "")
                    {
                        SetCurrentTabName(CurrentScriptCanvas.OpenFileName);
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
            if (IsCursorLock())
            {
                return;
            }

            if (path is null)
            {
                path = ShowLoadDialog(CBS_FILTER);
                if (path is null)
                {
                    return;
                }
            }

            if (TryTabForcusByContainsPath(path))
            {
                return;
            }

            if (!File.Exists(path))
            {
                string msg = string.Format(CapyCSS.Language.Instance["SYSTEM_ModuleControler_04"], path);
                ControlTools.ShowErrorMessage(msg);
                return;
            }

            TryCursorLockNotUnlock(() =>
                {
                    try
                    {
                        LoadXMLCurrentScriptCanvas(path);
                    }
                    catch (Exception ex)
                    {
                        ControlTools.ShowErrorMessage(ex.Message);
                        CursorUnlock();
                    }
                });
        }

        /// <summary>
        /// 既にファイルが読み込まれていればタブにフォーカスを当てます。
        /// </summary>
        /// <param name="path">探すファイルパス</param>
        /// <returns>true==見つかった</returns>
        private bool TryTabForcusByContainsPath(string path)
        {
            TabItem tab = FindTabFromPath(path);
            if (tab != null)
            {
                // 既に存在するのでタブを表示するだけにする

                tab.IsSelected = true;
                return true;
            }
            return false;
        }

        /// <summary>
        /// カレントのスクリプトキャンバスにcbsファイルを読み込みます。
        /// </summary>
        /// <param name="path">ファイルパス</param>
        public void LoadXMLCurrentScriptCanvas(string path)
        {
            Debug.Assert(path != null);
            CurrentScriptCanvas.LoadXML(
                System.IO.Path.GetFullPath(path),
                () =>
                {
                    SetCurrentTabName(path);
                    CursorUnlock();
                });
        }

        /// <summary>
        /// ファイル作成用ダイアログを表示します。
        /// </summary>
        /// <returns>読み込むCBSファイルのパス</returns>
        public static string MakeNewFileDialog(string filter)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog();

            dialog.AddExtension = true;
            dialog.CheckFileExists = false;

            // ファイルの種類を設定
            dialog.Filter = filter;

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

        /// <summary>
        /// 読み込み用ファイル選択ダイアログを表示します。
        /// </summary>
        /// <returns>読み込むCBSファイルのパス</returns>
        public static string ShowLoadDialog(string filter)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog();

            // ファイルの種類を設定
            dialog.Filter = filter;

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

        /// <summary>
        /// カレントタブの名前を変更します。
        /// </summary>
        /// <param name="path">ファイルパス</param>
        public void SetCurrentTabName(string path)
        {
            var label = CurrentTabItem.Header as RemovableLabel;
            label.Title = path;
            RequestSetTitle();
        }

        /// <summary>
        /// 新しいタブを作ってcbsファイルを読み込みます。
        /// </summary>
        /// <param name="path">cbsファイルのパス</param>
        public void AddLoadContents(string path)
        {
            if (!File.Exists(path))
            {
                string msg = string.Format(CapyCSS.Language.Instance["SYSTEM_ModuleControler_04"], path);
                ControlTools.ShowErrorMessage(msg);
                return;
            }
            TryCursorLockNotUnlock(() =>
                {
                    if (TryTabForcusByContainsPath(path))
                    {
                        CursorUnlock();
                        return;
                    }
                    _AddNewContents();
                    Dispatcher.BeginInvoke(new Action(() =>
                        {
                            try
                            {
                                LoadXMLCurrentScriptCanvas(path);   // ロック解除は内部で行われる。
                            }
                            catch (Exception ex)
                            {
                                ControlTools.ShowErrorMessage(ex.Message);
                                CursorUnlock();
                            }
                        }), DispatcherPriority.ApplicationIdle);
                });
        }

        private static int newNumbering = 1;

        /// <summary>
        /// 新規スクリプト作業領域を作成します。
        /// </summary>
        /// <param name="path">新しいスクリプトファイル</param>
        /// <returns>true==作成できた</returns>
        public bool AddNewContents(string path = null)
        {
            if (FindTabFromPath(path) != null)
            {
                // 既に存在している

                ControlTools.ShowErrorMessage(CapyCSS.Language.Instance["Help:SYSTEM_Error_Exist"]);
                return false;
            }

            CursorLock(() =>
                {
                    _AddNewContents(path);
                });

            return true;
        }

        private void _AddNewContents(string path = null)
        {
            CommandCanvas commandCanvas = new CommandCanvas(this);
            if (path is null)
            {
                path = $"New{newNumbering++}";
            }
            else
            {
                commandCanvas.OpenFileName = path;
            }
            var newItem = new TabItem()
            {
                Content = commandCanvas
            };
            newItem.Header = new RemovableLabel()
            {
                Title = path,
                Mask = (n) => System.IO.Path.GetFileNameWithoutExtension(n),
                ClickEvent = () =>
                {
                    // タブ付属の削除機能

                    TryCursorLock(() =>
                    {
                        RemoveScriptCanvas(newItem);
                    });
                }
            };
            Tab.Items.Add(newItem);
            newItem.IsSelected = true;
            SetupScriptCommandRecent(commandCanvas);
        }

        ~CommandCanvasList()
        {
            Dispose();
        }

        private bool backupIsEnebleExecuteButton = false;

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
            }
            else
            {
                // 実行時

                backupIsEnebleExecuteButton = ExecuteButton.IsEnabled;
                ExecuteButton.IsEnabled = false;
            }
            LoadButton.IsEnabled = enable;
            SaveButton.IsEnabled = enable;
            CommandMenuButton.IsEnabled = enable;
            AddButton.IsEnabled = enable;
            ConvertCSS.IsEnabled = ExecuteButton.IsEnabled;
        }

        public delegate object NodeFunction(bool fromScript, string entryPointName);

        /// <summary>
        /// エントリーポイント管理
        /// </summary>
        class EntryPoint
        {
            public object owner = null;
            public NodeFunction function = null;
            public EntryPoint(object owner, NodeFunction func)
            {
                this.owner = owner;
                this.function = func;
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
            foreach (var enableAction in AllExecuteEntryPointList)
            {
                enableAction?.Invoke(enable);
            }
        }

        /// <summary>
        /// スクリプト実行用公開エントリーポイントリストにエントリーポイントを追加します。
        /// </summary>
        public void AddPublicExecuteEntryPoint(object owner, NodeFunction func)
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
            LoadButton.IsEnabled = !IsEmptyScriptCanvas;
            SaveButton.IsEnabled = !IsEmptyScriptCanvas;
            CommandMenuButton.IsEnabled = !IsEmptyScriptCanvas;
            ConvertCSS.IsEnabled = ExecuteButton.IsEnabled;
        }

        /// <summary>
        /// スクリプト実行用公開エントリーポイントリストからエントリーポイントを削除します。
        /// </summary>
        public void RemovePublicExecuteEntryPoint(NodeFunction func)
        {
            PublicExecuteEntryPointList.RemoveAll(s => s.function == func);
            UpdateButtonEnable();
        }

        [ScriptMethod(path: CapyCSSbase.FlowLib.LIB_FLOW_NAME)]
        /// <summary>
        /// エントリーポイントリストからエントリーポイントを探して呼び出します。
        /// エントリーポイント名を指定していない場合は、カレントタブのエントリーポイントが呼ばれます。
        /// ※スクリプトからの呼び出し用です。
        /// </summary>
        /// <param name="entryPointName">エントリーポイント名</param>
        /// <returns>スクリプトの返し値</returns>
        public static object CallEntryPoint(string entryPointName = null)
        {
            return Instance.CallPublicExecuteEntryPoint(true, entryPointName);
        }

        [ScriptMethod(oldSpecification: true)]
        /// <summary>
        /// （廃止）
        /// ※スクリプトからの呼び出し用です。
        /// </summary>
        /// <param name="entryPointName">エントリーポイント名</param>
        /// <returns>スクリプトの返し値</returns>
        public static object CallCurrentWorkEntryPoint(string entryPointName = null)
        {
            return Instance.CallPublicExecuteEntryPoint(true, entryPointName);
        }

        /// <summary>
        /// エントリーポイント名が存在するかチェックします。
        /// </summary>
        /// <param name="owner">対象インスタンスを限定</param>
        /// <param name="entryPointName">エントリーポイント名</param>
        /// <returns>存在する==true</returns>
        public bool IsExistEntryPoint(object owner, string entryPointName)
        {
            if (entryPointName != null && entryPointName.Trim().Length == 0)
            {
                return false;
            }

            int count = 0;
            foreach (EntryPoint entryPoint in PublicExecuteEntryPointList)
            {
                if (entryPoint.function is null)
                {
                    continue;
                }
                object result = null;
                if (owner is null)
                {
                    result = entryPoint.function.Invoke(false, ":" + entryPointName);
                }
                else
                {
                    if (entryPoint.owner == owner)
                    {
                        result = entryPoint.function.Invoke(false, ":" + entryPointName);
                    }
                }
                if (result != null)
                {
                    // 実行結果が返ってきた

                    count++;
                    if (count > 1)
                    {
                        // 自分以外にもう一つ有った

                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// すべてのエントリーポイントに名前の衝突チェックを依頼します。
        /// </summary>
        /// <param name="owner">対象インスタンスを限定</param>
        public static void CheckPickupAllEntryPoint(object owner)
        {
            Instance._CheckPickupAllEntryPoint(owner);
        }

        public void _CheckPickupAllEntryPoint(object owner)
        {
            string command = ":+";
            foreach (EntryPoint entryPoint in PublicExecuteEntryPointList)
            {
                if (entryPoint.function is null)
                {
                    continue;
                }
                if (owner is null)
                {
                    entryPoint.function.Invoke(false, command);
                }
                else
                {
                    if (entryPoint.owner == owner)
                    {
                        entryPoint.function.Invoke(false, command);
                    }
                }
            }
        }

        /// <summary>
        /// エントリーポイントリストからエントリーポイントを探して呼び出します。
        /// エントリーポイント名を指定していない場合は、カレントタブのエントリーポイントが呼ばれます。
        /// </summary>
        /// <param name="entryPointName">エントリーポイント名</param>
        /// <returns>スクリプトの返し値</returns>
        public object CallPublicExecuteEntryPoint(bool fromScript, string entryPointName = null)
        {
            var result = _CallPublicExecuteEntryPoint(fromScript, entryPointName);
            if (!fromScript)
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (IsAutoExit)
                    {
                        // スクリプト実行後自動終了

                        CallClosing?.Invoke();
                    }
                }), DispatcherPriority.ApplicationIdle);
            }
            return result;
        }

        private object _CallPublicExecuteEntryPoint(bool fromScript, string entryPointName = null)
        {
            if (!fromScript && IsCursorLock())
            {
                return null;
            }

            object owner = null;
            if (string.IsNullOrWhiteSpace(entryPointName))
            {
                // カレントの中からエントリーポイントを探す

                owner = CurrentScriptCanvas;
            }
            else
            {
                owner = FindCommandCanvas(entryPointName);
                if (owner != null)
                {
                    // タブの中に名前が見つかったので空のエントリーポイントを呼び出す

                    entryPointName = "";
                }
                else
                {
                    // タブの中に名前が見つからなかったので全ての中からエントリーポイントを探す
                }
            }

            if (entryPointName is null)
            {
                entryPointName = "";
            }

            object result = null;

            if (entryPointName != null)
            {
                if (CheckMultipleEntryPoints(owner, entryPointName))
                {
                    var errorMessage = $"multiple \"{entryPointName}\" entry points!";
                    if (owner != null)
                    {
                        // CommandCanvas から呼ばれた

                        Console.WriteLine(nameof(CommandCanvas) + $": {errorMessage}");
                        return null;
                    }
                    else
                    {
                        // CallEntryPoint から呼ばれた

                        throw new Exception(errorMessage);
                    }
                }
            }

            IEnumerable<EntryPoint> entryPoints;
            if (owner is null)
            {
                entryPoints = PublicExecuteEntryPointList.Where(n => n.function != null);
            }
            else
            {
                entryPoints = PublicExecuteEntryPointList.Where(n => n.function != null && n.owner == owner);
            }

            // １件のみ実行
            entryPoints.Any(n =>
            {
                result = n.function.Invoke(fromScript, entryPointName);
                return result != null;
            });

            return result;
        }

        /// <summary>
        /// エントリーポイント名が複数無いかチェックします。
        /// </summary>
        /// <param name="owner">対象オーナー</param>
        /// <param name="entryPointName">エントリーポイント名</param>
        /// <returns>true==複数有る</returns>
        private bool CheckMultipleEntryPoints(object owner, string entryPointName)
        {
            if (entryPointName.Trim().Length == 0)
            {
                entryPointName = null;
            }
            else if (PublicExecuteEntryPointList.Where(n => n.function.Invoke(false, ":" + entryPointName) != null).Count() > 1)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// スクリプトからc#を構築します。
        /// </summary>
        /// <param name="owner">オーナー</param>
        /// <returns>c#コード</returns>
        public string BuildScript(object owner = null)
        {
            if (owner is null)
            {
                owner = CurrentScriptCanvas;
            }
            string result = null;
            IEnumerable<EntryPoint> entryPoints = null;
            if (owner != null)
            {
                entryPoints = PublicExecuteEntryPointList.Where(n => n.function != null && n.owner == owner);
            }
            bool isWorkStackImported = false;
            if (entryPoints.Count() > 0)
            {
                foreach (var entryPoint in entryPoints)
                {
                    if (entryPoint.owner is CommandCanvas commandCanvas && !isWorkStackImported)
                    {
                        BuildScriptInfo scr = commandCanvas.WorkStack.RequestBuildScript();
                        if (scr != null)
                        {
                            result += scr.BuildScript(null);
                            isWorkStackImported = true;
                        }
                    }
                    var script = entryPoint.function.Invoke(false, ":*");
                    if (script != null)
                    {
                        result += script;
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// スクリプトからc#を構築しウインドウに出力します。
        /// </summary>
        /// <param name="owner">オーナー</param>
        public void BuildScriptAndOut(object owner = null)
        {
            string script = BuildScript(owner);
            if (!string.IsNullOrWhiteSpace(script))
            {
                OutputWindow.CreateWindow(CurrentScriptTitle).AddBindText = script;
            }
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
        /// カレントのスクリプトキャンバスを消します。
        /// </summary>
        private void RemoveCurrentScriptCanvas()
        {
            if (CurrentScriptCanvas != null)
            {
                TryCursorLock(() =>
                    {
                        RemoveScriptCanvas(CurrentTabItem);
                    });
            }
        }

        /// <summary>
        /// パスに一致するスクリプトキャンバスを消します。
        /// </summary>
        public void RemoveScriptCanvas(string path)
        {
            TabItem tab = FindTabFromPath(path);
            if (CurrentScriptCanvas != null && tab != null)
            {
                TryCursorLock(() =>
                    {
                        RemoveScriptCanvas(tab);
                    });
            }
        }

        /// <summary>
        /// スクリプトキャンバスが存在するかを確認します。
        /// </summary>
        public bool IsEmptyScriptCanvas => Tab.Items.Count == 0;

        /// <summary>
        /// すべてのスクリプトキャンバスを消します。
        /// </summary>
        public static void ClearScriptCanvas()
        {
            CursorLock(() =>
                {
                    Instance.InnerClearScriptCanvas();
                });
        }

        /// <summary>
        /// すべてのスクリプトキャンバスを消します。
        /// </summary>
        public void InnerClearScriptCanvas()
        {
            foreach (var item in Tab.Items)
            {
                DeleteCommandCanvas(item as TabItem);
            }

            Dispatcher.BeginInvoke(new Action(() =>
                {
                    Tab.Items.Clear();
                }), DispatcherPriority.Background);
        }

        /// <summary>
        /// タブを削除します。
        /// </summary>
        /// <param name="tabItem">タブ</param>
        private void RemoveScriptCanvas(TabItem tabItem)
        {
            DeleteCommandCanvas(tabItem);

            Dispatcher.BeginInvoke(new Action(() =>
                {
                    Tab.Items.Remove(tabItem);
                    Tab.Items.Refresh();
                }), DispatcherPriority.Loaded);
        }

        /// <summary>
        /// スクリプトキャンバスを削除します。
        /// </summary>
        /// <param name="tabItem">タブ</param>
        private void DeleteCommandCanvas(TabItem tabItem)
        {
            var commandCanvas = tabItem.Content as CommandCanvas;
            tabItem.Content = null;
            tabItem.Header = null;
            commandCanvas.ClearWorkCanvas();
            commandCanvas.Dispose();
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
        /// スクリプト実行ボタンの処理です。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExecuteButton_Click(object sender, RoutedEventArgs e)
        {
            object result = CallPublicExecuteEntryPoint(false, EntryPointName.Text.Trim());
            if (result != null)
            {
                ShowResultWindow(result);
            }
        }

        private static void ShowResultWindow(object result)
        {
            var resultWindow = new ResultWindow(result.ToString());
            resultWindow.Owner = OwnerWindow;
            resultWindow.Show();
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
                        CallPublicExecuteEntryPoint(false);
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
            RequestSetTitle();
            UpdateButtonEnable();
        }

        public static void ResetTitle()
        {
            Instance?.RequestSetTitle();
        }

        /// <summary>
        /// タイトルのセットを依頼します。
        /// </summary>
        public void RequestSetTitle()
        {
            if (CurrentScriptTitle is null || CurrentScriptCanvas is null)
            {
                SetTitleFunc?.Invoke($"{Project.ProjectName}");
            }
            else
            {
                if (string.IsNullOrWhiteSpace(CurrentScriptCanvas.OpenFileName))
                {
                    // New? のときは、OpenFileName は空。

                    SetTitleFunc?.Invoke($"{Project.ProjectName} - {CurrentScriptTitle}");
                }
                else
                {
                    SetTitleFunc?.Invoke($"{Project.ProjectName} - {System.IO.Path.GetFileNameWithoutExtension(CurrentScriptCanvas.OpenFileName)}");
                }
            }
        }

        /// <summary>
        /// タブからスクリプトキャンバスを取得します。
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private static CommandCanvas GetCommandCanvas(TabItem item)
        {
            return item.Content as CommandCanvas;
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

            foreach (var item in Tab.Items)
            {
                CommandCanvas commandCanvas = GetCommandCanvas(item as TabItem);
                commandCanvas.WorkCanvas.AddScriptCommandRecent(name);
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

            foreach (var item in Instance.Tab.Items)
            {
                CommandCanvas commandCanvas = GetCommandCanvas(item as TabItem);
                SetTargetWorkCanvasBB(commandCanvas, path, stretch);
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

        private void EntryPointName_TextChanged(object sender, TextChangedEventArgs e)
        {
            EntryPointNamePh.Visibility = (EntryPointName.Text.Trim().Length != 0) ? Visibility.Hidden : Visibility.Visible;
        }

        private void ConvertCSS_Click(object sender, RoutedEventArgs e)
        {
            BuildScriptAndOut();
        }

        /// <summary>
        /// スクリプト実行中パネルを表示します。
        /// </summary>
        public void ShowRunningPanel()
        {
            RunningPanel.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// スクリプト実行中パネルを消します。
        /// </summary>
        public void HideRunningPanel()
        {
            RunningPanel.Visibility = Visibility.Hidden;
        }

        public void Dispose()
        {
            CapyCSS.Language.Instance.Dispose();
            ToolExec.KillProcess();

            GC.SuppressFinalize(this);
        }
    }
}
