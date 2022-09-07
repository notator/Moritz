namespace Krystals4Application
{
    partial class NewExpansionDialog
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
            this.SetExpanderButton = new System.Windows.Forms.Button();
            this.ExpandBtn = new System.Windows.Forms.Button();
            this.CloseBtn = new System.Windows.Forms.Button();
            this.SetDensityInputButton = new System.Windows.Forms.Button();
            this.SetPointsInputValuesButton = new System.Windows.Forms.Button();
            this.ExpanderLabel = new System.Windows.Forms.Label();
            this.DensityInputLabel = new System.Windows.Forms.Label();
            this.InputValuesLabel = new System.Windows.Forms.Label();
            this.ExpanderFilenameLabel = new System.Windows.Forms.Label();
            this.DensityInputFilenameLabel = new System.Windows.Forms.Label();
            this.PointsInputFilenameLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // SetExpanderButton
            // 
            this.SetExpanderButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.SetExpanderButton.Location = new System.Drawing.Point(231, 62);
            this.SetExpanderButton.Name = "SetExpanderButton";
            this.SetExpanderButton.Size = new System.Drawing.Size(60, 23);
            this.SetExpanderButton.TabIndex = 3;
            this.SetExpanderButton.Text = "Set...";
            this.SetExpanderButton.UseVisualStyleBackColor = true;
            this.SetExpanderButton.Click += new System.EventHandler(this.SetExpanderButton_Click);
            // 
            // ExpandBtn
            // 
            this.ExpandBtn.Enabled = false;
            this.ExpandBtn.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.ExpandBtn.Location = new System.Drawing.Point(116, 149);
            this.ExpandBtn.Name = "ExpandBtn";
            this.ExpandBtn.Size = new System.Drawing.Size(75, 23);
            this.ExpandBtn.TabIndex = 5;
            this.ExpandBtn.Text = "Expand";
            this.ExpandBtn.UseVisualStyleBackColor = true;
            this.ExpandBtn.Click += new System.EventHandler(this.ExpandBtn_Click);
            // 
            // CloseBtn
            // 
            this.CloseBtn.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.CloseBtn.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.CloseBtn.Location = new System.Drawing.Point(216, 149);
            this.CloseBtn.Name = "CloseBtn";
            this.CloseBtn.Size = new System.Drawing.Size(75, 23);
            this.CloseBtn.TabIndex = 6;
            this.CloseBtn.Text = "Close";
            this.CloseBtn.UseVisualStyleBackColor = true;
            this.CloseBtn.Click += new System.EventHandler(this.CloseBtn_Click);
            // 
            // SetDensityInputButton
            // 
            this.SetDensityInputButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.SetDensityInputButton.Location = new System.Drawing.Point(231, 27);
            this.SetDensityInputButton.Name = "SetDensityInputButton";
            this.SetDensityInputButton.Size = new System.Drawing.Size(60, 23);
            this.SetDensityInputButton.TabIndex = 1;
            this.SetDensityInputButton.Text = "Set...";
            this.SetDensityInputButton.UseVisualStyleBackColor = true;
            this.SetDensityInputButton.Click += new System.EventHandler(this.SetDensityInputButton_Click);
            // 
            // SetPointsInputValuesButton
            // 
            this.SetPointsInputValuesButton.Enabled = false;
            this.SetPointsInputValuesButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.SetPointsInputValuesButton.Location = new System.Drawing.Point(231, 97);
            this.SetPointsInputValuesButton.Name = "SetPointsInputValuesButton";
            this.SetPointsInputValuesButton.Size = new System.Drawing.Size(60, 23);
            this.SetPointsInputValuesButton.TabIndex = 2;
            this.SetPointsInputValuesButton.Text = "Set...";
            this.SetPointsInputValuesButton.UseVisualStyleBackColor = true;
            this.SetPointsInputValuesButton.Click += new System.EventHandler(this.SetPointsInputButton_Click);
            // 
            // ExpanderLabel
            // 
            this.ExpanderLabel.AutoSize = true;
            this.ExpanderLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.ExpanderLabel.Location = new System.Drawing.Point(26, 67);
            this.ExpanderLabel.Name = "ExpanderLabel";
            this.ExpanderLabel.Size = new System.Drawing.Size(55, 13);
            this.ExpanderLabel.TabIndex = 9;
            this.ExpanderLabel.Text = "Expander:";
            this.ExpanderLabel.Click += new System.EventHandler(this.SetExpanderButton_Click);
            // 
            // DensityInputLabel
            // 
            this.DensityInputLabel.AutoSize = true;
            this.DensityInputLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.DensityInputLabel.Location = new System.Drawing.Point(26, 32);
            this.DensityInputLabel.Name = "DensityInputLabel";
            this.DensityInputLabel.Size = new System.Drawing.Size(71, 13);
            this.DensityInputLabel.TabIndex = 11;
            this.DensityInputLabel.Text = "Density input:";
            this.DensityInputLabel.Click += new System.EventHandler(this.SetDensityInputButton_Click);
            // 
            // InputValuesLabel
            // 
            this.InputValuesLabel.AutoSize = true;
            this.InputValuesLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.InputValuesLabel.Location = new System.Drawing.Point(26, 102);
            this.InputValuesLabel.Name = "InputValuesLabel";
            this.InputValuesLabel.Size = new System.Drawing.Size(65, 13);
            this.InputValuesLabel.TabIndex = 12;
            this.InputValuesLabel.Text = "Points input:";
            this.InputValuesLabel.Click += new System.EventHandler(this.SetPointsInputButton_Click);
            // 
            // ExpanderFilenameLabel
            // 
            this.ExpanderFilenameLabel.AutoSize = true;
            this.ExpanderFilenameLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.ExpanderFilenameLabel.Location = new System.Drawing.Point(96, 67);
            this.ExpanderFilenameLabel.Name = "ExpanderFilenameLabel";
            this.ExpanderFilenameLabel.Size = new System.Drawing.Size(68, 13);
            this.ExpanderFilenameLabel.TabIndex = 13;
            this.ExpanderFilenameLabel.Text = "<unasigned>";
            this.ExpanderFilenameLabel.Click += new System.EventHandler(this.SetExpanderButton_Click);
            // 
            // DensityInputFilenameLabel
            // 
            this.DensityInputFilenameLabel.AutoSize = true;
            this.DensityInputFilenameLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.DensityInputFilenameLabel.Location = new System.Drawing.Point(96, 32);
            this.DensityInputFilenameLabel.Name = "DensityInputFilenameLabel";
            this.DensityInputFilenameLabel.Size = new System.Drawing.Size(73, 13);
            this.DensityInputFilenameLabel.TabIndex = 15;
            this.DensityInputFilenameLabel.Text = "<unassigned>";
            this.DensityInputFilenameLabel.Click += new System.EventHandler(this.SetDensityInputButton_Click);
            // 
            // PointsInputFilenameLabel
            // 
            this.PointsInputFilenameLabel.AutoSize = true;
            this.PointsInputFilenameLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.PointsInputFilenameLabel.Location = new System.Drawing.Point(96, 102);
            this.PointsInputFilenameLabel.Name = "PointsInputFilenameLabel";
            this.PointsInputFilenameLabel.Size = new System.Drawing.Size(73, 13);
            this.PointsInputFilenameLabel.TabIndex = 16;
            this.PointsInputFilenameLabel.Text = "<unassigned>";
            this.PointsInputFilenameLabel.Click += new System.EventHandler(this.SetPointsInputButton_Click);
            // 
            // NewExpansionDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ControlLight;
            this.ClientSize = new System.Drawing.Size(320, 196);
            this.Controls.Add(this.PointsInputFilenameLabel);
            this.Controls.Add(this.DensityInputFilenameLabel);
            this.Controls.Add(this.ExpanderFilenameLabel);
            this.Controls.Add(this.InputValuesLabel);
            this.Controls.Add(this.DensityInputLabel);
            this.Controls.Add(this.ExpanderLabel);
            this.Controls.Add(this.SetPointsInputValuesButton);
            this.Controls.Add(this.SetDensityInputButton);
            this.Controls.Add(this.CloseBtn);
            this.Controls.Add(this.ExpandBtn);
            this.Controls.Add(this.SetExpanderButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "NewExpansionDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "new expansion";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button CloseBtn;
        private System.Windows.Forms.Button SetDensityInputButton;
        private System.Windows.Forms.Label ExpanderLabel;
        private System.Windows.Forms.Label DensityInputLabel;
        private System.Windows.Forms.Label InputValuesLabel;
        private System.Windows.Forms.Label ExpanderFilenameLabel;
        private System.Windows.Forms.Label DensityInputFilenameLabel;
        private System.Windows.Forms.Label PointsInputFilenameLabel;
        internal System.Windows.Forms.Button SetExpanderButton;
        internal System.Windows.Forms.Button ExpandBtn;
        internal System.Windows.Forms.Button SetPointsInputValuesButton;
    }
}