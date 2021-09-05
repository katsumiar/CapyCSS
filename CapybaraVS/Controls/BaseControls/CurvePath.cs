using CapybaraVS.Controls.BaseControls;
using CapybaraVS.Script;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Xml.Serialization;
using CapybaraVS.Control.BaseControls;
using System.Windows.Threading;

namespace CapybaraVS
{
    public class PointIdProvider
    {
        private static Dictionary<int, ITargetPoint> PointList = new Dictionary<int, ITargetPoint>();
        private static Dictionary<int, List<Action<ITargetPoint>>> PointRequest = new Dictionary<int, List<Action<ITargetPoint>>>();
        private static Dictionary<int, int> PointChangeDictionary = new Dictionary<int, int>();
        private static bool IsCheckRequestStart = false;
        private static int _pointId = 0;
        private int pointId = ++_pointId;    // 初期化時に _pointID をインクリメントする
        public int PointId
        {
            get => pointId;
            set
            {
                // セットされる Id を新しい Id に変更しつつ変更記録を残す
                PointChangeDictionary.Add(value, ++_pointId);
                value = _pointId;

                // pointId が変化するのでリスト情報を更新する

                PointList.Add(value, PointList[pointId]);
                PointList.Remove(pointId);
                pointId = value;
                if (IsCheckRequestStart)
                    CheckRequest();
            }
        }
        public static void CheckRequestStart()
        {
            IsCheckRequestStart = true;
            CheckRequest();
        }
        public PointIdProvider(ITargetPoint owner)
        {
            if (owner is null)
                new NotImplementedException();
            PointList.Add(PointId, owner);
        }
        ~PointIdProvider()
        {
            PointList.Remove(PointId);
        }
        public static void RequestGetTargetPoint(int targetId, Action<ITargetPoint> action)
        {
            List<Action<ITargetPoint>> list = null;
            if (PointRequest.ContainsKey(targetId))
            {
                list = PointRequest[targetId];
                list.Add(action);
            }
            else
            {
                list = new List<Action<ITargetPoint>>();
                list.Add(action);
                PointRequest.Add(targetId, list);
            }
        }
        private static void CheckRequest(int targetId)
        {
            if (IsCheckRequestStart == false)
                return;

            if (PointRequest.ContainsKey(targetId))
            {
                List<Action<ITargetPoint>> list = PointRequest[targetId];

                int chgId;
                if (!PointChangeDictionary.ContainsKey(targetId))
                    return;

                chgId = PointChangeDictionary[targetId];

                if (PointList.ContainsKey(chgId))
                {
                    ITargetPoint target = PointList[chgId];
                    foreach (var node in list)
                    {
                        node(target);
                    }
                    list.Clear();
                    PointRequest.Remove(targetId);
                }
            }
        }

        /// <summary>
        /// XMLを展開する前に呼ぶ必要がある
        /// </summary>
        public static void InitCheckRequest()
        {
            IsCheckRequestStart = false;
            PointChangeDictionary.Clear();
            PointRequest.Clear();
        }

        /// <summary>
        /// ターゲットポイント関連の依頼を処理する
        /// </summary>
        public static bool CheckRequest()
        {
            if (IsCheckRequestStart)
            {
                foreach (KeyValuePair<int, List<Action<ITargetPoint>>> kvp in PointRequest)
                {
                    CheckRequest(kvp.Key);
                }
            }
            return PointRequest.Count() != 0;
        }
    }

    //-----------------------------------------------------------------------------------
    /// <summary>
    /// 接続解除インターフェイス
    /// </summary>
    public interface ICloseLink
        : IDisposable
    {
        void CloseLink();
    }

    //-----------------------------------------------------------------------------------
    /// <summary>
    /// 接続ポイントインターフェイス
    /// </summary>
    public interface ITargetPoint
    {
        /// <summary>
        /// 識別子
        /// </summary>
        int TargetPointId { get; set; }

        /// <summary>
        /// 曲線を描画するCanvas
        /// </summary>
        Canvas CurveCanvas { get; }

        /// <summary>
        /// 接続座標を取得
        /// </summary>
        Point TargetPoint { get; }
    }

