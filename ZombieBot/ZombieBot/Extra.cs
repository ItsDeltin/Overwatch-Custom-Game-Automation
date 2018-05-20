using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace ZombieBot
{
    public static class Extra
    {
        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        public static int SquashArray(int[] list, int count)
        {
            count++;
            int a = 0;
            for (int i = 0; i < count; i++)
                a += list[i];
            return a;
        }
        public static int SquashArray(List<int> list, int count)
        {
            return SquashArray(list.ToArray(), count);
        }

        public static string ConsoleInput(string send, params string[] validinputs)
        {
            Console.Write(send);
            int line = Console.CursorTop;
            while (true)
            {
                string input = Console.ReadLine().ToLower();
                if (validinputs.Contains(input))
                    return input;
                else
                {
                    Console.SetCursorPosition(send.Length, line);
                    Console.Write(string.Concat(Enumerable.Repeat(" ", input.Length)));
                    Console.SetCursorPosition(send.Length, line);
                }
            }
        }

        public static string GetExecutingDirectory()
        {
            string[] executorFilePath = System.Reflection.Assembly.GetExecutingAssembly().Location.Split('\\');
            return String.Join("\\", executorFilePath, 0, executorFilePath.Length - 1) + "\\";
        }
    }
}
