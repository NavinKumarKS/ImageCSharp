using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CannyImage
{
    struct G
    {
        int magnitude;  //gradient magnitude
        int part;       //partition of gradient
        public G(int magnitude, int part)
        {
            this.magnitude = magnitude;
            this.part = part;
        }
    };
    class Program
    {
        static void Main()
        {
            Bitmap original = new Bitmap(@"C:\Image\KCL_3700.jpg");
            BitmapData bitmapData = original.LockBits(new Rectangle(0, 0, original.Width, original.Height), ImageLockMode.ReadWrite, original.PixelFormat);
            int bytesPerPixel = System.Drawing.Bitmap.GetPixelFormatSize(bitmapData.PixelFormat) / 8;
            int heightInPixels = bitmapData.Height;
            int widthInBytes = bitmapData.Width * bytesPerPixel;
            unsafe
            {
                byte* ptrFirstPixel = (byte*)bitmapData.Scan0;
                byte[,] color = new byte[widthInBytes, heightInPixels];
                #region RGB to gray level
                for (int y = 0; y < heightInPixels; y++)
                {
                    byte* currentLine = ptrFirstPixel + (y * bitmapData.Stride);
                    for (int x = 0; x < widthInBytes; x = x + bytesPerPixel)
                    {
                        int B = currentLine[x];
                        int G = currentLine[x + 1];
                        int R = currentLine[x + 2];

                        var r = (R * 0.299);
                        var g = (G * 0.587);
                        var b = (B * 0.114);
                        var avg = (r + g + b);

                        currentLine[x] = (byte)avg;
                        currentLine[x + 1] = (byte)avg;
                        currentLine[x + 2] = (byte)avg;
                        color[x, y] = (byte)avg;
                    }
                }

                #endregion

                int[,] Gaussian3 = new int[3, 3] { { 1, 2, 1 }, { 2, 4, 2 }, { 1, 2, 1 } };

                int n = 3;
                for (int y = 0; y < heightInPixels; y++)
                {
                    byte* currentLine = ptrFirstPixel + (y * bitmapData.Stride);
                    for (int x = 0; x < widthInBytes; x += 3)
                    {
                        int sum = 0;
                        for (int dy = -(n / 2), i = 0; dy <= n / 2; dy++, i++)
                            for (int dx = -(n / 2), j = 0; dx <= n / 2; dx++, j++)
                            {
                                var _dy = y + dy;
                                var _dx = x / 3 + dx;
                                sum += color[_dx < 0 ? 0 : _dx, _dy < 0 ? 0 : _dy] * Gaussian3[i, j];
                            }
                        color[x / 3, y] = currentLine[y * widthInBytes + x] = currentLine[y * widthInBytes + x + 1] = currentLine[y * widthInBytes + x + 2] = (byte)(sum / 16);
                    }
                }
                original.UnlockBits(bitmapData);
            }
            #region noise removal


            #endregion
            //int[,] sobel_dx = new int[3, 3] { { -1, 0, 1 }, { -2, 0, 2 }, { -1, 0, 1 } };
            //int[,] sobel_dy = new int[3, 3] { { -1, -2, -1 }, { 0, 0, 0 }, { 1, 2, 1 } };




            var path = string.Format(@"{0}\{1}.jpg", Path.GetTempPath(), DateTime.Now.Ticks);
            original.Save(path);
        }



    }
}