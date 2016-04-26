using System.Collections.Generic;
using System.Diagnostics;

using Krystals4ObjectLibrary;
using Moritz.Algorithm;
using Moritz.Palettes;
using Moritz.Spec;

namespace Moritz.Algorithm.SongSix
{
    internal class Furies1 : Trk
    {
		/// <summary>
		/// A VoiceDef beginning at MsPosition = 0, and containing a single RestDef having msDuration
		/// </summary>
		/// <param name="msDuration"></param>
		internal Furies1(int midiChannel, int msDuration)
			: base(midiChannel)
        {
			RestDef lmRestDef = new RestDef(0, msDuration);
			_uniqueDefs.Add(lmRestDef);
		}

        internal void GetBeforeInterlude3(Clytemnestra clytemnestra, Trk wind1, Trk furies2, Palette cheepsPalette)
        {
            int[] cheepIndices = { 4, 8, 2, 6, 10, 0, 1, 3, 5, 7, 9, 11 };
            int[] transpositions = { 2, 1, 3, 0, 4, -3, 5, 10, 6, 9, 7, 8 };
            double[] velocityfactors = { 0.32, 0.31, 0.34, 0.3, 0.35, 0.37, 0.36, 0.43, 0.37, 0.42, 0.39, 0.4 };
            int[] msPositions =
            { 
                furies2[8].MsPositionReTrk + 200, 
                furies2[12].MsPositionReTrk + 100, 
                furies2[24].MsPositionReTrk + 300, 
                furies2[30].MsPositionReTrk + 400, 
                furies2[40].MsPositionReTrk + 500,
                clytemnestra[122].MsPositionReTrk,
                clytemnestra[132].MsPositionReTrk + 110,
                clytemnestra[141].MsPositionReTrk + 220,
                clytemnestra[150].MsPositionReTrk + 330,
                clytemnestra[158].MsPositionReTrk + 440,
                clytemnestra[164].MsPositionReTrk + 550,
                clytemnestra[173].MsPositionReTrk,
            };
            for(int i = 0; i < cheepsPalette.Count; ++i)
            {
                MidiChordDef cheep = cheepsPalette.UniqueDurationDef(cheepIndices[i]) as MidiChordDef;
                Debug.Assert(cheep != null);
                cheep.MsPositionReTrk = msPositions[i];
                cheep.MsDuration *= 2;
                cheep.AdjustVelocities(velocityfactors[i]);
                cheep.Transpose(transpositions[i]);
                InsertInRest(cheep);
            }

            AlignObjectAtIndex(11, 12, 13, clytemnestra[123].MsPositionReTrk);
            AlignObjectAtIndex(21, 22, 23, clytemnestra[168].MsPositionReTrk);
        }

        #region finale
        internal void GetFinale(List<Palette> palettes, Dictionary<string, int> msPositions, Krystal krystal)
        {
            Palette f1FinalePalette1 = palettes[12];
            Palette f1FinalePalette2 = palettes[16];
            Palette f1PostludePalette = palettes[20];

            List<int> strandIndices = new List<int>();
            int index = 0;
            for(int i = 0; i < krystal.Strands.Count; ++i)
            {
                strandIndices.Add(index);
                index += krystal.Strands[i].Values.Count;
            }

            Trk f1Interlude3Verse4e = GetF1FinalePart1(f1FinalePalette1, krystal, strandIndices, msPositions);
            Trk f1Verse4eVerse5 = GetF1FinalePart2(f1FinalePalette2, krystal, strandIndices, msPositions);
            Trk f1Postlude = GetF1Postlude(f1PostludePalette, krystal, strandIndices, msPositions);

            Trk furies1Finale = f1Interlude3Verse4e;

            furies1Finale.AddRange(f1Verse4eVerse5);
            furies1Finale.AddRange(f1Postlude);

            //furies1Finale.TransposeNotation(-12);

            if(furies1Finale[furies1Finale.Count - 1] is RestDef)
            {
                furies1Finale.RemoveAt(furies1Finale.Count - 1);
            }

            if(furies1Finale[furies1Finale.Count - 1].MsPositionReTrk + furies1Finale[furies1Finale.Count - 1].MsDuration > msPositions["endOfPiece"])
            {
                furies1Finale.RemoveAt(furies1Finale.Count - 1);
            }

            InsertInRest(furies1Finale);

            Erase(this[282].MsPositionReTrk, msPositions["endOfPiece"]);
            
            AdjustPitchWheelDeviations(msPositions["interlude3"], msPositions["endOfPiece"], 5, 28 );
        }

