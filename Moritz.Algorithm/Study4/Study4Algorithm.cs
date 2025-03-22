using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Krystals5ObjectLibrary;
using Moritz.Globals;
using Moritz.Palettes;
using Moritz.Spec;
using Moritz.Symbols;

namespace Moritz.Algorithm.Study4
{
	public partial class Study4Algorithm : CompositionAlgorithm
	{
		public Study4Algorithm()
			: base()
		{
			CheckParameters();
		}

		public override int NumberOfMidiChannels { get { return 8; } }
		public override int NumberOfBars { get { return 55; } }
		public override IReadOnlyList<int> RegionStartBarIndices { get { return new List<int>() { 0, 1, 5, 11, 19, 29, 41 }; } }

		/// <summary>
		/// See CompositionAlgorithm.DoAlgorithm()
		/// </summary>
		public override List<Bar> DoAlgorithm(List<Krystal> krystals, List<Palette> palettes)
		{
			_krystals = krystals;
			_palettes = palettes;

			/**************************************
			* Study 4 krystals are currently:
			*    xk4(7.7.12)-2.krys
			*    xk3(7.7.4)-2.krys
			*    xk2(7.7.4)-1.krys
			*    lk1(7)-12.krys
			* Each krystal is the hierarchy (=p) input for the kystal above.
			* These krystals can be viewed by clicking the "show selected krystal" button in the Study 4 settings dialog.
			*****************************************/

			Tests();

			GamutVector study4GamutVector = GetStudy4GamutVector();

			GetTrksAndBarlines(study4GamutVector, out List<Trk> trks, out List<int> barlineMsPositions);

			Bar mainSeq = new Seq(0, trks, NumberOfMidiChannels);

			//Do global changes that affect the whole piece here (accel., rit, transpositions etc.)
			FinalizeMainSeq(mainSeq);

			List<Bar> bars = GetBars(mainSeq, barlineMsPositions, null, null);

			return bars;
		}

		/// <summary>
		/// 10.10.2019: GetStudy4GamutVector() returns 55 Gamuts.
		/// There is going to be 1 Gamut per bar, so there will be 55 bars.
		/// Regions begin at the following bar numbers:
		///   1, 2, 6, 12, 20, 30, 42
		/// (See paper notebook 7th October 2019)
		/// </summary>
		/// <param name="bars"></param>
		/// <returns></returns>
		public override ScoreData SetScoreRegionsData(List<Bar> bars)
		{
			Debug.Assert(bars.Count == NumberOfBars); // 08.10.2019

			List<(int index, int msPosition)> regionBorderlines = GetRegionBarlineIndexMsPosList(bars);

			// Each regionBorderline consists of a bar's index and its msPositionInScore.
			// The finalBarline is also included, so regionBorderlines.Count is 1 + RegionStartBarIndices.Count.

			// The following regions can be redefined later to express the existing logic better.
			RegionDef rd1 = new RegionDef("A", regionBorderlines[0], regionBorderlines[1]);
			RegionDef rd2 = new RegionDef("B", regionBorderlines[1], regionBorderlines[2]);
			RegionDef rd3 = new RegionDef("C", regionBorderlines[2], regionBorderlines[3]);
			RegionDef rd4 = new RegionDef("D", regionBorderlines[3], regionBorderlines[4]);
			RegionDef rd5 = new RegionDef("E", regionBorderlines[4], regionBorderlines[5]);
			RegionDef rd6 = new RegionDef("F", regionBorderlines[5], regionBorderlines[6]);
			RegionDef rd7 = new RegionDef("G", regionBorderlines[6], regionBorderlines[7]);

			List<RegionDef> regionDefs = new List<RegionDef>() { rd1, rd2, rd3, rd4, rd5, rd6, rd7 };

			// Temporary definition. Redefine the sequence later, when the actual bar content is known.
			// Regions can repeat in any order since they all begin with, and end pointing at, the same Gamut.
			RegionSequence regionSequence = new RegionSequence(regionDefs, "ABCDEFGA");

			ScoreData scoreData = new ScoreData(regionSequence);

			return scoreData;
		}

