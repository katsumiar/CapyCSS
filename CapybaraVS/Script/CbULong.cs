﻿using System;

namespace CapyCSS.Script
{
    /// <summary>
    /// ulong 型
    /// </summary>
    public class CbULong 
        : BaseCbValueClass<ulong>
        , ICbValueClass<ulong>
    {
        public override Type MyType => typeof(CbULong);

        public CbULong(ulong n = 0, string name = "")
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
                    Value = ulong.Parse(value);
            }
        }

        public static CbULong Create(string name) => new CbULong(0, name);

        public static CbULong Create(ulong n = 0, string name = "") => new CbULong(n, name);

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
    /// ulong? 型
    /// </summary>
    public class CbNullableULong
        : CbULong
    {
        public override Type MyType => typeof(CbNullableULong);

        public CbNullableULong(ulong n = 0, string name = "")
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
                    // ただのキャストでは sbyte から ulong への変換などで例外が出るので ChangeType を使って変換する

                    Value = (ulong)Convert.ChangeType(value, typeof(ulong));
                }
            }
        }

        public static new CbNullableULong Create(string name) => new CbNullableULong(0, name);

        public static new CbNullableULong Create(ulong n = 0, string name = "") => new CbNullableULong(n, name);

        public static new Func<ICbValue> TF = () => Create();
        public static new Func<string, ICbValue> NTF = (name) => Create(name);
    }
}
