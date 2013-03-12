namespace Moritz
{
	partial class PreferencesDialog
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
			if(disposing)
			{
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
            this.OKBtn = new System.Windows.Forms.Button();
            this.LocalKrystalsFolderLabel = new System.Windows.Forms.Label();
            this.LocalScoresRootFolderLabel = new System.Windows.Forms.Label();
            this.LocalExpansionFieldsFolderLabel = new System.Windows.Forms.Label();
            this.LocalModulationOperatorsLabel = new System.Windows.Forms.Label();
            this.DevicesGroupBox = new System.Windows.Forms.GroupBox();
            this.PreferredOutputDeviceNameTextBox = new System.Windows.Forms.TextBox();
            this.PreferredOutputDeviceLabel = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.PreferredInputDeviceNameTextBox = new System.Windows.Forms.TextBox();
            this.ActiveOutputDevicesListBox = new System.Windows.Forms.ListBox();
            this.ActiveInputDevicesListBox = new System.Windows.Forms.ListBox();
            this.PreferrredInputDeviceLabel = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.LocalUserFolderButton = new System.Windows.Forms.Button();
            this.LocalUserFolderTextBox = new System.Windows.Forms.TextBox();
            this.LocalUserFolderLabel = new System.Windows.Forms.Label();
            this.LocalKrystalsFolderInfoLabel = new System.Windows.Forms.Label();
            this.LocalExpansionFieldsFolderInfoLabel = new System.Windows.Forms.Label();
            this.LocalModulationOperatorsFolderInfoLabel = new System.Windows.Forms.Label();
            this.LocalScoresRootFolderInfoLabel = new System.Windows.Forms.Label();
            this.PreferencesFileLabel = new System.Windows.Forms.Label();
            this.PreferencesFilePathLabel = new System.Windows.Forms.Label();
            this.LocalUserFoldersGroupBox = new System.Windows.Forms.GroupBox();
            this.OnlineUserFolderTextBox = new System.Windows.Forms.TextBox();
            this.OnlineUserFolderLabel = new System.Windows.Forms.Label();
            this.OnlineUserFoldersGroupBox = new System.Windows.Forms.GroupBox();
            this.OnlineMoritzAudioFolderLabel = new System.Windows.Forms.Label();
            this.OnlineMoritzAudioFolderInfoLabel = new System.Windows.Forms.Label();
            this.DevicesGroupBox.SuspendLayout();
            this.LocalUserFoldersGroupBox.SuspendLayout();
            this.OnlineUserFoldersGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // OKBtn
            // 
            this.OKBtn.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.OKBtn.Location = new System.Drawing.Point(663, 460);
            this.OKBtn.Name = "OKBtn";
            this.OKBtn.Size = new System.Drawing.Size(70, 23);
            this.OKBtn.TabIndex = 19;
            this.OKBtn.Text = "OK";
            this.OKBtn.UseVisualStyleBackColor = true;
            this.OKBtn.Click += new System.EventHandler(this.OKBtn_Click);
            // 
            // LocalKrystalsFolderLabel
            // 
            this.LocalKrystalsFolderLabel.AutoSize = true;
            this.LocalKrystalsFolderLabel.Location = new System.Drawing.Point(72, 21);
            this.LocalKrystalsFolderLabel.Name = "LocalKrystalsFolderLabel";
            this.LocalKrystalsFolderLabel.Size = new System.Drawing.Size(104, 13);
            this.LocalKrystalsFolderLabel.TabIndex = 22;
            this.LocalKrystalsFolderLabel.Text = "Local Krystals folder:";
            // 
            // LocalScoresRootFolderLabel
            // 
            this.LocalScoresRootFolderLabel.AutoSize = true;
            this.LocalScoresRootFolderLabel.Location = new System.Drawing.Point(54, 81);
            this.LocalScoresRootFolderLabel.Name = "LocalScoresRootFolderLabel";
            this.LocalScoresRootFolderLabel.Size = new System.Drawing.Size(122, 13);
            this.LocalScoresRootFolderLabel.TabIndex = 26;
            this.LocalScoresRootFolderLabel.Text = "Local Scores root folder:";
            // 
            // LocalExpansionFieldsFolderLabel
            // 
            this.LocalExpansionFieldsFolderLabel.AutoSize = true;
            this.LocalExpansionFieldsFolderLabel.Location = new System.Drawing.Point(32, 41);
            this.LocalExpansionFieldsFolderLabel.Name = "LocalExpansionFieldsFolderLabel";
            this.LocalExpansionFieldsFolderLabel.Size = new System.Drawing.Size(144, 13);
            this.LocalExpansionFieldsFolderLabel.TabIndex = 23;
            this.LocalExpansionFieldsFolderLabel.Text = "Local Expansion fields folder:";
            // 
            // LocalModulationOperatorsLabel
            // 
            this.LocalModulationOperatorsLabel.AutoSize = true;
            this.LocalModulationOperatorsLabel.Location = new System.Drawing.Point(9, 61);
            this.LocalModulationOperatorsLabel.Name = "LocalModulationOperatorsLabel";
            this.LocalModulationOperatorsLabel.Size = new System.Drawing.Size(167, 13);
            this.LocalModulationOperatorsLabel.TabIndex = 24;
            this.LocalModulationOperatorsLabel.Text = "Local Modulation operators folder:";
            // 
            // DevicesGroupBox
            // 
            this.DevicesGroupBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(248)))), ((int)(((byte)(248)))));
            this.DevicesGroupBox.Controls.Add(this.PreferredOutputDeviceNameTextBox);
            this.DevicesGroupBox.Controls.Add(this.PreferredOutputDeviceLabel);
            this.DevicesGroupBox.Controls.Add(this.label6);
            this.DevicesGroupBox.Controls.Add(this.PreferredInputDeviceNameTextBox);
            this.DevicesGroupBox.Controls.Add(this.ActiveOutputDevicesListBox);
            this.DevicesGroupBox.Controls.Add(this.ActiveInputDevicesListBox);
            this.DevicesGroupBox.Controls.Add(this.PreferrredInputDeviceLabel);
            this.DevicesGroupBox.Controls.Add(this.label1);
            this.DevicesGroupBox.Controls.Add(this.label2);
            this.DevicesGroupBox.Location = new System.Drawing.Point(165, 285);
            this.DevicesGroupBox.Name = "DevicesGroupBox";
            this.DevicesGroupBox.Size = new System.Drawing.Size(427, 198);
            this.DevicesGroupBox.TabIndex = 18;
            this.DevicesGroupBox.TabStop = false;
            this.DevicesGroupBox.Text = "MIDI Devices";
            // 
            // PreferredOutputDeviceNameTextBox
            // 
            this.PreferredOutputDeviceNameTextBox.Location = new System.Drawing.Point(233, 126);
            this.PreferredOutputDeviceNameTextBox.Name = "PreferredOutputDeviceNameTextBox";
            this.PreferredOutputDeviceNameTextBox.Size = new System.Drawing.Size(161, 20);
            this.PreferredOutputDeviceNameTextBox.TabIndex = 12;
            // 
            // PreferredOutputDeviceLabel
            // 
            this.PreferredOutputDeviceLabel.AutoSize = true;
            this.PreferredOutputDeviceLabel.Location = new System.Drawing.Point(233, 111);
            this.PreferredOutputDeviceLabel.Name = "PreferredOutputDeviceLabel";
            this.PreferredOutputDeviceLabel.Size = new System.Drawing.Size(125, 13);
            this.PreferredOutputDeviceLabel.TabIndex = 13;
            this.PreferredOutputDeviceLabel.Text = "Preferred Output Device:";
            // 
            // label6
            // 
            this.label6.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.label6.AutoSize = true;
            this.label6.ForeColor = System.Drawing.Color.RoyalBlue;
            this.label6.Location = new System.Drawing.Point(43, 149);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(337, 39);
            this.label6.TabIndex = 11;
            this.label6.Text = "Copy the preferred input and output device names from the above lists\r\ninto the c" +
    "orresponding fields.\r\nThese values will be used as the defaults in Moritz\' devic" +
    "e selectors.\r\n";
            this.label6.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // PreferredInputDeviceNameTextBox
            // 
            this.PreferredInputDeviceNameTextBox.Location = new System.Drawing.Point(33, 126);
            this.PreferredInputDeviceNameTextBox.Name = "PreferredInputDeviceNameTextBox";
            this.PreferredInputDeviceNameTextBox.Size = new System.Drawing.Size(161, 20);
            this.PreferredInputDeviceNameTextBox.TabIndex = 1;
            // 
            // ActiveOutputDevicesListBox
            // 
            this.ActiveOutputDevicesListBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(248)))), ((int)(((byte)(248)))));
            this.ActiveOutputDevicesListBox.FormattingEnabled = true;
            this.ActiveOutputDevicesListBox.Location = new System.Drawing.Point(233, 35);
            this.ActiveOutputDevicesListBox.Name = "ActiveOutputDevicesListBox";
            this.ActiveOutputDevicesListBox.Size = new System.Drawing.Size(161, 69);
            this.ActiveOutputDevicesListBox.TabIndex = 7;
            // 
            // ActiveInputDevicesListBox
            // 
            this.ActiveInputDevicesListBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(248)))), ((int)(((byte)(248)))));
            this.ActiveInputDevicesListBox.FormattingEnabled = true;
            this.ActiveInputDevicesListBox.Location = new System.Drawing.Point(33, 35);
            this.ActiveInputDevicesListBox.Name = "ActiveInputDevicesListBox";
            this.ActiveInputDevicesListBox.Size = new System.Drawing.Size(161, 69);
            this.ActiveInputDevicesListBox.TabIndex = 5;
            // 
            // PreferrredInputDeviceLabel
            // 
            this.PreferrredInputDeviceLabel.AutoSize = true;
            this.PreferrredInputDeviceLabel.Location = new System.Drawing.Point(33, 111);
            this.PreferrredInputDeviceLabel.Name = "PreferrredInputDeviceLabel";
            this.PreferrredInputDeviceLabel.Size = new System.Drawing.Size(117, 13);
            this.PreferrredInputDeviceLabel.TabIndex = 9;
            this.PreferrredInputDeviceLabel.Text = "Preferred Input Device:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(33, 21);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(109, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "Active Input Devices:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(233, 20);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(117, 13);
            this.label2.TabIndex = 6;
            this.label2.Text = "Active Output Devices:";
            // 
            // LocalUserFolderButton
            // 
            this.LocalUserFolderButton.Location = new System.Drawing.Point(706, 47);
            this.LocalUserFolderButton.Name = "LocalUserFolderButton";
            this.LocalUserFolderButton.Size = new System.Drawing.Size(27, 23);
            this.LocalUserFolderButton.TabIndex = 29;
            this.LocalUserFolderButton.Text = "...";
            this.LocalUserFolderButton.UseVisualStyleBackColor = true;
            this.LocalUserFolderButton.Click += new System.EventHandler(this.LocalUserFolderButton_Click);
            // 
            // LocalUserFolderTextBox
            // 
            this.LocalUserFolderTextBox.Location = new System.Drawing.Point(165, 49);
            this.LocalUserFolderTextBox.Name = "LocalUserFolderTextBox";
            this.LocalUserFolderTextBox.Size = new System.Drawing.Size(535, 20);
            this.LocalUserFolderTextBox.TabIndex = 28;
            this.LocalUserFolderTextBox.Leave += new System.EventHandler(this.LocalUserFolderTextBox_Leave);
            // 
            // LocalUserFolderLabel
            // 
            this.LocalUserFolderLabel.AutoSize = true;
            this.LocalUserFolderLabel.Location = new System.Drawing.Point(76, 52);
            this.LocalUserFolderLabel.Name = "LocalUserFolderLabel";
            this.LocalUserFolderLabel.Size = new System.Drawing.Size(88, 13);
            this.LocalUserFolderLabel.TabIndex = 30;
            this.LocalUserFolderLabel.Text = "Local user folder:";
            // 
            // LocalKrystalsFolderInfoLabel
            // 
            this.LocalKrystalsFolderInfoLabel.AutoSize = true;
            this.LocalKrystalsFolderInfoLabel.ForeColor = System.Drawing.Color.RoyalBlue;
            this.LocalKrystalsFolderInfoLabel.Location = new System.Drawing.Point(174, 21);
            this.LocalKrystalsFolderInfoLabel.Name = "LocalKrystalsFolderInfoLabel";
            this.LocalKrystalsFolderInfoLabel.Size = new System.Drawing.Size(71, 13);
            this.LocalKrystalsFolderInfoLabel.TabIndex = 31;
            this.LocalKrystalsFolderInfoLabel.Text = "krystals folder";
            // 
            // LocalExpansionFieldsFolderInfoLabel
            // 
            this.LocalExpansionFieldsFolderInfoLabel.AutoSize = true;
            this.LocalExpansionFieldsFolderInfoLabel.ForeColor = System.Drawing.Color.RoyalBlue;
            this.LocalExpansionFieldsFolderInfoLabel.Location = new System.Drawing.Point(174, 41);
            this.LocalExpansionFieldsFolderInfoLabel.Name = "LocalExpansionFieldsFolderInfoLabel";
            this.LocalExpansionFieldsFolderInfoLabel.Size = new System.Drawing.Size(111, 13);
            this.LocalExpansionFieldsFolderInfoLabel.TabIndex = 32;
            this.LocalExpansionFieldsFolderInfoLabel.Text = "expansion fields folder";
            // 
            // LocalModulationOperatorsFolderInfoLabel
            // 
            this.LocalModulationOperatorsFolderInfoLabel.AutoSize = true;
            this.LocalModulationOperatorsFolderInfoLabel.ForeColor = System.Drawing.Color.RoyalBlue;
            this.LocalModulationOperatorsFolderInfoLabel.Location = new System.Drawing.Point(174, 61);
            this.LocalModulationOperatorsFolderInfoLabel.Name = "LocalModulationOperatorsFolderInfoLabel";
            this.LocalModulationOperatorsFolderInfoLabel.Size = new System.Drawing.Size(134, 13);
            this.LocalModulationOperatorsFolderInfoLabel.TabIndex = 33;
            this.LocalModulationOperatorsFolderInfoLabel.Text = "modulation operators folder";
            // 
            // LocalScoresRootFolderInfoLabel
            // 
            this.LocalScoresRootFolderInfoLabel.AutoSize = true;
            this.LocalScoresRootFolderInfoLabel.ForeColor = System.Drawing.Color.RoyalBlue;
            this.LocalScoresRootFolderInfoLabel.Location = new System.Drawing.Point(174, 81);
            this.LocalScoresRootFolderInfoLabel.Name = "LocalScoresRootFolderInfoLabel";
            this.LocalScoresRootFolderInfoLabel.Size = new System.Drawing.Size(88, 13);
            this.LocalScoresRootFolderInfoLabel.TabIndex = 35;
            this.LocalScoresRootFolderInfoLabel.Text = "scores root folder";
            // 
            // PreferencesFileLabel
            // 
            this.PreferencesFileLabel.AutoSize = true;
            this.PreferencesFileLabel.Location = new System.Drawing.Point(81, 19);
            this.PreferencesFileLabel.Name = "PreferencesFileLabel";
            this.PreferencesFileLabel.Size = new System.Drawing.Size(83, 13);
            this.PreferencesFileLabel.TabIndex = 36;
            this.PreferencesFileLabel.Text = "Preferences file:";
            // 
            // PreferencesFilePathLabel
            // 
            this.PreferencesFilePathLabel.AutoSize = true;
            this.PreferencesFilePathLabel.Location = new System.Drawing.Point(165, 19);
            this.PreferencesFilePathLabel.Name = "PreferencesFilePathLabel";
            this.PreferencesFilePathLabel.Size = new System.Drawing.Size(103, 13);
            this.PreferencesFilePathLabel.TabIndex = 37;
            this.PreferencesFilePathLabel.Text = "preferences file path";
            // 
            // LocalUserFoldersGroupBox
            // 
            this.LocalUserFoldersGroupBox.Controls.Add(this.LocalKrystalsFolderLabel);
            this.LocalUserFoldersGroupBox.Controls.Add(this.LocalScoresRootFolderLabel);
            this.LocalUserFoldersGroupBox.Controls.Add(this.LocalScoresRootFolderInfoLabel);
            this.LocalUserFoldersGroupBox.Controls.Add(this.LocalExpansionFieldsFolderLabel);
            this.LocalUserFoldersGroupBox.Controls.Add(this.LocalModulationOperatorsLabel);
            this.LocalUserFoldersGroupBox.Controls.Add(this.LocalModulationOperatorsFolderInfoLabel);
            this.LocalUserFoldersGroupBox.Controls.Add(this.LocalKrystalsFolderInfoLabel);
            this.LocalUserFoldersGroupBox.Controls.Add(this.LocalExpansionFieldsFolderInfoLabel);
            this.LocalUserFoldersGroupBox.Location = new System.Drawing.Point(18, 73);
            this.LocalUserFoldersGroupBox.Name = "LocalUserFoldersGroupBox";
            this.LocalUserFoldersGroupBox.Size = new System.Drawing.Size(715, 109);
            this.LocalUserFoldersGroupBox.TabIndex = 38;
            this.LocalUserFoldersGroupBox.TabStop = false;
            this.LocalUserFoldersGroupBox.Text = "Local user folders ( info )";
            // 
            // OnlineUserFolderTextBox
            // 
            this.OnlineUserFolderTextBox.Location = new System.Drawing.Point(165, 197);
            this.OnlineUserFolderTextBox.Name = "OnlineUserFolderTextBox";
            this.OnlineUserFolderTextBox.Size = new System.Drawing.Size(568, 20);
            this.OnlineUserFolderTextBox.TabIndex = 39;
            this.OnlineUserFolderTextBox.Leave += new System.EventHandler(this.OnlineUserFolderTextBox_Leave);
            // 
            // OnlineUserFolderLabel
            // 
            this.OnlineUserFolderLabel.AutoSize = true;
            this.OnlineUserFolderLabel.Location = new System.Drawing.Point(41, 200);
            this.OnlineUserFolderLabel.Name = "OnlineUserFolderLabel";
            this.OnlineUserFolderLabel.Size = new System.Drawing.Size(123, 13);
            this.OnlineUserFolderLabel.TabIndex = 40;
            this.OnlineUserFolderLabel.Text = "Online user folder (URL):";
            // 
            // OnlineUserFoldersGroupBox
            // 
            this.OnlineUserFoldersGroupBox.Controls.Add(this.OnlineMoritzAudioFolderLabel);
            this.OnlineUserFoldersGroupBox.Controls.Add(this.OnlineMoritzAudioFolderInfoLabel);
            this.OnlineUserFoldersGroupBox.Location = new System.Drawing.Point(18, 221);
            this.OnlineUserFoldersGroupBox.Name = "OnlineUserFoldersGroupBox";
            this.OnlineUserFoldersGroupBox.Size = new System.Drawing.Size(715, 51);
            this.OnlineUserFoldersGroupBox.TabIndex = 39;
            this.OnlineUserFoldersGroupBox.TabStop = false;
            this.OnlineUserFoldersGroupBox.Text = "Online user folders ( info )";
            // 
            // OnlineMoritzAudioFolderLabel
            // 
            this.OnlineMoritzAudioFolderLabel.AutoSize = true;
            this.OnlineMoritzAudioFolderLabel.Location = new System.Drawing.Point(52, 21);
            this.OnlineMoritzAudioFolderLabel.Name = "OnlineMoritzAudioFolderLabel";
            this.OnlineMoritzAudioFolderLabel.Size = new System.Drawing.Size(124, 13);
            this.OnlineMoritzAudioFolderLabel.TabIndex = 23;
            this.OnlineMoritzAudioFolderLabel.Text = "Online User Audio folder:";
            // 
            // OnlineMoritzAudioFolderInfoLabel
            // 
            this.OnlineMoritzAudioFolderInfoLabel.AutoSize = true;
            this.OnlineMoritzAudioFolderInfoLabel.ForeColor = System.Drawing.Color.RoyalBlue;
            this.OnlineMoritzAudioFolderInfoLabel.Location = new System.Drawing.Point(174, 21);
            this.OnlineMoritzAudioFolderInfoLabel.Name = "OnlineMoritzAudioFolderInfoLabel";
            this.OnlineMoritzAudioFolderInfoLabel.Size = new System.Drawing.Size(116, 13);
            this.OnlineMoritzAudioFolderInfoLabel.TabIndex = 32;
            this.OnlineMoritzAudioFolderInfoLabel.Text = "online user audio folder";
            // 
            // PreferencesDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(248)))), ((int)(((byte)(248)))));
            this.ClientSize = new System.Drawing.Size(753, 499);
            this.ControlBox = false;
            this.Controls.Add(this.OnlineUserFoldersGroupBox);
            this.Controls.Add(this.OnlineUserFolderTextBox);
            this.Controls.Add(this.OnlineUserFolderLabel);
            this.Controls.Add(this.LocalUserFoldersGroupBox);
            this.Controls.Add(this.PreferencesFilePathLabel);
            this.Controls.Add(this.PreferencesFileLabel);
            this.Controls.Add(this.LocalUserFolderButton);
            this.Controls.Add(this.LocalUserFolderTextBox);
            this.Controls.Add(this.LocalUserFolderLabel);
            this.Controls.Add(this.DevicesGroupBox);
            this.Controls.Add(this.OKBtn);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "PreferencesDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Moritz Preferences";
            this.DevicesGroupBox.ResumeLayout(false);
            this.DevicesGroupBox.PerformLayout();
            this.LocalUserFoldersGroupBox.ResumeLayout(false);
            this.LocalUserFoldersGroupBox.PerformLayout();
            this.OnlineUserFoldersGroupBox.ResumeLayout(false);
            this.OnlineUserFoldersGroupBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

        private System.Windows.Forms.Button OKBtn;
        private System.Windows.Forms.Label LocalKrystalsFolderLabel;
        private System.Windows.Forms.Label LocalScoresRootFolderLabel;
        private System.Windows.Forms.Label LocalExpansionFieldsFolderLabel;
        private System.Windows.Forms.Label LocalModulationOperatorsLabel;
        private System.Windows.Forms.GroupBox DevicesGroupBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ListBox ActiveOutputDevicesListBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ListBox ActiveInputDevicesListBox;
        private System.Windows.Forms.TextBox PreferredInputDeviceNameTextBox;
        private System.Windows.Forms.Label PreferrredInputDeviceLabel;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox PreferredOutputDeviceNameTextBox;
        private System.Windows.Forms.Label PreferredOutputDeviceLabel;
        private System.Windows.Forms.Button LocalUserFolderButton;
        private System.Windows.Forms.TextBox LocalUserFolderTextBox;
        private System.Windows.Forms.Label LocalUserFolderLabel;
        private System.Windows.Forms.Label LocalKrystalsFolderInfoLabel;
        private System.Windows.Forms.Label LocalExpansionFieldsFolderInfoLabel;
        private System.Windows.Forms.Label LocalModulationOperatorsFolderInfoLabel;
        private System.Windows.Forms.Label LocalScoresRootFolderInfoLabel;
        private System.Windows.Forms.Label PreferencesFileLabel;
        private System.Windows.Forms.Label PreferencesFilePathLabel;
        private System.Windows.Forms.GroupBox LocalUserFoldersGroupBox;
        private System.Windows.Forms.TextBox OnlineUserFolderTextBox;
        private System.Windows.Forms.Label OnlineUserFolderLabel;
        private System.Windows.Forms.GroupBox OnlineUserFoldersGroupBox;
        private System.Windows.Forms.Label OnlineMoritzAudioFolderLabel;
        private System.Windows.Forms.Label OnlineMoritzAudioFolderInfoLabel;
	}
}