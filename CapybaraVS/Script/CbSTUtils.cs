﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using CapyCSS.Script;
using CbVS.Script;

namespace CapybaraVS.Script
{
    //----------------------------------------------------------------------------------------
    public class CbSTUtils
    {
        // 文字列部分を変更すると型名が全体的に適用されます。

        public const string VOID_STR = "void";
        public const string FUNC_STR = "Func";
        public const string ACTION_STR = "Action";
        public const string LITERAL_LIST_STR = "List";
        public const string LIST_STR = "ICollection";
        public const string CLASS_STR = "Class";
        public const string ENUM_STR = "Enum";
        public const string STRUCT_STR = "Struct";
        public const string INTERFACE_STR = "Interface";

        public const string OBJECT_STR = "object";
        public const string INT_STR = "int";
        public const string BYTE_STR = "byte";
        public const string SBYTE_STR = "sbyte";
        public const string SHORT_STR = "short";
        public const string LONG_STR = "long";
        public const string USHORT_STR = "ushort";
        public const string UINT_STR = "uint";
        public const string ULONG_STR = "ulong";
        public const string CHAR_STR = "char";
        public const string FLOAT_STR = "float";
        public const string DOUBLE_STR = "double";
        public const string DECIMAL_STR = "decimal";
        public const string BOOL_STR = "bool";
        public const string STRING_STR = "string";
        public const string TEXT_STR = "text";

        public const string GENELICS_GROUP_STR = "genelics.";
        public const string INTERFACE_GROUP_STR = "interface.";
        public const string SIGNED_GROUP_STR = "signed.";
        public const string UNSIGNED_GROUP_STR = "unsigned.";
        public const string ACTION_GROUP_STR = "action.";
        public const string FUNC_GROUP_STR = "func.";

        public static readonly Type LIST_TYPE = typeof(List<>);
        public static readonly Type LIST_INTERFACE_TYPE = typeof(IEnumerable<>);
        public static readonly Type COLLECTION_INTERFACE_TYPE = typeof(ICollection<>);
        public static readonly Type INDEX_INTERFACE_TYPE = typeof(IList<>);
        public static readonly Type FUNC_TYPE = typeof(Func<>);
        public static readonly Type FUNC2ARG_TYPE = typeof(Func<,>);
        public static readonly Type ACTION_TYPE = typeof(Action<>);
        public static readonly Type DUMMY_TYPE = typeof(int); // ダミー

        public const string ERROR_STR = "[ERROR]";          // エラーの表現
        public const string NULL_STR = "null";              // nullの表現
        public const string DELEGATE_STR = "[delegate]";    // dlegateの表現
        public const string UI_NULL_STR = "<null>";         // UI上のnullの表現

        public const string MENU_STATIC = "[static]";       // コマンドメニュー上での静的表現
        public const string MENU_VIRTUAL = "[override]";    // コマンドメニュー上でのオーバーライド表現

        public const string MENU_GETTER = ".(getter).";     // コマンドメニュー上でのゲッターグループ表現
        public const string MENU_SETTER = ".(setter).";     // コマンドメニュー上でのセッターグループ表現
        public const string MENU_CONSTRUCTOR = ".(new).";   // コマンドメニュー上でのコンストラクタグループ表現

