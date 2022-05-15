using CapyCSS.Controls;
using CapyCSS.Controls.BaseControls;
using CapyCSS.Script;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Xml;
using System.Xml.Serialization;

namespace CapyCSS.Controls.BaseControls
{
    public class LinePos
    {
        public Point start;
        public Point end;
        public LinePos(Point sp, Point ed)
        {
            start = sp;
            end = ed;
        }
        public Action Update = null;
    }

    /// <summary>
    /// 配置コントロールの移動可能キャンバス
    /// </summary>
    interface IMovableCanvas
    {
        Transform CanvasRenderTransform { get; set; }
        double CanvasScale { get; }
    }

    interface IDisplayPriority
    {
        int Priority { get; }
    }

    /// <summary>
    /// BaseWorkCanvas.xaml の相互作用ロジック
    /// </summary>
    public partial class BaseWorkCanvas
        : UserControl
        , IMovableCanvas
        , IList
        , INotifyCollectionChanged
        , IAsset
        , IDisposable
        , IHaveCommandCanvas
    {
        public static readonly int DATA_VERSION = 1;

        #region ID管理
        private AssetIdProvider assetIdProvider = null;
        public int AssetId
        {
            get => assetIdProvider.AssetId;
            set { assetIdProvider.AssetId = value; }
        }
        #endregion

        #region XML定義
        [XmlRoot(nameof(BaseWorkCanvas))]
        public class _AssetXML<OwnerClass> : IDisposable
            where OwnerClass : BaseWorkCanvas
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
                    if (DataVersion != DATA_VERSION)
                    {
                        ControlTools.ShowErrorMessage(CapyCSS.Language.Instance["Help:DataVersionError"]);
                        return;
                    }

                    self.AssetId = AssetId;
                    self.EnabelGridLine = EnabelGridLine;
                    self.EnableInfo = EnableInfo;
                    self.ForcedDrawGridLine();
                    if (CanvasRenderTransform != null)
                    {
                        self.CanvasRenderTransform = new MatrixTransform(CanvasRenderTransform.Value);
                    }

                    // 次回の為の初期化
                    self.AssetXML = new _AssetXML<BaseWorkCanvas>(self);
                };
            }
            public _AssetXML(OwnerClass self)
            {
                WriteAction = () =>
                {
                    AssetId = self.AssetId;
                    DataVersion = DATA_VERSION;
                    EnabelGridLine = self.EnabelGridLine;
                    EnableInfo = self.EnableInfo;
                    CanvasRenderTransform = self.CanvasRenderTransform.Value;
                };
            }
            [XmlAttribute("Id")]
            public int AssetId { get; set; } = 0;
            #region 固有定義
            public int DataVersion { get; set; } = 0;
            public bool EnabelGridLine { get; set; } = true;
            public bool EnableInfo { get; set; } = false;
            public double CanvasScale { get; set; } = 1.0;  // 不要になった
            public Matrix? CanvasRenderTransform { get; set; }

            protected virtual void Dispose(bool disposing)
            {
                if (!disposedValue)
                {
                    if (disposing)
                    {
                        WriteAction = null;
                        ReadAction = null;

                        // 以下、固有定義開放
                        CanvasRenderTransform = null;
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
        public _AssetXML<BaseWorkCanvas> AssetXML { get; set; } = null;
        #endregion

        #region 選択コピー用XML定義
        [XmlRoot("CopyAsset")]
        public class _CopyAssetXML<OwnerClass> : IDisposable
            where OwnerClass : BaseWorkCanvas
        {
            [XmlIgnore]
            public Action WriteAction = null;
            [XmlIgnore]
            public Action<OwnerClass,Point?> ReadAction = null;
            private bool disposedValue;

            public _CopyAssetXML()
            {
                ReadAction = (self, relPos) =>
                {
                    self.ClearSelectedObjects();
                    var nodeList = new List<FrameworkElement>();
                    Point leftTopPoint = new Point(double.MaxValue, double.MaxValue);
                    foreach (var node in WorkCanvasAssetList)
                    {
                        Movable movableAsset = new Movable(this);
                        self.Add(movableAsset);
                        movableAsset.OwnerCommandCanvas = self.OwnerCommandCanvas;
                        movableAsset.AssetXML = node;
                        movableAsset.AssetXML.ReadAction?.Invoke(movableAsset);
                        movableAsset.SelectedObject = true;
                        self.SelectedNodes.Add(movableAsset);
                        if (relPos != null)
                        {
                            var el = movableAsset as FrameworkElement;
                            nodeList.Add(el);
                            leftTopPoint.X = Math.Min(leftTopPoint.X, Canvas.GetLeft(el));
                            leftTopPoint.Y = Math.Min(leftTopPoint.Y, Canvas.GetTop(el));
                        }
                    }
                    foreach (var node in nodeList)
                    {
                        Canvas.SetLeft(node, Canvas.GetLeft(node) + relPos.Value.X - leftTopPoint.X);
                        Canvas.SetTop(node, Canvas.GetTop(node) + relPos.Value.Y - leftTopPoint.Y);
                    }

                    // 次回の為の初期化
                    self.CopyAssetXML = new _CopyAssetXML<BaseWorkCanvas>(self);
                };
            }
            public _CopyAssetXML(OwnerClass self)
            {
                WriteAction = () =>
                {
                    List<Movable._AssetXML<Movable>> workList = new List<Movable._AssetXML<Movable>>();
                    foreach (var node in self.SelectedNodes)
                    {
                        if (node is Movable target)
                        {
                            target.AssetXML.WriteAction?.Invoke();
                            workList.Add(target.AssetXML);
                        }
                    }
                    WorkCanvasAssetList = workList;
                    self.ClearSelectedObjects();
                };
            }
            [XmlAttribute("Id")]
            public int AssetId { get; set; } = 0;
            #region 固有定義
            [XmlArrayItem("Asset")]
            public List<Movable._AssetXML<Movable>> WorkCanvasAssetList { get; set; } = null;

            protected virtual void Dispose(bool disposing)
            {
                if (!disposedValue)
                {
                    if (disposing)
                    {
                        WriteAction = null;
                        ReadAction = null;

                        // 以下、固有定義開放
                        CbSTUtils.ForeachDispose(WorkCanvasAssetList);
                        WorkCanvasAssetList = null;
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
        public _CopyAssetXML<BaseWorkCanvas> CopyAssetXML { get; set; } = null;
        #endregion

        /// <summary>
        /// グリッドラインの間隔
        /// </summary>
        public const double BACKGROUND_LINE_SPCAE = 60;

        /// <summary>
        /// グリッドラインの余剰描画数
        /// サイズ拡張時に未描画部分が見えないように余分に描いておく
        /// </summary>
        private const int EXTRA_LINE_COUNT = 3;

        /// <summary>
        /// グリッドラインの余剰描画サイズ
        /// サイズ拡張時に未描画部分が見えないように余分に描いておく
        /// </summary>
        private double EXTRA_LINE_SIZE => BACKGROUND_LINE_SPCAE * (EXTRA_LINE_COUNT + 5) * (1.0 / CanvasScale);

        private double CANVAS_MAX_SCALE = 5.0;
        private double CANVAS_MIN_SCALE = 0.1;

        private int before_GridCanvas_widthCount = 0;
        private int before_GridCanvas_heightCount = 0;

        private bool enabelGridLine = true;
        
        /// <summary>
        /// 重ね合わせキャンバスの一元管理用リスト
        /// </summary>
        private List<Canvas> WorkCanvasList = null;

        /// <summary>
        /// マウスのドラッグ移動管理
        /// </summary>
        private MouseDragObserver mouseDragObserver = null;

        /// <summary>
        /// 矩形選択表示用オブジェクト
        /// </summary>
        private Rectangle rectangle = null;

        /// <summary>
        /// 矩形選択開始位置
        /// </summary>
        private Point startPoint = new Point();

        /// <summary>
        /// 選択中のコントロール
        /// </summary>
        public ObservableCollection<Movable> SelectedNodes = new ObservableCollection<Movable>();

        private CommandCanvas _OwnerCommandCanvas = null;

        public CommandCanvas OwnerCommandCanvas
        {
            get => _OwnerCommandCanvas;
            set
            {
                if (_OwnerCommandCanvas is null)
                    _OwnerCommandCanvas = value;

                Dispatcher.BeginInvoke(new Action(() =>
                {
                    try
                    {
                        if (BackGrountImagePath != null)
                        {
                            // スクリプトキャンバスの背景をセットする

                            CommandCanvasList.SetWorkCanvasBG(BackGrountImagePath);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(nameof(BaseWorkCanvas) + ":" + ex.Message);
                    }
                }), DispatcherPriority.Loaded);
            }
        }

        public BaseWorkCanvas()
        {
            InitializeComponent();
            EnableInfo = false;
            assetIdProvider = new AssetIdProvider(this);
            AssetXML = new _AssetXML<BaseWorkCanvas>(this);
            CopyAssetXML = new _CopyAssetXML<BaseWorkCanvas>(this);

            SizeChanged += new SizeChangedEventHandler(CanvasSizeChanged);

            // Canvasを追加する場合は、WorkCanvasListに追加する
            WorkCanvasList ??= new List<Canvas>();
            WorkCanvasList.Add(GridCanvas);
            WorkCanvasList.Add(ControlsCanvas);
            WorkCanvasList.Add(InfoCanvas);
        }

        /// <summary>
        /// カーブ用キャンバスを参照します。
        /// </summary>
        public Canvas CurveCanvas => ControlsCanvas;

        /// <summary>
        /// 背景イメージへのパスです。
        /// </summary>
        static public string BackGrountImagePath = null;    // TODO static を外す

        /// <summary>
        /// コントロールを返す処理を登録するとクリック時にコントロールをセットします。
        /// </summary>
        public Func<object> SetObjectCommand = null;

        public string SetObjectCommandName = null;

        /// <summary>
        /// SetObjectCommand の終了時処理
        /// </summary>
        public Action SetObjectExitCommand = null;

        /// <summary>
        /// キャンバスの子コントロールを消す
        /// </summary>
        public void Clear()
        {
            for (int i = 0; i < ControlsCanvas.Children.Count; ++i)
            {
                if (ControlsCanvas.Children[i] is IDisposable target)
                    target.Dispose();
            }
            ControlsCanvas.Children.Clear();

            for (int i = 0; i < InfoCanvas.Children.Count; ++i)
            {
                if (InfoCanvas.Children[i] is IDisposable target)
                    target.Dispose();
            }
            InfoCanvas.Children.Clear();

            SetObjectCommand = null;
            SetObjectExitCommand = null;
            SetObjectCommandName = null;

            mouseDragObserver = null;
            SelectedNodes?.Clear();
        }

        private Point DisplayTopLeft
        {
            get
            {
                Point pos = new Point(0, 0);
                return ScreenToCanvasPosition(pos);
            }
        }

        /// <summary>
        /// キャンバスの表示横サイズ
        /// </summary>
        public double DisplayWidth => ActualWidth * (1.0 / CanvasScale);
        /// <summary>
        /// キャンバスの表示縦サイズ
        /// </summary>
        public double DisplayHeight => ActualHeight * (1.0 / CanvasScale);

        /// <summary>
        /// キャンバスのRenderTransform
        /// </summary>
        public Transform CanvasRenderTransform
        {
            set
            {
                foreach (var node in WorkCanvasList)
                    node.RenderTransform = value;
                ForcedDrawGridLine();
            }
            get
            {
                return GridCanvas.RenderTransform;
            }
        }

        /// <summary>
        /// trueならグリッドラインを描画する
        /// </summary>
        public bool EnabelGridLine 
        {
            get => enabelGridLine;
            set 
            { 
                enabelGridLine = value;
                CanvasForcedDrawGridLine();
            }
        }

        /// <summary>
        /// キャンバスのスケール
        /// </summary>
        public double CanvasScale
        { 
            get => CanvasRenderTransform.Value.M11;
            set 
            {
                CanvasAtScale(value);
            }
        }

        /// <summary>
        /// キャンバスの位置
        /// </summary>
        public Point CanvasPos
        {
            get {
                Matrix matrix = CanvasRenderTransform.Value;
                return new Point(matrix.OffsetX, matrix.OffsetY);
            }
        }

        /// <summary>
        /// trueなら情報表示を有効にする
        /// </summary>
        public bool EnableInfo
        {
            set
            {
                if (value)
                {
                    mouseInfo.Visibility = Visibility.Visible;
                }
                else
                {
                    mouseInfo.Visibility = Visibility.Hidden;
                }
            }
            get { return mouseInfo.Visibility == Visibility.Visible; }
        }

        /// <summary>
        /// キャンバスのスケールを設定する
        /// </summary>
        /// <param name="scale">スケール</param>
        /// <param name="pos">中心位置</param>
        public void CanvasAtScale(double scale = 1.0, Point? pos = null)
        {
            if (scale > 1.0 && CanvasScale >= CANVAS_MAX_SCALE)
                return;

            if (scale < 1.0 && CanvasScale <= CANVAS_MIN_SCALE)
                return;

            Matrix matrix = CanvasRenderTransform.Value;

            if (pos.HasValue)
            {
                matrix.ScaleAt(scale, scale, pos.Value.X, pos.Value.Y);
            }
            else
            {
                matrix.Scale(scale, scale);
            }

            CanvasRenderTransform = new MatrixTransform(matrix);
        }

        const double ADJUST_MARGIN_WIDTH = 200;
        const double ADJUST_MARGIN_HEIGHT = 100;

        /// <summary>
        /// 画面に配置されているスクリプト全体を画面に収めるようにスケールをアジャストします。
        /// </summary>
        public void AdjustScriptScale()
        {
            Point leftTop, rightBottom;
            GetScriptNodesRect(out leftTop, out rightBottom);

            double revScale = 1.0 / CanvasScale;
            double width = Math.Abs(leftTop.X - rightBottom.X);
            double height = Math.Abs(leftTop.Y - rightBottom.Y);
            double canvasWidth = ControlsCanvas.ActualWidth * revScale;
            double canvasHeight = ControlsCanvas.ActualHeight * revScale;

            double requestScaleX = canvasWidth / (width + ADJUST_MARGIN_WIDTH);
            double requestScaleY = canvasHeight / (height + ADJUST_MARGIN_HEIGHT);
            double requestScale = Math.Abs(requestScaleX) > Math.Abs(requestScaleY) ? requestScaleY : requestScaleX;
            requestScale = ((requestScale / 0.05) - 0) * 0.05;

            // スケール変更
            CanvasAtScale(requestScale);
        }

        /// <summary>
        /// 画面に配置されているスクリプト全体の範囲を取得します。
        /// </summary>
        /// <param name="leftTop"></param>
        /// <param name="rightBottom"></param>
        private void GetScriptNodesRect(out Point leftTop, out Point rightBottom)
        {
            leftTop = new Point(double.MaxValue, double.MaxValue);
            rightBottom = new Point(double.MinValue, double.MinValue);

            IEnumerable enumerable;
            if (SelectedNodes.Count != 0)
                enumerable = SelectedNodes;
            else
                enumerable = ControlsCanvas.Children;

            foreach (var node in enumerable)
            {
                if (node is Movable)
                {
                    var el = node as FrameworkElement;
                    double x = Canvas.GetLeft(el);
                    double y = Canvas.GetTop(el);
                    leftTop.X = Math.Min(leftTop.X, x);
                    leftTop.Y = Math.Min(leftTop.Y, y);
                    rightBottom.X = Math.Max(rightBottom.X, x + el.ActualWidth);
                    rightBottom.Y = Math.Max(rightBottom.Y, y + el.ActualHeight);
                }
            }
        }

        /// <summary>
        /// 画面に配置されているスクリプト全体を画面に画面左上にアジャストします。
        /// </summary>
        public void AdjustScriptLeftTopPos()
        {
            Point leftTop, rightBottom;
            GetScriptNodesRect(out leftTop, out rightBottom);

            {// 移動
                Point canvasLeftTop = ScreenToCanvasPosition(new Point(0, 0));
                CanvasMovePos(
                    ((canvasLeftTop.X - leftTop.X) + ADJUST_MARGIN_WIDTH / 2.0) * CanvasScale,
                    ((canvasLeftTop.Y - leftTop.Y) + ADJUST_MARGIN_HEIGHT / 2.0) * CanvasScale);
            }
        }

        /// <summary>
        /// 画面に配置されているスクリプト全体を画面に画面中央にアジャストします。
        /// </summary>
        public void AdjustScriptCenterPos()
        {
            Point leftTop, rightBottom;
            GetScriptNodesRect(out leftTop, out rightBottom);

            double halfWidth = Math.Abs(leftTop.X - rightBottom.X) / 2.0;
            double halfHeight = Math.Abs(leftTop.Y - rightBottom.Y) / 2.0;

            double revScale = 1.0 / CanvasScale;
            double halfCanvasWidth = ControlsCanvas.ActualWidth * revScale / 2.0;
            double halfcanvasHeight = ControlsCanvas.ActualHeight * revScale / 2.0;

            {// 移動
                Point canvasLeftTop = ScreenToCanvasPosition(new Point(0, 0));
                CanvasMovePos(
                    ((canvasLeftTop.X - leftTop.X) + (halfCanvasWidth - halfWidth)) * CanvasScale,
                    ((canvasLeftTop.Y - leftTop.Y) + (halfcanvasHeight - halfHeight)) * CanvasScale);
            }
        }

        /// <summary>
        /// キャンバスの表示位置を移動します。
        /// </summary>
        /// <param name="moveX"></param>
        /// <param name="moveY"></param>
        private void CanvasMovePos(double moveX, double moveY)
        {
            Matrix matrix = CanvasRenderTransform.Value;
            matrix.Translate(moveX, moveY);
            CanvasRenderTransform = new MatrixTransform(matrix);
        }

        /// <summary>
        /// キャンバスサイズ変更時に呼ぶ
        /// </summary>
        /// <param name="sender">呼び出したコントロール</param>
        /// <param name="e">イベント</param>
        private void CanvasSizeChanged(object sender, System.EventArgs e)
        {
            Dispatcher.BeginInvoke(
                new Action(() =>
                    {
                        CanvasDrawGridLine();
                    }
                ), DispatcherPriority.Loaded);
        }

        /// <summary>
        /// グリッドラインの再描画
        /// </summary>
        public void DrawGridLine()
        {
            Dispatcher.BeginInvoke(
                new Action(() =>
                    {
                        CanvasDrawGridLine();
                    }
                ), DispatcherPriority.Loaded);
        }

        /// <summary>
        /// グリッドラインの再描画
        /// </summary>
        public void ForcedDrawGridLine()
        {
            Dispatcher.BeginInvoke(
                new Action(() =>
                    {
                        CanvasForcedDrawGridLine();
                    }
                ), DispatcherPriority.Loaded);
        }

        /// <summary>
        /// グリッドラインが有効なとき、強制的にグリッドラインを再描画する
        /// </summary>
        private void CanvasForcedDrawGridLine()
        {
            GridCanvas.Children.Clear();
            if (enabelGridLine)
            {
                int witdhCount = (int)(DisplayWidth / BACKGROUND_LINE_SPCAE);
                int heightCount = (int)(DisplayHeight / BACKGROUND_LINE_SPCAE);

                CanvasDrawGridLine(witdhCount, heightCount);
            }
        }

        private void CanvasDrawGridLine()
        {
            int witdhCount = (int)(DisplayWidth / BACKGROUND_LINE_SPCAE);
            int heightCount = (int)(DisplayHeight / BACKGROUND_LINE_SPCAE);

            bool witdhCountChanged = before_GridCanvas_widthCount != witdhCount;
            bool heightCountChanged = before_GridCanvas_heightCount != heightCount;

            if (witdhCountChanged || heightCountChanged)
            {
                GridCanvas.Children.Clear();
                if (enabelGridLine)
                {
                    CanvasDrawGridLine(witdhCount, heightCount);
                }
            }
        }

        private void CanvasDrawGridLine(int witdhCount, int heightCount)
        {
            double displayHeight = DisplayHeight + EXTRA_LINE_SIZE;
            double displayWidth = DisplayWidth + EXTRA_LINE_SIZE;

            double left = DisplayTopLeft.X;
            double top = DisplayTopLeft.Y;

            left -= left % BACKGROUND_LINE_SPCAE;
            top -= top % BACKGROUND_LINE_SPCAE;

            // 縦のグリッド線
            for (int i = 0; i < witdhCount + EXTRA_LINE_COUNT; ++i)
            {
                Line line = new Line();
                line.Stroke = (Brush)Application.Current.FindResource("ScriptCavasGridLineBrush");
                line.X1 = left + i * BACKGROUND_LINE_SPCAE;
                line.X2 = line.X1;
                line.Y1 = top - EXTRA_LINE_SIZE;
                line.Y2 = top + displayHeight;
                line.StrokeThickness = 0.5;
                GridCanvas.Children.Add(line);
            }
            // 横のグリッド線
            for (int j = 0; j < heightCount + EXTRA_LINE_COUNT; ++j)
            {
                Line line = new Line();
                line.Stroke = (Brush)Application.Current.FindResource("ScriptCavasGridLineBrush");
                line.X1 = left - EXTRA_LINE_SIZE;
                line.X2 = left + displayWidth;
                line.Y1 = top + j * BACKGROUND_LINE_SPCAE;
                line.Y2 = line.Y1;
                line.StrokeThickness = 0.5;
                GridCanvas.Children.Add(line);
            }

            before_GridCanvas_widthCount = witdhCount;
            before_GridCanvas_heightCount = heightCount;
        }

        private void CanvasMouseWheel(object sender, MouseWheelEventArgs e)
        {
            // ホイールの情報から拡縮を計算する
            double scale = 1.0;

            const double SCALSE = 1.05;

            if (e.Delta > 0)
                scale = SCALSE; // 拡大
            else if (e.Delta < 0)
                scale = 1/ SCALSE; // 縮小

            CanvasAtScale(
                scale,
                new Point(
                    e.GetPosition(sender as IInputElement).X,
                    e.GetPosition(sender as IInputElement).Y
                )
            );
        }

        /// <summary>
        /// 実際の座標に合わせる
        /// </summary>
        /// <param name="pos">対象座標</param>
        /// <returns></returns>
        public Point ScreenToCanvasPosition(Point pos)
        {
            // 逆マトリックスでクリック位置を変換する

            Matrix matrix = CanvasRenderTransform.Value;
            matrix.Invert();
            MatrixTransform trans = new MatrixTransform(matrix);
            pos = trans.Transform(pos);

            return pos;
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (CommandCanvasList.IsCursorLock())
                return; // 処理中は禁止

            InfoCanvas.Focus();    // キーイベントを拾うためにフォーカスを当てる

            Grid_MouseUp(sender, e);

            // マウスカーソルの座標で取得する
            Point pos = e.GetPosition(sender as IInputElement);
            pos = ScreenToCanvasPosition(pos);

            startPoint.X = pos.X;
            startPoint.Y = pos.Y;

            if (SetObjectCommand is null)
            {
                if (e.ChangedButton == MouseButton.Middle && e.ButtonState == MouseButtonState.Pressed)
                {
                    OwnerCommandCanvas.ShowCommandMenu(new Point(Mouse.GetPosition(null).X, Mouse.GetPosition(null).Y));
                }
                else if ((Keyboard.GetKeyStates(Key.LeftShift) & KeyStates.Down) > 0 ||
                    (Keyboard.GetKeyStates(Key.RightShift) & KeyStates.Down) > 0)
                {
                    // 選択用矩形描写開始

                    ClearSelectedObjects();

                    InfoCanvas.CaptureMouse();
                    rectangle = new Rectangle()
                    {
                        Stroke = (Brush)Application.Current.FindResource("CanvasRectangleBrush"),
                        StrokeThickness = 3,
                    };

                    Canvas.SetLeft(rectangle, startPoint.X);
                    Canvas.SetTop(rectangle, startPoint.Y);

                    InfoCanvas.Children.Add(rectangle);
                }
                else
                {
                    // ControlsCanvasの子コントロールは、ドラックによる移動対象とする

                    mouseDragObserver = new MouseDragObserver(ControlsCanvas, sender, e, this, null);
                    mouseDragObserver.OwnerCommandCanvas = OwnerCommandCanvas;
                }
            }
            else
            {
                if (e.RightButton == MouseButtonState.Pressed)
                {
                    // コマンドを取り消す

                    ResetCommand();
                }
                else
                {
                    // コマンドが登録されていたので実行する

                    ProcessCommand(pos);
                }
            }
        }

        /// <summary>
        /// 登録されているコマンドを実行します。
        /// </summary>
        /// <param name="setPos">ノードを置く位置</param>
        public void ProcessCommand(Point setPos)
        {
            object obj = SetObjectCommand?.Invoke();
            OwnerCommandCanvas.InstalledMultiRootConnector = null;
            if (obj != null)
            {
                if (obj is UIElement element)
                {
                    // SetObjectCommand がコントロールを返したのでCanvasにセットする

                    // ムーバブルコントロールに入れる
                    Movable movable = new Movable(this);
                    movable.OwnerCommandCanvas = OwnerCommandCanvas;
                    movable.SetControl(element);

                    Add(movable);

                    // レイヤー設定
                    if (element is IDisplayPriority dp)
                    {
                        Canvas.SetZIndex(movable, dp.Priority);
                    }

                    // マウスカーソルの座標でセットする
                    Canvas.SetLeft(movable, setPos.X);
                    Canvas.SetTop(movable, setPos.Y);

                    if (element is MultiRootConnector rootConnector)
                    {
                        // 設置した MultiRootConnector を記録します。

                        OwnerCommandCanvas.InstalledMultiRootConnector = rootConnector;
                    }

                    // 全てのワークに最近使ったスクリプトノードを記録します。
                    OwnerCommandCanvas.CommandCanvasControl.AddScriptCommandRecent(SetObjectCommandName);
                    OwnerCommandCanvas.RecordUnDoPoint(CapyCSS.Language.Instance["Help:SYSTEM_COMMAND_AddScriptNode"]);
                }
            }
            ResetCommand();
        }

        /// <summary>
        /// 最近使ったスクリプトノードに記録します。
        /// </summary>
        public void AddScriptCommandRecent(string name)
        {
            if (name != null)
            {
                // 最近使ったコマンドノード記録に登録する

                var recentNode = OwnerCommandCanvas.CommandMenu.GetRecent();

                if (OwnerCommandCanvas.CommandMenu.FindMenuName(recentNode, name) is null)
                {
                    // 未登録なので登録する

                    recentNode.AddChild(new TreeMenuNode(TreeMenuNode.NodeType.NORMAL, name, OwnerCommandCanvas.CreateImmediateExecutionCanvasCommand(() =>
                    {
                        OwnerCommandCanvas.CommandMenu.ExecuteFindCommand(name);
                    })));
                    OwnerCommandCanvas.CommandMenu.AdjustNumberRecent();
                }
            }
        }

        /// <summary>
        /// 登録されているコマンドを解除します。
        /// </summary>
        public void ResetCommand()
        {
            SetObjectCommand = null;
            SetObjectExitCommand?.Invoke();
            SetObjectExitCommand = null;
        }

        private void Grid_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (rectangle != null)
            {
                InfoCanvas.Children.Remove(rectangle);
                rectangle = null;
                InfoCanvas.ReleaseMouseCapture();
            }

            mouseDragObserver?.MouseUp(sender, e);
            mouseDragObserver = null;
        }

        private void Grid_MouseMove(object sender, MouseEventArgs e)
        {
            Point pos = e.GetPosition(sender as IInputElement);
            pos = ScreenToCanvasPosition(pos);
            
            if (rectangle != null)
            {
                double xpos = pos.X - startPoint.X;
                double ypos = pos.Y - startPoint.Y;

                if (xpos < 0)
                    Canvas.SetLeft(rectangle, startPoint.X + xpos);
                if (ypos < 0)
                    Canvas.SetTop(rectangle, startPoint.Y + ypos);

                rectangle.Width = Math.Abs(xpos);
                rectangle.Height = Math.Abs(ypos);

                Rect rect = new Rect(Canvas.GetLeft(rectangle), Canvas.GetTop(rectangle), rectangle.Width, rectangle.Height);
                GetControlWithinRange(rect, ref SelectedNodes);
            }

            mouseInfo.Content = "MOUSE POS: (" + (int)pos.X + ", " + (int)pos.Y + ")";

            mouseDragObserver?.MouseMove(sender, e);
        }

        /// <summary>
        /// 範囲内の移動可能コントロールを取得する
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="movables"></param>
        public void GetControlWithinRange(Rect rect, ref ObservableCollection<Movable> movables, bool selectMark = true)
        {
            foreach (var node in ControlsCanvas.Children)
            {
                if (node is Movable target)
                {
                    bool isContains = target.IsContains(rect);
                    bool isCollect = movables.Contains(target);

                    if (isContains && !isCollect)
                    {
                        // 選択オブジェクトに追加

                        if (selectMark)
                            target.SelectedObject = true;
                        movables.Add(target);
                    }
                    else if (!isContains && isCollect)
                    {
                        // 選択オブジェクトから外す

                        if (selectMark)
                            target.SelectedObject = false;
                        movables.Remove(target);
                    }
                }
            }
        }

        /// <summary>
        /// 全選択します。
        /// </summary>
        public void SelectAll()
        {
            foreach (var obj in ControlsCanvas.Children)
            {
                if (obj is Movable movable)
                {
                    movable.SelectedObject = true;
                    SelectedNodes.Add(movable);
                }
            }
        }

        /// <summary>
        /// 選択中アセットを解除します。
        /// </summary>
        public void ClearSelectedObjects()
        {
            foreach (var node in SelectedNodes)
            {
                node.SelectedObject = false;
            }
            SelectedNodes.Clear();
        }

        private void Grid_MouseLeave(object sender, MouseEventArgs e)
        {
            mouseInfo.Content = "";
        }

        private void CanvasBase_KeyDown(object sender, KeyEventArgs e)
        {
            // マウスカーソルの座標で取得する
            Point pos = new Point();

            if ((Keyboard.GetKeyStates(Key.LeftCtrl) & KeyStates.Down) > 0 ||
                  (Keyboard.GetKeyStates(Key.RightCtrl) & KeyStates.Down) > 0)
            {
                // Ctrl + key

                switch (e.Key)
                {
                    case Key.A: // 全選択
                        SelectAll();
                        break;

                    case Key.Space: // 選択解除
                        ClearSelectedObjects();
                        break;

                    case Key.C:
                        try
                        {
                            // 選択された作業内容をxmlシリアライズしてクリップボードにコピーする

                            CopySelectedNodesToClipboard();
                        }
                        catch (Exception ex)
                        {
                            ControlTools.ShowErrorMessage(nameof(CanvasBase_KeyDown) + ": [Key.C] " + ex.Message);
                        }
                        break;

                    case Key.V:
                        try
                        {
                            var reader = new StringReader(Clipboard.GetText());
                            string text = reader.ReadToEnd();
                            if (!IsSerializeCBSdata(text))
                            {
                                // テキストの内容に合わせて貼り付ける

                                CreateTextAsset(startPoint, text);
                            }
                            else
                            {
                                // クリップボードのxmlをデシリアライズしてアセットをキャンバスに置く

                                PasteNodesFromClipboard();
                            }
                        }
                        catch (Exception ex)
                        {
                            ControlTools.ShowErrorMessage(nameof(CanvasBase_KeyDown) + ": [Key.V] " + ex.Message);
                        }
                        break;

                    case Key.J: // スクリプト全体の左上位置を画面に合わせる
                        AdjustScriptLeftTopPos();
                        break;

                    case Key.N: // 使用済み
                        break;

                    case Key.Y:
                        OwnerCommandCanvas.ReDo();
                        break;

                    case Key.Z:
                        OwnerCommandCanvas.UnDo();
                        break;
                }
            }
            else if ((Keyboard.GetKeyStates(Key.LeftShift) & KeyStates.Down) > 0 ||
                        (Keyboard.GetKeyStates(Key.RightShift) & KeyStates.Down) > 0)
            {
                // Shift + key



            }
            else
            {
                switch (e.Key)
                {
                    case Key.Space:
                        // コマンドウインドウを表示する

                        OwnerCommandCanvas.ShowCommandMenu(pos);
                        e.Handled = true;
                        break;

                    case Key.J:
                        // スクリプト全体を画面に収める（スクリプトは画面中央に表示する）
                        
                        AdjustScriptScale();
                        AdjustScriptCenterPos();
                        OwnerCommandCanvas.RecordUnDoPoint(CapyCSS.Language.Instance["Help:SYSTEM_COMMAND_AdjustDisplayScript"]);

                        e.Handled = true;
                        break;

                    case Key.Delete:
                        DeleteSelectedNodes();
                        e.Handled = true;
                        break;
                }
            }
        }

        /// <summary>
        /// シリアライズされたCBSデータかどうか判定します。
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private static bool IsSerializeCBSdata(string text)
        {
            return text.Contains("<CopyAsset Id=");
        }

        /// <summary>
        /// クリップボードにあるシリアライズされたノードをデシリアライズしてペーストします。
        /// </summary>
        private void PasteNodesFromClipboard()
        {
            StringReader reader = new StringReader(Clipboard.GetText());
            XmlSerializer serializer = new XmlSerializer(CopyAssetXML.GetType());

            XmlDocument doc = new XmlDocument();
            doc.PreserveWhitespace = true;
            doc.Load(reader);
            XmlNodeReader nodeReader = new XmlNodeReader(doc.DocumentElement);

            object data = (_CopyAssetXML<BaseWorkCanvas>)serializer.Deserialize(nodeReader);
            CopyAssetXML = (_CopyAssetXML<BaseWorkCanvas>)data;

            PointIdProvider.InitCheckRequest();
            CopyAssetXML.ReadAction(this, startPoint);

            OwnerCommandCanvas.RecordUnDoPoint(CapyCSS.Language.Instance["Help:SYSTEM_COMMAND_PasteScript"]);
        }

        /// <summary>
        /// 選択されているノードをシリアライズしてクリップボードにコピーします。
        /// </summary>
        private void CopySelectedNodesToClipboard()
        {
            try
            {
                var writer = new StringWriter();
                var serializer = new XmlSerializer(CopyAssetXML.GetType());
                var namespaces = new XmlSerializerNamespaces();
                namespaces.Add(string.Empty, string.Empty);
                CopyAssetXML.WriteAction();
                serializer.Serialize(writer, CopyAssetXML, namespaces);

                Clipboard.SetDataObject(writer.ToString());
            }
            catch (Exception ex)
            {
                ControlTools.ShowErrorMessage(nameof(CanvasBase_KeyDown) + ": [Key.C] " + ex.Message);
            }
        }

        /// <summary>
        /// 選択されているノードを削除します。
        /// </summary>
        public void DeleteSelectedNodes()
        {
            if (SelectedNodes.Count != 0 &&
                    ControlTools.ShowSelectMessage(
                        CapyCSS.Language.Instance["SYSTEM_ConfirmationDelete"],
                        CapyCSS.Language.Instance["SYSTEM_Confirmation"],
                        MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {
                foreach (var node in SelectedNodes)
                {
                    node.Dispose();
                    ControlsCanvas.Children.Remove(node);
                }
                ClearSelectedObjects();
                OwnerCommandCanvas.RecordUnDoPoint(CapyCSS.Language.Instance["Help:SYSTEM_COMMAND_DeleteNode"]);
            }
        }

        private void InfoCanvas_PreviewDragOver(object sender, DragEventArgs e)
        {
            bool allow = true;
            if (allow)
                e.Effects = System.Windows.DragDropEffects.Copy;
            else
                e.Effects = System.Windows.DragDropEffects.None;
            e.Handled = true;
        }

        private void InfoCanvas_Drop(object sender, DragEventArgs e)
        {
            Point pos = e.GetPosition(sender as IInputElement);
            pos = ScreenToCanvasPosition(pos);

            if (e.Data.GetDataPresent(DataFormats.Text))
            {
                var text = e.Data.GetData(DataFormats.Text) as string;
                if (text != null)
                {
                    CreateTextAsset(pos, text);
                }
                return;
            }

            var dropFiles = e.Data.GetData(System.Windows.DataFormats.FileDrop) as string[];
            if (dropFiles != null)
            {
                foreach (var path in dropFiles)
                {
                    if (System.IO.Path.HasExtension(path))
                    {
                        if (System.IO.Path.GetExtension(path) == ".cbs")
                        {
                            // スクリプトファイルを読み込む

                            OwnerCommandCanvas.CommandCanvasControl.AddLoadContents(path);
                            return;
                        }
                    }

                    CreateTextAsset(pos, path);
                        
                    // 重ならないようにする（値は適当）
                    pos.X += 20;
                    pos.Y += 50;
                }
            }
        }

        private void CreateTextAsset(Point pos, string contents)
        {
            var obj = new MultiRootConnector(this);
            obj.OwnerCommandCanvas = OwnerCommandCanvas;
            obj.AttachParam = new MultiRootConnector.AttachText(contents);

            if (obj is UIElement element)
            {
                // ObjectSetCommandがコントロールを返したのでCanvasにセットする

                // ムーバブルコントロールに入れる
                Movable movable = new Movable(this);
                movable.OwnerCommandCanvas = OwnerCommandCanvas;
                movable.SetControl(element);

                Add(movable);

                movable.OwnerCommandCanvas = OwnerCommandCanvas;    // TODO 必要かチェックする

                // ドロップ位置の座標でセットする
                Canvas.SetLeft(movable, pos.X);
                Canvas.SetTop(movable, pos.Y);

                OwnerCommandCanvas.RecordUnDoPoint(CapyCSS.Language.Instance["Help:SYSTEM_COMMAND_AddScriptNode"]);
            }
        }

        //-------------------------------------------------------------------------------------
        #region IList を実装

        public bool IsFixedSize => false;

        public bool IsReadOnly => false;

        public int Count => ControlsCanvas.Children.Count;

        public bool IsSynchronized => ControlsCanvas.Children.IsSynchronized;

        public object SyncRoot => ControlsCanvas.Children.SyncRoot;

        public object this[int index] 
        {
            get => ControlsCanvas.Children[index];
            set
            {
                if (value is UIElement element)
                {
                    ControlsCanvas.Children[index] = element;
                    sendNotifyCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, element, index));
                }
            }
        }

        public int Add(object value)
        {
            if (value is null)
                return -1;
            if (value is UIElement element)
            {
                ControlsCanvas.Children.Add(element);
                sendNotifyCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, value));
                return IndexOf(element);
            }
            return -1;
        }

        public bool Contains(object value)
        {
            if (value is null)
                return false;
            if (!(value is UIElement element))
                return false;
             
            bool hit = ControlsCanvas.Children.Contains(element);
            if (hit) return true;

            foreach (var obj in ControlsCanvas.Children)
            {
                if (obj is Movable movable)
                {
                    if (movable.ControlObject == element)
                        return true;
                }
            }

            return false;
        }

        public int IndexOf(object value)
        {
            if (value is null)
                return -1;
            if (value is UIElement element)
                return ControlsCanvas.Children.IndexOf(element);
            return -1;
        }

        public void Insert(int index, object value)
        {
            if (value is null)
                return;
            if (value is UIElement element)
            {
                ControlsCanvas.Children.Insert(index, element);
                sendNotifyCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, value, index));
            }
        }

        public void Remove(object value)
        {
            if (value is null)
                return;
            if (value is UIElement element)
            {
                ControlsCanvas.Children.Remove(element);
                sendNotifyCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, value));
            }
        }

        public void RemoveAt(int index)
        {
            if (ControlsCanvas.Children[index] is IDisposable disposable)
            {
                disposable.Dispose();
            }
            ControlsCanvas.Children.RemoveAt(index);
            sendNotifyCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, index));
        }

        public void CopyTo(Array array, int index)
        {
            ControlsCanvas.Children.CopyTo(array, index);
        }

        public IEnumerator GetEnumerator()
        {
            return ControlsCanvas.Children.GetEnumerator();
        }

