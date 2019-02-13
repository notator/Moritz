using System;
using System.Collections.Generic;
using System.Diagnostics;

using Krystals4ObjectLibrary;
using Moritz.Globals;
using Moritz.Palettes;
using Moritz.Spec;

namespace Moritz.Algorithm.ThreeCrashes
{
    public class ThreeCrashesAlgorithm : CompositionAlgorithm
	{
		public ThreeCrashesAlgorithm()
            : base()
        {
            CheckParameters();
        }

        public override IReadOnlyList<int> MidiChannelPerOutputVoice { get{	return new List<int>() { 0, 1, 2, 3, 4, 5 }; }}
		public override int NumberOfBars { get{	return 27; }}
		public override IReadOnlyList<int> MidiChannelPerInputVoice { get { return null; } }

		private readonly int nKeyboardPitches = 85;
		// crashes 1-3 contain nKeyboardPitches values each -- not the 89 values described in the online analysis.
		private static readonly IReadOnlyList<IReadOnlyList<byte>> crashAWagons = new List<List<byte>>()
		{
			new List<byte>() // Wagon A1, 17 values
			{10, 13, 17, 27, 29, 37, 47, 49, 54, 57, 58, 62, 64, 66, 68, 73, 80 },
			new List<byte>() // Wagon A2, 17 values
			{19, 20, 24, 25, 32, 34, 36, 38, 40, 55, 56, 59, 74, 78, 79, 82, 85 },
			new List<byte>() // Wagon A3, 17 values
			{ 1,  4,  5, 11, 15, 21, 23, 28, 39, 42, 46, 48, 60, 65, 69, 72, 77 },
			new List<byte>() // Wagon A4, 17 values
			{14, 18, 26, 30, 33, 35, 43, 44, 45, 50, 51, 52, 61, 63, 76, 81, 84 },
			new List<byte>() // Wagon A5, 17 values
			{ 2,  3,  6,  7,  8,  9, 12, 16, 22, 31, 41, 53, 67, 70, 71, 75, 83 },
			new List<byte>() // All A Wagons interspersed, nKeyboardPitches values
			{10, 19,  1, 14,  2, 13, 20,  4, 18,  3,
			 17, 24,  5, 26,  6, 27, 25, 11, 30,  7,
			 29, 32, 15, 33,  8, 37, 34, 21, 35,  9,
			 47, 36, 23, 43, 12, 49, 38, 28, 44, 16,
			 54, 40, 39, 45, 22, 57, 55, 42, 50, 31,
			 58, 56, 46, 51, 41, 62, 59, 48, 52, 53,
			 64, 74, 60, 61, 67, 66, 78, 65, 63, 70,
			 68, 79, 69, 76, 71, 73, 82, 72, 81, 75,
			 80, 85, 77, 84, 83 }
		};
		private static readonly IReadOnlyList<IReadOnlyList<byte>> crashBWagons = new List<List<byte>>()
		{
			new List<byte>() // Wagon B1, 23 values
			{ 2,  4, 15, 20, 26, 32, 35, 37, 39, 41, 46, 48, 49, 52, 56, 60, 63, 65, 67, 77, 78, 82, 83 },
			new List<byte>() // Wagon B2, 20 values
			{ 3,  6,  7, 12, 16, 22, 29, 30, 31, 34, 53, 58, 59, 68, 69, 70, 74, 79, 84, 85 },
			new List<byte>() // Wagon B3, 17 values
			{ 9, 10, 13, 14, 24, 25, 42, 43, 44, 61, 64, 66, 71, 72, 73, 76, 81 },
			new List<byte>() // Wagon B4, 14 values
			{ 1,  8, 17, 19, 21, 28, 36, 50, 51, 54, 57, 62, 75, 80 },
			new List<byte>() // Wagon B5, 11 values
			{ 5, 11, 18, 23, 27, 33, 38, 40, 45, 47, 55 },
			new List<byte>() // All B Wagons interspersed, nKeyboardPitches values
			{ 2,  3,  9,  1,  5,  4,  6, 10,  8, 11,
			 15,  7, 13, 17, 18, 20, 12, 14, 19, 23,
			 26, 16, 24, 21, 27, 32, 22, 25, 28, 33,
			 35, 29, 42, 36, 38, 37, 30, 43, 50, 40,
			 39, 31, 44, 51, 45, 41, 34, 61, 54, 47,
			 46, 53, 64, 57, 55, 48, 58, 66, 62, 49,
			 59, 71, 75, 52, 68, 72, 80, 56, 69, 73,
			 60, 70, 76, 63, 74, 81, 65, 79, 67, 84,
			 77, 85, 78, 82, 83 }
		};
		private static readonly IReadOnlyList<IReadOnlyList<byte>> crashCWagons = new List<List<byte>>()
		{
			new List<byte>() // Wagon C1, 29 values
			{ 1,  4,  5,  7, 14, 20, 30, 32, 34, 35, 38, 40, 41, 46, 50, 53, 57, 61, 63, 66, 68, 69, 72, 74, 75, 76, 77, 78, 82 },
			new List<byte>() // Wagon C2, 23 values
			{ 8,  9, 10, 13, 15, 19, 24, 26, 29, 33, 37, 39, 43, 44, 45, 49, 51, 52, 54, 62, 67, 70, 85 },
			new List<byte>() // Wagon C3, 17 values
			{ 2,  3, 11, 16, 21, 25, 28, 31, 36, 47, 48, 55, 60, 65, 81, 83, 84 },
			new List<byte>() // Wagon C4, 11 values
			{ 6, 12, 17, 18, 56, 58, 59, 64, 71, 79, 80 },
			new List<byte>() // Wagon C5, 5 values
			{ 22, 23, 27, 42, 73 },
			new List<byte>() // All C Wagons interspersed, nKeyboardPitches values
			{ 1,  8,  2,  6, 22,  4,  9,  3, 12, 23,
			  5, 10, 11, 17, 27,  7, 13, 16, 18, 42,
			 14, 15, 21, 56, 73, 20, 19, 25, 58, 30,
			 24, 28, 59, 32, 26, 31, 64, 34, 29, 36,
			 71, 35, 33, 47, 79, 38, 37, 48, 80, 40,
			 39, 55, 41, 43, 60, 46, 44, 65, 50, 45,
			 81, 53, 49, 83, 57, 51, 84, 61, 52, 63,
			 54, 66, 62, 68, 67, 69, 70, 72, 85, 74,
			 75, 76, 77, 78, 82 }
		};

