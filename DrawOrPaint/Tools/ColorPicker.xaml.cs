﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;

namespace DrawOrPaint.Tools
{
    /// <summary>
    /// Interaction logic for ColorPicker.xaml
    /// </summary>
    public partial class ColorPicker : UserControl
    {
        int red, green, blue;
        double c, m, y, k;
        bool rgbEdit = false;
        bool cmykEdit = false;

        public delegate void ColorPickerChangeHandler(Color color);

        public event ColorPickerChangeHandler OnPickColor;

        private Color currentColor;

        public Color CurrentColor
        {
            get
            {
                return currentColor;
            }

            set
            {
                currentColor = value;
            }
        }

        public void updateSelectedColor()
        {
            Color color = Color.FromRgb(System.Convert.ToByte(red), System.Convert.ToByte(green), System.Convert.ToByte(blue));
            ColorRectangle.Fill = new SolidColorBrush(color);
            CurrentColor = color;
            OnPickColor?.Invoke(color);
        }

        public ColorPicker()
        {
            InitializeComponent();
        }

        private void RGB_Changed(object sender, TextChangedEventArgs e)
        {
            if (cmykEdit == false)
            {
                rgbEdit = true;
                try
                {
                    red = int.Parse(RedEditText.Text.ToString());
                    green = int.Parse(GreenEditText.Text.ToString());
                    blue = int.Parse(BlueEditText.Text.ToString());

                    updateSelectedColor();
                    RGBToCMYK();
                    updateCMYKTextBox();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message.ToString());
                }
                rgbEdit = false;
            }
        }

        private void CMYK_Changed(object sender, TextChangedEventArgs e)
        {
            if (rgbEdit == false)
            {
                cmykEdit = true;
                try
                {
                    c = Double.Parse(CyanEditText.Text.ToString());
                    m = Double.Parse(MagentaEditText.Text.ToString());
                    y = Double.Parse(YellowEditText.Text.ToString());
                    k = Double.Parse(BlackEditText.Text.ToString());
                    CMYKToRGB();
                    updateRGBTextBox();
                    updateSelectedColor();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message.ToString());
                }
                cmykEdit = false;
            }
        }

        private void RGB_Slider_Changed(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e)
        {
            Slider rgb = sender as Slider;
            string _name = rgb.Name;
            if(_name.Equals("RedSlider"))
            {
                RedEditText.Text = Math.Round(RedSlider.Value,0).ToString();
            }
            else if (_name.Equals("GreenSlider"))
            {
                GreenEditText.Text = Math.Round(GreenSlider.Value, 0).ToString();
            }
            else if (_name.Equals("BlueSlider"))
            {
                BlueEditText.Text = Math.Round(BlueSlider.Value, 0).ToString();
            }
        }

        private void CMYK_Slider_Changed(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e)
        {
            Slider rgb = sender as Slider;
            string _name = rgb.Name;
            if (_name.Equals("CyanSlider"))
            {
                CyanEditText.Text = Math.Round(CyanSlider.Value, 2).ToString();
            }
            else if (_name.Equals("MagentaSlider"))
            {
                MagentaEditText.Text = Math.Round(MagentaSlider.Value, 2).ToString();
            }
            else if (_name.Equals("YellowSlider"))
            {
                YellowEditText.Text = Math.Round(YellowSlider.Value, 2).ToString();
            }
            else if (_name.Equals("BlackSlider"))
            {
                BlackEditText.Text = Math.Round(BlackSlider.Value, 2).ToString();
            }
        }

        private void updateCMYKTextBox()
        {
            CyanEditText.Text = (Math.Round(c,2)).ToString();
            MagentaEditText.Text = (Math.Round(m, 2)).ToString();
            YellowEditText.Text = (Math.Round(y, 2)).ToString();
            BlackEditText.Text = (Math.Round(k, 2)).ToString();

            CyanSlider.Value = (Math.Round(c, 2));
            MagentaSlider.Value = (Math.Round(m, 2));
            YellowSlider.Value = (Math.Round(y, 2));
            BlackSlider.Value = (Math.Round(k, 2));
        }

        private void updateRGBTextBox()
        {
            RedEditText.Text = red.ToString();
            GreenEditText.Text = green.ToString();
            BlueEditText.Text = blue.ToString();
            RedSlider.Value = red;
            BlueSlider.Value = blue;
            GreenSlider.Value = green;

        }

        private double Max(double rPi, double gPi, double bPi)
        {
            if(rPi==gPi || rPi==bPi)
            {
                return rPi;
            }
            List<double> genList = new List<double>() { rPi, gPi, bPi };
            return genList.Max();
        }

        private void RGBToCMYK()
        {
            double rPi = red / 255.0;
            double gPi = green / 255.0;
            double bPi = blue / 255.0;

            k = 1.0 - Max(rPi, gPi,bPi);
            c = (1.0 - rPi - k) / (1.0 - k);
            m = (1.0 - gPi - k) / (1.0 - k);
            y = (1.0 - bPi - k) / (1.0 - k);
        }


        private void CMYKToRGB()
        {
            red = (int)(255 * (1.0 - c) * (1.0 - k));
            green = (int)(255 * (1.0 - m) * (1.0 - k));
            blue = (int)(255 * (1.0 - y) * (1.0 - k));
        }
    }
}
