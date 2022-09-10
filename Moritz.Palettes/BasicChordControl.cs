using Moritz.Globals;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;
using System.Xml;

namespace Moritz.Palettes
{
    public partial class BasicChordControl : UserControl
    {
        public BasicChordControl(SetDialogStateDelegate setDialogState)
        {
            InitializeComponent();
            SetDialogState = setDialogState;
        }

        public void ReadBasicChordControl(XmlReader r)
        {
            Debug.Assert(r.Name == "basicChord" && r.IsStartElement());

            #region default values
            DurationsTextBox.Text = "";
            VelocitiesTextBox.Text = "";
            MidiPitchesTextBox.Text = "";
            ChordOffsTextBox.Text = "";
            ChordDensitiesTextBox.Text = "";
            RootInversionTextBox.Text = "";
            InversionIndicesTextBox.Text = "";
            VerticalVelocityFactorsTextBox.Text = "";
            #endregion

            if(!r.IsEmptyElement)
            {
                M.ReadToXmlElementTag(r,
                    "durations", "velocities", "midiPitches",
                    "chordOffs", "chordDensities", "rootInversion", "inversionIndices", "verticalVelocityFactors");
                while(r.Name == "durations" || r.Name == "velocities" || r.Name == "midiPitches"
                    || r.Name == "chordOffs"
                    || r.Name == "chordDensities" || r.Name == "rootInversion"
                    || r.Name == "inversionIndices" || r.Name == "verticalVelocityFactors")
                {
                    if(r.NodeType != XmlNodeType.EndElement)
                    {
                        switch(r.Name)
                        {
                            case "durations":
                                DurationsTextBox.Text = r.ReadElementContentAsString();
                                break;
                            case "velocities":
                                VelocitiesTextBox.Text = r.ReadElementContentAsString();
                                break;
                            case "midiPitches":
                                MidiPitchesTextBox.Text = r.ReadElementContentAsString();
                                break;
                            case "chordOffs":
                                ChordOffsTextBox.Text = r.ReadElementContentAsString();
                                break;
                            case "chordDensities":
                                ChordDensitiesTextBox.Text = r.ReadElementContentAsString();
                                break;
                            case "rootInversion":
                                RootInversionTextBox.Text = r.ReadElementContentAsString();
                                break;
                            case "inversionIndices":
                                InversionIndicesTextBox.Text = r.ReadElementContentAsString();
                                break;
                            case "verticalVelocityFactors":
                                VerticalVelocityFactorsTextBox.Text = r.ReadElementContentAsString();
                                break;
                        }
                    }
                    M.ReadToXmlElementTag(r,
                        "basicChord", "durations", "velocities", "midiPitches",
                        "chordOffs", "chordDensities", "rootInversion", "inversionIndices", "verticalVelocityFactors");
                }
                Debug.Assert(r.Name == "basicChord"); // end element
            }
        }

        public void WriteBasicChordControl(XmlWriter w)
        {
            w.WriteStartElement("basicChord");
            if(!string.IsNullOrEmpty(DurationsTextBox.Text))
            {
                w.WriteStartElement("durations");
                w.WriteString(DurationsTextBox.Text.Replace(" ", ""));
                w.WriteEndElement();
            }

            if(!string.IsNullOrEmpty(VelocitiesTextBox.Text))
            {
                w.WriteStartElement("velocities");
                w.WriteString(VelocitiesTextBox.Text.Replace(" ", ""));
                w.WriteEndElement();
            }
            if(!string.IsNullOrEmpty(MidiPitchesTextBox.Text))
            {
                w.WriteStartElement("midiPitches");
                w.WriteString(MidiPitchesTextBox.Text.Replace(" ", ""));
                w.WriteEndElement();
            }
            if(!string.IsNullOrEmpty(this.ChordOffsTextBox.Text))
            {
                w.WriteStartElement("chordOffs");
                w.WriteString(ChordOffsTextBox.Text.Replace(" ", ""));
                w.WriteEndElement();
            }
            if(!string.IsNullOrEmpty(ChordDensitiesTextBox.Text))
            {
                w.WriteStartElement("chordDensities");
                w.WriteString(ChordDensitiesTextBox.Text.Replace(" ", ""));
                w.WriteEndElement();
            }
            if(!string.IsNullOrEmpty(RootInversionTextBox.Text))
            {
                w.WriteStartElement("rootInversion");
                w.WriteString(RootInversionTextBox.Text.Replace(" ", ""));
                w.WriteEndElement();
            }
            if(!string.IsNullOrEmpty(InversionIndicesTextBox.Text))
            {
                w.WriteStartElement("inversionIndices");
                w.WriteString(InversionIndicesTextBox.Text.Replace(" ", ""));
                w.WriteEndElement();
            }
            if(!string.IsNullOrEmpty(VerticalVelocityFactorsTextBox.Text))
            {
                w.WriteStartElement("verticalVelocityFactors");
                w.WriteString(VerticalVelocityFactorsTextBox.Text.Replace(" ", ""));
                w.WriteEndElement();
            }
            w.WriteEndElement();
        }

