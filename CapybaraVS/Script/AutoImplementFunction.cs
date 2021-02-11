using CapybaraVS.Controls;
using CapybaraVS.Controls.BaseControls;
using CbVS.Script;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using static CapybaraVS.Script.ScriptImplement;

namespace CapybaraVS.Script
{
    /// <summary>
    /// リフレクションによる自動実装用ファンクションアセット定義クラス
    /// </summary>
    public class AutoImplementFunction : FuncAssetSub, IFuncAssetWithArgumentDef
    {
        static public AutoImplementFunction Create(AutoImplementFunctionInfo info)
        {
            AutoImplementFunction ret = new AutoImplementFunction()
            {
                AssetCode = info.assetCode,
                FuncCode = funcCodeFilter(info),
                hint = info.hint,
                nodeHint = info.nodeHint,
                MenuTitle = info.menuTitle,
                FuncTitle = info.funcTitle,
                ClassType = info.classType,
                ReturnType = info.returnType,
                ArgumentTypeList = info.argumentTypeList,
                DllModule = info.dllModule,
                IsConstructor = info.isConstructor
            };
            return ret;
        }

        public static string funcCodeFilter(AutoImplementFunctionInfo info)
        {
            string funcCode = info.assetCode;
            if (funcCode.Contains("#"))
            {
                string[] sp = funcCode.Split('#');
                funcCode = sp[0];
            }
            if (funcCode.Contains("."))
            {
                string[] sp = funcCode.Split('.');
                funcCode = sp[sp.Length - 1];
            }

            return funcCode;
        }

        public string AssetCode { get; set; } = "";

        public string FuncCode { get; set; } = "";

        protected string hint = "";

        public string HelpText => hint;

        protected string nodeHint = "";

        public string NodeHelpText => nodeHint;

        public string MenuTitle { get; set; } = "";

        public string FuncTitle { get; set; } = "";

        //public CbST TargetType { get; set; } = CbST.Double; // dummy
        public string ValueType { get; } = typeof(double).FullName;    // Dummy

        public CbType[] DeleteSelectItems => null;

        public Type ClassType { get; set; } = null;

        public List<ArgumentInfoNode> ArgumentTypeList { get; set; } = null;

        public Func<ICbValue> ReturnType { get; set; } = null;

        /// <summary>
        /// モジュール（DLL）
        /// </summary>
        public Module DllModule = null;

        /// <summary>
        /// コンストラクターか？
        /// </summary>
        public bool IsConstructor = false;

        public virtual bool ImplAsset(MultiRootConnector col, bool noThreadMode = false)
        {
            return ImplAsset(col, noThreadMode, null);
        }

        protected bool ImplAsset(MultiRootConnector col, bool noThreadMode = false, DummyArgumentsControl dummyArgumentsControl = null)
        {
            List<ICbValue> argumentTypeList = new List<ICbValue>();
            List<int> variableIdList = new List<int>();
            VariableGetter variableGetter = null;

            if (ArgumentTypeList != null)
            {
                // 引数に変数IDをアサインする

                if (noThreadMode)
                {
                    variableGetter = AssignedVariableArgumentForNoThread(col, argumentTypeList, variableIdList, variableGetter);
                }
                else
                {
                    foreach (var node in ArgumentTypeList)
                    {
                        if (node.IsByRef)
                        {
                            ListSelectWindow.DefaultValue = node.CreateArgument();
                            StackNode stackNode = ListSelectWindow.Create(
                                col.OwnerCommandCanvas,
                                "Variable",
                                col.OwnerCommandCanvas.ScriptWorkStack.StackData,
                                false,
                                new Point(Mouse.GetPosition(null).X, Mouse.GetPosition(null).Y));

                            if (stackNode is null)
                                return false;

                            col.AddAttachVariableId(stackNode.Id);
                            variableGetter = CreateVariableGetter(col, FuncTitle, col.GetAttachVariableIdsCount() - 1);
                            if (variableGetter.IsError)
                            {
                                variableIdList.Add(-1);
                                return false;
                            }

                            variableIdList.Add(variableGetter.Id);

                            FuncTitle = variableGetter.MakeName;
                            continue;
                        }
                        variableIdList.Add(-1);
                        argumentTypeList.Add(node.CreateArgument());
                    }
                }
            }

            col.MakeFunction(
                FuncTitle,
                NodeHelpText,
                ReturnType,
                argumentTypeList,
                new Func<List<ICbValue>, DummyArgumentsStack, ICbValue>(
                    (argument, cagt) =>
                    {
                        var ret = ReturnType();
                        if (dummyArgumentsControl != null && dummyArgumentsControl.IsInvalid(cagt))
                            return ret; // 実行環境が有効でない

                        ImplCallMethod(col, dummyArgumentsControl, argument, cagt, variableIdList, ret);
                        return ret;
                    }
                )
            );

            return true;
        }

