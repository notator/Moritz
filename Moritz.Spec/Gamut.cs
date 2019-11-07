using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Moritz.Globals;

namespace Moritz.Spec
{
	/// <summary>
	/// A non-empty list of PitchWeight objects ordered by pitch.
	/// The pitches are unique, and in range 0..127.
	/// </summary>
	public abstract class GamutBase
	{
		protected GamutBase() { }

		/// <summary>
		/// Throws an exception if the PitchWeights list
		/// 1. is null or empty, or
		/// 2. contains duplicate pitches, or
		/// 3. the pitches are not in ascending order.
		/// </summary>
		private void AssertBaseValidity()
		{
			if(PitchWeights == null || PitchWeights.Count == 0)
			{
				throw new ApplicationException($"{nameof(PitchWeights)} is null or empty.");
			}
			for(int i = 1; i < PitchWeights.Count; ++i)
			{
				if(PitchWeights[i - 1].Pitch >= PitchWeights[i].Pitch)
				{
					throw new ApplicationException($"Pitches must be unique and in ascending order.");
				}
			}
		}

		/// <summary>
		/// Throws an ApplicationException if the argument pitch is not found.
		/// </summary>
		/// <param name="pitch"></param>
		/// <returns></returns>
		internal int Weight(int pitch)
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

		/// <summary>
		/// Adds transposition (which can be positive, negative or zero) to all pitches in the PitchWeights.
		/// If the resulting pitch would be less than 0 or greater than 127, it is silently omitted.
		/// Transposition means that some absolute pitches may be missing in some octaves. 
		/// </summary>
		/// <param name="transposition"></param>
		public virtual void Transpose(int transposition)
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

