﻿using System.Collections.Generic;
using System.Diagnostics;

using Krystals4ObjectLibrary;
using Moritz.Algorithm;
using Moritz.Palettes;
using Moritz.Spec;

namespace Moritz.Algorithm.SongSix
{
    internal class Furies4 : Trk
    {
		/// <summary>
		/// A VoiceDef beginning at MsPosition = 0, and containing a single RestDef having msDuration
		/// </summary>
		/// <param name="msDuration"></param>
		internal Furies4(int midiChannel, int msDuration)
			: base(midiChannel)
        {
			RestDef lmRestDef = new RestDef(0, msDuration);
			_uniqueDefs.Add(lmRestDef);
		}

		internal void GetBeforeInterlude3(int firstRestMsDuration, Clytemnestra clytemnestra, Trk wind1, List<Palette> palettes)
        {
            GetSnores(firstRestMsDuration, clytemnestra, wind1, palettes[1]);
            AddGrowlsToInterlude2AndVerse3(clytemnestra, palettes[3]);
        }

        #region before Interlude3
        private void AddGrowlsToInterlude2AndVerse3(Clytemnestra clytemnestra, Palette growlsPalette)
        {
            int[] growlIndices = { 0,2,5,1 };
            //int[] transpositions = { 0,0,0,0 };
            //double[] velocityfactors = { 1,1,1,1 };
            int[] msPositions =
            { 
                this[24].MsPositionReTrk + 200, 
                this[26].MsPositionReTrk + 200, 
                this[30].MsPositionReTrk + 200, 
                this[32].MsPositionReTrk + 200, 
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
                MidiChordDef growl = growlsPalette.UniqueDurationDef(growlIndices[i]) as MidiChordDef;
                Debug.Assert(growl != null);
                growl.MsPositionReTrk = msPositions[i];
                growl.MsDuration = msDurations[i];
                //growl.AdjustVelocities(velocityfactors[i]);
                //growl.Transpose(transpositions[i]);
                InsertInRest(growl);
            }

            AgglomerateRestOrChordAt(40);

            AlignObjectAtIndex(34, 35, 36, clytemnestra[140].MsPositionReTrk);
            AlignObjectAtIndex(35, 36, 37, clytemnestra[141].MsPositionReTrk);
            AlignObjectAtIndex(38, 39, 40, clytemnestra[162].MsPositionReTrk);
        }

        private void GetSnores(int firstRestMsDuration, Clytemnestra clytemnestra, Trk wind1, Palette snoresPalette)
        {
            List<IUniqueDef> snores = new List<IUniqueDef>();
            int msPosition = 0;

            IUniqueDef firstRest = new RestDef(msPosition, firstRestMsDuration);
            snores.Add(firstRest);
            msPosition += firstRestMsDuration;

            #region prelude + verse1
            int[] transpositions1 = { 0, 0, 0, 0, 0, 1, 0 };
            for(int i = 0; i < 7; ++i)
            {
                IUniqueDef snore = snoresPalette.UniqueDurationDef(i);
                snore.MsPositionReTrk = msPosition;
                msPosition += snore.MsDuration;
                MidiChordDef iumdd = snore as MidiChordDef;
                if(iumdd != null)
                {
                    iumdd.Transpose(transpositions1[i]);
                    iumdd.PitchWheelDeviation = 3;
                }
                snores.Add(snore);

                RestDef rest = new RestDef(msPosition, 2500);
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
                IUniqueDef snore = snoresPalette.UniqueDurationDef(i / 2);
                snore.MsPositionReTrk = msPosition;
                factor = factors[i];
                msDuration = snore.MsDuration * factor;
                snore.MsDuration = (int)msDuration;
                msPosition += snore.MsDuration;
                MidiChordDef iumdd = snore as MidiChordDef;
                if(iumdd != null)
                {
                    iumdd.Transpose(transpositions2[i]);
                    iumdd.PitchWheelDeviation = 20;
                }
                //iumdd.MidiVelocity = (byte)((double)snore.MidiVelocity * factor * factor);
                snores.Add(snore);

                restDuration = 2500 / factor;
                RestDef rest = new RestDef(msPosition, (int)restDuration);
                msPosition += rest.MsDuration;
                snores.Add(rest);
            }

            snores[snores.Count - 1].MsDuration = clytemnestra.EndMsPosition - snores[snores.Count - 1].MsPositionReTrk;

            this._uniqueDefs = snores;

            AdjustVelocitiesHairpin(13, Count, 0.25);

            #region alignments before Interlude3
            AlignObjectAtIndex(7, 8, 9, clytemnestra[3].MsPositionReTrk);
            AlignObjectAtIndex(8, 9, 10, clytemnestra[7].MsPositionReTrk);
            AlignObjectAtIndex(9, 10, 11, clytemnestra[16].MsPositionReTrk);
            AlignObjectAtIndex(10, 11, 12, clytemnestra[24].MsPositionReTrk);
            AlignObjectAtIndex(11, 12, 13, clytemnestra[39].MsPositionReTrk);
            AlignObjectAtIndex(12, 13, 14, clytemnestra[42].MsPositionReTrk);
            AlignObjectAtIndex(14, 34, Count, wind1[38].MsPositionReTrk); // rest at start of Interlude3
            #endregion

            RemoveScorePitchWheelCommands(0, 13); // pitchwheeldeviation is 20 for R2M
        }
        #endregion before Interlude3