		#region available Trk transformations
		// Add();
		// AddRange();
		// AdjustChordMsDurations();
		// AdjustExpression();
		// AdjustVelocities();
		// AdjustVelocitiesHairpin();
		// AlignObjectAtIndex();
		// CreateAccel();
		// FindIndexAtMsPositionReFirstIUD();
		// Insert();
		// InsertRange();
		// Permute();
		// Remove();
		// RemoveAt();
		// RemoveBetweenMsPositions();
		// RemoveRange();
		// RemoveScorePitchWheelCommands();
		// Replace();
		// SetDurationsFromPitches();
		// SetPanGliss(0, subT.MsDuration, 0, 127);
		// SetPitchWheelDeviation();
		// SetPitchWheelSliders();
		// SetVelocitiesFromDurations();
		// SetVelocityPerAbsolutePitch();
		// TimeWarp();
		// Translate();
		// Transpose();
		// TransposeStepsInModeGamut();
		// TransposeToRootInModeGamut();
		#endregion available Trk transformations

		/// <summary>
		/// The compulsory first barline (at msPosition=0) is NOT included in the returned list.
		/// The compulsory final barline (at the end of the final GamutSegment) IS included in the returned list.
		/// There is a barline at the end of each voice1 gamutSegment.
		/// All the returned barline positions are unique, and in ascending order.
		/// </summary>
		private List<int> GetBarlinePositions(List<IUniqueDef> trk0iuds)
		{
			//var msValuesListList = voice1.GetMsValuesOfGamutGrpTrks();

			List<int> barlinePositions = new List<int>();
			int currentPosition = 0;
			foreach(IUniqueDef iud in trk0iuds)
			{
				currentPosition += iud.MsDuration;
				barlinePositions.Add(currentPosition);
			}

			// add further barlines here, maybe using a list provided as an argument.

			// old code:
			//foreach(IReadOnlyList<MsValues> msValuesList in msValuesListList)
			//{
			//	foreach(MsValues msValues in msValuesList)
			//	{
			//		barlinePositions.Add(msValues.EndMsPosition);
			//	}
			//}

			return barlinePositions;
		}

		/// <summary>
		/// Pad empty Trks with a single MidiRestDef.
		/// Also, do other global changes that affect the whole piece here (accel., rit, transpositions etc.).
		/// </summary>
		private void FinalizeMainSeq(Bar mainSeq)
		{
			mainSeq.PadEmptyTrks();
		}

		/// <summary>
		/// See summary and example code on abstract definition in CompositionAlogorithm.cs
		/// </summary>
		protected override List<List<SortedDictionary<int, string>>> GetClefChangesPerBar(int nBars, int nVoicesPerBar)
		{
			return null;
		}

