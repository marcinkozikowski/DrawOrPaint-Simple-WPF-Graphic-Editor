
using DrawOrPaint.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ImageConvolutionFilters
{
    public class Blur3x3Filter : FilterBase
    {
        public override string FilterName
        {
            get { return "Blur3x3Filter"; }
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
            new double[,] { { 0.0, 0.2, 0.0, }, 
                            { 0.2, 0.2, 0.2, }, 
                            { 0.0, 0.2, 0.2, }, };

        public override double[,] FilterMatrix
        {
            get { return filterMatrix; }
        }
    }

    

    public class Gaussian3x3BlurFilter : FilterBase
    {
        public override string FilterName
        {
            get { return "Gaussian3x3BlurFilter"; }
        }

        private double factor = 1.0 / 16.0;
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
            new double[,] { { 1, 2, 1, }, 
                            { 2, 4, 2, }, 
                            { 1, 2, 1, }, };

        public override double[,] FilterMatrix
        {
            get { return filterMatrix; }
        }
    }
}
