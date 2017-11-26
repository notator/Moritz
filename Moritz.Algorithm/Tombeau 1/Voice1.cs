using System;
using System.Collections.Generic;
using System.Diagnostics;
using Moritz.Spec;

namespace Moritz.Algorithm.Tombeau1
{
	#region available Trk and GamutGrpTrk transformations
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
	#endregion available Trk and GamutGrpTrk transformations

	internal class Voice1 : Tombeau1Voice
    {
		public Voice1(int midiChannel)
			: base(midiChannel)
		{
			int relativePitchHierarchyIndex = 0;
			int basePitch = 9;
			int nPitchesPerOctave = 12;

			RootGamut = new Gamut(relativePitchHierarchyIndex, basePitch, nPitchesPerOctave);

			_modeSegments = GetModeSegments(20);
		 }

		private List<ModeSegment> GetModeSegments(int nModeSegments)
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
			List<ModeSegment> modeSegments = new List<ModeSegment>();
			int msPositionReContainer = 0;
			foreach(var gamut in baseGamuts)
			{
				ModeSegment modeSegment = GetBasicModeSegment(msPositionReContainer, rootOctave, gamut.BasePitch, gamut.RelativePitchHierarchyIndex);
				modeSegments.Add(modeSegment);
				msPositionReContainer += modeSegment.MsDuration;
			}

			for(int i = 0; i < modeSegments.Count; ++i)
			{
				ModeSegment modeSegment = modeSegments[i];

				if(i % 2 == 1)
				{
					modeSegment.Reverse();
				}

				modeSegments[i] = Compose(modeSegment);

				SetModeSegmentMsPositionsReContainer(modeSegments);
			} 
			return modeSegments;
		}

