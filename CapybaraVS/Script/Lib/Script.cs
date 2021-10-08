using CapybaraVS;
using CapybaraVS.Script;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CapyCSS.Script.Lib
{
    public class Script
    {
        public const string LIB_Script_NAME = "Script";

        [ScriptMethod(LIB_Script_NAME)]
        public static void ShowMessage(string title, string contents)
        {
            ControlTools.ShowSelectMessage(title, contents);
        }

        [ScriptMethod(LIB_Script_NAME)]
        public static MessageBoxResult ShowConfirmMessage(string title, string contents)
        {
            return ControlTools.ShowSelectMessage(title, contents, MessageBoxButton.OKCancel);
        }
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
    public class Script_boolArray
    {
        private const string LIB_NAME = Script.LIB_Script_NAME + ".Array.bool";

        [ScriptMethod(LIB_NAME)]
        public static int Length(bool[] array)
        {
            return array.Length;
        }

        [ScriptMethod(LIB_NAME)]
        public static bool All(bool[] array, Func<bool, bool> predicate)
        {
            return array.All(predicate);
        }

        [ScriptMethod(LIB_NAME)]
        public static bool Any(bool[] array, Func<bool, bool> predicate)
        {
            return array.Any(predicate);
        }
    }

    //------------------------------------------------------------------
    public class Script_byteArray
    {
        private const string LIB_NAME = Script.LIB_Script_NAME + ".Array.byte";

        [ScriptMethod(LIB_NAME)]
        public static int Length(byte[] array)
        {
            return array.Length;
        }

        [ScriptMethod(LIB_NAME)]
        public static bool All(byte[] array, Func<byte, bool> predicate)
        {
            return array.All(predicate);
        }

        [ScriptMethod(LIB_NAME)]
        public static bool Any(byte[] array, Func<byte, bool> predicate)
        {
            return array.Any(predicate);
        }
    }

    //------------------------------------------------------------------
    public class Script_sbyteArray
    {
        private const string LIB_NAME = Script.LIB_Script_NAME + ".Array.sbyte";

        [ScriptMethod(LIB_NAME)]
        public static int Length(byte[] array)
        {
            return array.Length;
        }

        [ScriptMethod(LIB_NAME)]
        public static bool All(sbyte[] array, Func<sbyte, bool> predicate)
        {
            return array.All(predicate);
        }

        [ScriptMethod(LIB_NAME)]
        public static bool Any(sbyte[] array, Func<sbyte, bool> predicate)
        {
            return array.Any(predicate);
        }
    }
    //------------------------------------------------------------------
    public class Script_charArray
    {
        private const string LIB_NAME = Script.LIB_Script_NAME + ".Array.char";

        [ScriptMethod(LIB_NAME)]
        public static int Length(char[] array)
        {
            return array.Length;
        }

        [ScriptMethod(LIB_NAME)]
        public static bool All(char[] array, Func<char, bool> predicate)
        {
            return array.All(predicate);
        }

        [ScriptMethod(LIB_NAME)]
        public static bool Any(char[] array, Func<char, bool> predicate)
        {
            return array.Any(predicate);
        }
    }

    //------------------------------------------------------------------
    public class Script_shortArray
    {
        private const string LIB_NAME = Script.LIB_Script_NAME + ".Array.short";

        [ScriptMethod(LIB_NAME)]
        public static int Length(short[] array)
        {
            return array.Length;
        }

        [ScriptMethod(LIB_NAME)]
        public static bool All(short[] array, Func<short, bool> predicate)
        {
            return array.All(predicate);
        }

        [ScriptMethod(LIB_NAME)]
        public static bool Any(short[] array, Func<short, bool> predicate)
        {
            return array.Any(predicate);
        }
    }

    //------------------------------------------------------------------
    public class Script_intArray
    {
        private const string LIB_NAME = Script.LIB_Script_NAME + ".Array.int";

        [ScriptMethod(LIB_NAME)]
        public static int Length(int[] array)
        {
            return array.Length;
        }

        [ScriptMethod(LIB_NAME)]
        public static bool All(int[] array, Func<int, bool> predicate)
        {
            return array.All(predicate);
        }

        [ScriptMethod(LIB_NAME)]
        public static bool Any(int[] array, Func<int, bool> predicate)
        {
            return array.Any(predicate);
        }
    }

    //------------------------------------------------------------------
    public class Script_longArray
    {
        private const string LIB_NAME = Script.LIB_Script_NAME + ".Array.long";

        [ScriptMethod(LIB_NAME)]
        public static int Length(long[] array)
        {
            return array.Length;
        }

        [ScriptMethod(LIB_NAME)]
        public static bool All(long[] array, Func<long, bool> predicate)
        {
            return array.All(predicate);
        }

        [ScriptMethod(LIB_NAME)]
        public static bool Any(long[] array, Func<long, bool> predicate)
        {
            return array.Any(predicate);
        }
    }

    //------------------------------------------------------------------
    public class Script_floatArray
    {
        private const string LIB_NAME = Script.LIB_Script_NAME + ".Array.float";

        [ScriptMethod(LIB_NAME)]
        public static int Length(float[] array)
        {
            return array.Length;
        }

        [ScriptMethod(LIB_NAME)]
        public static bool All(float[] array, Func<float, bool> predicate)
        {
            return array.All(predicate);
        }

        [ScriptMethod(LIB_NAME)]
        public static bool Any(float[] array, Func<float, bool> predicate)
        {
            return array.Any(predicate);
        }
    }

    //------------------------------------------------------------------
    public class Script_doubleArray
    {
        private const string LIB_NAME = Script.LIB_Script_NAME + ".Array.double";

        [ScriptMethod(LIB_NAME)]
        public static int Length(double[] array)
        {
            return array.Length;
        }

        [ScriptMethod(LIB_NAME)]
        public static bool All(double[] array, Func<double, bool> predicate)
        {
            return array.All(predicate);
        }

        [ScriptMethod(LIB_NAME)]
        public static bool Any(double[] array, Func<double, bool> predicate)
        {
            return array.Any(predicate);
        }
    }

    //------------------------------------------------------------------
    public class Script_ushortArray
    {
        private const string LIB_NAME = Script.LIB_Script_NAME + ".Array.ushort";

        [ScriptMethod(LIB_NAME)]
        public static int Length(ushort[] array)
        {
            return array.Length;
        }

        [ScriptMethod(LIB_NAME)]
        public static bool All(ushort[] array, Func<ushort, bool> predicate)
        {
            return array.All(predicate);
        }

        [ScriptMethod(LIB_NAME)]
        public static bool Any(ushort[] array, Func<ushort, bool> predicate)
        {
            return array.Any(predicate);
        }
    }

    //------------------------------------------------------------------
    public class Script_uintArray
    {
        private const string LIB_NAME = Script.LIB_Script_NAME + ".Array.uint";

        [ScriptMethod(LIB_NAME)]
        public static int Length(uint[] array)
        {
            return array.Length;
        }

        [ScriptMethod(LIB_NAME)]
        public static bool All(uint[] array, Func<uint, bool> predicate)
        {
            return array.All(predicate);
        }

        [ScriptMethod(LIB_NAME)]
        public static bool Any(uint[] array, Func<uint, bool> predicate)
        {
            return array.Any(predicate);
        }
    }

    //------------------------------------------------------------------
    public class Script_ulongArray
    {
        private const string LIB_NAME = Script.LIB_Script_NAME + ".Array.ulong";

        [ScriptMethod(LIB_NAME)]
        public static int Length(ulong[] array)
        {
            return array.Length;
        }

        [ScriptMethod(LIB_NAME)]
        public static bool All(ulong[] array, Func<ulong, bool> predicate)
        {
            return array.All(predicate);
        }

        [ScriptMethod(LIB_NAME)]
        public static bool Any(ulong[] array, Func<ulong, bool> predicate)
        {
            return array.Any(predicate);
        }
    }

    //------------------------------------------------------------------
    public class Script_decimalArray
    {
        private const string LIB_NAME = Script.LIB_Script_NAME + ".Array.decimal";

        [ScriptMethod(LIB_NAME)]
        public static int Length(decimal[] array)
        {
            return array.Length;
        }

        [ScriptMethod(LIB_NAME)]
        public static bool All(decimal[] array, Func<decimal, bool> predicate)
        {
            return array.All(predicate);
        }

        [ScriptMethod(LIB_NAME)]
        public static bool Any(decimal[] array, Func<decimal, bool> predicate)
        {
            return array.Any(predicate);
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

    //------------------------------------------------------------------
    public class Script_Literal2
    {
        public const string LIB_Script_literal_NAME = "Literal";

        [ScriptMethod(LIB_Script_literal_NAME)]
        public static object Null()
        {
            return null;
        }
    }
}