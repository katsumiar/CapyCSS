using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;

namespace CapybaraVS
{
    class ToolExec
    {
        public static List<Process> Processes = null;

        public List<string> ParamData = new List<string>();
        public string FileName { get; set; } = null;

        public ToolExec(string fileName)
        {
            FileName = fileName;
        }

        public static void KillProcess()
        {
            if (Processes is null)
                return;

            foreach (var proc in Processes)
            {
                if (!proc.HasExited)
                {
                    proc.Kill();
                }
                proc.Close();
            }
            Processes = null;
        }

        public int Start(string logName, bool redirect = false)
        {
            string execOption = null;
            foreach (var node in ParamData)
            {
                execOption ??= new string("");
                if (execOption != "")
                    execOption += " ";
                execOption += node;
            }

            ProcessStartInfo psInfo = new ProcessStartInfo();
            psInfo.FileName = FileName;
            if (execOption != null)
                psInfo.Arguments = execOption;
            psInfo.CreateNoWindow = true;                   // コンソールウィンドウを開かない
            psInfo.UseShellExecute = !redirect;             // シェル機能を使用しない
            psInfo.RedirectStandardOutput = redirect;       // 標準出力をリダイレクト

            Process p = Process.Start(psInfo);              // アプリの実行開始

            if (redirect)
            {
                p.WaitForExit();

                string output = p.StandardOutput.ReadToEnd();   // 標準出力の読み取り

                // 出力
                MainWindow.Instance.MainLog.OutString(logName, output);

                return p.ExitCode;
            }

            Processes ??= new List<Process>();
            Processes.Add(p);
            return 0;
        }
    }
}