    //-----------------------------------------------------------------------------------
    /// <summary>
    /// （接続先が保持する）接続元インターフェイス
    /// </summary>
    public interface ICurveLinkRoot 
        : ITargetPoint
        , ICbExecutable
    {
        /// <summary>
        /// 接続元が保持するデータを参照する
        /// </summary>
        /// <returns></returns>
        object RootValue { get; }

        /// <summary>
        /// 曲線の再描画依頼
        /// </summary>
        /// <returns></returns>
        bool RequestBuildCurve(ICurveLinkPoint target, Point? endPos);

        /// <summary>
        /// 曲線の再描画依頼
        /// </summary>
        /// <returns></returns>
        bool RequestBuildCurve();

        /// <summary>
        /// 接続要求
        /// </summary>
        /// <param name="point">リンク先</param>
        /// <returns></returns>
        bool RequestLinkCurve(ICurveLinkPoint point);

        /// <summary>
        /// 保持しているICurveLinkRootの削除依頼
        /// </summary>
        /// <param name="point">削除を要求している接続先</param>
        void RequestRemoveCurveLinkRoot(ICurveLinkPoint point);
    }

    //-----------------------------------------------------------------------------------
    /// <summary>
    /// （接続元が保持する）接続先インターフェイス
    /// </summary>
    public interface ICurveLinkPoint 
        : ITargetPoint
        , ICbExecutable
    {
        /// <summary>
        /// データ参照更新依頼
        /// </summary>
        void UpdateRootValue();

        /// <summary>
        /// 接続要求
        /// </summary>
        /// <param name="point">リンク先</param>
        /// <returns></returns>
        bool RequestLinkCurve(ICurveLinkRoot root);

        /// <summary>
        /// 保持しているICurveLinkRootの削除依頼
        /// </summary>
        /// <param name="root">削除を要求している接続元</param>
        void RequestRemoveCurveLinkPoint(ICurveLinkRoot root);
    }

    //-----------------------------------------------------------------------------------
    /// <summary>
    /// コネクター接続クラス
    /// </summary>
    public class CurveLink 
        : ICloseLink
        , ICbExecutable
    {
        public ICurveLinkPoint curveLinkPoint = null;
        public CurvePath curvePath = null;
        private ICurveLinkRoot _self = null;
        private Canvas _canvas = null;

        public CurveLink(ICurveLinkRoot self, Canvas canvas)
        {
            _self = self;
            _canvas = canvas;
        }

        public void EnterEmphasis()
        {
            curvePath?.EntryMouseEvent();
        }

        public void LeaveEmphasis()
        {
            curvePath?.LeaveMouseEvent();
        }

        public bool RequestLinkCurve(ICurveLinkPoint point)
        {
            if (point is null)
                return false;

            if (curveLinkPoint == point)
                return true;

            // 既存のリンクをカットする
            curveLinkPoint?.RequestRemoveCurveLinkPoint(_self);

            // 新しいリンク先をセットする
            curveLinkPoint = point;

            // 新しい接続先にリンク曲線をつなぐ
            curvePath ??= new CurvePath(_canvas, _self);
            curvePath.TargetEndPoint = point;

            // 仮の終端座標を解除
            curvePath.EndPosition = null;
            // 曲線を構築
            bool ret = RequestBuildCurve(null, null);
            if (ret)
            {
                // リンク曲線が描けたので正式にリンクする

                point.RequestLinkCurve(_self);
            }
            else
            {
                curveLinkPoint = null;
                CloseLink();
            }
            return ret;
        }

        public bool RequestBuildCurve()
        {
            if (curvePath is null)
                return false;
            return curvePath.BuildCurve();
        }

        public bool RequestBuildCurve(ICurveLinkPoint target, Point? endPos)
        {
            if (curvePath is null)
                return false;
            if (curveLinkPoint == target)
            {
                // ターゲットに対して座標を指定する

                curvePath.EndPosition = endPos;
            }
            return curvePath.BuildCurve();
        }

        public void RequestRemoveCurveLinkRoot(ICurveLinkPoint point)
        {
            var ontTime = curveLinkPoint;
            curveLinkPoint = null;
            ontTime?.RequestRemoveCurveLinkPoint(_self);
            CloseLink();
        }

        public void RequestUpdateRootValue()
        {
            curveLinkPoint?.UpdateRootValue();
        }

        public void RequestExecute(List<object> functionStack, DummyArgumentsStack preArgument)
        {
            curveLinkPoint?.RequestExecute(functionStack, preArgument);
        }

        public void CloseLink()
        {
            curveLinkPoint?.RequestRemoveCurveLinkPoint(_self);
            curveLinkPoint = null;
            curvePath?.Dispose();
            curvePath = null;
        }

        public void Dispose()
        {
            CloseLink();
            curveLinkPoint = null;
            curvePath?.Dispose();
            curvePath = null;
            _self = null;
            _canvas = null;
        }
    }

