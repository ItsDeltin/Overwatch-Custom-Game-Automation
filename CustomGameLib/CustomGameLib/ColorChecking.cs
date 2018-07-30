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

        bool CompareColor(Point point, int[] color, int fade)
        {
            return CompareColor(point.X, point.Y, color, fade);
        }

        bool CompareColor(int x, int y, int[] color, int fade)
        {
            lock (BmpLock)
            {
                return bmp.CompareColor(x, y, color, fade);
            }
        }

        bool CompareColor(Point point, Point point2, int fade)
        {
            return CompareColor(point.X, point.Y, point2.X, point2.Y, fade);
        }

        bool CompareColor(int x, int y, int x2, int y2, int fade)
        {
            lock (BmpLock)
            {
                return bmp.CompareColor(x, y, x2, y2, fade);
            }
        }

        bool CompareColor(Point point, int[] min, int[] max)
        {
            return CompareColor(point.X, point.Y, min, max);
        }

        bool CompareColor(int x, int y, int[] min, int[] max)
        {
            lock (BmpLock)
            {
                return bmp.CompareColor(x, y, min, max);
            }
        }

        Color GetPixelAt(Point point)
        {
            return GetPixelAt(point.X, point.Y);
        }

        Color GetPixelAt(int x, int y)
        {
            lock (BmpLock)
            {
                return bmp.GetPixelAt(x, y);
            }
        }

        Bitmap BmpClone(Point point, int width, int height)
        {
            return BmpClone(point.X, point.Y, width, height);
        }

        Bitmap BmpClone(int x, int y, int width, int height)
        {
            lock (BmpLock)
            {
                return bmp.Clone(new Rectangle(x, y, width, height), bmp.PixelFormat);
            }
        }

        Bitmap BmpClone()
        {
            lock (BmpLock)
            {
                return bmp.Clone(new Rectangle(0, 0, bmp.Width, bmp.Height), bmp.PixelFormat);
            }
        }

        bool WaitForColor(Point point, int[] color, int fade, int maxtime)
        {
            return WaitForColor(point.X, point.Y, color, fade, maxtime);
        }

        bool WaitForColor(int x, int y, int[] color, int fade, int maxtime)
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

        bool WaitForUpdate(Point point, int fade, int maxtime)
        {
            return WaitForUpdate(point.X, point.Y, fade, maxtime);
        }

        bool WaitForUpdate(int x, int y, int fade, int maxtime)
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
    }
}
