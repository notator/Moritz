using System;
using System.Collections.Generic;
using System.Diagnostics;

using Krystals4ObjectLibrary;
using Moritz.Algorithm;
using Moritz.Palettes;
using Moritz.Spec;

namespace Moritz.Algorithm.Tombeau1
{
	/// <summary>
	/// Algorithm for testing Song 6's palettes.
	/// This may develope as composition progresses...
	/// </summary>
	public class Tombeau1Algorithm : CompositionAlgorithm
	{
		/// <summary>
		/// This constructor can be called with both parameters null,
		/// just to get the overridden properties.
		/// </summary>
		public Tombeau1Algorithm()
			: base()
		{
		}

		public override List<int> MidiChannelIndexPerOutputVoice { get { return new List<int>() { 0, 1, 2, 3, 4, 5, 6 }; } }
		public override List<int> MasterVolumePerOutputVoice { get { return new List<int>() { 100, 100, 100, 100, 100, 100, 100 }; } }
		public override int NumberOfInputVoices { get { return 0; } }
		public override int NumberOfBars { get { return 200; } }

		/// <summary>
		/// See CompositionAlgorithm.DoAlgorithm()
		/// </summary>
		public override List<List<VoiceDef>> DoAlgorithm(List<Krystal> krystals, List<Palette> palettes)
		{
			_krystals = krystals;
			_palettes = palettes;

			// getBarData will probably do some squeezing and stretching of barlengths...
			List<BarData> barsData = getBarData(NumberOfBars);

			int tombeauMsDuration = getTotalDuration(barsData);
			 
			Debug.Assert(barsData.Count == NumberOfBars);

			/*********************************************************************************************
			Voices are basically in order background to foreground.
			I want each initially to be a simple krystal development, but to permute blocks (=trks?)...
			The blocks don't necessarily coincide with bars... but...
			I also want to write "harmony & counterpoint" by having related Trks in different voices,
			the Trks being simultaneous or slightly displaced.
			The "harmony & counterpoint" is not only in the pitches, its also in the dynamics, and the pan position...
			I need to create the palettes containing the ornaments, think about a top level structure, and about trks (lists of timeObjects) and how they relate...
			Tuning is a parameter that could be introduced later...
			Are trks controlled by krystal expansions? I think a bigger piece like this needs some kind of higher level organisation like that...
			*********************************************************************************************/
			List<VoiceDef> voiceDefs = new List<VoiceDef>();

			VoiceDef voice0 = CreateVoice0(barsData);
			int voiceMsDuration = TotalVoiceMsDuration(voice0);
			Debug.Assert(voiceMsDuration == tombeauMsDuration);
			voiceDefs.Add(voice0);

			VoiceDef voice1 = CreateVoice1(barsData);
			voiceMsDuration = TotalVoiceMsDuration(voice1);
			Debug.Assert(voiceMsDuration == tombeauMsDuration);
			voiceDefs.Add(voice1);

			VoiceDef voice2 = CreateVoice2(barsData);
			voiceMsDuration = TotalVoiceMsDuration(voice2);
			Debug.Assert(voiceMsDuration == tombeauMsDuration);
			voiceDefs.Add(voice2);

			VoiceDef voice3 = CreateVoice3(barsData);
			voiceMsDuration = TotalVoiceMsDuration(voice3);
			Debug.Assert(voiceMsDuration == tombeauMsDuration);
			voiceDefs.Add(voice3);

			VoiceDef voice4 = CreateVoice4(barsData);
			voiceMsDuration = TotalVoiceMsDuration(voice4);
			Debug.Assert(voiceMsDuration == tombeauMsDuration);
			voiceDefs.Add(voice4);

			VoiceDef voice5 = CreateVoice5(barsData);
			voiceMsDuration = TotalVoiceMsDuration(voice5);
			Debug.Assert(voiceMsDuration == tombeauMsDuration);
			voiceDefs.Add(voice5);

			VoiceDef voice6 = CreateVoice6(barsData);
			voiceMsDuration = TotalVoiceMsDuration(voice6);
			Debug.Assert(voiceMsDuration == tombeauMsDuration);
			voiceDefs.Add(voice6);

			List<List<VoiceDef>> bars = CreateBars(barsData, voiceDefs);

			// Possibly do a time warp here (warp the durations of the bars with their contents).
			// Possibly join some bars (i.e. delete some barlines) or add new barlines here.

			return bars;
		}

