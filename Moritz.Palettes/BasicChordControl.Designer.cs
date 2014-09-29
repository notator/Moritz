namespace Moritz.Palettes
{
    partial class BasicChordControl
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.VerticalVelocityFactorsTextBox = new System.Windows.Forms.TextBox();
            this.InversionIndicesTextBox = new System.Windows.Forms.TextBox();
            this.MidiPitchesHelpLabel = new System.Windows.Forms.Label();
            this.RootInversionTextBox = new System.Windows.Forms.TextBox();
            this.VelocitiesHelpLabel = new System.Windows.Forms.Label();
            this.ChordDensitiesTextBox = new System.Windows.Forms.TextBox();
            this.ChordDensitiesHelpLabel = new System.Windows.Forms.Label();
            this.VelocitiesTextBox = new System.Windows.Forms.TextBox();
            this.VerticalVelocityFactorsHelpLabel = new System.Windows.Forms.Label();
            this.MidiPitchesTextBox = new System.Windows.Forms.TextBox();
            this.InversionIndicesHelpLabel = new System.Windows.Forms.Label();
            this.VerticalVelocityFactorsLabel = new System.Windows.Forms.Label();
            this.RootInversionHelpLabel = new System.Windows.Forms.Label();
            this.InversionIndicesLabel = new System.Windows.Forms.Label();
            this.DurationsLabel = new System.Windows.Forms.Label();
            this.RootInversionLabel = new System.Windows.Forms.Label();
            this.ChordDensitiesLabel = new System.Windows.Forms.Label();
            this.DurationsHelpLabel = new System.Windows.Forms.Label();
            this.ChordOffsLabel = new System.Windows.Forms.Label();
            this.VelocitiesLabel = new System.Windows.Forms.Label();
            this.ChordOffsTextBox = new System.Windows.Forms.TextBox();
            this.MidiPitchesLabel = new System.Windows.Forms.Label();
            this.ChordOffsHelpLabel = new System.Windows.Forms.Label();
            this.DurationsTextBox = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // VerticalVelocityFactorsTextBox
            // 
            this.VerticalVelocityFactorsTextBox.Location = new System.Drawing.Point(178, 195);
            this.VerticalVelocityFactorsTextBox.Name = "VerticalVelocityFactorsTextBox";
            this.VerticalVelocityFactorsTextBox.Size = new System.Drawing.Size(361, 20);
            this.VerticalVelocityFactorsTextBox.TabIndex = 9;
            this.VerticalVelocityFactorsTextBox.TextChanged += new System.EventHandler(this.ParameterTextBox_TextChanged);
            this.VerticalVelocityFactorsTextBox.Leave += new System.EventHandler(this.VerticalVelocityFactorsTextBox_Leave);
            // 
            // InversionIndicesTextBox
            // 
            this.InversionIndicesTextBox.Location = new System.Drawing.Point(178, 169);
            this.InversionIndicesTextBox.Name = "InversionIndicesTextBox";
            this.InversionIndicesTextBox.Size = new System.Drawing.Size(361, 20);
            this.InversionIndicesTextBox.TabIndex = 8;
            this.InversionIndicesTextBox.TextChanged += new System.EventHandler(this.ParameterTextBox_TextChanged);
            this.InversionIndicesTextBox.Leave += new System.EventHandler(this.InversionIndicesTextBox_Leave);
            // 
            // MidiPitchesHelpLabel
            // 
            this.MidiPitchesHelpLabel.AutoSize = true;
            this.MidiPitchesHelpLabel.ForeColor = System.Drawing.Color.RoyalBlue;
            this.MidiPitchesHelpLabel.Location = new System.Drawing.Point(605, 63);
            this.MidiPitchesHelpLabel.Name = "MidiPitchesHelpLabel";
            this.MidiPitchesHelpLabel.Size = new System.Drawing.Size(168, 13);
            this.MidiPitchesHelpLabel.TabIndex = 123;
            this.MidiPitchesHelpLabel.Text = "7 integer values in range [ 0..127 ]";
            // 
            // RootInversionTextBox
            // 
            this.RootInversionTextBox.Location = new System.Drawing.Point(178, 143);
            this.RootInversionTextBox.Name = "RootInversionTextBox";
            this.RootInversionTextBox.Size = new System.Drawing.Size(361, 20);
            this.RootInversionTextBox.TabIndex = 7;
            this.RootInversionTextBox.TextChanged += new System.EventHandler(this.ParameterTextBox_TextChanged);
            this.RootInversionTextBox.Leave += new System.EventHandler(this.RootInversionTextBox_Leave);
            // 
            // VelocitiesHelpLabel
            // 
            this.VelocitiesHelpLabel.AutoSize = true;
            this.VelocitiesHelpLabel.ForeColor = System.Drawing.Color.RoyalBlue;
            this.VelocitiesHelpLabel.Location = new System.Drawing.Point(605, 37);
            this.VelocitiesHelpLabel.Name = "VelocitiesHelpLabel";
            this.VelocitiesHelpLabel.Size = new System.Drawing.Size(168, 13);
            this.VelocitiesHelpLabel.TabIndex = 124;
            this.VelocitiesHelpLabel.Text = "7 integer values in range [ 0..127 ]";
            // 
            // ChordDensitiesTextBox
            // 
            this.ChordDensitiesTextBox.Location = new System.Drawing.Point(148, 115);
            this.ChordDensitiesTextBox.Name = "ChordDensitiesTextBox";
            this.ChordDensitiesTextBox.Size = new System.Drawing.Size(454, 20);
            this.ChordDensitiesTextBox.TabIndex = 6;
            this.ChordDensitiesTextBox.TextChanged += new System.EventHandler(this.ParameterTextBox_TextChanged);
            this.ChordDensitiesTextBox.Leave += new System.EventHandler(this.ChordDensitiesTextBox_Leave);
            // 
            // ChordDensitiesHelpLabel
            // 
            this.ChordDensitiesHelpLabel.AutoSize = true;
            this.ChordDensitiesHelpLabel.ForeColor = System.Drawing.Color.RoyalBlue;
            this.ChordDensitiesHelpLabel.Location = new System.Drawing.Point(605, 118);
            this.ChordDensitiesHelpLabel.Name = "ChordDensitiesHelpLabel";
            this.ChordDensitiesHelpLabel.Size = new System.Drawing.Size(168, 13);
            this.ChordDensitiesHelpLabel.TabIndex = 125;
            this.ChordDensitiesHelpLabel.Text = "7 integer values in range [ 1..128 ]";
            // 
            // VelocitiesTextBox
            // 
            this.VelocitiesTextBox.Location = new System.Drawing.Point(148, 34);
            this.VelocitiesTextBox.Name = "VelocitiesTextBox";
            this.VelocitiesTextBox.Size = new System.Drawing.Size(454, 20);
            this.VelocitiesTextBox.TabIndex = 1;
            this.VelocitiesTextBox.TextChanged += new System.EventHandler(this.ParameterTextBox_TextChanged);
            this.VelocitiesTextBox.Leave += new System.EventHandler(this.VelocitiesTextBox_Leave);
            // 
            // VerticalVelocityFactorsHelpLabel
            // 
            this.VerticalVelocityFactorsHelpLabel.AutoSize = true;
            this.VerticalVelocityFactorsHelpLabel.ForeColor = System.Drawing.Color.RoyalBlue;
            this.VerticalVelocityFactorsHelpLabel.Location = new System.Drawing.Point(542, 198);
            this.VerticalVelocityFactorsHelpLabel.Name = "VerticalVelocityFactorsHelpLabel";
            this.VerticalVelocityFactorsHelpLabel.Size = new System.Drawing.Size(162, 13);
            this.VerticalVelocityFactorsHelpLabel.TabIndex = 128;
            this.VerticalVelocityFactorsHelpLabel.Text = "7 float values in range [ 0.0..1.0 ]";
            // 
            // MidiPitchesTextBox
            // 
            this.MidiPitchesTextBox.Location = new System.Drawing.Point(148, 61);
            this.MidiPitchesTextBox.Name = "MidiPitchesTextBox";
            this.MidiPitchesTextBox.Size = new System.Drawing.Size(454, 20);
            this.MidiPitchesTextBox.TabIndex = 2;
            this.MidiPitchesTextBox.TextChanged += new System.EventHandler(this.ParameterTextBox_TextChanged);
            this.MidiPitchesTextBox.Leave += new System.EventHandler(this.MidiPitchesTextBox_Leave);
            // 
            // InversionIndicesHelpLabel
            // 
            this.InversionIndicesHelpLabel.AutoSize = true;
            this.InversionIndicesHelpLabel.ForeColor = System.Drawing.Color.RoyalBlue;
            this.InversionIndicesHelpLabel.Location = new System.Drawing.Point(542, 172);
            this.InversionIndicesHelpLabel.Name = "InversionIndicesHelpLabel";
            this.InversionIndicesHelpLabel.Size = new System.Drawing.Size(162, 13);
            this.InversionIndicesHelpLabel.TabIndex = 127;
            this.InversionIndicesHelpLabel.Text = "7 integer values in range [ 0..19 ]";
            // 
            // VerticalVelocityFactorsLabel
            // 
            this.VerticalVelocityFactorsLabel.AutoSize = true;
            this.VerticalVelocityFactorsLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(60)))), ((int)(((byte)(60)))));
            this.VerticalVelocityFactorsLabel.Location = new System.Drawing.Point(16, 198);
            this.VerticalVelocityFactorsLabel.Name = "VerticalVelocityFactorsLabel";
            this.VerticalVelocityFactorsLabel.Size = new System.Drawing.Size(154, 13);
            this.VerticalVelocityFactorsLabel.TabIndex = 121;
            this.VerticalVelocityFactorsLabel.Text = "( vertical velocity factors ( 1.0 ))";
            // 
            // RootInversionHelpLabel
            // 
            this.RootInversionHelpLabel.AutoSize = true;
            this.RootInversionHelpLabel.ForeColor = System.Drawing.Color.RoyalBlue;
            this.RootInversionHelpLabel.Location = new System.Drawing.Point(542, 146);
            this.RootInversionHelpLabel.Name = "RootInversionHelpLabel";
            this.RootInversionHelpLabel.Size = new System.Drawing.Size(174, 13);
            this.RootInversionHelpLabel.TabIndex = 126;
            this.RootInversionHelpLabel.Text = "11 integer values in range [ 1..128 ]";
            // 
            // InversionIndicesLabel
            // 
            this.InversionIndicesLabel.AutoSize = true;
            this.InversionIndicesLabel.ForeColor = System.Drawing.Color.Brown;
            this.InversionIndicesLabel.Location = new System.Drawing.Point(90, 172);
            this.InversionIndicesLabel.Name = "InversionIndicesLabel";
            this.InversionIndicesLabel.Size = new System.Drawing.Size(85, 13);
            this.InversionIndicesLabel.TabIndex = 120;
            this.InversionIndicesLabel.Text = "inversion indices";
            // 
            // DurationsLabel
            // 
            this.DurationsLabel.AutoSize = true;
            this.DurationsLabel.ForeColor = System.Drawing.Color.Brown;
            this.DurationsLabel.Location = new System.Drawing.Point(94, 10);
            this.DurationsLabel.Name = "DurationsLabel";
            this.DurationsLabel.Size = new System.Drawing.Size(50, 13);
            this.DurationsLabel.TabIndex = 131;
            this.DurationsLabel.Text = "durations";
            // 
            // RootInversionLabel
            // 
            this.RootInversionLabel.AutoSize = true;
            this.RootInversionLabel.ForeColor = System.Drawing.Color.Brown;
            this.RootInversionLabel.Location = new System.Drawing.Point(105, 146);
            this.RootInversionLabel.Name = "RootInversionLabel";
            this.RootInversionLabel.Size = new System.Drawing.Size(70, 13);
            this.RootInversionLabel.TabIndex = 119;
            this.RootInversionLabel.Text = "root inversion";
            // 
            // ChordDensitiesLabel
            // 
            this.ChordDensitiesLabel.AutoSize = true;
            this.ChordDensitiesLabel.ForeColor = System.Drawing.Color.Brown;
            this.ChordDensitiesLabel.Location = new System.Drawing.Point(40, 118);
            this.ChordDensitiesLabel.Name = "ChordDensitiesLabel";
            this.ChordDensitiesLabel.Size = new System.Drawing.Size(104, 13);
            this.ChordDensitiesLabel.TabIndex = 118;
            this.ChordDensitiesLabel.Text = "base chord densities";
            // 
            // DurationsHelpLabel
            // 
            this.DurationsHelpLabel.AutoSize = true;
            this.DurationsHelpLabel.ForeColor = System.Drawing.Color.RoyalBlue;
            this.DurationsHelpLabel.Location = new System.Drawing.Point(605, 10);
            this.DurationsHelpLabel.Name = "DurationsHelpLabel";
            this.DurationsHelpLabel.Size = new System.Drawing.Size(112, 13);
            this.DurationsHelpLabel.TabIndex = 132;
            this.DurationsHelpLabel.Text = "7 integer values ( > 0 )";
            // 
            // ChordOffsLabel
            // 
            this.ChordOffsLabel.AutoSize = true;
            this.ChordOffsLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(60)))), ((int)(((byte)(60)))));
            this.ChordOffsLabel.Location = new System.Drawing.Point(62, 91);
            this.ChordOffsLabel.Name = "ChordOffsLabel";
            this.ChordOffsLabel.Size = new System.Drawing.Size(78, 13);
            this.ChordOffsLabel.TabIndex = 134;
            this.ChordOffsLabel.Text = "( chord offs (1))";
            // 
            // VelocitiesLabel
            // 
            this.VelocitiesLabel.AutoSize = true;
            this.VelocitiesLabel.ForeColor = System.Drawing.Color.Brown;
            this.VelocitiesLabel.Location = new System.Drawing.Point(67, 37);
            this.VelocitiesLabel.Name = "VelocitiesLabel";
            this.VelocitiesLabel.Size = new System.Drawing.Size(77, 13);
            this.VelocitiesLabel.TabIndex = 117;
            this.VelocitiesLabel.Text = "base velocities";
            // 
            // ChordOffsTextBox
            // 
            this.ChordOffsTextBox.Location = new System.Drawing.Point(148, 88);
            this.ChordOffsTextBox.Name = "ChordOffsTextBox";
            this.ChordOffsTextBox.Size = new System.Drawing.Size(454, 20);
            this.ChordOffsTextBox.TabIndex = 5;
            this.ChordOffsTextBox.TextChanged += new System.EventHandler(this.ParameterTextBox_TextChanged);
            this.ChordOffsTextBox.Leave += new System.EventHandler(this.ChordOffsTextBox_Leave);
            // 
            // MidiPitchesLabel
            // 
            this.MidiPitchesLabel.AutoSize = true;
            this.MidiPitchesLabel.ForeColor = System.Drawing.Color.Brown;
            this.MidiPitchesLabel.Location = new System.Drawing.Point(56, 64);
            this.MidiPitchesLabel.Name = "MidiPitchesLabel";
            this.MidiPitchesLabel.Size = new System.Drawing.Size(88, 13);
            this.MidiPitchesLabel.TabIndex = 116;
            this.MidiPitchesLabel.Text = "base midi pitches";
            // 
            // ChordOffsHelpLabel
            // 
            this.ChordOffsHelpLabel.AutoSize = true;
            this.ChordOffsHelpLabel.ForeColor = System.Drawing.Color.RoyalBlue;
            this.ChordOffsHelpLabel.Location = new System.Drawing.Point(605, 91);
            this.ChordOffsHelpLabel.Name = "ChordOffsHelpLabel";
            this.ChordOffsHelpLabel.Size = new System.Drawing.Size(173, 13);
            this.ChordOffsHelpLabel.TabIndex = 135;
            this.ChordOffsHelpLabel.Text = "7 boolean values ( 1=true, 0=false )";
            // 
            // DurationsTextBox
            // 
            this.DurationsTextBox.Location = new System.Drawing.Point(148, 7);
            this.DurationsTextBox.Name = "DurationsTextBox";
            this.DurationsTextBox.Size = new System.Drawing.Size(454, 20);
            this.DurationsTextBox.TabIndex = 0;
            this.DurationsTextBox.TextChanged += new System.EventHandler(this.ParameterTextBox_TextChanged);
            this.DurationsTextBox.Leave += new System.EventHandler(this.DurationsTextBox_Leave);
            // 
            // BasicChordControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Honeydew;
            this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Controls.Add(this.DurationsTextBox);
            this.Controls.Add(this.ChordOffsHelpLabel);
            this.Controls.Add(this.ChordOffsTextBox);
            this.Controls.Add(this.DurationsHelpLabel);
            this.Controls.Add(this.RootInversionHelpLabel);
            this.Controls.Add(this.InversionIndicesHelpLabel);
            this.Controls.Add(this.MidiPitchesTextBox);
            this.Controls.Add(this.VerticalVelocityFactorsHelpLabel);
            this.Controls.Add(this.VelocitiesTextBox);
            this.Controls.Add(this.ChordDensitiesHelpLabel);
            this.Controls.Add(this.ChordDensitiesTextBox);
            this.Controls.Add(this.VelocitiesHelpLabel);
            this.Controls.Add(this.RootInversionTextBox);
            this.Controls.Add(this.MidiPitchesHelpLabel);
            this.Controls.Add(this.InversionIndicesTextBox);
            this.Controls.Add(this.VerticalVelocityFactorsTextBox);
            this.Controls.Add(this.MidiPitchesLabel);
            this.Controls.Add(this.VelocitiesLabel);
            this.Controls.Add(this.ChordOffsLabel);
            this.Controls.Add(this.ChordDensitiesLabel);
            this.Controls.Add(this.RootInversionLabel);
            this.Controls.Add(this.DurationsLabel);
            this.Controls.Add(this.InversionIndicesLabel);
            this.Controls.Add(this.VerticalVelocityFactorsLabel);
            this.Name = "BasicChordControl";
            this.Size = new System.Drawing.Size(792, 221);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        public System.Windows.Forms.TextBox VerticalVelocityFactorsTextBox;
        public System.Windows.Forms.TextBox InversionIndicesTextBox;
        public System.Windows.Forms.TextBox RootInversionTextBox;
        public System.Windows.Forms.TextBox ChordDensitiesTextBox;
        public System.Windows.Forms.TextBox VelocitiesTextBox;
        public System.Windows.Forms.TextBox MidiPitchesTextBox;
        public System.Windows.Forms.Label VerticalVelocityFactorsLabel;
        public System.Windows.Forms.Label InversionIndicesLabel;
        public System.Windows.Forms.Label DurationsLabel;
        public System.Windows.Forms.Label RootInversionLabel;
        public System.Windows.Forms.Label ChordDensitiesLabel;
        public System.Windows.Forms.Label ChordOffsLabel;
        public System.Windows.Forms.Label VelocitiesLabel;
        public System.Windows.Forms.TextBox ChordOffsTextBox;
        public System.Windows.Forms.Label MidiPitchesLabel;
        public System.Windows.Forms.TextBox DurationsTextBox;
        public System.Windows.Forms.Label MidiPitchesHelpLabel;
        public System.Windows.Forms.Label VelocitiesHelpLabel;
        public System.Windows.Forms.Label ChordDensitiesHelpLabel;
        public System.Windows.Forms.Label VerticalVelocityFactorsHelpLabel;
        public System.Windows.Forms.Label InversionIndicesHelpLabel;
        public System.Windows.Forms.Label RootInversionHelpLabel;
        public System.Windows.Forms.Label DurationsHelpLabel;
        public System.Windows.Forms.Label ChordOffsHelpLabel;



    }
}
