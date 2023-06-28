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
		/// See CompositionAlgorithm.DoAlgorithm()
		/// </summary>
		public void Tests()
		{
			#region Gamut test1
			var absPitchSet1_Start = new List<int>() { 0, 2, 4, 11 };
			StandardGamut test1_StartGamut = new StandardGamut(0, 0, absPitchSet1_Start);

			var absPitchSet1_Target = new List<int>() { 0, 6, 7, 9, 11 };
			StandardGamut test1_TargetGamut = new StandardGamut(0, 0, absPitchSet1_Target);

			//All ints (both pitches and weights) must be in range [0..127]
			List<Tuple<int, int>> test1_PitchVectorEndPoints = new List<Tuple<int, int>>()
			{
				new Tuple<int,int>(12,11 ),
				new Tuple<int,int>( 4, 6 ),
				new Tuple<int,int>(14, 9 ),
				new Tuple<int,int>(11, 6 ),
				new Tuple<int,int>( 2, 7 )
			};
			var test1_GamutVector = new GamutVector(test1_StartGamut, test1_TargetGamut, test1_PitchVectorEndPoints, 6);

			#endregion Gamut test1

			#region Gamut test2
			Dictionary<int, int> test2_StartAbsPitchWeightDict = new Dictionary<int, int>()
			{
				{1, 100 },
				{5, 100 }
			};
			Gamut test2_StartGamut = new Gamut(test2_StartAbsPitchWeightDict);

			Dictionary<int, int> test2_TargetAbsPitchWeightDict = new Dictionary<int, int>()
			{
				{7, 50 },
				{8, 50 },
				{9, 50 }
			};
			Gamut test2_TargetGamut = new Gamut(test2_TargetAbsPitchWeightDict);

			List<Tuple<int, int>> test2_PitchVectorEndPoints = new List<Tuple<int, int>>()
			{
				new Tuple<int,int>( 17, 8 ),
				new Tuple<int,int>( 5,  7 ),
				new Tuple<int,int>( 5,  8 ),
				new Tuple<int,int>( 5,  9 ),
				new Tuple<int,int>( 13, 7 ),
				new Tuple<int,int>( 1, 21 ),
				new Tuple<int,int>( 13, 9 ),
				new Tuple<int,int>( 1, 19 )
			};
			var test2_GamutVector = new GamutVector(test2_StartGamut, test2_TargetGamut, test2_PitchVectorEndPoints, 4);

			#endregion Gamut test2

			#region Gamut test3

			Dictionary<int, int> test3_StartAbsPitchWeightDict = new Dictionary<int, int>()
			{
				{1, 100 },
				{2, 100 },
				{3, 100 }
			};
			Gamut test3_StartGamut = new Gamut(test3_StartAbsPitchWeightDict);

			Dictionary<int, int> test3_TargetAbsPitchWeightDict = new Dictionary<int, int>()
			{
				{7, 50 },
				{8, 50 },
				{9, 50 }
			};
			Gamut test3_TargetGamut = new Gamut(test3_TargetAbsPitchWeightDict);

			List<Tuple<int, int>> test3_PitchVectorEndPoints = new List<Tuple<int, int>>()
			{
				new Tuple<int,int>( 1,9 ),
				new Tuple<int,int>( 14,8 ),
				new Tuple<int,int>( 3,9 )
			};
			var test3_GamutVector = new GamutVector(test3_StartGamut, test3_TargetGamut, test3_PitchVectorEndPoints, 4);

			#endregion Gamut test3

			#region test4 transposition

			StandardGamut test3_StartGamut_Clone = test1_StartGamut.Clone() as StandardGamut;
			test3_StartGamut_Clone.Transpose(15);
			test3_StartGamut_Clone.Transpose(-30);

			#endregion test4
		}
	}
}

