using System.Collections.Generic;
using System.Diagnostics;
using System;

using Moritz.Score;
using Moritz.Score.Midi;
using Krystals4ObjectLibrary;

namespace Moritz.AssistantComposer
{
    /// <summary>
    /// The Furies1 "constructor" (part of the Song Six algorithm).
    /// </summary>
    internal partial class SongSixAlgorithm : MidiCompositionAlgorithm
    {
        #region before interlude3
        private VoiceDef GetFuries1(Clytemnestra clytemnestra, VoiceDef wind1, VoiceDef furies3, VoiceDef furies2, List<PaletteDef> _paletteDefs)
        {
            VoiceDef furies1 = GetF1Interlude2AndVerse3(clytemnestra, wind1, furies2, _paletteDefs[8]);

            return furies1;
        }
        private VoiceDef GetF1Interlude2AndVerse3(Clytemnestra clytemnestra, VoiceDef wind1, VoiceDef furies2, PaletteDef cheepsPalette)
        {
            VoiceDef furies1 = GetEmptyVoiceDef(wind1.EndMsPosition);

            int[] cheepIndices = { 4, 8, 2, 6, 10, 0, 1, 3, 5, 7, 9, 11 };
            int[] transpositions = { 2, 1, 3, 0, 4, -3, 5, 10, 6, 9, 7, 8 };
            double[] velocityfactors = { 0.32, 0.31, 0.34, 0.3, 0.35, 0.37, 0.36, 0.43, 0.37, 0.42, 0.39, 0.4 };
            int[] msPositions =
            { 
                furies2[8].MsPosition + 200, 
                furies2[12].MsPosition + 100, 
                furies2[24].MsPosition + 300, 
                furies2[30].MsPosition + 400, 
                furies2[40].MsPosition + 500,
                clytemnestra[122].MsPosition,
                clytemnestra[132].MsPosition + 110,
                clytemnestra[141].MsPosition + 220,
                clytemnestra[150].MsPosition + 330,
                clytemnestra[158].MsPosition + 440,
                clytemnestra[164].MsPosition + 550,
                clytemnestra[173].MsPosition,
            };
            for(int i = 0; i < 12; ++i)
            {
                IUniqueMidiDurationDef cheep = cheepsPalette[cheepIndices[i]].CreateUniqueMidiDurationDef();
                cheep.MsPosition = msPositions[i];
                cheep.MsDuration *= 2;
                cheep.AdjustVelocities(velocityfactors[i]);
                cheep.Transpose(transpositions[i]);
                furies1.InsertInRest(cheep);
            }

            furies1.AlignObjectAtIndex(11, 12, 13, clytemnestra[123].MsPosition);
            furies1.AlignObjectAtIndex(21, 22, 23, clytemnestra[168].MsPosition);

            return furies1;
        }
        #endregion before interlude3

        #region finale
        private VoiceDef GetF1Finale(List<PaletteDef> palettes, Dictionary<string, int> msPositions)
        {
            PaletteDef f1Interlude3Palette = palettes[12];
            PaletteDef f1Interlude4Palette = palettes[16];
            PaletteDef f1PostludePalette = palettes[20];

            //PermutationKrystal krystal = new PermutationKrystal("C://Moritz/krystals/krystals/pk4(12)-2.krys");
            ExpansionKrystal krystal = new ExpansionKrystal("C://Moritz/krystals/krystals/xk3(12.12.1)-1.krys");
            List<int> strandIndices = new List<int>();
            int index = 0;
            for(int i = 0; i < krystal.Strands.Count; ++i)
            {
                strandIndices.Add(index);
                index += krystal.Strands[i].Values.Count;
            }

            VoiceDef f1Interlude3Verse4e = GetF1Interlude3Verse4EsCaped(f1Interlude3Palette, krystal, strandIndices, msPositions);
            VoiceDef f1Verse4eVerse5 = GetF1Verse4EscapedVerse5Calls(f1Interlude4Palette, krystal, strandIndices, msPositions);
            VoiceDef f1Postlude = GetF1Postlude(f1PostludePalette, krystal, strandIndices, msPositions);

            VoiceDef furies1Finale = f1Interlude3Verse4e;

            furies1Finale.AddRange(f1Verse4eVerse5);
            furies1Finale.AddRange(f1Postlude);

            if(furies1Finale[furies1Finale.Count - 1] is UniqueMidiRestDef)
            {
                furies1Finale.RemoveAt(furies1Finale.Count - 1);
            }

            if(furies1Finale[furies1Finale.Count - 1].MsPosition + furies1Finale[furies1Finale.Count - 1].MsDuration > msPositions["endOfPiece"])
            {
                furies1Finale.RemoveAt(furies1Finale.Count - 1);
            }

            AdjustFuriesFinalePitchWheelDeviations(furies1Finale);

            return furies1Finale;
        }

