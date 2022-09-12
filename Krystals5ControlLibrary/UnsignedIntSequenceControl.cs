using System;
using System.Text;
using System.Windows.Forms;

namespace Krystals5ControlLibrary
{
    public partial class UnsignedIntSeqControl : UserControl
    {
        public UnsignedIntSeqControl()
        {
            InitializeComponent();
        }
        #region Events
        /// <summary>
        /// Checks that textbox.text is a valid krystal line sequence, containing only numerals and spaces.
        /// If the textbox contains illegal character(s):
        ///  1. a warning message is displayed
        ///  2. any illegal characters are removed from the textBox
        ///  3. the cursor is replaced in the textbox
        /// If the textbox contains redundant spaces (multiple spaces or spaces at the beginning and end of
        /// the sequence), these are silently removed.
        /// When this function returns, the Sequence property contains a valid sequence of unsigned
        /// integers. The Sequence may be empty - having Length == 0.
        /// </summary>
        /// <param name="textBox"></param>
        /// <returns></returns>
        private void UIntSeqTextBox_Leave(object sender, EventArgs e)
        {
            TextBox tb = sender as TextBox;
            _illegalCharacter = false;
            if(tb.Text.Length > 0)
            {
                _sequence.Append(tb.Text);
                RemoveIllegalCharactersAndRedundantSpacesFromSequence();
                tb.Text = _sequence.ToString();
                _sequence.Remove(0, _sequence.Length);
            }

            if(_illegalCharacter)
            {
                tb.Text = _sequence.ToString();
                string msg = "The sequence contained illegal characters, which have now been removed.";
                MessageBox.Show(msg, "Sequence error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                tb.BringToFront();
            }
        }
        #region Delegate for handling return key
        public delegate void UnsignedIntSeqControlReturnKeyHandler();
        public UnsignedIntSeqControlReturnKeyHandler updateContainer;
        private void UIntSeqTextBox_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if(e.KeyData == Keys.Return || e.KeyData == Keys.Enter)
            {
                UIntSeqTextBox_Leave(sender, e);
                if(updateContainer != null)
                    updateContainer(); // calls an event handler in the containing object
            }

        }
        #endregion Delegate for handling return key
        #endregion Events
        #region Properties
        /// <summary>
        /// The sequence of unsigned integers separated by single spaces. The sequence may be empty ("").
        /// </summary>
        public StringBuilder Sequence
        {
            get { return new StringBuilder(UIntSeqTextBox.Text); }
            set { UIntSeqTextBox.Text = value.ToString(); }
        }
        #endregion Properties
        #region private function
        private void RemoveIllegalCharactersAndRedundantSpacesFromSequence()
        {
            StringBuilder tempSeq = new StringBuilder();
            char c;
            for(int i = 0; i < _sequence.Length; i++)
            {
                c = _sequence[i];
                if(Char.IsNumber(c) || c.Equals(" "[0]))
                    tempSeq.Append(c);
                else _illegalCharacter = true;
            }

            if(tempSeq.Length > 0)
                while(tempSeq[0] == " "[0])
                    tempSeq.Remove(0, 1);
            if(tempSeq.Length > 0)
                while(tempSeq[tempSeq.Length - 1] == " "[0])
                    tempSeq.Remove(tempSeq.Length - 1, 1);

            if(tempSeq.Length < 2)
                _sequence = tempSeq;
            else // remove multiple spaces
            {
                _sequence.Length = 0;
                _sequence.Append(tempSeq[0]);
                for(int ti = 1, si = 0; ti < tempSeq.Length; ti++)
                {
                    if((_sequence[si] != " "[0])
                    || (_sequence[si] == " "[0] && tempSeq[ti] != " "[0]))
                    {
                        _sequence.Append(tempSeq[ti]);
                        si++;
                    }
                }
            }
        }
        #endregion private function
        #region private variables
        private StringBuilder _sequence = new StringBuilder();
        private bool _illegalCharacter;
        #endregion private variables
    }
}
