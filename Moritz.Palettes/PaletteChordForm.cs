using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Diagnostics;

using Moritz.Spec;
using Moritz.Globals;
using Moritz.Midi;
using Moritz.Spec;
using Moritz.Symbols;

namespace Moritz.Palettes
{
    public partial class PaletteChordForm : Form
    {
        public PaletteChordForm(PaletteForm paletteForm, BasicChordControl bcc, int midiChordIndex, SetDialogStateDelegate SetDialogState)
        {
            InitializeComponent();

            _paletteForm = paletteForm;
            _bcc = bcc;
            _setDialogState = SetDialogState; // delegate

            _paletteForm.Enabled = false;

            Text = "midi chord " + (midiChordIndex + 1).ToString();

            InitializeTextBoxes(paletteForm, bcc, midiChordIndex);

            if(_bcc.MaximumChordDensity > 1)
                EnableChordParameters(bcc.MaximumChordDensity);
            else // _bcc.MaximumChordDensity = 0 or 1
                DisableChordParameters();

            if(paletteForm.OrnamentSettingsForm != null)
                EnableOrnamentParameters(paletteForm.OrnamentSettingsForm.NumberOfOrnaments);
            else
                DisableOrnamentParameters();

            InitializeMidiEventButton(midiChordIndex);

            ChordDensityTextBox_Leave(ChordDensityTextBox, null);
        }

        private void InitializeMidiEventButton(int midiChordIndex)
        {
            this.MidiEventButton.Text = (midiChordIndex + 1).ToString();
            this.MidiEventButton.UseVisualStyleBackColor = true;
            this.MidiEventButton.Click += new EventHandler(MidiEventDemoButton_Click);
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
            velocitySBs = GetSubStrings(bcc.VelocitiesTextBox.Text, midiChordIndex);
            this.VelocityTextBox.Text = velocitySBs[1].ToString();
            baseMidiPitchSBs = GetSubStrings(bcc.MidiPitchesTextBox.Text, midiChordIndex);
            this.BaseMidiPitchTextBox.Text = baseMidiPitchSBs[1].ToString();
            chordOffSBs = GetSubStrings(bcc.ChordOffsTextBox.Text, midiChordIndex);
            this.ChordOffTextBox.Text = chordOffSBs[1].ToString();
            chordDensitySBs = GetSubStrings(bcc.ChordDensitiesTextBox.Text, midiChordIndex);
            this.ChordDensityTextBox.Text = chordDensitySBs[1].ToString();
 
            inversionIndexSBs = GetSubStrings(bcc.InversionIndicesTextBox.Text, midiChordIndex);
            this.InversionIndexTextBox.Text = inversionIndexSBs[1].ToString();
            verticalVelocityFactorSBs = GetSubStrings(bcc.VerticalVelocityFactorsTextBox.Text, midiChordIndex);
            this.VerticalVelocityFactorTextBox.Text = verticalVelocityFactorSBs[1].ToString();
            bankIndexSBs = GetSubStrings(paletteForm.BankIndicesTextBox.Text, midiChordIndex);
            this.BankIndexTextBox.Text = bankIndexSBs[1].ToString();
            patchIndexSBs = GetSubStrings(paletteForm.PatchIndicesTextBox.Text, midiChordIndex);
            this.PatchIndexTextBox.Text = patchIndexSBs[1].ToString();
            repeatsSBs = GetSubStrings(paletteForm.RepeatsTextBox.Text, midiChordIndex);
            this.RepeatsTextBox.Text = repeatsSBs[1].ToString();
            pitchwheelDeviationSBs = GetSubStrings(paletteForm.PitchwheelDeviationsTextBox.Text, midiChordIndex);
            this.PitchwheelDeviationTextBox.Text = pitchwheelDeviationSBs[1].ToString();
            pitchwheelEnvelopeSBs = GetSubStrings(paletteForm.PitchwheelEnvelopesTextBox.Text, midiChordIndex);
            this.PitchwheelEnvelopeTextBox.Text = pitchwheelEnvelopeSBs[1].ToString();
            panEnvelopeSBs = GetSubStrings(paletteForm.PanEnvelopesTextBox.Text, midiChordIndex);
            this.PanEnvelopeTextBox.Text = panEnvelopeSBs[1].ToString();
            modulationWheelEnvelopeSBs = GetSubStrings(paletteForm.ModulationWheelEnvelopesTextBox.Text, midiChordIndex);
            this.ModulationWheelEnvelopeTextBox.Text = modulationWheelEnvelopeSBs[1].ToString();
            expressionEnvelopeSBs = GetSubStrings(paletteForm.ExpressionEnvelopesTextBox.Text, midiChordIndex);
            this.ExpressionEnvelopeTextBox.Text = expressionEnvelopeSBs[1].ToString();
            ornamentNumberSBs = GetSubStrings(paletteForm.OrnamentNumbersTextBox.Text, midiChordIndex);
            this.OrnamentNumberTextBox.Text = ornamentNumberSBs[1].ToString();
            minMsDurationsSBs = GetSubStrings(paletteForm.MinMsDurationsTextBox.Text, midiChordIndex);
            this.MinMsDurationsTextBox.Text = minMsDurationsSBs[1].ToString();
        }
        private void EnableChordParameters(int maximumChordDensity)
        {
            Debug.Assert(maximumChordDensity > 1);
            ChordDensityTextBox.Enabled = true;
            ChordDensityHelpLabel.Text = "1 integer value in range [ 0.." + maximumChordDensity.ToString() + " ] ( 0 creates a rest. )";

            RootInversionHelpLabel.Text = "root inversion: " + _bcc.RootInversionTextBox.Text;

            InversionIndexTextBox.Enabled = true; 
            int inversionsMaxIndex = ((2 * (maximumChordDensity - 2)) - 1);
            InversionIndexHelpLabel.Text = "1 integer value in range [ 0.." + inversionsMaxIndex.ToString() + " ]";

            VerticalVelocityFactorTextBox.Enabled = true;
        }
        private void DisableChordParameters()
        {
            ChordDensityTextBox.Enabled = false;
            ChordDensityHelpLabel.Text = "";

            RootInversionHelpLabel.Text = "root inversion: " + _bcc.RootInversionTextBox.Text;

            InversionIndexTextBox.Enabled = false;
            InversionIndexHelpLabel.Text = "";

            VerticalVelocityFactorTextBox.Enabled = false;
            VerticalVelocityFactorHelpLabel.Text = "";
        }
        private void EnableOrnamentParameters(int numberOfOrnaments)
        {
            Debug.Assert(numberOfOrnaments > 0);
            OrnamentNumberTextBox.Enabled = true;
            OrnamentNumberHelpLabel.Text = "1 integer value in range [ 0.." + numberOfOrnaments.ToString() + " ] (0 means no ornament)";

            ShowOrnamentSettingsButton.Enabled = true;
        }
        private void DisableOrnamentParameters()
        {
            OrnamentNumberTextBox.Enabled = false;
            OrnamentNumberHelpLabel.Text = "";

            ShowOrnamentSettingsButton.Enabled = false;
        }
        #endregion initialization

