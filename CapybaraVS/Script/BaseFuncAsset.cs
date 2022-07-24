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
using static CapyCSS.Script.ScriptImplement;
using CapyCSSbase;

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

        public const string LIB_Script_literal_NAME = "Literal/Local";

        public ApiImporter(CommandCanvas ownerCommandCanvas)
        {
            OwnerCommandCanvas = ownerCommandCanvas;
            ProgramNode = CreateGroup(ownerCommandCanvas, MENU_TITLE_PROGRAM);

            CreateAssetMenu(ownerCommandCanvas, ProgramNode, new VoidSequence());
            CreateAssetMenu(ownerCommandCanvas, ProgramNode, new ResultSequence());

            {
                var literalNode = CreateGroup(ProgramNode, LIB_Script_literal_NAME);
                CreateAssetMenu(ownerCommandCanvas, literalNode, new LiteralType());
            }

            {
                var variableNode = CreateGroup(ProgramNode, "Variable");
                CreateAssetMenu(ownerCommandCanvas, variableNode, new CreateVariable());
                CreateAssetMenu(ownerCommandCanvas, variableNode, new GetVariable());
                CreateAssetMenu(ownerCommandCanvas, variableNode, new SetVariable());
            }

            {
                DotNet = CreateGroup(ProgramNode, MENU_TITLE_DOT_NET_FUNCTION);

                {
                    var flowOperation = CreateGroup(DotNet, CapyCSSbase.FlowLib.LIB_FLOW_NAME);
                    CreateAssetMenu(ownerCommandCanvas, flowOperation, new SwitchEnum());
                }

                {
                    var funcNode = CreateGroup(DotNet, CapyCSSbase.FlowLib.LIB_Fx_NAME);
                    CreateAssetMenu(ownerCommandCanvas, funcNode, new DummyArguments());
                }

                {
                    var math = CreateGroup(DotNet, CapyCSSbase.MathLib.LIB_MATH_NAME);
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
                    var script = CreateGroup(DotNet, CapyCSSbase.Script.LIB_Script_NAME);
                    CreateAssetMenu(ownerCommandCanvas, script, new ScriptDispose());
                }

                {
                    var io = CreateGroup(DotNet, CapyCSSbase.EnvironmentLib.LIB_IO_NAME);
                    var conOut = CreateGroup(io, "OutConsole");
                    CreateAssetMenu(ownerCommandCanvas, conOut, new OutConsole());
                }

                {
                    var tools = CreateGroup(DotNet, "Exec");
                    CreateAssetMenu(ownerCommandCanvas, tools, new CallFile());
                }

                ScriptImplement.ImportScriptMethods(ownerCommandCanvas, DotNet);
            }

            {
                if (ImportGroupNode is null)
                {
                    ImportGroupNode = CreateGroup(ProgramNode, MENU_TITLE_IMPORT);
                }
                ScriptImplement.ImportAutoDll(ownerCommandCanvas, ImportGroupNode);
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
            try
            {
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
                    return true;
                }
                Console.WriteLine($"faild.");
            }
            finally
            {
                CommandCanvasList.ResetOwnerCursor(Cursors.Wait);
            }
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

                        if (!CbSTUtils.AutoImportDllList.Contains(name))
                        {
                            var path = DllModulePathList.Find(n => n.EndsWith(name));
                            Debug.Assert(path != null);
                            DllModulePathList.Remove(path);
                        }
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
        public bool IsConstructor { get; set; } = false;
        public bool IsProperty => false;
        public IList<ArgumentInfoNode> ArgumentTypeList { get; set; } = null;
        public bool IsClassInstanceMethod => false;

        public Type[] GenericMethodParameters { get; set; } = null;

        public IList<TypeRequest> typeRequests { get; set; } = null;

        public BuildScriptFormat(string funcCode, Type classType = null)
        {
            FuncCode = funcCode;
            ClassType = classType;
        }
    }

    //-----------------------------------------------------------------
    // TODO 今は役割が変わっているので名前を変える
    class LiteralType : IFuncAssetLiteralDef
    {
        public Func<string> MenuTitle => () => "Literal/Local : T";

        public Func<string> HelpText => () => Language.Instance[ApiImporter.BASE_LIB_TAG_PRE + nameof(LiteralType)];

        public IList<TypeRequest> typeRequests => new List<TypeRequest>()
        {
            new TypeRequest(t => !t.IsAbstract || t == CbSTUtils.ARRAY_TYPE)
        };
    }

    //-----------------------------------------------------------------
    class Sum : FuncAssetSub, IFuncAssetWithArgumentDef
    {
        public string AssetCode => nameof(Sum);

        public Func<string> HelpText => () => Language.Instance[ApiImporter.BASE_LIB_TAG_PRE + AssetCode];

        public Func<string> MenuTitle => () => $"{AssetCode}({CbSTUtils.LIST_STR}<T>) : T";

        public IList<TypeRequest> typeRequests => new List<TypeRequest>()
        {
            new TypeRequest(CbSTUtils.LIST_INTERFACE_TYPE, t => CbScript.IsAggregate(t) || t == typeof(string))
        };

        public bool ImplAsset(MultiRootConnector col, bool isReBuildMode = false)
        {
            col.MakeFunction(
                $"{AssetCode}<{col.SelectedVariableTypeName[0]}>",
                HelpText(),
                CbST.CbCreateTF(col.SelectedVariableType[0].GenericTypeArguments[0]),  // 返し値の型
                new List<ICbValue>()  // 引数
                {
                    CbST.CbCreate(col.SelectedVariableType[0], "samples"),
                },
                new Func<IList<ICbValue>, DummyArgumentsMemento, ICbValue>(
                    (argument, dummyArguments) =>
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

            // エイリアス
            col.FunctionInfo = new BuildScriptFormat(nameof(CapyCSSbase.Script.Sum), typeof(CapyCSSbase.Script));

            return true;
        }
    }

    //-----------------------------------------------------------------
    class VoidSequence : FuncAssetSub, IFuncAssetWithArgumentDef
    {
        public string AssetCode => nameof(VoidSequence);

        public Func<string> HelpText => () => Language.Instance[ApiImporter.BASE_LIB_TAG_PRE + AssetCode];

        public Func<string> MenuTitle => () => $"{AssetCode}";

        private volatile IEnumerable<ICbValue> argList;

        public IList<TypeRequest> typeRequests => new List<TypeRequest>()
        {
            new TypeRequest(typeof(object))
        };

        public bool ImplAsset(MultiRootConnector col, bool isReBuildMode = false)
        {
            col.MakeFunction(
                $"{AssetCode}",
                HelpText(),
                CbVoid.TF,  // 返し値の型
                new List<ICbValue>()  // 引数
                {
                    CbST.CbCreate<IEnumerable<CbVoid>>("flow"),
                },
                new Func<IList<ICbValue>, DummyArgumentsMemento, ICbValue>(
                    (argument, dummyArguments) =>
                    {
                        try
                        {
                            argList = GetArgumentList(argument, 0);
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

            // エイリアス
            col.FunctionInfo = new BuildScriptFormat(nameof(VoidSequence), typeof(VoidSequence));

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
    class ResultSequence : FuncAssetSub, IFuncAssetWithArgumentDef
    {
        public string AssetCode => nameof(ResultSequence);

        public Func<string> HelpText => () => Language.Instance[ApiImporter.BASE_LIB_TAG_PRE + AssetCode];

        public Func<string> MenuTitle => () => $"{AssetCode}(T) : T";

        private volatile IEnumerable<ICbValue> argList;

        public IList<TypeRequest> typeRequests => new List<TypeRequest>()
        {
            new TypeRequest(t => CbScript.AcceptAll(t))
        };

        public bool ImplAsset(MultiRootConnector col, bool isReBuildMode = false)
        {
            col.MakeFunction(
                $"{AssetCode}",
                HelpText(),
                CbST.CbCreateTF(col.SelectedVariableType[0]),  // 返し値の型
                new List<ICbValue>()  // 引数
                {
                    CbST.CbCreate<IEnumerable<CbVoid>>("flow"),
                    CbST.CbCreate(col.SelectedVariableType[0], "result"),
                },
                new Func<IList<ICbValue>, DummyArgumentsMemento, ICbValue>(
                    (argument, dummyArguments) =>
                    {
                        var ret = CbST.CbCreate(col.SelectedVariableType[0]);    // 返し値
                        try
                        {
                            argList = GetArgumentList(argument, 0);
                            ret.Set(argument[1]);
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

            // エイリアス
            col.FunctionInfo = new BuildScriptFormat(nameof(ResultSequence), typeof(ResultSequence));

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

        public Func<string> HelpText => () => Language.Instance[ApiImporter.BASE_LIB_TAG_PRE + AssetCode];

        public Func<string> MenuTitle => () => $"{AssetCode}(T) : T";

        public IList<TypeRequest> typeRequests => new List<TypeRequest>()
        {
            new TypeRequest(t => CbScript.IsCalcable(t))
        };

        public bool ImplAsset(MultiRootConnector col, bool isReBuildMode = false)
        {
            col.MakeFunction(
                $"{AssetCode}<{col.SelectedVariableTypeName[0]}>",
                HelpText(),
                CbST.CbCreateTF(col.SelectedVariableType[0]),  // 返し値の型
                new List<ICbValue>()  // 引数
                {
                    CbST.CbCreate(col.SelectedVariableType[0], "n"),
                },
                new Func<IList<ICbValue>, DummyArgumentsMemento, ICbValue>(
                    (argument, dummyArguments) =>
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

            // エイリアス
            col.FunctionInfo = new BuildScriptFormat(nameof(CapyCSSbase.Script.Inc), typeof(CapyCSSbase.Script));

            return true;
        }
    }

    //-----------------------------------------------------------------
    class Dec : FuncAssetSub, IFuncAssetWithArgumentDef
    {
        public string AssetCode => nameof(Dec);

        public Func<string> HelpText => () => Language.Instance[ApiImporter.BASE_LIB_TAG_PRE + AssetCode];

        public Func<string> MenuTitle => () => $"{AssetCode}(T) : T";

        public IList<TypeRequest> typeRequests => new List<TypeRequest>()
        {
            new TypeRequest(t => CbScript.IsCalcable(t))
        };

        public bool ImplAsset(MultiRootConnector col, bool isReBuildMode = false)
        {
            col.MakeFunction(
                $"{AssetCode}<{col.SelectedVariableTypeName[0]}>",
                HelpText(),
                CbST.CbCreateTF(col.SelectedVariableType[0]),  // 返し値の型
                new List<ICbValue>()  // 引数
                {
                    CbST.CbCreate(col.SelectedVariableType[0], "n"),
                },
                new Func<IList<ICbValue>, DummyArgumentsMemento, ICbValue>(
                    (argument, dummyArguments) =>
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

            // エイリアス
            col.FunctionInfo = new BuildScriptFormat(nameof(CapyCSSbase.Script.Dec), typeof(CapyCSSbase.Script));

            return true;
        }
    }

    //-----------------------------------------------------------------
    class Modulo : FuncAssetSub, IFuncAssetWithArgumentDef
    {
        public string AssetCode => nameof(Modulo);

        public Func<string> HelpText => () => Language.Instance[ApiImporter.BASE_LIB_TAG_PRE + AssetCode];

        public Func<string> MenuTitle => () => $"{AssetCode}(T, T) : T";

        public IList<TypeRequest> typeRequests => new List<TypeRequest>()
        {
            new TypeRequest(t => CbScript.IsCalcable(t))
        };

        public bool ImplAsset(MultiRootConnector col, bool isReBuildMode = false)
        {
            col.MakeFunction(
                $"{AssetCode}<{col.SelectedVariableTypeName[0]}, {col.SelectedVariableTypeName[0]}>",
                HelpText(),
                CbST.CbCreateTF(col.SelectedVariableType[0]),  // 返し値の型
                new List<ICbValue>()  // 引数
                {
                    CbST.CbCreate(col.SelectedVariableType[0], "n1"),
                    CbST.CbCreate(col.SelectedVariableType[0], "n2"),
                },
                new Func<IList<ICbValue>, DummyArgumentsMemento, ICbValue>(
                    (argument, dummyArguments) =>
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

            // エイリアス
            col.FunctionInfo = new BuildScriptFormat(nameof(CapyCSSbase.Script.Modulo), typeof(CapyCSSbase.Script));

            return true;
        }
    }

    //-----------------------------------------------------------------
    class Multiply : FuncAssetSub, IFuncAssetWithArgumentDef
    {
        public string AssetCode => nameof(Multiply);

        public Func<string> HelpText => () => Language.Instance[ApiImporter.BASE_LIB_TAG_PRE + AssetCode];

        public Func<string> MenuTitle => () => $"{AssetCode}(T, {CbSTUtils.LIST_STR}<T>) : T";

        public IList<TypeRequest> typeRequests => new List<TypeRequest>()
        {
            new TypeRequest(CbSTUtils.LIST_INTERFACE_TYPE, t => CbScript.IsAggregate(t))
        };

        public bool ImplAsset(MultiRootConnector col, bool isReBuildMode = false)
        {
            col.MakeFunction(
                $"{AssetCode}<{CbSTUtils.GetTypeName(col.SelectedVariableType[0].GenericTypeArguments[0])}, {col.SelectedVariableTypeName[0]}>",
                HelpText(),
                CbST.CbCreateTF(col.SelectedVariableType[0].GenericTypeArguments[0]),  // 返し値の型
                new List<ICbValue>()  // 引数
                {
                    CbST.CbCreate(col.SelectedVariableType[0].GenericTypeArguments[0], "base"),
                    CbST.CbCreate(col.SelectedVariableType[0], "samples"),
                },
                new Func<IList<ICbValue>, DummyArgumentsMemento, ICbValue>(
                    (argument, dummyArguments) =>
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

            // エイリアス
            col.FunctionInfo = new BuildScriptFormat(nameof(CapyCSSbase.Script.Multiply), typeof(CapyCSSbase.Script));

            return true;
        }
    }

    //-----------------------------------------------------------------
    class Divide : FuncAssetSub, IFuncAssetWithArgumentDef
    {
        public string AssetCode => nameof(Divide);

        public Func<string> HelpText => () => Language.Instance[ApiImporter.BASE_LIB_TAG_PRE + AssetCode];

        public Func<string> MenuTitle => () => $"{AssetCode}(T, {CbSTUtils.LIST_STR}<T>) : T";

        public IList<TypeRequest> typeRequests => new List<TypeRequest>()
        {
            new TypeRequest(CbSTUtils.LIST_INTERFACE_TYPE, t => CbScript.IsAggregate(t))
        };

        public bool ImplAsset(MultiRootConnector col, bool isReBuildMode = false)
        {
            col.MakeFunction(
                $"{AssetCode}<{CbSTUtils.GetTypeName(col.SelectedVariableType[0].GenericTypeArguments[0])}, {col.SelectedVariableTypeName[0]}>",
                HelpText(),
                CbST.CbCreateTF(col.SelectedVariableType[0].GenericTypeArguments[0]),  // 返し値の型
                new List<ICbValue>()  // 引数
                {
                    CbST.CbCreate(col.SelectedVariableType[0].GenericTypeArguments[0], "base"),
                    CbST.CbCreate(col.SelectedVariableType[0], "samples"),
                },
                new Func<IList<ICbValue>, DummyArgumentsMemento, ICbValue>(
                    (argument, dummyArguments) =>
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

            // エイリアス
            col.FunctionInfo = new BuildScriptFormat(nameof(CapyCSSbase.Script.Divide), typeof(CapyCSSbase.Script));

            return true;
        }
    }

    //-----------------------------------------------------------------
    class Subtract : FuncAssetSub, IFuncAssetWithArgumentDef
    {
        public string AssetCode => nameof(Subtract);

        public Func<string> HelpText => () => Language.Instance[ApiImporter.BASE_LIB_TAG_PRE + AssetCode];

        public Func<string> MenuTitle => () => $"{AssetCode}(T, {CbSTUtils.LIST_STR}<T>) : T" + CbSTUtils.MENU_OLD_SPECIFICATION;

        public IList<TypeRequest> typeRequests => new List<TypeRequest>()
        {
            new TypeRequest(CbSTUtils.LIST_INTERFACE_TYPE, t => CbScript.IsAggregate(t))
        };

        public bool ImplAsset(MultiRootConnector col, bool isReBuildMode = false)
        {
            col.MakeFunction(
                $"{AssetCode}<{CbSTUtils.GetTypeName(col.SelectedVariableType[0].GenericTypeArguments[0])}, {col.SelectedVariableTypeName[0]}>",
                HelpText(),
                CbST.CbCreateTF(col.SelectedVariableType[0].GenericTypeArguments[0]),  // 返し値の型
                new List<ICbValue>()  // 引数
                {
                    CbST.CbCreate(col.SelectedVariableType[0].GenericTypeArguments[0], "base"),
                    CbST.CbCreate(col.SelectedVariableType[0], "samples"),
                },
                new Func<IList<ICbValue>, DummyArgumentsMemento, ICbValue>(
                    (argument, dummyArguments) =>
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

        public Func<string> HelpText => () => Language.Instance[ApiImporter.BASE_LIB_TAG_PRE + AssetCode];

        public Func<string> MenuTitle => () => $"{AssetCode}({CbSTUtils.STRING_STR}, {CbSTUtils.BOOL_STR}, {CbSTUtils.LIST_STR}<{CbSTUtils.STRING_STR}>) : {CbSTUtils.FUNC_STR}<{CbSTUtils.INT_STR}>";

        public IList<TypeRequest> typeRequests => new List<TypeRequest>()
        {
            new TypeRequest(typeof(string))
        };

        public bool ImplAsset(MultiRootConnector col, bool isReBuildMode = false)
        {
            col.MakeFunction(
                $"{AssetCode}",
                HelpText(),
                CbFunc<Func<int>, CbInt>.TF,   // 返し値の型
                new List<ICbValue>()      // 引数
                {
                    CbST.CbCreate<string>("path"),
                    CbST.CbCreate<bool>("redirect"),
                    CbST.CbCreate<List<string>>("arguments"),
                },
                new Func<IList<ICbValue>, DummyArgumentsMemento, ICbValue>(
                    (argument, dummyArguments) =>
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

                            ret.Callback = (dummyArguments2) =>
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

        public Func<string> HelpText => () => Language.Instance[ApiImporter.BASE_LIB_TAG_PRE + nameof(_GetVariable)];

        public bool ImplAsset(MultiRootConnector col, bool isReBuildMode = false)
        {
            using (VariableGetter variableGetter = new VariableGetter(col))
            {
                if (variableGetter.IsError)
                    return false;

                col.MakeFunction(
                    variableGetter.MakeName,
                    HelpText(),
                    CbST.CbCreateTF(col.SelectedVariableType[0]),  // 返し値の型
                    null,   // 引数はなし
                    new Func<IList<ICbValue>, DummyArgumentsMemento, ICbValue>(
                        (argument, dummyArguments) =>
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

                col.FunctionInfo = new BuildScriptFormat($"[{nameof(_GetVariable)}]", col.SelectedVariableType[0]);

                return true;
            }
        }
    }

    //-----------------------------------------------------------------
    class CreateVariable : _GetVariable, IFuncCreateVariableAssetDef
    {
        public Func<string> MenuTitle => () => "Create Variable : T";

        public new Func<string> HelpText => () => Language.Instance[ApiImporter.BASE_LIB_TAG_PRE + nameof(CreateVariable)];

        public List<TypeRequest> typeRequests => new List<TypeRequest>()
        {
            new TypeRequest(t => true)    // 新規作成を意味する
        };
    }

    //-----------------------------------------------------------------
    class GetVariable : _GetVariable, IFuncCreateVariableAssetDef
    {
        public Func<string> MenuTitle => () => "Get Variable";

        public new Func<string> HelpText => () => Language.Instance[ApiImporter.BASE_LIB_TAG_PRE + nameof(GetVariable)];

        public List<TypeRequest> typeRequests => null;    // 選択を意味する
    }

    //-----------------------------------------------------------------
    class SetVariable : FuncAssetSub, IFuncCreateVariableAssetDef
    {
        public string AssetCode => nameof(SetVariable);

        public Func<string> HelpText => () => Language.Instance[ApiImporter.BASE_LIB_TAG_PRE + nameof(SetVariable)];

        public Func<string> MenuTitle => () => "Set Variable";

        public List<TypeRequest> typeRequests => null;    // 選択を意味する

        public bool ImplAsset(MultiRootConnector col, bool isReBuildMode = false)
        {
            using (VariableGetter variableGetter = new VariableGetter(col))
            {
                if (variableGetter.IsError)
                    return false;

                col.MakeFunction(
                    variableGetter.MakeName,
                    HelpText(),
                    CbVoid.TF,  // 返し値の型
                    new List<ICbValue>()  // 引数
                    {
                        CbST.CbCreate(col.SelectedVariableType[0], "n"),
                    },
                    new Func<IList<ICbValue>, DummyArgumentsMemento, ICbValue>(
                        (argument, dummyArguments) =>
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

                col.FunctionInfo = new BuildScriptFormat($"[{nameof(SetVariable)}]", col.SelectedVariableType[0]);

                return true;
            }
        }
    }

    //-----------------------------------------------------------------
    class DummyArguments : FuncAssetSub, IFuncAssetWithArgumentDef
    {
        public string AssetCode => nameof(DummyArguments);

        public Func<string> HelpText => () => Language.Instance[ApiImporter.BASE_LIB_TAG_PRE + nameof(DummyArguments)];

        public Func<string> MenuTitle => () => "DummyArguments<T> : T";

        public IList<TypeRequest> typeRequests => new List<TypeRequest>()
        {
            new TypeRequest(t => CbScript.AcceptAll(t))
        };

        public bool ImplAsset(MultiRootConnector col, bool isReBuildMode = false)
        {
            col.MakeFunction(
                "DummyArguments<" + col.SelectedVariableTypeName[0] + ">",
                HelpText(),
                CbST.CbCreateTF(col.SelectedVariableType[0]),  // 返し値の型
                new List<ICbValue>()  // 引数
                {
                    CbST.CbCreate<CbFuncArguments.INDEX>("select"),
                },
                new Func<IList<ICbValue>, DummyArgumentsMemento, ICbValue>(
                    (argument, dummyArguments) =>
                    {
                        var ret = CbST.CbCreate(col.SelectedVariableType[0]);    // 返し値

                        var select = GetArgument<CbFuncArguments.INDEX>(argument, 0);

                        try
                        {
                            // 呼び元引数をセット

                            if (dummyArguments != null && dummyArguments.CanValue(select))
                                ret.Set(dummyArguments.GetValue(select));
                            else
                                throw new Exception($"Dummy Arguments not found.");
                        }
                        catch (Exception ex)
                        {
                            col.ExceptionFunc(ret, ex);
                        }
                        return ret;
                    }
                    )
                );

            col.FunctionInfo = new BuildScriptFormat(nameof(DummyArguments), typeof(DummyArguments));

            return true;
        }
    }

    //-----------------------------------------------------------------
    class SwitchEnum : FuncAssetSub, IFuncAssetWithArgumentDef
    {
        public string AssetCode => nameof(SwitchEnum);

        public Func<string> HelpText => () => Language.Instance[ApiImporter.BASE_LIB_TAG_PRE + nameof(SwitchEnum)];

        public Func<string> MenuTitle => () => $"Switch Case<TEnum> : {CbSTUtils.VOID_STR}";

        public IList<TypeRequest> typeRequests => new List<TypeRequest>()
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
                HelpText(),
                CbVoid.TF,  // 返し値の型
                args,  // 引数
                new Func<IList<ICbValue>, DummyArgumentsMemento, ICbValue>(
                    (argument, dummyArguments) =>
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
                                    TryCallCallBack(dummyArguments, caseNode);
                                    return null;
                                }
                            }

                            // どれにも一致しなかった場合のコールバックを呼ぶ
                            TryCallCallBack(dummyArguments, argument[2]);
                        }
                        catch (Exception ex)
                        {
                            col.ExceptionFunc(null, ex);
                        }

                        return null;
                    }
                    )
                );

            col.FunctionInfo = new BuildScriptFormat("__" + nameof(SwitchEnum), col.SelectedVariableType[0]);

            return true;
        }
    }

    //-----------------------------------------------------------------
    class OutConsole : FuncAssetSub, IFuncAssetWithArgumentDef
    {
        public string AssetCode => nameof(OutConsole);

        public Func<string> HelpText => () => Language.Instance[ApiImporter.BASE_LIB_TAG_PRE + nameof(OutConsole)];

        public Func<string> MenuTitle => () => $"{AssetCode}(T) : T";

        public IList<TypeRequest> typeRequests => new List<TypeRequest>()
        {
            new TypeRequest(t => CbScript.IsNotObject(t))
        };

        public bool ImplAsset(MultiRootConnector col, bool isReBuildMode = false)
        {
            col.MakeFunction(
                $"{AssetCode}<{col.SelectedVariableTypeName[0]}>",
                HelpText(),
                CbST.CbCreateTF(col.SelectedVariableType[0]),  // 返し値の型
                new List<ICbValue>()  // 引数
                {
                    CbST.CbCreate(col.SelectedVariableType[0], "n"),
                },
                new Func<IList<ICbValue>, DummyArgumentsMemento, ICbValue>(
                    (argument, dummyArguments) =>
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

            // エイリアス
            col.FunctionInfo = new BuildScriptFormat(nameof(CapyCSSbase.Script.OutConsole), typeof(CapyCSSbase.Script));

            return true;
        }
    }
    
    //-----------------------------------------------------------------
    class Abs : FuncAssetSub, IFuncAssetWithArgumentDef
    {
        public string AssetCode => nameof(Abs);

        public Func<string> HelpText => () => Language.Instance[ApiImporter.BASE_LIB_TAG_PRE + nameof(Abs)];

        public Func<string> MenuTitle => () => $"{AssetCode}(T) : T";

        public IList<TypeRequest> typeRequests => new List<TypeRequest>()
        {
            new TypeRequest(t => CbScript.IsSigned(t))
        };

        public bool ImplAsset(MultiRootConnector col, bool isReBuildMode = false)
        {
            col.MakeFunction(
                $"{AssetCode}<{col.SelectedVariableTypeName[0]}>",
                HelpText(),
                CbST.CbCreateTF(col.SelectedVariableType[0]),  // 返し値の型
                new List<ICbValue>()  // 引数
                {
                    CbST.CbCreate(col.SelectedVariableType[0], "n"),
                },
                new Func<IList<ICbValue>, DummyArgumentsMemento, ICbValue>(
                    (argument, dummyArguments) =>
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

            // エイリアス
            col.FunctionInfo = new BuildScriptFormat("Abs", typeof(Math));

            return true;
        }
    }

    //-----------------------------------------------------------------
    class ScriptDispose : FuncAssetSub, IFuncAssetWithArgumentDef
    {
        public string AssetCode => "Dispose";

        public Func<string> HelpText => () => Language.Instance[ApiImporter.BASE_LIB_TAG_PRE + AssetCode];

        public Func<string> MenuTitle => () => $"{AssetCode}(IDisposable) : void";

        public IList<TypeRequest> typeRequests => new List<TypeRequest>()
        {
            new TypeRequest(t => CbScript.IsDisposable(t))
        };

        public bool ImplAsset(MultiRootConnector col, bool isReBuildMode = false)
        {
            col.MakeFunction(
                $"{AssetCode}",
                HelpText(),
                CbVoid.TF,  // 返し値の型
                new List<ICbValue>()  // 引数
                {
                    CbClass.ClassValue(typeof(IDisposable), "disposable"),
                },
                new Func<IList<ICbValue>, DummyArgumentsMemento, ICbValue>(
                    (argument, dummyArguments) =>
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

            // エイリアス
            col.FunctionInfo = new BuildScriptFormat(nameof(CapyCSSbase.Script.Dispose), typeof(CapyCSSbase.Script));

            return true;
        }
    }

    //-----------------------------------------------------------------
    class Pow : FuncAssetSub, IFuncAssetWithArgumentDef
    {
        public string AssetCode => nameof(Pow);

        public Func<string> HelpText => () => Language.Instance[ApiImporter.BASE_LIB_TAG_PRE + AssetCode];

        public Func<string> MenuTitle => () => $"{AssetCode}({CbSTUtils.DOUBLE_STR}, {CbSTUtils.DOUBLE_STR}) : {CbSTUtils.DOUBLE_STR}" + CbSTUtils.MENU_OLD_SPECIFICATION;

        public IList<TypeRequest> typeRequests => new List<TypeRequest>()
        {
            new TypeRequest(typeof(double))
        };

        public bool ImplAsset(MultiRootConnector col, bool isReBuildMode = false)
        {
            col.MakeFunction(
                $"{AssetCode}",
                HelpText(),
                CbST.CbCreateTF<double>(),      // 返し値の型
                new List<ICbValue>()  // 引数
                {
                    CbST.CbCreate<double>("n"),
                    CbST.CbCreate<double>("p"),
                },
                new Func<IList<ICbValue>, DummyArgumentsMemento, ICbValue>(
                    (argument, dummyArguments) =>
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

        public Func<string> HelpText => () => Language.Instance[ApiImporter.BASE_LIB_TAG_PRE + AssetCode];

        public Func<string> MenuTitle => () => $"{AssetCode}({CbSTUtils.INT_STR}, {CbSTUtils.INT_STR}) : {CbSTUtils.INT_STR}" + CbSTUtils.MENU_OLD_SPECIFICATION;

        public IList<TypeRequest> typeRequests => new List<TypeRequest>()
        {
            new TypeRequest(typeof(int))
        };

        public bool ImplAsset(MultiRootConnector col, bool isReBuildMode = false)
        {
            col.MakeFunction(
                $"{AssetCode}",
                HelpText(),
                CbST.CbCreateTF<int>(),   // 返し値の型
                new List<ICbValue>()  // 引数
                {
                    CbST.CbCreate<int>("min", 0),
                    CbST.CbCreate<int>("max", 1),
                },
                new Func<IList<ICbValue>, DummyArgumentsMemento, ICbValue>(
                    (argument, dummyArguments) =>
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
