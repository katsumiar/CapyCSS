using CapyCSS.Script;
using CapyCSS.Controls;
using CbVS;
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

namespace CapyCSS.Controls.BaseControls
{
    /// <summary>
    /// StackGroup.xaml の相互作用ロジック
    /// </summary>
    public partial class StackGroup
        : UserControl
        , IHaveCommandCanvas
        , IDisposable
    {
        public ObservableCollection<StackGroup> ListData { get; set; } = new ObservableCollection<StackGroup>();

        #region EnableAdd 添付プロパティ実装

        private static ImplementDependencyProperty<StackGroup, bool> impEnableAdd =
            new ImplementDependencyProperty<StackGroup, bool>(
                nameof(EnableAdd),
                (self, getValue) =>
                {
                    bool value = getValue(self);
                    self.AddOption.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
                });

        public static readonly DependencyProperty EnableAddOptionProperty = impEnableAdd.Regist(false);

        public bool EnableAdd
        {
            get { return impEnableAdd.GetValue(this); }
            set { impEnableAdd.SetValue(this, value); }
        }

        #endregion

        #region UpdateListEvent 添付プロパティ実装

        private static ImplementDependencyProperty<StackGroup, Action> impUpdateListEvent =
            new ImplementDependencyProperty<StackGroup, Action>(
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

        private Func<StackNode> addEvent = null;
        public Func<StackNode> AddEvent
        {
            get => addEvent;
            set
            {
                addEvent = value;
                if (addEvent != null)
                {
                    EnableAdd = true;
                }
            }
        }

        public Func<bool> IsEnableDelete = null;

        public Action DeleteEvent = null;

        private StackNode CbValue = null;
        private StackNode CbListValue = null;

        private CommandCanvas _OwnerCommandCanvas = null;

        public CommandCanvas OwnerCommandCanvas
        {
            get => _OwnerCommandCanvas;
            set
            {
                Debug.Assert(value != null);
                SetOunerCanvas(ListData, value);
                if (_OwnerCommandCanvas is null)
                    _OwnerCommandCanvas = value;
            }
        }

        private void SetOunerCanvas(IEnumerable<StackGroup> list, CommandCanvas value)
        {
            if (list is null)
                return;

            foreach (var node in list)
            {
                if (node.OwnerCommandCanvas is null)
                    node.OwnerCommandCanvas = value;
            }
        }

        public StackNode stackNode
        {
            get
            {
                if (CbListValue != null)
                    return CbListValue;
                return CbValue;
            }
        }

        public StackGroup()
        {
            InitializeComponent();
            HideAccordion();
            ListView.ItemsSource = ListData;
            AddOption.Visibility = Visibility.Collapsed;
        }

        private HoldActionQueue<StackGroup> HoldAction = new HoldActionQueue<StackGroup>();

        private void Accordion_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!IsOpenList)
            {
                OpenAccordion();
            }
            else
            {
                CloseAccordion();
            }
        }

        public void UpdateValueData()
        {
            if (OwnerCommandCanvas.StackGroupHoldAction.Enabled)
            {
                // 画面反映はあとから一括で行う

                OwnerCommandCanvas.StackGroupHoldAction.Add(this, () =>
                    {
                        if (EnableAdd)
                            CloseAccordion();   // スクリプト実行後にアコーディオンを閉じる

                        _UpdateValueData();
                    }
                    );
                return;
            }
            _UpdateValueData();
        }

        private void _UpdateValueData()
        {
            if (HoldAction.Enabled)
            {
                // アコーディオンが閉じているので開いたときに更新する。

                HoldAction.Add(this, () => _UpdateValueData());
                return;
            }

            stackNode.UpdateValueData();
            if (CbListValue != null && CbListValue.ValueData.IsList)
            {
                ICbList target = CbListValue.ValueData.GetListValue;
                if (ListData.Count > 0 && target.Count > 0 &&
                    ListData[0].stackNode.ValueData.OriginalType == target[0].OriginalType)
                {
                    // 差分のコピー
                    int len = Math.Min(ListData.Count, target.Count);
                    int i = 0;
                    for (; i < len; ++i)
                    {
                        ListData[i].stackNode.ValueData = target[i];
                        ListData[i].stackNode.UpdateValueData();
                    }
                    int remaining = ListData.Count - target.Count;
                    if (remaining != 0)
                    {
                        if (remaining > 0)
                        {
                            // 多すぎる配列を消す

                            while (remaining-- > 0)
                            {
                                ListData[i].Dispose();
                                ListData.RemoveAt(i);
                            }
                        }
                        else
                        {
                            // 足りない配列を足す

                            remaining = Math.Abs(remaining);
                            for (int j = 0; j < remaining; ++j)
                            {
                                AddListNode(CbList.ConvertStackNode(OwnerCommandCanvas, target[i + j]));
                            }
                        }
                    }
                }
                else
                {
                    ListData.Clear();
                    if (!target.IsNull)
                    {
                        foreach (var node in target.Value)
                        {
                            AddListNode(CbList.ConvertStackNode(OwnerCommandCanvas, node));
                        }
                    }
                }
            }
        }

