using System;

namespace CapybaraVS.Script
{
    /// <summary>
    /// double 型
    /// </summary>
    public class CbDouble 
        : BaseCbValueClass<double>
        , ICbValueClass<double>
    {
        public override Type MyType => typeof(CbDouble);

        public CbDouble(double n = 0, string name = "")
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
                    Value = double.Parse(value);
            }
        }

        public static CbDouble Create(string name) => new CbDouble(0, name);

        public static CbDouble Create(double n = 0, string name = "") => new CbDouble(n, name);

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
    /// double? 型
    /// </summary>
    public class CbNullableDouble
        : CbDouble
    {
        public override Type MyType => typeof(CbNullableDouble);

        public CbNullableDouble(double n = 0, string name = "")
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

        public static new CbNullableDouble Create(string name) => new CbNullableDouble(0, name);

        public static new CbNullableDouble Create(double n = 0, string name = "") => new CbNullableDouble(n, name);

        public static new Func<ICbValue> TF = () => Create();
        public static new Func<string, ICbValue> NTF = (name) => Create(name);
    }
}
