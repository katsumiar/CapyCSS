using CapybaraVS.Controls;
using CapybaraVS.Controls.BaseControls;
using CapyCSS.Controls;
using CbVS.Script;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using static CapybaraVS.Controls.MultiRootConnector;

namespace CapybaraVS.Script
{
    /// <summary>
    /// スクリプトの機能をインポートしメニューに登録するクラスです。
    /// </summary>
    public class ApiImporter : ImplementAsset
    {
        private TreeMenuNode ProgramNode = null;
        private TreeMenuNode DllNode = null;
        private TreeMenuNode NuGetNode = null;
        private CommandCanvas OwnerCommandCanvas = null;
        public ObservableCollection<string> ModulueNameList = new ObservableCollection<string>();
        public List<string> DllModulePathList = new List<string>();
        public List<string> PackageModuleList = new List<string>();
        public List<string> ClassModuleList = new List<string>();
        public List<string> NuGetModuleList = new List<string>();

        public ApiImporter(CommandCanvas ownerCommandCanvas)
        {
            OwnerCommandCanvas = ownerCommandCanvas;
            ProgramNode = CreateGroup(ownerCommandCanvas, "Program");

            CreateAssetMenu(ownerCommandCanvas, ProgramNode, new Subroutine());

            {
                var literalNode = CreateGroup(ProgramNode, "Literal");
                CreateAssetMenu(ownerCommandCanvas, literalNode, new LiteralType());
                CreateAssetMenu(ownerCommandCanvas, literalNode, new LiteralListType());
            }

            {
                var variableNode = CreateGroup(ProgramNode, "Variable");
                CreateAssetMenu(ownerCommandCanvas, variableNode, new CreateVariable());
                CreateAssetMenu(ownerCommandCanvas, variableNode, new CreateVariableFunc());
                CreateAssetMenu(ownerCommandCanvas, variableNode, new GetVariable());
                CreateAssetMenu(ownerCommandCanvas, variableNode, new SetVariable());

                {
                    var variableListNode = CreateGroup(variableNode, "Variable List");
                    CreateAssetMenu(ownerCommandCanvas, variableListNode, new CreateVariableList());
                    CreateAssetMenu(ownerCommandCanvas, variableListNode, new GetVariableFromIndex());
                    CreateAssetMenu(ownerCommandCanvas, variableListNode, new SetVariableToIndex());
                    CreateAssetMenu(ownerCommandCanvas, variableListNode, new AppendVariableList());
                }
            }

            {
                var flowOperation = CreateGroup(ProgramNode, "Flow");
                CreateAssetMenu(ownerCommandCanvas, flowOperation, new If());
                CreateAssetMenu(ownerCommandCanvas, flowOperation, new If_Func());
                CreateAssetMenu(ownerCommandCanvas, flowOperation, new If_Action());
                CreateAssetMenu(ownerCommandCanvas, flowOperation, new For());
                CreateAssetMenu(ownerCommandCanvas, flowOperation, new Foreach());
            }

            {
                var listNode = CreateGroup(ProgramNode, "List");
                CreateAssetMenu(ownerCommandCanvas, listNode, new Count());
                CreateAssetMenu(ownerCommandCanvas, listNode, new Contains());
                CreateAssetMenu(ownerCommandCanvas, listNode, new GetListIndex());
                CreateAssetMenu(ownerCommandCanvas, listNode, new GetListLast());
                CreateAssetMenu(ownerCommandCanvas, listNode, new SetListIndex());
                CreateAssetMenu(ownerCommandCanvas, listNode, new Append());
            }

            {
                var funcNode = CreateGroup(ProgramNode, "f(x)");
                CreateAssetMenu(ownerCommandCanvas, funcNode, new CallerArgument());
                CreateAssetMenu(ownerCommandCanvas, funcNode, new CallerArguments());
                CreateAssetMenu(ownerCommandCanvas, funcNode, new Invoke());
                CreateAssetMenu(ownerCommandCanvas, funcNode, new InvokeWithArg());
                CreateAssetMenu(ownerCommandCanvas, funcNode, new InvokeAction());
                CreateAssetMenu(ownerCommandCanvas, funcNode, new InvokeActionWithArg());
            }

            {
                var embeddedNode = CreateGroup(ProgramNode, "Function");

                {
                    var mathNode = CreateGroup(embeddedNode, "Math");
                    CreateAssetMenu(ownerCommandCanvas, mathNode, new Abs());
                    CreateAssetMenu(ownerCommandCanvas, mathNode, new Inc());
                    CreateAssetMenu(ownerCommandCanvas, mathNode, new Dec());
                    CreateAssetMenu(ownerCommandCanvas, mathNode, new Sum());
                    CreateAssetMenu(ownerCommandCanvas, mathNode, new Sum_Func());
                    CreateAssetMenu(ownerCommandCanvas, mathNode, new Sub());
                    CreateAssetMenu(ownerCommandCanvas, mathNode, new Mul());
                    CreateAssetMenu(ownerCommandCanvas, mathNode, new Div());
                    CreateAssetMenu(ownerCommandCanvas, mathNode, new Pow());
                    CreateAssetMenu(ownerCommandCanvas, mathNode, new Mod());
                    CreateAssetMenu(ownerCommandCanvas, mathNode, new Rand());
                }

                {
                    var logicalOperation = CreateGroup(embeddedNode, "Logical operation");
                    CreateAssetMenu(ownerCommandCanvas, logicalOperation, new And());
                    CreateAssetMenu(ownerCommandCanvas, logicalOperation, new Or());
                    CreateAssetMenu(ownerCommandCanvas, logicalOperation, new Not());
                }

                {
                    var comparisonNode = CreateGroup(embeddedNode, "Comparison");
                    CreateAssetMenu(ownerCommandCanvas, comparisonNode, new Eq());
                    CreateAssetMenu(ownerCommandCanvas, comparisonNode, new Gt());
                    CreateAssetMenu(ownerCommandCanvas, comparisonNode, new Ge());
                    CreateAssetMenu(ownerCommandCanvas, comparisonNode, new Lt());
                    CreateAssetMenu(ownerCommandCanvas, comparisonNode, new Le());
                }
            }

            {
                var functionNode = CreateGroup(ProgramNode, ".Net Function");

                {
                    var io = CreateGroup(functionNode, "Input/Output");
                    var conOut = CreateGroup(io, "ConsoleOut");
                    CreateAssetMenu(ownerCommandCanvas, conOut, new ConsoleOut());
                }

                {
                    var tools = CreateGroup(functionNode, "Exec");
                    CreateAssetMenu(ownerCommandCanvas, tools, new CallFile());
                }

                ScriptImplement.ImportScriptMethods(ownerCommandCanvas, functionNode);
            }
        }

        /// <summary>
        /// スクリプトにDLLをインポートします。
        /// </summary>
        /// <param name="path">DLLファイルのパス</param>
        /// <param name="ignoreClassList">拒否するクラスのリスト</param>
        public void ImportDll(string path, List<string> ignoreClassList = null)
        {
            if (DllModulePathList.Contains(path))
            {
                return;
            }
            if (DllNode is null)
            {
                DllNode = CreateGroup(ProgramNode, "Import");
            }
            string name = ScriptImplement.ImportScriptMethodsFromDllFile(OwnerCommandCanvas, DllNode, path, ignoreClassList);
            if (name != null)
            {
                ModulueNameList.Add(name);  // インポートリストに表示
                DllModulePathList.Add(path);
            }
        }

        /// <summary>
        /// スクリプトにパッケージをインポートします。
        /// </summary>
        /// <param name="packageName">パッケージ名</param>
        /// <param name="ignoreClassList">拒否するクラスのリスト</param>
        public void ImportPackage(string packageName, List<string> ignoreClassList = null)
        {
            if (PackageModuleList.Contains(packageName))
            {
                return;
            }
            if (DllNode is null)
            {
                DllNode = CreateGroup(ProgramNode, "Import");
            }
            string name = ScriptImplement.ImportScriptMethodsFromPackage(OwnerCommandCanvas, DllNode, packageName, ignoreClassList);
            if (name != null)
            {
                ModulueNameList.Add(name);  // インポートリストに表示
                PackageModuleList.Add(packageName);
            }
        }

        /// <summary>
        /// スクリプトにクラスをインポートします。
        /// </summary>
        /// <param name="className">クラス名</param>
        /// <returns>true==成功</returns>
        public bool ImportClass(string className)
        {
            if (ClassModuleList.Contains(className))
            {
                return true;
            }
            Type classType = CbST.GetTypeEx(className);
            if (classType is null)
            {
                // クラスが見つからない

                return false;
            }
            if (DllNode is null)
            {
                DllNode = CreateGroup(ProgramNode, "Import");
            }
            string name = ScriptImplement.ImportScriptMethodsFromClass(OwnerCommandCanvas, DllNode, className);
            if (name != null)
            {
                ModulueNameList.Add(name);  // インポートリストに表示
                ClassModuleList.Add(className);
                return true;
            }
            return false;
        }

        /// <summary>
        /// スクリプトにNuGetでインポートします。
        /// </summary>
        /// <param name="packageDir">パッケージ格納ディレクトリ</param>
        /// <param name="pkgName">パッケージ名</param>
        /// <param name="version">バージョン</param>
        /// <returns>true==成功</returns>
        public bool ImportNuGet(string packageDir, string pkgName, string version)
        {
            return ImportNuGet(packageDir, pkgName + $"({version})");
        }

        /// <summary>
        /// スクリプトにNuGetでインポートします。
        /// </summary>
        /// <param name="packageDir">パッケージ格納ディレクトリ</param>
        /// <param name="pkgName">パッケージ名</param>
        /// <returns>true==成功</returns>
        public bool ImportNuGet(string packageDir, string pkgName)
        {
            if (NuGetModuleList.Contains(pkgName))
            {
                return true;
            }
            if (NuGetNode is null)
            {
                NuGetNode = CreateGroup(ProgramNode, "NuGet Package");
            }
            string name = ScriptImplement.ImportScriptMethodsFromNuGet(OwnerCommandCanvas, NuGetNode, packageDir, pkgName);
            if (name != null)
            {
                ModulueNameList.Add(name);  // インポートリストに表示
                NuGetModuleList.Add(pkgName);
                CommandCanvasList.OutPut.OutLine(nameof(ScriptImplement), $"NuGet successed.");
                return true;
            }
            CommandCanvasList.OutPut.OutLine(nameof(ScriptImplement), $"faild.");
            return false;
        }

