using System;
using System.Text;
using System.Windows.Forms;

namespace Krystals5Application
{
    internal partial class NewLineKrystalDialog : Form
    {
        public NewLineKrystalDialog()
        {
            InitializeComponent();
        }
        #region public functions
        /// <summary>
        /// This function is called when the dialog is simply being used to examine an existing krystal.
        /// </summary>
        public void SetButtons()
        {
            this.OKBtn.Hide();
            this.CancelBtn.Text = "Close";
        }
        #endregion public functions
        #region Events
        private void OKBtn_Click(object sender, EventArgs e)
        {
            _lineKrystalValue = UnsignedIntSequenceControl1.Sequence.ToString();
            Close();
        }
        private void CancelBtn_Click(object sender, EventArgs e)
        {
            Close();
        }
        #endregion Events
        #region Properties
        /// <summary>
        /// The set properties are used when the dialog is being used to examine an existing krystal.
        /// </summary>
        public string LineKrystalValue
        {
            get { return _lineKrystalValue; }
            set
            {
                _lineKrystalValue = value;
                UnsignedIntSequenceControl1.Sequence = new StringBuilder(value);
            }
        }
        #endregion Properties
        #region private variables
        private string _lineKrystalValue;
        #endregion private variables
    }
}