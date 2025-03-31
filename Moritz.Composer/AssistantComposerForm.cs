using Krystals5ObjectLibrary;

using Moritz.Algorithm;
using Moritz.Globals;
using Moritz.Symbols;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml;

namespace Moritz.Composer
{
    public partial class AssistantComposerForm : Form
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

            _algorithm = ComposableScore.Algorithm(_scoreTitle);

            Debug.Assert(_algorithm != null);

            GetSelectedSettings();

            if(NumberOfVoicesPerStaffTextBox.Text == "")
            {
                SetDefaultVoiceIndicesPerStaff(_algorithm.NumberOfMidiChannels);
            }
        }
        #region called from ctor

        private List<TextBox> GetAllTextBoxes()
        {
            List<TextBox> textBoxes = new List<TextBox>
            {
                MinimumGapsBetweenStavesTextBox,
                MinimumGapsBetweenSystemsTextBox,
                MinimumCrotchetDurationTextBox,

                NumberOfVoicesPerStaffTextBox,
                ClefsPerStaffTextBox,
                StafflinesPerStaffTextBox,
                StaffGroupsTextBox,

                LongStaffNamesTextBox,
                ShortStaffNamesTextBox,

                SystemStartBarsTextBox
            };
            return textBoxes;
        }
        private void ClearListBoxes()
        {
            KrystalsListBox.SuspendLayout();
            KrystalsListBox.Items.Clear();
            KrystalsListBox.ResumeLayout();
        }
        private void SetDefaultValues()
        {
            this.NotationGroupBox.Tag = SavedState.unconfirmed;
            this.KrystalsGroupBox.Tag = SavedState.unconfirmed;

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

        #endregion DeselectAll
        private void GetSelectedSettings()
        {
            LoadSettings();

            SetSystemStartBarsHelpLabel(_algorithm.NumberOfBars);

            NumberOfVoicesPerStaffTextBox_Leave(null, null); // sets _numberOfOutputStaves _numberOfStaves

            SetGroupBoxIsSaved(NotationGroupBox, ConfirmNotationButton, RevertNotationButton, (SavedState)KrystalsGroupBox.Tag);
        }
        #region helpers

        private void SetSystemStartBarsHelpLabel(int numberOfBars)
        {
            SystemStartBarsHelpLabel.Text = "(" + numberOfBars.ToString() + " bars. Default is 1 bar per system)";
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
            NumberOfVoicesPerStaffTextBox.Text = voiceIndexList.ToString();
            NumberOfVoicesPerStaffTextBox_Leave(null, null);
        }
        #endregion called from ctor

        #region public interface
        /// <summary>
        /// Used when reverting other forms.
        /// </summary>
        public string SettingsPath { get { return _settingsPath; } }
        public string LocalScoreAudioPath
        {
            get
            {
                string path = M.MoritzAudioFolder + @"\" + _scoreTitle;
                return path;
            }
        }
        public void UpdateMainFormState()
        {
            if(((SavedState)NotationGroupBox.Tag == SavedState.unconfirmed)
                || ((SavedState)KrystalsGroupBox.Tag == SavedState.unconfirmed)
                || ((SavedState)_dimensionsAndMetadataForm.Tag == SavedState.unconfirmed))
            {
                SetMainFormIsUnconfirmed();
            }
            else if(((SavedState)NotationGroupBox.Tag == SavedState.confirmed)
                 || ((SavedState)KrystalsGroupBox.Tag == SavedState.confirmed)
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
            Graphics g = e.Graphics;

            if(sender is ListBox listBox && listBox.Items.Count > e.Index)
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

        private void AssistantComposerForm_FormClosing(object sender, FormClosingEventArgs e)
        {
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
                (SavedState)KrystalsGroupBox.Tag);
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
                    (SavedState)KrystalsGroupBox.Tag);
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
            SetGroupBoxIsConfirmed(KrystalsGroupBox, ConfirmKrystalsListButton, (SavedState)NotationGroupBox.Tag);
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
                    (SavedState)NotationGroupBox.Tag);
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
                // The settings are saved here in case there is going to be an error while creating the score.
                SaveSettings();
                SetMainFormIsSaved();
            }
            else
            {
                Debug.Assert(SaveSettingsCreateScoreButton.Text.StartsWith("create"));
                CreateSVGScore();
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

                _dimensionsAndMetadataForm.Close();
                _dimensionsAndMetadataForm = new DimensionsAndMetadataForm(this, _settingsPath, _fsf);

                LoadSettings(); // clears existing settings

                ReshowForms(dimensionsAndMetadataFormLocation);
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

        private void ReshowForms(Point dimensionsAndMetadataFormLocation)
        {
            if(dimensionsAndMetadataFormLocation.X > int.MinValue)
            {
                _dimensionsAndMetadataForm.Show();
                _dimensionsAndMetadataForm.Location = dimensionsAndMetadataFormLocation;
            }
            else
                _dimensionsAndMetadataForm.Hide();

            this.BringToFront();
        }
        #endregion helpers

        private void QuitMoritzButton_Click(object sender, EventArgs e)
        {
            if(DiscardAnyChanges())
            {
                _moritzForm1.Close();
            }
        }
        private void QuitAssistantComposerButton_Click(object sender, EventArgs e)
        {
            if(DiscardAnyChanges())
            {
                _moritzForm1.CloseAssistantComposer();
            }
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
        }
        private void SetMainFormIsSaved()
        {
            _fsf.SetFormState(this, SavedState.saved);
            SetMainFormShowFormsButtons(_fsf.UnconfirmedFormsExist(), _fsf.ConfirmedFormsExist());
            SetMainFormSaveCreateRevertButtons(SavedState.saved);
            NotationGroupBox.Enabled = true;
            KrystalsGroupBox.Enabled = true;
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
            }
            else if(groupBox == KrystalsGroupBox)
            {
                NotationGroupBox.Enabled = false;

            }

            SetMainFormIsUnconfirmed();
        }
        /// <summary>
        /// Called when one of the groupBox Confirm buttons is clicked.
        /// </summary>
        private void SetGroupBoxIsConfirmed(GroupBox groupBox, Button confirmGroupBoxButton,
            SavedState otherGroupBox1State)
        {
            _fsf.SetGroupBoxState(groupBox, SavedState.confirmed);
            _fsf.SetSettingsAreConfirmed(this, M.HasError(_allTextBoxes), confirmGroupBoxButton);

            NotationGroupBox.Enabled = true;
            KrystalsGroupBox.Enabled = true;

            if(otherGroupBox1State == SavedState.unconfirmed
            || OtherFormStateIs(SavedState.unconfirmed))
                SetMainFormIsUnconfirmed();
            else
                SetMainFormIsConfirmed();
        }
        /// <summary>
        /// Called when a groupBox has been individually reverted.
        /// </summary>
        private void SetGroupBoxIsSaved(GroupBox groupBox, Button confirmButton, Button revertToSavedButton,
                        SavedState otherGroupBox1State)
        {
            _fsf.SetGroupBoxState(groupBox, SavedState.saved);
            _fsf.SetSettingsAreSaved(this, M.HasError(_allTextBoxes), confirmButton, revertToSavedButton);

            if(otherGroupBox1State == SavedState.unconfirmed || OtherFormStateIs(SavedState.unconfirmed))
            {
                SetMainFormIsUnconfirmed();
            }
            else if(otherGroupBox1State == SavedState.confirmed || OtherFormStateIs(SavedState.confirmed))
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
            {  
                rval = true;
            }   

            return rval;
        }
        #endregion helper
        #endregion state of group boxes in main form

        #endregion AssistantComposer form state

        #region notation groupBox
        #region comboBoxes

        private void NotatorComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetGroupBoxIsUnconfirmed(NotationGroupBox, ConfirmNotationButton, RevertNotationButton);
        }
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
        /// This function sets _voiceIndicesPerStaff and _numberOfStaves.
        /// </summary>
        private void NumberOfVoicesPerStaffTextBox_Leave(object sender, EventArgs e)
        {
            EnableStaffDependentControls(false);

            bool error = false;

            List<int> nVoicesPerStaff = M.StringToIntList(NumberOfVoicesPerStaffTextBox.Text, ',');

            if(nVoicesPerStaff.Count == 0)
            {
                error = true;
            }
            foreach(int nVoices in nVoicesPerStaff)
            {
                if(nVoices < 1 || nVoices > 2)
                {
                    error = true;
                }
            }

            if(error)
            {
                M.SetTextBoxErrorColorIfNotOkay(NumberOfVoicesPerStaffTextBox, false);
                NumberOfVoicesPerStaffTextBox.Focus();
            }
            else
            {
                _voiceIndicesPerStaff = new List<List<int>>();
                int midiIndex = 0;
                foreach(var nVoices in nVoicesPerStaff)
                {
                    var staffMidiChannels = new List<int>();
                    staffMidiChannels.Add(midiIndex++);
                    if(nVoices > 1)
                    {
                        staffMidiChannels.Add(midiIndex++);
                    }
                    _voiceIndicesPerStaff.Add(staffMidiChannels);
                }
                M.SetTextBoxErrorColorIfNotOkay(NumberOfVoicesPerStaffTextBox, true);
                EnableStaffDependentControls(true);
                NumberOfVoicesPerStaffTextBox.Text = NormalizedNumberOfVoicesPerStaffString(_voiceIndicesPerStaff);
            }
            SetGroupBoxIsUnconfirmed(NotationGroupBox, ConfirmNotationButton, RevertNotationButton);
        }

        private bool CheckMIDIChannelIndices(List<List<int>> midiChannelsPerStaff, List<int> availableMIDIChannels)
        {
            Debug.Assert(availableMIDIChannels != null);

            bool error = false;
            var flatMIDIChannels = new List<int>();
            #region get flat indices
            foreach(List<int> midiChannels in midiChannelsPerStaff)
            {
                if(midiChannels.Count == 0 || midiChannels.Count > 2)
                {
                    error = true;
                    break;
                }
                foreach(var index in midiChannels)
                {
                    flatMIDIChannels.Add(index);
                }
            }
            #endregion
            if(error == false)
            {
                if(error == false && flatMIDIChannels.Count != availableMIDIChannels.Count)
                {
                    error = true;
                }
                else
                {
                    for(var i = 0; i < availableMIDIChannels.Count; ++i)
                    {
                        if(flatMIDIChannels[i] != availableMIDIChannels[i])
                        {
                            error = true;
                            break;
                        }
                    }
                }
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

            if(okay && trimmedClefs.Count == _numberOfStaves && _numberOfStaves > 0)
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
                List<int> ints = M.StringToIntList(StaffGroupsTextBox.Text, ',');
                int sum = 0;
                foreach(int b in ints)
                {
                    sum += (int)b;
                }
                if(sum != _numberOfStaves)
                {
                    M.SetTextBoxErrorColorIfNotOkay(StaffGroupsTextBox, false);
                }
            }
            SetGroupBoxIsUnconfirmed(NotationGroupBox, ConfirmNotationButton, RevertNotationButton);
        }
        private void CheckStaffNames(TextBox textBox)
        {
            string[] names = textBox.Text.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            List<string> trimmedNames = new List<string>();
            foreach(string name in names)
            {
                if(name == " ")
                {
                    trimmedNames.Add(" ");
                }
                else
                {
                    trimmedNames.Add(name.Trim());
                }
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
            List<int> startBars = new List<int>();
            if(SystemStartBarsTextBox.Text.Length == 0)
            {
                for(int i = 1; i <= _algorithm.NumberOfBars; ++i)
                {
                    startBars.Add(i);
                }
            }
            else
            {
                startBars = M.StringToIntList(SystemStartBarsTextBox.Text, ',');
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
            }

            StringBuilder sb = new StringBuilder();
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
            if(SystemStartBarsTextBox.BackColor != M.TextBoxErrorColor)
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
            M.SetToWhite(NumberOfVoicesPerStaffTextBox);
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
        private string NormalizedNumberOfVoicesPerStaffString(List<List<int>> voiceListsPerStaff)
        {
            StringBuilder sb = new StringBuilder();
            if(voiceListsPerStaff.Count > 0)
            {
                foreach(List<int> staffVoices in voiceListsPerStaff)
                {
                    sb.Append(staffVoices.Count);
                    sb.Append(", ");
                }
                sb.Length = sb.Length - 2;
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
            KrystalsBrowser krystalBrowser = new KrystalsBrowser("All krystals", selectedKrystal, SetKrystal);
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
                string staffKrystalPath = M.MoritzKrystalsFolder + @"\" + newKrystal.Name;
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
            if(this.KrystalsListBox.SelectedItem is Krystal krystal)
            {
                _krystalBrowser = new KrystalsBrowser();
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
                    if(o is Krystal krystal)
                    {
                        allKrystals.Add(krystal);
                    }
                }
                return allKrystals;
            }
        }
        #endregion krystals groupBox

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

                    Debug.Assert(r.Name == "notation" || r.Name == "krystals");

                    while(r.Name == "notation" || r.Name == "krystals")
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
                            }
                        }
                        M.ReadToXmlElementTag(r, "notation", "krystals", "moritzKrystalScore");
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
            OutputChordSymbolTypeComboBox.SelectedIndex = 0; // default is standard outputChord symbols (all noteheads black)
            for(int i = 0; i < count; i++)
            {
                r.MoveToAttribute(i);
                switch(r.Name)
                {
                    case "outputChordSymbolType":
                        if(string.Compare(r.Value, "coloredVelocities") == 0)
                        {
                            OutputChordSymbolTypeComboBox.SelectedIndex = 1;
                        }
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
                    case "numberOfVoicesPerStaff":
                        NumberOfVoicesPerStaffTextBox.Text = r.Value;
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
            SetGroupBoxIsSaved(NotationGroupBox, ConfirmNotationButton, RevertNotationButton, (SavedState)KrystalsGroupBox.Tag);
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
                (SavedState)NotationGroupBox.Tag);
        }
        private Krystal GetKrystal(string krystalFileName)
        {
            Krystal krystal = null;
            try
            {
                string krystalPath = M.MoritzKrystalsFolder + @"\" + krystalFileName;
                krystal = K.LoadKrystal(krystalPath);
            }
            catch(Exception ex)
            {
                MessageBox.Show("Error loading krystal.\n\n" + ex.Message);
                krystal = null;
            }
            return krystal;
        }
        #endregion load settings

        #region save settings
        public void SaveSettings()
        {
            Debug.Assert(!string.IsNullOrEmpty(_settingsPath));

            M.CreateDirectoryIfItDoesNotExist(this._settingsFolderPath);

            #region do the save
            XmlWriterSettings settings = new XmlWriterSettings
            {
                Indent = true,
                IndentChars = ("\t"),
                NewLineOnAttributes = true,
                CloseOutput = false
            };
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
                w.WriteEndElement(); // closes the moritzKrystalScore element
                                     // the XmlWriter is closed automatically at the end of this using clause.
            }
            #endregion do the save
        }
        private void WriteNotation(XmlWriter w)
        {
            w.WriteStartElement("notation");

            if(OutputChordSymbolTypeComboBox.SelectedIndex > 0)
            {
                w.WriteAttributeString("outputChordSymbolType", "coloredVelocities");
            }
            w.WriteAttributeString("minimumCrotchetDuration", MinimumCrotchetDurationTextBox.Text);
            if(BeamsCrossBarlinesCheckBox.Checked)
                w.WriteAttributeString("beamsCrossBarlines", "true");
            else
                w.WriteAttributeString("beamsCrossBarlines", "false");
            w.WriteAttributeString("stafflineStemStrokeWidth", StafflineStemStrokeWidthComboBox.Text);
            w.WriteAttributeString("gap", GapPixelsComboBox.Text);
            w.WriteAttributeString("minGapsBetweenStaves", this.MinimumGapsBetweenStavesTextBox.Text);
            w.WriteAttributeString("minGapsBetweenSystems", MinimumGapsBetweenSystemsTextBox.Text);
            w.WriteAttributeString("voiceIndicesPerStaff", NumberOfVoicesPerStaffTextBox.Text);
            w.WriteAttributeString("clefsPerStaff", ClefsPerStaffTextBox.Text);
            w.WriteAttributeString("stafflinesPerStaff", StafflinesPerStaffTextBox.Text);
            w.WriteAttributeString("staffGroups", StaffGroupsTextBox.Text);
            w.WriteAttributeString("longStaffNames", LongStaffNamesTextBox.Text);
            w.WriteAttributeString("shortStaffNames", ShortStaffNamesTextBox.Text);
            w.WriteAttributeString("systemStartBars", SystemStartBarsTextBox.Text);

            w.WriteEndElement(); // notation

            SetGroupBoxIsSaved(NotationGroupBox, ConfirmNotationButton, RevertNotationButton, (SavedState)KrystalsGroupBox.Tag);

        }
        private void WriteKrystals(XmlWriter w)
        {
            if(this.KrystalsListBox.Items.Count > 0)
            {
                w.WriteStartElement("krystals");

                foreach(object o in KrystalsListBox.Items)
                {
                    if(o is Krystal krystal)
                    {
                        w.WriteStartElement("krystal");
                        w.WriteAttributeString("name", krystal.Name);
                        w.WriteEndElement();// krystal
                    }
                }
                w.WriteEndElement(); // krystals
            }
            SetGroupBoxIsSaved(KrystalsGroupBox, ConfirmKrystalsListButton, RevertKrystalsListButton, (SavedState)NotationGroupBox.Tag);
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
            List<Krystal> krystals = GetKrystalsList();
            PageFormat pageFormat = GetPageFormat();

            // These need clearing between creating different scores in one Moritz run.
            Metrics.ClearUsedCSSClasses();
            ClefMetrics.ClearUsedClefIDsList();
            FlagsMetrics.ClearUsedFlagIDsList();

            ComposableScore score =
                new KrystalScore(_scoreTitle,
                                        _algorithm,
                                        pageFormat,
                                        krystals,
                                        _settingsFolderPath,
                                        _dimensionsAndMetadataForm.Keywords,
                                        _dimensionsAndMetadataForm.Comment);

            if(score != null && score.Systems.Count > 0)
            {
                score.SaveMultiPageScore();
                score.SaveSingleSVGScore();
                // Opens the multi-page score in the program which is set by the system to open .svg files.
                global::System.Diagnostics.Process.Start(score.FilePath);
            }
        }

        private List<Krystal> GetKrystalsList()
        {
            var krystalsList = new List<Krystal>();
            foreach(Krystal krystal in KrystalsListBox.Items)
            {
                krystalsList.Add(krystal);
            }
            return krystalsList;
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
            pageFormat.Page1Title = damf.Page1Title;
            pageFormat.Page1Author = damf.Page1Author;
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
            switch(OutputChordSymbolTypeComboBox.SelectedIndex)
            {
                case 0:
                    pageFormat.ChordSymbolType = "standard";
                    break;
                case 1:
                    pageFormat.ChordSymbolType = "coloredVelocities";
                    break;
            }

            pageFormat.MinimumCrotchetDuration = int.Parse(this.MinimumCrotchetDurationTextBox.Text);
            pageFormat.BeamsCrossBarlines = this.BeamsCrossBarlinesCheckBox.Checked;

            float strokeWidth = float.Parse(StafflineStemStrokeWidthComboBox.Text, M.En_USNumberFormat) * pageFormat.ViewBoxMagnification;
            pageFormat.StafflineStemStrokeWidth = strokeWidth;
            pageFormat.Gap = float.Parse(GapPixelsComboBox.Text, M.En_USNumberFormat) * pageFormat.ViewBoxMagnification;
            pageFormat.DefaultDistanceBetweenStaves = int.Parse(MinimumGapsBetweenStavesTextBox.Text) * pageFormat.Gap;
            pageFormat.DefaultDistanceBetweenSystems = int.Parse(MinimumGapsBetweenSystemsTextBox.Text) * pageFormat.Gap;

            pageFormat.VoiceIndicesPerStaff = _voiceIndicesPerStaff; // one list of (max 2) midi channel indices per staff
            pageFormat.ClefPerStaff = M.StringToStringList(this.ClefsPerStaffTextBox.Text, ',');
            pageFormat.InitialClefPerMIDIChannel = GetClefPerVoice(pageFormat);

            pageFormat.StafflinesPerStaff = M.StringToIntList(this.StafflinesPerStaffTextBox.Text, ',');
            pageFormat.StaffGroups = M.StringToIntList(this.StaffGroupsTextBox.Text, ',');

            pageFormat.LongStaffNames = M.StringToStringList(this.LongStaffNamesTextBox.Text, ',');
            pageFormat.ShortStaffNames = M.StringToStringList(this.ShortStaffNamesTextBox.Text, ',');
            pageFormat.SystemStartBars = M.StringToIntList(SystemStartBarsTextBox.Text, ',');
        }

        /// <summary>
        /// Returns a clef for each ChannelDef (=MIDI Channel) in the system.
        /// The pageFormat.ClefsList has one clef per staff. Each staff can have either one or two ChannelDefs. 
        /// </summary>
        public List<string> GetClefPerVoice(PageFormat pageFormat)
        {
            List<string> clefPerVoice = new List<string>();
            int staffIndex = 0;
            for(; staffIndex < pageFormat.VoiceIndicesPerStaff.Count; staffIndex++)
            {
                string clef = pageFormat.ClefPerStaff[staffIndex];
                clefPerVoice.Add(clef);
                if(pageFormat.VoiceIndicesPerStaff[staffIndex].Count > 1)
                {
                    Debug.Assert(pageFormat.VoiceIndicesPerStaff[index: staffIndex].Count == 2);
                    clefPerVoice.Add(clef);
                }
            }

            return clefPerVoice;
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
        private KrystalsBrowser _krystalBrowser = null;
        private DimensionsAndMetadataForm _dimensionsAndMetadataForm;
        private FormStateFunctions _fsf;
        private List<TextBox> _allTextBoxes = null;
        #endregion editing
        #region score creation
        CompositionAlgorithm _algorithm = null;
        private List<int> _outputMIDIChannels = null;
        private List<List<int>> _voiceIndicesPerStaff = null; // always set in NumberOfVoicesPerStaffTextBox_Leave (cannot be empty)
        private int _numberOfStaves
        {
            get
            {
                Debug.Assert(_voiceIndicesPerStaff != null && _voiceIndicesPerStaff.Count > 0);
                var nStaves = _voiceIndicesPerStaff.Count;
                return nStaves;
            }
        }
        public PageFormat PageFormat = null;
        #endregion score creation

        #endregion private variables

    }
}