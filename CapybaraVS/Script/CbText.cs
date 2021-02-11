using CapybaraVS.Script;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapyCSS.Script
{
    /// <summary>
    /// text型（独自）
    /// </summary>
    public class CbText : CbString
    {
        public override Type MyType => typeof(CbText);

        public override string TypeName => "text";

        public CbText(string n = "", string name = "")
            : base(n, name)
        {
        }

        public static new CbText Create(string name = "")
        {
            var ret = new CbText("", name);
            return ret;
        }

        public static new CbText Create(string n, string name)
        {
            var ret = new CbText(n, name);
            return ret;
        }

        public static new Func<ICbValue> TF = () => CbText.Create();
        public static new Func<string, ICbValue> NTF = (name) => CbText.Create(name);
    }
}
