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
    /// スクリプトを追加します。
    /// </summary>
    internal class AddNewScript
        : IMenuCommand
    {
        public static IMenuCommand Create()
        {
            return new AddNewScript();
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
        public static string _Name => "New Script";
        public Func<string> HintText => () => "New Script(Ctrl+N)";

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
            var addNewCbsFile = AddNewCbsFile.Create();
            if (addNewCbsFile.CanExecute(null))
            {
                return true;
            }
            return self.IsCommandMask;
        }

        public void Execute(object parameter)
        {
            var self = CommandCanvasList.Instance;
            if (self != null)
            {
                if (!Command.AddNewCbsFile.TryExecute())
                {
                    // プロジェクトが開かれていないので、プロジェクト外でスクリプトを追加

                    self.AddNewContents();
                }

                self.CurrentScriptCanvas?.CloseCommandWindow();
            }
        }
    }
}
