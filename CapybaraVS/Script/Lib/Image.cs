using CapyCSS;
using CapyCSS.Script;
using CapyCSSattribute;
using CapyCSS.Script.Lib;
using CapyCSS.Controls;
using CbVS.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using static CapyCSS.Controls.PlotWindow;
using static CbVS.Script.Lib.Media;
using CapyCSSbase;

namespace CbVS.Script.Lib
{
    [ScriptClass]
    public static class Image
    {
        private const string LIB_NAME = "Graphics.Image";
        private const string LIB_NAME2 = LIB_NAME + ".Def";
        private const string nameSpace3 = LIB_NAME + ".Scaling";
        private const string nameSpace4 = LIB_NAME + ".Synthesis";
        private const string nameSpace5 = LIB_NAME + ".Info";
        private const string nameSpace6 = LIB_NAME + ".Lib";
        private const string nameSpace7 = LIB_NAME + ".Filter";
        private const string nameSpace8 = LIB_NAME + ".3x3Filter";

        public const int DEFAULT_dpi = 96;
        const double DEF_CRTGamma2dot2 = 2.2;
        const double DEF_GammaCollection2dot2 = 1.0 / DEF_CRTGamma2dot2;

        static int GetStride(int width, BitmapSource image)
        {
            return (width * 32) / 8;    // 32bit で作成する
            //return (width * image.Format.BitsPerPixel + 7) / 8;
        }

        [ScriptMethod(LIB_NAME2)]
        public static double GammaCollection2dot2()
        {
            return DEF_GammaCollection2dot2;
        }

        [ScriptMethod(LIB_NAME2)]
        public static double CRTGamma2dot2()
        {
            return DEF_CRTGamma2dot2;
        }

        /// <summary>
        /// 画像形式です。
        /// </summary>
        public enum ImageType
        {
            BMP,
            JPEG,
            PNG,
            TIFF,
            WMP,
            GIF,
        }

        [ScriptMethod(nameSpace3)]
        public static ImageType CreateImageType(ImageType type)
        {
            return type;
        }

        /// <summary>
        /// 色成分です。
        /// </summary>
        public enum ColorComponentType
        {
            // ビット並びなので並び替え禁止

            BLUE,
            GREEN,
            RED,
            ALPHA,
        }

        [ScriptMethod(nameSpace3)]
        public static ColorComponentType CreateColorComponentType(ColorComponentType type)
        {
            return type;
        }

        /// <summary>
        /// グレースケールの変換タイプです。
        /// </summary>
        public enum GlayScaleType
        {
            NONE,       // 何もしない
            AVERAGE,    // 画素値の平均
            BT_709,     // BT.709 色空間
            BT_601,     // BT.601 色空間
            YCgCo_Y,    // YCgCo(Y) 色空間
            ONLY_G,     // G だけ
        }

        [ScriptMethod(nameSpace3)]
        public static GlayScaleType CreateGlayScaleType(GlayScaleType type)
        {
            return type;
        }

        /// <summary>
        /// 空間フィルターの種類です。
        /// </summary>
        public enum K3x3FilterType
        {
            Average,            // 平均化フィルタ
            Gaussian,           // ガウシアンフィルタ
            FirstDerivative,    // 一次微分フィルタ
            Prewitt,            // プレヴィットフィルタ
            Sobel,              // ソーベルフィルタ
            Median,             // 中央値フィルタ
            Dilation,           // 膨張フィルタ
            Erosion,            // 収縮フィルタ
        }

        [ScriptMethod(nameSpace3)]
        public static K3x3FilterType Create3x3FilterType(K3x3FilterType type)
        {
            return type;
        }

        /// <summary>
        /// コンストラクトの種類です。
        /// </summary>
        public enum ContrastType
        {
            Linear,
            OutExp,
            InOutSine,
            OutCirc,
            OutSine,
            InCubic,
            InQuad,
            OutQuad,
        }

        [ScriptMethod(nameSpace3)]
        public static ContrastType CreateContrastType(ContrastType type)
        {
            return type;
        }

        /// <summary>
        /// RGB及び透過色に対してRGB及び透過色を個別に処理するためのインターフェイスです。
        /// </summary>
        public interface IRGBAFilter
        {
            /// <summary>
            /// 処理対象の画素値をセットします。
            /// </summary>
            /// <param name="r">赤色の画素値</param>
            /// <param name="g">緑色の画素値</param>
            /// <param name="b">青色の画素値</param>
            /// <param name="a">透過色の画素値</param>
            void CalcValueOfPixels(
                double r, double g, double b, double a,
                out double R, out double G, out double B, out double Alpha);
        }

        /// <summary>
        /// RGB 及び透過色の受け渡し用クラスです。
        /// </summary>
        public class CRGBA
        {
            public double R { get; set; } = 0;

            public double G { get; set; } = 0;

            public double B { get; set; } = 0;

            public double Alpha { get; set; } = 0;

            public void SetValueOfPixels(double r, double g, double b, double a)
            {
                R = r;
                G = g;
                B = b;
                Alpha = a;
            }
        }

        /// <summary>
        /// ３✕３の畳み込み処理用インターフェイスです。
        /// </summary>
        public interface IRGBA3x3Filter
        {
            /// <summary>
            /// 畳み込み対象の画素値をセットします。
            /// </summary>
            /// <param name="pixel00">入力(-1, -1)</param>
            /// <param name="pixel01">入力(-1,  0)</param>
            /// <param name="pixel02">入力(-1,  1)</param>
            /// <param name="pixel10">入力( 0, -1)</param>
            /// <param name="pixel11">入力( 0,  0)</param>
            /// <param name="pixel12">入力( 0,  1)</param>
            /// <param name="pixel20">入力( 1, -1)</param>
            /// <param name="pixel21">入力( 1,  0)</param>
            /// <param name="pixel22">入力( 1,  1)</param>
            void CalcValueOfPixels(
                CRGBA pixel00,
                CRGBA pixel01,
                CRGBA pixel02,
                CRGBA pixel10,
                CRGBA pixel11,
                CRGBA pixel12,
                CRGBA pixel20,
                CRGBA pixel21,
                CRGBA pixel22,
                out double R,
                out double G,
                out double B,
                out double Alpha
                );
        }

        //------------------------------------------------------------------
        [ScriptMethod(LIB_NAME)]
        public static void OutImageWindow(string title, BitmapSource image, MediaOption mediaOption = null)
        {
            if (image is null)
            {
                return;
            }
            var window = new MediaWindow();
            window.MediaOption = mediaOption;
            window.Owner = CommandCanvasList.OwnerWindow;
            window.ImageSource = image;
            window.Caption = title;
            window.Show();
        }

        //------------------------------------------------------------------
        [ScriptMethod(LIB_NAME)]
        public static BitmapImage OpenImage(string path)
        {
            if (File.Exists(path))
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(path);
                bitmap.EndInit();
                return bitmap;
            }
            return null;
        }

        //------------------------------------------------------------------
        [ScriptMethod(LIB_NAME)]
        public static void OutImageFile(BitmapSource image, string path)
        {
            string ext = Path.GetExtension(path);
            ImageType type = ImageType.BMP;
            switch (ext.ToLower())
            {
                case ".bmp":
                    type = ImageType.BMP;
                    break;

                case ".jpeg":
                case ".jpg":
                    type = ImageType.JPEG;
                    break;

                case ".png":
                    type = ImageType.PNG;
                    break;

                case ".tif":
                case ".tiff":
                    type = ImageType.TIFF;
                    break;

                case ".wmp":
                    type = ImageType.WMP;
                    break;

                case ".gif":
                    type = ImageType.GIF;
                    break;

                default:
                    return;
            }

            OutImageFile(image, path, type);
        }

        [ScriptMethod(LIB_NAME)]
        public static void OutImageFile(BitmapSource image, string path, ImageType type = ImageType.JPEG)
        {
            if (image is null)
            {
                return;
            }

            using (Stream stream = new FileStream(path, FileMode.Create))
            {
                BitmapEncoder encoder = null;
                switch (type)
                {
                    case ImageType.BMP:
                        encoder = new BmpBitmapEncoder();
                        break;
                    case ImageType.JPEG:
                        encoder = new JpegBitmapEncoder();
                        break;
                    case ImageType.PNG:
                        encoder = new PngBitmapEncoder();
                        break;
                    case ImageType.TIFF:
                        encoder = new TiffBitmapEncoder();
                        break;
                    case ImageType.WMP:
                        encoder = new WmpBitmapEncoder();
                        break;
                    case ImageType.GIF:
                        encoder = new GifBitmapEncoder();
                        break;
                }
                Debug.Assert(encoder != null);
                encoder.Frames.Add(BitmapFrame.Create(image));
                encoder.Save(stream);
            }
        }

