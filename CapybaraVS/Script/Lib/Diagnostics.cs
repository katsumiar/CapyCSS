using System;
using System.Collections.Generic;
using System.Text;

namespace CapybaraVS.Script.Lib
{
    public class DiagnosticsLib
    {
        [ScriptMethod("Diagnostics" + "." + nameof(GetListOfRunningProcesses), "",
            "RS=>DiagnosticsLib_GetListOfRunningProcesses"//"実行中のプロセス名一覧を参照します。\n<distinct> を True にすると同名をまとめます。"
            )]
        public static List<string> GetListOfRunningProcesses(bool distinct = false)
        {
            List<string> processNameList = new List<string>();
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
