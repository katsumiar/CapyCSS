using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        public const string LIST_STR = "List";
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

        public static readonly string FREE_LIST_TYPE_STR = typeof(List<>).FullName;
        public static readonly string FREE_FUNC_TYPE_STR = typeof(Func<>).FullName;
        public static readonly string FREE_FUNC2A_TYPE_STR = typeof(Func<,>).FullName;
        public static readonly string FREE_ACTION_TYPE_STR = typeof(Action<>).FullName;

        public const string FREE_ENUM_TYPE_STR = "<ENUM>";  // Enum型を要求する

        public const string FREE_TYPE_STR = "<FREE>";   // 型選択を要求する
        public static readonly string DUMMY_TYPE_STR = typeof(int).FullName; // ダミー

        public const string ERROR_STR = "[ERROR]";  // エラーの表現
        public const string NULL_STR = "<null>";    // nullの表現

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

        static private readonly List<string> eqTypeList = new List<string>()
        {
            USHORT_STR,
            UINT_STR,
            ULONG_STR,
            BYTE_STR,
            STRING_STR,
            BOOL_STR,
            OBJECT_STR,
        };

        /// <summary>
        /// オブジェクトの型の文字列名を返します。
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        static public string GetTypeName(object obj)
        {
            string typeName = obj.GetType().FullName;
            string newName = _GetTypeName(obj.GetType());
            if (!CbTypeNameList.ContainsKey(typeName))
            {
                CbTypeNameList.Add(typeName, newName);
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
            string geneString = "";

            if (type.IsArray)
            {
                // 配列は一先ず外す

                typeName = typeName.Replace("[]", "");
            }

            if (CbTypeNameList.ContainsKey(typeName))
            {
                return CbTypeNameList[typeName];
            }

            if (type.IsGenericType ||
                typeName.Contains("`")  // IsClass だと思われる
                )
            {
                // ジェネリック引数文字以降を削除
                typeName = typeName.Substring(0, typeName.IndexOf("`"));

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

            if (CbTypeNameList.ContainsKey(typeName))
            {
                typeName = CbTypeNameList[typeName];
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
                if (!CbTypeNameList.ContainsValue(reverseName))
                {
                    break;
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
            string toName, 
            string fromName, 
            Type toType, 
            Type fromType, 
            bool isCast = false
            )
        {
            if (CbFunc.IsCanConnect(toName, fromName))
            {
                // イベント接続

                if (!isCast)
                {
                    if (CbFunc.IsNormalConnect(toName, fromName))
                        return true;

                    return false;
                }

                return CbFunc.IsCastConnect(toName, fromName, toType, fromType);    // キャスト接続
            }

            if (toName == OBJECT_STR)
                return true;    // 代入先が object なら無条件で代入可能

            if (toName == VOID_STR)
                return false;   // 接続先が void なら無条件で代入禁止

            if (toName == fromName)
                return true;    // 型が同じなら代入可能

            if (toType.IsGenericType || fromType.IsGenericType)
            {
                // 一先ず、ジェネリックは完全一致だけ代入可能
                // TODO 判定方法を模索する

                if (toName == fromName)
                    return true;

                if (toType.IsAssignableFrom(fromType))
                    return true;

                return false;
            }

            if (fromName == OBJECT_STR)
            {
                // object からの代入ならキャストで許可

                if (isCast)
                {
                    return IsCastAssignment(toName, fromName);
                }
                return false;
            }

            if (toName == STRING_STR || toName == TEXT_STR)
            {
                return true;    // 文字列型になら変換可能
            }

            bool isToBase = CbTypeNameList.ContainsValue(toName);
            bool isFromBase = CbTypeNameList.ContainsValue(fromName);

            if (isToBase != isFromBase)
                return false;   // 片方だけが組み込み型の場合は、代入不可

            if (!isToBase)
            {
                // 組み込み型でない場合は、クラスやインターフェイスとして判定する

                if (toType.IsAssignableFrom(fromType))
                    return true;

                return false;
            }

            if (toName == BOOL_STR && isCast)
            {
                if (fromName == SBYTE_STR || fromName == SHORT_STR || fromName == INT_STR ||
                    fromName == LONG_STR || fromName == USHORT_STR || fromName == UINT_STR ||
                    fromName == ULONG_STR || fromName == DECIMAL_STR || fromName == BYTE_STR)
                {
                    return true;
                }
            }

            bool isToEq = eqTypeList.Contains(toName);
            bool isFromEq = eqTypeList.Contains(fromName);

            if (isToEq != isFromEq)
                return false;

            if (isCast)
            {
                return IsCastAssignment(toName, fromName);
            }

            return false;
        }

        /// <summary>
        /// キャストを通しての代入の可否を判定します。
        /// </summary>
        /// <param name="toName">代入先の型名</param>
        /// <param name="fromName">代入元の型名</param>
        /// <returns></returns>
        public static bool IsCastAssignment(string toName, string fromName)
        {
            if (fromName == OBJECT_STR)
                return true;    // 接続元が object なら無条件でキャスト可能

            if (fromName == DECIMAL_STR && toName == CHAR_STR)
                return false;

            if (fromName == CHAR_STR &&
                (toName == DECIMAL_STR || toName == FLOAT_STR || toName == DOUBLE_STR))
            {
                return false;
            }

            if (fromName == ULONG_STR || fromName == UINT_STR || fromName == USHORT_STR || fromName == BYTE_STR)
                return true;

            if (toName == STRING_STR || toName == BOOL_STR || toName == OBJECT_STR)
                return false;
            if (fromName == STRING_STR || fromName == BOOL_STR || fromName == OBJECT_STR)
                return false;

            return true;
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
    }
}
