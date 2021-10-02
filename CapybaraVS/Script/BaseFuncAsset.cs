using CapybaraVS.Controls;
using CapybaraVS.Controls.BaseControls;
using CapybaraVS.Script.Lib;
using CapyCSS.Controls;
using CapyCSS.Script.Lib;
using CbVS.Script;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;
using static CapybaraVS.Controls.BaseControls.CommandCanvas;
using static CapybaraVS.Controls.MultiRootConnector;

namespace CapybaraVS.Script
{
    /// <summary>
    /// スクリプトの機能をインポートしメニューに登録するクラスです。
    /// </summary>
    public class ApiImporter : ImplementAsset
    {
        private TreeMenuNode ProgramNode = null;
        private TreeMenuNode DotNet = null;
        private TreeMenuNode DllNode = null;
        private TreeMenuNode NuGetNode = null;
        private CommandCanvas OwnerCommandCanvas = null;
        public ObservableCollection<string> ModulueNameList = new ObservableCollection<string>();
        public List<string> DllModulePathList = new List<string>();
        public List<string> PackageModuleList = new List<string>();
        public List<string> ClassModuleList = new List<string>();
        public List<string> NuGetModuleList = new List<string>();

        public const string BASE_LIB_TAG_PRE = "BaseLib:";

        public const string MENU_TITLE_PROGRAM = "Program";
        public const string MENU_TITLE_DOT_NET_FUNCTION = "Function";
        public const string MENU_TITLE_DOT_NET_STANDERD = "Standard";
        public const string MENU_TITLE_DOT_NET_FUNCTION_FULL_PATH = MENU_TITLE_PROGRAM + "." + MENU_TITLE_DOT_NET_FUNCTION + ".";
        public const string MENU_TITLE_DOT_NET_STANDERD_FULL_PATH = MENU_TITLE_PROGRAM + "." + MENU_TITLE_DOT_NET_FUNCTION + "." + MENU_TITLE_DOT_NET_STANDERD + ".";

        public ApiImporter(CommandCanvas ownerCommandCanvas)
        {
            OwnerCommandCanvas = ownerCommandCanvas;
            ProgramNode = CreateGroup(ownerCommandCanvas, MENU_TITLE_PROGRAM);

            CreateAssetMenu(ownerCommandCanvas, ProgramNode, new Sequence());

            {
                var literalNode = CreateGroup(ProgramNode, Script_Literal.LIB_Script_literal_NAME);
                CreateAssetMenu(ownerCommandCanvas, literalNode, new LiteralType());
                CreateAssetMenu(ownerCommandCanvas, literalNode, new LiteralListType());
            }

            {
                var variableNode = CreateGroup(ProgramNode, "Variable");
                CreateAssetMenu(ownerCommandCanvas, variableNode, new CreateVariable());
                CreateAssetMenu(ownerCommandCanvas, variableNode, new CreateFuncVariable());
                CreateAssetMenu(ownerCommandCanvas, variableNode, new CreateNullableVariable());
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
                DotNet = CreateGroup(ProgramNode, MENU_TITLE_DOT_NET_FUNCTION);

                {
                    var flowOperation = CreateGroup(DotNet, FlowLib.LIB_FLOW_NAME);
                    CreateAssetMenu(ownerCommandCanvas, flowOperation, new SwitchEnum());
                }

                {
                    var funcNode = CreateGroup(DotNet, FlowLib.LIB_Fx_NAME);
                    CreateAssetMenu(ownerCommandCanvas, funcNode, new DummyArguments());
                }

                {
                    var math = CreateGroup(DotNet, MathLib.LIB_MATH_NAME);
                    var mathNode = CreateGroup(math, "Base");
                    CreateAssetMenu(ownerCommandCanvas, mathNode, new Abs());
                    CreateAssetMenu(ownerCommandCanvas, mathNode, new Inc());
                    CreateAssetMenu(ownerCommandCanvas, mathNode, new Dec());
                    CreateAssetMenu(ownerCommandCanvas, mathNode, new Sum());
                    CreateAssetMenu(ownerCommandCanvas, mathNode, new Subtract());
                    CreateAssetMenu(ownerCommandCanvas, mathNode, new Multiply());
                    CreateAssetMenu(ownerCommandCanvas, mathNode, new Divide());
                    CreateAssetMenu(ownerCommandCanvas, mathNode, new Pow());
                    CreateAssetMenu(ownerCommandCanvas, mathNode, new Modulo());
                    CreateAssetMenu(ownerCommandCanvas, mathNode, new Rand());
                }

                {
                    var script = CreateGroup(DotNet, CapyCSS.Script.Lib.Script.LIB_Script_NAME);
                    CreateAssetMenu(ownerCommandCanvas, script, new ScriptDispose());
                }

                {
                    var io = CreateGroup(DotNet, EnvironmentLib.LIB_IO_NAME);
                    var conOut = CreateGroup(io, "OutConsole");
                    CreateAssetMenu(ownerCommandCanvas, conOut, new OutConsole());
                }

                {
                    var tools = CreateGroup(DotNet, "Exec");
                    CreateAssetMenu(ownerCommandCanvas, tools, new CallFile());
                }

                ScriptImplement.ImportScriptMethods(ownerCommandCanvas, DotNet);
            }
        }

