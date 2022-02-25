using CapyCSS.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace CapyCSS.Script
{
    public class BuildScriptInfo
    {
        public enum CodeType
        {
            none,
            Sequece,
            ResultSequece,
            Variable,
            DummyArgument,
            VoidSwitch,
            Method,
            List,
            Data,
            Delegate,
            ResultDelegate,
            StackVariable,
            GetIndexer,
            SetIndexer,
            Property,
        }

        public enum AttributeType
        {
            None,
            ByRef,
            Out,
            In,
        }

        private const int TAB_SIZE = 4;
        private const string WORK_VARIABLE_NAME = "__cpb_work_";

        private string ScriptElement = null;
        public List<BuildScriptInfo> Child = null;
        private CodeType ElementType = CodeType.none;
        public string TypeName = null;
        public string Label = null;
        private AttributeType Attribute = AttributeType.None;
        public bool IsNotUseCache = false;
        private string SharedValiable = null;
        private bool IsInstanceMethod = false;

        private static int workCounter = 0;
        private static IList<BuildScriptInfo> SharedScripts = null;

        public CodeType GetElementType()
        {
            return ElementType;
        }

        /// <summary>
        /// 内容を obj にコピーします。
        /// </summary>
        /// <param name="obj">コピー先</param>
        public void CopyTo(BuildScriptInfo obj)
        {
            obj.ScriptElement = ScriptElement;
            obj.Child = Child;
            obj.ElementType = ElementType;
            obj.TypeName = TypeName;
            obj.Label = Label;
            obj.Attribute = Attribute;
            obj.IsNotUseCache = IsNotUseCache;
            obj.SharedValiable = SharedValiable;
            obj.IsInstanceMethod = IsInstanceMethod;
        }

        /// <summary>
        /// 内容を初期状態に戻します。
        /// </summary>
        public void Clear()
        {
            ScriptElement = null;
            Child = null;
            ElementType = CodeType.none;
            TypeName = null;
            Label = null;
            Attribute = AttributeType.None;
            IsNotUseCache = false;
            SharedValiable = null;
            IsInstanceMethod = false;
        }

        /// <summary>
        /// 内容が空か？
        /// </summary>
        /// <returns>true==空</returns>
        public bool IsEmpty()
        {
            return ScriptElement == null && Child == null;
        }

        /// <summary>
        /// 共有する結果を持つ変数が作られているかを調べます。
        /// </summary>
        /// <returns></returns>
        private bool isExistSharedValiable() => SharedValiable != null;

        /// <summary>
        /// 共有する結果を持つ変数に値を代入する為の情報を挿入します。
        /// </summary>
        /// <param name="src"></param>
        public static void InsertSharedScripts(BuildScriptInfo src)
        {
            if (SharedScripts != null)
            {
                if (src.Child == null || src.Child.Count == 0)
                {
                    return;
                }
                src.Child[0].Child.InsertRange(0, SharedScripts);
                SharedScripts = null;
            }
        }

        /// <summary>
        /// 結果を変数に入れる処理に分けて、変数を返します。
        /// 結果を変数に入れる処理は、BuildScriptInfo.InsertSharedScripts での出力用に残されます。
        /// </summary>
        /// <param name="src">置き換えるノード情報</param>
        /// <returns>変数</returns>
        public string MakeSharedValiable(BuildScriptInfo src)
        {
            if (isExistSharedValiable())
            {
                return SharedValiable;
            }
            BuildScriptInfo newNode = new BuildScriptInfo();
            SharedValiable = WORK_VARIABLE_NAME + workCounter++;
            newNode.Set(SharedValiable, CodeType.Variable);
            newNode.Child ??= new List<BuildScriptInfo>();

            var temp = BuildScriptInfo.CreateBuildScriptInfo(null);
            src.CopyTo(temp);

            newNode.Child.Add(temp);
            SharedScripts ??= new List<BuildScriptInfo>();
            SharedScripts.Add(newNode);
            return SharedValiable;
        }

        /// <summary>
        /// BuildScriptInfo を初期化します。
        /// </summary>
        private static void InitBuildScriptInfo()
        {
            workCounter = 0;
            if (SharedScripts != null)
            {
                SharedScripts.Clear();
            }
            buildScriptInfoList.Clear();
        }
        private static IDictionary<object, BuildScriptInfo> buildScriptInfoList = new Dictionary<object, BuildScriptInfo>();

        public static bool HaveBuildScriptInfo(object methodObject)
        {
            if (methodObject is null)
            {
                return false;
            }
            return buildScriptInfoList.ContainsKey(methodObject);
        }

        public static BuildScriptInfo CreateBuildScriptInfo(object methodObject)
        {
            if (HaveBuildScriptInfo(methodObject))
            {
                // すでに作られているので対応したものを返す

                return buildScriptInfoList[methodObject];
            }
            var result = new BuildScriptInfo();
            if (methodObject != null)
            {
                buildScriptInfoList.Add(methodObject, result);
            }
            return result;
        }

        public static BuildScriptInfo CreateBuildScriptInfo(
            object methodObject,
            string code, 
            CodeType scopeMode = CodeType.ResultSequece,
            string label = null)
        {
            if (HaveBuildScriptInfo(methodObject))
            {
                // すでに作られているので対応したものを返す

                return buildScriptInfoList[methodObject];
            }
            var result = new BuildScriptInfo(code, scopeMode, label);
            if (methodObject != null)
            {
                buildScriptInfoList.Add(methodObject, result);
            }
            return result;
        }

        private BuildScriptInfo() { }

        /// <summary>
        /// 要素を構築します。
        /// </summary>
        /// <param name="code">内容</param>
        /// <param name="scopeMode">タイプ</param>
        private BuildScriptInfo(string code, CodeType scopeMode = CodeType.none, string label = null)
        {
            Set(code, scopeMode, label);
        }

        /// <summary>
        /// 要素を設定します。
        /// </summary>
        /// <param name="code">内容</param>
        /// <param name="scopeMode">タイプ</param>
        public void Set(string code, CodeType scopeMode = CodeType.none, string label = null)
        {
            this.ScriptElement = code;
            this.ElementType = scopeMode;
            this.Label = label;
        }

        /// <summary>
        /// 型名を設定します。
        /// </summary>
        /// <param name="name"></param>
        public void SetTypeName(string name)
        {
            TypeName = name;
        }

        /// <summary>
        /// ラベルを設定します。
        /// </summary>
        /// <param name="name"></param>
        public void SetLabel(string name)
        {
            Label = name;
        }

        /// <summary>
        /// 引数属性を取り込みます。
        /// </summary>
        /// <param name="info"></param>
        public void SetArgumentAttr(ScriptImplement.ArgumentInfoNode info)
        {
            Attribute = AttributeType.None;
            Attribute = info.IsByRef ? AttributeType.ByRef : Attribute;
            Attribute = info.IsOut ? AttributeType.Out : Attribute;
            Attribute = info.IsIn ? AttributeType.In : Attribute;
        }

        /// <summary>
        /// インスタンスメソッドか否かをセットします。
        /// </summary>
        /// <param name="isInstanceMethod">true==インスタンスメソッド</param>
        public void SetInstanceMethod(bool isInstanceMethod)
        {
            IsInstanceMethod = isInstanceMethod;
        }

        /// <summary>
        /// 子を追加します。
        /// </summary>
        /// <param name="info">BuildScriptInfo</param>
        public void Add(BuildScriptInfo info)
        {
            if (info != null)
            {
                Child ??= new List<BuildScriptInfo>();
                Child.Add(info);
            }
        }

        /// <summary>
        /// 子を追加します。
        /// </summary>
        /// <param name="info">BuildScriptInfo</param>
        public void Add(IEnumerable<BuildScriptInfo> info)
        {
            if (info != null)
            {
                Child ??= new List<BuildScriptInfo>();
                Child.AddRange(info);
            }
        }

        private bool IsMethodType()
        {
            return ElementType == CodeType.Method
                || ElementType == CodeType.GetIndexer
                || ElementType == CodeType.SetIndexer;
        }

        /// <summary>
        /// 適切な括弧開きを取得します。
        /// </summary>
        /// <param name="level">段組み階層</param>
        /// <returns>括弧開き</returns>
        private string GetOpenBrakets(CodeType codeType)
        {
            switch (codeType)
            {
                case CodeType.GetIndexer:
                case CodeType.SetIndexer:
                    return "[";

                case CodeType.Method:
                    return "(";

                case CodeType.Sequece:
                case CodeType.ResultSequece:
                case CodeType.List:
                    return "{";

                default:
                    return "";
            }
        }

        /// <summary>
        /// 適切な括弧閉じを取得します。
        /// </summary>
        /// <param name="level">段組み階層</param>
        /// <returns>括弧閉じ</returns>
        private string GetCloseBrakets(CodeType codeType)
        {
            switch (codeType)
            {
                case CodeType.GetIndexer:
                case CodeType.SetIndexer:
                    return "]";

                case CodeType.Method:
                    return ")";

                case CodeType.Sequece:
                case CodeType.ResultSequece:
                case CodeType.List:
                    return "}";
                
                default:
                    return "";
            }
        }

        /// <summary>
        /// 段落を付与します。
        /// </summary>
        /// <param name="tabLevel">タブスペース</param>
        /// <param name="str">文字列</param>
        /// <param name="newLine">true==改行する</param>
        /// <returns>段落を付加した文字列</returns>
        private static string ParagraphConvert(int tabLevel, string str, bool newLine = true)
        {
            return new String(' ', tabLevel) + str + (newLine ? Environment.NewLine : "");
        }

        /// <summary>
        /// c#スクリプトを構築します。
        /// </summary>
        /// <param name="entryPointName">エントリーポイント名</param>
        /// <returns>c#スクリプト</returns>
        public string BuildScript(string entryPointName)
        {
            string result = "";
            if (entryPointName == null)
            {
                // 変数の展開

                if (Child != null)
                {
                    foreach (var node in Child)
                    {
                        result += node.ScriptElement + " " + node.Label + ";" + Environment.NewLine;
                    }
                }
                return result;
            }
            InitBuildScriptInfo();
            if (string.IsNullOrWhiteSpace(entryPointName))
            {
                entryPointName = CommandCanvasList.Instance.CurrentScriptTitle;
            }
            result = "";
            result += "var " + entryPointName + " = () =>";
            result += _BuildScript(0).Item1;
            result += ";" + Environment.NewLine;
            result += $"{typeof(CapyCSSbase.Script).FullName}.{nameof(CapyCSSbase.Script.AddEntryPoint)}(nameof({entryPointName}), {entryPointName});";
            result += Environment.NewLine + Environment.NewLine;
            return result;
        }

        /// <summary>
        /// c#スクリプトを構築します。
        /// </summary>
        /// <param name="tabLevel">段組み階層</param>
        /// <param name="codeType">ノードのタイプ</param>
        /// <returns>c#スクリプト</returns>
        private Tuple<string, IList<string>> _BuildScript(
            int tabLevel,
            CodeType codeType = CodeType.none)
        {
            string result = "";
            int nextTabLevel = tabLevel;
            if (ScriptElement != null)
            {
                if (ElementType == CodeType.VoidSwitch)
                {
                    result = BuildEnum(tabLevel, nextTabLevel);
                    return new Tuple<string, IList<string>>(result, null);
                }
                else
                {
                    result += ScriptElement;
                    switch (ElementType)
                    {
                        case CodeType.ResultSequece:
                            {
                                var child = Child[0];
                                string arg1 = child.Child[0]._BuildScript(tabLevel + TAB_SIZE, CodeType.Sequece).Item1;
                                string arg2 = child.Child[1]._BuildScript(tabLevel + TAB_SIZE, CodeType.Sequece).Item1;

                                result = ParagraphConvert(tabLevel, result.TrimStart() + " " + GetOpenBrakets(ElementType), true);
                                if (!string.IsNullOrWhiteSpace(arg1))
                                {
                                    result += arg1 + ";" + Environment.NewLine;
                                }
                                result += ParagraphConvert(tabLevel + TAB_SIZE, "return " + arg2.TrimStart() + ";", true);
                                result += ParagraphConvert(tabLevel, GetCloseBrakets(ElementType), false);

                                return new Tuple<string, IList<string>>(result, null);
                            }

                        case CodeType.Sequece:
                            {
                                var child = Child[0];
                                string arg = child.Child[0]._BuildScript(tabLevel + TAB_SIZE, CodeType.Sequece).Item1;

                                result = ParagraphConvert(tabLevel, result.TrimStart() + " " + GetOpenBrakets(ElementType), true);
                                result += arg + ";" + Environment.NewLine;
                                result += ParagraphConvert(tabLevel, GetCloseBrakets(ElementType), false);

                                return new Tuple<string, IList<string>>(result, null);
                            }

                        case CodeType.ResultDelegate:
                        case CodeType.Delegate:
                            {
                                string arg = Child[0]._BuildScript(tabLevel + TAB_SIZE, CodeType.Sequece).Item1;

                                result = ParagraphConvert(tabLevel, result.TrimStart() + " ", false);
                                result += arg.TrimStart();

                                return new Tuple<string, IList<string>>(result, null);
                            }

                        case CodeType.GetIndexer:
                        case CodeType.Method:
                        case CodeType.List:
                            codeType = ElementType;
                            break;

                        case CodeType.Property:
                            {
                                if (Child is null)
                                {
                                    return new Tuple<string, IList<string>>(result, null);
                                }

                                string arg = Child[0]._BuildScript(tabLevel + TAB_SIZE, CodeType.Variable).Item1;

                                if (!string.IsNullOrWhiteSpace(arg))
                                {
                                    if (IsInstanceMethod)
                                    {
                                        // インスタンスプロパティ（最初の引数は self）

                                        arg = arg.TrimStart();
                                        if (arg.StartsWith(CbSTUtils.NEW_STR + " "))
                                        {
                                            arg = $"( {arg} )";
                                        }
                                        result = arg + "." + result;
                                    }
                                }

                                return new Tuple<string, IList<string>>(result, null);
                            }

                        case CodeType.Variable:
                            {
                                if (Child is null)
                                {
                                    return new Tuple<string, IList<string>>(result, null);
                                }

                                string arg = Child[0]._BuildScript(tabLevel + TAB_SIZE, CodeType.Variable).Item1;

                                if (!string.IsNullOrWhiteSpace(arg))
                                {
                                    // 変数

                                    if (result.StartsWith(WORK_VARIABLE_NAME))
                                    {
                                        // キャッシュを作成

                                        result = "var " + result;
                                    }
                                    result = ParagraphConvert(tabLevel, result.TrimStart() + " = ", false);
                                    result += arg.TrimStart();
                                }

                                return new Tuple<string, IList<string>>(result, null);
                            }

                        case CodeType.DummyArgument:
                            break;
                    }
                }
                nextTabLevel += TAB_SIZE;
            }
            if (Child != null)
            {
                string args = "";
                IList<string> arguments = new List<string>();
                IList<IList<string>> arguments2 = new List<IList<string>>();
                IList<BuildScriptInfo> argList = Child;
                for (int i = 0; i < argList.Count; i++)
                {
                    BuildScriptInfo info = argList[i];
                    var ret = info._BuildScript(nextTabLevel, codeType);
                    string arg = ret.Item1;
                    arguments2.Add(ret.Item2);
                    if (codeType == CodeType.Sequece && (arg.Trim() == "" || arg.Trim() == "null"))
                    {
                        continue;
                    }
                    arg = AddAttribute(info, arg);
                    if (string.IsNullOrEmpty(args))
                    {
                        string _temp = ParagraphConvert(nextTabLevel, arg.TrimStart(), false);
                        arguments.Add(_temp);
                        args += _temp;
                    }
                    else
                    {
                        // ２つ目以降のパラメータ

                        string _temp = ParagraphConvert(nextTabLevel, arg.TrimStart(), false);
                        arguments.Add(_temp);
                        args += (codeType == CodeType.Sequece ? ";" : ",") + Environment.NewLine;
                        args += _temp;
                    }
                }
                if (ElementType == CodeType.DummyArgument)
                {
                    return new Tuple<string, IList<string>>(args.TrimStart().Replace("INDEX.", ""), arguments);
                }
                if (ElementType == CodeType.none)
                {
                    Debug.Assert(ScriptElement is null);
                    return new Tuple<string, IList<string>>(args, arguments);
                }
                string methodName = result;
                if (methodName == CbSTUtils.NULL_STR)
                {
                    return new Tuple<string, IList<string>>(methodName, null);
                }
                if (!string.IsNullOrEmpty(args))
                {
                    BuildScriptInfo info = argList[0];
                    if (IsInstanceMethod)
                    {
                        // インスタンスメソッド（最初の引数は self）

                        Debug.Assert(arguments2 != null && arguments2.Count > 0);
                        var argumentList = arguments2[0];
                        if (argumentList.Count() > 1)
                        {
                            // 引数有りのインスタンスメソッド

                            string instance = argumentList[0].Trim();
                            instance = StripInstance(instance);
                            if (ElementType == CodeType.GetIndexer)
                            {
                                // インデクサーの参照

                                string index = argumentList[1].Trim();
                                methodName = instance.TrimStart();  // インデクサーにメソッド名は不要
                                if (methodName.StartsWith(CbSTUtils.NEW_STR + " "))
                                {
                                    methodName = $"( {methodName} )";
                                }
                                result = methodName + GetOpenBrakets(ElementType) + $" {index} " + GetCloseBrakets(ElementType);
                            }
                            else if (ElementType == CodeType.SetIndexer)
                            {
                                // インデクサーへの代入

                                string index = argumentList[1].Trim();
                                string setValue = argumentList[2].Trim();
                                methodName = instance.TrimStart();  // インデクサーにメソッド名は不要
                                if (methodName.StartsWith(CbSTUtils.NEW_STR + " "))
                                {
                                    methodName = $"( {methodName} )";
                                }
                                result = methodName + GetOpenBrakets(ElementType) + $" {index} " + GetCloseBrakets(ElementType);
                                result += " = " + StripSequence(BuildMethod(0, codeType, "", setValue));
                            }
                            else
                            {
                                // インスタンスメソッドを構築

                                instance = instance.TrimStart();
                                if (instance.StartsWith(CbSTUtils.NEW_STR + " "))
                                {
                                    instance = $"( {instance} )";
                                }
                                methodName = instance + "." + methodName;
                                args = ShapeArguments(argumentList);
                                result = BuildMethod(tabLevel, codeType, methodName, args);
                            }
                        }
                        else
                        {
                            // 引数無しのインスタンスメソッド

                            args = args.TrimStart();
                            if (args.StartsWith(CbSTUtils.NEW_STR + " "))
                            {
                                args = $"( {args} )";
                            }
                            methodName = args + "." + methodName;
                            result = BuildMethod(tabLevel, codeType, methodName, "");
                        }
                    }
                    else
                    {
                        // 引数有りのstaticメソッド

                        result = BuildMethod(tabLevel, codeType, methodName, args);
                    }
                }
                else
                {
                    // 引数無しのstaticメソッド

                    result = methodName + GetOpenBrakets(codeType) + GetCloseBrakets(codeType);
                }
            }
            return new Tuple<string, IList<string>>(result, null);
        }

        private static string ShapeArguments(IList<string> argumentList)
        {
            string args = "";
            for (int i = 1; i < argumentList.Count; i++)
            {
                if (i == 1)
                {
                    args += argumentList[i];
                }
                else
                {
                    args += ',' + Environment.NewLine;
                    args += argumentList[i];
                }
            }
            return args;
        }

        private static string StripInstance(string instance)
        {
            if (instance.Length > 0 && instance[0] == '(' && instance.Last() == ')')
            {
                instance = instance.Substring(1, instance.Length - 2).Trim();
                instance = $"({instance})";
            }
            return instance;
        }

        /// <summary>
        /// 一番外側の{}を省きます。
        /// </summary>
        /// <param name="scr"></param>
        /// <returns></returns>
        private static string StripSequence(string scr)
        {
            if (scr.Length > 0 && scr[0] == '{' && scr.Last() == '}')
            {
                scr = scr.Substring(1, scr.Length - 2).Trim();
            }
            return scr;
        }

        /// <summary>
        /// 引数に[ref,out,in]修飾します。
        /// </summary>
        /// <param name="info"></param>
        /// <param name="arg"></param>
        /// <returns></returns>
        private static string AddAttribute(BuildScriptInfo info, string arg)
        {
            if (info.Attribute == AttributeType.ByRef)
                arg = "ref " + arg;
            if (info.Attribute == AttributeType.Out)
                arg = "out " + arg;
            if (info.Attribute == AttributeType.In)
                arg = "in " + arg;
            return arg;
        }

        /// <summary>
        /// メソッドを構築します。
        /// </summary>
        /// <param name="tabLevel"></param>
        /// <param name="codeType"></param>
        /// <param name="methodName"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        private string BuildMethod(
            int tabLevel,
            CodeType codeType,
            string methodName,
            string args)
        {
            string result;
            if (args.Split(Environment.NewLine).Length > 1)
            {
                // 引数が２個以上

                result = BuildOverOneArgumentsMethod(tabLevel, codeType, methodName, args);
            }
            else
            {
                // 引数が１個

                result = BuildOneArgumentMethod(tabLevel, codeType, methodName, args);
            }
            return result;
        }

        /// <summary>
        /// 引数が２つ以上のメソッドを構築します。
        /// </summary>
        /// <param name="tabLevel"></param>
        /// <param name="codeType"></param>
        /// <param name="methodName"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        private string BuildOverOneArgumentsMethod(
            int tabLevel,
            CodeType codeType,
            string methodName, 
            string args)
        {
            string result = "";
            result = ParagraphConvert(
                tabLevel,
                methodName.TrimStart() + (IsMethodType() ? "" : " ") + GetOpenBrakets(codeType),
                true);
            result += args;
            result += Environment.NewLine + ParagraphConvert(tabLevel, GetCloseBrakets(codeType), false);
            return result;
        }

        /// <summary>
        /// 引数が一つのメソッドを構築します。
        /// </summary>
        /// <param name="tabLevel"></param>
        /// <param name="codeType"></param>
        /// <param name="methodName"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        private string BuildOneArgumentMethod(
            int tabLevel, 
            CodeType codeType, 
            string methodName, 
            string args)
        {
            string result = "";
            string arg = args.Trim();
            if (arg.Length > 0)
            {
                arg = " " + arg + " ";
            }
            int maxLine = (methodName + arg).Split(Environment.NewLine).Select(n => n.Length).Max();
            if (maxLine > 80)
            {
                result = ParagraphConvert(
                    tabLevel,
                    methodName.TrimStart() + (IsMethodType() ? "" : " ") + GetOpenBrakets(codeType),
                    true);
                result += args;
                result += Environment.NewLine + ParagraphConvert(tabLevel, GetCloseBrakets(codeType), false);
            }
            else
            {
                // １行で表現する

                result = methodName + (IsMethodType() ? "" : " ")
                    + GetOpenBrakets(codeType) + arg + GetCloseBrakets(codeType);
            }
            return result;
        }

        /// <summary>
        /// 列挙型のc#スクリプトを構築します。
        /// </summary>
        /// <param name="tabLevel">段組み階層</param>
        /// <param name="nextTabLevel">次の段組み階層</param>
        /// <returns>c#スクリプト</returns>
        private string BuildEnum(int tabLevel, int nextTabLevel)
        {
            string result = "";
            Debug.Assert(Child != null && Child.Count > 0);
            var childRoot = Child[0];
            Debug.Assert(childRoot.Child != null && childRoot.Child.Count == 3);
            string target = childRoot.Child[0]._BuildScript(nextTabLevel + TAB_SIZE).Item1;
            string defaultCall = childRoot.Child[2]._BuildScript(nextTabLevel + TAB_SIZE).Item1;

            tabLevel += TAB_SIZE;
            result += "switch (" + target.TrimStart() + ") {" + Environment.NewLine;
            var caseList = childRoot.Child[1];
            foreach (var child in caseList.Child)
            {
                BuildScriptInfo label = child.Child[0];
                if (label != null && label.ScriptElement != "null")
                {
                    BuildScriptInfo caseTarget;
                    if (child.Label is null)
                    {
                        Debug.Assert(child.Child != null && child.Child.Count > 0);
                        caseTarget = child.Child[0];
                    }
                    else
                    {
                        caseTarget = child;
                    }
                    string caseName = TypeName + "." + caseTarget.Label.Replace(":", "");
                    string caseCall = label._BuildScript(tabLevel + TAB_SIZE * 2).Item1;
                    if (caseCall == "null")
                    {
                        continue;
                    }
                    result += new String(' ', tabLevel) + "case " + caseName + ": " + GetOpenBrakets(0) + Environment.NewLine;
                    result += ParagraphConvert(tabLevel + TAB_SIZE, caseCall + ";");
                    result += ParagraphConvert(tabLevel + TAB_SIZE, "break;");
                }
            }
            result += ParagraphConvert(tabLevel, "default: ");
            defaultCall = defaultCall.TrimStart();
            if (defaultCall != "null")
            {
                if (defaultCall.StartsWith("() => "))
                {
                    // "() => " を取り除く

                    defaultCall = defaultCall.Substring(6);
                }
                result += GetOpenBrakets(0);
                result += ParagraphConvert(tabLevel + TAB_SIZE, defaultCall);
            }
            result += ParagraphConvert(tabLevel + TAB_SIZE, "break;");
            tabLevel -= TAB_SIZE;
            result += ParagraphConvert(tabLevel, "}", false);

            return result;
        }
    }
}
