using System;
using System.Collections.Generic;
using System.Diagnostics;

using Krystals4ObjectLibrary;
using Moritz.Globals;
using Moritz.Midi;
using Moritz.Symbols;
using Moritz.Spec;

namespace Moritz.Palettes
{
    public class Palette
    {
        public Palette(PaletteForm paletteForm)
        {
            BasicChordFormSettings bcfs = new BasicChordFormSettings();
            bcfs.Durations = M.StringToIntList(paletteForm.BasicChordControl.DurationsTextBox.Text, ',');
            bcfs.Velocities = M.StringToByteList(paletteForm.BasicChordControl.VelocitiesTextBox.Text, ',');
            bcfs.MidiPitches = M.StringToByteList(paletteForm.BasicChordControl.MidiPitchesTextBox.Text, ',');
            bcfs.ChordOffs = M.StringToBoolList(paletteForm.BasicChordControl.ChordOffsTextBox.Text, ',');
            bcfs.ChordDensities = M.StringToByteList(paletteForm.BasicChordControl.ChordDensitiesTextBox.Text, ',');
            bcfs.Inversions = paletteForm.GetLinearInversions(paletteForm.BasicChordControl.RootInversionTextBox.Text);
            bcfs.InversionIndices = M.StringToIntList(paletteForm.BasicChordControl.InversionIndicesTextBox.Text, ',');
            bcfs.VerticalVelocityFactors = M.StringToFloatList(paletteForm.BasicChordControl.VerticalVelocityFactorsTextBox.Text, ',');

            _basicChordMidiSettings = new BasicChordMidiSettings(bcfs);

            _bankIndices = M.StringToByteList(paletteForm.BankIndicesTextBox.Text, ',');
            _patchIndices = M.StringToByteList(paletteForm.PatchIndicesTextBox.Text, ',');
            _repeats = M.StringToBoolList(paletteForm.RepeatsTextBox.Text, ',');
            _pitchwheelDeviations = M.StringToByteList(paletteForm.PitchwheelDeviationsTextBox.Text, ',');
            _pitchwheelEnvelopes = M.StringToByteLists(paletteForm.PitchwheelEnvelopesTextBox.Text);
            _panEnvelopes = M.StringToByteLists(paletteForm.PanEnvelopesTextBox.Text);
            _modulationWheelEnvelopes = M.StringToByteLists(paletteForm.ModulationWheelEnvelopesTextBox.Text);
            _expressionEnvelopes = M.StringToByteLists(paletteForm.ExpressionEnvelopesTextBox.Text);

            _ornamentNumbers = M.StringToIntList(paletteForm.OrnamentNumbersTextBox.Text, ',');
            _ornamentMinMsDurations = M.StringToIntList(paletteForm.MinMsDurationsTextBox.Text, ',');

            _ornamentSettings = null;
            if(paletteForm.OrnamentSettingsForm != null)
            {
                _ornamentSettings = new OrnamentSettings(paletteForm);
            }

            for(int chordIndex = 0; chordIndex < _basicChordMidiSettings.Durations.Count; ++chordIndex)
            {
                DurationDef dd = GetDurationDef(chordIndex);
                _durationDefs.Add(dd);
            }
        }

