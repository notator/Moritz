namespace Moritz.Composer
{
    partial class NewPaletteDialog
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
            this.OKButton = new System.Windows.Forms.Button();
            this.NameTextBox = new System.Windows.Forms.TextBox();
            this.NameLabel = new System.Windows.Forms.Label();
            this.CancelButton = new System.Windows.Forms.Button();
            this.DomainLabel = new System.Windows.Forms.Label();
            this.DomainTextBox = new System.Windows.Forms.TextBox();
            this.DomainHelpLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // OKButton
            // 
            this.OKButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.OKButton.Location = new System.Drawing.Point(176, 82);
            this.OKButton.Name = "OKButton";
            this.OKButton.Size = new System.Drawing.Size(75, 23);
            this.OKButton.TabIndex = 2;
            this.OKButton.Text = "OK";
            this.OKButton.UseVisualStyleBackColor = true;
            this.OKButton.Click += new System.EventHandler(this.OKButton_Click);
            // 
            // NameTextBox
            // 
            this.NameTextBox.Location = new System.Drawing.Point(68, 18);
            this.NameTextBox.Name = "NameTextBox";
            this.NameTextBox.Size = new System.Drawing.Size(183, 20);
            this.NameTextBox.TabIndex = 0;
            this.NameTextBox.Leave += new System.EventHandler(this.NameTextBox_Leave);
            // 
            // NameLabel
            // 
            this.NameLabel.AutoSize = true;
            this.NameLabel.Location = new System.Drawing.Point(33, 21);
            this.NameLabel.Name = "NameLabel";
            this.NameLabel.Size = new System.Drawing.Size(33, 13);
            this.NameLabel.TabIndex = 3;
            this.NameLabel.Text = "name";
            this.NameLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // CancelButton
            // 
            this.CancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.CancelButton.Location = new System.Drawing.Point(95, 82);
            this.CancelButton.Name = "CancelButton";
            this.CancelButton.Size = new System.Drawing.Size(75, 23);
            this.CancelButton.TabIndex = 3;
            this.CancelButton.Text = "Cancel";
            this.CancelButton.UseVisualStyleBackColor = true;
            // 
            // DomainLabel
            // 
            this.DomainLabel.AutoSize = true;
            this.DomainLabel.Location = new System.Drawing.Point(25, 50);
            this.DomainLabel.Name = "DomainLabel";
            this.DomainLabel.Size = new System.Drawing.Size(41, 13);
            this.DomainLabel.TabIndex = 5;
            this.DomainLabel.Text = "domain";
            this.DomainLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // DomainTextBox
            // 
            this.DomainTextBox.Location = new System.Drawing.Point(68, 47);
            this.DomainTextBox.Name = "DomainTextBox";
            this.DomainTextBox.Size = new System.Drawing.Size(34, 20);
            this.DomainTextBox.TabIndex = 1;
            this.DomainTextBox.Leave += new System.EventHandler(this.DomainTextBox_Leave);
            // 
            // DomainHelpLabel
            // 
            this.DomainHelpLabel.AutoSize = true;
            this.DomainHelpLabel.ForeColor = System.Drawing.Color.RoyalBlue;
            this.DomainHelpLabel.Location = new System.Drawing.Point(108, 50);
            this.DomainHelpLabel.Name = "DomainHelpLabel";
            this.DomainHelpLabel.Size = new System.Drawing.Size(73, 13);
            this.DomainHelpLabel.TabIndex = 160;
            this.DomainHelpLabel.Text = "range [ 1..12 ]";
            // 
            // NewPaletteDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(255)))), ((int)(((byte)(245)))));
            this.ClientSize = new System.Drawing.Size(280, 117);
            this.ControlBox = false;
            this.Controls.Add(this.DomainHelpLabel);
            this.Controls.Add(this.DomainLabel);
            this.Controls.Add(this.DomainTextBox);
            this.Controls.Add(this.CancelButton);
            this.Controls.Add(this.NameLabel);
            this.Controls.Add(this.NameTextBox);
            this.Controls.Add(this.OKButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "NewPaletteDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "New Palette";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button OKButton;
        private System.Windows.Forms.TextBox NameTextBox;
        private System.Windows.Forms.Label NameLabel;
        private System.Windows.Forms.Button CancelButton;
        private System.Windows.Forms.Label DomainLabel;
        private System.Windows.Forms.TextBox DomainTextBox;
        public System.Windows.Forms.Label DomainHelpLabel;
    }
}