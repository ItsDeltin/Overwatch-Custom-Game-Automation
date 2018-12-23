#if DEBUG
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using System.Threading;
using System.Diagnostics;

namespace Deltin.CustomGameAutomation
{
    partial class DebugMenu : Form
    {
        #region Fields
        private readonly CustomGame cg;

        private Image Chatbox;

        private Point SelectStartPoint;
        private Rectangle SelectRect = new Rectangle();
        private Brush SelectBrush = new SolidBrush(Color.FromArgb(128, 72, 145, 220));

        private bool SetLineMode = false;
        private Brush SetLineIndicator = new SolidBrush(Color.FromArgb(75, 204, 0, 255));
        private Rectangle SetLineRect = new Rectangle();
        #endregion

        #region Initialization
        public DebugMenu(CustomGame cg)
        {
            this.cg = cg;
            InitializeComponent();
        }
        #endregion

        #region Update Button
        private void button1_Click_1(object sender, EventArgs e)
        {
            cg.UpdateScreen();
            Bitmap chatbox = cg.Capture.CloneAsBitmap(Rectangles.LOBBY_CHATBOX);

            using (Graphics g = Graphics.FromImage(chatbox))
            {
                g.InterpolationMode = InterpolationMode.NearestNeighbor;
            }

            if (this.Chatbox != null)
                this.Chatbox.Dispose();
            this.Chatbox = chatbox;

            Chat.Invalidate();
        }
        #endregion

        #region Select
        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            if (Chatbox != null)
            {
                Draw(e, Chat, Chatbox);

                if (SelectRect != null && SelectRect.Width > 0 && SelectRect.Height > 0)
                    e.Graphics.FillRectangle(SelectBrush, ChatboxToContainer(SelectRect));

                if (SetLineRect != null)
                    e.Graphics.FillRectangle(SetLineIndicator, ChatboxToContainer(SetLineRect));

                if (LettersScanned != null && previousIndex != -1 && previousIndex < LettersScanned.Length && LettersScanned[previousIndex] != null)
                {
                    Rectangle[] rectangles = new Rectangle[LettersScanned[previousIndex].Letter.Pixel.GetLength(0)];
                    for (int r = 0; r < rectangles.Length; r++)
                        rectangles[r] = ChatboxToContainer(new Rectangle
                            (
                            x: LettersScanned[previousIndex].Location.X - Rectangles.LOBBY_CHATBOX.X + LettersScanned[previousIndex].Letter.Pixel[r, 0],
                            y: LettersScanned[previousIndex].Location.Y - Rectangles.LOBBY_CHATBOX.Y + LettersScanned[previousIndex].Letter.Pixel[r, 1],
                            width: 1, height: 1
                            ));

                    e.Graphics.FillRectangles(Brushes.IndianRed, rectangles);
                }
            }
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if (Chatbox == null)
                return;

            if (!SetLineMode)
                SelectStartPoint = ContainerToChatbox(e.Location);
            else
                SetLineMode = false;

            Invalidate();
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (Chatbox == null)
                return;

            Point selectLocation = ContainerToChatbox(e.Location);
            selectLocation.Offset(1, 0);
            selectLocation.X = Math.Min(Math.Max(selectLocation.X, 0), Chatbox.Width);
            selectLocation.Y = Math.Min(Math.Max(selectLocation.Y, 0), Chatbox.Height);

            cursorLocation.Text = $"{selectLocation.X}, {selectLocation.Y}. Line location: {SetLineRect.Y}";

            if (!SetLineMode)
            {
                if (e.Button != MouseButtons.Left)
                    return;

                SelectRect.Location = new Point(
                    Math.Min(SelectStartPoint.X, selectLocation.X),
                    Math.Min(SelectStartPoint.Y, selectLocation.Y));
                SelectRect.Size = new Size(
                    Math.Abs(SelectStartPoint.X - selectLocation.X),
                    Math.Abs(SelectStartPoint.Y - selectLocation.Y));

                Chat.Invalidate();
            }
            else
            {
                SetLineRect = new Rectangle(0, selectLocation.Y, Chat.Width, 1);
                Chat.Invalidate();
            }
        }
        #endregion

