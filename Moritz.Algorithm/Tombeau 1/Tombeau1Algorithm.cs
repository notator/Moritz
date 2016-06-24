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
		public override int NumberOfBars { get { return 27; } }

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
            
            *** 24.06.16 public MidiChordDef functions that have been implemented and are especially relevant to this project:
                constructors:
                SIMPLE MidiChordDefs (containing a single BasicMidiChordDef):
                    MidiChordDef(List<byte> pitches, List<byte> velocities, int msDuration, bool hasChordOff)
                    MidiChordDef(int nPitches, int rootPitch, List<int> absolutePitchHierarchy, int velocity, int msDuration, bool hasChordOff)
                ORNAMENTS having msDuration, and single-note BasicMidiChordDefs:    
                    MidiChordDef(int msDuration, List<int> basicMidiChordRootPitches)
                PALETTE MidiChordDefs (MidiChordDefs created from palettes):    
                    MidiChordDef(int msDuration, byte pitchWheelDeviation, bool hasChordOff, List<byte> rootMidiPitches, List<byte> rootMidiVelocities, int ornamentNumberSymbol, MidiChordSliderDefs midiChordSliderDefs, List<BasicMidiChordDef> basicMidiChordDefs)
                    
                Clone()
                Inversion()

                TimeWarp(Envelope envelope, double distortion) // Changes the msPositions of the BasicMidiChordDefs without changing the length of the MidiChordDef.
                MsDuration {get; set;} // set changes the durations of contained BasicMidiChordDefs
                AdjustMsDuration(double factor) // Multiplies the MsDuration by the given factor.

                SetVerticalDensity(int newDensity)
                GetNoteCombination(MidiChordDef mcd1, MidiChordDef mcd2, MidiChordPitchOperator midiChordPitchOperator) // a static function
                
                Transpose(int interval)

                SetVelocityPerAbsolutePitch(List<int> velocityPerAbsolutePitch)
                SetVerticalVelocityGradient(byte rootVelocity, byte topVelocity)
                AdjustVelocities(double factor) // Multiplies the velocities in NotatedMidiVelocities, and all BasicMidiChordDef.Velocities by the argument factor. 

                SetPitchWheelEnvelope(Envelope envelope)
                SetPanEnvelope(Envelope envelope)
                SetModulationWheelEnvelope(Envelope envelope)
                SetExpressionEnvelope(Envelope envelope)
                AdjustPitchWheel(double factor)
                PanMsbs {get; set;}
                AdjustModulationWheel(double factor)
                AdjustExpression(double factor)
            
            *** 24.06.16 public Envelope functions that have been implemented:
                consructors:
                Envelope(List<byte> inputValues, int inputDomain, int domain, int count)
                Envelope(List<int> inputValues, int inputDomain, int domain, int count)

                Clone()

                SetCount(int count)
                WarpVertically(int finalDomain)

                ValueList<T>(List<T> availableValues) // Uses the values in envelope.Original as indices in the availableValues list to create and return a list of values of type T.
                TimeWarp(List<int> originalMsPositions, double distortion)
                GetValuePerMsPosition(List<int> msPositions) // Returns a dictionary in which: Key is one of the positions in msPositions, Value is the envelope value at that msPosition.
                PitchSequence(int firstPitch, Gamut gamut)

            *** 24.06.16 public Gamut functions that have been implemented:
                constructors:
                Gamut(int indexInRelativePitchHierarchiesList, int nPitchesPerOctave, int basePitch)
                Gamut(List<int> absolutePitchHierarchy, int nPitchesPerOctave)

                Clone()

                AddOctaves(int pitchArg)
                RemoveOctaves(int pitchArg)

                IndexOf(int pitch)
                List {get;} // a copy of the private list.
                Count {get;}


            ** Draw a map of the chords that are to be used, showing the way they relate to each other.
               Use this drawing to create a data structure, containing the chords, that can be used when composing.
               Positions in this structure can probably be assigned to krystal values somehow...

            ** The following functions could be written:
               -- MidiChordDef.Gliss(toMidiChordDef)
                  This would create both a pitchwheel gliss from the pitchwheel setting of the original chord to the pitchwheel
                  setting of the argument chord, and a transformation in the basicMidiChordDefs to the first basicMidiChordDef
                  in the toMidiChordDef.
               -- List<MidiChordDef> MidiChordDef.ToMidiChordDefList()
                  Returns the midiChordDef's basicMidiChordDefs as a list of simple midiChordDefs. The msDurations of the
                  returned midiChordDefs are those of the original basicMidiChorddefs, so the msDuration of the list is the
                  msDuration of the original midiChordDef. The original midiChordDef can therefore be replaced by the list in
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

            Block initBlock = Init();                                   
            initBlock.SetInitialClefs(new List<string>() { "t", "t", "t", "t", "t", "t", "t", "t" });
            blocks.Add(initBlock);

            #region test blocks
            Block velocityPerAbsolutePitchTestBlock = VelocityPerAbsolutePitchTestBlock();
            blocks.Add(velocityPerAbsolutePitchTestBlock); // 2 bars (2 systems)

            Block gamutTestBlock = GamutTestBlock();
            blocks.Add(gamutTestBlock); // 2 bars (2 systems)

            Block verticalVelocityGradientTestBlock = VerticalVelocityGradientTestBlock();
            blocks.Add(verticalVelocityGradientTestBlock); // 2 bars (2 systems)

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
