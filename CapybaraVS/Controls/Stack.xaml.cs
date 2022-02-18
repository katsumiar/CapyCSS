using CapyCSS.Controls.BaseControls;
using CapyCSS.Script;
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

namespace CapyCSS.Controls
{
    public class StackNodeIdProvider
        : IDisposable
    {
        private static Dictionary<int, StackNode> AssetList = new Dictionary<int, StackNode>();
        private static int _stackNodeId = 0;
        private int stackNodeId = ++_stackNodeId;    // 初期化時に _pointID をインクリメントする
        private bool disposedValue;

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

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    AssetList.Clear();  // static なのでクリアだけする
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // このコードを変更しないでください。クリーンアップ コードを 'Dispose(bool disposing)' メソッドに記述します
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }

    public class StackNode
        : UIParam
        , IDisposable
    {
        #region ID管理
        private StackNodeIdProvider stackNodeIdProvider = null;
        private bool disposedValue;

        public int Id
        {
            get => stackNodeIdProvider.Id;
            set { stackNodeIdProvider.Id = value; }
        }
        #endregion

        #region XML定義
        [XmlRoot(nameof(StackNode))]
        public new class _AssetXML<OwnerClass> : IDisposable
            where OwnerClass : StackNode
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
                    self.Id = Id;
                    self.ValueData = CbST.CbCreate(CbST.GetTypeEx(AssetValueType), Name);
                    self.ValueData.IsLiteral = false;   // 変数なのでリテラルフラグを落とす
                    if (Value != CbSTUtils.ERROR_STR)
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
                    AssetValueType = self.ValueData.OriginalType.FullName;
                    if (!self.ValueData.IsDelegate)
                    {
                        // イベント系以外の値は保存対象

                        Value = self.ValueData.ValueUIString;
                    }
                    Name = self.ValueData.Name;
                };
            }
            #region 固有定義
            public int Id { get; set; } = 0;
            public string AssetValueType { get; set; } = null;
            public string Value { get; set; } = null;
            public string Name { get; set; } = null;

            protected virtual void Dispose(bool disposing)
            {
                if (!disposedValue)
                {
                    if (disposing)
                    {
                        WriteAction = null;
                        ReadAction = null;

                        // 以下、固有定義開放
                        AssetValueType = null;
                        Value = null;
                        Name = null;
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
        public new _AssetXML<StackNode> AssetXML { get; set; } = null;
        #endregion

        public StackNode(CommandCanvas ownerCommandCanvas, ICbValue obj = null)
        {
            stackNodeIdProvider = new StackNodeIdProvider(this);
            OwnerCommandCanvas = ownerCommandCanvas;
            AssetXML = new _AssetXML<StackNode>(this);
            ValueData = obj;
        }

        protected override void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    stackNodeIdProvider.Dispose();
                    stackNodeIdProvider = null;
                    AssetXML?.Dispose();
                    AssetXML = null;
                }
                disposedValue = true;
            }
        }

        public new void Dispose()
        {
            // このコードを変更しないでください。クリーンアップ コードを 'Dispose(bool disposing)' メソッドに記述します
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }

    /// <summary>
    /// Stack.xaml の相互作用ロジック
    /// </summary>
    public partial class Stack 
        : UserControl
        , IHaveCommandCanvas
        , ICbRequestScriptBuild
        , IDisposable
    {
        #region XML定義
        [XmlRoot(nameof(Stack))]
        public class _AssetXML<OwnerClass> : IDisposable
            where OwnerClass : Stack
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
                    foreach (var node in StackList)
                    {
                        StackNode stackAsset = new StackNode(self.OwnerCommandCanvas);
                        stackAsset.AssetXML = node;
                        stackAsset.AssetXML.ReadAction?.Invoke(stackAsset);
                        StackGroup sg = self.Append(stackAsset);
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

            protected virtual void Dispose(bool disposing)
            {
                if (!disposedValue)
                {
                    if (disposing)
                    {
                        WriteAction = null;
                        ReadAction = null;

                        // 以下、固有定義開放
                        CbSTUtils.ForeachDispose(StackList);
                        StackList = null;
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
        public _AssetXML<Stack> AssetXML { get; set; } = null;
        #endregion

        public ObservableCollection<StackGroup> StackData = new ObservableCollection<StackGroup>();

        /// <summary>
        /// 変数名変更時に通知する先を登録する
        /// </summary>
        private Dictionary<int, List<MultiRootConnector>> linkList = new Dictionary<int, List<MultiRootConnector>>();

        private CommandCanvas _OwnerCommandCanvas = null;
        private bool disposedValue;

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
        /// 変数を追加します。
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public StackGroup Append(ICbValue obj)
        {
            obj.IsLiteral = false;   // 変数なのでリテラルフラグを落とす
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
            StackNode node = FindStackNode(id);
            if (node != null)
                return node.ValueData;
            return null;
        }

        public StackNode FindStackNode(int id)
        {
            foreach (var node in OwnerCommandCanvas.ScriptWorkStack.StackData)
            {
                if (node.stackNode.Id == id)
                {
                    return node.stackNode;
                }
            }
            return null;
        }

        public bool NameContains(string name)
        {
            foreach (var node in OwnerCommandCanvas.ScriptWorkStack.StackData)
            {
                if (node.stackNode.ParamName == name)
                {
                    return true;
                }
            }
            return false;
        }

        public void FindSet(int id, ICbValue value)
        {
            foreach (var node in OwnerCommandCanvas.ScriptWorkStack.StackData)
            {
                if (node.stackNode.Id == id)
                {
                    var stackNode = node.stackNode;
                    string name = stackNode.ValueData.Name;
                    stackNode.ValueData = value;
                    stackNode.ValueData.Name = name;
                    return;
                }
            }
            Debug.Assert(false);    // id が見つからなかった
        }

        private bool IsAlreadyUsedName(StackNode self, string name)
        {
            foreach (var checkNode in OwnerCommandCanvas.ScriptWorkStack.StackData)
            {
                if (self != checkNode.stackNode &&
                    checkNode.stackNode.ParamName == name)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 変数名変更通知
        /// </summary>
        private void UpdateLinkedVariableAsset(int id)
        {
            if (linkList.ContainsKey(id))
            {
                StackNode stackNode = FindStackNode(id);
                if (stackNode is null)
                    return;

                CheckAlreadyUsedName(stackNode);

                List<MultiRootConnector> multiRootConnectors = linkList[id];
                foreach (var node in multiRootConnectors)
                {
                    // 関係するノードの表示を更新

                    node.VariableUpdate();
                }
            }
        }

        /// <summary>
        /// 既に使われている名前が設定されたなら名前を変更します。
        /// </summary>
        /// <param name="stackNode"></param>
        private void CheckAlreadyUsedName(StackNode stackNode)
        {
            int nameIndex = 1;
            string name = stackNode.ParamName;
            while (IsAlreadyUsedName(stackNode, name))
            {
                // 新しい名前候補を作成

                name = stackNode.ParamName + nameIndex++;
            }
            if (nameIndex != 1)
            {
                // 新しい名前に変更

                stackNode.ParamNameLabel.LabelString = name;
                stackNode.ParamNameLabel.UpdateEvent();
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
            CbSTUtils.ForeachDispose(StackData);
            CbSTUtils.ForeachDispose(linkList);
            foreach (var node in linkList)
            {
                CbSTUtils.ForeachDispose(node.Value);
            }
            linkList.Clear();
        }

        public BuildScriptInfo RequestBuildScript()
        {
            BuildScriptInfo result = BuildScriptInfo.CreateBuildScriptInfo(null);
            foreach (var stackNode in StackData)
            {
                if (stackNode.stackNode is StackNode variable)
                {
                    var value = variable.ValueData;
                    string name = CbSTUtils.GetTryFullName(value.OriginalType);
                    result.Add(BuildScriptInfo.CreateBuildScriptInfo(null, name, BuildScriptInfo.CodeType.StackVariable, value.Name));
                }
            }
            return result;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    AssetXML?.Dispose();
                    AssetXML = null;
                    Clear();
                    StackData = null;
                    linkList = null;
                    _OwnerCommandCanvas = null;
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