        /// <summary>
        /// メソッド呼び出しの実装です。
        /// </summary>
        /// <param name="col"></param>
        /// <param name="dummyArgumentsControl">仮引数管理オブジェクト</param>
        /// <param name="callArguments">引数リスト</param>
        /// <param name="dummyArgumentsStack">仮引数スタック</param>
        /// <param name="variableIds">スクリプト変数IDリスト</param>
        /// <param name="returnValue">返り値</param>
        private void ImplCallMethod(
            MultiRootConnector col, 
            DummyArgumentsControl dummyArgumentsControl, 
            List<ICbValue> callArguments, 
            DummyArgumentsStack dummyArgumentsStack, 
            List<int> variableIds, 
            ICbValue returnValue
            )
        {
            try
            {
                bool isClassInstanceMethod = ArgumentTypeList != null && ArgumentTypeList[0].IsSelf && !IsConstructor;
                List<object> methodArguments = null;

                methodArguments = SetArguments(
                    col,
                    dummyArgumentsControl,
                    callArguments,
                    dummyArgumentsStack,
                    variableIds,
                    isClassInstanceMethod,
                    methodArguments
                    );

                object result = CallMethod(
                    col,
                    callArguments,
                    variableIds,
                    isClassInstanceMethod, 
                    methodArguments);

                ProcReturnValue(col, returnValue, result);
            }
            catch (Exception ex)
            {
                col.ExceptionFunc(returnValue, ex);
            }
        }

        /// <summary>
        /// 引数に変数IDをアサインします。
        /// </summary>
        /// <param name="col"></param>
        /// <param name="argumentTypeList"></param>
        /// <param name="variableIds">スクリプト変数IDリスト</param>
        /// <param name="variableGetter">スクリプト変数参照オブジェクト</param>
        /// <returns>スクリプト変数参照オブジェクト</returns>
        private VariableGetter AssignedVariableArgumentForNoThread(
            MultiRootConnector col, 
            List<ICbValue> argumentTypeList, 
            List<int> variableIds, 
            VariableGetter variableGetter
            )
        {
            int variableIdIndex = 0;
            foreach (var node in ArgumentTypeList)
            {
                if (node.IsByRef)
                {
                    variableGetter = CreateVariableGetter(col, FuncTitle, variableIdIndex++);
                    variableIds.Add(variableGetter.Id);
                    MenuTitle = variableGetter.MakeName;
                    continue;
                }
                variableIds.Add(-1);
                argumentTypeList.Add(node.CreateArgument());
            }

            return variableGetter;
        }

        /// <summary>
        /// 引数をメソッドに合わせて調整しリストアップします。
        /// </summary>
        /// <param name="col"></param>
        /// <param name="dummyArgumentsControl">仮引数管理オブジェクト</param>
        /// <param name="callArguments">引数リスト</param>
        /// <param name="dummyArgumentsStack">仮引数スタック</param>
        /// <param name="variableIds">スクリプト変数IDリスト</param>
        /// <param name="isClassInstanceMethod">インスタンスメソッドか？</param>
        /// <param name="methodArguments">メソッド呼び出し引数リスト</param>
        /// <returns>メソッド呼び出し引数リスト</returns>
        private List<object> SetArguments(
            MultiRootConnector col, 
            DummyArgumentsControl dummyArgumentsControl, 
            List<ICbValue> callArguments, 
            DummyArgumentsStack dummyArgumentsStack, 
            List<int> variableIds, 
            bool isClassInstanceMethod,
            List<object> methodArguments
            )
        {
            int argumentIndex = 0;
            for (int i = 0; i < variableIds.Count; ++i)
            {
                methodArguments ??= new List<object>();
                if (variableIds[i] != -1)
                {
                    // アサインされている変数の値を引数用に取り込む

                    ICbValue cbVSValue = col.OwnerCommandCanvas.ScriptWorkStack.Find(variableIds[i]);
                    methodArguments.Add(cbVSValue.Data);
                    continue;
                }

                var node = callArguments[argumentIndex++];
                CheckArgument(node);

                if (i == 0 && isClassInstanceMethod)
                {
                    // クラスメソッドの第一引数は、self（this）を受け取るのでメソッド呼び出しの引数にしない

                    continue;
                }

                if (node is ICbList cbList)
                {
                    if (dummyArgumentsControl is null)
                    {
                        methodArguments.Add(cbList.ConvertOriginalTypeList(col, dummyArgumentsStack));
                    }
                    else
                    {
                        methodArguments.Add(cbList.ConvertOriginalTypeList(dummyArgumentsControl, dummyArgumentsStack));
                    }
                }
                else if (dummyArgumentsControl != null && node is ICbEvent cbEvent)
                {
                    if (cbEvent.CallBack is null)
                    {
                        methodArguments.Add(null);
                    }
                    else
                    {
                        Debug.Assert(dummyArgumentsControl != null);
                        methodArguments.Add(cbEvent.GetCallBackOriginalType(dummyArgumentsControl, dummyArgumentsStack));
                    }
                }
                else methodArguments.Add(node.Data);
            }

            return methodArguments;
        }

