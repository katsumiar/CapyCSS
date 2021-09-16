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
        private const string LIB_NAME = "File";

        //====================================================================================
        private const string LIB_NAME1 = LIB_NAME + ".Read";

        [ScriptMethod(LIB_NAME1)]
        public static StreamReader CreateReadStream(string fileName, string encoding = "utf-8")
        {
            var encodingCode = Encoding.GetEncoding(encoding);
            return new StreamReader(fileName, encodingCode);
        }

        //------------------------------------------------------------------
        [ScriptMethod(LIB_NAME1)]
        public static int StreamReadLine(
            StreamReader stream,
            bool autoClose,
            Action<string> func)
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
        [ScriptMethod(LIB_NAME1)]
        public static void CloseReadStream(StreamReader stream)
        {
            stream?.Close();
        }

        //====================================================================================
        private const string LIB_NAME2 = LIB_NAME + ".Writer";

        [ScriptMethod(LIB_NAME2)]
        public static StreamWriter CreateWriteStream(string fileName, bool append, string encoding = "utf-8")
        {
            var encodingCode = Encoding.GetEncoding(encoding);
            return new StreamWriter(fileName, append, encodingCode);
        }

        //------------------------------------------------------------------
        [ScriptMethod(LIB_NAME2)]
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
        [ScriptMethod(LIB_NAME2)]
        public static void CloseWriteStream(StreamWriter stream)
        {
            stream?.Close();
        }

        //------------------------------------------------------------------
        [ScriptMethod(LIB_NAME2)]
        public static void FileOpenAddWriteAndClose(string fileName, string str, bool lineMode, string encoding = "utf-8")
        {
            var stream = CreateWriteStream(fileName, true, encoding);
            StreamWrite(stream, str, lineMode, true);
        }

        //====================================================================================
        private const string LIB_NAME3 = LIB_NAME + ".Searcher";

        [ScriptMethod(LIB_NAME3)]
        public static long GetFilesSize(
            string path
            , string searchPattern = "*.*"
            , bool allDirectories = false)
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
        [ScriptMethod(LIB_NAME3)]
        static ICollection<string> GetDirectories(
            ICollection<string> list
            , string path
            , bool allDirectories
            , ICollection<string> ignoreList)
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

        [ScriptMethod(LIB_NAME3)]
        public static ICollection<string> GetDirectories(
            string path
            , bool allDirectories = false
            , ICollection<string> ignoreList = null)
        {
            var list = new List<string>();
            return GetDirectories(list, path, allDirectories, ignoreList);
        }

        //------------------------------------------------------------------
        [ScriptMethod(LIB_NAME3)]
        public static ICollection<string> GetFilesFromDirectories(
            IEnumerable<string> directories
            , string searchPattern = "*.*"
            , ICollection<string> ignoreList = null)
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

        //====================================================================================
        private const string LIB_NAME4 = LIB_NAME + ".Filtering";

        [ScriptMethod(LIB_NAME4)]
        public static ICollection<string> PathListToFileList(IEnumerable<string> pathList)
        {
            var list = new List<string>();
            foreach (var path in pathList)
            {
                list.Add(Path.GetFileName(path));
            }
            return list;
        }

        //------------------------------------------------------------------
        [ScriptMethod(LIB_NAME4)]
        public static ICollection<string> PathListToExtensionList(IEnumerable<string> pathList)
        {
            var list = new List<string>();
            foreach (var path in pathList)
            {
                list.Add(Path.GetExtension(path));
            }
            return list;
        }

        //------------------------------------------------------------------
        [ScriptMethod(LIB_NAME4)]
        public static ICollection<string> PathListToDirectoryList(IEnumerable<string> pathList)
        {
            var list = new List<string>();
            foreach (var path in pathList)
            {
                list.Add(Path.GetDirectoryName(path));
            }
            return list;
        }

        //====================================================================================
        [ScriptMethod(LIB_NAME)]
        public static string GetFileName(string path)
        {
            return Path.GetFileName(path);
        }

        //------------------------------------------------------------------
        [ScriptMethod(LIB_NAME)]
        public static string GetExtension(string path)
        {
            return Path.GetExtension(path);
        }

        //------------------------------------------------------------------
        [ScriptMethod(LIB_NAME)]
        public static string GetDirectoryName(string path)
        {
            return Path.GetDirectoryName(path);
        }

        //------------------------------------------------------------------
        [ScriptMethod(LIB_NAME)]
        public static string GetFileNameWithoutExtension(string path)
        {
            return Path.GetFileNameWithoutExtension(path);
        }

        //------------------------------------------------------------------
        [ScriptMethod(LIB_NAME)]
        public static string GetCurrentDirectory()
        {
            return Directory.GetCurrentDirectory();
        }

        //------------------------------------------------------------------
        [ScriptMethod(LIB_NAME)]
        public static string SetCurrentDirectory(string path)
        {
            Directory.SetCurrentDirectory(path);
            return path;
        }

        //------------------------------------------------------------------
        [ScriptMethod(LIB_NAME)]
        public static string GetPathRoot(string path)
        {
            return Path.GetPathRoot(path);
        }

        //------------------------------------------------------------------
        [ScriptMethod(LIB_NAME)]
        public static string GetFullPath(string path)
        {
            return Path.GetFullPath(path);
        }

        //------------------------------------------------------------------
        [ScriptMethod(LIB_NAME)]
        public static bool HasExtension(string path)
        {
            return Path.HasExtension(path);
        }

        //------------------------------------------------------------------
        [ScriptMethod(LIB_NAME)]
        public static bool IsPathRooted(string path)
        {
            return Path.IsPathRooted(path);
        }

        //------------------------------------------------------------------
        [ScriptMethod(LIB_NAME)]
        public static bool Exists(string path)
        {
            return File.Exists(path);
        }

        //------------------------------------------------------------------
        [ScriptMethod(LIB_NAME)]
        public static bool DirectoryExists(string path)
        {
            return Directory.Exists(path);
        }

        //------------------------------------------------------------------
        [ScriptMethod(LIB_NAME)]
        public static void Delete(string path)
        {
            File.Delete(path);
        }

        //------------------------------------------------------------------
        [ScriptMethod(LIB_NAME)]
        public static void Copy(string sourceFileName, string destFileName, bool overwrite = true)
        {
            File.Copy(sourceFileName, destFileName, overwrite);
        }

        //------------------------------------------------------------------
        [ScriptMethod(LIB_NAME)]
        public static void Move(string sourceFileName, string destFileName, bool overwrite = true)
        {
            File.Move(sourceFileName, destFileName, overwrite);
        }

        //------------------------------------------------------------------
        [ScriptMethod(LIB_NAME)]
        public static long GetFileSize(string path)
        {
            FileInfo file = new FileInfo(path);
            return file.Length;
        }

        //------------------------------------------------------------------
        [ScriptMethod(LIB_NAME)]
        public static string CreateDirectory(
            string path
            , bool addSerialNumber = false)
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
        [ScriptMethod(LIB_NAME)]
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
        [ScriptMethod(LIB_NAME)]
        public static ICollection<string> GetFiles(
            string path
            , string searchPattern = "*.*"
            , bool allDirectories = false
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
        [ScriptMethod(LIB_NAME)]
        public static int ForeachFiles(
            string path
            , Action<string> func
            , string searchPattern = "*.*"
            , bool allDirectories = false
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
    }
}
