using System;
using System.Collections.Generic;
using Krystals4ObjectLibrary;
using Moritz.Globals;
using Moritz.Spec;

namespace Moritz.Algorithm.Tombeau1
{
	internal class Voice2 : Tombeau1Voice
    {
		public Voice2(int midiChannel, Voice1 voice1, Envelope centredEnvelope, Envelope basedEnvelope)
			: base(midiChannel)
        {
			_modeSegments = Compose(voice1, centredEnvelope, basedEnvelope);
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

		private List<ModeSegment> Compose(Voice1 voice1, Envelope centredEnvelope, Envelope basedEnvelope)
		{
			List<ModeSegment> voice2ModeSegments = new List<ModeSegment>();
			return voice2ModeSegments;
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
			var voice2BarlinePositions = new List<int>();
			return voice2BarlinePositions;
		}
	}
}