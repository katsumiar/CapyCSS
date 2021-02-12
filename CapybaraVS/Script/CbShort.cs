using System;

namespace CapybaraVS.Script
{
    /// <summary>
    /// short 型
    /// </summary>
    public class CbShort : BaseCbValueClass<short>, ICbValueClass<short>
    {
        public override Type MyType => typeof(CbShort);

        public CbShort(short n = 0, string name = "")
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
                    Value = short.Parse(value);
            }
        }

        public static CbShort Create(string name)
        {
            var ret = new CbShort(0, name);
            return ret;
        }

        public static CbShort Create(short n = 0, string name = "")
        {
            var ret = new CbShort(n, name);
            return ret;
        }

        public static Func<ICbValue> TF = () => CbShort.Create();
        public static Func<string, ICbValue> NTF = (name) => CbShort.Create(name);
    }
}
