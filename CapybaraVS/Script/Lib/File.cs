using CapyCSS.Controls;
using CbVS.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Media.Imaging;

namespace CapybaraVS.Script.Lib
{
    public class FileLib
    {
        [ScriptMethod("Input/Output" + ".ConsoleOut.List." + "Out String List", "",
            "RS=>FileLib_ConsoleOutStringList"//"文字列リストをコンソールに出力します。"
            )]
        public static int ConsoleOutStringList(List<string> list)
        {
            foreach (var node in list)
            {
                CommandCanvasList.OutPut.OutLine(nameof(ConsoleOut), node);
            }
            return list.Count;
        }

        //------------------------------------------------------------------
        [ScriptMethod("Input/Output" + ".ConsoleOut.List." + "Out Int List", "",
            "RS=>FileLib_ConsoleOutIntList"//"int 型リストをコンソールに出力します。"
            )]
        public static int ConsoleOutIntList(List<int> list)
        {
            foreach (var node in list)
            {
                CommandCanvasList.OutPut.OutLine(nameof(ConsoleOut), node.ToString());
            }
            return list.Count;
        }

        //------------------------------------------------------------------
        [ScriptMethod("Input/Output" + ".ConsoleOut.List." + "Out Long List", "",
            "RS=>FileLib_ConsoleOutLongList"//"long 型リストをコンソールに出力します。"
            )]
        public static int ConsoleOutLongList(List<long> list)
        {
            foreach (var node in list)
            {
                CommandCanvasList.OutPut.OutLine(nameof(ConsoleOut), node.ToString());
            }
            return list.Count;
        }

        //------------------------------------------------------------------
        [ScriptMethod("Input/Output" + ".ConsoleOut.List." + "Out Double List", "",
            "RS=>FileLib_ConsoleOutDoubleList"//"double 型リストをコンソールに出力します。"
            )]
        public static int ConsoleOutDoubleList(List<double> list)
        {
            foreach (var node in list)
            {
                CommandCanvasList.OutPut.OutLine(nameof(ConsoleOut), node.ToString());
            }
            return list.Count;
        }

        //------------------------------------------------------------------
        [ScriptMethod("Input/Output" + ".ConsoleOut.List." + "Out Bool List", "",
            "RS=>FileLib_ConsoleOutBoolList"//"bool 型リストをコンソールに出力します。"
            )]
        public static int ConsoleOutBoolList(List<bool> list)
        {
            foreach (var node in list)
            {
                CommandCanvasList.OutPut.OutLine(nameof(ConsoleOut), node.ToString());
            }
            return list.Count;
        }

        //------------------------------------------------------------------
        [ScriptMethod("File" + ".Reader." + "GetReadStreamVariable",
            "[ {0} ]",  // 変数名用
            "RS=>FileLib_CreateReadStreamVariable"//"<stream> 入力用ファイルストリーム変数を作成もしくは参照します。", "(none)"
            )]
        public static StreamReader CreateReadStreamVariable(ref StreamReader stream)
        {
            return stream;
        }

        //------------------------------------------------------------------
        [ScriptMethod("File" + ".Reader" + "." + "CreateReadStream", "",
            "RS=>FileLib_GetReadStream"//"入力用ファイルストリームを作成します。"
            )]
        public static StreamReader GetReadStream(string fileName, string encoding = "utf-8")
        {
            var encodingCode = Encoding.GetEncoding(encoding);
            return new StreamReader(fileName, encodingCode);
        }

        //------------------------------------------------------------------
        [ScriptMethod("File" + ".Reader." + nameof(StreamReadLine), "",
            "RS=>FileLib_StreamReadLine"//"ファイル入力：\n<stream> 入力用ファイルストリームから１行の文字列を入力しその内容を仮引数に <func> をコールし行数を返します。\n<autoClose> が True の場合、入力後にファイルストリームを破棄します。"
            )]
        public static int StreamReadLine(
            StreamReader stream,
            bool autoClose,
            [param: ScriptParam("func f(line)")] Action<string> func)
        {
            int ret = 0;
            if (func is null)
                return ret;
            if (stream != null)
                while (!stream.EndOfStream)
                {
                    string line = stream.ReadLine();
                    ret++;
                    func.Invoke(line);
                }

            if (autoClose)
                stream?.Close();

            return ret;
        }

