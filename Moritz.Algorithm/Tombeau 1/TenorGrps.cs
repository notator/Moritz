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

                TenorPaletteGrp tpg = new TenorPaletteGrp(gamut, domain);

                #region begin test code 1 Shear and permute
                //tpg.Shear(0, -1 * (gamut.NPitchesPerOctave));

                //if(domain % 2 != 0)
                //{
                //    tpg.Permute(1, 7);
                //}
                #endregion end test code 1

                #region begin test code 2 transpose to the same absolute root pitch
                for(int iudIndex = 0; iudIndex < tpg.Count; ++iudIndex)
                {
                    tpg.TransposeChordDownToAbsolutePitch(iudIndex, 0);
                }
                #endregion end test code 2

                #region begin test code 3, adjust velocities
                //if(domain % 2 != 0)
                //{
                //    tpg.AdjustVelocities(0.5);
                //}
                #endregion

                #region begin test code 4, adjust velocities
                if(domain % 2 != 0 && tpg.Count > 1)
                {
                    tpg.AdjustVelocitiesHairpin(0, tpg.Count - 1, 0.5, 1.0);
                }
                #endregion

                grps.Add(tpg);
            }

            return (grps);
        }
    }       
}