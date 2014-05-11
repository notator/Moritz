using System;
using System.Windows.Forms;
using System.Xml;
using System.Diagnostics;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

using Moritz.Globals;

namespace Moritz.AssistantComposer
{
    public partial class PerformerOptionsForm : Form
    {
        /// <summary>
        /// Creates a new, empty PerformerOptionsForm for setting the following options.
        /// These are all defaults, that will be adjustable (as before) in the AP:
        /// </summary>
        public PerformerOptionsForm(AssistantComposerMainForm assistantComposerMainForm, int nTracks)
        {
            InitializeComponent();

            _trackPanels = GetAllTrackPanels();
            _noteOnPitchCheckBoxes = GetNoteOnPitchCheckBoxes();
            _noteOnVelocityCheckBoxes = GetNoteOnVelocityCheckBoxes();
            _pressureCheckBoxes = GetPressureCheckBoxes();
            _pitchWheelCheckBoxes = GetPitchWheelCheckBoxes();
            _modWheelCheckBoxes = GetModWheelCheckBoxes();

            _assistantComposerMainForm = assistantComposerMainForm;

            #region set default values
            this._nTracks = nTracks;
            SetPerformersTrackNumberComboBoxItems();
            this.PerformersTrackNumberComboBox.Text = "1";

            this.NoteOnPitchTracksComboBox.Text = "none";
            this.NoteOnVelocityTracksComboBox.Text = "none";

            this.PressureControllerComboBox.Text = "aftertouch";
            this.PressureTracksComboBox.Text = "none";
            this.PitchWheelControllerComboBox.Text = "pitch wheel";
            this.PitchWheelTracksComboBox.Text = "none";
            this.ModWheelControllerComboBox.Text = "modulation (1)";
            this.ModWheelTracksComboBox.Text = "none";

            this.SpeedControllerComboBox.Text = "none";
            this.OptionalMaximumSpeedGroupBox.Enabled = false;
            this.MaximumSpeedPercentTextBox.Text = "400";

            this.OptionalMinimumVolumeGroupBox.Enabled = false;
            this.MinimumVolumeTextBox.Text = "50";
            #endregion

            DisableUnusedTrackPanels(_nTracks);
        }

        private void SetPerformersTrackNumberComboBoxItems()
        {
            PerformersTrackNumberComboBox.Items.Clear();
            for(int j = 0; j < _nTracks; ++j)
            {
                PerformersTrackNumberComboBox.Items.Add((j + 1).ToString());
            }
        }

        public void Read(XmlReader r)
        {
            Debug.Assert(r.Name == "performerOptions");

            #region get attributes
            int count = r.AttributeCount;
            for(int i = 0; i < count; i++)
            {
                r.MoveToAttribute(i);
                switch(r.Name)
                {
                    case "noteOnPitchTracks":
                        SetTrackCheckBoxes(r.Value, this._noteOnPitchCheckBoxes);
                        break;
                    case "noteOnVelocityTracks":
                        SetTrackCheckBoxes(r.Value, this._noteOnVelocityCheckBoxes);
                        break;
                    case "pressureController":
                        PressureControllerComboBox.Text = r.Value;
                        break;
                    case "pressureTracks":
                        SetTrackCheckBoxes(r.Value, this._pressureCheckBoxes);
                        break;

                    case "pitchWheelController":
                        PitchWheelControllerComboBox.Text = r.Value;
                        break;
                    case "pitchWheelTracks":
                        SetTrackCheckBoxes(r.Value, this._pitchWheelCheckBoxes);
                        break;

                    case "modWheelController":
                        ModWheelControllerComboBox.Text = r.Value;
                        break;
                    case "modWheelTracks":
                        SetTrackCheckBoxes(r.Value, this._modWheelCheckBoxes);
                        break;

                    case "speedController":
                        this.SpeedControllerComboBox.Text = r.Value;
                        break;
                    case "speedMaxPercent":
                        this.MaximumSpeedPercentTextBox.Text = r.Value;
                        break;

                    case "minVolume":
                        this.MinimumVolumeTextBox.Text = r.Value;
                        break;

                    case "trackIndex":
                        _performersTrackIndex = int.Parse(r.Value);
                        Debug.Assert(_performersTrackIndex < this._nTracks);
                        PerformersTrackNumberComboBox.SelectedIndex = _performersTrackIndex;
                        break;
                }
            }
            #endregion

            SetAllTrackComboBoxesFromCheckBoxes(_nTracks);

            SetPerformersTrackPanelDisplay(_performersTrackIndex);

            OptionalMaximumSpeedGroupBox.Enabled = (SpeedControllerComboBox.SelectedIndex > 0);
            SetMinimumVolumePanelEnabledStatus();

            SetSettingsHaveBeenSaved();
            DeselectAll();
        }

