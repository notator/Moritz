using System.Collections.Generic;
using System.Diagnostics;

using Krystals4ObjectLibrary;
using Moritz.Algorithm;
using Moritz.Palettes;
using Moritz.Spec;

namespace Moritz.Algorithm.SongSix
{
    internal class Furies2 : Trk
    {
        internal Furies2(byte midiChannel, int msDuration)
			: base(midiChannel, msDuration)
        {
        }

        #region before Interlude3
        internal void GetBeforeInterlude3(Clytemnestra clytemnestra, Trk wind1, Trk furies3, List<Palette> _palettes)
        {
            GetFuries2Interlude2(clytemnestra, wind1, furies3);
            AddFuries2ChirpsForInterlude2AndVerse3(clytemnestra, wind1, _palettes[7]);
        }
        /// <summary>
        /// Steals the ticks from furies 3, then agglommerates the remaining rests in furies3...
        /// </summary>
        private void GetFuries2Interlude2(Clytemnestra clytemnestra, Trk wind1, Trk furies3)
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
                MidiChordDef ticksChord = furies3[f3Index] as MidiChordDef;
                Debug.Assert(ticksChord != null);
                RestDef ticksRest = new RestDef(ticksChord.MsPosition, ticksChord.MsDuration);
                furies3.Replace(f3Index, ticksRest);
                InsertInRest(ticksChord);
            }

            MidiChordDef lastTicksBeforeVerse3 = (MidiChordDef)((MidiChordDef)this[39]).DeepClone();
            lastTicksBeforeVerse3.MsPosition = furies3[155].MsPosition + furies3[155].MsDuration;
            lastTicksBeforeVerse3.MsDuration = clytemnestra[117].MsPosition - lastTicksBeforeVerse3.MsPosition;
            lastTicksBeforeVerse3.Transpose(10);
            InsertInRest(lastTicksBeforeVerse3);

