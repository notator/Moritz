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
            MoritzFolderLocationInfoLabel.Text = M.MoritzAppDataFolder;
            PreferencesFilePathInfoLabel.Text = M.MoritzPreferencesPath;
            AudioFolderInfoLabel.Text = M.MoritzAudioFolder;
            KrystalsFolderInfoLabel.Text = M.MoritzKrystalsFolder;
            ExpansionFieldsFolderInfoLabel.Text = M.MoritzExpansionFieldsFolder;
            ModulationOperatorsFolderInfoLabel.Text = M.MoritzModulationOperatorsFolder;
            ScoresRootFolderInfoLabel.Text = M.MoritzScoresFolder;

            OnlineXMLSchemasFolderInfoLabel.Text = M.OnlineXMLSchemasFolder;
        }

        private void OKBtn_Click(object sender, EventArgs e)
        {
            M.Preferences.Save();
            Close();
        }
    }
}