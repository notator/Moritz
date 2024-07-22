using Krystals5ObjectLibrary;

using Moritz.Globals; // Preferences

using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace Krystals5Application
{
    internal partial class NewExpansionDialog : Form
    {
        public NewExpansionDialog()
        {
            InitializeComponent();
            _krystalsBrowser = null;

            SetDensityInputButton.Focus();

            ExpandBtn.Enabled = false;
            CloseBtn.Enabled = true;
        }
        #region Events
        private void SetDensityInputButton_Click(object sender, EventArgs e)
        {
            if(_krystalsBrowser != null)
            {
                _krystalsBrowser.Close();
            }
            _krystalsBrowser = new KrystalsBrowser("All krystals: (Get Density Input)", SetDensityInputKrystal); // TODO: set filter to show all files
            _krystalsBrowser.Show();
        }
        private void SetDensityInputKrystal(Krystal krystal)
        {
            DensityInputFilenameLabel.Text = krystal.Name;
            _densityInputFilepath = _krystalsFolder + @"\" + krystal.Name;
            if(_krystalsBrowser != null)
            {
                _krystalsBrowser.Close();
            }

            SetButtonsEnabledState();
        }

        private void SetExpanderButton_Click(object sender, EventArgs e)
        {
            _expanderFilepath = K.GetFilepathFromOpenFileDialog(K.DialogFilterIndex.expander);
            if(_expanderFilepath != "")
            {
                ExpanderFilenameLabel.Text = Path.GetFileName(_expanderFilepath);
            }
            SetButtonsEnabledState();
        }

        private void SetPointsInputButton_Click(object sender, EventArgs e)
        {
            char[] dot = new char[] { '.' };
            var densityFilename = Path.GetFileName(_densityInputFilepath);
            var expanderFilename = Path.GetFileName(_expanderFilepath);

            int inputDomain = K.GetDomainFromFirstComponent(expanderFilename);
            List<int> shapeListFilter = K.GetShapeFromKrystalName(densityFilename);

            string title = $"Possible points input krystals -- DensityInput: {densityFilename}, Expander: {expanderFilename}";

            _krystalsBrowser = new KrystalsBrowser(title, inputDomain, shapeListFilter, SetPointsInputKrystal);

            _krystalsBrowser.Show();
        }
        private void SetPointsInputKrystal(Krystal krystal)
        {
            PointsInputFilenameLabel.Text = krystal.Name;
            _pointsInputFilepath = _krystalsFolder + @"\" + krystal.Name;
            if(_krystalsBrowser != null)
            {
                _krystalsBrowser.Close();
            }

            SetButtonsEnabledState();
        }

        private void SetButtonsEnabledState()
        {
            SetPointsInputValuesButton.Enabled = !(String.IsNullOrEmpty(_densityInputFilepath) || String.IsNullOrEmpty(_expanderFilepath));
            ExpandBtn.Enabled = !(String.IsNullOrEmpty(_densityInputFilepath) || String.IsNullOrEmpty(_pointsInputFilepath) || String.IsNullOrEmpty(_expanderFilepath));
        }

        private void CloseBtn_Click(object sender, EventArgs e)
        {
            // DialogResult = DialogResult.OK is set in the designer
            _densityInputFilepath = "";
            _pointsInputFilepath = "";
            _expanderFilepath = "";
        }
        private void ExpandBtn_Click(object sender, EventArgs e)
        {
            try
            {
                var expansionKrystal = new ExpansionKrystal(_densityInputFilepath, _pointsInputFilepath, _expanderFilepath);
                var hasBeenSaved = expansionKrystal.Save();
                if(hasBeenSaved)
                {
                    _krystalsBrowser = new KrystalsBrowser("New Krystal", expansionKrystal, null);
                    _krystalsBrowser.Show();
                    _krystalsBrowser.BringToFront();
                }
            }
            catch(ApplicationException ae)
            {
                MessageBox.Show(ae.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        #endregion Events

        #region private variables
        private readonly string _krystalsFolder = M.MoritzKrystalsFolder;
        private string _densityInputFilepath;
        private string _pointsInputFilepath;
        private string _expanderFilepath;
        private KrystalsBrowser _krystalsBrowser;
        #endregion private variables
    }
}