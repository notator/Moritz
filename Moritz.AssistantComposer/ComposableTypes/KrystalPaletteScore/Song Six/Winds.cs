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
            List<int> newBarlineIndices = new List<int>() { 1, 3, 5, 15, 27, 40, 45, 63, 77 }; // by inspection of the score
            MidiDefSequence bassWind = MidiDefSequences[0];
            foreach(int index in newBarlineIndices)
            {
                barlineMsPositions.Add(bassWind.LocalizedMidiDurationDefs[index].MsPosition);
            }
            barlineMsPositions.Sort();

            return barlineMsPositions;
        }

        // I've commented the following function out because I don't after all want to align so many wind chords.
        // Going back to no bass or tenor chords aligned. Alto and soprano winds will be constructed inside the
        // tenor and base chords, and _they_ will probably be aligned. I need to finish composing the winds
        // before doing the aligning...

        ///// <summary>
        ///// Moves certain LocalizedMidiDurationDefs in both bass and treble winds to align with
        ///// LocalizedMidiDurationDefs in clytemnestra.
        ///// LocalizedMidiDurationDefs which are already at a barlineMsPosition may not move.
        ///// </summary>
        ///// <param name="barlineMsPositions"></param>
        //internal void AlignChords(MidiDefSequence clytemnestra, List<int> barlineMsPositions)
        //{
        //    List<int> barMsPositions = new List<int>(barlineMsPositions);
        //    barMsPositions.Insert(0, 0);
        //    MidiDefSequence bassWind = MidiDefSequences[0];
        //    MidiDefSequence tenorWind = MidiDefSequences[1];

        //    #region tenor wind
        //    // verse 1
        //    List<int> clytemnestraIndices = new List<int>() { 3, 12, 18, 27, 39, 49 };
        //    for(int i = 10; i < 16; ++i)
        //    {
        //        tenorWind.AlignChordOrRest(barMsPositions, i - 1, i, i + 1, clytemnestra[clytemnestraIndices[i - 10]].MsPosition);
        //    }
        //    // interlude
        //    tenorWind.AlignChordOrRest(barMsPositions, 15, 19, 21, bassWind[18].MsPosition);
        //    tenorWind.AlignChordOrRest(barMsPositions, 19, 21, 22, bassWind[20].MsPosition);
        //    // verse 2
        //    clytemnestraIndices = new List<int>() { 67, 81, 93, 101 };
        //    for(int i = 22; i < 26; ++i)
        //    {
        //        tenorWind.AlignChordOrRest(barMsPositions, i - 1, i, i + 1, clytemnestra[clytemnestraIndices[i - 22]].MsPosition);
        //    }
        //    // verse 3
        //    clytemnestraIndices = new List<int>() { 119, 124, 131, 141, 151, 162, 170 };
        //    for(int i = 34; i < 41; ++i)
        //    {
        //        tenorWind.AlignChordOrRest(barMsPositions, i - 1, i, i + 1, clytemnestra[clytemnestraIndices[i - 34]].MsPosition);
        //    }
        //    // verse 4
        //    clytemnestraIndices = new List<int>() { 177, 188, 198, 208, 217, 225, 240, 254, 267 };
        //    for(int i = 50; i < 59; ++i)
        //    {
        //        tenorWind.AlignChordOrRest(barMsPositions, i - 1, i, i + 1, clytemnestra[clytemnestraIndices[i - 50]].MsPosition);
        //    }
        //    // interlude
        //    tenorWind.AlignChordOrRest(barMsPositions, 59, 62, 72, bassWind[61].MsPosition);
        //    tenorWind.AlignChordOrRest(barMsPositions, 62, 64, 72, bassWind[63].MsPosition);
        //    tenorWind.AlignChordOrRest(barMsPositions, 64, 69, 72, bassWind[67].MsPosition);
        //    tenorWind.AlignChordOrRest(barMsPositions, 69, 71, 72, bassWind[69].MsPosition);
        //    // verse 5
        //    clytemnestraIndices = new List<int>() { 270,281 };
        //    for(int i = 72; i < 74; ++i)
        //    {
        //        tenorWind.AlignChordOrRest(barMsPositions, i - 1, i, i + 1, clytemnestra[clytemnestraIndices[i - 72]].MsPosition);
        //    }
        //    tenorWind.AlignChordOrRest(barMsPositions, 74, 81, 82, bassWind[81].MsPosition); // align the final note
        //    tenorWind.AlignChordOrRest(barMsPositions, 73, 74, 81, clytemnestra[287].MsPosition);
        //    #endregion
        //    #region bass wind
        //    // verse 1
        //    clytemnestraIndices = new List<int>() { 7, 16, 27, 38, 49, 58 };
        //    for(int i = 9; i < 15; ++i)
        //    {
        //        bassWind.AlignChordOrRest(barMsPositions, i - 1, i, i + 1, clytemnestra[clytemnestraIndices[i - 9]].MsPosition);
        //    }
        //    // verse 2
        //    clytemnestraIndices = new List<int>() { 71, 86, 94, 107 };
        //    for(int i = 21; i < 25; ++i)
        //    {
        //        bassWind.AlignChordOrRest(barMsPositions, i - 1, i, i + 1, clytemnestra[clytemnestraIndices[i - 21]].MsPosition);
        //    }
        //    // verse 3
        //    clytemnestraIndices = new List<int>() { 126, 137, 144, 152, 164, 173 };
        //    for(int i = 34; i < 40; ++i)
        //    {
        //        bassWind.AlignChordOrRest(barMsPositions, i - 1, i, i + 1, clytemnestra[clytemnestraIndices[i - 34]].MsPosition);
        //    }
        //    // verse 4
        //    clytemnestraIndices = new List<int>() { 192, 206, 215, 221, 234, 246, 257 };
        //    for(int i = 50; i < 57; ++i)
        //    {
        //        bassWind.AlignChordOrRest(barMsPositions, i - 1, i, i + 1, clytemnestra[clytemnestraIndices[i - 50]].MsPosition);
        //    }
        //    // verse 5
        //    clytemnestraIndices = new List<int>() { 279,283,288 };
        //    for(int i = 71; i < 74; ++i)
        //    {
        //        bassWind.AlignChordOrRest(barMsPositions, i - 1, i, i + 1, clytemnestra[clytemnestraIndices[i - 71]].MsPosition);
        //    }
        //    bassWind.AlignChordOrRest(barMsPositions, 73, 74, 77, clytemnestra[289].MsPosition);
        //    #endregion
        //}

        #region attributes
        internal List<int> BaseWindKrystalStrandIndices = new List<int>();

        // each MidiDefSequence has the duration of the whole piece
        // The sequences are in bottom to top order. MidiDefSequences[0] is the base Wind
        internal List<MidiDefSequence> MidiDefSequences = new List<MidiDefSequence>();
        #endregion
    }
}