        #region buttons
        private void MidiInstrumentsHelpButton_Click(object sender, EventArgs e)
        {
            _paletteForm.MidiInstrumentsHelpButton_Click(sender, e);
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
            allTextBoxes.Add(this.MinMsDurationsTextBox);

            return allTextBoxes;
        }
        private bool HasError()
        {
            bool hasError = false;
            List<TextBox> allTextBoxes = AllTextBoxes();
            foreach(TextBox textBox in allTextBoxes)
            {
                if(textBox.Enabled && textBox.BackColor == M.TextBoxErrorColor)
                {
                    hasError = true;
                    break;
                }
            }

            return hasError;
        }
        private void SaveAndCloseButton_Click(object sender, EventArgs e)
        {
            if(HasError())
            {
                DoErrorMessage("Can't save because there is an error in one or more of the fields.");
            }
            else if(this.ChordDensityTextBox.Text == "0") // a rest
            {
                _bcc.DurationsTextBox.Text = durationSBs[0].ToString() + this.DurationTextBox.Text + durationSBs[2].ToString();
                _bcc.ChordDensitiesTextBox.Text = chordDensitySBs[0].ToString() + this.ChordDensityTextBox.Text + chordDensitySBs[2].ToString();
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
                
                _paletteForm.MinMsDurationsTextBox.Text = minMsDurationsSBs[0].ToString() + this.MinMsDurationsTextBox.Text + minMsDurationsSBs[2].ToString();
            }

            _bcc.SetChordControls();
            _paletteForm.Enabled = true;
            _paletteForm.BringToFront();

            this.Close();
        }

        private void CloseWithoutSavingButton_Click(object sender, EventArgs e)
        {
            _paletteForm.Enabled = true;
            _paletteForm.BringToFront();
            this.Close();
        }
        #endregion buttons

