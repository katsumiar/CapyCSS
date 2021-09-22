using CapyCSS.Controls;
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

namespace CapybaraVS.Controls.BaseControls
{
    /// <summary>
    /// AddLabel.xaml の相互作用ロジック
    /// </summary>
    public partial class AddLabel : UserControl
    {
        public static readonly DependencyProperty CaptionProperty =
            DependencyProperty.Register("Caption",
                                        typeof(string),
                                        typeof(AddLabel),
                                        new FrameworkPropertyMetadata("Caption", new PropertyChangedCallback(OnCaptionChanged)));

        public AddLabel()
        {
            InitializeComponent();
        }

        public string Caption
        {
            get { return (string)GetValue(CaptionProperty); }
            set { SetValue(CaptionProperty, value); }
        }

        private static void OnCaptionChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            // オブジェクトを取得して処理する

            AddLabel ctrl = obj as AddLabel;
            if (ctrl != null)
            {
                ctrl.Label.Content = ctrl.Caption;
            }
        }

        private void StackPanel_MouseEnter(object sender, MouseEventArgs e)
        {
            CommandCanvasList.SetOwnerCursor(Cursors.Hand);
        }

        private void StackPanel_MouseLeave(object sender, MouseEventArgs e)
        {
            CommandCanvasList.SetOwnerCursor(null);
        }
    }
}
