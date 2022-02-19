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
    public class DummyArgumentsControl
    {
        /// <summary>
        /// 仮引数の不正フラグ
        /// ※仮引数が有効（登録されたら）なら false となっている。
        /// </summary>
        bool argumentInvalid = true;
        /// <summary>
        /// 仮引数値（リファレンス）
        /// </summary>
        CbFuncArguments argumentRef = null;
        MultiRootConnector owner = null;

        public DummyArgumentsControl(MultiRootConnector owner)
        {
            this.owner = owner;
            argumentInvalid = false;
            argumentRef = null;
            List<ICbValue> convList = new List<ICbValue>();
            for (int i = 0; i < Enum.GetNames(typeof(CbFuncArguments.INDEX)).Length; ++i)
            {
                convList.Add(null);
            }
            argumentRef = new CbFuncArguments(convList);
            owner.PreFunction = () => new DummyArgumentsStack.Node(() => argumentInvalid, argumentRef);
        }

        /// <summary>
        /// 仮引数に引数を登録し、有効化します。
        /// ※Invokeメソッド用です。
        /// </summary>
        /// <typeparam name="T">登録する型</typeparam>
        /// <param name="dummyArgumentstack">仮引数スタック</param>
        /// <param name="argument">引数</param>
        public void EnableCbValue(DummyArgumentsStack dummyArgumentstack, ICbValue argument)
        {
            argumentInvalid = false;
            argumentRef[CbFuncArguments.INDEX.ARG_1] = argument;
            dummyArgumentstack.Push(owner.PreFunction());
        }

        /// <summary>
        /// 仮引数に引数を登録し、有効化します。
        /// </summary>
        /// <typeparam name="T">登録する型</typeparam>
        /// <param name="dummyArgumentstack">仮引数スタック</param>
        /// <param name="argument">引数</param>
        public void Enable(DummyArgumentsStack dummyArgumentstack, params object[] argument)
        {
            argumentInvalid = false;
            for (int i = 0; i < argument.Length; ++i)
            {
                argumentRef[(CbFuncArguments.INDEX)i] ??= CbObject.Create();
                argumentRef[(CbFuncArguments.INDEX)i].Data = argument[i];
            }
            dummyArgumentstack.Push(owner.PreFunction());
        }

        /// <summary>
        /// 仮引数を無効化します。
        /// </summary>
        /// <param name="dummyArgumentstack">仮引数スタック</param>
        public void Invalidated(DummyArgumentsStack dummyArgumentstack)
        {
            dummyArgumentstack.Pop();
            argumentInvalid = true;
        }
    }
}
