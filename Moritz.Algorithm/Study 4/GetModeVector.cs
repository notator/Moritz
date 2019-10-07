using System;
using System.Collections.Generic;
using System.Diagnostics;

using Krystals4ObjectLibrary;
using Moritz.Globals;
using Moritz.Palettes;
using Moritz.Spec;
using Moritz.Symbols;

namespace Moritz.Algorithm.Study4
{
	public partial class Study4Algorithm : CompositionAlgorithm
	{
		/// <summary>
		/// Returns a list of Modes that begins with startMode and moves towards
		/// the targetMode argument (which is not included in the returned list).
		/// The vector is constructed by making linear connections between the pitches in the pitchVectors.
		/// 
		/// The startMode, target Mode and pitchVectorData are all completely independent.
		/// In particular, startMode and the targetMode can have different pitches, and/or
		/// a different pitchHierarchy (=weights) and/or a different _number_ of pitches.
		/// 
		/// Each int in the pitchVectorsData is an arbitrary pitch in range [0..127].
		/// Tuple.Item1 is the pitch at which the pitch vector begins. Tuple.Item2 is the pitch at which it ends.
		/// If Tuple.Item1 is not in the startMode.Gamut, then its weight will be 0 by default.
		/// Similarly, if Tuple.Item2 is not in the targetMode.Gamut, its weight will be 0 by default.
		/// This means that pitch weights will gradually fade in and out where the start or end pitches are missing.
		/// Duplicates are allowed: there may be more than one pitch vector starting or ending in a particular pitch.
		/// Omissions are also allowed:
		///     1.  If not all of this Mode's pitches are present in the pitchVectorsData.Item1s, there will
		///         be no corresponding pitchVector connecting the missing pitch to a pitch in the target.
		///     2.  If not all of the targetMode's pitches are present in the pitchVectorsData.Item2s, there
		///         will be no corresponding pitchVector connecting to that missing pitch. (It will then
		///         appear suddenly, when the target is reached -- if the targetMode is ever reached.)
		/// The returned Modes may contain different numbers of absolute pitches [range 0..11], but the number of
		/// pitches in each Mode will never be greater than the number of pitchVectors.
		/// The weight of an absolute pitch (range [0..11]) in a returned Mode will be the maximum weight for that
		/// absolute pitch in any of the constructed pitch vectors.
		/// 
		/// Precursors to this function can be found in earlier versions of Tombeau 1, and especially
		/// in my paper notebook beginning on 25 September 2019.
		/// </summary>
		/// <param name="targetMode"></param>
		/// <param name="pitchVectorsData">All ints must be in range [0..127].</param>
		/// <param name="steps">An integer greater than 1</param>
		/// <returns>A list of steps Modes, beginning with this Mode and not including the target.</returns>
		public List<Mode> GetModeVector(Mode startMode, Mode targetMode, List<Tuple<int, int>> pitchVectorsData, int steps)
		{
			List<List<Tuple<int, int>>> singlePitchVectorsList = new List<List<Tuple<int, int>>>();
			foreach(Tuple<int, int> pitchVectorData in pitchVectorsData)
			{
				Debug.Assert(pitchVectorData.Item1 >= 0 && pitchVectorData.Item1 <= 127);
				Debug.Assert(pitchVectorData.Item2 >= 0 && pitchVectorData.Item2 <= 127);

				List<Tuple<int, int>> singlePitchVector = GetSinglePitchVector(startMode, targetMode, pitchVectorData, steps);
				singlePitchVectorsList.Add(singlePitchVector);
			}

			List<List<Tuple<int, int>>> modeDataList = GetModeDataList(steps, singlePitchVectorsList);

			// Each modeData in the modeDataList can contain both multiple entries for each absolute pitch and weights that are 0.
			// The pitches in the following dictionary use their maximum weight in the modeData. Pitches that have weight == 0
			// are filtered out. 
			List<Dictionary<int, int>> absPitchWeightDictList = new List<Dictionary<int, int>>();
			foreach(var modeData in modeDataList)
			{
				Dictionary<int, int> absPitchWeightDict = new Dictionary<int, int>();
				for(int absPitch = 0; absPitch < 12; ++absPitch)
				{
					int? maxWeight = GetMaxWeight(modeData, absPitch);
					if(maxWeight != null && maxWeight > 0)
					{
						absPitchWeightDict.Add(absPitch, (int)maxWeight);
					}
				}
				absPitchWeightDictList.Add(absPitchWeightDict);
			}

			List<Mode> rval = new List<Mode>();
			foreach(var absPitchWeightDict in absPitchWeightDictList)
			{
				Mode mode = new Mode(absPitchWeightDict);
				rval.Add(mode);
			}

			return rval;
		}

