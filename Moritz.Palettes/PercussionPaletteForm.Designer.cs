namespace Moritz.Palettes
{
	partial class PercussionPaletteForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private global::System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if(disposing)
			{
				if(components != null)
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PercussionPaletteForm));
            this.DeleteOrnamentSettingsButton = new System.Windows.Forms.Button();
            this.NewOrnamentSettingsButton = new System.Windows.Forms.Button();
            this.OrnamentNumbersLabel = new System.Windows.Forms.Label();
            this.OrnamentNumbersTextBox = new System.Windows.Forms.TextBox();
            this.OrnamentNumbersHelpLabel = new System.Windows.Forms.Label();
            this.ShowOrnamentSettingsButton = new System.Windows.Forms.Button();
            this.PanEnvelopesLabel = new System.Windows.Forms.Label();
            this.PanEnvelopesTextBox = new System.Windows.Forms.TextBox();
            this.PanEnvelopesHelpLabel = new System.Windows.Forms.Label();
            this.ExpressionEnvelopesTextBox = new System.Windows.Forms.TextBox();
            this.VelocityEnvelopesLabel = new System.Windows.Forms.Label();
            this.VelocityEnvelopesHelpLabel = new System.Windows.Forms.Label();
            this.HelpTextLabel = new System.Windows.Forms.Label();
            this.MidiInstrumentsHelpButton = new System.Windows.Forms.Button();
            this.MinMsDurationsHelpLabel = new System.Windows.Forms.Label();
            this.MinMsDurationsLabel = new System.Windows.Forms.Label();
            this.MinMsDurationsTextBox = new System.Windows.Forms.TextBox();
            this.EnvelopesExtraHelpLabel = new System.Windows.Forms.Label();
            this.ShowMainScoreFormButton = new System.Windows.Forms.Button();
            this.midiLabel = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.AudioHelpLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // DeleteOrnamentSettingsButton
            // 
            this.DeleteOrnamentSettingsButton.Location = new System.Drawing.Point(531, 429);
            this.DeleteOrnamentSettingsButton.Name = "DeleteOrnamentSettingsButton";
            this.DeleteOrnamentSettingsButton.Size = new System.Drawing.Size(137, 31);
            this.DeleteOrnamentSettingsButton.TabIndex = 14;
            this.DeleteOrnamentSettingsButton.Text = "delete ornament settings";
            this.DeleteOrnamentSettingsButton.UseVisualStyleBackColor = true;
            this.DeleteOrnamentSettingsButton.Click += new System.EventHandler(this.DeleteOrnamentSettingsButton_Click);
            // 
            // NewOrnamentSettingsButton
            // 
            this.NewOrnamentSettingsButton.Location = new System.Drawing.Point(531, 392);
            this.NewOrnamentSettingsButton.Name = "NewOrnamentSettingsButton";
            this.NewOrnamentSettingsButton.Size = new System.Drawing.Size(137, 31);
            this.NewOrnamentSettingsButton.TabIndex = 13;
            this.NewOrnamentSettingsButton.Text = "new ornament settings";
            this.NewOrnamentSettingsButton.UseVisualStyleBackColor = true;
            this.NewOrnamentSettingsButton.Click += new System.EventHandler(this.NewOrnamentSettingsButton_Click);
            // 
            // OrnamentNumbersLabel
            // 
            this.OrnamentNumbersLabel.AutoSize = true;
            this.OrnamentNumbersLabel.ForeColor = System.Drawing.Color.Brown;
            this.OrnamentNumbersLabel.Location = new System.Drawing.Point(77, 286);
            this.OrnamentNumbersLabel.Name = "OrnamentNumbersLabel";
            this.OrnamentNumbersLabel.Size = new System.Drawing.Size(97, 14);
            this.OrnamentNumbersLabel.TabIndex = 36;
            this.OrnamentNumbersLabel.Text = "ornament numbers";
            // 
            // OrnamentNumbersTextBox
            // 
            this.OrnamentNumbersTextBox.Location = new System.Drawing.Point(173, 283);
            this.OrnamentNumbersTextBox.Name = "OrnamentNumbersTextBox";
            this.OrnamentNumbersTextBox.Size = new System.Drawing.Size(454, 20);
            this.OrnamentNumbersTextBox.TabIndex = 6;
            this.OrnamentNumbersTextBox.TextChanged += new System.EventHandler(this.OrnamentNumbersTextBox_TextChanged);
            this.OrnamentNumbersTextBox.Leave += new System.EventHandler(this.OrnamentNumbersTextBox_Leave);
            // 
            // OrnamentNumbersHelpLabel
            // 
            this.OrnamentNumbersHelpLabel.AutoSize = true;
            this.OrnamentNumbersHelpLabel.ForeColor = System.Drawing.Color.RoyalBlue;
            this.OrnamentNumbersHelpLabel.Location = new System.Drawing.Point(630, 279);
            this.OrnamentNumbersHelpLabel.Name = "OrnamentNumbersHelpLabel";
            this.OrnamentNumbersHelpLabel.Size = new System.Drawing.Size(171, 28);
            this.OrnamentNumbersHelpLabel.TabIndex = 57;
            this.OrnamentNumbersHelpLabel.Text = "7 integer values in range [ 0..128 ]\r\n(0 means no ornament)";
            // 
            // ShowOrnamentSettingsButton
            // 
            this.ShowOrnamentSettingsButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(215)))), ((int)(((byte)(225)))), ((int)(((byte)(215)))));
            this.ShowOrnamentSettingsButton.Location = new System.Drawing.Point(677, 392);
            this.ShowOrnamentSettingsButton.Name = "ShowOrnamentSettingsButton";
            this.ShowOrnamentSettingsButton.Size = new System.Drawing.Size(137, 31);
            this.ShowOrnamentSettingsButton.TabIndex = 11;
            this.ShowOrnamentSettingsButton.Text = "show ornament settings";
            this.ShowOrnamentSettingsButton.UseVisualStyleBackColor = false;
            this.ShowOrnamentSettingsButton.Click += new System.EventHandler(this.ShowOrnamentSettingsButton_Click);
            // 
            // PanEnvelopesLabel
            // 
            this.PanEnvelopesLabel.AutoSize = true;
            this.PanEnvelopesLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(60)))), ((int)(((byte)(60)))));
            this.PanEnvelopesLabel.Location = new System.Drawing.Point(62, 249);
            this.PanEnvelopesLabel.Name = "PanEnvelopesLabel";
            this.PanEnvelopesLabel.Size = new System.Drawing.Size(112, 14);
            this.PanEnvelopesLabel.TabIndex = 35;
            this.PanEnvelopesLabel.Text = "( pan envelopes (64))";
            // 
            // PanEnvelopesTextBox
            // 
            this.PanEnvelopesTextBox.Location = new System.Drawing.Point(173, 246);
            this.PanEnvelopesTextBox.Name = "PanEnvelopesTextBox";
            this.PanEnvelopesTextBox.Size = new System.Drawing.Size(554, 20);
            this.PanEnvelopesTextBox.TabIndex = 3;
            this.PanEnvelopesTextBox.TextChanged += new System.EventHandler(this.PanEnvelopesTextBox_TextChanged);
            this.PanEnvelopesTextBox.Leave += new System.EventHandler(this.PanEnvelopesTextBox_Leave);
            // 
            // PanEnvelopesHelpLabel
            // 
            this.PanEnvelopesHelpLabel.AutoSize = true;
            this.PanEnvelopesHelpLabel.Enabled = false;
            this.PanEnvelopesHelpLabel.ForeColor = System.Drawing.Color.RoyalBlue;
            this.PanEnvelopesHelpLabel.Location = new System.Drawing.Point(728, 249);
            this.PanEnvelopesHelpLabel.Name = "PanEnvelopesHelpLabel";
            this.PanEnvelopesHelpLabel.Size = new System.Drawing.Size(70, 14);
            this.PanEnvelopesHelpLabel.TabIndex = 56;
            this.PanEnvelopesHelpLabel.Text = "7 envelopes*";
            // 
            // ExpressionEnvelopesTextBox
            // 
            this.ExpressionEnvelopesTextBox.Location = new System.Drawing.Point(173, 172);
            this.ExpressionEnvelopesTextBox.Name = "ExpressionEnvelopesTextBox";
            this.ExpressionEnvelopesTextBox.Size = new System.Drawing.Size(554, 20);
            this.ExpressionEnvelopesTextBox.TabIndex = 5;
            this.ExpressionEnvelopesTextBox.TextChanged += new System.EventHandler(this.ExpressionEnvelopesTextBox_TextChanged);
            this.ExpressionEnvelopesTextBox.Leave += new System.EventHandler(this.ExpressionEnvelopesTextBox_Leave);
            // 
            // VelocityEnvelopesLabel
            // 
            this.VelocityEnvelopesLabel.AutoSize = true;
            this.VelocityEnvelopesLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(60)))), ((int)(((byte)(60)))));
            this.VelocityEnvelopesLabel.Location = new System.Drawing.Point(20, 175);
            this.VelocityEnvelopesLabel.Name = "VelocityEnvelopesLabel";
            this.VelocityEnvelopesLabel.Size = new System.Drawing.Size(154, 14);
            this.VelocityEnvelopesLabel.TabIndex = 63;
            this.VelocityEnvelopesLabel.Text = "( expression envelopes (127))";
            // 
            // VelocityEnvelopesHelpLabel
            // 
            this.VelocityEnvelopesHelpLabel.AutoSize = true;
            this.VelocityEnvelopesHelpLabel.Enabled = false;
            this.VelocityEnvelopesHelpLabel.ForeColor = System.Drawing.Color.RoyalBlue;
            this.VelocityEnvelopesHelpLabel.Location = new System.Drawing.Point(728, 175);
            this.VelocityEnvelopesHelpLabel.Name = "VelocityEnvelopesHelpLabel";
            this.VelocityEnvelopesHelpLabel.Size = new System.Drawing.Size(70, 14);
            this.VelocityEnvelopesHelpLabel.TabIndex = 65;
            this.VelocityEnvelopesHelpLabel.Text = "7 envelopes*";
            // 
            // HelpTextLabel
            // 
            this.HelpTextLabel.AutoSize = true;
            this.HelpTextLabel.ForeColor = System.Drawing.Color.RoyalBlue;
            this.HelpTextLabel.Location = new System.Drawing.Point(170, 390);
            this.HelpTextLabel.Name = "HelpTextLabel";
            this.HelpTextLabel.Size = new System.Drawing.Size(352, 70);
            this.HelpTextLabel.TabIndex = 94;
            this.HelpTextLabel.Text = resources.GetString("HelpTextLabel.Text");
            // 
            // MidiInstrumentsHelpButton
            // 
            this.MidiInstrumentsHelpButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(215)))), ((int)(((byte)(225)))), ((int)(((byte)(215)))));
            this.MidiInstrumentsHelpButton.Location = new System.Drawing.Point(30, 336);
            this.MidiInstrumentsHelpButton.Name = "MidiInstrumentsHelpButton";
            this.MidiInstrumentsHelpButton.Size = new System.Drawing.Size(95, 24);
            this.MidiInstrumentsHelpButton.TabIndex = 16;
            this.MidiInstrumentsHelpButton.Text = "MIDI Percussion";
            this.MidiInstrumentsHelpButton.UseVisualStyleBackColor = false;
            this.MidiInstrumentsHelpButton.Click += new System.EventHandler(this.MidiPercussionHelpButton_Click);
            // 
            // MinMsDurationsHelpLabel
            // 
            this.MinMsDurationsHelpLabel.AutoSize = true;
            this.MinMsDurationsHelpLabel.ForeColor = System.Drawing.Color.RoyalBlue;
            this.MinMsDurationsHelpLabel.Location = new System.Drawing.Point(630, 312);
            this.MinMsDurationsHelpLabel.Name = "MinMsDurationsHelpLabel";
            this.MinMsDurationsHelpLabel.Size = new System.Drawing.Size(122, 14);
            this.MinMsDurationsHelpLabel.TabIndex = 100;
            this.MinMsDurationsHelpLabel.Text = "7 integer values ( >= 0 )";
            // 
            // MinMsDurationsLabel
            // 
            this.MinMsDurationsLabel.AutoSize = true;
            this.MinMsDurationsLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(60)))), ((int)(((byte)(60)))));
            this.MinMsDurationsLabel.Location = new System.Drawing.Point(20, 312);
            this.MinMsDurationsLabel.Name = "MinMsDurationsLabel";
            this.MinMsDurationsLabel.Size = new System.Drawing.Size(154, 14);
            this.MinMsDurationsLabel.TabIndex = 101;
            this.MinMsDurationsLabel.Text = "( minimum chord durations (1))";
            // 
            // MinMsDurationsTextBox
            // 
            this.MinMsDurationsTextBox.Location = new System.Drawing.Point(173, 309);
            this.MinMsDurationsTextBox.Name = "MinMsDurationsTextBox";
            this.MinMsDurationsTextBox.Size = new System.Drawing.Size(454, 20);
            this.MinMsDurationsTextBox.TabIndex = 7;
            this.MinMsDurationsTextBox.TextChanged += new System.EventHandler(this.MinMsDurationsTextBox_TextChanged);
            this.MinMsDurationsTextBox.Leave += new System.EventHandler(this.MinMsDurationsTextBox_Leave);
            // 
            // EnvelopesExtraHelpLabel
            // 
            this.EnvelopesExtraHelpLabel.AutoSize = true;
            this.EnvelopesExtraHelpLabel.ForeColor = System.Drawing.Color.RoyalBlue;
            this.EnvelopesExtraHelpLabel.Location = new System.Drawing.Point(170, 193);
            this.EnvelopesExtraHelpLabel.Name = "EnvelopesExtraHelpLabel";
            this.EnvelopesExtraHelpLabel.Size = new System.Drawing.Size(640, 42);
            this.EnvelopesExtraHelpLabel.TabIndex = 102;
            this.EnvelopesExtraHelpLabel.Text = resources.GetString("EnvelopesExtraHelpLabel.Text");
            // 
            // ShowMainScoreFormButton
            // 
            this.ShowMainScoreFormButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(215)))), ((int)(((byte)(225)))), ((int)(((byte)(215)))));
            this.ShowMainScoreFormButton.Location = new System.Drawing.Point(677, 429);
            this.ShowMainScoreFormButton.Name = "ShowMainScoreFormButton";
            this.ShowMainScoreFormButton.Size = new System.Drawing.Size(137, 31);
            this.ShowMainScoreFormButton.TabIndex = 103;
            this.ShowMainScoreFormButton.Text = "show main score form";
            this.ShowMainScoreFormButton.UseVisualStyleBackColor = false;
            this.ShowMainScoreFormButton.Click += new System.EventHandler(this.ShowMainScoreFormButton_Click);
            // 
            // midiLabel
            // 
            this.midiLabel.AutoSize = true;
            this.midiLabel.ForeColor = System.Drawing.Color.RoyalBlue;
            this.midiLabel.Location = new System.Drawing.Point(146, 341);
            this.midiLabel.Name = "midiLabel";
            this.midiLabel.Size = new System.Drawing.Size(25, 14);
            this.midiLabel.TabIndex = 108;
            this.midiLabel.Text = "midi";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.ForeColor = System.Drawing.Color.RoyalBlue;
            this.label1.Location = new System.Drawing.Point(138, 366);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(33, 14);
            this.label1.TabIndex = 109;
            this.label1.Text = "audio";
            // 
            // AudioHelpLabel
            // 
            this.AudioHelpLabel.AutoSize = true;
            this.AudioHelpLabel.ForeColor = System.Drawing.Color.RoyalBlue;
            this.AudioHelpLabel.Location = new System.Drawing.Point(20, 390);
            this.AudioHelpLabel.Name = "AudioHelpLabel";
            this.AudioHelpLabel.Size = new System.Drawing.Size(133, 56);
            this.AudioHelpLabel.TabIndex = 110;
            this.AudioHelpLabel.Text = "audio buttons:\r\n  left click: load, start, stop\r\n  right click: reload\r\n  middle " +
    "click: file info";
            // 
            // PercussionPaletteForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 14F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(255)))), ((int)(((byte)(245)))));
            this.ClientSize = new System.Drawing.Size(836, 478);
            this.ControlBox = false;
            this.Controls.Add(this.AudioHelpLabel);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.midiLabel);
            this.Controls.Add(this.ShowMainScoreFormButton);
            this.Controls.Add(this.EnvelopesExtraHelpLabel);
            this.Controls.Add(this.MinMsDurationsTextBox);
            this.Controls.Add(this.MinMsDurationsHelpLabel);
            this.Controls.Add(this.MidiInstrumentsHelpButton);
            this.Controls.Add(this.HelpTextLabel);
            this.Controls.Add(this.ExpressionEnvelopesTextBox);
            this.Controls.Add(this.DeleteOrnamentSettingsButton);
            this.Controls.Add(this.NewOrnamentSettingsButton);
            this.Controls.Add(this.OrnamentNumbersHelpLabel);
            this.Controls.Add(this.OrnamentNumbersTextBox);
            this.Controls.Add(this.ShowOrnamentSettingsButton);
            this.Controls.Add(this.PanEnvelopesTextBox);
            this.Controls.Add(this.VelocityEnvelopesHelpLabel);
            this.Controls.Add(this.PanEnvelopesHelpLabel);
            this.Controls.Add(this.MinMsDurationsLabel);
            this.Controls.Add(this.VelocityEnvelopesLabel);
            this.Controls.Add(this.OrnamentNumbersLabel);
            this.Controls.Add(this.PanEnvelopesLabel);
            this.Font = new System.Drawing.Font("Arial", 8F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Location = new System.Drawing.Point(250, 100);
            this.Name = "PercussionPaletteForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "krystal palette";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.PercussionPaletteForm_FormClosing);
            this.Click += new System.EventHandler(this.PercussionPaletteForm_Click);
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

        private System.Windows.Forms.Button DeleteOrnamentSettingsButton;
        private System.Windows.Forms.Button NewOrnamentSettingsButton;
        private System.Windows.Forms.Label OrnamentNumbersLabel;
        private System.Windows.Forms.Label OrnamentNumbersHelpLabel;
        private System.Windows.Forms.Button ShowOrnamentSettingsButton;
        private System.Windows.Forms.Label PanEnvelopesLabel;
        private System.Windows.Forms.Label PanEnvelopesHelpLabel;
        private System.Windows.Forms.Label VelocityEnvelopesLabel;
        private System.Windows.Forms.Label VelocityEnvelopesHelpLabel;
        private System.Windows.Forms.Label HelpTextLabel;
        private System.Windows.Forms.Button MidiInstrumentsHelpButton;
        private System.Windows.Forms.Label MinMsDurationsHelpLabel;
        private System.Windows.Forms.Label MinMsDurationsLabel;
        public System.Windows.Forms.TextBox OrnamentNumbersTextBox;
        public System.Windows.Forms.TextBox PanEnvelopesTextBox;
        public System.Windows.Forms.TextBox ExpressionEnvelopesTextBox;
        public System.Windows.Forms.TextBox MinMsDurationsTextBox;
        private System.Windows.Forms.Label EnvelopesExtraHelpLabel;
        private System.Windows.Forms.Button ShowMainScoreFormButton;
        private System.Windows.Forms.Label midiLabel;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label AudioHelpLabel;


    }
}

