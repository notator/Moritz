using System;
using System.Collections.Generic;
using System.Diagnostics;
using Moritz.Globals;

namespace Moritz.Spec
{
	public class Gamut
	{
		/// <summary>
		/// The first line of this array:
		///     {0, 7, 4, 10, 2, 5, 9, 11, 1, 3, 6, 8}
		/// has been deduced from the harmonic series as follows (decimals rounded to 3 figures):
		/// 
		///              absolute   equal              harmonic:     absolute         closest
		///              pitch:  temperament                         harmonic    equal temperament
		///                        factor:                           factor:       absolute pitch:
		///                0:       1.000       |          1   ->   1/1  = 1.000  ->     0:
		///                1:       1.059       |          3   ->   3/2  = 1.500  ->     7:
		///                2:       1.122       |          5   ->   5/4  = 1.250  ->     4:
		///                3:       1.189       |          7   ->   7/4  = 1.750  ->     10:
		///                4:       1.260       |          9   ->   9/8  = 1.125  ->     2:
		///                5:       1.335       |         11   ->  11/8  = 1.375  ->     5:
		///                6:       1.414       |         13   ->  13/8  = 1.625  ->     9:
		///                7:       1.498       |         15   ->  15/8  = 1.875  ->     11:
		///                8:       1.587       |         17   ->  17/16 = 1.063  ->     1:
		///                9:       1.682       |         19   ->  19/16 = 1.187  ->     3:
		///                10:      1.782       |         21   ->  21/16 = 1.313  ->     
		///                11:      1.888       |         23   ->  23/16 = 1.438  ->     6:
		///                                     |         25   ->  25/16 = 1.563  ->     8:
		/// </summary>
		public static IReadOnlyList<IReadOnlyList<int>> PitchHierarchies = new List<List<int>>()
		{
			new List<int>(){0, 7, 4, 10, 2, 5, 9, 11, 1, 3, 6, 8}, // index 0 
			new List<int>(){7, 0, 10, 4, 5, 2, 11, 9, 3, 1, 8, 6}, // index 1 
			new List<int>(){7, 10, 0, 5, 4, 11, 2, 3, 9, 8, 1, 6}, // index 2
			new List<int>(){10, 7, 5, 0, 11, 4, 3, 2, 8, 9, 6, 1}, // index 3
			new List<int>(){10, 5, 7, 11, 0, 3, 4, 8, 2, 6, 9, 1}, // index 4
			new List<int>(){5, 10, 11, 7, 3, 0, 8, 4, 6, 2, 1, 9}, // index 5
			new List<int>(){5, 11, 10, 3, 7, 8, 0, 6, 4, 1, 2, 9}, // index 6
			new List<int>(){11, 5, 3, 10, 8, 7, 6, 0, 1, 4, 9, 2}, // index 7
			new List<int>(){11, 3, 5, 8, 10, 6, 7, 1, 0, 9, 4, 2}, // index 8
			new List<int>(){3, 11, 8, 5, 6, 10, 1, 7, 9, 0, 2, 4}, // index 9
			new List<int>(){3, 8, 11, 6, 5, 1, 10, 9, 7, 2, 0, 4}, // index 10
			new List<int>(){8, 3, 6, 11, 1, 5, 9, 10, 2, 7, 4, 0}, // index 11
			new List<int>(){8, 6, 3, 1, 11, 9, 5, 2, 10, 4, 7, 0}, // index 12 (inverse)
			new List<int>(){6, 8, 1, 3, 9, 11, 2, 5, 4, 10, 0, 7}, // index 13 
			new List<int>(){6, 1, 8, 9, 3, 2, 11, 4, 5, 0, 10, 7}, // index 14
			new List<int>(){1, 6, 9, 8, 2, 3, 4, 11, 0, 5, 7, 10}, // index 15
			new List<int>(){1, 9, 6, 2, 8, 4, 3, 0, 11, 7, 5, 10}, // index 16
			new List<int>(){9, 1, 2, 6, 4, 8, 0, 3, 7, 11, 10, 5}, // index 17
			new List<int>(){9, 2, 1, 4, 6, 0, 8, 7, 3, 10, 11, 5}, // index 18
			new List<int>(){2, 9, 4, 1, 0, 6, 7, 8, 10, 3, 5, 11}, // index 19
			new List<int>(){2, 4, 9, 0, 1, 7, 6, 10, 8, 5, 3, 11}, // index 20
			new List<int>(){4, 2, 0, 9, 7, 1, 10, 6, 5, 8, 11, 3}, // index 21
			new List<int>(){4, 0, 2, 7, 9, 10, 1, 5, 6, 11, 8, 3}, // index 22
			new List<int>(){0, 4, 7, 2, 10, 9, 5, 1, 11, 6, 3, 8}, // index 23
		};
		/// <summary>
		/// The standard weight associated with the corresponding position in a pitch hierarchy.
		/// These weights are used by the initial Gamut constructor.
		/// (A logarithmic scale from 64 to 127.)
		/// </summary>
		public static IReadOnlyList<int> StandardWeights = new List<int>()
		{
			127, 107, 94, 84, 78, 74, 71, 69, 67, 66, 65, 64
		};

