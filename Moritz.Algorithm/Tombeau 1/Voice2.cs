using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Moritz.Spec;

namespace Moritz.Algorithm.Tombeau1
{
    internal class Voice2 : Tombeau1Voice
    {
		public Voice2(Voice1 voice1)
			: base()
        {
			_composedModeSegments = Compose(voice1);
		}

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

		#region old code
		//    /// <summary>
		//    /// Creates a list of TenorPaletteGamutTrks, each of which has the same relativePitchHierarchyIndex.
		//    /// </summary>
		//    private List<GamutTrk> GetTenorPaletteGamutTrkList(int relativePitchHierarchyIndex)
		//    {
		//        const int gamutBasePitch = 0;
		//        List<GamutTrk> gamutTrks = new List<GamutTrk>();

		//        for(int i = 0, domain = 12; domain >= 1; --domain, ++i) // domain is both Gamut.PitchesPerOctave and nChords per GamutTrk
		//        {
		//            Gamut gamut = new Gamut(relativePitchHierarchyIndex, gamutBasePitch, domain);

		//            PaletteGamutTrk tpg = new PaletteGamutTrk(gamut, 3);
		//int minMsDuration = 200;
		//int maxMsDuration = 300;
		//tpg.SetDurationsFromPitches(maxMsDuration, minMsDuration, true);

		//#region begin test code 1 Shear and permute
		//tpg.Shear(0, -1 * (gamut.NPitchesPerOctave));
		//            tpg.SetVelocityPerAbsolutePitch(gamut);

		//            if(domain % 2 != 0)
		//            {
		//                tpg.Permute(1, 7);
		//            }
		//#endregion end test code 1

		//#region begin test code 2 transpose chords to the same absolute root pitch
		////for(int iudIndex = 0; iudIndex < tpg.Count; ++iudIndex)
		////{
		////    tpg.TransposeChordDownToAbsolutePitch(iudIndex, 0);
		////}
		//#endregion end test code 2

		//#region begin test code 3, adjust velocities
		////if(domain % 2 != 0)
		////{
		////    tpg.AdjustVelocities(0.5);
		////}
		//#endregion

		//#region begin test code 4, adjust velocities
		////if(domain % 2 != 0 && tpg.Count > 1)
		////{
		////	tpg.AdjustVelocitiesHairpin(0, (tpg.Count - 1) / 2, 0.5, 1.0);
		////	tpg.AdjustVelocitiesHairpin((tpg.Count - 1) / 2, tpg.Count - 1, 1.0, 0.5);
		////}
		//#endregion

		//#region begin test code 5, related GamutTrks
		////if(domain % 2 != 0 && tpg.Count > 1)
		////{
		////    TenorPaletteGamutTrk previousTpg = (TenorPaletteGamutTrk)gamutTrks[i - 1];
		////    //tpg = previousTpg.RelatedPitchHierarchyGamutTrk(previousTpg.Gamut.RelativePitchHierarchyIndex + 11);
		////    //tpg = previousTpg.RelatedBasePitchGamutTrk(11);
		////    tpg = previousTpg.RelatedDomainGamutTrk(6);
		////}
		//#endregion

		//#region begin test code 6, timeWarp
		////if(domain % 2 != 0 && tpg.Count > 1)
		////{
		////    tpg.TimeWarp(new Envelope(new List<int>() { 4, 7, 2 }, 7, 7, tpg.Count), 20);
		////}
		//#endregion

		//#region begin test code 7, SetPitchWheelSliders
		////Envelope env = new Envelope(new List<int>() { 0,8 }, 8, 127, tpg.Count);
		////tpg.SetPitchWheelSliders(env);
		//#endregion

		//#region begin test code 8, SetPanGliss
		////if(tpg.Count > 1)
		////{
		////    if(domain % 2 != 0)
		////    {
		////        tpg.SetPanGliss(0, tpg.Count - 1, 127, 0);
		////    }
		////    else
		////    {
		////        tpg.SetPanGliss(0, tpg.Count - 1, 0, 127);
		////    }
		////}
		//#endregion

		//#region begin test code 8, set inverse velocities
		////if(domain % 2 != 0 && tpg.Count > 1)
		////{
		////    TenorPaletteGamutTrk prevTpg = (TenorPaletteGamutTrk)gamutTrks[i - 1];
		////    Gamut prevGamut = prevTpg.Gamut;
		////    tpg = new TenorPaletteGamutTrk(prevGamut); // identical to prevTpg
		////    // inverse velocityPerAbsolutePitch
		////    List<byte> velocityPerAbsolutePitch = prevGamut.GetVelocityPerAbsolutePitch(20, 127, prevGamut.NPitchesPerOctave - 1);
		////    tpg.SetVelocityPerAbsolutePitch(velocityPerAbsolutePitch, 20);
		////}
		//#endregion

		//#region begin test code 8, set Gamut (pitches
		////if(domain % 2 != 0 && tpg.Count > 1)
		////{
		////    TenorPaletteGamutTrk prevTpg = (TenorPaletteGamutTrk)gamutTrks[i - 1];
		////    Gamut prevGamut = prevTpg.Gamut;
		////    tpg = new TenorPaletteGamutTrk(prevGamut); // identical to prevTpg

		////    int newRelativePitchHierarchyIndex = prevGamut.RelativePitchHierarchyIndex + 11;
		////    int newBasePitch = prevGamut.BasePitch;
		////    int newNPitchesPerOctave = 8;
		////    Gamut gamut1 = new Gamut(newRelativePitchHierarchyIndex, newBasePitch, newNPitchesPerOctave);
		////    tpg.Gamut = gamut1; // sets the pitches, velocities are still those of the original pitches.

		////    // reverse the velocityperAbsolutePitch hierarchy re the prevGamut.
		////    List<byte> velocityPerAbsolutePitch = prevGamut.GetVelocityPerAbsolutePitch(20, 127, prevGamut.NPitchesPerOctave - 1);
		////    tpg.SetVelocityPerAbsolutePitch(velocityPerAbsolutePitch, 20);
		////}
		//#endregion

		//gamutTrks.Add(tpg);
		//        }

		//        return (gamutTrks);
		//    }
		#endregion

		private List<ModeSegment> Compose(Voice1 voice1)
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
	}
}