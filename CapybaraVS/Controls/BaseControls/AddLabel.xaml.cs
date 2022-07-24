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

namespace CapyCSS.Controls.BaseControls
{
    /// <summary>
    /// AddLabel.xaml の相互作用ロジック
    /// </summary>
    public partial class AddLabel : UserControl
    {
        #region Title 添付プロパティ実装

        private static ImplementDependencyProperty<AddLabel, string> impTitle =
            new ImplementDependencyProperty<AddLabel, string>(
                nameof(Title),
                (self, getValue) =>
                {
                    string value = getValue(self);
                    self.Label.Content = value;
                });

        public static readonly DependencyProperty TitleProperty = impTitle.Regist(null);

        public string Title
        {
            get { return impTitle.GetValue(this); }
            set { impTitle.SetValue(this, value); }
        }

        #endregion

        #region Size 添付プロパティ実装

        private static ImplementDependencyProperty<AddLabel, int> impSize =
            new ImplementDependencyProperty<AddLabel, int>(
                nameof(Size),
                (self, getValue) =>
                {
                    int value = getValue(self);
                    self.ellipse.Width = value;
                    self.ellipse.Height = value;
                    self.line1.Width = value - 6;
                    self.line2.Height = value - 6;
                });

        public static readonly DependencyProperty SizeProperty = impSize.Regist(10);

        public int Size
        {
            get { return impSize.GetValue(this); }
            set { impSize.SetValue(this, value); }
        }

        #endregion

        #region ForegroundBrush 添付プロパティ実装

        private static ImplementDependencyProperty<AddLabel, Brush> impForegroundBrush =
            new ImplementDependencyProperty<AddLabel, Brush>(
                nameof(ForegroundBrush),
                (self, getValue) =>
                {
                    Brush value = getValue(self);
                    self.ellipse.Stroke = value;
                    self.Label.Foreground = value;
                    self.line1.Fill = value;
                    self.line2.Fill = value;
                });

        public static readonly DependencyProperty ForegroundBrushProperty = impForegroundBrush.Regist(null);

        public Brush ForegroundBrush
        {
            get { return impForegroundBrush.GetValue(this); }
            set { impForegroundBrush.SetValue(this, value); }
        }

        #endregion

        public AddLabel()
        {
            InitializeComponent();
            Title = "Sample";
            Size = 18;
            ForegroundBrush = (Brush)Application.Current.FindResource("AddLabelForegroundBrush");
        }

        private void StackPanel_MouseEnter(object sender, MouseEventArgs e)
        {
            CommandCanvasList.SetOwnerCursor(Cursors.Hand);
        }

        private void StackPanel_MouseLeave(object sender, MouseEventArgs e)
        {
            CommandCanvasList.ResetOwnerCursor(Cursors.Hand);
        }
    }
}
