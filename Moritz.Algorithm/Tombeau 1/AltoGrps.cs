using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Moritz.Spec;

namespace Moritz.Algorithm.Tombeau1
{
	internal class AltoGrps : PaletteGrps
	{
		public AltoGrps(int rootOctave, IReadOnlyList<IReadOnlyList<Grp>> sopranoGrps, IReadOnlyList<IReadOnlyList<Grp>> bassGrps)
			: base(rootOctave, 0, 9)
		{
			for(int i = 0; i < BaseGrps.Count; ++i)
			{
				IReadOnlyList<PaletteGrp> baseGrps = BaseGrps[i];

				List<PaletteGrp> grps = Compose(baseGrps); // default Compose...

				_composedGrps.Add(grps);
			}
		}

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

	}
}