        /// <summary>
        /// A temporary palette containing a single DurationDef.
        /// Used just to construct the single DurationDef with the existing code.
        /// </summary>
        /// <param name="paletteChordForm"></param>
        public Palette(PaletteChordForm paletteChordForm)
        {
            BasicChordFormSettings bcfs = new BasicChordFormSettings();
            bcfs.Durations = M.StringToIntList(paletteChordForm.DurationTextBox.Text, ',');
            bcfs.Velocities = M.StringToByteList(paletteChordForm.VelocityTextBox.Text, ',');
            bcfs.MidiPitches = M.StringToByteList(paletteChordForm.BaseMidiPitchTextBox.Text,  ',');
            bcfs.ChordOffs = M.StringToBoolList(paletteChordForm.ChordOffTextBox.Text, ',');
            bcfs.ChordDensities = M.StringToByteList(paletteChordForm.ChordDensityTextBox.Text, ',');
            bcfs.Inversions = paletteChordForm.PaletteForm.GetLinearInversions(paletteChordForm.PaletteForm.BasicChordControl.RootInversionTextBox.Text);
            bcfs.InversionIndices = M.StringToIntList(paletteChordForm.InversionIndexTextBox.Text, ',');
            bcfs.VerticalVelocityFactors = M.StringToFloatList(paletteChordForm.VerticalVelocityFactorTextBox.Text, ',');

            _basicChordMidiSettings = new BasicChordMidiSettings(bcfs);

            _bankIndices = M.StringToByteList(paletteChordForm.BankIndexTextBox.Text, ',');
            _patchIndices = M.StringToByteList(paletteChordForm.PatchIndexTextBox.Text, ',');
            _repeats = M.StringToBoolList(paletteChordForm.RepeatsTextBox.Text, ',');
            _pitchwheelDeviations = M.StringToByteList(paletteChordForm.PitchwheelDeviationTextBox.Text, ',');

            _pitchwheelEnvelopes = M.StringToByteLists(paletteChordForm.PitchwheelEnvelopeTextBox.Text);
            _panEnvelopes = M.StringToByteLists(paletteChordForm.PanEnvelopeTextBox.Text);
            _modulationWheelEnvelopes = M.StringToByteLists(paletteChordForm.ModulationWheelEnvelopeTextBox.Text);
            _expressionEnvelopes = M.StringToByteLists(paletteChordForm.ExpressionEnvelopeTextBox.Text);

            _ornamentNumbers = M.StringToIntList(paletteChordForm.OrnamentNumberTextBox.Text, ',');
            _ornamentMinMsDurations = M.StringToIntList(paletteChordForm.MinMsDurationsTextBox.Text, ',');

            _ornamentSettings = null;
            if(paletteChordForm.PaletteForm.OrnamentSettingsForm != null)
            {
                _ornamentSettings = new OrnamentSettings(paletteChordForm.PaletteForm);
            }

            DurationDef dd = GetDurationDef(0);
            _durationDefs.Add(dd);
        }

        public Palette(PercussionPaletteForm ppf)
        {
            BasicChordFormSettings bcfs = new BasicChordFormSettings();

            bcfs.Durations = M.StringToIntList(ppf.BasicChordControl.DurationsTextBox.Text, ',');
            bcfs.Velocities = M.StringToByteList(ppf.BasicChordControl.VelocitiesTextBox.Text, ',');
            bcfs.MidiPitches = M.StringToByteList(ppf.BasicChordControl.MidiPitchesTextBox.Text, ',');
            //percussion defaults
            bcfs.ChordOffs = new List<bool>();
            bcfs.ChordDensities = new List<byte>();
            bcfs.Inversions = new List<List<byte>>();
            bcfs.InversionIndices = new List<int>();
            bcfs.VerticalVelocityFactors = new List<float>();
            foreach(byte pitch in bcfs.MidiPitches)
            {
                bcfs.ChordOffs.Add(false); // Moritz never sends ChordOffs to channel 9
                bcfs.ChordDensities.Add((byte)1); // Moritz only ever uses 1 percussion instrument per chord.
            }

            _basicChordMidiSettings = new BasicChordMidiSettings(bcfs);

            _bankIndices = new List<byte>();
            _patchIndices = new List<byte>();
            for(int i = 0; i < bcfs.Durations.Count; ++i)
            { 
                // default for percussion palette (does not set a patch)
                _patchIndices.Add((byte) 0 );
            }
            _pitchwheelDeviations = new List<byte>();
            _pitchwheelEnvelopes = new List<List<byte>>();
            _modulationWheelEnvelopes = new List<List<byte>>();

            _panEnvelopes = ppf.GetEnvelopesAsByteLists(ppf.PanEnvelopesTextBox);
            _expressionEnvelopes = ppf.GetEnvelopesAsByteLists(ppf.ExpressionEnvelopesTextBox);

            _ornamentNumbers = M.StringToIntList(ppf.OrnamentNumbersTextBox.Text, ',');
            _ornamentMinMsDurations = M.StringToIntList(ppf.MinMsDurationsTextBox.Text, ',');

            // instruments in ornaments palette are absolute, not relative, so 'ignore' the
            // basic instrument index by setting it to 0.
            for(int i = 0; i < _ornamentNumbers.Count; ++i)
            {
                if(_ornamentNumbers[i] != 0) // is an ornament
                {
                    bcfs.MidiPitches[i] = 0;
                }
            }

            if(ppf.PercussionOrnamentsForm != null)
            {
                _ornamentSettings = new OrnamentSettings(ppf);
            }

            for(int chordIndex = 0; chordIndex < _basicChordMidiSettings.Durations.Count; ++chordIndex)
            {
                DurationDef dd = GetDurationDef(chordIndex);
                _durationDefs.Add(dd);
            }
        }

