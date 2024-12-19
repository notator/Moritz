using Moritz.Composer;
using Moritz.Globals;

using System;
using System.IO;
using System.Windows.Forms;

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
            ShowDialogForm(new Krystals5Application.MainWindow());
        }
        #endregion

        private void LoadScoreSettingsButton_Click(object sender, EventArgs e)
        {
            using(var openFileDialog = new OpenFileDialog())
            {
                openFileDialog.InitialDirectory = M.MoritzScoresFolder;
                openFileDialog.Filter = $"Krystal Score Settings (*{M.MoritzKrystalScoreSettingsExtension})|*{M.MoritzKrystalScoreSettingsExtension}";
                openFileDialog.FilterIndex = 0;
                openFileDialog.Title = "Load Krystal Score Settings";
                openFileDialog.RestoreDirectory = true;

                if(openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    var settingsPathname = openFileDialog.FileName;
                    if(!string.IsNullOrEmpty(settingsPathname))
                    {
                        _assistantComposerForm = new AssistantComposerForm(settingsPathname, this);
                        _assistantComposerForm.Show();
                        this.Hide();
                    }
                }
            }
        }

        private void PreferencesButton_Click(object sender, EventArgs e)
        {
            ShowDialogForm(new PreferencesDialog());
        }

        private void AboutButton_Click(object sender, EventArgs e)
        {
            ShowDialogForm(new AboutMoritzDialog());
        }

        private void QuitButton_Click(object sender, EventArgs e)
        {
            this.Close();
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
            _assistantComposerForm?.Dispose();
            M.Preferences.Dispose();
        }

        public void CloseAssistantComposer()
        {
            if(_assistantComposerForm != null && _assistantComposerForm.DiscardAnyChanges())
            {
                _assistantComposerForm.Close();
                _assistantComposerForm = null;
            }
            this.Show();
        }

        private void ShowDialogForm(Form form)
        {
            using(form)
            {
                form.ShowDialog(this);
            }
        }

        private AssistantComposerForm _assistantComposerForm = null;
    }
}
