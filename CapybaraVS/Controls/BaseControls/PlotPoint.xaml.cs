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

namespace CapyCSS.Controls.BaseControls
{
    /// <summary>
    /// PlotPoint.xaml の相互作用ロジック
    /// </summary>
    public partial class PlotPoint : UserControl
    {
        public PlotPoint()
        {
            InitializeComponent();
        }

        public enum PlotType
        {
            None,
            Type1,
            Type2
        }
        public void SetPlotType(PlotType type, SolidColorBrush solidColorBrush)
        {
            double size = 10;
            switch (type)
            {
                case PlotType.None:
                    Plot.Fill = Brushes.Transparent;
                    Plot.Stroke = null;
                    size = 10;
                    break;

                case PlotType.Type1:
                    {
                        if (solidColorBrush is null)
                        {
                            Plot.Fill = new SolidColorBrush(Color.FromArgb(0xff, 0xb2, 0xb2, 0xb2));
                        }
                        else
                        {
                            Plot.Fill = solidColorBrush;
                        }
                        Plot.Stroke = new SolidColorBrush(Color.FromArgb(0xff, 0x5f, 0x5f, 0x5f));
                        size = 10;
                    }
                    break;

                case PlotType.Type2:
                    {
                        if (solidColorBrush is null)
                        {
                            Plot.Fill = new SolidColorBrush(Color.FromArgb(0xff, 0xb2, 0x62, 0x62));
                        }
                        else
                        {
                            Plot.Fill = solidColorBrush;
                        }
                        Plot.Stroke = new SolidColorBrush(Color.FromArgb(0xff, 0x5f, 0x1f, 0x1f));
                        size = 4;
                    }
                    break;
            }
            Plot.Width = size;
            Plot.Height = size;
            Plot.Margin = new Thickness(-(size / 2), -(size / 2), 0, 0);
        }
    }
}
