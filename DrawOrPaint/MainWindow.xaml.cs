using AForge.Imaging;
using DrawOrPaint.Tools;
using ImageConvolutionFilters;
using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.ComponentModel;
using DrawOrPaint.Filters;

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
        const double ScaleRate = 1.2;
        ScaleTransform currentZoom;
        private int brightnessValue;

        public MainWindow()
        {
            InitializeComponent();
            CanvasTool = new CanvasTools(main_canvas);
            penColor = Colors.Black;
            fillColor = Colors.Black;
            penThickness = 5;
            currentZoom = new ScaleTransform();
            //Point1 = "70,130" Point2 = "220,20" Point3 = "180,160"
            //BezierSegment curve = new BezierSegment(new Point(70, 130), new Point(320, 200), new Point(180, 416), true);

            //// Set up the Path to insert the segments
            //PathGeometry path = new PathGeometry();

            //PathFigure pathFigure = new PathFigure();
            //pathFigure.StartPoint = new Point(20, 20);
            //pathFigure.IsClosed = false;
            //path.Figures.Add(pathFigure);


            //pathFigure.Segments.Add(curve);
            
            //System.Windows.Shapes.Path p = new System.Windows.Shapes.Path();
            //p.Stroke = Brushes.Red;
            //p.Data = path;

            //main_canvas.Children.Add(p); // H
        }

        Color buttonBackground = Color.FromRgb(System.Convert.ToByte(221), System.Convert.ToByte(221), System.Convert.ToByte(221));

        public int BrightnessValue
        {
            get
            {
                return brightnessValue;
            }

            set
            {
                brightnessValue = value;
            }
        }

        internal CanvasTools CanvasTool
        {
            get
            {
                return canvasTool;
            }

            set
            {
                canvasTool = value;
            }
        }

        #region MouseHandlers

        private void SetMouseCursor()
        {
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
            if (Cursor != desired_cursor) Cursor = desired_cursor;
            main_canvas.Cursor = desired_cursor;
        }
        private HitType SetHitType(Shape rect, Point point)
        {
            if (selectedShape is Rectangle || selectedShape is Ellipse)
            {
                double left = Canvas.GetLeft((selectedShape));
                double top = Canvas.GetTop((selectedShape));
                double right = left + (selectedShape).Width;
                double bottom = top + (selectedShape).Height;
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

                if (point.X < Math.Min(left, right)) return HitType.None;
                if (point.X > Math.Max(left, right)) return HitType.None;
                if (point.Y < Math.Min(top,bottom)) return HitType.None;
                if (point.Y > Math.Max(top, bottom)) return HitType.None;

                double cornerGap = 10;
                if ((Math.Abs(point.X - left) < cornerGap) && (Math.Abs(point.Y - top) < cornerGap))
                {
                    return HitType.L;
                }
                if ((Math.Abs(point.X - right) < cornerGap) && (Math.Abs(point.Y - bottom) < cornerGap))
                {
                    return HitType.R;
                }
                return HitType.Body;
            }
            else return HitType.None;
        }

        private void main_canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {   
            mDown = e.GetPosition(this.main_canvas);
            capture = true;

            if (selectedShape != null)
            {
                if (selectedShape is Rectangle)
                {
                    currentHitType = SetHitType((Rectangle)selectedShape, Mouse.GetPosition(main_canvas));
                    SetMouseCursor();
                    if (currentHitType == HitType.None) return;

                    LastPoint = Mouse.GetPosition(main_canvas);
                    dragInProgress = true;
                }
                else if (selectedShape is Ellipse)
                {
                    currentHitType = SetHitType((Ellipse)selectedShape, Mouse.GetPosition(main_canvas));
                    SetMouseCursor();
                    if (currentHitType == HitType.None) return;

                    LastPoint = Mouse.GetPosition(main_canvas);
                    dragInProgress = true;
                }
                else if (selectedShape is Line)
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
                if (CanvasTool.drawType == DrawType.ellipse || CanvasTool.drawType == DrawType.rectangle)
                {
                    Shape temp = null;
                    if (CanvasTool.drawType == DrawType.ellipse)
                    {
                        temp = new Ellipse();
                    }
                    else if (CanvasTool.drawType == DrawType.rectangle)
                    {
                        temp = new Rectangle();
                    }
                    temp.Stroke = new SolidColorBrush(penColor);
                    temp.Fill = Brushes.Transparent;
                    temp.StrokeThickness = penThickness;
                    temp.IsHitTestVisible = true;
                    temp.Width = currentShape.Width;
                    temp.Height = currentShape.Height;
                    Canvas.SetLeft(temp, currentShape.Margin.Left);
                    Canvas.SetTop(temp, currentShape.Margin.Top);
                    CanvasTool.DrawShape(temp, true);
                }

                currentShape = null;
            }
            else if (CanvasTool.drawType == DrawType.line && currentLine != null)
            {

                Line line = new Line();
                line.Stroke = new SolidColorBrush(penColor);
                line.StrokeThickness = penThickness;

                line.X1 = currentLine.X1;
                line.X2 = currentLine.X2;
                line.Y1 = currentLine.Y1;
                line.Y2 = currentLine.Y2;

                //double width = Math.Abs(line.X2 - line.X1);
                //double height = Math.Abs(line.Y2 - line.Y1);

                //line.Width = width;
                //line.Height = height;

                line.IsHitTestVisible = true;

                Canvas.SetLeft(line, currentLine.Margin.Left);
                Canvas.SetTop(line, currentLine.Margin.Right);

                CanvasTool.DrawShape(line, true);
            }
            currentLine = null;
        }

        private void main_canvas_MouseMove(object sender, MouseEventArgs e)
        {
            #region ChangePlace

            if (dragInProgress == false && selectedShape != null)
            {
                currentHitType = SetHitType(selectedShape, Mouse.GetPosition(main_canvas));
                SetMouseCursor();
            }
            else if (selectedShape != null && dragInProgress == true)
            {
                Point point = Mouse.GetPosition(main_canvas);

                double offset_x = point.X - LastPoint.X;
                double offset_y = point.Y - LastPoint.Y;

                double new_x = 0;
                double new_y = 0;
                double new_width = 0;
                double new_height = 0;
                double new_x2 = 0;
                double new_y2 = 0;

                // Get the current position.
                if (selectedShape is Rectangle || selectedShape is Ellipse)
                {
                    new_x = Canvas.GetLeft(selectedShape);
                    new_y = Canvas.GetTop(selectedShape);
                    new_width =selectedShape.Width;
                    new_height = selectedShape.Height;
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
                else if (selectedShape is Line)
                {
                    switch (currentHitType)
                    {
                        case HitType.Body:
                            new_x += offset_x;
                            new_y += offset_y;
                            new_x2 += offset_x;
                            new_y2 += offset_y;
                            break;
                        case HitType.L:
                            new_x += offset_x;
                            new_y += offset_y;
                            break;
                        case HitType.R:
                            new_x2 += offset_x;
                            new_y2 += offset_y;
                            break;
                    }
                }
                #endregion

                if ((new_width > 0) && (new_height > 0))
                {
                    if (selectedShape is Rectangle || selectedShape is Ellipse)
                    {
                        Canvas.SetLeft(selectedShape, new_x);
                        Canvas.SetTop(selectedShape, new_y);
                        selectedShape.Width = new_width;
                        selectedShape.Height = new_height;
                    }
                    LastPoint = point;
                }
                if (selectedShape is Line)
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
            if ((CanvasTool.drawType == DrawType.ellipse || CanvasTool.drawType == DrawType.rectangle) && capture)
            {

                if (currentShape == null)
                {
                    if (CanvasTool.drawType == DrawType.ellipse)
                    {
                        currentShape = new Ellipse();
                    }
                    else if (CanvasTool.drawType == DrawType.rectangle)
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
                    CanvasTool.DrawCapture(currentShape);
                }
            }
            else if (CanvasTool.drawType == DrawType.line && capture)
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
                    CanvasTool.DrawCapture(currentLine);
                }
            }
            #endregion
        }

        private void main_canvas_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (CanvasTool.drawType == DrawType.nothing)
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
                    else if (e.Source is Ellipse)
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

        #region MenuItems Handlers

        private void OpenMenuItem_Click(object sender, RoutedEventArgs e)
        {
                string filename = "";
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "Image files (*.ppm;*.jpeg;*.jpg)|*.ppm;*.jpeg;*.jpg";
                openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                if (openFileDialog.ShowDialog() == true)
                {
                    filename = openFileDialog.FileName;
                    CanvasTool.OpenNewImageFile(filename);

                }
                currentFileLabel.Content = "Res: "+PixelMap2.BmpWidth + "x" + PixelMap2.BmpHeight+"  Max Color Value: "+PixelMap2.BmpMaxVal;
                currentFileNameLabel.Content = System.IO.Path.GetFileName(filename);
        }

        private void SaveMenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                JpegQuality_Window w = new JpegQuality_Window();
                w.Show();
            }
            catch (Exception ex)
            {
                ShowException(ex.Message);
            }
        }
        #endregion

        #region Buttons Handlers
        private void ZoomOutBtn_Click(object sender, RoutedEventArgs e)
        {
            currentZoom.ScaleX /= ScaleRate;
            currentZoom.ScaleY /= ScaleRate;

            main_canvas.RenderTransform = currentZoom;
            double scale = Math.Round((currentZoom.ScaleX * 100), 0);
            CurrentZoom.Content = scale.ToString() + "%";
        }

        private void ZoomInBtn_Click(object sender, RoutedEventArgs e)
        {

            currentZoom.ScaleX *= ScaleRate;
            currentZoom.ScaleY *= ScaleRate;

            main_canvas.RenderTransform = currentZoom;

            double scale = Math.Round((currentZoom.ScaleX * 100), 0);
            CurrentZoom.Content = scale.ToString() + "%";
        }

        private void SaveDimensBtn_Click(object sender, RoutedEventArgs e)
        {
            if ((CanvasTool.drawType == DrawType.rectangle || CanvasTool.drawType == DrawType.ellipse) && (HeightTextBox.Text != "" && WidthTextBox.Text != ""))
            {
                if (CanvasTool.drawType == DrawType.rectangle)
                {
                    currentShape = new Rectangle();
                }
                else if (CanvasTool.drawType == DrawType.ellipse)
                {
                    currentShape = new Ellipse();
                }

                double width = 0;
                double height = 0;
                Double.TryParse(WidthTextBox.Text.ToString(), out width);
                Double.TryParse(HeightTextBox.Text.ToString(), out height);
                Canvas.SetTop(currentShape, 8);
                Canvas.SetLeft(currentShape, 8);
                currentShape.Fill = Brushes.Transparent;
                currentShape.Width = width;
                currentShape.Height = height;
                currentShape.Stroke = new SolidColorBrush(penColor);
                currentShape.StrokeThickness = penThickness;
                CanvasTool.DrawShape(currentShape, false);
                currentShape = null;

            }
            else if (selectedShape != null && CanvasTool.drawType == DrawType.nothing)
            {
                double width = 0;
                double height = 0;
                Double.TryParse(WidthTextBox.Text.ToString(), out width);
                Double.TryParse(HeightTextBox.Text.ToString(), out height);


                selectedShape.Width = width;
                selectedShape.Height = height;
                //selectedShape.Fill = Brushes.Transparent;
                selectedShape.Stroke = new SolidColorBrush(penColor);
                selectedShape.StrokeThickness = penThickness;

                CanvasTool.RemoveShape(selectedShape);
                CanvasTool.DrawShape(selectedShape, false);
                selectedShape = null;
            }
        }

        private void btnRightMenuHide_Click(object sender, RoutedEventArgs e)
        {
            btnRightMenuHide.Visibility = System.Windows.Visibility.Hidden;
            btnRightMenuShow.Visibility = System.Windows.Visibility.Visible;
            Storyboard sb = Resources["sbHideRightMenu"] as Storyboard;
            sb.Begin(pnlRightMenu);
        }

        private void btnRightMenuShow_Click(object sender, RoutedEventArgs e)
        {
            btnRightMenuHide.Visibility = System.Windows.Visibility.Visible;
            btnRightMenuShow.Visibility = System.Windows.Visibility.Hidden;
            Storyboard sb = Resources["sbShowRightMenu"] as Storyboard;
            sb.Begin(pnlRightMenu);
        }

        private void pickedColorChanged_Click(Color color)
        {
            penColor = color;
            currentColorLabel.Fill = new SolidColorBrush(penColor);
        }

        private void ApplayFilter(object sender, RoutedEventArgs e)
        {
            Button pickedFilter = sender as Button;


            System.Drawing.Bitmap bitmapCanvas = CanvasTool.getBitmapFromCanvas();
            System.Drawing.Bitmap afterFilter;

            System.Windows.Controls.Image MyImg = new System.Windows.Controls.Image();
            IntPtr hBitmap;
            FilterBase filter;

            switch (pickedFilter.Name)
            {
                case "EdgeDetection_Btn":
                    filter = new EdgeDetectionFilter();
                    afterFilter = bitmapCanvas.ConvolutionFilter(filter);
                    hBitmap = afterFilter.GetHbitmap();
                    MyImg.Source = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(hBitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                    main_canvas.Children.Clear();
                    main_canvas.Children.Add(MyImg);
                    break;
                case "GaussianBlur_Btn":
                    filter = new Gaussian3x3BlurFilter();
                    afterFilter = bitmapCanvas.ConvolutionFilter(filter);
                    hBitmap = afterFilter.GetHbitmap();
                    MyImg.Source = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(hBitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                    main_canvas.Children.Clear();
                    main_canvas.Children.Add(MyImg);
                    break;
                case "Soften_Btn":
                    filter = new SoftenFilter();
                    afterFilter = bitmapCanvas.ConvolutionFilter(filter);
                    hBitmap = afterFilter.GetHbitmap();
                    MyImg.Source = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(hBitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                    main_canvas.Children.Clear();
                    main_canvas.Children.Add(MyImg);
                    break;
                case "HighPass_Btn":
                    filter = new HighPass3x3Filter();
                    afterFilter = bitmapCanvas.ConvolutionFilter(filter);
                    hBitmap = afterFilter.GetHbitmap();
                    MyImg.Source = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(hBitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                    main_canvas.Children.Clear();
                    main_canvas.Children.Add(MyImg);
                    break;
                case "Median_Btn":
                    afterFilter = MedianFilter.CalculateMedianFilter(bitmapCanvas, 3, 0, false);
                    hBitmap = afterFilter.GetHbitmap();
                    MyImg.Source = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(hBitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                    main_canvas.Children.Clear();
                    main_canvas.Children.Add(MyImg);
                    break;
                default:
                    break;
            }
        }

        private void BrightnessValue_Changed(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int value = (int)Math.Round(BrightnessSlider.Value, 0);
            BrightnesValueTextBox.Content = value.ToString();
            BrightnessValue = value;
        }

        private void SetPointTransformation(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.Image MyImg = new System.Windows.Controls.Image();
            IntPtr hBitmap;

            System.Drawing.Bitmap bitmap = CanvasTool.getBitmapFromCanvas();
            int value = int.Parse(PointTransformationValue.Text.ToString());

            if (Addition.IsChecked==true)
            {
                bitmap = CanvasTool.SetAddition(value, bitmap);
            }
            else if(Subtraction.IsChecked==true)
            {
                bitmap = CanvasTool.SetSubtraction(value, bitmap);
            }
            else if(Division.IsChecked==true)
            {
                bitmap = CanvasTool.SetDivision(value, bitmap);
            }
            else if(Multiplication.IsChecked==true)
            {
                bitmap = CanvasTool.SetMultiplication(value, bitmap);
            }

            hBitmap = bitmap.GetHbitmap();
            MyImg.Source = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(hBitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());

            main_canvas.Children.Clear();
            main_canvas.Children.Add(MyImg);
        }

        private void ChangeImageBrightness(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.Image MyImg = new System.Windows.Controls.Image();
            IntPtr hBitmap;

            System.Drawing.Bitmap bitmap = CanvasTool.getBitmapFromCanvas();
            bitmap = CanvasTool.SetBrightness(BrightnessValue, bitmap);

            hBitmap = bitmap.GetHbitmap();
            MyImg.Source = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(hBitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());

            main_canvas.Children.Clear();
            main_canvas.Children.Add(MyImg);
        }

        private void SetGrayscale_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.Image MyImg = new System.Windows.Controls.Image();
            IntPtr hBitmap;

            System.Drawing.Bitmap bitmap = CanvasTool.getBitmapFromCanvas();
            bitmap = CanvasTool.SetGrayscale(bitmap);

            hBitmap = bitmap.GetHbitmap();
            MyImg.Source = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(hBitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());

            main_canvas.Children.Clear();
            main_canvas.Children.Add(MyImg);
        }

        #endregion


        private void ShowHistogram_Click(object sender, RoutedEventArgs e)
        {
            Histogram_Window h = new Histogram_Window();
            h.Show();
        }
        public void ShowException(string ex)
        {
            string message = ex;
            string caption = "PixelMap Error! " + ex;
            MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Error);
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
                CanvasTool.drawType = DrawType.line;
                main_canvas.Cursor = Cursors.Cross;
                currentToolLabel.Content = "Line";
            }
            else if (buttonText.Equals("CircleBtn"))
            {
                CircleBtn.Background = new SolidColorBrush(Colors.DarkGray);
                CanvasTool.drawType = DrawType.ellipse;
                main_canvas.Cursor = Cursors.Cross;
                currentToolLabel.Content = "Circle";
            }
            else if (buttonText.Equals("ArrowBtn"))
            {
                ArrowBtn.Background = new SolidColorBrush(Colors.DarkGray);
                CanvasTool.drawType = DrawType.nothing;
                main_canvas.Cursor = Cursors.Arrow;
                currentToolLabel.Content = "Cursor";
            }
            else if (buttonText.Equals("RectangleBtn"))
            {
                RectangleBtn.Background = new SolidColorBrush(Colors.DarkGray);
                CanvasTool.drawType = DrawType.rectangle;
                main_canvas.Cursor = Cursors.Cross;
                currentToolLabel.Content = "Rectangle";
            }
            else
                CanvasTool.drawType = DrawType.nothing;
        }
    }
}