		private static readonly List<List<int>> basicVelocityIndexPerWagonPerAngPos = new List<List<int>>()
			{
				new List<int>() {0,2,4,6,8,9,7,5,3,1}, // wagonIndex 0
				new List<int>() {3,3,4,5,7,7,6,5,4,2}, // wagonIndex 1
				new List<int>() {5,5,5,5,5,5,5,5,5,5}, // wagonIndex 2
				new List<int>() {7,7,6,5,3,3,4,5,6,8}, // wagonIndex 3
				new List<int>() {9,7,5,3,1,0,2,4,6,8}  // wagonIndex 4
			};
		// 10 velocities, equally spaced, from 28 to 127
		// c.f. MoritzStatics.cs/MaxMidiVelocity: -- 9 dynamic ranges, equally spaced from [0..15] (=pppp) to [113..127] (=fff) --. 
		private static readonly List<byte> basicVelocities = new List<byte>() { 28, 39, 50, 61, 72, 83, 94, 105, 116, 127 }; 

		// Neither the krystals, nor the palettes argument is used.
		public override List<Bar> DoAlgorithm(List<Krystal> krystals, List<Palette> palettes)
		{
			///*********************************************/
			List<Trk> crashATrks = GetElevenCrashTrks(0, crashAWagons, 0); // angular position in range [0..10]
			List<Trk> crashBTrks = GetElevenCrashTrks(1, crashBWagons, 3); // angular position in range [0..10]	
			List<Trk> crashCTrks = GetElevenCrashTrks(2, crashCWagons, 7); // angular position in range [0..10]

			crashCTrks = ReverseIndividualTrks(crashCTrks);

			SetBeamEnds(crashATrks);
			SetBeamEnds(crashBTrks);
			SetBeamEnds(crashCTrks);

			List<IUniqueDef> firstATrkUIDs = GetFirstTrkUIDs(crashATrks); // these IUniqueDefs track the msPosition of the eleven A trks
			List<IUniqueDef> firstBTrkUIDs = GetFirstTrkUIDs(crashBTrks); // these IUniqueDefs track the msPosition of the eleven B trks
			List<IUniqueDef> firstCTrkUIDs = GetFirstTrkUIDs(crashCTrks); // these IUniqueDefs track the msPosition of the eleven C trks

			Trk crashATrk = Concat(crashATrks);
			Trk crashBTrk = Concat(crashBTrks);
			Trk crashCTrk = Concat(crashCTrks);
			int msDuration = crashBTrk.MsDuration;
			crashATrk.MsDuration = msDuration;
			crashCTrk.MsDuration = msDuration;
			///*******************************************/
			///The following wagonTrks will be played in some way (fast) at various points in the piece...  
			List<Trk> aWagonTrkList = GetWagonTrks(3, crashAWagons);
			List<Trk> bWagonTrkList = GetWagonTrks(4, crashBWagons);
			List<Trk> cWagonTrkList = GetWagonTrks(5, crashCWagons);

			Trk aWagonsTrk = SetWagonsTrk(aWagonTrkList, firstATrkUIDs, msDuration);
			Trk bWagonsTrk = SetWagonsTrk(bWagonTrkList, firstBTrkUIDs, msDuration);
			Trk cWagonsTrk = SetWagonsTrk(cWagonTrkList, firstCTrkUIDs, msDuration);

			///*******************************************/

			List<Trk> trks = new List<Trk>() { crashATrk, crashBTrk, crashCTrk, aWagonsTrk, bWagonsTrk, cWagonsTrk };

			Seq mainSeq = new Seq(0, trks, MidiChannelPerOutputVoice);

			List<InputVoiceDef> inputVoiceDefs = new List<InputVoiceDef>();

			//List<int> endBarlinePositions = GetBalancedBarlineMsPositions(trks, null, NumberOfBars);

			List<int> endBarlinePositions = GetEndBarlineMsPositions(firstATrkUIDs, firstBTrkUIDs, firstCTrkUIDs, msDuration);

			Debug.Assert(NumberOfBars == endBarlinePositions.Count); // change NumberOfBars to match endBarlinePositions.Count! 

			List<List<SortedDictionary<int, string>>> clefChangesPerBar = GetClefChangesPerBar(endBarlinePositions.Count, mainSeq.Trks.Count);

			List<Bar> bars = GetBars(mainSeq, inputVoiceDefs, endBarlinePositions, clefChangesPerBar, null);

			SetPatch0InTheFirstChordInEachVoice(bars[0]);

			return bars;
		}

