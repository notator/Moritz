using System.Collections.Generic;
using System.Diagnostics;

using Multimedia.Midi;

using Moritz.Globals;
using Moritz.Score.Midi;

namespace Moritz.AssistantComposer
{
    ///<summary>
    /// A PaletteMidiChordDef is a MidiChordDef which is saved in or retreived from a palette.
    /// PaletteMidiChordDefs can be 'used' in SVG files, but are usually converted to UniqueMidiChordDefs.
    /// Related classes:
    /// 1. A UniqueMidiChordDef is a MidiChordDef which is saved locally in an SVG file.
    /// 2. A LocalMidiChordDef is a UniqueMidiChordDef with additional MsPositon and msDuration attributes.
    //</summary>
    internal class PaletteMidiChordDef : MidiChordDef
    {
        /// <summary>
        /// A PaletteMidiChordDef is a component of a PaletteDef.
        /// PaletteMidiChordDefs are saved in score:midiChord in score:midiDefs in SVG files. 
        /// </summary>
        public PaletteMidiChordDef(string chordID, Palette krystalPalette, int valueIndex)
            : base()
        {
            ID = chordID;

            _msDuration = krystalPalette.BasicChordSettings.Durations[valueIndex];

            BasicChordSettings bcs = krystalPalette.BasicChordSettings;
            BasicMidiChordDef rootChord = new ComposableBasicMidiChordDef(krystalPalette, valueIndex);
            _midiHeadSymbols = rootChord.Notes;
            _midiVelocity = bcs.Velocities[valueIndex];
            if(krystalPalette.OrnamentNumbers != null && krystalPalette.OrnamentNumbers.Count > 0)
                _ornamentNumberSymbol = krystalPalette.OrnamentNumbers[valueIndex];

            Debug.Assert(bcs.Durations != null && bcs.Durations.Count > 0);

            if(krystalPalette.BankIndices != null && krystalPalette.BankIndices.Count > 0)
                _bank = krystalPalette.BankIndices[valueIndex];

            if(krystalPalette.PatchIndices != null && krystalPalette.PatchIndices.Count > 0)
                _patch = krystalPalette.PatchIndices[valueIndex];

            if(krystalPalette.Volumes != null && krystalPalette.Volumes.Count > 0)
                _volume = krystalPalette.Volumes[valueIndex];

            if(krystalPalette.Repeats != null && krystalPalette.Repeats.Count > 0)
                _repeat = krystalPalette.Repeats[valueIndex]; // default is true

            if(krystalPalette.PitchwheelDeviations != null && krystalPalette.PitchwheelDeviations.Count > 0)
                _pitchWheelDeviation = krystalPalette.PitchwheelDeviations[valueIndex];

            if(bcs.ChordOffs != null && bcs.ChordOffs.Count > 0)
                _hasChordOff = bcs.ChordOffs[valueIndex];

            int ornamentNumber = 0;
            if(krystalPalette.OrnamentNumbers != null & krystalPalette.OrnamentNumbers.Count > 0)
            {
                ornamentNumber = krystalPalette.OrnamentNumbers[valueIndex];
            }

            this._minimumBasicMidiChordMsDuration = M.DefaultMinimumBasicMidiChordMsDuration;
            if(krystalPalette.OrnamentMinMsDurations != null && krystalPalette.OrnamentMinMsDurations.Count > 0)
                this._minimumBasicMidiChordMsDuration = krystalPalette.OrnamentMinMsDurations[valueIndex];

            if(krystalPalette.OrnamentSettings == null || ornamentNumber == 0)
            {
                BasicMidiChordDefs.Add(rootChord);
                BasicMidiChordDefs[0] = new BasicMidiChordDef(BasicMidiChordDefs[0], bcs.Durations[valueIndex]); 
            }
            else
            {
                BasicMidiChordDefs = GetOrnamentChords(krystalPalette, rootChord, valueIndex);
            }

            List<byte> pitchWheelEnvelopeMsbs = null;
            if(krystalPalette.PitchwheelEnvelopes != null && krystalPalette.PitchwheelEnvelopes.Count > 0)
                pitchWheelEnvelopeMsbs = krystalPalette.PitchwheelEnvelopes[valueIndex];

            List<byte> panEnvelopeMsbs = null;
            if(krystalPalette.PanEnvelopes != null && krystalPalette.PanEnvelopes.Count > 0)
                panEnvelopeMsbs = krystalPalette.PanEnvelopes[valueIndex];

            List<byte> modulationWheelEnvelopeMsbs = null;
            if(krystalPalette.ModulationWheelEnvelopes != null && krystalPalette.ModulationWheelEnvelopes.Count > 0)
                modulationWheelEnvelopeMsbs = krystalPalette.ModulationWheelEnvelopes[valueIndex];

            List<byte> expressionEnvelopeMsbs = null;
            if(krystalPalette.ExpressionEnvelopes != null && krystalPalette.ExpressionEnvelopes.Count > 0)
                expressionEnvelopeMsbs = krystalPalette.ExpressionEnvelopes[valueIndex];

            if(pitchWheelEnvelopeMsbs != null || panEnvelopeMsbs != null || modulationWheelEnvelopeMsbs != null || expressionEnvelopeMsbs != null)
            {
                MidiChordSliderDefs = new MidiChordSliderDefs(pitchWheelEnvelopeMsbs, panEnvelopeMsbs, modulationWheelEnvelopeMsbs, expressionEnvelopeMsbs);
            }
        }

