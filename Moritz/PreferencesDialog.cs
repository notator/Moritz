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

			Preferences = M.Preferences;

            SetConstantUserFoldersInfo();
            SetActiveDevicesComboBoxes();

			this.InputDevicesComboBox.SelectedItem = Preferences.PreferredInputDevice;
			this.OutputDevicesComboBox.SelectedItem = Preferences.PreferredOutputDevice;
        }

        private void SetConstantUserFoldersInfo()
        {
            LocalMoritzFolderTextBox.Text = Preferences.LocalMoritzFolderLocation;
            PreferencesFilePathLabel.Text = Preferences.LocalMoritzPreferencesPath;
            LocalAudioFolderInfoLabel.Text = Preferences.LocalMoritzAudioFolder;
            LocalKrystalsFolderInfoLabel.Text = Preferences.LocalMoritzKrystalsFolder;
            LocalExpansionFieldsFolderInfoLabel.Text = Preferences.LocalMoritzExpansionFieldsFolder;
            LocalModulationOperatorsFolderInfoLabel.Text = Preferences.LocalMoritzModulationOperatorsFolder;
            LocalScoresRootFolderInfoLabel.Text = Preferences.LocalMoritzScoresFolder;

            OnlineMoritzFolderInfoLabel.Text = Preferences.OnlineMoritzFolder;
            OnlineMoritzAudioFolderInfoLabel.Text = Preferences.OnlineMoritzAudioFolder;
            OnlineXMLSchemasFolderInfoLabel.Text = Preferences.OnlineXMLSchemasFolder;
        }

        private void SetActiveDevicesComboBoxes()
        {
			InputDevicesComboBox.SuspendLayout();
			InputDevicesComboBox.Items.Clear();
			foreach(string activeInputDevice in Preferences.AvailableMultimediaMidiInputDeviceNames)
			{
				InputDevicesComboBox.Items.Add(activeInputDevice);
			}
			if(InputDevicesComboBox.Items.Count == 0)
			{
				InputDevicesComboBox.Items.Add("");
			}
			InputDevicesComboBox.ResumeLayout();
			InputDevicesComboBox.SelectedIndex = 0;

			OutputDevicesComboBox.SuspendLayout();
			OutputDevicesComboBox.Items.Clear();
			foreach(string activeOutputDevice in Preferences.AvailableMultimediaMidiOutputDeviceNames)
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

		private void LocalMoritzFolderTextBox_Leave(object sender, EventArgs e)
		{
			if(Directory.Exists(LocalMoritzFolderTextBox.Text))
			{ 
				Preferences.LocalMoritzFolderLocation = LocalMoritzFolderTextBox.Text;
				M.SetTextBoxErrorColorIfNotOkay(LocalMoritzFolderTextBox, true);
				OKBtn.Enabled = true;
				SetConstantUserFoldersInfo();
			}
			else
			{
				M.SetTextBoxErrorColorIfNotOkay(LocalMoritzFolderTextBox, false);
			}
		}

		private void LocalMoritzFolderTextBox_Enter(object sender, EventArgs e)
		{
			OKBtn.Enabled = false;
		}

		#region OK, Cancel
		private void OKBtn_Click(object sender, EventArgs e)
		{
			string inOut = "";
			if(InputDevicesComboBox.SelectedItem == null)
			{
				inOut = "input";	
			}
			else if(OutputDevicesComboBox.SelectedItem == null)
			{
				inOut = "output";
			}

			if(!string.IsNullOrEmpty(inOut))
			{
				MessageBox.Show("The " + inOut + " device selector must be set to a valid value.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
			}
			else
			{
				Preferences.PreferredInputDevice = InputDevicesComboBox.SelectedItem.ToString(); // can be "" if there are no midi input devices
				Preferences.PreferredOutputDevice = OutputDevicesComboBox.SelectedItem.ToString(); // can be "" if there are no midi output devices
				Preferences.Save();
				Close();
			}			
		}
		#endregion

        public Preferences Preferences = null;
	}
}