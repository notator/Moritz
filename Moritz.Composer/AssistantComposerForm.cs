using System;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Xml;
using System.Text.RegularExpressions;

using Krystals4ObjectLibrary;
using Moritz.Globals;
using Moritz.Symbols;
using Moritz.Palettes;
using Moritz.Algorithm;
using Moritz.Krystals;

namespace Moritz.Composer
{
    public partial class AssistantComposerForm : Form, IPaletteFormsHostForm
    {
        public AssistantComposerForm(string settingsPath, IMoritzForm1 moritzForm1)
        {
            InitializeComponent();

            _allTextBoxes = GetAllTextBoxes();

            _moritzForm1 = moritzForm1;
            _fsf = new FormStateFunctions();

            SetDefaultValues();
            DeselectAll();

            Debug.Assert(File.Exists(settingsPath));

            _settingsPath = settingsPath;
            _settingsFolderPath = Path.GetDirectoryName(settingsPath);
            _scoreTitle = Path.GetFileNameWithoutExtension(settingsPath);

			this.Text = _settingsFolderPath.Substring(_settingsFolderPath.LastIndexOf('\\') + 1);
			this.QuitAssistantComposerButton.Text = "Quit algorithm:  " + _scoreTitle;

            _dimensionsAndMetadataForm = new DimensionsAndMetadataForm(this, _settingsPath, _fsf);

            _algorithm = ComposableSvgScore.Algorithm(_scoreTitle);

            Debug.Assert(_algorithm != null);

            _outputVoiceIndices = GetOutputVoiceIndices(_algorithm.MidiChannelIndexPerOutputVoice.Count);
                
            GetSelectedSettings();

            if(VoiceIndicesPerStaffTextBox.Text == "")
            {
                SetDefaultVoiceIndicesPerStaff(_algorithm.MidiChannelIndexPerOutputVoice.Count);
            }
        }
        #region called from ctor