        private void SetAllTrackComboBoxesFromCheckBoxes(int nTracks)
        {
            SetComboBoxFromCheckBoxes(null, NoteOnPitchTracksComboBox, nTracks, _performersTrackIndex, _noteOnPitchCheckBoxes);
            SetComboBoxFromCheckBoxes(null, NoteOnVelocityTracksComboBox, nTracks,  _performersTrackIndex, _noteOnVelocityCheckBoxes);
            SetComboBoxFromCheckBoxes(PressureControllerComboBox, PressureTracksComboBox, nTracks, _performersTrackIndex, _pressureCheckBoxes);
            SetComboBoxFromCheckBoxes(PitchWheelControllerComboBox, PitchWheelTracksComboBox, nTracks, _performersTrackIndex, _pitchWheelCheckBoxes);
            SetComboBoxFromCheckBoxes(ModWheelControllerComboBox, ModWheelTracksComboBox, nTracks, _performersTrackIndex, _modWheelCheckBoxes);
        }

        /// <summary>
        /// sets _nTracks from the length of the boolValues string (which contains nTracks 1s and 0s)
        /// </summary>
        /// <param name="value"></param>
        /// <param name="TracksComboBox"></param>
        /// <param name="checkBoxes"></param>
        private void SetTrackCheckBoxes(string boolValues, List<CheckBox> checkBoxes)
        {
            if(_nTracks == -1)
            {
                _nTracks = boolValues.Length;
            }
            else
            {
                Debug.Assert(_nTracks == boolValues.Length);
            }

            for(int i = 0; i < 16; ++i)
            {
                checkBoxes[i].Checked = false;
            }
            for(int i = 0; i < _nTracks; ++i)
            {
                checkBoxes[i].Checked = (boolValues[i].Equals('1'));
            }
        }

        private void SetPerformersTrackPanelDisplay(int performersTrackIndex)
        {
            // set standard trackPanel locations (as in Designer)
            int x = 315, y = 22;
            for(int i = 0; i < _trackPanels.Count; ++i)
            {
                _trackPanels[i].Location = new Point(x + (i * 19), y);
            }

            for(int i = 0; i < 16; ++i)
            {
                if(i == performersTrackIndex)
                {
                    _trackPanels[i].BorderStyle = BorderStyle.FixedSingle;
                    // correct for width of border
                    _trackPanels[i].Location = new Point(_trackPanels[i].Location.X - 1, _trackPanels[i].Location.Y - 1);
                    _trackPanels[i].BringToFront();
                }
                else
                {
                    _trackPanels[i].BorderStyle = BorderStyle.None;
                }
            }
        }

        private void DisableUnusedTrackPanels(int nTracks)
        {
            for(int i = 0; i < 16; ++i)
            {
                if(i < nTracks)
                {
                    _trackPanels[i].Enabled = true;
                }
                else
                {
                    _trackPanels[i].Enabled = false;
                }
            }
        }

        public void Write(XmlWriter w)
        {
            StringBuilder sb;

            w.WriteStartElement("performerOptions");

            sb = GetBoolString(_noteOnPitchCheckBoxes);
            w.WriteAttributeString("noteOnPitchTracks", sb.ToString());

            sb = GetBoolString(_noteOnVelocityCheckBoxes);
            w.WriteAttributeString("noteOnVelocityTracks", sb.ToString());

            w.WriteAttributeString("pressureController", this.PressureControllerComboBox.Text);
            sb = GetBoolString(_pressureCheckBoxes);
            w.WriteAttributeString("pressureTracks", sb.ToString());

            w.WriteAttributeString("pitchWheelController", this.PitchWheelControllerComboBox.Text);
            sb = GetBoolString(_pitchWheelCheckBoxes);
            w.WriteAttributeString("pitchWheelTracks", sb.ToString());

            w.WriteAttributeString("modWheelController", this.ModWheelControllerComboBox.Text);
            sb = GetBoolString(_modWheelCheckBoxes);
            w.WriteAttributeString("modWheelTracks", sb.ToString());

            w.WriteAttributeString("speedController", this.SpeedControllerComboBox.Text);
            w.WriteAttributeString("speedMaxPercent", this.MaximumSpeedPercentTextBox.Text);

            w.WriteAttributeString("minVolume", this.MinimumVolumeTextBox.Text);

            w.WriteAttributeString("trackIndex", this._performersTrackIndex.ToString());

            w.WriteEndElement(); // performerOptions
        }

