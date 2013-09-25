using System;
using System.Collections.Generic;
using System.Diagnostics;

using Krystals4ObjectLibrary;

using Moritz.Score;
using Moritz.Score.Midi;

namespace Moritz.AssistantComposer
{
    class Winds
    {
        public Winds(List<Krystal> krystals, List<PaletteDef> paletteDefs, int nBaseChords)
        {
            MidiDefSequence baseMidiDefSequence = GetBaseMidiDefSequence(krystals[0], paletteDefs[0], nBaseChords);
            MidiDefSequences.Add(baseMidiDefSequence);

            MidiDefSequence tenorMidiDefSequence = GetTenorMidiDefSequence(baseMidiDefSequence);
            MidiDefSequences.Add(tenorMidiDefSequence);

            SetBaseWindIndexLyrics(baseMidiDefSequence);

            SetTenorWindIndexLyrics(tenorMidiDefSequence);

            // etc. -- create and add other wind voices, so that wind channels are ordered bottom to top.
            // (MidiDefSequences[0] is the base Wind sequence.
        }

        private MidiDefSequence GetBaseMidiDefSequence(Krystal krystal, PaletteDef paletteDef, int nBaseChords)
        {
            List<List<int>> kValues = krystal.GetValues((uint)1);
            List<int> sequence = kValues[0]; // the flat list of values
            sequence = sequence.GetRange(0, nBaseChords);

            MidiDefSequence baseMidiDefSequence = new MidiDefSequence(paletteDef, sequence);

            baseMidiDefSequence.Transpose(-4);

            BaseWindKrystalStrandIndices = GetBaseWindStrandIndices(krystal, nBaseChords);

            return baseMidiDefSequence;
        }

        private MidiDefSequence GetTenorMidiDefSequence(MidiDefSequence baseMidiDefSequence)
        {
            MidiDefSequence tenorMidiDefSequence = baseMidiDefSequence.Clone();

            tenorMidiDefSequence.LocalizedMidiDurationDefs.Reverse();
            tenorMidiDefSequence.MsPosition = 0;
            tenorMidiDefSequence.Transpose(24);

            return tenorMidiDefSequence;
        }

        private List<int> GetBaseWindStrandIndices(Krystal krystal, int nBaseChords)
        {
            List<int> strandIndices = new List<int>();
            List<Strand> strands = krystal.Strands;
            int index = 0;
            foreach(Strand strand in strands)
            {
                if(index >= nBaseChords)
                {
                    break;
                }
                strandIndices.Add(index);
                index += strand.Values.Count;
            }
            return strandIndices;
        }

        private void SetBaseWindIndexLyrics(MidiDefSequence baseMidiDefSequence)
        {
            SetLyrics(baseMidiDefSequence, BaseWindKrystalStrandIndices);
        }

        private void SetTenorWindIndexLyrics(MidiDefSequence tenorMidiDefSequence)
        {
            List<int> tenorWindKrystalStrandIndices = new List<int>(){0};
            int count = tenorMidiDefSequence.Count;
            foreach(int index in BaseWindKrystalStrandIndices)
            {
                int newIndex = count - index;
                if(newIndex > 0 && newIndex < count)
                {
                    tenorWindKrystalStrandIndices.Add(newIndex);
                }
            }

            SetLyrics(tenorMidiDefSequence, tenorWindKrystalStrandIndices);
        }

        private void SetLyrics(MidiDefSequence midiDefSequence, List<int> strandIndices)
        {
            for(int index = 0; index < midiDefSequence.Count; ++index)
            {
                LocalMidiChordDef lmcd = midiDefSequence[index].LocalMidiDurationDef as LocalMidiChordDef;
                if(lmcd != null)
                {
                    if(strandIndices.Contains(index))
                    {
                        lmcd.Lyric = ">>" + index.ToString() + "<<";
                    }
                    else
                    {
                        lmcd.Lyric = index.ToString();
                    }
                }
            }
        }

        // each voice has the duration of the whole piece
        internal List<Voice> GetVoices(int topWindChannelIndex)
        {
            List<Voice> voices = new List<Voice>();
            int windChannelIndex = topWindChannelIndex;
            for(int i = MidiDefSequences.Count - 1; i >= 0; --i)
            {
                Voice voice = new Voice(null, (byte)(windChannelIndex++));
                voice.LocalizedMidiDurationDefs = MidiDefSequences[i].LocalizedMidiDurationDefs;
                voices.Add(voice);
            }
            return voices;
        }

        internal List<int> AddInterludeBarlinePositions(List<int> barlineMsPositions)
        {
            List<int> newBarlineIndices = new List<int>() { 1, 3, 5, 15, 27, 40, 45, 57, 63, 77 }; // by inspection of the score
            MidiDefSequence bassWind = MidiDefSequences[0];
            foreach(int index in newBarlineIndices)
            {
                barlineMsPositions.Add(bassWind.LocalizedMidiDurationDefs[index].MsPosition);
            }
            barlineMsPositions.Sort();

            return barlineMsPositions;
        }

        /// <summary>
        /// Moves certain LocalizedMidiDurationDefs in both bass and treble winds to align with
        /// LocalizedMidiDurationDefs in clytemnestra.
        /// LocalizedMidiDurationDefs which are already at a barlineMsPosition may not move.
        /// </summary>
        /// <param name="barlineMsPositions"></param>
        internal void AlignChords(MidiDefSequence clytemnestra, List<int> barlineMsPositions)
        {
            List<int> barMsPositions = new List<int>(barlineMsPositions);
            barMsPositions.Insert(0, 0);
            MidiDefSequence bassWind = MidiDefSequences[0];
            MidiDefSequence tenorWind = MidiDefSequences[1];

            int barNumber = 10;
            int anchor1Index = 10;
            int alignIndex = 12;
            int anchor2Index = 15;
            //bassWind.AdjustDefMsPosition(barMsPositions, anchor1index, alignIndex, anchor2index, toMsPos);
            tenorWind.AlignChordOrRest(barMsPositions, anchor1Index, alignIndex, anchor2Index, barMsPositions[barNumber - 1]);

        }

        #region attributes
        internal List<int> BaseWindKrystalStrandIndices = new List<int>();

        // each MidiDefSequence has the duration of the whole piece
        // The sequences are in bottom to top order. MidiDefSequences[0] is the base Wind
        internal List<MidiDefSequence> MidiDefSequences = new List<MidiDefSequence>();
        #endregion
    }
}
