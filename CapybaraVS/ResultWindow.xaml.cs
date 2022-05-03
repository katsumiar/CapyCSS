using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MahApps.Metro.Controls;

namespace CapyCSS
{
    /// <summary>
    /// ResultWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class ResultWindow : MetroWindow
    {
        public ResultWindow(string result)
        {
            InitializeComponent();
            DateTime dt = DateTime.Now;
            Title = "Result - " + dt.ToString("yyyy/MM/dd HH:mm:ss");
            ResultText.Text = result;
        }
    }
}
