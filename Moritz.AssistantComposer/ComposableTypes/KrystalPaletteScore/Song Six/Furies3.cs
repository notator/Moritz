using System.Collections.Generic;
using System.Diagnostics;
using System;

using Moritz.Score;
using Moritz.Score.Midi;
using Krystals4ObjectLibrary;

namespace Moritz.AssistantComposer
{
    /// <summary>
    /// The Furies "constructors" (part of the Song Six algorithm).
    /// </summary>
    internal partial class SongSixAlgorithm : MidiCompositionAlgorithm
    {
        private VoiceDef GetFuries3(int firstRestMsDuration, Clytemnestra clytemnestra, VoiceDef wind1, List<PaletteDef> paletteDefs)
        {
            VoiceDef furies3 = GetFuries3FluttersAndTicks(firstRestMsDuration, clytemnestra, wind1, paletteDefs);           
            return furies3;
        }

        private VoiceDef GetFuries3FluttersAndTicks(int firstRestMsDuration, Clytemnestra clytemnestra, VoiceDef wind1, List<PaletteDef> paletteDefs)
        {
            VoiceDef furies3 = GetFlutters(firstRestMsDuration, paletteDefs[2]);

            AddTicks(furies3, paletteDefs[4]);

            furies3.EndMsPosition = clytemnestra.EndMsPosition;

            #region alignments
            furies3.AlignObjectAtIndex(1, 25, 37, clytemnestra[61].MsPosition);
            furies3.AlignObjectAtIndex(25, 37, 49, clytemnestra[82].MsPosition);
            furies3.AlignObjectAtIndex(37, 49, 61, clytemnestra[98].MsPosition);
            furies3.AlignObjectAtIndex(49, 61, 106, wind1[25].MsPosition);
            furies3.AlignObjectAtIndex(61, 106, 134, wind1[28].MsPosition);
            furies3.AlignObjectAtIndex(106, 134, 136, wind1[30].MsPosition);
            #endregion

            return furies3;
        }

        private VoiceDef GetFlutters(int firstRestMsDuration, PaletteDef palette)
        {
            // each flutter begins with a chord, and ends with a rest.
            VoiceDef furies3FlutterSequence1 = GetFuries3Flutter1(palette);
            furies3FlutterSequence1.AdjustVelocities(0.7);

            VoiceDef furies3FlutterSequence2 = GetNextFlutterSequence(furies3FlutterSequence1, 0.89, 1);
            VoiceDef furies3FlutterSequence3 = GetNextFlutterSequence(furies3FlutterSequence2, 0.89, 1);
            VoiceDef furies3FlutterSequence4 = GetNextFlutterSequence(furies3FlutterSequence3, 0.89, 1);
            VoiceDef furies3FlutterSequence5 = GetNextFlutterSequence(furies3FlutterSequence4, 0.89, 1);
            VoiceDef furies3FlutterSequence6 = GetNextFlutterSequence(furies3FlutterSequence5, 0.89, 2);
            VoiceDef furies3FlutterSequence7 = GetNextFlutterSequence(furies3FlutterSequence6, 0.89, 2);
            VoiceDef furies3FlutterSequence8 = GetNextFlutterSequence(furies3FlutterSequence7, 0.89, 2);
            VoiceDef furies3FlutterSequence9 = GetNextFlutterSequence(furies3FlutterSequence8, 0.89, 3);
            VoiceDef furies3FlutterSequence10 = GetNextFlutterSequence(furies3FlutterSequence9, 0.89, 3);
            VoiceDef furies3FlutterSequence11 = GetNextFlutterSequence(furies3FlutterSequence10, 0.89, 4);
            VoiceDef furies3FlutterSequence12 = GetNextFlutterSequence(furies3FlutterSequence11, 0.89, 5); 

            VoiceDef furies3 = GetEmptyVoiceDef(firstRestMsDuration);

            furies3.AddRange(furies3FlutterSequence1);
            furies3.AddRange(furies3FlutterSequence2);
            furies3.AddRange(furies3FlutterSequence3);
            furies3.AddRange(furies3FlutterSequence4);
            furies3.AddRange(furies3FlutterSequence5);
            furies3.AddRange(furies3FlutterSequence6);
            furies3.AddRange(furies3FlutterSequence7);
            furies3.AddRange(furies3FlutterSequence8);
            furies3.AddRange(furies3FlutterSequence9);
            furies3.AddRange(furies3FlutterSequence10);
            furies3.AddRange(furies3FlutterSequence11);
            furies3.AddRange(furies3FlutterSequence12);

            return furies3;
        }

        private VoiceDef GetNextFlutterSequence(VoiceDef existingFlutter, double factor, int transposition)
        {
            VoiceDef nextFlutter = existingFlutter.Clone();
            nextFlutter.AdjustVelocities(factor);
            nextFlutter.AdjustMsDurations(factor);
            nextFlutter.AdjustRestMsDurations(factor);
            nextFlutter.Transpose(transposition);
            return nextFlutter;
        }

