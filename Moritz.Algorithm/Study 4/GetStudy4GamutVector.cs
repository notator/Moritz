using System;
using System.Collections.Generic;
using System.Diagnostics;

using Krystals4ObjectLibrary;
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
			gamut2.Transpose(7, true);
			Gamut gamut3 = gamut2.Clone() as Gamut;
			gamut3.Transpose(7, true);
			Gamut gamut4 = gamut3.Clone() as Gamut;
			gamut4.Transpose(7, true);
			Gamut gamut5 = gamut4.Clone() as Gamut;
			gamut5.Transpose(7, true);
			Gamut gamut6 = gamut5.Clone() as Gamut;
			gamut6.Transpose(7, true);
			Gamut gamut7 = gamut6.Clone() as Gamut;
			gamut7.Transpose(7, true);

			List<Tuple<int, int>> region21PitchVectorEndPoints = new List<Tuple<int, int>>()
			{
				new Tuple<int,int>( 0,7 ),
				new Tuple<int,int>( 14,9 ),
				new Tuple<int,int>( 4,11 ),
				new Tuple<int,int>( 5,0 ),
				new Tuple<int,int>( 7,14 ),
				new Tuple<int,int>( 9,4 ),
				new Tuple<int,int>( 10,17 ),
			};
			var gamutVector21 = new GamutVector(gamut1, gamut2, region21PitchVectorEndPoints, 2);

			List<Tuple<int, int>> region22PitchVectorEndPoints = new List<Tuple<int, int>>()
			{
				new Tuple<int,int>( 0,5 ),
				new Tuple<int,int>( 2,0 ),
				new Tuple<int,int>( 4,9),
				new Tuple<int,int>( 5,10 ),
				new Tuple<int,int>( 7,7 ),
				new Tuple<int,int>( 9,14 ),
				new Tuple<int,int>( 11,16 ),
			};
			var gamutVector22 = new GamutVector(gamut2, gamut1, region22PitchVectorEndPoints, 2);

			List<Tuple<int, int>> region31PitchVectorEndPoints = new List<Tuple<int, int>>()
			{
				new Tuple<int,int>(12,9 ),
				new Tuple<int,int>( 2,4),
				new Tuple<int,int>( 4,2),
				new Tuple<int,int>( 5,7),
				new Tuple<int,int>( 7,6),
				new Tuple<int,int>( 9,11),
				new Tuple<int,int>( 10,12),
			};
			var gamutVector31 = new GamutVector(gamut1, gamut3, region31PitchVectorEndPoints, 3);

			List<Tuple<int, int>> region32PitchVectorEndPoints = new List<Tuple<int, int>>()
			{
				new Tuple<int,int>( 0,0),
				new Tuple<int,int>( 2,4),
				new Tuple<int,int>( 4,2),
				new Tuple<int,int>( 6,10),
				new Tuple<int,int>( 7,5),
				new Tuple<int,int>( 9,7),
				new Tuple<int,int>( 11,9),
			};
			var gamutVector32 = new GamutVector(gamut3, gamut1, region32PitchVectorEndPoints, 3);

			List<Tuple<int, int>> region41PitchVectorEndPoints = new List<Tuple<int, int>>()
			{
				new Tuple<int,int>( 0,1),
				new Tuple<int,int>( 14,9),
				new Tuple<int,int>( 4,4),
				new Tuple<int,int>( 5,2),
				new Tuple<int,int>( 7,7),
				new Tuple<int,int>( 9,6),
				new Tuple<int,int>( 10,11),
			};
			var gamutVector41 = new GamutVector(gamut1, gamut4, region41PitchVectorEndPoints, 4);

			List<Tuple<int, int>> region42PitchVectorEndPoints = new List<Tuple<int, int>>()
			{
				new Tuple<int,int>( 1,2),
				new Tuple<int,int>( 2,0),
				new Tuple<int,int>( 4,4),
				new Tuple<int,int>( 6,9),
				new Tuple<int,int>( 7,7),
				new Tuple<int,int>( 9,10),
				new Tuple<int,int>( 11,17),
			};
			var gamutVector42 = new GamutVector(gamut4, gamut1, region42PitchVectorEndPoints, 4);

			List<Tuple<int, int>> region51PitchVectorEndPoints = new List<Tuple<int, int>>()
			{
				new Tuple<int,int>( 0,2 ),
				new Tuple<int,int>( 14,11),
				new Tuple<int,int>( 4,8),
				new Tuple<int,int>( 5,1),
				new Tuple<int,int>( 7,6),
				new Tuple<int,int>( 9,4),
				new Tuple<int,int>( 10,9),
			};
			var gamutVector51 = new GamutVector(gamut1, gamut5, region51PitchVectorEndPoints, 5);

			List<Tuple<int, int>> region52PitchVectorEndPoints = new List<Tuple<int, int>>()
			{
				new Tuple<int,int>( 1,0),
				new Tuple<int,int>( 2,4),
				new Tuple<int,int>( 4,2),
				new Tuple<int,int>( 6,9),
				new Tuple<int,int>( 8,5),
				new Tuple<int,int>( 9,7),
				new Tuple<int,int>( 11,10),
			};
			var gamutVector52 = new GamutVector(gamut5, gamut1, region52PitchVectorEndPoints, 5);

			List<Tuple<int, int>> region61PitchVectorEndPoints = new List<Tuple<int, int>>()
			{
				new Tuple<int,int>( 0,1),
				new Tuple<int,int>( 2,3),
				new Tuple<int,int>( 4,9),
				new Tuple<int,int>( 5,6),
				new Tuple<int,int>( 7,4),
				new Tuple<int,int>( 9,11),
				new Tuple<int,int>( 10,8),
			};
			var gamutVector61 = new GamutVector(gamut1, gamut6, region61PitchVectorEndPoints, 6);

			List<Tuple<int, int>> region62PitchVectorEndPoints = new List<Tuple<int, int>>()
			{
				new Tuple<int,int>( 1,4),
				new Tuple<int,int>( 3,9),
				new Tuple<int,int>( 4,7),
				new Tuple<int,int>( 6,2),
				new Tuple<int,int>( 8,12),
				new Tuple<int,int>( 9,10),
				new Tuple<int,int>( 11,5),
			};
			var gamutVector62 = new GamutVector(gamut6, gamut1, region62PitchVectorEndPoints, 6);

			List<Tuple<int, int>> region71PitchVectorEndPoints = new List<Tuple<int, int>>()
			{
				new Tuple<int,int>( 12,11),
				new Tuple<int,int>( 14,10),
				new Tuple<int,int>( 4,8),
				new Tuple<int,int>( 5,1),
				new Tuple<int,int>( 7,3),
				new Tuple<int,int>( 9,6),
				new Tuple<int,int>( 10,16),
			};
			var gamutVector71 = new GamutVector(gamut1, gamut7, region71PitchVectorEndPoints, 7);

			List<Tuple<int, int>> region72PitchVectorEndPoints = new List<Tuple<int, int>>()
			{
				new Tuple<int,int>( 1,5),
				new Tuple<int,int>( 3,0),
				new Tuple<int,int>( 16,10),
				new Tuple<int,int>( 6,9),
				new Tuple<int,int>( 8,4),
				new Tuple<int,int>( 10,14),
				new Tuple<int,int>( 11,7),
			};
			var gamutVector72 = new GamutVector(gamut7, gamut1, region72PitchVectorEndPoints, 7);
					   			 		  		  		 	   		
			List<GamutVector> allGamutVectors = new List<GamutVector>
			{
				gamutVector21,
				gamutVector22,
				gamutVector31,
				gamutVector32,
				gamutVector41,
				gamutVector42,
				gamutVector51,
				gamutVector52,
				gamutVector61,
				gamutVector62,
				gamutVector71,
				gamutVector72
			};

			GamutVector gamutVector = allGamutVectors[0];
			for(int i= 1; i < allGamutVectors.Count; ++i)
			{
				gamutVector = gamutVector.Concat(allGamutVectors[i]);
			}

			foreach(var pitchVector in gamutVector.PitchVectors)
			{
				pitchVector.GetRange(out int minPitch1, out int maxPitch1);
				pitchVector.MinimizePitchIntervals();
				pitchVector.GetRange(out int minPitch2, out int maxPitch2);
				M.Assert(pitchVector.SetOctave(60));
				pitchVector.GetRange(out int minPitch3, out int maxPitch3);
			}

			return gamutVector;
		}
	}
}
