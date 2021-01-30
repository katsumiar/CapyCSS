using System;
using System.Collections.Generic;
using System.Text;

namespace CapybaraVS.Script.Lib
{
    public class StatisticsLib
    {
        [ScriptMethod("Statistics" + "." + "Normalize width of value", "",
            "RS=>StatisticsLib_NormalizeWidthOfValue"//"値の幅のノーマライズ：\n<sample>リストの要素の値の幅を求め、比率を維持したまま0～1.0の間に収めます。"
            )]
        public static List<double> NormalizeWidthOfValue(List<double> sample)
        {
            double maxValue = double.MinValue;
            double minValue = double.MaxValue;
            foreach (var node in sample)
            {
                maxValue = Math.Max(node, maxValue);
                minValue = Math.Min(node, minValue);
            }
            List<double> normalizeList = new List<double>();
            double length = maxValue - minValue;
            if (minValue < 0 && maxValue > 0)
            {
                double absMinValue = minValue * -1;
                foreach (var node in sample)
                {
                    normalizeList.Add(node / length);
                }
            }
            else
            {
                foreach (var node in sample)
                {
                    normalizeList.Add((node - minValue) / length);
                }
            }
            return normalizeList;
        }

        //------------------------------------------------------------------
        [ScriptMethod("Statistics" + "." + "Normalize abs width of value", "",
            "RS=>StatisticsLib_NormalizeAbsWidthOfValue"//"値の幅の絶対値のノーマライズ：\n<sample>リストの要素の値の幅の絶対値を求め、比率を維持したまま0～1.0の間に収めます。"
            )]
        public static List<double> NormalizeAbsWidthOfValue(List<double> sample)
        {
            double maxValue = double.MinValue;
            double minValue = double.MaxValue;
            foreach (var node in sample)
            {
                maxValue = Math.Max(node, maxValue);
                minValue = Math.Min(node, minValue);
            }
            List<double> normalizeList = new List<double>();
            double length = maxValue - minValue;
            foreach (var node in sample)
            {
                normalizeList.Add((node - minValue) / length);
            }
            return normalizeList;
        }

        //------------------------------------------------------------------
        [ScriptMethod("Statistics" + ".Max." + "Max in List<int>", "",
            "RS=>StatisticsLib_MaxInTheList"//"最大値：\n<sample>リスト中の最大の要素の値を参照します。"
            )]
        public static int MaxInTheList(List<int> sample)
        {
            int maxValue = int.MinValue;
            foreach (var node in sample)
            {
                maxValue = Math.Max(node, maxValue);
            }
            return maxValue;
        }

        //------------------------------------------------------------------
        [ScriptMethod("Statistics" + ".Max." + "Max in List<double>", "",
            "RS=>StatisticsLib_MaxInTheList"//"最大値：\n<sample>リスト中の最大の要素の値を参照します。"
            )]
        public static double MaxInTheList(List<double> sample)
        {
            double maxValue = double.MinValue;
            foreach (var node in sample)
            {
                maxValue = Math.Max(node, maxValue);
            }
            return maxValue;
        }

        //------------------------------------------------------------------
        [ScriptMethod("Statistics" + ".Min." + "Min in List<int>", "",
            "RS=>StatisticsLib_MinInTheList"//"最小値：\n<sample>リスト中の最小の要素の値を参照します。"
            )]
        public static int MinInTheList(List<int> sample)
        {
            int minValue = int.MaxValue;
            foreach (var node in sample)
            {
                minValue = Math.Min(node, minValue);
            }
            return minValue;
        }

        //------------------------------------------------------------------
        [ScriptMethod("Statistics" + ".Min." + "Min in List<double>", "",
            "RS=>StatisticsLib_MinInTheList"//"最小値：\n<sample>リスト中の最小の要素の値を参照します。"
            )]
        public static double MinInTheList(List<double> sample)
        {
            double minValue = double.MaxValue;
            foreach (var node in sample)
            {
                minValue = Math.Min(node, minValue);
            }
            return minValue;
        }

        //------------------------------------------------------------------
        [ScriptMethod("Statistics" + "." + "Width of values in list", "",
            "RS=>StatisticsLib_WidthOfValueInTheList"//"距離：\n<sample>リスト中の最小値と最大値の距離を参照します。"
            )]
        public static double WidthOfValueInTheList(List<double> sample)
        {
            double maxValue = double.MinValue;
            double minValue = double.MaxValue;
            foreach (var node in sample)
            {
                maxValue = Math.Max(node, maxValue);
                minValue = Math.Min(node, minValue);
            }
            return maxValue - minValue;
        }

        //------------------------------------------------------------------
        [ScriptMethod("Statistics" + "." + "Squared Difference", "",
            "RS=>StatisticsLib_SquaredDifference"//"二乗差分：\n<n1> と <n2> の二乗差分を求めます。\n※<n1> - <n2> の2乗"
            )]
        public static double SquaredDifference(double n1, double n2)
        {
            return Math.Pow(Math.Abs(n1 - n2), 2);
        }
    }
}
