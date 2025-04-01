using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Moritz.Globals;

namespace Moritz.Spec
{
	/// <summary>
	/// A temporal sequence of Gamuts, a TargetGamut (not contained in the sequence), and
	/// a list of parallel PitchWeightVectors connecting the Gamuts.
	/// The PitchWeightVectors remind me somehow of the wiring inside a sequence of Enigma rotors...
	/// </summary>
	public class GamutVector
	{
		#region constructors
		/// <summary>
		/// The Gamuts attribute is a time-sequence of Gamuts as they occur in the score. The list contains at least one Gamut.
		/// TargetGamut is the Gamut following the last Gamut in Gamuts. This value is used when concatenating GamutVectors.
		/// PitchWeightVectors is a list of parallel PitchWeightVector objects. Each PitchWeightVector is a time-sequence of PitchWeights.
		///
		/// If, in a particuar returned Gamut, a pitch occurs in more than one pitchWeightVector, the maximum weight is used.
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
		/// Owing to overlapping PitchWeightVectors, the Gamuts may contain different numbers of (absolute) pitches [range 0..11].
		/// The weight of an absolute pitch (range [0..11]) in a returned Gamut will be the maximum weight for that
		/// absolute pitch in any of the constructed pitch vectors.
		/// 
		/// Precursors to this function can be found in earlier versions of Tombeau 1, and especially
		/// in my paper notebook beginning on 25 September 2019.
		/// </summary>
		/// <param name="startGamut"></param>
		/// <param name="targetGamut"></param>
		/// <param name="pitchVectorEndPointsList">All ints must be in range [0..127]. None of them have to be unique.</param>
		/// <param name="steps">An integer greater than 0 (The number of Gamuts in this GamutVector's Gamuts list.)</param>
		public GamutVector(Gamut startGamut, Gamut targetGamut, List<Tuple<int, int>> pitchVectorEndPointsList, int steps)
		{
			M.Assert(startGamut != null && targetGamut != null);
			M.Assert(pitchVectorEndPointsList != null && pitchVectorEndPointsList.Count >= 1);
			foreach(var tuple in pitchVectorEndPointsList)
			{
				M.Assert(tuple.Item1 >= startGamut.MinPitch && tuple.Item1 <= startGamut.MaxPitch);
				M.Assert(tuple.Item2 >= targetGamut.MinPitch && tuple.Item2 <= targetGamut.MaxPitch);
			}
			M.Assert(steps > 0);

			List<PitchWeightVector> pitchWeightVectors = new List<PitchWeightVector>();
			foreach(Tuple<int, int> pitchVectorEndPoints in pitchVectorEndPointsList)
			{
				PitchWeightVector pitchWeightVector = new PitchWeightVector(startGamut, targetGamut, pitchVectorEndPoints, steps);
				pitchWeightVectors.Add(pitchWeightVector);
			}

			List<Dictionary<int,int>> absPitchWeightsDictPerGamut = GetAbsPitchWeightDictPerGamut(pitchWeightVectors);

			List<Gamut> gamuts = new List<Gamut>();
			foreach(var pitchWeightDict in absPitchWeightsDictPerGamut)
			{
				Gamut gamut = new Gamut(pitchWeightDict);
				gamuts.Add(gamut);
			}

			Gamuts = gamuts;
			TargetGamut = (Gamut) targetGamut.Clone();
			PitchWeightVectors = pitchWeightVectors;
		}

		public GamutVector(IReadOnlyList<Gamut> gamuts, Gamut targetGamut, IReadOnlyList<PitchWeightVector> pitchVectors)
		{
			Gamuts = new List<Gamut>(gamuts);
			TargetGamut = targetGamut.Clone() as Gamut;
			List<PitchWeightVector> newPitchVectors = new List<PitchWeightVector>();
			foreach(var pitchVector in pitchVectors)
			{
				newPitchVectors.Add(new PitchWeightVector(new List<PitchWeight>(pitchVector.PitchWeights), pitchVector.TargetPitchWeight));
			}
			PitchWeightVectors = new List<PitchWeightVector>(pitchVectors);
		}

		#region constructor helpers

		/// <summary>
		/// The weight associated with an absolute pitch is the maximum weight
		/// associated with that absolute pitch in any of the pitchVectors.
		/// </summary>
		/// <param name="pitchVectors"></param>
		/// <returns></returns>
		private List<Dictionary<int, int>> GetAbsPitchWeightDictPerGamut(IReadOnlyList<PitchWeightVector> pitchVectors)
		{
			// all pitches in pitchVectors are in range 0..127 (has been checked before)

			List<Dictionary<int, int>> absPitchWeightDictPerGamut = new List<Dictionary<int, int>>();
			int nGamuts = pitchVectors[0].PitchWeights.Count;
			for(int i = 0; i < nGamuts; i++)
			{
				Dictionary<int, int> absPWDict = new Dictionary<int, int>();
				absPitchWeightDictPerGamut.Add(absPWDict);
			}

			for(int i = 0; i < nGamuts; i++)
			{
				var absPWDict = absPitchWeightDictPerGamut[i];
				for(int j = 0; j < pitchVectors.Count; j++)
				{
					PitchWeight pw = pitchVectors[j].PitchWeights[i];
					int absPitch = pw.Pitch % 12;
					if(absPWDict.ContainsKey(absPitch))
					{
						int weight = (pw.Weight > absPWDict[absPitch]) ? pw.Weight : absPWDict[absPitch];
						absPWDict[absPitch] = weight;
					}
					else
					{
						absPWDict.Add(absPitch, pw.Weight);
					}
				}
			}

			return absPitchWeightDictPerGamut;
		}

		#endregion constructor helpers
		#endregion constructors

		/// <summary>
		/// Concatenating GamutVectors means that the contained PitchWeightVectors must be concatenated,
		/// so there must be the same number of PitchWeightVectors in both GamutVectors.
		/// PitchWeightVectors will only concatenate if the first PitchWeightVector's TargetPitchWeight
		/// equals the concatenated PitchVector's first PitchWeight.
		/// Consecutive Enigma wheels... :-)
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

			List<Gamut> gamuts = new List<Gamut>(Gamuts);
			List<Gamut> cGamuts = new List<Gamut>(concatenatedGamutVector.Gamuts);
			gamuts.AddRange(cGamuts);

			return new GamutVector(gamuts, concatenatedGamutVector.TargetGamut, pitchWeightVectors);
		}

		/// <summary>
		/// Gamuts does not contain TargetGamut
		/// </summary>
		public IReadOnlyList<Gamut> Gamuts { get; private set; }
		public Gamut TargetGamut { get; private set; }
		public IReadOnlyList<PitchWeightVector> PitchWeightVectors { get; private set; }
	}
}
