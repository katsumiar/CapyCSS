using CapyCSS.Controls;
using CapyCSS.Controls.BaseControls;
using CapyCSS.Script.Lib;
using CbVS.Script;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;
using System.Windows.Input;
using System.IO;
using System.Linq;

using static CapyCSS.Controls.BaseControls.CommandCanvas;
using static CapyCSS.Controls.MultiRootConnector;
using CapyCSS.Script;

namespace CapyCSS.Script
{
    /// <summary>
    /// スクリプトの機能をインポートしメニューに登録するクラスです。
    /// </summary>
    public class ApiImporter : ImplementAsset
    {
        private TreeMenuNode ProgramNode = null;
        private TreeMenuNode DotNet = null;
        private TreeMenuNode ImportGroupNode = null;
        private CommandCanvas OwnerCommandCanvas = null;
        public ObservableCollection<string> ModulueNameList = new ObservableCollection<string>();
        public List<string> DllModulePathList = new List<string>();
        public List<string> NameSpaceModuleList = new List<string>();
        public List<string> NuGetModuleList = new List<string>();

        public const string BASE_LIB_TAG_PRE = "BaseLib:";

        public const string MENU_TITLE_PROGRAM = "Program";
        public const string MENU_TITLE_IMPORT = "Import";
        public const string MENU_TITLE_DOT_NET_FUNCTION = "Function";
        public const string MENU_TITLE_DOT_NET_FUNCTION_FULL_PATH = MENU_TITLE_PROGRAM + "." + MENU_TITLE_DOT_NET_FUNCTION + ".";
        public const string MENU_TITLE_IMPORT_FUNCTION_FULL_PATH = MENU_TITLE_PROGRAM + "." + MENU_TITLE_IMPORT + ".";

