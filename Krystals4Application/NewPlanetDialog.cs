using System.Windows.Forms;
using Krystals4ControlLibrary;

namespace Krystals4Application
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