		private void SetModeSegmentMsPositionsReContainer(List<ModeSegment> modeSegments)
		{
			int msPos = 0;
			foreach(ModeSegment modeSegment in modeSegments)
			{
				modeSegment.MsPositionReContainer = msPos;
				msPos += modeSegment.MsDuration;
			}
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
		/// The returned ModeSegment contains GamutGrpTrk objects that all have the same Gamut.Mode.AbsolutePitchHierarchy.
		/// </summary>
		private ModeSegment GetBasicModeSegment(int modeSegmentMsPositionReContainer, int rootOctave, int gamutBasePitch, int relativePitchHierarchyIndex)
		{
			const int maxChordsPerGamutGrpTrk = 12;
			const int minChordsPerGamutGrpTrk = 4;
			var gamutGrpTrks = new List<GamutGrpTrk>();
			int msPositionReContainer = 0;
			for(int nChords = maxChordsPerGamutGrpTrk; nChords >= minChordsPerGamutGrpTrk; --nChords)
			{
				int nPitchesPerOctave = nChords;
				Gamut gamut = new Gamut(relativePitchHierarchyIndex, gamutBasePitch, nPitchesPerOctave);

				GamutGrpTrk gamutGrpTrk = new GamutGrpTrk(this.MidiChannel, msPositionReContainer, new List<IUniqueDef>(), gamut, rootOctave);

				int pitchesPerChord = 5;
				int totalMsDuration = nChords * 200;
				List<IUniqueDef> iUniqueDefs = GetIUniqueDefs(gamutGrpTrk.Gamut, gamutGrpTrk.RootOctave, nChords, pitchesPerChord, totalMsDuration);
				gamutGrpTrk.SetIUniqueDefs(iUniqueDefs);

				int minMsDuration = 230;
				int maxMsDuration = 380;
				gamutGrpTrk.SetDurationsFromPitches(maxMsDuration, minMsDuration, true);

				gamutGrpTrk.SortRootNotatedPitchAscending();

				gamutGrpTrks.Add(gamutGrpTrk);

				msPositionReContainer += gamutGrpTrk.MsDuration;
			}

			ModeSegment basicModeSegment = new ModeSegment(this.MidiChannel, modeSegmentMsPositionReContainer, gamutGrpTrks);

			return (basicModeSegment);
		}

		/// <summary>
		/// Composes an IUniqueDefs list using gamut and rootOctave.
		/// </summary>
		public List<IUniqueDef> GetIUniqueDefs(Gamut gamut, int rootOctave, int nChords, int nPitchesPerChord, int totalMsDuration)
		{
			Debug.Assert(nChords > 0);
			Debug.Assert(nPitchesPerChord > 0);
			int msDurationPerChord = totalMsDuration / nChords;
			Debug.Assert(msDurationPerChord > 0);

			List<IUniqueDef> uniqueDefs = new List<IUniqueDef>();
			for(int i = 0; i < nChords; ++i)
			{
				int rootNotatedPitch;
				if(i == 0)
				{
					rootNotatedPitch = gamut.Mode.AbsolutePitchHierarchy[i] + (12 * rootOctave);
					rootNotatedPitch = (rootNotatedPitch <= gamut.MaxPitch) ? rootNotatedPitch : gamut.MaxPitch;
				}
				else
				{
					List<byte> previousPitches = ((MidiChordDef)uniqueDefs[i - 1]).BasicMidiChordDefs[0].Pitches;
					if(previousPitches.Count > 1)
					{
						rootNotatedPitch = previousPitches[1];
					}
					else
					{
						rootNotatedPitch = gamut.Mode.AbsolutePitchHierarchy[i];
						while(rootNotatedPitch < previousPitches[0])
						{
							rootNotatedPitch += 12;
							if(rootNotatedPitch > gamut.MaxPitch)
							{
								rootNotatedPitch = gamut.MaxPitch;
								break;
							}
						}
					}
				}
				MidiChordDef mcd = new MidiChordDef(msDurationPerChord, gamut, rootNotatedPitch, nPitchesPerChord, null);
				uniqueDefs.Add(mcd);
			}

			Trk tempTrk = new Trk(0, 0, uniqueDefs)
			{
				MsDuration = totalMsDuration // correct rounding errors
			};

			return tempTrk.UniqueDefs;
		}

		protected ModeSegment Compose(ModeSegment modeSegment)
		{
			for(int index = 0; index < modeSegment.Count; ++index)
			{
				GamutGrpTrk gamutGrpTrk = modeSegment[index];

				MidiChordDef lastMcd = gamutGrpTrk.LastMidiChordDef;
				MidiChordDef firstMcd = gamutGrpTrk.FirstMidiChordDef;

				#region current version
				if(firstMcd != lastMcd)
				{
					gamutGrpTrk.Shear(0, -1 * (gamutGrpTrk.Gamut.NPitchesPerOctave));

					while(lastMcd.NotatedMidiPitches[0] % 12 != (firstMcd.NotatedMidiPitches[0] % 12))
					{
						gamutGrpTrk.Shear(0, -1);
					}
				}

				var velocityPerAbsolutePitch = RootGamut.GetVelocityPerAbsolutePitch();
				gamutGrpTrk.SetVelocityPerAbsolutePitch(velocityPerAbsolutePitch);

				double minHairpin = 0.5;
				double maxHairpin = 1.3;
				int indexMax;
				if(gamutGrpTrk.Count > 1)
				{
					if(index % 2 != 0)
					{
						gamutGrpTrk.Permute(1, 7);
						indexMax = gamutGrpTrk.Count / 3;
						if(indexMax > 0 && indexMax < gamutGrpTrk.Count)
						{
							gamutGrpTrk.AdjustVelocitiesHairpin(0, indexMax, minHairpin, maxHairpin);
							gamutGrpTrk.AdjustVelocitiesHairpin(indexMax, gamutGrpTrk.Count, maxHairpin, minHairpin);
						}
					}
					else
					{
						indexMax = (gamutGrpTrk.Count * 2) / 3;
						if(indexMax > 0 && indexMax < gamutGrpTrk.Count)
						{
							gamutGrpTrk.AdjustVelocitiesHairpin(0, indexMax, minHairpin, maxHairpin);
							gamutGrpTrk.AdjustVelocitiesHairpin((gamutGrpTrk.Count * 2) / 3, gamutGrpTrk.Count, maxHairpin, minHairpin);
						}
					}
				}

				#endregion current version

				#region test code 1 insert a rest
				//modeSegment[index].Insert(1, new MidiRestDef(0, 200));
				//modeSegment.SetMsPositionsReThisModeSegment();
				#endregion

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

				#region begin test code 5, related GamutGrpTrks
				//if(domain % 2 != 0 && g.Count > 1)
				//{
				//    TenorPaletteGamutTrk previousTpg = (TenorPaletteGamutTrk)gamutGrpTrks[i - 1];
				//    //g = previousTpg.RelatedPitchHierarchyGamutTrk(previousTpg.Gamut.RelativePitchHierarchyIndex + 11);
				//    //g = previousTpg.RelatedBasePitchGamutTrk(11);
				//    g = previousTpg.RelatedDomainGamutTrk(6);
				//}
				#endregion

				#region begin test code 6, timeWarp
				//if(gamutGrpTrk.Count > 1)
				//{
				//	gamutGrpTrk.TimeWarp(new Envelope(new List<byte>() { 4, 7, 2 }, 7, 7, gamutGrpTrk.Count), 20);
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
				//    TenorPaletteGamutTrk prevTpg = (TenorPaletteGamutTrk)gamutGrpTrks[i - 1];
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
				//    TenorPaletteGamutTrk prevTpg = (TenorPaletteGamutTrk)gamutGrpTrks[i - 1];
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
			return modeSegment;
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

		public override List<int> BarlineMsPositions()
		{
			List<int> v1BarlinePositions = new List<int>();

			var msValuesListList = GetMsValuesOfGamutTrks();

			for(int i = 0; i < msValuesListList.Count; ++i)
			{
				var msValuesList = msValuesListList[i];
				MsValues lastMsValues = msValuesList[msValuesList.Count - 1];

				v1BarlinePositions.Add(lastMsValues.EndMsPosition);
			}

			#region insert intermediate barline positions
			//List<int> midBarlinePositions = new List<int>
			//{
			//	MidBarlineMsPos(msValuesListList, 1),
			//	MidBarlineMsPos(msValuesListList, 10),
			//	MidBarlineMsPos(msValuesListList, 12)
			//};
			//foreach(int b in midBarlinePositions)
			//{
			//	v1BarlinePositions.Add(b);
			//}
			#endregion

			return v1BarlinePositions;
		}

		public Gamut RootGamut { get; }
	}
}