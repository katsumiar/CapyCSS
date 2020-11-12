using System;
using System.Collections.Generic;
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

namespace CapybaraVS
{
    /// <summary>
    /// OutPutLog.xaml の相互作用ロジック
    /// </summary>
    public partial class OutPutLog : UserControl
    {
        static readonly List<string> consolOutTagList = new List<string>() 
        {
            // コンソール出力対象のタグ名リスト

            nameof(Script.ConsoleOut),
            nameof(ToolExec)
        };

        public OutPutLog()
        {
            InitializeComponent();
        }

        public void OutString(string tag, string msg)
        {
            // tag は将来的にタグの作成に使われる

            // スクロールを最終行へ移動
            Log.Focus();
            Log.CaretIndex = Log.Text.Length;
            Log.ScrollToEnd();

            Log.AppendText(msg);
            ImmediateReflection();

            if (consolOutTagList.Contains(tag))
            {
                // コンソールからの起動であればコンソールにも出力する

                Console.Write(msg);
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
                Log.Clear();
                ImmediateReflection();
            }
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
    }
}
