using CapybaraVS;
using CapybaraVS.Script;
using CbVS.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using static CbVS.Script.Lib.Image;

namespace CbVS.Script.Lib
{
    class Media
    {
        const string nameSpace = "Graphics.Media";

        //------------------------------------------------------------------
        [ScriptMethod(nameSpace + "." + "OutMediaWindow", "",
            "RS=>Image_OutMediaWindow"
            )]
        public static void OutMediaWindow(string title, MediaPlayer media)
        {
            if (media is null)
            {
                return;
            }
            var window = new MediaWindow();
            window.Owner = MainWindow.Instance;
            window.MediaSource = media;
            window.Caption = title;
            window.Show();
        }

        //------------------------------------------------------------------
        [ScriptMethod(nameSpace + "." + nameof(OpenMedia), "",
            "RS=>Image_OpenMedia"
            )]
        public static MediaPlayer OpenMedia(string fileName)
        {
            if (File.Exists(fileName))
            {
                var media = new MediaPlayer();
                media.Open(new Uri(fileName));
                media.ScrubbingEnabled = true;
                media.Pause();
                return media;
            }
            return null;
        }

        //------------------------------------------------------------------
        [ScriptMethod(nameSpace + "." + nameof(Seek), nameof(Seek),
            "RS=>Media_Seek"
            )]
        public static MediaPlayer Seek(MediaPlayer player, double seconds)
        {
            if (player is null)
            {
                return null;
            }

            // 指定位置へシーク
            player = (MediaPlayer)player.CloneCurrentValue();

            double volume = player.Volume;
            player.Volume = 0;

            player.Pause();
            player.Position = TimeSpan.FromSeconds(seconds);

            // 読み込みが完了するまで待機
            waitMediaSetup(player, seconds);

            player.Volume = volume;

            return player;
        }

        //------------------------------------------------------------------
        [ScriptMethod(nameSpace + "." + nameof(Captures), nameof(Captures),
           "RS=>Media_Capture_all"
           )]
        public static BitmapImage Captures(
            MediaPlayer player,
            double waitSeconds = 10,
            int maxColumns = 6,
            double maxWidth = 320,
            CRGBA3x3FilteringProc filtering = null,
            int dpi = Image.DEFAULT_dpi
            )
        {
            if (player is null)
            {
                return null;
            }

            // 読み込みが完了するまで待機
            waitMediaSetup(player);

            if (!player.NaturalDuration.HasTimeSpan)
            {
                return null;
            }

            return Captures(
                player,
                30,
                waitSeconds,
                (int)(player.NaturalDuration.TimeSpan.TotalSeconds / waitSeconds),
                maxColumns,
                maxWidth,
                filtering,
                dpi
                );
        }

        //------------------------------------------------------------------
        [ScriptMethod(nameSpace + "." + nameof(Captures) + "(double, double)", nameof(Captures),
            "RS=>Media_Capture_all_d_d"
            )]
        public static BitmapImage Captures(
            MediaPlayer player,
            double startSeconds = 0,
            double waitSeconds = 10,
            int count = 10,
            int maxColumns = 6,
            double maxWidth = 320,
            CRGBA3x3FilteringProc filtering = null,
            int dpi = Image.DEFAULT_dpi)
        {
            if (player is null)
            {
                return null;
            }

            // 読み込みが完了するまで待機
            waitMediaSetup(player);

            int width, height;
            // リサイズ後のサイズを計算
            calcRectSize(player, maxWidth, out width, out height);

            int widthSize = width * maxColumns;
            int heightCount = count / maxColumns;
            heightCount = (heightCount == 0) ? 1 : heightCount;
            int heightSize = height * heightCount;

            var renderTarget = new RenderTargetBitmap(widthSize, heightSize, dpi, dpi, PixelFormats.Pbgra32);

            double volume = player.Volume;
            player.Volume = 0;

            for (int i = 0; i < count; ++i)
            {
                // 指定位置へシーク
                double pos = startSeconds + (double)i * waitSeconds;
                player.Position = TimeSpan.FromSeconds(pos);
                player.Pause();

                // 読み込みが完了するまで待機
                waitMediaSetup(player, pos);

                capture(
                    renderTarget, 
                    player, 
                    (i % maxColumns) * width, 
                    (i / maxColumns) * height, 
                    width, 
                    height, 
                    filtering, 
                    dpi
                    );
            }

            player.Volume = volume;

            return Image.ConvertToBitmapImage(renderTarget, Image.ImageType.PNG);
        }

