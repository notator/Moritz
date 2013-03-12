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
            PreferencesFilePathLabel.Text = Preferences.PreferencesPath;

            SetFolderInfo();
            SetActiveDevicesListBoxes();

            this.PreferredInputDeviceNameTextBox.Text = Preferences.PreferredInputDevice;
            this.PreferredOutputDeviceNameTextBox.Text = Preferences.PreferredOutputDevice;
        }

        private void SetFolderInfo()
        {
            this.LocalUserFolderTextBox.Text = Preferences.LocalUserFolder;
            this.LocalKrystalsFolderInfoLabel.Text = Preferences.LocalKrystalsFolder;
            this.LocalExpansionFieldsFolderInfoLabel.Text = Preferences.LocalExpansionFieldsFolder;
            this.LocalModulationOperatorsFolderInfoLabel.Text = Preferences.LocalModulationOperatorsFolder;
            this.LocalScoresRootFolderInfoLabel.Text = Preferences.LocalScoresRootFolder;

            this.OnlineUserFolderTextBox.Text = Preferences.OnlineUserFolder;
            this.OnlineMoritzAudioFolderInfoLabel.Text = Preferences.OnlineUserAudioFolder;
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

        private void LocalUserFolderButton_Click(object sender, EventArgs e)
        {
            using(FolderBrowserDialog fbd = new FolderBrowserDialog())
            {
                fbd.Description = "Select root folder for all Moritz' local user documents:";
                fbd.RootFolder = Environment.SpecialFolder.Desktop;
                DialogResult result = fbd.ShowDialog();
                if(result == DialogResult.OK)
                {
                    LocalUserFolderTextBox.Text = fbd.SelectedPath;
                    LocalUserFolderTextBox_Leave(null, null);
                    //Preferences.LocalUserFolder = fbd.SelectedPath;
                    //SetFolderInfo();
                }
            }
        }

        private void LocalUserFolderTextBox_Leave(object sender, EventArgs e)
        {
            Preferences.LocalUserFolder = LocalUserFolderTextBox.Text;
            SetFolderInfo();
        }

        private void OnlineUserFolderTextBox_Leave(object sender, EventArgs e)
        {
            Preferences.OnlineUserFolder = OnlineUserFolderTextBox.Text;
            SetFolderInfo();
        }

		#region OK, Cancel
		private void OKBtn_Click(object sender, EventArgs e)
		{
            M.CreateDirectoryIfItDoesNotExist(LocalUserFolderTextBox.Text);
            M.CreateDirectoryIfItDoesNotExist(LocalKrystalsFolderInfoLabel.Text);
            M.CreateDirectoryIfItDoesNotExist(LocalExpansionFieldsFolderInfoLabel.Text);
            M.CreateDirectoryIfItDoesNotExist(LocalModulationOperatorsFolderInfoLabel.Text);
            M.CreateDirectoryIfItDoesNotExist(LocalScoresRootFolderInfoLabel.Text);
            M.CreateDirectoryIfItDoesNotExist(LocalUserFolderTextBox.Text);

            Preferences.LocalUserFolder = LocalUserFolderTextBox.Text;
            Preferences.PreferredInputDevice = PreferredInputDeviceNameTextBox.Text;
            Preferences.PreferredOutputDevice = PreferredOutputDeviceNameTextBox.Text;
            Preferences.Save();
			Close();
		}
		#endregion

		public Preferences Preferences = null;


	}
}