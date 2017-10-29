
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


    public class HorizontalEdgeDetectionFilter : FilterBase
    {
        public override string FilterName
        {
            get { return "HorizontalEdgeDetectionFilter"; }
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
            new double[,] { {  0,  0,  0,  0,  0, }, 
                            {  0,  0,  0,  0,  0, }, 
                            { -1, -1,  2,  0,  0, },
                            {  0,  0,  0,  0,  0, },
                            {  0,  0,  0,  0,  0, }, };

        public override double[,] FilterMatrix
        {
            get { return filterMatrix; }
        }
    }

    public class VerticalEdgeDetectionFilter : FilterBase
    {
        public override string FilterName
        {
            get { return "VerticalEdgeDetectionFilter"; }
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
            new double[,] { {  0,  0, -1,  0,  0, }, 
                            {  0,  0, -1,  0,  0, }, 
                            {  0,  0,  4,  0,  0, },
                            {  0,  0, -1,  0,  0, },
                            {  0,  0, -1,  0,  0, }, };

        public override double[,] FilterMatrix
        {
            get { return filterMatrix; }
        }
    }

    public class EdgeDetectionTopLeftBottomRightFilter : FilterBase
    {
        public override string FilterName
        {
            get { return "EdgeDetectionTopLeftBottomRightFilter"; }
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
            new double[,] { { -5,  0,  0, }, 
                            {  0,  0,  0, }, 
                            {  0,  0,  5, }, };

        public override double[,] FilterMatrix
        {
            get { return filterMatrix; }
        }
    }
}
