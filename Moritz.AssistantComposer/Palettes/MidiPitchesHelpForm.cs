using System;
using System.Windows.Forms;

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
