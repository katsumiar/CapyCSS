using CapyCSS.Script;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapyCSS.Script
{
	/// <summary>
	/// CbMoviePath型（独自）
	/// </summary>
	public class CbMoviePath : CbString
	{
		public override Type MyType => typeof(CbText);

		public override string TypeName => "MoviePath";

		public static bool IsExtension(string path)
		{
			var movieExtensions = new List<string>() { ".mp4", ".wma", ".m4v", ".mov", ".avi", ".3gp" };
			return movieExtensions.Contains(System.IO.Path.GetExtension(path.ToLower()));
		}

		public CbMoviePath(string n = "", string name = "")
			: base(n, name)
		{
		}

		public static new CbMoviePath Create(string name = "") => new CbMoviePath("", name);

		public static new CbMoviePath Create(string n, string name) => new CbMoviePath(n, name);

		public static new Func<ICbValue> TF = () => Create();
		public static new Func<string, ICbValue> NTF = (name) => Create(name);
	}
}
