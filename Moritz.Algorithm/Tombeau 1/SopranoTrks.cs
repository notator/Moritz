using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Moritz.Spec;

namespace Moritz.Algorithm.Tombeau1
{
    internal class SopranoTrks : TrkSequence
    {
        public SopranoTrks(TenorTrks tenorTrks, bool displayPalette)
            : base()
        {
            Palette = GetPalette(tenorTrks.Palette);

            if(displayPalette)
            {
                Trks = PaletteToTrks(Palette);
            }
            else
            {
                Trks = GetTombeau1SeqTrks(Palette);
            }
        }

        /// <summary>
        /// Each Grp in a GrpList has the same Gamut.
        /// </summary>
        private List<List<Grp>> GetPalette(List<List<Grp>> tenorPalette)
        {
            List<List<Grp>> grpLists = new List<List<Grp>>();

            for(int i = 0; i < tenorPalette.Count; ++i)
            {
                List<Grp> grpList = GetGrpList(tenorPalette[i]);
                grpLists.Add(grpList);
            }

            return grpLists;
        }

        /// <summary>
        /// Called by the above GetGrpLists() function.
        /// Creates a list of Grps having the same relativePitchHierarchyIndex.
        /// </summary>
        private List<Grp> GetGrpList(List<Grp> tenorGrps)
        {
            const int sopranoRootOctave = 6; // tenorRootOctave + 2

            List<Grp> grps = new List<Grp>();
            for(int i = 0; i < tenorGrps.Count; ++i)
            {
                Grp grp = tenorGrps[i].Clone();

                int rootPitch = grp.Gamut.AbsolutePitchHierarchy[i];
                if(grp.Gamut.Contains(rootPitch))
                {
                    grp.TransposeToRootInGamut(rootPitch + (sopranoRootOctave * 12));
                }
                
                grps.Add(grp);
            }
            return (grps);
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
        // subT.TransposeStepsInGamut();
        // subT.TransposeToRootInGamut();
        #endregion available trk transformations

        /// <summary>
        /// This is where the composition is actually done.
        /// The final sequence of Grps, possibly separated by RestDefs
        /// </summary>
        protected override List<Trk> GetTombeau1SeqTrks(List<List<Grp>> palette)
        {
            List<Trk> seqTrks = new List<Trk>();
            return seqTrks;
        }
    }       
}