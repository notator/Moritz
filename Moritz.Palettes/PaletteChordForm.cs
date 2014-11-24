using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Diagnostics;

using Moritz.Spec;
using Moritz.Globals;
using Moritz.Midi;

using Multimedia.Midi;

namespace Moritz.Palettes
{
    public partial class PaletteChordForm : Form
    {
        public PaletteChordForm(PaletteForm paletteForm, BasicChordControl bcc, int midiChordIndex)
        {
            InitializeComponent();

            Text = paletteForm.Text + ": midi chord " + (midiChordIndex + 1).ToString();

            _paletteForm = paletteForm;
            _bcc = bcc;
            _midiChordIndex = midiChordIndex;

            if(_paletteForm.IsPercussionPalette)
            {
                MidiInstrumentsHelpButton.Text = "Percussion Instr.";
            }
            else
            {
                MidiInstrumentsHelpButton.Text = "MIDI Instruments";
            }

            FindEmptyDefaultControls();

            InitializeTextBoxes(paletteForm, bcc, midiChordIndex);

            ChordDensityTextBox_Leave(ChordDensityTextBox, null);

            InitializeMidiEventButton(midiChordIndex);

            AddAudioSampleButtons(_paletteForm.Domain);

            SetDialogStateIsSaved();
        }

        private void FindEmptyDefaultControls()
        {
            _emptyDefaultLabels = new List<Label>();
            _emptyDefaultTextBoxes = new List<TextBox>();
            _emptyDefaultHelpLabels = new List<Label>();

            if(string.IsNullOrEmpty(_bcc.ChordOffsTextBox.Text))
            {
                _emptyDefaultLabels.Add(this.ChordOffLabel);
                _emptyDefaultTextBoxes.Add(this.ChordOffTextBox);
                _emptyDefaultHelpLabels.Add(this.ChordOffHelpLabel);
            }

            if(string.IsNullOrEmpty(_bcc.InversionIndicesTextBox.Text))
            {
                _emptyDefaultLabels.Add(this.InversionIndexLabel);
                _emptyDefaultTextBoxes.Add(this.InversionIndexTextBox);
                _emptyDefaultHelpLabels.Add(this.InversionIndexHelpLabel);
            }

            if(string.IsNullOrEmpty(_bcc.VerticalVelocityFactorsTextBox.Text))
            {
                _emptyDefaultLabels.Add(this.VerticalVelocityFactorLabel);
                _emptyDefaultTextBoxes.Add(this.VerticalVelocityFactorTextBox);
                _emptyDefaultHelpLabels.Add(this.VerticalVelocityFactorHelpLabel);
            }

            PaletteForm pf = _paletteForm;
            if(string.IsNullOrEmpty(pf.BankIndicesTextBox.Text))
            {
                _emptyDefaultLabels.Add(this.BankIndexLabel);
                _emptyDefaultTextBoxes.Add(this.BankIndexTextBox);
                _emptyDefaultHelpLabels.Add(this.BankIndexHelpLabel);
            }

            if(string.IsNullOrEmpty(pf.RepeatsTextBox.Text))
            {
                _emptyDefaultLabels.Add(this.RepeatsLabel);
                _emptyDefaultTextBoxes.Add(this.RepeatsTextBox);
                _emptyDefaultHelpLabels.Add(this.RepeatsHelpLabel);
            }

            if(string.IsNullOrEmpty(pf.PitchwheelDeviationsTextBox.Text))
            {
                _emptyDefaultLabels.Add(this.PitchwheelDeviationLabel);
                _emptyDefaultTextBoxes.Add(this.PitchwheelDeviationTextBox);
                _emptyDefaultHelpLabels.Add(this.PitchwheelDeviationHelpLabel);
            }

            if(string.IsNullOrEmpty(pf.PitchwheelEnvelopesTextBox.Text))
            {
                _emptyDefaultLabels.Add(this.PitchwheelEnvelopeLabel);
                _emptyDefaultTextBoxes.Add(this.PitchwheelEnvelopeTextBox);
                _emptyDefaultHelpLabels.Add(this.PitchwheelEnvelopeHelpLabel);
            }

            if(string.IsNullOrEmpty(pf.PanEnvelopesTextBox.Text))
            {
                _emptyDefaultLabels.Add(this.PanEnvelopeLabel);
                _emptyDefaultTextBoxes.Add(this.PanEnvelopeTextBox);
                _emptyDefaultHelpLabels.Add(this.PanEnvelopeHelpLabel);
            }

            if(string.IsNullOrEmpty(pf.ModulationWheelEnvelopesTextBox.Text))
            {
                _emptyDefaultLabels.Add(this.ModulationWheelEnvelopeLabel);
                _emptyDefaultTextBoxes.Add(this.ModulationWheelEnvelopeTextBox);
                _emptyDefaultHelpLabels.Add(this.ModulationWheelEnvelopeHelpLabel);
            }

            if(string.IsNullOrEmpty(pf.ExpressionEnvelopesTextBox.Text))
            {
                _emptyDefaultLabels.Add(this.ExpressionEnvelopeLabel);
                _emptyDefaultTextBoxes.Add(this.ExpressionEnvelopeTextBox);
                _emptyDefaultHelpLabels.Add(this.ExpressionEnvelopeHelpLabel);
            }

            if(string.IsNullOrEmpty(pf.MinMsDurationsTextBox.Text))
            {
                _emptyDefaultLabels.Add(this.MinMsDurationLabel);
                _emptyDefaultTextBoxes.Add(this.MinMsDurationTextBox);
                _emptyDefaultHelpLabels.Add(this.MinMsDurationHelpLabel);
            }

        }

