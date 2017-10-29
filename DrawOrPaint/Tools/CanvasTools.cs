using DrawOrPaint.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Drawing.Drawing2D;
using System.Drawing;

namespace DrawOrPaint
{
    enum DrawType { nothing, line, ellipse, rectangle };

    class CanvasTools
    {
        public Canvas canvas;  
        public DrawType drawType;

        public CanvasTools(Canvas c)
        {
            drawType = DrawType.nothing;
            canvas = c;
        }

        public void DrawCapture(Shape shape) 
        {
            double[] dashes = { 2, 2 };
            shape.StrokeDashArray = new System.Windows.Media.DoubleCollection(dashes);
            canvas.Children.Add(shape);
        }
        public void DrawShape(Shape shape, bool manual)
        {
            if (manual)
            {
                canvas.Children.RemoveAt(canvas.Children.Count - 1);
            }
            canvas.Children.Add(shape);
        }

        public void RemoveShape(Shape shape)
        {
            canvas.Children.Remove(shape);
        }

        public void OpenNewImageFile(string filePath)
        {
            Loading_Window lw = new Loading_Window();
            lw.Show();
            try
            {
                if (filePath.Contains(".ppm"))
                {
                    var bmp = PixelMap2.Load(filePath);

                    System.Drawing.Bitmap bmap = bmp;

                    IntPtr hBitmap = bmap.GetHbitmap();
                    System.Windows.Controls.Image MyImg = new System.Windows.Controls.Image();
                    MyImg.Source = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(hBitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());

                    canvas.Width = bmp.Width;
                    canvas.Height = bmp.Height;
                    canvas.Children.Clear();
                    canvas.Children.Add(MyImg);
                    RenderOptions.SetBitmapScalingMode(canvas, BitmapScalingMode.NearestNeighbor);
                }
                else if (filePath.Contains(".jpg"))
                {
                    ImageBrush brush = new ImageBrush();
                    System.Windows.Controls.Image MyImg = new System.Windows.Controls.Image();

                    Stream imageStreamSource = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                    JpegBitmapDecoder decoder = new JpegBitmapDecoder(imageStreamSource, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);
                    BitmapSource bitmapSource = decoder.Frames[0];

                    MyImg.Source = bitmapSource;
                    canvas.Children.Clear();
                    canvas.Width = bitmapSource.Width;
                    canvas.Height = bitmapSource.Height;
                    canvas.Children.Add(MyImg);

                    //((MainWindow)Application.Current.MainWindow).currentFileLabel.Content = "Res: " + bitmapSource.Width + "x" + bitmapSource.Height;
                }
            }
            catch(Exception ex)
            {
                ((MainWindow)Application.Current.MainWindow).ShowException(ex.Message);
            }
            lw.Close();
        }

        public void SaveJpegFile(Canvas canvas, string filename,int quality)
        {
            Rect b = VisualTreeHelper.GetDescendantBounds(canvas);

            /// new a RenderTargetBitmap with actual size of c


            Transform transform = canvas.LayoutTransform;

            canvas.LayoutTransform = null;

            System.Windows.Size size = new System.Windows.Size(canvas.Width, canvas.Height);

            canvas.Measure(size);
            canvas.Arrange(new Rect(size));

            RenderTargetBitmap renderBitmap = new RenderTargetBitmap(
    (int)b.Width, (int)b.Height,
    96, 96, PixelFormats.Pbgra32);

            renderBitmap.Render(canvas);

            JpegBitmapEncoder encoder = new JpegBitmapEncoder();
            encoder.QualityLevel = quality;
            encoder.Frames.Add(BitmapFrame.Create(renderBitmap));

            using (FileStream file = File.Create(filename))
            {
                encoder.Save(file);
            }
        }
        public Bitmap SetBrightness(int brightness,Bitmap _currentBitmap)
        {
            Bitmap temp = (Bitmap)_currentBitmap;
            Bitmap bmap = (Bitmap)temp.Clone();
            if (brightness < -255) brightness = -255;
            if (brightness > 255) brightness = 255;
            System.Drawing.Color c;
            for (int i = 0; i < bmap.Width; i++)
            {
                for (int j = 0; j < bmap.Height; j++)
                {
                    c = bmap.GetPixel(i, j);
                    int cR = c.R + brightness;
                    int cG = c.G + brightness;
                    int cB = c.B + brightness;

                    if (cR < 0) cR = 1;
                    if (cR > 255) cR = 255;

                    if (cG < 0) cG = 1;
                    if (cG > 255) cG = 255;

                    if (cB < 0) cB = 1;
                    if (cB > 255) cB = 255;

                    bmap.SetPixel(i, j, System.Drawing.Color.FromArgb((byte)cR, (byte)cG, (byte)cB));
                }
            }
            return _currentBitmap = (Bitmap)bmap.Clone();
        }

