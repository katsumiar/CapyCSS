using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace CapybaraVS.Script.Lib
{
    public class EnvironmentLib
    {
        [ScriptMethod("Environment" + "." + nameof(MachineName), "", "RS=>EnvironmentLib_MachineName")]
        public static string MachineName()
        {
            return Environment.MachineName;
        }

        //------------------------------------------------------------------
        [ScriptMethod("Environment" + "." + nameof(UserName), "", "RS=>EnvironmentLib_UserName")]
        public static string UserName()
        {
            return Environment.UserName;
        }

        //------------------------------------------------------------------
        [ScriptMethod("Environment" + "." + nameof(GetEnvironmentVariable), "", "RS=>EnvironmentLib_GetEnvironmentVariable")]
        public static string GetEnvironmentVariable(string name)
        {
            return Environment.GetEnvironmentVariable(name);
        }

        //------------------------------------------------------------------
        [ScriptMethod("Environment" + "." + nameof(ReplaceEnvironmentVariable), "", "RS=>EnvironmentLib_ReplaceEnvironmentVariable")]
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
        [ScriptMethod("Input/Output" + "." + nameof(CommandLineArgs), "", "RS=>EnvironmentLib_CommandLineArgs")]
        public static ICollection<string> CommandLineArgs()
        {
            return new List<string>(Environment.GetCommandLineArgs());
        }

        //------------------------------------------------------------------
        [ScriptMethod("Input/Output" + "." + nameof(CommandLineParam), "", "RS=>EnvironmentLib_CommandLineParam")]
        public static ICollection<string> CommandLineParam()
        {
            ICollection<string> arg = CommandLineArgs();
            var param = new List<string>();
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
        [ScriptMethod("Input/Output" + "." + nameof(CommandLineOption), "", "RS=>EnvironmentLib_CommandLineOption")]
        public static ICollection<string> CommandLineOption()
        {
            ICollection<string> arg = CommandLineArgs();
            var option = new List<string>();
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
