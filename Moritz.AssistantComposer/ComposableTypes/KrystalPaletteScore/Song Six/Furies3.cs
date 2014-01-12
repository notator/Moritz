using System.Collections.Generic;
using System.Diagnostics;
using System;

using Moritz.Score;
using Moritz.Score.Midi;
using Krystals4ObjectLibrary;

namespace Moritz.AssistantComposer
{
    internal class Furies3 : VoiceDef
    {
        internal Furies3(int msDuration)
            : base(msDuration)
        {
        }

        #region before Interlude3
        internal void GetBeforeInterlude3(int firstRestMsDuration, Clytemnestra clytemnestra, VoiceDef wind1, List<PaletteDef> paletteDefs)
        {
            GetFlutters(firstRestMsDuration, paletteDefs[2]);

            AddTicks(paletteDefs[4]);

            EndMsPosition = clytemnestra.EndMsPosition;

            #region alignments
            AlignObjectAtIndex(1, 25, 37, clytemnestra[61].MsPosition);
            AlignObjectAtIndex(25, 37, 49, clytemnestra[82].MsPosition);
            AlignObjectAtIndex(37, 49, 61, clytemnestra[98].MsPosition);
            AlignObjectAtIndex(49, 61, 106, wind1[25].MsPosition);
            AlignObjectAtIndex(61, 106, 134, wind1[28].MsPosition);
            AlignObjectAtIndex(106, 134, 136, wind1[30].MsPosition);
            #endregion
        }

