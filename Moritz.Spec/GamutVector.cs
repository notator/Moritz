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
		/// GamutVector.Gamuts[0] is a clone of the constructor's startGamut argument.
		/// GamutVector.TargetGamut is a clone of the constructor's targetGamut argument.
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
		/// <param name="pitchVectorEndPointsList">All ints must be in range [0..127]. None of them have to be unique.</param>
		/// <param name="steps">An integer greater than 1</param>
		/// <returns>A list of steps Gamuts, beginning with this Gamut and not including the target.</returns>
		public GamutVector(LongGamut startGamut, LongGamut targetGamut, List<Tuple<int, int>> pitchVectorEndPointsList, int steps)
		{
			M.Assert(startGamut != null && targetGamut != null);
			M.Assert(pitchVectorEndPointsList != null && pitchVectorEndPointsList.Count >= 1);
			foreach(var tuple in pitchVectorEndPointsList)
			{
				M.Assert(tuple.Item1 >= 0 && tuple.Item1 <= 127);
				M.Assert(tuple.Item2 >= 0 && tuple.Item2 <= 127);
			}
			M.Assert(steps > 1);

			List<PitchVector> pitchVectors = new List<PitchVector>();
			foreach(Tuple<int, int> pitchVectorEndPoints in pitchVectorEndPointsList)
			{
				PitchVector absPitchVector = new PitchVector(startGamut, targetGamut, pitchVectorEndPoints, steps);
				pitchVectors.Add(absPitchVector);
			}

			List<List<PitchVector>> pitchVectorsPerOctave = GetPitchVectorsPerOctave(pitchVectors);

			List<PitchVector> allPitchVectors = new List<PitchVector>();
			foreach(var pitchvectors in pitchVectorsPerOctave)
			{
				allPitchVectors.AddRange(pitchvectors);
			}

			PitchVectors = allPitchVectors;

			List<List<PitchWeight>> pitchWeightsPerGamut = GetPitchWeightsListPerGamut(allPitchVectors);

			List<LongGamut> gamuts = new List<LongGamut>();
			foreach(var pitchWeights in pitchWeightsPerGamut)
			{
				LongGamut gamut = new LongGamut(pitchWeights);
				gamuts.Add(gamut);
			}
			LongGamuts = gamuts;
			TargetLongGamut = new LongGamut(targetGamut.PitchWeights);
		}

		/// <summary>
		/// Returns a list of pitchVectors that includes the argument pitchVectors
		/// transposed to all possible octaves.
		/// </summary>
		/// <param name="pitchVectors"></param>
		/// <returns></returns>
		private List<List<PitchVector>> GetPitchVectorsPerOctave(List<PitchVector> pitchVectors)
		{
			List<List<PitchVector>> pitchVectorsPerOctave = new List<List<PitchVector>>();

			for(int octave = 0; octave < 127; octave += 12)
			{
				List<PitchVector> pitchVectorsInOctave = new List<PitchVector>();
				foreach(var absPitchVector in pitchVectors)
				{
					IReadOnlyList<PitchWeight> absPitchWeights = absPitchVector.PitchWeights;
					List<PitchWeight> relPitchWeights = new List<PitchWeight>();
					foreach(var absPitchWeight in absPitchWeights)
					{
						int absPitch = absPitchWeight.Pitch;
						int weight = absPitchWeight.Weight;
						int relPitch = M.SetRange0_127(absPitch + octave);
						relPitchWeights.Add(new PitchWeight(relPitch, weight));
					}

					int targetPitch = M.SetRange0_127(absPitchVector.TargetPitchWeight.Pitch + octave);
					int targetWeight = absPitchVector.TargetPitchWeight.Weight;
					PitchWeight targetPitchWeight = new PitchWeight(targetPitch, targetWeight);

					pitchVectorsInOctave.Add(new PitchVector(relPitchWeights, targetPitchWeight));
				}

				pitchVectorsPerOctave.Add(pitchVectorsInOctave);

			}
			return pitchVectorsPerOctave;
		}

		public GamutVector(IReadOnlyList<LongGamut> gamuts, LongGamut targetGamut, IReadOnlyList<PitchVector> pitchVectors)
		{
			LongGamuts = new List<LongGamut>(gamuts);
			TargetLongGamut = targetGamut.Clone() as LongGamut;
			List<PitchVector> newPitchVectors = new List<PitchVector>();
			foreach(var pitchVector in pitchVectors)
			{
				newPitchVectors.Add(new PitchVector(new List<PitchWeight>(pitchVector.PitchWeights), pitchVector.TargetPitchWeight));
			}
			PitchVectors = new List<PitchVector>(pitchVectors);
		}

		public object Clone()
		{
			return new GamutVector(LongGamuts, TargetLongGamut, PitchVectors);
		}

		#region constructor helpers
		private int GetMaxWeight(List<PitchWeight> pitchWeights, int absPitch)
		{
			int rval = 1;
			foreach(var pitchWeight in pitchWeights)
			{
				if(pitchWeight.Pitch == absPitch)
				{
					int weight = pitchWeight.Weight;
					rval = (rval > weight) ? rval : weight;
				}
			}

			return rval;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="pitchVectors">All the pitches and weights are in range [0..127]</param>
		private List<List<PitchWeight>> GetPitchWeightsListPerGamut(IReadOnlyList<PitchVector> pitchVectors)
		{
			List<List<PitchWeight>> pitchWeightsListPerGamut = new List<List<PitchWeight>>();
			int nGamuts = pitchVectors[0].PitchWeights.Count;
			for(int i = 0; i < nGamuts; i++)
			{
				List<PitchWeight> gamutPitchWeights = new List<PitchWeight>();
				pitchWeightsListPerGamut.Add(gamutPitchWeights);
			}
			foreach(var pitchVector in pitchVectors)
			{
				for(int i = 0; i < nGamuts; i++)
				{
					PitchWeight pitchWeight = pitchVector.PitchWeights[i];
					int gPitchWeightIndex = pitchWeightsListPerGamut[i].FindIndex(x => x.Pitch == pitchWeight.Pitch);

					if(gPitchWeightIndex == -1)
					{
						pitchWeightsListPerGamut[i].Add(pitchWeight);
					}
					else if(pitchWeightsListPerGamut[i][gPitchWeightIndex].Weight <= pitchWeight.Weight)
					{
						pitchWeightsListPerGamut[i][gPitchWeightIndex] = pitchWeight;
					}
				}
			}

			return pitchWeightsListPerGamut;
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
			M.Assert(PitchVectors.Count == concatenatedGamutVector.PitchVectors.Count);

			for(int i = 0; i < PitchVectors.Count; ++i)
			{
				PitchVector pitchVector = PitchVectors[i];
				int targetPitch = pitchVector.TargetPitchWeight.Pitch;
				bool found = false;
				foreach(PitchVector cPitchVector in concatenatedGamutVector.PitchVectors)
				{
					int linkedPitch = cPitchVector.PitchWeights[0].Pitch;
					if(targetPitch == linkedPitch)
					{
						found = true;
						break;
					}
				}
				Debug.Assert(found);
			}

			List<LongGamut> gamuts = new List<LongGamut>(LongGamuts);
			List<LongGamut> cGamuts = new List<LongGamut>(concatenatedGamutVector.LongGamuts);
			gamuts.AddRange(cGamuts);

			List<PitchVector> pitchVectors = new List<PitchVector>();
			foreach(PitchVector pitchVector in PitchVectors)
			{
				List<PitchWeight> pitchWeights = new List<PitchWeight>();
				foreach(PitchVector cPitchVector in concatenatedGamutVector.PitchVectors)
				{
					if(pitchVector.TargetPitchWeight.Pitch % 12 == cPitchVector.PitchWeights[0].Pitch % 12)
					{
						pitchWeights.AddRange(pitchVector.PitchWeights);
						pitchWeights.AddRange(cPitchVector.PitchWeights);
						pitchVectors.Add(new PitchVector(pitchWeights, cPitchVector.TargetPitchWeight));
						break;
					}
				}
			}

			return new GamutVector(gamuts, concatenatedGamutVector.TargetLongGamut, pitchVectors);
		}

		/// <summary>
		/// LongGamuts does not contain TargetLongGamut
		/// </summary>
		public IReadOnlyList<LongGamut> LongGamuts { get; private set; }
		public LongGamut TargetLongGamut { get; private set; }
		public IReadOnlyList<PitchVector> PitchVectors { get; private set; }
	}
}
