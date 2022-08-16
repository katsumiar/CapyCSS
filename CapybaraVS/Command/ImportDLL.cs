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
    internal class ImportDLL
        : IMenuCommand
    {
        public static ImportDLL Create()
        {
            return new ImportDLL();
        }

        public TreeMenuNode.NodeType NodeType => TreeMenuNode.NodeType.DEFULT_COMMAND;
        public string Name => _Name;
        public static string _Name => "Import DLL";
        public Func<string> HintText => null;

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
            return true;
        }

        public void Execute(object parameter)
        {
            ProjectControl.Instance.AddDllFile();
        }
    }
}
