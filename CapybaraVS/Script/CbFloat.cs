using System;

namespace CapybaraVS.Script
{
    /// <summary>
    /// float 型
    /// </summary>
    public class CbFloat : BaseCbValueClass<float>, ICbValueClass<float>
    {
        public override Type MyType => typeof(CbFloat);

        public CbFloat(float n = 0, string name = "")
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
                return Value.ToString();
            }
            set
            {
                if (value != null)
                    Value = float.Parse(value); 
            }
        }

        public static CbFloat Create(string name)
        {
            var ret = new CbFloat(0, name);
            return ret;
        }

        public static CbFloat Create(float n = 0, string name = "")
        {
            var ret = new CbFloat(n, name);
            return ret;
        }

        public static Func<ICbValue> TF = () => CbFloat.Create();
        public static Func<string, ICbValue> NTF = (name) => CbFloat.Create(name);
    }
}
