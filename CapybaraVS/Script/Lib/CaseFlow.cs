using System;
using System.Collections.Generic;
using System.Text;

namespace CapybaraVS.Script.Lib
{
    public class CaseFlow
    {
        //------------------------------------------------------------------
        // Dictionary<int, Func<object, int>>
        //------------------------------------------------------------------

        [ScriptMethod("Case" + ".int.CreateCaseDictionary")]
        public static Dictionary<int, Func<object, int>> CreateDictionaryIntInt()
        {
            return new Dictionary<int, Func<object, int>>();
        }

        [ScriptMethod("Case" + ".int." + nameof(Add))]
        public static Dictionary<int, Func<object, int>> Add(Dictionary<int, Func<object, int>> dic, int key, Func<object, int> value)
        {
            dic ??= new Dictionary<int, Func<object, int>>();
            dic.Add(key, value);
            return dic;
        }

        [ScriptMethod("Case" + ".int." + "AddFromList")]
        public static Dictionary<int, Func<object, int>> AddList(Dictionary<int, Func<object, int>> dic, List<int> keyList, List<Func<object, int>> valueList)
        {
            dic ??= new Dictionary<int, Func<object, int>>();
            for (int i = 0; i < Math.Min(keyList.Count, valueList.Count); ++i)
            {
                dic.Add(keyList[i], valueList[i]);
            }
            return dic;
        }

        [ScriptMethod("Case" + ".int." + nameof(GetFromDictionary))]
        public static Func<object, int> GetFromDictionary(Dictionary<int, Func<object, int>> dic, int key)
        {
            dic ??= new Dictionary<int, Func<object, int>>();
            if (!dic.ContainsKey(key))
            {
                return null;
            }
            return dic[key];
        }

        [ScriptMethod("Case" + ".int." + nameof(SwitchCall))]
        public static int SwitchCall(Dictionary<int, Func<object, int>> dic, int key, int arg, int notFoundValue)
        {
            dic ??= new Dictionary<int, Func<object, int>>();
            if (!dic.ContainsKey(key))
            {
                return notFoundValue;
            }
            Func<object, int> func = dic[key];
            if (func is null)
                return notFoundValue;
            return func(arg);
        }


        //------------------------------------------------------------------
        // Dictionary<int, Action<object>>
        //------------------------------------------------------------------

        [ScriptMethod("Case" + ".Action.CreateCaseDictionary")]
        public static Dictionary<int, Action<object>> CreateDictionaryIntAction()
        {
            return new Dictionary<int, Action<object>>();
        }

        [ScriptMethod("Case" + ".Action." + nameof(Add))]
        public static Dictionary<int, Action<object>> Add(Dictionary<int, Action<object>> dic, int key, Action<object> value)
        {
            dic ??= new Dictionary<int, Action<object>>();
            dic.Add(key, value);
            return dic;
        }

        [ScriptMethod("Case" + ".Action." + "AddFromList")]
        public static Dictionary<int, Action<object>> AddList(Dictionary<int, Action<object>> dic, List<int> keyList, List<Action<object>> valueList)
        {
            dic ??= new Dictionary<int, Action<object>>();
            for (int i = 0; i < Math.Min(keyList.Count, valueList.Count); ++i)
            {
                dic.Add(keyList[i], valueList[i]);
            }
            return dic;
        }

        [ScriptMethod("Case" + ".Action." + nameof(GetFromDictionary))]
        public static Action<object> GetFromDictionary(Dictionary<int, Action<object>> dic, int key)
        {
            dic ??= new Dictionary<int, Action<object>>();
            if (!dic.ContainsKey(key))
            {
                return null;
            }
            return dic[key];
        }

        [ScriptMethod("Case" + ".Action." + nameof(SwitchCall))]
        public static void SwitchCall(Dictionary<int, Action<object>> dic, int key, string arg, Action<object> notFoundAction)
        {
            dic ??= new Dictionary<int, Action<object>>();
            if (!dic.ContainsKey(key))
            {
                notFoundAction?.Invoke(arg);
            }
            Action<object> func = dic[key];
            if (func is null)
            {
                notFoundAction?.Invoke(arg);
            }
            else
            {
                func.Invoke(arg);
            }
        }
    }
}
