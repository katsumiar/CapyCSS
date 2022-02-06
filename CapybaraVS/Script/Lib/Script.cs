using CapyCSS;
using CapyCSS.Script;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CapyCSS.Script.Lib
{
    [ScriptClass]
    public static class Script
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
    [ScriptClass(Script.LIB_Script_NAME + ".Array.bool")]
    public static class Script_boolArray
    {
        [ScriptMethod]
        public static int Length(bool[] array)
        {
            return array.Length;
        }

        [ScriptMethod]
        public static bool All(bool[] array, Func<bool, bool> predicate)
        {
            return array.All(predicate);
        }

        [ScriptMethod]
        public static bool Any(bool[] array, Func<bool, bool> predicate)
        {
            return array.Any(predicate);
        }

        [ScriptMethod]
        public static IEnumerable<bool> ConvertList(IEnumerable<bool> array)
        {
            return new List<bool>(array);
        }
    }

    //------------------------------------------------------------------
    [ScriptClass(Script.LIB_Script_NAME + ".Array.byte")]
    public static class Script_byteArray
    {
        [ScriptMethod]
        public static int Length(byte[] array)
        {
            return array.Length;
        }

        [ScriptMethod]
        public static bool All(byte[] array, Func<byte, bool> predicate)
        {
            return array.All(predicate);
        }

        [ScriptMethod]
        public static bool Any(byte[] array, Func<byte, bool> predicate)
        {
            return array.Any(predicate);
        }

        [ScriptMethod]
        public static IEnumerable<byte> ConvertList(IEnumerable<byte> array)
        {
            return new List<byte>(array);
        }
    }

    //------------------------------------------------------------------
    [ScriptClass(Script.LIB_Script_NAME + ".Array.sbyte")]
    public static class Script_sbyteArray
    {
        [ScriptMethod]
        public static int Length(sbyte[] array)
        {
            return array.Length;
        }

        [ScriptMethod]
        public static bool All(sbyte[] array, Func<sbyte, bool> predicate)
        {
            return array.All(predicate);
        }

        [ScriptMethod]
        public static bool Any(sbyte[] array, Func<sbyte, bool> predicate)
        {
            return array.Any(predicate);
        }

        [ScriptMethod]
        public static IEnumerable<sbyte> ConvertList(IEnumerable<sbyte> array)
        {
            return new List<sbyte>(array);
        }
    }

    //------------------------------------------------------------------
    [ScriptClass(Script.LIB_Script_NAME + ".Array.char")]
    public static class Script_charArray
    {
        [ScriptMethod]
        public static int Length(char[] array)
        {
            return array.Length;
        }

        [ScriptMethod]
        public static bool All(char[] array, Func<char, bool> predicate)
        {
            return array.All(predicate);
        }

        [ScriptMethod]
        public static bool Any(char[] array, Func<char, bool> predicate)
        {
            return array.Any(predicate);
        }

        [ScriptMethod]
        public static IEnumerable<char> ConvertList(IEnumerable<char> array)
        {
            return new List<char>(array);
        }
    }

    //------------------------------------------------------------------
    [ScriptClass(Script.LIB_Script_NAME + ".Array.short")]
    public static class Script_shortArray
    {
        [ScriptMethod]
        public static int Length(short[] array)
        {
            return array.Length;
        }

        [ScriptMethod]
        public static bool All(short[] array, Func<short, bool> predicate)
        {
            return array.All(predicate);
        }

        [ScriptMethod]
        public static bool Any(short[] array, Func<short, bool> predicate)
        {
            return array.Any(predicate);
        }

        [ScriptMethod]
        public static IEnumerable<short> ConvertList(IEnumerable<short> array)
        {
            return new List<short>(array);
        }
    }

    //------------------------------------------------------------------
    [ScriptClass(Script.LIB_Script_NAME + ".Array.int")]
    public static class Script_intArray
    {
        [ScriptMethod]
        public static int Length(int[] array)
        {
            return array.Length;
        }

        [ScriptMethod]
        public static bool All(int[] array, Func<int, bool> predicate)
        {
            return array.All(predicate);
        }

        [ScriptMethod]
        public static bool Any(int[] array, Func<int, bool> predicate)
        {
            return array.Any(predicate);
        }

        [ScriptMethod]
        public static IEnumerable<int> ConvertList(IEnumerable<int> array)
        {
            return new List<int>(array);
        }
    }

    //------------------------------------------------------------------
    [ScriptClass(Script.LIB_Script_NAME + ".Array.long")]
    public static class Script_longArray
    {
        [ScriptMethod]
        public static int Length(long[] array)
        {
            return array.Length;
        }

        [ScriptMethod]
        public static bool All(long[] array, Func<long, bool> predicate)
        {
            return array.All(predicate);
        }

        [ScriptMethod]
        public static bool Any(long[] array, Func<long, bool> predicate)
        {
            return array.Any(predicate);
        }

        [ScriptMethod]
        public static IEnumerable<long> ConvertList(IEnumerable<long> array)
        {
            return new List<long>(array);
        }
    }

    //------------------------------------------------------------------
    [ScriptClass(Script.LIB_Script_NAME + ".Array.float")]
    public static class Script_floatArray
    {
        [ScriptMethod]
        public static int Length(float[] array)
        {
            return array.Length;
        }

        [ScriptMethod]
        public static bool All(float[] array, Func<float, bool> predicate)
        {
            return array.All(predicate);
        }

        [ScriptMethod]
        public static bool Any(float[] array, Func<float, bool> predicate)
        {
            return array.Any(predicate);
        }

        [ScriptMethod]
        public static IEnumerable<float> ConvertList(IEnumerable<float> array)
        {
            return new List<float>(array);
        }
    }

    //------------------------------------------------------------------
    [ScriptClass(Script.LIB_Script_NAME + ".Array.double")]
    public static class Script_doubleArray
    {
        [ScriptMethod]
        public static int Length(double[] array)
        {
            return array.Length;
        }

        [ScriptMethod]
        public static bool All(double[] array, Func<double, bool> predicate)
        {
            return array.All(predicate);
        }

        [ScriptMethod]
        public static bool Any(double[] array, Func<double, bool> predicate)
        {
            return array.Any(predicate);
        }

        [ScriptMethod]
        public static IEnumerable<double> ConvertList(IEnumerable<double> array)
        {
            return new List<double>(array);
        }
    }

    //------------------------------------------------------------------
    [ScriptClass(Script.LIB_Script_NAME + ".Array.ushort")]
    public static class Script_ushortArray
    {
        [ScriptMethod]
        public static int Length(ushort[] array)
        {
            return array.Length;
        }

        [ScriptMethod]
        public static bool All(ushort[] array, Func<ushort, bool> predicate)
        {
            return array.All(predicate);
        }

        [ScriptMethod]
        public static bool Any(ushort[] array, Func<ushort, bool> predicate)
        {
            return array.Any(predicate);
        }

        [ScriptMethod]
        public static IEnumerable<ushort> ConvertList(IEnumerable<ushort> array)
        {
            return new List<ushort>(array);
        }
    }

    //------------------------------------------------------------------
    [ScriptClass(Script.LIB_Script_NAME + ".Array.uint")]
    public static class Script_uintArray
    {
        [ScriptMethod]
        public static int Length(uint[] array)
        {
            return array.Length;
        }

        [ScriptMethod]
        public static bool All(uint[] array, Func<uint, bool> predicate)
        {
            return array.All(predicate);
        }

        [ScriptMethod]
        public static bool Any(uint[] array, Func<uint, bool> predicate)
        {
            return array.Any(predicate);
        }

        [ScriptMethod]
        public static IEnumerable<uint> ConvertList(IEnumerable<uint> array)
        {
            return new List<uint>(array);
        }
    }

    //------------------------------------------------------------------
    [ScriptClass(Script.LIB_Script_NAME + ".Array.ulong")]
    public static class Script_ulongArray
    {
        [ScriptMethod]
        public static int Length(ulong[] array)
        {
            return array.Length;
        }

        [ScriptMethod]
        public static bool All(ulong[] array, Func<ulong, bool> predicate)
        {
            return array.All(predicate);
        }

        [ScriptMethod]
        public static bool Any(ulong[] array, Func<ulong, bool> predicate)
        {
            return array.Any(predicate);
        }

        [ScriptMethod]
        public static IEnumerable<ulong> ConvertList(IEnumerable<ulong> array)
        {
            return new List<ulong>(array);
        }
    }

    //------------------------------------------------------------------
    [ScriptClass(Script.LIB_Script_NAME + ".Array.decimal")]
    public static class Script_decimalArray
    {
        [ScriptMethod]
        public static int Length(decimal[] array)
        {
            return array.Length;
        }

        [ScriptMethod]
        public static bool All(decimal[] array, Func<decimal, bool> predicate)
        {
            return array.All(predicate);
        }

        [ScriptMethod]
        public static bool Any(decimal[] array, Func<decimal, bool> predicate)
        {
            return array.Any(predicate);
        }

        [ScriptMethod]
        public static IEnumerable<decimal> ConvertList(IEnumerable<decimal> array)
        {
            return new List<decimal>(array);
        }
    }

    //------------------------------------------------------------------
    [ScriptClass]
    public static class Script_IEnumerable
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
    [ScriptClass]
    public static class Script_Literal
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
    [ScriptClass]
    public static class Script_Literal2
    {
        public const string LIB_Script_literal_NAME = "Literal";

        [ScriptMethod(LIB_Script_literal_NAME)]
        public static object Null()
        {
            return null;
        }
    }
}