using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Moritz.Globals;

namespace Moritz.Spec
{
	/// <summary>
	/// A list of at least two PitchWeight objects ordered by pitch.
	/// The PitchWeight list contains pitches that are unique and in range [0..127],
	/// Each absolute pitch occurs as a relative pitch in each possible octave in the given range.
	/// </summary>
	public class Gamut : ICloneable
	{
		protected Gamut()
		{
			PitchWeights = null; // to be set by the derived class
		}

		/// <summary>
		/// This constructor creates each PitchWeight that has the same absolute pitch with the same weight.
		/// </summary>
		/// <param name="absPitchWeightDict">keys (pitches) must be in range 0..11 and contain both rootPitch and maxPitch, the values (weights) must be in range 0..127</param>
		public Gamut(Dictionary<int, int> absPitchWeightDict)
		{
			#region checks
			M.AssertRange0_11(absPitchWeightDict.Keys);
			M.AssertRange0_127(absPitchWeightDict.Values);
			#endregion checks

			var pitchWeights = new List<PitchWeight>();
			for(int relPitch = 0; relPitch <= 127; ++relPitch)
			{
				int absPitch = relPitch % 12;
				if(absPitchWeightDict.TryGetValue(absPitch, out int weight))
				{
					PitchWeight pitchWeight = new PitchWeight(relPitch, weight);
					pitchWeights.Add(pitchWeight);
				}
			}
			PitchWeights = pitchWeights;

			AssertGamutValidity();
		}

		public Gamut(IReadOnlyList<PitchWeight> pitchWeights)
		{
			PitchWeights = new List<PitchWeight>(pitchWeights);
			AssertGamutValidity();
		}

		public virtual object Clone()
		{
			return new Gamut(PitchWeights);
		}

		/// <summary>
		/// Adds transposition (which can be positive, negative or zero) to all pitches in the PitchWeights.
		/// If the resulting pitch would be less than RootPitch or greater than MaxPitch, it is silently omitted.
		/// The PitchWeights list is then extended to ensure that each octave contains all possible absolute pitches.
		/// Transposition means that the lowest and highest octaves may not contain all the permitted absolute pitches.
		/// </summary>
		/// <param name="transposition"></param>
		/// <returns></returns>
		public void Transpose(int transposition)
		{
			if(transposition != 0)
			{
				var newPitchWeights = new List<PitchWeight>();
				foreach(var pitchWeight in PitchWeights)
				{
					int newPitch = pitchWeight.Pitch + transposition;
					if(newPitch >= 0 && newPitch <= 127)
					{
						newPitchWeights.Add(new PitchWeight(newPitch, (int)pitchWeight.Weight));
					}
				}
				PitchWeights = newPitchWeights;

				if(transposition > 0)
				{
					ExtendGamutAtLowEnd();
				}
				else
				{
					ExtendGamutAtHighEnd();
				}

				AssertGamutValidity();
			}
		}

		/// <summary>
		/// Helper function for Transpose()
		/// </summary>
		private void ExtendGamutAtLowEnd()
		{
			List<PitchWeight> lowAbsPitchWeights = GetLowAbsPitchWeights();

			List<PitchWeight> lowPitchWeights = new List<PitchWeight>();
			int pitch = -1;
			int octave = 0;
			while(pitch < PitchWeights[0].Pitch)
			{
				foreach(var pitchWeight in lowAbsPitchWeights)
				{
					pitch = pitchWeight.Pitch + octave;
					if(pitch >= PitchWeights[0].Pitch)
					{
						break;
					}
					lowPitchWeights.Add(new PitchWeight(pitch, pitchWeight.Weight));
				}
				octave += 12;
			}

			lowPitchWeights.AddRange(PitchWeights);
			PitchWeights = lowPitchWeights;
		}

		/// <summary>
		/// Helper function for ExtendGamutAtLowEnd().
		/// Returns the absolute PitchWeights in the lowest octave of the current PitchWeights
		/// in order of pitch.
		/// </summary>
		private List<PitchWeight> GetLowAbsPitchWeights()
		{
			List<PitchWeight> lowAbsPitchWeights = new List<PitchWeight>();
			List<int> absPitches = new List<int>();
			foreach(var pitchWeight in PitchWeights)
			{
				int absPitch = pitchWeight.Pitch % 12;
				if(absPitches.Contains(absPitch))
				{
					break;
				}
				absPitches.Add(absPitch);
				lowAbsPitchWeights.Add(new PitchWeight(absPitch, pitchWeight.Weight));
			}
			lowAbsPitchWeights = lowAbsPitchWeights.OrderBy(x => x.Pitch).ToList<PitchWeight>();

			return lowAbsPitchWeights;
		}

