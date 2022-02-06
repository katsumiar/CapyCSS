using System;
using System.Diagnostics;
using CbVS.Script;

namespace CapyCSS.Script
{
    /// <summary>
    /// object 型
    /// </summary>
    public class CbObject 
        : BaseCbValueClass<object>
        , ICbValueClass<object>
    {
        public override Type MyType => typeof(CbObject);

        private bool nullFlg = true;

        public CbObject(object n = null, string name = "")
        {
            Value = n;
            Name = name;
        }

        /// <summary>
        /// 持っている値の型で値を返す
        /// </summary>
        public ICbValue ValueTypeObject
        {
            get
            {
                if (IsNull)
                    return this;

                if (Value is ICbValue cbVSValue)
                {
                    ICbValue newValue = null;

                    if (cbVSValue.IsList)
                    {
                        ICbList cbList = cbVSValue.GetListValue;
                        newValue = cbList.CreateTF();
                    }
                    else
                        newValue = cbVSValue.NodeTF();
                    newValue.Set(cbVSValue);
                    // 型情報を復元

                    return newValue;
                }
                return this;
            }
        }

        private ICbValue copyOriginal = null;

        public override void Set(ICbValue n)
        {
            try
            {
                if (n.IsError)
                    throw new Exception(n.ErrorMessage);

                // 型情報を残す
                copyOriginal = n;
                if (n is ICbEvent || n is ICbClass)
                {
                    Value = n;
                }
                else
                {
                    if (n.IsNullable && n.IsNull)
                    {
                        // null許容型は、null のときに参照してはならない

                        Value = null;
                    }
                    else
                    {
                        Value = n.Data;
                    }
                }
                IsLiteral = n.IsLiteral;
                if (IsError)
                {
                    // エラーからの復帰

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

        public override string TypeName => CbSTUtils.OBJECT_STR;

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
                Value = value;
            }
        }

        public override object Value
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
                if (IsNull)
                {
                    return $"[{CbSTUtils.OBJECT_STR}]" + CbSTUtils.UI_NULL_STR;
                }
                else if (Value is ICbEvent cbEvent)
                {
                    return $"{cbEvent.TypeName}()";
                }
                else if (Value is ICbClass cbClass)
                {
                    return $"{cbClass.ValueUIString}";
                }
                else if (copyOriginal is null)
                {
                    return Value.ToString();
                }
                return copyOriginal.ValueUIString;
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
                    return Data.ToString();
                }
            }
            set => new NotImplementedException();
        }

        public override bool IsStringableValue => false;

        public override bool IsReadOnlyValue { get; set; } = true;

        public static CbObject Create(string name) => new CbObject(null, name);

        public static CbObject Create(object n = null, string name = "") => new CbObject(n, name);

        public override bool IsNull => nullFlg;

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
