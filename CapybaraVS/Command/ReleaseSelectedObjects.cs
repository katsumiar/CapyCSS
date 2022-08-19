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
    /// スクリプトの選択状態を解除します。
    /// </summary>
    internal class ReleaseSelectedObjects
        : IMenuCommand
    {
        public static IMenuCommand Create()
        {
            return new ReleaseSelectedObjects();
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
        public static string _Name => "Release Selected Objects";
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
            if (CommandCanvasList.Instance is null)
            {
                return false;
            }
            var self = CommandCanvasList.Instance.CurrentWorkCanvas;
            if (self is null)
            {
                return false;
            }
            return self.IsSelected() && CommandCanvasList.Instance.IsScriptRunningMask;
        }

        public void Execute(object parameter)
        {
            var self = CommandCanvasList.Instance;
            if (self != null)
            {
                self.CurrentWorkCanvas?.ReleaseSelectedObject();
                self.CurrentScriptCanvas?.CloseCommandWindow();
            }
        }
    }
}
