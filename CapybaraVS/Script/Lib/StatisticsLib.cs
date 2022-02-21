using CapyCSSattribute;
using System;
using System.Collections.Generic;
using System.Text;

namespace CapyCSS.Script.Lib
{
    [ScriptClass]
    public static class StatisticsLib
    {
        private const string LIB_NAME = "Statistics";

        [ScriptMethod(LIB_NAME)]
        public static ICollection<double> NormalizeWidthOfValue(IEnumerable<double> samples)
        {
            double maxValue = double.MinValue;
            double minValue = double.MaxValue;
            foreach (var sample in samples)
            {
                maxValue = Math.Max(sample, maxValue);
                minValue = Math.Min(sample, minValue);
            }
            var normalizeList = new List<double>();
            double length = maxValue - minValue;
            if (minValue < 0 && maxValue > 0)
            {
                foreach (var sample in samples)
                {
                    normalizeList.Add(sample / length);
                }
            }
            else
            {
                foreach (var sample in samples)
                {
                    normalizeList.Add((sample - minValue) / length);
                }
            }
            return normalizeList;
        }

        //------------------------------------------------------------------
        [ScriptMethod(LIB_NAME)]
        public static ICollection<double> NormalizeAbsWidthOfValue(IEnumerable<double> samples)
        {
            double maxValue = double.MinValue;
            double minValue = double.MaxValue;
            foreach (var sample in samples)
            {
                maxValue = Math.Max(sample, maxValue);
                minValue = Math.Min(sample, minValue);
            }
            var normalizeList = new List<double>();
            double length = maxValue - minValue;
            foreach (var sample in samples)
            {
                normalizeList.Add((sample - minValue) / length);
            }
            return normalizeList;
        }

        //------------------------------------------------------------------
        [ScriptMethod(LIB_NAME + ".Max")]
        public static int Max(IEnumerable<int> samples)
        {
            int maxValue = int.MinValue;
            foreach (var sample in samples)
            {
                maxValue = Math.Max(sample, maxValue);
            }
            return maxValue;
        }

        //------------------------------------------------------------------
        [ScriptMethod(LIB_NAME + ".Max")]
        public static double Max(IEnumerable<double> samples)
        {
            double maxValue = double.MinValue;
            foreach (var sample in samples)
            {
                maxValue = Math.Max(sample, maxValue);
            }
            return maxValue;
        }

        //------------------------------------------------------------------
        [ScriptMethod(LIB_NAME + ".Min")]
        public static int Min(IEnumerable<int> samples)
        {
            int minValue = int.MaxValue;
            foreach (var sample in samples)
            {
                minValue = Math.Min(sample, minValue);
            }
            return minValue;
        }

        //------------------------------------------------------------------
        [ScriptMethod(LIB_NAME + ".Min")]
        public static double Min(IEnumerable<double> samples)
        {
            double minValue = double.MaxValue;
            foreach (var sample in samples)
            {
                minValue = Math.Min(sample, minValue);
            }
            return minValue;
        }

        //------------------------------------------------------------------
        [ScriptMethod(LIB_NAME)]
        public static double WidthOfValue(IEnumerable<double> samples)
        {
            double maxValue = double.MinValue;
            double minValue = double.MaxValue;
            foreach (var sample in samples)
            {
                maxValue = Math.Max(sample, maxValue);
                minValue = Math.Min(sample, minValue);
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
