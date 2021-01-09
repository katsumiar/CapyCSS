using System;
using System.Collections.Generic;
using System.Reflection;

namespace CapybaraVS.Script
{
    /// <summary>
    /// enum型インターフェイス
    /// </summary>
    public interface ICbEnum
    {
        /// <summary>
        /// CbEnum<T> の管理するT型の名前
        /// </summary>
        string ItemName { get; }
    }

    public class CbEnumTools
    {
        /// <summary>
        /// CbEnum<T> 型なら同様の型の変数を返します。
        /// </summary>
        /// <param name="value">参考変数</param>
        /// <param name="name">変数名</param>
        /// <returns>CbEnum<T>型の変数</returns>
        public static ICbValue EnumValue(ICbValue value, string name)
        {
            if (value is ICbEnum cbEnum)
            {
                return EnumValue(Type.GetType(cbEnum.ItemName), name);
            }
            return null;
        }

        /// <summary>
        /// オリジナル型情報から CbEnum<type>型の変数を返します。
        /// </summary>
        /// <param name="type">オリジナルの共用体の型</param>
        /// <param name="name">変数名</param>
        /// <returns>CbEnum<type>型の変数</returns>
        public static ICbValue EnumValue(Type type, string name)
        {
            string typeName = type.FullName;
            if (type.IsByRef)
            {
                // リファレンス（スクリプト変数接続）

                typeName = typeName.Replace("&", "");
                type = Type.GetType(typeName);
            }
            Type openedType = typeof(CbEnum<>); //CapybaraVS.Script.CbEnum`1
            Type cbEnumType = openedType.MakeGenericType(type);

            object result = cbEnumType.InvokeMember("Create", BindingFlags.InvokeMethod,
                        null, null, new object[] { name }) as ICbValue;
            return result as ICbValue;
        }
    }

    /// <summary>
    /// enum型
    /// </summary>
    public class CbEnum<T> : BaseCbValueClass<Enum>, ICbValueEnumClass<Enum>, ICbEnum
         where T : struct
    {
        public override Type MyType => typeof(CbEnum<T>);

        public override CbST CbType
        {
            get
            {
                CbST ret = new CbST(NodeTF());
                return ret;
            }
        }

        public string ItemName => typeof(T).FullName;

        public static Type GetItemType() { return typeof(T); }  // ※リフレクションから参照されている

        public CbEnum(T n, string name = "")
        {
            Value = n as Enum;
            Name = name;
        }

        public CbEnum(string name = "")
        {
            Value = Enum.Parse(typeof(T), Enum.GetNames(typeof(T))[0]) as Enum;
            Name = name;
        }

        public override Func<ICbValue> NodeTF => TF;

        public override string TypeName => CbSTUtils._GetTypeName(typeof(T));

        public string[] ElementList
        {
            get
            {
                if (typeof(T) == typeof(CbType))
                {
                    // CbType のときは名前を変換する

                    List<string> list = new List<string>();
                    foreach (CbType value in Enum.GetValues(typeof(CbType)))
                    {
                        list.Add(CbSTUtils.EnumCbTypeToString(value));
                    }
                    return list.ToArray();
                }
                return Enum.GetNames(typeof(T));
            }
        }

        public override string ValueString
        {
            get
            {
                if (IsError)
                    return ERROR_STR;
                return CbSTUtils._GetTypeName(typeof(T)) + "." + Value.ToString();
            }
            set
            {
                if (value.Contains("."))
                    value = value.Substring(value.IndexOf(".") + 1, value.Length - value.IndexOf(".") - 1);

                if (typeof(T) == typeof(CbType))
                {
                    // CbType のときは名前を変換する

                    Value = Enum.ToObject(typeof(T), Array.IndexOf(ElementList, value)) as Enum;
                }
                else
                {
                    int num;
                    if (int.TryParse(value, out num))
                    {
                        // 数字を要素へ変換する

                        Value = Enum.ToObject(typeof(T), num) as Enum;
                    }
                    else
                    {
                        Value = Enum.Parse(typeof(T), value) as Enum;
                    }
                }
            }
        }

        public override bool IsStringableValue => true;

        public static CbEnum<T> Create(string name = "")
        {
            return new CbEnum<T>(name);
        }

        public static CbEnum<T> Create(T n, string name = "")
        {
            return new CbEnum<T>(n, name);
        }

        public static Func<ICbValue> TF = () => CbEnum<T>.Create();
        public static Func<string, ICbValue> NTF = (name) => CbEnum<T>.Create(name);
    }
}
