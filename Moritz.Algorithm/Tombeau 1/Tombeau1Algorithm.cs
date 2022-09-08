using System;
using System.Collections.Generic;
using System.Diagnostics;

using Krystals5ObjectLibrary;
using Moritz.Palettes;
using Moritz.Spec;
using Moritz.Symbols;

namespace Moritz.Algorithm.Tombeau1
{
	public partial class Tombeau1Algorithm : CompositionAlgorithm
	{
		public Tombeau1Algorithm()
            : base()
        {
            CheckParameters();
        }

        public override IReadOnlyList<int> MidiChannelPerOutputVoice { get { return new List<int>() { 0, 1, 2, 3 }; } }
		public override IReadOnlyList<int> MidiChannelPerInputVoice { get { return null; } }
        public override int NumberOfBars { get { return 50; } }

		/// <summary>
		/// See CompositionAlgorithm.DoAlgorithm()
		/// </summary>
		public override List<Bar> DoAlgorithm(List<Krystal> krystals, List<Palette> palettes)
		{
			_krystals = krystals;
			_palettes = palettes;

			#region main comment (thoughts etc.)
			/*********************************************************************************************
			Think Nancarrow: Mechanical Piano music... outside the notation system... outside the management system...
			Think Webern Piano Variations. Phrases, envelope counterpoint...
			Think Study 1: background/foreground, depth.
            Think Bach , Brückner, Reich. Repeating/changing, fairly fast groups of chords with harmonic support... and ornaments...
			Think Wagner: (especially Tristan) Parts moving by (possibly microtonal) step to recognisable harmonies...
            

			The following parameters can be controlled using the Resident Sf2 Synth:
			    Commands:    preset, pitchwheel
			    Controls:    volume, pan, pitchWheelDeviation, allControllersOff, allSoundOff
			    Note:        velocity, pitch.
			
			Tombeau1 is just going to use preset 0:0 (=grandPiano), so the available parameters are:
			    "tuning" -- i.e. pitchWheelDeviation and pitchWheel
			    pan,
			    velocity,
			    pitch

			** "Chords are colour" (Stockhausen)
            
            ** I want to keep the number of pitches in chords fairly low, so that they are recognisable.

            ** Using Seqs and Trks should make it possible to compose powerful, comprehensible, pregnant
			   relations between these parameters at every structural level. "Harmony & counterpoint" is
               not only going to be there in the pitches (chords, envelopes...), but also in the velocities
               and the pan positions... Tuning is a parameter that could be introduced as a surprise later...

			** Thinking about templates and the structural levels:
			   First create a set of Template1: Envelopes, Durations, MidiChordDefs, Trks etc. that can be
               used as the basis for similar objects in the piece. This includes MidiChordDefs created from pallets.
			   A new TrkPalette class will be used to organize Trk relations. These relations should include Envelope
			   relationships. Possibly also create an EnvelopePalette class...
			   Trks in the piece may be derived fairly simply from templates, but they may also be influenced by
               their containing Seqs.
			   How do notes/chords relate horizontally inside a trk? How do they relate vertically inside a seq?
               How do seqs relate to each other globally...
			   Seqs can also be cloned, but I dont think there should be Seqs in Template1.

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
               -- MidiChordDef = trk.ToMidiChordDef(msDuration) or MidiChordDef = new MidiChordDef(trk, msDuration);
                  Converts the trk's MidiChordDefs (just their NotatedPitches and NotatedVelocities) to BasicMidiChords in a
                  single, ornamented MidiChordDef. 
               -- Seq.AlignTrk(alignmentMsPosReSeq, trkIndex, trkUniqueDefIndex)
                  Shifts the trk horizontally in the seq until the indexed UniqueDef is at alignmentMsPosReSeq. The relative
                  positions of the UniqueDefs in the trk do not change.                   
                        
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
               Also, *annotations* that are instructions to performers (hairpins, accel., rit. spring to mind).
               Conventional dynamic symbols are, I think, meant for *performers*, so should only be attached to inputChords...
               The info in *outputChords* is the info that could go in a MIDI file, and vice versa.
			
			****************************************************************************
			13.06.2019 new Tombeau 1
			Modes might be useful later, when looking for chord relations to use, but I'm currently more interested in using
			a different approach:

			Instead of using Modes in Tombeau 1, I want to implement something like "passing notes" in voices that link particular,
			freely chosen, recognizable harmonies. To be recognisable, harmonies have to be both clearly recognizable and perceptibly
			repeated! Better to choose particular harmonies by ear than rely on some theoretical distance... We'll see.
			
			The first thing to do is create a specialized Trk class (ChainTrk) that contains IUniqueDefs that move stepwise,
			according to some envelope, between start and end pitches.
					a) the start and end pitches must be defined - even if they are not actually performed
					b) the pitch envelope is freely definable, and may exceed the range defined by the target pitches
						(the pitches in the chains may be microtonal).
					c) the duration envelope is freely definable.
					d) it should be possible to replace any MidiChordDef in a Chain by a MidiRestDef.

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
			#region Mode functions
			/***************************************************************************
            public Mode functions and properties that have been implemented:
                (Mode is immutable)
                constructor:
                Mode(List<int> absolutePitchHierarchy, int nPitchesPerOctave)

                Clone()
                Conjugate()

                Contains(byte pitch)
                ContainsAllPitches(List<byte> pitches)

                PitchSequence(int pitch1OctaveIndex, int pitch1pitchInOctaveIndex, Envelope envelope)
                GetChord(int rootPitch, int nPitches)

                GetVelocityPerAbsolutePitch(int minimumVelocity)

                Mode[i] { get; }
                IndexOf(int pitch)
                NPitchesPerOctave { get; }
                AbsolutePitchHierarchy  { get; } // a copy of the private list.
                List {get;} // a copy of the private list.
                Count {get;}            
			***************************************************************************/
			#endregion Mode functions
			#region MidiChordDef functions
			/***************************************************************************            
            public MidiChordDef functions that have been implemented and are especially relevant to this project:
                constructors:
                SIMPLE MidiChordDefs (containing a single BasicMidiChordDef):
                    MidiChordDef(List<byte> pitches, List<byte> velocities, int msDuration, bool hasChordOff)
                ORNAMENTS
                    MidiChordDef(int msDuration, Mode mode, int rootNotatedPitch, int nPitchesPerChord, Envelope ornamentEnvelope = null, string ornamentText = null)    
                PALETTE MidiChordDefs (MidiChordDefs created from palettes):    
                    MidiChordDef(int msDuration, byte pitchWheelDeviation, bool hasChordOff, List<byte> rootMidiPitches, List<byte> rootMidiVelocities, int ornamentNumberSymbol, MidiChordSliderDefs midiChordSliderDefs, List<BasicMidiChordDef> basicMidiChordDefs)
                    
                Clone()
                Conjugate() // requires MidiChordDef.Mode to be set.

                TimeWarp(Envelope envelope, double distortion) // Changes the msPositions of the BasicMidiChordDefs without changing the length of the MidiChordDef.
                MsDuration {get; set;} // set changes the durations of contained BasicMidiChordDefs
                AdjustMsDuration(double factor) // Multiplies the MsDuration by the given factor.

                SetVerticalDensity(int newDensity)
                GetNoteCombination(MidiChordDef mcd1, MidiChordDef mcd2, MidiChordPitchOperator midiChordPitchOperator) // a static function
                
                Invert(int nPitchesToShift) // shifts the lower pitches up one octave. The pitches stay in the Mode, if any.
                Transpose(int interval) // sets Mode to null
                Transpose(Mode mode, int steps)

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

                Mode {get; set;} // The Mode can only be set (or changed) if all the pitches in the MidiChordDef are in the new Mode.
            ***************************************************************************/
			#endregion MidiChordDef functions
			#region Trk functions
			/***************************************************************************            
            public Trk functions that have been implemented and are especially relevant to this project:
            constructors:
                Trk(int midiChannel, int msPositionReContainer, List<IUniqueDef> iuds)
                Trk(int midiChannel)

                Clone()
				ConcatCloneAt(VoiceDef trk2, int msPositionReTrk) // Appends __clones__ of the IUniqueDefs in trk2 at msPositionReTrk this Trk. (c.f. AddRange(VoiceDef trk)).
 
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
                MidiChannelPerOutputVoice { get; }

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

			Envelope centredEnvelope = _krystals[0].ToEnvelope(0, 127); // values distributed around 64, gradually becoming more eccentric
			Envelope basedEnvelope = _krystals[1].ToEnvelope(0, 127); // values increase gradually from 0 to 127, becoming more eccentric. 

			/**********************************************/

			List<IUniqueDef> trk0iuds = new List<IUniqueDef>();
			for(int i = 0; i < 50; i++)
			{
				trk0iuds.Add(new MidiChordDef(new List<byte>() { 64 }, new List<byte>() { 64 }, 5000, true));
			}

			Trk trk0 = new Trk(0, 0, trk0iuds);
			Trk trk1 = new Trk(1, 0, new List<IUniqueDef>());
			Trk trk2 = new Trk(2, 0, new List<IUniqueDef>());
			Trk trk3 = new Trk(3, 0, new List<IUniqueDef>());

			var trks = new List<Trk>() { trk0, trk1, trk2, trk3 };

			Seq mainSeq = new Seq(0, trks, MidiChannelPerOutputVoice);

			List<int> barlineMsPositions = GetBarlinePositions(trk0iuds);

			//Do global changes that affect the whole piece here (accel., rit, transpositions etc.)
			FinalizeMainSeq(mainSeq);

			List<Bar> bars = GetBars(mainSeq, null, barlineMsPositions, null, null);

			return bars;
        }

