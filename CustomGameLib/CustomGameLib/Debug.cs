#if DEBUG
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
#if DEBUG
        private void SetupDebugWindow()
        {
            Task.Run(() =>
            {
                debug = new Form
                {
                    Width = 1500,
                    Height = 1000
                };
                debug.Show();
                g = debug.CreateGraphics();
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                Application.Run(debug);
            });
        }

        internal Form debug;
        internal Graphics g;
#endif
    }
}
#endif