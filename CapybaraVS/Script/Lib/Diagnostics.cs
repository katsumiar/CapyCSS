using System;
using System.Collections.Generic;
using System.Text;

namespace CapyCSS.Script.Lib
{
    [ScriptClass("Diagnostics")]
    public static class DiagnosticsLib
    {
        public static ICollection<string> GetListOfRunningProcesses(bool distinct = false)
        {
            ICollection<string> processNameList = new List<string>();
            System.Diagnostics.Process[] ps =
                System.Diagnostics.Process.GetProcesses();
            foreach (System.Diagnostics.Process p in ps)
            {
                try
                {
                    processNameList.Add(p.ProcessName);
                }
                catch (Exception)
                {
                    // エラーを残す方法を考える
                }
            }
            if (distinct)
            {
                return ListFactory.Distinct(processNameList);
            }
            return processNameList;
        }
    }
}