        private StringBuilder GetBoolString(List<CheckBox> checkBoxes)
        {
            StringBuilder sb = new StringBuilder();
            for(int i = 0; i < _nTracks; ++i)
            {
                sb.Append((checkBoxes[i].Checked) ? '1' : '0');
            }
            return sb;
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

        private List<Panel> GetAllTrackPanels()
        {
            List<Panel> trackPanels = new List<Panel>();

            trackPanels.Add(TrackPanel1);
            trackPanels.Add(TrackPanel2);
            trackPanels.Add(TrackPanel3);
            trackPanels.Add(TrackPanel4);
            trackPanels.Add(TrackPanel5);
            trackPanels.Add(TrackPanel6);
            trackPanels.Add(TrackPanel7);
            trackPanels.Add(TrackPanel8);
            trackPanels.Add(TrackPanel9);
            trackPanels.Add(TrackPanel10);
            trackPanels.Add(TrackPanel11);
            trackPanels.Add(TrackPanel12);
            trackPanels.Add(TrackPanel13);
            trackPanels.Add(TrackPanel14);
            trackPanels.Add(TrackPanel15);
            trackPanels.Add(TrackPanel16);

            return trackPanels;
        }
        private List<CheckBox> GetNoteOnPitchCheckBoxes()
        {
            List<CheckBox> checkBoxes = new List<CheckBox>();
            checkBoxes.Add(this.NoteOnPitchCheckBox1);
            checkBoxes.Add(this.NoteOnPitchCheckBox2);
            checkBoxes.Add(this.NoteOnPitchCheckBox3);
            checkBoxes.Add(this.NoteOnPitchCheckBox4);
            checkBoxes.Add(this.NoteOnPitchCheckBox5);
            checkBoxes.Add(this.NoteOnPitchCheckBox6);
            checkBoxes.Add(this.NoteOnPitchCheckBox7);
            checkBoxes.Add(this.NoteOnPitchCheckBox8);
            checkBoxes.Add(this.NoteOnPitchCheckBox9);
            checkBoxes.Add(this.NoteOnPitchCheckBox10);
            checkBoxes.Add(this.NoteOnPitchCheckBox11);
            checkBoxes.Add(this.NoteOnPitchCheckBox12);
            checkBoxes.Add(this.NoteOnPitchCheckBox13);
            checkBoxes.Add(this.NoteOnPitchCheckBox14);
            checkBoxes.Add(this.NoteOnPitchCheckBox15);
            checkBoxes.Add(this.NoteOnPitchCheckBox16);
            return checkBoxes;
        }
        private List<CheckBox> GetNoteOnVelocityCheckBoxes()
        {
            List<CheckBox> checkBoxes = new List<CheckBox>();
            checkBoxes.Add(this.NoteOnVelocityCheckBox1);
            checkBoxes.Add(this.NoteOnVelocityCheckBox2);
            checkBoxes.Add(this.NoteOnVelocityCheckBox3);
            checkBoxes.Add(this.NoteOnVelocityCheckBox4);
            checkBoxes.Add(this.NoteOnVelocityCheckBox5);
            checkBoxes.Add(this.NoteOnVelocityCheckBox6);
            checkBoxes.Add(this.NoteOnVelocityCheckBox7);
            checkBoxes.Add(this.NoteOnVelocityCheckBox8);
            checkBoxes.Add(this.NoteOnVelocityCheckBox9);
            checkBoxes.Add(this.NoteOnVelocityCheckBox10);
            checkBoxes.Add(this.NoteOnVelocityCheckBox11);
            checkBoxes.Add(this.NoteOnVelocityCheckBox12);
            checkBoxes.Add(this.NoteOnVelocityCheckBox13);
            checkBoxes.Add(this.NoteOnVelocityCheckBox14);
            checkBoxes.Add(this.NoteOnVelocityCheckBox15);
            checkBoxes.Add(this.NoteOnVelocityCheckBox16);
            return checkBoxes;
        }
        private List<CheckBox> GetPressureCheckBoxes()
        {
            List<CheckBox> checkBoxes = new List<CheckBox>();
            checkBoxes.Add(this.PressureCheckBox1);
            checkBoxes.Add(this.PressureCheckBox2);
            checkBoxes.Add(this.PressureCheckBox3);
            checkBoxes.Add(this.PressureCheckBox4);
            checkBoxes.Add(this.PressureCheckBox5);
            checkBoxes.Add(this.PressureCheckBox6);
            checkBoxes.Add(this.PressureCheckBox7);
            checkBoxes.Add(this.PressureCheckBox8);
            checkBoxes.Add(this.PressureCheckBox9);
            checkBoxes.Add(this.PressureCheckBox10);
            checkBoxes.Add(this.PressureCheckBox11);
            checkBoxes.Add(this.PressureCheckBox12);
            checkBoxes.Add(this.PressureCheckBox13);
            checkBoxes.Add(this.PressureCheckBox14);
            checkBoxes.Add(this.PressureCheckBox15);
            checkBoxes.Add(this.PressureCheckBox16);
            return checkBoxes;
        }
        private List<CheckBox> GetPitchWheelCheckBoxes()
        {
            List<CheckBox> checkBoxes = new List<CheckBox>();
            checkBoxes.Add(this.PitchWheelCheckBox1);
            checkBoxes.Add(this.PitchWheelCheckBox2);
            checkBoxes.Add(this.PitchWheelCheckBox3);
            checkBoxes.Add(this.PitchWheelCheckBox4);
            checkBoxes.Add(this.PitchWheelCheckBox5);
            checkBoxes.Add(this.PitchWheelCheckBox6);
            checkBoxes.Add(this.PitchWheelCheckBox7);
            checkBoxes.Add(this.PitchWheelCheckBox8);
            checkBoxes.Add(this.PitchWheelCheckBox9);
            checkBoxes.Add(this.PitchWheelCheckBox10);
            checkBoxes.Add(this.PitchWheelCheckBox11);
            checkBoxes.Add(this.PitchWheelCheckBox12);
            checkBoxes.Add(this.PitchWheelCheckBox13);
            checkBoxes.Add(this.PitchWheelCheckBox14);
            checkBoxes.Add(this.PitchWheelCheckBox15);
            checkBoxes.Add(this.PitchWheelCheckBox16);
            return checkBoxes;
        }
        private List<CheckBox> GetModWheelCheckBoxes()
        {
            List<CheckBox> checkBoxes = new List<CheckBox>();
            checkBoxes.Add(this.ModWheelCheckBox1);
            checkBoxes.Add(this.ModWheelCheckBox2);
            checkBoxes.Add(this.ModWheelCheckBox3);
            checkBoxes.Add(this.ModWheelCheckBox4);
            checkBoxes.Add(this.ModWheelCheckBox5);
            checkBoxes.Add(this.ModWheelCheckBox6);
            checkBoxes.Add(this.ModWheelCheckBox7);
            checkBoxes.Add(this.ModWheelCheckBox8);
            checkBoxes.Add(this.ModWheelCheckBox9);
            checkBoxes.Add(this.ModWheelCheckBox10);
            checkBoxes.Add(this.ModWheelCheckBox11);
            checkBoxes.Add(this.ModWheelCheckBox12);
            checkBoxes.Add(this.ModWheelCheckBox13);
            checkBoxes.Add(this.ModWheelCheckBox14);
            checkBoxes.Add(this.ModWheelCheckBox15);
            checkBoxes.Add(this.ModWheelCheckBox16);
            return checkBoxes;
        }

        private List<TextBox> GetAllTextBoxes()
        {
            List<TextBox> textBoxes = new List<TextBox>();

            textBoxes.Add(MaximumSpeedPercentTextBox);
            textBoxes.Add(MinimumVolumeTextBox);

            return textBoxes;
        }

        private void DeselectAll()
        {
            bool settingsHaveBeenSaved = Text[Text.Length - 1] != '*';
            this.PerformersTrackLabel.Focus();
            if(settingsHaveBeenSaved)
                SetSettingsHaveBeenSaved();
            else
                SetSettingsNotSaved();
        }

        private void PerformerOptionsForm_Activated(object sender, EventArgs e)
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

            this.SaveSettingsButton.Enabled = okay;

            return okay;
        }