        private bool IsCancelHoldAction = false;
        private bool disposedValue;

        public StackNode AddListNode(StackNode node)
        {
            if (node is null)
                return null;

#if false   // 変数はスクリプト実行時に初期化されるようになったのでリストの特別扱いを止めた。
            // ※初期化リストを実装する場合は、この機能を再有効化して活用できそう。

            bool isGroupWithinGroup = false;
            if (CbListValue != null && CbListValue.ValueData != null)
            {
                // グループの中にグループを作らないようにする

                isGroupWithinGroup = CbListValue.ValueData.IsList;
            }

            if (node.ValueData.IsList && !isGroupWithinGroup)
            {
                ICbList cbList = node.ValueData.GetListValue;
                Debug.Assert(CbListValue is null);
                CbListValue = node;
                AddEvent = () =>
                {
                    var listNode = cbList.NodeTF();
                    cbList.Value.Add(listNode);
                    var node = new StackNode(OwnerCommandCanvas, listNode);
                    node.OwnerCommandCanvas = OwnerCommandCanvas;
                    AccordionCloseIcon.Visibility = Visibility.Collapsed;
                    AccordionOpenIcon.Visibility = Visibility.Visible;
                    return node;
                };
                InnerList.Margin = new Thickness(12, 0, 0, 0);

                ListPanel.Children.Insert(0, node);
                ListPanel.Children[0].Visibility = Visibility.Visible;
            }
            else
#endif
            {
                var grp = new StackGroup();
                grp.OwnerCommandCanvas = OwnerCommandCanvas;
                grp.ListPanel.Children.Insert(0, node);
                grp.ListPanel.Children[0].Visibility = Visibility.Visible;

                grp.MainPanelFrame.Visibility = Visibility.Collapsed;
                grp.MainPanel.Margin = new Thickness();
                grp.InnerList.Visibility = Visibility.Collapsed;

                if (CbListValue != null && CbListValue.ValueData.IsList)
                {
                    ICbList target = CbListValue.ValueData.GetListValue;
                    grp.IsEnableDelete = () => true;
                    grp.DeleteEvent = () =>
                    {
                        foreach (var lc in target.Value)
                        {
                            target.Value.Remove(node.ValueData);
                            break;
                        }
                        DeleteNode(node);
                    };
                }
                else
                {
                    CbValue = node;
                }

                grp.CbValue = node;
                if (ListData.Count == 0 || IsCancelHoldAction)
                {
                    ListData.Add(grp);
                }
                else
                {
                    // リスト構造の場合、アコーディオンを閉じて処理を保留する

                    CloseAccordion();
                    if (HoldAction.Enabled)
                    {
                        HoldAction.Add(() => ListData.Add(grp));
                    }
                }
            }
            SetConnectorBackground();
            return node;
        }

        public void DeleteNode(StackNode node)
        {
            foreach (var nd in ListData)
            {
                if (nd is StackGroup target)
                {
                    if (target.stackNode == node)
                    {
                        ListData.Remove(nd);
                        break;
                    }
                }
            }

            if (ListData.Count == 0)
            {
                IsShowAccordionIcon = false;
            }

            SetConnectorBackground();
        }

        /// <summary>
        /// アコーディオンモードで開きます。
        /// </summary>
        private void OpenAccordion()
        {
            CommandCanvasList.SetOwnerCursor(Cursors.Wait);
            Dispatcher.BeginInvoke(new Action(() =>
                {
                    CommandCanvasList.ResetOwnerCursor(Cursors.Wait);
                }),
                DispatcherPriority.ApplicationIdle
            );
            HoldAction.Enabled = false;
            if (CbValue is null)
            {
                AccordionOpenIcon.Visibility = Visibility.Collapsed;
                AccordionCloseIcon.Visibility = Visibility.Collapsed;
                CloseList();
                InnerList.Visibility = Visibility.Collapsed;
            }
            else
            {
                AccordionCloseIcon.Visibility = Visibility.Collapsed;
                AccordionOpenIcon.Visibility = Visibility.Visible;
                OpenList();
            }
        }

