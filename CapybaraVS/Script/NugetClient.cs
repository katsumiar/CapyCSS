using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO.Compression;
using System.IO;
using System.Xml.Linq;
using CapyCSS.Controls;

namespace CapyCSS.Script
{
    class NugetClient
    {
        private static List<string> loadedPackages = new List<string>();

        public class PackageInfo
        {
            public string Path { get; set; }
            public string Name { get; set; }
            public string Version { get; set; }
            public PackageInfo(string path, string name, string version)
            {
                Path = path;
                Name = name;
                Version = version;
            }
        }

        /// <summary>
        /// パッケージをインストールします。
        /// ただし、既にインストール済みならインストールはしません。
        /// </summary>
        /// <param name="packageDir">パッケージ展開用ディレクトリ</param>
        /// <param name="packageName">バージョン情報付きパッケージ名（name(var)）</param>
        /// <param name="pkgId">正式なパッケージ名（name(var)）</param>
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
        /// <param name="pkgId">正式なパッケージ名（name(var)）</param>
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
                CommandCanvasList.OutPut.OutString(nameof(NugetClient), $"Get..");
                webClient.DownloadFile($"https://www.nuget.org/api/v2/package/{packageName}/{version}", nupkg);
                CommandCanvasList.OutPut.OutString(nameof(NugetClient), $".");
                ZipFile.ExtractToDirectory(nupkg, packageRoot); // zip展開
                File.Delete(nupkg);
                CommandCanvasList.OutPut.OutLine(nameof(NugetClient), $"{packageName}({version}) package.");
            }

            // 依存パッケージを取得
            pkgId = "";
            IEnumerable<string> packages = GetDependencies(packageName, packageRoot, out string _pkgId, out string net_version);
            if (packages is null)
            {
                return null;
            }
            foreach (var package in packages)
            {
                List<PackageInfo> list = install(packageDir, package, out pkgId);
                if (list != null)
                {
                    packageList.AddRange(list);
                }
            }

            // dll をリストに登録
            string[] libPathList = DllPath(packageName, packageRoot, net_version);
            if (libPathList != null && libPathList.Length != 0)
            {
                string basePass = System.IO.Path.GetDirectoryName(libPathList[libPathList.Length - 1]);
                foreach (var libPath in libPathList)
                {
                    // 内部に複数のサブディレクトリを持っている場合、サブディレクトリ名を名前に残す
                    string name = libPath.Replace(basePass, "");
                    name = name.Replace(@"\", "/").TrimStart('/');

                    if (loadedPackages.Contains(libPath))
                    {
                        // 一度取り込んでいるdllは無視する

                        continue;
                    }
                    loadedPackages.Add(libPath);
                    packageList.Add(new PackageInfo(libPath, name, version));
                }
            }
            pkgId = _pkgId;
            return packageList;
        }

        /// <summary>
        /// .net standard のバージョンリスト
        /// </summary>
        private static List<string> netStandardVerList = new List<string>()
            {
                "2.1",
                "2.0",
                "1.6",
                "1.5",
                "1.4",
                "1.3",
                "1.2",
                "1.1",
                "1.0"
            };

        /// <summary>
        /// 依存パッケージを取得します。
        /// ※個々の依存の中にも依存がある場合があるが無視している...
        /// </summary>
        /// <param name="packageName">パッケージ名</param>
        /// <param name="packageRoot">ダウンロードしたパッケージディレクトリ</param>
        /// <param name="pkgId">正式なパッケージ名（name(var)）</param>
        /// <param name="net_version">バージョン（name(var)）</param>
        /// <returns>パッケージ一覧</returns>
        private static IEnumerable<string> GetDependencies(string packageName, string packageRoot, out string pkgId, out string net_version)
        {
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
            IEnumerable<XElement> groups = null;
            foreach (var ver in netStandardVerList)
            {
                // .NET Standard の最新バージョンを探す

                groups = dependencies.Elements(xmlNs + "group").Where(n => n.Attribute("targetFramework").Value.StartsWith($".NETStandard{ver}"));
                if (groups.Count() != 0)
                {
                    break;
                }
            }
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
                if (version.Contains(","))
                {
                    version = version.TrimStart('[').TrimEnd(']');
                }
                if (version.Contains(","))
                {
                    // バージョン, バージョン[, ...] 形式

                    var tkn = version.Split(',');
                    version = tkn[tkn.Length - 1].Trim();
                }
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
        /// <param name="version">.net standard のバージョン</param>
        /// <returns>dllリスト</returns>
        private static string[] DllPath(string packageName, string packageRoot, string version)
        {
            string libPath = Path.Combine(packageRoot, "lib");
            libPath = Path.Combine(libPath, "netstandard" + version);
            string[] libPathList = null;
            foreach (var ver in netStandardVerList)
            {
                // ディレクトリが存在しないことがある...

                if (System.IO.Directory.Exists(libPath))
                {
                    libPathList = System.IO.Directory.GetFiles(libPath, "*.dll", SearchOption.AllDirectories);
                }
                if (libPath.EndsWith("1.0"))
                {
                    return null;
                }
                // 一先ず手抜き...
                libPath.Replace("1.1", "1.0");
                libPath.Replace("1.2", "1.1");
                libPath.Replace("1.3", "1.2");
                libPath.Replace("1.4", "1.3");
                libPath.Replace("1.5", "1.4");
                libPath.Replace("1.6", "1.5");
                libPath.Replace("2.0", "1.6");
                libPath.Replace("2.1", "2.0");
            }
            if (libPathList != null && libPathList.Length > 1)
            {
                // 最初のファイルを一番うしろに持っていく
                //※一番最後のdllのメソッドが取り込まれる（すべて取り込む場合は、意味無し）

                var temp = libPathList[0];
                libPathList[0] = libPathList[libPathList.Length - 1];
                libPathList[libPathList.Length - 1] = temp;
            }
            return libPathList;
        }
    }
}
