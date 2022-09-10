using System;
using System.Text;
using System.Windows.Forms;

namespace Krystals5ControlLibrary
{
    internal partial class UnsignedIntControl : UserControl
    {
        public UnsignedIntControl()
        {
            InitializeComponent();
        }
        #region Events
        /// <summary>
        /// Checks that textbox.text represents a valid unsigned integer, containing only numerals.
        /// If the textbox is empty, or contains illegal character(s):
        ///  1. a warning message is displayed
        ///  2. any illegal characters are removed from the textBox
        ///  3. the textbox is reselected
        /// When this function returns, a StringBuilder containing the valid unsigned integer
        /// can be retrieved using the UnsignedInteger property.
        /// </summary>
        /// <param name="textBox"></param>
        /// <returns></returns>
        private void UIntTextBox_Leave(object sender, EventArgs e)
        {
            TextBox tb = sender as TextBox;
            _illegalCharacter = false;
            if(tb.Text.Length > 0)
            {
                _uInt.Append(tb.Text);
                RemoveIllegalCharacters();
                tb.Text = _uInt.ToString();
                _uInt.Remove(0, _uInt.Length);
            }

            if(_illegalCharacter)
            {
                string msg = "The constant value contained illegal characters, which have now been removed.";
                MessageBox.Show(msg, "Value error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                tb.BringToFront();
            }
        }
        #region Delegate for handling return key
        public delegate void UintControlReturnKeyHandler();
        public UintControlReturnKeyHandler updateContainer;
        private void UIntTextBox_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if(e.KeyData == Keys.Return || e.KeyData == Keys.Enter)
            {
                UIntTextBox_Leave(sender, e);
                if(updateContainer != null)
                    updateContainer(); // tells the containing object to update
            }
        }
        #endregion Delegate for handling return key
        #endregion Events
        #region Properties
        public StringBuilder UnsignedInteger
        {
            get { return new StringBuilder(UIntTextBox.Text); }
            set { UIntTextBox.Text = value.ToString(); } // used by PointGroupParameters to set a value of zero in an empty box
        }

        public uint Value
        {
            get { return uint.Parse(UIntTextBox.Text); }
            set { UIntTextBox.Text = value.ToString(); }
        }
        #endregion Properties
        #region private functions
        private void RemoveIllegalCharacters()
        {
            StringBuilder tempUInt = new StringBuilder();
            for(int i = 0; i < _uInt.Length; i++)
            {
                if(Char.IsNumber(_uInt[i]))
                    tempUInt.Append(_uInt[i]);
                else _illegalCharacter = true;
            }
            int j = 0;
            _uInt.Remove(0, _uInt.Length);
            while(tempUInt.Length > j && tempUInt[j] == "0"[0]) // remove leading zeros
                j++;
            while(j < tempUInt.Length)
            {
                _uInt.Append(tempUInt[j]);
                j++;
            }
        }
        #endregion private functions
        #region private variables
        private bool _illegalCharacter;
        private StringBuilder _uInt = new StringBuilder();
        #endregion private variables
    }
}
