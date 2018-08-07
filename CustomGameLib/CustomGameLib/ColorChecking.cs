using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading;
using System.Drawing;

namespace Deltin.CustomGameAutomation
{
    partial class CustomGame
    {
        object BmpLock = new object();

        // Tests if a pixel is within a certain color.
        internal bool CompareColor(int x, int y, int[] color, int fade)
        {
            lock (BmpLock)
            {
                return bmp.CompareColor(x, y, color, fade);
            }
        }
        internal bool CompareColor(Point point, int[] color, int fade)
        {
            return CompareColor(point.X, point.Y, color, fade);
        }

        // Tests if a pixel's color is within another pixel's color.
        internal bool CompareColor(int x, int y, int x2, int y2, int fade)
        {
            lock (BmpLock)
            {
                return bmp.CompareColor(x, y, x2, y2, fade);
            }
        }
        internal bool CompareColor(Point point, Point point2, int fade)
        {
            return CompareColor(point.X, point.Y, point2.X, point2.Y, fade);
        }

        // Tests if a pixel's color is above the min value and below the max value.
        internal bool CompareColor(int x, int y, int[] min, int[] max)
        {
            lock (BmpLock)
            {
                return bmp.CompareColor(x, y, min, max);
            }
        }
        internal bool CompareColor(Point point, int[] min, int[] max)
        {
            return CompareColor(point.X, point.Y, min, max);
        }

        // Gets a pixel
        internal Color GetPixelAt(int x, int y)
        {
            lock (BmpLock)
            {
                return bmp.GetPixelAt(x, y);
            }
        }

        // Clones the bitmap.
        internal Bitmap BmpClone(int x, int y, int width, int height)
        {
            lock (BmpLock)
            {
                return bmp.Clone(new Rectangle(x, y, width, height), bmp.PixelFormat);
            }
        }
        internal Bitmap BmpClone(Rectangle rectangle)
        {
            return BmpClone(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
        }
        internal Bitmap BmpClone()
        {
            lock (BmpLock)
            {
                return bmp.Clone(new Rectangle(0, 0, bmp.Width, bmp.Height), bmp.PixelFormat);
            }
        }

        // Waits for a pixel to change its color to another color.
        internal bool WaitForColor(int x, int y, int[] color, int fade, int maxtime)
        {
            Stopwatch wait = new Stopwatch();
            wait.Start();
            while (wait.ElapsedMilliseconds <= maxtime)
            {
                updateScreen();
                if (CompareColor(x, y, color, fade))
                    return true;
                Thread.Sleep(10);
            }
            return false;
        }
        internal bool WaitForColor(Point point, int[] color, int fade, int maxtime)
        {
            return WaitForColor(point.X, point.Y, color, fade, maxtime);
        }

        // Waits for a pixel to change its color.
        internal bool WaitForUpdate(int x, int y, int fade, int maxtime)
        {
            Stopwatch wait = new Stopwatch();
            wait.Start();
            Color startcolor = GetPixelAt(x, y);

            while (wait.ElapsedMilliseconds <= maxtime)
            {
                updateScreen();
                Color newcolor = GetPixelAt(x, y);

                if (Math.Abs(newcolor.R - startcolor.R) > fade ||
                    Math.Abs(newcolor.G - startcolor.G) > fade ||
                    Math.Abs(newcolor.B - startcolor.B) > fade)
                    return true;

                Thread.Sleep(10);
            }

            return false;
        }
        internal bool WaitForUpdate(Point point, int fade, int maxtime)
        {
            return WaitForUpdate(point.X, point.Y, fade, maxtime);
        }
    }
}
