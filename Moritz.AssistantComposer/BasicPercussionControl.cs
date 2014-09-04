using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Diagnostics;

using Krystals4ObjectLibrary;
using Moritz.Globals;
using Moritz.Krystals;
using Moritz.Score;
using Moritz.Score.Midi;
using Moritz.Score.Notation;
using Moritz.AssistantPerformer;

namespace Moritz.AssistantComposer
{
    public partial class BasicPercussionControl : UserControl
    {
        public BasicPercussionControl(SetDialogStateDelegate setDialogState)
        {
            InitializeComponent();
            //SetLabelPositions();
            SetDialogState = setDialogState;
        }

        public void ReadBasicChordControl(XmlReader r)
        {
            Debug.Assert(r.Name == "basicChord" && r.IsStartElement());
            M.ReadToXmlElementTag(r, "durations", "velocities", "midiPitches");
            while(r.Name == "durations" || r.Name == "velocities"|| r.Name == "midiPitches" )
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
                    }
                }
                M.ReadToXmlElementTag(r, 
                    "basicChord", "durations", "velocities", "midiPitches");
            }
            Debug.Assert(r.Name == "basicChord"); // end element
        }

        public void WriteBasicPercussionControl(XmlWriter w)
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
            w.WriteEndElement();
        }

        public void SetHelpLabels(int numberOfChordValues)
        {
            _numberOfChordValues = numberOfChordValues;
            if(numberOfChordValues == -1)
            {
                DurationsHelpLabel.Text = "";
                MidiPitchesHelpLabel.Text = "";
                VelocitiesHelpLabel.Text = "";
            }
            else
            {
                DurationsHelpLabel.Text = numberOfChordValues.ToString() + " integer values (greater than 0)";
                MidiPitchesHelpLabel.Text = numberOfChordValues.ToString() + " integer values in range [ 0..127 ]";
                VelocitiesHelpLabel.Text = numberOfChordValues.ToString() + " integer values in range [ 0..127 ]";
            }
        }

        public void SetToWhite(TextBox textBox)
        {
            if(textBox != null)
            {
                textBox.ForeColor = Color.Black;
                textBox.BackColor = Color.White;
            }
        }

        #region Event handlers
        /// <summary>
        /// Used by all parameter textBoxes (also the ones in the containing Form).
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ParameterTextBox_TextChanged(object sender, EventArgs e)
        {
            TextBox textBox = sender as TextBox;
            SetToWhite(textBox);
        }

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
            M.LeaveIntRangeTextBox(sender as TextBox, false, (uint)NumberOfChordValues, 35, 81, SetDialogState);
        }
        #endregion
        public void TouchAllTextBoxes()
        {
            DurationsTextBox_Leave(DurationsTextBox, null);
            VelocitiesTextBox_Leave(VelocitiesTextBox, null);
            MidiPitchesTextBox_Leave(MidiPitchesTextBox, null);
        }

        public int NumberOfChordValues 
        { 
            get { return _numberOfChordValues; } 
            set 
            { 
                _numberOfChordValues = value;
                SetHelpLabels(_numberOfChordValues);
            }
        }

        private int _numberOfChordValues = 0;
        private SetDialogStateDelegate SetDialogState = null;
    }
}