		private List<TextBox> GetAllTextBoxes()
        {
            List<TextBox> textBoxes = new List<TextBox>();

            textBoxes.Add(MinimumGapsBetweenStavesTextBox);
            textBoxes.Add(MinimumGapsBetweenSystemsTextBox);
            textBoxes.Add(MinimumCrotchetDurationTextBox);

            textBoxes.Add(VoiceIndicesPerStaffTextBox);
            textBoxes.Add(ClefsPerStaffTextBox);
            textBoxes.Add(StafflinesPerStaffTextBox);
            textBoxes.Add(StaffGroupsTextBox);


            textBoxes.Add(LongStaffNamesTextBox);
            textBoxes.Add(ShortStaffNamesTextBox);

            textBoxes.Add(SystemStartBarsTextBox);
            return textBoxes;
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
        private void SetDefaultValues()
        {
            this.NotationGroupBox.Tag = SavedState.unconfirmed;
            this.KrystalsGroupBox.Tag = SavedState.unconfirmed;
            this.PalettesGroupBox.Tag = SavedState.unconfirmed;

            this.StafflineStemStrokeWidthComboBox.SelectedIndex = 0;
            this.GapPixelsComboBox.SelectedIndex = 0;
            this.MinimumGapsBetweenStavesTextBox.Text = "8";
            this.MinimumGapsBetweenSystemsTextBox.Text = "11";
            this.MinimumCrotchetDurationTextBox.Text = "800";

            ClearListBoxes();
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
        private List<byte> GetOutputVoiceIndices(int nOutputVoices)
        {
            List<byte> rval = new List<byte>();
            for(byte i = 0; i < nOutputVoices; ++i)
            {
                rval.Add(i);
            }
            return rval;
        }
        private void GetSelectedSettings()
        {
			LoadSettings();

            SetVoiceIndicesHelpLabel(_algorithm.MidiChannelIndexPerOutputVoice.Count, _algorithm.NumberOfInputVoices);
            SetSystemStartBarsHelpLabel(_algorithm.NumberOfBars);

            VoiceIndicesPerStaffTextBox_Leave(null, null); // sets _numberOfOutputStaves _numberOfStaves

            SetGroupBoxIsSaved(NotationGroupBox, ConfirmNotationButton, RevertNotationButton,
                (SavedState)KrystalsGroupBox.Tag, (SavedState)PalettesGroupBox.Tag);
        }
        #region helpers
        private void SetVoiceIndicesHelpLabel(int nOutputVoices, int nInputVoices)
        {
            Debug.Assert(nOutputVoices > 0);
            StringBuilder sb = new StringBuilder();
            for(int i = 0; i < nOutputVoices; ++i)
            {
                sb.Append(", ");
                sb.Append(i.ToString());
            }
            sb.Remove(0, 2);
            if(nInputVoices > 0)
            {
                sb.Append(" | ");
                StringBuilder ivsb = new StringBuilder();
                for(int i = 0; i < nInputVoices; ++i)
                {
                    ivsb.Append(", ");
                    ivsb.Append(i.ToString());
                }
                ivsb.Remove(0, 2);
                sb.Append(ivsb);
            }
            VoiceIndicesHelpLabel.Text = sb.ToString();
        }

        private void SetSystemStartBarsHelpLabel(int numberOfBars)
        {
            SystemStartBarsHelpLabel.Text = "(" + numberOfBars.ToString() + " bars. Default is 5 bars per system)";
        }
        #endregion helpers
        private void SetDefaultVoiceIndicesPerStaff(int nVoices)
        {
            StringBuilder voiceIndexList = new StringBuilder();
            for(int i = 0; i < nVoices; ++i)
            {
                voiceIndexList.Append(i.ToString());
                voiceIndexList.Append(", ");
            }
            voiceIndexList.Remove(voiceIndexList.Length - 2, 2);
            VoiceIndicesPerStaffTextBox.Text = voiceIndexList.ToString();
            VoiceIndicesPerStaffTextBox_Leave(null, null);
        }
        #endregion called from ctor

        #region public interface
        /// <summary>
        /// Called when a PaletteChordForm is created or closed.
        /// When a PaletteChordForm is open, all other forms are disabled.
        /// Otherwise, they are all enabled.
        /// </summary>
        public void SetAllFormsExceptChordFormEnabledState(bool enabledState)
        {
            foreach(PaletteForm paletteForm in this.PalettesListBox.Items)
            {
                if(paletteForm.OrnamentsForm != null)
                {
                    paletteForm.OrnamentsForm.Enabled = enabledState;
                }
                paletteForm.Enabled = enabledState;
            }
            this._dimensionsAndMetadataForm.Enabled = enabledState;
            this.Enabled = enabledState;
        }
        /// <summary>
        /// Used when reverting other forms.
        /// </summary>
        public string SettingsPath { get { return _settingsPath; } }
        public string LocalScoreAudioPath
        {
            get
            {
                string path = M.Preferences.LocalMoritzAudioFolder + @"\" + _scoreTitle;
                return path;
            }
        }
        /// <summary>
        /// Called by paletteForms and their subsidiary ornamentForms
        /// </summary>
        public void UpdateForChangedPaletteForm()
        {
            NotationGroupBox.Enabled = false;
            KrystalsGroupBox.Enabled = false;
            SetPalettesGroupBox();
            UpdateMainFormState();
        }
        public void UpdateMainFormState()
        {
            if(((SavedState)NotationGroupBox.Tag == SavedState.unconfirmed)
                || ((SavedState)KrystalsGroupBox.Tag == SavedState.unconfirmed)
                || ((SavedState)PalettesGroupBox.Tag == SavedState.unconfirmed)
                || ((SavedState)_dimensionsAndMetadataForm.Tag == SavedState.unconfirmed))
            {
                SetMainFormIsUnconfirmed();
            }
            else if(((SavedState)NotationGroupBox.Tag == SavedState.confirmed)
                 || ((SavedState)KrystalsGroupBox.Tag == SavedState.confirmed)
                 || ((SavedState)PalettesGroupBox.Tag == SavedState.confirmed)
                 || ((SavedState)_dimensionsAndMetadataForm.Tag == SavedState.confirmed))
            {
                SetMainFormIsConfirmed();
            }
            else
                SetMainFormIsSaved();
        }
        #endregion public interface

        #region control helpers
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
        #endregion control helpers

        #region AssistantComposer form state

        #region main form event handlers
        #region helpers
        private void DisableGroupConfirmRevertButtons()
        {
            RevertNotationButton.Enabled = false;
            ConfirmNotationButton.Enabled = false;
            RevertKrystalsListButton.Enabled = false;
            ConfirmKrystalsListButton.Enabled = false;
            RevertPalettesListButton.Enabled = false;
        }
        private bool ThereAreUnconfirmedPalettes()
        {
            return ThereArePalettesOfType(SavedState.unconfirmed);
        }
        private bool ThereAreConfirmedPalettes()
        {
            return ThereArePalettesOfType(SavedState.confirmed);
        }
        private bool ThereArePalettesOfType(SavedState state)
        {
            bool rval = false;
            foreach(PaletteForm paletteForm in this.PalettesListBox.Items)
            {
                if(((SavedState)paletteForm.Tag == state)
                || (paletteForm.OrnamentsForm != null && ((SavedState)paletteForm.OrnamentsForm.Tag == state)))
                {
                    rval = true;
                    break;
                }
            }
            return rval;
        }
        #endregion helpers

        private void SetMainFormSaveCreateButtonText(bool thereAreTouchedForms)
        {
            this.SuspendLayout();
            if(thereAreTouchedForms)
            {
                if(!this.SaveSettingsCreateScoreButton.Text.StartsWith("save"))
                    this.SaveSettingsCreateScoreButton.Text = "save all settings";
            }
            else // if(state == SavedState.saved)
            {
                if(!this.SaveSettingsCreateScoreButton.Text.StartsWith("create"))
                    this.SaveSettingsCreateScoreButton.Text = "create score";
            }
            this.ResumeLayout();
        }

        private void SetPalettesGroupBox()
        {
            PalettesListBox.Refresh();
            if(ThereAreUnconfirmedPalettes())
            {
                _fsf.SetGroupBoxState(PalettesGroupBox, SavedState.unconfirmed);
                RevertPalettesListButton.Enabled = true;
            }
            else if(ThereAreConfirmedPalettes())
            {
                _fsf.SetGroupBoxState(PalettesGroupBox, SavedState.confirmed);              
                RevertPalettesListButton.Enabled = true;
            }
            else // all palettes are saved
            {
                _fsf.SetGroupBoxState(PalettesGroupBox, SavedState.saved);
                RevertPalettesListButton.Enabled = false;
            }
        }

        private void AssistantComposerForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            foreach(PaletteForm paletteForm in PalettesListBox.Items)
            {
                paletteForm.Delete();
            }

            if(_dimensionsAndMetadataForm != null)
            {
                _dimensionsAndMetadataForm.Close();
            }

            if(_krystalBrowser != null)
            {
                _krystalBrowser.Close();
                _krystalBrowser = null;
            }
        }
        private void DimensionsAndMetadataButton_Click(object sender, EventArgs e)
        {
            _dimensionsAndMetadataForm.Enabled = true;
            _dimensionsAndMetadataForm.Show();
            _dimensionsAndMetadataForm.BringToFront();
        }
        #region GroupBox confirm and revert buttons
        private void ConfirmNotationButton_Click(object sender, EventArgs e)
        {
            SetGroupBoxIsConfirmed(NotationGroupBox, ConfirmNotationButton,
                (SavedState)KrystalsGroupBox.Tag, (SavedState)PalettesGroupBox.Tag);
        }
        private void RevertNotationToSavedButton_Click(object sender, EventArgs e)
        {
            Debug.Assert(((SavedState)NotationGroupBox.Tag) == SavedState.unconfirmed || ((SavedState)NotationGroupBox.Tag) == SavedState.confirmed);
            DialogResult result =
                MessageBox.Show("Are you sure you want to revert the notation panel to the saved version?",
                                "Revert?", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);

            if(result == System.Windows.Forms.DialogResult.Yes)
            {
                ReadNotation();
                SetGroupBoxIsSaved(NotationGroupBox, ConfirmNotationButton, RevertNotationButton,
                    (SavedState)KrystalsGroupBox.Tag, (SavedState)PalettesGroupBox.Tag);
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
        private void ConfirmKrystalsButton_Click(object sender, EventArgs e)
        {
            SetGroupBoxIsConfirmed(KrystalsGroupBox, ConfirmKrystalsListButton,
                (SavedState)NotationGroupBox.Tag, (SavedState)PalettesGroupBox.Tag);
        }
        private void RevertKrystalsToSavedButton_Click(object sender, EventArgs e)
        {
            Debug.Assert(((SavedState)KrystalsGroupBox.Tag) == SavedState.unconfirmed || ((SavedState)KrystalsGroupBox.Tag) == SavedState.confirmed);
            DialogResult result =
                MessageBox.Show("Are you sure you want to revert the krystals panel to the saved version?",
                                "Revert?", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);

            if(result == System.Windows.Forms.DialogResult.Yes)
            {
                ReadKrystals();
                SetGroupBoxIsSaved(KrystalsGroupBox, ConfirmKrystalsListButton, RevertKrystalsListButton,
                    (SavedState)NotationGroupBox.Tag, (SavedState)PalettesGroupBox.Tag);
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

        private void RevertPalettesButton_Click(object sender, EventArgs e)
        {
            Debug.Assert(((SavedState)PalettesGroupBox.Tag) == SavedState.unconfirmed || ((SavedState)PalettesGroupBox.Tag) == SavedState.confirmed);
            DialogResult result =
                MessageBox.Show("Are you sure you want to close all palettes and revert them to their saved versions?",
                                "Revert?", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);

            if(result == System.Windows.Forms.DialogResult.Yes)
            {
                foreach(PaletteForm paletteForm in PalettesListBox.Items)
                {
                    paletteForm.Delete();
                }
                ReadPalettes();
                SetGroupBoxIsSaved(PalettesGroupBox, null, RevertPalettesListButton,
                    (SavedState)NotationGroupBox.Tag, (SavedState)KrystalsGroupBox.Tag);
                PalettesListBox.Refresh();
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
        #endregion GroupBox confirm and revert buttons
        private void ShowUnconfirmedFormsButton_Click(object sender, EventArgs e)
        {
            Debug.Assert(_fsf.UnconfirmedFormsExist());
            _fsf.ShowUnconfirmedForms();
        }
        private void ShowConfirmedFormsButton_Click(object sender, EventArgs e)
        {
            Debug.Assert(_fsf.ConfirmedFormsExist());
            _fsf.ShowConfirmedForms();
        }   
        #region SaveSettingsCreateScore button
        public void SaveSettingsCreateScoreButton_Click(object sender, EventArgs e)
        {
            if(SaveSettingsCreateScoreButton.Text.StartsWith("save"))
            {
                try
                {
                    // The settings are saved here in case there is going to be an error while creating the score.
                    SaveSettings();
                    SetMainFormIsSaved();
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

        #endregion SaveSettingsCreateScore button
        private void RevertEverythingButton_Click(object sender, EventArgs e)
        {
            DialogResult result =
            MessageBox.Show("Are you sure you want to revert all the settings to the saved version?",
                    "Revert?", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);

            if(result == System.Windows.Forms.DialogResult.Yes)
            {
                Point dimensionsAndMetadataFormLocation = GetDimensionsAndMetadataFormLocation();
                Dictionary<int, Point> visiblePaletteFormLocations = GetVisiblePaletteFormLocations();
                Dictionary<int, Point> visibleOrnamentFormLocations = GetVisibleOrnamentFormLocations();

                _dimensionsAndMetadataForm.Close();
                _dimensionsAndMetadataForm = new DimensionsAndMetadataForm(this, _settingsPath, _fsf);

                foreach(PaletteForm paletteForm in PalettesListBox.Items)
                {
                    paletteForm.Delete(); // closes any OrnamentsForm and/or PaletteChordform
                }

                LoadSettings(); // clears existing settings

                ReshowForms(dimensionsAndMetadataFormLocation, visiblePaletteFormLocations, visibleOrnamentFormLocations);             
            }
        }
        #region helpers
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
        /// <summary>
        /// Returns the index (in PalettesListBox.Items) and location of all visible ornamentForms
        /// </summary>
        private Dictionary<int, Point> GetVisibleOrnamentFormLocations()
        {
            Dictionary<int, Point> indexAndLocation = new Dictionary<int, Point>();
            for(int i = 0; i < PalettesListBox.Items.Count; ++i)
            {
                PaletteForm paletteForm = PalettesListBox.Items[i] as PaletteForm;
                if(paletteForm.OrnamentsForm != null && paletteForm.OrnamentsForm.Visible)
                {
                    indexAndLocation.Add(i, paletteForm.OrnamentsForm.Location);
                }
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
                    Debug.Assert(paletteForm.OrnamentsForm != null);
                    paletteForm.OrnamentsForm.Show();
                    paletteForm.OrnamentsForm.Location = visibleOrnamentFormLocations[i];
                }
                else if(paletteForm.OrnamentsForm != null)
                {
                    paletteForm.OrnamentsForm.Hide();
                }
            }

            this.BringToFront();
        }
        #endregion helpers

        private void QuitMoritzButton_Click(object sender, EventArgs e)
        {
            if(DiscardAnyChanges())
                _moritzForm1.Close();
        }
        private void QuitAssistantComposerButton_Click(object sender, EventArgs e)
        {
            if(DiscardAnyChanges())
                _moritzForm1.CloseAssistantComposer();
        }
        #region helper
        /// <summary>
        /// This function is also called by MoritzForm1.
        /// If there are any unsaved changes, this function asks the user if they should be discarded.
        /// If there are no changes, or the user answers yes, this function returns true.
        /// Otherwise it returns false.
        /// </summary>
        /// <returns></returns>
        public bool DiscardAnyChanges()
        {
            bool discard = true;
            if(_fsf.UnconfirmedFormsExist() || _fsf.ConfirmedFormsExist())
            {
                DialogResult result = MessageBox.Show("Discard changes?", "Discard changes?", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if(result == DialogResult.No)
                {
                    discard = false;
                }
            }
            return discard;
        }
        #endregion helper
        #endregion main form event handlers

        #region state of main form
        #region main SetMainFormIs... functions
        private void SetMainFormIsUnconfirmed()
        {
            _fsf.SetFormState(this, SavedState.unconfirmed);
            SetMainFormShowFormsButtons(_fsf.UnconfirmedFormsExist(), _fsf.ConfirmedFormsExist());
            SetMainFormSaveCreateRevertButtons(SavedState.unconfirmed);
        }
        private void SetMainFormIsConfirmed()
        {
            _fsf.SetFormState(this, SavedState.confirmed);
            SetMainFormShowFormsButtons(_fsf.UnconfirmedFormsExist(), _fsf.ConfirmedFormsExist());
            SetMainFormSaveCreateRevertButtons(SavedState.confirmed);
            NotationGroupBox.Enabled = true;
            KrystalsGroupBox.Enabled = true;
            PalettesGroupBox.Enabled = true;
        }
        private void SetMainFormIsSaved()
        {
            _fsf.SetFormState(this, SavedState.saved);
            SetMainFormShowFormsButtons(_fsf.UnconfirmedFormsExist(), _fsf.ConfirmedFormsExist());
            SetMainFormSaveCreateRevertButtons(SavedState.saved);
            NotationGroupBox.Enabled = true;
            KrystalsGroupBox.Enabled = true;
            PalettesGroupBox.Enabled = true;
            DisableGroupConfirmRevertButtons();
        }
        #region helpers
        private void SetEnabledButtonToLightGreen(Button button)
        {
            if(button.Enabled)
                button.BackColor = M.LightGreenButtonColor;
            else
                button.BackColor = Color.Transparent;
        }
        private void SetMainFormShowFormsButtons(bool formsNeedReview, bool confirmedFormsExist)
        {
            ShowUncheckedFormsButton.Enabled = formsNeedReview;
            SetEnabledButtonToLightGreen(ShowUncheckedFormsButton);
            ShowConfirmedFormsButton.Enabled = confirmedFormsExist;
            SetEnabledButtonToLightGreen(ShowConfirmedFormsButton);
        }
        private void SetMainFormSaveCreateRevertButtons(SavedState state)
        {
            SetMainFormSaveCreateButtonText(_fsf.UnconfirmedFormsExist() || _fsf.ConfirmedFormsExist());

            SaveSettingsCreateScoreButton.Enabled = !(state == SavedState.unconfirmed);
            RevertEverythingButton.Enabled = (_fsf.UnconfirmedFormsExist() || _fsf.ConfirmedFormsExist());
        }
        #endregion helpers
        #endregion main SetMainFormIs... functions

        #endregion state of main form

        #region state of group boxes in main form
        /// <summary>
        /// Called when the user changes any setting in a groupBox.
        /// </summary>
        private void SetGroupBoxIsUnconfirmed(GroupBox groupBox, Button confirmGroupBoxButton, Button revertGroupBoxButton)
        {
            _fsf.SetGroupBoxState(groupBox, SavedState.unconfirmed);
            // sets the main form's text and the groupBox buttons
            _fsf.SetSettingsAreUnconfirmed(this, M.HasError(_allTextBoxes), confirmGroupBoxButton, revertGroupBoxButton);
            if(groupBox == NotationGroupBox)
            {
                KrystalsGroupBox.Enabled = false;
                PalettesGroupBox.Enabled = false;
            }
            else if(groupBox == KrystalsGroupBox)
            {
                NotationGroupBox.Enabled = false;
                PalettesGroupBox.Enabled = false;
            }
            else if(groupBox == PalettesGroupBox)
            {
                KrystalsGroupBox.Enabled = false;
                NotationGroupBox.Enabled = false;
            }

            SetMainFormIsUnconfirmed();
        }
        /// <summary>
        /// Called when one of the groupBox Confirm buttons is clicked.
        /// </summary>
        private void SetGroupBoxIsConfirmed(GroupBox groupBox, Button confirmGroupBoxButton,
            SavedState otherGroupBox1State, SavedState otherGroupBox2State)
        {
            _fsf.SetGroupBoxState(groupBox, SavedState.confirmed);
            _fsf.SetSettingsAreConfirmed(this, M.HasError(_allTextBoxes), confirmGroupBoxButton);

            NotationGroupBox.Enabled = true;
            KrystalsGroupBox.Enabled = true;
            PalettesGroupBox.Enabled = true;

            if(otherGroupBox1State == SavedState.unconfirmed
            || otherGroupBox2State == SavedState.unconfirmed
            || OtherFormStateIs(SavedState.unconfirmed))
                SetMainFormIsUnconfirmed();
            else
                SetMainFormIsConfirmed();
        }
        /// <summary>
        /// Called when a groupBox has been individually reverted.
        /// </summary>
        private void SetGroupBoxIsSaved(GroupBox groupBox, Button confirmButton, Button revertToSavedButton,
                        SavedState otherGroupBox1State, SavedState otherGroupBox2State)
        {
            _fsf.SetGroupBoxState(groupBox, SavedState.saved);
            _fsf.SetSettingsAreSaved(this, M.HasError(_allTextBoxes), confirmButton, revertToSavedButton);

            if(otherGroupBox1State == SavedState.unconfirmed || otherGroupBox2State == SavedState.unconfirmed
                   || OtherFormStateIs(SavedState.unconfirmed))
            {
                SetMainFormIsUnconfirmed();
            }
            else if(otherGroupBox1State == SavedState.confirmed || otherGroupBox2State == SavedState.confirmed
                || OtherFormStateIs(SavedState.confirmed))
            {
                SetMainFormIsConfirmed();
            }
            else
            {
                SetMainFormIsSaved();
            }
        }
        #region helper
        private bool OtherFormStateIs(SavedState state)
        {
            bool rval = false;
            if((SavedState)_dimensionsAndMetadataForm.Tag == state)
                rval = true;
            else
            {
                foreach(PaletteForm paletteForm in PalettesListBox.Items)
                {
                    if(((SavedState)paletteForm.Tag == state)
                    || (paletteForm.OrnamentsForm != null && (SavedState)paletteForm.OrnamentsForm.Tag == state))
                    {
                        rval = true;
                        break;
                    }
                }
            }
            return rval;
        }
        #endregion helper
        #endregion state of group boxes in main form

        #endregion AssistantComposer form state

        #region notation groupBox
        #region comboBoxes
        private void StafflineStemStrokeWidthComboBox_Leave(object sender, EventArgs e)
        {
            SetComboBoxSelectedIndexFromText(StafflineStemStrokeWidthComboBox);
            SetGroupBoxIsUnconfirmed(NotationGroupBox, ConfirmNotationButton, RevertNotationButton);
        }
        private void StafflineStemStrokeWidthComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetGroupBoxIsUnconfirmed(NotationGroupBox, ConfirmNotationButton, RevertNotationButton);
        }
        /// <summary>
        /// This function ensures that the user can only use values in the ChordTypeComboBox's item list.
        /// Typing custom values won't work.
        /// </summary>
        private void GapPixelsComboBox_Leave(object sender, EventArgs e)
        {
            SetComboBoxSelectedIndexFromText(GapPixelsComboBox);
            SetGroupBoxIsUnconfirmed(NotationGroupBox, ConfirmNotationButton, RevertNotationButton);
        }
        private void GapPixelsComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetGroupBoxIsUnconfirmed(NotationGroupBox, ConfirmNotationButton, RevertNotationButton);
        }
        #endregion comboBoxes
        private void BeamsCrossBarlinesCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            SetGroupBoxIsUnconfirmed(NotationGroupBox, ConfirmNotationButton, RevertNotationButton);
        }
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
            SetGroupBoxIsUnconfirmed(NotationGroupBox, ConfirmNotationButton, RevertNotationButton);
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
        /// This function sets both _visibleOutputVoiceIndicesPerStaff and _inputVoiceIndicesPerStaff
        /// (and consequetially _numberOfStaves).
        /// It also sets _staffIsInput and the help texts on the dialog.
        /// </summary>
        private void VoiceIndicesPerStaffTextBox_Leave(object sender, EventArgs e)
        {
            EnableStaffDependentControls(false);

            bool error = false;

            string[] outInStrings = VoiceIndicesPerStaffTextBox.Text.Split('|');
            List<List<byte>> visibleOutputIndexLists = new List<List<byte>>();
            List<List<byte>> inputIndexLists = new List<List<byte>>();
            outInStrings[0] = outInStrings[0].Trim();
            if(outInStrings.Length > 1)
                outInStrings[1] = outInStrings[1].Trim();

            if(!string.IsNullOrEmpty(outInStrings[0]))
            {
                try
                {
                    visibleOutputIndexLists = M.StringToByteLists(outInStrings[0]);
                }
                catch
                {
                    error = true;
                }
            }
            if(!error && outInStrings.Length > 1 && !string.IsNullOrEmpty(outInStrings[1]))
            {
                try
                {
                    inputIndexLists = M.StringToByteLists(outInStrings[1]);
                }
                catch
                {
                    error = true;
                }
            }

            if(!error && ((visibleOutputIndexLists.Count + inputIndexLists.Count) < 1))
            {
                // there must be at least one visible voice in the score
                error = true;
            }


            if(!error && visibleOutputIndexLists.Count > 0)
            {
                error = CheckVoiceIndices(visibleOutputIndexLists, _outputVoiceIndices);
            }
            if(!error && inputIndexLists.Count > 0)
            {
                List<byte> inputVoiceIndices = InputVoiceIndices(_algorithm.NumberOfInputVoices);
                error = CheckVoiceIndices(inputIndexLists, inputVoiceIndices);
            }

            if(error)
            {
                M.SetTextBoxErrorColorIfNotOkay(VoiceIndicesPerStaffTextBox, false);
                VoiceIndicesPerStaffTextBox.Focus();
            }
            else
            {
                _visibleOutputVoiceIndicesPerStaff = visibleOutputIndexLists;
                _visibleInputVoiceIndicesPerStaff = inputIndexLists;
                EnableStaffDependentControls(true);
                VoiceIndicesPerStaffTextBox.Text = NormalizedVoiceIndicesString(visibleOutputIndexLists, inputIndexLists);
                M.SetTextBoxErrorColorIfNotOkay(VoiceIndicesPerStaffTextBox, true);
            }
            SetGroupBoxIsUnconfirmed(NotationGroupBox, ConfirmNotationButton, RevertNotationButton);
        }

        private List<byte> InputVoiceIndices(int numberOfVoices)
        {
            List<byte> rval = new List<byte>();
            for(byte i = 0; i < numberOfVoices; ++i)
                rval.Add(i);
            return rval;
        }

        private bool CheckVoiceIndices(List<List<byte>> voiceIndexLists, List<byte> availableIndices)
        {
            bool error = false;
            foreach(List<byte> voiceIndicesPerStaff in voiceIndexLists)
            {
                if(voiceIndicesPerStaff.Count == 0 || voiceIndicesPerStaff.Count > 2)
                {
                    error = true;
                    break;
                }
                foreach(byte ovIndex in voiceIndicesPerStaff)
                {
                    if(!availableIndices.Contains(ovIndex))
                    {
                        error = true;
                        break;
                    }
                }
                if(error)
                    break;
            }
            return error;
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

            if(okay && trimmedClefs.Count == _numberOfStaves  && _numberOfStaves > 0)
            {
                ClefsPerStaffTextBox.Text = NormalizedText(trimmedClefs);
                M.SetTextBoxErrorColorIfNotOkay(ClefsPerStaffTextBox, true);
                CheckClefsAndStafflineNumbers();
            }
            else
                M.SetTextBoxErrorColorIfNotOkay(ClefsPerStaffTextBox, false);

            SetGroupBoxIsUnconfirmed(NotationGroupBox, ConfirmNotationButton, RevertNotationButton);
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
            SetGroupBoxIsUnconfirmed(NotationGroupBox, ConfirmNotationButton, RevertNotationButton);
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
                bool outputOK = false;
                foreach(byte b in bytes)
                {
                    sum += (int)b;
                    if(sum == _visibleOutputVoiceIndicesPerStaff.Count
                        || (_visibleOutputVoiceIndicesPerStaff.Count == 0 && sum == _visibleInputVoiceIndicesPerStaff.Count))
                        outputOK = true;
                }
                    
                if(! outputOK || sum != _numberOfStaves)
                    M.SetTextBoxErrorColorIfNotOkay(StaffGroupsTextBox, false);
            }
            SetGroupBoxIsUnconfirmed(NotationGroupBox, ConfirmNotationButton, RevertNotationButton);
        }
        private void CheckStaffNames(TextBox textBox)
        {
            string[] names = textBox.Text.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            List<string> trimmedNames = new List<string>();
            foreach(string name in names)
            {
                trimmedNames.Add(name.Trim());
            }
            if(trimmedNames.Count == _numberOfStaves && _numberOfStaves > 0)
            {
                textBox.Text = NormalizedText(trimmedNames);
                M.SetTextBoxErrorColorIfNotOkay(textBox, true);
            }
            else
            {
                M.SetTextBoxErrorColorIfNotOkay(textBox, false);
            }
            SetGroupBoxIsUnconfirmed(NotationGroupBox, ConfirmNotationButton, RevertNotationButton);
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
            SetGroupBoxIsUnconfirmed(NotationGroupBox, ConfirmNotationButton, RevertNotationButton);
        }
        #endregion  TextBox Leave events
        #region TextChanged events (these just set the text boxes to white)
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
        private void VoiceIndicesPerStaffTextBox_TextChanged(object sender, EventArgs e)
        {
            M.SetToWhite(VoiceIndicesPerStaffTextBox);
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
        #region helper functions
        private string NormalizedVoiceIndicesString(List<List<byte>> visibleOutputIndexLists, List<List<byte>> inputIndexLists)
        {
            StringBuilder sb = new StringBuilder();
            if(visibleOutputIndexLists.Count > 0)
            {
                AppendOutInToSB(visibleOutputIndexLists, sb);
                sb.Append(" ");
            }
            if(inputIndexLists.Count > 0)
            {
                sb.Append("| ");
                AppendOutInToSB(inputIndexLists, sb);
            }
            return sb.ToString();
        }
        private static void AppendOutInToSB(List<List<byte>> indexLists, StringBuilder sb)
        {
            StringBuilder appendage = new StringBuilder();
            foreach(List<byte> bytes in indexLists)
            {
                appendage.Append(", ");
                StringBuilder bytesListSB = new StringBuilder();
                foreach(byte b in bytes)
                {
                    bytesListSB.Append(":");
                    bytesListSB.Append(b.ToString());
                }
                bytesListSB.Remove(0, 1);
                appendage.Append(bytesListSB.ToString());
            }
            appendage.Remove(0, 2);
            sb.Append(appendage);
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
                this.StaffGroupsHelpLabel.Text = "integers (whose total is " + _numberOfStaves.ToString() + ")\n" +
                                                "staff groups cannot contain both\ninput and output staves.";
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
        private void VoiceIndicesPerStaffHelp_MouseClick(object sender, MouseEventArgs e)
        {
            if(e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                string mainText =
                "Composition algorithms create output and input voices without deciding on the layout for a particular " +
                "score. Many different layouts are possible using the same logical information. (Output voices are " +
                "always created, but input voices do not necessarily exist.)\n\n" +

                "Each algorithm presents the voices it creates in the top-bottom order of a default layout. Voices are " +
                "therefore identified in this dialog using their default index.\n" +
                "(The default top output voice has index 0, as does the default top input voice.)\n" +
                "The available output and input voice indices are displayed in a help text above this input field. " +
                "Entering the help string as it stands will therefore create a score showing all the composed voices, " +
                "with one voice per staff, in the default order in which they were originally composed.\n\n" +

                "The string entered in this input field controls both the number of staves per system, and the " +
                "top-bottom order of the voices to be displayed.\n" +
                "It can have either one or two parts. The first controls the output voices, the second controls the " +
                "input voices. If present, the input voices are separated from the output voices by a '|' character. " +
                "Either of these parts can be empty, but not both: there must be at least one visible output or input " +
                "voice in the score.\n\n" +

                "There can be either one or two voices per staff. Voices on the same staff are separated by a ':'. " +
                "Staves are separated by a ','. White space is ignored.\n" +
                "For example, if an algorithm creates four output and two input voices, the help text would be: '0, 1, " +
                "2, 3 | 0, 1', and some possible score formatting strings are:\n" +
                "     '2'      One output staff per system. The staff shows output voice index 2.\n" +
                "     '|0'     One input staff per system. The staff shows input voice index 0.\n" +
                "     '1, 3 , 2 | 0:1'  Three visible output staves and one input staff per system. The input staff " +
                "has two input voices.\n" +
                "The numbers can be in any order, but must be selected from those given in the help text. The order " +
                "determines the top-bottom order of the visible voices in the final score.\n\n" +

                "The score is always printed with output staves above any input staves. Output staves are smaller than " +
                "input staves.\n\n" +

                "If there are any visible input staves, then all the output voices that are not entered in this input " +
                "field will automatically be included on invisible staves in the score's file. These invisible output " +
                "staves contain midi information, referenced by notes in the input voices, but no graphics. They are " +
                "currently written in a single block above the topmost visible staff in the file.\n\n" +
                
                "A note on midi channels:\n" +
                "Algorithms compose output voices complete with their midi channel (and master volume initialization " +
                "value). This allows them to stipulate the standard midi percussion channel (channel index 9) if they " +
                "want to.\n" +
                "The midi channel and master volume values are therefore fixed attributes of particular voices, and " +
                "move with them when the voices are re-ordered.\n\n";

                MessageBox.Show(mainText, "Help for the 'voices per staff' input field", MessageBoxButtons.OK);
            }
        }
        #endregion notation groupBox

        #region krystals groupBox
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
                KrystalsListBox.SetSelected(KrystalsListBox.Items.Count - 1, true); // triggers KrystalsListBox_SelectedIndexChanged()
                SetGroupBoxIsUnconfirmed(KrystalsGroupBox, ConfirmKrystalsListButton, RevertKrystalsListButton);
            }
            this.BringToFront();
        }
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
                    SetGroupBoxIsUnconfirmed(KrystalsGroupBox, ConfirmKrystalsListButton, RevertKrystalsListButton);
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
        #endregion krystals groupBox

        #region palettes groupBox
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
            //SetGroupBoxIsUnconfirmed(PalettesGroupBox, ConfirmPalettesListButton, RevertPalettesButton);
        }
        private void AddPaletteButton_Click(object sender, EventArgs e)
        {
            NewPaletteDialog dialog = new NewPaletteDialog();
            if(dialog.ShowDialog() == DialogResult.OK)
            {
                List<PaletteForm> currentPaletteForms = CurrentPaletteForms;
                if(PaletteNameAlreadyExists(dialog, currentPaletteForms))
                {
                    MessageBox.Show("That name already exists!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    PaletteForm paletteForm = new PaletteForm(this, dialog.PaletteName, dialog.PaletteDomain, _fsf);
                    currentPaletteForms.Add(paletteForm);
                    CurrentPaletteForms = currentPaletteForms;
                    PalettesListBox.SelectedIndex = PalettesListBox.Items.Count - 1;
                    paletteForm.Show();
                    paletteForm.BringToFront();

                    SetGroupBoxIsUnconfirmed(PalettesGroupBox, null, RevertPalettesListButton);
                }
            }
        }
        private bool PaletteNameAlreadyExists(NewPaletteDialog dialog, List<PaletteForm> currentPaletteForms)
        {
            bool rval = false;
            foreach(PaletteForm paletteForm in currentPaletteForms)
            {
                if(paletteForm.SavedName == dialog.PaletteName)
                {
                    rval = true;
                    break;
                }
            }
            return rval;
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
                    paletteForm.Delete();
                    currentPaletteForms.RemoveAt(selectedIndex);
                    CurrentPaletteForms = currentPaletteForms;

                    SetGroupBoxIsUnconfirmed(PalettesGroupBox, null, RevertPalettesListButton);
                }
            }
            SetGroupBoxIsUnconfirmed(PalettesGroupBox, null, RevertPalettesListButton);

        }
        private void PalettesListBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            DoSelectionColor(sender, e);
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
        #endregion palettes groupBox

        #region load settings
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
                Debug.Assert(!_fsf.UnconfirmedFormsExist());
                Debug.Assert(!_fsf.ConfirmedFormsExist());
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
                    case "voiceIndicesPerStaff":
                        VoiceIndicesPerStaffTextBox.Text = r.Value;
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
            SetGroupBoxIsSaved(NotationGroupBox, ConfirmNotationButton, RevertNotationButton,
                (SavedState)NotationGroupBox.Tag, (SavedState)KrystalsGroupBox.Tag);
        }
        #region helpers
        private void SetBeamsCrossBarlinesCheckBoxChecked(string value)
        {
            if(value == "true")
                BeamsCrossBarlinesCheckBox.Checked = true;
            else
                BeamsCrossBarlinesCheckBox.Checked = false;
        }
        private void SetStafflineStemStrokeWidth(string value)
        {
            int item = 0;
            do
            {
                StafflineStemStrokeWidthComboBox.SelectedIndex = item++;
            } while(value != StafflineStemStrokeWidthComboBox.SelectedItem.ToString());
        }
        private void SetGapPixelsComboBox(string value)
        {
            int item = 0;
            do
            {
                GapPixelsComboBox.SelectedIndex = item++;
            } while(value != GapPixelsComboBox.SelectedItem.ToString());
        }
        #endregion helpers
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
            SetGroupBoxIsSaved(KrystalsGroupBox, ConfirmKrystalsListButton, RevertKrystalsListButton,
                (SavedState)NotationGroupBox.Tag, (SavedState)PalettesGroupBox.Tag);
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

                    PaletteForm paletteForm = new PaletteForm(r, this, name, domain, isPercussionPalette, _fsf);

                    PalettesListBox.Items.Add(paletteForm);

                    M.ReadToXmlElementTag(r, "palette", "palettes");
                }
            }
            this.PalettesListBox.ResumeLayout();
            Debug.Assert(r.Name == "palettes");
            SetGroupBoxIsSaved(PalettesGroupBox, null, RevertPalettesListButton,
                (SavedState)NotationGroupBox.Tag, (SavedState)KrystalsGroupBox.Tag);
            PalettesListBox.Refresh();
        }
        #endregion load settings