        private Trk GetF1FinalePart1(Palette palette, Krystal krystal, List<int> strandIndices, Dictionary<string, int> msPositions)
        {
            Trk f1FinalePart1 = palette.NewTrk(0, krystal);

            List<int> f1eStrandDurations = GetStrandDurations(f1FinalePart1, strandIndices);

            int extraTime = 1000;
            int diff = extraTime / f1FinalePart1.Count;
            for(int i = f1FinalePart1.Count - 1; i > 0; --i)
            {
                if(strandIndices.Contains(i))
                {
                    RestDef umrd = new RestDef(f1FinalePart1[i].MsPositionReTrk, f1eStrandDurations[strandIndices.IndexOf(i)] + extraTime);
                    extraTime -= diff;
                    f1FinalePart1.Insert(i, umrd);
                }
            }

            f1FinalePart1.MsPositionReSeq = msPositions["interlude3Bar2"];

            f1FinalePart1.RemoveBetweenMsPositions(msPositions["verse4EsCaped"], int.MaxValue);

            if(f1FinalePart1[f1FinalePart1.Count - 1] is RestDef)
            {
                f1FinalePart1[f1FinalePart1.Count - 1].MsDuration = msPositions["verse4EsCaped"] - f1FinalePart1[f1FinalePart1.Count - 1].MsPositionReTrk;
            }

            return f1FinalePart1;
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

        private Trk GetF1FinalePart2(Palette f1FinalePalette2, Krystal krystal, List<int> strandIndices, Dictionary<string, int> msPositions)
        {
			Trk f1FinalePart2 = f1FinalePalette2.NewTrk(this.MidiChannel, krystal);

            List<int> f1eStrandDurations = GetStrandDurations(f1FinalePart2, strandIndices);

            int extraTime = 500;
            int diff = extraTime / f1FinalePart2.Count;
            for(int i = f1FinalePart2.Count - 1; i > 0; --i)
            {
                if(strandIndices.Contains(i))
                {
                    RestDef umrd = new RestDef(f1FinalePart2[i].MsPositionReTrk, f1eStrandDurations[strandIndices.IndexOf(i)] + extraTime);
                    extraTime -= diff;
                    f1FinalePart2.Insert(i, umrd);
                }
            }

            f1FinalePart2.MsPositionReSeq = msPositions["verse4EsCaped"];
            f1FinalePart2.RemoveBetweenMsPositions(msPositions["verse5Calls"], int.MaxValue);

            if(f1FinalePart2[f1FinalePart2.Count - 1] is RestDef)
            {
                f1FinalePart2[f1FinalePart2.Count - 1].MsDuration = msPositions["postlude"] - f1FinalePart2[f1FinalePart2.Count - 1].MsPositionReTrk;
            }

            return f1FinalePart2;
        }

        private Trk GetF1Postlude(Palette f1PostludePalette, Krystal krystal, List<int> strandIndices, Dictionary<string, int> msPositions)
        {
			Trk f1p = f1PostludePalette.NewTrk(this.MidiChannel, krystal);

            List<int> f1eStrandDurations = GetStrandDurations(f1p, strandIndices);

            for(int i = f1p.Count - 1; i > 0; --i)
            {
                if(strandIndices.Contains(i))
                {
                    RestDef umrd = new RestDef(f1p[i].MsPositionReTrk, f1eStrandDurations[strandIndices.IndexOf(i)] / 4);
                    f1p.Insert(i, umrd);
                }
            }

            f1p.MsPositionReSeq = msPositions["postlude"];
            f1p.RemoveBetweenMsPositions(msPositions["endOfPiece"], int.MaxValue);
            
            return f1p;
        }

        internal void AdjustAlignments(Clytemnestra c, Trk w2, Trk w3)
        {
            Debug.Assert(this[213] is RestDef);
            this[213].MsDuration += this[212].MsDuration;
            RemoveAt(212);
            AgglomerateRests();

            AlignObjectAtIndex(25, 84, 85, c[196].MsPositionReTrk);
            AlignObjectAtIndex(84, 85, 89, c[204].MsPositionReTrk + 200);
            AlignObjectAtIndex(85, 89, 96, c[215].MsPositionReTrk);
            AlignObjectAtIndex(89, 96, 102, c[226].MsPositionReTrk);
            AlignObjectAtIndex(102, 106, 117, c[242].MsPositionReTrk);
            AlignObjectAtIndex(106, 117, 140, c[268].MsPositionReTrk);
            AlignObjectAtIndex(117, 140, 165, w3[61].MsPositionReTrk);
            AlignObjectAtIndex(140, 165, 197, w2[65].MsPositionReTrk); // was AlignObjectAtIndex(140, 163, 197, wind3[65].MsPosition);
            AlignObjectAtIndex(165, 197, 200, c[269].MsPositionReTrk - 200);
            AlignObjectAtIndex(197, 200, 206, c[277].MsPositionReTrk);
            AlignObjectAtIndex(200, 206, 211, c[283].MsPositionReTrk + 400);
            AlignObjectAtIndex(206, 211, 212, c[286].MsPositionReTrk);
            AlignObjectAtIndex(211, 212, Count - 1, c[289].MsPositionReTrk);

            // final adjustments for R2M
            AlignObjectAtIndex(11, 12, 13, c[123].MsPositionReTrk - 200);
            AlignObjectAtIndex(106, 111, 112, c[254].MsPositionReTrk - 100);
        }

        internal void AdjustVelocities(Dictionary<string, int> msPositions)
        {
            AdjustVelocitiesHairpin(msPositions["interlude3"], this[102].MsPositionReTrk, 0.65, 0.2);
            AdjustVelocitiesHairpin(this[102].MsPositionReTrk, msPositions["interlude4"], 0.2, 0.7);
            AdjustVelocitiesHairpin(msPositions["interlude4"], msPositions["verse5"], 0.7, 1.0);
            AdjustVelocitiesHairpin(msPositions["verse5"], msPositions["postlude"], 0.5, 0.5);
            //AdjustVelocitiesHairpin(msPositions["postlude"], EndMsPosition, 0.8, 1.0);

            AdjustVelocitiesHairpin(msPositions["postlude"], msPositions["postludeDiminuendo"], 0.5, 1.0);
            AdjustVelocitiesHairpin(msPositions["postludeDiminuendo"], EndMsPosition, 1.0, 0.4);
        }

        internal void AdjustPostludePan(int postludeMsPosition, int postludeMsPosition1, int postludeMsPosition2, int postludeMsPosition3)
        {
            SetPanGliss(postludeMsPosition, postludeMsPosition1, 64, 32);
            SetPanGliss(postludeMsPosition1, postludeMsPosition2, 32, 96);
            SetPanGliss(postludeMsPosition2, postludeMsPosition3, 96, 0);
            SetPanGliss(postludeMsPosition3, EndMsPosition, 0, 127);
        }

        #endregion finale
    }
}
