using Moritz.Globals;

using System;
using System.IO;
using System.Windows.Forms;

namespace Moritz
{
    internal partial class FolderLocationsDialog : Form
    {
        public FolderLocationsDialog()
        {
            InitializeComponent();

            SetUserInfo();
        }

        private void OutputDevicesComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            OKBtn.Focus();
        }

        private void SetUserInfo()
        {
            MoritzFolderLocationInfoLabel.Text = M.MoritzAppDataFolder;

            KrystalsFolderInfoLabel.Text = M.MoritzKrystalsFolder;
            ExpansionFieldsFolderInfoLabel.Text = M.MoritzExpansionFieldsFolder;
            ModulationOperatorsFolderInfoLabel.Text = M.MoritzModulationOperatorsFolder;
            ScoresRootFolderInfoLabel.Text = M.MoritzScoresFolder;

            XMLSchemasFolderInfoLabel.Text = M.XMLSchemasFolder;
        }

        private void OKBtn_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}