        /// <summary>
        /// Returns either a new RestDef or a new MidiChordDef
        /// In both cases, MsPosition is set to zero, Lyric is set to null.
        /// </summary>
        private DurationDef GetDurationDef(int index)
        {
            DurationDef rval = null;
            BasicChordMidiSettings bcms = _basicChordMidiSettings;

            if(bcms.MidiPitches[index].Count == 0)
            {
                /// RestDefs are immutable, and have no MsPosition property.
                /// UniqueRestDefs are mutable RestDefs with both MsPositon and MsDuration properties.
                int restMsDuration = bcms.Durations[index];
                rval = new RestDef(0, restMsDuration);
            }
            else
            {
                /// Create a new MidiChordDef (with msPosition=0, lyric=null) 
                bool hasChordOff = BoolOrDefaultValue(bcms.ChordOffs, index, M.DefaultHasChordOff); // true
                int duration = bcms.Durations[index];
                List<byte> rootMidiPitches = bcms.MidiPitches[index];
                List<byte> rootMidiVelocities = bcms.Velocities[index];

                byte? bankIndex = ByteOrNull(_bankIndices, index);
                byte? patchIndex = ByteOrNull(_patchIndices, index);
                bool repeat = BoolOrDefaultValue(_repeats, index, M.DefaultChordRepeats); // false
                byte pitchwheelDeviation = ByteOrDefaultValue(_pitchwheelDeviations, index, M.DefaultPitchWheelDeviation); // 2
                List<byte> pitchwheelEnvelope = ListByte(_pitchwheelEnvelopes, index);
                List<byte> panEnvelope = ListByte(_panEnvelopes, index);
                List<byte> modulationWheelEnvelope = ListByte(_modulationWheelEnvelopes, index);
                List<byte> expressionEnvelope = ListByte(_expressionEnvelopes, index);

                MidiChordSliderDefs midiChordSliderDefs =
                    new MidiChordSliderDefs(pitchwheelEnvelope,
                                            panEnvelope,
                                            modulationWheelEnvelope,
                                            expressionEnvelope);

                OrnamentSettings os = _ornamentSettings;
                List<BasicMidiChordDef> basicMidiChordDefs = new List<BasicMidiChordDef>();
                int ornamentNumber;
                if(os == null || _ornamentNumbers[index] == 0)
                {
                    ornamentNumber = 0;
                    BasicMidiChordDef bmcd = new BasicMidiChordDef(duration, bankIndex, patchIndex, hasChordOff, rootMidiPitches, rootMidiVelocities);
                    basicMidiChordDefs.Add(bmcd);
                }
                else
                {
                    ornamentNumber = _ornamentNumbers[index];
                    int ornamentMinMsDuration = IntOrDefaultValue(_ornamentMinMsDurations, index, M.DefaultOrnamentMinimumDuration); // 1

                    List<int> ornamentValues = os.OrnamentValues[_ornamentNumbers[index] - 1];

                    for(int i = 0; i < ornamentValues.Count; ++i)
                    {
                        int oIndex = ornamentValues[i] - 1;
                        bool oHasChordOff = BoolOrDefaultValue(os.BasicChordMidiSettings.ChordOffs, oIndex, M.DefaultHasChordOff);
                        int oDuration = os.BasicChordMidiSettings.Durations[oIndex];
                        List<byte> oMidiPitches = os.BasicChordMidiSettings.MidiPitches[oIndex];
                        List<byte> oVelocities = os.BasicChordMidiSettings.Velocities[oIndex];
                        byte? oBank = ByteOrNull(os.BankIndices, oIndex);
                        byte? oPatch = ByteOrNull(os.PatchIndices, oIndex);

                        BasicMidiChordDef bmcd = new BasicMidiChordDef(oDuration, oBank, oPatch, oHasChordOff, oMidiPitches, oVelocities);
                        basicMidiChordDefs.Add(bmcd);
                    }

                    // The basicMidiChordDefs currently contain the values from the ornaments form.
                    // All oBank and oPatch values will be null if the corresponding field in the ornament form was empty.
                    // The durations, pitches and velocities are relative to the main palette's values.

                    RemoveDuplicateBankAndPatchValues(basicMidiChordDefs);

                    if(basicMidiChordDefs[0].BankIndex == null)
                        basicMidiChordDefs[0].BankIndex = bankIndex; // can be null
                    if(basicMidiChordDefs[0].PatchIndex == null)
                        basicMidiChordDefs[0].PatchIndex = patchIndex;

                    Debug.Assert(basicMidiChordDefs[0].PatchIndex != null);

                    basicMidiChordDefs = Moritz.Spec.MidiChordDef.FitToDuration(basicMidiChordDefs, duration, ornamentMinMsDuration);

                    foreach(BasicMidiChordDef b in basicMidiChordDefs)
                    {
                        List<byte> combinedPitches = new List<byte>();
                        foreach(byte pitch in b.Pitches)
                        {
                            foreach(byte rootMidiPitch in rootMidiPitches)
                            {
                                combinedPitches.Add(M.MidiValue(rootMidiPitch + pitch));
                            }
                        }
                        b.Pitches = combinedPitches;

                        List<byte> combinedVelocities = new List<byte>();
                        foreach(byte velocity in b.Velocities)
                        {
                            foreach(byte rootMidiVelocity in rootMidiVelocities)
                            {
                                combinedVelocities.Add(M.MidiValue(rootMidiVelocity + velocity));
                            }
                        }
                        b.Velocities = combinedVelocities;
                    }
                }

                rval = new MidiChordDef(
                    duration,
                    repeat,
                    pitchwheelDeviation,
                    hasChordOff,
                    rootMidiPitches,
                    ornamentNumber,
                    midiChordSliderDefs,
                    basicMidiChordDefs);
            }
            return rval;
        }