		private int? GetMaxWeight(List<Tuple<int, int>> modeData, int absPitch)
		{
			int? rval = null;
			foreach(var tuple in modeData)
			{
				if(tuple.Item1 == absPitch)
				{
					int weight = tuple.Item2;
					if(rval == null)
					{
						rval = weight;
					}
					else
					{
						rval = (rval > weight) ? rval : weight;
					}
				}
			}

			return rval;
		}

		/// <summary>
		/// Returns pitches (in each Tuple.Item1) in range [0..127] and weights (in each Tuple.Item2) in range [0..127]
		/// </summary>
		/// <param name="targetMode"></param>
		/// <param name="pitchVectorsData">All ints must be in range [0..127]</param>
		/// <param name="steps">An integer greater than 1</param>
		private List<Tuple<int, int>> GetSinglePitchVector(Mode startMode, Mode targetMode, Tuple<int, int> pitchVectorData, int steps)
		{
			int startPitch = pitchVectorData.Item1;
			int startWeight;
			if(!startMode.AbsolutePitchWeightDict.TryGetValue(startPitch % 12, out startWeight))
			{
				startWeight = 0; // default value
			}

			int endPitch = pitchVectorData.Item2;
			int endWeight;
			if(!targetMode.AbsolutePitchWeightDict.TryGetValue(endPitch % 12, out endWeight))
			{
				endWeight = 0; // default value
			}

			double pitchIncrPerStep = ((double)endPitch - startPitch) / steps;
			double weightIncrPerStep = ((double)(endWeight - startWeight)) / steps;

			List<Tuple<int, int>> singlePitchVector = new List<Tuple<int, int>>();
			double dPitch = startPitch;
			double dWeight = startWeight;
			for(int i = 0; i < steps; ++i)
			{
				int iPitch = (int)Math.Round(dPitch);
				iPitch = (iPitch >= 0) ? iPitch : 0;
				iPitch = (iPitch <= 127) ? iPitch : 127;

				int iWeight = (int)Math.Round(dWeight);
				iWeight = (iWeight >= 0) ? iWeight : 0;
				iWeight = (iWeight <= 127) ? iWeight : 127;

				var kvp = new Tuple<int, int>(iPitch, iWeight);
				singlePitchVector.Add(kvp);

				dPitch += pitchIncrPerStep;
				dWeight += weightIncrPerStep;
			}

			return singlePitchVector;
		}

		/// <summary>
		/// Returns absolute pitches (in each Tuple.Item1) in range [0..11] and their corresponding weights (in each Tuple.Item2) in range [0..127].
		/// </summary>
		/// <param name="steps">is greater than 0</param>
		/// <param name="singlePitchVectorsList">All the pitches and weights are in range [0..127]</param>
		private static List<List<Tuple<int, int>>> GetModeDataList(int steps, List<List<Tuple<int, int>>> singlePitchVectorsList)
		{
			List<List<Tuple<int, int>>> modeDatas = new List<List<Tuple<int, int>>>();
			for(int step = 0; step < steps; ++step)
			{
				List<Tuple<int, int>> modeData = new List<Tuple<int, int>>();
				for(int i = 0; i < singlePitchVectorsList.Count; ++i)
				{
					var entry = singlePitchVectorsList[i][step];
					var absolutePitch = entry.Item1 % 12; // Modes
					var weight = entry.Item2;
					modeData.Add(new Tuple<int, int>(absolutePitch, weight));
				}

				modeDatas.Add(modeData);
			}

			return modeDatas;
		}

	}
}
