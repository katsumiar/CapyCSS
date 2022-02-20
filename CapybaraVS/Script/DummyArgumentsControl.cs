using CapyCSS.Controls;
using CapyCSS.Script;
using System;
using System.Collections.Generic;
using System.Text;

namespace CbVS.Script
{
    /// <summary>
    /// 仮引数制御クラス
    /// </summary>
    public static class DummyArgumentsControl
    {
        /// <summary>
        /// 仮引数に引数を登録し、有効化します。
        /// </summary>
        /// <typeparam name="T">登録する型</typeparam>
        /// <param name="dummyArguments">仮引数スタック</param>
        /// <param name="argument">引数</param>
        public static void Enable(DummyArgumentsMemento dummyArguments, params object[] argument)
        {
            dummyArguments.CreateAndRegist(argument);
        }

        /// <summary>
        /// 仮引数を無効化します。
        /// </summary>
        /// <param name="dummyArguments">仮引数スタック</param>
        public static void Invalidated(DummyArgumentsMemento dummyArguments)
        {
            dummyArguments.Unregist();
        }
    }
}
