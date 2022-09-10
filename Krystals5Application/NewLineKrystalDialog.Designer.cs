namespace Krystals5Application
{
    partial class NewLineKrystalDialog
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NewLineKrystalDialog));
            this.OKBtn = new System.Windows.Forms.Button();
            this.CancelBtn = new System.Windows.Forms.Button();
            this.UintSeqLabel = new System.Windows.Forms.Label();
            this.LineLabel = new System.Windows.Forms.Label();
            this.UnsignedIntSequenceControl1 = new Krystals5ControlLibrary.UnsignedIntSeqControl();
            this.SuspendLayout();
            // 
            // OKBtn
            // 
            this.OKBtn.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.OKBtn.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.OKBtn.Location = new System.Drawing.Point(35, 108);
            this.OKBtn.Name = "OKBtn";
            this.OKBtn.Size = new System.Drawing.Size(75, 23);
            this.OKBtn.TabIndex = 3;
            this.OKBtn.Text = "OK";
            this.OKBtn.Click += new System.EventHandler(this.OKBtn_Click);
            // 
            // CancelBtn
            // 
            this.CancelBtn.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.CancelBtn.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.CancelBtn.Location = new System.Drawing.Point(130, 108);
            this.CancelBtn.Name = "CancelBtn";
            this.CancelBtn.Size = new System.Drawing.Size(75, 23);
            this.CancelBtn.TabIndex = 4;
            this.CancelBtn.Text = "Cancel";
            this.CancelBtn.Click += new System.EventHandler(this.CancelBtn_Click);
            // 
            // UintSeqLabel
            // 
            this.UintSeqLabel.AutoSize = true;
            this.UintSeqLabel.BackColor = System.Drawing.SystemColors.ControlLight;
            this.UintSeqLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.UintSeqLabel.ForeColor = System.Drawing.Color.LightSlateGray;
            this.UintSeqLabel.Location = new System.Drawing.Point(32, 58);
            this.UintSeqLabel.Name = "UintSeqLabel";
            this.UintSeqLabel.Size = new System.Drawing.Size(194, 26);
            this.UintSeqLabel.TabIndex = 25;
            this.UintSeqLabel.Text = "The line can contain any number of\r\nunsigned integers separated by spaces.";
            // 
            // LineLabel
            // 
            this.LineLabel.AutoSize = true;
            this.LineLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.LineLabel.Location = new System.Drawing.Point(63, 32);
            this.LineLabel.Name = "LineLabel";
            this.LineLabel.Size = new System.Drawing.Size(26, 13);
            this.LineLabel.TabIndex = 24;
            this.LineLabel.Text = "line:";
            // 
            // UnsignedIntSequenceControl1
            // 
            this.UnsignedIntSequenceControl1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.UnsignedIntSequenceControl1.BackColor = System.Drawing.SystemColors.Window;
            this.UnsignedIntSequenceControl1.Location = new System.Drawing.Point(86, 28);
            this.UnsignedIntSequenceControl1.Name = "UnsignedIntSequenceControl1";
            this.UnsignedIntSequenceControl1.Sequence = ((System.Text.StringBuilder)(resources.GetObject("UnsignedIntSequenceControl1.Sequence")));
            this.UnsignedIntSequenceControl1.Size = new System.Drawing.Size(83, 20);
            this.UnsignedIntSequenceControl1.TabIndex = 2;
            // 
            // NewLineKrystalDialog
            // 
            this.AcceptButton = this.OKBtn;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ControlLight;
            this.CancelButton = this.CancelBtn;
            this.ClientSize = new System.Drawing.Size(241, 159);
            this.Controls.Add(this.UnsignedIntSequenceControl1);
            this.Controls.Add(this.LineLabel);
            this.Controls.Add(this.UintSeqLabel);
            this.Controls.Add(this.CancelBtn);
            this.Controls.Add(this.OKBtn);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "NewLineKrystalDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "new line krystal";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button OKBtn;
        private System.Windows.Forms.Button CancelBtn;
        private System.Windows.Forms.Label UintSeqLabel;
        private System.Windows.Forms.Label LineLabel;
        private Krystals5ControlLibrary.UnsignedIntSeqControl UnsignedIntSequenceControl1;
    }
}