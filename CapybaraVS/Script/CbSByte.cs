using System;

namespace CapybaraVS.Script
{
    /// <summary>
    /// sbyte 型
    /// </summary>
    public class CbSByte 
        : BaseCbValueClass<sbyte>
        , ICbValueClass<sbyte>
    {
        public override Type MyType => typeof(CbSByte);

        public CbSByte(sbyte n = 0, string name = "")
        {
            Value = n;
            Name = name;
        }

        /// <summary>
        /// 値の文字列表現
        /// </summary>
        public override string ValueString
        {
            get => Value.ToString();
            set
            {
                if (IsNullable && value == CbSTUtils.UI_NULL_STR)
                {
                    isNull = true;
                    return;
                }
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

        private bool disposedValue;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    ClearWork();
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
