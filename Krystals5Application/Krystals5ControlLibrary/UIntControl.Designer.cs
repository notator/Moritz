namespace Krystals5ControlLibrary
{
    partial class UnsignedIntControl
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
            this.UIntTextBox = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // UIntTextBox
            // 
            this.UIntTextBox.Location = new System.Drawing.Point(0, 0);
            this.UIntTextBox.Name = "UIntTextBox";
            this.UIntTextBox.Size = new System.Drawing.Size(44, 20);
            this.UIntTextBox.TabIndex = 0;
            this.UIntTextBox.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(this.UIntTextBox_PreviewKeyDown);
            this.UIntTextBox.Leave += new System.EventHandler(this.UIntTextBox_Leave);
            // 
            // UnsignedIntControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.UIntTextBox);
            this.Name = "UnsignedIntControl";
            this.Size = new System.Drawing.Size(44, 20);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox UIntTextBox;
    }
}
