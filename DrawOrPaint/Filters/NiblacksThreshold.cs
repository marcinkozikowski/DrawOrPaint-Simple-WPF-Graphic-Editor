using Accord.Imaging;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrawOrPaint.Filters
{


    class NiblacksThreshold
    {
        private int radius = 15;
        private double k = 0.2D;
        private double c = 0;

        protected unsafe void Apply(UnmanagedImage sourceData, UnmanagedImage destinationData)
        {
            int width = sourceData.Width;
            int height = sourceData.Height;
            int size = radius * 2;

            int pixelSize = System.Drawing.Image.GetPixelFormatSize(sourceData.PixelFormat) / 8;

            int srcStride = sourceData.Stride;
            int dstStride = destinationData.Stride;

            int srcOffset = srcStride - width * pixelSize;
            int dstOffset = dstStride - width * pixelSize;

            byte* src = (byte*)sourceData.ImageData.ToPointer();
            byte* dst = (byte*)destinationData.ImageData.ToPointer();


            // do the processing job
            if (sourceData.PixelFormat == PixelFormat.Format8bppIndexed)
            {
                // for each line
                for (int y = 0; y < height; y++)
                {
                    // for each pixel
                    for (int x = 0; x < width; x++, src++, dst++)
                    {
                        long sum = 0;
                        int count = 0;

                        for (int i = 0; i < size; i++)
                        {
                            int ir = i - radius;
                            int t = y + ir;

                            if (t < 0)
                                continue;
                            if (t >= height)
                                break;

                            for (int j = 0; j < size; j++)
                            {
                                int jr = j - radius;
                                t = x + jr;

                                if (t < 0)
                                    continue;
                                if (t >= width)
                                    continue;

                                sum += src[ir * srcStride + jr];
                                count++;
                            }
                        }

                        double mean = sum / (double)count;
                        double variance = 0;

                        for (int i = 0; i < size; i++)
                        {
                            int ir = i - radius;
                            int t = y + ir;

                            if (t < 0)
                                continue;
                            if (t >= height)
                                break;

                            for (int j = 0; j < size; j++)
                            {
                                int jr = j - radius;
                                t = x + jr;

                                if (t < 0)
                                    continue;
                                if (t >= width)
                                    continue;

                                byte val = src[ir * srcStride + jr];
                                variance += (val - mean) * (val - mean);
                            }
                        }

                        variance /= count - 1;

                        double cut = mean + k * Math.Sqrt(variance) - c;

                        *dst = (*src > cut) ? (byte)255 : (byte)0;
                    }
                    src += srcOffset;
                    dst += dstOffset;
                }
            }
            else
            {
                // for each line
                for (int y = 0; y < height; y++)
                {
                    // for each pixel
                    for (int x = 0; x < width; x++, src += pixelSize, dst += pixelSize)
                    {
                        long sumR = 0;
                        long sumG = 0;
                        long sumB = 0;
                        int count = 0;

                        for (int i = 0; i < size; i++)
                        {
                            int ir = i - radius;
                            int t = y + ir;

                            if (t < 0)
                                continue;
                            if (t >= height)
                                break;

                            for (int j = 0; j < size; j++)
                            {
                                int jr = j - radius;
                                t = x + jr;

                                if (t < 0)
                                    continue;
                                if (t >= width)
                                    continue;

                                byte* p = &src[ir * srcStride + jr * pixelSize];

                                count++;

                                sumR += p[RGB.R];
                                sumG += p[RGB.G];
                                sumB += p[RGB.B];
                            }
                        }

                        double meanR = sumR / (double)count;
                        double meanG = sumG / (double)count;
                        double meanB = sumB / (double)count;

                        double varR = 0;
                        double varG = 0;
                        double varB = 0;

                        for (int i = 0; i < size; i++)
                        {
                            int ir = i - radius;
                            int t = y + ir;

                            if (t < 0)
                                continue;
                            if (t >= height)
                                break;

                            // for each kernel column
                            for (int j = 0; j < size; j++)
                            {
                                int jr = j - radius;
                                t = x + jr;

                                if (t < 0)
                                    continue;
                                if (t >= width)
                                    continue;

                                byte* p = &src[ir * srcStride + jr * pixelSize];

                                varR += (p[RGB.R] - meanR) * (p[RGB.R] - meanR);
                                varG += (p[RGB.G] - meanG) * (p[RGB.G] - meanG);
                                varB += (p[RGB.B] - meanB) * (p[RGB.B] - meanB);
                            }
                        }

                        varR /= count - 1;
                        varG /= count - 1;
                        varB /= count - 1;

                        double cutR = (meanR + k * Math.Sqrt(varR) - c);
                        double cutG = (meanG + k * Math.Sqrt(varG) - c);
                        double cutB = (meanB + k * Math.Sqrt(varB) - c);

                        dst[RGB.R] = (src[RGB.R] > cutR) ? (byte)255 : (byte)0;
                        dst[RGB.G] = (src[RGB.G] > cutG) ? (byte)255 : (byte)0;
                        dst[RGB.B] = (src[RGB.B] > cutB) ? (byte)255 : (byte)0;

                        // take care of alpha channel
                        if (pixelSize == 4)
                            dst[RGB.A] = src[RGB.A];
                    }
                    src += srcOffset;
                    dst += dstOffset;
                }
            }
        }
    }
}
