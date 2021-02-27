using System;
using System.Collections.Generic;
using System.Text;

namespace CapybaraVS.Script.Lib
{
    public class StatisticsLib
    {
        [ScriptMethod("Statistics" + "." + "Normalize width of value", "", "RS=>StatisticsLib_NormalizeWidthOfValue")]
        public static ICollection<double> NormalizeWidthOfValue(IEnumerable<double> sample)
        {
            double maxValue = double.MinValue;
            double minValue = double.MaxValue;
            foreach (var node in sample)
            {
                maxValue = Math.Max(node, maxValue);
                minValue = Math.Min(node, minValue);
            }
            var normalizeList = new List<double>();
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
        [ScriptMethod("Statistics" + "." + "Normalize abs width of value", "", "RS=>StatisticsLib_NormalizeAbsWidthOfValue")]
        public static ICollection<double> NormalizeAbsWidthOfValue(IEnumerable<double> sample)
        {
            double maxValue = double.MinValue;
            double minValue = double.MaxValue;
            foreach (var node in sample)
            {
                maxValue = Math.Max(node, maxValue);
                minValue = Math.Min(node, minValue);
            }
            var normalizeList = new List<double>();
            double length = maxValue - minValue;
            foreach (var node in sample)
            {
                normalizeList.Add((node - minValue) / length);
            }
            return normalizeList;
        }

        //------------------------------------------------------------------
        [ScriptMethod("Statistics" + ".Max." + "Max in List<int>", "", "RS=>StatisticsLib_MaxInTheList")]
        public static int Max(IEnumerable<int> sample)
        {
            int maxValue = int.MinValue;
            foreach (var node in sample)
            {
                maxValue = Math.Max(node, maxValue);
            }
            return maxValue;
        }

        //------------------------------------------------------------------
        [ScriptMethod("Statistics" + ".Max." + "Max in List<double>", "", "RS=>StatisticsLib_MaxInTheList")]
        public static double Max(IEnumerable<double> sample)
        {
            double maxValue = double.MinValue;
            foreach (var node in sample)
            {
                maxValue = Math.Max(node, maxValue);
            }
            return maxValue;
        }

        //------------------------------------------------------------------
        [ScriptMethod("Statistics" + ".Min." + "Min in List<int>", "", "RS=>StatisticsLib_MinInTheList")]
        public static int Min(IEnumerable<int> sample)
        {
            int minValue = int.MaxValue;
            foreach (var node in sample)
            {
                minValue = Math.Min(node, minValue);
            }
            return minValue;
        }

        //------------------------------------------------------------------
        [ScriptMethod("Statistics" + ".Min." + "Min in List<double>", "", "RS=>StatisticsLib_MinInTheList")]
        public static double Min(IEnumerable<double> sample)
        {
            double minValue = double.MaxValue;
            foreach (var node in sample)
            {
                minValue = Math.Min(node, minValue);
            }
            return minValue;
        }

        //------------------------------------------------------------------
        [ScriptMethod("Statistics" + "." + "Width of values in list", "", "RS=>StatisticsLib_WidthOfValueInTheList")]
        public static double WidthOfValue(IEnumerable<double> sample)
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
        [ScriptMethod("Statistics" + "." + "Squared Difference", "", "RS=>StatisticsLib_SquaredDifference")]
        public static double SquaredDifference(double n1, double n2)
        {
            return Math.Pow(Math.Abs(n1 - n2), 2);
        }
    }
}
