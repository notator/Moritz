using Krystals5ObjectLibrary;

using Moritz.Globals;
using Moritz.Xml;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Moritz.Spec
{
    ///<summary>
    /// A MidiChordDef can be saved and retrieved from a channel in an SVG file.
    /// It defines the midi information for a single chord event.
    /// It does not contain graphic information, but can be used to _construct_ graphic information.
    /// Each MidiChordDef is part of a sequence of IUniqueDefs stored in a Trk.
    /// There may be a list of parallel Trks containing alternative definitions of the same events.
    /// If this is the case, IUniqueDefs in the first Trk will be used to construct the graphics.
    /// The IUniqueDefs stored in Trks are MidiChordDef, RestDef, CautionaryChordDef, ClefDef.
    /// A MidiChordDef consists of
    ///     1. The pitch and velocity information for NoteOn/NoteOff messages,
    ///     2. The logical duration before the following DurationDef in a Trk,
    ///     3. Whether or not NoteOffs are sent after its msDuration has elapsed,
    ///     4. An optional set of (single) midi control messages that will be sent before the NoteOns.
    ///     5. An optional control envelope that will be sent over the duration of the midiChord.
    ///</summary>
    public class MidiChordDef : DurationDef
    {
        #region constructors
        /// <summary>
        /// A MidiChordDef whose optional set of MidiControlMessages is null.
        /// </summary>
        /// <param name="pitches">in range 0..127</param>
        /// <param name="velocities">In range 1..127</param>
        /// <param name="msDuration">greater than zero</param>
        /// <param name="hasChordOff"></param>
        /// <param name="midiChordControlDef">optional controls</param>
        public MidiChordDef(List<int> pitches, List<int> velocities, int msDuration, bool hasChordOff, MidiChordControlDef midiChordControlDef = null)
            : base(msDuration)
        {
            #region conditions
            Debug.Assert(pitches.Count == velocities.Count);
            foreach(int pitch in pitches)
            {
                AssertIsMidiValue(pitch);
            }
            foreach(int velocity in velocities)
            {
                AssertIsVelocityValue(velocity);
            }
            #endregion conditions

            _pitches = pitches;
            Velocities = velocities;

			MsPositionReFirstUD = 0; // default value
            HasChordOff = hasChordOff;
            OrnamentText = null; // could be useful later, but is only a non-functional annotation (2025)

            MidiChordControlDef = midiChordControlDef;

            AssertConsistency();
        }

        #endregion constructors

        #region Clone
        /// <summary>
        /// A deep clone!
        /// </summary>
        /// <returns></returns>
        public override object Clone()
        {
            MidiChordDef rval = new MidiChordDef(this._pitches, this.Velocities, this.MsDuration, this.HasChordOff)
            {
                MsPositionReFirstUD = this.MsPositionReFirstUD,
                MsDurationToNextBarline = this.MsDurationToNextBarline,
                BeamContinues = this.BeamContinues,
                Lyric = (this.Lyric == null) ? null : String.Copy(this.Lyric),
                OrnamentText = (this.OrnamentText == null) ? null : String.Copy(this.OrnamentText), // the displayed ornament Text (without the tilde)

                MidiChordControlDef = (this.MidiChordControlDef == null) ? null : (MidiChordControlDef)MidiChordControlDef.Clone(),
            };

            if(this.EnvelopeTypeDef != null)
            {
                List<int> envValues = new List<int>(this.EnvelopeTypeDef.Item2);
                EnvelopeTypeDef = new Tuple<int, List<int>>(this.EnvelopeTypeDef.Item1, envValues);
            }

            foreach(var midiDef in this.MidiDefs)
            {
                MidiDefs.Add((MidiChordDef)midiDef.Clone());  
            }

            AssertConsistency();

            return rval;
        }

        public void WriteSVG(SvgWriter w, int channel)
        { 
            AssertConsistency();

            List<Tuple<MidiMsg, int>> envelopeMessages = null;
            #region set envelopeMessages
            int noteOnsMsDuration = 0;
            if(EnvelopeTypeDef != null)
            {
                envelopeMessages = GetEnvelopeMessages(channel, EnvelopeTypeDef, MsDuration);

                MidiChordControlDef = MidiChordControlDef ?? (MidiChordControlDef = new MidiChordControlDef());
                var envMsg0 = envelopeMessages[0];                
                MidiChordControlDef.SetMsg(EnvelopeTypeDef.Item1, (int)envMsg0.Item1.Data2);
                noteOnsMsDuration = envMsg0.Item2;               

                envelopeMessages.RemoveAt(0);
            }
            #endregion
            
            w.WriteStartElement("midiChord");
            
            if(MidiChordControlDef != null)
            {
                MidiChordControlDef.WriteSVG(w, channel); // writes a "controls" element containing midi msgs
            }

            noteOnsMsDuration = (noteOnsMsDuration > 0) ? noteOnsMsDuration : MsDuration;
            this.WriteNoteOns(w, channel, noteOnsMsDuration);  // writes a "noteOns" element containing midi msgs

            if(envelopeMessages != null && envelopeMessages.Count > 0)
            {
                w.WriteStartElement("envelope"); // "envelope"
                foreach(var envMsg in envelopeMessages)
                {
                    envMsg.Item1.WriteSVG(w, envMsg.Item2);
                }
                w.WriteEndElement(); // "envelope"
            }

            if(HasChordOff)
            {
                this.WriteNoteOffs(w, channel);   // writes a "noteOffs" element containing midi msgs
            }

            w.WriteEndElement(); // midiChord
        }

        /// <summary>
        /// Returns a list of (MidiMsg, msDuration) Tuples whose total duration is exactly msDuration
        /// The default MidiMsg duration is 50ms.
        /// </summary>
        /// <param name="envelopeTypeDef">Item1 is the control type, Item2 is an envelope definition</param>
        /// <returns>A list of Tuples: Each Tuple.Item1 is a MidiMsg, Item2 is its msDuration</returns>
        private List<Tuple<MidiMsg, int>> GetEnvelopeMessages(int channel, Tuple<int, List<int>> envelopeTypeDef, int msDuration)
        {
            #region get msPosValues
            int defaultMsgMsDuration = 50;

            var envDef = envelopeTypeDef.Item2;
            int count = (int)Math.Round((double)msDuration / defaultMsgMsDuration);

            count = count < 3 ? 3 : count;

            List<int> defaultMsDurations = M.IntDivisionSizes(msDuration, count);
            List<int> defaultMsPositions = new List<int>();
            int msPos = 0;
            foreach(var duration in defaultMsDurations)
            {
                defaultMsPositions.Add(msPos);
                msPos += duration;
            }

            Envelope env = new Envelope(envDef, 127, 127, count);
            Dictionary<int, int> msPosValues = env.GetValuePerMsPosition(defaultMsPositions, out List<int> msPositions);
            List<int> msDurations = new List<int>(); 
            for(int i = 1; i < msPositions.Count; i++)
            {
                var msDur = msPositions[i] - msPositions[i-1];
                msDurations.Add(msDur);
            }
            msDurations.Add(msDuration - msPositions[msPositions.Count - 1]);
            #endregion

            List<Tuple<MidiMsg, int>> msgDurs = new List<Tuple<MidiMsg, int>>();

            int status;
            if(envelopeTypeDef.Item1 == (int)M.CMD.PITCH_WHEEL_224)
            {
                status = envelopeTypeDef.Item1 + channel;
                for(int i = 0; i < msPosValues.Count; ++i)
                {
                    var data = msPosValues[msPositions[i]];
                    MidiMsg msg = new MidiMsg(status, data, data);
                    int msDur = msDurations[i];
                    Tuple<MidiMsg, int> msgDur = new Tuple<MidiMsg, int>(msg, msDur);
                    msgDurs.Add(msgDur);
                }
            }
            else
            {
                status = (int)M.CMD.CONTROL_CHANGE_176 + channel;                
                for(int i = 0; i < msPosValues.Count; ++i)
                {
                    MidiMsg msg = new MidiMsg(status, envelopeTypeDef.Item1, msPosValues[msPositions[i]]);
                    int msDur = msDurations[i];
                    Tuple<MidiMsg, int> msgDur = new Tuple<MidiMsg, int>(msg, msDur);
                    msgDurs.Add(msgDur);
                }
            }

            return msgDurs;
        }

        private void WriteNoteOffs(SvgWriter w, int channel)
        {
            Debug.Assert(Velocities != null && _pitches.Count == Velocities.Count);
            w.WriteStartElement("noteOffs");
            int status = (int)M.CMD.NOTE_OFF_120 + channel; // NoteOff 
            for(int i = 0; i < _pitches.Count; ++i)
            {
                // The ResidentSynth ignores noteOff velocity, so it is not written to SVG.
                MidiMsg msg = new MidiMsg(status, _pitches[i]);
                msg.WriteSVG(w);
            }
            w.WriteEndElement(); // end of noteOns
        }

        private void WriteNoteOns(SvgWriter w, int channel, int msDuration)
        {
            Debug.Assert(Velocities != null && _pitches.Count == Velocities.Count);
            w.WriteStartElement("noteOns");
            w.WriteAttributeString("msDuration", msDuration.ToString());

            int status = (int)M.CMD.NOTE_ON_144 + channel; // NoteOn
            for(int i = 0; i < _pitches.Count; ++i)
            {
                MidiMsg msg = new MidiMsg(status, _pitches[i], Velocities[i]);
                msg.WriteSVG(w);
            }
            w.WriteEndElement(); // end of noteOns
        }

        public override string ToString() => $"MidiChordDef: MsDuration={MsDuration.ToString()} MsPositionReFirstUD={MsPositionReFirstUD} Pitches[0]={Pitches[0]}";

        /// <summary>
        /// Checks that pitches, velocities and EnvelopeTypeDef values are in the allowed ranges.
        /// Used by all constructors and Clone().
        /// Should always be called after changing the MidiChordDef's content.
        /// </summary>
        private void AssertConsistency()
		{
			Debug.Assert(Pitches != null, "Pitches is null");
			Debug.Assert(Velocities != null, "Velocities is null");

			foreach(int pitch in Pitches)
			{
				AssertIsMidiValue(pitch);
			}
            foreach(int velocity in Velocities)
            {
                AssertIsVelocityValue(velocity);
			}
            if(EnvelopeTypeDef != null)
            {
                foreach(var value in EnvelopeTypeDef.Item2)
                {
                    AssertIsMidiValue(value);
                }
            }
		}

        /// <summary>
        /// Used by Clone(). Returns null if listToClone is null, otherwise returns a clone of the listToClone.
        /// </summary>
        private List<int> NewListByteOrNull(List<int> listToClone)
        {
            List<int> newListByte = null;
            if(listToClone != null)
                newListByte = new List<int>(listToClone);
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
            OppositePitches(mode, oppositeMode, oppositeMCD._pitches);

            return oppositeMCD;
        }

        private void OppositePitches(Mode mode, Mode oppositeMode, List<int> pitches)
        {
            for(int i = 0; i < pitches.Count; ++i)
            {
                int pitchIndex = mode.IndexInGamut(pitches[i]);
                int oppositeModeGamutCount = oppositeMode.Gamut.Count;
                // N.B. it is not necessarily true that mode.Count == oppositeMode.Count.
                pitchIndex = (pitchIndex < oppositeModeGamutCount) ? pitchIndex : oppositeModeGamutCount - 1;
                pitches[i] = oppositeMode.Gamut[pitchIndex];
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

            InvertPitches(_pitches, nPitchesToShiftArg);
            RemoveDuplicateNotes(_pitches, Velocities);
        }

        private void InvertPitches(List<int> pitches, int nPitchesToShiftArg)
        {
            int nPitchesToShift = nPitchesToShiftArg % pitches.Count;
            for(int i = 0; i < nPitchesToShift; ++i)
            {
                int newPitch = (pitches[i] + 12);
                newPitch = (newPitch < 127) ? newPitch : 127;
                pitches[i] = newPitch;
            }
            pitches.Sort();
        }
        #endregion Invert (shift lowest notes up by one octave)
        private void SetEnvelopeTypeDef(int controlType, List<int> values)
        {
            throw new NotImplementedException();
        }

        #region SetVelocityPerAbsolutePitch
        /// <summary>
        /// The velocityPerAbsolutePitch argument contains a list of 12 velocity values (range [1..127] in order of absolute pitch.
        /// For example: If the MidiChordDef contains one or more C#s, they will be given velocity velocityPerAbsolutePitch[1].
        /// Middle-C is midi pitch 60 (60 % 12 == absolute pitch 0), middle-C# is midi pitch 61 (61 % 12 == absolute pitch 1), etc.
        /// This function applies equally to all the BasicMidiChordDefs in this MidiChordDef. 
        /// </summary>
        /// <param name="velocityPerAbsolutePitch">A list of 12 velocity values (range [1..127] in order of absolute pitch</param>
        public void SetVelocityPerAbsolutePitch(IReadOnlyList<int> velocityPerAbsolutePitch)
        {
            #region conditions
            Debug.Assert(velocityPerAbsolutePitch.Count == 12);
            for(int i = 0; i < 12; ++i)
            {
                int v = velocityPerAbsolutePitch[i];
                AssertIsVelocityValue(v);
            }
            Debug.Assert(this._pitches.Count == Velocities.Count);
            #endregion conditions

            SetVelocityPerAbsolutePitch(velocityPerAbsolutePitch);
       
            CheckAllVelocities();
        }

        /// <summary>
        /// Check that all velocities are in range 1..127. 
        /// If a function can set velocities, then it should call this function.
        /// It should probably also call SetNotatedValuesFromFirstBMCD().
        /// </summary>
        private void CheckAllVelocities()
        {
            foreach(int velocity in Velocities)
            {
                AssertIsVelocityValue(velocity);
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
        public void SetVelocityFromDuration(int msDurationRangeMin, int msDurationRangeMax, int velocityForMinMsDuration, int velocityForMaxMsDuration, double percent = 100.0)
        {
            Debug.Assert(MsDuration >= msDurationRangeMin && MsDuration <= msDurationRangeMax);
            Debug.Assert(msDurationRangeMin <= msDurationRangeMax);
            // velocityForMinMsDuration can be less than, equal to, or greater than velocityForMaxMsDuration
            AssertIsVelocityValue(velocityForMinMsDuration);
            AssertIsVelocityValue(velocityForMaxMsDuration);
            Debug.Assert(percent >= 0 && percent <= 100);

            double factorForNewValue = percent / 100;
            double factorForOldValue = 1 - factorForNewValue;

            double msDurationRange = msDurationRangeMax - msDurationRangeMin;
            double velocityRange = velocityForMaxMsDuration - velocityForMinMsDuration;
            int newVelocity = velocityForMinMsDuration;
            if(msDurationRange != 0)
            {
                double factor = ((double)(MsDuration - msDurationRangeMin)) / msDurationRange;
                int increment = (int)(factor * velocityRange);
                newVelocity = VelocityValue(velocityForMinMsDuration + increment);
            }

            for(int j = 0; j < Velocities.Count; ++j)
            {
                int oldVelocity = Velocities[j];
                int valueToSet = VelocityValue((int)Math.Round((oldVelocity * factorForOldValue) + (newVelocity * factorForNewValue)));
                Velocities[j] = valueToSet;
            }
        }

        #region SetVerticalVelocityGradient
        /// <summary>
        /// The arguments must both be in range [1..127].
        /// The velocities of the root and top notes in the chord are set to the argument values, and the other velocities
        /// are interpolated linearly.
        /// The root velocities change proportionaly to any change in Velocities[0].
        /// The verticalVelocityFactor is (((double)topVelocity) / rootVelocity). 
        /// </summary>
        public void SetVerticalVelocityGradient(int rootVelocity, int topVelocity)
        {
            #region conditions
            AssertIsVelocityValue(rootVelocity);
            AssertIsVelocityValue(topVelocity);
            AssertConsistency();
            #endregion conditions

            int nVelocities = Velocities.Count;
            double increment = (((double)(topVelocity - rootVelocity)) / (nVelocities - 1));
            double newVelocity = rootVelocity;
            for(int velocityIndex = 0; velocityIndex < nVelocities; ++velocityIndex)
            {
                int newVel = (int)Math.Round(newVelocity);
                newVel = VelocityValue(newVel);
                Velocities[velocityIndex] = newVel;
                newVelocity += increment;
            }

            double rootVelocityFactor = ((double)rootVelocity) / Velocities[0];
            double verticalVelocityFactor = ((double)topVelocity) / rootVelocity;

            rootVelocity = VelocityValue((int)(Math.Round(Velocities[0] * rootVelocityFactor)));
            topVelocity = VelocityValue((int)(Math.Round(rootVelocity * verticalVelocityFactor)));
            SetVerticalVelocityGradient(rootVelocity, topVelocity);

            AssertConsistency();
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
        /// Returns the argument as a int coerced to the range 0..127.
        /// </summary>
        private static int MidiValue(int value)
        {
            value = (value >= 0) ? value : 1;
            value = (value <= 127) ? value : 127;
            return value;
        }

        /// <summary>
        /// Returns the argument as a int coerced to the range 1..127.
        /// </summary>
        private int VelocityValue(int velocity)
        {
            velocity = (velocity >= 1) ? velocity : 1;
            velocity = (velocity <= 127) ? velocity : 127;
            return velocity;
        }
        #endregion utilities

 
        /// <summary>
        /// Transposes the Pitches by the number of semitones given in the argument interval.
        /// Negative interval values transpose down.
        /// It is not an error if Midi values would exceed the range 0..127.
        /// In this case, they are silently coerced to 0 or 127 respectively.
        /// If pitches become duplicated at the extremes, the duplicates are removed.
        /// </summary>
        public void Transpose(int interval)
        {
            for(int i = 0; i < _pitches.Count; ++i)
            {
                _pitches[i] = MidiValue(_pitches[i] + interval);
            }
            RemoveDuplicateNotes(_pitches, Velocities);
        }

        /// <summary>
        /// All the pitches in the MidiChordDef must be contained in the mode.
        /// Transposes the Pitches by the number of steps in the mode.
        /// Negative values transpose down.
        /// The vertical velocity sequence remains unchanged except when notes are removed.
        /// It is not an error if Midi values would exceed the range of the mode.
        /// In this case, they are silently coerced to the bottom or top notes of the mode respectively.
        /// Duplicate top and bottom mode pitches are removed.
        /// </summary>
        public void TransposeStepsInModeGamut(Mode mode, int steps)
        {
            #region conditions
            Debug.Assert(mode != null);
            foreach(int pitch in _pitches)
            {
                Debug.Assert(mode.Gamut.Contains(pitch));
            }
            #endregion conditions

            int bottomMostPitch = mode.Gamut[0];
            int topMostPitch = mode.Gamut[mode.Gamut.Count - 1];

            for(int i = 0; i < _pitches.Count; ++i)
            {
                _pitches[i] = TransposedPitchInModeGamut(_pitches[i], mode, steps);
            }
            RemoveDuplicateNotes(_pitches, Velocities);
        }

        /// <summary>
        /// The rootPitch and all the pitches in the MidiChordDef must be contained in the mode's Gamut.
        /// The vertical velocity sequence remains unchanged except when notes are removed because they are duplicates.
        /// Calculates the number of steps to transpose, and then calls TransposeStepsInModeGamut.
        /// When this function returns, rootPitch is the lowest pitch in both BasicMidiChordDefs[0] and NotatedMidiPitches.
        /// </summary>
        public void TransposeToRootInModeGamut(Mode mode, int rootPitch)
        {
            AssertConsistency();

            #region conditions
            Debug.Assert(mode != null);
            Debug.Assert(mode.Gamut.Contains(rootPitch));
            Debug.Assert(mode.Gamut.Contains(_pitches[0]));
            #endregion conditions

            int stepsToTranspose = mode.IndexInGamut(rootPitch) - mode.IndexInGamut(_pitches[0]);

            // checks that all the pitches are in the mode.
            TransposeStepsInModeGamut(mode, stepsToTranspose);

            AssertConsistency();
        }

        private int TransposedPitchInModeGamut(int initialPitch, Mode mode, int steps)
        {
            int index = mode.IndexInGamut(initialPitch);
            int newIndex = index + steps;
            int modeGamutCount = mode.Gamut.Count;

            newIndex = (newIndex >= 0) ? newIndex : 0;
            newIndex = (newIndex < modeGamutCount) ? newIndex : modeGamutCount - 1;

            return mode.Gamut[newIndex];
        }

        private void RemoveDuplicateNotes(List<int> pitches, List<int> velocities)
        {
            Debug.Assert(pitches.Count == velocities.Count);
            pitches.Sort(); // just to be sure
            List<int> indicesOfPitchesToRemove = new List<int>();
            for(int i = 1; i < pitches.Count; ++i)
            {
                if(pitches[i] == pitches[i - 1])
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

        internal void AdjustVelocities(double factor)
        {
            for(int j = 0; j < Velocities.Count; ++j)
            {
                Velocities[j] = M.VelocityValue((int)(Velocities[j] * factor));
            }
        }


        #region properties
        private List<int> _pitches = new List<int>();
        public List<int> Pitches
        {
            get => _pitches;
            set
            {
                foreach(int pitch in value)
                {
                    AssertIsMidiValue(pitch);
                }
                _pitches = value;
            }         
        }

        private List<int> _velocities = new List<int>();
        public List<int> Velocities
        {
            get => _velocities;
            set
            {
                foreach(int velocity in value)
                {
                    AssertIsVelocityValue(velocity);    
                }
                _velocities = value;
            }
        }
        public bool HasChordOff { get; set; } = true;
        public int? MsDurationToNextBarline { get; set; } = null;

        public List<MidiChordDef> MidiDefs { get; set; } = new List<MidiChordDef>();

        public bool BeamContinues { get; set; } = true;
        public string Lyric { get; set; } = null;
        /// <summary>
        /// If not null, this is simply a string that is printed after a tilde, above or below the chord.
        /// </summary>
        public string OrnamentText { get; private set; } = null;
        public MidiChordControlDef MidiChordControlDef { get; set; } = null;
        /// <summary>
        /// See ControlEnvelope: a class that converts an EnvelopeTypeDef into a series of control values with a specific Count.
        /// </summary>
        public Tuple<int, List<int>> EnvelopeTypeDef { get; set; } = null;

        #endregion properties
    }
}
