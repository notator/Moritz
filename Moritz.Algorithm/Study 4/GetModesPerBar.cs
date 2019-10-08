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
		List<Mode> GetModesPerBar()
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

			List<Tuple<int, int>> region21PitchVectors = new List<Tuple<int, int>>()
			{
				new Tuple<int,int>( 0,7 ),
				new Tuple<int,int>( 14,9 ),
				new Tuple<int,int>( 4,11 ),
				new Tuple<int,int>( 5,0 ),
				new Tuple<int,int>( 7,14 ),
				new Tuple<int,int>( 9,4 ),
				new Tuple<int,int>( 10,17 ),
			};
			List<Mode> region21Modes = mode1.GetModeVector(mode2, region21PitchVectors, 2);

			List<Tuple<int, int>> region22PitchVectors = new List<Tuple<int, int>>()
			{
				new Tuple<int,int>( 0,5 ),
				new Tuple<int,int>( 2,0 ),
				new Tuple<int,int>( 4,9 ),
				new Tuple<int,int>( 5,10 ),
				new Tuple<int,int>( 7,7 ),
				new Tuple<int,int>( 9,14 ),
				new Tuple<int,int>( 11,16 ),
			};
			List<Mode> region22Modes = mode2.GetModeVector(mode1, region22PitchVectors, 2);

			List<Tuple<int, int>> region31PitchVectors = new List<Tuple<int, int>>()
			{
				new Tuple<int,int>( 12,9 ),
				new Tuple<int,int>( 2,4),
				new Tuple<int,int>( 4,2),
				new Tuple<int,int>( 5,7),
				new Tuple<int,int>( 7,6),
				new Tuple<int,int>( 9,11),
				new Tuple<int,int>( 10,12),
			};
			List<Mode> region31Modes = mode1.GetModeVector(mode3, region31PitchVectors, 3);

			List<Tuple<int, int>> region32PitchVectors = new List<Tuple<int, int>>()
			{
				new Tuple<int,int>( 0,0),
				new Tuple<int,int>( 2,4),
				new Tuple<int,int>( 4,2),
				new Tuple<int,int>( 6,10),
				new Tuple<int,int>( 7,5),
				new Tuple<int,int>( 9,7),
				new Tuple<int,int>( 11,9),
			};
			List<Mode> region32Modes = mode3.GetModeVector(mode1, region32PitchVectors, 3);

			List<Tuple<int, int>> region41PitchVectors = new List<Tuple<int, int>>()
			{
				new Tuple<int,int>(0,1 ),
				new Tuple<int,int>(14,9 ),
				new Tuple<int,int>(4,4 ),
				new Tuple<int,int>(5,2 ),
				new Tuple<int,int>(7,7 ),
				new Tuple<int,int>(9,6 ),
				new Tuple<int,int>(10,11 ),
			};
			List<Mode> region41Modes = mode1.GetModeVector(mode4, region41PitchVectors, 4);

			List<Tuple<int, int>> region42PitchVectors = new List<Tuple<int, int>>()
			{
				new Tuple<int,int>(1,2),
				new Tuple<int,int>(2,0),
				new Tuple<int,int>(4,4),
				new Tuple<int,int>(6,9),
				new Tuple<int,int>(7,7),
				new Tuple<int,int>(9,10),
				new Tuple<int,int>(11,17),
			};
			List<Mode> region42Modes = mode4.GetModeVector(mode1, region42PitchVectors, 4);

			List<Tuple<int, int>> region51PitchVectors = new List<Tuple<int, int>>()
			{
				new Tuple<int,int>(0,2),
				new Tuple<int,int>(14,11),
				new Tuple<int,int>(4,8),
				new Tuple<int,int>(5,1),
				new Tuple<int,int>(7,6),
				new Tuple<int,int>(9,4),
				new Tuple<int,int>(10,9),
			};
			List<Mode> region51Modes = mode1.GetModeVector(mode5, region51PitchVectors, 5);

			List<Tuple<int, int>> region52PitchVectors = new List<Tuple<int, int>>()
			{
				new Tuple<int,int>(1,0),
				new Tuple<int,int>(2,4),
				new Tuple<int,int>(4,2),
				new Tuple<int,int>(6,9),
				new Tuple<int,int>(8,5),
				new Tuple<int,int>(9,7),
				new Tuple<int,int>(11,10),
			};
			List<Mode> region52Modes = mode5.GetModeVector(mode1, region52PitchVectors, 5);

			List<Tuple<int, int>> region61PitchVectors = new List<Tuple<int, int>>()
			{
				new Tuple<int,int>(0,1),
				new Tuple<int,int>(2,3),
				new Tuple<int,int>(4,9),
				new Tuple<int,int>(5,6),
				new Tuple<int,int>(7,4),
				new Tuple<int,int>(9,11),
				new Tuple<int,int>(10,8),
			};
			List<Mode> region61Modes = mode1.GetModeVector(mode6, region61PitchVectors, 6);

			List<Tuple<int, int>> region62PitchVectors = new List<Tuple<int, int>>()
			{
				new Tuple<int,int>(1,4),
				new Tuple<int,int>(3,9),
				new Tuple<int,int>(4,7),
				new Tuple<int,int>(6,2),
				new Tuple<int,int>(8,12),
				new Tuple<int,int>(9,10),
				new Tuple<int,int>(11,5),
			};
			List<Mode> region62Modes = mode6.GetModeVector(mode1, region62PitchVectors, 6);

			List<Tuple<int, int>> region71PitchVectors = new List<Tuple<int, int>>()
			{
				new Tuple<int,int>(12,11),
				new Tuple<int,int>(14,10),
				new Tuple<int,int>(4,8),
				new Tuple<int,int>(5,1),
				new Tuple<int,int>(7,3),
				new Tuple<int,int>(9,6),
				new Tuple<int,int>(10,16),
			};
			List<Mode> region71Modes = mode1.GetModeVector(mode7, region71PitchVectors, 7);

			List<Tuple<int, int>> region72PitchVectors = new List<Tuple<int, int>>()
			{
				new Tuple<int,int>(1,5),
				new Tuple<int,int>(3,0),
				new Tuple<int,int>(16,10),
				new Tuple<int,int>(6,9),
				new Tuple<int,int>(8,4),
				new Tuple<int,int>(10,14),
				new Tuple<int,int>(11,7),
			};
			List<Mode> region72Modes = mode7.GetModeVector(mode1, region72PitchVectors, 7);

			List<Mode> modesPerBar = new List<Mode>();
			modesPerBar.Add(mode1);
			modesPerBar.AddRange(region21Modes);
			modesPerBar.AddRange(region22Modes);
			modesPerBar.AddRange(region31Modes);
			modesPerBar.AddRange(region32Modes);
			modesPerBar.AddRange(region41Modes);
			modesPerBar.AddRange(region42Modes);
			modesPerBar.AddRange(region51Modes);
			modesPerBar.AddRange(region52Modes);
			modesPerBar.AddRange(region61Modes);
			modesPerBar.AddRange(region62Modes);
			modesPerBar.AddRange(region71Modes);
			modesPerBar.AddRange(region72Modes);

			return modesPerBar;
		}
	}
}
