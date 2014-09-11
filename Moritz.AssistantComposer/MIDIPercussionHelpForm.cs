using System;
using System.Windows.Forms;

namespace Moritz.AssistantComposer
{
    internal partial class MIDIPercussionHelpForm : Form
    {
        public MIDIPercussionHelpForm(CloseMIDIHelpFormDelegate CloseMIDIPercussionHelpForm)
        {
            InitializeComponent();
            closeMIDIPercussionHelpForm = CloseMIDIPercussionHelpForm;
        }

        private void CloseButton_Click(object sender, EventArgs e)
        {
            closeMIDIPercussionHelpForm();
        }

        private CloseMIDIHelpFormDelegate closeMIDIPercussionHelpForm;
    }
}
