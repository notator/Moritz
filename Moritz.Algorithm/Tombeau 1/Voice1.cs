﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using Moritz.Spec;

namespace Moritz.Algorithm.Tombeau1
{
	#region available Trk and GamutTrk transformations
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
	#endregion available Trk and GamutTrk transformations

	internal class Voice1 : Tombeau1Voice
    {
		public Voice1()
			: base()
		{
			int relativePitchHierarchyIndex = 0;
			int basePitch = 9;
			int nPitchesPerOctave = 12;

			RootGamut = new Gamut(relativePitchHierarchyIndex, basePitch, nPitchesPerOctave);

			_composedModeSegments = GetBasicModeSegments(12);
		 }

		private List<ModeSegment> GetBasicModeSegments(int nModeSegments)
		{
			List<GamutProximity> gamutProximities = RootGamut.FindRelatedGamuts();

			int maxIndex = gamutProximities.Count - 1;
			int indexInc = maxIndex / nModeSegments;
			List<int> gamutIndices = new List<int>();
			for(int i = 0; i < nModeSegments; i++)
			{
				gamutIndices.Add(i * indexInc);
			}
			gamutIndices[nModeSegments - 1] = maxIndex; // correct any rounding error.

			var baseGamuts = new List<Gamut>();
			for(int i = 0; i < nModeSegments; i++)
			{
				Gamut gamut = FindBaseGamut(gamutProximities, RootGamut.BasePitch, gamutIndices[i]);
				gamutProximities = gamut.FindRelatedGamuts();
				var common = RootGamut.GetCommonAbsolutePitches(gamut);
				baseGamuts.Add(gamut);
			}

			int rootOctave = 2;
			List<ModeSegment> basicModeSegments = new List<ModeSegment>();
			foreach(var gamut in baseGamuts)
			{
				ModeSegment modeSegment = GetBasicModeSegment(rootOctave, gamut.BasePitch, gamut.RelativePitchHierarchyIndex);
				basicModeSegments.Add(modeSegment);
			}

			for(int i = 0; i < basicModeSegments.Count; ++i)
			{
				ModeSegment modeSegment = basicModeSegments[i];
				modeSegment.RemoveRange(modeSegment.Count - 4, 4);

				if(i % 2 == 1)
				{
					modeSegment.Reverse();
				}

				basicModeSegments[i] = Compose(modeSegment);
			}

			return basicModeSegments;
		}

		/// <summary>
		/// Find the nearest Gamut to startIndex having BasePitch = basePitch.
		/// </summary>
		private Gamut FindBaseGamut(List<GamutProximity> gamutProximities, int basePitch, int startIndex)
		{
			Gamut rval = null;
			int? index1 = null;
			for(int index = startIndex; index >= 0; --index)
			{
				if(gamutProximities[index].Gamut.BasePitch == basePitch)
				{
					index1 = index;
					break;
				}
			}
			int? index2 = null;
			for(int index = startIndex + 1; index < gamutProximities.Count; ++index)
			{
				if(gamutProximities[index].Gamut.BasePitch == basePitch)
				{
					index2 = index;
					break;
				}

			}
			if(index1 == null)
			{
				if(index2 != null)
				{
					rval = gamutProximities[(int)index2].Gamut;
				}
			}
			if(index2 == null)
			{
				if(index1 != null)
				{
					rval = gamutProximities[(int)index1].Gamut;
				}
			}
			else // neither index1 nor index2 is null
			{
				rval = ((startIndex - index1) < (index2 - startIndex)) ? gamutProximities[(int)index1].Gamut : gamutProximities[(int)index2].Gamut;
			}
			return rval;
		}

		/// <summary>
		/// The returned ModeSegment contains GamutTrk objects that all have the same Gamut.Mode.AbsolutePitchHierarchy.
		/// </summary>
		private ModeSegment GetBasicModeSegment(int rootOctave, int gamutBasePitch, int relativePitchHierarchyIndex)
		{
			//const int gamutBasePitch = 9;
			var gamutTrks = new List<GamutTrk>();

			for(int i = 0, nPitchesPerOctave = 12; nPitchesPerOctave >= 1; --nPitchesPerOctave, ++i) // domain is both Gamut.PitchesPerOctave and nChords per GamutTrk
			{
				Gamut gamut = new Gamut(relativePitchHierarchyIndex, gamutBasePitch, nPitchesPerOctave);

				//pitchesPerChord = 5;
				//msDurationPerChord = 200;
				//velocityFactor = 0.5;
				GamutTrk gamutTrk = new GamutTrk(gamut, rootOctave, gamut.NPitchesPerOctave, 5, 200, 0.5);
				int minMsDuration = 230;
				int maxMsDuration = 380;
				gamutTrk.SetDurationsFromPitches(maxMsDuration, minMsDuration, true);

				gamutTrk.SortRootNotatedPitchAscending();

				gamutTrks.Add(gamutTrk);
			}

			ModeSegment basicModeSegment = new ModeSegment(gamutTrks);

			return (basicModeSegment);
		}

