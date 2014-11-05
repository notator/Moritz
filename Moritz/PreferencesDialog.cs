using System;
using System.Windows.Forms;

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
            SetActiveDevicesListBoxes();

            this.PreferredInputDeviceNameTextBox.Text = Preferences.PreferredInputDevice;
            this.PreferredOutputDeviceNameTextBox.Text = Preferences.PreferredOutputDevice;
        }

        private void SetConstantUserFoldersInfo()
        {
            LocalMoritzFolderInfoLabel.Text = Preferences.LocalMoritzFolder;
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

        private void SetActiveDevicesListBoxes()
        {
            ActiveInputDevicesListBox.SuspendLayout();
            foreach(string activeInputDevice in Preferences.AvailableMultimediaMidiInputDeviceNames)
            {
                ActiveInputDevicesListBox.Items.Add(activeInputDevice);
            }
            ActiveInputDevicesListBox.ResumeLayout();

            ActiveOutputDevicesListBox.SuspendLayout();
            foreach(string activeOutputDevice in Preferences.AvailableMultimediaMidiOutputDeviceNames)
            {
                ActiveOutputDevicesListBox.Items.Add(activeOutputDevice);
            }
            ActiveOutputDevicesListBox.ResumeLayout();
        }

		#region OK, Cancel
		private void OKBtn_Click(object sender, EventArgs e)
		{
            Preferences.PreferredInputDevice = PreferredInputDeviceNameTextBox.Text;
            Preferences.PreferredOutputDevice = PreferredOutputDeviceNameTextBox.Text;
            Preferences.Save();
			Close();
		}
		#endregion

        public Preferences Preferences = null;


	}
}