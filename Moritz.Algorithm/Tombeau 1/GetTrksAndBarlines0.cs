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
		private void GetTrksAndBarlines0(out List<Trk> trks, out List<int> barlineMsPositions )
		{
			#region constants
			var availableBarDurations = new List<int>()
			{
				2000, 2151, 2331, 2548, 2811, 3113, 3533, 4033, 4670, 5490, 6565, 8000
			};
			//var availableRelativeDurations = new List<List<int>>()
			//{
			//	_durations1,
			//	_durations2,
			//	_durations3,
			//	_durations4,
			//	_durations5,
			//	_durations6,
			//	_durations7,
			//	_durations8,
			//	_durations9,
			//	_durations10,
			//	_durations11,
			//	_durations12 
			//};

			List<int> chordsRoot = new List<int>() { 54, 57, 59, 63, 65 }; // f#, a, b, d#, f
			List<List<int>> targetChords = GetChords(chordsRoot);

			//List<int> chords6root = new List<int>() { 54, 57, 59, 63, 65, 70 }; // f#, a, b, d#, f, a#
			//List<List<int>> chords6 = GetChords(chords6root);

			/**********************************************/
			#endregion

			Envelope centredEnvelope = _krystals[0].ToEnvelope(0, availableBarDurations.Count - 1); // values distributed around 11, gradually becoming more eccentric
			centredEnvelope.SetCount(targetChords.Count);
			List<int> barIndices = centredEnvelope.Original;

			Envelope basedEnvelope = _krystals[1].ToEnvelope(0, 127); // values increase gradually from 0 to 127, becoming more eccentric.


			var relativeDurations = new List<int>() { 3, 2, 1, 1 };
			//var startPitches = new List<int>() { 60, 64, 68, 71, 75 };
			//var targetPitches = new List<int>() { int.MaxValue, 68, 71, int.MaxValue, 68 };

			barlineMsPositions = new List<int>();
			Trk trk0 = new Trk(0);
			//ChainTrk chainTrk = null;
			Trk newBar = null;

			for(int i = 0; i < targetChords.Count; i++)
			{
				var startPitches = targetChords[i];
				//var targetPitches = targetChords[i];
				var msDuration = availableBarDurations[barIndices[i]];
				//var relativeDurations = availableRelativeDurations[barDurationIndices[i]];

				//var pitchEnv = new Envelope(new List<int>() { 0, 4, 3, 2, 0 }, 4, 4, relativeDurations.Count + 1);
				//chainTrk = new ChainTrk(0, msDuration, relativeDurations, startPitches, targetPitches, pitchEnv);
				//trk0.ConcatCloneAt(chainTrk, trk0.MsDuration);

				newBar = new Trk(0);
				List<byte> startVelocities = new List<byte>();
				foreach(var pitch in startPitches)
				{
					startVelocities.Add((byte)100);
				}
				newBar.Add(new MidiChordDef(M.MidiList(startPitches), startVelocities, msDuration, true));

				trk0.ConcatCloneAt(newBar, trk0.MsDuration);

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
