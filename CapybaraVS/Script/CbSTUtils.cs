using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        static public string EnumCbTypeToString(CbType cbType)
        {
            switch (cbType)
            {
                case CbType.none: return cbType.ToString();
                case CbType.Int: return INT_STR;
                case CbType.String: return STRING_STR;
                case CbType.Double: return DOUBLE_STR;
                case CbType.Byte: return BYTE_STR;
                case CbType.Sbyte: return SBYTE_STR;
                case CbType.Long: return LONG_STR;
                case CbType.Short: return SHORT_STR;
                case CbType.UShort: return USHORT_STR;
                case CbType.UInt: return UINT_STR;
                case CbType.ULong: return ULONG_STR;
                case CbType.Char: return CHAR_STR;
                case CbType.Float: return FLOAT_STR;
                case CbType.Decimal: return DECIMAL_STR;
                case CbType.Bool: return BOOL_STR;
                case CbType.Object: return OBJECT_STR;
                case CbType.Class: return CLASS_STR;
                case CbType.Func: return FUNC_STR;
                default:
                    Debug.Assert(false);
                    break;
            }

            return "";
        }

        static public Dictionary<string, string> CbTypeNameList = new Dictionary<string, string>()
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

        static private List<string> eqTypeList = new List<string>()
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
            bool isNotGeneName = false;
            if (type.IsGenericType)
            {
                // ジェネリック型内のジェネリックCbクラス用

                if (type.GetGenericTypeDefinition() == typeof(CbClass<>) ||
                    type.GetGenericTypeDefinition() == typeof(CbEnum<>))
                {
                    isNotGeneName = true;
                }
            }

            string typeName = type.FullName;
            string geneString = "";

            if (CbTypeNameList.ContainsKey(typeName))
            {
                return CbTypeNameList[typeName];
            }

            if (type.IsGenericType)
            {
                // ジェネリック引数文字以降を削除
                typeName = typeName.Substring(0, typeName.IndexOf("`"));

                foreach (Type arg in type.GenericTypeArguments)
                {
                    string newName = _GetTypeName(arg);
                    if (geneString.Length != 0)
                        geneString += ",";
                    geneString += newName;
                }
                if (!isNotGeneName)
                    geneString = "<" + geneString + ">";
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
        /// <param name="toName"></param>
        /// <param name="fromName"></param>
        /// <param name="toType"></param>
        /// <param name="fromType"></param>
        /// <param name="isCast"></param>
        /// <returns></returns>
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

            if (toName == STRING_STR)
            {
                if (fromName == OBJECT_STR)
                    return false;

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
        /// <param name="toName">代入元の型名</param>
        /// <param name="fromName">代入先の型名</param>
        /// <returns></returns>
        public static bool IsCastAssignment(string toName, string fromName)
        {
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
    }
}
