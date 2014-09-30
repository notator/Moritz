namespace Moritz.Composer
{
	partial class AssistantComposerMainForm
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
                _blackBrush.Dispose();
                _whiteBrush.Dispose();
                _systemHighlightBrush.Dispose();
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
            this.CreateScoreButton = new System.Windows.Forms.Button();
            this.QuitAlgorithmButton = new System.Windows.Forms.Button();
            this.PalettesListBox = new System.Windows.Forms.ListBox();
            this.ShowSelectedPaletteButton = new System.Windows.Forms.Button();
            this.AddPaletteButton = new System.Windows.Forms.Button();
            this.AddKrystalButton = new System.Windows.Forms.Button();
            this.DeleteSelectedPaletteButton = new System.Windows.Forms.Button();
            this.KrystalsListBox = new System.Windows.Forms.ListBox();
            this.RemoveSelectedKrystalButton = new System.Windows.Forms.Button();
            this.AddPercussionPaletteButton = new System.Windows.Forms.Button();
            this.NotationGroupBox = new System.Windows.Forms.GroupBox();
            this.InputVoiceIndicesPerStaffHelpLabel2 = new System.Windows.Forms.Label();
            this.InputVoiceIndicesPerStaffHelpLabel = new System.Windows.Forms.Label();
            this.InputVoiceIndicesPerStaffLabel = new System.Windows.Forms.Label();
            this.InputVoiceIndicesPerStaffTextBox = new System.Windows.Forms.TextBox();
            this.ChordTypeComboBoxLabel = new System.Windows.Forms.Label();
            this.ChordTypeComboBox = new System.Windows.Forms.ComboBox();
            this.ShortStaffNamesHelpLabel = new System.Windows.Forms.Label();
            this.StandardChordsOptionsPanel = new System.Windows.Forms.Panel();
            this.BeamsCrossBarlinesCheckBox = new System.Windows.Forms.CheckBox();
            this.MinimumCrotchetDurationTextBox = new System.Windows.Forms.TextBox();
            this.MinimumCrotchetDurationLabel = new System.Windows.Forms.Label();
            this.LongStaffNamesHelpLabel = new System.Windows.Forms.Label();
            this.MidiChannelsPerStaffHelpLabel2 = new System.Windows.Forms.Label();
            this.ShortStaffNamesLabel = new System.Windows.Forms.Label();
            this.LongStaffNamesTextBox = new System.Windows.Forms.TextBox();
            this.MidiChannelsPerStaffHelpLabel = new System.Windows.Forms.Label();
            this.SystemStartBarsHelpLabel = new System.Windows.Forms.Label();
            this.ShortStaffNamesTextBox = new System.Windows.Forms.TextBox();
            this.StafflinesPerStaffHelpLabel = new System.Windows.Forms.Label();
            this.MidiChannelsPerStaffLabel = new System.Windows.Forms.Label();
            this.StafflinesPerStaffLabel = new System.Windows.Forms.Label();
            this.LongStaffNamesLabel = new System.Windows.Forms.Label();
            this.MidiChannelsPerStaffTextBox = new System.Windows.Forms.TextBox();
            this.ClefsPerStaffLabel = new System.Windows.Forms.Label();
            this.ClefsPerStaffHelpLabel = new System.Windows.Forms.Label();
            this.StaffGroupsHelpLabel = new System.Windows.Forms.Label();
            this.StaffGroupsLabel = new System.Windows.Forms.Label();
            this.StafflinesPerStaffTextBox = new System.Windows.Forms.TextBox();
            this.SystemStartBarsLabel = new System.Windows.Forms.Label();
            this.SystemStartBarsTextBox = new System.Windows.Forms.TextBox();
            this.ClefsPerStaffTextBox = new System.Windows.Forms.TextBox();
            this.StaffGroupsTextBox = new System.Windows.Forms.TextBox();
            this.UnitsHelpLabel = new System.Windows.Forms.Label();
            this.MinimumGapsBetweenSystemsTextBox = new System.Windows.Forms.TextBox();
            this.MinimumGapsBetweenStavesTextBox = new System.Windows.Forms.TextBox();
            this.StafflineStemStrokeWidthComboBox = new System.Windows.Forms.ComboBox();
            this.GapPixelsComboBox = new System.Windows.Forms.ComboBox();
            this.MinimumGapsBetweenSystemsLabel = new System.Windows.Forms.Label();
            this.StaffLineStemStrokeWidthLabel = new System.Windows.Forms.Label();
            this.MinimumGapsBetweenStavesLabel = new System.Windows.Forms.Label();
            this.GapPixelsLabel = new System.Windows.Forms.Label();
            this.ScoreComboBox = new System.Windows.Forms.ComboBox();
            this.ScoreComboBoxLabel = new System.Windows.Forms.Label();
            this.SaveSettingsButton = new System.Windows.Forms.Button();
            this.QuitMoritzButton = new System.Windows.Forms.Button();
            this.ShowMoritzButton = new System.Windows.Forms.Button();
            this.ShowSelectedKrystalButton = new System.Windows.Forms.Button();
            this.DimensionsAndMetadataButton = new System.Windows.Forms.Button();
            this.KrystalsGroupBox = new System.Windows.Forms.GroupBox();
            this.PalettesGroupBox = new System.Windows.Forms.GroupBox();
            this.NotationGroupBox.SuspendLayout();
            this.StandardChordsOptionsPanel.SuspendLayout();
            this.KrystalsGroupBox.SuspendLayout();
            this.PalettesGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // CreateScoreButton
            // 
            this.CreateScoreButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.CreateScoreButton.Font = new System.Drawing.Font("Arial", 8F);
            this.CreateScoreButton.Location = new System.Drawing.Point(440, 578);
            this.CreateScoreButton.Name = "CreateScoreButton";
            this.CreateScoreButton.Size = new System.Drawing.Size(127, 26);
            this.CreateScoreButton.TabIndex = 5;
            this.CreateScoreButton.Text = "create score";
            this.CreateScoreButton.UseVisualStyleBackColor = true;
            this.CreateScoreButton.Click += new System.EventHandler(this.CreateScoreButton_Click);
            // 
            // QuitAlgorithmButton
            // 
            this.QuitAlgorithmButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.QuitAlgorithmButton.Font = new System.Drawing.Font("Arial", 8F);
            this.QuitAlgorithmButton.Location = new System.Drawing.Point(582, 548);
            this.QuitAlgorithmButton.Name = "QuitAlgorithmButton";
            this.QuitAlgorithmButton.Size = new System.Drawing.Size(127, 26);
            this.QuitAlgorithmButton.TabIndex = 8;
            this.QuitAlgorithmButton.Text = "Quit algorithm";
            this.QuitAlgorithmButton.UseVisualStyleBackColor = true;
            this.QuitAlgorithmButton.Click += new System.EventHandler(this.QuitAssistantComposerButton_Click);
            // 
            // PalettesListBox
            // 
            this.PalettesListBox.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.PalettesListBox.FormattingEnabled = true;
            this.PalettesListBox.ItemHeight = 14;
            this.PalettesListBox.Location = new System.Drawing.Point(16, 23);
            this.PalettesListBox.Name = "PalettesListBox";
            this.PalettesListBox.Size = new System.Drawing.Size(183, 88);
            this.PalettesListBox.TabIndex = 0;
            this.PalettesListBox.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.PalettesListBox_DrawItem);
            this.PalettesListBox.SelectedIndexChanged += new System.EventHandler(this.PalettesListBox_SelectedIndexChanged);
            // 
            // ShowSelectedPaletteButton
            // 
            this.ShowSelectedPaletteButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(215)))), ((int)(((byte)(225)))), ((int)(((byte)(215)))));
            this.ShowSelectedPaletteButton.Enabled = false;
            this.ShowSelectedPaletteButton.Font = new System.Drawing.Font("Arial", 8F);
            this.ShowSelectedPaletteButton.ForeColor = System.Drawing.SystemColors.ControlText;
            this.ShowSelectedPaletteButton.Location = new System.Drawing.Point(16, 118);
            this.ShowSelectedPaletteButton.Name = "ShowSelectedPaletteButton";
            this.ShowSelectedPaletteButton.Size = new System.Drawing.Size(183, 26);
            this.ShowSelectedPaletteButton.TabIndex = 0;
            this.ShowSelectedPaletteButton.Text = "show selected palette";
            this.ShowSelectedPaletteButton.UseVisualStyleBackColor = false;
            this.ShowSelectedPaletteButton.Click += new System.EventHandler(this.ShowSelectedPaletteButton_Click);
            // 
            // AddPaletteButton
            // 
            this.AddPaletteButton.Font = new System.Drawing.Font("Arial", 8F);
            this.AddPaletteButton.ForeColor = System.Drawing.SystemColors.ControlText;
            this.AddPaletteButton.Location = new System.Drawing.Point(16, 151);
            this.AddPaletteButton.Name = "AddPaletteButton";
            this.AddPaletteButton.Size = new System.Drawing.Size(183, 26);
            this.AddPaletteButton.TabIndex = 1;
            this.AddPaletteButton.Text = "add palette";
            this.AddPaletteButton.UseVisualStyleBackColor = true;
            this.AddPaletteButton.Click += new System.EventHandler(this.AddPaletteButton_Click);
            // 
            // AddKrystalButton
            // 
            this.AddKrystalButton.Font = new System.Drawing.Font("Arial", 8F);
            this.AddKrystalButton.ForeColor = System.Drawing.SystemColors.ControlText;
            this.AddKrystalButton.Location = new System.Drawing.Point(16, 149);
            this.AddKrystalButton.Name = "AddKrystalButton";
            this.AddKrystalButton.Size = new System.Drawing.Size(183, 26);
            this.AddKrystalButton.TabIndex = 1;
            this.AddKrystalButton.Text = "add krystal";
            this.AddKrystalButton.UseVisualStyleBackColor = true;
            this.AddKrystalButton.Click += new System.EventHandler(this.AddKrystalButton_Click);
            // 
            // DeleteSelectedPaletteButton
            // 
            this.DeleteSelectedPaletteButton.Enabled = false;
            this.DeleteSelectedPaletteButton.Font = new System.Drawing.Font("Arial", 8F);
            this.DeleteSelectedPaletteButton.ForeColor = System.Drawing.SystemColors.ControlText;
            this.DeleteSelectedPaletteButton.Location = new System.Drawing.Point(16, 217);
            this.DeleteSelectedPaletteButton.Name = "DeleteSelectedPaletteButton";
            this.DeleteSelectedPaletteButton.Size = new System.Drawing.Size(183, 26);
            this.DeleteSelectedPaletteButton.TabIndex = 3;
            this.DeleteSelectedPaletteButton.Text = "delete selected palette";
            this.DeleteSelectedPaletteButton.UseVisualStyleBackColor = true;
            this.DeleteSelectedPaletteButton.Click += new System.EventHandler(this.DeletePaletteButton_Click);
            // 
            // KrystalsListBox
            // 
            this.KrystalsListBox.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.KrystalsListBox.FormattingEnabled = true;
            this.KrystalsListBox.ItemHeight = 14;
            this.KrystalsListBox.Location = new System.Drawing.Point(16, 23);
            this.KrystalsListBox.Name = "KrystalsListBox";
            this.KrystalsListBox.Size = new System.Drawing.Size(183, 88);
            this.KrystalsListBox.TabIndex = 0;
            this.KrystalsListBox.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.KrystalsListBox_DrawItem);
            this.KrystalsListBox.SelectedIndexChanged += new System.EventHandler(this.KrystalsListBox_SelectedIndexChanged);
            // 
            // RemoveSelectedKrystalButton
            // 
            this.RemoveSelectedKrystalButton.Enabled = false;
            this.RemoveSelectedKrystalButton.Font = new System.Drawing.Font("Arial", 8F);
            this.RemoveSelectedKrystalButton.ForeColor = System.Drawing.SystemColors.ControlText;
            this.RemoveSelectedKrystalButton.Location = new System.Drawing.Point(16, 181);
            this.RemoveSelectedKrystalButton.Name = "RemoveSelectedKrystalButton";
            this.RemoveSelectedKrystalButton.Size = new System.Drawing.Size(183, 26);
            this.RemoveSelectedKrystalButton.TabIndex = 2;
            this.RemoveSelectedKrystalButton.Text = "remove selected krystal";
            this.RemoveSelectedKrystalButton.UseVisualStyleBackColor = true;
            this.RemoveSelectedKrystalButton.Click += new System.EventHandler(this.RemoveSelectedKrystalButton_Click);
            // 
            // AddPercussionPaletteButton
            // 
            this.AddPercussionPaletteButton.Font = new System.Drawing.Font("Arial", 8F);
            this.AddPercussionPaletteButton.ForeColor = System.Drawing.SystemColors.ControlText;
            this.AddPercussionPaletteButton.Location = new System.Drawing.Point(16, 184);
            this.AddPercussionPaletteButton.Name = "AddPercussionPaletteButton";
            this.AddPercussionPaletteButton.Size = new System.Drawing.Size(183, 26);
            this.AddPercussionPaletteButton.TabIndex = 2;
            this.AddPercussionPaletteButton.Text = "add percussion palette";
            this.AddPercussionPaletteButton.UseVisualStyleBackColor = true;
            this.AddPercussionPaletteButton.Click += new System.EventHandler(this.AddPercussionPaletteButton_Click);
            // 
            // NotationGroupBox
            // 
            this.NotationGroupBox.Controls.Add(this.InputVoiceIndicesPerStaffHelpLabel2);
            this.NotationGroupBox.Controls.Add(this.InputVoiceIndicesPerStaffHelpLabel);
            this.NotationGroupBox.Controls.Add(this.InputVoiceIndicesPerStaffLabel);
            this.NotationGroupBox.Controls.Add(this.InputVoiceIndicesPerStaffTextBox);
            this.NotationGroupBox.Controls.Add(this.ChordTypeComboBoxLabel);
            this.NotationGroupBox.Controls.Add(this.ChordTypeComboBox);
            this.NotationGroupBox.Controls.Add(this.ShortStaffNamesHelpLabel);
            this.NotationGroupBox.Controls.Add(this.StandardChordsOptionsPanel);
            this.NotationGroupBox.Controls.Add(this.LongStaffNamesHelpLabel);
            this.NotationGroupBox.Controls.Add(this.MidiChannelsPerStaffHelpLabel2);
            this.NotationGroupBox.Controls.Add(this.ShortStaffNamesLabel);
            this.NotationGroupBox.Controls.Add(this.LongStaffNamesTextBox);
            this.NotationGroupBox.Controls.Add(this.MidiChannelsPerStaffHelpLabel);
            this.NotationGroupBox.Controls.Add(this.SystemStartBarsHelpLabel);
            this.NotationGroupBox.Controls.Add(this.ShortStaffNamesTextBox);
            this.NotationGroupBox.Controls.Add(this.StafflinesPerStaffHelpLabel);
            this.NotationGroupBox.Controls.Add(this.MidiChannelsPerStaffLabel);
            this.NotationGroupBox.Controls.Add(this.StafflinesPerStaffLabel);
            this.NotationGroupBox.Controls.Add(this.LongStaffNamesLabel);
            this.NotationGroupBox.Controls.Add(this.MidiChannelsPerStaffTextBox);
            this.NotationGroupBox.Controls.Add(this.ClefsPerStaffLabel);
            this.NotationGroupBox.Controls.Add(this.ClefsPerStaffHelpLabel);
            this.NotationGroupBox.Controls.Add(this.StaffGroupsHelpLabel);
            this.NotationGroupBox.Controls.Add(this.StaffGroupsLabel);
            this.NotationGroupBox.Controls.Add(this.StafflinesPerStaffTextBox);
            this.NotationGroupBox.Controls.Add(this.SystemStartBarsLabel);
            this.NotationGroupBox.Controls.Add(this.SystemStartBarsTextBox);
            this.NotationGroupBox.Controls.Add(this.ClefsPerStaffTextBox);
            this.NotationGroupBox.Controls.Add(this.StaffGroupsTextBox);
            this.NotationGroupBox.Controls.Add(this.UnitsHelpLabel);
            this.NotationGroupBox.Controls.Add(this.MinimumGapsBetweenSystemsTextBox);
            this.NotationGroupBox.Controls.Add(this.MinimumGapsBetweenStavesTextBox);
            this.NotationGroupBox.Controls.Add(this.StafflineStemStrokeWidthComboBox);
            this.NotationGroupBox.Controls.Add(this.GapPixelsComboBox);
            this.NotationGroupBox.Controls.Add(this.MinimumGapsBetweenSystemsLabel);
            this.NotationGroupBox.Controls.Add(this.StaffLineStemStrokeWidthLabel);
            this.NotationGroupBox.Controls.Add(this.MinimumGapsBetweenStavesLabel);
            this.NotationGroupBox.Controls.Add(this.GapPixelsLabel);
            this.NotationGroupBox.ForeColor = System.Drawing.Color.Brown;
            this.NotationGroupBox.Location = new System.Drawing.Point(23, 39);
            this.NotationGroupBox.Name = "NotationGroupBox";
            this.NotationGroupBox.Size = new System.Drawing.Size(466, 496);
            this.NotationGroupBox.TabIndex = 1;
            this.NotationGroupBox.TabStop = false;
            this.NotationGroupBox.Text = "notation";
            // 
            // InputVoiceIndicesPerStaffHelpLabel2
            // 
            this.InputVoiceIndicesPerStaffHelpLabel2.AutoSize = true;
            this.InputVoiceIndicesPerStaffHelpLabel2.ForeColor = System.Drawing.Color.RoyalBlue;
            this.InputVoiceIndicesPerStaffHelpLabel2.Location = new System.Drawing.Point(262, 200);
            this.InputVoiceIndicesPerStaffHelpLabel2.Name = "InputVoiceIndicesPerStaffHelpLabel2";
            this.InputVoiceIndicesPerStaffHelpLabel2.Size = new System.Drawing.Size(141, 14);
            this.InputVoiceIndicesPerStaffHelpLabel2.TabIndex = 190;
            this.InputVoiceIndicesPerStaffHelpLabel2.Text = "(right click this text for help)";
            this.InputVoiceIndicesPerStaffHelpLabel2.MouseClick += new System.Windows.Forms.MouseEventHandler(this.MidiChannelsPerStaffHelp_MouseClick);
            // 
            // InputVoiceIndicesPerStaffHelpLabel
            // 
            this.InputVoiceIndicesPerStaffHelpLabel.AutoSize = true;
            this.InputVoiceIndicesPerStaffHelpLabel.ForeColor = System.Drawing.Color.RoyalBlue;
            this.InputVoiceIndicesPerStaffHelpLabel.Location = new System.Drawing.Point(121, 180);
            this.InputVoiceIndicesPerStaffHelpLabel.Name = "InputVoiceIndicesPerStaffHelpLabel";
            this.InputVoiceIndicesPerStaffHelpLabel.Size = new System.Drawing.Size(104, 14);
            this.InputVoiceIndicesPerStaffHelpLabel.TabIndex = 189;
            this.InputVoiceIndicesPerStaffHelpLabel.Text = "input voices: x y z...";
            // 
            // InputVoiceIndicesPerStaffLabel
            // 
            this.InputVoiceIndicesPerStaffLabel.AutoSize = true;
            this.InputVoiceIndicesPerStaffLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(60)))), ((int)(((byte)(60)))));
            this.InputVoiceIndicesPerStaffLabel.Location = new System.Drawing.Point(24, 193);
            this.InputVoiceIndicesPerStaffLabel.Margin = new System.Windows.Forms.Padding(0);
            this.InputVoiceIndicesPerStaffLabel.Name = "InputVoiceIndicesPerStaffLabel";
            this.InputVoiceIndicesPerStaffLabel.Size = new System.Drawing.Size(97, 28);
            this.InputVoiceIndicesPerStaffLabel.TabIndex = 188;
            this.InputVoiceIndicesPerStaffLabel.Text = "input voices\r\nper voice per staff";
            this.InputVoiceIndicesPerStaffLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // InputVoiceIndicesPerStaffTextBox
            // 
            this.InputVoiceIndicesPerStaffTextBox.Location = new System.Drawing.Point(123, 197);
            this.InputVoiceIndicesPerStaffTextBox.Name = "InputVoiceIndicesPerStaffTextBox";
            this.InputVoiceIndicesPerStaffTextBox.Size = new System.Drawing.Size(136, 20);
            this.InputVoiceIndicesPerStaffTextBox.TabIndex = 187;
            this.InputVoiceIndicesPerStaffTextBox.TextChanged += new System.EventHandler(this.InputVoiceIndicesPerStaffTextBox_TextChanged);
            this.InputVoiceIndicesPerStaffTextBox.Leave += new System.EventHandler(this.InputVoiceIndicesPerStaffTextBox_Leave);
            // 
            // ChordTypeComboBoxLabel
            // 
            this.ChordTypeComboBoxLabel.AutoSize = true;
            this.ChordTypeComboBoxLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(60)))), ((int)(((byte)(60)))));
            this.ChordTypeComboBoxLabel.Location = new System.Drawing.Point(6, 23);
            this.ChordTypeComboBoxLabel.Margin = new System.Windows.Forms.Padding(0);
            this.ChordTypeComboBoxLabel.Name = "ChordTypeComboBoxLabel";
            this.ChordTypeComboBoxLabel.Size = new System.Drawing.Size(59, 14);
            this.ChordTypeComboBoxLabel.TabIndex = 186;
            this.ChordTypeComboBoxLabel.Text = "chord type";
            // 
            // ChordTypeComboBox
            // 
            this.ChordTypeComboBox.FormattingEnabled = true;
            this.ChordTypeComboBox.Location = new System.Drawing.Point(68, 19);
            this.ChordTypeComboBox.Name = "ChordTypeComboBox";
            this.ChordTypeComboBox.Size = new System.Drawing.Size(133, 22);
            this.ChordTypeComboBox.TabIndex = 12;
            this.ChordTypeComboBox.SelectedIndexChanged += new System.EventHandler(this.ChordTypeComboBox_SelectedIndexChanged);
            // 
            // ShortStaffNamesHelpLabel
            // 
            this.ShortStaffNamesHelpLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.ShortStaffNamesHelpLabel.AutoSize = true;
            this.ShortStaffNamesHelpLabel.ForeColor = System.Drawing.Color.RoyalBlue;
            this.ShortStaffNamesHelpLabel.Location = new System.Drawing.Point(100, 389);
            this.ShortStaffNamesHelpLabel.Name = "ShortStaffNamesHelpLabel";
            this.ShortStaffNamesHelpLabel.Size = new System.Drawing.Size(125, 14);
            this.ShortStaffNamesHelpLabel.TabIndex = 176;
            this.ShortStaffNamesHelpLabel.Text = "(x names, one per staff)";
            // 
            // StandardChordsOptionsPanel
            // 
            this.StandardChordsOptionsPanel.Controls.Add(this.BeamsCrossBarlinesCheckBox);
            this.StandardChordsOptionsPanel.Controls.Add(this.MinimumCrotchetDurationTextBox);
            this.StandardChordsOptionsPanel.Controls.Add(this.MinimumCrotchetDurationLabel);
            this.StandardChordsOptionsPanel.Location = new System.Drawing.Point(43, 45);
            this.StandardChordsOptionsPanel.Name = "StandardChordsOptionsPanel";
            this.StandardChordsOptionsPanel.Size = new System.Drawing.Size(158, 57);
            this.StandardChordsOptionsPanel.TabIndex = 184;
            // 
            // BeamsCrossBarlinesCheckBox
            // 
            this.BeamsCrossBarlinesCheckBox.AutoSize = true;
            this.BeamsCrossBarlinesCheckBox.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.BeamsCrossBarlinesCheckBox.ForeColor = System.Drawing.SystemColors.ControlText;
            this.BeamsCrossBarlinesCheckBox.Location = new System.Drawing.Point(28, 36);
            this.BeamsCrossBarlinesCheckBox.Name = "BeamsCrossBarlinesCheckBox";
            this.BeamsCrossBarlinesCheckBox.Size = new System.Drawing.Size(130, 18);
            this.BeamsCrossBarlinesCheckBox.TabIndex = 1;
            this.BeamsCrossBarlinesCheckBox.Text = "beams cross barlines";
            this.BeamsCrossBarlinesCheckBox.UseVisualStyleBackColor = true;
            this.BeamsCrossBarlinesCheckBox.CheckedChanged += new System.EventHandler(this.BeamsCrossBarlinesCheckBox_CheckedChanged);
            // 
            // MinimumCrotchetDurationTextBox
            // 
            this.MinimumCrotchetDurationTextBox.Location = new System.Drawing.Point(95, 4);
            this.MinimumCrotchetDurationTextBox.Name = "MinimumCrotchetDurationTextBox";
            this.MinimumCrotchetDurationTextBox.Size = new System.Drawing.Size(63, 20);
            this.MinimumCrotchetDurationTextBox.TabIndex = 0;
            this.MinimumCrotchetDurationTextBox.TextChanged += new System.EventHandler(this.MinimumCrotchetDurationTextBox_TextChanged);
            this.MinimumCrotchetDurationTextBox.Leave += new System.EventHandler(this.MinimumCrotchetDurationTextBox_TextChanged);
            // 
            // MinimumCrotchetDurationLabel
            // 
            this.MinimumCrotchetDurationLabel.AutoSize = true;
            this.MinimumCrotchetDurationLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(60)))), ((int)(((byte)(60)))));
            this.MinimumCrotchetDurationLabel.Location = new System.Drawing.Point(0, 0);
            this.MinimumCrotchetDurationLabel.Margin = new System.Windows.Forms.Padding(0);
            this.MinimumCrotchetDurationLabel.Name = "MinimumCrotchetDurationLabel";
            this.MinimumCrotchetDurationLabel.Size = new System.Drawing.Size(90, 28);
            this.MinimumCrotchetDurationLabel.TabIndex = 147;
            this.MinimumCrotchetDurationLabel.Text = "minimum crotchet\r\nduration (ms)";
            this.MinimumCrotchetDurationLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // LongStaffNamesHelpLabel
            // 
            this.LongStaffNamesHelpLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.LongStaffNamesHelpLabel.AutoSize = true;
            this.LongStaffNamesHelpLabel.ForeColor = System.Drawing.Color.RoyalBlue;
            this.LongStaffNamesHelpLabel.Location = new System.Drawing.Point(97, 347);
            this.LongStaffNamesHelpLabel.Name = "LongStaffNamesHelpLabel";
            this.LongStaffNamesHelpLabel.Size = new System.Drawing.Size(125, 14);
            this.LongStaffNamesHelpLabel.TabIndex = 174;
            this.LongStaffNamesHelpLabel.Text = "(x names, one per staff)";
            // 
            // MidiChannelsPerStaffHelpLabel2
            // 
            this.MidiChannelsPerStaffHelpLabel2.AutoSize = true;
            this.MidiChannelsPerStaffHelpLabel2.ForeColor = System.Drawing.Color.RoyalBlue;
            this.MidiChannelsPerStaffHelpLabel2.Location = new System.Drawing.Point(262, 157);
            this.MidiChannelsPerStaffHelpLabel2.Name = "MidiChannelsPerStaffHelpLabel2";
            this.MidiChannelsPerStaffHelpLabel2.Size = new System.Drawing.Size(141, 14);
            this.MidiChannelsPerStaffHelpLabel2.TabIndex = 183;
            this.MidiChannelsPerStaffHelpLabel2.Text = "(right click this text for help)";
            this.MidiChannelsPerStaffHelpLabel2.MouseClick += new System.Windows.Forms.MouseEventHandler(this.MidiChannelsPerStaffHelp_MouseClick);
            // 
            // ShortStaffNamesLabel
            // 
            this.ShortStaffNamesLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.ShortStaffNamesLabel.AutoSize = true;
            this.ShortStaffNamesLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(60)))), ((int)(((byte)(60)))));
            this.ShortStaffNamesLabel.Location = new System.Drawing.Point(10, 389);
            this.ShortStaffNamesLabel.Margin = new System.Windows.Forms.Padding(0);
            this.ShortStaffNamesLabel.Name = "ShortStaffNamesLabel";
            this.ShortStaffNamesLabel.Size = new System.Drawing.Size(93, 14);
            this.ShortStaffNamesLabel.TabIndex = 175;
            this.ShortStaffNamesLabel.Text = "short staff names";
            // 
            // LongStaffNamesTextBox
            // 
            this.LongStaffNamesTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.LongStaffNamesTextBox.Location = new System.Drawing.Point(10, 364);
            this.LongStaffNamesTextBox.Name = "LongStaffNamesTextBox";
            this.LongStaffNamesTextBox.Size = new System.Drawing.Size(440, 20);
            this.LongStaffNamesTextBox.TabIndex = 5;
            this.LongStaffNamesTextBox.TextChanged += new System.EventHandler(this.LongStaffNamesTextBox_TextChanged);
            this.LongStaffNamesTextBox.Leave += new System.EventHandler(this.LongStaffNamesTextBox_Leave);
            // 
            // MidiChannelsPerStaffHelpLabel
            // 
            this.MidiChannelsPerStaffHelpLabel.AutoSize = true;
            this.MidiChannelsPerStaffHelpLabel.ForeColor = System.Drawing.Color.RoyalBlue;
            this.MidiChannelsPerStaffHelpLabel.Location = new System.Drawing.Point(121, 137);
            this.MidiChannelsPerStaffHelpLabel.Name = "MidiChannelsPerStaffHelpLabel";
            this.MidiChannelsPerStaffHelpLabel.Size = new System.Drawing.Size(93, 14);
            this.MidiChannelsPerStaffHelpLabel.TabIndex = 182;
            this.MidiChannelsPerStaffHelpLabel.Text = "channels:  x y z...";
            // 
            // SystemStartBarsHelpLabel
            // 
            this.SystemStartBarsHelpLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.SystemStartBarsHelpLabel.AutoSize = true;
            this.SystemStartBarsHelpLabel.ForeColor = System.Drawing.Color.RoyalBlue;
            this.SystemStartBarsHelpLabel.Location = new System.Drawing.Point(139, 442);
            this.SystemStartBarsHelpLabel.Name = "SystemStartBarsHelpLabel";
            this.SystemStartBarsHelpLabel.Size = new System.Drawing.Size(150, 14);
            this.SystemStartBarsHelpLabel.TabIndex = 179;
            this.SystemStartBarsHelpLabel.Text = "(default is 5 bars per system)";
            // 
            // ShortStaffNamesTextBox
            // 
            this.ShortStaffNamesTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.ShortStaffNamesTextBox.Location = new System.Drawing.Point(10, 405);
            this.ShortStaffNamesTextBox.Name = "ShortStaffNamesTextBox";
            this.ShortStaffNamesTextBox.Size = new System.Drawing.Size(438, 20);
            this.ShortStaffNamesTextBox.TabIndex = 6;
            this.ShortStaffNamesTextBox.TextChanged += new System.EventHandler(this.ShortStaffNamesTextBox_TextChanged);
            this.ShortStaffNamesTextBox.Leave += new System.EventHandler(this.ShortStaffNamesTextBox_Leave);
            // 
            // StafflinesPerStaffHelpLabel
            // 
            this.StafflinesPerStaffHelpLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.StafflinesPerStaffHelpLabel.AutoSize = true;
            this.StafflinesPerStaffHelpLabel.ForeColor = System.Drawing.Color.RoyalBlue;
            this.StafflinesPerStaffHelpLabel.Location = new System.Drawing.Point(262, 267);
            this.StafflinesPerStaffHelpLabel.Name = "StafflinesPerStaffHelpLabel";
            this.StafflinesPerStaffHelpLabel.Size = new System.Drawing.Size(194, 28);
            this.StafflinesPerStaffHelpLabel.TabIndex = 178;
            this.StafflinesPerStaffHelpLabel.Text = "x integer values separated by commas\r\nstandard clefs must have 5 lines.";
            // 
            // MidiChannelsPerStaffLabel
            // 
            this.MidiChannelsPerStaffLabel.AutoSize = true;
            this.MidiChannelsPerStaffLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(60)))), ((int)(((byte)(60)))));
            this.MidiChannelsPerStaffLabel.Location = new System.Drawing.Point(16, 150);
            this.MidiChannelsPerStaffLabel.Margin = new System.Windows.Forms.Padding(0);
            this.MidiChannelsPerStaffLabel.Name = "MidiChannelsPerStaffLabel";
            this.MidiChannelsPerStaffLabel.Size = new System.Drawing.Size(105, 28);
            this.MidiChannelsPerStaffLabel.TabIndex = 181;
            this.MidiChannelsPerStaffLabel.Text = "midi output channels\r\nper voice per staff";
            this.MidiChannelsPerStaffLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // StafflinesPerStaffLabel
            // 
            this.StafflinesPerStaffLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.StafflinesPerStaffLabel.AutoSize = true;
            this.StafflinesPerStaffLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(60)))), ((int)(((byte)(60)))));
            this.StafflinesPerStaffLabel.Location = new System.Drawing.Point(24, 274);
            this.StafflinesPerStaffLabel.Margin = new System.Windows.Forms.Padding(0);
            this.StafflinesPerStaffLabel.Name = "StafflinesPerStaffLabel";
            this.StafflinesPerStaffLabel.Size = new System.Drawing.Size(97, 14);
            this.StafflinesPerStaffLabel.TabIndex = 177;
            this.StafflinesPerStaffLabel.Text = "stafflines per staff";
            // 
            // LongStaffNamesLabel
            // 
            this.LongStaffNamesLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.LongStaffNamesLabel.AutoSize = true;
            this.LongStaffNamesLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(60)))), ((int)(((byte)(60)))));
            this.LongStaffNamesLabel.Location = new System.Drawing.Point(10, 347);
            this.LongStaffNamesLabel.Margin = new System.Windows.Forms.Padding(0);
            this.LongStaffNamesLabel.Name = "LongStaffNamesLabel";
            this.LongStaffNamesLabel.Size = new System.Drawing.Size(88, 14);
            this.LongStaffNamesLabel.TabIndex = 173;
            this.LongStaffNamesLabel.Text = "long staff names";
            // 
            // MidiChannelsPerStaffTextBox
            // 
            this.MidiChannelsPerStaffTextBox.Location = new System.Drawing.Point(123, 154);
            this.MidiChannelsPerStaffTextBox.Name = "MidiChannelsPerStaffTextBox";
            this.MidiChannelsPerStaffTextBox.Size = new System.Drawing.Size(136, 20);
            this.MidiChannelsPerStaffTextBox.TabIndex = 1;
            this.MidiChannelsPerStaffTextBox.TextChanged += new System.EventHandler(this.MidiChannelsPerStaffTextBox_TextChanged);
            this.MidiChannelsPerStaffTextBox.Leave += new System.EventHandler(this.MidiChannelsPerStaffTextBox_Leave);
            // 
            // ClefsPerStaffLabel
            // 
            this.ClefsPerStaffLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.ClefsPerStaffLabel.AutoSize = true;
            this.ClefsPerStaffLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(60)))), ((int)(((byte)(60)))));
            this.ClefsPerStaffLabel.Location = new System.Drawing.Point(45, 237);
            this.ClefsPerStaffLabel.Margin = new System.Windows.Forms.Padding(0);
            this.ClefsPerStaffLabel.Name = "ClefsPerStaffLabel";
            this.ClefsPerStaffLabel.Size = new System.Drawing.Size(76, 14);
            this.ClefsPerStaffLabel.TabIndex = 172;
            this.ClefsPerStaffLabel.Text = "clefs per staff";
            // 
            // ClefsPerStaffHelpLabel
            // 
            this.ClefsPerStaffHelpLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.ClefsPerStaffHelpLabel.AutoSize = true;
            this.ClefsPerStaffHelpLabel.ForeColor = System.Drawing.Color.RoyalBlue;
            this.ClefsPerStaffHelpLabel.Location = new System.Drawing.Point(262, 230);
            this.ClefsPerStaffHelpLabel.Name = "ClefsPerStaffHelpLabel";
            this.ClefsPerStaffHelpLabel.Size = new System.Drawing.Size(208, 28);
            this.ClefsPerStaffHelpLabel.TabIndex = 171;
            this.ClefsPerStaffHelpLabel.Text = "x clefs separated by commas\r\navailable clefs: t, t1, t2, t3, b, b1, b2, b3, n";
            // 
            // StaffGroupsHelpLabel
            // 
            this.StaffGroupsHelpLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.StaffGroupsHelpLabel.AutoSize = true;
            this.StaffGroupsHelpLabel.ForeColor = System.Drawing.Color.RoyalBlue;
            this.StaffGroupsHelpLabel.Location = new System.Drawing.Point(262, 311);
            this.StaffGroupsHelpLabel.Name = "StaffGroupsHelpLabel";
            this.StaffGroupsHelpLabel.Size = new System.Drawing.Size(124, 14);
            this.StaffGroupsHelpLabel.TabIndex = 167;
            this.StaffGroupsHelpLabel.Text = "integer values (total = x)";
            // 
            // StaffGroupsLabel
            // 
            this.StaffGroupsLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.StaffGroupsLabel.AutoSize = true;
            this.StaffGroupsLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(60)))), ((int)(((byte)(60)))));
            this.StaffGroupsLabel.Location = new System.Drawing.Point(54, 311);
            this.StaffGroupsLabel.Margin = new System.Windows.Forms.Padding(0);
            this.StaffGroupsLabel.Name = "StaffGroupsLabel";
            this.StaffGroupsLabel.Size = new System.Drawing.Size(67, 14);
            this.StaffGroupsLabel.TabIndex = 166;
            this.StaffGroupsLabel.Text = "staff groups";
            // 
            // StafflinesPerStaffTextBox
            // 
            this.StafflinesPerStaffTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.StafflinesPerStaffTextBox.Location = new System.Drawing.Point(123, 271);
            this.StafflinesPerStaffTextBox.Name = "StafflinesPerStaffTextBox";
            this.StafflinesPerStaffTextBox.Size = new System.Drawing.Size(136, 20);
            this.StafflinesPerStaffTextBox.TabIndex = 3;
            this.StafflinesPerStaffTextBox.TextChanged += new System.EventHandler(this.StafflinesPerStaffTextBox_TextChanged);
            this.StafflinesPerStaffTextBox.Leave += new System.EventHandler(this.StafflinesPerStaffTextBox_Leave);
            // 
            // SystemStartBarsLabel
            // 
            this.SystemStartBarsLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.SystemStartBarsLabel.AutoSize = true;
            this.SystemStartBarsLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(60)))), ((int)(((byte)(60)))));
            this.SystemStartBarsLabel.Location = new System.Drawing.Point(10, 442);
            this.SystemStartBarsLabel.Margin = new System.Windows.Forms.Padding(0);
            this.SystemStartBarsLabel.Name = "SystemStartBarsLabel";
            this.SystemStartBarsLabel.Size = new System.Drawing.Size(131, 14);
            this.SystemStartBarsLabel.TabIndex = 152;
            this.SystemStartBarsLabel.Text = "system start bar numbers";
            this.SystemStartBarsLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // SystemStartBarsTextBox
            // 
            this.SystemStartBarsTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.SystemStartBarsTextBox.Location = new System.Drawing.Point(10, 459);
            this.SystemStartBarsTextBox.Name = "SystemStartBarsTextBox";
            this.SystemStartBarsTextBox.Size = new System.Drawing.Size(442, 20);
            this.SystemStartBarsTextBox.TabIndex = 7;
            this.SystemStartBarsTextBox.TextChanged += new System.EventHandler(this.SystemStartBarsTextBox_TextChanged);
            this.SystemStartBarsTextBox.Leave += new System.EventHandler(this.SystemStartBarsTextBox_Leave);
            // 
            // ClefsPerStaffTextBox
            // 
            this.ClefsPerStaffTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.ClefsPerStaffTextBox.Location = new System.Drawing.Point(123, 234);
            this.ClefsPerStaffTextBox.Name = "ClefsPerStaffTextBox";
            this.ClefsPerStaffTextBox.Size = new System.Drawing.Size(136, 20);
            this.ClefsPerStaffTextBox.TabIndex = 2;
            this.ClefsPerStaffTextBox.TextChanged += new System.EventHandler(this.ClefsPerStaffTextBox_TextChanged);
            this.ClefsPerStaffTextBox.Leave += new System.EventHandler(this.ClefsPerStaffTextBox_Leave);
            // 
            // StaffGroupsTextBox
            // 
            this.StaffGroupsTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.StaffGroupsTextBox.Location = new System.Drawing.Point(123, 308);
            this.StaffGroupsTextBox.Name = "StaffGroupsTextBox";
            this.StaffGroupsTextBox.Size = new System.Drawing.Size(136, 20);
            this.StaffGroupsTextBox.TabIndex = 4;
            this.StaffGroupsTextBox.TextChanged += new System.EventHandler(this.StaffGroupsTextBox_TextChanged);
            this.StaffGroupsTextBox.Leave += new System.EventHandler(this.StaffGroupsTextBox_Leave);
            // 
            // UnitsHelpLabel
            // 
            this.UnitsHelpLabel.AutoSize = true;
            this.UnitsHelpLabel.ForeColor = System.Drawing.Color.RoyalBlue;
            this.UnitsHelpLabel.Location = new System.Drawing.Point(304, 131);
            this.UnitsHelpLabel.Name = "UnitsHelpLabel";
            this.UnitsHelpLabel.Size = new System.Drawing.Size(152, 14);
            this.UnitsHelpLabel.TabIndex = 133;
            this.UnitsHelpLabel.Text = "*screen pixels (100% display)";
            // 
            // MinimumGapsBetweenSystemsTextBox
            // 
            this.MinimumGapsBetweenSystemsTextBox.Location = new System.Drawing.Point(394, 107);
            this.MinimumGapsBetweenSystemsTextBox.Name = "MinimumGapsBetweenSystemsTextBox";
            this.MinimumGapsBetweenSystemsTextBox.Size = new System.Drawing.Size(58, 20);
            this.MinimumGapsBetweenSystemsTextBox.TabIndex = 11;
            this.MinimumGapsBetweenSystemsTextBox.TextChanged += new System.EventHandler(this.MinimumGapsBetweenSystemsTextBox_TextChanged);
            // 
            // MinimumGapsBetweenStavesTextBox
            // 
            this.MinimumGapsBetweenStavesTextBox.Location = new System.Drawing.Point(394, 79);
            this.MinimumGapsBetweenStavesTextBox.Name = "MinimumGapsBetweenStavesTextBox";
            this.MinimumGapsBetweenStavesTextBox.Size = new System.Drawing.Size(58, 20);
            this.MinimumGapsBetweenStavesTextBox.TabIndex = 10;
            this.MinimumGapsBetweenStavesTextBox.TextChanged += new System.EventHandler(this.MinimumGapsBetweenStavesTextBox_TextChanged);
            // 
            // StafflineStemStrokeWidthComboBox
            // 
            this.StafflineStemStrokeWidthComboBox.FormattingEnabled = true;
            this.StafflineStemStrokeWidthComboBox.Items.AddRange(new object[] {
            "0.5",
            "1.0",
            "1.5",
            "2.0"});
            this.StafflineStemStrokeWidthComboBox.Location = new System.Drawing.Point(394, 19);
            this.StafflineStemStrokeWidthComboBox.Name = "StafflineStemStrokeWidthComboBox";
            this.StafflineStemStrokeWidthComboBox.Size = new System.Drawing.Size(58, 22);
            this.StafflineStemStrokeWidthComboBox.TabIndex = 8;
            this.StafflineStemStrokeWidthComboBox.SelectedIndexChanged += new System.EventHandler(this.StafflineStemStrokeWidthComboBox_SelectedIndexChanged);
            // 
            // GapPixelsComboBox
            // 
            this.GapPixelsComboBox.FormattingEnabled = true;
            this.GapPixelsComboBox.Items.AddRange(new object[] {
            "4",
            "6",
            "8",
            "10",
            "12",
            "14",
            "16",
            "18",
            "20",
            "24",
            "28"});
            this.GapPixelsComboBox.Location = new System.Drawing.Point(394, 49);
            this.GapPixelsComboBox.Name = "GapPixelsComboBox";
            this.GapPixelsComboBox.Size = new System.Drawing.Size(58, 22);
            this.GapPixelsComboBox.TabIndex = 9;
            this.GapPixelsComboBox.SelectedIndexChanged += new System.EventHandler(this.GapPixelsComboBox_SelectedIndexChanged);
            // 
            // MinimumGapsBetweenSystemsLabel
            // 
            this.MinimumGapsBetweenSystemsLabel.AutoSize = true;
            this.MinimumGapsBetweenSystemsLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(60)))), ((int)(((byte)(60)))));
            this.MinimumGapsBetweenSystemsLabel.Location = new System.Drawing.Point(217, 110);
            this.MinimumGapsBetweenSystemsLabel.Margin = new System.Windows.Forms.Padding(0);
            this.MinimumGapsBetweenSystemsLabel.Name = "MinimumGapsBetweenSystemsLabel";
            this.MinimumGapsBetweenSystemsLabel.Size = new System.Drawing.Size(176, 14);
            this.MinimumGapsBetweenSystemsLabel.TabIndex = 103;
            this.MinimumGapsBetweenSystemsLabel.Text = "(minimum) gaps between systems*";
            // 
            // StaffLineStemStrokeWidthLabel
            // 
            this.StaffLineStemStrokeWidthLabel.AutoSize = true;
            this.StaffLineStemStrokeWidthLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(60)))), ((int)(((byte)(60)))));
            this.StaffLineStemStrokeWidthLabel.Location = new System.Drawing.Point(230, 23);
            this.StaffLineStemStrokeWidthLabel.Margin = new System.Windows.Forms.Padding(0);
            this.StaffLineStemStrokeWidthLabel.Name = "StaffLineStemStrokeWidthLabel";
            this.StaffLineStemStrokeWidthLabel.Size = new System.Drawing.Size(163, 14);
            this.StaffLineStemStrokeWidthLabel.TabIndex = 111;
            this.StaffLineStemStrokeWidthLabel.Text = "staff line and stem stroke width*";
            // 
            // MinimumGapsBetweenStavesLabel
            // 
            this.MinimumGapsBetweenStavesLabel.AutoSize = true;
            this.MinimumGapsBetweenStavesLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(60)))), ((int)(((byte)(60)))));
            this.MinimumGapsBetweenStavesLabel.Location = new System.Drawing.Point(225, 82);
            this.MinimumGapsBetweenStavesLabel.Margin = new System.Windows.Forms.Padding(0);
            this.MinimumGapsBetweenStavesLabel.Name = "MinimumGapsBetweenStavesLabel";
            this.MinimumGapsBetweenStavesLabel.Size = new System.Drawing.Size(168, 14);
            this.MinimumGapsBetweenStavesLabel.TabIndex = 107;
            this.MinimumGapsBetweenStavesLabel.Text = "(minimum) gaps between staves*";
            // 
            // GapPixelsLabel
            // 
            this.GapPixelsLabel.AutoSize = true;
            this.GapPixelsLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(60)))), ((int)(((byte)(60)))));
            this.GapPixelsLabel.Location = new System.Drawing.Point(228, 53);
            this.GapPixelsLabel.Margin = new System.Windows.Forms.Padding(0);
            this.GapPixelsLabel.Name = "GapPixelsLabel";
            this.GapPixelsLabel.Size = new System.Drawing.Size(165, 14);
            this.GapPixelsLabel.TabIndex = 79;
            this.GapPixelsLabel.Text = "gap between staff lines (pixels)*";
            // 
            // ScoreComboBox
            // 
            this.ScoreComboBox.FormattingEnabled = true;
            this.ScoreComboBox.Location = new System.Drawing.Point(91, 12);
            this.ScoreComboBox.Name = "ScoreComboBox";
            this.ScoreComboBox.Size = new System.Drawing.Size(133, 22);
            this.ScoreComboBox.TabIndex = 0;
            this.ScoreComboBox.SelectedIndexChanged += new System.EventHandler(this.ScoreComboBox_SelectedIndexChanged);
            // 
            // ScoreComboBoxLabel
            // 
            this.ScoreComboBoxLabel.AutoSize = true;
            this.ScoreComboBoxLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(60)))), ((int)(((byte)(60)))));
            this.ScoreComboBoxLabel.Location = new System.Drawing.Point(53, 16);
            this.ScoreComboBoxLabel.Margin = new System.Windows.Forms.Padding(0);
            this.ScoreComboBoxLabel.Name = "ScoreComboBoxLabel";
            this.ScoreComboBoxLabel.Size = new System.Drawing.Size(35, 14);
            this.ScoreComboBoxLabel.TabIndex = 149;
            this.ScoreComboBoxLabel.Text = "score";
            // 
            // SaveSettingsButton
            // 
            this.SaveSettingsButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.SaveSettingsButton.Enabled = false;
            this.SaveSettingsButton.Font = new System.Drawing.Font("Arial", 8F);
            this.SaveSettingsButton.Location = new System.Drawing.Point(440, 548);
            this.SaveSettingsButton.Name = "SaveSettingsButton";
            this.SaveSettingsButton.Size = new System.Drawing.Size(127, 26);
            this.SaveSettingsButton.TabIndex = 4;
            this.SaveSettingsButton.Text = "save settings";
            this.SaveSettingsButton.UseVisualStyleBackColor = true;
            this.SaveSettingsButton.Click += new System.EventHandler(this.SaveSettingsButton_Click);
            // 
            // QuitMoritzButton
            // 
            this.QuitMoritzButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.QuitMoritzButton.Font = new System.Drawing.Font("Arial", 8F);
            this.QuitMoritzButton.Location = new System.Drawing.Point(582, 578);
            this.QuitMoritzButton.Name = "QuitMoritzButton";
            this.QuitMoritzButton.Size = new System.Drawing.Size(127, 26);
            this.QuitMoritzButton.TabIndex = 9;
            this.QuitMoritzButton.Text = "Quit Moritz";
            this.QuitMoritzButton.UseVisualStyleBackColor = true;
            this.QuitMoritzButton.Click += new System.EventHandler(this.QuitMoritzButton_Click);
            // 
            // ShowMoritzButton
            // 
            this.ShowMoritzButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.ShowMoritzButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(215)))), ((int)(((byte)(225)))), ((int)(((byte)(215)))));
            this.ShowMoritzButton.Font = new System.Drawing.Font("Arial", 8F);
            this.ShowMoritzButton.Location = new System.Drawing.Point(23, 578);
            this.ShowMoritzButton.Name = "ShowMoritzButton";
            this.ShowMoritzButton.Size = new System.Drawing.Size(183, 26);
            this.ShowMoritzButton.TabIndex = 6;
            this.ShowMoritzButton.Text = "Show Moritz";
            this.ShowMoritzButton.UseVisualStyleBackColor = false;
            this.ShowMoritzButton.Click += new System.EventHandler(this.ShowMoritzButton_Click);
            // 
            // ShowSelectedKrystalButton
            // 
            this.ShowSelectedKrystalButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(215)))), ((int)(((byte)(225)))), ((int)(((byte)(215)))));
            this.ShowSelectedKrystalButton.Enabled = false;
            this.ShowSelectedKrystalButton.Font = new System.Drawing.Font("Arial", 8F);
            this.ShowSelectedKrystalButton.ForeColor = System.Drawing.SystemColors.ControlText;
            this.ShowSelectedKrystalButton.Location = new System.Drawing.Point(16, 117);
            this.ShowSelectedKrystalButton.Name = "ShowSelectedKrystalButton";
            this.ShowSelectedKrystalButton.Size = new System.Drawing.Size(183, 26);
            this.ShowSelectedKrystalButton.TabIndex = 0;
            this.ShowSelectedKrystalButton.Text = "show selected krystal";
            this.ShowSelectedKrystalButton.UseVisualStyleBackColor = false;
            this.ShowSelectedKrystalButton.Click += new System.EventHandler(this.ShowSelectedKrystalButton_Click);
            // 
            // DimensionsAndMetadataButton
            // 
            this.DimensionsAndMetadataButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.DimensionsAndMetadataButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(215)))), ((int)(((byte)(225)))), ((int)(((byte)(215)))));
            this.DimensionsAndMetadataButton.Font = new System.Drawing.Font("Arial", 8F);
            this.DimensionsAndMetadataButton.Location = new System.Drawing.Point(23, 548);
            this.DimensionsAndMetadataButton.Name = "DimensionsAndMetadataButton";
            this.DimensionsAndMetadataButton.Size = new System.Drawing.Size(183, 26);
            this.DimensionsAndMetadataButton.TabIndex = 7;
            this.DimensionsAndMetadataButton.Text = "Page Dimensions and Metadata";
            this.DimensionsAndMetadataButton.UseVisualStyleBackColor = false;
            this.DimensionsAndMetadataButton.Click += new System.EventHandler(this.DimensionsAndMetadataButton_Click);
            // 
            // KrystalsGroupBox
            // 
            this.KrystalsGroupBox.Controls.Add(this.KrystalsListBox);
            this.KrystalsGroupBox.Controls.Add(this.ShowSelectedKrystalButton);
            this.KrystalsGroupBox.Controls.Add(this.AddKrystalButton);
            this.KrystalsGroupBox.Controls.Add(this.RemoveSelectedKrystalButton);
            this.KrystalsGroupBox.ForeColor = System.Drawing.Color.Brown;
            this.KrystalsGroupBox.Location = new System.Drawing.Point(495, 12);
            this.KrystalsGroupBox.Name = "KrystalsGroupBox";
            this.KrystalsGroupBox.Size = new System.Drawing.Size(218, 222);
            this.KrystalsGroupBox.TabIndex = 2;
            this.KrystalsGroupBox.TabStop = false;
            this.KrystalsGroupBox.Text = "krystals";
            // 
            // PalettesGroupBox
            // 
            this.PalettesGroupBox.Controls.Add(this.DeleteSelectedPaletteButton);
            this.PalettesGroupBox.Controls.Add(this.PalettesListBox);
            this.PalettesGroupBox.Controls.Add(this.ShowSelectedPaletteButton);
            this.PalettesGroupBox.Controls.Add(this.AddPaletteButton);
            this.PalettesGroupBox.Controls.Add(this.AddPercussionPaletteButton);
            this.PalettesGroupBox.ForeColor = System.Drawing.Color.Brown;
            this.PalettesGroupBox.Location = new System.Drawing.Point(495, 238);
            this.PalettesGroupBox.Name = "PalettesGroupBox";
            this.PalettesGroupBox.Size = new System.Drawing.Size(218, 257);
            this.PalettesGroupBox.TabIndex = 3;
            this.PalettesGroupBox.TabStop = false;
            this.PalettesGroupBox.Text = "palettes";
            // 
            // AssistantComposerMainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 14F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(255)))), ((int)(((byte)(245)))));
            this.ClientSize = new System.Drawing.Size(732, 619);
            this.ControlBox = false;
            this.Controls.Add(this.PalettesGroupBox);
            this.Controls.Add(this.KrystalsGroupBox);
            this.Controls.Add(this.DimensionsAndMetadataButton);
            this.Controls.Add(this.ShowMoritzButton);
            this.Controls.Add(this.QuitMoritzButton);
            this.Controls.Add(this.SaveSettingsButton);
            this.Controls.Add(this.QuitAlgorithmButton);
            this.Controls.Add(this.CreateScoreButton);
            this.Controls.Add(this.NotationGroupBox);
            this.Controls.Add(this.ScoreComboBox);
            this.Controls.Add(this.ScoreComboBoxLabel);
            this.Font = new System.Drawing.Font("Arial", 8F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Location = new System.Drawing.Point(250, 100);
            this.Name = "AssistantComposerMainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = " ";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.AssistantComposerMainForm_FormClosing);
            this.NotationGroupBox.ResumeLayout(false);
            this.NotationGroupBox.PerformLayout();
            this.StandardChordsOptionsPanel.ResumeLayout(false);
            this.StandardChordsOptionsPanel.PerformLayout();
            this.KrystalsGroupBox.ResumeLayout(false);
            this.PalettesGroupBox.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

        private System.Windows.Forms.Button CreateScoreButton;
        private System.Windows.Forms.Button QuitAlgorithmButton;
        private System.Windows.Forms.ListBox PalettesListBox;
        private System.Windows.Forms.Button ShowSelectedPaletteButton;
        private System.Windows.Forms.Button AddPaletteButton;
        private System.Windows.Forms.Button AddKrystalButton;
        private System.Windows.Forms.Button DeleteSelectedPaletteButton;
        private System.Windows.Forms.ListBox KrystalsListBox;
        private System.Windows.Forms.Button RemoveSelectedKrystalButton;
        private System.Windows.Forms.Button AddPercussionPaletteButton;
        private System.Windows.Forms.GroupBox NotationGroupBox;
        private System.Windows.Forms.Label MinimumGapsBetweenSystemsLabel;
        private System.Windows.Forms.TextBox MinimumGapsBetweenSystemsTextBox;
        private System.Windows.Forms.TextBox MinimumGapsBetweenStavesTextBox;
        private System.Windows.Forms.Label StaffLineStemStrokeWidthLabel;
        private System.Windows.Forms.ComboBox StafflineStemStrokeWidthComboBox;
        private System.Windows.Forms.ComboBox GapPixelsComboBox;
        private System.Windows.Forms.Label MinimumGapsBetweenStavesLabel;
        private System.Windows.Forms.Label GapPixelsLabel;
        private System.Windows.Forms.TextBox MinimumCrotchetDurationTextBox;
        private System.Windows.Forms.Label MinimumCrotchetDurationLabel;
        private System.Windows.Forms.ComboBox ScoreComboBox;
        private System.Windows.Forms.Label ScoreComboBoxLabel;
        private System.Windows.Forms.Label UnitsHelpLabel;
        private System.Windows.Forms.Button SaveSettingsButton;
        private System.Windows.Forms.Button QuitMoritzButton;
        private System.Windows.Forms.Button ShowMoritzButton;
        private System.Windows.Forms.Button ShowSelectedKrystalButton;
        private System.Windows.Forms.Label SystemStartBarsLabel;
        private System.Windows.Forms.TextBox SystemStartBarsTextBox;
        private System.Windows.Forms.CheckBox BeamsCrossBarlinesCheckBox;
        private System.Windows.Forms.Button DimensionsAndMetadataButton;
        private System.Windows.Forms.GroupBox KrystalsGroupBox;
        private System.Windows.Forms.GroupBox PalettesGroupBox;
        private System.Windows.Forms.TextBox LongStaffNamesTextBox;
        private System.Windows.Forms.TextBox ShortStaffNamesTextBox;
        private System.Windows.Forms.Label ShortStaffNamesHelpLabel;
        private System.Windows.Forms.Label ShortStaffNamesLabel;
        private System.Windows.Forms.Label LongStaffNamesHelpLabel;
        private System.Windows.Forms.Label LongStaffNamesLabel;
        private System.Windows.Forms.Label SystemStartBarsHelpLabel;
        private System.Windows.Forms.Label StafflinesPerStaffHelpLabel;
        private System.Windows.Forms.Label StafflinesPerStaffLabel;
        private System.Windows.Forms.Label ClefsPerStaffLabel;
        private System.Windows.Forms.Label ClefsPerStaffHelpLabel;
        private System.Windows.Forms.Label StaffGroupsHelpLabel;
        private System.Windows.Forms.Label StaffGroupsLabel;
        private System.Windows.Forms.TextBox StafflinesPerStaffTextBox;
        private System.Windows.Forms.TextBox ClefsPerStaffTextBox;
        private System.Windows.Forms.TextBox StaffGroupsTextBox;
        private System.Windows.Forms.Label MidiChannelsPerStaffHelpLabel;
        private System.Windows.Forms.Label MidiChannelsPerStaffLabel;
        private System.Windows.Forms.TextBox MidiChannelsPerStaffTextBox;
        private System.Windows.Forms.Label MidiChannelsPerStaffHelpLabel2;
        private System.Windows.Forms.Panel StandardChordsOptionsPanel;
        private System.Windows.Forms.Label ChordTypeComboBoxLabel;
        private System.Windows.Forms.ComboBox ChordTypeComboBox;
        private System.Windows.Forms.Label InputVoiceIndicesPerStaffHelpLabel2;
        private System.Windows.Forms.Label InputVoiceIndicesPerStaffHelpLabel;
        private System.Windows.Forms.Label InputVoiceIndicesPerStaffLabel;
        private System.Windows.Forms.TextBox InputVoiceIndicesPerStaffTextBox;

    }
}
