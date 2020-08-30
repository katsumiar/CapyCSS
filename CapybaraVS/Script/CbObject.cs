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

        /// <summary>
        /// 型情報
        /// </summary>
        public override CbST CbType
        {
            get
            {
                return new CbST(
                    Script.CbType.Func,
                    base.OriginalType.FullName   // 型名を持っていないとスクリプト読み込み時に再現できない
                    );
            }
        }

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

                    newValue.Data = cbVSValue.Data;

                    newValue.IsError = cbVSValue.IsError;
                    newValue.ErrorMessage = cbVSValue.ErrorMessage;
                    newValue.IsReadOnlyValue = cbVSValue.IsReadOnlyValue;

                    return newValue;
                }
                return this;
            }
        }

        public override void CopyValue(ICbValue cbVSValue)
        {
            if (cbVSValue is CbObject)
            {
                Value = cbVSValue.Data;
            }
            else
            {
                Value = cbVSValue;
            }
            IsError = cbVSValue.IsError;
            ErrorMessage = cbVSValue.ErrorMessage;
        }

        public override void Set(ICbValue n)
        {
            try
            {
                if (n.IsError)
                    throw new Exception(n.ErrorMessage);
                if (n is CbObject cbObject)
                {
                    Value = n.Data;
                }
                else
                {
                    Value = n;
                }
                IsError = n.IsError;
                ErrorMessage = n.ErrorMessage;
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
                string baseName = $"[{CbSTUtils.OBJECT_STR}]";
                if (IsError)
                    return ERROR_STR;
                if (IsNull)
                    return baseName + NULL_STR;
                return baseName;
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
