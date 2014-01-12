using System.Collections.Generic;
using System.Diagnostics;
using System;

using Moritz.Score;
using Moritz.Score.Midi;
using Krystals4ObjectLibrary;

namespace Moritz.AssistantComposer
{
    internal class Furies2 : VoiceDef
    {
        internal Furies2(int msDuration)
            : base(msDuration)
        {
        }

        #region before Interlude3
        internal void GetBeforeInterlude3(Clytemnestra clytemnestra, VoiceDef wind1, VoiceDef furies3, List<PaletteDef> _paletteDefs)
        {
            GetFuries2Interlude2(clytemnestra, wind1, furies3);
            AddFuries2ChirpsForInterlude2AndVerse3(clytemnestra, wind1, _paletteDefs[7]);
        }
        /// <summary>
        /// Steals the ticks from furies 3, then agglommerates the remaining rests in furies3...
        /// </summary>
        private void GetFuries2Interlude2(Clytemnestra clytemnestra, VoiceDef wind1, VoiceDef furies3)
        {
            List<int> furies3TickIndices = new List<int>()
            {
                66,70,74,81,85,89,93,
                96,100,104,109,113,117,122,
                126,130,135,139,143,148,152
            };
            for(int i = 0; i < furies3TickIndices.Count; ++i)
            {
                int f3Index = furies3TickIndices[i];
                IUniqueMidiDurationDef ticksChord = furies3[f3Index];
                Debug.Assert(ticksChord is UniqueMidiChordDef);
                UniqueMidiRestDef ticksRest = new UniqueMidiRestDef(ticksChord.MsPosition, ticksChord.MsDuration);
                furies3.Replace(f3Index, ticksRest);
                InsertInRest(ticksChord);
            }

            UniqueMidiChordDef lastTicksBeforeVerse3 = new UniqueMidiChordDef(this[39] as MidiChordDef);
            lastTicksBeforeVerse3.MsPosition = furies3[155].MsPosition + furies3[155].MsDuration;
            lastTicksBeforeVerse3.MsDuration = clytemnestra[117].MsPosition - lastTicksBeforeVerse3.MsPosition;
            lastTicksBeforeVerse3.Transpose(10);
            InsertInRest(lastTicksBeforeVerse3);

            furies3.AgglomerateRests();
        }

        private void AddFuries2ChirpsForInterlude2AndVerse3(Clytemnestra clytemnestra, VoiceDef wind1, PaletteDef chirpsPalette)
        {
            int[] chirpIndices = { 4, 6, 10, 0, 1, 3, 5, 7, 9, 11 };
            int[] transpositions = { 2, 0, 4, 11, 5, 10, 6, 9, 7, 8 };
            //double[] velocityfactors = { 0.3, 0.31, 0.32, 0.34, 0.35, 0.36, 0.37, 0.39, 0.4, 0.42, 0.43, 0.45 };
            double[] velocityfactors = { 0.32, 0.3, 0.35, 0.45, 0.36, 0.43, 0.37, 0.42, 0.39, 0.4 };
            int[] msPositions =
            { 
                this[2].MsPosition, 
                this[6].MsPosition, 
                this[16].MsPosition, 
                this[26].MsPosition,
                this[26].MsPosition + chirpsPalette[chirpIndices[3]].MsDuration,
                clytemnestra[129].MsPosition,
                clytemnestra[143].MsPosition + 110,
                clytemnestra[156].MsPosition + 220,
                clytemnestra[156].MsPosition + 220 + chirpsPalette[chirpIndices[7]].MsDuration,
                clytemnestra[168].MsPosition + 330,
            };
            for(int i = 9; i >=0 ; --i)
            {
                IUniqueMidiDurationDef cheep = chirpsPalette[chirpIndices[i]].CreateUniqueMidiDurationDef();
                Debug.Assert(cheep is UniqueMidiChordDef);
                cheep.MsPosition = msPositions[i];
                //cheep.MsDuration *= 2;
                cheep.AdjustVelocities(velocityfactors[i]);
                cheep.Transpose(transpositions[i]);
                InsertInRest(cheep);
            }

            AlignObjectAtIndex(50, 51, 52, clytemnestra[130].MsPosition);
            AlignObjectAtIndex(55, 56, 57, clytemnestra[159].MsPosition);

            AgglomerateRestOrChordAt(31);
        }
        #endregion before Interlude3

        #region finale

