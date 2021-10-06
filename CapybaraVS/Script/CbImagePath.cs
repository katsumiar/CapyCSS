using CapybaraVS.Script;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapyCSS.Script
{
    /// <summary>
    /// ImagePath型（独自）
    /// </summary>
    public class CbImagePath : CbString
    {
        public override Type MyType => typeof(CbText);

        public override string TypeName => "ImagePath";

        public CbImagePath(string n = "", string name = "")
            : base(n, name)
        {
        }

        public static new CbImagePath Create(string name = "") => new CbImagePath("", name);

        public static new CbImagePath Create(string n, string name) => new CbImagePath(n, name);

        public static new Func<ICbValue> TF = () => Create();
        public static new Func<string, ICbValue> NTF = (name) => Create(name);
    }
}
