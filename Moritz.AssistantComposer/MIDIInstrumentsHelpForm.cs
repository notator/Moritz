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
    internal partial class MIDIInstrumentsHelpForm : Form
    {
        public MIDIInstrumentsHelpForm(CloseMIDIHelpFormDelegate CloseMIDIInstrumentsHelpForm)
        {
            InitializeComponent();
            closeMIDIInstrumentsHelpForm = CloseMIDIInstrumentsHelpForm;
        }

        private void CloseButton_Click(object sender, EventArgs e)
        {
            closeMIDIInstrumentsHelpForm();
        }

        private CloseMIDIHelpFormDelegate closeMIDIInstrumentsHelpForm;
    }
}
