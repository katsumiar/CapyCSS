using System;

namespace CapyCSS.Script
{
    /// <summary>
    /// bool 型
    /// </summary>
    public class CbBool 
        : BaseCbValueClass<bool>
        , ICbValueClass<bool>
        , ICbValueEnum
    {
        public override Type MyType => typeof(CbBool);

        public CbBool(bool n = false, string name = "")
        {
            Value = n;
            Name = name;
        }

        /// <summary>
        /// 値のUI上の文字列表現
        /// </summary>
        public override string ValueUIString
        {
            get
            {
                if (IsError)
                    return CbSTUtils.ERROR_STR;
                if (IsNull)
                    return CbSTUtils.UI_NULL_STR;
                return Value.ToString().ToLower();  // ElementList は小文字
            }
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
                    Value = bool.Parse(value);
            }
        }

        public string[] ElementList
        {
            get
            {
                string[] list = { "false", "true" };
                return list;
            }
        }

        public static CbBool Create(string name)
        {
            var ret = new CbBool(false, name);
            return ret;
        }

        public static CbBool Create(bool n = false, string name = "")
        {
            var ret = new CbBool(n, name);
            return ret;
        }

        public static Func<ICbValue> TF = () => CbBool.Create();
        public static Func<string, ICbValue> NTF = (name) => CbBool.Create(name);
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
    /// bool? 型
    /// </summary>
    public class CbNullableBool
        : CbBool
    {
        public override Type MyType => typeof(CbNullableBool);

        public CbNullableBool(bool n = false, string name = "")
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

        public static new CbNullableBool Create(string name)
        {
            return new CbNullableBool(false, name);
        }

        public static new CbNullableBool Create(bool n = false, string name = "")
        {
            return new CbNullableBool(n, name);
        }

        public static new Func<ICbValue> TF = () => Create();
        public static new Func<string, ICbValue> NTF = (name) => Create(name);
    }
}
