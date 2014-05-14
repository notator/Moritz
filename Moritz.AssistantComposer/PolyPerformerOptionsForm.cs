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
    public partial class PolyPerformerOptionsForm : Form
    {
        /// <summary>
        /// Creates a new, empty PerformerOptionsForm for setting the following options.
        /// These are all defaults, that will be adjustable (as before) in the AP:
        /// </summary>
        public PolyPerformerOptionsForm(AssistantComposerMainForm assistantComposerMainForm, int nTracks)
        {
            InitializeComponent();

            _assistantComposerMainForm = assistantComposerMainForm;
            _nTracks = nTracks;
        }

        #region public interface
        #region not yet implemented
        public void Read(XmlReader r)
        {
            Debug.Assert(r.Name == "polyPerformerOptions");

            throw new NotImplementedException();

            #region old (monoPerformerOpions)
            #region get attributes
            //int count = r.AttributeCount;
            //for(int i = 0; i < count; i++)
            //{
            //    r.MoveToAttribute(i);
            //    switch(r.Name)
            //    {
            //        case "noteOnPitchTracks":
            //            SetTrackCheckBoxes(r.Value, this._noteOnPitchCheckBoxes);
            //            break;

            //        case "trackIndex":
            //            _performersTrackIndex = int.Parse(r.Value);
            //            Debug.Assert(_performersTrackIndex < this._nTracks);
            //            PerformersTrackNumberComboBox.SelectedIndex = _performersTrackIndex;
            //            break;
            //    }
            //}
            #endregion

            //SetAllTrackComboBoxesFromCheckBoxes(_nTracks);

            //SetPerformersTrackPanelDisplay(_performersTrackIndex);

            //OptionalMaximumSpeedGroupBox.Enabled = (SpeedControllerComboBox.SelectedIndex > 0);
            //SetMinimumVolumePanelEnabledStatus();

            //SetSettingsHaveBeenSaved();
            //DeselectAll();
            #endregion
        }

        public void Write(XmlWriter w)
        {
            // w.WriteStartElement("polyPerformerOptions");
            throw new NotImplementedException();

            #region old (monoPerformerOptions)
            //StringBuilder sb;
            //if(NoteOnPitchTracksComboBox.Text != "none")
            //{
            //    sb = GetBoolString(_noteOnPitchCheckBoxes);
            //    w.WriteAttributeString("noteOnPitchTracks", sb.ToString());
            //}

            //if(NoteOnVelocityTracksComboBox.Text != "none")
            //{
            //    sb = GetBoolString(_noteOnVelocityCheckBoxes);
            //    w.WriteAttributeString("noteOnVelocityTracks", sb.ToString());
            //}

            //if(PressureTracksComboBox.Text != "none")
            //{
            //    w.WriteAttributeString("pressureController", this.PressureControllerComboBox.Text);
            //    sb = GetBoolString(_pressureCheckBoxes);
            //    w.WriteAttributeString("pressureTracks", sb.ToString());
            //}

            //if(PitchWheelTracksComboBox.Text != "none")
            //{
            //    w.WriteAttributeString("pitchWheelController", this.PitchWheelControllerComboBox.Text);
            //    sb = GetBoolString(_pitchWheelCheckBoxes);
            //    w.WriteAttributeString("pitchWheelTracks", sb.ToString());
            //}

            //if(ModWheelTracksComboBox.Text != "none")
            //{
            //    w.WriteAttributeString("modWheelController", this.ModWheelControllerComboBox.Text);
            //    sb = GetBoolString(_modWheelCheckBoxes);
            //    w.WriteAttributeString("modWheelTracks", sb.ToString());
            //}

            //if(SpeedControllerComboBox.Text != "none")
            //{
            //    w.WriteAttributeString("speedController", this.SpeedControllerComboBox.Text);
            //    w.WriteAttributeString("speedMaxPercent", this.MaximumSpeedPercentTextBox.Text);
            //}

            //if(this.MinimumVolumeTextBox.Enabled)
            //{
            //    w.WriteAttributeString("minVolume", this.MinimumVolumeTextBox.Text);
            //}

            //w.WriteAttributeString("trackIndex", this._performersTrackIndex.ToString());
            #endregion

            //w.WriteEndElement(); // polyPerformerOptions
        }

        public bool IsEmpty()
        {
            return true; // i.e. not implemented

            #region old (monoPerformerOpions)
            //bool rval = true;
            //
            //if((NoteOnPitchTracksComboBox.Text != "none")
            //|| (NoteOnVelocityTracksComboBox.Text != "none")
            //|| (PressureTracksComboBox.Text != "none")
            //|| (PitchWheelTracksComboBox.Text != "none")
            //|| (ModWheelTracksComboBox.Text != "none")
            //|| (SpeedControllerComboBox.Text != "none"))
            //    rval = false;
            //
            //return rval;
            #endregion
        }
        public bool HasError()
        {
            throw new NotImplementedException();
            #region old (monoPerformerOptions)
            //bool error = false;
            //List<TextBox> textBoxes = GetAllTextBoxes();
            //foreach(TextBox textBox in textBoxes)
            //{
            //    if(textBox.BackColor == M.TextBoxErrorColor)
            //    {
            //        error = true;
            //        break;
            //    }
            //}
            //return error;
            #endregion
        }
        #endregion not yet implemented
        #region implemented
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
        #endregion implemented
        #endregion public interface

        // could be useful (used by monoPerformerOptions.Write()
        private StringBuilder GetBoolString(List<CheckBox> checkBoxes)
        {
            StringBuilder sb = new StringBuilder();
            for(int i = 0; i < _nTracks; ++i)
            {
                sb.Append((checkBoxes[i].Checked) ? '1' : '0');
            }
            return sb;
        }

        #region event handlers
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
        private AssistantComposerMainForm _assistantComposerMainForm = null;
    }
}
