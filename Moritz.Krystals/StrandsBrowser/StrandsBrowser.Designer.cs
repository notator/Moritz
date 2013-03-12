namespace Moritz.Krystals
{
    partial class StrandsBrowser
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
            this.StrandsTreeView = new System.Windows.Forms.TreeView();
            this.LineStrandLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // StrandsTreeView
            // 
            this.StrandsTreeView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.StrandsTreeView.Location = new System.Drawing.Point(0, 0);
            this.StrandsTreeView.Name = "StrandsTreeView";
            this.StrandsTreeView.Size = new System.Drawing.Size(232, 235);
            this.StrandsTreeView.TabIndex = 0;
            // 
            // LineStrandLabel
            // 
            this.LineStrandLabel.AutoSize = true;
            this.LineStrandLabel.Location = new System.Drawing.Point(3, 3);
            this.LineStrandLabel.Margin = new System.Windows.Forms.Padding(0);
            this.LineStrandLabel.Name = "LineStrandLabel";
            this.LineStrandLabel.Size = new System.Drawing.Size(84, 13);
            this.LineStrandLabel.TabIndex = 1;
            this.LineStrandLabel.Text = "LineStrandLabel";
            // 
            // StrandsBrowser
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(232, 235);
            this.Controls.Add(this.LineStrandLabel);
            this.Controls.Add(this.StrandsTreeView);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Name = "StrandsBrowser";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "StrandsBrowser";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TreeView StrandsTreeView;
        private System.Windows.Forms.Label LineStrandLabel;
    }
}