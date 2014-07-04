using System;
using System.Windows.Forms;

namespace Moritz
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            try
            {
                Application.Run(new MoritzForm1());
            }
            catch 
            {
                MessageBox.Show("No available MIDI output devices.\nThey are probably being used by another program.",
                    "Device Error", MessageBoxButtons.OK);
            }
        }
    }
}
