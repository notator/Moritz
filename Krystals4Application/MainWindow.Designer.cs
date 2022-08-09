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
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.constantkrystalToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.expansionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.modulatedkrystalToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.NewModulatedKrystalMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openKrystalToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.OpenConstantToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.OpenExpansionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.extractionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.graftToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.justificationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.OpenLineToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.OpenModulatedKrystalMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItemRebuildKrystalFamily = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItemQuit = new System.Windows.Forms.ToolStripMenuItem();
            this.editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutKrystals40ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.NewLineKrysButton = new System.Windows.Forms.Button();
            this.NewConstKrysButton = new System.Windows.Forms.Button();
            this.NewExpKrysButton = new System.Windows.Forms.Button();
            this.NewModKrysButton = new System.Windows.Forms.Button();
            this.RebuildKrystalFamilyButton = new System.Windows.Forms.Button();
            this.NewPathKrysButton = new System.Windows.Forms.Button();
            this.OpenKrystalsBrowserButton = new System.Windows.Forms.Button();
            this.SaveKrystalsWithNewNamesButton = new System.Windows.Forms.Button();
            this.archivePanel = new System.Windows.Forms.Panel();
            this.archivelabel = new System.Windows.Forms.Label();
            this.GlobalPanel = new System.Windows.Forms.Panel();
            this.globalLabel = new System.Windows.Forms.Label();
            this.editorsPanel = new System.Windows.Forms.Panel();
            this.editorsLabel = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.newLabel = new System.Windows.Forms.Label();
            this.globalTextlabel = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.NewExpansionKrysButton = new System.Windows.Forms.Button();
            this.shapedToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.permutationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pathToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1.SuspendLayout();
            this.archivePanel.SuspendLayout();
            this.GlobalPanel.SuspendLayout();
            this.editorsPanel.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.editToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(424, 24);
            this.menuStrip1.TabIndex = 4;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newToolStripMenuItem,
            this.openKrystalToolStripMenuItem,
            this.MenuItemRebuildKrystalFamily,
            this.MenuItemQuit});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // newToolStripMenuItem
            // 
            this.newToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.constantkrystalToolStripMenuItem,
            this.modulatedkrystalToolStripMenuItem,
            this.expansionToolStripMenuItem,
            this.NewModulatedKrystalMenuItem,
            this.shapedToolStripMenuItem,
            this.permutationToolStripMenuItem,
            this.pathToolStripMenuItem});
            this.newToolStripMenuItem.Name = "newToolStripMenuItem";
            this.newToolStripMenuItem.Size = new System.Drawing.Size(187, 22);
            this.newToolStripMenuItem.Text = "New krystal...";
            // 
            // constantkrystalToolStripMenuItem
            // 
            this.constantkrystalToolStripMenuItem.Name = "constantkrystalToolStripMenuItem";
            this.constantkrystalToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.constantkrystalToolStripMenuItem.Text = "constant...";
            this.constantkrystalToolStripMenuItem.Click += new System.EventHandler(this.NewConstantKrystalMenuItem_Click);
            // 
            // expansionToolStripMenuItem
            // 
            this.expansionToolStripMenuItem.Name = "expansionToolStripMenuItem";
            this.expansionToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.expansionToolStripMenuItem.Text = "expansion...";
            this.expansionToolStripMenuItem.Click += new System.EventHandler(this.NewExpansionKrystalMenuItem_Click);
            // 
            // modulatedkrystalToolStripMenuItem
            // 
            this.modulatedkrystalToolStripMenuItem.Name = "modulatedkrystalToolStripMenuItem";
            this.modulatedkrystalToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.modulatedkrystalToolStripMenuItem.Text = "line...";
            this.modulatedkrystalToolStripMenuItem.Click += new System.EventHandler(this.NewLineKrystalMenuItem_Click);
            // 
            // NewModulatedKrystalMenuItem
            // 
            this.NewModulatedKrystalMenuItem.Name = "NewModulatedKrystalMenuItem";
            this.NewModulatedKrystalMenuItem.Size = new System.Drawing.Size(180, 22);
            this.NewModulatedKrystalMenuItem.Text = "modulation...";
            this.NewModulatedKrystalMenuItem.Click += new System.EventHandler(this.NewModulatedKrystalMenuItem_Click);
            // 
            // openKrystalToolStripMenuItem
            // 
            this.openKrystalToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.OpenConstantToolStripMenuItem,
            this.OpenExpansionToolStripMenuItem,
            this.extractionToolStripMenuItem,
            this.graftToolStripMenuItem,
            this.justificationToolStripMenuItem,
            this.OpenLineToolStripMenuItem,
            this.OpenModulatedKrystalMenuItem});
            this.openKrystalToolStripMenuItem.Name = "openKrystalToolStripMenuItem";
            this.openKrystalToolStripMenuItem.Size = new System.Drawing.Size(187, 22);
            this.openKrystalToolStripMenuItem.Text = "Open krystal...";
            // 
            // OpenConstantToolStripMenuItem
            // 
            this.OpenConstantToolStripMenuItem.Name = "OpenConstantToolStripMenuItem";
            this.OpenConstantToolStripMenuItem.Size = new System.Drawing.Size(145, 22);
            this.OpenConstantToolStripMenuItem.Text = "constant...";
            this.OpenConstantToolStripMenuItem.Click += new System.EventHandler(this.OpenConstantKrystalMenuItem_Click);
            // 
            // OpenExpansionToolStripMenuItem
            // 
            this.OpenExpansionToolStripMenuItem.Name = "OpenExpansionToolStripMenuItem";
            this.OpenExpansionToolStripMenuItem.Size = new System.Drawing.Size(145, 22);
            this.OpenExpansionToolStripMenuItem.Text = "expansion...";
            this.OpenExpansionToolStripMenuItem.Click += new System.EventHandler(this.OpenExpansionKrystalMenuItem_Click);
            // 
            // extractionToolStripMenuItem
            // 
            this.extractionToolStripMenuItem.Enabled = false;
            this.extractionToolStripMenuItem.Name = "extractionToolStripMenuItem";
            this.extractionToolStripMenuItem.Size = new System.Drawing.Size(145, 22);
            this.extractionToolStripMenuItem.Text = "extraction...";
            // 
            // graftToolStripMenuItem
            // 
            this.graftToolStripMenuItem.Enabled = false;
            this.graftToolStripMenuItem.Name = "graftToolStripMenuItem";
            this.graftToolStripMenuItem.Size = new System.Drawing.Size(145, 22);
            this.graftToolStripMenuItem.Text = "graft...";
            // 
            // justificationToolStripMenuItem
            // 
            this.justificationToolStripMenuItem.Enabled = false;
            this.justificationToolStripMenuItem.Name = "justificationToolStripMenuItem";
            this.justificationToolStripMenuItem.Size = new System.Drawing.Size(145, 22);
            this.justificationToolStripMenuItem.Text = "justification...";
            // 
            // OpenLineToolStripMenuItem
            // 
            this.OpenLineToolStripMenuItem.Name = "OpenLineToolStripMenuItem";
            this.OpenLineToolStripMenuItem.Size = new System.Drawing.Size(145, 22);
            this.OpenLineToolStripMenuItem.Text = "line...";
            this.OpenLineToolStripMenuItem.Click += new System.EventHandler(this.OpenLineKrystalMenuItem_Click);
            // 
            // OpenModulatedKrystalMenuItem
            // 
            this.OpenModulatedKrystalMenuItem.Name = "OpenModulatedKrystalMenuItem";
            this.OpenModulatedKrystalMenuItem.Size = new System.Drawing.Size(145, 22);
            this.OpenModulatedKrystalMenuItem.Text = "modulation...";
            this.OpenModulatedKrystalMenuItem.Click += new System.EventHandler(this.OpenModulatedKrystalMenuItem_Click);
            // 
            // MenuItemRebuildKrystalFamily
            // 
            this.MenuItemRebuildKrystalFamily.Name = "MenuItemRebuildKrystalFamily";
            this.MenuItemRebuildKrystalFamily.Size = new System.Drawing.Size(187, 22);
            this.MenuItemRebuildKrystalFamily.Text = "Rebuild krystal family";
            this.MenuItemRebuildKrystalFamily.Click += new System.EventHandler(this.MenuItemRebuildKrystalFamily_Click);
            // 
            // MenuItemQuit
            // 
            this.MenuItemQuit.Name = "MenuItemQuit";
            this.MenuItemQuit.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Q)));
            this.MenuItemQuit.Size = new System.Drawing.Size(187, 22);
            this.MenuItemQuit.Text = "Quit";
            this.MenuItemQuit.Click += new System.EventHandler(this.MenuItemQuit_Click);
            // 
            // editToolStripMenuItem
            // 
            this.editToolStripMenuItem.Name = "editToolStripMenuItem";
            this.editToolStripMenuItem.Size = new System.Drawing.Size(39, 20);
            this.editToolStripMenuItem.Text = "Edit";
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.aboutKrystals40ToolStripMenuItem});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.helpToolStripMenuItem.Text = "Help";
            // 
            // aboutKrystals40ToolStripMenuItem
            // 
            this.aboutKrystals40ToolStripMenuItem.Name = "aboutKrystals40ToolStripMenuItem";
            this.aboutKrystals40ToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.aboutKrystals40ToolStripMenuItem.Text = "About Krystals 4.0...";
            this.aboutKrystals40ToolStripMenuItem.Click += new System.EventHandler(this.About_Click);
            // 
            // NewLineKrysButton
            // 
            this.NewLineKrysButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.NewLineKrysButton.Location = new System.Drawing.Point(217, 36);
            this.NewLineKrysButton.Name = "NewLineKrysButton";
            this.NewLineKrysButton.Size = new System.Drawing.Size(160, 42);
            this.NewLineKrysButton.TabIndex = 2;
            this.NewLineKrysButton.Text = "new line krystal";
            this.NewLineKrysButton.Click += new System.EventHandler(this.NewLineKrystalMenuItem_Click);
            // 
            // NewConstKrysButton
            // 
            this.NewConstKrysButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.NewConstKrysButton.Location = new System.Drawing.Point(15, 36);
            this.NewConstKrysButton.Name = "NewConstKrysButton";
            this.NewConstKrysButton.Size = new System.Drawing.Size(160, 42);
            this.NewConstKrysButton.TabIndex = 1;
            this.NewConstKrysButton.Text = "new constant krystal";
            this.NewConstKrysButton.Click += new System.EventHandler(this.NewConstantKrystalMenuItem_Click);
            // 
            // NewExpKrysButton
            // 
            this.NewExpKrysButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.NewExpKrysButton.Location = new System.Drawing.Point(15, 37);
            this.NewExpKrysButton.Name = "NewExpKrysButton";
            this.NewExpKrysButton.Size = new System.Drawing.Size(160, 42);
            this.NewExpKrysButton.TabIndex = 3;
            this.NewExpKrysButton.Text = "open expansion editor";
            this.NewExpKrysButton.Click += new System.EventHandler(this.NewExpansionKrystalMenuItem_Click);
            // 
            // NewModKrysButton
            // 
            this.NewModKrysButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.NewModKrysButton.Location = new System.Drawing.Point(217, 37);
            this.NewModKrysButton.Name = "NewModKrysButton";
            this.NewModKrysButton.Size = new System.Drawing.Size(160, 42);
            this.NewModKrysButton.TabIndex = 4;
            this.NewModKrysButton.Text = "open modulation editor";
            this.NewModKrysButton.Click += new System.EventHandler(this.NewModulatedKrystalMenuItem_Click);
            // 
            // RebuildKrystalFamilyButton
            // 
            this.RebuildKrystalFamilyButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.RebuildKrystalFamilyButton.Location = new System.Drawing.Point(217, 51);
            this.RebuildKrystalFamilyButton.Name = "RebuildKrystalFamilyButton";
            this.RebuildKrystalFamilyButton.Size = new System.Drawing.Size(160, 42);
            this.RebuildKrystalFamilyButton.TabIndex = 5;
            this.RebuildKrystalFamilyButton.Text = "rebuild krystal family";
            this.RebuildKrystalFamilyButton.Click += new System.EventHandler(this.MenuItemRebuildKrystalFamily_Click);
            // 
            // NewPathKrysButton
            // 
            this.NewPathKrysButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.NewPathKrysButton.Location = new System.Drawing.Point(217, 167);
            this.NewPathKrysButton.Name = "NewPathKrysButton";
            this.NewPathKrysButton.Size = new System.Drawing.Size(160, 42);
            this.NewPathKrysButton.TabIndex = 0;
            this.NewPathKrysButton.Text = "new path krystal";
            this.NewPathKrysButton.Click += new System.EventHandler(this.NewPathKrystalMenuItem_Click);
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
            this.archivePanel.Location = new System.Drawing.Point(12, 530);
            this.archivePanel.Name = "archivePanel";
            this.archivePanel.Size = new System.Drawing.Size(400, 106);
            this.archivePanel.TabIndex = 8;
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
            this.GlobalPanel.Controls.Add(this.RebuildKrystalFamilyButton);
            this.GlobalPanel.Location = new System.Drawing.Point(12, 406);
            this.GlobalPanel.Name = "GlobalPanel";
            this.GlobalPanel.Size = new System.Drawing.Size(400, 106);
            this.GlobalPanel.TabIndex = 9;
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
            this.editorsPanel.Location = new System.Drawing.Point(12, 295);
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
            this.editorsLabel.Size = new System.Drawing.Size(49, 16);
            this.editorsLabel.TabIndex = 9;
            this.editorsLabel.Text = "Editors";
            // 
            // panel1
            // 
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Controls.Add(this.NewExpansionKrysButton);
            this.panel1.Controls.Add(this.newLabel);
            this.panel1.Controls.Add(this.NewConstKrysButton);
            this.panel1.Controls.Add(this.NewLineKrysButton);
            this.panel1.Controls.Add(this.NewPathKrysButton);
            this.panel1.Location = new System.Drawing.Point(12, 40);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(400, 237);
            this.panel1.TabIndex = 11;
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
            // NewExpansionKrysButton
            // 
            this.NewExpansionKrysButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.NewExpansionKrysButton.Location = new System.Drawing.Point(16, 84);
            this.NewExpansionKrysButton.Name = "NewExpansionKrysButton";
            this.NewExpansionKrysButton.Size = new System.Drawing.Size(160, 42);
            this.NewExpansionKrysButton.TabIndex = 10;
            this.NewExpansionKrysButton.Text = "new expansion krystal";
            this.NewExpansionKrysButton.Click += new System.EventHandler(this.NewExpansionKrystalMenuItem_Click);
            // 
            // shapedToolStripMenuItem
            // 
            this.shapedToolStripMenuItem.Name = "shapedToolStripMenuItem";
            this.shapedToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.shapedToolStripMenuItem.Text = "shaped";
            // 
            // permutationToolStripMenuItem
            // 
            this.permutationToolStripMenuItem.Name = "permutationToolStripMenuItem";
            this.permutationToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.permutationToolStripMenuItem.Text = "permutation";
            // 
            // pathToolStripMenuItem
            // 
            this.pathToolStripMenuItem.Name = "pathToolStripMenuItem";
            this.pathToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.pathToolStripMenuItem.Text = "path";
            // 
            // MainWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ControlLight;
            this.ClientSize = new System.Drawing.Size(424, 655);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.editorsPanel);
            this.Controls.Add(this.GlobalPanel);
            this.Controls.Add(this.archivePanel);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "MainWindow";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Krystals 4.0";
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.archivePanel.ResumeLayout(false);
            this.archivePanel.PerformLayout();
            this.GlobalPanel.ResumeLayout(false);
            this.GlobalPanel.PerformLayout();
            this.editorsPanel.ResumeLayout(false);
            this.editorsPanel.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem editToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem newToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem constantkrystalToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem expansionToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutKrystals40ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem modulatedkrystalToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem NewModulatedKrystalMenuItem;
        private System.Windows.Forms.Button NewLineKrysButton;
        private System.Windows.Forms.Button NewConstKrysButton;
        private System.Windows.Forms.Button NewExpKrysButton;
        private System.Windows.Forms.ToolStripMenuItem openKrystalToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem OpenConstantToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem OpenExpansionToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem extractionToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem graftToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem justificationToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem OpenLineToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem OpenModulatedKrystalMenuItem;
        private System.Windows.Forms.Button NewModKrysButton;
        private System.Windows.Forms.ToolStripMenuItem MenuItemQuit;
        private System.Windows.Forms.ToolStripMenuItem MenuItemRebuildKrystalFamily;
        private System.Windows.Forms.Button RebuildKrystalFamilyButton;
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
        private System.Windows.Forms.ToolStripMenuItem shapedToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem permutationToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem pathToolStripMenuItem;
    }
}

