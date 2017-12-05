using System;
using System.Collections.Generic;
using System.Diagnostics;
using Moritz.Spec;

namespace Moritz.Algorithm.Tombeau1
{
	#region available Trk and ModeGrpTrk transformations
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
	// TransposeStepsInModeGamut();
	// TransposeToRootInModeGamut();
	#endregion available Trk and ModeGrpTrk transformations

	internal class Voice1 : Tombeau1Voice
    {
		public Voice1(int midiChannel)
			: base(midiChannel)
		{
			int relativePitchHierarchyIndex = 0;
			int basePitch = 9;
			int nPitchesPerOctave = 12;

			RootMode = new Mode(relativePitchHierarchyIndex, basePitch, nPitchesPerOctave);

			_modeSegments = GetModeSegments(20);
		 }

		private List<ModeSegment> GetModeSegments(int nModeSegments)
		{
			List<ModeProximity> modeProximities = RootMode.GetModeProximities();

			int maxIndex = modeProximities.Count - 1;
			int indexInc = maxIndex / nModeSegments;
			List<int> modeIndices = new List<int>();
			for(int i = 0; i < nModeSegments; i++)
			{
				modeIndices.Add(i * indexInc);
			}
			modeIndices[nModeSegments - 1] = maxIndex; // correct any rounding error.

			var baseModes = new List<Mode>();
			for(int i = 0; i < nModeSegments; i++)
			{
				Mode mode = FindBaseMode(modeProximities, RootMode.BasePitch, modeIndices[i]);
				//modeProximities = mode.GetModeProximities();
				baseModes.Add(mode);
			}

			int rootOctave = 2;
			List<ModeSegment> modeSegments = new List<ModeSegment>();
			int msPositionReContainer = 0;
			foreach(var mode in baseModes)
			{
				ModeSegment modeSegment = GetBasicModeSegment(msPositionReContainer, rootOctave, mode.BasePitch, mode.RelativePitchHierarchyIndex);
				modeSegments.Add(modeSegment);
				msPositionReContainer += modeSegment.MsDuration;
			}

			List<Envelope> warpEnvelopes = GetModeSegmentWarpEnvelopes(modeSegments);

			for(int i = 0; i < modeSegments.Count; ++i)
			{
				ModeSegment modeSegment = modeSegments[i];
				Envelope envelope = warpEnvelopes[i];

				int oShortestMsDuration = modeSegment.ShortestIUDMsDuration;

				if(i % 2 == 1)
				{
					modeSegment.Reverse();
				}

				AdjustModeGrpTrks(modeSegment, envelope);

				modeSegment.TimeWarpIUDs(envelope, distortion: (i / 2) + 1.2);

				int newShortestMsDuration = modeSegment.ShortestIUDMsDuration;

				double factor = ((double)oShortestMsDuration) / newShortestMsDuration;
				Debug.WriteLine($"factor={factor}");

				modeSegment.MsDuration =  (int)(modeSegment.MsDuration * factor);

				SetModeSegmentMsPositionsReContainer(modeSegments);
			} 
			return modeSegments;
		}