        //------------------------------------------------------------------
        [ScriptMethod("File" + ".Reader." + nameof(CloseReadStream), "",
            "RS=>FileLib_CloseReadStream"//"<stream> 入力用ファイルストリームを破棄します。"
            )]
        public static void CloseReadStream(StreamReader stream)
        {
            stream?.Close();
        }

        //------------------------------------------------------------------
        [ScriptMethod("File" + ".Writer." + "GetWriteStreamVariable",
            "[ {0} ]",  // 変数名用
            "RS=>FileLib_CreateWriteStreamVariable"//"<stream> 出力用ファイルストリーム変数を作成もしくは参照します。", "(none)"
            )]
        public static StreamWriter CreateWriteStreamVariable(ref StreamWriter stream)
        {
            return stream;
        }

        //------------------------------------------------------------------
        [ScriptMethod("File" + ".Writer" + "." + "CreateWriteStream", "",
            "RS=>FileLib_GetWriteStream"//"出力用ファイルストリームを作成：\n<stream> 出力用ファイルストリームを作成します。\n<append> を True にした場合、追加書き込み用に作成します。"
            )]
        public static StreamWriter GetWriteStream(string fileName, bool append, string encoding = "utf-8")
        {
            var encodingCode = Encoding.GetEncoding(encoding);
            return new StreamWriter(fileName, append, encodingCode);
        }

        //------------------------------------------------------------------
        [ScriptMethod("File" + ".Writer" + "." + nameof(StreamWrite), "",
            "RS=>FileLib_StreamWrite"//"ファイル出力：\n出力用ファイルストリームに <str> 文字列を出力します。\n<lineMode> が True の場合、出力後に改行します。\n<autoClose> が True の場合、出力後にファイルストリームを破棄します。"
            )]
        public static string StreamWrite(
            StreamWriter stream,
            string str,
            bool lineMode,
            bool autoClose)
        {
            if (lineMode)
                stream?.WriteLine(str);
            else
                stream?.Write(str);

            if (autoClose)
                stream?.Close();

            return str;
        }

        //------------------------------------------------------------------
        [ScriptMethod("File" + ".Writer." + nameof(CloseWriteStream), "",
            "RS=>FileLib_CloseWriteStream"//"<stream> 出力用ファイルストリームを破棄します。"
            )]
        public static void CloseWriteStream(StreamWriter stream)
        {
            stream?.Close();
        }

        //------------------------------------------------------------------
        [ScriptMethod("File" + ".Writer." + nameof(FileOpenAddWriteAndClose), "",
            "RS=>FileLib_FileOpenAddWriteAndClose"//"ファイルへの追加出力：\n<fileName> ファイルへ <str> テキストを出力します。\n<lineMode> が True の場合、出力後に改行します。\n<fileName> ファイル が無ければ作成します。"
            )]
        public static void FileOpenAddWriteAndClose(string fileName, string str, bool lineMode, string encoding = "utf-8")
        {
            var stream = GetWriteStream(fileName, true, encoding);
            StreamWrite(stream, str, lineMode, true);
        }

        //------------------------------------------------------------------
        [ScriptMethod("File" + "." + nameof(GetFileName), "",
            "RS=>FileLib_GetFileName"//"パスからファイル名を参照します。"
            )]
        public static string GetFileName(string path)
        {
            return Path.GetFileName(path);
        }

        //------------------------------------------------------------------
        [ScriptMethod("File" + "." + nameof(GetExtension), "",
            "RS=>FileLib_GetExtension"//"パスから拡張子を参照します。"
            )]
        public static string GetExtension(string path)
        {
            return Path.GetExtension(path);
        }

        //------------------------------------------------------------------
        [ScriptMethod("File" + "." + nameof(GetDirectoryName), "",
            "RS=>FileLib_GetDirectoryName"//"パスからディレクトリ名を参照します。"
            )]
        public static string GetDirectoryName(string path)
        {
            return Path.GetDirectoryName(path);
        }

