using System;
using System.Text;
using System.Windows.Forms;

namespace Krystals4ControlLibrary
{
    internal partial class KrystalFilenameControl : UserControl
    {
        public KrystalFilenameControl()
        {
            InitializeComponent();
            _filename = new StringBuilder();
        }
        #region Events
        /// <summary>
        /// Checks that textbox.text is a valid krystal filename (without the .krys suffix), containing only numerals,
        /// alphabetic characters and the characters '-' and '_'.
        /// If the textbox contains illegal character(s):
        ///  1. a warning message is displayed
        ///  2. any illegal characters are removed from the textBox
        ///  3. the cursor is replaced in the textbox
        /// When this function returns, the Filename property either return a valid filename or it has Length = 0.
        /// </summary>
        /// <param name="textBox"></param>
        /// <returns></returns>
        private void TextBox_Leave(object sender, EventArgs e)
        {
            TextBox tb = sender as TextBox;
            if( !GetAndValidateKrystalFileName(tb.Text) )
            {
                tb.Text = _filename.ToString();
                string msg = "The file name contained illegal characters, which have now been removed.";
                MessageBox.Show(msg, "File name error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                tb.BringToFront();
            }
        }
        #endregion Events
        #region private functions
        private bool GetAndValidateKrystalFileName(string text)
        {
            bool returnValue = true;
            if( _filename.Length > 0 )
                _filename.Remove(0, _filename.Length);

            foreach( char c in text )
            {
                if( Char.IsLetter(c) || Char.IsNumber(c) || "-_".Contains(c.ToString()) )
                    _filename.Append(c);
                else returnValue = false;
            }

            return returnValue;
        }
        #endregion private functions
        #region Properties
        public StringBuilder Filename
        {
            get { return new StringBuilder(KrystalFilenameTextBox.Text); }
            set { KrystalFilenameTextBox.Text = value.ToString(); }
        }
        #endregion Properties
        #region private variables
        private StringBuilder _filename;
        #endregion private variables
    }
}
