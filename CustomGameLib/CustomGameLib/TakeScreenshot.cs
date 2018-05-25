using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.Threading;
using System.Diagnostics;

namespace Deltin.CustomGameAutomation
{
    public enum ScreenshotMethods
    {
        BitBlt,
        ScreenCopy
    }

    partial class CustomGame
    {
        public ScreenshotMethods ScreenshotMethod;

        object screenshotLock = new object();

        // This grabs a screenshot of the Overwatch handle
        void updateScreen()
        {
            if (Disposed)
                throw new ObjectDisposedException("This CustomGame object has already been disposed.");

            if (Monitor.TryEnter(screenshotLock))
            {
                try
                {
                    Screenshot(ScreenshotMethod, OverwatchHandle, ref bmp);
                }
                finally
                {
                    Monitor.Exit(screenshotLock);
                }
            }
            else
            {
                while (!Monitor.TryEnter(screenshotLock)) Thread.Sleep(10);
                Monitor.Exit(screenshotLock);
            }
            //Monitor.Wait(screenshotLock);
        }

        static void Screenshot(ScreenshotMethods method, IntPtr hWnd, ref Bitmap bmp)
        {
            // Show the window behind all other opened windows. Screenshot does not work if Overwatch is minimized.
            SetupWindow(hWnd, method);

            if (method == ScreenshotMethods.BitBlt)
                ScreenshotBitBlt(hWnd, ref bmp);
            else if (method == ScreenshotMethods.ScreenCopy)
                ScreenshotScreenCopy(hWnd, ref bmp);
        }

        static void ScreenshotBitBlt(IntPtr hWnd, ref Bitmap bmp, bool adjust = true)
        {
            // get the hDC of the target window
            IntPtr hdcSrc = User32.GetDC(hWnd);
            // get the size
            Rectangle windowRect = new Rectangle();
            User32.GetWindowRect(hWnd, ref windowRect);
            int width = windowRect.Right - windowRect.Left;
            int height = windowRect.Bottom - windowRect.Top;
            // create a device context we can copy to
            IntPtr hdcDest = Gdi32.CreateCompatibleDC(hdcSrc);
            // create a bitmap we can copy it to,
            // using GetDeviceCaps to get the width/height
            IntPtr hBitmap = Gdi32.CreateCompatibleBitmap(hdcSrc, width, height);
            // select the bitmap object
            IntPtr hOld = Gdi32.SelectObject(hdcDest, hBitmap);
            // bitblt over
            Gdi32.BitBlt(hdcDest, 1, 31, width - 10, height, hdcSrc, 0, 0, (uint)Gdi32.TernaryRasterOperations.SRCCOPY | (uint)Gdi32.TernaryRasterOperations.CAPTUREBLT);
            // restore selection
            Gdi32.SelectObject(hdcDest, hOld);
            // clean up 
            Gdi32.DeleteDC(hdcDest);
            User32.ReleaseDC(hWnd, hdcSrc);
            // get a .NET image object for it
            Image img = Image.FromHbitmap(hBitmap);
            // free up the Bitmap object
            Gdi32.DeleteObject(hBitmap);

            if (bmp != null)
                bmp.Dispose();
            bmp = new Bitmap(img);
            img.Dispose();
        }

        static void ScreenshotScreenCopy(IntPtr hWnd, ref Bitmap bmp)
        {
            Rectangle rect = new Rectangle();
            User32.GetWindowRect(hWnd, ref rect);

            int width = rect.Right - rect.Left;
            int height = rect.Bottom - rect.Top;

            if (bmp != null)
                bmp.Dispose();
            bmp = new Bitmap(width, height);
            Graphics g = Graphics.FromImage(bmp);
            g.CopyFromScreen(rect.Left - 7, rect.Top, -14, 0, new Size(width, height), CopyPixelOperation.SourceCopy);
            g.Dispose();
        }

        /// <summary>
        /// Takes a screenshot of the Overwatch window and saves it as an image to the specified path.
        /// </summary>
        /// <param name="path">Path to save screenshot to.</param>
        public void SaveScreenshot(string path)
        {
            updateScreen();

            bmp.Save(path);
        }
    }
}