        //------------------------------------------------------------------
        [ScriptMethod(LIB_NAME)]
        public static BitmapImage RGBAFilteringProc(
            BitmapSource image,
            IRGBAFilter imageProc,
            IRGBAFilter afterImageProc = null,
            int dpi = DEFAULT_dpi)
        {
            if (image is null)
            {
                return null;
            }

            int width = image.PixelWidth;
            int height = image.PixelHeight;
            int stride = GetStride(width, image);
            byte[] originalPixels = GetBytesOfPixelValues(image);

            int colorComponentLength = Enum.GetNames(typeof(ColorComponentType)).Length;
            byte[] outBuffer = new byte[width * height * colorComponentLength];
            for (int i = 0; i < originalPixels.Length; i += colorComponentLength)
            {
                double valueR = (double)originalPixels[i + (int)ColorComponentType.RED] / (double)byte.MaxValue;
                double valueG = (double)originalPixels[i + (int)ColorComponentType.GREEN] / (double)byte.MaxValue;
                double valueB = (double)originalPixels[i + (int)ColorComponentType.BLUE] / (double)byte.MaxValue;
                double valueAlpha = (double)originalPixels[i + (int)ColorComponentType.ALPHA] / (double)byte.MaxValue;

                if (imageProc != null)
                {
                    imageProc.CalcValueOfPixels(
                        valueR, valueG, valueB, valueAlpha,
                        out valueR, out valueG, out valueB, out valueAlpha
                        );
                }

                if (afterImageProc != null)
                {
                    afterImageProc.CalcValueOfPixels(
                        valueR, valueG, valueB, valueAlpha,
                        out valueR, out valueG, out valueB, out valueAlpha
                        );
                }

                outBuffer[i + (int)ColorComponentType.RED] = (byte)Math.Min(valueR * (double)byte.MaxValue, byte.MaxValue);
                outBuffer[i + (int)ColorComponentType.GREEN] = (byte)Math.Min(valueG * (double)byte.MaxValue, byte.MaxValue);
                outBuffer[i + (int)ColorComponentType.BLUE] = (byte)Math.Min(valueB * (double)byte.MaxValue, byte.MaxValue);
                outBuffer[i + (int)ColorComponentType.ALPHA] = (byte)Math.Min(valueAlpha * (double)byte.MaxValue, byte.MaxValue);
            }

            return ConvertToBitmapImage(BitmapSource.Create(width, height, dpi, dpi, PixelFormats.Pbgra32, null, outBuffer, stride), ImageType.PNG);
        }

        //------------------------------------------------------------------
        static int convIndex(int x, int y, int width)
        {
            int colorComponentLength = Enum.GetNames(typeof(ColorComponentType)).Length;
            return x * colorComponentLength + (width * colorComponentLength) * y;
        }

        [ScriptMethod(LIB_NAME)]
        public static BitmapImage RGBA3x3FilteringProc(
            BitmapSource image,
            IRGBAFilter beforeImageProc = null,
            IRGBA3x3Filter imageProc = null,
            IRGBAFilter afterImageProc = null,
            bool useThread = true,
            int dpi = DEFAULT_dpi)
        {
            IList<IRGBA3x3Filter> imageProcingList = null;

            if (imageProc != null)
            {
                imageProcingList = new List<IRGBA3x3Filter>() { imageProc };
            }

            return RGBA3x3FilteringProc(
                image,
                beforeImageProc,
                imageProcingList,
                afterImageProc,
                useThread,
                dpi);
        }

        [ScriptMethod(LIB_NAME)]
        public static BitmapImage RGBA3x3FilteringProc(
            BitmapSource image,
            IRGBAFilter beforeImageProc = null,
            IList<IRGBA3x3Filter> imageProcList = null,
            IRGBAFilter afterImageProc = null,
            bool useThread = true,
            int dpi = DEFAULT_dpi)
        {
            if (image is null)
            {
                return null;
            }

            int width = image.PixelWidth;
            int height = image.PixelHeight;
            int stride = GetStride(width, image);
            byte[] originalPixels = GetBytesOfPixelValues(image);

            int colorComponentLength = Enum.GetNames(typeof(ColorComponentType)).Length;
            byte[] outBuffer = new byte[width * height * colorComponentLength];

            bool isOut = false;

            if (beforeImageProc != null)
            {
                // 前処理

                originalPixels = beforeProcess(beforeImageProc, originalPixels, colorComponentLength, outBuffer);
                isOut = true;
            }

            if (imageProcList != null)
            {
                for (int i = 0; i < imageProcList.Count; ++i)
                {
                    bool isEnd = i == imageProcList.Count - 1;

                    IRGBA3x3Filter imageProcessing = imageProcList[i];
                    if (imageProcessing != null)
                    {
#if true    // コールバックを呼ぶ処理が通る（UIが絡む）場合は、スレッドが使えない
                        if (useThread)
                        {
                            var tasks = new List<Task>();
                            for (int y = 0; y < height; ++y)
                            {
                                int iy = y;
                                tasks.Add(Task.Run(() => RGBAConvolutionOpe3x3Processing_InputLine(imageProcessing, width, height, originalPixels, outBuffer, iy)));
                            }
                            Task.WaitAll(tasks.ToArray());
                        }
                        else
                        {
                            for (int y = 0; y < height; ++y)
                            {
                                RGBAConvolutionOpe3x3Processing_InputLine(imageProcessing, width, height, originalPixels, outBuffer, y);
                            }
                        }
#else
                        var tasks = new List<Task>();
                        for (int y = 0; y < height; ++y)
                        {
                            int iy = y;
                            tasks.Add(Task.Run(() => RGBAConvolutionOpe3x3Processing_InputLine(imageProcessing, width, height, originalPixels, outBuffer, iy)));
                        }
                        Task.WaitAll(tasks.ToArray());
#endif
                        originalPixels = (byte[])outBuffer.Clone();
                        isOut = true;
                    }
                }
            }

            if (afterImageProc != null)
            {
                // 後処理

                beforeProcess(afterImageProc, originalPixels, colorComponentLength, outBuffer);
                isOut = true;
            }

            if (!isOut)
            {
                outBuffer = (byte[])originalPixels.Clone();
            }

            return ConvertToBitmapImage(BitmapSource.Create(width, height, dpi, dpi, PixelFormats.Pbgra32, null, outBuffer, stride), ImageType.PNG);
        }

        private static byte[] beforeProcess(IRGBAFilter imageProcessing, byte[] originalPixels, int colorComponentLength, byte[] outBuffer)
        {
            for (int i = 0; i < originalPixels.Length; i += colorComponentLength)
            {
                double valueR = (double)originalPixels[i + (int)ColorComponentType.RED] / (double)byte.MaxValue;
                double valueG = (double)originalPixels[i + (int)ColorComponentType.GREEN] / (double)byte.MaxValue;
                double valueB = (double)originalPixels[i + (int)ColorComponentType.BLUE] / (double)byte.MaxValue;
                double valueAlpha = (double)originalPixels[i + (int)ColorComponentType.ALPHA] / (double)byte.MaxValue;

                imageProcessing.CalcValueOfPixels(
                    valueR, valueG, valueB, valueAlpha,
                    out valueR, out valueG, out valueB, out valueAlpha
                    );

                outBuffer[i + (int)ColorComponentType.RED] = (byte)Math.Min(valueR * (double)byte.MaxValue, byte.MaxValue);
                outBuffer[i + (int)ColorComponentType.GREEN] = (byte)Math.Min(valueG * (double)byte.MaxValue, byte.MaxValue);
                outBuffer[i + (int)ColorComponentType.BLUE] = (byte)Math.Min(valueB * (double)byte.MaxValue, byte.MaxValue);
                outBuffer[i + (int)ColorComponentType.ALPHA] = (byte)Math.Min(valueAlpha * (double)byte.MaxValue, byte.MaxValue);
            }
            return (byte[])outBuffer.Clone();
        }

        /// <summary>
        /// カーネルの一辺のサイズ（変更不可）
        /// </summary>
        const int Kernel3x3Size = 3;

        private static void RGBAConvolutionOpe3x3Processing_InputLine(
            IRGBA3x3Filter imageProcessing, 
            int width, 
            int height, 
            byte[] originalPixels, 
            byte[] outBuffer, 
            int y)
        {
            CRGBA[] Kernel = new CRGBA[Kernel3x3Size * Kernel3x3Size]
            {
                new CRGBA(),
                new CRGBA(),
                new CRGBA(),

                new CRGBA(),
                new CRGBA(),
                new CRGBA(),

                new CRGBA(),
                new CRGBA(),
                new CRGBA(),
            };

            for (int x = 0; x < width; ++x)
            {
                setKernel(width, height, originalPixels, y, x, Kernel);

                double valueR;
                double valueG;
                double valueB;
                double valueAlpha;

                imageProcessing.CalcValueOfPixels(
                    Kernel[0], Kernel[1], Kernel[2],
                    Kernel[3], Kernel[4], Kernel[5],
                    Kernel[6], Kernel[7], Kernel[8],
                    out valueR, out valueG, out valueB, out valueAlpha
                );
                valueR = Math.Min(Math.Max(valueR, 0), 1.0);
                valueG = Math.Min(Math.Max(valueG, 0), 1.0);
                valueB = Math.Min(Math.Max(valueB, 0), 1.0);
                valueAlpha = Math.Min(Math.Max(valueAlpha, 0), 1.0);

                int outIndex = convIndex(x, y, width);

                outBuffer[outIndex + (int)ColorComponentType.RED] = (byte)Math.Min(valueR * (double)byte.MaxValue, byte.MaxValue);
                outBuffer[outIndex + (int)ColorComponentType.GREEN] = (byte)Math.Min(valueG * (double)byte.MaxValue, byte.MaxValue);
                outBuffer[outIndex + (int)ColorComponentType.BLUE] = (byte)Math.Min(valueB * (double)byte.MaxValue, byte.MaxValue);
                outBuffer[outIndex + (int)ColorComponentType.ALPHA] = (byte)Math.Min(valueAlpha * (double)byte.MaxValue, byte.MaxValue);
            }
        }

