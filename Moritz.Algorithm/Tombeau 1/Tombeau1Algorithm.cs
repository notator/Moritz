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
			Think Nancarrow.
            Think Bach, Brückner, Reich. Repeating/changing, fairly fast groups of chords with harmonic support... and ornaments...
            Think Study 1: background/foreground, depth.

			The following parameters can be controlled using the Resident Sf2 Synth:
				Commands:	preset, pitchwheel
				Controls:	volume, pan, pitchWheelDeviation, allControllersOff, allSoundOff
				Note:		velocity, pitch.
			
			Tombeau1 is just going to use preset 0:0 (=grandPiano), so the available parameters are:
				"tuning" -- i.e. pitchWheelDeviation and pitchWheel
				pan,
				velocity,
				pitch

			** "Chords are colour" (Stockhausen)
            
            ** I want to keep the number of pitches in chords fairly low, so that they are recognisable.

            ** Using Seqs and Trks should make it possible to compose powerful, comprehensible, pregnant
			   relations between these	parameters at every	structural level. "Harmony & counterpoint" is
               not only going to be there in the pitches (chords, envelopes...), but also in the velocities
               and the pan positions... Tuning is a parameter that could be introduced as a surprise later...

			** Thinking about templates and the structural levels:
			   First create a set of Tombeau1Templates: Envelopes, Durations, MidiChordDefs, Trks etc. that can be
               used as the basis for similar objects in the piece. This includes MidiChordDefs created from pallets.
			   Trks in the piece may be derived fairly simply from templates, but they may also be influenced by
               their containing Seqs.
			   How do notes/chords relate horizontally inside a trk? How do they relate vertically inside a seq?
               How do seqs relate to each other globally...
			   Seqs can also be cloned, but I dont think there should be Seqs in Tombeau1Templates.

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

            ****************************************************************************/
            #endregion main comment (thoughts etc.)
            #region MoritzStatics functions
            /***************************************************************************
            public static Moritz.Statics functions relating to pitch hierarchies and velocities:
                GetAbsolutePitchHierarchy(int relativePitchHierarchyIndex, int rootPitch)
            ***************************************************************************/
            #endregion MoritzStatics functions
            #region Envelope functions
            /***************************************************************************
            public Envelope functions that have been implemented:
                constructors:
                Envelope(List<byte> inputValues, int inputDomain, int domain, int count)
                Envelope(List<int> inputValues, int inputDomain, int domain, int count)

                Clone()

                SetCount(int count)
                WarpVertically(int finalDomain)

                ValueList<T>(List<T> availableValues) // Uses the values in envelope.Original as indices in the availableValues list to create and return a list of values of type T.
                TimeWarp(List<int> originalMsPositions, double distortion)
                GetValuePerMsPosition(List<int> msPositions) // Returns a dictionary in which: Key is one of the positions in msPositions, Value is the envelope value at that msPosition.
            ***************************************************************************/
            #endregion Envelope functions
            #region Gamut functions
            /***************************************************************************
            public Gamut functions and properties that have been implemented:
                (Gamut is immutable)
                constructor:
                Gamut(List<int> absolutePitchHierarchy, int nPitchesPerOctave)

                Clone()
                Conjugate()

                Contains(byte pitch)
                ContainsAllPitches(List<byte> pitches)

                PitchSequence(int pitch1OctaveIndex, int pitch1pitchInOctaveIndex, Envelope envelope)
                GetChord(int rootPitch, int nPitches)

                GetVelocityPerAbsolutePitch(int minimumVelocity)

                Gamut[i] { get; }
                IndexOf(int pitch)
                NPitchesPerOctave { get; }
                AbsolutePitchHierarchy  { get; } // a copy of the private list.
                List {get;} // a copy of the private list.
                Count {get;}            
			***************************************************************************/
            #endregion Gamut functions
            #region MidiChordDef functions
            /***************************************************************************            
            public MidiChordDef functions that have been implemented and are especially relevant to this project:
                constructors:
                SIMPLE MidiChordDefs (containing a single BasicMidiChordDef):
                    MidiChordDef(List<byte> pitches, List<byte> velocities, int msDuration, bool hasChordOff)
                ORNAMENTS
                    MidiChordDef(int msDuration, Gamut gamut, int rootNotatedPitch, int nPitchesPerChord, Envelope ornamentEnvelope = null)    
                PALETTE MidiChordDefs (MidiChordDefs created from palettes):    
                    MidiChordDef(int msDuration, byte pitchWheelDeviation, bool hasChordOff, List<byte> rootMidiPitches, List<byte> rootMidiVelocities, int ornamentNumberSymbol, MidiChordSliderDefs midiChordSliderDefs, List<BasicMidiChordDef> basicMidiChordDefs)
                    
                Clone()
                Conjugate() // requires MidiChordDef.Gamut to be set.

                TimeWarp(Envelope envelope, double distortion) // Changes the msPositions of the BasicMidiChordDefs without changing the length of the MidiChordDef.
                MsDuration {get; set;} // set changes the durations of contained BasicMidiChordDefs
                AdjustMsDuration(double factor) // Multiplies the MsDuration by the given factor.

                SetVerticalDensity(int newDensity)
                GetNoteCombination(MidiChordDef mcd1, MidiChordDef mcd2, MidiChordPitchOperator midiChordPitchOperator) // a static function
                
                Invert(int nPitchesToShift) // shifts the lower pitches up one octave. The pitches stay in the Gamut, if any.
                Transpose(int interval) // sets Gamut to null
                Transpose(Gamut gamut, int steps)

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

                Gamut {get; set;} // The Gamut can only be set (or changed) if all the pitches in the MidiChordDef are in the new Gamut.
            ***************************************************************************/
            #endregion MidiChordDef functions
            #region Trk functions
            /***************************************************************************            
            public Trk functions that have been implemented and are especially relevant to this project:
            constructors:
                Trk(int midiChannel, int msPositionReContainer, List<IUniqueDef> iuds)
                Trk(int midiChannel)

                Clone()
 
                this[i] { get; set; } // indexer (set sets MsPositionsReFirstUD in whole Trk
                uniqueDefs // enumerator

                **** Changing the trk without changing its duration ****
                // clefs, lyrics
                InsertClefChange(int index, string clefType)
                SetLyricsToIndex()
                // alignment changers
                TimeWarp(Envelope envelope, double distortion)
                FindIndexAtMsPositionReFirstIUD(int msPositionReFirstIUD) // Returns the index of the IUniqueDef which starts at or is otherwise current at msPosition.
                AlignObjectAtIndex(int anchor1Index, int indexToAlign, int anchor2Index, int toMsPositionReFirstIUD)
                // sorting and permutation
                SortRootNotatedPitchAscending()
                SortRootNotatedPitchDescending()
                SortVelocityIncreasing()
                SortVelocityDecreasing()
                Permute(int axisNumber, int contourNumber) // permutes recursively
                PermutePartitions(int axisNumber, int contourNumber, List<int> partitionSizes)
                // rests
                Erase(int startMsPosition, int endMsPosition) // Replace all the IUniqueDefs from startMsPosition to (not including) endMsPosition by a single rest.
                InsertInRest(Trk trk) // An attempt is made to insert the argument trk in a rest in the host trk.
                InsertInRest(MidiChordDef midiChordDef) // Creates a new TrkDef the midiChordDef, then calls the other InsertInRest() function.
                // replace and translate
                Replace(int index, IUniqueDef replacementIUnique) // Removes the iUniqueDef at index from the list, and then inserts the replacement at the same index.
                Translate(int fromIndex, int nUniqueDefs, int toIndex) // Extracts nUniqueDefs from the uniqueDefs, and then inserts them again at the toIndex.
                // sliders
                AdjustExpression(int beginIndex, int endIndex, double factor)
                AdjustExpression(double factor)
                SetPanGliss(int startMsPosition, int endMsPosition, int startPanValue, int endPanValue)
                SetPitchWheelSliders(Envelope envelope)
                SetPitchWheelSliders(Dictionary<int, int> pitchWheelValuesPerMsPosition) // called by Seq and Block
                SetPitchWheelDeviation(int beginIndex, int endIndex, int deviation)
                RemoveScorePitchWheelCommands(int beginIndex, int endIndex)
                AdjustPitchWheelDeviations(int startMsPosition, int endMsPosition, int startPwd, int endPwd)
                // pitch
                Transpose(int beginIndex, int endIndex, int interval)
                Transpose(int interval)
                TransposeNotation(int semitonesToTranspose)
                TransposeToDict(Dictionary<int, int> msPosTranspositionDict)
                StepwiseGliss(int beginIndex, int endIndex, int glissInterval)
                // velocity
                AdjustVelocities(int beginIndex, int endIndex, double factor)
                AdjustVelocities(double factor)
                AdjustVelocitiesHairpin(int startMsPosition, int endMsPosition, double startFactor, double endFactor)
                SetVelocityPerAbsolutePitch(List<int> velocityPerAbsolutePitch)
                SetVerticalVelocityGradient(byte rootVelocity, byte topVelocity)

                **** Functions that change trk's duration ****
                CreateAccel(int beginIndex, int endIndex, double startEndRatio) // Creates an exponential accelerando or decelerando from beginIndex to (not including) endIndex.
                Add(IUniqueDef iUniqueDef) // Appends the new IUniqueDef to the end of the trk.
                AddRange(VoiceDef trk) // Adds the argument's UniqueDefs to the end of the trk.
                Insert(int index, IUniqueDef iUniqueDef) // Inserts the iUniqueDef in the list at the given index.
                InsertRange(int index, Trk trk) // Inserts the trk's UniqueDefs in the list at the given index.
                Remove(IUniqueDef iUniqueDef) // removes the iUniqueDef from the list.
                RemoveAt(int index) // Removes the iUniqueDef at index from the list.
                RemoveRange(int index, int count) // Removes count iUniqueDefs from the list, startíng with the iUniqueDef at index.
                RemoveBetweenMsPositions(int startMsPosReFirstIUD, int endMsPosReFirstIUD) // Remove the IUniqueDefs which start between startMsPosReFirstIUD and (not including) endMsPosReFirstIUD
                RemoveRests() // Removes all the rests in this Trk
                AdjustMsDurations(int beginIndex, int endIndex, double factor, int minThreshold = 100)
                AdjustMsDurations(double factor, int minThreshold = 100)
                AdjustChordMsDurations(int beginIndex, int endIndex, double factor, int minThreshold = 100)
                AdjustChordMsDurations(double factor, int minThreshold = 100)
                AdjustRestMsDurations(int beginIndex, int endIndex, double factor, int minThreshold = 100)
                AdjustRestMsDurations(double factor, int minThreshold = 100)

                **** Properties ****
                MidiChannel  { get; set; }
                Count { get; } // The number of IUniqueDefs in this TrkDef
                DurationsCount { get; } // The number of MidiChordDefs and RestDefs in this TrkDef
                MsDuration { get; set; } // set stretches or compresses each IUniqueDef to fit the new duration
                EndMsPositionReFirstIUD  { get; set; }  // set changes the msDuration of the last IUniqueDef in the trk.
                MsPositionReContainer  { get; set; } // The trk's MsPosition wrt the containing Seq.
                AxisIndex { get; set; } // The index of the UniqueDef (in the UniqueDefs list) that will be aligned when calling Seq.AlignTrkAxes().
                MasterVolume // Algorithms must set this value in the first Trk in each midiChannel in the score. (Default is null.)

            ***************************************************************************/
            #endregion Trk functions
            #region Seq functions
            /***************************************************************************            
            public Seq functions that have been implemented and are especially relevant to this project:
                constructors:
                Seq(int absSeqMsPosition, List<Trk> trks, IReadOnlyList<int> midiChannelIndexPerOutputVoice)
                Seq(int absSeqMsPosition, IReadOnlyList<int> midiChannelIndexPerOutputVoice)

                Clone()

                Concat(Seq seq2) // Concatenates seq2 to the caller (seq1). Returns a pointer to the caller.

                SetTrk(Trk trk) // replaces or updates the trk having the same channel in the Seg.

                Normalize() // Shifts the Trks so that the earliest trk.MsPositionReContainer is 0.

                // Sorting (These functions smply call the coresponding function on each trk.)
                SortVelocityIncreasing()
                SortVelocityDecreasing()
                SortRootNotatedPitchAscending()
                SortRootNotatedPitchDescending()

                AlignTrkUniqueDefs(List<int> indicesToAlign) // Aligns the Trk UniqueDefs whose indices are given in the argument.
                AlignTrkAxes()// Aligns using each trk.AxisIndex.

                ShiftTrks(List<int> msShifts) // Shifts each track by adding the corresponding millisecond argument to its MsPositionReContainer attribute.

                // Envelopes
                TimeWarp(Envelope envelope, double distortion)
                SetPitchWheelSliders(Envelope envelope)

                // Properties
                Trks { get; }
                AbsMsPosition { get; set; }
                MsDuration { get; set; } // Setting this value stretches or compresses the msDurations of all the trks and their contained UniqueDefs.
                MidiChannelIndexPerOutputVoice { get; }

            ***************************************************************************/
            #endregion Seq functions
            #region Block functions
            /***************************************************************************            
            public Block functions that have been implemented and are especially relevant to this project:
                constructors:
                Block(Seq seq, List<int> rightBarlineMsPositions)
                Block(List<Block> blocks)

                Clone()

                Concat(Block block2)

                ConvertToBars()

                AddInputVoice(InputVoiceDef ivd)

                SetInitialClefs(List<string> clefs)

                // Envelopes
                TimeWarp(Envelope envelope, double distortion)
                SetPitchWheelSliders(Envelope envelope)

                // Properties
                MsDuration { get; set; } // Setting this value stretches or compresses the msDurations of all the voiceDefs and their contained UniqueDefs.
                AbsMsPosition { get; set; }
                BarlineMsPositions // public List<int>
                Trks { get; } 
                InputVoiceDefs { get; }

            ***************************************************************************/
            #endregion Block functions

            /**********************************************/

            #region initialization
            Palette palette = GetPaletteByName("Tombeau1.1");
            // more palletes could be loaded here
            Tombeau1Templates tombeau1Templates = new Tombeau1Templates(new List<Palette>() { palette });
            #endregion initialization

            List<Block> blockList = new List<Block>();

            Block startBlock = new StartBlock(tombeau1Templates.Trks, MidiChannelIndexPerOutputVoice);
            blockList.Add(startBlock); // 2 bars

            Block target1Block = new Target1Block(tombeau1Templates.Trks, MidiChannelIndexPerOutputVoice);
            blockList.Add(target1Block); // 2 bars

            /************************************************/
            Block vpapBlock = VPAPBlock(tombeau1Templates.Trks);
            blockList.Add(vpapBlock);   // 2 bars

            /************************************************/
            #region test blocks
            Block block2TestBlock = Block2TestBlock(tombeau1Templates.PitchWheelTestMidiChordDefs, tombeau1Templates.OrnamentTestMidiChordDefs);
            blockList.Add(block2TestBlock);   // 2 bars

            Block verticalVelocityColorsTestBlock = VerticalVelocityColorsTestBlock(tombeau1Templates.PaletteMidiChordDefs[0]);
            blockList.Add(verticalVelocityColorsTestBlock); // 2 bars

            Block velocityPerAbsolutePitchTestBlock = VelocityPerAbsolutePitchTestBlock();
            blockList.Add(velocityPerAbsolutePitchTestBlock); // 2 bars

            Block gamutTestBlock = GamutTestBlock();
            blockList.Add(gamutTestBlock); // 2 bars

            Block verticalVelocityGradientTestBlock = VerticalVelocityGradientTestBlock();
            blockList.Add(verticalVelocityGradientTestBlock); // 2 bars

            Block timeWarpVVTestBlock = TimeWarpTestBlock(verticalVelocityGradientTestBlock);
            blockList.Add(timeWarpVVTestBlock); // 4 bars (1 system)

            Block trksTestBlock = TrksTestBlock(tombeau1Templates.PaletteMidiChordDefs[0]);
            blockList.Add(trksTestBlock); // 1 bar (1 system)

            Block simpleVelocityColorsTestBlock = SimpleVelocityColorsTestBlock();
            blockList.Add(simpleVelocityColorsTestBlock); // 1 bar

            //Block timeWarpSVTestBlock = TimeWarpTestBlock(simpleVelocityColorsTestBlock);
            //blockList.Add(timeWarpSVTestBlock); // 1 bar

            #endregion test blocks

            MainBlock mainBlock = new MainBlock(InitialClefPerChannel, blockList);

            List<List<VoiceDef>> bars = mainBlock.ConvertToBars();

            return bars;
		}
    }
}