        /// <summary>
        /// モジュールの登録を削除します。
        /// </summary>
        public void ClearModule()
        {
            ModulueNameList.Clear();
            DllModulePathList.Clear();
            PackageModuleList.Clear();
            ClassModuleList.Clear();
            NuGetModuleList.Clear();
            if (DllNode != null)
            {
                ProgramNode.Child.Remove(DllNode);
                DllNode = null;
            }
            if (NuGetNode != null)
            {
                ProgramNode.Child.Remove(NuGetNode);
                NuGetNode = null;
            }
        }
    }

    //-----------------------------------------------------------------
    class LiteralType : IFuncAssetLiteralDef
    {
        public string MenuTitle => "Literal";

        public string HelpText { get; } = Language.GetInstance["LiteralType"];

        public string ValueType { get; } = CbSTUtils.FREE_TYPE_STR;

        public CbType[] DeleteSelectItems => CbScript.BaseDeleteCbTypes2;
    }

    //-----------------------------------------------------------------
    class LiteralListType : IFuncAssetLiteralDef
    {
        public string MenuTitle => "Literal List";

        public string HelpText { get; } = Language.GetInstance["LiteralListType"];

        public string ValueType { get; } = CbSTUtils.FREE_LIST_TYPE_STR;

        public CbType[] DeleteSelectItems => CbScript.BaseDeleteCbTypes;
    }

    //-----------------------------------------------------------------
    class Sum : FuncAssetSub, IFuncAssetWithArgumentDef
    {
        public string AssetCode => nameof(Sum);

        public string HelpText { get; } = Language.GetInstance["Sum"];

        public string MenuTitle => AssetCode;

        public string ValueType { get; } = CbSTUtils.FREE_TYPE_STR;

        public CbType[] DeleteSelectItems => CbScript.MathDeleteNotCbAddableTypes;

        public bool ImplAsset(MultiRootConnector col, bool notheradMode = false)
        {
            col.MakeFunction(
                MenuTitle,
                HelpText,
                CbST.CbCreateTF(col.SelectedVariableType[0]),  // 返し値の型
                new List<ICbValue>()  // 引数
                {
                    CbList.Create(col.SelectedVariableType[0], "sample"),
                },
                new Func<List<ICbValue>, DummyArgumentsStack, ICbValue>(
                    (argument, cagt) =>
                    {
                        var ret = CbST.CbCreate(col.SelectedVariableType[0]);    // 返し値
                        try
                        {
                            TryArgListProc(argument[0],
                                (valueData) =>
                                {
                                    ret.Add(valueData);
                                });
                        }
                        catch (Exception ex)
                        {
                            col.ExceptionFunc(ret, ex);
                        }
                        return ret;
                    }
                    )
                );

            return true;
        }
    }

    //-----------------------------------------------------------------
    class Sum_Func : FuncAssetSub, IFuncAssetWithArgumentDef
    {
        public string AssetCode => nameof(Sum_Func);

        public string HelpText { get; } = Language.GetInstance["Sum_Func"];

        public string MenuTitle => "Sum<Func>";

        public string ValueType { get; } = CbSTUtils.FREE_TYPE_STR;

        public CbType[] DeleteSelectItems => CbScript.MathDeleteNotCbAddableTypes;

        public bool ImplAsset(MultiRootConnector col, bool notheradMode = false)
        {
            // 仮引数コントロールを作成
            DummyArgumentsControl dummyArgumentsControl = new DummyArgumentsControl(col);

            col.MakeFunction(
                $"Sum Func<{col.SelectedVariableTypeName[0]},{col.SelectedVariableTypeName[0]}>.Invoke",
                HelpText,
                CbST.CbCreateTF(col.SelectedVariableType[0]),  // 返し値の型
                new List<ICbValue>()  // 引数
                {
                    CbST.CbCreate(col.SelectedVariableType[0], "base"),
                    CbList.Create(typeof(Func<,>).MakeGenericType(
                        col.SelectedVariableType[0],
                        col.SelectedVariableType[0]),
                        "sample"),
                },
                new Func<List<ICbValue>, DummyArgumentsStack, ICbValue>(
                    (argument, cagt) =>
                    {
                        var ret = CbST.CbCreate(col.SelectedVariableType[0]);    // 返し値
                        if (dummyArgumentsControl.IsInvalid(cagt))
                            return ret; // 実行環境が有効でない

                        try
                        {
                            ret.Set(argument[0]);

                            TryArgListProc(argument[1],
                                (valueData) =>
                                {
                                    dummyArgumentsControl.Enable(cagt, ret.Data);    // 仮引数に引数を登録

                                    if (IsCallBack(valueData))
                                        ret.Add(CallEvent(valueData, cagt));

                                    dummyArgumentsControl.Invalidated(cagt);    // 仮引数後処理
                                });
                        }
                        catch (Exception ex)
                        {
                            col.ExceptionFunc(ret, ex);
                        }
                        return ret;
                    }
                    )
                );

            return true;
        }
    }

    //-----------------------------------------------------------------
    class Subroutine : FuncAssetSub, IFuncAssetWithArgumentDef
    {
        public string AssetCode => nameof(Subroutine);

        public string HelpText { get; } = Language.GetInstance["Subroutine"];

        public string MenuTitle => "Sequence";

        public string ValueType { get; } = CbSTUtils.FREE_TYPE_STR;

        public CbType[] DeleteSelectItems => CbScript.BaseDeleteCbTypes;

        public bool ImplAsset(MultiRootConnector col, bool notheradMode = false)
        {
            col.MakeFunction(
                MenuTitle,
                HelpText,
                CbST.CbCreateTF(col.SelectedVariableType[0]),  // 返し値の型
                new List<ICbValue>()  // 引数
                {
                    CbList.Create(col.SelectedVariableType[0], "call list"),
                },
                new Func<List<ICbValue>, DummyArgumentsStack, ICbValue>(
                    (argument, cagt) =>
                    {
                        var ret = CbST.CbCreate(col.SelectedVariableType[0]);    // 返し値
                        try
                        {
                            var argList = GetArgumentList(argument, 0);
                            if (argList.Count != 0)
                            {
                                ret.Set(argList[argList.Count - 1]);
                            }
                        }
                        catch (Exception ex)
                        {
                            col.ExceptionFunc(ret, ex);
                        }
                        return ret;
                    }
                    )
                );

            // 実行を可能にする
            col.LinkConnectorControl.IsRunable = true;

            if (!notheradMode)
            {// 「call list」のリンクコネクターを取得する
                LinkConnector arg = col.LinkConnectorControl.GetArgument(0);
                // 要素を1つ増やす
                arg?.TryAddListNode(1);
            }

            return true;
        }
    }

    //-----------------------------------------------------------------
    class Inc : FuncAssetSub, IFuncAssetWithArgumentDef
    {
        public string AssetCode => nameof(Inc);

        public string HelpText { get; } = Language.GetInstance["Inc"];

        public string MenuTitle => AssetCode;

        public string ValueType { get; } = CbSTUtils.FREE_TYPE_STR;

        public CbType[] DeleteSelectItems => CbScript.MathDeleteNotCbCalcableTypes;

        public bool ImplAsset(MultiRootConnector col, bool notheradMode = false)
        {
            col.MakeFunction(
                MenuTitle,
                HelpText,
                CbST.CbCreateTF(col.SelectedVariableType[0]),  // 返し値の型
                new List<ICbValue>()  // 引数
                {
                    CbST.CbCreate(col.SelectedVariableType[0], "n"),
                },
                new Func<List<ICbValue>, DummyArgumentsStack, ICbValue>(
                    (argument, cagt) =>
                    {
                        var ret = CbST.CbCreate(col.SelectedVariableType[0]);    // 返し値
                        try
                        {
                            ret.Set(argument[0]);
                            ret.Add(CbInt.Create(1));
                        }
                        catch (Exception ex)
                        {
                            col.ExceptionFunc(ret, ex);
                        }
                        return ret;
                    }
                    )
                );

            return true;
        }
    }

    //-----------------------------------------------------------------
    class Dec : FuncAssetSub, IFuncAssetWithArgumentDef
    {
        public string AssetCode => nameof(Dec);

        public string HelpText { get; } = Language.GetInstance["Dec"];

        public string MenuTitle => AssetCode;

        public string ValueType { get; } = CbSTUtils.FREE_TYPE_STR;

        public CbType[] DeleteSelectItems => CbScript.MathDeleteNotCbCalcableTypes;

        public bool ImplAsset(MultiRootConnector col, bool notheradMode = false)
        {
            col.MakeFunction(
                MenuTitle,
                HelpText,
                CbST.CbCreateTF(col.SelectedVariableType[0]),  // 返し値の型
                new List<ICbValue>()  // 引数
                {
                    CbST.CbCreate(col.SelectedVariableType[0], "n"),
                },
                new Func<List<ICbValue>, DummyArgumentsStack, ICbValue>(
                    (argument, cagt) =>
                    {
                        var ret = CbST.CbCreate(col.SelectedVariableType[0]);    // 返し値
                        try
                        {
                            ret.Set(argument[0]);
                            ret.Subtract(CbInt.Create(1));
                        }
                        catch (Exception ex)
                        {
                            col.ExceptionFunc(ret, ex);
                        }
                        return ret;
                    }
                    )
                );

            return true;
        }
    }

    //-----------------------------------------------------------------
    class Mod : FuncAssetSub, IFuncAssetWithArgumentDef
    {
        public string AssetCode => nameof(Mod);

        public string HelpText { get; } = Language.GetInstance["Mod"];

        public string MenuTitle => AssetCode;

        public string ValueType { get; } = CbSTUtils.FREE_TYPE_STR;

        public CbType[] DeleteSelectItems => CbScript.MathDeleteNotCbCalcableTypes;