		protected ModeSegment Compose(ModeSegment paletteModeSegment)
		{
			var pModeSegment = paletteModeSegment.Clone();

			for(int index = 0; index < pModeSegment.Count; ++index)
			{
				GamutTrk gamutTrk = pModeSegment[index];

				#region current version
				if(gamutTrk.UniqueDefs[0] is MidiChordDef firstMcd && gamutTrk.UniqueDefs[gamutTrk.UniqueDefs.Count - 1] is MidiChordDef lastMcd)
				{
					gamutTrk.Shear(0, -1 * (gamutTrk.Gamut.NPitchesPerOctave));

					while(lastMcd.NotatedMidiPitches[0] % 12 != (firstMcd.NotatedMidiPitches[0] % 12))
					{
						gamutTrk.Shear(0, -1);
					}
				}

				var velocityPerAbsolutePitch = RootGamut.GetVelocityPerAbsolutePitch();
				gamutTrk.SetVelocityPerAbsolutePitch(velocityPerAbsolutePitch);

				double minHairpin = 0.5;
				double maxHairpin = 1.3;
				int indexMax;
				if(gamutTrk.Count > 1)
				{
					if(index % 2 != 0)
					{
						gamutTrk.Permute(1, 7);
						indexMax = gamutTrk.Count / 3;
						if(indexMax > 0 && indexMax < gamutTrk.Count)
						{
							gamutTrk.AdjustVelocitiesHairpin(0, indexMax, minHairpin, maxHairpin);
							gamutTrk.AdjustVelocitiesHairpin(indexMax, gamutTrk.Count, maxHairpin, minHairpin);
						}
					}
					else
					{
						indexMax = (gamutTrk.Count * 2) / 3;
						if(indexMax > 0 && indexMax < gamutTrk.Count)
						{
							gamutTrk.AdjustVelocitiesHairpin(0, indexMax, minHairpin, maxHairpin);
							gamutTrk.AdjustVelocitiesHairpin((gamutTrk.Count * 2) / 3, gamutTrk.Count, maxHairpin, minHairpin);
						}
					}
				}

				//gamutTrk.AdjustChordMsDurations(factor: 5);
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

				#region begin test code 5, related GamutTrks
				//if(domain % 2 != 0 && g.Count > 1)
				//{
				//    TenorPaletteGamutTrk previousTpg = (TenorPaletteGamutTrk)gamutTrks[i - 1];
				//    //g = previousTpg.RelatedPitchHierarchyGamutTrk(previousTpg.Gamut.RelativePitchHierarchyIndex + 11);
				//    //g = previousTpg.RelatedBasePitchGamutTrk(11);
				//    g = previousTpg.RelatedDomainGamutTrk(6);
				//}
				#endregion

				#region begin test code 6, timeWarp
				//if(gamutTrk.Count > 1)
				//{
				//	gamutTrk.TimeWarp(new Envelope(new List<byte>() { 4, 7, 2 }, 7, 7, gamutTrk.Count), 20);
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
				//    TenorPaletteGamutTrk prevTpg = (TenorPaletteGamutTrk)gamutTrks[i - 1];
				//    Gamut prevGamut = prevTpg.Gamut;
				//    g = new TenorPaletteGamutTrk(prevGamut); // identical to prevTpg
				//    // inverse velocityPerAbsolutePitch
				//    List<byte> velocityPerAbsolutePitch = prevGamut.GetVelocityPerAbsolutePitch(20, 127, prevGamut.NPitchesPerOctave - 1);
				//    g.SetVelocityPerAbsolutePitch(velocityPerAbsolutePitch, 20);
				//}
				#endregion

				#region begin test code 8, set Gamut (pitches
				//if(domain % 2 != 0 && g.Count > 1)
				//{
				//    TenorPaletteGamutTrk prevTpg = (TenorPaletteGamutTrk)gamutTrks[i - 1];
				//    Gamut prevGamut = prevTpg.Gamut;
				//    g = new TenorPaletteGamutTrk(prevGamut); // identical to prevTpg

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
			return pModeSegment;
		}

		internal void AdjustForTwoVoices()
		{
			throw new NotImplementedException();
		}

		internal void AdjustForThreeVoices()
		{
			throw new NotImplementedException();
		}

		internal void AdjustForFourVoices()
		{
			throw new NotImplementedException();
		}

		public Gamut RootGamut { get; }
	}
}