        private void InitializeMidiEventButton(int midiChordIndex)
        {
            this.MidiEventButton.Text = (midiChordIndex + 1).ToString();
            this.MidiEventButton.UseVisualStyleBackColor = true;
            this.MidiEventButton.Click += new EventHandler(MidiEventDemoButton_Click);
        }

        private void AddAudioSampleButtons(int domain)
        {
            _audioSampleButtons = new List<Button>();
            int x = 181;
            int y = 582;
            for(int i = 0; i < domain; ++i)
            {
                CreateAudioSampleButton(x, y, i);
                x += 33;
            }
        }

        private void CreateAudioSampleButton(int x, int y, int i)
        {
            Button b = new Button();
            b.Location = new System.Drawing.Point(x, y);
            b.Size = new System.Drawing.Size(27, 24);
            b.Image = _paletteForm.PaletteButtonsControl.AudioSampleButtons[i].Image;
            b.UseVisualStyleBackColor = false;

            b.MouseDown += new MouseEventHandler(AudioSampleButton_MouseDown);

            this.Controls.Add(b);
            _audioSampleButtons.Add(b);
            b.BringToFront();
        }

        private void AudioSampleButton_MouseDown(object sender, MouseEventArgs e)
        {
            Button chordFormButton = sender as Button;
            int index = _audioSampleButtons.IndexOf(chordFormButton);
            Button paletteButtonsControlButton = _paletteForm.PaletteButtonsControl.AudioSampleButtons[index];

            _paletteForm.PaletteButtonsControl.SetLinkedButton(chordFormButton);
            _paletteForm.PaletteButtonsControl.AudioSampleButton_MouseDown(paletteButtonsControlButton, e);
        }

