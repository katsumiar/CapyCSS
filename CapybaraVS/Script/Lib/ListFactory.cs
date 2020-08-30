using System;
using System.Collections.Generic;
using System.Text;

namespace CapybaraVS.Script.Lib
{
    class ListFactory
    {
        [ScriptMethod(nameof(ListFactory) + "." + nameof(MakeListDouble), "",
            "RS=>ListFactory_MakeListDouble"//"<value> を開始値に <num> の数だけ <step> を加算した要素を持った配列を作成します。"
            )]
        public static List<double> MakeListDouble(int num, double value, double step)
        {
            List<double> vs = new List<double>();
            while (num-- != 0) 
            {
                vs.Add(value);
                value += step;
            }
            return vs;
        }

        //------------------------------------------------------------------
        [ScriptMethod(nameof(ListFactory) + "." + nameof(MakeListDouble2), "",
            "RS=>ListFactory_MakeListDouble2"//"<value> を開始値に <num> の数だけ <step> を加算し <power> 乗した要素を持った配列を作成します。"
            )]
        public static List<double> MakeListDouble2(int num, double value, double step, double power)
        {
            List<double> vs = new List<double>();
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
        [ScriptMethod(nameof(ListFactory) + ".Convert." + nameof(IntToDouble), "",
            "RS=>ListFactory_IntToDouble"//"int 型リストから double 型リストを作成します。"
            )]
        public static List<double> IntToDouble(List<int> list)
        {
            return list.ConvertAll(new Converter<int, double>((n) => (double)n));
        }

        //------------------------------------------------------------------
        [ScriptMethod(nameof(ListFactory) + ".Convert." + nameof(IntToString), "",
            "RS=>ListFactory_IntToString"//"int 型リストから string 型リストを作成します。"
            )]
        public static List<string> IntToString(List<int> list)
        {
            return list.ConvertAll(new Converter<int, string>((n) => n.ToString()));
        }

        //------------------------------------------------------------------
        [ScriptMethod(nameof(ListFactory) + ".Convert." + nameof(DoubleToInt), "",
            "RS=>ListFactory_DoubleToInt"//"double 型リストから int 型リストを作成します。"
            )]
        public static List<int> DoubleToInt(List<double> list)
        {
            return list.ConvertAll(new Converter<double, int>((n) => (int)n));
        }

        //------------------------------------------------------------------
        [ScriptMethod(nameof(ListFactory) + ".Convert." + nameof(DoubleToString), "",
            "RS=>ListFactory_DoubleToString"//"double 型リストから string 型リストを作成します。"
            )]
        public static List<string> DoubleToString(List<double> list)
        {
            return list.ConvertAll(new Converter<double, string>((n) => n.ToString()));
        }

        //------------------------------------------------------------------
        [ScriptMethod(nameof(ListFactory) + ".Convert." + nameof(StringToParseIntList), "",
            "RS=>ListFactory_StringToParseIntList"//", 区切りで数字の書かれた文字列から int 型リストを作成します。"
            )]
        public static List<int> StringToParseIntList(string value)
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
        [ScriptMethod(nameof(ListFactory) + ".Convert." + nameof(StringToParseDoubleList), "",
            "RS=>ListFactory_StringToParseDoubleList"//", 区切りで数字の書かれた文字列から double 型リストを作成します。"
            )]
        public static List<double> StringToParseDoubleList(string value)
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
        [ScriptMethod(nameof(ListFactory) + ".Join." + nameof(JoinIntList), "",
            "RS=>ListFactory_JoinIntList"//"<valus> int 型リストを <separator> を区切りに連結します。"
            )]
        public static string JoinIntList(string separator, List<int> value)
        {
            return string.Join(separator, value.ToArray());
        }

        //------------------------------------------------------------------
        [ScriptMethod(nameof(ListFactory) + ".Join." + nameof(JoinDoubleList), "",
            "RS=>ListFactory_JoinDoubleList"//"<valus> double 型リストを <separator> を区切りに連結します。"
            )]
        public static string JoinDoubleList(string separator, List<double> value)
        {
            return string.Join(separator, value.ToArray());
        }

        //------------------------------------------------------------------
        [ScriptMethod(nameof(ListFactory) + ".Join." + nameof(JoinStringList), "",
            "RS=>ListFactory_JoinStringList"//"<valus> string 型リストを <separator> を区切りに連結します。"
            )]
        public static string JoinStringList(string separator, List<string> value)
        {
            return string.Join(separator, value.ToArray());
        }

        //------------------------------------------------------------------
        [ScriptMethod(nameof(ListFactory) + ".Counter." + nameof(IntListSetCount), "",
            "RS=>ListFactory_IntListSetCount"//"int 型の <sample> リストの要素が <value> と同じ値をいくつ持っているかカウントします。"
            )]
        public static int IntListSetCount(List<int> sample, int value)
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
        [ScriptMethod(nameof(ListFactory) + ".Counter." + nameof(IntCountInvoke), "",
            "RS=>ListFactory_IntCountInvoke"//"int 型の <sample> リストの要素を仮引数に <predicate> をコールし返り値が True であればカウントアップしてカウント数を返します。\n※<predicate> には Assignment Func を接続します。"
            )]
        public static int IntCountInvoke(
            List<int> sample
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
        [ScriptMethod(nameof(ListFactory) + ".Counter." + nameof(DoubleCount), "",
            "RS=>ListFactory_DoubleCount"//"double 型の <sample> リストの要素が <value> と同じ値をいくつ持っているかカウントします。"
            )]
        public static int DoubleCount(List<double> sample, double value)
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
        [ScriptMethod(nameof(ListFactory) + ".Counter." + nameof(DoubleCountInvoke), "",
            "RS=>ListFactory_DoubleCountInvoke"//"double 型の <sample> リストの要素を仮引数に <predicate> をコールし返り値が True であればカウントアップしてカウント数を返します。\n※<predicate> には Assignment Func を接続します。"
            )]
        public static int DoubleCountInvoke(
            List<double> sample
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
        [ScriptMethod(nameof(ListFactory) + ".Counter." + nameof(WordCount), "",
            "RS=>ListFactory_WordCount"//"string 型の <sample> リストの要素が <word> と同じ値をいくつ持っているかカウントします。"
            )]
        public static int WordCount(List<string> sample, string word)
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
        [ScriptMethod(nameof(ListFactory) + ".Counter." + nameof(WordCountInvoke), "",
            "RS=>ListFactory_WordCountInvoke"//"string 型の <sample> リストの要素を仮引数に <predicate> をコールし返り値が True であればカウントアップしてカウント数を返します。\n※<predicate> には Assignment Func を接続します。"
            )]
        public static int WordCountInvoke(
            List<string> sample
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
        [ScriptMethod(nameof(ListFactory) + ".Sort." + nameof(SortDescIntList), "",
            "RS=>ListFactory_SortDescIntList"//"int 型のリストを昇順にソートします。"
            )]
        public static List<int> SortDescIntList(List<int> sample)
        {
            var ret = new List<int>(sample);
            ret.Sort();
            return ret;
        }

        //------------------------------------------------------------------
        [ScriptMethod(nameof(ListFactory) + ".Sort." + nameof(SortAscIntList), "",
            "RS=>ListFactory_SortAscIntList"//"int 型のリストを降順にソートします。"
            )]
        public static List<int> SortAscIntList(List<int> sample)
        {
            var ret = new List<int>(sample);
            ret.Sort((a, b) => b.CompareTo(a));
            return ret;
        }

        //------------------------------------------------------------------
        [ScriptMethod(nameof(ListFactory) + ".Sort." + nameof(SortDescDoubleList), "",
            "RS=>ListFactory_SortDescDoubleList"//"double 型のリストを昇順にソートします。"
            )]
        public static List<double> SortDescDoubleList(List<double> sample)
        {
            var ret = new List<double>(sample);
            ret.Sort();
            return ret;
        }

        //------------------------------------------------------------------
        [ScriptMethod(nameof(ListFactory) + ".Sort." + nameof(SortAscDoubleList), "",
            "RS=>ListFactory_SortAscDoubleList"//"double 型のリストを降順にソートします。"
            )]
        public static List<double> SortAscDoubleList(List<double> sample)
        {
            var ret = new List<double>(sample);
            ret.Sort((a, b) => b.CompareTo(a));
            return ret;
        }

        //------------------------------------------------------------------
        [ScriptMethod(nameof(ListFactory) + ".Sort." + nameof(SortDescStringList), "",
            "RS=>ListFactory_SortDescStringList"//"string 型のリストを昇順にソートします。"
            )]
        public static List<string> SortDescStringList(List<string> sample)
        {
            var ret = new List<string>(sample);
            ret.Sort();
            return ret;
        }

        //------------------------------------------------------------------
        [ScriptMethod(nameof(ListFactory) + ".Sort." + nameof(SortAscStringList), "",
            "RS=>ListFactory_SortAscStringList"//"string 型のリストを降順にソートします。"
            )]
        public static List<string> SortAscStringList(List<string> sample)
        {
            var ret = new List<string>(sample);
            ret.Sort((a, b) => b.CompareTo(a));
            return ret;
        }

        //------------------------------------------------------------------
        [ScriptMethod(nameof(ListFactory) + ".Filtering." + nameof(FilteringIntList), "",
            "RS=>ListFactory_FilteringIntList"//"int 型の <sample> リストの要素を仮引数に <predicate> をコールし返り値が True であればリストに登録して返します。\n※<predicate> には Assignment Func を接続します。"
            )]
        public static List<int> FilteringIntList(Func<object, bool> predicate, List<int> sample)
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
        [ScriptMethod(nameof(ListFactory) + ".Filtering." + nameof(FilteringDoubleList), "",
            "RS=>ListFactory_FilteringDoubleList"//"double 型の <sample> リストの要素を仮引数に <predicate> をコールし返り値が True であればリストに登録して返します。\n※<predicate> には Assignment Func を接続します。"
            )]
        public static List<double> FilteringDoubleList(Func<object, bool> predicate, List<double> sample)
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
        [ScriptMethod(nameof(ListFactory) + ".Filtering." + nameof(FilteringWordList), "",
            "RS=>ListFactory_FilteringWordList"//"string 型の <sample> リストの要素を仮引数に <predicate> をコールし返り値が True であればリストに登録して返します。\n※<predicate> には Assignment Func を接続します。"
            )]
        public static List<string> FilteringWordList(Func<object, bool> predicate, List<string> sample)
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
        [ScriptMethod(nameof(ListFactory) + ".Filtering." + nameof(Distinct), "",
            "RS=>ListFactory_Distinct"//"string 型のリストの同じ内容の要素を一つにまとめます。"
            )]
        public static List<string> Distinct(List<string> sample)
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
