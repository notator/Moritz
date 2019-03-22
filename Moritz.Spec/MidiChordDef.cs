using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Krystals4ObjectLibrary;
using Moritz.Globals;
using Moritz.Xml;

namespace Moritz.Spec
{
	///<summary>
	/// A MidiChordDef can either be saved and retrieved from voices in an SVG file, or
	/// retrieved from a palette (whereby the pallete makes a deep clone of its contained values).
	///</summary>
	public class MidiChordDef : DurationDef, IUniqueSplittableChordDef
    {
        #region constructors
        /// <summary>
        /// A MidiChordDef containing a single BasicMidiChordDef. Absent fields are set to 0 or null.
        /// The pitches argument is used to set both the NotatedMidiPitches and BasicMidiChordDefs[0].Pitches.
        /// The velocities argument is used to set the NotatedMIDIVelocities and BasicMidiChordDefs[0].Velocities.
        /// Velocities must be in range 1..127.
        /// </summary>
        /// <param name="pitches">in range 0..127</param>
        /// <param name="velocities">In range 1..127</param>
        /// <param name="msDuration">greater than zero</param>
        /// <param name="hasChordOff"></param>
        public MidiChordDef(List<byte> pitches, List<byte> velocities, int msDuration, bool hasChordOff)
            : base(msDuration)
        {
            #region conditions
            Debug.Assert(pitches.Count == velocities.Count);
            foreach(byte pitch in pitches)
            {
                AssertIsMidiValue(pitch);
            }
            foreach(byte velocity in velocities)
            {
                AssertIsVelocityValue(velocity);
            }
            #endregion conditions

            MsPositionReFirstUD = 0; // default value
            HasChordOff = hasChordOff;
            MinimumBasicMidiChordMsDuration = 1; // not used (this is not an ornament)

            OrnamentText = null;

            MidiChordSliderDefs = null;

            byte? bank = null;                                                                           
            byte? patch = null;

            BasicDurationDefs.Add(new BasicMidiChordDef(msDuration, bank, patch, hasChordOff, pitches, velocities));

            SetNotatedValuesFromFirstBMCD();

			AssertConsistency(this);
        }

        /// <summary>
        /// A MidiChordDef having msDuration, and containing an ornament having BasicMidiChordDefs with nPitchesPerChord.
        /// The notated pitch and the pitch of BasicMidiChordDefs[0] are set to rootNotatedPitch.
        /// The notated velocity of all pitches is set to 127.
        /// The root pitches of the BasicMidiChordDefs begin with rootNotatedPitch, and follow the ornamentEnvelope, using
        /// the ornamentEnvelope's values as indices in the mode. Their durations are as equal as possible, to give the
        /// overall msDuration. If ornamentEnvelope is null, a single, one-note BasicMidiChordDef will be created.
        /// This constructor uses Mode.GetChord(rootNotatedPitch, nPitchesPerChord) which returns pitches that are
        /// vertically spaced differently according to the absolute height of the rootNotatedPitch. The number of pitches
        /// in a chord may also be less than nPitchesPerChord (see mode.GetChord(...) ).
        /// An exception is thrown if rootNotatedPitch is not in the mode.
        /// </summary>        
        /// <param name="msDuration">The duration of this MidiChordDef</param>
        /// <param name="mode">The mode containing all the pitches.</param>
        /// <param name="rootNotatedPitch">The lowest notated pitch. Also the lowest pitch of BasicMidiChordDefs[0].</param>
        /// <param name="nPitchesPerChord">The chord density (some chords may have less pitches).</param>
        /// <param name="ornamentEnvelope">The ornament definition.</param>
        public MidiChordDef(int msDuration, Mode mode, int rootNotatedPitch, int nPitchesPerChord, Envelope ornamentEnvelope = null, string ornamentText = null)
            : base(msDuration) 
        {
            NotatedMidiPitches = mode.GetChord(rootNotatedPitch, nPitchesPerChord);
            var nmVelocities = new List<byte>();
            foreach(byte pitch in NotatedMidiPitches) // can be less than nPitchesPerChord
            {
                nmVelocities.Add(127);
            }
            NotatedMidiVelocities = nmVelocities;

            // Sets BasicMidiChords. If ornamentEnvelope == null, BasicMidiChords[0] is set to the NotatedMidiChord.
            SetOrnament(mode, ornamentEnvelope, ornamentText);

			AssertConsistency(this);
		}

        /// <summary>
        /// Constructor used when creating a list of DurationDef templates from a Palette.
        /// The palette has created new values for all the arguments, so this constructor simply transfers
        /// those values to the new MidiChordDef. MsPositionReFirstIUD is set to 0, lyric is set to null.
        /// </summary>
        public MidiChordDef(
            int msDuration, // the total duration (this should be the sum of the durations of the basicMidiChordDefs)
            byte pitchWheelDeviation, // should be set to the default value: M.DEFAULT_PITCHWHEELDEVIATION_2 if unsure.
			bool hasChordOff, // default is M.DefaultHasChordOff (=true)
            List<byte> rootMidiPitches, // the pitches defined in the root chord settings (displayed, by default, in the score).
            List<byte> rootMidiVelocities, // the velocities defined in the root chord settings (displayed, by default, in the score).
            int ornamentNumber, // the number used to identify the ornament (to the right of the tilde). 0 means that there is no ornament
            MidiChordSliderDefs midiChordSliderDefs, // can be null or contain empty lists
            List<BasicMidiChordDef> basicMidiChordDefs)
            : base(msDuration)
        {
            Debug.Assert(rootMidiPitches.Count <= rootMidiVelocities.Count);
            foreach(byte pitch in rootMidiPitches)
            {
                AssertIsMidiValue(pitch);
            }

            MsPositionReFirstUD = 0;
            _pitchWheelDeviation = pitchWheelDeviation;
            HasChordOff = hasChordOff;
            _notatedMidiPitches = rootMidiPitches;
            _notatedMidiVelocities = rootMidiVelocities;

			if(ornamentNumber > 0)
			{
				OrnamentText = ornamentNumber.ToString();
			}

            MidiChordSliderDefs = midiChordSliderDefs;

			foreach(BasicMidiChordDef basicMidiChordDef in basicMidiChordDefs)
			{
				BasicDurationDefs.Add(basicMidiChordDef);
			}

            AssertConsistency(this);
        }

