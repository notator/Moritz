using System;
using System.Collections.Generic;
using System.Diagnostics;

using Krystals5ObjectLibrary;
using Moritz.Globals;
using Moritz.Palettes;
using Moritz.Spec;
using Moritz.Symbols;

namespace Moritz.Algorithm.Study4
{
	public partial class Study4Algorithm : CompositionAlgorithm
	{
		/// <summary>
		/// Returns the GamutVector for the whole of Study 4
		/// </summary>
		/// <returns></returns>
		GamutVector GetStudy4GamutVector()
		{
			/// <summary>
			/// This series of RelativePitchHierarchies is derived from the "most consonant" hierarchy at index 0:
			///                    0, 7, 4, 10, 2, 5, 9, 11, 1, 3, 6, 8
			/// which has been deduced from the harmonic series as follows (decimals rounded to 3 figures):
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
			Dictionary<int, int> gamut1AbsPitchWeightDict = new Dictionary<int, int>()
			{
				// The first 7 of the above hierarchy, weighted from ff to ppp (see Moritz.Globals)			
				{ 0, 113}, // ff
				{ 7, 99}, // f
				{ 4, 85}, // mf
				{ 10,71}, // mp
				{ 2, 57}, // p
				{ 5, 43}, // pp
				{ 9, 29}  // ppp
			};
			Gamut gamut1 = new Gamut(gamut1AbsPitchWeightDict);
			Gamut gamut2 = gamut1.Clone() as Gamut;
			gamut2.Transpose(7);
			Gamut gamut3 = gamut2.Clone() as Gamut;
			gamut3.Transpose(7);
			Gamut gamut4 = gamut3.Clone() as Gamut;
			gamut4.Transpose(7);
			Gamut gamut5 = gamut4.Clone() as Gamut;
			gamut5.Transpose(7);
			Gamut gamut6 = gamut5.Clone() as Gamut;
			gamut6.Transpose(7);
			Gamut gamut7 = gamut6.Clone() as Gamut;
			gamut7.Transpose(7);

			List<Tuple<Gamut, Gamut>> gamutsPerGamutVector = new List<Tuple<Gamut, Gamut>>
			{
				new Tuple<Gamut, Gamut>(gamut1, gamut1),
				new Tuple<Gamut, Gamut>(gamut1, gamut2),
				new Tuple<Gamut, Gamut>(gamut2, gamut1),
				new Tuple<Gamut, Gamut>(gamut1, gamut3),
				new Tuple<Gamut, Gamut>(gamut3, gamut1),
				new Tuple<Gamut, Gamut>(gamut1, gamut4),
				new Tuple<Gamut, Gamut>(gamut4, gamut1),
				new Tuple<Gamut, Gamut>(gamut1, gamut5),
				new Tuple<Gamut, Gamut>(gamut5, gamut1),
				new Tuple<Gamut, Gamut>(gamut1, gamut6),
				new Tuple<Gamut, Gamut>(gamut6, gamut1),
				new Tuple<Gamut, Gamut>(gamut1, gamut7),
				new Tuple<Gamut, Gamut>(gamut7, gamut1),
				new Tuple<Gamut, Gamut>(gamut1, null)

			};

			List<List<int>> pitchVectorEndPointsPerGamutVector = new List<List<int>>()
			{
				// Each vertical column is a global pitch vector here.
				// These are the pitch vectors in the 7th Oct. 2019 sketch in my A5 notebook.
				new List<int>(){ 0, 14,  4,  5,  7,  9, 10}, // begin region1
				new List<int>(){ 0, 14,  4,  5,  7,  9, 10}, // begin region21, end region1
				new List<int>(){ 7,  9, 11,  0, 14,  4, 17}, // begin region22, end region21
				new List<int>(){ 7, 14, 16,  5, 12,  9, 22}, // begin region31, end region22
				new List<int>(){ 6, 16, 14,  7,  9, 11, 24}, // begin region32, end region31
				new List<int>(){10, 14, 16,  5,  7,  9, 24}, // begin region41, end region32
				new List<int>(){11,  9, 16,  2,  7,  6, 25}, // begin region42, end region41
				new List<int>(){17, 10, 16,  0,  7,  9, 26}, // begin region51, end region42
				new List<int>(){13,  9, 20,  2,  6,  4, 23}, // begin region52, end region51
				new List<int>(){12,  7, 17,  4,  9,  2, 22}, // begin region61, end region52
				new List<int>(){13,  4, 18,  9, 11,  3, 20}, // begin region62, end region61
				new List<int>(){16,  7, 14, 10,  5,  9, 24}, // begin region71, end region62
				new List<int>(){20,  3, 10, 16,  1,  6, 23}, // begin region72, end region71
				new List<int>(){16,  0, 14, 10,  5,  9, 19}  // end region72
			};

			AssertConsistency(gamutsPerGamutVector, pitchVectorEndPointsPerGamutVector);

			int nPitchVectors = pitchVectorEndPointsPerGamutVector[0].Count;
			List<int> nGamutsPerGamutVector = new List<int>() { 1, 2, 2, 3, 3, 4, 4, 5, 5, 6, 6, 7, 7 };

			List<GamutVector> gamutVectors = new List<GamutVector>();
			for(int i = 0; i < pitchVectorEndPointsPerGamutVector.Count - 1; i++)
			{
				var regionBeginPVEndpoints = pitchVectorEndPointsPerGamutVector[i];
				var regionEndPVEndpoints = pitchVectorEndPointsPerGamutVector[i + 1];

				List<Tuple<int, int>> endpointsPerGamutVector = new List<Tuple<int, int>>();
				for(int j = 0; j < nPitchVectors; j++)
				{
					Tuple<int, int> pvEndpoints = new Tuple<int, int>(regionBeginPVEndpoints[j], regionEndPVEndpoints[j]);
					endpointsPerGamutVector.Add(pvEndpoints);
				}
				var startGamut = gamutsPerGamutVector[i].Item1;
				var targetGamut = gamutsPerGamutVector[i].Item2;

				var localGamutVector = new GamutVector(startGamut, targetGamut, endpointsPerGamutVector, nGamutsPerGamutVector[i]);
				gamutVectors.Add(localGamutVector);
			}

			var concatenatedGamutVector = gamutVectors[0];

			for(int i= 1; i < gamutVectors.Count; ++i)
			{
				concatenatedGamutVector = concatenatedGamutVector.Concat(gamutVectors[i]);
			}

			return concatenatedGamutVector;
		}