        private void SetCheckBoxesFromComboBox(ComboBox controllerComboBox, ComboBox tracksComboBox, int performersTrackIndex, List<CheckBox> checkBoxes)
        {
            if(controllerComboBox != null)
                controllerComboBox.Enabled = true;

            // trackComboBoxes contain the following items: 'none', 'performer', 'other', 'all', 'custom'
            switch(tracksComboBox.Text)
            {
                case "none":
                    for(int i = 0; i < _nTracks; ++i)
                    {
                        checkBoxes[i].Checked = false;
                    }
                    if(controllerComboBox != null)
                        controllerComboBox.Enabled = false;
                    break;
                case "performer":
                    for(int i = 0; i < _nTracks; ++i)
                    {
                        checkBoxes[i].Checked = (i == performersTrackIndex);
                    }
                    break;
                case "other":
                    for(int i = 0; i < _nTracks; ++i)
                    {
                        checkBoxes[i].Checked = (i != performersTrackIndex);
                    }
                    break;
                case "all":
                    for(int i = 0; i < _nTracks; ++i)
                    {
                        checkBoxes[i].Checked = true;
                    }
                    break;
                case "custom":
                    SetComboBoxFromCheckBoxes(controllerComboBox, tracksComboBox, _nTracks,  performersTrackIndex, checkBoxes);
                    break;
                default:
                    throw new Exception("Unknown menu item.");
            }
            SetMinimumVolumePanelEnabledStatus();
        }