		/// <summary>
		/// Returns a new, ornamented MidiChordDef having msDuration, whose BasicDurationDefs are created from the MidiChordDefs
		/// and RestDefs in the iUniqueDefs argument. Condition: The first iUniqueDef in the iUnqueDefs must be a MidiChordDef.
		/// BasicDurationDefs created from MidiChordDefs are that MidiChordDef's BasicMidiChordDef[0].
		/// BasicDurationDefs created from RestDefs are BasicRestDefs.
		/// The durations of the returned BasicDurationDefs are in proportion to the durations of the MidiChordDefs and RestDefs
		/// in the iUniqueDefs. This function makes a deep clone of all the required attributes in the iUniqueDefs.
		/// </summary>
		/// <param name="msDuration">The duration of the returned MidiChordDef</param>
		/// <param name="iUniqueDefs">See function summary.</param>
		/// <param name="ornamentText">Usually a single character. Will be appended to a tilde, and added (usually above) to the chord symbol.</param>
		public MidiChordDef(int msDuration, List<IUniqueDef> iUniqueDefs, string ornamentText)
			:base(msDuration)
		{
			if(!(iUniqueDefs[0] is MidiChordDef))
			{
				throw new ApplicationException();
			}

			MidiChordDef mcd0 = iUniqueDefs[0] as MidiChordDef;

			MsPositionReFirstUD = 0;
			_pitchWheelDeviation = mcd0.PitchWheelDeviation;
			HasChordOff = mcd0.HasChordOff;
			_notatedMidiPitches = new List<byte>(mcd0.NotatedMidiPitches);
			_notatedMidiVelocities = new List<byte>(mcd0.NotatedMidiVelocities);

			this.OrnamentText = ornamentText; // usually a single character. Will be appended to a tilde, and added to the chord symbol - usually above.

			MidiChordSliderDefs mcsd = mcd0.MidiChordSliderDefs;
			if(mcsd != null)
			{
				MidiChordSliderDefs = new MidiChordSliderDefs(mcsd.PitchWheelMsbs, mcsd.PanMsbs, mcsd.ModulationWheelMsbs, mcsd.ExpressionMsbs);
			}

			#region construct BasicMidiChordDefs

			List<int> msDurations = new List<int>();
			foreach(IUniqueDef iud in iUniqueDefs)
			{
				if(iud is MidiChordDef || iud is MidiRestDef)
				{
					msDurations.Add(iud.MsDuration);
				}
			}

			// fit the msDurations to total msDuration
			msDurations = M.IntDivisionSizes(msDuration, msDurations);
			this.BasicDurationDefs = new List<BasicDurationDef>();
			for(int i = 0; i < iUniqueDefs.Count; ++i)
			{
				IUniqueDef iud = iUniqueDefs[i];
				int iudMsDuration = msDurations[i];

				// default values (used when iud is a MidiRestDef)
				byte? bmcBank = null;
				byte? bmcPatch = null;
				bool bmcHasChordOff = false;
				List<byte> bmcPitches = null;
				List<byte> bmcVelocities = null;

				if(iud is MidiChordDef mcd)
				{
					BasicMidiChordDef bmcd = mcd.FirstBasicDurationDef;
					bmcBank = bmcd.BankIndex;
					bmcPatch = bmcd.PatchIndex;
					bmcHasChordOff = bmcd.HasChordOff;
					bmcPitches = new List<byte>(bmcd.Pitches);
					bmcVelocities = new List<byte>(bmcd.Velocities);
				}

				if(bmcPitches != null)
				{
					var basicMidiChordDef = new BasicMidiChordDef(iudMsDuration, bmcBank, bmcPatch, bmcHasChordOff, bmcPitches, bmcVelocities);
					BasicDurationDefs.Add(basicMidiChordDef);
				}
				else
				{
					var basicMidiRestDef = new BasicMidiRestDef(iudMsDuration);
					BasicDurationDefs.Add(basicMidiRestDef);
				}
			}
			#endregion

			AssertConsistency(this);
		}
		/// <summary>
		/// private constructor -- used by Clone()
		/// </summary>
		private MidiChordDef()
            : base(0)
        {
        }
		#endregion constructors

		#region Clone
		/// <summary>
		/// A deep clone!
		/// </summary>
		/// <returns></returns>
		public override object Clone()
		{
			MidiChordDef rval = new MidiChordDef()
			{
				MsPositionReFirstUD = this.MsPositionReFirstUD,
				// rval.MsDuration must be set after setting BasicMidiChordDefs See below.
				Bank = this.Bank,
				Patch = this.Patch,
				PitchWheelDeviation = this.PitchWheelDeviation,
				HasChordOff = this.HasChordOff,
				BeamContinues = this.BeamContinues,
				Lyric = this.Lyric,
				MinimumBasicMidiChordMsDuration = MinimumBasicMidiChordMsDuration, // required when changing a midiChord's duration
				NotatedMidiPitches = _notatedMidiPitches, // a clone of the displayed notehead pitches
				NotatedMidiVelocities = _notatedMidiVelocities, // a clone of the displayed notehead velocities

				// rval.MidiVelocity must be set after setting BasicMidiChordDefs See below.
				OrnamentText = this.OrnamentText, // the displayed ornament Text (without the tilde)

				MidiChordSliderDefs = null
			};

			MidiChordSliderDefs m = this.MidiChordSliderDefs;
			if(m != null)
			{
				List<byte> pitchWheelMsbs = NewListByteOrNull(m.PitchWheelMsbs);
				List<byte> panMsbs = NewListByteOrNull(m.PanMsbs);
				List<byte> modulationWheelMsbs = NewListByteOrNull(m.ModulationWheelMsbs);
				List<byte> expressionMsbs = NewListByteOrNull(m.ExpressionMsbs);
				if(pitchWheelMsbs != null || panMsbs != null || modulationWheelMsbs != null || expressionMsbs != null)
					rval.MidiChordSliderDefs = new MidiChordSliderDefs(pitchWheelMsbs, panMsbs, modulationWheelMsbs, expressionMsbs);
			}

			List<BasicDurationDef> newBdds = new List<BasicDurationDef>();
			foreach(BasicDurationDef bdd in BasicDurationDefs)
			{
				if(bdd is BasicMidiChordDef bmcd)
				{
					List<byte> pitches = new List<byte>(bmcd.Pitches);
					List<byte> velocities = new List<byte>(bmcd.Velocities);
					newBdds.Add(new BasicMidiChordDef(bmcd.MsDuration, bmcd.BankIndex, bmcd.PatchIndex, bmcd.HasChordOff, pitches, velocities));
				}
				else if(bdd is BasicMidiRestDef bmrd)
				{
					newBdds.Add(new BasicMidiRestDef(bmrd.MsDuration));
				}
				else
				{
					throw new ApplicationException("Unknown BasicDurationDef type.");
				}
			}
			rval.BasicDurationDefs = newBdds;
			rval.MsDuration = this.MsDuration;

			AssertConsistency(rval);

			return rval;
		}


		/// <summary>
		/// Used by all constructors and Clone().
		/// ought also to be called at the end of any function that changes the MidiChordDef's content.
		/// </summary>
		private void AssertConsistency(MidiChordDef mcd)
		{
			//Debug.Assert(mcd.BasicDurationDefs != null && mcd.BasicDurationDefs.Count > 0
			//		&& mcd.BasicDurationDefs[0] is BasicMidiChordDef);
			if(mcd.BasicDurationDefs == null || mcd.BasicDurationDefs.Count == 0 || !(mcd.BasicDurationDefs[0] is BasicMidiChordDef))
			{
				throw new ApplicationException();
			}

			foreach(BasicDurationDef bdd in mcd.BasicDurationDefs)
			{
				if(bdd is BasicMidiChordDef bmcd)
				{
					bmcd.AssertConsistency();
				}
			}

			List<int> basicDurations = BasicDurations;
			int sumDurations = 0;
			foreach(int basicDuration in basicDurations)
			{
				sumDurations += basicDuration;
			}

			//Debug.Assert(mcd.MsDuration == sumDurations);
			if(mcd.MsDuration != sumDurations)
			{
				throw new ApplicationException();
			}
			
		}

		/// <summary>
		/// Used by Clone(). Returns null if listToClone is null, otherwise returns a clone of the listToClone.
		/// </summary>
		private List<byte> NewListByteOrNull(List<byte> listToClone)
        {
            List<byte> newListByte = null;
            if(listToClone != null)
                newListByte = new List<byte>(listToClone);
            return newListByte;
        }
        #endregion Clone

