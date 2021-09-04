using CapybaraVS.Controls.BaseControls;
using CapybaraVS.Script;
using System;
using System.Collections.Generic;
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
    public interface IRunableControl
    {

    }

    public enum RunableFunctionType
    {
        none,
        Execute,
    }

    /// <summary>
    /// RunableControl.xaml の相互作用ロジック
    /// </summary>
    public partial class RunableControl
        : UserControl
        , IDisposable
        , IRunableControl
        , IHaveCommandCanvas
    {
        #region XML定義
        [XmlRoot(nameof(RunableControl))]
        public class _AssetXML<OwnerClass> : IDisposable
            where OwnerClass : RunableControl
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
                    self.AssetFunctionType = RunableFunc.AssetFunctionType;

                    self.LinkConnectorListControl.AssetXML = List;
                    self.LinkConnectorListControl.AssetXML.ReadAction?.Invoke(self.LinkConnectorListControl);

                    self.CaptionLabel.AssetXML = Caption;
                    self.CaptionLabel.AssetXML.ReadAction?.Invoke(self.CaptionLabel);

                    // 次回の為の初期化
                    self.AssetXML = new _AssetXML<RunableControl>(self);
                };
            }
            public _AssetXML(OwnerClass self)
            {
                WriteAction = () =>
                {
                    RunableFuncType info = new RunableFuncType();
                    info.AssetFunctionType = self.AssetFunctionType;
                    RunableFunc = info;

                    self.LinkConnectorListControl.AssetXML.WriteAction?.Invoke();
                    List = self.LinkConnectorListControl.AssetXML;

                    self.CaptionLabel.AssetXML.WriteAction?.Invoke();
                    Caption = self.CaptionLabel.AssetXML;
                };
            }
            #region 固有定義
            public class RunableFuncType
            {
                [XmlAttribute(nameof(AssetFunctionType))]
                public RunableFunctionType AssetFunctionType { get; set; }
            }
            public RunableFuncType RunableFunc { get; set; } = null;
            public LinkConnectorList._AssetXML<LinkConnectorList> List { get; set; } = null;
            public NameLabel._AssetXML<NameLabel> Caption { get; set; } = null;

            protected virtual void Dispose(bool disposing)
            {
                if (!disposedValue)
                {
                    if (disposing)
                    {
                        WriteAction = null;
                        ReadAction = null;

                        // 以下、固有定義開放
                        RunableFunc = null;
                        List?.Dispose();
                        List = null;
                        Caption?.Dispose();
                        Caption = null;
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
        public _AssetXML<RunableControl> AssetXML { get; set; } = null;
        #endregion

        #region AssetFunctionType 添付プロパティ実装

        private static ImplementDependencyProperty<RunableControl, RunableFunctionType> impAssetFunctionType =
            new ImplementDependencyProperty<RunableControl, RunableFunctionType>(
                nameof(AssetFunctionType),
                (self, getValue) =>
                {
                    RunableFunctionType value = getValue(self);
                    self.MakeFunction();
                });

        public static readonly DependencyProperty AssetFunctionTypeProperty = impAssetFunctionType.Regist(RunableFunctionType.none);

        public RunableFunctionType AssetFunctionType
        {
            get { return impAssetFunctionType.GetValue(this); }
            set { impAssetFunctionType.SetValue(this, value); }
        }

        #endregion

        private RelayCommand runCommand = null;

        #region Caption プロパティ実装

        private static ImplementDependencyProperty<RunableControl, string> impLabelString =
            new ImplementDependencyProperty<RunableControl, string>(
                nameof(Caption),
                (self, getValue) =>
                {
                    self.CaptionLabel.LabelString = getValue(self);
                });

        public static readonly DependencyProperty LabelStringProperty = impLabelString.Regist(nameof(Caption));

        public string Caption
        {
            get { return impLabelString.GetValue(this); }
            set { impLabelString.SetValue(this, value); }
        }

        #endregion

        public RunableControl()
        {
            InitializeComponent();
            AssetXML = new _AssetXML<RunableControl>(this);
            DataContext = this;
        }

        ~RunableControl()
        {
            Dispose();
        }

        private CommandCanvas _OwnerCommandCanvas = null;

        public CommandCanvas OwnerCommandCanvas
        {
            get => _OwnerCommandCanvas;
            set
            {
                Debug.Assert(value != null);
                SetOunerCanvas(SetGrid.Children, value);
                if (_OwnerCommandCanvas is null)
                    _OwnerCommandCanvas = value;
            }
        }

        private void SetOunerCanvas(UIElementCollection list, CommandCanvas value)
        {
            if (list is null)
                return;

            foreach (var node in list)
            {
                if (node is IHaveCommandCanvas haveCommandCanvas)
                {
                    if (haveCommandCanvas.OwnerCommandCanvas is null)
                        haveCommandCanvas.OwnerCommandCanvas = value;
                }
            }
        }

        public void SetContents(UIElement element)
        {
            SetGrid.Children.Add(element);
        }

        public string Text  // この迂回設定はいずれ解消したい
        {
            get => Caption;
            set
            {
                Caption = value;
            }
        }

        public RelayCommand RunCommand
        {
            get => runCommand;
            set
            {
                runCommand = value;
            }
        }

        private void MakeFunction()
        {
            switch (AssetFunctionType)
            {
                case RunableFunctionType.none:
                    break;

                case RunableFunctionType.Execute:
                    {
                        Caption = "Node List";
                        RunCommand = new RelayCommand(
                            (list) =>
                            {
                                int count = 0;
                                if (list is LinkConnectorList linkConnectorList)
                                {
                                    foreach (LinkConnector node in linkConnectorList.ListData)
                                    {
                                        count++;
                                        node.RequestExecute(null, null);
                                        ICbValue value = node.ValueData;
                                        if (value != null)
                                            OwnerCommandCanvas.CommandCanvasControl.MainLog.OutLine(OwnerCommandCanvas.ScriptWorkCanvas.Name, value.ValueUIString);
                                    }
                                    OwnerCommandCanvas.CommandCanvasControl.MainLog.OutLine(OwnerCommandCanvas.ScriptWorkCanvas.Name, "[Total Count] " + count);
                                }
                                else
                                {
                                    OwnerCommandCanvas.CommandCanvasControl.MainLog.OutLine(OwnerCommandCanvas.ScriptWorkCanvas.Name, "not found data!!!");
                                }
                            });
                    }
                    break;
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
                    LinkConnectorListControl.Dispose();
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