		/// <summary>
		/// Helper function for Transpose()
		/// </summary>
		private void ExtendGamutAtHighEnd()
		{
			List<PitchWeight> highAbsPitchWeights = GetHighAbsPitchWeights();

			int topPitch = PitchWeights[PitchWeights.Count - 1].Pitch;
			List<int> highPitches = new List<int>();
			int octave = 12;
			foreach(var absPitchWeight in highAbsPitchWeights)
			{
				int highPitch = absPitchWeight.Pitch;
				while(highPitch <= topPitch)
				{
					highPitch += octave;
				}
				while(highPitch <= 127)
				{
					highPitches.Add(highPitch);
					highPitch += octave;
				}
			}
			highPitches.Sort();

			List<PitchWeight> highPitchWeights = new List<PitchWeight>();			
			foreach(int pitch in highPitches)
			{
				int weightIndex = highAbsPitchWeights.FindIndex(x => x.Pitch == (pitch % 12));
				PitchWeight pitchWeight = new PitchWeight(pitch, highAbsPitchWeights[weightIndex].Weight);
				highPitchWeights.Add(pitchWeight);
			}

			List<PitchWeight> pitchWeights = new List<PitchWeight>(PitchWeights);
			pitchWeights.AddRange(highPitchWeights);
			PitchWeights = pitchWeights;
		}

		/// <summary>
		/// Helper function for ExtendGamutAtHighEnd().
		/// Returns an unordered list of PitchWeights. The Pitches are in range 0..11.
		/// The pitchWeights are derived from those in the top octave of the current PitchWeights.
		/// </summary>
		/// <returns></returns>
		private List<PitchWeight> GetHighAbsPitchWeights()
		{
			List<PitchWeight> highAbsPitchWeights = new List<PitchWeight>();
			List<int> absPitches = new List<int>();
			for(int i = PitchWeights.Count - 1; i >= 0; i--)
			{
				PitchWeight pitchWeight = PitchWeights[i];
				int absPitch = pitchWeight.Pitch % 12;
				if(absPitches.Contains(absPitch))
				{
					break;
				}
				absPitches.Add(absPitch);
				highAbsPitchWeights.Add(new PitchWeight(absPitch, pitchWeight.Weight));
			}

			return highAbsPitchWeights;
		}

		/// <summary>
		/// Throws an exception if 
		/// 1. the PitchWeights list is null or contains less than two entries, or
		/// 2. the PitchWeights list contains duplicate pitches, or
		/// 3. the pitches are not in ascending order.
		/// 4. each absolute pitch does not exist in all possible octaves
		/// </summary>
		protected void AssertGamutValidity()
		{
			if(PitchWeights == null || PitchWeights.Count < 2)
			{
				throw new ApplicationException($"{nameof(PitchWeights)} is null or too short.");
			}
			for(int i = 1; i < PitchWeights.Count; ++i)
			{
				if(PitchWeights[i - 1].Pitch >= PitchWeights[i].Pitch)
				{
					throw new ApplicationException($"Pitches must be unique and in ascending order.");
				}
			}

			for(int i = 0; i < PitchWeights.Count; ++i)
			{
				if(i > 0)
				{
					Debug.Assert(PitchWeights[i].Pitch > PitchWeights[i - 1].Pitch, $"{nameof(PitchWeights)} values must be in ascending pitch order.");
				}
			}

			var absPitches = AbsolutePitches;
			List<int> relPitches = new List<int>(Pitches);
			int minPitch = MinPitch;
			int maxPitch = MaxPitch;
			foreach(var absPitch in absPitches)
			{
				int relPitch = absPitch;
				while(relPitch <= maxPitch)
				{
					if(relPitch > minPitch && relPitches.FindIndex(x => x == relPitch) < 0)
					{
						throw new ApplicationException("Each absolute pitch must occur in each possible octave.");
					}
					relPitch += 12;
				}
			}
		}

		/// <summary>
		/// Throws an ApplicationException if the argument pitch is not found.
		/// </summary>
		/// <param name="pitch"></param>
		/// <returns></returns>
		public int Weight(int pitch)
		{
			int weight = -1;
			foreach(var pitchWeight in PitchWeights)
			{
				if(pitchWeight.Pitch == pitch)
				{
					weight = pitchWeight.Weight;
					break;
				}
			}
			if(weight == -1)
			{
				throw new ApplicationException($"Gamut.PitchWeights does not contain pitch {pitch}");
			}

			return weight;
		}

		public int MinPitch
		{
			get
			{
				M.Assert(PitchWeights != null && PitchWeights.Count > 1);

				return PitchWeights[0].Pitch;
			}
		}

		public int MaxPitch
		{
			get
			{
				M.Assert(PitchWeights != null && PitchWeights.Count > 1);

				return PitchWeights[PitchWeights.Count - 1].Pitch;
			}
		}

