using System;
using System.Drawing;
using System.Windows.Forms;

namespace Moritz
{
    public partial class AboutMoritzDialog : Form
    {
        public AboutMoritzDialog()
        {
            InitializeComponent();
            this.BackColor = Color.FromArgb(((int)(((byte)(251)))),
                                            ((int)(((byte)(255)))),
                                            ((int)(((byte)(251)))));
            //        Color_PatchWindowBackground =
            //Color.FromArgb(((int) (((byte) (245)))),
            //               ((int) (((byte) (255)))),
            //               ((int) (((byte) (245)))));
        }

        // To get this function, select everything in the designer, then add a Click event and rename
        // the resulting function. The designer adds the event to all the selected objects!
        // I subsequently removed this event from all the objects which link to websites or send mail.
        private void Anything_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void AboutMoritzDialog_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.DrawRectangle(Pens.DarkGreen, 0, 0, this.Width - 1, this.Height - 1);
        }

        private void JamesIngramLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                WebsiteLinkLabel.LinkVisited = true;
                System.Diagnostics.Process.Start("https://james-ingram-act-two.de/impressum/curriculumVitae.html");
            }
            catch
            {
                string msg = "Unable to open the default email application";
                MessageBox.Show(msg, "error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void LeslieSanfordLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                LeslieSanfordLinkLabel.LinkVisited = true;
                System.Diagnostics.Process.Start("https://github.com/tebjan/Sanford.Multimedia.Midi");
            }
            catch
            {
                string msg = "Unable to open https://github.com/tebjan/Sanford.Multimedia.Midi";
                MessageBox.Show(msg, "error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }

        }

        private void MaxUndMoritzLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                MaxUndMoritzLinkLabel.LinkVisited = true;
                System.Diagnostics.Process.Start("https://en.wikipedia.org/wiki/Max_and_Moritz");
            }
            catch
            {
                string msg = "Unable to open https://en.wikipedia.org/wiki/Max_and_Moritz";
                MessageBox.Show(msg, "error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        /// <summary>
        /// Closes the dialog when it deactivates.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AboutMoritzDialog_Deactivate(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}