		public override ScoreData SetScoreRegionsData(List<Bar> bars)
		{
			Dictionary<int, (int index, int msPosition)> msPosPerBarlineIndexDict = GetMsPosPerBarlineIndexDict(bars);

			// Each barline is the left barline of the bar with the same index.
			// The finalBarline has also been added to the Dictionary, so
			// msPosPerBarlineIndexDict.Count is 1 + bars.Count.
			var barline0 = msPosPerBarlineIndexDict[0];
			var barline6 = msPosPerBarlineIndexDict[6];
			var barline16 = msPosPerBarlineIndexDict[16];
			var barline31 = msPosPerBarlineIndexDict[31];
			var finalBarline = msPosPerBarlineIndexDict[msPosPerBarlineIndexDict.Count - 1];

			RegionDef rd1 = new RegionDef("A", barline0, barline6);
			RegionDef rd2 = new RegionDef("B", barline6, barline31);
			RegionDef rd3 = new RegionDef("C", barline6, barline16);
			RegionDef rd4 = new RegionDef("D", barline31, finalBarline);

			List<RegionDef> regionDefs = new List<RegionDef>() { rd1, rd2, rd3, rd4 };

			RegionSequence regionSequence = new RegionSequence(regionDefs, "ABCAD");

			ScoreData scoreData = new ScoreData(regionSequence);

			return scoreData;
		}