        #region initialization
        /// <summary>
        /// returns a 3 element list containing:
        ///     [0]: the substring of text before the substring at midiChordIndex (can be empty, but not null)
        ///          If not empty, [0] ends with ", ".
        ///     [1]: the trimmed substring of text at midiChordIndex (never empty, contains no commas or spaces)
        ///     [2]: the substring of text after the substring at  midiChordIndex (can be empty, but not null)
        ///          If not empty, [2] begins with ", ".
        /// </summary>
        private List<StringBuilder> GetSubStrings(string text, int midiChordIndex)
        {
            List<StringBuilder> rval = new List<StringBuilder>() { new StringBuilder(), new StringBuilder(), new StringBuilder() };

            string[] substrings = text.Split(new char[] { ',' });
            for(int i = 0; i < substrings.Length; ++i)
            {
                substrings[i] = substrings[i].Trim();
            }

            if(substrings.Length > midiChordIndex)
            {
                for(int i = 0; i < midiChordIndex; ++i)
                {
                    rval[0].Append(substrings[i]);
                    rval[0].Append(", ");
                }
                rval[1].Append(substrings[midiChordIndex]);
                for(int i = midiChordIndex + 1; i < substrings.Length; ++i)
                {
                    rval[2].Append(", ");
                    rval[2].Append(substrings[i]);
                }
            }
            return rval;
        }
        private void InitializeTextBoxes(PaletteForm paletteForm, BasicChordControl bcc, int midiChordIndex)
        {
            durationSBs = GetSubStrings(bcc.DurationsTextBox.Text, midiChordIndex);
            this.DurationTextBox.Text = durationSBs[1].ToString();
            _savedDurationTextBoxText = DurationTextBox.Text;
            _allTextBoxes.Add(DurationTextBox);
            velocitySBs = GetSubStrings(bcc.VelocitiesTextBox.Text, midiChordIndex);
            this.VelocityTextBox.Text = velocitySBs[1].ToString();
            _savedVelocityTextBoxText = VelocityTextBox.Text;
            _allTextBoxes.Add(VelocityTextBox);
            baseMidiPitchSBs = GetSubStrings(bcc.MidiPitchesTextBox.Text, midiChordIndex);
            this.BaseMidiPitchTextBox.Text = baseMidiPitchSBs[1].ToString();
            _savedBaseMidiPitchTextBoxText = BaseMidiPitchTextBox.Text;
            _allTextBoxes.Add(BaseMidiPitchTextBox);
            chordOffSBs = GetSubStrings(bcc.ChordOffsTextBox.Text, midiChordIndex);
            this.ChordOffTextBox.Text = chordOffSBs[1].ToString();
            _savedChordOffTextBoxText = ChordOffTextBox.Text;
            _allTextBoxes.Add(ChordOffTextBox);
            chordDensitySBs = GetSubStrings(bcc.ChordDensitiesTextBox.Text, midiChordIndex);
            this.ChordDensityTextBox.Text = chordDensitySBs[1].ToString();
            _savedChordDensityTextBoxText = ChordDensityTextBox.Text;
            _allTextBoxes.Add(ChordDensityTextBox);

            inversionIndexSBs = GetSubStrings(bcc.InversionIndicesTextBox.Text, midiChordIndex);
            this.InversionIndexTextBox.Text = inversionIndexSBs[1].ToString();
            _savedInversionIndexTextBoxText = InversionIndexTextBox.Text;
            _allTextBoxes.Add(InversionIndexTextBox);
            verticalVelocityFactorSBs = GetSubStrings(bcc.VerticalVelocityFactorsTextBox.Text, midiChordIndex);
            this.VerticalVelocityFactorTextBox.Text = verticalVelocityFactorSBs[1].ToString();
            _savedVerticalVelocityFactorTextBoxText = VerticalVelocityFactorTextBox.Text;
            _allTextBoxes.Add(VerticalVelocityFactorTextBox);

            bankIndexSBs = GetSubStrings(paletteForm.BankIndicesTextBox.Text, midiChordIndex);
            this.BankIndexTextBox.Text = bankIndexSBs[1].ToString();
            _savedBankIndexTextBoxText = BankIndexTextBox.Text;
            _allTextBoxes.Add(BankIndexTextBox);
            patchIndexSBs = GetSubStrings(paletteForm.PatchIndicesTextBox.Text, midiChordIndex);
            this.PatchIndexTextBox.Text = patchIndexSBs[1].ToString();
            _savedPatchIndexTextBoxText = PatchIndexTextBox.Text;
            _allTextBoxes.Add(PatchIndexTextBox);
            repeatsSBs = GetSubStrings(paletteForm.RepeatsTextBox.Text, midiChordIndex);
            this.RepeatsTextBox.Text = repeatsSBs[1].ToString();
            _savedRepeatsTextBoxText = RepeatsTextBox.Text;
            _allTextBoxes.Add(RepeatsTextBox);
            pitchwheelDeviationSBs = GetSubStrings(paletteForm.PitchwheelDeviationsTextBox.Text, midiChordIndex);
            this.PitchwheelDeviationTextBox.Text = pitchwheelDeviationSBs[1].ToString();
            _savedPitchwheelDeviationTextBoxText = PitchwheelDeviationTextBox.Text;
            _allTextBoxes.Add(PitchwheelDeviationTextBox);
            pitchwheelEnvelopeSBs = GetSubStrings(paletteForm.PitchwheelEnvelopesTextBox.Text, midiChordIndex);
            this.PitchwheelEnvelopeTextBox.Text = pitchwheelEnvelopeSBs[1].ToString();
            _savedPitchwheelEnvelopeTextBoxText = PitchwheelEnvelopeTextBox.Text;
            _allTextBoxes.Add(PitchwheelEnvelopeTextBox);
            panEnvelopeSBs = GetSubStrings(paletteForm.PanEnvelopesTextBox.Text, midiChordIndex);
            this.PanEnvelopeTextBox.Text = panEnvelopeSBs[1].ToString();
            _savedPanEnvelopeTextBoxText = PanEnvelopeTextBox.Text;
            _allTextBoxes.Add(PanEnvelopeTextBox);
            modulationWheelEnvelopeSBs = GetSubStrings(paletteForm.ModulationWheelEnvelopesTextBox.Text, midiChordIndex);
            this.ModulationWheelEnvelopeTextBox.Text = modulationWheelEnvelopeSBs[1].ToString();
            _savedModulationWheelEnvelopeTextBoxText = ModulationWheelEnvelopeTextBox.Text;
            _allTextBoxes.Add(ModulationWheelEnvelopeTextBox);
            expressionEnvelopeSBs = GetSubStrings(paletteForm.ExpressionEnvelopesTextBox.Text, midiChordIndex);
            this.ExpressionEnvelopeTextBox.Text = expressionEnvelopeSBs[1].ToString();
            _savedExpressionEnvelopeTextBoxText = ExpressionEnvelopeTextBox.Text;
            _allTextBoxes.Add(ExpressionEnvelopeTextBox);
            ornamentNumberSBs = GetSubStrings(paletteForm.OrnamentNumbersTextBox.Text, midiChordIndex);
            this.OrnamentNumberTextBox.Text = ornamentNumberSBs[1].ToString();
            _savedOrnamentNumberTextBoxText = OrnamentNumberTextBox.Text;
            _allTextBoxes.Add(OrnamentNumberTextBox);
            minMsDurationsSBs = GetSubStrings(paletteForm.MinMsDurationsTextBox.Text, midiChordIndex);
            this.MinMsDurationTextBox.Text = minMsDurationsSBs[1].ToString();
            _savedMinMsDurationTextBoxText = MinMsDurationTextBox.Text;
            _allTextBoxes.Add(MinMsDurationTextBox);
        }

