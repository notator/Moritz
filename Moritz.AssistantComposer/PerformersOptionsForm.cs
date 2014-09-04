﻿using System;
using System.Windows.Forms;
using System.Xml;
using System.Diagnostics;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

using Krystals4ObjectLibrary;
using Moritz.Globals;
using Moritz.Krystals;
using Moritz.Score;
using Moritz.Score.Midi;
using Moritz.Score.Notation;
using Moritz.AssistantPerformer;

namespace Moritz.AssistantComposer
{
    public partial class PerformersOptionsForm : Form
    {
        /// <summary>
        /// Creates a new, empty MonoPerformerOptionsForm.
        /// These are all defaults, that will be adjustable (as before) in the AP:
        /// </summary>
        public PerformersOptionsForm(Form assistantComposerMainForm, int nTracks)
        {
            InitializeComponent();

            SetMasterVolumeControls(nTracks);

            _trackPanels = GetAllTrackPanels();
            _topTrackLabels = GetAllTopTrackLabels();
            _masterVolumeTrackLabels = GetAllMasterVolumeTrackLabels();
            _pressureCheckBoxes = GetPressureCheckBoxes();
            _pitchWheelCheckBoxes = GetPitchWheelCheckBoxes();
            _modWheelCheckBoxes = GetModWheelCheckBoxes();

            _assistantComposerMainForm = assistantComposerMainForm;

            #region set default values
            this._nTracks = nTracks;
            SetPerformersTrackNumberComboBoxItems();
            this.PerformersTrackNumberComboBox.Text = "1";
            this.PerformersEventHandlerComboBox.Text = "none";

            this.PressureControllerComboBox.Text = "aftertouch";
            this.PressureTracksComboBox.Text = "none";
            this.PitchWheelControllerComboBox.Text = "pitch wheel";
            this.PitchWheelTracksComboBox.Text = "none";
            this.ModWheelControllerComboBox.Text = "modulation (1)";
            this.ModWheelTracksComboBox.Text = "none";

            this.SpeedControllerComboBox.SelectedIndex = 0; // "none";
            this.MaximumSpeedTextBox.Text = "400";
            SpeedControllComboBox_SelectedIndexChanged(SpeedControllerComboBox, null);

            this.MinimumVolumeTextBox.Enabled = false;
            this.MinimumVolumeTextBox.Text = "50";
            #endregion
        }

        private void SetMasterVolumeControls(int nTracks)
        {
            int textBoxWidth = 30;
            int width = this.masterVolumeLabel.Size.Width + 3 + (nTracks * (textBoxWidth + 1));
            int deltaX = ((this.masterVolumeGroupBox.Size.Width - width) / 2);

            _ilcMasterVolumes = new IntListControl(111, 26, 30, 0, 127, nTracks, ContainedControlHasChanged);
            this.masterVolumeGroupBox.Controls.Add(_ilcMasterVolumes);

            foreach(Control c in this.masterVolumeGroupBox.Controls)
            {
                if(c.Name.CompareTo(MVCommentLabel.Name) != 0)
                {
                    c.Location = new Point(c.Location.X + deltaX, c.Location.Y);
                }
            }
        }

        /// <summary>
        /// Called by a contained IntListControl when it changes
        /// </summary>
        public void ContainedControlHasChanged(IntListControl sender)
        {
            _intListControlHasError = false;

            if(sender == _ilcMasterVolumes)
            {
                if(_ilcMasterVolumes.HasError())
                {
                    _intListControlHasError = true;
                }

                if(!_intListControlHasError)
                {
                    _ilcMasterVolumes.SetColoredDefaultTexts(100);
                }
            }

            SaveSettingsButton.Enabled = !_intListControlHasError;
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
            Debug.Assert(r.Name == "performersOptions");

            #region get attributes
            int count = r.AttributeCount;
            for(int i = 0; i < count; i++)
            {
                r.MoveToAttribute(i);
                switch(r.Name)
                {
                    case "performersEventHandler":
                        PerformersEventHandlerComboBox.Text = r.Value;
                        break;

                    case "trackIndex":
                        _performersTrackIndex = int.Parse(r.Value);
                        Debug.Assert(_performersTrackIndex < this._nTracks);
                        PerformersTrackNumberComboBox.SelectedIndex = _performersTrackIndex;
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

                    case "masterVolumes":
                        _ilcMasterVolumes.Set(r.Value);
                        _ilcMasterVolumes.SetColoredDefaultTexts(100);
                        break;

                    case "speedController":
                        this.SpeedControllerComboBox.Text = r.Value;
                        break;
                    case "speedMaxPercent":
                        this.MaximumSpeedTextBox.Text = r.Value;
                        break;

                    case "minVolume":
                        this.MinimumVolumeTextBox.Text = r.Value;
                        break;
                }
            }
            #endregion

            SetAllTrackComboBoxesFromCheckBoxes(_nTracks);

            SetDisplay(_nTracks, _performersTrackIndex);

            SetMinimumVolumePanelEnabledStatus();

            SetSettingsHaveBeenSaved();
            DeselectAll();
        }

