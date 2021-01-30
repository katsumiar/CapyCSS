using CapybaraVS.Controls.BaseControls;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CapybaraVS.Controls
{
    /// <summary>
    /// DeleteIcon.xaml の相互作用ロジック
    /// </summary>
    public partial class DeleteIcon : UserControl
    {
        #region ClickEvent 添付プロパティ実装

        private static ImplementDependencyProperty<DeleteIcon, Action> impClickEvent =
            new ImplementDependencyProperty<DeleteIcon, Action>(
                nameof(ClickEvent),
                (self, getValue) =>
                {
                    Action value = getValue(self);
                    //self.Visibility = (value is null) ? Visibility.Collapsed : Visibility.Visible;
                });

        public static readonly DependencyProperty ClickEventProperty = impClickEvent.Regist(null);

        public Action ClickEvent
        {
            get { return impClickEvent.GetValue(this); }
            set { impClickEvent.SetValue(this, value); }
        }

        #endregion

        public DeleteIcon()
        {
            InitializeComponent();

            SizeChanged += DeleteIcon_SizeChanged;
        }

        private void DeleteIcon_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Peke1.Width = ActualWidth * 0.1;
            Peke1.Height = ActualHeight * 0.95;
            Peke2.Width = Peke1.Width;
            Peke2.Height = Peke1.Height;
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ClickEvent?.Invoke();
        }

        private void Grid_MouseEnter(object sender, MouseEventArgs e)
        {
            Cursor = Cursors.Hand;
        }

        private void Grid_MouseLeave(object sender, MouseEventArgs e)
        {
            Cursor = null;
        }
    }
}
