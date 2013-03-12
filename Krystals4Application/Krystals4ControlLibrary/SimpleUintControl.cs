using System;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Krystals4ControlLibrary
{
    public class SimpleUIntControl : TextBox
    {
        public SimpleUIntControl()
            : base()
        {
            base.BorderStyle = BorderStyle.None;
            base.BackColor = Color.White;
            base.Bounds = new Rectangle(0, 0, 30, 16); // default values (ignored by containing Table)
            base.TextAlign = HorizontalAlignment.Right;

            base.Dock = DockStyle.Fill;
            base.Margin = new Padding(2, 2, 3, 3);

            base.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(TextBox_PreviewKeyDown);
            base.Leave += new System.EventHandler(TextBox_Leave);
        }
        #region Events
        #region Delegates
        #region delegate to tell the container that this control's value has changed
        public delegate void SimpleUintControlValueChanged();
        public SimpleUintControlValueChanged ValueHasChanged;
        #endregion this control's value has changed
        //#region delegate to tell the container that the return key has been pressed
        //public delegate void SimpleUintControlReturnKeyHandler();
        //public SimpleUintControlReturnKeyHandler ReturnKeyPressed;
        //#endregion delegate to tell the container that the return key has been pressed
        #region delegate for sending the container messages about special key presses
        public delegate void SimpleUintControlEventHandler(object sender, SUICEventArgs e);
        public SimpleUintControlEventHandler EventHandler;
        #endregion delegate for sending the container messages about special key presses

        #endregion Delegates
        /// <summary>
        /// Checks that base.Text is either empty or represents a valid unsigned integer, containing only numerals.
        /// If the textbox contains illegal character(s):
        ///  1. a warning message is displayed
        ///  2. any illegal characters are removed from the textBox
        ///  3. the textbox is reselected
        /// Leading zeros are silently removed. A single zero is valid, as is an empty value.
        /// When this function returns, the value is available as the property:
        ///  string ValueString
        /// Calls a delegate to tell the container of this control that its value has changed.
        /// </summary>
        /// <param name="textBox"></param>
        /// <returns></returns>
        private void TextBox_Leave(object sender, EventArgs e)
        {
            TextBox tb = sender as TextBox;
            _illegalCharacter = false;
            if (tb.Text.Length > 0)
            {
                _uInt.Append(tb.Text);
                RemoveIllegalCharacters();
                tb.Text = _uInt.ToString();
                _uInt.Remove(0, _uInt.Length);
            }

            if (_illegalCharacter)
            {
                string msg = "The cell contained illegal characters, which have now been removed.";
                MessageBox.Show(msg, "Value error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                tb.BringToFront();
            }

            if (EventHandler != null)
                EventHandler(this, new SUICEventArgs(SUICMessage.ValueChanged)); // delegate
        }
        /// <summary>
        /// Calls a delegate to tell the container of this control of any special
        /// keys that have been pressed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBox_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            TextBox_Leave(sender, e);
            if (EventHandler != null)
                switch (e.KeyData)
                {
                    case Keys.Return:
                        EventHandler(this, new SUICEventArgs(SUICMessage.Return)); // delegate
                        break;
                    case Keys.Up:
                        EventHandler(this, new SUICEventArgs(SUICMessage.Up)); // delegate
                        break;
                    case Keys.Down:
                        EventHandler(this, new SUICEventArgs(SUICMessage.Down)); // delegate
                        break;
                    case Keys.Left:
                        EventHandler(this, new SUICEventArgs(SUICMessage.Left)); // delegate
                        break;
                    case Keys.Right:
                        EventHandler(this, new SUICEventArgs(SUICMessage.Right)); // delegate
                        break;
                    case (Keys.MButton | Keys.Space):
                        EventHandler(this, new SUICEventArgs(SUICMessage.Pos1)); // delegate
                        break;
                }
        }
        #endregion Events
        #region Properties
        public string ValueString
        {
            get { return base.Text; }
            set
            { 
                base.Text = value;
                TextBox_Leave(this, new EventArgs());
            }
        }
        #endregion Properties
        #region private functions
        private void RemoveIllegalCharacters()
        {
            StringBuilder tempUInt = new StringBuilder();
            for (int i = 0; i < _uInt.Length; i++)
            {
                if (Char.IsNumber(_uInt[i]))
                    tempUInt.Append(_uInt[i]);
                else _illegalCharacter = true;
            }
            int j = 0;
            _uInt.Remove(0, _uInt.Length);
            while (tempUInt.Length > j && tempUInt[j] == '0') // remove leading zeros
                j++;
            if (j == tempUInt.Length)
                _uInt.Append("0");
            else
            while (j < tempUInt.Length)
                _uInt.Append(tempUInt[j++]);
        }
        #endregion private functions
        #region private variables
        private bool _illegalCharacter;
        private StringBuilder _uInt = new StringBuilder();
        #endregion private variables
    }
    public enum SUICMessage { ValueChanged, Return, Up, Down, Left, Right, Pos1 };
    public class SUICEventArgs : EventArgs
    {
        public SUICEventArgs(SUICMessage m)
        {
            Message = m;
        }
        public SUICMessage Message;
    }


}
