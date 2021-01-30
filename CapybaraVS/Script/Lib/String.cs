using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace CapybaraVS.Script.Lib
{
    public class String
    {
        [ScriptMethod(nameof(String) + ".Parse." + nameof(ToInt), "",
            "RS=>String_ToInt"//"値へ変換：\n文字列を int 型の値に変換します。"
            )]
        public static int ToInt(string str)
        {
            return int.Parse(str);
        }

        //------------------------------------------------------------------
        [ScriptMethod(nameof(String) + ".Parse." + nameof(ToLong), "",
            "RS=>String_ToLong"//"値へ変換：\n文字列を long 型の値に変換します。"
            )]
        public static long ToLong(string str)
        {
            return long.Parse(str);
        }

        //------------------------------------------------------------------
        [ScriptMethod(nameof(String) + ".Parse." + nameof(ToFloat), "",
            "RS=>String_ToFloat"//"値へ変換：\n文字列を float 型の値に変換します。"
            )]
        public static float ToFloat(string str)
        {
            return float.Parse(str);
        }

        //------------------------------------------------------------------
        [ScriptMethod(nameof(String) + ".Parse." + nameof(ToDouble), "",
            "RS=>String_ToDouble"//"値へ変換：\n文字列を double 型の値に変換します。"
            )]
        public static double ToDouble(string str)
        {
            return double.Parse(str);
        }

        //------------------------------------------------------------------
        [ScriptMethod(nameof(String) + ".Match." + nameof(IsMatch), "",
            "RS=>String_IsMatch"//"正規表現による判定：\n<input> 文字列を <pattern>正規表現で判定します。"
            )]
        public static bool IsMatch(string input, string pattern)
        {
            return Regex.IsMatch(input, pattern);
        }

        //------------------------------------------------------------------
        [ScriptMethod(nameof(String) + ".Match." + nameof(Match), "",
            "RS=>String_Match"//"正規表現による参照：\n<input> 文字列の <pattern>正規表現による一致を参照します。"
            )]
        public static string Match(string input, string pattern)
        {
            Match matche = Regex.Match(input, pattern);
            return matche.Value;
        }

        //------------------------------------------------------------------
        [ScriptMethod(nameof(String) + ".Match." + nameof(MatchCollection), "",
            "RS=>String_MatchCollection"//"正規表現による全参照：\n<input> 文字列の <pattern>正規表現によるすべての一致を参照します。"
            )]
        public static List<string> MatchCollection(string input, string pattern)
        {
            MatchCollection matche = Regex.Matches(input, pattern);
            List<string> list = new List<string>();
            foreach (Match m in matche)
            {
                list.Add(m.Value);
            }
            return list;
        }

        //------------------------------------------------------------------
        [ScriptMethod(nameof(String) + ".Match." + nameof(MatchReplace))]
        public static string MatchReplace(string input, string pattern, string replacement)
        {
            Regex reg = new Regex(pattern);
            return reg.Replace(input, replacement);
        }

        //------------------------------------------------------------------
        [ScriptMethod(nameof(String) + ".Enter." + nameof(NewLine), "",
            "RS=>String_NewLine"//""改行"
            )]
        public static string NewLine()
        {
            return Environment.NewLine;
        }

        //------------------------------------------------------------------
        [ScriptMethod(nameof(String) + ".Enter." + @"\n", "",
            "RS=>String_GetEnter"//@"改行(\n)"
            )]
        public static char GetEnter()
        {
            return '\n';
        }

        //------------------------------------------------------------------
        [ScriptMethod(nameof(String) + ".Enter." + @"\r", "",
            "RS=>String_GetEnter2"//"改行(\r)"
            )]
        public static char GetEnter2()
        {
            return '\r';
        }

        //------------------------------------------------------------------
        [ScriptMethod(nameof(String) + ".Trim." + nameof(Trim), "",
            "RS=>String_Trim"//"文字列のトリム：\n文字列の頭と末の空白を削除します。"
            )]
        public static string Trim(string str)
        {
            return str.Trim();
        }

        //------------------------------------------------------------------
        [ScriptMethod(nameof(String) + ".Trim." + nameof(TrimStart), "",
            "RS=>String_TrimStart"//"文字列の頭のトリム：\n文字列の頭の空白を削除します。"
            )]
        public static string TrimStart(string str)
        {
            return str.TrimStart();
        }

        //------------------------------------------------------------------
        [ScriptMethod(nameof(String) + ".Trim." + nameof(TrimEnd), "",
            "RS=>String_TrimEnd"//"文字列末のトリム：\n文字列末の空白を削除します。"
            )]
        public static string TrimEnd(string str)
        {
            return str.TrimEnd();
        }

        //------------------------------------------------------------------
        [ScriptMethod(nameof(String) + "." + nameof(ToUpper), "",
            "RS=>String_ToUpper"//"文字列の大文字化：\n文字列中の小文字を大文字に変換します。"
            )]
        public static string ToUpper(string str)
        {
            return str.ToUpperInvariant();
        }

        //------------------------------------------------------------------
        [ScriptMethod(nameof(String) + "." + nameof(ToLower), "",
            "RS=>String_ToLower"//"文字列の小文字化：\n文字列中の小文字を小文字に変換します。"
            )]
        public static string ToLower(string str)
        {
            return str.ToLowerInvariant();
        }

        //------------------------------------------------------------------
        [ScriptMethod(nameof(String) + "." + nameof(ToTitleCase))]
        public static string ToTitleCase(string str)
        {
            System.Globalization.TextInfo ti =
               System.Globalization.CultureInfo.CurrentCulture.TextInfo;
            return ti.ToTitleCase(str);
        }

        //------------------------------------------------------------------
        [ScriptMethod(nameof(String) + "." + nameof(StartsWith), "",
            "RS=>String_StartsWith"//"先頭文字列一致判定：\n<str>文字列の先頭から <value> 文字列の一致を判定します。"
            )]
        public static bool StartsWith(string str, string value)
        {
            return str.StartsWith(value);
        }

        //------------------------------------------------------------------
        [ScriptMethod(nameof(String) + "." + nameof(Split), "",
            "RS=>String_Split"//"文字列の分割：\n<str>文字列を <separator> 文字で分割します。"
            )]
        public static List<string> Split(string str, char separator)
        {
            return new List<string>(str.Split(separator));
        }

        //------------------------------------------------------------------
        [ScriptMethod(nameof(String) + "." + nameof(SplitFromString), "",
            "RS=>String_SplitFromString"//"文字列の分割：\n<str>文字列を <separator> 文字列で分割します。"
            )]
        public static List<string> SplitFromString(string str, string separator)
        {
            return new List<string>(str.Split(separator));
        }

        //------------------------------------------------------------------
        [ScriptMethod(nameof(String) + "." + nameof(Replace), "",
            "RS=>String_Replace"//"文字列の置換：\n<str>文字列の <oldStr> を <newStr> に置換します。"
            )]
        public static string Replace(string str, string oldStr, string newStr)
        {
            return str.Replace(oldStr, newStr);
        }

        //------------------------------------------------------------------
        [ScriptMethod(nameof(String) + "." + nameof(Insert), "",
            "RS=>String_Insert"//"文字列への挿入：\n<str>文字列の <startIndex> 文字目に <value> 文字列を挿入します。"
            )]
        public static string Insert(string str, int startIndex, string value)
        {
            return str.Insert(startIndex, value);
        }

        //------------------------------------------------------------------
        [ScriptMethod(nameof(String) + ".IndexOf." + "IndexOf(char)", "IndexOf",
            "RS=>String_IndexOf"//"文字列への文字検索：\n<str>文字列にある <value> 文字の位置を参照します。"
            )]
        public static int IndexOf(string str, char value)
        {
            return str.IndexOf(value);
        }

        //------------------------------------------------------------------
        [ScriptMethod(nameof(String) + ".IndexOf." + "IndexOf(string)", "IndexOf",
            "RS=>String_IndexOf_s"//"<str>文字列にある <value> 文字列の位置を参照します。"
            )]
        public static int IndexOf(string str, string value)
        {
            return str.IndexOf(value);
        }

        //------------------------------------------------------------------
        [ScriptMethod(nameof(String) + ".IndexOf." + "IndexOf(string, int)", "IndexOf",
            "RS=>String_IndexOf_s_n"//"<str>文字列の<index>文字以降にある<value>文字列の位置を参照します。"
            )]
        public static int IndexOf(string str, string value, int index)
        {
            return str.IndexOf(value, index);
        }

        //------------------------------------------------------------------
        [ScriptMethod(nameof(String) + ".IndexOf." + "IndexOf(string, int, int)", "IndexOf",
            "RS=>String_IndexOf_s_n_n"//"<str>文字列の<index>文字以降<length>文字内にある<value>文字列の位置を参照します。"
            )]
        public static int IndexOf(string str, string value, int index, int length)
        {
            return str.IndexOf(value, index, length);
        }

        //------------------------------------------------------------------
        [ScriptMethod(nameof(String) + "." + nameof(Substring), "",
            "RS=>String_Substring"//"文字列の部分参照：\n<str>文字列の <startIndex> 文字目から <length> 文字を参照します。"
            )]
        public static string Substring(string str, int startIndex, int length)
        {
            return str.Substring(startIndex, length);
        }

        //------------------------------------------------------------------
        [ScriptMethod(nameof(String) + "." + nameof(Remove), "",
            "RS=>String_Remove"//"文字列の削除：\n<str>文字列の <startIndex> 文字目から <length> 文字を削除します。"
            )]
        public static string Remove(string str, int startIndex, int length)
        {
            return str.Remove(startIndex, length);
        }

        //------------------------------------------------------------------
        [ScriptMethod(nameof(String) + "." + nameof(IsNullOrEmpty))]
        public static bool IsNullOrEmpty(string str)
        {
            return string.IsNullOrEmpty(str);
        }
    }
}
