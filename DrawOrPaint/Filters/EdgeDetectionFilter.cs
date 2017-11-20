
using DrawOrPaint.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ImageConvolutionFilters
{
    public class EdgeDetectionFilter : FilterBase
    {
        public override string FilterName
        {
            get { return "EdgeDetectionFilter"; }
        }

        private double factor = 1.0;
        public override double Factor
        {
            get { return factor; }
        }

        private double bias = 0.0;
        public override double Bias
        {
            get { return bias; }
        }

        private double[,] filterMatrix =
            new double[,] { { -1, -1, -1, }, 
                            { -1,  8, -1, }, 
                            { -1, -1, -1, }, };

        public override double[,] FilterMatrix
        {
            get { return filterMatrix; }
        }
    }
}
