using System;
using System.Collections.Generic;

namespace CapyCSS.Script
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

        private Dictionary<int, string> GetCodeDictionary()
        {
            return new Dictionary<int, string>()
            {
                { 0, "[NUL]" },
                { 1, "[SOH]" },
                { 2, "[STX]" },
                { 3, "[ETX]" },
                { 4, "[EOT]" },
                { 5, "[ENQ]" },
                { 6, "[ACK]" },
                { 7, "[BEL]" },
                { 8, "[BS]" },
                { 9, "[HT]" },
                { 10, "[LF]" },
                { 11, "[VT]" },
                { 12, "[FF]" },
                { 13, "[CR]" },
                { 14, "[SO]" },
                { 15, "[SI]" },
                { 16, "[DLE]" },
                { 17, "[DC1]" },
                { 18, "[DC2]" },
                { 19, "[DC3]" },
                { 20, "[DC4]" },
                { 21, "[NAK]" },
                { 22, "[SYN]" },
                { 23, "[ETB]" },
                { 24, "[CAN]" },
                { 25, "[EM]" },
                { 26, "[SUB]" },
                { 27, "[ESC]" },
                { 28, "[FS]" },
                { 29, "[GS]" },
                { 30, "[RS]" },
                { 31, "[US]" },
                { 127, "[DEL]" },
            };
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

                var dic = GetCodeDictionary();
                if (dic.ContainsKey((int)Value))
                {
                    return dic[(int)Value];
                }
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
                    value = value.Trim();
                    if (value.StartsWith("\\x") && value.Length == 4)
                    {
                        // ASCII 16進数表現

                        Value = (char)(int.Parse(value.Replace("\\x", ""), System.Globalization.NumberStyles.HexNumber));
                        return;
                    }
                    var dic = new Dictionary<string, int>()
                    {
                        { "\\0", 0x0000 },
                        { "\\a", 0x0007 },
                        { "\\b", 0x0008 },
                        { "\\f", 0x000c },
                        { "\\n", 0x000a },
                        { "\\r", 0x000d },
                        { "\\t", 0x0009 },
                        { "\\v", 0x000b },
                    };
                    foreach (var pair in dic)
                    {
                        if (pair.Key == value)
                        {
                            // エスケープシーケンス

                            Value = (char)pair.Value;
                            return;
                        }
                    }
                    foreach (var pair in GetCodeDictionary())
                    {
                        if (pair.Value == value)
                        {
                            Value = (char)pair.Key;
                            return;
                        }
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
