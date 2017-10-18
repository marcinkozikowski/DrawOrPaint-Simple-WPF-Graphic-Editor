using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrawOrPaint.Tools
{
    class PixelMap2
    {
        private static int bmpWidth = -1;
        private static int bmpHeight = -1;
        private static int bmpMaxVal = -1;

        public static Bitmap Load(string fileName)
        {
            Bitmap bmp = null;
            using (var f = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                bmp = Load(f);
            }
            return bmp;
        }

        public static Bitmap Load(Stream stream)
        {
            Bitmap bmp = null;
            string line;
            string[] lineArray;
            string[] lineWithOutComments;
            char pnmType;

            bmpWidth = -1;
            bmpHeight = -1;
            bmpMaxVal = -1;

            if ((char)stream.ReadByte() != 'P') throw new ApplicationException("Incorrect file format.");
            pnmType = (char)stream.ReadByte();
            if ((pnmType < '1') || (pnmType > '6')) throw new ApplicationException("Unrecognized bitmap type.");

            while (stream.Position < stream.Length)
            {
                line = ReadLine(stream);
                if (line.Length == 0) continue;
                if(line.Contains('#'))
                {
                    lineWithOutComments = line.Split('#');
                    line = lineWithOutComments[0];
                }
                lineWithOutComments = line.Split(whitespace, StringSplitOptions.RemoveEmptyEntries);
                lineArray = lineWithOutComments;
                if (lineArray.Length == 0) continue;

                for (int i = 0; i < lineArray.Length; i++)
                {
                    if (BmpWidth == -1) { BmpWidth = Convert.ToInt32(lineArray[i]); }
                    else if (BmpHeight == -1) { BmpHeight = Convert.ToInt32(lineArray[i]); }
                    else if (BmpMaxVal == -1) { BmpMaxVal = Convert.ToInt32(lineArray[i]); }
                }

                if ((BmpWidth != -1) && (BmpHeight != -1) && (BmpMaxVal != -1))
                    break;
            }

            if ((BmpWidth <= 0) || (BmpHeight <= 0) || (BmpMaxVal <= 0))
                throw new ApplicationException("Invalid image dimensions.");

            int numPixels = BmpWidth * BmpHeight;
            int maxElementCount = numPixels * 4;

            var bmpData = new byte[maxElementCount];    //byte array to prepare bmp image
            byte[] buffor = ReadAllBytes(stream);   //Read whole image file to buffor
            Stream stream2 = new MemoryStream(buffor);  //Make stream from buffor

            try
            {
                if (pnmType == '3') //color bitmap (ascii)
                {
                    int elementCount = 0, elementMod = 2;
                    int elementVal;
                    while (stream2.Position < stream2.Length)
                    {
                        line = ReadLine(stream2);
                        if (line.Length == 0) continue;
                        if (line.Contains('#'))
                        {
                            lineWithOutComments = line.Split('#');
                            line = lineWithOutComments[0];
                        }
                        lineWithOutComments = line.Split(whitespace, StringSplitOptions.RemoveEmptyEntries);
                        lineArray = lineWithOutComments;

                        for (int i = 0; i < lineArray.Length; i++)
                        {
                            if (elementCount >= maxElementCount) break;
                            elementVal = Convert.ToInt32(lineArray[i]);
                            bmpData[elementCount + elementMod] = (byte)((elementVal * 255) / bmpMaxVal);
                            elementMod--;
                            if (elementMod < 0) { elementCount += 4; elementMod = 2; }
                        }
                        if (elementCount >= maxElementCount) break;
                    }
                }
                else if (pnmType == '6') //color bitmap (binary)
                {
                    byte[] pixel = new byte[16];
                    int elementCount = 0;
                    if (bmpMaxVal < 256)
                    {
                        for (int i = 0; i < numPixels; i++)
                        {
                            stream2.Read(pixel, 0, 3);
                            bmpData[elementCount++] = pixel[2];
                            bmpData[elementCount++] = pixel[1];
                            bmpData[elementCount++] = pixel[0];
                            elementCount++;
                        }
                    }
                    else if (bmpMaxVal < 65536)
                    {
                        for (int i = 0; i < numPixels; i++)
                        {
                            stream2.Read(pixel, 0, 6);
                            bmpData[elementCount++] = pixel[4];
                            bmpData[elementCount++] = pixel[2];
                            bmpData[elementCount++] = pixel[0];
                            elementCount++;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Error while processing PNM file: " + e.Message);
            }

            bmp = new Bitmap(BmpWidth, BmpHeight, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
            System.Drawing.Imaging.BitmapData bmpBits = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), System.Drawing.Imaging.ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
            System.Runtime.InteropServices.Marshal.Copy(bmpData, 0, bmpBits.Scan0, bmpData.Length);
            bmp.UnlockBits(bmpBits);
            return bmp;
        }


        private static char[] whitespace = { ' ', '\t', '\r', '\n' };

        public static int BmpWidth
        {
            get
            {
                return bmpWidth;
            }

            set
            {
                bmpWidth = value;
            }
        }

        public static int BmpHeight
        {
            get
            {
                return bmpHeight;
            }

            set
            {
                bmpHeight = value;
            }
        }

        public static int BmpMaxVal
        {
            get
            {
                return bmpMaxVal;
            }

            set
            {
                bmpMaxVal = value;
            }
        }

        private static string ReadLine(Stream stream)
        {
            string str = "";
            byte[] lineBytes = new byte[1024];
            int startPos = (int)stream.Position;
            stream.Read(lineBytes, 0, 1024);
            int strLen = 0;
            while (strLen < 1024)
            {
                if ((lineBytes[strLen] == '\r') || (lineBytes[strLen] == '\n')) { strLen++; break; }
                strLen++;
            }
            if (strLen > 1)
                str = Encoding.ASCII.GetString(lineBytes, 0, strLen - 1);

            stream.Position = startPos + strLen;
            return str;
        }

        private static byte[] ReadAllBytes(Stream reader)
        {
            const int bufferSize = 2048;
            using (var ms = new MemoryStream())
            {
                byte[] buffer = new byte[bufferSize];
                int count;
                while ((count = reader.Read(buffer, 0, buffer.Length)) != 0)
                    ms.Write(buffer, 0, count);
                return ms.ToArray();
            }

        }
    }
}
