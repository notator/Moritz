using System;
using System.Windows.Forms;
using System.IO;

using Krystals4ObjectLibrary;
using Moritz.Globals; // Preferences
using Moritz.Krystals; //KrystalBrowser

namespace Krystals4Application
{
    internal partial class NewExpansionDialog : Form
    {
        public NewExpansionDialog()
        {
            InitializeComponent();
            _krystalsFolder = M.Preferences.LocalMoritzKrystalsFolder;
        }
        #region Events
        private void SetDensityInputButton_Click(object sender, EventArgs e)
        { 
            KrystalBrowser krystalBrowser = new KrystalBrowser(null, _krystalsFolder, SetDensityInputKrystal);
            krystalBrowser.Show();
        }
        private void SetDensityInputKrystal(Krystal krystal)
        {
            DensityInputFilenameLabel.Text = krystal.Name;
            _densityInputFilepath = _krystalsFolder + @"\" + krystal.Name;
        }

        private void SetPointsInputButton_Click(object sender, EventArgs e)
        {
            KrystalBrowser krystalBrowser = new KrystalBrowser(null, _krystalsFolder, SetPointsInputKrystal);
            krystalBrowser.Show();
        }
        private void SetPointsInputKrystal(Krystal krystal)
        {
            PointsInputFilenameLabel.Text = krystal.Name;
            _pointsInputFilepath = _krystalsFolder + @"\" + krystal.Name;
        }

        private void SetExpanderButton_Click(object sender, EventArgs e)
        {
            _expanderFilepath = K.GetFilepathFromOpenFileDialog(K.DialogFilterIndex.expander);
            ExpanderFilenameLabel.Text = Path.GetFileName(_expanderFilepath);
        } 
        private void CancelBtn_Click(object sender, EventArgs e)
        {
            _densityInputFilepath = "";
            _pointsInputFilepath = "";
            _expanderFilepath = "";
            Close();
        }
        private void OKBtn_Click(object sender, EventArgs e)
        {
            Close();
        }
        #endregion Events
        #region Properties
        public string DensityInputFilepath { get { return _densityInputFilepath; } }
        public string PointsInputFilepath { get { return _pointsInputFilepath; } }
        public string ExpanderFilepath { get { return _expanderFilepath; } }
        #endregion Properties
        #region private variables
        private string _densityInputFilepath;
        private string _pointsInputFilepath;
        private string _expanderFilepath;
        private string _krystalsFolder;
        #endregion private variables

    }
}