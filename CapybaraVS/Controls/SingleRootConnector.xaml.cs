using CapybaraVS.Controls.BaseControls;
using CapybaraVS.Script;
using CbVS.Script;
using System;
using System.Collections.Generic;
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
    /// SingleRootConnector.xaml の相互作用ロジック
    /// </summary>
    public partial class SingleRootConnector
        : UserControl
        , ICbExecutable
        , IDisposable
        , IHaveCommandCanvas
    {
        #region XML定義
        [XmlRoot(nameof(SingleRootConnector))]
        public class _AssetXML<OwnerClass>
            where OwnerClass : SingleRootConnector
        {
            [XmlIgnore]
            public Action WriteAction = null;
            [XmlIgnore]
            public Action<OwnerClass> ReadAction = null;
            public _AssetXML()
            {
                ReadAction = (self) =>
                {
                    self.LinkConnectorControl.AssetXML = RootConnector;
                    self.LinkConnectorControl.AssetXML.ReadAction?.Invoke(self.LinkConnectorControl);

                    // 次回の為の初期化
                    self.AssetXML = new _AssetXML<SingleRootConnector>(self);
                };
            }
            public _AssetXML(OwnerClass self)
            {
                WriteAction = () =>
                {
                    self.LinkConnectorControl.AssetXML.WriteAction?.Invoke();
                    RootConnector = self.LinkConnectorControl.AssetXML;
                };
            }
            #region 固有定義
            public RootConnector._AssetXML<RootConnector> RootConnector { get; set; } = null;
            #endregion
        }
        public _AssetXML<SingleRootConnector> AssetXML { get; set; } = null;
        #endregion

        #region Function 添付プロパティ実装

        private static ImplementDependencyProperty<SingleRootConnector, Func<List<ICbValue>, DummyArgumentsStack, ICbValue>> impFunction =
            new ImplementDependencyProperty<SingleRootConnector, Func<List<ICbValue>, DummyArgumentsStack, ICbValue>>(
                nameof(Function),
                (self, getValue) =>
                {
                    Func<List<ICbValue>, DummyArgumentsStack, ICbValue> value = getValue(self);
                    self.LinkConnectorControl.Function = value;
                });

        public static readonly DependencyProperty FunctionProperty = impFunction.Regist(null);

        public Func<List<ICbValue>, DummyArgumentsStack, ICbValue> Function
        {
            get { return impFunction.GetValue(this); }
            set { impFunction.SetValue(this, value); }
        }

        #endregion

        public CommandCanvas OwnerCommandCanvas { get; set; } = null;

        public SingleRootConnector()
        {
            InitializeComponent();
            AssetXML = new _AssetXML<SingleRootConnector>(this);
            LinkConnectorControl.SingleLinkMode = true;
            LinkConnectorControl.ValueData = new ParamNameOnly("Root");
        }

        public SingleRootConnector AppendToBox(ICbValue variable, bool openList = false)
        {
            LinkConnectorControl.AppendArgument(variable, openList);
            return this;
        }

        /// <summary>
        /// Function イベント実行依頼
        /// </summary>
        /// <param name="functionStack"></param>
        public void RequestExecute(List<object> functionStack = null, DummyArgumentsStack preArgument = null)
        {
            LinkConnectorControl.RequestExecute(functionStack, preArgument);
        }

        #region IDisposable Support
        private bool disposedValue = false; // 重複する呼び出しを検出するには

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    LinkConnectorControl.Dispose();
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
