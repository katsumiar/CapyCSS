using CapybaraVS.Script;
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
        private string InstallDllDirectory = null;

        private const string MESSAGE_TITLE = "Dll Import";

        private const string HEADER_PACKAGE = "Package ";
        private const string HEADER_CLASS = "Class ";

        private const int SELECT_INSTALL_DLL = 0;
        private const int SELECT_IMPORT_CLASS = 1;

        public ModuleControler(ApiImporter apiImporter, string installDllDirectory)
        {
            ApiImporter = apiImporter;
            InstallDllDirectory = installDllDirectory;
            InitializeComponent();
            ImportList.ItemsSource = apiImporter.ModulueNameList;
            ListFetchInstallDll();
        }

        /// <summary>
        /// dllの選択を要求します。
        /// </summary>
        /// <param name="filter">nullなら任意のdllを選択, それ以外はファイルを指定</param>
        /// <returns>null==失敗、それ以外は、選択されたdllファイル</returns>
        private string dllSelectionRequest(string filter = null)
        {
            if (filter != null)
            {
                // TODO フォルダを指定してもらいファイルのパスを特定してもらう

                string msg = string.Format(CapybaraVS.Language.GetInstance["ModuleControler_04"], filter);
                MessageBox.Show(msg, MESSAGE_TITLE);
                return null;    // 失敗
            }

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
            if (InstallDllList.SelectedIndex == SELECT_INSTALL_DLL)
            {
                // 新規にdllをインストールしてからインポートする

                string installDllPath = InstallDll();
                if (installDllPath is null)
                {
                    // dll のインストールに失敗

                    return;
                }

                // dllをインポートする
                ApiImporter.ImportDll(installDllPath);
                return;
            }
            else if (InstallDllList.SelectedIndex == SELECT_IMPORT_CLASS)
            {
                // 新規にクラス指定でインポートする

                string imputText = ClassName.Text.Trim();
                if (imputText == "")
                    return;

                // 入力が正しいか調べる
                Type classType = CbST.GetTypeEx(imputText);
                if (classType is null)
                {
                    // クラスが見つからない

                    backupBackground = ClassName.Background;
                    ClassName.BorderBrush = Brushes.Red;
                    return;
                }

                InstallDllList.SelectedIndex = SELECT_INSTALL_DLL;
                closeInputClassName();

                // クラスをインポートする
                ApiImporter.ImportClass(imputText);
                return;
            }

            // 既存のインポート

            string selected = (string)InstallDllList.SelectedItem;

            if (IsPackage(selected))
            {
                // パッケージをインポートする

                ApiImporter.ImportPackage(selected.Split(" ")[1]);
                return;
            }
            if (IsClass(selected))
            {
                // クラスをインポートする

                ApiImporter.ImportClass(selected.Split(" ")[1]);
                return;
            }

            string dllPath = System.IO.Path.Combine(InstallDllDirectory, selected);
            if (File.Exists(selected))
            {
                string msg = string.Format(CapybaraVS.Language.GetInstance["ModuleControler_04"], dllPath);
                MessageBox.Show(msg, MESSAGE_TITLE);
                return;
            }

            // インストール済みのdllをインポートする
            ApiImporter.ImportDll(dllPath);
        }

        /// <summary>
        /// パッケージを判定します。
        /// </summary>
        /// <param name="selected">名前</param>
        /// <returns>true==パッケージ</returns>
        private static bool IsPackage(string selected)
        {
            return selected.StartsWith(HEADER_PACKAGE);
        }

        /// <summary>
        /// クラスを判定します。
        /// </summary>
        /// <param name="selected">名前</param>
        /// <returns>true==クラス</returns>
        private static bool IsClass(string selected)
        {
            return selected.StartsWith(HEADER_CLASS);
        }

        /// <summary>
        /// Dllを判定します。
        /// </summary>
        /// <param name="selected">名前</param>
        /// <returns>true==Dll</returns>
        private static bool IsDll(string selected)
        {
            return System.IO.Path.GetExtension(selected) == ".dll";
        }

        /// <summary>
        /// dllがインポート可能かを調査します。
        /// </summary>
        /// <param name="importItem">インポート対象の名前</param>
        /// <returns>可能ならtrue</returns>
        public bool CheckImportable(string importItem)
        {
            if (!IsDll(importItem))
            {
                return true;
            }
            
            if (!File.Exists(importItem))
            {
                // dllのインストールを要求する

                if (InstallDll(System.IO.Path.GetFileName(importItem)) == null)
                {
                    // dllのインストールに失敗

                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// DLL をインストールします。
        /// </summary>
        /// <param name="importDllPath">インポート先のパス</param>
        /// <returns>dll 格納パス</returns>
        private string InstallDll(string importDllPath = null)
        {
            importDllPath = dllSelectionRequest(importDllPath);
            if (importDllPath is null)
            {
                // dll のパス取得に失敗

                return null;
            }
            string dllFileName = System.IO.Path.GetFileName(importDllPath);
            string dllPath = System.IO.Path.Combine(InstallDllDirectory, dllFileName);
            bool isReboot = false;
            if (File.Exists(dllPath))
            {
                // dll は既にインポート済み

                string msg = string.Format(CapybaraVS.Language.GetInstance["ModuleControler_01"], dllFileName);
                var ret = MessageBox.Show(msg, MESSAGE_TITLE, MessageBoxButton.YesNo);
                if (ret == MessageBoxResult.No)
                {
                    return null;
                }

                // dll を上書きインストールを選択した
                isReboot = true;
            }
            File.Copy(importDllPath, dllPath, true);
            if (isReboot)
            {
                // 上書きインストール成功

                string msg = string.Format(CapybaraVS.Language.GetInstance["ModuleControler_02"], dllFileName);
                MessageBox.Show(msg, MESSAGE_TITLE);

                // アプリケーションの終了
                CommandCanvasList.CallClosing?.Invoke();
            }
            else
            {
                // インストール成功

                string msg = string.Format(CapybaraVS.Language.GetInstance["ModuleControler_03"], dllFileName);
                MessageBox.Show(msg, MESSAGE_TITLE);
            }

            // ドロップダウンリストに登録
            InstallDllList.Items.Add(dllPath);
            return dllPath;
        }

        /// <summary>
        /// インストール済みの dll 一覧を得ます。
        /// </summary>
        private void ListFetchInstallDll()
        {
            InstallDllList.Items.Clear();
            InstallDllList.Items.Add("[ Install dll ]");
            InstallDllList.Items.Add("[ Import class ]");
            InstallDllList.Items.Add(HEADER_PACKAGE + "MathNet.Numerics");
            InstallDllList.Items.Add(HEADER_PACKAGE + "ClosedXML");
            InstallDllList.SelectedIndex = SELECT_INSTALL_DLL;
            string[] paths = Directory.GetFiles(InstallDllDirectory, "*.dll");
            foreach (string path in paths)
            {
                InstallDllList.Items.Add(System.IO.Path.GetFileName(path));
            }
        }

        private void ComboBox_DropDownOpened(object sender, EventArgs e)
        {
            ListFetchInstallDll();
        }

        private Brush backupBackground = null;

        /// <summary>
        /// クラス名入力欄を閉じます。
        /// </summary>
        private void closeInputClassName()
        {
            imputClassName.Visibility = Visibility.Collapsed;
            if (backupBackground != null)
            {
                ClassName.Background = backupBackground;
            }
        }

        private void InstallDllList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (InstallDllList.SelectedIndex == SELECT_IMPORT_CLASS)
            {
                // クラス名入力欄を表示する

                imputClassName.Visibility = Visibility.Visible;
            }
            else
            {
                // クラス名入力欄を閉じる

                closeInputClassName();
            }
        }
    }
}
