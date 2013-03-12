namespace Krystals4Application
{
    partial class NewConstantKrystalDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NewConstantKrystalDialog));
            this.OKBtn = new System.Windows.Forms.Button();
            this.CancelBtn = new System.Windows.Forms.Button();
            this.ConstantValueLabel = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.UnsignedIntControl1 = new Krystals4ControlLibrary.UnsignedIntControl();
            this.SuspendLayout();
            // 
            // OKBtn
            // 
            this.OKBtn.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.OKBtn.Location = new System.Drawing.Point(23, 103);
            this.OKBtn.Name = "OKBtn";
            this.OKBtn.Size = new System.Drawing.Size(75, 23);
            this.OKBtn.TabIndex = 3;
            this.OKBtn.Text = "OK";
            this.OKBtn.Click += new System.EventHandler(this.OKBtn_Click);
            // 
            // CancelBtn
            // 
            this.CancelBtn.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.CancelBtn.Location = new System.Drawing.Point(117, 103);
            this.CancelBtn.Name = "CancelBtn";
            this.CancelBtn.Size = new System.Drawing.Size(75, 23);
            this.CancelBtn.TabIndex = 4;
            this.CancelBtn.Text = "Cancel";
            this.CancelBtn.Click += new System.EventHandler(this.CancelBtn_Click);
            // 
            // ConstantValueLabel
            // 
            this.ConstantValueLabel.AutoSize = true;
            this.ConstantValueLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.ConstantValueLabel.Location = new System.Drawing.Point(35, 33);
            this.ConstantValueLabel.Name = "ConstantValueLabel";
            this.ConstantValueLabel.Size = new System.Drawing.Size(80, 13);
            this.ConstantValueLabel.TabIndex = 18;
            this.ConstantValueLabel.Text = "constant value:";
            this.ConstantValueLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.BackColor = System.Drawing.SystemColors.ControlLight;
            this.label1.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.label1.ForeColor = System.Drawing.Color.LightSlateGray;
            this.label1.Location = new System.Drawing.Point(48, 58);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(115, 26);
            this.label1.TabIndex = 21;
            this.label1.Text = "constant krystal values\r\nare unsigned integers.";
            this.label1.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // UnsignedIntControl1
            // 
            this.UnsignedIntControl1.Location = new System.Drawing.Point(121, 29);
            this.UnsignedIntControl1.Name = "UnsignedIntControl1";
            this.UnsignedIntControl1.Size = new System.Drawing.Size(44, 20);
            this.UnsignedIntControl1.TabIndex = 2;
            this.UnsignedIntControl1.UnsignedInteger = ((System.Text.StringBuilder)(resources.GetObject("UnsignedIntControl1.UnsignedInteger")));
            // 
            // NewConstantKrystalDialog
            // 
            this.AcceptButton = this.OKBtn;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ControlLight;
            this.CancelButton = this.CancelBtn;
            this.ClientSize = new System.Drawing.Size(216, 149);
            this.Controls.Add(this.UnsignedIntControl1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.ConstantValueLabel);
            this.Controls.Add(this.CancelBtn);
            this.Controls.Add(this.OKBtn);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "NewConstantKrystalDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "new constant krystal";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button OKBtn;
        private System.Windows.Forms.Button CancelBtn;
        private System.Windows.Forms.Label ConstantValueLabel;
        private System.Windows.Forms.Label label1;
        private Krystals4ControlLibrary.UnsignedIntControl UnsignedIntControl1;
    }
}