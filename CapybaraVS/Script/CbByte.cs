using System;

namespace CapybaraVS.Script
{
    /// <summary>
    /// byte 型
    /// </summary>
    public class CbByte 
        : BaseCbValueClass<byte>
        , ICbValueClass<byte>
    {
        public override Type MyType => typeof(CbByte);

        public CbByte(byte n = 0, string name = "")
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
