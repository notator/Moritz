using Krystals4ObjectLibrary;

using Moritz.Globals; // Preferences
using Moritz.Krystals; //KrystalBrowser

using System;
using System.IO;
using System.Windows.Forms;
using System.Xml;

namespace Krystals4Application
{
    public partial class NewPathExpansionDialog : Form
    {
        public NewPathExpansionDialog()
        {
            InitializeComponent();
            _krystalsFolder = M.LocalMoritzKrystalsFolder;
        }

        #region Events
        private void SetSVGInputButton_Click(object sender, EventArgs e)
        {
            _SVGInputFilepath = K.GetFilepathFromOpenFileDialog(K.DialogFilterIndex.svg);
            SVGInputFilenameLabel.Text = Path.GetFileName(_SVGInputFilepath);
        }
        private void SetDensityInputButton_Click(object sender, EventArgs e)
        {
            KrystalBrowser krystalBrowser = new KrystalBrowser(null, _krystalsFolder, SetDensityInputKrystal);
            krystalBrowser.Show(); // calls SetDensityInputKrystal(krystal) as a delegate
        }

        private void SetDensityInputKrystal(Krystal krystal)
        {
            DensityInputFilenameLabel.Text = krystal.Name;
            _densityInputKrystalFilepath = _krystalsFolder + @"\" + krystal.Name;
        }

        private void CancelBtn_Click(object sender, EventArgs e)
        {
            _densityInputKrystalFilepath = "";
            _SVGInputFilepath = "";
            Close();
        }
        private void OKBtn_Click(object sender, EventArgs e)
        {
            Close();
        }
        #endregion Events

        #region Properties
        public string DensityInputKrystalFilepath { get { return _densityInputKrystalFilepath; } }
        public string TrajectorySVGFilepath { get { return _SVGInputFilepath; } }

        #endregion Properties
        #region private variables
        private string _densityInputKrystalFilepath;
        private string _SVGInputFilepath;

        private string _krystalsFolder;

        #endregion private variables


    }
}
