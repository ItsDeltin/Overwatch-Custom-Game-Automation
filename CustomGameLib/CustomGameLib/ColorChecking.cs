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
        internal bool CompareColor(int x, int y, int[] color, int fade)
        {
            lock (BmpLock)
            {
                return bmp.CompareColor(x, y, color, fade);
            }
        }

        internal bool CompareColor(int x, int y, int x2, int y2, int fade)
        {
            lock (BmpLock)
            {
                return bmp.CompareColor(x, y, x2, y2, fade);
            }
        }

        internal bool CompareColor(int x, int y, int[] min, int[] max)
        {
            lock (BmpLock)
            {
                return bmp.CompareColor(x, y, min, max);
            }
        }

        internal Color GetPixelAt(int x, int y)
        {
            lock (BmpLock)
            {
                return bmp.GetPixelAt(x, y);
            }
        }

        internal Bitmap BmpClone(int x, int y, int width, int height)
        {
            lock (BmpLock)
            {
                return bmp.Clone(new Rectangle(x, y, width, height), bmp.PixelFormat);
            }
        }

        internal Bitmap BmpClone()
        {
            lock (BmpLock)
            {
                return bmp.Clone(new Rectangle(0, 0, bmp.Width, bmp.Height), bmp.PixelFormat);
            }
        }

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
    }
}