        #region save settings
        public void SaveSettings()
        {
            Debug.Assert(!string.IsNullOrEmpty(_settingsPath));

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
                w.WriteAttributeString("xsi", "noNamespaceSchemaLocation", null, M.Preferences.OnlineXMLSchemasFolder + "/moritzKrystalScore.xsd");

                _dimensionsAndMetadataForm.Write(w);

                WriteNotation(w);
                WriteKrystals(w);
                WritePalettes(w);
                w.WriteEndElement(); // closes the moritzKrystalScore element
                w.Close(); // close unnecessary because of the using statement?
            }
            #endregion do the save
        }
        private void WriteNotation(XmlWriter w)
        {
            w.WriteStartElement("notation");

            w.WriteAttributeString("minimumCrotchetDuration", MinimumCrotchetDurationTextBox.Text);
            if(BeamsCrossBarlinesCheckBox.Checked)
                w.WriteAttributeString("beamsCrossBarlines", "true");
            else
                w.WriteAttributeString("beamsCrossBarlines", "false");
            w.WriteAttributeString("stafflineStemStrokeWidth", StafflineStemStrokeWidthComboBox.Text);
            w.WriteAttributeString("gap", GapPixelsComboBox.Text);
            w.WriteAttributeString("minGapsBetweenStaves", this.MinimumGapsBetweenStavesTextBox.Text);
            w.WriteAttributeString("minGapsBetweenSystems", MinimumGapsBetweenSystemsTextBox.Text);
            w.WriteAttributeString("voiceIndicesPerStaff", VoiceIndicesPerStaffTextBox.Text);
            w.WriteAttributeString("clefsPerStaff", ClefsPerStaffTextBox.Text);
            w.WriteAttributeString("stafflinesPerStaff", StafflinesPerStaffTextBox.Text);
            w.WriteAttributeString("staffGroups", StaffGroupsTextBox.Text);
            w.WriteAttributeString("longStaffNames", LongStaffNamesTextBox.Text);
            w.WriteAttributeString("shortStaffNames", ShortStaffNamesTextBox.Text);
            w.WriteAttributeString("systemStartBars", SystemStartBarsTextBox.Text);

            w.WriteEndElement(); // notation

            SetGroupBoxIsSaved(NotationGroupBox, ConfirmNotationButton, RevertNotationButton,
                (SavedState)NotationGroupBox.Tag, (SavedState)KrystalsGroupBox.Tag);

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
            SetGroupBoxIsSaved(KrystalsGroupBox, ConfirmKrystalsListButton, RevertKrystalsListButton,
                (SavedState)NotationGroupBox.Tag, (SavedState)PalettesGroupBox.Tag);
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
            SetGroupBoxIsSaved(PalettesGroupBox, null, RevertPalettesListButton,
                (SavedState)NotationGroupBox.Tag, (SavedState)KrystalsGroupBox.Tag);
        }
        #endregion save settings

