﻿using System;
using System.Collections.Generic;
using System.Text;

namespace CapybaraVS.Script.Lib
{
    public class ListFactory
    {
        [ScriptMethod(nameof(ListFactory) + "." + nameof(MakeListDouble), "", "RS=>ListFactory_MakeListDouble")]
        public static ICollection<double> MakeListDouble(int num, double value, double step)
        {
            var vs = new List<double>();
            while (num-- != 0) 
            {
                vs.Add(value);
                value += step;
            }
            return vs;
        }

        //------------------------------------------------------------------
        [ScriptMethod(nameof(ListFactory) + "." + nameof(MakeListDouble), "", "RS=>ListFactory_MakeListDouble2")]
        public static ICollection<double> MakeListDouble(int num, double value, double step, double power)
        {
            var vs = new List<double>();
            double index = value;
            while (num-- != 0)
            {
                vs.Add(index);
                value += step;
                index = Math.Pow(value, power);
            }
            return vs;
        }

        //------------------------------------------------------------------
        /// <summary>
        /// リストの要素に対して任意の変換を通したリストを作成します。
        /// </summary>
        /// <typeparam name="T1">型</typeparam>
        /// <param name="list">対象のリスト</param>
        /// <param name="converter">任意の変換処理</param>
        /// <returns>変換したリスト</returns>
        public static ICollection<T2> ConvertList<T1, T2>(IEnumerable<T1> list, Converter<T1, T2> converter)
        {
            ICollection<T2> result = new List<T2>();
            foreach (var node in list)
            {
                result.Add(converter(node));
            }
            return result;
        }

        //------------------------------------------------------------------
        [ScriptMethod(nameof(ListFactory) + ".Convert." + nameof(ToDouble), "", "RS=>ListFactory_IntToDouble")]
        public static ICollection<double> ToDouble(IEnumerable<int> list)
        {
            return ConvertList(list, (n) => (double)n);
        }

        //------------------------------------------------------------------
        [ScriptMethod(nameof(ListFactory) + ".Convert." + nameof(ToInteger), "", "RS=>ListFactory_DoubleToInt")]
        public static ICollection<int> ToInteger(IEnumerable<double> list)
        {
            return ConvertList(list, (n) => (int)n);
        }

        //------------------------------------------------------------------
        [ScriptMethod(nameof(ListFactory) + ".Convert." + nameof(ToString), "", "RS=>ListFactory_IntToString")]
        public static ICollection<string> ToString(IEnumerable<int> list)
        {
            return ConvertList(list, (n) => n.ToString());
        }

        //------------------------------------------------------------------
        [ScriptMethod(nameof(ListFactory) + ".Convert." + nameof(ToString), "", "RS=>ListFactory_DoubleToString")]
        public static ICollection<string> ToString(IEnumerable<double> list)
        {
            return ConvertList(list, (n) => n.ToString());
        }

        //------------------------------------------------------------------
        [ScriptMethod(nameof(ListFactory) + ".Parse." + nameof(ParseIntegerCSV), "", "RS=>ListFactory_StringToParseIntList")]
        public static ICollection<int> ParseIntegerCSV(string value)
        {
            var ret = new List<int>();
            var n = value.Split(",");
            foreach (var node in n)
            {
                ret.Add(int.Parse(node));
            }
            return ret;
        }

        //------------------------------------------------------------------
        [ScriptMethod(nameof(ListFactory) + ".Parse." + nameof(ParseDoubleCSV), "", "RS=>ListFactory_StringToParseDoubleList")]
        public static ICollection<double> ParseDoubleCSV(string value)
        {
            var ret = new List<double>();
            var n = value.Split(",");
            foreach (var node in n)
            {
                ret.Add(double.Parse(node));
            }
            return ret;
        }

        //------------------------------------------------------------------
        [ScriptMethod(nameof(ListFactory) + ".Join." + nameof(Join), "", "RS=>ListFactory_JoinIntList")]
        public static string Join(string separator, IEnumerable<int> value)
        {
            return string.Join(separator, value);
        }

        //------------------------------------------------------------------
        [ScriptMethod(nameof(ListFactory) + ".Join." + nameof(Join), "", "RS=>ListFactory_JoinDoubleList")]
        public static string Join(string separator, IEnumerable<double> value)
        {
            return string.Join(separator, value);
        }

        //------------------------------------------------------------------
        [ScriptMethod(nameof(ListFactory) + ".Join." + nameof(Join), "", "RS=>ListFactory_JoinStringList")]
        public static string Join(string separator, IEnumerable<string> value)
        {
            return string.Join(separator, value);
        }

        //------------------------------------------------------------------
        [ScriptMethod(nameof(ListFactory) + ".Counter." + nameof(Counter), "", "RS=>ListFactory_IntListSetCount")]
        public static int Counter(ICollection<int> sample, int value)
        {
            int count = 0;
            foreach (var node in sample)
            {
                if (sample.Contains(value))
                {
                    count++;
                }
            }
            return count;
        }

        //------------------------------------------------------------------
        [ScriptMethod(nameof(ListFactory) + ".Counter." + nameof(Counter), "", "RS=>ListFactory_DoubleCount")]
        public static int Counter(ICollection<double> sample, double value)
        {
            int count = 0;
            foreach (var node in sample)
            {
                if (sample.Contains(value))
                {
                    count++;
                }
            }
            return count;
        }

