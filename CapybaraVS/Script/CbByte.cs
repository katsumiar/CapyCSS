using System;

namespace CapybaraVS.Script
{
    /// <summary>
    /// byte 型
    /// </summary>
    public class CbByte : BaseCbValueClass<byte>, ICbValueClass<byte>
    {
        public override Type MyType => typeof(CbByte);

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

        public CbByte(byte n = 0, string name = "")
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
                    Value = byte.Parse(value);
            }
        }

        public static CbByte Create(string name)
        {
            var ret = new CbByte(0, name);
            return ret;
        }

        public static CbByte Create(byte n = 0, string name = "")
        {
            var ret = new CbByte(n, name);
            return ret;
        }

        public static Func<ICbValue> TF = () => CbByte.Create();
        public static Func<string, ICbValue> NTF = (name) => CbByte.Create(name);
    }
}
