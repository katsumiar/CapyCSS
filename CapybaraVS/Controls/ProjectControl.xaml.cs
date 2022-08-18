using CapyCSS.Command;
using CapyCSS.Controls.BaseControls;
using CapyCSS.Script;
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
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

namespace CapyCSS.Controls
{
    /// <summary>
    /// ProjectControl.xaml の相互作用ロジック
    /// </summary>
    public partial class ProjectControl 
        : UserControl
        , IDisposable
    {
        #region XML定義
        [XmlRoot(nameof(ProjectControl))]
        public class _AssetXML<OwnerClass> : IDisposable
            where OwnerClass : ProjectControl
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
                    string basePath = System.IO.Path.GetDirectoryName(self.projectFilePath);
                    if (CbsPathList != null)
                    {
                        self.cbsGroup.Child.Clear();
                        foreach (var path in CbsPathList)
                        {
                            string tempPath = System.IO.Path.GetFullPath(path, basePath);
                            self.AddCbsFile(tempPath);
                        }
                    }

                    if (DllList != null)
                    {
                        self.dllGroup.Child.Clear();
                        foreach (var path in DllList)
                        {
                            self.AddDllFile(path);
                        }
                    }

                    // 次回の為の初期化
                    self.AssetXML = new _AssetXML<ProjectControl>(self);
                };
            }
            public _AssetXML(OwnerClass self)
            {
                WriteAction = () =>
                {
                    string basePath = System.IO.Path.GetDirectoryName(self.projectFilePath);
                    CbsPathList = new List<string>();
                    foreach (var item in self.cbsGroup.Child)
                    {
                        string tempPath = System.IO.Path.GetRelativePath(basePath, item.HintText);
                        CbsPathList.Add(tempPath);
                    }

                    DllList = new List<string>();
                    foreach (var item in self.dllGroup.Child)
                    {
                        DllList.Add(item.HintText);
                    }
                };
            }
            #region 固有定義
            /// <summary>
            /// CBSファイルパスリストです。
            /// ※プロジェクトディレクトリからの相対パスになります。
            /// </summary>
            public List<string> CbsPathList { get; set; } = null;

            /// <summary>
            /// DLLリストです。
            /// ※プロジェクトディレクトリ直下である必要がある為、ファイル名のみ管理します。
            /// </summary>
            public List<string> DllList { get; set; } = null;
            #endregion

            protected virtual void Dispose(bool disposing)
            {
                if (!disposedValue)
                {
                    if (disposing)
                    {
                        WriteAction = null;
                        ReadAction = null;

                        // 以下、固有定義開放
                        CbsPathList = null;
                    }
                    disposedValue = true;
                }
            }

            public void Dispose()
            {
                Dispose(disposing: true);
                GC.SuppressFinalize(this);
            }
        }
        public _AssetXML<ProjectControl> AssetXML { get; set; } = null;
        #endregion

        #region ProjectName 添付プロパティ実装
        private static ImplementDependencyProperty<ProjectControl, string> impProjectName =
            new ImplementDependencyProperty<ProjectControl, string>(
                nameof(ProjectName),
                (self, getValue) =>
                {
                    var value = getValue(self);
                    if (String.IsNullOrWhiteSpace(value))
                    {
                        self.ProjectName = self.loadProjectName;
                        return;
                    }
                    if (value != self.loadProjectName)
                    {
                        self.RenameProject(value);
                    }
                });

        public static readonly DependencyProperty ProjectNameProperty = impProjectName.Regist(INIT_PROJECT_NAME);

        public string ProjectName
        {
            get { return impProjectName.GetValue(this); }
            set { impProjectName.SetValue(this, value); }
        }
        #endregion

        #region ChangedFlag 添付プロパティ実装
        private static ImplementDependencyProperty<ProjectControl, bool> impChangedFlag =
            new ImplementDependencyProperty<ProjectControl, bool>(
                nameof(ChangedFlag),
                (self, getValue) =>
                {
                    bool value = getValue(self);
                    self.ChangedState.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
                    CommandCanvasList.UpdateTitle();
                });

        public static readonly DependencyProperty ChangedFlagProperty = impChangedFlag.Regist(false);

        public bool ChangedFlag
        {
            get { return impChangedFlag.GetValue(this); }
            set { impChangedFlag.SetValue(this, value); }
        }
        #endregion

        public const string INIT_PROJECT_NAME = "(no name)";

        private string loadProjectName = "";

        private TreeMenuNode commandGroup = new TreeMenuNode(TreeMenuNode.NodeType.GROUP, "Command");
        private TreeMenuNode cbsGroup = new TreeMenuNode(TreeMenuNode.NodeType.GROUP, "CBS Files");
        private TreeMenuNode dllGroup = new TreeMenuNode(TreeMenuNode.NodeType.GROUP, "DLL Files");

        public static ProjectControl Instance => instance;
        private static ProjectControl instance = null;

        /// <summary>
        /// プロジェクトファイル(.cbs)パスです。
        /// </summary>
        public string ProjectFilePath => projectFilePath;

        private string projectFilePath = CommandCanvasList.GetSamplePath();

        /// <summary>
        /// プロジェクトのルートディレクトリです。
        /// </summary>
        public string ParentProjectDirectory => parentProjectDirectory;

        private string parentProjectDirectory = CommandCanvasList.GetSamplePath();

        public ProjectControl()
        {
            InitializeComponent();
            AssetXML = new _AssetXML<ProjectControl>(this);
            DataContext = this;

            ProjectTree.AssetTreeData = new ObservableCollection<TreeMenuNode>();
            MakeCommandMenu(ProjectTree);

            instance = this;
        }

        /// <summary>
        /// プロジェクトを開いているかを確認します。
        /// </summary>
        public bool IsOpenProject => ProjectName != INIT_PROJECT_NAME;

        /// <summary>
        /// コマンドリストを作成します。
        /// </summary>
        /// <param name="treeViewCommand">登録するツリー</param>
        private void MakeCommandMenu(TreeViewCommand treeViewCommand)
        {
            {
                // コマンドを追加

                commandGroup.AddChild(new TreeMenuNode(Command.NewProject.Create()));
                commandGroup.AddChild(new TreeMenuNode(Command.LoadProject.Create()));
                commandGroup.AddChild(new TreeMenuNode(Command.SaveProject.Create()));
                commandGroup.AddChild(new TreeMenuNode(Command.AddNewCbsFile.Create()));
                commandGroup.AddChild(new TreeMenuNode(Command.AddCbsFile.Create()));
                commandGroup.AddChild(new TreeMenuNode(Command.ImportDLL.Create()));
                commandGroup.IsExpanded = true;
                treeViewCommand.AssetTreeData.Add(commandGroup);
            }
            treeViewCommand.AssetTreeData.Add(dllGroup);
            treeViewCommand.AssetTreeData.Add(cbsGroup);
            UpdateCommandEnable();
        }

        /// <summary>
        /// コマンドの有効状態を反映します。
        /// </summary>
        private void UpdateCommandEnable()
        {
            ProjectTree.RefreshItem();
        }

        /// <summary>
        /// プロジェクトをクリアします。
        /// </summary>
        public bool ClearProject(bool forced = false)
        {
            if (!forced && CommandCanvasList.IsCursorLock())
            {
                return false;
            }

            bool isModified = ChangedFlag || CommandCanvasList.Instance.IsModified;
            if (!isModified)
            {
                return true;
            }
            if (isModified && ControlTools.ShowSelectMessage(
                        CapyCSS.Language.Instance["SYSTEM_ConfirmationDelete"],
                        CapyCSS.Language.Instance["SYSTEM_Confirmation"],
                        MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {
                ProjectName = INIT_PROJECT_NAME;
                dllGroup.ClearChild();
                cbsGroup.ClearChild();
                CommandCanvasList.ClearScriptCanvas();
                UpdateCommandEnable();
                ChangedFlag = false;
                return true;
            }
            return false;
        }

        /// <summary>
        /// プロジェクト情報をセットします。
        /// </summary>
        /// <param name="path">プロジェクトファイルのパス</param>
        private void SetProjectName(string path)
        {
            Directory.SetCurrentDirectory(System.IO.Path.GetDirectoryName(path));
            loadProjectName = System.IO.Path.GetFileNameWithoutExtension(path);
            ProjectName = loadProjectName;
            projectFilePath = path;
            CommandCanvasList.UpdateTitle();
            ChangedFlag = true;
        }

        /// <summary>
        /// 新規のプロジェクトを作成します。
        /// </summary>
        public void NewProject()
        {
            if (CommandCanvasList.IsCursorLock())
            {
                return;
            }

            if (!ClearProject(true))
            {
                return;
            }

            string path;

            var dialog = CreateProject.Create();
            dialog.ProjectPath = parentProjectDirectory;
            var result = dialog.ShowDialog();
            if (result.HasValue && result.Value)
            {
                path = System.IO.Path.Combine(dialog.ProjectPath, dialog.ProjectFile);
                var projectDirectory = Directory.CreateDirectory(path);
                if (!projectDirectory.Exists)
                {
                    // プロジェクト用ディレクトリ作成に失敗

                    return;
                }
                path = System.IO.Path.Combine(path, dialog.ProjectFile + CommandCanvasList.CBSPROJ_EXT);
            }
            else
            {
                // キャンセル

                return;
            }

            if (File.Exists(path))
            {
                ControlTools.ShowErrorMessage(CapyCSS.Language.Instance["Help:SYSTEM_Error_Exists"]);
                return;
            }

            SetProjectName(path);
            UpdateCommandEnable();

            // デフォルトのDLLとして登録する
            foreach (var dllPath in CbSTUtils.AutoImportDllList)
            {
                AddDllFile(dllPath);
            }

            _SaveProject();
        }

        /// <summary>
        /// プロジェクトファイルを読み込みます。
        /// </summary>
        public void LoadProject()
        {
            if (CommandCanvasList.IsCursorLock())
            {
                return;
            }

            if (!ClearProject(true))
            {
                return;
            }

            string path = CommandCanvasList.ShowLoadDialog(CommandCanvasList.CBSPROJ_FILTER, parentProjectDirectory);
            if (path is null)
            {
                return;
            }

            {
                // プロジェクトディレクトリ階層をプロジェクト用カレントディレクトリとして保存する
                // ※プロジェクトのディレクトリの一つ上の階層がプロジェクトディレクトリになる。
                DirectoryInfo directoryInfo = new DirectoryInfo(System.IO.Path.GetDirectoryName(path));
                parentProjectDirectory = directoryInfo.Parent.FullName;
            }

            if (IsOpenProject)
            {
                // 新しい実行ファイルでプロジェクトを読み込む

                ProcessStartInfo pInfo = new ProcessStartInfo();
                pInfo.FileName = CommandCanvasList.DOTNET;
                pInfo.Arguments = Assembly.GetEntryAssembly().Location + " " + path;
                Process.Start(pInfo);
            }
            else
            {
                // プロジェクトを読み込む

                CommandCanvasList.ClearScriptCanvas();
                LoadProject(path);
            }
        }

        /// <summary>
        /// プロジェクトファイルを読み込みます。
        /// ※起動時に引数指定している場合に呼ばれます。
        /// </summary>
        /// <param name="path"></param>
        public void LoadProject(string path)
        {
            SetProjectName(path);   // 相対ディレクトリの基準がセットされるので readProjectFile の前に処理する
            readProjectFile(path);
            UpdateCommandEnable();
            ChangedFlag = false;
        }

        private void readProjectFile(string path)
        {
            using (StreamReader reader = new StreamReader(path))
            {
                try
                {
                    XmlSerializer serializer = new XmlSerializer(AssetXML.GetType());

                    XmlDocument doc = new XmlDocument();
                    doc.PreserveWhitespace = true;
                    doc.Load(reader);
                    XmlNodeReader nodeReader = new XmlNodeReader(doc.DocumentElement);

                    object data = (_AssetXML<ProjectControl>)serializer.Deserialize(nodeReader);
                    AssetXML = (_AssetXML<ProjectControl>)data;
                    AssetXML.ReadAction(this);

                    Console.WriteLine($"Loaded...\"{path}\"");

                    ChangedFlag = false;
                }
                catch (Exception ex)
                {
                    ControlTools.ShowErrorMessage(ex.Message);
                }
            }
        }

        /// <summary>
        /// プロジェクトファイルを保存します。
        /// </summary>
        public void SaveProject()
        {
            if (CommandCanvasList.IsCursorLock())
            {
                return;
            }

            _SaveProject();
        }

        private void _SaveProject()
        {
            Debug.Assert(projectFilePath != null);
            string path = projectFilePath;
            if (path is null)
            {
                return;
            }

            SetProjectName(path);   // 相対ディレクトリの基準がセットされるので保存の前に処理する

            try
            {
                var writer = new StringWriter();
                var serializer = new XmlSerializer(AssetXML.GetType());
                var namespaces = new XmlSerializerNamespaces();
                namespaces.Add(string.Empty, string.Empty);
                AssetXML.WriteAction();
                serializer.Serialize(writer, AssetXML, namespaces);
                using (StreamWriter swriter = new StreamWriter(path, false))
                {
                    swriter.WriteLine(writer.ToString());
                }

                Console.WriteLine($"Saved...\"{path}\"");

                ChangedFlag = false;
            }
            catch (Exception ex)
            {
                ControlTools.ShowErrorMessage(ex.Message);
            }
            UpdateCommandEnable();
        }

        /// <summary>
        /// 新しいcbsファイルをプロジェクトに追加します。
        /// </summary>
        public void AddNewCbsFile()
        {
            // プロジェクトディレクトリ（カレントディレクトリ）にファイルを作成する。
            string path = CommandCanvasList.ShowSaveDialog(CommandCanvasList.CBS_FILTER, Environment.CurrentDirectory, true);
            if (path is null)
            {
                return;
            }

            if (File.Exists(path) || ContainsCBS(path))
            {
                ControlTools.ShowErrorMessage(CapyCSS.Language.Instance["Help:SYSTEM_Error_Exists"]);
                return;
            }

            if (CommandCanvasList.Instance.AddNewContents(path))
            {
                CommandCanvasList.Instance.OverwriteCbsFile(true /* forced */);
                AddCbsFile(path);
                ChangedFlag = true;
            }
            UpdateCommandEnable();
        }

        /// <summary>
        /// cbsファイルをプロジェクトに追加します。
        /// </summary>
        public void AddCbsFile()
        {
            // プロジェクトディレクトリ（カレントディレクトリ）をカレントにファイルを探す。
            string path = CommandCanvasList.ShowLoadDialog(CommandCanvasList.CBS_FILTER, Environment.CurrentDirectory);
            if (path is null)
            {
                return;
            }

            AddCbsFile(path);
            ChangedFlag = true;
        }

        /// <summary>
        /// Cbsファイルをプロジェクトに追加します。
        /// </summary>
        /// <param name="path"></param>
        public void AddCbsFile(string path)
        {
            if (ContainsCBS(path))
            {
                // 登録済み

                return;
            }

            var node = new TreeMenuNode(
                TreeMenuNode.NodeType.NORMAL,
                System.IO.Path.GetFileNameWithoutExtension(path),
                () => path,
                CreateImmediateExecutionCanvasCommand(() => OpenScript(path))
                );
            node.DeleteClickCommand = new TreeMenuNodeCommand((a) =>
            {
                if (ControlTools.ShowSelectMessage(
                        CapyCSS.Language.Instance["SYSTEM_ConfirmationRemoveScript"],
                        CapyCSS.Language.Instance["SYSTEM_Confirmation"],
                        MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                {
                    CommandCanvasList.Instance?.RemoveScriptCanvas(path);
                    cbsGroup.Child.Remove(node);
                    ChangedFlag = true;
                }
            });
            if (!File.Exists(path))
            {
                node.IsEnabled = false;
            }
            cbsGroup.AddChild(node);
            cbsGroup.IsExpanded = true;
        }

        /// <summary>
        /// cbs一覧に登録されているか確認します。
        /// </summary>
        /// <param name="path">cbsのパス</param>
        /// <returns>true==存在している</returns>
        private bool ContainsCBS(string path)
        {
            return cbsGroup.Child.Any(c => c.HintText == path);
        }

        /// <summary>
        /// cbsファイルを読み込みます。
        /// </summary>
        /// <param name="path">cbsファイルパス</param>
        private void OpenScript(string path)
        {
            CommandCanvasList.Instance?.AddLoadContents(path);
        }

        /// <summary>
        /// dllファイルをプロジェクトに追加します。
        /// </summary>
        public void AddDllFile()
        {
            // プロジェクトディレクトリ（カレントディレクトリ）をカレントにファイルを探す。
            string path = CommandCanvasList.ShowLoadDialog(CommandCanvasList.DLL_FILTER, Environment.CurrentDirectory);
            if (path is null)
            {
                return;
            }

            AddDllFile(path);
            ChangedFlag = true;
        }

        /// <summary>
        /// dllファイルをプロジェクトに追加します。
        /// </summary>
        /// <param name="path">dllファイルパス</param>
        public void AddDllFile(string path)
        {
            string dllFileName = System.IO.Path.GetFileName(path);
            string localDir = System.IO.Path.GetDirectoryName(ProjectFilePath);
            string localPath = System.IO.Path.Combine(localDir, dllFileName);

            TreeMenuNode node = null;
            if (dllGroup != null && dllGroup.Child.Count > 0)
            {
                node = dllGroup.Child.First(c => c.HintText == localPath);
            }
            if (node is null)
            {
                // 未登録

                node = new TreeMenuNode(
                    TreeMenuNode.NodeType.NORMAL,
                    System.IO.Path.GetFileNameWithoutExtension(localPath),  // Item Name
                    () => localPath                                         // Item Hint
                    );
                node.DeleteClickCommand = new TreeMenuNodeCommand((a) =>
                {
                    if (ControlTools.ShowSelectMessage(
                            CapyCSS.Language.Instance["SYSTEM_ConfirmationRemoveScript"],
                            CapyCSS.Language.Instance["SYSTEM_Confirmation"],
                            MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                    {
                        //File.Delete(localPath); 削除はしない
                        dllGroup.Child.Remove(node);
                        ChangedFlag = true;
                    }
                });
                dllGroup.AddChild(node);
                dllGroup.IsExpanded = true;
            }

            if (!File.Exists(path))
            {
                // インポート元のDLLファイルが見つからない

                node.IsEnabled = false;
            }
            else
            {
                if (!File.Exists(localPath))
                {
                    // インポート先にDLLファイルが見つからないのでコピーする

                    File.Copy(path, localPath);
                }

                // DLLファイルを取り込む
                Assembly.LoadFrom(dllFileName);
            }
        }

        /// <summary>
        /// dll一覧に登録されているか確認します。
        /// </summary>
        /// <param name="path">cbsのパス</param>
        /// <returns>true==存在している</returns>
        private bool ContainsDll(string path)
        {
            return dllGroup.Child.Any(c => c.HintText == path);
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
                    if (CommandCanvasList.IsCursorLock())
                    {
                        return; // 処理中は禁止
                    }

                    action?.Invoke();
                }
            );
        }

        /// <summary>
        /// プロジェクトファイル名を変更します。
        /// </summary>
        /// <param name="name">新しいプロジェクトファイル名</param>
        private void RenameProject(string name)
        {
            if (!IsOpenProject)
            {
                if (ProjectName != INIT_PROJECT_NAME)
                {
                    ProjectName = loadProjectName;
                }
                return;
            }

            try
            {
                string oldDir = System.IO.Path.GetDirectoryName(projectFilePath);
                if (Directory.Exists(oldDir))
                {
                    DirectoryInfo directoryInfo = new DirectoryInfo(oldDir);
                    string newDir = System.IO.Path.Combine(directoryInfo.Parent.FullName, name);

                    // カレントディレクトリの設定が残っているとディレクトリ名を変更できないので、一先ず一つ上の階層に設定する。
                    Directory.SetCurrentDirectory(directoryInfo.Parent.FullName);

                    // プロジェクトディレクトリ名を変更
                    Directory.Move(oldDir, newDir);

                    // プロジェクトファイル名を変更
                    string ext = System.IO.Path.GetExtension(projectFilePath);
                    string oldProjectFileName = System.IO.Path.Combine(newDir, System.IO.Path.GetFileName(projectFilePath));
                    string newProjectFileName = System.IO.Path.Combine(newDir, name + ext);
                    File.Move(oldProjectFileName, newProjectFileName);

                    // 新しいプロジェクト名を反映
                    LoadProject(newProjectFileName);
                }
            }
            catch (Exception ex)
            {
                ControlTools.ShowErrorMessage(ex.Message);
                ProjectName = loadProjectName;
            }
            UpdateCommandEnable();
        }

        public void Dispose()
        {
            if (ChangedFlag && ControlTools.ShowSelectMessage(
                        CapyCSS.Language.Instance["SYSTEM_SaveProjectConfirmation"],
                        CapyCSS.Language.Instance["SYSTEM_Confirmation"],
                        MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                SaveProject();
            }
        }
    }
}
