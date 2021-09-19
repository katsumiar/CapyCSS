using System;

namespace CapybaraVS.Script
{
    /// <summary>
    /// long 型
    /// </summary>
    public class CbLong 
        : BaseCbValueClass<long>
        , ICbValueClass<long>
    {
        public override Type MyType => typeof(CbLong);

        public CbLong(long n = 0, string name = "")
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
                    Value = long.Parse(value);
            }
        }

        public static CbLong Create(string name)
        {
            var ret = new CbLong(0, name);
            return ret;
        }

        public static CbLong Create(long n = 0, string name = "")
        {
            var ret = new CbLong(n, name);
            return ret;
        }

        public static Func<ICbValue> TF = () => CbLong.Create();
        public static Func<string, ICbValue> NTF = (name) => CbLong.Create(name);

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
    /// long? 型
    /// </summary>
    public class CbNullableLong
    : CbLong
    {
        public override Type MyType => typeof(CbNullableLong);

        public CbNullableLong(long n = 0, string name = "")
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

        public static new CbNullableLong Create(string name)
        {
            return new CbNullableLong(0, name);
        }

        public static new CbNullableLong Create(long n = 0, string name = "")
        {
            return new CbNullableLong(n, name);
        }

        public static new Func<ICbValue> TF = () => CbNullableLong.Create();
        public static new Func<string, ICbValue> NTF = (name) => CbNullableLong.Create(name);
    }
}
