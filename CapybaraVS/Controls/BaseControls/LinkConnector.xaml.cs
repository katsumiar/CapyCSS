using CapyCSS.Script;
using CbVS.Script;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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

namespace CapyCSS.Controls.BaseControls
{
    /// <summary>
    /// LinkConnector.xaml の相互作用ロジック
    /// </summary>
    public partial class LinkConnector 
        : UserControl
        , ICurveLinkPoint
        , IDisposable
        , IHaveCommandCanvas
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
        [XmlRoot(nameof(LinkConnector))]
        public class _AssetXML<OwnerClass> : IDisposable
            where OwnerClass : LinkConnector
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
                    self.TargetPointId = PointId;

                    self.ChangeConnectorStyle(self.SingleLinkMode);

                    self.CastType = CastConnect;

                    if (Value != null)
                    {
                        self.defaultValueData = self.ValueData;
                        if (self.ValueData.IsStringableValue)
                        {
                            self.ValueData.ValueString = Value;
                        }
                    }

                    self.ParamTextBox.AssetXML = ParamInfo;
                    self.ParamTextBox.AssetXML.ReadAction?.Invoke(self.ParamTextBox);
                    self.ParamTextBox.UpdateValueData();

                    self.ConnectorList.AssetXML = LinkConnectorList;
                    self.ConnectorList.AssetXML.ReadAction?.Invoke(self.ConnectorList);

