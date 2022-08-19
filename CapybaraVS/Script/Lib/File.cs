using CapyCSS.Controls.BaseControls;
using CapyCSS.Controls;
using CapyCSSattribute;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Media.Imaging;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace CapyCSS.Script.Lib
{
    [ScriptClass]
    public static class FileLib
    {
        private const string LIB_NAME = "File.Browse";

        //------------------------------------------------------------------
        [ScriptMethod(path: LIB_NAME, methodName: $"{CbSTUtils.SCRIPT_ONLY} {nameof(BrowseFile)}")]
        public static string BrowseFile(string title, string currentDir, string filterName = "all", string filterExt = "*.*")
        {
            return BrowseFile(
                title,
                currentDir,
                new List<Tuple<string, string>>()
                {
                    new Tuple<string, string>(filterName, filterExt)
                }
                );
        }

        [ScriptMethod(path: LIB_NAME, methodName: $"{CbSTUtils.SCRIPT_ONLY} {nameof(BrowseFile)}")]
        public static string BrowseFile(string title, string currentDir, IEnumerable<Tuple<string, string>> filters = null)
        {
            if (string.IsNullOrEmpty(currentDir))
            {
                currentDir = CommandCanvasList.GetSamplePath();
            }

            using (var dialog = new CommonOpenFileDialog()
            {
                Title = title,
                InitialDirectory = currentDir,
                EnsurePathExists = true,
            })
            {
                if (filters != null)
                {
                    foreach (var filter in filters)
                    {
                        dialog.Filters.Add(new CommonFileDialogFilter(filter.Item1, filter.Item2));
                    }
                }
                if (dialog.ShowDialog() != CommonFileDialogResult.Ok)
                {
                    return null;
                }
                return dialog.FileName;
            }
        }

        //------------------------------------------------------------------
        [ScriptMethod(path: LIB_NAME, methodName: $"{CbSTUtils.SCRIPT_ONLY} {nameof(BrowseFolder)}")]
        public static string BrowseFolder(string title, string currentDir)
        {
            if (string.IsNullOrEmpty(currentDir))
            {
                currentDir = CommandCanvasList.GetSamplePath();
            }

            using (var dialog = new CommonOpenFileDialog()
            {
                Title = title,
                InitialDirectory = currentDir,
                IsFolderPicker = true,
            })
            {
                if (dialog.ShowDialog() != CommonFileDialogResult.Ok)
                {
                    return null;
                }
                return dialog.FileName;
            }
        }
    }
}