		// Simply breaks the voiceDefs (each currently contains a complete channel for the piece) into bars.
		// This code should not need to change.
		private List<List<VoiceDef>> CreateBars(List<BarData> barsData, List<VoiceDef> voiceDefs)
		{
			List<List<VoiceDef>> bars = new List<List<VoiceDef>>();

			foreach(BarData barData in barsData)
			{
				bars.Add(new List<VoiceDef>());
			}

			foreach(VoiceDef voiceDef in voiceDefs)
			{			
				byte midiChannel = voiceDef.MidiChannel;
				int iudIndex = 0;
				for(int b = 0; b < barsData.Count; ++b)
				{
					BarData barData = barsData[b];
					List<IUniqueDef> iuds = new List<IUniqueDef>();					
					int startMsPosition = barData.msPosition;
					int endMsPosition = barData.msPosition + barData.msDuration;
					while(iudIndex < voiceDef.UniqueDefs.Count)
					{
						IUniqueDef iud = voiceDef.UniqueDefs[iudIndex];
						if(iud.MsPosition >= endMsPosition)
						{
							break; // to next bar (without incrementing iudIndex)
						}
						if(iud.MsPosition >= startMsPosition)
						{
							iuds.Add(iud);
							iudIndex++;
						}						
					}
					Trk trk = new Trk(midiChannel, iuds);
					bars[b].Add(trk);
				}
			}
			return bars;
		}

		/// <summary>
		/// Creates a VoiceDef (actually a Trk) consisting of a list of bar rests.
		/// </summary>
		private VoiceDef CreateEmptyVoice(int channel, List<BarData> barsData)
		{
			List<IUniqueDef> rests = new List<IUniqueDef>();
			foreach(BarData barData in barsData)
			{
				IUniqueDef rest = new RestDef(barData.msPosition, barData.msDuration);
				rests.Add(rest);
			}
			Trk voiceDef = new Trk((byte)channel, rests);

			return voiceDef;
		}

		private VoiceDef CreateVoice6(List<BarData> barsData)
		{
			return CreateEmptyVoice(6, barsData);
		}

		private VoiceDef CreateVoice5(List<BarData> barsData)
		{
			return CreateEmptyVoice(5, barsData);
		}

		private VoiceDef CreateVoice4(List<BarData> barsData)
		{
			return CreateEmptyVoice(4, barsData);
		}

		private VoiceDef CreateVoice3(List<BarData> barsData)
		{
			return CreateEmptyVoice(3, barsData);
		}

		private VoiceDef CreateVoice2(List<BarData> barsData)
		{
			return CreateEmptyVoice(2, barsData);
		}

		private VoiceDef CreateVoice1(List<BarData> barsData)
		{
			return CreateEmptyVoice(1, barsData);
		}

		private VoiceDef CreateVoice0(List<BarData> barsData)
		{
			return CreateEmptyVoice(0, barsData);
		}

		private int getTotalDuration(List<BarData> barsData)
		{
			BarData lastBar = barsData[barsData.Count - 1];
			return lastBar.msPosition + lastBar.msDuration;
		}

		private int TotalVoiceMsDuration(VoiceDef voiceDef)
		{
			int total = 0;
			foreach(IUniqueDef iud in voiceDef.UniqueDefs)
			{
				total += iud.MsDuration;
			}
			return total;
		}

		private class BarData
		{
			public int number;
			public int msDuration;
			public int msPosition;
			public override string ToString()
			{
				return "barNumber=" + number.ToString() + " msDuration=" + msDuration.ToString() + " msPosition=" + msPosition.ToString();
			}
		}

		private List<BarData> getBarData(int numberOfBars)
		{
			Debug.Assert(_barMsDurations.Count == NumberOfBars);
			List<BarData> barsData = new List<BarData>();
			int msPosition = 0;
			foreach(KeyValuePair<int,int> numDur in _barMsDurations)
			{
				BarData bd = new BarData();
				bd.number = numDur.Key;
				bd.msDuration = numDur.Value;
				bd.msPosition = msPosition;
				msPosition += bd.msDuration;
				barsData.Add(bd);
			}
			return barsData;
		}

