using CapybaraVS;
using CapybaraVS.Controls.BaseControls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        #region Message プロパティ実装

        private static ImplementWindowDependencyProperty<CommandWindow, string> impCaption =
            new ImplementWindowDependencyProperty<CommandWindow, string>(
                nameof(Message),
                (self, getValue) =>
                {
                    string value = getValue(self);
                    if (value != null && value.Trim() != "")
                    {
                        self.MessageBox.Text = value;
                        self.MessageBox.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        self.MessageBox.Visibility = Visibility.Collapsed;
                    }
                });

        public static readonly DependencyProperty CaptionProperty = impCaption.Regist(null);

        public string Message
        {
            get { return impCaption.GetValue(this); }
            set { impCaption.SetValue(this, value); }
        }

        #endregion

        public void SetPos(Point? pos = null)
        {
            ControlTools.SetWindowPos(this, pos);
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
            filterProcTimer.Tick += EventHandler;
        }

        void EventHandler(object sender, EventArgs e)
        {
            filterProcTimer.Stop();

            // 待機後の処理
            treeViewCommand.SetFilter(
                findTreeViewCommand,
                FilterText.Text);
            filterProcTimer.IsEnabled = false;
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
                treeViewCommand.Visibility = Visibility.Collapsed;
                findTreeViewCommand.Visibility = Visibility.Visible;
            }
            else
            {
                filterProcTimer.IsEnabled = false;
                treeViewCommand.Visibility = Visibility.Visible;
                findTreeViewCommand.Visibility = Visibility.Collapsed;
            }
        }

        public void Dispose()
        {
            trueCloseing = true;
            Close();
            filterProcTimer.Tick -= EventHandler;
        }
    }
}