        //------------------------------------------------------------------
        [ScriptMethod("File" + "." + nameof(GetFileNameWithoutExtension), "",
            "RS=>FileLib_GetFileNameWithoutExtension"//"パスの拡張子のないファイル名を参照します。"
            )]
        public static string GetFileNameWithoutExtension(string path)
        {
            return Path.GetFileNameWithoutExtension(path);
        }

        //------------------------------------------------------------------
        [ScriptMethod("File" + "." + nameof(GetCurrentDirectory), "",
            "RS=>FileLib_GetCurrentDirectory"//"カレントディレクトリ参照：\n現在のカレントディレクトリを参照します。"
            )]
        public static string GetCurrentDirectory()
        {
            return Directory.GetCurrentDirectory();
        }

        //------------------------------------------------------------------
        [ScriptMethod("File" + "." + nameof(SetCurrentDirectory), "",
            "RS=>FileLib_SetCurrentDirectory"//"カレントディレクトリ変更：\n現在のカレントディレクトリを変更します。"
            )]
        public static string SetCurrentDirectory(string path)
        {
            Directory.SetCurrentDirectory(path);
            return path;
        }

        //------------------------------------------------------------------
        [ScriptMethod("File" + "." + nameof(GetPathRoot), "",
            "RS=>FileLib_GetPathRoot"//"パスからルートディレクトリ名を参照します。"
            )]
        public static string GetPathRoot(string path)
        {
            return Path.GetPathRoot(path);
        }

        //------------------------------------------------------------------
        [ScriptMethod("File" + "." + nameof(GetFullPath), "",
            "RS=>FileLib_GetFullPath"//"パスから絶対パスを参照します。"
            )]
        public static string GetFullPath(string path)
        {
            return Path.GetFullPath(path);
        }

        //------------------------------------------------------------------
        [ScriptMethod("File" + "." + nameof(HasExtension), "",
            "RS=>FileLib_HasExtension"//"拡張子の存在チェック：\nパスに拡張子が含まれていれば True を返します。"
            )]
        public static bool HasExtension(string path)
        {
            return Path.HasExtension(path);
        }

        //------------------------------------------------------------------
        [ScriptMethod("File" + "." + nameof(IsPathRooted), "",
            "RS=>FileLib_IsPathRooted"//"パスにルートが含まれていれば True を返します。"
            )]
        public static bool IsPathRooted(string path)
        {
            return Path.IsPathRooted(path);
        }

        //------------------------------------------------------------------
        [ScriptMethod("File" + "." + "FileExists", "",
            "RS=>FileLib_Exists"//"ファイルの存在チェック：\nファイルが存在していれば True を返します。"
            )]
        public static bool Exists(string path)
        {
            return File.Exists(path);
        }

        //------------------------------------------------------------------
        [ScriptMethod("File" + "." + "DirectoryExists", "",
            "RS=>FileLib_DirectoryExists"//"ディレクトリの存在チェック：\nファイルが存在していれば True を返します。"
            )]
        public static bool DirectoryExists(string path)
        {
            return Directory.Exists(path);
        }

        //------------------------------------------------------------------
        [ScriptMethod("File" + "." + nameof(Delete), "",
            "RS=>FileLib_Delete"//"ファイル削除：\nファイルを削除します。"
            )]
        public static void Delete(string path)
        {
            File.Delete(path);
        }

        //------------------------------------------------------------------
        [ScriptMethod("File" + "." + nameof(Copy), "",
            "RS=>FileLib_Copy"//"ファイルの複製：\nファイルをコピーします。"
            )]
        public static void Copy(string sourceFileName, string destFileName, bool overwrite = true)
        {
            File.Copy(sourceFileName, destFileName, overwrite);
        }

        //------------------------------------------------------------------
        [ScriptMethod("File" + "." + nameof(Move), "",
            "RS=>FileLib_Move"//"ファイル名変更（もしくは移動）：\n<sourceFileName> から <destFileName> に変更します。"
            )]
        public static void Move(string sourceFileName, string destFileName, bool overwrite = true)
        {
            File.Move(sourceFileName, destFileName, overwrite);
        }

        //------------------------------------------------------------------
        [ScriptMethod("File" + "." + nameof(GetFileSize), "",
            "RS=>FileLib_GetFileSize"//"ファイルサイズ参照：\nファイルサイズを参照します。"
            )]
        public static long GetFileSize(string path)
        {
            FileInfo file = new FileInfo(path);
            return file.Length;
        }