        /// <summary>
        /// ユーザーによる型作成時に組み込み型選択肢に出てくる型情報です。
        /// </summary>
        static public readonly Dictionary<string, string> BuiltInTypeList = new Dictionary<string, string>()
        {
            { typeof(List<>).FullName, GENELICS_GROUP_STR + "List<>" },
            { typeof(IList<>).FullName, INTERFACE_GROUP_STR + "IList<>" },
            { typeof(ICollection<>).FullName, INTERFACE_GROUP_STR + "ICollection<>" },
            { typeof(IEnumerable<>).FullName, INTERFACE_GROUP_STR + "IEnumerable<>" },
            { typeof(IDictionary<,>).FullName, INTERFACE_GROUP_STR + "IDictionary<,>" },

            { typeof(Action).FullName, ACTION_GROUP_STR + "Action" },
            { typeof(Action<>).FullName, ACTION_GROUP_STR + "Action<>" },
            { typeof(Action<,>).FullName, ACTION_GROUP_STR + "Action<,>" },
            { typeof(Action<,,>).FullName, ACTION_GROUP_STR + "Action<,,>" },
            { typeof(Action<,,,>).FullName, ACTION_GROUP_STR + "Action<,,,>" },
            { typeof(Action<,,,,>).FullName, ACTION_GROUP_STR + "Action<,,,,>" },
            { typeof(Action<,,,,,>).FullName, ACTION_GROUP_STR + "Action<,,,,,>" },
            { typeof(Action<,,,,,,>).FullName, ACTION_GROUP_STR + "Action<,,,,,,>" },
            { typeof(Action<,,,,,,,>).FullName, ACTION_GROUP_STR + "Action<,,,,,,,>" },
            { typeof(Action<,,,,,,,,>).FullName, ACTION_GROUP_STR + "Action<,,,,,,,,>" },
            { typeof(Action<,,,,,,,,,>).FullName, ACTION_GROUP_STR + "Action<,,,,,,,,,>" },
            { typeof(Action<,,,,,,,,,,>).FullName, ACTION_GROUP_STR + "Action<,,,,,,,,,,>" },
            { typeof(Action<,,,,,,,,,,,>).FullName, ACTION_GROUP_STR + "Action<,,,,,,,,,,,>" },
            { typeof(Action<,,,,,,,,,,,,>).FullName, ACTION_GROUP_STR + "Action<,,,,,,,,,,,,>" },
            { typeof(Action<,,,,,,,,,,,,,>).FullName, ACTION_GROUP_STR + "Action<,,,,,,,,,,,,,>" },
            { typeof(Action<,,,,,,,,,,,,,,>).FullName, ACTION_GROUP_STR + "Action<,,,,,,,,,,,,,,>" },
            { typeof(Action<,,,,,,,,,,,,,,,>).FullName, ACTION_GROUP_STR + "Action<,,,,,,,,,,,,,,,>" },

            { typeof(Func<>).FullName, FUNC_GROUP_STR + "Func<>" },
            { typeof(Func<,>).FullName, FUNC_GROUP_STR + "Func<,>" },
            { typeof(Func<,,>).FullName, FUNC_GROUP_STR + "Func<,,>" },
            { typeof(Func<,,,>).FullName, FUNC_GROUP_STR + "Func<,,,>" },
            { typeof(Func<,,,,>).FullName, FUNC_GROUP_STR + "Func<,,,,>" },
            { typeof(Func<,,,,,>).FullName, FUNC_GROUP_STR + "Func<,,,,,>" },
            { typeof(Func<,,,,,,>).FullName, FUNC_GROUP_STR + "Func<,,,,,,>" },
            { typeof(Func<,,,,,,,>).FullName, FUNC_GROUP_STR + "Func<,,,,,,,>" },
            { typeof(Func<,,,,,,,,>).FullName, FUNC_GROUP_STR + "Func<,,,,,,,,>" },
            { typeof(Func<,,,,,,,,,>).FullName, FUNC_GROUP_STR + "Func<,,,,,,,,,,>" },
            { typeof(Func<,,,,,,,,,,>).FullName, FUNC_GROUP_STR + "Func<,,,,,,,,,,,>" },
            { typeof(Func<,,,,,,,,,,,>).FullName, FUNC_GROUP_STR + "Func<,,,,,,,,,,,>" },
            { typeof(Func<,,,,,,,,,,,,>).FullName, FUNC_GROUP_STR + "Func<,,,,,,,,,,,,>" },
            { typeof(Func<,,,,,,,,,,,,,>).FullName, FUNC_GROUP_STR + "Func<,,,,,,,,,,,,,>" },
            { typeof(Func<,,,,,,,,,,,,,,>).FullName, FUNC_GROUP_STR + "Func<,,,,,,,,,,,,,,>" },
            { typeof(Func<,,,,,,,,,,,,,,,>).FullName, FUNC_GROUP_STR + "Func<,,,,,,,,,,,,,,,>" },
            { typeof(Func<,,,,,,,,,,,,,,,,>).FullName, FUNC_GROUP_STR + "Func<,,,,,,,,,,,,,,,,>" },

            { typeof(Byte).FullName, UNSIGNED_GROUP_STR + BYTE_STR },
            { typeof(SByte).FullName, SIGNED_GROUP_STR + SBYTE_STR },
            { typeof(Int16).FullName, SIGNED_GROUP_STR + SHORT_STR },
            { typeof(Int32).FullName, SIGNED_GROUP_STR + INT_STR },
            { typeof(Int64).FullName, SIGNED_GROUP_STR + LONG_STR },
            { typeof(UInt16).FullName, UNSIGNED_GROUP_STR + USHORT_STR },
            { typeof(UInt32).FullName, UNSIGNED_GROUP_STR + UINT_STR },
            { typeof(UInt64).FullName, UNSIGNED_GROUP_STR + ULONG_STR },
            { typeof(Char).FullName, CHAR_STR },
            { typeof(Single).FullName, FLOAT_STR },
            { typeof(Double).FullName, DOUBLE_STR },
            { typeof(Decimal).FullName, DECIMAL_STR },
            { typeof(Boolean).FullName, BOOL_STR },
            { typeof(String).FullName, STRING_STR },
            { typeof(Object).FullName, OBJECT_STR },
        };

