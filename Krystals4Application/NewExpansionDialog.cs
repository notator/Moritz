using System;
using System.Windows.Forms;
using System.IO;

using Krystals4ObjectLibrary;
using Moritz.Globals; // Preferences
using Moritz.Krystals; //KrystalBrowser
using System.Collections.Generic;

namespace Krystals4Application
{
    internal partial class NewExpansionDialog : Form
    {
        public NewExpansionDialog()
        {
            InitializeComponent();
            _krystalBrowser = null;
            _krystalsFolder = M.LocalMoritzKrystalsFolder;

            SetDensityInputButton.Enabled = true;
            SetDensityInputButton.Focus();
            SetPointsInputValuesButton.Enabled = false;
            SetExpanderButton.Enabled = false;
            OKBtn.Enabled = false;
            CancelBtn.Enabled = true;
        }
        #region Events
        private void SetDensityInputButton_Click(object sender, EventArgs e)
        {
            if(_krystalBrowser != null)
            {
                _krystalBrowser.Close();
            }
            _krystalBrowser = new KrystalsBrowser("All krystals: (Get Density Input)", SetDensityInputKrystal); // TODO: set filter to show all files
            _krystalBrowser.Show();
        }
        private void SetDensityInputKrystal(Krystal krystal)
        {
            DensityInputFilenameLabel.Text = krystal.Name;
            _densityInputFilepath = _krystalsFolder + @"\" + krystal.Name;
            if(_krystalBrowser != null)
            {
                _krystalBrowser.Close();
            }

            SetDensityInputButton.Enabled = false;
            SetExpanderButton.Enabled = true;
            SetExpanderButton.Focus();
        }

        private void SetExpanderButton_Click(object sender, EventArgs e)
        {
            _expanderFilepath = K.GetFilepathFromOpenFileDialog(K.DialogFilterIndex.expander);
            if(_expanderFilepath != "")
            {
                ExpanderFilenameLabel.Text = Path.GetFileName(_expanderFilepath);

                SetExpanderButton.Enabled = false;
                SetPointsInputValuesButton.Enabled = true;
                SetPointsInputValuesButton.Focus();
            }
        }

        private void SetPointsInputButton_Click(object sender, EventArgs e)
        {
            char[] dot = new char[] { '.' };
            var densityFilename = Path.GetFileName(_densityInputFilepath);
            var expanderFilename = Path.GetFileName(_expanderFilepath);
            
            int inputDomain = K.GetDomainFromFirstComponent(expanderFilename);
            List<int> shapeListFilter = K.GetShapeFromKrystalName(densityFilename);

            string title = $"Possible points input krystals -- DensityInput: {densityFilename}, Expander: {expanderFilename}";

            _krystalBrowser = new KrystalsBrowser(title, inputDomain, shapeListFilter, SetPointsInputKrystal);

            _krystalBrowser.Show();
        }
        private void SetPointsInputKrystal(Krystal krystal)
        {
            PointsInputFilenameLabel.Text = krystal.Name;
            _pointsInputFilepath = _krystalsFolder + @"\" + krystal.Name;
            if(_krystalBrowser != null)
            {
                _krystalBrowser.Close();
            }

            SetPointsInputValuesButton.Enabled = false;
            OKBtn.Enabled = true;
            OKBtn.Focus();
        }

        private void CancelBtn_Click(object sender, EventArgs e)
        {
            _densityInputFilepath = "";
            _pointsInputFilepath = "";
            _expanderFilepath = "";

            DialogResult = DialogResult.Cancel;
        }
        private void OKBtn_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
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
        private KrystalsBrowser _krystalBrowser;
        private readonly string _krystalsFolder;
        #endregion private variables

    }
}