        #region create score
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
        #region helpers
        private void SetPaperSize(PageFormat pageFormat)
        {
            pageFormat.PaperSize = _dimensionsAndMetadataForm.PaperSize;
            pageFormat.IsLandscape = _dimensionsAndMetadataForm.Landscape;
            int bottomPX;
            int rightPX;
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
            pageFormat.ChordSymbolType = "standard";
            pageFormat.MinimumCrotchetDuration = int.Parse(this.MinimumCrotchetDurationTextBox.Text);
            pageFormat.BeamsCrossBarlines = this.BeamsCrossBarlinesCheckBox.Checked;

            float strokeWidth = float.Parse(StafflineStemStrokeWidthComboBox.Text, M.En_USNumberFormat) * pageFormat.ViewBoxMagnification;
            pageFormat.StafflineStemStrokeWidth = strokeWidth;
            pageFormat.Gap = float.Parse(GapPixelsComboBox.Text, M.En_USNumberFormat) * pageFormat.ViewBoxMagnification;
            pageFormat.DefaultDistanceBetweenStaves = int.Parse(MinimumGapsBetweenStavesTextBox.Text) * pageFormat.Gap;
            pageFormat.DefaultDistanceBetweenSystems = int.Parse(MinimumGapsBetweenSystemsTextBox.Text) * pageFormat.Gap;

            pageFormat.VisibleOutputVoiceIndicesPerStaff = _visibleOutputVoiceIndicesPerStaff; // one value per output staff
            pageFormat.VisibleInputVoiceIndicesPerStaff = _visibleInputVoiceIndicesPerStaff; // one value per input staff
            pageFormat.ClefsList = M.StringToStringList(this.ClefsPerStaffTextBox.Text, ',');
            pageFormat.StafflinesPerStaff = M.StringToIntList(this.StafflinesPerStaffTextBox.Text, ',');
            pageFormat.StaffGroups = M.StringToIntList(this.StaffGroupsTextBox.Text, ',');

            pageFormat.LongStaffNames = M.StringToStringList(this.LongStaffNamesTextBox.Text, ',');
            pageFormat.ShortStaffNames = M.StringToStringList(this.ShortStaffNamesTextBox.Text, ',');
            pageFormat.SystemStartBars = M.StringToIntList(SystemStartBarsTextBox.Text, ',');
        }
        #endregion helpers
        #endregion
        #endregion create score

