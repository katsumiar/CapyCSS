using CapyCSS.Controls;
using CapyCSS.Controls.BaseControls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace CapyCSS.Command
{
    internal class ShowCommandMenu
        : IMenuCommand
    {
        public static IMenuCommand Create()
        {
            return new ShowCommandMenu();
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
        public static string _Name => "Show Command Menu";
        public Func<string> HintText => () => $"{_Name}(Ctrl+Space)";

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
            return !self.IsEmptyScriptCanvas && self.IsScriptRunningMask;
        }

        public void Execute(object parameter)
        {
            var self = CommandCanvasList.Instance;
            if (self != null)
            {
                if (parameter is Point point)
                {
                    self.CurrentScriptCanvas?.ShowCommandMenu(point);
                }
                else
                {
                    self.CurrentScriptCanvas?.ShowCommandMenu(new Point());
                }
                self.CurrentScriptCanvas?.CloseCommandWindow();
            }
        }
    }
}