		/// <summary>
		/// Returns a dictionary containing (barlineIndex, (barlineIndex, barlineMsPosition)) KeyValuePairs.
		/// Each barlineIndex is the index of the left barline of the bar with the same index, so the first KeyValuePair
		/// is (0,(0,0)). The finalBarline is also added to the returned dictionary, so its Count is 1 + bars.Count.
		/// The data for the final barline in the score is in the final entry.
		/// </summary>
		private Dictionary<int, (int index, int msPosition)> GetMsPosPerBarlineIndexDict(List<Bar> bars)
		{
			var rval = new Dictionary<int, (int index, int msPosition)>();

			int barlineMsPos = 0;
			int barsCount = bars.Count;
			for(int i = 0; i < barsCount; ++i)
			{
				var tuple = (index: i, msPosition: barlineMsPos);
				rval.Add(i, tuple);
				barlineMsPos += bars[i].MsDuration;
			}
			rval.Add(barsCount, (index: barsCount, msPosition: barlineMsPos));

			return rval;
		}

		#region available Trk and ModeGrpTrk transformations
		// Add();
		// AddRange();
		// AdjustChordMsDurations();
		// AdjustExpression();
		// AdjustVelocities();
		// AdjustVelocitiesHairpin();
		// AlignObjectAtIndex();
		// CreateAccel();
		// FindIndexAtMsPositionReFirstIUD();
		// Insert();
		// InsertRange();
		// Permute();
		// Remove();
		// RemoveAt();
		// RemoveBetweenMsPositions();
		// RemoveRange();
		// RemoveScorePitchWheelCommands();
		// Replace();
		// SetDurationsFromPitches();
		// SetPanGliss(0, subT.MsDuration, 0, 127);
		// SetPitchWheelDeviation();
		// SetPitchWheelSliders();
		// SetVelocitiesFromDurations();
		// SetVelocityPerAbsolutePitch();
		// TimeWarp();
		// Translate();
		// Transpose();
		// TransposeStepsInModeGamut();
		// TransposeToRootInModeGamut();
		#endregion available Trk and ModeGrpTrk transformations

