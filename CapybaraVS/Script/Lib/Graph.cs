using CapybaraVS.Controls;
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
        [ScriptMethod(nameof(Graph) + "." + nameof(MakePlotList), "",
           "RS=>Graph_MakePlotList"
           )]
        public static List<double> MakePlotList(int count, Func<double, double> func, double step = TickGraph)
        {
            List<double> list = new List<double>();
            if (step <= 0)
                return list;
            count--;
            for (double i = 0; i < count; i += step)
            {
                list.Add(func(i));
            }
            list.Add(func(count));  // 精度の歪みが気になるけども...計算しきるために入れておく
            return list;
        }

        //------------------------------------------------------------------
        [ScriptMethod(nameof(Graph) + "." + nameof(CreateErrorList), "",
            "RS=>Graph_CreateErrorList"
            )]
        public static List<double> CreateErrorList(List<double> list, double errorValue)
        {
            return list.ConvertAll(new Converter<double, double>((n) => n + errorValue));
        }

        //------------------------------------------------------------------
        [ScriptMethod(nameof(Graph) + "." + nameof(CreatePlotInfo) + "(List<double>)", nameof(CreatePlotInfo),
            "RS=>Graph_CreatePlotInfo"
            )]
        public static PlotInfo CreatePlotInfo(List<double> list, DrawType drawType = DrawType.NormalPlot)
        {
            return new PlotInfo(list, drawType);
        }

        //------------------------------------------------------------------
        [ScriptMethod(nameof(Graph) + "." + nameof(CreatePlotInfo) + "(List<double>, BrushColors)", nameof(CreatePlotInfo),
            "RS=>Graph_CreatePlotInfo"
            )]
        public static PlotInfo CreatePlotInfo(List<double> list, DrawType drawType = DrawType.NormalPlot, BrushColors colors = BrushColors.None)
        {
            return new PlotInfo(list, drawType, CreateBrushes(colors));
        }

        //------------------------------------------------------------------
        [ScriptMethod(nameof(Graph) + "." + nameof(CreatePlotInfo) + "(List<int>)", nameof(CreatePlotInfo),
            "RS=>Graph_CreatePlotInfo"
            )]
        public static PlotInfo CreatePlotInfo(List<int> list, DrawType drawType = DrawType.NormalPlot)
        {
            return new PlotInfo(list, drawType);
        }

        //------------------------------------------------------------------
        [ScriptMethod(nameof(Graph) + "." + nameof(CreatePlotInfo) + "(List<int>, BrushColors)", nameof(CreatePlotInfo),
            "RS=>Graph_CreatePlotInfo"
            )]
        public static PlotInfo CreatePlotInfo(List<int> list, DrawType drawType = DrawType.NormalPlot, BrushColors colors = BrushColors.None)
        {
            return new PlotInfo(list, drawType, CreateBrushes(colors));
        }

        //------------------------------------------------------------------
        [ScriptMethod(nameof(Graph) + "." + nameof(OutPlot), "",
            "RS=>Graph_OutPlot"
            )]
        public static void OutPlot(string msg, List<PlotInfo> list, List<double> guideLines = null)
        {
            if (list is null)
                return;

            PlotWindow.Create(msg, list, guideLines);
        }
    }
}
