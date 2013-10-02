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
        /// <summary>
        /// Constructs the initial state of the lowest Wind (5), putting it in the public WindDefSequences list.
        /// Wind channels are ordered bottom to top. i.e. MidiDefSequences[0] is always the Wind 5 (lowest) sequence.
        /// Also sets the public BaseWindKrystalStrandIndices attribute.
        /// </summary>
        public Winds(List<Krystal> krystals, List<PaletteDef> paletteDefs)
        {
            BaseWindKrystalStrandIndices = GetBaseWindStrandIndices(krystals[2]);
            
            MidiDefSequence wind5Sequence = GetWind5Sequence(krystals[2], paletteDefs[0]);
            wind5Sequence.Transpose(-13);
            MidiDefSequences.Add(wind5Sequence);

            // The other winds are created later, when the barline positions are known
        }

        /// <summary>
        /// The krystal is xk3(7.7.1)-9.krys:
        ///     expander: e(7.7.1).kexp
        ///     density and points inputs: xk2(7.7.1)-9.krys
        /// Ancestor krystal (xk2(7.7.1)-9.krys):
        ///     expander: e(7.7.1).kexp
        ///     density and points inputs: lk1(6)-1.krys containing the line {1, 2, 3, 4, 5, 6}
        /// </summary>
        private MidiDefSequence GetWind5Sequence(Krystal krystal, PaletteDef paletteDef)
        {
            List<List<int>> kValues = krystal.GetValues((uint)1);
            List<int> sequence = kValues[0]; // the flat list of values

            MidiDefSequence wind5Sequence = new MidiDefSequence(paletteDef, sequence);

            SetWind5LyricsToIndex(wind5Sequence);

            return wind5Sequence;
        }

        private List<int> GetBaseWindStrandIndices(Krystal krystal)
        {
            List<int> strandIndices = new List<int>();
            List<Strand> strands = krystal.Strands;
            int index = 0;
            foreach(Strand strand in strands)
            {
                strandIndices.Add(index);
                index += strand.Values.Count;
            }
            return strandIndices;
        }

        /// <summary>
        /// This function sets the lyrics in wind 5 to the index of the LocalMidiDurationDef,
        /// and adds additional markers around each lyric which begin a strand in the krystal.
        /// </summary>
        /// <param name="wind5Sequence"></param>
        private void SetWind5LyricsToIndex(MidiDefSequence wind5Sequence)
        {
            for(int index = 0; index < wind5Sequence.Count; ++index)
            {
                LocalMidiChordDef lmcd = wind5Sequence[index].LocalMidiDurationDef as LocalMidiChordDef;
                if(lmcd != null)
                {
                    if(BaseWindKrystalStrandIndices.Contains(index))
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

        /// <summary>
        /// Rests dont have lyrics, so their index in the midiDefSequence can't be shown there. 
        /// </summary>
        /// <param name="midiDefSequence"></param>
        private void SetLyricsToIndex(MidiDefSequence midiDefSequence)
        {
            for(int index = 0; index < midiDefSequence.Count; ++index)
            {
                LocalMidiChordDef lmcd = midiDefSequence[index].LocalMidiDurationDef as LocalMidiChordDef;
                if(lmcd != null)
                {
                    lmcd.Lyric = index.ToString();
                }
            }
        }

        /// <summary>
        /// Each voice has the duration of the whole piece. Some voices begin with rest(s).
        /// </summary>
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

        /// <summary>
        /// wind4 starts at bar 1. It is a rotated clone of wind5. The previous beginning of the cycle is at bar 83. 
        /// wind3 starts with a rest. Chords start at bar 21 (after verse 1)
        /// wind2 starts with a rest. Chords start at bar 40 (after verse 2)
        /// wind1 starts with a rest. Chords start at bar 58 (after verse 3)
        /// </summary>
        /// <param name="barlineMsPositions"></param>
        internal void ConstructUpperWinds(List<int> barlineMsPositions)
        {
            int startMsPosition;
            int finalBarlineMsPosition = barlineMsPositions[barlineMsPositions.Count-1];
            MidiDefSequence wind5Sequence = MidiDefSequences[0];
            // wind 4
            MidiDefSequence wind4Sequence = GetWind4Sequence(wind5Sequence, barlineMsPositions);
            SetLyricsToIndex(wind4Sequence);
            wind4Sequence.Transpose(7);
            MidiDefSequences.Add(wind4Sequence);
            // wind 3
            startMsPosition = barlineMsPositions[20];
            MidiDefSequence wind3Sequence = GetUpperWindSequence(wind5Sequence, startMsPosition, finalBarlineMsPosition);
            wind3Sequence.Transpose(12);
            SetLyricsToIndex(wind3Sequence);
            MidiDefSequences.Add(wind3Sequence);
            // wind 2
            startMsPosition = barlineMsPositions[39];
            MidiDefSequence wind2Sequence = GetUpperWindSequence(wind5Sequence, startMsPosition, finalBarlineMsPosition);
            wind2Sequence.Transpose(19);
            SetLyricsToIndex(wind2Sequence);
            MidiDefSequences.Add(wind2Sequence);
            // wind 1
            startMsPosition = barlineMsPositions[57];
            MidiDefSequence wind1Sequence = GetUpperWindSequence(wind5Sequence, startMsPosition, finalBarlineMsPosition);
            wind1Sequence.Transpose(24);
            SetLyricsToIndex(wind1Sequence);
            MidiDefSequences.Add(wind1Sequence);
        }

        /// <summary>
        /// Returns a MidiDefSequence containing clones of the LocalizedMidiDurationDefs in the MidiDefSequence argument,
        /// rotated so that the original first LocalizedMidiDurationDef is positioned at bar 83.
        /// The LocalizedMidiDurationDefs before bar 83 are stretched to fit. 
        /// The LocalizedMidiDurationDefs after bar 83 are compressed to fit. 
        /// </summary>
        /// <param name="originalMidiDefSequence"></param>
        /// <returns></returns>
        private MidiDefSequence GetWind4Sequence(MidiDefSequence originalMidiDefSequence, List<int> barlineMsPositions)
        {
            MidiDefSequence tempSequence = originalMidiDefSequence.Clone();
            int finalBarlineMsPosition = barlineMsPositions[barlineMsPositions.Count - 1];
            int msDurationAfterSynch = finalBarlineMsPosition - barlineMsPositions[82]; 

            List<LocalizedMidiDurationDef> originalLmdds = tempSequence.LocalizedMidiDurationDefs;
            List<LocalizedMidiDurationDef> originalStartLmdds = new List<LocalizedMidiDurationDef>();
            List<LocalizedMidiDurationDef> wind4Lmdds = new List<LocalizedMidiDurationDef>();
            int accumulatingMsDuration = 0;
            for(int i = 0; i < tempSequence.Count; ++i)
            {
                if(accumulatingMsDuration <= msDurationAfterSynch)
                {
                    originalStartLmdds.Add(originalLmdds[i]);
                    accumulatingMsDuration += originalLmdds[i].MsDuration;
                }
                else
                {
                    wind4Lmdds.Add(originalLmdds[i]);
                }
            }
            wind4Lmdds.AddRange(originalStartLmdds);

            int msPosition = 0;
            foreach(LocalizedMidiDurationDef lmdd in wind4Lmdds)
            {
                lmdd.MsPosition = msPosition;
                msPosition += lmdd.MsDuration;
            }
            MidiDefSequence wind4Sequence = new MidiDefSequence(wind4Lmdds);

            return wind4Sequence;
        }

        /// <summary>
        /// Creates a new Wind, which begins with a rest of duration=startMsPosition,
        /// followed by a clone of the beginning of the originalWind (=Wind 5).
        /// As much of the originalWind is used as possible. The cloned LocalizedMidiDurationDefs are stretched to fit exactly.
        /// </summary>
        private MidiDefSequence GetUpperWindSequence(MidiDefSequence originalWind, int startMsPosition, int finalBarlineMsPosition)
        {
            MidiDefSequence newWindSequence = originalWind.Clone();
            int msDuration = finalBarlineMsPosition - startMsPosition;
            int accumulatingDuration = 0;
            int maxIndex = 0;
            for(int i = 0; i < newWindSequence.Count; ++i)
            {
                accumulatingDuration += newWindSequence[i].MsDuration;
                if(accumulatingDuration > msDuration)
                {
                    break;
                }
                maxIndex = i;
            }
            for(int i = newWindSequence.Count - 1; i > maxIndex; --i)
            {
                newWindSequence.LocalizedMidiDurationDefs.RemoveAt(i);
            }
            newWindSequence.MsDuration = msDuration; // stretches or compresses the sequence
            LocalizedMidiDurationDef lmdd = new LocalizedMidiDurationDef(startMsPosition);
            newWindSequence.LocalizedMidiDurationDefs.Insert(0, lmdd);
            newWindSequence.MsPosition = 0;

            return newWindSequence;
        }

        /// <summary>
        /// Moves various LocalizedMidiDurationDefs in the winds to align with barlines
        /// </summary>
        internal void AlignChords(List<int> barMsPositions)
        {
            //MidiDefSequence wind5 = MidiDefSequences[0];
            MidiDefSequence wind4 = MidiDefSequences[1];
            MidiDefSequence wind3 = MidiDefSequences[2];
            MidiDefSequence wind2 = MidiDefSequences[3];
            //MidiDefSequence wind1 = MidiDefSequences[4];

            wind2.AlignChordOrRest(barMsPositions, 1, 49, 57, barMsPositions[91]);
            wind3.AlignChordOrRest(barMsPositions, 1, 10, 67, barMsPositions[39]);
            wind4.AlignChordOrRest(barMsPositions, 0, 16, 82, barMsPositions[20]);
            wind4.AlignChordOrRest(barMsPositions, 16, 57, 82, barMsPositions[82]);
        }

        #region attributes
        internal List<int> BaseWindKrystalStrandIndices = new List<int>();

        // each MidiDefSequence has the duration of the whole piece
        // The sequences are in bottom to top order. MidiDefSequences[0] is the lowest Wind (Wind 5)
        internal List<MidiDefSequence> MidiDefSequences = new List<MidiDefSequence>();
        #endregion
    }
}