        public bool ImplAsset(MultiRootConnector col, bool notheradMode = false)
        {
            col.MakeFunction(
                "Modulo",
                HelpText,
                CbST.CbCreateTF(col.SelectedVariableType[0]),  // 返し値の型
                new List<ICbValue>()  // 引数
                {
                    CbST.CbCreate(col.SelectedVariableType[0], "n1"),
                    CbST.CbCreate(col.SelectedVariableType[0], "n2"),
                },
                new Func<List<ICbValue>, DummyArgumentsStack, ICbValue>(
                    (argument, cagt) =>
                    {
                        var ret = CbST.CbCreate(col.SelectedVariableType[0]);    // 返し値
                        try
                        {
                            ret.Set(argument[0]);
                            ret.Modulo(argument[1]);
                        }
                        catch (Exception ex)
                        {
                            col.ExceptionFunc(ret, ex);
                        }
                        return ret;
                    }
                    )
                );

            return true;
        }
    }

    //-----------------------------------------------------------------
    class Eq : FuncAssetSub, IFuncAssetWithArgumentDef
    {
        public string AssetCode => nameof(Eq);

        public string HelpText { get; } = Language.GetInstance["Eq"];

        public string MenuTitle => "==";

        public string ValueType { get; } = CbSTUtils.FREE_TYPE_STR;

        public CbType[] DeleteSelectItems => CbScript.BaseDeleteCbTypes;

        public bool ImplAsset(MultiRootConnector col, bool notheradMode = false)
        {
            col.MakeFunction(
                "Comparison ==",
                HelpText,
                CbST.CbCreateTF<bool>(),        // 返し値の型
                new List<ICbValue>()  // 引数
                {
                    CbST.CbCreate(col.SelectedVariableType[0], "n1"),
                    CbST.CbCreate(col.SelectedVariableType[0], "n2"),
                },
                new Func<List<ICbValue>, DummyArgumentsStack, ICbValue>(
                    (argument, cagt) =>
                    {
                        var temp = CbST.CbCreate(col.SelectedVariableType[0]);    // 返し値
                        var ret = CbST.CbCreate<bool>();
                        try
                        {
                            temp.Set(argument[0]);
                            ret.Data = temp.Equal(argument[1]);
                        }
                        catch (Exception ex)
                        {
                            col.ExceptionFunc(ret, ex);
                        }
                        return ret;
                    }
                    )
                );

            return true;
        }
    }

    //-----------------------------------------------------------------
    class Ge : FuncAssetSub, IFuncAssetWithArgumentDef
    {
        public string AssetCode => nameof(Ge);

        public string HelpText { get; } = Language.GetInstance["Ge"];

        public string MenuTitle => ">=";

        public string ValueType { get; } = CbSTUtils.FREE_TYPE_STR;

        public CbType[] DeleteSelectItems => CbScript.BaseDeleteCbTypes;

        public bool ImplAsset(MultiRootConnector col, bool notheradMode = false)
        {
            col.MakeFunction(
                "Comparison >=",
                HelpText,
                CbST.CbCreateTF<bool>(),  // 返し値の型
                new List<ICbValue>()          // 引数
                {
                    CbST.CbCreate(col.SelectedVariableType[0], "n1"),
                    CbST.CbCreate(col.SelectedVariableType[0], "n2"),
                },
                new Func<List<ICbValue>, DummyArgumentsStack, ICbValue>(
                    (argument, cagt) =>
                    {
                        var temp = CbST.CbCreate(col.SelectedVariableType[0]);    // 返し値
                        var ret = CbST.CbCreate<bool>();
                        try
                        {
                            temp.Set(argument[0]);
                            ret.Data = temp.GreaterThanOrEqual(argument[1]);
                        }
                        catch (Exception ex)
                        {
                            col.ExceptionFunc(ret, ex);
                        }
                        return ret;
                    }
                    )
                );

            return true;
        }
    }

    //-----------------------------------------------------------------
    class Gt : FuncAssetSub, IFuncAssetWithArgumentDef
    {
        public string AssetCode => nameof(Gt);

        public string HelpText { get; } = Language.GetInstance["Gt"];

        public string MenuTitle => ">";

        public string ValueType { get; } = CbSTUtils.FREE_TYPE_STR;

        public CbType[] DeleteSelectItems => CbScript.BaseDeleteCbTypes;

        public bool ImplAsset(MultiRootConnector col, bool notheradMode = false)
        {
            col.MakeFunction(
                "Comparison >",
                HelpText,
                CbST.CbCreateTF<bool>(),  // 返し値の型
                new List<ICbValue>()          // 引数
                {
                    CbST.CbCreate(col.SelectedVariableType[0], "n1"),
                    CbST.CbCreate(col.SelectedVariableType[0], "n2"),
                },
                new Func<List<ICbValue>, DummyArgumentsStack, ICbValue>(
                    (argument, cagt) =>
                    {
                        var temp = CbST.CbCreate(col.SelectedVariableType[0]);
                        var ret = CbST.CbCreate<bool>();    // 返し値
                        try
                        {
                            temp.Set(argument[0]);
                            ret.Data = temp.GreaterThan(argument[1]);
                        }
                        catch (Exception ex)
                        {
                            col.ExceptionFunc(ret, ex);
                        }
                        return ret;
                    }
                    )
                );

            return true;
        }
    }

    //-----------------------------------------------------------------
    class Le : FuncAssetSub, IFuncAssetWithArgumentDef
    {
        public string AssetCode => nameof(Le);

        public string HelpText { get; } = Language.GetInstance["Le"];

        public string MenuTitle => "<=";

        public string ValueType { get; } = CbSTUtils.FREE_TYPE_STR;

        public CbType[] DeleteSelectItems => CbScript.BaseDeleteCbTypes;

        public bool ImplAsset(MultiRootConnector col, bool notheradMode = false)
        {
            col.MakeFunction(
                "Comparison <=",
                HelpText,
                CbST.CbCreateTF<bool>(),  // 返し値の型
                new List<ICbValue>()    // 引数
                {
                    CbST.CbCreate(col.SelectedVariableType[0], "n1"),
                    CbST.CbCreate(col.SelectedVariableType[0], "n2"),
                },
                new Func<List<ICbValue>, DummyArgumentsStack, ICbValue>(
                    (argument, cagt) =>
                    {
                        var temp = CbST.CbCreate(col.SelectedVariableType[0]);
                        var ret = CbST.CbCreate<bool>();    // 返し値
                        try
                        {
                            temp.Set(argument[0]);
                            ret.Data = temp.LessThanOrEqual(argument[1]);
                        }
                        catch (Exception ex)
                        {
                            col.ExceptionFunc(ret, ex);
                        }
                        return ret;
                    }
                    )
                );

            return true;
        }
    }

    //-----------------------------------------------------------------
    class Lt : FuncAssetSub, IFuncAssetWithArgumentDef
    {
        public string AssetCode => nameof(Lt);

        public string HelpText { get; } = Language.GetInstance["Lt"];

        public string MenuTitle => "<";

        public string ValueType { get; } = CbSTUtils.FREE_TYPE_STR;

        public CbType[] DeleteSelectItems => CbScript.BaseDeleteCbTypes;

        public bool ImplAsset(MultiRootConnector col, bool notheradMode = false)
        {
            col.MakeFunction(
                "Comparison <",
                HelpText,
                CbST.CbCreateTF<bool>(),  // 返し値の型
                new List<ICbValue>()          // 引数
                {
                    CbST.CbCreate(col.SelectedVariableType[0], "n1"),
                    CbST.CbCreate(col.SelectedVariableType[0], "n2"),
                },
                new Func<List<ICbValue>, DummyArgumentsStack, ICbValue>(
                    (argument, cagt) =>
                    {
                        var temp = CbST.CbCreate(col.SelectedVariableType[0]);
                        var ret = CbST.CbCreate<bool>();    // 返し値
                        try
                        {
                            temp.Set(argument[0]);
                            ret.Data = temp.LessThan(argument[1]);
                        }
                        catch (Exception ex)
                        {
                            col.ExceptionFunc(ret, ex);
                        }
                        return ret;
                    }
                    )
                );

            return true;
        }
    }

    //-----------------------------------------------------------------
    class And : FuncAssetSub, IFuncAssetWithArgumentDef
    {
        public string AssetCode => nameof(And);

        public string HelpText { get; } = Language.GetInstance["And"];

        public string MenuTitle => AssetCode;

        public string ValueType { get; } = typeof(bool).FullName;

        public CbType[] DeleteSelectItems => null;

        public bool ImplAsset(MultiRootConnector col, bool notheradMode = false)
        {
            col.MakeFunction(
                MenuTitle,
                HelpText,
                CbST.CbCreateTF<bool>(),  // 返し値の型
                new List<ICbValue>()          // 引数
                {
                    CbList.Create(typeof(bool), "sample"),
                },
                new Func<List<ICbValue>, DummyArgumentsStack, ICbValue>(
                    (argument, cagt) =>
                    {
                        bool temp = true;
                        var ret = CbST.CbCreate<bool>();    // 返し値
                        try
                        {
                            TryArgListProc(argument[0],
                                (valueData) =>
                                {
                                    temp = temp && GetArgument<bool>(valueData);
                                });

                            ret.Data = temp;
                        }
                        catch (Exception ex)
                        {
                            col.ExceptionFunc(ret, ex);
                        }
                        return ret;
                    }
                    )
                );

            return true;
        }
    }

    //-----------------------------------------------------------------
    class Or : FuncAssetSub, IFuncAssetWithArgumentDef
    {
        public string AssetCode => nameof(Or);

        public string HelpText { get; } = Language.GetInstance["Or"];

        public string MenuTitle => AssetCode;

        public string ValueType { get; } = typeof(bool).FullName;

        public CbType[] DeleteSelectItems => null;

        public bool ImplAsset(MultiRootConnector col, bool notheradMode = false)
        {
            col.MakeFunction(
                MenuTitle,
                HelpText,
                CbST.CbCreateTF<bool>(),  // 返し値の型
                new List<ICbValue>()          // 引数
                {
                    CbList.Create(typeof(bool), "sample"),
                },
                new Func<List<ICbValue>, DummyArgumentsStack, ICbValue>(
                    (argument, cagt) =>
                    {
                        bool temp = false;
                        var ret = CbST.CbCreate<bool>();    // 返し値
                        try
                        {
                            TryArgListProc(argument[0],
                                (valueData) =>
                                {
                                    temp = temp || GetArgument<bool>(valueData);
                                });

                            ret.Data = temp;
                        }
                        catch (Exception ex)
                        {
                            col.ExceptionFunc(ret, ex);
                        }
                        return ret;
                    }
                    )
                );

            return true;
        }
    }

