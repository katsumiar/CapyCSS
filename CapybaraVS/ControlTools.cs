using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace CapybaraVS
{
    /// <summary>
    /// 添付プロパティ実装クラス
    /// 
    /// 実装例）
    /// 
    ///    #region XXX プロパティ実装
    ///
    ///    private static ImplementDependencyProperty<クラス名, プロパティの型> impXXX =
    ///        new ImplementDependencyProperty<クラス名, プロパティの型>(
    ///            nameof(XXX),
    ///            (self, getValue) =>
    ///            {
    ///                self.EditControl.Text = getValue(self);
    ///                self.LabelControl.Content = getValue(self);
    ///            });
    ///
    ///    public static readonly DependencyProperty XXXProperty = impXXX.Regist();
    ///
    ///    public プロパティの型 XXX
    ///    {
    ///        get { return impXXX.GetValue(this); }
    ///        set { impXXX.SetValue(this, value); }
    ///    }
    ///
    ///    #endregion
    /// 
    /// </summary>
    /// <typeparam name="ClassType">実装するクラスの型</typeparam>
    /// <typeparam name="ValueType">実装するプロパティの型</typeparam>
    public class ImplementDependencyProperty<ClassType, ValueType>
        where ClassType : DispatcherObject
    {
        private DependencyProperty property = null;
        private string _valueName;
        private Action<ClassType, Func<UserControl, ValueType>> _func;

        public ImplementDependencyProperty(string valueName, Action<ClassType, Func<UserControl, ValueType>> func)
        {
            _valueName = valueName;
            _func = func;
        }

        public DependencyProperty Regist(ValueType defaultValue)
        {
            property = DependencyProperty.Register(
                                        _valueName,
                                        typeof(ValueType),
                                        typeof(ClassType),
                                        new FrameworkPropertyMetadata(defaultValue, (obj, e) =>
                                        {
                                            ClassType ctrl = obj as ClassType;
                                            if (ctrl != null)
                                            {
                                                _func(ctrl, GetValue);
                                            }
                                        }));
            return property;
        }

        public ValueType GetValue(UserControl self)
        {
            return (ValueType)self.GetValue(property);
        }
        public void SetValue(UserControl self, ValueType value)
        {
            self.SetValue(property, value);
        }
    }

    /// <summary>
    /// ImplementDependencyProperty のウインドウコントロール版
    /// ※手抜きでジェネリック化していない……
    /// </summary>
    /// <typeparam name="ClassType">実装するクラスの型</typeparam>
    /// <typeparam name="ValueType">実装するプロパティの型</typeparam>
    public class ImplementWindowDependencyProperty<ClassType, ValueType>
        where ClassType : DispatcherObject
    {
        private DependencyProperty property = null;
        private string _valueName;
        private Action<ClassType, Func<Window, ValueType>> _func;

        public ImplementWindowDependencyProperty(string valueName, Action<ClassType, Func<Window, ValueType>> func)
        {
            _valueName = valueName;
            _func = func;
        }

        public DependencyProperty Regist(ValueType defaultValue)
        {
            property = DependencyProperty.Register(
                                        _valueName,
                                        typeof(ValueType),
                                        typeof(ClassType),
                                        new FrameworkPropertyMetadata(defaultValue, (obj, e) =>
                                        {
                                            ClassType ctrl = obj as ClassType;
                                            if (ctrl != null)
                                            {
                                                _func(ctrl, GetValue);
                                            }
                                        }));
            return property;
        }

        public ValueType GetValue(Window self)
        {
            return (ValueType)self.GetValue(property);
        }
        public void SetValue(Window self, ValueType value)
        {
            self.SetValue(property, value);
        }
    }


    public class ControlTools
    {
        public static T FindAncestor<T>(DependencyObject findObject) where T : class
        {
            var target = findObject;
            try
            {
                do
                {
                    target = System.Windows.Media.VisualTreeHelper.GetParent(target);

                } while (target != null && !(target is T));

                return target as T;
            }
            finally
            {
                target = null;
                findObject = null;
            }
        }
    }
}
