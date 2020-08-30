using System;
using System.Collections.Generic;

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
