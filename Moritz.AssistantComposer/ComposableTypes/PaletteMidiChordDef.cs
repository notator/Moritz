using System.Collections.Generic;
using System.Diagnostics;

using Multimedia.Midi;

using Krystals4ObjectLibrary;
using Moritz.Globals;
using Moritz.Krystals;
using Moritz.Score;
using Moritz.Score.Midi;
using Moritz.Score.Notation;
using Moritz.AssistantPerformer;

namespace Moritz.AssistantComposer
{
    ///<summary>
    /// A MidiChordDef is a definition retrieved from a palette.
    /// (Palettes are saved in score options files (.mkss), not in the scores themselves.)
    /// MidiChordDefs are immutable, and have no MsPosition property. 
    /// A UniqueMidiChordDef is a mutable MidiChordDef having both MsPositon and msDuration attributes.
    //</summary>
    internal class PaletteMidiChordDef : MidiChordDef
    {
        /// <summary>
        /// A PaletteMidiChordDef is a component of a PaletteDef.
        /// PaletteMidiChordDefs are saved in score:midiChord in score:midiDefs in SVG files. 
        /// </summary>


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
