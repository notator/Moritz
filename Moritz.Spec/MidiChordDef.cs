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
        /// </summary>
        public MidiChordDef(List<byte> pitches, List<byte> velocities, int msDuration, bool hasChordOff)
            : base(msDuration)
        {
            #region conditions
            Debug.Assert(pitches.Count == velocities.Count);
            foreach(byte pitch in pitches)
                Debug.Assert(pitch == M.MidiValue((int)pitch), "Pitch out of range.");
            foreach(byte velocity in velocities)
                Debug.Assert(velocity == M.MidiValue((int)velocity), "Velocity out of range.");
            #endregion conditions

            _msPositionReFirstIUD = 0; // default value
            _hasChordOff = hasChordOff;
            _minimumBasicMidiChordMsDuration = 1; // not used (this is not an ornament)

            _notatedMidiPitches = pitches;
            _notatedMidiVelocities = velocities;

            _ornamentNumberSymbol = 0;

            MidiChordSliderDefs = null;

            byte? bank = null;                                                                           
            byte? patch = null;

            BasicMidiChordDefs.Add(new BasicMidiChordDef(msDuration, bank, patch, hasChordOff, pitches, velocities));

            CheckTotalDuration();
        }

        /// <summary>
        /// A MidiChordDef having msDuration, and containing an ornament having BasicMidiChordDefs with nPitchesPerChord.
        /// The notated pitch and the pitch of BasicMidiChordDefs[0] are set to rootNotatedPitch.
        /// The notated velocity of all pitches is set to 127.
        /// The root pitches of the BasicMidiChordDefs begin with rootNotatedPitch, and follow the ornamentEnvelope, using
        /// the ornamentEnvelope's values as indices in the gamut. Their durations are as equal as possible, to give the
        /// overall msDuration.
        /// If ornamentEnvelope is null, a single, one-note BasicMidiChordDef will be created.
        /// The number of pitches in a chord may be less than nPitchesPerChord (see gamut.GetChord(...) ).
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
                Debug.Assert(pitch == M.MidiValue((int)pitch), "Pitch out of range.");

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

            Gamut oppositeGamut = gamut.Opposite();
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

            InvertPitches(NotatedMidiPitches, nPitchesToShiftArg);
            RemoveDuplicateNotes(NotatedMidiPitches, NotatedMidiVelocities);
            foreach(BasicMidiChordDef bmcd in BasicMidiChordDefs)
            {
                InvertPitches(bmcd.Pitches, nPitchesToShiftArg);
                RemoveDuplicateNotes(bmcd.Pitches, bmcd.Velocities);
            } 
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
        /// The first argument contains a list of 12 velocity values (range [0..127] in order of absolute pitch.
        /// The second (optional) argument determines the proportion of the final velocity determined by this function.
        /// The other component is the existing velocity. If percent is 100.0, the existing velocity is replaced completely.
        /// For example: If the MidiChordDef contains one or more C#s, they will be given velocity velocityPerAbsolutePitch[1].
        /// Middle-C is midi pitch 60 (60 % 12 == absolute pitch 0), middle-C# is midi pitch 61 (61 % 12 == absolute pitch 1), etc.
        /// This function applies equally to all the BasicMidiChordDefs in this MidiChordDef. 
        /// </summary>
        /// <param name="velocityPerAbsolutePitch">A list of 12 velocity values (range [0..127] in order of absolute pitch</param>
        /// <param name="percent">In range 0..100. The proportion of the final velocity value that comes from this function.</param>
        public void SetVelocityPerAbsolutePitch(List<byte> velocityPerAbsolutePitch, double percent = 100.0)
        {
            #region conditions
            Debug.Assert(velocityPerAbsolutePitch.Count == 12);
            for(int i = 0; i < 12; ++i)
            {
                int v = velocityPerAbsolutePitch[i];
                Debug.Assert(v >= 0 && v <= 127);
            }
            Debug.Assert(percent >= 0 && percent <= 100);
            Debug.Assert(this.NotatedMidiPitches.Count == NotatedMidiVelocities.Count);
            #endregion conditions
            double factorForNewValue = percent / 100;
            double factorForOldValue = 1 - factorForNewValue;
            for(int pitchIndex = 0; pitchIndex < NotatedMidiPitches.Count; ++pitchIndex)
            {
                byte oldVelocity = NotatedMidiVelocities[pitchIndex];

                int absPitch = NotatedMidiPitches[pitchIndex] % 12;
                byte newVelocity = velocityPerAbsolutePitch[absPitch];
                int valueToSet = (int)Math.Round((oldVelocity * factorForOldValue) + (newVelocity * factorForNewValue));
                Debug.Assert(valueToSet >= 0 && valueToSet <= 127);
                NotatedMidiVelocities[pitchIndex] = M.MidiValue(valueToSet); 
            }

            foreach(BasicMidiChordDef bmcd in BasicMidiChordDefs)
            {
                bmcd.SetVelocityPerAbsolutePitch(velocityPerAbsolutePitch);
            }
        }
        #endregion SetVelocityPerAbsolutePitch

        /// <summary>
        /// Sets all velocities in the MidiChordDef to a value related to its msDuration.
        /// If percent has its default value 100, the new velocity will be in the same proportion between velocityForMinMsDuration
        /// and velocityForMaxMsDuration as MsDuration is between msDurationRangeMin and msDurationRangeMax.
        /// N.B 1) Neither velocityForMinMsDuration nor velocityForMaxMsDuration can be zero! -- that would be a NoteOff.
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
            // neither velocityForMinMsDuration nor velocityForMaxMsDuration can be zero!
            // velocityForMinMsDuration can be less than, equal to, or greater than velocityForMaxMsDuration
            Debug.Assert(velocityForMinMsDuration >= 1 && velocityForMinMsDuration <= 127);
            Debug.Assert(velocityForMaxMsDuration >= 1 && velocityForMaxMsDuration <= 127);
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
                newVelocity = M.MidiValue(velocityForMinMsDuration + increment);
            }
            for(int i = 0; i < _notatedMidiVelocities.Count; ++i)
            {
                byte oldVelocity = _notatedMidiVelocities[i];
                int valueToSet = (int)Math.Round((oldVelocity * factorForOldValue) + (newVelocity * factorForNewValue));
                Debug.Assert(valueToSet >= 0 && valueToSet <= 127);
                _notatedMidiVelocities[i] = M.MidiValue(valueToSet);

            }
            for(int i = 0; i < BasicMidiChordDefs.Count; ++i)
            {
                List<byte> bmcdVelocities = BasicMidiChordDefs[i].Velocities;
                for(int j = 0; j < bmcdVelocities.Count; ++j)
                {
                    byte oldVelocity = bmcdVelocities[j];
                    int valueToSet = (int)Math.Round((oldVelocity * factorForOldValue) + (newVelocity * factorForNewValue));
                    Debug.Assert(valueToSet >= 0 && valueToSet <= 127);
                    bmcdVelocities[j] = M.MidiValue(valueToSet);
                }
            }
        }

        #region SetVerticalVelocityGradient
        /// <summary>
        /// The arguments are both in range [1..127].
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
            Debug.Assert(rootVelocity > 0 && rootVelocity <= 127);
            Debug.Assert(topVelocity > 0 && topVelocity <= 127);
            #endregion conditions

            if(NotatedMidiVelocities.Count > 1)
            {
                double increment = (((double)(topVelocity - rootVelocity)) / (NotatedMidiVelocities.Count - 1));
                double newVelocity = rootVelocity;
                for(int velocityIndex = 0; velocityIndex < NotatedMidiVelocities.Count; ++velocityIndex)
                {
                    NotatedMidiVelocities[velocityIndex] = M.MidiValue((int)Math.Round(newVelocity));
                    newVelocity += increment;
                }
            }

            double rootVelocityFactor = ((double)rootVelocity) / NotatedMidiVelocities[0];
            double verticalVelocityFactor = ((double)topVelocity) / rootVelocity;

            foreach(BasicMidiChordDef bmcd in BasicMidiChordDefs)
            {
                byte bmcdRootVelocity = M.MidiValue((int)(Math.Round(bmcd.Velocities[0] * rootVelocityFactor)));
                byte bmcdTopVelocity = M.MidiValue((int)(Math.Round(bmcdRootVelocity * verticalVelocityFactor)));
                bmcd.SetVerticalVelocityGradient(bmcdRootVelocity, bmcdTopVelocity);
            }
        }

        #endregion SetVerticalVelocityGradient

        #endregion Functions that use Envelopes

        #region GetNoteCombination() 
        public enum MidiChordPitchOperator
        {
            // GetNoteCombination() returns all the pitches in both operands
            allArg1AndArg2,
            // GetNoteCombination() returns all the pitches that occur in the first operand except those that occur in the second operand.
            inArg1ButNotArg2,
            // GetNoteCombination() returns only the pitches that occur in both operands 
            inBothArg1AndArg2,
            // GetNoteCombination() returns all the pitches that occur either in the first or second operand, but not both.
            inEitherArg1OrArg2ButNotBoth
        }

        /// <summary>
        /// Returns a Tuple containing two new lists: the pitches and velocities that can be used to construct a new MidiChordDef.
        /// This function only uses the Notated pitches and velocities of its arguments. The BasicMidiChordDefs are ignored.
        /// The returned lists should be checked to see that they are not empty. If they *are* empty, maybe create a rest.
        /// </summary>
        /// <param name="mcd1"></param>
        /// <param name="mcd2"></param>
        /// <param name="midiChordPitchOperator"></param>
        /// <returns></returns>
        public static Tuple<List<byte>, List<byte>> GetNoteCombination(MidiChordDef mcd1, MidiChordDef mcd2, MidiChordPitchOperator midiChordPitchOperator)
        {
            Tuple<List<byte>, List<byte>> rval = null;

            List<byte> arg1Pitches = mcd1.NotatedMidiPitches;
            List<byte> arg1Velocities = mcd1.NotatedMidiVelocities;
            List<byte> arg2Pitches = mcd2.NotatedMidiPitches;
            List<byte> arg2Velocities = mcd2.NotatedMidiVelocities;

            switch(midiChordPitchOperator)
            {
                case MidiChordPitchOperator.allArg1AndArg2:
                    rval = AddNotes(arg1Pitches, arg1Velocities, arg2Pitches, arg2Velocities);
                    break;
                case MidiChordPitchOperator.inArg1ButNotArg2:
                    rval = SubtractNotes(arg1Pitches, arg1Velocities, arg2Pitches);
                    break;
                case MidiChordPitchOperator.inBothArg1AndArg2:
                    rval = OrNotesInclusive(arg1Pitches, arg1Velocities, arg2Pitches);
                    break;
                case MidiChordPitchOperator.inEitherArg1OrArg2ButNotBoth:
                    rval = OrNotesExclusive(arg1Pitches, arg1Velocities, arg2Pitches, arg2Velocities);
                    break;
            }

            return rval;
        }

        #region AddNotes
        /// <summary>
        /// Returns a Tuple containing two new lists: the pitches and velocities that can be used to construct a new MidiChordDef.
        /// This function first clones arg1Pitches and arg1Velocities, then adds midiPitches and midiVelocities from
        /// arg2Pitches and arg2Velocities to the clones. The clones are returned in the returned Tuple.
        /// If a pitch already exists, the larger of the two velocities is used, otherwise the new pitch is
        /// inserted in the pitches list at the appropriate position (so that pitches continue to be in
        /// ascending order), and the new velocity is inserted at the corresponding position in the velocities list.
        /// </summary>
        private static Tuple<List<byte>, List<byte>> AddNotes(List<byte> arg1Pitches, List<byte> arg1Velocities, List<byte> arg2Pitches, List<byte> arg2Velocities)
        {
            List<byte> pitches = new List<byte>(arg1Pitches);
            List<byte> velocities = new List<byte>(arg1Velocities);

            for(int i = 0; i < arg2Pitches.Count; ++i)
            {
                byte pitchToAdd = arg2Pitches[i];
                byte velocitytoAdd = arg2Velocities[i];
                bool found = false;
                int index = pitches.Count; // default is append at top of list
                for(int j = 0; j < pitches.Count; ++j)
                {
                    if(pitches[j] == pitchToAdd)
                    {
                        index = j;
                        found = true;
                        break;
                    }
                    if(pitches[j] > pitchToAdd)
                    {
                        index = j;
                        break;
                    }
                }
                if(found)
                {
                    velocities[index] = (velocities[index] > velocitytoAdd) ? velocities[index] : velocitytoAdd;
                }
                else
                {
                    pitches.Insert(index, pitchToAdd);
                    velocities.Insert(index, velocitytoAdd);
                }
            }

            return new Tuple<List<byte>, List<byte>>(pitches, velocities);
        }
        #endregion AddNotes(MidiChordDef mcd2)
        #region SubtractNotes
        /// <summary>
        /// Returns a Tuple containing two new lists: the pitches and velocities that can be used to construct a new MidiChordDef.
        /// Tuple.Item1 contains the pitches from arg1Pitches that are not in arg2Pitches. Tuple.Item2 contains the original
        /// velocities of the pitches in Tuple.Item1.
        /// Note that Tuple.Item1 should be checked to see if it is empty before attempting to use the lists to create a new
        /// MidiChordDef (maybe create a RestDef instead).
        /// </summary>
        private static Tuple<List<byte>, List<byte>> SubtractNotes(List<byte> arg1Pitches, List<byte> arg1Velocities, List<byte> arg2Pitches)
        {
            List<byte> pitches = new List<byte>();
            List<byte> velocities = new List<byte>();

            for(int i = 0; i < arg1Pitches.Count; ++i)
            {
                if(!arg2Pitches.Contains(arg1Pitches[i]))
                {
                    pitches.Add(arg1Pitches[i]);
                    velocities.Add(arg1Velocities[i]);
                }
            }

            return new Tuple<List<byte>, List<byte>>(pitches, velocities);
        }
        #endregion SubtractNotes
        #region OrNotesInclusive
        /// <summary>
        /// Returns a Tuple containing two new lists: the pitches and velocities that can be used to construct a new MidiChordDef.
        /// Tuple.Item1 contains the pitches from arg1Pitches that are also in arg2Pitches.
        /// Tuple.Item2 contains the original velocities of the pitches in Tuple.Item1.
        /// Note that Tuple.Item1 should be checked to see if it is empty before attempting to use the lists to create a new
        /// MidiChordDef (maybe create a RestDef instead).
        /// </summary>
        private static Tuple<List<byte>, List<byte>> OrNotesInclusive(List<byte> arg1Pitches, List<byte> arg1Velocities, List<byte> arg2Pitches)
        {
            List<byte> pitches = new List<byte>();
            List<byte> velocities = new List<byte>();

            for(int i = 0; i < arg1Pitches.Count; ++i)
            {
                if(arg2Pitches.Contains(arg1Pitches[i]))
                {
                    pitches.Add(arg1Pitches[i]);
                    velocities.Add(arg1Velocities[i]);
                }
            }

            return new Tuple<List<byte>, List<byte>>(pitches, velocities);
        }

        #endregion OrNotesInclusive
        #region OrNotesExclusive
        /// <summary>
        /// Returns a Tuple containing two new lists: the pitches and velocities that can be used to construct a new MidiChordDef.
        /// Tuple.Item1 contains the pitches that are either in arg1Pitches or in arg2Pitches, but not both. Tuple.Item2 contains
        /// the original velocities of the pitches in Tuple.Item1.
        /// Note that Tuple.Item1 should be checked to see if it is empty before attempting to use the lists to create a new
        /// MidiChordDef (maybe create a RestDef instead).
        /// </summary>
        private static Tuple<List<byte>, List<byte>> OrNotesExclusive(List<byte> arg1Pitches, List<byte> arg1Velocities, List<byte> arg2Pitches, List<byte> arg2Velocities )
        {
            Tuple<List<byte>, List<byte>> rval = AddNotes(arg1Pitches, arg1Velocities, arg2Pitches, arg2Velocities);

            List<byte> pitches = rval.Item1;
            List<byte> velocities = rval.Item2;

            for(int i = pitches.Count - 1; i >= 0; --i)
            {
                if(arg1Pitches.Contains(pitches[i]) && arg2Pitches.Contains(pitches[i]))
                {
                    pitches.RemoveAt(i);
                    velocities.RemoveAt(i);
                }
            }

            return rval;
        }

        #endregion OrNotesExclusive
        #endregion GetNoteCombination() 

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
            for(int i = 0; i < _notatedMidiPitches.Count; ++i)
            {
                _notatedMidiPitches[i] = (byte)M.MidiValue(_notatedMidiPitches[i] + interval);
            }
            RemoveDuplicateNotes(_notatedMidiPitches, _notatedMidiVelocities);

            foreach(BasicMidiChordDef bmcd in BasicMidiChordDefs)
            {
                List<byte> pitches = bmcd.Pitches;
                List<byte> velocities = bmcd.Velocities;
                for(int i = 0; i < pitches.Count; ++i)
                {
                    pitches[i] = (byte)M.MidiValue(pitches[i] + interval);
                }
                RemoveDuplicateNotes(pitches, velocities);
            }
        }

        /// <summary>
        /// All the pitches in the MidiChordDef must be contained in the gamut.
        /// Transposes the pitches in NotatedMidiPitches, and all BasicMidiChordDef.Pitches by
        /// the number of steps in the gamut. Negative values transpose down.
        /// It is not an error if Midi values would exceed the range of the gamut.
        /// In this case, they are silently coerced to the bottom or top notes of the gamut respectively.
        /// Duplicate top and bottom gamut pitches are removed.
        /// </summary>
        public void TransposeInGamut(Gamut gamut, int steps)
        {
            #region conditions
            Debug.Assert(gamut != null);
            foreach(int pitch in NotatedMidiPitches)
            {
                Debug.Assert(gamut.Contains(pitch));
            }
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

            for(int i = 0; i < NotatedMidiPitches.Count; ++i)
            {
                NotatedMidiPitches[i] = DoTranspose(NotatedMidiPitches[i], gamut, steps);
            }
            RemoveDuplicateNotes(_notatedMidiPitches, _notatedMidiVelocities);
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
        /// If a velocity would be less than 1, it is silently coerced to 1.
        /// If a velocity would be greater than 127, it is silently coerced to 127.
        /// </summary>
        /// <param name="factor"></param>
        public void AdjustVelocities(double factor)
		{
            for(int i=0; i< _notatedMidiVelocities.Count; ++i)
            {
                byte newVelocity = (byte)Math.Ceiling((_notatedMidiVelocities[i] * factor));
                newVelocity = (newVelocity < 1) ? (byte)1 : newVelocity;
                newVelocity = (newVelocity > 127) ? (byte)127 : newVelocity;
                _notatedMidiVelocities[i] = newVelocity;
            }
			foreach(BasicMidiChordDef bmcd in BasicMidiChordDefs)
			{
				for(int i = 0; i < bmcd.Velocities.Count; ++i)
				{
					byte velocity = (byte)Math.Ceiling((bmcd.Velocities[i] * factor));
                    velocity = (velocity < 1) ? (byte)1 : velocity;
                    velocity = (velocity > 127) ? (byte)127 : velocity;
                    bmcd.Velocities[i] = velocity;
				}
			}
		}

        /// <summary>
        /// Sets the number of values in NotatedMidiPitches, NotatedMidiVelocities, and all BasicMidiChordDef.Pitches and
        /// BasicMidiChordDef.Velocities to newDensity, by removing the upper pitches (and their velocities) as necessary.
        /// Requires newDensity to be less than or equal to the current vertical density of all the chords to be changed.
        /// </summary>
        public void SetVerticalDensity(int newDensity)
		{
            #region require
            Debug.Assert(newDensity <= _notatedMidiPitches.Count); // if its equal, do nothing
            foreach(BasicMidiChordDef bmcd in BasicMidiChordDefs)
			{
                Debug.Assert(newDensity <= bmcd.Pitches.Count);
			}
            #endregion require

            int nElementsToRemove = _notatedMidiPitches.Count - newDensity;
            if(nElementsToRemove > 0)
            {
                _notatedMidiPitches.RemoveRange(newDensity, nElementsToRemove);
                _notatedMidiVelocities.RemoveRange(newDensity, nElementsToRemove);
			}
            foreach(BasicMidiChordDef bmcd in BasicMidiChordDefs)
            {
                nElementsToRemove = bmcd.Pitches.Count - newDensity;
                if(nElementsToRemove > 0)
                {
                    bmcd.Pitches.RemoveRange(newDensity, nElementsToRemove);
                    bmcd.Velocities.RemoveRange(newDensity, nElementsToRemove);
                }
            }
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

        public void AdjustExpression(double factor)
        {
            List<byte> exprs = this.MidiChordSliderDefs.ExpressionMsbs;
            for(int i = 0; i < exprs.Count; ++i)
            {
                exprs[i] = M.MidiValue((int)(exprs[i] * factor));
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
                    pans.Add(M.MidiValue((int)(value[i])));
                }
            }
        }

        public void AdjustModulationWheel(double factor)
        {
            List<byte> modWheels = this.MidiChordSliderDefs.ModulationWheelMsbs;
            for(int i = 0; i < modWheels.Count; ++i)
            {
                modWheels[i] = M.MidiValue((int)(modWheels[i] * factor));
            }
        }

        public void AdjustPitchWheel(double factor)
        {
            List<byte> pitchWheels = this.MidiChordSliderDefs.PitchWheelMsbs;
            for(int i = 0; i < pitchWheels.Count; ++i)
            {
                pitchWheels[i] = M.MidiValue((int)(pitchWheels[i] * factor));
            }
        }

        /// <summary>
        /// Note that, unlike Rests, MidiChordDefs do not have a msDuration attribute.
        /// Their msDuration is deduced from the contained BasicMidiChords.
        /// Patch indices set in the BasicMidiChordDefs override those set in the main MidiChordDef.
        /// However, if BasicMidiChordDefs[0].PatchIndex is null, and this.Patch is set, BasicMidiChordDefs[0].PatchIndex is set to Patch.
        /// The same is true for Bank settings.  
        /// The AssistantPerformer therefore only needs to look at BasicMidiChordDefs to find Bank and Patch changes.
        /// While constructing Tracks, the AssistantPerformer should monitor the current Bank and/or Patch, so that it can decide
        /// whether or not to actually construct and send bank and/or patch change messages.
        /// </summary>
        public void WriteSvg(SvgWriter w)
        {
            w.WriteStartElement("score", "midiChord", null);  

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

            w.WriteEndElement(); // score:midiChord
        }

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
                    _pitchWheelDeviation = M.MidiValue(val);
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
                // If the Count is changed, the _notatedMidiVelocities must subsequently be set to
                // otherwise a Debug.Assert will fail when _notatedMidiVelocities are retrieved.
                foreach(byte pitch in value)
                {
                    Debug.Assert(pitch >= 0 && pitch <= 127);
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
                    Debug.Assert(velocity >= 0 && velocity <= 127);
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