        /// <summary>
        /// アコーディオンモードで閉じます。
        /// </summary>
        private void CloseAccordion()
        {
            HoldAction.Enabled = true;
            AccordionCloseIcon.Visibility = Visibility.Visible;
            AccordionOpenIcon.Visibility = Visibility.Collapsed;
            CloseList();
        }

        private void SetConnectorBackground()
        {
            ConnectorBackground.Visibility = (ListData.Count > 1) ? Visibility.Visible : Visibility.Collapsed;
        }

        private bool IsOpenList => ListView.Visibility == Visibility.Visible;

        /// <summary>
        /// リストを開きます。
        /// </summary>
        private void OpenList()
        {
            ListView.Visibility = Visibility.Visible;
            InnerList.Visibility = Visibility.Visible;
            SetConnectorBackground();
            EnableAddOption = true;
        }

        /// <summary>
        /// リストを閉じます。
        /// </summary>
        private void CloseList()
        {
            ListView.Visibility = Visibility.Collapsed;
            ConnectorBackground.Visibility = Visibility.Collapsed;
            InnerList.Visibility = Visibility.Visible;
            EnableAddOption = false;
        }

        /// <summary>
        /// UIからの子要素の追加を許可するか？
        /// </summary>
        private bool EnableAddOption
        {
            get => AddOption.Visibility == Visibility.Visible;
            set
            {
                AddOption.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        /// <summary>
        /// アコーディオンを無効にします。
        /// </summary>
        private void HideAccordion()
        {
            IsShowAccordionIcon = false;
            ListView.Visibility = Visibility.Visible;
            ListPanel.Children[0].Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// アコーディオンの有効状態です。
        /// </summary>
        private bool IsShowAccordionIcon
        {
            get => AccordionCloseIcon.Visibility == Visibility.Visible || AccordionOpenIcon.Visibility == Visibility.Visible;
            set
            {
                if (value)
                {
                    AccordionCloseIcon.Visibility = IsOpenList ? Visibility.Visible : Visibility.Collapsed;
                    AccordionOpenIcon.Visibility = IsOpenList ? Visibility.Collapsed : Visibility.Visible;
                }
                else
                {
                    AccordionCloseIcon.Visibility = Visibility.Collapsed;
                    AccordionOpenIcon.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void AddOption_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (AddEvent is null)
                return;
            IsCancelHoldAction = true;  // アコーディオンが閉じないようにする
            AddListNode(AddEvent());
            IsCancelHoldAction = false;
        }

        private void Delete_MouseDown(object sender, MouseButtonEventArgs e)
        {
            DeleteEvent?.Invoke();
        }

        private void StackPanel_MouseEnter(object sender, MouseEventArgs e)
        {
            if (IsEnableDelete is null)
                return;
            if (IsEnableDelete())
                Delete.Visibility = Visibility.Visible;
        }

        private void StackPanel_MouseLeave(object sender, MouseEventArgs e)
        {
            Delete.Visibility = Visibility.Collapsed;
        }

        private void ListView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                while (ListView.SelectedItems.Count != 0)
                {
                    if (ListView.SelectedItems[0] is StackGroup stackGroup)
                        stackGroup.DeleteEvent?.Invoke();
                }
            }
            else if (e.Key == Key.Insert)
            {
                AddListNode(AddEvent());
            }
        }

        private void Accordion1_MouseEnter(object sender, MouseEventArgs e)
        {
            CommandCanvasList.SetOwnerCursor(Cursors.Hand);
        }

        private void Accordion1_MouseLeave(object sender, MouseEventArgs e)
        {
            CommandCanvasList.ResetOwnerCursor(Cursors.Hand);
        }

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
                    if (ListPanel.Children.Count != 0)
                    {
                        if (ListPanel.Children[0] is StackNode stackNode)
                        {
                            stackNode.Dispose();
                        }
                    }

                    ListData.Clear();
                    ListData = null;
                    addEvent = null;
                    IsEnableDelete = null;
                    DeleteEvent = null;
                    CbValue?.Dispose();
                    CbValue = null;
                    CbListValue?.Dispose();
                    CbListValue = null;
                    _OwnerCommandCanvas = null;
                    HoldAction.Dispose();
                    HoldAction = null;
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
