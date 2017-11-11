using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Moritz.Spec;

namespace Moritz.Algorithm.Tombeau1
{
    internal class Voice4 : Tombeau1Voice
    {
        public Voice4(int midiChannel, Voice1 voice1, Voice2 voice2, Voice3 voice3)
			: base(midiChannel)
        {
			_modeSegments = Compose(voice1, voice2, voice3);
		}

		public override List<int> BarlineMsPositions()
		{
			throw new NotImplementedException();
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

		private List<ModeSegment> Compose(Voice1 voice1, Voice2 voice2, Voice3 voice3)
		{
			throw new NotImplementedException();
		}
	}       
}