		#region private properties for use by Study4Algorithm
		#region envelopes
		/// <summary>
		/// If domain is null, the returned shape will come from the _sliderShapesLong list. 
		/// </summary>
		/// <param name="domain">null, or in range 2..7</param>
		/// <param name="index"></param>
		/// <returns></returns>
		private IReadOnlyList<byte> SliderShape(int? domain, int index)
		{
			if(domain != null)
			{
				Debug.Assert(domain > 1 && domain < 8);
			}
			IReadOnlyList<byte> rval = null;
			switch(domain)
			{
				case 2:
					Debug.Assert(index < _sliderShapes2.Count);
					rval = _sliderShapes2[index];
					break;
				case 3:
					Debug.Assert(index < _sliderShapes3.Count);
					rval = _sliderShapes3[index];
					break;
				case 4:
					Debug.Assert(index < _sliderShapes4.Count);
					rval = _sliderShapes4[index];
					break;
				case 5:
					Debug.Assert(index < _sliderShapes5.Count);
					rval = _sliderShapes5[index];
					break;
				case 6:
					Debug.Assert(index < _sliderShapes6.Count);
					rval = _sliderShapes6[index];
					break;
				case 7:
					Debug.Assert(index < _sliderShapes7.Count);
					rval = _sliderShapes7[index];
					break;
				case null:
					Debug.Assert(index < _sliderShapesLong.Count);
					rval = _sliderShapesLong[index];
					break;
			}
			return rval;
		}
		private static IReadOnlyList<IReadOnlyList<byte>> _sliderShapes2 = new List<List<byte>>()
			{
				{ new List<byte>() {64, 0} },
				{ new List<byte>() {64, 18} },
				{ new List<byte>() {64, 36} },
				{ new List<byte>() {64, 54} },
				{ new List<byte>() {64, 72} },
				{ new List<byte>() {64, 91} },
				{ new List<byte>() {64, 109} },
				{ new List<byte>() {64, 127} }
			};
		private static IReadOnlyList<IReadOnlyList<byte>> _sliderShapes3 = new List<List<byte>>()
			{
				{ new List<byte>() {64, 0, 64} },
				{ new List<byte>() {64, 18, 64} },
				{ new List<byte>() {64, 36, 64} },
				{ new List<byte>() {64, 54, 64} },
				{ new List<byte>() {64, 72, 64} },
				{ new List<byte>() {64, 91, 64} },
				{ new List<byte>() {64, 109, 64} },
				{ new List<byte>() {64, 127, 64} }
			};
		private static IReadOnlyList<IReadOnlyList<byte>> _sliderShapes4 = new List<List<byte>>()
			{
				{ new List<byte>() {64, 0, 64, 64} },
				{ new List<byte>() {64, 22, 64, 64} },
				{ new List<byte>() {64, 22, 96, 64} },
				{ new List<byte>() {64, 64, 0, 64} },
				{ new List<byte>() {64, 64, 22, 64} },
				{ new List<byte>() {64, 64, 80, 64} },
				{ new List<byte>() {64, 80, 64, 64} },
				{ new List<byte>() {64, 96, 22, 64 } }
			};
		private static IReadOnlyList<IReadOnlyList<byte>> _sliderShapes5 = new List<List<byte>>()
			{
				{ new List<byte>() {64, 50, 72, 50, 64} },
				{ new List<byte>() {64, 64, 0, 64, 64} },
				{ new List<byte>() {64, 64, 64, 80, 64} },
				{ new List<byte>() {64, 64, 64, 106, 64} },
				{ new List<byte>() {64, 64, 127, 64, 64} },
				{ new List<byte>() {64, 70, 35, 105, 64} },
				{ new List<byte>() {64, 72, 50, 70, 64} },
				{ new List<byte>() {64, 80, 64, 64, 64} },
				{ new List<byte>() {64, 105, 35, 70, 64} },
				{ new List<byte>() {64, 106, 64, 64, 64} }
			};
		private static IReadOnlyList<IReadOnlyList<byte>> _sliderShapes6 = new List<List<byte>>()
			{
				{ new List<byte>() {64, 22, 43, 64, 64, 64} },
				{ new List<byte>() {64, 30, 78, 64, 40, 64} },
				{ new List<byte>() {64, 40, 64, 78, 30, 64} },
				{ new List<byte>() {64, 43, 106, 64, 64, 64} },
				{ new List<byte>() {64, 64, 64, 43, 22, 64} },
				{ new List<byte>() {64, 64, 64, 64, 106, 64} },
				{ new List<byte>() {64, 64, 64, 64, 127, 64} },
				{ new List<byte>() {64, 64, 64, 106, 43, 64} },
				{ new List<byte>() {64, 106, 64, 64, 64, 64} },
				{ new List<byte>() {64, 127, 127, 22, 64, 64} }
			};
		private static IReadOnlyList<IReadOnlyList<byte>> _sliderShapes7 = new List<List<byte>>()
			{
				{ new List<byte>() {64, 0, 0, 106, 106, 64, 64} },
				{ new List<byte>() {64, 28, 68, 48, 108, 88, 64} },
				{ new List<byte>() {64, 40, 20, 80, 60, 100, 64} },
				{ new List<byte>() {64, 55, 50, 75, 50, 64, 64} },
				{ new List<byte>() {64, 64, 64, 64, 64, 32, 64} },
				{ new List<byte>() {64, 64, 50, 75, 50, 55, 64} },
				{ new List<byte>() {64, 73, 78, 53, 78, 64, 64} },
				{ new List<byte>() {64, 85, 64, 106, 64, 127, 64} },
				{ new List<byte>() {64, 88, 108, 48, 68, 28, 64} },
				{ new List<byte>() {64, 100, 60, 80, 20, 40, 64} },
				{ new List<byte>() {64, 127, 127, 64, 64, 64, 64} }
			};
		private static IReadOnlyList<IReadOnlyList<byte>> _sliderShapesLong = new List<List<byte>>()
			{
				{ new List<byte>() {64, 0, 64, 96, 127, 30, 0, 64} },
				{ new List<byte>() {64, 64, 64, 127, 64, 106, 43, 64} },
				{ new List<byte>() {64, 64, 43, 43, 64, 64, 85, 22, 64} },
				{ new List<byte>() {64, 64, 64, 0, 64, 127, 0, 64, 64} },
				{ new List<byte>() {64, 80, 64, 92, 64, 64, 64, 98, 64} },
				{ new List<byte>() {64, 98, 64, 64, 64, 92, 64, 80, 64} },
				{ new List<byte>() {64, 64, 64, 0, 64, 127, 0, 64, 127, 0, 64, 64} },
				{ new List<byte>() {64, 64, 64, 64, 64, 64, 64, 64, 64, 100, 50, 100} },
				{ new List<byte>() {64, 64, 64, 64, 64, 64, 64, 64, 64, 127, 43, 127, 64} },
				{ new List<byte>() {64, 127, 43, 127, 64, 64, 64, 64, 64, 64, 64, 64, 64} },
				{ new List<byte>() {64, 64, 64, 64, 64, 64, 64, 127, 43, 127, 64, 127, 43, 127, 64} },
				{ new List<byte>() {64, 127, 43, 127, 43, 127, 64, 64, 64, 64, 64, 64, 64, 64, 64} },
				{ new List<byte>() {64, 64, 64, 0, 64, 127, 0, 64, 127, 64, 0, 64, 127, 64, 0, 64} },
				{ new List<byte>() {64, 127, 64, 64, 0, 64, 127, 0, 64, 127, 64, 0, 64, 127, 64, 0, 64} },
				{ new List<byte>() {64, 127, 43, 127, 43, 127, 64, 127, 43, 127, 43, 127, 64, 64, 64, 64, 64, 64, 64, 64, 64} }
			};
		#endregion envelopes
		#region duration modi
		private static IReadOnlyList<IReadOnlyList<int>> _durationModi = new List<List<int>>()
		{
			_durations1, _durations2, _durations3, _durations4, _durations5,_durations6,
			_durations7, _durations8, _durations9, _durations10, _durations11, _durations12

		};
		private static List<int> _durations1 = new List<int>()
			{   1000 };
		private static List<int> _durations2 = new List<int>()
			{   1000, 707 }; // 1 / ( 2^(1 / 2) )
		private static List<int> _durations3 = new List<int>()
			{   1000, 794, 630 }; // 1 / ( 2^(1 / 3) )
		private static List<int> _durations4 = new List<int>()
			{   1000, 841, 707, 595 }; // 1 / ( 2^(1 / 4) )
		private static List<int> _durations5 = new List<int>()
			{   1000, 871, 758, 660, 574 }; // 1 / ( 2^(1 / 5) )
		private static List<int> _durations6 = new List<int>()
			{   1000, 891, 794, 707, 630, 561 }; // 1 / ( 2^(1 / 6) )
		private static List<int> _durations7 = new List<int>()
			{   1000, 906, 820, 743, 673, 610, 552 }; // 1 / ( 2^(1 / 7) )
		private static List<int> _durations8 = new List<int>()
			{   1000, 917, 841, 771, 707, 648, 595, 545}; // 1 / ( 2^(1 / 8) )
		private static List<int> _durations9 = new List<int>()
			{   1000, 926, 857, 794, 735, 680, 630, 583, 540}; // 1 / ( 2^(1 / 9) )
		private static List<int> _durations10 = new List<int>()
			{   1000, 933, 871, 812, 758, 707, 660, 616, 574, 536}; // 1 / ( 2^(1 / 10) )
		private static List<int> _durations11 = new List<int>()
			{   1000, 939, 882, 828, 777, 730, 685, 643, 604, 567, 533 }; // 1 / ( 2^(1 / 11) )
		private static List<int> _durations12 = new List<int>()
			{   1000, 944, 891, 841, 794, 749, 707, 667, 630, 595, 561, 530 }; // 1 / ( 2^(1 / 12) )

		#endregion duration modi
		#endregion private properties for use by Study4Algorithm
	}
}