        //------------------------------------------------------------------
        [ScriptMethod("File" + "." + nameof(CreateDirectory), "",
            "RS=>FileLib_CreateDirectory"//"ディレクトリ作成：\n<add serial number> が True のときは、すでに同名のディレクトリが存在する場合は、2から番号を末尾に追加して作成します。"
            )]
        public static string CreateDirectory(
            string path
            , [param: ScriptParam("add serial number")] bool addSerialNumber = false)
        {
            var newPath = path;
            if (Directory.Exists(newPath))
            {
                if (!addSerialNumber)
                    return newPath;

                int i = 2;
                newPath = $"{path}({i})";
                while (Directory.Exists(newPath))
                {
                    newPath = $"{path}({i++})";
                }
            }
            Directory.CreateDirectory(newPath);
            return newPath;
        }

        //------------------------------------------------------------------
        [ScriptMethod("File" + "." + nameof(Combine), "",
            "RS=>FileLib_Combine"//"複数の文字列を１つのパスに結合します。\n<slashSeparate> が True のときは、<//> の代わりに / でディレクトリを区切ります。"
            )]
        public static string Combine(List<string> paths, bool slashSeparate = false)
        {
            string path = Path.Combine(paths.ToArray());
            if (slashSeparate)
            {
                path = path.Replace(@"\", @"/");
            }
            return path;
        }

        //------------------------------------------------------------------
        [ScriptMethod("File" + ".Searcher." + nameof(GetFiles), "",
            "RS=>FileLib_GetFiles"//"ファイル検索：\n<search pattern> パターンでファイル名を検索します。\n<all directories> が True の場合は、サブディレクトリ以下も再帰的に検索します。"
            )]
        public static List<string> GetFiles(
            string path
            , [param: ScriptParam("search pattern")] string searchPattern = "*.*"
            , [param: ScriptParam("all directories")] bool allDirectories = false
            , bool relativePath = false)
        {
            IEnumerable<string> files;
            if (allDirectories)
                files = Directory.EnumerateFiles(path, searchPattern, SearchOption.AllDirectories);
            else
                files = Directory.EnumerateFiles(path, searchPattern);
            var list = new List<string>(files);
            if (relativePath)
            {
                if (path.EndsWith(@"\"))
                    path = path.Replace(path, "");
                else
                    path = path.Replace(path + @"\", "");

                list.ForEach(s => s = s.Replace(path, ""));

            }
            return list;
        }

        //------------------------------------------------------------------
        [ScriptMethod("File" + ".Searcher." + "ForeachFiles Invoke", "",
            "RS=>FileLib_ForeachFilesInvoke"//"ファイル検索：\n<search pattern> パターンでファイル名を検索し見つかったファイル名を仮引数に <event> をコールします。\n<all directories> が True の場合は、サブディレクトリ以下も再帰的に検索します。\n※<invoke> には Assignment Func を接続します。"
            )]
        public static int ForeachFilesInvoke(
            string path
            , [param: ScriptParam("func f(path)")] Action<string> func
            , [param: ScriptParam("search pattern")] string searchPattern
            , [param: ScriptParam("all directories")] bool allDirectories = false
            , bool relativePath = false)
        {
            if (func is null)
            {
                return 0;
            }
            var files = FileLib.GetFiles(path, searchPattern, allDirectories);
            if (path.EndsWith(@"\"))
                path = path.Replace(path, "");
            else
                path = path.Replace(path + @"\", "");
            foreach (var node in files)
            {
                string _path = node;
                if (relativePath)
                {
                    _path = node.Replace(path + @"\", "");
                }
                func.Invoke(_path);
            }
            return files.Count;
        }

        //------------------------------------------------------------------
        [ScriptMethod("File" + ".Searcher." + nameof(GetFilesSize), "",
            "RS=>FileLib_GetFilesSize"//"トータルのファイルサイズを参照：\n<search pattern> パターンでファイルを検索しトータルのファイルサイズを参照します。\n<all directories> が True の場合は、サブディレクトリ以下も再帰的に検索します。"
            )]
        public static long GetFilesSize(
            string path
            , [param: ScriptParam("search pattern")] string searchPattern = "*.*"
            , [param: ScriptParam("all directories")] bool allDirectories = false)
        {
            IEnumerable<string> files;
            if (allDirectories)
                files = Directory.EnumerateFiles(path, searchPattern, SearchOption.AllDirectories);
            else
                files = Directory.EnumerateFiles(path, searchPattern);
            long size = 0;
            foreach (var node in files)
            {
                size += GetFileSize(node);
            }
            return size;
        }

        //------------------------------------------------------------------
        static List<string> GetDirectories(List<string> list, string path, bool allDirectories, List<string> ignoreList)
        {
            list ??= new List<string>();
            string[] dirs = Directory.GetDirectories(path);
            foreach (var dir in dirs)
            {
                if (ignoreList != null && ignoreList.Contains(Path.GetFileName(dir)))
                    continue;

                list.Add(dir);
                if (allDirectories)
                {
                    GetDirectories(list, dir, allDirectories, ignoreList);
                }
            }
            return list;
        }

        [ScriptMethod("File" + ".Searcher." + nameof(GetDirectories), "",
            "RS=>FileLib_GetDirectories"//"ディレクトリ一覧を参照：\n<path> ディレクトリにあるサブディレクトリ一覧を参照します。\n<all directories> が True の場合は、サブディレクトリ以下も再帰的に検索します。\n<ignore list> には無視するディレクトリ名を指定します。"
            )]
        public static List<string> GetDirectories(
            string path
            , [param: ScriptParam("all directories")] bool allDirectories = false
            , [param: ScriptParam("ignore list")] List<string> ignoreList = null)
        {
            var list = new List<string>();
            return GetDirectories(list, path, allDirectories, ignoreList);
        }

        //------------------------------------------------------------------
        [ScriptMethod("File" + ".Searcher." + nameof(GetFilesFromDirectories), "",
            "RS=>FileLib_GetFilsFromDirectories"//"ディレクトリ一覧を参照：\n<directories>ディレクトリリストにあるディレクトリからサブディレクトリ一覧を参照します。\n<all directories> が True の場合は、サブディレクトリ以下も再帰的に検索します。\n<ignore list> には無視するディレクトリ名を指定します。"
            )]
        public static List<string> GetFilesFromDirectories(
            List<string> directories
            , [param: ScriptParam("search pattern")] string searchPattern = "*.*"
            , [param: ScriptParam("ignore list")] List<string> ignoreList = null)
        {
            var list = new List<string>();
            foreach (var dir in directories)
            {
                string[] files = Directory.GetFiles(dir, searchPattern);
                if (ignoreList != null)
                {
                    foreach (var file in files)
                    {
                        if (ignoreList != null && ignoreList.Contains(file))
                            continue;

                        list.Add(file);
                    }
                }
                else
                    list.AddRange(files);
            }
            return list;
        }

        //------------------------------------------------------------------
        [ScriptMethod("File" + ".Filtering." + nameof(PathListToFileList), "",
            "RS=>FileLib_PathListToFileList"//"パスリストをファイル名リストに変換します。"
            )]
        public static List<string> PathListToFileList(List<string> pathList)
        {
            var list = new List<string>();
            foreach (var path in pathList)
            {
                list.Add(Path.GetFileName(path));
            }
            return list;
        }

        //------------------------------------------------------------------
        [ScriptMethod("File" + ".Filtering." + nameof(PathListToExtensionList), "",
            "RS=>FileLib_PathListToExtensionList"//"パスリストを拡張子リストに変換します。"
            )]
        public static List<string> PathListToExtensionList(List<string> pathList)
        {
            var list = new List<string>();
            foreach (var path in pathList)
            {
                list.Add(Path.GetExtension(path));
            }
            return list;
        }

        //------------------------------------------------------------------
        [ScriptMethod("File" + ".Filtering." + nameof(PathListToDirectoryList), "",
            "RS=>FileLib_PathListToDirectoryList"//"パスリストをディレクトリリストに変換します。"
            )]
        public static List<string> PathListToDirectoryList(List<string> pathList)
        {
            var list = new List<string>();
            foreach (var path in pathList)
            {
                list.Add(Path.GetDirectoryName(path));
            }
            return list;
        }
    }
}
