using System;
using System.Windows.Forms;
using System.IO;
using Krystals4ObjectLibrary;


namespace Krystals4Application
{
    internal partial class NewModulationDialog : Form
    {
        public NewModulationDialog()
        {
            InitializeComponent();
        }
        #region Events

        private void OpenKrystalButton_Click(object sender, EventArgs e)
        {
            try
            {
                string modulationKrystalFilepath = K.GetFilepathFromOpenFileDialog(K.DialogFilterIndex.modulation);
                if(modulationKrystalFilepath.Length > 0)
                {
                    ModulationKrystal mk = new ModulationKrystal(modulationKrystalFilepath);

                    _modulationKrystal = mk;
                    _xInputFilepath = K.KrystalsFolder + @"\" + mk.XInputFilename;
                    _yInputFilepath = K.KrystalsFolder + @"\" + mk.YInputFilename;
                    _modulatorFilepath = K.ModulationOperatorsFolder + @"\" + mk.ModulatorFilename;
                    XInputFilenameLabel.Text = mk.XInputFilename;
                    YInputFilenameLabel.Text = mk.YInputFilename;
                    ModulatorFilenameLabel.Text = mk.ModulatorFilename;
                }
            }
            catch(ApplicationException ae)
            {
                MessageBox.Show(ae.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
         }

        private void SetXInputButton_Click(object sender, EventArgs e)
        {
            _modulationKrystal = null;
            _xInputFilepath = K.GetFilepathFromOpenFileDialog(K.DialogFilterIndex.allKrystals);
            XInputFilenameLabel.Text = Path.GetFileName(_xInputFilepath);
        }
        private void SetYInputButton_Click(object sender, EventArgs e)
        {
            _modulationKrystal = null;
            _yInputFilepath = K.GetFilepathFromOpenFileDialog(K.DialogFilterIndex.allKrystals);
            YInputFilenameLabel.Text = Path.GetFileName(_yInputFilepath);
        }
        private void SetModulatorButton_Click(object sender, EventArgs e)
        {
            _modulationKrystal = null;
            _modulatorFilepath = K.GetFilepathFromOpenFileDialog(K.DialogFilterIndex.modulator);
            ModulatorFilenameLabel.Text = Path.GetFileName(_modulatorFilepath);
        }
        private void CancelBtn_Click(object sender, EventArgs e)
        {
            _modulationKrystal = null;
            _xInputFilepath = "";
            _yInputFilepath = "";
            _modulatorFilepath = "";
            Close();
        }
        private void OKBtn_Click(object sender, EventArgs e)
        {
			Close();
        }
        #endregion Events
        #region Properties
        public ModulationKrystal ModulationKrystal { get { return _modulationKrystal; } }
        public string XInputFilepath { get { return _xInputFilepath; } }
        public string YInputFilepath { get { return _yInputFilepath; } }
        public string ModulatorFilepath { get { return _modulatorFilepath; } }

        #endregion Properties
        #region private variables
        private ModulationKrystal _modulationKrystal = null;
        private string _xInputFilepath;
        private string _yInputFilepath;
        private string _modulatorFilepath;
        #endregion private variables

    }
}