        static public readonly Dictionary<string, string> CbTypeNameList = new Dictionary<string, string>()
        {
            // 型名変換
            { nameof(Byte), BYTE_STR },
            { nameof(SByte), SBYTE_STR },
            { nameof(Int16), SHORT_STR},
            { nameof(Int32), INT_STR },
            { nameof(Int64), LONG_STR },
            { nameof(UInt16), USHORT_STR },
            { nameof(UInt32), UINT_STR },
            { nameof(UInt64), ULONG_STR },
            { nameof(Char), CHAR_STR },
            { nameof(Single), FLOAT_STR },
            { nameof(Double), DOUBLE_STR },
            { nameof(Decimal), DECIMAL_STR },
            { nameof(Boolean), BOOL_STR },
            { nameof(String), STRING_STR},
            { nameof(Object), OBJECT_STR },

            { nameof(CbInt), INT_STR },
            { nameof(CbString), STRING_STR},
            { nameof(CbText), TEXT_STR },
            { nameof(CbDouble), DOUBLE_STR },
            { nameof(CbByte), BYTE_STR },
            { nameof(CbSByte), SBYTE_STR },
            { nameof(CbShort), SHORT_STR },
            { nameof(CbLong), LONG_STR },
            { nameof(CbUShort), USHORT_STR },
            { nameof(CbUInt), UINT_STR },
            { nameof(CbULong), ULONG_STR },
            { nameof(CbChar), CHAR_STR },
            { nameof(CbFloat), FLOAT_STR },
            { nameof(CbDecimal), DECIMAL_STR },
            { nameof(CbBool), BOOL_STR },
            { nameof(CbObject), OBJECT_STR },
            { nameof(CbList), LIST_STR },

            { nameof(CbVoid), VOID_STR },
        };

        private static ReaderWriterLock CbTypeNameListRwLock = new ReaderWriterLock();

        /// <summary>
        /// オブジェクトの型の文字列名を返します。
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        static public string GetTypeName(object obj)
        {
            return GetTypeName(obj.GetType());
        }

        /// <summary>
        /// オブジェクトの型の文字列名を返します。
        /// </summary>
        /// <param name="type">型情報</param>
        /// <returns></returns>
        static public string GetTypeName(Type type)
        {
            string typeName = type.FullName;
            string newName = _GetTypeName(type);

            try
            {
                CbTypeNameListRwLock.AcquireWriterLock(Timeout.Infinite);
                if (!CbTypeNameList.ContainsKey(typeName))
                {
                    CbTypeNameList.Add(typeName, newName);
                }
            }
            finally
            {
                CbTypeNameListRwLock.ReleaseWriterLock();
            }
            return newName;
        }

