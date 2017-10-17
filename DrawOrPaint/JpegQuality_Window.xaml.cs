using Microsoft.Win32;
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

namespace DrawOrPaint
{
    /// <summary>
    /// Interaction logic for JpegQuality_Window.xaml
    /// </summary>
    public partial class JpegQuality_Window : Window
    {
        private int quality = 80;

        public JpegQuality_Window()
        {
            InitializeComponent();
        }

        private void qualitySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
           quality = int.Parse(qualitySlider.Value.ToString());
        }

        private void JpegQualityOk_Click(object sender, RoutedEventArgs e)
        {
            //((MainWindow)this.Owner).OpenSaveFileDialog();
            //((MainWindow)Application.Current.MainWindow).Sh
            

            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "Jpeg Image|*.jpg";
            saveFileDialog1.Title = "Save Image File";
            saveFileDialog1.ShowDialog();
            this.Close();

            // If the file name is not an empty string open it for saving.
            if (saveFileDialog1.FileName != "")
            {
                CanvasTools ct = new CanvasTools(((MainWindow)Application.Current.MainWindow).main_canvas);
                ct.SaveJpegFile(((MainWindow)Application.Current.MainWindow).main_canvas, saveFileDialog1.FileName, quality);

            }
        }
    }
}
