﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
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
                return EnumValue(CbST.GetTypeEx(cbEnum.ItemName), name);
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
            if (type is null)
            {
                return null;
            }
            string typeName = type.FullName;
            if (type.IsByRef)
            {
                // リファレンス（スクリプト変数接続）

                typeName = typeName.Replace("&", "");
                type = CbST.GetTypeEx(typeName);
            }
            if (type.IsByRefLike)
            {
                // ref-like型構造体は、ジェネリック型引数にできない（不要だろうけど…）

                return null;
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

        public string[] ElementList => Enum.GetNames(typeof(T));

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
