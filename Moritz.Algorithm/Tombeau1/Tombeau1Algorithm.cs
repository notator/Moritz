using System;
using System.Collections.Generic;
using System.Diagnostics;

using Krystals4ObjectLibrary;
using Moritz.Algorithm;
using Moritz.Palettes;
using Moritz.Spec;

namespace Moritz.Algorithm.Tombeau1
{
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

			/*********************************************************************************************
			Voices are basically in order background to foreground.
			I want to compose in terms of Seqs. A Seq has a List<Trk>, the Trks being in parallel channels.
			Composing with Seqs will promote the use of "harmony and counterpoint" parameters since the
			vertical relation between contained Trks will be under control.
			A bigger piece like this needs some kind of higher level organisation like this...
			I want to be able to superimpose Seqs and reorder them. Note that Seqs don't necessarily coincide with bars.
			The "harmony & counterpoint" is not only in the pitches, its also in the dynamics, and the pan position... 
			I need to create the palettes containing the ornaments, think about a top level structure, and about trks (lists of timeObjects)
			and how they relate...
			Tuning is a parameter that could be introduced later...
			*********************************************************************************************/

			List<Seq> seqs = new List<Seq>();

			/**********************************************/
			/*** Create the Seqs here. ***/
			#region temp code
			
			List<Trk> trks = new List<Trk>();
			List<IUniqueDef> iuds = new List<IUniqueDef>();
			iuds.Add(new MidiChordDef(new List<byte>() { 60 }, new List<byte>() { 60 }, 0, 500, true));
			iuds.Add(new RestDef(500, 800000 - 1000));
			iuds.Add(new MidiChordDef(new List<byte>() { 60 }, new List<byte>() { 60 }, 800000 - 500, 500, true));
			for(int i = 0; i < MidiChannelIndexPerOutputVoice.Count; ++i)
			{
				Trk trk = new Trk((byte)MidiChannelIndexPerOutputVoice[i], iuds);
				trks.Add(trk);
			}

			Seq seq = new Seq(trks, MidiChannelIndexPerOutputVoice);
			seq.MsPosition = 0;

			seqs.Add(seq);

			#endregion temp code
			/**********************************************/

			List<VoiceDef> voiceDefs = GetVoiceDefs(seqs); // virtual function in CompositionAlgorithm.cs

			WarpDurations(voiceDefs);

			List<List<VoiceDef>> bars = CreateBars(voiceDefs, NumberOfBars);

			return bars;
		}

		#region WarpDurations
		private class GridElement
		{
			public int index;
			public int msDuration;
			public int msPosition;
			public double durationFactor;
			public override string ToString()
			{
				return "index=" + index.ToString() + " msDuration=" + msDuration.ToString() + " msPosition=" + msPosition.ToString();
			}
		}

		private void WarpDurations(List<VoiceDef> voiceDefs)
		{
			// The grid is a superimposed structural layer, used for doing a time warp and setting barlines
			List<GridElement> gridData = getBasicGridData(voiceDefs[0].MsDuration);

			SetGridData(gridData); // set each GridElement.durationFactor

			// warp the durations here
		}

		/// <summary>
		/// Temporary function version(?) Maybe this could be improved...
		/// </summary>
		private List<GridElement> getBasicGridData(int totalGridDuration)
		{
			List<GridElement> grid = new List<GridElement>();
			int gridSize = 1000; // milliseconds
			int msPosition = 0;
			int remainingDuration = totalGridDuration; // milliseconds
			int index = 0;
			while(remainingDuration > 1000)
			{
				GridElement gridElem = new GridElement();
				gridElem.index = index++;
				gridElem.msDuration = gridSize;
				gridElem.msPosition = msPosition;
				gridElem.durationFactor = 1.0;
				msPosition += gridSize;
				grid.Add(gridElem);
				remainingDuration -= gridSize;
			}
			if(remainingDuration > 0)
			{
				GridElement gridElem = new GridElement();
				gridElem.index = index;
				gridElem.msDuration = remainingDuration;
				gridElem.msPosition = msPosition;
				gridElem.durationFactor = 1.0;
				grid.Add(gridElem);
			}
			return grid;
		}

		// demo function (causes accel to double speed).
		private void SetGridData(List<GridElement> gridData)
		{
			double exp = Math.Pow(2, ((double)1 / gridData.Count));
			double factor = 1;

			foreach(GridElement gridElement in gridData)
			{
				gridElement.durationFactor *= factor;
				factor *= exp;
			}
		}
		#endregion WarpDurations

		// Breaks the voiceDefs (each currently contains a complete channel for the piece) into bars.
		private List<List<VoiceDef>> CreateBars(List<VoiceDef> voiceDefs, int numberOfBars)
		{
			List<List<VoiceDef>> bars = new List<List<VoiceDef>>();

			List<int> barlineEndMsPositions = GetBarlineEndMsPositions(voiceDefs, numberOfBars);

			List<VoiceDef> longBar = voiceDefs;
			foreach(int barlineEndMsPosition in barlineEndMsPositions)
			{
				List<List<VoiceDef>> twoBars = SplitBar(longBar, barlineEndMsPosition);
				bars.Add(twoBars[0]);
				longBar = twoBars[1];
			}

			return bars;
		}

		/// <summary>
		/// Temp function that just returns nBars barline end positions equally distributed across the sequence.
		/// Empty bars are allowed, but barlines should usually try to align with MidiChordDefs.
		/// </summary>
		private static List<int> GetBarlineEndMsPositions(List<VoiceDef> voiceDefs, int nBars)
		{
			List<int> barlineMsPositions = new List<int>();
			int sequenceMsDuration = voiceDefs[0].MsDuration;
			int barMsDuration = sequenceMsDuration / nBars;
			int msPos = barMsDuration;
			for(int i = 0; i < nBars - 1; ++i)
			{
				barlineMsPositions.Add(msPos);
				msPos += barMsDuration;
			}
			barlineMsPositions.Add(sequenceMsDuration);	// the final barline

			#region conditions
			Debug.Assert(barlineMsPositions[0] != 0);
			Debug.Assert(barlineMsPositions[barlineMsPositions.Count - 1] == sequenceMsDuration);
			Debug.Assert(barlineMsPositions.Count == nBars);
			#endregion

			return barlineMsPositions;
		}
	}
}