                    // 次回の為の初期化
                    self.AssetXML = new _AssetXML<LinkConnector>(self);
                };
            }
            public _AssetXML(OwnerClass self)
            {
                WriteAction = () =>
                {
                    PointId = self.TargetPointId;

                    CastConnect = self.CastType;

                    if (!(self.ValueData is ICbValueList))
                    {
                        if (self.backupValueData is null)
                        {
                            Value = self.ValueData.ValueUIString;
                        }
                        else
                        {
                            Value = self.backupValueData.ValueUIString;
                        }
                    }

                    self.ParamTextBox.AssetXML.WriteAction?.Invoke();
                    ParamInfo = self.ParamTextBox.AssetXML;

                    self.ConnectorList.AssetXML.WriteAction?.Invoke();
                    LinkConnectorList = self.ConnectorList.AssetXML;
                };
            }
            [XmlAttribute(nameof(PointId))]
            public int PointId { get; set; } = 0;
            #region 固有定義
            public string Value { get; set; } = null;
            public bool CastConnect { get; set; } = false;
            public UIParam._AssetXML<UIParam> ParamInfo { get; set; } = null;
            public LinkConnectorList._AssetXML<LinkConnectorList> LinkConnectorList { get; set; } = null;

            protected virtual void Dispose(bool disposing)
            {
                if (!disposedValue)
                {
                    if (disposing)
                    {
                        WriteAction = null;
                        ReadAction = null;

                        // 以下、固有定義開放
                        Value = null;
                        ParamInfo?.Dispose();
                        ParamInfo = null;
                        LinkConnectorList?.Dispose();
                        LinkConnectorList = null;
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
        public _AssetXML<LinkConnector> AssetXML { get; set; } = null;
        #endregion

        LinkCurveLinks linkCurveLinks = null;
        private Point backupPos = new Point(0, 0);

        private bool singleLinkMode = true;

        #region UpdateEvent 添付プロパティ実装

        private static ImplementDependencyProperty<LinkConnector, Action> impUpdateEvent =
            new ImplementDependencyProperty<LinkConnector, Action>(
                nameof(UpdateEvent),
                (self, getValue) =>
                {
                    //Action value = getValue(self);
                });

        public static readonly DependencyProperty UpdateListEventProperty = impUpdateEvent.Regist(null);

        public Action UpdateEvent
        {
            get { return impUpdateEvent.GetValue(this); }
            set { impUpdateEvent.SetValue(this, value); }
        }

        #endregion

        #region ReadOnly 添付プロパティ実装

        private static ImplementDependencyProperty<LinkConnector, bool> impReadOnly =
            new ImplementDependencyProperty<LinkConnector, bool>(
                nameof(ReadOnly),
                (self, getValue) =>
                {
                    bool value = getValue(self);
                    self.ParamTextBox.ReadOnly = value;
                });

        public static readonly DependencyProperty ReadOnlyProperty = impReadOnly.Regist(false);

        public bool ReadOnly
        {
            get { return impReadOnly.GetValue(this); }
            set { impReadOnly.SetValue(this, value); }
        }

        #endregion

        #region CastType 添付プロパティ実装

        private static ImplementDependencyProperty<LinkConnector, bool> impCastType =
            new ImplementDependencyProperty<LinkConnector, bool>(
                nameof(CastType),
                (self, getValue) =>
                {
                    bool value = getValue(self);
                    self.ParamTextBox.CastType = value;
                });

        public static readonly DependencyProperty impCastTypeProperty = impCastType.Regist(false);

        public bool CastType
        {
            get { return impCastType.GetValue(this); }
            set { impCastType.SetValue(this, value); }
        }

        #endregion

        #region OwnerLinkConnector 添付プロパティ実装
        private static ImplementDependencyProperty<LinkConnector, LinkConnector> impOwnerLinkConnector =
            new ImplementDependencyProperty<LinkConnector, LinkConnector>(
                nameof(OwnerLinkConnector),
                (self, getValue) =>
                {
                    //LinkConnector value = getValue(self);
                });

        public static readonly DependencyProperty OwnerLinkConnectorProperty = impOwnerLinkConnector.Regist(null);

        public LinkConnector OwnerLinkConnector
        {
            get { return impOwnerLinkConnector.GetValue(this); }
            set { impOwnerLinkConnector.SetValue(this, value); }
        }
        #endregion

        private CommandCanvas _OwnerCommandCanvas = null;

        public CommandCanvas OwnerCommandCanvas
        {
            get => _OwnerCommandCanvas;
            set
            {
                Debug.Assert(value != null);
                if (ConnectorList.OwnerCommandCanvas is null)
                    ConnectorList.OwnerCommandCanvas = value;
                if (ParamTextBox.OwnerCommandCanvas is null)
                    ParamTextBox.OwnerCommandCanvas = value;
                if (_OwnerCommandCanvas is null)
                {
                    _OwnerCommandCanvas = value;
                    ChangeConnectorStyle(SingleLinkMode);
                }
            }
        }

        public LinkConnector()
        {
            Debug.Assert(false);
        }

        private string debugCreateName = "";
        public LinkConnector(object self, string _ext = "", [CallerMemberName] string callerMethodName = "")
        {
            CommandCanvas.SetDebugCreateList(ref debugCreateName, this, self, callerMethodName, _ext);
            InitializeComponent();
            ConnectorList.OwnerLinkConnector = this;
            pointIdProvider = new PointIdProvider(this);
            AssetXML = new _AssetXML<LinkConnector>(this);
            DataContext = this;

            ChangeLinkConnectorStroke();
            ParamTextBox.UpdateEvent =
                new Action(
                () => 
                {
                    UpdateEvent?.Invoke();
                }
                );

            LayoutUpdated += _LayoutUpdated;
        }

        ~LinkConnector()
        {
            Dispose();
        }

        /// <summary>
        /// デリートアイコン
        /// </summary>
        public DeleteIcon DeleteEventIcon => Delete;

        /// <summary>
        /// 追加ボタン時イベントとしてノードを追加するイベントを登録する
        /// </summary>
        /// <param name="nodeType"></param>
        public void SetListNodeType(Func<ICbValue> nodeType)
        {
            BoxMainPanel.Visibility = Visibility.Visible;
            ConnectorList.CreateNodeEvent = new Func<ObservableCollection<LinkConnector>, ICbValue>(
                (ListData) => 
                {
                    return nodeType();
                }
                );
        }

        /// <summary>
        /// スクリプトの実行を依頼します。
        /// </summary>
        /// <param name="functionStack">スタック情報</param>
        /// <param name="preArgument"></param>
        public object RequestExecute(List<object> functionStack = null, DummyArgumentsStack preArgument = null)
        {
            object result = null;
            ConnectorList.RequestExecute(functionStack, preArgument);
            if (linkCurveLinks != null)
            {
                result = linkCurveLinks.RequestExecute(functionStack, preArgument);
            }
            return result;
        }

        public BuildScriptInfo? RequestBuildScript()
        {
            BuildScriptInfo? result = null;
            var scr = new BuildScriptInfo();
            var connectorResult = ConnectorList.RequestBuildScript();
            if (connectorResult.HasValue)
            {
                // 引数情報（要素情報リストが有効な場合は、こちらの情報は無い）

                result = connectorResult;
                if (result.HasValue)
                {
                    // そのまま流す
                }
            }
            if (linkCurveLinks != null)
            {
                // 引数がリストの場合の要素情報リスト（引数情報が有効な場合は、こちらの情報は無い）

                var linksResult = linkCurveLinks.RequestBuildScript();
                if (linksResult.HasValue)
                {
                    result = linksResult;
                }
            }
            if (!result.HasValue)
            {
                if (ValueData.Data is char)
                {
                    // 文字

                    scr.Add(new BuildScriptInfo("'" + ValueData.ValueString + "'", BuildScriptInfo.CodeType.Data, ValueData.Name));
                }
                else if (ValueData.Data is string)
                {
                    // 文字列

                    scr.Add(new BuildScriptInfo("\"" + ValueData.ValueString + "\"", BuildScriptInfo.CodeType.Data, ValueData.Name));
                }
                else if (!(ValueData.OriginalReturnType is object || ValueData.OriginalReturnType.IsEnum) &&
                        ValueData.OriginalReturnType.IsClass || CbStruct.IsStruct(ValueData.OriginalReturnType))
                {
                    // クラス

                    scr.Add(new BuildScriptInfo("new " + ValueData.OriginalReturnType.FullName + "()", BuildScriptInfo.CodeType.Data, ValueData.Name));
                }
                else if (ValueData is ICbEnum cbEnum)
                {
                    // 列挙型

                    scr.Add(new BuildScriptInfo(ValueData.ValueString, BuildScriptInfo.CodeType.Data, ValueData.Name));
                }
                else
                {
                    // その他

                    if (ValueData.ValueString == "")
                    {
                        if (ValueData.IsList)
                        {
                            scr.Add(new BuildScriptInfo("null", BuildScriptInfo.CodeType.Data, ValueData.Name));
                        }
                        else
                        {
                            Debug.Assert(false);
                        }
                    }
                    else
                    {
                        scr.Add(new BuildScriptInfo(ValueData.ValueString, BuildScriptInfo.CodeType.Data, ValueData.Name));
                    }
                }
                scr.SetTypeName(ValueData.TypeName);
                result = scr;
            }
            return result;
        }

        private bool hideLinkPoint = false;
        public bool HideLinkPoint
        {
            get => hideLinkPoint;
            set
            {
                hideLinkPoint = value;
                if (hideLinkPoint)
                {
                    EllipseType.Visibility = Visibility.Collapsed;
                    RectangleType.Visibility = Visibility.Collapsed;
                }
                else
                {
                    ChangeConnectorStyle(SingleLinkMode);
                }
            }
        }

        public bool IsHideLinkConnector => MainPanel.Visibility == Visibility.Collapsed;

        public void HideLinkConnector()
        {
            MainPanel.Visibility = Visibility.Collapsed;
            BoxMainPanel.Margin = new Thickness();
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

        public bool SingleLinkMode
        {
            get => singleLinkMode;
            set
            {
                if (singleLinkMode != value)
                    ChangeConnectorStyle(value);
                singleLinkMode = value;
            }
        }

        private void ChangeConnectorStyle(bool single)
        {
            if (!HideLinkPoint)
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
            }
            if (singleLinkMode == single)
            {
                if (single)
                {
                    linkCurveLinks ??= new LinkCurveSingleLinks(this);
                }
                else
                {
                    linkCurveLinks ??= new LinkCurveMultiLinks(this);
                }
                if (HideLinkPoint)
                    linkCurveLinks?.CloseLink();
                return;
            }
            linkCurveLinks?.CloseLink();
            if (HideLinkPoint)
                return;
            if (single)
            {
                linkCurveLinks = new LinkCurveSingleLinks(this);
            }
            else
            {
                linkCurveLinks = new LinkCurveMultiLinks(this);
            }
        }

        private ICbValue defaultValueData = null;
        public ICbValue DefaultValueData => defaultValueData;

        private ICbValue backupValueData = null;

        /// <summary>
        /// 接続しているノードの値情報です。
        /// ※ ただし、object型の場合はセットされない
        /// </summary>
        private ICbValue connectValueData = null;
        public ICbValue ValueData
        {
            get => ParamTextBox.ValueData;
            set
            {
                if (defaultValueData is null)
                {
                    defaultValueData = value;

                    Debug.Assert(defaultValueData != null);
                    if (defaultValueData != null)
                    {
                        ParamTextBox.TypeNameLabelOverlap = defaultValueData.TypeName;
                        ParamTextBox.ParamNameLabelOverlap = defaultValueData.Name;
                    }
                }

                ParamTextBox.ValueData = value;
            }
        }

        public Point TargetPoint
        {
            get
            {
                if (SingleLinkMode)
                    return
                        TransformToAncestor(CurveCanvas).Transform(
                            new Point(
                                RectangleType.Margin.Left + RectangleType.ActualWidth / 2,
                                MainPanel.ActualHeight / 2
                            )
                        );
                else
                    return
                        TransformToAncestor(CurveCanvas).Transform(
                            new Point(
                                EllipseType.ActualWidth / 2,
                                MainPanel.ActualHeight / 2
                            )
                        );
            }
        }

        /// <summary>
        /// 常に呼ばれる
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _LayoutUpdated(object sender, EventArgs e)
        {
            if (IsLoaded)
            {
                ForcedLayoutUpdated(IsOpenList);
            }
        }

        private bool IsOpenList = true;

        /// <summary>
        /// 接続線のレイアウトを正しく更新します。
        /// </summary>
        public void ForcedLayoutUpdated(bool isOpenList)
        {
            IsOpenList = isOpenList;
            if (linkCurveLinks != null && linkCurveLinks.Count != 0 && IsLoaded)
            {
                try
                {
                    Point pos;
                    if (OwnerLinkConnector != null && !IsOpenList)
                    {
                        pos = OwnerLinkConnector.TargetPoint;
                    }
                    else
                    {
                        pos = TargetPoint;
                    }
                    if (backupPos != pos)
                    {
                        linkCurveLinks?.RequestBuildCurve(this, pos);
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

        private bool ClearLock = false;

        /// <summary>
        /// Func<> 引数に接続したルートコネクターを参照します。
        /// </summary>
        private RootConnector eventLinkRootConnector = null;

        public bool RequestLinkCurve(ICurveLinkRoot root)
        {
            if (linkCurveLinks is null)
                return false;

            ClearLock = true;   // 接続の繋ぎ変えをするときに情報を消されないようにする
            var ret = linkCurveLinks.RequestLinkCurve(root);
            ClearLock = false;

            if (root.RootValue != null && root.RootValue is ICbValue value)
            {
                // リンク先の ValueData を保持している ValueData に接続する

                if (backupValueData is null)
                {
                    // もともとの接続情報を保存して新規接続する

                    backupValueData = CbST.CbCreate(ValueData.OriginalType);
                    backupValueData.Set(ValueData);
                    ReadOnly = true;
                }

                if (BoxMainPanel.Visibility == Visibility.Visible &&
                    value.IsList)
                {
                    // 接続可能かつリスト型の場合

                    ConnectorList.Connect(value);
                }

                ValueData = CbST.CbCreate(backupValueData.OriginalType);
                connectValueData = value;
                eventLinkRootConnector = root as RootConnector;

                SetupValue();
                ValueData.Name = ParamTextBox.ParamNameLabelOverlap;    // ノードを接続すると名前が消えるので上書きする
                ParamTextBox.UpdateValueData();

                UpdateEvent?.Invoke();

                SetIsCallBackLink();
            }

            ChangeLinkConnectorStroke();
            return ret;
        }

        /// <summary>
        /// 表示を更新します。
        /// </summary>
        public void UpdateValueData()
        {
            ParamTextBox.UpdateValueData();
        }

        /// <summary>
        /// 引数としてコールバックを受け取っているか？
        /// </summary>
        public bool IsCallBackLink = false;

        /// <summary>
        /// 接続がコールバック呼び出し接続かの判定を残しておきます。
        /// </summary>
        private void SetIsCallBackLink()
        {
            IsCallBackLink = ValueData is ICbEvent;
        }

        /// <summary>
        /// 受け取った値を正しく受け入れます。
        /// </summary>
        private void SetupValue()
        {
            if (connectValueData is null)
                return;

            if (ValueData.IsError)
            {
                // 一旦リセットする

                ValueData.IsError = false;
                ValueData.ErrorMessage = "";
            }

            try
            {
                bool isEvent = false;

                if (ValueData is ICbEvent cbEvent)
                {
                    isEvent = true;

                    // Func<> 型の接続なので直接コールするための仕組みを差し込む

                    var callbackReturnValue = cbEvent.NodeTF() as ICbEvent;

                    callbackReturnValue.Callback = (cagt2) =>
                    {
                        // 接続されているルートコネクターを直接実行する

                        eventLinkRootConnector.RequestExecute(null, cagt2);

                        if (eventLinkRootConnector.ValueData is ICbEvent cbEventValue)
                        {
                            // Func<> 型を Func<> 型の引数で受け取ったので実行して返し値を返す

                            cbEventValue.InvokeCallback(cagt2);

                            return cbEventValue.Value;
                        }

                        return eventLinkRootConnector.ValueData;
                    };

                    connectValueData = callbackReturnValue;
                }

                if (!isEvent && CastType)
                {
                    // 値をキャストする

                    SetupCastValue();
                }
                else
                {
                    ValueData.Set(connectValueData);
                }
            }
            catch (Exception ex)
            {
                ValueData.IsError = true;
                ValueData.ErrorMessage = MultiRootConnector.ExceptionFunc(ex, "Argument(error)");
            }
        }

        /// <summary>
        /// 接続された値をキャストする。
        /// </summary>
        private void SetupCastValue()
        {
            try
            {
                if (connectValueData.IsList && connectValueData.IsList)
                {
                    // ToArray() によるキャスト

                    ValueData.Set(connectValueData);
                }
                else if (CbScript.IsCast(ValueData.OriginalType))
                {
                    ValueData.Set(connectValueData);
                }
                else if (connectValueData is CbObject cbObject)
                {
                    //if (cbObject.Data is null)
                    //    return; // 保険

                    ValueData.Set(cbObject);
                }
                else
                {
                    bool isFromDf = connectValueData is CbDouble || connectValueData is CbFloat;
                    bool isToDF = ValueData is CbDouble || ValueData is CbFloat;
                    if (isFromDf && !isToDF)
                    {
                        double tmp = (dynamic)connectValueData.Data;
                        long tmp2 = (long)tmp;
                        ValueData.ValueString = tmp2.ToString();
                    }
                    else
                    {
                        ValueData.ValueString = Convert.ChangeType(connectValueData.Data, backupValueData.OriginalReturnType).ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                ValueData.IsError = true;
                ValueData.ErrorMessage = MultiRootConnector.ExceptionFunc(ex, "Cast(error)");
            }
        }

        /// <summary>
        /// 接続先から呼ばれる再表示処理です。
        /// </summary>
        public void UpdateRootValue()
        {
            SetupValue();
            ParamTextBox.UpdateValueData();
            UpdateConnectedList();
            UpdateEvent?.Invoke();
        }

        /// <summary>
        /// 管理している値が ICollection<> なら指定要素数だけ増やす
        /// </summary>
        /// <param name="count">増やす要素の数</param>
        public void TryAddListNode(int count)
        {
            if (BoxMainPanel.Visibility == Visibility.Visible &&
                ValueData.IsList)
            {
                // リンクされているリストを表示に再反映する

                while (count-- > 0)
                {
                    ConnectorList.AddOption_MouseDown(null, null);
                }
            }
        }

        /// <summary>
        /// 接続されているリストを表示に再反映します。
        /// </summary>
        private void UpdateConnectedList()
        {
            if (BoxMainPanel.Visibility == Visibility.Visible &&
                ValueData.IsList)
            {
                // 接続されているリストを表示に再反映する

                ConnectorList.UpdateListData(ValueData);
            }
        }

        public void RequestRemoveCurveLinkPoint(ICurveLinkRoot root)
        {
            linkCurveLinks?.RequestRemoveCurveLinkPoint(root);
            if (backupValueData != null)
            {
                ValueData.Set(backupValueData);
                UpdateEvent?.Invoke();
                backupValueData = null;
                ConnectorList?.Disconnect();
            }
            if (!ClearLock)
                RemoveQurveUpdate();
            ChangeLinkConnectorStroke();
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if ((Keyboard.GetKeyStates(Key.LeftCtrl) & KeyStates.Down) > 0 ||
                (Keyboard.GetKeyStates(Key.RightCtrl) & KeyStates.Down) > 0)
            {
                RequestRemoveQurve();
                e.Handled = true;
            }
        }

        public void RequestRemoveQurve()
        {
            linkCurveLinks?.CloseLink();
            if (backupValueData != null)
            {
                ValueData = backupValueData;
                UpdateEvent?.Invoke();
                backupValueData = null;
                ConnectorList.Disconnect();
            }
            RemoveQurveUpdate();
            ChangeLinkConnectorStroke();
        }

        private void RemoveQurveUpdate()
        {
            connectValueData = null;
            ReadOnly = false;
            CastType = false;
            ParamTextBox.UpdateValueData();
        }

        private void ChangeLinkConnectorStroke()
        {
            if (linkCurveLinks is null)
                ConnectorStroke = Brushes.Blue;
            else
                ConnectorStroke =
                    linkCurveLinks.Count == 0 ? Brushes.Blue : Brushes.CornflowerBlue;
        }

        private void MainPanel_MouseEnter(object sender, MouseEventArgs e)
        {
            if (Delete.ClickEvent != null)
                Delete.Visibility = Visibility.Visible;
        }

        private void MainPanel_MouseLeave(object sender, MouseEventArgs e)
        {
            if (Delete.ClickEvent != null)
                Delete.Visibility = Visibility.Collapsed;
        }

#region IDisposable Support
        private bool disposedValue = false; // 重複する呼び出しを検出するには

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    LayoutUpdated -= _LayoutUpdated;
                    ConnectorList.Dispose();
                    linkCurveLinks?.Dispose();
                    linkCurveLinks = null;

                    AssetXML?.Dispose();
                    AssetXML = null;
                    _OwnerCommandCanvas = null;
                    defaultValueData?.Dispose();
                    defaultValueData = null;
                    backupValueData?.Dispose();
                    backupValueData = null;
                    connectValueData = null;
                    eventLinkRootConnector = null;

                    ParamTextBox.ValueData?.Dispose();
                    ParamTextBox.Dispose();

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
