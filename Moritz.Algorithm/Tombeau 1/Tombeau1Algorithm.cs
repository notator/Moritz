using System;
using System.Collections.Generic;
using System.Diagnostics;

using Krystals4ObjectLibrary;

using Moritz.Globals;
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
               Boulez' chord addition don't really work... (The code I've written for that in MidiChordDef should be deleted.)
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
                    0:       1.000       |          1   ->   1/1  = 1.000  ->     0:
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

               The pitch hierarchy is for absolute pitches, so should be related to a circular field, but the pitches
               are all relative to 0, so the 0 should initially be omitted in the field and replaced later at position 0
               in all hierarchies. The result is defined below in:

                        static List<List<int>> circularPitchHierarchies

               Since I want to impose global harmonic hierarchies, I've defined two functions:
               
                       private List<int> GetVelocityPerAbsolutePitch
                                            (
                                                int basePitch,// The base pitch for the pitch hierarchy.
                                                int baseVelocity,   // the velocity given to any absolute base pitch (if it exists) in the MidiChordDef
                                                List<int> relativePitchHierarchy, // one of the circularPitchHierarchies,
                                                List<double> velocityFactorPerPitch // (see below) 
                                            )
                       
                                   // velocityFactorPerPitch is a list of 12 values in descending order, each value in range 1..0.
                                   // This list is the same length as the relativePitchHierarchy (12 values), and contains a set
                                   // of values that are always in descending order. The values are factors by which to multiply
                                   // the baseVelocity for each of the corresponding pitches in the relativePitchHierarchy.
                                   // The factors are standard values taken from a field calculated using a static function:
                                   //              VelocityFactorsPerPitch() (see below).
               and
                       
                        private void MidiChordDef.SetVelocityPerAbsolutePitch(List<int> velocityPerAbsolutePitch) 
                                       
               The result of the first function can be passed to many different chords using the second function.
         
            ** Thinking about the representation of velocity in chords/noteheads:
               I should create a new Notator class that notates velocities in outputChords as coloured or grey-scale noteheads.
               I can first create the notator class, and then experiment to see which colours or grey-scales work best.
               Dynamic symbols could be reserved for use in inputChords..
               Grey-scale velocities might be 100%black(==fff) --> 30%black(==pppp).
               Coloured noteheads might be BrightRed(==fff) --> DarkRed(==mf), DarkGreen(==mp) --> BrightGreen(==pppp).
               The Assistant Composer Form should provide a pop-up menu for selecting the appropriate notator for the score.

            ** The concept of InputChords is still a bit hazy. Why should they ever have more than one notehead?...
               But I could implement version (A) for Tombeau 1(that doesn't have inputChords) anyway...
               Also, *annotations* that are instructions to performers (hairpins spring to mind). Conventional dynamic symbols are,
               I think meant for *performers*, so should only be attached to inputChords...
               The info in *outputChords* is the info that could go in a MIDI file, and vice versa.
             
			//3. Chord pitch transposition has already been implemented in the function MidiChordDef.Transpose(MidiChordDef),
			//   but it contains known bugs. Chord velocity transposition could be implemented analogously.
			//4. Chords can have holes... Do I need filters? Maybe a MidiChordDef.SubtractNotes(...) function? 
			//5. Velocity gradients in the root chords of additions: bottom->top (="consonant") --> top->bottom (="dissonant")...
			
			*********************************************************************************************/
            #endregion main comments
            /**********************************************/

            Block system1Block = GetSystem1Block();

            Block system2Block = GetSystem2Block(system1Block);

            Block system3Block = GetSystem3VelocityTestBlock();

            #region set barlines
            List<int> barlineEndMsPositions = new List<int>();
            barlineEndMsPositions.Add(system1Block.MsDuration);
            barlineEndMsPositions.Add(system1Block.MsDuration + system2Block.MsDuration);
            barlineEndMsPositions.Add(system1Block.MsDuration + system2Block.MsDuration + system3Block.MsDuration);
            #endregion set barlines

            Block sequence = system1Block;
            sequence.Concat(system2Block);
            sequence.Concat(system3Block);

            List<List<VoiceDef>> bars = ConvertBlockToBars(sequence, barlineEndMsPositions);

            // Add clef changes here.
            // Testing... 
            bars[0][0].InsertClefChange(9, "t");
            bars[0][0].InsertClefChange(2, "b");

            //bars[1][1].InsertClefChange(3, "b");

            return bars;
		}

        private Block GetSystem3VelocityTestBlock()
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

        private Block GetSystem2Block(Block system1Block)
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
                int chordDensity = 8;
                List<IUniqueDef> sys1mcds = PaletteMidiChordDefs(0);
                for(int j = 0; j < sys1mcds.Count; ++j)
                {
                    MidiChordDef mcd = sys1mcds[j] as MidiChordDef;
                    mcd.Transpose(j);
                    mcd.Lyric = (j).ToString() + "." + chordDensity.ToString();

                    mcd.SetVerticalDensity(chordDensity);

                    List<int> velocityPerAbsolutePitch =
                        GetVelocityPerAbsolutePitch(j,    // The base pitch for the pitch hierarchy.
                                                    127,  // the velocity given to any absolute base pitch (if it exists) in the MidiChordDef
                                                    circularPitchHierarchies[i], // the pitch hierarchy for the chord,
                                                    velocityFactors[0]  // A list of 12 values in descending order, each value in range 1..0
                                                   );

                    mcd.SetVelocityPerAbsolutePitch(velocityPerAbsolutePitch);
                }
                Trk trk = new Trk(MidiChannelIndexPerOutputVoice[i], 0, sys1mcds);
                sys1Trks.Add(trk);
            }
            Seq system1Seq = new Seq(0, sys1Trks, MidiChannelIndexPerOutputVoice); // The Seq's MsPosition can change again later.
            return new Block(system1Seq);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="basePitch">In range [0..127]</param>
        /// <param name="baseVelocity">In range [0..127]</param>
        /// <param name="relativePitchHierarchy">A list of 12 unique values in range 0..11</param>
        /// <param name="velocityFactors">A list of 12 values in descending order, in range 1..0</param>
        private List<int> GetVelocityPerAbsolutePitch(int basePitch,
                                                      int baseVelocity,
                                                      List<int> relativePitchHierarchy,
                                                      List<double> velocityFactorPerPitch
                                                     )
        {
            #region conditions
            Debug.Assert(basePitch >= 0 && basePitch <= 127);
            Debug.Assert(baseVelocity >= 0 && baseVelocity <= 127);
            Debug.Assert(relativePitchHierarchy.Count == 12);
            Debug.Assert(relativePitchHierarchy[0] == 0);
            foreach(int pitch in relativePitchHierarchy)
            {
                Debug.Assert(pitch >= 0 && pitch <= 11);
            }
            Debug.Assert(velocityFactorPerPitch.Count == 12);
            Debug.Assert(velocityFactorPerPitch[0] == 1.0);
            for(int i = 1; i < 12; ++i)
            {
                double factor = velocityFactorPerPitch[i];
                Debug.Assert(factor <= 1.0 && factor >= 0.0);
                Debug.Assert(velocityFactorPerPitch[i - 1] >= factor);
            }
            #endregion conditions

            List<int> velocityPerAbsPitch = new List<int>();
            int baseAbsPitch = basePitch % 12;
            for(int absPitch = 0; absPitch < 12; ++absPitch)
            {
                int velocity = 0;
                int pitchRelBase = absPitch - baseAbsPitch;
                pitchRelBase = (pitchRelBase >= 0) ? pitchRelBase : pitchRelBase + 12;

                int vFactorIndex = relativePitchHierarchy.IndexOf(pitchRelBase);
                velocity = M.MidiValue((int)(baseVelocity * velocityFactorPerPitch[vFactorIndex]));

                velocityPerAbsPitch.Add(velocity);
            }
            return velocityPerAbsPitch;
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

        ///// <summary>
        ///// 20 lists
        ///// </summary>
        //private static List<List<int>> linearPitchHierarchies = new List<List<int>>()
        //{
        //    new List<int>(){ 0,  7,  4, 10,  2,  5,  9, 11,  1,  3,  6,  8 }, //  0
        //    new List<int>(){ 0,  4,  7, 10,  2,  5,  9, 11,  1,  3,  6,  8 }, //  1
        //    new List<int>(){ 0,  4, 10,  7,  2,  5,  9, 11,  1,  3,  6,  8 }, //  2
        //    new List<int>(){ 0, 10,  4,  2,  7,  5,  9, 11,  1,  3,  6,  8 }, //  3
        //    new List<int>(){ 0, 10,  2,  4,  5,  7,  9, 11,  1,  3,  6,  8 }, //  4
        //    new List<int>(){ 0,  2, 10,  5,  4,  9,  7, 11,  1,  3,  6,  8 }, //  5
        //    new List<int>(){ 0,  2,  5, 10,  9,  4, 11,  7,  1,  3,  6,  8 }, //  6
        //    new List<int>(){ 0,  5,  2,  9, 10, 11,  4,  1,  7,  3,  6,  8 }, //  7
        //    new List<int>(){ 0,  5,  9,  2, 11, 10,  1,  4,  3,  7,  6,  8 }, //  8
        //    new List<int>(){ 0,  9,  5, 11,  2,  1, 10,  3,  4,  6,  7,  8 }, //  9
        //    new List<int>(){ 0,  9, 11,  5,  1,  2,  3, 10,  6,  4,  8,  7 }, // 10
        //    new List<int>(){ 0, 11,  9,  1,  5,  3,  2,  6, 10,  8,  4,  7 }, // 11
        //    new List<int>(){ 0, 11,  1,  9,  3,  5,  6,  2,  8, 10,  4,  7 }, // 12
        //    new List<int>(){ 0,  1, 11,  3,  9,  6,  5,  8,  2, 10,  4,  7 }, // 13
        //    new List<int>(){ 0,  1,  3, 11,  6,  9,  8,  5,  2, 10,  4,  7 }, // 14
        //    new List<int>(){ 0,  3,  1,  6, 11,  8,  9,  5,  2, 10,  4,  7 }, // 15
        //    new List<int>(){ 0,  3,  6,  1,  8, 11,  9,  5,  2, 10,  4,  7 }, // 16
        //    new List<int>(){ 0,  6,  3,  8,  1, 11,  9,  5,  2, 10,  4,  7 }, // 17
        //    new List<int>(){ 0,  6,  8,  3,  1, 11,  9,  5,  2, 10,  4,  7 }, // 18
        //    new List<int>(){ 0,  8,  6,  3,  1, 11,  9,  5,  2, 10,  4,  7 }, // 19
        //};

        /// <summary>
        /// 22 lists
        /// </summary>
        private static List<List<int>> circularPitchHierarchies = new List<List<int>>()
        {
            new List<int>(){ 0,  7,  4, 10,  2,  5,  9, 11,  1,  3,  6,  8 }, //  0
            new List<int>(){ 0,  4,  7,  2, 10,  9,  5,  1, 11,  6,  3,  8 }, //  1
            new List<int>(){ 0,  4,  2,  7,  9, 10,  1,  5,  6, 11,  8,  3 }, //  2
            new List<int>(){ 0,  2,  4,  9,  7,  1, 10,  6,  5,  8, 11,  3 }, //  3
            new List<int>(){ 0,  2,  9,  4,  1,  7,  6, 10,  8,  5,  3, 11 }, //  4
            new List<int>(){ 0,  9,  2,  1,  4,  6,  7,  8, 10,  3,  5, 11 }, //  5
            new List<int>(){ 0,  9,  1,  2,  6,  4,  8,  7,  3, 10, 11,  5 }, //  6
            new List<int>(){ 0,  1,  9,  6,  2,  8,  4,  3,  7, 11, 10,  5 }, //  7
            new List<int>(){ 0,  1,  6,  9,  8,  2,  3,  4, 11,  7,  5, 10 }, //  8
            new List<int>(){ 0,  6,  1,  8,  9,  3,  2, 11,  4,  5,  7, 10 }, //  9
            new List<int>(){ 0,  6,  8,  1,  3,  9, 11,  2,  5,  4, 10,  7 }, // 10
            new List<int>(){ 0,  8,  6,  3,  1, 11,  9,  5,  2, 10,  4,  7 }, // 11
            new List<int>(){ 0,  8,  3,  6, 11,  1,  5,  9, 10,  2,  7,  4 }, // 12
            new List<int>(){ 0,  3,  8, 11,  6,  5,  1, 10,  9,  7,  2,  4 }, // 13
            new List<int>(){ 0,  3, 11,  8,  5,  6, 10,  1,  7,  9,  4,  2 }, // 14
            new List<int>(){ 0, 11,  3,  5,  8, 10,  6,  7,  1,  4,  9,  2 }, // 15
            new List<int>(){ 0, 11,  5,  3, 10,  8,  7,  6,  4,  1,  2,  9 }, // 16
            new List<int>(){ 0,  5, 11, 10,  3,  7,  8,  4,  6,  2,  1,  9 }, // 17
            new List<int>(){ 0,  5, 10, 11,  7,  3,  4,  8,  2,  6,  9,  1 }, // 18
            new List<int>(){ 0, 10,  5,  7, 11,  4,  3,  2,  8,  9,  6,  1 }, // 19
            new List<int>(){ 0, 10,  7,  5,  4, 11,  2,  3,  9,  8,  1,  6 }, // 20
            new List<int>(){ 0,  7, 10,  4,  5,  2, 11,  9,  3,  1,  8,  6 }, // 21 
        };


        private List<List<double>> velocityFactors = VelocityFactorsPerPitch();

        /// <summary>
        /// Returns a list of lists of double containing the following values:
        ///      y=0   | y=1    | y=2    | y=3    | y=4    | y=5    | y=6    | y=7    | y=8    | y=9    | y=10   | y=11   | 
        /// x=0: 1     | 0,9797 | 0,9206 | 0,8274 | 0,7077 | 0,5712 | 0,4288 | 0,2923 | 0,1726 | 0,0794 | 0,0203 | 0      | 
        /// x=1: 1     | 0,9799 | 0,9213 | 0,8289 | 0,7101 | 0,5747 | 0,4336 | 0,2981 | 0,1794 | 0,0870 | 0,0284 | 0,0083 | 
        /// x=2: 1     | 0,9806 | 0,9238 | 0,8343 | 0,7194 | 0,5883 | 0,4517 | 0,3206 | 0,2057 | 0,1162 | 0,0594 | 0,04   | 
        /// x=3: 1     | 0,9820 | 0,9294 | 0,8466 | 0,7402 | 0,6188 | 0,4923 | 0,3709 | 0,2645 | 0,1817 | 0,1291 | 0,1111 | 
        /// x=4: 1     | 0,9848 | 0,9405 | 0,8706 | 0,7808 | 0,6784 | 0,5716 | 0,4692 | 0,3794 | 0,3095 | 0,2652 | 0,25   | 
        /// x=5: 1     | 0,9901 | 0,9611 | 0,9155 | 0,8568 | 0,7900 | 0,7202 | 0,6534 | 0,5947 | 0,5491 | 0,5201 | 0,5102 | 
        /// x=6: 1     | 1      | 1      | 1      | 1      | 1      | 1      | 1      | 1      | 1      | 1      | 1      |
        /// 
        /// The algorithm is like the calculation of eccentricity: It uses 7 radius coordinates at 0 degrees, in a semicircular
        /// linear field having 12 foci: the returned doubles are the squares of the distances from the radius coordinates to
        /// the foci, rounded to 4 decimal places. The values at y=0 are normalised to 1, by dividing the whole line by the initial value.
        /// </summary>
        private static List<List<double>> VelocityFactorsPerPitch()
        {
            List<List<double>> rval = new List<List<double>>();

            double rY = 0; // constant
            for(int rEccnt = 0; rEccnt < 7; ++rEccnt)
            {
                List<double> eList = new List<double>();
                rval.Add(eList);
                double rX = 1.0 - (rEccnt / 6.0);
                for(int focus = 0; focus < 12; ++focus)
                {
                    double alpha = (focus * Math.PI) / 11.0;
                    double focusX = Math.Cos(alpha);
                    double focusY = Math.Sin(alpha);
                    double squaredDistance = SquaredDistance(rX, rY, focusX, focusY);
                    eList.Insert(0, squaredDistance);
                }
            }

            //for(int eccnt = 0; eccnt < 7; ++eccnt)
            //{
            //    Console.Write("eccnt:" + eccnt.ToString() + " ");
            //    for(int v = 0; v < 12; ++v)
            //    {
            //        Console.Write(rval[eccnt][v].ToString() + " | ");
            //    }
            //    Console.WriteLine();
            //}

            // normalize rval[0..6][0] to 1.
            for(int x = 0; x < 7; ++x)
            {
                double v1 = rval[x][0];
                for(int v = 0; v < 12; ++v)
                {
                    rval[x][v] = Math.Round((rval[x][v] / v1), 4);
                }
            }

            //for(int eccnt = 0; eccnt < 7; ++eccnt)
            //{
            //    Console.Write("eccnt:" + eccnt.ToString() + " ");
            //    for(int v = 0; v < 12; ++v)
            //    {
            //        Console.Write(rval[eccnt][v].ToString() + " | ");
            //    }
            //    Console.WriteLine();
            //}

            return rval;
        }
        /// <summary>
        /// Returns the distance between the two points, to the power of two, as a double.
        /// </summary>
        /// <param name="x1">point1 x</param>
        /// <param name="y1">point1 y</param>
        /// <param name="x2">point2 x</param>
        /// <param name="y2">point2 y</param>
        private static double SquaredDistance(double x1, double y1, double x2, double y2)
        {
            double x = x2 - x1;
            double y = y2 - y1;
            double result = (x * x) + (y * y);
            return result;
        }

    }
}