        /// <summary>
        /// Removes superfluous bank and patch values (setting them to null) so that duplicate messages will not be constructed.
        /// </summary>
        /// <param name="bmcds"></param>
        private void RemoveDuplicateBankAndPatchValues(List<BasicMidiChordDef> bmcds)
        {
            if(bmcds.Count > 1)
            {
                byte? prevBank = bmcds[0].BankIndex;
                byte? prevPatch = bmcds[0].PatchIndex;
                for(int i = 1; i < bmcds.Count; ++i)
                {
                    bmcds[i].BankIndex = (bmcds[i].BankIndex == null || bmcds[i].BankIndex == prevBank) ? null : bmcds[i].BankIndex;
                    prevBank = (bmcds[i].BankIndex == null) ? prevBank : bmcds[i].BankIndex;

                    bmcds[i].PatchIndex = (bmcds[i].PatchIndex == null || bmcds[i].PatchIndex == prevPatch) ? null : bmcds[i].PatchIndex;
                    prevPatch = (bmcds[i].PatchIndex == null) ? prevPatch : bmcds[i].PatchIndex;
                }
            }
        }

        /// <summary>
        /// Returns defaultValue if values is null or empty.
        /// Otherwise throws an exception if index is out of range.
        /// </summary>
        private bool BoolOrDefaultValue(List<bool> values, int index, bool defaultValue)
        {
            return (values != null && values.Count > 0) ? values[index] : defaultValue;
        }

