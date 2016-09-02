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

            Init(new List<string>() { "Tombeau1.1" });

            List<Block> blockList = new List<Block>(); // List to which new Blocks are added as Tombeau1 is being constructed.

            AddType1Block(blockList, 13000, 0, 1500);
            AddType1Block(blockList, 13000, 1, 1600);

            /************************************************/
            Block vpapBlock = VPAPBlock(Type1TemplateTrks[1]);
            blockList.Add(vpapBlock);   // 2 bars

            /************************************************/
            #region test blocks
            //Block block2TestBlock = Block2TestBlock(PitchWheelTestMidiChordDefs, OrnamentTestMidiChordDefs);
            //blockList.Add(block2TestBlock);   // 2 bars

            //Block verticalVelocityColorsTestBlock = VerticalVelocityColorsTestBlock(PaletteMidiChordDefs[0]);
            //blockList.Add(verticalVelocityColorsTestBlock); // 2 bars

            //Block velocityPerAbsolutePitchTestBlock = VelocityPerAbsolutePitchTestBlock();
            //blockList.Add(velocityPerAbsolutePitchTestBlock); // 2 bars

            //Block gamutTestBlock = GamutTestBlock();
            //blockList.Add(gamutTestBlock); // 2 bars

            //Block verticalVelocityGradientTestBlock = VerticalVelocityGradientTestBlock();
            //blockList.Add(verticalVelocityGradientTestBlock); // 2 bars

            //Block timeWarpVVTestBlock = TimeWarpTestBlock(verticalVelocityGradientTestBlock);
            //blockList.Add(timeWarpVVTestBlock); // 4 bars (1 system)

            //Block trksTestBlock = TrksTestBlock(PaletteMidiChordDefs[0]);
            //blockList.Add(trksTestBlock); // 1 bar (1 system)

            //Block simpleVelocityColorsTestBlock = SimpleVelocityColorsTestBlock();
            //blockList.Add(simpleVelocityColorsTestBlock); // 1 bar

            //Block timeWarpSVTestBlock = TimeWarpTestBlock(simpleVelocityColorsTestBlock);
            //blockList.Add(timeWarpSVTestBlock); // 1 bar

            #endregion test blocks

            MainBlock mainBlock = new MainBlock(InitialClefPerChannel, blockList);

            List<List<VoiceDef>> bars = mainBlock.ConvertToBars();

            return bars;
		}

        #region Init()
        /// <summary>
        /// Sets up the standard templates that will be used in the composition.
        /// </summary>
        private void Init(List<string> paletteNames)
        {
            List<Palette> palettes = new List<Palette>();
            foreach(string paletteName in paletteNames)
            {
                Palette palette = GetPaletteByName(paletteName);
                palettes.Add(palette);
            }

            SetPaletteMidiChordDefs(palettes);
            SetPitchWheelTestMidiChordDefs();
            SetOrnamentTestMidiChordDefs();

            SetType1TemplateTrks();
            // maybe define other template trk types, and add them here.
        }

        #region Init() helper functions
        private void SetPaletteMidiChordDefs(List<Palette> paletteList)
        {
            List<List<MidiChordDef>> paletteMidiChordDefs = new List<List<MidiChordDef>>();
            foreach(Palette palette in paletteList)
            {
                paletteMidiChordDefs.Add(GetPaletteMidiChordDefs(palette));
            }

            PaletteMidiChordDefs = paletteMidiChordDefs;
        }
        private List<MidiChordDef> GetPaletteMidiChordDefs(Palette palette)
        {
            List<MidiChordDef> midiChordDefs = new List<MidiChordDef>();
            for(int i = 0; i < palette.Count; ++i)
            {
                IUniqueDef iud = palette.UniqueDurationDef(i);
                Debug.Assert(iud is MidiChordDef);
                midiChordDefs.Add(iud as MidiChordDef);
            }
            return midiChordDefs;
        }

        private void SetPitchWheelTestMidiChordDefs()
        {
            IReadOnlyList<IReadOnlyList<IReadOnlyList<byte>>> EnvelopeShapes = new List<List<List<byte>>>()
            {
                EnvelopesShapes2, EnvelopeShapes3, EnvelopeShapes4, EnvelopeShapes5, EnvelopeShapes6, EnvelopeShapes7, EnvelopeShapesLong
            };
            List<List<MidiChordDef>> pitchWheelTestMidiChordDefs = new List<List<MidiChordDef>>();
            foreach(IReadOnlyList<IReadOnlyList<byte>> envList in EnvelopeShapes)
            {
                List<MidiChordDef> pwmcds = GetPitchWheelTestMidiChordDefs(envList);
                pitchWheelTestMidiChordDefs.Add(pwmcds);
            }

            PitchWheelTestMidiChordDefs = pitchWheelTestMidiChordDefs;
        }
        private List<MidiChordDef> GetPitchWheelTestMidiChordDefs(IReadOnlyList<IReadOnlyList<byte>> envList)
        {
            List<MidiChordDef> rval = new List<MidiChordDef>();
            foreach(List<byte> envelope in envList)
            {
                MidiChordDef mcd = new MidiChordDef(new List<byte>() { 60 }, new List<byte>() { 127 }, 1000, true);
                mcd.SetPitchWheelEnvelope(new Envelope(envelope, 127, 127, envelope.Count));
                rval.Add(mcd);
            }
            return rval;
        }

        private void SetOrnamentTestMidiChordDefs()
        {
            IReadOnlyList<IReadOnlyList<IReadOnlyList<byte>>> EnvelopeShapes = new List<List<List<byte>>>()
            {
                EnvelopesShapes2, EnvelopeShapes3, EnvelopeShapes4, EnvelopeShapes5, EnvelopeShapes6, EnvelopeShapes7, EnvelopeShapesLong
            };
            List<List<MidiChordDef>> ornamentTestMidiChordDefs = new List<List<MidiChordDef>>();
            int relativePitchHierarchyIndex = 0;
            int absHierarchyRoot = 0;
            foreach(IReadOnlyList<IReadOnlyList<byte>> envList in EnvelopeShapes)
            {
                IReadOnlyList<IReadOnlyList<byte>> localEnvList = EnvelopeShapes[6];
                List<MidiChordDef> omcds = GetOrnamentTestMidiChordDefs(localEnvList, relativePitchHierarchyIndex++, absHierarchyRoot++);
                ornamentTestMidiChordDefs.Add(omcds);
            }

            OrnamentTestMidiChordDefs = ornamentTestMidiChordDefs;
        }

        private List<MidiChordDef> GetOrnamentTestMidiChordDefs(IReadOnlyList<IReadOnlyList<byte>> envList, int relativePitchHierarchyIndex, int absHierarchyRoot)
        {
            List<MidiChordDef> rval = new List<MidiChordDef>();

            List<int> absolutePitchHierarchy = M.GetAbsolutePitchHierarchy(relativePitchHierarchyIndex, absHierarchyRoot);
            Gamut gamut = new Gamut(absolutePitchHierarchy, 8);

            foreach(List<byte> envelope in envList)
            {
                Envelope ornamentEnvelope = new Envelope(envelope, 127, 8, 6);
                Envelope timeWarpEnvelope = new Envelope(new List<byte>() { 127, 0, 127 }, 127, 127, 6);

                int msDuration = 1000;
                int firstPitch = 60;
                while(!gamut.Contains(firstPitch))
                {
                    firstPitch++;
                }
                MidiChordDef mcd = new MidiChordDef(msDuration, gamut, firstPitch, 1, ornamentEnvelope);

                mcd.TimeWarp(timeWarpEnvelope, 16);

                //mcd.Transpose(gamut, 3);

                rval.Add(mcd);
            }
            return rval;
        }

        private void SetType1TemplateTrks()
        {
            List<Trk> type1TemplateTrks = new List<Trk>();

            Trk templateTrk0 = GetType1TemplateTrk(4, 0, 9, new List<byte>() { 0, 127 }, 7);
            type1TemplateTrks.Add(templateTrk0);
            Trk templateTrk1 = GetType1TemplateTrk(6, 6, 9, new List<byte>() { 0, 127 }, 7);
            // maybe add more type1 template trks here.
            type1TemplateTrks.Add(templateTrk1);

            Type1TemplateTrks = type1TemplateTrks;
        }
        private Trk GetType1TemplateTrk(int relativePitchHierarchyIndex, int rootPitch, int nPitchesPerOctave, IReadOnlyList<byte> ornamentShape, int nOrnamentChords)
        {
            List<int> absolutePitchHierarchy = M.GetAbsolutePitchHierarchy(relativePitchHierarchyIndex, rootPitch);
            Gamut gamut = new Gamut(absolutePitchHierarchy, nPitchesPerOctave);

            List<IUniqueDef> iuds = new List<IUniqueDef>();
            int rootNotatedPitch = gamut[gamut.Count / 2];
            int nPitchesPerChord = 1;
            int msDuration = 1000;

            MidiChordDef mcd1 = new MidiChordDef(msDuration, gamut, rootNotatedPitch, nPitchesPerChord + 1, null);
            iuds.Add(mcd1);
            MidiChordDef mcd2 = new MidiChordDef(msDuration, gamut, rootNotatedPitch, nPitchesPerChord + 2, null);
            mcd2.TransposeInGamut(1);
            iuds.Add(mcd2);
            Envelope ornamentEnvelope = new Envelope(ornamentShape, 127, nPitchesPerOctave, nOrnamentChords);
            MidiChordDef mcd3 = new MidiChordDef(msDuration * 2, gamut, rootNotatedPitch, nPitchesPerChord + 3, ornamentEnvelope);
            mcd3.TransposeInGamut(2);
            iuds.Add(mcd3);
            MidiChordDef mcd4 = new MidiChordDef(msDuration, gamut, rootNotatedPitch, nPitchesPerChord + 4, null);
            mcd4.TransposeInGamut(3);
            iuds.Add(mcd4);

            Trk trk0 = new Trk(0, 0, iuds);

            return trk0;
        }
        #endregion Init() helper functions
        #endregion Init()

        /// <summary>
        /// Adds a Type1Block to blockList
        /// </summary>
        /// <param name="blockList">The list to which to add the new block</param>
        /// <param name="blockMsDuration">The duration of the block</param>
        /// <param name="type1TemplateTrkIndex">The index of the template Trk in Type1Templates</param>
        /// <param name="trk0InitialDelay">The duration of the rest at the beginning of track (=channel) 0</param>
        private void AddType1Block(List<Block> blockList, int blockMsDuration, int type1TemplateTrkIndex, int trk0InitialDelay)
        {
            Debug.Assert(blockList != null && blockMsDuration > 0 && type1TemplateTrkIndex >= 0 && trk0InitialDelay >= 0);

            Type1Block type1Block = new Type1Block(blockMsDuration, Type1TemplateTrks[type1TemplateTrkIndex], trk0InitialDelay, MidiChannelIndexPerOutputVoice);
            blockList.Add(type1Block);
        }

        #region private properties for use of Tombeau1Algorithm
        #region initialised by Init()
        private IReadOnlyList<IReadOnlyList<MidiChordDef>> PaletteMidiChordDefs = null;
        private IReadOnlyList<IReadOnlyList<MidiChordDef>> PitchWheelTestMidiChordDefs = null;
        private IReadOnlyList<IReadOnlyList<MidiChordDef>> OrnamentTestMidiChordDefs = null;
        private IReadOnlyList<Trk> Type1TemplateTrks = null;
        #endregion initialised by Init()
        #region envelopes
        private static List<List<byte>> EnvelopesShapes2 = new List<List<byte>>()
            {
                { new List<byte>() {64, 0} },
                { new List<byte>() {64, 18} },
                { new List<byte>() {64, 36} },
                { new List<byte>() {64, 54} },
                { new List<byte>() {64, 72} },
                { new List<byte>() {64, 91} },
                { new List<byte>() {64, 109} },
                { new List<byte>() {64, 127} }
            };
        private static List<List<byte>> EnvelopeShapes3 = new List<List<byte>>()
            {
                { new List<byte>() {64, 0, 64} },
                { new List<byte>() {64, 18, 64} },
                { new List<byte>() {64, 36, 64} },
                { new List<byte>() {64, 54, 64} },
                { new List<byte>() {64, 72, 64} },
                { new List<byte>() {64, 91, 64} },
                { new List<byte>() {64, 109, 64} },
                { new List<byte>() {64, 127, 64} }
            };
        private static List<List<byte>> EnvelopeShapes4 = new List<List<byte>>()
            {
                { new List<byte>() {64, 0, 64, 64} },
                { new List<byte>() {64, 22, 64, 64} },
                { new List<byte>() {64, 22, 96, 64} },
                { new List<byte>() {64, 64, 0, 64} },
                { new List<byte>() {64, 64, 22, 64} },
                { new List<byte>() {64, 64, 80, 64} },
                { new List<byte>() {64, 80, 64, 64} },
                { new List<byte>() {64, 96, 22, 64 } }
            };
        private static List<List<byte>> EnvelopeShapes5 = new List<List<byte>>()
            {
                { new List<byte>() {64, 50, 72, 50, 64} },
                { new List<byte>() {64, 64, 0, 64, 64} },
                { new List<byte>() {64, 64, 64, 80, 64} },
                { new List<byte>() {64, 64, 64, 106, 64} },
                { new List<byte>() {64, 64, 127, 64, 64} },
                { new List<byte>() {64, 70, 35, 105, 64} },
                { new List<byte>() {64, 72, 50, 70, 64} },
                { new List<byte>() {64, 80, 64, 64, 64} },
                { new List<byte>() {64, 105, 35, 70, 64} },
                { new List<byte>() {64, 106, 64, 64, 64} }
            };
        private static List<List<byte>> EnvelopeShapes6 = new List<List<byte>>()
            {
                { new List<byte>() {64, 22, 43, 64, 64, 64} },
                { new List<byte>() {64, 30, 78, 64, 40, 64} },
                { new List<byte>() {64, 40, 64, 78, 30, 64} },
                { new List<byte>() {64, 43, 106, 64, 64, 64} },
                { new List<byte>() {64, 64, 64, 43, 22, 64} },
                { new List<byte>() {64, 64, 64, 64, 106, 64} },
                { new List<byte>() {64, 64, 64, 64, 127, 64} },
                { new List<byte>() {64, 64, 64, 106, 43, 64} },
                { new List<byte>() {64, 106, 64, 64, 64, 64} },
                { new List<byte>() {64, 127, 127, 22, 64, 64} }
            };
        private static List<List<byte>> EnvelopeShapes7 = new List<List<byte>>()
            {
                { new List<byte>() {64, 0, 0, 106, 106, 64, 64} },
                { new List<byte>() {64, 28, 68, 48, 108, 88, 64} },
                { new List<byte>() {64, 40, 20, 80, 60, 100, 64} },
                { new List<byte>() {64, 55, 50, 75, 50, 64, 64} },
                { new List<byte>() {64, 64, 64, 64, 64, 32, 64} },
                { new List<byte>() {64, 64, 50, 75, 50, 55, 64} },
                { new List<byte>() {64, 73, 78, 53, 78, 64, 64} },
                { new List<byte>() {64, 85, 64, 106, 64, 127, 64} },
                { new List<byte>() {64, 88, 108, 48, 68, 28, 64} },
                { new List<byte>() {64, 100, 60, 80, 20, 40, 64} },
                { new List<byte>() {64, 127, 127, 64, 64, 64, 64} }
            };
        private static List<List<byte>> EnvelopeShapesLong = new List<List<byte>>()
            {
                { new List<byte>() {64, 0, 64, 96, 127, 30, 0, 64} },
                { new List<byte>() {64, 64, 64, 127, 64, 106, 43, 64} },
                { new List<byte>() {64, 64, 43, 43, 64, 64, 85, 22, 64} },
                { new List<byte>() {64, 64, 64, 0, 64, 127, 0, 64, 64} },
                { new List<byte>() {64, 80, 64, 92, 64, 64, 64, 98, 64} },
                { new List<byte>() {64, 98, 64, 64, 64, 92, 64, 80, 64} },
                { new List<byte>() {64, 64, 64, 0, 64, 127, 0, 64, 127, 0, 64, 64} },
                { new List<byte>() {64, 64, 64, 64, 64, 64, 64, 64, 64, 100, 50, 100} },
                { new List<byte>() {64, 64, 64, 64, 64, 64, 64, 64, 64, 127, 43, 127, 64} },
                { new List<byte>() {64, 127, 43, 127, 64, 64, 64, 64, 64, 64, 64, 64, 64} },
                { new List<byte>() {64, 64, 64, 64, 64, 64, 64, 127, 43, 127, 64, 127, 43, 127, 64} },
                { new List<byte>() {64, 127, 43, 127, 43, 127, 64, 64, 64, 64, 64, 64, 64, 64, 64} },
                { new List<byte>() {64, 64, 64, 0, 64, 127, 0, 64, 127, 64, 0, 64, 127, 64, 0, 64} },
                { new List<byte>() {64, 127, 64, 64, 0, 64, 127, 0, 64, 127, 64, 0, 64, 127, 64, 0, 64} },
                { new List<byte>() {64, 127, 43, 127, 43, 127, 64, 127, 43, 127, 43, 127, 64, 64, 64, 64, 64, 64, 64, 64, 64} }
            };
        #endregion envelopes
        #region duration modi
        private static List<int> Durations1 = new List<int>()
            {   1000 };
        private static List<int> Durations2 = new List<int>()
            {   1000, 707 }; // 1 / ( 2^(1 / 2) )
        private static List<int> Durations3 = new List<int>()
            {   1000, 794, 630 }; // 1 / ( 2^(1 / 3) )
        private static List<int> Durations4 = new List<int>()
            {   1000, 841, 707, 595 }; // 1 / ( 2^(1 / 4) )
        private static List<int> Durations5 = new List<int>()
            {   1000, 871, 758, 660, 574 }; // 1 / ( 2^(1 / 5) )
        private static List<int> Durations6 = new List<int>()
            {   1000, 891, 794, 707, 630, 561 }; // 1 / ( 2^(1 / 6) )
        private static List<int> Durations7 = new List<int>()
            {   1000, 906, 820, 743, 673, 610, 552 }; // 1 / ( 2^(1 / 7) )
        private static List<int> Durations8 = new List<int>()
            {   1000, 917, 841, 771, 707, 648, 595, 545}; // 1 / ( 2^(1 / 8) )
        private static List<int> Durations9 = new List<int>()
            {   1000, 926, 857, 794, 735, 680, 630, 583, 540}; // 1 / ( 2^(1 / 9) )
        private static List<int> Durations10 = new List<int>()
            {   1000, 933, 871, 812, 758, 707, 660, 616, 574, 536}; // 1 / ( 2^(1 / 10) )
        private static List<int> Durations11 = new List<int>()
            {   1000, 939, 882, 828, 777, 730, 685, 643, 604, 567, 533 }; // 1 / ( 2^(1 / 11) )
        private static List<int> Durations12 = new List<int>()
            {   1000, 944, 891, 841, 794, 749, 707, 667, 630, 595, 561, 530 }; // 1 / ( 2^(1 / 12) )

        #endregion duration modi
        #endregion private properties for use of Tombeau1Algorithm
    }
}
