using System;
using System.Collections.Generic;
using System.Diagnostics;

using Krystals4ObjectLibrary;

using Moritz.Globals;
using Moritz.Palettes;
using Moritz.Spec;

namespace Moritz.Algorithm.Tombeau1
{
	public partial class Tombeau1Algorithm : CompositionAlgorithm
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
		public override int NumberOfBars { get { return 22; } }

        List<MidiChordDef> majorCircularPalette = null;
        List<MidiChordDef> minorCircularPalette = null;

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

			Thinking about the structural levels:
			> create the palettes containing the channel objects (atoms, MidiChordDefs): notes/chords, ornaments,
			> structural layers between the objects in the palettes and trks (or are trks constructed from
			  vertical relations inside seqs?)
			> how notes/chords relate horizontally inside a trk
			> how notes/chords relate vertically inside a seq (How trks relate to each other inside seqs?),
			> and about how seqs relate to each other globally...

			Seqs can be superimposed, juxtaposed, repeated (with/without variation) and re-ordered.
			
			Chords:
			** "Chords are colour" (Stockhausen)
            
            ** I want to keep the number of pitches in chords fairly low, so that they are recognisable.
            
            ** Chords can have any pitches (construct some interesting palettes with some differently coloured chords).
               But velocities can be controlled independently to emphasise "harmonic" relations in areas of the score.
               Repeated chord sequences can be thus "filtered" differently as in a digital painting.
               See MidiChordDef.SetVelocityPerAbsolutePitch(velocityPerAbsolutePitch).
            
            ** For Trk construction, think "broken chords". Like ornaments, but on a larger scale. Sequences of chords
               whose roots are the pitches of a base chord...
            
            ** Draw a map of the chords that are to be used, showing the way they relate to each other.
               Use this drawing to create a data structure, containing the chords, that can be used when composing.
               Positions in this structure can probably be assigned to krystal values somehow...

            ** Write the following functions:
                   1. midiChordDef = fromMidiChordDef.Gliss(toMidiChordDef);
                   2. midiChordDef = startMidiChordDef.Tremolo(auxillaryMidiChordDef, nBasicMidiChordDefs);
               These could be used to characterise particular Blocks... 
            
            ** Think about creating a Seq (or Seqs) with a particular palette, then using exactly the same code but with
               a different palette to create further Seq(s)...
                        
            ** Representing velocity using coloured noteheads and extenders:
               There is a new pop-up menu in the Assistant Composer's main form for setting the output chord symbol type.
               This has two values: "standard black" and "coloured velocities". This value is saved in .mkss files.
               (Input chord symbols have no midi velocity, so their noteheads and extenders are never coloured anything but black.) 
               If "coloured velocities" is selected, (output) noteheads and extenders are coloured according to the velocities
               constructed either from the density and vertical velocity parameters in a palette or by passing velocities directly
               to a MidiChordDef constructor. The densities and velocities of BasicChordDefs depend on ornament definitions,
               so are independent of the notated values.
               The colours themselves are defined in MoritzStatics.cs:
                   public static readonly Dictionary<M.Dynamic, string> NoteheadColors

            ** Two functions have been written for setting the velocities of individual notes in a MidiChordDef.
               Both these functions affect the velocities of both the notated pitches and the BasicMidiChords:
               1. SetVerticalVelocityGradient(rootVelocity, topVelocity):
                  The arguments are both in range [1..127].
                  The velocities of the root and top notes in the chord are set to the argument values. The other velocities
                  are interpolated linearly. (((double)topVelocity) / rootvelocity ) is the verticalVelocityFactor.  
               2. SetVelocityPerAbsolutePitch(velocityPerAbsolutePitch):
                  this function sets note velocities according to the position of the note's pitch in a (harmonic) hierarchy of
                  *absolute pitches* (not intervals):
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
                      /// velocityFactorPerPitch is a list of 12 values in descending order, each value in range 1..0.
                      /// This list is the same length as the relativePitchHierarchy (12 values), and contains a set
                      /// of values that are always in descending order. The values are factors by which to multiply
                      /// the baseVelocity for each of the corresponding pitches in the relativePitchHierarchy.
                      /// The factors are standard values taken from a field calculated using a static function:
                      ///              VelocityFactorsPerPitch() (see below).
                      private List<int> GetVelocityPerAbsolutePitch
                          (
                              int basePitch,// The base pitch for the pitch hierarchy.
                              int baseVelocity,   // the velocity given to any absolute base pitch in the MidiChordDef
                              List<int> relativePitchHierarchy, // one of the circularPitchHierarchies,
                              List<double> velocityFactorPerPitch // (see below) 
                          )
                  and    
                      private void MidiChordDef.SetVelocityPerAbsolutePitch(List<int> velocityPerAbsolutePitch) 
                                           