        private void SetEnabledOrnamentControls(int numberOfOrnaments)
        {
            Debug.Assert(numberOfOrnaments > 0);
            if(OrnamentNumberHelpLabel.Enabled)
            {
                OrnamentNumberHelpLabel.Text = "1 integer value in range [ 0.." + numberOfOrnaments.ToString() + " ] (0 means no ornament)";
            }
            if(MinMsDurationHelpLabel.Enabled)
            {
                MinMsDurationHelpLabel.Text = "1 integer value greater than 0";
            }

            ShowOrnamentSettingsButton.Enabled = true;
        }

        private void DisableOrnamentControls()
        {
            OrnamentNumberLabel.Enabled = false;
            OrnamentNumberTextBox.Enabled = false;
            OrnamentNumberHelpLabel.Enabled = false;
            OrnamentNumberHelpLabel.Text = "(edit palette)";
            MinMsDurationLabel.Enabled = false;
            MinMsDurationTextBox.Enabled = false;
            MinMsDurationHelpLabel.Enabled = false;
            MinMsDurationHelpLabel.Text = "(edit palette)";

            ShowOrnamentSettingsButton.Enabled = false;
        }
        #endregion initialization

        #region ReviewableForm

        private void ConfirmButton_Click(object sender, EventArgs e)
        {
            _rff.SetSettingsCanBeSaved(this, M.HasError(_allTextBoxes), ConfirmButton);
            this.SaveAndCloseButton.Enabled = true;
            this.CloseWithoutSavingButton.Enabled = true;
        }

        private void RevertToSavedButton_Click(object sender, EventArgs e)
        {
            Debug.Assert(((ReviewableState)this.Tag) == ReviewableState.needsReview || ((ReviewableState)this.Tag) == ReviewableState.hasChanged);
            DialogResult result =
                MessageBox.Show("Are you sure you want to revert this dialog to the saved version?", "Revert?",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);

            if(result == System.Windows.Forms.DialogResult.Yes)
            {
                DurationTextBox.Text = _savedDurationTextBoxText;
                VelocityTextBox.Text = _savedVelocityTextBoxText;
                BaseMidiPitchTextBox.Text = _savedBaseMidiPitchTextBoxText;
                ChordOffTextBox.Text = _savedChordOffTextBoxText;
                ChordDensityTextBox.Text =_savedChordDensityTextBoxText;
                InversionIndexTextBox.Text =_savedInversionIndexTextBoxText;
                VerticalVelocityFactorTextBox.Text =_savedVerticalVelocityFactorTextBoxText;
                BankIndexTextBox.Text =_savedBankIndexTextBoxText;
                PatchIndexTextBox.Text =_savedPatchIndexTextBoxText;
                RepeatsTextBox.Text =_savedRepeatsTextBoxText;
                PitchwheelDeviationTextBox.Text =_savedPitchwheelDeviationTextBoxText;
                PitchwheelEnvelopeTextBox.Text =_savedPitchwheelEnvelopeTextBoxText;
                PanEnvelopeTextBox.Text =_savedPanEnvelopeTextBoxText;
                ModulationWheelEnvelopeTextBox.Text =_savedModulationWheelEnvelopeTextBoxText;
                ExpressionEnvelopeTextBox.Text =_savedExpressionEnvelopeTextBoxText;
                OrnamentNumberTextBox.Text =_savedOrnamentNumberTextBoxText;
                MinMsDurationTextBox.Text =_savedMinMsDurationTextBoxText;

                TouchAllTextBoxes();

                SetDialogStateIsSaved();
            }
        }

        private void SetDialogStateIsSaved()
        {
            _rff.SetSettingsAreSaved(this, M.HasError(_allTextBoxes), ConfirmButton, RevertToSavedButton);
            this.SaveAndCloseButton.Enabled = false;
            this.CloseWithoutSavingButton.Enabled = true;
        }

        #region reverting strings
        private string _savedDurationTextBoxText;
        private string _savedVelocityTextBoxText;
        private string _savedBaseMidiPitchTextBoxText;
        private string _savedChordOffTextBoxText;
        private string _savedChordDensityTextBoxText;
        private string _savedInversionIndexTextBoxText;
        private string _savedVerticalVelocityFactorTextBoxText;
        private string _savedBankIndexTextBoxText;
        private string _savedPatchIndexTextBoxText;
        private string _savedRepeatsTextBoxText;
        private string _savedPitchwheelDeviationTextBoxText;
        private string _savedPitchwheelEnvelopeTextBoxText;
        private string _savedPanEnvelopeTextBoxText;
        private string _savedModulationWheelEnvelopeTextBoxText;
        private string _savedExpressionEnvelopeTextBoxText;
        private string _savedOrnamentNumberTextBoxText;
        private string _savedMinMsDurationTextBoxText;
        #endregion reverting strings
        #endregion ReviewableForm

