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
	public class PitchVector
	{
		#region constructors
		/// <summary>
		/// The PitchWeights list, which has a minimum Count of 1, does not contain the target.
		/// </summary>
		/// <param name="startGamut">Must contain the pitch pitchVectorEndPoints.Item1</param>
		/// <param name="targetGamut">Must contain the pitch pitchVectorEndPoints.Item2</param>
		/// <param name="pitchVectorEndPoints">All ints (both start and end pitches) are in range [0..127]</param>
		/// <param name="steps">The number of PitchWeights in the constructed PitchWeights list. Must be greater than 0</param>
		public PitchVector(LongGamut startGamut, LongGamut targetGamut, Tuple<int, int> pitchVectorEndPoints, int steps)
		{
			M.AssertRange0_127(pitchVectorEndPoints.Item1);
			M.AssertRange0_127(pitchVectorEndPoints.Item2);
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
		public PitchVector(List<PitchWeight> pitchWeights, PitchWeight targetPitchWeight)
		{
			PitchWeights = pitchWeights;
			TargetPitchWeight = targetPitchWeight;
		}



		/// <summary>
		/// Transposes the pitchVector by octaves so that its minimum pitch is
		/// both greater than or equal to lowerBound, and less than lowerBound + 12.
		/// Returns false if this would result in any pitch having a value outside the range 0..127. 
		/// </summary>
		/// <param name="lowerBound"></param>
		public bool SetOctave(int lowerBound)
		{
			bool returnValue = true;

			GetRange(out int minPitch, out int maxPitch);

			int transposition = 0;
			#region get transposition (if any)
			while(minPitch < lowerBound)
			{
				minPitch += 12;
				maxPitch += 12;
				transposition += 12;
			}

			int upperBound = lowerBound + 11;
			while(minPitch > upperBound)
			{
				minPitch -= 12;
				maxPitch -= 12;
				transposition -= 12;
			}
			if(minPitch < 0 || maxPitch > 127)
			{
				returnValue = false;
			}
			#endregion

			List<PitchWeight> pitchWeights = new List<PitchWeight>(PitchWeights);
			if(transposition != 0)
			{
				for(int i = 0; i < PitchWeights.Count; ++i)
				{
					int newPitch = PitchWeights[i].Pitch + transposition;
					pitchWeights.Add(new PitchWeight(newPitch, PitchWeights[i].Weight));
				}
			}

			PitchWeights = pitchWeights;

			return returnValue;

		}

		public void GetRange(out int minPitch, out int maxPitch)
		{
			maxPitch = 0;
			minPitch = int.MaxValue;
			foreach(var pitchWeight in PitchWeights)
			{
				minPitch = (minPitch < pitchWeight.Pitch) ? minPitch : pitchWeight.Pitch;
				maxPitch = (maxPitch > pitchWeight.Pitch) ? maxPitch : pitchWeight.Pitch;
			}
		}

		/// <summary>
		/// Intervals between consecutive pitches are minimized (set to less than or equal to 6 semitones).
		/// If necessary, the pitchVector will automatically be transposed up or down by octaves
		/// so that the resulting pitches are in range 0..127.
		/// An ApplicationException is thrown if the resulting pitches can't be fit into range [0..127].
		/// TargetPitchWeight is not currently changed...
		/// </summary>
		/// <param name="minPitch">The minimum pitch in the pitchVector when the function returns.</param>
		/// <param name="maxPitch">The maximum pitch in the pitchVector when the function returns.</param>
		public void MinimizePitchIntervals()
		{
			int maxPitch = 0;
			int minPitch = int.MaxValue;

			List<int> newPitches = new List<int>() { PitchWeights[0].Pitch }; // newPitches initially has unbounded range

			int prevPitch;
			int thisPitch;
			for(int i = 1; i < PitchWeights.Count; ++i)
			{
				prevPitch = newPitches[i - 1];
				thisPitch = PitchWeights[i].Pitch;
				int pitchDiff = thisPitch - prevPitch;
				if(Math.Abs(pitchDiff) > 6)
				{
					while(Math.Abs(pitchDiff) > 6)
					{
						pitchDiff = (pitchDiff < 0) ? pitchDiff += 12 :	pitchDiff -= 12;
					}

					thisPitch = prevPitch + pitchDiff;
				}

				maxPitch = (maxPitch < thisPitch) ? thisPitch : maxPitch;
				minPitch = (minPitch > thisPitch) ? thisPitch : minPitch;

				newPitches.Add(thisPitch);
			}

			int minBefore = minPitch;
			int maxBefore = maxPitch;

			int transposition = 0;
			#region get transposition (if any)
			while(minPitch < 0)
			{
				minPitch += 12;
				maxPitch += 12;
				transposition += 12;
			}
			while(maxPitch > 127)
			{
				minPitch -= 12;
				maxPitch -= 12;
				transposition -= 12;
			}
			if(minPitch < 0)
			{
				throw new ApplicationException("Can't fit pitchVector pitches into range [0..127]");
			}
			#endregion

			if(transposition != 0)
			{
				List<PitchWeight> pitchWeights = new List<PitchWeight>(PitchWeights);
				for(int i = 0; i < newPitches.Count; ++i)
				{
					int newPitch = newPitches[i] + transposition;
					pitchWeights.Add( new PitchWeight(newPitch, PitchWeights[i].Weight));
					//Console.WriteLine($"{i + 1}: pitch = {newPitch}");
				}
				PitchWeights = pitchWeights;
			}

			//Console.WriteLine($"maxPitch before:after transposition = {maxBefore}:{maxPitch}");
			//Console.WriteLine($"minPitch before:after transposition = {minBefore}:{minPitch}");
			//Console.WriteLine("===================================");
		}

		#endregion constructors

		public PitchWeight TargetPitchWeight { get; private set; }
		public IReadOnlyList<PitchWeight> PitchWeights{ get; private set; }
	}
}
