using CapyCSS.Controls.BaseControls;
using CapyCSS.Controls;
using CapyCSSattribute;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Media.Imaging;

namespace CapyCSS.Script.Lib
{
    [ScriptClass]
    public static class FileLib
    {
        private const string LIB_NAME = "File";

        //====================================================================================
        private const string LIB_NAME1 = LIB_NAME + ".Read";

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

        //====================================================================================
        private const string LIB_NAME2 = LIB_NAME + ".Writer";

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
        public static void FileOpenAddWriteAndClose(string fileName, string str, bool lineMode, string encoding = "utf-8")
        {
            var stream = new StreamWriter(fileName, true, Encoding.GetEncoding(encoding));
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
            var files = new List<string>();
            if (allDirectories)
            {
                try
                {
                    var directories = Directory.EnumerateDirectories(path);
                    foreach (var dir in directories)
                    {
                        files.AddRange(GetFiles(dir, searchPattern, allDirectories, relativePath));
                    }
                }
                catch (Exception)
                {
                }
            }

            try
            {
                files.AddRange(Directory.EnumerateFiles(path, searchPattern));
            }
            catch (Exception)
            {
            }

            if (relativePath)
            {
                if (path.EndsWith(@"\"))
                    path = path.Replace(path, "");
                else
                    path = path.Replace(path + @"\", "");
                files.ForEach(s => s = s.Replace(path, ""));
            }
            return files;
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
            if (path is null)
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
        [ScriptMethod(LIB_NAME)]
        public static string BrowseFile(string title, string currentDir, string filter = "all (*.*)|*.*")
        {
            var dialog = new OpenFileDialog();
            if (!string.IsNullOrEmpty(title))
                dialog.Title = title;
            if (!string.IsNullOrEmpty(currentDir))
                dialog.InitialDirectory = currentDir;
            else
                dialog.InitialDirectory = CommandCanvas.GetSamplePath();
            if (!string.IsNullOrEmpty(filter))
                dialog.Filter = filter;
            if (dialog.ShowDialog() == true)
            {
                return dialog.FileName;
            }
            return null;
        }

        //------------------------------------------------------------------
        [ScriptMethod(LIB_NAME)]
        public static string BrowseFolder(string title, string currentDir)
        {
            var dialog = new OpenFileDialog() { FileName = "SelectFolder", Filter = "Folder|.", CheckFileExists = false };
            if (!string.IsNullOrEmpty(title))
                dialog.Title = title;
            if (!string.IsNullOrEmpty(currentDir))
                dialog.InitialDirectory = currentDir;
            else
                dialog.InitialDirectory = CommandCanvas.GetSamplePath();
            if (dialog.ShowDialog() == true)
            {
                return Path.GetDirectoryName(dialog.FileName);
            }
            return null;
        }
    }
}