        public void SetHelpLabels()
        {
            if(_numberOfChordValues == -1)
            {
                DurationsHelpLabel.Text = "";
                MidiPitchesHelpLabel.Text = "";
                VelocitiesHelpLabel.Text = "";
                ChordOffsHelpLabel.Text = "";
                ChordDensitiesHelpLabel.Text = "";
            }
            else
            {
                string valStr = (_numberOfChordValues == 1) ? " value " : " values ";
                DurationsHelpLabel.Text = _numberOfChordValues.ToString() + " integer" + valStr + "(greater than 0)";
                MidiPitchesHelpLabel.Text = _numberOfChordValues.ToString() + " integer" + valStr + "in range [ 0..127 ]";
                VelocitiesHelpLabel.Text = _numberOfChordValues.ToString() + " integer" + valStr + "in range [ 0..127 ]";
                ChordOffsHelpLabel.Text = _numberOfChordValues.ToString() + " boolean" + valStr + "( 1=true, 0=false )";
                ChordDensitiesHelpLabel.Text = _numberOfChordValues.ToString() + " integer" + valStr + "in range [ 0..128 ]";
            }
        }

        #region Event handlers
        /// <summary>
        /// Used by all textBoxes.
        /// </summary>
        private void SetToWhiteTextBox_TextChanged(object sender, EventArgs e)
        {
            TextBox textBox = sender as TextBox;
            M.SetToWhite(textBox);
        }
        #region Leave handlers
        private void DurationsTextBox_Leave(object sender, EventArgs e)
        {
            M.LeaveIntRangeTextBox(sender as TextBox, false, (uint)NumberOfChordValues, 0, int.MaxValue, SetDialogState);
        }
        private void VelocitiesTextBox_Leave(object sender, EventArgs e)
        {
            M.LeaveIntRangeTextBox(sender as TextBox, false, (uint)NumberOfChordValues, 0, 127, SetDialogState);
        }
        private void MidiPitchesTextBox_Leave(object sender, EventArgs e)
        {
            M.LeaveIntRangeTextBox(sender as TextBox, false, (uint)NumberOfChordValues, 0, 127, SetDialogState);
        }
        private void ChordOffsTextBox_Leave(object sender, EventArgs e)
        {
            M.LeaveIntRangeTextBox(sender as TextBox, true, (uint)NumberOfChordValues, 0, 1, SetDialogState);
        }
        private void ChordDensitiesTextBox_Leave(object sender, EventArgs e)
        {
            M.LeaveIntRangeTextBox(sender as TextBox, false, (uint)NumberOfChordValues, 0, 128, SetDialogState);
            SetChordControls();
        }
        private void RootInversionTextBox_Leave(object sender, EventArgs e)
        {
            bool canBeEmpty = true;
            TextBox rootInversionTextBox = sender as TextBox;
            if(rootInversionTextBox.Enabled)
                canBeEmpty = false;

            M.LeaveIntRangeTextBox(rootInversionTextBox, canBeEmpty, (uint)MaximumChordDensity - 1, 1, 127, SetDialogState);
        }
        private void InversionIndicesTextBox_Leave(object sender, EventArgs e)
        {
            TextBox inversionsTextBox = sender as TextBox;

            bool canBeEmpty = true;
            if(inversionsTextBox.Enabled)
                canBeEmpty = false;

            int maxVal = (2 * (MaximumChordDensity - 2)) - 1;
            M.LeaveIntRangeTextBox(inversionsTextBox, canBeEmpty, (uint)NumberOfChordValues, 0, maxVal, SetDialogState);
        }
        private void VerticalVelocityFactorsTextBox_Leave(object sender, EventArgs e)
        {
            M.LeaveFloatRangeTextBox(sender as TextBox, true, (uint)NumberOfChordValues, 0F, float.MaxValue, SetDialogState);
        }
        #endregion Leave handlers
        #endregion

