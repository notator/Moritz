namespace Krystals5ControlLibrary
{
    partial class KrystalFilenameControl
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
            if (disposing && (components != null))
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
            this.KrystalFilenameTextBox = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // KrystalFilenameTextBox
            // 
            this.KrystalFilenameTextBox.Location = new System.Drawing.Point(0, 0);
            this.KrystalFilenameTextBox.Name = "KrystalFilenameTextBox";
            this.KrystalFilenameTextBox.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.KrystalFilenameTextBox.Size = new System.Drawing.Size(182, 20);
            this.KrystalFilenameTextBox.TabIndex = 1;
            this.KrystalFilenameTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.KrystalFilenameTextBox.Leave += new System.EventHandler(this.TextBox_Leave);
            // 
            // KrystalFilenameControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.Controls.Add(this.KrystalFilenameTextBox);
            this.Name = "KrystalFilenameControl";
            this.Size = new System.Drawing.Size(182, 20);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox KrystalFilenameTextBox;
    }
}
