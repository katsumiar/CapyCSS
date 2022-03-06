using CapyCSS.Controls.BaseControls;
using CapyCSS.Script;
using CapyCSS.Script.Lib;
using CapyCSS.Controls;
using CbVS.Script;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Xml.Serialization;
using static CapyCSS.Controls.BaseControls.CommandCanvas;

namespace CapyCSS.Controls
{
    public enum FunctionType
    {
        none,
        ConnectorType,
        /// <summary>
        /// 型
        /// </summary>
        LiteralType,
        /// <summary>
        /// アセットのタイプ
        /// </summary>
        FuncType,
    }

    /// <summary>
    /// MultiRootConnector.xaml の相互作用ロジック
    /// </summary>
    public partial class MultiRootConnector
        : UserControl
        , ICbExecutable
        , IDisposable
        , IHaveCommandCanvas
    {
        public static readonly int DATA_VERSION = 1;

        #region XML定義
        [XmlRoot(nameof(MultiRootConnector))]
        public class _AssetXML<OwnerClass> : IDisposable
            where OwnerClass : MultiRootConnector
        {
            [XmlIgnore]
            public Action WriteAction = null;
            [XmlIgnore]
            public Action<OwnerClass> ReadAction = null;
            private bool disposedValue;

            public _AssetXML()
            {
                ReadAction = (self) =>
                {
                    if (DataVersion != DATA_VERSION)
                    {
                        ControlTools.ShowErrorMessage(CapyCSS.Language.Instance["Help:DataVersionError"]);
                        return;
                    }

                    self.attachVariableIds = RootFunc.AttachVariableIds;

                    if (RootFunc.AttachFileName != null)
                        self.AttachParam = new AttachText(RootFunc.AttachFileName);
                    if (RootFunc.AttachVariableId != 0)
                        self.AttachParam = new AttachVariableId(RootFunc.AttachVariableId);

                    if (RootFunc.AssetValueType != null)
                    {
                        Type type = CbST.GetTypeEx(RootFunc.AssetValueType);
                        self.SelectedVariableType[0] = type;
                        self.SelectedVariableTypeName[0] = RootFunc.AssetValueType;
                    }
                    else
                    {
                        self.SelectedVariableTypes = RootFunc.AssetValueTypes.ToArray();
                    }

                    self.IsReBuildMode = true;
                    self.AssetFuncType = RootFunc.AssetFuncType;
                    self.AssetType = RootFunc.AssetType;

                    self.LinkConnectorControl.AssetXML = RootFunc.RootConnector;
                    self.LinkConnectorControl.AssetXML.ReadAction?.Invoke(self.LinkConnectorControl);

                    // 次回の為の初期化
                    self.AssetXML = new _AssetXML<MultiRootConnector>(self);
                };
            }

            public _AssetXML(OwnerClass self)
            {
                WriteAction = () =>
                {
                    DataVersion = DATA_VERSION;

                    RootFuncType info = new RootFuncType();

                    info.AttachVariableIds = self.attachVariableIds;

                    if (self.AttachParam is AttachText)
                        info.AttachFileName = (string)self.AttachParam.Value;
                    else
                        info.AttachFileName = null;

                    if (self.AttachParam is AttachVariableId)
                        info.AttachVariableId = (int)self.AttachParam.Value;
                    else
                        info.AttachVariableId = 0;

                    info.AssetType = self.AssetType;
                    info.AssetFuncType = self.AssetFuncType;
                    info.AssetValueTypes = new List<string>();
                    foreach (var type in self.SelectedVariableType)
                    {
                        if (type != null)
                        {
                            info.AssetValueTypes.Add(type.FullName);
                        }
                    }

                    self.LinkConnectorControl.AssetXML.WriteAction?.Invoke();
                    info.RootConnector = self.LinkConnectorControl.AssetXML;

                    RootFunc = info;
                };
            }
            #region 固有定義
            public class RootFuncType : IDisposable
            {
                public List<int> AttachVariableIds { get; set; } = null;
                public string AttachFileName { get; set; } = null;
                public int AttachVariableId { get; set; }
                [XmlAttribute(nameof(AssetType))]
                public FunctionType AssetType { get; set; }
                public string AssetValueType { get; set; } = null;
                public List<string> AssetValueTypes { get; set; } = null;
                [XmlAttribute(nameof(AssetFuncType))]
                public string AssetFuncType { get; set; } = null;
                public RootConnector._AssetXML<RootConnector> RootConnector { get; set; } = null;

                public void Dispose()
                {
                    AttachVariableIds?.Clear();
                    AttachVariableIds = null;
                    AttachFileName = null;
                    AssetValueType = null;
                    AssetValueTypes?.Clear();
                    AssetValueTypes = null;
                    AssetFuncType = null;
                    RootConnector?.Dispose();
                    RootConnector = null;

                    GC.SuppressFinalize(this);
                }
            }
            public int DataVersion { get; set; } = 0;
            public RootFuncType RootFunc { get; set; } = null;

            protected virtual void Dispose(bool disposing)
            {
                if (!disposedValue)
                {
                    if (disposing)
                    {
                        WriteAction = null;
                        ReadAction = null;

                        // 以下、固有定義開放
                        RootFunc?.Dispose();
                        RootFunc = null;
                    }
                    disposedValue = true;
                }
            }

            public void Dispose()
            {
                Dispose(disposing: true);
                GC.SuppressFinalize(this);
            }
            #endregion
        }
        public _AssetXML<MultiRootConnector> AssetXML { get; set; } = null;
        #endregion

        #region Function 添付プロパティ実装

        private static ImplementDependencyProperty<MultiRootConnector, Func<IList<ICbValue>, DummyArgumentsMemento, ICbValue>> impFunction =
            new ImplementDependencyProperty<MultiRootConnector, Func<IList<ICbValue>, DummyArgumentsMemento, ICbValue>>(
                nameof(Function),
                (self, getValue) =>
                {
                    Func<IList<ICbValue>, DummyArgumentsMemento, ICbValue> value = getValue(self);
                    self.LinkConnectorControl.Function = value;
                });

        public static readonly DependencyProperty FunctionProperty = impFunction.Regist(null);

        public Func<IList<ICbValue>, DummyArgumentsMemento, ICbValue> Function
        {
            get { return impFunction.GetValue(this); }
            set { impFunction.SetValue(this, value); }
        }

        #endregion

        #region AttachParam 添付プロパティ実装

        public interface IAttachParam
        {
            object Value { get; set; }
        }

        public class AttachText : IAttachParam
        {
            public object Value { get; set; } = null;
            public AttachText(string test)
            {
                Value = test;
            }
        }

        public class AttachVariableId : IAttachParam
        {
            public object Value { get; set; } = null;
            public AttachVariableId(int id)
            {
                Value = id;
            }
        }

        private static ImplementDependencyProperty<MultiRootConnector, IAttachParam> impAttachParam =
            new ImplementDependencyProperty<MultiRootConnector, IAttachParam>(
                nameof(AttachParam),
                (self, getValue) =>
                {
                    IAttachParam value = getValue(self);
                    if (value is AttachVariableId vid)
                    {
                        // 古い仕様との互換用

                        if (self.attachVariableIds.Count > 0)
                            self.attachVariableIds[0] = (int)vid.Value;
                        else
                            self.attachVariableIds.Add((int)vid.Value);
                    }
                    else if (value is AttachText afn)
                    {
                        string text = (string)afn.Value;

                        ICbValue setValue = null;
                        if (text.Contains(','))
                        {
                            // , と数字だけだと double と判定されるので強引に string にする

                            setValue = toStringNode(self, ref text);
                        }
                        else if (int.TryParse(text, out int intNum))
                        {
                            setValue = CbInt.Create(intNum);
                        }
                        else if (long.TryParse(text, out long longNum))
                        {
                            setValue = CbLong.Create(longNum);
                        }
                        else if (double.TryParse(text, out double doubleNum))
                        {
                            setValue = CbDouble.Create(doubleNum);
                        }
                        else if (bool.TryParse(text, out bool boolNum))
                        {
                            setValue = CbBool.Create(boolNum);
                        }
                        else
                        {
                            setValue = toStringNode(self, ref text);
                        }

                        {
                            self.SelectedVariableType[0] = setValue.OriginalType;
                            self.SelectedVariableTypeName[0] = setValue.OriginalType.FullName;
                        }

                        self.AssetType = FunctionType.LiteralType;
                        if (self.LinkConnectorControl.ValueData != null && setValue != null)
                        {
                            self.LinkConnectorControl.Caption = text;
                            self.LinkConnectorControl.ValueData = setValue;
                            self.LinkConnectorControl.UpdateValueData();
                        }
                    }
                });

        /// <summary>
        /// 画像のパスとして有効なら ImagePath 型に変換します。
        /// string型に変換します。
        /// 複数行の場合は、text型に変換します。
        /// </summary>
        /// <param name="self">所属のクラスインスタンス</param>
        /// <param name="text">テキスト</param>
        /// <returns>型</returns>
        private static ICbValue toStringNode(MultiRootConnector self, ref string text)
        {
            ICbValue setValue;
            if (text.Contains(Environment.NewLine))
            {
                setValue = CbText.Create(text, "");
                text = text.Split(Environment.NewLine)[0];
            }
            else
            {
                var imageExtensions = new List<string>() { ".png", ".gif", ".jpg", ".bmp" };
                if (System.IO.File.Exists(text) && imageExtensions.Contains(System.IO.Path.GetExtension(text.ToLower())))
                {
                    setValue = CbImagePath.Create(text, "");
                }
                else
                {
                    setValue = CbString.Create(text, "");
                }
            }
            if (text.Length > 20)
            {
                text = text.Substring(0, 20) + "...";
            }
            return setValue;
        }

        public static readonly DependencyProperty AttachParamProperty = impAttachParam.Regist(null);

        public IAttachParam AttachParam
        {
            get { return impAttachParam.GetValue(this); }
            set { impAttachParam.SetValue(this, value); }
        }

        /// <summary>
        /// アタッチ変数配列
        /// ※引数の数だけ用意される可能性がある
        /// </summary>
        private List<int> attachVariableIds = new List<int>();
        public void AddAttachVariableId(int id)
        {
            if (attachVariableIds.Count == 0)
                AttachParam = new AttachVariableId(id);
            attachVariableIds.Add(id);
        }

        public int GetAttachVariableId(int index)
        {
            return attachVariableIds[index];
        }

        public int GetAttachVariableIdsCount() { return attachVariableIds.Count; }

        #endregion

        private CommandCanvas _OwnerCommandCanvas = null;

        public CommandCanvas OwnerCommandCanvas
        {
            get => _OwnerCommandCanvas;
            set
            {
                Debug.Assert(value != null);
                if (LinkConnectorControl.OwnerCommandCanvas is null)
                    LinkConnectorControl.OwnerCommandCanvas = value;
                if (_OwnerCommandCanvas is null)
                    _OwnerCommandCanvas = value;
            }
        }

        private string debugCreateName = "";
        public MultiRootConnector(object self, string _ext = "", [CallerMemberName] string callerMethodName = "")
        {
            CommandCanvas.SetDebugCreateList(ref debugCreateName, this, self, callerMethodName, _ext);
            InitializeComponent();
            AssetXML = new _AssetXML<MultiRootConnector>(this);
        }

        public MultiRootConnector AppendArgument(ICbValue variable, bool literalType = false)
        {
            LinkConnectorControl.AppendArgument(variable, literalType);
            return this;
        }

        /// <summary>
        /// Function イベント実行依頼
        /// </summary>
        /// <param name="functionStack"></param>
        public object RequestExecute(List<object> functionStack = null, DummyArgumentsMemento preArgument = null)
        {
            return LinkConnectorControl.RequestExecute(functionStack, preArgument);
        }

        public BuildScriptInfo RequestBuildScript()
        {
            return LinkConnectorControl.RequestBuildScript();
        }

        //--------------------------------------------------------------------------------
        #region ファンクションアセット機能

        public string[] SelectedVariableTypes
        {
            set
            {
                foreach (var node in value.Select((name, index) => new { name, index }))
                {
                    Type type = CbST.GetTypeEx(node.name);
                    SelectedVariableType[node.index] = type;
                    LinkConnectorControl.SelectedVariableType[node.index] = type;
                    SelectedVariableTypeName[node.index] = CbSTUtils.GetTypeName(type);
                }
            }
        }

        private Type[] selectedVariableType = new Type[16];
        public Type[] SelectedVariableType
        {
            get => selectedVariableType;
            set
            {
                selectedVariableType = value;
                LinkConnectorControl.SelectedVariableType = value;
            }
        }

        private string[] selectedVariableTypeName = new string[16];
        public string[] SelectedVariableTypeName
        {
            get => selectedVariableTypeName;
            set
            {
                selectedVariableTypeName = value;
            }
        }

        #region AssetType 添付プロパティ実装

        private static ImplementDependencyProperty<MultiRootConnector, FunctionType> impAssetType =
            new ImplementDependencyProperty<MultiRootConnector, FunctionType>(
                nameof(AssetType),
                (self, getValue) =>
                {
                    FunctionType value = getValue(self);
                    self.MakeFunction();
                });

        public static readonly DependencyProperty AssetTypeProperty = impAssetType.Regist(FunctionType.none);

        public FunctionType AssetType
        {
            get { return impAssetType.GetValue(this); }
            set { impAssetType.SetValue(this, value); }
        }

        #endregion

        #region AssetFuncType 添付プロパティ実装

        private static ImplementDependencyProperty<MultiRootConnector, string> impAssetFuncType =
            new ImplementDependencyProperty<MultiRootConnector, string>(
                nameof(AssetFuncType),
                (self, getValue) =>
                {
                    string value = getValue(self);
                });

        public static readonly DependencyProperty AssetFuncTypeProperty = impAssetFuncType.Regist("none");

        public string AssetFuncType
        {
            get { return impAssetFuncType.GetValue(this); }
            set { impAssetFuncType.SetValue(this, value); }
        }

        #endregion

        #region OldSpecification 添付プロパティ実装

        private static ImplementDependencyProperty<MultiRootConnector, bool> impOldSpecification =
            new ImplementDependencyProperty<MultiRootConnector, bool>(
                nameof(OldSpecification),
                (self, getValue) =>
                {
                    bool value = getValue(self);
                    self.oldSpecification.Visibility = value ? Visibility.Visible : Visibility.Hidden;
                });

        public static readonly DependencyProperty OldSpecificationProperty = impOldSpecification.Regist(false);

        /// <summary>
        /// 古い仕様指定
        /// </summary>
        public bool OldSpecification
        {
            get { return impOldSpecification.GetValue(this); }
            set { impOldSpecification.SetValue(this, value); }
        }

        #endregion

        #region FunctionInfo 添付プロパティ実装

        private static ImplementDependencyProperty<MultiRootConnector, IBuildScriptInfo> impFunctionInfo =
            new ImplementDependencyProperty<MultiRootConnector, IBuildScriptInfo>(
                nameof(FunctionInfo),
                (self, getValue) =>
                {
                    var value = getValue(self);
                    self.LinkConnectorControl.FunctionInfo = value;
                });

        public static readonly DependencyProperty FunctionInfoProperty = impFunctionInfo.Regist(null);

        public IBuildScriptInfo FunctionInfo
        {
            get { return impFunctionInfo.GetValue(this); }
            set { impFunctionInfo.SetValue(this, value); }
        }

        #endregion

        #region IsRunable 添付プロパティ実装

        private static ImplementDependencyProperty<MultiRootConnector, bool> impIsRunable =
            new ImplementDependencyProperty<MultiRootConnector, bool>(
                nameof(IsRunable),
                (self, getValue) =>
                {
                    self.LinkConnectorControl.IsRunable = getValue(self);
                });

        public static readonly DependencyProperty IsRunableProperty = impIsRunable.Regist(false);

        /// <summary>
        /// 任意実行可能ノードか？（RUNボタンが追加される）
        /// </summary>
        public bool IsRunable
        {
            get { return impIsRunable.GetValue(this); }
            set { impIsRunable.SetValue(this, value); }
        }

        #endregion

        /// <summary>
        /// アセットリスト
        /// </summary>
        public static ICollection<IFuncAssetDef> AssetFunctionList = new List<IFuncAssetDef>();

        public Func<string, string> GetVariableName = (name) => "[ " + name + " ]";
        public void VariableUpdate()
        {
            if (AttachParam is AttachVariableId variable)
            {
                string valueName = "!ERROR!";
                try
                {
                    int id = (int)variable.Value;
                    foreach (var node in OwnerCommandCanvas.ScriptWorkStack.StackData)
                    {
                        if (node.stackNode.Id == id)
                        {
                            valueName = node.stackNode.ValueData.Name;
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                }

                if (GetVariableName != null)
                    valueName = GetVariableName(valueName);
                LinkConnectorControl.Caption = valueName;
                LinkConnectorControl.UpdateValueData();
            }
        }

        public Type GetRequestType(IList<TypeRequest> typeRequests, string name)
        {
            return LinkConnectorControl.GetRequestType(typeRequests, name);
        }

        //--------------------------------------------------------------------------------
        public void MakeFunction(
            string name,
            string hint,
            Func<ICbValue> retType,
            IEnumerable<ICbValue> argumentList,
            Func<IList<ICbValue>, DummyArgumentsMemento, ICbValue> func
            )
        {
            LinkConnectorControl.OwnerCommandCanvas = OwnerCommandCanvas;
            LinkConnectorControl.Caption = name;
            ICbValue returnType = retType();
            returnType.IsReadOnlyValue = true;
            LinkConnectorControl.ValueData = returnType;
            LinkConnectorControl.Hint = hint;
            Function = func;
            if (argumentList != null)
            {
                foreach (var node in argumentList)
                {
                    // 引数を追加する

                    AppendArgument(node);
                }
            }
            LinkConnectorControl.UpdateMainPanelColor();
        }

        private bool MakeLiteral(Func<ICbValue> literalType)
        {
            if (literalType != null)
            {
                var value = literalType();
                LinkConnectorControl.OwnerCommandCanvas = OwnerCommandCanvas;
                LinkConnectorControl.Caption = literalType().TypeName;
                LinkConnectorControl.ValueData = value;

                // 引数を追加する
                AppendArgument(value, true);

                // コンストラクタ表示
                var methodInfo = new BuildScriptFormat(nameof(LiteralType), typeof(LiteralType));
                methodInfo.IsConstructor = (value.OriginalType != typeof(object) && value.OriginalType != typeof(string)) &&
                    (value.OriginalType.IsClass || CbStruct.IsStruct(value.OriginalType) || value.IsList);
                FunctionInfo = methodInfo;
                LinkConnectorControl.UpdateMainPanelColor();
                return true;
            }
            return false;
        }

        private void MakeMultiRootConnector(string name = "Root")
        {
            LinkConnectorControl.Caption = nameof(MultiRootConnector);
            LinkConnectorControl.ValueData = new ParamNameOnly(name);
        }

        //--------------------------------------------------------------------------------

        private void MakeFunction()
        {
            if (AssetType == FunctionType.none)
                return;

            if (AssetType == FunctionType.ConnectorType)
                MakeMultiRootConnector();

            if (AssetType == FunctionType.LiteralType)
            {
                Func<ICbValue> tf = CbST.CbCreateTF(SelectedVariableType[0]);
                MakeLiteral(tf);
            }

            if (AssetType == FunctionType.FuncType)
                MakeFunctionType();
        }

        /// <summary>
        /// メインログにエラーメッセージを出力し、エラーメッセージを返します。
        /// </summary>
        /// <param name="ex">例外</param>
        /// <param name="rootConnectorControl">エラーを起こした RootConnector</param>
        /// <returns>エラーメッセージ</returns>
        static public string ExceptionFunc(Exception ex, RootConnector rootConnectorControl)
        {
            return ExceptionFunc(ex, rootConnectorControl.Caption);
        }

        /// <summary>
        /// メインログにエラーメッセージを出力し、エラーメッセージを返します。
        /// </summary>
        /// <param name="ex">例外</param>
        /// <param name="caption">エラータイトル</param>
        /// <returns>エラーメッセージ</returns>
        static public string ExceptionFunc(Exception ex, string caption)
        {
            string msg;
            if (ex.InnerException != null)
                msg = ex.InnerException.Message;
            else
                msg = ex.Message;
            msg += Environment.NewLine;
            msg += ex.StackTrace;
            msg = caption + ": " + msg;
            Console.WriteLine(msg);
            System.Diagnostics.Debug.WriteLine(msg);
            return msg;
        }

        /// <summary>
        /// メインログにエラーメッセージを出力します。
        /// </summary>
        /// <param name="ex">例外</param>
        public void ExceptionFunc(Exception ex)
        {
            ExceptionFunc(ex, LinkConnectorControl);
        }

        /// <summary>
        /// メインログにエラーメッセージを出力します。
        /// </summary>
        /// <param name="variable">エラーをセットする変数</param>
        /// <param name="ex">例外</param>
        /// <param name="rootConnectorControl">エラーを起こした RootConnector</param>
        static public void ExceptionFunc(ICbValue variable, Exception ex, RootConnector rootConnectorControl)
        {
            ExceptionFunc(ex, rootConnectorControl);
            if (variable != null)
            {
                variable.IsError = true;
                if (ex.InnerException != null)
                    variable.ErrorMessage = ex.InnerException.Message;
                else
                    variable.ErrorMessage = ex.Message;
            }
        }

        /// <summary>
        /// メインログにエラーメッセージを出力します。
        /// </summary>
        /// <param name="variable">エラーをセットする変数</param>
        /// <param name="ex">例外</param>
        public void ExceptionFunc(ICbValue variable, Exception ex)
        {
            ExceptionFunc(variable, ex, LinkConnectorControl);
        }

        /// <summary>
        /// 再構築モードか？
        /// </summary>
        private bool IsReBuildMode = false;

        private void MakeFunctionType()
        {
            IFuncAssetDef asset = AssetFunctionList.FirstOrDefault(m => m.AssetCode == AssetFuncType);
            if (asset is null)
            {
                // 恐らく古い形式のアセットコード

                asset = AssetFunctionList.FirstOrDefault(m => m.AssetCode.Contains(AssetFuncType));
                if (asset is null)
                {
                    // 存在しないアセットコード

                    Console.WriteLine("Implement Error: " + AssetFuncType);
                    return;
                }
                AssetFuncType = asset.AssetCode;    // 正しいアセットコードにリセットする
            }
            if (asset != null)
            {
                if (IsReBuildMode)
                {
                    // スレッドとの絡みでノード間の接続が面倒になっている……

                    if (!asset.ImplAsset(this, IsReBuildMode))
                    {
                        // 失敗かキャンセル

                        // コントロールを削除する
                        OwnerCommandCanvas.ScriptWorkCanvas.Remove(ControlTools.FindAncestor<Movable>(this));
                    }
                }
                else
                {
                    Dispatcher.BeginInvoke(
                        new Action(() =>
                            {
                                if (!asset.ImplAsset(this, IsReBuildMode))
                                {
                                    // 失敗かキャンセル

                                    // コントロールを削除する
                                    OwnerCommandCanvas.ScriptWorkCanvas.Remove(ControlTools.FindAncestor<Movable>(this));
                                }
                            }
                        ), DispatcherPriority.Loaded);
                }
            }
        }
        #endregion

        #region IDisposable Support
        private bool disposedValue = false; // 重複する呼び出しを検出するには

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    LinkConnectorControl?.Dispose();

                    if (AttachParam is AttachVariableId variable)
                    {
                        // 変数アセットは参照の解除を知らせる

                        OwnerCommandCanvas.ScriptWorkStack?.Unlink((int)variable.Value, this);
                    }
                    //LinkConnectorControl = null;

                    AssetXML?.Dispose();
                    AssetXML = null;
                    attachVariableIds = null;
                    _OwnerCommandCanvas = null;
                    selectedVariableType = null;
                    selectedVariableTypeName = null;
                    //AssetFunctionList = null; // static なので消しては駄目
                    GetVariableName = null;

                    CommandCanvas.RemoveDebugCreateList(debugCreateName);
                }
                disposedValue = true;
            }
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
