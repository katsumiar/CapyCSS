using System;
using CbVS.Script;

namespace CapybaraVS.Script
{
    /// <summary>
    /// string 型
    /// </summary>
    public class CbString : BaseCbValueClass<string>, ICbValueClass<string>
    {
        public override Type MyType => typeof(CbString);

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

        public CbString(string n = "", string name = "")
        {
            Value = n;
            Name = name;
        }

        public override string ValueString 
        {
            get
            {
                if (IsError)
                    return ERROR_STR;
                return Value;
            }
            set
            {
                Value = value;
            }
        }

        public override void CopyValue(ICbValue cbVSValue)
        {
            if (cbVSValue.Data is string)
            {
                Data = cbVSValue.Data;
            }
            else
            {
                ValueString = cbVSValue.ValueString;
            }
            IsError = cbVSValue.IsError;
            ErrorMessage = cbVSValue.ErrorMessage;
            if (this is ICbEvent cbEvent && cbVSValue is ICbEvent cbEven2)
            {
                cbEvent.CallBack = cbEven2.CallBack;
            }
        }

        public static CbString Create(string name = "")
        {
            var ret = new CbString("", name);
            return ret;
        }

        public static CbString Create(string n, string name)
        {
            var ret = new CbString(n, name);
            return ret;
        }

        public static Func<ICbValue> TF = () => CbString.Create();
        public static Func<string, ICbValue> NTF = (name) => CbString.Create(name);
    }
}
