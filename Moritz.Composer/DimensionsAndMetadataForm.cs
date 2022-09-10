using Moritz.Globals;
using Moritz.Palettes;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;
using System.Xml;

namespace Moritz.Composer
{

    internal partial class DimensionsAndMetadataForm : Form
    {
        /// <summary>
        /// Creates a new, empty DimensionsAndMetadataForm.
        /// </summary>
        internal DimensionsAndMetadataForm(AssistantComposerForm assistantComposerForm, string settingsPath, FormStateFunctions fsf)
        {
            InitializeComponent();
            _assistantComposerForm = assistantComposerForm;
            _settingsPath = settingsPath; // used when reverting
            _fsf = fsf;
            _allTextBoxes = GetAllTextBoxes();
            SetDefaultValues();
        }
        private List<TextBox> GetAllTextBoxes()
        {
            List<TextBox> textBoxes = new List<TextBox>
            {
                BottomMarginTextBox,
                TopMarginPage1TextBox,
                TopMarginOtherPagesTextBox,
                RightMarginTextBox,
                LeftMarginTextBox,
                Page1TitleTextBox,
                Page1AuthorTextBox,
                Page1TitleYTextBox,
                Page1TitleHeightTextBox,
                Page1AuthorHeightTextBox,
                AboutLinkTextTextBox,
                AboutLinkURLTextBox,
                MetadataCommentTextBox,
                MetadataKeywordsTextBox
            };

            return textBoxes;
        }