        /// <summary>
        /// Returns null if values is null or empty.
        /// Otherwise throws an exception if index is out of range.
        /// </summary>
        private byte? ByteOrNull(List<byte> values, int index)
        {
            return (values == null || values.Count == 0) ? (byte?)null : values[index];
        }

        /// <summary>
        /// Returns defaultValue if values is empty.
        /// Otherwise throws an exception if index is out of range.
        /// </summary>
        private byte ByteOrDefaultValue(List<byte> values, int index, int defaultValue)
        {
            return (values.Count > 0) ? values[index] : M.MidiValue(defaultValue);
        }

        /// <summary>
        /// Returns defaultValue if values is empty.
        /// Otherwise throws an exception if index is out of range.
        /// </summary>
        private int IntOrDefaultValue(List<int> values, int index, int defaultValue)
        {
            return (values.Count > 0) ? values[index] : defaultValue;
        }

        /// <summary>
        /// Returns an empty list if values is empty.
        /// Otherwise throws an exception if index is out of range.
        /// </summary>
        private List<byte> ListByte(List<List<byte>> values, int index)
        {
            return (values.Count > 0) ? values[index] : new List<byte>();
        }

        private BasicChordMidiSettings _basicChordMidiSettings;
        private List<byte> _bankIndices;
        private List<byte> _patchIndices;
        private List<bool> _repeats;
        private List<byte> _pitchwheelDeviations;
        private List<List<byte>> _pitchwheelEnvelopes;
        private List<List<byte>> _panEnvelopes;
        private List<List<byte>> _modulationWheelEnvelopes;
        private List<List<byte>> _expressionEnvelopes;

        private List<int> _ornamentNumbers;
        private List<int> _ornamentMinMsDurations;
        private OrnamentSettings _ornamentSettings;

        private List<DurationDef> _durationDefs = new List<DurationDef>();

        public int Count { get { return _durationDefs.Count; } }
        public IUniqueDef UniqueDurationDef(int index)
        {
            return _durationDefs[index].DeepClone();
        }
        /// <summary>
        /// Returns a MidiChordDef if the object at index is a MidiChordDef,
        /// otherwise throws an exception.
        /// </summary>
        public MidiChordDef MidiChordDef(int index)
        {
            MidiChordDef midiChordDef = _durationDefs[index].DeepClone() as MidiChordDef;
            if(midiChordDef == null)
            {
                throw new ApplicationException("The indexed object was not a MidiChordDef.");
            }

            return midiChordDef;
        }

        public OutputVoiceDef NewOutputVoiceDef(List<int> sequence)
        {
            List<IUniqueDef> iuds = new List<IUniqueDef>();
            int msPosition = 0;
            foreach(int value in sequence)
            {
                Debug.Assert((value > 0 && value <= this.Count), "Illegal argument: value out of range in sequence");

                IUniqueDef iumdd = this.UniqueDurationDef(value - 1);
                iumdd.MsPosition = msPosition;
                msPosition += iumdd.MsDuration;
                iuds.Add(iumdd);
            }
            OutputVoiceDef ovd = new OutputVoiceDef(iuds);
            return ovd;
        }

        public OutputVoiceDef NewOutputVoiceDef(Krystal krystal)
        {
            List<int> sequence = krystal.GetValues((uint)1)[0];
            return NewOutputVoiceDef(sequence);
        }

