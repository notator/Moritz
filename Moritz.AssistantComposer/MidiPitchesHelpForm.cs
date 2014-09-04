using System;
using System.Windows.Forms;

using Krystals4ObjectLibrary;
using Moritz.Globals;
using Moritz.Krystals;
using Moritz.Score;
using Moritz.Score.Midi;
using Moritz.Score.Notation;
using Moritz.AssistantPerformer;

namespace Moritz.AssistantComposer
{
    internal partial class MidiPitchesHelpForm : Form
    {
        public MidiPitchesHelpForm(CloseMidiPitchesHelpFormDelegate CloseMidiPitchesHelpForm)
        {
            InitializeComponent();
            closeMidiPitchesHelpForm = CloseMidiPitchesHelpForm;
        }

        private void CloseButton_Click(object sender, EventArgs e)
        {
            closeMidiPitchesHelpForm();
        }

        private CloseMidiPitchesHelpFormDelegate closeMidiPitchesHelpForm;
    }
}
