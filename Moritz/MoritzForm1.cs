using System;
using System.Windows.Forms;
using System.Xml;
using System.Diagnostics;
using System.IO;

using Moritz.Globals;
using Moritz.Composer;
using Moritz.Performer;

using Krystals4Application;

namespace Moritz
{
    public partial class MoritzForm1 : Form, IMoritzForm1
    {
        public MoritzForm1()
        {
            InitializeComponent(); 
            SetInputDevicesComboBox();
            SetOutputDevicesComboBox();
        }

        private void SetInputDevicesComboBox()
        {
            InputDeviceComboBox.SuspendLayout();
            InputDeviceComboBox.Items.Clear();
            if(M.Preferences.AvailableMultimediaMidiInputDeviceNames.Count > 0)
            {
                foreach(string name in M.Preferences.AvailableMultimediaMidiInputDeviceNames)
                {
                    InputDeviceComboBox.Items.Add(name);
                }
                if(M.Preferences.AvailableMultimediaMidiInputDeviceNames.Contains(M.Preferences.PreferredInputDevice))
                {
                    InputDeviceComboBox.SelectedItem = M.Preferences.PreferredInputDevice;
                }
                else
                {
                    InputDeviceComboBox.SelectedIndex = 0;
                }
            }
            else
            {
                InputDeviceComboBox.Hide();
            }
            InputDeviceComboBox.ResumeLayout();
        }