        #region Opposite
        /// <summary>
        /// 1. Creates a new, opposite mode from the argument Mode (see Mode.Opposite()).
        /// 2. Clones this MidiChordDef, and replaces the clone's pitches by the equivalent pitches in the opposite Mode.
        /// 3. Returns the clone.
        /// </summary>
        public MidiChordDef Opposite(Mode mode)
        {
            #region conditions
            Debug.Assert(mode != null);
            #endregion conditions

            int relativePitchHierarchyIndex = (mode.RelativePitchHierarchyIndex + 11) % 22;
            Mode oppositeMode = new Mode(relativePitchHierarchyIndex, mode.BasePitch, mode.NPitchesPerOctave);
            MidiChordDef oppositeMCD = (MidiChordDef)Clone();

            #region conditions
            Debug.Assert(mode.Gamut[0] == oppositeMode.Gamut[0]);
            Debug.Assert(mode.NPitchesPerOctave == oppositeMode.NPitchesPerOctave);
            // N.B. it is not necessarily true that mode.Count == oppositeMode.Count.
            #endregion conditions

            // Substitute the oppositeMCD's pitches by the equivalent pitches in the oppositeMode.
            OppositePitches(mode, oppositeMode, oppositeMCD.NotatedMidiPitches);
            foreach(BasicMidiChordDef bmcd in oppositeMCD.BasicDurationDefs)
            {
                OppositePitches(mode, oppositeMode, bmcd.Pitches);
            }

            return oppositeMCD;
        }

        private void OppositePitches(Mode mode, Mode oppositeMode, List<byte> pitches)
        {
            for(int i = 0; i < pitches.Count; ++i)
            {
                int pitchIndex = mode.IndexInGamut(pitches[i]);
				int oppositeModeGamutCount = oppositeMode.Gamut.Count;
				// N.B. it is not necessarily true that mode.Count == oppositeMode.Count.
				pitchIndex = (pitchIndex < oppositeModeGamutCount) ? pitchIndex : oppositeModeGamutCount - 1;
                pitches[i] = (byte)oppositeMode.Gamut[pitchIndex];
            }
        }
        #endregion Opposite

        #region Invert (shift lowest notes up by one octave)
        /// <summary>
        /// In each chord in this MidiChordDef:
        /// 1. nPitchesToShift is set to nPitchesToShiftArg % nPitches in the chord.
        /// 2. nPitchesToShift pitches are shifted up one octave.
        /// 3. The pitch list is resorted to be in ascending order.
        /// Notes:
        /// a) Velocities do not change. They remain in the same order as they were before this function was called.
        /// b) The pitches thereby remain part of the Mode, if there is one. 
        /// </summary>
        /// <returns></returns>
        public void Invert(int nPitchesToShiftArg)
        {
            #region conditions
            Debug.Assert(nPitchesToShiftArg >= 0);
            #endregion conditions

            foreach(BasicMidiChordDef bmcd in BasicDurationDefs)
            {
                InvertPitches(bmcd.Pitches, nPitchesToShiftArg);
                RemoveDuplicateNotes(bmcd.Pitches, bmcd.Velocities);
            }
            SetNotatedValuesFromFirstBMCD();
        }

        private void InvertPitches(List<byte> pitches, int nPitchesToShiftArg)
        {
            int nPitchesToShift = nPitchesToShiftArg % pitches.Count;
            for(int i = 0; i < nPitchesToShift; ++i)
            {
                byte newPitch = (byte) (pitches[i] + 12);
                newPitch = (newPitch < 127) ? newPitch : (byte)127;
                pitches[i] = newPitch;
            }
            pitches.Sort();
        }
        #endregion Invert (shift lowest notes up by one octave)

        #region Functions that use Envelopes
        /// <summary>
        /// Changes the msPositions of the BasicMidiChordDefs without changing the length of the MidiChordDef. Has no effect on Sliders.
        /// ArgumentExceptions are thrown if BasicMidiChordDefs.Count==0 or distortion is less than 1.
        /// A Debug.Assertion fails if an attempt is made to set a BasicMidiChordDef.MsDuration less than _minimumBasicMidiChordMsDuration. 
        /// See Envelope.TimeWarp() for a description of the arguments.
        /// </summary>
        /// <param name="envelope"></param>
        /// <param name="distortion"></param>
        public void TimeWarp(Envelope envelope, double distortion)
        {
            #region conditions
            if(BasicDurationDefs.Count == 0)
            {
                throw new ArgumentException($"{nameof(BasicDurationDefs)}.Count must be greater than 0.");
            }
            if(distortion < 1)
            {
                throw new ArgumentException($"{nameof(distortion)} may not be less than 1.");
            }
            #endregion conditions

            // if BasicMidiChordDefs.Count == 1, do nothing.
            if(BasicDurationDefs.Count > 1)
            {
                int originalMsDuration = MsDuration;

                List<int> originalPositions = new List<int>();
                #region 1. create originalPositions
                // originalPositions contains the msPositions of the BasicMidiChordDefs re the MidiChordDef
                // plus the end msPosition of the final BasicMidiChordDef.
                int msPos = 0;
                foreach(BasicMidiChordDef bmcd in BasicDurationDefs)
                {
                    originalPositions.Add(msPos);
                    msPos += bmcd.MsDuration;
                }
                originalPositions.Add(msPos); // end position of duration to warp.
                #endregion
                List<int> newPositions = envelope.TimeWarp(originalPositions, distortion);

                for(int i = 0; i < BasicDurationDefs.Count; ++i)
                {
                    BasicDurationDef bmdd = BasicDurationDefs[i];
                    bmdd.MsDuration = newPositions[i + 1] - newPositions[i];
                    Debug.Assert(MinimumBasicMidiChordMsDuration <= bmdd.MsDuration);
                }
            }
        }

		/// <summary>
		/// Calls the other SetOrnament function
		/// </summary>
		/// <param name="mode"></param>
		/// <param name="ornamentShape"></param>
		/// <param name="nOrnamentChords"></param>
		/// <param name="ornamentText">The string that will follow the tilde. Cannot be null or empty.</param>
		public void SetOrnament(Mode mode, IReadOnlyList<byte> ornamentShape, int nOrnamentChords, string ornamentText)
        {
			Debug.Assert(!String.IsNullOrEmpty(ornamentText));
            int nPitchesPerOctave = mode.NPitchesPerOctave;
			Envelope ornamentEnvelope = new Envelope(new List<byte>(ornamentShape), 127, nPitchesPerOctave, nOrnamentChords);
            SetOrnament(mode, ornamentEnvelope, ornamentText);
        }

