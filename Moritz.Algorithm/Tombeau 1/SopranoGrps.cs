using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Moritz.Spec;

namespace Moritz.Algorithm.Tombeau1
{
    internal class SopranoGrps
    {
        public SopranoGrps(List<List<Grp>> tenorGrps)
        {
            for(int i = 0; i < tenorGrps.Count; ++i)
            {
                List<Grp> grpList = GetGrpList(tenorGrps[i]);
                Grps.Add(grpList);
            }
        }

        public List<List<Grp>> Grps = new List<List<Grp>>();

        /// <summary>
        /// Each returned Grp has the same Gamut as the parallel tenorGrp.
        /// </summary>
        private List<Grp> GetGrpList(List<Grp> tenorGrps)
        {
            const int relativeSopranoRootOctave = 3; // tenorRootOctave + 3

            List<Grp> grps = new List<Grp>();
            for(int i = 0; i < tenorGrps.Count; ++i)
            {
                Grp grp = tenorGrps[i].Clone();

                grp.SortRootNotatedPitchAscending();

                int rootPitch = ((MidiChordDef)grp.UniqueDefs[0]).NotatedMidiPitches[0];
                grp.TransposeToRootInGamut(rootPitch + (relativeSopranoRootOctave * 12));

                int steps = 4;
                ((MidiChordDef)grp.UniqueDefs[0]).TransposeStepsInGamut(grp.Gamut, steps);

                grps.Add(grp);
            }
            return (grps);
        }

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

    }
}