		/// <summary>
		/// Checks 
		/// 1. that the two arguments contain the same number of lists. 
		/// 2. that all contained lists in pitchVectorEndPointsPerGamutVector have the same Count.
		/// 3. that all values in pitchVectorEndPointsPerGamutVector are in range 0..127
		/// 4. that each Item1 gamut in gamutsPerGamutVector contains the pitches in the corresponding pitchVectorEndPointsPerGamutVector list.
		/// </summary>
		/// <param name="gamutsPerGamutVector"></param>
		/// <param name="pitchVectorEndPointsPerGamutVector"></param>
		private void AssertConsistency(List<Tuple<Gamut, Gamut>> gamutsPerGamutVector, List<List<int>> pitchVectorEndPointsPerGamutVector)
		{
			int nEntries = pitchVectorEndPointsPerGamutVector.Count;
			Debug.Assert(nEntries == gamutsPerGamutVector.Count); // condition 1

			int listCount = pitchVectorEndPointsPerGamutVector[0].Count;
			foreach(var list in pitchVectorEndPointsPerGamutVector)
			{
				Debug.Assert(listCount == list.Count); // condition 2
				foreach(var value in list)
				{
					Debug.Assert(value >= 0 && value <= 127); // condition 3
				}
			}

			List<List<int>> pitchListPerGamut = new List<List<int>>();
			for(int i = 0; i < nEntries; i++)
			{
				List<int> pitches = new List<int>(gamutsPerGamutVector[i].Item1.Pitches);
				pitchListPerGamut.Add(pitches);
			}
			for(int i = 0; i < nEntries; i++)
			{
				List<int> pitchList = pitchListPerGamut[i];
				List<int> pitchVectorStartPoints = pitchVectorEndPointsPerGamutVector[i];
				foreach(var pitchVectorStartPoint in pitchVectorStartPoints)
				{
					Debug.Assert(pitchList.Contains(pitchVectorStartPoint));
				}
			}
		}
	}
}