        /// <summary>
        /// Sets an ornament having the shape and number of elements in the ornamentEnvelope.
		/// The ornament will only contain BasicMidiChordDefs (i.e. no BasicMidiRestDefs)
        /// If ornamentEnvelope == null, BasicMidiChords[0] is set to the NotatedMidiChord using the NotatedMidiPitches as the first chord.
        /// Uses the current Mode.
        /// Replaces any existing ornament.
        /// Sets OrnamentText to ornamentText. Note that OrnamentText is a property that has a private setter.
		/// (usually to a single character).
        /// </summary>
        /// <param name="ornamentEnvelope"></param>
        public void SetOrnament(Mode mode, Envelope ornamentEnvelope = null, string ornamentText = null)
        {
            Debug.Assert(mode != null);
			Debug.Assert((ornamentEnvelope == null && ornamentText == null) || (ornamentEnvelope != null && ornamentText != null));

            List<int> basicMidiChordRootPitches = mode.PitchSequence(_notatedMidiPitches[0], ornamentEnvelope);
            // If ornamentEnvelope is null, basicMidiChordRootPitches will only contain rootNotatedpitch.

            BasicDurationDefs = new List<BasicDurationDef>();
            foreach(int rootPitch in basicMidiChordRootPitches)
            {
                BasicMidiChordDef bmcd = new BasicMidiChordDef(1000, mode, rootPitch, _notatedMidiPitches.Count);
                BasicDurationDefs.Add(bmcd);
            }
            this.MsDuration = _msDuration; // resets the BasicMidiChordDef msDurations.

            if(basicMidiChordRootPitches.Count > 1)
            {
                OrnamentText = ornamentText;
            }
        }

        #region Sliders
        public void SetPitchWheelEnvelope(Envelope envelope)
        {
            SetSliderEnvelope(envelope.Domain, envelope.OriginalAsBytes, null, null, null);
        }
        public void SetPanEnvelope(Envelope envelope)
        {
            SetSliderEnvelope(envelope.Domain, null, envelope.OriginalAsBytes, null, null);
        }
        public void SetModulationWheelEnvelope(Envelope envelope)
        {
            SetSliderEnvelope(envelope.Domain, null, null, envelope.OriginalAsBytes, null);
        }
        public void SetExpressionEnvelope(Envelope envelope)
        {
            SetSliderEnvelope(envelope.Domain, null, null, null, envelope.OriginalAsBytes);
        }
        private void SetSliderEnvelope(int domain, List<byte> pitchWheelBytes, List<byte> panBytes, List<byte> modulationBytes, List<byte> expressionBytes)
        {
            #region condition
            if(domain != 127)
            {
                throw new ArgumentException($"{nameof(domain)} must be 127.");
            }
            #endregion condition

            if(MidiChordSliderDefs == null)
            {
                MidiChordSliderDefs = new MidiChordSliderDefs(null, null, null, null);
            }
            if(pitchWheelBytes != null)
            {
                MidiChordSliderDefs.PitchWheelMsbs = pitchWheelBytes;
            }
            else
            if(panBytes != null)
            {
                MidiChordSliderDefs.PanMsbs = panBytes;
            }
            else
            if(modulationBytes != null)
            {
                MidiChordSliderDefs.ModulationWheelMsbs = modulationBytes;
            }
            else
            if(expressionBytes != null)
            {
                MidiChordSliderDefs.ExpressionMsbs = expressionBytes;
            }
        }
        #endregion Sliders

        #region SetVelocityPerAbsolutePitch
        /// <summary>
        /// The velocityPerAbsolutePitch argument contains a list of 12 velocity values (range [1..127] in order of absolute pitch.
        /// For example: If the MidiChordDef contains one or more C#s, they will be given velocity velocityPerAbsolutePitch[1].
        /// Middle-C is midi pitch 60 (60 % 12 == absolute pitch 0), middle-C# is midi pitch 61 (61 % 12 == absolute pitch 1), etc.
        /// This function applies equally to all the BasicMidiChordDefs in this MidiChordDef. 
        /// </summary>
        /// <param name="velocityPerAbsolutePitch">A list of 12 velocity values (range [1..127] in order of absolute pitch</param>
        public void SetVelocityPerAbsolutePitch(IReadOnlyList<byte> velocityPerAbsolutePitch)
        {
            #region conditions
            Debug.Assert(velocityPerAbsolutePitch.Count == 12);
            for(int i = 0; i < 12; ++i)
            {
                int v = velocityPerAbsolutePitch[i];
                AssertIsVelocityValue(v);
            }
            Debug.Assert(this.NotatedMidiPitches.Count == NotatedMidiVelocities.Count);
            #endregion conditions

            foreach(BasicMidiChordDef bmcd in BasicDurationDefs)
            {
                bmcd.SetVelocityPerAbsolutePitch(velocityPerAbsolutePitch);
            }
            CheckAllBasicMidiChordDefVelocities();
            SetNotatedValuesFromFirstBMCD();
        }

        /// <summary>
        /// Check that all velocities are in range 1..127. 
        /// If a function can set velocities, then it should call this function.
        /// It should probably also call SetNotatedValuesFromFirstBMCD().
        /// </summary>
        private void CheckAllBasicMidiChordDefVelocities()
        {
            foreach(BasicMidiChordDef bmcd in BasicDurationDefs)
            {
                foreach(byte velocity in bmcd.Velocities)
                {
                    AssertIsVelocityValue(velocity);
                }
            }
        }

        private void SetNotatedValuesFromFirstBMCD()
        {
			AssertConsistency(this);

			BasicMidiChordDef firstBMCD = BasicDurationDefs[0] as BasicMidiChordDef;
			_notatedMidiPitches = firstBMCD.Pitches;
			_notatedMidiVelocities = firstBMCD.Velocities;
        }

		#endregion SetVelocityPerAbsolutePitch

		/// <summary>
		/// N.B. This function's behaviour wrt velocities should be changed to that of SetVelocityPerAbsolutePitch() -- see above.
		/// Sets all velocities in the MidiChordDef to a value related to its msDuration.
		/// If percent has its default value 100, the new velocity will be in the same proportion between velocityForMinMsDuration
		/// and velocityForMaxMsDuration as MsDuration is between msDurationRangeMin and msDurationRangeMax.
		/// N.B 1) Both velocityForMinMsDuration and velocityForMaxMsDuration must be in range 1..127.
		/// and 2) velocityForMinMsDuration can be less than, equal to, or greater than velocityForMaxMsDuration
		/// The (optional) percent argument determines the proportion of the final velocity for which this function is responsible.
		/// The other component of the final velocity value is its existing velocity. If percent is 100.0, the existing velocity
		/// is replaced completely.
		/// </summary>
		/// <param name="msDurationRangeMin">less than or equal to the current MsDuration and less than or equal to msDurationRangeMax</param>
		/// <param name="msDurationRangeMax">greater than or equal to the current MsDuration and greater than or equal to msDurationRangeMin</param>
		/// <param name="velocityForMinMsDuration">in range 1..127</param>
		/// <param name="velocityForMaxMsDuration">in range 1..127</param>
		/// <param name="percent">In range 0..100. The proportion of the final velocity value that comes from this function.</param>
		public void SetVelocityFromDuration(int msDurationRangeMin, int msDurationRangeMax, byte velocityForMinMsDuration, byte velocityForMaxMsDuration, double percent = 100.0)
        {
            Debug.Assert(_msDuration >= msDurationRangeMin && _msDuration <= msDurationRangeMax);
            Debug.Assert(msDurationRangeMin <= msDurationRangeMax);
            // velocityForMinMsDuration can be less than, equal to, or greater than velocityForMaxMsDuration
            AssertIsVelocityValue(velocityForMinMsDuration);
            AssertIsVelocityValue(velocityForMaxMsDuration);
            Debug.Assert(percent >= 0 && percent <= 100);

            double factorForNewValue = percent / 100;
            double factorForOldValue = 1 - factorForNewValue;

            double msDurationRange = msDurationRangeMax - msDurationRangeMin;
            double velocityRange = velocityForMaxMsDuration - velocityForMinMsDuration;
            byte newVelocity = velocityForMinMsDuration;
            if(msDurationRange != 0)
            {
                double factor = ((double)(MsDuration - msDurationRangeMin)) / msDurationRange;
                int increment = (int)(factor * velocityRange);
                newVelocity = VelocityValue(velocityForMinMsDuration + increment);
            }

			for(int i = 0; i < BasicDurationDefs.Count; ++i)
			{
				if(BasicDurationDefs[i] is BasicMidiChordDef bmcd)
				{ 
					List<byte> bmcdVelocities = bmcd.Velocities;
					for(int j = 0; j < bmcdVelocities.Count; ++j)
					{
						byte oldVelocity = bmcdVelocities[j];
						byte valueToSet = VelocityValue((int)Math.Round((oldVelocity * factorForOldValue) + (newVelocity * factorForNewValue)));
						bmcdVelocities[j] = valueToSet;
					}
				}
            }

            SetNotatedValuesFromFirstBMCD();
        }

