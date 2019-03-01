#if DEBUG
namespace Deltin.CustomGameAutomation
{
    partial class DebugMenu
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.cursorLocation = new System.Windows.Forms.Label();
            this.Chat = new System.Windows.Forms.PictureBox();
            this.label1 = new System.Windows.Forms.Label();
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.CharacterText = new System.Windows.Forms.Label();
            this.characterInput = new System.Windows.Forms.TextBox();
            this.panel2 = new System.Windows.Forms.Panel();
            this.updateButton = new System.Windows.Forms.Button();
            this.setlineButton = new System.Windows.Forms.Button();
            this.processButton = new System.Windows.Forms.Button();
            this.processError = new System.Windows.Forms.Label();
            this.letterOutput = new Deltin.CustomGameAutomation.PictureBoxExtended();
            this.panel3 = new System.Windows.Forms.Panel();
            this.copyButton = new System.Windows.Forms.Button();
            this.output = new System.Windows.Forms.TextBox();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.commandsTab = new System.Windows.Forms.TabPage();
            this.imageDebugTab = new System.Windows.Forms.TabPage();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.imageDebuggerContainer = new System.Windows.Forms.Panel();
            this.imageDebugger = new Deltin.CustomGameAutomation.PictureBoxExtended();
            this.tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.Chat)).BeginInit();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.letterOutput)).BeginInit();
            this.panel3.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.commandsTab.SuspendLayout();
            this.imageDebugTab.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.imageDebuggerContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.imageDebugger)).BeginInit();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.AutoScroll = true;
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.Controls.Add(this.cursorLocation, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.Chat, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.label1, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.richTextBox1, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.panel1, 0, 4);
            this.tableLayoutPanel1.Controls.Add(this.panel2, 0, 5);
            this.tableLayoutPanel1.Controls.Add(this.letterOutput, 0, 6);
            this.tableLayoutPanel1.Controls.Add(this.panel3, 0, 7);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 8;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(524, 458);
            this.tableLayoutPanel1.TabIndex = 2;
            // 
            // cursorLocation
            // 
            this.cursorLocation.AutoSize = true;
            this.cursorLocation.Location = new System.Drawing.Point(3, 0);
            this.cursorLocation.Name = "cursorLocation";
            this.cursorLocation.Size = new System.Drawing.Size(10, 13);
            this.cursorLocation.TabIndex = 2;
            this.cursorLocation.Text = "-";
            // 
            // Chat
            // 
            this.Chat.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.Chat.Location = new System.Drawing.Point(3, 16);
            this.Chat.Name = "Chat";
            this.Chat.Size = new System.Drawing.Size(610, 10);
            this.Chat.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.Chat.TabIndex = 0;
            this.Chat.TabStop = false;
            this.Chat.Paint += new System.Windows.Forms.PaintEventHandler(this.pictureBox1_Paint);
            this.Chat.MouseDown += new System.Windows.Forms.MouseEventHandler(this.pictureBox1_MouseDown);
            this.Chat.MouseMove += new System.Windows.Forms.MouseEventHandler(this.pictureBox1_MouseMove);
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(3, 29);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(78, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Scanned Text:";
            // 
            // richTextBox1
            // 
            this.richTextBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.richTextBox1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.richTextBox1.Cursor = System.Windows.Forms.Cursors.Default;
            this.richTextBox1.DetectUrls = false;
            this.richTextBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.richTextBox1.Location = new System.Drawing.Point(3, 45);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.ReadOnly = true;
            this.richTextBox1.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.None;
            this.richTextBox1.Size = new System.Drawing.Size(610, 32);
            this.richTextBox1.TabIndex = 8;
            this.richTextBox1.Text = "";
            this.richTextBox1.ContentsResized += new System.Windows.Forms.ContentsResizedEventHandler(this.richTextBox1_ContentsResized);
            this.richTextBox1.MouseLeave += new System.EventHandler(this.richTextBox1_MouseLeave);
            this.richTextBox1.MouseMove += new System.Windows.Forms.MouseEventHandler(this.richTextBox1_MouseMove);
            // 
            // panel1
            // 
            this.panel1.AutoSize = true;
            this.panel1.Controls.Add(this.CharacterText);
            this.panel1.Controls.Add(this.characterInput);
            this.panel1.Location = new System.Drawing.Point(3, 83);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(159, 26);
            this.panel1.TabIndex = 3;
            // 
            // CharacterText
            // 
            this.CharacterText.AutoSize = true;
            this.CharacterText.Location = new System.Drawing.Point(3, 6);
            this.CharacterText.Name = "CharacterText";
            this.CharacterText.Size = new System.Drawing.Size(56, 13);
            this.CharacterText.TabIndex = 0;
            this.CharacterText.Text = "Character:";
            // 
            // characterInput
            // 
            this.characterInput.Location = new System.Drawing.Point(65, 3);
            this.characterInput.MaxLength = 1;
            this.characterInput.Name = "characterInput";
            this.characterInput.Size = new System.Drawing.Size(91, 20);
            this.characterInput.TabIndex = 1;
            // 
            // panel2
            // 
            this.panel2.AutoSize = true;
            this.panel2.Controls.Add(this.updateButton);
            this.panel2.Controls.Add(this.setlineButton);
            this.panel2.Controls.Add(this.processButton);
            this.panel2.Controls.Add(this.processError);
            this.panel2.Location = new System.Drawing.Point(3, 115);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(268, 29);
            this.panel2.TabIndex = 6;
            // 
            // updateButton
            // 
            this.updateButton.Location = new System.Drawing.Point(3, 3);
            this.updateButton.Name = "updateButton";
            this.updateButton.Size = new System.Drawing.Size(75, 23);
            this.updateButton.TabIndex = 2;
            this.updateButton.Text = "Update";
            this.updateButton.UseVisualStyleBackColor = true;
            this.updateButton.Click += new System.EventHandler(this.button1_Click_1);
            // 
            // setlineButton
            // 
            this.setlineButton.Location = new System.Drawing.Point(84, 3);
            this.setlineButton.Name = "setlineButton";
            this.setlineButton.Size = new System.Drawing.Size(75, 23);
            this.setlineButton.TabIndex = 3;
            this.setlineButton.Text = "Set Line";
            this.setlineButton.UseVisualStyleBackColor = true;
            this.setlineButton.Click += new System.EventHandler(this.button2_Click);
            // 
            // processButton
            // 
            this.processButton.Location = new System.Drawing.Point(165, 3);
            this.processButton.Name = "processButton";
            this.processButton.Size = new System.Drawing.Size(94, 23);
            this.processButton.TabIndex = 4;
            this.processButton.Text = "Process Letter";
            this.processButton.UseVisualStyleBackColor = true;
            this.processButton.Click += new System.EventHandler(this.button3_Click);
            // 
            // processError
            // 
            this.processError.AutoSize = true;
            this.processError.Location = new System.Drawing.Point(265, 8);
            this.processError.Name = "processError";
            this.processError.Size = new System.Drawing.Size(0, 13);
            this.processError.TabIndex = 5;
            // 
            // letterOutput
            // 
            this.letterOutput.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
            this.letterOutput.Location = new System.Drawing.Point(3, 150);
            this.letterOutput.Name = "letterOutput";
            this.letterOutput.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;
            this.letterOutput.Size = new System.Drawing.Size(268, 114);
            this.letterOutput.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.letterOutput.TabIndex = 4;
            this.letterOutput.TabStop = false;
            // 
            // panel3
            // 
            this.panel3.AutoSize = true;
            this.panel3.Controls.Add(this.copyButton);
            this.panel3.Controls.Add(this.output);
            this.panel3.Location = new System.Drawing.Point(3, 270);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(590, 30);
            this.panel3.TabIndex = 7;
            // 
            // copyButton
            // 
            this.copyButton.Location = new System.Drawing.Point(6, 4);
            this.copyButton.Name = "copyButton";
            this.copyButton.Size = new System.Drawing.Size(75, 23);
            this.copyButton.TabIndex = 6;
            this.copyButton.Text = "Copy";
            this.copyButton.UseVisualStyleBackColor = true;
            this.copyButton.Visible = false;
            this.copyButton.Click += new System.EventHandler(this.copyButton_Click);
            // 
            // output
            // 
            this.output.AcceptsTab = true;
            this.output.Location = new System.Drawing.Point(87, 6);
            this.output.Name = "output";
            this.output.ReadOnly = true;
            this.output.Size = new System.Drawing.Size(500, 20);
            this.output.TabIndex = 5;
            this.output.Visible = false;
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.commandsTab);
            this.tabControl1.Controls.Add(this.imageDebugTab);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(538, 490);
            this.tabControl1.TabIndex = 2;
            // 
            // commandsTab
            // 
            this.commandsTab.Controls.Add(this.tableLayoutPanel1);
            this.commandsTab.Location = new System.Drawing.Point(4, 22);
            this.commandsTab.Name = "commandsTab";
            this.commandsTab.Padding = new System.Windows.Forms.Padding(3);
            this.commandsTab.Size = new System.Drawing.Size(530, 464);
            this.commandsTab.TabIndex = 0;
            this.commandsTab.Text = "Commands";
            this.commandsTab.UseVisualStyleBackColor = true;
            // 
            // imageDebugTab
            // 
            this.imageDebugTab.Controls.Add(this.tableLayoutPanel2);
            this.imageDebugTab.Location = new System.Drawing.Point(4, 22);
            this.imageDebugTab.Name = "imageDebugTab";
            this.imageDebugTab.Padding = new System.Windows.Forms.Padding(3);
            this.imageDebugTab.Size = new System.Drawing.Size(530, 464);
            this.imageDebugTab.TabIndex = 1;
            this.imageDebugTab.Text = "Debugger";
            this.imageDebugTab.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 1;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.Controls.Add(this.imageDebuggerContainer, 0, 2);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 2;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(524, 458);
            this.tableLayoutPanel2.TabIndex = 3;
            // 
            // imageDebuggerContainer
            // 
            this.imageDebuggerContainer.AutoScroll = true;
            this.imageDebuggerContainer.Controls.Add(this.imageDebugger);
            this.imageDebuggerContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.imageDebuggerContainer.Location = new System.Drawing.Point(0, 0);
            this.imageDebuggerContainer.Margin = new System.Windows.Forms.Padding(0);
            this.imageDebuggerContainer.Name = "imageDebuggerContainer";
            this.imageDebuggerContainer.Size = new System.Drawing.Size(534, 458);
            this.imageDebuggerContainer.TabIndex = 1;
            // 
            // imageDebugger
            // 
            this.imageDebugger.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
            this.imageDebugger.Location = new System.Drawing.Point(0, -16);
            this.imageDebugger.Margin = new System.Windows.Forms.Padding(0);
            this.imageDebugger.Name = "imageDebugger";
            this.imageDebugger.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Default;
            this.imageDebugger.Size = new System.Drawing.Size(100, 100);
            this.imageDebugger.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.imageDebugger.TabIndex = 0;
            this.imageDebugger.TabStop = false;
            // 
            // DebugMenu
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.ClientSize = new System.Drawing.Size(538, 490);
            this.Controls.Add(this.tabControl1);
            this.Name = "DebugMenu";
            this.Text = "DebugMenu";
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.Chat)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.letterOutput)).EndInit();
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            this.tabControl1.ResumeLayout(false);
            this.commandsTab.ResumeLayout(false);
            this.imageDebugTab.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.imageDebuggerContainer.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.imageDebugger)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.PictureBox Chat;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label CharacterText;
        private System.Windows.Forms.TextBox characterInput;
        private System.Windows.Forms.Label cursorLocation;
        private System.Windows.Forms.Label processError;
        private System.Windows.Forms.Button processButton;
        private System.Windows.Forms.Button setlineButton;
        private System.Windows.Forms.Button updateButton;
        private System.Windows.Forms.Panel panel2;
        private PictureBoxExtended letterOutput;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Button copyButton;
        private System.Windows.Forms.TextBox output;
        private System.Windows.Forms.RichTextBox richTextBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage commandsTab;
        private System.Windows.Forms.TabPage imageDebugTab;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private PictureBoxExtended imageDebugger;
        private System.Windows.Forms.Panel imageDebuggerContainer;
    }
}
#endif