        //------------------------------------------------------------------
        [ScriptMethod(nameSpace + "." + nameof(CaptureOutGifAnimation), nameof(CaptureOutGifAnimation),
           "RS=>Media_CaptureOutGifAnimation"
           )]
        public static void CaptureOutGifAnimation(
            string path,
            MediaPlayer player,
            double waitSeconds = 0.1,
            double maxWidth = 640,
            CRGBA3x3FilteringProc filtering = null,
            int dpi = Image.DEFAULT_dpi
            )
        {
            if (player is null)
            {
                return;
            }

            // 読み込みが完了するまで待機
            waitMediaSetup(player);

            if (!player.NaturalDuration.HasTimeSpan)
            {
                return;
            }

            CaptureOutGifAnimation(
                path,
                player,
                30,
                waitSeconds,
                (int)(player.NaturalDuration.TimeSpan.TotalSeconds / waitSeconds),
                maxWidth,
                filtering,
                dpi
                );
        }

        //------------------------------------------------------------------
        [ScriptMethod(nameSpace + "." + nameof(CaptureOutGifAnimation) + "(double, double)", nameof(CaptureOutGifAnimation),
            "RS=>Media_CaptureOutGifAnimation"
            )]
        public static void CaptureOutGifAnimation(
            string path,
            MediaPlayer player,
            double startSeconds = 0,
            double waitSeconds = 0.1,
            int count = 10,
            double maxWidth = 640,
            CRGBA3x3FilteringProc filtering = null,
            int dpi = Image.DEFAULT_dpi)
        {
            if (player is null)
            {
                return;
            }

            // 読み込みが完了するまで待機
            waitMediaSetup(player);

            int width, height;
            // リサイズ後のサイズを計算
            calcRectSize(player, maxWidth, out width, out height);

            double volume = player.Volume;
            player.Volume = 0;

            var gEnc = new GifBitmapEncoder();

            for (int i = 0; i < count; ++i)
            {
                // 指定位置へシーク
                double pos = startSeconds + (double)i * waitSeconds;
                player.Position = TimeSpan.FromSeconds(pos);
                player.Pause();

                // 読み込みが完了するまで待機
                waitMediaSetup(player, pos);

                var renderTarget = new RenderTargetBitmap(width, height, dpi, dpi, PixelFormats.Pbgra32);

                capture(
                    renderTarget,
                    player,
                    width,
                    height,
                    filtering,
                    dpi
                    );

                gEnc.Frames.Add(BitmapFrame.Create(renderTarget));
            }

            player.Volume = volume;

            using (FileStream fs = new FileStream(path, FileMode.Create))
            {
                gEnc.Save(fs);
            }
        }

        //------------------------------------------------------------------
        [ScriptMethod(nameSpace + "." + nameof(Capture) + "(MediaPlayer, double)", nameof(Capture),
            "RS=>Media_Capture_cm_d"
            )]
        public static BitmapImage Capture(
            MediaPlayer player, 
            double seconds, 
            double maxWidth = 640,
            CRGBA3x3FilteringProc filtering = null,
            int dpi = Image.DEFAULT_dpi
            )
        {
            if (player is null)
            {
                return null;
            }

            // 指定位置へシーク
            player = (MediaPlayer)player.CloneCurrentValue();

            double volume = player.Volume;
            player.Volume = 0;

            player.Position = TimeSpan.FromSeconds(seconds);
            player.Pause();

            // 読み込みが完了するまで待機
            waitMediaSetup(player, seconds);

            player.Volume = volume;

            return capture(player, maxWidth, filtering, dpi);
        }

        //------------------------------------------------------------------
        [ScriptMethod(nameSpace + "." + nameof(Capture) + "(MediaPlayer)", nameof(Capture),
           "RS=>Media_Capture_cm"
           )]
        public static BitmapImage Capture(
            MediaPlayer player, 
            double maxWidth = 640,
            CRGBA3x3FilteringProc filtering = null,
            int dpi = Image.DEFAULT_dpi)
        {
            double volume = player.Volume;
            player.Volume = 0;

            player.Pause();

            player.Volume = volume;

            // 読み込みが完了するまで待機
            waitMediaSetup(player);

            return capture(player, maxWidth, filtering, dpi);
        }

