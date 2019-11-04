using System;
using System.Collections.Generic;
using System.Diagnostics;
using Moritz.Globals;

namespace Moritz.Spec
{
	/// <summary>
	/// </summary>
	public class GamutVector
	{
		#region constructors

		/// <summary>
		/// GamutVector.Gamuts is a list of Gamuts that begins with startGamut and moves towards the targetGamut argument (which is not included in the list).
		/// GamutVector.PitchVectors is a corresponding list of pitch vectors. Each pitch vector is a list of (startPitch, targetPitch) tuples.
		/// 
		/// GamutVector.PitchVectors is first constructed by making linear connections between the pitches in the pitchVectorsData.
		/// GamutVector.Gamuts is then constructed from the PitchVectors.
		/// If, in a particuar step, an absolute pitch occurs in more than one pitch vector, the maximum weight will be used in the corresponding Gamut.
		/// 
		/// The startGamut, targetGamut and pitchVectorData arguments are all completely independent.
		/// In particular, startGamut and the targetGamut can have different pitches, and/or
		/// a different pitchHierarchy (=weights) and/or a different _number_ of pitches.
		/// 
		/// Each int in the pitchVectorsData is an arbitrary pitch in range [0..127].
		/// Tuple.Item1 is the pitch at which the pitch vector begins. Tuple.Item2 is the pitch at which it ends.
		/// If Tuple.Item1 is not in the startGamut, then its weight will be 0 by default.
		/// Similarly, if Tuple.Item2 is not in the targetGamut, its weight will be 0 by default.
		/// This means that pitch weights will gradually fade in and out where the start or end pitches are missing.
		/// Duplicates are allowed: there may be more than one pitch vector starting or ending in a particular pitch.
		/// Omissions are also allowed:
		///     1.  If not all of this Gamut's pitches are present in the pitchVectorsData.Item1s, there will
		///         be no corresponding pitchVector connecting the missing pitch to a pitch in the target.
		///     2.  If not all of the targetGamut's pitches are present in the pitchVectorsData.Item2s, there
		///         will be no corresponding pitchVector connecting to that missing pitch. (It will then
		///         appear suddenly, when the target is reached -- if the targetGamut is ever reached.)
		/// The returned Gamuts may contain different numbers of absolute pitches [range 0..11], but the number of
		/// pitches in each Gamut will never be greater than the number of pitchVectors.
		/// The weight of an absolute pitch (range [0..11]) in a returned Gamut will be the maximum weight for that
		/// absolute pitch in any of the constructed pitch vectors.
		/// 
		/// Precursors to this function can be found in earlier versions of Tombeau 1, and especially
		/// in my paper notebook beginning on 25 September 2019.
		/// </summary>
		/// <param name="startGamut"></param>
		/// <param name="targetGamut"></param>
		/// <param name="pitchVectorsData">All ints must be in range [0..127]. None of them have to be unique.</param>
		/// <param name="steps">An integer greater than 1</param>
		/// <returns>A list of steps Gamuts, beginning with this Gamut and not including the target.</returns>
		public GamutVector(Gamut startGamut, Gamut targetGamut, List<Tuple<int, int>> pitchVectorsData, int steps)
		{
			Debug.Assert(startGamut != null && targetGamut != null);
			Debug.Assert(pitchVectorsData != null && pitchVectorsData.Count >= 1);
			Debug.Assert(steps > 1);

			List<PitchVector> pitchVectors = new List<PitchVector>();
			foreach(Tuple<int, int> pitchVectorData in pitchVectorsData)
			{
				Debug.Assert(pitchVectorData.Item1 >= 0 && pitchVectorData.Item1 <= 127);
				Debug.Assert(pitchVectorData.Item2 >= 0 && pitchVectorData.Item2 <= 127);

				Tuple<UInt7, UInt7> pitchVector = new Tuple<UInt7, UInt7>((UInt7)pitchVectorData.Item1, (UInt7)pitchVectorData.Item2);

				PitchVector singlePitchVector = new PitchVector(startGamut, targetGamut, pitchVector, steps);
				pitchVectors.Add(singlePitchVector);
			}

			PitchVectors = pitchVectors;

			List<Dictionary<int, int>> absPitchWeightDictList = GetAbsPitchWeightDictList(PitchVectors, steps);

			List<Gamut> gamuts = new List<Gamut>();
			foreach(var absPitchWeightDict in absPitchWeightDictList)
			{
				Gamut gamut = new Gamut(absPitchWeightDict);
				gamuts.Add(gamut);
			}
			Gamuts = gamuts;
		}

		public GamutVector(List<Gamut> gamuts, List<PitchVector> pitchVectors)
		{
			Gamuts = new List<Gamut>(gamuts);
			PitchVectors = new List<PitchVector>(pitchVectors);
		}

