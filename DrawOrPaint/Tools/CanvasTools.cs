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
                    Image MyImg = new Image();
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
                    Image MyImg = new Image();

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
            RenderTargetBitmap renderBitmap = new RenderTargetBitmap(
             (int)canvas.Width, (int)canvas.Height,
             96d, 96d, PixelFormats.Pbgra32);
            // needed otherwise the image output is black
            canvas.Measure(new Size((int)canvas.Width, (int)canvas.Height));
            canvas.Arrange(new Rect(new Size((int)canvas.Width, (int)canvas.Height)));

            renderBitmap.Render(canvas);

            JpegBitmapEncoder encoder = new JpegBitmapEncoder();
            encoder.QualityLevel = quality;
            encoder.Frames.Add(BitmapFrame.Create(renderBitmap));

            using (FileStream file = File.Create(filename))
            {
                encoder.Save(file);
            }
        }
    }
}
