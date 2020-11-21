using CapybaraVS.Control.BaseControls;
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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Xml.Serialization;

namespace CapyCSS.Controls.BaseControls
{
    public interface IGroupList
    {
        /// <summary>
        /// 所属する移動可能コントロールの一覧を参照します。
        /// </summary>
        ObservableCollection<Movable> GroupList { get; }
    }

    /// <summary>
    /// GroupArea.xaml の相互作用ロジック
    /// </summary>
    public partial class GroupArea
        : UserControl
        , IGroupList
        , IDisplayPriority
        , IHaveCommandCanvas
    {
        #region XML定義
        [XmlRoot(nameof(GroupArea))]
        public class _AssetXML<OwnerClass>
            where OwnerClass : GroupArea
        {
            [XmlIgnore]
            public Action WriteAction = null;
            [XmlIgnore]
            public Action<OwnerClass> ReadAction = null;
            public _AssetXML()
            {
                ReadAction = (self) =>
                {
                    self.TextView.Text = Text;
                    self.Width = Witdh;
                    self.Height = Height;

                    // 次回の為の初期化
                    self.AssetXML = new _AssetXML<GroupArea>(self);
                };
            }
            public _AssetXML(OwnerClass self)
            {
                WriteAction = () =>
                {
                    Text = self.TextView.Text;
                    Witdh = self.ActualWidth;
                    Height = self.ActualHeight;
                };
            }
            #region 固有定義
            public string Text { get; set; } = "";
            public double Witdh { get; set; } = 0;
            public double Height { get; set; } = 0;
            #endregion
        }
        public _AssetXML<GroupArea> AssetXML { get; set; } = null;
        #endregion

        public ObservableCollection<Movable> GroupList
        { 
            get
            {
                ObservableCollection<Movable> groupList = new ObservableCollection<Movable>();
                UIElement parentControl = (Parent as FrameworkElement).Parent as UIElement;
                OwnerCommandCanvas.ScriptWorkCanvas.GetControlWithinRange(
                    new Rect(Canvas.GetLeft(parentControl), Canvas.GetTop(parentControl), Width, Height),
                    ref groupList,
                    false);
                return groupList;
            }
        }

        public int Priority => -500;

        private readonly double minWidth = 36;
        private readonly double minHeight = 36;

        // 高さと幅は、内部で独自に管理する（リアルタイムでの取得だとタイミング上の問題で振れる）
        private double _witdh;
        private double _height;

        private CommandCanvas _OwnerCommandCanvas = null;

        public CommandCanvas OwnerCommandCanvas
        {
            get => _OwnerCommandCanvas;
            set
            {
                if (_OwnerCommandCanvas is null)
                    _OwnerCommandCanvas = value;
            }
        }

        public GroupArea()
        {
            InitializeComponent();
            AssetXML = new _AssetXML<GroupArea>(this);
            MinWidth = minWidth;
            MinHeight = minHeight;

            Dispatcher.BeginInvoke(new Action(() =>
            {
                // 取得タイミングを遅らせないと取得できない

                _witdh = Width;
                _height = Height;
            }), DispatcherPriority.Loaded);

            ResizeAreaNWSE_D.QueryCursor += ShapeQueryCursorNWSE;
            ResizeAreaNESW_D.QueryCursor += ShapeQueryCursorNESW;
            ResizeAreaNWSE_U.QueryCursor += ShapeQueryCursorNWSE;
            ResizeAreaNESW_U.QueryCursor += ShapeQueryCursorNESW;
        }

        enum MouseControlType
        {
            NONE,
            //
            NWSE_D,
            NESW_D,
            NWSE_U,
            NESW_U,
        }

        private MouseControlType sizeControlType = MouseControlType.NONE;
        private Point startPoint;
        private UIElement captureObject = null;
        private UIElement movableControl = null;

        private void Border_MouseDownNWSE_D(object sender, MouseButtonEventArgs e)
        {
            sizeControlType = MouseControlType.NWSE_D;
            initMouseControl(sender, e);
        }

        private void Border_MouseDownNESW_D(object sender, MouseButtonEventArgs e)
        {
            sizeControlType = MouseControlType.NESW_D;
            initMouseControl(sender, e);
        }

        private void Border_MouseDownNWSE_U(object sender, MouseButtonEventArgs e)
        {
            sizeControlType = MouseControlType.NWSE_U;
            initMouseControl(sender, e);
        }

        private void Border_MouseDownNESW_U(object sender, MouseButtonEventArgs e)
        {
            sizeControlType = MouseControlType.NESW_U;
            initMouseControl(sender, e);
        }

        private void initMouseControl(object sender, MouseButtonEventArgs e)
        {
            startPoint = PointToScreen(Mouse.GetPosition(this));
            captureObject = sender as UIElement;
            captureObject.CaptureMouse();
            movableControl = (Parent as FrameworkElement).Parent as UIElement;
            e.Handled = true;
        }

        private void ShapeQueryCursorNWSE(object sender, QueryCursorEventArgs e)
        {
            e.Cursor = Cursors.SizeNWSE;
            e.Handled = true;
        }

        private void ShapeQueryCursorNESW(object sender, QueryCursorEventArgs e)
        {
            e.Cursor = Cursors.SizeNESW;
            e.Handled = true;
        }

        private void Border_MouseMove(object sender, MouseEventArgs e)
        {
            if (sizeControlType == MouseControlType.NONE)
                return;

            Point currentPoint = PointToScreen(Mouse.GetPosition(this));
            double offsetX = currentPoint.X - startPoint.X;
            double offsetY = currentPoint.Y - startPoint.Y;
            startPoint = currentPoint;

            double ms = 1.0 / OwnerCommandCanvas.ScriptWorkCanvas.CanvasScale;
            offsetX *= ms;
            offsetY *= ms;

            switch (sizeControlType)
            {
                case MouseControlType.NWSE_D:
                    {
                        double setWitdh = _witdh + offsetX;
                        double setHeight = _height + offsetY;
                        if (setWitdh >= minWidth + 2)
                        {
                            Width = _witdh = setWitdh;
                        }
                        if (setHeight >= minHeight + 2)
                        {
                            Height = _height = setHeight;
                        }
                    }
                    break;

                case MouseControlType.NESW_D:
                    {
                        double setWitdh = _witdh - offsetX;
                        double setHeight = _height + offsetY;
                        if (offsetX < 0 || (offsetX > 0 && setWitdh >= minWidth + 2))
                        {
                            Width = _witdh = setWitdh;
                            Canvas.SetLeft(movableControl, Canvas.GetLeft(movableControl) + offsetX);
                        }
                        if (setHeight >= minHeight + 2)
                        {
                            Height = _height = setHeight;
                        }
                    }
                    break;

                case MouseControlType.NESW_U:
                    {
                        double setWitdh = _witdh + offsetX;
                        double setHeight = _height - offsetY;
                        if (setWitdh >= minWidth + 2)
                        {
                            Width = _witdh = setWitdh;
                        }
                        if (offsetY < 0 || (offsetY > 0 && setHeight >= minHeight + 2))
                        {
                            Height = _height = setHeight;
                            Canvas.SetTop(movableControl, Canvas.GetTop(movableControl) + offsetY);
                        }
                    }
                    break;

                case MouseControlType.NWSE_U:
                    {
                        double setWitdh = _witdh - offsetX;
                        double setHeight = _height - offsetY;
                        if (offsetX < 0 || (offsetX > 0 && setWitdh >= minWidth + 2))
                        {
                            Width = _witdh = setWitdh;
                            Canvas.SetLeft(movableControl, Canvas.GetLeft(movableControl) + offsetX);
                        }
                        if (offsetY < 0 || (offsetY > 0 && setHeight >= minHeight + 2))
                        {
                            Height = _height = setHeight;
                            Canvas.SetTop(movableControl, Canvas.GetTop(movableControl) + offsetY);
                        }
                    }
                    break;
            }

            e.Handled = true;
        }

        private void EndDrug()
        {
            sizeControlType = MouseControlType.NONE;
            captureObject?.ReleaseMouseCapture();
            captureObject = null;
            movableControl = null;
        }

        private void Border_MouseLeave(object sender, MouseEventArgs e)
        {
            // ※キャプチャしているので意味が無くなっている

            EndDrug();
            e.Handled = true;
        }

        private void Border_MouseUp(object sender, MouseButtonEventArgs e)
        {
            EndDrug();
            e.Handled = true;
        }

        private void ContentControl_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            TextView.Visibility = Visibility.Collapsed;
            Edit.Text = TextView.Text;
            Edit.Visibility = Visibility.Visible;

            Edit.Dispatcher.BeginInvoke(new Action(() =>
            {
                // タイミングを見てフォーカスを当てる

                Edit.Focus();
            }), DispatcherPriority.Loaded);

            Edit.LostFocus += (sender, e) =>
            {
                // フォーカスが外れたら内容が確定される

                TextView.Text = Edit.Text;
                TextView.Visibility = Visibility.Visible;
                Edit.Visibility = Visibility.Collapsed;
            };

            e.Handled = true;
        }
    }
}
