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
		/// Returns the ModeVector for the whole of Study 4
		/// </summary>
		/// <returns></returns>
		ModeVector GetStudy4ModeVector()
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
			Dictionary<int, int> mode1AbsPitchWeightDict = new Dictionary<int, int>()
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
			Mode mode1 = new Mode(mode1AbsPitchWeightDict);
			Mode mode2 = mode1.Clone() as Mode;
			mode2.Transpose(7);
			Mode mode3 = mode2.Clone() as Mode;
			mode3.Transpose(7);
			Mode mode4 = mode3.Clone() as Mode;
			mode4.Transpose(7);
			Mode mode5 = mode4.Clone() as Mode;
			mode5.Transpose(7);
			Mode mode6 = mode5.Clone() as Mode;
			mode6.Transpose(7);
			Mode mode7 = mode6.Clone() as Mode;
			mode7.Transpose(7);

			List<Tuple<int, int>> region1PitchVectorsData = new List<Tuple<int, int>>()
			{
				new Tuple<int,int>( 0,0 ),
				new Tuple<int,int>( 2,2 ),
				new Tuple<int,int>( 4,4 ),
				new Tuple<int,int>( 5,5 ),
				new Tuple<int,int>( 7,7 ),
				new Tuple<int,int>( 9,9 ),
				new Tuple<int,int>( 10,10 ),
			};
			List<Tuple<int, int>> region21PitchVectorsData = new List<Tuple<int, int>>()
			{
				new Tuple<int,int>( 0,7 ),
				new Tuple<int,int>( 14,9 ),
				new Tuple<int,int>( 4,11 ),
				new Tuple<int,int>( 5,0 ),
				new Tuple<int,int>( 7,14 ),
				new Tuple<int,int>( 9,4 ),
				new Tuple<int,int>( 10,17 ),
			};
			List<Tuple<int, int>> region22PitchVectorsData = new List<Tuple<int, int>>()
			{
				new Tuple<int,int>( 0,5 ),
				new Tuple<int,int>( 2,0 ),
				new Tuple<int,int>( 4,9),
				new Tuple<int,int>( 5,10 ),
				new Tuple<int,int>( 7,7 ),
				new Tuple<int,int>( 9,14 ),
				new Tuple<int,int>( 11,16 ),
			};
			List<Tuple<int, int>> region31PitchVectorsData = new List<Tuple<int, int>>()
			{
				new Tuple<int,int>(12,9 ),
				new Tuple<int,int>( 2,4),
				new Tuple<int,int>( 4,2),
				new Tuple<int,int>( 5,7),
				new Tuple<int,int>( 7,6),
				new Tuple<int,int>( 9,11),
				new Tuple<int,int>( 10,12),
			};
			List<Tuple<int, int>> region32PitchVectorsData = new List<Tuple<int, int>>()
			{
				new Tuple<int,int>( 0,0),
				new Tuple<int,int>( 2,4),
				new Tuple<int,int>( 4,2),
				new Tuple<int,int>( 6,10),
				new Tuple<int,int>( 7,5),
				new Tuple<int,int>( 9,7),
				new Tuple<int,int>( 11,9),
			};
			List<Tuple<int, int>> region41PitchVectorsData = new List<Tuple<int, int>>()
			{
				new Tuple<int,int>( 0,1),
				new Tuple<int,int>( 14,9),
				new Tuple<int,int>( 4,4),
				new Tuple<int,int>( 5,2),
				new Tuple<int,int>( 7,7),
				new Tuple<int,int>( 9,6),
				new Tuple<int,int>( 10,11),
			};
			List<Tuple<int, int>> region42PitchVectorsData = new List<Tuple<int, int>>()
			{
				new Tuple<int,int>( 1,2),
				new Tuple<int,int>( 2,0),
				new Tuple<int,int>( 4,4),
				new Tuple<int,int>( 6,9),
				new Tuple<int,int>( 7,7),
				new Tuple<int,int>( 9,10),
				new Tuple<int,int>( 11,17),
			};
			List<Tuple<int, int>> region51PitchVectorsData = new List<Tuple<int, int>>()
			{
				new Tuple<int,int>(0,2 ),
				new Tuple<int,int>( 14,11),
				new Tuple<int,int>( 4,8),
				new Tuple<int,int>( 5,1),
				new Tuple<int,int>( 7,6),
				new Tuple<int,int>( 9,4),
				new Tuple<int,int>( 10,9),
			};
			List<Tuple<int, int>> region52PitchVectorsData = new List<Tuple<int, int>>()
			{
				new Tuple<int,int>( 1,0),
				new Tuple<int,int>( 2,4),
				new Tuple<int,int>( 4,2),
				new Tuple<int,int>( 6,9),
				new Tuple<int,int>( 8,5),
				new Tuple<int,int>( 9,7),
				new Tuple<int,int>( 11,10),
			};
			List<Tuple<int, int>> region61PitchVectorsData = new List<Tuple<int, int>>()
			{
				new Tuple<int,int>(0,1 ),
				new Tuple<int,int>( 2,3),
				new Tuple<int,int>( 4,9),
				new Tuple<int,int>( 5,6),
				new Tuple<int,int>( 7,4),
				new Tuple<int,int>( 9,11),
				new Tuple<int,int>( 10,8),
			};
			List<Tuple<int, int>> region62PitchVectorsData = new List<Tuple<int, int>>()
			{
				new Tuple<int,int>( 1,4),
				new Tuple<int,int>( 3,9),
				new Tuple<int,int>( 4,7),
				new Tuple<int,int>( 6,2),
				new Tuple<int,int>( 8,12),
				new Tuple<int,int>( 9,10),
				new Tuple<int,int>( 11,5),
			};
			List<Tuple<int, int>> region71PitchVectorsData = new List<Tuple<int, int>>()
			{
				new Tuple<int,int>( 12,11),
				new Tuple<int,int>( 14,10),
				new Tuple<int,int>( 4,8),
				new Tuple<int,int>( 5,1),
				new Tuple<int,int>( 7,3),
				new Tuple<int,int>( 9,6),
				new Tuple<int,int>( 10,16),
			};
			List<Tuple<int, int>> region72PitchVectorsData = new List<Tuple<int, int>>()
			{
				new Tuple<int,int>( 1,5),
				new Tuple<int,int>( 3,0),
				new Tuple<int,int>( 16,10),
				new Tuple<int,int>( 6,9),
				new Tuple<int,int>( 8,4),
				new Tuple<int,int>( 10,14),
				new Tuple<int,int>( 11,7),
			};

			List<ModeVector> modeVectors = new List<ModeVector>();

			modeVectors.Add(new ModeVector(mode1, mode1, region1PitchVectorsData, 1));
			modeVectors.Add(new ModeVector(mode1, mode2, region21PitchVectorsData, 2));
			modeVectors.Add(new ModeVector(mode2, mode1, region22PitchVectorsData, 2));
			modeVectors.Add(new ModeVector(mode1, mode3, region31PitchVectorsData, 3));
			modeVectors.Add(new ModeVector(mode3, mode1, region32PitchVectorsData, 3));
			modeVectors.Add(new ModeVector(mode1, mode4, region41PitchVectorsData, 4));
			modeVectors.Add(new ModeVector(mode4, mode1, region42PitchVectorsData, 4));
			modeVectors.Add(new ModeVector(mode1, mode5, region51PitchVectorsData, 5));
			modeVectors.Add(new ModeVector(mode5, mode1, region52PitchVectorsData, 5));
			modeVectors.Add(new ModeVector(mode1, mode6, region61PitchVectorsData, 6));
			modeVectors.Add(new ModeVector(mode6, mode1, region62PitchVectorsData, 6));
			modeVectors.Add(new ModeVector(mode1, mode7, region71PitchVectorsData, 7));
			modeVectors.Add(new ModeVector(mode7, mode1, region72PitchVectorsData, 7));

			ModeVector modeVector = modeVectors[0];
			for(int i= 1; i < modeVectors.Count; ++i)
			{
				modeVector = modeVector.Concat(modeVectors[i]);
			}

			return modeVector;
		}
	}
}