		#region constructors
		public Gamut(int pitchHierarchyIndex, int rootPitch, HashSet<int> pitchSet)
		{
			HashSet<UInt4> UInt4PitchSet = new HashSet<UInt4>();
			foreach(var pitch in pitchSet)
			{
				UInt4PitchSet.Add((UInt4)pitch);
			}

			SetGamut(pitchHierarchyIndex, rootPitch, UInt4PitchSet);
			AssertPitchWeightsValidity();
		}

		public Gamut(int pitchHierarchyIndex, int rootPitch, HashSet<UInt4> pitchSet)
		{
			SetGamut(pitchHierarchyIndex, rootPitch, pitchSet);
			AssertPitchWeightsValidity();
		}

		public Gamut(IReadOnlyList<PitchWeight> pitchWeights)
		{
			PitchWeights = new List<PitchWeight>(pitchWeights);
			AssertPitchWeightsValidity();
		}

		/// <summary>
		/// The dictionary keys must be in range 0..11, the values must be in range 0..127
		/// </summary>
		/// <param name="absPitchWeightDict"></param>
		public Gamut(Dictionary<int, int> absPitchWeightDict)
		{
			#region conditions
			foreach(var key in absPitchWeightDict.Keys)
			{
				Debug.Assert(key >= 0 && key <= 11);
			}
			foreach(var value in absPitchWeightDict.Values)
			{
				Debug.Assert(value >= 0 && value <= 127);
			}
			#endregion conditions

			var pitchWeights = new List<PitchWeight>();
			for(int relPitch = 0; relPitch <= UInt7.MaxValue; ++relPitch)
			{
				int absPitch = relPitch % 12;
				if(absPitchWeightDict.TryGetValue(absPitch, out int weight))
				{
					PitchWeight pitchWeight = new PitchWeight(relPitch, weight);
					pitchWeights.Add(pitchWeight);
				}
			}
			PitchWeights = pitchWeights;
			AssertPitchWeightsValidity();
		}

		public object Clone()
		{
			return new Gamut(PitchWeights);
		}

		private void SetGamut(int pitchHierarchyIndex, int rootPitch, HashSet<UInt4> pitchSet)
		{
			IReadOnlyList<int> pitchHierarchy = new List<int>(PitchHierarchies[pitchHierarchyIndex]);

			List<int> transpPitchHierarchy = new List<int>();
			foreach(var pitch in pitchHierarchy)
			{
				int val = (pitch + rootPitch) % 12;
				transpPitchHierarchy.Add(val);
			}

			var pitchWeights = new List<PitchWeight>();

			for(int relPitch = 0; relPitch <= UInt7.MaxValue; ++relPitch)
			{
				UInt4 absPitch = (UInt4)(relPitch % 12);
				if(pitchSet.Contains(absPitch))
				{
					int index = transpPitchHierarchy.FindIndex(x => x == absPitch.Int);
					int weight = StandardWeights[transpPitchHierarchy[index]];
					PitchWeight pitchWeight = new PitchWeight(relPitch, weight);
					pitchWeights.Add(pitchWeight);
				}
			}
			PitchWeights = pitchWeights;
		}
		#endregion constructors

		/// <summary>
		/// Throws an exception if Gamut is invalid for any of the following reasons:
		/// 1. Gamut is null or empty.
		/// 2. All the pitch values must be different, in ascending order, and in range [0..127].
		/// 3. Each absolute pitch exists in all possible octaves.
		/// </summary>
		private void AssertPitchWeightsValidity()
		{
			Debug.Assert(PitchWeights != null && PitchWeights.Count > 0, $"{nameof(PitchWeights)} is null or empty.");

			HashSet<int> pitchSet = new HashSet<int>();
			for(int i = 0; i < PitchWeights.Count; ++i)
			{
				if(i > 0)
				{
					Debug.Assert(PitchWeights[i].Pitch > PitchWeights[i - 1].Pitch, $"{nameof(PitchWeights)} values must be in ascending pitch order.");
				}
				int absPitch = PitchWeights[i].Pitch.Int % 12;
				if(! pitchSet.Contains(absPitch))
				{
					pitchSet.Add(absPitch);
				}
			}
			List<PitchWeight> pitchWeights = new List<PitchWeight>(PitchWeights); // so that FindIndex(...) can be used.
			foreach(var absPitch in pitchSet)
			{
				int relPitch = absPitch; 
				while(relPitch <= UInt7.MaxValue)
				{
					if(pitchWeights.FindIndex(x => x.Pitch.Int == relPitch) < 0)
					{
						throw new ApplicationException("Gamut must contain each absolute pitch in each octave.");
					}
				}
				relPitch += 12;
			}
		}

		public Gamut Transpose(int transposition)
		{
			var newPitchWeights = new List<PitchWeight>();
			foreach(var pitchWeight in PitchWeights)
			{
				int newPitch = pitchWeight.Pitch.Int + transposition;
				if(newPitch >= UInt7.MinValue && newPitch <= UInt7.MaxValue)
				{
					newPitchWeights.Add(new PitchWeight(newPitch, (int)pitchWeight.Weight));
				}
			}
			PitchWeights = newPitchWeights;
			return this;
		}

		public IReadOnlyList<PitchWeight> PitchWeights { get; private set; }
	}
}