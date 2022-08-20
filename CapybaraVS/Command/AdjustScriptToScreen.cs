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
    /// スクリプトが画面に収まるように表示位置とスケールを調整します。
    /// </summary>
    internal class AdjustScriptToScreen
        : IMenuCommand
    {
        public static IMenuCommand Create()
        {
            return new AdjustScriptToScreen();
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
        public static string _Name => "Adjust Script to screen";
        public Func<string> HintText => () => $"{_Name}(J)";

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
            return !self.IsEmptyScriptCanvas && self.IsCommandMask;
        }

        public void Execute(object parameter)
        {
            var self = CommandCanvasList.Instance;
            if (self != null)
            {
                self.CurrentWorkCanvas?.AdjustScriptToScreen();
                self.CurrentScriptCanvas?.CloseCommandWindow();
            }
        }
    }
}
