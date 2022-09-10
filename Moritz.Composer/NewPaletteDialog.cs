using Moritz.Globals;

using System;
using System.Windows.Forms;

namespace Moritz.Composer
{
    public partial class NewPaletteDialog : Form
    {
        public NewPaletteDialog()
        {
            InitializeComponent();
            OK_Button.Enabled = false;
        }

        private void NameTextBox_Leave(object sender, EventArgs e)
        {
            NameTextBox.Text = NameTextBox.Text.Trim();
            if(!String.IsNullOrEmpty(NameTextBox.Text)
            && DomainTextBox.BackColor != M.TextBoxErrorColor
            && !String.IsNullOrEmpty(DomainTextBox.Text))
            {
                OK_Button.Enabled = true;
                OK_Button.Focus();
            }
        }

        /// <summary>
        /// Palette domains are limited to 12 because that seems adequate.
        /// The domain is actually only limited by the width of palette forms.
        /// (There has to be enough space for the demo buttons.) 
        /// </summary>
        private void DomainTextBox_Leave(object sender, EventArgs e)
        {
            M.LeaveIntRangeTextBox(DomainTextBox, false, 1, 0, 12, SetDialogState);
            if(DomainTextBox.BackColor != M.TextBoxErrorColor)
            {
                _domain = int.Parse(DomainTextBox.Text);
                if(!String.IsNullOrEmpty(NameTextBox.Text))
                {
                    OK_Button.Enabled = true;
                    OK_Button.Focus();
                }
            }
            else
            {
                OK_Button.Enabled = false;
            }
        }

        private void SetDialogState(TextBox textBox, bool okay)
        {
            M.SetTextBoxErrorColorIfNotOkay(textBox, okay);
        }

        private void OKButton_Click(object sender, EventArgs e)
        {
            PaletteName = NameTextBox.Text;
            PaletteDomain = _domain;
        }

        private int _domain;

        public string PaletteName = null;
        public int PaletteDomain = -1;


    }
}
