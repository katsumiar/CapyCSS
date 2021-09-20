using System;

namespace CapybaraVS.Script
{
    /// <summary>
    /// char 型
    /// </summary>
    public class CbChar 
        : BaseCbValueClass<char>
        , ICbValueClass<char>
    {
        public override Type MyType => typeof(CbChar);

        public CbChar(char n = '*', string name = "")
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

                if (Value == '\n') return "\\n";
                if (Value == '\0') return "\\0";
                if (Value == '\a') return "\\a";
                if (Value == '\b') return "\\b";
                if (Value == '\f') return "\\f";
                if (Value == '\n') return "\\n";
                if (Value == '\r') return "\\r";
                if (Value == '\t') return "\\t";
                if (Value == '\v') return "\\v";
                return Value.ToString();
            }
        }

        /// <summary>
        /// 値の文字列表現
        /// </summary>
        public override string ValueString
        {
            get => ValueUIString;
            set
            {
                if (value != null)
                {
                    if (value.Contains('\\'))
                    {
                        value = value.Replace("\\", "\\");
                        value = value.Replace("\\0", "\0");
                        value = value.Replace("\\a", "\a");
                        value = value.Replace("\\b", "\b");
                        value = value.Replace("\\f", "\f");
                        value = value.Replace("\\n", "\n");
                        value = value.Replace("\\r", "\r");
                        value = value.Replace("\\t", "\t");
                        value = value.Replace("\\v", "\v");
                    }
                    Value = char.Parse(value);
                }
            }
        }

        public static CbChar Create(string name) => new CbChar((char)0, name);

        public static CbChar Create(char n = (char)0, string name = "") => new CbChar(n, name);

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
    /// char? 型
    /// </summary>
    public class CbNullableChar
        : CbChar
    {
        public override Type MyType => typeof(CbNullableChar);

        public CbNullableChar(char n = (char)0, string name = "")
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
            get => ValueUIString;
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

        public static new CbNullableChar Create(string name) => new CbNullableChar((char)0, name);

        public static new CbNullableChar Create(char n = (char)0, string name = "") => new CbNullableChar(n, name);

        public static new Func<ICbValue> TF = () => Create();
        public static new Func<string, ICbValue> NTF = (name) => Create(name);
    }
}
