//#define SHOW_LINK_ARRAY   // リスト型を接続したときにリストの要素をコピーして表示する

using CbVS.Script;
using System;
using System.Diagnostics;
using System.Reflection;

namespace CapyCSS.Script
{
    /// <summary>
    /// class型インターフェイス
    /// </summary>
    public interface ICbClass
    {
        /// <summary>
        /// 変数の値
        /// </summary>
        object Data { get; }

        /// <summary>
        /// CbClass<T> の管理するT型の名前
        /// </summary>
        string ItemName { get; }

        /// <summary>
        /// null か？
        /// </summary>
        bool IsNull { get; }

        Type OriginalReturnType { get; }

        /// <summary>
        /// UI接続上の表示を文字列で参照します。
        /// </summary>
        string ValueUIString { get; }

        /// <summary>
        /// 値の変化後に動かす必要のある処理です。
        /// </summary>
        Action<object> ReturnAction { set; get; }
    }

    public class CbClass
    {
        /// <summary>
        /// CbClass<T> 型なら同様の型の変数を返します。
        /// </summary>
        /// <param name="value">参考変数</param>
        /// <param name="name">変数名</param>
        /// <returns>CbClass<T>型の変数</returns>
        public static ICbValue ClassValue(ICbValue value, string name)
        {
            if (value is ICbClass cbClass)
            {
                return ClassValue(cbClass.OriginalReturnType, name);
            }
            return null;
        }

        /// <summary>
        /// オリジナル型情報から CbClass<type>型の変数を返します。
        /// </summary>
        /// <param name="type">オリジナルのクラスの型</param>
        /// <param name="name"></param>
        /// <returns>CbClass<T>型の変数</returns>
        public static ICbValue ClassValue(Type type, string name)
        {
            if (type is null)
            {
                return null;
            }

            string typeName = type.FullName;
            if (typeName is null)
            {
                typeName = type.Name;
            }
            if (type.IsByRef)
            {
                // リファレンス（スクリプト変数接続）

                typeName = typeName.Replace("&", "");
                type = CbST.GetTypeEx(typeName);
                if (type is null)
                {
                    return null;
                }
            }
            if (type.IsByRefLike)
            {
                // ref-like型構造体は、ジェネリック型引数にできない

                return null;
            }
            if (!type.IsVisible && !type.IsTypeDefinition)
            {
                // 理由はわからないがこの条件は MakeGenericType が通らない

                return null;
            }
            if (type.IsPointer)
            {
                // ポインタ型は、対象外

                return null;
            }
            if (type.IsValueType)
            {
                // 未知の値型（今の所、対応する予定は無い）
                // IntPtr 型など

                return null;
            }

            Type openedType = typeof(CbClass<>); //CapybaraVS.Script.CbClass`1
            Type cbClassType = openedType.MakeGenericType(type);

            object result = cbClassType.InvokeMember(
                        nameof(CbClass<CbInt>.Create),//"Create",
                        BindingFlags.InvokeMethod,
                        null, null, new object[] { name }) as ICbValue;
            return result as ICbValue;
        }
    }

    /// <summary>
    /// class型
    /// </summary>
    public class CbClass<T> 
        : BaseCbValueClass<T>
        , ICbValueClass<T>
        , ICbClass
         where T : class
    {
        public override Type MyType => typeof(CbClass<T>);

        public string ItemName => typeof(T).FullName;

        public static Type GetItemType() {
            Debug.Assert(false);    // 参照されていない？
            return typeof(T);
        }

        public override Type OriginalReturnType => typeof(T);

        public override Type OriginalType => typeof(T);

#pragma warning disable 414  // 「割り当てられていますが、値は使用されていません」と言われる
        private bool nullFlg = true;

        public CbClass(T n, string name = "")
        {
            Value = n;
            Name = name;
        }

        public CbClass(string name = "")
        {
            Value = null;
            Name = name;
        }

        public override string TypeName
        {
            get
            {
                if (CbVoid.Is(typeof(T)))
                    return CbSTUtils.VOID_STR;
                return CbSTUtils.GetTypeName(typeof(T));
            }
        }

        public override Func<ICbValue> NodeTF => TF;

        /// <summary>
        /// 変数の持つ値を object として参照します。
        /// ※ 型を厳密に扱う場合は Value を参照します。
        /// </summary>
        public override object Data
        {
            get
            {
                Debug.Assert(!(IsNullable && IsNull));
                return Value as object;
            }
            set
            {
                Value = (T)value;
            }
        }

        public override T Value
        {
            get => _value;
            set
            {
                if (value is null)
                {
                    nullFlg = true;
                }
                else
                {
                    nullFlg = false;
                }
                _value = value;
            }
        }

        /// <summary>
        /// 値のUI上の文字列表現
        /// </summary>
        public override string ValueUIString
        {
            get
            {
                if (IsError)
                    return CbSTUtils.ERROR_STR;
                if (CbVoid.Is(typeof(T)))
                    return CbSTUtils.VOID_STR;
                string baseName = $"[{TypeName}]";
                if (IsNull)
                {
                    return baseName + CbSTUtils.UI_NULL_STR;
                }
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
                    return CbSTUtils.NULL_STR;
                }
                else
                {
                    return Value.ToString();
                }
            }
            set => new NotImplementedException();
        }

        public override void Set(ICbValue n)
        {
            try
            {
                if (n.IsError)
                    throw new Exception(n.ErrorMessage);

                ReturnAction = null;

                if (n is CbObject cbObject)
                {
                    n = (dynamic)cbObject.ValueTypeObject;
                }

                Debug.Assert(!IsList);
                Debug.Assert(!CbScript.IsCalcable(typeof(T)));

                if (this is CbClass<CbVoid>)
                {
                    // 何もしない
                }
                else if (IsNullable)
                {
                    Value = (dynamic)Convert.ChangeType(n.Data, OriginalReturnType);
                }
                else
                {
                    Value = (dynamic)n.Data;
                }

                isNull = n.IsNull;

                IsLiteral = n.IsLiteral;
                if (IsError)
                {
                    IsError = false;
                    ErrorMessage = "";
                }
            }
            catch (Exception ex)
            {
                IsError = true;
                ErrorMessage = ex.Message;
                throw;
            }
        }

        /// <summary>
        /// 変数の持つ値は null か？
        /// </summary>
        public override bool IsNull => Value is null;

        public override bool IsStringableValue => true;

        public override bool IsReadOnlyValue { get; set; } = true;

        public static CbClass<T> Create(string name = "") => new CbClass<T>(name);

        public static CbClass<T> Create(T n, string name = "") => new CbClass<T>(n, name);

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
                    if (!IsNull)
                    {
                        if (Value != null && Value is IDisposable cbValue)
                        {
                            cbValue.Dispose();
                        }
                        Value = null;
                    }
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
}
