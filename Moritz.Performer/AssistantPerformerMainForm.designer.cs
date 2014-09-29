using System.Windows.Forms;

namespace Moritz.Performer
{
	partial class AssistantPerformerMainForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AssistantPerformerMainForm));
            this.PerformersPitchOptionsGroupBox = new System.Windows.Forms.GroupBox();
            this.PlayPerformedPitchesRadioButton = new System.Windows.Forms.RadioButton();
            this.PlayNotatedPitchesRadioButton = new System.Windows.Forms.RadioButton();
            this.PerformerSilentRadioButton = new System.Windows.Forms.RadioButton();
            this.PerformersVelocityOptionsGroupBox = new System.Windows.Forms.GroupBox();
            this.PlayNotatedDynamicsRadioButton = new System.Windows.Forms.RadioButton();
            this.PlayPerformedDynamicsRadioButton = new System.Windows.Forms.RadioButton();
            this.DeleteMpoxFileButton = new System.Windows.Forms.Button();
            this.AssistantsDurationsOptionsGroupBox = new System.Windows.Forms.GroupBox();
            this.SpeedFactorLabel = new System.Windows.Forms.Label();
            this.SpeedFactorTextBox = new System.Windows.Forms.TextBox();
            this.AssistantsDurationsSymbolsRelativeRadioButton = new System.Windows.Forms.RadioButton();
            this.AssistantsDurationsSymbolsAbsoluteRadioButton = new System.Windows.Forms.RadioButton();
            this.AssistantPitchesComment = new System.Windows.Forms.Label();
            this.MoritzPlayersPanel = new System.Windows.Forms.Panel();
            this.KeyboardOptionsButton = new System.Windows.Forms.Button();
            this.NewMpoxFileButton = new System.Windows.Forms.Button();
            this.BottomControlsPanel = new System.Windows.Forms.Panel();
            this.QuitMoritzButton = new System.Windows.Forms.Button();
            this.ShowMoritzButton = new System.Windows.Forms.Button();
            this.MomentPositionLabel = new System.Windows.Forms.Label();
            this.MomentNumberLabel = new System.Windows.Forms.Label();
            this.MomentPositionCommentLabel = new System.Windows.Forms.Label();
            this.MomentNumberCommentLabel = new System.Windows.Forms.Label();
            this.QuitAssistantPerformerButton = new System.Windows.Forms.Button();
            this.GlobalOptionsGroupBox = new System.Windows.Forms.GroupBox();
            this.MomentsRangeLabel = new System.Windows.Forms.Label();
            this.SaveMidiCheckBox = new System.Windows.Forms.CheckBox();
            this.MinimumOrnamentChordDurationLabel2 = new System.Windows.Forms.Label();
            this.MinimumOrnamentChordDurationLabel1 = new System.Windows.Forms.Label();
            this.RepeatCheckBox = new System.Windows.Forms.CheckBox();
            this.MinimumOrnamentChordDurationComment = new System.Windows.Forms.Label();
            this.MinimumOrnamentChordDurationTextBox = new System.Windows.Forms.TextBox();
            this.StartMomentTextBox = new System.Windows.Forms.TextBox();
            this.StartMomentLabel = new System.Windows.Forms.Label();
            this.PlaySymbolsButton = new System.Windows.Forms.Button();
            this.PerformLiveButton = new System.Windows.Forms.Button();
            this.StopButton = new System.Windows.Forms.Button();
            this.CommentOnSilentStaves = new System.Windows.Forms.Label();
            this.SelectAllButton = new System.Windows.Forms.Button();
            this.AssistantButton = new System.Windows.Forms.Button();
            this.SilentButton = new System.Windows.Forms.Button();
            this.PerformerButton = new System.Windows.Forms.Button();
            this.ShowScoreButton = new System.Windows.Forms.Button();
            this.MpoxFilenamesComboBox = new System.Windows.Forms.ComboBox();
            this.PerformersPitchOptionsGroupBox.SuspendLayout();
            this.PerformersVelocityOptionsGroupBox.SuspendLayout();
            this.AssistantsDurationsOptionsGroupBox.SuspendLayout();
            this.BottomControlsPanel.SuspendLayout();
            this.GlobalOptionsGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // PerformersPitchOptionsGroupBox
            // 
            this.PerformersPitchOptionsGroupBox.Controls.Add(this.PlayPerformedPitchesRadioButton);
            this.PerformersPitchOptionsGroupBox.Controls.Add(this.PlayNotatedPitchesRadioButton);
            this.PerformersPitchOptionsGroupBox.ForeColor = System.Drawing.SystemColors.ControlText;
            this.PerformersPitchOptionsGroupBox.Location = new System.Drawing.Point(2, 262);
            this.PerformersPitchOptionsGroupBox.Margin = new System.Windows.Forms.Padding(0);
            this.PerformersPitchOptionsGroupBox.Name = "PerformersPitchOptionsGroupBox";
            this.PerformersPitchOptionsGroupBox.Padding = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.PerformersPitchOptionsGroupBox.Size = new System.Drawing.Size(504, 56);
            this.PerformersPitchOptionsGroupBox.TabIndex = 0;
            this.PerformersPitchOptionsGroupBox.TabStop = false;
            this.PerformersPitchOptionsGroupBox.Text = "Performer\'s Pitch Options";
            // 
            // PlayPerformedPitchesRadioButton
            // 
            this.PlayPerformedPitchesRadioButton.AutoSize = true;
            this.PlayPerformedPitchesRadioButton.ForeColor = System.Drawing.Color.Black;
            this.PlayPerformedPitchesRadioButton.Location = new System.Drawing.Point(109, 32);
            this.PlayPerformedPitchesRadioButton.Margin = new System.Windows.Forms.Padding(0);
            this.PlayPerformedPitchesRadioButton.Name = "PlayPerformedPitchesRadioButton";
            this.PlayPerformedPitchesRadioButton.Size = new System.Drawing.Size(302, 17);
            this.PlayPerformedPitchesRadioButton.TabIndex = 1;
            this.PlayPerformedPitchesRadioButton.Text = "Performed pitches are played. Notated pitches are ignored.";
            this.PlayPerformedPitchesRadioButton.UseVisualStyleBackColor = true;
            this.PlayPerformedPitchesRadioButton.MouseDown += new System.Windows.Forms.MouseEventHandler(this.PlayPerformedPitchesRadioButton_MouseDown);
            // 
            // PlayNotatedPitchesRadioButton
            // 
            this.PlayNotatedPitchesRadioButton.AutoSize = true;
            this.PlayNotatedPitchesRadioButton.ForeColor = System.Drawing.Color.Black;
            this.PlayNotatedPitchesRadioButton.Location = new System.Drawing.Point(109, 15);
            this.PlayNotatedPitchesRadioButton.Margin = new System.Windows.Forms.Padding(0);
            this.PlayNotatedPitchesRadioButton.Name = "PlayNotatedPitchesRadioButton";
            this.PlayNotatedPitchesRadioButton.Size = new System.Drawing.Size(302, 17);
            this.PlayNotatedPitchesRadioButton.TabIndex = 0;
            this.PlayNotatedPitchesRadioButton.Text = "Notated pitches are played. Performed pitches are ignored.";
            this.PlayNotatedPitchesRadioButton.UseVisualStyleBackColor = true;
            this.PlayNotatedPitchesRadioButton.MouseDown += new System.Windows.Forms.MouseEventHandler(this.PlayNotatedPitchesRadioButton_MouseDown);
            // 
            // PerformerSilentRadioButton
            // 
            this.PerformerSilentRadioButton.AutoSize = true;
            this.PerformerSilentRadioButton.ForeColor = System.Drawing.Color.Black;
            this.PerformerSilentRadioButton.Location = new System.Drawing.Point(65, 50);
            this.PerformerSilentRadioButton.Margin = new System.Windows.Forms.Padding(0);
            this.PerformerSilentRadioButton.Name = "PerformerSilentRadioButton";
            this.PerformerSilentRadioButton.Size = new System.Drawing.Size(275, 17);
            this.PerformerSilentRadioButton.TabIndex = 2;
            this.PerformerSilentRadioButton.Text = "The performed staves are silent (\"conductor\" option).";
            this.PerformerSilentRadioButton.UseVisualStyleBackColor = true;
            this.PerformerSilentRadioButton.MouseDown += new System.Windows.Forms.MouseEventHandler(this.PerformerSilentRadioButton_MouseDown);
            // 
            // PerformersVelocityOptionsGroupBox
            // 
            this.PerformersVelocityOptionsGroupBox.Controls.Add(this.PlayNotatedDynamicsRadioButton);
            this.PerformersVelocityOptionsGroupBox.Controls.Add(this.PerformerSilentRadioButton);
            this.PerformersVelocityOptionsGroupBox.Controls.Add(this.PlayPerformedDynamicsRadioButton);
            this.PerformersVelocityOptionsGroupBox.ForeColor = System.Drawing.SystemColors.ControlText;
            this.PerformersVelocityOptionsGroupBox.Location = new System.Drawing.Point(2, 182);
            this.PerformersVelocityOptionsGroupBox.Margin = new System.Windows.Forms.Padding(0);
            this.PerformersVelocityOptionsGroupBox.Name = "PerformersVelocityOptionsGroupBox";
            this.PerformersVelocityOptionsGroupBox.Padding = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.PerformersVelocityOptionsGroupBox.Size = new System.Drawing.Size(504, 74);
            this.PerformersVelocityOptionsGroupBox.TabIndex = 1;
            this.PerformersVelocityOptionsGroupBox.TabStop = false;
            this.PerformersVelocityOptionsGroupBox.Text = "Performer\'s Velocity Options";
            // 
            // PlayNotatedDynamicsRadioButton
            // 
            this.PlayNotatedDynamicsRadioButton.AutoSize = true;
            this.PlayNotatedDynamicsRadioButton.ForeColor = System.Drawing.Color.Black;
            this.PlayNotatedDynamicsRadioButton.Location = new System.Drawing.Point(65, 16);
            this.PlayNotatedDynamicsRadioButton.Margin = new System.Windows.Forms.Padding(0);
            this.PlayNotatedDynamicsRadioButton.Name = "PlayNotatedDynamicsRadioButton";
            this.PlayNotatedDynamicsRadioButton.Size = new System.Drawing.Size(391, 17);
            this.PlayNotatedDynamicsRadioButton.TabIndex = 0;
            this.PlayNotatedDynamicsRadioButton.TabStop = true;
            this.PlayNotatedDynamicsRadioButton.Text = "MIDI velocities written in the score are used. Performed velocities are ignored.";
            this.PlayNotatedDynamicsRadioButton.UseVisualStyleBackColor = true;
            this.PlayNotatedDynamicsRadioButton.MouseDown += new System.Windows.Forms.MouseEventHandler(this.PlayNotatedDynamicsRadioButton_MouseDown);
            // 
            // PlayPerformedDynamicsRadioButton
            // 
            this.PlayPerformedDynamicsRadioButton.AutoSize = true;
            this.PlayPerformedDynamicsRadioButton.ForeColor = System.Drawing.Color.Black;
            this.PlayPerformedDynamicsRadioButton.Location = new System.Drawing.Point(65, 33);
            this.PlayPerformedDynamicsRadioButton.Margin = new System.Windows.Forms.Padding(0);
            this.PlayPerformedDynamicsRadioButton.Name = "PlayPerformedDynamicsRadioButton";
            this.PlayPerformedDynamicsRadioButton.Size = new System.Drawing.Size(392, 17);
            this.PlayPerformedDynamicsRadioButton.TabIndex = 1;
            this.PlayPerformedDynamicsRadioButton.TabStop = true;
            this.PlayPerformedDynamicsRadioButton.Text = "Performed MIDI velocities are used. Velocities written in the score are ignored.";
            this.PlayPerformedDynamicsRadioButton.UseVisualStyleBackColor = true;
            this.PlayPerformedDynamicsRadioButton.MouseDown += new System.Windows.Forms.MouseEventHandler(this.PlayPerformedDynamicsRadioButton_MouseDown);
            // 
            // DeleteMpoxFileButton
            // 
            this.DeleteMpoxFileButton.ForeColor = System.Drawing.SystemColors.ControlText;
            this.DeleteMpoxFileButton.Location = new System.Drawing.Point(258, 41);
            this.DeleteMpoxFileButton.Margin = new System.Windows.Forms.Padding(0);
            this.DeleteMpoxFileButton.Name = "DeleteMpoxFileButton";
            this.DeleteMpoxFileButton.Size = new System.Drawing.Size(129, 22);
            this.DeleteMpoxFileButton.TabIndex = 9;
            this.DeleteMpoxFileButton.Text = "Delete .mpox file";
            this.DeleteMpoxFileButton.UseVisualStyleBackColor = true;
            this.DeleteMpoxFileButton.Click += new System.EventHandler(this.DeleteMpoxFileButton_Click);
            // 
            // AssistantsDurationsOptionsGroupBox
            // 
            this.AssistantsDurationsOptionsGroupBox.Controls.Add(this.SpeedFactorLabel);
            this.AssistantsDurationsOptionsGroupBox.Controls.Add(this.SpeedFactorTextBox);
            this.AssistantsDurationsOptionsGroupBox.Controls.Add(this.AssistantsDurationsSymbolsRelativeRadioButton);
            this.AssistantsDurationsOptionsGroupBox.Controls.Add(this.AssistantsDurationsSymbolsAbsoluteRadioButton);
            this.AssistantsDurationsOptionsGroupBox.Controls.Add(this.AssistantPitchesComment);
            this.AssistantsDurationsOptionsGroupBox.ForeColor = System.Drawing.SystemColors.ControlText;
            this.AssistantsDurationsOptionsGroupBox.Location = new System.Drawing.Point(2, 354);
            this.AssistantsDurationsOptionsGroupBox.Margin = new System.Windows.Forms.Padding(0);
            this.AssistantsDurationsOptionsGroupBox.Name = "AssistantsDurationsOptionsGroupBox";
            this.AssistantsDurationsOptionsGroupBox.Padding = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.AssistantsDurationsOptionsGroupBox.Size = new System.Drawing.Size(504, 138);
            this.AssistantsDurationsOptionsGroupBox.TabIndex = 2;
            this.AssistantsDurationsOptionsGroupBox.TabStop = false;
            this.AssistantsDurationsOptionsGroupBox.Text = "Assistant\'s Duration Options";
            // 
            // SpeedFactorLabel
            // 
            this.SpeedFactorLabel.AutoSize = true;
            this.SpeedFactorLabel.ForeColor = System.Drawing.SystemColors.ControlText;
            this.SpeedFactorLabel.Location = new System.Drawing.Point(136, 81);
            this.SpeedFactorLabel.Margin = new System.Windows.Forms.Padding(0);
            this.SpeedFactorLabel.Name = "SpeedFactorLabel";
            this.SpeedFactorLabel.Size = new System.Drawing.Size(247, 13);
            this.SpeedFactorLabel.TabIndex = 10;
            this.SpeedFactorLabel.Text = "greater than 0 ( higher values lead to higher speed)";
            // 
            // SpeedFactorTextBox
            // 
            this.SpeedFactorTextBox.ForeColor = System.Drawing.Color.Blue;
            this.SpeedFactorTextBox.Location = new System.Drawing.Point(93, 78);
            this.SpeedFactorTextBox.Margin = new System.Windows.Forms.Padding(0);
            this.SpeedFactorTextBox.Name = "SpeedFactorTextBox";
            this.SpeedFactorTextBox.Size = new System.Drawing.Size(39, 20);
            this.SpeedFactorTextBox.TabIndex = 9;
            this.SpeedFactorTextBox.Enter += new System.EventHandler(this.SpeedFactorTextBox_Enter);
            this.SpeedFactorTextBox.Leave += new System.EventHandler(this.SpeedFactorTextBox_Leave);
            // 
            // AssistantsDurationsSymbolsRelativeRadioButton
            // 
            this.AssistantsDurationsSymbolsRelativeRadioButton.AutoSize = true;
            this.AssistantsDurationsSymbolsRelativeRadioButton.CheckAlign = System.Drawing.ContentAlignment.TopLeft;
            this.AssistantsDurationsSymbolsRelativeRadioButton.ForeColor = System.Drawing.Color.Black;
            this.AssistantsDurationsSymbolsRelativeRadioButton.Location = new System.Drawing.Point(24, 102);
            this.AssistantsDurationsSymbolsRelativeRadioButton.Margin = new System.Windows.Forms.Padding(0);
            this.AssistantsDurationsSymbolsRelativeRadioButton.Name = "AssistantsDurationsSymbolsRelativeRadioButton";
            this.AssistantsDurationsSymbolsRelativeRadioButton.Size = new System.Drawing.Size(474, 30);
            this.AssistantsDurationsSymbolsRelativeRadioButton.TabIndex = 6;
            this.AssistantsDurationsSymbolsRelativeRadioButton.Text = "Relative Symbols: The assistant\'s moment durations will be those written in the s" +
    "core multiplied\r\nby the relation between the written and performed durations of " +
    "the performer\'s previous moment.";
            this.AssistantsDurationsSymbolsRelativeRadioButton.UseVisualStyleBackColor = true;
            this.AssistantsDurationsSymbolsRelativeRadioButton.MouseDown += new System.Windows.Forms.MouseEventHandler(this.AssistantsDurationsSymbolsRelativeRadioButton_MouseDown);
            // 
            // AssistantsDurationsSymbolsAbsoluteRadioButton
            // 
            this.AssistantsDurationsSymbolsAbsoluteRadioButton.AutoSize = true;
            this.AssistantsDurationsSymbolsAbsoluteRadioButton.CheckAlign = System.Drawing.ContentAlignment.TopLeft;
            this.AssistantsDurationsSymbolsAbsoluteRadioButton.ForeColor = System.Drawing.Color.Black;
            this.AssistantsDurationsSymbolsAbsoluteRadioButton.Location = new System.Drawing.Point(24, 47);
            this.AssistantsDurationsSymbolsAbsoluteRadioButton.Margin = new System.Windows.Forms.Padding(0);
            this.AssistantsDurationsSymbolsAbsoluteRadioButton.Name = "AssistantsDurationsSymbolsAbsoluteRadioButton";
            this.AssistantsDurationsSymbolsAbsoluteRadioButton.Size = new System.Drawing.Size(460, 30);
            this.AssistantsDurationsSymbolsAbsoluteRadioButton.TabIndex = 7;
            this.AssistantsDurationsSymbolsAbsoluteRadioButton.Text = "Absolute Symbols: The assistant\'s moment durations will be those written in the s" +
    "core divided\r\nby the following ( floating point ) factor:";
            this.AssistantsDurationsSymbolsAbsoluteRadioButton.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            this.AssistantsDurationsSymbolsAbsoluteRadioButton.UseVisualStyleBackColor = true;
            this.AssistantsDurationsSymbolsAbsoluteRadioButton.MouseDown += new System.Windows.Forms.MouseEventHandler(this.AssistantsDurationsSymbolsAbsoluteRadioButton_MouseDown);
            // 
            // AssistantPitchesComment
            // 
            this.AssistantPitchesComment.AutoSize = true;
            this.AssistantPitchesComment.ForeColor = System.Drawing.Color.CornflowerBlue;
            this.AssistantPitchesComment.Location = new System.Drawing.Point(21, 18);
            this.AssistantPitchesComment.Margin = new System.Windows.Forms.Padding(0);
            this.AssistantPitchesComment.Name = "AssistantPitchesComment";
            this.AssistantPitchesComment.Size = new System.Drawing.Size(473, 26);
            this.AssistantPitchesComment.TabIndex = 2;
            this.AssistantPitchesComment.Text = "The assistant\'s MIDI velocities and pitches are always those written in the score" +
    ".\r\nSubject to the global option above, durations inside ornaments are always as " +
    "specified in the score.";
            // 
            // MoritzPlayersPanel
            // 
            this.MoritzPlayersPanel.AutoScroll = true;
            this.MoritzPlayersPanel.BackColor = System.Drawing.Color.White;
            this.MoritzPlayersPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.MoritzPlayersPanel.Location = new System.Drawing.Point(169, 38);
            this.MoritzPlayersPanel.Margin = new System.Windows.Forms.Padding(0);
            this.MoritzPlayersPanel.Name = "MoritzPlayersPanel";
            this.MoritzPlayersPanel.Size = new System.Drawing.Size(192, 138);
            this.MoritzPlayersPanel.TabIndex = 3;
            // 
            // KeyboardOptionsButton
            // 
            this.KeyboardOptionsButton.ForeColor = System.Drawing.SystemColors.ControlText;
            this.KeyboardOptionsButton.Location = new System.Drawing.Point(105, 325);
            this.KeyboardOptionsButton.Margin = new System.Windows.Forms.Padding(0);
            this.KeyboardOptionsButton.Name = "KeyboardOptionsButton";
            this.KeyboardOptionsButton.Size = new System.Drawing.Size(320, 22);
            this.KeyboardOptionsButton.TabIndex = 12;
            this.KeyboardOptionsButton.Text = "Performer\'s Keyboard Options";
            this.KeyboardOptionsButton.UseVisualStyleBackColor = true;
            this.KeyboardOptionsButton.Click += new System.EventHandler(this.KeyboardOptionsButton_Click);
            // 
            // NewMpoxFileButton
            // 
            this.NewMpoxFileButton.ForeColor = System.Drawing.SystemColors.ControlText;
            this.NewMpoxFileButton.Location = new System.Drawing.Point(123, 41);
            this.NewMpoxFileButton.Margin = new System.Windows.Forms.Padding(0);
            this.NewMpoxFileButton.Name = "NewMpoxFileButton";
            this.NewMpoxFileButton.Size = new System.Drawing.Size(129, 22);
            this.NewMpoxFileButton.TabIndex = 14;
            this.NewMpoxFileButton.Text = "New .mpox file";
            this.NewMpoxFileButton.UseVisualStyleBackColor = true;
            this.NewMpoxFileButton.Click += new System.EventHandler(this.NewMpoxFileButton_Click);
            // 
            // BottomControlsPanel
            // 
            this.BottomControlsPanel.AutoSize = true;
            this.BottomControlsPanel.Controls.Add(this.QuitMoritzButton);
            this.BottomControlsPanel.Controls.Add(this.ShowMoritzButton);
            this.BottomControlsPanel.Controls.Add(this.MomentPositionLabel);
            this.BottomControlsPanel.Controls.Add(this.MomentNumberLabel);
            this.BottomControlsPanel.Controls.Add(this.MomentPositionCommentLabel);
            this.BottomControlsPanel.Controls.Add(this.MomentNumberCommentLabel);
            this.BottomControlsPanel.Controls.Add(this.QuitAssistantPerformerButton);
            this.BottomControlsPanel.Controls.Add(this.GlobalOptionsGroupBox);
            this.BottomControlsPanel.Controls.Add(this.PlaySymbolsButton);
            this.BottomControlsPanel.Controls.Add(this.PerformLiveButton);
            this.BottomControlsPanel.Controls.Add(this.CommentOnSilentStaves);
            this.BottomControlsPanel.Controls.Add(this.NewMpoxFileButton);
            this.BottomControlsPanel.Controls.Add(this.SelectAllButton);
            this.BottomControlsPanel.Controls.Add(this.AssistantButton);
            this.BottomControlsPanel.Controls.Add(this.SilentButton);
            this.BottomControlsPanel.Controls.Add(this.DeleteMpoxFileButton);
            this.BottomControlsPanel.Controls.Add(this.PerformerButton);
            this.BottomControlsPanel.Controls.Add(this.KeyboardOptionsButton);
            this.BottomControlsPanel.Controls.Add(this.AssistantsDurationsOptionsGroupBox);
            this.BottomControlsPanel.Controls.Add(this.PerformersVelocityOptionsGroupBox);
            this.BottomControlsPanel.Controls.Add(this.PerformersPitchOptionsGroupBox);
            this.BottomControlsPanel.Controls.Add(this.ShowScoreButton);
            this.BottomControlsPanel.Controls.Add(this.StopButton);
            this.BottomControlsPanel.Location = new System.Drawing.Point(10, 180);
            this.BottomControlsPanel.Margin = new System.Windows.Forms.Padding(0);
            this.BottomControlsPanel.Name = "BottomControlsPanel";
            this.BottomControlsPanel.Size = new System.Drawing.Size(511, 573);
            this.BottomControlsPanel.TabIndex = 18;
            // 
            // QuitMoritzButton
            // 
            this.QuitMoritzButton.Location = new System.Drawing.Point(4, 542);
            this.QuitMoritzButton.Margin = new System.Windows.Forms.Padding(0);
            this.QuitMoritzButton.Name = "QuitMoritzButton";
            this.QuitMoritzButton.Size = new System.Drawing.Size(144, 22);
            this.QuitMoritzButton.TabIndex = 41;
            this.QuitMoritzButton.Text = "Quit Moritz";
            this.QuitMoritzButton.UseVisualStyleBackColor = true;
            this.QuitMoritzButton.Click += new System.EventHandler(this.QuitMoritzButton_Click);
            // 
            // ShowMoritzButton
            // 
            this.ShowMoritzButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(215)))), ((int)(((byte)(225)))), ((int)(((byte)(215)))));
            this.ShowMoritzButton.Location = new System.Drawing.Point(154, 542);
            this.ShowMoritzButton.Margin = new System.Windows.Forms.Padding(0);
            this.ShowMoritzButton.Name = "ShowMoritzButton";
            this.ShowMoritzButton.Size = new System.Drawing.Size(99, 22);
            this.ShowMoritzButton.TabIndex = 40;
            this.ShowMoritzButton.Text = "Show Moritz";
            this.ShowMoritzButton.UseVisualStyleBackColor = false;
            this.ShowMoritzButton.Click += new System.EventHandler(this.ShowMoritzButton_Click);
            // 
            // MomentPositionLabel
            // 
            this.MomentPositionLabel.AutoSize = true;
            this.MomentPositionLabel.ForeColor = System.Drawing.Color.Blue;
            this.MomentPositionLabel.Location = new System.Drawing.Point(357, 496);
            this.MomentPositionLabel.Margin = new System.Windows.Forms.Padding(0);
            this.MomentPositionLabel.Name = "MomentPositionLabel";
            this.MomentPositionLabel.Size = new System.Drawing.Size(13, 13);
            this.MomentPositionLabel.TabIndex = 38;
            this.MomentPositionLabel.Text = "0";
            // 
            // MomentNumberLabel
            // 
            this.MomentNumberLabel.AutoSize = true;
            this.MomentNumberLabel.ForeColor = System.Drawing.Color.Blue;
            this.MomentNumberLabel.Location = new System.Drawing.Point(216, 496);
            this.MomentNumberLabel.Margin = new System.Windows.Forms.Padding(0);
            this.MomentNumberLabel.Name = "MomentNumberLabel";
            this.MomentNumberLabel.Size = new System.Drawing.Size(13, 13);
            this.MomentNumberLabel.TabIndex = 37;
            this.MomentNumberLabel.Text = "0";
            // 
            // MomentPositionCommentLabel
            // 
            this.MomentPositionCommentLabel.AutoSize = true;
            this.MomentPositionCommentLabel.ForeColor = System.Drawing.SystemColors.ControlText;
            this.MomentPositionCommentLabel.Location = new System.Drawing.Point(270, 496);
            this.MomentPositionCommentLabel.Margin = new System.Windows.Forms.Padding(0);
            this.MomentPositionCommentLabel.Name = "MomentPositionCommentLabel";
            this.MomentPositionCommentLabel.Size = new System.Drawing.Size(86, 13);
            this.MomentPositionCommentLabel.TabIndex = 36;
            this.MomentPositionCommentLabel.Text = "moment position:";
            // 
            // MomentNumberCommentLabel
            // 
            this.MomentNumberCommentLabel.AutoSize = true;
            this.MomentNumberCommentLabel.ForeColor = System.Drawing.SystemColors.ControlText;
            this.MomentNumberCommentLabel.Location = new System.Drawing.Point(76, 496);
            this.MomentNumberCommentLabel.Margin = new System.Windows.Forms.Padding(0);
            this.MomentNumberCommentLabel.Name = "MomentNumberCommentLabel";
            this.MomentNumberCommentLabel.Size = new System.Drawing.Size(139, 13);
            this.MomentNumberCommentLabel.TabIndex = 35;
            this.MomentNumberCommentLabel.Text = "performer\'s moment number:";
            this.MomentNumberCommentLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // QuitAssistantPerformerButton
            // 
            this.QuitAssistantPerformerButton.Location = new System.Drawing.Point(4, 518);
            this.QuitAssistantPerformerButton.Margin = new System.Windows.Forms.Padding(0);
            this.QuitAssistantPerformerButton.Name = "QuitAssistantPerformerButton";
            this.QuitAssistantPerformerButton.Size = new System.Drawing.Size(144, 22);
            this.QuitAssistantPerformerButton.TabIndex = 25;
            this.QuitAssistantPerformerButton.Text = "Quit Assistant Performer";
            this.QuitAssistantPerformerButton.UseVisualStyleBackColor = true;
            this.QuitAssistantPerformerButton.Click += new System.EventHandler(this.QuitAssistantPerformerButton_Click);
            // 
            // GlobalOptionsGroupBox
            // 
            this.GlobalOptionsGroupBox.Controls.Add(this.MomentsRangeLabel);
            this.GlobalOptionsGroupBox.Controls.Add(this.SaveMidiCheckBox);
            this.GlobalOptionsGroupBox.Controls.Add(this.MinimumOrnamentChordDurationLabel2);
            this.GlobalOptionsGroupBox.Controls.Add(this.MinimumOrnamentChordDurationLabel1);
            this.GlobalOptionsGroupBox.Controls.Add(this.RepeatCheckBox);
            this.GlobalOptionsGroupBox.Controls.Add(this.MinimumOrnamentChordDurationComment);
            this.GlobalOptionsGroupBox.Controls.Add(this.MinimumOrnamentChordDurationTextBox);
            this.GlobalOptionsGroupBox.Controls.Add(this.StartMomentTextBox);
            this.GlobalOptionsGroupBox.Controls.Add(this.StartMomentLabel);
            this.GlobalOptionsGroupBox.Location = new System.Drawing.Point(4, 67);
            this.GlobalOptionsGroupBox.Margin = new System.Windows.Forms.Padding(0);
            this.GlobalOptionsGroupBox.Name = "GlobalOptionsGroupBox";
            this.GlobalOptionsGroupBox.Size = new System.Drawing.Size(504, 110);
            this.GlobalOptionsGroupBox.TabIndex = 19;
            this.GlobalOptionsGroupBox.TabStop = false;
            this.GlobalOptionsGroupBox.Text = "Global Options";
            // 
            // MomentsRangeLabel
            // 
            this.MomentsRangeLabel.AutoSize = true;
            this.MomentsRangeLabel.ForeColor = System.Drawing.Color.CornflowerBlue;
            this.MomentsRangeLabel.Location = new System.Drawing.Point(207, 21);
            this.MomentsRangeLabel.Margin = new System.Windows.Forms.Padding(0);
            this.MomentsRangeLabel.Name = "MomentsRangeLabel";
            this.MomentsRangeLabel.Size = new System.Drawing.Size(34, 13);
            this.MomentsRangeLabel.TabIndex = 34;
            this.MomentsRangeLabel.Text = "[  ...  ]";
            // 
            // SaveMidiCheckBox
            // 
            this.SaveMidiCheckBox.AutoSize = true;
            this.SaveMidiCheckBox.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.SaveMidiCheckBox.Location = new System.Drawing.Point(387, 19);
            this.SaveMidiCheckBox.Margin = new System.Windows.Forms.Padding(0);
            this.SaveMidiCheckBox.Name = "SaveMidiCheckBox";
            this.SaveMidiCheckBox.Size = new System.Drawing.Size(99, 17);
            this.SaveMidiCheckBox.TabIndex = 33;
            this.SaveMidiCheckBox.Text = "Save MIDI file?";
            this.SaveMidiCheckBox.UseVisualStyleBackColor = true;
            this.SaveMidiCheckBox.CheckedChanged += new System.EventHandler(this.SaveMidiCheckBox_CheckedChanged);
            // 
            // MinimumOrnamentChordDurationLabel2
            // 
            this.MinimumOrnamentChordDurationLabel2.AutoSize = true;
            this.MinimumOrnamentChordDurationLabel2.ForeColor = System.Drawing.SystemColors.ControlText;
            this.MinimumOrnamentChordDurationLabel2.Location = new System.Drawing.Point(319, 86);
            this.MinimumOrnamentChordDurationLabel2.Margin = new System.Windows.Forms.Padding(0);
            this.MinimumOrnamentChordDurationLabel2.Name = "MinimumOrnamentChordDurationLabel2";
            this.MinimumOrnamentChordDurationLabel2.Size = new System.Drawing.Size(63, 13);
            this.MinimumOrnamentChordDurationLabel2.TabIndex = 12;
            this.MinimumOrnamentChordDurationLabel2.Text = "milliseconds";
            // 
            // MinimumOrnamentChordDurationLabel1
            // 
            this.MinimumOrnamentChordDurationLabel1.AutoSize = true;
            this.MinimumOrnamentChordDurationLabel1.ForeColor = System.Drawing.SystemColors.ControlText;
            this.MinimumOrnamentChordDurationLabel1.Location = new System.Drawing.Point(110, 86);
            this.MinimumOrnamentChordDurationLabel1.Margin = new System.Windows.Forms.Padding(0);
            this.MinimumOrnamentChordDurationLabel1.Name = "MinimumOrnamentChordDurationLabel1";
            this.MinimumOrnamentChordDurationLabel1.Size = new System.Drawing.Size(169, 13);
            this.MinimumOrnamentChordDurationLabel1.TabIndex = 11;
            this.MinimumOrnamentChordDurationLabel1.Text = "Minimum ornament chord duration:";
            // 
            // RepeatCheckBox
            // 
            this.RepeatCheckBox.AutoSize = true;
            this.RepeatCheckBox.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.RepeatCheckBox.Location = new System.Drawing.Point(305, 19);
            this.RepeatCheckBox.Margin = new System.Windows.Forms.Padding(0);
            this.RepeatCheckBox.Name = "RepeatCheckBox";
            this.RepeatCheckBox.Size = new System.Drawing.Size(67, 17);
            this.RepeatCheckBox.TabIndex = 30;
            this.RepeatCheckBox.Text = "Repeat?";
            this.RepeatCheckBox.UseVisualStyleBackColor = true;
            this.RepeatCheckBox.CheckedChanged += new System.EventHandler(this.RepeatCheckBox_CheckedChanged);
            // 
            // MinimumOrnamentChordDurationComment
            // 
            this.MinimumOrnamentChordDurationComment.AutoSize = true;
            this.MinimumOrnamentChordDurationComment.ForeColor = System.Drawing.Color.CornflowerBlue;
            this.MinimumOrnamentChordDurationComment.Location = new System.Drawing.Point(19, 40);
            this.MinimumOrnamentChordDurationComment.Margin = new System.Windows.Forms.Padding(0);
            this.MinimumOrnamentChordDurationComment.Name = "MinimumOrnamentChordDurationComment";
            this.MinimumOrnamentChordDurationComment.Size = new System.Drawing.Size(463, 39);
            this.MinimumOrnamentChordDurationComment.TabIndex = 3;
            this.MinimumOrnamentChordDurationComment.Text = resources.GetString("MinimumOrnamentChordDurationComment.Text");
            // 
            // MinimumOrnamentChordDurationTextBox
            // 
            this.MinimumOrnamentChordDurationTextBox.ForeColor = System.Drawing.Color.Blue;
            this.MinimumOrnamentChordDurationTextBox.Location = new System.Drawing.Point(279, 83);
            this.MinimumOrnamentChordDurationTextBox.Margin = new System.Windows.Forms.Padding(0);
            this.MinimumOrnamentChordDurationTextBox.Name = "MinimumOrnamentChordDurationTextBox";
            this.MinimumOrnamentChordDurationTextBox.Size = new System.Drawing.Size(39, 20);
            this.MinimumOrnamentChordDurationTextBox.TabIndex = 10;
            this.MinimumOrnamentChordDurationTextBox.Enter += new System.EventHandler(this.MinimumOrnamentChordDurationTextBox_Enter);
            this.MinimumOrnamentChordDurationTextBox.Leave += new System.EventHandler(this.MinimumOrnamentChordDurationTextBox_Leave);
            // 
            // StartMomentTextBox
            // 
            this.StartMomentTextBox.ForeColor = System.Drawing.Color.Blue;
            this.StartMomentTextBox.Location = new System.Drawing.Point(154, 18);
            this.StartMomentTextBox.Margin = new System.Windows.Forms.Padding(0);
            this.StartMomentTextBox.Name = "StartMomentTextBox";
            this.StartMomentTextBox.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.StartMomentTextBox.Size = new System.Drawing.Size(51, 20);
            this.StartMomentTextBox.TabIndex = 28;
            this.StartMomentTextBox.Enter += new System.EventHandler(this.StartMomentTextBox_Enter);
            this.StartMomentTextBox.Leave += new System.EventHandler(this.StartMomentTextBox_Leave);
            // 
            // StartMomentLabel
            // 
            this.StartMomentLabel.AutoSize = true;
            this.StartMomentLabel.Location = new System.Drawing.Point(19, 21);
            this.StartMomentLabel.Margin = new System.Windows.Forms.Padding(0);
            this.StartMomentLabel.Name = "StartMomentLabel";
            this.StartMomentLabel.Size = new System.Drawing.Size(135, 13);
            this.StartMomentLabel.TabIndex = 29;
            this.StartMomentLabel.Text = "Start at performer\'s moment";
            this.StartMomentLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // PlaySymbolsButton
            // 
            this.PlaySymbolsButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.PlaySymbolsButton.ForeColor = System.Drawing.SystemColors.ControlText;
            this.PlaySymbolsButton.Location = new System.Drawing.Point(406, 518);
            this.PlaySymbolsButton.Margin = new System.Windows.Forms.Padding(0);
            this.PlaySymbolsButton.Name = "PlaySymbolsButton";
            this.PlaySymbolsButton.Size = new System.Drawing.Size(100, 46);
            this.PlaySymbolsButton.TabIndex = 16;
            this.PlaySymbolsButton.Text = "Play Symbols";
            this.PlaySymbolsButton.UseVisualStyleBackColor = true;
            this.PlaySymbolsButton.Click += new System.EventHandler(this.PlaySymbolsButton_Click);
            // 
            // PerformLiveButton
            // 
            this.PerformLiveButton.ForeColor = System.Drawing.SystemColors.ControlText;
            this.PerformLiveButton.Location = new System.Drawing.Point(300, 518);
            this.PerformLiveButton.Margin = new System.Windows.Forms.Padding(0);
            this.PerformLiveButton.Name = "PerformLiveButton";
            this.PerformLiveButton.Size = new System.Drawing.Size(100, 46);
            this.PerformLiveButton.TabIndex = 11;
            this.PerformLiveButton.Text = "Perform Live";
            this.PerformLiveButton.UseVisualStyleBackColor = true;
            this.PerformLiveButton.Click += new System.EventHandler(this.PerformLiveButton_Click);
            // 
            // StopButton
            // 
            this.StopButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(255)))), ((int)(((byte)(200)))));
            this.StopButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.StopButton.ForeColor = System.Drawing.SystemColors.ControlText;
            this.StopButton.Location = new System.Drawing.Point(300, 518);
            this.StopButton.Margin = new System.Windows.Forms.Padding(0);
            this.StopButton.Name = "StopButton";
            this.StopButton.Size = new System.Drawing.Size(206, 46);
            this.StopButton.TabIndex = 18;
            this.StopButton.TabStop = false;
            this.StopButton.Text = "Stop";
            this.StopButton.UseVisualStyleBackColor = false;
            this.StopButton.Click += new System.EventHandler(this.StopButton_Click);
            // 
            // CommentOnSilentStaves
            // 
            this.CommentOnSilentStaves.AutoSize = true;
            this.CommentOnSilentStaves.ForeColor = System.Drawing.Color.CornflowerBlue;
            this.CommentOnSilentStaves.Location = new System.Drawing.Point(75, 24);
            this.CommentOnSilentStaves.Margin = new System.Windows.Forms.Padding(0);
            this.CommentOnSilentStaves.Name = "CommentOnSilentStaves";
            this.CommentOnSilentStaves.Size = new System.Drawing.Size(381, 13);
            this.CommentOnSilentStaves.TabIndex = 10;
            this.CommentOnSilentStaves.Text = "Silent channels/staves are played neither by the performer, nor by the assistant." +
    "\r\n";
            // 
            // SelectAllButton
            // 
            this.SelectAllButton.Location = new System.Drawing.Point(98, 0);
            this.SelectAllButton.Margin = new System.Windows.Forms.Padding(0);
            this.SelectAllButton.Name = "SelectAllButton";
            this.SelectAllButton.Size = new System.Drawing.Size(74, 22);
            this.SelectAllButton.TabIndex = 4;
            this.SelectAllButton.Text = "Select All";
            this.SelectAllButton.UseVisualStyleBackColor = true;
            this.SelectAllButton.Click += new System.EventHandler(this.SelectAllButton_Click);
            // 
            // AssistantButton
            // 
            this.AssistantButton.BackColor = System.Drawing.Color.Gainsboro;
            this.AssistantButton.Location = new System.Drawing.Point(178, 0);
            this.AssistantButton.Margin = new System.Windows.Forms.Padding(0);
            this.AssistantButton.Name = "AssistantButton";
            this.AssistantButton.Size = new System.Drawing.Size(74, 22);
            this.AssistantButton.TabIndex = 5;
            this.AssistantButton.Text = "Assistant";
            this.AssistantButton.UseVisualStyleBackColor = false;
            this.AssistantButton.Click += new System.EventHandler(this.AssistantButton_Click);
            // 
            // SilentButton
            // 
            this.SilentButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(210)))), ((int)(((byte)(210)))));
            this.SilentButton.Location = new System.Drawing.Point(338, 0);
            this.SilentButton.Margin = new System.Windows.Forms.Padding(0);
            this.SilentButton.Name = "SilentButton";
            this.SilentButton.Size = new System.Drawing.Size(74, 22);
            this.SilentButton.TabIndex = 7;
            this.SilentButton.Text = "Silent";
            this.SilentButton.UseVisualStyleBackColor = false;
            this.SilentButton.Click += new System.EventHandler(this.SilentButton_Click);
            // 
            // PerformerButton
            // 
            this.PerformerButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(210)))), ((int)(((byte)(210)))), ((int)(((byte)(255)))));
            this.PerformerButton.Location = new System.Drawing.Point(258, 0);
            this.PerformerButton.Margin = new System.Windows.Forms.Padding(0);
            this.PerformerButton.Name = "PerformerButton";
            this.PerformerButton.Size = new System.Drawing.Size(74, 22);
            this.PerformerButton.TabIndex = 6;
            this.PerformerButton.Text = "Performer";
            this.PerformerButton.UseVisualStyleBackColor = false;
            this.PerformerButton.Click += new System.EventHandler(this.PerformerButton_Click);
            // 
            // ShowScoreButton
            // 
            this.ShowScoreButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(215)))), ((int)(((byte)(225)))), ((int)(((byte)(215)))));
            this.ShowScoreButton.Location = new System.Drawing.Point(154, 518);
            this.ShowScoreButton.Margin = new System.Windows.Forms.Padding(0);
            this.ShowScoreButton.Name = "ShowScoreButton";
            this.ShowScoreButton.Size = new System.Drawing.Size(99, 22);
            this.ShowScoreButton.TabIndex = 39;
            this.ShowScoreButton.Text = "Show score";
            this.ShowScoreButton.UseVisualStyleBackColor = false;
            this.ShowScoreButton.Click += new System.EventHandler(this.ShowScoreButton_Click);
            // 
            // MpoxFilenamesComboBox
            // 
            this.MpoxFilenamesComboBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MpoxFilenamesComboBox.ForeColor = System.Drawing.Color.Blue;
            this.MpoxFilenamesComboBox.Items.AddRange(new object[] {
            "dummy text.1.mpox"});
            this.MpoxFilenamesComboBox.Location = new System.Drawing.Point(144, 11);
            this.MpoxFilenamesComboBox.Margin = new System.Windows.Forms.Padding(0);
            this.MpoxFilenamesComboBox.MaximumSize = new System.Drawing.Size(454, 0);
            this.MpoxFilenamesComboBox.Name = "MpoxFilenamesComboBox";
            this.MpoxFilenamesComboBox.Size = new System.Drawing.Size(242, 21);
            this.MpoxFilenamesComboBox.TabIndex = 19;
            this.MpoxFilenamesComboBox.SelectedIndexChanged += new System.EventHandler(this.MpoxFileNamesComboBox_SelectedIndexChanged);
            // 
            // AssistantPerformerMainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(248)))), ((int)(((byte)(248)))));
            this.ClientSize = new System.Drawing.Size(529, 762);
            this.ControlBox = false;
            this.Controls.Add(this.MpoxFilenamesComboBox);
            this.Controls.Add(this.MoritzPlayersPanel);
            this.Controls.Add(this.BottomControlsPanel);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.Name = "AssistantPerformerMainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Assistant Performer:";
            this.Click += new System.EventHandler(this.PerformanceOptionsDialog_Click);
            this.PerformersPitchOptionsGroupBox.ResumeLayout(false);
            this.PerformersPitchOptionsGroupBox.PerformLayout();
            this.PerformersVelocityOptionsGroupBox.ResumeLayout(false);
            this.PerformersVelocityOptionsGroupBox.PerformLayout();
            this.AssistantsDurationsOptionsGroupBox.ResumeLayout(false);
            this.AssistantsDurationsOptionsGroupBox.PerformLayout();
            this.BottomControlsPanel.ResumeLayout(false);
            this.BottomControlsPanel.PerformLayout();
            this.GlobalOptionsGroupBox.ResumeLayout(false);
            this.GlobalOptionsGroupBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion


        private GroupBox PerformersPitchOptionsGroupBox;
		//private Label AllPitchesIgnoredLabel;
		private RadioButton PerformerSilentRadioButton;
		//private System.Windows.Forms.RadioButton SilentRadioButton;
		private RadioButton PlayNotatedPitchesRadioButton;
		//private System.Windows.Forms.RadioButton MouseRadioButton;
		//private Label SinglePitchLabel;
		private RadioButton PlayPerformedPitchesRadioButton;
		//private System.Windows.Forms.RadioButton OnePitchKeepOtherScorePitchesRadioButton;
		//private System.Windows.Forms.RadioButton OnePitchTransposeRadioButton;

		private GroupBox PerformersVelocityOptionsGroupBox;
		private System.Windows.Forms.RadioButton PlayPerformedDynamicsRadioButton;
        private System.Windows.Forms.RadioButton PlayNotatedDynamicsRadioButton;
        private Button DeleteMpoxFileButton;
        private GroupBox AssistantsDurationsOptionsGroupBox;
        private Panel MoritzPlayersPanel;
        private Label AssistantPitchesComment;
        private RadioButton AssistantsDurationsSymbolsRelativeRadioButton;
        private RadioButton AssistantsDurationsSymbolsAbsoluteRadioButton;
        private Button KeyboardOptionsButton;
        private Button NewMpoxFileButton;
        private Panel BottomControlsPanel;
        private Button PerformerButton;
        private Button SilentButton;
        private Button AssistantButton;
        private Button SelectAllButton;
        private Label CommentOnSilentStaves;
        private ComboBox MpoxFilenamesComboBox;
        private Button PlaySymbolsButton;
        private Button PerformLiveButton;
        private Button StopButton;
        private Label SpeedFactorLabel;
        private TextBox SpeedFactorTextBox;
        private GroupBox GlobalOptionsGroupBox;
        private TextBox MinimumOrnamentChordDurationTextBox;
        private Label MinimumOrnamentChordDurationComment;
        private Label MinimumOrnamentChordDurationLabel2;
        private Label MinimumOrnamentChordDurationLabel1;
        private TextBox StartMomentTextBox;
        private Label StartMomentLabel;
        private CheckBox RepeatCheckBox;
        private Button QuitAssistantPerformerButton;
        private CheckBox SaveMidiCheckBox;
        private Label MomentsRangeLabel;
        private Label MomentPositionLabel;
        private Label MomentNumberLabel;
        private Label MomentPositionCommentLabel;
        private Label MomentNumberCommentLabel;
        private Button ShowScoreButton;
        private Button ShowMoritzButton;
        private Button QuitMoritzButton;
	}
}

