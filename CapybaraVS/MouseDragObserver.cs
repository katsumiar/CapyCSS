using CapyCSS.Controls.BaseControls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Linq;
using CapyCSS.Controls;

namespace CapyCSS
{
    public interface IMovableControl
    {
        void SetControl(UIElement element);
    }

    class MouseDragObserver 
        : IHaveCommandCanvas
    {
        private IInputElement targetCanvas = null;
        private UIElement target = null;
        private bool isDrug = false;
        private Point dragOffset;
        private Point targetOffset;
        private UIElement captureTarget = null;
        private ObservableCollection<Movable> groupList = null;

        enum MoveRecord
        {
            none,
            //
            MoveCanvas,
            MoveSelectedScriptNode,
            MoveGroupScriptNode,
            MoveScriptNode,
        }

        private MoveRecord moveRecord = MoveRecord.none;

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

        public MouseDragObserver(
            UIElement canvas,
            object sender,
            MouseButtonEventArgs e,
            IMovableCanvas other = null,
            Func<FrameworkElement, bool> isAcceptFunc = null)
        {
            target = null;
            targetCanvas = null;
            captureTarget = null;

            if (isAcceptFunc is null)
                isAcceptFunc = new Func<FrameworkElement, bool>((a) => true);

            // クリック位置を調べて一番上のコントロールを操作対象にする
            VisualTreeHelper.HitTest(canvas, null,
                new HitTestResultCallback(
                    new Func<HitTestResult, HitTestResultBehavior>((hit) => hitCheck(hit, canvas, e, isAcceptFunc))
                ),
                new PointHitTestParameters(e.GetPosition(canvas)));

            if (target == null && other is UIElement element)
            {
                // ターゲットが見つからなかった場合は、other を対象とする

                targetCanvas = element;
                target = element;
                isDrug = true;
                captureTarget = canvas;
            }

            if (targetCanvas != null)
                dragOffset = e.GetPosition(targetCanvas);

            if (!(target is IMovableCanvas movementCanvas))
            {
                if (target != null && target is Movable movable)
                {
                    if (movable.ControlObject is IGroupList haveGroupNode)
                    {
                        // グループ情報を持っている

                        groupList = haveGroupNode.GroupList;
                    }
                }
            }

            captureTarget?.CaptureMouse();
        }

        private HitTestResultBehavior hitCheck(HitTestResult hit, UIElement canvas, MouseButtonEventArgs e, Func<FrameworkElement, bool> isAcceptFunc)
        {
            if (hit.VisualHit is Canvas)
                return HitTestResultBehavior.Stop;

            if (hit.VisualHit is FrameworkElement element)
            {
                FrameworkElement test = VisualTreeHelper.GetParent(element) as FrameworkElement;
                if (test is null)
                    return HitTestResultBehavior.Continue;

                do
                {
                    if (test is IMovableControl)
                    {
                        break;
                    }
                    test = test.Parent as FrameworkElement;
                } while (test != null);
                if (test is null)
                    return HitTestResultBehavior.Continue;

                if (test is IMovableControl && isAcceptFunc(test))
                {
                    targetCanvas = canvas;
                    target = test;
                    targetOffset = e.GetPosition(target);
                    isDrug = true;
                    captureTarget = target;
                    return HitTestResultBehavior.Stop;
                }
            }
            return HitTestResultBehavior.Continue;
        }

        private Point beforePoint;  // 移動前の位置

        public void MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (isDrug)
            {
                Point pos = e.GetPosition(targetCanvas);
                captureTarget?.ReleaseMouseCapture();
                isDrug = false;
                var distance = Math.Sqrt((Math.Pow(beforePoint.X - pos.X, 2) + Math.Pow(beforePoint.Y - pos.Y, 2)));
                if (distance > 0.00001)
                {
                    switch (moveRecord)
                    {
                        case MoveRecord.MoveCanvas:
                            OwnerCommandCanvas.RecordUnDoPoint(CapyCSS.Language.Instance["Help:SYSTEM_COMMAND_MoveCanvas"]);
                            break;
                        case MoveRecord.MoveSelectedScriptNode:
                            OwnerCommandCanvas.RecordUnDoPoint(CapyCSS.Language.Instance["Help:SYSTEM_COMMAND_MoveSelectedScriptNode"]);
                            break;
                        case MoveRecord.MoveGroupScriptNode:
                            OwnerCommandCanvas.RecordUnDoPoint(CapyCSS.Language.Instance["Help:SYSTEM_COMMAND_MoveGroupScriptNode"]);
                            break;
                        case MoveRecord.MoveScriptNode:
                            OwnerCommandCanvas.RecordUnDoPoint(CapyCSS.Language.Instance["Help:SYSTEM_COMMAND_MoveScriptNode"]);
                            break;
                    }
                }
                moveRecord = MoveRecord.none;
            }
        }

