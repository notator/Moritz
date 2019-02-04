namespace Moritz.Composer
{
    partial class DimensionsAndMetadataForm
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
			this.MetadataGroupBox = new System.Windows.Forms.GroupBox();
			this.MetadataCommentTextBox = new System.Windows.Forms.TextBox();
			this.MetadataCommentLabel = new System.Windows.Forms.Label();
			this.MetadataKeywordsTextBox = new System.Windows.Forms.TextBox();
			this.MetadataKeywordsLabel = new System.Windows.Forms.Label();
			this.ConfirmButton = new System.Windows.Forms.Button();
			this.LandscapeCheckBox = new System.Windows.Forms.CheckBox();
			this.PaperSizeComboBox = new System.Windows.Forms.ComboBox();
			this.WebsiteLinksGroupBox = new System.Windows.Forms.GroupBox();
			this.AboutLinkTextTextBox = new System.Windows.Forms.TextBox();
			this.AboutLinkURLTextBox = new System.Windows.Forms.TextBox();
			this.AboutLinkTextLabel = new System.Windows.Forms.Label();
			this.AboutLinkURLLabel = new System.Windows.Forms.Label();
			this.MarginsGroupBox = new System.Windows.Forms.GroupBox();
			this.BottomMarginTextBox = new System.Windows.Forms.TextBox();
			this.TopMarginPage1TextBox = new System.Windows.Forms.TextBox();
			this.TopMarginOtherPagesTextBox = new System.Windows.Forms.TextBox();
			this.RightMarginTextBox = new System.Windows.Forms.TextBox();
			this.LeftMarginTextBox = new System.Windows.Forms.TextBox();
			this.Page1AboveTopSystemLabel = new System.Windows.Forms.Label();
			this.AllPagesRightMarginLabel = new System.Windows.Forms.Label();
			this.OtherPagesAboveTopSystemLabel = new System.Windows.Forms.Label();
			this.AllPagesLeftMarginLabel = new System.Windows.Forms.Label();
			this.AllPagesBottomMarginLabel = new System.Windows.Forms.Label();
			this.Page1TitleGroupBox = new System.Windows.Forms.GroupBox();
			this.authorOverrideLabel = new System.Windows.Forms.Label();
			this.titleOverrideLabel = new System.Windows.Forms.Label();
			this.Page1TitleTextBox = new System.Windows.Forms.TextBox();
			this.Page1AuthorTextBox = new System.Windows.Forms.TextBox();
			this.Page1TitleYTextBox = new System.Windows.Forms.TextBox();
			this.Page1TitleHeightTextBox = new System.Windows.Forms.TextBox();
			this.Page1AuthorHeightTextBox = new System.Windows.Forms.TextBox();
			this.Page1TitleYLabel = new System.Windows.Forms.Label();
			this.Page1TitleHeightLabel = new System.Windows.Forms.Label();
			this.Page1AuthorHeightLabel = new System.Windows.Forms.Label();
			this.PaperSizeLabel = new System.Windows.Forms.Label();
			this.ShowMainScoreFormButton = new System.Windows.Forms.Button();
			this.UnitsHelpLabel = new System.Windows.Forms.Label();
			this.RevertToSavedButton = new System.Windows.Forms.Button();
			this.MetadataGroupBox.SuspendLayout();
			this.WebsiteLinksGroupBox.SuspendLayout();
			this.MarginsGroupBox.SuspendLayout();
			this.Page1TitleGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// MetadataGroupBox
			// 
			this.MetadataGroupBox.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.MetadataGroupBox.Controls.Add(this.MetadataCommentTextBox);
			this.MetadataGroupBox.Controls.Add(this.MetadataCommentLabel);
			this.MetadataGroupBox.Controls.Add(this.MetadataKeywordsTextBox);
			this.MetadataGroupBox.Controls.Add(this.MetadataKeywordsLabel);
			this.MetadataGroupBox.ForeColor = System.Drawing.Color.Brown;
			this.MetadataGroupBox.Location = new System.Drawing.Point(347, 75);
			this.MetadataGroupBox.Name = "MetadataGroupBox";
			this.MetadataGroupBox.Size = new System.Drawing.Size(293, 259);
			this.MetadataGroupBox.TabIndex = 9;
			this.MetadataGroupBox.TabStop = false;
			this.MetadataGroupBox.Text = "metadata";
			// 
			// MetadataCommentTextBox
			// 
			this.MetadataCommentTextBox.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.MetadataCommentTextBox.Location = new System.Drawing.Point(11, 63);
			this.MetadataCommentTextBox.Multiline = true;
			this.MetadataCommentTextBox.Name = "MetadataCommentTextBox";
			this.MetadataCommentTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.MetadataCommentTextBox.Size = new System.Drawing.Size(268, 184);
			this.MetadataCommentTextBox.TabIndex = 1;
			this.MetadataCommentTextBox.TextChanged += new System.EventHandler(this.SetToWhiteTextBox_TextChanged);
			this.MetadataCommentTextBox.Leave += new System.EventHandler(this.MetadataCommentTextBox_Leave);
			// 
			// MetadataCommentLabel
			// 
			this.MetadataCommentLabel.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.MetadataCommentLabel.AutoSize = true;
			this.MetadataCommentLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(60)))), ((int)(((byte)(60)))));
			this.MetadataCommentLabel.Location = new System.Drawing.Point(8, 45);
			this.MetadataCommentLabel.Margin = new System.Windows.Forms.Padding(0);
			this.MetadataCommentLabel.Name = "MetadataCommentLabel";
			this.MetadataCommentLabel.Size = new System.Drawing.Size(50, 14);
			this.MetadataCommentLabel.TabIndex = 137;
			this.MetadataCommentLabel.Text = "comment";
			// 
			// MetadataKeywordsTextBox
			// 
			this.MetadataKeywordsTextBox.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.MetadataKeywordsTextBox.Location = new System.Drawing.Point(66, 21);
			this.MetadataKeywordsTextBox.Name = "MetadataKeywordsTextBox";
			this.MetadataKeywordsTextBox.Size = new System.Drawing.Size(213, 20);
			this.MetadataKeywordsTextBox.TabIndex = 0;
			this.MetadataKeywordsTextBox.TextChanged += new System.EventHandler(this.SetToWhiteTextBox_TextChanged);
			this.MetadataKeywordsTextBox.Leave += new System.EventHandler(this.MetadataKeywordsTextBox_Leave);
			// 
			// MetadataKeywordsLabel
			// 
			this.MetadataKeywordsLabel.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.MetadataKeywordsLabel.AutoSize = true;
			this.MetadataKeywordsLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(60)))), ((int)(((byte)(60)))));
			this.MetadataKeywordsLabel.Location = new System.Drawing.Point(8, 25);
			this.MetadataKeywordsLabel.Margin = new System.Windows.Forms.Padding(0);
			this.MetadataKeywordsLabel.Name = "MetadataKeywordsLabel";
			this.MetadataKeywordsLabel.Size = new System.Drawing.Size(56, 14);
			this.MetadataKeywordsLabel.TabIndex = 135;
			this.MetadataKeywordsLabel.Text = "keywords";
			// 
			// ConfirmButton
			// 
			this.ConfirmButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.ConfirmButton.BackColor = System.Drawing.Color.Transparent;
			this.ConfirmButton.Enabled = false;
			this.ConfirmButton.Font = new System.Drawing.Font("Arial", 8F);
			this.ConfirmButton.ForeColor = System.Drawing.Color.Blue;
			this.ConfirmButton.Location = new System.Drawing.Point(503, 421);
			this.ConfirmButton.Name = "ConfirmButton";
			this.ConfirmButton.Size = new System.Drawing.Size(137, 26);
			this.ConfirmButton.TabIndex = 1;
			this.ConfirmButton.Text = "confirm";
			this.ConfirmButton.UseVisualStyleBackColor = false;
			this.ConfirmButton.Click += new System.EventHandler(this.ConfirmButton_Click);
			// 
			// LandscapeCheckBox
			// 
			this.LandscapeCheckBox.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.LandscapeCheckBox.AutoSize = true;
			this.LandscapeCheckBox.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.LandscapeCheckBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(60)))), ((int)(((byte)(60)))));
			this.LandscapeCheckBox.Location = new System.Drawing.Point(209, 19);
			this.LandscapeCheckBox.Name = "LandscapeCheckBox";
			this.LandscapeCheckBox.Size = new System.Drawing.Size(76, 18);
			this.LandscapeCheckBox.TabIndex = 5;
			this.LandscapeCheckBox.Text = "landscape";
			this.LandscapeCheckBox.UseVisualStyleBackColor = true;
			this.LandscapeCheckBox.CheckedChanged += new System.EventHandler(this.LandscapeCheckBox_CheckedChanged);
			// 
			// PaperSizeComboBox
			// 
			this.PaperSizeComboBox.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.PaperSizeComboBox.FormattingEnabled = true;
			this.PaperSizeComboBox.Items.AddRange(new object[] {
            "A4",
            "B4",
            "A5",
            "B5",
            "A3",
            "Letter",
            "Legal",
            "Tabloid"});
			this.PaperSizeComboBox.Location = new System.Drawing.Point(122, 16);
			this.PaperSizeComboBox.Name = "PaperSizeComboBox";
			this.PaperSizeComboBox.Size = new System.Drawing.Size(56, 22);
			this.PaperSizeComboBox.TabIndex = 4;
			this.PaperSizeComboBox.SelectedIndexChanged += new System.EventHandler(this.PaperSizeComboBox_SelectedIndexChanged);
			// 
			// WebsiteLinksGroupBox
			// 
			this.WebsiteLinksGroupBox.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.WebsiteLinksGroupBox.Controls.Add(this.AboutLinkTextTextBox);
			this.WebsiteLinksGroupBox.Controls.Add(this.AboutLinkURLTextBox);
			this.WebsiteLinksGroupBox.Controls.Add(this.AboutLinkTextLabel);
			this.WebsiteLinksGroupBox.Controls.Add(this.AboutLinkURLLabel);
			this.WebsiteLinksGroupBox.ForeColor = System.Drawing.Color.Brown;
			this.WebsiteLinksGroupBox.Location = new System.Drawing.Point(19, 340);
			this.WebsiteLinksGroupBox.Name = "WebsiteLinksGroupBox";
			this.WebsiteLinksGroupBox.Size = new System.Drawing.Size(621, 67);
			this.WebsiteLinksGroupBox.TabIndex = 8;
			this.WebsiteLinksGroupBox.TabStop = false;
			this.WebsiteLinksGroupBox.Text = "website links";
			// 
			// AboutLinkTextTextBox
			// 
			this.AboutLinkTextTextBox.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.AboutLinkTextTextBox.Location = new System.Drawing.Point(11, 34);
			this.AboutLinkTextTextBox.Name = "AboutLinkTextTextBox";
			this.AboutLinkTextTextBox.Size = new System.Drawing.Size(95, 20);
			this.AboutLinkTextTextBox.TabIndex = 0;
			this.AboutLinkTextTextBox.TextChanged += new System.EventHandler(this.SetToWhiteTextBox_TextChanged);
			this.AboutLinkTextTextBox.Leave += new System.EventHandler(this.AboutLinkTextTextBox_Leave);
			// 
			// AboutLinkURLTextBox
			// 
			this.AboutLinkURLTextBox.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.AboutLinkURLTextBox.Location = new System.Drawing.Point(113, 34);
			this.AboutLinkURLTextBox.Name = "AboutLinkURLTextBox";
			this.AboutLinkURLTextBox.Size = new System.Drawing.Size(494, 20);
			this.AboutLinkURLTextBox.TabIndex = 1;
			this.AboutLinkURLTextBox.TextChanged += new System.EventHandler(this.SetToWhiteTextBox_TextChanged);
			this.AboutLinkURLTextBox.Leave += new System.EventHandler(this.AboutLinkURLTextBox_Leave);
			// 
			// AboutLinkTextLabel
			// 
			this.AboutLinkTextLabel.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.AboutLinkTextLabel.AutoSize = true;
			this.AboutLinkTextLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(60)))), ((int)(((byte)(60)))));
			this.AboutLinkTextLabel.Location = new System.Drawing.Point(8, 15);
			this.AboutLinkTextLabel.Margin = new System.Windows.Forms.Padding(0);
			this.AboutLinkTextLabel.Name = "AboutLinkTextLabel";
			this.AboutLinkTextLabel.Size = new System.Drawing.Size(73, 14);
			this.AboutLinkTextLabel.TabIndex = 91;
			this.AboutLinkTextLabel.Text = "about link text";
			// 
			// AboutLinkURLLabel
			// 
			this.AboutLinkURLLabel.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.AboutLinkURLLabel.AutoSize = true;
			this.AboutLinkURLLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(60)))), ((int)(((byte)(60)))));
			this.AboutLinkURLLabel.Location = new System.Drawing.Point(110, 15);
			this.AboutLinkURLLabel.Margin = new System.Windows.Forms.Padding(0);
			this.AboutLinkURLLabel.Name = "AboutLinkURLLabel";
			this.AboutLinkURLLabel.Size = new System.Drawing.Size(75, 14);
			this.AboutLinkURLLabel.TabIndex = 95;
			this.AboutLinkURLLabel.Text = "about link URL";
			// 
			// MarginsGroupBox
			// 
			this.MarginsGroupBox.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.MarginsGroupBox.Controls.Add(this.BottomMarginTextBox);
			this.MarginsGroupBox.Controls.Add(this.TopMarginPage1TextBox);
			this.MarginsGroupBox.Controls.Add(this.TopMarginOtherPagesTextBox);
			this.MarginsGroupBox.Controls.Add(this.RightMarginTextBox);
			this.MarginsGroupBox.Controls.Add(this.LeftMarginTextBox);
			this.MarginsGroupBox.Controls.Add(this.Page1AboveTopSystemLabel);
			this.MarginsGroupBox.Controls.Add(this.AllPagesRightMarginLabel);
			this.MarginsGroupBox.Controls.Add(this.OtherPagesAboveTopSystemLabel);
			this.MarginsGroupBox.Controls.Add(this.AllPagesLeftMarginLabel);
			this.MarginsGroupBox.Controls.Add(this.AllPagesBottomMarginLabel);
			this.MarginsGroupBox.ForeColor = System.Drawing.Color.Brown;
			this.MarginsGroupBox.Location = new System.Drawing.Point(19, 218);
			this.MarginsGroupBox.Name = "MarginsGroupBox";
			this.MarginsGroupBox.Size = new System.Drawing.Size(315, 116);
			this.MarginsGroupBox.TabIndex = 7;
			this.MarginsGroupBox.TabStop = false;
			this.MarginsGroupBox.Text = "margins";
			// 
			// BottomMarginTextBox
			// 
			this.BottomMarginTextBox.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.BottomMarginTextBox.Location = new System.Drawing.Point(149, 84);
			this.BottomMarginTextBox.Name = "BottomMarginTextBox";
			this.BottomMarginTextBox.Size = new System.Drawing.Size(34, 20);
			this.BottomMarginTextBox.TabIndex = 4;
			this.BottomMarginTextBox.TextChanged += new System.EventHandler(this.SetToWhiteTextBox_TextChanged);
			this.BottomMarginTextBox.Leave += new System.EventHandler(this.BottomMarginTextBox_Leave);
			// 
			// TopMarginPage1TextBox
			// 
			this.TopMarginPage1TextBox.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.TopMarginPage1TextBox.Location = new System.Drawing.Point(90, 22);
			this.TopMarginPage1TextBox.Name = "TopMarginPage1TextBox";
			this.TopMarginPage1TextBox.Size = new System.Drawing.Size(34, 20);
			this.TopMarginPage1TextBox.TabIndex = 0;
			this.TopMarginPage1TextBox.TextChanged += new System.EventHandler(this.SetToWhiteTextBox_TextChanged);
			this.TopMarginPage1TextBox.Leave += new System.EventHandler(this.TopMarginPage1TextBox_Leave);
			// 
			// TopMarginOtherPagesTextBox
			// 
			this.TopMarginOtherPagesTextBox.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.TopMarginOtherPagesTextBox.Location = new System.Drawing.Point(219, 22);
			this.TopMarginOtherPagesTextBox.Name = "TopMarginOtherPagesTextBox";
			this.TopMarginOtherPagesTextBox.Size = new System.Drawing.Size(34, 20);
			this.TopMarginOtherPagesTextBox.TabIndex = 1;
			this.TopMarginOtherPagesTextBox.TextChanged += new System.EventHandler(this.SetToWhiteTextBox_TextChanged);
			this.TopMarginOtherPagesTextBox.Leave += new System.EventHandler(this.TopMarginOtherPagesTextBox_Leave);
			// 
			// RightMarginTextBox
			// 
			this.RightMarginTextBox.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.RightMarginTextBox.Location = new System.Drawing.Point(252, 55);
			this.RightMarginTextBox.Name = "RightMarginTextBox";
			this.RightMarginTextBox.Size = new System.Drawing.Size(34, 20);
			this.RightMarginTextBox.TabIndex = 3;
			this.RightMarginTextBox.TextChanged += new System.EventHandler(this.SetToWhiteTextBox_TextChanged);
			this.RightMarginTextBox.Leave += new System.EventHandler(this.RightMarginTextBox_Leave);
			// 
			// LeftMarginTextBox
			// 
			this.LeftMarginTextBox.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.LeftMarginTextBox.Location = new System.Drawing.Point(56, 55);
			this.LeftMarginTextBox.Name = "LeftMarginTextBox";
			this.LeftMarginTextBox.Size = new System.Drawing.Size(34, 20);
			this.LeftMarginTextBox.TabIndex = 2;
			this.LeftMarginTextBox.TextChanged += new System.EventHandler(this.SetToWhiteTextBox_TextChanged);
			this.LeftMarginTextBox.Leave += new System.EventHandler(this.LeftMarginTextBox_Leave);
			// 
			// Page1AboveTopSystemLabel
			// 
			this.Page1AboveTopSystemLabel.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.Page1AboveTopSystemLabel.AutoSize = true;
			this.Page1AboveTopSystemLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(60)))), ((int)(((byte)(60)))));
			this.Page1AboveTopSystemLabel.Location = new System.Drawing.Point(29, 25);
			this.Page1AboveTopSystemLabel.Margin = new System.Windows.Forms.Padding(0);
			this.Page1AboveTopSystemLabel.Name = "Page1AboveTopSystemLabel";
			this.Page1AboveTopSystemLabel.Size = new System.Drawing.Size(58, 14);
			this.Page1AboveTopSystemLabel.TabIndex = 99;
			this.Page1AboveTopSystemLabel.Text = "page 1 top";
			// 
			// AllPagesRightMarginLabel
			// 
			this.AllPagesRightMarginLabel.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.AllPagesRightMarginLabel.AutoSize = true;
			this.AllPagesRightMarginLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(60)))), ((int)(((byte)(60)))));
			this.AllPagesRightMarginLabel.Location = new System.Drawing.Point(221, 58);
			this.AllPagesRightMarginLabel.Margin = new System.Windows.Forms.Padding(0);
			this.AllPagesRightMarginLabel.Name = "AllPagesRightMarginLabel";
			this.AllPagesRightMarginLabel.Size = new System.Drawing.Size(28, 14);
			this.AllPagesRightMarginLabel.TabIndex = 116;
			this.AllPagesRightMarginLabel.Text = "right";
			// 
			// OtherPagesAboveTopSystemLabel
			// 
			this.OtherPagesAboveTopSystemLabel.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.OtherPagesAboveTopSystemLabel.AutoSize = true;
			this.OtherPagesAboveTopSystemLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(60)))), ((int)(((byte)(60)))));
			this.OtherPagesAboveTopSystemLabel.Location = new System.Drawing.Point(133, 25);
			this.OtherPagesAboveTopSystemLabel.Margin = new System.Windows.Forms.Padding(0);
			this.OtherPagesAboveTopSystemLabel.Name = "OtherPagesAboveTopSystemLabel";
			this.OtherPagesAboveTopSystemLabel.Size = new System.Drawing.Size(83, 14);
			this.OtherPagesAboveTopSystemLabel.TabIndex = 101;
			this.OtherPagesAboveTopSystemLabel.Text = "other pages top";
			// 
			// AllPagesLeftMarginLabel
			// 
			this.AllPagesLeftMarginLabel.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.AllPagesLeftMarginLabel.AutoSize = true;
			this.AllPagesLeftMarginLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(60)))), ((int)(((byte)(60)))));
			this.AllPagesLeftMarginLabel.Location = new System.Drawing.Point(28, 58);
			this.AllPagesLeftMarginLabel.Margin = new System.Windows.Forms.Padding(0);
			this.AllPagesLeftMarginLabel.Name = "AllPagesLeftMarginLabel";
			this.AllPagesLeftMarginLabel.Size = new System.Drawing.Size(22, 14);
			this.AllPagesLeftMarginLabel.TabIndex = 114;
			this.AllPagesLeftMarginLabel.Text = "left";
			// 
			// AllPagesBottomMarginLabel
			// 
			this.AllPagesBottomMarginLabel.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.AllPagesBottomMarginLabel.AutoSize = true;
			this.AllPagesBottomMarginLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(60)))), ((int)(((byte)(60)))));
			this.AllPagesBottomMarginLabel.Location = new System.Drawing.Point(107, 87);
			this.AllPagesBottomMarginLabel.Margin = new System.Windows.Forms.Padding(0);
			this.AllPagesBottomMarginLabel.Name = "AllPagesBottomMarginLabel";
			this.AllPagesBottomMarginLabel.Size = new System.Drawing.Size(39, 14);
			this.AllPagesBottomMarginLabel.TabIndex = 105;
			this.AllPagesBottomMarginLabel.Text = "bottom";
			// 
			// Page1TitleGroupBox
			// 
			this.Page1TitleGroupBox.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.Page1TitleGroupBox.Controls.Add(this.authorOverrideLabel);
			this.Page1TitleGroupBox.Controls.Add(this.titleOverrideLabel);
			this.Page1TitleGroupBox.Controls.Add(this.Page1TitleTextBox);
			this.Page1TitleGroupBox.Controls.Add(this.Page1AuthorTextBox);
			this.Page1TitleGroupBox.Controls.Add(this.Page1TitleYTextBox);
			this.Page1TitleGroupBox.Controls.Add(this.Page1TitleHeightTextBox);
			this.Page1TitleGroupBox.Controls.Add(this.Page1AuthorHeightTextBox);
			this.Page1TitleGroupBox.Controls.Add(this.Page1TitleYLabel);
			this.Page1TitleGroupBox.Controls.Add(this.Page1TitleHeightLabel);
			this.Page1TitleGroupBox.Controls.Add(this.Page1AuthorHeightLabel);
			this.Page1TitleGroupBox.ForeColor = System.Drawing.Color.Brown;
			this.Page1TitleGroupBox.Location = new System.Drawing.Point(19, 75);
			this.Page1TitleGroupBox.Name = "Page1TitleGroupBox";
			this.Page1TitleGroupBox.Size = new System.Drawing.Size(315, 139);
			this.Page1TitleGroupBox.TabIndex = 6;
			this.Page1TitleGroupBox.TabStop = false;
			this.Page1TitleGroupBox.Text = "page 1 titles";
			// 
			// authorOverrideLabel
			// 
			this.authorOverrideLabel.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.authorOverrideLabel.AutoSize = true;
			this.authorOverrideLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(60)))), ((int)(((byte)(60)))));
			this.authorOverrideLabel.Location = new System.Drawing.Point(13, 51);
			this.authorOverrideLabel.Margin = new System.Windows.Forms.Padding(0);
			this.authorOverrideLabel.Name = "authorOverrideLabel";
			this.authorOverrideLabel.Size = new System.Drawing.Size(81, 14);
			this.authorOverrideLabel.TabIndex = 141;
			this.authorOverrideLabel.Text = "author override";
			// 
			// titleOverrideLabel
			// 
			this.titleOverrideLabel.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.titleOverrideLabel.AutoSize = true;
			this.titleOverrideLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(60)))), ((int)(((byte)(60)))));
			this.titleOverrideLabel.Location = new System.Drawing.Point(28, 21);
			this.titleOverrideLabel.Margin = new System.Windows.Forms.Padding(0);
			this.titleOverrideLabel.Name = "titleOverrideLabel";
			this.titleOverrideLabel.Size = new System.Drawing.Size(66, 14);
			this.titleOverrideLabel.TabIndex = 140;
			this.titleOverrideLabel.Text = "title override";
			// 
			// Page1TitleTextBox
			// 
			this.Page1TitleTextBox.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.Page1TitleTextBox.Location = new System.Drawing.Point(96, 18);
			this.Page1TitleTextBox.Name = "Page1TitleTextBox";
			this.Page1TitleTextBox.Size = new System.Drawing.Size(200, 20);
			this.Page1TitleTextBox.TabIndex = 96;
			this.Page1TitleTextBox.TextChanged += new System.EventHandler(this.Page1TitleTextBox_Leave);
			// 
			// Page1AuthorTextBox
			// 
			this.Page1AuthorTextBox.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.Page1AuthorTextBox.Location = new System.Drawing.Point(96, 48);
			this.Page1AuthorTextBox.Name = "Page1AuthorTextBox";
			this.Page1AuthorTextBox.Size = new System.Drawing.Size(200, 20);
			this.Page1AuthorTextBox.TabIndex = 139;
			this.Page1AuthorTextBox.TextChanged += new System.EventHandler(this.Page1AuthorTextBox_Leave);
			// 
			// Page1TitleYTextBox
			// 
			this.Page1TitleYTextBox.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.Page1TitleYTextBox.Location = new System.Drawing.Point(149, 108);
			this.Page1TitleYTextBox.Name = "Page1TitleYTextBox";
			this.Page1TitleYTextBox.Size = new System.Drawing.Size(34, 20);
			this.Page1TitleYTextBox.TabIndex = 2;
			this.Page1TitleYTextBox.TextChanged += new System.EventHandler(this.SetToWhiteTextBox_TextChanged);
			this.Page1TitleYTextBox.Leave += new System.EventHandler(this.Page1TitleYTextBox_Leave);
			// 
			// Page1TitleHeightTextBox
			// 
			this.Page1TitleHeightTextBox.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.Page1TitleHeightTextBox.Location = new System.Drawing.Point(66, 78);
			this.Page1TitleHeightTextBox.Name = "Page1TitleHeightTextBox";
			this.Page1TitleHeightTextBox.Size = new System.Drawing.Size(34, 20);
			this.Page1TitleHeightTextBox.TabIndex = 0;
			this.Page1TitleHeightTextBox.TextChanged += new System.EventHandler(this.SetToWhiteTextBox_TextChanged);
			this.Page1TitleHeightTextBox.Leave += new System.EventHandler(this.Page1TitleHeightTextBox_Leave);
			// 
			// Page1AuthorHeightTextBox
			// 
			this.Page1AuthorHeightTextBox.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.Page1AuthorHeightTextBox.Location = new System.Drawing.Point(235, 78);
			this.Page1AuthorHeightTextBox.Name = "Page1AuthorHeightTextBox";
			this.Page1AuthorHeightTextBox.Size = new System.Drawing.Size(34, 20);
			this.Page1AuthorHeightTextBox.TabIndex = 1;
			this.Page1AuthorHeightTextBox.TextChanged += new System.EventHandler(this.SetToWhiteTextBox_TextChanged);
			this.Page1AuthorHeightTextBox.Leave += new System.EventHandler(this.Page1AuthorHeightTextBox_Leave);
			// 
			// Page1TitleYLabel
			// 
			this.Page1TitleYLabel.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.Page1TitleYLabel.AutoSize = true;
			this.Page1TitleYLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(60)))), ((int)(((byte)(60)))));
			this.Page1TitleYLabel.Location = new System.Drawing.Point(58, 111);
			this.Page1TitleYLabel.Margin = new System.Windows.Forms.Padding(0);
			this.Page1TitleYLabel.Name = "Page1TitleYLabel";
			this.Page1TitleYLabel.Size = new System.Drawing.Size(89, 14);
			this.Page1TitleYLabel.TabIndex = 97;
			this.Page1TitleYLabel.Text = "title and author Y";
			// 
			// Page1TitleHeightLabel
			// 
			this.Page1TitleHeightLabel.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.Page1TitleHeightLabel.AutoSize = true;
			this.Page1TitleHeightLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(60)))), ((int)(((byte)(60)))));
			this.Page1TitleHeightLabel.Location = new System.Drawing.Point(8, 81);
			this.Page1TitleHeightLabel.Margin = new System.Windows.Forms.Padding(0);
			this.Page1TitleHeightLabel.Name = "Page1TitleHeightLabel";
			this.Page1TitleHeightLabel.Size = new System.Drawing.Size(55, 14);
			this.Page1TitleHeightLabel.TabIndex = 91;
			this.Page1TitleHeightLabel.Text = "title height";
			// 
			// Page1AuthorHeightLabel
			// 
			this.Page1AuthorHeightLabel.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.Page1AuthorHeightLabel.AutoSize = true;
			this.Page1AuthorHeightLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(60)))), ((int)(((byte)(60)))));
			this.Page1AuthorHeightLabel.Location = new System.Drawing.Point(164, 81);
			this.Page1AuthorHeightLabel.Margin = new System.Windows.Forms.Padding(0);
			this.Page1AuthorHeightLabel.Name = "Page1AuthorHeightLabel";
			this.Page1AuthorHeightLabel.Size = new System.Drawing.Size(70, 14);
			this.Page1AuthorHeightLabel.TabIndex = 95;
			this.Page1AuthorHeightLabel.Text = "author height";
			// 
			// PaperSizeLabel
			// 
			this.PaperSizeLabel.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.PaperSizeLabel.AutoSize = true;
			this.PaperSizeLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(60)))), ((int)(((byte)(60)))));
			this.PaperSizeLabel.Location = new System.Drawing.Point(61, 20);
			this.PaperSizeLabel.Margin = new System.Windows.Forms.Padding(0);
			this.PaperSizeLabel.Name = "PaperSizeLabel";
			this.PaperSizeLabel.Size = new System.Drawing.Size(58, 14);
			this.PaperSizeLabel.TabIndex = 0;
			this.PaperSizeLabel.Text = "paper size";
			// 
			// ShowMainScoreFormButton
			// 
			this.ShowMainScoreFormButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.ShowMainScoreFormButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(215)))), ((int)(((byte)(225)))), ((int)(((byte)(215)))));
			this.ShowMainScoreFormButton.Location = new System.Drawing.Point(19, 421);
			this.ShowMainScoreFormButton.Name = "ShowMainScoreFormButton";
			this.ShowMainScoreFormButton.Size = new System.Drawing.Size(137, 26);
			this.ShowMainScoreFormButton.TabIndex = 3;
			this.ShowMainScoreFormButton.Text = "show main score form";
			this.ShowMainScoreFormButton.UseVisualStyleBackColor = false;
			this.ShowMainScoreFormButton.Click += new System.EventHandler(this.ShowMainScoreFormButton_Click);
			// 
			// UnitsHelpLabel
			// 
			this.UnitsHelpLabel.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.UnitsHelpLabel.AutoSize = true;
			this.UnitsHelpLabel.ForeColor = System.Drawing.Color.RoyalBlue;
			this.UnitsHelpLabel.Location = new System.Drawing.Point(44, 50);
			this.UnitsHelpLabel.Name = "UnitsHelpLabel";
			this.UnitsHelpLabel.Size = new System.Drawing.Size(265, 14);
			this.UnitsHelpLabel.TabIndex = 138;
			this.UnitsHelpLabel.Text = "Dimensions in screen pixels (integers, 100% display).";
			// 
			// RevertToSavedButton
			// 
			this.RevertToSavedButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.RevertToSavedButton.BackColor = System.Drawing.Color.Transparent;
			this.RevertToSavedButton.Enabled = false;
			this.RevertToSavedButton.Font = new System.Drawing.Font("Arial", 8F);
			this.RevertToSavedButton.ForeColor = System.Drawing.Color.Red;
			this.RevertToSavedButton.Location = new System.Drawing.Point(359, 421);
			this.RevertToSavedButton.Name = "RevertToSavedButton";
			this.RevertToSavedButton.Size = new System.Drawing.Size(137, 26);
			this.RevertToSavedButton.TabIndex = 2;
			this.RevertToSavedButton.Text = "revert to saved";
			this.RevertToSavedButton.UseVisualStyleBackColor = false;
			this.RevertToSavedButton.Click += new System.EventHandler(this.RevertToSavedButton_Click);
			// 
			// DimensionsAndMetadataForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 14F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(255)))), ((int)(((byte)(245)))));
			this.ClientSize = new System.Drawing.Size(658, 460);
			this.ControlBox = false;
			this.Controls.Add(this.RevertToSavedButton);
			this.Controls.Add(this.UnitsHelpLabel);
			this.Controls.Add(this.ShowMainScoreFormButton);
			this.Controls.Add(this.MetadataGroupBox);
			this.Controls.Add(this.ConfirmButton);
			this.Controls.Add(this.LandscapeCheckBox);
			this.Controls.Add(this.PaperSizeComboBox);
			this.Controls.Add(this.WebsiteLinksGroupBox);
			this.Controls.Add(this.MarginsGroupBox);
			this.Controls.Add(this.Page1TitleGroupBox);
			this.Controls.Add(this.PaperSizeLabel);
			this.Font = new System.Drawing.Font("Arial", 8F);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Location = new System.Drawing.Point(250, 100);
			this.Name = "DimensionsAndMetadataForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Dimensions and Metadata";
			this.MetadataGroupBox.ResumeLayout(false);
			this.MetadataGroupBox.PerformLayout();
			this.WebsiteLinksGroupBox.ResumeLayout(false);
			this.WebsiteLinksGroupBox.PerformLayout();
			this.MarginsGroupBox.ResumeLayout(false);
			this.MarginsGroupBox.PerformLayout();
			this.Page1TitleGroupBox.ResumeLayout(false);
			this.Page1TitleGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox MetadataGroupBox;
        private System.Windows.Forms.TextBox MetadataCommentTextBox;
        private System.Windows.Forms.Label MetadataCommentLabel;
        private System.Windows.Forms.TextBox MetadataKeywordsTextBox;
        private System.Windows.Forms.Label MetadataKeywordsLabel;
        private System.Windows.Forms.Button ConfirmButton;
        private System.Windows.Forms.CheckBox LandscapeCheckBox;
        private System.Windows.Forms.ComboBox PaperSizeComboBox;
        private System.Windows.Forms.GroupBox WebsiteLinksGroupBox;
        private System.Windows.Forms.TextBox AboutLinkTextTextBox;
        private System.Windows.Forms.TextBox AboutLinkURLTextBox;
        private System.Windows.Forms.Label AboutLinkTextLabel;
        private System.Windows.Forms.Label AboutLinkURLLabel;
        private System.Windows.Forms.GroupBox MarginsGroupBox;
        private System.Windows.Forms.TextBox BottomMarginTextBox;
        private System.Windows.Forms.TextBox TopMarginPage1TextBox;
        private System.Windows.Forms.TextBox TopMarginOtherPagesTextBox;
        private System.Windows.Forms.TextBox RightMarginTextBox;
        private System.Windows.Forms.TextBox LeftMarginTextBox;
        private System.Windows.Forms.Label Page1AboveTopSystemLabel;
        private System.Windows.Forms.Label AllPagesRightMarginLabel;
        private System.Windows.Forms.Label OtherPagesAboveTopSystemLabel;
        private System.Windows.Forms.Label AllPagesLeftMarginLabel;
        private System.Windows.Forms.Label AllPagesBottomMarginLabel;
        private System.Windows.Forms.GroupBox Page1TitleGroupBox;
        private System.Windows.Forms.TextBox Page1TitleYTextBox;
        private System.Windows.Forms.TextBox Page1TitleHeightTextBox;
        private System.Windows.Forms.TextBox Page1AuthorHeightTextBox;
        private System.Windows.Forms.Label Page1TitleYLabel;
        private System.Windows.Forms.Label Page1TitleHeightLabel;
        private System.Windows.Forms.Label Page1AuthorHeightLabel;
        private System.Windows.Forms.Label PaperSizeLabel;
        private System.Windows.Forms.Button ShowMainScoreFormButton;
        private System.Windows.Forms.Label UnitsHelpLabel;
        private System.Windows.Forms.Button RevertToSavedButton;
		private System.Windows.Forms.Label authorOverrideLabel;
		private System.Windows.Forms.Label titleOverrideLabel;
		private System.Windows.Forms.TextBox Page1TitleTextBox;
		private System.Windows.Forms.TextBox Page1AuthorTextBox;
	}
}