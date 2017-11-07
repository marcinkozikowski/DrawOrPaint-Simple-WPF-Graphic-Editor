using AForge.Imaging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    /// <summary>
    /// Interaction logic for Histogram_Window.xaml
    /// </summary>
    public partial class Histogram_Window : Window, INotifyPropertyChanged
    {
        private PointCollection luminanceHistogramPoints = null;
        public event PropertyChangedEventHandler PropertyChanged;
        private CanvasTools canvasTool;

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

        public Histogram_Window()
        {
            InitializeComponent();
            canvasTool = new CanvasTools(((MainWindow)System.Windows.Application.Current.MainWindow).main_canvas);
            // Luminance
            ImageStatisticsHSL hslStatistics = new ImageStatisticsHSL(canvasTool.getBitmapFromCanvas());
            this.LuminanceHistogramPoints = canvasTool.ConvertToPointCollection(hslStatistics.Luminance.Values);

            histoPolygon.Points = LuminanceHistogramPoints;
            // RGB
            //ImageStatistics rgbStatistics = new ImageStatistics(bmp);
            //this.RedColorHistogramPoints = ConvertToPointCollection(rgbStatistics.Red.Values);
            //this.GreenColorHistogramPoints = ConvertToPointCollection(rgbStatistics.Green.Values);
            //this.BlueColorHistogramPoints = ConvertToPointCollection(rgbStatistics.Blue.Values);
        }
    }
}
