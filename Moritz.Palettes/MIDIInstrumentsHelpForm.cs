using System;
using System.Windows.Forms;

namespace Moritz.Palettes
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
