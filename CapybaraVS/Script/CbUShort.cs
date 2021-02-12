using System;

namespace CapybaraVS.Script
{
    /// <summary>
    /// ushort 型
    /// </summary>
    public class CbUShort : BaseCbValueClass<ushort>, ICbValueClass<ushort>
    {
        public override Type MyType => typeof(CbUShort);

        public CbUShort(ushort n = 0, string name = "")
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
                    Value = ushort.Parse(value);
            }
        }

        public static CbUShort Create(string name)
        {
            var ret = new CbUShort(0, name);
            return ret;
        }

        public static CbUShort Create(ushort n = 0, string name = "")
        {
            var ret = new CbUShort(n, name);
            return ret;
        }

        public static Func<ICbValue> TF = () => CbUShort.Create();
        public static Func<string, ICbValue> NTF = (name) => CbUShort.Create(name);
    }
}
