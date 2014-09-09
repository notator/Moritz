using System.Collections.Generic;
using System.Diagnostics;

using Krystals4ObjectLibrary;
using Moritz.Globals;
using Moritz.Krystals;
using Moritz.Score;
using Moritz.Score.Midi;
using Moritz.Score.Notation;
using Moritz.AssistantPerformer;

namespace Moritz.AssistantComposer
{
    internal class ComposableBasicMidiChordDef : BasicMidiChordDef
    {
        /// <summary>
        /// Used when the chordsymbol is not an ornament
        /// </summary>
        /// <param name="krystalPalette"></param>
        /// <param name="valueIndex"></param>
        public ComposableBasicMidiChordDef(Palette krystalPalette, int valueIndex)
            : this(krystalPalette.BasicChordSettings, krystalPalette._bankIndices, krystalPalette._patchIndices, valueIndex)
        {
        }

        /// <summary>
        /// Used when creating a BasicMidiChord which is part of an ornament.
        /// </summary>
        /// <param name="krystalPalette"></param>
        /// <param name="valueIndex"></param>
        public ComposableBasicMidiChordDef(OrnamentSettings ornamentSettings, int index)
            : this(ornamentSettings.BasicChordSettings, ornamentSettings.BankIndices, ornamentSettings.PatchIndices, index)
        {
        }

        public ComposableBasicMidiChordDef(BasicChordFormSettings bcs, List<byte> bankIndices, List<byte> patchIndices, int index)
        {
            // MsPosition is set when all the MsDurations of all chords and ornamentChords have been finally set.

            Debug.Assert(bcs.Durations != null && bcs.Durations.Count > 0);
            Debug.Assert(bcs.Velocities != null && bcs.Velocities.Count > 0);
            Debug.Assert(bcs.MidiPitches != null && bcs.MidiPitches.Count > 0);
            Debug.Assert(bcs.ChordDensities != null && bcs.ChordDensities.Count > 0);

            _msDuration = bcs.Durations[index]; // recalculated later, if this is an ornament chord

            byte density = bcs.ChordDensities[index];
            Pitches = GetMidiNotes(bcs, index, density);
            Velocities = GetMidiVelocities(bcs, index, density);

            BankIndex = null;
            if(bankIndices != null && bankIndices.Count > 0)
            {
                BankIndex = (byte?)bankIndices[index];
            }

            PatchIndex = null; // percussion chords use PatchIndex = null;
            if(patchIndices != null && patchIndices.Count > 0)
            {
                PatchIndex = (byte?)patchIndices[index];
            }

            HasChordOff = true; // default
            if(bcs.ChordOffs != null && bcs.ChordOffs.Count > 0)
            {
                HasChordOff = bcs.ChordOffs[index];
            }
        }

        /// <summary>
        /// Returns a list of midi pitch numbers in ascending order separated by space. 
        /// </summary>
        /// <param name="palette"></param>
        /// <param name="valueIndex"></param>
        /// <returns></returns>
        private List<byte> GetMidiNotes(BasicChordFormSettings bcs, int valueIndex, byte chordDensity)
        {
            List<byte> primeIntervals = new List<byte>();
            Debug.Assert(bcs.Inversions != null);
            if(bcs.Inversions.Count > 1)
            {
                Debug.Assert(bcs.InversionIndices != null && bcs.InversionIndices.Count > valueIndex);
                primeIntervals = bcs.Inversions[bcs.InversionIndices[valueIndex]];
            }
            else if(bcs.Inversions.Count == 1)
                primeIntervals.Add(bcs.Inversions[0][0]);
            // If krystalPalette.Inversions.Count == 0, primeIntervals is empty.

            byte midiPitch = bcs.MidiPitches[valueIndex];
            List<byte> midiPitches = new List<byte>();
            for(int p = 0; p < chordDensity; p++)
            {
                midiPitches.Add(midiPitch);
                if(p < (chordDensity - 1))
                {
                    int newpitch = midiPitch + primeIntervals[p];
                    midiPitch = (byte)((newpitch > 127) ? 127 : newpitch);
                }
            }
            return midiPitches;
        }

        private List<byte> GetMidiVelocities(BasicChordFormSettings bcs, int valueIndex, int noteCount)
        {
            byte basicMidiVelocity = bcs.Velocities[valueIndex];
            float verticalVelocityFactor = 1F;
            if(bcs.VerticalVelocityFactors != null && bcs.VerticalVelocityFactors.Count > 0)
            {
                verticalVelocityFactor = bcs.VerticalVelocityFactors[valueIndex];
            }

            List<byte> midiVelocities = new List<byte>();

            if(verticalVelocityFactor == 1F || noteCount == 1)
            {
                for(int i = 0; i < noteCount; ++i)
                {
                    midiVelocities.Add((byte)basicMidiVelocity);
                }
            }
            else
            {
                float bottomVelocity = basicMidiVelocity;
                if(verticalVelocityFactor > 1.0F)
                    bottomVelocity = bottomVelocity / verticalVelocityFactor;
                float topVelocity = bottomVelocity * verticalVelocityFactor;
                float velocityDifference = (topVelocity - bottomVelocity) / ((float)(noteCount - 1));
                float newVelocity = bottomVelocity;
                for(int i = 0; i < noteCount; ++i)
                {
                    midiVelocities.Add((byte)newVelocity);

                    newVelocity += velocityDifference;
                    newVelocity = newVelocity < 0F ? 0F : newVelocity;
                    newVelocity = newVelocity > 127F ? 127F : newVelocity;
                }
            }
            return midiVelocities;
        }
    }
}
