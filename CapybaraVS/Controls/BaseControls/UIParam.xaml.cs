//#define SHOW_LINK_ARRAY   // リスト型を接続したときにリストの要素をコピーして表示する

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
using System.Windows.Threading;
using System.Xml.Serialization;

namespace CapybaraVS.Controls.BaseControls
{
    /// <summary>
    /// UIParam.xaml の相互作用ロジック
    /// </summary>
    public partial class UIParam : UserControl
    {
        #region XML定義
        [XmlRoot(nameof(UIParam))]
        public class _AssetXML<OwnerClass>
            where OwnerClass : UIParam
        {
            [XmlIgnore]
            public Action WriteAction = null;
            [XmlIgnore]
            public Action<OwnerClass> ReadAction = null;
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
                    string backup = self.ValueData.ValueString;
                    try
                    {
                        if (self.ValueData is CbObject cbObject)
                        {
                            // CbObject は中身を表示する

                            self.Edit.Text = cbObject.ValueTypeObject.ValueString.Trim('\r', '\n');
                        }
                        if (self.ValueData is ICbClass cbClass && cbClass.Data != null)
                        {
                            // ICbClass は中身の型を表示する

                            self.Edit.Text = $"[{CbSTUtils._GetTypeName(cbClass.Data.GetType())}]";
                        }
                        else
                        {
                            if (self.ValueData.IsStringableValue)
                                self.ValueData.ValueString = text;
                            self.Edit.Text = self.ValueData.ValueString.Trim('\r', '\n');
                        }
                        self.ToolTipUpdate();
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine(ex.Message);

                        self.Edit.Text = backup;
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
                    self.TypeNameLabel.Content = getValue(self);
                    if (self.TypeNameLabelOverlap.Length != 0 && !(self.ValueData is ParamNameOnly))
                        self.TypeNameLabel.Content = self.TypeNameLabelOverlap;
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
                        self.ParamNameLabel.LabelString = self.ParamNameLabelOverlap;
                    if (self.ValueData is ICbValue value)
                    {
                        value.Name = text;
                        self.ParamEdit = value.ValueString;
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
                        self.UpdateValueData(valueData);
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

            if (CommandCanvas.UIParamHoldAction.Enabled)
            {
                // 画面反映はあとから一括で行う

                CommandCanvas.UIParamHoldAction.Add(this, () => UpdateValueData(valueData));
                return;
            }

            _UpdateValueData(valueData);
        }

        private void _UpdateValueData(ICbValue valueData)
        {
            string typeName = CbSTUtils._GetTypeName(valueData.OriginalType);
            if (valueData is CbObject cbObject)
            {
                // CbObject は中身を表示する

                _UpdateValueData(cbObject.ValueTypeObject, typeName);
                return;
            }

            _UpdateValueData(valueData, typeName);
        }

        private void _UpdateValueData(ICbValue valueData, string typeName)
        {
            if (valueData is null)
                return;

            if (typeName.Length != 0)
                TypeName = typeName;
            if (valueData.Name.Length != 0)
                ParamName = valueData.Name;
            TypeNameLabel.Visibility = (typeName.Length != 0 ? Visibility.Visible : Visibility.Collapsed);
            ParamNameLabel.Visibility = (valueData.Name.Length != 0 ? Visibility.Visible : Visibility.Collapsed);
            if (ParamNameLabelOverlap.Length != 0)
                ParamNameLabel.Visibility = Visibility.Visible;

            Edit.Visibility = Visibility.Collapsed;
            Select.Visibility = Visibility.Collapsed;
            ImagePanel.Visibility = Visibility.Collapsed;
            MediaPanel.Visibility = Visibility.Collapsed;

            // ※ valueData.Data が null でも表示は必要
            if (valueData is ICbValueEnum selectValue)
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
                    //var image = (valueData.Data as BitmapSource).Clone();
                    var image = (valueData.Data as BitmapSource);

                    ImageBox.Source = image;
                }

                ImagePanel.Visibility = Visibility.Visible;
            }
            else if (valueData is ICbClass cbClass3 && cbClass3.OriginalReturnType == typeof(MediaPlayer))
            {
                // MediaPlayerイメージを表示する

                if (valueData.Data != null)
                {
                    MediaPlayer image = (MediaPlayer)(valueData.Data as MediaPlayer);
                    //MediaPlayer image = (MediaPlayer)(valueData.Data as MediaPlayer).Clone();
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

                if (valueData.ValueString != null)
                {
                    ParamEdit = valueData.ValueString;
                }
                ToolTipUpdate();    // 必ず更新確認が必要

                Edit.IsReadOnly = valueData.IsReadOnlyValue || ReadOnly || valueData.IsNull;
                if (Edit.IsReadOnly)
                    Edit.Background = Brushes.Lavender;
                else
                    Edit.Background = Brushes.White;
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
                if (node == valueData.ValueString ||
                    selectValue.TypeName + "." + node == valueData.ValueString)
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
                Select.Foreground = Brushes.DarkGray;
            else
                Select.Foreground = Brushes.Black;
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
                    self.Edit.IsReadOnly = self.ValueData.IsReadOnlyValue || value;
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
            Edit.LostFocus += (sender, e) =>
            {
                ExitEditMode();
            };
        }

        private void ToolTipUpdate()
        {
            if (ValueData is CbObject cbObject)
            {
                if (!cbObject.ValueTypeObject.IsNull && cbObject.ValueTypeObject.Data is ICbShowValue cbVSShow)
                {
                    Edit.ToolTip = cbVSShow.DataString.Trim('\r', '\n');
                    return;
                }
            }
            if (ValueData is ICbClass cbClass)
            {
                if (cbClass.Data != null)
                {
                    if (cbClass.Data is ICbShowValue cbVSShow)
                    {
                        Edit.ToolTip = cbVSShow.DataString.Trim('\r', '\n');
                    }
                    else
                    {
                        Edit.ToolTip = cbClass.Data.ToString().Trim('\r', '\n');
                        return;
                    }
                }
            }
#if !SHOW_LINK_ARRAY
            else if (ValueData is ICbList cbList)
            {
                if (!cbList.IsNull && cbList is ICbShowValue cbVSShow)
                {
                    Edit.ToolTip = cbVSShow.DataString.Trim('\r', '\n');
                    return;
                }
            }
#endif
            else
            {
                if (!ValueData.IsNull && ValueData.Data is ICbShowValue cbVSShow)
                {
                    Edit.ToolTip = cbVSShow.DataString.Trim('\r', '\n');
                    return;
                }
            }
            Edit.ToolTip = Edit.Text.Trim('\r', '\n');
        }

        private void Edit_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ExitEditMode();
            }
        }

        private void ExitEditMode()
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

            ValueData.ValueString = selectedItem as string;
            UpdateEvent?.Invoke();
        }

        private void MediaBox_MouseEnter(object sender, MouseEventArgs e)
        {
            MediaBox.Position = TimeSpan.Zero;
            //MediaBox.Visibility = Visibility.Visible;
            MediaBox.LoadedBehavior = MediaState.Manual;
            MediaBox.Play();
        }

        private void MediaBox_MouseLeave(object sender, MouseEventArgs e)
        {
            MediaBox.Stop();
        }
    }
}
