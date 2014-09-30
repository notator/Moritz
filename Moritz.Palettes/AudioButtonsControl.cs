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
    public partial class AudioButtonsControl : UserControl
    {
        internal AudioButtonsControl(int domain, Point location, IPaletteForm iPaletteForm, string audioFolder)
        {
            InitializeComponent();

            _iPaletteForm = iPaletteForm;
            _audioFolder = audioFolder;
            this.SuspendLayout();
            this.Location = location;
            this.Size = new Size(domain * 33, 50);
            AddAudioButtons(domain);
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
            Debug.Assert(_audioSampleButtons != null && _audioSampleButtons.Length > 0);
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
                        button.SuspendLayout();
                        button.Image = global::Moritz.Globals.Properties.Resources.start23x20;
                        button.ResumeLayout();
                    }
                    ++buttonIndex;
                }
                M.ReadToXmlElementTag(r, "audioFiles", "file");
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

        private void AddAudioButtons(int domain)
        {
            _audioSampleButtons = new Button[domain];
            int x = 0;
            int y = 0;
            for(int i = 0; i < domain; ++i)
            {
                CreateMidiEventDemoButton(x, y, i);
                CreateAudioSampleButton(x, y + 25, i);
                x += 33;
            }
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
            _audioSampleButtons[i] = b;
            b.BringToFront();
        }

        #region playMidiEvent
        private void MidiEventDemoButton_Click(object sender, EventArgs e)
        {
            Button button = sender as Button;
            if(button != null)
            {
                int index = int.Parse(button.Text) - 1;
                if(index >= 0 && index < _midiEventDemoButtons.Count)
                {
                    PaletteForm paletteForm = this._iPaletteForm as PaletteForm;
                    Palette palette = new Palette(paletteForm);
                    IUniqueDef iud = palette.UniqueDurationDef(index);

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
                        int midiChannel = 0;
                        if(_iPaletteForm is PercussionPaletteForm)
                            midiChannel = 9;
                        _midiEventDemoButtons[index].Select();
                        Refresh();
                        MidiChordDef midiChordDef = iud as MidiChordDef;
                        Debug.Assert(midiChordDef != null);
                        MidiChord midiChord = new MidiChord(midiChannel, midiChordDef);
                        midiChord.Send(); //sends in this thread
                        //// This blocks the current thread (keeps the button selected)
                        //Thread.Sleep(iud.MsDuration);
                    }
                }
            }
        }
        #endregion

        private void AudioSampleButton_MouseDown(object sender, MouseEventArgs e)
        {
            Form thisForm = sender as Form;
            if(thisForm != null)
            {
                StopCurrentMediaPlayer();
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
                _performingButton.SuspendLayout();
                _performingButton.Image = global::Moritz.Globals.Properties.Resources.loudspeaker23x20;
                _performingButton.ResumeLayout();
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
                _performingButton.SuspendLayout();
                _performingButton.Image = global::Moritz.Globals.Properties.Resources.start23x20;
                _performingButton.ResumeLayout();
                _performingButton = null;
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
                        button.SuspendLayout();
                        button.Image = global::Moritz.Globals.Properties.Resources.start23x20;
                        button.ResumeLayout();
                    }
                }
            }
            catch(Exception ae)
            {
                MessageBox.Show(ae.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private readonly IPaletteForm _iPaletteForm;
        private readonly string _audioFolder;

        private Button[] _audioSampleButtons = null;
        private List<Button> _midiEventDemoButtons = new List<Button>();
        private List<Label> _restLabels = new List<Label>();
    }
}
