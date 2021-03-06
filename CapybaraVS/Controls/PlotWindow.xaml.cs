﻿using CapybaraVS.Controls.BaseControls;
using CapybaraVS.Script.Lib;
using CapyCSS.Controls;
using System;
using System.Collections.Generic;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace CapybaraVS.Controls
{
    /// <summary>
    /// PlotWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class PlotWindow
        : Window
    {
        private double magnification = 1.0;
        private double baseLine = 0;
        private double topSpace = 10;

        public PlotWindow()
        {
            InitializeComponent();
        }

        public enum DrawType
        {
            BrokenLine,
            Line,
            NormalPlot,
            BarGraph,
        }

        public class PlotInfo
        {
            public List<double> list;
            public DrawType drawType;
            public SolidColorBrush solidColorBrush;
            public PlotInfo(List<double> list, DrawType drawType, SolidColorBrush solidColorBrush = null)
            {
                this.list = list;
                this.drawType = drawType;
                this.solidColorBrush = solidColorBrush;
            }
            public PlotInfo(List<int> list, DrawType drawType, SolidColorBrush solidColorBrush = null)
            {
                this.list = list.ConvertAll(new Converter<int, double>((n) => (double)n));
                this.drawType = drawType;
                this.solidColorBrush = solidColorBrush;
            }
        }

        public class Pair
        {
            public double X { get; set; } = 0;
            public double Y { get; set; } = 0;
            public Pair(double x, double y)
            {
                X = x;
                Y = y;
            }
        }

        public void AddPlot(Pair pair, object point, DrawType drawType, SolidColorBrush solidColorBrush, Pair afterPos = null)
        {
            Dispatcher.BeginInvoke(
                new Action(() =>
                    {
                        _AddPlot(pair, point, drawType, solidColorBrush, afterPos);
                    }
                ), DispatcherPriority.Loaded);
        }

        private void _AddPlot(Pair pair, object point, DrawType drawType, SolidColorBrush solidColorBrush, Pair afterPos)
        {
            try
            {
                Canvas target = PlotCanvas;
                PlotPoint.PlotType plotType = PlotPoint.PlotType.None;
                double width = PlotCanvas.ActualWidth;
                double height = (PlotCanvas.ActualHeight - topSpace) * magnification;
                switch (drawType)
                {
                    case DrawType.NormalPlot:
                        plotType = PlotPoint.PlotType.Type1;
                        break;

                    case DrawType.Line:
                        if (afterPos != null)
                        {
                            Line line = new Line()
                            {
                                Stroke = (solidColorBrush is null) ? Brushes.Black : solidColorBrush,
                                StrokeThickness = 1,
                                X1 = width * pair.X,
                                Y1 = height * pair.Y + topSpace,
                                X2 = width * afterPos.X,
                                Y2 = height * afterPos.Y + topSpace,
                            };
                            PlotCanvas.Children.Add(line);
                        }
                        break;

                    case DrawType.BarGraph:
                        if (afterPos != null)
                        {
                            Line line = new Line()
                            {
                                Stroke = (solidColorBrush is null) ? Brushes.Black : solidColorBrush,
                                StrokeThickness = 3,
                                X1 = width * pair.X,
                                Y1 = height * pair.Y + topSpace,
                                X2 = width * afterPos.X,
                                Y2 = height * afterPos.Y + topSpace,
                            };
                            PlotCanvas.Children.Add(line);
                        }
                        break;

                    case DrawType.BrokenLine:
                        if (afterPos != null)
                        {
                            Line line = new Line()
                            {
                                Stroke = (solidColorBrush is null) ? Brushes.Gray : solidColorBrush,
                                StrokeThickness = 1,
                                X1 = width * pair.X,
                                Y1 = height * pair.Y + topSpace,
                                X2 = width * afterPos.X,
                                Y2 = height * afterPos.Y + topSpace,
                                StrokeDashArray = new DoubleCollection(new List<double>() { 1, 2 })
                            };
                            PlotCanvas.Children.Add(line);
                        }
                        break;
                }
                PlotPoint plotPoint = new PlotPoint();
                plotPoint.SetPlotType(plotType, solidColorBrush);
                PlotCanvas.Children.Add(plotPoint);
                Canvas.SetLeft(plotPoint, width * pair.X);
                Canvas.SetTop(plotPoint, height * pair.Y + topSpace);
                Canvas.SetZIndex(plotPoint, (int)drawType);
                plotPoint.ToolTip = point.ToString();
            }
            catch (Exception)
            {
                PlotCanvas.Background = Brushes.Red;
            }
        }

        public void ResetBaseLine(double add, double basePoint)
        {
            double hight = PlotCanvas.ActualHeight - topSpace;
            Numbers.Children.Add(
                new Label()
                {
                    Content = basePoint.ToString(),
                    Margin = new Thickness(0, 0, 0, hight * magnification * (baseLine + add) + 9),
                    VerticalAlignment = VerticalAlignment.Bottom,
                    HorizontalAlignment = HorizontalAlignment.Right
                }
                );

            Lines.Children.Add(
                new Grid()
                {
                    Background = Brushes.Gray,
                    Height = 1,
                    Margin = new Thickness(0, 0, 0, hight * magnification * (baseLine + add)),
                    VerticalAlignment = VerticalAlignment.Bottom
                }
                );
        }

        public static void Create(CommandCanvas OwnerCommandCanvas, string msg, List<PlotInfo> plotInfos, double baseLine = 0)
        {
            List<double> lines = new List<double>() { baseLine };

            if (OwnerCommandCanvas.PlotWindowHoldAction.Enabled)
            {
                // スクリプト実行後に一括で行う

                OwnerCommandCanvas.PlotWindowHoldAction.Add(() => Create(msg, plotInfos, lines));
                return;
            }
            Create(msg, plotInfos, lines);
        }
        public static Task Create(string msg, List<PlotInfo> plotInfos, List<double> baseLines)
        {
            if (plotInfos is null)
                return null;

            if (baseLines is null)
            {
                baseLines = new List<double>() { 0 };
            }

            List<double> allList = new List<double>(baseLines);
            foreach (var plotInfo in plotInfos)
            {
                if (plotInfo.list.Count < 2)
                    continue;

                allList.AddRange(plotInfo.list);
            }

            if (allList.Count == baseLines.Count)
                return null;

#if false   // あまり意味が無さそう
            var tasks2 = new List<Task<double>>();
            tasks2.Add(Task.Run(() => StatisticsLib.MinInTheList(allList)));
            tasks2.Add(Task.Run(() => StatisticsLib.MaxInTheList(allList)));
            var aaaa = Task.WhenAll(tasks2).Result;
            double minPoint = aaaa[0];
            double maxPoint = aaaa[1];
#else
            double minPoint = StatisticsLib.MinInTheList(allList);
            double maxPoint = StatisticsLib.MaxInTheList(allList);
#endif

            List<double> _baseLines = new List<double>(baseLines);
            if (_baseLines.Count == 0)
            {
                _baseLines.Add(minPoint);
                _baseLines.Add(maxPoint);
                _baseLines.Add(0);
            }

            PlotWindow plotWindow = new PlotWindow();
            plotWindow.Owner = CommandCanvasList.OwnerWindow;

            plotWindow.Dispatcher.BeginInvoke(new Action(() =>
            {
                ControlTools.SetWindowPos(plotWindow, null);
            }), DispatcherPriority.Loaded);

            plotWindow.Title = "Plot";
            if (msg != null && msg.Trim() != "")
                plotWindow.Title += ": " + msg;
            plotWindow.Show();

            DrawGuideLine(plotWindow, _baseLines, minPoint, maxPoint);

            var tasks = new List<Task>();
            foreach (var plotInfo in plotInfos)
            {
                var task = Task.Run(() => PlotOut(plotWindow, plotInfo.list, minPoint, maxPoint, plotInfo.drawType, plotInfo.solidColorBrush));
                tasks.Add(task);
            }
            return Task.WhenAll(tasks);
        }

        private static void DrawGuideLine(PlotWindow plotWindow, List<double> baseLines, double minPoint, double maxPoint)
        {
            double dis = Math.Abs(maxPoint - minPoint);
            double addLine = 0;

            if (minPoint - minPoint > 0)
            {
                addLine = Math.Abs(minPoint - minPoint);
                plotWindow.baseLine = addLine / dis;
            }
            plotWindow.ResetBaseLine(0, minPoint);

            foreach (var node in baseLines)
            {
                if (node == minPoint)
                    continue;

                plotWindow.ResetBaseLine((node - minPoint + addLine) * (1 / dis), node);
            }
        }

        private static void PlotOut(PlotWindow plotWindow, List<double> list, double minPoint, double maxPoint, DrawType drawType, SolidColorBrush solidColorBrush)
        {
            List<double> tlist = new List<double>(list);
            tlist.Add(minPoint);
            tlist.Add(maxPoint);
            tlist = StatisticsLib.NormalizeWidthOfValue(tlist);
            tlist = StatisticsLib.NormalizeAbsWidthOfValue(tlist);

            double count = tlist.Count - 3;
            for (int i = 0; i < tlist.Count - 2; i++)
            {
                double node = tlist[i];
                Pair pair = new Pair(i / count, 1 - node);
                if (drawType == DrawType.NormalPlot || i == 0)
                {
                    plotWindow.AddPlot(pair, list[i], drawType, solidColorBrush);
                }
                else if (drawType == DrawType.BarGraph)
                {
                    plotWindow.AddPlot(pair, list[i], drawType, solidColorBrush, new Pair(i / count, 1));
                }
                else
                {
                    plotWindow.AddPlot(pair, list[i], drawType, solidColorBrush, new Pair((i - 1) / count, 1 - tlist[i - 1]));
                }
            }
        }
    }
}
