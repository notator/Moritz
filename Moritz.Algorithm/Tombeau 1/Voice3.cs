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
			_modeSegments = Compose(voice1, voice2, centredEnvelope, basedEnvelope);
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

		private List<ModeSegment> Compose(Voice1 voice1, Voice2 voice2, Envelope centredEnvelope, Envelope basedEnvelope)
		{ 
			throw new NotImplementedException();
		}

		internal void AdjustForFourVoices()
		{
			throw new NotImplementedException();
		}

		public override List<int> BarlineMsPositions()
		{
			throw new NotImplementedException();
		}
	}
}