		private void SetBeamEnds(List<Trk> crashTrks)
		{
			foreach(Trk crashTrk in crashTrks)
			{
				if(crashTrk.UniqueDefs[crashTrk.UniqueDefs.Count - 1] is MidiChordDef lastMCD)
				{
					lastMCD.BeamContinues = false;
				}
			}
		}

		private List<Trk> ReverseIndividualTrks(List<Trk> crashTrks)
		{
			List<Trk> reversedTrks = new List<Trk>();
			int midiChannel = crashTrks[0].MidiChannel;

			foreach(Trk trk in crashTrks)
			{
				Trk newTrk = new Trk(midiChannel);
				for(int i = trk.UniqueDefs.Count - 1; i >= 0; --i)
				{
					newTrk.Add((IUniqueDef)trk.UniqueDefs[i].Clone());
				}
				reversedTrks.Add(newTrk);
			}

			return reversedTrks;
		}

		private List<IUniqueDef> GetFirstTrkUIDs(List<Trk> crashATrks)
		{
			List<IUniqueDef> firstTrkUIDs = new List<IUniqueDef>();
			foreach(Trk trk in crashATrks)
			{
				firstTrkUIDs.Add(trk.UniqueDefs[0]);
			}
			return firstTrkUIDs;
		}

		private Trk SetWagonsTrk(List<Trk> wagonTrkList, List<IUniqueDef> firstCrashTrkIUDs, int msDuration)
		{
			Trk trk = new Trk(wagonTrkList[0].MidiChannel);
			List<IUniqueDef> firstWagonTrkIUDs = new List<IUniqueDef>();

			List<int> trkPanPositions = GetWagonTrkPanPositions(wagonTrkList, firstCrashTrkIUDs);

			for(int i = wagonTrkList.Count - 1; i >=0; --i)
			{
				Trk wTrk = wagonTrkList[i];
				int panPos = trkPanPositions[i];
				foreach(IUniqueDef wTrkIud in wTrk.UniqueDefs)
				{
					if(wTrkIud is MidiChordDef mcd)
					{
						mcd.PanMsbs = new List<byte>() { (byte)panPos };
					}
				}
				IUniqueDef iud = wTrk[0];
				for(int j= wTrk.Count - 1; j >= 0; --j)
				{
					trk.Insert(0, wTrk[j]);
				}
				firstWagonTrkIUDs.Insert(0, iud);
			}

			List<int> trkMsPositions = GetWagonTrkMsPositions(wagonTrkList, firstCrashTrkIUDs);

			for(int i = 0; i < firstWagonTrkIUDs.Count; ++i)
			{
				IUniqueDef iud = firstWagonTrkIUDs[i];
				int iudMsPos = iud.MsPositionReFirstUD;
				int trkMsPos = trkMsPositions[i];
				int insertRestMsDuration = trkMsPos - iudMsPos;
				if(insertRestMsDuration > 0)
				{
					int index = trk.UniqueDefs.IndexOf(iud);
					MidiRestDef imrd = new MidiRestDef(0, insertRestMsDuration);
					trk.Insert(index, imrd);
				}
			}

			int finalRestDuration = msDuration - trk.MsDuration;
			MidiRestDef fmrd = new MidiRestDef(0, finalRestDuration);
			trk.Add(fmrd);

			return trk;
		}

