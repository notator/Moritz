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
		public override IReadOnlyList<int> MasterVolumePerOutputVoice { get { return new List<int>() { 127, 127, 127, 127, 127, 127, 127, 127 }; } }
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

            Block system1Block = GetSystem1Block();

            Block system2Block = GetSystem2Block(system1Block);

            Block system3Block = GetSystem3Block(system1Block);

            Block system4Block = GetSystem4VelocityTestBlock();

            #region set barlines
            List<int> barlineEndMsPositions = new List<int>();
            barlineEndMsPositions.Add(system1Block.MsDuration);
            barlineEndMsPositions.Add(system1Block.MsDuration + system2Block.MsDuration);
            barlineEndMsPositions.Add(system1Block.MsDuration + system2Block.MsDuration + system3Block.MsDuration);
            barlineEndMsPositions.Add(system1Block.MsDuration + system2Block.MsDuration + system3Block.MsDuration + system4Block.MsDuration);
            #endregion set barlines

            Block sequence = system1Block;
			sequence.Concat(system2Block);
            sequence.Concat(system3Block);
            sequence.Concat(system4Block);

            List<List<VoiceDef>> bars = ConvertBlockToBars(sequence, barlineEndMsPositions);

            // Add clef changes here.
            // Testing... 
            //bars[0][0].InsertClefChange(12, "t");
            //bars[0][0].InsertClefChange(2, "b");

            //bars[1][1].InsertClefChange(3, "b");

            return bars;
		}

        private Block GetSystem4VelocityTestBlock()
        {
            List<Trk> trks = new List<Trk>();
            MidiChordDef baseMidiChordDef = new MidiChordDef(new List<byte>() { (byte)64 }, new List<byte>() { (byte)127 }, 0, 1000, true);
            byte velocity = 0;
            for(int trkIndex = 0; trkIndex < MidiChannelIndexPerOutputVoice.Count; ++trkIndex)
            {
                Trk trk = new Trk(MidiChannelIndexPerOutputVoice[trkIndex], 0, new List<IUniqueDef>());
                for(int j = 0; j < 16; ++j)
                {
                    MidiChordDef mcd = baseMidiChordDef.Clone() as MidiChordDef;
                    mcd.BasicMidiChordDefs[0].Velocities[0] = velocity;
                    mcd.Lyric = (velocity).ToString();
                    velocity++;

                    trk.Add(mcd);
                }
                
                trks.Add(trk);
            }

            Seq seq = new Seq(0, trks, MidiChannelIndexPerOutputVoice); // The Seq's MsPosition can change again later.

            return new Block(seq);
        }

        private Block GetSystem3Block(Block system1Block)
        {
            Block system3Block = system1Block.Clone();

            // Blocks can be warped...
            List<double> warp = new List<double>() { 0, 0.1, 0.3, 0.6, 1 };
            system3Block.WarpDurations(warp);

            return system3Block;
        }

        private Block GetSystem1Block()
        {
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
                Trk trk = new Trk(MidiChannelIndexPerOutputVoice[i], 0, sys1mcds);
                sys1Trks.Add(trk);
            }
            Seq system1Seq = new Seq(0, sys1Trks, MidiChannelIndexPerOutputVoice); // The Seq's MsPosition can change again later.
            return new Block(system1Seq);
        }

        private Block GetSystem2Block(Block system1Block)
        {
            List<Trk> sys2Trks = new List<Trk>();
            int startIndex = 2;
            for(int i = 0; i < MidiChannelIndexPerOutputVoice.Count; ++i)
            {
                Trk trk = new Trk(MidiChannelIndexPerOutputVoice[i], 0, new List<IUniqueDef>());
                MidiChordDef mcd1 = (MidiChordDef)system1Block.Trks[5][startIndex].Clone();
                trk.Add(mcd1);
                MidiChordDef mcd2 = (MidiChordDef)system1Block.Trks[5][startIndex + 1].Clone();
                trk.Add(mcd2);

                MidiChordDef sum = (MidiChordDef)mcd1.Clone();
                sum.Lyric = "sum";
                sum.AddNotes(mcd2);

                trk.Add(sum);

                RestDef rest = new RestDef(0, system1Block.MsDuration - trk.EndMsPositionReFirstIUD);
                trk.Add(rest);

                sys2Trks.Add(trk);

                startIndex++;
            }
            Seq system2Seq = new Seq(0, sys2Trks, MidiChannelIndexPerOutputVoice); // The Seq's MsPosition can change again later.

            return new Block(system2Seq);
        }

        private List<IUniqueDef> PaletteMidiChordDefs(int paletteIndex)
		{
			List<IUniqueDef> iuds = new List<IUniqueDef>();
			Palette palette = _palettes[paletteIndex];
			int msPositionReFirstIUD = 0;
			for(int i = 0; i < palette.Count; ++i)
			{
				MidiChordDef mcd = palette.MidiChordDef(i);
				mcd.MsPositionReFirstUD = msPositionReFirstIUD;
				msPositionReFirstIUD += mcd.MsDuration;
				iuds.Add(mcd);
			}
			return iuds;
		}

	}
}