        private void SetComboBoxFromCheckBoxes(ComboBox controllerComboBox, ComboBox trackComboBox, int nTracks, int performersTrackIndex, List<CheckBox> checkBoxes)
        {
            // trackComboBoxes contain the following items: 'none', 'performer', 'other', 'all', 'custom'
            bool none = false;
            bool all = false;
            bool performer = false;
            bool other = false;
            bool custom = false;

            none = IsNone(nTracks, checkBoxes);
            if(!none)
            {
                all = IsAll(nTracks, checkBoxes);
                if(!all)
                {
                    performer = IsPerformer(nTracks, performersTrackIndex, checkBoxes);
                    if(!performer)
                    {
                        other = IsOther(nTracks, performersTrackIndex, checkBoxes);
                        if(!other)
                        {
                            custom = true;
                        }
                    }
                }
            }

            if(controllerComboBox != null)
                controllerComboBox.Enabled = true;

            if(all)
                trackComboBox.Text = "all";
            else if(none)
            {
                trackComboBox.Text = "none";
                if(controllerComboBox != null)
                    controllerComboBox.Enabled = false;
            }
            else if(performer)
                trackComboBox.Text = "performer";
            else if(other)
                trackComboBox.Text = "other";
            else if(custom)
                trackComboBox.Text = "custom";

            SetMinimumVolumePanelEnabledStatus();
        }

        private bool IsNone(int nTracks, List<CheckBox> checkBoxes)
        {
            bool rval = true;
            for(int i = 0; i < nTracks; ++i)
            {
                if(checkBoxes[i].Checked == true)
                {
                    rval = false;
                    break;
                }
            }
            return rval;
        }
        private bool IsAll(int nTracks, List<CheckBox> checkBoxes)
        {
            bool rval = true;
            for(int i = 0; i < nTracks; ++i)
            {
                if(checkBoxes[i].Checked == false)
                {
                    rval = false;
                    break;
                }
            }
            return rval;
        }
        private bool IsPerformer(int nTracks, int performersTrackIndex, List<CheckBox> checkBoxes)
        {
            bool rval = true;
            for(int i = 0; i < nTracks; ++i)
            {
                if((i == performersTrackIndex && checkBoxes[i].Checked == false)
                || (i != performersTrackIndex && checkBoxes[i].Checked == true))
                {
                    rval = false;
                    break;
                }
            }
            return rval;
        }
        private bool IsOther(int nTracks, int performersTrackIndex, List<CheckBox> checkBoxes)
        {
            bool rval = true;
            for(int i = 0; i < nTracks; ++i)
            {
                if((i != performersTrackIndex && checkBoxes[i].Checked == false)
                || (i == performersTrackIndex && checkBoxes[i].Checked == true))
                {
                    rval = false;
                    break;
                }
            }
            return rval;
        }

        private void PerformersTrackNumberComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            _performersTrackIndex = PerformersTrackNumberComboBox.SelectedIndex;

            SetAllTrackComboBoxesFromCheckBoxes(_nTracks);
            SetPerformersTrackPanelDisplay(_performersTrackIndex);
            SetSettingsNotSaved();
        }