        /// <summary>
        /// Constructs an OutputVoiceDef at MsPosition=0, containing a clone of the sequence of DurationDefs in the PaletteDef.
        /// </summary
        public OutputVoiceDef NewOutputVoiceDef()
        {
            List<int> sequence = new List<int>();
            for(int i = 1; i <= Count; ++i)
            {
                sequence.Add(i);
            }
            return NewOutputVoiceDef(sequence);
        }
    }

    public class OrnamentSettings
    {
        public OrnamentSettings(PaletteForm kpf)
        {
            OrnamentSettingsForm osf = kpf.OrnamentSettingsForm;
            Debug.Assert(osf != null);

            BasicChordFormSettings bcs = new BasicChordFormSettings();
            /// relative durations
            bcs.Durations = M.StringToIntList(osf.BasicChordControl.DurationsTextBox.Text, ',');
            /// velocity increments
            bcs.Velocities = M.StringToByteList(osf.BasicChordControl.VelocitiesTextBox.Text, ',');
            /// transposition intervals
            bcs.MidiPitches = M.StringToByteList(osf.BasicChordControl.MidiPitchesTextBox.Text, ',');
            bcs.ChordOffs = M.StringToBoolList(osf.BasicChordControl.ChordOffsTextBox.Text, ',');
            bcs.ChordDensities = M.StringToByteList(osf.BasicChordControl.ChordDensitiesTextBox.Text, ',');
            bcs.Inversions = kpf.GetLinearInversions(osf.BasicChordControl.RootInversionTextBox.Text);
            bcs.InversionIndices = M.StringToIntList(osf.BasicChordControl.InversionIndicesTextBox.Text, ',');
            bcs.VerticalVelocityFactors = M.StringToFloatList(osf.BasicChordControl.VerticalVelocityFactorsTextBox.Text, ',');

            BasicChordMidiSettings = new BasicChordMidiSettings(bcs);
            // if BankIndices or PatchIndices != null, their values override the values in the upper BasicMidiChord
            if(osf.BankIndicesTextBox.Text == "")
            {
                BankIndices = null;
            }
            else
            {
                BankIndices = M.StringToByteList(osf.BankIndicesTextBox.Text, ',');
            }
            if(osf.PatchIndicesTextBox.Text == "")
            {
                PatchIndices = null;
            }
            else
            {
                PatchIndices = M.StringToByteList(osf.PatchIndicesTextBox.Text, ',');
            }

            Krystal ornamentsKrystal = osf.OrnamentsKrystal;
            uint ornamentLevel = uint.Parse(osf.OrnamentsLevelTextBox.Text);
            OrnamentValues = ornamentsKrystal.GetValues(ornamentLevel);

        }

        public OrnamentSettings(PercussionPaletteForm ppf)
        {
            PercussionOrnamentsForm pof = ppf.PercussionOrnamentsForm;
            Debug.Assert(pof != null);

            BasicChordFormSettings bcs = new BasicChordFormSettings();

            //percussion defaults
            bcs.ChordOffs = new List<bool>();
            bcs.ChordDensities = new List<byte>();
            bcs.Inversions = new List<List<byte>>();
            bcs.InversionIndices = new List<int>();
            bcs.VerticalVelocityFactors = new List<float>();

            bcs.Durations = M.StringToIntList(pof.BasicPercussionControl.DurationsTextBox.Text, ',');
            /// velocity increments
            bcs.Velocities = M.StringToByteList(pof.BasicPercussionControl.VelocitiesTextBox.Text, ',');
            /// transposition intervals (for percussion ornaments, the "base pitch=instrument" has been set to 0.
            bcs.MidiPitches = M.StringToByteList(pof.BasicPercussionControl.MidiPitchesTextBox.Text, ',');

            foreach(byte pitch in bcs.MidiPitches)
            {
                bcs.ChordOffs.Add(false); // Moritz never sends ChordOffs to channel 9
                bcs.ChordDensities.Add((byte)1); // Moritz only ever uses 1 percussion instrument per chord.
            }

            BasicChordMidiSettings = new BasicChordMidiSettings(bcs);
            BankIndices = null;
            PatchIndices = null;

            Krystal ornamentsKrystal = pof.OrnamentsKrystal;
            uint ornamentLevel = uint.Parse(pof.OrnamentsLevelTextBox.Text);
            OrnamentValues = ornamentsKrystal.GetValues(ornamentLevel);
        }

