﻿using System;

namespace CapyCSS.Script
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

        public static CbInt Create(string name) => new CbInt(0, name);

        public static CbInt Create(int n = 0, string name = "") => new CbInt(n, name);

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
    /// int? 型
    /// </summary>
    public class CbNullableInt
        : CbInt
    {
        public override Type MyType => typeof(CbNullableInt);

        public CbNullableInt(int n = 0, string name = "")
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
                if (value != null)
                    Value = int.Parse(value);
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
                    // ただのキャストでは sbyte から int への変換などで例外が出るので ChangeType を使って変換する

                    Value = (int)Convert.ChangeType(value, typeof(int));
                }
            }
        }

        public static new CbNullableInt Create(string name) => new CbNullableInt(0, name);

        public static new CbNullableInt Create(int n = 0, string name = "") => new CbNullableInt(n, name);

        public static new Func<ICbValue> TF = () => Create();
        public static new Func<string, ICbValue> NTF = (name) => Create(name);
    }
}
