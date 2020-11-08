using CapybaraVS;
using CapybaraVS.Controls.BaseControls;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace CapyCSS.Controls
{
    /// <summary>
    /// CommandWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class CommandWindow : Window
    {
        public static TreeViewCommand TreeViewCommand = new TreeViewCommand();

        private static CommandWindow self = null;

        /// <summary>
        /// コマンドウインドウを表示します。
        /// </summary>
        /// <param name="pos">表示位置</param>
        /// <returns>ウインドウクラスのインスタンス</returns>
        public static CommandWindow Create(Point? pos = null)
        {
            if (self is null)
            {
                self = new CommandWindow();
                self.Owner = MainWindow.Instance;
            }
            CommandWindow commandWindow = self;
            if (pos.HasValue)
            {
                commandWindow.Left = pos.Value.X;
                commandWindow.Top = pos.Value.Y;
                if (MainWindow.Instance.WindowState != WindowState.Maximized)
                {
                    // ウインドウが最大化されても元のサイズが帰ってくるようなので、最大化していないときだけ相対位置にする

                    commandWindow.Left += MainWindow.Instance.Left;
                    commandWindow.Top += MainWindow.Instance.Top;
                }
                else
                {
                    if (MainWindow.Instance.Left > SystemParameters.PrimaryScreenWidth)
                    {
                        // セカンダリディスプレイでクリックされた

                        commandWindow.Left += SystemParameters.PrimaryScreenWidth;
                    }
                }
            }
            return commandWindow;
        }

        /// <summary>
        /// /コマンドウインドウを閉じます。
        /// </summary>
        public static void CloseWindow()
        {
            self?.Hide();
        }

        public CommandWindow()
        {
            InitializeComponent();

            (OpenListContents as IAddChild).AddChild(TreeViewCommand);
        }

        /// <summary>
        /// ウインドウ破棄をキャンセルします。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Hide();
            e.Cancel = true;
        }

        DispatcherTimer filterProcTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(300) };

        private void FilterText_KeyUp(object sender, KeyEventArgs e)
        {
            FilterText.Text = FilterText.Text.Trim();
            if (FilterText.Text != "")
            {
                // フィルタリング処理は、スレッドが使えないのでタイマーで時間差を置いて処理する

                filterProcTimer.IsEnabled = true;
                filterProcTimer.Start();
                filterProcTimer.Tick += (s, args) =>
                {
                    filterProcTimer.Stop();

                    // 待機後の処理
                    TreeViewCommand.SetFilter(FilterText.Text);
                    filterProcTimer.IsEnabled = false;
                };
            }
            else
            {
                filterProcTimer.IsEnabled = false;
                TreeViewCommand.ClearFilter();
            }
        }
    }
}
