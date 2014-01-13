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
            //PermutationKrystal krystal = new PermutationKrystal("C://Moritz/krystals/krystals/pk4(12)-2.krys");
            ExpansionKrystal krystal = new ExpansionKrystal("C://Moritz/krystals/krystals/xk4(12.12.1)-1.krys");

            VoiceDef furies2Finale = GetF2Finale(palettes, krystal, msPositions);

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

        private List<int> GetStrandIndices(ExpansionKrystal krystal)
        {
 	        List<int> strandIndices = new List<int>();
            int index = 0;
            for(int i = 0; i < krystal.Strands.Count; ++i)
            {
                strandIndices.Add(index);
                index += krystal.Strands[i].Values.Count;
            }

            return strandIndices;
        }

        private VoiceDef GetF2Finale(List<PaletteDef> palettes, ExpansionKrystal krystal, Dictionary<string, int> msPositions)
        {
            PaletteDef f2FinalePalette1 = palettes[11];
            PaletteDef f2FinalePalette2 = palettes[15];
            PaletteDef f2PostludePalette = palettes[19];

            List<int> strandIndices = GetStrandIndices(krystal);

            VoiceDef finalePart1 = new VoiceDef(f2FinalePalette1, krystal);
            Transform(finalePart1, msPositions, strandIndices);
            VoiceDef finalePart2 = new VoiceDef(f2FinalePalette2, krystal);
            Transform(finalePart2, msPositions, strandIndices);
            VoiceDef postlude = new VoiceDef(f2PostludePalette, krystal);
            Transform(postlude, msPositions, strandIndices);

            VoiceDef finale = GetFinaleSections(finalePart1, finalePart2, postlude, 59, 175);

            return finale;

            #region old
            //List<int> strandIndices = new List<int>();
            //int index = 0;
            //for(int i = 0; i < krystal.Strands.Count; ++i)
            //{
            //    strandIndices.Add(index);
            //    index += krystal.Strands[i].Values.Count;
            //}

            //VoiceDef f2IFinalePart1 = GetF2FinalePart1(f2FinalePalette1, krystal, strandIndices, msPositions);
            //VoiceDef f2FinalePart2 = GetF2FinalePart2(f2FinalePalette2, krystal, strandIndices, msPositions);
            //VoiceDef f2Postlude = GetF2Postlude(f2PostludePalette, krystal, strandIndices, msPositions);

            //VoiceDef furies2Finale = f2IFinalePart1;

            //furies2Finale.AddRange(f2FinalePart2);
            //furies2Finale.AddRange(f2Postlude);
            //furies2Finale.AgglomerateRests();
            #endregion
        }

        /// <summary>
        /// ACHTUNG: this function should be in a furies class. It is used by furies 2 and furies 4 (probably furies 3 too!)
        /// The three argument VoiceDefs are parallel. They have the same number of DurationDefs, each having the same MsPosition
        /// and MsDuration. The DurationDefs come from different palettes, so can otherwise have different parameters.
        /// This function simply creates a new VoiceDef by selecting the apropriate DurationDefs from each VoiceDef argument.
        /// </summary>
        private VoiceDef GetFinaleSections(VoiceDef finalePart1, VoiceDef finalePart2, VoiceDef postlude, int part2Index, int postludeIndex)
        {
            List<IUniqueMidiDurationDef> iumdds = new List<IUniqueMidiDurationDef>();

            for(int i = 0; i < part2Index; ++i)
            {
                iumdds.Add(finalePart1[i]);
            }
            for(int i = part2Index; i < postludeIndex; ++i)
            {
                iumdds.Add(finalePart2[i]);
            }
            for(int i = postludeIndex; i < postlude.Count; ++i)
            {
                iumdds.Add(postlude[i]);
            }

            return new VoiceDef(iumdds);
        }

        private void Transform(VoiceDef section, Dictionary<string, int> msPositions, List<int> strandIndices)
        {
            List<int> strandDurations = GetStrandDurations(section, strandIndices);

            int extraTime = 750;
            int diff = extraTime / section.Count;
            //bool rest = false;
            for(int i = section.Count - 1; i > 0; --i)
            {
                if(strandIndices.Contains(i))
                {
                    //if(rest)
                    //{
                    UniqueMidiRestDef umrd = new UniqueMidiRestDef(section[i].MsPosition, strandDurations[strandIndices.IndexOf(i)] + extraTime);
                    extraTime -= diff;
                    section.Insert(i, umrd);
                    //}
                    //rest = !rest;
                }
            }

            section.StartMsPosition = msPositions["furies2FinaleStart"];

            //double factor = 10;

            //section.AdjustMsDurations(factor);

            section.CreateAccel(0, section.Count, 0.25);

            //section.RemoveBetweenMsPositions(msPositions["interlude4End"], int.MaxValue);
            section.RemoveBetweenMsPositions(msPositions["finalWindChord"], int.MaxValue);

            if(section[section.Count - 1] is UniqueMidiRestDef)
            {
                //section[section.Count - 1].MsDuration = msPositions["interlude4End"] - section[section.Count - 1].MsPosition;
                section[section.Count - 1].MsDuration = msPositions["endOfPiece"] - section[section.Count - 1].MsPosition;
            }
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

        #region old
        //private VoiceDef GetF2FinalePart2(PaletteDef f2FinalePalette2, ExpansionKrystal krystal, List<int> strandIndices, Dictionary<string, int> msPositions)
        //{
        //    VoiceDef f2FinalePart2 = new VoiceDef(f2FinalePalette2, krystal);

        //    List<int> f2eStrandDurations = GetStrandDurations(f2FinalePart2, strandIndices);

        //    int extraTime = 500;
        //    int diff = extraTime / f2FinalePart2.Count;
        //    for(int i = f2FinalePart2.Count - 1; i > 0; --i)
        //    {
        //        if(strandIndices.Contains(i))
        //        {
        //            UniqueMidiRestDef umrd = new UniqueMidiRestDef(f2FinalePart2[i].MsPosition, f2eStrandDurations[strandIndices.IndexOf(i)] + extraTime);
        //            extraTime -= diff;
        //            f2FinalePart2.Insert(i, umrd);
        //        }
        //    }

        //    f2FinalePart2.StartMsPosition = msPositions["furies2FinalePart2Start"];
        //    f2FinalePart2.RemoveBetweenMsPositions(msPositions["postlude"], int.MaxValue);

        //    if(f2FinalePart2[f2FinalePart2.Count - 1] is UniqueMidiRestDef)
        //    {
        //        f2FinalePart2[f2FinalePart2.Count - 1].MsDuration = msPositions["postlude"] - f2FinalePart2[f2FinalePart2.Count - 1].MsPosition;
        //    }

        //    return f2FinalePart2;
        //}

        //private VoiceDef GetF2Postlude(PaletteDef f2PostludePalette, ExpansionKrystal krystal, List<int> strandIndices, Dictionary<string, int> msPositions)
        //{
        //    VoiceDef f2p = new VoiceDef(100);
        //    //VoiceDef f2p = new VoiceDef(f2PostludePalette, krystal);

        //    //List<int> f2eStrandDurations = GetStrandDurations(f2p, strandIndices);

        //    //for(int i = f2p.Count - 1; i > 0; --i)
        //    //{
        //    //    if(strandIndices.Contains(i))
        //    //    {
        //    //        UniqueMidiRestDef umrd = new UniqueMidiRestDef(f2p[i].MsPosition, f2eStrandDurations[strandIndices.IndexOf(i)] / 4);
        //    //        f2p.Insert(i, umrd);
        //    //    }
        //    //}

        //    //f2p.StartMsPosition = msPositions["postlude"];
        //    //f2p.RemoveBetweenMsPositions(msPositions["endOfPiece"], int.MaxValue);

        //    return f2p;
        //}
        #endregion

        internal void AdjustAlignments(Furies1 f1, Furies4 f4, Clytemnestra c)
        {
            AlignObjectAtIndex(58, 85, 100, f1[73].MsPosition);
            AlignObjectAtIndex(85, 100, 106, c[204].MsPosition);
            AlignObjectAtIndex(100, 106, 125, c[216].MsPosition);

            AlignObjectAtIndex(106, 125, 129, c[255].MsPosition);
            AlignObjectAtIndex(125, 129, 131, f1[115].MsPosition);
            AlignObjectAtIndex(129, 131, 135, c[268].MsPosition);
            AlignObjectAtIndex(131, 135, 141, f1[122].MsPosition);
            AlignObjectAtIndex(135, 141, 157, f1[123].MsPosition);
            AlignObjectAtIndex(141, 157, 164, f1[138].MsPosition);
            AlignObjectAtIndex(157, 164, 169, f4[46].MsPosition);
            AlignObjectAtIndex(164, 169, 214, f4[47].MsPosition);

            AlignObjectAtIndex(169, 214, 217, c[269].MsPosition);
            AlignObjectAtIndex(214, 217, 219, c[277].MsPosition);
            AlignObjectAtIndex(217, 219, 229, c[278].MsPosition);
            AlignObjectAtIndex(219, 229, 232, c[287].MsPosition);

            AlignObjectAtIndex(229, 232, 233, c[288].MsPosition);
            AlignObjectAtIndex(232, 233, 256, c[289].MsPosition);
            AlignObjectAtIndex(233, 256, this.Count - 2, f1[248].MsPosition);
        }

        internal void AdjustVelocities(Dictionary<string, int> msPositions)
        {
            AdjustVelocitiesHairpin(msPositions["furies2FinaleStart"], this[82].MsPosition, 0.2, 0.6);
            AdjustVelocitiesHairpin(this[82].MsPosition, msPositions["verse4"], 0.6, 0.2);
            AdjustVelocitiesHairpin(msPositions["verse4"], msPositions["furies2FinalePart2Start"], 0.2, 0.2);
            AdjustVelocitiesHairpin(msPositions["furies2FinalePart2Start"], msPositions["interlude4"], 0.2, 0.4);

            AdjustVelocitiesHairpin(msPositions["interlude4"], msPositions["verse5"], 0.5, 1.0);
            AdjustVelocitiesHairpin(msPositions["verse5"], msPositions["postlude"], 0.5, 0.5);
            AdjustVelocitiesHairpin(msPositions["postlude"], EndMsPosition, 0.5, 0.85);

            // example code from furies1

            //AdjustVelocitiesHairpin(msPositions["interlude3"], this[102].MsPosition, 1.0, 0.35);
            //AdjustVelocitiesHairpin(this[102].MsPosition, msPositions["interlude4"], 0.35, 0.7);
            //AdjustVelocitiesHairpin(msPositions["interlude4"], msPositions["verse5"], 0.7, 1.0);
            //AdjustVelocitiesHairpin(msPositions["verse5"], msPositions["postlude"], 0.5, 0.5);
            //AdjustVelocitiesHairpin(msPositions["postlude"], EndMsPosition, 0.8, 1.0);
        }

        internal void AdjustPostludePan(int postludeMsPosition)
        {
            double posDiff = ((double)(EndMsPosition - postludeMsPosition)) / 4;
            int postludeMsPosition1 = postludeMsPosition + (int)posDiff;
            int postludeMsPosition2 = postludeMsPosition + (int)(posDiff * 2);
            int postludeMsPosition3 = postludeMsPosition + (int)(posDiff * 3);

            SetPanGliss(postludeMsPosition, postludeMsPosition1, 15, 96);
            SetPanGliss(postludeMsPosition1, postludeMsPosition2, 96, 64);
            SetPanGliss(postludeMsPosition2, postludeMsPosition3, 64, 127);
            SetPanGliss(postludeMsPosition3, EndMsPosition, 127, 0);

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