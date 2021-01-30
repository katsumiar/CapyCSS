using CapybaraVS.Script;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
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

        public ModuleControler(ApiImporter apiImporter)
        {
            ApiImporter = apiImporter;
            InitializeComponent();
            ImportList.ItemsSource = apiImporter.ModulueNameList;
        }

        /// <summary>
        /// 読み込み用ファイル選択ダイアログを表示します。
        /// </summary>
        /// <returns></returns>
        private string ShowLoadDialog()
        {
            var dialog = new OpenFileDialog();

            // ファイルの種類を設定
            dialog.Filter = "DLL files (*.dll)|*.dll";

            // ダイアログを表示する
            if (dialog.ShowDialog() == true)
            {
                return dialog.FileName;
            }
            return null;
        }

        /// <summary>
        /// 外部モジュールを取り込みます。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddLabel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            string dllPath = ShowLoadDialog();
            if (dllPath != null)
            {
                ApiImporter.LoadDll(dllPath);
            }
        }
    }
}
