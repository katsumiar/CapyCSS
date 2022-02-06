using CapyCSS.Controls;
using System;
using System.Collections.Generic;
using System.IO;
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
