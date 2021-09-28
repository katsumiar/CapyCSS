using System;

namespace CapybaraVS.Script
{
    /// <summary>
    /// float 型
    /// </summary>
    public class CbFloat 
        : BaseCbValueClass<float>
        , ICbValueClass<float>
    {
        public override Type MyType => typeof(CbFloat);

        public CbFloat(float n = 0, string name = "")
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
                    Value = float.Parse(value);
            }
        }

        public static CbFloat Create(string name) => new CbFloat(0, name);

        public static CbFloat Create(float n = 0, string name = "") => new CbFloat(n, name);

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
    /// float? 型
    /// </summary>
    public class CbNullableFloat
        : CbFloat
    {
        public override Type MyType => typeof(CbNullableFloat);

        public CbNullableFloat(float n = 0, string name = "")
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

        public static new CbNullableFloat Create(string name) => new CbNullableFloat(0, name);

        public static new CbNullableFloat Create(float n = 0, string name = "") => new CbNullableFloat(n, name);

        public static new Func<ICbValue> TF = () => Create();
        public static new Func<string, ICbValue> NTF = (name) => Create(name);
    }
}