		private List<int> GetWagonTrkMsPositions(List<Trk> wagonTrkList, List<IUniqueDef> firstCrashTrkIUDs)
		{
			List<int> rval = new List<int>();

			for(int i = 0; i < wagonTrkList.Count; ++i)
			{
				int crashTrkIndex = ((i * 2) + 1);
				Debug.Assert(crashTrkIndex < firstCrashTrkIUDs.Count);
				rval.Add(firstCrashTrkIUDs[crashTrkIndex].MsPositionReFirstUD);
			}
			return rval;
		}


		private List<int> GetWagonTrkPanPositions(List<Trk> wagonTrkList, List<IUniqueDef> firstCrashTrkIUDs)
		{
			List<int> rval = new List<int>();

			for(int i = 0; i < wagonTrkList.Count; ++i)
			{
				int crashTrkIndex = ((i * 2) + 1);
				Debug.Assert(crashTrkIndex < firstCrashTrkIUDs.Count);
				MidiChordDef mcd = firstCrashTrkIUDs[crashTrkIndex] as MidiChordDef;
				rval.Add(mcd.PanMsbs[0]);
			}
			return rval;
		}

		private List<int> GetEndBarlineMsPositions(List<IUniqueDef> firstATrkUIDs, List<IUniqueDef> firstBTrkUIDs, List<IUniqueDef> firstCTrkUIDs, int msDuration)
		{
			List<int> endBarlinePositions = new List<int>();

			foreach(IUniqueDef iud in firstATrkUIDs)
			{
				if(iud.MsPositionReFirstUD > 0)
				{
					endBarlinePositions.Add(iud.MsPositionReFirstUD);
				}
			}
			foreach(IUniqueDef iud in firstBTrkUIDs)
			{
				if(iud.MsPositionReFirstUD > 0)
				{
					endBarlinePositions.Add(iud.MsPositionReFirstUD);
				}
			}
			foreach(IUniqueDef iud in firstCTrkUIDs)
			{
				if(iud.MsPositionReFirstUD > 0)
				{
					endBarlinePositions.Add(iud.MsPositionReFirstUD);
				}
			}

			endBarlinePositions.Add(msDuration);

			endBarlinePositions.Sort();

			// remove duplicates and barlines that are closer than 1000ms to the previous one
			for(int i = endBarlinePositions.Count - 1; i > 0; --i)
			{
				if(endBarlinePositions[i] - endBarlinePositions[i - 1] <= 1000)
				{
					endBarlinePositions.RemoveAt(i);
				}
			}
			return endBarlinePositions;
		}

		private Trk Concat(List<Trk> crashTrks)
		{
			Debug.Assert(crashTrks.Count == 11);

			Trk crashTrk = new Trk(crashTrks[0].MidiChannel);

			foreach(Trk trk in crashTrks)
			{
				crashTrk.AddRange(trk);
			}
			return crashTrk;
		}

