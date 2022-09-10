using System;
using System.Text;
using System.Windows.Forms;

namespace Krystals5Application
{
    internal partial class NewConstantKrystalDialog : Form
    {
        public NewConstantKrystalDialog()
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
            this.CancelBtn.Enabled = true;
            this.CancelBtn.Text = "Close";
        }
        #endregion public functions
        #region Events
        private void OKBtn_Click(object sender, EventArgs e)
        {
            _constantKrystalValue = UnsignedIntControl1.UnsignedInteger.ToString();
            Close();
        }
        private void CancelBtn_Click(object sender, EventArgs e)
        {
            Close();
        }
        #endregion Events
        #region Properties
        public string ConstantKrystalValue
        { 
            get { return _constantKrystalValue; }
            set 
            {
                UnsignedIntControl1.UnsignedInteger = new StringBuilder(value);
                _constantKrystalValue = UnsignedIntControl1.UnsignedInteger.ToString();
            }
        }
        #endregion Properties
        #region private variables
        private string _constantKrystalValue;
        #endregion private variables
    }
}