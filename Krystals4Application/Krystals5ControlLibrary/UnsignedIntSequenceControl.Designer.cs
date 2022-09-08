namespace Krystals5ControlLibrary
{
    partial class UnsignedIntSeqControl
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
            this.UIntSeqTextBox = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // UIntSeqTextBox
            // 
            this.UIntSeqTextBox.Location = new System.Drawing.Point(0, 0);
            this.UIntSeqTextBox.Name = "UIntSeqTextBox";
            this.UIntSeqTextBox.Size = new System.Drawing.Size(82, 20);
            this.UIntSeqTextBox.TabIndex = 0;
            this.UIntSeqTextBox.WordWrap = false;
            this.UIntSeqTextBox.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(this.UIntSeqTextBox_PreviewKeyDown);
            this.UIntSeqTextBox.Leave += new System.EventHandler(this.UIntSeqTextBox_Leave);
            // 
            // UnsignedIntSeqControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.Controls.Add(this.UIntSeqTextBox);
            this.Name = "UnsignedIntSeqControl";
            this.Size = new System.Drawing.Size(82, 20);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox UIntSeqTextBox;





    }
}
