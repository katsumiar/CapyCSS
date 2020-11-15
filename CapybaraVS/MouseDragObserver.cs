using CapybaraVS.Control.BaseControls;
using CapybaraVS.Controls.BaseControls;
using CapyCSS.Controls.BaseControls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace CapybaraVS
{
    public interface IMovableControl
    {
        void SetControl(UIElement element);
    }

    class MouseDragObserver
    {
        private IInputElement targetCanvas = null;
        private UIElement target = null;
        private bool isDrug = false;
        private Point dragOffset;
        private Point targetOffset;
        private UIElement captureTarget = null;
        private ObservableCollection<Movable> groupList = null;

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
                // 単体移動時

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

        public void MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (isDrug)
            {
                captureTarget?.ReleaseMouseCapture();
                isDrug = false;
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

                    if (CommandCanvas.ScriptWorkCanvas.SelectedNodes.Count == 0)
                    {
                        // 全体を移動

                        Point movePoint = pos;
                        movePoint.X -= dragOffset.X;
                        movePoint.Y -= dragOffset.Y;

                        Matrix matrix = movementCanvas.CanvasRenderTransform.Value;
                        matrix.Translate(movePoint.X, movePoint.Y);
                        movementCanvas.CanvasRenderTransform = new MatrixTransform(matrix);
                    }
                    else
                    {
                        // 選択されているノードを移動

                        MoveSelectedNode(pos, CommandCanvas.ScriptWorkCanvas.SelectedNodes);
                    }
                }
                else
                {
                    // 個別のノードを移動

                    if (groupList != null)
                    {
                        // グループ情報を持っているのでグループ（自身を含む）で移動する

                        MoveSelectedNode(pos, groupList, false);
                    }
                    else
                    {
                        SingleMove(pos, isGridMove);
                    }
                }
                dragOffset = pos;
            }
        }

        private Point SingleMove(Point pos, bool isGridMove)
        {
            double x = pos.X - targetOffset.X;
            double y = pos.Y - targetOffset.Y;

            if (isGridMove)
            {
                x -= x % 30;
                y -= y % 30;
            }

            Canvas.SetLeft(target, x);
            Canvas.SetTop(target, y);
            return pos;
        }

        private void MoveSelectedNode(Point currentPoint, ObservableCollection<Movable> movables, bool scale = true)
        {
            double sx = currentPoint.X - dragOffset.X;
            double sy = currentPoint.Y - dragOffset.Y;
            
            if (scale)
            {
                double ms = 1.0 / CommandCanvas.ScriptWorkCanvas.CanvasScale;
                sx *= ms;
                sy *= ms;
            }

            foreach (var node in movables)
            {
                Matrix matrix = node.RenderTransform.Value;
                matrix.Translate(matrix.OffsetX + sx, matrix.OffsetY + sy);
                var mt = new MatrixTransform(matrix);
                Point newPos = mt.Transform(new Point(Canvas.GetLeft(node), Canvas.GetTop(node)));
                Canvas.SetLeft(node, newPos.X);
                Canvas.SetTop(node, newPos.Y);
            }
        }
    }
}
