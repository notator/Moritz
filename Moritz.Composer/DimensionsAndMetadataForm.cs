using System;
using System.Windows.Forms;
using System.Xml;
using System.Diagnostics;
using System.Collections.Generic;
using System.Drawing;

using Moritz.Globals;

namespace Moritz.Composer
{
    public partial class DimensionsAndMetadataForm : Form
    {
        /// <summary>
        /// Creates a new, empty AssistantComposerMainForm.
        /// </summary>
        /// <param name="assistantComposer"></param>
        /// <param name="krystal"></param>
        public DimensionsAndMetadataForm(AssistantComposerMainForm assistantComposerMainForm)
        {
            InitializeComponent();
            SetDefaultValues();

            _assistantComposerMainForm = assistantComposerMainForm;
        }
        private void SetDefaultValues()
        {
            this.PaperSizeComboBox.Text = "A3";
            this.LandscapeCheckBox.Checked = false;

            this.Page1TitleHeightTextBox.Text = "32";
            this.Page1AuthorHeightTextBox.Text = "16";
            this.Page1TitleYTextBox.Text = "50";

            this.TopMarginPage1TextBox.Text = "90";
            this.TopMarginOtherPagesTextBox.Text = "50";
            this.RightMarginTextBox.Text = "50";
            this.BottomMarginTextBox.Text = "50";
            this.LeftMarginTextBox.Text = "50";

            this.AboutLinkTextTextBox.Text = "";
            this.AboutLinkURLTextBox.Text = "";
        }

        #region HasError
        public bool HasError
        {
            get
            {
                bool hasError = false;
                if(_allTextBoxes == null || _allTextBoxes.Count == 0)
                {
                    _allTextBoxes = GetAllTextBoxes();
                }
                foreach(TextBox textBox in _allTextBoxes)
                {
                    if(textBox.BackColor == M.TextBoxErrorColor)
                    {
                        hasError = true;
                        break;
                    }
                }
                return hasError;
            }
        }
        private List<TextBox> GetAllTextBoxes()
        {
            List<TextBox> textBoxes = new List<TextBox>();

            textBoxes.Add(BottomMarginTextBox);
            textBoxes.Add(TopMarginPage1TextBox);
            textBoxes.Add(TopMarginOtherPagesTextBox);
            textBoxes.Add(RightMarginTextBox);
            textBoxes.Add(LeftMarginTextBox);
            textBoxes.Add(Page1TitleYTextBox);
            textBoxes.Add(Page1TitleHeightTextBox);
            textBoxes.Add(Page1AuthorHeightTextBox);
            textBoxes.Add(AboutLinkTextTextBox);
            textBoxes.Add(AboutLinkURLTextBox);
            textBoxes.Add(MetadataCommentTextBox);
            textBoxes.Add(MetadataKeywordsTextBox);

            return textBoxes;
        }
        #endregion HasError

