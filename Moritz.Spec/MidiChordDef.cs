using System;
using System.Diagnostics;
using System.Collections.Generic;

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

            _msPositionReFirstIUD = 0; // default value
            _hasChordOff = hasChordOff;
            _minimumBasicMidiChordMsDuration = 1; // not used (this is not an ornament)

            _ornamentNumberSymbol = 0;

            MidiChordSliderDefs = null;

            byte? bank = null;                                                                           
            byte? patch = null;

            BasicMidiChordDefs.Add(new BasicMidiChordDef(msDuration, bank, patch, hasChordOff, pitches, velocities));

            SetNotatedValuesFromFirstBMCD();

            CheckTotalDuration();
        }

        /// <summary>
        /// A MidiChordDef having msDuration, and containing an ornament having BasicMidiChordDefs with nPitchesPerChord.
        /// The notated pitch and the pitch of BasicMidiChordDefs[0] are set to rootNotatedPitch.
        /// The notated velocity of all pitches is set to 127.
        /// The root pitches of the BasicMidiChordDefs begin with rootNotatedPitch, and follow the ornamentEnvelope, using
        /// the ornamentEnvelope's values as indices in the gamut. Their durations are as equal as possible, to give the
        /// overall msDuration. If ornamentEnvelope is null, a single, one-note BasicMidiChordDef will be created.
        /// This constructor uses Gamut.GetChord(rootNotatedPitch, nPitchesPerChord) which returns pitches that are
        /// vertically spaced differently according to the absolute height of the rootNotatedPitch. The number of pitches
        /// in a chord may also be less than nPitchesPerChord (see gamut.GetChord(...) ).
        /// An exception is thrown if rootNotatedPitch is not in the gamut.
        /// </summary>        
        /// <param name="msDuration">The duration of this MidiChordDef</param>
        /// <param name="gamut">The gamut containing all the pitches.</param>
        /// <param name="rootNotatedPitch">The lowest notated pitch. Also the lowest pitch of BasicMidiChordDefs[0].</param>
        /// <param name="nPitchesPerChord">The chord density (some chords may have less pitches).</param>
        /// <param name="ornamentEnvelope">The ornament definition.</param>
        public MidiChordDef(int msDuration, Gamut gamut, int rootNotatedPitch, int nPitchesPerChord, Envelope ornamentEnvelope = null)
            : base(msDuration) 
        {
            NotatedMidiPitches = gamut.GetChord(rootNotatedPitch, nPitchesPerChord);
            var nmVelocities = new List<byte>();
            foreach(byte pitch in NotatedMidiPitches) // can be less than nPitchesPerChord
            {
                nmVelocities.Add(127);
            }
            NotatedMidiVelocities = nmVelocities;

            // Sets BasicMidiChords. If ornamentEnvelope == null, BasicMidiChords[0] is set to the NotatedMidiChord.
            SetOrnament(gamut, ornamentEnvelope);
        }

        /// <summary>
        /// Constructor used when creating a list of DurationDef templates from a Palette.
        /// The palette has created new values for all the arguments, so this constructor simply transfers
        /// those values to the new MidiChordDef. MsPositionReFirstIUD is set to 0, lyric is set to null.
        /// </summary>
        public MidiChordDef(
            int msDuration, // the total duration (this should be the sum of the durations of the basicMidiChordDefs)
            byte pitchWheelDeviation, // default is M.DefaultPitchWheelDeviation (=2)
            bool hasChordOff, // default is M.DefaultHasChordOff (=true)
            List<byte> rootMidiPitches, // the pitches defined in the root chord settings (displayed, by default, in the score).
            List<byte> rootMidiVelocities, // the velocities defined in the root chord settings (displayed, by default, in the score).
            int ornamentNumberSymbol, // is 0 when there is no ornament
            MidiChordSliderDefs midiChordSliderDefs, // can be null or contain empty lists
            List<BasicMidiChordDef> basicMidiChordDefs)
            : base(msDuration)
        {
            Debug.Assert(rootMidiPitches.Count <= rootMidiVelocities.Count);
            foreach(byte pitch in rootMidiPitches)
            {
                AssertIsMidiValue(pitch);
            }

            _msPositionReFirstIUD = 0;
            _msDuration = msDuration;
            _pitchWheelDeviation = pitchWheelDeviation;
            _hasChordOff = hasChordOff;
            _notatedMidiPitches = rootMidiPitches;
            _notatedMidiVelocities = rootMidiVelocities;

            _ornamentNumberSymbol = ornamentNumberSymbol;

            MidiChordSliderDefs = midiChordSliderDefs;
            BasicMidiChordDefs = basicMidiChordDefs;

            CheckTotalDuration();
        }

        /// <summary>
        /// private constructor -- used by Clone()
        /// </summary>
        private MidiChordDef()
            : base(0)
        {
        }
        #endregion constructors

        private void CheckTotalDuration()
        {
            List<int> basicChordDurations = BasicChordDurations;
            int sumDurations = 0;
            foreach(int bcd in basicChordDurations)
                sumDurations += bcd;
            Debug.Assert(_msDuration == sumDurations);
        }

        #region Clone
        /// <summary>
        /// A deep clone!
        /// </summary>
        /// <returns></returns>
        public override object Clone()
        {
            MidiChordDef rval = new MidiChordDef();

            rval.MsPositionReFirstUD = this.MsPositionReFirstUD;
            // rval.MsDuration must be set after setting BasicMidiChordDefs See below.
            rval.Bank = this.Bank;
            rval.Patch = this.Patch;
            rval.PitchWheelDeviation = this.PitchWheelDeviation;
            rval.HasChordOff = this.HasChordOff;
            rval.BeamContinues = this.BeamContinues;
            rval.Lyric = this.Lyric;
            rval.MinimumBasicMidiChordMsDuration = MinimumBasicMidiChordMsDuration; // required when changing a midiChord's duration
            rval.NotatedMidiPitches = _notatedMidiPitches; // a clone of the displayed notehead pitches
            rval.NotatedMidiVelocities = _notatedMidiVelocities; // a clone of the displayed notehead velocities

            // rval.MidiVelocity must be set after setting BasicMidiChordDefs See below.
            rval.OrnamentNumberSymbol = this.OrnamentNumberSymbol; // the displayed ornament number

			rval.MidiChordSliderDefs = null;
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

            List<BasicMidiChordDef> newBs = new List<BasicMidiChordDef>();
            foreach(BasicMidiChordDef b in BasicMidiChordDefs)
            {
                List<byte> pitches = new List<byte>(b.Pitches);
                List<byte> velocities = new List<byte>(b.Velocities);
                newBs.Add(new BasicMidiChordDef(b.MsDuration, b.BankIndex, b.PatchIndex, b.HasChordOff, pitches, velocities));
            }
            rval.BasicMidiChordDefs = newBs;
            rval.MsDuration = this.MsDuration;

           return rval;
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
        /// 1. Creates a new, opposite gamut from the argument Gamut (see Gamut.Opposite()).
        /// 2. Clones this MidiChordDef, and replaces the clone's pitches by the equivalent pitches in the opposite Gamut.
        /// 3. Returns the clone.
        /// </summary>
        public MidiChordDef Opposite(Gamut gamut)
        {
            #region conditions
            Debug.Assert(gamut != null);
            #endregion conditions

            int relativePitchHierarchyIndex = (gamut.RelativePitchHierarchyIndex + 11) % 22;
            Gamut oppositeGamut = new Gamut(relativePitchHierarchyIndex, gamut.BasePitch, gamut.NPitchesPerOctave);
            MidiChordDef oppositeMCD = (MidiChordDef)Clone();

            #region conditions
            Debug.Assert(gamut[0] == oppositeGamut[0]);
            Debug.Assert(gamut.NPitchesPerOctave == oppositeGamut.NPitchesPerOctave);
            // N.B. it is not necessarily true that gamut.Count == oppositeGamut.Count.
            #endregion conditions

            // Substitute the oppositeMCD's pitches by the equivalent pitches in the oppositeGamut.
            OppositePitches(gamut, oppositeGamut, oppositeMCD.NotatedMidiPitches);
            foreach(BasicMidiChordDef bmcd in oppositeMCD.BasicMidiChordDefs)
            {
                OppositePitches(gamut, oppositeGamut, bmcd.Pitches);
            }

            return oppositeMCD;
        }

        private void OppositePitches(Gamut gamut, Gamut oppositeGamut, List<byte> pitches)
        {
            for(int i = 0; i < pitches.Count; ++i)
            {
                int pitchIndex = gamut.IndexOf(pitches[i]);
                // N.B. it is not necessarily true that gamut.Count == oppositeGamut.Count.
                pitchIndex = (pitchIndex < oppositeGamut.Count) ? pitchIndex : oppositeGamut.Count - 1;
                pitches[i] = (byte)oppositeGamut[pitchIndex];
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
        /// b) The pitches thereby remain part of the Gamut, if there is one. 
        /// </summary>
        /// <returns></returns>
        public void Invert(int nPitchesToShiftArg)
        {
            #region conditions
            Debug.Assert(nPitchesToShiftArg >= 0);
            #endregion conditions

            foreach(BasicMidiChordDef bmcd in BasicMidiChordDefs)
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
            if(BasicMidiChordDefs.Count == 0)
            {
                throw new ArgumentException($"{nameof(BasicMidiChordDefs)}.Count must be greater than 0.");
            }
            if(distortion < 1)
            {
                throw new ArgumentException($"{nameof(distortion)} may not be less than 1.");
            }
            #endregion conditions

            // if BasicMidiChordDefs.Count == 1, do nothing.
            if(BasicMidiChordDefs.Count > 1)
            {
                int originalMsDuration = MsDuration;

                List<int> originalPositions = new List<int>();
                #region 1. create originalPositions
                // originalPositions contains the msPositions of the BasicMidiChordDefs re the MidiChordDef
                // plus the end msPosition of the final BasicMidiChordDef.
                int msPos = 0;
                foreach(BasicMidiChordDef bmcd in BasicMidiChordDefs)
                {
                    originalPositions.Add(msPos);
                    msPos += bmcd.MsDuration;
                }
                originalPositions.Add(msPos); // end position of duration to warp.
                #endregion
                List<int> newPositions = envelope.TimeWarp(originalPositions, distortion);

                for(int i = 0; i < BasicMidiChordDefs.Count; ++i)
                {
                    BasicMidiChordDef bmcd = BasicMidiChordDefs[i];
                    bmcd.MsDuration = newPositions[i + 1] - newPositions[i];
                    Debug.Assert(_minimumBasicMidiChordMsDuration <= bmcd.MsDuration);
                }
            }
        }

        /// <summary>
        /// Calls the other SetOrnament function
        /// </summary>
        /// <param name="ornamentShape"></param>
        /// <param name="nOrnamentChords"></param>
        public void SetOrnament(Gamut gamut, IReadOnlyList<byte> ornamentShape, int nOrnamentChords)
        {
            int nPitchesPerOctave = gamut.NPitchesPerOctave;
            Envelope ornamentEnvelope = new Envelope(ornamentShape, 127, nPitchesPerOctave, nOrnamentChords);
            SetOrnament(gamut, ornamentEnvelope);
        }

        /// <summary>
        /// Sets an ornament having the shape and number of elements in the ornamentEnvelope.
        /// If ornamentEnvelope == null, BasicMidiChords[0] is set to the NotatedMidiChord.
        /// using the NotatedMidiPitches as the first chord.
        /// Uses the current Gamut.
        /// Replaces any existing ornament.
        /// Sets the OrnamentNumberSymbol to the number of BasicMidiChordDefs.
        /// </summary>
        /// <param name="ornamentEnvelope"></param>
        public void SetOrnament(Gamut gamut, Envelope ornamentEnvelope)
        {
            Debug.Assert(gamut != null);
            List<int> basicMidiChordRootPitches = gamut.PitchSequence(_notatedMidiPitches[0], ornamentEnvelope);
            // If ornamentEnvelope is null, basicMidiChordRootPitches will only contain rootNotatedpitch.

            BasicMidiChordDefs = new List<BasicMidiChordDef>();
            foreach(int rootPitch in basicMidiChordRootPitches)
            {
                BasicMidiChordDef bmcd = new BasicMidiChordDef(1000, gamut, rootPitch, _notatedMidiPitches.Count);
                BasicMidiChordDefs.Add(bmcd);
            }
            this.MsDuration = _msDuration; // resets the BasicMidiChordDef msDurations.

            if(basicMidiChordRootPitches.Count > 1)
            {
                _ornamentNumberSymbol = basicMidiChordRootPitches.Count;
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
        /// Argument 1 (velocityPerAbsolutePitch) contains a list of 12 velocity values (range [1..127] in order of absolute pitch.
        /// Argument 2 (minimumVelocity) is the used to raise the values in (a copy of) argument 1: minimumVelocity is the value
        /// used wherever there is a smaller value in argument 1. The other values are raised proportionally.
        /// Argument 3 (optional) determines the proportion of the final velocity determined by this function.
        /// The other component is the existing velocity. If percent is 100.0, the existing velocity is replaced completely.
        /// For example: If the MidiChordDef contains one or more C#s, they will be given velocity velocityPerAbsolutePitch[1].
        /// Middle-C is midi pitch 60 (60 % 12 == absolute pitch 0), middle-C# is midi pitch 61 (61 % 12 == absolute pitch 1), etc.
        /// This function applies equally to all the BasicMidiChordDefs in this MidiChordDef. 
        /// </summary>
        /// <param name="velocityPerAbsolutePitch">A list of 12 velocity values (range [1..127] in order of absolute pitch</param>
        /// <param name="minimumVelocity">In range 1..127</param>
        /// <param name="percent">In range 0..100. The proportion of the final velocity value that comes from this function.</param>
        public void SetVelocityPerAbsolutePitch(List<byte> velocityPerAbsolutePitch, byte minimumVelocity, double percent = 100.0)
        {
            #region conditions
            Debug.Assert(velocityPerAbsolutePitch.Count == 12);
            for(int i = 0; i < 12; ++i)
            {
                int v = velocityPerAbsolutePitch[i];
                AssertIsVelocityValue(v);
            }
            AssertIsVelocityValue(minimumVelocity);
            Debug.Assert(percent >= 0 && percent <= 100);
            Debug.Assert(this.NotatedMidiPitches.Count == NotatedMidiVelocities.Count);
            #endregion conditions

            List<byte> localVelocityPerAbsolutePitch = new List<byte>(velocityPerAbsolutePitch);
            if(minimumVelocity > 1)
            {
                #region reset localVelocityPerAbsolutePitch
                double tan = ((double)127 - minimumVelocity) / 127;
                for(int i = 0; i < localVelocityPerAbsolutePitch.Count; ++i)
                {
                    byte value = localVelocityPerAbsolutePitch[i];
                    if(value < minimumVelocity)
                    {
                        value = minimumVelocity;
                    }
                    else
                    {
                        value = VelocityValue((int)Math.Round((minimumVelocity + (value * tan))));                      
                    }                   
                    localVelocityPerAbsolutePitch[i] = value;
                }
                #endregion reset localVelocityPerAbsolutePitch
            }

            foreach(BasicMidiChordDef bmcd in BasicMidiChordDefs)
            {
                bmcd.SetVelocityPerAbsolutePitch(localVelocityPerAbsolutePitch, percent);
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
            foreach(BasicMidiChordDef bmcd in BasicMidiChordDefs)
            {
                foreach(byte velocity in bmcd.Velocities)
                {
                    AssertIsVelocityValue(velocity);
                }
            }
        }

        private void SetNotatedValuesFromFirstBMCD()
        {
            BasicMidiChordDef firstBMC = BasicMidiChordDefs[0];

            _notatedMidiPitches.Clear();
            _notatedMidiVelocities.Clear();
            if(firstBMC != null)
            {
                _notatedMidiPitches.AddRange(firstBMC.Pitches);
                _notatedMidiVelocities.AddRange(firstBMC.Velocities);
            }
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

            for(int i = 0; i < BasicMidiChordDefs.Count; ++i)
            {
                List<byte> bmcdVelocities = BasicMidiChordDefs[i].Velocities;
                for(int j = 0; j < bmcdVelocities.Count; ++j)
                {
                    byte oldVelocity = bmcdVelocities[j];
                    byte valueToSet = VelocityValue((int)Math.Round((oldVelocity * factorForOldValue) + (newVelocity * factorForNewValue)));
                    bmcdVelocities[j] = valueToSet;
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
            #endregion conditions

            List<byte> velocities0 = BasicMidiChordDefs[0].Velocities;
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

            foreach(BasicMidiChordDef bmcd in BasicMidiChordDefs)
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
            foreach(BasicMidiChordDef bmcd in BasicMidiChordDefs)
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
        /// All the pitches in the MidiChordDef must be contained in the gamut.
        /// Transposes the pitches in NotatedMidiPitches, and all BasicMidiChordDef.Pitches by
        /// the number of steps in the gamut. Negative values transpose down.
        /// The vertical velocity sequence remains unchanged except when notes are removed.
        /// It is not an error if Midi values would exceed the range of the gamut.
        /// In this case, they are silently coerced to the bottom or top notes of the gamut respectively.
        /// Duplicate top and bottom gamut pitches are removed.
        /// </summary>
        public void TransposeStepsInGamut(Gamut gamut, int steps)
        {
            #region conditions
            Debug.Assert(gamut != null);
            foreach(BasicMidiChordDef bmcd in BasicMidiChordDefs)
            {
                foreach(int pitch in bmcd.Pitches)
                {
                    Debug.Assert(gamut.Contains(pitch));
                }
            }
            #endregion conditions

            int bottomMostPitch = gamut[0];
            int topMostPitch = gamut[gamut.Count - 1];

            foreach(BasicMidiChordDef bmcd in BasicMidiChordDefs)
            {
                List<byte> pitches = bmcd.Pitches;
                List<byte> velocities = bmcd.Velocities;
                for(int i = 0; i < pitches.Count; ++i)
                {
                    pitches[i] = DoTranspose(pitches[i], gamut, steps);
                }
                RemoveDuplicateNotes(pitches, velocities);
            }

            SetNotatedValuesFromFirstBMCD();
        }

        /// <summary>
        /// The rootPitch and all the pitches in the MidiChordDef must be contained in the gamut.
        /// The vertical velocity sequence remains unchanged except when notes are removed because they are duplicates.
        /// Calculates the number of steps to transpose, and then calls TransposeStepsInGamut.
        /// When this function returns, rootPitch is the lowest pitch in both BasicMidiChordDefs[0] and NotatedMidiPitches.
        /// </summary>
        public void TransposeToRootInGamut(Gamut gamut, int rootPitch)
        {
            #region conditions
            Debug.Assert(gamut != null);
            Debug.Assert(gamut.Contains(rootPitch));
            Debug.Assert(gamut.Contains(BasicMidiChordDefs[0].Pitches[0]));
            #endregion conditions

            int stepsToTranspose = gamut.IndexOf(rootPitch) - gamut.IndexOf(BasicMidiChordDefs[0].Pitches[0]);

            // checks that all the pitches are in the gamut.
            TransposeStepsInGamut(gamut, stepsToTranspose);
        }

        private byte DoTranspose(byte initialValue, Gamut gamut, int steps)
        {
            int index = gamut.IndexOf(initialValue);
            int newIndex = index + steps;
            newIndex = (newIndex >= 0) ? newIndex : 0;
            newIndex = (newIndex < gamut.Count) ? newIndex : gamut.Count - 1;

            return (byte)gamut[newIndex];
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
			foreach(BasicMidiChordDef bmcd in BasicMidiChordDefs)
			{
                bmcd.AdjustVelocities(factor);
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

            foreach(BasicMidiChordDef bmcd in BasicMidiChordDefs)
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
        public override string ToString() => $"MidiChordDef: MsDuration={MsDuration} BasePitch={NotatedMidiPitches[0]} MsPositionReFirstIUD={MsPositionReFirstUD}";

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
        /// If a non-null gamut argument is given, a check is made (in the Trk constructor) to ensure that it contains
        /// all the pitches (having velocity greater than 1) in this MidiChordDef.
        /// Each new MidiChordDef has one BasicMidiChordDef, which is a copy of a BasicMidiChordDef from this MidiChordDef.
        /// The new MidiChordDef's notated pitches and velocities are the same as its BasicMidiChordDef's.
        /// The durations of the returned MidiChordDefs are in proportion to the durations of the original BasicMidichordDefs.
        /// This MidiChordDef is not changed by calling this function.
        /// </summary>
        /// <param name="msDuration">The duration of the returned Trk</param>
        /// <param name="midiChannel">The channel of the returned Trk</param>
        /// <param name="gamut">The gamut must contain all the pitches in this MidiChordDef.</param>
        public Trk ToTrk(int msDuration, int midiChannel)
        {
            List<IUniqueDef> iuds = new List<IUniqueDef>();
            foreach(BasicMidiChordDef bmcd in this.BasicMidiChordDefs)
            {
                // this constructor checks that pitches are in range 0..127, and velocities are in range 1..127.
                MidiChordDef mcd = new MidiChordDef(bmcd.Pitches, bmcd.Velocities, bmcd.MsDuration, bmcd.HasChordOff);
                mcd.BasicMidiChordDefs[0].BankIndex = bmcd.BankIndex;
                mcd.BasicMidiChordDefs[0].PatchIndex = bmcd.PatchIndex;
                mcd.BasicMidiChordDefs[0].HasChordOff = bmcd.HasChordOff;
                iuds.Add(mcd);
            }

            Trk trk = new Trk(midiChannel, 0, iuds);
            trk.MsDuration = msDuration; // calls Trk.SetMsPositionsReFirstUD();
            
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
        /// 
        /// Patch indices set in the BasicMidiChordDefs override those set in the main MidiChordDef.
        /// However, if BasicMidiChordDefs[0].PatchIndex is null, and this.Patch is set, BasicMidiChordDefs[0].PatchIndex is set to Patch.
        /// The same is true for Bank settings.  
        /// The AssistantPerformer therefore only needs to look at BasicMidiChordDefs to find Bank and Patch changes.
        /// While constructing Tracks, the AssistantPerformer should monitor the current Bank and/or Patch, so that it can decide
        /// whether or not to actually construct and send bank and/or patch change messages.
        /// </summary>

        #region old WriteSVG()
        public void WriteSvg(SvgWriter w)
        {
            w.WriteStartElement("score", "midi", null);  

            Debug.Assert(BasicMidiChordDefs != null && BasicMidiChordDefs.Count > 0);
            
            if(BasicMidiChordDefs[0].BankIndex == null && Bank != null)
            {
                BasicMidiChordDefs[0].BankIndex = Bank;
            }
            if(BasicMidiChordDefs[0].PatchIndex == null && Patch != null)
            {
                BasicMidiChordDefs[0].PatchIndex = Patch;
            }
            if(HasChordOff == false)
                w.WriteAttributeString("hasChordOff", "0");
            if(PitchWheelDeviation != null && PitchWheelDeviation != M.DefaultPitchWheelDeviation)
                w.WriteAttributeString("pitchWheelDeviation", PitchWheelDeviation.ToString());
            if(MinimumBasicMidiChordMsDuration != M.DefaultMinimumBasicMidiChordMsDuration)
                w.WriteAttributeString("minBasicChordMsDuration", MinimumBasicMidiChordMsDuration.ToString());

            w.WriteStartElement("basicChords");
            foreach(BasicMidiChordDef basicMidiChord in BasicMidiChordDefs) // containing basic <midiChord> elements
                basicMidiChord.WriteSVG(w);
            w.WriteEndElement();

            if(MidiChordSliderDefs != null)
                MidiChordSliderDefs.WriteSVG(w); // writes sliders element

            w.WriteEndElement(); // score:midi
        }
        #endregion old WriteSVG()

        private static List<int> GetBasicMidiChordDurations(List<BasicMidiChordDef> ornamentChords)
        {
            List<int> returnList = new List<int>();
            foreach(BasicMidiChordDef bmc in ornamentChords)
            {
                returnList.Add(bmc.MsDuration);
            }
            return returnList;
        }

        /// <summary>
        /// This function returns the maximum number of ornament chords that can be fit into the given msDuration
        /// using the given relativeDurations and minimumOrnamentChordMsDuration.
        /// </summary>
        private static int GetNumberOfOrnamentChords(int msDuration, List<int> relativeDurations, int minimumOrnamentChordMsDuration)
        {
            bool okay = true;
            int numberOfOrnamentChords = 1;
            float factor = 1.0F;
            // try each ornament length in turn until okay is true
            for(int numChords = relativeDurations.Count; numChords > 0; --numChords)
            {
                okay = true;
                int sum = 0;
                for(int i = 0; i < numChords; ++i)
                    sum += relativeDurations[i];
                factor = ((float)msDuration / (float)sum);

                for(int i = 0; i < numChords; ++i)
                {
                    if((relativeDurations[i] * factor) < (float)minimumOrnamentChordMsDuration)
                        okay = false;
                }
                if(okay)
                {
                    numberOfOrnamentChords = numChords;
                    break;
                }
            }
            Debug.Assert(okay);
            return numberOfOrnamentChords;
        }

        /// <summary>
        /// Returns a list of (millisecond) durations whose sum is msDuration.
        /// The List contains the maximum number of durations which can be fit from relativeDurations into the msDuration
        /// such that no duration is less than minimumOrnamentChordMsDuration.
        /// </summary>
        /// <param name="msDuration"></param>
        /// <param name="relativeDurations"></param>
        /// <param name="ornamentMinMsDuration"></param>
        /// <returns></returns>
        private static List<int> GetDurations(int msDuration, List<int> relativeDurations, int ornamentMinMsDuration)
        {
            int numberOfOrnamentChords = GetNumberOfOrnamentChords(msDuration, relativeDurations, ornamentMinMsDuration);

            List<int> actualRelativeDurations = new List<int>();
            for(int i = 0; i< numberOfOrnamentChords; ++i)
            {
                actualRelativeDurations.Add(relativeDurations[i]);
            }

            List<int> intDurations = M.IntDivisionSizes(msDuration, actualRelativeDurations);

            return intDurations;
        }

        /// <summary>
        /// Returns a new list of basicMidiChordDefs having the msOuterDuration, shortening the list if necessary.
        /// </summary>
        /// <param name="basicMidiChordDefs"></param>
        /// <param name="msOuterDuration"></param>
        /// <param name="minimumMsDuration"></param>
        /// <returns></returns>
        public static List<BasicMidiChordDef> FitToDuration(List<BasicMidiChordDef> bmcd, int msOuterDuration, int minimumMsDuration)
        {
            List<int> relativeDurations = GetBasicMidiChordDurations(bmcd);
            List<int> msDurations = GetDurations(msOuterDuration, relativeDurations, minimumMsDuration);

            // msDurations.Count can be less than bmcd.Count

            List<BasicMidiChordDef> rList = new List<BasicMidiChordDef>();
            BasicMidiChordDef b;
            for(int i = 0; i < msDurations.Count; ++i)
            {
                b = bmcd[i];
                // constructor checks that pitches are in range 0..127, and velocities are in range 1..127.
                rList.Add(new BasicMidiChordDef(msDurations[i], b.BankIndex, b.PatchIndex, b.HasChordOff, b.Pitches, b.Velocities));
            }

            return rList;
        }
        #region properties

        public List<int> BasicChordDurations
        {
            get
            {
                List<int> rList = new List<int>();
                foreach(BasicMidiChordDef bmcd in BasicMidiChordDefs)
                {
                    rList.Add(bmcd.MsDuration);
                }
                return rList;
            }
        }

        /****************************************************************************/

        public int MsPositionReFirstUD { get { return _msPositionReFirstIUD; } set { _msPositionReFirstIUD = value; } }
        private int _msPositionReFirstIUD = 0;

        public override int MsDuration
        {
            get
            {
                return _msDuration;
            }
            set
            {
                Debug.Assert(BasicMidiChordDefs != null && BasicMidiChordDefs.Count > 0);
                _msDuration = value;
                int sumDurations = 0;
                foreach(int bcd in BasicChordDurations)
                    sumDurations += bcd;
                if(_msDuration != sumDurations)
                {
                    BasicMidiChordDefs = FitToDuration(BasicMidiChordDefs, _msDuration, _minimumBasicMidiChordMsDuration);
                }
            }
        }

        public byte? Bank { get { return _bank; } set { _bank = value; } }
        private byte? _bank = null;
        public byte? Patch { get { return _patch; } set { _patch = value; } }
        private byte? _patch = null;
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
        public byte? _pitchWheelDeviation = null;
        public bool HasChordOff { get { return _hasChordOff; } set { _hasChordOff = value; } }
        private bool _hasChordOff = true;
        public bool BeamContinues { get { return _beamContinues; } set { _beamContinues = value; } }
        private bool _beamContinues = true;
        public string Lyric { get { return _lyric; } set { _lyric = value; } }
        private string _lyric = null; 
        public int MinimumBasicMidiChordMsDuration { get { return _minimumBasicMidiChordMsDuration; } set { _minimumBasicMidiChordMsDuration = value; } }
        private int _minimumBasicMidiChordMsDuration = 1;
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

        public int OrnamentNumberSymbol { get { return _ornamentNumberSymbol; } set { _ornamentNumberSymbol = value; } }
        private int _ornamentNumberSymbol = 0;

        public MidiChordSliderDefs MidiChordSliderDefs = null;
        public List<BasicMidiChordDef> BasicMidiChordDefs = new List<BasicMidiChordDef>();

        public int? MsDurationToNextBarline { get { return _msDurationToNextBarline; } set { _msDurationToNextBarline = value; } }

        private int? _msDurationToNextBarline = null;

        #endregion properties
    }
}
