using System;
using System.Collections.Generic;
using System.Drawing;
using System.Diagnostics;
using System.Windows.Forms;
using System.IO;
using System.Text.RegularExpressions;

using Moritz.Globals;
using Moritz.Score;

namespace Moritz.AssistantPerformer
{
    public partial class AssistantPerformerMainForm : Form
    {
        public AssistantPerformerMainForm(string scorePathname, IMoritzForm1 moritzForm1)
        {
            InitializeComponent();

            _moritzForm1 = moritzForm1; // closed when this form is closed

            _midiInputDevice = new MidiInputDevice(M.Preferences.CurrentMultimediaMidiInputDevice);
            _midiOutputDevice = new MidiOutputDevice(M.Preferences.CurrentMultimediaMidiOutputDevice);

            _algorithmFolder = M.AlgorithmFolder(scorePathname);
            _scoreFolder = Path.GetDirectoryName(scorePathname);

            _scoreFilenameWithoutExtension = Path.GetFileNameWithoutExtension(scorePathname);
            
            _svgScore = new SvgScore(scorePathname);

            _numberOfVoices = 0;
            foreach(Staff staff in _svgScore.Systems[0].Staves)
                foreach(Voice voice in staff.Voices)
                    ++_numberOfVoices;

            PopulateMpoxFilenamesListBox(this.MpoxFilenamesComboBox);
            InitializeMoritzPlayersPanel();
            if(_po == null)
            {
                _po = new MoritzPerformanceOptions(M.Preferences, _scoreFolder + "\\" + ((string)this.MpoxFilenamesComboBox.SelectedItem), _svgScore);
            }

            _po.Save();

            SetOptionsView();
            SetWindowLayout();
            // the following were set to "0" in the designer, so that they are selectable when designing.
            MomentNumberLabel.Text = "";
            MomentPositionLabel.Text = "";
        }        
        #region constructor helper functions

        private int MpoxFilenameNumber(string mpoxFileName)
        {
            int mpoxFileNameNumber = -1;
            // the following Regex expression does not check the characters at the beginning for their validity as Windows file names.
            // A legal .mpox filename is here a sequence of ANY characters, followed by a '.', followed by an integer, followed by ".mpox". 
            if(mpoxFileName != null && Regex.IsMatch(mpoxFileName, @"^.+[.][0-9]+[.][m][p][o][x]$"))
            {
                string substring = mpoxFileName.Substring(0, mpoxFileName.LastIndexOf('.'));
                substring = substring.Substring(substring.LastIndexOf('.') + 1);
                mpoxFileNameNumber = int.Parse(substring);
            }
            else
            {
                throw new ApplicationException("Illegal .mpox file name.");
            }
            return mpoxFileNameNumber;
        }
        /// <summary>
        /// Used when sorting .mpox files.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        private int CompareMpoxFilesByNumber(string x, string y)
        {
            Debug.Assert((!String.IsNullOrEmpty(x)) && (!String.IsNullOrEmpty(y)));
            int xNumber = MpoxFilenameNumber(x);
            int yNumber = MpoxFilenameNumber(y);
            return (xNumber.CompareTo(yNumber));
        }
        private void PopulateMpoxFilenamesListBox(ComboBox comboBox)
        {
            string[] filePaths = Directory.GetFiles(_scoreFolder);
            List<string> mpoxFiles = new List<string>();
            foreach(string filePath in filePaths)
            {
                string fileNameWithNumber = Path.GetFileNameWithoutExtension(filePath);
                string filenameWithoutNumber = Path.GetFileNameWithoutExtension(fileNameWithNumber);
                if(filenameWithoutNumber == _scoreFilenameWithoutExtension
                && Path.GetExtension(filePath) == M.MoritzPerformanceOptionsExtension)
                {
                    mpoxFiles.Add(Path.GetFileName(filePath));
                }
            }

            if(mpoxFiles.Count == 0)
            {
                string mpoxFileName = _scoreFilenameWithoutExtension + ".1" + M.MoritzPerformanceOptionsExtension;
                mpoxFiles.Add(mpoxFileName);
            }

            comboBox.SuspendLayout();
            comboBox.Items.Clear();
            mpoxFiles.Sort(CompareMpoxFilesByNumber);
            foreach(string filename in mpoxFiles)
            {
                comboBox.Items.Add(filename);
            }
            comboBox.ResumeLayout();
            // the following line triggers the MpoxFilenamesComboBox.SelectedIndexChanged event, thus calling
            // MpoxFileNamesComboBox_SelectedIndexChanged() which can create a new MoritzPerformanceOptions (_po).
            comboBox.SelectedIndex = comboBox.Items.Count - 1;           
       }
        /// <summary>
        /// There is one MoritzPlayer per voice, top to bottom in systems
        /// </summary>
        private void InitializeMoritzPlayersPanel()
        {
            MoritzPlayersPanel.SuspendLayout();
            int i = 0;
            foreach(Staff staff in _svgScore.Systems[0].Staves)
            {
                int staffVoiceNumber = 1;
                foreach(Voice voice in staff.Voices)
                {
                    PanelRow row = new PanelRow(new Point(0, i * PanelRow.Height), (i + 1).ToString(),
                        staff.Staffname + ".voice" + staffVoiceNumber.ToString(), _maxInstrumentWidth, SelectedChanged);

                    row.Performer = _po.MoritzPlayers[i];

                    MoritzPlayersPanel.Controls.Add(row);
                    _rows.Add(row);

                    ++staffVoiceNumber;
                    ++i;
                }
            }
            MoritzPlayersPanel.ResumeLayout();
        }
        private void SetWindowLayout()
        {
            this.Text = "Assistant Performer: " + _scoreFilenameWithoutExtension;
            string lastItem = ((string)MpoxFilenamesComboBox.Items[MpoxFilenamesComboBox.Items.Count - 1]);
            Size textSize = TextRenderer.MeasureText(lastItem, MpoxFilenamesComboBox.Font);
            int textWidth = (int)textSize.Width;
            int comboBoxHeight = MpoxFilenamesComboBox.Height;
            int comboBoxWidth = MpoxFilenamesComboBox.Width;
            int thisWidth = this.Width;

            MpoxFilenamesComboBox.Size = new Size(textWidth + 20, comboBoxHeight);
            MpoxFilenamesComboBox.Location = new Point((((this.Width - (textWidth + 20)) / 2) - 3), MpoxFilenamesComboBox.Location.Y);

            #region set _maxInstrumentWidth, moritzPlayersPanelHeight and moritzPlayersPanelWidth
            _maxInstrumentWidth = 0;
            foreach(PanelRow row in _rows)
            {
                textSize = TextRenderer.MeasureText(row.InstrumentName, row.Font);
                _maxInstrumentWidth = (_maxInstrumentWidth > ((int)textSize.Width)) ? _maxInstrumentWidth : (int)textSize.Width;
            }
            foreach(PanelRow row in _rows)
            {
                row.SetWidthForInstrumentWidth(_maxInstrumentWidth);
            }

            int moritzPlayersPanelHeight = 0;
            int moritzPlayersPanelWidth = 0;
            if(_rows.Count > 6)
            {
                moritzPlayersPanelHeight = PanelRow.Height * 6;
                moritzPlayersPanelWidth = _rows[0].Width + 2 + 17;
            }
            else if(_rows.Count > 0)
            {
                moritzPlayersPanelHeight = PanelRow.Height * MoritzPlayersPanel.Controls.Count + 3;
                moritzPlayersPanelWidth = _rows[0].Width + 2;
            }
            #endregion

            MoritzPlayersPanel.Size = new Size(moritzPlayersPanelWidth, moritzPlayersPanelHeight);
            MoritzPlayersPanel.Location = new Point(((this.Width - moritzPlayersPanelWidth) / 2) - 3, MoritzPlayersPanel.Top + 2);

            BottomControlsPanel.Top = MoritzPlayersPanel.Bottom + 8;
  
            this.Size = new Size(this.Width + 2, BottomControlsPanel.Top + BottomControlsPanel.Height + 28);
        }
        #endregion constructor helper functions
        #region internal events
        protected override void OnPaint(PaintEventArgs e)
        {
            if(_po != null)
            {
                if(_isPerforming)
                    DoEnabledWhilePerforming();
                else
                    DoEnabledWhileNotPerforming();

                this.SpeedFactorTextBox.Text = _po.SpeedFactor.ToString(M.En_USNumberFormat);
                this.MinimumOrnamentChordDurationTextBox.Text = _po.MinimumOrnamentChordMsDuration.ToString(M.En_USNumberFormat);
            }

        }

