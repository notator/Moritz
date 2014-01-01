using System.Collections.Generic;
using System.Diagnostics;
using System;

using Moritz.Score;
using Moritz.Score.Midi;
using Krystals4ObjectLibrary;

namespace Moritz.AssistantComposer
{
    internal class Furies4 : VoiceDef
    {
        internal Furies4(int msDuration)
            : base(msDuration)
        {
        }

        internal void GetBeforeInterlude3(int firstRestMsDuration, Clytemnestra clytemnestra, VoiceDef wind1, List<PaletteDef> palettes)
        {
            GetSnores(firstRestMsDuration, clytemnestra, wind1, palettes[1]);
            AddGrowlsToInterlude2AndVerse3(clytemnestra, palettes[3]);
        }

        #region before Interlude3
        private void AddGrowlsToInterlude2AndVerse3(Clytemnestra clytemnestra, PaletteDef growlsPalette)
        {
            int[] growlIndices = { 0,2,5,1 };
            //int[] transpositions = { 0,0,0,0 };
            //double[] velocityfactors = { 1,1,1,1 };
            int[] msPositions =
            { 
                this[24].MsPosition + 200, 
                this[26].MsPosition + 200, 
                this[30].MsPosition + 200, 
                this[32].MsPosition + 200, 
            };
            int[] msDurations =
            {
                this[24].MsDuration / 2,
                this[26].MsDuration / 2,
                this[30].MsDuration / 2,
                this[32].MsDuration / 2
            };

            for(int i = 3; i >= 0; --i)
            {
                IUniqueMidiDurationDef growl = growlsPalette[growlIndices[i]].CreateUniqueMidiDurationDef();
                growl.MsPosition = msPositions[i];
                growl.MsDuration = msDurations[i];
                //growl.AdjustVelocities(velocityfactors[i]);
                //growl.Transpose(transpositions[i]);
                InsertInRest(growl);
            }

            AgglomerateRestOrChordAt(40);

            AlignObjectAtIndex(34, 35, 36, clytemnestra[140].MsPosition);
            AlignObjectAtIndex(35, 36, 37, clytemnestra[141].MsPosition);
            AlignObjectAtIndex(38, 39, 40, clytemnestra[162].MsPosition);
        }

        private void GetSnores(int firstRestMsDuration, Clytemnestra clytemnestra, VoiceDef wind1, PaletteDef snoresPalette)
        {
            List<IUniqueMidiDurationDef> snores = new List<IUniqueMidiDurationDef>();
            int msPosition = 0;

            IUniqueMidiDurationDef firstRest = new UniqueMidiRestDef(msPosition, firstRestMsDuration);
            snores.Add(firstRest);
            msPosition += firstRestMsDuration;

            #region prelude + verse1
            int[] transpositions1 = { 0, 0, 0, 0, 0, 1, 0 };
            for(int i = 0; i < 7; ++i)
            {
                IUniqueMidiDurationDef snore = snoresPalette[i].CreateUniqueMidiDurationDef();
                snore.MsPosition = msPosition;
                msPosition += snore.MsDuration;
                snore.Transpose(transpositions1[i]);
                snore.PitchWheelDeviation = 3;
                snores.Add(snore);

                UniqueMidiRestDef rest = new UniqueMidiRestDef(msPosition, 2500);
                msPosition += rest.MsDuration;
                snores.Add(rest);
            }
            #endregion

            double factor;
            double msDuration;
            double restDuration;
            int[] transpositions2 = { 1, 1, 2, 2, 3, 3, 4, 4, 5, 5 };
            double[] factors = { 0.93, 0.865, 0.804, 0.748, 0.696, 0.647, 0.602, 0.56, 0.52, 0.484 };
            for(int i = 0; i < 10; ++i)
            {
                IUniqueMidiDurationDef snore = snoresPalette[i / 2].CreateUniqueMidiDurationDef();
                snore.MsPosition = msPosition;
                factor = factors[i];
                msDuration = snore.MsDuration * factor;
                snore.MsDuration = (int)msDuration;
                msPosition += snore.MsDuration;
                snore.Transpose(transpositions2[i]);
                snore.PitchWheelDeviation = 20;
                //snore.MidiVelocity = (byte)((double)snore.MidiVelocity * factor * factor);
                snores.Add(snore);

                restDuration = 2500 / factor;
                UniqueMidiRestDef rest = new UniqueMidiRestDef(msPosition, (int)restDuration);
                msPosition += rest.MsDuration;
                snores.Add(rest);
            }

            snores[snores.Count - 1].MsDuration = clytemnestra.EndMsPosition - snores[snores.Count - 1].MsPosition;

            this._uniqueMidiDurationDefs = snores;

            AdjustVelocitiesHairpin(13, Count, 0.25);

            #region alignments before Interlude3
            AlignObjectAtIndex(7, 8, 9, clytemnestra[3].MsPosition);
            AlignObjectAtIndex(8, 9, 10, clytemnestra[7].MsPosition);
            AlignObjectAtIndex(9, 10, 11, clytemnestra[16].MsPosition);
            AlignObjectAtIndex(10, 11, 12, clytemnestra[24].MsPosition);
            AlignObjectAtIndex(11, 12, 13, clytemnestra[39].MsPosition);
            AlignObjectAtIndex(12, 13, 14, clytemnestra[42].MsPosition);
            AlignObjectAtIndex(14, 34, Count, wind1[38].MsPosition); // rest at start of Interlude3
            #endregion

            RemoveScorePitchWheelCommands(0, 13); // pitchwheeldeviation is 20 for R2M
        }
        #endregion before Interlude3

