using Microsoft.Win32;
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
using System.Windows.Shapes;
using System.Windows.Threading;

namespace CbVS.Controls
{
    /// <summary>
    /// ImageWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MediaWindow : Window
    {
        BitmapSource imageData = null;

        public MediaWindow()
        {
            InitializeComponent();
        }

        string caption = "";

        public string Caption
        {
            get => caption;
            set
            {
                caption = value;
                Title = "MediaWindow";
                if (caption != "")
                {
                    Title += " - " + caption;
                }
            }
        }

        public BitmapSource ImageSource
        {
            set
            {
                if (value is null)
                    return;

                Width = value.PixelWidth;
                Height = value.PixelHeight;
                imageData = value.CloneCurrentValue();
                ImageBox.Source = imageData;
                //ImageBox.Source = value;

                ImageBoxS.Visibility = Visibility.Visible;
                toggleUniform.Visibility = Visibility.Visible;
                SaveButton.Visibility = Visibility.Visible;
                Dispatcher.BeginInvoke(
                   new Action(() =>
                   {
                       if (Width > 1600)
                           Width = 1600;
                       if (Height > 900)
                           Height = 900;
                   }
                   ), DispatcherPriority.Loaded);
            }
        }

        public MediaPlayer MediaSource
        {
            set
            {
                if (value is null)
                    return;

                Width = 640;
                Height = 480;
                MediaPlayer media = (MediaPlayer)(value as MediaPlayer).CloneCurrentValue();
                //MediaPlayer media = (MediaPlayer)(value as MediaPlayer);
                MediaBox.Source = media.Source;
                MediaBox.Visibility = Visibility.Visible;
            }
        }

        private void toggleUniform_Click(object sender, RoutedEventArgs e)
        {
            if (ImageBox.Stretch == Stretch.Uniform)
            {
                ImageBox.Stretch = Stretch.None;
                ImageBoxS.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;
            }
            else
            {
                ImageBox.Stretch = Stretch.Uniform;
                ImageBoxS.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            string path = ShowSaveDialog();

            if (path is null)
                return;

            CbVS.Script.Lib.Image.OutImageFile(imageData, path);
        }

        /// <summary>
        /// 保存用ファイル選択ダイアログを表示します。
        /// </summary>
        /// <returns></returns>
        public static string ShowSaveDialog()
        {
            var dialog = new SaveFileDialog();

            // ファイルの種類を設定
            dialog.Filter = "イメージファイル (*.bmp)|*.bmp|(*.jpeg)|*.jpeg|(*.png)|*.png|(*.gif)|*.gif|(*.tiff)|*.tiff";

            // ダイアログを表示する
            if (dialog.ShowDialog() == true)
            {
                return dialog.FileName;
            }
            return null;
        }
    }
}