            furies3.AgglomerateRests();
        }

        private void AddFuries2ChirpsForInterlude2AndVerse3(Clytemnestra clytemnestra, Trk wind1, Palette chirpsPalette)
        {
            int[] chirpIndices = { 4, 6, 10, 0, 1, 3, 5, 7, 9, 11 };
            int[] transpositions = { 2, 0, 4, 11, 5, 10, 6, 9, 7, 8 };
            double[] velocityfactors = { 0.32, 0.3, 0.35, 0.45, 0.36, 0.43, 0.37, 0.42, 0.39, 0.4 };
            IUniqueDef c3 = chirpsPalette.UniqueDurationDef(chirpIndices[3]);
            IUniqueDef c7 = chirpsPalette.UniqueDurationDef(chirpIndices[7]);
            int[] msPositions =
            { 
                this[2].MsPosition, 
                this[6].MsPosition, 
                this[16].MsPosition, 
                this[26].MsPosition,
                this[26].MsPosition + c3.MsDuration,
                clytemnestra[129].MsPosition,
                clytemnestra[143].MsPosition + 110,
                clytemnestra[156].MsPosition + 220,
                clytemnestra[156].MsPosition + 220 + c7.MsDuration,
                clytemnestra[168].MsPosition + 330,
            };
            for(int i = 9; i >=0 ; --i)
            {
                MidiChordDef cheep = chirpsPalette.MidiChordDef(chirpIndices[i]);
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

        internal void GetFinale(List<Palette> palettes, Dictionary<string, int> msPositions, Krystal krystal)
        {
            Trk furies2Finale = GetF2Finale(palettes, krystal, msPositions);

            if(furies2Finale[furies2Finale.Count - 1] is RestDef)
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

        private List<int> GetStrandIndices(Krystal krystal)
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

        private Trk GetF2Finale(List<Palette> palettes, Krystal krystal, Dictionary<string, int> msPositions)
        {
            Palette f2FinalePalette1 = palettes[11];
            Palette f2FinalePalette2 = palettes[15];
            Palette f2PostludePalette = palettes[19];

            List<int> strandIndices = GetStrandIndices(krystal);

            Trk finalePart1 = f2FinalePalette1.NewTrkDef(this.MidiChannel, krystal);
            Transform(finalePart1, msPositions, strandIndices);
			Trk finalePart2 = f2FinalePalette2.NewTrkDef(this.MidiChannel, krystal);
            Transform(finalePart2, msPositions, strandIndices);
			Trk postlude = f2PostludePalette.NewTrkDef(this.MidiChannel, krystal);
            Transform(postlude, msPositions, strandIndices);

            Trk finale = GetFinaleSections(finalePart1, finalePart2, postlude, 71, 175);

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
        private Trk GetFinaleSections(Trk finalePart1, Trk finalePart2, Trk postlude, int part2Index, int postludeIndex)
        {
            List<IUniqueDef> iumdds = new List<IUniqueDef>();

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

            return new Trk(this.MidiChannel, iumdds);
        }

        private void Transform(Trk section, Dictionary<string, int> msPositions, List<int> strandIndices)
        {
            List<int> strandDurations = GetStrandDurations(section, strandIndices);

            int extraTime = 750;
            int diff = extraTime / section.Count;
            for(int i = section.Count - 1; i > 0; --i)
            {
                if(strandIndices.Contains(i))
                {
                    RestDef umrd = new RestDef(section[i].MsPosition, strandDurations[strandIndices.IndexOf(i)] + extraTime);
                    extraTime -= diff;
                    section.Insert(i, umrd);
                }
            }

            section.MsPosition = msPositions["furies2FinaleStart"];

            //double factor = 10;

            //section.AdjustMsDurations(factor);

            section.CreateAccel(0, section.Count, 0.25);

            //section.RemoveBetweenMsPositions(msPositions["interlude4End"], int.MaxValue);
            section.RemoveBetweenMsPositions(msPositions["finalWindChord"], int.MaxValue);

            if(section[section.Count - 1] is RestDef)
            {
                //section[section.Count - 1].MsDuration = msPositions["interlude4End"] - section[section.Count - 1].MsPosition;
                section[section.Count - 1].MsDuration = msPositions["endOfPiece"] - section[section.Count - 1].MsPosition;
            }
        }

        /// <summary>
        /// voiceDef contains the MidiChordDefs defined by a krystal, and nothing else.
        /// </summary>
        /// <param name="voiceDef"></param>
        /// <param name="strandIndices"></param>
        /// <returns></returns>
        private List<int> GetStrandDurations(Trk voiceDef, List<int> strandIndices)
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

        internal void AdjustAlignments(Furies1 f1, Furies4 f4, Clytemnestra c)
        {
            AlignObjectAtIndex(58, 67, 74, f1[56].MsPosition);
            AlignObjectAtIndex(67, 74, 85, f1[61].MsPosition);
            AlignObjectAtIndex(74, 85, 87, f1[73].MsPosition);
            AlignObjectAtIndex(85, 87, 90, c[174].MsPosition);
            AlignObjectAtIndex(87, 90, 91, c[184].MsPosition);
            AlignObjectAtIndex(90, 91, 100, c[185].MsPosition);
            AlignObjectAtIndex(91, 100, 106, c[204].MsPosition);
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
            AlignObjectAtIndex(229, 232, 233, c[288].MsPosition - 200);
            AlignObjectAtIndex(232, 233, 256, c[289].MsPosition);
            AlignObjectAtIndex(233, 256, this.Count - 2, f1[248].MsPosition);

            // final adjustments for R2M
            AlignObjectAtIndex(49,50,51, c[130].MsPosition - 100);

            AlignObjectAtIndex(78, 92, 93, c[184].MsPosition - 200);
            AlignObjectAtIndex(88, 89, 90, c[177].MsPosition);

            AlignObjectAtIndex(123, 124, 129, c[255].MsPosition);

            AlignObjectAtIndex(131, 158, 159, f1[138].MsPosition - 200);
            AlignObjectAtIndex(166, 168, 181, f4[47].MsPosition);
        }

        internal void AdjustVelocities(Dictionary<string, int> msPositions)
        {
            AdjustVelocitiesHairpin(msPositions["furies2FinaleStart"], this[82].MsPosition, 0.3, 0.6);
            AdjustVelocitiesHairpin(this[82].MsPosition, msPositions["verse4"], 0.6, 0.3);
            AdjustVelocitiesHairpin(msPositions["verse4"], msPositions["furies2FinalePart2Start"], 0.2, 0.2);
            AdjustVelocitiesHairpin(msPositions["furies2FinalePart2Start"], msPositions["interlude4"], 0.2, 0.4);
            AdjustVelocitiesHairpin(msPositions["interlude4"], msPositions["verse5"], 0.5, 1.0);
            AdjustVelocitiesHairpin(msPositions["verse5"], msPositions["postlude"], 0.5, 0.5);
            //AdjustVelocitiesHairpin(msPositions["postlude"], EndMsPosition, 0.5, 0.85);
            AdjustVelocitiesHairpin(msPositions["postlude"], msPositions["postludeDiminuendo"], 0.5, 0.85);
            AdjustVelocitiesHairpin(msPositions["postludeDiminuendo"], EndMsPosition, 0.5, 0.2);

            // example code from furies1
            //AdjustVelocitiesHairpin(msPositions["interlude3"], this[102].MsPosition, 1.0, 0.35);
            //AdjustVelocitiesHairpin(this[102].MsPosition, msPositions["interlude4"], 0.35, 0.7);
            //AdjustVelocitiesHairpin(msPositions["interlude4"], msPositions["verse5"], 0.7, 1.0);
            //AdjustVelocitiesHairpin(msPositions["verse5"], msPositions["postlude"], 0.5, 0.5);
            //AdjustVelocitiesHairpin(msPositions["postlude"], EndMsPosition, 0.8, 1.0);
        }

        /// <summary>
        /// Motion is approximately contrary to the pan gliss in furies 1
        /// </summary>
        internal void AdjustPostludePan(int postludeMsPosition, int postludeMsPosition1, int postludeMsPosition2, int postludeMsPosition3)
        {
            SetPanGliss(postludeMsPosition, postludeMsPosition1, 20, 69);
            SetPanGliss(postludeMsPosition1, postludeMsPosition2, 69, 35);
            SetPanGliss(postludeMsPosition2, postludeMsPosition3, 35, 127);
            SetPanGliss(postludeMsPosition3, EndMsPosition, 127, 0);
        }

        #endregion finale
    }
}