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
            if(!(Directory.Exists(M.LocalAssistantPerformerScoresFolder)
                && Directory.Exists(M.LocalMoritzAlgorithmFolder)))
            {
                MessageBox.Show("By design, this program only runs on my main desktop computer (j.i.).", "Can´t Run.", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                this.Close();
            }
            else
            {
                using(Form krystals5Application = new Krystals5Application.MainWindow())
                {
                    krystals5Application.ShowDialog();
                }
            }
        }
        #endregion

        private void LoadScoreSettingsButton_Click(object sender, EventArgs e)
        {
            if(!(Directory.Exists(M.LocalAssistantPerformerScoresFolder)
                && Directory.Exists(M.LocalMoritzAlgorithmFolder)))
            {
                MessageBox.Show("By design, this program only runs on my main desktop computer (j.i.).", "Can´t Run.", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                this.Close();
            }
            else
            {
                using(OpenFileDialog openFileDialog = new OpenFileDialog())
                {
                    openFileDialog.InitialDirectory = M.LocalAssistantPerformerScoresFolder;
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
        }

        private void PreferencesButton_Click(object sender, EventArgs e)
        {
            if(!(Directory.Exists(M.LocalAssistantPerformerScoresFolder)
                && Directory.Exists(M.LocalMoritzAlgorithmFolder)))
            {
                MessageBox.Show("By design, this program only runs on my main desktop computer (j.i.).", "Can´t Run.", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                this.Close();
            }
            else
            {
                using(PreferencesDialog pd = new PreferencesDialog())
                {
                    pd.ShowDialog(this);
                }
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
            this.Show();
        }

        private AssistantComposerForm _assistantComposerForm = null;
    }
}