		/// <summary>
		/// Key is barNumber, Value is msDuration
		/// </summary>
		private static readonly Dictionary<int, int> _barMsDurations = new Dictionary<int, int>
		{
			{ 1, 4000 }, { 2, 4000 }, { 3, 4000 }, { 4, 4000 }, { 5, 4000 }, { 6, 4000 }, { 7, 4000 }, { 8, 4000 }, { 9, 4000 },
			{ 10, 4000 }, { 11, 4000 }, { 12, 4000 }, { 13, 4000 }, { 14, 4000 }, { 15, 4000 }, { 16, 4000 }, { 17, 4000 }, { 18, 4000 }, { 19, 4000 },
			{ 20, 4000 }, { 21, 4000 }, { 22, 4000 }, { 23, 4000 }, { 24, 4000 }, { 25, 4000 }, { 26, 4000 }, { 27, 4000 }, { 28, 4000 }, { 29, 4000 },
			{ 30, 4000 }, { 31, 4000 }, { 32, 4000 }, { 33, 4000 }, { 34, 4000 }, { 35, 4000 }, { 36, 4000 }, { 37, 4000 }, { 38, 4000 }, { 39, 4000 },
			{ 40, 4000 }, { 41, 4000 }, { 42, 4000 }, { 43, 4000 }, { 44, 4000 }, { 45, 4000 }, { 46, 4000 }, { 47, 4000 }, { 48, 4000 }, { 49, 4000 },
			{ 50, 4000 }, { 51, 4000 }, { 52, 4000 }, { 53, 4000 }, { 54, 4000 }, { 55, 4000 }, { 56, 4000 }, { 57, 4000 }, { 58, 4000 }, { 59, 4000 },
			{ 60, 4000 }, { 61, 4000 }, { 62, 4000 }, { 63, 4000 }, { 64, 4000 }, { 65, 4000 }, { 66, 4000 }, { 67, 4000 }, { 68, 4000 }, { 69, 4000 },
			{ 70, 4000 }, { 71, 4000 }, { 72, 4000 }, { 73, 4000 }, { 74, 4000 }, { 75, 4000 }, { 76, 4000 }, { 77, 4000 }, { 78, 4000 }, { 79, 4000 },
			{ 80, 4000 }, { 81, 4000 }, { 82, 4000 }, { 83, 4000 }, { 84, 4000 }, { 85, 4000 }, { 86, 4000 }, { 87, 4000 }, { 88, 4000 }, { 89, 4000 },
			{ 90, 4000 }, { 91, 4000 }, { 92, 4000 }, { 93, 4000 }, { 94, 4000 }, { 95, 4000 }, { 96, 4000 }, { 97, 4000 }, { 98, 4000 }, { 99, 4000 },
			
			{ 100, 4000 }, { 101, 4000 }, { 102, 4000 }, { 103, 4000 }, { 104, 4000 }, { 105, 4000 }, { 106, 4000 }, { 107, 4000 }, { 108, 4000 }, { 109, 4000 },
			{ 110, 4000 }, { 111, 4000 }, { 112, 4000 }, { 113, 4000 }, { 114, 4000 }, { 115, 4000 }, { 116, 4000 }, { 117, 4000 }, { 118, 4000 }, { 119, 4000 },
			{ 120, 4000 }, { 121, 4000 }, { 122, 4000 }, { 123, 4000 }, { 124, 4000 }, { 125, 4000 }, { 126, 4000 }, { 127, 4000 }, { 128, 4000 }, { 129, 4000 },
			{ 130, 4000 }, { 131, 4000 }, { 132, 4000 }, { 133, 4000 }, { 134, 4000 }, { 135, 4000 }, { 136, 4000 }, { 137, 4000 }, { 138, 4000 }, { 139, 4000 },
			{ 140, 4000 }, { 141, 4000 }, { 142, 4000 }, { 143, 4000 }, { 144, 4000 }, { 145, 4000 }, { 146, 4000 }, { 147, 4000 }, { 148, 4000 }, { 149, 4000 },
			{ 150, 4000 }, { 151, 4000 }, { 152, 4000 }, { 153, 4000 }, { 154, 4000 }, { 155, 4000 }, { 156, 4000 }, { 157, 4000 }, { 158, 4000 }, { 159, 4000 },
			{ 160, 4000 }, { 161, 4000 }, { 162, 4000 }, { 163, 4000 }, { 164, 4000 }, { 165, 4000 }, { 166, 4000 }, { 167, 4000 }, { 168, 4000 }, { 169, 4000 },
			{ 170, 4000 }, { 171, 4000 }, { 172, 4000 }, { 173, 4000 }, { 174, 4000 }, { 175, 4000 }, { 176, 4000 }, { 177, 4000 }, { 178, 4000 }, { 179, 4000 },
			{ 180, 4000 }, { 181, 4000 }, { 182, 4000 }, { 183, 4000 }, { 184, 4000 }, { 185, 4000 }, { 186, 4000 }, { 187, 4000 }, { 188, 4000 }, { 189, 4000 },
			{ 190, 4000 }, { 191, 4000 }, { 192, 4000 }, { 193, 4000 }, { 194, 4000 }, { 195, 4000 }, { 196, 4000 }, { 197, 4000 }, { 198, 4000 }, { 199, 4000 },

			{ 200, 4000 }
		};
	}
}