		/// <summary>
		/// Adds cloned IUniqueDefs from crashTrks to the returned Trk.
		/// Does not change crashTrks.
		/// <returns></returns>
		private Trk Intersperse(IReadOnlyList<Trk> crashTrks, IReadOnlyList<IReadOnlyList<byte>> crashWagons)
		{
			int nWagons = crashWagons.Count - 1;
			Trk trk = new Trk(crashTrks[0].MidiChannel);
			IReadOnlyList<byte> interspersedValues = crashWagons[nWagons]; // (!)
			foreach(byte value in interspersedValues)
			{
				int wagonIndex = -1;
				int valueIndex = -1;
				for(int i = 0; i < nWagons; ++i)
				{
					IReadOnlyList<byte> crashWagon = crashWagons[i];
					for(int j = 0; j < crashWagon.Count; ++j)
					{
						byte val = crashWagon[j];
						if(val == value)
						{
							valueIndex = j;
							wagonIndex = i;
							break;
						}
					}
					if(wagonIndex != -1)
					{
						break;
					}
				}
				if(wagonIndex == -1)
				{
					throw new ApplicationException("Couldn't find the wagon!");
				}
				IUniqueDef iud = (IUniqueDef) crashTrks[wagonIndex].UniqueDefs[valueIndex].Clone();
				trk.Add(iud);
			}

			return trk;
		}

		/// <summary>
		/// See summary and example code on abstract definition in CompositionAlogorithm.cs
		/// </summary>
		protected override List<List<SortedDictionary<int, string>>> GetClefChangesPerBar(int nBars, int nVoicesPerBar)
		{
			return null;			
		}

		private List<IUniqueDef> GetMidiChordDefs(List<byte> pitches, List<byte> velocities, List<int> msDurations)
		{
			Debug.Assert(pitches.Count == velocities.Count);
			Debug.Assert(msDurations.Count == velocities.Count);

			List<IUniqueDef> defs = new List<IUniqueDef>();
			int msPosition = 0;
			for(int i = 0; i < pitches.Count; ++i)
			{
				List<byte> pitchesArg = new List<byte>() { pitches[i] };
				List<byte> velocitiesArg = new List<byte>() { velocities[i] };
				int msDuration = msDurations[i];
				MidiChordDef midiChordDef = new MidiChordDef(pitchesArg, velocitiesArg, msDuration, true)
				{
					MsPositionReFirstUD = msPosition
				};
				defs.Add(midiChordDef);
				msPosition += msDuration;
			}
			return defs;
		}

		private Trk GetWagonTrk(int midiChannel, List<byte> pitches, List<byte> velocities, List<int> durations)
		{
			Trk trk = new Trk(midiChannel);
			List<IUniqueDef> midiChordDefs = GetMidiChordDefs(pitches, velocities, durations);
			trk.UniqueDefs.AddRange(midiChordDefs);

			return trk;
		}

		/// <summary>
		/// Returns a sequence of eleven Trks, each having nKeyboardPitches notes. The first and last Trks are at angularPosition.
		/// Assumes that the observer is at the centre of the circle around which the crash moves with constant velocity,
		/// so global durations and velocity are the same for each Trk.
		/// As the crash moves round the circle, the pan value and relative velocity of the wagons changes. 
		/// </summary>
		/// <param name="midiChannel"></param>
		/// <param name="crashWagons"></param>
		/// <param name="initialAngularPosition"></param>
		/// <returns></returns>
		private List<Trk> GetElevenCrashTrks(int midiChannel, IReadOnlyList<IReadOnlyList<byte>> crashWagons, int initialAngularPosition)
		{
			List<Trk> crashTrks = new List<Trk>();

			for(int i = 0; i < 11; ++i)
			{
				int angularPosition = (initialAngularPosition + i) % 10; // in range [0..10]
				List<Trk> wagonTrks = new List<Trk>();

				for(int wagonIndex = 0; wagonIndex < crashWagons.Count - 1; ++wagonIndex)
				{
					IReadOnlyList<byte> wagonValues = crashWagons[wagonIndex];
					List<byte> velocities = GetBasicVelocities(wagonIndex, angularPosition, wagonValues.Count);
					List<int> pitchDurations = GetBasicPitchDurations(wagonValues);
					List<byte> midiPitches = GetBasicMidiPitches(wagonValues);

					Trk wagonTrk = GetWagonTrk(midiChannel, midiPitches, velocities, pitchDurations);

					wagonTrks.Add(wagonTrk);
				}

				Trk crashTrk = Intersperse(wagonTrks, crashWagons);

				if(i < 10) // the final crashTrk does not rotate
				{
					crashTrk = SetPan(crashTrk, angularPosition);
				}
				crashTrk = SetVelocities(crashTrk, angularPosition, minFactor:0.3);
				crashTrk = SetDuration(crashTrk, angularPosition, maxFactor: 3);

				Debug.Assert(crashTrk.Count == nKeyboardPitches);

				crashTrks.Add(crashTrk);
			}
			return crashTrks;
		}

