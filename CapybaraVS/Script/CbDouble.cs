using System;

namespace CapybaraVS.Script
{
    /// <summary>
    /// double 型
    /// </summary>
    public class CbDouble : BaseCbValueClass<double>, ICbValueClass<double>
    {
        public override Type MyType => typeof(CbDouble);

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
                    return ERROR_STR;
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
