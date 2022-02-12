using System;

namespace CapyCSS.Script
{
    /// <summary>
    /// short 型
    /// </summary>
    public class CbShort
        : BaseCbValueClass<short>
        , ICbValueClass<short>
    {
        public override Type MyType => typeof(CbShort);

        public CbShort(short n = 0, string name = "")
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
                    Value = short.Parse(value);
            }
        }

        public static CbShort Create(string name) => new CbShort(0, name);

        public static CbShort Create(short n = 0, string name = "") => new CbShort(n, name);

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
    /// short? 型
    /// </summary>
    public class CbNullableShort
        : CbShort
    {
        public override Type MyType => typeof(CbNullableShort);

        public CbNullableShort(short n = 0, string name = "")
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

        public override object Data
        {
            get
            {
                if (isNull)
                {
                    return null;
                }
                return Value as object;
            }
            set
            {
                if (value is null)
                {
                    isNull = true;
                }
                else
                {
                    // ただのキャストでは sbyte から short への変換などで例外が出るので ChangeType を使って変換する

                    Value = (short)Convert.ChangeType(value, typeof(short));
                }
            }
        }

        public static new CbNullableShort Create(string name) => new CbNullableShort(0, name);

        public static new CbNullableShort Create(short n = 0, string name = "") => new CbNullableShort(n, name);

        public static new Func<ICbValue> TF = () => Create();
        public static new Func<string, ICbValue> NTF = (name) => Create(name);
    }
}
