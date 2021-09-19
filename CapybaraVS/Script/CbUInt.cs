using System;

namespace CapybaraVS.Script
{
    /// <summary>
    /// uint 型
    /// </summary>
    public class CbUInt 
        : BaseCbValueClass<uint>
        , ICbValueClass<uint>
    {
        public override Type MyType => typeof(CbUInt);

        public CbUInt(uint n = 0, string name = "")
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
    /// uint? 型
    /// </summary>
    public class CbNullableUInt
        : CbUInt
    {
        public override Type MyType => typeof(CbNullableUInt);

        public CbNullableUInt(uint n = 0, string name = "")
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

        public static new CbNullableUInt Create(string name)
        {
            return new CbNullableUInt(0, name);
        }

        public static new CbNullableUInt Create(uint n = 0, string name = "")
        {
            return new CbNullableUInt(n, name);
        }

        public static new Func<ICbValue> TF = () => Create();
        public static new Func<string, ICbValue> NTF = (name) => Create(name);
    }

}
