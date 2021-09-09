using System;
using System.Collections.Generic;
using System.Text;

namespace CapybaraVS.Script.Lib
{
    public class StatisticsLib
    {
        private const string LIB_NAME = "Statistics";

        [ScriptMethod(LIB_NAME)]
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
        [ScriptMethod(LIB_NAME)]
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
        [ScriptMethod(LIB_NAME + ".Max")]
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
        [ScriptMethod(LIB_NAME + ".Max")]
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
        [ScriptMethod(LIB_NAME + ".Min")]
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
        [ScriptMethod(LIB_NAME + ".Min")]
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
        [ScriptMethod(LIB_NAME)]
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
        [ScriptMethod(LIB_NAME)]
        public static double SquaredDifference(double n1, double n2)
        {
            return Math.Pow(Math.Abs(n1 - n2), 2);
        }
    }
}
