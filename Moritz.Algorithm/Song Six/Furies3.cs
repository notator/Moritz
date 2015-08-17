﻿using System.Collections.Generic;
using System.Diagnostics;

using Krystals4ObjectLibrary;
using Moritz.Algorithm;
using Moritz.Palettes;
using Moritz.Spec;

namespace Moritz.Algorithm.SongSix
{
    internal class Furies3 : TrkDef
    {
        internal Furies3(int msDuration)
            : base(msDuration)
        {
        }

        #region before Interlude3
        internal void GetBeforeInterlude3(int firstRestMsDuration, Clytemnestra clytemnestra, TrkDef wind1, List<Palette> palettes)
        {
            GetFlutters(firstRestMsDuration, palettes[2]);

            AddTicks(palettes[4]);

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

        private void GetFlutters(int firstRestMsDuration, Palette palette)
        {
            // each flutter begins with a chord, and ends with a rest.
            TrkDef furies3FlutterSequence1 = GetFlutter1(palette);
            furies3FlutterSequence1.AdjustVelocities(0.7);

            TrkDef furies3FlutterSequence2 = GetNextFlutterSequence(furies3FlutterSequence1, 0.89, 1);
            TrkDef furies3FlutterSequence3 = GetNextFlutterSequence(furies3FlutterSequence2, 0.89, 1);
            TrkDef furies3FlutterSequence4 = GetNextFlutterSequence(furies3FlutterSequence3, 0.89, 1);
            TrkDef furies3FlutterSequence5 = GetNextFlutterSequence(furies3FlutterSequence4, 0.89, 1);
            TrkDef furies3FlutterSequence6 = GetNextFlutterSequence(furies3FlutterSequence5, 0.89, 2);
            TrkDef furies3FlutterSequence7 = GetNextFlutterSequence(furies3FlutterSequence6, 0.89, 2);
            TrkDef furies3FlutterSequence8 = GetNextFlutterSequence(furies3FlutterSequence7, 0.89, 2);
            TrkDef furies3FlutterSequence9 = GetNextFlutterSequence(furies3FlutterSequence8, 0.89, 3);
            TrkDef furies3FlutterSequence10 = GetNextFlutterSequence(furies3FlutterSequence9, 0.89, 3);
            TrkDef furies3FlutterSequence11 = GetNextFlutterSequence(furies3FlutterSequence10, 0.89, 4);
            TrkDef furies3FlutterSequence12 = GetNextFlutterSequence(furies3FlutterSequence11, 0.89, 5);

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

            this._uniqueDefs = f3.UniqueDefs;
        }

        private TrkDef GetNextFlutterSequence(TrkDef existingFlutter, double factor, int transposition)
        {
            TrkDef nextFlutter = existingFlutter.DeepClone();
            nextFlutter.AdjustVelocities(factor);
            nextFlutter.AdjustMsDurations(factor);
            nextFlutter.AdjustRestMsDurations(factor);
            nextFlutter.Transpose(transposition);
            return nextFlutter;
        }

        private TrkDef GetFlutter1(Palette palette)
        {
            List<IUniqueDef> flutter1 = new List<IUniqueDef>();
            int msPosition = 0;

            for(int i = 0; i < 7; ++i)
            {
                int[] contour = K.Contour(7, 11, 7);
                IUniqueDef flutter = palette.UniqueDurationDef(contour[i] - 1);
                flutter.MsPosition = msPosition;
                msPosition += flutter.MsDuration;
                flutter1.Add(flutter);

                if(i != 3 && i != 5)
                {
                    RestDef rest = new RestDef(msPosition, flutter.MsDuration);
                    msPosition += rest.MsDuration;
                    flutter1.Add(rest);
                }
            }

            TrkDef furies3FlutterSequence1 = new TrkDef(flutter1);

            return furies3FlutterSequence1;
        }

        /// <summary>
        /// These ticks are "stolen" by Furies2 later.
        /// </summary>
        /// <param name="furies3"></param>
        /// <param name="ticksPalette"></param>
        private void AddTicks(Palette ticksPalette)
        {
            List<int> TickInsertionIndices = new List<int>()
            {
                66,69,72,78,81,84,87,
                89,92,95,99,102,105,109,
                112,115,119,122,125,129,132
            };

            List<IUniqueDef> ticksList = GetTicksSequence(ticksPalette);

            Debug.Assert(TickInsertionIndices.Count == ticksList.Count); // 21 objects

            for(int i = ticksList.Count-1; i >= 0; --i)
            {
                Insert(TickInsertionIndices[i], ticksList[i]);
            }
        }

        private List<IUniqueDef> GetTicksSequence(Palette ticksPalette)
        {
            List<IUniqueDef> ticksSequence = new List<IUniqueDef>();
            int msPosition = 0;
            int[] transpositions = { 12, 14, 17 };

            for(int i = 0; i < 3; ++i)
            {
                int[] contour = K.Contour(7, 4, 10 - i);
                for(int j = 6; j >= 0; --j)
                {
                    IUniqueDef ticks = ticksPalette.UniqueDurationDef(contour[j] - 1);
                    ticks.MsPosition = msPosition;
                    msPosition += ticks.MsDuration;
                    MidiChordDef iumdd = ticks as MidiChordDef;
                    if(iumdd != null)
                    {
                        iumdd.Transpose(transpositions[i] + contour[j]);
                        iumdd.AdjustVelocities(0.6);
                    }

                    ticksSequence.Add(ticks);
                }
            }

            return ticksSequence;
        }

        internal void GetChirpsInInterlude2AndVerse3(TrkDef furies1, TrkDef furies2, Clytemnestra clytemnestra, TrkDef wind1, Palette chirpsPalette)
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
                MidiChordDef cheep = chirpsPalette.MidiChordDef(chirpIndices[i]);
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

        internal void GetFinale(List<Palette> palettes, Dictionary<string, int> msPositions, Krystal krystal)
        {
            TrkDef furies3Finale = GetF3Finale(palettes, krystal, msPositions);

            InsertInRest(furies3Finale);

            AdjustPitchWheelDeviations(msPositions["interlude4"], msPositions["endOfPiece"], 5, 28);
        }

        private TrkDef GetF3Finale(List<Palette> palettes, Krystal krystal, Dictionary<string, int> msPositions)
        {
            Palette f3FinalePalette1 = palettes[10]; // correct 1.1.2014
            Palette f3FinalePalette2 = palettes[14];
            Palette f3PostludePalette = palettes[18];

            List<int> strandIndices = GetStrandIndices(krystal);

            TrkDef finalePart1 = f3FinalePalette1.NewTrkDef(krystal);
            Transform(finalePart1, msPositions, strandIndices);
            TrkDef finalePart2 = f3FinalePalette2.NewTrkDef(krystal);
            Transform(finalePart2, msPositions, strandIndices);
            TrkDef postlude = f3PostludePalette.NewTrkDef(krystal);
            Transform(postlude, msPositions, strandIndices);

            TrkDef finale = GetFinaleSections(finalePart1, finalePart2, postlude, 77, 206);

            Cleanup(finale, msPositions["endOfPiece"]);

            return finale;
        }

        /// <summary>
        /// ACHTUNG: could be a protected virtual function in a furies class
        /// </summary>
        private void Transform(TrkDef section, Dictionary<string, int> msPositions, List<int> strandIndices)
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

            section.StartMsPosition = msPositions["furies3FinaleStart"];

            #region old
            //double factor = 10;
            //section.AdjustMsDurations(factor);
            #endregion

            //section.CreateAccel(0, section.Count, 0.25);
            section.CreateAccel(0, section.Count, 0.13);

            //section.RemoveBetweenMsPositions(msPositions["interlude4End"], int.MaxValue);
            section.RemoveBetweenMsPositions(msPositions["finalWindChord"], int.MaxValue);

            if(section[section.Count - 1] is RestDef)
            {
                //section[section.Count - 1].MsDuration = msPositions["interlude4End"] - section[section.Count - 1].MsPosition;
                section[section.Count - 1].MsDuration = msPositions["endOfPiece"] - section[section.Count - 1].MsPosition;
            }
        }