        /// <summary>
        /// 'byte' types are midi (msb) parameters in range 0..127.
        /// </summary>
        public BasicChordMidiSettings BasicChordMidiSettings;
        public List<byte> BankIndices;
        public List<byte> PatchIndices;
        public List<List<int>> OrnamentValues;
    }

    public class BasicChordFormSettings
    {
        /// <summary>
        /// 'byte' types are midi (msb) parameters in range 0..127.
        /// </summary>
        public List<int> Durations;
        public List<byte> Velocities;
        public List<byte> MidiPitches;
        public List<bool> ChordOffs;
        public List<byte> ChordDensities;
        // The following is a linear array calculated from ornament.rootInversion;
        // If Inversions.Count == 0, then there is a maximum chord density of 1, and there are no inversions.
        // If Inversions.Count == 1, then the contained list contains a single interval for a 2-note chord.
        // Otherwise Inversions.Count == (n-1)*2, where the contained intLists.Count == n.
        public List<List<byte>> Inversions;
        public List<int> InversionIndices;
        public List<float> VerticalVelocityFactors;
    }

    public class BasicChordMidiSettings
    {
        /// <summary>
        /// Converts the form settings to the actual pitches and velocities
        /// </summary>
        /// <param name="bcfs"></param>
        public BasicChordMidiSettings(BasicChordFormSettings bcfs)
        {
            Durations = new List<int>(bcfs.Durations);
            ChordOffs = new List<bool>(bcfs.ChordOffs);
            for(int chordIndex = 0; chordIndex < bcfs.Durations.Count; ++chordIndex)
            {
                List<byte> midiPitches = new List<byte>();
                List<byte> midiVelocities = new List<byte>();
                float verticalVelocityFactor = 1.0F;
                if(bcfs.VerticalVelocityFactors.Count > chordIndex)
                    verticalVelocityFactor = bcfs.VerticalVelocityFactors[chordIndex];

                if(bcfs.ChordDensities[chordIndex] > 0)
                {
                    if(bcfs.ChordDensities[chordIndex] == 1)
                    {
                        midiPitches.Add(bcfs.MidiPitches[chordIndex]);
                        midiVelocities.Add(bcfs.Velocities[chordIndex]);
                    }
                    else
                    {
                        midiPitches = GetMidiPitches(chordIndex, bcfs.MidiPitches, bcfs.ChordDensities, bcfs.Inversions, bcfs.InversionIndices);
                        midiVelocities = GetVerticalVelocities(bcfs.Velocities[chordIndex], bcfs.ChordDensities[chordIndex], verticalVelocityFactor);
                    }

                }
                MidiPitches.Add(midiPitches);
                Velocities.Add(midiVelocities);
            }
        }

        private List<byte> GetMidiPitches(int chordIndex, List<byte> rootPitches, List<byte> densities, List<List<byte>> inversions, List<int> inversionIndices)
        {
            List<byte> primeIntervals = null;
            Debug.Assert(inversions != null);
            if(inversions.Count > 1)
            {
                int inversionIndex = inversionIndices[chordIndex];
                Debug.Assert(inversions.Count > inversionIndex);
                primeIntervals = inversions[inversionIndex];
            }
            else if(inversions.Count == 1)
            {
                primeIntervals = inversions[0];
            }
            // If krystalPalette.Inversions.Count == 0, primeIntervals is empty.

            byte rootPitch = rootPitches[chordIndex];
            int nUpperPitches = densities[chordIndex] - 1;
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

        public List<int> Durations;
        public List<List<byte>> Velocities = new List<List<byte>>();
        public List<List<byte>> MidiPitches = new List<List<byte>>();
        public List<bool> ChordOffs;
    }
}
