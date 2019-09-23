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
			#region constants
			var availableBarDurations = new List<int>()
			{
				2000, 2151, 2331, 2548, 2811, 3113, 3533, 4033, 4670, 5490, 6565, 8000
			};
			List<int> chordsRoot = new List<int>() { 54, 57, 59, 63, 65 }; // f#, a, b, d#, f
			targetChords = GetChords1(chordsRoot);
			#endregion

			Krystal k4 = _krystals[0];
			Krystal k3 = _krystals[1];
			Krystal k2 = _krystals[2];
			Krystal k1 = _krystals[3];
			List<List<int>> k4ValuesPerStrand = k4.GetValues(k4.Level);
			List<List<int>> k4ValuesPerBar = k4.GetValues(k4.Level - 1);
			List<List<int>> k3ValuesPerBar = k3.GetValues(k3.Level);
			List<List<int>> k2ValuesPerBar = k2.GetValues(k2.Level + 1);

			List<List<List<int>>> k4ValuesPerStrandPerBar = GetValuesPerStrandPerBar(k4ValuesPerStrand, k4ValuesPerBar);

			Envelope centredEnvelope = _krystals[0].ToEnvelope(0, availableBarDurations.Count - 1); // values distributed around 11, gradually becoming more eccentric
			centredEnvelope.SetCount(targetChords.Count);
			List<int> barIndices = centredEnvelope.Original;

			barlineMsPositions = new List<int>();
			Trk trk0 = new Trk(0);
			for(int i = 0; i < targetChords.Count; i++)
			{
				var startPitches = targetChords[i];
				var msDuration = availableBarDurations[barIndices[i]];

				List<byte> startVelocities = new List<byte>();
				foreach(var pitch in startPitches)
				{
					startVelocities.Add((byte)100);
				}
				trk0.Add(new MidiChordDef(M.MidiList(startPitches), startVelocities, msDuration, true));

				barlineMsPositions.Add(trk0.MsDuration);
			}

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

		private List<List<int>> GetChords1(List<int> root)
		{
			List<int> nums = new List<int>() { 1, 2, 3, 4, 5 };
			List<List<int>> allPermutations = new List<List<int>>();
			//GetAllPermutations(nums.Count, nums, allPermutations);

			List<List<int>> sortedPermutations = GetSortedLists(allPermutations);
			List<List<int>> sortedAbsChords = GetSortedAbsChords(root, sortedPermutations);
			List<List<int>> sortedRelChords = GetSortedRelChords(sortedAbsChords);

			return sortedRelChords;
		}

		/// <summary>
		/// Shifts pitches up as necessary (by octaves), so that each list is in ascending order.  
		/// </summary>
		/// <param name="sortedAbsChords"></param>
		/// <returns></returns>
		private List<List<int>> GetSortedRelChords(List<List<int>> sortedAbsChords)
		{
			List<List<int>> rval = new List<List<int>>();
			foreach(var chord in sortedAbsChords)
			{
				var rChord = new List<int>
				{
					chord[0]
				};
				if(chord.Count > 1)
				{
					for(int i = 1; i < chord.Count; i++)
					{
						int prevValue = rChord[i-1];
						int nextValue = chord[i];
						while(prevValue > nextValue )
						{
							nextValue += 12;
						}
						rChord.Add(nextValue);
					}
				}
				rval.Add(rChord);
			}
			return rval;
		}

		/// <summary>
		/// Simply substitutes absolute pitches for the values in sortedPermutations
		/// </summary>
		/// <param name="root"></param>
		/// <param name="sortedPermutations"></param>
		/// <returns></returns>
		private List<List<int>> GetSortedAbsChords(List<int> root, List<List<int>> sortedPermutations)
		{
			List<List<int>> rval = new List<List<int>>();
			foreach(var list in sortedPermutations)
			{
				var newList = new List<int>();
				foreach(var number in list)
				{
					newList.Add(root[number - 1]);
				}
				rval.Add(newList);
			}
			return rval;
		}

		private List<List<int>> GetSortedLists(List<List<int>> allPermutations)
		{
			int ValueOf(List<int> list)
			{
				int factor = 1;
				int value = 0;
				for(int i = list.Count - 1; i >= 0; i--)
				{
					value += (list[i] * factor);
					factor *= 10;
				}
				return value;
			}

			List<List<int>> rval = new List<List<int>>();
			Dictionary<int, int> valuesDict = new Dictionary<int, int>();
			for(int i = 0; i < allPermutations.Count; i++)
			{
				valuesDict.Add(i, ValueOf(allPermutations[i]));
			}

			while(valuesDict.Count > 0)
			{
				int currentValue = int.MaxValue;
				int currentKey = 0;
				
				foreach(var key in valuesDict.Keys)
				{
					int value = valuesDict[key];
					if(value < currentValue)
					{
						currentValue = value;
						currentKey = key;
					}
				}

				rval.Add(new List<int>(allPermutations[currentKey]));
				valuesDict.Remove(currentKey);
			}

			return rval;
		}


	}
}
