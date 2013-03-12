using System;
using System.Drawing;
using System.Windows.Forms;

namespace Krystals4Application
{
    internal partial class AboutKrystals4 : Form
    {
        public AboutKrystals4()
        {
            InitializeComponent();
        }

        private void AboutKrystals4_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.DrawRectangle(new Pen(Brushes.Black),1,1,this.Width-2,this.Height-2);
        }

        private void AboutKrystals4_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}