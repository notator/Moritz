namespace Krystals4Application
{
    partial class NewPathExpansionDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NewPathExpansionDialog));
            this.OKBtn = new System.Windows.Forms.Button();
            this.CancelBtn = new System.Windows.Forms.Button();
            this.SVGInputFilenameLabel = new System.Windows.Forms.Label();
            this.DensityInputFilenameLabel = new System.Windows.Forms.Label();
            this.SVGInputLabel = new System.Windows.Forms.Label();
            this.DensityInputLabel = new System.Windows.Forms.Label();
            this.SetSVGInputButton = new System.Windows.Forms.Button();
            this.SetDensityInputButton = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // OKBtn
            // 
            this.OKBtn.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.OKBtn.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.OKBtn.Location = new System.Drawing.Point(79, 237);
            this.OKBtn.Name = "OKBtn";
            this.OKBtn.Size = new System.Drawing.Size(75, 23);
            this.OKBtn.TabIndex = 6;
            this.OKBtn.Text = "OK";
            this.OKBtn.UseVisualStyleBackColor = true;
            this.OKBtn.Click += new System.EventHandler(this.OKBtn_Click);
            // 
            // CancelBtn
            // 
            this.CancelBtn.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.CancelBtn.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.CancelBtn.Location = new System.Drawing.Point(172, 237);
            this.CancelBtn.Name = "CancelBtn";
            this.CancelBtn.Size = new System.Drawing.Size(75, 23);
            this.CancelBtn.TabIndex = 7;
            this.CancelBtn.Text = "Cancel";
            this.CancelBtn.UseVisualStyleBackColor = true;
            this.CancelBtn.Click += new System.EventHandler(this.CancelBtn_Click);
            // 
            // SVGInputFilenameLabel
            // 
            this.SVGInputFilenameLabel.AutoSize = true;
            this.SVGInputFilenameLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.SVGInputFilenameLabel.Location = new System.Drawing.Point(102, 23);
            this.SVGInputFilenameLabel.Name = "SVGInputFilenameLabel";
            this.SVGInputFilenameLabel.Size = new System.Drawing.Size(77, 13);
            this.SVGInputFilenameLabel.TabIndex = 22;
            this.SVGInputFilenameLabel.Text = "<unassigned>*";
            // 
            // DensityInputFilenameLabel
            // 
            this.DensityInputFilenameLabel.AutoSize = true;
            this.DensityInputFilenameLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.DensityInputFilenameLabel.Location = new System.Drawing.Point(102, 127);
            this.DensityInputFilenameLabel.Name = "DensityInputFilenameLabel";
            this.DensityInputFilenameLabel.Size = new System.Drawing.Size(77, 13);
            this.DensityInputFilenameLabel.TabIndex = 21;
            this.DensityInputFilenameLabel.Text = "<unassigned>*";
            // 
            // SVGInputLabel
            // 
            this.SVGInputLabel.AutoSize = true;
            this.SVGInputLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.SVGInputLabel.Location = new System.Drawing.Point(21, 23);
            this.SVGInputLabel.Name = "SVGInputLabel";
            this.SVGInputLabel.Size = new System.Drawing.Size(58, 13);
            this.SVGInputLabel.TabIndex = 20;
            this.SVGInputLabel.Text = "SVG input:";
            // 
            // DensityInputLabel
            // 
            this.DensityInputLabel.AutoSize = true;
            this.DensityInputLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.DensityInputLabel.Location = new System.Drawing.Point(21, 127);
            this.DensityInputLabel.Name = "DensityInputLabel";
            this.DensityInputLabel.Size = new System.Drawing.Size(71, 13);
            this.DensityInputLabel.TabIndex = 19;
            this.DensityInputLabel.Text = "Density input:";
            // 
            // SetSVGInputButton
            // 
            this.SetSVGInputButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.SetSVGInputButton.Location = new System.Drawing.Point(245, 18);
            this.SetSVGInputButton.Name = "SetSVGInputButton";
            this.SetSVGInputButton.Size = new System.Drawing.Size(60, 23);
            this.SetSVGInputButton.TabIndex = 18;
            this.SetSVGInputButton.Text = "Set...";
            this.SetSVGInputButton.UseVisualStyleBackColor = true;
            this.SetSVGInputButton.Click += new System.EventHandler(this.SetSVGInputButton_Click);
            // 
            // SetDensityInputButton
            // 
            this.SetDensityInputButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.SetDensityInputButton.Location = new System.Drawing.Point(245, 122);
            this.SetDensityInputButton.Name = "SetDensityInputButton";
            this.SetDensityInputButton.Size = new System.Drawing.Size(60, 23);
            this.SetDensityInputButton.TabIndex = 17;
            this.SetDensityInputButton.Text = "Set...";
            this.SetDensityInputButton.UseVisualStyleBackColor = true;
            this.SetDensityInputButton.Click += new System.EventHandler(this.SetDensityInputButton_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.ForeColor = System.Drawing.Color.CornflowerBlue;
            this.label1.Location = new System.Drawing.Point(16, 78);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(308, 39);
            this.label1.TabIndex = 23;
            this.label1.Text = "The density input shape must have a level that contains\r\n the number of nodes in " +
    "the trajectory path.\r\nThe density input must therefore have a level greater than" +
    " zero. ";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.ForeColor = System.Drawing.Color.CornflowerBlue;
            this.label2.Location = new System.Drawing.Point(16, 46);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(289, 26);
            this.label2.TabIndex = 24;
            this.label2.Text = "The SVG input filename has the following structure:\r\n    path.<domain>.<number of" +
    " trajectory nodes>.<index>.svg\r\n";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.ForeColor = System.Drawing.Color.CornflowerBlue;
            this.label3.Location = new System.Drawing.Point(16, 157);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(278, 65);
            this.label3.TabIndex = 25;
            this.label3.Text = resources.GetString("label3.Text");
            // 
            // NewPathExpansionDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(328, 272);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.SVGInputFilenameLabel);
            this.Controls.Add(this.DensityInputFilenameLabel);
            this.Controls.Add(this.SVGInputLabel);
            this.Controls.Add(this.DensityInputLabel);
            this.Controls.Add(this.SetSVGInputButton);
            this.Controls.Add(this.SetDensityInputButton);
            this.Controls.Add(this.CancelBtn);
            this.Controls.Add(this.OKBtn);
            this.Name = "NewPathExpansionDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "new path expansion";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button OKBtn;
        private System.Windows.Forms.Button CancelBtn;
        private System.Windows.Forms.Label SVGInputFilenameLabel;
        private System.Windows.Forms.Label DensityInputFilenameLabel;
        private System.Windows.Forms.Label SVGInputLabel;
        private System.Windows.Forms.Label DensityInputLabel;
        private System.Windows.Forms.Button SetSVGInputButton;
        private System.Windows.Forms.Button SetDensityInputButton;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
    }
}