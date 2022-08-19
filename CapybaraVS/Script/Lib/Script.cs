using CapyCSS;
using CapyCSS.Script;
using CapyCSSattribute;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CapyCSS.Script.Lib
{
    [ScriptClass]
    public static class Script
    {
        public const string LIB_Script_NAME = "Script.Message";

        [ScriptMethod(path: LIB_Script_NAME)]
        public static void ShowMessage(string title, string contents)
        {
            ControlTools.ShowSelectMessage(title, contents);
        }

        [ScriptMethod(path: LIB_Script_NAME)]
        public static MessageBoxResult ShowConfirmMessage(string title, string contents)
        {
            return ControlTools.ShowSelectMessage(title, contents, MessageBoxButton.OKCancel);
        }
    }
}