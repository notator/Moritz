using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Moritz.Spec
{
	/// <summary>
	/// </summary>
	public class ModeVector
	{
		#region constructors

		/// <summary>
		/// ModeVector.Modes is a list of Modes that begins with startMode and moves towards the targetMode argument (which is not included in the list).
		/// ModeVector.PitchVectors is a corresponding list of pitch vectors. Each pitch vector is a list of (absolutePitch, weight) tuples.
		/// 
		/// ModeVector.PitchVectors is first constructed by making linear connections between the pitches in the pitchVectorsData.
		/// ModeVector.Modes is then constructed from the PitchVectors.
		/// If, in a particuar step, an absolute pitch occurs in more than one pitch vector, the maximum weight will be used in the corresponding Mode.
		/// 
		/// The startMode, target Mode and pitchVectorData arguments are all completely independent.
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
		/// <param name="startMode"></param>
		/// <param name="targetMode"></param>
		/// <param name="pitchVectorsData">All ints must be in range [0..127]. None of them have to be unique.</param>
		/// <param name="steps">An integer greater than 1</param>
		/// <returns>A list of steps Modes, beginning with this Mode and not including the target.</returns>
		public ModeVector(Mode startMode, Mode targetMode, List<Tuple<int, int>> pitchVectorsData, int steps)
		{
			foreach(Tuple<int, int> pitchVectorData in pitchVectorsData)
			{
				Debug.Assert(pitchVectorData.Item1 >= 0 && pitchVectorData.Item1 <= 127);
				Debug.Assert(pitchVectorData.Item2 >= 0 && pitchVectorData.Item2 <= 127);

				PitchVector singlePitchVector = new PitchVector(startMode, targetMode, pitchVectorData, steps);
				_pitchVectors.Add(singlePitchVector);
			}

			List<Dictionary<int, int>> absPitchWeightDictList = GetAbsPitchWeightDictList(PitchVectors, steps);

			foreach(var absPitchWeightDict in absPitchWeightDictList)
			{
				Mode mode = new Mode(absPitchWeightDict);
				_modes.Add(mode);
			}
		}

		public ModeVector(List<Mode> modes, List<PitchVector> pitchVectors)
		{
			_modes = new List<Mode>(modes);
			_pitchVectors = new List<PitchVector>(pitchVectors);
		}

		#region constructor helpers
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
		/// returns steps Dictionaries whose Key is absolute pitch, and Value is the maximum weight for that pitch in any pitch vector.
		/// </summary>
		/// <param name="steps">is greater than 0</param>
		/// <param name="pitchVectors">All the pitches and weights are in range [0..127]</param>
		private List<Dictionary<int, int>> GetAbsPitchWeightDictList(IReadOnlyList<PitchVector> pitchVectors, int steps)
		{
			List<List<Tuple<int, int>>> modeDatas = new List<List<Tuple<int, int>>>();
			for(int step = 0; step < steps; ++step)
			{
				List<Tuple<int, int>> modeData = new List<Tuple<int, int>>();
				for(int i = 0; i < pitchVectors.Count; ++i)
				{
					Tuple<int, int> entry = pitchVectors[i].PitchWeights[step];
					var absolutePitch = entry.Item1 % 12; // for Mode
					var weight = entry.Item2;
					modeData.Add(new Tuple<int, int>(absolutePitch, weight));
				}

				modeDatas.Add(modeData);
			}

			// Each modeData in the modeDataList can contain both multiple entries for each absolute pitch and weights that are 0.
			// The pitches in the following dictionary use their maximum weight in the modeData. Pitches that have weight == 0
			// are filtered out. 
			List<Dictionary<int, int>> absPitchWeightDictList = new List<Dictionary<int, int>>();
			foreach(var modeData in modeDatas)
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

			return absPitchWeightDictList;
		}

		#endregion constructor helpers
		#endregion constructors

		/// <summary>
		/// Concatenating ModeVectors means that the contained PitchVectors must be concatenated,
		/// so there must be the same number of PitchVectors in each ModeVector.
		/// PitchVectors will only concatenate if the absolute value of the first PitchVector's
		/// target pitch equals the absolute value of the concatenated PitchVector's first pitch.
		/// In other words, pitches in different octaves are equivalent when concatenating.
		/// Note that Mode.AbsolutePitchHeirarchy values are in range 0..11,
		/// but pitchWeight.Item1 values (pitches) are in range 0..127.
		/// </summary>
		/// <param name="concatenatedModeVector"></param>
		/// <returns></returns>
		public ModeVector Concat(ModeVector concatenatedModeVector)
		{
			Debug.Assert(PitchVectors.Count == concatenatedModeVector.PitchVectors.Count);
			foreach(PitchVector pitchVector in PitchVectors)
			{
				bool found = false;
				foreach(PitchVector cPitchVector in concatenatedModeVector.PitchVectors)
				{
					// just compare absolute pitches
					if(pitchVector.TargetPitchWeight.Item1 % 12 == cPitchVector.PitchWeights[0].Item1 % 12)
					{
						found = true;
						break;
					}
				}
				Debug.Assert(found);
			}

			List<Mode> modes = new List<Mode>(_modes);
			List<Mode> cModes = new List<Mode>(concatenatedModeVector.Modes);
			modes.AddRange(cModes);

			List<PitchVector> pitchVectors = new List<PitchVector>();
			foreach(PitchVector pitchVector in PitchVectors)
			{
				List<Tuple<int, int>> pitchWeights = new List<Tuple<int, int>>();
				foreach(PitchVector cPitchVector in concatenatedModeVector.PitchVectors)
				{
					if(pitchVector.TargetPitchWeight.Item1 % 12 == cPitchVector.PitchWeights[0].Item1 % 12)
					{
						pitchWeights.AddRange(pitchVector.PitchWeights);
						pitchWeights.AddRange(cPitchVector.PitchWeights);
						pitchVectors.Add(new PitchVector(pitchWeights, cPitchVector.TargetPitchWeight));
						break;
					}
				}
			}

			return new ModeVector(modes, pitchVectors);
		}

		/// <summary>
		/// Modes does not contain TargetMode
		/// </summary>
		public IReadOnlyList<Mode> Modes { get { return _modes; } }
		private readonly List<Mode> _modes = new List<Mode>();
		public IReadOnlyList<PitchVector> PitchVectors { get { return _pitchVectors; } }
		private readonly List<PitchVector> _pitchVectors = new List<PitchVector>();
	}
}
