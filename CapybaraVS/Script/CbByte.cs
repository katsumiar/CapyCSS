using System;

namespace CapyCSS.Script
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



    /// <summary>
    /// byte? 型
    /// </summary>
    public class CbNullableByte : CbByte
    {
        public override Type MyType => typeof(CbNullableByte);

        public CbNullableByte(byte n = 0, string name = "")
            : base(n, name) {}

        /// <summary>
        /// null許容型か？
        /// </summary>
        public override bool IsNullable => true;

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

        public static new CbNullableByte Create(string name)
        {
            return new CbNullableByte(0, name);
        }

        public static new CbNullableByte Create(byte n = 0, string name = "")
        {
            return new CbNullableByte(n, name);
        }

        public static new Func<ICbValue> TF = () => CbNullableByte.Create();
        public static new Func<string, ICbValue> NTF = (name) => CbNullableByte.Create(name);
    }
}
