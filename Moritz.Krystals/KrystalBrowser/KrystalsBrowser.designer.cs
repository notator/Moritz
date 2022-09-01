namespace Moritz.Krystals
{
	partial class KrystalsBrowser
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
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.splitContainer3 = new System.Windows.Forms.SplitContainer();
            this.StrandsTreeView = new System.Windows.Forms.TreeView();
            this.Krystal2DTextBox = new System.Windows.Forms.TextBox();
            this.BasicData = new System.Windows.Forms.Label();
            this.Shape = new System.Windows.Forms.Label();
            this.CloseButton = new System.Windows.Forms.Button();
            this.MissingValues = new System.Windows.Forms.Label();
            this.ReturnKrystalButton = new System.Windows.Forms.Button();
            this.RenameKrystalButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).BeginInit();
            this.splitContainer3.Panel1.SuspendLayout();
            this.splitContainer3.Panel2.SuspendLayout();
            this.splitContainer3.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer1.BackColor = System.Drawing.SystemColors.Window;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.splitContainer2);
            this.splitContainer1.Size = new System.Drawing.Size(1368, 388);
            this.splitContainer1.SplitterDistance = 184;
            this.splitContainer1.TabIndex = 0;
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Name = "splitContainer2";
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.splitContainer3);
            this.splitContainer2.Size = new System.Drawing.Size(1180, 388);
            this.splitContainer2.SplitterDistance = 202;
            this.splitContainer2.TabIndex = 0;
            // 
            // splitContainer3
            // 
            this.splitContainer3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer3.Location = new System.Drawing.Point(0, 0);
            this.splitContainer3.Name = "splitContainer3";
            // 
            // splitContainer3.Panel1
            // 
            this.splitContainer3.Panel1.Controls.Add(this.StrandsTreeView);
            // 
            // splitContainer3.Panel2
            // 
            this.splitContainer3.Panel2.Controls.Add(this.Krystal2DTextBox);
            this.splitContainer3.Size = new System.Drawing.Size(974, 388);
            this.splitContainer3.SplitterDistance = 254;
            this.splitContainer3.TabIndex = 1;
            // 
            // StrandsTreeView
            // 
            this.StrandsTreeView.BackColor = System.Drawing.SystemColors.Window;
            this.StrandsTreeView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.StrandsTreeView.Location = new System.Drawing.Point(0, 0);
            this.StrandsTreeView.Name = "StrandsTreeView";
            this.StrandsTreeView.Size = new System.Drawing.Size(254, 388);
            this.StrandsTreeView.TabIndex = 0;
            // 
            // Krystal2DTextBox
            // 
            this.Krystal2DTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.Krystal2DTextBox.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Krystal2DTextBox.Location = new System.Drawing.Point(0, 0);
            this.Krystal2DTextBox.Multiline = true;
            this.Krystal2DTextBox.Name = "Krystal2DTextBox";
            this.Krystal2DTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.Krystal2DTextBox.Size = new System.Drawing.Size(714, 388);
            this.Krystal2DTextBox.TabIndex = 0;
            // 
            // BasicData
            // 
            this.BasicData.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.BasicData.AutoSize = true;
            this.BasicData.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(248)))), ((int)(((byte)(248)))));
            this.BasicData.Location = new System.Drawing.Point(12, 401);
            this.BasicData.Name = "BasicData";
            this.BasicData.Size = new System.Drawing.Size(55, 13);
            this.BasicData.TabIndex = 24;
            this.BasicData.Text = "basicData";
            // 
            // Shape
            // 
            this.Shape.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.Shape.AutoSize = true;
            this.Shape.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(248)))), ((int)(((byte)(248)))));
            this.Shape.Location = new System.Drawing.Point(12, 443);
            this.Shape.Name = "Shape";
            this.Shape.Size = new System.Drawing.Size(36, 13);
            this.Shape.TabIndex = 29;
            this.Shape.Text = "shape";
            // 
            // CloseButton
            // 
            this.CloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.CloseButton.Location = new System.Drawing.Point(1261, 436);
            this.CloseButton.Name = "CloseButton";
            this.CloseButton.Size = new System.Drawing.Size(94, 23);
            this.CloseButton.TabIndex = 23;
            this.CloseButton.Text = "Close";
            this.CloseButton.UseVisualStyleBackColor = true;
            this.CloseButton.Click += new System.EventHandler(this.OKButton_Click);
            // 
            // MissingValues
            // 
            this.MissingValues.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.MissingValues.AutoSize = true;
            this.MissingValues.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(248)))), ((int)(((byte)(248)))));
            this.MissingValues.Location = new System.Drawing.Point(12, 422);
            this.MissingValues.Name = "MissingValues";
            this.MissingValues.Size = new System.Drawing.Size(75, 13);
            this.MissingValues.TabIndex = 31;
            this.MissingValues.Text = "missing values";
            // 
            // ReturnKrystalButton
            // 
            this.ReturnKrystalButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.ReturnKrystalButton.Location = new System.Drawing.Point(1261, 436);
            this.ReturnKrystalButton.Name = "ReturnKrystalButton";
            this.ReturnKrystalButton.Size = new System.Drawing.Size(94, 23);
            this.ReturnKrystalButton.TabIndex = 35;
            this.ReturnKrystalButton.Text = "Return krystal";
            this.ReturnKrystalButton.UseVisualStyleBackColor = true;
            this.ReturnKrystalButton.Click += new System.EventHandler(this.ReturnKrystalButton_Click);
            // 
            // RenameKrystalButton
            // 
            this.RenameKrystalButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.RenameKrystalButton.Location = new System.Drawing.Point(1126, 436);
            this.RenameKrystalButton.Name = "RenameKrystalButton";
            this.RenameKrystalButton.Size = new System.Drawing.Size(125, 23);
            this.RenameKrystalButton.TabIndex = 36;
            this.RenameKrystalButton.Text = "Rename Krystal...";
            this.RenameKrystalButton.UseVisualStyleBackColor = true;
            this.RenameKrystalButton.Visible = false;
            this.RenameKrystalButton.Click += new System.EventHandler(this.RenameKrystalButton_Click);
            // 
            // KrystalsBrowser
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(248)))), ((int)(((byte)(248)))));
            this.ClientSize = new System.Drawing.Size(1366, 470);
            this.Controls.Add(this.RenameKrystalButton);
            this.Controls.Add(this.MissingValues);
            this.Controls.Add(this.CloseButton);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.BasicData);
            this.Controls.Add(this.Shape);
            this.Controls.Add(this.ReturnKrystalButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Name = "KrystalsBrowser";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Krystals";
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.splitContainer3.Panel1.ResumeLayout(false);
            this.splitContainer3.Panel2.ResumeLayout(false);
            this.splitContainer3.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).EndInit();
            this.splitContainer3.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.SplitContainer splitContainer1;
		private System.Windows.Forms.Label BasicData;
		private System.Windows.Forms.Label Shape;
		private System.Windows.Forms.Button CloseButton;
		private System.Windows.Forms.Label MissingValues;
		private System.Windows.Forms.TreeView StrandsTreeView;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.Button ReturnKrystalButton;
        private System.Windows.Forms.Button RenameKrystalButton;
        private System.Windows.Forms.SplitContainer splitContainer3;
        private System.Windows.Forms.TextBox Krystal2DTextBox;
    }
}