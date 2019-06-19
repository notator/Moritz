using System;
using System.Collections.Generic;
using System.Diagnostics;

using Krystals4ObjectLibrary;
using Moritz.Globals;
using Moritz.Palettes;
using Moritz.Spec;
using Moritz.Symbols;

namespace Moritz.Algorithm.Tombeau1
{
	public partial class Tombeau1Algorithm : CompositionAlgorithm
	{
		private void GetTrksAndBarlines0(out List<Trk> trks, out List<int> barlineMsPositions, out List<List<int>> targetChords)
		{
			#region constants
			var availableBarDurations = new List<int>()
			{
				2000, 2151, 2331, 2548, 2811, 3113, 3533, 4033, 4670, 5490, 6565, 8000
			};
			List<int> chordsRoot = new List<int>() { 54, 57, 59, 63, 65 }; // f#, a, b, d#, f
			targetChords = GetChords(chordsRoot);
			#endregion

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

			//Trk trk0 = new Trk(0, 0, trk0iuds);
			Trk trk1 = new Trk(1, 0, new List<IUniqueDef>());
			Trk trk2 = new Trk(2, 0, new List<IUniqueDef>());
			Trk trk3 = new Trk(3, 0, new List<IUniqueDef>());

			trks = new List<Trk>() { trk0, trk1, trk2, trk3 };
		}

		private List<List<int>> GetChords(List<int> root)
		{
			List<List<int>> rval = new List<List<int>>();
			rval.Add(root);
			List<int> next = null;
			//for(int j = 0; j < 5; j++)
			for(int j = 0; j < 6; j++)
			{
				List<List<int>> newChords = new List<List<int>>();
				foreach(var list in rval)
				{
					for(int i = 0; i < list.Count; i++)
					{
						next = new List<int>(list);
						next[i] += 12;
						newChords.Add(next);
					}
				}
				rval.AddRange(newChords);
			}

			foreach(var list in rval)
			{
				list.Sort();
			}

			List<List<int>> duplicates = new List<List<int>>();
			for(int i = rval.Count - 1; i > 0; i--)
			{
				HashSet<int> t1 = new HashSet<int>(rval[i]);
				for(int j = i - 1; j >= 0; j--)
				{
					HashSet<int> t2 = new HashSet<int>(rval[j]);
					if(t1.SetEquals(t2))
					{
						duplicates.Add(rval[i]);
						rval.RemoveAt(i);
						break;
					}
				}
			}

			List<List<int>> spanTooBig = new List<List<int>>();
			List<List<int>> spanTooSmall = new List<List<int>>();
			for(int i = rval.Count - 1; i > 0; i--)
			{
				var list = rval[i];
				if((list[list.Count - 1] - list[0]) > 24)
				{
					spanTooBig.Add(list);
					rval.RemoveAt(i);
				}
				else if((list[list.Count - 1] - list[0]) < 12)
				{
					spanTooSmall.Add(list);
					rval.RemoveAt(i);
				}
			}

			List<List<int>> intervalTooBig = new List<List<int>>();
			List<List<int>> intervalTooSmall = new List<List<int>>();
			for(int i = rval.Count - 1; i > 0; i--)
			{
				var list = rval[i];
				for(int j = 1; j < list.Count; j++)
				{
					int diff = list[j] - list[j - 1];
					if(diff < 2)
					{
						intervalTooSmall.Add(list);
						rval.RemoveAt(i);
						break;
					}
					if(diff > 12)
					{
						intervalTooBig.Add(list);
						rval.RemoveAt(i);
						break;
					}
				}
			}

			return rval;
		}
	}
}
