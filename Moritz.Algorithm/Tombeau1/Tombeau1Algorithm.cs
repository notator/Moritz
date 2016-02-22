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

		public override IReadOnlyList<int> MidiChannelIndexPerOutputVoice { get { return new List<int>() { 0, 1, 2, 3, 4, 5, 6, 7 }; } }
		public override IReadOnlyList<int> MasterVolumePerOutputVoice { get { return new List<int>() { 100, 100, 100, 100, 100, 100, 100, 100 }; } }
		public override int NumberOfInputVoices { get { return 0; } }
		public override int NumberOfBars { get { return 3; } }

		/// <summary>
		/// See CompositionAlgorithm.DoAlgorithm()
		/// </summary>
		public override List<List<VoiceDef>> DoAlgorithm(List<Krystal> krystals, List<Palette> palettes)
		{
			_krystals = krystals;
			_palettes = palettes;

			#region main comment (thoughts etc.)
			/*********************************************************************************************
			Think Nancarrow, but (especially) with background/foreground. Think Study 1. Depth.

			The following parameters can be controlled using the Resident Sf2 Synth:
				Commands:	preset, pitchwheel
				Controls:	volume, pan, pitchWheelDeviation, allControllersOff, allSoundOff
				Note:		velocity, pitch.
			
			Tombeau1 is just going to use preset 0:0 (=grandPiano), so the available parameters are:
				"tuning" -- i.e. pitchWheelDeviation and pitchWheel
				pan,
				velocity,
				pitch

			Using Seqs and Trks should make it possible to compose powerful, comprehensible, pregnant
			relations between these	parameters at every	structural level. "Harmony & counterpoint" is
			not only going to be there in the pitches (chords, contours...), but also in the velocities
			and the pan positions... Tuning is a parameter that could be introduced as a surprise later...

			I need to think diagonally about the structural levels:
			> create the palettes containing the channel objects (atoms, MidiChordDefs): notes/chords, ornaments,
			> structural layers between the objects in the palettes and trks (or are trks constructed from
			  vertical reltions inside seqs?)
			> how notes/chords relate horizontally inside a trk
			> how notes/chords relate vertically inside a seq (How trks relate to each other inside seqs?),
			> and about how seqs relate to each other globally...

			Seqs can be superimposed, juxtaposed, repeated and re-ordered.
			
			Chords:
			1. Think Boulez' chord addition: Starting with any chord in system 1 of alternative 1 (below), add the
			   intervals of any other chord (or the same chord) in the system to each of the pitches in the original
			   chord. Remove any duplicate pitches -- which one probably depends on the velocities of the duplicates
			   (to be worked out later) -- and re-order the pitches (in ascending order).
			2. Study 1 chords can have holes... 
			3. Velocity gradients in Study 1 chords: bottom->top (="consonant") --> top->bottom (="dissonant")...
			4. Chord pitch transposition is allowed (but not ad. lib. transposition of the pitches inside the chord)...
			5. Chord velocity transposition is allowed (but not ad. lib. transposition of the velocities inside the chord)...
			6. "Chords are colour" (Stockhausen)
			7. If a chord is added to the pitch of a root chord, then its root should have the velocity of that pitch...
						
			*********************************************************************************************/
			#endregion main comments
			/**********************************************/
			Seq mainSeq;
			/*** Create the main seq here. ***/
			#region alternative 1: (Seq construction functions demo: new Seq(), seq.Clone(), seq.Concat() ) 
			//List<Trk> trks = new List<Trk>();
			//for(int i = 0; i < MidiChannelIndexPerOutputVoice.Count; ++i)
			//{
			//	List<IUniqueDef> iuds = new List<IUniqueDef>();
			//	iuds.Add(new MidiChordDef(new List<byte>() { 60 }, new List<byte>() { 60 }, 0, 500, true));
			//	iuds.Add(new RestDef(500, 1000));
			//	iuds.Add(new MidiChordDef(new List<byte>() { 60 }, new List<byte>() { 60 }, 1500, 500, true));

			//	Trk trk = new Trk((byte)MidiChannelIndexPerOutputVoice[i], iuds);
			//	trks.Add(trk);
			//}
			//Seq seq1 = new Seq(0, trks, MidiChannelIndexPerOutputVoice); // The Seq's MsPosition can change again later.
			//Seq seq2 = seq1.Clone();

			////seq2.MsPosition = 0;

			//// Add clef changes here
			////seq1.Trks[5].UniqueDefs.Insert(1, new ClefChangeDef("t", trks[5].UniqueDefs[1]));
			////seq1.Trks[6].UniqueDefs.Insert(2, new ClefChangeDef("t", trks[6].UniqueDefs[2]));

			//mainSeq = new Seq(seq1, seq2);
			//mainSeq.Concat(seq2);
			#endregion alternative 1: (Seq construction functions demo: new Seq(), seq.Clone(), seq.Concat() ) 

			#region alternative 2: Trks construction (from palette)
			List<Trk> trks = new List<Trk>();
			#region bar 1
			for(int i = 0; i < MidiChannelIndexPerOutputVoice.Count; ++i)
			{
				int chordDensity = 8 - i;
				List<IUniqueDef> mcds = PaletteMidiChordDefs(0);
				for(int j = 0; j < mcds.Count; ++j)
				{
					MidiChordDef mcd = mcds[j] as MidiChordDef;
					mcd.SetVerticalDensity(chordDensity);
					mcd.Lyric = (j).ToString() + "." + chordDensity.ToString();
				}
				Trk trk = new Trk(MidiChannelIndexPerOutputVoice[i], mcds);
				trks.Add(trk);
			}

			Seq seq1 = new Seq(0, trks, MidiChannelIndexPerOutputVoice); // The Seq's MsPosition can change again later.
			#endregion bar1

			#region bar2
			Seq seq2 = seq1.Clone();  //seq2.MsPosition = 0;
			#endregion bar2

			#region bar3
			Seq seq3 = seq1.Clone();
			#endregion bar3

			// Add clef changes here
			//seq2.Trks[0].UniqueDefs.Insert(0, new ClefChangeDef("b", seq2.Trks[0].UniqueDefs[0]));
			//seq2.Trks[6].UniqueDefs.Insert(2, new ClefChangeDef("b", seq2.Trks[6].UniqueDefs[2]));

			mainSeq = seq1;
			mainSeq.Concat(seq2);
			mainSeq.Concat(seq3);
			#endregion alternative 2: Trks construction (from palette)

			/**********************************************/

			List<VoiceDef> voiceDefs = GetVoiceDefs(mainSeq); // virtual function in CompositionAlgorithm.cs

			WarpDurations(voiceDefs);

			List<List<VoiceDef>> bars = CreateBars(voiceDefs, NumberOfBars);

			return bars;
		}

		private List<IUniqueDef> PaletteMidiChordDefs(int paletteIndex)
		{
			List<IUniqueDef> iuds = new List<IUniqueDef>();
			Palette palette = _palettes[paletteIndex];
			int msPosition = 0;
			for(int i = 0; i < palette.Count; ++i)
			{
				MidiChordDef mcd = palette.MidiChordDef(i);
				mcd.MsPosition = msPosition;
				msPosition += mcd.MsDuration;
				iuds.Add(mcd);
			}
			return iuds;
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
			// The grid is a superimposed structural layer, used for doing a time warp
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
		/// Rewrite this function when the composition is complete.
		/// Empty bars are allowed, but barlines should usually try to align with MidiChordDefs.
		/// </summary>
		private static List<int> GetBarlineEndMsPositions(List<VoiceDef> voiceDefs, int nBars)
		{
			List<int> barlineEndMsPositions = new List<int>();
			int sequenceMsDuration = voiceDefs[0].MsDuration;
			int barMsDuration = sequenceMsDuration / nBars;
			int msPos = barMsDuration;
			for(int i = 0; i < nBars - 1; ++i)
			{
				barlineEndMsPositions.Add(msPos);
				msPos += barMsDuration;
			}
			barlineEndMsPositions.Add(sequenceMsDuration);  // the final barline

			#region conditions (these also have to be met when this function has been rewritten) 
			Debug.Assert(barlineEndMsPositions[0] != 0);
			Debug.Assert(barlineEndMsPositions[barlineEndMsPositions.Count - 1] == sequenceMsDuration);
			Debug.Assert(barlineEndMsPositions.Count == nBars);
			#endregion

			return barlineEndMsPositions;
		}
	}
}
