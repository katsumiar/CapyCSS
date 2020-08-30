using System;

namespace CapybaraVS.Script
{
    /// <summary>
    /// long 型
    /// </summary>
    public class CbLong : BaseCbValueClass<long>, ICbValueClass<long>
    {
        public override Type MyType => typeof(CbLong);

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

        public CbLong(long n = 0, string name = "")
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
