namespace Moritz.AssistantComposer
{
    partial class PolyPerformerOptionsForm
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
            this.SaveSettingsButton = new System.Windows.Forms.Button();
            this.ShowMainScoreFormButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // SaveSettingsButton
            // 
            this.SaveSettingsButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.SaveSettingsButton.Enabled = false;
            this.SaveSettingsButton.Font = new System.Drawing.Font("Arial", 8F);
            this.SaveSettingsButton.Location = new System.Drawing.Point(513, 305);
            this.SaveSettingsButton.Name = "SaveSettingsButton";
            this.SaveSettingsButton.Size = new System.Drawing.Size(106, 26);
            this.SaveSettingsButton.TabIndex = 7;
            this.SaveSettingsButton.Text = "save settings";
            this.SaveSettingsButton.UseVisualStyleBackColor = true;
            this.SaveSettingsButton.Click += new System.EventHandler(this.SaveSettingsButton_Click);
            // 
            // ShowMainScoreFormButton
            // 
            this.ShowMainScoreFormButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.ShowMainScoreFormButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(215)))), ((int)(((byte)(225)))), ((int)(((byte)(215)))));
            this.ShowMainScoreFormButton.Location = new System.Drawing.Point(350, 305);
            this.ShowMainScoreFormButton.Name = "ShowMainScoreFormButton";
            this.ShowMainScoreFormButton.Size = new System.Drawing.Size(137, 26);
            this.ShowMainScoreFormButton.TabIndex = 8;
            this.ShowMainScoreFormButton.Text = "show main score form";
            this.ShowMainScoreFormButton.UseVisualStyleBackColor = false;
            this.ShowMainScoreFormButton.Click += new System.EventHandler(this.ShowMainScoreFormButton_Click);
            // 
            // PolyPerformerOptionsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 14F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(255)))), ((int)(((byte)(245)))));
            this.ClientSize = new System.Drawing.Size(641, 359);
            this.ControlBox = false;
            this.Controls.Add(this.ShowMainScoreFormButton);
            this.Controls.Add(this.SaveSettingsButton);
            this.Font = new System.Drawing.Font("Arial", 8F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Location = new System.Drawing.Point(250, 100);
            this.Name = "PolyPerformerOptionsForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Assistant Composer: Poly Performer Options";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button SaveSettingsButton;
        private System.Windows.Forms.Button ShowMainScoreFormButton;
    }
}