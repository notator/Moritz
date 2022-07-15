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
            this.graftkrystalToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.justifiedkrystalToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.linekrystalToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
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
            this.menuStrip1.SuspendLayout();
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
            this.expansionToolStripMenuItem,
            this.graftkrystalToolStripMenuItem,
            this.justifiedkrystalToolStripMenuItem,
            this.linekrystalToolStripMenuItem,
            this.modulatedkrystalToolStripMenuItem,
            this.NewModulatedKrystalMenuItem});
            this.newToolStripMenuItem.Name = "newToolStripMenuItem";
            this.newToolStripMenuItem.Size = new System.Drawing.Size(187, 22);
            this.newToolStripMenuItem.Text = "New krystal...";
            // 
            // constantkrystalToolStripMenuItem
            // 
            this.constantkrystalToolStripMenuItem.Name = "constantkrystalToolStripMenuItem";
            this.constantkrystalToolStripMenuItem.Size = new System.Drawing.Size(145, 22);
            this.constantkrystalToolStripMenuItem.Text = "constant...";
            this.constantkrystalToolStripMenuItem.Click += new System.EventHandler(this.NewConstantKrystalMenuItem_Click);
            // 
            // expansionToolStripMenuItem
            // 
            this.expansionToolStripMenuItem.Name = "expansionToolStripMenuItem";
            this.expansionToolStripMenuItem.Size = new System.Drawing.Size(145, 22);
            this.expansionToolStripMenuItem.Text = "expansion...";
            this.expansionToolStripMenuItem.Click += new System.EventHandler(this.NewExpansionKrystalMenuItem_Click);
            // 
            // graftkrystalToolStripMenuItem
            // 
            this.graftkrystalToolStripMenuItem.Enabled = false;
            this.graftkrystalToolStripMenuItem.Name = "graftkrystalToolStripMenuItem";
            this.graftkrystalToolStripMenuItem.Size = new System.Drawing.Size(145, 22);
            this.graftkrystalToolStripMenuItem.Text = "extraction...";
            // 
            // justifiedkrystalToolStripMenuItem
            // 
            this.justifiedkrystalToolStripMenuItem.Enabled = false;
            this.justifiedkrystalToolStripMenuItem.Name = "justifiedkrystalToolStripMenuItem";
            this.justifiedkrystalToolStripMenuItem.Size = new System.Drawing.Size(145, 22);
            this.justifiedkrystalToolStripMenuItem.Text = "graft...";
            // 
            // linekrystalToolStripMenuItem
            // 
            this.linekrystalToolStripMenuItem.Enabled = false;
            this.linekrystalToolStripMenuItem.Name = "linekrystalToolStripMenuItem";
            this.linekrystalToolStripMenuItem.Size = new System.Drawing.Size(145, 22);
            this.linekrystalToolStripMenuItem.Text = "justification...";
            // 
            // modulatedkrystalToolStripMenuItem
            // 
            this.modulatedkrystalToolStripMenuItem.Name = "modulatedkrystalToolStripMenuItem";
            this.modulatedkrystalToolStripMenuItem.Size = new System.Drawing.Size(145, 22);
            this.modulatedkrystalToolStripMenuItem.Text = "line...";
            this.modulatedkrystalToolStripMenuItem.Click += new System.EventHandler(this.NewLineKrystalMenuItem_Click);
            // 
            // NewModulatedKrystalMenuItem
            // 
            this.NewModulatedKrystalMenuItem.Name = "NewModulatedKrystalMenuItem";
            this.NewModulatedKrystalMenuItem.Size = new System.Drawing.Size(145, 22);
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
            this.aboutKrystals40ToolStripMenuItem.Size = new System.Drawing.Size(177, 22);
            this.aboutKrystals40ToolStripMenuItem.Text = "About Krystals 4.0...";
            this.aboutKrystals40ToolStripMenuItem.Click += new System.EventHandler(this.About_Click);
            // 
            // NewLineKrysButton
            // 
            this.NewLineKrysButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.NewLineKrysButton.Location = new System.Drawing.Point(42, 127);
            this.NewLineKrysButton.Name = "NewLineKrysButton";
            this.NewLineKrysButton.Size = new System.Drawing.Size(160, 42);
            this.NewLineKrysButton.TabIndex = 2;
            this.NewLineKrysButton.Text = "new line krystal";
            this.NewLineKrysButton.Click += new System.EventHandler(this.NewLineKrystalMenuItem_Click);
            // 
            // NewConstKrysButton
            // 
            this.NewConstKrysButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.NewConstKrysButton.Location = new System.Drawing.Point(42, 67);
            this.NewConstKrysButton.Name = "NewConstKrysButton";
            this.NewConstKrysButton.Size = new System.Drawing.Size(160, 42);
            this.NewConstKrysButton.TabIndex = 1;
            this.NewConstKrysButton.Text = "new constant krystal";
            this.NewConstKrysButton.Click += new System.EventHandler(this.NewConstantKrystalMenuItem_Click);
            // 
            // NewExpKrysButton
            // 
            this.NewExpKrysButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.NewExpKrysButton.Location = new System.Drawing.Point(228, 67);
            this.NewExpKrysButton.Name = "NewExpKrysButton";
            this.NewExpKrysButton.Size = new System.Drawing.Size(160, 42);
            this.NewExpKrysButton.TabIndex = 3;
            this.NewExpKrysButton.Text = "open expansion editor";
            this.NewExpKrysButton.Click += new System.EventHandler(this.NewExpansionKrystalMenuItem_Click);
            // 
            // NewModKrysButton
            // 
            this.NewModKrysButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.NewModKrysButton.Location = new System.Drawing.Point(228, 127);
            this.NewModKrysButton.Name = "NewModKrysButton";
            this.NewModKrysButton.Size = new System.Drawing.Size(160, 42);
            this.NewModKrysButton.TabIndex = 4;
            this.NewModKrysButton.Text = "open modulation editor";
            this.NewModKrysButton.Click += new System.EventHandler(this.NewModulatedKrystalMenuItem_Click);
            // 
            // RebuildKrystalFamilyButton
            // 
            this.RebuildKrystalFamilyButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.RebuildKrystalFamilyButton.Location = new System.Drawing.Point(129, 317);
            this.RebuildKrystalFamilyButton.Name = "RebuildKrystalFamilyButton";
            this.RebuildKrystalFamilyButton.Size = new System.Drawing.Size(160, 42);
            this.RebuildKrystalFamilyButton.TabIndex = 5;
            this.RebuildKrystalFamilyButton.Text = "rebuild krystal family";
            this.RebuildKrystalFamilyButton.Click += new System.EventHandler(this.MenuItemRebuildKrystalFamily_Click);
            // 
            // NewPathKrysButton
            // 
            this.NewPathKrysButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.NewPathKrysButton.Location = new System.Drawing.Point(42, 187);
            this.NewPathKrysButton.Name = "NewPathKrysButton";
            this.NewPathKrysButton.Size = new System.Drawing.Size(160, 42);
            this.NewPathKrysButton.TabIndex = 0;
            this.NewPathKrysButton.Text = "new path krystal";
            this.NewPathKrysButton.Click += new System.EventHandler(this.NewPathKrystalMenuItem_Click);
            // 
            // MainWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ControlLight;
            this.ClientSize = new System.Drawing.Size(424, 410);
            this.Controls.Add(this.NewPathKrysButton);
            this.Controls.Add(this.RebuildKrystalFamilyButton);
            this.Controls.Add(this.NewModKrysButton);
            this.Controls.Add(this.NewExpKrysButton);
            this.Controls.Add(this.NewConstKrysButton);
            this.Controls.Add(this.NewLineKrysButton);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "MainWindow";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Krystals 4.0";
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
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
        private System.Windows.Forms.ToolStripMenuItem graftkrystalToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem justifiedkrystalToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem linekrystalToolStripMenuItem;
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
    }
}

