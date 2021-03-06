﻿using System;

namespace CapybaraVS.Script
{
    /// <summary>
    /// uint 型
    /// </summary>
    public class CbUInt : BaseCbValueClass<uint>, ICbValueClass<uint>
    {
        public override Type MyType => typeof(CbUInt);

        public CbUInt(uint n = 0, string name = "")
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
                return Value.ToString();
            }
            set
            {
                if (value != null)
                    Value = uint.Parse(value);
            }
        }

        public static CbUInt Create(string name)
        {
            var ret = new CbUInt(0, name);
            return ret;
        }

        public static CbUInt Create(uint n = 0, string name = "")
        {
            var ret = new CbUInt(n, name);
            return ret;
        }

        public static Func<ICbValue> TF = () => CbUInt.Create();
        public static Func<string, ICbValue> NTF = (name) => CbUInt.Create(name);
    }
}
