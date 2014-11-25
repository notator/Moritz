using System;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Xml;
using System.Text.RegularExpressions;

//using Multimedia;
using Multimedia.Midi;

using Krystals4ObjectLibrary;
using Moritz.Globals;
using Moritz.Symbols;
using Moritz.Midi;
using Moritz.Spec;
using Moritz.Palettes;
using Moritz.Algorithm;
using Moritz.Algorithm.PaletteDemo;
using Moritz.Algorithm.SongSix;
using Moritz.Algorithm.Study2c3_1;
using Moritz.Algorithm.Study3Sketch1;
using Moritz.Algorithm.Study3Sketch2;

namespace Moritz.Composer
{
    public partial class AssistantComposerMainForm : Form
    {
        public AssistantComposerMainForm(string settingsPath, IMoritzForm1 moritzForm1)
        {
            InitializeComponent();

            _allTextBoxes = GetAllTextBoxes();
            _callbacks = GetCallbacks();

            _moritzForm1 = moritzForm1;

            M.PopulateComboBox(ChordTypeComboBox, M.ChordTypes);
            SetDefaultValues();
            DeselectAll();

            Debug.Assert(File.Exists(settingsPath));

            _settingsPath = settingsPath;
            _settingsFolderPath = Path.GetDirectoryName(settingsPath);

            _scoreTitle = Path.GetFileNameWithoutExtension(settingsPath);

            this.QuitAlgorithmButton.Text = "Quit " + _scoreTitle;

            _dimensionsAndMetadataForm = new DimensionsAndMetadataForm(this, _settingsPath);

            _algorithm = ComposableSvgScore.Algorithm(_scoreTitle);

            Debug.Assert(_algorithm != null);

            _outputVoiceIndices = GetOutputVoiceIndices(_algorithm.MidiChannelIndexPerOutputVoice.Count);
                
            SetScoreComboBoxItems(_settingsFolderPath);
            ScoreComboBox.SelectedIndexChanged -= ScoreComboBox_SelectedIndexChanged;
            ScoreComboBox.SelectedIndex = ScoreComboBox.Items.IndexOf(_scoreTitle);
            GetSelectedSettings();
            ScoreComboBox.SelectedIndexChanged += ScoreComboBox_SelectedIndexChanged;

            if(OutputVoiceIndicesStaffTextBox.Text == "")
            {
                SetDefaultVoiceIndicesPerStaff(_algorithm.MidiChannelIndexPerOutputVoice.Count);
            }

            this.Text = _scoreTitle + " algorithm";
        }
        #region DeselectAll
        private void DeselectAll()
        {
            DeselectAllKrystalListBoxItems();
            DeselectAllPalettesListBoxItems();
            this.Focus(); // deletes the dotted frame around the last selected item.
        }
        private void DeselectAllKrystalListBoxItems()
        {
            for(int i = 0; i < KrystalsListBox.Items.Count; i++)
            {
                KrystalsListBox.SetSelected(i, false);
            }
            RemoveSelectedKrystalButton.Enabled = false;
        }
        private void DeselectAllPalettesListBoxItems()
        {
            for(int i = 0; i < PalettesListBox.Items.Count; i++)
            {
                PalettesListBox.SetSelected(i, false);
            }
            DeleteSelectedPaletteButton.Enabled = false;
            ShowSelectedPaletteButton.Enabled = false;
        }
        #endregion DeselectAll


        #region ReviewableForm

        private void AssistantComposerMainForm_Activated(object sender, EventArgs e)
        {
            SetFormButtons();
        }

        private void SetFormButtons()
        {
            this.ShowUncheckedFormsButton.Enabled = (_rff.FormsThatNeedReview.Count > 0) ? true : false;
            this.ShowConfirmedFormsButton.Enabled = (_rff.ConfirmedForms.Count > 0) ? true : false;
            this.SaveSettingsCreateScoreButton.Enabled = (_rff.FormsThatNeedReview.Count > 0) ? false : true;
            this.SuspendLayout();
            if(_rff.ConfirmedForms.Count > 0)
            {
                if(! this.SaveSettingsCreateScoreButton.Text.StartsWith("save"))
                    this.SaveSettingsCreateScoreButton.Text = "save all settings";
            }
            else
            {
                if(! this.SaveSettingsCreateScoreButton.Text.StartsWith("create"))
                    this.SaveSettingsCreateScoreButton.Text = "create score";
            }
            this.ResumeLayout();
            this.RevertEverythingButton.Enabled = (_rff.FormsThatNeedReview.Count > 0 || _rff.ConfirmedForms.Count > 0);
        }
        private void SetSettingsNeedReview()
        {
            _rff.SetFormState(this, ReviewableState.needsReview);
            SetFormButtons();
            this.SaveSettingsCreateScoreButton.Enabled = false;
        }
        private void SetSettingsHaveChanged()
        {
            _rff.SetFormState(this, ReviewableState.hasChanged);
            SetFormButtons();
            if(!SaveSettingsCreateScoreButton.Text.StartsWith("save"))
                this.SaveSettingsCreateScoreButton.Text = "save all settings";
            this.SaveSettingsCreateScoreButton.Enabled = true;
            NotationGroupBox.Enabled = true;
            KrystalsGroupBox.Enabled = true;
            PalettesGroupBox.Enabled = true;
        }

        private void SetSettingsAreSaved()
        {
            _rff.SetFormState(this, ReviewableState.saved);
            SetFormButtons();
            if(!SaveSettingsCreateScoreButton.Text.StartsWith("create"))
                this.SaveSettingsCreateScoreButton.Text = "create score";
            this.SaveSettingsCreateScoreButton.Enabled = true;
            NotationGroupBox.Enabled = true;
            KrystalsGroupBox.Enabled = true;
            PalettesGroupBox.Enabled = true;
        }

        private void SetNotationPanelNeedsReview()
        {
            // sets this form's text and the argument buttons
            _rff.SetSettingsNeedReview(this, M.HasError(_allTextBoxes), ConfirmNotationButton, RevertNotationButton);
            NotationGroupBox.Tag = ReviewableState.needsReview;
            KrystalsGroupBox.Enabled = false;
            PalettesGroupBox.Enabled = false;
            SetSettingsNeedReview();
        }
        private void SetKrystalsPanelNeedsReview()
        {
            _rff.SetSettingsNeedReview(this, M.HasError(_allTextBoxes), ConfirmKrystalsButton, RevertKrystalsButton);
            KrystalsGroupBox.Tag = ReviewableState.needsReview;
            NotationGroupBox.Enabled = false;
            PalettesGroupBox.Enabled = false;
            SetSettingsNeedReview();
        }
        private void SetPalettesPanelNeedsReview()
        {
            _rff.SetSettingsNeedReview(this, M.HasError(_allTextBoxes), ConfirmPalettesButton, RevertPalettesButton);
            PalettesGroupBox.Tag = ReviewableState.needsReview;
            SetSettingsNeedReview();
            NotationGroupBox.Enabled = false;
            KrystalsGroupBox.Enabled = false;
        }

        /// <summary>
        /// Called when one of the groupBox OkaytoSaveButtons is clicked.
        /// </summary>
        private void SetThisFormsState(GroupBox groupBox, Button groupBoxConfirmButton, 
            ReviewableState otherGroupBox1State, ReviewableState otherGroupBox2State)
        {
            groupBox.Tag = ReviewableState.hasChanged;
            _rff.SetSettingsCanBeSaved(this, M.HasError(_allTextBoxes), groupBoxConfirmButton);

            if(otherGroupBox1State == ReviewableState.needsReview 
            || otherGroupBox2State == ReviewableState.needsReview
            || OtherFormStateIs(ReviewableState.needsReview))
                SetSettingsNeedReview();
            else
                SetSettingsHaveChanged();
        }

        private void ConfirmNotationButton_Click(object sender, EventArgs e)
        {
            SetThisFormsState(NotationGroupBox, ConfirmNotationButton, 
                (ReviewableState)KrystalsGroupBox.Tag, (ReviewableState)PalettesGroupBox.Tag);

        }
        private void ConfirmKrystalsButton_Click(object sender, EventArgs e)
        {
            SetThisFormsState(KrystalsGroupBox, ConfirmKrystalsButton, 
                (ReviewableState)NotationGroupBox.Tag, (ReviewableState)PalettesGroupBox.Tag);

        }
        private void ConfirmPalettesButton_Click(object sender, EventArgs e)
        {
            SetThisFormsState(PalettesGroupBox, ConfirmPalettesButton, 
                (ReviewableState)NotationGroupBox.Tag, (ReviewableState)KrystalsGroupBox.Tag);
        }

        private bool OtherFormStateIs(ReviewableState state)
        {
            bool rval = false;
            if((ReviewableState)_dimensionsAndMetadataForm.Tag == state)
                rval = true;
            else
            {
                foreach(PaletteForm paletteForm in PalettesListBox.Items)
                {
                    if((paletteForm.OrnamentSettingsForm != null
                    && (ReviewableState)paletteForm.OrnamentSettingsForm.Tag == state)
                    || ((ReviewableState)paletteForm.Tag == state))
                    {
                        rval = true;
                        break;
                    }
                }
            }
            return rval;
        }
        /// <summary>
        /// Called after the groupBox has been loaded or reverted, when one of the groupBox RevertToSavedButtons is clicked.
        /// </summary>
        private void SetThisFormsState(GroupBox groupBox, Button confirmButton, Button revertToSavedButton,
                        ReviewableState otherGroupBox1State, ReviewableState otherGroupBox2State)
        {
            groupBox.Tag = ReviewableState.saved;
            _rff.SetSettingsAreSaved(this, M.HasError(_allTextBoxes), confirmButton, revertToSavedButton);

            if(otherGroupBox1State == ReviewableState.needsReview || otherGroupBox2State == ReviewableState.needsReview
                || OtherFormStateIs(ReviewableState.needsReview))
                SetSettingsNeedReview();
            else if(otherGroupBox1State == ReviewableState.hasChanged || otherGroupBox2State == ReviewableState.hasChanged
                || OtherFormStateIs(ReviewableState.hasChanged))
                SetSettingsHaveChanged();
            else
                SetSettingsAreSaved();
        }

