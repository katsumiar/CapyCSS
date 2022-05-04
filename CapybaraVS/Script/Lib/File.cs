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

        //------------------------------------------------------------------
        [ScriptMethod(path: LIB_NAME)]
        public static string BrowseFile(string title, string currentDir, string filter = "all (*.*)|*.*")
        {
            var dialog = new OpenFileDialog();
            if (!string.IsNullOrEmpty(title))
                dialog.Title = title;
            if (!string.IsNullOrEmpty(currentDir))
                dialog.InitialDirectory = currentDir;
            else
                dialog.InitialDirectory = CommandCanvasList.GetSamplePath();
            if (!string.IsNullOrEmpty(filter))
                dialog.Filter = filter;
            if (dialog.ShowDialog() == true)
            {
                return dialog.FileName;
            }
            return null;
        }

        //------------------------------------------------------------------
        [ScriptMethod(path: LIB_NAME)]
        public static string BrowseFolder(string title, string currentDir)
        {
            var dialog = new OpenFileDialog() { FileName = "SelectFolder", Filter = "Folder|.", CheckFileExists = false };
            if (!string.IsNullOrEmpty(title))
                dialog.Title = title;
            if (!string.IsNullOrEmpty(currentDir))
                dialog.InitialDirectory = currentDir;
            else
                dialog.InitialDirectory = CommandCanvasList.GetSamplePath();
            if (dialog.ShowDialog() == true)
            {
                return Path.GetDirectoryName(dialog.FileName);
            }
            return null;
        }
    }
}
