using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Drawing.Imaging;

namespace Deltin.CustomGameAutomation
{
    internal unsafe static class Extensions
    {
        // Gets a pixel color from a bitmap without locking the bitmap from other threads. Also a lot faster.
        public static Color GetPixelAt(this Bitmap bmp, int x, int y)
        {
            Rectangle rect = new Rectangle(x, y, 1, 1);

            BitmapData bmd = bmp.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

            byte* pixel = (byte*)bmd.Scan0;
            pixel = pixel + (0 * 4);

            byte b = pixel[0];
            byte g = pixel[1];
            byte r = pixel[2];

            bmp.UnlockBits(bmd);
            return Color.FromArgb(r, g, b);
        }

        // Compares colors of bitmap
        public static bool CompareColor(this Bitmap bmp, int x, int y, int[] color, int fade)
        {
            // If the color of the pixel at the input X and Y coordinates of the input bmp is at least (int fade) units or less close to the input color variable, return false.
            return Math.Abs(bmp.GetPixelAt(x, y).R - color[0]) < fade &&
                Math.Abs(bmp.GetPixelAt(x, y).G - color[1]) < fade &&
                Math.Abs(bmp.GetPixelAt(x, y).B - color[2]) < fade;
        }
        public static bool CompareColor(this Bitmap bmp, Point point, int[] color, int fade)
        {
            return CompareColor(bmp, point.X, point.Y, color, fade);
        }

        public static bool CompareColor(this Bitmap bmp, int x, int y, int x2, int y2, int fade)
        {
            // If the color of the pixel at the input X and Y coordinates of the input bmp is at least (int fade) units or less close to the input color variable, return false.
            return Math.Abs(bmp.GetPixelAt(x, y).R - bmp.GetPixelAt(x2, y2).R) < fade &&
                Math.Abs(bmp.GetPixelAt(x, y).G - bmp.GetPixelAt(x2, y2).G) < fade &&
                Math.Abs(bmp.GetPixelAt(x, y).B - bmp.GetPixelAt(x2, y2).B) < fade;
        }

        public static bool CompareColor(this Bitmap bmp, int x, int y, int[] min, int[] max)
        {
            var pixel = bmp.GetPixelAt(x, y);
            return min[0] < pixel.R && pixel.R < max[0] &&
                   min[1] < pixel.G && pixel.G < max[1] &&
                   min[2] < pixel.B && pixel.B < max[2];
        }

        public static bool CompareColor(this Bitmap bmp1, Bitmap bmp2, int x, int y, int fade)
        {
            Color pixel1 = bmp1.GetPixelAt(x, y);
            Color pixel2 = bmp2.GetPixelAt(x, y);
            return Math.Abs(pixel1.R - pixel2.R) < fade
                && Math.Abs(pixel1.G - pixel2.G) < fade
                && Math.Abs(pixel1.B - pixel2.B) < fade;
        }

        public static bool CompareBitmaps(this Bitmap bmp1, Bitmap bmp2, int fade, int minimumPercentMatching)
        {
            double success = 0;
            for (int x = 0; x < bmp1.Width; x++)
                for (int y = 0; y < bmp2.Width; y++)
                    if (bmp1.CompareColor(bmp2, x, y, fade))
                        success++;
            return (success / (bmp1.Width * bmp1.Height)) * 100 >= minimumPercentMatching;
        }

        public static void ConvertToMarkup(this Bitmap bmp, int[] color, int fade, bool invert)
        {
            for (int x = 0; x < bmp.Width; x++)
                for (int y = 0; y < bmp.Height; y++)
                {
                    bool matches = bmp.CompareColor(x, y, color, fade);
                    if ((!invert && matches) || (invert && !matches))
                        bmp.SetPixel(x, y, Color.Black);
                    else
                        bmp.SetPixel(x, y, Color.White);
                }
        }

        public static int[] ToInt(this Color color)
        {
            return new int[] { color.R, color.G, color.B };
        }

        public static int InvertNumber(int num, int invertMax, int invertMin = 0)
        {
            int dif = invertMax - num;
            return invertMin + dif;
        }
    }
}
