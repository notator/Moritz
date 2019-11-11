using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Moritz.Globals;

namespace Moritz.Spec
{
	/// <summary>
	/// </summary>
	public class GamutVector
	{
		#region constructors

		/// <summary>
		/// The LongGamuts attribute is a time-sequence of LongGamuts as they occur in the score. The list contains at least one LongGamut.
		/// TargetLongGamut is the LongGamut following the last LongGamut in LongGamuts. This value is used when concatenating.
		/// PitchWeightVectors is a list of parallel PitchWeightVector objects. Each PitchWeightVector is a time-sequence of PitchWeights.
		///
		/// If, in a particuar LongGamut, a pitch occurs in more than one pitchWeightVector, the maximum weight is used in the corresponding Gamut.
		/// 
		/// The constructor arguments startGamut and targetGamut can have different pitches, and/or a different pitchHierarchy (=weights) and/or
		/// a different number of relative and/or absolute pitches.
		/// 
		/// In the pitchVectorEndPointsList argument:
		///    1. Each int in is in range [0..127].
		///    2. Each Tuple.Item1 is the pitch at which a pitchWeightVector begins. This value MUST be in startGamut.
		///    3. Each Tuple.Item2 is the pitchWeightVector's TargetPitchWeight.Pitch. This value MUST be in targetGamut.
		///    4. There may be more than one pitchWeightVector starting or ending with a particular pitchWeight.
		/// Omissions are also allowed:
		///     1.  If not all of the startGamut's pitches are present in the pitchVectorsData.Item1s, there will
		///         be no corresponding pitchVector connecting the missing pitch to a pitch in the target.
		///         Depending on the composition algorithm, the missing pitch may still be available.
		///     2.  If not all of the targetGamut's pitches are present in the pitchVectorsData.Item2s, there
		///         will be no corresponding pitchVector connecting to that missing pitch. (It will then
		///         suddenly become available, when the target is reached -- if the targetGamut is ever reached.)
		/// Owing to overlapping PitchWeightVectors, the LongGamuts may contain different numbers of (absolute) pitches [range 0..11].
		/// The weight of an absolute pitch (range [0..11]) in a returned Gamut will be the maximum weight for that
		/// absolute pitch in any of the constructed pitch vectors.
		/// 
		/// Precursors to this function can be found in earlier versions of Tombeau 1, and especially
		/// in my paper notebook beginning on 25 September 2019.
		/// </summary>
		/// <param name="startGamut"></param>
		/// <param name="targetGamut"></param>
		/// <param name="pitchVectorEndPointsList">All ints must be in range [0..127]. None of them have to be unique.</param>
		/// <param name="steps">An integer greater than 0 (The number of LongGamuts in this GamutVector's LongGamuts list.)</param>
		public GamutVector(LongGamut startGamut, LongGamut targetGamut, List<Tuple<int, int>> pitchVectorEndPointsList, int steps)
		{
			M.Assert(startGamut != null && targetGamut != null);
			M.Assert(pitchVectorEndPointsList != null && pitchVectorEndPointsList.Count >= 1);
			foreach(var tuple in pitchVectorEndPointsList)
			{
				M.Assert(tuple.Item1 >= 0 && tuple.Item1 <= 127);
				M.Assert(tuple.Item2 >= 0 && tuple.Item2 <= 127);
			}
			M.Assert(steps > 0);

			List<PitchWeightVector> pitchWeightVectors = new List<PitchWeightVector>();
			foreach(Tuple<int, int> pitchVectorEndPoints in pitchVectorEndPointsList)
			{
				PitchWeightVector pitchWeightVector = new PitchWeightVector(startGamut, targetGamut, pitchVectorEndPoints, steps);
				pitchWeightVectors.Add(pitchWeightVector);
			}

			List<List<PitchWeight>> absPitchWeightsListPerGamut = GetAbsPitchWeightsListPerGamut(pitchWeightVectors);

			List<LongGamut> gamuts = new List<LongGamut>();
			foreach(var absPitchWeights in absPitchWeightsListPerGamut)
			{
				List<PitchWeight> gamutPitchWeights = GetAllPitchWeights(absPitchWeights);
				LongGamut gamut = new LongGamut(gamutPitchWeights);
				gamuts.Add(gamut);
			}

			LongGamuts = gamuts;
			TargetLongGamut = new LongGamut(targetGamut.PitchWeights);
			PitchWeightVectors = pitchWeightVectors;
		}

		private List<PitchWeight> GetAllPitchWeights(List<PitchWeight> absPitchWeights)
		{
			absPitchWeights.OrderBy(x => x.Pitch);

			List<PitchWeight> gamutPitchWeights = new List<PitchWeight>();
			int relPitch = 0;
			int octave = 0;
			while(relPitch < 127)
			{
				foreach(var pitchWeight in absPitchWeights)
				{
					relPitch = pitchWeight.Pitch + octave;
					if( relPitch > 127)
					{
						break;
					}
					gamutPitchWeights.Add(new PitchWeight(relPitch, pitchWeight.Weight));
				}
				octave += 12;
			}

			return gamutPitchWeights;
		}

