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
        }

        public enum AttributeType
        {
            None,
            ByRef,
            Out,
            In,
            Self,
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

        private static int workCounter = 0;
        private static List<BuildScriptInfo> SharedScripts = null;

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
        }

        /// <summary>
        /// 内容が空か？
        /// </summary>
        /// <returns>true==空</returns>
        public bool IsEmpty()
        {
            return ScriptElement == null && Child == null;
        }

        private bool isExistSharedValiable() => SharedValiable != null;

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
        private static Dictionary<object, BuildScriptInfo> buildScriptInfoList = new Dictionary<object, BuildScriptInfo>();

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

        public static BuildScriptInfo CreateBuildScriptInfo(object methodObject, string code, CodeType scopeMode = CodeType.ResultSequece, string label = null)
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
            Attribute = info.IsSelf ? AttributeType.Self : Attribute;
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

        /// <summary>
        /// 適切な括弧開きを取得します。
        /// </summary>
        /// <param name="level">段組み階層</param>
        /// <returns>括弧開き</returns>
        private string GetOpenBrakets(CodeType codeType)
        {
            switch (codeType)
            {
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
            result += _BuildScript(entryPointName, 0);
            result += ";" + Environment.NewLine;
            result += $"{typeof(Lib.Script).FullName}.{nameof(Lib.Script.AddEntryPoint)}(nameof({entryPointName}), {entryPointName});";
            result += Environment.NewLine + Environment.NewLine;
            return result;
        }

        /// <summary>
        /// c#スクリプトを構築します。
        /// </summary>
        /// <param name="entryPointName">エントリーポイント名</param>
        /// <param name="tabLevel">段組み階層</param>
        /// <param name="isNotUseCache">キャッシュを使わない</param>
        /// <returns>c#スクリプト</returns>
        private string _BuildScript(string entryPointName, int tabLevel, CodeType codeType = CodeType.none)
        {
            string result = "";
            int nextTabLevel = tabLevel;
            if (ScriptElement != null)
            {
                if (ElementType == CodeType.VoidSwitch)
                {
                    result = BuildEnum(tabLevel, nextTabLevel);
                    return result;
                }
                else
                {
                    result += ScriptElement;
                    switch (ElementType)
                    {
                        case CodeType.ResultSequece:
                            {
                                var child = Child[0];
                                string arg1 = child.Child[0]._BuildScript(null, tabLevel + TAB_SIZE, CodeType.Sequece);
                                string arg2 = child.Child[1]._BuildScript(null, tabLevel + TAB_SIZE, CodeType.Sequece);

                                result = ParagraphConvert(tabLevel, result.TrimStart() + " " + GetOpenBrakets(ElementType), true);
                                if (!string.IsNullOrWhiteSpace(arg1))
                                {
                                    result += arg1 + ";" + Environment.NewLine;
                                }
                                result += ParagraphConvert(tabLevel + TAB_SIZE, "return " + arg2.TrimStart() + ";", true);
                                result += ParagraphConvert(tabLevel, GetCloseBrakets(ElementType), false);

                                return result;
                            }

                        case CodeType.Sequece:
                            {
                                var child = Child[0];
                                string arg = child.Child[0]._BuildScript(null, tabLevel + TAB_SIZE, CodeType.Sequece);

                                result = ParagraphConvert(tabLevel, result.TrimStart() + " " + GetOpenBrakets(ElementType), true);
                                result += arg + ";" + Environment.NewLine;
                                result += ParagraphConvert(tabLevel, GetCloseBrakets(ElementType), false);

                                return result;
                            }

                        case CodeType.ResultDelegate:
                        case CodeType.Delegate:
                            {
                                string arg = Child[0]._BuildScript(null, tabLevel + TAB_SIZE, CodeType.Sequece);

                                result = ParagraphConvert(tabLevel, result.TrimStart() + " ", false);
                                result += arg.TrimStart();

                                return result;
                            }

                        case CodeType.Method:
                        case CodeType.List:
                            codeType = ElementType;
                            break;

                        case CodeType.Variable:
                            {
                                if (Child is null)
                                {
                                    return result;
                                }

                                string arg = Child[0]._BuildScript(null, tabLevel + TAB_SIZE, CodeType.Variable);

                                if (!string.IsNullOrWhiteSpace(arg))
                                {
                                    if (Child[0].Child != null && Child[0].Child.Count > 0 && Child[0].Child[0].Attribute == AttributeType.Self)
                                    {
                                        // インスタンスプロパティ（最初の引数は self）

                                        result = arg.TrimStart() + result.Substring(result.LastIndexOf("."));
                                    }
                                    else
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
                                }
                                else
                                {
                                    result += arg.TrimStart();
                                }

                                return result;
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
                IList<BuildScriptInfo> argList = Child;
                for (int i = 0; i < argList.Count; i++)
                {
                    BuildScriptInfo info = argList[i];
                    string arg = info._BuildScript(null, nextTabLevel, codeType);
                    if (codeType == CodeType.Sequece && (arg.Trim() == "" || arg.Trim() == "null"))
                    {
                        continue;
                    }
                    if (info.Attribute == AttributeType.ByRef)
                        arg = "ref " + arg;
                    if (info.Attribute == AttributeType.Out)
                        arg = "out " + arg;
                    if (info.Attribute == AttributeType.In)
                        arg = "in " + arg;
                    if (string.IsNullOrEmpty(args))
                    {
                        args += ParagraphConvert(nextTabLevel, arg.TrimStart(), false);
                    }
                    else
                    {
                        // ２つ目以降のパラメータ

                        args += (codeType == CodeType.Sequece ? ";" : ",") + Environment.NewLine;
                        args += ParagraphConvert(nextTabLevel, arg.TrimStart(), false);
                    }
                }
                if (ElementType == CodeType.DummyArgument)
                {
                    return args.TrimStart().Replace("INDEX.", "");
                }
                if (ElementType == CodeType.none)
                {
                    Debug.Assert(ScriptElement is null);
                    return args;
                }
                if (!string.IsNullOrEmpty(args))
                {
                    BuildScriptInfo info = argList[0];
                    if (info.Child != null && info.Child.Count > 0 && info.Child[0].Attribute == AttributeType.Self)
                    {
                        // インスタンスメソッド（最初の引数は self）

                        if (args.Contains(','))
                        {
                            string instance = args.Substring(0, args.IndexOf(','));
                            args = args.Substring(args.IndexOf(',') + 1, args.Length - instance.Length - 1).TrimStart();
                            result = instance.TrimStart() + result.Substring(result.LastIndexOf("."));
                        }
                        else
                        {
                            result = args.TrimStart() + result.Substring(result.LastIndexOf("."));
                            args = "";
                        }
                    }
                    if (args.Split(Environment.NewLine).Length > 1)
                    {
                        // 引数が２個以上

                        result = ParagraphConvert(
                            tabLevel,
                            result.TrimStart() + (ElementType == CodeType.Method ? "" : " ") + GetOpenBrakets(codeType),
                            true);
                        result += args;
                        result += Environment.NewLine + ParagraphConvert(tabLevel, GetCloseBrakets(codeType), false);
                    }
                    else
                    {
                        // 引数が１個

                        string arg = args.Trim();
                        if (arg.Length > 0)
                        {
                            arg = " " + arg + " ";
                        }
                        if (result.Length + arg.Length > 80)
                        {
                            result = ParagraphConvert(
                                tabLevel,
                                result.TrimStart() + (ElementType == CodeType.Method ? "" : " ") + GetOpenBrakets(codeType),
                                true);
                            result += args;
                            result += Environment.NewLine + ParagraphConvert(tabLevel, GetCloseBrakets(codeType), false);

                        }
                        else
                        {
                            // １行で表現する

                            result += (ElementType == CodeType.Method ? "" : " ")
                                + GetOpenBrakets(codeType) + arg + GetCloseBrakets(codeType);
                        }
                    }
                }
                else
                {
                    result += GetOpenBrakets(codeType) + GetCloseBrakets(codeType);
                }
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
            string target = childRoot.Child[0]._BuildScript(null, nextTabLevel + TAB_SIZE);
            string defaultCall = childRoot.Child[2]._BuildScript(null, nextTabLevel + TAB_SIZE);

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
                    string caseCall = label._BuildScript(null, tabLevel + TAB_SIZE * 2);
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
