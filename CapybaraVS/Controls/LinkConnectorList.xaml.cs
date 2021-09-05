//#define SHOW_LINK_ARRAY   // リスト型を接続したときにリストの要素をコピーして表示する

using CapybaraVS.Controls.BaseControls;
using CapybaraVS.Script;
using CbVS.Script;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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

namespace CapybaraVS.Controls
{
    /// <summary>
    /// LinkConnectorList.xaml の相互作用ロジック
    /// </summary>
    public partial class LinkConnectorList 
        : UserControl
        , IDisposable
        , ICbExecutable
    {
        #region XML定義
        [XmlRoot(nameof(LinkConnectorList))]
        public class _AssetXML<OwnerClass> : IDisposable
            where OwnerClass : LinkConnectorList
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
                    foreach (var node in List)
                    {
                        var connector = self.AddListNode(self.CreateNodeEvent);

                        connector.AssetXML = node;
                        connector.AssetXML.ReadAction?.Invoke(connector);
                    }
                    self.UpdateListEvent?.Invoke();
                    self.Dispatcher.BeginInvoke(
                        new Action(() =>
                        {
                            if (IsOpenList)
                            {
                                self.OpenAccordion();
                            }
                            else
                            {
                                self.CloseAccordion();
                            }
                        }
                        ), DispatcherPriority.Loaded);

                    // 次回の為の初期化
                    self.AssetXML = new _AssetXML<LinkConnectorList>(self);
                };
            }
            public _AssetXML(OwnerClass self)
            {
                WriteAction = () =>
                {
                    IsOpenList = self.IsOpenList;
                    List = new List<LinkConnector._AssetXML<LinkConnector>>();
                    ObservableCollection<LinkConnector> target = self.noneConnectedListData;
                    if (target != null)
                    {
                        foreach (var node in target)
                        {
                            node.AssetXML.WriteAction?.Invoke();
                            List.Add(node.AssetXML);
                        }
                    }
                };
            }
            #region 固有定義
            [XmlArrayItem("LinkConnector")]
            public List<LinkConnector._AssetXML<LinkConnector>> List { get; set; } = null;
            public bool IsOpenList { get; set; } = true;

            protected virtual void Dispose(bool disposing)
            {
                if (!disposedValue)
                {
                    if (disposing)
                    {
                        WriteAction = null;
                        ReadAction = null;

                        // 以下、固有定義開放
                        List?.GetEnumerator().Dispose();
                        List = null;
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
        public _AssetXML<LinkConnectorList> AssetXML { get; set; } = null;
        #endregion

        public ObservableCollection<LinkConnector> ListData { get; set; } = null;
        /// <summary>
        /// 接続用のリスト
        /// </summary>
        private ObservableCollection<LinkConnector> connectedListData = new ObservableCollection<LinkConnector>();
        /// <summary>
        /// 非接続用のリスト
        /// </summary>
        private ObservableCollection<LinkConnector> noneConnectedListData = new ObservableCollection<LinkConnector>();

        #region EnableAdd 添付プロパティ実装

        private bool forcedNotAdd = false;

        private static ImplementDependencyProperty<LinkConnectorList, bool> impEnableAdd =
            new ImplementDependencyProperty<LinkConnectorList, bool>(
                nameof(EnableAdd),
                (self, getValue) =>
                {
                    bool value = getValue(self);
                    self.AddOption.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
                });

        public static readonly DependencyProperty EnableAddOptionProperty = impEnableAdd.Regist(true);

        /// <summary>
        /// ユーザーからのノード追加は有効か？
        /// </summary>
        public bool EnableAdd
        {
            get { return impEnableAdd.GetValue(this); }
            set { impEnableAdd.SetValue(this, value); }
        }

        #endregion

        #region UpdateListEvent 添付プロパティ実装

        private static ImplementDependencyProperty<LinkConnectorList, Action> impUpdateListEvent =
            new ImplementDependencyProperty<LinkConnectorList, Action>(
                nameof(UpdateListEvent),
                (self, getValue) =>
                {
                    //Action value = getValue(self);
                });

        public static readonly DependencyProperty UpdateListEventProperty = impUpdateListEvent.Regist(null);

        public Action UpdateListEvent
        {
            get { return impUpdateListEvent.GetValue(this); }
            set { impUpdateListEvent.SetValue(this, value); }
        }
        #endregion

        #region OwnerLinkConnector 添付プロパティ実装
        private static ImplementDependencyProperty<LinkConnectorList, LinkConnector> impOwnerLinkConnector =
            new ImplementDependencyProperty<LinkConnectorList, LinkConnector>(
                nameof(OwnerLinkConnector),
                (self, getValue) =>
                {
                    //Action value = getValue(self);
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
                SetOunerCanvas(connectedListData, value);
                SetOunerCanvas(noneConnectedListData, value);
                if (_OwnerCommandCanvas is null)
                    _OwnerCommandCanvas = value;
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

        public LinkConnectorList()
        {
            InitializeComponent();
            ListData = noneConnectedListData;
            AssetXML = new _AssetXML<LinkConnectorList>(this);
            ListView.ItemsSource = ListData;
        }

        /// <summary>
        /// 接続されたときの処理を行います（外部との接続）。
        /// </summary>
        /// <param name="list">接続された変数</param>
        public void Connect(ICbValue variable)
        {
            if (variable is null)
                return;

            foreach (var node in ListData)
            {
                // 既存のノードへの接続を切る

                node.RequestRemoveQurve();
            }
            if (ListData == connectedListData)
            {
                // 接続用のリストをクリアする

                ClearList();
            }
            else
            {
                // 接続用リストに切り替える

                ListData = connectedListData;
            }

            BuildConnectorList(variable.GetListValue.Value);

            // 接続されたらリスト表示を無くす
            DisenableList();
            EnableAdd = false;  // ノード追加機能を無効化
        }

        /// <summary>
        /// リスト型変数をコネクターリストに展開します。
        /// </summary>
        /// <param name="variableList"></param>
        private void BuildConnectorList(List<ICbValue> variableList)
        {
            foreach (var variable in variableList)
            {
                AppendList(variable);
            }
        }

        /// <summary>
        /// コネクタノードを追加します（内部リンク用）。
        /// </summary>
        /// <param name="variable">リンクする変数</param>
        private LinkConnector AppendList(ICbValue variable)
        {
            if (ListData != noneConnectedListData)
            {
                return null;
            }

            var linkConnector = new LinkConnector()
            {
                OwnerCommandCanvas = this.OwnerCommandCanvas,
                ValueData = variable,
                OwnerLinkConnector = OwnerLinkConnector,
                HideLinkPoint = false    // 接続できるようにする
            };
            //linkConnector.ReadOnly = true;

            AddNodeEvent(linkConnector);

            // 変更通知用イベント
            linkConnector.ConnectorList.UpdateListEvent =
                () => {
                    UpdateListEvent?.Invoke();
                };

            linkConnector.UpdateRootValue();
            ListData.Add(linkConnector);

            return linkConnector;
        }

        /// <summary>
        /// 接続されているリスト型の変数です（内部との接続）。
        /// </summary>
        private List<ICbValue> linkedListTypeVariable = null;

        /// <summary>
        /// リスト型の変数をコネクタにリンクします（内部との接続）。
        /// </summary>
        /// <param name="list">リンクする変数</param>
        public void LinkListTypeVariable(ICbValue variable)
        {
            if (variable is null)
                return;

            var listTypeVariable = variable.GetListValue;

            if (!(variable as ICbList).HaveAdd)
            {
                // 追加できないリスト

                forcedNotAdd = true;
                EnableAdd = false;  // ノード追加機能を無効化
            }

            Disconnect();

            // 渡された変数のリストをリンクする
            linkedListTypeVariable = listTypeVariable.Value;
            ListData.Clear();
            for (var i = 0; i < listTypeVariable.Count; ++i)
            {
                // リストの要素を作成したコネクターに登録し、バインディングリストに登録する
                AppendList(listTypeVariable[i]);
            }
            OpenAccordion();
            UpdateListEvent?.Invoke();
        }

        /// <summary>
        /// リスト型の変数の表示を更新します。
        /// </summary>
        /// <param name="variable"></param>
        public void UpdateListData(ICbValue variable)
        {
            var listTypeVariable = variable.GetListValue;

            if (OwnerCommandCanvas.LinkConnectorListHoldAction.Enabled)
            {
                // 画面反映はあとから一括で行う

                OwnerCommandCanvas.LinkConnectorListHoldAction.Add(this, () =>
                        {
                            UpdateListData(listTypeVariable);
                        }
                    );
                return;
            }

            if (ListData.Count > 0 && listTypeVariable.Count > 0 && ListData[0].ValueData.OriginalType == listTypeVariable[0].OriginalType)
            {
                // 差分のコピー

                int len = Math.Min(ListData.Count, listTypeVariable.Count);
                int i = 0;
                for (; i < len; ++i)
                {
                    ListData[i].ValueData = listTypeVariable[i];
                    ListData[i].UpdateValueData();
                }
                int remaining = ListData.Count - listTypeVariable.Count;
                if (remaining != 0)
                {
                    if (remaining > 0)
                    {
                        // 多すぎる配列を消す

                        while (remaining-- > 0)
                        {
                            ListData.RemoveAt(i);
                        }
                    }
                    else
                    {
                        // 足りない配列を足す

                        remaining = Math.Abs(remaining);
                        for (int j = 0; j < remaining; ++j)
                        {
                            AppendList(listTypeVariable[i + j]);
                        }
                    }
                }
            }
            else
            {
                ClearList();
                BuildConnectorList(listTypeVariable.Value);
            }

            SetConnectorBackground();
        }

        /// <summary>
        /// 外部との接続を切ります。
        /// </summary>
        public void Disconnect()
        {
            if (ListData is null)
                return;
            foreach (var node in ListData)
            {
                // 既存のノードへの接続を切る

                node.RequestRemoveQurve();
            }
            if (ListData == connectedListData)
            {
                // 接続用だったならリストをクリアする

                ClearList();

                // 非接続用リストに切り替える
                ListData = noneConnectedListData;
            }
            ListView.ItemsSource = ListData;
            EnableList();

            if (!forcedNotAdd)
            {
                EnableAdd = true;  // ノード追加機能を有効化
            }
        }

        /// <summary>
        /// ノードをCloseLinkしてリストをクリアします（外部接続用）。
        /// </summary>
        private void ClearList()
        {
            Debug.Assert(ListData == connectedListData);

            foreach (var node in ListData)
                if (node is ICloseLink target)
                    target.CloseLink();
            ListData.Clear();
        }

        /// <summary>
        /// 実行処理を要求します。
        /// </summary>
        /// <param name="functionStack"></param>
        /// <param name="preArgument"></param>
        public void RequestExecute(List<object> functionStack = null, DummyArgumentsStack preArgument = null)
        {
            foreach (var node in ListData)
            {
                if (node.IsCallBackLink)
                {
                    // イベント呼び出しは、参照対象としない。
                    // 接続時にイベントとして処理している。
                }
                else
                {
                    node.RequestExecute(functionStack, preArgument);
                }
            }
        }

        /// <summary>
        /// ノード作成用イベントです。
        /// </summary>
        public Func<ObservableCollection<LinkConnector>, ICbValue> CreateNodeEvent =
            new Func<ObservableCollection<LinkConnector>, ICbValue>((ListData) => new ParamNameOnly("argument" + (ListData.Count + 1)));

        /// <summary>
        /// 追加ボタンによるリストノード挿入です。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void AddOption_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (CreateNodeEvent is null)
                return;

            AddListNode();
        }

        /// <summary>
        /// リストノードを増やします（内部接続用）。
        /// </summary>
        private void AddListNode()
        {
            AddListNode(CreateNodeEvent);
            UpdateListEvent?.Invoke();
        }

        /// <summary>
        /// ノード作成用イベントを使ってリストノードを追加します（内部接続用）。
        /// </summary>
        /// <param name="createNode">ノード作成用イベント</param>
        /// <returns></returns>
        private LinkConnector AddListNode(Func<ObservableCollection<LinkConnector>, ICbValue> createNode)
        {
            ICbValue addVariable = createNode(noneConnectedListData);

            var linkConnector = AppendList(addVariable);
            if (linkConnector is null)
            {
                return null;
            }
            linkedListTypeVariable.Add(addVariable);

            //AddNodeEvent(linkConnector);

            SetConnectorBackground();

            // リストの変更を所有者に伝える
            UpdateListEvent?.Invoke();
            return linkConnector;
        }

        /// <summary>
        /// ノードにイベントを登録します。
        /// </summary>
        /// <param name="linkConnector">コネクター</param>
        private void AddNodeEvent(LinkConnector linkConnector)
        {
            ICbValue addVariable = linkConnector.ValueData;

            // リスト更新イベントを登録
            linkConnector.UpdateEvent = new Action(
                    () =>
                    {
                        int index = linkedListTypeVariable.IndexOf(addVariable);
                        if (index != -1)
                        {
                            try
                            {
                                linkedListTypeVariable[index].Set(linkConnector.ValueData);

                                // リストの変更を所有者に伝える
                                UpdateListEvent?.Invoke();
                            }
                            catch (Exception ex)
                            {
                                // スクリプト実行時エラーなのでログに出力する
                                System.Diagnostics.Debug.WriteLine(ex.Message);
                            }
                        }
                    });

            // ノード削除イベントを登録
            linkConnector.DeleteEventIcon.ClickEvent = new Action(
                    () =>
                    {
                        // 削除イベント

                        linkedListTypeVariable.Remove(addVariable);
                        linkConnector.Dispose();
                        noneConnectedListData.Remove(linkConnector);
                        SetConnectorBackground();

                        // リストの変更を所有者に伝える
                        UpdateListEvent?.Invoke();
                    }
                    );
        }

        /// <summary>
        /// ノードリストは開いているか？
        /// </summary>
        public bool IsOpenList => ListPanel.Visibility == Visibility.Visible;

        /// <summary>
        /// ノードリストを開きます。
        /// </summary>
        private void OpenList()
        {
            ListPanel.Visibility = Visibility.Visible;
            SetConnectorBackground();

            foreach (var node in ListData)
            {
                node.ForcedLayoutUpdated(true);
            }
        }

        private void SetConnectorBackground()
        {
            ConnectorBackground.Visibility = ListData.Count != 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        /// <summary>
        /// ノードリストを閉じます。
        /// </summary>
        private void CloseList()
        {
            ListPanel.Visibility = Visibility.Collapsed;

            foreach (var node in ListData)
            {
                node.ForcedLayoutUpdated(false);
            }
        }

        /// <summary>
        /// アコーディオンモードで開きます。
        /// </summary>
        private void OpenAccordion()
        {
            AccordionOpenIcon.Visibility = Visibility.Collapsed;
            AccordionCloseIcon.Visibility = Visibility.Visible;
            OpenList();
        }

        /// <summary>
        /// アコーディオンモードで閉じます。
        /// </summary>
        private void CloseAccordion()
        {
            AccordionOpenIcon.Visibility = Visibility.Visible;
            AccordionCloseIcon.Visibility = Visibility.Collapsed;
            CloseList();
        }

        /// <summary>
        /// アコーディオンモードを有効にします。
        /// </summary>
        private void EnableAccordion()
        {
            if (IsOpenList)
            {
                OpenAccordion();
            }
            else
            {
                CloseAccordion();
            }
        }

        /// <summary>
        /// アコーディオンモードを無効にします。
        /// </summary>
        private void DisenableAccordion()
        {
            AccordionOpenIcon.Visibility = Visibility.Collapsed;
            AccordionCloseIcon.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// ノードリストを無効にします。
        /// </summary>
        private void EnableList()
        {
            EnableAccordion();
        }

        /// <summary>
        /// ノードリストを無効にします。
        /// </summary>
        private void DisenableList()
        {
            DisenableAccordion();
            ListPanel.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// アコーディオンの開閉を行います。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Accordion_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (IsOpenList)
            {
                CloseAccordion();
            }
            else
            {
                OpenAccordion();
            }
        }

        private void ListView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                while (ListView.SelectedItems.Count != 0)
                {
                    if (ListView.SelectedItems[0] is LinkConnector linkConnector)
                        linkConnector.DeleteEventIcon?.ClickEvent?.Invoke();
                }
            }
            else if (e.Key == Key.Insert)
            {
                AddListNode(CreateNodeEvent);
                UpdateListEvent?.Invoke();
            }
        }

        private void AccordionOpenIcon_MouseEnter(object sender, MouseEventArgs e)
        {
            Cursor = Cursors.Hand;
        }

        private void AccordionOpenIcon_MouseLeave(object sender, MouseEventArgs e)
        {
            Cursor = null;
        }

#region IDisposable Support
        private bool disposedValue = false; // 重複する呼び出しを検出するには

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    foreach (var node in connectedListData)
                    {
                        node.Dispose();
                    }
                    foreach (var node in noneConnectedListData)
                    {
                        node.Dispose();
                    }
                    connectedListData?.Clear();
                    connectedListData = null;
                    noneConnectedListData?.Clear();
                    noneConnectedListData = null;
                    AssetXML?.Dispose();
                    AssetXML = null;
                    ListData.Clear();
                    ListData = null;
                    _OwnerCommandCanvas = null;
                    linkedListTypeVariable?.Clear();
                    linkedListTypeVariable = null;
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
