﻿using CapyCSS.Script;
using CapyCSS.Controls;
using CbVS.Script;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
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
using CapyCSS.Script.Lib;
using CapyCSSbase;
using static CapyCSS.Controls.BaseControls.CommandCanvas;

namespace CapyCSS.Controls.BaseControls
{
    /// <summary>
    /// RootConnector.xaml の相互作用ロジック
    /// </summary>
    public partial class RootConnector 
        : UserControl
        , ICurveLinkRoot
        , ILinkCheck
        , IDisposable
    {
        #region ID管理
        private PointIdProvider pointIdProvider = null;
        public int TargetPointId 
        {
            get => pointIdProvider.PointId;
            set { pointIdProvider.PointId = value; }
        }
        #endregion

        #region XML定義
        [XmlRoot(nameof(RootConnector))]
        public class _AssetXML<OwnerClass> : IDisposable
            where OwnerClass : RootConnector
        {
            //private static int queueCounter = 0;
            [XmlIgnore]
            public Action WriteAction = null;
            [XmlIgnore]
            public Action<OwnerClass> ReadAction = null;
            private bool disposedValue;

            public _AssetXML()
            {
                ReadAction = (self) =>
                {
                    self.IsInitializing = true;
                    try
                    {
                        self.TargetPointId = PointId;

                        self.FuncCaption.AssetXML = Caption;
                        self.FuncCaption.AssetXML.ReadAction?.Invoke(self.FuncCaption);

                        if (Value != null && self.ValueData != null && Value != CbSTUtils.ERROR_STR)
                        {
                            if (self.ValueData.IsStringableValue)
                            {
                                if (self.ValueData.IsSecretString)
                                {
                                    Value = CbPassword.Decrypt(Value);
                                }
                                self.ValueData.ValueString = Value;
                            }
                            self.NameText.UpdateValueData();
                        }

                        self.ForcedChecked = ForcedChecked;
                        self.IsPublicExecute.IsChecked = IsPublicExecute;
                        if (EntryPointName != null)
                        {
                            self.EntryPointName.Text = EntryPointName;
                        }

                        for (int i = 0; i < Arguments.Count; ++i)
                        {
                            if (i >= self.ListData.Count)
                                break;  // ここで作らなくても問題ない
                            self.ListData[i].AssetXML = Arguments[i];
                            self.ListData[i].AssetXML.ReadAction?.Invoke(self.ListData[i]);
                        }
                    }
                    finally
                    {
                        self.IsInitializing = false;
                    }

                    // レイアウトが変更されるのでレイアウトの変更を待って続きを処理する必要がある
                    self.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        // レイアウト処理が終わるまで処理できない
                        self.ChangeConnectorStyle(self.SingleLinkMode);

                        Debug.Assert(self.CurveCanvas != null); // キャンバスが取得できるようになっている筈

                        self.rootCurveLinks.AssetXML = Connector;
                        self.rootCurveLinks.AssetXML.ReadAction?.Invoke(self.rootCurveLinks);

                        self.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            // ノードを起き終わってから接続線を繋げる

                            PointIdProvider.CheckRequestStart();
                        }), DispatcherPriority.Background);

                    }), DispatcherPriority.Loaded);

                    // 次回の為の初期化
                    self.AssetXML = new _AssetXML<RootConnector>(self);
                };
            }
            public _AssetXML(OwnerClass self)
            {
                WriteAction = () =>
                {
                    PointId = self.TargetPointId;
                    self.FuncCaption.AssetXML.WriteAction?.Invoke();
                    Caption = self.FuncCaption.AssetXML;

                    if (!(self.ValueData is ICbValueList) && self.ValueData != null)
                    {
                        if (self.ValueData.IsStringableValue)
                        {
                            Value = self.ValueData.ValueString;
                            if (self.ValueData.IsSecretString)
                            {
                                Value = CbPassword.Encrypt(Value);
                            }
                        }
                    }

                    ForcedChecked = self.ForcedChecked;

                    self.rootCurveLinks.AssetXML.WriteAction?.Invoke();
                    Connector = self.rootCurveLinks.AssetXML;

                    IsPublicExecute = self.IsPublicExecute.IsChecked == true;
                    EntryPointName = self.EntryPointName.Text;

                    Arguments = new List<LinkConnector._AssetXML<LinkConnector>>();
                    foreach (var node in self.ListData)
                    {
                        node.AssetXML.WriteAction?.Invoke();
                        Arguments.Add(node.AssetXML);
                    }
                };
            }
            [XmlAttribute(nameof(PointId))]
            public int PointId { get; set; } = 0;
            #region 固有定義
            public NameLabel._AssetXML<NameLabel> Caption { get; set; } = null;
            public string Value { get; set; } = null;
            public bool ForcedChecked { get; set; } = false;
            public RootCurveLinks._AssetXML<RootCurveLinks> Connector { get; set; } = null;
            [XmlArrayItem("LinkConnector")]
            public List<LinkConnector._AssetXML<LinkConnector>> Arguments { get; set; } = null;
            public bool IsPublicExecute { get; set; } = false;
            public string EntryPointName { get; set; } = "";

            protected virtual void Dispose(bool disposing)
            {
                if (!disposedValue)
                {
                    if (disposing)
                    {
                        WriteAction = null;
                        ReadAction = null;

                        // 以下、固有定義開放
                        Caption?.Dispose();
                        Caption = null;
                        Value = null;
                        Connector?.Dispose();
                        Connector = null;
                        CbSTUtils.ForeachDispose(Arguments);
                        Arguments = null;
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
        public _AssetXML<RootConnector> AssetXML { get; set; } = null;
        #endregion

        private CurvePath curvePath = null;
        private RootCurveLinks rootCurveLinks = null;

        /// <summary>
        /// ノード接続数を参照します。
        /// </summary>
        private int LinkCount
        {
            get
            {
                if (rootCurveLinks is null)
                    return 0;
                return rootCurveLinks.Count;
            }
        }
        private Point backupPos = new Point(0, 0);

        private bool singleLinkMode = false;

        public ObservableCollection<LinkConnector> ListData { get; set; } = new ObservableCollection<LinkConnector>();

        #region Caption プロパティ実装

        private static ImplementDependencyProperty<RootConnector, string> impCaption =
            new ImplementDependencyProperty<RootConnector, string>(
                nameof(Caption),
                (self, getValue) =>
                {
                    self.FuncCaption.LabelString = getValue(self);
                });

        public static readonly DependencyProperty CaptionProperty = impCaption.Regist("(none)");

        public string Caption
        {
            get { return impCaption.GetValue(this); }
            set { impCaption.SetValue(this, value); }
        }

        #endregion

        #region CaptionReadOnly プロパティ実装

        private static ImplementDependencyProperty<RootConnector, bool> impCaptionReadOnly =
            new ImplementDependencyProperty<RootConnector, bool>(
                nameof(CaptionReadOnly),
                (self, getValue) =>
                {
                    self.FuncCaption.ReadOnly = getValue(self);
                });

        public static readonly DependencyProperty CaptionReadOnlyProperty = impCaptionReadOnly.Regist(false);

        public bool CaptionReadOnly
        {
            get { return impCaptionReadOnly.GetValue(this); }
            set { impCaptionReadOnly.SetValue(this, value); }
        }

        #endregion

        #region Function 添付プロパティ実装

        private static ImplementDependencyProperty<RootConnector, Func<IList<ICbValue>, DummyArgumentsMemento, ICbValue>> impFunction =
            new ImplementDependencyProperty<RootConnector, Func<IList<ICbValue>, DummyArgumentsMemento, ICbValue>>(
                nameof(Function),
                (self, getValue) =>
                {
                    //Func<List<ICbVSValue>, CbPushList, ICbVSValue> value = getValue(self);
                });

        public static readonly DependencyProperty FunctionProperty = impFunction.Regist(null);

        public Func<IList<ICbValue>, DummyArgumentsMemento, ICbValue> Function
        {
            get { return impFunction.GetValue(this); }
            set { impFunction.SetValue(this, value); }
        }

        #endregion

        #region RunCommand 添付プロパティ実装

        private static ImplementDependencyProperty<RootConnector, RelayCommand> impRelayCommand =
            new ImplementDependencyProperty<RootConnector, RelayCommand>(
                nameof(RunCommand),
                (self, getValue) =>
                {
                    RelayCommand value = getValue(self);
                    self.ExecuteButtunControl.Command = value;
                });

        public static readonly DependencyProperty RunCommandProperty = impRelayCommand.Regist(null);

        public RelayCommand RunCommand
        {
            get { return impRelayCommand.GetValue(this); }
            set { impRelayCommand.SetValue(this, value); }
        }

        #endregion

        #region IsRunable 添付プロパティ実装

        private static ImplementDependencyProperty<RootConnector, bool> impIsRunable =
            new ImplementDependencyProperty<RootConnector, bool>(
                nameof(IsRunable),
                (self, getValue) =>
                {
                    bool value = getValue(self);
                    self.ExecuteButtunControl.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
                    self.IsPublicExecute.Visibility = self.ExecuteButtunControl.Visibility;

                    if (value)
                    {
                        self.OwnerCommandCanvas.CommandCanvasControl.AddAllExecuteEntryPointEnable(self.SetExecuteButtonEnable);
                    }
                    else
                    {
                        self.OwnerCommandCanvas.CommandCanvasControl.RemoveAllExecuteEntryPointEnable(self.SetExecuteButtonEnable);
                    }
                });

        public static readonly DependencyProperty IsRunableProperty = impIsRunable.Regist(false);

        /// <summary>
        /// 任意実行可能ノード（RUNボタンが追加される）
        /// </summary>
        public bool IsRunable
        {
            get { return impIsRunable.GetValue(this); }
            set { impIsRunable.SetValue(this, value); }
        }

        #endregion

        /// <summary>
        /// ジェネリックメソッドの場合のジェネリックパラメータです。
        /// </summary>
        public Type[] SelectedVariableType = new Type[16];

        public Type GetRequestType(IList<TypeRequest> typeRequests, string name)
        {
            for (int i = 0; i < typeRequests.Count; i++)
            {
                TypeRequest typeRequest = typeRequests[i];
                if (typeRequest.Name == name)
                {
                    // 対応する型が見つかった

                    return SelectedVariableType[i];
                }
            }
            return null;
        }

        private IBuildScriptInfo functionInfo = null;
        public IBuildScriptInfo FunctionInfo 
        {
            get => functionInfo;
            set
            {
                functionInfo = value;

                bool isConstructor = functionInfo != null && functionInfo.IsConstructor;
                bool isList = functionInfo != null && IsSetVariableFunction() && CbList.HaveInterface(functionInfo.ClassType, typeof(IEnumerable<>));

                if (isConstructor || isList)
                {
                    // ノードデザインをコンストラクタにする

                    SetupConstructorNodeDesign();
                }
                else if (functionInfo != null && functionInfo.ClassType == typeof(DummyArguments))
                {
                    // ノードデザインを仮引数参照にする

                    SetupDummyArgumentsNodeDesign();
                }
                if (ValueData != null && ValueData.IsDelegate && IsGetVariableFunction())
                {
                    // 変数参照デリゲートをマークする

                    ValueData.IsVariableDelegate = true;
                }
            }
        }

        private void SetupDummyArgumentsNodeDesign()
        {
            RectBox.RadiusX = 20;
            RectBox.RadiusY = 20;
        }

        private void SetupConstructorNodeDesign()
        {
            RectBox.RadiusX = 10;
            RectBox.RadiusY = 10;
            RectBox.Fill = (Brush)Application.Current.FindResource("ConstructorNodeBackgroundBrush");
            NameText.Fill = (Brush)Application.Current.FindResource("ConstructorTypeBackgroundBrush");
        }

        private void SetupNormalNodeDesign()
        {
            RectBox.RadiusX = 6;
            RectBox.RadiusY = 6;
            RectBox.Fill = (Brush)Application.Current.FindResource("NormalNodeBackgroundBrush");
            NameText.Fill = (Brush)Application.Current.FindResource("ParamTypeBackgroundBrush");
        }

        /// <summary>
        /// EntryPoint指定時のノード背景色です。
        /// </summary>
        private Brush NodeEntryColor = (Brush)Application.Current.FindResource("EntryPointNodeBackgroundBrush");

        private CommandCanvas _OwnerCommandCanvas = null;

        public CommandCanvas OwnerCommandCanvas
        {
            get => _OwnerCommandCanvas;
            set
            {
                Debug.Assert(value != null);
                SetOunerCanvas(ListData, value);
                if (NameText.OwnerCommandCanvas is null)
                {
                    NameText.OwnerCommandCanvas = value;
                }
                if (_OwnerCommandCanvas is null)
                {
                    _OwnerCommandCanvas = value;
                    ChangeConnectorStyle(SingleLinkMode);
                }
            }
        }

        private void SetOunerCanvas(IEnumerable<LinkConnector> list, CommandCanvas value)
        {
            if (list is null)
                return;

            foreach (var node in list)
            {
                if (node.OwnerCommandCanvas is null)
                {
                    node.OwnerCommandCanvas = value;
                }
            }
        }

        private string debugCreateName = "";
        public RootConnector()
        {
            CommandCanvas.SetDebugCreateList(ref debugCreateName, this);
            InitializeComponent();
            pointIdProvider = new PointIdProvider(this);
            AssetXML = new _AssetXML<RootConnector>(this);
            DataContext = this;
            Box.ItemsSource = ListData;

            ChangeLinkConnectorStroke();
            RectBox.Stroke = RectboxStroke;
            CheckBoxVisibility();

            Forced.ToolTip = CapyCSS.Language.Instance["SYSTEM_ArgumentForced"];
            IsPublicExecute.ToolTip = CapyCSS.Language.Instance["SYSTEM_IsPublicExecute"];

            FuncCaption.UpdateEvent = () =>
                {
                    // ノード名が変更された

                    OwnerCommandCanvas.RecordUnDoPoint(CapyCSS.Language.Instance["Help:SYSTEM_COMMAND_EditNodeName"]);
                };

            NameText.UpdateEvent = () =>
                {
                    // 情報の変更を接続先に伝える

                    rootCurveLinks?.RequestUpdateRootValue();
                    OwnerCommandCanvas.RecordUnDoPoint(CapyCSS.Language.Instance["Help:SYSTEM_COMMAND_EditArgumentInformation"]);
                };

            // ノードの実行ボタンに実行機能を登録します。
            RunCommand = new RelayCommand(
                (list) =>
                {
                    // スクリプトを処理する

                    ExecuteRoot(false, EntryPointName.Text);
                }
                );
            
            LayoutUpdated += _LayoutUpdated;
        }

        /// <summary>
        /// エントリーポイント名を取得します。
        /// </summary>
        /// <returns></returns>
        public string GetEntryPointName()
        {
            if (!IsPublicExecute.IsChecked.Value)
            {
                return "";
            }
            string entryPointName = EntryPointName.Text.Trim();
            return entryPointName;
        }

        /// <summary>
        /// このスクリプトノードを起点にスクリプトを実行します。
        /// </summary>
        /// <param name="fromScript">スクリプトから呼ぶ時==true</param>
        /// <param name="entryPointName">エントリーポイント名</param>
        /// <returns>返り値</returns>
        public object ExecuteRoot(bool fromScript, string entryPointName = null)
        {
            if (!fromScript)
            {
                if (entryPointName.StartsWith(":"))
                {
                    if (entryPointName == ":*")
                    {
                        // スクリプトコードを取得

                        var sc = RequestBuildScript();
                        if (sc != null)
                        {
                            string name = GetEntryPointName();
                            return sc != null ? sc.BuildScript(name) : null;
                        }
                    }
                    if (entryPointName != null)
                    {
                        if (entryPointName.StartsWith(":+"))
                        {
                            // エントリーポイント名の衝突チェック

                            SetPickupEntryPoint(OwnerCommandCanvas.CommandCanvasControl.IsExistEntryPoint(OwnerCommandCanvas, GetEntryPointName()));
                            return null;
                        }

                        if (entryPointName.Substring(1) == GetEntryPointName())
                        {
                            // 名前が一致している

                            return true;
                        }
                    }
                    return null;    // false
                }

                if (CommandCanvasList.IsCursorLock())
                    return null;
            }

            try
            {
                if (!fromScript)
                {
                    OwnerCommandCanvas.ShowRunningPanel();
                }
                return _ExecuteRoot(fromScript, entryPointName);
            }
            finally
            {
                if (!fromScript)
                {
                    OwnerCommandCanvas.HideRunningPanel();
                }
            }
        }

        public object _ExecuteRoot(bool fromScript, string entryPointName = null)
        {
            object result = null;

            CommandCanvasList.SetOwnerCursor(Cursors.Wait);
            try
            {
                if (!fromScript)
                {
                    OwnerCommandCanvas.CommandCanvasControl.CallAllExecuteEntryPointEnable(false);
                    OwnerCommandCanvas.CommandCanvasControl.MainLog.TryAutoClear();
                    GC.Collect();
                }

                if (IsPublicExecute.IsChecked.Value &&
                    !(entryPointName is null && GetEntryPointName().Length == 0) &&
                    (entryPointName is null || entryPointName != GetEntryPointName()))
                {
                    // エントリーポイントに名前が付けられていて、且つ名前が一致しない

                    OwnerCommandCanvas.CommandCanvasControl.CallAllExecuteEntryPointEnable(true);
                    return null;
                }

                // スクリプトを実行する

                Stopwatch sw = null;
                if (!fromScript)
                {
                    OwnerCommandCanvas.EnabledScriptHoldActionMode = true;  // 表示更新処理を保留する

                    sw = new Stopwatch();
                    sw.Start();
                }

                OwnerCommandCanvas.WorkStack.Initialize();  // 変数の初期化
                result = RequestExecute(null, null);

                if (!fromScript)
                {
                    sw.Stop();
                    TimeSpan ts = sw.Elapsed;
                    Console.WriteLine($"execution time: {sw.ElapsedMilliseconds} (ms)");
                    OwnerCommandCanvas.CommandCanvasControl.MainLog.Flush();

                    OwnerCommandCanvas.EnabledScriptHoldActionMode = false; // 保留した表示更新処理を実行する
                }

                if (result != null)
                {
                    if (result.GetType() == typeof(CbClass<CbVoid>) || result.GetType() == typeof(CbClass<CbClass<CbVoid>>))
                    {
                        return null;
                    }
                }
            }
            finally
            {
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    // アイドル状態（画面の更新処理が終わってから）になってから戻す

                    OwnerCommandCanvas.CommandCanvasControl.CallAllExecuteEntryPointEnable(true);
                    GC.Collect();
                    CommandCanvasList.ResetOwnerCursor(Cursors.Wait);

                }), DispatcherPriority.ApplicationIdle);
            }
            return result;
        }

        ~RootConnector()
        {
            Dispose();
        }

        /// <summary>
        /// 初期化中を判定します。
        /// ※ロード及びペースト時に true になります。
        /// </summary>
        private bool IsInitializing = false;

        /// <summary>
        /// 実行ボタンの有効か無効かを制御します。
        /// </summary>
        /// <param name="enable">true = 有効</param>
        public void SetExecuteButtonEnable(bool enable)
        {
            ExecuteButtunControl.IsEnabled = enable;
        }

        /// <summary>
        /// ノードの返し値を参照します。
        /// </summary>
        public object RootValue { get => ValueData; }

        /// <summary>
        /// Forced チェックの状態を参照します。
        /// ※チェックするとノードの実行結果のキャッシュ値を返さずに常に実行した値を返すモードになります。
        /// </summary>
        public bool ForcedChecked
        {
            get => Forced.IsChecked.Value;
            set
            {
                Forced.IsChecked = value;
                UpdateMainPanelColor();
            }
        }

        /// <summary>
        /// アセットのヒント
        /// </summary>
        public string Hint
        {
            get => (string)FuncCaption.ToolTip;
            set
            {
                if (value is null)
                    return;

                if (value.Trim() != "")
                    FuncCaption.ToolTip = value;
            }
        }

        public bool LinkCheck()
        {
            bool isCollision = false;
            foreach (var node in ListData)
            {
                if (node is LinkConnector connector)
                {
                    // 接続しているノードから先をチェックする

                    isCollision = connector.LinkCheck();
                    if (isCollision)
                    {
                        break;
                    }
                }
            }
            return isCollision;
        }

        public object RequestExecute(List<object> functionStack, DummyArgumentsMemento dummyArguments)
        {
            functionStack ??= new List<object>();

            List<ICbValue> arguments = GetArguments(ref functionStack, dummyArguments);

            if (Function != null)
            {
                // ファンクションを実行する
                ICbValue ret = Function(arguments, dummyArguments);
                try
                {
                    if (ValueData.TypeName == CbSTUtils.VOID_STR)
                    {
                        // 何もしない
                    }
                    else if (ValueData.TypeName == ret.TypeName)
                    {
                        // メソッド処理結果をスクリプトノードに反映する
                        // ValueData = ret;
                        // ↑管理情報まで上書きするのでまるごと上書きしてはダメ

                        ValueData.Set(ret);
                    }
                    else
                    {
                        // メソッド処理結果の値とスクリプトノードの値の型が異なる

                        new NotImplementedException();
                    }
                }
                catch (Exception ex)
                {
                    MultiRootConnector.ExceptionFunc(ret, ex, this);
                }
                rootCurveLinks?.RequestUpdateRootValue();
                NameText.UpdateValueData();
            }
            else
            {
                if (ValueData is ICbClass cbClass)
                {
                    var ret = Activator.CreateInstance(ValueData.OriginalType);
                    ValueData.Data = ret;
                }
            }

            functionStack.Add(this);    // 実行済みであることを記録する
            arguments?.Clear();

            if (ValueData is null || (ValueData.IsNullable && ValueData.IsNull))
            {
                return null;
            }
            return ValueData.Data;
        }

        public BuildScriptInfo RequestBuildScript()
        {
            BuildScriptInfo result;
            if (IsGetVariableFunction())
            {
                // 変数参照はキャッシュしない

                result = BuildScriptInfo.CreateBuildScriptInfo(null);
            }
            else
            {
                if (BuildScriptInfo.HaveBuildScriptInfo(this))
                {
                    // すでに情報を取得済みなので、作成済みの内容を返します。

                    return BuildScriptInfo.CreateBuildScriptInfo(this);
                }
                result = BuildScriptInfo.CreateBuildScriptInfo(this);
            }

            if (Function != null || (FunctionInfo != null && FunctionInfo.IsConstructor))
            {
                // 引数情報
                BuildScriptInfo args = GetArgumentsBuildScript();
                // メソッドの返し値情報
                bool isList = ValueData.IsList;
                // メソッド呼び出しコードを取り出す
                string name = (string)FuncCaption.LabelString;
                if (FunctionInfo != null)
                {
                    if (FunctionInfo.ClassType == typeof(VoidSequence))
                    {
                        // 返し値無し

                        string methodName = "";
                        result.Set(methodName, BuildScriptInfo.CodeType.Sequece);
                        result.SetTypeName(CbSTUtils.GetTypeFullName(ValueData.OriginalType));
                        BuildScriptInfo.InsertSharedScripts(args);
                        result.Add(args);
                    }
                    else if (FunctionInfo.ClassType == typeof(ResultSequence))
                    {
                        // シーケンス

                        string methodName = "";
                        result.Set(methodName, BuildScriptInfo.CodeType.ResultSequece);
                        result.IsNotUseCache = ForcedChecked;
                        result.SetTypeName(CbSTUtils.GetTypeFullName(ValueData.OriginalType));
                        BuildScriptInfo.InsertSharedScripts(args);
                        result.Add(args);
                        if (!ForcedChecked && ValueData.TypeName != "void")
                        {
                            // キャッシュする
                            // 結果を変数に入れる処理に分けて、自身は変数を返します。
                            // 結果を変数に入れるノードは、BuildScriptInfo.InsertSharedScripts で出力されます。

                            string tempValiable = result.MakeSharedValiable(result);
                            result.Clear();
                            result.Set(tempValiable, BuildScriptInfo.CodeType.Variable);
                        }
                    }
                    else if (FunctionInfo.ClassType == typeof(DummyArguments))
                    {
                        // 仮引数

                        result.Set("", BuildScriptInfo.CodeType.DummyArgument);
                        result.SetTypeName(CbSTUtils.GetTypeFullName(ValueData.OriginalType));
                        result.Add(args);
                    }
                    else if (IsVariableFunction())
                    {
                        // 変数

                        string variableName = name.Substring(2, name.Length - 4);

                        bool isDelegate = ValueData is ICbEvent;
                        BuildScriptInfo.CodeType codeType;
                        if (isDelegate)
                        {
                            codeType = BuildScriptInfo.CodeType.DelegateVariable;
                        }
                        else
                        {
                            codeType = BuildScriptInfo.CodeType.Variable;
                        }

                        if (args.Child is null)
                        {
                            result.Set(variableName, codeType, variableName);
                        }
                        else
                        {
                            result.Set($"{variableName} = ", codeType, variableName);
                        }
                        result.SetTypeName(CbSTUtils.GetTypeFullName(FunctionInfo.ClassType));
                        result.Add(args);
                    }
                    else if (FunctionInfo.FuncCode == "__" + nameof(SwitchEnum))
                    {
                        // Switch文

                        result.Set(name, BuildScriptInfo.CodeType.VoidSwitch);
                        result.SetTypeName(CbSTUtils.GetTypeFullName(FunctionInfo.ClassType));
                        result.Add(args);
                    }
                    else if (FunctionInfo.ClassType == typeof(LiteralType) && ValueData.IsList)
                    {
                        // このメソッドでリストをUI的にまとめているだけなので、そのまま流す

                        if (!ForcedChecked)
                        {
                            // インスタンスを括弧で囲む

                            result.Set("", BuildScriptInfo.CodeType.Method);
                        }
                        result.Add(args);
                        if (!ForcedChecked && ValueData.TypeName != "void")
                        {
                            // キャッシュする
                            // 結果を変数に入れる処理に分けて、自身は変数を返します。
                            // 結果を変数に入れるノードは、BuildScriptInfo.InsertSharedScripts で出力されます。

                            string tempValiable = result.MakeSharedValiable(result);
                            result.Clear();
                            result.Add(BuildScriptInfo.CreateBuildScriptInfo(null, tempValiable, BuildScriptInfo.CodeType.Variable));
                        }
                    }
                    else
                    {
                        if (FunctionInfo.IsProperty)
                        {
                            if (FunctionInfo.FuncCode == "get_Item" || FunctionInfo.FuncCode == "set_Item")
                            {
                                // インデクサー

                                string funcCode = FunctionInfo.FuncCode.Substring(4);   // "get_" "set_" を取り除く
                                string className = CbSTUtils.GetTypeFullName(FunctionInfo.ClassType);
                                string methodName = className;
                                result.Set(methodName,
                                    FunctionInfo.FuncCode == "get_Item" ? BuildScriptInfo.CodeType.GetIndexer : BuildScriptInfo.CodeType.SetIndexer);
                                result.SetTypeName(CbSTUtils.GetTypeFullName(ValueData.OriginalType));
                                result.SetInstanceMethod(FunctionInfo.IsClassInstanceMethod);
                                result.Add(args);
                            }
                            else
                            {
                                // プロパティ

                                Debug.Assert(FunctionInfo.FuncCode.StartsWith("get_"));

                                string funcCode = FunctionInfo.FuncCode.Substring(4);   // "get_" を取り除く
                                string className = CbSTUtils.GetTypeFullName(FunctionInfo.ClassType);
                                string methodName = "";
                                if (!FunctionInfo.IsClassInstanceMethod)
                                {
                                    methodName = className + ".";
                                }
                                methodName += funcCode;
                                result.Set(methodName, BuildScriptInfo.CodeType.Property);
                                result.SetTypeName(CbSTUtils.GetTypeFullName(ValueData.OriginalType));
                                result.SetInstanceMethod(FunctionInfo.IsClassInstanceMethod);
                                result.Add(args);
                            }
                        }
                        else
                        {
                            // 通常のメソッド

                            string methodName = "";
                            if (FunctionInfo.IsConstructor)
                            {
                                // コンストラクタ

                                if (ValueData.OriginalType == typeof(CbNull))
                                {
                                    // CbNull は null 定数として扱う

                                    methodName = CbSTUtils.NULL_STR;
                                }
                                else
                                {
                                    methodName = CbSTUtils.NEW_STR + " " + CbSTUtils.GetTypeFullName(ValueData.OriginalType);
                                }
                            }
                            else
                            {
                                string className = CbSTUtils.GetTypeFullName(FunctionInfo.ClassType);
                                if (FunctionInfo.FuncCode == nameof(CommandCanvasList.CallEntryPoint) &&
                                    FunctionInfo.ClassType == typeof(CommandCanvasList))
                                {
                                    // CommandCanvasList.CallEntryPoint は、c#変換時に CapyCSSbase.Script.CallEntryPoint に差し替える

                                    className = typeof(CapyCSSbase.Script).FullName;
                                }
                                if (!FunctionInfo.IsClassInstanceMethod)
                                {
                                    methodName = className + ".";
                                }
                                methodName += FunctionInfo.FuncCode;
                                if (FunctionInfo.GenericMethodParameters != null)
                                {
                                    // ジェネリックパラメータを追加
                                    IEnumerable<string> methodGenericParameters =
                                        FunctionInfo.GenericMethodParameters.Select(n => CbSTUtils.GetTypeFullName(GetRequestType(FunctionInfo.typeRequests, n.Name)));
                                    if (methodGenericParameters.Count() > 0)
                                    {
                                        string temp = null;
                                        foreach (string param in methodGenericParameters)
                                        {
                                            if (temp is null)
                                                temp = "<" + param;
                                            else
                                                temp += ", " + param;
                                        }
                                        methodName += temp + ">";
                                    }
                                }
                            }

                            result.Set(methodName, BuildScriptInfo.CodeType.Method);
                            result.IsNotUseCache = ForcedChecked;
                            result.SetTypeName(CbSTUtils.GetTypeFullName(ValueData.OriginalType));
                            result.SetInstanceMethod(FunctionInfo.IsClassInstanceMethod);
                            result.Add(args);
                            if (!ForcedChecked && ValueData.TypeName != "void")
                            {
                                // キャッシュする
                                // 結果を変数に入れる処理に分けて、自身は変数を返します。
                                // 結果を変数に入れるノードは、BuildScriptInfo.InsertSharedScripts で出力されます。

                                string tempValiable = result.MakeSharedValiable(result);
                                result.Clear();
                                result.Add(BuildScriptInfo.CreateBuildScriptInfo(null, tempValiable, BuildScriptInfo.CodeType.Variable));
                            }
                        }
                    }
                }
            }
            else
            {
                if (ListData.Count == 0)
                {
                    // 引数無しのオブジェクト作成

                    if (ValueData.OriginalType == typeof(char))
                    {
                        // 文字

                        result.Add(BuildScriptInfo.CreateBuildScriptInfo(null, "'" + ValueData.ValueString + "'", BuildScriptInfo.CodeType.Data, ValueData.Name));
                    }
                    else if (ValueData.OriginalType == typeof(string))
                    {
                        // 文字列

                        string str = ValueData.ValueString;
                        str = str.Replace(Environment.NewLine, "\\n");
                        str = str.Replace("\n", "\\n");
                        str = str.Replace("\r", "\\r");
                        str = str.Replace("\t", "\\t");
                        result.Add(BuildScriptInfo.CreateBuildScriptInfo(null, "\"" + str + "\"", BuildScriptInfo.CodeType.Data, ValueData.Name));
                    }
                    else if (ValueData is ICbEnum cbEnum)
                    {
                        // 列挙型

                        string name = cbEnum.ItemName;
                        if (name == typeof(CbFuncArguments.INDEX).FullName.Replace("+", "."))
                        {
                            result.Add(BuildScriptInfo.CreateBuildScriptInfo(null, cbEnum.SelectedItemName, BuildScriptInfo.CodeType.Data, ValueData.Name));
                        }
                        else
                        {
                            result.Add(BuildScriptInfo.CreateBuildScriptInfo(null, name + "." + cbEnum.SelectedItemName, BuildScriptInfo.CodeType.Data, ValueData.Name));
                        }
                    }
                    else if (ValueData.OriginalType != typeof(object) &&
                            (ValueData.OriginalType.IsClass || CbStruct.IsStruct(ValueData.OriginalType) || ValueData.IsList))
                    {
                        // クラスもしくは構造体

                        if (ValueData.IsNull)
                        {
                            result.Add(BuildScriptInfo.CreateBuildScriptInfo(null, "null", BuildScriptInfo.CodeType.Data, ValueData.Name));
                        }
                        else
                        {
                            string name = CbSTUtils.GetTypeFullName(ValueData.OriginalType);
                            result.Add(BuildScriptInfo.CreateBuildScriptInfo(null, $"{CbSTUtils.NEW_STR} {name}()", BuildScriptInfo.CodeType.Method, ValueData.Name));

                            result.IsNotUseCache = ForcedChecked;
                            if (!ForcedChecked && ValueData.TypeName != "void")
                            {
                                // キャッシュする
                                // 結果を変数に入れる処理に分けて、自身は変数を返します。
                                // 結果を変数に入れるノードは、BuildScriptInfo.InsertSharedScripts で出力されます。

                                string tempValiable = result.MakeSharedValiable(result);
                                result.Clear();
                                result.Set(tempValiable, BuildScriptInfo.CodeType.Variable);
                            }
                        }
                    }
                    else
                    {
                        // その他

                        Debug.Assert(ValueData.ValueString != "");
                        if (ValueData.OriginalType == typeof(bool))
                        {
                            result.Add(BuildScriptInfo.CreateBuildScriptInfo(null, ValueData.ValueString.ToLower(), BuildScriptInfo.CodeType.Data, ValueData.Name));
                        }
                        else
                        {
                            result.Add(BuildScriptInfo.CreateBuildScriptInfo(null, ValueData.ValueString, BuildScriptInfo.CodeType.Data, ValueData.Name));
                        }
                    }
                    result.SetTypeName(CbSTUtils.GetTypeFullName(ValueData.OriginalType));
                }
                else
                {
                    // 引数有りのオブジェクト作成

                    // ここは元に戻す

                    result = GetArgumentsBuildScript();
                }
            }
            return result;
        }

        /// <summary>
        /// 変数参照メソッドノードを判定します。
        /// </summary>
        /// <returns>true==変数参照メソッドノード</returns>
        private bool IsVariableFunction()
        {
            return IsSetVariableFunction() || IsGetVariableFunction();
        }

        /// <summary>
        /// リスト変数書き込みメソッドを判定します。
        /// </summary>
        /// <returns>true==リスト変数書き込みメソッド</returns>
        private bool IsSetVariableListFunction()
        {
            bool isList = functionInfo != null && IsSetVariableFunction() && CbList.HaveInterface(functionInfo.ClassType, typeof(IEnumerable<>));
            return IsSetVariableFunction() && isList;
        }

        /// <summary>
        /// リスト変数書き込みメソッドがリストを作成するかを判定します。
        /// </summary>
        /// <returns>true==リスト変数書き込みメソッド</returns>
        private bool IsVariableListConstructor()
        {
            return IsSetVariableListFunction() && GetArgument(0).IsOpenNodeList;
        }

        private bool IsGetVariableFunction()
        {
            if (FunctionInfo is null || FunctionInfo.FuncCode is null)
            {
                return false;
            }
            return FunctionInfo.FuncCode == $"[{nameof(_GetVariable)}]";
        }

        /// <summary>
        /// 変数代入メソッドノードを判定します。
        /// </summary>
        /// <returns>true==変数代入メソッドノード</returns>
        private bool IsSetVariableFunction()
        {
            return FunctionInfo != null && FunctionInfo.FuncCode == $"[{nameof(SetVariable)}]";
        }

        /// <summary>
        /// 引数のスクリプト構築の為の要素を収集します。
        /// </summary>
        /// <returns>BuildScriptInfo?</returns>
        public BuildScriptInfo GetArgumentsBuildScript()
        {
            if (ListData is null)
            {
                return null;
            }
            BuildScriptInfo result = BuildScriptInfo.CreateBuildScriptInfo(null);
            for (int j = 0; j < ListData.Count; j++)
            {
                LinkConnector node = ListData[j];
                // イベント呼び出し

                if (node is LinkConnector connector)
                {
                    BuildScriptInfo argResult = connector.RequestBuildScript();
                    if (node.IsCallBackLink)
                    {
                        // イベント呼び出し

                        if (argResult.GetElementType() != BuildScriptInfo.CodeType.DelegateVariable
                            && connector.ValueData is ICbEvent cbEvent && !argResult.IsNullConstant())
                        {
                            string argStr = "";
                            // 引数情報を作成する
                            for (int i = 0; i < cbEvent.ArgumentsNum; i++)
                            {
                                if (i > 0)
                                    argStr += ", ";
                                argStr += $"ARG_{i + 1}";
                            }
                            var temp = BuildScriptInfo.CreateBuildScriptInfo(null);
                            if (argStr.Length > 0)
                            {
                                argStr = " " + argStr + " ";
                            }
                            temp.Set($"({argStr}) =>", 
                                (cbEvent.ReturnTypeName == "Void" ? BuildScriptInfo.CodeType.Delegate : BuildScriptInfo.CodeType.ResultDelegate),
                                node.ValueData.Name);
                            temp.Add(argResult);
                            temp.SetTypeName(CbSTUtils.GetTypeFullName(ValueData.OriginalReturnType));
                            argResult = temp;
                        }
                    }
                    if (argResult != null && !argResult.IsEmpty())
                    {
                        if (FunctionInfo != null && FunctionInfo.ArgumentTypeList != null)
                        {
                            argResult.SetArgumentAttr(FunctionInfo.ArgumentTypeList[j]);
                        }
                        result.Add(argResult);
                        result.SetTypeName(CbSTUtils.GetTypeFullName(node.ValueData.OriginalType));
                    }
                    else
                    {
                        Debug.Assert(false);
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// 引数に接続された情報を参照します。
        /// </summary>
        /// <param name="functionStack"></param>
        /// <param name="dummyArguments"></param>
        /// <returns></returns>
        private List<ICbValue> GetArguments(ref List<object> functionStack, DummyArgumentsMemento dummyArguments)
        {
            List<ICbValue> arguments = new List<ICbValue>();

            if (ForcedChecked)
            {
                // スタックを積み増す

                functionStack = new List<object>();
            }

            foreach (var node in ListData)
            {
                if (node is LinkConnector connector)
                {
                    // 接続しているファンクションノードの実行依頼

                    bool isVariableDelegate = false;
                    if (node.ValueData is ICbEvent cbEvent)
                    {
                        // デリゲート変数参照ノードからの接続かの情報を取得

                        isVariableDelegate = cbEvent.IsVariableDelegate;
                    }

                    if (node.IsCallBackLink && !isVariableDelegate)
                    {
                        // イベント呼び出しは、参照対象としない。
                        // 接続時にイベントとして処理している。
                        // ただし、デリゲート変数ノードに対しては、参照する必要がある。
                    }
                    else
                    {
                        connector.RequestExecute(functionStack, dummyArguments);
                    }
                    arguments.Add(connector.ValueData);
                }
            }

            return arguments;
        }

        /// <summary>
        /// 指定番目の引数を管理するリンクコネクターを取得する
        /// </summary>
        /// <param name="index">指定位置</param>
        /// <returns>引数を管理するリンクコネクター</returns>
        public LinkConnector GetArgument(int index)
        {
            if (ListData.Count <= index)
            {
                return null;
            }
            return ListData[index] as LinkConnector;
        }

        public void UpdateValueData()
        {
            NameText.UpdateValueData();
        }

        /// <summary>
        /// 接続のマルチリンクとシングルリンクを切り替えます。
        /// </summary>
        public bool SingleLinkMode
        {
            get => singleLinkMode;
            set
            {
                ChangeConnectorStyle(value, true);
                singleLinkMode = value;
            }
        }

        /// <summary>
        /// Not Use Cache チェックボックスの状態によるノード枠の色を参照します。
        /// </summary>
        private Brush RectboxStroke
        {
            get
            {
                return Forced.IsChecked == false ?
                    (Brush)Application.Current.FindResource("NormalNodeStrokeBrush")
                    :
                    (Brush)Application.Current.FindResource("NotUseCacheNormalNodeStrokeBrush");
            }
        }

        private Brush ConnectorStroke
        {
            get => EllipseType.Stroke;
            set
            {
                EllipseType.Stroke = value;
                RectangleType.Stroke = value;
            }
        }

        /// <summary>
        /// 接続のマルチリンクとシングルリンクの切り替えを行います。
        /// </summary>
        /// <param name="single">シングルなら true</param>
        /// <param name="disposeFlg"></param>
        private void ChangeConnectorStyle(bool single, bool disposeFlg = false)
        {
            if (single)
            {
                EllipseType.Visibility = Visibility.Collapsed;
                RectangleType.Visibility = Visibility.Visible;
            }
            else
            {
                EllipseType.Visibility = Visibility.Visible;
                RectangleType.Visibility = Visibility.Collapsed;
            }
            if (CurveCanvas is null)
                return; // キャンバスがnullの状態でRootCurveSingleLinkが作られるのは困る
            if (!disposeFlg)
            {
                if (single)
                {
                    rootCurveLinks ??= new RootCurveSingleLink(this, CurveCanvas);
                }
                else
                {
                    rootCurveLinks ??= new RootCurveMulitiLink(this, CurveCanvas);
                }
                return;
            }
            rootCurveLinks?.CloseLink();
            if (single)
            {
                rootCurveLinks = new RootCurveSingleLink(this, CurveCanvas);
            }
            else
            {
                rootCurveLinks = new RootCurveMulitiLink(this, CurveCanvas);
            }
        }

        /// <summary>
        /// 引数を追加します。
        /// </summary>
        /// <param name="variable">リンクする変数</param>
        /// <param name="literalType">リテラルタイプか？</param>
        public void AppendArgument(ICbValue variable, bool literalType = false)
        {
            // 引数とリンクしたリンクコネクターを作成する

            LinkConnector makeLinkConnector(int index)
            {
                return new LinkConnector(this, index.ToString())
                {
                    OwnerCommandCanvas = this.OwnerCommandCanvas,
                    ValueData = variable,
                    ChangeLinkedEvent = (isLinked) =>
                    {
                        if (IsVariableListConstructor())
                        {
                            // ノードの接続状態に依ってコンストラクタからそうでないかが決まるノード

                            if (isLinked)
                            {
                                // 引数に接続要求が来た

                                SetupNormalNodeDesign();
                            }
                            else
                            {
                                // 引数に接続解除要求が来た

                                SetupConstructorNodeDesign();
                            }
                        }
                    }
                };
            }

            if (variable.IsList)
            {
                // リスト型の引数を追加する

                AppendListArgument(makeLinkConnector(0), variable, literalType);
            }
            else
            {
                if (literalType)
                {
                    // 引数にしない（ルートのみ）

                    return;
                }

                // 引数UIを追加する
                AppendUIArgument(makeLinkConnector(1));
            }
        }

        /// <summary>
        /// リスト型引数の為のノードリストを用意します。
        /// </summary>
        /// <param name="linkConnector">コネクター</param>
        /// <param name="variable">リンクする変数</param>
        /// <param name="literalType">リテラルタイプか？</param>
        private void AppendListArgument(LinkConnector linkConnector, ICbValue variable, bool literalType = false)
        {
            // 要素を増やす場合の型の作成方法を登録
            linkConnector.SetListNodeType(variable.NodeTF);
            // リストを返し値と同期させる
            if (literalType)
            {
                // 更新時処理を登録する
                linkConnector.ConnectorList.UpdateListEvent =
                    () =>
                    {
                        // 変更したら自身（ルート）の表示を更新する
                        UpdateValueData();

                        // 変更をルートの接続先に伝える
                        rootCurveLinks?.RequestUpdateRootValue();
                    };

                // 引数の親に対してのコネクターへの接続を禁止する
                linkConnector.HideLinkConnector();
            }
            else
            {
                // 更新時処理を登録する
                linkConnector.ConnectorList.UpdateListEvent =
                    () =>
                    {
                        linkConnector.UpdateValueData();
                    };
            }

            // 変数をコネクターに登録する
            linkConnector.ConnectorList.LinkListTypeVariable(variable);

            // 引数UIを追加する
            AppendUIArgument(linkConnector);
        }

        /// <summary>
        /// 引数UIを追加します。
        /// </summary>
        /// <param name="linkConnector">コネクター</param>
        private void AppendUIArgument(LinkConnector linkConnector)
        {
            ListData.Add(linkConnector);
            CheckBoxVisibility();
        }

        private void CheckBoxVisibility()
        {
            BoxMainPanel.Visibility = ListData.Count == 0 ? Visibility.Collapsed : Visibility.Visible;
            Forced.Visibility = BoxMainPanel.Visibility;
        }

        public ICbValue ValueData
        {
            get => NameText.ValueData;
            set
            {
                if (value != null && value.IsDelegate && IsGetVariableFunction())
                {
                    // 変数参照デリゲートをマークする

                    (value as ICbEvent).IsVariableDelegate = true;
                }
                NameText.ValueData = value;
            }
        }

        public Point TargetPoint
        {
            get
            {
                if (SingleLinkMode)
                {
                    return
                        TransformToAncestor(CurveCanvas).Transform(
                            new Point(
                                MainPanel.ActualWidth - (EllipseType.ActualWidth / 2),
                                RootLinkMainPanel.ActualHeight / 2 + FuncCaption.ActualHeight
                            )
                        );
                }
                else
                {
                    return
                        TransformToAncestor(CurveCanvas).Transform(
                            new Point(
                                MainPanel.ActualWidth - (RectangleType.ActualWidth / 2),
                                RootLinkMainPanel.ActualHeight / 2 + FuncCaption.ActualHeight
                            )
                        );
                }
            } 
        }

        /// <summary>
        /// 常に呼ばれる
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _LayoutUpdated(object sender, EventArgs e)
        {
            if (LinkCount != 0 && IsLoaded)
            {
                try
                {
                    Point pos = TargetPoint;
                    if (backupPos != pos)
                    {
                        // コントロールの位置が移動していたらリンク線を再描画

                        RequestBuildCurve();
                        backupPos = pos;
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                }
            }
        }

        public Canvas CurveCanvas
        {
            get
            {
                if (OwnerCommandCanvas is null)
                    return null;

                return OwnerCommandCanvas.CurveCanvas;
            }
        }

        public bool RequestBuildCurve(ICurveLinkPoint target, Point? endPos)
        {
            if (rootCurveLinks is null)
                return false;
            var ret = rootCurveLinks.RequestBuildCurve(target, endPos);
            ChangeLinkConnectorStroke();
            return ret;
        }

        public bool RequestBuildCurve()
        {
            if (rootCurveLinks is null)
                return false;
            var ret = rootCurveLinks.RequestBuildCurve();
            ChangeLinkConnectorStroke();
            return ret;
        }

        public bool RequestLinkCurve(ICurveLinkPoint point)
        {
            if (rootCurveLinks is null)
                return false;
            var ret = rootCurveLinks.RequestLinkCurve(point);
            ChangeLinkConnectorStroke();
            OwnerCommandCanvas.RecordUnDoPoint(CapyCSS.Language.Instance["Help:SYSTEM_COMMAND_LinkNode"]);
            return ret;
        }

        public void RequestRemoveCurveLinkRoot(ICurveLinkPoint point)
        {
            rootCurveLinks?.RequestRemoveCurveLinkRoot(point);
            ChangeLinkConnectorStroke();
        }

        private void ChangeLinkConnectorStroke()
        {
            if (rootCurveLinks is null)
            {
                ConnectorStroke = (Brush)Application.Current.FindResource("LinkedConnectorStrokeBrush");
            }
            else
            {
                ConnectorStroke = rootCurveLinks.Count == 0 ?
                    (Brush)Application.Current.FindResource("LinkedConnectorStrokeBrush")
                    :
                    (Brush)Application.Current.FindResource("UnlinkedConnectorStrokeBrush");
            }
            UpdateMainPanelColor();
        }

        private void Forced_Click(object sender, RoutedEventArgs e)
        {
            UpdateMainPanelColor();
            if (e != null)
                e.Handled = true;
        }

        public void UpdateMainPanelColor()
        {
            if (rootCurveLinks != null && ValueData != null)
            {
                bool isLiteral = ValueData.IsLiteral;
                bool isNotConstructor = FunctionInfo is null || !FunctionInfo.IsConstructor;
                bool isNoReturn = ValueData is CbVoid;
                bool isRootOrNoReturnOrDummyArguments = rootCurveLinks.Count < 2 || isNoReturn || (functionInfo != null && functionInfo.ClassType == typeof(DummyArguments));
                if (isLiteral || (isNotConstructor && isRootOrNoReturnOrDummyArguments))
                {
                    // 仮引数ノードかルートか返し値がVoid、あるいは接続先が一つだけの場合はキャッシュしない

                    Forced.IsChecked = true;
                }
                else
                {
                    bool check = false;
                    if (FunctionInfo != null && FunctionInfo.IsConstructor)
                    {
                        // コンストラクター

                        check = true;
                        for (int i = 0; check && i < rootCurveLinks.Count; ++i)
                        {
                            var linkConnector = rootCurveLinks.LinkedInfo(i);
                            if (linkConnector is null)
                                continue;
                            if (linkConnector.IsSelf)
                            {
                                // リンク先にself引数があるならキャッシュする

                                check = false;
                            }
                        }
                    }
                    else
                    {
                        if (!check)
                        {
                            for (int i = 0; !check && i < rootCurveLinks.Count; ++i)
                            {
                                var linkConnector = rootCurveLinks.LinkedInfo(i);
                                if (linkConnector is null)
                                    continue;
                                if (linkConnector.ValueData.IsDelegate)
                                {
                                    // リンク先にデリゲートがあるならキャッシュはしない

                                    check = true;
                                }
                            }
                        }
                        for (int i = 0; check && i < rootCurveLinks.Count; ++i)
                        {
                            var linkConnector = rootCurveLinks.LinkedInfo(i);
                            if (linkConnector is null)
                                continue;
                            if (linkConnector.IsIn || linkConnector.IsOut || linkConnector.IsByRef)
                            {
                                // リンク先が in, out, ref 修飾された引数ならキャッシュする

                                check = false;
                            }
                        }
                    }
                    Forced.IsChecked = check;
                }
            }
            RectBox.Stroke = RectboxStroke;
            if (Forced.IsChecked == true)
            {
                Forced.Foreground = (Brush)Application.Current.FindResource("CheckedNotUseCacheBrush");
            }
            else
            {
                Forced.Foreground = (Brush)Application.Current.FindResource("NotUseCacheBrush");
            }
        }

        /// <summary>
        /// Entry指定時の処理です。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void IsPublicExecute_Checked(object sender, RoutedEventArgs e)
        {
            OwnerCommandCanvas.CommandCanvasControl.AddPublicExecuteEntryPoint(OwnerCommandCanvas, ExecuteRoot);
            IsPublicExecute.Foreground = (Brush)Application.Current.FindResource("CheckedEntryPointBrush");
            RectBox.Fill = NodeEntryColor;
            EntryPointName.Visibility = Visibility.Visible;

            // 名前の衝突チェック
            CommandCanvasList.CheckPickupAllEntryPoint(OwnerCommandCanvas);
            if (!IsInitializing)
            {
                OwnerCommandCanvas.RecordUnDoPoint(CapyCSS.Language.Instance["Help:SYSTEM_COMMAND_ChangeEntryPoint"]);
            }
        }

        /// <summary>
        /// Entry指定解除時の処理です。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void IsPublicExecute_Unchecked(object sender, RoutedEventArgs e)
        {
            OwnerCommandCanvas.CommandCanvasControl.RemovePublicExecuteEntryPoint(ExecuteRoot);
            IsPublicExecute.Foreground = (Brush)Application.Current.FindResource("EntryPointBrush");
            RectBox.Fill = (Brush)Application.Current.FindResource("NormalNodeBackgroundBrush");
            EntryPointName.Visibility = Visibility.Collapsed;

            // 名前の衝突チェック
            CommandCanvasList.CheckPickupAllEntryPoint(OwnerCommandCanvas);
            if (!IsInitializing)
            {
                OwnerCommandCanvas.RecordUnDoPoint(CapyCSS.Language.Instance["Help:SYSTEM_COMMAND_ChangeEntryPoint"]);
            }
        }

        private void EntryPointName_TextChanged(object sender, TextChangedEventArgs e)
        {
            // 名前の衝突チェック

            CommandCanvasList.CheckPickupAllEntryPoint(OwnerCommandCanvas);
            if (!IsInitializing)
            {
                OwnerCommandCanvas.RecordUnDoPoint(CapyCSS.Language.Instance["Help:SYSTEM_COMMAND_EditEntryPointName"]);
            }
        }

        /// <summary>
        /// エントリーポイント名の背景色の色替えを行います。
        /// </summary>
        /// <param name="flg">true==名前の衝突色</param>
        private void SetPickupEntryPoint(bool flg)
        {
            if (flg)
            {
                EntryPointName.Background = (Brush)Application.Current.FindResource("EntryPointNameCollisionBackgroundBrush");
            }
            else
            {
                EntryPointName.Background = (Brush)Application.Current.FindResource("EntryPointNameBackgroundBrush");
            }
        }

        //-----------------------------------------------------------------------------------
        #region リンク曲線操作

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // 曲線コントロールを作成
            if (CurveCanvas != null)
            {
                e.Handled = true;

                // 仮のリンク曲線を作成
                curvePath?.Dispose();
                curvePath = new CurvePath(CurveCanvas, this);

                MouseDown += (s, e) => {
                    if (e.ChangedButton == MouseButton.Right && e.ButtonState == MouseButtonState.Pressed)
                    {
                        // 接続操作をキャンセルする

                        curvePath?.Dispose();
                        curvePath = null;
                    }
                };
                MouseMove += Grid_MouseMove;
                MouseUp += Grid_MouseLeftButtonUp;
                CaptureMouse();
            }
        }

        private void Grid_MouseMove(object sender, MouseEventArgs e)
        {
            if (curvePath != null && curvePath.TargetEndPoint is null)
            {
                Point pos = e.GetPosition(CurveCanvas);
                
                // 仮のリンク曲線の終点をセット
                curvePath.EndPosition = pos;

                LinkConnector target = HitTestLinkConnector(e.GetPosition(CurveCanvas));
                if (target != null)
                {
                    // 接続できる場所の上にいる

                    target?.OnConnectionReservation();  // LinkCheck() 用
                    if (IsNgAssignment(target) || LinkCheck())
                    {
                        // 絶対に接続不可

                        curvePath.LineColor = (Brush)Application.Current.FindResource("CurvePathUnconnectableBrush");
                    }
                    else if (IsAssignment(target))
                    {
                        // 接続可能

                        curvePath.LineColor = (Brush)Application.Current.FindResource("CurvePathConnectableBrush");
                    }
                    else if (IsAssignmentCast(target))
                    {
                        // Castによる接続可能

                        curvePath.LineColor = (Brush)Application.Current.FindResource("CurvePathCastConnectableBrush");
                    }
                    else
                    {
                        // 接続不可

                        curvePath.LineColor = (Brush)Application.Current.FindResource("CurvePathUnconnectableBrush");
                    }
                    target?.OffConnectionReservation();  // LinkCheck() 用
                }
                else
                {
                    curvePath.LineColor = (Brush)Application.Current.FindResource("CurvePathDuringBrush");
                }

                // 仮のリンク曲線を更新
                curvePath.BuildCurve();
            }
        }

        /// <summary>
        /// キャスト接続判定
        /// </summary>
        /// <param name="linkConnector"></param>
        /// <returns></returns>
        private bool IsAssignmentCast(LinkConnector linkConnector)
        {
            return linkConnector.DefaultValueData.IsAssignment(ValueData, true);
        }

        /// <summary>
        /// 接続判定
        /// </summary>
        /// <param name="linkConnector"></param>
        /// <returns></returns>
        private bool IsAssignment(LinkConnector linkConnector)
        {
            return linkConnector.DefaultValueData.IsAssignment(ValueData);
        }

        /// <summary>
        /// 接続不可判定
        /// </summary>
        /// <param name="linkConnector"></param>
        /// <returns></returns>
        private bool IsNgAssignment(LinkConnector linkConnector)
        {
            if (linkConnector is null)
                return true;

            if (ValueData is null)
                return true;

            if (linkConnector.HideLinkPoint)
                return true;

            if (linkConnector.IsHideLinkConnector)
                return true;

            if (linkConnector.ValueData is null)
                return true;

            return false;
        }

        /// <summary>
        /// 型と一致する引数リンクコネクターを返します。
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public LinkConnector GetLinkConnector(Type type)
        {
            foreach (var node in ListData)
            {
                if (node.ValueData.OriginalType.IsAssignableFrom(type))
                {
                    return node;
                }
            }
            return null;
        }

        private void Grid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (curvePath != null)
            {
                LinkConnector target = HitTestLinkConnector(e.GetPosition(CurveCanvas));

                // 仮のリンク曲線を消す
                curvePath?.Dispose();
                curvePath = null;

                ICbValue rootValue = null;
                if (target is null)
                {
                    // 接続ターゲットが無い

                    var setPos = e.GetPosition(CurveCanvas);
                    rootValue = RootValue as ICbValue;
                    OwnerCommandCanvas.RootConnectorValueType = rootValue.OriginalType;
                    string targetName = rootValue.TypeName;

                    // コマンドウインドウを開く
                    Command.ShowCommandMenu.TryExecute(e.GetPosition(null), CbSTUtils.StripParamater(targetName));

                    // コマンドが登録されていたら実行する
                    OwnerCommandCanvas.ProcessCommand(setPos);
                }

                OwnerCommandCanvas.WorkCanvas.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (rootValue != null)
                    {
                        // WorkCanvas に最後に置かれた MultiRootConnector の引数の中から rootValue.OriginalType の型と一致する LinkConnector を取得する

                        target = OwnerCommandCanvas.GetLinkConnectorFromInstalledMultiRootConnector(rootValue.OriginalType);
                    }

                    target?.OnConnectionReservation();  // LinkCheck() 用
                    if (IsNgAssignment(target) || LinkCheck())
                    {
                        // 絶対に接続不可

                    }
                    else if (IsAssignment(target))
                    {
                        // 正式な接続を作成

                        var backup = target.CastType;
                        // 接続を前提に Cast モードを変更
                        target.CastType = false;
                        if (!RequestLinkCurve(target))
                        {
                            // 接続に失敗したので接続先の Cast モードを戻す

                            target.CastType = backup;
                        }
                    }
                    else if (IsAssignmentCast(target))
                    {
                        // キャストによる正式な接続を作成

                        var backup = target.CastType;
                        // 接続を前提に Cast モードを変更
                        target.CastType = true;
                        if (!RequestLinkCurve(target))
                        {
                            // 接続に失敗したので接続先の Cast モードを戻す

                            target.CastType = backup;
                        }
                    }
                    target?.OffConnectionReservation();  // LinkCheck() 用

                    MouseMove -= Grid_MouseMove;
                    MouseUp -= Grid_MouseLeftButtonUp;
                    ReleaseMouseCapture();

                }), DispatcherPriority.ApplicationIdle);
            }
        }

        private LinkConnector HitTestLinkConnector(Point point)
        {
            LinkConnector target = null;

            VisualTreeHelper.HitTest(CurveCanvas, null,
                new HitTestResultCallback(
                    new Func<HitTestResult, HitTestResultBehavior>(
                        (hit) =>
                        {
                            if (hit.VisualHit is Canvas)
                                return HitTestResultBehavior.Stop;

                            if (hit.VisualHit is FrameworkElement element)
                            {
                                FrameworkElement test = VisualTreeHelper.GetParent(element) as FrameworkElement;
                                if (test is null)
                                    return HitTestResultBehavior.Continue;

                                do
                                {
                                    if (test is LinkConnector)
                                    {
                                        break;
                                    }
                                    test = test.Parent as FrameworkElement;
                                } while (test != null);
                                if (test is null)
                                    return HitTestResultBehavior.Continue;

                                target = test as LinkConnector;
                                return HitTestResultBehavior.Stop;
                            }
                            return HitTestResultBehavior.Continue;
                        }
                    )
                ),
                new PointHitTestParameters(point));
            return target;
        }

        /// <summary>
        /// 接続線を強調表示します。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RootMainPanel_MouseEnter(object sender, MouseEventArgs e)
        {
            rootCurveLinks?.EnterEmphasis();
        }

        /// <summary>
        /// 接続線の強調表示を解除します。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RootMainPanel_MouseLeave(object sender, MouseEventArgs e)
        {
            rootCurveLinks?.LeaveEmphasis();
        }

        #endregion

        private void EllipseType_MouseEnter(object sender, MouseEventArgs e)
        {
            CommandCanvasList.SetOwnerCursor(Cursors.Hand);
        }

        private void EllipseType_MouseLeave(object sender, MouseEventArgs e)
        {
            CommandCanvasList.ResetOwnerCursor(Cursors.Hand);
        }

        //-----------------------------------------------------------------------------------
        #region IDisposable Support
        private bool disposedValue = false; // 重複する呼び出しを検出するには

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (IsRunable)
                    {
                        OwnerCommandCanvas.CommandCanvasControl.RemovePublicExecuteEntryPoint(ExecuteRoot);
                        OwnerCommandCanvas.CommandCanvasControl.RemoveAllExecuteEntryPointEnable(SetExecuteButtonEnable);
                    }
                    LayoutUpdated -= _LayoutUpdated;
                    MouseMove -= Grid_MouseMove;
                    MouseUp -= Grid_MouseLeftButtonUp;

                    CbSTUtils.ForeachDispose(ListData);
                    ListData = null;

                    rootCurveLinks?.CloseLink();
                    rootCurveLinks?.Dispose();
                    rootCurveLinks = null;

                    curvePath?.Dispose();
                    curvePath = null;

                    AssetXML?.Dispose();
                    AssetXML = null;
                    NodeEntryColor = null;
                    _OwnerCommandCanvas = null;

                    ValueData?.Dispose();
                    ValueData = null;
                    NameText.Dispose();
                    FuncCaption.Dispose();

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