        private void SetDefaultValues()
        {
            this.PaperSizeComboBox.Text = "A3";
            this.LandscapeCheckBox.Checked = false;

            this.Page1TitleTextBox.Text = "";
            this.Page1AuthorTextBox.Text = "";
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

        #region Read
        public void Read(XmlReader r)
        {
            _isLoading = true;
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
            _fsf.SetSettingsAreSaved(this, M.HasError(_allTextBoxes), ConfirmButton, RevertToSavedButton);
            _isLoading = false;
        }
        private void GetMetadata(XmlReader r)
        {
            Debug.Assert(r.Name == "metadata");
            #region default values
            this.Page1TitleTextBox.Text = "";
            this.Page1AuthorTextBox.Text = "";
            this.MetadataKeywordsTextBox.Text = "";
            this.MetadataCommentTextBox.Text = "";
            #endregion
            int count = r.AttributeCount;
            for(int i = 0; i < count; i++)
            {
                r.MoveToAttribute(i);
                switch(r.Name)
                {
                    case "page1Title":
                        this.Page1TitleTextBox.Text = r.Value;
                        break;
                    case "page1Author":
                        this.Page1AuthorTextBox.Text = r.Value;
                        break;

                    case "keywords":
                        this.MetadataKeywordsTextBox.Text = r.Value;
                        break;
                    case "comment":
                        this.MetadataCommentTextBox.Text = r.Value;
                        break;
                }
            }
            GetWebsiteLinks(r);
        }
        private void GetWebsiteLinks(XmlReader r)
        {
            //Debug.Assert(r.Name == "metadata");
            M.ReadToXmlElementTag(r, "websiteLink");
            int count = r.AttributeCount;
            for(int i = 0; i < count; i++)
            {
                r.MoveToAttribute(i);
                switch(r.Name)
                {
                    case "aboutLinkText":
                        this.AboutLinkTextTextBox.Text = r.Value;
                        break;
                    case "aboutLinkURL":
                        this.AboutLinkURLTextBox.Text = r.Value;
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
                        int item = 0;
                        do
                        {
                            PaperSizeComboBox.SelectedIndex = item++;
                        } while(r.Value != PaperSizeComboBox.SelectedItem.ToString());
                        break;
                    case "landscape":
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
                        break;
                    case "authorHeight":
                        Page1AuthorHeightTextBox.Text = r.Value;
                        break;
                    case "titleY":
                        Page1TitleYTextBox.Text = r.Value;
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
                        break;
                    case "topOtherPages":
                        TopMarginOtherPagesTextBox.Text = r.Value;
                        break;
                    case "right":
                        RightMarginTextBox.Text = r.Value;
                        break;
                    case "bottom":
                        BottomMarginTextBox.Text = r.Value;
                        break;
                    case "left":
                        LeftMarginTextBox.Text = r.Value;
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

            _fsf.SetSettingsAreSaved(this, M.HasError(_allTextBoxes), ConfirmButton, RevertToSavedButton);
        }
        private void WriteMetadata(XmlWriter w)
        {
            w.WriteStartElement("metadata");

            if(!String.IsNullOrEmpty(Page1TitleTextBox.Text))
                w.WriteAttributeString("page1Title", this.Page1TitleTextBox.Text);
            if(!String.IsNullOrEmpty(Page1AuthorTextBox.Text))
                w.WriteAttributeString("page1Author", this.Page1AuthorTextBox.Text);

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
        private void SetSettingsHaveChanged()
        {
            _fsf.SetSettingsAreUnconfirmed(this, M.HasError(_allTextBoxes), ConfirmButton, RevertToSavedButton);
            if(!_isLoading)
            {
                _assistantComposerForm.UpdateMainFormState();
            }
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
            CheckTextBoxIsInt(this.Page1TitleHeightTextBox);
            SetSettingsHaveChanged();
        }
        private void Page1AuthorHeightTextBox_Leave(object sender, EventArgs e)
        {
            CheckTextBoxIsInt(this.Page1AuthorHeightTextBox);
            SetSettingsHaveChanged();
        }
        private void Page1TitleYTextBox_Leave(object sender, EventArgs e)
        {
            CheckTextBoxIsInt(this.Page1TitleYTextBox);
            SetSettingsHaveChanged();
        }

        private void TopMarginPage1TextBox_Leave(object sender, EventArgs e)
        {
            CheckTextBoxIsInt(this.TopMarginPage1TextBox);
            SetSettingsHaveChanged();
        }
        private void TopMarginOtherPagesTextBox_Leave(object sender, EventArgs e)
        {
            CheckTextBoxIsInt(this.TopMarginOtherPagesTextBox);
            SetSettingsHaveChanged();
        }
        private void RightMarginTextBox_Leave(object sender, EventArgs e)
        {
            CheckTextBoxIsInt(this.RightMarginTextBox);
            SetSettingsHaveChanged();
        }
        private void BottomMarginTextBox_Leave(object sender, EventArgs e)
        {
            CheckTextBoxIsInt(this.BottomMarginTextBox);
            SetSettingsHaveChanged();
        }
        private void LeftMarginTextBox_Leave(object sender, EventArgs e)
        {
            CheckTextBoxIsInt(this.LeftMarginTextBox);
            SetSettingsHaveChanged();
        }

        private void CheckTextBoxIsInt(TextBox textBox)
        {
            bool okay = true;
            textBox.Text.Trim();
            try
            {
                int i = int.Parse(textBox.Text, M.En_USNumberFormat);
            }
            catch
            {
                okay = false;
            }

            M.SetTextBoxErrorColorIfNotOkay(textBox, okay);
        }

        private void Page1TitleTextBox_Leave(object sender, EventArgs e)
        {
            SetSettingsHaveChanged();
        }
        private void Page1AuthorTextBox_Leave(object sender, EventArgs e)
        {
            SetSettingsHaveChanged();
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
            this._assistantComposerForm.BringToFront();
        }

        private void ConfirmButton_Click(object sender, EventArgs e)
        {
            _fsf.SetSettingsAreConfirmed(this, M.HasError(_allTextBoxes), ConfirmButton);
            if(!_isLoading)
            {
                _assistantComposerForm.UpdateMainFormState();
            }
        }
        #region RevertToSaved
        private void RevertToSavedButton_Click(object sender, EventArgs e)
        {
            Debug.Assert(((SavedState)this.Tag) == SavedState.unconfirmed || ((SavedState)this.Tag) == SavedState.confirmed);
            DialogResult result =
                MessageBox.Show("Are you sure you want to revert this form to the saved version?", "Revert?",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);

            if(result == DialogResult.Yes)
            {

                try
                {
                    using(XmlReader r = XmlReader.Create(_settingsPath))
                    {
                        M.ReadToXmlElementTag(r, "moritzKrystalScore");
                        Read(r); // _isLoading is true during read, but set to false at the end of the function.
                    }
                    _isLoading = true;
                    TouchAllTextBoxes();
                    _fsf.SetSettingsAreSaved(this, M.HasError(_allTextBoxes), ConfirmButton, RevertToSavedButton);
                    _isLoading = false;
                }
                catch(Exception ex)
                {
                    string msg = "Exception message:\n\n" + ex.Message;
                    MessageBox.Show(msg, "Error reading moritz krystal score settings", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
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

        public string Page1Title { get { return Page1TitleTextBox.Text; } }
        public string Page1Author { get { return Page1AuthorTextBox.Text; } }

        public float TitleHeight { get { return float.Parse(Page1TitleHeightTextBox.Text, M.En_USNumberFormat); } }
        public float AuthorHeight { get { return float.Parse(Page1AuthorHeightTextBox.Text, M.En_USNumberFormat); } }
        public float TitleY { get { return float.Parse(Page1TitleYTextBox.Text, M.En_USNumberFormat); } }

        public int TopMarginWidthPage1 { get { return int.Parse(TopMarginPage1TextBox.Text, M.En_USNumberFormat); } }
        public int TopMarginWidthOtherPages { get { return int.Parse(TopMarginOtherPagesTextBox.Text, M.En_USNumberFormat); } }
        public int RightMarginWidth { get { return int.Parse(RightMarginTextBox.Text, M.En_USNumberFormat); } }
        public int BottomMarginWidth { get { return int.Parse(BottomMarginTextBox.Text, M.En_USNumberFormat); } }
        public int LeftMarginWidth { get { return int.Parse(LeftMarginTextBox.Text, M.En_USNumberFormat); } }

        public string AboutLinkText { get { return AboutLinkTextTextBox.Text; } }
        public string AboutLinkURL { get { return AboutLinkURLTextBox.Text; } }
        #endregion public properties

        #region private variables
        private string _settingsPath;
        private AssistantComposerForm _assistantComposerForm = null;
        private List<TextBox> _allTextBoxes;
        private FormStateFunctions _fsf;
        private bool _isLoading;
        #endregion private variables
    }
}
