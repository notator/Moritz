using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

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
