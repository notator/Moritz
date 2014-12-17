using System;
using System.Windows.Forms;
using System.IO;

using Moritz.Globals;

namespace Moritz
{
	internal partial class PreferencesDialog : Form
	{
        public PreferencesDialog()
		{
			InitializeComponent();

			SetActiveDevicesComboBoxes();
			this.OutputDevicesComboBox.SelectedItem = M.Preferences.PreferredOutputDevice;
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
            LocalMoritzFolderTextBox.Text = M.Preferences.LocalMoritzFolderLocation;
            PreferencesFilePathLabel.Text = M.Preferences.LocalMoritzPreferencesPath;
            LocalAudioFolderInfoLabel.Text = M.Preferences.LocalMoritzAudioFolder;
            LocalKrystalsFolderInfoLabel.Text = M.Preferences.LocalMoritzKrystalsFolder;
            LocalExpansionFieldsFolderInfoLabel.Text = M.Preferences.LocalMoritzExpansionFieldsFolder;
            LocalModulationOperatorsFolderInfoLabel.Text = M.Preferences.LocalMoritzModulationOperatorsFolder;
            LocalScoresRootFolderInfoLabel.Text = M.Preferences.LocalMoritzScoresFolder;

            OnlineXMLSchemasFolderInfoLabel.Text = M.Preferences.OnlineXMLSchemasFolder;
        }

		private void LocalMoritzFolderTextBox_Leave(object sender, EventArgs e)
		{
			if(Directory.Exists(LocalMoritzFolderTextBox.Text))
			{ 
				M.Preferences.LocalMoritzFolderLocation = LocalMoritzFolderTextBox.Text;
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