        /// <summary>
        /// Sets the state of the RootInversion, Inversions and VerticalVelocities input fields,
        /// depending on the maximum density in the ChordDensities TextBox.
        /// </summary>
        public void SetChordControls()
        {
            GetMaximumChordDensity();

            if(_maximumChordDensity > 1)
            {
                EnableChordParameters();
            }
            else
            {
                DisableChordParameters();
            }
            RootInversionTextBox_Leave(RootInversionTextBox, null);
            InversionIndicesTextBox_Leave(InversionIndicesTextBox, null);
            VerticalVelocityFactorsTextBox_Leave(VerticalVelocityFactorsTextBox, null);
        }

        public void TouchAllTextBoxes()
        {
            DurationsTextBox_Leave(DurationsTextBox, null);
            VelocitiesTextBox_Leave(VelocitiesTextBox, null);
            MidiPitchesTextBox_Leave(MidiPitchesTextBox, null);
            ChordOffsTextBox_Leave(ChordOffsTextBox, null);
            ChordDensitiesTextBox_Leave(ChordDensitiesTextBox, null);
            SetChordControls();
        }

        private void GetMaximumChordDensity()
        {
            List<int> densities = M.StringToIntList(ChordDensitiesTextBox.Text, ',');

            _maximumChordDensity = 1;
            foreach(int density in densities)
            {
                _maximumChordDensity = _maximumChordDensity > density ? _maximumChordDensity : density;
            }
        }

        private void DisableChordParameters()
        {
            VerticalVelocityFactorsLabel.Enabled = false;
            VerticalVelocityFactorsTextBox.Enabled = false;

            RootInversionLabel.Enabled = false;
            RootInversionTextBox.Enabled = false;

            InversionIndicesLabel.Enabled = false;
            InversionIndicesTextBox.Enabled = false;

            RootInversionTextBox.Text = "";
            InversionIndicesTextBox.Text = "";
            VerticalVelocityFactorsTextBox.Text = "";
        }

        private void EnableChordParameters()
        {
            string countString = _numberOfChordValues.ToString() + " ";
            string integerString = "integer ";
            string floatString = "float ";
            string valStr = (_numberOfChordValues == 1) ? "value " : "values ";
            string rootInvValString = (_maximumChordDensity < 3) ? "value " : "values ";

            string primeIntervalsCountString = (_maximumChordDensity - 1).ToString() + " ";

            #region enable chord parameters
            RootInversionLabel.Enabled = true;
            RootInversionHelpLabel.Enabled = true;
            RootInversionTextBox.Enabled = true;
            RootInversionHelpLabel.Text = primeIntervalsCountString + integerString + rootInvValString + "in range [ 1..127 ]";

            VerticalVelocityFactorsHelpLabel.Text = countString + floatString + valStr + "greater than 0.0";
            VerticalVelocityFactorsLabel.Enabled = true;
            VerticalVelocityFactorsHelpLabel.Enabled = true;
            VerticalVelocityFactorsTextBox.Enabled = true;

            if(_maximumChordDensity > 2)
            {
                string inversionsMaxIndexString = ((2 * (_maximumChordDensity - 2)) - 1).ToString();
                InversionIndicesLabel.Enabled = true;
                InversionIndicesHelpLabel.Enabled = true;
                InversionIndicesTextBox.Enabled = true;
                InversionIndicesHelpLabel.Text = countString + integerString + valStr + "in range [ 0.." + inversionsMaxIndexString + " ]";
            }
            else
            {
                InversionIndicesLabel.Enabled = false;
                InversionIndicesHelpLabel.Enabled = false;
                InversionIndicesTextBox.Enabled = false;
                InversionIndicesHelpLabel.Text = "";
                InversionIndicesTextBox.Text = "";
            }

            #endregion enable chord parameters
        }

        public int NumberOfChordValues
        {
            get { return _numberOfChordValues; }
            set
            {
                _numberOfChordValues = value;
            }
        }
        public int MaximumChordDensity { get { return _maximumChordDensity; } }
        private int _maximumChordDensity = 1;
        private int _numberOfChordValues = -1;
        private SetDialogStateDelegate SetDialogState = null;
    }
}