        static public string _GetTypeName(Type type)
        {
            string ret = __GetTypeName(type);
            if (type.IsArray)
            {
                // 外した配列を付け直す

                ret += "[]";
            }
            return ret;
        }

        static public string __GetTypeName(Type type)
        {
            bool isNotGeneName = false;
            if (type.IsGenericType)
            {
                // ジェネリック型内のジェネリックCbクラス用

                if (type.GetGenericTypeDefinition() == typeof(CbClass<>) ||
                    type.GetGenericTypeDefinition() == typeof(CbStruct<>) ||
                    type.GetGenericTypeDefinition() == typeof(CbEnum<>))
                {
                    isNotGeneName = true;
                }
            }

            string typeName = type.FullName;
            if (typeName is null)
            {
                typeName = type.Name;
            }
            string geneString = "";

            if (type.IsArray)
            {
                // 配列は一先ず外す

                typeName = typeName.Replace("[]", "");
            }

            try
            {
                CbTypeNameListRwLock.AcquireReaderLock(Timeout.Infinite);
                if (CbTypeNameList.ContainsKey(typeName))
                {
                    return CbTypeNameList[typeName];
                }
            }
            finally
            {
                CbTypeNameListRwLock.ReleaseReaderLock();
            }

            if (type.IsGenericType && typeName.Contains("`"))
            {
                if (typeName.Contains("`"))
                {
                    // ジェネリック引数文字以降を削除

                    typeName = typeName.Substring(0, typeName.IndexOf("`"));
                }

                foreach (Type arg in type.GenericTypeArguments)
                {
                    string newName = _GetTypeName(arg);
                    if (geneString.Length != 0)
                    {
                        geneString += ",";
                    }
                    geneString += newName;
                }
                if (!isNotGeneName)
                {
                    if (geneString == "")
                    {
                        // IsClass だと思われる

                        // TODO 詳細を調べる
                    }
                    else
                    {
                        geneString = "<" + geneString + ">";
                    }
                }
            }

            // ネームスペースを省略できるかチェックをここで行い、省略できるなら省略する
            typeName = Optimisation(typeName);

            try
            {
                CbTypeNameListRwLock.AcquireReaderLock(Timeout.Infinite);
                if (CbTypeNameList.ContainsKey(typeName))
                {
                    typeName = CbTypeNameList[typeName];
                }
            }
            finally
            {
                CbTypeNameListRwLock.ReleaseReaderLock();
            }

            if (type.IsEnum || type.IsInterface || type.IsClass)
            {
                // enum定義のネームスペースを削除

                typeName = typeName.Substring(typeName.IndexOf("+") + 1, typeName.Length - typeName.IndexOf("+") - 1);
            }

            if (isNotGeneName)
                return Optimisation(geneString);

            if (typeName == "Nullable")
            {
                typeName = geneString.Substring(1).Split('>')[0] + "?";
                geneString = "";
            }

            return Optimisation(typeName + geneString);
        }

        /// <summary>
        /// 型名のネームスペースを最小限して返します。
        /// </summary>
        /// <param name="typeName"></param>
        /// <returns></returns>
        private static string Optimisation(string typeName)
        {
#if true
            int pos = typeName.LastIndexOf(".");
            if (pos != -1)
            {
                typeName = typeName.Substring(pos + 1);
            }
            return typeName;
#else
            string[] arr = typeName.Split('.');
            List<string> testName = new List<string>();
            foreach (var node in arr)
            {
                testName.Add(node);
            }
            testName.Reverse();
            string reverseName = testName[0];
            for (int i = 1; i < testName.Count; ++i)
            {
                try
                {
                    CbTypeNameListRwLock.AcquireReaderLock(Timeout.Infinite);
                    if (!CbTypeNameList.ContainsValue(reverseName))
                    {
                        break;
                    }
                }
                finally
                {
                    CbTypeNameListRwLock.ReleaseReaderLock();
                }
                reverseName += "." + testName[i];
            }
            string[] arr2 = reverseName.Split('.');
            List<string> reverseWork = new List<string>();
            foreach (var node in arr2)
            {
                reverseWork.Add(node);
            }
            reverseWork.Reverse();
            string newName = string.Join(".", reverseWork);
            return newName;
#endif
        }