        private static void setKernel(int width, int height, byte[] originalPixels, int y, int x, CRGBA[] Kernel)
        {
            for (int rx = 0; rx < Kernel3x3Size; ++rx)
                for (int ry = 0; ry < Kernel3x3Size; ++ry)
                {
                    int nx = x + rx;    // 近傍のX座標
                    int ny = y + ry;    // 近傍のY座標

                    bool withinRange = (0 < nx && nx < width) && (0 < ny && ny < height);

                    if (withinRange)
                    {
                        // 参照位置が画像の範囲内

                        int index = convIndex(nx - 1, ny - 1, width);
                        Kernel[ry * Kernel3x3Size + rx].SetValueOfPixels(
                            (double)originalPixels[index + (int)ColorComponentType.RED] / (double)byte.MaxValue,
                            (double)originalPixels[index + (int)ColorComponentType.GREEN] / (double)byte.MaxValue,
                            (double)originalPixels[index + (int)ColorComponentType.BLUE] / (double)byte.MaxValue,
                            (double)originalPixels[index + (int)ColorComponentType.ALPHA] / (double)byte.MaxValue
                        );
                    }
                    else
                    {
                        // 参照できない領域を参照しようとした

                        Kernel[ry * Kernel3x3Size + rx].SetValueOfPixels(
                            0.0,
                            0.0,
                            0.0,
                            1.0
                        );
                    }
                }
        }

        public class CRGBA3x3FilteringProc
        {
            public IRGBAFilter beforeImageProcessing = null;
            public IList<IRGBA3x3Filter> imageProcessingList = null;
            public IRGBAFilter afterImageProcessing = null;
            public bool useThread = true;
        }

        [ScriptMethod(LIB_NAME)]
        public static BitmapImage RGBA3x3FilteringProc(
            BitmapSource image,
            CRGBA3x3FilteringProc cRGBA3x3Proc,
            bool useThread = true,
            int dpi = DEFAULT_dpi)
        {
            return RGBA3x3FilteringProc(
                image,
                cRGBA3x3Proc.beforeImageProcessing,
                cRGBA3x3Proc.imageProcessingList,
                cRGBA3x3Proc.afterImageProcessing,
                useThread,
                dpi
                );
        }

        [ScriptMethod(LIB_NAME)]
        public static CRGBA3x3FilteringProc CreateRGBA3x3FilteringProc(
            IRGBAFilter beforeImageProcessing = null,
            IList<IRGBA3x3Filter> imageProcessingList = null,
            IRGBAFilter afterImageProcessing = null,
            bool useThread = true
            )
        {
            return new CRGBA3x3FilteringProc()
            {
                beforeImageProcessing = beforeImageProcessing,
                imageProcessingList = imageProcessingList,
                afterImageProcessing = afterImageProcessing,
                useThread = useThread,
            };
        }

        //------------------------------------------------------------------
        /// <summary>
        /// スクリーンをキャプチャします。
        /// </summary>
        /// <returns>キャプチャしたイメージ</returns>
        [ScriptMethod(LIB_NAME)]
        public static BitmapSource ScreenCapture(bool hideAndCapture = true)
        {
            if (hideAndCapture)
            {
                CommandCanvasList.OwnerWindow.WindowState = WindowState.Minimized;
                Thread.Sleep(500);
            }
            BitmapSource bitmap = getScreenImage();
            if (hideAndCapture)
            {
                CommandCanvasList.OwnerWindow.WindowState = WindowState.Normal;
            }
            return bitmap;
        }

        public static BitmapSource getScreenImage()
        {
            using (var bitmap = new Bitmap((int)SystemParameters.PrimaryScreenWidth, (int)SystemParameters.PrimaryScreenHeight))
            using (System.Drawing.Graphics graphics = System.Drawing.Graphics.FromImage(bitmap))
            {
                graphics.CopyFromScreen(0, 0, 0, 0, bitmap.Size);
                IntPtr bmpHandle = bitmap.GetHbitmap();
                return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                        bmpHandle,
                        IntPtr.Zero,
                        Int32Rect.Empty,
                        System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
            }
        }

        //------------------------------------------------------------------
        [ScriptMethod(nameSpace3)]
        public static BitmapImage Scaling(BitmapSource image, double widthScale, double heightScale, int dpi = DEFAULT_dpi)
        {
            if (image is null)
                return null;
            int width = (int)(image.PixelWidth * widthScale);
            int height = (int)(image.PixelHeight * heightScale);
            return Scaling(image, width, height, dpi);
        }

        //------------------------------------------------------------------
        [ScriptMethod(nameSpace3)]
        public static BitmapImage Scaling(BitmapSource image, int width, int height, int dpi = DEFAULT_dpi)
        {
            if (image is null)
                return null;
            var tempRenderTarget = new RenderTargetBitmap((int)width, (int)height, dpi, dpi, PixelFormats.Pbgra32);
            var visualForImage = new DrawingVisual();
            using (var context = visualForImage.RenderOpen())
            {
                context.DrawImage(image, new System.Windows.Rect(0, 0, width, height));
            }
            // ビットマップに visualForImage をレンダリング
            tempRenderTarget.Render(visualForImage);
            return Image.ConvertToBitmapImage(tempRenderTarget, Image.ImageType.PNG);
        }

        //------------------------------------------------------------------
        [ScriptMethod(nameSpace4)]
        public static BitmapImage Synthesis(
            BitmapSource image,
            BitmapSource add,
            int x,
            int y,
            double transparent = 1.0,
            int dpi = DEFAULT_dpi)
        {
            if (image is null || add is null)
                return null;

            return Synthesis(
                image,
                add,
                x,
                y,
                add.PixelWidth,
                add.PixelHeight,
                transparent,
                dpi);
        }

        [ScriptMethod(nameSpace4)]
        public static BitmapImage Synthesis(
            BitmapSource image,
            BitmapSource add,
            double transparent = 1.0,
            int dpi = DEFAULT_dpi)
        {
            if (image is null || add is null)
                return null;

            int baseWidth = image.PixelWidth;
            int baseHeight = image.PixelHeight;
            return Synthesis(
                image,
                add,
                0,
                0,
                baseWidth,
                baseHeight,
                transparent,
                dpi);
        }

        [ScriptMethod(nameSpace4)]
        public static BitmapImage Synthesis(
            BitmapSource image, 
            BitmapSource add, 
            int x,
            int y,
            int width,
            int height,
            double transparent = 1.0, 
            int dpi = DEFAULT_dpi)
        {
            if (image is null || add is null)
                return null;
            int baseWidth = image.PixelWidth;
            int baseHeight = image.PixelHeight;
            var tempRenderTarget = new RenderTargetBitmap((int)baseWidth, (int)baseHeight, dpi, dpi, PixelFormats.Pbgra32);
            {
                var visualForImage = new DrawingVisual();
                using (var context = visualForImage.RenderOpen())
                {
                    context.DrawImage(image, new System.Windows.Rect(0, 0, baseWidth, baseHeight));
                }
                // ビットマップに visualForImage をレンダリング
                tempRenderTarget.Render(visualForImage);
            }
            if (!transparent.Equals(1.0))
            {
                add = RGBAFilteringProc(
                            add,
                            CreateRGBATransparentFilter(transparent),
                            null,
                            dpi);
            }
            {
                var visualForImage = new DrawingVisual();
                using (var context = visualForImage.RenderOpen())
                {
                    context.DrawImage(add, new System.Windows.Rect(x, y, width, height));
                }
                // ビットマップに visualForImage をレンダリング
                tempRenderTarget.Render(visualForImage);
            }
            return Image.ConvertToBitmapImage(tempRenderTarget, Image.ImageType.PNG);
        }

        //------------------------------------------------------------------
        /// <summary>
        /// 画像のヒストグラムを取得します。
        /// </summary>
        /// <param name="image">対象の画像</param>
        /// <returns>画素値の分布配列</returns>
        public static int[] GetHistogram(BitmapSource image, ColorComponentType colorComponent)
        {
            if (image is null)
            {
                return new int[byte.MaxValue];
            }

            byte[] originalPixels = GetBytesOfPixelValues(image);

            int colorComponentLength = Enum.GetNames(typeof(ColorComponentType)).Length;
            int[] numberOfPixels = new int[byte.MaxValue];
            for (int i = 0; i < originalPixels.Length; i += colorComponentLength)
            {
                byte value = originalPixels[i + (int)colorComponent];
                if (value == 0)
                    continue;
                numberOfPixels[value - 1]++;
            }
            numberOfPixels[0] = 0;  // 0値の個数は不要

            return numberOfPixels;
        }

        [ScriptMethod(nameSpace5)]
        public static IList<int> GetHistogramList(BitmapSource image, ColorComponentType colorComponent)
        {
            return new List<int>(GetHistogram(image, colorComponent));
        }

