using Krystals5ControlLibrary;

using System.Windows.Forms;

namespace Krystals5Application
{
    internal partial class NewPlanetDialog : Form
    {
        public NewPlanetDialog()
        {
            InitializeComponent();
            StartMomentIntSeqControl.updateContainer += new UnsignedIntSeqControl.UnsignedIntSeqControlReturnKeyHandler(HandleReturnKeyInUIntSeqControl);
        }
        #region Delegate
        private void HandleReturnKeyInUIntSeqControl()
        {
            this.DialogResult = DialogResult.OK;
        }
        #endregion Delegate
    }
}