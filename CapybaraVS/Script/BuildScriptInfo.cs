using CapyCSS.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace CapyCSS.Script
{
    public struct BuildScriptInfo
    {
        public enum CodeType
        {
            none,
            Sequece,
            ResultSequece,
            Variable,
            DummyArgument,
            Enum,
            Method,
            List,
            Data,
            Delegate,
            ResultDelegate,
            StackVariable,
        }

        private const int TAB_SIZE = 4;

        public string ScriptElement = null;
        public List<BuildScriptInfo> Child = null;
        public CodeType ElementType = CodeType.Method;
        public string TypeName = null;
        public string Label = null;

        /// <summary>
        /// 要素を構築します。
        /// </summary>
        /// <param name="code">内容</param>
        /// <param name="scopeMode">タイプ</param>
        public BuildScriptInfo(string code, CodeType scopeMode = CodeType.Method, string label = null)
        {
            Set(code, scopeMode, label);
        }

        /// <summary>
        /// 要素を設定します。
        /// </summary>
        /// <param name="code">内容</param>
        /// <param name="scopeMode">タイプ</param>
        public void Set(string code, CodeType scopeMode = CodeType.Method, string label = null)
        {
            ScriptElement = code;
            ElementType = scopeMode;
            Label = label;
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
        /// 子を追加します。
        /// </summary>
        /// <param name="info">BuildScriptInfo</param>
        public void Add(BuildScriptInfo? info)
        {
            if (info.HasValue)
            {
                Child ??= new List<BuildScriptInfo>();
                Child.Add(info.Value);
            }
        }

        /// <summary>
        /// 子を追加します。
        /// </summary>
        /// <param name="infos">ScriptCodeInfoリスト</param>
        public void Add(List<BuildScriptInfo> infos)
        {
            Child ??= new List<BuildScriptInfo>();
            Child.AddRange(infos);
        }

        /// <summary>
        /// 子ノードを手繰って最初に ScriptCode が見つかる BuildScriptInfo を参照します。
        /// </summary>
        /// <param name="current">true:親も対象とする</param>
        /// <returns>最初に見つかったScriptCode</returns>
        public BuildScriptInfo? DeepGetCodeInfo(bool current = true)
        {
            if (current && ScriptElement != null)
                return this;
            if (Child == null)
                return null;
            foreach (BuildScriptInfo info in Child)
            {
                if (info.ScriptElement != null)
                    return info;
                BuildScriptInfo? result = info.DeepGetCodeInfo(true);
                if (result.HasValue)
                    return result;
            }
            return null;
        }

        /// <summary>
        /// 子ノードを手繰って最初に ScriptCode が見つかる BuildScriptInfo の ScriptCode を参照します。
        /// </summary>
        /// <param name="current">true:親も対象とする</param>
        /// <returns>最初に見つかったScriptCode</returns>
        public string DeepGetCode(bool current = true)
        {
            var result = DeepGetCodeInfo(current);
            if (result.HasValue)
                return result.Value.ScriptElement;
            return null;
        }

        /// <summary>
        /// 子ノードを手繰って最初に ScriptCode が見つかる BuildScriptInfo の CodeType を参照します。
        /// </summary>
        /// <param name="current">true:親も対象とする</param>
        /// <returns>最初に見つかったScriptCode</returns>
        public CodeType DeepGetCodeType(bool current = true)
        {
            var result = DeepGetCodeInfo(current);
            if (result.HasValue)
                return result.Value.ElementType;
            return CodeType.none;
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
            if (string.IsNullOrWhiteSpace(entryPointName))
            {
                entryPointName = CommandCanvasList.Instance.CurrentScriptTitle;
            }
            result = "var " + entryPointName + " = () =>";
            result += _BuildScript(entryPointName, 0);
            result += ";" + Environment.NewLine;
            result += $"AddEntryPoint(nameof({entryPointName}), {entryPointName});";
            result += Environment.NewLine + Environment.NewLine;
            return result;
        }

        /// <summary>
        /// c#スクリプトを構築します。
        /// </summary>
        /// <param name="entryPointName">エントリーポイント名</param>
        /// <param name="tabLevel">段組み階層</param>
        /// <returns>c#スクリプト</returns>
        private string _BuildScript(string entryPointName, int tabLevel = 0, CodeType codeType = CodeType.none)
        {
            string result = "";
            int nextTabLevel = tabLevel;
            if (ScriptElement != null)
            {
                if (ElementType == CodeType.Enum)
                {
                    BuildEnum(ref tabLevel, ref result, nextTabLevel);
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
                                result += arg1 + ";" + Environment.NewLine;
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

                                string arg = Child[0]._BuildScript(null, tabLevel + TAB_SIZE, CodeType.Sequece);

                                result = ParagraphConvert(tabLevel, result.TrimStart() + " = ", false);
                                result += arg.TrimStart();

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
                for (int i = 0; i < Child.Count; i++)
                {
                    BuildScriptInfo info = Child[i];
                    string arg = info._BuildScript(null, nextTabLevel, codeType);
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
                if (ElementType == CodeType.none)
                {
                    Debug.Assert(ScriptElement is null);
                    return args;
                }
                if (ElementType == CodeType.DummyArgument)
                {
                    return args.TrimStart().Replace("INDEX.", "");
                }
                if (!string.IsNullOrEmpty(args))
                {
                    if (args.Split(Environment.NewLine).Length > 1)
                    {
                        // 取得した構築コードが複数行なら一行表現を諦める

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

                        result += GetOpenBrakets(codeType) + " " + args.Trim() + " " + GetCloseBrakets(codeType);
                    }
                }
            }
            return result;
        }

        private void BuildEnum(ref int tabLevel, ref string result, int nextTabLevel)
        {
            var childRoot = Child[0];
            string target = childRoot.Child[0]._BuildScript(null, nextTabLevel + TAB_SIZE);
            string defaultCall = childRoot.Child[2]._BuildScript(null, nextTabLevel + TAB_SIZE);

            tabLevel += TAB_SIZE;
            result += "switch (" + target.TrimStart() + ") {" + Environment.NewLine;
            var caseList = childRoot.Child[1];
            foreach (var child in caseList.Child)
            {
                BuildScriptInfo? label = child.Child[0];
                if (label.HasValue && label.Value.ScriptElement != "null")
                {
                    string caseName = TypeName + "." + child.Label.Replace(":", "");
                    string caseCall = label.Value._BuildScript(null, tabLevel + TAB_SIZE * 2);
                    result += new String(' ', tabLevel) + "case " + caseName + ": " + GetOpenBrakets(0) + Environment.NewLine;
                    result += ParagraphConvert(tabLevel + TAB_SIZE, caseCall + ";");
                    result += ParagraphConvert(tabLevel + TAB_SIZE, "break;");
                }
            }
            result += ParagraphConvert(tabLevel, "default: ");
            defaultCall = defaultCall.TrimStart();
            if (defaultCall != "null")
            {
                result += GetOpenBrakets(0);
                result += defaultCall + Environment.NewLine;
            }
            result += ParagraphConvert(tabLevel + TAB_SIZE, "break;");
            tabLevel -= TAB_SIZE;
            result += ParagraphConvert(tabLevel, "}", false);
        }
    }
}