		private List<Trk> GetWagonTrks(int midiChannel, IReadOnlyList<IReadOnlyList<byte>> crashWagons)
		{
			List<Trk> wagonTrks = new List<Trk>();

			for(int wagonIndex = 0; wagonIndex < crashWagons.Count - 1; ++wagonIndex)
			{
				IReadOnlyList<byte> wagonValues = crashWagons[wagonIndex];
				List<byte> velocities = GetBasicVelocities(wagonIndex, 0, wagonValues.Count); // angular position 0
				List<int> pitchDurations = GetBasicPitchDurations(wagonValues);
				List<byte> midiPitches = GetBasicMidiPitches(wagonValues);

				Trk wagonTrk = GetWagonTrk(midiChannel, midiPitches, velocities, pitchDurations);

				wagonTrks.Add(wagonTrk);
			}
			return wagonTrks;
		}

		private Trk SetPan(Trk crashTrk, int angularPosition)
		{
			Debug.Assert(0 <= angularPosition && angularPosition < 11);
			const int left = 0;
			const int halfLeft = 24; // 64 * (3/8);
			const int centre = 64;
			const int halfRight = 79; // 127 * (5/8);
			const int right = 127;

			int startPanValue = 0;
			int endPanValue = 0;
			switch(angularPosition)
			{
				case 0:
					startPanValue = centre;
					endPanValue = halfLeft;
					break;
				case 1:
					startPanValue = halfLeft;
					endPanValue = left;
					break;
				case 2:
					startPanValue = left;
					endPanValue = left;
					break;
				case 3:
					startPanValue = left;
					endPanValue = halfLeft;
					break;
				case 4:
					startPanValue = halfLeft;
					endPanValue = centre;
					break;
				case 5:
					startPanValue = centre;
					endPanValue = halfRight;
					break;
				case 6:
					startPanValue = halfRight;
					endPanValue = right;
					break;
				case 7:
					startPanValue = right;
					endPanValue = right;
					break;
				case 8:
					startPanValue = right;
					endPanValue = halfRight;
					break;
				case 9:
					startPanValue = halfRight;
					endPanValue = centre;
					break;
				case 10:
					startPanValue = centre;
					endPanValue = centre;
					break;
			}

			crashTrk.SetPanGliss(0, crashTrk.Count, startPanValue, endPanValue);

			return crashTrk;
		}

		private Trk SetDuration(Trk crashTrk, int angularPosition, double maxFactor)
		{
			Debug.Assert(0 <= angularPosition && angularPosition < 11);

			double factor = Math.Pow(maxFactor, 0.2);
			double factor0 = 1;
			double factor1 = factor;
			double factor2 = factor1 * factor;
			double factor3 = factor2 * factor;
			double factor4 = factor3 * factor;
			double factor5 = factor4 * factor;

			double warpFactor = 0;

			switch(angularPosition)
			{
				case 0:
				case 10:
					warpFactor = factor0;
					break;
				case 1:
				case 9:
					warpFactor = factor1;
					break;
				case 2:
				case 8:
					warpFactor = factor2;
					break;
				case 3:
				case 7:
					warpFactor = factor3;
					break;
				case 4:
				case 6:
					warpFactor = factor4;
					break;
				case 5:
					warpFactor = factor5;
					break;
			}

			crashTrk.MsDuration = (int)(crashTrk.MsDuration * warpFactor);

			return crashTrk;
		}