		/// <summary>
		/// A sorted list (values in range 0..11).
		/// </summary>
		public List<int> AbsolutePitches
		{
			get
			{
				M.Assert(PitchWeights != null && PitchWeights.Count > 1);

				List<int> absolutePitches = new List<int>();
				foreach(var pitchWeight in PitchWeights)
				{
					int absPitch = pitchWeight.Pitch % 12;
					if(absolutePitches.Contains(absPitch))
					{
						break;
					}
					absolutePitches.Add(absPitch);
				}

				absolutePitches.Sort();

				return absolutePitches;
			}
		}
		public IReadOnlyList<int> Pitches
		{
			get
			{
				M.Assert(PitchWeights != null && PitchWeights.Count > 1);

				List<int> pitches = new List<int>();
				foreach(PitchWeight pitchWeight in PitchWeights)
				{
					pitches.Add(pitchWeight.Pitch);
				}
				return pitches;
			}
		}
		public IReadOnlyList<int> Weights
		{
			get
			{
				M.Assert(PitchWeights != null && PitchWeights.Count > 1);

				List<int> weights = new List<int>();
				foreach(PitchWeight pitchWeight in PitchWeights)
				{
					weights.Add(pitchWeight.Weight);
				}
				return weights;
			}
		}

		public IReadOnlyList<PitchWeight> PitchWeights { get; protected set; }
	}

	/// <summary>
	/// A StandardGamut creates its PitchWeights list using its StandardPitchHierarchies and
	/// StandardWeights lists.
	/// </summary>
	public class StandardGamut : Gamut
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
		public static IReadOnlyList<IReadOnlyList<int>> StandardPitchHierarchies = new List<List<int>>()
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
		/// The standard weight associated with a corresponding position in the above pitch hierarchy.
		/// These standard weights can be used by Gamut constructors. 
		/// Values 1 to 8 are the values Moritz associates with fff to ppp (see MoritzStatics.cs)
		///   127 - fff
		///	  113 - ff
		///	  99 - f
		///	  85 - mf
		///	  71 - mp
		///	  57 - p
		///	  43 - pp
		///	  29 - ppp
		///	  25
		///	  21
		///	  17
		///	  14 - (pppp is 15 in MoritzStatics.cs)
		/// </summary>
		public static IReadOnlyList<int> StandardWeights = new List<int>()
		{
			127, 113, 99, 85, 71, 57, 43, 29, 25, 21, 17, 14
		};

		#region constructors
		/// <summary>
		/// This constructor creates each instance of the same absolute pitch with the same weight.
		/// </summary>
		/// <param name="pitchHierarchyIndex">The index of the pitchHierarchy in the static PitchHierarchies list (in range 0..23)</param>
		/// <param name="transposition">The value by which to transpose the pitchHierarchy (Mod 12) (in range 0..11)</param>
		/// <param name="absolutePitches">A sorted list containing unique pitches to select from the transposed pitchHierarchy (in range 0..11)</param>
		public StandardGamut(int pitchHierarchyIndex, int transposition, List<int> absolutePitches)
		{
			#region conditions
			if(pitchHierarchyIndex < 0 || pitchHierarchyIndex > 23)
			{
				throw new ApplicationException($"{nameof(pitchHierarchyIndex)} out of range.");
			}
			M.AssertRange0_11(transposition);
			M.AssertRange0_11(absolutePitches);
			for(int i = 1; i < absolutePitches.Count; i++)
			{
				M.Assert(absolutePitches[i - 1] < absolutePitches[i]);
			}
			#endregion conditions

			PitchHierarchyIndex = pitchHierarchyIndex;
			PitchHierarchyTransposition = transposition;

			List<int> pitchHierarchy = new List<int>(StandardPitchHierarchies[pitchHierarchyIndex]);

			for(int i = 0; i < pitchHierarchy.Count; i++)
			{
				pitchHierarchy[i] = (pitchHierarchy[i] + transposition) % 12;
			}

			Dictionary<int, int> absPitchWeightDict = new Dictionary<int, int>();
			foreach(int absPitch in absolutePitches)
			{
				int index = pitchHierarchy.FindIndex(x => x == absPitch);
				int weight = StandardWeights[pitchHierarchy[index]];
				absPitchWeightDict.Add(absPitch, weight);
			}

			Gamut gamut = new Gamut(absPitchWeightDict);

			PitchWeights = gamut.PitchWeights;
		}

		public override object Clone()
		{
			List<int> absolutePitches = AbsolutePitches;

			return new StandardGamut(PitchHierarchyIndex, PitchHierarchyTransposition, absolutePitches);
		}

		#endregion constructors		

		public int PitchHierarchyIndex { get; }
		public int PitchHierarchyTransposition { get; }
	}
}