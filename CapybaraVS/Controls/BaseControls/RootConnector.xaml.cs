using CapybaraVS.Script;
using CapyCSS.Controls;
using CbVS.Script;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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
        public class _AssetXML<OwnerClass>
            where OwnerClass : RootConnector
        {
            private static int queueCounter = 0;
            [XmlIgnore]
            public Action WriteAction = null;
            [XmlIgnore]
            public Action<OwnerClass> ReadAction = null;
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
                    NameText.OwnerCommandCanvas = value;
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
                    node.OwnerCommandCanvas = value;
            }
        }

        public RootConnector()
        {
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

                    ExecuteRoot();
                }
                );
            
            LayoutUpdated += _LayoutUpdated;
        }

        public void ExecuteRoot()
        {
            if (IsLockExecute)
                return; // 再入を禁止する

            CommandCanvasList.OwnerWindow.Cursor = Cursors.Wait;
            IsLockExecute = true;

            OwnerCommandCanvas.ScriptWorkCanvas.Dispatcher.BeginInvoke(new Action(() =>
            {
                OwnerCommandCanvas.CommandCanvasControl.MainLog.TryAutoClear();
                GC.Collect();

                // スクリプトを実行する

                OwnerCommandCanvas.CommandCanvasControl.CallAllExecuteEntryPointEnable(false);
                OwnerCommandCanvas.EnabledScriptHoldActionMode = true;  // 表示更新処理を保留する

                var sw = new System.Diagnostics.Stopwatch();
                sw.Start();

                RequestExecute(null, null);

                sw.Stop();
                TimeSpan ts = sw.Elapsed;
                OwnerCommandCanvas.CommandCanvasControl.MainLog.OutLine(
                    "system",
                    $"Execute Time: {sw.ElapsedMilliseconds} (ms)");

                OwnerCommandCanvas.EnabledScriptHoldActionMode = false; // 保留した表示更新処理を実行する

                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    // アイドル状態になってから戻す

                    IsLockExecute = false;
                    OwnerCommandCanvas.CommandCanvasControl.CallAllExecuteEntryPointEnable(true);
                    GC.Collect();
                    CommandCanvasList.OwnerWindow.Cursor = null;

                }), DispatcherPriority.ApplicationIdle);

            }), DispatcherPriority.ApplicationIdle);
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
        /// スクリプト実行ボタン再入禁止フラグ
        /// </summary>
        private bool IsLockExecute
        {
            get => OwnerCommandCanvas.CommandCanvasControl.IsLockExecute;
            set
            {
                OwnerCommandCanvas.CommandCanvasControl.IsLockExecute = value;
            }
        }

        /// <summary>
        /// ノードの返し値を参照します。
        /// </summary>
        public object RootValue { get => ValueData; }

        /// <summary>
        /// Forced チェックの状態を参照します。
        /// ※チェックするとノードの実行結果のキャッシュ値を返さずに常に実行した値を返すモードになります。
        /// </summary>
        public bool ForcedChecked { get => Forced.IsChecked.Value; set { Forced.IsChecked = value; }}

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

        public void RequestExecute(List<object> functionStack, DummyArgumentsStack preArgument)
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
                        // メソッド処理結果をアセットに反映する
                        // ValueData = ret;
                        // ↑管理情報まで上書きするのでまるごと上書きしてはダメ

                        ValueData.Set(ret);
                    }
                    else
                    {
                        // メソッド処理結果の値とアセットの値の型が異なる

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
            rootCurveLinks?.Dispose();
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
            var linkConnector = new LinkConnector()
            {
                OwnerCommandCanvas = this.OwnerCommandCanvas,
                ValueData = variable
            };
            if (variable.IsList)
            {
                // リスト型の引数を追加する

                AppendListArgument(linkConnector, variable, literalType);
            }
            else
            {
                if (literalType)
                {
                    // 引数にしない（ルートのみ）

                    return;
                }

                // 引数UIを追加する
                AppendUIArgument(linkConnector);
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
            RectBox.Stroke = RectboxStroke;
            if (Forced.IsChecked == true)
            {
                Forced.Foreground = Brushes.Tomato;
            }
            else
            {
                Forced.Foreground = Brushes.Black;
            }
            if (e != null)
                e.Handled = true;
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

        private void Grid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (curvePath != null)
            {
                LinkConnector target = HitTestLinkConnector(e.GetPosition(CurveCanvas));

                // 仮のリンク曲線を消す
                curvePath?.Dispose();
                curvePath = null;

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
            Cursor = Cursors.Hand;
        }

        private void EllipseType_MouseLeave(object sender, MouseEventArgs e)
        {
            Cursor = null;
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
                    foreach (var node in ListData)
                    {
                        node.Dispose();
                    }
                    rootCurveLinks?.Dispose();
                    rootCurveLinks = null;
                }
                disposedValue = true;
            }
        }
        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
