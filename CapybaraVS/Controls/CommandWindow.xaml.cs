using CapyCSS;
using CapyCSS.Controls.BaseControls;
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
using MahApps.Metro.Controls;

namespace CapyCSS.Controls
{
    /// <summary>
    /// CommandWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class CommandWindow
        : MetroWindow
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

        #region FilterString プロパティ実装

        private static ImplementWindowDependencyProperty<CommandWindow, string> impFilterString =
            new ImplementWindowDependencyProperty<CommandWindow, string>(
                nameof(FilterString),
                (self, getValue) =>
                {
                    string value = getValue(self);
                    if (value != null)
                    {
                        self.FilteringCommand();
                    }
                });

        public static readonly DependencyProperty FilterStringProperty = impFilterString.Regist(null);

        public string FilterString
        {
            get { return impFilterString.GetValue(this); }
            set { impFilterString.SetValue(this, value); }
        }

        #endregion

        public CommandWindow()
        {
            InitializeComponent();
            filterProcTimer.Tick += EventHandler;
            filterProcTimer.IsEnabled = false;
            DataContext = this;
        }

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

        /// <summary>
        /// コマンドの有効状態を反映します。
        /// </summary>
        public void UpdateCommandEnable()
        {
            treeViewCommand.RefreshItem();
        }

        void EventHandler(object sender, EventArgs e)
        {
            filterProcTimer.Stop();

            cancellationTokenSource?.Cancel();
            filteringViewCommand.AssetTreeData.Clear();

            // 待機後の処理
            cancellationTokenSource = treeViewCommand.SetFilter(
                filteringViewCommand,
                FilterString);

            filterProcTimer.IsEnabled = false;
        }

        bool trueCloseing = false;

        /// <summary>
        /// ウインドウを見せかけ上で消します。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!trueCloseing)
            {
                cancellationTokenSource?.Cancel();
                filterProcTimer.Stop();
                filterProcTimer.IsEnabled = false;
                Hide();
                e.Cancel = true;
            }
        }

        System.Threading.CancellationTokenSource cancellationTokenSource = null;
        DispatcherTimer filterProcTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(300) };

        public void FilteringCommand()
        {
            FilterString = FilterString.Trim();
            if (FilterString != "")
            {
                // フィルタリング処理は、タイマーで時間差を置いて処理する

                if (filterProcTimer.IsEnabled)
                {
                    filterProcTimer.Stop();
                    filterProcTimer.Start();
                    return;
                }

                ShowFilteringViewCommand();
                filterProcTimer.IsEnabled = true;
                filterProcTimer.Start();
            }
            else
            {
                // フィルタリング解除

                HideFilteringViewCommand();
                cancellationTokenSource?.Cancel();
                cancellationTokenSource = null;
                filterProcTimer.Stop();
                filterProcTimer.IsEnabled = false;
            }
        }

        private void HideFilteringViewCommand()
        {
            treeViewCommand.Visibility = Visibility.Visible;
            filteringViewCommand.Visibility = Visibility.Collapsed;
        }

        public void ShowFilteringViewCommand()
        {
            treeViewCommand.Visibility = Visibility.Collapsed;
            filteringViewCommand.Visibility = Visibility.Visible;
        }

        public void Dispose()
        {
            cancellationTokenSource?.Cancel();
            cancellationTokenSource = null;
            trueCloseing = true;
            Close();
            filterProcTimer.Tick -= EventHandler;
            filterProcTimer = null;
            filteringViewCommand.AssetTreeData.Clear();
            treeViewCommand.AssetTreeData.Clear();

            GC.SuppressFinalize(this);
        }
    }
}