        private Point ContainerToChatbox(Point point)
        {
            float ratioX = (float)point.X / Chat.Width;
            float ratioY = (float)point.Y / Chat.Height;

            int x = (int)(Chatbox.Width * ratioX);
            int y = (int)(Chatbox.Height * ratioY);

            return new Point(x, y);
        }
        private Rectangle ContainerToChatbox(Rectangle rectangle)
        {
            float ratioX = (float)rectangle.X / Chat.Width;
            float ratioY = (float)rectangle.Y / Chat.Height;
            float ratioWidth = (float)rectangle.Width / Chat.Width;
            float ratioHeight = (float)rectangle.Height / Chat.Height;

            int x = (int)(Chatbox.Width * ratioX);
            int y = (int)(Chatbox.Height * ratioY);
            int width = (int)(Chatbox.Width * ratioWidth);
            int height = (int)(Chatbox.Height * ratioHeight);

            return new Rectangle(x, y, width, height);
        }

        private Point ChatboxToContainer(Point point)
        {
            float ratioX = (float)point.X / Chatbox.Width;
            float ratioY = (float)point.Y / Chatbox.Height;

            int x = (int)(Chat.Width * ratioX);
            int y = (int)(Chat.Height * ratioY);

            return new Point(x, y);
        }
        private Rectangle ChatboxToContainer(Rectangle rectangle)
        {
            float ratioX = (float)rectangle.X / Chatbox.Width;
            float ratioY = (float)rectangle.Y / Chatbox.Height;
            float ratioWidth = (float)rectangle.Width / Chatbox.Width;
            float ratioHeight = (float)rectangle.Height / Chatbox.Height;

            int x = (int)(Chat.Width * ratioX);
            int y = (int)(Chat.Height * ratioY);
            int width = (int)(Chat.Width * ratioWidth);
            int height = (int)(Chat.Height * ratioHeight);

            return new Rectangle(x, y, width, height);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            SetLineMode = true;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (Chatbox == null)
            {
                processError.Text = "Click Update then select an area of the image.";
                return;
            }
            if (SelectRect.IsEmpty)
            {
                processError.Text = "Select an area in the image above.";
                return;
            }
            if (SetLineRect.Y == 0)
            {
                processError.Text = "Set the line origin.";
                return;
            }
            if (characterInput.Text.Length == 0)
            {
                processError.Text = "Type in the character.";
                return;
            }

            Bitmap tmpbmp = new Bitmap(Chatbox);
            Bitmap editBitmap = tmpbmp.Clone(SelectRect, Chatbox.PixelFormat);
            tmpbmp.Dispose();

            int yOffset = (SelectRect.Y + SelectRect.Height) - (SetLineRect.Y + 1);

            int xOffset;
            bool found = false;
            for (xOffset = 0; xOffset < editBitmap.Width && !found; xOffset++)
                for (int i = 0; i < CustomGameAutomation.Chat.ChatColors.Length; i++)
                    if (editBitmap.GetPixel(xOffset, editBitmap.Height - 1).CompareColor(CustomGameAutomation.Chat.ChatColors[i], CustomGameAutomation.Chat.ChatFade))
                    {
                        found = true;
                        break;
                    }
            xOffset -= 1;

            int highX = 0;

            List<Point> points = new List<Point>();
            for (int x = 0; x < editBitmap.Width; x++)
                for (int y = 0; y < editBitmap.Height; y++)
                {
                    bool matched = false;
                    for (int i = 0; i < CustomGameAutomation.Chat.ChatColors.Length; i++)
                        if (editBitmap.GetPixel(x, y).CompareColor(CustomGameAutomation.Chat.ChatColors[i], CustomGameAutomation.Chat.ChatFade))
                        {
                            editBitmap.SetPixel(x, y, Color.Black);
                            points.Add(new Point(x - xOffset, -(SelectRect.Height - 1 - (y + yOffset))));

                            if (highX < x)
                                highX = x;

                            matched = true;
                            break;
                        }
                    if (!matched)
                    {
                        editBitmap.SetPixel(x, y, Color.White);
                    }
                }

            output.Visible = true;
            output.Text = $"new Letter(new int[,] {{{string.Join(",", points.Select(v => $"{{{v.X},{v.Y}}}"))}}}, '{characterInput.Text}', {highX}, {-xOffset})";
            copyButton.Visible = true;

            if (letterOutput.Image != null)
                letterOutput.Image.Dispose();
            letterOutput.Image = editBitmap;

            processError.Text = "Success!";
        }