        #region buttons
        private void MidiInstrumentsHelpButton_Click(object sender, EventArgs e)
        {
            if(_paletteForm.IsPercussionPalette)
            {
                _paletteForm.PercussionInstrHelpButton_Click(sender, e);
            }
            else
            {
                _paletteForm.MidiInstrumentsHelpButton_Click(sender, e);
            }
        }
        private void MidiPitchesHelpButton_Click(object sender, EventArgs e)
        {
            _paletteForm.MidiPitchesHelpButton_Click(sender, e);
        }
        private void ShowOrnamentSettingsButton_Click(object sender, EventArgs e)
        {
            _paletteForm.ShowOrnamentSettingsButton_Click(sender, e);
        }
        private List<TextBox> AllTextBoxes()
        {
            List<TextBox> allTextBoxes = new List<TextBox>();

            allTextBoxes.Add(this.DurationTextBox);
            allTextBoxes.Add(this.VelocityTextBox);
            allTextBoxes.Add(this.BaseMidiPitchTextBox);
            allTextBoxes.Add(this.ChordOffTextBox);
            allTextBoxes.Add(this.ChordDensityTextBox);
            allTextBoxes.Add(this.InversionIndexTextBox);
            allTextBoxes.Add(this.VerticalVelocityFactorTextBox);
            allTextBoxes.Add(this.BankIndexTextBox);
            allTextBoxes.Add(this.PatchIndexTextBox);
            allTextBoxes.Add(this.RepeatsTextBox);
            allTextBoxes.Add(this.PitchwheelDeviationTextBox);
            allTextBoxes.Add(this.PitchwheelEnvelopeTextBox);
            allTextBoxes.Add(this.PanEnvelopeTextBox);
            allTextBoxes.Add(this.ModulationWheelEnvelopeTextBox);
            allTextBoxes.Add(this.ExpressionEnvelopeTextBox);
            allTextBoxes.Add(this.OrnamentNumberTextBox);
            allTextBoxes.Add(this.MinMsDurationTextBox);

            return allTextBoxes;
        }

        private void SaveAndCloseButton_Click(object sender, EventArgs e)
        {
            if(M.HasError(_allTextBoxes))
            {
                DoErrorMessage("Can't save because there is an error in one or more of the fields.");
            }
            else if(this.ChordDensityTextBox.Text == "0") // a rest
            {
                _bcc.DurationsTextBox.Text = durationSBs[0].ToString() + this.DurationTextBox.Text + durationSBs[2].ToString();
                _bcc.ChordDensitiesTextBox.Text = chordDensitySBs[0].ToString() + this.ChordDensityTextBox.Text + chordDensitySBs[2].ToString();
                _paletteForm.ClosePaletteChordForm(_midiChordIndex);
            }
            else
            {
                _bcc.DurationsTextBox.Text = durationSBs[0].ToString() + this.DurationTextBox.Text + durationSBs[2].ToString();
                _bcc.VelocitiesTextBox.Text = velocitySBs[0].ToString() + this.VelocityTextBox.Text + velocitySBs[2].ToString();
                _bcc.MidiPitchesTextBox.Text = baseMidiPitchSBs[0].ToString() + BaseMidiPitchTextBox.Text + baseMidiPitchSBs[2].ToString();
                _bcc.ChordOffsTextBox.Text = chordOffSBs[0].ToString() + this.ChordOffTextBox.Text + chordOffSBs[2].ToString();
                _bcc.ChordDensitiesTextBox.Text = chordDensitySBs[0].ToString() + this.ChordDensityTextBox.Text + chordDensitySBs[2].ToString();

                if(this.ChordDensityTextBox.Text != "1") // a chord with more than one notehead
                {
                    _bcc.InversionIndicesTextBox.Text = inversionIndexSBs[0].ToString() + this.InversionIndexTextBox.Text + inversionIndexSBs[2].ToString();
                    _bcc.VerticalVelocityFactorsTextBox.Text = verticalVelocityFactorSBs[0].ToString() + this.VerticalVelocityFactorTextBox.Text + verticalVelocityFactorSBs[2].ToString();
                }

                _paletteForm.BankIndicesTextBox.Text = bankIndexSBs[0].ToString() + this.BankIndexTextBox.Text + bankIndexSBs[2].ToString();
                _paletteForm.PatchIndicesTextBox.Text = patchIndexSBs[0].ToString() + this.PatchIndexTextBox.Text + patchIndexSBs[2].ToString();
                _paletteForm.RepeatsTextBox.Text = repeatsSBs[0].ToString() + this.RepeatsTextBox.Text + repeatsSBs[2].ToString();
                _paletteForm.PitchwheelDeviationsTextBox.Text = pitchwheelDeviationSBs[0].ToString() + this.PitchwheelDeviationTextBox.Text + pitchwheelDeviationSBs[2].ToString();
                _paletteForm.PitchwheelEnvelopesTextBox.Text = pitchwheelEnvelopeSBs[0].ToString() + this.PitchwheelEnvelopeTextBox.Text + pitchwheelEnvelopeSBs[2].ToString();
                _paletteForm.PanEnvelopesTextBox.Text = panEnvelopeSBs[0].ToString() + this.PanEnvelopeTextBox.Text + panEnvelopeSBs[2].ToString();
                _paletteForm.ModulationWheelEnvelopesTextBox.Text = modulationWheelEnvelopeSBs[0].ToString() + this.ModulationWheelEnvelopeTextBox.Text + modulationWheelEnvelopeSBs[2].ToString();
                _paletteForm.ExpressionEnvelopesTextBox.Text = expressionEnvelopeSBs[0].ToString() + this.ExpressionEnvelopeTextBox.Text + expressionEnvelopeSBs[2].ToString();

                if(_paletteForm.OrnamentNumbersTextBox.Enabled)
                {
                    _paletteForm.OrnamentNumbersTextBox.Text = ornamentNumberSBs[0].ToString() + this.OrnamentNumberTextBox.Text + ornamentNumberSBs[2].ToString();
                }
                
                _paletteForm.MinMsDurationsTextBox.Text = minMsDurationsSBs[0].ToString() + this.MinMsDurationTextBox.Text + minMsDurationsSBs[2].ToString();

                _paletteForm.SetSettingsHaveChanged();
                _paletteForm.ClosePaletteChordForm(_midiChordIndex);
            }

        }

