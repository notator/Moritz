

namespace Moritz.AssistantComposer
{
    partial class BasicPercussionControl
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
            this.MidiPitchesHelpLabel = new System.Windows.Forms.Label();
            this.VelocitiesHelpLabel = new System.Windows.Forms.Label();
            this.VelocitiesTextBox = new System.Windows.Forms.TextBox();
            this.MidiPitchesTextBox = new System.Windows.Forms.TextBox();
            this.DurationsLabel = new System.Windows.Forms.Label();
            this.DurationsHelpLabel = new System.Windows.Forms.Label();
            this.VelocitiesLabel = new System.Windows.Forms.Label();
            this.MidiPitchesLabel = new System.Windows.Forms.Label();
            this.DurationsTextBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // MidiPitchesHelpLabel
            // 
            this.MidiPitchesHelpLabel.AutoSize = true;
            this.MidiPitchesHelpLabel.ForeColor = System.Drawing.Color.RoyalBlue;
            this.MidiPitchesHelpLabel.Location = new System.Drawing.Point(605, 67);
            this.MidiPitchesHelpLabel.Name = "MidiPitchesHelpLabel";
            this.MidiPitchesHelpLabel.Size = new System.Drawing.Size(168, 13);
            this.MidiPitchesHelpLabel.TabIndex = 123;
            this.MidiPitchesHelpLabel.Text = "7 integer values in range [ 35..81 ]";
            // 
            // VelocitiesHelpLabel
            // 
            this.VelocitiesHelpLabel.AutoSize = true;
            this.VelocitiesHelpLabel.ForeColor = System.Drawing.Color.RoyalBlue;
            this.VelocitiesHelpLabel.Location = new System.Drawing.Point(605, 41);
            this.VelocitiesHelpLabel.Name = "VelocitiesHelpLabel";
            this.VelocitiesHelpLabel.Size = new System.Drawing.Size(168, 13);
            this.VelocitiesHelpLabel.TabIndex = 124;
            this.VelocitiesHelpLabel.Text = "7 integer values in range [ 0..127 ]";
            // 
            // VelocitiesTextBox
            // 
            this.VelocitiesTextBox.Location = new System.Drawing.Point(148, 38);
            this.VelocitiesTextBox.Name = "VelocitiesTextBox";
            this.VelocitiesTextBox.Size = new System.Drawing.Size(454, 20);
            this.VelocitiesTextBox.TabIndex = 1;
            this.VelocitiesTextBox.TextChanged += new System.EventHandler(this.ParameterTextBox_TextChanged);
            this.VelocitiesTextBox.Leave += new System.EventHandler(this.VelocitiesTextBox_Leave);
            // 
            // MidiPitchesTextBox
            // 
            this.MidiPitchesTextBox.Location = new System.Drawing.Point(148, 65);
            this.MidiPitchesTextBox.Name = "MidiPitchesTextBox";
            this.MidiPitchesTextBox.Size = new System.Drawing.Size(454, 20);
            this.MidiPitchesTextBox.TabIndex = 2;
            this.MidiPitchesTextBox.TextChanged += new System.EventHandler(this.ParameterTextBox_TextChanged);
            this.MidiPitchesTextBox.Leave += new System.EventHandler(this.MidiPitchesTextBox_Leave);
            // 
            // DurationsLabel
            // 
            this.DurationsLabel.AutoSize = true;
            this.DurationsLabel.ForeColor = System.Drawing.Color.Brown;
            this.DurationsLabel.Location = new System.Drawing.Point(94, 14);
            this.DurationsLabel.Name = "DurationsLabel";
            this.DurationsLabel.Size = new System.Drawing.Size(50, 13);
            this.DurationsLabel.TabIndex = 131;
            this.DurationsLabel.Text = "durations";
            // 
            // DurationsHelpLabel
            // 
            this.DurationsHelpLabel.AutoSize = true;
            this.DurationsHelpLabel.ForeColor = System.Drawing.Color.RoyalBlue;
            this.DurationsHelpLabel.Location = new System.Drawing.Point(605, 14);
            this.DurationsHelpLabel.Name = "DurationsHelpLabel";
            this.DurationsHelpLabel.Size = new System.Drawing.Size(112, 13);
            this.DurationsHelpLabel.TabIndex = 132;
            this.DurationsHelpLabel.Text = "7 integer values ( > 0 )";
            // 
            // VelocitiesLabel
            // 
            this.VelocitiesLabel.AutoSize = true;
            this.VelocitiesLabel.ForeColor = System.Drawing.Color.Brown;
            this.VelocitiesLabel.Location = new System.Drawing.Point(67, 41);
            this.VelocitiesLabel.Name = "VelocitiesLabel";
            this.VelocitiesLabel.Size = new System.Drawing.Size(77, 13);
            this.VelocitiesLabel.TabIndex = 117;
            this.VelocitiesLabel.Text = "base velocities";
            // 
            // MidiPitchesLabel
            // 
            this.MidiPitchesLabel.AutoSize = true;
            this.MidiPitchesLabel.ForeColor = System.Drawing.Color.Brown;
            this.MidiPitchesLabel.Location = new System.Drawing.Point(25, 68);
            this.MidiPitchesLabel.Name = "MidiPitchesLabel";
            this.MidiPitchesLabel.Size = new System.Drawing.Size(119, 13);
            this.MidiPitchesLabel.TabIndex = 116;
            this.MidiPitchesLabel.Text = "basic instrument indices";
            // 
            // DurationsTextBox
            // 
            this.DurationsTextBox.Location = new System.Drawing.Point(148, 11);
            this.DurationsTextBox.Name = "DurationsTextBox";
            this.DurationsTextBox.Size = new System.Drawing.Size(454, 20);
            this.DurationsTextBox.TabIndex = 0;
            this.DurationsTextBox.TextChanged += new System.EventHandler(this.ParameterTextBox_TextChanged);
            this.DurationsTextBox.Leave += new System.EventHandler(this.DurationsTextBox_Leave);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.ForeColor = System.Drawing.Color.RoyalBlue;
            this.label1.Location = new System.Drawing.Point(146, 86);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(414, 13);
            this.label1.TabIndex = 133;
            this.label1.Text = "Instrument indices defined in the percussion ornaments dialog override the basic " +
    "index.";
            // 
            // BasicPercussionControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Honeydew;
            this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Controls.Add(this.label1);
            this.Controls.Add(this.DurationsTextBox);
            this.Controls.Add(this.DurationsHelpLabel);
            this.Controls.Add(this.MidiPitchesTextBox);
            this.Controls.Add(this.VelocitiesTextBox);
            this.Controls.Add(this.VelocitiesHelpLabel);
            this.Controls.Add(this.MidiPitchesHelpLabel);
            this.Controls.Add(this.MidiPitchesLabel);
            this.Controls.Add(this.VelocitiesLabel);
            this.Controls.Add(this.DurationsLabel);
            this.Name = "BasicPercussionControl";
            this.Size = new System.Drawing.Size(792, 105);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        public System.Windows.Forms.TextBox VelocitiesTextBox;
        public System.Windows.Forms.TextBox MidiPitchesTextBox;
        public System.Windows.Forms.Label DurationsLabel;
        public System.Windows.Forms.Label VelocitiesLabel;
        public System.Windows.Forms.Label MidiPitchesLabel;
        public System.Windows.Forms.TextBox DurationsTextBox;
        public System.Windows.Forms.Label MidiPitchesHelpLabel;
        public System.Windows.Forms.Label VelocitiesHelpLabel;
        public System.Windows.Forms.Label DurationsHelpLabel;
        public System.Windows.Forms.Label label1;



    }
}