    //-----------------------------------------------------------------
    class Not : FuncAssetSub, IFuncAssetWithArgumentDef
    {
        public string AssetCode => nameof(Not);

        public string HelpText { get; } = Language.GetInstance["Not"];

        public string MenuTitle => AssetCode;

        public string ValueType { get; } = typeof(bool).FullName;

        public CbType[] DeleteSelectItems => null;

        public bool ImplAsset(MultiRootConnector col, bool notheradMode = false)
        {
            col.MakeFunction(
                MenuTitle,
                HelpText,
                CbST.CbCreateTF<bool>(),  // 返し値の型
                new List<ICbValue>()          // 引数
                {
                    CbST.CbCreate<bool>("sample", false),
                },
                new Func<List<ICbValue>, DummyArgumentsStack, ICbValue>(
                    (argument, cagt) =>
                    {
                        var ret = CbST.CbCreate<bool>();    // 返し値
                        try
                        {
                            ret.Data = !GetArgument<bool>(argument, 0);
                        }
                        catch (Exception ex)
                        {
                            col.ExceptionFunc(ret, ex);
                        }
                        return ret;
                    }
                    )
                );

            return true;
        }
    }

    //-----------------------------------------------------------------
    class Mul : FuncAssetSub, IFuncAssetWithArgumentDef
    {
        public string AssetCode => nameof(Mul);

        public string HelpText { get; } = Language.GetInstance["Mul"];

        public string MenuTitle => AssetCode;

        public string ValueType { get; } = CbSTUtils.FREE_TYPE_STR;

        public CbType[] DeleteSelectItems => CbScript.MathDeleteNotCbCalcableTypes;

        public bool ImplAsset(MultiRootConnector col, bool notheradMode = false)
        {
            col.MakeFunction(
                "Multiply",
                HelpText,
                CbST.CbCreateTF(col.SelectedVariableType[0]),  // 返し値の型
                new List<ICbValue>()  // 引数
                {
                    CbST.CbCreate(col.SelectedVariableType[0], "base"),
                    CbList.Create(col.SelectedVariableType[0], "sample"),
                },
                new Func<List<ICbValue>, DummyArgumentsStack, ICbValue>(
                    (argument, cagt) =>
                    {
                        var ret = CbST.CbCreate(col.SelectedVariableType[0]);    // 返し値
                        try
                        {
                            ret.Set(argument[0]);

                            TryArgListProc(argument[1],
                                (valueData) =>
                                {
                                    ret.Multiply(valueData);
                                });
                        }
                        catch (Exception ex)
                        {
                            col.ExceptionFunc(ret, ex);
                        }
                        return ret;
                    }
                    )
                );

            return true;
        }
    }

    //-----------------------------------------------------------------
    class Div : FuncAssetSub, IFuncAssetWithArgumentDef
    {
        public string AssetCode => nameof(Div);

        public string HelpText { get; } = Language.GetInstance["Div"];

        public string MenuTitle => AssetCode;

        public string ValueType { get; } = CbSTUtils.FREE_TYPE_STR;

        public CbType[] DeleteSelectItems => CbScript.MathDeleteNotCbCalcableTypes;

        public bool ImplAsset(MultiRootConnector col, bool notheradMode = false)
        {
            col.MakeFunction(
                "Divide",
                HelpText,
                CbST.CbCreateTF(col.SelectedVariableType[0]),  // 返し値の型
                new List<ICbValue>()  // 引数
                {
                    CbST.CbCreate(col.SelectedVariableType[0], "n1"),
                    CbList.Create(col.SelectedVariableType[0], "sample"),
                },
                new Func<List<ICbValue>, DummyArgumentsStack, ICbValue>(
                    (argument, cagt) =>
                    {
                        var ret = CbST.CbCreate(col.SelectedVariableType[0]);    // 返し値
                        try
                        {
                            ret.Set(argument[0]);
                            TryArgListProc(argument[1],
                               (valueData) =>
                               {
                                   ret.Divide(valueData);
                               });
                        }
                        catch (Exception ex)
                        {
                            col.ExceptionFunc(ret, ex);
                        }
                        return ret;
                    }
                    )
                );

            return true;
        }
    }

    //-----------------------------------------------------------------
    class Sub : FuncAssetSub, IFuncAssetWithArgumentDef
    {
        public string AssetCode => nameof(Sub);

        public string HelpText { get; } = Language.GetInstance["Sub"];

        public string MenuTitle => AssetCode;

        public string ValueType { get; } = CbSTUtils.FREE_TYPE_STR;

        public CbType[] DeleteSelectItems => CbScript.MathDeleteNotCbCalcableTypes;

        public bool ImplAsset(MultiRootConnector col, bool notheradMode = false)
        {
            col.MakeFunction(
                "Subtract",
                HelpText,
                CbST.CbCreateTF(col.SelectedVariableType[0]),  // 返し値の型
                new List<ICbValue>()  // 引数
                {
                    CbST.CbCreate(col.SelectedVariableType[0], "base"),
                    CbList.Create(col.SelectedVariableType[0], "sample"),
                },
                new Func<List<ICbValue>, DummyArgumentsStack, ICbValue>(
                    (argument, cagt) =>
                    {
                        var ret = CbST.CbCreate(col.SelectedVariableType[0]);    // 返し値
                        try
                        {
                            ret.Set(argument[0]);

                            TryArgListProc(argument[1],
                                (valueData) =>
                                {
                                    ret.Subtract(valueData);
                                });
                        }
                        catch (Exception ex)
                        {
                            col.ExceptionFunc(ret, ex);
                        }
                        return ret;
                    }
                    )
                );

            return true;
        }
    }

    //-----------------------------------------------------------------
    class CallFile : FuncAssetSub, IFuncAssetWithArgumentDef
    {
        public string AssetCode => nameof(CallFile);

        public string HelpText { get; } = Language.GetInstance["CallFile"];

        public string MenuTitle => "Call File";

        public string ValueType { get; } = typeof(string).FullName;

        public CbType[] DeleteSelectItems => null;

        public bool ImplAsset(MultiRootConnector col, bool notheradMode = false)
        {
            col.MakeFunction(
                MenuTitle,
                HelpText,
                CbFunc<Func<int>, CbInt>.TF,   // 返し値の型
                new List<ICbValue>()      // 引数
                {
                    CbST.CbCreate<string>("path"),
                    CbST.CbCreate<bool>("redirect"),
                    CbST.CbCreate<List<string>>("arguments"),
                },
                new Func<List<ICbValue>, DummyArgumentsStack, ICbValue>(
                    (argument, cagt) =>
                    {
                        var ret = CbST.CbCreate<Func<int>>() as ICbEvent;    // 返し値
                        try
                        {
                            string path = GetArgument<string>(argument, 0);
                            bool redirect = GetArgument<bool>(argument, 1);

                            ToolExec toolExec = new ToolExec(path);

                            TryArgListProc(argument[2],
                                (valueData) =>
                                {
                                    toolExec.ParamList.Add(GetArgument<string>(valueData));
                                });

                            ret.CallBack = (cagt2) =>
                            {
                                return CbInt.Create(toolExec.Start(redirect));
                            };
                        }
                        catch (Exception ex)
                        {
                            col.ExceptionFunc(ret, ex);
                        }
                        return ret;
                    }
                    )
                );

            return true;
        }
    }

    //-----------------------------------------------------------------
    class If_Func : FuncAssetSub, IFuncAssetWithArgumentDef
    {
        public string AssetCode => nameof(If_Func);

        public string HelpText { get; } = Language.GetInstance["If_Func"];

        public string MenuTitle => $"If<{CbSTUtils.FUNC_STR}>";

        public string ValueType { get; } = CbSTUtils.FREE_TYPE_STR;

        public CbType[] DeleteSelectItems => CbScript.BaseDeleteCbTypes;

        public bool ImplAsset(MultiRootConnector col, bool notheradMode = false)
        {
            col.MakeFunction(
                $"If {CbSTUtils.FUNC_STR}<{col.SelectedVariableTypeName[0]}>.Invoke",
                HelpText,
                CbST.CbCreateTF(col.SelectedVariableType[0]),  // 返し値の型
                new List<ICbValue>()  // 引数
                {
                    CbST.CbCreate<bool>("conditions", false),
                    CbFunc.CreateFunc(col.SelectedVariableType[0], "func true"),
                    CbFunc.CreateFunc(col.SelectedVariableType[0], "func false"),
                },
                new Func<List<ICbValue>, DummyArgumentsStack, ICbValue>(
                    (argument, cagt) =>
                    {
                        var ret = CbST.CbCreate(col.SelectedVariableType[0]);    // 返し値
                        try
                        {
                            if (GetArgument<bool>(argument, 0))
                            {
                                if (IsCallBack(argument[1]))
                                    ret.Set(CallEvent(argument[1], cagt));
                            }
                            else
                            {
                                if (IsCallBack(argument[2]))
                                    ret.Set(CallEvent(argument[2], cagt));
                            }
                        }
                        catch (Exception ex)
                        {
                            col.ExceptionFunc(ret, ex);
                        }
                        return ret;
                    }
                    )
                );

            return true;
        }
    }

    //-----------------------------------------------------------------
    class If_Action : FuncAssetSub, IFuncAssetWithArgumentDef
    {
        public string AssetCode => nameof(If_Action);

        public string HelpText { get; } = Language.GetInstance["If_Action"];

        public string MenuTitle => $"If<{CbSTUtils.ACTION_STR}>";

        public string ValueType { get; } = CbSTUtils.DUMMY_TYPE_STR;

        public CbType[] DeleteSelectItems => CbScript.BaseDeleteCbTypes;

        public bool ImplAsset(MultiRootConnector col, bool notheradMode = false)
        {
            col.MakeFunction(
                $"If {CbSTUtils.ACTION_STR}.Invoke",
                HelpText,
                CbVoid.TF,  // 返し値の型
                new List<ICbValue>()  // 引数
                {
                    CbST.CbCreate<bool>("conditions", false),
                    CbFunc.CreateAction("func true"),
                    CbFunc.CreateAction("func false"),
                },
                new Func<List<ICbValue>, DummyArgumentsStack, ICbValue>(
                    (argument, cagt) =>
                    {
                        try
                        {
                            if (GetArgument<bool>(argument, 0))
                            {
                                if (IsCallBack(argument[1]))
                                    CallEvent(argument[1], cagt);
                            }
                            else
                            {
                                if (IsCallBack(argument[2]))
                                    CallEvent(argument[2], cagt);
                            }
                        }
                        catch (Exception ex)
                        {
                            col.ExceptionFunc(null, ex);
                        }
                        return null;
                    }
                    )
                );

            return true;
        }
    }

