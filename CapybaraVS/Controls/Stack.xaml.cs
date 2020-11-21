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
    public class StackNodeIdProvider
    {
        private static Dictionary<int, StackNode> AssetList = new Dictionary<int, StackNode>();
        private static int _stackNodeId = 0;
        private int stackNodeId = ++_stackNodeId;    // 初期化時に _pointID をインクリメントする
        public int Id
        {
            get => stackNodeId;
            set
            {
                if (value >= _stackNodeId)
                {
                    // _pointID は常に最大にする

                    _stackNodeId = value + 1;
                }
                stackNodeId = value;
            }
        }
        public StackNodeIdProvider(StackNode owner)
        {
            if (owner is null)
                new NotImplementedException();
            AssetList.Add(Id, owner);
        }
        ~StackNodeIdProvider()
        {
            AssetList.Remove(Id);
        }
    }

    public class StackNode : UIParam
    {
        #region ID管理
        private StackNodeIdProvider stackNodeIdProvider = null;
        public int Id
        {
            get => stackNodeIdProvider.Id;
            set { stackNodeIdProvider.Id = value; }
        }
        #endregion

        #region XML定義
        [XmlRoot(nameof(StackNode))]
        public new class _AssetXML<OwnerClass>
            where OwnerClass : StackNode
        {
            [XmlIgnore]
            public Action WriteAction = null;
            [XmlIgnore]
            public Action<OwnerClass> ReadAction = null;
            public _AssetXML()
            {
                ReadAction = (self) =>
                {
                    self.Id = Id;
                    var valueType = new CbST();
                    VariableType.ReadAction?.Invoke(valueType);
                    self.ValueData = CbST.Create(valueType, Name);
                    if (Value != "[ERROR]")
                    {
                        if (self.ValueData != null && self.ValueData.IsStringableValue)
                        {
                            self.ValueData.ValueString = Value;
                        }
                    }
                    self.UpdateValueData();

                    // 次回の為の初期化
                    self.AssetXML = new _AssetXML<StackNode>(self);
                };
            }
            public _AssetXML(OwnerClass self)
            {
                WriteAction = () =>
                {
                    Id = self.Id;

                    // 直接作ろうとすると失敗することがあるようなのでコピーしてから作成
                    var valueType = new CbST(self.ValueData);
                    valueType.AssetXML.WriteAction?.Invoke();
                    VariableType = valueType.AssetXML;

                    if (valueType.ObjectType != CbCType.Func && valueType.LiteralType != CbType.Func)
                    {
                        // イベント系以外の値は保存対象

                        Value = self.ValueData.ValueString;
                    }
                    Name = self.ValueData.Name;
                };
            }
            #region 固有定義
            public int Id { get; set; } = 0;
            public CbST._AssetXML<CbST> VariableType { get; set; } = null;
            public string Value { get; set; } = null;
            public string Name { get; set; } = null;
            #endregion
        }
        public new _AssetXML<StackNode> AssetXML { get; set; } = null;
        #endregion

        public StackNode(CommandCanvas ownerCommandCanvas, ICbValue obj = null)
        {
            stackNodeIdProvider = new StackNodeIdProvider(this);
            OwnerCommandCanvas = ownerCommandCanvas;
            AssetXML = new _AssetXML<StackNode>(this);
            ValueData = obj;
        }
    }

    /// <summary>
    /// Stack.xaml の相互作用ロジック
    /// </summary>
    public partial class Stack 
        : UserControl
        , IHaveCommandCanvas
    {
        #region XML定義
        [XmlRoot(nameof(Stack))]
        public class _AssetXML<OwnerClass>
            where OwnerClass : Stack
        {
            [XmlIgnore]
            public Action WriteAction = null;
            [XmlIgnore]
            public Action<OwnerClass> ReadAction = null;
            public _AssetXML()
            {
                ReadAction = (self) =>
                {
                    foreach (var node in StackList)
                    {
                        StackNode stackAsset = new StackNode(self.OwnerCommandCanvas);
                        stackAsset.AssetXML = node;
                        stackAsset.AssetXML.ReadAction?.Invoke(stackAsset);
                        StackGroup sg = self.Append(stackAsset);

                        if (node.VariableType.ListNodeValue != null)
                        {
                            if (stackAsset.ValueData is ICbList cbList)
                            {
                                foreach (var vs in node.VariableType.ListNodeValue)
                                {
                                    var insertValue = cbList.NodeTF();
                                    insertValue.ValueString = vs;
                                    cbList.Value.Add(insertValue);
                                    var sn = new StackNode(self.OwnerCommandCanvas)
                                    {
                                        ValueData = insertValue
                                    };
                                    sg.AddListNode(sn);
                                }
                            }
                        }
                    }

                    // 次回の為の初期化
                    self.AssetXML = new _AssetXML<Stack>(self);
                };
            }
            public _AssetXML(OwnerClass self)
            {
                WriteAction = () =>
                {
                    List<StackNode._AssetXML<StackNode>> stackList = new List<StackNode._AssetXML<StackNode>>();
                    foreach (var stackNode in self.StackData)
                    {
                        if (stackNode.stackNode is StackNode target)
                        {
                            target.AssetXML.WriteAction?.Invoke();
                            stackList.Add(target.AssetXML);
                        }
                    }
                    StackList = stackList;
                };
            }
            #region 固有定義
            [XmlArrayItem("Asset")]
            public List<StackNode._AssetXML<StackNode>> StackList { get; set; } = null;
            #endregion
        }
        public _AssetXML<Stack> AssetXML { get; set; } = null;
        #endregion

        public ObservableCollection<StackGroup> StackData = new ObservableCollection<StackGroup>();

        /// <summary>
        /// 変数名変更時に通知する先を登録する
        /// </summary>
        private Dictionary<int, List<MultiRootConnector>> linkList = new Dictionary<int, List<MultiRootConnector>>();

        private CommandCanvas _OwnerCommandCanvas = null;

        public CommandCanvas OwnerCommandCanvas
        {
            get => _OwnerCommandCanvas;
            set
            {
                Debug.Assert(value != null);
                SetOunerCanvas(StackData, value);
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

        public Stack()
        {
            InitializeComponent();
            AssetXML = new _AssetXML<Stack>(this);
            StackList.ItemsSource = StackData;
            Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// 変数を追加
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public StackGroup Append(ICbValue obj)
        {
            StackNode stackNode = new StackNode(OwnerCommandCanvas, obj);
            return Append(stackNode);
        }

        public StackGroup Append(StackNode stackNode)
        {
            stackNode.UpdateEvent = () => UpdateLinkedVariableAsset(stackNode.Id);
            StackGroup stackGroup = new StackGroup();
            stackGroup.OwnerCommandCanvas = OwnerCommandCanvas;
            stackGroup.IsEnableDelete = () =>
            {
                return IsLinkedVariableAsset(stackNode.Id);
            };
            stackGroup.DeleteEvent = () =>
            {
                stackGroup.DeleteNode(stackNode);
                StackData.Remove(stackGroup);

                if (StackData.Count == 0)
                    Visibility = Visibility.Collapsed;
            };
            stackGroup.AddListNode(stackNode);
            StackData.Add(stackGroup);

            Visibility = Visibility.Visible;

            return stackGroup;
        }

        /// <summary>
        /// 変数の値を一括更新
        /// </summary>
        public void UpdateValueData()
        {
            foreach (var node in StackData)
            {
                node.stackNode.UpdateValueData();
            }
        }

        public void UpdateValueData(int id)
        {
            foreach (var node in OwnerCommandCanvas.ScriptWorkStack.StackData)
            {
                if (node.stackNode.Id == id)
                {
                    node.UpdateValueData();
                }
            }
        }

        public ICbValue Find(int id)
        {
            foreach (var node in OwnerCommandCanvas.ScriptWorkStack.StackData)
            {
                if (node.stackNode.Id == id)
                {
                    return node.stackNode.ValueData;
                }
            }
            return null;
        }

        /// <summary>
        /// 変数名変更通知
        /// </summary>
        private void UpdateLinkedVariableAsset(int id)
        {
            if (linkList.ContainsKey(id))
            {
                List<MultiRootConnector> multiRootConnectors = linkList[id];
                foreach (var node in multiRootConnectors)
                {
                    node.VariableUpdate();
                }
            }
        }

        private bool IsLinkedVariableAsset(int id)
        {
            if (linkList.ContainsKey(id))
            {
                List<MultiRootConnector> multiRootConnectors = linkList[id];
                return multiRootConnectors.Count == 0;
            }
            return true;
        }

        /// <summary>
        /// 変数名変更通知受付登録
        /// </summary>
        /// <param name="id">変数ID</param>
        /// <param name="target">通知を受けるコネクター</param>
        public void Link(int id, MultiRootConnector target)
        {
            if (linkList.ContainsKey(id))
            {
                List<MultiRootConnector> multiRootConnectors = linkList[id];
                multiRootConnectors.Add(target);
            }
            else
            {
                List<MultiRootConnector> multiRootConnectors = new List<MultiRootConnector>();
                multiRootConnectors.Add(target);
                linkList.Add(id, multiRootConnectors);
            }
        }

        /// <summary>
        /// 変数名変更通知受付取り消し
        /// </summary>
        /// <param name="id">変数ID</param>
        /// <param name="target">通知を受けているコネクター</param>
        public void Unlink(int id, MultiRootConnector target)
        {
            if (linkList.ContainsKey(id))
            {
                List<MultiRootConnector> multiRootConnectors = linkList[id];
                if (multiRootConnectors.Contains(target))
                {
                    multiRootConnectors.Remove(target);
                }
            }
        }

        public void Clear()
        {
            StackData.Clear();
            linkList.Clear();
        }
    }
}
