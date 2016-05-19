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
        /// A MidiChordDef containing a single BasicMidiChordDef. Absent fields are set to 0 or null.
        /// The pitches arguments are used to set both the NotatedMidiPitches and BasicMidiChordDefs[0].Pitches.
        /// All pitches are given the same velocity.
        /// The pitchHierarchy argument contains a list of _absolute_pitches_ (in range [0..11]. 0 is C natural, 1 is C# etc.
        /// First the root pitch is found, then pitches are added to the top of the chord (from the absolute pitchHierarchy)
        /// until the chord has density pitches, whereby the minimum number of octaves is added to each absolute pitch as necessary.
        /// A Debug.Assertion will fail if an attempt is made to create a pitch whose value exceeds 127.
        /// </summary>
        /// <param name="density">The number of pitches in the chord. (range [1..12])</param>
        /// <param name="rootOctave">The octave for the chord's root midiPitch (range [0..10]).
        /// (The root midiPitch will be 60, middle C, if rootOctave == 5 and pitchHierarchy[0] == 0.)</param>
        /// <param name="pitchHierarchy">A list _absolute_pitches_ (in range [0..11]. Count is in range [density..12]. Duplicate pitches are allowed.</param>
        /// <param name="velocity">All notes are given this velocity (range [1..127])</param>
        /// <param name="msDuration">The chord's msDuration (greater than 0).</param>
        /// <param name="hasChordOff">Does the chord have a chordOff?</param>
        public MidiChordDef(int density, int rootOctave, List<int> pitchHierarchy, int velocity, int msDuration, bool hasChordOff)
            : base(msDuration)
        {
            #region conditions
            Debug.Assert(density > 0 && density <= 12);
            Debug.Assert(rootOctave >= 0 && rootOctave <= 10);
            Debug.Assert(pitchHierarchy.Count >= density && pitchHierarchy.Count <= 12);
            foreach(byte pitch in pitchHierarchy)
                Debug.Assert(pitch >= 0 && pitch <= 11); // can include duplicates.
            Debug.Assert(velocity > 0 && velocity <= 127);
            Debug.Assert(msDuration > 0);
            #endregion conditions

            _msPositionReFirstIUD = 0; // default value
            _hasChordOff = hasChordOff;
            _minimumBasicMidiChordMsDuration = 1; // not used (this is not an ornament)

            List<byte> pitches = GetPitches(density, rootOctave, pitchHierarchy);
            List<byte> velocities = new List<byte>();
            foreach(byte pitch in pitches)
            {
                velocities.Add((byte)velocity);
            } 

            _notatedMidiPitches = pitches;
            _notatedMidiVelocities = velocities;

            _ornamentNumberSymbol = 0;

            MidiChordSliderDefs = null;

            byte? bank = null;
            byte? patch = null;

            BasicMidiChordDefs.Add(new BasicMidiChordDef(msDuration, bank, patch, hasChordOff, pitches, velocities));

            CheckTotalDuration();
        }
        private List<byte> GetPitches(int density, int rootOctave, List<int> pitchHierarchy)
        {
            List<int> pitches = new List<int>();
            pitches.Add(pitchHierarchy[0] + (rootOctave * 12));
            for(int i = 1; i < density; ++i )
            {
                int pitch = pitchHierarchy[i];
                while(pitch <= pitches[i-1])
                {
                    pitch += 12;
                }
                pitches.Add(pitch);
            }

            List<byte> bytePitches = new List<byte>();
            foreach(int pitch in pitches)
            {
                //Debug.Assert(pitch >= 0 && pitch <= 127);
                if(pitch <= 127)
                {
                    bytePitches.Add((byte)pitch);
                }
            }
            return bytePitches;
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
            MidiChordSliderDefs midiChordSliderDefs, // can contain empty lists
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

            _lyric = null;
            _ornamentNumberSymbol = ornamentNumberSymbol;
            _lyric = null;

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
        public override IUniqueDef Clone()
        {
            MidiChordDef rval = new MidiChordDef();

            rval.MsPositionReFirstUD = this.MsPositionReFirstUD;
            // rval.MsDuration must be set after setting BasicMidiChordDefs See below.
            rval.Bank = this.Bank;
            rval.Patch = this.Patch;
            rval.PitchWheelDeviation = this.PitchWheelDeviation;
            rval.HasChordOff = this.HasChordOff;
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
            rval.BaseMidiVelocity = this.BaseMidiVelocity; // needed for displaying dynamics (must be set *after* setting BasicMidiChordDefs)

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

        #region Boulez addition (commented out)
        //      /// <summary>
        //      /// Boulez Addition: (Does not affect the sliders in this MidiChordDef.)
        //      /// Notes (Pitches and their Velocities) in this MidiChordDef's BasicMidiChordDefs are preserved. The pitches and
        //      /// velocities in the notes in each BasicMidiChordDef, and used as roots to which the pitch and velocity intervals
        //      /// in mcd2's first BasicMidiChordDef are added.
        //      /// A note is added only if its pitch is not already present, and its velocity is greater than 0.
        //      /// A velocity that would be greater than 127 is coerced to 127.
        //      /// When the BasicMidiChordDefs are complete, this MidiChordDef's NotatedMidiPitches are set to the pitches in
        //      /// the first BasicMidiChordDef.
        //      /// </summary>
        //      /// <param name="mcd2"></param>
        //      public MidiChordDef AddNotes(MidiChordDef mcd2)
        //{
        //	Debug.Assert(mcd2.BasicMidiChordDefs[0].Pitches.Count > 1, "The chord to be added must have at least two pitches.");

        //	List<int> pitchIntervalsToAdd = GetIntervals(mcd2.BasicMidiChordDefs[0].Pitches);
        //	// velocityIntervals can be negative
        //	List<int> velocityIntervalsToAdd = GetIntervals(mcd2.BasicMidiChordDefs[0].Velocities);

        //	foreach(BasicMidiChordDef bmcd in BasicMidiChordDefs)
        //	{
        //		List<byte> newPitches = new List<byte>();
        //		List<byte> newVelocities = new List<byte>();
        //		for(int i = 0; i < bmcd.Pitches.Count; ++i)
        //		{
        //			byte rootPitch = bmcd.Pitches[i];
        //			byte rootVelocity = bmcd.Velocities[i];

        //			newPitches.Add(rootPitch);
        //			newVelocities.Add(rootVelocity);

        //			for(int j = 0; j < pitchIntervalsToAdd.Count; ++j)
        //			{
        //				int pitchInterval = pitchIntervalsToAdd[j];
        //				int velocityInterval = velocityIntervalsToAdd[j];

        //				int newPitch = rootPitch + pitchInterval;
        //				int newVelocity = rootVelocity + velocityInterval;

        //				if(newPitch <= 127 
        //				&& (!newPitches.Contains((byte)newPitch))
        //				&& (!bmcd.Pitches.Contains((byte)newPitch))
        //				&& newVelocity > 0)
        //				{
        //					newVelocity = (newVelocity < 128) ? newVelocity : 127;

        //					newPitches.Add((byte)newPitch);
        //					newVelocities.Add((byte)newVelocity);
        //				}
        //			}
        //		}
        //		SortPitchesAndVelocities(newPitches, newVelocities);

        //		bmcd.Pitches = newPitches;
        //		bmcd.Velocities = newVelocities;
        //	}

        //	_notatedMidiPitches = new List<byte>(BasicMidiChordDefs[0].Pitches);

        //	return this; // this function can be chained
        //}

        //private void SortPitchesAndVelocities(List<byte> newPitches, List<byte> newVelocities)
        //{
        //	List<byte> sortedPitches = new List<byte>();
        //	List<byte> sortedVelocities = new List<byte>();
        //	while(newPitches.Count > 0)
        //	{
        //		byte smallestPitch = newPitches[0];
        //		int indexOfSmallestPitch = 0;
        //		for(int i = 0; i < newPitches.Count; ++i)
        //		{
        //			if(newPitches[i] < smallestPitch)
        //			{
        //				smallestPitch = newPitches[i];
        //				indexOfSmallestPitch = i;
        //			}
        //		}
        //		sortedPitches.Add(newPitches[indexOfSmallestPitch]);
        //		sortedVelocities.Add(newVelocities[indexOfSmallestPitch]);

        //		newPitches.RemoveAt(indexOfSmallestPitch);
        //		newVelocities.RemoveAt(indexOfSmallestPitch);
        //	}
        //	for(int i = 0; i < sortedPitches.Count; ++i)
        //	{
        //		newPitches.Add(sortedPitches[i]);
        //		newVelocities.Add(sortedVelocities[i]);	
        //	}
        //}

        //private List<int> GetIntervals(List<byte> bytes)
        //{
        //	List<int> intervals = new List<int>();

        //	for(int i = 1; i < bytes.Count; ++i)
        //	{
        //		intervals.Add(bytes[i] - bytes[0]);
        //	}
        //	return intervals;
        //}

        #endregion Boulez addition (commented out)

        #region AddNotes(MidiChordDef mcd2)
        /// <summary>
        /// Adds midiPitches and midiVelocities from the argument to this MidiChordDef's pitches and velocities.
        /// The argument MidiChordDef is not changed.
        /// Neither the original MidiChordDef nor the argument may contain more than one BasicMidiChordDef.
        /// The argument's NotatedMidiPitches and BasicMidiChordDefs[0].Pitches are used to set this MidiChordDef's
        /// NotatedMidiPitches and BasicMidiChordDefs[0].Pitches respectively.
        /// If a pitch already exists, the larger of the two velocities is used, otherwise the new pitch is
        /// inserted in the current pitch list at the appropriate position so that pitches continue to be in
        /// ascending order, and the new velocity is inserted at the corresponding position in the velocities list.
        /// </summary>
        public void AddNotes(MidiChordDef mcd2)
        {
            Debug.Assert(BasicMidiChordDefs.Count == 1);
            Debug.Assert(mcd2.BasicMidiChordDefs.Count == 1);

            List<byte> pitches = new List<byte>(mcd2.NotatedMidiPitches);
            List<byte> velocities = new List<byte>(mcd2.NotatedMidiVelocities);

            Debug.Assert(pitches.Count == velocities.Count);

            _AddNotes(_notatedMidiPitches, _notatedMidiVelocities, pitches, velocities);

            pitches = new List<byte>(mcd2.BasicMidiChordDefs[0].Pitches);
            velocities = new List<byte>(mcd2.BasicMidiChordDefs[0].Velocities);

            Debug.Assert(pitches.Count == velocities.Count);

            _AddNotes(BasicMidiChordDefs[0].Pitches, BasicMidiChordDefs[0].Velocities, pitches, velocities);
        }

        private void _AddNotes(List<byte> existingMidiPitches, List<byte> existingMidiVelocities, List<byte> pitches, List<byte> velocities)
        {

            for(int i = 0; i < pitches.Count; ++i)
            {
                byte pitch = pitches[i];
                byte velocity = velocities[i];
                bool found = false;
                int index = existingMidiPitches.Count; // default is append at top of list
                for(int j = 0; j < existingMidiPitches.Count; ++j)
                {
                    if(existingMidiPitches[j] == pitch)
                    {
                        index = j;
                        found = true;
                        break;
                    }
                    if(existingMidiPitches[j] > pitch)
                    {
                        index = j;
                        break;
                    }
                }
                if(found)
                {
                    existingMidiVelocities[index] = (existingMidiVelocities[index] > velocity) ? existingMidiVelocities[index] : velocity;
                }
                else
                {
                    existingMidiPitches.Insert(index, pitch);
                    existingMidiVelocities.Insert(index, velocity);
                }
            }
        }

        #endregion AddNotes(MidiChordDef mcd2)

        #region SetVerticalVelocityGradient
        /// <summary>
        /// The arguments are both in range [1..127].
        /// This function changes the velocities in both the notated chord and the BasicChordDefs.
        /// The velocities of the root and top notes in the chord are set to the argument values, and the other velocities
        /// are interpolated linearly. 
        /// </summary>
        public void SetVerticalVelocityGradient(byte rootVelocity, byte topVelocity)
        {
            #region conditions
            Debug.Assert(rootVelocity > 0 && rootVelocity <= 127);
            Debug.Assert(topVelocity > 0 && topVelocity <= 127);
            #endregion conditions

            double rootVelocityFactor = ((double)rootVelocity) / NotatedMidiVelocities[0];
            double verticalVelocityFactor = ((double)topVelocity) / rootVelocity;

            SetVelocities(NotatedMidiVelocities, rootVelocityFactor, verticalVelocityFactor);
            
            foreach(BasicMidiChordDef bmcd in BasicMidiChordDefs)
            {
                SetVelocities(bmcd.Velocities, rootVelocityFactor, verticalVelocityFactor);
            }
        }
        /// <summary>
        /// Called by the above function
        /// </summary>
        private void SetVelocities(List<byte> velocities, double rootVelocityFactor, double verticalVelocityFactor)
        {
            if(velocities.Count > 1)
            {
                byte rootVelocity = M.MidiValue((int)(Math.Round(velocities[0] * rootVelocityFactor)));
                byte topVelocity = M.MidiValue((int)(Math.Round(rootVelocity * verticalVelocityFactor)));
                double increment = (((double)(topVelocity - rootVelocity)) / (velocities.Count - 1));
                double newVelocity = rootVelocity;
                for(int velocityIndex = 0; velocityIndex < velocities.Count; ++velocityIndex)
                {
                    velocities[velocityIndex] = M.MidiValue((int)Math.Round(newVelocity));
                    newVelocity += increment;
                }
            }
        }
        #endregion SetVerticalVelocityGradient

        #region SetVelocityPerAbsolutePitch
        /// <summary>
        /// The argument contains a list of 12 velocity values (range [0..127] in order of absolute pitch.
        /// For example: If the MidiChordDef contains one or more C#s, they will be given velocity velocityPerAbsolutePitch[1].
        /// Middle-C is midi pitch 60 (60 % 12 == absolute pitch 0), middle-C# is midi pitch 61 (61 % 12 == absolute pitch 1), etc.
        /// This function applies equally to all the BasicMidiChordDefs in this MidiChordDef. 
        /// </summary>
        /// <param name="velocityPerAbsolutePitch">A list of 12 velocity values (range [0..127] in order of absolute pitch</param>
        public void SetVelocityPerAbsolutePitch(List<int> velocityPerAbsolutePitch)
        {
            #region conditions
            Debug.Assert(velocityPerAbsolutePitch.Count == 12);
            for(int i = 0; i < 12; ++i)
            {
                int v = velocityPerAbsolutePitch[i];
                Debug.Assert(v >= 0 && v <= 127);
            }
            #endregion conditions

            Debug.Assert(this.NotatedMidiPitches.Count == NotatedMidiVelocities.Count);
            for(int pitchIndex = 0; pitchIndex < NotatedMidiPitches.Count; ++pitchIndex)
            {
                int absPitch = NotatedMidiPitches[pitchIndex] % 12;
                NotatedMidiVelocities[pitchIndex] = (byte)velocityPerAbsolutePitch[absPitch];
            }

            foreach(BasicMidiChordDef bmcd in BasicMidiChordDefs)
            {
                List<byte> pitches = bmcd.Pitches;
                List<byte> velocities = bmcd.Velocities;
                Debug.Assert(pitches.Count == velocities.Count);
                for(int pitchIndex = 0; pitchIndex < pitches.Count; ++pitchIndex)
                {
                    int absPitch = pitches[pitchIndex] % 12;
                    velocities[pitchIndex] = (byte)velocityPerAbsolutePitch[absPitch];
                }
            }
        }
        #endregion SetVelocityPerAbsolutePitch

        #region IUniqueChordDef
        /// <summary>
        /// Transposes the pitches in NotatedMidiPitches, and all BasicMidiChordDef.Pitches by the number of semitones
        /// given in the argument interval. Negative interval values transpose down.
        /// It is not an error if Midi values would exceed the range 0..127.
        /// In this case, they are silently coerced to 0 or 127 respectively.
        /// Duplicate 0 and 127 pitches are removed.
        /// </summary>
        public void Transpose(int interval)
        {
            for(int i = 0; i < _notatedMidiPitches.Count; ++i)
            {
                _notatedMidiPitches[i] = (byte) M.MidiValue(_notatedMidiPitches[i] + interval);
            }
            RemoveDuplicate0And127Pitches(_notatedMidiPitches, _notatedMidiVelocities);

            foreach(BasicMidiChordDef bmcd in BasicMidiChordDefs)
            {
                for(int i = 0; i < bmcd.Pitches.Count; ++i)
                {
                    bmcd.Pitches[i] = (byte)M.MidiValue(bmcd.Pitches[i] + interval);
                }
            }
            foreach(BasicMidiChordDef bmcd in BasicMidiChordDefs)
            {
				List<byte> pitches = bmcd.Pitches;
				List<byte> velocities = bmcd.Velocities;
                RemoveDuplicate0And127Pitches(pitches, velocities);
            }
        }

        private void RemoveDuplicate0And127Pitches(List<byte> pitches, List<byte> velocities)
        {
            Debug.Assert(pitches.Count == velocities.Count);
            bool found0 = false;
            bool found127 = false;
            for(int i = pitches.Count - 1; i >= 0; --i)
            {
                if(pitches[i] == 0)
                {
                    if(found0 == true)
                    {
                        pitches.RemoveAt(i);
                        velocities.RemoveAt(i);
                    }
                    found0 = true;
                }
                if(pitches[i] == 127)
                {
                    if(found127 == true)
                    {
                        pitches.RemoveAt(i);
                        velocities.RemoveAt(i);
                    }
                    found127 = true;
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
        /// Calls Transpose(interval).
        /// Gets BasicMidiChordDefs[0].Pitches[0].
        /// Sets BasicMidiChordDefs[0].Pitches[0] to value, transposing the other pitches accordingly.
        /// </summary>
        public byte BaseMidiPitch
        {
            get { return BasicMidiChordDefs[0].Pitches[0]; }
            set
            {
                Debug.Assert(BasicMidiChordDefs != null && BasicMidiChordDefs.Count > 0
                    && BasicMidiChordDefs[0].Pitches != null && BasicMidiChordDefs[0].Pitches.Count > 0);

                if(value != BasicMidiChordDefs[0].Velocities[0])
                {
                    int interval = value - BasicMidiChordDefs[0].Velocities[0];
                    Transpose(interval);
                }
            }
        }

        /// <summary>
        /// Calls AdjustVelocities(factor).
        /// Gets BasicMidichordDefs[0].Velocities[0].
        /// Sets BasicMidiChordDefs[0].Velocities[0] to value, and the other velocities so that the original proportions are kept.
        /// </summary>
        public byte BaseMidiVelocity
		{
			get { return BasicMidiChordDefs[0].Velocities[0]; }
			set
			{
				Debug.Assert(BasicMidiChordDefs != null && BasicMidiChordDefs.Count > 0
					&& BasicMidiChordDefs[0].Velocities != null && BasicMidiChordDefs[0].Velocities.Count > 0);

				if(value != BasicMidiChordDefs[0].Velocities[0])
				{
					double factor = (((double)value) / BasicMidiChordDefs[0].Velocities[0]);
					AdjustVelocities(factor);
				}
			}
		}

        /// <summary>
        /// Sets the number of values in NotatedMidiPitches, NotatedMidiVelocities, and all BasicMidiChordDef.Pitches and
        /// BasicMidiChordDef.Velocities to newDensity, by removing the upper pitches as necessary.
        /// Requires newDensity to be less than or equal to the current vertical density, and the lengths of all
        /// the affected lists to be the same.
        /// ACHTUNG: this function can't be used if an ornament has added pitches to a BasicMidiChordDef. In other
        /// words, the "note density factors" field in the Ornaments dialog must only contain 1s.
        /// </summary>
        public void SetVerticalDensity(int newDensity)
		{
            int currentDensity = _notatedMidiPitches.Count;
            #region require
            Debug.Assert(newDensity <= currentDensity); // if its equal, do nothing
            Debug.Assert(currentDensity == _notatedMidiVelocities.Count);
            foreach(BasicMidiChordDef bmcd in BasicMidiChordDefs)
			{
				Debug.Assert(currentDensity == bmcd.Pitches.Count
                 && currentDensity == bmcd.Velocities.Count);
			}
            #endregion require

            int nElementsToRemove = currentDensity - newDensity;
            if(nElementsToRemove > 0)
            {
                _notatedMidiPitches.RemoveRange(newDensity, nElementsToRemove);
                _notatedMidiVelocities.RemoveRange(newDensity, nElementsToRemove);
                foreach(BasicMidiChordDef bmcd in BasicMidiChordDefs)
				{
					bmcd.Pitches.RemoveRange(newDensity, nElementsToRemove);
					bmcd.Velocities.RemoveRange(newDensity, nElementsToRemove);
				}
			}
		}

		#region IUniqueDef
		public override string ToString()
        {
            return ("MsPositionReFirstIUD=" + MsPositionReFirstUD.ToString() + " MsDuration=" + MsDuration.ToString() + " MidiChordDef");
        }

        /// <summary>
        /// Multiplies the MsDuration by the given factor.
        /// </summary>
        /// <param name="factor"></param>
        public void AdjustMsDuration(double factor)
        {
            MsDuration = (int)(_msDuration * factor);
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
