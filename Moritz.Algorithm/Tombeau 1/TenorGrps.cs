using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Moritz.Spec;

namespace Moritz.Algorithm.Tombeau1
{
    internal class TenorGrps : PaletteGrps
    {
        public TenorGrps(int rootOctave, IReadOnlyList<IReadOnlyList<Grp>> sopranoGrps,	IReadOnlyList<IReadOnlyList<Grp>> altoGrps, IReadOnlyList<IReadOnlyList<Grp>> bassGrps)
			: base(rootOctave, 9, 79000)
        {
			for(int i = 0; i < BaseGrps.Count; ++i)
			{
				IReadOnlyList<PaletteGrp> baseGrps = BaseGrps[i];

				List<PaletteGrp> grps = Compose(baseGrps); // default Compose...

				_composedGrps.Add(grps);
			}
		}

        /// <summary>
        /// Creates a list of TenorPaletteGrps, each of which has the same relativePitchHierarchyIndex.
        /// </summary>
        private List<Grp> GetTenorPaletteGrpList(int relativePitchHierarchyIndex)
        {
            const int gamutBasePitch = 0;
            List<Grp> grps = new List<Grp>();

            for(int i = 0, domain = 12; domain >= 1; --domain, ++i) // domain is both Gamut.PitchesPerOctave and nChords per Grp
            {
                Gamut gamut = new Gamut(relativePitchHierarchyIndex, gamutBasePitch, domain);

                PaletteGrp tpg = new PaletteGrp(gamut, 3);
				int minMsDuration = 200;
				int maxMsDuration = 300;
				tpg.SetDurationsFromPitches(maxMsDuration, minMsDuration, true);

				#region begin test code 1 Shear and permute
				tpg.Shear(0, -1 * (gamut.NPitchesPerOctave));
                tpg.SetVelocitiesForGamut();

                if(domain % 2 != 0)
                {
                    tpg.Permute(1, 7);
                }
				#endregion end test code 1

				#region begin test code 2 transpose chords to the same absolute root pitch
				//for(int iudIndex = 0; iudIndex < tpg.Count; ++iudIndex)
				//{
				//    tpg.TransposeChordDownToAbsolutePitch(iudIndex, 0);
				//}
				#endregion end test code 2

				#region begin test code 3, adjust velocities
				//if(domain % 2 != 0)
				//{
				//    tpg.AdjustVelocities(0.5);
				//}
				#endregion

				#region begin test code 4, adjust velocities
				//if(domain % 2 != 0 && tpg.Count > 1)
				//{
				//	tpg.AdjustVelocitiesHairpin(0, (tpg.Count - 1) / 2, 0.5, 1.0);
				//	tpg.AdjustVelocitiesHairpin((tpg.Count - 1) / 2, tpg.Count - 1, 1.0, 0.5);
				//}
				#endregion

				#region begin test code 5, related Grps
				//if(domain % 2 != 0 && tpg.Count > 1)
				//{
				//    TenorPaletteGrp previousTpg = (TenorPaletteGrp)grps[i - 1];
				//    //tpg = previousTpg.RelatedPitchHierarchyGrp(previousTpg.Gamut.RelativePitchHierarchyIndex + 11);
				//    //tpg = previousTpg.RelatedBasePitchGrp(11);
				//    tpg = previousTpg.RelatedDomainGrp(6);
				//}
				#endregion

				#region begin test code 6, timeWarp
				//if(domain % 2 != 0 && tpg.Count > 1)
				//{
				//    tpg.TimeWarp(new Envelope(new List<int>() { 4, 7, 2 }, 7, 7, tpg.Count), 20);
				//}
				#endregion

				#region begin test code 7, SetPitchWheelSliders
				//Envelope env = new Envelope(new List<int>() { 0,8 }, 8, 127, tpg.Count);
				//tpg.SetPitchWheelSliders(env);
				#endregion

				#region begin test code 8, SetPanGliss
				//if(tpg.Count > 1)
				//{
				//    if(domain % 2 != 0)
				//    {
				//        tpg.SetPanGliss(0, tpg.Count - 1, 127, 0);
				//    }
				//    else
				//    {
				//        tpg.SetPanGliss(0, tpg.Count - 1, 0, 127);
				//    }
				//}
				#endregion

				#region begin test code 8, set inverse velocities
				//if(domain % 2 != 0 && tpg.Count > 1)
				//{
				//    TenorPaletteGrp prevTpg = (TenorPaletteGrp)grps[i - 1];
				//    Gamut prevGamut = prevTpg.Gamut;
				//    tpg = new TenorPaletteGrp(prevGamut); // identical to prevTpg
				//    // inverse velocityPerAbsolutePitch
				//    List<byte> velocityPerAbsolutePitch = prevGamut.GetVelocityPerAbsolutePitch(20, 127, prevGamut.NPitchesPerOctave - 1);
				//    tpg.SetVelocityPerAbsolutePitch(velocityPerAbsolutePitch, 20);
				//}
				#endregion

				#region begin test code 8, set Gamut (pitches
				//if(domain % 2 != 0 && tpg.Count > 1)
				//{
				//    TenorPaletteGrp prevTpg = (TenorPaletteGrp)grps[i - 1];
				//    Gamut prevGamut = prevTpg.Gamut;
				//    tpg = new TenorPaletteGrp(prevGamut); // identical to prevTpg

				//    int newRelativePitchHierarchyIndex = prevGamut.RelativePitchHierarchyIndex + 11;
				//    int newBasePitch = prevGamut.BasePitch;
				//    int newNPitchesPerOctave = 8;
				//    Gamut gamut1 = new Gamut(newRelativePitchHierarchyIndex, newBasePitch, newNPitchesPerOctave);
				//    tpg.Gamut = gamut1; // sets the pitches, velocities are still those of the original pitches.

				//    // reverse the velocityperAbsolutePitch hierarchy re the prevGamut.
				//    List<byte> velocityPerAbsolutePitch = prevGamut.GetVelocityPerAbsolutePitch(20, 127, prevGamut.NPitchesPerOctave - 1);
				//    tpg.SetVelocityPerAbsolutePitch(velocityPerAbsolutePitch, 20);
				//}
				#endregion

				grps.Add(tpg);
            }

            return (grps);
        }


	}       
}