using System;
using System.Text;
using System.Windows.Forms;

namespace Krystals4ControlLibrary
{
    internal partial class FloatControl : UserControl
    {
        public FloatControl()
        {
            InitializeComponent();
            _float = new StringBuilder();
        }
        #region Events
        /// <summary>
        /// Checks that textbox.text represents a valid float, containing only 
        ///     1. numerals
        ///     2. an optional prefixed minus sign
        ///     3. an optional infixed decimal separator
        /// If the textbox contains illegal character(s):
        ///  1. a warning message is displayed
        ///  2. any illegal characters are removed from the textBox
        ///  3. the textbox is reselected
        /// When this function returns, the Float property either contains a valid float or has Length = 0.
        /// </summary>
        /// <param name="textBox"></param>
        /// <returns></returns>
        private void FloatTextBox_Leave(object sender, EventArgs e)
        {
            TextBox tb = sender as TextBox;
            _illegalCharacter = false;
            if (tb.Text.Length > 0)
            {
                _float.Append(tb.Text);
                ValidateFloat();
                tb.Text = _float.ToString();
                _float.Remove(0, _float.Length);
            }

            if(_illegalCharacter)
            {
                string msg = "The float value contained illegal characters, which have now been removed.";
                MessageBox.Show(msg, "Value error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                tb.BringToFront();
            }
        }
        #region use of delegate to handle return key
        public delegate void FloatControlReturnKeyHandler();
        public FloatControlReturnKeyHandler updateContainer;
        private void FloatTextBox_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyData == Keys.Return || e.KeyData == Keys.Enter)
            {
                FloatTextBox_Leave(sender, e);
                if (updateContainer != null)
                    updateContainer();
            }
        }
        #endregion
        #endregion Events
        #region Properties
        public StringBuilder Float
        {
            get { return new StringBuilder(FloatTextBox.Text); }
            set { FloatTextBox.Text = value.ToString(); } // used to set the box to "0" when empty
        }

		public float Value
		{
			get { return float.Parse(FloatTextBox.Text); }
			set { FloatTextBox.Text = value.ToString(); }
		}
        #endregion Properties
        #region private functions
        private void ValidateFloat()
        {
            StringBuilder tempFloat = new StringBuilder();
            bool decimalSeparatorFound = false;
            bool numeralFound = false;

            int i = 0;
            while( i < _float.Length && _float[i] == " "[0] ) // ignore leading spaces
                i++;
            while( i < _float.Length && _float[i] == "0"[0] ) // ignore leading zeros
                i++;

            if (_float.Length > (i+1) && _float[i] == '-')
            {
                tempFloat.Append('-');
                i++;
            }

            while( i < _float.Length )
            {
                if (_float[i] == '.' || _float[i] == ',')
                {
                    if (!decimalSeparatorFound)
                    {
                        tempFloat.Append(_localDecimalSeparator);
                        decimalSeparatorFound = true;
                    }
                    else _illegalCharacter = true;
                }
                else if( Char.IsDigit(_float[i]))
                {
                    tempFloat.Append(_float[i]);
                    numeralFound = true;
                }
                else _illegalCharacter = true;
                i++;
            }

            if (numeralFound)
            {
                if (tempFloat[0] == _localDecimalSeparator[0])
                    tempFloat.Insert(0, "0");
                if (tempFloat[tempFloat.Length - 1] == _localDecimalSeparator[0])
                    tempFloat.Append("0");
            }
            if (tempFloat.Length > 0)
                _float = tempFloat;
            else _float = new StringBuilder("0");
        }
        #endregion private functions
        #region private variables
        private StringBuilder _float;
        private bool _illegalCharacter;
        private readonly string _localDecimalSeparator = System.Globalization.NumberFormatInfo.CurrentInfo.NumberDecimalSeparator;
        #endregion private variables
    }
}
