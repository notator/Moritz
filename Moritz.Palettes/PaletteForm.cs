using System;
using System.Windows.Forms;
using System.Diagnostics;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Xml;

using Krystals4ObjectLibrary;
using Moritz.Globals;

namespace Moritz.Palettes
{
    public partial class PaletteForm : Form, IPaletteForm
    {
        public PaletteForm(XmlReader r, string name, int domain, ComposerFormCallbacks mainFormCallbacks)
            : this(name, domain, mainFormCallbacks)
        {
            ReadPalette(r);
            if(_paletteButtonsControl != null)
            {
                _paletteButtonsControl.Enabled = true;
            }
            DeselectAll();
        }
        /// <summary>
        /// Creates a new, empty PalettesForm with help texts adjusted for the given domain.
        /// </summary>
        /// <param name="assistantComposer"></param>
        /// <param name="krystal"></param>
        public PaletteForm(string name, int domain, ComposerFormCallbacks mainFormCallbacks)
        {
            InitializeComponent();
            _name = name;
            _domain = domain;
            _callbacks = mainFormCallbacks;

            Text = name;

            ConnectBasicChordControl();
            if(M.Preferences.CurrentMultimediaMidiOutputDevice != null)
            {
                ConnectPaletteButtonsControl(domain);
                _paletteButtonsControl.Enabled = false;
            }

            _allMainTextBoxes = GetAllMainTextBoxes();
            _allChordParameterTextBoxes = GetAllChordParameterTextBoxes();

            SetDialogForDomain(domain);
        }

        public void ShowPaletteChordForm(int midiChordIndex)
        {
            PaletteChordForm f = new PaletteChordForm(this, _bcc, midiChordIndex, SetDialogState);
            f.Show();
        }


        private void ConnectBasicChordControl()
        {
            _bcc = new BasicChordControl(SetDialogState);
            _bcc.Location = new Point(22, 14);
            Controls.Add(_bcc);
        }
        private void ConnectPaletteButtonsControl(int domain)
        {
            Point location = new Point(this.MinMsDurationsTextBox.Location.X, this.MinMsDurationsTextBox.Location.Y + 27);
            _paletteButtonsControl = new PaletteButtonsControl(domain, location, this, _callbacks.SettingsFolderPath() + @"\audio");
            Debug.Assert(_paletteButtonsControl != null);
            Controls.Add(_paletteButtonsControl);
        }

        public static string NewPaletteName(int paletteNumber, int domain)
        {
            return "palette " + paletteNumber.ToString() + "  [domain " + domain.ToString() + " ]";
        }

        /// <summary>
        /// Can be called by OrnamentSettingsForm when clearing the ornament settings.
        /// </summary>
        public void NewOrnamentSettingsForm()
        {
            if(_ornamentSettingsForm != null)
            {
                _ornamentSettingsForm.Close();
                _ornamentSettingsForm = null;
            }

            _ornamentSettingsForm = new OrnamentSettingsForm(this);

            _ornamentSettingsForm.Show();

            SetOrnamentControls();
        }

        public void BringOrnamentSettingsFormToFront()
        {
            if(_ornamentSettingsForm != null)
            {
                _ornamentSettingsForm.BringToFront();
            }
        }

