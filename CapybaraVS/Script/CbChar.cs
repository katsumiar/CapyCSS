using System;

namespace CapybaraVS.Script
{
    /// <summary>
    /// char 型
    /// </summary>
    public class CbChar : BaseCbValueClass<char>, ICbValueClass<char>
    {
        public override Type MyType => typeof(CbChar);

        public CbChar(char n = '*', string name = "")
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
                    Value = char.Parse(value);
            }
        }

        public static CbChar Create(string name)
        {
            var ret = new CbChar('*', name);    // 初期値は検討中
            return ret;
        }

        public static CbChar Create(char n = '*', string name = "")    // 初期値は検討中
        {
            var ret = new CbChar(n, name);
            return ret;
        }

        public static Func<ICbValue> TF = () => CbChar.Create();
        public static Func<string, ICbValue> NTF = (name) => CbChar.Create(name);
    }
}
