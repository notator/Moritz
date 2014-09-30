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
                MessageBox.Show("Moritz threw an unhandled exception.",
                    "Error", MessageBoxButtons.OK);
            }
        }
    }
}
