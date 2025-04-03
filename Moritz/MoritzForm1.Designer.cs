namespace Moritz
{
	partial class MoritzForm1
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
			this.QuitButton = new System.Windows.Forms.Button();
			this.PreferencesButton = new System.Windows.Forms.Button();
			this.LoadScoreSettingsButton = new System.Windows.Forms.Button();
			this.KrystalsEditorButton = new System.Windows.Forms.Button();
			this.AssistantComposerPanel = new System.Windows.Forms.Panel();
			this.AssistantComposerLabel = new System.Windows.Forms.Label();
			this.KrystalsEditorPanel = new System.Windows.Forms.Panel();
			this.KrystalsEditorLabel = new System.Windows.Forms.Label();
			this.panel1 = new System.Windows.Forms.Panel();
			this.AboutButton = new System.Windows.Forms.Button();
			this.label1 = new System.Windows.Forms.Label();
			this.AssistantComposerPanel.SuspendLayout();
			this.KrystalsEditorPanel.SuspendLayout();
			this.panel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// QuitButton
			// 
			this.QuitButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(64)))), ((int)(((byte)(0)))));
			this.QuitButton.Location = new System.Drawing.Point(105, 281);
			this.QuitButton.Name = "QuitButton";
			this.QuitButton.Size = new System.Drawing.Size(113, 31);
			this.QuitButton.TabIndex = 2;
			this.QuitButton.Text = "quit";
			this.QuitButton.UseVisualStyleBackColor = true;
			this.QuitButton.Click += new System.EventHandler(this.QuitButton_Click);
			// 
			// PreferencesButton
			// 
			this.PreferencesButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(64)))), ((int)(((byte)(0)))));
			this.PreferencesButton.Location = new System.Drawing.Point(31, 30);
			this.PreferencesButton.Name = "PreferencesButton";
			this.PreferencesButton.Size = new System.Drawing.Size(113, 31);
			this.PreferencesButton.TabIndex = 0;
			this.PreferencesButton.Text = "folder locations";
			this.PreferencesButton.UseVisualStyleBackColor = true;
			this.PreferencesButton.Click += new System.EventHandler(this.PreferencesButton_Click);
			// 
			// LoadScoreSettingsButton
			// 
			this.LoadScoreSettingsButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(64)))), ((int)(((byte)(0)))));
			this.LoadScoreSettingsButton.Location = new System.Drawing.Point(92, 30);
			this.LoadScoreSettingsButton.Name = "LoadScoreSettingsButton";
			this.LoadScoreSettingsButton.Size = new System.Drawing.Size(113, 31);
			this.LoadScoreSettingsButton.TabIndex = 0;
			this.LoadScoreSettingsButton.Text = "load settings";
			this.LoadScoreSettingsButton.UseVisualStyleBackColor = true;
			this.LoadScoreSettingsButton.Click += new System.EventHandler(this.LoadScoreSettingsButton_Click);
			// 
			// KrystalsEditorButton
			// 
			this.KrystalsEditorButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(64)))), ((int)(((byte)(0)))));
			this.KrystalsEditorButton.Location = new System.Drawing.Point(92, 30);
			this.KrystalsEditorButton.Name = "KrystalsEditorButton";
			this.KrystalsEditorButton.Size = new System.Drawing.Size(113, 31);
			this.KrystalsEditorButton.TabIndex = 0;
			this.KrystalsEditorButton.Text = "open";
			this.KrystalsEditorButton.UseVisualStyleBackColor = true;
			this.KrystalsEditorButton.Click += new System.EventHandler(this.KrystalsEditorButton_Click);
			// 
			// AssistantComposerPanel
			// 
			this.AssistantComposerPanel.BackColor = System.Drawing.Color.Honeydew;
			this.AssistantComposerPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.AssistantComposerPanel.Controls.Add(this.AssistantComposerLabel);
			this.AssistantComposerPanel.Controls.Add(this.LoadScoreSettingsButton);
			this.AssistantComposerPanel.Location = new System.Drawing.Point(12, 99);
			this.AssistantComposerPanel.Name = "AssistantComposerPanel";
			this.AssistantComposerPanel.Size = new System.Drawing.Size(294, 80);
			this.AssistantComposerPanel.TabIndex = 0;
			// 
			// AssistantComposerLabel
			// 
			this.AssistantComposerLabel.AutoSize = true;
			this.AssistantComposerLabel.Font = new System.Drawing.Font("Verdana", 9F);
			this.AssistantComposerLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(64)))), ((int)(((byte)(0)))));
			this.AssistantComposerLabel.Location = new System.Drawing.Point(82, 8);
			this.AssistantComposerLabel.Name = "AssistantComposerLabel";
			this.AssistantComposerLabel.Size = new System.Drawing.Size(133, 14);
			this.AssistantComposerLabel.TabIndex = 0;
			this.AssistantComposerLabel.Text = "Assistant Composer";
			// 
			// KrystalsEditorPanel
			// 
			this.KrystalsEditorPanel.BackColor = System.Drawing.Color.Honeydew;
			this.KrystalsEditorPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.KrystalsEditorPanel.Controls.Add(this.KrystalsEditorLabel);
			this.KrystalsEditorPanel.Controls.Add(this.KrystalsEditorButton);
			this.KrystalsEditorPanel.Location = new System.Drawing.Point(12, 185);
			this.KrystalsEditorPanel.Name = "KrystalsEditorPanel";
			this.KrystalsEditorPanel.Size = new System.Drawing.Size(294, 80);
			this.KrystalsEditorPanel.TabIndex = 2;
			// 
			// KrystalsEditorLabel
			// 
			this.KrystalsEditorLabel.AutoSize = true;
			this.KrystalsEditorLabel.Font = new System.Drawing.Font("Verdana", 9F);
			this.KrystalsEditorLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(64)))), ((int)(((byte)(0)))));
			this.KrystalsEditorLabel.Location = new System.Drawing.Point(99, 8);
			this.KrystalsEditorLabel.Name = "KrystalsEditorLabel";
			this.KrystalsEditorLabel.Size = new System.Drawing.Size(98, 14);
			this.KrystalsEditorLabel.TabIndex = 0;
			this.KrystalsEditorLabel.Text = "Krystals Editor";
			// 
			// panel1
			// 
			this.panel1.BackColor = System.Drawing.Color.Honeydew;
			this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panel1.Controls.Add(this.AboutButton);
			this.panel1.Controls.Add(this.label1);
			this.panel1.Controls.Add(this.PreferencesButton);
			this.panel1.Location = new System.Drawing.Point(12, 12);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(294, 80);
			this.panel1.TabIndex = 3;
			// 
			// AboutButton
			// 
			this.AboutButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(64)))), ((int)(((byte)(0)))));
			this.AboutButton.Location = new System.Drawing.Point(154, 30);
			this.AboutButton.Name = "AboutButton";
			this.AboutButton.Size = new System.Drawing.Size(113, 31);
			this.AboutButton.TabIndex = 2;
			this.AboutButton.Text = "about Moritz";
			this.AboutButton.UseVisualStyleBackColor = true;
			this.AboutButton.Click += new System.EventHandler(this.AboutButton_Click);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Font = new System.Drawing.Font("Verdana", 9F);
			this.label1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(64)))), ((int)(((byte)(0)))));
			this.label1.Location = new System.Drawing.Point(125, 8);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(45, 14);
			this.label1.TabIndex = 0;
			this.label1.Text = "Moritz";
			// 
			// MoritzForm1
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(255)))), ((int)(((byte)(245)))));
			this.ClientSize = new System.Drawing.Size(319, 328);
			this.ControlBox = false;
			this.Controls.Add(this.panel1);
			this.Controls.Add(this.KrystalsEditorPanel);
			this.Controls.Add(this.QuitButton);
			this.Controls.Add(this.AssistantComposerPanel);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "MoritzForm1";
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Moritz v3";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MoritzForm1_FormClosing);
			this.AssistantComposerPanel.ResumeLayout(false);
			this.AssistantComposerPanel.PerformLayout();
			this.KrystalsEditorPanel.ResumeLayout(false);
			this.KrystalsEditorPanel.PerformLayout();
			this.panel1.ResumeLayout(false);
			this.panel1.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

        private System.Windows.Forms.Button QuitButton;
		private System.Windows.Forms.Button PreferencesButton;
		private System.Windows.Forms.Button LoadScoreSettingsButton;
		private System.Windows.Forms.Button KrystalsEditorButton;
        private System.Windows.Forms.Panel AssistantComposerPanel;
        private System.Windows.Forms.Label AssistantComposerLabel;
        private System.Windows.Forms.Panel KrystalsEditorPanel;
		private System.Windows.Forms.Label KrystalsEditorLabel;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Button AboutButton;
		private System.Windows.Forms.Label label1;

    }
}