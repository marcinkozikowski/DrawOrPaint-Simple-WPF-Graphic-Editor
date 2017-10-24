using System;
using System.Drawing;
using System.IO;
using System.Text;


namespace DrawOrPaint.Tools
{
    public static class PixelMap
    {
        public static long position = 0;
        private static int height;
        private static int width;
        private static string fileName;

        public static Bitmap ReadPPM(string file)
        {
            position = 0;
            Bitmap bitmap;

            bool ppmP3 = true, ppmP6 = false, Is255 = true;

            string sWidth = "", sHeight = "";

            FileStream stream = new FileStream(file, FileMode.Open);

            FileName = Path.GetFileName(file);

            var reader = new BinaryReader(stream);

            int MagicNumber = FindNextValue(reader, true);

            if(MagicNumber==3)
            {
                ppmP3 = true;
            }
            else ppmP3 = false;

            if(ppmP3==false && ppmP6==false)
            {
                throw new Exception("File has invalid Header or there is no header. Check your file: "+file);
            }

            sWidth = FindNextValue(reader, true).ToString();
            sHeight = FindNextValue(reader, true).ToString();

            int maxColorInt = FindNextValue(reader, true);

            if (maxColorInt > 255)
            {
                Is255 = false;
            }
            else Is255 = true;
            

            Width = int.Parse(sWidth);
            Height = int.Parse(sHeight);

            byte[] buffor = ReadAllBytes(reader);

            bitmap = new Bitmap(Width, Height);
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    if (Is255)
                    {
                        if (ppmP3)
                        {
                            int red = FindNextValue(buffor);
                            int green = FindNextValue(buffor);
                            int blue = FindNextValue(buffor);

                            bitmap.SetPixel(x, y, Color.FromArgb(red,
                                green, blue));
                        }
                        else if (ppmP6) 
                        {
                            bitmap.SetPixel(x, y, Color.FromArgb(reader.ReadByte(), reader.ReadByte(),
                                reader.ReadByte()));
                        }
                    }
                    else if(!Is255)
                    {
                        if (ppmP3) 
                        {
                            int red = (int)Math.Sqrt(FindNextValue(buffor));
                            int green = (int)Math.Sqrt(FindNextValue(buffor));
                            int blue = (int)Math.Sqrt(FindNextValue(buffor));
                            bitmap.SetPixel(x, y, Color.FromArgb(red,
                                green, blue));
                        }
                        else if (ppmP6) 
                        {
                            int r = (int)Math.Sqrt(reader.ReadByte() * reader.ReadByte());
                            int g = (int)Math.Sqrt(reader.ReadByte() * reader.ReadByte());
                            int b = (int)Math.Sqrt(reader.ReadByte() * reader.ReadByte());

                            bitmap.SetPixel(x, y, Color.FromArgb(r, g, b));
                        }
                    }

                }
            }
            stream.Close();
            return bitmap;
        }

        #region BinaryReaderFunctions

        private static char FindNextSingleValue(BinaryReader reader)
        {

            char temp = ' ';
            if (reader.BaseStream.Position != reader.BaseStream.Length)
            {
                temp = reader.ReadChar();
            }
            if(temp.Equals('#'))
            {
                OmitCommentLine(reader);
            }
            while((Char.IsDigit(temp)==false))
            {
                try
                {
                    temp = reader.ReadChar();
                    if (temp.Equals('#'))
                    {
                        OmitCommentLine(reader);
                    }
                }
                catch
                {
                    temp = (Char)reader.ReadByte();
                }
            }
            return temp;
                
        }

        private static void OmitCommentLine(BinaryReader reader)
        {
            char temp = ' ';
            while (!temp.Equals('\n'))
            {
                try
                {
                    temp = reader.ReadChar();
                }
                catch
                {
                    temp = (Char)reader.ReadByte();
                }
            }
        }

        private static int FindNextValue(BinaryReader reader, bool max)
        {

            char tempChar = ' ';
            char nextChar = ' ';
            long position = reader.BaseStream.Position;
            if (reader.BaseStream.Position != reader.BaseStream.Length)
            {
                tempChar = FindNextSingleValue(reader);
                position = reader.BaseStream.Position;
                if (position+1 != reader.BaseStream.Length)
                {
                        nextChar = (char)reader.PeekChar();
                }
                else
                    nextChar = ' ';
            }
            int value;

            StringBuilder valueString = new StringBuilder();

            if (Char.IsDigit(nextChar) == false)
            {
                int.TryParse(tempChar.ToString(), out value);
                return value;
            }
            else
            {

                while ((reader.BaseStream.Position != reader.BaseStream.Length) && (Char.IsDigit(nextChar) == true))
                {
                    valueString.Append(tempChar);
                    nextChar = (char)reader.PeekChar();
                    if (Char.IsDigit(nextChar) == true)
                    {
                        tempChar = FindNextSingleValue(reader);
                    }
                }


                int.TryParse(valueString.ToString(), out value);

                return value;
            }
        }
        #endregion

        #region ByteArrayOnBufforFunctions

        private static void OmitCommentLine(byte[] reader)
        {
            char temp = ' ';
            while (!temp.Equals('\n'))
            {
                    temp = (Char)reader[position];
                    position += 1;
            }
        }

        private static char FindNextSingleValue(byte[] reader)
        {

            char temp = ' ';
            if (position != reader.Length)
            {
                temp = (Char)reader[position];
                position += 1;
            }
            if (temp.Equals('#'))
            {
                OmitCommentLine(reader);
            }
            while ((Char.IsDigit(temp) == false))
            {
                    temp = Convert.ToChar(reader[position]);
                    position += 1;
                    if (temp.Equals('#'))
                    {
                        OmitCommentLine(reader);
                    }
            }
            return temp;

        }

        private static byte[] ReadAllBytes(this BinaryReader reader)
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

        public static StringBuilder valueString = new StringBuilder();

        #region Properties
        public static int Height
        {
            get
            {
                return height;
            }

            set
            {
                height = value;
            }
        }

        public static int Width
        {
            get
            {
                return width;
            }

            set
            {
                width = value;
            }
        }

        public static string FileName
        {
            get
            {
                return fileName;
            }

            set
            {
                fileName = value;
            }
        }
        #endregion

        private static int FindData(byte[] reader)
        {
            char temp = ' ';
            temp = Convert.ToChar(reader[position]);
            while(temp=='#')
            {
                OmitCommentLine(reader);
                temp = Convert.ToChar(reader[position]);
            }
            while(Char.IsWhiteSpace(temp))
            {
                position += 1;
                temp= Convert.ToChar(reader[position]);
            }
            StringBuilder data = new StringBuilder();

            temp = Convert.ToChar(reader[position]);
            while (Char.IsDigit(temp))
            {
                data.Append(temp);
                position += 1;
                temp = Convert.ToChar(reader[position]);
            }

            int value;
            int.TryParse(data.ToString(), out value);
            return value;
        }

        private static int FindNextValue(byte[] reader)
        {

            char tempChar = ' ';
            char nextChar = ' ';
            long current_position = position;
            if (current_position != reader.Length)
            {
                tempChar = FindNextSingleValue(reader);
                current_position = position;
                if (current_position+1 != reader.Length)
                {
                    nextChar = Convert.ToChar(reader[position]);
                }
                else
                    nextChar = ' ';
            }
            int value;

            //valueString = new StringBuilder();

            if (Char.IsDigit(nextChar) == false)
            {
                int.TryParse(tempChar.ToString(), out value);
                //position += 2;
                return value;
            }
            else
            {
                while ((position != reader.Length) && (Char.IsDigit(nextChar) == true))
                {
                    valueString.Append(tempChar);
                    nextChar = (char)reader[position];
                    if (Char.IsDigit(nextChar) == true)
                    { 
                        tempChar = FindNextSingleValue(reader);
                    }
                }
                int.TryParse(valueString.ToString(), out value);
                valueString.Clear();
                return value;
            }
        }


        #endregion
    }
}

