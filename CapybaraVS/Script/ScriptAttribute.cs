//#define DEBUG_IMPORT    // インポート機能のデバッグモード

using CapyCSS.Controls.BaseControls;
using CapyCSS.Controls;
using CapyCSS.Script;
using CapyCSS.Script.Lib;
using CbVS.Script;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using static CapyCSS.Controls.BaseControls.CommandCanvas;

namespace CapyCSS.Script
{
    public interface IScriptArribute
    {
        string Path { get; }
        string MethodName { get; }
        bool OldSpecification { get; }
        bool DefaultHide { get; }
        bool IsRunable { get; }
    }

    /// <summary>
    /// リフレクションによるメソッドのスクリプトノード化用のメソッド用属性です。
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class ScriptClassAttribute : Attribute, IScriptArribute
    {
        private string path;            // メニュー用のパス
        private bool oldSpecification;  // 古い仕様のメソッド（ユーザーに廃止を促すのに使用します）
        private bool defaultHide;       // 非表示指定
        public string Path => path;
        public string MethodName => null;
        public bool OldSpecification => oldSpecification;
        public bool DefaultHide => defaultHide;
        public bool IsRunable => false;
        public ScriptClassAttribute(string path = "", bool defaultHide = true, bool oldSpecification = false)
        {
            if (path != "" && !path.EndsWith("."))
            {
                path += ".";
            }
            this.path = path;
            this.oldSpecification = oldSpecification;
            this.defaultHide = defaultHide;
        }
    }

    /// <summary>
    /// リフレクションによるメソッドのスクリプトノード化用のメソッド用属性です。
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
    public class ScriptMethodAttribute : Attribute, IScriptArribute
    {
        private string path;            // メニュー用のパス
        private string methodName;      // メソッド名
        private bool oldSpecification;  // 古い仕様のメソッド（ユーザーに廃止を促すのに使用します）
        private bool isRunable;         // 任意実行可能ノード指定
        public string Path => path;
        public string MethodName => methodName;
        public bool OldSpecification => oldSpecification;
        public bool DefaultHide => false;
        public bool IsRunable => isRunable;
        public ScriptMethodAttribute(string path = "", string methodName = null, bool oldSpecification = false, bool isRunable = false)
        {
            if (path != "" && !path.EndsWith("."))
            {
                path += ".";
            }
            this.path = path;
            this.methodName = methodName;
            this.oldSpecification = oldSpecification;
            this.isRunable = isRunable;
        }
    }

