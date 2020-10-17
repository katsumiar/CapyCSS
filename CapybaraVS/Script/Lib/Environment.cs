using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace CapybaraVS.Script.Lib
{
    class EnvironmentLib
    {
        [ScriptMethod("Environment" + "." + nameof(MachineName), "",
            "RS=>EnvironmentLib_MachineName"//"マシン名：\nマシン名を参照します。"
            )]
        public static string MachineName()
        {
            return Environment.MachineName;
        }

        //------------------------------------------------------------------
        [ScriptMethod("Environment" + "." + nameof(UserName), "",
            "RS=>EnvironmentLib_UserName"//"ユーザー名：\nユーザー名を参照します。"
            )]
        public static string UserName()
        {
            return Environment.UserName;
        }

        //------------------------------------------------------------------
        [ScriptMethod("Environment" + "." + nameof(GetEnvironmentVariable), "",
            "RS=>EnvironmentLib_GetEnvironmentVariable"//"環境変数参照：\n<name>環境変数を参照します。"
            )]
        public static string GetEnvironmentVariable(string name)
        {
            return Environment.GetEnvironmentVariable(name);
        }

        //------------------------------------------------------------------
        [ScriptMethod("Environment" + "." + nameof(ReplaceEnvironmentVariable), "",
            "RS=>EnvironmentLib_ReplaceEnvironmentVariable"//"環境変数置換：\n文字列中の %環境変数% を環境変数に置換します。"
            )]
        public static string ReplaceEnvironmentVariable(string name)
        {
            string ret = name;
            MatchCollection matche = Regex.Matches(name, "%(.+?)%");
            foreach (Match m in matche)
            {
                string rep = Environment.GetEnvironmentVariable(m.Groups[1].Value);
                if (rep != null)
                {
                    ret = ret.Replace(m.Groups[0].Value, rep);
                }
            }
            return ret;
        }

        //------------------------------------------------------------------
        [ScriptMethod("Input/Output" + "." + nameof(CommandLineArgs), "",
            "RS=>EnvironmentLib_CommandLineArgs"//"コマンドライン引数：\nコマンドライン引数を参照します。"
            )]
        public static List<string> CommandLineArgs()
        {
            return new List<string>(Environment.GetCommandLineArgs());
        }

        //------------------------------------------------------------------
        [ScriptMethod("Input/Output" + "." + nameof(CommandLineParam), "")]
        public static List<string> CommandLineParam()
        {
            List<string> arg = CommandLineArgs();
            List<string> param = new List<string>();
            int skipCount = 1;
            foreach (var value in arg)
            {
                if (skipCount-- != 0)
                {
                    continue;
                }
                if (!value.StartsWith("-"))
                {
                    param.Add(value);
                }
            }
            return param;
        }

        //------------------------------------------------------------------
        [ScriptMethod("Input/Output" + "." + nameof(CommandLineOption), "")]
        public static List<string> CommandLineOption()
        {
            List<string> arg = CommandLineArgs();
            List<string> option = new List<string>();
            foreach (var value in arg)
            {
                if (value.StartsWith("-"))
                {
                    option.Add(value);
                }
            }
            return option;
        }
    }
}