		private Trk SetVelocities(Trk crashTrk, int angularPosition, double minFactor)
		{
			Debug.Assert(0 <= angularPosition && angularPosition < 11);
			Debug.Assert(0 < minFactor && minFactor < 1);

			double factor = Math.Pow(minFactor, 0.2);
			double factor0 = 1;
			double factor1 = factor;
			double factor2 = factor1 * factor;
			double factor3 = factor2 * factor;
			double factor4 = factor3 * factor;
			double factor5 = factor4 * factor;

			double warpFactor = 0;

			switch(angularPosition)
			{
				case 0:
				case 10:
					warpFactor = factor0;
					break;
				case 1:
				case 9:
					warpFactor = factor1;
					break;
				case 2:
				case 8:
					warpFactor = factor2;
					break;
				case 3:
				case 7:
					warpFactor = factor3;
					break;
				case 4:
				case 6:
					warpFactor = factor4;
					break;
				case 5:
					warpFactor = factor5;
					break;
			}

			foreach(IUniqueDef iud in crashTrk.UniqueDefs)
			{
				if(iud is MidiChordDef mcd)
				{
					BasicMidiChordDef bmcd = mcd.BasicMidiChordDefs[0];
					byte originalVelocity = bmcd.Velocities[0];
					byte newVelocity = (byte)Math.Round(originalVelocity * warpFactor);
					bmcd.AdjustVelocities(originalVelocity, newVelocity);
					mcd.NotatedMidiVelocities[0] = bmcd.Velocities[0];
				}
			}

			return crashTrk;
		}

		private List<byte> GetBasicVelocities(int wagonIndex, int angularPos, int wagonValuesCount)
		{
			List<int> basicVelocityIndexPerAngPos = basicVelocityIndexPerWagonPerAngPos[wagonIndex];
			int velocityIndex = basicVelocityIndexPerAngPos[angularPos];
			byte velocity = basicVelocities[velocityIndex];
			List<byte> velocities = new List<byte>();
			for(int i=0; i < wagonValuesCount; ++i)
			{
				velocities.Add(velocity);
			}
			return velocities;
		}

		private List<int> GetBasicPitchDurations(IReadOnlyList<byte> wagonValues)
		{
			// nKeyboardPitches durations (longLow to shortHigh)
			IReadOnlyList<int> basicDurations = BasicDurations();

			Debug.Assert(wagonValues.Count <= nKeyboardPitches && basicDurations.Count == nKeyboardPitches);
			List<int> pitchDurations = new List<int>();
			foreach(byte pitch in wagonValues)
			{
				Debug.Assert(pitch >= 1 && pitch <= nKeyboardPitches);
				pitchDurations.Add(basicDurations[pitch - 1]);
			}
			return pitchDurations;
		}

		/// <summary>
		/// returns the real (transposed) MIDI pitch values.
		/// </summary>
		private List<byte> GetBasicMidiPitches(IReadOnlyList<byte> wagonValues)
		{
			byte transposition = (byte)((127 - nKeyboardPitches) / 2); // 21 -- puts the range in middle of the MIDI range
			List<byte> midiPitches = new List<byte>();
			foreach(byte value in wagonValues)
			{
				midiPitches.Add((byte)(value + transposition));
			}
			return midiPitches;
		}

		/// <summary>
		/// returns a list of ms durations by pitch index (in relation nKeyboardPitchesth root of 2).
		/// The top (=last) value is nearly half the bottom (=first) value;
		/// </summary>
		private IReadOnlyList<int> BasicDurations()
		{
			const int longestMsDuration = 350; 
			double factor = Math.Pow(2.0, ((double)1 / nKeyboardPitches));

			List<double> dDurations = new List<double>();
			List<int> durations = new List<int>();
			dDurations.Add(longestMsDuration);
			durations.Add(longestMsDuration);
			for(int i = 1; i < nKeyboardPitches; ++i)
			{
				double dDuration = dDurations[dDurations.Count - 1] / factor;
				dDurations.Add(dDuration);
				durations.Add((int)dDuration);
			}
			return durations as IReadOnlyList<int>;
		}

		/// <summary>
		/// The patch only needs to be set in the first chord in each voice,
		/// since it will be set by shunting if the Assistant Performer starts later.
		/// </summary>
		private void SetPatch0InTheFirstChordInEachVoice(Bar bar1)
		{
			MidiChordDef midiChordDef = null;
			foreach(VoiceDef voiceDef in bar1.VoiceDefs)
			{
				foreach(IUniqueDef iUniqueDef in voiceDef.UniqueDefs)
				{
					midiChordDef = iUniqueDef as MidiChordDef;
					if(midiChordDef != null)
					{
						midiChordDef.Patch = 0;
                        break;
					}
				}
			}
		}
	}
}
