using System;
using System.Drawing;
using System.Windows.Forms;

namespace Krystals5Application
{
    internal partial class AboutKrystals5 : Form
    {
        public AboutKrystals5()
        {
            InitializeComponent();
        }

        private void AboutKrystals5_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.DrawRectangle(new Pen(Brushes.Blue), 1, 1, this.Width - 2, this.Height - 2);
        }

        private void AboutKrystals5_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}