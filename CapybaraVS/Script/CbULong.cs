using System;

namespace CapybaraVS.Script
{
    /// <summary>
    /// ulong 型
    /// </summary>
    public class CbULong : BaseCbValueClass<ulong>, ICbValueClass<ulong>
    {
        public override Type MyType => typeof(CbULong);

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

        public CbULong(ulong n = 0, string name = "")
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
                    Value = ulong.Parse(value); 
            }
        }

        public static CbULong Create(string name)
        {
            var ret = new CbULong(0, name);
            return ret;
        }

        public static CbULong Create(ulong n = 0, string name = "")
        {
            var ret = new CbULong(n, name);
            return ret;
        }

        public static Func<ICbValue> TF = () => CbULong.Create();
        public static Func<string, ICbValue> NTF = (name) => CbULong.Create(name);
    }
}