		private List<Envelope> GetModeSegmentWarpEnvelopes(List<ModeSegment> modeSegments)
		{
			List<List<byte>> byteLists = new List<List<byte>>()
			{
				new List<byte>() { 73, 127, 73 },
				new List<byte>() { 73, 127, 73 },

				new List<byte>() { 73, 127, 73 },
				new List<byte>() { 73, 127, 73 },

				new List<byte>() { 54, 127, 54 },
				new List<byte>() { 54, 127, 54 },

				new List<byte>() { 54, 127, 54 },
				new List<byte>() { 54, 127, 73 },

				new List<byte>() { 73, 54, 127, 54 },
				new List<byte>() { 73, 127, 73, 54 },

				new List<byte>() { 73, 127, 73, 127 },
				new List<byte>() { 127, 73, 127, 73 },

				new List<byte>() { 54, 73, 127, 73 },
				new List<byte>() { 73, 127, 73, 54 },

				new List<byte>() { 73, 91, 127, 54 },
				new List<byte>() { 54, 127, 91, 73 },

				new List<byte>() { 54, 109, 73, 91, 127 },
				new List<byte>() { 127, 91, 73, 109, 54 },

				new List<byte>() { 73, 127, 73 },
				new List<byte>() { 73, 127 }
			};
			Debug.Assert(byteLists.Count == modeSegments.Count);
			var rval = new List<Envelope>();
			for(int i = 0; i < modeSegments.Count; i++)
			{
				rval.Add(new Envelope(byteLists[i], 127, 127, modeSegments[i].IUDCount));
			}
			return rval;
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
		/// Find the nearest Mode to startIndex having BasePitch = basePitch.
		/// </summary>
		private Mode FindBaseMode(List<ModeProximity> modeProximities, int basePitch, int startIndex)
		{
			Mode rval = null;
			int? index1 = null;
			for(int index = startIndex; index >= 0; --index)
			{
				if(modeProximities[index].Mode.BasePitch == basePitch)
				{
					index1 = index;
					break;
				}
			}
			int? index2 = null;
			for(int index = startIndex + 1; index < modeProximities.Count; ++index)
			{
				if(modeProximities[index].Mode.BasePitch == basePitch)
				{
					index2 = index;
					break;
				}

			}
			if(index1 == null)
			{
				if(index2 != null)
				{
					rval = modeProximities[(int)index2].Mode;
				}
			}
			if(index2 == null)
			{
				if(index1 != null)
				{
					rval = modeProximities[(int)index1].Mode;
				}
			}
			else // neither index1 nor index2 is null
			{
				rval = ((startIndex - index1) < (index2 - startIndex)) ? modeProximities[(int)index1].Mode : modeProximities[(int)index2].Mode;
			}
			return rval;
		}

		/// <summary>
		/// The returned ModeSegment contains ModeGrpTrk objects that all have the same Mode.Mode.AbsolutePitchHierarchy.
		/// </summary>
		private ModeSegment GetBasicModeSegment(int modeSegmentMsPositionReContainer, int rootOctave, int basePitch, int relativePitchHierarchyIndex)
		{
			const int maxChordsPerModeGrpTrk = 12;
			const int minChordsPerModeGrpTrk = 3;
			var ModeGrpTrks = new List<ModeGrpTrk>();
			int msPositionReContainer = 0;
			for(int nChords = maxChordsPerModeGrpTrk; nChords >= minChordsPerModeGrpTrk; --nChords)
			{
				int nPitchesPerOctave = nChords;
				Mode mode = new Mode(relativePitchHierarchyIndex, basePitch, nPitchesPerOctave);
				ModeGrpTrk ModeGrpTrk = new ModeGrpTrk(this.MidiChannel, msPositionReContainer, new List<IUniqueDef>(), mode, rootOctave);

				var velocityPerAbsolutePitch = mode.GetDefaultVelocityPerAbsolutePitch();
				velocityPerAbsolutePitch = Mode.SetVelocityPerAbsolutePitchGradient(velocityPerAbsolutePitch, 1);
				velocityPerAbsolutePitch = Mode.SetVelocityPerAbsolutePitchRange(velocityPerAbsolutePitch, 20, 127);
				ModeGrpTrk.SetVelocityPerAbsolutePitch(velocityPerAbsolutePitch);

				int pitchesPerChord = 5;
				int totalMsDuration = nChords * 200;
				List<IUniqueDef> iUniqueDefs = GetIUniqueDefs(ModeGrpTrk.Mode, ModeGrpTrk.RootOctave, nChords, pitchesPerChord, totalMsDuration);
				ModeGrpTrk.SetIUniqueDefs(iUniqueDefs);

				int minMsDuration = 230;
				int maxMsDuration = 380;
				ModeGrpTrk.SetDurationsFromPitches(maxMsDuration, minMsDuration, true);

				ModeGrpTrk.SortRootNotatedPitchAscending();

				ModeGrpTrks.Add(ModeGrpTrk);

				msPositionReContainer += ModeGrpTrk.MsDuration;
			}

			ModeSegment basicModeSegment = new ModeSegment(this.MidiChannel, modeSegmentMsPositionReContainer, ModeGrpTrks);

			return (basicModeSegment);
		}

		/// <summary>
		/// Composes an IUniqueDefs list using mode and rootOctave.
		/// </summary>
		public List<IUniqueDef> GetIUniqueDefs(Mode mode, int rootOctave, int nChords, int nPitchesPerChord, int totalMsDuration)
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
					rootNotatedPitch = mode.AbsolutePitchHierarchy[i] + (12 * rootOctave);
					rootNotatedPitch = (rootNotatedPitch <= mode.MaxGamutPitch) ? rootNotatedPitch : mode.MaxGamutPitch;
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
						rootNotatedPitch = mode.AbsolutePitchHierarchy[i];
						while(rootNotatedPitch < previousPitches[0])
						{
							rootNotatedPitch += 12;
							if(rootNotatedPitch > mode.MaxGamutPitch)
							{
								rootNotatedPitch = mode.MaxGamutPitch;
								break;
							}
						}
					}
				}
				MidiChordDef mcd = new MidiChordDef(msDurationPerChord, mode, rootNotatedPitch, nPitchesPerChord, null);
				uniqueDefs.Add(mcd);
			}

			Trk tempTrk = new Trk(0, 0, uniqueDefs)
			{
				MsDuration = totalMsDuration // correct rounding errors
			};

			return tempTrk.UniqueDefs;
		}

		protected void AdjustModeGrpTrks(ModeSegment modeSegment, Envelope envelope)
		{
			IReadOnlyList<ModeGrpTrk> modeGrpTrks = modeSegment.ModeGrpTrks;
			for(int index = 0; index < modeGrpTrks.Count; ++index)
			{
				ModeGrpTrk ModeGrpTrk = modeGrpTrks[index];
																								   
				MidiChordDef lastMcd = ModeGrpTrk.LastMidiChordDef;
				MidiChordDef firstMcd = ModeGrpTrk.FirstMidiChordDef;

				#region current version
				if(firstMcd != lastMcd)
				{
					ModeGrpTrk.Shear(0, -1 * (ModeGrpTrk.Mode.NPitchesPerOctave));

					while(lastMcd.NotatedMidiPitches[0] % 12 != (firstMcd.NotatedMidiPitches[0] % 12))
					{
						ModeGrpTrk.Shear(0, -1);
					}
				}

				//var velocityPerAbsolutePitch = RootMode.GetVelocityPerAbsolutePitch();

				Mode mode = ModeGrpTrk.Mode;
				var velocityPerAbsolutePitch = mode.GetDefaultVelocityPerAbsolutePitch();
				//velocityPerAbsolutePitch = Mode.SetVelocityPerAbsolutePitchGradient(velocityPerAbsolutePitch, 1);
				velocityPerAbsolutePitch = Mode.SetVelocityPerAbsolutePitchRange(velocityPerAbsolutePitch, 20, 127);
				ModeGrpTrk.SetVelocityPerAbsolutePitch(velocityPerAbsolutePitch);

				double minHairpin = 0.5;
				double maxHairpin = 1.3;
				int indexMax;
				if(ModeGrpTrk.Count > 1)
				{
					if(index % 2 != 0)
					{
						ModeGrpTrk.Permute(1, 7);
						indexMax = ModeGrpTrk.Count / 3;
						if(indexMax > 0 && indexMax < ModeGrpTrk.Count)
						{
							ModeGrpTrk.AdjustVelocitiesHairpin(0, indexMax, minHairpin, maxHairpin);
							ModeGrpTrk.AdjustVelocitiesHairpin(indexMax, ModeGrpTrk.Count, maxHairpin, minHairpin);
						}
					}
					else
					{
						indexMax = (ModeGrpTrk.Count * 2) / 3;
						if(indexMax > 0 && indexMax < ModeGrpTrk.Count)
						{
							ModeGrpTrk.AdjustVelocitiesHairpin(0, indexMax, minHairpin, maxHairpin);
							ModeGrpTrk.AdjustVelocitiesHairpin((ModeGrpTrk.Count * 2) / 3, ModeGrpTrk.Count, maxHairpin, minHairpin);
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

				#region begin test code 5, related ModeGrpTrks
				//if(domain % 2 != 0 && g.Count > 1)
				//{
				//    TenorPaletteModeTrk previousTpg = (TenorPaletteModeTrk)ModeGrpTrks[i - 1];
				//    //g = previousTpg.RelatedPitchHierarchyModeTrk(previousTpg.Mode.RelativePitchHierarchyIndex + 11);
				//    //g = previousTpg.RelatedBasePitchModeTrk(11);
				//    g = previousTpg.RelatedDomainModeTrk(6);
				//}
				#endregion

				#region begin test code 6, timeWarp
				if(ModeGrpTrk.Count > 1)
				{
					ModeGrpTrk.TimeWarp(new Envelope(new List<byte>() { 4, 7, 2 }, 7, 7, ModeGrpTrk.Count), distortion: 4);
				}
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
				//    TenorPaletteModeTrk prevTpg = (TenorPaletteModeTrk)ModeGrpTrks[i - 1];
				//    Mode prevMode = prevTpg.Mode;
				//    g = new TenorPaletteModeTrk(prevMode); // identical to prevTpg
				//    // inverse velocityPerAbsolutePitch
				//    List<byte> velocityPerAbsolutePitch = prevMode.GetVelocityPerAbsolutePitch(20, 127, prevMode.NPitchesPerOctave - 1);
				//    g.SetVelocityPerAbsolutePitch(velocityPerAbsolutePitch, 20);
				//}
				#endregion

				#region begin test code 8, set Mode (pitches
				//if(domain % 2 != 0 && g.Count > 1)
				//{
				//    TenorPaletteModeTrk prevTpg = (TenorPaletteModeTrk)ModeGrpTrks[i - 1];
				//    Mode prevMode = prevTpg.Mode;
				//    g = new TenorPaletteModeTrk(prevMode); // identical to prevTpg

				//    int newRelativePitchHierarchyIndex = prevMode.RelativePitchHierarchyIndex + 11;
				//    int newBasePitch = prevMode.BasePitch;
				//    int newNPitchesPerOctave = 8;
				//    Mode mode1 = new Mode(newRelativePitchHierarchyIndex, newBasePitch, newNPitchesPerOctave);
				//    g.Mode = mode1; // sets the pitches, velocities are still those of the original pitches.

				//    // reverse the velocityperAbsolutePitch hierarchy re the prevMode.
				//    List<byte> velocityPerAbsolutePitch = prevMode.GetVelocityPerAbsolutePitch(20, 127, prevMode.NPitchesPerOctave - 1);
				//    g.SetVelocityPerAbsolutePitch(velocityPerAbsolutePitch, 20);
				//}
				#endregion

			}
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

			var msValuesListList = GetMsValuesOfModeTrks();

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

		public Mode RootMode { get; }
	}
}