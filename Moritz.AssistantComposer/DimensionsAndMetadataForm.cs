using System;
using System.Windows.Forms;
using System.Xml;
using System.Diagnostics;
using System.Collections.Generic;
using System.Drawing;

using Moritz.Globals;

namespace Moritz.AssistantComposer
{
    public partial class DimensionsAndMetadataForm : Form
    {
        /// <summary>
        /// Creates a new, empty PalettesForm with help texts adjusted for the given krystal.
        /// </summary>
        /// <param name="assistantComposer"></param>
        /// <param name="krystal"></param>
        public DimensionsAndMetadataForm(AssistantComposerMainForm assistantComposerMainForm)
        {
            InitializeComponent();
            SetDefaultValues();

            _assistantComposerMainForm = assistantComposerMainForm;

            Text = _assistantComposerMainForm.Text + ": Dimensions and Metadata";
        }

        public bool HasError()
        {
            bool error = false;
            List<TextBox> textBoxes = GetAllTextBoxes();
            foreach(TextBox textBox in textBoxes)
            {
                if(textBox.BackColor == M.TextBoxErrorColor)
                {
                    error = true;
                    break;
                }
            }
            return error;
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
                M.ReadToXmlElementTag(r, "metadata", "dimensions", "notation", "names", "krystals", "palettes");
            }
            SetSettingsHaveBeenSaved();
            DeselectAll();
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

        private void DeselectAll()
        {
            bool settingsHaveBeenSaved = Text[Text.Length - 1] != '*';
            this.PaperSizeLabel.Focus();
            if(settingsHaveBeenSaved)
                SetSettingsHaveBeenSaved();
            else
                SetSettingsNotSaved();
        }

        private void DimensionsAndMetadataForm_Activated(object sender, EventArgs e)
        {
            bool mainFormHasBeenSaved = !this._assistantComposerMainForm.Text.EndsWith("*");
            this.DeselectAll();
            if(mainFormHasBeenSaved)
                _assistantComposerMainForm.SetSettingsHaveBeenSaved();
            else
                _assistantComposerMainForm.SetSettingsHaveNotBeenSaved();
        }
        /// <summary>
        /// Removes the '*' in Text, disables the SaveButton and informs _ornamentSettingsForm
        /// </summary>
        public void SetSettingsHaveBeenSaved()
        {
            if(this.Text.EndsWith("*"))
            {
                this.Text = this.Text.Remove(this.Text.Length - 1);
            }
            this.SaveSettingsButton.Enabled = false;
        }
        /// <summary>
        /// Sets the '*' in Text, enables the SaveButton and informs _assistantComposerMainForm
        /// </summary>
        public void SetSettingsNotSaved()
        {
            if(!this.Text.EndsWith("*"))
            {
                this.Text = this.Text + "*";
                if(this._assistantComposerMainForm != null)
                {
                    _assistantComposerMainForm.SetSettingsHaveNotBeenSaved();
                }
            }
            this.SaveSettingsButton.Enabled = true;
        }

        #region control events
        private void PaperSizeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetSettingsNotSaved();
            this.PaperSizeLabel.Focus();
        }

        private void LandscapeCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            SetSettingsNotSaved();
        }

        private void Page1TitleHeightTextBox_TextChanged(object sender, EventArgs e)
        {
            if(this.CheckTextBoxIsFloat(this.Page1TitleHeightTextBox))
                SetSettingsNotSaved();
        }
        private void Page1AuthorHeightTextBox_TextChanged(object sender, EventArgs e)
        {
            if(CheckTextBoxIsFloat(this.Page1AuthorHeightTextBox))
                SetSettingsNotSaved();
        }
        private void Page1TitleYTextBox_TextChanged(object sender, EventArgs e)
        {
            if(CheckTextBoxIsFloat(this.Page1TitleYTextBox))
                SetSettingsNotSaved();
        }

        private void TopMarginPage1TextBox_TextChanged(object sender, EventArgs e)
        {
            if(CheckTextBoxIsFloat(this.TopMarginPage1TextBox))
                SetSettingsNotSaved();
        }
        private void TopMarginOtherPagesTextBox_TextChanged(object sender, EventArgs e)
        {
            if(CheckTextBoxIsFloat(this.TopMarginOtherPagesTextBox))
                SetSettingsNotSaved();
        }
        private void RightMarginTextBox_TextChanged(object sender, EventArgs e)
        {
            if(CheckTextBoxIsFloat(this.RightMarginTextBox))
                SetSettingsNotSaved();
        }
        private void BottomMarginTextBox_TextChanged(object sender, EventArgs e)
        {
            if(CheckTextBoxIsFloat(this.BottomMarginTextBox))
                SetSettingsNotSaved();
        }
        private void LeftMarginTextBox_TextChanged(object sender, EventArgs e)
        {
            if(CheckTextBoxIsFloat(this.LeftMarginTextBox))
                SetSettingsNotSaved();
        }

        private bool CheckTextBoxIsFloat(TextBox textBox)
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
            return okay;
        }

        private void AboutLinkTextTextBox_TextChanged(object sender, EventArgs e)
        {
            SetSettingsNotSaved();
        }
        private void AboutLinkURLTextBox_TextChanged(object sender, EventArgs e)
        {
            SetSettingsNotSaved();
        }
        private void RecordingTextBox_TextChanged(object sender, EventArgs e)
        {
            SetSettingsNotSaved();
        }

        private void MetadataKeywordsTextBox_TextChanged(object sender, EventArgs e)
        {
            SetSettingsNotSaved();
        }

        private void MetadataCommentTextBox_TextChanged(object sender, EventArgs e)
        {
            SetSettingsNotSaved();
        }
        #endregion

        private void ShowMainScoreFormButton_Click(object sender, EventArgs e)
        {
            this._assistantComposerMainForm.BringToFront();
        }

        private void SaveSettingsButton_Click(object sender, EventArgs e)
        {
            this._assistantComposerMainForm.SaveSettings();
            SetSettingsHaveBeenSaved();
        }

        private AssistantComposerMainForm _assistantComposerMainForm = null;
    }
}
