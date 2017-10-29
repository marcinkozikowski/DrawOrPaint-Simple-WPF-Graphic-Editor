﻿/*
 * The Following Code was developed by Dewald Esterhuizen
 * View Documentation at: http://softwarebydefault.com
 * Licensed under Ms-PL 
*/
using DrawOrPaint.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ImageConvolutionFilters
{
    public class HighPass3x3Filter : FilterBase
    {
        public override string FilterName
        {
            get { return "HighPass3x3Filter"; }
        }

        private double factor = 1.0 / 16.0;
        public override double Factor
        {
            get { return factor; }
        }

        private double bias = 128.0;
        public override double Bias
        {
            get { return bias; }
        }

        private double[,] filterMatrix =
            new double[,] { { -1, -2, -1, }, 
                            { -2, 12, -2, }, 
                            { -1, -2, -1, }, };

        public override double[,] FilterMatrix
        {
            get { return filterMatrix; }
        }
    }
}
