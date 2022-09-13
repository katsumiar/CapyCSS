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
    /// CBSファイル用の.NETプロジェクトを作成します。
    /// </summary>
    internal class CreateCsProject
        : IMenuCommand
    {
        public static IMenuCommand Create()
        {
            return new CreateCsProject();
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
        public static string _Name => "Make .NET Project";
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
            var canvas = CommandCanvasList.Instance.CurrentScriptCanvas;
            if (self is null || !self.IsCommandMask || canvas is null)
            {
                return false;
            }
            return canvas.OpenFileName != "" && self.IsEntryPointsContainsCurrentScriptCanvas();
        }

        public void Execute(object parameter)
        {
            CommandCanvasList.Instance?.CreateCsProject();
            CommandCanvasList.Instance?.CurrentScriptCanvas?.CloseCommandWindow();
        }
    }
}
