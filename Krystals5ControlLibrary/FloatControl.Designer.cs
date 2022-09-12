namespace Krystals5ControlLibrary
{
    partial class FloatControl
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
            if( disposing && ( components != null ) )
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
            this.FloatTextBox = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // FloatTextBox
            // 
            this.FloatTextBox.Location = new System.Drawing.Point(0, 0);
            this.FloatTextBox.Name = "FloatTextBox";
            this.FloatTextBox.Size = new System.Drawing.Size(44, 20);
            this.FloatTextBox.TabIndex = 1;
            this.FloatTextBox.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(this.FloatTextBox_PreviewKeyDown);
            this.FloatTextBox.Leave += new System.EventHandler(this.FloatTextBox_Leave);
            // 
            // FloatControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.FloatTextBox);
            this.Name = "FloatControl";
            this.Size = new System.Drawing.Size(44, 20);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox FloatTextBox;
    }
}
