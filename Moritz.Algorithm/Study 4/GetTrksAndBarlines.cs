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
		private void GetTrksAndBarlines(GamutVector modeVector, out List<Trk> trks, out List<int> barlineMsPositions)
		{
			const int barChordDuration = 1000; //ms
			trks = GetTrks(modeVector.PitchVectors, barChordDuration);

			barlineMsPositions = new List<int>();
			foreach(var uid in trks[0].UniqueDefs)
			{
				barlineMsPositions.Add(uid.MsPositionReFirstUD + barChordDuration);
			}
		}

		/// <summary>
		/// Creates a list of 8 Trks, each of which contains 55 (=each pitchList.Count) MidiChordDefs.
		/// Each MidiChorddef has a duration of 1000ms. There is going to be 1 MidiChordDef per Bar.
		/// The last Trk is a tutti trk: each MidiChordDef contains all the pitches in the above Trks once.
		/// If a pitch would occur more than once in a tutti chord, then it occurs once with the largest
		/// velocity of any duplicated pitch.
		/// </summary>
		/// <param name="pitchVectors"></param>
		/// <returns></returns>
		private List<Trk> GetTrks(IReadOnlyList<PitchVector> pitchVectors, int barChordDuration)
		{
			List<Trk> trks = new List<Trk>();
			int channel = 0;
			foreach(var pitchVector in pitchVectors)
			{
				Trk trk = new Trk(channel++);
				IReadOnlyList<PitchWeight> pitchWeights = pitchVector.PitchWeights;
				foreach(var def in pitchVector.PitchWeights)
				{
					List<UInt7> pitches = new List<UInt7>() { def.Pitch };
					List<UInt7> velocities = new List<UInt7>() { def.Weight };
					IUniqueDef midiChordDef = new MidiChordDef(pitches, velocities, barChordDuration, true);
					trk.Add(midiChordDef);
				}

				trks.Add(trk);
			}
			// now create the bottom, tutti Trk
			int nChords = pitchVectors[0].PitchWeights.Count;
			List<IUniqueDef> tuttiChords = new List<IUniqueDef>();
			for(int i = 0; i < nChords; ++i)
			{
				List<UInt7> pitches = new List<UInt7>();
				List<UInt7> velocities = new List<UInt7>();

				for(int j= 0; j < pitchVectors.Count; j++)
				{
					PitchVector pitchVector = pitchVectors[j];
					var def = pitchVector.PitchWeights[i];
					UInt7 pitch = (UInt7)def.Pitch;
					UInt7 velocity = (UInt7)def.Weight;
					if(!pitches.Contains(pitch))
					{
						int insertIndex = pitches.Count;
						for(int k = 0; k < pitches.Count; ++k)
						{
							if(pitches[k] > pitch)
							{
								insertIndex = k;
								break;
							}
						}
						pitches.Insert(insertIndex, pitch);
						velocities.Insert(insertIndex, velocity);
					}
					else
					{
						int pitchIndex = pitches.IndexOf(pitch);
						UInt7 existingVelocity = velocities[pitchIndex];
						velocities[pitchIndex] = (existingVelocity > velocity) ? existingVelocity : velocity;
					}
				}
				tuttiChords.Add(new MidiChordDef(pitches, velocities, barChordDuration, true));
			}
			trks.Add(new Trk(channel, 0, tuttiChords));

			return trks;
		}

		#region commented out
		///// <summary>
		///// Transposes each absPitch to a unique position (octaves are allowed), such that
		///// 1) intervals between pitches in successive chords (=strands) are minimized and
		///// 2) chords have minimum outer range.
		///// (The "chords" are going to be presented as sequences of single pitches later.)
		///// </summary>
		///// <param name="trk0AbsPitchesPerStrandPerBar"></param>
		///// <returns></returns>
		//private List<List<List<int>>> GetStrandChordProgressionPerBar(List<List<List<int>>> trk0AbsPitchesPerStrandPerBar)
		//{
		//	var rval = new List<List<List<int>>>();

		//	return rval;
		//}

		//private List<List<List<int>>> GetAbsPitchesPerStrandPerBar(Krystal k4)
		//{
		//	List<List<int>> k4ValuesPerStrand = k4.GetValues(k4.Level);
		//	List<List<int>> k4ValuesPerBar = k4.GetValues(k4.Level - 1);

		//	List<List<List<int>>> k4StrandsValuesPerBar = GetValuesPerStrandPerBar(k4ValuesPerStrand, k4ValuesPerBar);
		//	List<List<List<int>>> absPitchesPerStrandPerBar = new List<List<List<int>>>();

		//	IReadOnlyList<List<int>> _pitchesPerBars = absPitchesPerStrandValuePerBar;
		//	Debug.Assert(_pitchesPerBars.Count == k4StrandsValuesPerBar.Count);

		//	for(int i = 0; i < _pitchesPerBars.Count; ++i)
		//	{
		//		List<List<int>> barStrandsValues = k4StrandsValuesPerBar[i];
		//		List<int> barPitches = _pitchesPerBars[i];
		//		List<List<int>> barPitchesPerStrand = new List<List<int>>();
		//		foreach(List<int> strandValues in barStrandsValues)
		//		{
		//			List<int> absPitchValues = new List<int>();
		//			foreach(int value in strandValues)
		//			{
		//				absPitchValues.Add(barPitches[value - 1]);
		//			}
		//			barPitchesPerStrand.Add(absPitchValues);
		//		}
		//		absPitchesPerStrandPerBar.Add(barPitchesPerStrand);
		//	}
		//	return absPitchesPerStrandPerBar;
		//}

		//private List<List<List<int>>> GetValuesPerStrandPerBar(List<List<int>> k4ValuesPerStrand, List<List<int>> k4ValuesPerBar)
		//{
		//	List<List<List<int>>> rval = new List<List<List<int>>>();
		//	int strandIndex = 0;
		//	for(int barIndex= 0; barIndex < k4ValuesPerBar.Count; ++barIndex)
		//	{
		//		int valuesInBar = k4ValuesPerBar[barIndex].Count;
		//		int nValues = 0;
		//		List<List<int>> strandsPerBar = new List<List<int>>();

		//		while(nValues < valuesInBar)
		//		{
		//			nValues += k4ValuesPerStrand[strandIndex].Count;
		//			strandsPerBar.Add(k4ValuesPerStrand[strandIndex]);
		//			strandIndex++;
		//		}
		//		rval.Add(strandsPerBar);
		//	}
		//	return rval;
		//}

		//private static List<int> availableBarDurations = new List<int>()
		//	{
		//		2000, 2151, 2331, 2548, 2811, 3113, 3533, 4033, 4670, 5490, 6565, 8000
		//	};

		//// See notebook 1. October 2019
		//// These values describe modi that evolve by step, rather in the way that Tristan's harmonies evolve...
		//// Interesting that the concepts 'chord' and 'mode' are getting confused here. There's something fundamental about that...
		//// Has to do with smearing time... (Think carefully about this.)
		//private static IReadOnlyList<List<int>> absPitchesPerStrandValuePerBar = new List<List<int>>()
		//	{
		//		new List<int>() {0, 2, 3, 5, 8, 9,11}, // bar 1

		//		new List<int>() {3, 9, 0, 6, 7,10, 4},  // bar 2
		//		new List<int>() {2, 9, 2, 4, 7,10, 5},  // bar 3

		//		new List<int>() {1,10, 4, 2, 7,11, 5}, // bar 4
		//		new List<int>() {11,10,4, 1, 6, 9, 6}, // bar 5
		//		new List<int>() {9, 9, 5, 1, 4, 8, 9}, // bar 6

		//		new List<int>() {8, 9, 5, 0, 2, 6,11},  // bar 7
		//		new List<int>() {8, 8, 5, 1, 2, 4, 0}, // bar 8
		//		new List<int>() {8, 8, 6, 2, 1, 3, 1}, // bar 9
		//		new List<int>() {9, 7, 6, 3, 1, 2, 2}, // bar 10

		//		new List<int>() {9, 6, 7, 4, 1, 0, 3}, // bar 11
		//		new List<int>() {9, 5, 7, 5, 1, 0, 4}, // bar 12
		//		new List<int>() {9, 5, 6, 5, 0, 0, 5}, // bar 13
		//		new List<int>() {10,4, 5, 6, 0, 1, 6},  // bar 14
		//		new List<int>() {10,3, 5, 6,11, 1, 7}, // bar 15

		//		new List<int>() {10,2, 4, 7,11, 1, 8}, // bar 16
		//		new List<int>() {11,3, 4, 7,11, 1, 8}, // bar 17
		//		new List<int>() {0, 4, 5, 6, 0, 0, 8}, // bar 18
		//		new List<int>() {0, 5, 5, 6, 0, 0, 8}, // bar 19
		//		new List<int>() {1, 6, 5, 6, 1, 0, 9}, // bar 20
		//		new List<int>() {2, 7, 6, 5, 1,11, 9},  // bar 21

		//		new List<int>() {3, 8, 6, 5, 2,11, 9}, // bar 22
		//		new List<int>() {3, 7, 6, 5, 3,11, 9}, // bar 23
		//		new List<int>() {2, 6, 5, 5, 4,11,10}, // bar 24
		//		new List<int>() {2, 6, 5, 5, 4,10,10}, // bar 25
		//		new List<int>() {1, 5, 4, 5, 5,10,10}, // bar 26
		//		new List<int>() {1, 4, 4, 5, 6,10,10}, // bar 27
		//		new List<int>() {0, 3, 3, 5, 7, 9,11}, // bar 28
		//	};
		#endregion commented out

		#region commented out
		//private List<List<int>> GetChords1(List<int> root)
		//{
		//	List<int> nums = new List<int>() { 1, 2, 3, 4, 5 };
		//	List<List<int>> allPermutations = new List<List<int>>();
		//	//GetAllPermutations(nums.Count, nums, allPermutations);

		//	List<List<int>> sortedPermutations = GetSortedLists(allPermutations);
		//	List<List<int>> sortedAbsChords = GetSortedAbsChords(root, sortedPermutations);
		//	List<List<int>> sortedRelChords = GetSortedRelChords(sortedAbsChords);

		//	return sortedRelChords;
		//}

		///// <summary>
		///// Shifts pitches up as necessary (by octaves), so that each list is in ascending order.  
		///// </summary>
		///// <param name="sortedAbsChords"></param>
		///// <returns></returns>
		//private List<List<int>> GetSortedRelChords(List<List<int>> sortedAbsChords)
		//{
		//	List<List<int>> rval = new List<List<int>>();
		//	foreach(var chord in sortedAbsChords)
		//	{
		//		var rChord = new List<int>
		//		{
		//			chord[0]
		//		};
		//		if(chord.Count > 1)
		//		{
		//			for(int i = 1; i < chord.Count; i++)
		//			{
		//				int prevValue = rChord[i-1];
		//				int nextValue = chord[i];
		//				while(prevValue > nextValue )
		//				{
		//					nextValue += 12;
		//				}
		//				rChord.Add(nextValue);
		//			}
		//		}
		//		rval.Add(rChord);
		//	}
		//	return rval;
		//}

		///// <summary>
		///// Simply substitutes absolute pitches for the values in sortedPermutations
		///// </summary>
		///// <param name="root"></param>
		///// <param name="sortedPermutations"></param>
		///// <returns></returns>
		//private List<List<int>> GetSortedAbsChords(List<int> root, List<List<int>> sortedPermutations)
		//{
		//	List<List<int>> rval = new List<List<int>>();
		//	foreach(var list in sortedPermutations)
		//	{
		//		var newList = new List<int>();
		//		foreach(var number in list)
		//		{
		//			newList.Add(root[number - 1]);
		//		}
		//		rval.Add(newList);
		//	}
		//	return rval;
		//}

		//private List<List<int>> GetSortedLists(List<List<int>> allPermutations)
		//{
		//	int ValueOf(List<int> list)
		//	{
		//		int factor = 1;
		//		int value = 0;
		//		for(int i = list.Count - 1; i >= 0; i--)
		//		{
		//			value += (list[i] * factor);
		//			factor *= 10;
		//		}
		//		return value;
		//	}

		//	List<List<int>> rval = new List<List<int>>();
		//	Dictionary<int, int> valuesDict = new Dictionary<int, int>();
		//	for(int i = 0; i < allPermutations.Count; i++)
		//	{
		//		valuesDict.Add(i, ValueOf(allPermutations[i]));
		//	}

		//	while(valuesDict.Count > 0)
		//	{
		//		int currentValue = int.MaxValue;
		//		int currentKey = 0;

		//		foreach(var key in valuesDict.Keys)
		//		{
		//			int value = valuesDict[key];
		//			if(value < currentValue)
		//			{
		//				currentValue = value;
		//				currentKey = key;
		//			}
		//		}

		//		rval.Add(new List<int>(allPermutations[currentKey]));
		//		valuesDict.Remove(currentKey);
		//	}

		//	return rval;
		//}

		#endregion
	}
}