        private void GetFlutters(int firstRestMsDuration, PaletteDef palette)
        {
            // each flutter begins with a chord, and ends with a rest.
            VoiceDef furies3FlutterSequence1 = GetFlutter1(palette);
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

            Furies3 f3 = new Furies3(firstRestMsDuration);

            f3.AddRange(furies3FlutterSequence1);
            f3.AddRange(furies3FlutterSequence2);
            f3.AddRange(furies3FlutterSequence3);
            f3.AddRange(furies3FlutterSequence4);
            f3.AddRange(furies3FlutterSequence5);
            f3.AddRange(furies3FlutterSequence6);
            f3.AddRange(furies3FlutterSequence7);
            f3.AddRange(furies3FlutterSequence8);
            f3.AddRange(furies3FlutterSequence9);
            f3.AddRange(furies3FlutterSequence10);
            f3.AddRange(furies3FlutterSequence11);
            f3.AddRange(furies3FlutterSequence12);

            this._uniqueMidiDurationDefs = f3.UniqueMidiDurationDefs;
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

        private VoiceDef GetFlutter1(PaletteDef palette)
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
        private void AddTicks(PaletteDef ticksPalette)
        {
            List<int> TickInsertionIndices = new List<int>()
            {
                66,69,72,78,81,84,87,
                89,92,95,99,102,105,109,
                112,115,119,122,125,129,132
            };

            List<IUniqueMidiDurationDef> ticksList = GetTicksSequence(ticksPalette);

            Debug.Assert(TickInsertionIndices.Count == ticksList.Count); // 21 objects

            for(int i = ticksList.Count-1; i >= 0; --i)
            {
                Insert(TickInsertionIndices[i], ticksList[i]);
            }
        }

        private List<IUniqueMidiDurationDef> GetTicksSequence(PaletteDef ticksPalette)
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

        internal void GetChirpsInInterlude2AndVerse3(VoiceDef furies1, VoiceDef furies2, Clytemnestra clytemnestra, VoiceDef wind1, PaletteDef chirpsPalette)
        {
            int[] chirpIndices = { 4, 8, 2, 6, 10, 0 };
            int[] transpositions = { 2, 1, 3, 0, 4, 5 };
            //double[] velocityfactors = { 0.3, 0.31, 0.32, 0.34, 0.35, 0.36, 0.37, 0.39, 0.4, 0.42, 0.43, 0.45 };
            double[] velocityfactors = { 0.32, 0.34, 0.36, 0.38, 0.40, 0.42 };
            int[] msPositions =
            { 
                this[112].MsPosition + 200, 
                this[129].MsPosition + 500, 
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
                InsertInRest(cheep);
            }

            AlignObjectAtIndex(25, 30, 31, clytemnestra[65].MsPosition);
            AlignObjectAtIndex(140, 141, 142, clytemnestra[119].MsPosition);
            AlignObjectAtIndex(142, 143, 144, clytemnestra[140].MsPosition);
            AlignObjectAtIndex(144, 145, 146, clytemnestra[152].MsPosition);
            AlignObjectAtIndex(146, 147, 148, furies1[23].MsPosition);

            AgglomerateRestOrChordAt(114);

        }
        #endregion before Interlude3

        #region finale
        internal void GetFinale(List<PaletteDef> palettes, Dictionary<string, int> msPositions)
        {
            PaletteDef f3FinalePalette1 = palettes[10]; // correct 1.1.2014
            PaletteDef f3FinalePalette2 = palettes[14];
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

            VoiceDef f3Interlude3Verse4e = GetF3Interlude3Verse4EsCaped(f3FinalePalette1, krystal, strandIndices, msPositions);
            VoiceDef f3Verse4eVerse5 = GetF3Verse4EscapedVerse5Calls(f3FinalePalette2, krystal, strandIndices, msPositions);
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

            InsertInRest(furies3Finale);

            AdjustPitchWheelDeviations(msPositions["interlude3"], msPositions["endOfPiece"], 5, 28);
        }

        private VoiceDef GetF3Interlude3Verse4EsCaped(PaletteDef f3Int3Palette, ExpansionKrystal krystal, List<int> strandIndices, Dictionary<string, int> msPositions)
        {
            VoiceDef f33 = new VoiceDef(f3Int3Palette, krystal);

            //List<int> f3eStrandDurations = GetStrandDurations(f33, strandIndices);

            //int extraTime = 1000;
            //int diff = extraTime / f33.Count;
            //for(int i = f33.Count - 1; i > 0; --i)
            //{
            //    if(strandIndices.Contains(i))
            //    {
            //        UniqueMidiRestDef umrd = new UniqueMidiRestDef(f33[i].MsPosition, f3eStrandDurations[strandIndices.IndexOf(i)] + extraTime);
            //        extraTime -= diff;
            //        f33.Insert(i, umrd);
            //    }
            //}

            //f33.StartMsPosition = msPositions["interlude3Bar2"];

            //f33.RemoveBetweenMsPositions(msPositions["verse4EsCaped"], int.MaxValue);

            //if(f33[f33.Count - 1] is UniqueMidiRestDef)
            //{
            //    f33[f33.Count - 1].MsDuration = msPositions["verse4EsCaped"] - f33[f33.Count - 1].MsPosition;
            //}

            return f33;
        }

        private VoiceDef GetF3Verse4EscapedVerse5Calls(PaletteDef f3Int4Palette, ExpansionKrystal krystal, List<int> strandIndices, Dictionary<string, int> msPositions)
        {
            VoiceDef f34 = new VoiceDef(f3Int4Palette, krystal);

            //List<int> f3eStrandDurations = GetStrandDurations(f34, strandIndices);

            //int extraTime = 500;
            //int diff = extraTime / f34.Count;
            //for(int i = f34.Count - 1; i > 0; --i)
            //{
            //    if(strandIndices.Contains(i))
            //    {
            //        UniqueMidiRestDef umrd = new UniqueMidiRestDef(f34[i].MsPosition, f3eStrandDurations[strandIndices.IndexOf(i)] + extraTime);
            //        extraTime -= diff;
            //        f34.Insert(i, umrd);
            //    }
            //}

            //f34.StartMsPosition = msPositions["verse4EsCaped"];
            //f34.RemoveBetweenMsPositions(msPositions["verse5Calls"], int.MaxValue);

            //if(f34[f34.Count - 1] is UniqueMidiRestDef)
            //{
            //    f34[f34.Count - 1].MsDuration = msPositions["postlude"] - f34[f34.Count - 1].MsPosition;
            //}

            return f34;
        }

        private VoiceDef GetF3Postlude(PaletteDef f3PostludePalette, ExpansionKrystal krystal, List<int> strandIndices, Dictionary<string, int> msPositions)
        {
            VoiceDef f3p = new VoiceDef(f3PostludePalette, krystal);

            //List<int> f3eStrandDurations = GetStrandDurations(f3p, strandIndices);

            //for(int i = f3p.Count - 1; i > 0; --i)
            //{
            //    if(strandIndices.Contains(i))
            //    {
            //        UniqueMidiRestDef umrd = new UniqueMidiRestDef(f3p[i].MsPosition, f3eStrandDurations[strandIndices.IndexOf(i)] / 4);
            //        f3p.Insert(i, umrd);
            //    }
            //}

            //f3p.StartMsPosition = msPositions["postlude"];
            //f3p.RemoveBetweenMsPositions(msPositions["endOfPiece"], int.MaxValue);

            return f3p;
        }

        internal void AdjustAlignments(Clytemnestra clytemnestra, VoiceDef wind3)
        {
            // example code from furies1

            //Debug.Assert(this[213] is UniqueMidiRestDef);
            //this[213].MsDuration += this[212].MsDuration;
            //RemoveAt(212);
            //AgglomerateRests();

            //AlignObjectAtIndex(25, 84, 85, clytemnestra[196].MsPosition);
            //AlignObjectAtIndex(84, 85, 89, clytemnestra[204].MsPosition + 200);
            //AlignObjectAtIndex(85, 89, 96, clytemnestra[215].MsPosition);
            //AlignObjectAtIndex(89, 96, 102, clytemnestra[226].MsPosition);
            //AlignObjectAtIndex(102, 106, 117, clytemnestra[242].MsPosition);
            //AlignObjectAtIndex(106, 117, 140, clytemnestra[268].MsPosition);
            //AlignObjectAtIndex(117, 140, 163, wind3[61].MsPosition);
            //AlignObjectAtIndex(140, 163, 197, wind3[65].MsPosition);
            //AlignObjectAtIndex(163, 197, 206, clytemnestra[269].MsPosition - 200);
            //AlignObjectAtIndex(197, 206, 211, clytemnestra[283].MsPosition + 400);
            //AlignObjectAtIndex(206, 211, 212, clytemnestra[286].MsPosition);
            //AlignObjectAtIndex(211, 212, Count - 1, clytemnestra[289].MsPosition);
        }

        internal void AdjustVelocities(Dictionary<string, int> msPositions)
        {
            // example code from furies1

            //int indexAtVerse4 = FindIndexAtMsPosition(msPositions["verse4"]);
            //int indexAtInterval4 = FindIndexAtMsPosition(msPositions["interlude4"]);
            //int indexAtVerse5 = FindIndexAtMsPosition(msPositions["verse5"]);
            //int indexAtPostlude = FindIndexAtMsPosition(msPositions["postlude"]);

            //AdjustVelocities(indexAtVerse4, indexAtInterval4, 0.5);
            //AdjustVelocities(96, 106, 0.7);

            //AdjustVelocitiesHairpin(msPositions["interlude4"], msPositions["verse5"], 0.8, 1.0);

            //AdjustVelocities(indexAtVerse5, indexAtPostlude, 0.5);

            //AdjustVelocitiesHairpin(msPositions["postlude"], EndMsPosition, 0.8, 1.0);
        }

        internal void AdjustPostludePan(int postludeMsPosition)
        {
            // example code from furies1

            //double posDiff = ((double)(EndMsPosition - postludeMsPosition)) / 4;
            //int postludeMsPosition1 = postludeMsPosition + (int)posDiff;
            //int postludeMsPosition2 = postludeMsPosition + (int)(posDiff * 2);
            //int postludeMsPosition3 = postludeMsPosition + (int)(posDiff * 3);

            //SetPanGliss(postludeMsPosition, postludeMsPosition1, 64, 32);
            //SetPanGliss(postludeMsPosition1, postludeMsPosition2, 32, 96);
            //SetPanGliss(postludeMsPosition2, postludeMsPosition3, 96, 0);
            //SetPanGliss(postludeMsPosition3, EndMsPosition, 0, 127);
        }
        #endregion finale
    }
}