        /// <summary>
        /// 代入の可否を判定します。
        /// </summary>
        /// <param name="toName">代入先の型名</param>
        /// <param name="fromName">代入元の型名</param>
        /// <param name="toType">代入先の型情報</param>
        /// <param name="fromType">代入元の型情報</param>
        /// <param name="isCast">キャストでの判定なら true</param>
        /// <returns>接続可能なら true</returns>
        static public bool IsAssignment(
            Type toType, 
            Type fromType, 
            bool isCast = false
            )
        {
            /// <summary>
            /// キャストを通しての代入の可否を判定します。
            /// </summary>
            bool IsCastAssignment(Type toType, Type fromType)
            {
                if (fromType == typeof(object))
                    return true;    // 接続元が object なら無条件でキャスト可能

                if (!CbScript.IsValueType(fromType) || !CbScript.IsValueType(toType))
                    return false;

                // 以下、値を表現する型のみ

                if (fromType == typeof(decimal) && toType == typeof(char))
                    return false;

                if (fromType == typeof(char) &&
                    (toType == typeof(decimal) || toType == typeof(float) || toType == typeof(double)))
                    return false;

                if (fromType == typeof(ulong) || fromType == typeof(uint) || fromType == typeof(ushort) || fromType == typeof(byte))
                    return true;

                return true;
            }

            if (toType == typeof(object))
                return true;    // object型なら無条件に繋がる
            
            if (CbFunc.IsActionType(toType))
                return true;    // Action型なら無条件に繋がる

            if (fromType == typeof(CbVoid))
                return false;   // object と Action 以外には繋がらない

            if (toType == typeof(string))
                return true;    // CbVoid型以外なら無条件に繋がる

            if (isCast && IsCastAssignment(toType, fromType))
                return true;    // Cast接続なら繋がる

            if (CbFunc.IsFuncType(toType))
            {
                Type argType = toType.GetGenericArguments()[0]; // Func の返り値の型
                if (IsAssignment(argType, fromType, isCast))
                    return true;    // Func の返り値の型に代入可能なら繋がる
            }

            if (toType.IsAssignableFrom(fromType))
                return true;    // 繋がる

            return false;
        }

        /// <summary>
        /// 型カテゴリの名前を返します。
        /// </summary>
        /// <param name="type">型情報</param>
        /// <returns>グループ名</returns>
        public static string GetTypeGroupName(Type type)
        {
            if (type.IsEnum)
            {
                return CbSTUtils.ENUM_STR;
            }
            else if (type.IsInterface)
            {
                return CbSTUtils.INTERFACE_STR;
            }
            else if (CbStruct.IsStruct(type))
            {
                return CbSTUtils.STRUCT_STR;
            }
            else if (type.IsClass)
            {
                return CbSTUtils.CLASS_STR;
            }
            return null;
        }


        public static string MakeGroupedTypeNameWithOutNameSpace(Type type)
        {
            string result = MakeGroupedTypeName(type);
            if (result.Contains('.'))
            {
                return result.Split('.').Last();
            }
            return result;
        }

        /// <summary>
        /// 型情報から名前を作成します。
        /// </summary>
        /// <param name="type">型情報</param>
        /// <returns>名前</returns>
        public static string MakeGroupedTypeName(Type type)
        {
            string name = type.FullName.Replace('+', '.');
            string[] nss = name.Split('.');

            List<string> outNodes = new List<string>();
            foreach (var node in nss)
            {
                string outNode = node;
                if (node.Contains('`'))
                {
                    string gs = node.Split('`')[1];
                    outNode = node.Split('`')[0];
                    outNode += "<";
                    for (int i = 1; i < Int16.Parse(gs); ++i)
                    {
                        outNode += ",";
                    }
                    outNode += ">";
                }
                outNodes.Add(outNode);
            }
            return String.Join(".", outNodes);
        }

        public static string GetGenericParamatersString(MethodBase type)
        {
            return GetGenericParamatersString(type.GetGenericArguments());
        }