    //-----------------------------------------------------------------
    class If : FuncAssetSub, IFuncAssetWithArgumentDef
    {
        public string AssetCode => nameof(If);

        public string HelpText { get; } = Language.GetInstance["If"];

        public string MenuTitle => AssetCode;

        public string ValueType { get; } = CbSTUtils.FREE_TYPE_STR;

        public CbType[] DeleteSelectItems => CbScript.BaseDeleteCbTypes;

        public bool ImplAsset(MultiRootConnector col, bool notheradMode = false)
        {
            col.MakeFunction(
                "If<" + col.SelectedVariableTypeName[0] + ">",
                HelpText,
                CbST.CbCreateTF(col.SelectedVariableType[0]),  // 返し値の型
                new List<ICbValue>()  // 引数
                {
                    CbST.CbCreate<bool>("conditions", false),
                    CbST.CbCreate(col.SelectedVariableType[0], "return true"),
                    CbST.CbCreate(col.SelectedVariableType[0], "return false"),
                },
                new Func<List<ICbValue>, DummyArgumentsStack, ICbValue>(
                    (argument, cagt) =>
                    {
                        var ret = CbST.CbCreate(col.SelectedVariableType[0]);    // 返し値
                        try
                        {
                            if (GetArgument<bool>(argument, 0))
                            {
                                ret.Set(argument[1]);
                            }
                            else
                            {
                                ret.Set(argument[2]);
                            }
                        }
                        catch (Exception ex)
                        {
                            col.ExceptionFunc(ret, ex);
                        }
                        return ret;
                    }
                    )
                );

            return true;
        }
    }

    //-----------------------------------------------------------------
    class Invoke : FuncAssetSub, IFuncAssetWithArgumentDef
    {
        public string AssetCode => "Invoke Func";

        public string HelpText { get; } = Language.GetInstance["Invoke"];

        public string MenuTitle => AssetCode;

        public string ValueType { get; } = CbSTUtils.FREE_TYPE_STR;

        public CbType[] DeleteSelectItems => CbScript.BaseDeleteCbTypes;

        public bool ImplAsset(MultiRootConnector col, bool notheradMode = false)
        {
            col.MakeFunction(
                $"{CbSTUtils.FUNC_STR}<{col.SelectedVariableTypeName[0]}>.Invoke",
                HelpText,
                CbST.CbCreateTF(col.SelectedVariableType[0]),  // 返し値の型
                new List<ICbValue>()  // 引数
                {
                    CbFunc.CreateFunc(col.SelectedVariableType[0], "func"),
                },
                new Func<List<ICbValue>, DummyArgumentsStack, ICbValue>(
                    (argument, cagt) =>
                    {
                        var ret = CbST.CbCreate(col.SelectedVariableType[0]);    // 返し値
                        try
                        {
                            if (IsCallBack(argument[0]))
                                ret.Set(CallEvent(argument[0], cagt));
                        }
                        catch (Exception ex)
                        {
                            col.ExceptionFunc(ret, ex);
                        }
                        return ret;
                    }
                    )
                );

            // 実行を可能にする
            col.LinkConnectorControl.IsRunable = true;

            return true;
        }
    }

    //-----------------------------------------------------------------
    class InvokeWithArg : FuncAssetSub, IFuncAssetWithArgumentDef
    {
        public string AssetCode => nameof(InvokeWithArg);

        public string HelpText { get; } = Language.GetInstance["InvokeWithArg"];

        public string MenuTitle => "Invoke Func With Argument";

        public string ValueType { get; } = CbSTUtils.FREE_TYPE_STR;

        public CbType[] DeleteSelectItems => CbScript.BaseDeleteCbTypes;

        public bool ImplAsset(MultiRootConnector col, bool notheradMode = false)
        {
            // 仮引数コントロールを作成
            DummyArgumentsControl dummyArgumentsControl = new DummyArgumentsControl(col);

            col.MakeFunction(
                $"{CbSTUtils.FUNC_STR}<{CbSTUtils.OBJECT_STR},{col.SelectedVariableTypeName[0]}>.Invoke",
                HelpText,
                CbST.CbCreateTF(col.SelectedVariableType[0]),  // 返し値の型
                new List<ICbValue>()  // 引数
                {
                   CbST.CbCreate(col.SelectedVariableType[0], "argument"),
                   CbFunc.CreateFunc(typeof(object), col.SelectedVariableType[0], "func"),
                },
                new Func<List<ICbValue>, DummyArgumentsStack, ICbValue>(
                    (argument, cagt) =>
                    {
                        var ret = CbST.CbCreate(col.SelectedVariableType[0]);    // 返し値
                        if (dummyArgumentsControl.IsInvalid(cagt))
                            return ret; // 実行環境が有効でない

                        try
                        {
                            dummyArgumentsControl.EnableCbValue(cagt, argument[0]);    // 仮引数に引数を登録

                            if (IsCallBack(argument[1]))
                                ret.Set(CallEvent(argument[1], cagt));

                            dummyArgumentsControl.Invalidated(cagt);    // 仮引数後処理
                        }
                        catch (Exception ex)
                        {
                            col.ExceptionFunc(ret, ex);
                        }
                        return ret;
                    }
                    )
                );

            // 実行を可能にする
            col.LinkConnectorControl.IsRunable = true;

            return true;
        }
    }

    //-----------------------------------------------------------------
    class InvokeAction : FuncAssetSub, IFuncAssetWithArgumentDef
    {
        public string AssetCode => "Invoke Action";

        public string HelpText { get; } = Language.GetInstance["Invoke"];

        public string MenuTitle => AssetCode;

        public string ValueType { get; } = CbSTUtils.DUMMY_TYPE_STR;

        public CbType[] DeleteSelectItems => CbScript.BaseDeleteCbTypes;

        public bool ImplAsset(MultiRootConnector col, bool notheradMode = false)
        {
            col.MakeFunction(
                $"{CbSTUtils.ACTION_STR}.Invoke",
                HelpText,
                CbVoid.TF,  // 返し値の型
                new List<ICbValue>()  // 引数
                {
                    CbFunc.CreateAction("func"),
                },
                new Func<List<ICbValue>, DummyArgumentsStack, ICbValue>(
                    (argument, cagt) =>
                    {
                        try
                        {
                            if (IsCallBack(argument[0]))
                                CallEvent(argument[0], cagt);
                        }
                        catch (Exception ex)
                        {
                            col.ExceptionFunc(null, ex);
                        }
                        return null;
                    }
                    )
                );

            // 実行を可能にする
            col.LinkConnectorControl.IsRunable = true;

            return true;
        }
    }

    //-----------------------------------------------------------------
    class InvokeActionWithArg : FuncAssetSub, IFuncAssetWithArgumentDef
    {
        public string AssetCode => nameof(InvokeActionWithArg);

        public string HelpText { get; } = Language.GetInstance["InvokeWithArg"];

        public string MenuTitle => "Invoke Action With Argument";

        public string ValueType { get; } = CbSTUtils.FREE_TYPE_STR;

        public CbType[] DeleteSelectItems => CbScript.BaseDeleteCbTypes;

        public bool ImplAsset(MultiRootConnector col, bool notheradMode = false)
        {
            // 仮引数コントロールを作成
            DummyArgumentsControl dummyArgumentsControl = new DummyArgumentsControl(col);

            col.MakeFunction(
                $"{CbSTUtils.ACTION_STR}<{col.SelectedVariableTypeName[0]}>.Invoke",
                HelpText,
                CbVoid.TF,    // 返し値の型
                new List<ICbValue>()          // 引数
                {
                   CbST.CbCreate(col.SelectedVariableType[0], "argument"),
                   CbFunc.CreateAction(col.SelectedVariableType[0], "func"),
                },
                new Func<List<ICbValue>, DummyArgumentsStack, ICbValue>(
                    (argument, cagt) =>
                    {
                        if (dummyArgumentsControl.IsInvalid(cagt))
                            return null; // 実行環境が有効でない

                        try
                        {
                            dummyArgumentsControl.EnableCbValue(cagt, argument[0]);    // 仮引数に引数を登録

                            if (IsCallBack(argument[1]))
                                CallEvent(argument[1], cagt);

                            dummyArgumentsControl.Invalidated(cagt);    // 仮引数後処理
                        }
                        catch (Exception ex)
                        {
                            col.ExceptionFunc(null, ex);
                        }
                        return null;
                    }
                    )
                );

            // 実行を可能にする
            col.LinkConnectorControl.IsRunable = true;

            return true;
        }
    }

    //-----------------------------------------------------------------
    class For : FuncAssetSub, IFuncAssetWithArgumentDef
    {
        public string AssetCode => nameof(For);

        public string HelpText { get; } = Language.GetInstance["For"];

        public string MenuTitle => $"For<{CbSTUtils.ACTION_STR}>";

        public string ValueType { get; } = CbSTUtils.DUMMY_TYPE_STR;

        public CbType[] DeleteSelectItems => CbScript.MathDeleteNotCbCalcableTypes;