        //------------------------------------------------------------------
        /// <summary>
        /// 準備が完了するまで待機します。
        /// </summary>
        /// <param name="player"></param>
        private static void waitMediaSetup(
            MediaPlayer player, 
            double? pos = null)
        {
            if (pos != null)
            {
                int retryCount = 0;

                while (
                    player.DownloadProgress < 1.0 ||
                    player.NaturalVideoWidth == 0 ||
                    player.NaturalVideoHeight == 0 ||
                    player.IsBuffering ||
                    !(Math.Abs(player.Position.TotalSeconds - pos.Value) < 0.5)    // 浮動小数点の誤差を超える
                    )
                {
                    if (retryCount++ > 3)
                        break;  // 状態を満たせないことがある…
                    Thread.Sleep(100);
                }
                Thread.Sleep(200);   // 最低限スリープしないとうまく動かないようだ…
            }
            else
            {
                do
                {
                    Thread.Sleep(100);
                }
                while (
                    player.DownloadProgress < 1.0 ||
                    player.NaturalVideoWidth == 0 ||
                    player.NaturalVideoHeight == 0 ||
                    player.IsBuffering
                    );
            }
        }

        //------------------------------------------------------------------
        private static BitmapImage capture(
            MediaPlayer player, 
            double maxWidth,
            CRGBA3x3FilteringProc filtering = null,
            int dpi = DEFAULT_dpi
            )
        {
            int width, height;
            // リサイズ後のサイズを計算
            calcRectSize(player, maxWidth, out width, out height);

            var renderTarget = new RenderTargetBitmap(width, height, dpi, dpi, PixelFormats.Pbgra32);
            capture(renderTarget, player, width, height, filtering, dpi);

            return Image.ConvertToBitmapImage(renderTarget, Image.ImageType.PNG);
        }

        private static void capture(
            RenderTargetBitmap renderTarget,
            MediaPlayer player,
            double width,
            double height,
            CRGBA3x3FilteringProc filtering = null,
            int dpi = DEFAULT_dpi
            )
        {
            capture(renderTarget, player, 0, 0, width, height, filtering, dpi);
        }

        private static void capture(
            RenderTargetBitmap renderTarget,
            MediaPlayer player,
            double x,
            double y,
            double width,
            double height,
            CRGBA3x3FilteringProc filtering = null,
            int dpi = DEFAULT_dpi
            )
        {
            if (filtering != null)
            {
                var tempRenderTarget = new RenderTargetBitmap((int)width, (int)height, dpi, dpi, PixelFormats.Pbgra32);

                // 動画の映像をレンダリングする
                var visualForVideo = new DrawingVisual();
                using (var context = visualForVideo.RenderOpen())
                {
                    context.DrawVideo(player, new System.Windows.Rect(0, 0, width, height));
                }
                // ビットマップに visualForVideo をレンダリング
                tempRenderTarget.Render(visualForVideo);

                // フィルタリングを通す
                BitmapImage bitmap = RGBA3x3FilteringProc(
                    tempRenderTarget,
                    filtering,
                    (filtering is null) ? true : filtering.useThread,
                    dpi);

                // レンダリングされた動画のイメージを再配置する
                var visualForImage = new DrawingVisual();
                using (var context = visualForImage.RenderOpen())
                {
                    context.DrawImage(bitmap, new System.Windows.Rect(x, y, width, height));
                }
                // ビットマップに visualForImage をレンダリング
                renderTarget.Render(visualForImage);
            }
            else
            {
                // 動画の映像をレンダリングする
                var visualForImage = new DrawingVisual();
                using (var context = visualForImage.RenderOpen())
                {
                    context.DrawVideo(player, new System.Windows.Rect(x, y, width, height));
                }
                // ビットマップに visualForImage をレンダリング
                renderTarget.Render(visualForImage);
            }
        }

        private static void calcRectSize(MediaPlayer player, double maxWidth, out int width, out int height)
        {
            double ratio = maxWidth / player.NaturalVideoWidth;

            width = (int)((double)player.NaturalVideoWidth * ratio);
            height = (int)((double)player.NaturalVideoHeight * ratio);
        }
    }
}
