using System;

namespace CapybaraVS.Script
{
    /// <summary>
    /// int 型
    /// </summary>
    public class CbInt 
        : BaseCbValueClass<int>
        , ICbValueClass<int>
    {
        public override Type MyType => typeof(CbInt);

        public CbInt(int n = 0, string name = "")
        {
            Value = n;
            Name = name;
        }

        public override int Value
        {
            get => _value;
            set
            {
                _value = value;
            }
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
                return Value.ToString();
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
                    Value = int.Parse(value);
            }
        }

        public static CbInt Create(string name)
        {
            var ret = new CbInt(0, name);
            return ret;
        }

        public static CbInt Create(int n = 0, string name = "")
        {
            var ret = new CbInt(n, name);
            return ret;
        }

        public static Func<ICbValue> TF = () => CbInt.Create();
        public static Func<string, ICbValue> NTF = (name) => CbInt.Create(name);

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