        /// <summary>
        /// スクリプトに基本のクラスをインポートします。
        /// </summary>
        /// <param name="className">クラス名</param>
        /// <returns>true==成功</returns>
        public bool ImportBase()
        {
            ScriptImplement.ImportScriptMethodsForBase(OwnerCommandCanvas, CreateGroup(DotNet, MENU_TITLE_DOT_NET_STANDERD));
            return true;
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
        public string MenuTitle => "Literal : T";

        public string HelpText => Language.Instance[ApiImporter.BASE_LIB_TAG_PRE + nameof(LiteralType)];

        public List<TypeRequest> typeRequests => new List<TypeRequest>()
        {
            new TypeRequest(t => !t.IsAbstract)
        };
    }

    //-----------------------------------------------------------------
    class LiteralListType : IFuncAssetLiteralDef
    {
        public string MenuTitle => $"Literal List : {CbSTUtils.LITERAL_LIST_STR}<T>";

        public string HelpText => Language.Instance[ApiImporter.BASE_LIB_TAG_PRE + nameof(LiteralListType)];

        public List<TypeRequest> typeRequests => new List<TypeRequest>()
        { 
            new TypeRequest(CbSTUtils.LIST_TYPE, t => CbScript.AcceptAll(t))
        };
    }

    //-----------------------------------------------------------------
    class Sum : FuncAssetSub, IFuncAssetWithArgumentDef
    {
        public string AssetCode => nameof(Sum);

        public string HelpText => Language.Instance[ApiImporter.BASE_LIB_TAG_PRE + AssetCode];

        public string MenuTitle => $"{AssetCode}({CbSTUtils.LIST_STR}<T>) : T";

        public List<TypeRequest> typeRequests => new List<TypeRequest>()
        {
            new TypeRequest(CbSTUtils.LIST_INTERFACE_TYPE, t => CbScript.IsCalcable(t) || t == typeof(string))
        };