				AssertBaseValidity();
			}
		}

		public IReadOnlyList<int> Pitches
		{
			get
			{
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
				List<int> weights = new List<int>();
				foreach(PitchWeight pitchWeight in PitchWeights)
				{
					weights.Add(pitchWeight.Pitch);
				}
				return weights;
			}
		}
		public IReadOnlyList<PitchWeight> PitchWeights { get; protected set; }
	}

	/// <summary>
	/// The PitchWeight list contains pitches in range 0..127,
	/// Each possible octave exists, and contains the same absolute pitches.
	/// </summary>
	public class LongGamut : GamutBase
	{
		protected LongGamut() { }

		/// <summary>
		/// This constructor creates each PitchWeight that has the same absolute pitch with the same weight.
		/// </summary>
		/// <param name="absPitchWeightDict">keys must be in range 0..11, the values must be in range 0..127</param>
		public LongGamut(Dictionary<int, int> absPitchWeightDict)
			: base()
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

			AssertLongGamutValidity();
		}

		public LongGamut(IReadOnlyList<PitchWeight> pitchWeights)
		{
			PitchWeights = new List<PitchWeight>(pitchWeights);
		}

		public virtual object Clone()
		{
			return new LongGamut(PitchWeights);
		}

		/// <summary>
		/// Adds transposition (which can be positive, negative or zero) to all pitches.
		/// Then extends the PitchWeights list to ensure that each absolute pitch exists in all possible octaves.
		/// </summary>
		/// <param name="transposition"></param>
		/// <param name="extend"></param>
		/// <returns></returns>
		public override void Transpose(int transposition)
		{
			base.Transpose(transposition);
			if(transposition != 0)
			{
				if(transposition > 0)
				{
					ExtendGamutAtLowEnd();
				}
				else
				{
					ExtendGamutAtHighEnd();
				}

				AssertLongGamutValidity();
			}
		}

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

		private void ExtendGamutAtHighEnd()
		{
			List<PitchWeight> highAbsPitchWeights = GetHighAbsPitchWeights();

			int topPitch = PitchWeights[PitchWeights.Count - 1].Pitch;
			PitchWeight firstAbsHighPitchWeight = highAbsPitchWeights[0];
			List<PitchWeight> highPitchWeights = new List<PitchWeight>();
			int highPitch = 0;
			while(highPitch < 127)
			{
				foreach(var absPitchWeight in highAbsPitchWeights)
				{
					highPitch = absPitchWeight.Pitch;
				while(highPitch <= topPitch && highPitch < 127)
				{
					highPitch += 12;
				}
				if(highPitch > 127)
				{
					break;
				}
				topPitch = highPitch;
				highPitchWeights.Add(new PitchWeight(highPitch, absPitchWeight.Weight));
				}
			}
			highPitchWeights = highPitchWeights.OrderBy(x => x.Pitch).ToList<PitchWeight>();
			List<PitchWeight> newPitchWeights = new List<PitchWeight>(PitchWeights);
			newPitchWeights.AddRange(highPitchWeights);
			PitchWeights = newPitchWeights;
		}

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
			highAbsPitchWeights = highAbsPitchWeights.OrderBy(x => x.Pitch).ToList<PitchWeight>();

			return highAbsPitchWeights;
		}

		/// <summary>
		/// Throws an exception if each absolute pitch does not exist in all possible octaves.
		/// </summary>
		protected void AssertLongGamutValidity()
		{
			HashSet<int> pitchSet = new HashSet<int>();
			List<int> relPitches = new List<int>();
			for(int i = 0; i < PitchWeights.Count; ++i)
			{
				if(i > 0)
				{
					Debug.Assert(PitchWeights[i].Pitch > PitchWeights[i - 1].Pitch, $"{nameof(PitchWeights)} values must be in ascending pitch order.");
				}
				relPitches.Add(PitchWeights[i].Pitch);
				pitchSet.Add(PitchWeights[i].Pitch % 12); // The bool return value is ignored here.
			}

			foreach(var absPitch in pitchSet)
			{
				int relPitch = absPitch;
				while(relPitch <= 127)
				{
					if(relPitches.FindIndex(x => x == relPitch) < 0)
					{
						throw new ApplicationException("Gamut must contain each absolute pitch in each octave.");
					}
					relPitch += 12;
				}
			}

		}
	}

	/// <summary>
	/// A StandardGamut creates its PitchWeights list using its StandardPitchHierarchies and
	/// StandardWeights lists.
	/// </summary>
	public class StandardGamut : LongGamut
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
		/// <param name="rootPitch">The value by which to transpose the pitchHierarchy (Mod 12) (in range 0..11)</param>
		/// <param name="absolutePitches">The pitches to select from the transposed pitchHierarchy (in range 0..11)</param>
		public StandardGamut(int pitchHierarchyIndex, int rootPitch, HashSet<int> absolutePitches)
			: base()
		{
			if(pitchHierarchyIndex < 0 || pitchHierarchyIndex > 23)
			{
				throw new ApplicationException($"{nameof(pitchHierarchyIndex)} out of range.");
			}
			M.AssertRange0_11(rootPitch);
			M.AssertRange0_11(absolutePitches);

			PitchHierarchyIndex = pitchHierarchyIndex;
			RootPitch = rootPitch;
			AbsolutePitches = new HashSet<int>(absolutePitches);

			List<int> pitchHierarchy = new List<int>(StandardPitchHierarchies[pitchHierarchyIndex]);

			for(int i = 0; i < pitchHierarchy.Count; i++)
			{
				pitchHierarchy[i] = (pitchHierarchy[i] + rootPitch) % 12;
			}

			var pitchWeights = new List<PitchWeight>();
			for(int relPitch = 0; relPitch <= 127; ++relPitch)
			{
				int absPitch = relPitch % 12;
				if(absolutePitches.Contains(absPitch))
				{
					int index = pitchHierarchy.FindIndex(x => x == absPitch);
					int weight = StandardWeights[pitchHierarchy[index]];
					PitchWeight pitchWeight = new PitchWeight(relPitch, weight);
					pitchWeights.Add(pitchWeight);
				}
			}
			PitchWeights = pitchWeights;

			AssertLongGamutValidity();
		}

		public StandardGamut(IReadOnlyList<PitchWeight> pitchWeights)
		{
			// don't implement!
			throw new NotImplementedException();
		}

		public StandardGamut(Dictionary<int, int> absPitchWeightDictList)
		{
			// don't implement!
			throw new NotImplementedException();
		}

		public override object Clone()
		{
			return new StandardGamut(PitchHierarchyIndex, RootPitch, AbsolutePitches);
		}

		#endregion constructors		

		public int PitchHierarchyIndex { get; private set; }
		public int RootPitch { get; private set; }
		public HashSet<int> AbsolutePitches { get; private set; }
	}
}