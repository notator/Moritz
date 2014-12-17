using System;
using System.Windows.Forms;
using System.Xml;
using System.Diagnostics;
using System.IO;

using Moritz.Globals;
using Moritz.Composer;

using Krystals4Application;

namespace Moritz
{
    public partial class MoritzForm1 : Form, IMoritzForm1
    {
        public MoritzForm1()
        {
            InitializeComponent(); 
        }

        #region Krystals Editor
        private void KrystalsEditorButton_Click(object sender, EventArgs e)
        {
            using(Form krystals4Application = new Krystals4Application.MainWindow())
            {
                krystals4Application.ShowDialog();
            }
        }
        #endregion

        private void LoadScoreSettingsButton_Click(object sender, EventArgs e)
        {
            using(OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.InitialDirectory = M.Preferences.LocalMoritzScoresFolder;
                string filterString = @"Krystal Score Settings (*" + M.MoritzKrystalScoreSettingsExtension +
                    @")|*" + M.MoritzKrystalScoreSettingsExtension;
                // "Krystal Score Settings (*.mkss)|*.mkss";
                openFileDialog.Filter = filterString;
                openFileDialog.FilterIndex = (int)0;
                openFileDialog.Title = "Load Krystal Score Settings";
                openFileDialog.RestoreDirectory = true;

                if(openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string settingsPathname = openFileDialog.FileName;
                    if(!String.IsNullOrEmpty(settingsPathname))
                    {
                        _assistantComposerForm = new AssistantComposerForm(settingsPathname, (IMoritzForm1)this);
                        _assistantComposerForm.Show();
                        this.Hide();
                    }
                }
            }
        }

        private void CloseAssistantComposerButton_Click(object sender, EventArgs e)
        {
            CloseAssistantComposer();
        }

        private void PreferencesButton_Click(object sender, EventArgs e)
        {
            using(PreferencesDialog pd = new PreferencesDialog())
            {
                pd.ShowDialog(this);
            }
        }

        private void AboutButton_Click(object sender, EventArgs e)
        {
            using(Form aboutMoritz = new AboutMoritzDialog())
            {
                aboutMoritz.ShowDialog();
            }
        }

        private void QuitButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

		//private void MidiOutputDevicesComboBox_SelectedIndexChanged(object sender, EventArgs e)
		//{
		//	M.Preferences.CurrentOutputDeviceName = (string)OutputDeviceComboBox.SelectedItem;
		//	CurrentOutputDeviceLabel.Focus(); // deselects the text in the comboBox
		//}
        private void MoritzForm1_VisibleChanged(object sender, EventArgs e)
        {
            if( _assistantComposerForm != null)
            {
				this.PreferencesButton.Enabled = false;
            }
            else
			{
				this.PreferencesButton.Enabled = true;
            }

            if(_assistantComposerForm != null)
            {
                LoadScoreSettingsButton.Hide();
                CloseAssistantComposerButton.Show();
            }
            else
            {
                LoadScoreSettingsButton.Show();
                CloseAssistantComposerButton.Hide();
            }
        }
        private void EnableDeviceComboBoxesAndPreferences(bool enable)
        {
            this.PreferencesButton.Enabled = enable;
        }

        /// <summary>
        /// MoritzForm1 can be closed by a subsidiary Form, for example when
        /// quitting from the AssistantComposer.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MoritzForm1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if(_assistantComposerForm != null && !_assistantComposerForm.Disposing)
                _assistantComposerForm.Dispose();
            M.Preferences.Dispose();
        }

        public void CloseAssistantComposer()
        {
            if(_assistantComposerForm != null && _assistantComposerForm.DiscardAnyChanges())
            {
                _assistantComposerForm.Close();
                _assistantComposerForm = null;
            }
            MoritzForm1_VisibleChanged(null, null);
            this.Show();
        }

        private AssistantComposerForm _assistantComposerForm = null;
    }
}