        //------------------------------------------------------------------
        [ScriptMethod(nameof(ListFactory) + ".Counter." + nameof(Counter), "", "RS=>ListFactory_WordCount")]
        public static int Counter(ICollection<string> sample, string value)
        {
            int count = 0;
            foreach (var node in sample)
            {
                if (sample.Contains(value))
                {
                    count++;
                }
            }
            return count;
        }

        //------------------------------------------------------------------
        [ScriptMethod(nameof(ListFactory) + ".Counter." + nameof(Counter), "", "RS=>ListFactory_IntCountInvoke")]
        public static int Counter(IEnumerable<int> sample, Func<int, bool> predicate)
        {
            int count = 0;
            foreach (var node in sample)
            {
                if (predicate(node))
                {
                    count++;
                }
            }
            return count;
        }

        //------------------------------------------------------------------
        [ScriptMethod(nameof(ListFactory) + ".Counter." + nameof(Counter), "", "RS=>ListFactory_DoubleCountInvoke")]
        public static int Counter(IEnumerable<double> sample, Func<double, bool> predicate)
        {
            int count = 0;
            foreach (var node in sample)
            {
                if (predicate(node))
                {
                    count++;
                }
            }
            return count;
        }

        //------------------------------------------------------------------
        [ScriptMethod(nameof(ListFactory) + ".Counter." + nameof(Counter), "", "RS=>ListFactory_WordCountInvoke")]
        public static int Counter(IEnumerable<string> sample, Func<string, bool> predicate)
        {
            int count = 0;
            foreach (var node in sample)
            {
                if (predicate(node))
                {
                    count++;
                }
            }
            return count;
        }

        //------------------------------------------------------------------
        [ScriptMethod(nameof(ListFactory) + ".Sort.Desc." + nameof(DescSort), "", "RS=>ListFactory_SortDescIntList")]
        public static ICollection<int> DescSort(IEnumerable<int> sample)
        {
            var ret = new List<int>(sample);
            ret.Sort();
            return ret;
        }

        //------------------------------------------------------------------
        [ScriptMethod(nameof(ListFactory) + ".Sort.Desc." + nameof(DescSort), "", "RS=>ListFactory_SortDescDoubleList")]
        public static ICollection<double> DescSort(IEnumerable<double> sample)
        {
            var ret = new List<double>(sample);
            ret.Sort();
            return ret;
        }

        //------------------------------------------------------------------
        [ScriptMethod(nameof(ListFactory) + ".Sort.Desc." + nameof(DescSort), "", "RS=>ListFactory_SortDescStringList")]
        public static ICollection<string> DescSort(IEnumerable<string> sample)
        {
            var ret = new List<string>(sample);
            ret.Sort();
            return ret;
        }

        //------------------------------------------------------------------
        [ScriptMethod(nameof(ListFactory) + ".Sort.Asc." + nameof(AscSort), "", "RS=>ListFactory_SortAscIntList")]
        public static ICollection<int> AscSort(IEnumerable<int> sample)
        {
            var ret = new List<int>(sample);
            ret.Sort((a, b) => b.CompareTo(a));
            return ret;
        }

        //------------------------------------------------------------------
        [ScriptMethod(nameof(ListFactory) + ".Sort.Asc." + nameof(AscSort), "", "RS=>ListFactory_SortAscDoubleList")]
        public static ICollection<double> AscSort(IEnumerable<double> sample)
        {
            var ret = new List<double>(sample);
            ret.Sort((a, b) => b.CompareTo(a));
            return ret;
        }

        //------------------------------------------------------------------
        [ScriptMethod(nameof(ListFactory) + ".Sort.Asc." + nameof(AscSort), "", "RS=>ListFactory_SortAscStringList")]
        public static ICollection<string> AscSort(IEnumerable<string> sample)
        {
            var ret = new List<string>(sample);
            ret.Sort((a, b) => b.CompareTo(a));
            return ret;
        }

        //------------------------------------------------------------------
        [ScriptMethod(nameof(ListFactory) + ".Filtering." + nameof(Filtering), "", "RS=>ListFactory_FilteringIntList")]
        public static ICollection<int> Filtering(IEnumerable<int> sample, Func<int, bool> predicate)
        {
            var ret = new List<int>();
            foreach (var node in sample)
            {
                if (predicate(node))
                    ret.Add(node);
            }
            return ret;
        }

        //------------------------------------------------------------------
        [ScriptMethod(nameof(ListFactory) + ".Filtering." + nameof(Filtering), "", "RS=>ListFactory_FilteringDoubleList")]
        public static ICollection<double> Filtering(IEnumerable<double> sample, Func<double, bool> predicate)
        {
            var ret = new List<double>();
            foreach (var node in sample)
            {
                if (predicate(node))
                    ret.Add(node);
            }
            return ret;
        }

        //------------------------------------------------------------------
        [ScriptMethod(nameof(ListFactory) + ".Filtering." + nameof(Filtering), "", "RS=>ListFactory_FilteringWordList")]
        public static ICollection<string> Filtering(IEnumerable<string> sample, Func<string, bool> predicate)
        {
            var ret = new List<string>();
            foreach (var node in sample)
            {
                if (predicate(node))
                    ret.Add(node);
            }
            return ret;
        }

        //------------------------------------------------------------------
        [ScriptMethod(nameof(ListFactory) + ".Filtering." + nameof(Distinct), "", "RS=>ListFactory_Distinct")]
        public static ICollection<string> Distinct(IEnumerable<string> sample)
        {
            var ret = new List<string>();
            foreach (var node in sample)
            {
                if (!ret.Contains(node))
                {
                    ret.Add(node);
                }
            }
            return ret;
        }
    }
}