        #region Read
        public void Read(XmlReader r)
        {
            Debug.Assert(r.Name == "moritzKrystalScore");
            M.ReadToXmlElementTag(r, "metadata", "dimensions");
            while(r.Name == "metadata" || r.Name == "dimensions")
            {
                if(r.NodeType != XmlNodeType.EndElement)
                {
                    switch(r.Name)
                    {
                        case "metadata":
                            GetMetadata(r);
                            break;
                        case "dimensions":
                            GetDimensions(r);
                            break;
                    }
                }
                M.ReadToXmlElementTag(r, "metadata", "dimensions", "performerOptions", "notation", "names", "krystals", "palettes");
            }
            SetSettingsHaveBeenSaved();
        }
        private void GetMetadata(XmlReader r)
        {
            Debug.Assert(r.Name == "metadata");
            #region default values
            this.MetadataKeywordsTextBox.Text = "";
            this.MetadataCommentTextBox.Text = "";
            #endregion
            int count = r.AttributeCount;
            for(int i = 0; i < count; i++)
            {
                r.MoveToAttribute(i);
                switch(r.Name)
                {
                    case "keywords":
                        this.MetadataKeywordsTextBox.Text = r.Value;
                        SavedMetadataKeywordsTextBoxText = MetadataKeywordsTextBox.Text;
                        break;
                    case "comment":
                        this.MetadataCommentTextBox.Text = r.Value;
                        SavedMetadataCommentTextBoxText = MetadataCommentTextBox.Text;
                        break;
                }
            }
            GetWebsiteLinks(r);
        }
        private void GetWebsiteLinks(XmlReader r)
        {
            Debug.Assert(r.Name == "metadata");
            M.ReadToXmlElementTag(r, "websiteLink");
            int count = r.AttributeCount;
            for(int i = 0; i < count; i++)
            {
                r.MoveToAttribute(i);
                switch(r.Name)
                {
                    case "aboutLinkText":
                        this.AboutLinkTextTextBox.Text = r.Value;
                        SavedAboutLinkTextTextBoxText = AboutLinkTextTextBox.Text;
                        break;
                    case "aboutLinkURL":
                        this.AboutLinkURLTextBox.Text = r.Value;
                        SavedAboutLinkURLTextBoxText = AboutLinkURLTextBox.Text;
                        break;
                }
            }
        }
        private void GetDimensions(XmlReader r)
        {
            M.ReadToXmlElementTag(r, "paper", "title", "margins");
            while(r.Name == "paper" || r.Name == "title" || r.Name == "margins")
            {
                if(r.NodeType != XmlNodeType.EndElement)
                {
                    switch(r.Name)
                    {
                        case "paper":
                            GetPaperSize(r);
                            break;
                        case "title":
                            GetTitleSizesAndPositions(r);
                            break;
                        case "margins":
                            GetFrame(r);
                            break;
                    }
                }
                M.ReadToXmlElementTag(r, "paper", "title", "margins", "dimensions");
            }
        }
        private void GetPaperSize(XmlReader r)
        {
            Debug.Assert(r.Name == "paper");
            int count = r.AttributeCount;
            for(int i = 0; i < count; i++)
            {
                r.MoveToAttribute(i);
                switch(r.Name)
                {
                    case "size":
                        SavedPaperSize = r.Value;
                        int item = 0;
                        do
                        {
                            PaperSizeComboBox.SelectedIndex = item++;
                        } while(r.Value != PaperSizeComboBox.SelectedItem.ToString());
                        break;
                    case "landscape":
                        SavedLandscapeCheckBoxChecked = r.Value;
                        if(r.Value == "1")
                            LandscapeCheckBox.Checked = true;
                        else
                            LandscapeCheckBox.Checked = false;
                        break;
                }
            }
        }
        private void GetTitleSizesAndPositions(XmlReader r)
        {
            Debug.Assert(r.Name == "title");
            int count = r.AttributeCount;
            for(int i = 0; i < count; i++)
            {
                r.MoveToAttribute(i);
                switch(r.Name)
                {
                    case "titleHeight":
                        Page1TitleHeightTextBox.Text = r.Value;
                        SavedPage1TitleHeightTextBoxText = Page1TitleHeightTextBox.Text;
                        break;
                    case "authorHeight":
                        Page1AuthorHeightTextBox.Text = r.Value;
                        SavedPage1AuthorHeightTextBoxText = Page1AuthorHeightTextBox.Text;
                        break;
                    case "titleY":
                        Page1TitleYTextBox.Text = r.Value;
                        SavedPage1TitleYTextBoxText = Page1TitleYTextBox.Text;
                        break;
                }
            }
        }
        private void GetFrame(XmlReader r)
        {
            Debug.Assert(r.Name == "margins");
            int count = r.AttributeCount;
            for(int i = 0; i < count; i++)
            {
                r.MoveToAttribute(i);
                switch(r.Name)
                {
                    case "topPage1":
                        TopMarginPage1TextBox.Text = r.Value;
                        SavedTopMarginPage1TextBoxText = TopMarginPage1TextBox.Text;
                        break;
                    case "topOtherPages":
                        TopMarginOtherPagesTextBox.Text = r.Value;
                        SavedTopMarginOtherPagesTextBoxText = TopMarginOtherPagesTextBox.Text;
                        break;
                    case "right":
                        RightMarginTextBox.Text = r.Value;
                        SavedRightMarginTextBoxText = RightMarginTextBox.Text;
                        break;
                    case "bottom":
                        BottomMarginTextBox.Text = r.Value;
                        SavedBottomMarginTextBoxText = BottomMarginTextBox.Text;
                        break;
                    case "left":
                        LeftMarginTextBox.Text = r.Value;
                        SavedLeftMarginTextBoxText = LeftMarginTextBox.Text;
                        break;
                }
            }
        }
        /// <summary>
        /// Removes the '*' in Text, disables the OkayToSave and RevertToSaved Buttons
        /// </summary>
        private void SetSettingsHaveBeenSaved()
        {
            if(this.Text.EndsWith("*"))
            {
                this.Text = this.Text.Remove(this.Text.Length - 1);
            }
            this.OkayToSaveButton.Enabled = false;
            this.RevertToSavedButton.Enabled = false;
        }
        #endregion Read