        private void NoteOnPitchTracksComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetCheckBoxesFromComboBox(null, NoteOnPitchTracksComboBox, _performersTrackIndex, _noteOnPitchCheckBoxes);
            SetSettingsNotSaved();
        }

        private void NoteOnVelocityTracksComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetCheckBoxesFromComboBox(null, NoteOnVelocityTracksComboBox, _performersTrackIndex, _noteOnVelocityCheckBoxes);
            SetSettingsNotSaved();
        }

        private void NoteOnPitchTrackCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            SetComboBoxFromCheckBoxes(null, NoteOnPitchTracksComboBox, _nTracks, _performersTrackIndex, _noteOnPitchCheckBoxes);
            SetSettingsNotSaved();
        }

        private void NoteOnVelocityTrackCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            SetComboBoxFromCheckBoxes(null, NoteOnVelocityTracksComboBox, _nTracks, _performersTrackIndex, _noteOnVelocityCheckBoxes);
            SetSettingsNotSaved();
        }

        private void PressureTrackCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            SetComboBoxFromCheckBoxes(PressureControllerComboBox, PressureTracksComboBox, _nTracks, _performersTrackIndex, _pressureCheckBoxes);
            SetSettingsNotSaved();

        }
        private void PitchWheelTrackCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            SetComboBoxFromCheckBoxes(PitchWheelControllerComboBox, PitchWheelTracksComboBox, _nTracks, _performersTrackIndex, _pitchWheelCheckBoxes);
            SetSettingsNotSaved();
        }
        private void ModWheelTrackCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            SetComboBoxFromCheckBoxes(ModWheelControllerComboBox, ModWheelTracksComboBox, _nTracks, _performersTrackIndex, _modWheelCheckBoxes);
            SetSettingsNotSaved();
        }

        private void PressureControllerComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetMinimumVolumePanelEnabledStatus();
            SetSettingsNotSaved();
        }

        private void SetMinimumVolumePanelEnabledStatus()
        {
            OptionalMinimumVolumeGroupBox.Enabled =
                ((PressureControllerComboBox.Text == "volume (7)" && PressureControllerComboBox.Enabled)
                || (PitchWheelControllerComboBox.Text == "volume (7)" && PitchWheelControllerComboBox.Enabled)
                || (ModWheelControllerComboBox.Text == "volume (7)" && ModWheelControllerComboBox.Enabled));
        }

        private void PressureTracksComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetCheckBoxesFromComboBox(PressureControllerComboBox, PressureTracksComboBox, _performersTrackIndex, _pressureCheckBoxes);
            SetSettingsNotSaved();
        }

        private void PitchWheelControllerComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetMinimumVolumePanelEnabledStatus();
            SetSettingsNotSaved();
        }

        private void PitchWheelTracksComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetCheckBoxesFromComboBox(PitchWheelControllerComboBox, PitchWheelTracksComboBox, _performersTrackIndex, _pitchWheelCheckBoxes);
            SetSettingsNotSaved();
        }

        private void ModWheelControllerComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetMinimumVolumePanelEnabledStatus();
            SetSettingsNotSaved();
        }

        private void ModWheelTracksComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetCheckBoxesFromComboBox(ModWheelControllerComboBox, ModWheelTracksComboBox, _performersTrackIndex, _modWheelCheckBoxes);
            SetSettingsNotSaved();
        }

        private void SpeedControllComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            OptionalMaximumSpeedGroupBox.Enabled = (SpeedControllerComboBox.SelectedIndex > 0);
            SetSettingsNotSaved();
        }

        private void MaximumSpeedTextBox_TextChanged(object sender, EventArgs e)
        {
            if(this.CheckTextBoxIsFloat(this.MaximumSpeedPercentTextBox))
                SetSettingsNotSaved();
        }

        private void MinimumVolumeTextBox_TextChanged(object sender, EventArgs e)
        {
            if(this.CheckTextBoxIsFloat(this.MinimumVolumeTextBox))
                SetSettingsNotSaved();
        }

        private void ShowMainScoreFormButton_Click(object sender, EventArgs e)
        {
            this._assistantComposerMainForm.BringToFront();
        }

        private void SaveSettingsButton_Click(object sender, EventArgs e)
        {
            this._assistantComposerMainForm.SaveSettings(0);
            SetSettingsHaveBeenSaved();
        }

        #endregion

        private int _nTracks;
        private List<Panel> _trackPanels;

        private int _performersTrackIndex = 0;
        private List<CheckBox> _noteOnPitchCheckBoxes;
        private List<CheckBox> _noteOnVelocityCheckBoxes;
        private List<CheckBox> _pressureCheckBoxes;
        private List<CheckBox> _pitchWheelCheckBoxes;
        private List<CheckBox> _modWheelCheckBoxes;

        private AssistantComposerMainForm _assistantComposerMainForm = null;
    }
}