        private VoiceDef GetF1Interlude3Verse4EsCaped(PaletteDef f1Int3Palette, ExpansionKrystal krystal, List<int> strandIndices, Dictionary<string, int> msPositions)
        {
            VoiceDef f13 = new VoiceDef(f1Int3Palette, krystal);

            List<int> f1eStrandDurations = GetStrandDurations(f13, strandIndices);

            int extraTime = 1000;
            int diff = extraTime / f13.Count;
            for(int i = f13.Count - 1; i > 0; --i)
            {
                if(strandIndices.Contains(i))
                {
                    UniqueMidiRestDef umrd = new UniqueMidiRestDef(f13[i].MsPosition, f1eStrandDurations[strandIndices.IndexOf(i)] + extraTime);
                    extraTime -= diff;
                    f13.Insert(i, umrd);
                }
            }

            f13.StartMsPosition = msPositions["interlude3Bar2"];

            f13.RemoveBetweenMsPositions(msPositions["verse4EsCaped"], int.MaxValue);

            if(f13[f13.Count - 1] is UniqueMidiRestDef)
            {
                f13[f13.Count - 1].MsDuration = msPositions["verse4EsCaped"] - f13[f13.Count - 1].MsPosition;
            }

            return f13;
        }

        private VoiceDef GetF1Verse4EscapedVerse5Calls(PaletteDef f1Int4Palette, ExpansionKrystal krystal, List<int> strandIndices, Dictionary<string, int> msPositions)
        {
            VoiceDef f14 = new VoiceDef(f1Int4Palette, krystal);

            List<int> f1eStrandDurations = GetStrandDurations(f14, strandIndices);

            int extraTime = 500;
            int diff = extraTime / f14.Count;
            for(int i = f14.Count - 1; i > 0; --i)
            {
                if(strandIndices.Contains(i))
                {
                    UniqueMidiRestDef umrd = new UniqueMidiRestDef(f14[i].MsPosition, f1eStrandDurations[strandIndices.IndexOf(i)] + extraTime);
                    extraTime -= diff;
                    f14.Insert(i, umrd);
                }
            }

            f14.StartMsPosition = msPositions["verse4EsCaped"];
            f14.RemoveBetweenMsPositions(msPositions["verse5Calls"], int.MaxValue);

            if(f14[f14.Count - 1] is UniqueMidiRestDef)
            {
                f14[f14.Count - 1].MsDuration = msPositions["postlude"] - f14[f14.Count - 1].MsPosition;
            }

            return f14;
        }

        private VoiceDef GetF1Postlude(PaletteDef f1PostludePalette, ExpansionKrystal krystal, List<int> strandIndices, Dictionary<string, int> msPositions)
        {
            VoiceDef f1p = new VoiceDef(f1PostludePalette, krystal);

            List<int> f1eStrandDurations = GetStrandDurations(f1p, strandIndices);

            for(int i = f1p.Count - 1; i > 0; --i)
            {
                if(strandIndices.Contains(i))
                {
                    UniqueMidiRestDef umrd = new UniqueMidiRestDef(f1p[i].MsPosition, f1eStrandDurations[strandIndices.IndexOf(i)] / 4);
                    f1p.Insert(i, umrd);
                }
            }

            f1p.StartMsPosition = msPositions["postlude"];
            f1p.RemoveBetweenMsPositions(msPositions["endOfPiece"], int.MaxValue);

            return f1p;
        }