        private void SetAllTrackComboBoxesFromCheckBoxes(int nTracks)
        {
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

        private void SetDisplay(int nTracks, int performersTrackIndex)
        {
            // set standard trackPanel locations (as in Designer)
            int x = 313, y = 31;
            for(int i = 0; i < _trackPanels.Count; ++i)
            {
                _trackPanels[i].Location = new Point(x + (i * 19), y);
            }

            for(int i = 0; i < 16; ++i)
            {
                if(i < nTracks)
                {
                    _trackPanels[i].Show();
                    _masterVolumeTrackLabels[i].Show();

                    if(i == performersTrackIndex)
                    {
                        _topTrackLabels[i].ForeColor = Color.Blue;
                        _masterVolumeTrackLabels[i].ForeColor = Color.Blue;
                        _trackPanels[i].BorderStyle = BorderStyle.FixedSingle;
                        // correct for width of border
                        _trackPanels[i].Location = new Point(_trackPanels[i].Location.X - 1, _trackPanels[i].Location.Y - 1);
                        _trackPanels[i].BringToFront();
                    }
                    else
                    {
                        _topTrackLabels[i].ForeColor = Color.Black;
                        _masterVolumeTrackLabels[i].ForeColor = Color.Black;
                        _trackPanels[i].BorderStyle = BorderStyle.None;
                    }
                }
                else
                {
                    _trackPanels[i].Hide();
                    _masterVolumeTrackLabels[i].Hide();
                }
            }
        }

        public void Write(XmlWriter w)
        {
            StringBuilder sb;

            w.WriteStartElement("performersOptions");

            w.WriteAttributeString("performersEventHandler", this.PerformersEventHandlerComboBox.Text);

            w.WriteAttributeString("trackIndex", this._performersTrackIndex.ToString());

            if(PressureTracksComboBox.Text != "none")
            {
                w.WriteAttributeString("pressureController", this.PressureControllerComboBox.Text);
                sb = GetBoolString(_pressureCheckBoxes);
                w.WriteAttributeString("pressureTracks", sb.ToString());
            }

            if(PitchWheelTracksComboBox.Text != "none")
            {
                w.WriteAttributeString("pitchWheelController", this.PitchWheelControllerComboBox.Text);
                sb = GetBoolString(_pitchWheelCheckBoxes);
                w.WriteAttributeString("pitchWheelTracks", sb.ToString());
            }

            if(ModWheelTracksComboBox.Text != "none")
            {
                w.WriteAttributeString("modWheelController", this.ModWheelControllerComboBox.Text);
                sb = GetBoolString(_modWheelCheckBoxes);
                w.WriteAttributeString("modWheelTracks", sb.ToString());
            }

            if(!this._ilcMasterVolumes.IsEmpty())
            {
                w.WriteAttributeString("masterVolumes", _ilcMasterVolumes.ValuesAsString());
            }

            if(SpeedControllerComboBox.Text != "none")
            {
                w.WriteAttributeString("speedController", this.SpeedControllerComboBox.Text);
                w.WriteAttributeString("speedMaxPercent", this.MaximumSpeedTextBox.Text);
            }

            if(this.MinimumVolumeTextBox.Enabled)
            {
                w.WriteAttributeString("minVolume", this.MinimumVolumeTextBox.Text);
            }

            w.WriteEndElement(); // performersOptions
        }

        public bool HasEventHandler()
        {
            return (PerformersEventHandlerComboBox.Text != "none");
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

            error = error || _intListControlHasError;

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

        private List<Label> GetAllTopTrackLabels()
        {
            List<Label> trackLabels = new List<Label>();

            trackLabels.Add(TrackLabel1);
            trackLabels.Add(TrackLabel2);
            trackLabels.Add(TrackLabel3);
            trackLabels.Add(TrackLabel4);
            trackLabels.Add(TrackLabel5);
            trackLabels.Add(TrackLabel6);
            trackLabels.Add(TrackLabel7);
            trackLabels.Add(TrackLabel8);
            trackLabels.Add(TrackLabel9);
            trackLabels.Add(TrackLabel10);
            trackLabels.Add(TrackLabel11);
            trackLabels.Add(TrackLabel12);
            trackLabels.Add(TrackLabel13);
            trackLabels.Add(TrackLabel14);
            trackLabels.Add(TrackLabel15);
            trackLabels.Add(TrackLabel16);

            return trackLabels;
        }

        private List<Label> GetAllMasterVolumeTrackLabels()
        {
            List<Label> trackLabels = new List<Label>();

            trackLabels.Add(MVTrackLabel1);
            trackLabels.Add(MVTrackLabel2);
            trackLabels.Add(MVTrackLabel3);
            trackLabels.Add(MVTrackLabel4);
            trackLabels.Add(MVTrackLabel5);
            trackLabels.Add(MVTrackLabel6);
            trackLabels.Add(MVTrackLabel7);
            trackLabels.Add(MVTrackLabel8);
            trackLabels.Add(MVTrackLabel9);
            trackLabels.Add(MVTrackLabel10);
            trackLabels.Add(MVTrackLabel11);
            trackLabels.Add(MVTrackLabel12);
            trackLabels.Add(MVTrackLabel13);
            trackLabels.Add(MVTrackLabel14);
            trackLabels.Add(MVTrackLabel15);
            trackLabels.Add(MVTrackLabel16);

            return trackLabels;
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

            textBoxes.Add(MaximumSpeedTextBox);
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
            SetDisplay(_nTracks, _performersTrackIndex);
            SetSettingsNotSaved();
        }


        private void PerformersEventHandlerComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(PerformersEventHandlerComboBox.Text == "none")
            {
                SettingsPanel.Enabled = false;
            }
            else
            {
                SettingsPanel.Enabled = true;
            }
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
            bool enabled =
                ((PressureControllerComboBox.Text == "volume (7)" && PressureControllerComboBox.Enabled)
                || (PitchWheelControllerComboBox.Text == "volume (7)" && PitchWheelControllerComboBox.Enabled)
                || (ModWheelControllerComboBox.Text == "volume (7)" && ModWheelControllerComboBox.Enabled));

            MinimumVolumeLabel.Enabled = enabled;
            MinimumVolumeTextBox.Enabled = enabled;
            MinimumVolumeRangeLabel.Enabled = enabled;
            MinimumVolumeHelpLabel.Enabled = enabled;
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
            ComboBox cb = sender as ComboBox;
            if(cb != null)
            {
                bool enabled = (cb.SelectedIndex > 0);
                MaximumSpeedLabel.Enabled = enabled;
                MaximumSpeedTextBox.Enabled = enabled;
                MaximumSpeedPercentLabel.Enabled = enabled;
                MaximumSpeedHelpLabel.Enabled = enabled;

                SetSettingsNotSaved();
            }
        }

        private void MaximumSpeedTextBox_TextChanged(object sender, EventArgs e)
        {
            if(this.CheckTextBoxIsFloat(this.MaximumSpeedTextBox))
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
        private List<CheckBox> _pressureCheckBoxes;
        private List<CheckBox> _pitchWheelCheckBoxes;
        private List<CheckBox> _modWheelCheckBoxes;

        private List<Label> _topTrackLabels;
        private List<Label> _masterVolumeTrackLabels;

        static private bool _intListControlHasError = false;
        private IntListControl _ilcMasterVolumes;

        private Form _assistantComposerMainForm = null;

    }
}