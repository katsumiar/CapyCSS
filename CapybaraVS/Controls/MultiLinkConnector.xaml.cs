using CapyCSS.Controls.BaseControls;
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

namespace CapyCSS.Controls
{
    /// <summary>
    /// MultiLinkConnector.xaml の相互作用ロジック
    /// </summary>
    public partial class MultiLinkConnector 
        : UserControl
        , IDisposable
        , IHaveCommandCanvas
    {
        #region XML定義
        [XmlRoot(nameof(MultiLinkConnector))]
        public class _AssetXML<OwnerClass> : IDisposable
            where OwnerClass : MultiLinkConnector
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
                    self.LinkConnectorControl.AssetXML = LinkConnector;
                    self.LinkConnectorControl.AssetXML.ReadAction?.Invoke(self.LinkConnectorControl);

                    // 次回の為の初期化
                    self.AssetXML = new _AssetXML<MultiLinkConnector>(self);
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

            protected virtual void Dispose(bool disposing)
            {
                if (!disposedValue)
                {
                    if (disposing)
                    {
                        WriteAction = null;
                        ReadAction = null;

                        // 以下、固有定義開放
                        LinkConnector?.Dispose();
                        LinkConnector = null;
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
        public _AssetXML<MultiLinkConnector> AssetXML { get; set; } = null;
        #endregion

        public CommandCanvas OwnerCommandCanvas { get; set; } = null;

        public MultiLinkConnector()
        {
            InitializeComponent();
            AssetXML = new _AssetXML<MultiLinkConnector>(this);
            LinkConnectorControl.SingleLinkMode = false;
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
                    AssetXML?.Dispose();
                    AssetXML = null;
                    OwnerCommandCanvas = null;
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
