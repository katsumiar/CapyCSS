using CapybaraVS.Controls.BaseControls;
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
    /// SingleLinkConnector.xaml の相互作用ロジック
    /// </summary>
    public partial class SingleLinkConnector : UserControl, IDisposable
    {
        #region XML定義
        [XmlRoot(nameof(SingleLinkConnector))]
        public class _AssetXML<OwnerClass>
            where OwnerClass : SingleLinkConnector
        {
            [XmlIgnore]
            public Action WriteAction = null;
            [XmlIgnore]
            public Action<OwnerClass> ReadAction = null;
            public _AssetXML()
            {
                ReadAction = (self) =>
                {
                    self.LinkConnectorControl.AssetXML = LinkConnector;
                    self.LinkConnectorControl.AssetXML.ReadAction?.Invoke(self.LinkConnectorControl);

                    // 次回の為の初期化
                    self.AssetXML = new _AssetXML<SingleLinkConnector>(self);
                };
            }
            public _AssetXML(OwnerClass self)
            {
                WriteAction = () =>
                {
                    self.LinkConnectorControl.AssetXML.WriteAction?.Invoke();
                    LinkConnector = self.LinkConnectorControl.AssetXML;
                };
            }
            #region 固有定義
            public LinkConnector._AssetXML<LinkConnector> LinkConnector { get; set; } = null;
            #endregion
        }
        public _AssetXML<SingleLinkConnector> AssetXML { get; set; } = null;
        #endregion

        public SingleLinkConnector()
        {
            InitializeComponent();
            AssetXML = new _AssetXML<SingleLinkConnector>(this);
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
