using System;
using System.Collections.Generic;
using System.Diagnostics;
using Moritz.Globals;

namespace Moritz.Spec
{
	/// <summary>
	/// A list of PitchWeights and a TargetPitchWeight.
	/// The PitchWeights list cannot be null or empty.
	/// The TargetPitchWeight cannot be null.
	/// The PitchWeights list does not contain the TargetPitchWeight. 
	/// </summary>
	public class PitchWeightVector
	{
		#region constructors
		/// <summary>
		/// The PitchWeights list, which has a minimum Count of 1, does not contain the target.
		/// </summary>
		/// <param name="startGamut">Must contain the pitch pitchVectorEndPoints.Item1</param>
		/// <param name="targetGamut">Must contain the pitch pitchVectorEndPoints.Item2</param>
		/// <param name="pitchVectorEndPoints">All ints (both start and end pitches) are in range [0..127]</param>
		/// <param name="steps">The number of PitchWeights in the constructed PitchWeights list. Must be greater than 0</param>
		public PitchWeightVector(Gamut startGamut, Gamut targetGamut, Tuple<int, int> pitchVectorEndPoints, int steps)
		{
			M.Assert(pitchVectorEndPoints.Item1 >= startGamut.MinPitch && pitchVectorEndPoints.Item1 <= startGamut.MaxPitch);
			M.Assert(pitchVectorEndPoints.Item2 >= targetGamut.MinPitch && pitchVectorEndPoints.Item2 <= targetGamut.MaxPitch);
			M.Assert(steps > 0);

			int startPitch = pitchVectorEndPoints.Item1;
			int startWeight = startGamut.Weight(startPitch);

			int endPitch = pitchVectorEndPoints.Item2;
			int endWeight = targetGamut.Weight(endPitch);

			TargetPitchWeight = new PitchWeight(endPitch, endWeight);

			double pitchIncrPerStep = ((double)endPitch - startPitch) / steps;
			double weightIncrPerStep = ((double)(endWeight - startWeight)) / steps;

			double dPitch = startPitch;
			double dWeight = startWeight;
			int iPitch, iWeight;
			List<PitchWeight> pitchWeights = new List<PitchWeight>();
			for(int i = 0; i < steps; ++i)
			{
				iPitch = (int)Math.Round(dPitch);
				iWeight = (int)Math.Round(dWeight);
				iWeight = (iWeight == 0) ? 1 : iWeight;
				var kvp = new PitchWeight(iPitch, iWeight);
				pitchWeights.Add(kvp);

				dPitch += pitchIncrPerStep;
				dWeight += weightIncrPerStep;
			}
			PitchWeights = pitchWeights;
		}

		/// <summary>
		/// Contains a list of pitchWeights and their target pitchWeight.
		/// The list does not contain the target.
		/// All pitches are in range 0..127, weights are in range 1..127.
		/// </summary>
		public PitchWeightVector(List<PitchWeight> pitchWeights, PitchWeight targetPitchWeight)
		{
			PitchWeights = pitchWeights;
			TargetPitchWeight = targetPitchWeight;
		}

		#endregion constructors

		internal PitchWeightVector Concat(PitchWeightVector pitchWeightVectorToConcat)
		{
			M.Assert(TargetPitchWeight == pitchWeightVectorToConcat.PitchWeights[0]);

			List<PitchWeight> pitchWeights = new List<PitchWeight>(PitchWeights);
			pitchWeights.AddRange(pitchWeightVectorToConcat.PitchWeights);

			return new PitchWeightVector(pitchWeights, pitchWeightVectorToConcat.TargetPitchWeight);
		}

		public PitchWeight TargetPitchWeight { get; private set; }
		public IReadOnlyList<PitchWeight> PitchWeights{ get; private set; }
	}
}
