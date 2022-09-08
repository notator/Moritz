namespace Krystals5Application
{
    partial class NewModulationDialog
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
            this.CompulsoryTextlabel = new System.Windows.Forms.Label();
            this.YInputFilenameLabel = new System.Windows.Forms.Label();
            this.XInputFilenameLabel = new System.Windows.Forms.Label();
            this.ModulatorFilenameLabel = new System.Windows.Forms.Label();
            this.InputValuesLabel = new System.Windows.Forms.Label();
            this.DensityInputLabel = new System.Windows.Forms.Label();
            this.ModulatorLabel = new System.Windows.Forms.Label();
            this.SetInputValuesButton = new System.Windows.Forms.Button();
            this.SetDensityInputButton = new System.Windows.Forms.Button();
            this.CancelBtn = new System.Windows.Forms.Button();
            this.OKBtn = new System.Windows.Forms.Button();
            this.SetInputFieldButton = new System.Windows.Forms.Button();
            this.LoadKrystalButton = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // CompulsoryTextlabel
            // 
            this.CompulsoryTextlabel.AutoSize = true;
            this.CompulsoryTextlabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.CompulsoryTextlabel.ForeColor = System.Drawing.Color.SlateGray;
            this.CompulsoryTextlabel.Location = new System.Drawing.Point(29, 157);
            this.CompulsoryTextlabel.Name = "CompulsoryTextlabel";
            this.CompulsoryTextlabel.Size = new System.Drawing.Size(166, 13);
            this.CompulsoryTextlabel.TabIndex = 9;
            this.CompulsoryTextlabel.Text = "* the above two fields must be set";
            // 
            // YInputFilenameLabel
            // 
            this.YInputFilenameLabel.AutoSize = true;
            this.YInputFilenameLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.YInputFilenameLabel.Location = new System.Drawing.Point(82, 128);
            this.YInputFilenameLabel.Name = "YInputFilenameLabel";
            this.YInputFilenameLabel.Size = new System.Drawing.Size(77, 13);
            this.YInputFilenameLabel.TabIndex = 8;
            this.YInputFilenameLabel.Text = "<unassigned>*";
            // 
            // XInputFilenameLabel
            // 
            this.XInputFilenameLabel.AutoSize = true;
            this.XInputFilenameLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.XInputFilenameLabel.Location = new System.Drawing.Point(82, 97);
            this.XInputFilenameLabel.Name = "XInputFilenameLabel";
            this.XInputFilenameLabel.Size = new System.Drawing.Size(77, 13);
            this.XInputFilenameLabel.TabIndex = 6;
            this.XInputFilenameLabel.Text = "<unassigned>*";
            // 
            // ModulatorFilenameLabel
            // 
            this.ModulatorFilenameLabel.AutoSize = true;
            this.ModulatorFilenameLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.ModulatorFilenameLabel.Location = new System.Drawing.Point(86, 199);
            this.ModulatorFilenameLabel.Name = "ModulatorFilenameLabel";
            this.ModulatorFilenameLabel.Size = new System.Drawing.Size(104, 13);
            this.ModulatorFilenameLabel.TabIndex = 11;
            this.ModulatorFilenameLabel.Text = "to be edited (default)";
            // 
            // InputValuesLabel
            // 
            this.InputValuesLabel.AutoSize = true;
            this.InputValuesLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.InputValuesLabel.Location = new System.Drawing.Point(29, 128);
            this.InputValuesLabel.Name = "InputValuesLabel";
            this.InputValuesLabel.Size = new System.Drawing.Size(43, 13);
            this.InputValuesLabel.TabIndex = 7;
            this.InputValuesLabel.Text = "Y input:";
            // 
            // DensityInputLabel
            // 
            this.DensityInputLabel.AutoSize = true;
            this.DensityInputLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.DensityInputLabel.Location = new System.Drawing.Point(29, 97);
            this.DensityInputLabel.Name = "DensityInputLabel";
            this.DensityInputLabel.Size = new System.Drawing.Size(43, 13);
            this.DensityInputLabel.TabIndex = 5;
            this.DensityInputLabel.Text = "X input:";
            // 
            // ModulatorLabel
            // 
            this.ModulatorLabel.AutoSize = true;
            this.ModulatorLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.ModulatorLabel.Location = new System.Drawing.Point(29, 199);
            this.ModulatorLabel.Name = "ModulatorLabel";
            this.ModulatorLabel.Size = new System.Drawing.Size(57, 13);
            this.ModulatorLabel.TabIndex = 10;
            this.ModulatorLabel.Text = "Modulator:";
            // 
            // SetInputValuesButton
            // 
            this.SetInputValuesButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.SetInputValuesButton.Location = new System.Drawing.Point(217, 123);
            this.SetInputValuesButton.Name = "SetInputValuesButton";
            this.SetInputValuesButton.Size = new System.Drawing.Size(60, 23);
            this.SetInputValuesButton.TabIndex = 1;
            this.SetInputValuesButton.Text = "Set...";
            this.SetInputValuesButton.UseVisualStyleBackColor = true;
            this.SetInputValuesButton.Click += new System.EventHandler(this.SetYInputButton_Click);
            // 
            // SetDensityInputButton
            // 
            this.SetDensityInputButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.SetDensityInputButton.Location = new System.Drawing.Point(217, 92);
            this.SetDensityInputButton.Name = "SetDensityInputButton";
            this.SetDensityInputButton.Size = new System.Drawing.Size(60, 23);
            this.SetDensityInputButton.TabIndex = 0;
            this.SetDensityInputButton.Text = "Set...";
            this.SetDensityInputButton.UseVisualStyleBackColor = true;
            this.SetDensityInputButton.Click += new System.EventHandler(this.SetXInputButton_Click);
            // 
            // CancelBtn
            // 
            this.CancelBtn.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.CancelBtn.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.CancelBtn.Location = new System.Drawing.Point(202, 244);
            this.CancelBtn.Name = "CancelBtn";
            this.CancelBtn.Size = new System.Drawing.Size(75, 23);
            this.CancelBtn.TabIndex = 4;
            this.CancelBtn.Text = "Cancel";
            this.CancelBtn.UseVisualStyleBackColor = true;
            this.CancelBtn.Click += new System.EventHandler(this.CancelBtn_Click);
            // 
            // OKBtn
            // 
            this.OKBtn.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.OKBtn.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.OKBtn.Location = new System.Drawing.Point(102, 244);
            this.OKBtn.Name = "OKBtn";
            this.OKBtn.Size = new System.Drawing.Size(75, 23);
            this.OKBtn.TabIndex = 3;
            this.OKBtn.Text = "OK";
            this.OKBtn.UseVisualStyleBackColor = true;
            this.OKBtn.Click += new System.EventHandler(this.OKBtn_Click);
            // 
            // SetInputFieldButton
            // 
            this.SetInputFieldButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.SetInputFieldButton.Location = new System.Drawing.Point(217, 194);
            this.SetInputFieldButton.Name = "SetInputFieldButton";
            this.SetInputFieldButton.Size = new System.Drawing.Size(60, 23);
            this.SetInputFieldButton.TabIndex = 2;
            this.SetInputFieldButton.Text = "Set...";
            this.SetInputFieldButton.UseVisualStyleBackColor = true;
            this.SetInputFieldButton.Click += new System.EventHandler(this.SetModulatorButton_Click);
            // 
            // LoadKrystalButton
            // 
            this.LoadKrystalButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.LoadKrystalButton.Location = new System.Drawing.Point(28, 21);
            this.LoadKrystalButton.Name = "LoadKrystalButton";
            this.LoadKrystalButton.Size = new System.Drawing.Size(248, 23);
            this.LoadKrystalButton.TabIndex = 12;
            this.LoadKrystalButton.Text = "Load Existing Modulation Krystal...";
            this.LoadKrystalButton.UseVisualStyleBackColor = true;
            this.LoadKrystalButton.Click += new System.EventHandler(this.OpenKrystalButton_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.label1.ForeColor = System.Drawing.Color.SlateGray;
            this.label1.Location = new System.Drawing.Point(28, 49);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(268, 26);
            this.label1.TabIndex = 13;
            this.label1.Text = "Use the above button to set the values of the three\r\nfields below, and/or use the" +
                " \'Set...\' buttons individually. ";
            // 
            // NewModulationDialog
            // 
            this.AcceptButton = this.OKBtn;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ControlLight;
            this.CancelButton = this.CancelBtn;
            this.ClientSize = new System.Drawing.Size(307, 292);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.LoadKrystalButton);
            this.Controls.Add(this.CompulsoryTextlabel);
            this.Controls.Add(this.YInputFilenameLabel);
            this.Controls.Add(this.XInputFilenameLabel);
            this.Controls.Add(this.ModulatorFilenameLabel);
            this.Controls.Add(this.InputValuesLabel);
            this.Controls.Add(this.DensityInputLabel);
            this.Controls.Add(this.ModulatorLabel);
            this.Controls.Add(this.SetInputValuesButton);
            this.Controls.Add(this.SetDensityInputButton);
            this.Controls.Add(this.CancelBtn);
            this.Controls.Add(this.OKBtn);
            this.Controls.Add(this.SetInputFieldButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "NewModulationDialog";
            this.ShowIcon = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "new modulation";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label CompulsoryTextlabel;
        private System.Windows.Forms.Label YInputFilenameLabel;
        private System.Windows.Forms.Label XInputFilenameLabel;
        private System.Windows.Forms.Label ModulatorFilenameLabel;
        private System.Windows.Forms.Label InputValuesLabel;
        private System.Windows.Forms.Label DensityInputLabel;
        private System.Windows.Forms.Label ModulatorLabel;
        private System.Windows.Forms.Button SetInputValuesButton;
        private System.Windows.Forms.Button SetDensityInputButton;
        private System.Windows.Forms.Button CancelBtn;
        private System.Windows.Forms.Button OKBtn;
        private System.Windows.Forms.Button SetInputFieldButton;
        private System.Windows.Forms.Button LoadKrystalButton;
        private System.Windows.Forms.Label label1;
    }
}