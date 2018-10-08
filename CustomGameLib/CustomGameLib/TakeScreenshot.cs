using System;
using System.Drawing;
using System.Threading;
using System.Collections.Generic;

namespace Deltin.CustomGameAutomation
{
    /// <summary>
    /// The screenshot method used to capture the Overwatch window screen.
    /// BitBlt is faster and works even if another window is over the Overwatch window.
    /// If BitBlt does not work for you, use ScreenCopy.
    /// </summary>
    public enum ScreenshotMethod
    {
        /// <summary>
        /// The BitBlt method of screen capturing.
        /// </summary>
        BitBlt,
        /// <summary>
        /// The ScreenCopy method of screen capturing.
        /// </summary>
        ScreenCopy
    }

    partial class CustomGame
    {
        ScreenshotMethod ScreenshotMethod;

        object ScreenshotLock = new object();

        // This grabs a screenshot of the Overwatch handle
        internal void updateScreen()
        {
            if (Disposed)
                throw new ObjectDisposedException("This CustomGame object has already been disposed.");

            // This will take a screenshot of the Overwatch window.
            if (Monitor.TryEnter(ScreenshotLock)) // (1) If another thread is already updating the screen...
            {
                lock (BmpLock)
                {
                    try
                    {
                        Screenshot(ScreenshotMethod, OverwatchHandle, ref bmp);
                    }
                    finally
                    {
                        Monitor.Exit(ScreenshotLock);
                    }
                }
            }
            else
            {
                // (1) ...Just wait for the thread to finish updating it then continue.
                //Monitor.Wait(screenshotLock);
                while (!Monitor.TryEnter(ScreenshotLock)) Thread.Sleep(10);
                Monitor.Exit(ScreenshotLock);
            }
        }

        static void Screenshot(ScreenshotMethod method, IntPtr hWnd, ref Bitmap bmp)
        {
            // Show the window behind all other opened windows. Screenshot does not work if Overwatch is minimized.
            SetupWindow(hWnd, method);

            if (method == ScreenshotMethod.BitBlt)
                ScreenshotBitBlt(hWnd, ref bmp);
            else if (method == ScreenshotMethod.ScreenCopy)
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