        #region finale
        internal void GetFinale(List<Palette> palettes, Dictionary<string, int> msPositions, Krystal krystal)
        {
            Trk furies4Finale = GetF4Finale(palettes, krystal, msPositions);

            if(furies4Finale[furies4Finale.Count - 1] is RestDef)
            {
                furies4Finale.RemoveAt(furies4Finale.Count - 1);
            }

            if(furies4Finale[furies4Finale.Count - 1].MsPositionReTrk + furies4Finale[furies4Finale.Count - 1].MsDuration > msPositions["endOfPiece"])
            {
                furies4Finale.RemoveAt(furies4Finale.Count - 1);
            }

            InsertInRest(furies4Finale);

            RemoveScorePitchWheelCommandsFromControlledChords(); // interlude4 (to immediately before verse5)

            AdjustPitchWheelDeviations(msPositions["verse5"], msPositions["postlude"], 5, 1);
            RemoveScorePitchWheelCommands(59, this.Count); // postlude           
        }

        private void RemoveScorePitchWheelCommandsFromControlledChords()
        {
            RemoveScorePitchWheelCommands(42, 43);
            RemoveScorePitchWheelCommands(45, 46);
            RemoveScorePitchWheelCommands(47, 48);
            RemoveScorePitchWheelCommands(49, 50);
            RemoveScorePitchWheelCommands(52, 53);
        }

        private Trk GetF4Finale(List<Palette> palettes , Krystal krystal, Dictionary<string, int> msPositions)
        {
            Palette f4FinalePalette1 = palettes[9];
            Palette f4FinalePalette2 = palettes[13];
            Palette f4PostludePalette = palettes[17];

			Trk interlude4Start = f4FinalePalette1.NewTrk(this.MidiChannel, krystal);
            Transform(interlude4Start, msPositions);
			Trk interlude4EndVerse5 = f4FinalePalette2.NewTrk(this.MidiChannel, krystal);
            Transform(interlude4EndVerse5, msPositions);
			Trk postlude = f4PostludePalette.NewTrk(this.MidiChannel, krystal);
            Transform(postlude, msPositions);

            Trk finale = GetFinaleSections(interlude4Start, interlude4EndVerse5, postlude, 7, 17);

            return finale;
        }

        /// <summary>
        /// ACHTUNG: this function should be in a furies class. It is used by furies 2 and furies 4 (probably furies 3 too!)
        /// The three VoiceDefs are parallel. They have the same number of DurationDefs, each having the same MsPosition and
        /// MsDuration. The DurationDefs come from different palettes, so use different pitches etc.
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

        private void Transform(Trk section, Dictionary<string, int> msPositions)
        {
            section.RemoveRange(40, section.Count - 40);

            section.MsPositionReSeq = msPositions["interlude4"];

            //double factor = 10;

            //section.AdjustMsDurations(factor);

            section.CreateAccel(0, section.Count, 0.08);

            //section.RemoveBetweenMsPositions(msPositions["interlude4End"], int.MaxValue);
            section.RemoveBetweenMsPositions(msPositions["finalWindChord"], int.MaxValue);

            if(section[section.Count - 1] is RestDef)
            {
                //section[section.Count - 1].MsDuration = msPositions["interlude4End"] - section[section.Count - 1].MsPosition;
                section[section.Count - 1].MsDuration = msPositions["endOfPiece"] - section[section.Count - 1].MsPositionReTrk;
            }
        }

        internal void AdjustAlignments(Furies1 furies1, Clytemnestra clytemnestra, Trk wind3)
        {
            AlignObjectAtIndex(42, Count-1, Count, furies1[280].MsPositionReTrk);

            AlignObjectAtIndex(42, 43, 45, furies1[126].MsPositionReTrk);
            AlignObjectAtIndex(43, 45, 49, furies1[138].MsPositionReTrk);
            AlignObjectAtIndex(45, 49, 59, furies1[165].MsPositionReTrk);
            AlignObjectAtIndex(49, 59, 69, furies1[212].MsPositionReTrk);

            AlignObjectAtIndex(59, 69, Count-1, furies1[248].MsPositionReTrk);
        }

