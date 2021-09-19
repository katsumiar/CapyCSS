using System;

namespace CapybaraVS.Script
{
    /// <summary>
    /// decimal 型
    /// </summary>
    public class CbDecimal 
        : BaseCbValueClass<decimal>
        , ICbValueClass<decimal>
    {
        public override Type MyType => typeof(CbDecimal);

        public CbDecimal(decimal n = 0, string name = "")
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
                    Value = decimal.Parse(value);
            }
        }

        public static CbDecimal Create(string name)
        {
            var ret = new CbDecimal(0, name);
            return ret;
        }

        public static CbDecimal Create(decimal n = 0, string name = "")
        {
            var ret = new CbDecimal(n, name);
            return ret;
        }

        public static Func<ICbValue> TF = () => CbDecimal.Create();
        public static Func<string, ICbValue> NTF = (name) => CbDecimal.Create(name);
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
    /// decimal? 型
    /// </summary>
    public class CbNullableDecimal
        : CbDecimal
    {
        public override Type MyType => typeof(CbNullableDecimal);

        public CbNullableDecimal(decimal n = 0, string name = "")
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

        public static new CbNullableDecimal Create(string name)
        {
            return new CbNullableDecimal(0, name);
        }

        public static new CbNullableDecimal Create(decimal n = 0, string name = "")
        {
            return new CbNullableDecimal(n, name);
        }

        public static new Func<ICbValue> TF = () => Create();
        public static new Func<string, ICbValue> NTF = (name) => Create(name);
    }
}
