//#define SHOW_LINK_ARRAY   // リスト型を接続したときにリストの要素をコピーして表示する

using CbVS.Script;
using System;
using System.Reflection;

namespace CapybaraVS.Script
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

        Type OriginalReturnType { get; }

        /// <summary>
        /// UI接続上の表示を文字列で参照します。
        /// </summary>
        string ValueString { get; }

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

            object result = cbClassType.InvokeMember("Create", BindingFlags.InvokeMethod,
                        null, null, new object[] { name }) as ICbValue;
            return result as ICbValue;
        }
    }

    /// <summary>
    /// class型
    /// </summary>
    public class CbClass<T> : BaseCbValueClass<T>, ICbValueClass<T>, ICbClass
         where T : class
    {
        public override Type MyType => typeof(CbClass<T>);

        public string ItemName => typeof(T).FullName;

        public static Type GetItemType() { return typeof(T); }

        public override Type OriginalReturnType => typeof(T);

        public override Type OriginalType => typeof(T);

#pragma warning disable 414  // 使っているのに使っていないと言われる
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
                return CbSTUtils._GetTypeName(typeof(T));
            }
        }

        public override Func<ICbValue> NodeTF => TF;

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

        public override string ValueString
        {
            get
            {
                if (IsError)
                    return ERROR_STR;
                if (CbVoid.Is(typeof(T)))
                    return CbSTUtils.VOID_STR;
                string baseName = $"[{TypeName}]";
                if (IsNull)
                {
                    return baseName + NULL_STR;
                }
                return baseName;
            }
            set => new NotImplementedException();
        }

        public override bool IsStringableValue => true;

        public override bool IsReadOnlyValue { get; set; } = true;

        public static CbClass<T> Create(string name = "")
        {
            return new CbClass<T>(name);
        }

        public static CbClass<T> Create(T n, string name = "")
        {
            return new CbClass<T>(n, name);
        }

        public static Func<ICbValue> TF = () => CbClass<T>.Create();
        public static Func<string, ICbValue> NTF = (name) => CbClass<T>.Create(name);
    }
}
