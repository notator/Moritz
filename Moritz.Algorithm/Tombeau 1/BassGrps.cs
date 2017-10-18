using System;
using System.Collections.Generic;
using Moritz.Spec;

namespace Moritz.Algorithm.Tombeau1
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

	internal class BassGrps : PaletteGrps
    {
		public BassGrps(int rootOctave)
			: base(rootOctave, 0, 9, 78000)
		{
			for(int i = 0; i < BaseGrps.Count; ++i)
			{
				IReadOnlyList<PaletteGrp> baseGrps = BaseGrps[i];

				List<PaletteGrp> grps = Compose(baseGrps);

				_composedGrps.Add(grps);
			}
		}

		protected override List<PaletteGrp> Compose(IReadOnlyList<PaletteGrp> baseGrps)
		{
			var pGrps = new List<PaletteGrp>(baseGrps);
			pGrps.RemoveRange(pGrps.Count - 2, 2);

			for(int index = 0; index < pGrps.Count; ++index)
			{
				PaletteGrp pGrp = pGrps[index];

				#region current version
				if(pGrp.UniqueDefs[0] is MidiChordDef firstMcd && pGrp.UniqueDefs[pGrp.UniqueDefs.Count - 1] is MidiChordDef lastMcd)
				{
					pGrp.Shear(0, -1 * (pGrp.Gamut.NPitchesPerOctave));

					while(lastMcd.NotatedMidiPitches[0] % 12 != (firstMcd.NotatedMidiPitches[0] % 12))
					{
						pGrp.Shear(0, -1);
					}
				}

				pGrp.SetVelocityPerAbsolutePitch(_rootGamut);

				double minHairpin = 0.5;
				double maxHairpin = 1.3;
				if(pGrp.Count > 1)
				{
					if(index % 2 != 0)
					{
						pGrp.Permute(1, 7);
						pGrp.AdjustVelocitiesHairpin(0, pGrp.Count / 3, minHairpin, maxHairpin);
						pGrp.AdjustVelocitiesHairpin(pGrp.Count / 3, pGrp.Count, maxHairpin, minHairpin);
					}
					else
					{
						pGrp.AdjustVelocitiesHairpin(0, (pGrp.Count * 2) / 3, minHairpin, maxHairpin);
						pGrp.AdjustVelocitiesHairpin((pGrp.Count * 2) / 3, pGrp.Count, maxHairpin, minHairpin);
					}
				}

				//pGrp.AdjustChordMsDurations(factor: 5);
				#endregion current version

				#region begin test code 2 transpose chords to the same absolute root pitch
				//for(int iudIndex = 0; iudIndex < g.Count; ++iudIndex)
				//{
				//    g.TransposeChordDownToAbsolutePitch(iudIndex, 0);
				//}
				#endregion end test code 2

				#region begin test code 3, adjust velocities
				//if(domain % 2 != 0)
				//{
				//    g.AdjustVelocities(0.5);
				//}
				#endregion

				#region begin test code 5, related Grps
				//if(domain % 2 != 0 && g.Count > 1)
				//{
				//    TenorPaletteGrp previousTpg = (TenorPaletteGrp)grps[i - 1];
				//    //g = previousTpg.RelatedPitchHierarchyGrp(previousTpg.Gamut.RelativePitchHierarchyIndex + 11);
				//    //g = previousTpg.RelatedBasePitchGrp(11);
				//    g = previousTpg.RelatedDomainGrp(6);
				//}
				#endregion

				#region begin test code 6, timeWarp
				//if(domain % 2 != 0 && g.Count > 1)
				//{
				//	g.TimeWarp(new Envelope(new List<int>() { 4, 6, 2 }, 7, 7, g.Count), 20);
				//}
				#endregion

				#region begin test code 7, SetPitchWheelSliders
				//Envelope env = new Envelope(new List<int>() { 0,8 }, 8, 127, g.Count);
				//g.SetPitchWheelSliders(env);
				#endregion

				#region begin test code 8, SetPanGliss
				//if(g.Count > 1)
				//{
				//    if(domain % 2 != 0)
				//    {
				//        g.SetPanGliss(0, g.Count - 1, 127, 0);
				//    }
				//    else
				//    {
				//        g.SetPanGliss(0, g.Count - 1, 0, 127);
				//    }
				//}
				#endregion

				#region begin test code 8, set inverse velocities
				//if(domain % 2 != 0 && g.Count > 1)
				//{
				//    TenorPaletteGrp prevTpg = (TenorPaletteGrp)grps[i - 1];
				//    Gamut prevGamut = prevTpg.Gamut;
				//    g = new TenorPaletteGrp(prevGamut); // identical to prevTpg
				//    // inverse velocityPerAbsolutePitch
				//    List<byte> velocityPerAbsolutePitch = prevGamut.GetVelocityPerAbsolutePitch(20, 127, prevGamut.NPitchesPerOctave - 1);
				//    g.SetVelocityPerAbsolutePitch(velocityPerAbsolutePitch, 20);
				//}
				#endregion

				#region begin test code 8, set Gamut (pitches
				//if(domain % 2 != 0 && g.Count > 1)
				//{
				//    TenorPaletteGrp prevTpg = (TenorPaletteGrp)grps[i - 1];
				//    Gamut prevGamut = prevTpg.Gamut;
				//    g = new TenorPaletteGrp(prevGamut); // identical to prevTpg

				//    int newRelativePitchHierarchyIndex = prevGamut.RelativePitchHierarchyIndex + 11;
				//    int newBasePitch = prevGamut.BasePitch;
				//    int newNPitchesPerOctave = 8;
				//    Gamut gamut1 = new Gamut(newRelativePitchHierarchyIndex, newBasePitch, newNPitchesPerOctave);
				//    g.Gamut = gamut1; // sets the pitches, velocities are still those of the original pitches.

				//    // reverse the velocityperAbsolutePitch hierarchy re the prevGamut.
				//    List<byte> velocityPerAbsolutePitch = prevGamut.GetVelocityPerAbsolutePitch(20, 127, prevGamut.NPitchesPerOctave - 1);
				//    g.SetVelocityPerAbsolutePitch(velocityPerAbsolutePitch, 20);
				//}
				#endregion

			}
			return pGrps;
		}
	}
}