        //------------------------------------------------------------------
        /// <summary>
        /// 画像のヒストグラムを取得します。
        /// </summary>
        /// <param name="image">対象の画像</param>
        [ScriptMethod(nameSpace5)]
        public static void OutHistogram(string msg, BitmapSource image, GlayScaleType glayScale = GlayScaleType.NONE, bool R = true, bool G = true, bool B = true)
        {
            if (image is null)
                return;

            if (glayScale != GlayScaleType.NONE)
            {
                // グレースケールを掛けます。

                image = RGBAFilteringProc(image, CreateRGBAGlayScaleFilter(glayScale));
            }

            byte[] originalPixels = GetBytesOfPixelValues(image);

            int colorComponentLength = Enum.GetNames(typeof(ColorComponentType)).Length;
            int[] numberOfGryPixels = new int[byte.MaxValue + 1];
            int[] numberOfRPixels = new int[byte.MaxValue + 1];
            int[] numberOfGPixels = new int[byte.MaxValue + 1];
            int[] numberOfBPixels = new int[byte.MaxValue + 1];
            for (int i = 0; i < originalPixels.Length; i += colorComponentLength)
            {
                numberOfRPixels[originalPixels[i + (int)ColorComponentType.RED]]++;
                numberOfGPixels[originalPixels[i + (int)ColorComponentType.GREEN]]++;
                numberOfBPixels[originalPixels[i + (int)ColorComponentType.BLUE]]++;
            }
            bool isTotalEq = true;
            for (int i = 0; i < byte.MaxValue + 1; ++i)
            {
                int r = R ? numberOfRPixels[i] : 0;
                int g = G ? numberOfGPixels[i] : 0;
                int b = B ? numberOfBPixels[i] : 0;
                var srt = new List<int>() { r, g, b };
                srt.Sort();
                bool isBar = false;
                if (!isBar)
                    isBar = srt[2] == srt[1] && srt[0] == 0;            // ２つが同じで残りは０
                if (!isBar)
                    isBar = srt[0] == srt[1] && srt[1] == srt[2];       // ３つとも同じ
                if (isBar)
                {
                    numberOfGryPixels[i] = srt[2];
                }
                else
                {
                    isTotalEq = false;
                    numberOfGryPixels[i] = 0;
                }
            }

            var plotList = new List<PlotInfo>();

            var listGry = new List<int>(numberOfGryPixels);
            listGry.RemoveAt(0);   // 0の分布数は不要
            var listR = new List<int>(numberOfRPixels);
            listR.RemoveAt(0);   // 0の分布数は不要
            var listG = new List<int>(numberOfGPixels);
            listG.RemoveAt(0);   // 0の分布数は不要
            var listB = new List<int>(numberOfBPixels);
            listB.RemoveAt(0);   // 0の分布数は不要

            if (isTotalEq)
            {
                plotList.Add(Graph.CreatePlotInfo(listGry, System.Windows.Media.Brushes.Black, DrawType.BarGraph));
            }
            else
            {
                if (listGry.Sum() > 0)
                    plotList.Add(Graph.CreatePlotInfo(listGry, System.Windows.Media.Brushes.Gray, DrawType.Line));

                if (listR.Sum() > 0)
                    plotList.Add(Graph.CreatePlotInfo(listR, System.Windows.Media.Brushes.Red, DrawType.Line));

                if (listG.Sum() > 0)
                    plotList.Add(Graph.CreatePlotInfo(listG, System.Windows.Media.Brushes.Green, DrawType.Line));

                if (listB.Sum() > 0)
                    plotList.Add(Graph.CreatePlotInfo(listB, System.Windows.Media.Brushes.Blue, DrawType.Line));
            }

            Graph.OutPlot(msg, plotList);
        }

        //------------------------------------------------------------------
        /// <summary>
        /// 画像のヒストグラムを取得します。
        /// </summary>
        /// <param name="imagePath">対象の画像へのパス</param>
        [ScriptMethod(nameSpace5)]
        public static void OutHistogram(string msg, string imagePath, GlayScaleType glayScale = GlayScaleType.NONE, bool R = true, bool G = true, bool B = true)
        {
            OutHistogram(msg, OpenImage(imagePath), glayScale, R, G, B);
        }

        //------------------------------------------------------------------
        /// <summary>
        /// 画像の画素値分布中の最大数存在する画素値の分布数を取得します。
        /// ※透過色は無視します。
        /// </summary>
        /// <param name="image">対象の画像</param>
        /// <returns>最大数存在する画素値の分布数</returns>
        [ScriptMethod(nameSpace5)]
        public static int GetMaxNumberOfPixels(BitmapSource image)
        {
            if (image is null)
            {
                return 0;
            }

            byte[] originalPixels = GetBytesOfPixelValues(image);

            int colorComponentLength = Enum.GetNames(typeof(ColorComponentType)).Length;
            int[] numberOfPixels = new int[byte.MaxValue * colorComponentLength];
            for (int i = 0; i < originalPixels.Length; i += colorComponentLength)
            {
                for (int colorComponent = 0; colorComponent < colorComponentLength - 1; ++colorComponent)
                {
                    byte pixelValue = originalPixels[i + colorComponent];
                    numberOfPixels[byte.MaxValue * colorComponent + pixelValue]++;
                }
            }

            return StatisticsLib.Max(new List<int>(numberOfPixels));
        }

        //------------------------------------------------------------------
        /// <summary>
        /// 画像の画素値の平均値を取得します。
        /// ※透過色は無視します。
        /// </summary>
        /// <param name="image">対象の画像</param>
        /// <returns>平均値</returns>
        [ScriptMethod(nameSpace5)]
        public static double GetAverageNumberOfPixels(BitmapSource image)
        {
            if (image is null)
            {
                return 0;
            }

            byte[] originalPixels = GetBytesOfPixelValues(image);

            int colorComponentLength = Enum.GetNames(typeof(ColorComponentType)).Length;
            double total = 0;
            for (int i = 0; i < originalPixels.Length; i += colorComponentLength)
            {
                double valueR = (double)originalPixels[i + (int)ColorComponentType.RED] / (double)byte.MaxValue;
                double valueG = (double)originalPixels[i + (int)ColorComponentType.GREEN] / (double)byte.MaxValue;
                double valueB = (double)originalPixels[i + (int)ColorComponentType.BLUE] / (double)byte.MaxValue;

                total += (valueR + valueG + valueB) / 3.0;
            }

            return total / ((double)originalPixels.Length / (double)colorComponentLength);
        }

        //------------------------------------------------------------------
        /// <summary>
        /// ビットマップをバイト情報に変換します。
        /// </summary>
        /// <param name="image">ビットマップ</param>
        /// <returns>バイト情報</returns>
        private static byte[] GetBytesOfPixelValues(BitmapSource image)
        {
            int colorComponentLength = Enum.GetNames(typeof(ColorComponentType)).Length;
            FormatConvertedBitmap bitmap = new FormatConvertedBitmap(image, PixelFormats.Pbgra32, null, 0);
            int width = bitmap.PixelWidth;
            int height = bitmap.PixelHeight;
            int stride = GetStride(width, image);
            byte[] originalPixels = new byte[width * height * colorComponentLength];
            bitmap.CopyPixels(originalPixels, stride, 0);
            return originalPixels;
        }

        //------------------------------------------------------------------
        [ScriptMethod(nameSpace5)]
        public static int GetPixelWidth(BitmapSource image)
        {
            if (image is null)
            {
                return 0;
            }
            return image.PixelWidth;
        }

        //------------------------------------------------------------------
        [ScriptMethod(nameSpace5)]
        public static int GetPixelHeight(BitmapSource image)
        {
            if (image is null)
            {
                return 0;
            }
            return image.PixelHeight;
        }

        //------------------------------------------------------------------
        public static BitmapImage ConvertToBitmapImage(BitmapSource image, ImageType type = ImageType.JPEG)
        {
            return ConvertToBitmapImage(BitmapFrame.Create(image), type);
        }
        public static BitmapImage ConvertToBitmapImage(BitmapFrame bitmapFrame, ImageType type = ImageType.JPEG)
        {
            BitmapEncoder encoder = null;
            switch (type)
            {
                case ImageType.BMP:
                    encoder = new BmpBitmapEncoder();
                    break;
                case ImageType.JPEG:
                    encoder = new JpegBitmapEncoder();
                    break;
                case ImageType.PNG:
                    encoder = new PngBitmapEncoder();
                    break;
                case ImageType.TIFF:
                    encoder = new TiffBitmapEncoder();
                    break;
                case ImageType.WMP:
                    encoder = new WmpBitmapEncoder();
                    break;
                case ImageType.GIF:
                    encoder = new GifBitmapEncoder();
                    break;
            }
            Debug.Assert(encoder != null);
            var bitmapImage = new BitmapImage();
            encoder.Frames.Add(bitmapFrame);
            using (var stream = new MemoryStream())
            {
                encoder.Save(stream);
                stream.Seek(0, SeekOrigin.Begin);

                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.StreamSource = stream;
                bitmapImage.EndInit();
            }

            return bitmapImage;
        }

        //------------------------------------------------------------------
        [ScriptMethod(nameSpace6)]
        public static byte GammaCorrection(byte pixelValue, double gamma = DEF_GammaCollection2dot2)
        {
            return (byte)(Math.Pow((double)pixelValue / 255.0, gamma) * 255.0);
        }

        //------------------------------------------------------------------
        [ScriptMethod(nameSpace6)]
        public static double GammaCorrection(double pixelValue, double gamma = DEF_GammaCollection2dot2)
        {
            return Math.Pow((double)pixelValue, gamma);
        }

        //------------------------------------------------------------------
        /// <summary>
        /// 複数のIRGBAFilterをひとまとめにするためのハブです。
        /// </summary>
        public class CRGBAFilteringHub : IRGBAFilter, IRGBA3x3Filter
        {
            IList<IRGBAFilter> list = null;
            public CRGBAFilteringHub(IList<IRGBAFilter> list)
            {
                this.list = list;
            }

            public override string ToString()
            {
                string str = $"{this.GetType().FullName} - {nameof(list)}" + Environment.NewLine;
                foreach (var node in list)
                {
                    str += node.ToString() + Environment.NewLine;
                }
                return str;
            }

            void IRGBA3x3Filter.CalcValueOfPixels(
                CRGBA pixel00,
                CRGBA pixel01,
                CRGBA pixel02,
                CRGBA pixel10,
                CRGBA pixel11,
                CRGBA pixel12,
                CRGBA pixel20,
                CRGBA pixel21,
                CRGBA pixel22,
                out double R,
                out double G,
                out double B,
                out double Alpha
                )
            {
                CalcValueOfPixels(
                    pixel11.R, pixel11.G, pixel11.B, pixel11.Alpha,
                    out R, out G, out B, out Alpha
                    );
            }

            public void CalcValueOfPixels(
                double r, double g, double b, double a,
                out double R, out double G, out double B, out double Alpha
                )
            {
                R = r;
                G = g;
                B = b;
                Alpha = a;

                if (list is null)
                    return;

                foreach (IRGBAFilter proc in list)
                {
                    if (proc is null)
                        continue;

                    proc.CalcValueOfPixels(
                        R, G, B, Alpha,
                        out R, out G, out B, out Alpha
                        );
                }
            }
        }

