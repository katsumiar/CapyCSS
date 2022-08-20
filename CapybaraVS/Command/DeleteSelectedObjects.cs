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
    internal class DeleteSelectedObjects
        : IMenuCommand
    {
        public static IMenuCommand Create()
        {
            return new DeleteSelectedObjects();
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
        public static string _Name => "Delete Selected Objects";
        public Func<string> HintText => () => $"{_Name}(DEL)";

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
            if (self is null || !self.IsCommandMask || self.CurrentWorkCanvas is null)
            {
                return false;
            }
            return self.CurrentWorkCanvas.IsSelected();
        }

        public void Execute(object parameter)
        {
            var self = CommandCanvasList.Instance;
            if (self != null)
            {
                self.CurrentWorkCanvas?.DeleteSelectedObjects();
                self.CurrentScriptCanvas?.CloseCommandWindow();
            }
        }
    }
}
