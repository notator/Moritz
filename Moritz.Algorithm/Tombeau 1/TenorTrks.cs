using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Moritz.Spec;

namespace Moritz.Algorithm.Tombeau1
{
    internal class TenorTrks : TrkSequence
    {
        public TenorTrks()
            : base()
        {
            GrpLists = GetGrpLists();

            bool displayGrpPalettes = true;
            if(displayGrpPalettes)
            {
                Trks = GetGrpPalettes(GrpLists);
            }
            else
            {
                Trks = GetTombeau1SeqTrks(GrpLists);
            }
        }

        #region available trk transformations
        // subT.Add();
        // subT.AddRange();
        // subT.AdjustChordMsDurations();
        // subT.AdjustExpression();
        // subT.AdjustVelocities();
        // subT.AdjustVelocitiesHairpin();
        // subT.AlignObjectAtIndex();
        // subT.CreateAccel();
        // subT.FindIndexAtMsPositionReFirstIUD();
        // subT.Insert();
        // subT.InsertRange();
        // subT.Permute();
        // subT.Remove();
        // subT.RemoveAt();
        // subT.RemoveBetweenMsPositions();
        // subT.RemoveRange();
        // subT.RemoveScorePitchWheelCommands();
        // subT.Replace();
        // subT.SetDurationsFromPitches();
        // subT.SetPanGliss(0, subT.MsDuration, 0, 127);
        // subT.SetPitchWheelDeviation();
        // subT.SetPitchWheelSliders();
        // subT.SetVelocitiesFromDurations();
        // subT.SetVelocityPerAbsolutePitch();
        // subT.TimeWarp();
        // subT.Translate();
        // subT.Transpose();
        // subT.TransposeInGamut();
        #endregion available trk transformations

        /// <summary>
        /// Called by the GetGrpLists() function.
        /// Creates a list of Grps having the same relativePitchHierarchyIndex.
        /// </summary>
        protected override List<Grp> GetGrpList(int relativePitchHierarchyIndex)
        {
            const int nGrpsPerPalette = 7;
            const int gamutBasePitch = 0;
            const int tenorRootOctave = 4;
            const int tenorPitchesPerChord = 6;
            const int tenorMsDurationPerChord = 200;
            const double velocityFactor = 0.5;

            int nPitchesPerOctave = 12; // decreases during the loop
            List<Grp> grps = new List<Grp>();
            for(int i = 0; i < nGrpsPerPalette; ++i)
            {
                Gamut gamut = new Gamut(relativePitchHierarchyIndex, gamutBasePitch, nPitchesPerOctave);
                int nChords = nPitchesPerOctave;
                Grp grp = new Grp(gamut, tenorRootOctave, tenorPitchesPerChord, tenorMsDurationPerChord, nChords, velocityFactor);
                grp.TransposeInGamut(gamut.AbsolutePitchHierarchy[i]);
                grps.Add(grp);

                nPitchesPerOctave--;
            }
            return (grps);
        }

        /// <summary>
        /// This is where the composition is actually done.
        /// The final sequence of Grps, possibly separated by RestDefs
        /// </summary>
        protected override List<Trk> GetTombeau1SeqTrks(List<List<Grp>> grpLists)
        {
            List<Trk> seqTrks = new List<Trk>();
            return seqTrks;
        }
    }       
}