using System;
using System.Reflection;

namespace CapybaraVS.Script
{
    /// <summary>
    /// struct型インターフェイス
    /// </summary>
    public interface ICbStruct
    {
        /// <summary>
        /// 変数の値
        /// </summary>
        object Data { get; }

        /// <summary>
        /// CbStruct<T> の管理するT型の名前
        /// </summary>
        string ItemName { get; }

        Type OriginalReturnType { get; }

        /// <summary>
        /// 値を文字列で参照します。
        /// </summary>
        string ValueString { get; }
    }

    public class CbStruct
    {
        /// <summary>
        /// CbStruct<T> 型なら同様の型の変数を返します。
        /// </summary>
        /// <param name="value">参考変数</param>
        /// <param name="name">変数名</param>
        /// <returns>CbStruct<T>型の変数</returns>
        public static ICbValue StructValue(ICbValue value, string name)
        {
            if (value is ICbStruct cbStruct)
            {
                return StructValue(cbStruct.OriginalReturnType, name);
            }
            return null;
        }

        /// <summary>
        /// オリジナル型情報から CbStruct<type>型の変数を返します。
        /// </summary>
        /// <param name="type">オリジナルのクラスの型</param>
        /// <param name="name"></param>
        /// <returns>CbStruct<T>型の変数</returns>
        public static ICbValue StructValue(Type type, string name)
        {
            string typeName = type.FullName;
            if (type.IsByRef)
            {
                // リファレンス（スクリプト変数接続）

                typeName = typeName.Replace("&", "");
                type = Type.GetType(typeName);
            }
            Type openedType = typeof(CbStruct<>); //CapybaraVS.Script.CbStruct`1
            Type cbStructType = openedType.MakeGenericType(type);

            object result = cbStructType.InvokeMember("Create", BindingFlags.InvokeMethod,
                        null, null, new object[] { name }) as ICbValue;
            return result as ICbValue;
        }

        /// <summary>
        /// 構造体を判定します。
        /// </summary>
        /// <param name="type">true==構造体</param>
        /// <returns></returns>
        public static bool IsStruct(System.Type type)
        {
            return type.IsValueType && !type.IsPrimitive && !type.IsEnum;
        }
    }

    /// <summary>
    /// struct型
    /// </summary>
    public class CbStruct<T> : BaseCbValueClass<T>, ICbValueClass<T>, ICbStruct
         where T : struct
    {
        public override Type MyType => typeof(CbStruct<T>);

        public override CbST CbType
        {
            get
            {
                CbST ret = new CbST(NodeTF());
                return ret;
            }
        }

        public string ItemName => typeof(T).FullName;

        public static Type GetItemType() { return typeof(T); }

        public override Type OriginalReturnType => typeof(T);

        public override Type OriginalType => typeof(T);

        public CbStruct(T n, string name = "")
        {
            Value = n;
            Name = name;
        }

        public CbStruct(string name = "")
        {
            Name = name;
        }

        public override string TypeName
        {
            get
            {
                return CbSTUtils._GetTypeName(typeof(T));
            }
        }

        public override Func<ICbValue> NodeTF => TF;

        public override T Value
        {
            get => _value;
            set
            {
                _value = value;
            }
        }

        public override string ValueString
        {
            get
            {
                string baseName = "[" + TypeName + "]";
                if (IsError)
                    return ERROR_STR;
                return baseName;
            }
            set => new NotImplementedException();
        }

        public override bool IsStringableValue => false;

        public override bool IsReadOnlyValue { get; set; } = true;

        public static CbStruct<T> Create(string name = "")
        {
            return new CbStruct<T>(name);
        }

        public static CbStruct<T> Create(T n, string name = "")
        {
            return new CbStruct<T>(n, name);
        }

        public static Func<ICbValue> TF = () => CbStruct<T>.Create();
        public static Func<string, ICbValue> NTF = (name) => CbStruct<T>.Create(name);
    }
}
