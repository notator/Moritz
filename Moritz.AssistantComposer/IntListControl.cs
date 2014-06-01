using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Xml;
using System.Diagnostics;
using System.Text;

using Moritz.Globals;

namespace Moritz.AssistantComposer
{
    public partial class IntListControl : UserControl
    {
        public IntListControl(int x, int y, int textBoxWidth, int minInt, int maxInt, int nBoxes, ControlHasChangedDelegate controlHasChanged)
        {
            InitializeComponent();

            this.Location = new Point(x, y);
            _minInt = minInt;
            _maxInt = maxInt;

            _ControlHasChanged = controlHasChanged;

            Init(nBoxes, textBoxWidth);
        }

        public bool HasError()
        {
            bool rval = false;
            for(int i = 0; i < _boxes.Count; ++i)
            {
                if(_boxes[i].BackColor == M.TextBoxErrorColor)
                {
                    rval = true;
                    break;
                }
            }
            return rval;
        }

        /// <summary>
        /// If all boxes have the default intValue, they are emptied.
        /// Otherwise, all empty boxes are set to the default intValue.
        /// The text of boxes having the default value is coloured RoyalBlue,
        /// other boxes' text is coloured Black.
        /// </summary>
        /// <param name="intVal"></param>
        public void SetColoredDefaultTexts(int intVal)
        {
            string intStr = intVal.ToString();

            bool allDefaults = true;

            foreach(TextBox box in _boxes)
            {
                if(box.Text.CompareTo(intStr) != 0 && box.Text.CompareTo("") != 0)
                {
                    allDefaults = false;
                    break;
                }
            }

            if(allDefaults)
            {
                foreach(TextBox box in _boxes)
                {
                    box.Text = "";
                }
            }
            else
            {
                foreach(TextBox box in _boxes)
                {
                    if(box.Text.Length == 0)
                    {
                        box.Text = intStr;
                    }

                    if(box.Text.CompareTo(intStr) == 0)
                    {
                        box.ForeColor = Color.RoyalBlue;
                    }
                    else
                    {
                        box.ForeColor = Color.Black;
                    }
                }
            }
        }

        /// <summary>
        /// The attributeString contains a comma-delimited list of values.
        /// There must be the same number of values as there are boxes in this control.
        /// </summary>
        /// <param name="attributeString"></param>
        public void Set(string attributeString)
        { 
            string[] values = attributeString.Split(',');
            Debug.Assert(values.Length == _boxes.Count);
            for(int i = 0; i < _boxes.Count; ++i)
            {
                _boxes[i].Text = values[i];
            }
        }

        /// <summary>
        /// The comma-separated list of values, that can be used as an attribute.
        /// </summary>
        /// <returns></returns>
        public string ValuesAsString()
        {
            StringBuilder sb = new StringBuilder();
            for(int i = 0; i < _boxes.Count; ++i)
            {
                sb.Append(_boxes[i].Text);
                sb.Append(',');
            }
            sb.Remove(sb.Length - 1, 1);
            return sb.ToString();
        }

        private void Init(int nBoxes, int textBoxWidth)
        {
            int x = 0;
            this.SuspendLayout();
            for(int i = 0; i < nBoxes; ++i)
            {
                TextBox textBox = new TextBox();
                textBox.Size = new Size(textBoxWidth, 20);
                textBox.Location = new Point(x, 0);
                textBox.Visible = true;
                textBox.TabIndex = i;
                x += (textBoxWidth + 1);
                textBox.Enter += TextBox_Enter;
                textBox.Leave += TextBox_Leave;
                _boxes.Add(textBox);
                this.Controls.Add(textBox);
            }
            this.Size = new Size(x, 20);
            this.ResumeLayout();
            for(int i = 0; i < nBoxes; ++i)
            {
                _boxes[i].Show();
            }
        }

        private void SetTextBoxState(TextBox textBox, bool okay)
        {
            if(okay)
                textBox.BackColor = Color.White;
            else
                textBox.BackColor = M.TextBoxErrorColor;
        }

        private void TextBox_Leave(object sender, EventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if(textBox != null)
            {
                textBox.Text = textBox.Text.Trim();
                M.LeaveIntRangeTextBox(textBox, true, 1, _minInt, _maxInt, SetTextBoxState);

                if(textBox.BackColor == Color.White && _ControlHasChanged != null)
                {
                    _ControlHasChanged(this); // delegate
                }
            }
        }

        private void TextBox_Enter(object sender, EventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if(textBox != null)
                textBox.BackColor = Color.White;
            textBox.ForeColor = Color.Black;
        }

        public bool IsEmpty()
        {
            bool isEmpty = true;
            foreach(TextBox tb in _boxes)
            {
                if(tb.Text != "")
                {
                    isEmpty = false;
                    break;
                }
            }
            return isEmpty;
        }

        private int _minInt;
        private int _maxInt;
        private ControlHasChangedDelegate _ControlHasChanged = null;
        private List<TextBox> _boxes = new List<TextBox>();
    }
}
