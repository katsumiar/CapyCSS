using System;

namespace CapybaraVS.Script
{
    /// <summary>
    /// bool 型
    /// </summary>
    public class CbBool : BaseCbValueClass<bool>, ICbValueClass<bool>, ICbValueEnum
    {
        public override Type MyType => typeof(CbBool);

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

        public CbBool(bool n = false, string name = "")
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
                    Value = bool.Parse(value);
            }
        }

        public string[] ElementList
        {
            get
            {
                string[] list = { "False", "True" };
                return list;
            }
        }

        public static CbBool Create(string name)
        {
            var ret = new CbBool(false, name);
            return ret;
        }

        public static CbBool Create(bool n = false, string name = "")
        {
            var ret = new CbBool(n, name);
            return ret;
        }

        public static Func<ICbValue> TF = () => CbBool.Create();
        public static Func<string, ICbValue> NTF = (name) => CbBool.Create(name);
    }
}
