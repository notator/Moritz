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

			this.HiddenTextBox.Hide();

			Preferences = M.Preferences;

            SetUserInfo();
        }

        private void SetUserInfo()
        {
			MidiOutputDeviceHelpLabel.Text = Preferences.PreferredOutputDevice + "  (change this in the Assistant Composer)";

            LocalMoritzFolderTextBox.Text = Preferences.LocalMoritzFolderLocation;
            PreferencesFilePathLabel.Text = Preferences.LocalMoritzPreferencesPath;
            LocalAudioFolderInfoLabel.Text = Preferences.LocalMoritzAudioFolder;
            LocalKrystalsFolderInfoLabel.Text = Preferences.LocalMoritzKrystalsFolder;
            LocalExpansionFieldsFolderInfoLabel.Text = Preferences.LocalMoritzExpansionFieldsFolder;
            LocalModulationOperatorsFolderInfoLabel.Text = Preferences.LocalMoritzModulationOperatorsFolder;
            LocalScoresRootFolderInfoLabel.Text = Preferences.LocalMoritzScoresFolder;

            OnlineXMLSchemasFolderInfoLabel.Text = Preferences.OnlineXMLSchemasFolder;
        }

		private void LocalMoritzFolderTextBox_Leave(object sender, EventArgs e)
		{
			if(Directory.Exists(LocalMoritzFolderTextBox.Text))
			{ 
				Preferences.LocalMoritzFolderLocation = LocalMoritzFolderTextBox.Text;
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
			Preferences.Save();
			Close();			
		}
		#endregion

        public Preferences Preferences = null;
	}
}