		/// <summary>
		/// The compulsory first barline (at msPosition=0) is NOT included in the returned list.
		/// The compulsory final barline (at the end of the final ModeSegment) IS included in the returned list.
		/// There is a barline at the end of each voice1 modeSegment.
		/// All the returned barline positions are unique, and in ascending order.
		/// </summary>
		private List<int> GetBarlinePositions(List<IUniqueDef> trk0iuds)
		{
			//var msValuesListList = voice1.GetMsValuesOfModeGrpTrks();

			List<int> barlinePositions = new List<int>();
			int currentPosition = 0;
			foreach(IUniqueDef iud in trk0iuds)
			{
				currentPosition += iud.MsDuration;
				barlinePositions.Add(currentPosition);
			}

			// add further barlines here, maybe using a list provided as an argument.

			// old code:
			//foreach(IReadOnlyList<MsValues> msValuesList in msValuesListList)
			//{
			//	foreach(MsValues msValues in msValuesList)
			//	{
			//		barlinePositions.Add(msValues.EndMsPosition);
			//	}
			//}

			return barlinePositions;
		}

		private void RemoveDuplicates(List<int> barlinePositions)
		{
			for(int i = barlinePositions.Count - 1; i > 0; --i)
			{
				int iPos = barlinePositions[i];
				for(int j = i - 1; j >= 0; j--)
				{
					if(iPos == barlinePositions[j])
					{
						barlinePositions.RemoveAt(i);
						break;
					}
				}
			}
		}

		/// <summary>
		/// Pad empty Trks with a single MidiRestDef.
		/// Also, do other global changes that affect the whole piece here (accel., rit, transpositions etc.).
		/// </summary>
		private void FinalizeMainSeq(Seq mainSeq)
		{
			mainSeq.PadEmptyTrks();
		}

		/// <summary>
		/// See summary and example code on abstract definition in CompositionAlogorithm.cs
		/// </summary>
		protected override List<List<SortedDictionary<int, string>>> GetClefChangesPerBar(int nBars, int nVoicesPerBar)
		{
			return null;
		}