        /// <summary>
        /// Displays the score in the program currently set on this computer to display files
        /// having the score's type (.html or .svg).
        /// </summary>
        private void ShowScoreButton_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(_svgScore.FilePath);
        }
        private void ShowMoritzButton_Click(object sender, EventArgs e)
        {
            _moritzForm1.Show();
        }
        private void QuitAssistantPerformerButton_Click(object sender, EventArgs e)
        {
            _moritzForm1.CloseAssistantPerformer();
        }

        private void QuitMoritzButton_Click(object sender, EventArgs e)
        {
            _moritzForm1.Close();
        }

        //private void AssistantPerformerMainForm_FormClosing(object sender, FormClosingEventArgs e)
        //{
        //    _moritzForm1.Close(); // disposes preferences and devices
        //}

        /// <summary>
        /// Helper function for OnPaint().
        /// Called to set the dialog's appearance when the Assistant performer is performing.
        /// </summary>
        void DoEnabledWhilePerforming()
        { 
            foreach(Control control in Controls)
            {
                control.Enabled = false;
            }

            MomentNumberCommentLabel.Enabled = true;
            MomentNumberLabel.Enabled = true;
            MomentPositionCommentLabel.Enabled = true;
            MomentPositionLabel.Enabled = true;

            BottomControlsPanel.Enabled = true;
            StopButton.Enabled = true;
            StopButton.Visible = true;
            //StopButton.BringToFront();
            GlobalOptionsGroupBox.Enabled = false;
            AssistantsDurationsOptionsGroupBox.Enabled = false;
            PerformersVelocityOptionsGroupBox.Enabled = false;
            PerformersPitchOptionsGroupBox.Enabled = false;
            KeyboardOptionsButton.Enabled = false;

            NewMpoxFileButton.Enabled = false;
            PerformLiveButton.Visible = false;
            PlaySymbolsButton.Visible = false;
            DeleteMpoxFileButton.Enabled = false;

            SelectAllButton.Enabled = false;

            ShowScoreButton.Enabled = false;
            QuitAssistantPerformerButton.Enabled = false;
            ShowMoritzButton.Enabled = false;
            QuitMoritzButton.Enabled = false;
        }
        #region helper functions for DoEnabledWhileNotPerforming()
        private bool Has(MoritzPlayer moritzPlayer)
        {
            bool returnValue = false;
            foreach(MoritzPlayer player in _po.MoritzPlayers)
            {
                if(player == moritzPlayer)
                {
                    returnValue = true;
                    break;
                }
            }
            return returnValue;

        }
        private bool HasLivePerformer
        {
            get
            {
                return Has(MoritzPlayer.LivePerformer);
            }
        }
        private bool HasAssistant
        {
            get
            {
                return Has(MoritzPlayer.Assistant);
            }
        }
        #endregion // helper functions for DoEnabledWhileNotPerforming()
        /// <summary>
        /// Helper function for OnPaint().
        /// Called to set the dialog's appearance when the Assistant performer is not performing.
        /// </summary>
        private void DoEnabledWhileNotPerforming()
        {
            StartMomentLabel.Enabled = true;
            StartMomentTextBox.Enabled = true;
            RepeatCheckBox.Enabled = true;
            SaveMidiCheckBox.Enabled = true;

            StopButton.Visible = false;
            NewMpoxFileButton.Enabled = true;
            PerformLiveButton.Visible = true;
            PlaySymbolsButton.Visible = true;
            DeleteMpoxFileButton.Enabled = true;
            SelectAllButton.Enabled = true;

            MomentNumberCommentLabel.Enabled = false;
            MomentNumberLabel.Enabled = false;
            MomentPositionCommentLabel.Enabled = false;
            MomentPositionLabel.Enabled = false;

            this.GlobalOptionsGroupBox.Enabled = true;
            this.MinimumOrnamentChordDurationTextBox.Text = _po.MinimumOrnamentChordMsDuration.ToString(M.En_USNumberFormat);

            MpoxFilenamesComboBox.Enabled = true;
            MoritzPlayersPanel.Enabled = true;
            BottomControlsPanel.Enabled = true;
            #region has live performer
            if(this.HasLivePerformer)
            {
                PerformersVelocityOptionsGroupBox.Enabled = true;
                PerformersPitchOptionsGroupBox.Enabled = true;
                KeyboardOptionsButton.Enabled = true;
                PerformLiveButton.Enabled = true;
                #region also has assistant
                if(this.HasAssistant)
                {
                    AssistantsDurationsOptionsGroupBox.Enabled = true;
                    this.SpeedFactorTextBox.Text = _po.SpeedFactor.ToString(M.En_USNumberFormat);
                    AssistantsDurationsSymbolsRelativeRadioButton.Enabled = true; 
                }
                else
                {
                    AssistantsDurationsOptionsGroupBox.Enabled = false;
                }
                #endregion

            }
            else if(this.HasAssistant)
            {
                PerformersVelocityOptionsGroupBox.Enabled = false;
                PerformersPitchOptionsGroupBox.Enabled = false;
                KeyboardOptionsButton.Enabled = false;
                PerformLiveButton.Enabled = false;
                AssistantsDurationsOptionsGroupBox.Enabled = true;
                this.SpeedFactorTextBox.Text = _po.SpeedFactor.ToString(M.En_USNumberFormat);
                // no live performer here
                AssistantsDurationsSymbolsRelativeRadioButton.Enabled = false;
                AssistantsDurationsSymbolsRelativeRadioButton.Checked = false;
                AssistantsDurationsSymbolsAbsoluteRadioButton.Checked = true;
            }
            #endregion
            #region single buttons and radio buttons
            SilentButton.Enabled = true;
            PerformerSilentRadioButton.Enabled = true;
            DeleteMpoxFileButton.Enabled = true;
            PlaySymbolsButton.Enabled = true;
            if(_po.MoritzPlayers.Count == 1)
            {
                if(PerformerSilentRadioButton.Checked)
                {
                    this.PlayNotatedDynamicsRadioButton.Checked = true;
                }
                SilentButton.Enabled = false;
                PerformerSilentRadioButton.Enabled = false;
            }
            if(!HasAssistant && !HasLivePerformer)
            {
                PlaySymbolsButton.Enabled = false;
            }
            if(PerformerSilentRadioButton.Checked)
            {
                PerformersPitchOptionsGroupBox.Enabled = false;
            }

            ShowScoreButton.Enabled = true;
            QuitAssistantPerformerButton.Enabled = true;
            ShowMoritzButton.Enabled = true;
            QuitMoritzButton.Enabled = true;

            #endregion

        }
        #region SetOptionsView() helper functions
        /// <summary>
        /// Delegate called by PanelRow. Argument to PanelRow constructor called in SetOptionsView().
        /// </summary>
        /// <param name="row"></param>
        private void SelectedChanged(PanelRow thisRow)
        {
            if(thisRow.IsSelected)
            {
                foreach(PanelRow row in _rows)
                    row.IsSelected = false;
                thisRow.IsSelected = true;
                _selectedRow = thisRow;
                AssistantButton.Enabled = true;
                PerformerButton.Enabled = true;
                if(_rows.Count > 1)
                    SilentButton.Enabled = true;
                else
                    SilentButton.Enabled = false;
            }
            else
            {
                _selectedRow = null;
                AssistantButton.Enabled = false;
                PerformerButton.Enabled = false;
                SilentButton.Enabled = false;
            }
        }
        /// <summary>
        /// helper function for SetOptionsView()
        /// </summary>
        private void SetRadioButtonColours()
        {
            PlayNotatedDynamicsRadioButton.ForeColor = System.Drawing.Color.Black;
            PlayPerformedDynamicsRadioButton.ForeColor = System.Drawing.Color.Black;
            PerformerSilentRadioButton.ForeColor = System.Drawing.Color.Black;
            switch(_po.PerformersDynamicsType)
            {
                case PerformersDynamicsType.AsNotated:
                    PlayNotatedDynamicsRadioButton.ForeColor = System.Drawing.Color.Blue;
                    break;
                case PerformersDynamicsType.AsPerformed:
                    PlayPerformedDynamicsRadioButton.ForeColor = System.Drawing.Color.Blue;
                    break;
                case PerformersDynamicsType.Silent:
                    PerformerSilentRadioButton.ForeColor = System.Drawing.Color.Blue;
                    break;
            }

            PlayNotatedPitchesRadioButton.ForeColor = System.Drawing.Color.Black;
            PlayPerformedPitchesRadioButton.ForeColor = System.Drawing.Color.Black;
            switch(_po.PerformersPitchesType)
            {
                case PerformersPitchesType.AsNotated:
                    PlayNotatedPitchesRadioButton.ForeColor = System.Drawing.Color.Blue;
                    break;
                case PerformersPitchesType.AsPerformed:
                    PlayPerformedPitchesRadioButton.ForeColor = System.Drawing.Color.Blue;
                    break;
            }

            AssistantsDurationsSymbolsAbsoluteRadioButton.ForeColor = System.Drawing.Color.Black;
            AssistantsDurationsSymbolsRelativeRadioButton.ForeColor = System.Drawing.Color.Black;
            SpeedFactorTextBox.ForeColor = System.Drawing.Color.Black;
            SpeedFactorLabel.ForeColor = System.Drawing.Color.Black;
            switch(_po.AssistantsDurationsType)
            {
                case AssistantsDurationsType.SymbolsAbsolute:
                    AssistantsDurationsSymbolsAbsoluteRadioButton.ForeColor = System.Drawing.Color.Blue;
                    SpeedFactorTextBox.ForeColor = System.Drawing.Color.Blue;
                    SpeedFactorLabel.ForeColor = System.Drawing.Color.Blue; 
                    break;
                case AssistantsDurationsType.SymbolsRelative:
                    AssistantsDurationsSymbolsRelativeRadioButton.ForeColor = System.Drawing.Color.Blue;
                    break;
            }
        }
        #endregion // SetOptionsView() helper functions
        /// <summary>
        /// Sets all the controls in this dialog (except the mpoxFilesComboBox) to the values in _po,
        /// then deselects mpoxFilesComboBox and calls Refresh().
        /// Called internally, and by AssistantPerformer after reallocating Performance Data (before recording from symbols).
        /// </summary>
        private void SetOptionsView()
        {
            this.Text = "Assistant Performer: " + _scoreFilenameWithoutExtension;

            this.StartMomentTextBox.Text = _po.StartAtMoment.ToString();

            SetForPerformerOrAssistantsMoments();

            this.RepeatCheckBox.Checked = _po.RepeatPerformance;
            this.SaveMidiCheckBox.Checked = _po.SaveMidiFile;
            this.MinimumOrnamentChordDurationTextBox.Text = _po.MinimumOrnamentChordMsDuration.ToString();

            #region radio buttons
            if(_po.PerformersPitchesType == PerformersPitchesType.AsNotated)
                this.PlayNotatedPitchesRadioButton.Checked = true;
            else if(_po.PerformersPitchesType == PerformersPitchesType.AsPerformed)
                this.PlayPerformedPitchesRadioButton.Checked = true;

            PerformersPitchOptionsGroupBox.Enabled = true;
            if(_po.PerformersDynamicsType == PerformersDynamicsType.AsNotated)
                this.PlayNotatedDynamicsRadioButton.Checked = true;
            else if(_po.PerformersDynamicsType == PerformersDynamicsType.AsPerformed)
                this.PlayPerformedDynamicsRadioButton.Checked = true;
            else if(_po.PerformersDynamicsType == PerformersDynamicsType.Silent)
                this.PerformerSilentRadioButton.Checked = true;

            if(_po.AssistantsDurationsType == AssistantsDurationsType.SymbolsAbsolute)
            {
                this.AssistantsDurationsSymbolsAbsoluteRadioButton.Checked = true;
                this.SpeedFactorTextBox.Enabled = true;
                this.SpeedFactorTextBox.Text = _po.SpeedFactor.ToString(M.En_USNumberFormat);
            }
            else if(_po.AssistantsDurationsType == AssistantsDurationsType.SymbolsRelative)
            {
                this.AssistantsDurationsSymbolsRelativeRadioButton.Checked = true;
                this.SpeedFactorTextBox.Enabled = false;
                this.SpeedFactorTextBox.Text = "";
                this.SpeedFactorTextBox.BackColor = Color.White;
                _assistantsDurationFactorError = false;
            }

            SetRadioButtonColours();

            #endregion // radio buttons

            #region MoritzPlayers control
            this.MoritzPlayersPanel.SuspendLayout();
            int i = 0;
            foreach(Control control in MoritzPlayersPanel.Controls)
            {
                PanelRow panelRow = control as PanelRow;
                if(panelRow != null)
                {
                    panelRow.Performer = _po.MoritzPlayers[i];
                    i++;
                }
            }
            MoritzPlayersPanel.ResumeLayout();

            AssistantButton.Enabled = false;
            PerformerButton.Enabled = false;
            SilentButton.Enabled = false;
            #endregion

            DeselectMpoxFilesComboBox();
            Refresh();
        }

