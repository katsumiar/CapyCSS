using CapybaraVS.Script;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapyCSS.Script.Lib
{
    public class Script
    {
        public const string LIB_Script_NAME = "Script";
    }

    //------------------------------------------------------------------
    public class Script_bool
    {
        private const string LIB_NAME = Script.LIB_Script_NAME + ".bool";

        [ScriptMethod(LIB_NAME)]
        public static bool Parse(string s)
        {
            return bool.Parse(s);
        }

        [ScriptMethod(LIB_NAME)]
        public static bool TryParse(string s, out bool value)
        {
            return bool.TryParse(s, out value);
        }
    }

    //------------------------------------------------------------------
    public class Script_byte
    {
        private const string LIB_NAME = Script.LIB_Script_NAME + ".byte";

        [ScriptMethod(LIB_NAME)]
        public static byte MaxValue()
        {
            return byte.MaxValue;
        }

        [ScriptMethod(LIB_NAME)]
        public static byte MinValue()
        {
            return byte.MinValue;
        }

        [ScriptMethod(LIB_NAME)]
        public static byte Parse(string s)
        {
            return byte.Parse(s);
        }

        [ScriptMethod(LIB_NAME)]
        public static bool TryParse(string s, out byte value)
        {
            return byte.TryParse(s, out value);
        }
    }

    //------------------------------------------------------------------
    public class Script_sbyte
    {
        private const string LIB_NAME = Script.LIB_Script_NAME + ".sbyte";

        [ScriptMethod(LIB_NAME)]
        public static sbyte MaxValue()
        {
            return sbyte.MaxValue;
        }

        [ScriptMethod(LIB_NAME)]
        public static sbyte MinValue()
        {
            return sbyte.MinValue;
        }

        [ScriptMethod(LIB_NAME)]
        public static sbyte Parse(string s)
        {
            return sbyte.Parse(s);
        }

        [ScriptMethod(LIB_NAME)]
        public static bool TryParse(string s, out sbyte value)
        {
            return sbyte.TryParse(s, out value);
        }
    }

    //------------------------------------------------------------------
    public class Script_char
    {
        private const string LIB_NAME = Script.LIB_Script_NAME + ".char";

        [ScriptMethod(LIB_NAME)]
        public static char MaxValue()
        {
            return char.MaxValue;
        }

        [ScriptMethod(LIB_NAME)]
        public static char MinValue()
        {
            return char.MinValue;
        }

        [ScriptMethod(LIB_NAME)]
        public static char Parse(string s)
        {
            return char.Parse(s);
        }

        [ScriptMethod(LIB_NAME)]
        public static bool TryParse(string s, out char value)
        {
            return char.TryParse(s, out value);
        }
    }

    //------------------------------------------------------------------
    public class Script_short
    {
        private const string LIB_NAME = Script.LIB_Script_NAME + ".short";

        [ScriptMethod(LIB_NAME)]
        public static short MaxValue()
        {
            return short.MaxValue;
        }

        [ScriptMethod(LIB_NAME)]
        public static short MinValue()
        {
            return short.MinValue;
        }

        [ScriptMethod(LIB_NAME)]
        public static short Parse(string s)
        {
            return short.Parse(s);
        }

        [ScriptMethod(LIB_NAME)]
        public static bool TryParse(string s, out short value)
        {
            return short.TryParse(s, out value);
        }
    }

    //------------------------------------------------------------------
    public class Script_int
    {
        private const string LIB_NAME = Script.LIB_Script_NAME + ".int";

        [ScriptMethod(LIB_NAME)]
        public static int MaxValue()
        {
            return int.MaxValue;
        }

        [ScriptMethod(LIB_NAME)]
        public static int MinValue()
        {
            return int.MinValue;
        }

        [ScriptMethod(LIB_NAME)]
        public static int Parse(string s)
        {
            return int.Parse(s);
        }

        [ScriptMethod(LIB_NAME)]
        public static bool TryParse(string s, out int value)
        {
            return int.TryParse(s, out value);
        }
    }

    //------------------------------------------------------------------
    public class Script_long
    {
        private const string LIB_NAME = Script.LIB_Script_NAME + ".long";

        [ScriptMethod(LIB_NAME)]
        public static long MaxValue()
        {
            return long.MaxValue;
        }

        [ScriptMethod(LIB_NAME)]
        public static long MinValue()
        {
            return long.MinValue;
        }

        [ScriptMethod(LIB_NAME)]
        public static long Parse(string s)
        {
            return long.Parse(s);
        }

        [ScriptMethod(LIB_NAME)]
        public static bool TryParse(string s, out long value)
        {
            return long.TryParse(s, out value);
        }
    }

    //------------------------------------------------------------------
    public class Script_float
    {
        private const string LIB_NAME = Script.LIB_Script_NAME + ".float";

        [ScriptMethod(LIB_NAME)]
        public static float MaxValue()
        {
            return float.MaxValue;
        }

        [ScriptMethod(LIB_NAME)]
        public static float MinValue()
        {
            return float.MinValue;
        }

        [ScriptMethod(LIB_NAME)]
        public static float Parse(string s)
        {
            return float.Parse(s);
        }

        [ScriptMethod(LIB_NAME)]
        public static bool TryParse(string s, out float value)
        {
            return float.TryParse(s, out value);
        }
    }

    //------------------------------------------------------------------
    public class Script_double
    {
        private const string LIB_NAME = Script.LIB_Script_NAME + ".double";

        [ScriptMethod(LIB_NAME)]
        public static double MaxValue()
        {
            return double.MaxValue;
        }

        [ScriptMethod(LIB_NAME)]
        public static double MinValue()
        {
            return double.MinValue;
        }

        [ScriptMethod(LIB_NAME)]
        public static double Parse(string s)
        {
            return double.Parse(s);
        }

        [ScriptMethod(LIB_NAME)]
        public static bool TryParse(string s, out double value)
        {
            return double.TryParse(s, out value);
        }
    }

    //------------------------------------------------------------------
    public class Script_ushort
    {
        private const string LIB_NAME = Script.LIB_Script_NAME + ".ushort";

        [ScriptMethod(LIB_NAME)]
        public static ushort MaxValue()
        {
            return ushort.MaxValue;
        }

        [ScriptMethod(LIB_NAME)]
        public static ushort MinValue()
        {
            return ushort.MinValue;
        }

        [ScriptMethod(LIB_NAME)]
        public static ushort Parse(string s)
        {
            return ushort.Parse(s);
        }

        [ScriptMethod(LIB_NAME)]
        public static bool TryParse(string s, out ushort value)
        {
            return ushort.TryParse(s, out value);
        }
    }

    //------------------------------------------------------------------
    public class Script_uint
    {
        private const string LIB_NAME = Script.LIB_Script_NAME + ".uint";

        [ScriptMethod(LIB_NAME)]
        public static uint MaxValue()
        {
            return ushort.MaxValue;
        }

        [ScriptMethod(LIB_NAME)]
        public static uint MinValue()
        {
            return uint.MinValue;
        }

        [ScriptMethod(LIB_NAME)]
        public static uint Parse(string s)
        {
            return uint.Parse(s);
        }

        [ScriptMethod(LIB_NAME)]
        public static bool TryParse(string s, out uint value)
        {
            return uint.TryParse(s, out value);
        }
    }

    //------------------------------------------------------------------
    public class Script_ulong
    {
        private const string LIB_NAME = Script.LIB_Script_NAME + ".ulong";

        [ScriptMethod(LIB_NAME)]
        public static ulong MaxValue()
        {
            return ulong.MaxValue;
        }

        [ScriptMethod(LIB_NAME)]
        public static ulong MinValue()
        {
            return ulong.MinValue;
        }

        [ScriptMethod(LIB_NAME)]
        public static ulong Parse(string s)
        {
            return ulong.Parse(s);
        }

        [ScriptMethod(LIB_NAME)]
        public static bool TryParse(string s, out ulong value)
        {
            return ulong.TryParse(s, out value);
        }
    }

    //------------------------------------------------------------------
    public class Script_decimal
    {
        private const string LIB_NAME = Script.LIB_Script_NAME + ".decimal";

        [ScriptMethod(LIB_NAME)]
        public static decimal MaxValue()
        {
            return decimal.MaxValue;
        }

        [ScriptMethod(LIB_NAME)]
        public static decimal MinValue()
        {
            return decimal.MinValue;
        }

        [ScriptMethod(LIB_NAME)]
        public static decimal Parse(string s)
        {
            return decimal.Parse(s);
        }

        [ScriptMethod(LIB_NAME)]
        public static bool TryParse(string s, out decimal value)
        {
            return decimal.TryParse(s, out value);
        }
    }

    //------------------------------------------------------------------
    public class Script_IEnumerable
    {
        private const string LIB_NAME = Script.LIB_Script_NAME + ".IEnumerable";

        [ScriptMethod(LIB_NAME)]
        public static IEnumerator<TElement> GetEnumerator<TCollection, TElement>(TCollection collection)
            where TCollection : IEnumerable<TElement>
        {
            return collection.GetEnumerator();
        }
    }

    //------------------------------------------------------------------
    public class Script_Literal
    {
        public const string LIB_Script_literal_NAME = "Literal";

        [ScriptMethod(LIB_Script_literal_NAME)]
        public static T Null<T>()
            where T : class
        {
            return null;
        }
    }
}