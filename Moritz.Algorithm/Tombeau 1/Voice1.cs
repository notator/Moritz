using System;
using System.Collections.Generic;
using System.Diagnostics;
using Krystals4ObjectLibrary;
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
		public Voice1(Tombeau1Algorithm.Tombeau1Type tombeau1Type, int midiChannel, Envelope centredEnvelope, Envelope basedEnvelope)
			: base(midiChannel)
		{
			int relativePitchHierarchyIndex = 0;
			int basePitch = 9;
			int nPitchesPerOctave = 12;

			RootMode = new Mode(relativePitchHierarchyIndex, basePitch, nPitchesPerOctave);

			_modeSegments = GetModeSegments(tombeau1Type, centredEnvelope, basedEnvelope);

			GlobalAdjustments();
		 }

		private void GlobalAdjustments()
		{
			foreach(ModeSegment modeSegment in _modeSegments)
			{
				foreach(ModeGrpTrk modeGrpTrk in modeSegment.ModeGrpTrks)
				{
					/// raise the velocities globally in the whole piece
					modeGrpTrk.AdjustVelocities(0, modeGrpTrk.Count, 20, 55);
				}
			}

		}

		private List<ModeSegment> GetModeSegments(Tombeau1Algorithm.Tombeau1Type tombeau1Type, Envelope centredEnvelope, Envelope basedEnvelope)
		{
			int nModeSegments = tombeau1Type.NModeSegments;
			List<ModeProximity> modeProximities = RootMode.GetModeProximities();
			List<int> modeIndices = GetModeIndices(nModeSegments, modeProximities);

			BasicModeSegments = GetBasicModeSegments(tombeau1Type, modeProximities, modeIndices);
			List<ModeSegment> modeSegments = GetBasicModeSegments(tombeau1Type, modeProximities, modeIndices);

			TimeWarpPerIUDEnvelopePerModeSegment = GetTimeWarpPerIUDEnvelopesPerModeSegment(BasicModeSegments, centredEnvelope);
			AbsPitchPerModeGrpTrkEnvelopePerModeSegment = GetAbsPitchPerModeGrpTrkEnvelopesPerModeSegment(BasicModeSegments, basedEnvelope);

			for(int i = 0; i < nModeSegments; i++)
			{
				ModeSegment modeSegment = modeSegments[i];
				Envelope timeWarpPerIUDEnvelope = TimeWarpPerIUDEnvelopePerModeSegment[i];
				Envelope absPitchPerModeGrpTrkEnvelope = AbsPitchPerModeGrpTrkEnvelopePerModeSegment[i];

				AdjustPitches(modeSegment, absPitchPerModeGrpTrkEnvelope);

				AdjustDurations(modeSegment, timeWarpPerIUDEnvelope, timeWarpDistortion: ((double)i / 1.5) + 5);

				AdjustVelocities(modeSegment);

				SetModeSegmentMsPositionsReContainer(modeSegments);
			}

			return modeSegments;
		}

		private List<ModeSegment> GetBasicModeSegments(Tombeau1Algorithm.Tombeau1Type tombeau1Type, List<ModeProximity> modeProximities, List<int> modeIndices)
		{
			int nModeSegments = tombeau1Type.NModeSegments;
			int maxChordsPerModeGrpTrk = tombeau1Type.MaxChordsPerModeGrpTrk;
			int nModeGrpTrksPerModeSegment = tombeau1Type.NModeGrpTrksPerModeSegment;
			int rootOctave = 2;
			List<ModeSegment> modeSegments = new List<ModeSegment>();

			for(int i = 0; i < nModeSegments; i++)
			{
				Mode mode = FindBaseMode(modeProximities, RootMode.BasePitch, modeIndices[i]);
				ModeSegment modeSegment = GetBasicModeSegment(maxChordsPerModeGrpTrk, nModeGrpTrksPerModeSegment, rootOctave, mode.BasePitch, mode.RelativePitchHierarchyIndex);

				if(i % 2 == 1)
				{
					modeSegment.Reverse();
				}
				modeSegments.Add(modeSegment);

				SetModeSegmentMsPositionsReContainer(modeSegments);
			}

			#region remove last ModeGrpTrk in last modeSegment
			ModeSegment lastModeSegment = modeSegments[modeSegments.Count - 1];
			int midiChannel = lastModeSegment.ModeGrpTrks[0].MidiChannel;
			int msPos = lastModeSegment.MsPositionReContainer;
			List<ModeGrpTrk> lastModeGrpTrks = new List<ModeGrpTrk>(lastModeSegment.ModeGrpTrks);
			lastModeGrpTrks.RemoveAt(lastModeGrpTrks.Count - 1);
			ModeSegment newLastModeSegment = new ModeSegment(midiChannel, msPos, lastModeGrpTrks);

			modeSegments.Remove(lastModeSegment);
			modeSegments.Add(newLastModeSegment);
			#endregion

			return modeSegments;
		}

		/// <summary>
		/// The returned envelopes have range [1..127], and are centred around value 64.
		/// </summary>
		private List<Envelope> GetTimeWarpPerIUDEnvelopesPerModeSegment(List<ModeSegment> modeSegments, Envelope centredEnvelope)
		{
			int nAllIUDs = 0;
			foreach(ModeSegment modeSegment in modeSegments)
			{
				nAllIUDs += modeSegment.IUDCount;
			}

			List<Envelope> timeWarpEnvelopes = new List<Envelope>();
			int domain = centredEnvelope.Domain;
			Envelope envelopeClone = centredEnvelope.Clone();
			envelopeClone.SetCount(nAllIUDs);
			Debug.Assert(domain > 0); // 0 --> 1 below.
			List<int> allValues = envelopeClone.Original;
			for(int i = 0; i < allValues.Count; i++)
			{
				int val = allValues[i];
				allValues[i] = (val == 0) ? 1 : val;
			}

			int startIndex = 0;
			for(int i = 0; i < modeSegments.Count; i++)
			{
				int nValuesToCopy = modeSegments[i].IUDCount;
				List<int> values = allValues.GetRange(startIndex, nValuesToCopy);
				startIndex += nValuesToCopy;
				Envelope timeWarpEnvelope = new Envelope(values, domain, domain, values.Count);
				timeWarpEnvelopes.Add(timeWarpEnvelope);
			}

			#region check range
			foreach(Envelope timeWarpEnvelope in timeWarpEnvelopes)
			{
				foreach(int val in timeWarpEnvelope.Original)
				{
					Debug.Assert(val >= 1 && val <= 127);
				}
			}
			#endregion

			return timeWarpEnvelopes;
		}

		/// <summary>
		/// The returned envelopes have range [0..11]
		/// </summary>
		/// <returns></returns>
		private List<Envelope> GetAbsPitchPerModeGrpTrkEnvelopesPerModeSegment(List<ModeSegment> modeSegments, Envelope basedEnvelope)
		{
			int nAllModeGrpTrks = 0;
			foreach(ModeSegment modeSegment in modeSegments)
			{
				nAllModeGrpTrks += modeSegment.ModeGrpTrks.Count;
			}
			List<Envelope> envelopes = new List<Envelope>();
			Envelope allModeGrpTrksEnvelope = new Envelope(basedEnvelope.Original, basedEnvelope.Domain, 11, nAllModeGrpTrks);
			List<int> allValues = allModeGrpTrksEnvelope.Original;
			int startIndex = 0;
			for(int i = 0; i < modeSegments.Count; i++)
			{
				int nValuesToCopy = modeSegments[i].ModeGrpTrks.Count;
				List<int> values = allValues.GetRange(startIndex, nValuesToCopy);
				startIndex += nValuesToCopy;
				Envelope env = new Envelope(values, allModeGrpTrksEnvelope.Domain, 11, values.Count);
				envelopes.Add(env);
			}

			#region check range
			foreach(Envelope envelope in envelopes)
			{
				foreach(int val in envelope.Original)
				{
					Debug.Assert(val >= 0 && val <= 11);
				}
			}
			#endregion

			return envelopes;
		}

		private static void AdjustDurations(ModeSegment modeSegment, Envelope timeWarpPerIUDEnvelope, double timeWarpDistortion)
		{
			int originalShortestMsDuration = modeSegment.ShortestIUDMsDuration;

			modeSegment.TimeWarpIUDs(timeWarpPerIUDEnvelope, timeWarpDistortion);

			int newShortestMsDuration = modeSegment.ShortestIUDMsDuration;

			double factor = ((double)originalShortestMsDuration) / newShortestMsDuration;
			Debug.WriteLine($"factor={factor}");

			modeSegment.MsDuration = (int)(modeSegment.MsDuration * factor);
		}

		private static List<int> GetModeIndices(int nModeSegments, List<ModeProximity> modeProximities)
		{
			int nModeProximities = modeProximities.Count;
			List<int> modeIndices = new List<int>();

			int fib1 = 1;
			int fib2 = 2;
			modeIndices.Add(fib1 - 1);
			modeIndices.Add(fib2 - 1);
			int nIndices = nModeSegments - 2;
			if(nIndices > 0)
			{
				for(int i = 0; i < nIndices; i++)
				{
					fib1 = modeIndices[i] + 1;
					fib2 = modeIndices[i + 1] + 1;
					modeIndices.Add((fib1 + fib2 - 1) % nModeProximities);
				}
			}
			return modeIndices;
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
		/// The returned ModeSegment contains nModeGrpTrks ModeGrpTrk objects that all have the same Mode.Mode.AbsolutePitchHierarchy.
		/// </summary>
		private ModeSegment GetBasicModeSegment(int maxChordsPerModeGrpTrk, int nModeGrpTrks, int rootOctave, int basePitch, int relativePitchHierarchyIndex)
		{
			var ModeGrpTrks = new List<ModeGrpTrk>();
			int msPositionReContainer = 0;
			for(int i = 0; i < nModeGrpTrks; i++)
			{
				int nChords = maxChordsPerModeGrpTrk - i;
				int nPitchesPerOctave = nChords;
				Mode mode = new Mode(relativePitchHierarchyIndex, basePitch, nPitchesPerOctave);
				ModeGrpTrk ModeGrpTrk = new ModeGrpTrk(this.MidiChannel, msPositionReContainer, new List<IUniqueDef>(), mode, rootOctave);

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

			ModeSegment basicModeSegment = new ModeSegment(this.MidiChannel, 0, ModeGrpTrks);

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

		private void AdjustPitches(ModeSegment modeSegment, Envelope absPitchPerModeGrpTrkEnvelope)
		{
			IReadOnlyList<ModeGrpTrk> modeGrpTrks = modeSegment.ModeGrpTrks;
			Debug.Assert(modeGrpTrks.Count == absPitchPerModeGrpTrkEnvelope.Original.Count);
			var absPitchHierarchy = modeGrpTrks[0].Mode.AbsolutePitchHierarchy;
			var absPitchList = absPitchPerModeGrpTrkEnvelope.Original;

			for(int index = 0; index < modeGrpTrks.Count; ++index)
			{
				ModeGrpTrk modeGrpTrk = modeGrpTrks[index];
				int nPitchesPerOctave = modeGrpTrk.Mode.NPitchesPerOctave;
				int absPitchIndex = (int) Math.Floor((double)absPitchList[index] * nPitchesPerOctave / 12);
				Debug.Assert(absPitchIndex < nPitchesPerOctave);

				MidiChordDef lastMcd = modeGrpTrk.LastMidiChordDef;
				MidiChordDef firstMcd = modeGrpTrk.FirstMidiChordDef;

				byte toPitch = (byte) absPitchHierarchy[absPitchIndex];
				#region check toPitch
				bool found = false;
				foreach(int val in modeGrpTrk.Mode.Gamut)
				{
					if(toPitch == val || (toPitch + 12) == val)
					{
						found = true;
						break;	
					}
				}
				Debug.Assert(found);
				#endregion

				if(firstMcd != lastMcd)
				{
					modeGrpTrk.Shear(0, -1 * (modeGrpTrk.Mode.NPitchesPerOctave));

					while(lastMcd.NotatedMidiPitches[0] % 12 != toPitch)
					{
						Debug.Assert(lastMcd.NotatedMidiPitches[0] > toPitch);

						Debug.WriteLine($"apIndex={absPitchIndex}, p={lastMcd.NotatedMidiPitches[0]}, pMod12={lastMcd.NotatedMidiPitches[0] % 12}, to={toPitch}");
						modeGrpTrk.Shear(0, -1);
					}
				}

				if(index % 2 != 0)
				{
					modeGrpTrk.Permute(1, 7);
				}

				#region test code
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
				//if(ModeGrpTrk.Count > 1)
				//{
				//	ModeGrpTrk.TimeWarp(new Envelope(new List<byte>() { 4, 7, 2 }, 7, 7, ModeGrpTrk.Count), distortion: 4);
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
				#endregion
			}
		}

		private void AdjustVelocities(ModeSegment modeSegment)
		{
			IReadOnlyList<ModeGrpTrk> modeGrpTrks = modeSegment.ModeGrpTrks;
			double minHairpin = 0.5;
			double maxHairpin = 1.3;
			ModeGrpTrk modeGrpTrk = null;
			for(int index = 0; index < modeGrpTrks.Count; ++index)
			{
				modeGrpTrk = modeGrpTrks[index];
				Mode mode = modeGrpTrk.Mode;
				var velocityPerAbsolutePitch = mode.GetDefaultVelocityPerAbsolutePitch();
				//velocityPerAbsolutePitch = Mode.SetVelocityPerAbsolutePitchGradient(velocityPerAbsolutePitch, 1);
				velocityPerAbsolutePitch = Mode.SetVelocityPerAbsolutePitchRange(velocityPerAbsolutePitch, 20, 127);
				modeGrpTrk.SetVelocityPerAbsolutePitch(velocityPerAbsolutePitch);

				int indexMax;
				if(modeGrpTrk.Count > 1)
				{
					if(index % 2 != 0)
					{
						indexMax = modeGrpTrk.Count / 3;
					}
					else
					{
						indexMax = (modeGrpTrk.Count * 2) / 3;
					}
					if(indexMax > 0 && indexMax < modeGrpTrk.Count)
					{
						modeGrpTrk.AdjustVelocitiesHairpin(0, indexMax, minHairpin, maxHairpin);
						modeGrpTrk.AdjustVelocitiesHairpin(indexMax, modeGrpTrk.Count, maxHairpin, minHairpin);
					}
				}

				#region test code
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
				//if(ModeGrpTrk.Count > 1)
				//{
				//	ModeGrpTrk.TimeWarp(new Envelope(new List<byte>() { 4, 7, 2 }, 7, 7, ModeGrpTrk.Count), distortion: 4);
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
				#endregion

			}
		}

		internal void AdjustForTwoVoices()
		{
			//throw new NotImplementedException();
		}

		internal void AdjustForThreeVoices()
		{
			//throw new NotImplementedException();
		}

		internal void AdjustForFourVoices()
		{
			throw new NotImplementedException();
		}

		public override List<int> BarlineMsPositions()
		{
			List<int> v1BarlinePositions = new List<int>();

			var msValuesListList = GetMsValuesOfModeGrpTrks();

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
		public List<ModeSegment> BasicModeSegments { get; private set; }
		public List<Envelope> TimeWarpPerIUDEnvelopePerModeSegment { get; private set; }
		public List<Envelope> AbsPitchPerModeGrpTrkEnvelopePerModeSegment { get; private set; }
	}
}