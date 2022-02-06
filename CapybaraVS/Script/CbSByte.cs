using System;

namespace CapyCSS.Script
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
                if (value != null)
                    Value = sbyte.Parse(value);
            }
        }

        public static CbSByte Create(string name) => new CbSByte(0, name);

        public static CbSByte Create(sbyte n = 0, string name = "") => new CbSByte(n, name);

        public static Func<ICbValue> TF = () => Create();
        public static Func<string, ICbValue> NTF = (name) => Create(name);

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
    /// sbyte? 型
    /// </summary>
    public class CbNullableSByte
        : CbSByte
    {
        public override Type MyType => typeof(CbNullableSByte);

        public CbNullableSByte(sbyte n = 0, string name = "")
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
                base.ValueString = value;
            }
        }

        public static new CbNullableSByte Create(string name) => new CbNullableSByte(0, name);

        public static new CbNullableSByte Create(sbyte n = 0, string name = "") => new CbNullableSByte(n, name);

        public static new Func<ICbValue> TF = () => Create();
        public static new Func<string, ICbValue> NTF = (name) => Create(name);
    }
}
