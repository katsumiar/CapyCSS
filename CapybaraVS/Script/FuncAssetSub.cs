using CapyCSS.Controls;
using CbVS.Script;
using System;
using System.Collections.Generic;
using System.Text;
using static CapyCSS.Controls.BaseControls.CommandCanvas;
using static CapyCSS.Controls.MultiRootConnector;
using static CapyCSS.Script.ScriptImplement;

namespace CapyCSS.Script
{
    /// <summary>
    /// アセット実装登録インターフェイス
    /// </summary>
    public interface IFuncAssetDef
    {
        /// <summary>
        /// アセット型識別コード
        /// </summary>
        string AssetCode { get; }

        /// <summary>
        /// 説明
        /// </summary>
        string HelpText { get; }

        /// <summary>
        /// アセット処理を実装する
        /// </summary>
        /// <param name="col">スクリプトのルートノード</param>
        /// <param name="isReBuildMode">再構築か？（保存データからの復帰）</param>
        bool ImplAsset(MultiRootConnector col, bool isReBuildMode = false);
    }

    /// <summary>
    /// 最もシンプルなファンクションアセット
    /// </summary>
    public interface IFuncCreateAssetDef : IFuncAssetDef
    {
        /// <summary>
        /// アセットツリー上の名前
        /// </summary>
        string MenuTitle { get; }
    }

    /// <summary>
    /// 変数対応型ファンクションアセット
    /// </summary>
    public interface IFuncCreateVariableAssetDef : IFuncAssetDef
    {
        /// <summary>
        /// アセットツリー上の名前
        /// </summary>
        string MenuTitle { get; }

        /// <summary>
        /// 型選択リストの受け入れ項目
        /// ※必要がないなら null にする
        /// </summary>
        List<TypeRequest> typeRequests { get; }
    }

    /// <summary>
    /// 配列変数参照型ファンクションアセット
    /// ※対象変数選択リストに配列変数のみピックアップされる
    /// </summary>
    public interface IFuncCreateVariableListAssetDef : IFuncAssetDef
    {
        /// <summary>
        /// アセットツリー上の名前
        /// </summary>
        string MenuTitle { get; }

        /// <summary>
        /// 型選択リストの受け入れ項目
        /// ※必要がないなら null にする
        /// </summary>
        List<TypeRequest> typeRequests { get; }
    }

    /// <summary>
    /// 引数対応型ファンクションアセット
    /// </summary>
    public interface IFuncAssetWithArgumentDef : IFuncAssetDef
    {
        /// <summary>
        /// アセットツリー上の名前
        /// </summary>
        string MenuTitle { get; }


        /// <summary>
        /// 型選択リストの受け入れ項目
        /// ※必要がないなら null にする
        /// </summary>
        List<TypeRequest> typeRequests { get; }
    }

    /// <summary>
    /// リテラル定義側ファンクションアセット
    /// </summary>
    public interface IFuncAssetLiteralDef
    {
        /// <summary>
        /// 説明
        /// </summary>
        string HelpText { get; }

        /// <summary>
        /// アセットツリー上の名前
        /// </summary>
        string MenuTitle { get; }

        /// <summary>
        /// 型選択リストの受け入れ項目
        /// ※必要がないなら null にする
        /// </summary>
        List<TypeRequest> typeRequests { get; }
    }

    /// <summary>
    /// c#コード化用スクリプト情報
    /// </summary>
    public interface IBuildScriptInfo
    {
        string FuncCode { get; }
        Type ClassType { get; }
        /// <summary>
        /// コンストラクターか？
        /// </summary>
        bool IsConstructor { get; }
        /// <summary>
        /// プロパティのゲッターもしくはセッターか？
        /// </summary>
        bool IsProperty { get; }
        /// <summary>
        /// 引数情報
        /// </summary>
        List<ArgumentInfoNode> ArgumentTypeList { get; }
    }

