using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Moritz.Spec;

namespace Moritz.Algorithm.Tombeau1
{
    internal class TenorGrps
    {
        public TenorGrps()
        {
            for(int i = 0; i < Gamut.RelativePitchHierarchiesCount; ++i)
            {
                List<Grp> grpList = GetGrpList(i);
                Grps.Add(grpList);
            }
        }

        public List<List<Grp>> Grps = new List<List<Grp>>();

        /// <summary>
        /// Creates a list of Grps having the same relativePitchHierarchyIndex.
        /// </summary>
        private List<Grp> GetGrpList(int relativePitchHierarchyIndex)
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
                int rootPitch = gamut.AbsolutePitchHierarchy[i];
                if(gamut.Contains(rootPitch))
                {
                    grp.TransposeToRootInGamut(rootPitch + (tenorRootOctave * 12));
                }
                
                grps.Add(grp);

                nPitchesPerOctave--;
            }
            return (grps);
        }
    }       
}