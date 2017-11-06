using System;
using System.Collections.Generic;
using System.Diagnostics;
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
			: base(rootOctave, 0, 9)
		{
			for(int i = 0; i < BaseGrps.Count; ++i)
			{
				List<PaletteGrp> baseGrps = new List<PaletteGrp>(BaseGrps[i]);
				baseGrps.RemoveRange(baseGrps.Count - 2, 2);

				if(i % 2 == 1)
				{
					baseGrps.Reverse();
				}

				List<PaletteGrp> grps = Compose(baseGrps);

				_composedGrps.Add(grps);
			}
		}

		protected override List<PaletteGrp> Compose(IReadOnlyList<PaletteGrp> baseGrps)
		{
			var pGrps = new List<PaletteGrp>(baseGrps);


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
				int indexMax;
				if(pGrp.Count > 1)
				{
					if(index % 2 != 0)
					{
						pGrp.Permute(1, 7);
						indexMax = pGrp.Count / 3;
						if(indexMax > 0 && indexMax < pGrp.Count)
						{
							pGrp.AdjustVelocitiesHairpin(0, indexMax, minHairpin, maxHairpin);
							pGrp.AdjustVelocitiesHairpin(indexMax, pGrp.Count, maxHairpin, minHairpin);
						}
					}
					else
					{
						indexMax = (pGrp.Count * 2) / 3;
						if(indexMax > 0 && indexMax < pGrp.Count)
						{
							pGrp.AdjustVelocitiesHairpin(0, indexMax, minHairpin, maxHairpin);
							pGrp.AdjustVelocitiesHairpin((pGrp.Count * 2) / 3, pGrp.Count, maxHairpin, minHairpin);
						}
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

		/// <summary>
		/// The compulsory first barline (at msPosition=0) is NOT included in the returned list.
		/// The compulsory final barline (at the end of the final PaletteGrp) IS included in the returned list.
		/// There is a barline at the end of each list of PaletteGrp (i.e. gamut).
		/// All the returned barline positions are at the boundaries of composed PaletteGrps.
		/// </summary>
		internal List<int> BarlinePositions()
		{
			List<int> barlinePositions = new List<int>() { 0 }; // entry is removed just before this function returns

			int endMsPos = 0;
			for(int i = 0; i < _composedGrps.Count; ++i)
			{
				IReadOnlyList<PaletteGrp> gamut = _composedGrps[i];
				for(int j = 0; j < gamut.Count; j++)
				{
					PaletteGrp pGrp = gamut[j];
					endMsPos += pGrp.MsDuration;
				}				
				barlinePositions.Add(endMsPos);
			}

			#region insert intermediate barline positions
			//List<int> intermediateBarlinePositions = new List<int>();
			//SplitGamut(barlinePositions, 1, intermediateBarlinePositions);
			//SplitGamut(barlinePositions, 10, intermediateBarlinePositions);
			//SplitGamut(barlinePositions, 12, intermediateBarlinePositions);
			//foreach(int b in intermediateBarlinePositions)
			//{
			//	barlinePositions.Add(b);
			//}
			#endregion

			barlinePositions.Remove(0); 
			barlinePositions.Sort();
			
			Debug.Assert(barlinePositions[0] != 0);

			return barlinePositions;
		}

		private void SplitGamut(List<int> barlinePositions, int gamutNumber, List<int> intermediateBarlinePositions)
		{
			Debug.Assert(barlinePositions[0] == 0);
			int msPos = barlinePositions[gamutNumber - 1] + ((barlinePositions[gamutNumber] - barlinePositions[gamutNumber - 1]) / 2); // end barline msPosition for gamut 1, divided by 2
			int barlineMsPos = PaletteGrpEndMsPosFollowingMsPos(msPos);
			intermediateBarlinePositions.Add(barlineMsPos);
		}

		private int PaletteGrpEndMsPosFollowingMsPos(int msPos)
		{
			int endMsPos = 0;
			int rval = 0;
			for(int i = 0; i < _composedGrps.Count; ++i)
			{
				IReadOnlyList<PaletteGrp> gamut = _composedGrps[i];
				for(int j = 0; j < gamut.Count; j++)
				{
					PaletteGrp pGrp = gamut[j];
					endMsPos += pGrp.MsDuration;
					if(endMsPos >= msPos)
					{
						rval = endMsPos;
						break;
					}
				}
				if(rval > 0)
				{
					break;
				}
			}
			return rval;
		}
	}
}