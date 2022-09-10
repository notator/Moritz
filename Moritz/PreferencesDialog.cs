using Moritz.Globals;

using System;
using System.IO;
using System.Windows.Forms;

namespace Moritz
{
    internal partial class PreferencesDialog : Form
    {
        public PreferencesDialog()
        {
            InitializeComponent();

            SetActiveDevicesComboBoxes();
            this.OutputDevicesComboBox.SelectedItem = M.Preferences.PreferredOutputDevice;
            // if the PreferredOutputDevice was not found, set it to the top item in the comboBox
            M.Preferences.PreferredOutputDevice = (string)this.OutputDevicesComboBox.SelectedItem;

            this.OutputDevicesComboBox.SelectedIndexChanged += OutputDevicesComboBox_SelectedIndexChanged;

            SetUserInfo();
        }

        private void SetActiveDevicesComboBoxes()
        {
            OutputDevicesComboBox.SuspendLayout();
            OutputDevicesComboBox.Items.Clear();
            foreach(string activeOutputDevice in M.Preferences.AvailableMultimediaMidiOutputDeviceNames)
            {
                OutputDevicesComboBox.Items.Add(activeOutputDevice);
            }
            if(OutputDevicesComboBox.Items.Count == 0)
            {
                OutputDevicesComboBox.Items.Add("");
            }
            OutputDevicesComboBox.ResumeLayout();
            OutputDevicesComboBox.SelectedIndex = 0;
        }

        private void OutputDevicesComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            M.Preferences.PreferredOutputDevice = OutputDevicesComboBox.SelectedItem.ToString();
            OKBtn.Focus();
        }

        private void SetUserInfo()
        {
            LocalMoritzFolderTextBox.Text = M.LocalMoritzFolderLocation;
            PreferencesFilePathLabel.Text = M.LocalMoritzPreferencesPath;
            LocalAudioFolderInfoLabel.Text = M.LocalMoritzAudioFolder;
            LocalKrystalsFolderInfoLabel.Text = M.LocalMoritzKrystalsFolder;
            LocalExpansionFieldsFolderInfoLabel.Text = M.LocalMoritzExpansionFieldsFolder;
            LocalModulationOperatorsFolderInfoLabel.Text = M.LocalMoritzModulationOperatorsFolder;
            LocalScoresRootFolderInfoLabel.Text = M.LocalMoritzScoresFolder;

            OnlineXMLSchemasFolderInfoLabel.Text = M.OnlineXMLSchemasFolder;
        }

        private void LocalMoritzFolderTextBox_Leave(object sender, EventArgs e)
        {
            if(Directory.Exists(LocalMoritzFolderTextBox.Text))
            {
                M.LocalMoritzFolderLocation = LocalMoritzFolderTextBox.Text;
                M.SetTextBoxErrorColorIfNotOkay(LocalMoritzFolderTextBox, true);
                OKBtn.Enabled = true;
                SetUserInfo();
            }
            else
            {
                M.SetTextBoxErrorColorIfNotOkay(LocalMoritzFolderTextBox, false);
                OKBtn.Enabled = false;
            }
        }

        private void LocalMoritzFolderTextBox_Enter(object sender, EventArgs e)
        {
            M.SetTextBoxErrorColorIfNotOkay(LocalMoritzFolderTextBox, true);
            OKBtn.Enabled = true;
        }

        #region OK, Cancel
        private void OKBtn_Click(object sender, EventArgs e)
        {
            M.Preferences.Save();
            Close();
        }
        #endregion
    }
}