        #region These could be protected functions in a furies class
        /// <summary>
        /// Could be a protected function in a furies class
        /// </summary>
        private void Cleanup(TrkDef finale, int endOfPieceMsPosition)
        {
            if(finale[finale.Count - 1] is RestDef)
            {
                finale.RemoveAt(finale.Count - 1);
            }

            if(finale[finale.Count - 1].MsPosition + finale[finale.Count - 1].MsDuration > endOfPieceMsPosition)
            {
                finale.RemoveAt(finale.Count - 1);
            }

        }

        /// <summary>
        /// Could be a protected function in a furies class
        /// </summary>
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

        /// <summary>
        /// ACHTUNG: this function should be in a furies class. It is used by furies 2 and furies 4 (probably furies 3 too!)
        /// The three argument VoiceDefs are parallel. They have the same number of DurationDefs, each having the same MsPosition
        /// and MsDuration. The DurationDefs come from different palettes, so can otherwise have different parameters.
        /// This function simply creates a new VoiceDef by selecting the apropriate DurationDefs from each VoiceDef argument.
        /// </summary>
        private TrkDef GetFinaleSections(TrkDef finalePart1, TrkDef finalePart2, TrkDef postlude, int part2Index, int postludeIndex)
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

            return new TrkDef(iumdds);
        }

