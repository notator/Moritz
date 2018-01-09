namespace Moritz.Composer
{
	partial class AssistantComposerForm
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
			this.QuitAssistantComposerButton = new System.Windows.Forms.Button();
			this.PalettesListBox = new System.Windows.Forms.ListBox();
			this.ShowSelectedPaletteButton = new System.Windows.Forms.Button();
			this.AddPaletteButton = new System.Windows.Forms.Button();
			this.AddKrystalButton = new System.Windows.Forms.Button();
			this.DeleteSelectedPaletteButton = new System.Windows.Forms.Button();
			this.KrystalsListBox = new System.Windows.Forms.ListBox();
			this.RemoveSelectedKrystalButton = new System.Windows.Forms.Button();
			this.NotationGroupBox = new System.Windows.Forms.GroupBox();
			this.OutputChordSymbolTypeComboBox = new System.Windows.Forms.ComboBox();
			this.OutputChordSymbolTypeLabel = new System.Windows.Forms.Label();
			this.MinimumCrotchetDurationTextBox = new System.Windows.Forms.TextBox();
			this.BeamsCrossBarlinesCheckBox = new System.Windows.Forms.CheckBox();
			this.MinimumCrotchetDurationLabel = new System.Windows.Forms.Label();
			this.VoiceIndicesHelpLabelLabel = new System.Windows.Forms.Label();
			this.ShortStaffNamesHelpLabel = new System.Windows.Forms.Label();
			this.LongStaffNamesHelpLabel = new System.Windows.Forms.Label();
			this.VoiceIndicesPerStaffHelpLabel = new System.Windows.Forms.Label();
			this.ShortStaffNamesLabel = new System.Windows.Forms.Label();
			this.LongStaffNamesTextBox = new System.Windows.Forms.TextBox();
			this.VoiceIndicesHelpLabel = new System.Windows.Forms.Label();
			this.SystemStartBarsHelpLabel = new System.Windows.Forms.Label();
			this.ShortStaffNamesTextBox = new System.Windows.Forms.TextBox();
			this.StafflinesPerStaffHelpLabel = new System.Windows.Forms.Label();
			this.VoiceIndicesPerStaffLabel = new System.Windows.Forms.Label();
			this.StafflinesPerStaffLabel = new System.Windows.Forms.Label();
			this.LongStaffNamesLabel = new System.Windows.Forms.Label();
			this.VoiceIndicesPerStaffTextBox = new System.Windows.Forms.TextBox();
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
			this.ConfirmNotationButton = new System.Windows.Forms.Button();
			this.RevertNotationButton = new System.Windows.Forms.Button();
			this.SaveSettingsCreateScoreButton = new System.Windows.Forms.Button();
			this.QuitMoritzButton = new System.Windows.Forms.Button();
			this.ShowSelectedKrystalButton = new System.Windows.Forms.Button();
			this.DimensionsAndMetadataButton = new System.Windows.Forms.Button();
			this.KrystalsGroupBox = new System.Windows.Forms.GroupBox();
			this.ConfirmKrystalsListButton = new System.Windows.Forms.Button();
			this.RevertKrystalsListButton = new System.Windows.Forms.Button();
			this.PalettesGroupBox = new System.Windows.Forms.GroupBox();
			this.RevertPalettesListButton = new System.Windows.Forms.Button();
			this.RevertEverythingButton = new System.Windows.Forms.Button();
			this.ShowUncheckedFormsButton = new System.Windows.Forms.Button();
			this.ShowConfirmedFormsButton = new System.Windows.Forms.Button();
			this.NotationGroupBox.SuspendLayout();
			this.KrystalsGroupBox.SuspendLayout();
			this.PalettesGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// QuitAssistantComposerButton
			// 
			this.QuitAssistantComposerButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.QuitAssistantComposerButton.Font = new System.Drawing.Font("Arial", 8F);
			this.QuitAssistantComposerButton.Location = new System.Drawing.Point(490, 504);
			this.QuitAssistantComposerButton.Name = "QuitAssistantComposerButton";
			this.QuitAssistantComposerButton.Size = new System.Drawing.Size(325, 26);
			this.QuitAssistantComposerButton.TabIndex = 2;
			this.QuitAssistantComposerButton.Text = "Quit algorithm";
			this.QuitAssistantComposerButton.UseVisualStyleBackColor = true;
			this.QuitAssistantComposerButton.Click += new System.EventHandler(this.QuitAssistantComposerButton_Click);
			// 
			// PalettesListBox
			// 
			this.PalettesListBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.PalettesListBox.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
			this.PalettesListBox.FormattingEnabled = true;
			this.PalettesListBox.ItemHeight = 14;
			this.PalettesListBox.Location = new System.Drawing.Point(16, 23);
			this.PalettesListBox.Name = "PalettesListBox";
			this.PalettesListBox.Size = new System.Drawing.Size(183, 242);
			this.PalettesListBox.TabIndex = 4;
			this.PalettesListBox.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.PalettesListBox_DrawItem);
			this.PalettesListBox.SelectedIndexChanged += new System.EventHandler(this.PalettesListBox_SelectedIndexChanged);
			// 
			// ShowSelectedPaletteButton
			// 
			this.ShowSelectedPaletteButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.ShowSelectedPaletteButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(215)))), ((int)(((byte)(225)))), ((int)(((byte)(215)))));
			this.ShowSelectedPaletteButton.Enabled = false;
			this.ShowSelectedPaletteButton.Font = new System.Drawing.Font("Arial", 8F);
			this.ShowSelectedPaletteButton.ForeColor = System.Drawing.SystemColors.ControlText;
			this.ShowSelectedPaletteButton.Location = new System.Drawing.Point(16, 271);
			this.ShowSelectedPaletteButton.Name = "ShowSelectedPaletteButton";
			this.ShowSelectedPaletteButton.Size = new System.Drawing.Size(183, 27);
			this.ShowSelectedPaletteButton.TabIndex = 0;
			this.ShowSelectedPaletteButton.Text = "show selected palette";
			this.ShowSelectedPaletteButton.UseVisualStyleBackColor = false;
			this.ShowSelectedPaletteButton.Click += new System.EventHandler(this.ShowSelectedPaletteButton_Click);
			// 
			// AddPaletteButton
			// 
			this.AddPaletteButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.AddPaletteButton.Font = new System.Drawing.Font("Arial", 8F);
			this.AddPaletteButton.ForeColor = System.Drawing.SystemColors.ControlText;
			this.AddPaletteButton.Location = new System.Drawing.Point(16, 303);
			this.AddPaletteButton.Name = "AddPaletteButton";
			this.AddPaletteButton.Size = new System.Drawing.Size(183, 27);
			this.AddPaletteButton.TabIndex = 3;
			this.AddPaletteButton.Text = "add palette";
			this.AddPaletteButton.UseVisualStyleBackColor = true;
			this.AddPaletteButton.Click += new System.EventHandler(this.AddPaletteButton_Click);
			// 
			// AddKrystalButton
			// 
			this.AddKrystalButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.AddKrystalButton.Font = new System.Drawing.Font("Arial", 8F);
			this.AddKrystalButton.ForeColor = System.Drawing.SystemColors.ControlText;
			this.AddKrystalButton.Location = new System.Drawing.Point(16, 304);
			this.AddKrystalButton.Name = "AddKrystalButton";
			this.AddKrystalButton.Size = new System.Drawing.Size(183, 26);
			this.AddKrystalButton.TabIndex = 3;
			this.AddKrystalButton.Text = "add krystal";
			this.AddKrystalButton.UseVisualStyleBackColor = true;
			this.AddKrystalButton.Click += new System.EventHandler(this.AddKrystalButton_Click);
			// 
			// DeleteSelectedPaletteButton
			// 
			this.DeleteSelectedPaletteButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.DeleteSelectedPaletteButton.Enabled = false;
			this.DeleteSelectedPaletteButton.Font = new System.Drawing.Font("Arial", 8F);
			this.DeleteSelectedPaletteButton.ForeColor = System.Drawing.SystemColors.ControlText;
			this.DeleteSelectedPaletteButton.Location = new System.Drawing.Point(16, 335);
			this.DeleteSelectedPaletteButton.Name = "DeleteSelectedPaletteButton";
			this.DeleteSelectedPaletteButton.Size = new System.Drawing.Size(183, 27);
			this.DeleteSelectedPaletteButton.TabIndex = 5;
			this.DeleteSelectedPaletteButton.Text = "delete selected palette";
			this.DeleteSelectedPaletteButton.UseVisualStyleBackColor = true;
			this.DeleteSelectedPaletteButton.Click += new System.EventHandler(this.DeletePaletteButton_Click);
			// 
			// KrystalsListBox
			// 
			this.KrystalsListBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.KrystalsListBox.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
			this.KrystalsListBox.FormattingEnabled = true;
			this.KrystalsListBox.ItemHeight = 14;
			this.KrystalsListBox.Location = new System.Drawing.Point(16, 23);
			this.KrystalsListBox.Name = "KrystalsListBox";
			this.KrystalsListBox.Size = new System.Drawing.Size(183, 242);
			this.KrystalsListBox.TabIndex = 4;
			this.KrystalsListBox.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.KrystalsListBox_DrawItem);
			this.KrystalsListBox.SelectedIndexChanged += new System.EventHandler(this.KrystalsListBox_SelectedIndexChanged);
			// 
			// RemoveSelectedKrystalButton
			// 
			this.RemoveSelectedKrystalButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.RemoveSelectedKrystalButton.Enabled = false;
			this.RemoveSelectedKrystalButton.Font = new System.Drawing.Font("Arial", 8F);
			this.RemoveSelectedKrystalButton.ForeColor = System.Drawing.SystemColors.ControlText;
			this.RemoveSelectedKrystalButton.Location = new System.Drawing.Point(16, 336);
			this.RemoveSelectedKrystalButton.Name = "RemoveSelectedKrystalButton";
			this.RemoveSelectedKrystalButton.Size = new System.Drawing.Size(183, 26);
			this.RemoveSelectedKrystalButton.TabIndex = 5;
			this.RemoveSelectedKrystalButton.Text = "remove selected krystal";
			this.RemoveSelectedKrystalButton.UseVisualStyleBackColor = true;
			this.RemoveSelectedKrystalButton.Click += new System.EventHandler(this.RemoveSelectedKrystalButton_Click);
			// 
			// NotationGroupBox
			// 
			this.NotationGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.NotationGroupBox.Controls.Add(this.OutputChordSymbolTypeComboBox);
			this.NotationGroupBox.Controls.Add(this.OutputChordSymbolTypeLabel);
			this.NotationGroupBox.Controls.Add(this.MinimumCrotchetDurationTextBox);
			this.NotationGroupBox.Controls.Add(this.BeamsCrossBarlinesCheckBox);
			this.NotationGroupBox.Controls.Add(this.MinimumCrotchetDurationLabel);
			this.NotationGroupBox.Controls.Add(this.VoiceIndicesHelpLabelLabel);
			this.NotationGroupBox.Controls.Add(this.ShortStaffNamesHelpLabel);
			this.NotationGroupBox.Controls.Add(this.LongStaffNamesHelpLabel);
			this.NotationGroupBox.Controls.Add(this.VoiceIndicesPerStaffHelpLabel);
			this.NotationGroupBox.Controls.Add(this.ShortStaffNamesLabel);
			this.NotationGroupBox.Controls.Add(this.LongStaffNamesTextBox);
			this.NotationGroupBox.Controls.Add(this.VoiceIndicesHelpLabel);
			this.NotationGroupBox.Controls.Add(this.SystemStartBarsHelpLabel);
			this.NotationGroupBox.Controls.Add(this.ShortStaffNamesTextBox);
			this.NotationGroupBox.Controls.Add(this.StafflinesPerStaffHelpLabel);
			this.NotationGroupBox.Controls.Add(this.VoiceIndicesPerStaffLabel);
			this.NotationGroupBox.Controls.Add(this.StafflinesPerStaffLabel);
			this.NotationGroupBox.Controls.Add(this.LongStaffNamesLabel);
			this.NotationGroupBox.Controls.Add(this.VoiceIndicesPerStaffTextBox);
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
			this.NotationGroupBox.Location = new System.Drawing.Point(15, 8);
			this.NotationGroupBox.Name = "NotationGroupBox";
			this.NotationGroupBox.Size = new System.Drawing.Size(466, 490);
			this.NotationGroupBox.TabIndex = 9;
			this.NotationGroupBox.TabStop = false;
			this.NotationGroupBox.Text = "notation";
			// 
			// OutputChordSymbolTypeComboBox
			// 
			this.OutputChordSymbolTypeComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.OutputChordSymbolTypeComboBox.FormattingEnabled = true;
			this.OutputChordSymbolTypeComboBox.Items.AddRange(new object[] {
            "standard black",
            "coloured velocities"});
			this.OutputChordSymbolTypeComboBox.Location = new System.Drawing.Point(127, 21);
			this.OutputChordSymbolTypeComboBox.Name = "OutputChordSymbolTypeComboBox";
			this.OutputChordSymbolTypeComboBox.Size = new System.Drawing.Size(120, 22);
			this.OutputChordSymbolTypeComboBox.TabIndex = 192;
			this.OutputChordSymbolTypeComboBox.SelectedIndexChanged += new System.EventHandler(this.NotatorComboBox_SelectedIndexChanged);
			// 
			// OutputChordSymbolTypeLabel
			// 
			this.OutputChordSymbolTypeLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.OutputChordSymbolTypeLabel.AutoSize = true;
			this.OutputChordSymbolTypeLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(60)))), ((int)(((byte)(60)))));
			this.OutputChordSymbolTypeLabel.Location = new System.Drawing.Point(57, 18);
			this.OutputChordSymbolTypeLabel.Margin = new System.Windows.Forms.Padding(0);
			this.OutputChordSymbolTypeLabel.Name = "OutputChordSymbolTypeLabel";
			this.OutputChordSymbolTypeLabel.Size = new System.Drawing.Size(68, 28);
			this.OutputChordSymbolTypeLabel.TabIndex = 191;
			this.OutputChordSymbolTypeLabel.Text = "output chord\r\nsymbol type";
			this.OutputChordSymbolTypeLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// MinimumCrotchetDurationTextBox
			// 
			this.MinimumCrotchetDurationTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.MinimumCrotchetDurationTextBox.Location = new System.Drawing.Point(128, 58);
			this.MinimumCrotchetDurationTextBox.Name = "MinimumCrotchetDurationTextBox";
			this.MinimumCrotchetDurationTextBox.Size = new System.Drawing.Size(68, 20);
			this.MinimumCrotchetDurationTextBox.TabIndex = 0;
			this.MinimumCrotchetDurationTextBox.TextChanged += new System.EventHandler(this.MinimumCrotchetDurationTextBox_TextChanged);
			this.MinimumCrotchetDurationTextBox.Leave += new System.EventHandler(this.MinimumCrotchetDurationTextBox_Leave);
			// 
			// BeamsCrossBarlinesCheckBox
			// 
			this.BeamsCrossBarlinesCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.BeamsCrossBarlinesCheckBox.AutoSize = true;
			this.BeamsCrossBarlinesCheckBox.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.BeamsCrossBarlinesCheckBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(60)))), ((int)(((byte)(60)))));
			this.BeamsCrossBarlinesCheckBox.Location = new System.Drawing.Point(38, 93);
			this.BeamsCrossBarlinesCheckBox.Name = "BeamsCrossBarlinesCheckBox";
			this.BeamsCrossBarlinesCheckBox.Size = new System.Drawing.Size(130, 18);
			this.BeamsCrossBarlinesCheckBox.TabIndex = 1;
			this.BeamsCrossBarlinesCheckBox.Text = "beams cross barlines";
			this.BeamsCrossBarlinesCheckBox.UseVisualStyleBackColor = true;
			this.BeamsCrossBarlinesCheckBox.CheckedChanged += new System.EventHandler(this.BeamsCrossBarlinesCheckBox_CheckedChanged);
			// 
			// MinimumCrotchetDurationLabel
			// 
			this.MinimumCrotchetDurationLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.MinimumCrotchetDurationLabel.AutoSize = true;
			this.MinimumCrotchetDurationLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(60)))), ((int)(((byte)(60)))));
			this.MinimumCrotchetDurationLabel.Location = new System.Drawing.Point(35, 54);
			this.MinimumCrotchetDurationLabel.Margin = new System.Windows.Forms.Padding(0);
			this.MinimumCrotchetDurationLabel.Name = "MinimumCrotchetDurationLabel";
			this.MinimumCrotchetDurationLabel.Size = new System.Drawing.Size(90, 28);
			this.MinimumCrotchetDurationLabel.TabIndex = 147;
			this.MinimumCrotchetDurationLabel.Text = "minimum crotchet\r\nduration (ms)";
			this.MinimumCrotchetDurationLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// VoiceIndicesHelpLabelLabel
			// 
			this.VoiceIndicesHelpLabelLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.VoiceIndicesHelpLabelLabel.AutoSize = true;
			this.VoiceIndicesHelpLabelLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(60)))), ((int)(((byte)(60)))));
			this.VoiceIndicesHelpLabelLabel.Location = new System.Drawing.Point(111, 145);
			this.VoiceIndicesHelpLabelLabel.Margin = new System.Windows.Forms.Padding(0);
			this.VoiceIndicesHelpLabelLabel.Name = "VoiceIndicesHelpLabelLabel";
			this.VoiceIndicesHelpLabelLabel.Size = new System.Drawing.Size(70, 14);
			this.VoiceIndicesHelpLabelLabel.TabIndex = 190;
			this.VoiceIndicesHelpLabelLabel.Text = "voice indices";
			this.VoiceIndicesHelpLabelLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// ShortStaffNamesHelpLabel
			// 
			this.ShortStaffNamesHelpLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.ShortStaffNamesHelpLabel.AutoSize = true;
			this.ShortStaffNamesHelpLabel.ForeColor = System.Drawing.Color.RoyalBlue;
			this.ShortStaffNamesHelpLabel.Location = new System.Drawing.Point(100, 387);
			this.ShortStaffNamesHelpLabel.Name = "ShortStaffNamesHelpLabel";
			this.ShortStaffNamesHelpLabel.Size = new System.Drawing.Size(125, 14);
			this.ShortStaffNamesHelpLabel.TabIndex = 176;
			this.ShortStaffNamesHelpLabel.Text = "(x names, one per staff)";
			// 
			// LongStaffNamesHelpLabel
			// 
			this.LongStaffNamesHelpLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.LongStaffNamesHelpLabel.AutoSize = true;
			this.LongStaffNamesHelpLabel.ForeColor = System.Drawing.Color.RoyalBlue;
			this.LongStaffNamesHelpLabel.Location = new System.Drawing.Point(97, 344);
			this.LongStaffNamesHelpLabel.Name = "LongStaffNamesHelpLabel";
			this.LongStaffNamesHelpLabel.Size = new System.Drawing.Size(125, 14);
			this.LongStaffNamesHelpLabel.TabIndex = 174;
			this.LongStaffNamesHelpLabel.Text = "(x names, one per staff)";
			// 
			// VoiceIndicesPerStaffHelpLabel
			// 
			this.VoiceIndicesPerStaffHelpLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.VoiceIndicesPerStaffHelpLabel.AutoSize = true;
			this.VoiceIndicesPerStaffHelpLabel.ForeColor = System.Drawing.Color.RoyalBlue;
			this.VoiceIndicesPerStaffHelpLabel.Location = new System.Drawing.Point(250, 180);
			this.VoiceIndicesPerStaffHelpLabel.Name = "VoiceIndicesPerStaffHelpLabel";
			this.VoiceIndicesPerStaffHelpLabel.Size = new System.Drawing.Size(141, 14);
			this.VoiceIndicesPerStaffHelpLabel.TabIndex = 183;
			this.VoiceIndicesPerStaffHelpLabel.Text = "(right click this text for help)";
			this.VoiceIndicesPerStaffHelpLabel.MouseClick += new System.Windows.Forms.MouseEventHandler(this.VoiceIndicesPerStaffHelp_MouseClick);
			// 
			// ShortStaffNamesLabel
			// 
			this.ShortStaffNamesLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.ShortStaffNamesLabel.AutoSize = true;
			this.ShortStaffNamesLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(60)))), ((int)(((byte)(60)))));
			this.ShortStaffNamesLabel.Location = new System.Drawing.Point(10, 387);
			this.ShortStaffNamesLabel.Margin = new System.Windows.Forms.Padding(0);
			this.ShortStaffNamesLabel.Name = "ShortStaffNamesLabel";
			this.ShortStaffNamesLabel.Size = new System.Drawing.Size(93, 14);
			this.ShortStaffNamesLabel.TabIndex = 175;
			this.ShortStaffNamesLabel.Text = "short staff names";
			// 
			// LongStaffNamesTextBox
			// 
			this.LongStaffNamesTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.LongStaffNamesTextBox.Location = new System.Drawing.Point(10, 361);
			this.LongStaffNamesTextBox.Name = "LongStaffNamesTextBox";
			this.LongStaffNamesTextBox.Size = new System.Drawing.Size(440, 20);
			this.LongStaffNamesTextBox.TabIndex = 14;
			this.LongStaffNamesTextBox.TextChanged += new System.EventHandler(this.LongStaffNamesTextBox_TextChanged);
			this.LongStaffNamesTextBox.Leave += new System.EventHandler(this.LongStaffNamesTextBox_Leave);
			// 
			// VoiceIndicesHelpLabel
			// 
			this.VoiceIndicesHelpLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.VoiceIndicesHelpLabel.AutoSize = true;
			this.VoiceIndicesHelpLabel.ForeColor = System.Drawing.Color.RoyalBlue;
			this.VoiceIndicesHelpLabel.Location = new System.Drawing.Point(111, 160);
			this.VoiceIndicesHelpLabel.Name = "VoiceIndicesHelpLabel";
			this.VoiceIndicesHelpLabel.Size = new System.Drawing.Size(45, 14);
			this.VoiceIndicesHelpLabel.TabIndex = 182;
			this.VoiceIndicesHelpLabel.Text = "0 1 2 | 0";
			// 
			// SystemStartBarsHelpLabel
			// 
			this.SystemStartBarsHelpLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.SystemStartBarsHelpLabel.AutoSize = true;
			this.SystemStartBarsHelpLabel.ForeColor = System.Drawing.Color.RoyalBlue;
			this.SystemStartBarsHelpLabel.Location = new System.Drawing.Point(139, 442);
			this.SystemStartBarsHelpLabel.Name = "SystemStartBarsHelpLabel";
			this.SystemStartBarsHelpLabel.Size = new System.Drawing.Size(144, 14);
			this.SystemStartBarsHelpLabel.TabIndex = 179;
			this.SystemStartBarsHelpLabel.Text = "(default is 1 bar per system)";
			// 
			// ShortStaffNamesTextBox
			// 
			this.ShortStaffNamesTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.ShortStaffNamesTextBox.Location = new System.Drawing.Point(10, 403);
			this.ShortStaffNamesTextBox.Name = "ShortStaffNamesTextBox";
			this.ShortStaffNamesTextBox.Size = new System.Drawing.Size(440, 20);
			this.ShortStaffNamesTextBox.TabIndex = 15;
			this.ShortStaffNamesTextBox.TextChanged += new System.EventHandler(this.ShortStaffNamesTextBox_TextChanged);
			this.ShortStaffNamesTextBox.Leave += new System.EventHandler(this.ShortStaffNamesTextBox_Leave);
			// 
			// StafflinesPerStaffHelpLabel
			// 
			this.StafflinesPerStaffHelpLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.StafflinesPerStaffHelpLabel.AutoSize = true;
			this.StafflinesPerStaffHelpLabel.ForeColor = System.Drawing.Color.RoyalBlue;
			this.StafflinesPerStaffHelpLabel.Location = new System.Drawing.Point(250, 255);
			this.StafflinesPerStaffHelpLabel.Name = "StafflinesPerStaffHelpLabel";
			this.StafflinesPerStaffHelpLabel.Size = new System.Drawing.Size(194, 28);
			this.StafflinesPerStaffHelpLabel.TabIndex = 178;
			this.StafflinesPerStaffHelpLabel.Text = "x integer values separated by commas\r\nstandard clefs must have 5 lines.";
			// 
			// VoiceIndicesPerStaffLabel
			// 
			this.VoiceIndicesPerStaffLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.VoiceIndicesPerStaffLabel.AutoSize = true;
			this.VoiceIndicesPerStaffLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(60)))), ((int)(((byte)(60)))));
			this.VoiceIndicesPerStaffLabel.Location = new System.Drawing.Point(24, 180);
			this.VoiceIndicesPerStaffLabel.Margin = new System.Windows.Forms.Padding(0);
			this.VoiceIndicesPerStaffLabel.Name = "VoiceIndicesPerStaffLabel";
			this.VoiceIndicesPerStaffLabel.Size = new System.Drawing.Size(84, 14);
			this.VoiceIndicesPerStaffLabel.TabIndex = 3;
			this.VoiceIndicesPerStaffLabel.Text = "voices per staff";
			this.VoiceIndicesPerStaffLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// StafflinesPerStaffLabel
			// 
			this.StafflinesPerStaffLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.StafflinesPerStaffLabel.AutoSize = true;
			this.StafflinesPerStaffLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(60)))), ((int)(((byte)(60)))));
			this.StafflinesPerStaffLabel.Location = new System.Drawing.Point(11, 264);
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
			this.LongStaffNamesLabel.Location = new System.Drawing.Point(10, 344);
			this.LongStaffNamesLabel.Margin = new System.Windows.Forms.Padding(0);
			this.LongStaffNamesLabel.Name = "LongStaffNamesLabel";
			this.LongStaffNamesLabel.Size = new System.Drawing.Size(88, 14);
			this.LongStaffNamesLabel.TabIndex = 173;
			this.LongStaffNamesLabel.Text = "long staff names";
			// 
			// VoiceIndicesPerStaffTextBox
			// 
			this.VoiceIndicesPerStaffTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.VoiceIndicesPerStaffTextBox.Location = new System.Drawing.Point(111, 177);
			this.VoiceIndicesPerStaffTextBox.Name = "VoiceIndicesPerStaffTextBox";
			this.VoiceIndicesPerStaffTextBox.Size = new System.Drawing.Size(136, 20);
			this.VoiceIndicesPerStaffTextBox.TabIndex = 9;
			this.VoiceIndicesPerStaffTextBox.TextChanged += new System.EventHandler(this.VoiceIndicesPerStaffTextBox_TextChanged);
			this.VoiceIndicesPerStaffTextBox.Leave += new System.EventHandler(this.VoiceIndicesPerStaffTextBox_Leave);
			// 
			// ClefsPerStaffLabel
			// 
			this.ClefsPerStaffLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.ClefsPerStaffLabel.AutoSize = true;
			this.ClefsPerStaffLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(60)))), ((int)(((byte)(60)))));
			this.ClefsPerStaffLabel.Location = new System.Drawing.Point(32, 222);
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
			this.ClefsPerStaffHelpLabel.Location = new System.Drawing.Point(250, 214);
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
			this.StaffGroupsHelpLabel.Location = new System.Drawing.Point(250, 297);
			this.StaffGroupsHelpLabel.Name = "StaffGroupsHelpLabel";
			this.StaffGroupsHelpLabel.Size = new System.Drawing.Size(147, 42);
			this.StaffGroupsHelpLabel.TabIndex = 167;
			this.StaffGroupsHelpLabel.Text = "integer values (total = x).\r\nstaff groups cannot contain\r\nboth input and output s" +
    "taves.";
			// 
			// StaffGroupsLabel
			// 
			this.StaffGroupsLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.StaffGroupsLabel.AutoSize = true;
			this.StaffGroupsLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(60)))), ((int)(((byte)(60)))));
			this.StaffGroupsLabel.Location = new System.Drawing.Point(41, 306);
			this.StaffGroupsLabel.Margin = new System.Windows.Forms.Padding(0);
			this.StaffGroupsLabel.Name = "StaffGroupsLabel";
			this.StaffGroupsLabel.Size = new System.Drawing.Size(67, 14);
			this.StaffGroupsLabel.TabIndex = 166;
			this.StaffGroupsLabel.Text = "staff groups";
			// 
			// StafflinesPerStaffTextBox
			// 
			this.StafflinesPerStaffTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.StafflinesPerStaffTextBox.Location = new System.Drawing.Point(111, 261);
			this.StafflinesPerStaffTextBox.Name = "StafflinesPerStaffTextBox";
			this.StafflinesPerStaffTextBox.Size = new System.Drawing.Size(136, 20);
			this.StafflinesPerStaffTextBox.TabIndex = 12;
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
			this.SystemStartBarsLabel.TabIndex = 17;
			this.SystemStartBarsLabel.Text = "system start bar numbers";
			this.SystemStartBarsLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// SystemStartBarsTextBox
			// 
			this.SystemStartBarsTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.SystemStartBarsTextBox.Location = new System.Drawing.Point(10, 459);
			this.SystemStartBarsTextBox.Name = "SystemStartBarsTextBox";
			this.SystemStartBarsTextBox.Size = new System.Drawing.Size(440, 20);
			this.SystemStartBarsTextBox.TabIndex = 16;
			this.SystemStartBarsTextBox.TextChanged += new System.EventHandler(this.SystemStartBarsTextBox_TextChanged);
			this.SystemStartBarsTextBox.Leave += new System.EventHandler(this.SystemStartBarsTextBox_Leave);
			// 
			// ClefsPerStaffTextBox
			// 
			this.ClefsPerStaffTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.ClefsPerStaffTextBox.Location = new System.Drawing.Point(111, 219);
			this.ClefsPerStaffTextBox.Name = "ClefsPerStaffTextBox";
			this.ClefsPerStaffTextBox.Size = new System.Drawing.Size(136, 20);
			this.ClefsPerStaffTextBox.TabIndex = 11;
			this.ClefsPerStaffTextBox.TextChanged += new System.EventHandler(this.ClefsPerStaffTextBox_TextChanged);
			this.ClefsPerStaffTextBox.Leave += new System.EventHandler(this.ClefsPerStaffTextBox_Leave);
			// 
			// StaffGroupsTextBox
			// 
			this.StaffGroupsTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.StaffGroupsTextBox.Location = new System.Drawing.Point(111, 303);
			this.StaffGroupsTextBox.Name = "StaffGroupsTextBox";
			this.StaffGroupsTextBox.Size = new System.Drawing.Size(136, 20);
			this.StaffGroupsTextBox.TabIndex = 13;
			this.StaffGroupsTextBox.TextChanged += new System.EventHandler(this.StaffGroupsTextBox_TextChanged);
			this.StaffGroupsTextBox.Leave += new System.EventHandler(this.StaffGroupsTextBox_Leave);
			// 
			// UnitsHelpLabel
			// 
			this.UnitsHelpLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.UnitsHelpLabel.AutoSize = true;
			this.UnitsHelpLabel.ForeColor = System.Drawing.Color.RoyalBlue;
			this.UnitsHelpLabel.Location = new System.Drawing.Point(304, 133);
			this.UnitsHelpLabel.Name = "UnitsHelpLabel";
			this.UnitsHelpLabel.Size = new System.Drawing.Size(152, 14);
			this.UnitsHelpLabel.TabIndex = 133;
			this.UnitsHelpLabel.Text = "*screen pixels (100% display)";
			// 
			// MinimumGapsBetweenSystemsTextBox
			// 
			this.MinimumGapsBetweenSystemsTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.MinimumGapsBetweenSystemsTextBox.Location = new System.Drawing.Point(394, 109);
			this.MinimumGapsBetweenSystemsTextBox.Name = "MinimumGapsBetweenSystemsTextBox";
			this.MinimumGapsBetweenSystemsTextBox.Size = new System.Drawing.Size(58, 20);
			this.MinimumGapsBetweenSystemsTextBox.TabIndex = 8;
			this.MinimumGapsBetweenSystemsTextBox.TextChanged += new System.EventHandler(this.MinimumGapsBetweenSystemsTextBox_TextChanged);
			this.MinimumGapsBetweenSystemsTextBox.Leave += new System.EventHandler(this.MinimumGapsBetweenSystemsTextBox_Leave);
			// 
			// MinimumGapsBetweenStavesTextBox
			// 
			this.MinimumGapsBetweenStavesTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.MinimumGapsBetweenStavesTextBox.Location = new System.Drawing.Point(394, 81);
			this.MinimumGapsBetweenStavesTextBox.Name = "MinimumGapsBetweenStavesTextBox";
			this.MinimumGapsBetweenStavesTextBox.Size = new System.Drawing.Size(58, 20);
			this.MinimumGapsBetweenStavesTextBox.TabIndex = 7;
			this.MinimumGapsBetweenStavesTextBox.TextChanged += new System.EventHandler(this.MinimumGapsBetweenStavesTextBox_TextChanged);
			this.MinimumGapsBetweenStavesTextBox.Leave += new System.EventHandler(this.MinimumGapsBetweenStavesTextBox_Leave);
			// 
			// StafflineStemStrokeWidthComboBox
			// 
			this.StafflineStemStrokeWidthComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.StafflineStemStrokeWidthComboBox.FormattingEnabled = true;
			this.StafflineStemStrokeWidthComboBox.Items.AddRange(new object[] {
            "0.25",
            "0.5",
            "1.0",
            "1.5",
            "2.0"});
			this.StafflineStemStrokeWidthComboBox.Location = new System.Drawing.Point(394, 21);
			this.StafflineStemStrokeWidthComboBox.Name = "StafflineStemStrokeWidthComboBox";
			this.StafflineStemStrokeWidthComboBox.Size = new System.Drawing.Size(58, 22);
			this.StafflineStemStrokeWidthComboBox.TabIndex = 5;
			this.StafflineStemStrokeWidthComboBox.SelectedIndexChanged += new System.EventHandler(this.StafflineStemStrokeWidthComboBox_SelectedIndexChanged);
			this.StafflineStemStrokeWidthComboBox.Leave += new System.EventHandler(this.StafflineStemStrokeWidthComboBox_Leave);
			// 
			// GapPixelsComboBox
			// 
			this.GapPixelsComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.GapPixelsComboBox.FormattingEnabled = true;
			this.GapPixelsComboBox.Items.AddRange(new object[] {
            "1",
            "2",
            "2.5",
            "3",
            "3.5",
            "4",
            "5",
            "6",
            "7",
            "8",
            "10",
            "12",
            "14",
            "16",
            "18",
            "20",
            "24",
            "28"});
			this.GapPixelsComboBox.Location = new System.Drawing.Point(394, 51);
			this.GapPixelsComboBox.Name = "GapPixelsComboBox";
			this.GapPixelsComboBox.Size = new System.Drawing.Size(58, 22);
			this.GapPixelsComboBox.TabIndex = 6;
			this.GapPixelsComboBox.SelectedIndexChanged += new System.EventHandler(this.GapPixelsComboBox_SelectedIndexChanged);
			this.GapPixelsComboBox.Leave += new System.EventHandler(this.GapPixelsComboBox_Leave);
			// 
			// MinimumGapsBetweenSystemsLabel
			// 
			this.MinimumGapsBetweenSystemsLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.MinimumGapsBetweenSystemsLabel.AutoSize = true;
			this.MinimumGapsBetweenSystemsLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(60)))), ((int)(((byte)(60)))));
			this.MinimumGapsBetweenSystemsLabel.Location = new System.Drawing.Point(217, 112);
			this.MinimumGapsBetweenSystemsLabel.Margin = new System.Windows.Forms.Padding(0);
			this.MinimumGapsBetweenSystemsLabel.Name = "MinimumGapsBetweenSystemsLabel";
			this.MinimumGapsBetweenSystemsLabel.Size = new System.Drawing.Size(176, 14);
			this.MinimumGapsBetweenSystemsLabel.TabIndex = 103;
			this.MinimumGapsBetweenSystemsLabel.Text = "(minimum) gaps between systems*";
			// 
			// StaffLineStemStrokeWidthLabel
			// 
			this.StaffLineStemStrokeWidthLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.StaffLineStemStrokeWidthLabel.AutoSize = true;
			this.StaffLineStemStrokeWidthLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(60)))), ((int)(((byte)(60)))));
			this.StaffLineStemStrokeWidthLabel.Location = new System.Drawing.Point(297, 18);
			this.StaffLineStemStrokeWidthLabel.Margin = new System.Windows.Forms.Padding(0);
			this.StaffLineStemStrokeWidthLabel.Name = "StaffLineStemStrokeWidthLabel";
			this.StaffLineStemStrokeWidthLabel.Size = new System.Drawing.Size(96, 28);
			this.StaffLineStemStrokeWidthLabel.TabIndex = 111;
			this.StaffLineStemStrokeWidthLabel.Text = "staff line and stem\r\nstroke width*";
			this.StaffLineStemStrokeWidthLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// MinimumGapsBetweenStavesLabel
			// 
			this.MinimumGapsBetweenStavesLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.MinimumGapsBetweenStavesLabel.AutoSize = true;
			this.MinimumGapsBetweenStavesLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(60)))), ((int)(((byte)(60)))));
			this.MinimumGapsBetweenStavesLabel.Location = new System.Drawing.Point(225, 84);
			this.MinimumGapsBetweenStavesLabel.Margin = new System.Windows.Forms.Padding(0);
			this.MinimumGapsBetweenStavesLabel.Name = "MinimumGapsBetweenStavesLabel";
			this.MinimumGapsBetweenStavesLabel.Size = new System.Drawing.Size(168, 14);
			this.MinimumGapsBetweenStavesLabel.TabIndex = 107;
			this.MinimumGapsBetweenStavesLabel.Text = "(minimum) gaps between staves*";
			// 
			// GapPixelsLabel
			// 
			this.GapPixelsLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.GapPixelsLabel.AutoSize = true;
			this.GapPixelsLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(60)))), ((int)(((byte)(60)))));
			this.GapPixelsLabel.Location = new System.Drawing.Point(274, 55);
			this.GapPixelsLabel.Margin = new System.Windows.Forms.Padding(0);
			this.GapPixelsLabel.Name = "GapPixelsLabel";
			this.GapPixelsLabel.Size = new System.Drawing.Size(119, 14);
			this.GapPixelsLabel.TabIndex = 79;
			this.GapPixelsLabel.Text = "(output staff) gap size*";
			// 
			// ConfirmNotationButton
			// 
			this.ConfirmNotationButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.ConfirmNotationButton.BackColor = System.Drawing.Color.Transparent;
			this.ConfirmNotationButton.Enabled = false;
			this.ConfirmNotationButton.Font = new System.Drawing.Font("Arial", 8F);
			this.ConfirmNotationButton.ForeColor = System.Drawing.Color.Blue;
			this.ConfirmNotationButton.Location = new System.Drawing.Point(362, 504);
			this.ConfirmNotationButton.Name = "ConfirmNotationButton";
			this.ConfirmNotationButton.Size = new System.Drawing.Size(103, 26);
			this.ConfirmNotationButton.TabIndex = 1;
			this.ConfirmNotationButton.Text = "confirm notation";
			this.ConfirmNotationButton.UseVisualStyleBackColor = false;
			this.ConfirmNotationButton.Click += new System.EventHandler(this.ConfirmNotationButton_Click);
			// 
			// RevertNotationButton
			// 
			this.RevertNotationButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.RevertNotationButton.BackColor = System.Drawing.Color.Transparent;
			this.RevertNotationButton.Enabled = false;
			this.RevertNotationButton.Font = new System.Drawing.Font("Arial", 8F);
			this.RevertNotationButton.ForeColor = System.Drawing.Color.Red;
			this.RevertNotationButton.Location = new System.Drawing.Point(251, 504);
			this.RevertNotationButton.Name = "RevertNotationButton";
			this.RevertNotationButton.Size = new System.Drawing.Size(103, 26);
			this.RevertNotationButton.TabIndex = 2;
			this.RevertNotationButton.Text = "revert notation";
			this.RevertNotationButton.UseVisualStyleBackColor = false;
			this.RevertNotationButton.Click += new System.EventHandler(this.RevertNotationToSavedButton_Click);
			// 
			// SaveSettingsCreateScoreButton
			// 
			this.SaveSettingsCreateScoreButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.SaveSettingsCreateScoreButton.BackColor = System.Drawing.Color.Transparent;
			this.SaveSettingsCreateScoreButton.Enabled = false;
			this.SaveSettingsCreateScoreButton.Font = new System.Drawing.Font("Arial", 8F);
			this.SaveSettingsCreateScoreButton.ForeColor = System.Drawing.Color.Blue;
			this.SaveSettingsCreateScoreButton.Location = new System.Drawing.Point(713, 461);
			this.SaveSettingsCreateScoreButton.Name = "SaveSettingsCreateScoreButton";
			this.SaveSettingsCreateScoreButton.Size = new System.Drawing.Size(214, 31);
			this.SaveSettingsCreateScoreButton.TabIndex = 0;
			this.SaveSettingsCreateScoreButton.Text = "save all settings";
			this.SaveSettingsCreateScoreButton.UseVisualStyleBackColor = false;
			this.SaveSettingsCreateScoreButton.Click += new System.EventHandler(this.SaveSettingsCreateScoreButton_Click);
			// 
			// QuitMoritzButton
			// 
			this.QuitMoritzButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.QuitMoritzButton.Font = new System.Drawing.Font("Arial", 8F);
			this.QuitMoritzButton.Location = new System.Drawing.Point(824, 504);
			this.QuitMoritzButton.Name = "QuitMoritzButton";
			this.QuitMoritzButton.Size = new System.Drawing.Size(103, 26);
			this.QuitMoritzButton.TabIndex = 3;
			this.QuitMoritzButton.Text = "Quit Moritz";
			this.QuitMoritzButton.UseVisualStyleBackColor = true;
			this.QuitMoritzButton.Click += new System.EventHandler(this.QuitMoritzButton_Click);
			// 
			// ShowSelectedKrystalButton
			// 
			this.ShowSelectedKrystalButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.ShowSelectedKrystalButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(215)))), ((int)(((byte)(225)))), ((int)(((byte)(215)))));
			this.ShowSelectedKrystalButton.Enabled = false;
			this.ShowSelectedKrystalButton.Font = new System.Drawing.Font("Arial", 8F);
			this.ShowSelectedKrystalButton.ForeColor = System.Drawing.SystemColors.ControlText;
			this.ShowSelectedKrystalButton.Location = new System.Drawing.Point(16, 272);
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
			this.DimensionsAndMetadataButton.Location = new System.Drawing.Point(15, 504);
			this.DimensionsAndMetadataButton.Name = "DimensionsAndMetadataButton";
			this.DimensionsAndMetadataButton.Size = new System.Drawing.Size(214, 26);
			this.DimensionsAndMetadataButton.TabIndex = 6;
			this.DimensionsAndMetadataButton.Text = "Page Dimensions and Metadata...";
			this.DimensionsAndMetadataButton.UseVisualStyleBackColor = false;
			this.DimensionsAndMetadataButton.Click += new System.EventHandler(this.DimensionsAndMetadataButton_Click);
			// 
			// KrystalsGroupBox
			// 
			this.KrystalsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.KrystalsGroupBox.Controls.Add(this.KrystalsListBox);
			this.KrystalsGroupBox.Controls.Add(this.ShowSelectedKrystalButton);
			this.KrystalsGroupBox.Controls.Add(this.AddKrystalButton);
			this.KrystalsGroupBox.Controls.Add(this.RemoveSelectedKrystalButton);
			this.KrystalsGroupBox.ForeColor = System.Drawing.Color.Brown;
			this.KrystalsGroupBox.Location = new System.Drawing.Point(490, 8);
			this.KrystalsGroupBox.Name = "KrystalsGroupBox";
			this.KrystalsGroupBox.Size = new System.Drawing.Size(214, 372);
			this.KrystalsGroupBox.TabIndex = 10;
			this.KrystalsGroupBox.TabStop = false;
			this.KrystalsGroupBox.Text = "krystals";
			// 
			// ConfirmKrystalsListButton
			// 
			this.ConfirmKrystalsListButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.ConfirmKrystalsListButton.BackColor = System.Drawing.Color.Transparent;
			this.ConfirmKrystalsListButton.Enabled = false;
			this.ConfirmKrystalsListButton.Font = new System.Drawing.Font("Arial", 8F);
			this.ConfirmKrystalsListButton.ForeColor = System.Drawing.Color.Blue;
			this.ConfirmKrystalsListButton.Location = new System.Drawing.Point(601, 384);
			this.ConfirmKrystalsListButton.Name = "ConfirmKrystalsListButton";
			this.ConfirmKrystalsListButton.Size = new System.Drawing.Size(103, 26);
			this.ConfirmKrystalsListButton.TabIndex = 2;
			this.ConfirmKrystalsListButton.Text = "confirm krystals";
			this.ConfirmKrystalsListButton.UseVisualStyleBackColor = false;
			this.ConfirmKrystalsListButton.Click += new System.EventHandler(this.ConfirmKrystalsButton_Click);
			// 
			// RevertKrystalsListButton
			// 
			this.RevertKrystalsListButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.RevertKrystalsListButton.BackColor = System.Drawing.Color.Transparent;
			this.RevertKrystalsListButton.Enabled = false;
			this.RevertKrystalsListButton.Font = new System.Drawing.Font("Arial", 8F);
			this.RevertKrystalsListButton.ForeColor = System.Drawing.Color.Red;
			this.RevertKrystalsListButton.Location = new System.Drawing.Point(490, 384);
			this.RevertKrystalsListButton.Name = "RevertKrystalsListButton";
			this.RevertKrystalsListButton.Size = new System.Drawing.Size(103, 26);
			this.RevertKrystalsListButton.TabIndex = 1;
			this.RevertKrystalsListButton.Text = "revert krystals";
			this.RevertKrystalsListButton.UseVisualStyleBackColor = false;
			this.RevertKrystalsListButton.Click += new System.EventHandler(this.RevertKrystalsToSavedButton_Click);
			// 
			// PalettesGroupBox
			// 
			this.PalettesGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.PalettesGroupBox.Controls.Add(this.DeleteSelectedPaletteButton);
			this.PalettesGroupBox.Controls.Add(this.PalettesListBox);
			this.PalettesGroupBox.Controls.Add(this.ShowSelectedPaletteButton);
			this.PalettesGroupBox.Controls.Add(this.AddPaletteButton);
			this.PalettesGroupBox.ForeColor = System.Drawing.Color.Brown;
			this.PalettesGroupBox.Location = new System.Drawing.Point(713, 8);
			this.PalettesGroupBox.Name = "PalettesGroupBox";
			this.PalettesGroupBox.Size = new System.Drawing.Size(214, 372);
			this.PalettesGroupBox.TabIndex = 11;
			this.PalettesGroupBox.TabStop = false;
			this.PalettesGroupBox.Text = "palettes";
			// 
			// RevertPalettesListButton
			// 
			this.RevertPalettesListButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.RevertPalettesListButton.BackColor = System.Drawing.Color.Transparent;
			this.RevertPalettesListButton.Enabled = false;
			this.RevertPalettesListButton.Font = new System.Drawing.Font("Arial", 8F);
			this.RevertPalettesListButton.ForeColor = System.Drawing.Color.Red;
			this.RevertPalettesListButton.Location = new System.Drawing.Point(729, 384);
			this.RevertPalettesListButton.Name = "RevertPalettesListButton";
			this.RevertPalettesListButton.Size = new System.Drawing.Size(183, 26);
			this.RevertPalettesListButton.TabIndex = 2;
			this.RevertPalettesListButton.Text = "revert palettes";
			this.RevertPalettesListButton.UseVisualStyleBackColor = false;
			this.RevertPalettesListButton.Click += new System.EventHandler(this.RevertPalettesButton_Click);
			// 
			// RevertEverythingButton
			// 
			this.RevertEverythingButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.RevertEverythingButton.BackColor = System.Drawing.Color.Transparent;
			this.RevertEverythingButton.Enabled = false;
			this.RevertEverythingButton.Font = new System.Drawing.Font("Arial", 8F);
			this.RevertEverythingButton.ForeColor = System.Drawing.Color.Red;
			this.RevertEverythingButton.Location = new System.Drawing.Point(490, 461);
			this.RevertEverythingButton.Name = "RevertEverythingButton";
			this.RevertEverythingButton.Size = new System.Drawing.Size(214, 31);
			this.RevertEverythingButton.TabIndex = 1;
			this.RevertEverythingButton.Text = "revert everything";
			this.RevertEverythingButton.UseVisualStyleBackColor = false;
			this.RevertEverythingButton.Click += new System.EventHandler(this.RevertEverythingButton_Click);
			// 
			// ShowUncheckedFormsButton
			// 
			this.ShowUncheckedFormsButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.ShowUncheckedFormsButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(205)))), ((int)(((byte)(240)))), ((int)(((byte)(205)))));
			this.ShowUncheckedFormsButton.Enabled = false;
			this.ShowUncheckedFormsButton.Font = new System.Drawing.Font("Arial", 8F);
			this.ShowUncheckedFormsButton.ForeColor = System.Drawing.SystemColors.ControlText;
			this.ShowUncheckedFormsButton.Location = new System.Drawing.Point(490, 423);
			this.ShowUncheckedFormsButton.Name = "ShowUncheckedFormsButton";
			this.ShowUncheckedFormsButton.Size = new System.Drawing.Size(214, 31);
			this.ShowUncheckedFormsButton.TabIndex = 5;
			this.ShowUncheckedFormsButton.Text = "show unchecked forms";
			this.ShowUncheckedFormsButton.UseVisualStyleBackColor = false;
			this.ShowUncheckedFormsButton.Click += new System.EventHandler(this.ShowUnconfirmedFormsButton_Click);
			// 
			// ShowConfirmedFormsButton
			// 
			this.ShowConfirmedFormsButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.ShowConfirmedFormsButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(215)))), ((int)(((byte)(225)))), ((int)(((byte)(215)))));
			this.ShowConfirmedFormsButton.Enabled = false;
			this.ShowConfirmedFormsButton.Font = new System.Drawing.Font("Arial", 8F);
			this.ShowConfirmedFormsButton.Location = new System.Drawing.Point(713, 423);
			this.ShowConfirmedFormsButton.Name = "ShowConfirmedFormsButton";
			this.ShowConfirmedFormsButton.Size = new System.Drawing.Size(214, 31);
			this.ShowConfirmedFormsButton.TabIndex = 4;
			this.ShowConfirmedFormsButton.Text = "show confirmed forms";
			this.ShowConfirmedFormsButton.UseVisualStyleBackColor = false;
			this.ShowConfirmedFormsButton.Click += new System.EventHandler(this.ShowConfirmedFormsButton_Click);
			// 
			// AssistantComposerForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 14F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(255)))), ((int)(((byte)(245)))));
			this.ClientSize = new System.Drawing.Size(945, 546);
			this.ControlBox = false;
			this.Controls.Add(this.RevertPalettesListButton);
			this.Controls.Add(this.RevertKrystalsListButton);
			this.Controls.Add(this.ConfirmKrystalsListButton);
			this.Controls.Add(this.RevertNotationButton);
			this.Controls.Add(this.ConfirmNotationButton);
			this.Controls.Add(this.ShowConfirmedFormsButton);
			this.Controls.Add(this.ShowUncheckedFormsButton);
			this.Controls.Add(this.RevertEverythingButton);
			this.Controls.Add(this.PalettesGroupBox);
			this.Controls.Add(this.KrystalsGroupBox);
			this.Controls.Add(this.DimensionsAndMetadataButton);
			this.Controls.Add(this.QuitMoritzButton);
			this.Controls.Add(this.SaveSettingsCreateScoreButton);
			this.Controls.Add(this.QuitAssistantComposerButton);
			this.Controls.Add(this.NotationGroupBox);
			this.Font = new System.Drawing.Font("Arial", 8F);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Location = new System.Drawing.Point(250, 100);
			this.Name = "AssistantComposerForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = " ";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.AssistantComposerForm_FormClosing);
			this.NotationGroupBox.ResumeLayout(false);
			this.NotationGroupBox.PerformLayout();
			this.KrystalsGroupBox.ResumeLayout(false);
			this.PalettesGroupBox.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

        private System.Windows.Forms.Button QuitAssistantComposerButton;
        private System.Windows.Forms.ListBox PalettesListBox;
        private System.Windows.Forms.Button ShowSelectedPaletteButton;
        private System.Windows.Forms.Button AddPaletteButton;
        private System.Windows.Forms.Button AddKrystalButton;
        private System.Windows.Forms.Button DeleteSelectedPaletteButton;
        private System.Windows.Forms.ListBox KrystalsListBox;
        private System.Windows.Forms.Button RemoveSelectedKrystalButton;
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
        private System.Windows.Forms.Label UnitsHelpLabel;
        private System.Windows.Forms.Button SaveSettingsCreateScoreButton;
		private System.Windows.Forms.Button QuitMoritzButton;
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
        private System.Windows.Forms.Label VoiceIndicesHelpLabel;
        private System.Windows.Forms.Label VoiceIndicesPerStaffLabel;
        private System.Windows.Forms.TextBox VoiceIndicesPerStaffTextBox;
		private System.Windows.Forms.Label VoiceIndicesPerStaffHelpLabel;
        private System.Windows.Forms.Button RevertEverythingButton;
        private System.Windows.Forms.Button ShowUncheckedFormsButton;
        private System.Windows.Forms.Button ShowConfirmedFormsButton;
        private System.Windows.Forms.Button ConfirmKrystalsListButton;
        private System.Windows.Forms.Button RevertKrystalsListButton;
        private System.Windows.Forms.Button ConfirmNotationButton;
        private System.Windows.Forms.Button RevertNotationButton;
        private System.Windows.Forms.Button RevertPalettesListButton;
		private System.Windows.Forms.Label VoiceIndicesHelpLabelLabel;
        private System.Windows.Forms.ComboBox OutputChordSymbolTypeComboBox;
        private System.Windows.Forms.Label OutputChordSymbolTypeLabel;
    }
}