        public bool ImplAsset(MultiRootConnector col, bool notheradMode = false)
        {
            // 仮引数コントロールを作成
            DummyArgumentsControl dummyArgumentsControl = new DummyArgumentsControl(col);

            col.MakeFunction(
                $"For {CbSTUtils.ACTION_STR}<{CbSTUtils.INT_STR}>.Invoke",
                HelpText,
                CbVoid.TF,        // 返し値の型
                new List<ICbValue>()  // 引数
                {
                    CbST.CbCreate<int>("[from", 0),
                    CbST.CbCreate<int>("to)", 0),
                    CbST.CbCreate<int>("step", 1),
                    CbFunc.CreateAction(typeof(int), "func f(index)"),
                },
                new Func<List<ICbValue>, DummyArgumentsStack, ICbValue>(
                    (argument, cagt) =>
                    {
                        if (dummyArgumentsControl.IsInvalid(cagt))
                            return null; // 実行環境が有効でない

                        try
                        {
                            int n1 = GetArgument<int>(argument, 0);
                            int n2 = GetArgument<int>(argument, 1);
                            int step = GetArgument<int>(argument, 2);
                            for (int i = n1; i < n2; i += step)
                            {
                                if (argument[3] is ICbEvent cbEvent && cbEvent.CallBack is null)
                                {

                                }
                                else
                                {
                                    dummyArgumentsControl.Enable(cagt, i);    // 仮引数に引数を登録
                                    if (IsCallBack(argument[3]))
                                        CallEvent(argument[3], cagt);
                                    dummyArgumentsControl.Invalidated(cagt);    // 仮引数後処理
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            col.ExceptionFunc(null, ex);
                        }
                        return null;
                    }
                    )
                );

            return true;
        }
    }

    //-----------------------------------------------------------------
    class _GetVariable : FuncAssetSub
    {
        public string AssetCode => nameof(GetVariable);

        public string HelpText { get; } = "";

        public bool ImplAsset(MultiRootConnector col, bool notheradMode = false)
        {
            VariableGetter variableGetter = new VariableGetter(col);
            if (variableGetter.IsError)
                return false;

            col.MakeFunction(
                variableGetter.MakeName,
                HelpText,
                CbST.CbCreateTF(col.SelectedVariableType[0]),  // 返し値の型
                null,   // 引数はなし
                new Func<List<ICbValue>, DummyArgumentsStack, ICbValue>(
                    (argument, cagt) =>
                    {
                        ICbValue ret = CbST.CbCreate(col.SelectedVariableType[0]);    // 返し値
                        try
                        {
                            ICbValue cbVSValue = col.OwnerCommandCanvas.ScriptWorkStack.Find(variableGetter.Id);
                            ret.CopyValue(cbVSValue);
                            col.LinkConnectorControl.UpdateValueData();
                        }
                        catch (Exception ex)
                        {
                            col.ExceptionFunc(ret, ex);
                        }
                        return ret;
                    }
                    )
                );

            return true;
        }
    }

    //-----------------------------------------------------------------
    class CreateVariable : _GetVariable, IFuncCreateVariableAssetDef
    {
        public string MenuTitle => "Create Variable";

        public new string HelpText { get; } = Language.GetInstance["CreateVariable"];

        public string ValueType { get; } = CbSTUtils.FREE_TYPE_STR;

        public CbType[] DeleteSelectItems => CbScript.BaseDeleteCbTypes;
    }

    //-----------------------------------------------------------------
    class CreateVariableList : _GetVariable, IFuncCreateVariableAssetDef
    {
        public string MenuTitle => "Create VariableList";

        public new string HelpText { get; } = Language.GetInstance["CreateVariableList"];

        public string ValueType { get; } = CbSTUtils.FREE_LIST_TYPE_STR;

        public CbType[] DeleteSelectItems => CbScript.BaseDeleteCbTypes;
    }

    //-----------------------------------------------------------------
    class CreateVariableFunc : _GetVariable, IFuncCreateVariableAssetDef
    {
        public string ValueType { get; } = CbSTUtils.FREE_FUNC_TYPE_STR;

        public new string HelpText { get; } = Language.GetInstance["CreateVariableFunc"];

        public string MenuTitle => $"Create Variable<{CbSTUtils.FUNC_STR}>";

        public CbType[] DeleteSelectItems => CbScript.BaseDeleteCbTypes;
    }

    //-----------------------------------------------------------------
    class GetVariable : _GetVariable, IFuncCreateVariableAssetDef
    {
        public string MenuTitle => "Get Variable";

        public new string HelpText { get; } = Language.GetInstance["GetVariable"];

        public string ValueType { get; } = CbSTUtils.FREE_TYPE_STR;

        public CbType[] DeleteSelectItems => null;
    }

    //-----------------------------------------------------------------
    class SetVariable : FuncAssetSub, IFuncCreateVariableAssetDef
    {
        public string AssetCode => nameof(SetVariable);

        public string HelpText { get; } = Language.GetInstance["SetVariable"];

        public string MenuTitle => "Set Variable";

        public string ValueType { get; } = CbSTUtils.FREE_FUNC_TYPE_STR;

        public CbType[] DeleteSelectItems => null;

        public bool ImplAsset(MultiRootConnector col, bool notheradMode = false)
        {
            VariableGetter variableGetter = new VariableGetter(col);
            if (variableGetter.IsError)
                return false;

            col.MakeFunction(
                variableGetter.MakeName,
                HelpText,
                CbST.CbCreateTF(col.SelectedVariableType[0]),  // 返し値の型
                new List<ICbValue>()  // 引数
                {
                    CbST.CbCreate(col.SelectedVariableType[0], "n"),
                },
                new Func<List<ICbValue>, DummyArgumentsStack, ICbValue>(
                    (argument, cagt) =>
                    {
                        ICbValue ret = CbST.CbCreate(col.SelectedVariableType[0]);    // 返し値
                        try
                        {
                            ICbValue cbVSValue = col.OwnerCommandCanvas.ScriptWorkStack.Find(variableGetter.Id);

                            if (argument[0] is ICbList cbList && cbVSValue is ICbList toList)
                            {
                                // リストのコピー

                                cbList.CopyTo(toList);
                            }
                            else
                            {
                                // 値のコピー

                                cbVSValue.CopyValue(argument[0]);
                            }
                            col.OwnerCommandCanvas.ScriptWorkStack.UpdateValueData(variableGetter.Id);
                            ret.Set(argument[0]);
                            col.LinkConnectorControl.UpdateValueData();
                        }
                        catch (Exception ex)
                        {
                            col.ExceptionFunc(ret, ex);
                        }
                        return ret;
                    }
                    )
                );

            return true;
        }
    }

    //-----------------------------------------------------------------
    class CallerArgument : FuncAssetSub, IFuncAssetWithArgumentDef
    {
        public string AssetCode => nameof(CallerArgument);

        public string HelpText { get; } = Language.GetInstance["CallerArgument"];

        public string MenuTitle => "DummyArgument";

        public string ValueType { get; } = CbSTUtils.FREE_TYPE_STR;

        public CbType[] DeleteSelectItems => CbScript.BaseDeleteCbTypes;

        public bool ImplAsset(MultiRootConnector col, bool notheradMode = false)
        {
            col.MakeFunction(
                "DummyArgument<" + col.SelectedVariableTypeName[0] + ">",
                HelpText,
                CbST.CbCreateTF(col.SelectedVariableType[0]),  // 返し値の型
                null,   // 引数はなし
                new Func<List<ICbValue>, DummyArgumentsStack, ICbValue>(
                    (argument, cagt) =>
                    {
                        var ret = CbST.CbCreate(col.SelectedVariableType[0]);    // 返し値
                        if (cagt.IsEmpty())
                        {
                            cagt.InvalidReturn();   // 有効でないまま返す
                            return ret;
                        }
                        try
                        {
                            // 呼び元引数をセット

                            if (cagt.IsGetValue())
                                ret.Set(cagt.GetValue());
                            else
                                cagt.InvalidReturn();   // 有効でないまま返す
                        }
                        catch (Exception ex)
                        {
                            col.ExceptionFunc(ret, ex);
                        }
                        return ret;
                    }
                    )
                );

            return true;
        }
    }

    //-----------------------------------------------------------------
    class CallerArguments : FuncAssetSub, IFuncAssetWithArgumentDef
    {
        public string AssetCode => nameof(CallerArguments);

        public string HelpText { get; } = Language.GetInstance["CallerArgument"];

        public string MenuTitle => "DummyArguments";

        public string ValueType { get; } = CbSTUtils.FREE_TYPE_STR;

        public CbType[] DeleteSelectItems => CbScript.BaseDeleteCbTypes;

        public bool ImplAsset(MultiRootConnector col, bool notheradMode = false)
        {
            col.MakeFunction(
                "DummyArguments<" + col.SelectedVariableTypeName[0] + ">",
                HelpText,
                CbST.CbCreateTF(col.SelectedVariableType[0]),  // 返し値の型
                new List<ICbValue>()  // 引数
                {
                    CbST.CbCreate<CbFuncArguments.INDEX>("select"),
                },
                new Func<List<ICbValue>, DummyArgumentsStack, ICbValue>(
                    (argument, cagt) =>
                    {
                        var ret = CbST.CbCreate(col.SelectedVariableType[0]);    // 返し値

                        var select = GetArgument<CbFuncArguments.INDEX>(argument, 0);

                        if (cagt.IsEmpty())
                        {
                            cagt.InvalidReturn();   // 有効でないまま返す
                            return ret;
                        }
                        try
                        {
                            // 呼び元引数をセット

                            if (cagt.IsGetValue())
                                ret.Set(cagt.GetValue(select));
                            else
                                cagt.InvalidReturn();   // 有効でないまま返す
                        }
                        catch (Exception ex)
                        {
                            col.ExceptionFunc(ret, ex);
                        }
                        return ret;
                    }
                    )
                );

            return true;
        }
    }

    //-----------------------------------------------------------------
    class Foreach : FuncAssetSub, IFuncAssetWithArgumentDef
    {
        public string AssetCode => nameof(Foreach);

        public string HelpText { get; } = Language.GetInstance["Foreach"];

        public string MenuTitle => $"Foreach<{CbSTUtils.ACTION_STR}>";

        public string ValueType { get; } = CbSTUtils.FREE_TYPE_STR;

        public CbType[] DeleteSelectItems => CbScript.BaseDeleteCbTypes;

        public bool ImplAsset(MultiRootConnector col, bool notheradMode = false)
        {
            // 仮引数コントロールを作成
            DummyArgumentsControl dummyArgumentsControl = new DummyArgumentsControl(col);

            col.MakeFunction(
                $"Foreach {CbSTUtils.ACTION_STR}<{col.SelectedVariableTypeName[0]}>.Invoke",
                HelpText,
                CbVoid.TF,        // 返し値の型
                new List<ICbValue>()  // 引数
                {
                    CbList.Create(col.SelectedVariableType[0], "sample"),
                    CbFunc.CreateAction(col.SelectedVariableType[0], "func f(node)"),
                },
                new Func<List<ICbValue>, DummyArgumentsStack, ICbValue>(
                    (argument, cagt) =>
                    {
                        if (dummyArgumentsControl.IsInvalid(cagt))
                            return null; // 実行環境が有効でない

                        try
                        {
                            var sample = GetArgumentList(argument, 0);
                            foreach (var node in sample)
                            {
                                dummyArgumentsControl.Enable(cagt, node.Data);    // 仮引数に引数を登録
                                if (IsCallBack(argument[1]))
                                    CallEvent(argument[1], cagt);
                                dummyArgumentsControl.Invalidated(cagt);    // 仮引数後処理
                            }
                        }
                        catch (Exception ex)
                        {
                            col.ExceptionFunc(null, ex);
                        }

                        return null;
                    }
                    )
                );

            return true;
        }
    }

    //-----------------------------------------------------------------
    class ConsoleOut : FuncAssetSub, IFuncAssetWithArgumentDef
    {
        public string AssetCode => nameof(ConsoleOut);

        public string HelpText { get; } = Language.GetInstance["ConsoleOut"];

        public string MenuTitle => "ConsoleOut";

        public string ValueType { get; } = CbSTUtils.FREE_TYPE_STR;

        public CbType[] DeleteSelectItems => CbScript.ObjectDeleteCbTypes;

        public bool ImplAsset(MultiRootConnector col, bool notheradMode = false)
        {
            col.MakeFunction(
                MenuTitle,
                HelpText,
                CbST.CbCreateTF(col.SelectedVariableType[0]),  // 返し値の型
                new List<ICbValue>()  // 引数
                {
                    CbST.CbCreate(col.SelectedVariableType[0], "n"),
                },
                new Func<List<ICbValue>, DummyArgumentsStack, ICbValue>(
                    (argument, cagt) =>
                    {
                        var ret = CbST.CbCreate(col.SelectedVariableType[0]);    // 返し値
                        try
                        {
                            ret.Set(argument[0]);
                            string str = argument[0].ValueString;
                            col.OwnerCommandCanvas.CommandCanvasControl.MainLog.OutLine(nameof(ConsoleOut), str);
                        }
                        catch (Exception ex)
                        {
                            col.ExceptionFunc(ret, ex);
                        }
                        return ret;
                    }
                    )
                );

            // 実行を可能にする
            col.LinkConnectorControl.IsRunable = true;

            return true;
        }
    }

    //-----------------------------------------------------------------
    class Abs : FuncAssetSub, IFuncAssetWithArgumentDef
    {
        public string AssetCode => nameof(Abs);

        public string HelpText { get; } = Language.GetInstance["Abs"];

        public string MenuTitle => AssetCode;

        public string ValueType { get; } = CbSTUtils.FREE_TYPE_STR;

        public CbType[] DeleteSelectItems => CbScript.MathDeleteNotCbSignedNumberTypes;

        public bool ImplAsset(MultiRootConnector col, bool notheradMode = false)
        {
            col.MakeFunction(
                MenuTitle,
                HelpText,
                CbST.CbCreateTF(col.SelectedVariableType[0]),  // 返し値の型
                new List<ICbValue>()  // 引数
                {
                    CbST.CbCreate(col.SelectedVariableType[0], "n"),
                },
                new Func<List<ICbValue>, DummyArgumentsStack, ICbValue>(
                    (argument, cagt) =>
                    {
                        var ret = CbST.CbCreate(col.SelectedVariableType[0]);    // 返し値
                        try
                        {
                            ret.Data = Math.Abs((dynamic)argument[0].Data);
                        }
                        catch (Exception ex)
                        {
                            col.ExceptionFunc(ret, ex);
                        }
                        return ret;
                    }
                    )
                );

            return true;
        }
    }

    //-----------------------------------------------------------------
    class GetVariableFromIndex : FuncAssetSub, IFuncCreateVariableListAssetDef
    {
        public string AssetCode => nameof(GetVariableFromIndex);

        public string HelpText { get; } = Language.GetInstance["GetVariableFromIndex"];

        public string MenuTitle => "Get VariableList[index]";

        public string ValueType { get; } = CbSTUtils.FREE_TYPE_STR;

        public CbType[] DeleteSelectItems => null;

        public bool ImplAsset(MultiRootConnector col, bool notheradMode = false)
        {
            VariableGetter variableGetter = new VariableGetter(col, (name) => "[ " + name + " [index] ]");
            if (variableGetter.IsError)
                return false;

            col.MakeFunction(
               variableGetter.MakeName,
               HelpText,
               col.ReturnValueTypeTF,// CbST.CreateTF(col.AssetLiteralType.GetObjectTypeNone()), // 返し値の型   Func<> 型の場合に返し値の型で接続するために ObjectType を落としている（方法に置き換える予定）
               new List<ICbValue>()       // 引数
               {
                   CbST.CbCreate<int>("index", 0),
               },
               new Func<List<ICbValue>, DummyArgumentsStack, ICbValue>(
                   (argument, cagt) =>
                   {
                       var ret = col.ReturnValueTypeTF();    // 返し値
                       try
                       {
                           int index = GetArgument<int>(argument, 0);

                           ICbValue cbVSValue = col.OwnerCommandCanvas.ScriptWorkStack.Find(variableGetter.Id);
                           var argList = (cbVSValue as ICbList).Value;

                           ret.CopyValue(argList[index]);
                           col.LinkConnectorControl.UpdateValueData();
                       }
                       catch (Exception ex)
                       {
                           col.ExceptionFunc(ret, ex);
                       }
                       return ret;
                   }
                   )
               );

            return true;
        }
    }

    //-----------------------------------------------------------------
    class SetVariableToIndex : FuncAssetSub, IFuncCreateVariableListAssetDef
    {
        public string AssetCode => nameof(SetVariableToIndex);

        public string HelpText { get; } = Language.GetInstance["SetVariableToIndex"];

        public string MenuTitle => "Set VariableList[index]";

        public string ValueType { get; } = CbSTUtils.FREE_TYPE_STR;

        public CbType[] DeleteSelectItems => null;

        public bool ImplAsset(MultiRootConnector col, bool notheradMode = false)
        {
            VariableGetter variableGetter = new VariableGetter(col, (name) => "[ " + name + " [index] ]");
            if (variableGetter.IsError)
                return false;

            col.MakeFunction(
               variableGetter.MakeName,
               HelpText,
               col.ReturnValueTypeTF,
               new List<ICbValue>()       // 引数
               {
                    CbST.CbCreate<int>("index", 0),
                    col.ReturnValueTypeNTF("n")//CbST.Create(col.AssetLiteralType.GetObjectTypeNone(), "n"),  // Func<> 型の場合に返し値の型で接続するために ObjectType を落としている（方法に置き換える予定）
               },
               new Func<List<ICbValue>, DummyArgumentsStack, ICbValue>(
                   (argument, cagt) =>
                   {
                       var ret = col.ReturnValueTypeTF();    // 返し値
                       try
                       {
                           int index = GetArgument<int>(argument, 0);

                           ICbValue cbVSValue = col.OwnerCommandCanvas.ScriptWorkStack.Find(variableGetter.Id);
                           var argList = (cbVSValue as ICbList).Value;

                           argList[index].CopyValue(argument[1]);
                           col.OwnerCommandCanvas.ScriptWorkStack.UpdateValueData(variableGetter.Id);
                           ret.Set(argument[1]);
                           col.LinkConnectorControl.UpdateValueData();

                       }
                       catch (Exception ex)
                       {
                           col.ExceptionFunc(ret, ex);
                       }
                       return ret;
                   }
                   )
               );

            return true;
        }
    }

    //-----------------------------------------------------------------
    class AppendVariableList : FuncAssetSub, IFuncCreateVariableListAssetDef
    {
        public string AssetCode => nameof(AppendVariableList);

        public string HelpText { get; } = Language.GetInstance["AppendVariableList"];

        public string MenuTitle => "Append VariableList";

        public string ValueType { get; } = CbSTUtils.FREE_TYPE_STR;

        public CbType[] DeleteSelectItems => null;

        public bool ImplAsset(MultiRootConnector col, bool notheradMode = false)
        {
            VariableGetter variableGetter = new VariableGetter(col, (name) => "Append [ " + name + " ]");
            if (variableGetter.IsError)
                return false;

            col.MakeFunction(
               variableGetter.MakeName,
               HelpText,
               CbST.CbCreateTF(col.SelectedVariableType[0]),   // 返し値の型
               new List<ICbValue>()   // 引数
               {
                    col.ReturnValueTypeNTF("n")//CbST.Create(col.AssetLiteralType.GetObjectTypeNone(), "n"), // Func<> 型の場合に返し値の型で接続するために ObjectType を落としている（方法に置き換える予定）
               },
               new Func<List<ICbValue>, DummyArgumentsStack, ICbValue>(
                   (argument, cagt) =>
                   {
                       var ret = CbST.CbCreate(col.SelectedVariableType[0]);    // 返し値
                       try
                       {
                           ICbValue cbVSValue = col.OwnerCommandCanvas.ScriptWorkStack.Find(variableGetter.Id);
                           var argList = cbVSValue as ICbList;
                           argList.Append(argument[0]);

                           col.OwnerCommandCanvas.ScriptWorkStack.UpdateValueData(variableGetter.Id);
                           argList.CopyTo(ret as ICbList);
                           col.LinkConnectorControl.UpdateValueData();
                       }
                       catch (Exception ex)
                       {
                           col.ExceptionFunc(ret, ex);
                       }
                       return ret;
                   }
                   )
               );

            return true;
        }
    }

    //-----------------------------------------------------------------
    class Count : FuncAssetSub, IFuncAssetWithArgumentDef
    {
        public string AssetCode => nameof(Count);

        public string HelpText { get; } = Language.GetInstance["Count"];

        public string MenuTitle => AssetCode;

        public string ValueType { get; } = CbSTUtils.FREE_TYPE_STR;

        public CbType[] DeleteSelectItems => CbScript.BaseDeleteCbTypes;

        public bool ImplAsset(MultiRootConnector col, bool notheradMode = false)
        {
            col.MakeFunction(
                MenuTitle,
                HelpText,
                CbST.CbCreateTF<int>(),   // 返し値の型
                new List<ICbValue>()          // 引数
                {
                    CbList.Create(col.SelectedVariableType[0], "sample"),
                },
                new Func<List<ICbValue>, DummyArgumentsStack, ICbValue>(
                    (argument, cagt) =>
                    {
                        var ret = CbST.CbCreate<int>();    // 返し値
                        try
                        {
                            ret.Data = (argument[0] as ICbList).Count;
                        }
                        catch (Exception ex)
                        {
                            col.ExceptionFunc(ret, ex);
                        }
                        return ret;
                    }
                    )
                );

            return true;
        }
    }

    //-----------------------------------------------------------------
    class Contains : FuncAssetSub, IFuncAssetWithArgumentDef
    {
        public string AssetCode => nameof(Contains);

        public string HelpText { get; } = Language.GetInstance["Contains"];

        public string MenuTitle => AssetCode;

        public string ValueType { get; } = CbSTUtils.FREE_TYPE_STR;

        public CbType[] DeleteSelectItems => CbScript.BaseDeleteCbTypes;

        public bool ImplAsset(MultiRootConnector col, bool notheradMode = false)
        {
            col.MakeFunction(
                MenuTitle,
                HelpText,
                CbST.CbCreateTF<bool>(),  // 返し値の型
                new List<ICbValue>()          // 引数
                {
                    CbList.Create(col.SelectedVariableType[0], "sample"),
                    CbST.CbCreate(col.SelectedVariableType[0], "n"),
                },
                new Func<List<ICbValue>, DummyArgumentsStack, ICbValue>(
                    (argument, cagt) =>
                    {
                        var ret = CbBool.Create(false);    // 返し値
                        try
                        {
                            ret.Data = (argument[0] as ICbList).Contains(argument[1]);
                        }
                        catch (Exception ex)
                        {
                            col.ExceptionFunc(ret, ex);
                        }
                        return ret;
                    }
                    )
                );

            return true;
        }
    }

    //-----------------------------------------------------------------
    class GetListIndex : FuncAssetSub, IFuncAssetWithArgumentDef
    {
        public string AssetCode => nameof(GetListIndex);

        public string HelpText { get; } = Language.GetInstance["GetListIndex"];

        public string MenuTitle => "Get List[index]";

        public string ValueType { get; } = CbSTUtils.FREE_TYPE_STR;

        public CbType[] DeleteSelectItems => CbScript.BaseDeleteCbTypes;

        public bool ImplAsset(MultiRootConnector col, bool notheradMode = false)
        {
            col.MakeFunction(
                MenuTitle,
                HelpText,
                CbST.CbCreateTF(col.SelectedVariableType[0]),    // 返し値の型
                new List<ICbValue>()  // 引数
                {
                    CbList.Create(col.SelectedVariableType[0], "sample"),
                    CbST.CbCreate<int>("index"),
                },
                new Func<List<ICbValue>, DummyArgumentsStack, ICbValue>(
                    (argument, cagt) =>
                    {
                        var ret = CbST.CbCreate(col.SelectedVariableType[0]);    // 返し値
                        try
                        {
                            int index = GetArgument<int>(argument, 1);
                            ret.Data = (argument[0] as ICbList)[index].Data;
                        }
                        catch (Exception ex)
                        {
                            col.ExceptionFunc(ret, ex);
                        }
                        return ret;
                    }
                    )
                );

            return true;
        }
    }

    //-----------------------------------------------------------------
    class GetListLast : FuncAssetSub, IFuncAssetWithArgumentDef
    {
        public string AssetCode => nameof(GetListLast);

        public string HelpText { get; } = Language.GetInstance["GetListLast"];

        public string MenuTitle => "Get List[last]";

        public string ValueType { get; } = CbSTUtils.FREE_TYPE_STR;

        public CbType[] DeleteSelectItems => CbScript.BaseDeleteCbTypes;

        public bool ImplAsset(MultiRootConnector col, bool notheradMode = false)
        {
            col.MakeFunction(
                MenuTitle,
                HelpText,
                CbST.CbCreateTF(col.SelectedVariableType[0]),  // 返し値の型
                new List<ICbValue>()  // 引数
                {
                    CbList.Create(col.SelectedVariableType[0], "sample"),
                },
                new Func<List<ICbValue>, DummyArgumentsStack, ICbValue>(
                    (argument, cagt) =>
                    {
                        var ret = CbST.CbCreate(col.SelectedVariableType[0]);    // 返し値
                        try
                        {
                            var argList = argument[0] as ICbList;
                            ret.Data = argList[argList.Count - 1].Data;
                        }
                        catch (Exception ex)
                        {
                            col.ExceptionFunc(ret, ex);
                        }
                        return ret;
                    }
                    )
                );

            return true;
        }
    }

    //-----------------------------------------------------------------
    class SetListIndex : FuncAssetSub, IFuncAssetWithArgumentDef
    {
        public string AssetCode => nameof(SetListIndex);

        public string HelpText { get; } = Language.GetInstance["SetListIndex"];

        public string MenuTitle => "Set List[index]";

        public string ValueType { get; } = CbSTUtils.FREE_TYPE_STR;

        public CbType[] DeleteSelectItems => CbScript.BaseDeleteCbTypes;

        public bool ImplAsset(MultiRootConnector col, bool notheradMode = false)
        {
            col.MakeFunction(
                MenuTitle,
                HelpText,
                CbST.CbCreateTF(col.SelectedVariableType[0]),  // 返し値の型
                new List<ICbValue>()  // 引数
                {
                    CbList.Create(col.SelectedVariableType[0], "sample"),
                    CbST.CbCreate<int>("index"),
                    CbST.CbCreate(col.SelectedVariableType[0], "n"),
                },
                new Func<List<ICbValue>, DummyArgumentsStack, ICbValue>(
                    (argument, cagt) =>
                    {
                        var ret = CbST.CbCreate(col.SelectedVariableType[0]);    // 返し値
                        try
                        {
                            int index = GetArgument<int>(argument, 1);
                            (argument[0] as ICbList)[index].Data = argument[2].Data;
                        }
                        catch (Exception ex)
                        {
                            col.ExceptionFunc(ret, ex);
                        }
                        return ret;
                    }
                    )
                );

            return true;
        }
    }

    //-----------------------------------------------------------------
    class Append : FuncAssetSub, IFuncAssetWithArgumentDef
    {
        public string AssetCode => nameof(Append);

        public string HelpText { get; } = Language.GetInstance["Append"];

        public string MenuTitle => AssetCode;

        public string ValueType { get; } = CbSTUtils.FREE_TYPE_STR;

        public CbType[] DeleteSelectItems => CbScript.BaseDeleteCbTypes;

        public bool ImplAsset(MultiRootConnector col, bool notheradMode = false)
        {
            col.MakeFunction(
               MenuTitle,
               HelpText,
               CbList.CreateTF(col.SelectedVariableType[0]),  // 返し値の型
               new List<ICbValue>()   // 引数
               {
                    CbList.Create(col.SelectedVariableType[0], "list"),
                    col.ReturnValueTypeNTF("n")//CbST.Create(col.AssetLiteralType.GetObjectTypeNone(), "n"), // Func<> 型の場合に返し値の型で接続するために ObjectType を落としている（方法に置き換える予定）
               },
               new Func<List<ICbValue>, DummyArgumentsStack, ICbValue>(
                   (argument, cagt) =>
                   {
                       var ret = CbList.Create(col.SelectedVariableType[0]);    // 返し値
                       try
                       {
                           var argList = argument[0] as ICbList;
                           argList.CopyTo(ret as ICbList);
                           (ret as ICbList).Append(argument[1]);

                           col.LinkConnectorControl.UpdateValueData();
                       }
                       catch (Exception ex)
                       {
                           col.ExceptionFunc(ret, ex);
                       }
                       return ret;
                   }
                   )
               );

            return true;
        }
    }

    //-----------------------------------------------------------------
    class Pow : FuncAssetSub, IFuncAssetWithArgumentDef
    {
        public string AssetCode => nameof(Pow);

        public string HelpText { get; } = Language.GetInstance["Pow"];

        public string MenuTitle => AssetCode;

        public string ValueType { get; } = typeof(double).FullName;

        public CbType[] DeleteSelectItems => null;

        public bool ImplAsset(MultiRootConnector col, bool notheradMode = false)
        {
            col.MakeFunction(
                MenuTitle,
                HelpText,
                CbST.CbCreateTF<double>(),      // 返し値の型
                new List<ICbValue>()  // 引数
                {
                    CbST.CbCreate<double>("n"),
                    CbST.CbCreate<double>("p"),
                },
                new Func<List<ICbValue>, DummyArgumentsStack, ICbValue>(
                    (argument, cagt) =>
                    {
                        var ret = CbST.CbCreate<double>();    // 返し値
                        try
                        {
                            var n = GetArgument<double>(argument, 0);
                            var p = GetArgument<double>(argument, 1);

                            ret.Data = Math.Pow(n, p);
                        }
                        catch (Exception ex)
                        {
                            col.ExceptionFunc(ret, ex);
                        }
                        return ret;
                    }
                    )
                );

            return true;
        }
    }

    //-----------------------------------------------------------------
    class Rand : FuncAssetSub, IFuncAssetWithArgumentDef
    {
        public string AssetCode => nameof(Rand);

        public string HelpText { get; } = Language.GetInstance["Rand"];

        public string MenuTitle => AssetCode;

        public string ValueType { get; } = typeof(int).FullName;

        public CbType[] DeleteSelectItems => null;

        public bool ImplAsset(MultiRootConnector col, bool notheradMode = false)
        {
            col.MakeFunction(
                MenuTitle,
                HelpText,
                CbST.CbCreateTF<int>(),   // 返し値の型
                new List<ICbValue>()  // 引数
                {
                    CbST.CbCreate<int>("max", 0),
                    CbST.CbCreate<int>("int", 1),
                },
                new Func<List<ICbValue>, DummyArgumentsStack, ICbValue>(
                    (argument, cagt) =>
                    {
                        var ret = CbST.CbCreate<int>();   // 返し値
                        try
                        {
                            var min = GetArgument<int>(argument, 0);
                            var max = GetArgument<int>(argument, 1);
                            ret.Data = random.Next(min, max + 1);
                        }
                        catch (Exception ex)
                        {
                            col.ExceptionFunc(ret, ex);
                        }
                        return ret;
                    }
                    )
                );

            return true;
        }
    }
}
