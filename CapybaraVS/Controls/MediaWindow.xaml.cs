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
using static CbVS.Script.Lib.Media;

namespace CbVS.Controls
{
    /// <summary>
    /// ImageWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MediaWindow : Window
    {
        BitmapSource imageData = null;

        public MediaOption MediaOption = null;

        public MediaWindow()
        {
            InitializeComponent();
            KeyUp += (o, e) => { if (e.Key == Key.Escape) Close(); };
            MouseLeftButtonDown += (o, e) => DragMove();
            ImageBox.MouseLeftButtonDown += (o, e) => DragMove();
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

                Width = value.PixelWidth + SystemParameters.VerticalScrollBarWidth;
                Height = value.PixelHeight + SystemParameters.VerticalScrollBarWidth;
                imageData = value.CloneCurrentValue();
                ImageBox.Source = imageData;

                ImageBoxS.Visibility = Visibility.Visible;
                toggleUniform.Visibility = Visibility.Visible;
                SaveButton.Visibility = Visibility.Visible;
                Dispatcher.BeginInvoke(
                   new Action(() =>
                   {
                       if (MediaOption != null)
                       {
                           Width = MediaOption.width;
                           Height = MediaOption.height;
                           ImageBox.Width = MediaOption.width * MediaOption.scale;
                           ImageBox.Height = MediaOption.height * MediaOption.scale;
                           ImageBox.Margin = new Thickness(-MediaOption.offsetX, -MediaOption.offsetY, 0, 0);

                           setupWindowPosition();
                       }
                       else
                       {
                           if (Width > 1600)
                               Width = 1600;
                           if (Height > 900)
                               Height = 900;
                       }
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

                MediaPlayer media = (MediaPlayer)(value as MediaPlayer).CloneCurrentValue();
                MediaBox.Source = media.Source;
                MediaBox.Visibility = Visibility.Visible;
                MediaBox.LoadedBehavior = MediaState.Manual;
                if (MediaOption != null && MediaOption.display != Display.None)
                {
                    WindowStyle = WindowStyle.None;
                    AllowsTransparency = true;

                    if (MediaOption.repeat)
                    {
                        // リピート再生制御を追加する

                        MediaBox.MediaEnded += (s, e) =>
                        {
                            MediaBox.Stop();
                            MediaBox.Play();
                        };
                    }
                }

                Dispatcher.BeginInvoke(
                   new Action(() =>
                   {
                       if (MediaOption != null)
                       {
                           Width = MediaOption.width;
                           Height = MediaOption.height;
                           double sw = media.NaturalVideoWidth * MediaOption.scale;
                           double sh = media.NaturalVideoHeight * MediaOption.scale;
                           MediaBox.Margin = new Thickness(
                               -MediaOption.offsetX - (sw - media.NaturalVideoWidth) / 2.0,
                               -MediaOption.offsetY - (sh - media.NaturalVideoHeight) / 2.0,
                               0,
                               0);
                           MediaBox.Width = sw;
                           MediaBox.Height = sh;

                           setupWindowPosition();
                       }
                       else
                       {
                           Width = media.NaturalVideoWidth;
                           Height = media.NaturalVideoHeight;
                       }
                       MediaBox.Play();
                   }
                   ), DispatcherPriority.Loaded);
            }
        }

        private void setupWindowPosition()
        {
            switch (MediaOption.display)
            {
                case Display.None:
                    Left += MediaOption.posX;
                    Top += MediaOption.posY;
                    break;
                case Display.Primary:
                    Left = MediaOption.posX;
                    Top = MediaOption.posY;
                    Width = Math.Min(Width, SystemParameters.PrimaryScreenWidth - MediaOption.posX);
                    break;
                case Display.Secondary:
                    Left = MediaOption.posX;
                    Top = MediaOption.posY;
                    Left += SystemParameters.PrimaryScreenWidth;
                    Width = Math.Min(Width, SystemParameters.VirtualScreenWidth - SystemParameters.PrimaryScreenWidth - MediaOption.posX);
                    break;
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
