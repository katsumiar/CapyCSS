using CapyCSS;
using CapyCSS.Script;
using Microsoft.Win32;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CapyCSS.Controls.BaseControls
{
    /// <summary>
    /// ModuleControler.xaml の相互作用ロジック
    /// </summary>
    public partial class ModuleControler : UserControl
    {
        private ApiImporter ApiImporter = null;

        // 過去のインポートの再取り込み時の識別用文字列
        public const string HEADER_NAMESPACE = "namespace ";

        public ModuleControler(ApiImporter apiImporter, string installDllDirectory, string installNuGetDirectory)
        {
            ApiImporter = apiImporter;
            InitializeComponent();
            ImportList.ItemsSource = apiImporter.ModulueNameList;
        }

        /// <summary>
        /// 外部モジュールを取り込みます。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddLabel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            CommandCanvasList.SetOwnerCursor(Cursors.Wait);
            try
            {
                AddLabel_MouseDown();
            }
            finally
            {
                CommandCanvasList.ResetOwnerCursor(Cursors.Wait);
            }
        }

        private bool AddLabel_MouseDown()
        {
            // ネームスペース指定でインポートする

            string imputText = NameSpaceName.Text.Trim();
            if (imputText == "")
            {
                return false;
            }

            // ネームスペースをインポートする
            return ApiImporter.ImportNameSpace(imputText);
        }
    }
}
