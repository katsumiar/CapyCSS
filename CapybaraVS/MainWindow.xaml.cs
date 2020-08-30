using CapybaraVS.Controls.BaseControls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
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

namespace CapybaraVS
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public bool EnablePublicExecuteEntryPoint
        {
            get => ExecuteButton.IsEnabled;
            set
            {
                ExecuteButton.IsEnabled = value;
            }
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
                // 有効化時は、復元として動作する

                EnablePublicExecuteEntryPoint = backupIsEnebleExecuteButton;
            }
            else
            {
                backupIsEnebleExecuteButton = EnablePublicExecuteEntryPoint;
                EnablePublicExecuteEntryPoint = false;
            }
        }

        static public bool IsLockExecute = false; 

        /// <summary>
        /// 全スクリプトエントリーポイントEnableリスト
        /// </summary>
        private static List<Action<bool>> AllExecuteEntryPointList = new List<Action<bool>>();

        /// <summary>
        /// スクリプト実行用公開エントリーポイントリスト
        /// </summary>
        private static List<Action> PublicExecuteEntryPointList = new List<Action>();

        /// <summary>
        /// スクリプト実行用公開エントリーポイントリストをクリアします。
        /// </summary>
        public static void ClearPublicExecuteEntryPoint()
        {
            AllExecuteEntryPointList.Clear();
            AddAllExecuteEntryPointEnable(Instance.SetExecuteButtonEnable);

            PublicExecuteEntryPointList.Clear();
            
            Instance.EnablePublicExecuteEntryPoint = false;
        }

        /// <summary>
        /// 全スクリプトエントリーポイントEnableリストにEnable制御を追加します。
        /// </summary>
        public static void AddAllExecuteEntryPointEnable(Action<bool> func)
        {
            AllExecuteEntryPointList.Add(func);
        }

        /// <summary>
        /// 全スクリプトエントリーポイントリストからエントリーポイントを削除します。
        /// </summary>
        public static void RemoveAllExecuteEntryPointEnable(Action<bool> func)
        {
            AllExecuteEntryPointList.Remove(func);
        }

        /// <summary>
        /// 全スクリプトエントリーポイントリストを使ってすべての Enable を制御します。
        /// </summary>
        public static void CallAllExecuteEntryPointEnable(bool enable)
        {
            foreach (var act in AllExecuteEntryPointList)
            {
                act?.Invoke(enable);
            }
        }

        /// <summary>
        /// スクリプト実行用公開エントリーポイントリストにエントリーポイントを追加します。
        /// </summary>
        public static void AddPublicExecuteEntryPoint(Action func)
        {
            PublicExecuteEntryPointList.Add(func);
            Instance.EnablePublicExecuteEntryPoint = true;
        }

        /// <summary>
        /// スクリプト実行用公開エントリーポイントリストからエントリーポイントを削除します。
        /// </summary>
        public static void RemovePublicExecuteEntryPoint(Action func)
        {
            PublicExecuteEntryPointList.Remove(func);
            if (PublicExecuteEntryPointList.Count == 0)
            {
                Instance.EnablePublicExecuteEntryPoint = false;
            }
        }

        /// <summary>
        /// スクリプト実行用公開エントリーポイントリストからエントリーポイントをまとめて順次呼び出しします。
        /// </summary>
        public static void CallPublicExecuteEntryPoint()
        {
            if (IsLockExecute)
                return; // 再入を禁止する

            foreach (var act in PublicExecuteEntryPointList)
            {
                act?.Invoke();
            }
            Instance.Dispatcher.BeginInvoke(new Action(() =>
            {
                // アイドル状態になってから戻す

                if (App.IsAutoExit)
                {
                    // スクリプト実行後自動終了

                    MainWindow.Instance.CallClosing();
                }
            }), DispatcherPriority.ApplicationIdle);
        }

        static private MainWindow instance;

        /// <summary>
        /// MainWindowのインスタンスを参照します。
        /// </summary>
        static public MainWindow Instance
        {
            get { return instance; }
        }

        public MainWindow()
        {
            InitializeComponent();
            instance = this;
            Closing += MainWindow_Closing;
            OpenFileName = "";
            ClearPublicExecuteEntryPoint();

            Dispatcher.BeginInvoke(new Action(() =>
            {
                // アイドル状態になってから戻す

                ShowSystemErrorLog();
            }), DispatcherPriority.ApplicationIdle);
        }

        /// <summary>
        /// システムエラーを出力する
        /// </summary>
        private static void ShowSystemErrorLog()
        {
            if (App.ErrorLog.Length != 0)
            {
                OutputWindow outputWindow = new OutputWindow();
                outputWindow.Title = "SystemError";
                outputWindow.Owner = MainWindow.Instance;
                outputWindow.Show();

                outputWindow.AddBindText = App.ErrorLog;
            }
        }

        private string openFileName;

        /// <summary>
        /// 開いているファイルを参照します。
        /// </summary>
        public string OpenFileName
        {
            get => openFileName;
            set
            {
                var assm = Assembly.GetExecutingAssembly();
                var name = assm.GetName();
                openFileName = value;
                Title = name.Name + " ver " + AppVersion;
                if (openFileName != "")
                    Title += $" [{openFileName}]";
            }
        }

        /// <summary>
        /// アプリバージョンを参照します。
        /// </summary>
        public string AppVersion
        {
            get
            {
                var assm = Assembly.GetExecutingAssembly();
                var name = assm.GetName();
                return name.Version.ToString();
            }
        }

        /// <summary>
        /// アプリの終了処理です。
        /// </summary>
        public void CallClosing()
        {
            ToolExec.KillProcess();
            Close();
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ToolExec.KillProcess();
        }

        /// <summary>
        /// スクリプトを実行します。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // スクリプトを実行する

            CallPublicExecuteEntryPoint();
        }

        /// <summary>
        /// キーボードによる操作を定義します。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Grid_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.F5:    // スクリプト実行
                    CallPublicExecuteEntryPoint();
                    break;
            }
        }
    }
}
