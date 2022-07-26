using CapyCSS.Script;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapyCSS.Script
{
    /// <summary>
    /// Password型（独自）
    /// </summary>
    public class CbPassword : CbString
    {
        public override Type MyType => typeof(CbPassword);

        public override string TypeName => CbSTUtils.GetTypeName(this);

        public override bool IsSecretString => true;    // 非公開文字列

        /// <summary>
        /// 値のUI上の文字列表現
        /// ※パスワードの編集は、PasswordBoxで行うのでそのままのソースを返す（CbStringでは逆にマスク文字列を返す）
        /// </summary>
        public override string ValueUIString
        {
            get
            {
                if (IsError)
                    return "";
                if (IsNull)
                {
                    if (IsNull)
                    {
                        return "";
                    }
                }
                return Value;
            }
        }

        private static readonly int baseNumber = ' ';
        private static readonly int maxNumber = '~' - ' ';

        /// <summary>
        /// 文字列を非公開用に簡易に暗号文にします。
        /// ※セキュアではありません。読み難くする程度の内容です。
        /// </summary>
        /// <param name="str">平文</param>
        /// <returns>暗号文</returns>
        public static string Encrypt(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return str;
            }
            char[] buffer = new char[str.Length];
            int index = 0;
            foreach (char c in str)
            {
                int temp = (int)(c - baseNumber);
                int p = ((index + 3) ^ 10) << 1;
                temp = temp + p;
                temp %= maxNumber;
                buffer[index++] = (char)(temp + baseNumber);
            }
            return new string(buffer, 0, index);
        }

        /// <summary>
        /// 非公開用に簡易に暗号化された文字列を平文に戻します。
        /// </summary>
        /// <param name="str">暗号文</param>
        /// <returns>平文</returns>
        public static string Decrypt(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return str;
            }
            char[] buffer = new char[str.Length];
            int index = 0;
            foreach (char c in str)
            {
                int temp = (int)(c - baseNumber);
                int p = ((index + 3) ^ 10) << 1;
                temp = temp - p;
                if (temp < 0)
                {
                    temp = maxNumber + temp % maxNumber;
                }
                buffer[index++] = (char)(temp + baseNumber);
            }
            return new string(buffer, 0, index);
        }

        public CbPassword(string n = "", string name = "")
            : base(n, name)
        {
        }

        public static new CbPassword Create(string name = "") => new CbPassword("", name);

        public static new CbPassword Create(string n, string name) => new CbPassword(n, name);

        public static new Func<ICbValue> TF = () => Create();
        public static new Func<string, ICbValue> NTF = (name) => Create(name);
    }
}
