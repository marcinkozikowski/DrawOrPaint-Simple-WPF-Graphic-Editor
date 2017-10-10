using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace DrawOrPaint
{
    enum DrawType { nothing, line, ellipse, rectangle };

    class CanvasTools
    {
        public Canvas canvas;  //Danh sách các hình trên trang
        public DrawType drawType; //Kiểu vẽ hiện tại.

        public CanvasTools(Canvas c)
        {
            drawType = DrawType.nothing;
            canvas = c;
        }

        public void DrawCapture(Shape shape) //Vẽ hình được kéo ra lúc chưa nhả chuột
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
    }
}
