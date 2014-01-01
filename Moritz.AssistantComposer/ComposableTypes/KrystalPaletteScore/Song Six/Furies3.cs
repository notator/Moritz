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

        #region before Interlude3
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
            List<IUniqueMidiDurationDef> flutter1 = new List<IUniqueMidiDurationDef>();
            int msPosition = 0;

            for(int i = 0; i < 7; ++i)
            {
                int[] contour = K.Contour(7, 11, 7);
                IUniqueMidiDurationDef flutter = palette[contour[i] - 1].CreateUniqueMidiDurationDef();
                flutter.MsPosition = msPosition;
                msPosition += flutter.MsDuration;
                flutter1.Add(flutter);

                if(i != 3 && i != 5)
                {
                    IUniqueMidiDurationDef rest = new UniqueMidiRestDef(msPosition, flutter.MsDuration);
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

            List<IUniqueMidiDurationDef> ticksList = GetFuries3TicksSequence(ticksPalette);

            Debug.Assert(TickInsertionIndices.Count == ticksList.Count); // 21 objects

            for(int i = ticksList.Count-1; i >= 0; --i)
            {
                furies3.Insert(TickInsertionIndices[i], ticksList[i]);
            }
        }

        private List<IUniqueMidiDurationDef> GetFuries3TicksSequence(PaletteDef ticksPalette)
        {
            List<IUniqueMidiDurationDef> ticksSequence = new List<IUniqueMidiDurationDef>();
            int msPosition = 0;
            int[] transpositions = { 12, 14, 17 };

            for(int i = 0; i < 3; ++i)
            {
                int[] contour = K.Contour(7, 4, 10 - i);
                for(int j = 6; j >= 0; --j)
                {
                    IUniqueMidiDurationDef ticks = ticksPalette[contour[j]-1].CreateUniqueMidiDurationDef();
                    ticks.Transpose(transpositions[i] + contour[j]);
                    ticks.AdjustVelocities(0.6);
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
                IUniqueMidiDurationDef cheep = chirpsPalette[chirpIndices[i]].CreateUniqueMidiDurationDef();
                cheep.MsPosition = msPositions[i];
                cheep.AdjustVelocities(velocityfactors[i]);
                cheep.Transpose(transpositions[i]);
                furies3.InsertInRest(cheep);
            }

            furies3.AlignObjectAtIndex(25, 30, 31, clytemnestra[65].MsPosition);
            furies3.AlignObjectAtIndex(140, 141, 142, clytemnestra[119].MsPosition);
            furies3.AlignObjectAtIndex(142, 143, 144, clytemnestra[140].MsPosition);
            furies3.AlignObjectAtIndex(144, 145, 146, clytemnestra[152].MsPosition);
            furies3.AlignObjectAtIndex(146, 147, 148, furies1[23].MsPosition);

            furies3.AgglomerateRestOrChordAt(114);

        }
        #endregion before Interlude3

        #region finale
        private VoiceDef GetF3Finale(List<PaletteDef> palettes, Dictionary<string, int> msPositions)
        {
            PaletteDef f3Interlude3Palette = palettes[10]; // correct 1.1.2014
            PaletteDef f3Interlude4Palette = palettes[14];
            PaletteDef f3PostludePalette = palettes[18];

            //PermutationKrystal krystal = new PermutationKrystal("C://Moritz/krystals/krystals/pk4(12)-2.krys");
            ExpansionKrystal krystal = new ExpansionKrystal("C://Moritz/krystals/krystals/xk3(12.12.1)-1.krys");
            List<int> strandIndices = new List<int>();
            int index = 0;
            for(int i = 0; i < krystal.Strands.Count; ++i)
            {
                strandIndices.Add(index);
                index += krystal.Strands[i].Values.Count;
            }

            VoiceDef f3Interlude3Verse4e = GetF3Interlude3Verse4EsCaped(f3Interlude3Palette, krystal, strandIndices, msPositions);
            VoiceDef f3Verse4eVerse5 = GetF3Verse4EscapedVerse5Calls(f3Interlude4Palette, krystal, strandIndices, msPositions);
            VoiceDef f3Postlude = GetF3Postlude(f3PostludePalette, krystal, strandIndices, msPositions);

            VoiceDef furies3Finale = f3Interlude3Verse4e;

            furies3Finale.AddRange(f3Verse4eVerse5);
            furies3Finale.AddRange(f3Postlude);

            if(furies3Finale[furies3Finale.Count - 1] is UniqueMidiRestDef)
            {
                furies3Finale.RemoveAt(furies3Finale.Count - 1);
            }

            if(furies3Finale[furies3Finale.Count - 1].MsPosition + furies3Finale[furies3Finale.Count - 1].MsDuration > msPositions["endOfPiece"])
            {
                furies3Finale.RemoveAt(furies3Finale.Count - 1);
            }

            AdjustFuriesFinalePitchWheelDeviations(furies3Finale);

            return furies3Finale;
        }

        private VoiceDef GetF3Interlude3Verse4EsCaped(PaletteDef f3Int3Palette, ExpansionKrystal krystal, List<int> strandIndices, Dictionary<string, int> msPositions)
        {
            VoiceDef f33 = new VoiceDef(f3Int3Palette, krystal);

            List<int> f3eStrandDurations = GetStrandDurations(f33, strandIndices);

            int extraTime = 1000;
            int diff = extraTime / f33.Count;
            for(int i = f33.Count - 1; i > 0; --i)
            {
                if(strandIndices.Contains(i))
                {
                    UniqueMidiRestDef umrd = new UniqueMidiRestDef(f33[i].MsPosition, f3eStrandDurations[strandIndices.IndexOf(i)] + extraTime);
                    extraTime -= diff;
                    f33.Insert(i, umrd);
                }
            }

            f33.StartMsPosition = msPositions["interlude3Bar2"];

            f33.RemoveBetweenMsPositions(msPositions["verse4EsCaped"], int.MaxValue);

            if(f33[f33.Count - 1] is UniqueMidiRestDef)
            {
                f33[f33.Count - 1].MsDuration = msPositions["verse4EsCaped"] - f33[f33.Count - 1].MsPosition;
            }

            return f33;
        }

        private VoiceDef GetF3Verse4EscapedVerse5Calls(PaletteDef f3Int4Palette, ExpansionKrystal krystal, List<int> strandIndices, Dictionary<string, int> msPositions)
        {
            VoiceDef f34 = new VoiceDef(f3Int4Palette, krystal);

            List<int> f3eStrandDurations = GetStrandDurations(f34, strandIndices);

            int extraTime = 500;
            int diff = extraTime / f34.Count;
            for(int i = f34.Count - 1; i > 0; --i)
            {
                if(strandIndices.Contains(i))
                {
                    UniqueMidiRestDef umrd = new UniqueMidiRestDef(f34[i].MsPosition, f3eStrandDurations[strandIndices.IndexOf(i)] + extraTime);
                    extraTime -= diff;
                    f34.Insert(i, umrd);
                }
            }

            f34.StartMsPosition = msPositions["verse4EsCaped"];
            f34.RemoveBetweenMsPositions(msPositions["verse5Calls"], int.MaxValue);

            if(f34[f34.Count - 1] is UniqueMidiRestDef)
            {
                f34[f34.Count - 1].MsDuration = msPositions["postlude"] - f34[f34.Count - 1].MsPosition;
            }

            return f34;
        }

        private VoiceDef GetF3Postlude(PaletteDef f3PostludePalette, ExpansionKrystal krystal, List<int> strandIndices, Dictionary<string, int> msPositions)
        {
            VoiceDef f3p = new VoiceDef(f3PostludePalette, krystal);

            List<int> f3eStrandDurations = GetStrandDurations(f3p, strandIndices);

            for(int i = f3p.Count - 1; i > 0; --i)
            {
                if(strandIndices.Contains(i))
                {
                    UniqueMidiRestDef umrd = new UniqueMidiRestDef(f3p[i].MsPosition, f3eStrandDurations[strandIndices.IndexOf(i)] / 4);
                    f3p.Insert(i, umrd);
                }
            }

            f3p.StartMsPosition = msPositions["postlude"];
            f3p.RemoveBetweenMsPositions(msPositions["endOfPiece"], int.MaxValue);

            return f3p;
        }

        private void AdjustF3Alignments(VoiceDef furies3, Clytemnestra clytemnestra, VoiceDef wind3)
        {
            Debug.Assert(furies3[213] is UniqueMidiRestDef);
            furies3[213].MsDuration += furies3[212].MsDuration;
            furies3.RemoveAt(212);
            furies3.AgglomerateRests();

            furies3.AlignObjectAtIndex(25, 84, 85, clytemnestra[196].MsPosition);
            furies3.AlignObjectAtIndex(84, 85, 89, clytemnestra[204].MsPosition + 200);
            furies3.AlignObjectAtIndex(85, 89, 96, clytemnestra[215].MsPosition);
            furies3.AlignObjectAtIndex(89, 96, 102, clytemnestra[226].MsPosition);
            furies3.AlignObjectAtIndex(102, 106, 117, clytemnestra[242].MsPosition);
            furies3.AlignObjectAtIndex(106, 117, 140, clytemnestra[268].MsPosition);
            furies3.AlignObjectAtIndex(117, 140, 163, wind3[61].MsPosition);
            furies3.AlignObjectAtIndex(140, 163, 197, wind3[65].MsPosition);
            furies3.AlignObjectAtIndex(163, 197, 206, clytemnestra[269].MsPosition - 200);
            furies3.AlignObjectAtIndex(197, 206, 211, clytemnestra[283].MsPosition + 400);
            furies3.AlignObjectAtIndex(206, 211, 212, clytemnestra[286].MsPosition);
            furies3.AlignObjectAtIndex(211, 212, furies3.Count - 1, clytemnestra[289].MsPosition);
        }

        private void AdjustF3Velocities(VoiceDef furies3, Dictionary<string, int> msPositions)
        {
            int indexAtVerse4 = furies3.FindIndexAtMsPosition(msPositions["verse4"]);
            int indexAtInterval4 = furies3.FindIndexAtMsPosition(msPositions["interlude4"]);
            int indexAtVerse5 = furies3.FindIndexAtMsPosition(msPositions["verse5"]);
            int indexAtPostlude = furies3.FindIndexAtMsPosition(msPositions["postlude"]);

            furies3.AdjustVelocities(indexAtVerse4, indexAtInterval4, 0.5);
            furies3.AdjustVelocities(96, 106, 0.7);

            furies3.AdjustVelocitiesHairpin(msPositions["interlude4"], msPositions["verse5"], 0.8, 1.0);

            furies3.AdjustVelocities(indexAtVerse5, indexAtPostlude, 0.5);

            //furies3.AdjustVelocitiesHairpin(msPositions["postlude"], msPositions["finalWindChord"], 0.8, 1.0);
            //furies3.AdjustVelocitiesHairpin(msPositions["finalWindChord"], furies3.EndMsPosition, 1.0, 0);

            furies3.AdjustVelocitiesHairpin(msPositions["postlude"], furies3.EndMsPosition, 0.8, 1.0);
        }

        private void AdjustF3PostludePan(VoiceDef furies3, int postludeMsPosition)
        {
            double posDiff = ((double)(furies3.EndMsPosition - postludeMsPosition)) / 4;
            int postludeMsPosition1 = postludeMsPosition + (int)posDiff;
            int postludeMsPosition2 = postludeMsPosition + (int)(posDiff * 2);
            int postludeMsPosition3 = postludeMsPosition + (int)(posDiff * 3);

            furies3.SetPan(postludeMsPosition, postludeMsPosition1, 64, 32);
            furies3.SetPan(postludeMsPosition1, postludeMsPosition2, 32, 96);
            furies3.SetPan(postludeMsPosition2, postludeMsPosition3, 96, 0);
            furies3.SetPan(postludeMsPosition3, furies3.EndMsPosition, 0, 127);
        }
        #endregion finale
    }
}
