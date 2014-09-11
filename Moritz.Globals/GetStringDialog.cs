using System;
using System.Windows.Forms;

namespace Moritz.Globals
{
    public partial class GetStringDialog : Form
    {
        public GetStringDialog(string title, string instructions)
        {
            InitializeComponent(); 
            this.Text = title;
            this.InstructionLabel.Text = instructions;
        }

        private void OK_Button_Click(object sender, EventArgs e)
        {
            String = this.StringTextBox.Text;
        }

        private void StringTextBox_Leave(object sender, EventArgs e)
        {
            this.OK_Button.Focus();
        }

        public string String = null;

        private void Cancel_Button_Click(object sender, EventArgs e)
        {
            String = "";
        }
    }
}
