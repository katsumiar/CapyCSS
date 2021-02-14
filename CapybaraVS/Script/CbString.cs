using System;
using System.Diagnostics;
using CbVS.Script;

namespace CapybaraVS.Script
{
    /// <summary>
    /// string 型
    /// </summary>
    public class CbString : BaseCbValueClass<string>, ICbValueClass<string>
    {
        public override Type MyType => typeof(CbString);

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

        public override void Set(ICbValue n)
        {
            try
            {
                if (n is CbObject || n is ICbClass)
                {
                    Data = n.Data;
                }
                else
                {
                    // 値を文字列にしてコピーする

                    ValueString = n.ValueString;
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
