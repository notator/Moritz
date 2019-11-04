using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Moritz.Globals;

namespace Moritz.Spec
{
	/// <summary>
	/// A Mode is a cloneable class, containing the following public attributes:
	///    1. PitchHierarchies: A standard static List of List of int, that is a circular Mod 12 array whose
	///       first line is 0, 7, 4, 10, 2, 5, 9, 11, 1, 3, 6, 8.
	///    2. StandardWeights: A standard static list of 12 weights. This is the weight associated with a
	///       position a 12-pitch pitchHierarchy.
	///    3. PitchHierarchyIndex: The index used to find a 12-pitch pitchHierarchy in PitchHierarchies.
	///    4. RootPitch: The pitch value for the value 0 in a pitchHierarchy.
	///       This is the value by which to transpose the pitches in a pitchHierarchy to find the absolute
	///       pitchHierarchy (with C natural = 0).
	///    5. PitchSet: A HashSet containing the absolute (=UInt4) values of the pitches (with C natural = 0)
	///       contained in the Gamut.
	///    6. Gamut: a readonly list of PitchWeight objects (structs), in ascending pitch order (range [0..127]).
	///       Each pitch in the PitchSet exists at all possible octaves in the Gamut.
	///       (Each octave range in the gamut contains the same absolute pitches.)
	///       The Gamut is initially constructed with each pitch having its standard weight as found as follows:
	///       a) pitchHierarchy = PitchHierarchies[PitchHierarchyIndex]
	///       b) transpose pitchHierarchy (Mod 12) using RootPitch
	///       c) construct each octave of the Gamut using the PitchSet, the transposed pitchHierarchy and the
	///          StandardWeights
	/// The Gamut pitches' weights can be used, for example, to determine their relative velocities, durations etc.
	/// </summary>
	public class Mode : ICloneable
	{
		/// <summary>
		/// The first line of this array has been deduced from the harmonic series as follows (decimals rounded to 3 figures):
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
		/// The standard weight associated with a horizontal position in a pitch hierarchy.
		/// These weights are used by the Mode constructor.
		/// (A logarithmic scale from 64 to 127.)
		/// </summary>
		public static IReadOnlyList<int> StandardWeights = new List<int>()
		{
			127, 107, 94, 84, 78, 74, 71, 69, 67, 66, 65, 64
		};

		public int PitchHierarchyIndex { get; private set; }
		public UInt4 RootPitch { get; private set; }
		public HashSet<UInt4> PitchSet { get; private set; }
		public List<PitchWeight> Gamut { get; private set; }

		#region constructors

		public Mode(int pitchHierarchyIndex, UInt4 rootPitch, HashSet<UInt4> pitchSet, List<PitchWeight> gamut = null)
		{
			Construct(pitchHierarchyIndex, rootPitch, pitchSet, gamut);
		}
		public Mode(int pitchHierarchyIndex, int iRootPitch, HashSet<int> iAbsPitchSet, List<PitchWeight> gamut = null)
		{
			UInt4 rootPitch = (UInt4)iRootPitch;
			var pitchSet = new HashSet<UInt4>();
			foreach(var iAbsPitch in iAbsPitchSet)
			{
				pitchSet.Add((UInt4)iAbsPitch);
			}
			Construct(pitchHierarchyIndex, rootPitch, pitchSet, gamut);
		}

		private void Construct(int pitchHierarchyIndex, UInt4 rootPitch, HashSet<UInt4> pitchSet, List<PitchWeight> gamut)
		{
			PitchHierarchyIndex = pitchHierarchyIndex;
			RootPitch = rootPitch;
			PitchSet = pitchSet;

			if(gamut == null) // create a new Gamut
			{
				Gamut = GetGamut(pitchHierarchyIndex, rootPitch, pitchSet);
			}
			else
			{
				Gamut = gamut;
			}

			AssertGamutValidity(Gamut);
		}

		private List<PitchWeight> GetGamut(int pitchHierarchyIndex, UInt4 rootPitch, HashSet<UInt4> pitchSet)
		{
			IReadOnlyList<int> pitchHierarchy = PitchHierarchies[pitchHierarchyIndex];
			List<int> transpPitchHierarchy = new List<int>();
			foreach(var pitch in pitchHierarchy)
			{
				int val = (pitch + rootPitch.Int) % 12;
				transpPitchHierarchy.Add(val);
			}

			List<PitchWeight> gamut = new List<PitchWeight>();

			for(int relPitch = 0; relPitch <= UInt7.MaxValue; ++relPitch)
			{
				int absPitch = (relPitch % 12);
				if(PitchSet.Contains((UInt4)absPitch))
				{
					int index = transpPitchHierarchy.FindIndex(x => x == absPitch);
					int weight = StandardWeights[transpPitchHierarchy[index]];
					PitchWeight pitchWeight = new PitchWeight(relPitch, weight);
					gamut.Add(pitchWeight);
				}
			}
			return gamut;
		}

		public object Clone() => new Mode(PitchHierarchyIndex, RootPitch, PitchSet, Gamut);

		#endregion constructors

		/// <summary>
		/// Transposes this Mode up or down.
		/// This function changes RootPitch, PitchSet and Gamut.
		/// PitchHierarchyIndex does not change.
		/// Note that transposition can change the number of pitches in the Gamut
		/// because pitches that would be out of range are silently omitted.
		/// </summary>
		/// <param name="transposition"></param>
		public void Transpose(int transposition)
		{
			RootPitch = (UInt4)((RootPitch.Int + transposition) % 12);

			HashSet<UInt4> newPitchSet = new HashSet<UInt4>(); 
			foreach(var pitch in PitchSet)
			{
				newPitchSet.Add((UInt4)((pitch.Int + transposition) % 12));
			}
			PitchSet = newPitchSet;

			var newGamut = new List<PitchWeight>();
			foreach(var pitchWeight in Gamut)
			{
				int newPitch = pitchWeight.Pitch.Int + transposition;
				if(newPitch >= UInt7.MinValue && newPitch <= UInt7.MaxValue)
				{
					newGamut.Add(new PitchWeight(newPitch, (int) pitchWeight.Weight));
				}				
			}
			AssertGamutValidity(newGamut);
			Gamut = newGamut;			
		}

		/// <summary>
		/// Throws an exception if Gamut is invalid for any of the following reasons:
		/// 1. Gamut is null or empty.
		/// 2. All the pitch values must be different, in ascending order, and in range [0..127].
		/// 3. Each absolute pitch exists at all possible octaves above the base pitch.
		/// </summary>
		private void AssertGamutValidity(List<PitchWeight> gamut)
		{
			Debug.Assert(gamut != null && gamut.Count > 0, $"{nameof(gamut)} is null or empty.");

			for(int i = 0; i < gamut.Count; ++i)
			{
				if(i > 0)
				{
					Debug.Assert(gamut[i].Pitch > gamut[i - 1].Pitch, $"{nameof(gamut)} values must be in ascending pitch order.");
				}
				Debug.Assert(PitchSet.Contains((UInt4)(gamut[0].Pitch.Int % 12)));
			}
		}
	}
}
