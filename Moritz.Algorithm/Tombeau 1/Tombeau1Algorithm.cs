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
		public override int NumberOfBars { get { return 25; } }

        List<MidiChordDef> majorPalette = null;
        List<MidiChordDef> minorPalette = null;

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

			//Thinking about the structural levels:
			//> create the palettes containing the channel objects (atoms, MidiChordDefs): notes/chords, ornaments,
			//> structural layers between the objects in the palettes and trks (or are trks constructed from
			//  vertical relations inside seqs?)
			//> how notes/chords relate horizontally inside a trk
			//> how notes/chords relate vertically inside a seq (How trks relate to each other inside seqs?),
			//> and about how seqs relate to each other globally...

			Seqs can be superimposed, juxtaposed, repeated (with/without variation) and re-ordered.
			
			Chords:
			** "Chords are colour" (Stockhausen)
            
            ** I want to keep the number of pitches in chords fairly low, so that they are recognisable.
            
            ** ROOT PITCHES of BasicMidiChordDefs and MidiChordDefs can be constructed using appropriate constructors in
               the BasicMidiChordDef, MidiChordDef and Trk classes. These constructors are flexible enough to define tremolos.
            ** ADDITIONAL PITCHES TODO: write functions in the BasicMidiChordDef, MidiChordDef and Trk classes for adding
               further pitches to one-note chords.
            ** NOTE VELOCITIES can be controlled independently to emphasise "harmonic" relations in areas of the score.
               Repeated chord sequences can be thus "filtered" differently as in a digital painting.
               There are currently two functions for setting velocities.
               Both of these have been implemented in the BasicMidiChordDef, MidiChordDef and Trk classes:
               -- SetVelocityPerAbsolutePitch(velocityPerAbsolutePitch):
                  The argument for SetVelocityPerAbsolutePitch(velocityPerAbsolutePitch) is created by calling
                  the static function GetVelocityPerAbsolutePitch(...), which is defined and documented in Moritz.Statics.
               -- SetVerticalVelocityGradient(rootVelocity, topVelocity):
                  The arguments are both in range [1..127].
                  The velocities of the root and top notes in the chord are set to the argument values. The other velocities
                  are interpolated linearly. (((double)topVelocity) / rootvelocity ) is the verticalVelocityFactor. 
            
            ** Draw a map of the chords that are to be used, showing the way they relate to each other.
               Use this drawing to create a data structure, containing the chords, that can be used when composing.
               Positions in this structure can probably be assigned to krystal values somehow...

            ** TODO: Write the following functions:
               -- MidiChordDef.Gliss(toMidiChordDef)
                  This would create both a pitchwheel gliss from the pitchwheel setting of the original chord to the pitchwheel
                  setting of the argument chord, and a transformation in the basicMidiChordDefs to the first basicMidiChordDef
                  in the toMidiChordDef.
               -- List<MidiChordDef> MidiChordDef.ToMidiChordDefList()
                  Returns the midiChordDef's basicMidiChordDefs as a list of simple midiChordDefs. The msDurations of the
                  returned midiChordDefs are those of the original basicMidiChorddefs, so the msDuration of the list is the
                  msDuration of the original midiChordDef. The original midiChordDef can therefore be replaced by list in
                  a host Trk.  
                        
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

            ** The concept of InputChords is still a bit hazy. Why should they ever have more than one notehead?...
               But I can compose Tombeau 1(that doesn't have inputChords) before worrying about that...
               Also, *annotations* that are instructions to performers (hairpins spring to mind). Conventional dynamic symbols are,
               I think meant for *performers*, so should only be attached to inputChords...
               The info in *outputChords* is the info that could go in a MIDI file, and vice versa.
             
			*********************************************************************************************/
            #endregion main comments
            /**********************************************/

            List<Block> blocks = new List<Block>();

            int chordDensity = 5;
            majorPalette = GetMajorPalette(chordDensity);
            minorPalette = UpsideDownChords(majorPalette, chordDensity);

            Block initBlock = Init();                                   
            initBlock.SetInitialClefs(new List<string>() { "t", "t", "t", "t", "t", "t", "t", "t" });
            blocks.Add(initBlock);

            #region test blocks
            Block velocityPerAbsolutePitchTestBlock = VelocityPerAbsolutePitchTestBlock(majorPalette, minorPalette);
            blocks.Add(velocityPerAbsolutePitchTestBlock); // 2 bars (2 systems)

            Block harmonicVelocityChordsTestBlock = HarmonicVelocityChordsTestBlock(majorPalette, minorPalette);
            blocks.Add(harmonicVelocityChordsTestBlock); // 2 bars (2 systems)

            Block triadsCycleBlock = TriadsCycleBlock();
            blocks.Add(triadsCycleBlock); // 4 bars (1 system)

            Block timeWarpTriadsTestBlock = TimeWarpTestBlock(triadsCycleBlock);
            blocks.Add(timeWarpTriadsTestBlock); // 4 bars (1 system)

            Block verticalVelocityColorsTestBlock = VerticalVelocityColorsTestBlock();
            blocks.Add(verticalVelocityColorsTestBlock); // 4 bars (1 system)

            Block timeWarpVVTestBlock = TimeWarpTestBlock(verticalVelocityColorsTestBlock);
            blocks.Add(timeWarpVVTestBlock); // 4 bars (1 system)

            Block trksTestBlock = TrksTestBlock();
            blocks.Add(trksTestBlock); // 1 bar (1 system)

            Block simpleVelocityColorsTestBlock = SimpleVelocityColorsTestBlock();
            blocks.Add(simpleVelocityColorsTestBlock); // 1 bar (1 system)

            Block timeWarpSVTestBlock = TimeWarpTestBlock(simpleVelocityColorsTestBlock);
            blocks.Add(timeWarpSVTestBlock); // 1 bar (1 system)

            #endregion test blocks

            Block block = GetCompleteSequence(blocks);
            List<List<VoiceDef>> bars = block.ConvertToBars();

            return bars;
		}

        #region functions called from this file or more than one other file

        private List<MidiChordDef> GetMajorPalette(int chordDensity)
        {
            List<List<int>> absolutePitchHierarchies = new List<List<int>>();
            int phIndex = 0;
            int rootPitch = 0;
            while(true)
            {
                try
                {
                    absolutePitchHierarchies.Add(M.GetAbsolutePitchHierarchy(phIndex++, rootPitch));
                }
                catch
                {
                    break;
                }     
            }
            List<MidiChordDef> majorCircularPalette = new List<MidiChordDef>();
            for(int j = 0; j < 24; ++j)
            {
                List<int> absolutePitchHierarchy = absolutePitchHierarchies[0];

                MidiChordDef mcd = new MidiChordDef(chordDensity, 60 + j, absolutePitchHierarchy, 127, 1200, true);

                mcd.Lyric = (j).ToString();
                majorCircularPalette.Add(mcd);
            }
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

        #endregion functions called from this file or more than one other file
    }
}