        public static void Draw(PaintEventArgs e, Control control, Image image)
        {
            int width = control.Width;
            int height = (int)(width * ((double)image.Height / image.Width));

            e.Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
            e.Graphics.PixelOffsetMode = PixelOffsetMode.Half;
            e.Graphics.DrawImage(
                image,
                new Rectangle(0, 0, width, height),
                // destination rectangle 
                0,
                0,           // upper-left corner of source rectangle
                image.Width,       // width of source rectangle
                image.Height,      // height of source rectangle
                GraphicsUnit.Pixel);

            control.Height = height;
        }

        private void copyButton_Click(object sender, EventArgs e)
        {
            Thread clipboardThread = new Thread(() => 
            {
                Clipboard.SetText(output.Text);
            });
            clipboardThread.SetApartmentState(ApartmentState.STA);
            clipboardThread.IsBackground = false;
            clipboardThread.Start();
        }

        private void richTextBox1_MouseMove(object sender, MouseEventArgs e)
        {
            int index = richTextBox1.GetCharIndexFromPosition(e.Location);

            if (previousIndex == index)
                return;

            int selectStart = richTextBox1.SelectionStart,
                selectLength = richTextBox1.SelectionLength;

            if (previousIndex != -1)
            {
                richTextBox1.Select(previousIndex, 1);
                richTextBox1.SelectionColor = Color.Black;
            }

            richTextBox1.Select(index, 1);
            richTextBox1.SelectionColor = Color.DarkOrange;

            previousIndex = index;

            richTextBox1.Select(selectStart, selectLength);

            Chat.Invalidate();
        }

        private void richTextBox1_MouseLeave(object sender, EventArgs e)
        {
            if (previousIndex == -1)
                return;

            int selectStart = richTextBox1.SelectionStart,
                selectLength = richTextBox1.SelectionLength;

            richTextBox1.Select(previousIndex, 1);
            richTextBox1.SelectionColor = Color.Black;

            richTextBox1.Select(selectStart, selectLength);
            previousIndex = -1;
            Chat.Invalidate();
        }

        int previousIndex = -1;

        public void ShowScan(List<Commands.LetterResult> letterInfos)
        {
            string command = new string(letterInfos.Select(info => info?.Letter.Char ?? ' ').ToArray());

            LettersScanned = new Commands.LetterResult[command.Length];
            for (int i = 0; i < LettersScanned.Length; i++)
                LettersScanned[i] = letterInfos[i];

            SetTextBox(command);
        }

        private void SetTextBox(string value)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<string>(SetTextBox), new object[] { value });
                return;
            }
            richTextBox1.Text = value;
        }

        private Commands.LetterResult[] LettersScanned;

        private void richTextBox1_ContentsResized(object sender, ContentsResizedEventArgs e)
        {
            ((RichTextBox)sender).Height = e.NewRectangle.Height + 5;
        }
    }

    /// <summary>
    /// Inherits from PictureBox; adds Interpolation Mode Setting
    /// </summary>
    internal class PictureBoxExtended : PictureBox
    {
        public InterpolationMode InterpolationMode { get; set; }
        public PixelOffsetMode PixelOffsetMode { get; set; }

        protected override void OnPaint(PaintEventArgs paintEventArgs)
        {
            paintEventArgs.Graphics.InterpolationMode = InterpolationMode;
            paintEventArgs.Graphics.PixelOffsetMode = PixelOffsetMode;
            base.OnPaint(paintEventArgs);
        }
    }

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
        internal DebugMenu DebugMenu = null;
        private bool DebugStarted = false;

        private void SetupDebugWindow()
        {
            Task.Run(() =>
            {
                DebugMenu = new DebugMenu(this);
                DebugMenu.Load += DebugMenu_Load;
                DebugMenu.ShowDialog();
            });
            SpinWait.SpinUntil(() => { return DebugStarted; });
        }
        private void DebugMenu_Load(object sender, EventArgs e)
        {
            DebugStarted = true;
        }
    }
}
#endif