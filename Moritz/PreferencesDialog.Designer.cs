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
            this.LocalMoritzFolderLabel = new System.Windows.Forms.Label();
            this.LocalKrystalsFolderInfoLabel = new System.Windows.Forms.Label();
            this.LocalExpansionFieldsFolderInfoLabel = new System.Windows.Forms.Label();
            this.LocalModulationOperatorsFolderInfoLabel = new System.Windows.Forms.Label();
            this.LocalScoresRootFolderInfoLabel = new System.Windows.Forms.Label();
            this.PreferencesFileLabel = new System.Windows.Forms.Label();
            this.PreferencesFilePathLabel = new System.Windows.Forms.Label();
            this.LocalMoritzFoldersGroupBox = new System.Windows.Forms.GroupBox();
            this.LocalMoritzFolderInfoLabel = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.OnlineMoritzFolderLabel = new System.Windows.Forms.Label();
            this.OnlineMoritzFolderInfoLabel = new System.Windows.Forms.Label();
            this.OnlineMoritzAudioFolderLabel = new System.Windows.Forms.Label();
            this.OnlineMoritzAudioFolderInfoLabel = new System.Windows.Forms.Label();
            this.OnlineXMLSchemasFolderLabel = new System.Windows.Forms.Label();
            this.OnlineXMLSchemasFolderInfoLabel = new System.Windows.Forms.Label();
            this.DevicesGroupBox.SuspendLayout();
            this.LocalMoritzFoldersGroupBox.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // OKBtn
            // 
            this.OKBtn.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.OKBtn.Location = new System.Drawing.Point(365, 620);
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
            this.DevicesGroupBox.Location = new System.Drawing.Point(187, 18);
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
            // LocalMoritzFolderLabel
            // 
            this.LocalMoritzFolderLabel.AutoSize = true;
            this.LocalMoritzFolderLabel.Location = new System.Drawing.Point(94, 25);
            this.LocalMoritzFolderLabel.Name = "LocalMoritzFolderLabel";
            this.LocalMoritzFolderLabel.Size = new System.Drawing.Size(96, 13);
            this.LocalMoritzFolderLabel.TabIndex = 30;
            this.LocalMoritzFolderLabel.Text = "Local Moritz folder:";
            // 
            // LocalKrystalsFolderInfoLabel
            // 
            this.LocalKrystalsFolderInfoLabel.AutoSize = true;
            this.LocalKrystalsFolderInfoLabel.ForeColor = System.Drawing.Color.RoyalBlue;
            this.LocalKrystalsFolderInfoLabel.Location = new System.Drawing.Point(174, 21);
            this.LocalKrystalsFolderInfoLabel.Name = "LocalKrystalsFolderInfoLabel";
            this.LocalKrystalsFolderInfoLabel.Size = new System.Drawing.Size(148, 13);
            this.LocalKrystalsFolderInfoLabel.TabIndex = 31;
            this.LocalKrystalsFolderInfoLabel.Text = "%documents%/Moritz/krystals";
            // 
            // LocalExpansionFieldsFolderInfoLabel
            // 
            this.LocalExpansionFieldsFolderInfoLabel.AutoSize = true;
            this.LocalExpansionFieldsFolderInfoLabel.ForeColor = System.Drawing.Color.RoyalBlue;
            this.LocalExpansionFieldsFolderInfoLabel.Location = new System.Drawing.Point(174, 41);
            this.LocalExpansionFieldsFolderInfoLabel.Name = "LocalExpansionFieldsFolderInfoLabel";
            this.LocalExpansionFieldsFolderInfoLabel.Size = new System.Drawing.Size(248, 13);
            this.LocalExpansionFieldsFolderInfoLabel.TabIndex = 32;
            this.LocalExpansionFieldsFolderInfoLabel.Text = "%documents%/Moritz/krystals/expansion operators";
            // 
            // LocalModulationOperatorsFolderInfoLabel
            // 
            this.LocalModulationOperatorsFolderInfoLabel.AutoSize = true;
            this.LocalModulationOperatorsFolderInfoLabel.ForeColor = System.Drawing.Color.RoyalBlue;
            this.LocalModulationOperatorsFolderInfoLabel.Location = new System.Drawing.Point(174, 61);
            this.LocalModulationOperatorsFolderInfoLabel.Name = "LocalModulationOperatorsFolderInfoLabel";
            this.LocalModulationOperatorsFolderInfoLabel.Size = new System.Drawing.Size(251, 13);
            this.LocalModulationOperatorsFolderInfoLabel.TabIndex = 33;
            this.LocalModulationOperatorsFolderInfoLabel.Text = "%documents%/Moritz/krystals/modulation operators";
            // 
            // LocalScoresRootFolderInfoLabel
            // 
            this.LocalScoresRootFolderInfoLabel.AutoSize = true;
            this.LocalScoresRootFolderInfoLabel.ForeColor = System.Drawing.Color.RoyalBlue;
            this.LocalScoresRootFolderInfoLabel.Location = new System.Drawing.Point(32, 27);
            this.LocalScoresRootFolderInfoLabel.Name = "LocalScoresRootFolderInfoLabel";
            this.LocalScoresRootFolderInfoLabel.Size = new System.Drawing.Size(674, 13);
            this.LocalScoresRootFolderInfoLabel.TabIndex = 35;
            this.LocalScoresRootFolderInfoLabel.Text = "%documents%\\Visual Studio\\Projects\\Web Development\\Projects\\MyWebsite\\james-ingra" +
    "m-act-two\\open-source\\assistantPerformer\\scores";
            // 
            // PreferencesFileLabel
            // 
            this.PreferencesFileLabel.AutoSize = true;
            this.PreferencesFileLabel.Location = new System.Drawing.Point(78, 50);
            this.PreferencesFileLabel.Name = "PreferencesFileLabel";
            this.PreferencesFileLabel.Size = new System.Drawing.Size(112, 13);
            this.PreferencesFileLabel.TabIndex = 36;
            this.PreferencesFileLabel.Text = "Local Preferences file:";
            // 
            // PreferencesFilePathLabel
            // 
            this.PreferencesFilePathLabel.AutoSize = true;
            this.PreferencesFilePathLabel.ForeColor = System.Drawing.Color.RoyalBlue;
            this.PreferencesFilePathLabel.Location = new System.Drawing.Point(188, 50);
            this.PreferencesFilePathLabel.Name = "PreferencesFilePathLabel";
            this.PreferencesFilePathLabel.Size = new System.Drawing.Size(195, 13);
            this.PreferencesFilePathLabel.TabIndex = 37;
            this.PreferencesFilePathLabel.Text = "%documents%/Moritz/Preferences.mzpf";
            // 
            // LocalMoritzFoldersGroupBox
            // 
            this.LocalMoritzFoldersGroupBox.Controls.Add(this.LocalKrystalsFolderLabel);
            this.LocalMoritzFoldersGroupBox.Controls.Add(this.LocalExpansionFieldsFolderLabel);
            this.LocalMoritzFoldersGroupBox.Controls.Add(this.LocalModulationOperatorsLabel);
            this.LocalMoritzFoldersGroupBox.Controls.Add(this.LocalModulationOperatorsFolderInfoLabel);
            this.LocalMoritzFoldersGroupBox.Controls.Add(this.LocalKrystalsFolderInfoLabel);
            this.LocalMoritzFoldersGroupBox.Controls.Add(this.LocalExpansionFieldsFolderInfoLabel);
            this.LocalMoritzFoldersGroupBox.Location = new System.Drawing.Point(14, 74);
            this.LocalMoritzFoldersGroupBox.Name = "LocalMoritzFoldersGroupBox";
            this.LocalMoritzFoldersGroupBox.Size = new System.Drawing.Size(713, 88);
            this.LocalMoritzFoldersGroupBox.TabIndex = 38;
            this.LocalMoritzFoldersGroupBox.TabStop = false;
            this.LocalMoritzFoldersGroupBox.Text = "Local Moritz folders (used by the Assistant Composer and Krystals Editor)";
            // 
            // LocalMoritzFolderInfoLabel
            // 
            this.LocalMoritzFolderInfoLabel.AutoSize = true;
            this.LocalMoritzFolderInfoLabel.ForeColor = System.Drawing.Color.RoyalBlue;
            this.LocalMoritzFolderInfoLabel.Location = new System.Drawing.Point(188, 25);
            this.LocalMoritzFolderInfoLabel.Name = "LocalMoritzFolderInfoLabel";
            this.LocalMoritzFolderInfoLabel.Size = new System.Drawing.Size(108, 13);
            this.LocalMoritzFolderInfoLabel.TabIndex = 38;
            this.LocalMoritzFolderInfoLabel.Text = "%documents%/Moritz";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.LocalScoresRootFolderInfoLabel);
            this.groupBox1.Location = new System.Drawing.Point(14, 177);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(713, 64);
            this.groupBox1.TabIndex = 39;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Local Scores Root Folder (written to by Assistant Composer)";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.groupBox3);
            this.groupBox2.Controls.Add(this.LocalMoritzFolderLabel);
            this.groupBox2.Controls.Add(this.groupBox1);
            this.groupBox2.Controls.Add(this.PreferencesFilePathLabel);
            this.groupBox2.Controls.Add(this.LocalMoritzFolderInfoLabel);
            this.groupBox2.Controls.Add(this.PreferencesFileLabel);
            this.groupBox2.Controls.Add(this.LocalMoritzFoldersGroupBox);
            this.groupBox2.Location = new System.Drawing.Point(21, 234);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(745, 366);
            this.groupBox2.TabIndex = 40;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Environment files and folders ( for reference )";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.OnlineXMLSchemasFolderLabel);
            this.groupBox3.Controls.Add(this.OnlineXMLSchemasFolderInfoLabel);
            this.groupBox3.Controls.Add(this.OnlineMoritzAudioFolderLabel);
            this.groupBox3.Controls.Add(this.OnlineMoritzAudioFolderInfoLabel);
            this.groupBox3.Controls.Add(this.OnlineMoritzFolderLabel);
            this.groupBox3.Controls.Add(this.OnlineMoritzFolderInfoLabel);
            this.groupBox3.Location = new System.Drawing.Point(14, 256);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(713, 94);
            this.groupBox3.TabIndex = 40;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Online Folders";
            // 
            // OnlineMoritzFolderLabel
            // 
            this.OnlineMoritzFolderLabel.AutoSize = true;
            this.OnlineMoritzFolderLabel.Location = new System.Drawing.Point(77, 16);
            this.OnlineMoritzFolderLabel.Name = "OnlineMoritzFolderLabel";
            this.OnlineMoritzFolderLabel.Size = new System.Drawing.Size(100, 13);
            this.OnlineMoritzFolderLabel.TabIndex = 34;
            this.OnlineMoritzFolderLabel.Text = "Online Moritz folder:";
            // 
            // OnlineMoritzFolderInfoLabel
            // 
            this.OnlineMoritzFolderInfoLabel.AutoSize = true;
            this.OnlineMoritzFolderInfoLabel.ForeColor = System.Drawing.Color.RoyalBlue;
            this.OnlineMoritzFolderInfoLabel.Location = new System.Drawing.Point(174, 16);
            this.OnlineMoritzFolderInfoLabel.Name = "OnlineMoritzFolderInfoLabel";
            this.OnlineMoritzFolderInfoLabel.Size = new System.Drawing.Size(249, 13);
            this.OnlineMoritzFolderInfoLabel.TabIndex = 35;
            this.OnlineMoritzFolderInfoLabel.Text = "http://james-ingram-act-two.de/open-source/Moritz";
            // 
            // OnlineMoritzAudioFolderLabel
            // 
            this.OnlineMoritzAudioFolderLabel.AutoSize = true;
            this.OnlineMoritzAudioFolderLabel.Location = new System.Drawing.Point(47, 39);
            this.OnlineMoritzAudioFolderLabel.Name = "OnlineMoritzAudioFolderLabel";
            this.OnlineMoritzAudioFolderLabel.Size = new System.Drawing.Size(130, 13);
            this.OnlineMoritzAudioFolderLabel.TabIndex = 36;
            this.OnlineMoritzAudioFolderLabel.Text = "Online Moritz Audio folder:";
            // 
            // OnlineMoritzAudioFolderInfoLabel
            // 
            this.OnlineMoritzAudioFolderInfoLabel.AutoSize = true;
            this.OnlineMoritzAudioFolderInfoLabel.ForeColor = System.Drawing.Color.RoyalBlue;
            this.OnlineMoritzAudioFolderInfoLabel.Location = new System.Drawing.Point(174, 39);
            this.OnlineMoritzAudioFolderInfoLabel.Name = "OnlineMoritzAudioFolderInfoLabel";
            this.OnlineMoritzAudioFolderInfoLabel.Size = new System.Drawing.Size(280, 13);
            this.OnlineMoritzAudioFolderInfoLabel.TabIndex = 37;
            this.OnlineMoritzAudioFolderInfoLabel.Text = "http://james-ingram-act-two.de/open-source/Moritz/audio";
            // 
            // OnlineXMLSchemasFolderLabel
            // 
            this.OnlineXMLSchemasFolderLabel.AutoSize = true;
            this.OnlineXMLSchemasFolderLabel.Location = new System.Drawing.Point(36, 64);
            this.OnlineXMLSchemasFolderLabel.Name = "OnlineXMLSchemasFolderLabel";
            this.OnlineXMLSchemasFolderLabel.Size = new System.Drawing.Size(141, 13);
            this.OnlineXMLSchemasFolderLabel.TabIndex = 38;
            this.OnlineXMLSchemasFolderLabel.Text = "Online XML Schemas folder:";
            // 
            // OnlineXMLSchemasFolderInfoLabel
            // 
            this.OnlineXMLSchemasFolderInfoLabel.AutoSize = true;
            this.OnlineXMLSchemasFolderInfoLabel.ForeColor = System.Drawing.Color.RoyalBlue;
            this.OnlineXMLSchemasFolderInfoLabel.Location = new System.Drawing.Point(174, 64);
            this.OnlineXMLSchemasFolderInfoLabel.Name = "OnlineXMLSchemasFolderInfoLabel";
            this.OnlineXMLSchemasFolderInfoLabel.Size = new System.Drawing.Size(287, 13);
            this.OnlineXMLSchemasFolderInfoLabel.TabIndex = 39;
            this.OnlineXMLSchemasFolderInfoLabel.Text = "http://james-ingram-act-two.de/open-source/XMLSchemas";
            // 
            // PreferencesDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(248)))), ((int)(((byte)(248)))));
            this.ClientSize = new System.Drawing.Size(788, 668);
            this.ControlBox = false;
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.DevicesGroupBox);
            this.Controls.Add(this.OKBtn);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "PreferencesDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Moritz Preferences";
            this.DevicesGroupBox.ResumeLayout(false);
            this.DevicesGroupBox.PerformLayout();
            this.LocalMoritzFoldersGroupBox.ResumeLayout(false);
            this.LocalMoritzFoldersGroupBox.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.ResumeLayout(false);

		}

		#endregion

        private System.Windows.Forms.Button OKBtn;
        private System.Windows.Forms.Label LocalKrystalsFolderLabel;
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
        private System.Windows.Forms.Label LocalMoritzFolderLabel;
        private System.Windows.Forms.Label LocalKrystalsFolderInfoLabel;
        private System.Windows.Forms.Label LocalExpansionFieldsFolderInfoLabel;
        private System.Windows.Forms.Label LocalModulationOperatorsFolderInfoLabel;
        private System.Windows.Forms.Label LocalScoresRootFolderInfoLabel;
        private System.Windows.Forms.Label PreferencesFileLabel;
        private System.Windows.Forms.Label PreferencesFilePathLabel;
        private System.Windows.Forms.GroupBox LocalMoritzFoldersGroupBox;
        private System.Windows.Forms.Label LocalMoritzFolderInfoLabel;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Label OnlineMoritzFolderLabel;
        private System.Windows.Forms.Label OnlineMoritzFolderInfoLabel;
        private System.Windows.Forms.Label OnlineXMLSchemasFolderLabel;
        private System.Windows.Forms.Label OnlineXMLSchemasFolderInfoLabel;
        private System.Windows.Forms.Label OnlineMoritzAudioFolderLabel;
        private System.Windows.Forms.Label OnlineMoritzAudioFolderInfoLabel;
	}
}