        private void CloseWithoutSavingButton_Click(object sender, EventArgs e)
        {
            _paletteForm.ClosePaletteChordForm(_midiChordIndex);
        }
        #endregion buttons

        #region TextBox_Leave handlers
        private void SetDialogState(TextBox textBox, bool okay)
        {
            M.SetTextBoxErrorColorIfNotOkay(textBox, okay);
            _rff.SetSettingsNeedReview(this, M.HasError(_allTextBoxes), ConfirmButton, RevertToSavedButton);
            this.CloseWithoutSavingButton.Enabled = true;
            this.SaveAndCloseButton.Enabled = false;
        }

        private void DurationTextBox_Leave(object sender, EventArgs e)
        {
            M.LeaveIntRangeTextBox(sender as TextBox, false, 1, 1, int.MaxValue, SetDialogState);
        }
        private void VelocityTextBox_Leave(object sender, EventArgs e)
        {
            M.LeaveIntRangeTextBox(sender as TextBox, false, 1, 0, 127, SetDialogState);
        }
        private void BaseMidiPitchTextBox_Leave(object sender, EventArgs e)
        {
            M.LeaveIntRangeTextBox(sender as TextBox, false, 1, 0, 127, SetDialogState);
        }
        private void ChordOffTextBox_Leave(object sender, EventArgs e)
        {
            M.LeaveIntRangeTextBox(sender as TextBox, false, 1, 0, 1, SetDialogState);
        }

        private void EnableAllTextBoxesAndMidiButton()
        {
            List<TextBox> allTextBoxes = AllTextBoxes();
            foreach(TextBox textBox in allTextBoxes)
            {
                textBox.Enabled = true;
            }
            this.MidiEventButton.Enabled = true;
        }
        private void DisableAllTextBoxesAndMidiButton()
        {
            List<TextBox> allTextBoxes = AllTextBoxes();
            foreach(TextBox textBox in allTextBoxes)
            {
                textBox.Enabled = false;
            }
            this.MidiEventButton.Enabled = false;
        }
        private void DisableEmptyDefaultControls()
        {
            foreach(Label label in _emptyDefaultLabels)
            {
                label.Enabled = false;
            }
            foreach(TextBox textBox in _emptyDefaultTextBoxes)
            {
                textBox.Enabled = false;
            }
            foreach(Label label in _emptyDefaultHelpLabels)
            {
                label.Enabled = false;
            }
        }
        private void SetHelpTexts()
        {
            ChordDensityHelpLabel.Text = "1 integer value in range [ 0.." + _bcc.MaximumChordDensity.ToString() + " ] ( 0 creates a rest. )";
            RootInversionHelpLabel.Text = "root inversion: " + _bcc.RootInversionTextBox.Text;

            foreach(Label label in _emptyDefaultHelpLabels)
            {
                label.Text = "(edit palette)";
            }
        }
        private void ChordDensityTextBox_Leave(object sender, EventArgs e)
        {
            TextBox chordDensityTextBox = sender as TextBox;

            M.LeaveIntRangeTextBox(chordDensityTextBox, false, 1, 0, _bcc.MaximumChordDensity, SetDialogState);

            if(chordDensityTextBox.BackColor == M.TextBoxErrorColor)
            {
                DisableAllTextBoxesAndMidiButton();
                this.ChordDensityTextBox.Enabled = true;
            }
            else
            {
                EnableAllTextBoxesAndMidiButton();

                DisableEmptyDefaultControls();
                SetHelpTexts();

                int thisChordDensity = int.Parse(chordDensityTextBox.Text);
                if(thisChordDensity == 0) // a rest
                {
                    DisableAllTextBoxesAndMidiButton();
                    this.DurationTextBox.Enabled = true;
                    this.ChordDensityTextBox.Enabled = true;
                }
                else if(thisChordDensity == 1)
                {
                    InversionIndexTextBox.Enabled = false;
                    VerticalVelocityFactorTextBox.Enabled = false;
                }

                if(InversionIndexTextBox.Enabled)
                {
                    int inversionsMaxIndex = ((2 * (_bcc.MaximumChordDensity - 2)) - 1);
                    InversionIndexHelpLabel.Text = "1 integer value in range [ 0.." + inversionsMaxIndex.ToString() + " ]";
                }

                if(_paletteForm.OrnamentSettingsForm != null && _paletteForm.OrnamentSettingsForm.Ornaments != null)
                    SetEnabledOrnamentControls(_paletteForm.OrnamentSettingsForm.Ornaments.Count);
                else
                    DisableOrnamentControls();
            }
        }

