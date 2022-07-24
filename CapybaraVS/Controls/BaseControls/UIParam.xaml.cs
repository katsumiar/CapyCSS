using CapyCSS.Script;
using CapyCSS.Script.Lib;
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

namespace CapyCSS.Controls.BaseControls
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
                        self.Edit.Background = (Brush)Application.Current.FindResource("ParamBackgroundBrush");
                        self.ToolTipUpdate();
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine(ex.Message);
                        self.Edit.Background = (Brush)Application.Current.FindResource("EditErrorBackgroundBrush");
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

        /// <summary>
        /// パラメータを更新します。
        /// </summary>
        /// <param name="valueData"></param>
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

                if (valueData.Name == "self")
                {
                    TypePanel.Fill = (Brush)Application.Current.FindResource("SelfParamBackgroundBrush");
                }
                ParamName = valueData.Name;
            }

            ShowNameLabels();
            HideParamViewers();
            ShowParam(valueData);

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

        /// <summary>
        /// パラメータを表示します。
        /// </summary>
        /// <param name="valueData"></param>
        private void ShowParam(ICbValue valueData)
        {
            if (!valueData.IsNull && valueData is ICbValueEnum selectValue)
            {
                // enum型を表示する

                ShowEnumTypeParamViewer(valueData, selectValue);
                return;
            }

            if (valueData is ICbClass cbClass)
            {
                if (!cbClass.IsNull)
                {
                    object cbClassData = cbClass.Data;
                    if (cbClassData != null)
                    {
                        if (cbClassData is ImageSource imageSource)
                        {
                            // イメージを表示する

                            ShowImageSourceTypeParamViewer(imageSource);
                            return;
                        }
                        if (cbClassData is Image image)
                        {
                            // イメージを表示する

                            ShowImageTypeParamViewer(image);
                            return;
                        }
                        if (cbClassData is MediaPlayer mediaPlayer)
                        {
                            // MediaPlayerイメージを表示する

                            ShowMediaPlayerTypeParamViewer(mediaPlayer);
                            return;
                        }
                    }
                }
            }

            if (valueData is CbImagePath cbImagePath)
            {
                // イメージを表示する

                if (ShowImagePathTypeParamViewer(cbImagePath))
                {
                    return;
                }
            }

            Edit.Visibility = Visibility.Visible;
            if (valueData.ValueUIString != null)
            {
                ParamEdit = valueData.ValueUIString;
            }
            ToolTipUpdate();    // 必ず更新確認が必要
            if (valueData is CbText cbText)
            {
                // cbText型の場合は、編集領域を広げる

                ShowTextTypeParamEdit();
            }
            else
            {
                // 通常の編集領域

                ShowOthersTypeParamEdit();
            }
            Edit.IsReadOnly = valueData.IsReadOnlyValue || ReadOnly || valueData.IsNull;
            if (!Edit.IsReadOnly)
            {
                Edit.Background = (Brush)Application.Current.FindResource("ParamBackgroundBrush");
            }
            else
            {
                Edit.Background = (Brush)Application.Current.FindResource("ReadOnlyParamBackgroundBrush");
            }
        }

        /// <summary>
        /// パラメータ名及び型名を表示します。
        /// </summary>
        private void ShowNameLabels()
        {
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
        }

        /// <summary>
        /// 一般的なパラメータ表示を行います。
        /// </summary>
        private void ShowOthersTypeParamEdit()
        {
            Edit.MaxHeight = 36;
        }

        /// <summary>
        /// ImageSource型のパラメータを表示します。
        /// </summary>
        /// <param name="imageSource"></param>
        private void ShowImageSourceTypeParamViewer(ImageSource imageSource)
        {
            ImageBox.Source = imageSource;
            ImagePanel.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Image型のパラメータを表示します。
        /// </summary>
        /// <param name="image"></param>
        private void ShowImageTypeParamViewer(Image image)
        {
            ImageBox = image;
            ImagePanel.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// ImagePath型のパラメータを表示します。
        /// </summary>
        /// <param name="imagePath"></param>
        private bool ShowImagePathTypeParamViewer(CbImagePath cbImagePath)
        {
            if (!System.IO.File.Exists(cbImagePath.Value))
            {
                return false;
            }
            ImageBox.ToolTip = cbImagePath.Value;
            ImageBox.Source = new BitmapImage(new Uri(cbImagePath.Value));
            ImageBox.Width = 240;
            ImageBox.Height = 240;
            ImagePanel.Visibility = Visibility.Visible;
            return true;
        }

        /// <summary>
        /// MediaPlayer型のパラメータを表示します。
        /// </summary>
        /// <param name="mediaPlayer"></param>
        private void ShowMediaPlayerTypeParamViewer(MediaPlayer mediaPlayer)
        {
            MediaPlayer image = mediaPlayer;
            MediaBox.Source = image.Source;
            MediaBox.LoadedBehavior = MediaState.Stop;
            MediaBox.Visibility = Visibility.Visible;
            MediaPanel.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Text型のパラメータを表示します。
        /// </summary>
        private void ShowTextTypeParamEdit()
        {
            Edit.ToolTip = null;
            Edit.MaxWidth = Edit.MinWidth = Edit.Width = 360;
            Edit.MaxHeight = Edit.MinHeight = Edit.Height = 300;
            Edit.AcceptsReturn = true;
            Edit.AcceptsTab = true;
            Edit.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            Edit.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            //Edit.TextWrapping = TextWrapping.Wrap;
        }

        /// <summary>
        /// enum型のパラメータを表示します。
        /// </summary>
        /// <param name="valueData"></param>
        /// <param name="selectValue"></param>
        private void ShowEnumTypeParamViewer(ICbValue valueData, ICbValueEnum selectValue)
        {
            Select.SelectionChanged -= Select_SelectionChanged; // ユーザー操作として扱わないようにする
            try
            {
                Select.Visibility = Visibility.Visible;
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
                    Select.Foreground = (Brush)Application.Current.FindResource("ReadOnlyEnumParamForegroundBrush");
                }
                else
                {
                    Select.Foreground = (Brush)Application.Current.FindResource("EnumParamForegroundBrush");
                }
            }
            finally
            {
                Select.SelectionChanged += Select_SelectionChanged;
            }
        }

        /// <summary>
        /// パラメータービュアー類を非表示にします。
        /// </summary>
        private void HideParamViewers()
        {
            Edit.Visibility = Visibility.Collapsed;
            Select.Visibility = Visibility.Collapsed;
            ImagePanel.Visibility = Visibility.Collapsed;
            MediaPanel.Visibility = Visibility.Collapsed;
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
                    self.TypeNameLabel.Foreground = value ?
                        (Brush)Application.Current.FindResource("CaseParamForegroundBrush")
                        : 
                        (Brush)Application.Current.FindResource("ParamForegroundBrush");
                });

        public static readonly DependencyProperty impCastTypeProperty = impCastType.Regist(false);

        public bool CastType
        {
            get { return impCastType.GetValue(this); }
            set { impCastType.SetValue(this, value); }
        }
        #endregion

        #region Fill 添付プロパティ実装
        private static ImplementDependencyProperty<UIParam, Brush> impFill =
            new ImplementDependencyProperty<UIParam, Brush>(
                nameof(Fill),
                (self, getValue) =>
                {
                    Brush value = getValue(self);
                    self.TypePanel.Fill = value;
                });

        public static readonly DependencyProperty impFillProperty = impFill.Regist((Brush)Application.Current.FindResource("ParamTypeBackgroundBrush"));

        public Brush Fill
        {
            get { return impFill.GetValue(this); }
            set { impFill.SetValue(this, value); }
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
                {
                    _OwnerCommandCanvas = value;
                }
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
