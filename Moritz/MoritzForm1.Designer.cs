namespace Moritz
{
	partial class MoritzForm1
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
            this.PerformScoreButton = new System.Windows.Forms.Button();
            this.QuitButton = new System.Windows.Forms.Button();
            this.PreferencesButton = new System.Windows.Forms.Button();
            this.CurrentOutputDeviceLabel = new System.Windows.Forms.Label();
            this.CurrentInputDeviceLabel = new System.Windows.Forms.Label();
            this.OutputDeviceComboBox = new System.Windows.Forms.ComboBox();
            this.InputDeviceComboBox = new System.Windows.Forms.ComboBox();
            this.CloseAssistantPerformerButton = new System.Windows.Forms.Button();
            this.NewScoreSettingsButton = new System.Windows.Forms.Button();
            this.LoadScoreSettingsButton = new System.Windows.Forms.Button();
            this.CloseAssistantComposerButton = new System.Windows.Forms.Button();
            this.KrystalsEditorButton = new System.Windows.Forms.Button();
            this.AboutButton = new System.Windows.Forms.Button();
            this.AssistantComposerPanel = new System.Windows.Forms.Panel();
            this.AssistantComposerLabel = new System.Windows.Forms.Label();
            this.AssistantPerformerPanel = new System.Windows.Forms.Panel();
            this.AssistantPerformerLabel = new System.Windows.Forms.Label();
            this.KrystalsEditorPanel = new System.Windows.Forms.Panel();
            this.KrystalsEditorLabel = new System.Windows.Forms.Label();
            this.MoritzPanel = new System.Windows.Forms.Panel();
            this.MoritzLabel = new System.Windows.Forms.Label();
            this.AssistantComposerPanel.SuspendLayout();
            this.AssistantPerformerPanel.SuspendLayout();
            this.KrystalsEditorPanel.SuspendLayout();
            this.MoritzPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // PerformScoreButton
            // 
            this.PerformScoreButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(64)))), ((int)(((byte)(0)))));
            this.PerformScoreButton.Location = new System.Drawing.Point(16, 30);
            this.PerformScoreButton.Name = "PerformScoreButton";
            this.PerformScoreButton.Size = new System.Drawing.Size(204, 31);
            this.PerformScoreButton.TabIndex = 0;
            this.PerformScoreButton.Text = "perform .html or .svg score";
            this.PerformScoreButton.UseVisualStyleBackColor = true;
            this.PerformScoreButton.Click += new System.EventHandler(this.PerformScoreButton_Click);
            // 
            // QuitButton
            // 
            this.QuitButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(64)))), ((int)(((byte)(0)))));
            this.QuitButton.Location = new System.Drawing.Point(224, 30);
            this.QuitButton.Name = "QuitButton";
            this.QuitButton.Size = new System.Drawing.Size(98, 31);
            this.QuitButton.TabIndex = 2;
            this.QuitButton.Text = "quit";
            this.QuitButton.UseVisualStyleBackColor = true;
            this.QuitButton.Click += new System.EventHandler(this.QuitButton_Click);
            // 
            // PreferencesButton
            // 
            this.PreferencesButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(64)))), ((int)(((byte)(0)))));
            this.PreferencesButton.Location = new System.Drawing.Point(14, 30);
            this.PreferencesButton.Name = "PreferencesButton";
            this.PreferencesButton.Size = new System.Drawing.Size(98, 31);
            this.PreferencesButton.TabIndex = 3;
            this.PreferencesButton.Text = "preferences";
            this.PreferencesButton.UseVisualStyleBackColor = true;
            this.PreferencesButton.Click += new System.EventHandler(this.PreferencesButton_Click);
            // 
            // CurrentOutputDeviceLabel
            // 
            this.CurrentOutputDeviceLabel.AutoSize = true;
            this.CurrentOutputDeviceLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(64)))), ((int)(((byte)(0)))));
            this.CurrentOutputDeviceLabel.Location = new System.Drawing.Point(278, 11);
            this.CurrentOutputDeviceLabel.Name = "CurrentOutputDeviceLabel";
            this.CurrentOutputDeviceLabel.Size = new System.Drawing.Size(116, 13);
            this.CurrentOutputDeviceLabel.TabIndex = 45;
            this.CurrentOutputDeviceLabel.Text = "Current Output Device:";
            // 
            // CurrentInputDeviceLabel
            // 
            this.CurrentInputDeviceLabel.AutoSize = true;
            this.CurrentInputDeviceLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(64)))), ((int)(((byte)(0)))));
            this.CurrentInputDeviceLabel.Location = new System.Drawing.Point(23, 11);
            this.CurrentInputDeviceLabel.Name = "CurrentInputDeviceLabel";
            this.CurrentInputDeviceLabel.Size = new System.Drawing.Size(108, 13);
            this.CurrentInputDeviceLabel.TabIndex = 42;
            this.CurrentInputDeviceLabel.Text = "Current Input Device:";
            // 
            // OutputDeviceComboBox
            // 
            this.OutputDeviceComboBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(64)))), ((int)(((byte)(0)))));
            this.OutputDeviceComboBox.FormattingEnabled = true;
            this.OutputDeviceComboBox.Location = new System.Drawing.Point(278, 27);
            this.OutputDeviceComboBox.Name = "OutputDeviceComboBox";
            this.OutputDeviceComboBox.Size = new System.Drawing.Size(237, 21);
            this.OutputDeviceComboBox.TabIndex = 44;
            this.OutputDeviceComboBox.SelectedIndexChanged += new System.EventHandler(this.MidiOutputDevicesComboBox_SelectedIndexChanged);
            // 
            // InputDeviceComboBox
            // 
            this.InputDeviceComboBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(64)))), ((int)(((byte)(0)))));
            this.InputDeviceComboBox.FormattingEnabled = true;
            this.InputDeviceComboBox.Location = new System.Drawing.Point(23, 27);
            this.InputDeviceComboBox.Name = "InputDeviceComboBox";
            this.InputDeviceComboBox.Size = new System.Drawing.Size(237, 21);
            this.InputDeviceComboBox.TabIndex = 43;
            this.InputDeviceComboBox.SelectedIndexChanged += new System.EventHandler(this.MidiInputDevicesComboBox_SelectedIndexChanged);
            // 
            // CloseAssistantPerformerButton
            // 
            this.CloseAssistantPerformerButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(64)))), ((int)(((byte)(0)))));
            this.CloseAssistantPerformerButton.Location = new System.Drawing.Point(16, 30);
            this.CloseAssistantPerformerButton.Name = "CloseAssistantPerformerButton";
            this.CloseAssistantPerformerButton.Size = new System.Drawing.Size(204, 31);
            this.CloseAssistantPerformerButton.TabIndex = 46;
            this.CloseAssistantPerformerButton.Text = "close";
            this.CloseAssistantPerformerButton.UseVisualStyleBackColor = true;
            this.CloseAssistantPerformerButton.Click += new System.EventHandler(this.CloseAssistantPerformerButton_Click);
            // 
            // NewScoreSettingsButton
            // 
            this.NewScoreSettingsButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(64)))), ((int)(((byte)(0)))));
            this.NewScoreSettingsButton.Location = new System.Drawing.Point(17, 30);
            this.NewScoreSettingsButton.Name = "NewScoreSettingsButton";
            this.NewScoreSettingsButton.Size = new System.Drawing.Size(99, 31);
            this.NewScoreSettingsButton.TabIndex = 47;
            this.NewScoreSettingsButton.Text = "new settings";
            this.NewScoreSettingsButton.UseVisualStyleBackColor = true;
            this.NewScoreSettingsButton.Click += new System.EventHandler(this.NewScoreSettingsButton_Click);
            // 
            // LoadScoreSettingsButton
            // 
            this.LoadScoreSettingsButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(64)))), ((int)(((byte)(0)))));
            this.LoadScoreSettingsButton.Location = new System.Drawing.Point(122, 30);
            this.LoadScoreSettingsButton.Name = "LoadScoreSettingsButton";
            this.LoadScoreSettingsButton.Size = new System.Drawing.Size(99, 31);
            this.LoadScoreSettingsButton.TabIndex = 48;
            this.LoadScoreSettingsButton.Text = "load settings";
            this.LoadScoreSettingsButton.UseVisualStyleBackColor = true;
            this.LoadScoreSettingsButton.Click += new System.EventHandler(this.LoadScoreSettingsButton_Click);
            // 
            // CloseAssistantComposerButton
            // 
            this.CloseAssistantComposerButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(64)))), ((int)(((byte)(0)))));
            this.CloseAssistantComposerButton.Location = new System.Drawing.Point(67, 30);
            this.CloseAssistantComposerButton.Name = "CloseAssistantComposerButton";
            this.CloseAssistantComposerButton.Size = new System.Drawing.Size(99, 31);
            this.CloseAssistantComposerButton.TabIndex = 49;
            this.CloseAssistantComposerButton.Text = "close";
            this.CloseAssistantComposerButton.UseVisualStyleBackColor = true;
            this.CloseAssistantComposerButton.Click += new System.EventHandler(this.CloseAssistantComposerButton_Click);
            // 
            // KrystalsEditorButton
            // 
            this.KrystalsEditorButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(64)))), ((int)(((byte)(0)))));
            this.KrystalsEditorButton.Location = new System.Drawing.Point(20, 30);
            this.KrystalsEditorButton.Name = "KrystalsEditorButton";
            this.KrystalsEditorButton.Size = new System.Drawing.Size(98, 31);
            this.KrystalsEditorButton.TabIndex = 50;
            this.KrystalsEditorButton.Text = "open";
            this.KrystalsEditorButton.UseVisualStyleBackColor = true;
            this.KrystalsEditorButton.Click += new System.EventHandler(this.KrystalsEditorButton_Click);
            // 
            // AboutButton
            // 
            this.AboutButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(64)))), ((int)(((byte)(0)))));
            this.AboutButton.Location = new System.Drawing.Point(119, 30);
            this.AboutButton.Name = "AboutButton";
            this.AboutButton.Size = new System.Drawing.Size(98, 31);
            this.AboutButton.TabIndex = 51;
            this.AboutButton.Text = "about";
            this.AboutButton.UseVisualStyleBackColor = true;
            this.AboutButton.Click += new System.EventHandler(this.AboutButton_Click);
            // 
            // AssistantComposerPanel
            // 
            this.AssistantComposerPanel.BackColor = System.Drawing.Color.Honeydew;
            this.AssistantComposerPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.AssistantComposerPanel.Controls.Add(this.AssistantComposerLabel);
            this.AssistantComposerPanel.Controls.Add(this.CloseAssistantComposerButton);
            this.AssistantComposerPanel.Controls.Add(this.NewScoreSettingsButton);
            this.AssistantComposerPanel.Controls.Add(this.LoadScoreSettingsButton);
            this.AssistantComposerPanel.Location = new System.Drawing.Point(23, 64);
            this.AssistantComposerPanel.Name = "AssistantComposerPanel";
            this.AssistantComposerPanel.Size = new System.Drawing.Size(237, 80);
            this.AssistantComposerPanel.TabIndex = 52;
            // 
            // AssistantComposerLabel
            // 
            this.AssistantComposerLabel.AutoSize = true;
            this.AssistantComposerLabel.Font = new System.Drawing.Font("Verdana", 9F);
            this.AssistantComposerLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(64)))), ((int)(((byte)(0)))));
            this.AssistantComposerLabel.Location = new System.Drawing.Point(7, 6);
            this.AssistantComposerLabel.Name = "AssistantComposerLabel";
            this.AssistantComposerLabel.Size = new System.Drawing.Size(133, 14);
            this.AssistantComposerLabel.TabIndex = 0;
            this.AssistantComposerLabel.Text = "Assistant Composer";
            // 
            // AssistantPerformerPanel
            // 
            this.AssistantPerformerPanel.BackColor = System.Drawing.Color.Honeydew;
            this.AssistantPerformerPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.AssistantPerformerPanel.Controls.Add(this.AssistantPerformerLabel);
            this.AssistantPerformerPanel.Controls.Add(this.PerformScoreButton);
            this.AssistantPerformerPanel.Controls.Add(this.CloseAssistantPerformerButton);
            this.AssistantPerformerPanel.Location = new System.Drawing.Point(278, 64);
            this.AssistantPerformerPanel.Name = "AssistantPerformerPanel";
            this.AssistantPerformerPanel.Size = new System.Drawing.Size(237, 80);
            this.AssistantPerformerPanel.TabIndex = 53;
            // 
            // AssistantPerformerLabel
            // 
            this.AssistantPerformerLabel.AutoSize = true;
            this.AssistantPerformerLabel.Font = new System.Drawing.Font("Verdana", 9F);
            this.AssistantPerformerLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(64)))), ((int)(((byte)(0)))));
            this.AssistantPerformerLabel.Location = new System.Drawing.Point(7, 6);
            this.AssistantPerformerLabel.Name = "AssistantPerformerLabel";
            this.AssistantPerformerLabel.Size = new System.Drawing.Size(131, 14);
            this.AssistantPerformerLabel.TabIndex = 0;
            this.AssistantPerformerLabel.Text = "Assistant Performer";
            // 
            // KrystalsEditorPanel
            // 
            this.KrystalsEditorPanel.BackColor = System.Drawing.Color.Honeydew;
            this.KrystalsEditorPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.KrystalsEditorPanel.Controls.Add(this.KrystalsEditorLabel);
            this.KrystalsEditorPanel.Controls.Add(this.KrystalsEditorButton);
            this.KrystalsEditorPanel.Location = new System.Drawing.Point(23, 161);
            this.KrystalsEditorPanel.Name = "KrystalsEditorPanel";
            this.KrystalsEditorPanel.Size = new System.Drawing.Size(137, 80);
            this.KrystalsEditorPanel.TabIndex = 54;
            // 
            // KrystalsEditorLabel
            // 
            this.KrystalsEditorLabel.AutoSize = true;
            this.KrystalsEditorLabel.Font = new System.Drawing.Font("Verdana", 9F);
            this.KrystalsEditorLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(64)))), ((int)(((byte)(0)))));
            this.KrystalsEditorLabel.Location = new System.Drawing.Point(7, 6);
            this.KrystalsEditorLabel.Name = "KrystalsEditorLabel";
            this.KrystalsEditorLabel.Size = new System.Drawing.Size(98, 14);
            this.KrystalsEditorLabel.TabIndex = 0;
            this.KrystalsEditorLabel.Text = "Krystals Editor";
            // 
            // MoritzPanel
            // 
            this.MoritzPanel.BackColor = System.Drawing.Color.Honeydew;
            this.MoritzPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.MoritzPanel.Controls.Add(this.MoritzLabel);
            this.MoritzPanel.Controls.Add(this.AboutButton);
            this.MoritzPanel.Controls.Add(this.PreferencesButton);
            this.MoritzPanel.Controls.Add(this.QuitButton);
            this.MoritzPanel.Location = new System.Drawing.Point(177, 161);
            this.MoritzPanel.Name = "MoritzPanel";
            this.MoritzPanel.Size = new System.Drawing.Size(338, 80);
            this.MoritzPanel.TabIndex = 55;
            // 
            // MoritzLabel
            // 
            this.MoritzLabel.AutoSize = true;
            this.MoritzLabel.Font = new System.Drawing.Font("Verdana", 9F);
            this.MoritzLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(64)))), ((int)(((byte)(0)))));
            this.MoritzLabel.Location = new System.Drawing.Point(7, 6);
            this.MoritzLabel.Name = "MoritzLabel";
            this.MoritzLabel.Size = new System.Drawing.Size(45, 14);
            this.MoritzLabel.TabIndex = 0;
            this.MoritzLabel.Text = "Moritz";
            // 
            // MoritzForm1
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(255)))), ((int)(((byte)(245)))));
            this.ClientSize = new System.Drawing.Size(539, 265);
            this.ControlBox = false;
            this.Controls.Add(this.KrystalsEditorPanel);
            this.Controls.Add(this.AssistantPerformerPanel);
            this.Controls.Add(this.CurrentOutputDeviceLabel);
            this.Controls.Add(this.CurrentInputDeviceLabel);
            this.Controls.Add(this.OutputDeviceComboBox);
            this.Controls.Add(this.InputDeviceComboBox);
            this.Controls.Add(this.AssistantComposerPanel);
            this.Controls.Add(this.MoritzPanel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "MoritzForm1";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Moritz v2";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MoritzForm1_FormClosing);
            this.VisibleChanged += new System.EventHandler(this.MoritzForm1_VisibleChanged);
            this.AssistantComposerPanel.ResumeLayout(false);
            this.AssistantComposerPanel.PerformLayout();
            this.AssistantPerformerPanel.ResumeLayout(false);
            this.AssistantPerformerPanel.PerformLayout();
            this.KrystalsEditorPanel.ResumeLayout(false);
            this.KrystalsEditorPanel.PerformLayout();
            this.MoritzPanel.ResumeLayout(false);
            this.MoritzPanel.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

        private System.Windows.Forms.Button PerformScoreButton;
        private System.Windows.Forms.Button QuitButton;
        private System.Windows.Forms.Button PreferencesButton;
        private System.Windows.Forms.Label CurrentOutputDeviceLabel;
        private System.Windows.Forms.Label CurrentInputDeviceLabel;
        private System.Windows.Forms.ComboBox OutputDeviceComboBox;
        private System.Windows.Forms.ComboBox InputDeviceComboBox;
        private System.Windows.Forms.Button CloseAssistantPerformerButton;
        private System.Windows.Forms.Button NewScoreSettingsButton;
        private System.Windows.Forms.Button LoadScoreSettingsButton;
        private System.Windows.Forms.Button CloseAssistantComposerButton;
        private System.Windows.Forms.Button KrystalsEditorButton;
        private System.Windows.Forms.Button AboutButton;
        private System.Windows.Forms.Panel AssistantComposerPanel;
        private System.Windows.Forms.Label AssistantComposerLabel;
        private System.Windows.Forms.Panel AssistantPerformerPanel;
        private System.Windows.Forms.Label AssistantPerformerLabel;
        private System.Windows.Forms.Panel KrystalsEditorPanel;
        private System.Windows.Forms.Label KrystalsEditorLabel;
        private System.Windows.Forms.Panel MoritzPanel;
        private System.Windows.Forms.Label MoritzLabel;

    }
}