        private void InversionIndexTextBox_Leave(object sender, EventArgs e)
        {
            int inversionsMaxIndex = ((2 * (_bcc.MaximumChordDensity - 2)) - 1);
            M.LeaveIntRangeTextBox(sender as TextBox, false, 1, 0, inversionsMaxIndex, SetDialogState);
        }
        private void VerticalVelocityFactorTextBox_Leave(object sender, EventArgs e)
        {
            M.LeaveFloatRangeTextBox(sender as TextBox, false, 1, 0F, float.MaxValue, SetDialogState);
        }
        private void BankIndexTextBox_Leave(object sender, EventArgs e)
        {
            M.LeaveIntRangeTextBox(sender as TextBox, false, 1, 0, 127, SetDialogState);
        }
        private void PatchIndexTextBox_Leave(object sender, EventArgs e)
        {
            M.LeaveIntRangeTextBox(sender as TextBox, false, 1, 0, 127, SetDialogState);
        }
        private void RepeatsTextBox_Leave(object sender, EventArgs e)
        {
            M.LeaveIntRangeTextBox(sender as TextBox, false, 1, 0, 1, SetDialogState);
        }
        private void PitchwheelDeviationTextBox_Leave(object sender, EventArgs e)
        {
            M.LeaveIntRangeTextBox(sender as TextBox, false, 1, 0, 127, SetDialogState);
        }
        /// <summary>
        /// If the textBox is disabled, this function does nothing
        /// </summary>
        /// <param name="textBox"></param>
        private void CheckEnvelope(TextBox textBox)
        {
            if(textBox.Enabled)
            {
                StringBuilder envelopeSB = new StringBuilder();
                List<string> numbers = _paletteForm.GetCheckedIntStrings(textBox.Text, -1, 0, 127, ':');
                if(numbers != null)
                {
                    foreach(string numberStr in numbers)
                    {
                        envelopeSB.Append(":");
                        envelopeSB.Append(numberStr);
                    }
                    envelopeSB.Remove(0, 1);
                    textBox.Text = envelopeSB.ToString();
                    textBox.BackColor = Color.White;
                }
                else if(!string.IsNullOrEmpty(textBox.Text)) // it is not an error if the textBox is empty
                {
                    textBox.BackColor = M.TextBoxErrorColor;
                }
                SetDialogState(textBox, textBox.BackColor == Color.White);
            }
        }
        private void PitchwheelEnvelopeTextBox_Leave(object sender, EventArgs e)
        {
            CheckEnvelope(sender as TextBox);
        }
        private void PanEnvelopeTextBox_Leave(object sender, EventArgs e)
        {
            CheckEnvelope(sender as TextBox);
        }
        private void ModulationWheelEnvelopeTextBox_Leave(object sender, EventArgs e)
        {
            CheckEnvelope(sender as TextBox);
        }
        private void ExpressionEnvelopeTextBox_Leave(object sender, EventArgs e)
        {
            CheckEnvelope(sender as TextBox);
        }
        private void OrnamentNumberTextBox_Leave(object sender, EventArgs e)
        {
            M.LeaveIntRangeTextBox(sender as TextBox, false, 1, 0, _paletteForm.OrnamentSettingsForm.Ornaments.Count, SetDialogState);
        }
        private void MinMsDurationTextBox_Leave(object sender, EventArgs e)
        {
            M.LeaveIntRangeTextBox(sender as TextBox, false, 1, 0, int.MaxValue, SetDialogState);
        }
        #endregion TextBox_Leave handlers