        [ScriptMethod(nameSpace7)]
        public static CRGBAFilteringHub CreateRGBAFilteringHub(IList<IRGBAFilter> list = null)
        {
            return new CRGBAFilteringHub(list);
        }

        //------------------------------------------------------------------
        /// <summary>
        /// 各画素値に対して処理を定義するクラスです。
        /// </summary>
        public class CRGBAEventFuncFilter : IRGBAFilter, IRGBA3x3Filter
        {
            Func<double, double> funcR = null;
            Func<double, double> funcG = null;
            Func<double, double> funcB = null;
            Func<double, double> funcA = null;
            public CRGBAEventFuncFilter(
                Func<double, double> funcR = null,
                Func<double, double> funcG = null, 
                Func<double, double> funcB = null,
                Func<double, double> funcA = null
                )
            {
                this.funcR = funcR;
                this.funcG = funcG;
                this.funcB = funcB;
                this.funcA = funcA;
            }

            double CallFunc(double pixel, Func<double, double> func)
            {
                if (func != null)
                {
                    pixel = func(pixel);
                    if (pixel > 1.0)
                    {
                        return 1.0;
                    }
                    if (pixel < 0.0)
                    {
                        return 0.0;
                    }
                }
                return pixel;
            }

            void IRGBA3x3Filter.CalcValueOfPixels(
                CRGBA pixel00,
                CRGBA pixel01,
                CRGBA pixel02,
                CRGBA pixel10,
                CRGBA pixel11,
                CRGBA pixel12,
                CRGBA pixel20,
                CRGBA pixel21,
                CRGBA pixel22,
                out double R,
                out double G,
                out double B,
                out double Alpha
                )
            {
                CalcValueOfPixels(
                    pixel11.R, pixel11.G, pixel11.B, pixel11.Alpha,
                    out R, out G, out B, out Alpha
                    );
            }

            public void CalcValueOfPixels(
                double r, double g, double b, double a,
                out double R, out double G, out double B, out double Alpha
                )
            {
                R = CallFunc(r, funcR);
                G = CallFunc(g, funcG);
                B = CallFunc(b, funcB);
                Alpha = CallFunc(a, funcA);
            }
        }

        [ScriptMethod(nameSpace7)]
        public static CRGBAEventFuncFilter CreateRGBAEventFuncFilter(
                Func<double, double> funcR = null,
                Func<double, double> funcG = null,
                Func<double, double> funcB = null,
                Func<double, double> funcA = null
            )
        {
            return new CRGBAEventFuncFilter(funcR, funcG, funcB, funcA);
        }

        //------------------------------------------------------------------
        /// <summary>
        /// 各画素値を画素値の平均で引く処理を定義するクラスです。
        /// </summary>
        public class CRGBASubAverageFilter : IRGBAFilter, IRGBA3x3Filter
        {
            public CRGBASubAverageFilter() { }

            void IRGBA3x3Filter.CalcValueOfPixels(
                CRGBA pixel00,
                CRGBA pixel01,
                CRGBA pixel02,
                CRGBA pixel10,
                CRGBA pixel11,
                CRGBA pixel12,
                CRGBA pixel20,
                CRGBA pixel21,
                CRGBA pixel22,
                out double R,
                out double G,
                out double B,
                out double Alpha
                )
            {
                CalcValueOfPixels(
                    pixel11.R, pixel11.G, pixel11.B, pixel11.Alpha,
                    out R, out G, out B, out Alpha
                    );
            }

            public void CalcValueOfPixels(
                double r, double g, double b, double a,
                out double R, out double G, out double B, out double Alpha
                )
            {
                double avg = (r + g + b) / 3.0;
                R = ((r > avg) ? r - avg : 0.0);
                G = ((g > avg) ? g - avg : 0.0);
                B = ((b > avg) ? b - avg : 0.0);
                Alpha = a;
            }
        }

        [ScriptMethod(nameSpace7)]
        public static CRGBASubAverageFilter CreateRGBASubAverageFilter()
        {
            return new CRGBASubAverageFilter();
        }

        //------------------------------------------------------------------
        /// <summary>
        /// 各画素値を定数倍する処理を作成します。
        /// </summary>
        public class CRGBAMulRateFilter : IRGBAFilter, IRGBA3x3Filter
        {
            double rateR = 1;
            double rateG = 1;
            double rateB = 1;
            public CRGBAMulRateFilter(double rateR, double rateG, double rateB)
            {
                this.rateR = Math.Abs(rateR);
                this.rateG = Math.Abs(rateG);
                this.rateB = Math.Abs(rateB);
            }

            public override string ToString()
            {
                return $"{this.GetType().FullName} - {nameof(rateR)}: {rateR}, {nameof(rateG)}: {rateG}, {nameof(rateB)}: {rateB}";
            }

            void IRGBA3x3Filter.CalcValueOfPixels(
                CRGBA pixel00,
                CRGBA pixel01,
                CRGBA pixel02,
                CRGBA pixel10,
                CRGBA pixel11,
                CRGBA pixel12,
                CRGBA pixel20,
                CRGBA pixel21,
                CRGBA pixel22,
                out double R,
                out double G,
                out double B,
                out double Alpha
                )
            {
                CalcValueOfPixels(
                    pixel11.R, pixel11.G, pixel11.B, pixel11.Alpha,
                    out R, out G, out B, out Alpha
                    );
            }

            public void CalcValueOfPixels(
                double r, double g, double b, double a,
                out double R, out double G, out double B, out double Alpha
                )
            {
                R = r * rateR;
                G = g * rateG;
                B = b * rateB;
                if (R > 1.0) R = 1.0;
                if (G > 1.0) G = 1.0;
                if (B > 1.0) B = 1.0;
                Alpha = a;
            }
        }

        [ScriptMethod(nameSpace7)]
        public static CRGBAMulRateFilter CreateRGBAMulRateFilter(
            double rateR = 1,
            double rateG = 1,
            double rateB = 1)
        {
            return new CRGBAMulRateFilter(rateR, rateG, rateB);
        }

        //------------------------------------------------------------------
        /// <summary>
        /// ガンマ処理を定義するクラスです。
        /// </summary>
        public class CRGBAGammaCorrectionFilter : IRGBAFilter, IRGBA3x3Filter
        {
            public double gammaR = DEF_GammaCollection2dot2;
            public double gammaG = DEF_GammaCollection2dot2;
            public double gammaB = DEF_GammaCollection2dot2;
            public CRGBAGammaCorrectionFilter() { }
            public CRGBAGammaCorrectionFilter(double gamma = DEF_GammaCollection2dot2)
            {
                gammaR = gamma;
                gammaG = gamma;
                gammaB = gamma;
            }
            public CRGBAGammaCorrectionFilter(double gammaR, double gammaG, double gammaB)
            {
                this.gammaR = gammaR;
                this.gammaG = gammaG;
                this.gammaB = gammaB;
            }

            void IRGBA3x3Filter.CalcValueOfPixels(
                CRGBA pixel00,
                CRGBA pixel01,
                CRGBA pixel02,
                CRGBA pixel10,
                CRGBA pixel11,
                CRGBA pixel12,
                CRGBA pixel20,
                CRGBA pixel21,
                CRGBA pixel22,
                out double R,
                out double G,
                out double B,
                out double Alpha
                )
            {
                CalcValueOfPixels(
                    pixel11.R, pixel11.G, pixel11.B, pixel11.Alpha,
                    out R, out G, out B, out Alpha
                    );
            }

            public void CalcValueOfPixels(
                double r, double g, double b, double a,
                out double R, out double G, out double B, out double Alpha
                )
            {
                R = GammaCorrection(r, gammaR);
                G = GammaCorrection(g, gammaG);
                B = GammaCorrection(b, gammaB);
                Alpha = a;
            }

            public override string ToString()
            {
                return $"{this.GetType().FullName} - {nameof(gammaR)}: {gammaR}, {nameof(gammaG)}: {gammaG}, {nameof(gammaB)}: {gammaB}";
            }
        }

        [ScriptMethod(nameSpace7)]
        public static CRGBAGammaCorrectionFilter CreateRGBAGammaCorrectionFilter(
            double gamma = DEF_GammaCollection2dot2)
        {
            return new CRGBAGammaCorrectionFilter(gamma);
        }

        [ScriptMethod(nameSpace7)]
        public static CRGBAGammaCorrectionFilter CreateRGBAGammaCorrectionFilter(
            double gammaR = DEF_GammaCollection2dot2,
            double gammaG = DEF_GammaCollection2dot2,
            double gammaB = DEF_GammaCollection2dot2)
        {
            return new CRGBAGammaCorrectionFilter(gammaR, gammaG, gammaB);
        }

        //------------------------------------------------------------------
        /// <summary>
        /// グレースケールを定義するクラスです。
        /// </summary>
        public class CRGBAGlayScaleFilter : IRGBAFilter, IRGBA3x3Filter
        {
            GlayScaleType type;
            IRGBAFilter beforeImageProc;

            public CRGBAGlayScaleFilter(
                GlayScaleType type = GlayScaleType.AVERAGE,
                IRGBAFilter beforeImageProc = null)
            {
                this.type = type;
                this.beforeImageProc = beforeImageProc;
            }

            public override string ToString()
            {
                return $"{this.GetType().FullName} - " + 
                    $"{nameof(type)}: {type.ToString()}, {nameof(beforeImageProc)}: {beforeImageProc?.ToString()}";
            }

