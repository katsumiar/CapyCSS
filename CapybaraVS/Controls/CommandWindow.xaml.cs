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
    public partial class CommandWindow
        : Window
        , IDisposable
    {
        public TreeViewCommand treeViewCommand = new TreeViewCommand();

        /// <summary>
        /// コマンドウインドウを表示します。
        /// </summary>
        /// <param name="pos">表示位置</param>
        /// <returns>ウインドウクラスのインスタンス</returns>
        public static CommandWindow Create(Point? pos = null)
        {
            CommandWindow commandWindow = new CommandWindow();
            commandWindow.Owner = CommandCanvasList.OwnerWindow;
            commandWindow.SetPos(pos);
            return commandWindow;
        }

        public void SetPos(Point? pos = null)
        {
            if (pos.HasValue)
            {
                Left = pos.Value.X;
                Top = pos.Value.Y;
                if (CommandCanvasList.OwnerWindow.WindowState != WindowState.Maximized)
                {
                    // ウインドウが最大化されても元のサイズが帰ってくるようなので、最大化していないときだけ相対位置にする

                    Left += CommandCanvasList.OwnerWindow.Left;
                    Top += CommandCanvasList.OwnerWindow.Top;
                }
                else
                {
                    if (CommandCanvasList.OwnerWindow.Left > SystemParameters.PrimaryScreenWidth)
                    {
                        // セカンダリディスプレイでクリックされた

                        Left += SystemParameters.PrimaryScreenWidth;
                    }
                }
            }
        }

        /// <summary>
        /// /コマンドウインドウを閉じます。
        /// </summary>
        public void CloseWindow()
        {
            Close();
        }

        public CommandWindow()
        {
            InitializeComponent();

            (OpenListContents as IAddChild).AddChild(treeViewCommand);
        }

        bool trueCloseing = false;

        /// <summary>
        /// ウインドウ破棄をキャンセルします。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!trueCloseing)
            {
                Hide();
                e.Cancel = true;
            }
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
                    treeViewCommand.SetFilter(FilterText.Text);
                    filterProcTimer.IsEnabled = false;
                };
            }
            else
            {
                filterProcTimer.IsEnabled = false;
                treeViewCommand.ClearFilter();
            }
        }

        public void Dispose()
        {
            trueCloseing = true;
            Close();
        }
    }
}
