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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DrawOrPaint
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private enum HitType
        {
            None, Body, UL, UR, LR, LL, L, R, T, B
        };

        private CanvasTools canvasTool;
        private Point mDown, mMove;
        private Point LastPoint;
        private Shape currentShape;
        private Shape selectedShape;
        private Line currentLine;
        private Color fillColor, penColor;
        HitType currentHitType = HitType.None;
        private double penThickness;
        private bool capture;
        private bool addShape;
        private bool dragInProgress = false;

        public MainWindow()
        {
            InitializeComponent();
            canvasTool = new CanvasTools(main_canvas);
            penColor = Colors.Black;
            fillColor = Colors.Black;
            penThickness = 5;
        }
        Color buttonBackground = Color.FromRgb(System.Convert.ToByte(221), System.Convert.ToByte(221), System.Convert.ToByte(221));

        #region MouseHandlers

        private void SetMouseCursor()
        {
            // See what cursor we should display.
            
            Cursor desired_cursor = Cursors.Arrow;
            switch (currentHitType)
            {
                case HitType.None:
                    desired_cursor = Cursors.Arrow;
                    break;
                case HitType.Body:
                    desired_cursor = Cursors.ScrollAll;
                    break;
                case HitType.UL:
                case HitType.LR:
                    desired_cursor = Cursors.SizeNWSE;
                    break;
                case HitType.LL:
                case HitType.UR:
                    desired_cursor = Cursors.SizeNESW;
                    break;
                case HitType.T:
                case HitType.B:
                    desired_cursor = Cursors.SizeNS;
                    break;
                case HitType.L:
                case HitType.R:
                    desired_cursor = Cursors.SizeWE;
                    break;
            }
            // Display the desired cursor.
            if (Cursor != desired_cursor) Cursor = desired_cursor;
            main_canvas.Cursor = desired_cursor;
        }
        // Return a HitType value to indicate what is at the point.
        private HitType SetHitType(Shape rect, Point point)
        {
            if (selectedShape is Rectangle)
            {
                double left = Canvas.GetLeft((selectedShape as Rectangle));
                double top = Canvas.GetTop((selectedShape as Rectangle));
                double right = left + (selectedShape as Rectangle).Width;
                double bottom = top + (selectedShape as Rectangle).Height;
                if (point.X < left) return HitType.None;
                if (point.X > right) return HitType.None;
                if (point.Y < top) return HitType.None;
                if (point.Y > bottom) return HitType.None;

                const double GAP = 10;
                if (point.X - left < GAP)
                {
                    // Left edge.
                    if (point.Y - top < GAP) return HitType.UL;
                    if (bottom - point.Y < GAP) return HitType.LL;
                    return HitType.L;
                }
                if (right - point.X < GAP)
                {
                    // Right edge.
                    if (point.Y - top < GAP) return HitType.UR;
                    if (bottom - point.Y < GAP) return HitType.LR;
                    return HitType.R;
                }
                if (point.Y - top < GAP) return HitType.T;
                if (bottom - point.Y < GAP) return HitType.B;
                return HitType.Body;
            }
            else if(selectedShape is Ellipse)
            {
                double left = Canvas.GetLeft((selectedShape as Ellipse));
                double top = Canvas.GetTop((selectedShape as Ellipse));
                double right = left + (selectedShape as Ellipse).Width;
                double bottom = top + (selectedShape as Ellipse).Height;
                if (point.X < left) return HitType.None;
                if (point.X > right) return HitType.None;
                if (point.Y < top) return HitType.None;
                if (point.Y > bottom) return HitType.None;

                const double GAP = 10;
                if (point.X - left < GAP)
                {
                    // Left edge.
                    if (point.Y - top < GAP) return HitType.UL;
                    if (bottom - point.Y < GAP) return HitType.LL;
                    return HitType.L;
                }
                if (right - point.X < GAP)
                {
                    // Right edge.
                    if (point.Y - top < GAP) return HitType.UR;
                    if (bottom - point.Y < GAP) return HitType.LR;
                    return HitType.R;
                }
                if (point.Y - top < GAP) return HitType.T;
                if (bottom - point.Y < GAP) return HitType.B;
                return HitType.Body;
            }
            else if (selectedShape is Line)
            {
                double left = (selectedShape as Line).X1;
                double top = (selectedShape as Line).Y1;
                double right = (selectedShape as Line).X2;
                double bottom = (selectedShape as Line).Y2;

                double width = Math.Abs(right - left);
                double height = Math.Abs(bottom - top);

                if (point.X < left) return HitType.None;
                if (point.X > right) return HitType.None;
                if (point.Y < top) return HitType.None;
                if (point.Y > bottom) return HitType.None;

                const double GAP = 1;
                if (point.X == left)
                {
                    // Left edge.
                    if (point.Y - top < GAP) return HitType.UL;
                    if (bottom - point.Y < GAP) return HitType.LL;
                    return HitType.L;
                }
                if (right == point.X)
                {
                    // Right edge.
                    if (point.Y - top < GAP) return HitType.UR;
                    if (bottom - point.Y < GAP) return HitType.LR;
                    return HitType.R;
                }
                if (point.X >= Math.Min(left, right) && point.Y >= Math.Min(top, bottom))
                {
                    return HitType.Body;
                }
                return HitType.None;
            }
            else return HitType.None;
        }

        private void main_canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            mDown = e.GetPosition(this.main_canvas);
            capture = true;

            if (selectedShape != null)
            {
                if(selectedShape is Rectangle)
                {
                    currentHitType = SetHitType((Rectangle)selectedShape, Mouse.GetPosition(main_canvas));
                    SetMouseCursor();
                    if (currentHitType == HitType.None) return;

                    LastPoint = Mouse.GetPosition(main_canvas);
                    dragInProgress = true;
                }
                else if(selectedShape is Ellipse)
                {
                    currentHitType = SetHitType((Ellipse)selectedShape, Mouse.GetPosition(main_canvas));
                    SetMouseCursor();
                    if (currentHitType == HitType.None) return;

                    LastPoint = Mouse.GetPosition(main_canvas);
                    dragInProgress = true;
                }
                else if(selectedShape is Line)
                {
                    currentHitType = SetHitType((Line)selectedShape, Mouse.GetPosition(main_canvas));
                    SetMouseCursor();
                    if (currentHitType == HitType.None) return;

                    LastPoint = Mouse.GetPosition(main_canvas);
                    dragInProgress = true;
                }

            }
        }

        private void main_canvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            dragInProgress = false;
            capture = false;
            if (currentShape != null)
            {
                if (canvasTool.drawType == DrawType.ellipse || canvasTool.drawType == DrawType.rectangle)
                {
                    Shape temp = null;
                    if (canvasTool.drawType == DrawType.ellipse)
                    {
                        temp = new Ellipse();
                    }
                    else if (canvasTool.drawType == DrawType.rectangle)
                    {
                        temp = new Rectangle();
                    }
                    temp.Stroke = new SolidColorBrush(penColor);
                    temp.StrokeThickness = penThickness;
                    temp.IsHitTestVisible = true;
                    temp.Width = currentShape.Width;
                    temp.Height = currentShape.Height;
                    Canvas.SetLeft(temp, currentShape.Margin.Left);
                    Canvas.SetTop(temp, currentShape.Margin.Top);
                    canvasTool.DrawShape(temp,true);
                }

                currentShape = null;
            }
            else if (canvasTool.drawType == DrawType.line && currentLine != null)
            {

                Line line = new Line();
                line.Stroke = new SolidColorBrush(penColor);
                line.StrokeThickness = penThickness;

                line.X1 = currentLine.X1;
                line.X2 = currentLine.X2;
                line.Y1 = currentLine.Y1;
                line.Y2 = currentLine.Y2;

                line.IsHitTestVisible = true;

                Canvas.SetLeft(line, currentLine.Margin.Left);
                Canvas.SetTop(line, currentLine.Margin.Right);

                canvasTool.DrawShape(line,true);
            }
            currentLine = null;
        }

        private void main_canvas_MouseMove(object sender, MouseEventArgs e)
        {
            #region ChangePlace

            if(dragInProgress==false && selectedShape != null)
            {
                if (selectedShape is Rectangle)
                {
                    currentHitType = SetHitType((Rectangle)selectedShape, Mouse.GetPosition(main_canvas));
                }
                else if(selectedShape is Ellipse)
                {
                    currentHitType = SetHitType((Ellipse)selectedShape, Mouse.GetPosition(main_canvas));
                }
                else if (selectedShape is Line)
                {
                    currentHitType = SetHitType((Line)selectedShape, Mouse.GetPosition(main_canvas));
                }
                SetMouseCursor();
            } 
            else if(selectedShape !=null && dragInProgress==true)
            {
                // See how much the mouse has moved.
                Point point = Mouse.GetPosition(main_canvas);

                double offset_x = point.X - LastPoint.X;
                double offset_y = point.Y - LastPoint.Y;

                double new_x=0;
                double new_y=0;
                double new_width=0;
                double new_height=0;
                double new_x2=0;
                double new_y2=0;

                // Get the rectangle's current position.
                if (selectedShape is Rectangle)
                {
                    new_x = Canvas.GetLeft((Rectangle)selectedShape);
                    new_y = Canvas.GetTop((Rectangle)selectedShape);
                    new_width = (selectedShape as Rectangle).Width;
                    new_height = (selectedShape as Rectangle).Height;
                 }
                else if (selectedShape is Ellipse)
                {
                    new_x = Canvas.GetLeft((Ellipse)selectedShape);
                    new_y = Canvas.GetTop((Ellipse)selectedShape);
                    new_width = (selectedShape as Ellipse).Width;
                    new_height = (selectedShape as Ellipse).Height;
                }
                else if (selectedShape is Line)
                {
                    new_x = (selectedShape as Line).X1;
                    new_y = (selectedShape as Line).Y1;
                    new_x2 = (selectedShape as Line).X2;
                    new_y2 = (selectedShape as Line).Y2;
                }

                // Update the rectangle or elipse
                #region Update Shape Position
                if (selectedShape is Rectangle || selectedShape is Ellipse)
                {
                    switch (currentHitType)
                    {
                        case HitType.Body:
                            new_x += offset_x;
                            new_y += offset_y;
                            break;
                        case HitType.UL:
                            new_x += offset_x;
                            new_y += offset_y;
                            new_width -= offset_x;
                            new_height -= offset_y;
                            break;
                        case HitType.UR:
                            new_y += offset_y;
                            new_width += offset_x;
                            new_height -= offset_y;
                            break;
                        case HitType.LR:
                            new_width += offset_x;
                            new_height += offset_y;
                            break;
                        case HitType.LL:
                            new_x += offset_x;
                            new_width -= offset_x;
                            new_height += offset_y;
                            break;
                        case HitType.L:
                            new_x += offset_x;
                            new_width -= offset_x;
                            break;
                        case HitType.R:
                            new_width += offset_x;
                            break;
                        case HitType.B:
                            new_height += offset_y;
                            break;
                        case HitType.T:
                            new_y += offset_y;
                            new_height -= offset_y;
                            break;
                    }
                }
                else if(selectedShape is Line)
                {
                    switch (currentHitType)
                    {
                        case HitType.Body:
                            new_x += offset_x;
                            new_y += offset_y;
                            new_x2 += offset_x;
                            new_y2 += offset_y;
                            break;
                        //case HitType.UL:
                        //    new_x += offset_x;
                        //    new_y += offset_y;
                        //    new_width -= offset_x;
                        //    new_height -= offset_y;
                        //    break;
                        //case HitType.UR:
                        //    new_y += offset_y;
                        //    new_width += offset_x;
                        //    new_height -= offset_y;
                        //    break;
                        //case HitType.LR:
                        //    new_width += offset_x;
                        //    new_height += offset_y;
                        //    break;
                        //case HitType.LL:
                        //    new_x += offset_x;
                        //    new_width -= offset_x;
                        //    new_height += offset_y;
                        //    break;
                        //case HitType.L:
                        //    new_x += offset_x;
                        //    new_width -= offset_x;
                        //    break;
                        //case HitType.R:
                        //    new_width += offset_x;
                        //    break;
                        //case HitType.B:
                        //    new_height += offset_y;
                        //    break;
                        //case HitType.T:
                        //    new_y += offset_y;
                        //    new_height -= offset_y;
                        //    break;
                    }
                }
                #endregion

                // Don't use negative width or height.
                if ((new_width > 0) && (new_height > 0))
                {
                    if(selectedShape is Rectangle)
                    {
                        Canvas.SetLeft((Rectangle)selectedShape, new_x);
                        Canvas.SetTop((Rectangle)selectedShape, new_y);
                        (selectedShape as Rectangle).Width = new_width;
                        (selectedShape as Rectangle).Height = new_height;
                    }
                    else if(selectedShape is Ellipse)
                    {
                        Canvas.SetLeft((Ellipse)selectedShape, new_x);
                        Canvas.SetTop((Ellipse)selectedShape, new_y);
                        (selectedShape as Ellipse).Width = new_width;
                        (selectedShape as Ellipse).Height = new_height;
                    }
                    // Save the mouse's new location.
                    LastPoint = point;
                }
                if(selectedShape is Line)
                {
                    (selectedShape as Line).X1 = new_x;
                    (selectedShape as Line).Y1 = new_y;
                    (selectedShape as Line).X2 = new_x2;
                    (selectedShape as Line).Y2 = new_y2;

                    LastPoint = point;
                }
            }
            #endregion

            #region Draw_NewShape

            mMove = e.GetPosition(this.main_canvas);
            addShape = false;
            if ((canvasTool.drawType == DrawType.ellipse || canvasTool.drawType == DrawType.rectangle) && capture)
            {

                if (currentShape == null)
                {
                    if (canvasTool.drawType == DrawType.ellipse)
                    {
                        currentShape = new Ellipse();
                    }
                    else if (canvasTool.drawType == DrawType.rectangle)
                    {
                        currentShape = new Rectangle();
                    }
                    addShape = true;
                    currentShape.StrokeThickness = penThickness;
                    currentShape.Stroke = new SolidColorBrush(penColor);
                }

                if (mMove.X <= mDown.X && mMove.Y <= mDown.Y)
                {
                    currentShape.Margin = new Thickness(mMove.X, mMove.Y, 0, 0);
                }
                else if (mMove.X >= mDown.X && mMove.Y <= mDown.Y)
                {
                    currentShape.Margin = new Thickness(mDown.X, mMove.Y, 0, 0);
                }
                else if (mMove.X >= mDown.X && mMove.Y >= mDown.Y)
                {
                    currentShape.Margin = new Thickness(mDown.X, mDown.Y, 0, 0);
                }
                else if (mMove.X <= mDown.X && mMove.Y >= mDown.Y)
                {
                    currentShape.Margin = new Thickness(mMove.X, mDown.Y, 0, 0);
                }

                currentShape.Width = Math.Abs(mMove.X - mDown.X);
                currentShape.Height = Math.Abs(mMove.Y - mDown.Y);


                if (addShape)
                {
                    canvasTool.DrawCapture(currentShape);
                }
            }
            else if (canvasTool.drawType == DrawType.line && capture)
            {
                if (currentLine == null)
                {
                    currentLine = new Line();
                    addShape = true;
                }
                currentLine.X1 = mDown.X;
                currentLine.Y1 = mDown.Y;
                currentLine.X2 = mMove.X;
                currentLine.Y2 = mMove.Y;
                currentLine.StrokeThickness = penThickness;
                currentLine.Stroke = new SolidColorBrush(penColor);

                if (addShape)
                {
                    canvasTool.DrawCapture(currentLine);
                }
            }
            #endregion
        }

        private void main_canvas_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (canvasTool.drawType == DrawType.nothing)
            {
                if (e.Source != main_canvas && e.Source.GetType() == typeof(Rectangle) || e.Source.GetType() == typeof(Ellipse) || e.Source.GetType() == typeof(Line))
                {
                    if (e.Source is Rectangle)
                        {
                        currentShapeLabel.Content = "Rectangle";
                        HeightTextBox.Text = ((Rectangle)e.Source).Height.ToString();
                        WidthTextBox.Text = ((Rectangle)e.Source).Width.ToString();
                        selectedShape = (Rectangle)e.Source;
                    }
                    else if(e.Source is Ellipse)
                    {
                        currentShapeLabel.Content = "Circle";
                        HeightTextBox.Text = ((Ellipse)e.Source).Height.ToString();
                        WidthTextBox.Text = ((Ellipse)e.Source).Width.ToString();
                        selectedShape = e.Source as Ellipse;
                    }
                    else if (e.Source is Line)
                    {
                        currentShapeLabel.Content = "Line";
                        HeightTextBox.Text = ((Line)e.Source).Height.ToString();
                        WidthTextBox.Text = ((Line)e.Source).Width.ToString();
                        selectedShape = e.Source as Line;
                    }

                }
            }
        }
        #endregion

        private void ZoomOutBtn_Click(object sender, RoutedEventArgs e)
        {
            var st = new ScaleTransform();
            st.ScaleX /= 1.1;
            st.ScaleY /= 1.1;

            main_canvas.RenderTransform = st;
        }

        private void ZoomInBtn_Click(object sender, RoutedEventArgs e)
        {
            var st = new ScaleTransform();
            st.ScaleX *= 1.1;
            st.ScaleY *= 1.1;

            main_canvas.RenderTransform = st;
        }

        private void SaveDimensBtn_Click(object sender, RoutedEventArgs e)
        {
            if((canvasTool.drawType==DrawType.rectangle || canvasTool.drawType == DrawType.ellipse) && (HeightTextBox.Text !="" && WidthTextBox.Text!=""))
            {
                if(canvasTool.drawType==DrawType.rectangle)
                {
                    currentShape = new Rectangle();
                }
                else if(canvasTool.drawType ==DrawType.ellipse)
                {
                    currentShape = new Ellipse();
                }

                    double width = 0;
                    double height = 0;
                    Double.TryParse(WidthTextBox.Text.ToString(), out width);
                    Double.TryParse(HeightTextBox.Text.ToString(), out height);
                    Canvas.SetTop(currentShape, 8);
                    Canvas.SetLeft(currentShape, 8);
                    currentShape.Width = width;
                    currentShape.Height = height;
                    currentShape.Stroke = new SolidColorBrush(penColor);
                    currentShape.StrokeThickness = penThickness;
                    canvasTool.DrawShape(currentShape, false);
                    currentShape = null;
                
            }
            else if(selectedShape!=null && canvasTool.drawType == DrawType.nothing)
            {
                double width = 0;
                double height = 0;
                Double.TryParse(WidthTextBox.Text.ToString(), out width);
                Double.TryParse(HeightTextBox.Text.ToString(), out height);


                selectedShape.Width = width;
                selectedShape.Height = height;
                selectedShape.Stroke = new SolidColorBrush(penColor);
                selectedShape.StrokeThickness = penThickness;

                canvasTool.RemoveShape(selectedShape);
                canvasTool.DrawShape(selectedShape, false);
                selectedShape = null;
            }
        }

        private void getShape(object sender, RoutedEventArgs e)
        {
            String buttonText = (sender as Button).Name;
            CircleBtn.Background = new SolidColorBrush(buttonBackground);
            LineBtn.Background = new SolidColorBrush(buttonBackground);
            RectangleBtn.Background = new SolidColorBrush(buttonBackground);
            ArrowBtn.Background = new SolidColorBrush(buttonBackground);
            currentToolLabel.Content = "null";

            if (buttonText.Equals("LineBtn"))
            {
                LineBtn.Background = new SolidColorBrush(Colors.DarkGray);
                canvasTool.drawType = DrawType.line;
                main_canvas.Cursor = Cursors.Cross;
                currentToolLabel.Content = "Line";
            }
            else if (buttonText.Equals("CircleBtn"))
            {
                CircleBtn.Background = new SolidColorBrush(Colors.DarkGray);
                canvasTool.drawType = DrawType.ellipse;
                main_canvas.Cursor = Cursors.Cross;
                currentToolLabel.Content = "Circle";
            }
            else if (buttonText.Equals("ArrowBtn"))
            {
                ArrowBtn.Background = new SolidColorBrush(Colors.DarkGray);
                canvasTool.drawType = DrawType.nothing;
                main_canvas.Cursor = Cursors.Arrow;
                currentToolLabel.Content = "Cursor";
            }
            else if (buttonText.Equals("RectangleBtn"))
            {
                RectangleBtn.Background = new SolidColorBrush(Colors.DarkGray);
                canvasTool.drawType = DrawType.rectangle;
                main_canvas.Cursor = Cursors.Cross;
                currentToolLabel.Content = "Rectangle";
            }
            else
                canvasTool.drawType = DrawType.nothing;
        }


    }
}
