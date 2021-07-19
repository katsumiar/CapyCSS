﻿using CapybaraVS.Controls;
using System;
using System.Collections.Generic;
using System.Text;
using static CapybaraVS.Controls.PlotWindow;
using static CbVS.Script.Lib.Graphics;

namespace CapybaraVS.Script.Lib
{
    public class Graph
    {
        const double TickGraph = 1.0 / 20.0;

        [ScriptMethod(nameof(Graph) + "." + nameof(CreateDrawType))]
        public static DrawType CreateDrawType(DrawType type = DrawType.NormalPlot)
        {
            return type;
        }

        //------------------------------------------------------------------
        [ScriptMethod(nameof(Graph) + "." + nameof(MakePlotList))]
        public static ICollection<double> MakePlotList(int count, Func<double, double> func, double step = TickGraph)
        {
            var list = new List<double>();
            if (step <= 0)
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
        [ScriptMethod(nameof(Graph) + "." + nameof(CreateErrorList))]
        public static ICollection<double> CreateErrorList(IEnumerable<double> list, double errorValue)
        {
            return ListFactory.ConvertList(list, (n) => n + errorValue);
        }

        //------------------------------------------------------------------
        [ScriptMethod(nameof(Graph) + "." + nameof(CreatePlotInfo) + "." + nameof(CreatePlotInfo))]
        public static PlotInfo CreatePlotInfo(IEnumerable<double> list, DrawType drawType = DrawType.NormalPlot)
        {
            return new PlotInfo(list, drawType);
        }

        //------------------------------------------------------------------
        [ScriptMethod(nameof(Graph) + "." + nameof(CreatePlotInfo) + "." + nameof(CreatePlotInfo))]
        public static PlotInfo CreatePlotInfo(IEnumerable<double> list, DrawType drawType = DrawType.NormalPlot, BrushColors colors = BrushColors.None)
        {
            return new PlotInfo(list, drawType, CreateBrushes(colors));
        }

        //------------------------------------------------------------------
        [ScriptMethod(nameof(Graph) + "." + nameof(CreatePlotInfo) + "." + nameof(CreatePlotInfo))]
        public static PlotInfo CreatePlotInfo(IEnumerable<int> list, DrawType drawType = DrawType.NormalPlot)
        {
            return new PlotInfo(list, drawType);
        }

        //------------------------------------------------------------------
        [ScriptMethod(nameof(Graph) + "." + nameof(CreatePlotInfo) + "." + nameof(CreatePlotInfo))]
        public static PlotInfo CreatePlotInfo(IEnumerable<int> list, DrawType drawType = DrawType.NormalPlot, BrushColors colors = BrushColors.None)
        {
            return new PlotInfo(list, drawType, CreateBrushes(colors));
        }

        //------------------------------------------------------------------
        [ScriptMethod(nameof(Graph) + "." + nameof(OutPlot))]
        public static void OutPlot(string msg, ICollection<PlotInfo> list, ICollection<double> guideLines = null)
        {
            if (list is null)
                return;

            PlotWindow.Create(msg, list, guideLines);
        }
    }
}
