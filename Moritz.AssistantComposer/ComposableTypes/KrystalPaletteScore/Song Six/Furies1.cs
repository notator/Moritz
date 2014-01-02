using System.Collections.Generic;
using System.Diagnostics;
using System;

using Moritz.Score;
using Moritz.Score.Midi;
using Krystals4ObjectLibrary;

namespace Moritz.AssistantComposer
{
    internal class Furies1 : VoiceDef
    {


        internal Furies1(int msDuration)
            : base(msDuration)
        {
        }

        internal void GetBeforeInterlude3(Clytemnestra clytemnestra, VoiceDef wind1, VoiceDef furies2, PaletteDef cheepsPalette)
        {
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
                InsertInRest(cheep);
            }

            AlignObjectAtIndex(11, 12, 13, clytemnestra[123].MsPosition);
            AlignObjectAtIndex(21, 22, 23, clytemnestra[168].MsPosition);
        }


        #region finale
        internal void GetFinale(List<PaletteDef> palettes, Dictionary<string, int> msPositions)
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

            InsertInRest(furies1Finale);

            Erase(this[282].MsPosition, msPositions["endOfPiece"]);
            
            AdjustPitchWheelDeviations(msPositions["interlude3"], msPositions["endOfPiece"], 5, 28 );
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

        /// <summary>
        /// voiceDef contains the UniqueMidiChordDefs defined by a krystal, and nothing else.
        /// </summary>
        /// <param name="voiceDef"></param>
        /// <param name="strandIndices"></param>
        /// <returns></returns>
        private List<int> GetStrandDurations(VoiceDef voiceDef, List<int> strandIndices)
        {
            List<int> strandDurations = new List<int>();
            int duration;
            for(int i = 1; i < strandIndices.Count; ++i)
            {
                duration = 0;
                for(int j = strandIndices[i - 1]; j < strandIndices[i]; ++j)
                {
                    duration += voiceDef[j].MsDuration;
                }
                strandDurations.Add(duration);
            }
            duration = 0;
            for(int i = strandIndices[strandIndices.Count - 1]; i < voiceDef.Count; ++i)
            {
                duration += voiceDef[i].MsDuration;
            }
            strandDurations.Add(duration);
            return strandDurations;
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

        internal void AdjustAlignments(Clytemnestra clytemnestra, VoiceDef wind3)
        {
            Debug.Assert(this[213] is UniqueMidiRestDef);
            this[213].MsDuration += this[212].MsDuration;
            RemoveAt(212);
            AgglomerateRests();

            AlignObjectAtIndex(25, 84, 85, clytemnestra[196].MsPosition);
            AlignObjectAtIndex(84, 85, 89, clytemnestra[204].MsPosition + 200);
            AlignObjectAtIndex(85, 89, 96, clytemnestra[215].MsPosition);
            AlignObjectAtIndex(89, 96, 102, clytemnestra[226].MsPosition);
            AlignObjectAtIndex(102, 106, 117, clytemnestra[242].MsPosition);
            AlignObjectAtIndex(106, 117, 140, clytemnestra[268].MsPosition);
            AlignObjectAtIndex(117, 140, 163, wind3[61].MsPosition);
            AlignObjectAtIndex(140, 163, 197, wind3[65].MsPosition);
            AlignObjectAtIndex(163, 197, 206, clytemnestra[269].MsPosition - 200);
            AlignObjectAtIndex(197, 206, 211, clytemnestra[283].MsPosition + 400);
            AlignObjectAtIndex(206, 211, 212, clytemnestra[286].MsPosition);
            AlignObjectAtIndex(211, 212, Count - 1, clytemnestra[289].MsPosition);
        }

        internal void AdjustVelocities(Dictionary<string, int> msPositions)
        {
            AdjustVelocitiesHairpin(msPositions["interlude3"], this[102].MsPosition, 1.0, 0.5);
            AdjustVelocitiesHairpin(this[102].MsPosition, msPositions["interlude4"], 0.5, 0.8);
            AdjustVelocitiesHairpin(msPositions["interlude4"], msPositions["verse5"], 0.8, 1.0);
            AdjustVelocitiesHairpin(msPositions["verse5"], msPositions["postlude"], 0.5, 0.5);
            AdjustVelocitiesHairpin(msPositions["postlude"], EndMsPosition, 0.8, 1.0);
        }

        #endregion finale
    }
}
