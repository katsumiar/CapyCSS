using System;
using System.Reflection;

namespace CapyCSS.Script
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
        string ValueUIString { get; }

        /// <summary>
        /// 値を初期化します。
        /// </summary>
        void Initialize();
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
            return _StructValue(type, name, typeof(CbStruct<>));
        }

        /// <summary>
        /// オリジナル型情報から CbNullableStruct<type>型の変数を返します。
        /// </summary>
        /// <param name="type">オリジナルのクラスの型</param>
        /// <param name="name"></param>
        /// <returns>CbStruct<T>型の変数</returns>
        public static ICbValue NullableStructValue(Type type, string name)
        {
            return _StructValue(type, name, typeof(CbNullableStruct<>));
        }

        private static ICbValue _StructValue(Type type, string name, Type openedType)
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
                // ref-like型構造体は、ジェネリック型引数にできない

                return null;
            }

            Type cbStructType = openedType.MakeGenericType(type);

            object result = cbStructType.InvokeMember(
                        nameof(CbStruct<dummy>.Create),//"Create",
                        BindingFlags.InvokeMethod,
                        null, null, new object[] { name }) as ICbValue;
            return result as ICbValue;
        }

        private struct dummy {};

        /// <summary>
        /// 構造体を判定します。
        /// </summary>
        /// <param name="type">true==構造体</param>
        /// <returns></returns>
        public static bool IsStruct(System.Type type)
        {
            if (type is null)
            {
                return false;
            }
            if (type.IsByRef)
            {
                // リファレンスの場合は、リファレンスで無い場合の型情報を評価する（スクリプトの仕組み上の条件）

                string name = type.FullName;
                if (name is null)
                    name = type.Name;

                return IsStruct(CbST.GetTypeEx(name.Replace("&", "")));
            }
            return type.IsValueType && !type.IsPrimitive && !type.IsEnum;
        }
    }

    /// <summary>
    /// struct型
    /// </summary>
    public class CbStruct<T> 
        : BaseCbValueClass<T>
        , ICbValueClass<T>
        , ICbStruct
         where T : struct
    {
        public override Type MyType => typeof(CbStruct<T>);

        public string ItemName => typeof(T).FullName;

        public CbStruct(T n, string name = "")
        {
            Value = n;
            Name = name;
        }

        public CbStruct(string name = "")
        {
            Name = name;
        }

        public void Initialize()
        {
            Value = (T)Activator.CreateInstance(typeof(T));
        }

        public override string TypeName
        {
            get
            {
                return CbSTUtils.GetTypeName(typeof(T));
            }
        }

        public override Func<ICbValue> NodeTF => TF;

        /// <summary>
        /// 値のUI上の文字列表現
        /// </summary>
        public override string ValueUIString
        {
            get
            {
                string baseName = "[" + TypeName + "]";
                if (IsError)
                    return CbSTUtils.ERROR_STR;
                return baseName;
            }
        }

        /// <summary>
        /// 値の文字列表現
        /// </summary>
        public override string ValueString
        {
            get => Value.ToString();
            set => new NotImplementedException();
        }

        public override bool IsStringableValue => false;

        public override bool IsReadOnlyValue { get; set; } = true;

        public static CbStruct<T> Create(string name = "") => new CbStruct<T>(name);

        public static CbStruct<T> Create(T n, string name = "") => new CbStruct<T>(n, name);

        public static Func<ICbValue> TF = () => Create();
        public static Func<string, ICbValue> NTF = (name) => Create(name);

        private bool disposedValue;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    ClearWork();
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }



    public class CbNullableStruct<T>
        : CbStruct<T>
     where T : struct
    {
        public override Type MyType => typeof(CbNullableStruct<T>);

        public CbNullableStruct(T n, string name = "")
            : base(n, name) {}

        public CbNullableStruct(string name = "")
            : base(name) {}

        /// <summary>
        /// null許容型か？
        /// </summary>
        public override bool IsNullable => true;

        /// <summary>
        /// 値のUI上の文字列表現
        /// </summary>
        public override string ValueUIString
        {
            get
            {
                string baseName = "[" + TypeName + "]";
                if (IsError)
                    return CbSTUtils.ERROR_STR;
                if (IsNull)
                    return $"{baseName}{CbSTUtils.UI_NULL_STR}";
                return baseName;
            }
        }

        /// <summary>
        /// 値の文字列表現
        /// </summary>
        public override string ValueString
        {
            get
            {
                if (IsNull)
                {
                    string baseName = "[" + TypeName + "]";
                    return $"{baseName}{CbSTUtils.UI_NULL_STR}";
                }
                else
                {
                    return Value.ToString();
                }
            }
            set => new NotImplementedException();
        }

        public override object Data
        {
            get
            {
                if (isNull)
                {
                    return null;
                }
                return Value as object;
            }
            set
            {
                if (value is null)
                {
                    isNull = true;
                }
                else
                {
                    Value = (dynamic)value;
                }
            }
        }

        public static new CbNullableStruct<T> Create(string name = "") => new CbNullableStruct<T>(name);

        public static new CbNullableStruct<T> Create(T n, string name = "") => new CbNullableStruct<T>(n, name);

        public static new Func<ICbValue> TF = () => Create();
        public static new Func<string, ICbValue> NTF = (name) => Create(name);
    }
}
