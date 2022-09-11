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
            this.QuitButton.Location = new System.Drawing.Point(245, 174);
            this.QuitButton.Name = "QuitButton";
            this.QuitButton.Size = new System.Drawing.Size(75, 23);
            this.QuitButton.TabIndex = 1;
            this.QuitButton.Text = "Quit";
            this.QuitButton.UseVisualStyleBackColor = true;
            this.QuitButton.Click += new System.EventHandler(this.QuitButton_Click);
            // 
            // UtilitiesForm1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(332, 209);
            this.ControlBox = false;
            this.Controls.Add(this.QuitButton);
            this.Controls.Add(this.DeleteUnusedDuplicateKrystalsButton);
            this.Name = "UtilitiesForm1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Krystal Utilities";
            this.ResumeLayout(false);

        }

        #endregion

        private Button QuitButton;
        private Button DeleteUnusedDuplicateKrystalsButton;
    }
}