        public Bitmap SetAddition(int value, Bitmap _currentBitmap)
        {
            Bitmap temp = (Bitmap)_currentBitmap;
            Bitmap bmap = (Bitmap)temp.Clone();
            if (value < -255) value = -255;
            if (value > 255) value = 255;
            System.Drawing.Color c;
            for (int i = 0; i < bmap.Width; i++)
            {
                for (int j = 0; j < bmap.Height; j++)
                {
                    c = bmap.GetPixel(i, j);
                    int cR = c.R + value;
                    int cG = c.G + value;
                    int cB = c.B + value;

                    if (cR < 0) cR = 1;
                    if (cR > 255) cR = 255;

                    if (cG < 0) cG = 1;
                    if (cG > 255) cG = 255;

                    if (cB < 0) cB = 1;
                    if (cB > 255) cB = 255;

                    bmap.SetPixel(i, j, System.Drawing.Color.FromArgb((byte)cR, (byte)cG, (byte)cB));
                }
            }
            return _currentBitmap = (Bitmap)bmap.Clone();
        }

        public Bitmap SetSubtraction(int value, Bitmap _currentBitmap)
        {
            Bitmap temp = (Bitmap)_currentBitmap;
            Bitmap bmap = (Bitmap)temp.Clone();
            if (value < -255) value = -255;
            if (value > 255) value = 255;
            System.Drawing.Color c;
            for (int i = 0; i < bmap.Width; i++)
            {
                for (int j = 0; j < bmap.Height; j++)
                {
                    c = bmap.GetPixel(i, j);
                    int cR = c.R - value;
                    int cG = c.G - value;
                    int cB = c.B - value;

                    if (cR < 0) cR = 1;
                    if (cR > 255) cR = 255;

                    if (cG < 0) cG = 1;
                    if (cG > 255) cG = 255;

                    if (cB < 0) cB = 1;
                    if (cB > 255) cB = 255;

                    bmap.SetPixel(i, j, System.Drawing.Color.FromArgb((byte)cR, (byte)cG, (byte)cB));
                }
            }
            return _currentBitmap = (Bitmap)bmap.Clone();
        }

        public Bitmap SetMultiplication(int value, Bitmap _currentBitmap)
        {
            Bitmap temp = (Bitmap)_currentBitmap;
            Bitmap bmap = (Bitmap)temp.Clone();
            if (value < -255) value = -255;
            if (value > 255) value = 255;
            System.Drawing.Color c;
            for (int i = 0; i < bmap.Width; i++)
            {
                for (int j = 0; j < bmap.Height; j++)
                {
                    c = bmap.GetPixel(i, j);
                    int cR = c.R * value;
                    int cG = c.G * value;
                    int cB = c.B * value;

                    if (cR < 0) cR = 1;
                    if (cR > 255) cR = 255;

                    if (cG < 0) cG = 1;
                    if (cG > 255) cG = 255;

                    if (cB < 0) cB = 1;
                    if (cB > 255) cB = 255;

                    bmap.SetPixel(i, j, System.Drawing.Color.FromArgb((byte)cR, (byte)cG, (byte)cB));
                }
            }
            return _currentBitmap = (Bitmap)bmap.Clone();
        }

        public Bitmap SetDivision(int value, Bitmap _currentBitmap)
        {
            Bitmap temp = (Bitmap)_currentBitmap;
            Bitmap bmap = (Bitmap)temp.Clone();
            if (value < 1) value = 1;
            if (value > 255) value = 255;
            System.Drawing.Color c;
            for (int i = 0; i < bmap.Width; i++)
            {
                for (int j = 0; j < bmap.Height; j++)
                {
                    c = bmap.GetPixel(i, j);
                    int cR = c.R / value;
                    int cG = c.G / value;
                    int cB = c.B / value;

                    if (cR < 0) cR = 1;
                    if (cR > 255) cR = 255;

                    if (cG < 0) cG = 1;
                    if (cG > 255) cG = 255;

                    if (cB < 0) cB = 1;
                    if (cB > 255) cB = 255;

                    bmap.SetPixel(i, j, System.Drawing.Color.FromArgb((byte)cR, (byte)cG, (byte)cB));
                }
            }
            return _currentBitmap = (Bitmap)bmap.Clone();
        }

        public Bitmap SetGrayscale(Bitmap _currentBitmap)
        {
            Bitmap temp = (Bitmap)_currentBitmap;
            Bitmap bmap = (Bitmap)temp.Clone();
            System.Drawing.Color c;
            for (int i = 0; i < bmap.Width; i++)
            {
                for (int j = 0; j < bmap.Height; j++)
                {
                    c = bmap.GetPixel(i, j);
                    byte gray = (byte)(.299 * c.R + .587 * c.G + .114 * c.B);

                    bmap.SetPixel(i, j, System.Drawing.Color.FromArgb(gray, gray, gray));
                }
            }
            return _currentBitmap = (Bitmap)bmap.Clone();
        }

        public Bitmap getBitmapFromCanvas()
        {
            Bitmap bmpCanvas;

            Rect b = VisualTreeHelper.GetDescendantBounds(canvas);

            Transform transform = canvas.LayoutTransform;

            canvas.LayoutTransform = null;

            System.Windows.Size size = new System.Windows.Size(canvas.Width, canvas.Height);

            canvas.Measure(size);
            canvas.Arrange(new Rect(size));

            RenderTargetBitmap renderBitmap = new RenderTargetBitmap(
    (int)b.Width, (int)b.Height,
    96, 96, PixelFormats.Pbgra32);

            renderBitmap.Render(canvas);
            MemoryStream stream = new MemoryStream();

            BitmapEncoder encoder = new BmpBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(renderBitmap));
            encoder.Save(stream);

            bmpCanvas = new Bitmap(stream);

            return bmpCanvas;
        }
    }
}
