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
            _densityInputKrystalFilepath = "";
            DensityInputFilenameLabel.Text = "<unassigned>";
            this.OKBtn.Enabled = false;

            _SVGInputFilepath = K.GetFilepathFromOpenFileDialog(K.DialogFilterIndex.svg);
            SVGInputFilenameLabel.Text = Path.GetFileName(_SVGInputFilepath);
        }
        private void SetDensityInputButton_Click(object sender, EventArgs e)
        {
            KrystalBrowser krystalBrowser = new KrystalBrowser("All krystals", SetDensityInputKrystal);
            krystalBrowser.Show(); // calls SetDensityInputKrystal(krystal) as a delegate
        }

        private void SetDensityInputKrystal(Krystal krystal)
        {
            char[] splitChar = { '.' };
            var components = SVGInputFilenameLabel.Text.Split(splitChar);
            int nEffectiveTrajectoryNodes = int.Parse(components[1]); // can be 1 (A constant: the first node in the trajectory)

            if(nEffectiveTrajectoryNodes == 1 || (krystal.Level > 0 && DensityShapeContainsNTrajectoryNodes(nEffectiveTrajectoryNodes, krystal)))
            {
                DensityInputFilenameLabel.Text = krystal.Name;
                _densityInputKrystalFilepath = _krystalsFolder + @"\" + krystal.Name;
                this.OKBtn.Enabled = true;
            }
            else
            {
                MessageBox.Show($"The density input krystal's shape must include the number of (effective) trajectory nodes ({nEffectiveTrajectoryNodes}).", "Warning", MessageBoxButtons.OK);
                KrystalBrowser krystalBrowser = new KrystalBrowser("All krystals", SetDensityInputKrystal);
                krystalBrowser.Show(); // calls SetDensityInputKrystal(krystal) as a delegate
            }
        }

        private bool DensityShapeContainsNTrajectoryNodes(int nTrajectoryNodes, Krystal krystal)
        {
            bool rval = false;
            foreach(int value in krystal.ShapeArray)
            {
                if(nTrajectoryNodes == value)
                {
                    rval = true;
                    break;
                }
            }
            return rval;
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
        private string _densityInputKrystalFilepath = "";
        private string _SVGInputFilepath = "";
        private string _krystalsFolder = "";

        #endregion private variables


    }
}
