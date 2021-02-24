using System;
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
        [ScriptMethod(nameof(ListFactory) + "." + nameof(MakeListDouble2), "", "RS=>ListFactory_MakeListDouble2")]
        public static ICollection<double> MakeListDouble2(int num, double value, double step, double power)
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
        [ScriptMethod(nameof(ListFactory) + ".Convert." + nameof(IntToDouble), "", "RS=>ListFactory_IntToDouble")]
        public static ICollection<double> IntToDouble(IEnumerable<int> list)
        {
            return ConvertList(list, (n) => (double)n);
        }

        //------------------------------------------------------------------
        [ScriptMethod(nameof(ListFactory) + ".Convert." + nameof(IntToString), "", "RS=>ListFactory_IntToString")]
        public static ICollection<string> IntToString(IEnumerable<int> list)
        {
            return ConvertList(list, (n) => n.ToString());
        }

        //------------------------------------------------------------------
        [ScriptMethod(nameof(ListFactory) + ".Convert." + nameof(DoubleToInt), "", "RS=>ListFactory_DoubleToInt")]
        public static ICollection<int> DoubleToInt(IEnumerable<double> list)
        {
            return ConvertList(list, (n) => (int)n);
        }

        //------------------------------------------------------------------
        [ScriptMethod(nameof(ListFactory) + ".Convert." + nameof(DoubleToString), "", "RS=>ListFactory_DoubleToString")]
        public static ICollection<string> DoubleToString(IEnumerable<double> list)
        {
            return ConvertList(list, (n) => n.ToString());
        }

        //------------------------------------------------------------------
        [ScriptMethod(nameof(ListFactory) + ".Convert." + nameof(StringToParseIntList), "", "RS=>ListFactory_StringToParseIntList")]
        public static ICollection<int> StringToParseIntList(string value)
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
        [ScriptMethod(nameof(ListFactory) + ".Convert." + nameof(StringToParseDoubleList), "", "RS=>ListFactory_StringToParseDoubleList")]
        public static ICollection<double> StringToParseDoubleList(string value)
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
        [ScriptMethod(nameof(ListFactory) + ".Join." + nameof(JoinIntList), "", "RS=>ListFactory_JoinIntList")]
        public static string JoinIntList(string separator, IEnumerable<int> value)
        {
            return string.Join(separator, value);
        }

        //------------------------------------------------------------------
        [ScriptMethod(nameof(ListFactory) + ".Join." + nameof(JoinDoubleList), "", "RS=>ListFactory_JoinDoubleList")]
        public static string JoinDoubleList(string separator, IEnumerable<double> value)
        {
            return string.Join(separator, value);
        }

        //------------------------------------------------------------------
        [ScriptMethod(nameof(ListFactory) + ".Join." + nameof(JoinStringList), "", "RS=>ListFactory_JoinStringList")]
        public static string JoinStringList(string separator, IEnumerable<string> value)
        {
            return string.Join(separator, value);
        }

        //------------------------------------------------------------------
        [ScriptMethod(nameof(ListFactory) + ".Counter." + nameof(IntListSetCount), "", "RS=>ListFactory_IntListSetCount")]
        public static int IntListSetCount(ICollection<int> sample, int value)
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
        [ScriptMethod(nameof(ListFactory) + ".Counter." + nameof(IntCountInvoke), "", "RS=>ListFactory_IntCountInvoke")]
        public static int IntCountInvoke(
            IEnumerable<int> sample
            , [param: ScriptParam("check predicate f(value)")] Func<int, bool> func)
        {
            int count = 0;
            foreach (var node in sample)
            {
                if (func(node))
                {
                    count++;
                }
            }
            return count;
        }

        //------------------------------------------------------------------
        [ScriptMethod(nameof(ListFactory) + ".Counter." + nameof(DoubleCount), "", "RS=>ListFactory_DoubleCount")]
        public static int DoubleCount(ICollection<double> sample, double value)
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
        [ScriptMethod(nameof(ListFactory) + ".Counter." + nameof(DoubleCountInvoke), "", "RS=>ListFactory_DoubleCountInvoke")]
        public static int DoubleCountInvoke(
            IEnumerable<double> sample
            , [param: ScriptParam("check predicate f(value)")] Func<double, bool> func)
        {
            int count = 0;
            foreach (var node in sample)
            {
                if (func(node))
                {
                    count++;
                }
            }
            return count;
        }

        //------------------------------------------------------------------
        [ScriptMethod(nameof(ListFactory) + ".Counter." + nameof(WordCount), "", "RS=>ListFactory_WordCount")]
        public static int WordCount(ICollection<string> sample, string word)
        {
            int count = 0;
            foreach (var node in sample)
            {
                if (sample.Contains(word))
                {
                    count++;
                }
            }
            return count;
        }

        //------------------------------------------------------------------
        [ScriptMethod(nameof(ListFactory) + ".Counter." + nameof(WordCountInvoke), "", "RS=>ListFactory_WordCountInvoke")]
        public static int WordCountInvoke(
            IEnumerable<string> sample
            , [param: ScriptParam("check predicate f(str)")] Func<string, bool> func)
        {
            int count = 0;
            foreach (var node in sample)
            {
                if (func(node))
                {
                    count++;
                }
            }
            return count;
        }

        //------------------------------------------------------------------
        [ScriptMethod(nameof(ListFactory) + ".Sort." + nameof(SortDescIntList), "", "RS=>ListFactory_SortDescIntList")]
        public static ICollection<int> SortDescIntList(IEnumerable<int> sample)
        {
            var ret = new List<int>(sample);
            ret.Sort();
            return ret;
        }

        //------------------------------------------------------------------
        [ScriptMethod(nameof(ListFactory) + ".Sort." + nameof(SortAscIntList), "", "RS=>ListFactory_SortAscIntList")]
        public static ICollection<int> SortAscIntList(IEnumerable<int> sample)
        {
            var ret = new List<int>(sample);
            ret.Sort((a, b) => b.CompareTo(a));
            return ret;
        }

        //------------------------------------------------------------------
        [ScriptMethod(nameof(ListFactory) + ".Sort." + nameof(SortDescDoubleList), "", "RS=>ListFactory_SortDescDoubleList")]
        public static ICollection<double> SortDescDoubleList(IEnumerable<double> sample)
        {
            var ret = new List<double>(sample);
            ret.Sort();
            return ret;
        }

        //------------------------------------------------------------------
        [ScriptMethod(nameof(ListFactory) + ".Sort." + nameof(SortAscDoubleList), "", "RS=>ListFactory_SortAscDoubleList")]
        public static ICollection<double> SortAscDoubleList(IEnumerable<double> sample)
        {
            var ret = new List<double>(sample);
            ret.Sort((a, b) => b.CompareTo(a));
            return ret;
        }

        //------------------------------------------------------------------
        [ScriptMethod(nameof(ListFactory) + ".Sort." + nameof(SortDescStringList), "", "RS=>ListFactory_SortDescStringList")]
        public static ICollection<string> SortDescStringList(IEnumerable<string> sample)
        {
            var ret = new List<string>(sample);
            ret.Sort();
            return ret;
        }

        //------------------------------------------------------------------
        [ScriptMethod(nameof(ListFactory) + ".Sort." + nameof(SortAscStringList), "", "RS=>ListFactory_SortAscStringList")]
        public static ICollection<string> SortAscStringList(IEnumerable<string> sample)
        {
            var ret = new List<string>(sample);
            ret.Sort((a, b) => b.CompareTo(a));
            return ret;
        }

        //------------------------------------------------------------------
        [ScriptMethod(nameof(ListFactory) + ".Filtering." + nameof(FilteringIntList), "", "RS=>ListFactory_FilteringIntList")]
        public static ICollection<int> FilteringIntList(Func<object, bool> predicate, ICollection<int> sample)
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
        [ScriptMethod(nameof(ListFactory) + ".Filtering." + nameof(FilteringDoubleList), "", "RS=>ListFactory_FilteringDoubleList")]
        public static ICollection<double> FilteringDoubleList(Func<object, bool> predicate, IEnumerable<double> sample)
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
        [ScriptMethod(nameof(ListFactory) + ".Filtering." + nameof(FilteringWordList), "", "RS=>ListFactory_FilteringWordList")]
        public static ICollection<string> FilteringWordList(Func<object, bool> predicate, IEnumerable<string> sample)
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
