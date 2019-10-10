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
		/// See CompositionAlgorithm.DoAlgorithm()
		/// </summary>
		public void Tests()
		{
			#region Mode test1
			Dictionary<int, int> test1_StartAbsPitchWeightDict = new Dictionary<int, int>()
			{
				{0, 64 },
				{4,120 },
				{2, 30 },
				{11,93 }
			};
			Mode test1_StartMode = new Mode(test1_StartAbsPitchWeightDict);

			Dictionary<int, int> test1_TargetAbsPitchWeightDict = new Dictionary<int, int>()
			{
				{7, 64 },
				{0, 64 },
				{11,120 },
				{9, 30 },
				{6, 93 }
			};
			Mode test1_TargetMode = new Mode(test1_TargetAbsPitchWeightDict);

			//All ints (both pitches and weights) are in range [0..127]
			List<Tuple<int, int>> test1_PitchVectorsData = new List<Tuple<int, int>>()
			{
				new Tuple<int,int>(12,11 ),
				new Tuple<int,int>( 4, 6 ),
				new Tuple<int,int>(14, 9 ),
				new Tuple<int,int>(11, 6 ),
				new Tuple<int,int>( 3, 7 )
			};
			var test1_ModeVector = new ModeVector(test1_StartMode, test1_TargetMode, test1_PitchVectorsData, 6);

			#endregion mode test1

			#region Mode test2
			Dictionary<int, int> test2_StartAbsPitchWeightDict = new Dictionary<int, int>()
			{
				{1, 100 },
				{5, 100 }
			};
			Mode test2_StartMode = new Mode(test2_StartAbsPitchWeightDict);

			Dictionary<int, int> test2_TargetAbsPitchWeightDict = new Dictionary<int, int>()
			{
				{7, 50 },
				{8, 50 },
				{9, 50 }
			};
			Mode test2_TargetMode = new Mode(test2_TargetAbsPitchWeightDict);

			List<Tuple<int, int>> test2_PitchVectorsData = new List<Tuple<int, int>>()
			{
				new Tuple<int,int>( 12,11 ),
				new Tuple<int,int>( 5,7 ),
				new Tuple<int,int>( 5,10 ),
				new Tuple<int,int>( 5,2 ),
				new Tuple<int,int>( 6,7 ),
				new Tuple<int,int>( 8,7 ),
				new Tuple<int,int>( 13,9 ),
				new Tuple<int,int>( 1,3 )
			};
			var test2_ModeVector = new ModeVector(test2_StartMode, test2_TargetMode, test2_PitchVectorsData, 4);

			#endregion mode test2

			#region Mode test3

			Dictionary<int, int> test3_StartAbsPitchWeightDict = new Dictionary<int, int>()
			{
				{1, 100 },
				{2, 100 },
				{3, 100 }
			};
			Mode test3_StartMode = new Mode(test3_StartAbsPitchWeightDict);

			Dictionary<int, int> test3_TargetAbsPitchWeightDict = new Dictionary<int, int>()
			{
				{7, 50 },
				{8, 50 },
				{9, 50 }
			};
			Mode test3_TargetMode = new Mode(test3_TargetAbsPitchWeightDict);

			List<Tuple<int, int>> test3_PitchVectorsData = new List<Tuple<int, int>>()
			{
				new Tuple<int,int>( 1,7 ),
				new Tuple<int,int>( 14,8 ),
				new Tuple<int,int>( 3,9 )
			};
			var test3_ModeVector = new ModeVector(test3_StartMode, test3_TargetMode, test3_PitchVectorsData, 4);

			#endregion mode test3

			#region test4 transposition

			Mode test3_StartMode_Clone = test3_StartMode.Clone() as Mode;
			test3_StartMode_Clone.Transpose(5);

			#endregion test4
		}
	}
}