        #region SetVerticalVelocityGradient
        /// <summary>
        /// The arguments must both be in range [1..127].
        /// This function changes the velocities in both the notated chord and the BasicChordDefs.
        /// The velocities of the root and top notes in the chord are set to the argument values, and the other velocities
        /// are interpolated linearly.
        /// The root velocities of all the BasicMidiChordDefs change proportionaly to any change in NotatedMidiVelocities[0].
        /// The verticalVelocityFactor is (((double)topVelocity) / rootVelocity), and is the same for the NotatedMidiVelocities
        /// and all the BasicMidiChordDef velocities. 
        /// </summary>
        public void SetVerticalVelocityGradient(byte rootVelocity, byte topVelocity)
        {
            #region conditions
            AssertIsVelocityValue(rootVelocity);
            AssertIsVelocityValue(topVelocity);
			AssertConsistency(this);
			#endregion conditions

			BasicMidiChordDef firstBMCD = BasicDurationDefs[0] as BasicMidiChordDef;

			List<byte> velocities0 = firstBMCD.Velocities;
            int nVelocities0 = velocities0.Count;
            double increment = (((double)(topVelocity - rootVelocity)) / (nVelocities0 - 1));
            double newVelocity = rootVelocity;
            for(int velocityIndex = 0; velocityIndex < nVelocities0; ++velocityIndex)
            {
                byte newVel = (byte)Math.Round(newVelocity);
                newVel = VelocityValue(newVel);
                velocities0[velocityIndex] = newVel;
                newVelocity += increment;
            }

            double rootVelocityFactor = ((double)rootVelocity) / velocities0[0];
            double verticalVelocityFactor = ((double)topVelocity) / rootVelocity;

            foreach(BasicMidiChordDef bmcd in BasicDurationDefs)
            {
                byte bmcdRootVelocity = VelocityValue((int)(Math.Round(bmcd.Velocities[0] * rootVelocityFactor)));
                byte bmcdTopVelocity = VelocityValue((int)(Math.Round(bmcdRootVelocity * verticalVelocityFactor)));
                bmcd.SetVerticalVelocityGradient(bmcdRootVelocity, bmcdTopVelocity);
            }

            SetNotatedValuesFromFirstBMCD();
        }
        #endregion SetVerticalVelocityGradient

        #region utilities
        /// <summary>
        /// A Debug.Assert that fails if the argument is outside the range 0..127.
        /// </summary>
        private static void AssertIsMidiValue(int value)
        {
            Debug.Assert(value >= 0 && value <= 127);
        }

        /// <summary>
        /// A Debug.Assert that fails if the argument is outside the range 1..127.
        /// </summary>
        private static void AssertIsVelocityValue(int velocity)
        {
            Debug.Assert(velocity >= 1 && velocity <= 127);
        }

        /// <summary>
        /// Returns the argument as a byte coerced to the range 0..127.
        /// </summary>
        private byte MidiValue(int value)
        {
            value = (value >= 0) ? value : 1;
            value = (value <= 127) ? value : 127;
            return (byte)value;
        }

        /// <summary>
        /// Returns the argument as a byte coerced to the range 1..127.
        /// </summary>
        private byte VelocityValue(int velocity)
        {
            velocity = (velocity >= 1) ? velocity : 1;
            velocity = (velocity <= 127) ? velocity : 127;
            return (byte)velocity;
        }
        #endregion utilities

        #endregion Functions that use Envelopes 

        #region IUniqueChordDef
        /// <summary>
        /// Transposes the pitches in NotatedMidiPitches, and all BasicMidiChordDef.Pitches by the number of semitones
        /// given in the argument interval. Negative interval values transpose down.
        /// It is not an error if Midi values would exceed the range 0..127.
        /// In this case, they are silently coerced to 0 or 127 respectively.
        /// If pitches become duplicated at the extremes, the duplicates are removed.
        /// </summary>
        public void Transpose(int interval)
        {
            foreach(BasicMidiChordDef bmcd in BasicDurationDefs)
            {
                List<byte> pitches = bmcd.Pitches;
                List<byte> velocities = bmcd.Velocities;
                for(int i = 0; i < pitches.Count; ++i)
                {
                    pitches[i] = MidiValue(pitches[i] + interval);
                }
                RemoveDuplicateNotes(pitches, velocities);
            }

            SetNotatedValuesFromFirstBMCD();
        }

        /// <summary>
        /// All the pitches in the MidiChordDef must be contained in the mode.
        /// Transposes the pitches in NotatedMidiPitches, and all BasicMidiChordDef.Pitches by
        /// the number of steps in the mode. Negative values transpose down.
        /// The vertical velocity sequence remains unchanged except when notes are removed.
        /// It is not an error if Midi values would exceed the range of the mode.
        /// In this case, they are silently coerced to the bottom or top notes of the mode respectively.
        /// Duplicate top and bottom mode pitches are removed.
        /// </summary>
        public void TransposeStepsInModeGamut(Mode mode, int steps)
        {
            #region conditions
            Debug.Assert(mode != null);
            foreach(BasicMidiChordDef bmcd in BasicDurationDefs)
            {
                foreach(int pitch in bmcd.Pitches)
                {
                    Debug.Assert(mode.Gamut.Contains(pitch));
                }
            }
            #endregion conditions

            int bottomMostPitch = mode.Gamut[0];
            int topMostPitch = mode.Gamut[mode.Gamut.Count - 1];

            foreach(BasicMidiChordDef bmcd in BasicDurationDefs)
            {
                List<byte> pitches = bmcd.Pitches;
                List<byte> velocities = bmcd.Velocities;
                for(int i = 0; i < pitches.Count; ++i)
                {
                    pitches[i] = TransposedPitchInModeGamut(pitches[i], mode, steps);
                }
                RemoveDuplicateNotes(pitches, velocities);
            }

            SetNotatedValuesFromFirstBMCD();
        }

