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
    /// スクリプトファイルを読み込みます。
    /// </summary>
    internal class LoadScript
        : IMenuCommand
    {
        public static IMenuCommand Create()
        {
            return new LoadScript();
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
        public static string _Name => "Load Script";
        public Func<string> HintText => () => $"{_Name}(Ctrl+O)";

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
            if (self is null || !self.IsCommandMask)
            {
                return false;
            }
            var addCbsFile = AddCbsFile.Create();
            if (addCbsFile.CanExecute(null))
            {
                return true;
            }
            return !self.IsEmptyScriptCanvas;
        }

        public void Execute(object parameter)
        {
            var self = CommandCanvasList.Instance;
            if (self != null)
            {
                if (!Command.AddCbsFile.TryExecute())
                {
                    // プロジェクトが開かれていないので、プロジェクト外でスクリプトを読み込む

                    self.LoadCbsFile();
                }
                self.CurrentScriptCanvas?.CloseCommandWindow();
            }
        }
    }
}
