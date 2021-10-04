using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace CapybaraVS.Script.Lib
{
    public class ListFactory
    {
        private const string LIB_NAME = "ListFactory";
        private const string LIB_NAME2 = LIB_NAME + ".Convert";
        private const string LIB_NAME3 = LIB_NAME + ".Parse";
        private const string LIB_NAME5 = LIB_NAME + ".Counter";
        private const string LIB_NAME6 = LIB_NAME + ".Sort";
        private const string LIB_NAME8 = LIB_NAME + ".Filtering";

        //------------------------------------------------------------------
        /// <summary>
        /// リストの要素に対して任意の変換を通したリストを作成します。
        /// </summary>
        /// <typeparam name="T1">型</typeparam>
        /// <param name="list">対象のリスト</param>
        /// <param name="converter">任意の変換処理</param>
        /// <returns>変換したリスト</returns>
        [ScriptMethod(LIB_NAME2)]
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
        /// <summary>
        /// リストを T1 型から T2 型に変換します。
        /// </summary>
        /// <typeparam name="T1">変換前の型</typeparam>
        /// <typeparam name="T2">変換後の型</typeparam>
        /// <param name="list">変換対象のリスト</param>
        /// <returns>変換したリスト</returns>
        [ScriptMethod(LIB_NAME2)]
        public static ICollection<T2> CastConvert<T1, T2>(IEnumerable<T1> list) where T2 : class
        {
            return ConvertList(list, (n) => (T2)(dynamic)n);
        }

        //------------------------------------------------------------------
        [ScriptMethod(LIB_NAME5)]
        public static int Counter<T>(IEnumerable<T> samples, Predicate<T> predicate)
        {
            int count = 0;
            foreach (var sample in samples)
            {
                if (predicate(sample))
                {
                    count++;
                }
            }
            return count;
        }

        //------------------------------------------------------------------
        [ScriptMethod(LIB_NAME5)]
        public static int ConteinsCounter<T>(IEnumerable<T> samples, T value)
        {
            int count = 0;
            foreach (var node in samples)
            {
                if (samples.Contains(value))
                {
                    count++;
                }
            }
            return count;
        }

        //------------------------------------------------------------------
        [ScriptMethod(LIB_NAME6)]
        public static ICollection<T> DescSort<T>(IEnumerable<T> list)
        {
            var ret = new List<T>(list);
            ret.Sort();
            return ret;
        }

        //------------------------------------------------------------------
        [ScriptMethod(LIB_NAME6)]
        public static ICollection<T> AscSort<T>(IEnumerable<T> list) where T : IComparable
        {
            var ret = new List<T>(list);
            ret.Sort((a, b) => b.CompareTo(a));
            return ret;
        }

        //------------------------------------------------------------------
        [ScriptMethod(LIB_NAME8)]
        public static ICollection<T> Filtering<T>(IEnumerable<T> samples, Predicate<T> predicate)
        {
            if (predicate is null)
                return null;
            var ret = new List<T>();
            foreach (var sample in samples)
            {
                if (predicate(sample))
                {
                    ret.Add(sample);
                }
            }
            return ret;
        }

        //------------------------------------------------------------------
        /// <summary>
        /// value から始める num 個のリストを作成します。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="num">リストの個数</param>
        /// <param name="value">開始値</param>
        /// <param name="converter">value を受け取って次の要素に入れる値の変換処理</param>
        /// <returns>作成したリスト</returns>
        [ScriptMethod(LIB_NAME)]
        public static ICollection<T> MakeList<T>(int num, T value, Converter<T, T> converter)
        {
            var vs = new List<T>();
            while (num-- != 0) 
            {
                vs.Add(value);
                value = converter(value);
            }
            return vs;
        }

        //------------------------------------------------------------------
        [ScriptMethod(LIB_NAME)]
        public static string Join<T>(string separator, IEnumerable<T> values)
        {
            return string.Join(separator, values);
        }

        //------------------------------------------------------------------
        [ScriptMethod(LIB_NAME)]
        public static ICollection<T> ParseCSV<T>(string value)
        {
            var ret = new List<T>();
            var n = value.Split(",");
            Type type = typeof(T);
            foreach (var node in n)
            {
                var parse = (T)type.InvokeMember(
                    "Parse",
                    BindingFlags.InvokeMethod,
                    null,
                    type,
                    new object[] { node }
                    );
                ret.Add(parse);
            }
            return ret;
        }

        //------------------------------------------------------------------
        [ScriptMethod(LIB_NAME)]
        public static ICollection<T> Distinct<T>(IEnumerable<T> samples)
        {
            var ret = new List<T>();
            foreach (var sample in samples)
            {
                if (!ret.Contains(sample))
                {
                    ret.Add(sample);
                }
            }
            return ret;
        }

        //------------------------------------------------------------------
        [ScriptMethod(LIB_NAME)]
        public static bool Contains<T>(IEnumerable<T> samples, T target)
        {
            return samples.Contains(target);
        }
    }
}
