using CapyCSS.Controls.BaseControls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
                    string basePath = System.IO.Path.GetDirectoryName(self.ProjectFilePath);
                    if (CbsPathList != null)
                    {
                        self.cbsGroup.Child.Clear();
                        foreach (var path in CbsPathList)
                        {
                            string tempPath = System.IO.Path.GetFullPath(path, basePath);
                            self.AddCbsFile(tempPath);
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
                    string basePath = System.IO.Path.GetDirectoryName(self.ProjectFilePath);
                    CbsPathList = new List<string>();
                    foreach (var item in self.cbsGroup.Child)
                    {
                        string tempPath = System.IO.Path.GetRelativePath(basePath, item.HintText);
                        CbsPathList.Add(tempPath);
                    }
                };
            }
            #region 固有定義
            public List<string> CbsPathList { get; set; } = null;

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
            #endregion
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
        private const string COMMAND_SAVE_PROJECT = "Save Project";
        private const string COMMAND_CLEAR_PROJECT = "Clear Project";
        private const string COMMAND_ADD_NEW_CBS_FILE = "Add New CBS File";
        private const string COMMAND_ADD_CBS_FILE = "Add CBS File";

        private string loadProjectName = "";

        private TreeMenuNode commandGroup = new TreeMenuNode(TreeMenuNode.NodeType.GROUP, "Command");
        private TreeMenuNode cbsGroup = new TreeMenuNode(TreeMenuNode.NodeType.GROUP, "CBS Files");

        private string ProjectFilePath = CommandCanvasList.GetSamplePath();

        public ProjectControl()
        {
            InitializeComponent();
            AssetXML = new _AssetXML<ProjectControl>(this);
            DataContext = this;

            ProjectTree.AssetTreeData = new ObservableCollection<TreeMenuNode>();
            MakeCommandMenu(ProjectTree);
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

                commandGroup.AddChild(new TreeMenuNode(TreeMenuNode.NodeType.NORMAL, "New Project", CreateImmediateExecutionCanvasCommand(() => NewProject())));
                commandGroup.AddChild(new TreeMenuNode(TreeMenuNode.NodeType.NORMAL, "Load Project", CreateImmediateExecutionCanvasCommand(() => LoadProject())));
                commandGroup.AddChild(new TreeMenuNode(TreeMenuNode.NodeType.NORMAL, COMMAND_SAVE_PROJECT, CreateImmediateExecutionCanvasCommand(() => SaveProject())));
                commandGroup.AddChild(new TreeMenuNode(TreeMenuNode.NodeType.NORMAL, COMMAND_CLEAR_PROJECT, CreateImmediateExecutionCanvasCommand(() => ClearProject())));
                commandGroup.AddChild(new TreeMenuNode(TreeMenuNode.NodeType.NORMAL, COMMAND_ADD_NEW_CBS_FILE, CreateImmediateExecutionCanvasCommand(() => AddNewCbsFile())));
                commandGroup.AddChild(new TreeMenuNode(TreeMenuNode.NodeType.NORMAL, COMMAND_ADD_CBS_FILE, CreateImmediateExecutionCanvasCommand(() => AddCbsFile())));
                commandGroup.IsExpanded = true;
                treeViewCommand.AssetTreeData.Add(commandGroup);
            }
            treeViewCommand.AssetTreeData.Add(cbsGroup);
            UpdateCommandEnable();
        }

        /// <summary>
        /// コマンドの有効状態を反映します。
        /// </summary>
        private void UpdateCommandEnable()
        {
            string[] commands = new string[] {
                COMMAND_SAVE_PROJECT,
                COMMAND_CLEAR_PROJECT,
                COMMAND_ADD_NEW_CBS_FILE,
                COMMAND_ADD_CBS_FILE,
            };
            foreach (string command in commands)
            {
                SetCommandEnable(command, IsOpenProject);
            }
        }

        /// <summary>
        /// コマンドの有効状態をセットします。
        /// </summary>
        /// <param name="name">コマンド名</param>
        /// <param name="enable">true==有効</param>
        private void SetCommandEnable(string name, bool enable)
        {
            foreach (var item in commandGroup.Child)
            {
                var node = item as TreeMenuNode;
                if (node.Name == name)
                {
                    node.IsEnabled = enable;
                    break;
                }
            }
        }

        /// <summary>
        /// プロジェクトをクリアします。
        /// </summary>
        public void ClearProject()
        {
            if (CommandCanvasList.IsCursorLock())
            {
                return;
            }

            if (ControlTools.ShowSelectMessage(
                        CapyCSS.Language.Instance["SYSTEM_ConfirmationDelete"],
                        CapyCSS.Language.Instance["SYSTEM_Confirmation"],
                        MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {
                ProjectName = INIT_PROJECT_NAME;
                cbsGroup.ClearChild();
                CommandCanvasList.ClearScriptCanvas();
                UpdateCommandEnable();
                ChangedFlag = false;
            }
        }

        /// <summary>
        /// プロジェクト情報をセットします。
        /// </summary>
        /// <param name="path"></param>
        private void SetProjectName(string path)
        {
            loadProjectName = System.IO.Path.GetFileNameWithoutExtension(path);
            ProjectName = loadProjectName;
            ProjectFilePath = path;
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

            string path = CommandCanvasList.MakeNewFileDialog(CommandCanvasList.CBSPROJ_FILTER);
            if (path is null || loadProjectName == path)
            {
                return;
            }

            if (File.Exists(path))
            {
                ControlTools.ShowErrorMessage(CapyCSS.Language.Instance["Help:SYSTEM_Error_Exist"]);
                return;
            }

            CommandCanvasList.ClearScriptCanvas();
            SetProjectName(path);
            UpdateCommandEnable();
            ChangedFlag = true;
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

            string path = CommandCanvasList.ShowLoadDialog(CommandCanvasList.CBSPROJ_FILTER);
            if (path is null)
            {
                return;
            }

            CommandCanvasList.ClearScriptCanvas();
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
            Debug.Assert(ProjectFilePath != null);

            if (CommandCanvasList.IsCursorLock())
            {
                return;
            }

            string path = CommandCanvasList.ShowSaveDialog(CommandCanvasList.CBSPROJ_FILTER, ProjectFilePath);
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
        private void AddNewCbsFile()
        {
            string path = CommandCanvasList.MakeNewFileDialog(CommandCanvasList.CBS_FILTER);
            if (path is null)
            {
                return;
            }

            if (File.Exists(path) || ContainsCBS(path))
            {
                ControlTools.ShowErrorMessage(CapyCSS.Language.Instance["Help:SYSTEM_Error_Exist"]);
                return;
            }

            if (CommandCanvasList.Instance.AddNewContents(path))
            {
                CommandCanvasList.Instance.OverwriteCbsFile();
                AddCbsFile(path);
            }
            UpdateCommandEnable();
        }

        /// <summary>
        /// cbsファイルをプロジェクトに追加します。
        /// </summary>
        private void AddCbsFile()
        {
            string path = CommandCanvasList.ShowLoadDialog(CommandCanvasList.CBS_FILTER);

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
                string dir = System.IO.Path.GetDirectoryName(ProjectFilePath);
                string ext = System.IO.Path.GetExtension(ProjectFilePath);
                string newName = System.IO.Path.Combine(dir, name + ext);

                if (File.Exists(ProjectFilePath))
                {
                    System.IO.File.Move(ProjectFilePath, newName);
                    Console.WriteLine($"Rename...\"{ProjectFilePath}\" => \"{newName}\"");
                }
                ProjectFilePath = newName;
                loadProjectName = System.IO.Path.GetFileNameWithoutExtension(ProjectFilePath);
                CommandCanvasList.UpdateTitle();
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
