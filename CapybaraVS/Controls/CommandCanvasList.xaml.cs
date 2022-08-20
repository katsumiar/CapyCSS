using CapyCSS;
using CapyCSS.Controls.BaseControls;
using CapyCSS.Script;
using CapyCSSattribute;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Channels;
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

        public const string DOTNET = "dotnet";
        public const string CBS_EXT = ".cbs";
        public const string CBSPROJ_EXT = ".cbsproj";
        public static readonly List<Tuple<string, string>> CBSPROJ_FILTER = new List<Tuple<string, string>>
        {
            new Tuple<string, string>("CBS Project Directory", $"*{CBSPROJ_EXT}"),
        };
        public static readonly List<Tuple<string, string>> CBS_FILTER = new List<Tuple<string, string>>
        {
            new Tuple<string, string>("CBS files", "*.cbs"),
        };
        public static readonly List<Tuple<string, string>> DLL_FILTER = new List<Tuple<string, string>>
        {
            new Tuple<string, string>("DLL files", "*.dll"),
        };

        public static string CAPYCSS_WORK_PATH = null;
        public static string CAPYCSS_INFO_PATH = @"CapyCSS.xml";

        public bool IsAutoExecute = false;   // スクリプトの自動実行
        public bool IsAutoExit = false;      // スクリプトの自動実行後自動終了

        public static string ErrorLog = "";

        public Action<string> SetTitleFunc = null;

        public static Action CallClosing = null;

        private List<string> scriptWorkRecentList = new List<string>();

        private static Window ownerWindow = null;

        public static Window OwnerWindow => ownerWindow;

        private static IDictionary<Cursor, int> CursolLockCounter = null;
        private static IEnumerable<Cursor> LockTargetCursors = new List<Cursor>() { Cursors.Wait, Cursors.Hand };

        /// <summary>
        /// カーソル形状を変更します。
        /// </summary>
        /// <param name="cursor">変更するカーソル形状</param>
        public static void SetOwnerCursor(Cursor cursor)
        {
            Debug.Assert(cursor != null);
            if (CursolLockCounter is null)
            {
                // ロックカウンターの初期化

                CursolLockCounter = new Dictionary<Cursor, int>();
                foreach (var node in LockTargetCursors)
                {
                   CursolLockCounter.Add(node, 0);
                }
            }

            if (LockTargetCursors.Contains(cursor))
            {
                if (!(OwnerWindow.Cursor == Cursors.Wait && cursor != Cursors.Wait))
                {
                    OwnerWindow.Cursor = cursor;
                }
                CursolLockCounter[cursor]++;
            }
            else
            {
                OwnerWindow.Cursor = cursor;
            }
        }

        /// <summary>
        /// カーソルをリセットします。
        /// </summary>
        /// <param name="cursor">リセットするカーソル形状</param>
        public static void ResetOwnerCursor(Cursor cursor)
        {
            Debug.Assert(cursor != null);
            if (CursolLockCounter is null)
            {
                return;
            }

            if (LockTargetCursors.Contains(cursor))
            {
                Debug.Assert(CursolLockCounter[cursor] >= 0);
                CursolLockCounter[cursor]--;
                if (CursolLockCounter[cursor] == 0)
                {
                    if (!(OwnerWindow.Cursor == Cursors.Wait && cursor != Cursors.Wait))
                    {
                        OwnerWindow.Cursor = null;
                    }
                }
            }
            else
            {
                OwnerWindow.Cursor = null;
            }
        }

        /// <summary>
        /// カーソル形状を参照します。
        /// </summary>
        /// <returns>カーソル形状</returns>
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

        /// <summary>
        /// カレントのスクリプトキャンバスを参照します。
        /// </summary>
        public CommandCanvas CurrentScriptCanvas
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

        /// <summary>
        /// カレントの低レベルスクリプトキャンバスを参照します。
        /// </summary>
        public BaseWorkCanvas CurrentWorkCanvas
        {
            get
            {
                if (CurrentScriptCanvas is null)
                {
                    return null;
                }
                return CurrentScriptCanvas.ScriptWorkCanvas;
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
                var header = CurrentTabItem.Header as RemovableLabel;
                return header is null ? null : System.IO.Path.GetFileNameWithoutExtension(header.Title);
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

            // ボタンに機能を登録する。
            SetButtonCommand(HelpButton, Command.ShowHelp.Create());
            SetButtonCommand(LoadButton, Command.LoadScript.Create());
            SetButtonCommand(SaveButton, Command.SaveScript.Create());
            SetButtonCommand(AddButton, Command.AddNewScript.Create());
            SetButtonCommand(CommandMenuButton, Command.ShowCommandMenu.Create());
            SetButtonCommand(undo, Command.UnDo.Create());
            SetButtonCommand(redo, Command.ReDo.Create());
            SetButtonCommand(ConvertCSS, Command.ConvertCS.Create());
            SetButtonCommand(ExecuteButton, Command.ExecuteScript.Create());

            Dispatcher.BeginInvoke(new Action(() =>
                {
                    // アイドル状態になってから新規シートを作成する

                    if (reserveLoadCbsFilePath != null)
                    {
                        // 起動読み込みをキックする

                        string ext = Path.GetExtension(reserveLoadCbsFilePath);

                        if (ext == CommandCanvasList.CBS_EXT)
                        {
                            // CBSファイル読み込み

                            AddLoadContents(reserveLoadCbsFilePath);
                        }
                        else if (ext == CommandCanvasList.CBSPROJ_EXT)
                        {
                            // プロジェクトファイル読み込み

                            ProjectExpander.IsExpanded = true;
                            ProjectControl.Instance.LoadProject(reserveLoadCbsFilePath);
                        }

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
        /// <param name="autoLoadFile">起動時読み込みcbsファイル</param>
        /// <param name="isAutoExecute">起動時実行</param>
        /// <param name="isAutoExit">自動終了</param>
        public void Setup(
            Window owner,
            string path,
            Action<string> setTitleFunc = null,
            Action closingFunc = null,
            string autoLoadFile = null,
            bool isAutoExecute = false,
            bool isAutoExit = false)
        {
            ownerWindow = owner;
            OwnerWindow.ForceCursor = true;
            SetTitleFunc = setTitleFunc;

            CAPYCSS_WORK_PATH = path;
            CAPYCSS_INFO_PATH = Path.Combine(CAPYCSS_WORK_PATH, CAPYCSS_INFO_PATH);

            CallClosing = closingFunc;
            SetTitleFunc(null);
            if (autoLoadFile != null)
            {
                SetLoadFile(autoLoadFile);
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
        /// スクリプト追加ボタンを非表示にします。
        /// </summary>
        private void hideAddScriptButton()
        {
            AddButton.Visibility = Visibility.Collapsed;
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
            CommandCanvasList.ResetOwnerCursor(Cursors.Wait);
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
            dispatcherObject.Dispatcher.BeginInvoke(new Action(() =>
                {
                    CursorUnlock();
                }), DispatcherPriority.ApplicationIdle);
        }

        /// <summary>
        /// スクリプトが修正されているか？
        /// </summary>
        /// <returns><see langword="true"/>==修正されている</returns>
        public bool IsModified
        {
            get
            {
                var saveTargets = Tab.Items.Cast<TabItem>()
                                .Where(n => !(n.Content as CommandCanvas).IsInitialPoint)
                                .Select(n => n.Content as CommandCanvas);
                return saveTargets.Count() != 0;
            }
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
        public bool OverwriteSaveXML(CommandCanvas commandCanvas, bool forced = false)
        {
            if (!forced && IsCursorLock())
            {
                return false;
            }

            bool isShowSaveDialog = commandCanvas.OpenFileName == "";
            string path;
            if (isShowSaveDialog)
            {
                path = ShowSaveDialog(CapyCSS.Language.Instance["Help:SYSTEM_SelectSaveScriptFile"], CBS_FILTER, commandCanvas.OpenFileName, true);

                if (path is null)
                {
                    return false;
                }
            }
            else
            {
                path = commandCanvas.OpenFileName;
            }

            commandCanvas.SaveXML(path, forced);
            if (isShowSaveDialog)
            {
                // 正式に名前が決まったのでタブにも反映する

                SetCurrentTabName(commandCanvas.OpenFileName);
            }
            if (!forced)
            {
                commandCanvas.ClearUnDoPoint();
                commandCanvas.RecordUnDoPoint(CapyCSS.Language.Instance["Help:SYSTEM_COMMAND_OverwriteScript"]);
            }
            return true;
        }

        /// <summary>
        /// CBS ファイルを保存します。
        /// </summary>
        public void SaveCbsFile()
        {
            saveCbsFile(CurrentScriptCanvas);
        }

        /// <summary>
        /// CBS ファイルを保存します。
        /// </summary>
        /// <param name="commandCanvas">スクリプトキャンバス</param>
        public void saveCbsFile(CommandCanvas commandCanvas)
        {
            if (IsCursorLock())
            {
                return;
            }

            string path = ShowSaveDialog(CapyCSS.Language.Instance["Help:SYSTEM_SelectSaveScriptFile"], CBS_FILTER, commandCanvas.OpenFileName, true);
            if (path is null)
            {
                return;
            }

            commandCanvas.SaveXML(path);
            SetCurrentTabName(commandCanvas.OpenFileName);
            commandCanvas.ClearUnDoPoint();
            commandCanvas.RecordUnDoPoint(CapyCSS.Language.Instance["Help:SYSTEM_COMMAND_SaveScript"]);
        }

        /// <summary>
        /// 保存用ファイル選択ダイアログを表示します。
        /// </summary>
        /// <param name="filters">フィルター</param>
        /// <param name="addExtention">拡張子の自動付加（filters が１件だけであること）</param>
        /// <param name="currentPath">カレントパス</param>
        /// <returns>保存するCBSファイルのパス</returns>
        public static string ShowSaveDialog(string title, IEnumerable<Tuple<string, string>> filters, string currentPath = null, bool addExtention = false)
        {
            string fileName = null;
            string currentDirectory = null;
            if (currentPath != null && Path.GetExtension(currentPath) != "")
            {
                fileName = System.IO.Path.GetFileNameWithoutExtension(currentPath);
                currentDirectory = System.IO.Path.GetDirectoryName(currentDirectory);
            }
            else
            {
                if (String.IsNullOrWhiteSpace(currentPath))
                {
                    string projectFile = ProjectControl.Instance.ProjectFilePath;

                    if (projectFile != null)
                    {
                        currentDirectory = System.IO.Path.GetDirectoryName(projectFile);
                    }
                    else
                    {
                        currentDirectory = GetSamplePath();
                    }
                }
                else
                {
                    currentDirectory = currentPath;
                }
            }
            using (var dialog = new CommonOpenFileDialog(fileName)
            {
                Title = title,
                InitialDirectory = currentDirectory,
                DefaultDirectory = GetSamplePath(),
                EnsureValidNames = true,
                EnsurePathExists = false,
            })
            {
                foreach (var filter in filters)
                {
                    dialog.Filters.Add(new CommonFileDialogFilter(filter.Item1, filter.Item2));
                }
                if (dialog.ShowDialog() != CommonFileDialogResult.Ok)
                {
                    return null;
                }
                string result = dialog.FileName;
                if (addExtention)
                {
                    // 拡張子の自動付加

                    Debug.Assert(filters.Count() == 1);
                    if (Path.GetExtension(result) == "")
                    {
                        // 拡張子が付いていないのでフィルターの拡張子を参考にして付ける

                        string filterExtention = filters.First().Item2;
                        result += filterExtention.Substring(filterExtention.IndexOf('.'));
                    }
                }
                return result;
            }
        }

        /// <summary>
        /// すべての変更を保存します。
        /// </summary>
        /// <returns>true==保存できた</returns>
        public static bool SaveAllChanges()
        {
            return Instance.saveAllChanges();
        }

        /// <summary>
        /// すべての変更を保存します。
        /// </summary>
        /// <returns>true==保存できた</returns>
        private bool saveAllChanges()
        {
            try
            {
                var saveTargets = Tab.Items.Cast<TabItem>()
                    .Where(n => !(n.Content as CommandCanvas).IsInitialPoint)
                    .Select(n => n.Content as CommandCanvas);
                foreach (var target in saveTargets)
                {
                    if (!OverwriteSaveXML(target, true))
                    {
                        // 途中でキャンセルされた

                        return false;
                    }
                }
                SaveInfo();
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// CBS ファイルを上書き保存します。
        /// </summary>
        public void OverwriteCbsFile(bool forced = false)
        {
            if (forced)
            {
                OverwriteSaveXML(CurrentScriptCanvas, true);
            }
            else
            {
                Dispatcher.BeginInvoke(new Action(() =>
                    {
                        OverwriteSaveXML(CurrentScriptCanvas);
                    }), DispatcherPriority.ApplicationIdle);
            }
        }

        /// <summary>
        /// 読み込み予約cbsファイルです。
        /// </summary>
        private string reserveLoadCbsFilePath = null;

        /// <summary>
        /// ファイル読み込みを予約します。
        /// </summary>
        /// <param name="path">ファイルのパス</param>
        public void SetLoadFile(string path = null)
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
                TabItem tab = FindTabFromPath(path);
                if (tab != null)
                {
                    tab.IsSelected = true;
                    if (!(tab.Content as CommandCanvas).IsInitialPoint)
                    {
                        if (ControlTools.ShowSelectMessage(
                                    CapyCSS.Language.Instance["SYSTEM_ConfirmationReloadScript"],
                                    CapyCSS.Language.Instance["SYSTEM_Confirmation"],
                                    MessageBoxButton.OKCancel) == MessageBoxResult.Cancel)
                        {
                            // 読み直さないのでフォーカスを当てるだけ

                            return;
                        }

                        // 再読み込みのフロー
                    }
                    else
                    {
                        // スクリプトキャンバスと同じファイルでキャンバスは編集されていないのでフォーカスを当てるだけ

                        return;
                    }
                }
                else
                {
                    // 新しいタブで新規に読み込むフロー
                }
            }
            else
            {
                // プロジェクトからの依頼

                if (TryTabForcusByContainsPath(path))
                {
                    return;
                }
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
                    CurrentScriptCanvas.ClearUnDoPoint();
                    CurrentScriptCanvas.RecordUnDoPoint(CapyCSS.Language.Instance["Help:SYSTEM_COMMAND_LoadScript"]);
                    CursorUnlock();
                });
        }

        /// <summary>
        /// ディレクトリ選択用ダイアログを開きます。
        /// </summary>
        /// <returns></returns>
        public static string SelectDirectroyDialog(string initDirectory = null)
        {
            using (var dialog = new CommonOpenFileDialog()
            {
                Title = CapyCSS.Language.Instance["Help:SYSTEM_SelectDirectory"],
                InitialDirectory = initDirectory,
                DefaultDirectory = GetSamplePath(),
                IsFolderPicker = true,
            })
            {
                if (dialog.ShowDialog() != CommonFileDialogResult.Ok)
                {
                    return null;
                }
                return dialog.FileName;
            }
        }

        /// <summary>
        /// 読み込み用ファイル選択ダイアログを表示します。
        /// </summary>
        /// <returns>読み込むCBSファイルのパス</returns>
        public static string ShowLoadDialog(IEnumerable<Tuple<string, string>> filters, string currentDirectory = null)
        {
            using (var dialog = new CommonOpenFileDialog()
            {
                Title = CapyCSS.Language.Instance["Help:SYSTEM_SelectFile"],
                InitialDirectory=currentDirectory,
                DefaultDirectory = GetSamplePath(),
                EnsurePathExists = true,
            })
            {
                foreach (var filter in filters)
                {
                    dialog.Filters.Add(new CommonFileDialogFilter(filter.Item1, filter.Item2));
                }
                if (dialog.ShowDialog() != CommonFileDialogResult.Ok)
                {
                    return null;
                }
                return dialog.FileName;
            }
        }

        private static string _samplePath = null;

        /// <summary>
        /// sample ディレクトリのフルパスを取得します。
        /// </summary>
        /// <returns>sampleディレクトリのフルパス</returns>
        public static string GetSamplePath()
        {
            _samplePath ??= getSamplePath();
            return _samplePath;
        }
        private static string getSamplePath()
        {
            string exexPath = Environment.CommandLine;
            if (exexPath == null)
            {
                return Environment.CurrentDirectory;
            }
            FileInfo fi = new FileInfo(exexPath.Replace("\"", ""));
            string startupPath = fi.Directory.FullName;
            var resultPath = Path.Combine(startupPath, "Sample");
            if (!Directory.Exists(resultPath))
            {
                return Environment.CurrentDirectory;
            }
            return Path.Combine(startupPath, "Sample");
        }

        /// <summary>
        /// カレントタブの名前を変更します。
        /// </summary>
        /// <param name="path">ファイルパス</param>
        public void SetCurrentTabName(string path)
        {
            var label = CurrentTabItem.Header as RemovableLabel;
            label.Title = path;
            updateTitle();
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

                ControlTools.ShowErrorMessage(CapyCSS.Language.Instance["Help:SYSTEM_Error_Exists"]);
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

        private bool isScriptRunningMask = true;
        public bool IsScriptRunningMask => isScriptRunningMask;

        /// <summary>
        /// 実行ボタンの有効/無効を制御します。
        /// </summary>
        /// <param name="enable">スクリプト実行時 == false</param>
        public void ScriptRunningMask(bool enable)
        {
            isScriptRunningMask = enable;
            if (isScriptRunningMask)
            {
                // スクリプト実行終了時は、状態をチェックするトリガーが無いので、強制的に更新する

                CommandManager.InvalidateRequerySuggested();
            }
        }

        /// <summary>
        /// ボタンにコマンドを登録します。
        /// </summary>
        /// <param name="button">対象のボタン</param>
        /// <param name="menuCommand">コマンド</param>
        private void SetButtonCommand(Button button, IMenuCommand menuCommand)
        {
            //button.Content = menuCommand.Name;
            button.ToolTip = menuCommand.HintText();
            button.Command = menuCommand;
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
            AddAllExecuteEntryPointEnable(ScriptRunningMask);
            if (owner is null)
            {
                PublicExecuteEntryPointList.Clear();
            }
            else
            {
                PublicExecuteEntryPointList.RemoveAll(s => s.owner == owner);
            }
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
        }

        /// <summary>
        /// スクリプトのエントリーポイントがカレントスクリプトに含まれているかを判定します。
        /// </summary>
        /// <returns></returns>
        public bool IsEntryPointsContainsCurrentScriptCanvas()
        {
            return PublicExecuteEntryPointList.Any(s => s.owner == CurrentScriptCanvas);
        }

        /// <summary>
        /// スクリプト実行用公開エントリーポイントリストからエントリーポイントを削除します。
        /// </summary>
        public void RemovePublicExecuteEntryPoint(NodeFunction func)
        {
            PublicExecuteEntryPointList.RemoveAll(s => s.function == func);
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
        /// スクリプトキャンバスが空であることを確認します。
        /// </summary>
        public bool IsEmptyScriptCanvas => Tab.Items.Count == 0;

        /// <summary>
        /// すべてのスクリプトキャンバスを消します。
        /// </summary>
        public static void ClearScriptCanvas(bool forced = false)
        {
            if (forced)
            {
                Instance.InnerClearScriptCanvas();
            }
            else
            {
                CursorLock(() =>
                    {
                        Instance.InnerClearScriptCanvas();
                    });
            }
        }

        /// <summary>
        /// すべてのスクリプトキャンバスを消します。
        /// </summary>
        public void InnerClearScriptCanvas()
        {
            bool IsAnyInitialPoint = Tab.Items.Cast<TabItem>().Any(n => !(n.Content as CommandCanvas).IsInitialPoint);
            if (IsAnyInitialPoint &&
                ControlTools.ShowSelectMessage(
                        CapyCSS.Language.Instance["SYSTEM_ConfirmationDestructionScript"],
                        CapyCSS.Language.Instance["SYSTEM_Confirmation"],
                        MessageBoxButton.OKCancel) == MessageBoxResult.Cancel)
            {
                // 削除を取りやめ

                return;
            }

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
            if (!CurrentScriptCanvas.IsInitialPoint &&
                ControlTools.ShowSelectMessage(
                        CapyCSS.Language.Instance["SYSTEM_ConfirmationDestructionScript"],
                        CapyCSS.Language.Instance["SYSTEM_Confirmation"],
                        MessageBoxButton.OKCancel) == MessageBoxResult.Cancel)
            {
                // 削除を取りやめ

                return;
            }

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
        /// スクリプト実行ボタンの処理です。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExecuteButton_Click(object sender, RoutedEventArgs e)
        {
            ExecuteScriptFromEntryPoint();
        }

        /// <summary>
        /// エントリーポイントからスクリプトを起動します。
        /// </summary>
        public void ExecuteScriptFromEntryPoint()
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
                        Command.OverwriteScript.TryExecute();
                        e.Handled = true;
                        break;

                    case Key.O: // 読み込み
                        Command.LoadScript.TryExecute();
                        e.Handled = true;
                        break;

                    case Key.N: // 新規項目の追加
                        Command.AddNewScript.TryExecute();
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
                        Command.ExecuteScript.TryExecute();
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
            updateTitle();
        }

        /// <summary>
        /// タイトルを更新します。
        /// </summary>
        public static void UpdateTitle()
        {
            Instance?.updateTitle();
        }

        /// <summary>
        /// タイトルのセットを依頼します。
        /// </summary>
        private void updateTitle()
        {
            if (CurrentScriptTitle is null || CurrentScriptCanvas is null)
            {
                SetTitleFunc?.Invoke($"{Project.ProjectName}");
            }
            else
            {
                string currentScriptChangeState = "";
                string projectChangeState = "";
                if (!CurrentScriptCanvas.IsInitialPoint)
                {
                    currentScriptChangeState = "*";
                }
                if (Project.IsModified)
                {
                    projectChangeState = "*";
                }
                if (string.IsNullOrWhiteSpace(CurrentScriptCanvas.OpenFileName))
                {
                    // New? のときは、OpenFileName は空。

                    SetTitleFunc?.Invoke($"{Project.ProjectName}{projectChangeState} - {CurrentScriptTitle}{currentScriptChangeState}");
                }
                else
                {
                    SetTitleFunc?.Invoke($"{Project.ProjectName}{projectChangeState} - {System.IO.Path.GetFileNameWithoutExtension(CurrentScriptCanvas.OpenFileName)}{currentScriptChangeState}");
                }
                (CurrentTabItem.Header as RemovableLabel).ChangedFlag = !CurrentScriptCanvas.IsInitialPoint;

                // UnDo/ReDo ボタンの状態反映
                undo.IsEnabled = CurrentScriptCanvas != null && CurrentScriptCanvas.CanUnDo;
                redo.IsEnabled = CurrentScriptCanvas != null && CurrentScriptCanvas.CanReDo;
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

        /// <summary>
        /// ヘルプウィンドウを表示します。
        /// </summary>
        public static void ShowHelpWindow()
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
        public static void ShowSystemErrorLog(string title = null)
        {
            if (ErrorLog.Length != 0)
            {
                if (title is null)
                {
                    title = $"Error";
                }
                else
                {
                    title = $"Error - {title}";
                }
                OutputWindow outputWindow = OutputWindow.CreateWindow(title);
                outputWindow.AddBindText = ErrorLog;
                ErrorLog = "";
            }
        }

        private void EntryPointName_TextChanged(object sender, TextChangedEventArgs e)
        {
            EntryPointNamePh.Visibility = (EntryPointName.Text.Trim().Length != 0) ? Visibility.Hidden : Visibility.Visible;
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


        [ScriptMethod]
        public static void SetExitCode(int code)
        {
            Environment.Exit(code);
        }

        public void Dispose()
        {
            var saveTargets = Tab.Items.Cast<TabItem>()
                                        .Where(n => !(n.Content as CommandCanvas).IsInitialPoint)
                                        .Select(n => n.Content as CommandCanvas);
            if (saveTargets.Count() != 0 && ControlTools.ShowSelectMessage(
                        CapyCSS.Language.Instance["SYSTEM_SaveConfirmation"],
                        CapyCSS.Language.Instance["SYSTEM_Confirmation"],
                        MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                foreach (var commandCanvas in saveTargets)
                {
                    saveCbsFile(commandCanvas);
                }
            }

            Project.Dispose();
            CapyCSS.Language.Instance.Dispose();
            ToolExec.KillProcess();

            GC.SuppressFinalize(this);
        }
    }
}
