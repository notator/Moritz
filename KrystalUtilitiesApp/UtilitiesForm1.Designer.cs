namespace DeleteUnusedDuplicateKrystalsApp
{
    partial class UtilitiesForm1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.DeleteUnusedDuplicateKrystalsButton = new System.Windows.Forms.Button();
            this.QuitButton = new System.Windows.Forms.Button();
            this.SaveKrystalsWithNewNamesButton = new System.Windows.Forms.Button();
            this.ArchivedCodeGroupBox = new System.Windows.Forms.GroupBox();
            this.ArchiveCodeCommentLabel = new System.Windows.Forms.Label();
            this.ArchivedCodeGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // DeleteUnusedDuplicateKrystalsButton
            // 
            this.DeleteUnusedDuplicateKrystalsButton.Location = new System.Drawing.Point(12, 12);
            this.DeleteUnusedDuplicateKrystalsButton.Name = "DeleteUnusedDuplicateKrystalsButton";
            this.DeleteUnusedDuplicateKrystalsButton.Size = new System.Drawing.Size(308, 23);
            this.DeleteUnusedDuplicateKrystalsButton.TabIndex = 0;
            this.DeleteUnusedDuplicateKrystalsButton.Text = "Delete unused duplicate krystals in krystals folder";
            this.DeleteUnusedDuplicateKrystalsButton.UseVisualStyleBackColor = true;
            this.DeleteUnusedDuplicateKrystalsButton.Click += new System.EventHandler(this.DeleteUnusedDuplicateKrystalsButton_Click);
            // 
            // QuitButton
            // 
            this.QuitButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.QuitButton.Location = new System.Drawing.Point(245, 197);
            this.QuitButton.Name = "QuitButton";
            this.QuitButton.Size = new System.Drawing.Size(75, 23);
            this.QuitButton.TabIndex = 1;
            this.QuitButton.Text = "Quit";
            this.QuitButton.UseVisualStyleBackColor = true;
            this.QuitButton.Click += new System.EventHandler(this.QuitButton_Click);
            // 
            // SaveKrystalsWithNewNamesButton
            // 
            this.SaveKrystalsWithNewNamesButton.Enabled = false;
            this.SaveKrystalsWithNewNamesButton.ForeColor = System.Drawing.Color.Black;
            this.SaveKrystalsWithNewNamesButton.Location = new System.Drawing.Point(7, 77);
            this.SaveKrystalsWithNewNamesButton.Name = "SaveKrystalsWithNewNamesButton";
            this.SaveKrystalsWithNewNamesButton.Size = new System.Drawing.Size(267, 23);
            this.SaveKrystalsWithNewNamesButton.TabIndex = 2;
            this.SaveKrystalsWithNewNamesButton.Text = "Save krystals with new names (09.08.2022)";
            this.SaveKrystalsWithNewNamesButton.UseVisualStyleBackColor = true;
            this.SaveKrystalsWithNewNamesButton.Click += new System.EventHandler(this.SaveKrystalsWithNewNamesButton_Click);
            // 
            // ArchivedCodeGroupBox
            // 
            this.ArchivedCodeGroupBox.Controls.Add(this.ArchiveCodeCommentLabel);
            this.ArchivedCodeGroupBox.Controls.Add(this.SaveKrystalsWithNewNamesButton);
            this.ArchivedCodeGroupBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(170)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.ArchivedCodeGroupBox.Location = new System.Drawing.Point(12, 64);
            this.ArchivedCodeGroupBox.Name = "ArchivedCodeGroupBox";
            this.ArchivedCodeGroupBox.Size = new System.Drawing.Size(308, 124);
            this.ArchivedCodeGroupBox.TabIndex = 3;
            this.ArchivedCodeGroupBox.TabStop = false;
            this.ArchivedCodeGroupBox.Text = "Archived code";
            // 
            // ArchiveCodeCommemtLabel
            // 
            this.ArchiveCodeCommentLabel.AutoSize = true;
            this.ArchiveCodeCommentLabel.Location = new System.Drawing.Point(7, 20);
            this.ArchiveCodeCommentLabel.Name = "ArchiveCodeCommemtLabel";
            this.ArchiveCodeCommentLabel.Size = new System.Drawing.Size(267, 45);
            this.ArchiveCodeCommentLabel.TabIndex = 3;
            this.ArchiveCodeCommentLabel.Text = "This code is old, and never needs to be run again.\r\nIt has been kept for historic" +
    "al reasons, and in case\r\nanything similar needs to be written again.";
            // 
            // UtilitiesForm1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(332, 229);
            this.ControlBox = false;
            this.Controls.Add(this.ArchivedCodeGroupBox);
            this.Controls.Add(this.QuitButton);
            this.Controls.Add(this.DeleteUnusedDuplicateKrystalsButton);
            this.Name = "UtilitiesForm1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Krystal Utilities";
            this.ArchivedCodeGroupBox.ResumeLayout(false);
            this.ArchivedCodeGroupBox.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private Button QuitButton;
        private Button DeleteUnusedDuplicateKrystalsButton;
        private Button SaveKrystalsWithNewNamesButton;
        private GroupBox ArchivedCodeGroupBox;
        private Label ArchiveCodeCommentLabel;
    }
}