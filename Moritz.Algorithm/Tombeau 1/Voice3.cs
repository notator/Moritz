using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Moritz.Spec;

using Krystals4ObjectLibrary;

namespace Moritz.Algorithm.Tombeau1
{
	internal class Voice3 : Tombeau1Voice
	{
		public Voice3(int midiChannel, Voice1 voice1, Voice2 voice2, Envelope centredEnvelope, Envelope basedEnvelope)
			: base(midiChannel)
		{
			_modeSegments = GetModeSegments(voice1);
		}

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

		private List<ModeSegment> GetModeSegments(Voice1 voice1)
		{
			List<ModeSegment> basicModeSegments = voice1.ModeSegments;
			List<ModeSegment> rval = new List<ModeSegment>();

			List<Envelope> timeWarpPerIUDEnvelopePerModeSegment = voice1.TimeWarpPerIUDEnvelopePerModeSegment;
			List<Envelope> absPitchPerModeGrpTrkEnvelopePerModeSegment = voice1.AbsPitchPerModeGrpTrkEnvelopePerModeSegment;

			// modeSegment[0] just contains a rest
			var restDef = new MidiRestDef(0, voice1.ModeSegments[0].MsDuration);
			var iudList = new List<IUniqueDef>() { restDef };
			var mode = basicModeSegments[0].ModeGrpTrks[0].Mode;
			var modeGrpTrk = new ModeGrpTrk(MidiChannel, 0, iudList, mode, 0);
			var modeGrpTrks = new List<ModeGrpTrk>() { modeGrpTrk };
			var modeSegment0 = new ModeSegment(MidiChannel, MsPositionReContainer, modeGrpTrks);

			//var iudList = new List<IUniqueDef>();
			//var mode = modeSegments[0].ModeGrpTrks[0].Mode;
			//var modeGrpTrk = new ModeGrpTrk(MidiChannel, 0, iudList, mode, 0);
			//var modeGrpTrks = new List<ModeGrpTrk>() { modeGrpTrk };
			//var modeSegment0 = new ModeSegment(MidiChannel, MsPositionReContainer, modeGrpTrks);

			rval.Add(modeSegment0);

			Debug.Assert(rval[0].MsDuration == voice1.ModeSegments[0].MsDuration);

			int nModeSegments = basicModeSegments.Count;
			for(int i = 1; i < nModeSegments; i++)
			{
				ModeSegment modeSegment = basicModeSegments[i - 1].Clone();
				Envelope timeWarpPerIUDEnvelope = timeWarpPerIUDEnvelopePerModeSegment[i];
				Envelope absPitchPerModeGrpTrkEnvelope = absPitchPerModeGrpTrkEnvelopePerModeSegment[i];

				//AdjustPitches(modeSegment, absPitchPerModeGrpTrkEnvelope);

				//AdjustDurations(modeSegment, timeWarpPerIUDEnvelope, timeWarpDistortion: ((double)i / 1.5) + 5);

				//AdjustVelocities(modeSegment);

				modeSegment.MsDuration = voice1.ModeSegments[i].MsDuration;
				rval.Add(modeSegment);

				SetModeSegmentMsPositionsReContainer(rval);

				Debug.Assert(rval[i].MsDuration == voice1.ModeSegments[i].MsDuration);
			}

			return rval;
		}

		internal void AdjustForFourVoices()
		{
			//throw new NotImplementedException();
		}
	}
}