        /// <summary>
        /// The rootPitch and all the pitches in the MidiChordDef must be contained in the mode's Gamut.
        /// The vertical velocity sequence remains unchanged except when notes are removed because they are duplicates.
        /// Calculates the number of steps to transpose, and then calls TransposeStepsInModeGamut.
        /// When this function returns, rootPitch is the lowest pitch in both BasicMidiChordDefs[0] and NotatedMidiPitches.
        /// </summary>
        public void TransposeToRootInModeGamut(Mode mode, int rootPitch)
        {
			AssertConsistency(this);
			BasicMidiChordDef bmcd = BasicDurationDefs[0] as BasicMidiChordDef;

			#region conditions
            Debug.Assert(mode != null);
            Debug.Assert(mode.Gamut.Contains(rootPitch));
			Debug.Assert(mode.Gamut.Contains(bmcd.Pitches[0]));
            #endregion conditions

            int stepsToTranspose = mode.IndexInGamut(rootPitch) - mode.IndexInGamut(bmcd.Pitches[0]);

            // checks that all the pitches are in the mode.
            TransposeStepsInModeGamut(mode, stepsToTranspose);
        }

        private byte TransposedPitchInModeGamut(byte initialPitch, Mode mode, int steps)
        {
            int index = mode.IndexInGamut(initialPitch);
            int newIndex = index + steps;
			int modeGamutCount = mode.Gamut.Count;

			newIndex = (newIndex >= 0) ? newIndex : 0;
            newIndex = (newIndex < modeGamutCount) ? newIndex : modeGamutCount - 1;

            return (byte)mode.Gamut[newIndex];
        }

        private void RemoveDuplicateNotes(List<byte> pitches, List<byte> velocities)
        {
            Debug.Assert(pitches.Count == velocities.Count);
            pitches.Sort(); // just to be sure
            List<int> indicesOfPitchesToRemove = new List<int>();
            for(int i = 1; i < pitches.Count; ++i)
            {
                if(pitches[i] == pitches[i-1])
                {
                    indicesOfPitchesToRemove.Add(i);
                }
            }
            for(int i = pitches.Count - 1; i > 0; --i)
            {
                if(indicesOfPitchesToRemove.Contains(i))
                {
                    pitches.RemoveAt(i);
                    velocities.RemoveAt(i);
                }
            }
            Debug.Assert(pitches.Count == velocities.Count);
        }

		/// <summary>
		/// Multiplies the velocities in NotatedMidiVelocities, and all BasicMidiChordDef.Velocities by the argument factor.
		/// The resulting velocities are in range 1..127.
		/// If a resulting velocity would have been less than 1, it is silently coerced to 1.
		/// If it would have been greater than 127, it is silently coerced to 127.
		/// </summary>
		/// <param name="factor">greater than 0</param>
		public void AdjustVelocities(double factor)
		{
			Debug.Assert(factor > 0.0);
			foreach(BasicMidiChordDef bmcd in BasicDurationDefs)
			{
				bmcd.AdjustVelocities(factor);
			}
			CheckAllBasicMidiChordDefVelocities();
			SetNotatedValuesFromFirstBMCD();
		}

		/// <summary>
		/// Velocities having originalVelocity are changed to newVelocity.
		/// Velocity values above originalVelocity are changed proportionally with max possible velocity at 127.
		/// Velocity values below originalVelocity are changed proportionally with min possible velocity at 1.
		/// </summary>
		/// <param name="factor">greater than 0</param>
		public void AdjustVelocities(byte originalVelocity, byte newVelocity)
		{
			AssertIsVelocityValue(originalVelocity);
			AssertIsVelocityValue(newVelocity);

			foreach(BasicMidiChordDef bmcd in BasicDurationDefs)
			{
				bmcd.AdjustVelocities(originalVelocity, newVelocity);
			}
			CheckAllBasicMidiChordDefVelocities();
			SetNotatedValuesFromFirstBMCD();
		}

		/// <summary>
		/// Truncates the number of notes in all BasicMidiChordDefs by removing the upper notes as necessary.
		/// If the original number of notes in a BasicMidiChordDef is less than newDensity, that bmcd is not changed.
		/// </summary>
		/// <param name="newDensity">Must be greater than 0</param>
		public void SetVerticalDensity(int newDensity)
		{
            Debug.Assert(newDensity > 0);

            foreach(BasicMidiChordDef bmcd in BasicDurationDefs)
            {
                int nElementsToRemove = bmcd.Pitches.Count - newDensity;
                if(nElementsToRemove > 0)
                {
                    bmcd.Pitches.RemoveRange(newDensity, nElementsToRemove);
                    bmcd.Velocities.RemoveRange(newDensity, nElementsToRemove);
                }
            }
            SetNotatedValuesFromFirstBMCD();
        }

        #region IUniqueDef
        public override string ToString() => $"MidiChordDef: MsPositionReFirstIUD={MsPositionReFirstUD} MsDuration={MsDuration} BasePitch={NotatedMidiPitches[0]}";

        /// <summary>
        /// Multiplies the MsDuration by the given factor.
        /// </summary>
        /// <param name="factor"></param>
        public void AdjustMsDuration(double factor)
        {
            MsDuration = (int)(_msDuration * factor);
            Debug.Assert(MsDuration > 0, "A UniqueDef's MsDuration may not be set to zero!");
        }

        #endregion IUniqueDef
        #endregion IUniqueChordDef

        /// <summary>
        /// Returns a new Trk having msDuration and midiChannel, whose MidiChordDefs are created from this MidiChordDef's BasicMidiChordDefs.
        /// If a non-null mode argument is given, a check is made (in the Trk constructor) to ensure that it contains
        /// all the pitches (having velocity greater than 1) in this MidiChordDef.
        /// Each new MidiChordDef has one BasicMidiChordDef, which is a copy of a BasicMidiChordDef from this MidiChordDef.
        /// The new MidiChordDef's notated pitches and velocities are the same as its BasicMidiChordDef's.
        /// The durations of the returned MidiChordDefs are in proportion to the durations of the original BasicMidichordDefs.
        /// This MidiChordDef is not changed by calling this function.
        /// </summary>
        /// <param name="msDuration">The duration of the returned Trk</param>
        /// <param name="midiChannel">The channel of the returned Trk</param>
        public Trk ToTrk(int msDuration, int midiChannel)
        {
            List<IUniqueDef> iuds = new List<IUniqueDef>();
            foreach(BasicMidiChordDef bmcd in this.BasicDurationDefs)
            {
                // this constructor checks that pitches are in range 0..127, and velocities are in range 1..127.
                MidiChordDef mcd = new MidiChordDef(bmcd.Pitches, bmcd.Velocities, bmcd.MsDuration, bmcd.HasChordOff);
                mcd.FirstBasicDurationDef.BankIndex = bmcd.BankIndex;
                mcd.FirstBasicDurationDef.PatchIndex = bmcd.PatchIndex;
                mcd.FirstBasicDurationDef.HasChordOff = bmcd.HasChordOff;
                iuds.Add(mcd);
            }

            Trk trk = new Trk(midiChannel, 0, iuds)
			{
				MsDuration = msDuration	// calls Trk.SetMsPositionsReFirstUD();
			};
            
            return trk;
        }

        public void AdjustExpression(double factor)
        {
            List<byte> exprs = this.MidiChordSliderDefs.ExpressionMsbs;
            for(int i = 0; i < exprs.Count; ++i)
            {
                exprs[i] = MidiValue((int)(exprs[i] * factor));
            }
        }