		#region constructor helpers
		private int GetMaxWeight(List<PitchWeight> pitchWeights, int absPitch)
		{
			int rval = 1;
			foreach(var pitchWeight in pitchWeights)
			{
				if(pitchWeight.Pitch.Int == absPitch)
				{
					int weight = pitchWeight.Weight.Int;
					rval = (rval > weight) ? rval : weight;
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
			List<List<PitchWeight>> pitchWeightsList = new List<List<PitchWeight>>();
			for(int step = 0; step < steps; ++step)
			{
				List<PitchWeight> pitchWeights = new List<PitchWeight>();
				foreach(var pitchVector in pitchVectors)
				{
					PitchWeight pitchWeight = pitchVector.PitchWeights[step];
					var absolutePitch = pitchWeight.Pitch.Int % 12; // for Gamut
					var weight = pitchWeight.Weight.Int;
					pitchWeights.Add(new PitchWeight(absolutePitch, weight));
				}

				pitchWeightsList.Add(pitchWeights);
			}

			// Each pitchWeights in the pitchWeightsList can contain both multiple entries for each absolute pitch and weights that are 0.
			// The pitches in the following dictionary use their maximum weight in the pitchWeights. Pitches that have weight == 0
			// are filtered out. 
			List<Dictionary<int, int>> absPitchWeightDictList = new List<Dictionary<int, int>>();
			foreach(var pitchWeights in pitchWeightsList)
			{
				Dictionary<int, int> absPitchWeightDict = new Dictionary<int, int>();
				for(int absPitch = 0; absPitch < 12; ++absPitch)
				{
					int maxWeight = GetMaxWeight(pitchWeights, absPitch);
					absPitchWeightDict.Add(absPitch, maxWeight);
				}
				absPitchWeightDictList.Add(absPitchWeightDict);
			}

			return absPitchWeightDictList;
		}

		#endregion constructor helpers
		#endregion constructors

		/// <summary>
		/// Concatenating GamutVectors means that the contained PitchVectors must be concatenated,
		/// so there must be the same number of PitchVectors in each GamutVector.
		/// PitchVectors will only concatenate if the absolute value of the first PitchVector's
		/// target pitch equals the absolute value of the concatenated PitchVector's first pitch.
		/// In other words, pitches in different octaves are equivalent when concatenating.
		/// Note that Gamut.AbsolutePitchHeirarchy values are in range 0..11,
		/// but pitchWeight.Item1 values (pitches) are in range 0..127.
		/// </summary>
		/// <param name="concatenatedGamutVector"></param>
		/// <returns></returns>
		public GamutVector Concat(GamutVector concatenatedGamutVector)
		{
			Debug.Assert(PitchVectors.Count == concatenatedGamutVector.PitchVectors.Count);
			for(int i = 0; i < PitchVectors.Count; ++i)
			{
				PitchVector pitchVector = PitchVectors[i];
				UInt7 targetPitch = pitchVector.TargetPitchWeight.Pitch;
				bool found = false;
				foreach(PitchVector cPitchVector in concatenatedGamutVector.PitchVectors)
				{
					UInt7 linkedPitch = cPitchVector.PitchWeights[0].Pitch;
					if(targetPitch == linkedPitch)
					{
						found = true;
						break;
					}
				}
				Debug.Assert(found);
			}

			List<Gamut> gamuts = new List<Gamut>(Gamuts);
			List<Gamut> cGamuts = new List<Gamut>(concatenatedGamutVector.Gamuts);
			gamuts.AddRange(cGamuts);

			List<PitchVector> pitchVectors = new List<PitchVector>();
			foreach(PitchVector pitchVector in PitchVectors)
			{
				List<PitchWeight> pitchWeights = new List<PitchWeight>();
				foreach(PitchVector cPitchVector in concatenatedGamutVector.PitchVectors)
				{
					if(pitchVector.TargetPitchWeight.Pitch.Int % 12 == cPitchVector.PitchWeights[0].Pitch.Int % 12)
					{
						pitchWeights.AddRange(pitchVector.PitchWeights);
						pitchWeights.AddRange(cPitchVector.PitchWeights);
						pitchVectors.Add(new PitchVector(pitchWeights, cPitchVector.TargetPitchWeight));
						break;
					}
				}
			}

			return new GamutVector(gamuts, pitchVectors);
		}

		/// <summary>
		/// Gamuts does not contain TargetGamut
		/// </summary>
		public IReadOnlyList<Gamut> Gamuts { get; private set; }
		public IReadOnlyList<PitchVector> PitchVectors { get; private set; }
	}
}