    /// <summary>
    /// リフレクションによるメソッドのスクリプトノード化用の引数用属性です。
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter)]
    public class ScriptParamAttribute : Attribute
    {
        private string name;    // 引数名
        public string ParamName => name;
        public ScriptParamAttribute(string name)
        {
            this.name = name;
        }
    }

    public class ScriptImplement
    {
        public static string ImportingName = "";

        /// <summary>
        /// 引数情報です。
        /// </summary>
        public struct ArgumentInfoNode
        {
            public Func<ICbValue> CreateArgument;
            public bool IsByRef = false;
            public bool IsOut = false;
            public bool IsIn = false;
            public bool IsSelf = false;
        }

        /// <summary>
        /// NuGetをでインストールしてメソッドを取り込み、ノード化します。
        /// </summary>
        /// <param name="OwnerCommandCanvas">オーナーキャンバス</param>
        /// <param name="node">登録先のノード</param>
        /// <param name="packageDir"></param>
        /// <param name="packageName">パッケージ名</param>
        /// <returns>インポートしたパッケージ名</returns>
        public static string ImportScriptMethodsFromNuGet(
            CommandCanvas OwnerCommandCanvas,
            TreeMenuNode node,
            string packageDir,
            string packageName)
        {
            string pkgId;
            string ver = "(" + packageName.Split("(")[1];
            Console.WriteLine($"NuGet {packageName}.");
#if !DEBUG_IMPORT
            try
#endif
            {
                List<NugetClient.PackageInfo> packageList = CapyCSS.Script.NugetClient.install(packageDir, packageName, out pkgId);
                if (packageList is null)
                {
                    return null;
                }
                foreach (var package in packageList)
                {
#if true
                    ScriptImplement.ImportScriptMethodsFromDllFile(OwnerCommandCanvas, node, package.Path, null, $"{package.Name}({package.Version})", packageName);
#else
                    if (package == packageList.Last())
                    {
                        ScriptImplement.ImportScriptMethodsFromDllFile(OwnerCommandCanvas, node, package.Path, null, $"{package.Name}({package.Version})");
                    }
                    else
                    {
                        Assembly.LoadFrom(package.Path);
                        CommandCanvasList.OutPut.OutLine(nameof(ScriptImplement), $"imported {package.Name}({package.Version}) package.");
                    }
#endif
                }
            }
#if !DEBUG_IMPORT
            catch (WebException)
            {
                Console.WriteLine($"NG.");
                return null;
            }
#endif
            return ModuleControler.HEADER_NUGET + pkgId + ver;
        }

        /// <summary>
        /// 本アプリ内のメソッドを取り込み、ノード化します。
        /// </summary>
        /// <param name="OwnerCommandCanvas">オーナーキャンバス</param>
        /// <param name="node">登録先のノード</param>
        public static void ImportScriptMethods(
            CommandCanvas OwnerCommandCanvas,
            TreeMenuNode node)
        {
            // 現在のコードを実行しているアセンブリを取得する
            Assembly asm = Assembly.GetExecutingAssembly();
            GetApiFromAssemblyForScriptMethodAttribute(OwnerCommandCanvas, node, asm);
        }

        /// <summary>
        /// ネームスペース名からメソッドをスクリプトで使えるように取り込みます。
        /// </summary>
        /// <param name="OwnerCommandCanvas">オーナーキャンバス</param>
        /// <param name="node">登録先のノード</param>
        /// <param name="name">ネームスペース名</param>
        /// <returns>インポートしたネームスペース名</returns>
        public static string ImportScriptMethodsFromNameSpace(
            CommandCanvas OwnerCommandCanvas,
            TreeMenuNode node,
            string name)
        {
            string outputName = ModuleControler.HEADER_NAMESPACE + name;
            ImportingName = outputName;
            TreeMenuNode functionNode = ImplementAsset.CreateGroup(node, outputName);
            List<Type> types = new List<Type>();
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (Type ct in assembly.GetTypes())
                {
                    if (ct.Namespace == name)
                    {
                        types.Add(ct);
                    }
                }
            }
            List<Type> addTypes = new List<Type>();
            ImportScriptMethods(
                OwnerCommandCanvas,
                functionNode,
                null,
                null,
                (t) => addTypes.Add(t),
                types.ToArray()
                );
            foreach (var addType in addTypes)
            {
                OwnerCommandCanvas.AddImportTypeMenu(addType);
            }
            Console.WriteLine($"imported namespace {name}");
            return outputName;
        }

        /// <summary>
        /// DLLファイルを読み込んでメソッドをスクリプトで使えるように取り込みます。
        /// </summary>
        /// <param name="OwnerCommandCanvas">オーナーキャンバス</param>
        /// <param name="node">登録先のノード</param>
        /// <param name="path"></param>
        /// <param name="importNameList">取り込む名前リスト</param>
        /// <param name="moduleName">モジュール名（NuGet用）</param>
        /// <param name="ownerModuleName">親モジュール名（NuGet用）</param>
        /// <returns>インポートしたモジュール名</returns>
        public static string ImportScriptMethodsFromDllFile(
            CommandCanvas OwnerCommandCanvas,
            TreeMenuNode node,
            string path,
            List<string> importNameList,
            string moduleName = null,
            string ownerModuleName = null)
        {
#if !DEBUG_IMPORT
            try
#endif
            {
                var asm = Assembly.LoadFrom(path);
                Module mod = asm.GetModule(path);
                string name = Path.GetFileName(path);

                string createGroupName;
                if (moduleName is null)
                {
                    // DLL

                    createGroupName = ModuleControler.HEADER_DLL + name;
                }
                else
                {
                    // NuGet

                    createGroupName = ModuleControler.HEADER_DLL + moduleName + $":{ownerModuleName}";
                }
                ImportingName = createGroupName;

                List<Type> addTypes = new List<Type>();
                ImportScriptMethods(
                    OwnerCommandCanvas,
                    ImplementAsset.CreateGroup(node, createGroupName),
                    asm,
                    mod,
                    importNameList,
                    (t) => addTypes.Add(t)
                    );
                foreach (var addType in addTypes)
                {
                    OwnerCommandCanvas.AddImportTypeMenu(addType);
                }
                if (moduleName is null)
                {
                    // DLL

                    Console.WriteLine($"imported {name}");
                    CommandCanvasList.OutPut.Flush();
                }
                else
                {
                    // NuGet

                    Console.WriteLine($"imported {moduleName}");
                    CommandCanvasList.OutPut.Flush();
                }
                return ModuleControler.HEADER_DLL + name;
            }
#if !DEBUG_IMPORT
            catch (Exception ex)
            {
                Console.WriteLine($"Import Dll Error: {ex.Message}.");
            }
            return null;
#endif
        }

        /// <summary>
        /// 受け入れられるメソッドか判定します。
        /// </summary>
        /// <param name="classType">クラス情報</param>
        /// <param name="methodInfo">メソッド情報</param>
        /// <returns>true==受け入れられる</returns>
        private static bool IsAcceptMethod(Type classType, MethodBase methodInfo)
        {
            if (!classType.IsInterface && methodInfo.IsAbstract)
                return false;   // 抽象メソッドは呼べない

            return true;
        }

        /// <summary>
        /// 受け入れられるクラスか判定します。
        /// </summary>
        /// <param name="classType">クラスの型</param>
        /// <returns>true==受け入れられる</returns>
        private static bool IsAcceptClass(Type classType)
        {
            if (classType.IsNestedPrivate)
                return false;   // 扱えない

            if (classType.IsNotPublic)
                return false;   // 扱えない

            if (classType.IsInterface)
                return true;    // インターフェイスは扱う

            if (classType.IsValueType)
                return true;    // 値型は扱う

            if (!classType.IsClass)
                return false;   // クラス以外は、扱わない

            return true;
        }

        /// <summary>
        /// 受け入れる型を判定します。
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private static bool IsAcceptTypeMenuType(Type type)
        {
            if (type.IsNestedPrivate)
                return false;   // 扱えない

            if (type.IsNotPublic)
                return false;   // 扱えない

            if (type.IsPointer)
                return false;   // 扱えない

            if (type.IsByRefLike)
            {
                // ref-like型構造体は、ジェネリック型引数にできない

                return false;
            }

            if (type.IsEnum)
                return true;

            if (type.IsInterface)
                return true;

            if (CbStruct.IsStruct(type))
                return true;

            if (type.IsClass)
                return true;

            if (type.IsValueType)
                return true;

            return false;
        }

        /// <summary>
        /// Assemblyからメソッドをスクリプトで使えるように取り込みます。
        /// </summary>
        /// <param name="OwnerCommandCanvas">オーナーキャンバス</param>
        /// <param name="node">登録先のノード</param>
        /// <param name="asm">対象Assembly</param>
        /// <param name="module">モジュール</param>
        /// <param name="importNameList">取り込む名前リスト</param>
        public static void ImportScriptMethods(
            CommandCanvas OwnerCommandCanvas,
            TreeMenuNode node,
            Assembly asm,
            Module module,
            List<string> importNameList,
            Action<Type> inportTypeMenu)
        {
            Type[] types = null;
            if (module is null)
            {
                types = asm.GetTypes();
            }
            else
            {
                types = module.GetTypes();
            }

            ImportScriptMethods(OwnerCommandCanvas, node, module, importNameList, inportTypeMenu, types);
        }

        private static void ImportScriptMethods(CommandCanvas OwnerCommandCanvas, TreeMenuNode node, Module module, List<string> importNameList, Action<Type> inportTypeMenu, Type[] types)
        {
            Task<List<Type>> tcTask = null;
            if (inportTypeMenu != null)
            {
                // 型情報を収集する

                tcTask = Task.Run(() =>
                {
                    List<Type> resultTypes = new List<Type>();
                    foreach (Type type in types)
                    {
                        if (importNameList != null && !importNameList.Any(n => type.FullName.StartsWith(n)))
                            continue;

                        if (!IsAcceptTypeMenuType(type))
                            continue;

                        resultTypes.Add(type);
                    }
                    return resultTypes;
                });
#if DEBUG_IMPORT
                tcTask.Wait();
#endif
            }

            List<Task<List<AutoImplementFunctionInfo>>> tasks = CreateMakeInportFunctionInfoTasks(module, importNameList, types);

            Task[] addMethodsAndTypesTasks = new Task[] {
                // メソッドをメニューに登録する
                Task.Run(() =>
                {
                    foreach (var task in tasks)
                    {
                        var classInfos = task.Result;
                        if (classInfos is null)
                            continue;

                        foreach (var info in classInfos)
                        {
                            if (info is null)
                                continue;

                            CreateMethodNode(OwnerCommandCanvas, node, info);
                        }
                    }
                }),
                // 型をメニューに登録する
                Task.Run(() =>
                {
                    if (tcTask != null)
                    {
                        var typeList = tcTask.Result;
                        foreach (var type in typeList)
                        {
                            inportTypeMenu(type);
                        }
                    }
                })
            };

            Task.WaitAll(addMethodsAndTypesTasks);
        }

        /// <summary>
        /// メソッド取り込み用情報収集タスクリストを作成します。
        /// </summary>
        /// <param name="module">モジュール</param>
        /// <param name="importNameList">取り込む名前リスト</param>
        /// <param name="types">モジュールの情報</param>
        /// <returns>タスクリスト</returns>
        private static List<Task<List<AutoImplementFunctionInfo>>> CreateMakeInportFunctionInfoTasks(Module module, List<string> importNameList, Type[] types)
        {
            var tasks = new List<Task<List<AutoImplementFunctionInfo>>>();
            Type actionType = typeof(Action);
            foreach (Type classType in types)
            {
                if (importNameList != null && importNameList.Count > 0 && !importNameList.Any(n => classType.FullName.StartsWith(n)))
                    continue;

                if (!IsAcceptClass(classType))
                    continue;   // 扱えない

                if (classType == typeof(Action))
                    continue;   // 本システムで特殊な扱いをしているので扱わない
                if (classType.IsGenericType)
                {
                    if (classType.GetGenericTypeDefinition() == typeof(Action<>))
                        continue;   // 本システムで特殊な扱いをしているので扱わない
                    if (classType.GetGenericTypeDefinition() == typeof(Func<>))
                        continue;   // 本システムで特殊な扱いをしているので扱わない
                }

                string className = classType.Name;
                if (className.EndsWith("Exception"))
                    continue;   // 例外は扱わない
                if (className.StartsWith("Async"))
                    continue;   // 本システムで特殊な扱いをしているので扱わない

                if (!classType.IsAbstract)
                {
                    // コンストラクタをインポートする

                    Task<List<AutoImplementFunctionInfo>> importConstructorTask = Task.Run(() =>
                    {
                        return CreateMakeInportConstructorInfoTasks(module, classType);
                    });
                    tasks.Add(importConstructorTask);
#if DEBUG_IMPORT
                    importConstructorTask.Wait();
#endif
                }

                // メソッドをインポートする
                Task<List<AutoImplementFunctionInfo>> importMethodTask = Task.Run(() =>
                {
                    return CreateMakeInportMethodInfoTasks(module, classType);
                });
                tasks.Add(importMethodTask);
#if DEBUG_IMPORT
                importMethodTask.Wait();
#endif
            }

            return tasks;
        }

        private static List<AutoImplementFunctionInfo> CreateMakeInportConstructorInfoTasks(Module module, Type classType)
        {
            List<AutoImplementFunctionInfo> importFuncInfoList = new List<AutoImplementFunctionInfo>();
            foreach (ConstructorInfo constructorInfo in classType.GetConstructors())
            {
                if (!IsAcceptMethod(classType, constructorInfo))
                    continue;   // 未対応

#if !DEBUG_IMPORT
                try
#endif
                {
                    var functionInfo = MakeInportFunctionInfo(classType, constructorInfo, null, null, module);
                    if (functionInfo != null)
                    {
                        importFuncInfoList.Add(functionInfo);
                    }
                }
#if !DEBUG_IMPORT
                catch (Exception ex)
                {
                    CommandCanvasList.ErrorLog += nameof(ScriptImplement) + "." + nameof(ImportScriptMethods) + ": " + ex.Message + Environment.NewLine + Environment.NewLine;
                }
#endif
            }
            return importFuncInfoList;
        }

        private static List<AutoImplementFunctionInfo> CreateMakeInportMethodInfoTasks(Module module, Type classType)
        {
            List<AutoImplementFunctionInfo> importFuncInfoList = new List<AutoImplementFunctionInfo>();
            foreach (MethodInfo methodInfo in classType.GetMethods())
            {
                if (!IsAcceptMethod(classType, methodInfo))
                    continue;   // 未対応

#if !DEBUG_IMPORT
                try
#endif
                {
                    var functionInfo = MakeInportFunctionInfo(classType, methodInfo, methodInfo.ReturnType, null, module);
                    if (functionInfo != null)
                    {
                        importFuncInfoList.Add(functionInfo);
                    }
                }
#if !DEBUG_IMPORT
                catch (Exception ex)
                {
                    CommandCanvasList.ErrorLog += nameof(ScriptImplement) + "." + nameof(ImportScriptMethods) + ": " + ex.Message + Environment.NewLine + Environment.NewLine;
                }
#endif
            }
            return importFuncInfoList;
        }

        /// <summary>
        /// リフレクションでAssemblyからScriptMethodAttribute属性を持つメソッドをスクリプトで使えるように取り込みます。
        /// </summary>
        /// <param name="OwnerCommandCanvas">オーナーキャンバス</param>
        /// <param name="node">登録先のノード</param>
        /// <param name="asm">対象Assembly</param>
        public static void GetApiFromAssemblyForScriptMethodAttribute(
            CommandCanvas OwnerCommandCanvas,
            TreeMenuNode node,
            Assembly asm)
        {
            Type[] types = asm.GetTypes();

            // 型情報を収集する
            var task = Task.Run(()=>
            {
                foreach (Type type in types)
                {
                    if (!IsAcceptTypeMenuType(type))
                        return;   // 扱えない

                    Debug.Assert(false);
                    // AddImportTypeMenu の間違い？
                    OwnerCommandCanvas.AddTypeMenu(type);
                }
            });

            foreach (Type classType in types)
            {
                var classAttr = classType.GetCustomAttribute(typeof(ScriptClassAttribute)) as ScriptClassAttribute;
                if (classAttr is null)
                {
                    continue;
                }

                if (!IsAcceptClass(classType))
                    continue;   // 扱えない

                if (!classType.IsAbstract)
                {
                    // コンストラクタをインポートする

                    foreach (ConstructorInfo constructorInfo in classType.GetConstructors())
                    {
#if !DEBUG_IMPORT
                        try
#endif
                        {
                            if (!IsAcceptMethod(classType, constructorInfo))
                                continue;   // 未対応

                            importScriptMethodAttributeMethods(OwnerCommandCanvas, node, classType, constructorInfo, classAttr, null);
                        }
#if !DEBUG_IMPORT
                        catch (Exception ex)
                        {
                            CommandCanvasList.ErrorLog += nameof(ScriptImplement) + "." + nameof(ImportScriptMethods) + ": " + ex.Message + Environment.NewLine + Environment.NewLine;
                        }
#endif
                    }
                }

                // メソッドをインポートする
                foreach (MethodInfo methodInfo in classType.GetMethods())
                {
#if !DEBUG_IMPORT
                    try
#endif
                    {
                        if (!IsAcceptMethod(classType, methodInfo))
                            continue;   // 未対応

                        importScriptMethodAttributeMethods(OwnerCommandCanvas, node, classType, methodInfo, classAttr, methodInfo.ReturnType);
                    }
#if !DEBUG_IMPORT
                    catch (Exception ex)
                    {
                        CommandCanvasList.ErrorLog += nameof(ScriptImplement) + "." + nameof(ImportScriptMethods) + ": " + ex.Message + Environment.NewLine + Environment.NewLine;
                    }
#endif
                }

                task.Wait();
            }
        }

        /// <summary>
        /// クラスとメソッド情報からScriptMethodAttribute属性を持つメソッドをスクリプトで使えるように取り込みます。
        /// </summary>
        /// <param name="OwnerCommandCanvas">オーナーキャンバス</param>
        /// <param name="node">登録先のノード</param>
        /// <param name="classType">クラスの型情報</param>
        /// <param name="methodInfo">メソッド情報</param>
        /// <param name="returnType">メソッドの返り値の型情報</param>
        private static void importScriptMethodAttributeMethods(
            CommandCanvas OwnerCommandCanvas, 
            TreeMenuNode node, 
            Type classType,
            MethodBase methodInfo,
            IScriptArribute methodAttr,
            Type returnType)
        {
            List<Task<AutoImplementFunctionInfo>> tasks = CreateMakeInportFunctionInfoTasks(classType, methodInfo, methodAttr, returnType);

            if (tasks is null)
            {
                return;
            }

            // ノード化
            foreach (var info in tasks)
            {
                if (info is null || info.Result is null)
                    continue;

                CreateMethodNode(OwnerCommandCanvas, node, info.Result);
            }
        }

        public class ScriptAttributeInfo : IScriptArribute
        {
            public string Path { get; set; } = null;
            public string MethodName { get; set; } = null;
            public bool OldSpecification { get; set; } = false;
            public bool DefaultHide { get; set; } = false;
            public bool IsRunable { get; set; } = false;
        }

        /// <summary>
        /// メソッド取り込み用情報収集タスクリストを作成します。
        /// </summary>
        /// <param name="classType">クラスの型情報</param>
        /// <param name="methodInfo">メソッド情報</param>
        /// <param name="returnType">メソッドの返り値の型情報</param>
        /// <returns>タスクリスト</returns>
        private static List<Task<AutoImplementFunctionInfo>> CreateMakeInportFunctionInfoTasks(
            Type classType, 
            MethodBase methodInfo,
            IScriptArribute methodAttr,
            Type returnType)
        {
            var tasks = new List<Task<AutoImplementFunctionInfo>>();

            var scriptAttributeInfo = new ScriptAttributeInfo()
            {
                Path = methodAttr.Path,
                MethodName = methodAttr.MethodName,
                OldSpecification = methodAttr.OldSpecification,
                DefaultHide = methodAttr.DefaultHide,
                IsRunable = methodAttr.IsRunable,
            };

            var ovrMethodAttr = methodInfo.GetCustomAttribute(typeof(ScriptMethodAttribute)) as ScriptMethodAttribute;
            if (ovrMethodAttr != null)
            {
                if (!String.IsNullOrEmpty(ovrMethodAttr.Path))
                {
                    scriptAttributeInfo.Path = ovrMethodAttr.Path;
                }
                if (!String.IsNullOrEmpty(ovrMethodAttr.MethodName))
                {
                    scriptAttributeInfo.MethodName = ovrMethodAttr.MethodName;
                }
                if (ovrMethodAttr.OldSpecification)
                {
                    scriptAttributeInfo.OldSpecification = ovrMethodAttr.OldSpecification;
                }
                if (ovrMethodAttr.IsRunable)
                {
                    scriptAttributeInfo.IsRunable = ovrMethodAttr.IsRunable;
                }
            }
            else if (scriptAttributeInfo.DefaultHide)
            {
                return null;
            }

            if (methodAttr != null)
            {
                Task<AutoImplementFunctionInfo> task = Task.Run(() =>
                {
                    return MakeInportFunctionInfo(classType, methodInfo, returnType, scriptAttributeInfo);
                });
                tasks.Add(task);
#if DEBUG_IMPORT
                task.Wait();
#endif
            }
            return tasks;
        }

        /// <summary>
        /// クラスとメソッド情報からスクリプトで使えるように取り込みます。
        /// </summary>
        /// <param name="OwnerCommandCanvas">オーナーキャンバス</param>
        /// <param name="node">登録先のノード</param>
        /// <param name="classType">クラスの型情報</param>
        /// <param name="methodInfo">メソッド情報</param>
        /// <param name="returnType">メソッドの返り値の型情報</param>
        /// <param name="methodAttr">ScriptMethodAttribute情報(or null)</param>
        /// <param name="module">DLLモジュール(or null)</param>
        /// <returns>スクリプト用メソッド情報</returns>
        private static AutoImplementFunctionInfo MakeInportFunctionInfo(
            Type classType,
            MethodBase methodInfo, 
            Type returnType,
            IScriptArribute methodAttr = null,
            Module module = null)
        {
            if (returnType != null && returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>))
                return null;
            if (returnType == typeof(Task))
                return null;

            List<ArgumentInfoNode> argumentList = null;

            string genericArgs = "";
            List<TypeRequest> genericTypeRequests = null;
            if (classType.IsGenericType)
            {
                // ジェネリッククラスのメニュー名に追加するジェネリック情報を作成
                // 型確定時に選択を要求する型リクエスト情報リストを作成

                genericTypeRequests ??= new List<TypeRequest>();
                MakeGenericParamaterTypeRequestList(classType, genericTypeRequests);
            }
            if (methodInfo.IsGenericMethod)
            {
                // ジェネリックメソッドのメニュー名に追加するジェネリック情報を作成
                // 型確定時に選択を要求する型リクエスト情報リストを作成

                genericTypeRequests ??= new List<TypeRequest>();
                MakeGenericParamaterTypeRequestList(methodInfo, genericTypeRequests);
                genericArgs = CbSTUtils.GetGenericParamatersString(methodInfo);
            }

            if (!methodInfo.IsStatic && !methodInfo.IsConstructor)
            {
                // 静的メソッドでは無いので所属するクラスの情報を取得

                var refType = methodInfo.ReflectedType;
                if (refType.IsValueType   // 絞り込み
                        && refType.Name == "Void"   // 絞り込み
                        && refType.FullName == "System.Void")
                {
                    // System.Void に所属するものには対応しない
                    // System.Void は、ジェネリック引数に使えないので CbStruct で管理できない…

                    return null;
                }

                var selfType = TryGetCbType(methodInfo.ReflectedType);
                if (selfType is null)
                    return null;
                argumentList = MakeSelfTypeForClassMethod(methodInfo, argumentList, selfType);
            }

            if (GetArgumentList(methodInfo, ref argumentList))
            {
                Func<string, ICbValue> retType;

                if (methodInfo.IsConstructor)
                {
                    // コンストラクタ

                    retType = TryGetCbType(classType);
                }
                else
                {
                    // 返り値の型を準備
                    if (returnType.IsValueType   // 絞り込み
                        && returnType.Name == "Void"   // 絞り込み
                        && returnType.FullName == "System.Void")
                    {
                        // void 型は専用のクラスを利用する

                        retType = TryGetCbType(CbVoid.T);
                    }
                    else
                    {
                        retType = TryGetCbType(returnType);
                    }
                }

                if (retType is null)
                    return null; // 返り値の型が対象外

                if (retType("") is null)
                    return null; // 返り値の型が対象外

                // オーバーロード用の名前保管情報を作成（同名にならないようにする）
                string addArg = "#";
                ParameterInfo[] paramsinfo = methodInfo.GetParameters();
                foreach (ParameterInfo para in paramsinfo)
                {
                    addArg += "_" + CbSTUtils._GetTypeName(para.ParameterType);
                }

                string nodeCode = methodInfo.ReflectedType.Namespace + "." + methodInfo.ReflectedType.Name + "." + methodInfo.Name + addArg;

                string addState = "";

                //----------------------------------
                // クラス名を作成
                string className;
                if (classType.IsGenericType)
                {
                    className = CbSTUtils.GetGenericTypeName(classType);
                }
                else
                {
                    className = classType.Name;
                }

                //----------------------------------
                // メソッド名を作成
                string nodeName;
                string nodeTitle;
                if (methodAttr != null)
                {
                    // メソッド属性情報を持っている

                    if (methodAttr.MethodName != null)
                    {
                        // 指定の名前を採用する

                        nodeTitle = nodeName = methodAttr.MethodName;

                    }
                    else
                    {
                        nodeTitle = nodeName = methodInfo.Name;
                    }
                }
                else
                {
                    if (methodInfo.IsConstructor)
                    {
                        if (methodInfo.IsAbstract)
                        {
                            // 抽象クラスは new できない

                            return null;
                        }
                        nodeName = "new " + className;
                        nodeTitle = className;
                    }
                    else
                    {
                        nodeTitle = nodeName = methodInfo.Name;
                    }
                }

                //----------------------------------
                // メニュー項目名を作成
                string menuName = methodInfo.Name;
                if (methodAttr != null)
                {
                    // 指定の名前を採用する

                    menuName = methodAttr.Path + nodeName;
                }
                else if (methodAttr is null)
                {
                    // 名前を作成する

                    if (methodInfo.IsConstructor)
                    {
                        menuName = className + CbSTUtils.MENU_CONSTRUCTOR + className;
                    }
                    else
                    {
                        string group = "";
                        if (methodInfo.Name.Contains("_") && methodInfo.IsSpecialName)
                        {
                            // 最初に見つかった _ までの文字列でグループを分ける

                            group = methodInfo.Name.Split("_")[0] + ".";
                        }
                        menuName += genericArgs;
                        if (methodInfo.IsStatic)
                        {
                            addState = CbSTUtils.MENU_STATIC;
                        }
                        if (methodInfo.IsVirtual)
                        {
                            addState = CbSTUtils.MENU_VIRTUAL;
                        }
                        switch (group)
                        {
                            case "get.":
                                menuName = menuName.Replace("get_","");
                                nodeTitle = nodeTitle.Replace("get_","");
                                menuName = className + CbSTUtils.MENU_GETTER + menuName;
                                break;
                            case "set.":
                                menuName = menuName.Replace("set_", "");
                                nodeTitle = nodeTitle.Replace("set_", "");
                                menuName = className + CbSTUtils.MENU_SETTER + menuName;
                                break;
                            default:
                                {
                                    List<string> baseGroup = new List<string>()
                                    {
                                        "ToString",
                                        "GetType",
                                        "Equals",
                                        "GetHashCode"
                                    };
                                    if (baseGroup.Contains(methodInfo.Name))
                                    {
                                        menuName = className + ".(base)." + menuName;
                                    }
                                    else
                                    {
                                        menuName = className + "." + group + methodInfo.Name + "." + menuName;
                                    }
                                }
                                break;
                        }
                    }
                    if (classType.Namespace != null)
                    {
                        // ネームスペースを付ける

                        menuName = classType.Namespace + "." + menuName;
                    }
                }

                // 引数情報を追加する
                string paramString = MakeParamsString(methodInfo);
                menuName += paramString;
                string nodeHintTitle = $"{classType.Namespace}.{className}.{nodeTitle}{paramString}";

                // 返り値の型名を追加する
                if (methodInfo.IsConstructor && classType.IsGenericType)
                {
                    menuName += " : " + className;
                }
                else
                {
                    var type = retType("");
                    string returnTypeString;
                    if (type.MyType == typeof(CbClass<CbGeneMethArg>))
                        returnTypeString = CbSTUtils.GetGenericTypeName((type.Data as CbGeneMethArg).ArgumentType);
                    else
                        returnTypeString = type.TypeName;
                    menuName += " : " + returnTypeString;
                    nodeHintTitle += " : " + returnTypeString;
                }

                string helpCode = $"{classType.Namespace}:{className}." + nodeName.Replace(" ", "_");
                menuName = menuName + " " + addState;
                helpCode = nodeCode + " " + addState;

                // スクリプトノード用のヒント
                string nodeHint = null;
                // ノード用ヒントを取得
                nodeHint = Language.Instance[$"Assembly.{helpCode}/node"];
                nodeHint = $"【{nodeHintTitle}】" + (nodeHint is null ? "" : Environment.NewLine + nodeHint);

                string hint = null;
                bool _oldSpecification = false;
                string addTitleMessage = "";
                bool isRunable = false;
                if (methodAttr != null)
                {
                    // メニュー用ヒントをリソースから取得
                    hint = Language.Instance[$"Assembly.{helpCode}/menu"];

                    // 古い仕様指定されているか？
                    _oldSpecification = methodAttr.OldSpecification;
                    if (_oldSpecification)
                    {
                        addTitleMessage += CbSTUtils.MENU_OLD_SPECIFICATION;
                    }

                    isRunable = methodAttr.IsRunable;
                    if (isRunable)
                    {
                        addTitleMessage += CbSTUtils.MENU_RUNABLE;
                    }
                }

                if (hint is null)
                    hint = nodeHint;
                if (hint is null)
                    hint = "";

                // ノード化依頼用の情報をセット
                AutoImplementFunctionInfo autoImplementFunctionInfo = new AutoImplementFunctionInfo()
                {
                    assetCode = nodeCode,
                    menuTitle = menuName + addTitleMessage,
                    funcTitle = nodeTitle,
                    hint = hint,
                    nodeHint = nodeHint,
                    classType = classType,
                    returnType = () => retType(""),
                    argumentTypeList = argumentList,
                    dllModule = module,
                    isConstructor = methodInfo.IsConstructor,
                    typeRequests = genericTypeRequests,
                    genericMethodParameters = (methodInfo.IsGenericMethod) ? methodInfo.GetGenericArguments() : null,
                    oldSpecification = _oldSpecification,
                    isRunable = isRunable,
                };

                return autoImplementFunctionInfo;
            }
            return null;
        }

        private static void MakeGenericParamaterTypeRequestList(Type classType, List<TypeRequest> genericTypeRequests)
        {
            foreach (var geneArg in classType.GetGenericArguments())
            {
                SetParamaterTypeFilter(genericTypeRequests, geneArg);
            }
        }

        private static void MakeGenericParamaterTypeRequestList(MethodBase method, List<TypeRequest> genericTypeRequests)
        {
            foreach (var geneArg in method.GetGenericArguments())
            {
                SetParamaterTypeFilter(genericTypeRequests, geneArg);
            }
        }

        /// <summary>
        /// パラメータ型の型制約フィルターを登録します。
        /// </summary>
        /// <param name="genericTypeRequests">型リクエストリスト</param>
        /// <param name="geneArg">パラメータ型</param>
        private static void SetParamaterTypeFilter(List<TypeRequest> genericTypeRequests, Type geneArg)
        {
            if (!geneArg.IsGenericParameter)
                return;

            if (genericTypeRequests.Any(n => n.Name == geneArg.Name))
                return; // 二重登録拒否

            Func<Type, bool> isAccept = MakeParameterConstraintAccepter(geneArg);
            genericTypeRequests.Add(new TypeRequest(isAccept, geneArg.Name));
        }

        /// <summary>
        /// パラメータ型の型制約フィルターを作成します。
        /// </summary>
        /// <param name="geneArg"></param>
        /// <returns></returns>
        public static Func<Type, bool> MakeParameterConstraintAccepter(Type geneArg)
        {
            Func<Type, bool> isAccept = (t) =>
            {
                // t には、判定対象の型が入ります。

                GenericParameterAttributes sConstraints =
                            geneArg.GenericParameterAttributes &
                            GenericParameterAttributes.SpecialConstraintMask;

                if (sConstraints != GenericParameterAttributes.None)
                {
                    if (t.IsClass &&    // 構造体は含めない（デフォルトの挙動が有る）
                        GenericParameterAttributes.None != (sConstraints &
                        GenericParameterAttributes.DefaultConstructorConstraint))
                    {
                        var query = t.GetMethods(BindingFlags.Public).Where(n => n.IsConstructor);      // 公開コンストラクタを探す
                        bool haveDefaultConstructer = query.Any(n => n.GetParameters().Length == 0);    // 引数無しを探す
                        if (!haveDefaultConstructer)
                            return false;    // 型がパラメーターなしのコンストラクターを持たなければ拒否
                    }
                    if (GenericParameterAttributes.None != (sConstraints &
                        GenericParameterAttributes.ReferenceTypeConstraint))
                    {
                        if (t.IsValueType || t.IsPointer)
                            return false;   // 型が参照型でなければ拒否（値型で無くポインタ型でも無ければ参照型）
                    }
                    if (GenericParameterAttributes.None != (sConstraints &
                        GenericParameterAttributes.NotNullableValueTypeConstraint))
                    {
                        if (!t.IsValueType)
                            return false;   // 型が値型の場合は拒否
                        if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>))
                            return false;   // Null許容型の場合は拒否
                    }
                }

                return true;
            };
            return isAccept;
        }

        /// <summary>
        /// スクリプト用メソッド情報を基にメソッドをノード化してメニューに紐付けます。
        /// </summary>
        /// <param name="OwnerCommandCanvas">オーナーキャンバス</param>
        /// <param name="node">登録先のノード</param>
        /// <param name="autoImplementFunctionInfo">スクリプト用メソッド情報</param>
        private static void CreateMethodNode(CommandCanvas OwnerCommandCanvas, TreeMenuNode node, AutoImplementFunctionInfo autoImplementFunctionInfo)
        {
            //if (CbFunc.ContainsEvent(autoImplementFunctionInfo.argumentTypeList))
            {
                // Func<> 引数を持つノードを作成

                ImplementAsset.CreateAssetMenu(
                    OwnerCommandCanvas,
                    node,
                    AutoImplementEventFunction.Create(autoImplementFunctionInfo)
                );
            }
            //else
            //{
            //    // 通常のノードを作成

            //    ImplementAsset.CreateAssetMenu(
            //        OwnerCommandCanvas,
            //        node,
            //        AutoImplementFunction.Create(autoImplementFunctionInfo)
            //    );
            //}
        }

        /// <summary>
        /// 引数情報を文字列化します。
        /// </summary>
        /// <param name="methodInfo">メソッド情報</param>
        /// <returns>文字列化した引数情報</returns>
        private static string MakeParamsString(MethodBase methodInfo)
        {
            string paramsStr = "()";
            ParameterInfo[] paramss = methodInfo.GetParameters();
            if (paramss.Length != 0)
            {
                paramsStr = "(";
                bool isFirst = true;
                foreach (ParameterInfo para in paramss)
                {
                    if (!isFirst)
                    {
                        paramsStr += ",";
                    }

                    string argStr = CbSTUtils._GetTypeName(para.ParameterType);
                    argStr = ReplaceModifier(para, argStr);
                    paramsStr += argStr;

                    isFirst = false;
                }
                paramsStr += ")";
            }
            return paramsStr;
        }

        /// <summary>
        /// 修飾子を置き換えます。
        /// </summary>
        /// <param name="para">パラメータ情報</param>
        /// <param name="argStr">置き換える前の名前</param>
        /// <returns>置き換えた文字列</returns>
        private static string ReplaceModifier(ParameterInfo para, string argStr)
        {
            argStr = argStr.Replace("&", "");
            if (para.IsIn)
            {
                argStr = $"{CbSTUtils.MENU_IN_STR} " + argStr;
            }
            else if (para.IsOut)
            {
                argStr = $"{CbSTUtils.MENU_OUT_STR} " + argStr;
            }
            else if (para.ParameterType.IsByRef)
            {
                argStr = $"{CbSTUtils.MENU_REF_STR} " + argStr;
            }
            return argStr;
        }

        /// <summary>
        /// スクリプトノード名を作成します。
        /// </summary>
        /// <param name="name">ネームスペース付きの名前</param>
        /// <returns>スクリプトノード名</returns>
        private static string MakeScriptNodeName(string name)
        {
            if (name.Contains("."))
            {
                string[] tokens = name.Split(".");
                if (tokens.Length > 1)
                    name = tokens[tokens.Length - 1];
            }
            return name;
        }

        /// <summary>
        /// クラスメソッド用に第一引数用のクラス型の self 引数を作成します。
        /// </summary>
        /// <param name="methodInfo">メソッド情報</param>
        /// <param name="argumentList">引数リスト</param>
        /// <param name="selfType"></param>
        /// <returns></returns>
        private static List<ArgumentInfoNode> MakeSelfTypeForClassMethod(
            MethodBase methodInfo,
            List<ArgumentInfoNode> argumentList,
            Func<string, ICbValue> selfType)
        {
            ArgumentInfoNode argNode = new ArgumentInfoNode();
            string name = "self";// info.ReflectedType.Name;
            foreach (var attrNode in methodInfo.ReflectedType.GetCustomAttributes())
            {
                if (attrNode is ScriptParamAttribute scriptParam)
                    name = scriptParam.ParamName;
            }
            if (selfType(name) != null)
            {
                argumentList ??= new List<ArgumentInfoNode>();
                argNode.CreateArgument = () =>
                {
                    ICbValue addParam = selfType(name);
                    return addParam;
                };
                argNode.IsSelf = true;
                argumentList.Add(argNode);
            }
            return argumentList;
        }

        /// <summary>
        /// 引数情報リストを作成します。
        /// </summary>
        /// <param name="info"></param>
        /// <param name="argumentList"></param>
        /// <returns></returns>
        private static bool GetArgumentList(
            MethodBase info,
            ref List<ArgumentInfoNode> argumentList)
        {
            if (info is null)
                return false;

            ParameterInfo[] paramsinfo = info.GetParameters();

            foreach (ParameterInfo para in paramsinfo)
            {
                var typeInfo = TryGetCbType(para.ParameterType);
                if (typeInfo is null)
                    return false;

                ArgumentInfoNode argNode = new ArgumentInfoNode();

                // 引数の修飾をチェック
                if (para.IsIn)
                {
                    argNode.IsIn = true;
                }
                else if (para.IsOut)
                {
                    argNode.IsOut = true;
                }
                else if (para.ParameterType.IsByRef)
                {
                    // IsIn と IsOut が false

                    argNode.IsByRef = true;
                }

                // 引数名を取得
                string name = para.Name;
                foreach (var attrNode in para.GetCustomAttributes())
                {
                    if (attrNode is ScriptParamAttribute scriptParam)
                        name = scriptParam.ParamName;
                }

                if (typeInfo(name) is null)
                    return false;   // 型情報の作成に失敗

                // 引数作成用の処理を作成
                argumentList ??= new List<ArgumentInfoNode>();
                argNode.CreateArgument = () =>
                {
                    ICbValue addParam = typeInfo(name);
                    addParam.IsByRef = argNode.IsByRef;
                    addParam.IsOut = argNode.IsOut;
                    addParam.IsIn = argNode.IsIn;
                    if (para.HasDefaultValue)
                    {
                        if (para.DefaultValue != null)
                        {
                            // デフォルト引数を設定

                            addParam.Data = para.DefaultValue;
                        }
                    }
                    return addParam;
                };

                argumentList.Add(argNode);
            }
            return true;
        }

        /// <summary>
        /// 型をチェックし対応する型であれば対応する CbXXX クラスの型情報を取得します。
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private static Func<string, ICbValue> TryGetCbType(Type type)
        {
            if (type is null)
                return null;

            return CbST.CbCreateNTF(type);
        }
    }
}
