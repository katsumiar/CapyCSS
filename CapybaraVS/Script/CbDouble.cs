using System;

namespace CapybaraVS.Script
{
    /// <summary>
    /// double 型
    /// </summary>
    public class CbDouble : BaseCbValueClass<double>, ICbValueClass<double>
    {
        public override Type MyType => typeof(CbDouble);

        public CbDouble(double n = 0, string name = "")
        {
            Value = n;
            Name = name;
        }

        public override string ValueString
        {
            get
            {
                if (IsError)
                    return CbSTUtils.ERROR_STR;
                return Value.ToString();
            }
            set
            {
                if (value != null)
                    Value = double.Parse(value);
            }
        }

        public static CbDouble Create(string name)
        {
            var ret = new CbDouble(0, name);
            return ret;
        }

        public static CbDouble Create(double n = 0, string name = "")
        {
            var ret = new CbDouble(n, name);
            return ret;
        }

        public static Func<ICbValue> TF = () => CbDouble.Create();
        public static Func<string, ICbValue> NTF = (name) => CbDouble.Create(name);
    }
}
