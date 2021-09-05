using System;

namespace CapybaraVS.Script
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
}
