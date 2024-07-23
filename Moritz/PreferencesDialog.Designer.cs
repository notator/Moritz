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
            this.PreferencesFilePathInfoLabel = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.ScoresRootFolderInfoLabel = new System.Windows.Forms.Label();
            this.AudioFolderInfoLabel = new System.Windows.Forms.Label();
            this.ModulationOperatorsFolderInfoLabel = new System.Windows.Forms.Label();
            this.KrystalsFolderInfoLabel = new System.Windows.Forms.Label();
            this.ExpansionFieldsFolderInfoLabel = new System.Windows.Forms.Label();
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
            this.MoritzFolderLocationInfoLabel = new System.Windows.Forms.Label();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // OKBtn
            // 
            this.OKBtn.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.OKBtn.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.OKBtn.Location = new System.Drawing.Point(273, 279);
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
            this.PreferencesFileLabel.Location = new System.Drawing.Point(137, 46);
            this.PreferencesFileLabel.Name = "PreferencesFileLabel";
            this.PreferencesFileLabel.Size = new System.Drawing.Size(114, 13);
            this.PreferencesFileLabel.TabIndex = 36;
            this.PreferencesFileLabel.Text = "Moritz Preferences file:";
            // 
            // PreferencesFilePathLabel
            // 
            this.PreferencesFilePathInfoLabel.AutoSize = true;
            this.PreferencesFilePathInfoLabel.ForeColor = System.Drawing.Color.RoyalBlue;
            this.PreferencesFilePathInfoLabel.Location = new System.Drawing.Point(249, 46);
            this.PreferencesFilePathInfoLabel.Name = "PreferencesFilePathLabel";
            this.PreferencesFilePathInfoLabel.Size = new System.Drawing.Size(192, 13);
            this.PreferencesFilePathInfoLabel.TabIndex = 5;
            this.PreferencesFilePathInfoLabel.Text = "MoritzFolderLocation\\Preferences.mzpf";
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.groupBox2.Controls.Add(this.MoritzFolderLocationInfoLabel);
            this.groupBox2.Controls.Add(this.ScoresRootFolderInfoLabel);
            this.groupBox2.Controls.Add(this.AudioFolderInfoLabel);
            this.groupBox2.Controls.Add(this.ModulationOperatorsFolderInfoLabel);
            this.groupBox2.Controls.Add(this.KrystalsFolderInfoLabel);
            this.groupBox2.Controls.Add(this.ExpansionFieldsFolderInfoLabel);
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Controls.Add(this.LocalAudioFolderLabel);
            this.groupBox2.Controls.Add(this.LocalKrystalsFolderLabel);
            this.groupBox2.Controls.Add(this.LocalExpansionFieldsFolderLabel);
            this.groupBox2.Controls.Add(this.LocalModulationOperatorsLabel);
            this.groupBox2.Controls.Add(this.PreferencesFilePathInfoLabel);
            this.groupBox2.Controls.Add(this.LocalMoritzFolderLabel);
            this.groupBox2.Controls.Add(this.PreferencesFileLabel);
            this.groupBox2.Location = new System.Drawing.Point(18, 38);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(581, 173);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Local files and folders";
            // 
            // LocalScoresRootFolderInfoLabel
            // 
            this.ScoresRootFolderInfoLabel.AutoSize = true;
            this.ScoresRootFolderInfoLabel.ForeColor = System.Drawing.Color.RoyalBlue;
            this.ScoresRootFolderInfoLabel.Location = new System.Drawing.Point(249, 146);
            this.ScoresRootFolderInfoLabel.Name = "LocalScoresRootFolderInfoLabel";
            this.ScoresRootFolderInfoLabel.Size = new System.Drawing.Size(141, 13);
            this.ScoresRootFolderInfoLabel.TabIndex = 7;
            this.ScoresRootFolderInfoLabel.Text = "MoritzFolderLocation\\scores";
            // 
            // LocalAudioFolderInfoLabel
            // 
            this.AudioFolderInfoLabel.AutoSize = true;
            this.AudioFolderInfoLabel.ForeColor = System.Drawing.Color.RoyalBlue;
            this.AudioFolderInfoLabel.Location = new System.Drawing.Point(249, 71);
            this.AudioFolderInfoLabel.Name = "LocalAudioFolderInfoLabel";
            this.AudioFolderInfoLabel.Size = new System.Drawing.Size(136, 13);
            this.AudioFolderInfoLabel.TabIndex = 1;
            this.AudioFolderInfoLabel.Text = "MoritzFolderLocation\\audio";
            // 
            // LocalModulationOperatorsFolderInfoLabel
            // 
            this.ModulationOperatorsFolderInfoLabel.AutoSize = true;
            this.ModulationOperatorsFolderInfoLabel.ForeColor = System.Drawing.Color.RoyalBlue;
            this.ModulationOperatorsFolderInfoLabel.Location = new System.Drawing.Point(249, 128);
            this.ModulationOperatorsFolderInfoLabel.Name = "LocalModulationOperatorsFolderInfoLabel";
            this.ModulationOperatorsFolderInfoLabel.Size = new System.Drawing.Size(248, 13);
            this.ModulationOperatorsFolderInfoLabel.TabIndex = 4;
            this.ModulationOperatorsFolderInfoLabel.Text = "MoritzFolderLocation\\krystals\\modulation operators";
            // 
            // LocalKrystalsFolderInfoLabel
            // 
            this.KrystalsFolderInfoLabel.AutoSize = true;
            this.KrystalsFolderInfoLabel.ForeColor = System.Drawing.Color.RoyalBlue;
            this.KrystalsFolderInfoLabel.Location = new System.Drawing.Point(249, 90);
            this.KrystalsFolderInfoLabel.Name = "LocalKrystalsFolderInfoLabel";
            this.KrystalsFolderInfoLabel.Size = new System.Drawing.Size(185, 13);
            this.KrystalsFolderInfoLabel.TabIndex = 2;
            this.KrystalsFolderInfoLabel.Text = "MoritzFolderLocation\\krystals\\krystals";
            // 
            // LocalExpansionFieldsFolderInfoLabel
            // 
            this.ExpansionFieldsFolderInfoLabel.AutoSize = true;
            this.ExpansionFieldsFolderInfoLabel.ForeColor = System.Drawing.Color.RoyalBlue;
            this.ExpansionFieldsFolderInfoLabel.Location = new System.Drawing.Point(249, 109);
            this.ExpansionFieldsFolderInfoLabel.Name = "LocalExpansionFieldsFolderInfoLabel";
            this.ExpansionFieldsFolderInfoLabel.Size = new System.Drawing.Size(245, 13);
            this.ExpansionFieldsFolderInfoLabel.TabIndex = 3;
            this.ExpansionFieldsFolderInfoLabel.Text = "MoritzFolderLocation\\krystals\\expansion operators";
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
            this.LocalMoritzFolderLabel.Location = new System.Drawing.Point(89, 22);
            this.LocalMoritzFolderLabel.Name = "LocalMoritzFolderLabel";
            this.LocalMoritzFolderLabel.Size = new System.Drawing.Size(162, 13);
            this.LocalMoritzFolderLabel.TabIndex = 41;
            this.LocalMoritzFolderLabel.Text = "Moritz documents folder location:";
            // 
            // groupBox3
            // 
            this.groupBox3.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.groupBox3.Controls.Add(this.OnlineXMLSchemasFolderLabel);
            this.groupBox3.Controls.Add(this.OnlineXMLSchemasFolderInfoLabel);
            this.groupBox3.Location = new System.Drawing.Point(74, 211);
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
            this.OnlineXMLSchemasFolderInfoLabel.Size = new System.Drawing.Size(292, 13);
            this.OnlineXMLSchemasFolderInfoLabel.TabIndex = 2;
            this.OnlineXMLSchemasFolderInfoLabel.Text = "https://james-ingram-act-two.de/open-source/XMLSchemas";
            // 
            // MidiOutputDeviceLabel
            // 
            this.MidiOutputDeviceLabel.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.MidiOutputDeviceLabel.AutoSize = true;
            this.MidiOutputDeviceLabel.Location = new System.Drawing.Point(121, 17);
            this.MidiOutputDeviceLabel.Name = "MidiOutputDeviceLabel";
            this.MidiOutputDeviceLabel.Size = new System.Drawing.Size(147, 13);
            this.MidiOutputDeviceLabel.TabIndex = 4;
            this.MidiOutputDeviceLabel.Text = "Preferred MIDI output device:";
            // 
            // OutputDevicesComboBox
            // 
            this.OutputDevicesComboBox.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.OutputDevicesComboBox.FormattingEnabled = true;
            this.OutputDevicesComboBox.Location = new System.Drawing.Point(267, 14);
            this.OutputDevicesComboBox.Name = "OutputDevicesComboBox";
            this.OutputDevicesComboBox.Size = new System.Drawing.Size(171, 21);
            this.OutputDevicesComboBox.TabIndex = 2;
            // 
            // MoritzFolderLocationLabel
            // 
            this.MoritzFolderLocationInfoLabel.AutoSize = true;
            this.MoritzFolderLocationInfoLabel.ForeColor = System.Drawing.Color.RoyalBlue;
            this.MoritzFolderLocationInfoLabel.Location = new System.Drawing.Point(249, 22);
            this.MoritzFolderLocationInfoLabel.Name = "MoritzFolderLocationLabel";
            this.MoritzFolderLocationInfoLabel.Size = new System.Drawing.Size(105, 13);
            this.MoritzFolderLocationInfoLabel.TabIndex = 53;
            this.MoritzFolderLocationInfoLabel.Text = "MoritzFolderLocation";
            // 
            // PreferencesDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(248)))), ((int)(((byte)(248)))));
            this.ClientSize = new System.Drawing.Size(618, 317);
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
		private System.Windows.Forms.Label PreferencesFilePathInfoLabel;
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.Label LocalMoritzFolderLabel;
		private System.Windows.Forms.Label ScoresRootFolderInfoLabel;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label LocalAudioFolderLabel;
		private System.Windows.Forms.Label AudioFolderInfoLabel;
		private System.Windows.Forms.Label LocalKrystalsFolderLabel;
		private System.Windows.Forms.Label LocalExpansionFieldsFolderLabel;
		private System.Windows.Forms.Label LocalModulationOperatorsLabel;
		private System.Windows.Forms.Label ModulationOperatorsFolderInfoLabel;
		private System.Windows.Forms.Label KrystalsFolderInfoLabel;
		private System.Windows.Forms.Label ExpansionFieldsFolderInfoLabel;
		private System.Windows.Forms.GroupBox groupBox3;
		private System.Windows.Forms.Label OnlineXMLSchemasFolderLabel;
		private System.Windows.Forms.Label OnlineXMLSchemasFolderInfoLabel;
		private System.Windows.Forms.Label MidiOutputDeviceLabel;
		private System.Windows.Forms.ComboBox OutputDevicesComboBox;
        private System.Windows.Forms.Label MoritzFolderLocationInfoLabel;
    }
}