            void IRGBA3x3Filter.CalcValueOfPixels(
                CRGBA pixel00,
                CRGBA pixel01,
                CRGBA pixel02,
                CRGBA pixel10,
                CRGBA pixel11,
                CRGBA pixel12,
                CRGBA pixel20,
                CRGBA pixel21,
                CRGBA pixel22,
                out double R,
                out double G,
                out double B,
                out double Alpha
                )
            {
                CalcValueOfPixels(
                    pixel11.R, pixel11.G, pixel11.B, pixel11.Alpha,
                    out R, out G, out B, out Alpha
                    );
            }

            public void CalcValueOfPixels(
                double r, double g, double b, double a,
                out double R, out double G, out double B, out double Alpha
                )
            {
                Alpha = a;
                double PixelValue = 0;
                if (beforeImageProc != null)
                {
                    beforeImageProc.CalcValueOfPixels(
                        r, g, b, a,
                        out r, out g, out b, out a
                        );
                }
                switch (type)
                {
                    case GlayScaleType.AVERAGE:
                        PixelValue = (r + g + b) / 3.0;
                        break;

                    case GlayScaleType.BT_709:
                        PixelValue = 0.2126 * r + 0.7152 * g + 0.0722 * b;
                        break;

                    case GlayScaleType.BT_601:
                        PixelValue = 0.299 * r + 0.587 * g + 0.114 * b;
                        break;

                    case GlayScaleType.YCgCo_Y:
                        PixelValue = r / 4 + g / 2 + b / 4;
                        break;

                    case GlayScaleType.ONLY_G:
                        PixelValue = g;
                        break;

                    default:
                        R = r;
                        G = g;
                        B = b;
                        return;
                }

                R = PixelValue;
                G = PixelValue;
                B = PixelValue;
            }
        }

        [ScriptMethod(nameSpace7)]
        public static CRGBAGlayScaleFilter CreateRGBAGlayScaleFilter(GlayScaleType type = GlayScaleType.AVERAGE, IRGBAFilter beforeImageProc = null)
        {
            return new CRGBAGlayScaleFilter(type, beforeImageProc);
        }

        //------------------------------------------------------------------
        /// <summary>
        /// ２値化処理を定義するクラスです。
        /// </summary>
        public class CRGBABinarizationFilter : IRGBAFilter, IRGBA3x3Filter
        {
            double threshold = 0;
            public CRGBABinarizationFilter(double threshold)
            {
                this.threshold = threshold;
            }

            public override string ToString()
            {
                return $"{this.GetType().FullName} - {nameof(threshold)}: {threshold}";
            }

            void IRGBA3x3Filter.CalcValueOfPixels(
                CRGBA pixel00,
                CRGBA pixel01,
                CRGBA pixel02,
                CRGBA pixel10,
                CRGBA pixel11,
                CRGBA pixel12,
                CRGBA pixel20,
                CRGBA pixel21,
                CRGBA pixel22,
                out double R,
                out double G,
                out double B,
                out double Alpha
                )
            {
                CalcValueOfPixels(
                    pixel11.R, pixel11.G, pixel11.B, pixel11.Alpha,
                    out R, out G, out B, out Alpha
                    );
            }

            public void CalcValueOfPixels(
                double r, double g, double b, double a,
                out double R, out double G, out double B, out double Alpha
                )
            {
                double check = (r + g + b) / 3.0;
                double value = (check > threshold) ? byte.MaxValue : 0;

                R = value;
                G = value;
                B = value;
                Alpha = a;
            }
        }

        [ScriptMethod(nameSpace7)]
        public static CRGBABinarizationFilter CreateRGBABinarizationFilter(double threshold = 0.5)
        {
            return new CRGBABinarizationFilter(threshold);
        }

        [ScriptMethod(nameSpace7)]
        public static CRGBABinarizationFilter CreateRGBAAdaptiveBinarizationFilter(BitmapSource image)
        {
            return new CRGBABinarizationFilter(GetAverageNumberOfPixels(image));
        }

        //------------------------------------------------------------------
        /// <summary>
        /// ネガポジ反転処理を定義するクラスです。
        /// </summary>
        public class CRGBANegativePositiveReversalFilter : IRGBAFilter, IRGBA3x3Filter
        {
            public CRGBANegativePositiveReversalFilter()
            {
            }

            public override string ToString()
            {
                return $"{this.GetType().FullName}";
            }

            void IRGBA3x3Filter.CalcValueOfPixels(
                CRGBA pixel00,
                CRGBA pixel01,
                CRGBA pixel02,
                CRGBA pixel10,
                CRGBA pixel11,
                CRGBA pixel12,
                CRGBA pixel20,
                CRGBA pixel21,
                CRGBA pixel22,
                out double R,
                out double G,
                out double B,
                out double Alpha
                )
            {
                CalcValueOfPixels(
                    pixel11.R, pixel11.G, pixel11.B, pixel11.Alpha,
                    out R, out G, out B, out Alpha
                    );
            }

            public void CalcValueOfPixels(
                double r, double g, double b, double a,
                out double R, out double G, out double B, out double Alpha
                )
            {
                R = Math.Abs(1.0 - r);
                G = Math.Abs(1.0 - g);
                B = Math.Abs(1.0 - b);
                Alpha = a;
            }
        }

        [ScriptMethod(nameSpace7)]
        public static CRGBANegativePositiveReversalFilter CreateRGBANegativePositiveReversalFilter()
        {
            return new CRGBANegativePositiveReversalFilter();
        }

        //------------------------------------------------------------------
        /// <summary>
        /// 透過処理を定義するクラスです。
        /// </summary>
        public class CRGBATransparentFilter : IRGBAFilter, IRGBA3x3Filter
        {
            double rate = 1.0;
            public CRGBATransparentFilter(double rate = 1.0)
            {
                this.rate = rate;
            }

            public override string ToString()
            {
                return $"{this.GetType().FullName} - {nameof(rate)}: {rate}";
            }

            void IRGBA3x3Filter.CalcValueOfPixels(
                CRGBA pixel00,
                CRGBA pixel01,
                CRGBA pixel02,
                CRGBA pixel10,
                CRGBA pixel11,
                CRGBA pixel12,
                CRGBA pixel20,
                CRGBA pixel21,
                CRGBA pixel22,
                out double R,
                out double G,
                out double B,
                out double Alpha
                )
            {
                CalcValueOfPixels(
                    pixel11.R, pixel11.G, pixel11.B, pixel11.Alpha,
                    out R, out G, out B, out Alpha
                    );
            }

            public void CalcValueOfPixels(
                double r, double g, double b, double a,
                out double R, out double G, out double B, out double Alpha
                )
            {
                R = r;
                G = g;
                B = b;
                Alpha = rate;
            }
        }

        [ScriptMethod(nameSpace7)]
        public static CRGBATransparentFilter CreateRGBATransparentFilter(double rate)
        {
            return new CRGBATransparentFilter(rate);
        }

        //------------------------------------------------------------------
        /// <summary>
        /// 透過処理を定義するクラスです。
        /// </summary>
        public class CRGBAWhiteMaskFilter : IRGBAFilter, IRGBA3x3Filter
        {
            bool smooth = false;

            public CRGBAWhiteMaskFilter(bool smooth = false)
            {
                this.smooth = smooth;
            }

            public override string ToString()
            {
                return $"{this.GetType().FullName} - {nameof(smooth)}: {smooth}";
            }

            void IRGBA3x3Filter.CalcValueOfPixels(
                CRGBA pixel00,
                CRGBA pixel01,
                CRGBA pixel02,
                CRGBA pixel10,
                CRGBA pixel11,
                CRGBA pixel12,
                CRGBA pixel20,
                CRGBA pixel21,
                CRGBA pixel22,
                out double R,
                out double G,
                out double B,
                out double Alpha
                )
            {
                CalcValueOfPixels(
                    pixel11.R, pixel11.G, pixel11.B, pixel11.Alpha,
                    out R, out G, out B, out Alpha
                    );
            }

            public void CalcValueOfPixels(
                double r, double g, double b, double a,
                out double R, out double G, out double B, out double Alpha
                )
            {
                R = r;
                G = g;
                B = b;
                if (!smooth && r.Equals(1.0) && r.Equals(g) && g.Equals(b))
                {
                    Alpha = 0;
                }
                else if (r.Equals(g) && g.Equals(b))
                {
                    double avg = (r + g + b) / 3.0;
                    Alpha = 1.0 - EasingFunction.InOutSin(avg);
                }
                else
                {
                    Alpha = 1.0;
                }
            }
        }

        [ScriptMethod(nameSpace7)]
        public static CRGBAWhiteMaskFilter CreateRGBAWhiteMaskFilter(bool smooth = false)
        {
            return new CRGBAWhiteMaskFilter(smooth);
        }

        //------------------------------------------------------------------
        /// <summary>
        /// コントラスト処理を定義するクラスです。
        /// </summary>
        public class CRGBAContrastFilter : IRGBAFilter, IRGBA3x3Filter
        {
            Func<double, double> func = null;

            public CRGBAContrastFilter(Func<double, double> func)
            {
                this.func = func;
            }

            public override string ToString()
            {
                return $"{this.GetType().FullName}";
            }

            void IRGBA3x3Filter.CalcValueOfPixels(
                CRGBA pixel00,
                CRGBA pixel01,
                CRGBA pixel02,
                CRGBA pixel10,
                CRGBA pixel11,
                CRGBA pixel12,
                CRGBA pixel20,
                CRGBA pixel21,
                CRGBA pixel22,
                out double R,
                out double G,
                out double B,
                out double Alpha
                )
            {
                CalcValueOfPixels(
                    pixel11.R, pixel11.G, pixel11.B, pixel11.Alpha,
                    out R, out G, out B, out Alpha
                    );
            }

