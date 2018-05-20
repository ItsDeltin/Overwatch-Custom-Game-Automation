using System;
using System.Windows.Forms;
using System.Threading;
using System.Runtime.InteropServices;
using System.Drawing;

namespace Deltin.CustomGameAutomation
{
    partial class CustomGame
    {
        void ScreenToClient(ref int x, ref int y)
        {
            ScreenToClient(OverwatchHandle, ref x, ref y);
        }

        static void ScreenToClient(IntPtr hWnd, ref int x, ref int y)
        {
            Point p = new Point(x, y);
            User32.ScreenToClient(hWnd, ref p);
            x = p.X;
            y = p.Y;
        }

        const int WM_LBUTTONDOWN = 0x0201;
        const int WM_LBUTTONUP = 0x0202;

        const int WM_RBUTTONDOWN = 0x0204;
        const int WM_RBUTTONUP = 0x0205;

        const int WM_MOUSEMOVE = 0x0200;

        const int WM_ACTIVATE = 0x0006;

        const uint WM_KEYDOWN = 0x100;
        const uint WM_KEYUP = 0x0101;

        Keys[] NumberKeys = new Keys[] { Keys.D0, Keys.D1, Keys.D2, Keys.D3, Keys.D4, Keys.D5, Keys.D6, Keys.D7, Keys.D8, Keys.D9 };

        static int MakeLParam(int LoWord, int HiWord)
        {
            return (int)((HiWord << 16) | (LoWord & 0xFFFF));
        }

        public void ClickTest(int x, int y)
        {
            LeftClick(x, y);
        }

        void LeftClick(int x, int y, int waitTime = 500)
        {
            LeftClick(OverwatchHandle, x, y, waitTime);
        }

        static void LeftClick(IntPtr hWnd, int x, int y, int waitTime = 500)
        {
            ScreenToClient(hWnd, ref x, ref y);

            User32.PostMessage(hWnd, WM_ACTIVATE, 2, 0);
            User32.PostMessage(hWnd, WM_MOUSEMOVE, 0, MakeLParam(x, y));
            User32.PostMessage(hWnd, WM_LBUTTONDOWN, 0, MakeLParam(x, y));
            User32.PostMessage(hWnd, WM_LBUTTONUP, 0, MakeLParam(x, y));
            Thread.Sleep(waitTime);
        }

        void RightClick(int x, int y, int waitTime = 500)
        {
            RightClick(OverwatchHandle, x, y, waitTime);
        }

        static void RightClick(IntPtr hWnd, int x, int y, int waitTime = 500)
        {
            ScreenToClient(hWnd, ref x, ref y);

            User32.PostMessage(hWnd, WM_ACTIVATE, 2, 0);
            User32.PostMessage(hWnd, WM_MOUSEMOVE, 0, MakeLParam(x, y));
            Thread.Sleep(100);
            User32.PostMessage(hWnd, WM_RBUTTONDOWN, 0, MakeLParam(x, y));
            User32.PostMessage(hWnd, WM_RBUTTONUP, 0, MakeLParam(x, y));
            Thread.Sleep(waitTime);
        }

        // Cursor position
        Point SavedCursorPosition;
        Point Cursor
        {
            get
            {
                return SavedCursorPosition;
            }
            set
            {
                SavedCursorPosition = value;
                MoveMouseTo(value.X, value.Y);
            }
        }

        void MoveMouseTo(int x, int y)
        {
            MoveMouseTo(OverwatchHandle, x, y);
        }

        static void MoveMouseTo(IntPtr hWnd, int x, int y)
        {
            ScreenToClient(hWnd, ref x, ref y);
            User32.PostMessage(hWnd, WM_MOUSEMOVE, 0, MakeLParam(x, y));
        }

        void KeyDown(params Keys[] keysToSend)
        {
            KeyDown(OverwatchHandle, keysToSend);
        }
        void KeyUp(params Keys[] keysToSend)
        {
            KeyUp(OverwatchHandle, keysToSend);
        }
        void KeyPress(params Keys[] keysToSend)
        {
            KeyPress(OverwatchHandle, keysToSend);
        }

        static void KeyDown(IntPtr hWnd, params Keys[] keysToSend)
        {
            foreach (Keys key in keysToSend)
            {
                User32.PostMessage(hWnd, WM_KEYDOWN, (IntPtr)(key), IntPtr.Zero);
            }
        }

        static void KeyUp(IntPtr hWnd, params Keys[] keysToSend)
        {
            foreach (Keys key in keysToSend)
            {
                User32.PostMessage(hWnd, WM_KEYUP, (IntPtr)(key), IntPtr.Zero);
            }
        }

        static void KeyPress(IntPtr hWnd, params Keys[] keysToSend)
        {
            foreach (Keys key in keysToSend)
            {
                User32.PostMessage(hWnd, WM_KEYDOWN, (IntPtr)(key), IntPtr.Zero);
                User32.PostMessage(hWnd, WM_KEYUP, (IntPtr)(key), IntPtr.Zero);
            }
        }

        void AlternateInput(int keycode)
        {
            AlternateInput(OverwatchHandle, keycode);
        }

        static void AlternateInput(IntPtr hWnd, int keycode)
        {
            User32.PostMessage(hWnd, WM_KEYDOWN, keycode, 0);
            User32.PostMessage(hWnd, WM_KEYUP, keycode, 0);
        }

        const int WM_CHAR = 0x0102;

        void TextInput(string text)
        {
            TextInput(OverwatchHandle, text);
        }

        static void TextInput(IntPtr hWnd, string text)
        {
            for(int i = 0; i < text.Length; i++)
            {
                char letter = text[i];
                User32.PostMessage(hWnd, WM_CHAR, (int)letter, 0);
            }
        }
    }
}
