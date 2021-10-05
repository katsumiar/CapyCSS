#define SHOW_LINK_ARRAY   // リスト型を接続したときにリストの要素をコピーして表示する

using CapybaraVS.Script;
using CapyCSS.Script;
using CbVS.Script;
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
using System.Windows.Threading;
using System.Xml.Serialization;

namespace CapybaraVS.Controls.BaseControls
{
    /// <summary>
    /// UIParam.xaml の相互作用ロジック
    /// </summary>
    public partial class UIParam 
        : UserControl
        , IHaveCommandCanvas
        , IDisposable
    {
        #region XML定義
        [XmlRoot(nameof(UIParam))]
        public class _AssetXML<OwnerClass> : IDisposable
            where OwnerClass : UIParam
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
                    self.ParamNameLabelOverlap = ParamNameLabelOverlap;
                    self.ParamName = ParamName;

                    self.ParamNameLabel.AssetXML = ParamNameLabel;
                    self.ParamNameLabel.AssetXML.ReadAction?.Invoke(self.ParamNameLabel);

                    // 次回の為の初期化
                    self.AssetXML = new _AssetXML<UIParam>(self);
                };
            }
            public _AssetXML(OwnerClass self)
            {
                WriteAction = () =>
                {
                    ParamNameLabelOverlap = self.ParamNameLabelOverlap;
                    ParamName = self.ParamName;

                    self.ParamNameLabel.AssetXML.WriteAction?.Invoke();
                    ParamNameLabel = self.ParamNameLabel.AssetXML;
                };
            }
            #region 固有定義
            public string ParamName { get; set; } = "";
            public string ParamNameLabelOverlap { get; set; } = "";
            public NameLabel._AssetXML<NameLabel> ParamNameLabel { get; set; } = null;

            protected virtual void Dispose(bool disposing)
            {
                if (!disposedValue)
                {
                    if (disposing)
                    {
                        WriteAction = null;
                        ReadAction = null;

                        // 以下、固有定義開放
                        ParamName = null;
                        ParamNameLabelOverlap = null;
                        ParamNameLabel?.Dispose();
                        ParamNameLabel = null;
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
        public _AssetXML<UIParam> AssetXML { get; set; } = null;
        #endregion

        #region ParamEdit 添付プロパティ実装
        private static ImplementDependencyProperty<UIParam, string> impParamEdit =
            new ImplementDependencyProperty<UIParam, string>(
                nameof(ParamEdit),
                (self, getValue) =>
                {
                    string text = getValue(self);
                    try
                    {
                        if (self.ValueData.IsStringableValue)
                        {
                            self.ValueData.ValueString = text;
                        }
                        self.Edit.Text = self.ValueData.ValueUIString.Trim('\r', '\n');
                        self.Edit.Background = Brushes.White;
                        self.ToolTipUpdate();
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine(ex.Message);
                        self.Edit.Background = Brushes.Salmon;
                    }
                    self.ParamName = self.ValueData.Name;
                });

        public static readonly DependencyProperty ParamEditProperty = impParamEdit.Regist("ParamEdit");

        public string ParamEdit
        {
            get { return impParamEdit.GetValue(this); }
            set { impParamEdit.SetValue(this, value); }
        }
        #endregion

        #region TypeName 添付プロパティ実装
        private static ImplementDependencyProperty<UIParam, string> impTypeName =
            new ImplementDependencyProperty<UIParam, string>(
                nameof(TypeName),
                (self, getValue) =>
                {
                    string value = getValue(self);
                    if (self.TypeNameLabelOverlap.Length != 0 && !(self.ValueData is ParamNameOnly))
                    {
                        self.TypeNameLabel.Content = self.TypeNameLabelOverlap;
                    }
                    self.TypeNameLabel.FontWeight = FontWeights.UltraBold;
                    if (self.ValueData.IsIn)
                    {
                        value = $"{CbSTUtils.UI_IN_STR} {value}";
                    }
                    else if (self.ValueData.IsOut)
                    {
                        value = $"{CbSTUtils.UI_OUT_STR} {value}";
                    }
                    else if (self.ValueData.IsByRef)
                    {
                        value = $"{CbSTUtils.UI_REF_STR} {value}";
                    }
                    else if (self.ValueData.IsNullable)
                    {
                        value = $"{value}?";
                    }
                    else
                    {
                        self.TypeNameLabel.FontWeight = FontWeights.Normal;
                    }
                    self.TypeNameLabel.Content = value;
                });

        public static readonly DependencyProperty TypeNameProperty = impTypeName.Regist("TypeName");

        public string TypeName
        {
            get { return impTypeName.GetValue(this); }
            set { impTypeName.SetValue(this, value); }
        }
        #endregion

        #region ParamName 添付プロパティ実装
        private static ImplementDependencyProperty<UIParam, string> impParamName =
            new ImplementDependencyProperty<UIParam, string>(
                nameof(ParamName),
                (self, getValue) =>
                {
                    string text = getValue(self);
                    self.ParamNameLabel.LabelString = text;
                    if (self.ParamNameLabelOverlap.Length != 0 && !(self.ValueData is ParamNameOnly))
                    {
                        self.ParamNameLabel.LabelString = self.ParamNameLabelOverlap;
                    }
                    if (self.ValueData is ICbValue value)
                    {
                        value.Name = text;  // 変数に名前もコピーする
                    }
                });

        public static readonly DependencyProperty ParamNameProperty = impParamName.Regist("ParamName");

        public string ParamName
        {
            get { return impParamName.GetValue(this); }
            set { impParamName.SetValue(this, value); }
        }
        #endregion

        #region ValueData プロパティ実装
        private static ImplementDependencyProperty<UIParam, ICbValue> impValueData =
            new ImplementDependencyProperty<UIParam, ICbValue>(
                nameof(ValueData),
                (self, getValue) =>
                {
                    ICbValue valueData = getValue(self);
                    if (valueData != null)
                    {
                        self.UpdateValueData(valueData);
                    }
                });

        public static readonly DependencyProperty ValueDataProperty = impValueData.Regist(null);

        public ICbValue ValueData
        {
            get { return impValueData.GetValue(this); }
            set { impValueData.SetValue(this, value); }
        }

        public void UpdateValueData(ICbValue valueData = null)
        {
            valueData ??= ValueData;

            if (OwnerCommandCanvas.UIParamHoldAction != null && OwnerCommandCanvas.UIParamHoldAction.Enabled)
            {
                // 画面反映はあとから一括で行う

                OwnerCommandCanvas.UIParamHoldAction.Add(this, () => UpdateValueData(valueData));
                return;
            }

            _UpdateValueData(valueData);
        }

        private void _UpdateValueData(ICbValue valueData)
        {
            if (valueData is null)
                return;

            if (valueData.TypeName.Length != 0)
            {
                // 型名

                TypeName = valueData.TypeName;
            }
            if (valueData.Name.Length != 0)
            {
                // パラメータ名

                ParamName = valueData.Name;
            }

            if (ParamName.Length == 0)
            {
                // 何もないと編集もできなくなるので空白を入れておく

                ParamName = " ";
            }

            TypeNameLabel.Visibility = TypeName.Length != 0 ? Visibility.Visible : Visibility.Collapsed;
            ParamNameLabel.Visibility = ParamName.Length != 0 ? Visibility.Visible : Visibility.Collapsed;
            if (ParamNameLabelOverlap.Length != 0)
            {
                ParamNameLabel.Visibility = Visibility.Visible;
            }

            Edit.Visibility = Visibility.Collapsed;
            Select.Visibility = Visibility.Collapsed;
            ImagePanel.Visibility = Visibility.Collapsed;
            MediaPanel.Visibility = Visibility.Collapsed;

            // valueData.Data が null でも表示は必要
            if (!valueData.IsNull && valueData is ICbValueEnum selectValue)
            {
                Select.Visibility = Visibility.Visible;
                UpdateTypeEnum(valueData, selectValue);
            }
            else if (valueData is ICbClass cbClass && cbClass.OriginalReturnType == typeof(BitmapImage))
            {
                // Bitmapイメージを表示する

                if (valueData.Data != null)
                {
                    var image = (valueData.Data as BitmapImage).Clone();

                    image.DecodePixelWidth = (int)ImageBox.Width;
                    image.DecodePixelHeight = (int)ImageBox.Height;
                    ImageBox.Source = image;
                }

                ImagePanel.Visibility = Visibility.Visible;
            }
            else if (valueData is ICbClass cbClass2 && cbClass2.OriginalReturnType == typeof(BitmapSource))
            {
                // Bitmapイメージを表示する

                if (valueData.Data != null)
                {
                    var image = valueData.Data as BitmapSource;
                    ImageBox.Source = image;
                }

                ImagePanel.Visibility = Visibility.Visible;
            }
            else if (valueData is ICbClass cbClass3 && cbClass3.OriginalReturnType == typeof(MediaPlayer))
            {
                // MediaPlayerイメージを表示する

                if (valueData.Data != null)
                {
                    MediaPlayer image = valueData.Data as MediaPlayer;
                    MediaBox.Source = image.Source;
                    MediaBox.LoadedBehavior = MediaState.Stop;
                    MediaBox.Visibility = Visibility.Visible;
                }
                else
                {
                    MediaBox.LoadedBehavior = MediaState.Close;
                    MediaBox.Visibility = Visibility.Hidden;
                }

                MediaPanel.Visibility = Visibility.Visible;
            }
            else
            {
                Edit.Visibility = Visibility.Visible;

                if (valueData.ValueUIString != null)
                {
                    ParamEdit = valueData.ValueUIString;
                }
                ToolTipUpdate();    // 必ず更新確認が必要

                Edit.IsReadOnly = valueData.IsReadOnlyValue || ReadOnly || valueData.IsNull;

                if (Edit.IsReadOnly)
                {
                    Edit.Background = Brushes.Lavender;
                }

                if (valueData is CbText cbText)
                {
                    Edit.ToolTip = null;
                    Edit.MaxWidth = Edit.MinWidth = Edit.Width = 360;
                    Edit.MaxHeight = Edit.MinHeight = Edit.Height = 300;
                    Edit.AcceptsReturn = true;
                    Edit.AcceptsTab = true;
                    Edit.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
                    Edit.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
                    //Edit.TextWrapping = TextWrapping.Wrap;
                    if (!Edit.IsReadOnly)
                    {
                        Edit.Background = Brushes.Honeydew;
                    }
                }
                else
                {
                    Edit.MaxHeight = 36;
                    if (!Edit.IsReadOnly)
                    {
                        Edit.Background = Brushes.White;
                    }
                }
            }
            if (!valueData.IsVisibleValue)
            {
                Select.Visibility = Edit.Visibility = Visibility.Collapsed;
            }
            Error.Visibility = valueData.IsError ? Visibility.Visible : Visibility.Collapsed;
            if (valueData.IsError)
            {
                Error.ToolTip = valueData.ErrorMessage;
            }
            else if (valueData is ICbClass && valueData.TypeName == CbSTUtils.VOID_STR)
            {
                Edit.Visibility = Visibility.Collapsed;
            }
        }

        private void UpdateTypeEnum(ICbValue valueData, ICbValueEnum selectValue)
        {
            Select.Items.Clear();

            int selectIndex = 0;
            int count = 0;
            foreach (var node in selectValue.ElementList)
            {
                Select.Items.Add(node);
                if (node == valueData.ValueUIString ||
                    selectValue.TypeName + "." + node == valueData.ValueUIString)
                {
                    selectIndex = count;
                }
                count++;
            }
            Select.SelectedIndex = selectIndex;

            Select.IsReadOnly = valueData.IsReadOnlyValue || ReadOnly || valueData.IsNull;
            Select.IsHitTestVisible = !Select.IsReadOnly;
            Select.IsTabStop = !Select.IsReadOnly;
            if (Select.IsReadOnly)
            {
                Select.Foreground = Brushes.DarkGray;
            }
            else
            {
                Select.Foreground = Brushes.Black;
            }
        }
#endregion

        #region UpdateEvent 添付プロパティ実装

        private static ImplementDependencyProperty<UIParam, Action> impUpdateEvent =
            new ImplementDependencyProperty<UIParam, Action>(
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

        private static ImplementDependencyProperty<UIParam, bool> impReadOnly =
            new ImplementDependencyProperty<UIParam, bool>(
                nameof(ReadOnly),
                (self, getValue) =>
                {
                    bool value = getValue(self);
                    if (self.ValueData is null)
                    {
                        self.Edit.IsReadOnly = true;
                    }
                    else
                    {
                        self.Edit.IsReadOnly = self.ValueData.IsReadOnlyValue || value;
                    }
                });

        public static readonly DependencyProperty ReadOnlyProperty = impReadOnly.Regist(false);

        public bool ReadOnly
        {
            get { return impReadOnly.GetValue(this); }
            set { impReadOnly.SetValue(this, value); }
        }

        #endregion

        #region ParamNameLabelOverlap 添付プロパティ実装
        private static ImplementDependencyProperty<UIParam, string> impParamNameLabelOverlap =
            new ImplementDependencyProperty<UIParam, string>(
                nameof(ParamNameLabelOverlap),
                (self, getValue) =>
                {
                    string text = getValue(self);
                    self.ParamNameLabel.LabelString = text;
                });

        public static readonly DependencyProperty ParamNameLabelOverlapProperty = impParamNameLabelOverlap.Regist("");

        public string ParamNameLabelOverlap
        {
            get { return impParamNameLabelOverlap.GetValue(this); }
            set { impParamNameLabelOverlap.SetValue(this, value); }
        }
        #endregion

        #region TypeNameLabelOverlap 添付プロパティ実装
        private static ImplementDependencyProperty<UIParam, string> impTypeNameLabelOverlap =
            new ImplementDependencyProperty<UIParam, string>(
                nameof(TypeNameLabelOverlap),
                (self, getValue) =>
                {
                    string text = getValue(self);
                    self.TypeNameLabel.Content = text;
                });

        public static readonly DependencyProperty TypeNameLabelOverlapProperty = impTypeNameLabelOverlap.Regist("");

        public string TypeNameLabelOverlap
        {
            get { return impTypeNameLabelOverlap.GetValue(this); }
            set { impTypeNameLabelOverlap.SetValue(this, value); }
        }
        #endregion

        #region CastType 添付プロパティ実装
        private static ImplementDependencyProperty<UIParam, bool> impCastType =
            new ImplementDependencyProperty<UIParam, bool>(
                nameof(CastType),
                (self, getValue) =>
                {
                    bool value = getValue(self);
                    self.TypeNameLabel.Foreground = value ? Brushes.OrangeRed : Brushes.Black;
                });

        public static readonly DependencyProperty impCastTypeProperty = impCastType.Regist(false);

        public bool CastType
        {
            get { return impCastType.GetValue(this); }
            set { impCastType.SetValue(this, value); }
        }
        #endregion

        private CommandCanvas _OwnerCommandCanvas = null;
        private bool disposedValue;

        public CommandCanvas OwnerCommandCanvas
        {
            get => _OwnerCommandCanvas;
            set
            {
                Debug.Assert(value != null);
                if (_OwnerCommandCanvas is null)
                    _OwnerCommandCanvas = value;
            }
        }

        public UIParam()
        {
            InitializeComponent();
            AssetXML = new _AssetXML<UIParam>(this);
            ParamNameLabel.UpdateEvent =
                new Action(
                () =>
                {
                    ParamNameLabelOverlap = ParamNameLabel.LabelString;
                    ParamName = ParamNameLabel.LabelString;
                    UpdateEvent?.Invoke();
                }
                );
            Edit.LostFocus += ExitEditMode;
        }

        private void ToolTipUpdate()
        {
            string valueString = null;
            if (ValueData is CbObject cbObject)
            {
                if (!cbObject.IsNull)
                {
                    if (cbObject.Data is ICbShowValue cbVSShow)
                    {
                        valueString = cbVSShow.DataString;
                    }
                    else
                    {
                        valueString = CbSTUtils.DataToString(cbObject.Data);
                    }
                }
                else
                {
                    valueString = cbObject.ValueUIString;
                }
            }
            else if (ValueData is ICbClass cbClass)
            {
                if (cbClass.Data != null)
                {
                    if (cbClass.Data is ICbShowValue cbVSShow)
                    {
                        valueString = cbVSShow.DataString;
                    }
                    else
                    {
                        valueString = CbSTUtils.DataToString(cbClass.Data);
                    }
                }
                else
                {
                    valueString = cbClass.ValueUIString;
                }
            }
#if !SHOW_LINK_ARRAY
            else if (ValueData.IsList)
            {
                ICbList cbList = ValueData.GetListValue;
                if (!cbList.IsNull && cbList is ICbShowValue cbVSShow)
                {
                    valueString = cbVSShow.DataString;
                }
                else
                {
                    valueString = cbList.ValueUIString;
                }
            }
#endif
            else
            {
                if (!ValueData.IsNull && ValueData.Data is ICbShowValue cbVSShow)
                {
                    valueString = cbVSShow.DataString;
                }
                else
                {
                    valueString = ValueData.ValueUIString;
                }
            }
            if (valueString is null)
            {
                valueString = Edit.Text;
            }
            Edit.ToolTip = valueString.Trim('\r', '\n');
        }

        private void Edit_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && !(ValueData is CbText))
            {
                ExitEditMode();
            }
        }

        private void ExitEditMode(object sender = null, RoutedEventArgs e = null)
        {
            // 編集した後に正しい形式に変換する

            ParamEdit = Edit.Text;
            UpdateEvent?.Invoke();
        }

        private void Select_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedItem = (string)Select.SelectedItem;
            if (selectedItem == null)
                return;

            string value = selectedItem as string;
            if (ValueData.ValueString.ToUpper() != value.ToUpper())
            {
                ValueData.ValueString = selectedItem as string;
            }
            UpdateEvent?.Invoke();
        }

        private void MediaBox_MouseEnter(object sender, MouseEventArgs e)
        {
            MediaBox.Position = TimeSpan.Zero;
            MediaBox.LoadedBehavior = MediaState.Manual;
            MediaBox.Play();
        }

        private void MediaBox_MouseLeave(object sender, MouseEventArgs e)
        {
            MediaBox.Stop();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Edit.LostFocus -= ExitEditMode;
                    Edit.ToolTip = null;
                    AssetXML?.Dispose();
                    AssetXML = null;
                    _OwnerCommandCanvas = null;
                    ParamNameLabel.Dispose();
                    ValueData?.Dispose();
                    ValueData = null;
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
