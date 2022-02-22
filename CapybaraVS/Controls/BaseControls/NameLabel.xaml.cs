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
using System.Windows.Threading;
using System.Xml.Serialization;

namespace CapyCSS.Controls.BaseControls
{
    /// <summary>
    /// NameLabel.xaml の相互作用ロジック
    /// </summary>
    public partial class NameLabel 
        : UserControl
        , IDisposable
    {
        #region XML定義
        [XmlRoot(nameof(NameLabel))]
        public class _AssetXML<OwnerClass> : IDisposable
            where OwnerClass : NameLabel
        {
            [XmlIgnore]
            public Action WriteAction = null;
            [XmlIgnore]
            public Action<OwnerClass> ReadAction = null;
            private bool disposedValue;

            public _AssetXML()
            {
                ReadAction = (self) =>
                {
                    self.LabelString = LabelString;

                    // 次回の為の初期化
                    self.AssetXML = new _AssetXML<NameLabel>(self);
                };
            }
            public _AssetXML(OwnerClass self)
            {
                WriteAction = () =>
                {
                    LabelString = self.LabelString;
                };
            }
            #region 固有定義
            public string LabelString { get; set; } = null;

            protected virtual void Dispose(bool disposing)
            {
                if (!disposedValue)
                {
                    if (disposing)
                    {
                        WriteAction = null;
                        ReadAction = null;

                        // 以下、固有定義開放
                        LabelString = null;
                    }
                    disposedValue = true;
                }
            }

            public void Dispose()
            {
                Dispose(disposing: true);
                GC.SuppressFinalize(this);
            }
            #endregion
        }
        public _AssetXML<NameLabel> AssetXML { get; set; } = null;
        #endregion

        #region LabelString プロパティ実装

        private static ImplementDependencyProperty<NameLabel, string> impLabelString =
            new ImplementDependencyProperty<NameLabel, string>(
                nameof(LabelString),
                (self, getValue) =>
                {
                    self.EditControl.Text = getValue(self);
                    self.LabelControl.Text = getValue(self);
                });

        public static readonly DependencyProperty LabelStringProperty = impLabelString.Regist("(none)");

        public string LabelString
        {
            get { return impLabelString.GetValue(this); }
            set { impLabelString.SetValue(this, value); }
        }

        #endregion

        #region Hint プロパティ実装

        private static ImplementDependencyProperty<NameLabel, string> impHint =
            new ImplementDependencyProperty<NameLabel, string>(
                nameof(Hint),
                (self, getValue) =>
                {
                    self.LabelControl.ToolTip = getValue(self);
                });

        public static readonly DependencyProperty LabelHint = impHint.Regist(null);

        public string Hint
        {
            get { return impHint.GetValue(this); }
            set { impHint.SetValue(this, value); }
        }

        #endregion

        #region CehckTitle プロパティ実装

        private static ImplementDependencyProperty<NameLabel, bool> impCehckTitle =
            new ImplementDependencyProperty<NameLabel, bool>(
                nameof(CehckTitle),
                (self, getValue) =>
                {
                    bool flg = getValue(self);
                    self.LabelControl.FontSize = flg ? 16 : 12;
                });

        public static readonly DependencyProperty CehckTitleProperty = impCehckTitle.Regist(false);

        public bool CehckTitle
        {
            get { return impCehckTitle.GetValue(this); }
            set { impCehckTitle.SetValue(this, value); }
        }

        #endregion

        #region UpdateEvent 添付プロパティ実装

        private static ImplementDependencyProperty<NameLabel, Action> impUpdateEvent =
            new ImplementDependencyProperty<NameLabel, Action>(
                nameof(UpdateEvent),
                (self, getValue) =>
                {
                    //Action value = getValue(self);
                });

        public static readonly DependencyProperty UpdateListEventProperty = impUpdateEvent.Regist(null);

        public Action UpdateEvent
        {
            get { return impUpdateEvent.GetValue(this); }
            set { impUpdateEvent.SetValue(this, value); }
        }

        #endregion

        #region CehckBigTitle プロパティ実装

        private static ImplementDependencyProperty<NameLabel, bool> impCehckBigTitle =
            new ImplementDependencyProperty<NameLabel, bool>(
                nameof(CehckBigTitle),
                (self, getValue) =>
                {
                    bool flg = getValue(self);
                    if (flg)
                    {
                        self.FontSize = 20;
                        self.LabelControl.Foreground = Brushes.Gray;
                    }
                });

        public static readonly DependencyProperty CehckBigTitleProperty = impCehckBigTitle.Regist(false);

        public bool CehckBigTitle
        {
            get { return impCehckBigTitle.GetValue(this); }
            set { impCehckBigTitle.SetValue(this, value); }
        }

        #endregion

        #region ReadOnly プロパティ実装

        private static ImplementDependencyProperty<NameLabel, bool> impReadOnly =
            new ImplementDependencyProperty<NameLabel, bool>(
                nameof(ReadOnly),
                (self, getValue) =>
                {
                    self.EditControl.IsReadOnly = getValue(self);
                });
        private bool disposedValue;
        public static readonly DependencyProperty ReadOnlyProperty = impReadOnly.Regist(false);

        public bool ReadOnly
        {
            get { return impReadOnly.GetValue(this); }
            set { impReadOnly.SetValue(this, value); }
        }

        #endregion

        public NameLabel()
        {
            InitializeComponent();
            AssetXML = new _AssetXML<NameLabel>(this);
            LostFocus += ExitEditMode;
        }

        private void LabelControl_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (EditControl.IsReadOnly)
            {
                return;
            }
            if (e.ClickCount < 2)
            {
                // シングルクリックは拒否する

                return;
            }

            LabelControl.Visibility = Visibility.Hidden;
            EditControl.Visibility = Visibility.Visible;

            Dispatcher.BeginInvoke(
                new Action(() =>
                {
                    EditControl.Focus();
                }
            ), DispatcherPriority.Loaded);
        }

        private void EditControl_KeyDown(object sender, KeyEventArgs e)
        {
            if ((Keyboard.GetKeyStates(Key.LeftAlt) & KeyStates.Down) > 0 ||
                       (Keyboard.GetKeyStates(Key.RightAlt) & KeyStates.Down) > 0)
            {
                if ((Keyboard.GetKeyStates(Key.Return) & KeyStates.Down) > 0)
                {
                    EditControl.SelectedText = Environment.NewLine;
                    EditControl.Select(EditControl.SelectionStart + 1, 0);
                    return;
                }
            }

            if (e.Key == Key.Enter)
            {
                EditControl.Text = EditControl.Text.Trim('\r', '\n');
                ExitEditMode();
            }
        }

        private void ExitEditMode(object sender = null, RoutedEventArgs e = null)
        {
            EditControl.Visibility = Visibility.Hidden;
            LabelControl.Visibility = Visibility.Visible;
            LabelString = EditControl.Text;
            UpdateEvent?.Invoke();
        }

        private void LabelControl_MouseEnter(object sender, MouseEventArgs e)
        {
            CommandCanvasList.SetOwnerCursor(Cursors.Hand);
        }

        private void LabelControl_MouseLeave(object sender, MouseEventArgs e)
        {
            CommandCanvasList.SetOwnerCursor(null);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    EditControl.Text = null;
                    UpdateEvent = null;
                    LostFocus -= ExitEditMode;
                    AssetXML?.Dispose();
                    AssetXML = null;
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
