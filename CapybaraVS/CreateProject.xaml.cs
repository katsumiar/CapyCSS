using CapyCSS.Controls;
using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace CapyCSS
{
    /// <summary>
    /// CreateProject.xaml の相互作用ロジック
    /// </summary>
    public partial class CreateProject 
        : MetroWindow
    {
        #region ProjectFile 添付プロパティ実装
        private static ImplementWindowDependencyProperty<CreateProject, string> impProjectFile =
            new ImplementWindowDependencyProperty<CreateProject, string>(
                nameof(ProjectFile),
                (self, getValue) =>
                {
                    string text = getValue(self);
                    text ??= "";
                    self.OkButton.IsEnabled = text.Length == 0 ? false : true;
                });

        public static readonly DependencyProperty ProjectFileProperty = impProjectFile.Regist("");

        public string ProjectFile
        {
            get { return impProjectFile.GetValue(this); }
            set { impProjectFile.SetValue(this, value); }
        }
        #endregion

        private static string _ProjectPath = null; 
        public string ProjectPath
        {
            get => _ProjectPath;
            set
            {
                if (!Directory.Exists(value))
                {
                    _ProjectPath = CommandCanvasList.GetSamplePath();
                }
                else
                {
                    _ProjectPath = value;
                }
                projectPath.Text = _ProjectPath;
            }
        }

        public static CreateProject Create(Point? pos = null)
        {
            CreateProject createProjectWindow = new CreateProject();
            createProjectWindow.Owner = CommandCanvasList.OwnerWindow;
            ControlTools.SetWindowPos(createProjectWindow, pos);
            return createProjectWindow;
        }

        public CreateProject()
        {
            InitializeComponent();
            DataContext = this;
            Extention.Text = CommandCanvasList.CBSPROJ_EXT;
            if (ProjectPath is null)
            {
                ProjectPath ??= CommandCanvasList.GetSamplePath();
            }
            else
            {
                projectPath.Text = ProjectPath;
            }
        }

        private void SelectDirectory(object sender, MouseButtonEventArgs e)
        {
            string path = CommandCanvasList.SelectDirectroyDialog(ProjectPath);
            if (path != null)
            {
                ProjectPath = path;
            }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
