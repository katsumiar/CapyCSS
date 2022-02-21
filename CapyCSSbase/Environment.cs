using CapyCSSattribute;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace CapyCSSbase
{
    [ScriptClass]
    public static class EnvironmentLib
    {
        private const string LIB_NAME = "Environment";
        public const string LIB_IO_NAME = "Input/Output";

        [ScriptMethod(path: LIB_NAME)]
        public static string MachineName()
        {
            return Environment.MachineName;
        }

        //------------------------------------------------------------------
        [ScriptMethod(path: LIB_NAME)]
        public static string UserName()
        {
            return Environment.UserName;
        }

        //------------------------------------------------------------------
        [ScriptMethod(path: LIB_NAME)]
        public static string GetEnvironmentVariable(string name)
        {
            return Environment.GetEnvironmentVariable(name);
        }

        //------------------------------------------------------------------
        [ScriptMethod(path: LIB_NAME)]
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
        [ScriptMethod(path: LIB_IO_NAME)]
        public static ICollection<string> CommandLineArgs()
        {
            return new List<string>(Environment.GetCommandLineArgs());
        }

        //------------------------------------------------------------------
        [ScriptMethod(path: LIB_IO_NAME)]
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
        [ScriptMethod(path: LIB_IO_NAME)]
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
