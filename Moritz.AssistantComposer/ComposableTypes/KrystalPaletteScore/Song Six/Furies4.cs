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
            //PermutationKrystal krystal = new PermutationKrystal("C://Moritz/krystals/krystals/pk4(12)-2.krys");
            ExpansionKrystal krystal = new ExpansionKrystal("C://Moritz/krystals/krystals/xk3(12.12.1)-1.krys");

            VoiceDef furies4Finale = GetF4Finale(palettes, krystal, msPositions);

            if(furies4Finale[furies4Finale.Count - 1] is UniqueMidiRestDef)
            {
                furies4Finale.RemoveAt(furies4Finale.Count - 1);
            }

            if(furies4Finale[furies4Finale.Count - 1].MsPosition + furies4Finale[furies4Finale.Count - 1].MsDuration > msPositions["endOfPiece"])
            {
                furies4Finale.RemoveAt(furies4Finale.Count - 1);
            }

            InsertInRest(furies4Finale);

            RemoveScorePitchWheelCommandsFromControlledChords(); // interlude4 (to immediately before verse5)

            AdjustPitchWheelDeviations(msPositions["verse5"], msPositions["endOfPiece"], 5, 28);
        }

        private void RemoveScorePitchWheelCommandsFromControlledChords()
        {
            RemoveScorePitchWheelCommands(42, 43);
            RemoveScorePitchWheelCommands(45, 46);
            RemoveScorePitchWheelCommands(47, 48);
            RemoveScorePitchWheelCommands(49, 50);
            RemoveScorePitchWheelCommands(52, 53);
        }

        private VoiceDef GetF4Finale(List<PaletteDef> palettes , ExpansionKrystal krystal, Dictionary<string, int> msPositions)
        {
            PaletteDef f4FinalePalette1 = palettes[9];
            PaletteDef f4FinalePalette2 = palettes[13];
            PaletteDef f4PostludePalette = palettes[17];

            VoiceDef interlude4Start = new VoiceDef(f4FinalePalette1, krystal);
            Transform(interlude4Start, msPositions);
            VoiceDef interlude4EndVerse5 = new VoiceDef(f4FinalePalette2, krystal);
            Transform(interlude4EndVerse5, msPositions);
            VoiceDef postlude = new VoiceDef(f4PostludePalette, krystal);
            Transform(postlude, msPositions);

            VoiceDef finale = GetFinaleSections(interlude4Start, interlude4EndVerse5, postlude, 7, 17);

            return finale;
        }

        /// <summary>
        /// ACHTUNG: this function should be in a furies class. It is used by furies 2 and furies 4 (probably furies 3 too!)
        /// The three VoiceDefs are parallel. They have the same number of DurationDefs, each having the same MsPosition and
        /// MsDuration. The DurationDefs come from different palettes, so use different pitches etc.
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

        private void Transform(VoiceDef section, Dictionary<string, int> msPositions)
        {
            section.RemoveRange(40, section.Count - 40);

            section.StartMsPosition = msPositions["interlude4"];

            //double factor = 10;

            //section.AdjustMsDurations(factor);

            section.CreateAccel(0, section.Count, 0.08);

            //section.RemoveBetweenMsPositions(msPositions["interlude4End"], int.MaxValue);
            section.RemoveBetweenMsPositions(msPositions["finalWindChord"], int.MaxValue);

            if(section[section.Count - 1] is UniqueMidiRestDef)
            {
                //section[section.Count - 1].MsDuration = msPositions["interlude4End"] - section[section.Count - 1].MsPosition;
                section[section.Count - 1].MsDuration = msPositions["endOfPiece"] - section[section.Count - 1].MsPosition;
            }
        }

        internal void AdjustAlignments(Furies1 furies1, Clytemnestra clytemnestra, VoiceDef wind3)
        {
            AlignObjectAtIndex(42, Count-1, Count, furies1[280].MsPosition);

            AlignObjectAtIndex(42, 43, 45, furies1[126].MsPosition);
            AlignObjectAtIndex(43, 45, 49, furies1[138].MsPosition);
            AlignObjectAtIndex(45, 49, 59, furies1[165].MsPosition);
            AlignObjectAtIndex(49, 59, 69, furies1[212].MsPosition);

            AlignObjectAtIndex(59, 69, Count-1, furies1[248].MsPosition);
        }

        internal void AdjustVelocities(Dictionary<string, int> msPositions)
        {
            AdjustVelocitiesHairpin(msPositions["interlude4"], msPositions["verse5"], 0.3, 0.5);
            AdjustVelocitiesHairpin(msPositions["verse5"], msPositions["postlude"], 0.4, 0.4);

            //AdjustVelocitiesHairpin(msPositions["postlude"], EndMsPosition, 0.4, 1.0);

            AdjustVelocitiesHairpin(msPositions["postlude"], msPositions["postludeDiminuendo"], 0.4, 1.0);
            AdjustVelocitiesHairpin(msPositions["postludeDiminuendo"], EndMsPosition, 1.0, 0.2);
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