        internal void AdjustVelocities(Dictionary<string, int> msPositions)
        {
            AdjustVelocitiesHairpin(msPositions["interlude4"], msPositions["verse5"], 0.5, 0.8);
            AdjustVelocitiesHairpin(msPositions["verse5"], msPositions["postlude"], 0.5, 0.5);

            //AdjustVelocitiesHairpin(msPositions["postlude"], EndMsPosition, 0.4, 1.0);

            AdjustVelocitiesHairpin(msPositions["postlude"], msPositions["postludeDiminuendo"], 0.3, 0.8);
            AdjustVelocitiesHairpin(msPositions["postludeDiminuendo"], EndMsPosition, 0.8, 0.4);
        }

        /// <summary>
        /// Returns a dictionary containing the current transposition per msPosition
        /// ( Dictionary[msPositon, transposition] )
        /// </summary>
        /// <returns></returns>
        public Dictionary<int, int> SetFinalMelody(Krystal mod7krys, Krystal mod12krys)
        {
            int f4Interlude4Index = 42;
            int f4PostludeIndex = 59;
            int f4finalPhaseIndex = 73;

            List<int> mod7Values = mod7krys.GetValues(1)[0];
            List<int> mod12Values = mod12krys.GetValues(1)[0];

            // circle of fifths hierarchy (mod 12): 0 7 4 2 10 9 3 4 8 11 1 6
            // rearranged for mod7krys hierarchy (4536271)
            int[] transpositionArray1 = { 3, -2, 4, 0, 4, 2, -3 }; // 7 changed to 4 (was too high!)
            // circle of fifths hierarchy centred on 0:
            int[] transpositionArray2 = { 0, 4, 4, 2, -2, -3, 3, 4, -4, -1, 1, 6 }; // 7 changed to 4 (was too high!)
            // widened
            int[] transpositionArray3 = { 0, 9, 4, 4, -2, -5, 6, 8, -7, -2, 2, 11 };

            Dictionary<int, int> msPosTranspositionDict = new Dictionary<int, int>();

            int transposition;
            int valueIndex = 0;
            for(int i = f4Interlude4Index; i < f4PostludeIndex; ++i)
            {
                MidiChordDef umcd = this[i] as MidiChordDef;
                if(umcd != null)
                {
                    transposition = transpositionArray1[mod7Values[valueIndex++] - 1];
                    umcd.Transpose(transposition);
                    msPosTranspositionDict.Add(umcd.MsPositionReTrk, transposition);
                }
            }
            valueIndex = 0;
            for(int i = f4PostludeIndex; i < f4finalPhaseIndex; ++i)
            {
                MidiChordDef umcd = this[i] as MidiChordDef;
                if(umcd != null)
                {
                    transposition = transpositionArray2[mod12Values[valueIndex++] - 1];
                    umcd.Transpose(transposition);
                    msPosTranspositionDict.Add(umcd.MsPositionReTrk, transposition);
                }
            }
            valueIndex = 0;
            for(int i = f4finalPhaseIndex; i < this.Count; ++i)
            {
                MidiChordDef umcd = this[i] as MidiChordDef;
                if(umcd != null)
                {
                    transposition = transpositionArray3[mod12Values[valueIndex++] - 1];
                    umcd.Transpose(transposition);
                    msPosTranspositionDict.Add(umcd.MsPositionReTrk, transposition);
                }
            }

            return msPosTranspositionDict;
        }

        /// <summary>
        /// Motion is contrary to the pan gliss in furies 1
        /// </summary>
        internal void AdjustPostludePan(int postludeMsPosition, int postludeMsPosition1, int postludeMsPosition2, int postludeMsPosition3)
        {
            SetPanGliss(postludeMsPosition, postludeMsPosition1, 64, 96);
            SetPanGliss(postludeMsPosition1, postludeMsPosition2, 96, 32);
            SetPanGliss(postludeMsPosition2, postludeMsPosition3, 32, 127);
            SetPanGliss(postludeMsPosition3, EndMsPosition, 127, 0);

            // furies1
            //SetPanGliss(postludeMsPosition, postludeMsPosition1, 64, 32);
            //SetPanGliss(postludeMsPosition1, postludeMsPosition2, 32, 96);
            //SetPanGliss(postludeMsPosition2, postludeMsPosition3, 96, 0);
            //SetPanGliss(postludeMsPosition3, EndMsPosition, 0, 127);
        }
        #endregion finale
    }
}
