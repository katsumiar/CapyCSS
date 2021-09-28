using System;

namespace CapybaraVS.Script
{
    /// <summary>
    /// ushort 型
    /// </summary>
    public class CbUShort 
        : BaseCbValueClass<ushort>
        , ICbValueClass<ushort>
    {
        public override Type MyType => typeof(CbUShort);

        public CbUShort(ushort n = 0, string name = "")
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
                    Value = ushort.Parse(value);
            }
        }

        public static CbUShort Create(string name) => new CbUShort(0, name);

        public static CbUShort Create(ushort n = 0, string name = "") => new CbUShort(n, name);

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
    /// ushort? 型
    /// </summary>
    public class CbNullableUShort
        : CbUShort
    {
        public override Type MyType => typeof(CbNullableUShort);

        public CbNullableUShort(ushort n = 0, string name = "")
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

        public static new CbNullableUShort Create(string name) => new CbNullableUShort(0, name);

        public static new CbNullableUShort Create(ushort n = 0, string name = "") => new CbNullableUShort(n, name);

        public static new Func<ICbValue> TF = () => Create();
        public static new Func<string, ICbValue> NTF = (name) => Create(name);
    }
}
