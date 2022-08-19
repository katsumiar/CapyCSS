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
    /// プロジェクトにDLLを取り込みます。
    /// </summary>
    internal class ImportDLL
        : IMenuCommand
    {
        public static IMenuCommand Create()
        {
            return new ImportDLL();
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
        public static string _Name => "Import DLL";
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
            if (ProjectControl.Instance is null || CommandCanvasList.Instance is null)
            {
                return false;
            }
            return ProjectControl.Instance.IsOpenProject && CommandCanvasList.Instance.IsScriptRunningMask;
        }

        public void Execute(object parameter)
        {
            ProjectControl.Instance.AddDllFile();
            CommandCanvasList.Instance?.CurrentScriptCanvas?.CloseCommandWindow();
        }
    }
}
