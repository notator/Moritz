using System;
using System.Collections.Generic;
using System.Diagnostics;
using Moritz.Globals;

namespace Moritz.Spec
{
	/// <summary>
	/// </summary>
	public class PitchVector
	{
		#region constructors
		/// <summary>
		/// The PitchWeights list, which has a minimum Count of 1, does not contain the target.
		/// Both start and end pitches All pitches and weights are in range 0..127.
		/// </summary>
		/// <param name="startGamut"></param>
		/// <param name="targetGamut"></param>
		/// <param name="pitchVector">All ints (both start and end pitches) are in range [0..127]</param>
		/// <param name="steps">An integer greater than 1</param>
		public PitchVector(Gamut startGamut, Gamut targetGamut, Tuple<int, int> pitchVector, int steps)
		{
			M.AssertRange0_127(pitchVector.Item1);
			M.AssertRange0_127(pitchVector.Item2);
			M.Assert(steps > 1);

			Debug.Assert(steps > 1);
			int startPitch = pitchVector.Item1;
			int startWeight = 1;
			foreach(var pitchWeight in startGamut.PitchWeights)
			{
				if(pitchWeight.Pitch == startPitch)
				{
					startWeight = pitchWeight.Weight;
					break;
				}
			}

			int endPitch = pitchVector.Item2;
			int endWeight = 1;
			foreach(var pitchWeight in targetGamut.PitchWeights)
			{
				if(pitchWeight.Pitch == endPitch)
				{
					endWeight = pitchWeight.Weight;
					break;
				}
			}

			TargetPitchWeight = new PitchWeight(endPitch, endWeight);

			double pitchIncrPerStep = ((double)endPitch - startPitch) / steps;
			double weightIncrPerStep = ((double)(endWeight - startWeight)) / steps;

			double dPitch = startPitch;
			double dWeight = startWeight;
			int iPitch, iWeight;
			for(int i = 0; i < steps; ++i)
			{
				iPitch = (int)Math.Round(dPitch);
				iWeight = (int)Math.Round(dWeight);
				iWeight = (iWeight == 0) ? 1 : iWeight;
				var kvp = new PitchWeight(iPitch, iWeight);
				_pitchWeights.Add(kvp);

				dPitch += pitchIncrPerStep;
				dWeight += weightIncrPerStep;
			}
		}

		/// <summary>
		/// Contains a list of pitchWeights and their target pitchWeight.
		/// The list does not contain the target.
		/// All pitches are in range 0..127, weights are in range 1..127.
		/// </summary>
		public PitchVector(List<PitchWeight> pitchWeights, PitchWeight targetPitchWeight)
		{
			_pitchWeights = pitchWeights;
			TargetPitchWeight = targetPitchWeight;
		}

		/// <summary>
		/// Transposes the pitchVector by octaves so that its minimum pitch is
		/// both greater than or equal to lowerBound, and less than lowerBound + 12.
		/// </summary>
		/// <param name="lowerBound"></param>
		public void SetOctave(int lowerBound)
		{
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
				throw new ApplicationException($"Can't transpose pitches to lowerBound {lowerBound}");
			}
			#endregion

			if(transposition != 0)
			{
				for(int i = 0; i < _pitchWeights.Count; ++i)
				{
					int newPitch = _pitchWeights[i].Pitch + transposition;
					_pitchWeights[i] = new PitchWeight(newPitch, _pitchWeights[i].Weight);
				}
			}
		}

		public void GetRange(out int minPitch, out int maxPitch)
		{
			maxPitch = 0;
			minPitch = int.MaxValue;
			foreach(var pitchWeight in _pitchWeights)
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
		/// </summary>
		/// <param name="minPitch">The minimum pitch in the pitchVector when the function returns.</param>
		/// <param name="maxPitch">The maximum pitch in the pitchVector when the function returns.</param>
		public void MinimizePitchIntervals()
		{
			int maxPitch = 0;
			int minPitch = int.MaxValue;

			List<int> newPitches = new List<int>() { _pitchWeights[0].Pitch }; // newPitches initially has unbounded range

			int prevPitch;
			int thisPitch;
			for(int i = 1; i < _pitchWeights.Count; ++i)
			{
				prevPitch = newPitches[i - 1];
				thisPitch = _pitchWeights[i].Pitch;
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
				for(int i = 0; i < newPitches.Count; ++i)
				{
					int newPitch = newPitches[i] + transposition;
					_pitchWeights[i] = new PitchWeight(newPitch, _pitchWeights[i].Weight);
					//Console.WriteLine($"{i + 1}: pitch = {newPitch}");
				}
			}

			//Console.WriteLine($"maxPitch before:after transposition = {maxBefore}:{maxPitch}");
			//Console.WriteLine($"minPitch before:after transposition = {minBefore}:{minPitch}");
			//Console.WriteLine("===================================");
		}

		#endregion constructors

		public PitchWeight TargetPitchWeight { get; }

		public IReadOnlyList<PitchWeight> PitchWeights{get{ return _pitchWeights; } }
		private readonly List<PitchWeight> _pitchWeights = new List<PitchWeight>();

	}
}