        private VoiceDef GetFuries3Flutter1(PaletteDef palette)
        {
            List<LocalMidiDurationDef> flutter1 = new List<LocalMidiDurationDef>();
            int msPosition = 0;

            for(int i = 0; i < 7; ++i)
            {
                int[] contour = K.Contour(7, 11, 7);
                LocalMidiDurationDef flutter = new LocalMidiDurationDef(palette[contour[i] - 1]);
                flutter.MsPosition = msPosition;
                msPosition += flutter.MsDuration;
                flutter1.Add(flutter);

                if(i != 3 && i != 5)
                {
                    LocalMidiDurationDef rest = new LocalMidiDurationDef(msPosition, flutter.MsDuration);
                    msPosition += rest.MsDuration;
                    flutter1.Add(rest);
                }
            }

            VoiceDef furies3FlutterSequence1 = new VoiceDef(flutter1);

            return furies3FlutterSequence1;
        }

        /// <summary>
        /// These ticks are "stolen" by Furies2 later.
        /// </summary>
        /// <param name="furies3"></param>
        /// <param name="ticksPalette"></param>
        private void AddTicks(VoiceDef furies3, PaletteDef ticksPalette)
        {
            List<int> TickInsertionIndices = new List<int>()
            {
                66,69,72,78,81,84,87,
                89,92,95,99,102,105,109,
                112,115,119,122,125,129,132
            };

            List<LocalMidiDurationDef> ticksList = GetFuries3TicksSequence(ticksPalette);

            Debug.Assert(TickInsertionIndices.Count == ticksList.Count); // 21 objects

            for(int i = ticksList.Count-1; i >= 0; --i)
            {
                furies3.Insert(TickInsertionIndices[i], ticksList[i]);
            }
        }

        private List<LocalMidiDurationDef> GetFuries3TicksSequence(PaletteDef ticksPalette)
        {
            List<LocalMidiDurationDef> ticksSequence = new List<LocalMidiDurationDef>();
            int msPosition = 0;
            int[] transpositions = { 12, 14, 17 };

            for(int i = 0; i < 3; ++i)
            {
                int[] contour = K.Contour(7, 4, 10 - i);
                for(int j = 6; j >= 0; --j)
                {
                    LocalMidiDurationDef ticks = new LocalMidiDurationDef(ticksPalette[contour[j]-1]);
                    ticks.Transpose(transpositions[i] + contour[j]);
                    ticks.UniqueMidiDurationDef.AdjustVelocities(0.6);
                    ticks.MsPosition = msPosition;
                    msPosition += ticks.MsDuration;
                    ticksSequence.Add(ticks);
                }
            }

            return ticksSequence;
        }

        private void GetFuries3ChirpsInInterlude2AndVerse3(VoiceDef furies1, VoiceDef furies2, VoiceDef furies3, Clytemnestra clytemnestra, VoiceDef wind1, PaletteDef chirpsPalette)
        {
            int[] chirpIndices = { 4, 8, 2, 6, 10, 0 };
            int[] transpositions = { 2, 1, 3, 0, 4, 5 };
            //double[] velocityfactors = { 0.3, 0.31, 0.32, 0.34, 0.35, 0.36, 0.37, 0.39, 0.4, 0.42, 0.43, 0.45 };
            double[] velocityfactors = { 0.32, 0.34, 0.36, 0.38, 0.40, 0.42 };
            int[] msPositions =
            { 
                furies3[112].MsPosition + 200, 
                furies3[129].MsPosition + 500, 
                clytemnestra[118].MsPosition,
                clytemnestra[138].MsPosition + 250,
                clytemnestra[151].MsPosition,
                furies2[57].MsPosition
            };
            for(int i = 5; i >=0; --i)
            {
                LocalMidiDurationDef cheep = new LocalMidiDurationDef(chirpsPalette[chirpIndices[i]]);
                cheep.MsPosition = msPositions[i];
                cheep.UniqueMidiDurationDef.AdjustVelocities(velocityfactors[i]);
                cheep.UniqueMidiDurationDef.Transpose(transpositions[i]);
                furies3.InsertInRest(cheep);
            }

            furies3.AlignObjectAtIndex(25, 30, 31, clytemnestra[65].MsPosition);
            furies3.AlignObjectAtIndex(140, 141, 142, clytemnestra[119].MsPosition);
            furies3.AlignObjectAtIndex(142, 143, 144, clytemnestra[140].MsPosition);
            furies3.AlignObjectAtIndex(144, 145, 146, clytemnestra[152].MsPosition);
            furies3.AlignObjectAtIndex(146, 147, 148, furies1[23].MsPosition);

            furies3.AgglomerateRestOrChordAt(114);
 
        }

    }
}
