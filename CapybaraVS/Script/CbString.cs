using System;
using System.Diagnostics;
using CbVS.Script;

namespace CapybaraVS.Script
{
    /// <summary>
    /// string 型
    /// </summary>
    public class CbString
        : BaseCbValueClass<string>
        , ICbValueClass<string>
    {
        public override Type MyType => typeof(CbString);

        public CbString(string n = "", string name = "")
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
                {
                    if (IsNull)
                    {
                        return $"[{TypeName}]{CbSTUtils.UI_NULL_STR}";
                    }
                }
                return Value;
            }
        }

        /// <summary>
        /// 値の文字列表現
        /// </summary>
        public override string ValueString
        {
            get
            {
                if (IsNull)
                {
                    return $"[{TypeName}]{CbSTUtils.UI_NULL_STR}";
                }
                return Value;
            }
            set
            {
                Value = value;
            }
        }

        public override void Set(ICbValue n)
        {
            try
            {
                if (n.IsError)
                    throw new Exception(n.ErrorMessage);

                // 値を文字列にしてコピーする

                ValueString = n.ValueString;
                IsLiteral = n.IsLiteral;
                if (IsError)
                {
                    // エラーからの復帰

                    IsError = false;
                    ErrorMessage = "";
                }
            }
            catch (Exception ex)
            {
                IsError = true;
                ErrorMessage = ex.Message;
                throw;
            }
        }

        public static CbString Create(string name = "") => new CbString("", name);

        public static CbString Create(string n, string name) => new CbString(n, name);

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
}
