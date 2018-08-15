using System;
using System.Windows.Forms;
using System.Threading;
using System.Collections.Generic;
using System.Drawing;

namespace Deltin.CustomGameAutomation
{
    partial class CustomGame
    {
        // Some of Overwatch's input will not work unless Activate() is called beforehand.
        // The known instances are Opening chat and going to lobby after starting/restarting a game.
        static internal void Activate(IntPtr hWnd)
        {
            User32.PostMessage(hWnd, 0x0006, 2, 0); // 0x0006 = WM_ACTIVATE 2 = WA_CLICKACTIVE
            User32.PostMessage(hWnd, 0x0086, 1, 0); // 0x0086 = WM_NCACTIVATE
            User32.PostMessage(hWnd, 0x0007, 0, 0); // 0x0007 = WM_DEVICECHANGE
        }
        internal void Activate()
        {
            Activate(OverwatchHandle);
        }

        static void ScreenToClient(IntPtr hWnd, ref int x, ref int y)
        {
            Point p = new Point(x, y);
            User32.ScreenToClient(hWnd, ref p);
            x = p.X;
            y = p.Y;
        }
        void ScreenToClient(ref int x, ref int y)
        {
            ScreenToClient(OverwatchHandle, ref x, ref y);
        }

        const int WM_LBUTTONDOWN = 0x0201;
        const int WM_LBUTTONUP = 0x0202;

        const int WM_RBUTTONDOWN = 0x0204;
        const int WM_RBUTTONUP = 0x0205;

        const int WM_MOUSEMOVE = 0x0200;

        const int WM_ACTIVATE = 0x0006;

        const uint WM_KEYDOWN = 0x100;
        const uint WM_KEYUP = 0x0101;

        internal Keys[] GetNumberKeys(int value)
        {
            Keys[] numberKeys = new Keys[] { Keys.D0, Keys.D1, Keys.D2, Keys.D3, Keys.D4, Keys.D5, Keys.D6, Keys.D7, Keys.D8, Keys.D9 };

            List<Keys> keys = new List<Keys>();

            string get = value.ToString();
            for (int i = 0; i < get.Length; i++)
                if (get[i] == '-')
                    keys.Add(Keys.Subtract);
                else
                    keys.Add(numberKeys[Int32.Parse(get[i].ToString())]);

            return keys.ToArray();
        }

        static int MakeLParam(int LoWord, int HiWord)
        {
            return (int)((HiWord << 16) | (LoWord & 0xFFFF));
        }

        internal void LeftClick(int x, int y, int waitTime = 500)
        {
            LeftClick(OverwatchHandle, x, y, waitTime);
        }
        internal void LeftClick(Point point, int waitTime = 500)
        {
            LeftClick(point.X, point.Y, waitTime);
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

        internal void RightClick(int x, int y, int waitTime = 500)
        {
            RightClick(OverwatchHandle, x, y, waitTime);
        }
        internal void RightClick(Point point, int waitTime = 500)
        {
            RightClick(point.X, point.Y, waitTime);
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
        internal Point SavedCursorPosition;
        internal Point Cursor
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

        internal void MoveMouseTo(int x, int y)
        {
            MoveMouseTo(OverwatchHandle, x, y);
        }
        static void MoveMouseTo(IntPtr hWnd, int x, int y)
        {
            ScreenToClient(hWnd, ref x, ref y);
            User32.PostMessage(hWnd, WM_MOUSEMOVE, 0, MakeLParam(x, y));
        }

        internal void KeyDown(params Keys[] keysToSend)
        {
            KeyDown(OverwatchHandle, keysToSend);
        }
        internal void KeyUp(params Keys[] keysToSend)
        {
            KeyUp(OverwatchHandle, keysToSend);
        }
        internal void KeyPress(params Keys[] keysToSend)
        {
            KeyPress(OverwatchHandle, keysToSend);
        }
        // With waitTime
        internal void KeyDown(int waitTime, params Keys[] keysToSend)
        {
            KeyDown(OverwatchHandle, waitTime, keysToSend);
        }
        internal void KeyUp(int waitTime, params Keys[] keysToSend)
        {
            KeyUp(OverwatchHandle, waitTime, keysToSend);
        }
        internal void KeyPress(int waitTime, params Keys[] keysToSend)
        {
            KeyPress(OverwatchHandle, waitTime, keysToSend);
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
        // With waitTime
        static void KeyDown(IntPtr hWnd, int waitTime, params Keys[] keysToSend)
        {
            foreach (Keys key in keysToSend)
            {
                User32.PostMessage(hWnd, WM_KEYDOWN, (IntPtr)(key), IntPtr.Zero);
                Thread.Sleep(waitTime);
            }
        }
        static void KeyUp(IntPtr hWnd, int waitTime, params Keys[] keysToSend)
        {
            foreach (Keys key in keysToSend)
            {
                User32.PostMessage(hWnd, WM_KEYUP, (IntPtr)(key), IntPtr.Zero);
                Thread.Sleep(waitTime);
            }
        }
        static void KeyPress(IntPtr hWnd, int waitTime, params Keys[] keysToSend)
        {
            foreach (Keys key in keysToSend)
            {
                User32.PostMessage(hWnd, WM_KEYDOWN, (IntPtr)(key), IntPtr.Zero);
                User32.PostMessage(hWnd, WM_KEYUP, (IntPtr)(key), IntPtr.Zero);
                Thread.Sleep(waitTime);
            }
        }

        internal void AlternateInput(int keycode)
        {
            AlternateInput(OverwatchHandle, keycode);
        }
        static void AlternateInput(IntPtr hWnd, int keycode)
        {
            User32.PostMessage(hWnd, WM_KEYDOWN, keycode, 0);
            User32.PostMessage(hWnd, WM_KEYUP, keycode, 0);
        }

        const int WM_CHAR = 0x0102;

        internal void TextInput(string text)
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