        /// <summary>
        /// The number of moments can change if a performed voice is set not to be played
        /// or if an unplayed voice is set to be performed...
        /// </summary>
        private void SetForPerformerOrAssistantsMoments()
        {
            MidiScore midiScore = new MidiScore(_svgScore, _po, false);
            if(midiScore.PerformersMoments.Count > 0)
            {
                this.StartMomentLabel.Text = "Start at performer's moment";
                this.MomentNumberCommentLabel.Text = "performer's moment number:";
                MomentNumberCommentLabel.Location = new Point(76, MomentNumberCommentLabel.Location.Y);
                SetPositionMomentNumberDict(midiScore.PerformersMoments);
            }
            else
            {
                this.StartMomentLabel.Text = "Start at assistant's moment";
                this.MomentNumberCommentLabel.Text = "assistant's moment number:";
                MomentNumberCommentLabel.Location = new Point(79, MomentNumberCommentLabel.Location.Y);
                SetPositionMomentNumberDict(midiScore.AssistantsMoments);
            }
            midiScore = null;
            int numberOfMoments = _positionMomentNumberDict.Count;
            this.MomentsRangeLabel.Text = "[ 1.." + numberOfMoments.ToString() + " ]";
            int currentStartMoment = int.Parse(StartMomentTextBox.Text);
            if(currentStartMoment > numberOfMoments)
            {
                StartMomentTextBox.BackColor = Color.Pink;
                StartMomentTextBox.ForeColor = Color.Black;
                _startMomentError = true;
            }
            else
            {
                StartMomentTextBox.BackColor = Color.White;
                StartMomentTextBox.ForeColor = Color.Blue;
                _startMomentError = false;
            }
        }
        private void SetPositionMomentNumberDict(List<Moritz.Score.Midi.MidiMoment> moments)
        {
            _positionMomentNumberDict.Clear();
            for(int momentIndex = 0; momentIndex < moments.Count; ++momentIndex)
            {
                _positionMomentNumberDict.Add(moments[momentIndex].MsPosition, momentIndex + 1);
            }
        }

