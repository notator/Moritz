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
			1. "Chords are colour" (Stockhausen)
			2. Boulez' chord addition has been implemented in the function MidiChordDef.AddNotes(MidiChordDef).
			3. Chord pitch transposition has already been implemented in the function MidiChordDef.Transpose(MidiChordDef),
			   but it contains known bugs. Chord velocity transposition should be implemented analogously.
			4. Chords can have holes... Do I need filters? Maybe a MidiChordDef.SubtractNotes(...) function? 
			5. Velocity gradients in the root chords of additions: bottom->top (="consonant") --> top->bottom (="dissonant")...
			
			*********************************************************************************************/
			#endregion main comments
			/**********************************************/
			Seq mainSeq;

			#region system 1
			List<Trk> sys1Trks = new List<Trk>();
			for(int i = 0; i < MidiChannelIndexPerOutputVoice.Count; ++i)
			{
				int chordDensity = 8 - i;
				List<IUniqueDef> sys1mcds = PaletteMidiChordDefs(0);
				for(int j = 0; j < sys1mcds.Count; ++j)
				{
					MidiChordDef mcd = sys1mcds[j] as MidiChordDef;
					mcd.Lyric = (j).ToString() + "." + chordDensity.ToString();

					mcd.SetVerticalDensity(chordDensity);
				}
				Trk trk = new Trk(MidiChannelIndexPerOutputVoice[i], sys1mcds);
				sys1Trks.Add(trk);
			}
			Seq system1Seq = new Seq(0, sys1Trks, MidiChannelIndexPerOutputVoice); // The Seq's MsPosition can change again later.
			#endregion system 1

			#region  system 2
			List<Trk> sys2Trks = new List<Trk>();
			int startIndex = 2;
			for(int i = 0; i < MidiChannelIndexPerOutputVoice.Count; ++i)
			{
				Trk trk = new Trk(MidiChannelIndexPerOutputVoice[i], new List<IUniqueDef>());
				MidiChordDef mcd1 = (MidiChordDef)sys1Trks[5][startIndex].Clone();
				trk.Add(mcd1);
				MidiChordDef mcd2 = (MidiChordDef)sys1Trks[5][startIndex + 1].Clone();
				trk.Add(mcd2);

				MidiChordDef sum = (MidiChordDef) mcd1.Clone();
				sum.Lyric = "sum";
				sum.AddNotes(mcd2);

				trk.Add(sum);

				RestDef rest = new RestDef(0, system1Seq.MsDuration - trk.EndMsPosition);
				trk.Add(rest);

				sys2Trks.Add(trk);

				startIndex++;
			}
			Seq system2Seq = new Seq(0, sys2Trks, MidiChannelIndexPerOutputVoice); // The Seq's MsPosition can change again later.
			#endregion  system 2

			#region  system 3
			// Blocks can be warped...
			List<double> warp = new List<double>() { 0, 0.1, 0.3, 0.6, 1 };
			Block system3Block = new Block(system1Seq.Clone());
			system3Block.WarpDurations(warp);
			#endregion  system 3

			// Add clef changes here
			// system1Seq.Trks[0].UniqueDefs.Insert(0, new ClefChangeDef("b", system1Seq.Trks[0].UniqueDefs[0]));
			system3Block.Trks[0].UniqueDefs.Insert(0, new ClefChangeDef("b", system3Block.Trks[0].UniqueDefs[0]));
			//seq2.Trks[6].UniqueDefs.Insert(2, new ClefChangeDef("b", seq2.Trks[6].UniqueDefs[2]));

			mainSeq = system1Seq.Clone();
			mainSeq.Concat(system2Seq);
			mainSeq.Concat(system3Block);

			// Blocks expose a list of VoiceDefs
			Block mainSequence = new Block(mainSeq); // converts mainSeq to a block

			/**********************************************/

			List<VoiceDef> voiceDefs = mainSequence.VoiceDefs;

			#region set barlines
			List<int> barlineEndMsPositions = new List<int>();
			barlineEndMsPositions.Add(system1Seq.MsDuration);
			barlineEndMsPositions.Add(system1Seq.MsDuration + system2Seq.MsDuration);
			barlineEndMsPositions.Add(system1Seq.MsDuration + system2Seq.MsDuration + system3Block.MsDuration);
			#endregion set barlines

			List<List<VoiceDef>> bars = CreateBars(voiceDefs, barlineEndMsPositions);

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

		// Breaks the voiceDefs (each currently contains a complete channel for the piece) into bars=systems.
		private List<List<VoiceDef>> CreateBars(List<VoiceDef> voiceDefs, List<int> barlineEndMsPositions)
		{
			List<List<VoiceDef>> bars = new List<List<VoiceDef>>();

			List<VoiceDef> longBar = voiceDefs;
			foreach(int barlineEndMsPosition in barlineEndMsPositions)
			{
				List<List<VoiceDef>> twoBars = SplitBar(longBar, barlineEndMsPosition);
				bars.Add(twoBars[0]);
				longBar = twoBars[1];
			}

			return bars;
		}
	}
}