        #region private variables
        #region Brushes
        SolidBrush _systemHighlightBrush = new SolidBrush(SystemColors.Highlight);
        SolidBrush _whiteBrush = new SolidBrush(Color.White);
        SolidBrush _blackBrush = new SolidBrush(Color.Black);
        #endregion Brushes
        #region names and paths
        /// <summary>
        /// The complete path of the file containing the settings.
        /// This is the _settingsFolderPath plus the scoreTitle plus the ".mkss" suffix.
        /// </summary>
        string _settingsPath = null;
        /// <summary>
        /// The folder (in the AssistantPerformer's folder)
        /// in which the score and all its associated files is saved
        /// </summary>
        string _settingsFolderPath = null;
        /// <summary>
        /// _scoreTitle is the name of the score without any path or extension. For example "Study 2b2".
        /// </summary>
        string _scoreTitle = null;
        #endregion names and paths
        #region editing
        private IMoritzForm1 _moritzForm1;
        private KrystalBrowser _krystalBrowser = null;
        private DimensionsAndMetadataForm _dimensionsAndMetadataForm;
        private FormStateFunctions _fsf;
        private List<TextBox> _allTextBoxes = null;
        #endregion editing
        #region score creation
        CompositionAlgorithm _algorithm = null;
        private List<byte> _outputVoiceIndices = null;
        private List<List<byte>> _visibleOutputVoiceIndicesPerStaff; // set in OutputVoiceIndicesPerStaffTextBox_Leave
        private List<List<byte>> _visibleInputVoiceIndicesPerStaff; // set in InputVoiceIndicesPerStaffTextBox_Leave
        private int _numberOfStaves
        {
            get
            {
                if(_visibleOutputVoiceIndicesPerStaff != null && _visibleInputVoiceIndicesPerStaff != null)
                {
                    return _visibleOutputVoiceIndicesPerStaff.Count + _visibleInputVoiceIndicesPerStaff.Count;
                }
                else
                {
                    return 0;
                }
            }
        }
        public PageFormat PageFormat = null;
        #endregion score creation

        #endregion private variables
    }
}