        private Dictionary<int, int> _positionMomentNumberDict = new Dictionary<int, int>();
        #endregion internal events

        #region functions called as delegates by AssistantPerformerRuntime
        private void ReportPerformersPosition(int momentPosition)
        {
            if(InvokeRequired)
            {
                MethodInvoker report = delegate 
                {
                    Report(momentPosition);
                };
                Invoke(report);
            }
            else
                Report(momentPosition);
        }
        private void Report(int momentPosition)
        {
            if(_positionMomentNumberDict.ContainsKey(momentPosition))
            {
                this.MomentNumberLabel.Text = _positionMomentNumberDict[momentPosition].ToString();
                this.MomentPositionLabel.Text = momentPosition.ToString();
            }
        }

        private void SaveSequenceAsMidiFile(Multimedia.Midi.Sequence sequence, string defaultFilename)
        {
            if(InvokeRequired)
            {
                MethodInvoker saveMidi = delegate
                {
                    SaveMidiFile(sequence, defaultFilename);
                };
                Invoke(saveMidi);
            }
            else
                SaveMidiFile(sequence, defaultFilename);
        }
        private void SaveMidiFile(Multimedia.Midi.Sequence sequence, string defaultFilename)
        {
            string midiFolder = _algorithmFolder + "\\midi";
            M.CreateDirectoryIfItDoesNotExist(midiFolder);
            using(SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.InitialDirectory = midiFolder;
                sfd.AddExtension = true;
                sfd.Filter = "MIDI files (*.mid)|*.mid";
                sfd.FilterIndex = (int)0;
                sfd.Title = "Save MIDI File As...";
                sfd.RestoreDirectory = true;
                sfd.FileName = defaultFilename;

                if(sfd.ShowDialog() == DialogResult.OK)
                {
                    sequence.Save(sfd.FileName);
                }
            }
        }
        #endregion
        #region buttons
        #region helper functions for buttons
        /// <summary>
        /// Helper function for all buttons.
        /// If a recording is present, it is deleted. Current options are saved, and SetOptionsView() is called.
        /// </summary>
        private void DoOptionsHaveChanged()
        {
            if(this.IsHandleCreated)
            {
                _po.Save();
                SetOptionsView();
            }
        }
        #endregion // helper functions for buttons
        #region .mpox combobox
        private void MpoxFileNamesComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(this.MpoxFilenamesComboBox.SelectedItem != null)
            {
                _po = new MoritzPerformanceOptions(M.Preferences, _scoreFolder + "\\" + ((string)this.MpoxFilenamesComboBox.SelectedItem), _svgScore);
                this.SetOptionsView();
            }
        }

        private void DeselectMpoxFilesComboBox()
        {
            this.MoritzPlayersPanel.Focus();
            this.Refresh();
        }
        #endregion  // .mpox combobox
        #region MoritzPlayerListBox buttons
        private void SelectAllButton_Click(object sender, EventArgs e)
        {
            foreach(PanelRow row in _rows)
                row.IsSelected = true;
            AssistantButton.Enabled = true;
            PerformerButton.Enabled = true;
            if(_rows.Count > 1)
                SilentButton.Enabled = true;
            else
                SilentButton.Enabled = false;
        }
        private void SetSelectedRowsTo(MoritzPlayer moritzPlayer)
        {
            for(int i = 0; i < _rows.Count; i++)
            {
                if(_rows[i].IsSelected)
                {
                    if(_rows[i].Performer != moritzPlayer)
                    {
                        _rows[i].Performer = moritzPlayer;
                        _po.MoritzPlayers[i] = moritzPlayer;
                    }
                }
            }
            DoOptionsHaveChanged();
        }
        private void AssistantButton_Click(object sender, EventArgs e)
        {
            SetSelectedRowsTo(MoritzPlayer.Assistant);
            SetForPerformerOrAssistantsMoments();
        }
        private void PerformerButton_Click(object sender, EventArgs e)
        {
            if(IsFirstPerformer)
                _po.AssistantsDurationsType = AssistantsDurationsType.SymbolsAbsolute; // default
            SetSelectedRowsTo(MoritzPlayer.LivePerformer);
            SetForPerformerOrAssistantsMoments();
        }
        private bool IsFirstPerformer
        {
            get
            {
                bool isFirstPerformer = true;
                for(int i = 0; i < _rows.Count; i++)
                {
                        if(_rows[i].Performer == MoritzPlayer.LivePerformer)
                        {
                            isFirstPerformer = false;
                            break;
                        }
                }
                return isFirstPerformer;
            }
        }
        private void SilentButton_Click(object sender, EventArgs e)
        {
            SetSelectedRowsTo(MoritzPlayer.None);
            SetForPerformerOrAssistantsMoments();
        }
        private void PerformanceOptionsDialog_Click(object sender, EventArgs e)
        {
            foreach(PanelRow row in _rows)
                row.IsSelected = false;
            AssistantButton.Enabled = false;
            PerformerButton.Enabled = false;
            SilentButton.Enabled = false;
        }
        #endregion // MoritzPlayerListBox buttons
        #region performer's dynamics radio buttons
        private void ResetPerformersDynamicsRadioButtons(PerformersDynamicsType performersDynamicsType)
        {
            switch(performersDynamicsType)
            {
                case PerformersDynamicsType.AsNotated:
                    PlayNotatedDynamicsRadioButton.Checked = true;
                    break;
                case PerformersDynamicsType.AsPerformed:
                    PlayPerformedDynamicsRadioButton.Checked = true;
                    break;
                case PerformersDynamicsType.Silent:
                    PerformerSilentRadioButton.Checked = true;
                    break;
            }
        }
        private void DoPerformersDynamicsRadioButtons(PerformersDynamicsType performersDynamicsType)
        {
            _po.PerformersDynamicsType = performersDynamicsType;
            DoOptionsHaveChanged();
        }
        private void PlayNotatedDynamicsRadioButton_MouseDown(object sender, MouseEventArgs e)
        {
            DoPerformersDynamicsRadioButtons(PerformersDynamicsType.AsNotated);
        }
        private void PlayPerformedDynamicsRadioButton_MouseDown(object sender, MouseEventArgs e)
        {
            DoPerformersDynamicsRadioButtons(PerformersDynamicsType.AsPerformed);
        }
        private void PerformerSilentRadioButton_MouseDown(object sender, MouseEventArgs e)
        {
            DoPerformersDynamicsRadioButtons(PerformersDynamicsType.Silent);
        }
        #endregion // performer's dynamics radio buttons
        #region performer's pitch buttons
        private void ResetPerformersPitchesRadioButtons(PerformersPitchesType performersPitchesType)
        {
            switch(performersPitchesType)
            {
                case PerformersPitchesType.AsNotated:
                    PlayNotatedPitchesRadioButton.Checked = true;
                    break;
                case PerformersPitchesType.AsPerformed:
                    PlayPerformedPitchesRadioButton.Checked = true;
                    break;
            }
        }
        private void DoPerformersPitchesRadioButtons(PerformersPitchesType performersPitchesType)
        {
            _po.PerformersPitchesType = performersPitchesType;
            DoOptionsHaveChanged();
        }
        private void PlayNotatedPitchesRadioButton_MouseDown(object sender, MouseEventArgs e)
        {
            DoPerformersPitchesRadioButtons(PerformersPitchesType.AsNotated);
        }
        private void PlayPerformedPitchesRadioButton_MouseDown(object sender, MouseEventArgs e)
        {
            DoPerformersPitchesRadioButtons(PerformersPitchesType.AsPerformed);
        }
        #endregion // performer's pitch buttons
        #region performer's keyboard options
        private void KeyboardOptionsButton_Click(object sender, EventArgs e)
        {
            using(KeyboardSettingsDialog dialog = new KeyboardSettingsDialog(_po))
            {
                dialog.ShowDialog();
            }
        }
        #endregion // performer's keyboard options
        #region assistant's durations radio buttons
        private void ResetAssistantsDurationsRadioButtons(AssistantsDurationsType assistantsDurationsType)
        {
            switch(assistantsDurationsType)
            {
                case AssistantsDurationsType.SymbolsAbsolute:
                    AssistantsDurationsSymbolsAbsoluteRadioButton.Checked = true;
                    break;
                case AssistantsDurationsType.SymbolsRelative:
                    AssistantsDurationsSymbolsRelativeRadioButton.Checked = true;
                    break;
            }
        }
        private void DoAssistantsDurationsRadioButtons(AssistantsDurationsType assistantsDurationsType)
        {
            _po.AssistantsDurationsType = assistantsDurationsType;
            DoOptionsHaveChanged();
        }
        private void AssistantsDurationsSymbolsAbsoluteRadioButton_MouseDown(object sender, MouseEventArgs e)
        {
            DoAssistantsDurationsRadioButtons(AssistantsDurationsType.SymbolsAbsolute);
        }
        private void SpeedFactorTextBox_Leave(object sender, EventArgs e)
        {
            try
            {
                SpeedFactorTextBox.Text = SpeedFactorTextBox.Text.Replace(',', '.');
                float factor = float.Parse(this.SpeedFactorTextBox.Text, M.En_USNumberFormat);
                if(factor <= 0f)
                    throw new ApplicationException();
                SpeedFactorTextBox.Text = factor.ToString(M.En_USNumberFormat);
                _po.SpeedFactor = factor;
            }
            catch
            {
                SpeedFactorTextBox.BackColor = Color.Pink;
                SpeedFactorTextBox.ForeColor = Color.Black;
                _po.SpeedFactor = 1;
                _assistantsDurationFactorError = true;
            }
            DoOptionsHaveChanged(); // calls _po.Save()
        }
        private void SpeedFactorTextBox_Enter(object sender, EventArgs e)
        {
            SpeedFactorTextBox.BackColor = Color.White;
            SpeedFactorTextBox.ForeColor = Color.Blue;
            _assistantsDurationFactorError = false;
        }

        private void StartMomentTextBox_Leave(object sender, EventArgs e)
        {
            try
            {
                int startAtMoment = int.Parse(this.StartMomentTextBox.Text, M.En_USNumberFormat);
                if(startAtMoment <= 0 || startAtMoment > _positionMomentNumberDict.Count)
                    throw new ApplicationException();
                StartMomentTextBox.Text = startAtMoment.ToString(M.En_USNumberFormat);
                _po.StartAtMoment = startAtMoment;
            }
            catch
            {
                StartMomentTextBox.BackColor = Color.Pink;
                StartMomentTextBox.ForeColor = Color.Black;
                _po.StartAtMoment = 1;
                _startMomentError = true;
            }
            DoOptionsHaveChanged();
        }
        private void StartMomentTextBox_Enter(object sender, EventArgs e)
        {
            StartMomentTextBox.BackColor = Color.White;
            StartMomentTextBox.ForeColor = Color.Blue;
            _startMomentError = false;
        }

        private void RepeatCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            _po.RepeatPerformance = RepeatCheckBox.Checked;
            DoOptionsHaveChanged();
        }
        private void SaveMidiCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            _po.SaveMidiFile = SaveMidiCheckBox.Checked;
            DoOptionsHaveChanged();
        }

        private void MinimumOrnamentChordDurationTextBox_Leave(object sender, EventArgs e)
        {
            try
            {
                int minDuration = int.Parse(this.MinimumOrnamentChordDurationTextBox.Text, M.En_USNumberFormat);
                if(minDuration <= 0)
                    throw new ApplicationException();
                MinimumOrnamentChordDurationTextBox.Text = minDuration.ToString(M.En_USNumberFormat);
                _po.MinimumOrnamentChordMsDuration = minDuration;
            }
            catch
            {
                MinimumOrnamentChordDurationTextBox.BackColor = Color.Pink;
                MinimumOrnamentChordDurationTextBox.ForeColor = Color.Black;
                _po.MinimumOrnamentChordMsDuration = M.DefaultMinimumBasicMidiChordMsDuration;
                _minimumOrnamentChordMsDurationError = true;
            }
            DoOptionsHaveChanged(); // calls _po.Save()
        }
        private void MinimumOrnamentChordDurationTextBox_Enter(object sender, EventArgs e)
        {
            MinimumOrnamentChordDurationTextBox.BackColor = Color.White;
            MinimumOrnamentChordDurationTextBox.ForeColor = Color.Blue;
            _minimumOrnamentChordMsDurationError = false;
        }

        private void AssistantsDurationsSymbolsRelativeRadioButton_MouseDown(object sender, MouseEventArgs e)
        {
            DoAssistantsDurationsRadioButtons(AssistantsDurationsType.SymbolsRelative);
        }
        #endregion // assistant's durations radio buttons
        #region bottom buttons
        private void SetPerformanceView(string stopButtonText)
        {
            _isPerforming = true;
            MomentNumberLabel.Text = "";
            MomentPositionLabel.Text = "";
            StopButton.Text = stopButtonText;
            BringToFront();
            Refresh(); // call OnPaint()
            StopButton.Focus();
        }
        /// <summary>
        /// Creates a new set of performance options with a new name.
        /// The new performance options are the same as the current options, but have no recording.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NewMpoxFileButton_Click(object sender, EventArgs e)
        {
            string lastMpoxFile = (string)MpoxFilenamesComboBox.Items[MpoxFilenamesComboBox.Items.Count - 1];
            int newNumber = MpoxFilenameNumber(lastMpoxFile) + 1;
            string newName = _scoreFilenameWithoutExtension + '.' + newNumber.ToString() + M.MoritzPerformanceOptionsExtension;

            MoritzPerformanceOptions newOptions = new MoritzPerformanceOptions(M.Preferences, _po.FilePath, _svgScore);
            newOptions.FilePath = _scoreFolder + "\\" + newName;
            newOptions.Save();
            _po = newOptions;

            MpoxFilenamesComboBox.SuspendLayout();
            MpoxFilenamesComboBox.Items.Add(newName);
            MpoxFilenamesComboBox.SelectedIndex = MpoxFilenamesComboBox.Items.Count - 1;
            MpoxFilenamesComboBox.ResumeLayout();

            SetOptionsView();
        }
        /// <summary>
        /// This button is only available if there is more than one .mpox file.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeleteMpoxFileButton_Click(object sender, EventArgs e)
        {
            string message = "";
            DialogResult result = DialogResult.No;
            string fileName1 = _scoreFilenameWithoutExtension + ".1" + M.MoritzPerformanceOptionsExtension;
            if(_po.FileName == fileName1 && MpoxFilenamesComboBox.Items.Count == 1)
            {
                message = fileName1 + "\nperformance options cannot be deleted.";
                result = DialogResult.No;
                MessageBox.Show(message, "Delete", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                message = "Delete " + _po.FileName + "?";
                result = MessageBox.Show(message, "Delete?", MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);
            }
           
            if(result == DialogResult.Yes)
            {
                string filePath = "";
                File.Delete(_po.FilePath);
                MpoxFilenamesComboBox.SuspendLayout();
                MpoxFilenamesComboBox.Items.Remove(_po.FileName);
                MpoxFilenamesComboBox.SelectedIndex = MpoxFilenamesComboBox.Items.Count - 1;
                if(MpoxFilenamesComboBox.Items.Count > 0)
                {
                    string lastMpoxFile = (string)MpoxFilenamesComboBox.Items[MpoxFilenamesComboBox.Items.Count - 1];
                    filePath = _scoreFolder + "\\" + lastMpoxFile;
                }
                else
                {
                    filePath = _scoreFolder + "\\" + fileName1;
                    MpoxFilenamesComboBox.SuspendLayout();
                    MpoxFilenamesComboBox.Items.Add(fileName1);
                    MpoxFilenamesComboBox.SelectedIndex = 0;
                }
                MpoxFilenamesComboBox.ResumeLayout(); 
                _po = new MoritzPerformanceOptions(M.Preferences, filePath, null);
                SetOptionsView();
            }
        }

        private void PerformLiveButton_Click(object sender, EventArgs e)
        {
            if(ErrorInPerformanceOptions())
                return;

            SetPerformanceView("Stop live performance");

            // in case _po is currently set to assistant only (see PlaySymbolsButton_Click())
            _po = new MoritzPerformanceOptions(M.Preferences, _po.FilePath, _svgScore);

            _assistantPerformerRuntime = 
                new AssistantPerformerRuntime(_midiInputDevice, _midiOutputDevice, _svgScore, _po, false, 
                    ReportPerformersPosition, NotifyCompletion, SaveSequenceAsMidiFile);
            if(_assistantPerformerRuntime != null)
            {
                _assistantPerformerRuntime.Go();
            }
        }
        private void PlaySymbolsButton_Click(object sender, EventArgs e)
        {
            if(ErrorInPerformanceOptions())
                return;

            SetPerformanceView("Stop playing symbols");

            _assistantPerformerRuntime = new AssistantPerformerRuntime(_midiInputDevice, _midiOutputDevice, _svgScore, _po, true,
                ReportPerformersPosition, NotifyCompletion, SaveSequenceAsMidiFile);            
            if(_assistantPerformerRuntime != null)
            {
                _assistantPerformerRuntime.Go();
            }
        }

        private bool ErrorInPerformanceOptions()
        {
            if(_assistantsDurationFactorError || _minimumOrnamentChordMsDurationError || _startMomentError)
            {
                string message = "Cannot play symbols:\n\nError in performance options.";
                MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return true;
            }
            else
                return false;
        }

        private void SetMomentsToAssistant()
        {
            for(int i = 0; i < _po.MoritzPlayers.Count; i++)
            {
                if(_po.MoritzPlayers[i] == MoritzPlayer.LivePerformer)
                    _po.MoritzPlayers[i] = MoritzPlayer.Assistant;
            }
            // MoritzPlayers are now either Silent or Assistant
        }

        private void StopButton_Click(object sender, EventArgs e)
        {
            if(_assistantPerformerRuntime.IsRunning)
                _assistantPerformerRuntime.Stop();
        }

        private void NotifyCompletion()
        {
            _isPerforming = false;
            foreach(Control control in MoritzPlayersPanel.Controls)
            {
                PanelRow panelRow = control as PanelRow;
                if(panelRow != null)
                {
                    panelRow.IsSelected = false;
                }
            }

            if(InvokeRequired)
            {
                MethodInvoker bringToFront = delegate { BringToFront(); };
                MethodInvoker refresh = delegate { Refresh(); };
                MethodInvoker activate = delegate { Activate(); };
                Invoke(bringToFront);
                Invoke(refresh);
                Invoke(activate);
            }
            else
            {
                BringToFront();
                Refresh(); // call OnPaint()
                Focus();
            }
        }

        #endregion // bottom buttons
        #endregion // buttons
        #region private variables and properties
        #region MoritzPlayersPanel
        private PanelRow _selectedRow = null;
        private List<PanelRow> _rows = new List<PanelRow>();
        private int _maxInstrumentWidth = 0;
        #endregion // MoritzPlayersPanel
        private SvgScore _svgScore = null;
        /// <summary>
        /// The folder containing score folders.
        /// </summary>
        private string _algorithmFolder = null;
        /// <summary>
        /// The folder containing the score.
        /// </summary>
        private string _scoreFolder = null;
        /// <summary>
        /// This has no .capx extension and is used to generate new .mpox file names.
        /// </summary>
        private string _scoreFilenameWithoutExtension = null;
        /// <summary>
        /// The current performance options are always saved to PerformanceOptionsPath as soon as possible.
        /// </summary>
        private MoritzPerformanceOptions _po = null;
        /// <summary>
        /// The parent node.
        /// </summary>
        private AssistantPerformerRuntime _assistantPerformerRuntime = null;
        /// <summary>
        /// Used to control this dialog's appearance.
        /// </summary>
        private bool _isPerforming = false;
        /// <summary>
        /// Flags an error in the AssistantsDurationFactorTextBox
        /// </summary>
        private bool _assistantsDurationFactorError = false;
        /// <summary>
        /// Flags an error in the StartMomentTextBox
        /// </summary>
        private bool _startMomentError = false;
        /// <summary>
        /// Flags an error in the MinimumOrnamentChordMsDurationTextBox
        /// </summary>
        private bool _minimumOrnamentChordMsDurationError = false;

        private int _numberOfVoices = 0;
        private IMoritzForm1 _moritzForm1;
        #endregion // private variables and properties
        #region devices
        private MidiInputDevice _midiInputDevice = null;
        private MidiOutputDevice _midiOutputDevice = null;
        #endregion devices


    }
}