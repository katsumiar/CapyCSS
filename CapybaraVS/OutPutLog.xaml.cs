using CapyCSS.Controls;
using CapyCSS.Script;
using Microsoft.VisualBasic.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace CapyCSS
{
    /// <summary>
    /// OutPutLog.xaml の相互作用ロジック
    /// </summary>
    public partial class OutPutLog : UserControl
    {
        public OutPutLog()
        {
            InitializeComponent();
            SetupTimer();
        }

        public void OutString(string tag, string msg)
        {
            //※ tag は将来的にタグの作成に使われる予定だったが、Console出力に統一したので無くすことにした

            Log.AppendText(msg);
            afterPutTimer.Start();

            if (CommandCanvasList.Instance.IsAutoExecute)
            {
                // コンソールからの起動であればコンソールにも出力する

                SystemTextWriter.StdOut?.Write(msg);
            }
        }

        public void OutLine(string tag, string msg)
        {
            // tag は将来的にタグの作成に使われる

            OutString(tag, msg + Environment.NewLine);
        }

        public void TryAutoClear()
        {
            if ((bool)AutoClear.IsChecked)
            {
                Clear();
                ImmediateReflection();
            }
        }

        public void Clear()
        {
            afterPutTimer?.Stop();
            Log.Clear();
        }

        private void ImmediateReflection()
        {
            DispatcherFrame frame = new DispatcherFrame();
            var callback = new DispatcherOperationCallback(obj =>
                {
                    ((DispatcherFrame)obj).Continue = false;
                    return null;
                }
            );
            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background, callback, frame);
            Dispatcher.PushFrame(frame);
        }

        public void Flush()
        {
            afterPutTimer?.Stop();
            Log.ScrollToEnd();
            ImmediateReflection();
        }

        private DispatcherTimer afterPutTimer = null;
        private void SetupTimer()
        {
            afterPutTimer = new DispatcherTimer();
            afterPutTimer.Interval = new TimeSpan(0, 0, 1);
            afterPutTimer.Tick += (x,e) => { Flush(); };
        }

        private void Log_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                int startPos = Log.SelectionStart;
                int lineCount = Log.Text.Substring(0, startPos).Count((c) => c == '\n');
                string[] lines = Log.Text.Split(Environment.NewLine);

                string currentLine = lines[lineCount].Trim();
                if (currentLine.StartsWith(">"))
                {
                    // コマンドを実行する

                    string command = currentLine.Substring(1, currentLine.Length - 1);
                    if (command == "cls")
                    {
                        // クリアコマンド

                        Log.Text = "";
                    }
                    else
                    {
                        // シェルコマンドを実行する

                        TryAutoClear();
                        CommandShellExecuter(command, Encoding.Default);
                    }
                }
                else
                {
                    // 改行コードを挿入する

                    Log.Text = Log.Text.Insert(startPos, Environment.NewLine);
                    Log.SelectionStart = startPos + 1;
                    Log.Focus();
                }
            }
        }

        public int CommandShellExecuter(string command, Encoding encoding)
        {
            ProcessStartInfo processStartInfo = new ProcessStartInfo(CbSTUtils.Shell, CbSTUtils.ShellOption + command);
            return BaseCommandExecuter(processStartInfo, encoding);
        }

        public int CommandExecuter(string command, Encoding encoding)
        {
            ProcessStartInfo processStartInfo = new ProcessStartInfo(command);
            return BaseCommandExecuter(processStartInfo, encoding);
        }

        private int BaseCommandExecuter(ProcessStartInfo processStartInfo, Encoding encoding)
        {
            processStartInfo.CreateNoWindow = true;
            processStartInfo.UseShellExecute = false;
            processStartInfo.RedirectStandardOutput = true;
            processStartInfo.RedirectStandardError = true;
            processStartInfo.WorkingDirectory = Environment.CurrentDirectory;
            if (encoding != Encoding.Default)
            {
                processStartInfo.StandardErrorEncoding = encoding;
                processStartInfo.StandardOutputEncoding = encoding;
            }
            Process process = Process.Start(processStartInfo);
            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();
            int code = process.ExitCode;
            process.Close();
            Log.Text += Environment.NewLine;

            Log.Text += error;
            Log.Text += output;
            Log.Text += $"Exit Code: {code}" + Environment.NewLine;
            Log.SelectionStart = Log.Text.Length - 1;
            Log.ScrollToEnd();
            return code;
        }
    }

    public class SystemTextWriter
        : TextWriter
    {
        public static TextWriter StdOut = null;
        private const string TAG = "System";
        public override Encoding Encoding => Encoding.UTF8;
        public SystemTextWriter(IFormatProvider formatProvider) : base(formatProvider) { }
        public SystemTextWriter() 
        {
            StdOut = Console.Out;
        }
        public override void Write(string value) => CommandCanvasList.Instance.MainLog.OutString(TAG, value);
        public override void WriteLine(string value) => CommandCanvasList.Instance.MainLog.OutLine(TAG, value);

        public override void Flush()
        {
            base.Flush();
            CommandCanvasList.OutPut.Flush();
        }
    }
}