        public static string GetGenericParamatersString(Type type)
        {
            return GetGenericParamatersString(type.GetGenericArguments());
        }

        public static string GetGenericParamatersString(Type[] types)
        {
            if (types.Length == 0)
                return "";
            string ret = null;
            foreach (var ga in types)
            {
                string name = "";
                if (ga.IsGenericType)
                {
                    name = GetGenericTypeName(ga);
                }
                else
                {
                    if (ga.IsGenericParameter)
                        name = ga.Name;
                    else
                        name = GetTypeName(ga);
                }

                if (ret is null)
                    ret = $"<{name}";
                else
                    ret += $", {name}";
            }
            return ret + ">";
        }

        public static string GetClassNameOnly(Type type)
        {
            string name = type.Name;
            if (name.Contains("`"))
                name = name.Substring(0, name.IndexOf("`"));
            return name;
        }

        public static string GetGenericTypeName(Type type)
        {
            return GetClassNameOnly(type) + GetGenericParamatersString(type);
        }

        /// <summary>
        /// ジェネリック引数をストリップしたジェネリック引数を返します。
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string StripParamater(string name)
        {
            string _StripParamater(string outName)
            {
                var paramArea = new Tuple<char, char>('<', '>');
                string param = GetParamater(paramArea, outName);
                if (param is null)
                    return outName;
                int count = param.Length - param.Replace(",", "").Length;
                string repParam = "<";
                while (count-- != 0)
                    repParam += ",";
                repParam += ">";
                if (outName.Contains(", "))
                    outName = outName.Replace(param, repParam);
                else
                    outName = outName.Replace(param.Replace(" ", ""), repParam);
                return outName;
            }

            if (!name.Contains("<") || !name.Contains(">"))
                return name;

            string temp;
            do
            {
                temp = name;
                name = _StripParamater(name);
            }
            while (name != temp);

            return name;
        }

        /// <summary>
        /// ジェネリックパラメータに該当する文字列部分を取り出します。
        /// ※「,」の後ろにスペースを入れます。
        /// </summary>
        /// <param name="name">対象文字列</param>
        /// <returns>ジェネリックパラメータに該当する文字列部分</returns>
        public static string GetParamater(string name)
        {
            return CbSTUtils.GetParamater(new Tuple<char, char>('<', '>'), name);
        }

        public static string GetParamater(Tuple<char, char> tuple, string name)
        {
            int sPos = name.IndexOf(tuple.Item1);
            int ePos = name.IndexOf(tuple.Item2);
            if (sPos != -1 && ePos != -1)
            {
                string ret = name.Substring(sPos, ePos - sPos + 1);
                if (!ret.Contains(", ") && ret.Contains(","))
                {
                    ret = ret.Replace(",", ", ");
                }
                return ret;
            }
            return null;
        }

        /// <summary>
        /// 前方一致した文字列を削除した文字列を返します。
        /// </summary>
        /// <param name="str">対象の文字列</param>
        /// <param name="strip">削除する文字列</param>
        /// <param name="IgnoreCase">大文字小文字を無視するなら true</param>
        /// <returns>前方一致した文字列を削除した文字列</returns>
        public static string StartStrip(string str, string strip, bool IgnoreCase = false)
        {
            if (str.StartsWith(strip)
                || (IgnoreCase && str.ToUpper().StartsWith(strip.ToUpper()))
                )
            {
                return str.Substring(strip.Length);
            }
            return str;
        }

        /// <summary>
        /// リスト内の要素を Dispose します。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        public static void ForeachDispose<T>(ICollection<T> list)
        {
            if (list is null)
                return;
            foreach (var node in list)
            {
                if (node is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
            list.Clear();
        }

        /// <summary>
        /// リスト内の要素を Dispose します。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        public static void ForeachDispose<T1,T2>(IDictionary<T1,T2> list)
        {
            if (list is null)
                return;
            foreach (var node in list)
            {
                if (node.Key is IDisposable disposable)
                {
                    disposable.Dispose();
                }
                if (node.Value is IDisposable disposable2)
                {
                    disposable2.Dispose();
                }
            }
            list.Clear();
        }
    }
}
