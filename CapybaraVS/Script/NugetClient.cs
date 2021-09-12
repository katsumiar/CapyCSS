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
using CapybaraVS;
using CapybaraVS.Script.Lib;

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
            pkgId = "";
            string toolPath = Path.Combine(packageDir, "nuget.exe");
            if (!File.Exists(toolPath))
            {
                ControlTools.ShowErrorMessage($"Nuget.exe を {packageDir} に置いて下さい。");
                return null;
            }

            string packageVName = $"{packageName}.{version}";
            string packageRoot = Path.Combine(packageDir, packageVName);

            if (!Directory.Exists(packageRoot))
            {
                // nugetファイルをダウンロード

                CommandCanvasList.OutPut.OutString(nameof(NugetClient), $"Get[{packageVName}]...");

                ToolExec toolExec = new ToolExec(toolPath);
                toolExec.ParamList.Add("Install");
                toolExec.ParamList.Add(packageName);
                toolExec.ParamList.Add("-Version");
                toolExec.ParamList.Add(version);
                toolExec.ParamList.Add("-PackageSaveMode");
                toolExec.ParamList.Add("nuspec");
                toolExec.ParamList.Add("-OutputDirectory");
                toolExec.ParamList.Add(packageDir);
                toolExec.Start(true);

                CommandCanvasList.OutPut.OutString(nameof(NugetClient), $"OK");
            }

            List<string> dllList = new List<string>();
            List<string> libPathList = new List<string>(FileLib.GetDirectories(packageDir, true));
            libPathList.Sort((a, b) => b.CompareTo(a));
            foreach (var dir in libPathList)
            {
                if (dir.Contains(@"\ref\"))
                    continue;
                if (dir.Contains("netstandard"))
                    {
                    ICollection<string> dll = FileLib.GetFiles(dir, "*.dll");
                    if (dll.Count != 0)
                    {
                        bool isHit = dllList.Any((n) => dir.StartsWith(n.Substring(0, n.IndexOf("netstandard"))));
                        if (isHit)
                            continue;
                        dllList.Add(dll.First());
                    } 
                }
            }

            // dll をリストに登録
            var packageList = new List<PackageInfo>();
            if (dllList != null && dllList.Count != 0)
            {
                string basePass = System.IO.Path.GetDirectoryName(dllList[dllList.Count - 1]);
                foreach (var libPath in dllList)
                {
                    // 内部に複数のサブディレクトリを持っている場合、サブディレクトリ名を名前に残す
                    string name = libPath.Substring(libPath.LastIndexOf("\\")).Replace(".lib","").Replace(" ", "");

                    if (loadedPackages.Contains(libPath))
                    {
                        // 一度取り込んでいるdllは無視する

                        continue;
                    }
                    loadedPackages.Add(libPath);
                    packageList.Add(new PackageInfo(libPath, name, version));
                }
            }
            pkgId = packageVName;
            return packageList;
        }
    }
}