        public void MouseMove(object sender, MouseEventArgs e)
        {
            if (isDrug)
            {
                // 移動量を求める
                Point pos = e.GetPosition(targetCanvas);

                bool isGridMove = (Keyboard.GetKeyStates(Key.LeftCtrl) & KeyStates.Down) > 0 ||
                                (Keyboard.GetKeyStates(Key.RightCtrl) & KeyStates.Down) > 0;

                if (target is IMovableCanvas movementCanvas)
                {
                    // 画面全体を動かす

                    Point movePoint = pos;
                    movePoint.X -= dragOffset.X;
                    movePoint.Y -= dragOffset.Y;

                    Matrix matrix = movementCanvas.CanvasRenderTransform.Value;
                    matrix.Translate(movePoint.X, movePoint.Y);
                    movementCanvas.CanvasRenderTransform = new MatrixTransform(matrix);
                    if (moveRecord == MoveRecord.none)
                    {
                        moveRecord = MoveRecord.MoveCanvas;
                        beforePoint = pos;
                    }
                }
                else
                {
                    if (OwnerCommandCanvas.ScriptWorkCanvas.SelectedNodes.Count != 0 && OwnerCommandCanvas.ScriptWorkCanvas.SelectedNodes.Contains(target))
                    {
                        // 選択されたノードを移動

                        Point point = SingleMove(pos, isGridMove);
                        MoveSelectedNode(point, isGridMove, OwnerCommandCanvas.ScriptWorkCanvas.SelectedNodes);
                        if (moveRecord == MoveRecord.none)
                        {
                            moveRecord = MoveRecord.MoveSelectedScriptNode;
                            beforePoint = pos;
                        }
                    }
                    else if (groupList != null)
                    {
                        // グループを移動

                        Point point = SingleMove(pos, isGridMove);
                        MoveSelectedNode(point, isGridMove, groupList);
                        if (moveRecord == MoveRecord.none)
                        {
                            moveRecord = MoveRecord.MoveGroupScriptNode;
                            beforePoint = pos;
                        }
                    }
                    else
                    {
                        // 個別のノードを移動

                        SingleMove(pos, isGridMove);
                        if (moveRecord == MoveRecord.none)
                        {
                            moveRecord = MoveRecord.MoveScriptNode;
                            beforePoint = pos;
                        }
                    }
                }
                dragOffset = pos;
            }
        }

        private double GUID_MOVE_POINT = (BaseWorkCanvas.BACKGROUND_LINE_SPCAE / 2);

        /// <summary>
        /// 単体のノードを移動します。
        /// </summary>
        /// <param name="pos">ドラッグ起点</param>
        /// <param name="isGridMove">true:グリッド線に沿う</param>
        /// <returns>移動量</returns>
        private Point SingleMove(Point pos, bool isGridMove)
        {
            double sx = pos.X - targetOffset.X;
            double sy = pos.Y - targetOffset.Y;

            if (isGridMove)
            {
                sx += OwnerCommandCanvas.ScriptWorkCanvas.CanvasPos.X % GUID_MOVE_POINT;
                sy += OwnerCommandCanvas.ScriptWorkCanvas.CanvasPos.Y % GUID_MOVE_POINT;
                sx -= sx % GUID_MOVE_POINT;
                sy -= sy % GUID_MOVE_POINT;
            }

            double beforeX = Canvas.GetLeft(target);
            double beforeY =  Canvas.GetTop(target);
            Canvas.SetLeft(target, sx);
            Canvas.SetTop(target, sy);
            return new Point(sx - beforeX, sy - beforeY);
        }

        /// <summary>
        /// グループ化されたノードを移動します。
        /// </summary>
        /// <param name="vec">移動量</param>
        /// <param name="isGridMove">true:グリッド線に沿う</param>
        /// <param name="movables">グループ化されたノードリスト</param>
        private void MoveSelectedNode(Point vec, bool isGridMove, ObservableCollection<Movable> movables)
        {
            foreach (var node in movables)
            {
                if (node == target)
                    continue;

                Matrix matrix = node.RenderTransform.Value;
                matrix.Translate(matrix.OffsetX + vec.X, matrix.OffsetY + vec.Y);
                var mt = new MatrixTransform(matrix);
                Point newPos = mt.Transform(new Point(Canvas.GetLeft(node), Canvas.GetTop(node)));

                Canvas.SetLeft(node, newPos.X);
                Canvas.SetTop(node, newPos.Y);
            }
        }
    }
}
