﻿using System;

namespace CapybaraVS.Script
{
    /// <summary>
    /// char 型
    /// </summary>
    public class CbChar : BaseCbValueClass<char>, ICbValueClass<char>
    {
        public override Type MyType => typeof(CbChar);

        public CbChar(char n = '*', string name = "")
        {
            Value = n;
            Name = name;
        }

        public override string ValueString 
        {
            get
            {
                if (IsError)
                    return ERROR_STR;
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

        public static CbChar Create(string name)
        {
            var ret = new CbChar('*', name);    // 初期値は検討中
            return ret;
        }

        public static CbChar Create(char n = '*', string name = "")    // 初期値は検討中
        {
            var ret = new CbChar(n, name);
            return ret;
        }

        public static Func<ICbValue> TF = () => CbChar.Create();
        public static Func<string, ICbValue> NTF = (name) => CbChar.Create(name);
    }
}
