namespace Moritz.AssistantComposer
{
    partial class TrackInitForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TrackInitForm));
            this.SaveSettingsButton = new System.Windows.Forms.Button();
            this.ShowMainScoreFormButton = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.TrackLabel1 = new System.Windows.Forms.Label();
            this.TrackLabel2 = new System.Windows.Forms.Label();
            this.TrackLabel3 = new System.Windows.Forms.Label();
            this.TrackLabel4 = new System.Windows.Forms.Label();
            this.TrackLabel5 = new System.Windows.Forms.Label();
            this.TrackLabel16 = new System.Windows.Forms.Label();
            this.TrackLabel6 = new System.Windows.Forms.Label();
            this.TrackLabel7 = new System.Windows.Forms.Label();
            this.TrackLabel8 = new System.Windows.Forms.Label();
            this.TrackLabel9 = new System.Windows.Forms.Label();
            this.TrackLabel10 = new System.Windows.Forms.Label();
            this.TrackLabel11 = new System.Windows.Forms.Label();
            this.TrackLabel12 = new System.Windows.Forms.Label();
            this.TrackLabel13 = new System.Windows.Forms.Label();
            this.TrackLabel14 = new System.Windows.Forms.Label();
            this.TrackLabel15 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // SaveSettingsButton
            // 
            this.SaveSettingsButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.SaveSettingsButton.Enabled = false;
            this.SaveSettingsButton.Font = new System.Drawing.Font("Arial", 8F);
            this.SaveSettingsButton.Location = new System.Drawing.Point(487, 311);
            this.SaveSettingsButton.Name = "SaveSettingsButton";
            this.SaveSettingsButton.Size = new System.Drawing.Size(106, 26);
            this.SaveSettingsButton.TabIndex = 7;
            this.SaveSettingsButton.Text = "save settings";
            this.SaveSettingsButton.UseVisualStyleBackColor = true;
            this.SaveSettingsButton.Click += new System.EventHandler(this.SaveSettingsButton_Click);
            // 
            // ShowMainScoreFormButton
            // 
            this.ShowMainScoreFormButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.ShowMainScoreFormButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(215)))), ((int)(((byte)(225)))), ((int)(((byte)(215)))));
            this.ShowMainScoreFormButton.Location = new System.Drawing.Point(324, 311);
            this.ShowMainScoreFormButton.Name = "ShowMainScoreFormButton";
            this.ShowMainScoreFormButton.Size = new System.Drawing.Size(137, 26);
            this.ShowMainScoreFormButton.TabIndex = 8;
            this.ShowMainScoreFormButton.Text = "show main score form";
            this.ShowMainScoreFormButton.UseVisualStyleBackColor = false;
            this.ShowMainScoreFormButton.Click += new System.EventHandler(this.ShowMainScoreFormButton_Click);
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label2.AutoSize = true;
            this.label2.ForeColor = System.Drawing.Color.RoyalBlue;
            this.label2.Location = new System.Drawing.Point(27, 226);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(568, 56);
            this.label2.TabIndex = 10;
            this.label2.Text = resources.GetString("label2.Text");
            // 
            // label5
            // 
            this.label5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label5.AutoSize = true;
            this.label5.ForeColor = System.Drawing.Color.RoyalBlue;
            this.label5.Location = new System.Drawing.Point(56, 317);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(245, 14);
            this.label5.TabIndex = 14;
            this.label5.Text = "Controller values are integers in the range 0..127.";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.ForeColor = System.Drawing.Color.Blue;
            this.label1.Location = new System.Drawing.Point(36, 196);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(58, 14);
            this.label1.TabIndex = 15;
            this.label1.Text = "modulation";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.ForeColor = System.Drawing.Color.Blue;
            this.label3.Location = new System.Drawing.Point(27, 92);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(67, 14);
            this.label3.TabIndex = 16;
            this.label3.Text = "pwDeviation";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.ForeColor = System.Drawing.Color.Blue;
            this.label4.Location = new System.Drawing.Point(53, 66);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(41, 14);
            this.label4.TabIndex = 17;
            this.label4.Text = "volume";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.ForeColor = System.Drawing.Color.Blue;
            this.label6.Location = new System.Drawing.Point(32, 40);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(62, 14);
            this.label6.TabIndex = 18;
            this.label6.Text = "maxVolume\r\n";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.ForeColor = System.Drawing.Color.Blue;
            this.label7.Location = new System.Drawing.Point(69, 144);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(25, 14);
            this.label7.TabIndex = 19;
            this.label7.Text = "pan";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.ForeColor = System.Drawing.Color.Blue;
            this.label8.Location = new System.Drawing.Point(33, 170);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(61, 14);
            this.label8.TabIndex = 20;
            this.label8.Text = "expression";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.ForeColor = System.Drawing.Color.Blue;
            this.label9.Location = new System.Drawing.Point(34, 118);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(60, 14);
            this.label9.TabIndex = 21;
            this.label9.Text = "pitchWheel";
            // 
            // TrackLabel1
            // 
            this.TrackLabel1.AutoSize = true;
            this.TrackLabel1.ForeColor = System.Drawing.Color.Blue;
            this.TrackLabel1.Location = new System.Drawing.Point(106, 22);
            this.TrackLabel1.Name = "TrackLabel1";
            this.TrackLabel1.Size = new System.Drawing.Size(13, 14);
            this.TrackLabel1.TabIndex = 29;
            this.TrackLabel1.Text = "1";
            this.TrackLabel1.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // TrackLabel2
            // 
            this.TrackLabel2.AutoSize = true;
            this.TrackLabel2.ForeColor = System.Drawing.Color.Blue;
            this.TrackLabel2.Location = new System.Drawing.Point(137, 22);
            this.TrackLabel2.Name = "TrackLabel2";
            this.TrackLabel2.Size = new System.Drawing.Size(13, 14);
            this.TrackLabel2.TabIndex = 30;
            this.TrackLabel2.Text = "2";
            this.TrackLabel2.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // TrackLabel3
            // 
            this.TrackLabel3.AutoSize = true;
            this.TrackLabel3.ForeColor = System.Drawing.Color.Blue;
            this.TrackLabel3.Location = new System.Drawing.Point(168, 22);
            this.TrackLabel3.Name = "TrackLabel3";
            this.TrackLabel3.Size = new System.Drawing.Size(13, 14);
            this.TrackLabel3.TabIndex = 31;
            this.TrackLabel3.Text = "3";
            this.TrackLabel3.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // TrackLabel4
            // 
            this.TrackLabel4.AutoSize = true;
            this.TrackLabel4.ForeColor = System.Drawing.Color.Blue;
            this.TrackLabel4.Location = new System.Drawing.Point(199, 22);
            this.TrackLabel4.Name = "TrackLabel4";
            this.TrackLabel4.Size = new System.Drawing.Size(13, 14);
            this.TrackLabel4.TabIndex = 32;
            this.TrackLabel4.Text = "4";
            this.TrackLabel4.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // TrackLabel5
            // 
            this.TrackLabel5.AutoSize = true;
            this.TrackLabel5.ForeColor = System.Drawing.Color.Blue;
            this.TrackLabel5.Location = new System.Drawing.Point(230, 22);
            this.TrackLabel5.Name = "TrackLabel5";
            this.TrackLabel5.Size = new System.Drawing.Size(13, 14);
            this.TrackLabel5.TabIndex = 33;
            this.TrackLabel5.Text = "5";
            this.TrackLabel5.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // TrackLabel16
            // 
            this.TrackLabel16.AutoSize = true;
            this.TrackLabel16.ForeColor = System.Drawing.Color.Blue;
            this.TrackLabel16.Location = new System.Drawing.Point(566, 22);
            this.TrackLabel16.Name = "TrackLabel16";
            this.TrackLabel16.Size = new System.Drawing.Size(19, 14);
            this.TrackLabel16.TabIndex = 34;
            this.TrackLabel16.Text = "16";
            this.TrackLabel16.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // TrackLabel6
            // 
            this.TrackLabel6.AutoSize = true;
            this.TrackLabel6.ForeColor = System.Drawing.Color.Blue;
            this.TrackLabel6.Location = new System.Drawing.Point(261, 22);
            this.TrackLabel6.Name = "TrackLabel6";
            this.TrackLabel6.Size = new System.Drawing.Size(13, 14);
            this.TrackLabel6.TabIndex = 35;
            this.TrackLabel6.Text = "6";
            this.TrackLabel6.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // TrackLabel7
            // 
            this.TrackLabel7.AutoSize = true;
            this.TrackLabel7.ForeColor = System.Drawing.Color.Blue;
            this.TrackLabel7.Location = new System.Drawing.Point(292, 22);
            this.TrackLabel7.Name = "TrackLabel7";
            this.TrackLabel7.Size = new System.Drawing.Size(13, 14);
            this.TrackLabel7.TabIndex = 36;
            this.TrackLabel7.Text = "7";
            this.TrackLabel7.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // TrackLabel8
            // 
            this.TrackLabel8.AutoSize = true;
            this.TrackLabel8.ForeColor = System.Drawing.Color.Blue;
            this.TrackLabel8.Location = new System.Drawing.Point(323, 22);
            this.TrackLabel8.Name = "TrackLabel8";
            this.TrackLabel8.Size = new System.Drawing.Size(13, 14);
            this.TrackLabel8.TabIndex = 37;
            this.TrackLabel8.Text = "8";
            this.TrackLabel8.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // TrackLabel9
            // 
            this.TrackLabel9.AutoSize = true;
            this.TrackLabel9.ForeColor = System.Drawing.Color.Blue;
            this.TrackLabel9.Location = new System.Drawing.Point(354, 22);
            this.TrackLabel9.Name = "TrackLabel9";
            this.TrackLabel9.Size = new System.Drawing.Size(13, 14);
            this.TrackLabel9.TabIndex = 38;
            this.TrackLabel9.Text = "9";
            this.TrackLabel9.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // TrackLabel10
            // 
            this.TrackLabel10.AutoSize = true;
            this.TrackLabel10.ForeColor = System.Drawing.Color.Blue;
            this.TrackLabel10.Location = new System.Drawing.Point(382, 22);
            this.TrackLabel10.Name = "TrackLabel10";
            this.TrackLabel10.Size = new System.Drawing.Size(19, 14);
            this.TrackLabel10.TabIndex = 39;
            this.TrackLabel10.Text = "10";
            this.TrackLabel10.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // TrackLabel11
            // 
            this.TrackLabel11.AutoSize = true;
            this.TrackLabel11.ForeColor = System.Drawing.Color.Blue;
            this.TrackLabel11.Location = new System.Drawing.Point(413, 22);
            this.TrackLabel11.Name = "TrackLabel11";
            this.TrackLabel11.Size = new System.Drawing.Size(18, 14);
            this.TrackLabel11.TabIndex = 40;
            this.TrackLabel11.Text = "11";
            this.TrackLabel11.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // TrackLabel12
            // 
            this.TrackLabel12.AutoSize = true;
            this.TrackLabel12.ForeColor = System.Drawing.Color.Blue;
            this.TrackLabel12.Location = new System.Drawing.Point(444, 22);
            this.TrackLabel12.Name = "TrackLabel12";
            this.TrackLabel12.Size = new System.Drawing.Size(19, 14);
            this.TrackLabel12.TabIndex = 41;
            this.TrackLabel12.Text = "12";
            this.TrackLabel12.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // TrackLabel13
            // 
            this.TrackLabel13.AutoSize = true;
            this.TrackLabel13.ForeColor = System.Drawing.Color.Blue;
            this.TrackLabel13.Location = new System.Drawing.Point(475, 22);
            this.TrackLabel13.Name = "TrackLabel13";
            this.TrackLabel13.Size = new System.Drawing.Size(19, 14);
            this.TrackLabel13.TabIndex = 42;
            this.TrackLabel13.Text = "13";
            this.TrackLabel13.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // TrackLabel14
            // 
            this.TrackLabel14.AutoSize = true;
            this.TrackLabel14.ForeColor = System.Drawing.Color.Blue;
            this.TrackLabel14.Location = new System.Drawing.Point(505, 22);
            this.TrackLabel14.Name = "TrackLabel14";
            this.TrackLabel14.Size = new System.Drawing.Size(19, 14);
            this.TrackLabel14.TabIndex = 43;
            this.TrackLabel14.Text = "14";
            this.TrackLabel14.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // TrackLabel15
            // 
            this.TrackLabel15.AutoSize = true;
            this.TrackLabel15.ForeColor = System.Drawing.Color.Blue;
            this.TrackLabel15.Location = new System.Drawing.Point(536, 22);
            this.TrackLabel15.Name = "TrackLabel15";
            this.TrackLabel15.Size = new System.Drawing.Size(19, 14);
            this.TrackLabel15.TabIndex = 44;
            this.TrackLabel15.Text = "15";
            this.TrackLabel15.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // label10
            // 
            this.label10.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label10.AutoSize = true;
            this.label10.ForeColor = System.Drawing.Color.RoyalBlue;
            this.label10.Location = new System.Drawing.Point(61, 285);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(500, 14);
            this.label10.TabIndex = 45;
            this.label10.Text = "maxVolume: 127, volume: 100, pwDeviation: 2, pitchWheel: 64, pan: 64, expression:" +
    " 127, modulation: 0";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.ForeColor = System.Drawing.Color.Green;
            this.label11.Location = new System.Drawing.Point(427, 268);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(35, 14);
            this.label11.TabIndex = 46;
            this.label11.Text = "green";
            // 
            // TrackInitForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 14F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(255)))), ((int)(((byte)(245)))));
            this.ClientSize = new System.Drawing.Size(622, 359);
            this.ControlBox = false;
            this.Controls.Add(this.label11);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.TrackLabel15);
            this.Controls.Add(this.TrackLabel14);
            this.Controls.Add(this.TrackLabel13);
            this.Controls.Add(this.TrackLabel12);
            this.Controls.Add(this.TrackLabel11);
            this.Controls.Add(this.TrackLabel10);
            this.Controls.Add(this.TrackLabel9);
            this.Controls.Add(this.TrackLabel8);
            this.Controls.Add(this.TrackLabel7);
            this.Controls.Add(this.TrackLabel6);
            this.Controls.Add(this.TrackLabel16);
            this.Controls.Add(this.TrackLabel5);
            this.Controls.Add(this.TrackLabel4);
            this.Controls.Add(this.TrackLabel3);
            this.Controls.Add(this.TrackLabel2);
            this.Controls.Add(this.TrackLabel1);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.ShowMainScoreFormButton);
            this.Controls.Add(this.SaveSettingsButton);
            this.Font = new System.Drawing.Font("Arial", 8F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Location = new System.Drawing.Point(250, 100);
            this.Name = "TrackInitForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Assistant Composer: Track Initialization Values";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button SaveSettingsButton;
        private System.Windows.Forms.Button ShowMainScoreFormButton;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label TrackLabel1;
        private System.Windows.Forms.Label TrackLabel2;
        private System.Windows.Forms.Label TrackLabel3;
        private System.Windows.Forms.Label TrackLabel4;
        private System.Windows.Forms.Label TrackLabel5;
        private System.Windows.Forms.Label TrackLabel16;
        private System.Windows.Forms.Label TrackLabel6;
        private System.Windows.Forms.Label TrackLabel7;
        private System.Windows.Forms.Label TrackLabel8;
        private System.Windows.Forms.Label TrackLabel9;
        private System.Windows.Forms.Label TrackLabel10;
        private System.Windows.Forms.Label TrackLabel11;
        private System.Windows.Forms.Label TrackLabel12;
        private System.Windows.Forms.Label TrackLabel13;
        private System.Windows.Forms.Label TrackLabel14;
        private System.Windows.Forms.Label TrackLabel15;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label11;
    }
}