		#region private properties for use by Tombeau1Algorithm
		#region envelopes
		/// <summary>
		/// If domain is null, the returned shape will come from the _sliderShapesLong list. 
		/// </summary>
		/// <param name="domain">null, or in range 2..7</param>
		/// <param name="index"></param>
		/// <returns></returns>
		private IReadOnlyList<byte> SliderShape(int? domain, int index)
        {
            if(domain != null)
            {
                Debug.Assert(domain > 1 && domain < 8);
            }
            IReadOnlyList<byte> rval = null;
            switch (domain)
            {
                case 2:
                    Debug.Assert(index < _sliderShapes2.Count);
                    rval = _sliderShapes2[index];
                    break;
                case 3:
                    Debug.Assert(index < _sliderShapes3.Count);
                    rval = _sliderShapes3[index];
                    break;
                case 4:
                    Debug.Assert(index < _sliderShapes4.Count);
                    rval = _sliderShapes4[index];
                    break;
                case 5:
                    Debug.Assert(index < _sliderShapes5.Count);
                    rval = _sliderShapes5[index];
                    break;
                case 6:
                    Debug.Assert(index < _sliderShapes6.Count);
                    rval = _sliderShapes6[index];
                    break;
                case 7:
                    Debug.Assert(index < _sliderShapes7.Count);
                    rval = _sliderShapes7[index];
                    break;
                case null:
                    Debug.Assert(index < _sliderShapesLong.Count);
                    rval = _sliderShapesLong[index];
                    break;
            }
            return rval;
        }
        private static IReadOnlyList<IReadOnlyList<byte>> _sliderShapes2 = new List<List<byte>>()
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
        private static IReadOnlyList<IReadOnlyList<byte>> _sliderShapes3 = new List<List<byte>>()
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
        private static IReadOnlyList<IReadOnlyList<byte>> _sliderShapes4 = new List<List<byte>>()
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
        private static IReadOnlyList<IReadOnlyList<byte>> _sliderShapes5 = new List<List<byte>>()
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
        private static IReadOnlyList<IReadOnlyList<byte>> _sliderShapes6 = new List<List<byte>>()
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
        private static IReadOnlyList<IReadOnlyList<byte>> _sliderShapes7 = new List<List<byte>>()
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
        private static IReadOnlyList<IReadOnlyList<byte>> _sliderShapesLong = new List<List<byte>>()
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
        private static IReadOnlyList<IReadOnlyList<int>> _durationModi = new List<List<int>>()
        {
            _durations1, _durations2, _durations3, _durations4, _durations5,_durations6,
            _durations7, _durations8, _durations9, _durations10, _durations11, _durations12

        };
        private static List<int> _durations1 = new List<int>()
            {   1000 };
        private static List<int> _durations2 = new List<int>()
            {   1000, 707 }; // 1 / ( 2^(1 / 2) )
        private static List<int> _durations3 = new List<int>()
            {   1000, 794, 630 }; // 1 / ( 2^(1 / 3) )
        private static List<int> _durations4 = new List<int>()
            {   1000, 841, 707, 595 }; // 1 / ( 2^(1 / 4) )
        private static List<int> _durations5 = new List<int>()
            {   1000, 871, 758, 660, 574 }; // 1 / ( 2^(1 / 5) )
        private static List<int> _durations6 = new List<int>()
            {   1000, 891, 794, 707, 630, 561 }; // 1 / ( 2^(1 / 6) )
        private static List<int> _durations7 = new List<int>()
            {   1000, 906, 820, 743, 673, 610, 552 }; // 1 / ( 2^(1 / 7) )
        private static List<int> _durations8 = new List<int>()
            {   1000, 917, 841, 771, 707, 648, 595, 545}; // 1 / ( 2^(1 / 8) )
        private static List<int> _durations9 = new List<int>()
            {   1000, 926, 857, 794, 735, 680, 630, 583, 540}; // 1 / ( 2^(1 / 9) )
        private static List<int> _durations10 = new List<int>()
            {   1000, 933, 871, 812, 758, 707, 660, 616, 574, 536}; // 1 / ( 2^(1 / 10) )
        private static List<int> _durations11 = new List<int>()
            {   1000, 939, 882, 828, 777, 730, 685, 643, 604, 567, 533 }; // 1 / ( 2^(1 / 11) )
        private static List<int> _durations12 = new List<int>()
            {   1000, 944, 891, 841, 794, 749, 707, 667, 630, 595, 561, 530 }; // 1 / ( 2^(1 / 12) )

        #endregion duration modi
        #endregion private properties for use by Tombeau1Algorithm
    }
}
