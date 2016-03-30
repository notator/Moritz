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

			Seqs can be superimposed, juxtaposed, repeated (with/without variation) and re-ordered.
			
			Chords:
			** "Chords are colour" (Stockhausen)
            ** I want to keep the number of pitches in chords fairly low, so that they are recognisable. Strategies like
               Boulez' chord addition don't really work...
            ** Chords can have any pitches (construct some interesting palettes with some differently coloured chords).
               But velocities can be controlled independently to emphasise "harmonic" relations in areas of the score.
               Repeated chord sequences can be thus "filtered" differently as in a digital painting...
            ** Write a function that sets note velocities according to the position of the note's pitch in a (harmonic)
               hierarchy of *absolute pitches* (not intervals):
               The absolute, equal temperament pitch hierarchy relating to the harmonic series can be found as follows
               (decimals rounded to 3 figures):
                 absolute    equal              harmonic:     absolute         closest
                 pitch:   temperament                         harmonic     equal temperament
                            factor:                            factor:      absolute pitch:
                    0:       1.000       |          2   ->   2/2  = 1.000  ->     0:
                    1:       1.059       |          3   ->   3/2  = 1.500  ->     7:
                    2:       1.122       |          5   ->   5/4  = 1.250  ->     4:
                    3:       1.189       |          7   ->   7/4  = 1.750  ->     10:
                    4:       1.260       |          9   ->   9/8  = 1.125  ->     2:
                    5:       1.335       |         11   ->  11/8  = 1.375  ->     5:
                    6:       1.414       |         13   ->  13/8  = 1.625  ->     9:
                    7:       1.498       |         15   ->  15/8  = 1.875  ->     11:
                    8:       1.587       |         17   ->  17/16 = 1.063  ->     1:
                    9:       1.682       |         19   ->  19/16 = 1.187  ->     3:
                    10:      1.782       |         21   ->  21/16 = 1.313  ->     
                    11:      1.888       |         23   ->  23/16 = 1.438  ->     6:
                                         |         25   ->  25/16 = 1.563  ->     8:

               Giving the absolute pitch hierarchy:
                    0, 7, 4, 10, 2, 5, 9, 11, 1, 3, 6, 8         
               
               Octaves are treated equivalently:
                    12, 24, 36 etc. will be given the same velocity as the base pitch 0,
                    19, 31, 43 etc. will be given the same velocity as the fifth (7),
                    etc.

               The pitchHierarchy could be chosen from a linear or circular field that includes the above absolute hierarchy.
               I think I should try both linear and circular pitch hierarchy tables, to see which is more effective.

               The pitches are all relative to 0, so the 0 should probably be omitted.

               The function could be defined as:
                        MidiChordDef.SetVelocities( int baseAbsPitch, // The (absolute) base pitch for the pitch hierarchy.
                                                    int baseVelocity, // the velocity given to any absolute base pitch (if it exists) in the MidiChordDef
                                                    List<int> pitchHierarchy, // e.g. [7, 4, 10, 2, 5, 9, 11, 1, 3, 6, 8] or [8, 6, 3, 1, 11, 9, 5, 2, 10, 4, 7] etc.
                                                    List<int> velocityHierarchy, // velocities... (see below).
                                                  );
               The velocityHierarchy is the same length as the pitchHierarchy, and contains a set of values that are always
               in descending order. The values are factors by which to multiply the baseVelocity for each of the correspondng
               pitches in the pitchHierarchy. The factors are calculated (like eccentricity) using a radius coordinate at 0 degrees
               in a semicircular linear field: the percentages being calculated from the (squares of the) distances to the foci.
               Maybe hide the calculation of the factors inside the function, and just pass a velocityEccentricity value argument
               (in the range [1..7])
               The pitch hierarchy table could also be hidden...
               The function would then reduce to:
                        MidiChordDef.SetVelocities( int baseAbsPitch, // The (absolute) base pitch for the pitch hierarchy.
                                                    int baseVelocity, // the velocity given to any absolute base pitch (if it exists) in the MidiChordDef
                                                    int pitchHierarchyIndex,
                                                    int velocityEccentricity
                                                  );
               Both the pitchHierarchy and velocityHierarchy tables would then be static objects inside the function.
               Interesting that this function has density, eccentricity and absolute hierarchy inputs! (The density is the density of
               the MidiChordDef.)
                

            ** Move unused VoiceDef and Trk functions into "unused" or "Song Six" #regions, or into separate "partial class" files.
             
			//2. Boulez' chord addition has been implemented in the function MidiChordDef.AddNotes(MidiChordDef).
			//3. Chord pitch transposition has already been implemented in the function MidiChordDef.Transpose(MidiChordDef),
			//   but it contains known bugs. Chord velocity transposition could be implemented analogously.
			//4. Chords can have holes... Do I need filters? Maybe a MidiChordDef.SubtractNotes(...) function? 
			//5. Velocity gradients in the root chords of additions: bottom->top (="consonant") --> top->bottom (="dissonant")...
			
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
