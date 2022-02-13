using CapyCSS;
using CapyCSS.Controls.BaseControls;
using CapyCSS.Script;
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

        public static void SetOwnerCursor(Cursor cursor)
        {
            OwnerWindow.Cursor = cursor;
        }

        public static Cursor GetOwnerCursor()
        {
            return OwnerWindow.Cursor;
        }

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

            // Consoleの出力先を変更する
            Console.SetOut(new SystemTextWriter());

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
                CurrentTabItem.Header = System.IO.Path.GetFileNameWithoutExtension(CurrentScriptCanvas.OpenFileName);
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
            ExecuteAllButton.IsEnabled = PublicExecuteEntryPointList.Count != 0;
            LoadButton.IsEnabled = Tab.Items.Count != 0;
            SaveButton.IsEnabled = Tab.Items.Count != 0;
            DeleteButton.IsEnabled = Tab.Items.Count != 0;
            CommandMenuButton.IsEnabled = Tab.Items.Count != 0;
        }

        /// <summary>
        /// スクリプト実行用公開エントリーポイントリストからエントリーポイントを削除します。
        /// </summary>
        public void RemovePublicExecuteEntryPoint(NodeFunction func)
        {
            PublicExecuteEntryPointList.RemoveAll(s => s.function == func);
            UpdateButtonEnable();
        }

        [ScriptMethod]
        /// <summary>
        /// エントリーポイントリストからエントリーポイントをまとめて順次呼び出しします。
        /// ただし、スクリプトが返し値を返した場合は、そこで終了します。
        /// エントリーポイント名を指定した場合は、一致したエントリーポイントのみ実行します。
        /// スクリプト側にエントリーポイント名が設定されている場合は、エントリーポイント名が一致する必要があります。
        /// ※スクリプトからの呼び出し用です。
        /// </summary>
        /// <param name="entryPointName">エントリーポイント名</param>
        /// <returns>スクリプトの返し値</returns>
        public static object CallEntryPoint(string entryPointName = null)
        {
            return Instance.CallPublicExecuteEntryPoint(null, true, entryPointName);
        }

        [ScriptMethod]
        /// <summary>
        /// 所属するスクリプトキャンバスのエントリーポイントリストからエントリーポイントをまとめて順次呼び出しします。
        /// ただし、スクリプトが返し値を返した場合は、そこで終了します。
        /// エントリーポイント名を指定した場合は、一致したエントリーポイントのみ実行します。
        /// スクリプト側にエントリーポイント名が設定されている場合は、エントリーポイント名が一致する必要があります。
        /// ※スクリプトからの呼び出し用です。
        /// </summary>
        /// <param name="entryPointName">エントリーポイント名</param>
        /// <returns>スクリプトの返し値</returns>
        public static object CallCurrentWorkEntryPoint(string entryPointName = null)
        {
            return Instance.CallPublicExecuteEntryPoint(Instance.CurrentScriptCanvas, true, entryPointName);
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
        /// エントリーポイントリストからエントリーポイントをまとめて順次呼び出しします。
        /// ただし、スクリプトが返し値を返した場合は、そこで終了します。
        /// エントリーポイント名を指定した場合は、一致したエントリーポイントのみ実行します。
        /// スクリプト側にエントリーポイント名が設定されている場合は、エントリーポイント名が一致する必要があります。
        /// </summary>
        /// <param name="owner">対象インスタンスを限定</param>
        /// <param name="entryPointName">エントリーポイント名</param>
        /// <returns>スクリプトの返し値</returns>
        public object CallPublicExecuteEntryPoint(object owner, bool fromScript, string entryPointName = null)
        {
            var result = _CallPublicExecuteEntryPoint(owner, fromScript, entryPointName);
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
        private object _CallPublicExecuteEntryPoint(object owner, bool fromScript, string entryPointName = null)
        {
            if (!fromScript && CommandCanvasList.GetOwnerCursor() == Cursors.Wait)
                return null;

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
            else
            {
                entryPointName = "";
            }

            var entryPoints = PublicExecuteEntryPointList.Where(n => n.function != null);
            if (owner != null)
            {
                // オーナー限定

                entryPoints = PublicExecuteEntryPointList.Where(n => n.owner == owner);
            }
            // １件のみ実行
            if (!entryPoints.Any(n =>
            {
                result = n.function.Invoke(fromScript, entryPointName);
                return result != null;
            }))
            {
                if (entryPointName != null)
                {
                    var errorMessage = $"\"{entryPointName}\" entry point not found!";
                    if (owner != null)
                    {
                        // CommandCanvas から呼ばれた

                        Console.WriteLine(nameof(CommandCanvas) + $": {errorMessage}");
                    }
                    else
                    {
                        // CallEntryPoint から呼ばれた

                        throw new Exception(errorMessage);
                    } 
                }
            }

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
        public string BuildScript(object owner)
        {
            string result = null;
            IEnumerable<EntryPoint> entryPoints = null;
            if (owner != null)
            {
                // オーナー限定

                // TODO ここはフィルタリングできていない様なので原因を調べる
                entryPoints = PublicExecuteEntryPointList.FindAll(n => n.function != null && n.owner == owner);
            }
            else
            {
                entryPoints = PublicExecuteEntryPointList.Where(n => n.function != null);
            }
            var completionList = new List<CommandCanvas>();
            if (entryPoints.Count() > 0)
            {
                foreach (var entryPoint in entryPoints)
                {
                    if (entryPoint.owner is CommandCanvas commandCanvas && !completionList.Contains(commandCanvas))
                    {
                        BuildScriptInfo? scr = commandCanvas.WorkStack.RequestBuildScript();
                        if (scr.HasValue)
                        {
                            result += scr.Value.BuildScript(null);
                            completionList.Add(commandCanvas);
                        }
                    }
                    var script = entryPoint.function.Invoke(false, ":*");
                    if (script != null)
                    {
                        result += script;
                    }
                }
                result += $"{nameof(CommandCanvasList.CallEntryPoint)} (\"{BuildScriptInfo.DEFAULT_ENTRY_POINT_LABEL_NAME}\");";
            }
            return result;
        }

        /// <summary>
        /// スクリプトからc#を構築しウインドウに出力します。
        /// </summary>
        /// <param name="owner">オーナー</param>
        public void BuildScriptAndOut(object owner)
        {
            string title = "(owner)";
            if (owner is null)
            {
                title = CurrentTabItem.Header.ToString();
            }
            string code = BuildScript(CurrentTabItem.Content);
            if (!string.IsNullOrEmpty(code))
            {
                OutputWindow.CreateWindow(title).AddBindText = BuildScript(owner);
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
            object result = CallPublicExecuteEntryPoint(null, false, EntryPointName.Text.Trim());
            if (result != null)
            {
                ShowResultWindow(result);
            }
        }

        /// <summary>
        /// スクリプト実行ボタンの処理です。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExecuteButton_Click(object sender, RoutedEventArgs e)
        {
            object result = CallPublicExecuteEntryPoint(CurrentScriptCanvas, false, EntryPointName.Text.Trim());
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

                    case Key.F5:    // 全スクリプト実行
                        CallPublicExecuteEntryPoint(null, false, null);
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
                        CallPublicExecuteEntryPoint(CurrentScriptCanvas, false);
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
            CapyCSS.Language.Instance.Dispose();
            ToolExec.KillProcess();

            GC.SuppressFinalize(this);
        }

        private void EntryPointName_TextChanged(object sender, TextChangedEventArgs e)
        {
            EntryPointNamePh.Visibility = (EntryPointName.Text.Trim().Length != 0) ? Visibility.Hidden : Visibility.Visible;
        }
    }
}