        public ApiImporter(CommandCanvas ownerCommandCanvas)
        {
            OwnerCommandCanvas = ownerCommandCanvas;
            ProgramNode = CreateGroup(ownerCommandCanvas, MENU_TITLE_PROGRAM);

            CreateAssetMenu(ownerCommandCanvas, ProgramNode, new Sequence());

            {
                var literalNode = CreateGroup(ProgramNode, Script_Literal.LIB_Script_literal_NAME);
                CreateAssetMenu(ownerCommandCanvas, literalNode, new LiteralType());
            }

            {
                var variableNode = CreateGroup(ProgramNode, "Variable");
                CreateAssetMenu(ownerCommandCanvas, variableNode, new CreateVariable());
                CreateAssetMenu(ownerCommandCanvas, variableNode, new GetVariable());
                CreateAssetMenu(ownerCommandCanvas, variableNode, new SetVariable());

                {
                    var variableListNode = CreateGroup(variableNode, "Variable List");
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
        /// 基本的にインポートする情報を参照します。
        /// </summary>
        /// <returns>インポート情報</returns>
        public List<string> GetBaseImportList()
        {
            return new List<string>()
                {
                    $"{ModuleControler.HEADER_NAMESPACE}System",
                    $"{ModuleControler.HEADER_NAMESPACE}System.Collections.Generic"
                };
        }

        /// <summary>
        /// 基本的なモジュールをインポートします。
        /// </summary>
        public void ImportBaseModule()
        {
            foreach (var ns in GetBaseImportList())
            {
                if (ns.StartsWith(ModuleControler.HEADER_NAMESPACE))
                {
                    ImportNameSpace(ns.Substring(ModuleControler.HEADER_NAMESPACE.Length));
                }
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
            if (ImportGroupNode is null)
            {
                ImportGroupNode = CreateGroup(ProgramNode, MENU_TITLE_IMPORT);
            }
            string name = ScriptImplement.ImportScriptMethodsFromDllFile(OwnerCommandCanvas, ImportGroupNode, path, ignoreClassList);
            if (name != null)
            {
                ModulueNameList.Add(name);  // インポートリストに表示
                DllModulePathList.Add(path);
            }
        }

        /// <summary>
        /// スクリプトにネームスペースをインポートします。
        /// </summary>
        /// <param name="nameSpaceName">ネームスペース名</param>
        /// <returns>true==成功</returns>
        public bool ImportNameSpace(string nameSpaceName)
        {
            if (NameSpaceModuleList.Contains(nameSpaceName))
            {
                return true;
            }
            if (ImportGroupNode is null)
            {
                ImportGroupNode = CreateGroup(ProgramNode, MENU_TITLE_IMPORT);
            }
            string name = ScriptImplement.ImportScriptMethodsFromNameSpace(OwnerCommandCanvas, ImportGroupNode, nameSpaceName);
            if (name != null)
            {
                ModulueNameList.Add(name);  // インポートリストに表示
                NameSpaceModuleList.Add(nameSpaceName);
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
            CommandCanvasList.SetOwnerCursor(Cursors.Wait);
            if (ImportGroupNode is null)
            {
                ImportGroupNode = CreateGroup(ProgramNode, MENU_TITLE_IMPORT);
            }
            string name = ScriptImplement.ImportScriptMethodsFromNuGet(OwnerCommandCanvas, ImportGroupNode, packageDir, pkgName);
            if (name != null)
            {
                ModulueNameList.Add(name);  // インポートリストに表示
                NuGetModuleList.Add(pkgName);
                Console.WriteLine($"NuGet successed.");
                CommandCanvasList.SetOwnerCursor(null);
                return true;
            }
            Console.WriteLine($"faild.");
            CommandCanvasList.SetOwnerCursor(null);
            return false;
        }

        /// <summary>
        /// モジュールの登録を削除します。
        /// </summary>
        public void ClearModule(ICollection<string> importList)
        {
            // インポート情報の作成
            List<string> removeList = new List<string>();
            foreach (var nd in ImportGroupNode.Child)
            {
                removeList.Add(nd.Name);
            }

            if (removeList.Count != 0)
            {
                // 削除除外を適用
                foreach (var ns in importList)
                {
                    if (removeList.Count == 0)
                    {
                        break;
                    }
                    if (ns.StartsWith(ModuleControler.HEADER_DLL))
                    {
                        string name = ns.Substring(ModuleControler.HEADER_DLL.Length);
                        string path = importList.First(n => n.EndsWith(name));
                        removeList.Remove(ModuleControler.HEADER_DLL + Path.GetFileName(name));
                    }
                    else if (ns.StartsWith(ModuleControler.HEADER_NUGET))
                    {
                        string name = ns.Substring(ModuleControler.HEADER_NUGET.Length);
                        foreach (var path in removeList.FindAll(n => n.EndsWith(name)))
                        {
                            removeList.Remove(path);
                        }
                    }
                    else
                    {
                        removeList.Remove(ns);
                    }
                }
            }

            // インポート情報の削除
            foreach (var rd in removeList)
            {
                TreeMenuNode treeMenuNode = null;
                foreach (var childNode in ImportGroupNode.Child)
                {
                    if (childNode.Name == rd)
                    {
                        treeMenuNode = childNode;
                        break;
                    }
                }
                if (treeMenuNode != null)
                {
                    ImportGroupNode.Child.Remove(treeMenuNode);
                    ModulueNameList.Remove(rd);
                    OwnerCommandCanvas.ClearTypeImportMenu(rd);
                    if (rd.StartsWith(ModuleControler.HEADER_NAMESPACE))
                    {
                        // namespace

                        string name = rd.Substring(ModuleControler.HEADER_NAMESPACE.Length);
                        NameSpaceModuleList.Remove(name);
                    }
                    else if (rd.StartsWith(ModuleControler.HEADER_DLL) && rd.Contains(':'))
                    {
                        // NuGet

                        string name = rd.Substring(rd.IndexOf(':') + 1);
                        NuGetModuleList.Remove(name);
                        ModulueNameList.Remove(ModuleControler.HEADER_NUGET + name);
                        string temp = rd.Substring(ModuleControler.HEADER_DLL.Length);
                        NugetClient.RemoveLoadedPackage(temp.Substring(0, temp.IndexOf(':')));
                    }
                    else if (rd.StartsWith(ModuleControler.HEADER_DLL))
                    {
                        // DLL

                        string name = rd.Substring(ModuleControler.HEADER_DLL.Length);
                        var path = DllModulePathList.Find(n => n.EndsWith(name));
                        Debug.Assert(path != null);
                        DllModulePathList.Remove(path);
                    }
                }
            }

            if (ImportGroupNode != null && ImportGroupNode.Child.Count == 0)
            {
                ProgramNode.Child.Remove(ImportGroupNode);
                ImportGroupNode = null;
            }
        }
    }

    /// <summary>
    /// スクリプト構築情報
    /// </summary>
    public class BuildScriptFormat
        : IBuildScriptInfo
    {
        public string FuncCode { get; set; } = "";
        public Type ClassType { get; set; } = null;
        public BuildScriptFormat(string funcCode, Type classType = null)
        {
            FuncCode = funcCode;
            ClassType = classType;
        }
    }

    //-----------------------------------------------------------------
    class LiteralType : IFuncAssetLiteralDef
    {
        public string MenuTitle => "Literal : T";

        public string HelpText => Language.Instance[ApiImporter.BASE_LIB_TAG_PRE + nameof(LiteralType)];

        public List<TypeRequest> typeRequests => new List<TypeRequest>()
        {
            new TypeRequest(t => !t.IsAbstract || t == CbSTUtils.ARRAY_TYPE)
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
            new TypeRequest(CbSTUtils.LIST_INTERFACE_TYPE, t => CbScript.IsAggregate(t) || t == typeof(string))
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
                $"{AssetCode}<{CbSTUtils.GetTypeName(col.SelectedVariableType[0].GenericTypeArguments[0])}>",
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
            new TypeRequest(CbSTUtils.LIST_INTERFACE_TYPE, t => CbScript.IsAggregate(t))
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
            new TypeRequest(CbSTUtils.LIST_INTERFACE_TYPE, t => CbScript.IsAggregate(t))
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

        public string MenuTitle => $"{AssetCode}(T, {CbSTUtils.LIST_STR}<T>) : T" + CbSTUtils.MENU_OLD_SPECIFICATION;

        public List<TypeRequest> typeRequests => new List<TypeRequest>()
        {
            new TypeRequest(CbSTUtils.LIST_INTERFACE_TYPE, t => CbScript.IsAggregate(t))
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

            // 古い仕様であることを知らせる
            col.OldSpecification = true;

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
                            CommandCanvasList.OutPut.Flush();
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

                Func<ICbValue> resultValue;
                if (col.SelectedVariableType[0].IsArray)
                {
                    resultValue = CbST.CbCreateTF(col.SelectedVariableType[0].GetElementType());
                }
                else
                {
                    resultValue = CbST.CbCreateTF(col.SelectedVariableType[0].GenericTypeArguments[0]);
                }

                col.MakeFunction(
                    variableGetter.MakeName,
                    HelpText,
                    resultValue,  // 返し値の型
                    new List<ICbValue>()       // 引数
                    {
                        CbST.CbCreate<int>("index", 0),
                    },
                    new Func<List<ICbValue>, DummyArgumentsStack, ICbValue>(
                        (argument, cagt) =>
                        {
                            var ret = resultValue();    // 返し値
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

                ICbValue resultValue;
                if (col.SelectedVariableType[0].IsArray)
                {
                    resultValue = CbST.CbCreate(col.SelectedVariableType[0].GetElementType(), "n");
                }
                else
                {
                    resultValue = CbST.CbCreate(col.SelectedVariableType[0].GenericTypeArguments[0], "n");
                }

                col.MakeFunction(
                    variableGetter.MakeName,
                    HelpText,
                    CbST.CbCreateTF(col.SelectedVariableType[0]),   // 返し値の型
                    new List<ICbValue>()       // 引数
                    {
                        CbST.CbCreate<int>("index", 0),
                        resultValue
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

        public string MenuTitle => "Append VariableList" + CbSTUtils.MENU_OLD_SPECIFICATION;

        public string HelpText => Language.Instance[ApiImporter.BASE_LIB_TAG_PRE + AssetCode];

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

                ICbValue resultValue;
                if (col.SelectedVariableType[0].IsArray)
                {
                    resultValue = CbST.CbCreate(col.SelectedVariableType[0].GetElementType(), "n");
                }
                else
                {
                    resultValue = CbST.CbCreate(col.SelectedVariableType[0].GenericTypeArguments[0], "n");
                }

                col.MakeFunction(
                    variableGetter.MakeName,
                    HelpText,
                    CbST.CbCreateTF(col.SelectedVariableType[0]),   // 返し値の型
                    new List<ICbValue>()   // 引数
                    {
                        resultValue
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

                // 古い仕様であることを知らせる
                col.OldSpecification = true;

                return true;
            }
        }
    }

    //-----------------------------------------------------------------
    class Pow : FuncAssetSub, IFuncAssetWithArgumentDef
    {
        public string AssetCode => nameof(Pow);

        public string HelpText => Language.Instance[ApiImporter.BASE_LIB_TAG_PRE + AssetCode];

        public string MenuTitle => $"{AssetCode}({CbSTUtils.DOUBLE_STR}, {CbSTUtils.DOUBLE_STR}) : {CbSTUtils.DOUBLE_STR}" + CbSTUtils.MENU_OLD_SPECIFICATION;

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

            // 古い仕様であることを知らせる
            col.OldSpecification = true;

            return true;
        }
    }

    //-----------------------------------------------------------------
    class Rand : FuncAssetSub, IFuncAssetWithArgumentDef
    {
        public string AssetCode => nameof(Rand);

        public string HelpText => Language.Instance[ApiImporter.BASE_LIB_TAG_PRE + AssetCode];

        public string MenuTitle => $"{AssetCode}({CbSTUtils.INT_STR}, {CbSTUtils.INT_STR}) : {CbSTUtils.INT_STR}" + CbSTUtils.MENU_OLD_SPECIFICATION;

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

            // 古い仕様であることを知らせる
            col.OldSpecification = true;

            return true;
        }
    }
}