                  The result of GetVelocityPerAbsolutePitch(...) can be passed to many different chords using SetVelocityPerAbsolutePitch(...).

            ** The concept of InputChords is still a bit hazy. Why should they ever have more than one notehead?...
               But I can compose Tombeau 1(that doesn't have inputChords) before worrying about that...
               Also, *annotations* that are instructions to performers (hairpins spring to mind). Conventional dynamic symbols are,
               I think meant for *performers*, so should only be attached to inputChords...
               The info in *outputChords* is the info that could go in a MIDI file, and vice versa.
             
			*********************************************************************************************/
            #endregion main comments
            /**********************************************/

            // Each Tuple is a Block and a list of barline positions re the start of
            // the Block (not including the barline at msPosition == 0).
            List<Block> blocks = new List<Block>();

            #region test blocks
            //int chordDensity = 3;
            //majorCircularPalette = GetMajorCircularPalette(chordDensity);
            //minorCircularPalette = UpsideDownChords(majorCircularPalette, chordDensity);

            //blocks.Add(VerticalVelocityChordsTestBlock()); // 2 bars (2 systems)
            //blocks.Add(HarmonicVelocityChordsTestBlock()); // 2 bars (2 systems)
            //blocks.Add(TriadsCycleBlock()); // 4 bars (1 system)
            //blocks.Add(WarpDurationsTestBlock(blocks[2])); // 4 bars (1 system)
            //blocks.Add(VerticalVelocityColorsTestBlock()); // 4 bars (1 system)
            //blocks.Add(WarpDurationsTestBlock(blocks[4])); // 4 bars (1 system)
            //blocks.Add(TrksTestBlock()); // 1 bar (1 system)
            //blocks.Add(SimpleVelocityColorsTestBlock()); // 1 bar (1 system)
            //blocks.Add(WarpDurationsTestBlock(blocks[7])); // 1 bar (1 system)

            #endregion test blocks

            blocks.Add(PalettesTestBlock("exStudy3p", 1, 8));
            blocks.Add(PalettesTestBlock("exSongSix", 1, 8));
            blocks.Add(PalettesTestBlock("exSongSix", 9, 16));
            blocks.Add(PalettesTestBlock("exSongSix", 17, 21));

            Block tombeau1PalettesBlock = PalettesTestBlock("tombeau1p", 1, 2);
            tombeau1PalettesBlock.BarlineMsPositions = GetBarlinePositions(tombeau1PalettesBlock.Trks[1]);

            blocks.Add(tombeau1PalettesBlock);

            Block block = GetCompleteSequence(blocks);
            List<List<VoiceDef>> bars = block.ConvertToBars();

            // Add clef changes here.
            // Testing... 
            //bars[0][0].InsertClefChange(9, "t");
            //bars[0][0].InsertClefChange(2, "b");

            //bars[1][1].InsertClefChange(3, "b");

             return bars;
		}

        private List<int> GetBarlinePositions(Trk trk)
        {
            List<int> barlinePositions = new List<int>();
            int bPos = 0;
            foreach(IUniqueDef iud in trk.UniqueDefs)
            {
                bPos += iud.MsDuration;
                barlinePositions.Add(bPos);
            }
            return barlinePositions;
        }

        #region functions called from this file or more than one other file

        private List<MidiChordDef> GetMajorPalette(List<List<int>> pitchHierarchies, int chordDensity)
        {
            List<MidiChordDef> majorCircularPalette = new List<MidiChordDef>();
            for(int j = 0; j < pitchHierarchies.Count; ++j)
            {
                List<int> cph = pitchHierarchies[j];

                MidiChordDef mcd = new MidiChordDef(chordDensity, 5, cph, 127, 1200, true);
                mcd.Lyric = (j).ToString();
                majorCircularPalette.Add(mcd);
            }
            return majorCircularPalette;
        }

        private List<MidiChordDef> GetMajorCircularPalette(int chordDensity)
        {
            List<MidiChordDef> majorCircularPalette = GetMajorPalette(circularPitchHierarchies, chordDensity);
            return majorCircularPalette;
        }

        /// <summary>
        /// Returns a Block that is the concatenation of the argument blocks.
        /// This function consumes its arguments.
        /// </summary>
        private Block GetCompleteSequence(List<Block> blocks)
        {
            Block returnBlock = blocks[0];
            for(int i = 1; i < blocks.Count; ++i)
            {
                returnBlock.Concat(blocks[i]);
            }
            return returnBlock;
        }

