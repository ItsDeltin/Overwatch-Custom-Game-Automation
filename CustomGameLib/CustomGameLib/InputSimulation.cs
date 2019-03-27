using System;
using System.Windows.Forms;
using System.Threading;
using System.Collections.Generic;
using System.Drawing;

namespace Deltin.CustomGameAutomation
{
    partial class CustomGame
    {
        const int WM_LBUTTONDOWN = 0x0201;
        const int WM_LBUTTONUP = 0x0202;

        const int WM_RBUTTONDOWN = 0x0204;
        const int WM_RBUTTONUP = 0x0205;

        const int WM_MOUSEMOVE = 0x0200;

        const int WM_ACTIVATE = 0x0006;

        const uint WM_KEYDOWN = 0x100;
        const uint WM_KEYUP = 0x0101;

        const int WM_CHAR = 0x0102;
        const int WM_UNICHAR = 0x0109;

        const int WM_SYSKEYDOWN = 0x0104;
        const int WM_SYSKEYUP = 0x0105;

        // Some of Overwatch's input will not work unless Activate() is called beforehand.
        // The known instances are Opening chat and going to lobby after starting/restarting a game.
        internal void Activate()
        {
            Validate();

            User32.PostMessage(OverwatchHandle, 0x0006, 2, 0); // 0x0006 = WM_ACTIVATE 2 = WA_CLICKACTIVE
            User32.PostMessage(OverwatchHandle, 0x0086, 1, 0); // 0x0086 = WM_NCACTIVATE
            User32.PostMessage(OverwatchHandle, 0x0007, 0, 0); // 0x0007 = WM_DEVICECHANGE
        }

        private void ScreenToClient(ref int x, ref int y)
        {
            Validate();

            Point p = new Point(x, y);
            User32.ScreenToClient(OverwatchHandle, ref p);
            x = p.X;
            y = p.Y;
        }

        internal static Keys[] GetNumberKeys(int value)
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

        private static int MakeLParam(int LoWord, int HiWord)
        {
            return (int)((HiWord << 16) | (LoWord & 0xFFFF));
        }

        // Left Click
        internal void LeftClick(int x, int y, int waitTime = 500)
        {
            Validate();

            ScreenToClient(ref x, ref y);

            User32.PostMessage(OverwatchHandle, WM_ACTIVATE, 2, 0);
            User32.PostMessage(OverwatchHandle, WM_MOUSEMOVE, 0, MakeLParam(x, y));
            User32.PostMessage(OverwatchHandle, WM_LBUTTONDOWN, 0, MakeLParam(x, y));
            User32.PostMessage(OverwatchHandle, WM_LBUTTONUP, 0, MakeLParam(x, y));
            Thread.Sleep(waitTime);
        }
        internal void LeftClick(Point point, int waitTime = 500) => LeftClick(point.X, point.Y, waitTime);

        // Right Click
        internal void RightClick(int x, int y, int waitTime = 500)
        {
            Validate();

            ScreenToClient(ref x, ref y);

            User32.PostMessage(OverwatchHandle, WM_ACTIVATE, 2, 0);
            User32.PostMessage(OverwatchHandle, WM_MOUSEMOVE, 0, MakeLParam(x, y));
            Thread.Sleep(100);
            User32.PostMessage(OverwatchHandle, WM_RBUTTONDOWN, 0, MakeLParam(x, y));
            User32.PostMessage(OverwatchHandle, WM_RBUTTONUP, 0, MakeLParam(x, y));
            Thread.Sleep(waitTime);
        }
        internal void RightClick(Point point, int waitTime = 500) => RightClick(point.X, point.Y, waitTime);

        // Move Mouse
        internal void MoveMouseTo(int x, int y)
        {
            Validate();

            ScreenToClient(ref x, ref y);
            User32.PostMessage(OverwatchHandle, WM_MOUSEMOVE, 0, MakeLParam(x, y));
        }
        internal void MoveMouseTo(Point point) => MoveMouseTo(point.X, point.Y);

        // Key Press
        internal void KeyPress(int waitTime, params Keys[] keys)
        {
            Validate();
            foreach (Keys key in keys)
            {
                User32.PostMessage(OverwatchHandle, WM_KEYDOWN, (IntPtr)(key), IntPtr.Zero);
                User32.PostMessage(OverwatchHandle, WM_KEYUP, (IntPtr)(key), IntPtr.Zero);
                Thread.Sleep(waitTime);
            }
        }
        internal void KeyPress(params Keys[] keysToSend) => KeyPress(0, keysToSend);

        // Key Down
        internal void KeyDown(int waitTime, params Keys[] keysToSend)
        {
            Validate();
            foreach (Keys key in keysToSend)
            {
                User32.PostMessage(OverwatchHandle, WM_KEYDOWN, (IntPtr)(key), IntPtr.Zero);
                Thread.Sleep(waitTime);
            }
        }
        internal void KeyDown(params Keys[] keysToSend) => KeyDown(0, keysToSend);

        // Key Up
        internal void KeyUp(int waitTime, params Keys[] keysToSend)
        {
            Validate();
            foreach (Keys key in keysToSend)
            {
                User32.PostMessage(OverwatchHandle, WM_KEYUP, (IntPtr)(key), IntPtr.Zero);
                Thread.Sleep(waitTime);
            }
        }
        internal void KeyUp(params Keys[] keysToSend) => KeyUp(0, keysToSend);

        // Alternate Key Input
        internal void AlternateInput(int keycode)
        {
            Validate();
            User32.PostMessage(OverwatchHandle, WM_KEYDOWN, keycode, 0);
            User32.PostMessage(OverwatchHandle, WM_KEYUP, keycode, 0);
        }

        // Text input
        internal void TextInput(string text)
        {
            Validate();
            for (int i = 0; i < text.Length; i++)
            {
                char letter = text[i];
                User32.PostMessage(OverwatchHandle, WM_UNICHAR, (int)letter, 0);
            }
        }

        // Alt
        internal void Alt(Keys key)
        {
            Validate();
            Activate();
            Thread.Sleep(50);
            Activate();
            Thread.Sleep(50);
            User32.PostMessage(OverwatchHandle, WM_SYSKEYDOWN, 0x12, 1);
            User32.PostMessage(OverwatchHandle, WM_SYSKEYUP, (uint)key, 1);
            User32.PostMessage(OverwatchHandle, WM_KEYUP, 0x12, 0);
        }

        // Clipboard
        internal static string GetClipboard()
        {
            string clipboardText = null;
            Thread getClipboardThread = new Thread(() => clipboardText = Clipboard.GetText());
            getClipboardThread.SetApartmentState(ApartmentState.STA);
            getClipboardThread.Start();
            getClipboardThread.Join();
            return clipboardText;
        }
        internal static void SetClipboard(string text)
        {
            Thread setClipboardThread = new Thread(() => Clipboard.SetText(text));
            setClipboardThread.SetApartmentState(ApartmentState.STA); //Set the thread to STA
            setClipboardThread.Start();
            setClipboardThread.Join();
        }

        internal void SelectAll()
        {
            KeyDown(Keys.LControlKey);
            KeyDown(Keys.A);
            KeyUp(Keys.LControlKey);
        }

        internal void Copy()
        {
            KeyDown(Keys.LControlKey);
            KeyDown(Keys.C);
            KeyUp(Keys.LControlKey);
        }
    }
}
