using System;
using System.Collections.Generic;
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
    /// RemovableLabel.xaml の相互作用ロジック
    /// </summary>
    public partial class RemovableLabel : UserControl
    {
        #region Title プロパティ実装
        private static ImplementDependencyProperty<RemovableLabel, string> impTitle =
            new ImplementDependencyProperty<RemovableLabel, string>(
                nameof(Title),
                (self, getValue) =>
                {
                    var value = getValue(self);
                    if (self.Mask is null)
                    {
                        self.Caption.Text = value;
                        self.Caption.ToolTip = null;
                    }
                    else
                    {
                        self.Caption.Text = self.Mask(value);
                        self.Caption.ToolTip = value;
                    }
                });

        public static readonly DependencyProperty TitleProperty = impTitle.Regist("(none)");

        public string Title
        {
            get { return impTitle.GetValue(this); }
            set { impTitle.SetValue(this, value); }
        }
        #endregion

        #region Mask プロパティ実装
        private static ImplementDependencyProperty<RemovableLabel, Func<string,string>> impMask =
            new ImplementDependencyProperty<RemovableLabel, Func<string, string>>(
                nameof(Mask),
                (self, getValue) =>
                {
                    var value = getValue(self);
                    if (value != null)
                    {
                        self.Caption.Text = value(self.Title);
                        self.Caption.ToolTip = self.Title;
                    }
                });

        public static readonly DependencyProperty MaskProperty = impMask.Regist(null);

        public Func<string, string> Mask
        {
            get { return impMask.GetValue(this); }
            set { impMask.SetValue(this, value); }
        }
        #endregion

        #region ClickEvent 添付プロパティ実装
        private static ImplementDependencyProperty<RemovableLabel, Action> impClickEvent =
            new ImplementDependencyProperty<RemovableLabel, Action>(
                nameof(ClickEvent),
                (self, getValue) =>
                {
                    Action value = getValue(self);
                    self.Delete.ClickEvent = value;
                });

        public static readonly DependencyProperty ClickEventProperty = impClickEvent.Regist(null);

        public Action ClickEvent
        {
            get { return impClickEvent.GetValue(this); }
            set { impClickEvent.SetValue(this, value); }
        }
        #endregion

        public RemovableLabel()
        {
            InitializeComponent();
        }

        private void Delete_MouseEnter(object sender, MouseEventArgs e)
        {
            if (ClickEvent != null)
            {
                Delete.Visibility = Visibility.Visible;
            }
        }

        private void Delete_MouseLeave(object sender, MouseEventArgs e)
        {
            if (ClickEvent != null)
            {
                Delete.Visibility = Visibility.Hidden;
            }
        }

        private void Delete_MouseEnter_1(object sender, MouseEventArgs e)
        {

        }

        private void Delete_MouseLeave_1(object sender, MouseEventArgs e)
        {

        }
    }
}