        /// <summary>
        /// A list of the MidiChordDefs defined in the palette.
        /// </summary>
        private List<IUniqueDef> PaletteMidiChordDefs(Palette palette)
        {
            List<IUniqueDef> iuds = new List<IUniqueDef>();
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
 
        /// <summary>
        /// The returned List contains values in range [1..127]
        /// </summary>
        /// <param name="basePitch">In range [0..127]</param>
        /// <param name="baseVelocity">In range [1..127]</param>
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
            Debug.Assert(baseVelocity >= 1 && baseVelocity <= 127);
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
                velocity = (velocity == 0) ? 1 : velocity;

                velocityPerAbsPitch.Add(velocity);
            }
            return velocityPerAbsPitch;
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

        /// <summary>
        /// The maximum velocity for each dynamic symbol (ppp-fff)
        /// </summary>
        private static List<byte> dynamicSymbolsMaximumVelocities = new List<byte>()
                {
                    M.MaxMidiVelocity[M.Dynamic.ppp],
                    M.MaxMidiVelocity[M.Dynamic.pp],
                    M.MaxMidiVelocity[M.Dynamic.p],
                    M.MaxMidiVelocity[M.Dynamic.mp],
                    M.MaxMidiVelocity[M.Dynamic.mf],
                    M.MaxMidiVelocity[M.Dynamic.f],
                    M.MaxMidiVelocity[M.Dynamic.ff],
                    M.MaxMidiVelocity[M.Dynamic.fff]
                };

        private List<List<double>> velocityFactors = VelocityFactorsPerPitch();

        /// <summary>
        /// Returns a list of lists of double containing the following values (rounded to 5 decimal places):
        ///      y=0 | y=1     | y=2     | y=3     | y=4     | y=5     | y=6     | y=7     | y=8     | y=9     | y=10    | y=11    | 
        /// x=0: 1   | 0,92985 | 0,8597  | 0,78955 | 0,7194  | 0,64925 | 0,5791  | 0,50895 | 0,4388  | 0,36865 | 0,2985  | 0,22835 | 
        /// x=1: 1   | 0,93987 | 0,87974 | 0,81961 | 0,75948 | 0,69936 | 0,63923 | 0,5791  | 0,51897 | 0,45884 | 0,39871 | 0,33858 | 
        /// x=2: 1   | 0,94989 | 0,89979 | 0,84968 | 0,79957 | 0,74946 | 0,69936 | 0,64925 | 0,59914 | 0,54903 | 0,49893 | 0,44882 | 
        /// x=3: 1   | 0,95991 | 0,91983 | 0,87974 | 0,83966 | 0,79957 | 0,75948 | 0,7194  | 0,67931 | 0,63923 | 0,59914 | 0,55906 | 
        /// x=4: 1   | 0,96994 | 0,93987 | 0,90981 | 0,87974 | 0,84968 | 0,81961 | 0,78955 | 0,75948 | 0,72942 | 0,69936 | 0,66929 | 
        /// x=5: 1   | 0,97996 | 0,95991 | 0,93987 | 0,91983 | 0,89979 | 0,87974 | 0,8597  | 0,83966 | 0,81961 | 0,79957 | 0,77953 | 
        /// x=5: 1   | 0,98998 | 0,97996 | 0,96994 | 0,95991 | 0,94989 | 0,93987 | 0,92985 | 0,91983 | 0,90981 | 0,89979 | 0,88976 | 
        /// x=7: 1   | 1       | 1       | 1       | 1       | 1       | 1       | 1       | 1       | 1       | 1       | 1       | 
        /// </summary>
        private static List<List<double>> VelocityFactorsPerPitch()
        {
            List<List<double>> rval = new List<List<double>>();

            for(int x = 0; x < dynamicSymbolsMaximumVelocities.Count; ++x)
            {
                double minVelocity = dynamicSymbolsMaximumVelocities[x];
                List<double> factors = new List<double>();
                double decrement = (127 - minVelocity) / 11;
                double currentVelocity = 127;
                for(int y = 0; y < 12; ++y)
                {
                    factors.Add(currentVelocity / 127);
                    currentVelocity -= decrement;
                }
                rval.Add(factors);
            }

            //for(int x = 0; x < dynamicSymbolsMaximumVelocities.Count; ++x)
            //{
            //    Console.Write("x:" + x.ToString() + " ");
            //    for(int v = 0; v < 12; ++v)
            //    {
            //        double val = Math.Round(rval[x][v], 5);
            //        Console.Write(val.ToString() + " | ");
            //    }
            //    Console.WriteLine();
            //}

            return rval;
        }

        #endregion functions called from this file or more than one other file
    }
}