            public void CalcValueOfPixels(
                double r, double g, double b, double a,
                out double R, out double G, out double B, out double Alpha
                )
            {
                if (func != null)
                {
                    R = func(r);
                    G = func(g);
                    B = func(b);
                }
                else
                {
                    R = r;
                    G = g;
                    B = b;
                }
                Alpha = a;
            }
        }

        [ScriptMethod(nameSpace7)]
        public static CRGBAContrastFilter CreateRGBAContrastFilter(Func<double, double> func)
        {
            return new CRGBAContrastFilter(func);
        }

        [ScriptMethod(nameSpace7)]
        public static CRGBAContrastFilter CreateRGBAContrastFilter(ContrastType type)
        {
            switch (type)
            {
                case ContrastType.InOutSine:
                    return new CRGBAContrastFilter((x) => EasingFunction.InOutSin(x));
                case ContrastType.Linear:
                    return new CRGBAContrastFilter((x) => EasingFunction.Linear(x));
                case ContrastType.OutExp:
                    return new CRGBAContrastFilter((x) => EasingFunction.OutExp(x));
                case ContrastType.OutCirc:
                    return new CRGBAContrastFilter((x) => EasingFunction.OutCirc(x));
                case ContrastType.OutSine:
                    return new CRGBAContrastFilter((x) => EasingFunction.OutSin(x));
                case ContrastType.InCubic:
                    return new CRGBAContrastFilter((x) => EasingFunction.InCubic(x));
                case ContrastType.InQuad:
                    return new CRGBAContrastFilter((x) => EasingFunction.InQuad(x));
                case ContrastType.OutQuad:
                    return new CRGBAContrastFilter((x) => EasingFunction.OutQuad(x));
            }
            return null;
        }

        //------------------------------------------------------------------
        /// <summary>
        /// 複数のIKernel3x3RGBAをひとまとめにするためのハブです。
        /// </summary>
        public class CRGBA3x3FilteringHub : IRGBA3x3Filter
        {
            IList<IRGBA3x3Filter> filters;

            public CRGBA3x3FilteringHub(IList<IRGBA3x3Filter> filters)
            {
                this.filters = filters;
            }

            public override string ToString()
            {
                string str = $"{this.GetType().FullName} -" + Environment.NewLine;
                foreach (var filter in filters)
                {
                    str += filter.ToString();
                }
                return str;
            }

            public void CalcValueOfPixels(
                CRGBA pixel00,
                CRGBA pixel01,
                CRGBA pixel02,
                CRGBA pixel10,
                CRGBA pixel11,
                CRGBA pixel12,
                CRGBA pixel20,
                CRGBA pixel21,
                CRGBA pixel22,
                out double R,
                out double G,
                out double B,
                out double Alpha
                )
            {
                R = pixel11.R;
                G = pixel11.G;
                B = pixel11.B;
                Alpha = pixel11.Alpha;
                foreach (var filter in filters)
                {
                    filter.CalcValueOfPixels(
                            pixel00, pixel01, pixel02, pixel10, pixel11, pixel12, pixel20, pixel21, pixel22,
                            out R, out G, out B, out Alpha
                        );
                }
            }
        }

        [ScriptMethod(nameSpace8)]
        public static CRGBA3x3FilteringHub CreateRGBA3x3FilteringHub(IList<IRGBA3x3Filter> list)
        {
            return new CRGBA3x3FilteringHub(list);
        }

        //------------------------------------------------------------------
        /// <summary>
        /// ハブの処理結果との差分を定義するクラスです。
        /// </summary>
        public class CRGBA3x3DiffFilter : IRGBA3x3Filter
        {
            CRGBA3x3FilteringHub hub;

            public CRGBA3x3DiffFilter(CRGBA3x3FilteringHub hub)
            {
                this.hub = hub;
            }

            public override string ToString()
            {
                string str = $"{this.GetType().FullName} -" + Environment.NewLine;
                str += hub.ToString();
                return str;
            }

            void IRGBA3x3Filter.CalcValueOfPixels(
                CRGBA pixel00,
                CRGBA pixel01,
                CRGBA pixel02,
                CRGBA pixel10,
                CRGBA pixel11,
                CRGBA pixel12,
                CRGBA pixel20,
                CRGBA pixel21,
                CRGBA pixel22,
                out double R,
                out double G,
                out double B,
                out double Alpha
                )
            {
                double tR;
                double tG;
                double tB;
                double tAlpha;

                hub.CalcValueOfPixels(
                        pixel00, pixel01, pixel02, pixel10, pixel11, pixel12, pixel20, pixel21, pixel22,
                        out tR, out tG, out tB, out tAlpha
                    );

                R = Math.Abs(pixel11.R - tR);
                G = Math.Abs(pixel11.G - tG);
                B = Math.Abs(pixel11.B - tB);
                Alpha = pixel11.Alpha;
            }
        }

        [ScriptMethod(nameSpace8)]
        public static CRGBA3x3DiffFilter CreateRGBA3x3DiffFilter(CRGBA3x3FilteringHub hub)
        {
            return new CRGBA3x3DiffFilter(hub);
        }

        [ScriptMethod(nameSpace8)]
        public static CRGBA3x3DiffFilter CreateRGBA3x3DiffFilter(IList<IRGBA3x3Filter> filters)
        {
            return new CRGBA3x3DiffFilter(CreateRGBA3x3FilteringHub(filters));
        }

        //------------------------------------------------------------------
        /// <summary>
        /// ３✕３のカーネルを定義するクラスです。
        /// </summary>
        public class CRGBA3x3FreeFilter : IRGBA3x3Filter
        {
            double D00 = 1.0;
            double D01 = 1.0;
            double D02 = 1.0;
            double D10 = 1.0;
            double D11 = 1.0;
            double D12 = 1.0;
            double D20 = 1.0;
            double D21 = 1.0;
            double D22 = 1.0;

            public CRGBA3x3FreeFilter(List<double> kernel, double div)
            {
                kernel = kernel.ConvertAll((o) => o / div);
                SetupKernel(kernel);
            }

            public CRGBA3x3FreeFilter(IList<double> kernel)
            {
                SetupKernel(kernel);
            }

            private void SetupKernel(IList<double> kernel)
            {
                for (int i = 0; i < kernel.Count; ++i)
                {
                    switch (i)
                    {
                        case 0: D00 = kernel[i]; break;
                        case 1: D01 = kernel[i]; break;
                        case 2: D02 = kernel[i]; break;
                        case 3: D10 = kernel[i]; break;
                        case 4: D11 = kernel[i]; break;
                        case 5: D12 = kernel[i]; break;
                        case 6: D20 = kernel[i]; break;
                        case 7: D21 = kernel[i]; break;
                        case 8: D22 = kernel[i]; break;
                    }
                }
            }
            public override string ToString()
            {
                return $"{this.GetType().FullName} -"
                    + Environment.NewLine
                    + $"{nameof(D00)}: {D00}, {nameof(D10)}: {D10}, {nameof(D20)}: {D20}" + Environment.NewLine
                    + $"{nameof(D01)}: {D01}, {nameof(D11)}: {D11}, {nameof(D21)}: {D21}" + Environment.NewLine
                    + $"{nameof(D02)}: {D02}, {nameof(D12)}: {D12}, {nameof(D22)}: {D22}"
                    ;
            }

            void IRGBA3x3Filter.CalcValueOfPixels(
                CRGBA pixel00,
                CRGBA pixel01,
                CRGBA pixel02,
                CRGBA pixel10,
                CRGBA pixel11,
                CRGBA pixel12,
                CRGBA pixel20,
                CRGBA pixel21,
                CRGBA pixel22,
                out double R,
                out double G,
                out double B,
                out double Alpha
                )
            {
                R = pixel00.R * D00 + pixel10.R * D10 + pixel20.R * D20 +
                    pixel01.R * D01 + pixel11.R * D11 + pixel21.R * D21 +
                    pixel02.R * D02 + pixel12.R * D12 + pixel22.R * D22;
                G = pixel00.G * D00 + pixel10.G * D10 + pixel20.G * D20 +
                    pixel01.G * D01 + pixel11.G * D11 + pixel21.G * D21 +
                    pixel02.G * D02 + pixel12.G * D12 + pixel22.G * D22;
                B = pixel00.B * D00 + pixel10.B * D10 + pixel20.B * D20 +
                    pixel01.B * D01 + pixel11.B * D11 + pixel21.B * D21 +
                    pixel02.B * D02 + pixel12.B * D12 + pixel22.B * D22;
                Alpha = pixel11.Alpha;
            }
        }

        [ScriptMethod(nameSpace8)]
        public static CRGBA3x3FreeFilter CreateRGBA3x3FreeFilter(IList<double> kernel)
        {
            return new CRGBA3x3FreeFilter(kernel);
        }

        [ScriptMethod(nameSpace8)]
        public static CRGBA3x3FreeFilter CreateRGBA3x3FreeFilter(List<double> kernel, double div)
        {
            return new CRGBA3x3FreeFilter(kernel, div);
        }

        //------------------------------------------------------------------
        /// <summary>
        /// 平滑化（平均）処理を定義するクラスです。
        /// </summary>
        [ScriptMethod(nameSpace8)]
        public static CRGBA3x3FreeFilter CreateRGBA3x3AverageFilter()
        {
            return CreateRGBA3x3FreeFilter(
                new List<double>()
                {
                    1.0, 1.0, 1.0,
                    1.0, 2.0, 1.0,
                    1.0, 1.0, 1.0
                }, 10.0
                );
        }