        #region TextBox_Leave handlers
        private void DurationTextBox_Leave(object sender, EventArgs e)
        {
            M.LeaveIntRangeTextBox(sender as TextBox, false, 1, 1, int.MaxValue, _setDialogState);
        }
        private void VelocityTextBox_Leave(object sender, EventArgs e)
        {
            M.LeaveIntRangeTextBox(sender as TextBox, false, 1, 0, 127, _setDialogState);  
        }
        private void BaseMidiPitchTextBox_Leave(object sender, EventArgs e)
        {
            M.LeaveIntRangeTextBox(sender as TextBox, false, 1, 0, 127, _setDialogState);
        }
        private void ChordOffTextBox_Leave(object sender, EventArgs e)
        {
            M.LeaveIntRangeTextBox(sender as TextBox, true, 1, 0, 1, _setDialogState);
        }
        private void ChordDensityTextBox_Leave(object sender, EventArgs e)
        {
            TextBox chordDensityTextBox = sender as TextBox;
            List<TextBox> allTextBoxes = AllTextBoxes();

            M.LeaveIntRangeTextBox(chordDensityTextBox, false, 1, 0, _bcc.MaximumChordDensity, _setDialogState);

            foreach(TextBox textBox in allTextBoxes)
            {
                textBox.Enabled = true;
            }

            if(chordDensityTextBox.BackColor == M.TextBoxErrorColor)
            {
                foreach(TextBox textBox in allTextBoxes)
                {
                    textBox.Enabled = false;
                }
                this.ChordDensityTextBox.Enabled = true;
            }
            else if(chordDensityTextBox.Text == "1")
            {
                InversionIndexTextBox.Enabled = false;
                VerticalVelocityFactorTextBox.Enabled = false;
            }
            else if(chordDensityTextBox.Text == "0") // a rest
            {
                foreach(TextBox textBox in allTextBoxes)
                {
                    textBox.Enabled = false;
                }
                this.DurationTextBox.Enabled = true;
                this.ChordDensityTextBox.Enabled = true;
            }
        }
        private void InversionIndexTextBox_Leave(object sender, EventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if(textBox != null)
            {
                int inversionsMaxIndex = ((2 * (_bcc.MaximumChordDensity - 2)) - 1);
                M.LeaveIntRangeTextBox(textBox, false, 1, 0, inversionsMaxIndex, _setDialogState);
            }
        }
        private void VerticalVelocityFactorTextBox_Leave(object sender, EventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if(textBox != null)
            {
                M.LeaveFloatRangeTextBox(sender as TextBox, false, 1, 0F, float.MaxValue, _setDialogState);
            }
        }
        private void BankIndexTextBox_Leave(object sender, EventArgs e)
        {
            M.LeaveIntRangeTextBox(sender as TextBox, true, 1, 0, 127, _setDialogState);
        }
        private void PatchIndexTextBox_Leave(object sender, EventArgs e)
        {
            M.LeaveIntRangeTextBox(sender as TextBox, false, 1, 0, 127, _setDialogState);
        }
        private void RepeatsTextBox_Leave(object sender, EventArgs e)
        {
            M.LeaveIntRangeTextBox(sender as TextBox, true, 1, 0, 1, _setDialogState);
        }
        private void PitchwheelDeviationTextBox_Leave(object sender, EventArgs e)
        {
            M.LeaveIntRangeTextBox(sender as TextBox, true, 1, 0, 127, _setDialogState);
        }
        private void CheckEnvelope(TextBox textBox)
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
            else
            {
                textBox.BackColor = M.TextBoxErrorColor;
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
            M.LeaveIntRangeTextBox(sender as TextBox, true, 1, 0, _paletteForm.OrnamentSettingsForm.NumberOfOrnaments, _setDialogState);
        }
        private void MinMsDurationsTextBox_Leave(object sender, EventArgs e)
        {
            M.LeaveIntRangeTextBox(sender as TextBox, true, 1, 0, int.MaxValue, _setDialogState);
        }
        #endregion TextBox_Leave handlers

        private static void DoErrorMessage(string message)
        {
            MessageBox.Show(message, "Error");
        }

        #region play midi event
        private void MidiEventDemoButton_Click(object sender, EventArgs e)
        {
            if(HasError())
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
                    MidiChord midiChord = new MidiChord(0, midiChordDef);
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

        private List<byte> GetMidiPitches(byte rootPitch, byte density, List<byte> primeIntervals)
        {
            int nUpperPitches = density - 1;
            List<byte> midiPitches = new List<byte>();
            midiPitches.Add(rootPitch);
            for(int p = 0; p < nUpperPitches; p++)
            {
                byte newpitch = M.MidiValue(midiPitches[p] + primeIntervals[p]);
                midiPitches.Add(newpitch);
            }
            return midiPitches;
        }

        private List<byte> GetVerticalVelocities(byte rootVelocity, int vDensity, float vvFactor)
        {
            List<byte> midiVelocities = new List<byte>();

            if(vvFactor == 1F || vDensity == 1)
            {
                for(int i = 0; i < vDensity; ++i)
                {
                    midiVelocities.Add((byte)rootVelocity);
                }
            }
            else
            {
                float bottomVelocity = rootVelocity;
                if(vvFactor > 1.0F)
                    bottomVelocity = bottomVelocity / vvFactor;
                float topVelocity = bottomVelocity * vvFactor;
                float velocityDifference = (topVelocity - bottomVelocity) / ((float)(vDensity - 1));
                float newVelocity = bottomVelocity;
                for(int i = 0; i < vDensity; ++i)
                {
                    midiVelocities.Add((byte)newVelocity);

                    newVelocity += velocityDifference;
                    newVelocity = newVelocity < 0F ? 0F : newVelocity;
                    newVelocity = newVelocity > 127F ? 127F : newVelocity;
                }
            }
            return midiVelocities;
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

        SetDialogStateDelegate _setDialogState;
        public PaletteForm PaletteForm { get { return _paletteForm; } }
        PaletteForm _paletteForm;
        BasicChordControl _bcc;

        MIDIInstrumentsHelpForm _midiInstrumentsHelpForm;
        MidiPitchesHelpForm _midiPitchesHelpForm;
        #endregion  private variables
    }
    
}
