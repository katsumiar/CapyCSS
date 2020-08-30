using System;

namespace CapybaraVS.Script
{
    /// <summary>
    /// uint 型
    /// </summary>
    public class CbUInt : BaseCbValueClass<uint>, ICbValueClass<uint>
    {
        public override Type MyType => typeof(CbUInt);

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

        public CbUInt(uint n = 0, string name = "")
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
                    Value = uint.Parse(value);
            }
        }

        public static CbUInt Create(string name)
        {
            var ret = new CbUInt(0, name);
            return ret;
        }

        public static CbUInt Create(uint n = 0, string name = "")
        {
            var ret = new CbUInt(n, name);
            return ret;
        }

        public static Func<ICbValue> TF = () => CbUInt.Create();
        public static Func<string, ICbValue> NTF = (name) => CbUInt.Create(name);
    }
}