        private void SetAllSettingsHaveBeenReverted()
        {
            SetNotationPanelHasBeenReverted();
            SetKrystalsPanelHasBeenReverted();
            SetPalettesPanelHasBeenReverted();
            _rff.SetFormState(_dimensionsAndMetadataForm, ReviewableState.saved);
            foreach(PaletteForm paletteForm in PalettesListBox.Items)
            {
                if(paletteForm.OrnamentSettingsForm != null)
                    _rff.SetFormState(paletteForm.OrnamentSettingsForm, ReviewableState.saved);
                _rff.SetFormState(paletteForm, ReviewableState.saved);
            }
            this.SaveSettingsCreateScoreButton.Enabled = true;
            this.SaveSettingsCreateScoreButton.Text = "create score";
            this.RevertEverythingButton.Enabled = false;
        }
        private void SetNotationPanelHasBeenReverted()
        {
            SetThisFormsState(NotationGroupBox, ConfirmNotationButton, RevertNotationButton,
                (ReviewableState)KrystalsGroupBox.Tag, (ReviewableState)PalettesGroupBox.Tag);
        }
        private void SetKrystalsPanelHasBeenReverted()
        {
            SetThisFormsState(KrystalsGroupBox, ConfirmKrystalsButton, RevertKrystalsButton,
                (ReviewableState)NotationGroupBox.Tag, (ReviewableState)PalettesGroupBox.Tag);
        }
        private void SetPalettesPanelHasBeenReverted()
        {
            SetThisFormsState(PalettesGroupBox, ConfirmPalettesButton, RevertPalettesButton,
                (ReviewableState)NotationGroupBox.Tag, (ReviewableState)KrystalsGroupBox.Tag);
        }

