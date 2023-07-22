using CapyCSS.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;

namespace CapyCSS
{
    class ToolExec
    {
        /// <summary>
        /// 呼ばれた実行ファイルのプロセスリストです。
        /// </summary>
        private static List<Process> ProcessList = null;

        /// <summary>
        /// 実行ファイルを呼び出すときの引数リストです。
        /// </summary>
        public List<string> ParamList = new List<string>();

        /// <summary>
        /// 実行ファイルのパスです。
        /// </summary>
        public string ExecPath => _ExecPath;

        private string _ExecPath = null;
        public ToolExec(string path)
        {
            _ExecPath = path;
        }

        /// <summary>
        /// ToolExecによって作られた実行ファイルをすべて殺します。
        /// </summary>
        public static void KillProcess()
        {
            if (ProcessList is null)
                return;

            foreach (var proc in ProcessList)
            {
                if (!proc.HasExited)
                {
                    proc.Kill();
                }
                proc.Close();
            }
            ProcessList = null;
        }

        /// <summary>
        /// ToolExecによって呼ばれたすべての実行ファイルの終了を待ちます。
        /// </summary>
        public static void WaitForExit()
        {
            if (ProcessList is null)
                return;

            foreach (var proc in ProcessList)
            {
                if (!proc.HasExited)
                {
                    proc.WaitForExit();
                }
            }
        }

		/// <summary>
		/// プロセスを実行します。
		/// ※redirectがfalseの場合、実行ファイルを呼び出すと終わりを待たずにすぐに返ります。
		/// </summary>
		/// <param name="redirect">リダイレクトするか？</param>
		/// <returns>呼び出された実行ファイルの終了コード（リダイレクトでない場合は、常に 0）</returns>
		public int Start(bool redirect = false)
		{
			string execOption = null;
			foreach (var node in ParamList)
			{
				execOption ??= new string("");
				if (execOption != "")
					execOption += " ";
				execOption += node;
			}

			ProcessStartInfo psInfo = new ProcessStartInfo();

			// Check if the file is a .lnk file
			if (!ExecPath.EndsWith(".exe"))
			{
				psInfo.FileName = "cmd.exe";
				psInfo.Arguments = $"/C \"{ExecPath}\" {execOption}";
			}
			else
			{
				psInfo.FileName = ExecPath;
				if (execOption != null)
					psInfo.Arguments = execOption;
			}

			psInfo.CreateNoWindow = true;                   // コンソールウィンドウを開かない
			psInfo.UseShellExecute = !redirect;             // シェル機能を使用するか？
			psInfo.RedirectStandardOutput = redirect;       // 標準出力をリダイレクトするか？

			Process p = new Process();
			p.StartInfo = psInfo;
			p.Start();

			if (psInfo.RedirectStandardOutput)
			{
				p.WaitForExit();
				string output = p.StandardOutput.ReadToEnd();   // 標準出力の読み取り
				CommandCanvasList.OutPut.OutString(nameof(ToolExec), output);
				CommandCanvasList.OutPut.Flush();
				return p.ExitCode;
			}

			ProcessList ??= new List<Process>();
			ProcessList.Add(p);
			return 0;
		}
	}
}
