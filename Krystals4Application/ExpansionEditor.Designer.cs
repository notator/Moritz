namespace Krystals5Application
{
    partial class ExpansionEditor
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
            if (disposing)
            {
                _bufferedGraphicsContext.Dispose();
                if(components != null)
                {
                    components.Dispose();
                }
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
            this.SaveButton = new System.Windows.Forms.Button();
            this.ZoomComboBox = new System.Windows.Forms.ComboBox();
            this.PercentLabel = new System.Windows.Forms.Label();
            this.ZoomLabel = new System.Windows.Forms.Label();
            this.FieldPanel = new System.Windows.Forms.Panel();
            this.StatusTextBox = new System.Windows.Forms.TextBox();
            this.InputSubpathsComboBox = new System.Windows.Forms.ComboBox();
            this.InputPlanetsComboBox = new System.Windows.Forms.ComboBox();
            this.FixedInputsComboBox = new System.Windows.Forms.ComboBox();
            this.OutputSubpathsComboBox = new System.Windows.Forms.ComboBox();
            this.OutputPlanetsComboBox = new System.Windows.Forms.ComboBox();
            this.FixedOutputsComboBox = new System.Windows.Forms.ComboBox();
            this.menuPanel = new System.Windows.Forms.Panel();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItemNew = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItemOpenKrystal = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.MenuItemLoadDensityInputKrystal = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItemLoadPointsInputKrystal = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
            this.MenuItemLoadExpander = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItemLoadInputGamete = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItemLoadOutputGamete = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.MenuItemSave = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItemSaveAs = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.MenuItemClose = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItemEdit = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem4 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.EditDeleteCurrentPointGroupToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.EditDeleteCurrentPlanetToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.editFixedInputPointsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editFixedOutputPointsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editInputPlanetToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editOutputPlanetToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ExpandButton = new System.Windows.Forms.Button();
            this.SplitContainer = new System.Windows.Forms.SplitContainer();
            this.TreeView = new System.Windows.Forms.TreeView();
            this.PointGroupParameters = new Krystals5ControlLibrary.PointGroupParameters();
            this.RadioButtonFixedInput = new System.Windows.Forms.RadioButton();
            this.RadioButtonFixedOutput = new System.Windows.Forms.RadioButton();
            this.RadioButtonInputPlanet = new System.Windows.Forms.RadioButton();
            this.RadioButtonOutputPlanet = new System.Windows.Forms.RadioButton();
            this.menuPanel.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.SplitContainer.Panel1.SuspendLayout();
            this.SplitContainer.Panel2.SuspendLayout();
            this.SplitContainer.SuspendLayout();
            this.SuspendLayout();
            // 
            // SaveButton
            // 
            this.SaveButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.SaveButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.SaveButton.Location = new System.Drawing.Point(420, 627);
            this.SaveButton.Name = "SaveButton";
            this.SaveButton.Size = new System.Drawing.Size(63, 23);
            this.SaveButton.TabIndex = 3;
            this.SaveButton.TabStop = false;
            this.SaveButton.Tag = "";
            this.SaveButton.Text = "Save...";
            this.SaveButton.Click += new System.EventHandler(this.SaveButton_Click);
            // 
            // ZoomComboBox
            // 
            this.ZoomComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ZoomComboBox.Enabled = false;
            this.ZoomComboBox.FormattingEnabled = true;
            this.ZoomComboBox.Items.AddRange(new object[] {
            "15",
            "30",
            "50",
            "75",
            "100",
            "125",
            "150",
            "175",
            "200"});
            this.ZoomComboBox.Location = new System.Drawing.Point(421, 560);
            this.ZoomComboBox.Name = "ZoomComboBox";
            this.ZoomComboBox.Size = new System.Drawing.Size(60, 21);
            this.ZoomComboBox.TabIndex = 102;
            this.ZoomComboBox.TabStop = false;
            this.ZoomComboBox.SelectedValueChanged += new System.EventHandler(this.ZoomComboBox_SelectedValueChanged);
            // 
            // PercentLabel
            // 
            this.PercentLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.PercentLabel.AutoSize = true;
            this.PercentLabel.Enabled = false;
            this.PercentLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.PercentLabel.Location = new System.Drawing.Point(483, 564);
            this.PercentLabel.Name = "PercentLabel";
            this.PercentLabel.Size = new System.Drawing.Size(15, 13);
            this.PercentLabel.TabIndex = 101;
            this.PercentLabel.Text = "%";
            // 
            // ZoomLabel
            // 
            this.ZoomLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ZoomLabel.AutoSize = true;
            this.ZoomLabel.Enabled = false;
            this.ZoomLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.ZoomLabel.Location = new System.Drawing.Point(392, 564);
            this.ZoomLabel.Name = "ZoomLabel";
            this.ZoomLabel.Size = new System.Drawing.Size(34, 13);
            this.ZoomLabel.TabIndex = 100;
            this.ZoomLabel.Text = "Zoom";
            // 
            // FieldPanel
            // 
            this.FieldPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.FieldPanel.BackColor = System.Drawing.Color.White;
            this.FieldPanel.Location = new System.Drawing.Point(0, 0);
            this.FieldPanel.Name = "FieldPanel";
            this.FieldPanel.Size = new System.Drawing.Size(281, 627);
            this.FieldPanel.TabIndex = 8;
            this.FieldPanel.Paint += new System.Windows.Forms.PaintEventHandler(this.FieldPanel_Paint);
            // 
            // StatusTextBox
            // 
            this.StatusTextBox.AllowDrop = true;
            this.StatusTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.StatusTextBox.BackColor = System.Drawing.Color.White;
            this.StatusTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.StatusTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.StatusTextBox.Location = new System.Drawing.Point(0, 630);
            this.StatusTextBox.Name = "StatusTextBox";
            this.StatusTextBox.Size = new System.Drawing.Size(376, 20);
            this.StatusTextBox.TabIndex = 110;
            this.StatusTextBox.TabStop = false;
            // 
            // InputSubpathsComboBox
            // 
            this.InputSubpathsComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.InputSubpathsComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.InputSubpathsComboBox.FormattingEnabled = true;
            this.InputSubpathsComboBox.Location = new System.Drawing.Point(394, 157);
            this.InputSubpathsComboBox.MaxDropDownItems = 25;
            this.InputSubpathsComboBox.Name = "InputSubpathsComboBox";
            this.InputSubpathsComboBox.Size = new System.Drawing.Size(115, 21);
            this.InputSubpathsComboBox.TabIndex = 0;
            this.InputSubpathsComboBox.SelectedIndexChanged += new System.EventHandler(this.InputSubpathComboBox_SelectedIndexChanged);
            // 
            // InputPlanetsComboBox
            // 
            this.InputPlanetsComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.InputPlanetsComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.InputPlanetsComboBox.FormattingEnabled = true;
            this.InputPlanetsComboBox.Location = new System.Drawing.Point(394, 128);
            this.InputPlanetsComboBox.MaxDropDownItems = 25;
            this.InputPlanetsComboBox.Name = "InputPlanetsComboBox";
            this.InputPlanetsComboBox.Size = new System.Drawing.Size(115, 21);
            this.InputPlanetsComboBox.TabIndex = 12;
            this.InputPlanetsComboBox.SelectedIndexChanged += new System.EventHandler(this.InputPlanetsComboBox_SelectedIndexChanged);
            // 
            // FixedInputsComboBox
            // 
            this.FixedInputsComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.FixedInputsComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.FixedInputsComboBox.FormattingEnabled = true;
            this.FixedInputsComboBox.Location = new System.Drawing.Point(394, 128);
            this.FixedInputsComboBox.MaxDropDownItems = 25;
            this.FixedInputsComboBox.Name = "FixedInputsComboBox";
            this.FixedInputsComboBox.Size = new System.Drawing.Size(115, 21);
            this.FixedInputsComboBox.TabIndex = 14;
            this.FixedInputsComboBox.SelectedIndexChanged += new System.EventHandler(this.FixedInputsComboBox_SelectedIndexChanged);
            // 
            // OutputSubpathsComboBox
            // 
            this.OutputSubpathsComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.OutputSubpathsComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.OutputSubpathsComboBox.FormattingEnabled = true;
            this.OutputSubpathsComboBox.Location = new System.Drawing.Point(394, 157);
            this.OutputSubpathsComboBox.MaxDropDownItems = 25;
            this.OutputSubpathsComboBox.Name = "OutputSubpathsComboBox";
            this.OutputSubpathsComboBox.Size = new System.Drawing.Size(115, 21);
            this.OutputSubpathsComboBox.TabIndex = 0;
            this.OutputSubpathsComboBox.SelectedIndexChanged += new System.EventHandler(this.OutputSubpathComboBox_SelectedIndexChanged);
            // 
            // OutputPlanetsComboBox
            // 
            this.OutputPlanetsComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.OutputPlanetsComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.OutputPlanetsComboBox.FormattingEnabled = true;
            this.OutputPlanetsComboBox.Location = new System.Drawing.Point(394, 128);
            this.OutputPlanetsComboBox.MaxDropDownItems = 25;
            this.OutputPlanetsComboBox.Name = "OutputPlanetsComboBox";
            this.OutputPlanetsComboBox.Size = new System.Drawing.Size(115, 21);
            this.OutputPlanetsComboBox.TabIndex = 13;
            this.OutputPlanetsComboBox.SelectedIndexChanged += new System.EventHandler(this.OutputPlanetsComboBox_SelectedIndexChanged);
            // 
            // FixedOutputsComboBox
            // 
            this.FixedOutputsComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.FixedOutputsComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.FixedOutputsComboBox.FormattingEnabled = true;
            this.FixedOutputsComboBox.Location = new System.Drawing.Point(394, 128);
            this.FixedOutputsComboBox.MaxDropDownItems = 25;
            this.FixedOutputsComboBox.Name = "FixedOutputsComboBox";
            this.FixedOutputsComboBox.Size = new System.Drawing.Size(115, 21);
            this.FixedOutputsComboBox.TabIndex = 13;
            this.FixedOutputsComboBox.SelectedIndexChanged += new System.EventHandler(this.FixedOutputsComboBox_SelectedIndexChanged);
            // 
            // menuPanel
            // 
            this.menuPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.menuPanel.Controls.Add(this.menuStrip1);
            this.menuPanel.Location = new System.Drawing.Point(376, 0);
            this.menuPanel.Name = "menuPanel";
            this.menuPanel.Size = new System.Drawing.Size(151, 24);
            this.menuPanel.TabIndex = 109;
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.MenuItemEdit});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(151, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MenuItemNew,
            this.MenuItemOpenKrystal,
            this.toolStripSeparator4,
            this.MenuItemLoadDensityInputKrystal,
            this.MenuItemLoadPointsInputKrystal,
            this.toolStripSeparator6,
            this.MenuItemLoadExpander,
            this.MenuItemLoadInputGamete,
            this.MenuItemLoadOutputGamete,
            this.toolStripSeparator5,
            this.MenuItemSave,
            this.MenuItemSaveAs,
            this.toolStripSeparator3,
            this.MenuItemClose});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(35, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // MenuItemNew
            // 
            this.MenuItemNew.Name = "MenuItemNew";
            this.MenuItemNew.Size = new System.Drawing.Size(209, 22);
            this.MenuItemNew.Text = "New...";
            this.MenuItemNew.Click += new System.EventHandler(this.MenuItemNew_Click);
            // 
            // MenuItemOpenKrystal
            // 
            this.MenuItemOpenKrystal.Name = "MenuItemOpenKrystal";
            this.MenuItemOpenKrystal.Size = new System.Drawing.Size(209, 22);
            this.MenuItemOpenKrystal.Text = "Open krystal...";
            this.MenuItemOpenKrystal.Click += new System.EventHandler(this.MenuItemOpenKrystal_Click);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(206, 6);
            // 
            // MenuItemLoadDensityInputKrystal
            // 
            this.MenuItemLoadDensityInputKrystal.Enabled = false;
            this.MenuItemLoadDensityInputKrystal.Name = "MenuItemLoadDensityInputKrystal";
            this.MenuItemLoadDensityInputKrystal.Size = new System.Drawing.Size(209, 22);
            this.MenuItemLoadDensityInputKrystal.Text = "Load density input krystal...";
            this.MenuItemLoadDensityInputKrystal.Click += new System.EventHandler(this.MenuItemLoadDensityInputKrystal_Click);
            // 
            // MenuItemLoadPointsInputKrystal
            // 
            this.MenuItemLoadPointsInputKrystal.Enabled = false;
            this.MenuItemLoadPointsInputKrystal.Name = "MenuItemLoadPointsInputKrystal";
            this.MenuItemLoadPointsInputKrystal.Size = new System.Drawing.Size(209, 22);
            this.MenuItemLoadPointsInputKrystal.Text = "Load points input krystal...";
            this.MenuItemLoadPointsInputKrystal.Click += new System.EventHandler(this.MenuItemLoadPointsInputKrystal_Click);
            // 
            // toolStripSeparator6
            // 
            this.toolStripSeparator6.Name = "toolStripSeparator6";
            this.toolStripSeparator6.Size = new System.Drawing.Size(206, 6);
            // 
            // MenuItemLoadExpander
            // 
            this.MenuItemLoadExpander.Enabled = false;
            this.MenuItemLoadExpander.Name = "MenuItemLoadExpander";
            this.MenuItemLoadExpander.Size = new System.Drawing.Size(209, 22);
            this.MenuItemLoadExpander.Text = "Load expander...";
            this.MenuItemLoadExpander.Click += new System.EventHandler(this.MenuItemLoadExpander_Click);
            // 
            // MenuItemLoadInputGamete
            // 
            this.MenuItemLoadInputGamete.Enabled = false;
            this.MenuItemLoadInputGamete.Name = "MenuItemLoadInputGamete";
            this.MenuItemLoadInputGamete.Size = new System.Drawing.Size(209, 22);
            this.MenuItemLoadInputGamete.Text = "Load input gamete...";
            this.MenuItemLoadInputGamete.Click += new System.EventHandler(this.MenuItemLoadInputGamete_Click);
            // 
            // MenuItemLoadOutputGamete
            // 
            this.MenuItemLoadOutputGamete.Enabled = false;
            this.MenuItemLoadOutputGamete.Name = "MenuItemLoadOutputGamete";
            this.MenuItemLoadOutputGamete.Size = new System.Drawing.Size(209, 22);
            this.MenuItemLoadOutputGamete.Text = "Load output gamete...";
            this.MenuItemLoadOutputGamete.Click += new System.EventHandler(this.MenuItemLoadOutputGamete_Click);
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            this.toolStripSeparator5.Size = new System.Drawing.Size(206, 6);
            // 
            // MenuItemSave
            // 
            this.MenuItemSave.Enabled = false;
            this.MenuItemSave.Name = "MenuItemSave";
            this.MenuItemSave.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
            this.MenuItemSave.Size = new System.Drawing.Size(209, 22);
            this.MenuItemSave.Text = "Save";
            this.MenuItemSave.Click += new System.EventHandler(this.MenuItemSave_Click);
            // 
            // MenuItemSaveAs
            // 
            this.MenuItemSaveAs.Enabled = false;
            this.MenuItemSaveAs.Name = "MenuItemSaveAs";
            this.MenuItemSaveAs.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift)
                        | System.Windows.Forms.Keys.S)));
            this.MenuItemSaveAs.Size = new System.Drawing.Size(209, 22);
            this.MenuItemSaveAs.Text = "Replace...";
            this.MenuItemSaveAs.Click += new System.EventHandler(this.MenuItemReplace_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(206, 6);
            // 
            // MenuItemClose
            // 
            this.MenuItemClose.Name = "MenuItemClose";
            this.MenuItemClose.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Q)));
            this.MenuItemClose.Size = new System.Drawing.Size(209, 22);
            this.MenuItemClose.Text = "Close";
            this.MenuItemClose.Click += new System.EventHandler(this.MenuItemClose_Click);
            // 
            // MenuItemEdit
            // 
            this.MenuItemEdit.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem1,
            this.toolStripMenuItem2,
            this.toolStripMenuItem3,
            this.toolStripMenuItem4,
            this.toolStripSeparator2,
            this.EditDeleteCurrentPointGroupToolStripMenuItem,
            this.EditDeleteCurrentPlanetToolStripMenuItem,
            this.toolStripSeparator1,
            this.editFixedInputPointsToolStripMenuItem,
            this.editFixedOutputPointsToolStripMenuItem,
            this.editInputPlanetToolStripMenuItem,
            this.editOutputPlanetToolStripMenuItem});
            this.MenuItemEdit.Enabled = false;
            this.MenuItemEdit.Name = "MenuItemEdit";
            this.MenuItemEdit.Size = new System.Drawing.Size(37, 20);
            this.MenuItemEdit.Text = "Edit";
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.I)));
            this.toolStripMenuItem1.Size = new System.Drawing.Size(253, 22);
            this.toolStripMenuItem1.Text = "New fixed input group";
            this.toolStripMenuItem1.Click += new System.EventHandler(this.MenuItemNewFixedInputGroup_Click);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
            this.toolStripMenuItem2.Size = new System.Drawing.Size(253, 22);
            this.toolStripMenuItem2.Text = "New fixed output group";
            this.toolStripMenuItem2.Click += new System.EventHandler(this.MenuItemNewFixedOutputsGroup_Click);
            // 
            // toolStripMenuItem3
            // 
            this.toolStripMenuItem3.Name = "toolStripMenuItem3";
            this.toolStripMenuItem3.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift)
                        | System.Windows.Forms.Keys.I)));
            this.toolStripMenuItem3.Size = new System.Drawing.Size(253, 22);
            this.toolStripMenuItem3.Text = "New input planet";
            this.toolStripMenuItem3.Click += new System.EventHandler(this.MenuItemNewInputPlanet_Click);
            // 
            // toolStripMenuItem4
            // 
            this.toolStripMenuItem4.Name = "toolStripMenuItem4";
            this.toolStripMenuItem4.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift)
                        | System.Windows.Forms.Keys.O)));
            this.toolStripMenuItem4.Size = new System.Drawing.Size(253, 22);
            this.toolStripMenuItem4.Text = "New output planet";
            this.toolStripMenuItem4.Click += new System.EventHandler(this.MenuItemNewOutputPlanet_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(250, 6);
            // 
            // EditDeleteCurrentPointGroupToolStripMenuItem
            // 
            this.EditDeleteCurrentPointGroupToolStripMenuItem.Name = "EditDeleteCurrentPointGroupToolStripMenuItem";
            this.EditDeleteCurrentPointGroupToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Delete)));
            this.EditDeleteCurrentPointGroupToolStripMenuItem.Size = new System.Drawing.Size(253, 22);
            this.EditDeleteCurrentPointGroupToolStripMenuItem.Text = "Delete current point group";
            this.EditDeleteCurrentPointGroupToolStripMenuItem.Click += new System.EventHandler(this.MenuItemDeleteCurrentPointGroup_Click);
            // 
            // EditDeleteCurrentPlanetToolStripMenuItem
            // 
            this.EditDeleteCurrentPlanetToolStripMenuItem.Name = "EditDeleteCurrentPlanetToolStripMenuItem";
            this.EditDeleteCurrentPlanetToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift)
                        | System.Windows.Forms.Keys.Delete)));
            this.EditDeleteCurrentPlanetToolStripMenuItem.Size = new System.Drawing.Size(253, 22);
            this.EditDeleteCurrentPlanetToolStripMenuItem.Text = "Delete current planet";
            this.EditDeleteCurrentPlanetToolStripMenuItem.Click += new System.EventHandler(this.MenuItemDeleteCurrentPlanet_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(250, 6);
            // 
            // editFixedInputPointsToolStripMenuItem
            // 
            this.editFixedInputPointsToolStripMenuItem.Name = "editFixedInputPointsToolStripMenuItem";
            this.editFixedInputPointsToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.I)));
            this.editFixedInputPointsToolStripMenuItem.Size = new System.Drawing.Size(253, 22);
            this.editFixedInputPointsToolStripMenuItem.Text = "Edit fixed input points";
            this.editFixedInputPointsToolStripMenuItem.Click += new System.EventHandler(this.MenuItemEditFixedInputPoints_Click);
            // 
            // editFixedOutputPointsToolStripMenuItem
            // 
            this.editFixedOutputPointsToolStripMenuItem.Name = "editFixedOutputPointsToolStripMenuItem";
            this.editFixedOutputPointsToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.O)));
            this.editFixedOutputPointsToolStripMenuItem.Size = new System.Drawing.Size(253, 22);
            this.editFixedOutputPointsToolStripMenuItem.Text = "Edit fixed output points";
            this.editFixedOutputPointsToolStripMenuItem.Click += new System.EventHandler(this.MenuItemEditFixedOutputPoints_Click);
            // 
            // editInputPlanetToolStripMenuItem
            // 
            this.editInputPlanetToolStripMenuItem.Name = "editInputPlanetToolStripMenuItem";
            this.editInputPlanetToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.Shift)
                        | System.Windows.Forms.Keys.I)));
            this.editInputPlanetToolStripMenuItem.Size = new System.Drawing.Size(253, 22);
            this.editInputPlanetToolStripMenuItem.Text = "Edit input planet";
            this.editInputPlanetToolStripMenuItem.Click += new System.EventHandler(this.MenuItemEditInputPlanets_Click);
            // 
            // editOutputPlanetToolStripMenuItem
            // 
            this.editOutputPlanetToolStripMenuItem.Name = "editOutputPlanetToolStripMenuItem";
            this.editOutputPlanetToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.Shift)
                        | System.Windows.Forms.Keys.O)));
            this.editOutputPlanetToolStripMenuItem.Size = new System.Drawing.Size(253, 22);
            this.editOutputPlanetToolStripMenuItem.Text = "Edit output planet";
            this.editOutputPlanetToolStripMenuItem.Click += new System.EventHandler(this.MenuItemEditOutputPlanets_Click);
            // 
            // ExpandButton
            // 
            this.ExpandButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ExpandButton.Enabled = false;
            this.ExpandButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.ExpandButton.Location = new System.Drawing.Point(420, 594);
            this.ExpandButton.Name = "ExpandButton";
            this.ExpandButton.Size = new System.Drawing.Size(63, 23);
            this.ExpandButton.TabIndex = 111;
            this.ExpandButton.TabStop = false;
            this.ExpandButton.Tag = "";
            this.ExpandButton.Text = "Expand";
            this.ExpandButton.Click += new System.EventHandler(this.ExpandButton_Click);
            // 
            // SplitContainer
            // 
            this.SplitContainer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.SplitContainer.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.SplitContainer.Location = new System.Drawing.Point(0, 0);
            this.SplitContainer.Name = "SplitContainer";
            // 
            // SplitContainer.Panel1
            // 
            this.SplitContainer.Panel1.Controls.Add(this.TreeView);
            // 
            // SplitContainer.Panel2
            // 
            this.SplitContainer.Panel2.Controls.Add(this.FieldPanel);
            this.SplitContainer.Size = new System.Drawing.Size(376, 627);
            this.SplitContainer.SplitterDistance = 89;
            this.SplitContainer.TabIndex = 112;
            this.SplitContainer.TabStop = false;
            this.SplitContainer.SplitterMoved += new System.Windows.Forms.SplitterEventHandler(this.SplitContainer_SplitterMoved);
            // 
            // TreeView
            // 
            this.TreeView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.TreeView.HideSelection = false;
            this.TreeView.Location = new System.Drawing.Point(0, 0);
            this.TreeView.Name = "TreeView";
            this.TreeView.Size = new System.Drawing.Size(87, 627);
            this.TreeView.TabIndex = 0;
            this.TreeView.TabStop = false;
            this.TreeView.NodeMouseClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.TreeView_NodeMouseClick);
            // 
            // PointGroupParameters
            // 
            this.PointGroupParameters.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.PointGroupParameters.Busy = false;
            this.PointGroupParameters.Enabled = false;
            this.PointGroupParameters.Location = new System.Drawing.Point(382, 191);
            this.PointGroupParameters.Name = "PointGroupParameters";
            this.PointGroupParameters.Size = new System.Drawing.Size(138, 352);
            this.PointGroupParameters.TabIndex = 1;
            // 
            // RadioButtonFixedInput
            // 
            this.RadioButtonFixedInput.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.RadioButtonFixedInput.AutoSize = true;
            this.RadioButtonFixedInput.Location = new System.Drawing.Point(410, 33);
            this.RadioButtonFixedInput.Name = "RadioButtonFixedInput";
            this.RadioButtonFixedInput.Size = new System.Drawing.Size(73, 17);
            this.RadioButtonFixedInput.TabIndex = 117;
            this.RadioButtonFixedInput.TabStop = true;
            this.RadioButtonFixedInput.Text = "fixed input";
            this.RadioButtonFixedInput.UseVisualStyleBackColor = true;
            this.RadioButtonFixedInput.Click += new System.EventHandler(this.RadioButtonFixedInput_Click);
            // 
            // RadioButtonFixedOutput
            // 
            this.RadioButtonFixedOutput.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.RadioButtonFixedOutput.AutoSize = true;
            this.RadioButtonFixedOutput.Location = new System.Drawing.Point(410, 55);
            this.RadioButtonFixedOutput.Name = "RadioButtonFixedOutput";
            this.RadioButtonFixedOutput.Size = new System.Drawing.Size(80, 17);
            this.RadioButtonFixedOutput.TabIndex = 118;
            this.RadioButtonFixedOutput.TabStop = true;
            this.RadioButtonFixedOutput.Text = "fixed output";
            this.RadioButtonFixedOutput.UseVisualStyleBackColor = true;
            this.RadioButtonFixedOutput.Click += new System.EventHandler(this.RadioButtonFixedOutput_Click);
            // 
            // RadioButtonInputPlanet
            // 
            this.RadioButtonInputPlanet.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.RadioButtonInputPlanet.AutoSize = true;
            this.RadioButtonInputPlanet.Location = new System.Drawing.Point(410, 77);
            this.RadioButtonInputPlanet.Name = "RadioButtonInputPlanet";
            this.RadioButtonInputPlanet.Size = new System.Drawing.Size(80, 17);
            this.RadioButtonInputPlanet.TabIndex = 119;
            this.RadioButtonInputPlanet.TabStop = true;
            this.RadioButtonInputPlanet.Text = "input planet";
            this.RadioButtonInputPlanet.UseVisualStyleBackColor = true;
            this.RadioButtonInputPlanet.Click += new System.EventHandler(this.RadioButtonInputPlanet_Click);
            // 
            // RadioButtonOutputPlanet
            // 
            this.RadioButtonOutputPlanet.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.RadioButtonOutputPlanet.AutoSize = true;
            this.RadioButtonOutputPlanet.Location = new System.Drawing.Point(410, 99);
            this.RadioButtonOutputPlanet.Name = "RadioButtonOutputPlanet";
            this.RadioButtonOutputPlanet.Size = new System.Drawing.Size(87, 17);
            this.RadioButtonOutputPlanet.TabIndex = 120;
            this.RadioButtonOutputPlanet.TabStop = true;
            this.RadioButtonOutputPlanet.Text = "output planet";
            this.RadioButtonOutputPlanet.UseVisualStyleBackColor = true;
            this.RadioButtonOutputPlanet.Click += new System.EventHandler(this.RadioButtonOutputPlanet_Click);
            // 
            // ExpansionEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ControlLight;
            this.ClientSize = new System.Drawing.Size(528, 650);
            this.Controls.Add(this.RadioButtonOutputPlanet);
            this.Controls.Add(this.RadioButtonInputPlanet);
            this.Controls.Add(this.RadioButtonFixedOutput);
            this.Controls.Add(this.RadioButtonFixedInput);
            this.Controls.Add(this.SplitContainer);
            this.Controls.Add(this.ExpandButton);
            this.Controls.Add(this.StatusTextBox);
            this.Controls.Add(this.InputSubpathsComboBox);
            this.Controls.Add(this.menuPanel);
            this.Controls.Add(this.PointGroupParameters);
            this.Controls.Add(this.ZoomComboBox);
            this.Controls.Add(this.PercentLabel);
            this.Controls.Add(this.ZoomLabel);
            this.Controls.Add(this.SaveButton);
            this.Controls.Add(this.FixedInputsComboBox);
            this.Controls.Add(this.FixedOutputsComboBox);
            this.Controls.Add(this.InputPlanetsComboBox);
            this.Controls.Add(this.OutputPlanetsComboBox);
            this.Controls.Add(this.OutputSubpathsComboBox);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "ExpansionEditor";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "editor";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.SizeChanged += new System.EventHandler(this.FieldEditorWindow_SizeChanged);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FieldEditorWindow_FormClosing);
            this.menuPanel.ResumeLayout(false);
            this.menuPanel.PerformLayout();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.SplitContainer.Panel1.ResumeLayout(false);
            this.SplitContainer.Panel2.ResumeLayout(false);
            this.SplitContainer.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button SaveButton;
		private System.Windows.Forms.ComboBox ZoomComboBox;
		private System.Windows.Forms.Label PercentLabel;
		private System.Windows.Forms.Label ZoomLabel;
        private System.Windows.Forms.Panel FieldPanel;
		private System.Windows.Forms.ComboBox OutputPlanetsComboBox;
        private System.Windows.Forms.ComboBox OutputSubpathsComboBox;
        private System.Windows.Forms.ComboBox FixedOutputsComboBox;
        private System.Windows.Forms.ComboBox FixedInputsComboBox;
		private System.Windows.Forms.ComboBox InputSubpathsComboBox;
        private System.Windows.Forms.ComboBox InputPlanetsComboBox;
        private Krystals5ControlLibrary.PointGroupParameters PointGroupParameters;
        private System.Windows.Forms.Panel menuPanel;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem MenuItemSave;
        private System.Windows.Forms.ToolStripMenuItem MenuItemSaveAs;
        private System.Windows.Forms.ToolStripMenuItem MenuItemClose;
        private System.Windows.Forms.ToolStripMenuItem MenuItemEdit;
        private System.Windows.Forms.ToolStripMenuItem editFixedInputPointsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem editFixedOutputPointsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem editInputPlanetToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem editOutputPlanetToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.TextBox StatusTextBox;
        private System.Windows.Forms.Button ExpandButton;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem3;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem4;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.SplitContainer SplitContainer;
        private System.Windows.Forms.TreeView TreeView;
        private System.Windows.Forms.ToolStripMenuItem EditDeleteCurrentPointGroupToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem EditDeleteCurrentPlanetToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripMenuItem MenuItemLoadExpander;
        private System.Windows.Forms.ToolStripMenuItem MenuItemLoadInputGamete;
        private System.Windows.Forms.ToolStripMenuItem MenuItemLoadOutputGamete;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator6;
        private System.Windows.Forms.RadioButton RadioButtonFixedInput;
        private System.Windows.Forms.RadioButton RadioButtonFixedOutput;
        private System.Windows.Forms.RadioButton RadioButtonInputPlanet;
        private System.Windows.Forms.RadioButton RadioButtonOutputPlanet;
        protected System.Windows.Forms.ToolStripMenuItem MenuItemNew;
        protected System.Windows.Forms.ToolStripMenuItem MenuItemOpenKrystal;
        protected System.Windows.Forms.ToolStripMenuItem MenuItemLoadDensityInputKrystal;
        protected System.Windows.Forms.ToolStripMenuItem MenuItemLoadPointsInputKrystal;

    }
}