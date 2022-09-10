using CapyCSS.Controls;
using CapyCSS.Controls.BaseControls;
using CapyCSS.Script;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace CapyCSS
{
    class CsProject
    {
        /// <summary>
        /// .NETプロジェクトを作成します。
        /// </summary>
        public static void CreateCsProject()
        {
            string appBasePath = AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\');
            string cbsPath = CommandCanvasList.Instance.CurrentScriptCanvas.OpenFileName;
            string name = Path.GetFileNameWithoutExtension(cbsPath);
            string dir = Path.GetDirectoryName(cbsPath);

            // .NETプロジェクトの作成
            if (CommandCanvasList.Instance.CommandShellExecuter($"dotnet new console --force -o {name}", Encoding.UTF8) == 0)
            {
                // 作成したプロジェクトファイルを読み込む
                string projText = File.ReadAllText($"{name}/{name}.csproj", Encoding.GetEncoding("utf-8"));

                // DLLリファレンス定義の作成とDLLファイルのコピー
                string referenceInclude = "";
                foreach (var dll in CbSTUtils.BaseDllList)
                {
                    referenceInclude += $"  <ItemGroup>\n    <Reference Include=\"{dll}\"/>\n  </ItemGroup>\n";
                    copyDllFile(Path.Combine(appBasePath, dll), Path.Combine(name, dll));
                }
                foreach (var dll in CbSTUtils.AutoImportDllList)
                {
                    var _dll = Path.GetFileNameWithoutExtension(dll);
                    referenceInclude += $"  <ItemGroup>\n    <Reference Include=\"{_dll}\"/>\n  </ItemGroup>\n";
                    copyDllFile(Path.Combine(appBasePath, _dll), Path.Combine(name, _dll));
                }
                foreach (var dll in GetProjectDllList())
                {
                    referenceInclude += $"  <ItemGroup>\n    <Reference Include=\"{dll}\"/>\n  </ItemGroup>\n";
                    copyDllFile(dll, Path.Combine(name, dll));
                }

                // プロジェクトファイルにDLLリファレンス定義の組み込み
                projText = projText.Replace("</Project>", referenceInclude + "\n</Project>");

                // プロジェクトファイルのNullableを無効にする
                projText = projText.Replace("<Nullable>enable</Nullable>", "<Nullable>disable</Nullable>");

                // プロジェクトファイルを書き換える
                File.WriteAllText($"{name}/{name}.csproj", projText);

                // スクリプトをc#に変換し、Program.csに書き込む
                string script = CommandCanvasList.Instance.BuildScript();
                string programFile = Path.Combine(dir, name, "Program.cs");
                File.WriteAllText(programFile, script);

                Console.WriteLine("successfully created.");
            }
            else
            {
                Console.WriteLine("failed to create.");
            }
        }

        private static void copyDllFile(string from, string to)
        {
            to = to + ".dll";
            if (File.Exists(to))
            {
                File.Delete(to);
            }
            File.Copy(from + ".dll", to);
        }

        private static IEnumerable<string> GetProjectDllList()
        {
            var result = new List<string>();
            if (ProjectControl.Instance != null)
            {
                foreach (var dll in ProjectControl.Instance.GetDllList())
                {
                    result.Add(Path.GetFileNameWithoutExtension(dll));
                }
            }
            return result;
        }
    }
}