        public bool ImplAsset(MultiRootConnector col, bool isReBuildMode = false)
        {
            col.MakeFunction(
                $"{AssetCode}<{col.SelectedVariableTypeName[0]}>",
                HelpText,
                CbST.CbCreateTF(col.SelectedVariableType[0].GenericTypeArguments[0]),  // 返し値の型
                new List<ICbValue>()  // 引数
                {
                    CbST.CbCreate(col.SelectedVariableType[0], "samples"),
                },
                new Func<List<ICbValue>, DummyArgumentsStack, ICbValue>(
                    (argument, cagt) =>
                    {
                        var ret = CbST.CbCreate(col.SelectedVariableType[0].GenericTypeArguments[0]);    // 返し値
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
    class Sequence : FuncAssetSub, IFuncAssetWithArgumentDef
    {
        public string AssetCode => nameof(Sequence);

        public string HelpText => Language.Instance[ApiImporter.BASE_LIB_TAG_PRE + AssetCode];

        public string MenuTitle => $"{AssetCode}({CbSTUtils.LIST_STR}<T>) : T";

        public List<TypeRequest> typeRequests => new List<TypeRequest>()
        { 
            new TypeRequest(CbSTUtils.LIST_INTERFACE_TYPE, t => CbScript.AcceptAll(t))
        };

        public bool ImplAsset(MultiRootConnector col, bool isReBuildMode = false)
        {
            col.MakeFunction(
                $"{AssetCode}<{col.SelectedVariableTypeName[0]}>",
                HelpText,
                CbST.CbCreateTF(col.SelectedVariableType[0].GenericTypeArguments[0]),  // 返し値の型
                new List<ICbValue>()  // 引数
                {
                    CbST.CbCreate(col.SelectedVariableType[0], "call list"),
                },
                new Func<List<ICbValue>, DummyArgumentsStack, ICbValue>(
                    (argument, cagt) =>
                    {
                        var ret = CbST.CbCreate(col.SelectedVariableType[0].GenericTypeArguments[0]);    // 返し値
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

            if (!isReBuildMode)
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

        public string HelpText => Language.Instance[ApiImporter.BASE_LIB_TAG_PRE + AssetCode];

        public string MenuTitle => $"{AssetCode}(T) : T";

        public List<TypeRequest> typeRequests => new List<TypeRequest>()
        {
            new TypeRequest(t => CbScript.IsCalcable(t))
        };

        public bool ImplAsset(MultiRootConnector col, bool isReBuildMode = false)
        {
            col.MakeFunction(
                $"{AssetCode}<{col.SelectedVariableTypeName[0]}>",
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

        public string HelpText => Language.Instance[ApiImporter.BASE_LIB_TAG_PRE + AssetCode];

        public string MenuTitle => $"{AssetCode}(T) : T";

        public List<TypeRequest> typeRequests => new List<TypeRequest>()
        {
            new TypeRequest(t => CbScript.IsCalcable(t))
        };

        public bool ImplAsset(MultiRootConnector col, bool isReBuildMode = false)
        {
            col.MakeFunction(
                $"{AssetCode}<{col.SelectedVariableTypeName[0]}>",
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
    class Modulo : FuncAssetSub, IFuncAssetWithArgumentDef
    {
        public string AssetCode => nameof(Modulo);

        public string HelpText => Language.Instance[ApiImporter.BASE_LIB_TAG_PRE + AssetCode];

        public string MenuTitle => $"{AssetCode}(T, T) : T";

        public List<TypeRequest> typeRequests => new List<TypeRequest>()
        {
            new TypeRequest(t => CbScript.IsCalcable(t))
        };

        public bool ImplAsset(MultiRootConnector col, bool isReBuildMode = false)
        {
            col.MakeFunction(
                $"{AssetCode}<{col.SelectedVariableTypeName[0]}, {col.SelectedVariableTypeName[0]}>",
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
    class Multiply : FuncAssetSub, IFuncAssetWithArgumentDef
    {
        public string AssetCode => nameof(Multiply);

        public string HelpText => Language.Instance[ApiImporter.BASE_LIB_TAG_PRE + AssetCode];

        public string MenuTitle => $"{AssetCode}(T, {CbSTUtils.LIST_STR}<T>) : T";

        public List<TypeRequest> typeRequests => new List<TypeRequest>()
        {
            new TypeRequest(CbSTUtils.LIST_INTERFACE_TYPE, t => CbScript.IsCalcable(t))
        };

        public bool ImplAsset(MultiRootConnector col, bool isReBuildMode = false)
        {
            col.MakeFunction(
                $"{AssetCode}<{CbSTUtils.GetTypeName(col.SelectedVariableType[0].GenericTypeArguments[0])}, {col.SelectedVariableTypeName[0]}>",
                HelpText,
                CbST.CbCreateTF(col.SelectedVariableType[0].GenericTypeArguments[0]),  // 返し値の型
                new List<ICbValue>()  // 引数
                {
                    CbST.CbCreate(col.SelectedVariableType[0].GenericTypeArguments[0], "base"),
                    CbST.CbCreate(col.SelectedVariableType[0], "samples"),
                },
                new Func<List<ICbValue>, DummyArgumentsStack, ICbValue>(
                    (argument, cagt) =>
                    {
                        var ret = CbST.CbCreate(col.SelectedVariableType[0].GenericTypeArguments[0]);    // 返し値
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
    class Divide : FuncAssetSub, IFuncAssetWithArgumentDef
    {
        public string AssetCode => nameof(Divide);

        public string HelpText => Language.Instance[ApiImporter.BASE_LIB_TAG_PRE + AssetCode];

        public string MenuTitle => $"{AssetCode}(T, {CbSTUtils.LIST_STR}<T>) : T";

        public List<TypeRequest> typeRequests => new List<TypeRequest>()
        {
            new TypeRequest(CbSTUtils.LIST_INTERFACE_TYPE, t => CbScript.IsCalcable(t))
        };

        public bool ImplAsset(MultiRootConnector col, bool isReBuildMode = false)
        {
            col.MakeFunction(
                $"{AssetCode}<{CbSTUtils.GetTypeName(col.SelectedVariableType[0].GenericTypeArguments[0])}, {col.SelectedVariableTypeName[0]}>",
                HelpText,
                CbST.CbCreateTF(col.SelectedVariableType[0].GenericTypeArguments[0]),  // 返し値の型
                new List<ICbValue>()  // 引数
                {
                    CbST.CbCreate(col.SelectedVariableType[0].GenericTypeArguments[0], "base"),
                    CbST.CbCreate(col.SelectedVariableType[0], "samples"),
                },
                new Func<List<ICbValue>, DummyArgumentsStack, ICbValue>(
                    (argument, cagt) =>
                    {
                        var ret = CbST.CbCreate(col.SelectedVariableType[0].GenericTypeArguments[0]);    // 返し値
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
    class Subtract : FuncAssetSub, IFuncAssetWithArgumentDef
    {
        public string AssetCode => nameof(Subtract);

        public string HelpText => Language.Instance[ApiImporter.BASE_LIB_TAG_PRE + AssetCode];

        public string MenuTitle => $"{AssetCode}(T, {CbSTUtils.LIST_STR}<T>) : T";

        public List<TypeRequest> typeRequests => new List<TypeRequest>()
        {
            new TypeRequest(CbSTUtils.LIST_INTERFACE_TYPE, t => CbScript.IsCalcable(t))
        };

        public bool ImplAsset(MultiRootConnector col, bool isReBuildMode = false)
        {
            col.MakeFunction(
                $"{AssetCode}<{CbSTUtils.GetTypeName(col.SelectedVariableType[0].GenericTypeArguments[0])}, {col.SelectedVariableTypeName[0]}>",
                HelpText,
                CbST.CbCreateTF(col.SelectedVariableType[0].GenericTypeArguments[0]),  // 返し値の型
                new List<ICbValue>()  // 引数
                {
                    CbST.CbCreate(col.SelectedVariableType[0].GenericTypeArguments[0], "base"),
                    CbST.CbCreate(col.SelectedVariableType[0], "samples"),
                },
                new Func<List<ICbValue>, DummyArgumentsStack, ICbValue>(
                    (argument, cagt) =>
                    {
                        var ret = CbST.CbCreate(col.SelectedVariableType[0].GenericTypeArguments[0]);    // 返し値
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

        public string HelpText => Language.Instance[ApiImporter.BASE_LIB_TAG_PRE + AssetCode];

        public string MenuTitle => $"{AssetCode}({CbSTUtils.STRING_STR}, {CbSTUtils.BOOL_STR}, {CbSTUtils.LIST_STR}<{CbSTUtils.STRING_STR}>) : {CbSTUtils.FUNC_STR}<{CbSTUtils.INT_STR}>";

        public List<TypeRequest> typeRequests => new List<TypeRequest>()
        {
            new TypeRequest(typeof(string))
        };

        public bool ImplAsset(MultiRootConnector col, bool isReBuildMode = false)
        {
            col.MakeFunction(
                $"{AssetCode}",
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

                            ret.Callback = (cagt2) =>
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
    class _GetVariable : FuncAssetSub
    {
        public string AssetCode => nameof(GetVariable);

        public string HelpText => Language.Instance[ApiImporter.BASE_LIB_TAG_PRE + nameof(_GetVariable)];

        public bool ImplAsset(MultiRootConnector col, bool isReBuildMode = false)
        {
            using (VariableGetter variableGetter = new VariableGetter(col))
            {
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
                            try
                            {
                                ICbValue cbVSValue = col.OwnerCommandCanvas.ScriptWorkStack.Find(variableGetter.Id);
                                col.LinkConnectorControl.UpdateValueData();
                                if (!cbVSValue.IsDelegate)
                                {
                                    if (cbVSValue.IsList)
                                    {
                                        ICbList cbList = cbVSValue.GetListValue;
                                        cbVSValue.ReturnAction = (value) =>
                                        {
                                            cbList.CopyFrom(value);
                                        };
                                    }
                                    else if (!(cbVSValue is ICbClass))
                                    {
                                        cbVSValue.ReturnAction = (value) =>
                                        {
                                            cbVSValue.Data = value;
                                        };
                                    }
                                }
                                cbVSValue.IsLiteral = false;

                                // スクリプト処理後に変数の値変化を反映する（参照渡し対応）
                                col.OwnerCommandCanvas.ScriptWorkStack.UpdateValueData(variableGetter.Id);
                                return cbVSValue;
                            }
                            catch (Exception ex)
                            {
                                ICbValue ret = CbST.CbCreate(col.SelectedVariableType[0]);    // 返し値
                                col.ExceptionFunc(ret, ex);
                                return ret;
                            }
                        }
                        )
                    );
                return true;
            }
        }
    }

    //-----------------------------------------------------------------
    class CreateVariable : _GetVariable, IFuncCreateVariableAssetDef
    {
        public string MenuTitle => "Create Variable : T";

        public new string HelpText => Language.Instance[ApiImporter.BASE_LIB_TAG_PRE + nameof(CreateVariable)];

        public List<TypeRequest> typeRequests => new List<TypeRequest>()
        {
            new TypeRequest(t => true)    // 新規作成を意味する
        };
    }

    //-----------------------------------------------------------------
    class CreateVariableList : _GetVariable, IFuncCreateVariableAssetDef
    {
        public string MenuTitle => $"Create Variable List : {CbSTUtils.LITERAL_LIST_STR}<T>";

        public new string HelpText => Language.Instance[ApiImporter.BASE_LIB_TAG_PRE + nameof(CreateVariableList)];

        public List<TypeRequest> typeRequests => new List<TypeRequest>()
        {
            new TypeRequest(CbSTUtils.LIST_TYPE, t => true)
        };
    }

    //-----------------------------------------------------------------
    class CreateFuncVariable : _GetVariable, IFuncCreateVariableAssetDef
    {
        public new string HelpText => Language.Instance[ApiImporter.BASE_LIB_TAG_PRE + nameof(CreateFuncVariable)];

        public string MenuTitle => $"Create Variable : {CbSTUtils.FUNC_STR}<T>";

        public List<TypeRequest> typeRequests => new List<TypeRequest>()
        {
            new TypeRequest(CbSTUtils.FUNC_TYPE, t => true)
        };
    }

    //-----------------------------------------------------------------
    class CreateNullableVariable : _GetVariable, IFuncCreateVariableAssetDef
    {
        public new string HelpText => Language.Instance[ApiImporter.BASE_LIB_TAG_PRE + nameof(CreateNullableVariable)];

        public string MenuTitle => $"Create Variable : T?";

        public List<TypeRequest> typeRequests => new List<TypeRequest>()
        {
            new TypeRequest(CbSTUtils.NULLABLE_TYPE, t => true)
        };
    }

    //-----------------------------------------------------------------
    class GetVariable : _GetVariable, IFuncCreateVariableAssetDef
    {
        public string MenuTitle => "Get Variable";

        public new string HelpText => Language.Instance[ApiImporter.BASE_LIB_TAG_PRE + nameof(GetVariable)];

        public List<TypeRequest> typeRequests => null;    // 選択を意味する
    }

    //-----------------------------------------------------------------
    class SetVariable : FuncAssetSub, IFuncCreateVariableAssetDef
    {
        public string AssetCode => nameof(SetVariable);

        public string HelpText => Language.Instance[ApiImporter.BASE_LIB_TAG_PRE + nameof(SetVariable)];

        public string MenuTitle => "Set Variable";

        public List<TypeRequest> typeRequests => null;    // 選択を意味する

        public bool ImplAsset(MultiRootConnector col, bool isReBuildMode = false)
        {
            using (VariableGetter variableGetter = new VariableGetter(col))
            {
                if (variableGetter.IsError)
                    return false;

                col.MakeFunction(
                    variableGetter.MakeName,
                    HelpText,
                    CbVoid.TF,  // 返し値の型
                    new List<ICbValue>()  // 引数
                    {
                    CbST.CbCreate(col.SelectedVariableType[0], "n"),
                    },
                    new Func<List<ICbValue>, DummyArgumentsStack, ICbValue>(
                        (argument, cagt) =>
                        {
                            try
                            {
                                ICbValue cbVSValue = col.OwnerCommandCanvas.ScriptWorkStack.Find(variableGetter.Id);
                                if (argument[0].IsLiteral)
                                {
                                    if (argument[0].IsList && cbVSValue.IsList)
                                    {
                                        // リストのコピー

                                        ICbList cbList = argument[0].GetListValue;
                                        ICbList toList = cbVSValue.GetListValue;
                                        toList.CopyFrom(cbList);
                                    }
                                    else
                                    {
                                        // 値のコピー

                                        cbVSValue.Set(argument[0]);
                                    }
                                }
                                else
                                {
                                    // 変数の中身を代入

                                    col.OwnerCommandCanvas.ScriptWorkStack.FindSet(variableGetter.Id, argument[0]);
                                }
                                col.OwnerCommandCanvas.ScriptWorkStack.UpdateValueData(variableGetter.Id);
                                col.LinkConnectorControl.UpdateValueData();
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
    }

    //-----------------------------------------------------------------
    class DummyArguments : FuncAssetSub, IFuncAssetWithArgumentDef
    {
        public string AssetCode => nameof(DummyArguments);

        public string HelpText => Language.Instance[ApiImporter.BASE_LIB_TAG_PRE + nameof(DummyArguments)];

        public string MenuTitle => "DummyArguments<T> : T";

        public List<TypeRequest> typeRequests => new List<TypeRequest>()
        {
            new TypeRequest(t => CbScript.AcceptAll(t))
        };

        public bool ImplAsset(MultiRootConnector col, bool isReBuildMode = false)
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
    class SwitchEnum : FuncAssetSub, IFuncAssetWithArgumentDef
    {
        public string AssetCode => nameof(SwitchEnum);

        public string HelpText => Language.Instance[ApiImporter.BASE_LIB_TAG_PRE + nameof(SwitchEnum)];

        public string MenuTitle => $"Switch Case<TEnum> : {CbSTUtils.VOID_STR}";

        public List<TypeRequest> typeRequests => new List<TypeRequest>()
        {
            new TypeRequest(t => CbScript.IsEnum(t))
        };

        public bool ImplAsset(MultiRootConnector col, bool isReBuildMode = false)
        {
            // メソッドの引数を作成する
            var args = new List<ICbValue>();
            args.Add(
                CbST.CbCreate(col.SelectedVariableType[0], "target")
                );
            var caseList = CbST.CbCreate<List<Action>>("case list") as ICbList;
            if (!isReBuildMode)
            {
                // case 要素を作成する
                foreach (string name in Enum.GetNames(col.SelectedVariableType[0]))
                {
                    caseList.Append(
                        CbST.CbCreate<Action>("")
                        ).Name = $":{name}";    // Append では名前がコピーされないので後から設定する
                }
            }
            caseList.CastType = typeof(IEnumerable<Action>);  // キャスト
            args.Add(caseList);
            args.Add(
                CbST.CbCreate<Action>("default")
                );
            caseList.AddLock = true;

            col.MakeFunction(
                $"Switch Case<{col.SelectedVariableTypeName[0]}>",
                HelpText,
                CbVoid.TF,  // 返し値の型
                args,  // 引数
                new Func<List<ICbValue>, DummyArgumentsStack, ICbValue>(
                    (argument, cagt) =>
                    {
                        try
                        {
                            // 選択した要素名を作成
                            ICbEnum select = argument[0] as ICbEnum;
                            string selectName = select.SelectedItemName;
                            if (selectName.Contains('.'))
                            {
                                var tokens = selectName.Split('.');
                                selectName = tokens[tokens.Length - 1];
                            }
                            selectName = $":{selectName}";

                            // 一致する要素のコールバックを呼ぶ
                            var caseList = GetArgumentList(argument, 1);
                            foreach (var caseNode in caseList)
                            {
                                if (caseNode.Name == selectName)
                                {
                                    TryCallCallBack(cagt, caseNode);
                                    return null;
                                }
                            }

                            // どれにも一致しなかった場合のコールバックを呼ぶ
                            TryCallCallBack(cagt, argument[2]);
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
    class OutConsole : FuncAssetSub, IFuncAssetWithArgumentDef
    {
        public string AssetCode => nameof(OutConsole);

        public string HelpText => Language.Instance[ApiImporter.BASE_LIB_TAG_PRE + nameof(OutConsole)];

        public string MenuTitle => $"{AssetCode}(T) : T";

        public List<TypeRequest> typeRequests => new List<TypeRequest>()
        {
            new TypeRequest(t => CbScript.IsNotObject(t))
        };

        public bool ImplAsset(MultiRootConnector col, bool isReBuildMode = false)
        {
            col.MakeFunction(
                $"{AssetCode}<{col.SelectedVariableTypeName[0]}>",
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
                            string str = argument[0].ValueUIString;
                            Console.WriteLine(str);
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

        public string HelpText => Language.Instance[ApiImporter.BASE_LIB_TAG_PRE + nameof(Abs)];

        public string MenuTitle => $"{AssetCode}(T) : T";

        public List<TypeRequest> typeRequests => new List<TypeRequest>()
        {
            new TypeRequest(t => CbScript.IsSigned(t))
        };

        public bool ImplAsset(MultiRootConnector col, bool isReBuildMode = false)
        {
            col.MakeFunction(
                $"{AssetCode}<{col.SelectedVariableTypeName[0]}>",
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
    class ScriptDispose : FuncAssetSub, IFuncAssetWithArgumentDef
    {
        public string AssetCode => "Dispose";

        public string HelpText => Language.Instance[ApiImporter.BASE_LIB_TAG_PRE + AssetCode];

        public string MenuTitle => $"{AssetCode}(IDisposable) : void";

        public List<TypeRequest> typeRequests => new List<TypeRequest>()
        {
            new TypeRequest(t => CbScript.IsDisposable(t))
        };

        public bool ImplAsset(MultiRootConnector col, bool isReBuildMode = false)
        {
            col.MakeFunction(
                $"{AssetCode}",
                HelpText,
                CbVoid.TF,  // 返し値の型
                new List<ICbValue>()  // 引数
                {
                    CbClass.ClassValue(typeof(IDisposable), "disposable"),
                },
                new Func<List<ICbValue>, DummyArgumentsStack, ICbValue>(
                    (argument, cagt) =>
                    {
                        try
                        {
                            IDisposable disposable = argument[0].Data as IDisposable;
                            disposable.Dispose();
                            argument[0].Data = null;    // UI が Dispose したインスタンスを参照しないようにする
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
    class GetVariableFromIndex : FuncAssetSub, IFuncCreateVariableListAssetDef
    {
        public string AssetCode => nameof(GetVariableFromIndex);

        public string HelpText => Language.Instance[ApiImporter.BASE_LIB_TAG_PRE + nameof(GetVariableFromIndex)];

        public string MenuTitle => "Get VariableList[index]";

        public List<TypeRequest> typeRequests => new List<TypeRequest>()
        {
            new TypeRequest(t => CbScript.AcceptAll(t))
        };

        public bool ImplAsset(MultiRootConnector col, bool isReBuildMode = false)
        {
            using (VariableGetter variableGetter = new VariableGetter(col, (name) => "[ " + name + " [index] ]"))
            {
                if (variableGetter.IsError)
                    return false;

                col.MakeFunction(
                    variableGetter.MakeName,
                    HelpText,
                    CbST.CbCreateTF(col.SelectedVariableType[0]),  // 返し値の型
                    new List<ICbValue>()       // 引数
                    {
                        CbST.CbCreate<int>("index", 0),
                    },
                    new Func<List<ICbValue>, DummyArgumentsStack, ICbValue>(
                        (argument, cagt) =>
                        {
                            var ret = CbST.CbCreate(col.SelectedVariableType[0]);    // 返し値
                            try
                            {
                                int index = GetArgument<int>(argument, 0);

                                ICbValue cbVSValue = col.OwnerCommandCanvas.ScriptWorkStack.Find(variableGetter.Id);
                                var argList = cbVSValue.GetListValue.Value;

                                ret.Set(argList[index]);
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
            }
            return true;
        }
    }

    //-----------------------------------------------------------------
    class SetVariableToIndex : FuncAssetSub, IFuncCreateVariableListAssetDef
    {
        public string AssetCode => nameof(SetVariableToIndex);

        public string HelpText => Language.Instance[ApiImporter.BASE_LIB_TAG_PRE + nameof(SetVariableToIndex)];

        public string MenuTitle => "Set VariableList[index]";

        public List<TypeRequest> typeRequests => new List<TypeRequest>()
        {
            new TypeRequest(t => CbScript.AcceptAll(t))
        };

        public bool ImplAsset(MultiRootConnector col, bool isReBuildMode = false)
        {
            using (VariableGetter variableGetter = new VariableGetter(col, (name) => "[ " + name + " [index] ]"))
            {
                if (variableGetter.IsError)
                    return false;

                col.MakeFunction(
                    variableGetter.MakeName,
                    HelpText,
                    CbST.CbCreateTF(col.SelectedVariableType[0]),   // 返し値の型
                    new List<ICbValue>()       // 引数
                    {
                        CbST.CbCreate<int>("index", 0),
                        CbST.CbCreate(col.SelectedVariableType[0].GenericTypeArguments[0], "n")
                    },
                    new Func<List<ICbValue>, DummyArgumentsStack, ICbValue>(
                        (argument, cagt) =>
                        {
                            try
                            {
                                int index = GetArgument<int>(argument, 0);

                                ICbList cbVSValue = col.OwnerCommandCanvas.ScriptWorkStack.Find(variableGetter.Id).GetListValue;
                                cbVSValue[index].Set(argument[1]);

                                col.OwnerCommandCanvas.ScriptWorkStack.UpdateValueData(variableGetter.Id);
                                col.LinkConnectorControl.UpdateValueData();
                                return cbVSValue;
                            }
                            catch (Exception ex)
                            {
                                var ret = CbST.CbCreate(col.SelectedVariableType[0]);    // 返し値
                                col.ExceptionFunc(ret, ex);
                                return ret;
                            }
                        }
                        )
                    );
                return true;
            }
        }
    }

    //-----------------------------------------------------------------
    class AppendVariableList : FuncAssetSub, IFuncCreateVariableListAssetDef
    {
        public string AssetCode => nameof(AppendVariableList);

        public string HelpText => Language.Instance[ApiImporter.BASE_LIB_TAG_PRE + AssetCode];

        public string MenuTitle => "Append VariableList";

        public List<TypeRequest> typeRequests => new List<TypeRequest>()
        {
            new TypeRequest(t => CbScript.AcceptAll(t))
        };

        public bool ImplAsset(MultiRootConnector col, bool isReBuildMode = false)
        {
            using (VariableGetter variableGetter = new VariableGetter(col, (name) => "Append [ " + name + " ]"))
            {
                if (variableGetter.IsError)
                    return false;

                col.MakeFunction(
                    variableGetter.MakeName,
                    HelpText,
                    CbST.CbCreateTF(col.SelectedVariableType[0]),   // 返し値の型
                    new List<ICbValue>()   // 引数
                    {
                        CbST.CbCreate(col.SelectedVariableType[0].GenericTypeArguments[0], "n")
                    },
                    new Func<List<ICbValue>, DummyArgumentsStack, ICbValue>(
                        (argument, cagt) =>
                        {
                            try
                            {
                                ICbList cbVSValue = col.OwnerCommandCanvas.ScriptWorkStack.Find(variableGetter.Id).GetListValue;
                                cbVSValue.Append(argument[0]);

                                col.OwnerCommandCanvas.ScriptWorkStack.UpdateValueData(variableGetter.Id);
                                col.LinkConnectorControl.UpdateValueData();
                                return cbVSValue;
                            }
                            catch (Exception ex)
                            {
                                var ret = CbST.CbCreate(col.SelectedVariableType[0]);    // 返し値
                                col.ExceptionFunc(ret, ex);
                                return ret;
                            }
                        }
                        )
                    );
                return true;
            }
        }
    }

    //-----------------------------------------------------------------
    class Pow : FuncAssetSub, IFuncAssetWithArgumentDef
    {
        public string AssetCode => nameof(Pow);

        public string HelpText => Language.Instance[ApiImporter.BASE_LIB_TAG_PRE + AssetCode];

        public string MenuTitle => $"{AssetCode}({CbSTUtils.DOUBLE_STR}, {CbSTUtils.DOUBLE_STR}) : {CbSTUtils.DOUBLE_STR}";

        public List<TypeRequest> typeRequests => new List<TypeRequest>()
        {
            new TypeRequest(typeof(double))
        };

        public bool ImplAsset(MultiRootConnector col, bool isReBuildMode = false)
        {
            col.MakeFunction(
                $"{AssetCode}",
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

        public string HelpText => Language.Instance[ApiImporter.BASE_LIB_TAG_PRE + AssetCode];

        public string MenuTitle => $"{AssetCode}({CbSTUtils.INT_STR}, {CbSTUtils.INT_STR}) : {CbSTUtils.INT_STR}";

        public List<TypeRequest> typeRequests => new List<TypeRequest>()
        {
            new TypeRequest(typeof(int))
        };

        public bool ImplAsset(MultiRootConnector col, bool isReBuildMode = false)
        {
            col.MakeFunction(
                $"{AssetCode}",
                HelpText,
                CbST.CbCreateTF<int>(),   // 返し値の型
                new List<ICbValue>()  // 引数
                {
                    CbST.CbCreate<int>("min", 0),
                    CbST.CbCreate<int>("max", 1),
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