    public class FuncAssetSub
    {
        /// <summary>
        /// 乱数発生器
        /// </summary>
        public static Random random = new System.Random();

        protected void TryArgListProc(ICbValue variable, Action<ICbValue> func)
        {
            if (func is null)
                return;

            var argList = variable.GetListValue.Value;
            foreach (var node in argList)
            {
                if (node.IsError)
                    throw new Exception(node.ErrorMessage);

                func?.Invoke(node);
            }
        }

        protected void TryArgListCancelableProc(ICbValue variable, Func<ICbValue, bool> func)
        {
            if (func is null)
                return;

            var argList = variable.GetListValue.Value;
            foreach (var node in argList)
            {
                if (node.IsError)
                    throw new Exception(node.ErrorMessage);

                if (func.Invoke(node))
                    break;
            }
        }

        /// <summary>
        /// コールバックを呼び出します。
        /// </summary>
        /// <param name="variable">変数</param>
        /// <param name="dummyArgumentstack">仮引数スタック</param>
        /// <returns>コールバックの返し値</returns>
        protected ICbValue CallEvent(ICbValue variable, DummyArgumentsMemento dummyArgumentstack)
        {
            ICbEvent cbEvent = variable as ICbEvent;
            cbEvent.InvokeCallback(dummyArgumentstack);
            return cbEvent.Value;
        }

        /// <summary>
        /// コールバックかを判断します。
        /// </summary>
        /// <param name="variable">変数</param>
        /// <returns>true = コールバック</returns>
        protected bool CanCallBack(ICbValue variable)
        {
            ICbEvent cbEvent = variable as ICbEvent;
            if (cbEvent is null)
                return false;
            return cbEvent.IsCallback;
        }

        /// <summary>
        /// 引数が異常かをチェックし内容を参照します。
        /// </summary>
        /// <typeparam name="T">引数の型</typeparam>
        /// <param name="variable">引数</param>
        /// <returns>値</returns>
        protected T GetArgument<T>(ICbValue variable)
        {
            CheckArgument(variable);
            return (T)variable.Data;
        }

        /// <summary>
        /// 変数が異常かをチェックします。
        /// </summary>
        /// <param name="variable">変数</param>
        protected void CheckArgument(ICbValue variable)
        {
            if (variable is null)
                throw new Exception("Argument is null.");
            if (variable.IsError)
                throw new Exception(variable.ErrorMessage);
        }

        /// <summary>
        /// 引数を参照します。
        /// </summary>
        /// <typeparam name="T">引数の型</typeparam>
        /// <param name="arguments">引数リスト</param>
        /// <param name="index">引数のインデックス</param>
        /// <returns>値</returns>
        protected T GetArgument<T>(List<ICbValue> arguments, int index)
        {
            return GetArgument<T>(arguments[index]);
        }

        /// <summary>
        /// 引数からリストを参照します。
        /// </summary>
        /// <param name="arguments">引数リスト</param>
        /// <param name="index">引数のインデックス</param>
        /// <returns>配列</returns>
        protected List<ICbValue> GetArgumentList(List<ICbValue> arguments, int index)
        {
            ICbValue valueData = arguments[index];
            CheckArgument(valueData);
            return valueData.GetListValue.Value;
        }

        /// <summary>
        /// コールバックを呼びます。
        /// </summary>
        /// <param name="dummyArguments">仮引数</param>
        /// <param name="func">コールバック変数</param>
        protected void CallCallBack(DummyArgumentsMemento dummyArguments, ICbValue func)
        {
            CallEvent(func, dummyArguments);
        }

        /// <summary>
        /// 可能ならコールバックを呼びます。
        /// </summary>
        /// <param name="dummyArguments">仮引数スタック</param>
        /// <param name="func">コールバック変数</param>
        protected void TryCallCallBack(DummyArgumentsMemento dummyArguments, ICbValue func)
        {
            if (CanCallBack(func))
            {
                CallCallBack(dummyArguments, func);
            }
        }
    }
}
