using System;

namespace CapybaraVS.Script
{
    /// <summary>
    /// sbyte 型
    /// </summary>
    public class CbSByte : BaseCbValueClass<sbyte>, ICbValueClass<sbyte>
    {
        public override Type MyType => typeof(CbSByte);

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

        public CbSByte(sbyte n = 0, string name = "")
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
                    Value = sbyte.Parse(value);
            }
        }

        public static CbSByte Create(string name)
        {
            var ret = new CbSByte(0, name);
            return ret;
        }

        public static CbSByte Create(sbyte n = 0, string name = "")
        {
            var ret = new CbSByte(n, name);
            return ret;
        }

        public static Func<ICbValue> TF = () => CbSByte.Create();
        public static Func<string, ICbValue> NTF = (name) => CbSByte.Create(name);
    }
}
