using CapyCSS.Controls;
using CapyCSSattribute;
using CapyCSSbase;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media;
using static CapyCSS.Controls.PlotWindow;

namespace CapyCSS.Script.Lib
{
    [ScriptClass]
    public static class Graph
    {
        private const string LIB_NAME = "Graph";

        const double TickGraph = 1.0 / 20.0;

        [ScriptMethod(LIB_NAME)]
        public static DrawType CreateDrawType(DrawType type = DrawType.NormalPlot)
        {
            return type;
        }

        //------------------------------------------------------------------
        [ScriptMethod(LIB_NAME)]
        public static ICollection<double> MakePlotList(int count, Func<double, double> func, double step = TickGraph)
        {
            var list = new List<double>();
            if (step <= 0 || func is null)
                return list;
            count--;
            for (double i = 0; i < count; i += step)
            {
                list.Add(func(i));
            }
            list.Add(func(count));  // 精度に課題？
            return list;
        }

        //------------------------------------------------------------------
        [ScriptMethod(LIB_NAME)]
        public static ICollection<double> CreateErrorList(IEnumerable<double> list, double errorValue)
        {
            return ListFactory.ConvertList(list, (n) => n + errorValue);
        }

        //------------------------------------------------------------------
        private const string LIB_NAME2 = "Graph.CreatePlotInfo";

        [ScriptMethod(LIB_NAME2)]
        public static PlotInfo CreatePlotInfo(IEnumerable<double> list, DrawType drawType = DrawType.NormalPlot)
        {
            return new PlotInfo(list, drawType);
        }

        //------------------------------------------------------------------
        [ScriptMethod(LIB_NAME2)]
        public static PlotInfo CreatePlotInfo(IEnumerable<double> list, SolidColorBrush brushes, DrawType drawType = DrawType.NormalPlot)
        {
            return new PlotInfo(list, drawType, brushes);
        }

        //------------------------------------------------------------------
        [ScriptMethod(LIB_NAME2)]
        public static PlotInfo CreatePlotInfo(IEnumerable<int> list, DrawType drawType = DrawType.NormalPlot)
        {
            return new PlotInfo(list, drawType);
        }

        //------------------------------------------------------------------
        [ScriptMethod(LIB_NAME2)]
        public static PlotInfo CreatePlotInfo(IEnumerable<int> list, SolidColorBrush brushes, DrawType drawType = DrawType.NormalPlot)
        {
            return new PlotInfo(list, drawType, brushes);
        }

        //------------------------------------------------------------------
        [ScriptMethod(LIB_NAME)]
        public static void OutPlot(string msg, IEnumerable<PlotInfo> list, IEnumerable<double> guideLines = null)
        {
            if (list is null)
                return;

            PlotWindow.Create(msg, list, guideLines);
        }
    }
}
