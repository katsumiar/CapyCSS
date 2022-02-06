using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace CapyCSS
{
    public class RelayCommand : ICommand
    {
        // Command実行時に実行するアクション、引数を受け取りたい場合はこのActionをAction<object>などにする
        private Action<object> _action;

        public RelayCommand(Action<object> action)
        {
            // コンストラクタでActionを登録
            _action = action;
        }

        #region ICommandインターフェースの必須実装

        //-----------------------------------------------------------------------------------
        /// <summary>
        /// 
        /// </summary>
#pragma warning disable 67  // インターフェイスにより強制されるが現在未使用
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            // とりあえずActionがあれば実行可能
            return _action != null;
        }

        public void Execute(object parameter)
        {
            // 今回は引数を使わずActionを実行
            _action?.Invoke(parameter);
        }

        #endregion
    }
}
