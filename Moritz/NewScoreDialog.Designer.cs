namespace Moritz
{
    partial class NewScoreDialog
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
            this.AlgorithmsComboBoxLabel = new System.Windows.Forms.Label();
            this.OKButton = new System.Windows.Forms.Button();
            this.Cancel_Button = new System.Windows.Forms.Button();
            this.AlgorithmComboBox = new System.Windows.Forms.ComboBox();
            this.ScoreNameTextBox = new System.Windows.Forms.TextBox();
            this.ScoreNameLabel = new System.Windows.Forms.Label();
            this.ExistingScoresLabel = new System.Windows.Forms.Label();
            this.ExistingScoresListBox = new System.Windows.Forms.ListBox();
            this.ErrorHelpLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // AlgorithmsComboBoxLabel
            // 
            this.AlgorithmsComboBoxLabel.AutoSize = true;
            this.AlgorithmsComboBoxLabel.ForeColor = System.Drawing.Color.RoyalBlue;
            this.AlgorithmsComboBoxLabel.Location = new System.Drawing.Point(192, 9);
            this.AlgorithmsComboBoxLabel.Name = "AlgorithmsComboBoxLabel";
            this.AlgorithmsComboBoxLabel.Size = new System.Drawing.Size(50, 14);
            this.AlgorithmsComboBoxLabel.TabIndex = 0;
            this.AlgorithmsComboBoxLabel.Text = "algorithm";
            // 
            // OKButton
            // 
            this.OKButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.OKButton.Location = new System.Drawing.Point(268, 175);
            this.OKButton.Name = "OKButton";
            this.OKButton.Size = new System.Drawing.Size(67, 23);
            this.OKButton.TabIndex = 1;
            this.OKButton.Text = "OK";
            this.OKButton.UseVisualStyleBackColor = true;
            this.OKButton.Click += new System.EventHandler(this.OKButton_Click);
            // 
            // CancelButton
            // 
            this.Cancel_Button.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Cancel_Button.Location = new System.Drawing.Point(192, 175);
            this.Cancel_Button.Name = "CancelButton";
            this.Cancel_Button.Size = new System.Drawing.Size(67, 23);
            this.Cancel_Button.TabIndex = 2;
            this.Cancel_Button.Text = "Cancel";
            this.Cancel_Button.UseVisualStyleBackColor = true;
            this.Cancel_Button.Click += new System.EventHandler(this.CancelButton_Click);
            // 
            // AlgorithmComboBox
            // 
            this.AlgorithmComboBox.FormattingEnabled = true;
            this.AlgorithmComboBox.Location = new System.Drawing.Point(192, 26);
            this.AlgorithmComboBox.Name = "AlgorithmComboBox";
            this.AlgorithmComboBox.Size = new System.Drawing.Size(143, 22);
            this.AlgorithmComboBox.TabIndex = 4;
            // 
            // ScoreNameTextBox
            // 
            this.ScoreNameTextBox.Location = new System.Drawing.Point(192, 70);
            this.ScoreNameTextBox.Name = "ScoreNameTextBox";
            this.ScoreNameTextBox.Size = new System.Drawing.Size(143, 20);
            this.ScoreNameTextBox.TabIndex = 0;
            this.ScoreNameTextBox.TextChanged += new System.EventHandler(this.ScoreNameTextBox_TextChanged);
            // 
            // ScoreNameLabel
            // 
            this.ScoreNameLabel.AutoSize = true;
            this.ScoreNameLabel.ForeColor = System.Drawing.Color.RoyalBlue;
            this.ScoreNameLabel.Location = new System.Drawing.Point(192, 53);
            this.ScoreNameLabel.Name = "ScoreNameLabel";
            this.ScoreNameLabel.Size = new System.Drawing.Size(64, 14);
            this.ScoreNameLabel.TabIndex = 7;
            this.ScoreNameLabel.Text = "score name";
            // 
            // ExistingScoresLabel
            // 
            this.ExistingScoresLabel.AutoSize = true;
            this.ExistingScoresLabel.ForeColor = System.Drawing.Color.RoyalBlue;
            this.ExistingScoresLabel.Location = new System.Drawing.Point(24, 9);
            this.ExistingScoresLabel.Name = "ExistingScoresLabel";
            this.ExistingScoresLabel.Size = new System.Drawing.Size(110, 14);
            this.ExistingScoresLabel.TabIndex = 8;
            this.ExistingScoresLabel.Text = "existing score names";
            // 
            // ExistingScoresListBox
            // 
            this.ExistingScoresListBox.FormattingEnabled = true;
            this.ExistingScoresListBox.ItemHeight = 14;
            this.ExistingScoresListBox.Location = new System.Drawing.Point(24, 26);
            this.ExistingScoresListBox.Name = "ExistingScoresListBox";
            this.ExistingScoresListBox.Size = new System.Drawing.Size(143, 172);
            this.ExistingScoresListBox.TabIndex = 9;
            // 
            // ErrorHelpLabel
            // 
            this.ErrorHelpLabel.AutoSize = true;
            this.ErrorHelpLabel.ForeColor = System.Drawing.Color.RoyalBlue;
            this.ErrorHelpLabel.Location = new System.Drawing.Point(189, 93);
            this.ErrorHelpLabel.Name = "ErrorHelpLabel";
            this.ErrorHelpLabel.Size = new System.Drawing.Size(154, 42);
            this.ErrorHelpLabel.TabIndex = 10;
            this.ErrorHelpLabel.Text = "New score names are illegal if\r\nthey contain illegal characters,\r\nor the score al" +
    "ready exists.";
            // 
            // NewScoreDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 14F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(255)))), ((int)(((byte)(245)))));
            this.ClientSize = new System.Drawing.Size(358, 220);
            this.ControlBox = false;
            this.Controls.Add(this.ErrorHelpLabel);
            this.Controls.Add(this.ExistingScoresListBox);
            this.Controls.Add(this.ExistingScoresLabel);
            this.Controls.Add(this.ScoreNameLabel);
            this.Controls.Add(this.ScoreNameTextBox);
            this.Controls.Add(this.AlgorithmComboBox);
            this.Controls.Add(this.Cancel_Button);
            this.Controls.Add(this.OKButton);
            this.Controls.Add(this.AlgorithmsComboBoxLabel);
            this.Font = new System.Drawing.Font("Arial", 8F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "NewScoreDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "New Score";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label AlgorithmsComboBoxLabel;
        private System.Windows.Forms.Button OKButton;
        private System.Windows.Forms.Button Cancel_Button;
        private System.Windows.Forms.Label ScoreNameLabel;
        public System.Windows.Forms.ComboBox AlgorithmComboBox;
        public System.Windows.Forms.TextBox ScoreNameTextBox;
        private System.Windows.Forms.Label ExistingScoresLabel;
        private System.Windows.Forms.ListBox ExistingScoresListBox;
        private System.Windows.Forms.Label ErrorHelpLabel;
    }
}