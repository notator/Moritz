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
			this.PreferencesFileLabel = new System.Windows.Forms.Label();
			this.PreferencesFilePathLabel = new System.Windows.Forms.Label();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.LocalMoritzFolderHelpText = new System.Windows.Forms.Label();
			this.LocalScoresRootFolderInfoLabel = new System.Windows.Forms.Label();
			this.LocalAudioFolderInfoLabel = new System.Windows.Forms.Label();
			this.LocalModulationOperatorsFolderInfoLabel = new System.Windows.Forms.Label();
			this.LocalKrystalsFolderInfoLabel = new System.Windows.Forms.Label();
			this.LocalExpansionFieldsFolderInfoLabel = new System.Windows.Forms.Label();
			this.LocalMoritzFolderTextBox = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.LocalAudioFolderLabel = new System.Windows.Forms.Label();
			this.LocalKrystalsFolderLabel = new System.Windows.Forms.Label();
			this.LocalExpansionFieldsFolderLabel = new System.Windows.Forms.Label();
			this.LocalModulationOperatorsLabel = new System.Windows.Forms.Label();
			this.LocalMoritzFolderLabel = new System.Windows.Forms.Label();
			this.groupBox3 = new System.Windows.Forms.GroupBox();
			this.OnlineXMLSchemasFolderLabel = new System.Windows.Forms.Label();
			this.OnlineXMLSchemasFolderInfoLabel = new System.Windows.Forms.Label();
			this.MidiOutputDeviceLabel = new System.Windows.Forms.Label();
			this.OutputDevicesComboBox = new System.Windows.Forms.ComboBox();
			this.groupBox2.SuspendLayout();
			this.groupBox3.SuspendLayout();
			this.SuspendLayout();
			// 
			// OKBtn
			// 
			this.OKBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.OKBtn.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.OKBtn.Location = new System.Drawing.Point(273, 310);
			this.OKBtn.Name = "OKBtn";
			this.OKBtn.Size = new System.Drawing.Size(70, 23);
			this.OKBtn.TabIndex = 0;
			this.OKBtn.Text = "OK";
			this.OKBtn.UseVisualStyleBackColor = true;
			this.OKBtn.Click += new System.EventHandler(this.OKBtn_Click);
			// 
			// PreferencesFileLabel
			// 
			this.PreferencesFileLabel.AutoSize = true;
			this.PreferencesFileLabel.Location = new System.Drawing.Point(137, 22);
			this.PreferencesFileLabel.Name = "PreferencesFileLabel";
			this.PreferencesFileLabel.Size = new System.Drawing.Size(114, 13);
			this.PreferencesFileLabel.TabIndex = 36;
			this.PreferencesFileLabel.Text = "Moritz Preferences file:";
			// 
			// PreferencesFilePathLabel
			// 
			this.PreferencesFilePathLabel.AutoSize = true;
			this.PreferencesFilePathLabel.ForeColor = System.Drawing.Color.RoyalBlue;
			this.PreferencesFilePathLabel.Location = new System.Drawing.Point(249, 23);
			this.PreferencesFilePathLabel.Name = "PreferencesFilePathLabel";
			this.PreferencesFilePathLabel.Size = new System.Drawing.Size(293, 13);
			this.PreferencesFilePathLabel.TabIndex = 5;
			this.PreferencesFilePathLabel.Text = "C:\\User\\James\\AppData\\Roaming\\Moritz\\Preferences.mzpf";
			// 
			// groupBox2
			// 
			this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.groupBox2.Controls.Add(this.LocalMoritzFolderHelpText);
			this.groupBox2.Controls.Add(this.LocalScoresRootFolderInfoLabel);
			this.groupBox2.Controls.Add(this.LocalAudioFolderInfoLabel);
			this.groupBox2.Controls.Add(this.LocalModulationOperatorsFolderInfoLabel);
			this.groupBox2.Controls.Add(this.LocalKrystalsFolderInfoLabel);
			this.groupBox2.Controls.Add(this.LocalExpansionFieldsFolderInfoLabel);
			this.groupBox2.Controls.Add(this.LocalMoritzFolderTextBox);
			this.groupBox2.Controls.Add(this.PreferencesFilePathLabel);
			this.groupBox2.Controls.Add(this.label1);
			this.groupBox2.Controls.Add(this.LocalAudioFolderLabel);
			this.groupBox2.Controls.Add(this.LocalKrystalsFolderLabel);
			this.groupBox2.Controls.Add(this.LocalExpansionFieldsFolderLabel);
			this.groupBox2.Controls.Add(this.LocalModulationOperatorsLabel);
			this.groupBox2.Controls.Add(this.LocalMoritzFolderLabel);
			this.groupBox2.Controls.Add(this.PreferencesFileLabel);
			this.groupBox2.Location = new System.Drawing.Point(18, 38);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(581, 196);
			this.groupBox2.TabIndex = 1;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "Local files and folders";
			// 
			// LocalMoritzFolderHelpText
			// 
			this.LocalMoritzFolderHelpText.AutoSize = true;
			this.LocalMoritzFolderHelpText.ForeColor = System.Drawing.Color.RoyalBlue;
			this.LocalMoritzFolderHelpText.Location = new System.Drawing.Point(394, 47);
			this.LocalMoritzFolderHelpText.Name = "LocalMoritzFolderHelpText";
			this.LocalMoritzFolderHelpText.Size = new System.Drawing.Size(150, 13);
			this.LocalMoritzFolderHelpText.TabIndex = 53;
			this.LocalMoritzFolderHelpText.Text = "(e.g. C:\\Documents or D: etc.)";
			// 
			// LocalScoresRootFolderInfoLabel
			// 
			this.LocalScoresRootFolderInfoLabel.AutoSize = true;
			this.LocalScoresRootFolderInfoLabel.ForeColor = System.Drawing.Color.RoyalBlue;
			this.LocalScoresRootFolderInfoLabel.Location = new System.Drawing.Point(10, 164);
			this.LocalScoresRootFolderInfoLabel.Name = "LocalScoresRootFolderInfoLabel";
			this.LocalScoresRootFolderInfoLabel.Size = new System.Drawing.Size(567, 13);
			this.LocalScoresRootFolderInfoLabel.TabIndex = 7;
			this.LocalScoresRootFolderInfoLabel.Text = "MoritzFolderLocation\\Visual Studio\\Projects\\MyWebsite\\james-ingram-act-two\\open-s" +
    "ource\\assistantPerformer\\scores";
			// 
			// LocalAudioFolderInfoLabel
			// 
			this.LocalAudioFolderInfoLabel.AutoSize = true;
			this.LocalAudioFolderInfoLabel.ForeColor = System.Drawing.Color.RoyalBlue;
			this.LocalAudioFolderInfoLabel.Location = new System.Drawing.Point(249, 71);
			this.LocalAudioFolderInfoLabel.Name = "LocalAudioFolderInfoLabel";
			this.LocalAudioFolderInfoLabel.Size = new System.Drawing.Size(169, 13);
			this.LocalAudioFolderInfoLabel.TabIndex = 1;
			this.LocalAudioFolderInfoLabel.Text = "MoritzFolderLocation\\Moritz\\audio";
			// 
			// LocalModulationOperatorsFolderInfoLabel
			// 
			this.LocalModulationOperatorsFolderInfoLabel.AutoSize = true;
			this.LocalModulationOperatorsFolderInfoLabel.ForeColor = System.Drawing.Color.RoyalBlue;
			this.LocalModulationOperatorsFolderInfoLabel.Location = new System.Drawing.Point(249, 128);
			this.LocalModulationOperatorsFolderInfoLabel.Name = "LocalModulationOperatorsFolderInfoLabel";
			this.LocalModulationOperatorsFolderInfoLabel.Size = new System.Drawing.Size(281, 13);
			this.LocalModulationOperatorsFolderInfoLabel.TabIndex = 4;
			this.LocalModulationOperatorsFolderInfoLabel.Text = "MoritzFolderLocation\\Moritz\\krystals\\modulation operators";
			// 
			// LocalKrystalsFolderInfoLabel
			// 
			this.LocalKrystalsFolderInfoLabel.AutoSize = true;
			this.LocalKrystalsFolderInfoLabel.ForeColor = System.Drawing.Color.RoyalBlue;
			this.LocalKrystalsFolderInfoLabel.Location = new System.Drawing.Point(249, 90);
			this.LocalKrystalsFolderInfoLabel.Name = "LocalKrystalsFolderInfoLabel";
			this.LocalKrystalsFolderInfoLabel.Size = new System.Drawing.Size(178, 13);
			this.LocalKrystalsFolderInfoLabel.TabIndex = 2;
			this.LocalKrystalsFolderInfoLabel.Text = "MoritzFolderLocation\\Moritz\\krystals";
			// 
			// LocalExpansionFieldsFolderInfoLabel
			// 
			this.LocalExpansionFieldsFolderInfoLabel.AutoSize = true;
			this.LocalExpansionFieldsFolderInfoLabel.ForeColor = System.Drawing.Color.RoyalBlue;
			this.LocalExpansionFieldsFolderInfoLabel.Location = new System.Drawing.Point(249, 109);
			this.LocalExpansionFieldsFolderInfoLabel.Name = "LocalExpansionFieldsFolderInfoLabel";
			this.LocalExpansionFieldsFolderInfoLabel.Size = new System.Drawing.Size(278, 13);
			this.LocalExpansionFieldsFolderInfoLabel.TabIndex = 3;
			this.LocalExpansionFieldsFolderInfoLabel.Text = "MoritzFolderLocation\\Moritz\\krystals\\expansion operators";
			// 
			// LocalMoritzFolderTextBox
			// 
			this.LocalMoritzFolderTextBox.Location = new System.Drawing.Point(249, 43);
			this.LocalMoritzFolderTextBox.Name = "LocalMoritzFolderTextBox";
			this.LocalMoritzFolderTextBox.Size = new System.Drawing.Size(143, 20);
			this.LocalMoritzFolderTextBox.TabIndex = 0;
			this.LocalMoritzFolderTextBox.Text = "C:\\Documents";
			this.LocalMoritzFolderTextBox.Enter += new System.EventHandler(this.LocalMoritzFolderTextBox_Enter);
			this.LocalMoritzFolderTextBox.Leave += new System.EventHandler(this.LocalMoritzFolderTextBox_Leave);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(145, 146);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(106, 13);
			this.label1.TabIndex = 52;
			this.label1.Text = "score creation folder:";
			// 
			// LocalAudioFolderLabel
			// 
			this.LocalAudioFolderLabel.AutoSize = true;
			this.LocalAudioFolderLabel.Location = new System.Drawing.Point(186, 70);
			this.LocalAudioFolderLabel.Name = "LocalAudioFolderLabel";
			this.LocalAudioFolderLabel.Size = new System.Drawing.Size(65, 13);
			this.LocalAudioFolderLabel.TabIndex = 50;
			this.LocalAudioFolderLabel.Text = "audio folder:";
			// 
			// LocalKrystalsFolderLabel
			// 
			this.LocalKrystalsFolderLabel.AutoSize = true;
			this.LocalKrystalsFolderLabel.Location = new System.Drawing.Point(177, 89);
			this.LocalKrystalsFolderLabel.Name = "LocalKrystalsFolderLabel";
			this.LocalKrystalsFolderLabel.Size = new System.Drawing.Size(74, 13);
			this.LocalKrystalsFolderLabel.TabIndex = 44;
			this.LocalKrystalsFolderLabel.Text = "krystals folder:";
			// 
			// LocalExpansionFieldsFolderLabel
			// 
			this.LocalExpansionFieldsFolderLabel.AutoSize = true;
			this.LocalExpansionFieldsFolderLabel.Location = new System.Drawing.Point(137, 108);
			this.LocalExpansionFieldsFolderLabel.Name = "LocalExpansionFieldsFolderLabel";
			this.LocalExpansionFieldsFolderLabel.Size = new System.Drawing.Size(114, 13);
			this.LocalExpansionFieldsFolderLabel.TabIndex = 45;
			this.LocalExpansionFieldsFolderLabel.Text = "expansion fields folder:";
			// 
			// LocalModulationOperatorsLabel
			// 
			this.LocalModulationOperatorsLabel.AutoSize = true;
			this.LocalModulationOperatorsLabel.Location = new System.Drawing.Point(114, 127);
			this.LocalModulationOperatorsLabel.Name = "LocalModulationOperatorsLabel";
			this.LocalModulationOperatorsLabel.Size = new System.Drawing.Size(137, 13);
			this.LocalModulationOperatorsLabel.TabIndex = 46;
			this.LocalModulationOperatorsLabel.Text = "modulation operators folder:";
			// 
			// LocalMoritzFolderLabel
			// 
			this.LocalMoritzFolderLabel.AutoSize = true;
			this.LocalMoritzFolderLabel.Location = new System.Drawing.Point(57, 46);
			this.LocalMoritzFolderLabel.Name = "LocalMoritzFolderLabel";
			this.LocalMoritzFolderLabel.Size = new System.Drawing.Size(194, 13);
			this.LocalMoritzFolderLabel.TabIndex = 41;
			this.LocalMoritzFolderLabel.Text = "User\'s Moritz documents folder location:";
			// 
			// groupBox3
			// 
			this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.groupBox3.Controls.Add(this.OnlineXMLSchemasFolderLabel);
			this.groupBox3.Controls.Add(this.OnlineXMLSchemasFolderInfoLabel);
			this.groupBox3.Location = new System.Drawing.Point(74, 242);
			this.groupBox3.Name = "groupBox3";
			this.groupBox3.Size = new System.Drawing.Size(469, 56);
			this.groupBox3.TabIndex = 3;
			this.groupBox3.TabStop = false;
			this.groupBox3.Text = "Online Folders";
			// 
			// OnlineXMLSchemasFolderLabel
			// 
			this.OnlineXMLSchemasFolderLabel.AutoSize = true;
			this.OnlineXMLSchemasFolderLabel.Location = new System.Drawing.Point(25, 22);
			this.OnlineXMLSchemasFolderLabel.Name = "OnlineXMLSchemasFolderLabel";
			this.OnlineXMLSchemasFolderLabel.Size = new System.Drawing.Size(141, 13);
			this.OnlineXMLSchemasFolderLabel.TabIndex = 38;
			this.OnlineXMLSchemasFolderLabel.Text = "Online XML Schemas folder:";
			// 
			// OnlineXMLSchemasFolderInfoLabel
			// 
			this.OnlineXMLSchemasFolderInfoLabel.AutoSize = true;
			this.OnlineXMLSchemasFolderInfoLabel.ForeColor = System.Drawing.Color.RoyalBlue;
			this.OnlineXMLSchemasFolderInfoLabel.Location = new System.Drawing.Point(163, 22);
			this.OnlineXMLSchemasFolderInfoLabel.Name = "OnlineXMLSchemasFolderInfoLabel";
			this.OnlineXMLSchemasFolderInfoLabel.Size = new System.Drawing.Size(287, 13);
			this.OnlineXMLSchemasFolderInfoLabel.TabIndex = 2;
			this.OnlineXMLSchemasFolderInfoLabel.Text = "http://james-ingram-act-two.de/open-source/XMLSchemas";
			// 
			// MidiOutputDeviceLabel
			// 
			this.MidiOutputDeviceLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.MidiOutputDeviceLabel.AutoSize = true;
			this.MidiOutputDeviceLabel.Location = new System.Drawing.Point(121, 17);
			this.MidiOutputDeviceLabel.Name = "MidiOutputDeviceLabel";
			this.MidiOutputDeviceLabel.Size = new System.Drawing.Size(147, 13);
			this.MidiOutputDeviceLabel.TabIndex = 4;
			this.MidiOutputDeviceLabel.Text = "Preferred MIDI output device:";
			// 
			// OutputDevicesComboBox
			// 
			this.OutputDevicesComboBox.FormattingEnabled = true;
			this.OutputDevicesComboBox.Location = new System.Drawing.Point(267, 14);
			this.OutputDevicesComboBox.Name = "OutputDevicesComboBox";
			this.OutputDevicesComboBox.Size = new System.Drawing.Size(171, 21);
			this.OutputDevicesComboBox.TabIndex = 2;
			// 
			// PreferencesDialog
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(248)))), ((int)(((byte)(248)))));
			this.ClientSize = new System.Drawing.Size(618, 347);
			this.ControlBox = false;
			this.Controls.Add(this.OutputDevicesComboBox);
			this.Controls.Add(this.MidiOutputDeviceLabel);
			this.Controls.Add(this.groupBox3);
			this.Controls.Add(this.groupBox2);
			this.Controls.Add(this.OKBtn);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "PreferencesDialog";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Moritz Preferences";
			this.groupBox2.ResumeLayout(false);
			this.groupBox2.PerformLayout();
			this.groupBox3.ResumeLayout(false);
			this.groupBox3.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button OKBtn;
        private System.Windows.Forms.Label PreferencesFileLabel;
		private System.Windows.Forms.Label PreferencesFilePathLabel;
		private System.Windows.Forms.GroupBox groupBox2;
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
		private System.Windows.Forms.Label LocalMoritzFolderHelpText;
		private System.Windows.Forms.GroupBox groupBox3;
		private System.Windows.Forms.Label OnlineXMLSchemasFolderLabel;
		private System.Windows.Forms.Label OnlineXMLSchemasFolderInfoLabel;
		private System.Windows.Forms.Label MidiOutputDeviceLabel;
		private System.Windows.Forms.ComboBox OutputDevicesComboBox;
	}
}