        private void SetOutputDevicesComboBox()
        {
            if(M.Preferences.AvailableMultimediaMidiOutputDeviceNames.Count > 0)
            {
                OutputDeviceComboBox.SuspendLayout();
                OutputDeviceComboBox.Items.Clear();

                foreach(string name in M.Preferences.AvailableMultimediaMidiOutputDeviceNames)
                {
                    OutputDeviceComboBox.Items.Add(name);
                }

                OutputDeviceComboBox.SelectedIndex = 0;
                for(int i = 0; i < OutputDeviceComboBox.Items.Count; i++)
                {
                    if(OutputDeviceComboBox.Items[i].ToString() == M.Preferences.PreferredOutputDevice)
                    {
                        OutputDeviceComboBox.SelectedIndex = i;
                        break;
                    }
                }
            }
            else
            {
                this.Close();
            }
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

        #region new score settings
        private void NewScoreSettingsButton_Click(object sender, EventArgs e)
        {
            string scoreName = null;
            string algorithmName = null;
            using(NewScoreDialog dialog = new NewScoreDialog())
            {
                if(dialog.ShowDialog(this) == DialogResult.OK)
                {
                    // Read the contents of testDialog's TextBox.
                    scoreName = dialog.ScoreNameTextBox.Text;
                    algorithmName = (string)dialog.AlgorithmComboBox.SelectedItem;

                    string settingsPath =
                        M.Preferences.LocalMoritzScoresFolder + @"\" + algorithmName + @"\" +
                            scoreName + @" score\" + scoreName + M.MoritzKrystalScoreSettingsExtension;

                    SaveDefaultSettings(settingsPath, algorithmName);

                    _assistantComposerMainForm = new AssistantComposerMainForm(settingsPath, (IMoritzForm1)this);
                    if(_assistantComposerMainForm != null
                    && !(_assistantComposerMainForm.Disposing || _assistantComposerMainForm.IsDisposed))
                    {
                        _assistantComposerMainForm.Show();
                        this.Hide();
                    }
                }
            }
        }
        public void SaveDefaultSettings(string settingsPath, string algorithmName)
        {
            Debug.Assert(!string.IsNullOrEmpty(settingsPath));

            string scoreFolderPath = Path.GetDirectoryName(settingsPath);

            M.CreateDirectoryIfItDoesNotExist(scoreFolderPath);

            #region do the save
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.IndentChars = ("\t");
            settings.NewLineOnAttributes = true;
            settings.CloseOutput = false;
            using(XmlWriter w = XmlWriter.Create(settingsPath, settings))
            {
                w.WriteStartDocument();
                w.WriteComment("file created: " + M.NowString);

                w.WriteStartElement("moritzKrystalScore");
                w.WriteAttributeString("xmlns", "xsi", null, "http://www.w3.org/2001/XMLSchema-instance");
                w.WriteAttributeString("xsi", "noNamespaceSchemaLocation", null, M.OnlineXMLSchemasFolder + "/moritzKrystalScore.xsd");

                WriteDefaultMetadata(w);
                WriteDefaultDimensions(w);
                WriteDefaultNotation(w, algorithmName);
                w.WriteEndElement(); // closes the moritzKrystalScore element
                w.Close(); // close unnecessary because of the using statement?
            }
            #endregion do the save
        }

        private void WriteDefaultMetadata(XmlWriter w)
        {
            w.WriteStartElement("metadata");
            w.WriteStartElement("websiteLink");
            w.WriteAttributeString("aboutLinkText", "");
            w.WriteAttributeString("aboutLinkURL", "");
            w.WriteEndElement(); // websiteLink
            w.WriteEndElement(); // metadata
        }
        private void WriteDefaultDimensions(XmlWriter w)
        {
            w.WriteStartElement("dimensions");
            w.WriteStartElement("paper");
            w.WriteAttributeString("size", "A3");
            w.WriteAttributeString("landscape", "0");
            w.WriteEndElement(); // paper

            w.WriteStartElement("title");
            w.WriteAttributeString("titleHeight", "32");
            w.WriteAttributeString("authorHeight", "16");
            w.WriteAttributeString("titleY", "50");
            w.WriteEndElement(); // title

            w.WriteStartElement("margins");
            w.WriteAttributeString("topPage1", "90");
            w.WriteAttributeString("topOtherPages", "50");
            w.WriteAttributeString("right", "50");
            w.WriteAttributeString("bottom", "50");
            w.WriteAttributeString("left", "50");
            w.WriteEndElement(); // margins
            w.WriteEndElement(); // dimensions
        }
        private void WriteDefaultNotation(XmlWriter w, string algorithmName)
        {
            w.WriteStartElement("notation");
            w.WriteAttributeString("chordSymbolType", "standard");
            w.WriteAttributeString("minimumCrotchetDuration", "800");
            w.WriteAttributeString("beamsCrossBarlines", "false");

            w.WriteAttributeString("stafflineStemStrokeWidth", "0.5");
            w.WriteAttributeString("gap", "4");
            w.WriteAttributeString("minGapsBetweenStaves", "8");
            w.WriteAttributeString("minGapsBetweenSystems", "11");
            w.WriteAttributeString("midiChannelsPerVoicePerStaff", "");
            w.WriteAttributeString("clefsPerStaff", "");
            w.WriteAttributeString("stafflinesPerStaff", "");
            w.WriteAttributeString("staffGroups", "");
            w.WriteAttributeString("longStaffNames", "");
            w.WriteAttributeString("shortStaffNames", "");
            w.WriteAttributeString("systemStartBars", "");
            w.WriteEndElement(); // notation
        }

        #endregion new score settings
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
                        _assistantComposerMainForm = new AssistantComposerMainForm(settingsPathname, (IMoritzForm1)this);
                        _assistantComposerMainForm.Show();
                        this.Hide();
                    }
                }
            }
        }

        private void CloseAssistantPerformerButton_Click(object sender, EventArgs e)
        {
            CloseAssistantPerformer();
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

        private void MidiInputDevicesComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            M.Preferences.CurrentInputDeviceName = (string)InputDeviceComboBox.SelectedItem;
            CurrentInputDeviceLabel.Focus(); // deselects the text in the comboBox
        }
        private void MidiOutputDevicesComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            M.Preferences.CurrentOutputDeviceName = (string)OutputDeviceComboBox.SelectedItem;
            CurrentOutputDeviceLabel.Focus(); // deselects the text in the comboBox
        }
        private void MoritzForm1_VisibleChanged(object sender, EventArgs e)
        {
            if(_assistantPerformerMainForm != null || _assistantComposerMainForm != null)
            {
                EnableDeviceComboBoxesAndPreferences(false);
            }
            else
            {
                EnableDeviceComboBoxesAndPreferences(true);
            }

            if(_assistantPerformerMainForm != null)
            {
                CloseAssistantPerformerButton.Show();
                PerformScoreButton.Hide();
            }
            else
            {
                PerformScoreButton.Show();
                CloseAssistantPerformerButton.Hide();
            }

            if(_assistantComposerMainForm != null)
            {
                NewScoreSettingsButton.Hide();
                LoadScoreSettingsButton.Hide();
                CloseAssistantComposerButton.Show();
            }
            else
            {
                NewScoreSettingsButton.Show();
                LoadScoreSettingsButton.Show();
                CloseAssistantComposerButton.Hide();
            }
        }
        private void EnableDeviceComboBoxesAndPreferences(bool enable)
        {
            this.CurrentInputDeviceLabel.Enabled = enable;
            this.InputDeviceComboBox.Enabled = enable;
            this.CurrentOutputDeviceLabel.Enabled = enable;
            this.OutputDeviceComboBox.Enabled = enable;
            this.PreferencesButton.Enabled = enable;
        }

        /// <summary>
        /// MoritzForm1 could be closed by a subsidiary Form, for example when
        /// quitting from the AssistantPerformer.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MoritzForm1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if(_assistantComposerMainForm != null && !_assistantComposerMainForm.Disposing)
                _assistantComposerMainForm.Dispose();
            M.Preferences.Dispose();
        }

        public void CloseAssistantComposer()
        {
            if(_assistantComposerMainForm != null)
            {
                _assistantComposerMainForm.Close();
                _assistantComposerMainForm = null;
            }
            MoritzForm1_VisibleChanged(null, null);
            this.Show();
        }

        private Form _assistantComposerMainForm = null;
    }
}