        //------------------------------------------------------------------
        /// <summary>
        /// 平滑化（ガウシアンフィルター）処理を定義するクラスです。
        /// </summary>
        [ScriptMethod(nameSpace8)]
        public static CRGBA3x3FreeFilter CreateRGBA3x3GaussianFilter()
        {
            return CreateRGBA3x3FreeFilter(
                new List<double>()
                {
                    1.0, 2.0, 1.0,
                    2.0, 4.0, 2.0,
                    1.0, 2.0, 1.0
                }, 16.0
                );
        }

        //------------------------------------------------------------------
        /// <summary>
        /// 輪郭抽出（一次微分フィルタ）処理を定義するクラスです。
        /// </summary>
        [ScriptMethod(nameSpace8)]
        public static CRGBA3x3FreeFilter CreateRGBA3x3FirstDerivativeFilter()
        {
            return CreateRGBA3x3FreeFilter(
                new List<double>()
                {
                     0.0, -1.0, 0.0,
                    -1.0,  0.0, 1.0,
                     0.0,  1.0, 0.0
                }
                );
        }

        //------------------------------------------------------------------
        /// <summary>
        /// 輪郭抽出（プレヴィットフィルタ）処理を定義するクラスです。
        /// </summary>
        [ScriptMethod(nameSpace8)]
        public static CRGBA3x3FreeFilter CreateRGBA3x3PrewittFilter()
        {
            return CreateRGBA3x3FreeFilter(
                new List<double>()
                {
                    -1.0, -1.0, -1.0,
                     0.0,  0.0,  0.0,
                     1.0,  1.0,  1.0
                }
                );
        }

        //------------------------------------------------------------------
        /// <summary>
        /// 輪郭抽出（ソーベルフィルタ）処理を定義するクラスです。
        /// </summary>
        [ScriptMethod(nameSpace8)]
        public static CRGBA3x3FreeFilter CreateRGBA3x3SobelFilter()
        {
            return CreateRGBA3x3FreeFilter(
                new List<double>()
                {
                    -1.0, -2.0, -1.0,
                     0.0,  0.0,  0.0,
                     1.0,  2.0,  1.0
                }
                );
        }

        //------------------------------------------------------------------
        /// <summary>
        /// 鮮鋭化（アンシャープマスキング）処理を定義するクラスです。
        /// </summary>
        [ScriptMethod(nameSpace8)]
        public static CRGBA3x3FreeFilter CreateRGBA3x3UnsharpMasking(double k = 2.0)
        {
            return CreateRGBA3x3FreeFilter(
                new List<double>()
                {
                    -k/9, -k/9, -k/9,
                    -k/9, 1+(8*k)/9, -k/9,
                    -k/9, -k/9, -k/9
                }
                );
        }

        //------------------------------------------------------------------
        public class CRGBA3x3DilationFilter : IRGBA3x3Filter
        {
            public CRGBA3x3DilationFilter() { }

            void IRGBA3x3Filter.CalcValueOfPixels(
                CRGBA pixel00,
                CRGBA pixel01,
                CRGBA pixel02,
                CRGBA pixel10,
                CRGBA pixel11,
                CRGBA pixel12,
                CRGBA pixel20,
                CRGBA pixel21,
                CRGBA pixel22,
                out double R,
                out double G,
                out double B,
                out double Alpha
                )
            {
                var listR = new List<double>() { pixel01.R, pixel10.R, pixel11.R, pixel12.R, pixel21.R };
                var listG = new List<double>() { pixel01.G, pixel10.G, pixel11.G, pixel12.G, pixel21.G };
                var listB = new List<double>() { pixel01.B, pixel10.B, pixel11.B, pixel12.B, pixel21.B };

                R = listR.Max();
                G = listG.Max();
                B = listB.Max();
                Alpha = pixel11.Alpha;
            }
        }

        /// <summary>
        /// 膨張処理を定義するクラスです。
        /// </summary>
        [ScriptMethod(nameSpace8)]
        public static IRGBA3x3Filter CreateRGBA3x3DilationFilter()
        {
            return new CRGBA3x3DilationFilter();
        }

        //------------------------------------------------------------------
        public class CRGBA3x3ErosionFilter : IRGBA3x3Filter
        {
            public CRGBA3x3ErosionFilter() { }

            void IRGBA3x3Filter.CalcValueOfPixels(
                CRGBA pixel00,
                CRGBA pixel01,
                CRGBA pixel02,
                CRGBA pixel10,
                CRGBA pixel11,
                CRGBA pixel12,
                CRGBA pixel20,
                CRGBA pixel21,
                CRGBA pixel22,
                out double R,
                out double G,
                out double B,
                out double Alpha
                )
            {
                var listR = new List<double>() { pixel01.R, pixel10.R, pixel11.R, pixel12.R, pixel21.R };
                var listG = new List<double>() { pixel01.G, pixel10.G, pixel11.G, pixel12.G, pixel21.G };
                var listB = new List<double>() { pixel01.B, pixel10.B, pixel11.B, pixel12.B, pixel21.B };

                R = listR.Min();
                G = listG.Min();
                B = listB.Min();
                Alpha = pixel11.Alpha;
            }
        }

        /// <summary>
        /// 収縮処理を定義するクラスです。
        /// </summary>
        [ScriptMethod(nameSpace8)]
        public static IRGBA3x3Filter CreateRGBA3x3ErosionFilter()
        {
            return new CRGBA3x3ErosionFilter();
        }

        //------------------------------------------------------------------
        /// <summary>
        /// 平滑化（メディアンフィルター）処理を定義するクラスです。
        /// </summary>
        public class CRGBA3x3MedianFilter : IRGBA3x3Filter
        {
            public CRGBA3x3MedianFilter() { }

            void IRGBA3x3Filter.CalcValueOfPixels(
                CRGBA pixel00,
                CRGBA pixel01,
                CRGBA pixel02,
                CRGBA pixel10,
                CRGBA pixel11,
                CRGBA pixel12,
                CRGBA pixel20,
                CRGBA pixel21,
                CRGBA pixel22,
                out double R,
                out double G,
                out double B,
                out double Alpha
                )
            {
                var sort = new List<Tuple<int, double>>()
                {
                    new Tuple<int, double>(0, (pixel00.R + pixel00.G + pixel00.B) / 3.0),
                    new Tuple<int, double>(1, (pixel01.R + pixel01.G + pixel01.B) / 3.0),
                    new Tuple<int, double>(2, (pixel02.R + pixel02.G + pixel02.B) / 3.0), 
                    new Tuple<int, double>(3, (pixel10.R + pixel10.G + pixel10.B) / 3.0), 
                    new Tuple<int, double>(4, (pixel11.R + pixel11.G + pixel11.B) / 3.0), 
                    new Tuple<int, double>(5, (pixel12.R + pixel12.G + pixel12.B) / 3.0), 
                    new Tuple<int, double>(6, (pixel20.R + pixel20.G + pixel20.B) / 3.0), 
                    new Tuple<int, double>(7, (pixel21.R + pixel21.G + pixel21.B) / 3.0),
                    new Tuple<int, double>(8, (pixel22.R + pixel22.G + pixel22.B) / 3.0)
                };
                sort.Sort((a, b) => {
                    if (a.Item2 > b.Item2)
                        return 1;
                    if (a.Item2 < b.Item2)
                        return -1;
                    return 0;
                    });

                int medianIndex = sort[9 / 2].Item1;    // 中央値のインデックスを取得

                var listR = new double[9] { pixel00.R, pixel01.R, pixel02.R, pixel10.R, pixel11.R, pixel12.R, pixel20.R, pixel21.R, pixel22.R };
                var listG = new double[9] { pixel00.G, pixel01.G, pixel02.G, pixel10.G, pixel11.G, pixel12.G, pixel20.G, pixel21.G, pixel22.G };
                var listB = new double[9] { pixel00.B, pixel01.B, pixel02.B, pixel10.B, pixel11.B, pixel12.B, pixel20.B, pixel21.B, pixel22.B };
                var listAlpha = new double[9] { pixel00.Alpha, pixel01.Alpha, pixel02.Alpha, pixel10.Alpha, pixel11.Alpha, pixel12.Alpha, pixel20.Alpha, pixel21.Alpha, pixel22.Alpha };

                R = listR[medianIndex];
                G = listG[medianIndex];
                B = listB[medianIndex];
                Alpha = listAlpha[medianIndex];
            }
        }

        [ScriptMethod(nameSpace8)]
        public static CRGBA3x3MedianFilter CreateRGBA3x3MedianFilter()
        {
            return new CRGBA3x3MedianFilter();
        }

        //------------------------------------------------------------------
        [ScriptMethod(nameSpace8)]
        public static IRGBA3x3Filter Create3x3Filter(K3x3FilterType type)
        {
            IRGBA3x3Filter filter = null;
            switch (type)
            {
                case K3x3FilterType.Average:
                    filter = CreateRGBA3x3AverageFilter();
                    break;
                case K3x3FilterType.Gaussian:
                    filter = CreateRGBA3x3GaussianFilter();
                    break;
                case K3x3FilterType.FirstDerivative:
                    filter = CreateRGBA3x3FirstDerivativeFilter();
                    break;
                case K3x3FilterType.Prewitt:
                    filter = CreateRGBA3x3PrewittFilter();
                    break;
                case K3x3FilterType.Sobel:
                    filter = CreateRGBA3x3SobelFilter();
                    break;
                case K3x3FilterType.Median:
                    filter = CreateRGBA3x3MedianFilter();
                    break;
                case K3x3FilterType.Dilation:
                    filter = CreateRGBA3x3DilationFilter();
                    break;
                case K3x3FilterType.Erosion:
                    filter = CreateRGBA3x3ErosionFilter();
                    break;
            }
            return filter;
        }
    }
}