        internal void GetFinale(List<PaletteDef> palettes, Dictionary<string, int> msPositions)
        {
            PaletteDef f2FinalePalette1 = palettes[11];
            PaletteDef f2FinalePalette2 = palettes[15];
            PaletteDef f2PostludePalette = palettes[19];

            //PermutationKrystal krystal = new PermutationKrystal("C://Moritz/krystals/krystals/pk4(12)-2.krys");
            ExpansionKrystal krystal = new ExpansionKrystal("C://Moritz/krystals/krystals/xk3(12.12.1)-1.krys");
            List<int> strandIndices = new List<int>();
            int index = 0;
            for(int i = 0; i < krystal.Strands.Count; ++i)
            {
                strandIndices.Add(index);
                index += krystal.Strands[i].Values.Count;
            }

            VoiceDef f2Interlude3Verse4e = GetF2Interlude3Verse4EsCaped(f2FinalePalette1, krystal, strandIndices, msPositions);
            VoiceDef f2Verse4eVerse5 = GetF2Verse4EscapedVerse5Calls(f2FinalePalette2, krystal, strandIndices, msPositions);
            VoiceDef f2Postlude = GetF2Postlude(f2PostludePalette, krystal, strandIndices, msPositions);

            VoiceDef furies2Finale = f2Interlude3Verse4e;

            furies2Finale.AddRange(f2Verse4eVerse5);
            furies2Finale.AddRange(f2Postlude);

            if(furies2Finale[furies2Finale.Count - 1] is UniqueMidiRestDef)
            {
                furies2Finale.RemoveAt(furies2Finale.Count - 1);
            }

            if(furies2Finale[furies2Finale.Count - 1].MsPosition + furies2Finale[furies2Finale.Count - 1].MsDuration > msPositions["endOfPiece"])
            {
                furies2Finale.RemoveAt(furies2Finale.Count - 1);
            }

            InsertInRest(furies2Finale);

            AdjustPitchWheelDeviations(msPositions["interlude3"], msPositions["endOfPiece"], 5, 28);
        }

        private VoiceDef GetF2Interlude3Verse4EsCaped(PaletteDef f2Int3Palette, ExpansionKrystal krystal, List<int> strandIndices, Dictionary<string, int> msPositions)
        {
            VoiceDef f23 = new VoiceDef(f2Int3Palette, krystal);

            //List<int> f2eStrandDurations = GetStrandDurations(f23, strandIndices);

            //int extraTime = 1000;
            //int diff = extraTime / f23.Count;
            //for(int i = f23.Count - 1; i > 0; --i)
            //{
            //    if(strandIndices.Contains(i))
            //    {
            //        UniqueMidiRestDef umrd = new UniqueMidiRestDef(f23[i].MsPosition, f2eStrandDurations[strandIndices.IndexOf(i)] + extraTime);
            //        extraTime -= diff;
            //        f23.Insert(i, umrd);
            //    }
            //}

            //f23.StartMsPosition = msPositions["interlude3Bar2"];

            //f23.RemoveBetweenMsPositions(msPositions["verse4EsCaped"], int.MaxValue);

            //if(f23[f23.Count - 1] is UniqueMidiRestDef)
            //{
            //    f23[f23.Count - 1].MsDuration = msPositions["verse4EsCaped"] - f23[f23.Count - 1].MsPosition;
            //}

            return f23;
        }

        private VoiceDef GetF2Verse4EscapedVerse5Calls(PaletteDef f2Int4Palette, ExpansionKrystal krystal, List<int> strandIndices, Dictionary<string, int> msPositions)
        {
            VoiceDef f24 = new VoiceDef(f2Int4Palette, krystal);

            //List<int> f2eStrandDurations = GetStrandDurations(f24, strandIndices);

            //int extraTime = 500;
            //int diff = extraTime / f24.Count;
            //for(int i = f24.Count - 1; i > 0; --i)
            //{
            //    if(strandIndices.Contains(i))
            //    {
            //        UniqueMidiRestDef umrd = new UniqueMidiRestDef(f24[i].MsPosition, f2eStrandDurations[strandIndices.IndexOf(i)] + extraTime);
            //        extraTime -= diff;
            //        f24.Insert(i, umrd);
            //    }
            //}

            //f24.StartMsPosition = msPositions["verse4EsCaped"];
            //f24.RemoveBetweenMsPositions(msPositions["verse5Calls"], int.MaxValue);

            //if(f24[f24.Count - 1] is UniqueMidiRestDef)
            //{
            //    f24[f24.Count - 1].MsDuration = msPositions["postlude"] - f24[f24.Count - 1].MsPosition;
            //}

            return f24;
        }

        private VoiceDef GetF2Postlude(PaletteDef f2PostludePalette, ExpansionKrystal krystal, List<int> strandIndices, Dictionary<string, int> msPositions)
        {
            VoiceDef f2p = new VoiceDef(f2PostludePalette, krystal);

            //List<int> f2eStrandDurations = GetStrandDurations(f2p, strandIndices);

            //for(int i = f2p.Count - 1; i > 0; --i)
            //{
            //    if(strandIndices.Contains(i))
            //    {
            //        UniqueMidiRestDef umrd = new UniqueMidiRestDef(f2p[i].MsPosition, f2eStrandDurations[strandIndices.IndexOf(i)] / 4);
            //        f2p.Insert(i, umrd);
            //    }
            //}

            //f2p.StartMsPosition = msPositions["postlude"];
            //f2p.RemoveBetweenMsPositions(msPositions["endOfPiece"], int.MaxValue);

            return f2p;
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