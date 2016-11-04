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
                List<Grp> grpList = GetTenorPaletteGrpList(i);
                Grps.Add(grpList);
            }
        }

        public List<List<Grp>> Grps = new List<List<Grp>>();

        /// <summary>
        /// Creates a list of TenorPaletteGrps, each of which has the same relativePitchHierarchyIndex.
        /// </summary>
        private List<Grp> GetTenorPaletteGrpList(int relativePitchHierarchyIndex)
        {
            const int gamutBasePitch = 0;
            List<Grp> grps = new List<Grp>();

            for(int domain = 12; domain >= 1; --domain) // domain is both Gamut.PitchesPerOctave and nChords per Grp
            {
                Gamut gamut = new Gamut(relativePitchHierarchyIndex, gamutBasePitch, domain);

                Grp grp = new TenorPaletteGrp(gamut, domain);
                grps.Add(grp);
            }

            return (grps);
        }
    }       
}