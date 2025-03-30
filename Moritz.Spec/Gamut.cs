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
		protected Gamut() { }

		/// <summary>
		/// This constructor creates each PitchWeight that has the same absolute pitch with the same weight.
		/// </summary>
		/// <param name="absPitchWeightDict">keys (pitches) must be in range 0..11 and contain both rootPitch and maxPitch, the values (weights) must be in range 0..127</param>
		public Gamut(Dictionary<int, int> absPitchWeightDict)
		{
			#region checks
            foreach(var item in absPitchWeightDict)
            {
                Debug.Assert(item.Key >= 0 && item.Key <= 11);
                Debug.Assert(item.Value >= 0 && item.Value <= 127);
            }
			#endregion checks

			var absolutePitchWeights = new List<PitchWeight>();
			foreach(var absPitch in absPitchWeightDict.Keys)
			{
				absolutePitchWeights.Add(new PitchWeight(absPitch, absPitchWeightDict[absPitch]));
			}

			absolutePitchWeights = new List<PitchWeight>(absolutePitchWeights.OrderByDescending(x => x.Weight));

			SetAttributes(absolutePitchWeights);
		}

		/// <summary>
		/// Sets all fixed attributes that depend on the absolutePitchWeights argument.
		/// The pitchWeights in the argument must
		///    1. exist, and be more than one.
		///    2. be in descending order of weight (in range 0..127) (weights can be equal).
		///    3. contain unique absolute pitches (in range 0..11)
		/// </summary>
		/// <param name="absolutePitchWeights"></param>
		protected void SetAttributes(IReadOnlyList<PitchWeight> absolutePitchWeights)
		{
			#region conditions
			Debug.Assert(absolutePitchWeights != null && absolutePitchWeights.Count > 1);
			foreach(var pitchWeight in absolutePitchWeights)
			{
				Debug.Assert(pitchWeight.Pitch >= 0 && pitchWeight.Pitch <= 11, "Absolute pitch out of range.");
				Debug.Assert(pitchWeight.Weight >= 1 && pitchWeight.Pitch <= 127, "Weight out of range.");
			}
			var exists = new List<int>();
			for(int i = 0; i < absolutePitchWeights.Count; i++)
			{
				Debug.Assert(!exists.Contains(absolutePitchWeights[i].Pitch), "Absolute pitches must be unique.");
				exists.Add(absolutePitchWeights[i].Pitch);
			}
			for(int i = 1; i < absolutePitchWeights.Count; i++)
			{
				Debug.Assert(absolutePitchWeights[i - 1].Weight >= absolutePitchWeights[i].Weight, "Weights must be in descending order.");
			}
			#endregion conditions

			_absolutePitchWeights = (List<PitchWeight>) absolutePitchWeights;
			SetAbsolutePitches();
			SetPitchWeights();
			SetLinearChordShapesMatrix();
		}

		#region SetAttributes helpers
		/// <summary>
		/// Sets AbsolutePitches containing the absolute pitches (possible range 0..11) sorted by inceasing Pitch.
		/// </summary>
		private void SetAbsolutePitches()
		{
			_absolutePitches = new List<int>();
			foreach(var pitchWeight in _absolutePitchWeights)
			{
				_absolutePitches.Add(pitchWeight.Pitch);
			}
			_absolutePitches.Sort();
		}
		/// <summary>
		/// Sets PitchWeights containing all the relative pitches (possible range 0..127) sorted by inceasing Pitch.
		/// </summary>
		private void SetPitchWeights()
		{
			var pitchWeights = new List<PitchWeight>();
			var defaultPitchWeight = new PitchWeight();
			for(int relPitch = 0; relPitch <= 127; ++relPitch)
			{
				int absPitch = relPitch % 12;
				PitchWeight absPitchWeight = _absolutePitchWeights.Find(x => x.Pitch == absPitch);
				if(absPitchWeight != defaultPitchWeight)
				{
					PitchWeight pitchWeight = new PitchWeight(relPitch, absPitchWeight.Weight);
					pitchWeights.Add(pitchWeight);
				}
			}
			_pitchWeights = pitchWeights;

			#region check PitchWeights
			Debug.Assert(PitchWeights[0].Pitch >= 0 && PitchWeights[PitchWeights.Count - 1].Pitch <= 127, "Pitch out of range.");
			for(int i = 1; i < PitchWeights.Count; ++i)
			{
				Debug.Assert(PitchWeights[i].Pitch > PitchWeights[i - 1].Pitch, "Pitches must be unique and in ascending order.");
			}
			var absPitches = AbsolutePitches;
			List<int> relPitches = new List<int>(Pitches);
			foreach(var absPitch in absPitches)
			{
				int relPitch = absPitch;
				while(relPitch <= 127)
				{
					Debug.Assert(relPitches.FindIndex(x => x == relPitch) >= 0, "Each absolute pitch must occur in each possible octave.");
					relPitch += 12;
				}
			}
			#endregion check PitchWeights
		}
		/// <summary>
		/// Sets LinearChordShapesMatrix which is a linear matrix whose top row is the gamut's absolute pitches in descending order of weight 
		/// (the order they have in the AbsolutePitchWeights attribute).
		/// </summary>
		private void SetLinearChordShapesMatrix()
		{
			List<int> inversion0 = new List<int>();
			foreach(var pitchWeight in _absolutePitchWeights)
			{
				inversion0.Add((int)pitchWeight.Pitch);
			}
			_linearChordShapesMatrix = M.GetLinearMatrix(inversion0);
		}
		#endregion

		public Gamut(IReadOnlyList<PitchWeight> absolutePitchWeights)
		{
			SetAttributes(absolutePitchWeights);
		}

		public virtual object Clone()
		{
			return new Gamut(AbsolutePitchWeights);
		}

		/// <summary>
		/// Adds transposition (which can be positive, negative or zero) to all absolute pitches.
		/// (Negative transpositions are implemented by adding 12 repeatedly until the transposition is zero or positive.)
		/// The resulting absolute pitches exist at all possible octaves in the resulting PitchWeights attribute.
		/// </summary>
		/// <param name="transposition"></param>
		/// <returns></returns>
		public void Transpose(int transposition)
		{
			if(transposition != 0)
			{
				while(transposition < 0)
				{
					transposition += 12;
				}
				List<PitchWeight> trAbsPitchWeights = new List<PitchWeight>();
				foreach(var pitchWeight in _absolutePitchWeights)
				{
					int pitch = (pitchWeight.Pitch + transposition) % 12;
					trAbsPitchWeights.Add(new PitchWeight(pitch, pitchWeight.Weight));
				}

				SetAttributes(trAbsPitchWeights); // trAbsPitchWeights is already ordered by descending weight
			}
		}

		/// <summary>
		/// Throws an Exception if the argument pitch is not found.
		/// </summary>
		/// <returns></returns>
		public int Weight(int pitch)
		{
			PitchWeight defaultPitchWeight = new PitchWeight();
			PitchWeight pitchWeight = ((List<PitchWeight>)PitchWeights).Find(x => x.Pitch == pitch);
			Debug.Assert(pitchWeight != defaultPitchWeight, $"Gamut.PitchWeights does not contain pitch {pitch}");
			return pitchWeight.Weight;
		}

		public int MinPitch
		{
			get
			{
				Debug.Assert(PitchWeights != null && PitchWeights.Count > 1);

				return PitchWeights[0].Pitch;
			}
		}
		public int MaxPitch
		{
			get
			{
				Debug.Assert(PitchWeights != null && PitchWeights.Count > 1);

				return PitchWeights[PitchWeights.Count - 1].Pitch;
			}
		}

		public IReadOnlyList<int> Pitches
		{
			get
			{
				Debug.Assert(PitchWeights != null && PitchWeights.Count > 1);

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
				Debug.Assert(PitchWeights != null && PitchWeights.Count > 1);

				List<int> weights = new List<int>();
				foreach(PitchWeight pitchWeight in PitchWeights)
				{
					weights.Add(pitchWeight.Weight);
				}
				return weights;
			}
		}

		/// <summary>
		/// A list of PitchWeights containing the absolute Pitch values (in range 0..11) in decreasing order of weight.
		/// </summary>
		public IReadOnlyList<PitchWeight> AbsolutePitchWeights { get { return _absolutePitchWeights; }}
		protected List<PitchWeight> _absolutePitchWeights;
		/// <summary>
		/// A list of the absolute pitches (possible range 0..11) ordered by increasing pitch.
		/// </summary>
		public IReadOnlyList<int> AbsolutePitches { get { return _absolutePitches; }	}
		protected List<int> _absolutePitches;
		/// <summary>
		/// A list of PitchWeights containing all the relative Pitch values (possible range 0..127) ordered by increasing pitch.
		/// </summary>
		public IReadOnlyList<PitchWeight> PitchWeights { get { return _pitchWeights; } }
		protected List<PitchWeight> _pitchWeights;
		/// <summary>
		/// LinearChordShapesMatrix is a linear matrix whose top row (index 0) is the gamut's absolute pitches
		/// in descending order of their weight (the order they have in the AbsolutePitchWeights attribute).
		/// </summary>
		public IReadOnlyList<List<int>> LinearChordShapesMatrix { get { return _linearChordShapesMatrix; } }
		protected List<List<int>> _linearChordShapesMatrix;
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
            Debug.Assert(transposition >= 0 && transposition <= 11);
            Debug.Assert(absolutePitches[0] >= 0 && absolutePitches[0] <= 11);

			for(int i = 1; i < absolutePitches.Count; i++)
			{
                Debug.Assert(absolutePitches[i] >= 0 && absolutePitches[i] <= 11);
                Debug.Assert(absolutePitches[i - 1] < absolutePitches[i]);
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

			SetAttributes(gamut.AbsolutePitchWeights);
		}

		public override object Clone()
		{
			List<int> absolutePitches = new List<int>(AbsolutePitches);

			return new StandardGamut(PitchHierarchyIndex, PitchHierarchyTransposition, absolutePitches);
		}

		#endregion constructors		

		public int PitchHierarchyIndex { get; }
		public int PitchHierarchyTransposition { get; }
	}
}