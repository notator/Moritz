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
			this.DevicesGroupBox = new System.Windows.Forms.GroupBox();
			this.label6 = new System.Windows.Forms.Label();
			this.OutputDevicesComboBox = new System.Windows.Forms.ComboBox();
			this.InputDevicesComboBox = new System.Windows.Forms.ComboBox();
			this.PreferredOutputDeviceLabel = new System.Windows.Forms.Label();
			this.PreferrredInputDeviceLabel = new System.Windows.Forms.Label();
			this.PreferencesFileLabel = new System.Windows.Forms.Label();
			this.PreferencesFilePathLabel = new System.Windows.Forms.Label();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.LocalScoresRootFolderHelpLabel = new System.Windows.Forms.Label();
			this.LocalScoresRootFolderInfoLabel = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.LocalAudioFolderLabel = new System.Windows.Forms.Label();
			this.LocalAudioFolderInfoLabel = new System.Windows.Forms.Label();
			this.LocalKrystalsFolderLabel = new System.Windows.Forms.Label();
			this.LocalExpansionFieldsFolderLabel = new System.Windows.Forms.Label();
			this.LocalModulationOperatorsLabel = new System.Windows.Forms.Label();
			this.LocalModulationOperatorsFolderInfoLabel = new System.Windows.Forms.Label();
			this.LocalKrystalsFolderInfoLabel = new System.Windows.Forms.Label();
			this.LocalExpansionFieldsFolderInfoLabel = new System.Windows.Forms.Label();
			this.LocalMoritzFolderTextBox = new System.Windows.Forms.TextBox();
			this.groupBox3 = new System.Windows.Forms.GroupBox();
			this.OnlineXMLSchemasFolderLabel = new System.Windows.Forms.Label();
			this.OnlineXMLSchemasFolderInfoLabel = new System.Windows.Forms.Label();
			this.OnlineMoritzAudioFolderLabel = new System.Windows.Forms.Label();
			this.OnlineMoritzAudioFolderInfoLabel = new System.Windows.Forms.Label();
			this.OnlineMoritzFolderLabel = new System.Windows.Forms.Label();
			this.OnlineMoritzFolderInfoLabel = new System.Windows.Forms.Label();
			this.LocalMoritzFolderLabel = new System.Windows.Forms.Label();
			this.LocalMoritzFolderHelpText = new System.Windows.Forms.Label();
			this.DevicesGroupBox.SuspendLayout();
			this.groupBox2.SuspendLayout();
			this.groupBox3.SuspendLayout();
			this.SuspendLayout();
			// 
			// OKBtn
			// 
			this.OKBtn.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.OKBtn.Location = new System.Drawing.Point(345, 450);
			this.OKBtn.Name = "OKBtn";
			this.OKBtn.Size = new System.Drawing.Size(70, 23);
			this.OKBtn.TabIndex = 0;
			this.OKBtn.Text = "OK";
			this.OKBtn.UseVisualStyleBackColor = true;
			this.OKBtn.Click += new System.EventHandler(this.OKBtn_Click);
			// 
			// DevicesGroupBox
			// 
			this.DevicesGroupBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(248)))), ((int)(((byte)(248)))));
			this.DevicesGroupBox.Controls.Add(this.label6);
			this.DevicesGroupBox.Controls.Add(this.OutputDevicesComboBox);
			this.DevicesGroupBox.Controls.Add(this.InputDevicesComboBox);
			this.DevicesGroupBox.Controls.Add(this.PreferredOutputDeviceLabel);
			this.DevicesGroupBox.Controls.Add(this.PreferrredInputDeviceLabel);
			this.DevicesGroupBox.Location = new System.Drawing.Point(167, 18);
			this.DevicesGroupBox.Name = "DevicesGroupBox";
			this.DevicesGroupBox.Size = new System.Drawing.Size(427, 109);
			this.DevicesGroupBox.TabIndex = 1;
			this.DevicesGroupBox.TabStop = false;
			this.DevicesGroupBox.Text = "MIDI Devices";
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.ForeColor = System.Drawing.Color.RoyalBlue;
			this.label6.Location = new System.Drawing.Point(36, 62);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(355, 39);
			this.label6.TabIndex = 43;
			this.label6.Text = "These selectors contain the currently active devices.\r\nThe values set here will b" +
    "e used as the defaults in Moritz\' device selectors\r\nwhen Moritz is restarted.\r\n";
			this.label6.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			// 
			// OutputDevicesComboBox
			// 
			this.OutputDevicesComboBox.FormattingEnabled = true;
			this.OutputDevicesComboBox.Location = new System.Drawing.Point(222, 38);
			this.OutputDevicesComboBox.Name = "OutputDevicesComboBox";
			this.OutputDevicesComboBox.Size = new System.Drawing.Size(171, 21);
			this.OutputDevicesComboBox.TabIndex = 1;
			// 
			// InputDevicesComboBox
			// 
			this.InputDevicesComboBox.FormattingEnabled = true;
			this.InputDevicesComboBox.Location = new System.Drawing.Point(36, 38);
			this.InputDevicesComboBox.Name = "InputDevicesComboBox";
			this.InputDevicesComboBox.Size = new System.Drawing.Size(171, 21);
			this.InputDevicesComboBox.TabIndex = 0;
			// 
			// PreferredOutputDeviceLabel
			// 
			this.PreferredOutputDeviceLabel.AutoSize = true;
			this.PreferredOutputDeviceLabel.Location = new System.Drawing.Point(222, 22);
			this.PreferredOutputDeviceLabel.Name = "PreferredOutputDeviceLabel";
			this.PreferredOutputDeviceLabel.Size = new System.Drawing.Size(125, 13);
			this.PreferredOutputDeviceLabel.TabIndex = 13;
			this.PreferredOutputDeviceLabel.Text = "Preferred Output Device:";
			// 
			// PreferrredInputDeviceLabel
			// 
			this.PreferrredInputDeviceLabel.AutoSize = true;
			this.PreferrredInputDeviceLabel.Location = new System.Drawing.Point(36, 22);
			this.PreferrredInputDeviceLabel.Name = "PreferrredInputDeviceLabel";
			this.PreferrredInputDeviceLabel.Size = new System.Drawing.Size(117, 13);
			this.PreferrredInputDeviceLabel.TabIndex = 9;
			this.PreferrredInputDeviceLabel.Text = "Preferred Input Device:";
			// 
			// PreferencesFileLabel
			// 
			this.PreferencesFileLabel.AutoSize = true;
			this.PreferencesFileLabel.Location = new System.Drawing.Point(177, 23);
			this.PreferencesFileLabel.Name = "PreferencesFileLabel";
			this.PreferencesFileLabel.Size = new System.Drawing.Size(143, 13);
			this.PreferencesFileLabel.TabIndex = 36;
			this.PreferencesFileLabel.Text = "Local Moritz Preferences file:";
			// 
			// PreferencesFilePathLabel
			// 
			this.PreferencesFilePathLabel.AutoSize = true;
			this.PreferencesFilePathLabel.ForeColor = System.Drawing.Color.RoyalBlue;
			this.PreferencesFilePathLabel.Location = new System.Drawing.Point(317, 23);
			this.PreferencesFilePathLabel.Name = "PreferencesFilePathLabel";
			this.PreferencesFilePathLabel.Size = new System.Drawing.Size(293, 13);
			this.PreferencesFilePathLabel.TabIndex = 0;
			this.PreferencesFilePathLabel.Text = "C:\\User\\James\\AppData\\Roaming\\Moritz\\Preferences.mzpf";
			// 
			// groupBox2
			// 
			this.groupBox2.Controls.Add(this.LocalMoritzFolderHelpText);
			this.groupBox2.Controls.Add(this.LocalScoresRootFolderHelpLabel);
			this.groupBox2.Controls.Add(this.LocalScoresRootFolderInfoLabel);
			this.groupBox2.Controls.Add(this.label1);
			this.groupBox2.Controls.Add(this.LocalAudioFolderLabel);
			this.groupBox2.Controls.Add(this.LocalAudioFolderInfoLabel);
			this.groupBox2.Controls.Add(this.LocalKrystalsFolderLabel);
			this.groupBox2.Controls.Add(this.LocalExpansionFieldsFolderLabel);
			this.groupBox2.Controls.Add(this.LocalModulationOperatorsLabel);
			this.groupBox2.Controls.Add(this.LocalModulationOperatorsFolderInfoLabel);
			this.groupBox2.Controls.Add(this.LocalKrystalsFolderInfoLabel);
			this.groupBox2.Controls.Add(this.LocalExpansionFieldsFolderInfoLabel);
			this.groupBox2.Controls.Add(this.LocalMoritzFolderTextBox);
			this.groupBox2.Controls.Add(this.groupBox3);
			this.groupBox2.Controls.Add(this.LocalMoritzFolderLabel);
			this.groupBox2.Controls.Add(this.PreferencesFilePathLabel);
			this.groupBox2.Controls.Add(this.PreferencesFileLabel);
			this.groupBox2.Location = new System.Drawing.Point(18, 134);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(725, 303);
			this.groupBox2.TabIndex = 2;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "Environment files and folders";
			// 
			// LocalScoresRootFolderHelpLabel
			// 
			this.LocalScoresRootFolderHelpLabel.AutoSize = true;
			this.LocalScoresRootFolderHelpLabel.ForeColor = System.Drawing.Color.RoyalBlue;
			this.LocalScoresRootFolderHelpLabel.Location = new System.Drawing.Point(317, 147);
			this.LocalScoresRootFolderHelpLabel.Name = "LocalScoresRootFolderHelpLabel";
			this.LocalScoresRootFolderHelpLabel.Size = new System.Drawing.Size(218, 13);
			this.LocalScoresRootFolderHelpLabel.TabIndex = 6;
			this.LocalScoresRootFolderHelpLabel.Text = "(The root folder in which scores are created.)";
			// 
			// LocalScoresRootFolderInfoLabel
			// 
			this.LocalScoresRootFolderInfoLabel.AutoSize = true;
			this.LocalScoresRootFolderInfoLabel.ForeColor = System.Drawing.Color.RoyalBlue;
			this.LocalScoresRootFolderInfoLabel.Location = new System.Drawing.Point(20, 164);
			this.LocalScoresRootFolderInfoLabel.Name = "LocalScoresRootFolderInfoLabel";
			this.LocalScoresRootFolderInfoLabel.Size = new System.Drawing.Size(704, 13);
			this.LocalScoresRootFolderInfoLabel.TabIndex = 7;
			this.LocalScoresRootFolderInfoLabel.Text = "MoritzFolderLocation\\Visual Studio\\Projects\\Web Development\\Projects\\MyWebsite\\ja" +
    "mes-ingram-act-two\\open-source\\assistantPerformer\\scores";
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(200, 147);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(120, 13);
			this.label1.TabIndex = 52;
			this.label1.Text = "Local scores root folder:";
			// 
			// LocalAudioFolderLabel
			// 
			this.LocalAudioFolderLabel.AutoSize = true;
			this.LocalAudioFolderLabel.Location = new System.Drawing.Point(226, 71);
			this.LocalAudioFolderLabel.Name = "LocalAudioFolderLabel";
			this.LocalAudioFolderLabel.Size = new System.Drawing.Size(94, 13);
			this.LocalAudioFolderLabel.TabIndex = 50;
			this.LocalAudioFolderLabel.Text = "Local audio folder:";
			// 
			// LocalAudioFolderInfoLabel
			// 
			this.LocalAudioFolderInfoLabel.AutoSize = true;
			this.LocalAudioFolderInfoLabel.ForeColor = System.Drawing.Color.RoyalBlue;
			this.LocalAudioFolderInfoLabel.Location = new System.Drawing.Point(317, 71);
			this.LocalAudioFolderInfoLabel.Name = "LocalAudioFolderInfoLabel";
			this.LocalAudioFolderInfoLabel.Size = new System.Drawing.Size(169, 13);
			this.LocalAudioFolderInfoLabel.TabIndex = 2;
			this.LocalAudioFolderInfoLabel.Text = "MoritzFolderLocation\\Moritz\\audio";
			// 
			// LocalKrystalsFolderLabel
			// 
			this.LocalKrystalsFolderLabel.AutoSize = true;
			this.LocalKrystalsFolderLabel.Location = new System.Drawing.Point(217, 90);
			this.LocalKrystalsFolderLabel.Name = "LocalKrystalsFolderLabel";
			this.LocalKrystalsFolderLabel.Size = new System.Drawing.Size(103, 13);
			this.LocalKrystalsFolderLabel.TabIndex = 44;
			this.LocalKrystalsFolderLabel.Text = "Local krystals folder:";
			// 
			// LocalExpansionFieldsFolderLabel
			// 
			this.LocalExpansionFieldsFolderLabel.AutoSize = true;
			this.LocalExpansionFieldsFolderLabel.Location = new System.Drawing.Point(177, 109);
			this.LocalExpansionFieldsFolderLabel.Name = "LocalExpansionFieldsFolderLabel";
			this.LocalExpansionFieldsFolderLabel.Size = new System.Drawing.Size(143, 13);
			this.LocalExpansionFieldsFolderLabel.TabIndex = 45;
			this.LocalExpansionFieldsFolderLabel.Text = "Local expansion fields folder:";
			// 
			// LocalModulationOperatorsLabel
			// 
			this.LocalModulationOperatorsLabel.AutoSize = true;
			this.LocalModulationOperatorsLabel.Location = new System.Drawing.Point(154, 128);
			this.LocalModulationOperatorsLabel.Name = "LocalModulationOperatorsLabel";
			this.LocalModulationOperatorsLabel.Size = new System.Drawing.Size(166, 13);
			this.LocalModulationOperatorsLabel.TabIndex = 46;
			this.LocalModulationOperatorsLabel.Text = "Local modulation operators folder:";
			// 
			// LocalModulationOperatorsFolderInfoLabel
			// 
			this.LocalModulationOperatorsFolderInfoLabel.AutoSize = true;
			this.LocalModulationOperatorsFolderInfoLabel.ForeColor = System.Drawing.Color.RoyalBlue;
			this.LocalModulationOperatorsFolderInfoLabel.Location = new System.Drawing.Point(317, 128);
			this.LocalModulationOperatorsFolderInfoLabel.Name = "LocalModulationOperatorsFolderInfoLabel";
			this.LocalModulationOperatorsFolderInfoLabel.Size = new System.Drawing.Size(281, 13);
			this.LocalModulationOperatorsFolderInfoLabel.TabIndex = 5;
			this.LocalModulationOperatorsFolderInfoLabel.Text = "MoritzFolderLocation\\Moritz\\krystals\\modulation operators";
			// 
			// LocalKrystalsFolderInfoLabel
			// 
			this.LocalKrystalsFolderInfoLabel.AutoSize = true;
			this.LocalKrystalsFolderInfoLabel.ForeColor = System.Drawing.Color.RoyalBlue;
			this.LocalKrystalsFolderInfoLabel.Location = new System.Drawing.Point(317, 90);
			this.LocalKrystalsFolderInfoLabel.Name = "LocalKrystalsFolderInfoLabel";
			this.LocalKrystalsFolderInfoLabel.Size = new System.Drawing.Size(178, 13);
			this.LocalKrystalsFolderInfoLabel.TabIndex = 3;
			this.LocalKrystalsFolderInfoLabel.Text = "MoritzFolderLocation\\Moritz\\krystals";
			// 
			// LocalExpansionFieldsFolderInfoLabel
			// 
			this.LocalExpansionFieldsFolderInfoLabel.AutoSize = true;
			this.LocalExpansionFieldsFolderInfoLabel.ForeColor = System.Drawing.Color.RoyalBlue;
			this.LocalExpansionFieldsFolderInfoLabel.Location = new System.Drawing.Point(317, 109);
			this.LocalExpansionFieldsFolderInfoLabel.Name = "LocalExpansionFieldsFolderInfoLabel";
			this.LocalExpansionFieldsFolderInfoLabel.Size = new System.Drawing.Size(278, 13);
			this.LocalExpansionFieldsFolderInfoLabel.TabIndex = 4;
			this.LocalExpansionFieldsFolderInfoLabel.Text = "MoritzFolderLocation\\Moritz\\krystals\\expansion operators";
			// 
			// LocalMoritzFolderTextBox
			// 
			this.LocalMoritzFolderTextBox.Location = new System.Drawing.Point(317, 44);
			this.LocalMoritzFolderTextBox.Name = "LocalMoritzFolderTextBox";
			this.LocalMoritzFolderTextBox.Size = new System.Drawing.Size(143, 20);
			this.LocalMoritzFolderTextBox.TabIndex = 1;
			this.LocalMoritzFolderTextBox.Text = "C:\\Documents";
			this.LocalMoritzFolderTextBox.Enter += new System.EventHandler(this.LocalMoritzFolderTextBox_Enter);
			this.LocalMoritzFolderTextBox.Leave += new System.EventHandler(this.LocalMoritzFolderTextBox_Leave);
			// 
			// groupBox3
			// 
			this.groupBox3.Controls.Add(this.OnlineXMLSchemasFolderLabel);
			this.groupBox3.Controls.Add(this.OnlineXMLSchemasFolderInfoLabel);
			this.groupBox3.Controls.Add(this.OnlineMoritzAudioFolderLabel);
			this.groupBox3.Controls.Add(this.OnlineMoritzAudioFolderInfoLabel);
			this.groupBox3.Controls.Add(this.OnlineMoritzFolderLabel);
			this.groupBox3.Controls.Add(this.OnlineMoritzFolderInfoLabel);
			this.groupBox3.Location = new System.Drawing.Point(94, 190);
			this.groupBox3.Name = "groupBox3";
			this.groupBox3.Size = new System.Drawing.Size(498, 94);
			this.groupBox3.TabIndex = 8;
			this.groupBox3.TabStop = false;
			this.groupBox3.Text = "Online Folders";
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
			this.OnlineXMLSchemasFolderInfoLabel.TabIndex = 2;
			this.OnlineXMLSchemasFolderInfoLabel.Text = "http://james-ingram-act-two.de/open-source/XMLSchemas";
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
			this.OnlineMoritzAudioFolderInfoLabel.TabIndex = 1;
			this.OnlineMoritzAudioFolderInfoLabel.Text = "http://james-ingram-act-two.de/open-source/Moritz/audio";
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
			this.OnlineMoritzFolderInfoLabel.TabIndex = 0;
			this.OnlineMoritzFolderInfoLabel.Text = "http://james-ingram-act-two.de/open-source/Moritz";
			// 
			// LocalMoritzFolderLabel
			// 
			this.LocalMoritzFolderLabel.AutoSize = true;
			this.LocalMoritzFolderLabel.Location = new System.Drawing.Point(184, 47);
			this.LocalMoritzFolderLabel.Name = "LocalMoritzFolderLabel";
			this.LocalMoritzFolderLabel.Size = new System.Drawing.Size(136, 13);
			this.LocalMoritzFolderLabel.TabIndex = 41;
			this.LocalMoritzFolderLabel.Text = "Local Moritz folder location:";
			// 
			// LocalMoritzFolderHelpText
			// 
			this.LocalMoritzFolderHelpText.AutoSize = true;
			this.LocalMoritzFolderHelpText.ForeColor = System.Drawing.Color.RoyalBlue;
			this.LocalMoritzFolderHelpText.Location = new System.Drawing.Point(462, 47);
			this.LocalMoritzFolderHelpText.Name = "LocalMoritzFolderHelpText";
			this.LocalMoritzFolderHelpText.Size = new System.Drawing.Size(150, 13);
			this.LocalMoritzFolderHelpText.TabIndex = 53;
			this.LocalMoritzFolderHelpText.Text = "(e.g. C:\\Documents or D: etc.)";
			// 
			// PreferencesDialog
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(248)))), ((int)(((byte)(248)))));
			this.ClientSize = new System.Drawing.Size(761, 488);
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
			this.groupBox2.ResumeLayout(false);
			this.groupBox2.PerformLayout();
			this.groupBox3.ResumeLayout(false);
			this.groupBox3.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Button OKBtn;
		private System.Windows.Forms.GroupBox DevicesGroupBox;
		private System.Windows.Forms.Label PreferrredInputDeviceLabel;
		private System.Windows.Forms.Label PreferredOutputDeviceLabel;
        private System.Windows.Forms.Label PreferencesFileLabel;
		private System.Windows.Forms.Label PreferencesFilePathLabel;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Label OnlineMoritzFolderLabel;
        private System.Windows.Forms.Label OnlineMoritzFolderInfoLabel;
        private System.Windows.Forms.Label OnlineXMLSchemasFolderLabel;
        private System.Windows.Forms.Label OnlineXMLSchemasFolderInfoLabel;
        private System.Windows.Forms.Label OnlineMoritzAudioFolderLabel;
		private System.Windows.Forms.Label OnlineMoritzAudioFolderInfoLabel;
		private System.Windows.Forms.ComboBox InputDevicesComboBox;
		private System.Windows.Forms.ComboBox OutputDevicesComboBox;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.Label LocalMoritzFolderLabel;
		private System.Windows.Forms.TextBox LocalMoritzFolderTextBox;
		private System.Windows.Forms.Label LocalScoresRootFolderInfoLabel;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label LocalAudioFolderLabel;
		private System.Windows.Forms.Label LocalAudioFolderInfoLabel;
		private System.Windows.Forms.Label LocalKrystalsFolderLabel;
		private System.Windows.Forms.Label LocalExpansionFieldsFolderLabel;
		private System.Windows.Forms.Label LocalModulationOperatorsLabel;
		private System.Windows.Forms.Label LocalModulationOperatorsFolderInfoLabel;
		private System.Windows.Forms.Label LocalKrystalsFolderInfoLabel;
		private System.Windows.Forms.Label LocalExpansionFieldsFolderInfoLabel;
		private System.Windows.Forms.Label LocalScoresRootFolderHelpLabel;
		private System.Windows.Forms.Label LocalMoritzFolderHelpText;
	}
}