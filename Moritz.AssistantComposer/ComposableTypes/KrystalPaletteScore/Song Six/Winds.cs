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
        /// Constructs the initial state of the lowest two Winds (4 and 5), putting them in the public WindDefSequences
        /// Wind channels are ordered bottom to top. i.e. MidiDefSequences[0] is always the Wind 5 (bass) sequence.
        /// Also sets the public BaseWindKrystalStrandIndices attribute.
        /// </summary>
        public Winds(List<Krystal> krystals, List<PaletteDef> paletteDefs)
        {
            MidiDefSequence windSequence5 = GetBaseMidiDefSequence(krystals[2], paletteDefs[0]);
            windSequence5.Transpose(-13);
            MidiDefSequences.Add(windSequence5);

            MidiDefSequence windSequence4 = ReversedMidiDefSequence(windSequence5);
            SetLyricsToIndex(windSequence4);
            windSequence4.Transpose(7);
            MidiDefSequences.Add(windSequence4);

            // The other winds are created later, when the barline positions are known

            BaseWindKrystalStrandIndices = GetBaseWindStrandIndices(krystals[2]);
        }

        /// <summary>
        /// The krystal is xk3(7.7.1)-9.krys:
        ///     expander: e(7.7.1).kexp
        ///     density and points inputs: xk2(7.7.1)-9.krys
        /// Ancestor krystal (xk2(7.7.1)-9.krys):
        ///     expander: e(7.7.1).kexp
        ///     density and points inputs: lk1(6)-1.krys containing the line {1, 2, 3, 4, 5, 6}
        /// </summary>
        private MidiDefSequence GetBaseMidiDefSequence(Krystal krystal, PaletteDef paletteDef)
        {
            List<List<int>> kValues = krystal.GetValues((uint)1);
            List<int> sequence = kValues[0]; // the flat list of values

            MidiDefSequence wind5Sequence = new MidiDefSequence(paletteDef, sequence);

            SetWind5LyricsToIndex(wind5Sequence);

            return wind5Sequence;
        }

        /// <summary>
        /// returns a new MidiDefSequence containing clones of the LocalizedMidiDurationDefs in the argument in reverse order.
        /// </summary>
        /// <param name="originalMidiDefSequence"></param>
        /// <returns></returns>
        private MidiDefSequence ReversedMidiDefSequence(MidiDefSequence originalMidiDefSequence)
        {
            MidiDefSequence newReversedMidiDefSequence = originalMidiDefSequence.Clone();

            newReversedMidiDefSequence.LocalizedMidiDurationDefs.Reverse();
            newReversedMidiDefSequence.MsPosition = 0;

            return newReversedMidiDefSequence;
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
        /// and adds an additional markers around each lyric which begin a strand in the krystal.
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

        // each voice has the duration of the whole piece (voices 1, 2 and 3 begin with a rest)
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
        /// wind3 will start at bar 21 (after verse 1)
        /// wind2 will start at bar 40 (after verse 2)
        /// wind1 will start at bar 58 (after verse 3)
        /// </summary>
        /// <param name="barlineMsPositions"></param>
        internal void ConstructUpperWinds(List<int> barlineMsPositions)
        {
            int startMsPosition;
            int finalBarlineMsPosition = barlineMsPositions[barlineMsPositions.Count-1];
            // wind 3
            startMsPosition = barlineMsPositions[20];
            MidiDefSequence windSequence3 = GetUpperWindSequence(MidiDefSequences[0], startMsPosition, finalBarlineMsPosition);
            windSequence3.Transpose(12);
            SetLyricsToIndex(windSequence3);
            MidiDefSequences.Add(windSequence3);
            // wind 2
            startMsPosition = barlineMsPositions[39];
            MidiDefSequence windSequence2 = GetUpperWindSequence(MidiDefSequences[0], startMsPosition, finalBarlineMsPosition);
            windSequence2.Transpose(19);
            SetLyricsToIndex(windSequence2);
            MidiDefSequences.Add(windSequence2);
            // wind 1
            startMsPosition = barlineMsPositions[57];
            MidiDefSequence windSequence1 = GetUpperWindSequence(MidiDefSequences[0], startMsPosition, finalBarlineMsPosition);
            windSequence1.Transpose(24);
            SetLyricsToIndex(windSequence1);
            MidiDefSequences.Add(windSequence1);
        }

        /// <summary>
        /// Creates a new Wind, which begins with a rest of duration startMsPosition,
        /// followed by a clone of the beginning of the original wind.
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
            newWindSequence.MsDuration = msDuration; // stretches the sequence
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
            wind4.AlignChordOrRest(barMsPositions, 0, 19, 82, barMsPositions[20]);
        }

        #region attributes
        internal List<int> BaseWindKrystalStrandIndices = new List<int>();

        // each MidiDefSequence has the duration of the whole piece
        // The sequences are in bottom to top order. MidiDefSequences[0] is the lowest Wind (Wind 5)
        internal List<MidiDefSequence> MidiDefSequences = new List<MidiDefSequence>();
        #endregion
    }
}