        #region Write
        public void WriteMetadata(XmlWriter w)
        {
            w.WriteStartElement("metadata");
            if(!String.IsNullOrEmpty(MetadataKeywordsTextBox.Text))
                w.WriteAttributeString("keywords", this.MetadataKeywordsTextBox.Text);
            if(!String.IsNullOrEmpty(MetadataCommentTextBox.Text))
                w.WriteAttributeString("comment", this.MetadataCommentTextBox.Text);

            w.WriteStartElement("websiteLink");
            w.WriteAttributeString("aboutLinkText", this.AboutLinkTextTextBox.Text);
            w.WriteAttributeString("aboutLinkURL", this.AboutLinkURLTextBox.Text);
            w.WriteEndElement(); // websiteLink
            w.WriteEndElement(); // metadata
        }
        public void WriteDimensions(XmlWriter w)
        {
            w.WriteStartElement("dimensions");
            w.WriteStartElement("paper");
            if(this.PaperSizeComboBox.SelectedIndex >= 0)
                w.WriteAttributeString("size", this.PaperSizeComboBox.SelectedItem.ToString());
            else
                w.WriteAttributeString("size", "A3");

            string landscapeBool = LandscapeCheckBox.Checked ? "1" : "0";
            w.WriteAttributeString("landscape", landscapeBool);
            w.WriteEndElement(); // paper

            w.WriteStartElement("title");
            w.WriteAttributeString("titleHeight", Page1TitleHeightTextBox.Text);
            w.WriteAttributeString("authorHeight", Page1AuthorHeightTextBox.Text);
            w.WriteAttributeString("titleY", Page1TitleYTextBox.Text);
            w.WriteEndElement(); // title

            w.WriteStartElement("margins");
            w.WriteAttributeString("topPage1", TopMarginPage1TextBox.Text);
            w.WriteAttributeString("topOtherPages", TopMarginOtherPagesTextBox.Text);
            w.WriteAttributeString("right", RightMarginTextBox.Text);
            w.WriteAttributeString("bottom", BottomMarginTextBox.Text);
            w.WriteAttributeString("left", LeftMarginTextBox.Text);
            w.WriteEndElement(); // margins
            w.WriteEndElement(); // dimensions
        }
        #endregion Write

