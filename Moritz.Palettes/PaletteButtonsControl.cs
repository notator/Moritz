using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Xml;
using System.Diagnostics;
using System.Threading;
using System.IO;

using Multimedia.Midi;

using Moritz.Globals;
using Moritz.Midi;
using Moritz.Spec;
using Moritz.Symbols;

namespace Moritz.Palettes
{
    public partial class PaletteButtonsControl : UserControl
    {
        internal PaletteButtonsControl(int domain, Point location, PaletteForm paletteForm, string audioFolder)
        {
            InitializeComponent();

            _paletteForm = paletteForm;
            _audioFolder = audioFolder;
            this.SuspendLayout();
            this.Location = location;
            this.Size = new Size(domain * 33, 75);
            AddPaletteButtons(domain);
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        /// <summary>
        /// Returns when the mediaPlayer has really stopped
        /// </summary>
        public void StopCurrentMediaPlayer()
        {
            if(_performingButton != null)
            {
                MoritzMediaPlayer player = _performingButton.Tag as MoritzMediaPlayer;
                player.StopPlaying();
                Thread.Sleep(400);
            }
            _performingButton = null;
        }

        public void ReadAudioFiles(XmlReader r)
        {
            Debug.Assert(_audioSampleButtons != null && _audioSampleButtons.Count > 0);
            Debug.Assert(r.Name == "audioFiles");

            int buttonIndex = 0;
            M.ReadToXmlElementTag(r, "file");
            while(r.Name == "file")
            {
                if(r.Name == "file" && r.NodeType != XmlNodeType.EndElement)
                {
                    Button button = _audioSampleButtons[buttonIndex];
                    Debug.Assert(button != null);
                    MoritzMediaPlayer player = button.Tag as MoritzMediaPlayer;
                    Debug.Assert(player != null);
                    string fileName = r.ReadElementContentAsString();
                    if(!String.IsNullOrEmpty(fileName))
                    {
                        player.URL = _audioFolder + @"\" + fileName;
                        _savedPlayerFilenames.Add(fileName);
                        SetLinkedButtonsImage(button, global::Moritz.Globals.Properties.Resources.start23x20);
                    }
                    else
                    {
                        _savedPlayerFilenames.Add("");
                    }
                    ++buttonIndex;
                }
                M.ReadToXmlElementTag(r, "audioFiles", "file");
            }
        }
        List<string> _savedPlayerFilenames = new List<string>();
        internal void RevertAudioButtonsToSaved()
        {
            for(int i = 0; i < _audioSampleButtons.Count; ++i)
            {
                Button button = _audioSampleButtons[i];
                Debug.Assert(button != null);
                MoritzMediaPlayer player = button.Tag as MoritzMediaPlayer;
                Debug.Assert(player != null);
                if(i < _savedPlayerFilenames.Count)
                {
                    string fileName = _savedPlayerFilenames[i];
                    if(!String.IsNullOrEmpty(fileName))
                    {
                        player.URL = _audioFolder + @"\" + fileName;
                        SetLinkedButtonsImage(button, global::Moritz.Globals.Properties.Resources.start23x20);
                    }
                    else
                    {
                        player.URL = "";
                        SetLinkedButtonsImage(button, global::Moritz.Globals.Properties.Resources.noFile23x20);
                    }
                }
                else
                {
                    player.URL = "";
                    SetLinkedButtonsImage(button, global::Moritz.Globals.Properties.Resources.noFile23x20);
                }
            }
        }
        public void WriteAudioFiles(XmlWriter w)
        {
            bool audioFilesExist = false;

            foreach(Button button in _audioSampleButtons)
            {
                MoritzMediaPlayer player = button.Tag as MoritzMediaPlayer;
                Debug.Assert(player != null);
                if(!(String.IsNullOrEmpty(player.URL)))
                {
                    audioFilesExist = true;
                    break;
                }

            }
            if(audioFilesExist)
            {
                w.WriteStartElement("audioFiles");
                foreach(Button button in _audioSampleButtons)
                {
                    MoritzMediaPlayer player = button.Tag as MoritzMediaPlayer;
                    Debug.Assert(player != null);
                    w.WriteStartElement("file");
                    if(String.IsNullOrEmpty(player.URL))
                        w.WriteString("");
                    else
                        w.WriteString(Path.GetFileName(player.URL));
                    w.WriteEndElement(); // "file"
                }
                w.WriteEndElement(); // "audioFiles"
            }
        }

        private void AddPaletteButtons(int domain)
        {
            _audioSampleButtons = new List<Button>();
            int x = 0;
            int y = 0;
            for(int i = 0; i < domain; ++i)
            {
                CreateMidiChordFormButton(x, y, i);
                CreateMidiEventDemoButton(x, y + 25, i);
                CreateAudioSampleButton(x, y + 50, i);
                x += 33;
            }
        }
        private void CreateMidiChordFormButton(int x, int y, int i)
        {
            Button b = new Button();
            b.Location = new System.Drawing.Point(x, y);
            b.Size = new System.Drawing.Size(27, 24);
            b.Tag = i;
            //b.UseVisualStyleBackColor = false;
            b.Text = (i + 1).ToString();

            b.MouseDown += new MouseEventHandler(PaletteChordFormButton_MouseDown);

            this.Controls.Add(b);
            _paletteChordFormButtons.Add(b);
            b.BringToFront();
        }
        private void CreateMidiEventDemoButton(int x, int y, int i)
        {
            Label label = new Label();
            label.ForeColor = System.Drawing.Color.RoyalBlue;
            label.Location = new System.Drawing.Point(x + 1, y + 4);
            label.Size = new System.Drawing.Size(26, 14);
            label.Text = "rest";

            this.Controls.Add(label);
            _restLabels.Add(label);
            label.SendToBack();

            Button b = new Button();
            b.Location = new System.Drawing.Point(x, y);
            b.Size = new System.Drawing.Size(27, 24);
            //b.TabIndex = 82 + i;
            b.Text = (i + 1).ToString();
            b.UseVisualStyleBackColor = true;

            b.Click += new EventHandler(MidiEventDemoButton_Click);

            this.Controls.Add(b);
            _midiEventDemoButtons.Add(b);
            b.BringToFront();
        }
        private void CreateAudioSampleButton(int x, int y, int i)
        {
            Button b = new Button();
            b.Location = new System.Drawing.Point(x, y);
            b.Size = new System.Drawing.Size(27, 24);
            b.Tag = new MoritzMediaPlayer(PlayerHasStopped); // the associated media player
            b.Image = global::Moritz.Globals.Properties.Resources.noFile23x20;
            b.UseVisualStyleBackColor = false;

            b.MouseDown += new MouseEventHandler(AudioSampleButton_MouseDown);

            this.Controls.Add(b);
            _audioSampleButtons.Add(b);
            b.BringToFront();
        }

        #region playMidiEvent
        private void MidiEventDemoButton_Click(object sender, EventArgs e)
        {
            Button button = sender as Button;

            if(button != null)
            {

                PaletteForm paletteForm = this._paletteForm as PaletteForm;
                Palette palette = null;
                IUniqueDef iud = null;
                OutputDevice outputDevice = M.Preferences.GetMidiOutputDevice(M.Preferences.PreferredOutputDevice);
                int midiChannel = 0;

                int index = int.Parse(button.Text) - 1;
                if(index >= 0 && index < _midiEventDemoButtons.Count)
                {
                    if(paletteForm != null)
                    {
                        palette = new Palette(paletteForm);
                        if(palette.IsPercussionPalette)
                        {
                            outputDevice = M.Preferences.GetMidiOutputDevice("Microsoft GS Wavetable Synth");
                            midiChannel = 9;
                        }
                    }

                    iud = palette.UniqueDurationDef(index);

                    if(iud is RestDef)
                    {
                        _midiEventDemoButtons[index].Hide();
                        this._restLabels[0].Select(); // just to deselect everything
                        Refresh(); // shows "rest" behind button
                        Thread.Sleep(iud.MsDuration);
                        _midiEventDemoButtons[index].Show();
                        Refresh();
                    }
                    else
                    {
                        _midiEventDemoButtons[index].Select();
                        Refresh();
                        MidiChordDef midiChordDef = iud as MidiChordDef;
                        Debug.Assert(midiChordDef != null);
                        MidiChord midiChord = new MidiChord(midiChannel, midiChordDef, outputDevice);
                        midiChord.Send(); //sends in this thread (blocks the current thread -- keeping the button selected)
                    }
                }
            }
        }
        #endregion

        private void PaletteChordFormButton_MouseDown(object sender, MouseEventArgs e)
        {
            Button button = sender as Button;
            Debug.Assert(button != null);

            if(button != null)
            {
                _paletteForm.ShowPaletteChordForm((int) button.Tag);
            }
        }

        internal void AudioSampleButton_MouseDown(object sender, MouseEventArgs e)
        {
            Form thisForm = sender as Form;
            if(thisForm != null)
            {
                StopCurrentMediaPlayer(); // a click on either the paletteForm or the paletteChordForm stops the performance
                return;
            }

            Button button = sender as Button;
            Debug.Assert(button != null);
            MoritzMediaPlayer player = button.Tag as MoritzMediaPlayer;
            Debug.Assert(player != null);

            if(button != null)
            {
                // Don't call StopMediaPlayer yet: It sets the value of _performingButton.
                if(e.Button == MouseButtons.Right
                || e.Button == MouseButtons.Left && String.IsNullOrEmpty(player.URL))
                {
                    StopCurrentMediaPlayer();
                    GetAudioFileName(button);
                    _paletteForm.SetSettingsHaveChanged();
                }
                else if(e.Button == MouseButtons.Left)
                {
                    if(button == _performingButton)
                    {
                        StopCurrentMediaPlayer(); // sets _performingButton to null
                    }
                    else
                    {
                        StopCurrentMediaPlayer();
                        PlayAudioEvent(button); // sets _performingButton
                    }
                }
                else if(e.Button == MouseButtons.Middle)
                {
                    ShowFileInfo(button, player);
                }
            }
        }

        private void ShowFileInfo(Button button, MoritzMediaPlayer player)
        {
            int buttonNumber = 1;
            foreach(Button b in _audioSampleButtons)
            {
                if(button == b)
                    break;
                else
                    ++buttonNumber;
            }

            string title = "Audio button " + buttonNumber.ToString();
            string message = null;
            if(String.IsNullOrEmpty(player.URL))
                message = "No file.";
            else
                message = Path.GetFileName(player.URL);
            MessageBox.Show(message, title);
        }

        private void PlayAudioEvent(Button button)
        {
            _performingButton = button;
            if(_performingButton != null)
            {
                MoritzMediaPlayer player = button.Tag as MoritzMediaPlayer;
                player.Play();
                SetLinkedButtonsImage(_performingButton, global::Moritz.Globals.Properties.Resources.loudspeaker23x20);
            }
        }

        private Button _performingButton = null;

        /// <summary>
        /// Called (as a delegate) by MoritzMediaPlayer when it changes to the stopped state.
        /// </summary>
        private void PlayerHasStopped()
        {
            if(_performingButton != null)
            {
                SetLinkedButtonsImage(_performingButton, global::Moritz.Globals.Properties.Resources.start23x20);
                _performingButton = null;
                _chordFormButton = null;
            }
        }

        /// <summary>
        /// returns the name of the audio file (without its path).
        /// If there is no audio directory in the score's base folder, an error message goes up.
        /// The file must be in the "audio" directory
        /// </summary>
        /// <param name="button"></param>
        /// <returns></returns>
        private void GetAudioFileName(Button button)
        {
            if(!Directory.Exists(_audioFolder))
            {
                MessageBox.Show("The audio folder does not exist.\n\n" +
                                "Audio files must be in an \"audio\" folder in the score's root folder.");
                return;
            }

            try
            {
                using(OpenFileDialog openFileDialog = new OpenFileDialog())
                {
                    openFileDialog.InitialDirectory = _audioFolder;
                    openFileDialog.Title = "Load Audio File";
                    openFileDialog.RestoreDirectory = true;

                    if(openFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        ((MoritzMediaPlayer)button.Tag).URL = openFileDialog.FileName;
                        SetLinkedButtonsImage(button, global::Moritz.Globals.Properties.Resources.start23x20);
                    }
                }
            }
            catch(Exception ae)
            {
                MessageBox.Show(ae.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        /// <summary>
        /// Called from an open PaletteChordForm, when a Button linked to an AudioSampleButton in
        /// this PaletteButtonsControl is clicked to start the audio playing.
        /// The _chordFormButton is set to null again when the audio stops.
        /// </summary>
        internal void SetLinkedButton(Button chordFormButton)
        {
            _chordFormButton = chordFormButton;
        }

        private void SetLinkedButtonsImage(Button localButton, Bitmap image)
        {
            localButton.SuspendLayout();
            localButton.Image = image;
            localButton.ResumeLayout();
            if(_chordFormButton != null)
            {
                _chordFormButton.SuspendLayout();
                _chordFormButton.Image = image;
                _chordFormButton.ResumeLayout();
            }
        }

        private readonly PaletteForm _paletteForm;
        private readonly string _audioFolder;

        public List<Button> AudioSampleButtons { get { return _audioSampleButtons; } }
        private List<Button> _audioSampleButtons = null;
        public List<Button> PaletteChordFormButtons { get { return _paletteChordFormButtons; } }
        private List<Button> _paletteChordFormButtons = new List<Button>();
        private List<Button> _midiEventDemoButtons = new List<Button>();
        private List<Label> _restLabels = new List<Label>();

        /// <summary>
        /// If _chordFormButton is non-null, its value is used while performing audio samples,
        /// to change the appearance of the corresponding button in the PaletteChordForm.
        /// </summary>
        private Button _chordFormButton = null;
    }
}