        #region finale
        internal void GetFinale(List<PaletteDef> palettes, Dictionary<string, int> msPositions)
        {
            PaletteDef f4Interlude3Palette = palettes[9];
            PaletteDef f4Interlude4Palette = palettes[13];
            PaletteDef f4PostludePalette = palettes[17];

            //PermutationKrystal krystal = new PermutationKrystal("C://Moritz/krystals/krystals/pk4(12)-2.krys");
            ExpansionKrystal krystal = new ExpansionKrystal("C://Moritz/krystals/krystals/xk3(12.12.1)-1.krys");
            List<int> strandIndices = new List<int>();
            int index = 0;
            for(int i = 0; i < krystal.Strands.Count; ++i)
            {
                strandIndices.Add(index);
                index += krystal.Strands[i].Values.Count;
            }

            VoiceDef f4Interlude3Verse4e = GetF4Interlude3Verse4EsCaped(f4Interlude3Palette, krystal, strandIndices, msPositions);
            VoiceDef f4Verse4eVerse5 = GetF4Verse4EscapedVerse5Calls(f4Interlude4Palette, krystal, strandIndices, msPositions);
            VoiceDef f4Postlude = GetF4Postlude(f4PostludePalette, krystal, strandIndices, msPositions);

            VoiceDef furies4Finale = f4Interlude3Verse4e;

            furies4Finale.AddRange(f4Verse4eVerse5);
            furies4Finale.AddRange(f4Postlude);

            if(furies4Finale[furies4Finale.Count - 1] is UniqueMidiRestDef)
            {
                furies4Finale.RemoveAt(furies4Finale.Count - 1);
            }

            if(furies4Finale[furies4Finale.Count - 1].MsPosition + furies4Finale[furies4Finale.Count - 1].MsDuration > msPositions["endOfPiece"])
            {
                furies4Finale.RemoveAt(furies4Finale.Count - 1);
            }

            InsertInRest(furies4Finale);

            AdjustPitchWheelDeviations(msPositions["interlude3"], msPositions["endOfPiece"], 5, 28);
        }

        private VoiceDef GetF4Interlude3Verse4EsCaped(PaletteDef f4Int3Palette, ExpansionKrystal krystal, List<int> strandIndices, Dictionary<string, int> msPositions)
        {
            VoiceDef f43 = new VoiceDef(f4Int3Palette, krystal);

            //List<int> f4eStrandDurations = GetStrandDurations(f43, strandIndices);

            //int extraTime = 1000;
            //int diff = extraTime / f43.Count;
            //for(int i = f43.Count - 1; i > 0; --i)
            //{
            //    if(strandIndices.Contains(i))
            //    {
            //        UniqueMidiRestDef umrd = new UniqueMidiRestDef(f43[i].MsPosition, f4eStrandDurations[strandIndices.IndexOf(i)] + extraTime);
            //        extraTime -= diff;
            //        f43.Insert(i, umrd);
            //    }
            //}

            //f43.StartMsPosition = msPositions["interlude3Bar2"];

            //f43.RemoveBetweenMsPositions(msPositions["verse4EsCaped"], int.MaxValue);

            //if(f43[f43.Count - 1] is UniqueMidiRestDef)
            //{
            //    f43[f43.Count - 1].MsDuration = msPositions["verse4EsCaped"] - f43[f43.Count - 1].MsPosition;
            //}

            return f43;
        }

        private VoiceDef GetF4Verse4EscapedVerse5Calls(PaletteDef f4Int4Palette, ExpansionKrystal krystal, List<int> strandIndices, Dictionary<string, int> msPositions)
        {
            VoiceDef f44 = new VoiceDef(f4Int4Palette, krystal);

            //List<int> f4eStrandDurations = GetStrandDurations(f44, strandIndices);

            //int extraTime = 500;
            //int diff = extraTime / f44.Count;
            //for(int i = f44.Count - 1; i > 0; --i)
            //{
            //    if(strandIndices.Contains(i))
            //    {
            //        UniqueMidiRestDef umrd = new UniqueMidiRestDef(f44[i].MsPosition, f4eStrandDurations[strandIndices.IndexOf(i)] + extraTime);
            //        extraTime -= diff;
            //        f44.Insert(i, umrd);
            //    }
            //}

            //f44.StartMsPosition = msPositions["verse4EsCaped"];
            //f44.RemoveBetweenMsPositions(msPositions["verse5Calls"], int.MaxValue);

            //if(f44[f44.Count - 1] is UniqueMidiRestDef)
            //{
            //    f44[f44.Count - 1].MsDuration = msPositions["postlude"] - f44[f44.Count - 1].MsPosition;
            //}

            return f44;
        }

        private VoiceDef GetF4Postlude(PaletteDef f4PostludePalette, ExpansionKrystal krystal, List<int> strandIndices, Dictionary<string, int> msPositions)
        {
            VoiceDef f4p = new VoiceDef(f4PostludePalette, krystal);

            //List<int> f4eStrandDurations = GetStrandDurations(f4p, strandIndices);

            //for(int i = f4p.Count - 1; i > 0; --i)
            //{
            //    if(strandIndices.Contains(i))
            //    {
            //        UniqueMidiRestDef umrd = new UniqueMidiRestDef(f4p[i].MsPosition, f4eStrandDurations[strandIndices.IndexOf(i)] / 4);
            //        f4p.Insert(i, umrd);
            //    }
            //}

            //f4p.StartMsPosition = msPositions["postlude"];
            //f4p.RemoveBetweenMsPositions(msPositions["endOfPiece"], int.MaxValue);

            return f4p;
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