        #region Event Handlers
        /// <summary>
        /// Sets the '*' in Text, enables the SaveButton and informs _assistantComposerMainForm
        /// </summary>
        private void SetSettingsHaveChanged()
        {
            if(!this.Text.EndsWith("*"))
            {
                this.Text = this.Text + "*";
                if(this._assistantComposerMainForm != null)
                {
                    _assistantComposerMainForm.SetSettingsHaveChanged();
                }
            }
            if(HasError)
            {
                this.OkayToSaveButton.Enabled = false;
            }
            else
            {
                this.OkayToSaveButton.Enabled = true;
            }

            this.RevertToSavedButton.Enabled = true;
        }
        private void PaperSizeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetSettingsHaveChanged();
            this.PaperSizeLabel.Focus();
        }
        private void LandscapeCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            SetSettingsHaveChanged();
        }
        private void SetToWhiteTextBox_TextChanged(object sender, EventArgs e)
        {
            M.SetToWhite(sender as TextBox);
        }
        #region TextBox_Leave handlers
        private void Page1TitleHeightTextBox_Leave(object sender, EventArgs e)
        {
            CheckTextBoxIsFloat(this.Page1TitleHeightTextBox);
            SetSettingsHaveChanged();
        }
        private void Page1AuthorHeightTextBox_Leave(object sender, EventArgs e)
        {
            CheckTextBoxIsFloat(this.Page1AuthorHeightTextBox);
            SetSettingsHaveChanged();
        }
        private void Page1TitleYTextBox_Leave(object sender, EventArgs e)
        {
            CheckTextBoxIsFloat(this.Page1TitleYTextBox);
            SetSettingsHaveChanged();
        }

        private void TopMarginPage1TextBox_Leave(object sender, EventArgs e)
        {
            CheckTextBoxIsFloat(this.TopMarginPage1TextBox);
            SetSettingsHaveChanged();
        }
        private void TopMarginOtherPagesTextBox_Leave(object sender, EventArgs e)
        {
            CheckTextBoxIsFloat(this.TopMarginOtherPagesTextBox);
            SetSettingsHaveChanged();
        }
        private void RightMarginTextBox_Leave(object sender, EventArgs e)
        {
            CheckTextBoxIsFloat(this.RightMarginTextBox);
            SetSettingsHaveChanged();
        }
        private void BottomMarginTextBox_Leave(object sender, EventArgs e)
        {
            CheckTextBoxIsFloat(this.BottomMarginTextBox);
            SetSettingsHaveChanged();
        }
        private void LeftMarginTextBox_Leave(object sender, EventArgs e)
        {
            CheckTextBoxIsFloat(this.LeftMarginTextBox);
            SetSettingsHaveChanged();
        }

        private void CheckTextBoxIsFloat(TextBox textBox)
        {
            bool okay = true;
            textBox.Text.Trim();
            try
            {
                float i = float.Parse(textBox.Text, M.En_USNumberFormat);
            }
            catch
            {
                okay = false;
            }

            if(okay)
            {
                textBox.BackColor = Color.White;
            }
            else
            {
                textBox.BackColor = M.TextBoxErrorColor;
            }
        }

        private void AboutLinkTextTextBox_Leave(object sender, EventArgs e)
        {
            SetSettingsHaveChanged();
        }
        private void AboutLinkURLTextBox_Leave(object sender, EventArgs e)
        {
            SetSettingsHaveChanged();
        }
        private void RecordingTextBox_Leave(object sender, EventArgs e)
        {
            SetSettingsHaveChanged();
        }
        private void MetadataKeywordsTextBox_Leave(object sender, EventArgs e)
        {
            SetSettingsHaveChanged();
        }
        private void MetadataCommentTextBox_Leave(object sender, EventArgs e)
        {
            SetSettingsHaveChanged();
        }
        #endregion TextBox_Leave handlers
        private void ShowMainScoreFormButton_Click(object sender, EventArgs e)
        {
            this._assistantComposerMainForm.BringToFront();
        }
        /// <summary>
        /// When the main Assistant Composer Form tries to save, it first checks
        /// that none of its subsidiary forms' Texts ends with a "*".
        /// If they do, then they must be reviewed, and either the OkayToSaveButton
        /// or RevertToSavedButton must be clicked.
        /// </summary>
        private void OkayToSaveButton_Click(object sender, EventArgs e)
        {
            Debug.Assert(!this.HasError);
            Debug.Assert(this.Text.EndsWith("*"));

            this.Text = this.Text.Remove(this.Text.Length - 1);
            if(!this.Text.EndsWith("(changed)"))
            {
                this.Text = this.Text + " (changed)";
            }
            this.OkayToSaveButton.Enabled = false;
        }
        #region RevertToSaved
        /// <summary>
        /// When the main Assistant Composer Form tries to save, it first checks
        /// that none of its subsidiary forms' Texts ends with a "*".
        /// If they do, then they must be reviewed, and either the OkayToSaveButton
        /// or RevertToSavedButton must be clicked.
        /// </summary>
        private void RevertToSavedButton_Click(object sender, EventArgs e)
        {
            Debug.Assert(this.Text.EndsWith("*") || this.Text.EndsWith(" (changed)"));
            DialogResult result =
                MessageBox.Show("Are you sure you want to revert this dialog to the saved version?", "Revert?",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);

            if(result == System.Windows.Forms.DialogResult.Yes)
            {
                MetadataKeywordsTextBox.Text = SavedMetadataKeywordsTextBoxText;
                MetadataCommentTextBox.Text = SavedMetadataCommentTextBoxText;
                AboutLinkTextTextBox.Text = SavedAboutLinkTextTextBoxText;
                AboutLinkURLTextBox.Text = SavedAboutLinkURLTextBoxText;

                ResetPaperSize(SavedPaperSize, SavedLandscapeCheckBoxChecked);

                Page1TitleHeightTextBox.Text = SavedPage1TitleHeightTextBoxText;
                Page1AuthorHeightTextBox.Text = SavedPage1AuthorHeightTextBoxText;
                Page1TitleYTextBox.Text = SavedPage1TitleYTextBoxText;

                TopMarginPage1TextBox.Text = SavedTopMarginPage1TextBoxText;
                TopMarginOtherPagesTextBox.Text = SavedTopMarginOtherPagesTextBoxText;
                RightMarginTextBox.Text = SavedRightMarginTextBoxText;
                BottomMarginTextBox.Text = SavedBottomMarginTextBoxText;
                LeftMarginTextBox.Text = SavedLeftMarginTextBoxText;

                #region identical to OrnamentSettingsForm.RevertToSavedButton_Click()

                SetAllTextBoxBackColorsToWhite();

                if(this.Text.EndsWith("*"))
                {
                    this.Text = this.Text.Remove(this.Text.Length - 1);
                }

                if(this.Text.EndsWith(" (changed)"))
                {
                    this.Text = this.Text.Remove(this.Text.Length - " (changed)".Length);
                }

                this.RevertToSavedButton.Enabled = false;
                this.OkayToSaveButton.Enabled = false;
                #endregion
            }
        }
        private void SetAllTextBoxBackColorsToWhite()
        {
            MetadataKeywordsTextBox.BackColor = Color.White;
            MetadataCommentTextBox.BackColor = Color.White;
            AboutLinkTextTextBox.BackColor = Color.White;
            AboutLinkURLTextBox.BackColor = Color.White;

            Page1TitleHeightTextBox.BackColor = Color.White;
            Page1AuthorHeightTextBox.BackColor = Color.White;
            Page1TitleYTextBox.BackColor = Color.White;

            TopMarginPage1TextBox.BackColor = Color.White;
            TopMarginOtherPagesTextBox.BackColor = Color.White;
            RightMarginTextBox.BackColor = Color.White;
            BottomMarginTextBox.BackColor = Color.White;
            LeftMarginTextBox.BackColor = Color.White;
        }
        private void ResetPaperSize(string SavedPaperSize, string SavedLandscapeCheckBoxChecked)
        {
            int item = 0;
            do
            {
                PaperSizeComboBox.SelectedIndex = item++;
            } while(SavedPaperSize != PaperSizeComboBox.SelectedItem.ToString());

            if(SavedLandscapeCheckBoxChecked == "1")
                LandscapeCheckBox.Checked = true;
            else
                LandscapeCheckBox.Checked = false;
        }
        #endregion RevertToSaved
        #endregion

        #region public properties
        public string Keywords { get { return MetadataKeywordsTextBox.Text; } }
        public string Comment { get { return MetadataCommentTextBox.Text; } }
        public string PaperSize { get { return PaperSizeComboBox.Text; } }
        public bool Landscape { get { return LandscapeCheckBox.Checked; } }

        public float TitleHeight { get { return float.Parse(Page1TitleHeightTextBox.Text, M.En_USNumberFormat); } }
        public float AuthorHeight { get { return float.Parse(Page1AuthorHeightTextBox.Text, M.En_USNumberFormat); } }
        public float TitleY { get { return float.Parse(Page1TitleYTextBox.Text, M.En_USNumberFormat); } }

        public float TopMarginWidthPage1 { get { return float.Parse(TopMarginPage1TextBox.Text, M.En_USNumberFormat); } }
        public float TopMarginWidthOtherPages { get { return float.Parse(TopMarginOtherPagesTextBox.Text, M.En_USNumberFormat); } }
        public float RightMarginWidth { get { return float.Parse(RightMarginTextBox.Text, M.En_USNumberFormat); } }
        public float BottomMarginWidth { get { return float.Parse(BottomMarginTextBox.Text, M.En_USNumberFormat); } }
        public float LeftMarginWidth { get { return float.Parse(LeftMarginTextBox.Text, M.En_USNumberFormat); } }

        public string AboutLinkText { get { return AboutLinkTextTextBox.Text; } }
        public string AboutLinkURL { get { return AboutLinkURLTextBox.Text; } }
        #endregion public properties

        #region private variables
        #region for reverting
        private string SavedMetadataKeywordsTextBoxText;
        private string SavedMetadataCommentTextBoxText;
        private string SavedAboutLinkTextTextBoxText;
        private string SavedAboutLinkURLTextBoxText;
        private string SavedPaperSize;
        private string SavedLandscapeCheckBoxChecked;
        private string SavedPage1TitleHeightTextBoxText;
        private string SavedPage1AuthorHeightTextBoxText;
        private string SavedPage1TitleYTextBoxText;
        private string SavedTopMarginPage1TextBoxText;
        private string SavedTopMarginOtherPagesTextBoxText;
        private string SavedRightMarginTextBoxText;
        private string SavedBottomMarginTextBoxText;
        private string SavedLeftMarginTextBoxText;
        #endregion
        private AssistantComposerMainForm _assistantComposerMainForm = null;
        private List<TextBox> _allTextBoxes;
        #endregion private variables
    }
}