        private void RevertNotationToSavedButton_Click(object sender, EventArgs e)
        {
            Debug.Assert(((ReviewableState)NotationGroupBox.Tag) == ReviewableState.needsReview || ((ReviewableState)NotationGroupBox.Tag) == ReviewableState.hasChanged);
            DialogResult result =
                MessageBox.Show("Are you sure you want to revert the notation panel to the saved version?",
                                "Revert?", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);

            if(result == System.Windows.Forms.DialogResult.Yes)
            {
                ReadNotation();
                SetNotationPanelHasBeenReverted();
            }
        }
        private void RevertKrystalsToSavedButton_Click(object sender, EventArgs e)
        {
            Debug.Assert(((ReviewableState)KrystalsGroupBox.Tag) == ReviewableState.needsReview || ((ReviewableState)KrystalsGroupBox.Tag) == ReviewableState.hasChanged);
            DialogResult result =
                MessageBox.Show("Are you sure you want to revert the krystals panel to the saved version?",
                                "Revert?", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);

            if(result == System.Windows.Forms.DialogResult.Yes)
            {
                ReadKrystals();
                SetKrystalsPanelHasBeenReverted();
            }
        }
        private void RevertPalettesButton_Click(object sender, EventArgs e)
        {
            Debug.Assert(((ReviewableState)PalettesGroupBox.Tag) == ReviewableState.needsReview || ((ReviewableState)PalettesGroupBox.Tag) == ReviewableState.hasChanged);
            DialogResult result =
                MessageBox.Show("Are you sure you want to close all palettes and revert them to their saved versions?",
                                "Revert?", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);

            if(result == System.Windows.Forms.DialogResult.Yes)
            {
                foreach(PaletteForm paletteForm in PalettesListBox.Items)
                {
                    paletteForm.Close();
                }
                ReadPalettes();
                SetPalettesPanelHasBeenReverted();
            }
        }
        private void ReadNotation()
        {
            try
            {
                using(XmlReader r = XmlReader.Create(_settingsPath))
                {
                    M.ReadToXmlElementTag(r, "moritzKrystalScore"); // check that this is a moritz preferences file
                    M.ReadToXmlElementTag(r, "notation");

                    while(r.Name == "notation")
                    {
                        if(r.NodeType != XmlNodeType.EndElement)
                        {
                            GetNotation(r);
                        }
                        M.ReadToXmlElementTag(r, "notation", "moritzKrystalScore");
                    }
                    Debug.Assert(r.Name == "moritzKrystalScore"); // end of krystal score
                }
            }
            catch(Exception ex)
            {
                string msg = "Exception message:\n\n" + ex.Message;
                MessageBox.Show(msg, "Error reading krystal score settings", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void ReadKrystals()
        {
            try
            {
                using(XmlReader r = XmlReader.Create(_settingsPath))
                {
                    M.ReadToXmlElementTag(r, "moritzKrystalScore"); // check that this is a moritz preferences file
                    M.ReadToXmlElementTag(r, "krystals");

                    while(r.Name == "krystals")
                    {
                        if(r.NodeType != XmlNodeType.EndElement)
                        {
                            GetKrystals(r);
                        }
                        M.ReadToXmlElementTag(r, "krystals", "moritzKrystalScore");
                    }
                    Debug.Assert(r.Name == "moritzKrystalScore"); // end of krystal score
                }
            }
            catch(Exception ex)
            {
                string msg = "Exception message:\n\n" + ex.Message;
                MessageBox.Show(msg, "Error reading krystal score settings", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void ReadPalettes()
        {
            try
            {
                using(XmlReader r = XmlReader.Create(_settingsPath))
                {
                    M.ReadToXmlElementTag(r, "moritzKrystalScore"); // check that this is a moritz preferences file
                    M.ReadToXmlElementTag(r, "palettes");

                    while(r.Name == "palettes")
                    {
                        if(r.NodeType != XmlNodeType.EndElement)
                        {
                            GetPalettes(r);
                        }
                        M.ReadToXmlElementTag(r, "palettes", "moritzKrystalScore");
                    }
                    Debug.Assert(r.Name == "moritzKrystalScore"); // end of krystal score
                }
            }
            catch(Exception ex)
            {
                string msg = "Exception message:\n\n" + ex.Message;
                MessageBox.Show(msg, "Error reading krystal score settings", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void RevertEverythingButton_Click(object sender, EventArgs e)
        {
            DialogResult result =
            MessageBox.Show("Are you sure you want to close all dependent forms and revert their settings to the saved version?",
                    "Revert?", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);

            if(result == System.Windows.Forms.DialogResult.Yes)
            {
                Reload();
            }
        }

        private void Reload()
        {
            _dimensionsAndMetadataForm.Close();
            _dimensionsAndMetadataForm = new DimensionsAndMetadataForm(this, _settingsPath);

            foreach(PaletteForm paletteForm in PalettesListBox.Items)
            {
                paletteForm.Close(); // closes any OrnamentSettingsForm and/or PaletteChordform
            }

            LoadSettings(); // clears existing settings
        }
        #endregion

        private void SetDefaultVoiceIndicesPerStaff(int nVoices)
        {
            StringBuilder voiceIndexList = new StringBuilder();
            for(int i = 0; i < nVoices; ++i)
            {
                voiceIndexList.Append(i.ToString());
                voiceIndexList.Append(", ");
            }
            voiceIndexList.Remove(voiceIndexList.Length - 2, 2);
            OutputVoiceIndicesStaffTextBox.Text = voiceIndexList.ToString();
            OutputVoiceIndicesStaffTextBox_Leave(null, null);
        }

        private void InitOutputVoiceIndicesPerStaffFields(int nVoices)
        {
            if(nVoices == 1)
            {
                OutputVoiceIndicesPerStaffHelpLabel.Text = "index: 0";
            }
            else
            {
                StringBuilder sb = new StringBuilder();
                for(int i = 0; i < nVoices; ++i)
                {
                    sb.Append(", ");
                    sb.Append(i.ToString());
                }
                sb.Remove(0, 2);
                sb.Insert(0, "indices: ");
                OutputVoiceIndicesPerStaffHelpLabel.Text = sb.ToString();
            }
        }
        private void InitInputVoiceIndicesPerStaffFields(int nInputVoices)
        {
            if(nInputVoices == 0)
            {
                InputVoiceIndicesPerStaffHelpLabel.Text = "This algorithm does not provide input voices.";
                InputVoiceIndicesPerStaffHelpLabel.Enabled = false;
                InputVoiceIndicesPerStaffLabel.Enabled = false;
                InputVoiceIndicesPerStaffTextBox.Enabled = false;
                InputVoiceIndicesPerStaffHelpLabel2.Enabled = false;
            }
            else
            {
                InputVoiceIndicesPerStaffHelpLabel.Enabled = true;
                InputVoiceIndicesPerStaffLabel.Enabled = true;
                InputVoiceIndicesPerStaffTextBox.Enabled = true;
                InputVoiceIndicesPerStaffHelpLabel2.Enabled = true;

                if(nInputVoices == 1)
                {
                    InputVoiceIndicesPerStaffHelpLabel.Text = "index: 0";
                }
                else
                {
                    StringBuilder sb = new StringBuilder();
                    for(int i = 0; i < nInputVoices; ++i)
                    {
                        sb.Append(", ");
                        sb.Append(i);
                    }
                    sb.Remove(0, 2);
                    sb.Insert(0, "indices: ");
                    InputVoiceIndicesPerStaffHelpLabel.Text = sb.ToString();
                }
            }
        }
        private void SetSystemStartBarsHelpLabel(int numberOfBars)
        {
            SystemStartBarsHelpLabel.Text = "(" + numberOfBars.ToString() + " bars. Default is 5 bars per system)";
        }

        private void LoadSettings()
        {
            try
            {
                ClearListBoxes();
                ReadFile();
            }
            catch(Exception ae)
            {
                MessageBox.Show(ae.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            SetAllSettingsHaveBeenReverted();
        }

        private void ClearListBoxes()
        {
            KrystalsListBox.SuspendLayout();
            KrystalsListBox.Items.Clear();
            KrystalsListBox.ResumeLayout();

            PalettesListBox.SuspendLayout();
            PalettesListBox.Items.Clear();
            PalettesListBox.ResumeLayout();
        }

        private List<TextBox> GetAllTextBoxes()
        {
            List<TextBox> textBoxes = new List<TextBox>();

            textBoxes.Add(MinimumGapsBetweenStavesTextBox);
            textBoxes.Add(MinimumGapsBetweenSystemsTextBox);
            textBoxes.Add(MinimumCrotchetDurationTextBox);
 
            textBoxes.Add(OutputVoiceIndicesStaffTextBox);
            textBoxes.Add(InputVoiceIndicesPerStaffTextBox);  
            textBoxes.Add(ClefsPerStaffTextBox);
            textBoxes.Add(StafflinesPerStaffTextBox);
            textBoxes.Add(StaffGroupsTextBox);

       
            textBoxes.Add(LongStaffNamesTextBox);
            textBoxes.Add(ShortStaffNamesTextBox);

            textBoxes.Add(SystemStartBarsTextBox);
            return textBoxes;
        }
        private void TouchAllTextBoxes()
        {
            MinimumGapsBetweenStavesTextBox_TextChanged(MinimumGapsBetweenStavesTextBox, null);
            MinimumGapsBetweenSystemsTextBox_TextChanged(MinimumGapsBetweenSystemsTextBox, null);
            MinimumCrotchetDurationTextBox_TextChanged(MinimumCrotchetDurationTextBox, null);
            OutputVoiceIndicesStaffTextBox_TextChanged(OutputVoiceIndicesStaffTextBox, null);
            InputVoiceIndicesPerStaffTextBox_TextChanged(InputVoiceIndicesPerStaffTextBox, null);
            ClefsPerStaffTextBox_TextChanged(ClefsPerStaffTextBox, null);
            StafflinesPerStaffTextBox_TextChanged(StafflinesPerStaffTextBox, null);
            StaffGroupsTextBox_TextChanged(StaffGroupsTextBox, null);
            LongStaffNamesTextBox_TextChanged(LongStaffNamesTextBox, null);
            ShortStaffNamesTextBox_TextChanged(ShortStaffNamesTextBox, null);
            SystemStartBarsTextBox_TextChanged(SystemStartBarsTextBox, null);
        }
        private void SetDefaultValues()
        {
            this.StafflineStemStrokeWidthComboBox.SelectedIndex = 0;
            this.GapPixelsComboBox.SelectedIndex = 0;
            this.MinimumGapsBetweenStavesTextBox.Text = "8";
            this.MinimumGapsBetweenSystemsTextBox.Text = "11";
            this.MinimumCrotchetDurationTextBox.Text = "800";

            ClearListBoxes();
        }

        private void ReadFile()
        {
            try
            {
                using(XmlReader r = XmlReader.Create(_settingsPath))
                {
                    M.ReadToXmlElementTag(r, "moritzKrystalScore"); // check that this is a moritz preferences file

                    _dimensionsAndMetadataForm.Read(r);

                    Debug.Assert(r.Name == "notation" || r.Name == "krystals" || r.Name == "palettes");

                    while(r.Name == "notation" || r.Name == "krystals" || r.Name == "palettes")
                    {
                        if(r.NodeType != XmlNodeType.EndElement)
                        {
                            switch(r.Name)
                            {
                                case "notation":
                                    GetNotation(r);
                                    break;
                                case "krystals":
                                    GetKrystals(r);
                                    break;
                                case "palettes":
                                    GetPalettes(r);
                                    break;
                            }
                        }
                        M.ReadToXmlElementTag(r, "notation", "krystals", "palettes", "moritzKrystalScore");
                    }
                    Debug.Assert(r.Name == "moritzKrystalScore"); // end of krystal score
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Error reading krystal score settings", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void GetNotation(XmlReader r)
        {
            Debug.Assert(r.Name == "notation");
            int count = r.AttributeCount;
            for(int i = 0; i < count; i++)
            {
                r.MoveToAttribute(i);
                switch(r.Name)
                {
                    case "chordSymbolType":
                        SetStandardChordsOptionsPanel(r.Value);
                        break;
                    case "minimumCrotchetDuration":
                        MinimumCrotchetDurationTextBox.Text = r.Value;
                        break;
                    case "beamsCrossBarlines":
                        SetBeamsCrossBarlinesCheckBoxChecked(r.Value);
                        break;
                    case "stafflineStemStrokeWidth":
                        SetStafflineStemStrokeWidth(r.Value);
                        break;
                    case "gap":
                        SetGapPixelsComboBox(r.Value);
                        break;
                    case "minGapsBetweenStaves":
                        MinimumGapsBetweenStavesTextBox.Text = r.Value;
                        break;
                    case "minGapsBetweenSystems":
                        MinimumGapsBetweenSystemsTextBox.Text = r.Value;
                        break;
                    case "outputVoicesPerVoicePerStaff":
                        OutputVoiceIndicesStaffTextBox.Text = r.Value;
                        break;
                    case "inputVoicesPerVoicePerStaff":
                        InputVoiceIndicesPerStaffTextBox.Text = r.Value;
                        break;
                    case "clefsPerStaff":
                        ClefsPerStaffTextBox.Text = r.Value;
                        break;
                    case "stafflinesPerStaff":
                        StafflinesPerStaffTextBox.Text = r.Value;
                        break;
                    case "staffGroups":
                        StaffGroupsTextBox.Text = r.Value;
                        break;
                    case "longStaffNames":
                        LongStaffNamesTextBox.Text = r.Value;
                        break;
                    case "shortStaffNames":
                        ShortStaffNamesTextBox.Text = r.Value;
                        break;
                    case "systemStartBars":
                        SystemStartBarsTextBox.Text = r.Value;
                        break;
                }
            }
            _rff.SetSettingsAreSaved(this, false, ConfirmNotationButton, RevertNotationButton);
            this.NotationGroupBox.Tag = ReviewableState.saved;
        }

        private void SetGapPixelsComboBox(string value)
        {
            int item = 0;
            do
            {
                GapPixelsComboBox.SelectedIndex = item++;
            } while(value != GapPixelsComboBox.SelectedItem.ToString());
        }

        private void SetStafflineStemStrokeWidth(string value)
        {
            int item = 0;
            do
            {
                StafflineStemStrokeWidthComboBox.SelectedIndex = item++;
            } while(value != StafflineStemStrokeWidthComboBox.SelectedItem.ToString());
        }

        private void SetBeamsCrossBarlinesCheckBoxChecked(string value)
        {
            if(value == "true")
                BeamsCrossBarlinesCheckBox.Checked = true;
            else
                BeamsCrossBarlinesCheckBox.Checked = false;
        }

        private void SetStandardChordsOptionsPanel(string value)
        {
            ChordTypeComboBox.SelectedItem = value;
            StandardChordsOptionsPanel.Visible = false;
            switch(value)
            {
                case "standard":
                    StandardChordsOptionsPanel.Visible = true;
                    break;
                case "study2b2":
                    break;
            }
        }

        /// <summary>
        /// Loads the combo box with the names of all the scores which use this algorithm.
        /// The names are the root names of all .mkss files in subdirectories of the
        /// algorithmFolderPath.
        /// </summary>
        /// <param name="algorithmFolderPath"></param>
        private void SetScoreComboBoxItems(string algorithmFolderPath)
        {
            List<string> scoreNames = M.ScoreNames(algorithmFolderPath);
            ScoreComboBox.SelectedIndexChanged -= ScoreComboBox_SelectedIndexChanged;
            ScoreComboBox.SuspendLayout();
            ScoreComboBox.Items.Clear();
            foreach(string scoreName in scoreNames)
            {
                ScoreComboBox.Items.Add(scoreName);
            }
            ScoreComboBox.ResumeLayout();
            ScoreComboBox.SelectedIndex = 0;
            ScoreComboBox.SelectedIndexChanged += ScoreComboBox_SelectedIndexChanged;
        }

        private void GetKrystals(XmlReader r)
        {
            Debug.Assert(r.Name == "krystals");

            M.ReadToXmlElementTag(r, "krystal");
            this.KrystalsListBox.SuspendLayout();
            this.KrystalsListBox.Items.Clear();
            while(r.Name == "krystal")
            {
                if(r.NodeType != XmlNodeType.EndElement)
                {
                    r.MoveToAttribute(0);
                    string krystalName = r.Value;
                    Krystal krystal = GetKrystal(krystalName);
                    this.KrystalsListBox.Items.Add(krystal);
                }
                M.ReadToXmlElementTag(r, "krystal", "krystals");
            }
            this.KrystalsListBox.ResumeLayout();
            _rff.SetSettingsAreSaved(this, false, ConfirmKrystalsButton, RevertKrystalsButton);
            this.KrystalsGroupBox.Tag = ReviewableState.saved;
        }

        private Krystal GetKrystal(string krystalFileName)
        {
            Krystal krystal = null;
            try
            {
                string krystalPath = M.Preferences.LocalMoritzKrystalsFolder + @"\" + krystalFileName;
                krystal = K.LoadKrystal(krystalPath);
            }
            catch(Exception ex)
            {
                MessageBox.Show("Error loading krystal.\n\n" + ex.Message);
                krystal = null;
            }
            return krystal;
        }

        private void GetPalettes(XmlReader r)
        {
            Debug.Assert(r.Name == "palettes");
            M.ReadToXmlElementTag(r, "palette");
            this.PalettesListBox.SuspendLayout();
            this.PalettesListBox.Items.Clear();
            while(r.Name == "palette")
            {
                if(r.NodeType != XmlNodeType.EndElement)
                {
                    string name = "";
                    int domain = 1;
                    bool isPercussionPalette = false;

                    int count = r.AttributeCount;
                    for(int i = 0; i < count; i++)
                    {
                        r.MoveToAttribute(i);
                        switch(r.Name)
                        {
                            case "name":
                                name = r.Value;
                                break;
                            case "domain":
                                domain = int.Parse(r.Value);
                                break;
                            case "percussion":
                                if(r.Value == "1")
                                    isPercussionPalette = true;
                                break;
                        }
                    }

                    PaletteForm paletteForm = new PaletteForm(r, name, domain, _callbacks, isPercussionPalette);

                    PalettesListBox.Items.Add(paletteForm);

                    M.ReadToXmlElementTag(r, "palette", "palettes");
                }
            }
            this.PalettesListBox.ResumeLayout();
            Debug.Assert(r.Name == "palettes");
            _rff.SetSettingsAreSaved(this, false, ConfirmPalettesButton, RevertPalettesButton);
            this.PalettesGroupBox.Tag = ReviewableState.saved;
        }

        private ComposerFormCallbacks GetCallbacks()
        {
            ComposerFormCallbacks callbacks = new ComposerFormCallbacks();
            callbacks.BringMainFormToFront = BringThisFormToFront;
            callbacks.SettingsPath = GetSettingsPath;
            callbacks.LocalScoreAudioPath = this.GetLocalScoreAudioPath;
            callbacks.APaletteChordFormIsOpen = this.APaletteChordFormIsOpen;
            return callbacks;
        }

        private void BringThisFormToFront()
        {
            this.Enabled = true;
            this.BringToFront();
        }

        private bool APaletteChordFormIsOpen()
        {
            bool aPaletteChordFormIsOpen = false;
            foreach(PaletteForm paletteForm in PalettesListBox.Items)
            {
                if(paletteForm.HasOpenChordForm)
                {
                    aPaletteChordFormIsOpen = true;
                    break;
                }
            }
            return aPaletteChordFormIsOpen;
        }
        private string GetSettingsPath()
        {
            return _settingsPath;
        }
        private string GetLocalScoreAudioPath()
        {
            string path = M.Preferences.LocalMoritzAudioFolder + @"\" + _scoreTitle;
            return path;
        }

        #region buttons
        public void SaveSettingsCreateScoreButton_Click(object sender, EventArgs e)
        {
            if(SaveSettingsCreateScoreButton.Text.StartsWith("save"))
            {
                try
                {
                    // The settings are only saved (and reloaded) here in case there is going to be an error while creating the score.
                    SaveSettings(); 
                }
                catch(Exception ex)
                {
                    string msg = "Failed to save the settings.\r\n\r\n"
                        + "Exception message: " + ex.Message;
                    MessageBox.Show(msg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
            }
            else
            {
                Debug.Assert(SaveSettingsCreateScoreButton.Text.StartsWith("create"));
                try
                {
                    CreateSVGScore();
                }
                catch(Exception ex)
                {
                    string msg = "Failed to create the score.\r\n\r\n"
                        + "Exception message: " + ex.Message;
                    MessageBox.Show(msg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
            }
        }

        #region save settings
        public void SaveSettings()
        {
            Debug.Assert(!string.IsNullOrEmpty(_settingsPath));

            Point dimensionsAndMetadataFormLocation = GetDimensionsAndMetadataFormLocation();
            Dictionary<int, Point> visiblePaletteFormLocations = GetVisiblePaletteFormLocations();
            Dictionary<int, Point> visibleOrnamentFormLocations = GetVisibleOrnamentFormLocations();

            M.CreateDirectoryIfItDoesNotExist(this._settingsFolderPath);

            #region do the save
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.IndentChars = ("\t");
            settings.NewLineOnAttributes = true;
            settings.CloseOutput = false;
            using(XmlWriter w = XmlWriter.Create(_settingsPath, settings))
            {
                w.WriteStartDocument();
                w.WriteComment("file created: " + M.NowString);

                w.WriteStartElement("moritzKrystalScore");
                w.WriteAttributeString("xmlns", "xsi", null, "http://www.w3.org/2001/XMLSchema-instance");
                w.WriteAttributeString("xsi", "noNamespaceSchemaLocation", null, M.OnlineXMLSchemasFolder + "/moritzKrystalScore.xsd");

                _dimensionsAndMetadataForm.Write(w);

                WriteNotation(w);
                WriteKrystals(w);
                WritePalettes(w);
                w.WriteEndElement(); // closes the moritzKrystalScore element
                w.Close(); // close unnecessary because of the using statement?
            }
            #endregion do the save

            Reload(); // important: reloads the values used for reverting palettes, ornaments and dimensionsAndMetadata forms

            ReshowForms(dimensionsAndMetadataFormLocation, visiblePaletteFormLocations, visibleOrnamentFormLocations);
        }

        /// <summary>
        /// Returns Point(int.MinVal, int.MinVal) if the form is not visible
        /// </summary>
        /// <returns></returns>
        private Point GetDimensionsAndMetadataFormLocation()
        {
            Point location;
            if(_dimensionsAndMetadataForm.Visible)
            {
                location = new Point(_dimensionsAndMetadataForm.Location.X, _dimensionsAndMetadataForm.Location.Y);
            }
            else
            {
                location = new Point(int.MinValue, int.MinValue);
            }
            return location;
        }
        /// <summary>
        /// Returns the index (in PalettesListBox.Items) and location of all visible ornamentForms
        /// </summary>
        private Dictionary<int, Point> GetVisibleOrnamentFormLocations()
        {
            Dictionary<int, Point> indexAndLocation = new Dictionary<int,Point>();
            for(int i = 0; i < PalettesListBox.Items.Count; ++i)
            {
                PaletteForm paletteForm = PalettesListBox.Items[i] as PaletteForm;
                if(paletteForm.OrnamentSettingsForm != null && paletteForm.OrnamentSettingsForm.Visible)
                {
                    indexAndLocation.Add(i, paletteForm.OrnamentSettingsForm.Location);
                }
            }
            return indexAndLocation;
        }
        /// <summary>
        /// Returns the index in PalettesListBox.Items and location of all visible paletteForms
        /// </summary>
        private Dictionary<int, Point> GetVisiblePaletteFormLocations()
        {
            Dictionary<int, Point> indexAndLocation = new Dictionary<int, Point>();
            for(int i = 0; i < PalettesListBox.Items.Count; ++i)
            {
                PaletteForm paletteForm = PalettesListBox.Items[i] as PaletteForm;
                if(paletteForm.Visible)
                    indexAndLocation.Add(i, paletteForm.Location);
            }
            return indexAndLocation;
        }
        private void ReshowForms(Point dimensionsAndMetadataFormLocation, Dictionary<int, Point> visiblePaletteFormLocations, Dictionary<int, Point> visibleOrnamentFormLocations)
        {
            if(dimensionsAndMetadataFormLocation.X > int.MinValue)
            {
                _dimensionsAndMetadataForm.Show();
                _dimensionsAndMetadataForm.Location = dimensionsAndMetadataFormLocation;
            }
            else
                _dimensionsAndMetadataForm.Hide();

            for(int i = 0; i < PalettesListBox.Items.Count; ++i)
            {
                PaletteForm paletteForm = PalettesListBox.Items[i] as PaletteForm;
                if(visiblePaletteFormLocations.ContainsKey(i))
                {
                    paletteForm.Show();
                    paletteForm.Location = visiblePaletteFormLocations[i];
                }
                else
                {
                    paletteForm.Hide();
                }
                if(visibleOrnamentFormLocations.ContainsKey(i))
                {
                    Debug.Assert(paletteForm.OrnamentSettingsForm != null);  
                    paletteForm.OrnamentSettingsForm.Show();
                    paletteForm.OrnamentSettingsForm.Location = visibleOrnamentFormLocations[i];
                }
                else if(paletteForm.OrnamentSettingsForm != null)
                {
                    paletteForm.OrnamentSettingsForm.Hide();
                }
            }
        }

        private void WriteNotation(XmlWriter w)
        {
            w.WriteStartElement("notation");
            w.WriteAttributeString("chordSymbolType", ChordTypeComboBox.Text);
            if(StandardChordsOptionsPanel.Visible)
            {
                w.WriteAttributeString("minimumCrotchetDuration", MinimumCrotchetDurationTextBox.Text);
                if(BeamsCrossBarlinesCheckBox.Checked)
                    w.WriteAttributeString("beamsCrossBarlines", "true");
                else
                    w.WriteAttributeString("beamsCrossBarlines", "false");
            }
            w.WriteAttributeString("stafflineStemStrokeWidth", StafflineStemStrokeWidthComboBox.Text);
            w.WriteAttributeString("gap", GapPixelsComboBox.Text);
            w.WriteAttributeString("minGapsBetweenStaves", this.MinimumGapsBetweenStavesTextBox.Text);
            w.WriteAttributeString("minGapsBetweenSystems", MinimumGapsBetweenSystemsTextBox.Text);
            w.WriteAttributeString("outputVoicesPerVoicePerStaff", OutputVoiceIndicesStaffTextBox.Text);
            w.WriteAttributeString("inputVoicesPerVoicePerStaff", InputVoiceIndicesPerStaffTextBox.Text);
            w.WriteAttributeString("clefsPerStaff", ClefsPerStaffTextBox.Text);
            w.WriteAttributeString("stafflinesPerStaff", StafflinesPerStaffTextBox.Text);
            w.WriteAttributeString("staffGroups", StaffGroupsTextBox.Text);
            w.WriteAttributeString("longStaffNames", LongStaffNamesTextBox.Text);
            w.WriteAttributeString("shortStaffNames", ShortStaffNamesTextBox.Text);
            w.WriteAttributeString("systemStartBars", SystemStartBarsTextBox.Text);
            w.WriteEndElement(); // notation
        }
        private void WriteKrystals(XmlWriter w)
        {
            if(this.KrystalsListBox.Items.Count > 0)
            {
                w.WriteStartElement("krystals");

                foreach(object o in KrystalsListBox.Items)
                {
                    Krystal krystal = o as Krystal;
                    if(krystal != null)
                    {
                        w.WriteStartElement("krystal");
                        w.WriteAttributeString("name", krystal.Name);
                        w.WriteEndElement();// krystal
                    }
                }
                w.WriteEndElement(); // krystals
            }
        }
        private void WritePalettes(XmlWriter w)
        {
            if(PalettesListBox.Items.Count > 0)
            {
                w.WriteStartElement("palettes");

                foreach(PaletteForm paletteForm in PalettesListBox.Items)
                {
                    paletteForm.WritePalette(w);
                }
                w.WriteEndElement(); // palettes
            }
        }
        #endregion save settings

        /// <summary>
        /// This function
        /// 1) creates the score, 
        /// 2) opens it in the program currently set in Windows for opening SVG files.
        /// </summary>
        private void CreateSVGScore()
        {
            try
            {
                List<Krystal> krystals = null;
                List<Palette> palettes = null;
                GetKrystalsAndPalettes(out krystals, out palettes);
                PageFormat pageFormat = GetPageFormat();
                ComposableSvgScore score =
                    new KrystalPaletteScore(_scoreTitle,
                                            _algorithm,
                                            pageFormat,
                                            krystals, palettes,
                                            _settingsFolderPath,
                                            _dimensionsAndMetadataForm.Keywords,
                                            _dimensionsAndMetadataForm.Comment);

                if(score != null && score.Systems.Count > 0)
                {
                    score.SaveSVGScore();
                    // Opens the score in the program which is set by the system to open .svg files.
                    global::System.Diagnostics.Process.Start(score.FilePath);
                }
            }
            catch(Exception ex)
            {
                string msg = "Failed to create score, or to open it in the browser.\r\n\r\n"
                    + "Exception message: " + ex.Message;
                MessageBox.Show(msg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        #region PageFormat
        private PageFormat GetPageFormat()
        {
            PageFormat pageFormat = new PageFormat();
            SetPaperSize(pageFormat);

            SetTitleSizeAndPosition(pageFormat);

            SetFrame(pageFormat);

            SetWebsiteLink(pageFormat);

            SetNotation(pageFormat);

            return pageFormat;
        }
        private void SetPaperSize(PageFormat pageFormat)
        {
            pageFormat.PaperSize = _dimensionsAndMetadataForm.PaperSize;
            pageFormat.IsLandscape = _dimensionsAndMetadataForm.Landscape;
            float bottomPX;
            float rightPX;
            Debug.Assert(Regex.Matches(pageFormat.PaperSize, @"^(A4|B4|A5|B5|A3|Letter|Legal|Tabloid)$") != null);
            if(pageFormat.IsLandscape == true)
            {
                bottomPX = (int)(pageFormat.VerticalPixelsPerMillimeter * M.PaperSizes[pageFormat.PaperSize].ShortDimension_MM);
                rightPX = (int)(pageFormat.HorizontalPixelsPerMillimeter * M.PaperSizes[pageFormat.PaperSize].LongDimension_MM);
            }
            else
            {
                rightPX = (int)(pageFormat.HorizontalPixelsPerMillimeter * M.PaperSizes[pageFormat.PaperSize].ShortDimension_MM);
                bottomPX = (int)(pageFormat.VerticalPixelsPerMillimeter * M.PaperSizes[pageFormat.PaperSize].LongDimension_MM);
            }
            pageFormat.RightVBPX = rightPX * pageFormat.ViewBoxMagnification;
            pageFormat.BottomVBPX = bottomPX * pageFormat.ViewBoxMagnification;
        }
        private void SetTitleSizeAndPosition(PageFormat pageFormat)
        {
            DimensionsAndMetadataForm damf = _dimensionsAndMetadataForm;
            pageFormat.Page1TitleHeight = damf.TitleHeight * pageFormat.ViewBoxMagnification;
            pageFormat.Page1AuthorHeight = damf.AuthorHeight * pageFormat.ViewBoxMagnification;
            pageFormat.Page1TitleY = damf.TitleY * pageFormat.ViewBoxMagnification;
        }
        private void SetFrame(PageFormat pageFormat)
        {
            DimensionsAndMetadataForm damf = _dimensionsAndMetadataForm;
            pageFormat.TopMarginPage1 = damf.TopMarginWidthPage1 * pageFormat.ViewBoxMagnification;
            pageFormat.TopMarginOtherPages = damf.TopMarginWidthOtherPages * pageFormat.ViewBoxMagnification;
            pageFormat.RightMarginPos = pageFormat.RightVBPX - (damf.RightMarginWidth * pageFormat.ViewBoxMagnification);
            pageFormat.LeftMarginPos = damf.LeftMarginWidth * pageFormat.ViewBoxMagnification;
            pageFormat.BottomMarginPos = pageFormat.BottomVBPX - (damf.BottomMarginWidth * pageFormat.ViewBoxMagnification);
        }
        private void SetWebsiteLink(PageFormat pageFormat)
        {
            DimensionsAndMetadataForm damf = _dimensionsAndMetadataForm;
            pageFormat.AboutLinkText = damf.AboutLinkText;
            pageFormat.AboutLinkURL = damf.AboutLinkURL;
        }

        private void SetNotation(PageFormat pageFormat)
        {
            pageFormat.ChordSymbolType = this.ChordTypeComboBox.SelectedItem.ToString();
            pageFormat.MinimumCrotchetDuration = int.Parse(this.MinimumCrotchetDurationTextBox.Text);
            pageFormat.BeamsCrossBarlines = this.BeamsCrossBarlinesCheckBox.Checked;

            float strokeWidth = float.Parse(StafflineStemStrokeWidthComboBox.Text, M.En_USNumberFormat) * pageFormat.ViewBoxMagnification;
            pageFormat.StafflineStemStrokeWidth = strokeWidth;
            pageFormat.Gap = float.Parse(GapPixelsComboBox.Text, M.En_USNumberFormat) * pageFormat.ViewBoxMagnification;
            pageFormat.DefaultDistanceBetweenStaves = int.Parse(MinimumGapsBetweenStavesTextBox.Text) * pageFormat.Gap;
            pageFormat.DefaultDistanceBetweenSystems = int.Parse(MinimumGapsBetweenSystemsTextBox.Text) * pageFormat.Gap;

            pageFormat.OutputVoiceIndicesPerStaff = _outputVoiceIndicesPerStaff; // one value per output staff
            pageFormat.InputVoiceIndicesPerStaff = _inputVoiceIndicesPerStaff; // one value per input staff
            pageFormat.ClefsList = M.StringToStringList(this.ClefsPerStaffTextBox.Text, ',');
            pageFormat.StafflinesPerStaff = M.StringToIntList(this.StafflinesPerStaffTextBox.Text, ',');
            pageFormat.StaffGroups = M.StringToIntList(this.StaffGroupsTextBox.Text, ',');

            pageFormat.LongStaffNames = M.StringToStringList(this.LongStaffNamesTextBox.Text, ',');
            pageFormat.ShortStaffNames = M.StringToStringList(this.ShortStaffNamesTextBox.Text, ',');
            pageFormat.SystemStartBars = M.StringToIntList(SystemStartBarsTextBox.Text, ',');
        }
        #endregion


        private void GetKrystalsAndPalettes(out List<Krystal> krystals, out List<Palette> palettes)
        {
            krystals = new List<Krystal>();
            palettes = new List<Palette>();
            foreach(Krystal krystal in KrystalsListBox.Items)
            {
                krystals.Add(krystal);
            }
            foreach(PaletteForm paletteForm in PalettesListBox.Items)
            {
                palettes.Add(new Palette(paletteForm));
            }
        }

        private void ShowMoritzButton_Click(object sender, EventArgs e)
        {
            _moritzForm1.Show();

        }
        private void DimensionsAndMetadataButton_Click(object sender, EventArgs e)
        {
            _dimensionsAndMetadataForm.Enabled = true;
            _dimensionsAndMetadataForm.Show();
            _dimensionsAndMetadataForm.BringToFront();
        }

        private void QuitMoritzButton_Click(object sender, EventArgs e)
        {
            CheckSaved();
            _moritzForm1.Close();
        }
        private void QuitAssistantComposerButton_Click(object sender, EventArgs e)
        {
            CheckSaved();
            _moritzForm1.CloseAssistantComposer();
        }

        private void CheckSaved()
        {
            if(SaveSettingsCreateScoreButton.Enabled && SaveSettingsCreateScoreButton.Text.StartsWith("save"))
            {
                DialogResult result = MessageBox.Show("Save settings?", "Save?", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if(result == DialogResult.Yes)
                {
                    SaveSettings();
                }
            }
        }

        #region AssistantComposerMainForm_Activated
        private void ShowUncheckedFormsButton_Click(object sender, EventArgs e)
        {
            Debug.Assert(_rff.FormsThatNeedReview.Count > 0);

            Point location = new Point(200,25);
            for(int i = 0; i < _rff.FormsThatNeedReview.Count; ++i )
            {
                int offset = i * 25;
                Form form = _rff.FormsThatNeedReview[i] as Form;
                form.Location = new Point(location.X + offset, location.Y + offset);
                form.Show();
                form.BringToFront();
            }
        }

        private void ShowConfirmedFormsButton_Click(object sender, EventArgs e)
        {
            Debug.Assert(_rff.ConfirmedForms.Count > 0);
            Point location = new Point(200, 200);
            for(int i = 0; i < _rff.ConfirmedForms.Count; ++i )
            {
                int offset = i * 25;
                Form form = (_rff.ConfirmedForms[i] as Form);
                form.Location = new Point(location.X + offset, location.Y + offset);
                form.Show();
                form.BringToFront();
            }
        }   
        #endregion

        private void AssistantComposerMainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            foreach(PaletteForm paletteForm in PalettesListBox.Items)
            {
                paletteForm.Close();
            }

            if(_dimensionsAndMetadataForm != null)
            {
                _dimensionsAndMetadataForm.Close();
            }
        }

        #endregion buttons

        private void BeamsCrossBarlinesCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            SetNotationPanelNeedsReview();
        }
        #region list box events and functions
        private void KrystalsListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(KrystalsListBox.SelectedIndex >= 0)
            {
                ShowSelectedKrystalButton.Enabled = true;
                RemoveSelectedKrystalButton.Enabled = true;
            }
            else
            {
                ShowSelectedKrystalButton.Enabled = false;
                RemoveSelectedKrystalButton.Enabled = false;
            }

            ListBox listBox = sender as ListBox;
            listBox.Refresh();
        }
        private void KrystalsListBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            DoSelectionColor(sender, e);
        }
        private void DoSelectionColor(object sender, DrawItemEventArgs e)
        {
            ListBox listBox = sender as ListBox;
            Graphics g = e.Graphics;

            if(listBox != null && listBox.Items.Count > e.Index)
            {
                string text = listBox.Items[e.Index].ToString();
                Point textOrigin = new Point(e.Bounds.Left, e.Bounds.Top);

                e.DrawBackground();

                if(listBox.SelectedIndex == e.Index)
                {
                    g.FillRectangle(_systemHighlightBrush, e.Bounds);
                    g.DrawString(text, e.Font, _whiteBrush, textOrigin);
                }
                else
                {
                    g.FillRectangle(_whiteBrush, e.Bounds);
                    g.DrawString(text, e.Font, _blackBrush, textOrigin);
                }
                e.DrawFocusRectangle();
            }
        }

        private void AddKrystalButton_Click(object sender, EventArgs e)
        {
            Krystal selectedKrystal = null;
            if(KrystalsListBox.SelectedIndex >= 0)
                selectedKrystal = KrystalsListBox.SelectedItem as Krystal;
            Moritz.Krystals.KrystalBrowser krystalBrowser = new Moritz.Krystals.KrystalBrowser(selectedKrystal, M.Preferences.LocalMoritzKrystalsFolder, SetKrystal);
            krystalBrowser.Show();
            // the krystalBrowser calls SetKrystal() as a delegate just before it closes.
        }
        /// <summary>
        /// Called as a delegate by a krystalBrowser just before it closes.
        /// The current krystal name in the browser is passed to this function.
        /// </summary>
        /// <param name="krystalname"></param>
        private void SetKrystal(Krystal newKrystal)
        {
            List<Krystal> allKrystals = AllKrystals;

            bool krystalAlreadyPresent = false;
            foreach(Krystal krystal in allKrystals)
            {
                if(krystal.Name == newKrystal.Name)
                {
                    krystalAlreadyPresent = true;
                    break;
                }
            }
            if(krystalAlreadyPresent)
            {
                MessageBox.Show(newKrystal.Name + "is already in the score.", "krystal already present", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                string staffKrystalPath = M.Preferences.LocalMoritzKrystalsFolder + @"\" + newKrystal.Name;
                Krystal krystal = K.LoadKrystal(staffKrystalPath);
                this.KrystalsListBox.SuspendLayout();
                this.KrystalsListBox.Items.Add(krystal);
                this.KrystalsListBox.ResumeLayout();
                SetKrystalsPanelNeedsReview();

                KrystalsListBox.SetSelected(KrystalsListBox.Items.Count - 1, true); // triggers KrystalsListBox_SelectedIndexChanged()
            }

            this.BringToFront();
        }

        #region krystal buttons
        private void ShowSelectedKrystalButton_Click(object sender, EventArgs e)
        {
            Krystal krystal = this.KrystalsListBox.SelectedItem as Krystal;
            if(krystal != null)
            {
                _krystalBrowser =
                    new Moritz.Krystals.KrystalBrowser(krystal, M.Preferences.LocalMoritzKrystalsFolder, null);
                _krystalBrowser.Show();
            }
        }

        private void RemoveSelectedKrystalButton_Click(object sender, EventArgs e)
        {
            if(KrystalsListBox.SelectedIndex >= 0)
            {
                Krystal selectedKrystal = KrystalsListBox.SelectedItem as Krystal;
                string msg = "The krystal\n\n     " + selectedKrystal.Name +
                    "\n\nwill be removed from this score.\n" +
                    "It will not be deleted from the main krystals folder.\n\n" +
                    "Proceed?\n\n";
                DialogResult proceed = MessageBox.Show(msg, "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if(proceed == DialogResult.Yes)
                {
                    KrystalsListBox.Items.RemoveAt(KrystalsListBox.SelectedIndex);
                    SetKrystalsPanelNeedsReview();
                }
            }
        }

        private List<Krystal> AllKrystals
        {
            get
            {
                List<Krystal> allKrystals = new List<Krystal>();
                foreach(object o in KrystalsListBox.Items)
                {
                    Krystal krystal = o as Krystal;
                    if(krystal != null)
                    {
                        allKrystals.Add(krystal);
                    }
                }
                return allKrystals;
            }
        }

        private List<PaletteForm> CurrentPaletteForms
        {
            get
            {
                List<PaletteForm> currentPalletForms = new List<PaletteForm>();

                foreach(object o in PalettesListBox.Items)
                {
                    PaletteForm ipf = o as PaletteForm;
                    currentPalletForms.Add(ipf);
                }
                return currentPalletForms;
            }
            set
            {
                PalettesListBox.SuspendLayout();
                PalettesListBox.Items.Clear();
                foreach(PaletteForm ipf in value)
                {
                    PalettesListBox.Items.Add(ipf);
                }
                PalettesListBox.ResumeLayout();
            }
        }
        #endregion krystalButtons

        #region palette buttons
        private void PalettesListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(PalettesListBox.SelectedIndex >= 0)
            {
                DeleteSelectedPaletteButton.Enabled = true;
                ShowSelectedPaletteButton.Enabled = true;
            }
            else
            {
                DeleteSelectedPaletteButton.Enabled = false;
                ShowSelectedPaletteButton.Enabled = false;
            }

            ListBox listBox = sender as ListBox;
            listBox.Refresh();
        }
        private void ShowSelectedPaletteButton_Click(object sender, EventArgs e)
        {
            int selectedIndex = -1;
            if(PalettesListBox.SelectedIndices.Count > 0)
                selectedIndex = PalettesListBox.SelectedIndices[0];
            if(selectedIndex >= 0)
            {
                List<PaletteForm> currentForms = CurrentPaletteForms;
                currentForms[selectedIndex].Enabled = true;
                currentForms[selectedIndex].Show();
                currentForms[selectedIndex].BringToFront();
            }
        }
        private void AddPaletteButton_Click(object sender, EventArgs e)
        {            
            // Palette domains are limited by the width of the palette forms.
            // There has to be enough space for the demo buttons.
            string message = "Domain of the new palette [1..20]:"; 
            GetStringDialog getStringDialog = new GetStringDialog("Get Domain", message);
            if(getStringDialog.ShowDialog() == DialogResult.OK)
            {
                int domain;
                if(int.TryParse(getStringDialog.String, out domain) && domain > 0 && domain < 21)
                {
                    
                    PaletteForm paletteForm = null;
                    string newname = NewPaletteName(PalettesListBox.Items.Count + 1);
                    paletteForm = new PaletteForm(newname, domain, _callbacks);
                    List<PaletteForm> currentPaletteForms = CurrentPaletteForms;
                    currentPaletteForms.Add(paletteForm);
                    CurrentPaletteForms = currentPaletteForms;
                    PalettesListBox.SelectedIndex = PalettesListBox.Items.Count - 1;
                    this.SetPalettesPanelNeedsReview();
                }
                else
                {
                    MessageBox.Show("Illegal domain.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }

        }

        private string NewPaletteName(int paletteNumber)
        {
            return "palette " + paletteNumber.ToString();
        }

        private void DeletePaletteButton_Click(object sender, EventArgs e)
        {
            int selectedIndex = -1;
            if(PalettesListBox.SelectedIndices.Count > 0)
                selectedIndex = PalettesListBox.SelectedIndices[0];
            if(selectedIndex >= 0)
            {
                string toDelete = PalettesListBox.Items[selectedIndex].ToString();
                string msg = toDelete + " will be deleted completely.\n\n" +
                    "Proceed?\n\n";
                DialogResult proceed = MessageBox.Show(msg, "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if(proceed == DialogResult.Yes)
                {
                    List<PaletteForm> currentPaletteForms = CurrentPaletteForms;
                    PaletteForm paletteForm = currentPaletteForms[selectedIndex];
                    paletteForm.Close();
                    currentPaletteForms.RemoveAt(selectedIndex);
                    CurrentPaletteForms = currentPaletteForms;
                    this.SetPalettesPanelNeedsReview();
                }
            }
        }

        private void PalettesListBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            DoSelectionColor(sender, e);
        }
        #endregion palette buttons


        #endregion list box events

        #region private variables
        SolidBrush _systemHighlightBrush = new SolidBrush(SystemColors.Highlight);
        SolidBrush _whiteBrush = new SolidBrush(Color.White);
        SolidBrush _blackBrush = new SolidBrush(Color.Black);

        /// <summary>
        /// _scoreTitle is the name of the score without any folder or extension. For example "Study 2b2".
        /// </summary>
        string _scoreTitle = null;
        CompositionAlgorithm _algorithm = null;

        /// <summary>
        /// The folder (in the AssistantPerformer's folder)
        /// in which the score and all its associated files is saved
        /// </summary>
        string _settingsFolderPath = null;
        string _settingsPath = null;
        const int _minimumMsDuration = 5;
        #endregion private variables

        #region ComboBox events
        #region handle the use of text in the ComboBox
        /// <summary>
        /// If comboBox.text is not in the comboBox.Items list, comboBox.SelectedIndex is set to 0.
        /// Otherwise comboBox.SelectedIndex is set to the appropriate index.
        /// </summary>
        private void SetComboBoxSelectedIndexFromText(ComboBox comboBox)
        {
            if(comboBox.SelectedIndex == -1)
            {
                int index = comboBox.Items.IndexOf(comboBox.Text);
                if(index == -1)
                    comboBox.SelectedIndex = 0;
                else
                    comboBox.SelectedIndex = comboBox.Items.IndexOf(comboBox.Text);
            }
        }
        /// <summary>
        /// This function ensures that the user can only use values in the ScoreComboBox's item list.
        /// Typing custom values won't work.
        /// </summary>
        private void ScoreComboBox_Leave(object sender, EventArgs e)
        {
            SetComboBoxSelectedIndexFromText(ScoreComboBox);
            SetNotationPanelNeedsReview();
        }
        /// <summary>
        /// This function ensures that the user can only use values in the ChordTypeComboBox's item list.
        /// Typing custom values won't work.
        /// </summary>
        private void ChordTypeComboBox_Leave(object sender, EventArgs e)
        {
            SetComboBoxSelectedIndexFromText(ChordTypeComboBox);
            SetNotationPanelNeedsReview();
        }
        /// <summary>
        /// This function ensures that the user can only use values in the ChordTypeComboBox's item list.
        /// Typing custom values won't work.
        /// </summary>
        private void StafflineStemStrokeWidthComboBox_Leave(object sender, EventArgs e)
        {
            SetComboBoxSelectedIndexFromText(StafflineStemStrokeWidthComboBox);
            SetNotationPanelNeedsReview();
        }
        /// <summary>
        /// This function ensures that the user can only use values in the ChordTypeComboBox's item list.
        /// Typing custom values won't work.
        /// </summary>
        private void GapPixelsComboBox_Leave(object sender, EventArgs e)
        {
            SetComboBoxSelectedIndexFromText(GapPixelsComboBox);
            SetNotationPanelNeedsReview();
        }
        #endregion
        private void ScoreComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            _dimensionsAndMetadataForm.Hide();
            foreach(PaletteForm palette in CurrentPaletteForms)
            {
                palette.Close();
            }
            if(_krystalBrowser != null)
            {
                _krystalBrowser.Close();
                _krystalBrowser = null;
            }

            string scoreName = ScoreComboBox.SelectedItem.ToString();
            _dimensionsAndMetadataForm.Text = scoreName + ": Page Dimensions and Metadata";

            GetSelectedSettings();
        }
        private void ChordTypeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.StandardChordsOptionsPanel.Visible = false;
            if(((string)ChordTypeComboBox.SelectedItem) == "standard")
                this.StandardChordsOptionsPanel.Visible = true;

            this.OutputVoiceIndicesStaffTextBox_Leave(null, null);
            SetNotationPanelNeedsReview();
        }
        private void StafflineStemStrokeWidthComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetNotationPanelNeedsReview();
        }
        private void GapPixelsComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetNotationPanelNeedsReview();
        }

        private void GetSelectedSettings()
        {
            LoadSettings();

            InitOutputVoiceIndicesPerStaffFields(_algorithm.MidiChannelIndexPerOutputVoice.Count);
            InitInputVoiceIndicesPerStaffFields(_algorithm.NumberOfInputVoices);

            SetSystemStartBarsHelpLabel(_algorithm.NumberOfBars);
            OutputVoiceIndicesStaffTextBox_Leave(null, null); // sets _numberOfOutputStaves _numberOfStaves
            InputVoiceIndicesPerStaffTextBox_Leave(null, null); // sets _numberOfInputStaves, _numberOfStaves

            SetAllSettingsHaveBeenReverted();
        }

        private void VoiceIndicesPerStaffHelp_MouseClick(object sender, MouseEventArgs e)
        {
            if(e.Button == System.Windows.Forms.MouseButtons.Right)
                ShowVoiceIndicesPerStaffHelp();
        }

        private void ShowVoiceIndicesPerStaffHelp()
        {
            StringBuilder voiceIndicesSB = VoiceIndicesSB();
            MessageBox.Show(voiceIndicesSB.ToString(), "Help for 'voices per voice per staff'", MessageBoxButtons.OK);
        }

        private StringBuilder VoiceIndicesSB()
        {
            string mainText = "Input fields 'output voices per voice per staff' and 'input voices per voice per staff'.\n\n" +

                "These fields control the top to bottom order in which voices appear in the score. Not all the output or " +
                "input voices need to be notated in a particular score, but scores must contain at least one output voice.\n\n" +

                "Composition algorithms create voices in staves in systems. (Output voices are always created, but input voices " +
                "are optional.) The staves and voices created by an algorithm are conceptually in top to bottom order, but " +
                "the order in which the voices actually appear in a score is controlled by these two fields.\n\n" +

                "Output voices are always printed above input voices, and these two input fields work in the same way:\n" +
                "Each algorithm declares how many output and input voices it creates. Their indices (voiceIDs) appear in the " +
                "help texts above the fields.\n" +
                "The numbers entered in the fields must be selected from those given in the help texts, and determine the " +
                "top to bottom order of the voices in the printed score:\n" +
                "Staves are separated by commas, voices are separated by colons.\n" +
                "For example, if the algorithm creates six output voices, and these are to be notated using standard " +
                "chord symbols, their indices (0, 1, 2, 3, 4, 5) appear in the help text. These voices " +
                "can be notated on 3 staves by entering '3:1, 0:2, 4:5' in the field. Voices with indices 2 and 3 " +
                "could be notated on separate staves, omitting the other voices, by entering '2, 3'.\n\n" +

                "Algorithms compose output voices complete with their midi channel (and master volume initialization value). " +
                "This allows them to stipulate the standard midi percussion channel (channel index 9).\n" +
                "The midi channel and master volume values are therefore fixed to their respective voiceIDs, and move with " +
                "them when the voices are re-ordered.\n\n";

            StringBuilder voiceIndicesSB = new StringBuilder();
            voiceIndicesSB.Append(mainText);
            voiceIndicesSB.Append("\n\nThe current algorithm ");
            if(_outputVoiceIndices.Count == 1)
                voiceIndicesSB.Append("only uses\noutput voice index 0");
            else
                voiceIndicesSB.Append("uses\noutput voice indices ");

            for(int i = 0; i < _outputVoiceIndices.Count; ++i)
            {
                if(i > 0)
                {
                    if(i == (_outputVoiceIndices.Count - 1))
                        voiceIndicesSB.Append(" and ");
                    else
                        voiceIndicesSB.Append(", ");
                }
                voiceIndicesSB.Append(_outputVoiceIndices[i].ToString());
            }
            int nInputVoices = _algorithm.NumberOfInputVoices;
            if(nInputVoices == 0)
                voiceIndicesSB.Append(",\nand has no input voices.\n\n");
            else if(nInputVoices == 1)
            {
                voiceIndicesSB.Append(",\nand input voice index 0\n\n");
            }
            else
            {
                voiceIndicesSB.Append(",\nand input voice indices " );
                for(int i = 0; i < nInputVoices; ++i)
                {
                    voiceIndicesSB.Append(i.ToString());
                    voiceIndicesSB.Append(", ");
                }
                voiceIndicesSB.Replace(", ", ".", voiceIndicesSB.Length - 2, 2);
            }

            return voiceIndicesSB;
        }

        private List<byte> GetOutputVoiceIndices(int nOutputVoices)
        {
            List<byte> rval = new List<byte>();
            for(byte i = 0; i < nOutputVoices; ++i)
            {
                rval.Add(i);
            }
            return rval;
        }

        #region helper functions
        private string NormalizedByteListsString(List<List<byte>> byteLists)
        {
            StringBuilder sb = new StringBuilder();
            if(byteLists.Count > 0)
            {
                foreach(List<byte> bytes in byteLists)
                {
                    sb.Append(", ");
                    StringBuilder bytesListSB = new StringBuilder();
                    foreach(byte b in bytes)
                    {
                        bytesListSB.Append(":");
                        bytesListSB.Append(b.ToString());
                    }
                    bytesListSB.Remove(0, 1);
                    sb.Append(bytesListSB.ToString());
                }
                sb.Remove(0, 2);
            }
            return sb.ToString();
        }
        private void EnableStaffDependentControls(bool enabled)
        {
            if(enabled)
            {
                SetHelpTexts();
                ClefsPerStaffLabel.Enabled = true;
                StafflinesPerStaffLabel.Enabled = true;
                StaffGroupsLabel.Enabled = true;
                LongStaffNamesLabel.Enabled = true;
                ShortStaffNamesLabel.Enabled = true;

                ClefsPerStaffTextBox.Enabled = true;
                StafflinesPerStaffTextBox.Enabled = true;
                StaffGroupsTextBox.Enabled = true;
                LongStaffNamesTextBox.Enabled = true;
                ShortStaffNamesTextBox.Enabled = true;

                ClefsPerStaffTextBox_Leave(null, null);
                StafflinesPerStaffTextBox_Leave(null, null);

                StaffGroupsTextBox_Leave(null, null);
                LongStaffNamesTextBox_Leave(null, null);
                ShortStaffNamesTextBox_Leave(null, null);
            }
            else
            {
                ClearHelpTexts();
                ClefsPerStaffLabel.Enabled = false;
                StafflinesPerStaffLabel.Enabled = false;
                StaffGroupsLabel.Enabled = false;
                LongStaffNamesLabel.Enabled = false;
                ShortStaffNamesLabel.Enabled = false;

                ClefsPerStaffTextBox.Enabled = false;
                StafflinesPerStaffTextBox.Enabled = false;
                StaffGroupsTextBox.Enabled = false;
                LongStaffNamesTextBox.Enabled = false;
                ShortStaffNamesTextBox.Enabled = false;
            }
        }
        private void SetHelpTexts()
        {
            StringBuilder clefsSB = new StringBuilder();
            foreach(string clef in M.Clefs)
            {
                clefsSB.Append(", ");
                clefsSB.Append(clef);
            }
            clefsSB.Remove(0, 2);

            if(_numberOfStaves == 1)
            {
                this.ClefsPerStaffHelpLabel.Text = "1 clef\n" +
                                                   "available clefs: " + clefsSB.ToString();
                this.StafflinesPerStaffHelpLabel.Text = "1 integer\n" +
                                                    "standard clefs must have 5 lines";
                this.StaffGroupsHelpLabel.Text = "must be 1";
                this.LongStaffNamesHelpLabel.Text = "1 name (for first system)";
                this.ShortStaffNamesHelpLabel.Text = "1 name (for other systems)";
            }
            else
            {
                this.ClefsPerStaffHelpLabel.Text = _numberOfStaves.ToString() + " clefs separated by commas.\n" +
                                                    "available clefs: " + clefsSB.ToString();
                this.StafflinesPerStaffHelpLabel.Text = _numberOfStaves.ToString() + " integers separated by commas.\n" +
                                                    "standard clefs must have 5 lines";
                this.StaffGroupsHelpLabel.Text = "integers (whose total is " + _numberOfStaves.ToString() + ")";
                this.LongStaffNamesHelpLabel.Text = _numberOfStaves.ToString() + " names (for first system)";
                this.ShortStaffNamesHelpLabel.Text = _numberOfStaves.ToString() + " names (for other systems)";
            }
        }
        private void ClearHelpTexts()
        {
            this.ClefsPerStaffHelpLabel.Text = "";
            this.StafflinesPerStaffHelpLabel.Text = "";
            this.StaffGroupsHelpLabel.Text = "";
            this.LongStaffNamesHelpLabel.Text = "";
            this.ShortStaffNamesHelpLabel.Text = "";
        }
        #endregion


        #endregion page format controls
        #region TextBox Leave events
        private void CheckTextBoxIsUInt(TextBox textBox)
        {
            bool okay = true;
            textBox.Text.Trim();
            try
            {
                uint i = uint.Parse(textBox.Text);
            }
            catch
            {
                okay = false;
            }

            M.SetTextBoxErrorColorIfNotOkay(textBox, okay);
            SetNotationPanelNeedsReview();
        }
        private void MinimumCrotchetDurationTextBox_Leave(object sender, EventArgs e)
        {
            CheckTextBoxIsUInt(this.MinimumCrotchetDurationTextBox);
        }
        private void MinimumGapsBetweenStavesTextBox_Leave(object sender, EventArgs e)
        {
            CheckTextBoxIsUInt(this.MinimumGapsBetweenStavesTextBox);
        }
        private void MinimumGapsBetweenSystemsTextBox_Leave(object sender, EventArgs e)
        {
            CheckTextBoxIsUInt(this.MinimumGapsBetweenSystemsTextBox);
        }
        /// <summary>
        /// This function sets _numberOfOutputStaves (and consequetially _numberOfStaves).
        /// It also sets _staffIsInput.
        /// It also sets the help texts on the dialog.
        /// </summary>
        private void OutputVoiceIndicesStaffTextBox_Leave(object sender, EventArgs e)
        {
            EnableStaffDependentControls(false);

            bool error = false;
            List<List<byte>> byteLists = new List<List<byte>>();
            try
            {
                byteLists = M.StringToByteLists(OutputVoiceIndicesStaffTextBox.Text);
            }
            catch
            {
                error = true;
            }

            if(byteLists.Count == 0)
                error = true;

            if(!error)
            {
                foreach(List<byte> outputVoiceIndices in byteLists)
                {
                    // outputVoiceIndices are output staff outputVoiceIndices here.
                    if(outputVoiceIndices.Count == 0 || outputVoiceIndices.Count > 2
                    || (outputVoiceIndices.Count > 1 && ((string)ChordTypeComboBox.SelectedItem) != "standard"))
                    {
                        error = true;
                        break;
                    }
                    foreach(byte ovIndex in outputVoiceIndices)
                    {
                        if(!_outputVoiceIndices.Contains(ovIndex))
                        {
                            error = true;
                            break;
                        }
                    }
                    if(error)
                        break;
                }
            }

            if(error)
            {
                M.SetTextBoxErrorColorIfNotOkay(OutputVoiceIndicesStaffTextBox, false);
            }
            else
            {
                _outputVoiceIndicesPerStaff = byteLists;
                EnableStaffDependentControls(true);
                OutputVoiceIndicesStaffTextBox.Text = NormalizedByteListsString(byteLists);
                M.SetTextBoxErrorColorIfNotOkay(OutputVoiceIndicesStaffTextBox, true);
            }
            SetNotationPanelNeedsReview();
        }
        /// <summary>
        /// This function sets _numberOfinputStaves (and consequetially _numberOfStaves).
        /// It also sets the help texts on the dialog.
        /// </summary>
        private void InputVoiceIndicesPerStaffTextBox_Leave(object sender, EventArgs e)
        {
            EnableStaffDependentControls(false);

            bool error = false;
            List<List<byte>> byteLists = new List<List<byte>>();

            try
            {
                byteLists = M.StringToByteLists(InputVoiceIndicesPerStaffTextBox.Text);
            }
            catch
            {
                error = true;
            }

            // byteLists.Count==0 (number of staves == 0) is not an error for inputs
            if(byteLists.Count > 0 && !error)
            {
                foreach(List<byte> outputVoiceIndices in byteLists)
                {
                    if(outputVoiceIndices.Count == 0 || outputVoiceIndices.Count > 2)
                    {
                        error = true;
                        break;
                    }
                    foreach(byte ovIndex in outputVoiceIndices)
                    {
                        if(ovIndex < 0 || ovIndex >= _algorithm.NumberOfInputVoices)
                        {
                            error = true;
                            break;
                        }
                    }
                    if(error)
                        break;
                }
            }

            if(error)
            {
                M.SetTextBoxErrorColorIfNotOkay(InputVoiceIndicesPerStaffTextBox, false);
            }
            else
            {
                _inputVoiceIndicesPerStaff = byteLists;
                EnableStaffDependentControls(true);
                InputVoiceIndicesPerStaffTextBox.Text = NormalizedByteListsString(byteLists);
                M.SetTextBoxErrorColorIfNotOkay(InputVoiceIndicesPerStaffTextBox, true);
            }
            SetNotationPanelNeedsReview();
        }
        private string NormalizedText(List<string> texts)
        {
            StringBuilder sb = new StringBuilder();
            foreach(string text in texts)
            {
                sb.Append(", ");
                sb.Append(text);
            }
            sb.Remove(0, 2);
            return sb.ToString();
        }
        /// <summary>
        /// This function _uses_ _numberOfStaves.
        /// </summary>
        private void ClefsPerStaffTextBox_Leave(object sender, EventArgs e)
        {
            List<string> trimmedClefs = M.StringToStringList(ClefsPerStaffTextBox.Text, ',');

            bool okay = true;
            foreach(string clef in trimmedClefs)
            {
                if(!M.Clefs.Contains(clef))
                {
                    okay = false;
                    break;
                }
            }

            if(okay && trimmedClefs.Count == _numberOfStaves)
            {
                ClefsPerStaffTextBox.Text = NormalizedText(trimmedClefs);
                M.SetTextBoxErrorColorIfNotOkay(ClefsPerStaffTextBox, true);
                CheckClefsAndStafflineNumbers();
            }
            else
                M.SetTextBoxErrorColorIfNotOkay(ClefsPerStaffTextBox, false);

            SetNotationPanelNeedsReview();
        }
        private void CheckClefsAndStafflineNumbers()
        {
            string[] clefs = ClefsPerStaffTextBox.Text.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            string[] stafflinesArray = StafflinesPerStaffTextBox.Text.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            if(clefs.Length == stafflinesArray.Length)
            {
                for(int i = 0; i < stafflinesArray.Length; ++i)
                {
                    if(stafflinesArray[i].Trim() != "5")
                    {
                        if(clefs[i].Trim() == "n")
                            M.SetTextBoxErrorColorIfNotOkay(StafflinesPerStaffTextBox, true);
                        else
                            M.SetTextBoxErrorColorIfNotOkay(StafflinesPerStaffTextBox, false);
                    }
                }
            }
            else
            {
                M.SetTextBoxErrorColorIfNotOkay(StafflinesPerStaffTextBox, false);
            }
            SetNotationPanelNeedsReview();
        }
        /// <summary>
        /// This function _uses_ _numberOfStaves.
        /// </summary>
        private void StafflinesPerStaffTextBox_Leave(object sender, EventArgs e)
        {
            M.LeaveIntRangeTextBox(StafflinesPerStaffTextBox, false, (uint)_numberOfStaves, 1, 127, M.SetTextBoxErrorColorIfNotOkay);
            CheckClefsAndStafflineNumbers();
        }
        /// <summary>
        /// This function _uses_ _numberOfStaves.
        /// </summary>
        private void StaffGroupsTextBox_Leave(object sender, EventArgs e)
        {
            M.LeaveIntRangeTextBox(StaffGroupsTextBox, false, uint.MaxValue, 1, 127, M.SetTextBoxErrorColorIfNotOkay);
            if(StaffGroupsTextBox.ForeColor != M.TextBoxErrorColor)
            {
                List<byte> bytes = M.StringToByteList(StaffGroupsTextBox.Text, ',');
                int sum = 0;
                foreach(byte b in bytes)
                    sum += (int)b;
                if(sum != _numberOfStaves)
                    M.SetTextBoxErrorColorIfNotOkay(StaffGroupsTextBox, false);
            }
            SetNotationPanelNeedsReview();
        }
        private void CheckStaffNames(TextBox textBox)
        {
            string[] names = textBox.Text.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            List<string> trimmedNames = new List<string>();
            foreach(string name in names)
            {
                trimmedNames.Add(name.Trim());
            }
            if(trimmedNames.Count == _numberOfStaves)
            {
                textBox.Text = NormalizedText(trimmedNames);
                M.SetTextBoxErrorColorIfNotOkay(textBox, true);
            }
            else
            {
                M.SetTextBoxErrorColorIfNotOkay(textBox, false);
            }
            SetNotationPanelNeedsReview();
        }
        private void LongStaffNamesTextBox_Leave(object sender, EventArgs e)
        {
            CheckStaffNames(LongStaffNamesTextBox);
        }
        private void ShortStaffNamesTextBox_Leave(object sender, EventArgs e)
        {
            CheckStaffNames(ShortStaffNamesTextBox);
        }
        /// <summary>
        /// This function sorts the start bars into ascending order, and silently removes duplicates.
        /// </summary>
        /// <returns></returns>
        private string NormalizedSystemStartBars()
        {
            Debug.Assert(SystemStartBarsTextBox.Text.Length > 0);
            List<int> startBars = M.StringToIntList(SystemStartBarsTextBox.Text, ',');
            StringBuilder sb = new StringBuilder();
            startBars.Sort();

            if(startBars.Count > 1)
            {   // remove duplicates
                int currentStartBar = startBars[startBars.Count - 1];
                for(int i = startBars.Count - 2; i >= 0; --i)
                {
                    if(startBars[i] == currentStartBar)
                        startBars.RemoveAt(i);
                    else
                        currentStartBar = startBars[i];
                }
                if(startBars[0] != 1 || startBars[startBars.Count - 1] > _algorithm.NumberOfBars)
                    M.SetTextBoxErrorColorIfNotOkay(SystemStartBarsTextBox, false);
                else
                    M.SetTextBoxErrorColorIfNotOkay(SystemStartBarsTextBox, true);
            }

            foreach(int val in startBars)
            {
                sb.Append(", ");
                sb.Append(val.ToString());
            }
            sb.Remove(0, 2);
            return sb.ToString();
        }
        private void SystemStartBarsTextBox_Leave(object sender, EventArgs e)
        {
            // Passing uint.MaxValue means that the list can have any number of values (including none).
            M.LeaveIntRangeTextBox(SystemStartBarsTextBox, true, uint.MaxValue, 1, int.MaxValue, M.SetTextBoxErrorColorIfNotOkay);
            if((SystemStartBarsTextBox.BackColor != M.TextBoxErrorColor)
            && (SystemStartBarsTextBox.Text.Length > 0))
            {
                SystemStartBarsTextBox.Text = NormalizedSystemStartBars();
            }
            SetNotationPanelNeedsReview();
        }
        #endregion  TextBox Leave events
        #region TextChanged events (just set text boxes to white)
        private void MinimumCrotchetDurationTextBox_TextChanged(object sender, EventArgs e)
        {
            M.SetToWhite(sender as TextBox);
        }
        private void MinimumGapsBetweenStavesTextBox_TextChanged(object sender, EventArgs e)
        {
            M.SetToWhite(sender as TextBox);
        }
        private void MinimumGapsBetweenSystemsTextBox_TextChanged(object sender, EventArgs e)
        {
            M.SetToWhite(sender as TextBox);
        }
        private void OutputVoiceIndicesStaffTextBox_TextChanged(object sender, EventArgs e)
        {
            M.SetToWhite(OutputVoiceIndicesStaffTextBox);
        }
        private void InputVoiceIndicesPerStaffTextBox_TextChanged(object sender, EventArgs e)
        {
            M.SetToWhite(InputVoiceIndicesPerStaffTextBox);
        }
        private void ClefsPerStaffTextBox_TextChanged(object sender, EventArgs e)
        {
            M.SetToWhite(ClefsPerStaffTextBox);
        }
        private void StafflinesPerStaffTextBox_TextChanged(object sender, EventArgs e)
        {
            M.SetToWhite(StafflinesPerStaffTextBox);
        }
        private void StaffGroupsTextBox_TextChanged(object sender, EventArgs e)
        {
            M.SetToWhite(StaffGroupsTextBox);
        }
        private void LongStaffNamesTextBox_TextChanged(object sender, EventArgs e)
        {
            M.SetToWhite(LongStaffNamesTextBox);
        }
        private void ShortStaffNamesTextBox_TextChanged(object sender, EventArgs e)
        {
            M.SetToWhite(ShortStaffNamesTextBox);
        }
        private void SystemStartBarsTextBox_TextChanged(object sender, EventArgs e)
        {
            M.SetToWhite(SystemStartBarsTextBox);
        }
        #endregion

        private ComposerFormCallbacks _callbacks;
        private List<TextBox> _allTextBoxes = null;
        private ReviewableFormFunctions _rff = new ReviewableFormFunctions();
        private IMoritzForm1 _moritzForm1;
        private Moritz.Krystals.KrystalBrowser _krystalBrowser = null;
        private DimensionsAndMetadataForm _dimensionsAndMetadataForm;

        private List<byte> _outputVoiceIndices = null;

        private List<List<byte>> _outputVoiceIndicesPerStaff; // set in OutputVoiceIndicesPerStaffTextBox_Leave
        private List<List<byte>> _inputVoiceIndicesPerStaff; // set in InputVoiceIndicesPerStaffTextBox_Leave
        
        private int _numberOfStaves 
        { 
            get 
            {
                if(_outputVoiceIndicesPerStaff != null && _inputVoiceIndicesPerStaff != null)
                {
                    return _outputVoiceIndicesPerStaff.Count + _inputVoiceIndicesPerStaff.Count;
                }
                else
                {
                    return 0;
                }
            } 
        }

        public PageFormat PageFormat = null;


    }
}