using System;
using System.Collections.Generic;
using System.Text;

namespace CapybaraVS.Script.Lib
{
    public class Dictionary
    {
        [ScriptMethod(nameof(Dictionary) + ".string.string.CreateDictionary")]
        public static Dictionary<string, string> CreateDictionaryStringString()
        {
            return new Dictionary<string, string>();
        }

        [ScriptMethod(nameof(Dictionary) + ".string.string." + nameof(Add))]
        public static Dictionary<string, string> Add(Dictionary<string, string> dic, string key, string value)
        {
            dic.Add(key, value);
            return dic;
        }

        [ScriptMethod(nameof(Dictionary) + ".string.string." + nameof(AddList))]
        public static Dictionary<string, string> AddList(Dictionary<string, string> dic, List<string> keyList, List<string> valueList)
        {
            for (int i = 0; i < Math.Min(keyList.Count, valueList.Count); ++i)
            {
                dic.Add(keyList[i], valueList[i]);
            }
            return dic;
        }

        [ScriptMethod(nameof(Dictionary) + ".string.string." + nameof(GetValue))]
        public static string GetValue(Dictionary<string, string> dic, string key, string notFoundValue)
        {
            if (!dic.ContainsKey(key))
            {
                return notFoundValue;
            }
            return dic[key];
        }

        //=========================================================================
        [ScriptMethod(nameof(Dictionary) + ".int.string.CreateDictionary")]
        public static Dictionary<int, string> CreateDictionaryIntString()
        {
            return new Dictionary<int, string>();
        }

        [ScriptMethod(nameof(Dictionary) + ".int.string." + nameof(Add))]
        public static Dictionary<int, string> Add(Dictionary<int, string> dic, int key, string value)
        {
            dic.Add(key, value);
            return dic;
        }

        [ScriptMethod(nameof(Dictionary) + ".int.string." + nameof(AddList))]
        public static Dictionary<int, string> AddList(Dictionary<int, string> dic, List<int> keyList, List<string> valueList)
        {
            for (int i = 0; i < Math.Min(keyList.Count, valueList.Count); ++i)
            {
                dic.Add(keyList[i], valueList[i]);
            }
            return dic;
        }

        [ScriptMethod(nameof(Dictionary) + ".int.string." + nameof(GetValue))]
        public static string GetValue(Dictionary<int, string> dic, int key, string notFoundValue)
        {
            if (!dic.ContainsKey(key))
            {
                return notFoundValue;
            }
            return dic[key];
        }

        //=========================================================================
        [ScriptMethod(nameof(Dictionary) + ".string.int.CreateDictionary")]
        public static Dictionary<string, int> CreateDictionaryStringInt()
        {
            return new Dictionary<string, int>();
        }

        [ScriptMethod(nameof(Dictionary) + ".string.int." + nameof(Add))]
        public static Dictionary<string, int> Add(Dictionary<string, int> dic, string key, int value)
        {
            dic.Add(key, value);
            return dic;
        }

        [ScriptMethod(nameof(Dictionary) + ".string.int." + nameof(AddList))]
        public static Dictionary<string, int> AddList(Dictionary<string, int> dic, List<string> keyList, List<int> valueList)
        {
            for (int i = 0; i < Math.Min(keyList.Count, valueList.Count); ++i)
            {
                dic.Add(keyList[i], valueList[i]);
            }
            return dic;
        }

        [ScriptMethod(nameof(Dictionary) + ".string.int." + nameof(GetValue))]
        public static int GetValue(Dictionary<string, int> dic, string key, int notFoundValue)
        {
            if (!dic.ContainsKey(key))
            {
                return notFoundValue;
            }
            return dic[key];
        }

        //=========================================================================
        [ScriptMethod(nameof(Dictionary) + ".int.int.CreateDictionary")]
        public static Dictionary<int, int> CreateDictionaryIntInt()
        {
            return new Dictionary<int, int>();
        }

        [ScriptMethod(nameof(Dictionary) + ".int.int." + nameof(Add))]
        public static Dictionary<int, int> Add(Dictionary<int, int> dic, int key, int value)
        {
            dic.Add(key, value);
            return dic;
        }

        [ScriptMethod(nameof(Dictionary) + ".int.int." + nameof(AddList))]
        public static Dictionary<int, int> AddList(Dictionary<int, int> dic, List<int> keyList, List<int> valueList)
        {
            for (int i = 0; i < Math.Min(keyList.Count, valueList.Count); ++i)
            {
                dic.Add(keyList[i], valueList[i]);
            }
            return dic;
        }

        [ScriptMethod(nameof(Dictionary) + ".int.int." + nameof(GetValue))]
        public static int GetValue(Dictionary<int, int> dic, int key, int notFoundValue)
        {
            if (!dic.ContainsKey(key))
            {
                return notFoundValue;
            }
            return dic[key];
        }

        //=========================================================================
        [ScriptMethod(nameof(Dictionary) + ".int.double.CreateDictionary")]
        public static Dictionary<int, double> CreateDictionaryIntDouble()
        {
            return new Dictionary<int, double>();
        }

        [ScriptMethod(nameof(Dictionary) + ".int.double." + nameof(Add))]
        public static Dictionary<int, double> Add(Dictionary<int, double> dic, int key, double value)
        {
            dic.Add(key, value);
            return dic;
        }

        [ScriptMethod(nameof(Dictionary) + ".int.double." + nameof(AddList))]
        public static Dictionary<int, double> AddList(Dictionary<int, double> dic, List<int> keyList, List<double> valueList)
        {
            for (int i = 0; i < Math.Min(keyList.Count, valueList.Count); ++i)
            {
                dic.Add(keyList[i], valueList[i]);
            }
            return dic;
        }

        [ScriptMethod(nameof(Dictionary) + ".int.double." + nameof(GetValue))]
        public static double GetValue(Dictionary<int, double> dic, int key, double notFoundValue)
        {
            if (!dic.ContainsKey(key))
            {
                return notFoundValue;
            }
            return dic[key];
        }

        //=========================================================================
        [ScriptMethod(nameof(Dictionary) + ".string.double.CreateDictionary")]
        public static Dictionary<string, double> CreateDictionaryStringDouble()
        {
            return new Dictionary<string, double>();
        }

        [ScriptMethod(nameof(Dictionary) + ".string.double." + nameof(Add))]
        public static Dictionary<string, double> Add(Dictionary<string, double> dic, string key, double value)
        {
            dic.Add(key, value);
            return dic;
        }

        [ScriptMethod(nameof(Dictionary) + ".string.double." + nameof(AddList))]
        public static Dictionary<string, double> AddList(Dictionary<string, double> dic, List<string> keyList, List<double> valueList)
        {
            for (int i = 0; i < Math.Min(keyList.Count, valueList.Count); ++i)
            {
                dic.Add(keyList[i], valueList[i]);
            }
            return dic;
        }

        [ScriptMethod(nameof(Dictionary) + ".string.double." + nameof(GetValue))]
        public static double GetValue(Dictionary<string, double> dic, string key, double notFoundValue)
        {
            if (!dic.ContainsKey(key))
            {
                return notFoundValue;
            }
            return dic[key];
        }
    }
}
