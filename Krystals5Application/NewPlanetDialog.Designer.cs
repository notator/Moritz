namespace Krystals5Application
{
	partial class NewPlanetDialog
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

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NewPlanetDialog));
            this.OKBtn = new System.Windows.Forms.Button();
            this.CancelBtn = new System.Windows.Forms.Button();
            this.NumberOfSubpathsLabel = new System.Windows.Forms.Label();
            this.ValueLabel = new System.Windows.Forms.Label();
            this.commentText = new System.Windows.Forms.Label();
            this.StartMomentIntSeqControl = new Krystals5ControlLibrary.UnsignedIntSeqControl();
            this.ValueUIntControl = new Krystals5ControlLibrary.UnsignedIntControl();
            this.SuspendLayout();
            // 
            // OKBtn
            // 
            this.OKBtn.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.OKBtn.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.OKBtn.Location = new System.Drawing.Point(145, 147);
            this.OKBtn.Name = "OKBtn";
            this.OKBtn.Size = new System.Drawing.Size(75, 23);
            this.OKBtn.TabIndex = 2;
            this.OKBtn.Text = "OK";
            // 
            // CancelBtn
            // 
            this.CancelBtn.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.CancelBtn.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.CancelBtn.Location = new System.Drawing.Point(240, 147);
            this.CancelBtn.Name = "CancelBtn";
            this.CancelBtn.Size = new System.Drawing.Size(75, 23);
            this.CancelBtn.TabIndex = 3;
            this.CancelBtn.Text = "Cancel";
            // 
            // NumberOfSubpathsLabel
            // 
            this.NumberOfSubpathsLabel.AutoSize = true;
            this.NumberOfSubpathsLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.NumberOfSubpathsLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.NumberOfSubpathsLabel.Location = new System.Drawing.Point(57, 57);
            this.NumberOfSubpathsLabel.Name = "NumberOfSubpathsLabel";
            this.NumberOfSubpathsLabel.Size = new System.Drawing.Size(153, 13);
            this.NumberOfSubpathsLabel.TabIndex = 5;
            this.NumberOfSubpathsLabel.Text = "start moment for each subpath:";
            // 
            // ValueLabel
            // 
            this.ValueLabel.AutoSize = true;
            this.ValueLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.ValueLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ValueLabel.Location = new System.Drawing.Point(142, 27);
            this.ValueLabel.Name = "ValueLabel";
            this.ValueLabel.Size = new System.Drawing.Size(68, 13);
            this.ValueLabel.TabIndex = 4;
            this.ValueLabel.Text = "planet value:";
            // 
            // commentText
            // 
            this.commentText.AutoSize = true;
            this.commentText.BackColor = System.Drawing.SystemColors.ControlLight;
            this.commentText.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.commentText.ForeColor = System.Drawing.Color.LightSlateGray;
            this.commentText.Location = new System.Drawing.Point(28, 78);
            this.commentText.Name = "commentText";
            this.commentText.Size = new System.Drawing.Size(311, 52);
            this.commentText.TabIndex = 2;
            this.commentText.Text = resources.GetString("commentText.Text");
            // 
            // StartMomentIntSeqControl
            // 
            this.StartMomentIntSeqControl.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.StartMomentIntSeqControl.BackColor = System.Drawing.SystemColors.Window;
            this.StartMomentIntSeqControl.Location = new System.Drawing.Point(206, 53);
            this.StartMomentIntSeqControl.Name = "StartMomentIntSeqControl";
            this.StartMomentIntSeqControl.Sequence = ((System.Text.StringBuilder)(resources.GetObject("StartMomentIntSeqControl.Sequence")));
            this.StartMomentIntSeqControl.Size = new System.Drawing.Size(82, 20);
            this.StartMomentIntSeqControl.TabIndex = 1;
            // 
            // ValueUIntControl
            // 
            this.ValueUIntControl.Location = new System.Drawing.Point(206, 24);
            this.ValueUIntControl.Name = "ValueUIntControl";
            this.ValueUIntControl.Size = new System.Drawing.Size(44, 20);
            this.ValueUIntControl.TabIndex = 0;
            this.ValueUIntControl.UnsignedInteger = ((System.Text.StringBuilder)(resources.GetObject("ValueUIntControl.UnsignedInteger")));
            // 
            // NewPlanetDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ControlLight;
            this.ClientSize = new System.Drawing.Size(367, 195);
            this.Controls.Add(this.commentText);
            this.Controls.Add(this.StartMomentIntSeqControl);
            this.Controls.Add(this.ValueUIntControl);
            this.Controls.Add(this.NumberOfSubpathsLabel);
            this.Controls.Add(this.CancelBtn);
            this.Controls.Add(this.OKBtn);
            this.Controls.Add(this.ValueLabel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "NewPlanetDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "new planet";
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button OKBtn;
		private System.Windows.Forms.Button CancelBtn;
		private System.Windows.Forms.Label NumberOfSubpathsLabel;
        private System.Windows.Forms.Label ValueLabel;
        public Krystals5ControlLibrary.UnsignedIntControl ValueUIntControl;
        private System.Windows.Forms.Label commentText;
        public Krystals5ControlLibrary.UnsignedIntSeqControl StartMomentIntSeqControl;
	}
}