        /// <summary>
        /// Returns a list of ornament chords.
        /// The msPositions of the chords are NOT set.
        /// The List contains the maximum number of durations which can be fit from relativeDurations into
        /// the msOuterDuration such that no duration is less than minimumOrnamentChordMsDuration.
        /// The sum of all ornamentChord durations in the list is exactly msOuterDuration.
        /// </summary>
        /// <param name="msOuterPosition"></param>
        /// <param name="msOuterDuration"></param>
        /// <param name="ornamentSettings"></param>
        /// <param name="ornamentIndex"></param>
        /// <returns></returns>
        private List<BasicMidiChordDef> GetOrnamentChords(Palette krystalPalette, BasicMidiChordDef rootChord, int index)
        {
            Debug.Assert(krystalPalette.OrnamentMinMsDurations != null);
            Debug.Assert(krystalPalette.OrnamentNumbers != null && krystalPalette.OrnamentNumbers.Count > index);

            int outerDuration = krystalPalette.BasicChordSettings.Durations[index];

            int ornamentMinMsDuration = 1; // default
            if(krystalPalette.OrnamentMinMsDurations.Count > index)
                ornamentMinMsDuration = krystalPalette.OrnamentMinMsDurations[index];

            int ornamentIndex = krystalPalette.OrnamentNumbers[index] - 1;
            Debug.Assert(ornamentIndex >= 0);

            List<BasicMidiChordDef> ornamentChords =
                GetOrnamentSettings(krystalPalette.OrnamentSettings, ornamentIndex, outerDuration, ornamentMinMsDuration);

            DoTranspositionAndVelocities(rootChord.Notes, rootChord.Velocities, ornamentChords);

            return ornamentChords;
        }

        /// <summary>
        /// Sets ornamentChords.Notes and ornamentChords.Velocities.
        /// These lists are parallel, and contain unique pitches (in ascending order) and their corresponding velocities.
        /// </summary>
        /// <param name="rootNotes"></param>
        /// <param name="ornamentChords"></param>
        private void DoTranspositionAndVelocities(List<byte> rootNotes, List<byte> rootVelocities, List<BasicMidiChordDef> ornamentChords)
        {
            Debug.Assert(rootNotes.Count == rootVelocities.Count);

            List<byte> newNotes = new List<byte>();
            List<byte> newVelocities = new List<byte>();

            foreach(BasicMidiChordDef ornamentChord in ornamentChords)
            {
                for(int i = 0; i < rootNotes.Count; ++i)
                {
                    byte rootPitch = rootNotes[i];
                    byte rootVelocity = rootVelocities[i];
                    newNotes.AddRange(NewByteList(rootPitch, ornamentChord.Notes));
                    newVelocities.AddRange(NewByteList(rootVelocity, ornamentChord.Velocities));
                }
                ornamentChord.Notes = new List<byte>(newNotes);
                ornamentChord.Velocities = new List<byte>(newVelocities);
                RemoveDuplicatePitches(ornamentChord.Notes, ornamentChord.Velocities);
                newNotes.Clear();
                newVelocities.Clear();
            }
        }

        private List<byte> NewByteList(byte root, List<byte> byteList)
        {
            List<byte> newByteList = new List<byte>();
            foreach(byte b in byteList)
            {
                int integer = (int)root + b;
                byte newByte = (byte)(integer > 127 ? 127 : integer);
                newByteList.Add(newByte);
            }
            return newByteList; // can contain duplicates
        }
        /// <summary>
        /// The input pitches can contain duplicate values which have to be removed.
        /// Returns a list of unique pitches in ascending order, and a parallel list of corresponding velocities.
        /// The returned corresponding velocity is the largest velocity corresponding to any of the corresponding 
        /// duplicate pitches in the input pitches list.
        /// </summary>
        /// <param name="notes"></param>
        /// <param name="velocities"></param>
        private void RemoveDuplicatePitches(List<byte> pitches, List<byte> velocities)
        {
            Debug.Assert(pitches.Count == velocities.Count);
            List<byte> uniquePitches = new List<byte>();
            foreach(byte pitch in pitches)
            {
                if(!uniquePitches.Contains(pitch))
                    uniquePitches.Add(pitch);
            }
            uniquePitches.Sort(); // the completed output pitch list.
            List<byte> newVelocities = new List<byte>();
            foreach(byte uniquePitch in uniquePitches)
            {
                byte maxVelocity = 0;
                for(int i = 0; i < pitches.Count; ++i)
                {
                    if((pitches[i] == uniquePitch) && (velocities[i] > maxVelocity))
                        maxVelocity = velocities[i];
                }
                newVelocities.Add(maxVelocity);
            }
            pitches.Clear();
            foreach(byte pitch in uniquePitches)
                pitches.Add(pitch);
            velocities.Clear();
            foreach(byte velocity in newVelocities)
                velocities.Add(velocity);
            Debug.Assert(pitches.Count == velocities.Count);
        }

        private List<BasicMidiChordDef> GetOrnamentSettings(OrnamentSettings ornamentSettings, int ornamentIndex,
            int msOuterDuration, int ornamentMinMsDuration)
        {
            Debug.Assert(ornamentSettings != null && ornamentIndex >= 0);

            List<int> ornamentNumbers = GetOrnamentNumbers(ornamentSettings, ornamentIndex);
            List<BasicMidiChordDef> firstPassChords = new List<BasicMidiChordDef>();

            foreach(int number in ornamentNumbers)
            {
                firstPassChords.Add(new ComposableBasicMidiChordDef(ornamentSettings, number - 1));
            }

            List<BasicMidiChordDef> ornamentChords = FitToDuration(firstPassChords, msOuterDuration, ornamentMinMsDuration);
            return ornamentChords;
        }

        private List<int> GetOrnamentNumbers(OrnamentSettings ornamentSettings, int ornamentIndex)
        {
            List<List<int>> ornamentValuesList =
                ornamentSettings.OrnamentsKrystal.GetValues((uint)ornamentSettings.OrnamentLevel);

            return ornamentValuesList[ornamentIndex];
        }
    }
}
