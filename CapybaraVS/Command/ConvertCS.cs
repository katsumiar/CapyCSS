﻿using CapyCSS.Controls;
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
    /// スクリプトをc#言語に変換します。
    /// </summary>
    internal class ConvertCS
        : IMenuCommand
    {
        public static IMenuCommand Create()
        {
            return new ConvertCS();
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
        public static string _Name => "Convert to C#";
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
            return self.IsEntryPointsContainsCurrentScriptCanvas() && self.IsScriptRunningMask;
        }

        public void Execute(object parameter)
        {
            var self = CommandCanvasList.Instance;
            if (self != null)
            {
                self.BuildScriptAndOut();
                self.CurrentScriptCanvas?.CloseCommandWindow();
            }
        }
    }
}