#endregion

        //-------------------------------------------------------------------------------------
#region INotifyCollectionChangedインターフェイスを実装

        private List<NotifyCollectionChangedEventHandler> NotifyCollectionChangedListeners = new List<NotifyCollectionChangedEventHandler>();

        private void sendNotifyCollectionChanged(NotifyCollectionChangedEventArgs arg)
        {
            foreach (var node in NotifyCollectionChangedListeners)
                node(this, arg);
        }

        event NotifyCollectionChangedEventHandler INotifyCollectionChanged.CollectionChanged
        {
            add => NotifyCollectionChangedListeners.Add(value);
            remove => NotifyCollectionChangedListeners.Remove(value);
        }

#endregion

#region IDisposable Support
        private bool disposedValue = false; // 重複する呼び出しを検出するには

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // ???
                    // MainWindow.Instance.KeyDown -= CanvasBase_KeyDown;
                    ControlsCanvas.Children.Clear();
                    AssetXML?.Dispose();
                    AssetXML = null;
                    CopyAssetXML?.Dispose();
                    CopyAssetXML = null;
                    WorkCanvasList = null;
                    mouseDragObserver = null;
                    rectangle = null;
                    SelectedNodes = null;
                    _OwnerCommandCanvas = null;
                    SetObjectCommand = null;
                    SetObjectCommandName = null;
                    SetObjectExitCommand = null;
                    NotifyCollectionChangedListeners = null;
                }
                disposedValue = true;
            }
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