        private void AdjustF1Alignments(VoiceDef furies1, Clytemnestra clytemnestra, VoiceDef wind3)
        {
            Debug.Assert(furies1[213] is UniqueMidiRestDef);
            furies1[213].MsDuration += furies1[212].MsDuration;
            furies1.RemoveAt(212);
            furies1.AgglomerateRests();

            furies1.AlignObjectAtIndex(25, 84, 85, clytemnestra[196].MsPosition);
            furies1.AlignObjectAtIndex(84, 85, 89, clytemnestra[204].MsPosition + 200);
            furies1.AlignObjectAtIndex(85, 89, 96, clytemnestra[215].MsPosition);
            furies1.AlignObjectAtIndex(89, 96, 102, clytemnestra[226].MsPosition);
            furies1.AlignObjectAtIndex(102, 106, 117, clytemnestra[242].MsPosition);
            furies1.AlignObjectAtIndex(106, 117, 140, clytemnestra[268].MsPosition);
            furies1.AlignObjectAtIndex(117, 140, 163, wind3[61].MsPosition);
            furies1.AlignObjectAtIndex(140, 163, 197, wind3[65].MsPosition);
            furies1.AlignObjectAtIndex(163, 197, 206, clytemnestra[269].MsPosition - 200);
            furies1.AlignObjectAtIndex(197, 206, 211, clytemnestra[283].MsPosition + 400);
            furies1.AlignObjectAtIndex(206, 211, 212, clytemnestra[286].MsPosition);
            furies1.AlignObjectAtIndex(211, 212, furies1.Count - 1, clytemnestra[289].MsPosition);
        }

        private void AdjustF1Velocities(VoiceDef furies1, Dictionary<string, int> msPositions)
        {
            int indexAtVerse4 = furies1.FindIndexAtMsPosition(msPositions["verse4"]);
            int indexAtInterval4 = furies1.FindIndexAtMsPosition(msPositions["interlude4"]);
            int indexAtVerse5 = furies1.FindIndexAtMsPosition(msPositions["verse5"]);
            int indexAtPostlude = furies1.FindIndexAtMsPosition(msPositions["postlude"]);

            furies1.AdjustVelocities(indexAtVerse4, indexAtInterval4, 0.5);
            furies1.AdjustVelocities(96, 106, 0.7);

            furies1.AdjustVelocitiesHairpin(msPositions["interlude4"], msPositions["verse5"], 0.8, 1.0);

            furies1.AdjustVelocities(indexAtVerse5, indexAtPostlude, 0.5);

            //furies1.AdjustVelocitiesHairpin(msPositions["postlude"], msPositions["finalWindChord"], 0.8, 1.0);
            //furies1.AdjustVelocitiesHairpin(msPositions["finalWindChord"], furies1.EndMsPosition, 1.0, 0);

            furies1.AdjustVelocitiesHairpin(msPositions["postlude"], furies1.EndMsPosition, 0.8, 1.0);
        }

        private void AdjustF1PostludePan(VoiceDef furies1, int postludeMsPosition)
        {
            double posDiff = ((double)(furies1.EndMsPosition - postludeMsPosition)) / 4;
            int postludeMsPosition1 = postludeMsPosition + (int)posDiff;
            int postludeMsPosition2 = postludeMsPosition + (int)(posDiff * 2);
            int postludeMsPosition3 = postludeMsPosition + (int)(posDiff * 3);

            furies1.SetPan(postludeMsPosition, postludeMsPosition1, 64, 32);
            furies1.SetPan(postludeMsPosition1, postludeMsPosition2, 32, 96);
            furies1.SetPan(postludeMsPosition2, postludeMsPosition3, 96, 0);
            furies1.SetPan(postludeMsPosition3, furies1.EndMsPosition, 0, 127);
        }
        #endregion finale
    }
}
