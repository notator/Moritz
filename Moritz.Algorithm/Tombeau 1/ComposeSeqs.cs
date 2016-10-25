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
        private void ComposeSeqs(List<Seq> seqs, List<List<Grp>> sopranoGrps, List<List<Grp>> altoGrps, List<List<Grp>> tenorGrps, List<List<Grp>> bassGrps, List<int> MidiChannelIndexPerOutputVoice)
        {
            // TODO

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