        public List<byte> PanMsbs
        {
            get
            {
                List<byte> rval;
                if(this.MidiChordSliderDefs == null || this.MidiChordSliderDefs.PanMsbs == null)
                {
                    rval = new List<byte>();
                }
                else
                {
                    rval = this.MidiChordSliderDefs.PanMsbs;
                }
                return rval;
            }
            set
            {
                if(this.MidiChordSliderDefs == null)
                {
                    this.MidiChordSliderDefs = new MidiChordSliderDefs(new List<byte>(), new List<byte>(), new List<byte>(), new List<byte>());
                }
                if(this.MidiChordSliderDefs.PanMsbs == null)
                {
                    this.MidiChordSliderDefs.PanMsbs = new List<byte>();
                }
                List<byte> pans = this.MidiChordSliderDefs.PanMsbs;
                pans.Clear();
                for(int i = 0; i < value.Count; ++i)
                {
                    pans.Add(MidiValue((int)(value[i])));
                }
            }
        }

        public void AdjustModulationWheel(double factor)
        {
            List<byte> modWheels = this.MidiChordSliderDefs.ModulationWheelMsbs;
            for(int i = 0; i < modWheels.Count; ++i)
            {
                modWheels[i] = MidiValue((int)(modWheels[i] * factor));
            }
        }

        public void AdjustPitchWheel(double factor)
        {
            List<byte> pitchWheels = this.MidiChordSliderDefs.PitchWheelMsbs;
            for(int i = 0; i < pitchWheels.Count; ++i)
            {
                pitchWheels[i] = MidiValue((int)(pitchWheels[i] * factor));
            }
        }

        /// <summary>
        /// Note that neither OutputRests nor OutputChords have a msDuration attribute.
        /// Their msDuration is deduced from the contained moment msDurations.
        /// See: https://github.com/notator/Moritz/issues/2
        /// </summary>
        public void WriteSVG(SvgWriter w, int channel, CarryMsgs carryMsgs)
        {
            #region set BasicMidiChordDefs[0] Bank, Patch and PitchWheelDeviation if necessary
            Debug.Assert(BasicDurationDefs != null && BasicDurationDefs.Count > 0);

            if(FirstBasicDurationDef.BankIndex == null && Bank != null)
            {
                FirstBasicDurationDef.BankIndex = Bank;
            }
            if(FirstBasicDurationDef.PatchIndex == null && Patch != null)
            {
                FirstBasicDurationDef.PatchIndex = Patch;
            }
            if(FirstBasicDurationDef.PitchWheelDeviation == null && PitchWheelDeviation != null)
            {
                FirstBasicDurationDef.PitchWheelDeviation = PitchWheelDeviation;
            }
            #endregion

            w.WriteStartElement("score", "midi", null);

            w.WriteStartElement("moments");

            foreach(BasicMidiChordDef bmcd in BasicDurationDefs)
            {
                // writes a single moment element which may contain
                // noteOffs, bank, patch, pitchWheelDeviation and noteOns elements 
                bmcd.WriteSVG(w, channel, carryMsgs);
            }

            w.WriteEndElement(); // end moments

            if(MidiChordSliderDefs != null)
            {
                // writes the envs element
                MidiChordSliderDefs.WriteSVG(w, channel, this.MsDuration, carryMsgs);
            }

            w.WriteEndElement(); // score:midi

            if(HasChordOff)
            {
                List<byte> hangingPitches = GetHangingPitches(BasicDurationDefs);
                if(hangingPitches.Count > 0)
                {
                    List<MidiMsg> noteOffMsgs = GetNoteOffMsgs(channel, hangingPitches);
                    carryMsgs.AddRange(noteOffMsgs);
                }
            }
        }

        private List<byte> GetHangingPitches(List<BasicDurationDef> basicDurationDefs)
        {
            List<byte> hangingPitches = new List<byte>();
            foreach(BasicDurationDef bmdd in basicDurationDefs)
            {
                if(bmdd is BasicMidiChordDef bmcd && !bmcd.HasChordOff)
                {
                    foreach(byte pitch in bmcd.Pitches)
                    {
                        if(!hangingPitches.Contains(pitch))
                        {
                            hangingPitches.Add(pitch);
                        }
                    }
                }
            }
            return hangingPitches;
        }

        private List<MidiMsg> GetNoteOffMsgs(int channel, List<byte> hangingPitches)
        {
            List<MidiMsg> noteOffMsgs = new List<MidiMsg>();
            foreach(byte pitch in hangingPitches)
            {
                MidiMsg msg = new MidiMsg(M.CMD_NOTE_OFF_0x80 + channel, pitch, M.DEFAULT_NOTEOFF_VELOCITY_64);
                noteOffMsgs.Add(msg);
            }
            return noteOffMsgs;
        }

        private static List<int> GetBasicDurations(List<BasicDurationDef> bdds)
        {
            List<int> returnList = new List<int>();
            foreach(BasicDurationDef bdd in bdds)
            {
                returnList.Add(bdd.MsDuration);
            }
            return returnList;
        }

		///// <summary>
		///// This function returns the maximum number of ornament chords that can be fit into the given msDuration
		///// using the given relativeDurations and minimumOrnamentChordMsDuration.
		///// </summary>
		//private static int GetNumberOfBasicDurations(int msDuration, List<int> relativeDurations, int minimumOrnamentChordMsDuration)
		//{
		//    bool okay = true;
		//    int numberOfOrnamentChords = 1;
		//    float factor = 1.0F;
		//    // try each ornament length in turn until okay is true
		//    for(int numChords = relativeDurations.Count; numChords > 0; --numChords)
		//    {
		//        okay = true;
		//        int sum = 0;
		//        for(int i = 0; i < numChords; ++i)
		//            sum += relativeDurations[i];
		//        factor = ((float)msDuration / (float)sum);

		//        for(int i = 0; i < numChords; ++i)
		//        {
		//            if((relativeDurations[i] * factor) < (float)minimumOrnamentChordMsDuration)
		//                okay = false;
		//        }
		//        if(okay)
		//        {
		//            numberOfOrnamentChords = numChords;
		//            break;
		//        }
		//    }
		//    Debug.Assert(okay);
		//    return numberOfOrnamentChords;
		//}

		/// <summary>
		/// Returns a list of (millisecond) durations whose sum is outerMsDuration.
		/// The first msDuration in the returned list is guaranteed not to be 0, but otherwise an msDuration
		/// that would have been less than minimumOutputMsDuration will be 0 in the returned list.
		/// </summary>
		/// <param name="outerMsDuration"></param>
		/// <param name="relativeDurations">Must contain at least one value.</param>
		/// <param name="minimumOutputMsDuration"></param>
		/// <returns></returns>
		private static List<int> GetDurations(int outerMsDuration, List<int> relativeDurations, int minimumOutputMsDuration)
        {
			Debug.Assert(relativeDurations.Count > 0);
			if(relativeDurations.Count == 1)
			{
				Debug.Assert(outerMsDuration >= minimumOutputMsDuration);
			}

			List<int> intDurations = M.IntDivisionSizes(outerMsDuration, relativeDurations);

			for(int i = 0; i < intDurations.Count; ++i)
			{
				int msDur = intDurations[i];
				if(msDur < minimumOutputMsDuration)
				{
					if(i == 0)
					{ 
						int diff = minimumOutputMsDuration - intDurations[0];
						intDurations[0] = minimumOutputMsDuration; // the first duration may not be zero!
						intDurations[1] -= diff;
					}
					else
					{
						intDurations[i - 1] += intDurations[i];
						intDurations[i] = 0;
					}
				}
			}

            return intDurations;
        }