        private void TouchAllTextBoxes()
        {
            ChordDensityTextBox_Leave(ChordDensityTextBox, null);
            InversionIndexTextBox_Leave(InversionIndexTextBox, null);
            VerticalVelocityFactorTextBox_Leave(VerticalVelocityFactorTextBox, null);
            BankIndexTextBox_Leave(BankIndexTextBox, null);
            PatchIndexTextBox_Leave(PatchIndexTextBox, null);
            RepeatsTextBox_Leave(RepeatsTextBox, null);
            PitchwheelDeviationTextBox_Leave(PitchwheelDeviationTextBox, null);
            PitchwheelEnvelopeTextBox_Leave(PitchwheelEnvelopeTextBox, null);
            ModulationWheelEnvelopeTextBox_Leave(ModulationWheelEnvelopeTextBox, null);
            ExpressionEnvelopeTextBox_Leave(ExpressionEnvelopeTextBox, null);
            OrnamentNumberTextBox_Leave(OrnamentNumberTextBox, null);
            MinMsDurationTextBox_Leave(this.MinMsDurationTextBox, null);
        }

        private static void DoErrorMessage(string message)
        {
            MessageBox.Show(message, "Error");
        }

        private void PaletteChordForm_MouseClick(object sender, MouseEventArgs e)
        {
            _paletteForm.PaletteButtonsControl.AudioSampleButton_MouseDown(this, e); // stops a playing audio sample
        }

        #region play midi event
        private void MidiEventDemoButton_Click(object sender, EventArgs e)
        {
            if(M.HasError(_allTextBoxes))
            {
                DoErrorMessage("Can't play because there is an error in one or more of the fields.");
            }
            else
            {
                Button midiEventDemoButton = sender as Button;
                DurationDef durationDef = GetDurationDef();
                MidiChordDef midiChordDef = durationDef as MidiChordDef;
                RestDef restDef = durationDef as RestDef;

                if(midiChordDef != null)
                {
                    int midiChannel = 0;
                    OutputDevice outputDevice = M.Preferences.CurrentMultimediaMidiOutputDevice;
                    if(_paletteForm.IsPercussionPalette)
                    {
                        midiChannel = 9;
                        outputDevice = M.Preferences.GetMidiOutputDevice("Microsoft GS Wavetable Synth");
                    }
                    MidiChord midiChord = new MidiChord(midiChannel, midiChordDef, outputDevice);
                    midiChord.Send(); //sends in this thread (blocks the current thread -- keeping the button selected)
                }
                else
                {
                    midiEventDemoButton.Hide();
                    Refresh(); // shows "rest" behind button
                    Debug.Assert(restDef != null);
                    Thread.Sleep(restDef.MsDuration);
                    midiEventDemoButton.Show();
                    Refresh();
                }
            }
        }

        /// <summary>
        /// Returns either a new RestDef or a new MidiChordDef
        /// In both cases, MsPosition is set to zero, Lyric is set to null.
        /// </summary>
        private DurationDef GetDurationDef()
        {
            DurationDef rval = null;

            if(this.ChordDensityTextBox.Text == "0")
            {
                int msDuration = 0;
                try
                {
                    msDuration = int.Parse(this.DurationTextBox.Text);
                }
                catch
                {
                    Debug.Assert(false);
                }
                Debug.Assert(msDuration > 0);
                rval = new RestDef(0, msDuration);
            }
            else
            {
                Palette palette = new Palette(this);
                rval = palette.MidiChordDef(0);
            }

            return rval;
        }

        #endregion

        #region private variables
        List<StringBuilder> durationSBs;
        List<StringBuilder> velocitySBs;
        List<StringBuilder> baseMidiPitchSBs;
        List<StringBuilder> chordOffSBs;
        List<StringBuilder> chordDensitySBs;
        List<StringBuilder> inversionIndexSBs;
        List<StringBuilder> verticalVelocityFactorSBs;
        List<StringBuilder> bankIndexSBs;
        List<StringBuilder> patchIndexSBs;
        List<StringBuilder> repeatsSBs;
        List<StringBuilder> pitchwheelDeviationSBs;
        List<StringBuilder> pitchwheelEnvelopeSBs;
        List<StringBuilder> panEnvelopeSBs;
        List<StringBuilder> modulationWheelEnvelopeSBs;
        List<StringBuilder> expressionEnvelopeSBs;
        List<StringBuilder> ornamentNumberSBs;
        List<StringBuilder> minMsDurationsSBs;
        
        PaletteForm _paletteForm;
        BasicChordControl _bcc;
        int _midiChordIndex;
        List<Label> _emptyDefaultLabels;
        List<TextBox> _emptyDefaultTextBoxes;
        List<Label> _emptyDefaultHelpLabels;
        List<Button> _audioSampleButtons;
        ReviewableFormFunctions _rff = new ReviewableFormFunctions();
        List<TextBox> _allTextBoxes = new List<TextBox>();
        #endregion  private variables

        public PaletteForm PaletteForm { get { return _paletteForm; } }
    }
}

