using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Drawing.Imaging;

namespace Deltin.CustomGameAutomation
{
    unsafe static class Extensions
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
            if (Math.Abs(bmp.GetPixelAt(x, y).R - color[0]) < fade &&
                Math.Abs(bmp.GetPixelAt(x, y).G - color[1]) < fade &&
                Math.Abs(bmp.GetPixelAt(x, y).B - color[2]) < fade)
            {
                return true;
            }
            return false;
        }

        public static bool CompareColor(this Bitmap bmp, int x, int y, int x2, int y2, int fade)
        {
            // If the color of the pixel at the input X and Y coordinates of the input bmp is at least (int fade) units or less close to the input color variable, return false.
            if (Math.Abs(bmp.GetPixelAt(x, y).R - bmp.GetPixelAt(x2, y2).R) < fade &&
                Math.Abs(bmp.GetPixelAt(x, y).G - bmp.GetPixelAt(x2, y2).G) < fade &&
                Math.Abs(bmp.GetPixelAt(x, y).B - bmp.GetPixelAt(x2, y2).B) < fade)
            {
                return true;
            }
            return false;
        }

        // Inverts a number
        public static int InvertNumber(int num, int invertMax, int invertMin = 0)
        {
            int dif = invertMax - num;
            return invertMin + dif;
        }
    }
}
