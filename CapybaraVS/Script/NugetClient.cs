using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO.Compression;
using System.IO;
using System.Xml.Linq;

namespace CapyCSS.Script
{
    class NugetClient
    {
        public class PackageInfo
        {
            public string Path { get; set; }
            public string Version { get; set; }
            public PackageInfo(string path, string version)
            {
                Path = path;
                Version = version;
            }
        }

        /// <summary>
        /// パッケージをインストールします。
        /// ただし、既にインストール済みならインストールはしません。
        /// </summary>
        /// <param name="packageDir">パッケージ展開用ディレクトリ</param>
        /// <param name="packageName">バージョン情報付きパッケージ名（name(var)）</param>
        /// <returns>dllリスト</returns>
		public static List<PackageInfo> install(string packageDir, string packageName, out string pkgId)
		{
            if (packageName.Contains("("))
            {
                var split = packageName.Split("(");
                string Name = split[0];
                string version = split[1].Replace(")", "");
                return install(packageDir, Name, version, out pkgId);
            }
            pkgId = "";
			return new List<PackageInfo>();
		}

        /// <summary>
        /// パッケージをインストールします。
        /// ただし、既にインストール済みならインストールはしません。
        /// </summary>
        /// <param name="packageDir">パッケージ展開用ディレクトリ</param>
        /// <param name="packageName">パッケージ名</param>
        /// <param name="version">バージョン</param>
        /// <returns>dllリスト</returns>
		public static List<PackageInfo> install(string packageDir, string packageName, string version, out string pkgId)
        {
            string packageRoot = Path.Combine(packageDir, $"{packageName}.{version}");
            var packageList = new List<PackageInfo>();

            if (!Directory.Exists(packageRoot))
            {
                // nugetファイルをダウンロード

                string nupkg = $"{packageRoot}.nupkg";
                var webClient = new WebClient();
                webClient.DownloadFile($"https://www.nuget.org/api/v2/package/{packageName}/{version}", nupkg);
                ZipFile.ExtractToDirectory(nupkg, packageRoot); // zip展開
                File.Delete(nupkg);
            }

            // 依存パッケージを取得
            IEnumerable<string> packages = GetDependencies(packageName, packageRoot, out string _pkgId, out string net_version);
            if (packages is null)
            {
                pkgId = "";
                return null;
            }
            foreach (var package in packages)
            {
                packageList.AddRange(install(packageDir, package, out pkgId));
            }

            // dll をリストに登録
            string libPath = DllPath(packageName, packageRoot, net_version);
            packageList.Add(new PackageInfo(libPath, version));
            pkgId = _pkgId;
            return packageList;
        }

        /// <summary>
        /// 依存パッケージを取得します。
        /// </summary>
        /// <param name="packageName">パッケージ名</param>
        /// <param name="packageRoot">ダウンロードしたパッケージディレクトリ</param>
        /// <returns>パッケージ一覧</returns>
        private static IEnumerable<string> GetDependencies(string packageName, string packageRoot, out string pkgId, out string net_version)
        {
            List<string> ignoreList = new List<string>() {
                "System",
                "Microsoft.NETCore",
                "Microsoft.CSharp",
                "NETStandard.Library"
            };

            string nuspecFile = Path.Combine(packageRoot, $"{packageName}.nuspec");
            XDocument nuspec = XDocument.Load(nuspecFile);
            var xmlNs = nuspec.Root.Name.Namespace;
            var dependencies = nuspec.Descendants(xmlNs + "dependencies");
            net_version = "";
            pkgId = "";
            if (dependencies.Count() == 0)
            {
                return null;
            }
            var groups = dependencies.Elements(xmlNs + "group").Where(n => n.Attribute("targetFramework").Value.StartsWith(".NETStandard"));
            if (groups.Count() == 0)
            {
                return null;
            }
            var group = groups.First();
            var dependencys = group.Elements(xmlNs + "dependency");
            var packageList = new List<string>();
            foreach (var dependency in dependencys)
            {
                string id = dependency.Attribute("id").Value;
                string version = dependency.Attribute("version").Value;
                var isHit = ignoreList.Where(n => id.StartsWith(n)).Count();
                if (isHit != 0)
                    continue;
                packageList.Add($"{id}({version})");
            }
            // パッケージの正式名称を得る
            pkgId = nuspec.Descendants(xmlNs + "id").First().Value;
            // 正しいバージョンを取得する
            net_version = group.Attribute("targetFramework").Value.Replace(".NETStandard", "");
            return packageList;
        }

        /// <summary>
        /// パッケージのdllパスを得ます。
        /// </summary>
        /// <param name="packageName">パッケージ名</param>
        /// <param name="packageRoot">ダウンロードしたパッケージディレクトリ</param>
        /// <returns>dllリスト</returns>
        private static string DllPath(string packageName, string packageRoot, string version)
        {
            string libPath = Path.Combine(packageRoot, "lib");
            libPath = Path.Combine(libPath, "netstandard" + version);
            libPath = Path.Combine(libPath, packageName + ".dll");
            return libPath;
        }
    }
}
