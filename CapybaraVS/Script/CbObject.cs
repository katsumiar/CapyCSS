using System;
using CbVS.Script;

namespace CapybaraVS.Script
{
    /// <summary>
    /// object 型
    /// </summary>
    public class CbObject : BaseCbValueClass<object>, ICbValueClass<object>
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

                    if (cbVSValue is ICbList cbList)
                        newValue = cbList.CreateTF();
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
                if (n is ICbEvent cbEvent)
                {
                    Value = n;
                }
                else
                {
                    Value = n.Data;
                }

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

        public override string ValueString
        {
            get 
            {
                if (IsError)
                    return ERROR_STR;
                if (IsNull)
                {
                    return $"[{CbSTUtils.OBJECT_STR}]" + NULL_STR;
                }
                else if (Value is ICbEvent cbEvent)
                {
                    return $"{cbEvent.TypeName}()";
                }
                if (copyOriginal is null)
                {
                    return Value.ToString();
                }
                return copyOriginal.ValueString;
            }
            set => new NotImplementedException();
        }
        public override bool IsStringableValue => false;

        public override bool IsReadOnlyValue { get; set; } = true;

        public static CbObject Create(string name)
        {
            var ret = new CbObject(null, name);
            return ret;
        }

        public static CbObject Create(object n = null, string name = "")
        {
            var ret = new CbObject(n, name);
            return ret;
        }

        public override bool IsNull => nullFlg;

        public static Func<ICbValue> TF = () => CbObject.Create();
        public static Func<string, ICbValue> NTF = (name) => CbObject.Create(name);
    }
}
