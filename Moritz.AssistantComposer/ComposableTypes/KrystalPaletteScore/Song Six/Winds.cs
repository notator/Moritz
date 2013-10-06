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
        /// Constructs the initial state of the lowest Wind (=Wind5), putting it in the public MidiPhrases list.
        /// Wind channels are ordered bottom to top. i.e. MidiPhrases[0] is always Wind 5 (the lowest).
        /// Also sets the public BaseWindKrystalStrandIndices attribute.
        /// </summary>
        public Winds(List<Krystal> krystals, List<PaletteDef> paletteDefs)
        {
            //BaseWindKrystalStrandIndices = GetBaseWindStrandIndices(krystals[2]);
            
            MidiPhrase wind5 = GetWind5(krystals[2], paletteDefs[0]);
            wind5.Transpose(-13);
            MidiPhrases.Add(wind5);

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
        private MidiPhrase GetWind5(Krystal krystal, PaletteDef paletteDef)
        {
            List<List<int>> kValues = krystal.GetValues((uint)1);
            List<int> values = kValues[0]; // the flat list of values

            MidiPhrase wind5 = new MidiPhrase(paletteDef, values);

            return wind5;
        }

        //private List<int> GetBaseWindStrandIndices(Krystal krystal)
        //{
        //    List<int> strandIndices = new List<int>();
        //    List<Strand> strands = krystal.Strands;
        //    int index = 0;
        //    foreach(Strand strand in strands)
        //    {
        //        strandIndices.Add(index);
        //        index += strand.Values.Count;
        //    }
        //    return strandIndices;
        //}

        /// <summary>
        /// Each voice has the duration of the whole piece. Some voices begin with rest(s).
        /// </summary>
        internal List<Voice> GetVoices(int topWindChannelIndex)
        {
            List<Voice> voices = new List<Voice>();
            int windChannelIndex = topWindChannelIndex;
            for(int i = MidiPhrases.Count - 1; i >= 0; --i)
            {
                Voice voice = new Voice(null, (byte)(windChannelIndex++));
                voice.LocalMidiDurationDefs = MidiPhrases[i].LocalMidiDurationDefs;
                voices.Add(voice);
            }
            return voices;
        }

        internal List<int> AddInterludeBarlinePositions(List<int> barlineMsPositions)
        {
            List<int> newBarlineIndices = new List<int>() { 1, 3, 5, 15, 27, 40, 45, 63, 77 }; // by inspection of the score
            MidiPhrase bassWind = MidiPhrases[0];
            foreach(int index in newBarlineIndices)
            {
                barlineMsPositions.Add(bassWind.LocalMidiDurationDefs[index].MsPosition);
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
        /// <param name="barMsPositions"></param>
        internal void CompleteTheWinds(List<int> barMsPositions)
        {
            int startMsPosition;
            int finalBarlineMsPosition = barMsPositions[barMsPositions.Count-1];
            MidiPhrase wind5 = MidiPhrases[0];

            MidiPhrase wind4 = GetWind4(wind5, barMsPositions);
            MidiPhrases.Add(wind4);

            startMsPosition = barMsPositions[20];
            MidiPhrase wind3 = GetWind3(wind5, startMsPosition, barMsPositions);
            MidiPhrases.Add(wind3);

            startMsPosition = barMsPositions[39];
            MidiPhrase wind2 = GetWind2(wind5, startMsPosition, barMsPositions);
            MidiPhrases.Add(wind2);

            startMsPosition = barMsPositions[57];
            MidiPhrase wind1 = GetWind1(wind5, startMsPosition, barMsPositions);
            MidiPhrases.Add(wind1);

            CompleteWind5(wind5, barMsPositions);
        }

        private MidiPhrase GetWind1(MidiPhrase wind5, int startMsPosition, List<int> barMsPositions)
        {
            int finalBarlineMsPosition = barMsPositions[barMsPositions.Count - 1];
            MidiPhrase wind1 = GetBasicUpperWind(wind5, startMsPosition, finalBarlineMsPosition);
            wind1.Transpose(24);

            int fromBarNumber = 83;
            int glissInterval = 48;
            GlissToTheEnd(wind1, barMsPositions[fromBarNumber - 1], glissInterval);

            return wind1;
        }

        private MidiPhrase GetWind2(MidiPhrase wind5, int startMsPosition, List<int> barMsPositions)
        {
            int finalBarlineMsPosition = barMsPositions[barMsPositions.Count - 1];
            MidiPhrase wind2 = GetBasicUpperWind(wind5, startMsPosition, finalBarlineMsPosition);
            wind2.Transpose(19);
            wind2.AlignObjectAtIndex(1, 49, 57, barMsPositions[91]);

            int fromBarNumber = 83;
            int glissInterval = 36;
            GlissToTheEnd(wind2, barMsPositions[fromBarNumber - 1], glissInterval);

            return wind2;
        }

        private MidiPhrase GetWind3(MidiPhrase wind5, int startMsPosition, List<int> barMsPositions)
        {
            int finalBarlineMsPosition = barMsPositions[barMsPositions.Count - 1];
            MidiPhrase wind3 = GetBasicUpperWind(wind5, startMsPosition, finalBarlineMsPosition);
            wind3.Transpose(12);
            wind3.AlignObjectAtIndex(1, 10, 67, barMsPositions[39]);

            int fromBarNumber = 83;
            //int glissInterval = 26;
            int glissInterval = 24;
            GlissToTheEnd(wind3, barMsPositions[fromBarNumber - 1], glissInterval); 

            return wind3;
        }

        private MidiPhrase GetWind4(MidiPhrase wind5, List<int> barMsPositions)
        {
            MidiPhrase wind4 = GetBasicWind4(wind5, barMsPositions);
            wind4.Transpose(7); // the basic pitch
            wind4.AlignObjectAtIndex(0, 10, 82, barMsPositions[6]);
            wind4.AlignObjectAtIndex(10, 16, 82, barMsPositions[20]);
            wind4.AlignObjectAtIndex(16, 57, 82, barMsPositions[82]);

            // now create a gliss from bar 61 to 83            
            int bar61MsPosition = barMsPositions[60];
            int startGlissIndex = wind4.FirstIndexAtOrAfterMsPos(bar61MsPosition);
            int bar83MsPosition = barMsPositions[82];
            int endGlissIndex = wind4.FirstIndexAtOrAfterMsPos(bar83MsPosition) - 1; // 57
            int glissInterval = 19;
            wind4.StepwiseGliss(startGlissIndex, endGlissIndex, glissInterval);

            // from bar 83 to the end is constant (19 semitones higher than before)

            for(int index = endGlissIndex + 1; index < wind4.Count; ++index)
            {
                wind4[index].Transpose(19);
            }
            return wind4;
        }

        private void CompleteWind5(MidiPhrase wind5, List<int> barMsPositions)
        {
            int fromBarNumber = 83;
            int glissInterval = 26;
            GlissToTheEnd(wind5, barMsPositions[fromBarNumber - 1], glissInterval);
        }

        private void GlissToTheEnd(MidiPhrase wind, int fromMsPosition, int glissInterval)
        {
            int startGlissIndex = wind.FirstIndexAtOrAfterMsPos(fromMsPosition);
            int endGlissIndex = wind.Count - 1;
            wind.StepwiseGliss(startGlissIndex, endGlissIndex, glissInterval);
        }

        /// <summary>
        /// Returns a MidiPhrase containing clones of the LocalMidiDurationDefs in the originalMidiPhrase
        /// argument, rotated so that the original first LocalMidiDurationDef is positioned at bar 83.
        /// The LocalMidiDurationDefs before bar 83 are stretched to fit. 
        /// The LocalMidiDurationDefs after bar 83 are compressed to fit. 
        /// </summary>
        /// <param name="originalMidiPhrase"></param>
        /// <returns></returns>
        private MidiPhrase GetBasicWind4(MidiPhrase originalMidiPhrase, List<int> barlineMsPositions)
        {
            MidiPhrase tempWind = originalMidiPhrase.Clone();
            int finalBarlineMsPosition = barlineMsPositions[barlineMsPositions.Count - 1];
            int msDurationAfterSynch = finalBarlineMsPosition - barlineMsPositions[82]; 

            List<LocalMidiDurationDef> originalLmdds = tempWind.LocalMidiDurationDefs;
            List<LocalMidiDurationDef> originalStartLmdds = new List<LocalMidiDurationDef>();
            List<LocalMidiDurationDef> wind4Lmdds = new List<LocalMidiDurationDef>();
            int accumulatingMsDuration = 0;
            for(int i = 0; i < tempWind.Count; ++i)
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
            foreach(LocalMidiDurationDef lmdd in wind4Lmdds)
            {
                lmdd.MsPosition = msPosition;
                msPosition += lmdd.MsDuration;
            }
            MidiPhrase wind4 = new MidiPhrase(wind4Lmdds);

            return wind4;
        }

        /// <summary>
        /// Creates a new Wind, which begins with a rest of duration=startMsPosition,
        /// followed by a clone of the beginning of the originalWind (=Wind 5).
        /// As much of the originalWind is used as possible. The cloned LocalMidiDurationDefs are stretched to fit exactly.
        /// </summary>
        private MidiPhrase GetBasicUpperWind(MidiPhrase originalWind, int startMsPosition, int finalBarlineMsPosition)
        {
            MidiPhrase newWind = originalWind.Clone();
            int msDuration = finalBarlineMsPosition - startMsPosition;
            int accumulatingDuration = 0;
            int maxIndex = 0;
            for(int i = 0; i < newWind.Count; ++i)
            {
                accumulatingDuration += newWind[i].MsDuration;
                if(accumulatingDuration > msDuration)
                {
                    break;
                }
                maxIndex = i;
            }
            for(int i = newWind.Count - 1; i > maxIndex; --i)
            {
                newWind.LocalMidiDurationDefs.RemoveAt(i);
            }
            newWind.MsDuration = msDuration; // stretches or compresses newWind
            LocalMidiDurationDef lmdd = new LocalMidiDurationDef(startMsPosition);
            newWind.LocalMidiDurationDefs.Insert(0, lmdd);
            newWind.MsPosition = 0;

            return newWind;
        }

        #region attributes
        internal List<int> BaseWindKrystalStrandIndices = new List<int>();

        // each MidiPhrase has the duration of the whole piece
        // The MidiPhrases are in bottom to top order. MidiPhrases[0] is the lowest Wind (Wind 5)
        internal List<MidiPhrase> MidiPhrases = new List<MidiPhrase>();
        #endregion
    }
}
