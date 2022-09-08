namespace Krystals5Application
{
    partial class ModulationEditor
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
            this.splitContainer = new System.Windows.Forms.SplitContainer();
            this.TreeView = new System.Windows.Forms.TreeView();
            this.RightPanel = new System.Windows.Forms.Panel();
            this.ModulateButton = new System.Windows.Forms.Button();
            this.CloseButton = new System.Windows.Forms.Button();
            this.OpenButton = new System.Windows.Forms.Button();
            this.NewButton = new System.Windows.Forms.Button();
            this.SaveButton = new System.Windows.Forms.Button();
            this.splitContainer.Panel1.SuspendLayout();
            this.splitContainer.SuspendLayout();
            this.RightPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer
            // 
            this.splitContainer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.splitContainer.Location = new System.Drawing.Point(0, -1);
            this.splitContainer.Margin = new System.Windows.Forms.Padding(0);
            this.splitContainer.Name = "splitContainer";
            // 
            // splitContainer.Panel1
            // 
            this.splitContainer.Panel1.Controls.Add(this.TreeView);
            // 
            // splitContainer.Panel2
            // 
            this.splitContainer.Panel2.BackColor = System.Drawing.SystemColors.ControlLight;
            this.splitContainer.Panel2.Paint += new System.Windows.Forms.PaintEventHandler(this.SplitContainer_Panel2_Paint);
            this.splitContainer.Panel2.Resize += new System.EventHandler(this.SplitContainer_Panel2_Resize);
            this.splitContainer.Size = new System.Drawing.Size(586, 362);
            this.splitContainer.SplitterDistance = 283;
            this.splitContainer.SplitterWidth = 3;
            this.splitContainer.TabIndex = 1;
            // 
            // TreeView
            // 
            this.TreeView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.TreeView.Location = new System.Drawing.Point(0, 0);
            this.TreeView.Margin = new System.Windows.Forms.Padding(0);
            this.TreeView.Name = "TreeView";
            this.TreeView.Size = new System.Drawing.Size(281, 362);
            this.TreeView.TabIndex = 0;
            // 
            // RightPanel
            // 
            this.RightPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.RightPanel.BackColor = System.Drawing.SystemColors.ControlLight;
            this.RightPanel.Controls.Add(this.ModulateButton);
            this.RightPanel.Controls.Add(this.CloseButton);
            this.RightPanel.Controls.Add(this.OpenButton);
            this.RightPanel.Controls.Add(this.NewButton);
            this.RightPanel.Controls.Add(this.SaveButton);
            this.RightPanel.Location = new System.Drawing.Point(586, 0);
            this.RightPanel.Name = "RightPanel";
            this.RightPanel.Size = new System.Drawing.Size(103, 364);
            this.RightPanel.TabIndex = 0;
            // 
            // ModulateButton
            // 
            this.ModulateButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ModulateButton.Enabled = false;
            this.ModulateButton.Location = new System.Drawing.Point(19, 161);
            this.ModulateButton.Name = "ModulateButton";
            this.ModulateButton.Size = new System.Drawing.Size(65, 23);
            this.ModulateButton.TabIndex = 7;
            this.ModulateButton.Text = "Modulate";
            this.ModulateButton.UseVisualStyleBackColor = true;
            this.ModulateButton.Click += new System.EventHandler(this.ModulateButton_Click);
            // 
            // CloseButton
            // 
            this.CloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.CloseButton.Location = new System.Drawing.Point(19, 92);
            this.CloseButton.Name = "CloseButton";
            this.CloseButton.Size = new System.Drawing.Size(65, 23);
            this.CloseButton.TabIndex = 6;
            this.CloseButton.Text = "Close";
            this.CloseButton.UseVisualStyleBackColor = true;
            this.CloseButton.Click += new System.EventHandler(this.CloseButton_Click);
            // 
            // OpenButton
            // 
            this.OpenButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.OpenButton.Location = new System.Drawing.Point(19, 58);
            this.OpenButton.Name = "OpenButton";
            this.OpenButton.Size = new System.Drawing.Size(65, 23);
            this.OpenButton.TabIndex = 5;
            this.OpenButton.Text = "Open...";
            this.OpenButton.UseVisualStyleBackColor = true;
            this.OpenButton.Click += new System.EventHandler(this.OpenButton_Click);
            // 
            // NewButton
            // 
            this.NewButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.NewButton.Location = new System.Drawing.Point(19, 24);
            this.NewButton.Name = "NewButton";
            this.NewButton.Size = new System.Drawing.Size(65, 23);
            this.NewButton.TabIndex = 4;
            this.NewButton.Text = "New...";
            this.NewButton.UseVisualStyleBackColor = true;
            this.NewButton.Click += new System.EventHandler(this.NewButton_Click);
            // 
            // SaveButton
            // 
            this.SaveButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.SaveButton.Enabled = false;
            this.SaveButton.Location = new System.Drawing.Point(19, 202);
            this.SaveButton.Name = "SaveButton";
            this.SaveButton.Size = new System.Drawing.Size(65, 23);
            this.SaveButton.TabIndex = 3;
            this.SaveButton.Text = "Save";
            this.SaveButton.UseVisualStyleBackColor = true;
            this.SaveButton.Click += new System.EventHandler(this.SaveButton_Click);
            // 
            // ModulationEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(687, 360);
            this.Controls.Add(this.RightPanel);
            this.Controls.Add(this.splitContainer);
            this.Name = "ModulationEditor";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "ModulationEditor";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ModulationEditor_FormClosing);
            this.splitContainer.Panel1.ResumeLayout(false);
            this.splitContainer.ResumeLayout(false);
            this.RightPanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer;
		private System.Windows.Forms.TreeView TreeView;
        private System.Windows.Forms.Panel RightPanel;
		private System.Windows.Forms.Button SaveButton;
        private System.Windows.Forms.Button CloseButton;
        protected System.Windows.Forms.Button OpenButton;
        protected System.Windows.Forms.Button NewButton;
        protected System.Windows.Forms.Button ModulateButton;
    }
}