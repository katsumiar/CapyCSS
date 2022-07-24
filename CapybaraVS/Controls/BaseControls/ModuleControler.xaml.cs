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
        private string InstallDllDirectory = null;
        private string InstallNuGetDirectory = null;

        private const string DLL_MESSAGE_TITLE = "Dll Import";

        // 過去のインポートの再取り込み時の識別用文字列
        public const string HEADER_NAMESPACE = "namespace ";
        public const string HEADER_NUGET = "NuGet ";
        public const string HEADER_DLL = "Import ";

        // メニュー定義
        private enum ImportMenu
        {
            NameSpace,
            DLL,
            NuGet,
            //
            End,                // メニュー終了
            Init = NameSpace,   // 基本表示項目
        }

        public ModuleControler(ApiImporter apiImporter, string installDllDirectory, string installNuGetDirectory)
        {
            ApiImporter = apiImporter;
            InstallDllDirectory = installDllDirectory;
            InstallNuGetDirectory = installNuGetDirectory;
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

                string msg = string.Format(CapyCSS.Language.Instance["SYSTEM_ModuleControler_04"], filter);
                ControlTools.ShowErrorMessage(msg, DLL_MESSAGE_TITLE);
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
            CommandCanvasList.SetOwnerCursor(Cursors.Wait);
            try
            {
                if (AddLabel_MouseDown())
                {
                    InportList.SelectedIndex = (int)ImportMenu.Init;
                    InstallDllList_SelectionChanged(null, null);
                }
            }
            finally
            {
                CommandCanvasList.ResetOwnerCursor(Cursors.Wait);
            }
        }
        private bool AddLabel_MouseDown()
        {
            string selectedValue = (string)InportList.SelectedValue;
            if (selectedValue is null)
            {
                return false;
            }

            if (selectedValue == "[ " + ImportMenu.DLL.ToString() + " ]")
            {
                // dllをインストールしてからインポートする

                string installDllPath = InstallDll();
                if (installDllPath != null)
                {
                    // dllをインポートする

                    ApiImporter.ImportDll(installDllPath);
                    return true;
                }
                return false;
            }
            else if (selectedValue == "[ " + ImportMenu.NameSpace.ToString() + " ]")
            {
                // ネームスペース指定でインポートする

                string imputText = NameSpaceName.Text.Trim();
                if (imputText == "")
                    return false;

                // ネームスペースをインポートする
                if (ApiImporter.ImportNameSpace(imputText))
                {
                    // 成功

                    InportList.SelectedIndex = (int)ImportMenu.Init;
                    return true;
                }
                return false;
            }
            else if (selectedValue == "[ " + ImportMenu.NuGet.ToString() + " ]")
            {
                string nugetName = PackageName.Text.Trim();
                if (nugetName == "")
                    return false;
                string version = Version.Text.Trim();
                if (version == "")
                    return false;

                // NuGetをインポートする
                if (ApiImporter.ImportNuGet(InstallNuGetDirectory, nugetName, version))
                {
                    // 成功

                    InportList.SelectedIndex = (int)ImportMenu.Init;
                    return true;
                }
                return false;
            }

            // 既存のインポート

            if (IsNuGet(selectedValue))
            {
                // NeGetをインポートする

                ApiImporter.ImportNuGet(InstallDllDirectory, selectedValue.Split(" ")[1]);
                return true;
            }

            string dllPath = System.IO.Path.Combine(InstallDllDirectory, selectedValue);
            if (!File.Exists(dllPath))
            {
                string msg = string.Format(CapyCSS.Language.Instance["SYSTEM_ModuleControler_04"], dllPath);
                ControlTools.ShowErrorMessage(msg, DLL_MESSAGE_TITLE);
                return true;
            }

            // インストール済みのdllをインポートする
            ApiImporter.ImportDll(dllPath);
            return true;
        }

        /// <summary>
        /// NuGetを判定します。
        /// </summary>
        /// <param name="selected">名前</param>
        /// <returns>true==クラス</returns>
        private static bool IsNuGet(string selected)
        {
            return selected.StartsWith(HEADER_NUGET);
        }

        /// <summary>
        /// Dllを判定します。
        /// </summary>
        /// <param name="selected">名前</param>
        /// <returns>true==Dll</returns>
        private static bool IsDll(string selected)
        {
            return selected.StartsWith(HEADER_DLL);
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

                string msg = string.Format(CapyCSS.Language.Instance["SYSTEM_ModuleControler_01"], dllFileName);
                var ret = ControlTools.ShowSelectMessage(msg, DLL_MESSAGE_TITLE, MessageBoxButton.YesNo);
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

                string msg = string.Format(CapyCSS.Language.Instance["SYSTEM_ModuleControler_02"], dllFileName);
                ControlTools.ShowMessage(msg, DLL_MESSAGE_TITLE);

                // アプリケーションの終了
                CommandCanvasList.CallClosing?.Invoke();
                return null;
            }
            else
            {
                // インストール成功

                string msg = string.Format(CapyCSS.Language.Instance["SYSTEM_ModuleControler_03"], dllFileName);
                ControlTools.ShowMessage(msg, DLL_MESSAGE_TITLE);
            }

            // ドロップダウンリストに登録
            InportList.Items.Add(dllPath);
            return dllPath;
        }

        /// <summary>
        /// インストール済みの dll 一覧を得ます。
        /// </summary>
        private void ListFetchInstallDll()
        {
            InportList.Items.Clear();
            foreach (ImportMenu value in Enum.GetValues(typeof(ImportMenu)))
            {
                if (value == ImportMenu.End)
                {
                    break;
                }
                InportList.Items.Add($"[ {Enum.GetName(typeof(ImportMenu), value)} ]");
            }
            InportList.SelectedIndex = (int)ImportMenu.Init;
            string[] paths = Directory.GetFiles(InstallDllDirectory, "*.dll");
            foreach (string path in paths)
            {
                InportList.Items.Add(System.IO.Path.GetFileName(path));
            }
        }

        private void ComboBox_DropDownOpened(object sender, EventArgs e)
        {
            ListFetchInstallDll();
        }

        /// <summary>
        /// ネームスペース名入力欄を閉じます。
        /// </summary>
        private void closeInputNameSpaceName()
        {
            NameSpaceName.Text = "";
            imputNameSpaceName.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// NuGet入力欄を閉じます。
        /// </summary>
        private void closeInputNuGet()
        {
            PackageName.Text = "";
            Version.Text = "";
            imputNuGetName.Visibility = Visibility.Collapsed;
        }

        private void InstallDllList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string selectedValue = (string)InportList.SelectedValue;
            if (selectedValue is null)
            {
                return;
            }
            closeInputNameSpaceName();
            closeInputNuGet();
            if (selectedValue == "[ " + ImportMenu.NameSpace.ToString() + " ]")
            {
                // ネームスペース名入力欄を表示する

                imputNameSpaceName.Visibility = Visibility.Visible;
            }
            else if (selectedValue == "[ " + ImportMenu.NuGet.ToString() + " ]")
            {
                // NuGet入力欄を表示する

                imputNuGetName.Visibility = Visibility.Visible;
            }
        }
    }
}
