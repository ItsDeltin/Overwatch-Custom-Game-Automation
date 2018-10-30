using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace Deltin.CustomGameAutomation
{
    internal static class Extensions
    {
        /*
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
        */

        public static int[] ToInt(this Color color)
        {
            return new int[] { color.R, color.G, color.B };
        }

        public static bool CompareColor(this Color color, Color other, int fade)
        {
            return CompareColor(color, other.ToInt(), fade);
        }
        public static bool CompareColor(this Color color, int[] other, int fade)
        {
            return Math.Abs(color.R - other[0]) < fade
                && Math.Abs(color.G - other[1]) < fade
                && Math.Abs(color.B - other[2]) < fade;
        }

        public static int InvertNumber(int num, int invertMax, int invertMin = 0)
        {
            int dif = invertMax - num;
            return invertMin + dif;
        }
    }
}
