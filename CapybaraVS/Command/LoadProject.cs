using CapyCSS.Controls;
using CapyCSS.Controls.BaseControls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CapyCSS.Command
{
    /// <summary>
    /// プロジェクトを読み込みます。
    /// </summary>
    internal class LoadProject
        : IMenuCommand
    {
        public static IMenuCommand Create()
        {
            return new LoadProject();
        }

        public static bool TryExecute(object parameter = null)
        {
            var self = Create();
            if (self.CanExecute(parameter))
            {
                self.Execute(parameter);
                return true;
            }
            return false;
        }

        public TreeMenuNode.NodeType NodeType => TreeMenuNode.NodeType.DEFULT_COMMAND;
        public string Name => _Name;
        public static string _Name => "Load Project";
        public Func<string> HintText => () => _Name;

        /// <summary>
        /// 実行可能かの変化を通知します。
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter)
        {
            var self = CommandCanvasList.Instance;
            if (self is null)
            {
                return false;
            }
            return self.IsCommandMask;
        }

        public void Execute(object parameter)
        {
            ProjectControl.Instance.LoadProject();
            CommandCanvasList.Instance?.CurrentScriptCanvas?.CloseCommandWindow();
        }
    }
}
