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
		private void GetTrksAndBarlines(out List<Trk> trks, out List<int> barlineMsPositions, out List<List<int>> targetChords)
		{
			Krystal k4 = _krystals[0];
			Krystal k3 = _krystals[1];
			Krystal k2 = _krystals[2];
			Krystal k1 = _krystals[3];

			List<List<int>> k3ValuesPerBar = k3.GetValues(k3.Level);
			List<List<int>> k2ValuesPerBar = k2.GetValues(k2.Level + 1);

			List<List<List<int>>> trk0AbsPitchesPerStrandPerBar = GetAbsPitchesPerStrandPerBar(k4);

			barlineMsPositions = new List<int>();
			targetChords = new List<List<int>>();
			Trk trk0 = new Trk(0);
			//for(int i = 0; i < targetChords.Count; i++)
			//{
			//	var startPitches = targetChords[i];
			//	var msDuration = availableBarDurations[barIndices[i]];

			//	List<byte> startVelocities = new List<byte>();
			//	foreach(var pitch in startPitches)
			//	{
			//		startVelocities.Add((byte)100);
			//	}
			//	trk0.Add(new MidiChordDef(M.MidiList(startPitches), startVelocities, msDuration, true));

			//	barlineMsPositions.Add(trk0.MsDuration);
			//}

			/**********************************************/

			trk0.Transpose(12);

			Trk trk1 = (Trk)trk0.Clone();
			trk1.MidiChannel = 1;
			trk1.Transpose(-12);

			Trk trk2 = (Trk)trk1.Clone();
			trk2.MidiChannel = 2;
			trk2.Transpose(-12);

			Trk trk3 = (Trk)trk2.Clone();
			trk3.MidiChannel = 3;
			trk3.Transpose(-12);

			trks = new List<Trk>() { trk0, trk1, trk2, trk3 };
		}

		private List<List<List<int>>> GetAbsPitchesPerStrandPerBar(Krystal k4)
		{
			List<List<int>> k4ValuesPerStrand = k4.GetValues(k4.Level);
			List<List<int>> k4ValuesPerBar = k4.GetValues(k4.Level - 1);

			List<List<List<int>>> k4StrandsValuesPerBar = GetValuesPerStrandPerBar(k4ValuesPerStrand, k4ValuesPerBar);
			List<List<List<int>>> absPitchesPerStrandPerBar = new List<List<List<int>>>();

			IReadOnlyList<List<int>> _pitchesPerBars = absPitchesPerStrandValuePerBar;
			Debug.Assert(_pitchesPerBars.Count == k4StrandsValuesPerBar.Count);

			for(int i = 0; i < _pitchesPerBars.Count; ++i)
			{
				List<List<int>> barStrandsValues = k4StrandsValuesPerBar[i];
				List<int> barPitches = _pitchesPerBars[i];
				List<List<int>> barPitchesPerStrand = new List<List<int>>();
				foreach(List<int> strandValues in barStrandsValues)
				{
					List<int> absPitchValues = new List<int>();
					foreach(int value in strandValues)
					{
						absPitchValues.Add(barPitches[value - 1]);
					}
					barPitchesPerStrand.Add(absPitchValues);
				}
				absPitchesPerStrandPerBar.Add(barPitchesPerStrand);
			}
			return absPitchesPerStrandPerBar;
		}

		private List<List<List<int>>> GetValuesPerStrandPerBar(List<List<int>> k4ValuesPerStrand, List<List<int>> k4ValuesPerBar)
		{
			List<List<List<int>>> rval = new List<List<List<int>>>();
			int strandIndex = 0;
			for(int barIndex= 0; barIndex < k4ValuesPerBar.Count; ++barIndex)
			{
				int valuesInBar = k4ValuesPerBar[barIndex].Count;
				int nValues = 0;
				List<List<int>> strandsPerBar = new List<List<int>>();

				while(nValues < valuesInBar)
				{
					nValues += k4ValuesPerStrand[strandIndex].Count;
					strandsPerBar.Add(k4ValuesPerStrand[strandIndex]);
					strandIndex++;
				}
				rval.Add(strandsPerBar);
			}
			return rval;
		}

		private static List<int> availableBarDurations = new List<int>()
			{
				2000, 2151, 2331, 2548, 2811, 3113, 3533, 4033, 4670, 5490, 6565, 8000
			};

		//See notebook 25th Sept.2019
		// These values describe modi that evolve by step, rather in the way that Tristan's harmonies evolve... 
		private static IReadOnlyList<List<int>> absPitchesPerStrandValuePerBar = new List<List<int>>()
			{
				new List<int>() {0,2,3,5,8,9,11}, // bar 1
				new List<int>() {0,2,3,5,7,8,11}, // bar 2
				new List<int>() {1,3,4,4,7,8,11}, // bar 3
				new List<int>() {1,3,4,4,6,7,11}, // bar 4
				new List<int>() {2,4,5,4,6,7,11}, // bar 5
				new List<int>() {2,4,5,3,5,6,11}, // bar 6
				new List<int>() {3,5,6,3,5,6,11}, // bar 7
				new List<int>() {3,5,6,2,4,5,11}, // bar 8
				new List<int>() {4,6,7,2,4,5,11}, // bar 9
				new List<int>() {4,6,7,1,3,4,11}, // bar 10
				new List<int>() {5,7,8,1,3,4,11}, // bar 11
				new List<int>() {5,7,8,0,2,3,11}, // bar 12
				new List<int>() {6,8,9,0,2,3,11}, // bar 13
				new List<int>() {6,8,9,1,2,3,10}, // bar 14
				new List<int>() {7,7,9,2,2,4,10}, // bar 15
				new List<int>() {7,7,8,2,2,4,9}, // bar 16
				new List<int>() {8,7,8,3,1,4,8}, // bar 17
				new List<int>() {8,6,8,4,1,5,7}, // bar 18
				new List<int>() {9,6,8,5,1,5,7}, // bar 19
				new List<int>() {9,6,7,6,1,5,6}, // bar 20
				new List<int>() {10,5,7,7,1,5,5}, // bar 21
				new List<int>() {10,5,7,8,1,6,4}, // bar 22
				new List<int>() {11,5,7,8,0,6,3}, // bar 23
				new List<int>() {11,5,6,9,0,6,3}, // bar 24
				new List<int>() {10,4,6,10,0,7,2}, // bar 25
				new List<int>() {10,4,6,11,0,7,1}, // bar 26
				new List<int>() {11,5,5,11,1,7,1}, // bar 27
				new List<int>() {11,6,4,10,2,8,0}, // bar 28
			};

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
