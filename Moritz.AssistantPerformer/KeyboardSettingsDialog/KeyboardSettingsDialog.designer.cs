namespace Moritz.AssistantPerformer
{
    partial class KeyboardSettingsDialog
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
            if(disposing && (components != null))
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
            this.DeadKeysButton = new System.Windows.Forms.Button();
            this.Solo_AssistantHearsFirstButton = new System.Windows.Forms.Button();
            this.AssistedKeysButton = new System.Windows.Forms.Button();
            this.NewKeyboardButton = new System.Windows.Forms.Button();
            this.DeleteKeyboardButton = new System.Windows.Forms.Button();
            this.OKButton = new System.Windows.Forms.Button();
            this.HelpLabel = new System.Windows.Forms.Label();
            this.UsageGroupBox = new System.Windows.Forms.GroupBox();
            this.InvisibleTextBox = new System.Windows.Forms.TextBox();
            this.CancelSettingsButton = new System.Windows.Forms.Button();
            this.MidiWidthButton = new System.Windows.Forms.Button();
            this.PianoWidthButton = new System.Windows.Forms.Button();
            this.Panel = new System.Windows.Forms.Panel();
            this.VScrollBar = new System.Windows.Forms.VScrollBar();
            this.Solo_AssistantHearsNothingButton = new System.Windows.Forms.Button();
            this.UsageGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // DeadKeysButton
            // 
            this.DeadKeysButton.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.DeadKeysButton.BackColor = System.Drawing.Color.FromArgb(((int) (((byte) (255)))), ((int) (((byte) (210)))), ((int) (((byte) (210)))));
            this.DeadKeysButton.FlatAppearance.BorderColor = System.Drawing.Color.Black;
            this.DeadKeysButton.Location = new System.Drawing.Point(807, 396);
            this.DeadKeysButton.Name = "DeadKeysButton";
            this.DeadKeysButton.Size = new System.Drawing.Size(133, 28);
            this.DeadKeysButton.TabIndex = 11;
            this.DeadKeysButton.Text = "Dead key";
            this.DeadKeysButton.UseVisualStyleBackColor = false;
            this.DeadKeysButton.Click += new System.EventHandler(this.DeadKeysButton_Click);
            // 
            // Solo_AssistantHearsFirstButton
            // 
            this.Solo_AssistantHearsFirstButton.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.Solo_AssistantHearsFirstButton.BackColor = System.Drawing.Color.FromArgb(((int) (((byte) (210)))), ((int) (((byte) (210)))), ((int) (((byte) (255)))));
            this.Solo_AssistantHearsFirstButton.FlatAppearance.BorderColor = System.Drawing.Color.Black;
            this.Solo_AssistantHearsFirstButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte) (0)));
            this.Solo_AssistantHearsFirstButton.Location = new System.Drawing.Point(646, 396);
            this.Solo_AssistantHearsFirstButton.Name = "Solo_AssistantHearsFirstButton";
            this.Solo_AssistantHearsFirstButton.Size = new System.Drawing.Size(153, 28);
            this.Solo_AssistantHearsFirstButton.TabIndex = 10;
            this.Solo_AssistantHearsFirstButton.Text = "Solo - first note assisted";
            this.Solo_AssistantHearsFirstButton.UseVisualStyleBackColor = false;
            this.Solo_AssistantHearsFirstButton.Click += new System.EventHandler(this.Solo_AssistantHearsFirstButton_Click);
            // 
            // AssistedKeysButton
            // 
            this.AssistedKeysButton.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.AssistedKeysButton.BackColor = System.Drawing.Color.Gainsboro;
            this.AssistedKeysButton.FlatAppearance.BorderColor = System.Drawing.Color.Black;
            this.AssistedKeysButton.Location = new System.Drawing.Point(344, 396);
            this.AssistedKeysButton.Name = "AssistedKeysButton";
            this.AssistedKeysButton.Size = new System.Drawing.Size(133, 28);
            this.AssistedKeysButton.TabIndex = 9;
            this.AssistedKeysButton.Text = "Assisted";
            this.AssistedKeysButton.UseVisualStyleBackColor = false;
            this.AssistedKeysButton.Click += new System.EventHandler(this.AssistedKeysButton_Click);
            // 
            // NewKeyboardButton
            // 
            this.NewKeyboardButton.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.NewKeyboardButton.Location = new System.Drawing.Point(538, 430);
            this.NewKeyboardButton.Name = "NewKeyboardButton";
            this.NewKeyboardButton.Size = new System.Drawing.Size(100, 28);
            this.NewKeyboardButton.TabIndex = 16;
            this.NewKeyboardButton.Text = "New keyboard";
            this.NewKeyboardButton.UseVisualStyleBackColor = true;
            this.NewKeyboardButton.Click += new System.EventHandler(this.NewKeyboardButton_Click);
            // 
            // DeleteKeyboardButton
            // 
            this.DeleteKeyboardButton.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.DeleteKeyboardButton.Enabled = false;
            this.DeleteKeyboardButton.Location = new System.Drawing.Point(646, 430);
            this.DeleteKeyboardButton.Name = "DeleteKeyboardButton";
            this.DeleteKeyboardButton.Size = new System.Drawing.Size(100, 28);
            this.DeleteKeyboardButton.TabIndex = 17;
            this.DeleteKeyboardButton.Text = "Delete keyboard";
            this.DeleteKeyboardButton.UseVisualStyleBackColor = true;
            this.DeleteKeyboardButton.Click += new System.EventHandler(this.DeleteKeyboardButton_Click);
            // 
            // OKButton
            // 
            this.OKButton.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.OKButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.OKButton.Location = new System.Drawing.Point(786, 430);
            this.OKButton.Name = "OKButton";
            this.OKButton.Size = new System.Drawing.Size(73, 28);
            this.OKButton.TabIndex = 18;
            this.OKButton.Text = "OK";
            this.OKButton.UseVisualStyleBackColor = true;
            this.OKButton.Click += new System.EventHandler(this.OKButton_Click);
            // 
            // HelpLabel
            // 
            this.HelpLabel.AutoSize = true;
            this.HelpLabel.BackColor = System.Drawing.Color.FromArgb(((int) (((byte) (248)))), ((int) (((byte) (248)))), ((int) (((byte) (248)))));
            this.HelpLabel.ForeColor = System.Drawing.Color.RoyalBlue;
            this.HelpLabel.Location = new System.Drawing.Point(5, 18);
            this.HelpLabel.Name = "HelpLabel";
            this.HelpLabel.Size = new System.Drawing.Size(276, 39);
            this.HelpLabel.TabIndex = 19;
            this.HelpLabel.Text = "<left-click> selects a single key or keyboard,\r\n<shift><left-click> selects addit" +
                "ional keys or keyboards,\r\n<shift><right-click> selects a region of keys or keybo" +
                "ards.";
            // 
            // UsageGroupBox
            // 
            this.UsageGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.UsageGroupBox.Controls.Add(this.HelpLabel);
            this.UsageGroupBox.Location = new System.Drawing.Point(40, 391);
            this.UsageGroupBox.Name = "UsageGroupBox";
            this.UsageGroupBox.Size = new System.Drawing.Size(285, 66);
            this.UsageGroupBox.TabIndex = 22;
            this.UsageGroupBox.TabStop = false;
            this.UsageGroupBox.Text = "Usage";
            // 
            // InvisibleTextBox
            // 
            this.InvisibleTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 5.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte) (0)));
            this.InvisibleTextBox.Location = new System.Drawing.Point(-20, -20);
            this.InvisibleTextBox.Name = "InvisibleTextBox";
            this.InvisibleTextBox.Size = new System.Drawing.Size(10, 15);
            this.InvisibleTextBox.TabIndex = 25;
            // 
            // CancelSettingsButton
            // 
            this.CancelSettingsButton.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.CancelSettingsButton.Location = new System.Drawing.Point(867, 430);
            this.CancelSettingsButton.Name = "CancelSettingsButton";
            this.CancelSettingsButton.Size = new System.Drawing.Size(73, 28);
            this.CancelSettingsButton.TabIndex = 23;
            this.CancelSettingsButton.Text = "Cancel";
            this.CancelSettingsButton.UseVisualStyleBackColor = true;
            this.CancelSettingsButton.Click += new System.EventHandler(this.CancelSettingsButton_Click);
            // 
            // MidiWidthButton
            // 
            this.MidiWidthButton.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.MidiWidthButton.Location = new System.Drawing.Point(425, 430);
            this.MidiWidthButton.Name = "MidiWidthButton";
            this.MidiWidthButton.Size = new System.Drawing.Size(73, 28);
            this.MidiWidthButton.TabIndex = 24;
            this.MidiWidthButton.Text = "Midi width";
            this.MidiWidthButton.UseVisualStyleBackColor = true;
            this.MidiWidthButton.Click += new System.EventHandler(this.MidiFrameButton_Click);
            // 
            // PianoWidthButton
            // 
            this.PianoWidthButton.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.PianoWidthButton.Location = new System.Drawing.Point(344, 430);
            this.PianoWidthButton.Name = "PianoWidthButton";
            this.PianoWidthButton.Size = new System.Drawing.Size(73, 28);
            this.PianoWidthButton.TabIndex = 25;
            this.PianoWidthButton.Text = "Piano width";
            this.PianoWidthButton.UseVisualStyleBackColor = true;
            this.PianoWidthButton.Click += new System.EventHandler(this.PianoFrameButton_Click);
            // 
            // Panel
            // 
            this.Panel.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.Panel.BackColor = System.Drawing.Color.White;
            this.Panel.Location = new System.Drawing.Point(12, 12);
            this.Panel.Name = "Panel";
            this.Panel.Size = new System.Drawing.Size(914, 377);
            this.Panel.TabIndex = 26;
            this.Panel.Click += new System.EventHandler(this.Panel_Click);
            // 
            // VScrollBar
            // 
            this.VScrollBar.Location = new System.Drawing.Point(927, 12);
            this.VScrollBar.Name = "VScrollBar";
            this.VScrollBar.Size = new System.Drawing.Size(17, 377);
            this.VScrollBar.TabIndex = 27;
            this.VScrollBar.Scroll += new System.Windows.Forms.ScrollEventHandler(this.VScrollBar_Scroll);
            // 
            // Solo_AssistantHearsNothingButton
            // 
            this.Solo_AssistantHearsNothingButton.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.Solo_AssistantHearsNothingButton.BackColor = System.Drawing.Color.FromArgb(((int) (((byte) (210)))), ((int) (((byte) (255)))), ((int) (((byte) (210)))));
            this.Solo_AssistantHearsNothingButton.FlatAppearance.BorderColor = System.Drawing.Color.Black;
            this.Solo_AssistantHearsNothingButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte) (0)));
            this.Solo_AssistantHearsNothingButton.Location = new System.Drawing.Point(485, 396);
            this.Solo_AssistantHearsNothingButton.Name = "Solo_AssistantHearsNothingButton";
            this.Solo_AssistantHearsNothingButton.Size = new System.Drawing.Size(153, 28);
            this.Solo_AssistantHearsNothingButton.TabIndex = 28;
            this.Solo_AssistantHearsNothingButton.Text = "Solo - assistant hears nothing";
            this.Solo_AssistantHearsNothingButton.UseVisualStyleBackColor = false;
            this.Solo_AssistantHearsNothingButton.Click += new System.EventHandler(this.Solo_AssistantHearsNothingButton_Click);
            // 
            // KeyboardSettingsDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int) (((byte) (248)))), ((int) (((byte) (248)))), ((int) (((byte) (248)))));
            this.ClientSize = new System.Drawing.Size(953, 469);
            this.Controls.Add(this.Solo_AssistantHearsFirstButton);
            this.Controls.Add(this.Solo_AssistantHearsNothingButton);
            this.Controls.Add(this.VScrollBar);
            this.Controls.Add(this.Panel);
            this.Controls.Add(this.PianoWidthButton);
            this.Controls.Add(this.MidiWidthButton);
            this.Controls.Add(this.CancelSettingsButton);
            this.Controls.Add(this.UsageGroupBox);
            this.Controls.Add(this.OKButton);
            this.Controls.Add(this.DeleteKeyboardButton);
            this.Controls.Add(this.NewKeyboardButton);
            this.Controls.Add(this.DeadKeysButton);
            this.Controls.Add(this.AssistedKeysButton);
            this.Controls.Add(this.InvisibleTextBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "KeyboardSettingsDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Performer\'s Keyboard Settings";
            this.MouseClick += new System.Windows.Forms.MouseEventHandler(this.KeyboardSettingsDialog_MouseClick);
            this.UsageGroupBox.ResumeLayout(false);
            this.UsageGroupBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button DeadKeysButton;
        private System.Windows.Forms.Button Solo_AssistantHearsFirstButton;
        private System.Windows.Forms.Button AssistedKeysButton;
        private System.Windows.Forms.Button NewKeyboardButton;
        private System.Windows.Forms.Button DeleteKeyboardButton;
        private System.Windows.Forms.Button OKButton;
        private System.Windows.Forms.Label HelpLabel;
        private System.Windows.Forms.GroupBox UsageGroupBox;
        private System.Windows.Forms.Button CancelSettingsButton;
        private System.Windows.Forms.Button MidiWidthButton;
        public System.Windows.Forms.TextBox InvisibleTextBox;
        private System.Windows.Forms.Button PianoWidthButton;
        private System.Windows.Forms.Panel Panel;
        private System.Windows.Forms.VScrollBar VScrollBar;
        private System.Windows.Forms.Button Solo_AssistantHearsNothingButton;

    }
}