        /// <summary>
        /// メソッドを実行します。
        /// </summary>
        /// <param name="callArguments">引数リスト</param>
        /// <param name="variableIds">スクリプト変数IDリスト</param>
        /// <param name="isClassInstanceMethod">インスタンスメソッドか？</param>
        /// <param name="methodArguments">メソッド呼び出し引数リスト</param>
        /// <returns>メソッドの返り値</returns>
        private object CallMethod(
            MultiRootConnector col,
            List<ICbValue> callArguments, 
            List<int> variableIds, 
            bool isClassInstanceMethod, 
            List<object> methodArguments
            )
        {
            object classInstance = null;

            if (isClassInstanceMethod)
            {
                // クラスメソッドの第一引数は、self（this）を受け取るのでクラスインスタンスとして扱う

                classInstance = callArguments[0].Data;
                if (classInstance is null)
                {
                    throw new Exception($"self(this) is invalid.");
                }
            }

            object result = null;
            if (IsConstructor)
            {
                // new されたコンストラクタとして振る舞う

                if (methodArguments is null)
                {
                    result = Activator.CreateInstance(ClassType);
                }
                else
                {
                    object[] args = methodArguments.ToArray();
                    result = Activator.CreateInstance(ClassType, args);
                }
            }
            else if (methodArguments is null)
            {
                // 引数のないメソッド

                result = ClassType.InvokeMember(FuncCode, BindingFlags.InvokeMethod,
                                null, classInstance, new object[] { });
            }
            else
            {
                // 引数ありのメソッド

                object[] args = methodArguments.ToArray();

                result = ClassType.InvokeMember(FuncCode, BindingFlags.InvokeMethod,
                                null, classInstance, args);

                for (int i = 0; i < variableIds.Count; ++i)
                {
                    if (variableIds[i] != -1)
                    {
                        // リファレンス引数のための結果の取り込み

                        ICbValue cbVSValue = col.OwnerCommandCanvas.ScriptWorkStack.Find(variableIds[i]);
                        cbVSValue.Data = args[i];
                        col.OwnerCommandCanvas.ScriptWorkStack.UpdateValueData(variableIds[i]);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// 返り値を処理します。
        /// </summary>
        /// <param name="col"></param>
        /// <param name="returnValue"></param>
        /// <param name="result"></param>
        private static void ProcReturnValue(MultiRootConnector col, ICbValue returnValue, object result)
        {
            if (returnValue is ICbList retCbList)
            {
                retCbList.CopyFrom(result);
                col.LinkConnectorControl.UpdateValueData();
            }
            else if (result != null && returnValue is ICbEvent cbEvent)
            {
                // Func<> or Action<> 型の返し値

                if (CbFunc.IsFuncType(result.GetType()))
                {
                    cbEvent.CallBack = (cagt) =>
                    {
                        ICbValue retTypeValue = null;
                        try
                        {
                            // イベントからの返り値を取得
                            object tempReturnValue = ((dynamic)result).Invoke(cagt.GetValue().Data);

                            // 直接メソッドを呼ぶため帰ってくるのは通常の値なので Cb タイプに変換する
                            retTypeValue = CbST.CbCreate(tempReturnValue.GetType());
                            retTypeValue.Data = tempReturnValue;
                        }
                        catch (Exception ex)
                        {
                            col.ExceptionFunc(null, ex);
                        }
                        return retTypeValue;
                    };
                }
                else if (CbFunc.IsActionType(result.GetType()))
                {
                    // Action 型だと考える

                    cbEvent.CallBack = (cagt) =>
                    {
                        try
                        {
                            var argType = cagt.GetValue();
                            if (argType is null)
                                ((dynamic)result).Invoke(null);
                            else
                                ((dynamic)result).Invoke(argType.Data);
                        }
                        catch (Exception ex)
                        {
                            col.ExceptionFunc(null, ex);
                        }
                        return new CbVoid();
                    };
                }
                else
                    throw new NotImplementedException();
            }
            else
                returnValue.Data = result;
        }

        public static VariableGetter CreateVariableGetter(MultiRootConnector col, string funcTitle, int index)
        {
            return new VariableGetter(
                    col,
                    (name) =>
                    {
                        string title = "";
                        try
                        {
                            title = string.Format(funcTitle, name);
                        }
                        catch (Exception)
                        {
                            title = "** Name Error **";
                        }
                        return title;
                    },
                    index
                    );
        }
    }
}
