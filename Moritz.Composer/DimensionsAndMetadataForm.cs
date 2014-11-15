using System;
using System.Windows.Forms;
using System.Xml;
using System.Diagnostics;
using System.Collections.Generic;
using System.Drawing;

using Moritz.Globals;
using Moritz.Palettes;

namespace Moritz.Composer
{
    public partial class DimensionsAndMetadataForm : Form, IRevertableForm
    {
        /// <summary>
        /// Creates a new, empty DimensionsAndMetadataForm.
        /// </summary>
        public DimensionsAndMetadataForm(AssistantComposerMainForm assistantComposerMainForm)
        {
            InitializeComponent();
            _assistantComposerMainForm = assistantComposerMainForm;
            _allTextBoxes = GetAllTextBoxes();
            SetDefaultValues();
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

        #region IRevertableForm
        public bool HasError { get { return M.HasError(_allTextBoxes); } }
        public bool NeedsReview { get { return _rff.NeedsReview(this); } }
        public bool HasBeenChecked { get { return _rff.HasBeenChecked(this); } }
        #endregion

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
            _rff.SetIsSaved(this, OkayToSaveButton, RevertToSavedButton);
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
        #endregion Read

        #region Write
        internal void Write(XmlWriter w)
        {
            WriteMetadata(w);
            WriteDimensions(w);

            _rff.SetIsSaved(this, OkayToSaveButton, RevertToSavedButton);
        }
        private void WriteMetadata(XmlWriter w)
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
        private void WriteDimensions(XmlWriter w)
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
        private void SetSettingsNeedReview()
        {
            _rff.SetSettingsNeedReview(this, OkayToSaveButton, RevertToSavedButton);
        }
        private void PaperSizeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetSettingsNeedReview();
            this.PaperSizeLabel.Focus();
        }
        private void LandscapeCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            SetSettingsNeedReview();
        }
        private void SetToWhiteTextBox_TextChanged(object sender, EventArgs e)
        {
            M.SetToWhite(sender as TextBox);
        }
        #region TextBox_Leave handlers
        private void Page1TitleHeightTextBox_Leave(object sender, EventArgs e)
        {
            CheckTextBoxIsFloat(this.Page1TitleHeightTextBox);
            SetSettingsNeedReview();
        }
        private void Page1AuthorHeightTextBox_Leave(object sender, EventArgs e)
        {
            CheckTextBoxIsFloat(this.Page1AuthorHeightTextBox);
            SetSettingsNeedReview();
        }
        private void Page1TitleYTextBox_Leave(object sender, EventArgs e)
        {
            CheckTextBoxIsFloat(this.Page1TitleYTextBox);
            SetSettingsNeedReview();
        }

        private void TopMarginPage1TextBox_Leave(object sender, EventArgs e)
        {
            CheckTextBoxIsFloat(this.TopMarginPage1TextBox);
            SetSettingsNeedReview();
        }
        private void TopMarginOtherPagesTextBox_Leave(object sender, EventArgs e)
        {
            CheckTextBoxIsFloat(this.TopMarginOtherPagesTextBox);
            SetSettingsNeedReview();
        }
        private void RightMarginTextBox_Leave(object sender, EventArgs e)
        {
            CheckTextBoxIsFloat(this.RightMarginTextBox);
            SetSettingsNeedReview();
        }
        private void BottomMarginTextBox_Leave(object sender, EventArgs e)
        {
            CheckTextBoxIsFloat(this.BottomMarginTextBox);
            SetSettingsNeedReview();
        }
        private void LeftMarginTextBox_Leave(object sender, EventArgs e)
        {
            CheckTextBoxIsFloat(this.LeftMarginTextBox);
            SetSettingsNeedReview();
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

            M.SetTextBoxErrorColorIfNotOkay(textBox, okay);
        }

        private void AboutLinkTextTextBox_Leave(object sender, EventArgs e)
        {
            SetSettingsNeedReview();
        }
        private void AboutLinkURLTextBox_Leave(object sender, EventArgs e)
        {
            SetSettingsNeedReview();
        }
        private void RecordingTextBox_Leave(object sender, EventArgs e)
        {
            SetSettingsNeedReview();
        }
        private void MetadataKeywordsTextBox_Leave(object sender, EventArgs e)
        {
            SetSettingsNeedReview();
        }
        private void MetadataCommentTextBox_Leave(object sender, EventArgs e)
        {
            SetSettingsNeedReview();
        }
        #endregion TextBox_Leave handlers
        private void ShowMainScoreFormButton_Click(object sender, EventArgs e)
        {
            this._assistantComposerMainForm.BringToFront();
        }

        private void OkayToSaveButton_Click(object sender, EventArgs e)
        {
            _rff.SetSettingsCanBeSaved(this, OkayToSaveButton); 
        }
        #region RevertToSaved
        private void RevertToSavedButton_Click(object sender, EventArgs e)
        {
            Debug.Assert(this.Text.EndsWith(_rff.NeedsReviewStr) || this.Text.EndsWith(_rff.ChangedAndCheckedStr));
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

                TouchAllTextBoxes();

                _rff.SetIsSaved(this, OkayToSaveButton, RevertToSavedButton);
            }
        }

        private void TouchAllTextBoxes()
        {
            BottomMarginTextBox_Leave(BottomMarginTextBox, null);
            TopMarginPage1TextBox_Leave(TopMarginPage1TextBox, null);
            TopMarginOtherPagesTextBox_Leave(TopMarginOtherPagesTextBox, null);
            RightMarginTextBox_Leave(RightMarginTextBox, null);
            LeftMarginTextBox_Leave(LeftMarginTextBox, null);
            Page1TitleYTextBox_Leave(Page1TitleYTextBox, null);
            Page1TitleHeightTextBox_Leave(Page1TitleHeightTextBox, null);
            Page1AuthorHeightTextBox_Leave(Page1AuthorHeightTextBox, null);
            AboutLinkTextTextBox_Leave(AboutLinkTextTextBox, null);
            AboutLinkURLTextBox_Leave(AboutLinkURLTextBox, null);
            MetadataCommentTextBox_Leave(MetadataCommentTextBox, null);
            MetadataKeywordsTextBox_Leave(MetadataKeywordsTextBox, null);
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
        private RevertableFormFunctions _rff = new RevertableFormFunctions();
        #endregion private variables
    }
}