        /// <summary>
        /// ACHTUNG: could be a protected functions in a furies class
        /// voiceDef contains the MidiChordDefs defined by a krystal, and nothing else.
        /// </summary>
        /// <param name="voiceDef"></param>
        /// <param name="strandIndices"></param>
        /// <returns></returns>
        private List<int> GetStrandDurations(TrkDef voiceDef, List<int> strandIndices)
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
        #endregion

        internal void AdjustAlignments(Furies1 f1, Furies2 f2, Furies4 f4, Clytemnestra c, TrkDef wind1)
        {
            AlignObjectAtIndex(147, 150, 158, f1[56].MsPosition);
            AlignObjectAtIndex(150, 158, 170, f1[61].MsPosition);
            AlignObjectAtIndex(158, 170, 175, c[174].MsPosition);
            AlignObjectAtIndex(170, 176, 183, c[184].MsPosition);
            AlignObjectAtIndex(175, 183, 212, c[196].MsPosition);
            AlignObjectAtIndex(183, 212, 217, c[242].MsPosition);
            AlignObjectAtIndex(212, 217, 218, c[254].MsPosition);
            AlignObjectAtIndex(217, 218, 224, c[259].MsPosition);
            AlignObjectAtIndex(218, 224, 290, wind1[57].MsPosition);
            AlignObjectAtIndex(224, 290, 296, f4[49].MsPosition);
            AlignObjectAtIndex(290, 296, 318, wind1[66].MsPosition);
            AlignObjectAtIndex(296, 318, 344, f4[53].MsPosition);
            AlignObjectAtIndex(318, 344, 350, c[283].MsPosition);
            AlignObjectAtIndex(344, 350, 353, c[287].MsPosition);
            AlignObjectAtIndex(350, 353, 354, c[288].MsPosition - 200);
            AlignObjectAtIndex(353, 354, 390, f4[59].MsPosition);
            AlignObjectAtIndex(354, 390, 401, f4[69].MsPosition);

            // final adjustments for R2M
            AlignObjectAtIndex(139, 140, 141, c[119].MsPosition - 200);

            AlignObjectAtIndex(141, 142, 143, c[140].MsPosition - 100);


            AlignObjectAtIndex(143, 144, 145, c[152].MsPosition - 200);
            AlignObjectAtIndex(145, 146, 147, c[173].MsPosition - 200);
            AlignObjectAtIndex(146, 147, 152, f2[64].MsPosition);

            AlignObjectAtIndex(147, 151, 152, f1[56].MsPosition - 100);

            AlignObjectAtIndex(152, 159, 160, f1[61].MsPosition - 100);

            AlignObjectAtIndex(160, 171, 172, c[174].MsPosition - 600);
            AlignObjectAtIndex(172, 176, 177, c[184].MsPosition - 200);
            AlignObjectAtIndex(212, 217, 218, c[254].MsPosition - 200);
            AlignObjectAtIndex(275, 291, 292, f4[49].MsPosition - 200);



            // example code from furies2
            //AlignObjectAtIndex(58, 85, 100, f1[73].MsPosition);
            //AlignObjectAtIndex(85, 100, 106, c[204].MsPosition);
            //AlignObjectAtIndex(100, 106, 125, c[216].MsPosition);
            //AlignObjectAtIndex(106, 125, 129, c[255].MsPosition);
            //AlignObjectAtIndex(125, 129, 131, f1[115].MsPosition);
            //AlignObjectAtIndex(129, 131, 135, c[268].MsPosition);
            //AlignObjectAtIndex(131, 135, 141, f1[122].MsPosition);
            //AlignObjectAtIndex(135, 141, 157, f1[123].MsPosition);
            //AlignObjectAtIndex(141, 157, 164, f1[138].MsPosition);
            //AlignObjectAtIndex(157, 164, 169, f4[46].MsPosition);
            //AlignObjectAtIndex(164, 169, 214, f4[47].MsPosition);
            //AlignObjectAtIndex(169, 214, 217, c[269].MsPosition);
            //AlignObjectAtIndex(214, 217, 219, c[277].MsPosition);
            //AlignObjectAtIndex(217, 219, 229, c[278].MsPosition);
            //AlignObjectAtIndex(219, 229, 232, c[287].MsPosition);
            //AlignObjectAtIndex(229, 232, 233, c[288].MsPosition);
            //AlignObjectAtIndex(232, 233, 256, c[289].MsPosition);
            //AlignObjectAtIndex(233, 256, this.Count - 2, f1[248].MsPosition);
        }
        internal void AdjustVelocities(Dictionary<string, int> msPositions)
        {
            //AdjustVelocitiesHairpin(msPositions["furies3FinaleStart"], this[165].MsPosition, 0.4, 0.6);
            AdjustVelocitiesHairpin(this[147].MsPosition, this[165].MsPosition, 0.4, 0.6);
            AdjustVelocitiesHairpin(this[165].MsPosition, msPositions["verse4"], 0.6, 0.3);
            AdjustVelocitiesHairpin(msPositions["verse4"], msPositions["furies2FinalePart2Start"], 0.3, 0.3);
            AdjustVelocitiesHairpin(msPositions["furies2FinalePart2Start"], msPositions["interlude4"], 0.3, 0.4);
            AdjustVelocitiesHairpin(msPositions["interlude4"], msPositions["verse5"], 0.5, 1.0);
            AdjustVelocitiesHairpin(msPositions["verse5"], msPositions["postlude"], 0.5, 0.5);
            //AdjustVelocitiesHairpin(msPositions["postlude"], EndMsPosition, 0.5, 0.85);

            AdjustVelocitiesHairpin(msPositions["postlude"], msPositions["postludeDiminuendo"], 0.5, 0.85);
            AdjustVelocitiesHairpin(msPositions["postludeDiminuendo"], EndMsPosition, 0.5, 0.2);

            // example code from furies2
            //AdjustVelocitiesHairpin(msPositions["furies2FinaleStart"], this[82].MsPosition, 0.2, 0.6);
            //AdjustVelocitiesHairpin(this[82].MsPosition, msPositions["verse4"], 0.6, 0.2);
            //AdjustVelocitiesHairpin(msPositions["verse4"], msPositions["furies2FinalePart2Start"], 0.2, 0.2);
            //AdjustVelocitiesHairpin(msPositions["furies2FinalePart2Start"], msPositions["interlude4"], 0.2, 0.4);
            //AdjustVelocitiesHairpin(msPositions["interlude4"], msPositions["verse5"], 0.5, 1.0);
            //AdjustVelocitiesHairpin(msPositions["verse5"], msPositions["postlude"], 0.5, 0.5);
            //AdjustVelocitiesHairpin(msPositions["postlude"], EndMsPosition, 0.5, 0.85);
        }
        /// <summary>
        /// Motion is contrary to the pan gliss in furies 2
        /// </summary>
        internal void AdjustPostludePan(int postludeMsPosition, int postludeMsPosition1, int postludeMsPosition2, int postludeMsPosition3)
        {
            SetPanGliss(postludeMsPosition, postludeMsPosition1, 69, 20);
            SetPanGliss(postludeMsPosition1, postludeMsPosition2, 35, 69);
            SetPanGliss(postludeMsPosition2, postludeMsPosition3, 127, 35);
            SetPanGliss(postludeMsPosition3, EndMsPosition, 0, 127);
        }
        #endregion finale
    }
}