		/// <summary>
		/// Returns a new list of BasicDurationDefs having the msOuterDuration, shortening the list if necessary.
		/// </summary>
		/// <param name="basicDurationDefs"></param>
		/// <param name="outerMsDuration"></param>
		/// <param name="minimumMsDuration"></param>
		/// <returns></returns>
		public static List<BasicDurationDef> FitToDuration(List<BasicDurationDef> basicDurationDefs, int outerMsDuration, int minimumMsDuration)
        {
            List<int> originalDurations = GetBasicDurations(basicDurationDefs);

            List<int> msDurations = GetDurations(outerMsDuration, originalDurations, minimumMsDuration);

			Debug.Assert(originalDurations.Count == msDurations.Count);
			Debug.Assert(msDurations[0] > 0);

            List<BasicDurationDef> rList = new List<BasicDurationDef>();
            
            for(int i = 0; i < msDurations.Count; ++i)
            {
				BasicDurationDef bdd = basicDurationDefs[i];
				int msDuration = msDurations[i];
				// Only create an output BasicDurationDef if its duration is going to be > 0ms.
				if(msDuration > 0)
				{
					if(bdd is BasicMidiChordDef bmcd)
					{
						// constructor checks that pitches are in range 0..127, and velocities are in range 1..127.
						rList.Add(new BasicMidiChordDef(msDurations[i], bmcd.BankIndex, bmcd.PatchIndex, bmcd.HasChordOff, bmcd.Pitches, bmcd.Velocities));
					}
					else if(bdd is BasicMidiRestDef bmrd)
					{
						rList.Add(new BasicMidiRestDef(msDurations[i]));
					}
					else
					{
						throw new ApplicationException("Unknown BasicDurationDef type.");
					}
				}
            }

            return rList;
        }

		/// <summary>
		/// This version for use in legacy Palette code.
		/// </summary>
		/// <param name="basicMidiChordDefs"></param>
		/// <param name="outerMsDuration"></param>
		/// <param name="minimumMsDuration"></param>
		/// <returns></returns>
		public static List<BasicMidiChordDef> FitToDuration(List<BasicMidiChordDef> basicMidiChordDefs, int outerMsDuration, int minimumMsDuration)
		{
			List<BasicDurationDef> basicDurationDefs = new List<BasicDurationDef>();
			foreach(BasicMidiChordDef bmcd in basicMidiChordDefs)
			{
				basicDurationDefs.Add(bmcd);
			}

			List<BasicDurationDef> bdds = FitToDuration(basicDurationDefs, outerMsDuration, minimumMsDuration);

			List<BasicMidiChordDef> bmcds = new List<BasicMidiChordDef>();
			foreach(BasicDurationDef bdd in bdds)
			{
				if(bdd is BasicMidiChordDef bmcd)
				{
					bmcds.Add(bmcd);
				}
				else
				{
					throw new ApplicationException("error: bdd must be a BasicMidiChordDef here.");
				}
			}

			return bmcds;
		}
		#region properties

		public List<int> BasicDurations
        {
            get
            {
                List<int> rList = new List<int>();
                foreach(BasicDurationDef bdd in BasicDurationDefs)
                {
                    rList.Add(bdd.MsDuration);
                }
                return rList;
            }
        }

		public BasicMidiChordDef FirstBasicDurationDef
		{
			get
			{
				AssertConsistency(this);

				return BasicDurationDefs[0] as BasicMidiChordDef;
			}
		}

		/****************************************************************************/

		public int MsPositionReFirstUD { get; set; } = 0;
		public override int MsDuration
        {
            get
            {
                return _msDuration;
            }
            set
            {
                Debug.Assert(BasicDurationDefs != null && BasicDurationDefs.Count > 0);

                _msDuration = value;
                int sumDurations = 0;
                foreach(int bcd in BasicDurations)
                    sumDurations += bcd;
                if(_msDuration != sumDurations)
                {
                    BasicDurationDefs = FitToDuration(BasicDurationDefs, _msDuration, MinimumBasicMidiChordMsDuration);
                }

				AssertConsistency(this);
			}
        }
		public int? MsDurationToNextBarline { get; set; } = null;
		public byte? Bank { get; set; } = null;
		public byte? Patch { get; set; } = null;
		public byte? PitchWheelDeviation
        {
            get
            {
                return _pitchWheelDeviation;
            }
            set
            {
                if(value == null)
                    _pitchWheelDeviation = null;
                else
                {
                    int val = (int)value;
                    _pitchWheelDeviation = MidiValue(val);
                }
            }
        }
        private byte? _pitchWheelDeviation = null;
		public bool HasChordOff { get; set; } = true;
		public bool BeamContinues { get; set; } = true;
		public string Lyric { get; set; } = null;
		public int MinimumBasicMidiChordMsDuration { get; set; } = 1;

		/// <summary>
		/// This NotatedMidiPitches field is used when displaying the chord's noteheads.
		/// Setting this field creates a clone of the supplied list, and does not affect the pitches in the BasicMidiChordDefs.
		/// </summary>
		public List<byte> NotatedMidiPitches
        { 
            get { return _notatedMidiPitches; } 
            set 
            {
                // N.B. this value can be set even if value.Count != _notatedMidiVelocities.Count
                // If the Count is changed, the _notatedMidiVelocities must subsequently be reset,
                // otherwise a Debug.Assert will fail when the _notatedMidiVelocities are retrieved.
                foreach(byte pitch in value)
                {
                    AssertIsMidiValue(pitch);
                }
                _notatedMidiPitches = new List<byte>(value);
            } 
        }
        private List<byte> _notatedMidiPitches = null;

        /// <summary>
        /// This NotatedMidiVelocities field is used when displaying the chord's noteheads.
        /// Setting this field creates a clone of the supplied list, and does not affect the velocities in the BasicMidiChordDefs.
        /// </summary>
        public List<byte> NotatedMidiVelocities
        {
            get
            {
                Debug.Assert(_notatedMidiVelocities.Count == _notatedMidiPitches.Count);
                return _notatedMidiVelocities;
            }
            set
            {
                Debug.Assert(value.Count == _notatedMidiPitches.Count);
                foreach(byte velocity in value)
                {
                    AssertIsVelocityValue(velocity);
                }
                _notatedMidiVelocities = new List<byte>(value);
            }
        }
        private List<byte> _notatedMidiVelocities = null;

		/// <summary>
		/// If not null, this string is printed after a tilde, above or below the chord.
		/// </summary>
		public string OrnamentText { get; private set; } = null;

		public MidiChordSliderDefs MidiChordSliderDefs = null;
        public List<BasicDurationDef> BasicDurationDefs = new List<BasicDurationDef>();
		/// <summary>
		/// Returns a list of the BasicMidiChordDefs in BasicDurationDefs, simply ignoring any BasicMidiRestDefs.
		/// </summary>
		public List<BasicMidiChordDef> BasicMidiChordDefs
		{
			get
			{
				List<BasicMidiChordDef> bmcds = new List<BasicMidiChordDef>();
				foreach(BasicDurationDef bdd in BasicDurationDefs)
				{
					if(bdd is BasicMidiChordDef bmcd)
					{
						bmcds.Add(bmcd);
					}
				}
				return bmcds;
			}
		}

		#endregion properties
	}
}
