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
        public const string LIB_Script_NAME = "Script";

        [Conditional("DEBUG")]
        [ScriptMethod]
        // テスト用
        public static void DelegateTest1<T>(T arg1, T arg2, T arg3, Action<T, T, T> action)
        {
            action?.Invoke(arg1, arg2, arg3);
        }

        [ScriptMethod]
        // テスト用
        public static T DelegateTest2<T>(T arg1, T arg2, T arg3, Func<T, T, T, T> func)
        {
            if (func is null)
                return arg1;
            return func(arg1, arg2, arg3);
        }

        [Conditional("DEBUG")]
        [ScriptMethod]
        // テスト用
        public static void DelegateTest<T>(T arg1, T arg2, Action<T, T, T> action1, Func<T, T, T> func)
        {
            action1?.Invoke(arg1, arg2, (func is null) ? arg2 : func(arg1, arg2));
        }

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