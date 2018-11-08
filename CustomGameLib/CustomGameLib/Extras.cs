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
