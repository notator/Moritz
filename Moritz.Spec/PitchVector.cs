using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Moritz.Spec
{
	/// <summary>
	/// </summary>
	public class PitchVector
	{
		#region constructors
		/// <summary>
		/// Contains a list of pitchWeights and their target pitchWeight.
		/// The list, which has a minimum Count of 1, does not contain the target.
		/// All pitches and weights are in range 0..127.
		/// </summary>
		/// <param name="startMode"></param>
		/// <param name="targetMode"></param>
		/// <param name="pitchVectorData">All ints (both pitches and weights) are in range [0..127]</param>
		/// <param name="steps">An integer greater than 1</param>
		public PitchVector(Mode startMode, Mode targetMode, Tuple<int, int> pitchVectorData, int steps)
		{
			int startPitch = pitchVectorData.Item1;
			int startWeight;
			if(!startMode.AbsolutePitchWeightDict.TryGetValue((startPitch % 12), out startWeight))
			{
				startWeight = 0; // default value
			}

			int endPitch = pitchVectorData.Item2;
			int endWeight;
			if(!targetMode.AbsolutePitchWeightDict.TryGetValue((endPitch % 12), out endWeight))
			{
				endWeight = 0; // default value
			}

			double pitchIncrPerStep = ((double)endPitch - startPitch) / steps;
			double weightIncrPerStep = ((double)(endWeight - startWeight)) / steps;

			double dPitch = startPitch;
			double dWeight = startWeight;
			int iPitch, iWeight;
			for(int i = 0; i < steps; ++i)
			{
				iPitch = (int)Math.Round(dPitch);
				iPitch = (iPitch >= 0) ? iPitch : 0;
				iPitch = (iPitch <= 127) ? iPitch : 127;

				iWeight = (int)Math.Round(dWeight);
				iWeight = (iWeight >= 0) ? iWeight : 0;
				iWeight = (iWeight <= 127) ? iWeight : 127;

				var kvp = new Tuple<int, int>(iPitch, iWeight);
				_pitchWeights.Add(kvp);

				dPitch += pitchIncrPerStep;
				dWeight += weightIncrPerStep;
			}

			iPitch = (int)Math.Round(dPitch);
			iPitch = (iPitch >= 0) ? iPitch : 0;
			iPitch = (iPitch <= 127) ? iPitch : 127;

			iWeight = (int)Math.Round(dWeight);
			iWeight = (iWeight >= 0) ? iWeight : 0;
			iWeight = (iWeight <= 127) ? iWeight : 127;

			_targetPitchWeight = new Tuple<int, int>(iPitch, iWeight);

			AssertPitchWeightConsistency();
		}

		/// <summary>
		/// Contains a list of pitchWeights and their target pitchWeight.
		/// The list does not contain the target.
		/// All pitches and weights are in range 0..127.
		/// </summary>
		public PitchVector(List<Tuple<int, int>> pitchWeights, Tuple<int, int> targetPitchWeight)
		{
			_pitchWeights = pitchWeights;
			_targetPitchWeight = targetPitchWeight;

			AssertPitchWeightConsistency();
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
					int newPitch = _pitchWeights[i].Item1 + transposition;
					_pitchWeights[i] = new Tuple<int, int>(newPitch, _pitchWeights[i].Item2);
				}
			}
		}

		public void GetRange(out int minPitch, out int maxPitch)
		{
			maxPitch = 0;
			minPitch = int.MaxValue;
			foreach(var pitchWeight in _pitchWeights)
			{
				minPitch = (minPitch < pitchWeight.Item1) ? minPitch : pitchWeight.Item1;
				maxPitch = (maxPitch > pitchWeight.Item1) ? maxPitch : pitchWeight.Item1;
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

			List<int> newPitches = new List<int>() { _pitchWeights[0].Item1 }; // newPitches initially has unbounded range

			int prevPitch;
			int thisPitch;
			for(int i = 1; i < _pitchWeights.Count; ++i)
			{
				prevPitch = newPitches[i - 1];
				thisPitch = _pitchWeights[i].Item1;
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
					_pitchWeights[i] = new Tuple<int, int>(newPitch, _pitchWeights[i].Item2);
					//Console.WriteLine($"{i + 1}: pitch = {newPitch}");
				}
			}

			//Console.WriteLine($"maxPitch before:after transposition = {maxBefore}:{maxPitch}");
			//Console.WriteLine($"minPitch before:after transposition = {minBefore}:{minPitch}");
			//Console.WriteLine("===================================");
		}


		private void AssertPitchWeightConsistency()
		{
			foreach(var pitchWeight in _pitchWeights)
			{
				Debug.Assert(pitchWeight.Item1 >= 0 && pitchWeight.Item1 <= 127);
				Debug.Assert(pitchWeight.Item2 >= 0 && pitchWeight.Item2 <= 127);
			}
			Debug.Assert(_targetPitchWeight.Item1 >= 0 && _targetPitchWeight.Item1 <= 127);
			Debug.Assert(_targetPitchWeight.Item2 >= 0 && _targetPitchWeight.Item2 <= 127);
		}

		#endregion constructors

		public Tuple<int,int> TargetPitchWeight { get { return _targetPitchWeight; } }
		private readonly Tuple<int, int> _targetPitchWeight;
		public IReadOnlyList<Tuple<int, int>> PitchWeights{get{ return _pitchWeights; } }
		private readonly List<Tuple<int, int>> _pitchWeights = new List<Tuple<int, int>>();

	}
}
