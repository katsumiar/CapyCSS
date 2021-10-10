using CapybaraVS.Script;
using CapyCSS.Controls;
using CbVS.Script;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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

namespace CapybaraVS.Controls.BaseControls
{
    /// <summary>
    /// RootConnector.xaml の相互作用ロジック
    /// </summary>
    public partial class RootConnector 
        : UserControl
        , ICurveLinkRoot
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
            private static int queueCounter = 0;
            [XmlIgnore]
            public Action WriteAction = null;
            [XmlIgnore]
            public Action<OwnerClass> ReadAction = null;
            private bool disposedValue;

            public _AssetXML()
            {
                ReadAction = (self) =>
                {
                    self.TargetPointId = PointId;

                    self.FuncCaption.AssetXML = Caption;
                    self.FuncCaption.AssetXML.ReadAction?.Invoke(self.FuncCaption);

                    if (Value != null && self.ValueData != null && Value != CbSTUtils.ERROR_STR)
                    {
                        if (self.ValueData.IsStringableValue)
                            self.ValueData.ValueString = Value;
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

                    // レイアウトが変更されるのでレイアウトの変更を待って続きを処理する必要がある
                    self.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        // ↓この中で CurveCanvas が取得されるがデザインがある程度整うまで取得できない
                        self.ChangeConnectorStyle(self.SingleLinkMode);

                        if (self.CurveCanvas is null)
                            return; // 最後までここで終わると接続が張られない可能性がある……

                        self.rootCurveLinks.AssetXML = Connector;
                        self.rootCurveLinks.AssetXML.ReadAction?.Invoke(self.rootCurveLinks);

                        queueCounter++;
                        self.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            // このタイミングでリンクを貼ることに不安を拭えない……（貼り漏れが発生するかも）
                            if (--queueCounter == 0)
                            {
                                PointIdProvider.CheckRequestStart();
                            }
                        }), DispatcherPriority.ApplicationIdle);

                    }), DispatcherPriority.ApplicationIdle);

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

                    if (!(self.ValueData is ICbValueList))
                        Value = self.ValueData.ValueUIString;

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

        private static ImplementDependencyProperty<RootConnector, Func<List<ICbValue>, DummyArgumentsStack, ICbValue>> impFunction =
            new ImplementDependencyProperty<RootConnector, Func<List<ICbValue>, DummyArgumentsStack, ICbValue>>(
                nameof(Function),
                (self, getValue) =>
                {
                    //Func<List<ICbVSValue>, CbPushList, ICbVSValue> value = getValue(self);
                });

        public static readonly DependencyProperty FunctionProperty = impFunction.Regist(null);

        public Func<List<ICbValue>, DummyArgumentsStack, ICbValue> Function
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

        public bool IsRunable
        {
            get { return impIsRunable.GetValue(this); }
            set { impIsRunable.SetValue(this, value); }
        }

        #endregion

        /// <summary>
        /// 通常のノード背景色です。
        /// </summary>
        private Brush NodeNormalColor = null;

        /// <summary>
        /// Entry指定時のノード背景色です。
        /// </summary>
        private Brush NodeEntryColor = (Brush)(new System.Windows.Media.BrushConverter()).ConvertFromString("#99BCBCFF");

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

            NodeNormalColor = RectBox.Fill;
            ChangeLinkConnectorStroke();
            RectBox.Stroke = RectboxStroke;
            CheckBoxVisibility();

            Forced.ToolTip = CapybaraVS.Language.Instance["SYSTEM_ArgumentForced"];
            IsPublicExecute.ToolTip = CapybaraVS.Language.Instance["SYSTEM_IsPublicExecute"];

            NameText.UpdateEvent =
                new Action(
                () =>
                {
                    // 情報の変更を接続先に伝える

                    rootCurveLinks?.RequestUpdateRootValue();
                }
                );

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
            object result = null;

            if (!fromScript)
            {
                if (entryPointName != null && entryPointName.StartsWith(":"))
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
                    return null;    // false
                }

                if (!fromScript && CommandCanvasList.GetOwnerCursor() == Cursors.Wait)
                    return null;

                CommandCanvasList.SetOwnerCursor(Cursors.Wait);
                OwnerCommandCanvas.CommandCanvasControl.CallAllExecuteEntryPointEnable(false);
                OwnerCommandCanvas.CommandCanvasControl.MainLog.TryAutoClear();
                GC.Collect();
            }

            if (IsPublicExecute.IsChecked.Value &&
                !(entryPointName is null && GetEntryPointName().Length == 0) &&
                (entryPointName is null || entryPointName != GetEntryPointName()))
            {
                // エントリーポイントに名前が付けられていて、且つ名前が一致しない

                CommandCanvasList.SetOwnerCursor(null);
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

            result = RequestExecute(null, null);

            if (!fromScript)
            {
                sw.Stop();
                TimeSpan ts = sw.Elapsed;
                Console.WriteLine($"Execute Time: {sw.ElapsedMilliseconds} (ms)");
                OwnerCommandCanvas.CommandCanvasControl.MainLog.Flush();

                OwnerCommandCanvas.EnabledScriptHoldActionMode = false; // 保留した表示更新処理を実行する


                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                // アイドル状態（画面の更新処理が終わってから）になってから戻す

                OwnerCommandCanvas.CommandCanvasControl.CallAllExecuteEntryPointEnable(true);
                    GC.Collect();
                    CommandCanvasList.SetOwnerCursor(null);

                }), DispatcherPriority.ApplicationIdle);
            }

            if (result != null)
            {
                if (result.GetType() == typeof(CbClass<CbVoid>) || result.GetType() == typeof(CbClass<CbClass<CbVoid>>))
                {
                    return null;
                }
            }
            return result;
        }

        ~RootConnector()
        {
            Dispose();
        }

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

        public object RequestExecute(List<object> functionStack, DummyArgumentsStack preArgument)
        {
            functionStack ??= new List<object>();
            preArgument ??= new DummyArgumentsStack();

            List<ICbValue> arguments = null;

            arguments = GetArguments(ref functionStack, preArgument);

            bool enableExecute = !preArgument.IsInvalid();   // 実行環境が有効か？

            if (Function != null && enableExecute)
            {
                // ファンクションを実行する
                ICbValue ret = Function(arguments, preArgument);
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

            functionStack.Add(this);    // 実行済みであることを記録する
            arguments?.Clear();

            if (ValueData is null || (ValueData.IsNullable && ValueData.IsNull))
            {
                return null;
            }
            return ValueData.Data;
        }

        /// <summary>
        /// 引数に接続された情報を参照します。
        /// </summary>
        /// <param name="functionStack"></param>
        /// <param name="preArgument"></param>
        /// <returns></returns>
        private List<ICbValue> GetArguments(ref List<object> functionStack, DummyArgumentsStack preArgument)
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
                    // 接続しているファンクションアセットの実行依頼

                    if (node.IsCallBackLink)
                    {
                        // イベント呼び出しは、参照対象としない。
                        // 接続時にイベントとして処理している。
                    }
                    else
                    {
                        connector.RequestExecute(functionStack, preArgument);
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
        /// Forced チェックボックスの状態による枠の色を参照します。
        /// </summary>
        private Brush RectboxStroke
        {
            get
            {
                return Forced.IsChecked == false ?
                    new SolidColorBrush(Color.FromArgb(102, 0x90, 0x90, 0x90))//Brushes.Silver
                    :
                    new SolidColorBrush(Color.FromArgb(102, 0x00, 0x00, 0xfa));//Brushes.LightSkyBlue;
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
                    ValueData = variable
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
            if (rootCurveLinks != null && rootCurveLinks.Count != 0 && IsLoaded)
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
                ConnectorStroke = Brushes.Blue;
            else
                ConnectorStroke = rootCurveLinks.Count == 0 ? Brushes.Blue : Brushes.CornflowerBlue;
        }

        private void Forced_Click(object sender, RoutedEventArgs e)
        {
            UpdateMainPanelColor();
            if (e != null)
                e.Handled = true;
        }

        private void UpdateMainPanelColor()
        {
            RectBox.Stroke = RectboxStroke;
            if (Forced.IsChecked == true)
            {
                Forced.Foreground = Brushes.Tomato;
            }
            else
            {
                Forced.Foreground = Brushes.Black;
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
            IsPublicExecute.Foreground = Brushes.Tomato;
            RectBox.Fill = NodeEntryColor;
            EntryPointName.Visibility = Visibility.Visible;

            // 名前の衝突チェック
            CommandCanvasList.CheckPickupAllEntryPoint(OwnerCommandCanvas);
        }

        /// <summary>
        /// Entry指定解除時の処理です。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void IsPublicExecute_Unchecked(object sender, RoutedEventArgs e)
        {
            OwnerCommandCanvas.CommandCanvasControl.RemovePublicExecuteEntryPoint(ExecuteRoot);
            IsPublicExecute.Foreground = Brushes.Black;
            RectBox.Fill = NodeNormalColor;
            EntryPointName.Visibility = Visibility.Collapsed;

            // 名前の衝突チェック
            CommandCanvasList.CheckPickupAllEntryPoint(OwnerCommandCanvas);
        }

        private void EntryPointName_TextChanged(object sender, TextChangedEventArgs e)
        {
            // 名前の衝突チェック

            CommandCanvasList.CheckPickupAllEntryPoint(OwnerCommandCanvas);
        }

        /// <summary>
        /// エントリーポイント名の背景色の色替えを行います。
        /// </summary>
        /// <param name="flg">true==名前の衝突色</param>
        private void SetPickupEntryPoint(bool flg)
        {
            if (flg)
            {
                EntryPointName.Background = Brushes.Tomato;
            }
            else
            {
                EntryPointName.Background = Brushes.NavajoWhite;
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

                    if (IsNgAssignment(target))
                    {
                        // 絶対に接続不可

                        curvePath.LineColor = Brushes.HotPink;
                    }
                    else if (IsAssignment(target))
                    {
                        // 接続可能

                        curvePath.LineColor = Brushes.Lime;
                    }
                    else if (IsAssignmentCast(target))
                    {
                        // Castによる接続可能

                        curvePath.LineColor = Brushes.Tomato;
                    }
                    else
                    {
                        // 接続不可

                        curvePath.LineColor = Brushes.HotPink;
                    }
                }
                else
                {
                    curvePath.LineColor = Brushes.DeepSkyBlue;
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
                    OwnerCommandCanvas.ShowCommandMenu(e.GetPosition(null), CbSTUtils.StripParamater(targetName));
                    
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

                    if (IsNgAssignment(target))
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
            CommandCanvasList.SetOwnerCursor(null);
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
                    NodeNormalColor = null;
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