        /// <summary>
        /// Sets the '*' in Text, enables the SaveButton and informs _assistantComposerMainForm
        /// </summary>
        public void SetSettingsNotSaved()
        {
            if(!this.Text.EndsWith("*"))
            {
                this.Text = this.Text + "*";
                if(this._callbacks != null)
                {
                    _callbacks.SetSettingsHaveNotBeenSaved();
                    this.SaveSettingsButton.Enabled = true;
                }
            }
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

        public void SetOrnamentControls()
        {
            if(this.OrnamentSettingsForm != null)
            {
                _numberOfOrnaments = this.OrnamentSettingsForm.NumberOfOrnaments;

                OrnamentNumbersLabel.Enabled = true;
                OrnamentNumbersTextBox.Enabled = true;
                OrnamentNumbersHelpLabel.Enabled = true;
                MinMsDurationsLabel.Enabled = true;
                MinMsDurationsTextBox.Enabled = true;
                MinMsDurationsHelpLabel.Enabled = true;

                string ornamentNumbersHelpString = String.Format("{0} integer values in range [0..{1}]\n(0 means no ornament)",
                    _domain.ToString(), _numberOfOrnaments.ToString());
                OrnamentNumbersHelpLabel.Text = ornamentNumbersHelpString;

                string minMsDurationsHelpString = String.Format("{0} integer values >= 0",
                        _domain.ToString());
                MinMsDurationsHelpLabel.Text = minMsDurationsHelpString;

                ShowOrnamentSettingsButton.Enabled = true;
                DeleteOrnamentSettingsButton.Enabled = true;
            }
            else
            {
                OrnamentNumbersLabel.Enabled = false;
                OrnamentNumbersTextBox.Enabled = false;
                OrnamentNumbersHelpLabel.Enabled = false;
                MinMsDurationsLabel.Enabled = false;
                MinMsDurationsTextBox.Enabled = false;
                MinMsDurationsHelpLabel.Enabled = false;

                OrnamentNumbersHelpLabel.Text = "";
                MinMsDurationsHelpLabel.Text = "";

                ShowOrnamentSettingsButton.Enabled = false;
                DeleteOrnamentSettingsButton.Enabled = false;
            }

            OrnamentNumbersTextBox_Leave(OrnamentNumbersTextBox, null);
            MinMsDurationsTextBox_Leave(MinMsDurationsTextBox, null);
        }

        protected void PaletteForm_Click(object sender, EventArgs e)
        {
            if(_paletteButtonsControl != null)
            {
                _paletteButtonsControl.StopCurrentMediaPlayer();
            }
        }

        #region buttons
        /// <summary>
        /// Used to populate the Inversions lists
        /// </summary>
        class IntervalPositionDistance
        {
            public IntervalPositionDistance(byte value, int position)
            {
                Value = value;
                Position = (float)position;
            }
            public readonly byte Value;
            public readonly float Position;
            public float Distance = 0;
        }

        private int Compare(IntervalPositionDistance ipd1, IntervalPositionDistance ipd2)
        {
            int rval = 0;
            if(ipd1.Distance > ipd2.Distance)
                rval = 1;
            else if(ipd1.Distance == ipd2.Distance)
                rval = 0;
            else if(ipd1.Distance < ipd2.Distance)
                rval = -1;
            return rval;
        }

        /// <summary>
        /// Returns a list of intLists whose first intList is inversion0.
        /// If inversion0 is null or inversion0.Count == 0, the returned list of intlists is empty, otherwise
        /// If inversion0.Count == 1, the contained intList is simply inversion0, otherwise
        /// The returned list of intLists has a Count of (n-1)*2, where n is the Count of inversion0.
        /// </summary>
        /// <param name="inversion0"></param>
        /// <returns></returns>
        public List<List<byte>> GetLinearInversions(string inversion0String)
        {
            List<byte> inversion0 = M.StringToByteList(inversion0String, ',');
            List<List<byte>> inversions = new List<List<byte>>();
            if(inversion0 != null && inversion0.Count != 0)
            {
                if(inversion0.Count == 1)
                    inversions.Add(inversion0);
                else
                {

                    List<IntervalPositionDistance> ipdList = new List<IntervalPositionDistance>();
                    for(int i = 0; i < inversion0.Count; i++)
                    {
                        IntervalPositionDistance ipd = new IntervalPositionDistance(inversion0[i], i);
                        ipdList.Add(ipd);
                    }
                    // ipdList is a now representaion of the field, now calculate the interval hierarchy per inversion
                    for(float pos = 0.25F; pos < (float)inversion0.Count - 1; pos += 0.5F)
                    {
                        List<IntervalPositionDistance> newIpdList = new List<IntervalPositionDistance>(ipdList);
                        foreach(IntervalPositionDistance ipd in newIpdList)
                        {
                            ipd.Distance = ipd.Position - pos;
                            ipd.Distance = ipd.Distance > 0 ? ipd.Distance : ipd.Distance * -1;
                        }
                        newIpdList.Sort(Compare);
                        List<byte> intervalList = new List<byte>();
                        foreach(IntervalPositionDistance ipd in newIpdList)
                            intervalList.Add(ipd.Value);
                        inversions.Add(intervalList);
                    }
                    // the intervalList for a particular inversionIndex is now inversions[inversionIndex]
                }
            }
            return inversions;
        }

        /// <summary>
        /// called after loading a file
        /// </summary>
        private void TouchAllTextBoxes()
        {
            _bcc.TouchAllTextBoxes();
            BankIndicesTextBox_Leave(BankIndicesTextBox, null);
            PatchIndicesTextBox_Leave(PatchIndicesTextBox, null);
            RepeatsTextBox_Leave(RepeatsTextBox, null);
            PitchwheelDeviationsTextBox_Leave(PitchwheelDeviationsTextBox, null);
            PitchwheelEnvelopesTextBox_Leave(PitchwheelEnvelopesTextBox, null);
            PanEnvelopesTextBox_Leave(PanEnvelopesTextBox, null);
            ModulationWheelEnvelopesTextBox_Leave(ModulationWheelEnvelopesTextBox, null);
            ExpressionEnvelopesTextBox_Leave(ExpressionEnvelopesTextBox, null);
            if(this.OrnamentSettingsForm != null)
                OrnamentNumbersTextBox_Leave(OrnamentNumbersTextBox, null);
        }

        private void SaveSettingsButton_Click(object sender, EventArgs e)
        {
            _callbacks.SaveSettings();
            DeselectAll();
        }

        private void DeselectAll()
        {
            bool settingsHaveBeenSaved = Text[Text.Length - 1] != '*';
            this.ModulationWheelEnvelopesLabel.Focus();
            if(settingsHaveBeenSaved)
                SetSettingsHaveBeenSaved();
            else
                SetSettingsNotSaved();
        }

        private void ShowMainScoreFormButton_Click(object sender, EventArgs e)
        {
            this._callbacks.MainFormBringToFront();
        }
        public void ShowOrnamentSettingsButton_Click(object sender, EventArgs e)
        {
            if(_ornamentSettingsForm != null)
            {
                _ornamentSettingsForm.Show();
                _ornamentSettingsForm.BringToFront();
            }
        }
        private void NewOrnamentSettingsButton_Click(object sender, EventArgs e)
        {
            NewOrnamentSettingsForm();
        }

        private void DeleteOrnamentSettingsButton_Click(object sender, EventArgs e)
        {
            string msg = "Do you really want to delete the ornament settings?";
            DialogResult result = MessageBox.Show(msg, "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
            if(result == DialogResult.No)
                return;

            if(_ornamentSettingsForm != null)
            {
                _ornamentSettingsForm.Close();
                _ornamentSettingsForm = null;

                ShowOrnamentSettingsButton.Enabled = false;
                DeleteOrnamentSettingsButton.Enabled = false;

                OrnamentNumbersTextBox.Text = "";
                OrnamentNumbersHelpLabel.Text = "";
                OrnamentNumbersLabel.Enabled = false;
                OrnamentNumbersTextBox.Enabled = false;
                OrnamentNumbersHelpLabel.Enabled = false;

                MinMsDurationsTextBox.Text = "";
                MinMsDurationsHelpLabel.Text = "";
                MinMsDurationsLabel.Enabled = false;
                MinMsDurationsTextBox.Enabled = false;
                MinMsDurationsHelpLabel.Enabled = false;

                SetSettingsNotSaved();
            }
        }

        public void MidiPitchesHelpButton_Click(object sender, EventArgs e)
        {
            if(_midiPitchesHelpForm == null)
            {
                _midiPitchesHelpForm = new MidiPitchesHelpForm(CloseMidiPitchesHelpForm);
                _midiPitchesHelpForm.Show();
            }
            _midiPitchesHelpForm.BringToFront();
        }

        private void CloseMidiPitchesHelpForm()
        {
            if(_midiPitchesHelpForm != null)
            {
                _midiPitchesHelpForm.Close();
                _midiPitchesHelpForm.Dispose();
                _midiPitchesHelpForm = null;
            }
        }

        public void MidiInstrumentsHelpButton_Click(object sender, EventArgs e)
        {
            if(_midiInstrumentsHelpForm == null)
            {
                _midiInstrumentsHelpForm = new MIDIInstrumentsHelpForm(CloseMIDIInstrumentsHelpForm);
                _midiInstrumentsHelpForm.Show();
            }
            _midiInstrumentsHelpForm.BringToFront();
        }

        private void CloseMIDIInstrumentsHelpForm()
        {
            if(_midiInstrumentsHelpForm != null)
            {
                _midiInstrumentsHelpForm.Close();
                _midiInstrumentsHelpForm.Dispose();
                _midiInstrumentsHelpForm = null;
            }
        }

        /// <summary>
        /// This function enables the parameter inputs and sets this dialog's help texts accordingly.
        /// The contents of the Parameter TextBoxes are not changed.
        /// </summary>
        protected void SetDialogForDomain(int domain)
        {
            _bcc.NumberOfChordValues = domain;

            string countString = domain.ToString() + " ";
            string integerString = "integer ";
            string floatString = "float ";
            string valuesInRangeString = "values in range ";
            string floatsPercentageString = countString + floatString + valuesInRangeString + "[ 0.0..100.0 ]";
            string envelopesHelpString = countString + "envelopes*";

            EnableMainParameters();
            DisableChordParameters();

            #region HelpLabels
            _bcc.SetHelpLabels();
            BankIndicesHelpLabel.Text = countString + "integer values in range [ 0..127 ]";
            PatchIndicesHelpLabel.Text = countString + "integer values in range [ 0..127 ]";
            RepeatsHelpLabel.Text = countString + "boolean values ( 1=true, 0=false )";
            PitchwheelDeviationsHelpLabel.Text = countString + integerString + valuesInRangeString + "[ 0..127 ]";
            PitchwheelEnvelopesHelpLabel.Text = envelopesHelpString;
            PanEnvelopesHelpLabel.Text = envelopesHelpString;
            ModulationWheelEnvelopesHelpLabel.Text = envelopesHelpString;
            VelocityEnvelopesHelpLabel.Text = envelopesHelpString;
            #endregion HelpLabels
        }

        private void DisableChordParameters()
        {
            _bcc.RootInversionLabel.Enabled = false;
            _bcc.RootInversionTextBox.Enabled = false;
            _bcc.RootInversionTextBox.Text = "";
            _bcc.InversionIndicesLabel.Enabled = false;
            _bcc.InversionIndicesTextBox.Enabled = false;
            _bcc.InversionIndicesTextBox.Text = "";
            _bcc.VerticalVelocityFactorsLabel.Enabled = false;
            _bcc.VerticalVelocityFactorsTextBox.Enabled = false;
            _bcc.VerticalVelocityFactorsTextBox.Text = "";
        }

        private List<TextBox> GetAllMainTextBoxes()
        {
            List<TextBox> mainTextBoxes = new List<TextBox>();
            mainTextBoxes.Add(_bcc.DurationsTextBox);
            mainTextBoxes.Add(_bcc.MidiPitchesTextBox);
            mainTextBoxes.Add(_bcc.VelocitiesTextBox);
            mainTextBoxes.Add(_bcc.ChordOffsTextBox);
            mainTextBoxes.Add(_bcc.ChordDensitiesTextBox);
            mainTextBoxes.Add(BankIndicesTextBox);
            mainTextBoxes.Add(PatchIndicesTextBox);
            mainTextBoxes.Add(RepeatsTextBox);
            mainTextBoxes.Add(PitchwheelDeviationsTextBox);
            mainTextBoxes.Add(ModulationWheelEnvelopesTextBox);
            mainTextBoxes.Add(PitchwheelEnvelopesTextBox);
            mainTextBoxes.Add(PanEnvelopesTextBox);
            mainTextBoxes.Add(ExpressionEnvelopesTextBox);
            return mainTextBoxes;
        }


        private List<TextBox> GetAllChordParameterTextBoxes()
        {
            List<TextBox> chordParameterTextBoxes = new List<TextBox>();
            chordParameterTextBoxes.Add(_bcc.RootInversionTextBox);
            chordParameterTextBoxes.Add(_bcc.VerticalVelocityFactorsTextBox);
            chordParameterTextBoxes.Add(_bcc.InversionIndicesTextBox);
            return chordParameterTextBoxes;
        }

        private void EnableMainParameters()
        {
            _bcc.DurationsLabel.Enabled = true;
            _bcc.DurationsTextBox.Enabled = true;
            _bcc.DurationsHelpLabel.Enabled = true;

            _bcc.MidiPitchesLabel.Enabled = true;
            _bcc.MidiPitchesTextBox.Enabled = true;
            _bcc.MidiPitchesHelpLabel.Enabled = true;

            _bcc.VelocitiesLabel.Enabled = true;
            _bcc.VelocitiesTextBox.Enabled = true;
            _bcc.VelocitiesHelpLabel.Enabled = true;

            _bcc.ChordDensitiesLabel.Enabled = true;
            _bcc.ChordDensitiesTextBox.Enabled = true;
            _bcc.ChordDensitiesHelpLabel.Enabled = true;

            BankIndicesLabel.Enabled = true;
            BankIndicesLabel.Enabled = true;
            BankIndicesLabel.Enabled = true;

            PatchIndicesLabel.Enabled = true;
            PatchIndicesTextBox.Enabled = true;
            PatchIndicesHelpLabel.Enabled = true;

            ModulationWheelEnvelopesLabel.Enabled = true;
            ModulationWheelEnvelopesTextBox.Enabled = true;
            ModulationWheelEnvelopesHelpLabel.Enabled = true;

            RepeatsLabel.Enabled = true;
            RepeatsTextBox.Enabled = true;
            RepeatsHelpLabel.Enabled = true;

            PitchwheelDeviationsLabel.Enabled = true;
            PitchwheelDeviationsTextBox.Enabled = true;
            PitchwheelDeviationsHelpLabel.Enabled = true;

            PitchwheelEnvelopesLabel.Enabled = true;
            PitchwheelEnvelopesTextBox.Enabled = true;
            PitchwheelEnvelopesHelpLabel.Enabled = true;

            PanEnvelopesLabel.Enabled = true;
            PanEnvelopesTextBox.Enabled = true;
            PanEnvelopesHelpLabel.Enabled = true;

            VelocityEnvelopesLabel.Enabled = true;
            ExpressionEnvelopesTextBox.Enabled = true;
            VelocityEnvelopesHelpLabel.Enabled = true;
        }

        private string DefaultRootInversionString(int count)
        {
            StringBuilder sb = new StringBuilder();
            for(int i = 0; i < count; i++)
            {
                switch(i)
                {
                    case 0:
                        sb.Append("12,");
                        break;
                    case 1:
                        sb.Append("7,");
                        break;
                    case 2:
                        sb.Append("5,");
                        break;
                    case 3:
                        sb.Append("4,");
                        break;
                    case 4:
                        sb.Append("3,");
                        break;
                    case 5:
                        sb.Append("2,");
                        break;
                    default:
                        sb.Append("1,");
                        break;
                }
            }
            sb.Remove(sb.Length - 1, 1); // final comma
            return sb.ToString();
        }

        private List<int> DefaultRootInversionIntList(int count)
        {
            List<int> intList = new List<int>();
            for(int i = 0; i < count; i++)
            {
                switch(i)
                {
                    case 0:
                        intList.Add(12);
                        break;
                    case 1:
                        intList.Add(7);
                        break;
                    case 2:
                        intList.Add(5);
                        break;
                    case 3:
                        intList.Add(4);
                        break;
                    case 4:
                        intList.Add(3);
                        break;
                    case 5:
                        intList.Add(2);
                        break;
                    default:
                        intList.Add(1);
                        break;
                }
            }
            return intList;
        }

        #endregion buttons

        #region text box events
        #region text changed event handlers

        private void RepeatsTextBox_TextChanged(object sender, EventArgs e)
        {
            M.SetToWhite(sender as TextBox);
        }

        private void PitchwheelDeviationsTextBox_TextChanged(object sender, EventArgs e)
        {
            M.SetToWhite(sender as TextBox);
        }

        private void PitchwheelEnvelopesTextBox_TextChanged(object sender, EventArgs e)
        {
            M.SetToWhite(sender as TextBox);
        }

        private void PanEnvelopesTextBox_TextChanged(object sender, EventArgs e)
        {
            M.SetToWhite(sender as TextBox);
        }

        private void ModulationWheelEnvelopesTextBox_TextChanged(object sender, EventArgs e)
        {
            M.SetToWhite(sender as TextBox);
        }

        private void ExpressionEnvelopesTextBox_TextChanged(object sender, EventArgs e)
        {
            M.SetToWhite(sender as TextBox);
        }

        private void OrnamentNumbersTextBox_TextChanged(object sender, EventArgs e)
        {
            M.SetToWhite(sender as TextBox);
        }

        private void MinMsDurationsTextBox_TextChanged(object sender, EventArgs e)
        {
            M.SetToWhite(sender as TextBox);
        }

        #endregion text changed event handlers

        private void BankIndicesTextBox_Leave(object sender, EventArgs e)
        {
            M.LeaveIntRangeTextBox(sender as TextBox, true, (uint)_bcc.NumberOfChordValues, 0, 127, SetDialogState);
        }

        private void PatchIndicesTextBox_Leave(object sender, EventArgs e)
        {
            M.LeaveIntRangeTextBox(sender as TextBox, false, (uint)_bcc.NumberOfChordValues, 0, 127, SetDialogState);
        }

        private void RepeatsTextBox_Leave(object sender, EventArgs e)
        {
            M.LeaveIntRangeTextBox(sender as TextBox, true, (uint)_bcc.NumberOfChordValues, 0, 1, SetDialogState);
        }

        private void PitchwheelDeviationsTextBox_Leave(object sender, EventArgs e)
        {
            M.LeaveIntRangeTextBox(sender as TextBox, true, (uint)_bcc.NumberOfChordValues, 0, 127, SetDialogState);
        }

        private void PitchwheelEnvelopesTextBox_Leave(object sender, EventArgs e)
        {
            GetEnvelopes(PitchwheelEnvelopesTextBox);
            if(PitchwheelEnvelopesTextBox.Text.Length == 0)
                PitchwheelDeviationsTextBox.Text = "";
        }

        private void PanEnvelopesTextBox_Leave(object sender, EventArgs e)
        {
            GetEnvelopes(PanEnvelopesTextBox);
        }

        private void ModulationWheelEnvelopesTextBox_Leave(object sender, EventArgs e)
        {
            GetEnvelopes(ModulationWheelEnvelopesTextBox);
        }

        private void ExpressionEnvelopesTextBox_Leave(object sender, EventArgs e)
        {
            GetEnvelopes(ExpressionEnvelopesTextBox);
        }

        private void OrnamentNumbersTextBox_Leave(object sender, EventArgs e)
        {
            bool optional = (OrnamentSettingsForm == null);
            M.LeaveIntRangeTextBox(sender as TextBox, optional, (uint)_bcc.NumberOfChordValues, 0, int.MaxValue, SetDialogState);
        }

        private void MinMsDurationsTextBox_Leave(object sender, EventArgs e)
        {
            M.LeaveIntRangeTextBox(sender as TextBox, true, (uint)_bcc.NumberOfChordValues, 0, int.MaxValue, SetDialogState);
        }

        #region helper functions
        private void GetEnvelopes(TextBox textBox)
        {
            List<string> envelopes = GetEnvelopes(textBox.Text);
            if(textBox.Text.Length > 0 && envelopes == null) // error
            {
                SetDialogState(textBox, false);
            }
            else
            {
                textBox.Text = TextBoxString(envelopes);
                SetDialogState(textBox, true);
            }
        }
        /// <summary>
        /// Checks the input text for errors and converts it to a list of strings.
        /// If text.Length == 0, an empty list is returned.
        /// If there is an error in the input text, null is returned.
        /// Each string in the result contains integers (in range [0..100]) separated by the ':' character.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private List<string> GetEnvelopes(string text)
        {
            List<string> returnList = new List<string>();
            if(text.Length > 0)
            {
                StringBuilder envelopeSB = new StringBuilder();
                if(text[text.Length - 1] == ',')
                    text = text.Remove(text.Length - 1);

                string[] envelopes = text.Split(',');
                if(envelopes.Length == _domain)
                {
                    foreach(string envelope in envelopes)
                    {
                        envelopeSB.Remove(0, envelopeSB.Length);
                        List<string> numbers = GetCheckedIntStrings(envelope, -1, 0, 127, ':');
                        if(numbers != null)
                        {
                            foreach(string numberStr in numbers)
                            {
                                envelopeSB.Append(":");
                                envelopeSB.Append(numberStr);
                            }
                            envelopeSB.Remove(0, 1);
                            returnList.Add(envelopeSB.ToString());
                        }
                        else
                        {
                            returnList = null;
                            break;
                        }
                    }
                }
                else
                {
                    returnList = null;
                }
            }
            return returnList;
        }

        private string TextBoxString(List<string> envelopes)
        {
            StringBuilder textBoxSB = new StringBuilder();
            if(envelopes.Count > 0)
            {
                foreach(string envelope in envelopes)
                {
                    textBoxSB.Append(",  ");
                    textBoxSB.Append(envelope);
                }
                textBoxSB.Remove(0, 3);
            }
            return textBoxSB.ToString();
        }

        /// <summary>
        /// Returns null if textBox.Text is empty, or the contained values are outside the given range.
        /// </summary>
        private List<string> GetCheckedFloatStrings(string text, float minVal, float maxVal)
        {
            List<string> strings = new List<string>();
            bool okay = true;
            if(text.Length > 0)
            {
                try
                {
                    List<float> floats = M.StringToFloatList(text, ',');
                    okay = CheckFloatList(floats, (int)_domain, minVal, maxVal);
                    if(okay)
                    {
                        foreach(float f in floats)
                            strings.Add(f.ToString(M.En_USNumberFormat));
                    }
                }
                catch
                {
                    okay = false;
                }
            }

            if(strings.Count > 0)
                return strings;
            else
                return null;
        }

        /// <summary>
        /// Returns null if textBox.Text is empty, or there are not count values separated by the separator
        /// character, or the values are outside the given range.
        /// To ignore the count parameter, pass count = -1.
        /// Also used by the ornaments palette.
        /// </summary>
        public List<string> GetCheckedIntStrings(string text, int count, int minVal, int maxVal, char separator)
        {
            List<string> strings = new List<string>();
            
            if(text.Length > 0)
            {
                List<int> ints = null;
                try
                {
                    ints = M.StringToIntList(text, separator);
                }
                catch
                {
                }
                if(ints != null && CheckIntList(ints, count, minVal, maxVal))
                {
                    foreach(int i in ints)
                        strings.Add(i.ToString());
                }
            }

            if(strings.Count > 0)
                return strings;
            else return null;
        }

        /// <summary>
        /// Also used by paletteChordForm and ornaments dialogs
        /// </summary>
        /// <param name="textBox"></param>
        /// <param name="okay"></param>
        /// <param name="anyTextBoxHasErrorColor"></param>
        private void SetDialogState(TextBox textBox, bool okay)
        {
            if(okay)
            {
                textBox.BackColor = Color.White;
            }
            else
            {
                textBox.BackColor = M.TextBoxErrorColor;
            }

            if(_paletteButtonsControl != null)
            {
                if(HasError())
                    _paletteButtonsControl.Enabled = false;
                else
                    _paletteButtonsControl.Enabled = true;
            }
            
            this.SetSettingsNotSaved();
        }

        /// <summary>
        /// Used by ornaments dialog.
        /// </summary>
        /// <param name="enabled"></param>
        public void SetSaveButton(bool enabled)
        {
            _callbacks.SetSaveAndCreateButtons(enabled);
        }



        /// <summary>
        /// Returne false if
        ///     (count != -1 && intList.Count != count)
        ///     or any value is less than minVal, 
        ///     or any value is greater than maxval.
        /// Passing count=-1 to ignore the count parameter.
        /// </summary>
        private bool CheckIntList(List<int> intList, int count, int minVal, int maxVal)
        {
            bool OK = true;
            if(count != -1 && intList.Count != count)
                OK = false;
            else
            {
                foreach(int value in intList)
                {
                    if(value < minVal || value > maxVal)
                    {
                        OK = false;
                        break;
                    }
                }
            }
            return OK;
        }

        /// <summary>
        /// Returne false if
        ///     floatList.Count != count
        ///     or any value is less than minVal, 
        ///     or any value is greater than maxval.
        /// </summary>
        private bool CheckFloatList(List<float> floatList, int count, float minVal, float maxVal)
        {
            bool OK = true;
            if(floatList.Count != count)
                OK = false;
            else
            {
                foreach(float value in floatList)
                {
                    if(value < minVal || value > maxVal)
                    {
                        OK = false;
                        break;
                    }
                }
            }
            return OK;
        }
        #endregion helper functions
        #endregion text box events

        private void PaletteForm_Activated(object sender, EventArgs e)
        {
            this.DeselectAll();
            if(_callbacks.MainFormHasBeenSaved())
                _callbacks.SetSettingsHaveBeenSaved();
            else
                _callbacks.SetSettingsHaveNotBeenSaved();
        }

        /// <summary>
        /// This form is closed only by the AssistantComposer form
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PaletteForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if(_strandsBrowser != null)
            {
                _strandsBrowser.Close();
                _strandsBrowser = null;
            }

            if(_ornamentSettingsForm != null)
            {
                _ornamentSettingsForm.Close();
                _ornamentSettingsForm = null;
            }

            CloseMidiPitchesHelpForm();
            CloseMIDIInstrumentsHelpForm();
        }

        #region iPaletteForm
        public string GetName() { return this._name; }
        public void SetName(string name)
        {
            this._name = name;
            if(this.Text.EndsWith("*"))
                Text = name + "*";
            else
                Text = name;
        }
        public override string ToString()
        {
            return this._name;
        }
        public bool HasError()
        {
            bool anyTextBoxHasErrorColour = ErrorInMainTextBoxes();
            if(!anyTextBoxHasErrorColour)
            {
                foreach(TextBox textBox in _allChordParameterTextBoxes)
                {
                    if(textBox.Enabled && textBox.BackColor == M.TextBoxErrorColor)
                    {
                        anyTextBoxHasErrorColour = true;
                        break;
                    }
                }
            }
            //bool hasError = (anyTextBoxHasErrorColour || String.IsNullOrEmpty(_bcc.DurationsTextBox.Text)); 
            //return hasError;
            return anyTextBoxHasErrorColour;
        }
        private bool ErrorInMainTextBoxes()
        {
            bool anyTextBoxHasErrorColour = false;
            foreach(TextBox textBox in _allMainTextBoxes)
            {
                if(textBox.Enabled && textBox.BackColor == M.TextBoxErrorColor)
                {
                    anyTextBoxHasErrorColour = true;
                    break;
                }
            }
            return anyTextBoxHasErrorColour;
        }
        /// <summary>
        /// Removes the '*' in Text, disables the SaveButton and informs _ornamentSettingsForm
        /// </summary>
        public void SetSettingsHaveBeenSaved()
        {
            if(this.Text.EndsWith("*"))
            {
                this.Text = this.Text.Remove(this.Text.Length - 1);
                if(this._ornamentSettingsForm != null)
                {
                    _ornamentSettingsForm.SetSettingsHaveBeenSaved();
                }
            }
            this.SaveSettingsButton.Enabled = false;
            //DeselectAll();
        }
        /// <summary>
        /// Returns false if the file already exists and the user cancels the save.
        /// </summary>
        /// <returns></returns>
        public void WritePalette(XmlWriter w)
        {
            w.WriteStartElement("palette");
            w.WriteAttributeString("name", _name);
            w.WriteAttributeString("domain", _domain.ToString());

            _bcc.WriteBasicChordControl(w);

            if(!string.IsNullOrEmpty(this.BankIndicesTextBox.Text))
            {
                w.WriteStartElement("bankIndices");
                w.WriteString(BankIndicesTextBox.Text.Replace(" ", ""));
                w.WriteEndElement();
            }
            if(!string.IsNullOrEmpty(this.PatchIndicesTextBox.Text))
            {
                w.WriteStartElement("patchIndices");
                w.WriteString(PatchIndicesTextBox.Text.Replace(" ", ""));
                w.WriteEndElement();
            }
            if(!string.IsNullOrEmpty(RepeatsTextBox.Text))
            {
                w.WriteStartElement("repeats");
                w.WriteString(RepeatsTextBox.Text.Replace(" ", ""));
                w.WriteEndElement();
            }
            if(!string.IsNullOrEmpty(PitchwheelDeviationsTextBox.Text))
            {
                w.WriteStartElement("pitchwheelDeviations");
                w.WriteString(PitchwheelDeviationsTextBox.Text.Replace(" ", ""));
                w.WriteEndElement();
            }
            if(!string.IsNullOrEmpty(PitchwheelEnvelopesTextBox.Text))
            {
                w.WriteStartElement("pitchwheelEnvelopes");
                w.WriteString(PitchwheelEnvelopesTextBox.Text.Replace(" ", ""));
                w.WriteEndElement();
            }
            if(!string.IsNullOrEmpty(PanEnvelopesTextBox.Text))
            {
                w.WriteStartElement("panEnvelopes");
                w.WriteString(PanEnvelopesTextBox.Text.Replace(" ", ""));
                w.WriteEndElement();
            }
            if(!string.IsNullOrEmpty(ModulationWheelEnvelopesTextBox.Text))
            {
                w.WriteStartElement("modulationWheelEnvelopes");
                w.WriteString(ModulationWheelEnvelopesTextBox.Text.Replace(" ", ""));
                w.WriteEndElement();
            }
            if(!string.IsNullOrEmpty(this.ExpressionEnvelopesTextBox.Text))
            {
                w.WriteStartElement("expressionEnvelopes");
                w.WriteString(this.ExpressionEnvelopesTextBox.Text.Replace(" ", ""));
                w.WriteEndElement();
            }

            if(_paletteButtonsControl != null)
            {
                _paletteButtonsControl.WriteAudioFiles(w);
            }

            if(!string.IsNullOrEmpty(OrnamentNumbersTextBox.Text))
            {
                w.WriteStartElement("ornamentNumbers");
                w.WriteString(OrnamentNumbersTextBox.Text.Replace(" ", ""));
                w.WriteEndElement();
            }

            if(!string.IsNullOrEmpty(this.MinMsDurationsTextBox.Text))
            {
                w.WriteStartElement("ornamentMinMsDurations");
                w.WriteString(MinMsDurationsTextBox.Text.Replace(" ", ""));
                w.WriteEndElement();
            }

            if(OrnamentSettingsForm != null)
                OrnamentSettingsForm.WriteOrnamentSettingsForm(w);

            w.WriteEndElement(); // closes the palette element

            SetSettingsHaveBeenSaved();
        }
        public void ReadPalette(XmlReader r)
        {
            Debug.Assert(r.Name == "name" || r.Name == "domain" || r.Name == "percussion"); // attributes
            Debug.Assert(_bcc != null);

            #region
            BankIndicesTextBox.Text = "";
            PatchIndicesTextBox.Text = "";
            RepeatsTextBox.Text = "";
            PitchwheelDeviationsTextBox.Text = "";
            PitchwheelEnvelopesTextBox.Text = "";
            PanEnvelopesTextBox.Text = "";
            ModulationWheelEnvelopesTextBox.Text = "";
            ExpressionEnvelopesTextBox.Text = "";
            OrnamentNumbersTextBox.Text = "";
            MinMsDurationsTextBox.Text = "";
            #endregion

            M.ReadToXmlElementTag(r, "basicChord");
            while(r.Name == "basicChord" || r.Name == "bankIndices" || r.Name == "patchIndices" 
                || r.Name == "repeats" || r.Name == "pitchwheelDeviations"
                || r.Name == "pitchwheelEnvelopes" || r.Name == "panEnvelopes"
                || r.Name == "modulationWheelEnvelopes" || r.Name == "expressionEnvelopes"
                || r.Name == "audioFiles"
                || r.Name == "ornamentNumbers" || r.Name == "ornamentMinMsDurations"
                || r.Name == "ornamentSettings")
            {
                if(r.NodeType != XmlNodeType.EndElement)
                {
                    switch(r.Name)
                    {
                        case "basicChord":
                            _bcc.ReadBasicChordControl(r);
                            break;
                        case "bankIndices":
                            BankIndicesTextBox.Text = r.ReadElementContentAsString();
                            break;
                        case "patchIndices":
                            PatchIndicesTextBox.Text = r.ReadElementContentAsString();
                            break;
                        case "repeats":
                            RepeatsTextBox.Text = r.ReadElementContentAsString();
                            break;
                        case "pitchwheelDeviations":
                            PitchwheelDeviationsTextBox.Text = r.ReadElementContentAsString();
                            break;
                        case "pitchwheelEnvelopes":
                            PitchwheelEnvelopesTextBox.Text = r.ReadElementContentAsString();
                            break;
                        case "panEnvelopes":
                            PanEnvelopesTextBox.Text = r.ReadElementContentAsString();
                            break;
                        case "modulationWheelEnvelopes":
                            ModulationWheelEnvelopesTextBox.Text = r.ReadElementContentAsString();
                            break;
                        case "expressionEnvelopes":
                            ExpressionEnvelopesTextBox.Text = r.ReadElementContentAsString();
                            break;
                        case "audioFiles":
                            if(_paletteButtonsControl != null)
                            {
                                _paletteButtonsControl.ReadAudioFiles(r);
                            }
                            break;
                        case "ornamentNumbers":
                            OrnamentNumbersTextBox.Text = r.ReadElementContentAsString();
                            break;
                        case "ornamentMinMsDurations":
                            this.MinMsDurationsTextBox.Text = r.ReadElementContentAsString();
                            break;
                        case "ornamentSettings":
                            _ornamentSettingsForm = new OrnamentSettingsForm(r, this);
                            ShowOrnamentSettingsButton.Enabled = true;
                            DeleteOrnamentSettingsButton.Enabled = true;
                            break;
                    }
                }
                M.ReadToXmlElementTag(r, "palette", "basicChord", "bankIndices", "patchIndices", "repeats",
                    "pitchwheelDeviations", "pitchwheelEnvelopes", "panEnvelopes", "modulationWheelEnvelopes",
                    "expressionEnvelopes", "audioFiles", "ornamentNumbers", "ornamentMinMsDurations", "ornamentSettings");
            }
            Debug.Assert(r.Name == "palette"); // end element
            SetOrnamentControls();
            TouchAllTextBoxes();
        }
        private string _name = null;
        private int _domain = 0;
        private ComposerFormCallbacks _callbacks = null;
        private List<TextBox> _allMainTextBoxes = new List<TextBox>();
        private PaletteButtonsControl _paletteButtonsControl = null;
        #endregion iPaletteForm

        #region public variables
        public int Domain { get { return _domain; } }
        public ComposerFormCallbacks Callbacks { get { return _callbacks; } }
        public BasicChordControl BasicChordControl { get { return _bcc; } }
        public OrnamentSettingsForm OrnamentSettingsForm { get { return _ornamentSettingsForm; } }
        public PaletteButtonsControl PaletteButtonsControl { get { return _paletteButtonsControl; } }
        #endregion public variables

        #region private variables
        private readonly List<TextBox> _allChordParameterTextBoxes = new List<TextBox>();
        private int _numberOfOrnaments;
        private Moritz.Krystals.StrandsBrowser _strandsBrowser = null;
        private OrnamentSettingsForm _ornamentSettingsForm = null;
        private MidiPitchesHelpForm _midiPitchesHelpForm = null;
        private MIDIInstrumentsHelpForm _midiInstrumentsHelpForm = null;
        private BasicChordControl _bcc = null;
        #endregion private variables

    }

    internal delegate void CloseMidiPitchesHelpFormDelegate();
    internal delegate void CloseMIDIHelpFormDelegate();
}