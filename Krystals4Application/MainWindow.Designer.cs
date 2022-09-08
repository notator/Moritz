namespace Krystals4Application
{
    partial class MainWindow
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
            this.NewLineKrysButton = new System.Windows.Forms.Button();
            this.NewConstKrysButton = new System.Windows.Forms.Button();
            this.NewExpKrysButton = new System.Windows.Forms.Button();
            this.NewModKrysButton = new System.Windows.Forms.Button();
            this.DeleteUnusedDuplicatesButton = new System.Windows.Forms.Button();
            this.NewPathKrysButton = new System.Windows.Forms.Button();
            this.OpenKrystalsBrowserButton = new System.Windows.Forms.Button();
            this.SaveKrystalsWithNewNamesButton = new System.Windows.Forms.Button();
            this.archivePanel = new System.Windows.Forms.Panel();
            this.label1 = new System.Windows.Forms.Label();
            this.archivelabel = new System.Windows.Forms.Label();
            this.GlobalPanel = new System.Windows.Forms.Panel();
            this.globalTextlabel = new System.Windows.Forms.Label();
            this.globalLabel = new System.Windows.Forms.Label();
            this.editorsPanel = new System.Windows.Forms.Panel();
            this.editorsLabel = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.NewPermutedKrystalButton = new System.Windows.Forms.Button();
            this.NewModulatedKrystalButton = new System.Windows.Forms.Button();
            this.NewExpansionKrysButton = new System.Windows.Forms.Button();
            this.newLabel = new System.Windows.Forms.Label();
            this.AboutButton = new System.Windows.Forms.Button();
            this.QuitButton = new System.Windows.Forms.Button();
            this.archivePanel.SuspendLayout();
            this.GlobalPanel.SuspendLayout();
            this.editorsPanel.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // NewLineKrysButton
            // 
            this.NewLineKrysButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.NewLineKrysButton.Location = new System.Drawing.Point(217, 36);
            this.NewLineKrysButton.Name = "NewLineKrysButton";
            this.NewLineKrysButton.Size = new System.Drawing.Size(160, 42);
            this.NewLineKrysButton.TabIndex = 2;
            this.NewLineKrysButton.Text = "new line krystal";
            this.NewLineKrysButton.Click += new System.EventHandler(this.NewLineKrystalButton_Click);
            // 
            // NewConstKrysButton
            // 
            this.NewConstKrysButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.NewConstKrysButton.Location = new System.Drawing.Point(15, 36);
            this.NewConstKrysButton.Name = "NewConstKrysButton";
            this.NewConstKrysButton.Size = new System.Drawing.Size(160, 42);
            this.NewConstKrysButton.TabIndex = 1;
            this.NewConstKrysButton.Text = "new constant krystal";
            this.NewConstKrysButton.Click += new System.EventHandler(this.NewConstantKrystalButton_Click);
            // 
            // NewExpKrysButton
            // 
            this.NewExpKrysButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.NewExpKrysButton.Location = new System.Drawing.Point(15, 37);
            this.NewExpKrysButton.Name = "NewExpKrysButton";
            this.NewExpKrysButton.Size = new System.Drawing.Size(160, 42);
            this.NewExpKrysButton.TabIndex = 3;
            this.NewExpKrysButton.Text = "open expansion editor";
            this.NewExpKrysButton.Click += new System.EventHandler(this.NewExpansionKrystalButton_Click);
            // 
            // NewModKrysButton
            // 
            this.NewModKrysButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.NewModKrysButton.Location = new System.Drawing.Point(217, 37);
            this.NewModKrysButton.Name = "NewModKrysButton";
            this.NewModKrysButton.Size = new System.Drawing.Size(160, 42);
            this.NewModKrysButton.TabIndex = 4;
            this.NewModKrysButton.Text = "open modulation editor";
            this.NewModKrysButton.Click += new System.EventHandler(this.NewModulatedKrystalButton_Click);
            // 
            // DeleteUnusedDuplicatesButton
            // 
            this.DeleteUnusedDuplicatesButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.DeleteUnusedDuplicatesButton.Location = new System.Drawing.Point(217, 51);
            this.DeleteUnusedDuplicatesButton.Name = "DeleteUnusedDuplicatesButton";
            this.DeleteUnusedDuplicatesButton.Size = new System.Drawing.Size(160, 42);
            this.DeleteUnusedDuplicatesButton.TabIndex = 5;
            this.DeleteUnusedDuplicatesButton.Text = "delete unused duplicates";
            this.DeleteUnusedDuplicatesButton.Click += new System.EventHandler(this.DeleteUnusedDuplicatesButton_Click);
            // 
            // NewPathKrysButton
            // 
            this.NewPathKrysButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.NewPathKrysButton.Location = new System.Drawing.Point(217, 132);
            this.NewPathKrysButton.Name = "NewPathKrysButton";
            this.NewPathKrysButton.Size = new System.Drawing.Size(160, 42);
            this.NewPathKrysButton.TabIndex = 0;
            this.NewPathKrysButton.Text = "new path krystal";
            this.NewPathKrysButton.Click += new System.EventHandler(this.NewPathKrystalButton_Click);
            // 
            // OpenKrystalsBrowserButton
            // 
            this.OpenKrystalsBrowserButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.OpenKrystalsBrowserButton.Location = new System.Drawing.Point(15, 51);
            this.OpenKrystalsBrowserButton.Name = "OpenKrystalsBrowserButton";
            this.OpenKrystalsBrowserButton.Size = new System.Drawing.Size(160, 42);
            this.OpenKrystalsBrowserButton.TabIndex = 6;
            this.OpenKrystalsBrowserButton.Text = "open krystals browser";
            this.OpenKrystalsBrowserButton.Click += new System.EventHandler(this.OpenKrystalsBrowserButton_Click);
            // 
            // SaveKrystalsWithNewNamesButton
            // 
            this.SaveKrystalsWithNewNamesButton.Enabled = false;
            this.SaveKrystalsWithNewNamesButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.SaveKrystalsWithNewNamesButton.Location = new System.Drawing.Point(14, 49);
            this.SaveKrystalsWithNewNamesButton.Name = "SaveKrystalsWithNewNamesButton";
            this.SaveKrystalsWithNewNamesButton.Size = new System.Drawing.Size(160, 42);
            this.SaveKrystalsWithNewNamesButton.TabIndex = 7;
            this.SaveKrystalsWithNewNamesButton.Text = "save krystals with new names";
            this.SaveKrystalsWithNewNamesButton.Click += new System.EventHandler(this.SaveKrystalsWithNewNamesButton_Click);
            // 
            // archivePanel
            // 
            this.archivePanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.archivePanel.Controls.Add(this.label1);
            this.archivePanel.Controls.Add(this.archivelabel);
            this.archivePanel.Controls.Add(this.SaveKrystalsWithNewNamesButton);
            this.archivePanel.Location = new System.Drawing.Point(12, 442);
            this.archivePanel.Name = "archivePanel";
            this.archivePanel.Size = new System.Drawing.Size(400, 106);
            this.archivePanel.TabIndex = 8;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(12, 25);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(373, 15);
            this.label1.TabIndex = 9;
            this.label1.Text = "These functions exist in the code-base, but should not be run again.";
            // 
            // archivelabel
            // 
            this.archivelabel.AutoSize = true;
            this.archivelabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.archivelabel.ForeColor = System.Drawing.Color.IndianRed;
            this.archivelabel.Location = new System.Drawing.Point(12, 9);
            this.archivelabel.Name = "archivelabel";
            this.archivelabel.Size = new System.Drawing.Size(58, 16);
            this.archivelabel.TabIndex = 8;
            this.archivelabel.Text = "Archive :\r\n";
            // 
            // GlobalPanel
            // 
            this.GlobalPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.GlobalPanel.Controls.Add(this.globalTextlabel);
            this.GlobalPanel.Controls.Add(this.globalLabel);
            this.GlobalPanel.Controls.Add(this.OpenKrystalsBrowserButton);
            this.GlobalPanel.Controls.Add(this.DeleteUnusedDuplicatesButton);
            this.GlobalPanel.Location = new System.Drawing.Point(12, 38);
            this.GlobalPanel.Name = "GlobalPanel";
            this.GlobalPanel.Size = new System.Drawing.Size(400, 106);
            this.GlobalPanel.TabIndex = 9;
            // 
            // globalTextlabel
            // 
            this.globalTextlabel.AutoSize = true;
            this.globalTextlabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.globalTextlabel.Location = new System.Drawing.Point(13, 27);
            this.globalTextlabel.Name = "globalTextlabel";
            this.globalTextlabel.Size = new System.Drawing.Size(179, 15);
            this.globalTextlabel.TabIndex = 10;
            this.globalTextlabel.Text = "Functions relating to all krystals.";
            // 
            // globalLabel
            // 
            this.globalLabel.AutoSize = true;
            this.globalLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.globalLabel.ForeColor = System.Drawing.Color.IndianRed;
            this.globalLabel.Location = new System.Drawing.Point(12, 11);
            this.globalLabel.Name = "globalLabel";
            this.globalLabel.Size = new System.Drawing.Size(50, 16);
            this.globalLabel.TabIndex = 9;
            this.globalLabel.Text = "Global:";
            // 
            // editorsPanel
            // 
            this.editorsPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.editorsPanel.Controls.Add(this.editorsLabel);
            this.editorsPanel.Controls.Add(this.NewExpKrysButton);
            this.editorsPanel.Controls.Add(this.NewModKrysButton);
            this.editorsPanel.Location = new System.Drawing.Point(12, 343);
            this.editorsPanel.Name = "editorsPanel";
            this.editorsPanel.Size = new System.Drawing.Size(400, 93);
            this.editorsPanel.TabIndex = 10;
            // 
            // editorsLabel
            // 
            this.editorsLabel.AutoSize = true;
            this.editorsLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.editorsLabel.ForeColor = System.Drawing.Color.IndianRed;
            this.editorsLabel.Location = new System.Drawing.Point(12, 12);
            this.editorsLabel.Name = "editorsLabel";
            this.editorsLabel.Size = new System.Drawing.Size(139, 16);
            this.editorsLabel.TabIndex = 9;
            this.editorsLabel.Text = "Editors (to be deleted)";
            // 
            // panel1
            // 
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Controls.Add(this.NewPermutedKrystalButton);
            this.panel1.Controls.Add(this.NewModulatedKrystalButton);
            this.panel1.Controls.Add(this.NewExpansionKrysButton);
            this.panel1.Controls.Add(this.newLabel);
            this.panel1.Controls.Add(this.NewConstKrysButton);
            this.panel1.Controls.Add(this.NewLineKrysButton);
            this.panel1.Controls.Add(this.NewPathKrysButton);
            this.panel1.Location = new System.Drawing.Point(12, 150);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(400, 187);
            this.panel1.TabIndex = 11;
            // 
            // NewPermutedKrystalButton
            // 
            this.NewPermutedKrystalButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.NewPermutedKrystalButton.Location = new System.Drawing.Point(16, 132);
            this.NewPermutedKrystalButton.Name = "NewPermutedKrystalButton";
            this.NewPermutedKrystalButton.Size = new System.Drawing.Size(160, 42);
            this.NewPermutedKrystalButton.TabIndex = 12;
            this.NewPermutedKrystalButton.Text = "new permuted krystal";
            this.NewPermutedKrystalButton.Click += new System.EventHandler(this.NewPermutedKrystalButton_Click);
            // 
            // NewModulatedKrystalButton
            // 
            this.NewModulatedKrystalButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.NewModulatedKrystalButton.Location = new System.Drawing.Point(217, 84);
            this.NewModulatedKrystalButton.Name = "NewModulatedKrystalButton";
            this.NewModulatedKrystalButton.Size = new System.Drawing.Size(160, 42);
            this.NewModulatedKrystalButton.TabIndex = 11;
            this.NewModulatedKrystalButton.Text = "new modulated krystal";
            this.NewModulatedKrystalButton.Click += new System.EventHandler(this.NewModulatedKrystalButton_Click);
            // 
            // NewExpansionKrysButton
            // 
            this.NewExpansionKrysButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.NewExpansionKrysButton.Location = new System.Drawing.Point(16, 84);
            this.NewExpansionKrysButton.Name = "NewExpansionKrysButton";
            this.NewExpansionKrysButton.Size = new System.Drawing.Size(160, 42);
            this.NewExpansionKrysButton.TabIndex = 10;
            this.NewExpansionKrysButton.Text = "new expansion krystal";
            this.NewExpansionKrysButton.Click += new System.EventHandler(this.NewExpansionKrystalButton_Click);
            // 
            // newLabel
            // 
            this.newLabel.AutoSize = true;
            this.newLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.newLabel.ForeColor = System.Drawing.Color.IndianRed;
            this.newLabel.Location = new System.Drawing.Point(12, 12);
            this.newLabel.Name = "newLabel";
            this.newLabel.Size = new System.Drawing.Size(34, 16);
            this.newLabel.TabIndex = 9;
            this.newLabel.Text = "New";
            // 
            // AboutButton
            // 
            this.AboutButton.Location = new System.Drawing.Point(337, 9);
            this.AboutButton.Name = "AboutButton";
            this.AboutButton.Size = new System.Drawing.Size(75, 23);
            this.AboutButton.TabIndex = 12;
            this.AboutButton.Text = "About";
            this.AboutButton.UseVisualStyleBackColor = true;
            this.AboutButton.Click += new System.EventHandler(this.About_Click);
            // 
            // QuitButton
            // 
            this.QuitButton.Location = new System.Drawing.Point(337, 554);
            this.QuitButton.Name = "QuitButton";
            this.QuitButton.Size = new System.Drawing.Size(75, 23);
            this.QuitButton.TabIndex = 13;
            this.QuitButton.Text = "Quit";
            this.QuitButton.UseVisualStyleBackColor = true;
            this.QuitButton.Click += new System.EventHandler(this.QuitButton_Click);
            // 
            // MainWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ControlLight;
            this.ClientSize = new System.Drawing.Size(424, 583);
            this.Controls.Add(this.QuitButton);
            this.Controls.Add(this.AboutButton);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.editorsPanel);
            this.Controls.Add(this.GlobalPanel);
            this.Controls.Add(this.archivePanel);
            this.Name = "MainWindow";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Krystals 4.0";
            this.archivePanel.ResumeLayout(false);
            this.archivePanel.PerformLayout();
            this.GlobalPanel.ResumeLayout(false);
            this.GlobalPanel.PerformLayout();
            this.editorsPanel.ResumeLayout(false);
            this.editorsPanel.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Button NewLineKrysButton;
        private System.Windows.Forms.Button NewConstKrysButton;
        private System.Windows.Forms.Button NewExpKrysButton;
        private System.Windows.Forms.Button NewModKrysButton;
        private System.Windows.Forms.Button DeleteUnusedDuplicatesButton;
        private System.Windows.Forms.Button NewPathKrysButton;
        private System.Windows.Forms.Button OpenKrystalsBrowserButton;
        private System.Windows.Forms.Button SaveKrystalsWithNewNamesButton;
        private System.Windows.Forms.Panel archivePanel;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label archivelabel;
        private System.Windows.Forms.Panel GlobalPanel;
        private System.Windows.Forms.Label globalTextlabel;
        private System.Windows.Forms.Label globalLabel;
        private System.Windows.Forms.Panel editorsPanel;
        private System.Windows.Forms.Label editorsLabel;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label newLabel;
        private System.Windows.Forms.Button NewExpansionKrysButton;
        private System.Windows.Forms.Button NewModulatedKrystalButton;
        private System.Windows.Forms.Button NewPermutedKrystalButton;
        private System.Windows.Forms.Button AboutButton;
        private System.Windows.Forms.Button QuitButton;
    }
}

