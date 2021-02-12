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
        public class _AssetXML<OwnerClass>
            where OwnerClass : LinkConnectorList
        {
            [XmlIgnore]
            public Action WriteAction = null;
            [XmlIgnore]
            public Action<OwnerClass> ReadAction = null;
            public _AssetXML()
            {
                ReadAction = (self) =>
                {
                    self.backupListDataForLinkList = self.ListData;
                    foreach (var node in List)
                    {
                        var connector = self.AddListNode(self.AddFunc);

                        connector.AssetXML = node;
                        connector.AssetXML.ReadAction?.Invoke(connector);
                    }

                    // 次回の為の初期化
                    self.AssetXML = new _AssetXML<LinkConnectorList>(self);
                };
            }
            public _AssetXML(OwnerClass self)
            {
                WriteAction = () =>
                {
                    List = new List<LinkConnector._AssetXML<LinkConnector>>();
                    ObservableCollection<LinkConnector> target = self.backupListDataForLinkList;
                    if (target is null)
                        target = self.ListData;
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
            #endregion
        }
        public _AssetXML<LinkConnectorList> AssetXML { get; set; } = null;
        #endregion

        public ObservableCollection<LinkConnector> ListData { get; set; } = new ObservableCollection<LinkConnector>();
        private ObservableCollection<LinkConnector> backupListDataForLinkList = null;
        private ObservableCollection<LinkConnector> backupListDataForSetList = null;

        #region EnableAddOption 添付プロパティ実装

        private static ImplementDependencyProperty<LinkConnectorList, bool> impEnableAdd =
            new ImplementDependencyProperty<LinkConnectorList, bool>(
                nameof(EnableAdd),
                (self, getValue) =>
                {
                    bool value = getValue(self);
                    self.AddOption.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
                });

        public static readonly DependencyProperty EnableAddOptionProperty = impEnableAdd.Regist(true);

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

        private CommandCanvas _OwnerCommandCanvas = null;

        public CommandCanvas OwnerCommandCanvas
        {
            get => _OwnerCommandCanvas;
            set
            {
                Debug.Assert(value != null);
                SetOunerCanvas(ListData, value);
                SetOunerCanvas(backupListDataForLinkList, value);
                SetOunerCanvas(backupListDataForSetList, value);
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
            AssetXML = new _AssetXML<LinkConnectorList>(this);

            ListView.ItemsSource = ListData;
        }

        /// <summary>
        /// 外部との接続用
        /// </summary>
        /// <param name="list">コネクターリスト</param>
        public void SetList(List<ICbValue> list)
        {
            // 既存のリンクをカットする
            foreach (var node in ListData)
                node.RequestRemoveQurve();

            if (list is null)
                return;

            if (backupListDataForSetList is null)
            {
                // リンク時専用リストに切り替える

                backupListDataForSetList = ListData;
                ListData = new ObservableCollection<LinkConnector>();
                ListView.ItemsSource = ListData;
            }
            else
            {
                // 既にリンク時専用リストに切り替わっているのでリストをクリアする
                // ※今はこのパスは通らない

                ClearList();
            }

            // 渡されたリストの内容をリストに表示する

            CopyLinkConnectorList(list);
            if (ListData.Count != 0)
                CloseAccordion();
            else
                HideAccordion();
            EnableAdd = false;
        }

        private void CopyLinkConnectorList(List<ICbValue> list)
        {
#if SHOW_LINK_ARRAY
            foreach (var node in list)
            {
                //ListData.Add(node); ←ノードが移動してしまう

                AppendList(node);
            }
#endif

            ConnectorBackground.Visibility = ListData.Count != 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private void AppendList(ICbValue node)
        {
            var linkConnector = new LinkConnector()
            {
                OwnerCommandCanvas = this.OwnerCommandCanvas,
                ValueData = node,
                HideLinkPoint = true
            };
            linkConnector.ReadOnly = true;
            linkConnector.UpdateRootValue();
            ListData.Add(linkConnector);
        }

        private List<ICbValue> backupLinkList = null;

        /// <summary>
        /// 内部接続用
        /// </summary>
        /// <param name="list">リンクリスト</param>
        public void LinkToList(ICbList list)
        {
            if (list is null)
                return;

            // 渡されたリストをリンクする
            ListData = list.LinkConnectors;
            ListView.ItemsSource = ListData;
            if (backupListDataForLinkList is null)
            {
                backupListDataForLinkList = ListData;
                backupLinkList = list.Value;
            }

            ConnectorBackground.Visibility = ListData.Count != 0 ? Visibility.Visible : Visibility.Collapsed;
            EnableAdd = true;
        }

        public void UpdateListData(List<ICbValue> list)
        {
            if (backupListDataForSetList != null)
            {
                if (OwnerCommandCanvas.LinkConnectorListHoldAction.Enabled)
                {
                    // 画面反映はあとから一括で行う

                    OwnerCommandCanvas.LinkConnectorListHoldAction.Add(this, () =>
                            {
                                UpdateListData(list);
                            }
                        );
                    return;
                }

                bool isEmpty = ListData.Count == 0;

                if (ListData.Count > 0 && list.Count > 0 && ListData[0].ValueData.OriginalType == list[0].OriginalType)
                {
                    // 差分のコピー

                    int len = Math.Min(ListData.Count, list.Count);
                    int i = 0;
                    for (; i < len; ++i)
                    {
                        ListData[i].ValueData = list[i];
                        ListData[i].UpdateParam();
                    }
                    int remaining = ListData.Count - list.Count;
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
                                AppendList(list[i + j]);
                            }
                        }
                    }
                }
                else
                {
                    ClearList();
                    CopyLinkConnectorList(list);
                }

                if (isEmpty && ListData.Count != 0)
                    CloseAccordion();
                if (ListData.Count == 0)
                    HideAccordion();
            }
        }

        public void ClearSetList()
        {
            if (backupListDataForSetList != null)
            {
                ListData = backupListDataForSetList;
                ListView.ItemsSource = ListData;
                backupListDataForSetList = null;
                HideAccordion();
            }

            ConnectorBackground.Visibility = ListData.Count != 0 ? Visibility.Visible : Visibility.Collapsed;
            EnableAdd = true;
        }

        public void Clear()
        {
            if (backupListDataForLinkList != null)
            {
                ListData = backupListDataForLinkList;
                ListView.ItemsSource = ListData;
                backupListDataForLinkList = null;
            }
            else
            {
                ClearList();
                ConnectorBackground.Visibility = Visibility.Collapsed;
            }

            ConnectorBackground.Visibility = ListData.Count != 0 ? Visibility.Visible : Visibility.Collapsed;
            EnableAdd = true;
        }

        private void ClearList()
        {
            foreach (var node in ListData)
                if (node is IDisposable target)
                    target.Dispose();
            ListData.Clear();
        }

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
                    node.RequestExecute(functionStack, preArgument);
            }
        }

        /// <summary>
        /// 追加ボタン時ノード挿入イベント
        /// </summary>
        public Func<ObservableCollection<LinkConnector>, ICbValue> AddFunc =
            new Func<ObservableCollection<LinkConnector>, ICbValue>((ListData) => new ParamNameOnly("argument" + (ListData.Count + 1)));

        public void AddOption_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (AddFunc is null)
                return;

            AddListNode(AddFunc);
            UpdateListEvent?.Invoke();
        }

        private LinkConnector AddListNode(Func<ObservableCollection<LinkConnector>, ICbValue> addFunc)
        {
            var addNode = addFunc(ListData);

            var linkConnector = new LinkConnector()
            {
                OwnerCommandCanvas = this.OwnerCommandCanvas,
                ValueData = addNode
            };

            if (backupListDataForLinkList != null)
            {
                backupLinkList?.Add(addNode);
            }

            linkConnector.UpdateEvent = new Action(
                    () =>
                    {
                        // リストの変更を所有者に伝える

                        UpdateListEvent?.Invoke();

                        int index = backupLinkList.IndexOf(addNode);
                        if (index != -1)
                        {
                            try
                            {
                                backupLinkList[index].Set(linkConnector.ValueData);
                            }
                            catch (Exception ex)
                            {
                                // アセット実行時エラーなので無視する
                                System.Diagnostics.Debug.WriteLine(ex.Message);
                            }
                        }
                    });

            linkConnector.DeleteEventIcon.ClickEvent = new Action(
                    ()=>
                    {
                        // 削除イベント

                        backupLinkList?.Remove(addNode);
                        linkConnector.Dispose();
                        ListData.Remove(linkConnector);
                        if (ListData.Count == 0)
                            ConnectorBackground.Visibility = Visibility.Collapsed;
                        UpdateListEvent?.Invoke();
                    }
                    );

            ListData.Add(linkConnector);
            ConnectorBackground.Visibility = Visibility.Visible;
            return linkConnector;
        }

        private void CloseAccordion()
        {
            AccordionOpenIcon.Visibility = Visibility.Visible;
            AccordionCloseIcon.Visibility = Visibility.Collapsed;
            ListPanel.Visibility = Visibility.Collapsed;
        }

        private void HideAccordion()
        {
            AccordionOpenIcon.Visibility = Visibility.Collapsed;
            AccordionCloseIcon.Visibility = Visibility.Collapsed;
            ListPanel.Visibility = Visibility.Visible;
        }

        private void Accordion_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ListPanel.Visibility = (ListPanel.Visibility == Visibility.Visible) ? Visibility.Collapsed : Visibility.Visible;
            if (ListPanel.Visibility == Visibility.Visible)
            {
                AccordionOpenIcon.Visibility = Visibility.Collapsed;
                AccordionCloseIcon.Visibility = Visibility.Visible;
            }
            else
            {
                AccordionOpenIcon.Visibility = Visibility.Visible;
                AccordionCloseIcon.Visibility = Visibility.Collapsed;
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
                AddListNode(AddFunc);
                UpdateListEvent?.Invoke();
            }
        }

#region IDisposable Support
        private bool disposedValue = false; // 重複する呼び出しを検出するには

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    foreach (var node in ListData)
                    {
                        node.Dispose();
                    }
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            // TODO: 上のファイナライザーがオーバーライドされる場合は、次の行のコメントを解除してください。
            // GC.SuppressFinalize(this);
        }
#endregion
    }
}