		public GamutVector(IReadOnlyList<LongGamut> gamuts, LongGamut targetGamut, IReadOnlyList<PitchWeightVector> pitchVectors)
		{
			LongGamuts = new List<LongGamut>(gamuts);
			TargetLongGamut = targetGamut.Clone() as LongGamut;
			List<PitchWeightVector> newPitchVectors = new List<PitchWeightVector>();
			foreach(var pitchVector in pitchVectors)
			{
				newPitchVectors.Add(new PitchWeightVector(new List<PitchWeight>(pitchVector.PitchWeights), pitchVector.TargetPitchWeight));
			}
			PitchWeightVectors = new List<PitchWeightVector>(pitchVectors);
		}

		#region constructor helpers

		/// <summary>
		/// The returned lists are ordered by Pitch.
		/// The weight associated with an absolute pitch is the maximum weight
		/// associated with that absolute pitch in any of the pitchVectors.
		/// </summary>
		/// <param name="pitchVectors"></param>
		/// <returns></returns>
		private List<List<PitchWeight>> GetAbsPitchWeightsListPerGamut(IReadOnlyList<PitchWeightVector> pitchVectors)
		{
			// all pitches in pitchVectors are in range 0..127 (has been checked before)

			List<List<PitchWeight>> absPitchWeightsPerGamut = new List<List<PitchWeight>>();
			int nGamuts = pitchVectors[0].PitchWeights.Count;
			for(int i = 0; i < nGamuts; i++)
			{
				List<PitchWeight> absPWPerGamut = new List<PitchWeight>();
				absPitchWeightsPerGamut.Add(absPWPerGamut);
			}
			for(int i = 0; i < nGamuts; i++)
			{
				var absPitchWeights = absPitchWeightsPerGamut[i];
				for(int j = 0; j < pitchVectors.Count; j++)
				{
					PitchWeight pw = pitchVectors[j].PitchWeights[i];
					int absPitch = pw.Pitch % 12;
					int index = absPitchWeights.FindIndex(x => x.Pitch == absPitch);
					if(index >= 0)
					{
						int weight = (pw.Weight > absPitchWeights[index].Weight) ? pw.Weight : absPitchWeights[index].Weight;
						absPitchWeights[index] = new PitchWeight(absPitch, weight);
					}
					else
					{
						absPitchWeights.Add(new PitchWeight(absPitch, pw.Weight));
					}
				}
				absPitchWeights.OrderBy(x => x.Pitch);
			}

			return absPitchWeightsPerGamut;
		}

		#endregion constructor helpers
		#endregion constructors

		/// <summary>
		/// Concatenating GamutVectors means that the contained PitchWeightVectors must be concatenated,
		/// so there must be the same number of PitchWeightVectors in both GamutVectors.
		/// PitchWeightVectors will only concatenate if the first PitchWeightVector's TargetPitchWeight
		/// equals the concatenated PitchVector's first PitchWeight.
		/// </summary>
		/// <param name="concatenatedGamutVector"></param>
		/// <returns></returns>
		public GamutVector Concat(GamutVector concatenatedGamutVector)
		{
			M.Assert(PitchWeightVectors.Count == concatenatedGamutVector.PitchWeightVectors.Count);

			List<PitchWeightVector> pitchWeightVectors = new List<PitchWeightVector>();
			for(int i = 0; i < PitchWeightVectors.Count; i++)
			{
				PitchWeightVector currentPitchWeightVector = PitchWeightVectors[i];
				PitchWeightVector pitchWeightVectorToConcat = concatenatedGamutVector.PitchWeightVectors[i];

				PitchWeightVector pitchWeightVector = currentPitchWeightVector.Concat(pitchWeightVectorToConcat);

				pitchWeightVectors.Add(pitchWeightVector);
			}

			List<LongGamut> gamuts = new List<LongGamut>(LongGamuts);
			List<LongGamut> cGamuts = new List<LongGamut>(concatenatedGamutVector.LongGamuts);
			gamuts.AddRange(cGamuts);

			return new GamutVector(gamuts, concatenatedGamutVector.TargetLongGamut, pitchWeightVectors);
		}

		/// <summary>
		/// LongGamuts does not contain TargetLongGamut
		/// </summary>
		public IReadOnlyList<LongGamut> LongGamuts { get; private set; }
		public LongGamut TargetLongGamut { get; private set; }
		public IReadOnlyList<PitchWeightVector> PitchWeightVectors { get; private set; }
	}
}
