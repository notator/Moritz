using System;
using System.Collections.Generic;
using System.Diagnostics;

using Krystals4ObjectLibrary;

using Moritz.Globals;
using Moritz.Palettes;
using Moritz.Spec;

namespace Moritz.Algorithm.Tombeau1
{
	public partial class Tombeau1Algorithm : CompositionAlgorithm
	{
        #region available Trk and Grp transformations
        // Add();
        // AddRange();
        // AdjustChordMsDurations();
        // AdjustExpression();
        // AdjustVelocities();
        // AdjustVelocitiesHairpin();
        // AlignObjectAtIndex();
        // CreateAccel();
        // FindIndexAtMsPositionReFirstIUD();
        // Insert();
        // InsertRange();
        // Permute();
        // Remove();
        // RemoveAt();
        // RemoveBetweenMsPositions();
        // RemoveRange();
        // RemoveScorePitchWheelCommands();
        // Replace();
        // SetDurationsFromPitches();
        // SetPanGliss(0, subT.MsDuration, 0, 127);
        // SetPitchWheelDeviation();
        // SetPitchWheelSliders();
        // SetVelocitiesFromDurations();
        // SetVelocityPerAbsolutePitch();
        // TimeWarp();
        // Translate();
        // Transpose();
        // TransposeStepsInGamut();
        // TransposeToRootInGamut();
        #endregion available Trk and Grp transformations

        /// <summary>
        /// Grps having the same location in the four GrpLists arguments all have (clones of) the same Gamut.
        /// </summary>
        private void ComposeSeqs(List<Seq> seqs, SopranoGrps sopranoGrps, AltoGrps altoGrps, TenorGrps tenorGrps, BassGrps bassGrps, IReadOnlyList<int> midiChannelIndexPerOutputVoice)
        {
			List<List<PaletteGrp>> sGrps = sopranoGrps.ComposedGrpsClone();
			List<List<PaletteGrp>> aGrps = altoGrps.ComposedGrpsClone();
			List<List<PaletteGrp>> tGrps = tenorGrps.ComposedGrpsClone();
			List<List<PaletteGrp>> bGrps = bassGrps.ComposedGrpsClone();

			#region just duplicate tenorGrps
			//List<Trk> trks = GrpListsToTrks(tenorGrps, midiChannelIndexPerOutputVoice[2]);
			//if(seqs.Count == 0)
			//{
			//    for(int i = 0; i < trks.Count; ++i)
			//    {
			//        seqs.Add(new Seq(0, new List<Trk>() { trks[i] }, midiChannelIndexPerOutputVoice));
			//    }
			//}
			//else
			//{
			//    Debug.Assert(seqs.Count == trks.Count);
			//    for(int i = 0; i < trks.Count; ++i)
			//    {
			//        Trk trk = trks[i];
			//        seqs[i].SetTrk(trk);
			//    }
			//}
			#endregion just duplicate tenorGrps

			// get the first 7 domain 7 tenorGrps
			List<PaletteGrp> dom7TenorGrps = new List<PaletteGrp>();
            for(int i = 0; i < 7; ++i)
            {
                List<PaletteGrp> tGrpList = tGrps[i];
                dom7TenorGrps.Add(tGrpList[5]);
            }

            Trk trk = GrpListToTrk(dom7TenorGrps, 2);
            trk.MsDuration *= 2;
            seqs.Add(new Seq(0, new List<Trk>() { trk }, midiChannelIndexPerOutputVoice));


            //Do global changes that affect all trks here (accel., rit, transpositions etc.)
            FinalizeSeqs(seqs);
        }

        /// <summary>
        /// Possibly do global changes that affect all trks here (accel., rit, transpositions etc.)
        /// </summary>
        /// <param name="seqs"></param>
        private void FinalizeSeqs(List<Seq> seqs)
        {

        }
    }
}
