using AForge.Imaging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
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

namespace DrawOrPaint
{
    public partial class Histogram_Window : Window, INotifyPropertyChanged
    {
        private PointCollection luminanceHistogramPoints = null;
        public event PropertyChangedEventHandler PropertyChanged;
        private CanvasTools canvasTool = new CanvasTools(((MainWindow)System.Windows.Application.Current.MainWindow).main_canvas);

        public PointCollection LuminanceHistogramPoints
        {
            get
            {
                return this.luminanceHistogramPoints;
            }
            set
            {
                if (this.luminanceHistogramPoints != value)
                {
                    this.luminanceHistogramPoints = value;
                    if (this.PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("LuminanceHistogramPoints"));
                    }
                }
            }
        }

        public PointCollection calculateHistogram(Bitmap image)
        {
            ImageStatisticsHSL hslStatistics = new ImageStatisticsHSL(image);
            this.LuminanceHistogramPoints = canvasTool.ConvertToPointCollection(hslStatistics.Luminance.Values);

            return LuminanceHistogramPoints;
        }


        public Histogram_Window()
        {
            InitializeComponent();
            calculateHistogram(canvasTool.getBitmapFromCanvas());
            histoPolygon.Points = LuminanceHistogramPoints;
        }

        private void HistogramEqualization_Click(object sender, RoutedEventArgs e)
        {
            Bitmap bmp = canvasTool.Equalize(canvasTool.getBitmapFromCanvas());
            canvasTool.SetBmpImageToCanvas(bmp);
            calculateHistogram(canvasTool.getBitmapFromCanvas());
            histoPolygon.Points = LuminanceHistogramPoints;
        }

        private void HistogramStretch_Click(object sender, RoutedEventArgs e)
        {
            Bitmap bmp = canvasTool.HistogramStretch(canvasTool.getBitmapFromCanvas());
            canvasTool.SetBmpImageToCanvas(bmp);
            calculateHistogram(canvasTool.getBitmapFromCanvas());
            histoPolygon.Points = LuminanceHistogramPoints;
        }
    }
}
