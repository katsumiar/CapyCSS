using CapybaraVS.Control.BaseControls;
using CapybaraVS.Controls.BaseControls;
using System;
using System.Collections.Generic;
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

                if (target is IMovableCanvas movementCanvas)
                {
                    // 画面全体を動かす

                    Point movePoint = pos;
                    movePoint.X -= dragOffset.X;
                    movePoint.Y -= dragOffset.Y;
                    dragOffset = pos;

                    if (CommandCanvas.ScriptWorkCanvas.SelectedContorls.Count == 0)
                    {
                        // 全体を移動

                        Matrix matrix = movementCanvas.CanvasRenderTransform.Value;
                        matrix.Translate(movePoint.X, movePoint.Y);
                        movementCanvas.CanvasRenderTransform = new MatrixTransform(matrix);
                    }
                    else
                    {
                        // 選択されているアセットを移動
                        // TODO ここの移動量計算は間違えているので直す必要がある

                        foreach (var node in CommandCanvas.ScriptWorkCanvas.SelectedContorls)
                        {
                            Matrix matrix = node.RenderTransform.Value;
                            matrix.Translate(movePoint.X, movePoint.Y);
                            var mt = new MatrixTransform(matrix);
                            Point newPos = mt.Transform(new Point(Canvas.GetLeft(node), Canvas.GetTop(node)));
                            Canvas.SetLeft(node, newPos.X);
                            Canvas.SetTop(node, newPos.Y);
                        }
                    }
                }
                else
                {
                    // 個別のアセットを移動

                    double x = pos.X - targetOffset.X;
                    double y = pos.Y - targetOffset.Y;

                    if ((Keyboard.GetKeyStates(Key.LeftCtrl) & KeyStates.Down) > 0 ||
                            (Keyboard.GetKeyStates(Key.RightCtrl) & KeyStates.Down) > 0)
                    {
                        x -= x % 30;
                        y -= y % 30;
                    }

                    Canvas.SetLeft(target, x);
                    Canvas.SetTop(target, y);
                }
            }
        }
    }
}
