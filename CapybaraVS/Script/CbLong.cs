using System;

namespace CapybaraVS.Script
{
    /// <summary>
    /// long 型
    /// </summary>
    public class CbLong : BaseCbValueClass<long>, ICbValueClass<long>
    {
        public override Type MyType => typeof(CbLong);

        public CbLong(long n = 0, string name = "")
        {
            Value = n;
            Name = name;
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
                return Value.ToString();
            }
        }

        /// <summary>
        /// 値の文字列表現
        /// </summary>
        public override string ValueString
        {
            get => Value.ToString();
            set
            {
                if (value != null)
                    Value = long.Parse(value);
            }
        }

        public static CbLong Create(string name)
        {
            var ret = new CbLong(0, name);
            return ret;
        }

        public static CbLong Create(long n = 0, string name = "")
        {
            var ret = new CbLong(n, name);
            return ret;
        }

        public static Func<ICbValue> TF = () => CbLong.Create();
        public static Func<string, ICbValue> NTF = (name) => CbLong.Create(name);
    }
}