    //-----------------------------------------------------------------------------------
    public abstract class RootCurveLinks
        : ICloseLink
        , ICbExecutable
    {
        #region XML定義
        [XmlRoot(nameof(RootCurveLinks))]
        public class _AssetXML<OwnerClass> : IDisposable
            where OwnerClass : RootCurveLinks
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
                    foreach (var node in LinkList)
                    {
                        PointIdProvider.RequestGetTargetPoint(node,
                            (targetPoint) =>
                            {
                                if (targetPoint is ICurveLinkPoint point)
                                {
                                    self.RequestLinkCurve(point);
                                }
                            });
                    }

                    // 次回の為の初期化
                    self.AssetXML = new _AssetXML<RootCurveLinks>(self);
                };
            }
            public _AssetXML(OwnerClass self)
            {
                WriteAction = () =>
                {
                    LinkList = new List<int>();
                    foreach (var node in self.CurveLinkData)
                    {
                        LinkList.Add(node.curveLinkPoint.TargetPointId);
                    }
                };
            }
            #region 固有定義
            [XmlArrayItem("PointID")]
            public List<int> LinkList { get; set; } = null;

            protected virtual void Dispose(bool disposing)
            {
                if (!disposedValue)
                {
                    if (disposing)
                    {
                        WriteAction = null;
                        ReadAction = null;

                        // 以下、固有定義開放
                        LinkList?.Clear();
                        LinkList = null;
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
        public _AssetXML<RootCurveLinks> AssetXML { get; set; } = null;
        #endregion

        protected List<CurveLink> CurveLinkData { get; set; } = new List<CurveLink>();
        protected ICurveLinkRoot _self = null;
        protected Canvas _canvas = null;

        public RootCurveLinks(ICurveLinkRoot self, Canvas canvas)
        {
            AssetXML = new _AssetXML<RootCurveLinks>(this);
            _self = self;
            _canvas = canvas;
        }

        public int Count => CurveLinkData.Count;

        public void EnterEmphasis()
        {
            foreach (var curvePath in CurveLinkData)
            {
                curvePath.EnterEmphasis();
            }
        }

        public void LeaveEmphasis()
        {
            foreach (var curvePath in CurveLinkData)
            {
                curvePath.LeaveEmphasis();
            }
        }

        public bool RequestBuildCurve(ICurveLinkPoint target, Point? endPos)
        {
            foreach (var curveLink in CurveLinkData)
            {
                curveLink?.RequestBuildCurve(target, endPos);
            }
            return true;
        }

        public bool RequestBuildCurve()
        {
            foreach (var curveLink in CurveLinkData)
                curveLink?.RequestBuildCurve();
            return true;
        }

        public abstract bool RequestLinkCurve(ICurveLinkPoint point);

        public void RequestRemoveCurveLinkRoot(ICurveLinkPoint point)
        {
            var curveLink = CurveLinkData.Find(n => n.curveLinkPoint == point);
            if (curveLink != null)
            {
                curveLink.RequestRemoveCurveLinkRoot(point);
                //curveLink?.Dispose(); TODO これが必要か不要か後で調べる
                CurveLinkData.Remove(curveLink);
            }
        }

        public void RequestUpdateRootValue()
        {
            foreach (var curveLink in CurveLinkData)
                curveLink?.RequestUpdateRootValue();
        }

        public void RequestExecute(List<object> functionStack, DummyArgumentsStack preArgument)
        {
            foreach (var curveLink in CurveLinkData)
                curveLink?.RequestExecute(functionStack, preArgument);
        }

        public void CloseLink()
        {
            while (CurveLinkData.Count != 0)
            {
                CurveLinkData[0]?.CloseLink();
            }
            CurveLinkData.Clear();
        }

        public void Dispose()
        {
            CloseLink();
            foreach (var node in CurveLinkData)
            {
                node.Dispose();
            }
            CurveLinkData.Clear();
            CurveLinkData = null;
            AssetXML?.Dispose();
            AssetXML = null;
            _self = null;
            _canvas = null;
        }
    }

    //-----------------------------------------------------------------------------------
    /// <summary>
    /// RootCurveSingleLinkと共通のインターフェイス（RootCurveLinks）を持つ複数のリンク先を持てるルートコネクター
    /// </summary>
    public class RootCurveMulitiLink 
        : RootCurveLinks
        , ICloseLink
    {
        public RootCurveMulitiLink(ICurveLinkRoot self, Canvas canvas)
            : base(self, canvas) { }
        public override bool RequestLinkCurve(ICurveLinkPoint point)
        {
            if (point is null)
                return false;

            CurveLink curveLink = CurveLinkData.Find(n => n.curveLinkPoint == point);
            if (curveLink is null)
            {
                // まだ未登録なので追加

                curveLink = new CurveLink(_self, _canvas);
                CurveLinkData.Add(curveLink);
            }
            return curveLink.RequestLinkCurve(point);
        }
    }

    //-----------------------------------------------------------------------------------
    /// <summary>
    /// RootCurveMulitiLinkと共通のインターフェイス（RootCurveLinks）を持つ単一のリンク先を持つルートコネクター
    /// </summary>
    public class RootCurveSingleLink
        : RootCurveLinks
        , ICloseLink
    {
        public RootCurveSingleLink(ICurveLinkRoot self, Canvas canvas)
            : base(self, canvas) { }
        public override bool RequestLinkCurve(ICurveLinkPoint point)
        {
            if (point is null)
                return false;

            if (CurveLinkData.Count != 0)
            {
                // 既存のリンクを解除

                CurveLink _curveLink = CurveLinkData[0];
                _curveLink.CloseLink();
            }
            CurveLink curveLink = new CurveLink(_self, _canvas);
            CurveLinkData.Add(curveLink);
            return curveLink.RequestLinkCurve(point);
        }
    }

    //-----------------------------------------------------------------------------------
    public abstract class LinkCurveLinks
        : ICloseLink
        , ICbExecutable
    {
        protected List<ICurveLinkRoot> CurveLinkRootData { get; set; } = new List<ICurveLinkRoot>();

        protected ICurveLinkPoint _self;
        public int Count
        {
            get
            {
                if (CurveLinkRootData is null)
                    return 0;
                return CurveLinkRootData.Count;
            }
        }

        public LinkCurveLinks(ICurveLinkPoint self)
        {
            _self = self;
        }
        public void RequestBuildCurve(ICurveLinkPoint target, Point? endPos)
        {
            foreach (var curveLinkRoot in CurveLinkRootData)
            {
                curveLinkRoot?.RequestBuildCurve(target, endPos);
            }
        }
        public void RequestBuildCurve()
        {
            foreach (var curveLinkRoot in CurveLinkRootData)
            {
                curveLinkRoot?.RequestBuildCurve();
            }
        }
        public abstract bool RequestLinkCurve(ICurveLinkRoot root);
        public void RequestRemoveCurveLinkPoint(ICurveLinkRoot root)
        {
            var curveLinkRoot = CurveLinkRootData.Find(n => n == root);
            if (curveLinkRoot != null)
            {
                curveLinkRoot.RequestRemoveCurveLinkRoot(_self);
            }
            CurveLinkRootData.Remove(curveLinkRoot);
        }

        public void RequestExecute(List<object> functionStack, DummyArgumentsStack preArgument)
        {
            foreach (var curveLinkRoot in CurveLinkRootData)
            {
                if (curveLinkRoot is null)
                    continue;

                functionStack ??= new List<object>();

                if (!functionStack.Contains(curveLinkRoot))
                {
                    // 未実行かスコープ指定（ローカルスコープイメージ）の場合は、実行する

                    curveLinkRoot?.RequestExecute(functionStack, preArgument);
                }
            }
        }

        public void CloseLink()
        {
            while (CurveLinkRootData.Count != 0)
            {
                var curveLinkRoot = CurveLinkRootData[0];
                curveLinkRoot?.RequestRemoveCurveLinkRoot(_self);
            }
            CurveLinkRootData.Clear();
        }

        public void Dispose()
        {
            CloseLink();
            CurveLinkRootData?.Clear();
            CurveLinkRootData = null;
            _self = null;
        }
    }

    //-----------------------------------------------------------------------------------
    /// <summary>
    /// LinkCurveMultiLinksと共通のインターフェイス（LinkCurveLinks）を持つ単一のリンク先を持つルートコネクター
    /// </summary>
    public class LinkCurveSingleLinks
        : LinkCurveLinks
        , ICloseLink
    {
        public LinkCurveSingleLinks(ICurveLinkPoint self)
            : base(self) { }
        public override bool RequestLinkCurve(ICurveLinkRoot root)
        {
            CloseLink();
            CurveLinkRootData.Add(root);
            return true;
        }
    }

    //-----------------------------------------------------------------------------------
    /// <summary>
    /// LinkCurveSingleLinksと共通のインターフェイス（LinkCurveLinks）を持つ単一のリンク先を持つルートコネクター
    /// </summary>
    public class LinkCurveMultiLinks
        : LinkCurveLinks
        , ICloseLink
    {
        public LinkCurveMultiLinks(ICurveLinkPoint self)
            : base(self) { }
        public override bool RequestLinkCurve(ICurveLinkRoot root)
        {
            CurveLinkRootData.Add(root);
            return true;
        }
    }

    //-----------------------------------------------------------------------------------
    /// <summary>
    /// 曲線を描画するクラスです。
    /// </summary>
    public class CurvePath
        : ICloseLink
    {
        private ITargetPoint startTarget = null;
        private Panel drawControl = null;
        private ITargetPoint targetEndPoint = null;
        private bool revert = false;
        private Path ellipsePath = null;
        private Path ellipseBorderPath = null;   // 縁取り用
        private LinePos MyLinePos = null;   // 交差チェック用

        private Brush DEFAULT_COLOR => Brushes.DodgerBlue;
        private Brush DEFAULT_BORDER_COLOR => Brushes.Snow;
        private Brush DEFAULT_FOCUS_COLOR => Brushes.Tomato;

        //-----------------------------------------------------------------------------------
        /// <summary>
        /// 曲線のコントロールを取得
        /// </summary>
        public Path PathContorl { get => ellipsePath; }

        //-----------------------------------------------------------------------------------
        /// <summary>
        /// 曲線描画クラスのコンストラクタです。
        /// </summary>
        /// <param name="drawElement">描画対象のエレメント</param>
        /// <param name="parent">親エレメント。<see langword="null"/>の場合は、描画エレメントを親とする。</param>
        public CurvePath(Panel drawElement, ITargetPoint parent = null)
        {
            Init(drawElement, parent);
        }

        //-----------------------------------------------------------------------------------
        /// <summary>
        /// Pathを作成します。
        /// </summary>
        /// <param name="cmd">Path用コマンド</param>
        /// <param name="color">Pathの色</param>
        /// <returns></returns>
        private Path CreatePath(Geometry cmd, Brush color)
        {
            Path pt = new Path();
            pt.Data = cmd;
            pt.Fill = color;
            return pt;
        }

        //-----------------------------------------------------------------------------------
        /// <summary>
        /// 初期化します。
        /// </summary>
        /// <param name="drawControl">描画対象のパネル（Canvas等）</param>
        /// <param name="startTarget">開始点コントロール</param>
        public void Init(Panel drawControl, ITargetPoint startTarget = null)
        {
            DrawControl = drawControl;
            StartTarget = startTarget;
        }

        //-----------------------------------------------------------------------------------
        /// <summary>
        /// 終点座標を参照します。
        /// <para>※TargetEndPointより優先される</para>
        /// </summary>
        public Point? EndPosition { get; set; } = null;

        //-----------------------------------------------------------------------------------
        /// <summary>
        /// 終点のターゲットエレメントを参照します。
        /// <para>EndPositionが優先される</para>
        /// </summary>
        public ITargetPoint TargetEndPoint
        {
            get => targetEndPoint;
            set
            {
                targetEndPoint = value;

                if (MyLinePos != null)
                {
                    LinePosList.Remove(MyLinePos);
                    MyLinePos = null;
                }
                if (targetEndPoint != null)
                {

                    MyLinePos = new LinePos(startPoint.Value, endPoint.Value);
                    MyLinePos.Update = new Action(() => UpdateColor());
                    LinePosList.Add(MyLinePos);
                    UpdateColors();
                }
            }
        }

        //-----------------------------------------------------------------------------------
        /// <summary>
        /// 曲線の色をデフォルトを戻します。
        /// </summary>
        public void ResetColor()
        {
            if (ellipsePath != null)
            {
                changeLineColor = false;
                ellipsePath.Stroke = CreateMainBrush();
            }
        }

        //-----------------------------------------------------------------------------------
        private bool changeLineColor = false;
        /// <summary>
        /// 曲線の色を変更します。
        /// </summary>
        public Brush LineColor
        {
            get { return ellipsePath.Stroke; }
            set
            {
                if (ellipsePath != null)
                {
                    ellipsePath.Stroke = value;
                    changeLineColor = true;
                }
            }
        }

        //-----------------------------------------------------------------------------------
        /// <summary>
        /// Path コマンドをセットします。
        /// </summary>
        /// <param name="command">コマンド</param>
        private bool set(string command)
        {
            if (command is null)
                return false;

            if (ellipsePath is null)
            {
                try
                {
                    CreateBackgroundPath(command);
                    CreateMainPath(command);
                }
                catch (Exception ex)
                {
                    // キャンバスの取得ミスによって発生する

                    System.Diagnostics.Debug.WriteLine(ex.Message);
                }
            }
            else
            {
                ellipsePath.Data = Geometry.Parse(command);
                ellipseBorderPath.Data = Geometry.Parse(command);
            }
            UpdateColor();
            UpdateColors(); // グループ移動したときに必要になる
            return true;
        }

        //-----------------------------------------------------------------------------------
        private bool IsUpdateColors = false;
        /// <summary>
        /// すべての交差をチェックをし直します。
        /// </summary>
        private void UpdateColors()
        {
            if (IsUpdateColors)
                return;
            IsUpdateColors = true;

            drawControl?.Dispatcher.BeginInvoke(new Action(() =>
                {
                    foreach (var node in LinePosList)
                    {
                        node.Update?.Invoke();
                    }
                    IsUpdateColors = false;
                }), DispatcherPriority.ApplicationIdle);
        }

        //-----------------------------------------------------------------------------------
        private bool IsUpdateColor = false;
        /// <summary>
        /// 交差チェックして交差しているならグラデーションで塗ります。
        /// </summary>
        private void UpdateColor()
        {
            if (ellipsePath is null)
                return;

            if (IsUpdateColor)
                return;
            IsUpdateColor = true;

            drawControl?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (MyLinePos != null)
                {
                    if (startPoint.HasValue)
                        MyLinePos.start = startPoint.Value;
                    if (endPoint.HasValue)
                        MyLinePos.end = endPoint.Value;
                }
                Brush brush = CreateMainBrush();
                if (brush != null)
                    ellipsePath.Stroke = brush;
                IsUpdateColor = false;
            }), DispatcherPriority.ApplicationIdle);
        }

        //-----------------------------------------------------------------------------------
        /// <summary>
        /// 背景を作成します。
        /// </summary>
        /// <param name="command"></param>
        private void CreateBackgroundPath(string command)
        {
            ellipseBorderPath = CreatePath(Geometry.Parse(command), DEFAULT_BORDER_COLOR);
            ellipseBorderPath.Fill = null;
            ellipseBorderPath.Stroke = DEFAULT_BORDER_COLOR;
            ellipseBorderPath.StrokeThickness = 7;
            //ellipsePath2.IsHitTestVisible = false;
            Canvas.SetZIndex(ellipseBorderPath, -1000);

            ellipseBorderPath.MouseEnter += (s, e) =>
            {
                EntryMouseEvent();
            };

            ellipseBorderPath.MouseLeave += (s, e) =>
            {
                LeaveMouseEvent();
            };

            DrawControl.Children.Add(ellipseBorderPath);
        }

        public void LeaveMouseEvent()
        {
            if (ellipsePath is null)
                return; // 保険

            Brush brush = CreateMainBrush(true);
            if (brush != null)
                ellipsePath.Stroke = brush;
        }

        public void EntryMouseEvent()
        {
            if (ellipsePath is null)
                return;
            ellipsePath.Stroke = DEFAULT_FOCUS_COLOR;
        }

        //-----------------------------------------------------------------------------------
        //private static List<LinePos> _LinePosList = new List<LinePos>(); // DrawControl.Parent が IHaveLinePosList インターフェイスを持っていなかった場合用
        private List<LinePos> LinePosListCache = null;
        /// <summary>
        /// 交差チェック用管理リストを参照します。
        /// </summary>
        private List<LinePos> LinePosList
        {
            get
            {
                if (LinePosListCache != null)
                    return LinePosListCache;
                if (DrawControl.Parent is Panel panel)
                {
                    if (panel.Parent is IHaveLinePosList haveLinePosList)
                    {
                        return LinePosListCache = haveLinePosList.LinePosList;
                    }
                }
                Debug.Assert(false);
                return null;
                //return LinePosListCache = _LinePosList;
            }
        }

        //-----------------------------------------------------------------------------------
        /// <summary>
        /// 交差チェックします。
        /// </summary>
        /// <param name="sp"></param>
        /// <param name="ep"></param>
        /// <returns></returns>
        private bool CrossJudge(LinePos sp, LinePos ep)
        {
            return CrossJudge(sp.start, sp.end, ep.start, ep.end);
        }

        private bool CrossJudge(Point sx, Point sy, Point es, Point ey)
        {
            var ch1 = (es.X - ey.X) * (sx.Y - es.Y) + (es.Y - ey.Y) * (es.X - sx.X);
            var ch2 = (es.X - ey.X) * (sy.Y - es.Y) + (es.Y - ey.Y) * (es.X - sy.X);
            if (ch1 * ch2 >= 0)
                return false;

            var ch3 = (sx.X - sy.X) * (es.Y - sx.Y) + (sx.Y - sy.Y) * (sx.X - es.X);
            var ch4 = (sx.X - sy.X) * (ey.Y - sx.Y) + (sx.Y - sy.Y) * (sx.X - ey.X);
            return ch3 * ch4 < 0 && sx.Y < es.Y;
        }

        //-----------------------------------------------------------------------------------
        private void CreateMainPath(string command)
        {
            Brush brush = CreateMainBrush(true);
            ellipsePath = CreatePath(Geometry.Parse(command), brush);
            ellipsePath.Fill = null;
            ellipsePath.Stroke = brush;
            ellipsePath.StrokeThickness = 3;
            ellipsePath.IsHitTestVisible = false;
            Canvas.SetZIndex(ellipsePath, -1000);
            DrawControl.Children.Add(ellipsePath);
        }

        //-----------------------------------------------------------------------------------
        static LinearGradientBrush LinearGradientBrushCashe = null;
        /// <summary>
        /// 交差チェックして色を作成します。
        /// </summary>
        /// <param name="create"></param>
        /// <returns></returns>
        private Brush CreateMainBrush(bool create = false)
        {
            if (changeLineColor)
                return null;

            bool gradiention = false;
            LinePos myLinePos = new LinePos(startPoint.Value, endPoint.Value);
            foreach (var node in LinePosList)
            {
                if (CrossJudge(node, myLinePos))
                {
                    gradiention = true;
                    break;
                }
            }

            if (!create)
            {
                if (ellipsePath is null)
                    return null;    // グループがまるごと消された
                if (ellipsePath.Stroke == DEFAULT_COLOR && !gradiention)
                    return null;    // 変更不要
                if (ellipsePath.Stroke != DEFAULT_COLOR && gradiention)
                    return null;    // 変更不要
            }

            if (gradiention)
            {
                if (LinearGradientBrushCashe is null)
                {
                    LinearGradientBrushCashe = new LinearGradientBrush();
                    LinearGradientBrushCashe.StartPoint = new Point(0, 0.5);
                    LinearGradientBrushCashe.EndPoint = new Point(1, 0.5);
                    LinearGradientBrushCashe.GradientStops.Add(
                        new GradientStop(Colors.DodgerBlue, 0.0));
                    LinearGradientBrushCashe.GradientStops.Add(
                        new GradientStop(Colors.PowderBlue, 0.25));
                    LinearGradientBrushCashe.GradientStops.Add(
                        new GradientStop(Colors.PowderBlue, 0.75));
                    LinearGradientBrushCashe.GradientStops.Add(
                        new GradientStop(Colors.DodgerBlue, 1.0));
                }
                return LinearGradientBrushCashe;
            }
            return DEFAULT_COLOR;
        }

        //-----------------------------------------------------------------------------------
        /// <summary>
        /// 曲線を構築します。
        /// </summary>
        /// <returns>構築が有効だったか</returns>
        public bool BuildCurve()
        {
            return set(buildCurvePath());
        }

        //-----------------------------------------------------------------------------------
        /// <summary>
        /// 計算用座標を取得します。
        /// <para>※位置情報がセットされていればターゲットより優先する</para>
        /// </summary>
        /// <param name="target">ターゲット</param>
        /// <param name="point">位置情報</param>
        /// <returns>有効な座標</returns>
        private Point? getPoint(ITargetPoint target, Point? point)
        {
            try
            {
                //if (DrawControl is null)
                //{
                //    Debug.Assert(false);    // 描画対象が無い
                //    return null;
                //}
                if (point is null)
                {
                    // retPoint が無効値であればターゲットエレメントの座標を基に算出する
                    // TODO インターフェイスを用いて座標を取得する

                    return target.TargetPoint;
                }
            }
            catch (Exception ex)
            {
                // 何らかの接続不具合が原意で呼ばれる場合がある（画面を見て原因を特定する）

                System.Diagnostics.Debug.WriteLine(ex.Message);
                return new Point(0, 0);
            }
            return point;
        }

        //-----------------------------------------------------------------------------------
        /// <summary>
        /// 計算用始点座標を参照します。
        /// </summary>
        private Point? startPoint
        {
            get
            {
                if (startTarget is null)
                {
                    Debug.Assert(false);    // 親の指定が無い
                    return null;
                }
                return getPoint(startTarget, null);
            }
        }

        //-----------------------------------------------------------------------------------
        /// <summary>
        /// 計算用終点座標を参照します。
        /// </summary>
        private Point? endPoint
        {
            get
            {
                return getPoint(TargetEndPoint, EndPosition);
            }
        }

        //-----------------------------------------------------------------------------------
        /// <summary>
        /// 始点の向きを反転します。
        /// </summary>
        public bool Revert
        {
            get => revert;
            set
            {
                revert = value;
            }
        }

        //-----------------------------------------------------------------------------------
        /// <summary>
        /// 描画対象エレメントを参照します。
        /// <para>※登録されていない場合、親エレメント(Parent)を返す</para>
        /// </summary>
        public Panel DrawControl
        {
            get
            {
                if (drawControl is null && StartTarget is Panel pnl)
                    return pnl;
                return drawControl;
            }
            set => drawControl = value;
        }

        //-----------------------------------------------------------------------------------
        /// <summary>
        /// 親エレメントを参照します。
        /// </summary>
        public ITargetPoint StartTarget { get => startTarget; set => startTarget = value; }

        //-----------------------------------------------------------------------------------
        /// <summary>
        /// 曲線の描画を消します。
        /// </summary>
        public void Clear()
        {
            try
            {
                if (MyLinePos != null)
                {
                    if (MyLinePos != null)
                    {
                        LinePosList.Remove(MyLinePos);
                        MyLinePos = null;
                    }
                    UpdateColors();
                }
                DrawControl.Children.Remove(ellipsePath);
                DrawControl.Children.Remove(ellipseBorderPath);
            }
            catch (Exception ex)
            {
                // データの再現に失敗したときにDrawControlがnullになる（画面を見て調査）

                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
            ellipsePath = null;
        }

        //-----------------------------------------------------------------------------------
        /// <summary>
        /// 描画済みの曲線を更新します。
        /// </summary>
        /// <returns>path コマンド</returns>
        private string buildCurvePath()
        {
            Point? sPos = startPoint;
            Point? ePos = endPoint;

            if (sPos == ePos)
                return null;

            if (sPos == null || ePos == null)
                return null;

            int stX = (int)sPos.Value.X;
            int stY = (int)sPos.Value.Y;

            int edX = (int)ePos.Value.X;
            int edY = (int)ePos.Value.Y;

            int startingWaypoint = calcMidpoint(sPos, ePos);
            int endWaypoint = startingWaypoint;

            // TODO Pointをトランスフォームする形式に変更
            if (Revert)
            {
                // 逆方向

                startingWaypoint = calcMidpoint(ePos, sPos) * -1;
            }

            return
                "M" + stX + "," + stY +
                " C" + (stX + startingWaypoint) + "," + stY +
                " " + (edX - endWaypoint) + "," + edY +
                " " + edX + "," + edY;
        }

        //-----------------------------------------------------------------------------------
        /// <summary>
        /// 曲線用の中間点の座標を求めます。
        /// </summary>
        /// <param name="start">始点の座標</param>
        /// <param name="end">終点の座標</param>
        /// <returns>中間点の座標</returns>
        private int calcMidpoint(Point? start, Point? end)
        {
            if (!start.HasValue || !end.HasValue)
                return 0;

            const double maxPointForMVec = 300;
            const double maxPointForPVec = 200;
            const double spaceScaleForX = 230;
            const double spaceScaleForY = 300;
            const double basePoint = 90;        // 最低の線長さ

            double sX = start.Value.X;
            double eX = end.Value.X;

            // ２点間の距離
            double deistance =
                Math.Max(
                    Math.Sqrt(Math.Pow(eX - sX, 2) + Math.Pow(end.Value.Y - start.Value.Y, 2))
                    , 60
                );
            double k = deistance / 60;

            double x = (eX - sX) - basePoint;
            double y = Math.Abs(end.Value.Y - start.Value.Y);
            y = Math.Min(y, spaceScaleForY);

            double m = Math.Abs(Math.Min(x, 0));
            m = Math.Min(m, spaceScaleForX);
            double p = Math.Abs(Math.Max(x, 0));
            p = Math.Min(p, spaceScaleForX);

            double ret =
                (
                    // 逆方向時
                    EasingFunction.OutExp(m, spaceScaleForX, maxPointForMVec, 0) +
                    // 正方向時
                    EasingFunction.OutCirc(p, spaceScaleForX, maxPointForPVec, 0)
                )
                *
                (
                    // 逆方向時適用率
                    EasingFunction.OutExp(y, spaceScaleForY, m, 0) / spaceScaleForX +
                    // 正方向時適用率
                    EasingFunction.InOutSine(y, spaceScaleForY, p, 0) / spaceScaleForX
                )
                +
                (int)k // 最低限の水平横伸び
                ;
            return (int)ret;
        }

        //-----------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------
        #region IDisposable Support
        private bool disposedValue = false; // 重複する呼び出しを検出するには

        public void CloseLink()
        {
            if (MyLinePos != null)
            {
                LinePosList.Remove(MyLinePos);
                MyLinePos = null;
            }
            Clear();
        }

        //-----------------------------------------------------------------------------------
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    CloseLink();
                    startTarget = null;
                    drawControl = null;
                    targetEndPoint = null;
                    ellipsePath = null;
                    ellipseBorderPath = null;
                    MyLinePos = null;
                    LinePosListCache.Clear();
                    LinePosListCache = null;
                }
                // 開放する
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
