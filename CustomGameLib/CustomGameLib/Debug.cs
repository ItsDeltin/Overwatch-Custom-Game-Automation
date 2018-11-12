#if DEBUG
#undef DEBUG_WINDOW

using System.Diagnostics;
using System.Windows.Forms;
using System.Drawing;
using System.Threading.Tasks;

namespace Deltin.CustomGameAutomation
{
    internal class CustomGameDebug
    {
        internal const string DebugHeader = "[CGA]";

        public static void WriteLine(string text)
        {
            Debug.WriteLine(Format(text));
        }

        private static string Format(string text)
        {
            return DebugHeader + " " + text;
        }
    }

    partial class CustomGame
    {
#if DEBUG_WINDOW
        private void SetupDebugWindow()
        {
            new Task(() =>
            {
                debug = new Form();
                debug.Width = 1500;
                debug.Height = 1000;
                debug.Show();
                g = debug.CreateGraphics();
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                Application.Run(debug);
            }).Start();
        }

        internal Form debug;
        internal Graphics g;
#endif
    }
}
#endif