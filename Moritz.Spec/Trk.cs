using Krystals5ObjectLibrary;

using Moritz.Globals;

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Moritz.Spec
{
    /// <summary>
	/// Trks are "OutputVoiceDefs".
    /// In Seqs, Trks can contain any combination of MidiRestDef and MidiChordDef.
    /// In Blocks, Trks can additionally contain CautionaryChordDefs and ClefDefs.
    /// <para>Trks are, like all VoiceDef objects, IEnumerable, so that foreach loops can be used.</para>
    /// <para>For example:</para>
    /// <para>foreach(IUniqueDef iumdd in trk) { ... }</para>
    /// <para>An Enumerator for MidiChordDefs is also defined so that</para>
    /// <para>foreach(MidiChordDef mcd in trkDef.MidiChordDefs) { ... }</para>
    /// <para>can also be used.</para>
    /// <para>This class is also indexable, as in:</para>
    /// <para>IUniqueDef iu = trk[index];</para>
    /// </summary>
    public class Trk : VoiceDef, ICloneable
    {
        #region constructors
        public Trk(int midiChannel, int msPositionReContainer, List<IUniqueDef> iuds)
            : base(midiChannel, msPositionReContainer, iuds)
        {
            AssertConsistency();
        }

        /// <summary>
        /// A Trk with msPositionReContainer=0 and an empty UniqueDefs list.
        /// </summary>
        public Trk(int midiChannel)
            : base(midiChannel, 0, new List<IUniqueDef>())
        {
        }

        /// <summary>
        /// Returns a deep clone of this Trk.
        /// </summary>
        public object Clone()
        {
            List<IUniqueDef> clonedIUDs = GetUniqueDefsClone();
            Trk trk = new Trk(MidiChannel, MsPositionReContainer, clonedIUDs) { Container = this.Container };

            return trk;
        }

        /// <summary>
        /// Also used by Clone() functions in subclasses
        /// </summary>
        protected List<IUniqueDef> GetUniqueDefsClone()
        {
            List<IUniqueDef> clonedIUDs = new List<IUniqueDef>();
            foreach(IUniqueDef iu in _uniqueDefs)
            {
                IUniqueDef clonedIUD = (IUniqueDef)iu.Clone();
                clonedIUDs.Add(clonedIUD);
            }
            return clonedIUDs;
        }

        #endregion constructors
        /// <summary>
        /// When this function is called, VoiceDef.AssertConsistency() is called (see documentation there), then
        /// the following are checked:
        /// 1. MidiChannels are in range [0..15]
        /// 2. The Container is either null, a Seq or a Bar.
        /// 3. If the Container is a Seq, the UniqueDefs contain any combination of MidiRestDef and MidiChordDef.
        ///    If the Container is a Bar, the UniqueDefs can also contain ClefDef and CautionaryChordDef objects.
        /// </summary>
        public override void AssertConsistency()
        {
            base.AssertConsistency();

            Debug.Assert(MidiChannel >= 0 && MidiChannel <= 15);

            Debug.Assert(Container == null || Container is Seq || Container is Bar);

            if(Container == null || Container is Seq)
            {
                foreach(IUniqueDef iud in UniqueDefs)
                {
                    Debug.Assert(iud is MidiChordDef || iud is MidiRestDef);
                }
            }
            else if(Container is Bar)
            {
                foreach(IUniqueDef iud in UniqueDefs)
                {
                    Debug.Assert(iud is MidiChordDef || iud is MidiRestDef || iud is ClefDef || iud is CautionaryChordDef);
                }
            }
        }


        #region Add, Remove, Insert, Replace objects in the Trk
        /// <summary>
        /// Appends the new MidiChordDef, MidiRestDef, CautionaryChordDef or ClefDef to the end of the list.
        /// Automatically sets the iUniqueDef's msPosition.
        /// N.B. Can be used to Add CautionaryChordDef and ClefDef arguments.
        /// </summary>
        public override void Add(IUniqueDef iUniqueDef)
        {
            Debug.Assert(iUniqueDef is MidiChordDef || iUniqueDef is MidiRestDef || iUniqueDef is CautionaryChordDef || iUniqueDef is ClefDef);
            _Add(iUniqueDef);
        }
        /// <summary>
        /// Moves the argument's UniqueDefs to the end of this Trk _without_cloning_them_.
        /// Sets the MsPositions of the appended UniqueDefs.
        /// This function automatically agglommerates rests.
        /// </summary>
        public override void AddRange(VoiceDef trk)
        {
            Debug.Assert(trk is Trk);
            _AddRange(trk);
        }

        /// <summary>
        /// Appends **clones** of the trk2.UniqueDefs after the end of this trk's UniqueDefs at msPositionReThisTrk, inserting a rest as necessary.
        /// To simply concatenate trk2 with this trk, pass this trk's MsDuration as the second argument.
        /// An exception will be thrown if msPositionReThisTrk is less than the current duration of this trk.
        /// Any rests that result from this operation are silently agglommerated.
        /// The MsPositionReFirstUD values in the concatenated IUniqueDefs are automatically adjusted for this trk. 
        /// </summary>
        public void ConcatCloneAt(Trk trk2, int msPositionReThisTrk)
        {
            Debug.Assert(MsDuration <= msPositionReThisTrk);
            if(msPositionReThisTrk > MsDuration)
            {
                int msDuration = (msPositionReThisTrk - MsDuration);
                IUniqueDef lastIud = _uniqueDefs[_uniqueDefs.Count - 1];
                if(lastIud is MidiRestDef finalRestDef) // Trks are "OutputVoiceDefs" so cannot contain InputRestDefs.
                {
                    lastIud.MsDuration += msDuration;
                }
                else
                {
                    MidiRestDef midiRestDef = new MidiRestDef(0, msDuration);
                    this.Add(midiRestDef);
                }
            }

            foreach(IUniqueDef iu2 in trk2.UniqueDefs)
            {
                IUniqueDef lastIud = (_uniqueDefs.Count > 0) ? _uniqueDefs[_uniqueDefs.Count - 1] : null;
                IUniqueDef clonedIUD = (IUniqueDef)iu2.Clone();

                if(_uniqueDefs.Count > 0 && (lastIud is RestDef finalRestDef && clonedIUD is RestDef restDef2))
                {
                    finalRestDef.MsDuration += restDef2.MsDuration;
                }
                else
                {
                    this._Add(clonedIUD);
                }
            }
        }

        /// <summary>
        /// Inserts the iUniqueDef in the list at the given index, and then
        /// resets the positions of all the uniqueDefs in the list.
        /// </summary>
        public override void Insert(int index, IUniqueDef iUniqueDef)
        {
            Debug.Assert(iUniqueDef is MidiChordDef || iUniqueDef is MidiRestDef || iUniqueDef is ClefDef);
            _Insert(index, iUniqueDef);
        }
        /// <summary>
        /// Inserts the trk's UniqueDefs in the list at the given index, and then
        /// resets the positions of all the uniqueDefs in the list.
        /// </summary>
        public virtual void InsertRange(int index, Trk trk)
        {
            _InsertRange(index, trk);
        }
        /// <summary>
        /// Removes the iUniqueDef at index from the list, and then inserts the replacement at the same index.
        /// </summary>
        public virtual void Replace(int index, IUniqueDef replacementIUnique)
        {
            Debug.Assert(replacementIUnique is MidiChordDef || replacementIUnique is MidiRestDef);
            _Replace(index, replacementIUnique);
        }
        #region Superimpose
        /// <summary> 
        /// This function attempts to add all the non-MidiRestDef UniqueDefs in trk2 to the calling Trk
        /// at the positions given by their MsPositionReFirstIUD added to trk2.MsPositionReContainer,
        /// whereby trk2.MsPositionReContainer is used with respect to the calling Trk's container.
        /// Before doing the superimposition, the calling Trk is given leading and trailing RestDefs
        /// so that trk2's uniqueDefs can be added at their original positions without any problem.
        /// These leading and trailing RestDefs are however removed before the function returns.
        /// The superimposed uniqueDefs will be placed at their original positions if they fit inside
        /// a MidiRestDef in the original Trk. A Debug.Assert() fails if this is not the case.
        /// To insert single uniqueDefs between existing uniqueDefs, call the function
        /// Trk.Insert(index, iudToInsert).
        /// trk2's UniqueDefs are not cloned.
        /// </summary>
        /// <returns>this</returns>
        public virtual Trk Superimpose(Trk trk2)
        {
            Debug.Assert(MidiChannel == trk2.MidiChannel);

            SuperimposeUniqueDefs(trk2);

            AssertConsistency();

            return this;
        }

        /// <summary>
        /// Does the actual superimposition.
        /// Inside this function, both Trks are aligned and may be padded at the beginning and/or end by a rest.
        /// The padding is removed before the function returns.
        /// </summary>
        /// <param name="trk2"></param>
        private void SuperimposeUniqueDefs(Trk trk2)
        {
            AlignAndJustifyWith(trk2);

            Debug.Assert(this.MsDuration == trk2.MsDuration);
            int thisIndex = 0;
            int trk2Index = 0;
            List<IUniqueDef> newUniqueDefs = new List<IUniqueDef>();
            Trk currentTrk = this;
            int currentIndex = 0;
            int currentDuration = 0;
            while(thisIndex < this.Count || trk2Index < trk2.Count)
            {
                #region get next non-rest iud in either trk
                do
                {
                    if(trk2Index == trk2.Count && thisIndex == this.Count)
                    {
                        break;
                    }
                    else if(trk2Index == trk2.Count && thisIndex < this.Count)
                    {
                        currentTrk = this;
                        currentIndex = thisIndex++;
                    }
                    else if(thisIndex == this.Count && trk2Index < trk2.Count)
                    {
                        currentTrk = trk2;
                        currentIndex = trk2Index++;
                    }
                    else // (thisIndex < this.Count && trk2Index < trk2.Count)
                    {
                        if(this[thisIndex].MsPositionReFirstUD < trk2[trk2Index].MsPositionReFirstUD)
                        {
                            currentTrk = this;
                            currentIndex = thisIndex++;
                        }
                        else if(trk2Index < trk2.Count)
                        {
                            currentTrk = trk2;
                            currentIndex = trk2Index++;
                        }
                    }
                } while(currentIndex < currentTrk.Count && currentTrk[currentIndex] is MidiRestDef);
                #endregion get next non-rest iud in either trk

                if(currentIndex < currentTrk.Count)
                {
                    IUniqueDef iudToAdd = currentTrk[currentIndex];

                    #region add iudToAdd to the newUniqueDefs
                    if(iudToAdd.MsPositionReFirstUD > currentDuration)
                    {
                        int restMsDuration = iudToAdd.MsPositionReFirstUD - currentDuration;
                        newUniqueDefs.Add(new MidiRestDef(0, restMsDuration));
                        currentDuration += restMsDuration;
                    }
                    else if(iudToAdd.MsPositionReFirstUD < currentDuration)
                    {
                        Debug.Assert(false, $"Error: Can't add UniqueDef at current position ({currentDuration}).");
                    }
                    newUniqueDefs.Add(iudToAdd);
                    currentDuration += iudToAdd.MsDuration;
                    #endregion add iudToAdd to the _uniqueDefs
                }
            }

            _uniqueDefs = newUniqueDefs;

            SetMsPositionsReFirstUD();

            Trim();
        }

        /// <summary>
        /// Makes both tracks the same length by adding rests at the beginnings and ends.
        /// The Alignment is found using this.MsPositionReContainer and trk2.MsPositionReContainer
        /// </summary>
        private void AlignAndJustifyWith(Trk trk2)
        {
            this.Trim();
            if(this.MsPositionReContainer > 0)
            {
                this.Insert(0, new MidiRestDef(0, this.MsPositionReContainer));
                this.MsPositionReContainer = 0;
            }
            trk2.Trim();
            if(trk2.MsPositionReContainer > 0)
            {
                trk2.Insert(0, new MidiRestDef(0, trk2.MsPositionReContainer));
                trk2.MsPositionReContainer = 0;
            }
            int lengthDiff = trk2.MsDuration - this.MsDuration;
            if(lengthDiff > 0)
            {
                this.Add(new MidiRestDef(0, lengthDiff));
            }
            else if(lengthDiff < 0)
            {
                trk2.Add(new MidiRestDef(0, -lengthDiff));
            }
        }

        /// <summary>
        /// Removes rests from the beginning and end of the UniqueDefs list.
        /// Updates MsPositionReContainer if necessary.
        /// </summary>
        private void Trim()
        {
            List<int> restIndicesToRemoveAtStart = new List<int>();
            for(int i = 0; i < UniqueDefs.Count; ++i)
            {
                if(UniqueDefs[i] is MidiRestDef)
                {
                    restIndicesToRemoveAtStart.Add(i);
                }
                else if(UniqueDefs[i] is MidiChordDef)
                {
                    break;
                }
            }
            List<int> restIndicesToRemoveAtEnd = new List<int>();
            for(int i = UniqueDefs.Count - 1; i >= 0; --i)
            {
                if(UniqueDefs[i] is MidiRestDef)
                {
                    restIndicesToRemoveAtEnd.Add(i);
                }
                else if(UniqueDefs[i] is MidiChordDef)
                {
                    break;
                }
            }
            for(int i = restIndicesToRemoveAtEnd.Count - 1; i >= 0; --i)
            {
                int index = restIndicesToRemoveAtEnd[i];
                RemoveAt(index);
            }
            for(int i = restIndicesToRemoveAtStart.Count - 1; i >= 0; --i)
            {
                int index = restIndicesToRemoveAtStart[i];
                MsPositionReContainer += UniqueDefs[index].MsDuration;
                RemoveAt(index);
            }
        }
        #endregion Superimpose
        #endregion Add, Remove, Insert, Replace objects in the Trk

        #region Changing the Trk's duration
        /// <summary>
        /// Multiplies the MsDuration of each midiChordDef from beginIndex to endIndex (exclusive) by factor.
        /// If a midiChordDef's MsDuration becomes less than minThreshold, it is removed.
        /// The total duration of this Trk changes accordingly.
        /// </summary>
        public void AdjustChordMsDurations(int beginIndex, int endIndex, double factor, int minThreshold = 100)
        {
            AdjustMsDurations<MidiChordDef>(beginIndex, endIndex, factor, minThreshold);
        }
        /// <summary>
        /// Multiplies the MsDuration of each midiChordDef in the UniqueDefs list by factor.
        /// If a midiChordDef's MsDuration becomes less than minThreshold, it is removed.
        /// The total duration of this Trk changes accordingly.
        /// </summary>
        public void AdjustChordMsDurations(double factor, int minThreshold = 100)
        {
            AdjustMsDurations<MidiChordDef>(0, _uniqueDefs.Count, factor, minThreshold);
        }
        #endregion Changing the Trk's duration

        #region Changing MidiChordDef attributes


        #region Envelopes

        public void SetPitchWheelSliders(Envelope envelope)
        {
            #region condition
            if(envelope.Domain != 127)
            {
                throw new ArgumentException($"{nameof(envelope.Domain)} must be 127.");
            }
            #endregion condition

            List<int> msPositions = GetMsPositions();

            Dictionary<int, int> pitchWheelValuesPerMsPosition = envelope.GetValuePerMsPosition(msPositions);

            SetPitchWheelSliders(pitchWheelValuesPerMsPosition);
        }
        private List<int> GetMsPositions()
        {
            int originalMsDuration = MsDuration;

            List<int> originalMsPositions = new List<int>();
            foreach(IUniqueDef iud in UniqueDefs)
            {
                originalMsPositions.Add(iud.MsPositionReFirstUD);
            }
            originalMsPositions.Add(originalMsDuration);

            return originalMsPositions;
        }

        /// <summary>
        /// Also used by Trks in Seq and Bar
        /// </summary>
        public void SetPitchWheelSliders(Dictionary<int, int> pitchWheelValuesPerMsPosition)
        {
            foreach(IUniqueDef iud in UniqueDefs)
            {
                if(iud is MidiChordDef mcd)
                {
                    int startMsPos = mcd.MsPositionReFirstUD;
                    int endMsPos = startMsPos + mcd.MsDuration;
                    List<int> mcdEnvelope = new List<int>();
                    foreach(int msPos in pitchWheelValuesPerMsPosition.Keys)
                    {
                        if(msPos >= startMsPos)
                        {
                            if(msPos <= endMsPos)
                            {
                                mcdEnvelope.Add(pitchWheelValuesPerMsPosition[msPos]);
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                    mcd.SetPitchWheelEnvelope(new Envelope(mcdEnvelope, 127, 127, mcdEnvelope.Count));
                }
            }
        }

        #region SetVelocityPerAbsolutePitch
        /// <summary>
        /// The arguments are passed unchanged to MidiChordDef.SetVelocityPerAbsolutePitch(...) for each MidiChordDef in this Trk.
        /// See the MidiChordDef documentation for details.
        /// </summary>
        /// <param name="velocityPerAbsolutePitch">A list of 12 velocity values (range [1..127] in order of absolute pitch</param>
        public virtual void SetVelocityPerAbsolutePitch(List<byte> velocityPerAbsolutePitch)
        {
            #region conditions
            Debug.Assert(velocityPerAbsolutePitch.Count == 12);
            for(int i = 0; i < 12; ++i)
            {
                M.AssertIsVelocityValue(velocityPerAbsolutePitch[i]);
            }
            #endregion conditions
            for(int i = 0; i < UniqueDefs.Count; ++i)
            {
                if(UniqueDefs[i] is MidiChordDef mcd)
                {
                    mcd.SetVelocityPerAbsolutePitch(velocityPerAbsolutePitch);
                    Debug.Assert(mcd.NotatedMidiPitches.Count > 0);
                }
            }
        }
        #endregion SetVelocityPerAbsolutePitch
        #region SetVelocitiesFromDurations
        /// <summary>
        /// The first two arguments must be in range [1..127].
        /// Sets the velocity of each MidiChordDef in the Trk (anti-)proportionally to its duration.
        /// The (optional) percent argument determines the proportion of the final velocity for which this function is responsible.
        /// The other component of the final velocity value is its existing velocity. If percent is 100.0, the existing velocity
        /// is replaced completely.
        /// N.B 1) Neither velocityForMinMsDuration nor velocityForMaxMsDuration can be zero! -- that would be a NoteOff.
        /// and 2) velocityForMinMsDuration can be less than, equal to, or greater than velocityForMaxMsDuration.
        /// </summary>
        /// <param name="velocityForMinMsDuration">in range 1..127</param>
        /// <param name="velocityForMaxMsDuration">in range 1..127</param>
        public virtual void SetVelocitiesFromDurations(byte velocityForMinMsDuration, byte velocityForMaxMsDuration, double percent = 100.0)
        {
            Debug.Assert(velocityForMinMsDuration >= 1 && velocityForMinMsDuration <= 127);
            Debug.Assert(velocityForMaxMsDuration >= 1 && velocityForMaxMsDuration <= 127);

            int msDurationRangeMin = int.MaxValue;
            int msDurationRangeMax = int.MinValue;

            #region find msDurationRangeMin and msDurationRangeMax 
            foreach(IUniqueDef iud in _uniqueDefs)
            {
                if(iud is MidiChordDef mcd)
                {
                    msDurationRangeMin = (msDurationRangeMin < mcd.MsDuration) ? msDurationRangeMin : mcd.MsDuration;
                    msDurationRangeMax = (msDurationRangeMax > mcd.MsDuration) ? msDurationRangeMax : mcd.MsDuration;
                }
            }
            #endregion find msDurationRangeMin and msDurationRangeMax

            foreach(IUniqueDef iud in _uniqueDefs)
            {
                if(iud is MidiChordDef mcd)
                {
                    mcd.SetVelocityFromDuration(msDurationRangeMin, msDurationRangeMax, velocityForMinMsDuration, velocityForMaxMsDuration, percent);
                }
            }
        }
        #endregion SetVelocitiesFromDurations
        #region SetVerticalVelocityGradient
        /// <summary>
        /// The arguments must both be in range [1..127].
        /// This function calls MidiChordDef.SetVerticalVelocityGradient(rootVelocity, topVelocity)
        /// on all the MidiChordDefs in the Trk. 
        /// </summary>
        public void SetVerticalVelocityGradient(byte rootVelocity, byte topVelocity)
        {
            #region conditions
            Debug.Assert(rootVelocity > 0 && rootVelocity <= 127);
            Debug.Assert(topVelocity > 0 && topVelocity <= 127);
            #endregion conditions

            foreach(IUniqueDef iud in UniqueDefs)
            {
                if(iud is MidiChordDef mcd)
                {
                    mcd.SetVerticalVelocityGradient(rootVelocity, topVelocity);
                }
            }
        }

        #endregion SetVerticalVelocityGradient
        #endregion Envelopes

        /// <summary>
        /// Preserves the MsDuration of the Trk as a whole by resetting it after doing the following:
        /// 1. Creates a sorted list of the unique bottom or top pitches in all the MidiChordDefs in the Trk.
        ///    The use of the bottom or top pitch is controlled by argument 3: useBottomPitch.
        /// 2. Creates a duration per pitch dictionary, whereby durationPerPitch[lowestPitch] is durationForLowestPitch
        ///    and durationPerPitch[lowestPitch] is durationForHighestPitch. The intermediate duration values are
        ///    interpolated logarithmically.
        /// 3. Sets the MsDuration of each MidiChordDef to (percent * the values found in the duration per pitch dictionary) plus
        ///   ((100-percent)percent * the original durations). Rest msDurations are left unchanged at this stage.
        /// 4. Resets the MsDuration of the Trk to its original value.
        /// N.B. a Debug.Assert() fails if an attempt is made to set the msDuration of a BasicMidiChordDef to zero.
        /// </summary>
        /// <param name="durationForLowestPitch"></param>
        /// <param name="durationForHighestPitch"></param>
        public void SetDurationsFromPitches(int durationForLowestPitch, int durationForHighestPitch, bool useBottomPitch, double percent = 100.0)
        {
            Debug.Assert(percent >= 0 && percent <= 100);

            List<byte> pitches = new List<byte>();
            #region get pitches
            foreach(IUniqueDef iud in _uniqueDefs)
            {
                if(iud is MidiChordDef mcd)
                {
                    byte pitch = (useBottomPitch == true) ? mcd.NotatedMidiPitches[0] : mcd.NotatedMidiPitches[mcd.NotatedMidiPitches.Count - 1];
                    if(!pitches.Contains(pitch))
                    {
                        pitches.Add(pitch);
                    }
                }
            }
            #endregion get pitches
            if(pitches.Count == 1)
            {
                int mcdMsDuration = (useBottomPitch == true) ? durationForLowestPitch : durationForHighestPitch;
                foreach(IUniqueDef iud in _uniqueDefs)
                {
                    if(iud is MidiChordDef mcd)
                    {
                        mcd.MsDuration = mcdMsDuration;
                    }
                }
            }
            else
            {
                Debug.Assert(pitches.Count > 1);
                pitches.Sort();
                #region get durations
                int nPitches = pitches.Count;
                double factor = Math.Pow((((double)durationForHighestPitch) / durationForLowestPitch), (((double)1) / (nPitches - 1)));
                List<int> msDurations = new List<int>();
                double msDuration = durationForLowestPitch;
                foreach(byte pitch in pitches)
                {
                    msDurations.Add((int)Math.Round(msDuration));
                    msDuration *= factor;
                }
                Debug.Assert(msDurations.Count == nPitches);
                Debug.Assert(msDurations[msDurations.Count - 1] == durationForHighestPitch);
                #endregion get durations
                #region get duration per pitch dictionary
                Dictionary<byte, int> msDurPerPitch = new Dictionary<byte, int>();
                for(int i = 0; i < nPitches; ++i)
                {
                    msDurPerPitch.Add(pitches[i], msDurations[i]);
                }
                #endregion get get duration per pitch dictionary
                #region set durations
                double factorForNewValue = percent / 100.0;
                double factorForOldValue = 1 - factorForNewValue;
                int currentMsDuration = this.MsDuration;
                foreach(IUniqueDef iud in _uniqueDefs)
                {
                    if(iud is MidiChordDef mcd)
                    {
                        byte pitch = (useBottomPitch == true) ? mcd.NotatedMidiPitches[0] : mcd.NotatedMidiPitches[mcd.NotatedMidiPitches.Count - 1];
                        int oldDuration = mcd.MsDuration;
                        int durPerPitch = msDurPerPitch[pitch];
                        mcd.MsDuration = (int)Math.Round((oldDuration * factorForOldValue) + (durPerPitch * factorForNewValue));
                    }
                }
                this.MsDuration = currentMsDuration;
                #endregion set durations
            }
        }

        /// <summary>
        /// Multiplies each expression value in the MidiChordDefs
        /// from beginIndex to (excluding) endIndex by the argument factor.
        /// </summary>
        public void AdjustExpression(int beginIndex, int endIndex, double factor)
        {
            if(CheckIndices(beginIndex, endIndex))
            {
                for(int i = beginIndex; i < endIndex; ++i)
                {
                    if(_uniqueDefs[i] is MidiChordDef iumdd)
                    {
                        iumdd.AdjustExpression(factor);
                    }
                }
            }
        }
        /// <summary>
        /// Multiplies each expression value in the UniqueDefs by the argument factor.
        /// </summary>
        public void AdjustExpression(double factor)
        {
            foreach(MidiChordDef mcd in MidiChordDefs)
            {
                mcd.AdjustExpression(factor);
            }
        }
        /// <summary>
        /// Multiplies each velocity value in the MidiChordDefs from beginIndex to (exclusive) endIndex by
        /// the argument factor (which must be greater than zero).
        /// The resulting velocities are in range 1..127.
        /// If a resulting velocity would have been less than 1, it is silently coerced to 1.
        /// If it would have been greater than 127, it is silently coerced to 127.
        /// </summary>
        public virtual void AdjustVelocities(int beginIndex, int nonInclusiveEndIndex, double factor)
        {
            if(CheckIndices(beginIndex, nonInclusiveEndIndex))
            {
                Debug.Assert(factor > 0.0);
                for(int i = beginIndex; i < nonInclusiveEndIndex; ++i)
                {
                    if(_uniqueDefs[i] is MidiChordDef mcd)
                    {
                        mcd.AdjustVelocities(factor);
                    }
                }
            }
        }

        /// <summary>
        /// Calls MidiChordDef.AdjustVelocities(byte originalVelocity, byte newVelocity) on each MidiChordDef
        /// in range beginIndex to (exclusive) endIndex.
        /// The resulting velocities are in range 1..127.
        /// Velocities having originalVelocity are changed to newVelocity.
        /// Velocity values above originalVelocity are changed proportionally with max possible velocity at 127.
        /// Velocity values below originalVelocity are changed proportionally with min possible velocity at 1.
        /// </summary>
        public void AdjustVelocities(int beginIndex, int nonInclusiveEndIndex, byte originalVelocity, byte newVelocity)
        {
            if(CheckIndices(beginIndex, nonInclusiveEndIndex))
            {
                for(int i = beginIndex; i < nonInclusiveEndIndex; ++i)
                {
                    if(_uniqueDefs[i] is MidiChordDef mcd)
                    {
                        mcd.AdjustVelocities(originalVelocity, newVelocity);
                    }
                }
            }
        }
        /// <summary>
        /// Multiplies each velocity value in the MidiChordDefs by the argument factor (must be greater than zero).
        /// N.B MidiChordDefs will be turned into RestDefs if all their notes have zero velocity!
        /// </summary>
        public virtual void AdjustVelocities(double factor)
        {
            Debug.Assert(factor > 0.0);
            for(int i = 0; i < UniqueDefs.Count; ++i)
            {
                if(UniqueDefs[i] is MidiChordDef mcd)
                {
                    mcd.AdjustVelocities(factor);
                    if(mcd.NotatedMidiPitches.Count == 0)
                    {
                        Replace(i, new MidiRestDef(mcd.MsPositionReFirstUD, mcd.MsDuration));
                    }
                }
            }
        }
        /// <summary>
        /// Creates a hairpin in the velocities from the IUniqueDef at beginIndex to the IUniqueDef at endIndex - 1 (inclusive).
        /// This function does NOT change velocities outside the range given in its arguments.
        /// There must be at least two IUniqueDefs in the msPosition range given in the arguments.
        /// The factors by which the velocities are multiplied change arithmetically:
        /// The velocity of the the IUniqueDef at beginIndex is multiplied by startFactor, and
        /// the velocity of the the IUniqueDef at endIndex is multiplied by endFactor.
        /// Can be used to create a diminueno or crescendo, or (using multiple calls) hairpins.
        /// N.B MidiChordDefs will be turned into RestDefs if all their notes have zero velocity!
        /// </summary>
        /// <param name="startIndex">index of start UniqueDef (range 0 to this.Count - 2)</param>
        /// <param name="endIndex">index of end UniqueDef (range startIndex + 1 to this.Count - 1)</param>
        /// <param name="startFactor">greater than or equal to 0</param>
        /// <param name="endFactor">greater than or equal to 0</param>
        public virtual void AdjustVelocitiesHairpin(int beginIndex, int endIndex, double startFactor, double endFactor)
        {
            if(CheckIndices(beginIndex, endIndex))
            {
                Debug.Assert(startFactor >= 0 && endFactor >= 0);
                int nNonMidiChordDefs = GetNumberOfNonMidiOrInputChordDefs(beginIndex, endIndex);
                int steps = endIndex - 1 - beginIndex - nNonMidiChordDefs;
                if(steps > 0)
                {
                    double factorIncrement = (endFactor - startFactor) / steps;
                    double factor = startFactor;
                    List<IUniqueDef> lmdds = _uniqueDefs;

                    for(int i = beginIndex; i < endIndex; ++i)
                    {
                        MidiChordDef mcd = _uniqueDefs[i] as MidiChordDef;
                        if(mcd != null)
                        {
                            mcd.AdjustVelocities(factor);
                            factor += factorIncrement;
                        }
                        if(mcd.NotatedMidiPitches.Count == 0)
                        {
                            Replace(i, new MidiRestDef(mcd.MsPositionReFirstUD, mcd.MsDuration));
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Creates a moving pan from startPanValue at beginIndex to endPanValue at (beginIndex + count - 1).
        /// (In other words, it sets count MidiChordDefs.)
        /// Implemented using one pan value per MidiChordDef.
        /// This function does NOT change pan values outside the position range given in its arguments.
        /// </summary>
        /// <param name="beginIndex">The index at which to start setting pan values</param>
        /// <param name="count">The number of IUniqueDefs to set (among these, only MidiChordDefs will be set)</param>
        /// <param name="startPanValue">The MSB of the initial pan value (in range 0..127)</param>
        /// <param name="endPanValue">The MSB of the final pan value (in range 0..127)</param>
        public void SetPanGliss(int beginIndex, int count, int startPanValue, int endPanValue)
        {
            int endIndex = beginIndex + count;
            if(CheckIndices(beginIndex, endIndex))
            {
                Debug.Assert(startPanValue >= 0 && startPanValue <= 127 && endPanValue >= 0 && endPanValue <= 127);

                int nNonMidiChordDefs = GetNumberOfNonMidiOrInputChordDefs(beginIndex, endIndex);
                int steps = (endIndex - 1 - beginIndex - nNonMidiChordDefs);
                if(steps > 0)
                {
                    double increment = ((double)(endPanValue - startPanValue)) / steps;
                    double panValue = startPanValue;
                    List<IUniqueDef> lmdds = _uniqueDefs;

                    for(int i = beginIndex; i < endIndex; ++i)
                    {
                        if(_uniqueDefs[i] is MidiChordDef iumdd)
                        {
                            byte panMsb = (byte)Math.Round(panValue);
                            iumdd.PanMsbs = new List<byte>() { panMsb };
                            panValue += increment;
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Sets the pitchwheelDeviation for MidiChordDefs in the defined range.
        /// Rests in the range dont change.
        /// </summary>
        /// <param name="beginIndex">The index at which to start setting pitchWheelDeviations</param>
        /// <param name="count">The number of IUniqueDefs to set (among these, only MidiChordDefs will be set)</param>
        public void SetPitchWheelDeviation(int beginIndex, int count, int deviation)
        {
            int endIndex = beginIndex + count;
            if(CheckIndices(beginIndex, endIndex))
            {
                for(int i = beginIndex; i < endIndex; ++i)
                {
                    if(this[i] is MidiChordDef mcd)
                    {
                        mcd.PitchWheelDeviation = M.MidiValue(deviation);
                    }
                }
            }
        }
        /// <summary>
        /// Removes the pitchwheel commands (not the pitchwheelDeviations)
        /// from chords in the range beginIndex to endIndex inclusive.
        /// Rests in the range are not changed.
        /// </summary>
        public void RemoveScorePitchWheelCommands(int beginIndex, int endIndex)
        {
            if(CheckIndices(beginIndex, endIndex))
            {
                for(int i = beginIndex; i < endIndex; ++i)
                {
                    if(this[i] is MidiChordDef umcd)
                    {
                        umcd.MidiChordSliderDefs.PitchWheelMsbs = new List<byte>();
                    }
                }
            }
        }

        /// <summary>
        /// Creates an exponential change (per index) of pitchwheelDeviation from beginIndex to endIndex,
        /// </summary>
        /// <param name="finale"></param>
        protected void AdjustPitchWheelDeviations(int beginIndex, int endIndex, int pwValueAtBeginIndex, int pwValueAtEndIndex)
        {
            Debug.Assert(beginIndex >= 0 && beginIndex < endIndex && endIndex < Count);
            Debug.Assert(pwValueAtBeginIndex >= 0 && pwValueAtEndIndex >= 0);
            Debug.Assert(pwValueAtBeginIndex <= 127 && pwValueAtEndIndex <= 127);

            int nNonMidiChordDefs = GetNumberOfNonMidiOrInputChordDefs(beginIndex, endIndex);

            double pwdfactor = Math.Pow(pwValueAtEndIndex / pwValueAtBeginIndex, (double)1 / (endIndex - beginIndex - nNonMidiChordDefs)); // f13.Count'th root of furies1EndPwdValue/furies1StartPwdValue -- the last pwd should be furies1EndPwdValue

            for(int i = beginIndex; i < endIndex; ++i)
            {
                if(_uniqueDefs[i] is MidiChordDef umc)
                {
                    umc.PitchWheelDeviation = M.MidiValue((int)(pwValueAtBeginIndex * (Math.Pow(pwdfactor, i))));
                }
            }
        }
        #endregion Changing MidiChordDef attributes

        #region alignment
        /// <summary>
        /// _uniqueDefs[indexToAlign] is moved to toMsPositionReFirstIUD, and the surrounding symbols are spread accordingly
        /// between those at anchor1Index and anchor2Index. The symbols at anchor1Index and anchor2Index do not move.
        /// Note that indexToAlign cannot be 0, and that anchor2Index CAN be equal to _uniqueDefs.Count (i.e.on the final barline).
        /// This function checks that 
        ///     1. anchor1Index is in range 0..(indexToAlign - 1),
        ///     2. anchor2Index is in range (indexToAlign + 1).._localizedMidiDurationDefs.Count
        ///     3. toPosition is greater than the msPosition at anchor1Index and less than the msPosition at anchor2Index.
        /// and throws an appropriate exception if there is a problem.
        /// </summary>
        public void AlignObjectAtIndex(int anchor1Index, int indexToAlign, int anchor2Index, int toMsPositionReFirstIUD)
        {
            // throws an exception if there's a problem.
            CheckAlignDefArgs(anchor1Index, indexToAlign, anchor2Index, toMsPositionReFirstIUD);

            List<IUniqueDef> lmdds = _uniqueDefs;
            int anchor1MsPositionReFirstIUD = lmdds[anchor1Index].MsPositionReFirstUD;
            int fromMsPositionReFirstIUD = lmdds[indexToAlign].MsPositionReFirstUD;
            int anchor2MsPositionReFirstIUD;
            if(anchor2Index == lmdds.Count) // i.e. anchor2 is on the final barline
            {
                anchor2MsPositionReFirstIUD = lmdds[anchor2Index - 1].MsPositionReFirstUD + lmdds[anchor2Index - 1].MsDuration;
            }
            else
            {
                anchor2MsPositionReFirstIUD = lmdds[anchor2Index].MsPositionReFirstUD;
            }

            float leftFactor = (float)(((float)(toMsPositionReFirstIUD - anchor1MsPositionReFirstIUD)) / ((float)(fromMsPositionReFirstIUD - anchor1MsPositionReFirstIUD)));
            for(int i = anchor1Index + 1; i < indexToAlign; ++i)
            {
                lmdds[i].MsPositionReFirstUD = anchor1MsPositionReFirstIUD + ((int)((lmdds[i].MsPositionReFirstUD - anchor1MsPositionReFirstIUD) * leftFactor));
            }

            lmdds[indexToAlign].MsPositionReFirstUD = toMsPositionReFirstIUD;

            float rightFactor = (float)(((float)(anchor2MsPositionReFirstIUD - toMsPositionReFirstIUD)) / ((float)(anchor2MsPositionReFirstIUD - fromMsPositionReFirstIUD)));
            for(int i = anchor2Index - 1; i > indexToAlign; --i)
            {
                lmdds[i].MsPositionReFirstUD = anchor2MsPositionReFirstIUD - ((int)((anchor2MsPositionReFirstIUD - lmdds[i].MsPositionReFirstUD) * rightFactor));
            }

            #region fix MsDurations
            for(int i = anchor1Index + 1; i <= anchor2Index; ++i)
            {
                if(i == lmdds.Count) // possible, when anchor2Index is the final barline
                {
                    lmdds[i - 1].MsDuration = anchor2MsPositionReFirstIUD - lmdds[i - 1].MsPositionReFirstUD;
                }
                else
                {
                    lmdds[i - 1].MsDuration = lmdds[i].MsPositionReFirstUD - lmdds[i - 1].MsPositionReFirstUD;
                }
            }
            #endregion

            AssertConsistency();
        }
        /// <summary>
        /// Debug.Assert fails if
        ///     1. the index arguments are not in ascending order or if any are equal.
        ///     2. any of the index arguments are out of range (anchor2Index CAN be _localizedMidiDurationDefs.Count, i.e. the final barline)
        ///     3. toPosition is not greater than the msPosition at anchor1Index and less than the msPosition at anchor2Index.
        /// </summary>
        private void CheckAlignDefArgs(int anchor1Index, int indexToAlign, int anchor2Index, int toMsPositionReFirstUD)
        {
            List<IUniqueDef> lmdds = _uniqueDefs;
            int count = lmdds.Count;
            string msg = "\nError in VoiceDef.cs,\nfunction AlignDefMsPosition()\n\n";
            Debug.Assert((anchor1Index < indexToAlign && anchor2Index > indexToAlign),
                    msg + "Index out of order.\n" +
                    "\nanchor1Index=" + anchor1Index.ToString() +
                    "\nindexToAlign=" + indexToAlign.ToString() +
                    "\nanchor2Index=" + anchor2Index.ToString());

            Debug.Assert(!(anchor1Index > (count - 2) || indexToAlign > (count - 1) || anchor2Index > count)// anchor2Index can be at the final barline (=count)!
                || (anchor1Index < 0 || indexToAlign < 1 || anchor2Index < 2),
                    msg + "Index out of range.\n" +
                    "\ncount=" + count.ToString() +
                    "\nanchor1Index=" + anchor1Index.ToString() +
                    "\nindexToAlign=" + indexToAlign.ToString() +
                    "\nanchor2Index=" + anchor2Index.ToString());

            int a1MsPosReFirstUD = lmdds[anchor1Index].MsPositionReFirstUD;
            int a2MsPosReFirstIUD;
            if(anchor2Index == lmdds.Count)
            {
                a2MsPosReFirstIUD = lmdds[anchor2Index - 1].MsPositionReFirstUD + lmdds[anchor2Index - 1].MsDuration;
            }
            else
            {
                a2MsPosReFirstIUD = lmdds[anchor2Index].MsPositionReFirstUD;
            }
            Debug.Assert((toMsPositionReFirstUD > a1MsPosReFirstUD && toMsPositionReFirstUD < a2MsPosReFirstIUD),
                    msg + "Target (msPos) position out of range.\n" +
                    "\nanchor1Index=" + anchor1Index.ToString() +
                    "\nindexToAlign=" + indexToAlign.ToString() +
                    "\nanchor2Index=" + anchor2Index.ToString() +
                    "\ntoMsPosition=" + toMsPositionReFirstUD.ToString());
        }

        /// <summary>
        /// _uniqueDefs[indexToAlign] is moved to msPositionReContainer.
        /// This whole Trk is shifted horizontally by setting its MsPositionReContainer.
        /// </summary>
        public void AlignObjectAtIndex(int indexToAlign, int msPositionReContainer)
        {
            Debug.Assert(indexToAlign >= 0 && indexToAlign < this.Count);
            int currentMsPositionReContainer = this.MsPositionReContainer + this[indexToAlign].MsPositionReFirstUD;
            int shift = msPositionReContainer - currentMsPositionReContainer;
            this.MsPositionReContainer += shift;
        }
        #endregion alignment

        #region Re-ordering the Trk's UniqueDefs

        #region Permute()
        /// <summary>
        /// This function permutes any number of UniqueDefs in the trk's UniqueDefs list according to the contour retrieved (from
        /// the static K.Contour[] array) using the axisNumber and contourNumber arguments. The trk's AxisIndex property is set.
        /// If there are more than 7 UniqueDefs in the list, 7 partitions are automatically created and permuted recursively
        /// using the same contour.
        /// </summary>
        /// <param name="axisNumber">A value in range 1..12 inclusive.</param>
        /// <param name="contourNumber">A value in range 1..12 inclusive.</param>
        public virtual void Permute(int axisNumber, int contourNumber)
        {
            Debug.Assert(!(contourNumber < 1 || contourNumber > 12), "contourNumber out of range 1..12");
            Debug.Assert(!(axisNumber < 1 || axisNumber > 12), "axisNumber out of range 1..12");

            PermuteRecursively(axisNumber, contourNumber, _uniqueDefs);
        }

        /// <summary>
        /// Re-orders the UniqueDefs in (part of) this Trk.
        /// <para>1. Creates a list of not more than 7 partitions that are as equal in length as possible.</para>   
        /// <para>2. Re-orders the partitions according to the contour retrieved (from the static K.Contour[] array) using</para>
        /// <para>-  the axisNumber and contourNumber arguments.</para>
        /// <para>3  Re-orders the UniqueDefs in each partition whose count is greater than 1 by calling itself recursively.</para>
        /// <para>4. Resets the UniqueDefs list to the concatenation of the partitions (that have been re-ordered internally and externally).</para>
        /// </summary>.
        /// <param name="uniqueDefs">Call this function with the trk's UniqueDefs list.</param>
        /// <param name="axisNumber">A value greater than or equal to 1, and less than or equal to 12 An exception is thrown if this is not the case.</param>
        /// <param name="contourNumber">A value greater than or equal to 1, and less than or equal to 12. An exception is thrown if this is not the case.</param>
        private void PermuteRecursively(int axisNumber, int contourNumber, List<IUniqueDef> uniqueDefs)
        {
            List<List<IUniqueDef>> partitions = GetPartitionsOfEqualLength(uniqueDefs);

            Debug.Assert(partitions.Count > 0 && partitions.Count <= 7);

            IUniqueDef axisUniqueDef = uniqueDefs[0];

            if(partitions.Count > 1)
            {
                // re-order the partitions
                partitions = DoContouring(axisNumber, contourNumber, partitions);
                int axisPartitionIndex = GetPartitionIndex(partitions.Count, axisNumber);
                List<IUniqueDef> axisPartition = partitions[axisPartitionIndex];
                axisUniqueDef = axisPartition[0];
                foreach(List<IUniqueDef> partition in partitions)
                {
                    if(partition.Count > 1)
                    {
                        PermuteRecursively(axisNumber, contourNumber, partition); // recursive call
                    }
                }
            }

            List<IUniqueDef> sortedLmdds = ConvertPartitionsToFlatIUDs(partitions);

            for(int i = 0; i < sortedLmdds.Count; ++i)
            {
                uniqueDefs[i] = sortedLmdds[i];
            }

            // only do the following at the top level of the recursion
            if(uniqueDefs == _uniqueDefs)
            {
                AxisIndex = _uniqueDefs.FindIndex(u => (u == axisUniqueDef));
                AssertConsistency();
            }
        }
        /// <summary>
        /// Returns a list of partitions (each of which is a list of IUniqueDefs)
        /// The returned list:
        ///     * is as long as possible, but contains not more than 7 partitions.
        ///     * contains partitions whose Count is distributed as evenly as possible along the list. 
        /// </summary>
        private List<List<IUniqueDef>> GetPartitionsOfEqualLength(List<IUniqueDef> uniqueDefs)
        {
            Debug.Assert(uniqueDefs.Count > 0);

            List<int> partitionSizes = GetEqualPartitionSizes(uniqueDefs.Count);
            List<List<IUniqueDef>> partitions = new List<List<IUniqueDef>>();
            int lmddIndex = 0;
            foreach(int size in partitionSizes)
            {
                List<IUniqueDef> partition = new List<IUniqueDef>();
                for(int i = 0; i < size; ++i)
                {
                    partition.Add(uniqueDefs[lmddIndex++]);
                }
                partitions.Add(partition);
            }
            return partitions;
        }
        /// <summary>
        /// Returns a list that adds up to count.
        /// The returned list:
        ///     * is as long as possible, but contains not more than 7 ints.
        ///     * contains values that are distributed evenly along the list. 
        /// </summary>
        /// <returns></returns>
        private List<int> GetEqualPartitionSizes(int count)
        {
            List<int> partitionSizes = new List<int>();
            if(count > 7)
            {
                partitionSizes = M.IntDivisionSizes(count, 7);
            }
            else
            {
                for(int i = 0; i < count; ++i)
                {
                    partitionSizes.Add(1);
                }
            }
            return partitionSizes;
        }
        #endregion Permute()

        #region PermutePartitions()
        /// <summary>
        /// Re-orders up to 7 partitions in this Trk's UniqueDefs list. The content of each partition is not changed. The Trk's AxisIndex property is set.
        /// <para>1. Creates partitions (lists of UniqueDefs) using the partitionSizes in the third argument.</para>  
        /// <para>2. Re-orders the partitions according to the contour retrieved (from the static K.Contour[] array) using the axisNumber and contourNumber arguments.</para>
        /// <para>3. Resets the UniqueDefs list to the concatenation of the re-ordered partitions.</para>
        /// </summary>
        /// <param name="axisNumber">A value greater than or equal to 1, and less than or equal to 12 An exception is thrown if this is not the case.</param>
        /// <param name="contourNumber">A value greater than or equal to 1, and less than or equal to 12. An exception is thrown if this is not the case.</param>
        /// <param name="partitionSizes">The number of UniqueDefs in each partition to be re-ordered.
        /// <para>This partitionSizes list must contain 1..7 partition sizes. The sizes must all be greater than 0. The sum of all the sizes must be equal
        /// to UniqueDefs.Count.</para>
        /// <para>An Exception is thrown if any of these conditions is not met.</para>
        /// <para>If the partitions list contains only one value, this function returns silently without doing anything.</para></param>
        public virtual void PermutePartitions(int axisNumber, int contourNumber, List<int> partitionSizes)
        {
            CheckPermutePartitionsArgs(axisNumber, contourNumber, partitionSizes);

            IUniqueDef axisUniqueDef = UniqueDefs[0];

            List<List<IUniqueDef>> partitions = GetPartitionsFromPartitionSizes(partitionSizes);

            if(partitions.Count > 1)
            {
                // re-order the partitions
                partitions = DoContouring(axisNumber, contourNumber, partitions);
                int axisPartitionIndex = GetPartitionIndex(partitions.Count, axisNumber);
                List<IUniqueDef> axisPartition = partitions[axisPartitionIndex];
                axisUniqueDef = axisPartition[0];
            }

            List<IUniqueDef> sortedLmdds = ConvertPartitionsToFlatIUDs(partitions);

            for(int i = 0; i < sortedLmdds.Count; ++i)
            {
                _uniqueDefs[i] = sortedLmdds[i];
            }

            AxisIndex = _uniqueDefs.FindIndex(u => (u == axisUniqueDef));

            AssertConsistency();
        }

        /// <summary>
        /// Debug.Assert fails if one of the following conditions is not met.
        /// <para>axisNumber must be in the range 1..12</para>
        /// <para>contourNumber must be in the range 1..12</para>
        /// <para>partitionSizes.Count must be greater than 0, and less than 8.</para>
        /// <para>each partitionSize must be greater then 0.</para>
        /// <para>the sum of all the partition sizes must be equal to UniqueDefs.Count</para>
        /// </summary>
        private void CheckPermutePartitionsArgs(int axisNumber, int contourNumber, List<int> partitionSizes)
        {
            Debug.Assert(!(axisNumber < 1 || axisNumber > 12), "axisNumber out of range 1..12");
            Debug.Assert(!(contourNumber < 1 || contourNumber > 12), "contourNumber out of range 1..12");

            Debug.Assert(!(partitionSizes.Count < 1 || partitionSizes.Count > 7), "partitionSizes.Count must be in range 1..7");

            int totalPartitionSizes = 0;
            foreach(int size in partitionSizes)
            {
                Debug.Assert(size >= 1, "each partition must contain at least one IUniqueDef");
                totalPartitionSizes += size;
            }
            Debug.Assert((totalPartitionSizes == _uniqueDefs.Count), "Sum of partition sizes does not match number of UniqueDefs.");
        }
        private List<List<IUniqueDef>> GetPartitionsFromPartitionSizes(List<int> partitionSizes)
        {
            List<List<IUniqueDef>> partitions = new List<List<IUniqueDef>>();
            int lmddIndex = 0;
            foreach(int size in partitionSizes)
            {
                List<IUniqueDef> partition = new List<IUniqueDef>();
                for(int i = 0; i < size; ++i)
                {
                    partition.Add(_uniqueDefs[lmddIndex++]);
                }
                partitions.Add(partition);
            }
            return partitions;
        }
        #endregion PermutePartitions

        #region common to Permute functions
        /// <summary>
        /// Re-orders the partitions according to the contour retrieved (from the static K.Contour[] array)
        /// <para>using partitions.Count and the contourNumber and axisNumber arguments.</para>
        /// <para>Does not change the inner contents of the partitions themselves.</para>
        /// </summary>
        /// <returns>A re-ordered list of partitions</returns>
        private List<List<IUniqueDef>> DoContouring(int axisNumber, int contourNumber, List<List<IUniqueDef>> partitions)
        {
            List<List<IUniqueDef>> contouredPartitions = new List<List<IUniqueDef>>();
            int[] contour = K.Contour(partitions.Count, contourNumber, axisNumber);
            foreach(int number in contour)
            {
                // K.Contour() always returns an array containing 7 values.
                // For densities less than 7, the final values are 0.
                if(number == 0)
                    break;
                contouredPartitions.Add(partitions[number - 1]);
            }

            return contouredPartitions;
        }
        /// <summary>
        /// The index of the value before the axis in the standard contour diagrams.
        /// (i.e. the position of the axis in the diagrams, minus 2)
        /// </summary>
        /// <param name="domain">In range [1..7]</param>
        /// <param name="axisNumber">In range [1..12]</param>
        private int GetPartitionIndex(int domain, int axisNumber)
        {
            Debug.Assert(domain > 0 && domain <= 7);
            Debug.Assert(axisNumber > 0 && axisNumber <= 12);

            return axisIndices[domain - 1, axisNumber - 1];
        }
        private static readonly int[,] axisIndices =
        {
            {0,0,0,0,0,0,0,0,0,0,0,0}, // domain 1
            {0,0,0,0,0,0,0,0,0,0,0,0}, // domain 2
            {0,0,0,0,0,0,1,1,1,1,1,1}, // domain 3
            {0,0,0,0,1,1,2,2,2,2,1,1}, // domain 4
            {0,0,0,1,1,2,3,3,3,2,2,1}, // domain 5
            {0,0,0,1,2,3,4,4,4,3,2,1}, // domain 6
            {0,0,1,2,3,4,5,5,4,3,2,1}  // domain 7
        };

        private List<IUniqueDef> ConvertPartitionsToFlatIUDs(List<List<IUniqueDef>> partitions)
        {
            List<IUniqueDef> newIUDs = new List<IUniqueDef>();
            int msPositionReFirstIUD = 0;
            foreach(List<IUniqueDef> partition in partitions)
            {
                foreach(IUniqueDef pLmdd in partition)
                {
                    pLmdd.MsPositionReFirstUD = msPositionReFirstIUD;
                    msPositionReFirstIUD += pLmdd.MsDuration;
                    newIUDs.Add(pLmdd);
                }
            }
            return newIUDs;
        }
        #endregion common to Permute functions

        #region Sort functions

        #region SortByRootNotatedPitch

        public virtual void SortRootNotatedPitchAscending()
        {
            SortByRootNotatedPitch(true);
        }

        public virtual void SortRootNotatedPitchDescending()
        {
            SortByRootNotatedPitch(false);
        }

        /// <summary>
        /// Re-orders the UniqueDefs in order of increasing root notated pitch.
        /// 1. The positions of any Rests are saved.
        /// 2. Each MidiChordDef is associated with a List of its velocities sorted into descending order.
        /// 3. The velocity lists are sorted into ascending order (shorter lists come first, as in sorting words)
        /// 4. The MidiChordDefs are sorted similarly.
        /// 5. Rests are re-inserted at their original positions.
        /// </summary>
        protected void SortByRootNotatedPitch(bool ascending)
        {
            Debug.Assert(!(Container is Bar), "Cannot sort inside a Bar.");

            List<IUniqueDef> localIUDs = new List<IUniqueDef>(UniqueDefs);
            // Remove any rests from localIUDs, and store them (the rests), with their original indices,
            // in the returned list of KeyValuePairs.
            List<KeyValuePair<int, IUniqueDef>> rests = ExtractRests(localIUDs);

            List<ulong> lowestPitches = GetLowestNotatedPitches(localIUDs);
            List<int> sortedOrder = GetSortedOrder(lowestPitches);
            if(ascending == false)
            {
                sortedOrder.Reverse();
            }

            FinalizeSort(localIUDs, sortedOrder, rests);
        }

        /// <summary>
        /// Returns a list containing the lowest pitch in each MidiChordDef in order
        /// </summary>
        private List<ulong> GetLowestNotatedPitches(List<IUniqueDef> localIUDs)
        {
            List<ulong> lowestPitches = new List<ulong>();
            foreach(IUniqueDef iud in localIUDs)
            {
                MidiChordDef mcd = iud as MidiChordDef;
                Debug.Assert(mcd != null);
                lowestPitches.Add(mcd.NotatedMidiPitches[0]);
            }
            return lowestPitches;
        }

        #endregion SortByRootNotatedPitch

        #region SortByVelocity

        public virtual void SortVelocityIncreasing()
        {
            SortByVelocity(true);
        }

        public virtual void SortVelocityDecreasing()
        {
            SortByVelocity(false);
        }

        /// <summary>
        /// Re-orders the UniqueDefs in order of increasing velocity.
        /// 1. The positions of any Rests are saved.
        /// 2. Each MidiChordDef is associated with a List of its velocities sorted into descending order.
        /// 3. The velocity lists are sorted into ascending order (shorter lists come first, as in sorting words)
        /// 4. The MidiChordDefs are sorted similarly.
        /// 5. Rests are re-inserted at their original positions.
        /// </summary>
        protected void SortByVelocity(bool increasing)
        {
            Debug.Assert(!(Container is Bar), "Cannot sort inside a Bar.");

            List<IUniqueDef> localIUDs = new List<IUniqueDef>(UniqueDefs);
            // Remove any rests from localIUDs, and store them (the rests), with their original indices,
            // in the returned list of KeyValuePairs.
            List<KeyValuePair<int, IUniqueDef>> rests = ExtractRests(localIUDs);

            List<List<byte>> mcdVelocities = GetSortedMidiChordDefVelocities(localIUDs);
            List<ulong> values = GetValuesFromVelocityLists(mcdVelocities);
            List<int> sortedOrder = GetSortedOrder(values);
            if(increasing == false)
            {
                sortedOrder.Reverse();
            }
            FinalizeSort(localIUDs, sortedOrder, rests);
        }

        /// <summary>
        /// Each list of bytes is converted to a ulong value representing its sort order
        /// </summary>
        /// <returns></returns>
        private List<ulong> GetValuesFromVelocityLists(List<List<byte>> mcdVelocities)
        {
            int maxCount = 0;
            foreach(List<byte> bytes in mcdVelocities)
            {
                maxCount = (bytes.Count > maxCount) ? bytes.Count : maxCount;
            }
            List<ulong> values = new List<ulong>();
            foreach(List<byte> bytes in mcdVelocities)
            {
                ulong val = 0;
                double factor = Math.Pow((double)128, (double)(maxCount - 1));
                foreach(byte b in bytes)
                {
                    val += (ulong)(b * factor);
                    factor /= 128;
                }
                if(bytes.Count < maxCount)
                {
                    Debug.Assert(factor > 1);
                    int remainingCount = maxCount - bytes.Count;
                    for(int i = 0; i < remainingCount; ++i)
                    {
                        val += (ulong)(128 * factor);
                        factor /= 128;
                    }
                }
                Debug.Assert(factor == (double)1 / 128);
                values.Add(val);
            }
            return values;
        }

        /// <summary>
        /// Returns a list of the velocities used by each MidiChordDef.
        /// Each list of velocities is sorted into descending order.
        /// </summary>
        /// <param name="mcds"></param>
        /// <returns></returns>
        private List<List<byte>> GetSortedMidiChordDefVelocities(List<IUniqueDef> mcds)
        {
            #region conditions
            foreach(IUniqueDef iud in mcds)
            {
                Debug.Assert(iud is MidiChordDef);
            }
            #endregion conditions

            List<List<byte>> sortedVelocities = new List<List<byte>>();
            foreach(IUniqueDef iud in mcds)
            {
                MidiChordDef mcd = iud as MidiChordDef;
                List<byte> velocities = new List<byte>(mcd.NotatedMidiVelocities);
                velocities.Sort();
                velocities.Reverse();
                sortedVelocities.Add(velocities);
            }
            return sortedVelocities;
        }

        #endregion SortByVelocity

        #region common to sort functions
        /// <summary>
        /// Remove any rests from localIUDs, and store them (the rests), with their original indices,
        /// in the returned list of KeyValuePairs.
        /// </summary>
        private List<KeyValuePair<int, IUniqueDef>> ExtractRests(List<IUniqueDef> iuds)
        {
            List<KeyValuePair<int, IUniqueDef>> rests = new List<KeyValuePair<int, IUniqueDef>>();
            for(int i = iuds.Count - 1; i >= 0; --i)
            {
                if(iuds[i] is MidiRestDef)
                {
                    rests.Add(new KeyValuePair<int, IUniqueDef>(i, iuds[i]));
                    iuds.RemoveAt(i);
                }
            }
            return rests;
        }

        /// <summary>
        /// The values are in unsorted order. To sort the values into ascending order, copy them
        /// to a new list in the order of the indices in the list returned from this function.
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        private static List<int> GetSortedOrder(List<ulong> values)
        {
            List<int> sortedOrder = new List<int>();
            for(int i = 0; i < values.Count; ++i)
            {
                ulong minVal = ulong.MaxValue;
                int index = -1;
                for(int j = 0; j < values.Count; ++j)
                {
                    if(values[j] < minVal)
                    {
                        minVal = values[j];
                        index = j;
                    }
                }
                sortedOrder.Add(index);
                values[index] = ulong.MaxValue;
            }

            return sortedOrder;
        }

        /// <summary>
        /// Create the sorted (rest-less) list, reinsert any rests, and reset the Trk's UniqueDefs to the result.
        /// </summary>
        /// <param name="localIUDs">A copy of the original UniqueDefs, from which the rests have been removed.</param>
        /// <param name="sortedOrder">The order in which to sort localIUDs</param>
        /// <param name="rests">The removed rests, with their original indices.</param>
        private void FinalizeSort(List<IUniqueDef> localIUDs, List<int> sortedOrder, List<KeyValuePair<int, IUniqueDef>> rests)
        {
            List<IUniqueDef> finalList = new List<IUniqueDef>();
            for(int i = 0; i < localIUDs.Count; ++i)
            {
                finalList.Add(localIUDs[sortedOrder[i]]);
            }
            foreach(KeyValuePair<int, IUniqueDef> rest in rests)
            {
                finalList.Insert(rest.Key, rest.Value);
            }

            for(int i = UniqueDefs.Count - 1; i >= 0; --i)
            {
                RemoveAt(i);
            }
            foreach(IUniqueDef iud in finalList)
            {
                _Add(iud);
            }
        }
        #endregion common to sort functions

        #endregion Sort functions

        #endregion Re-ordering the Trk's UniqueDefs

        #region Enumerators
        protected IEnumerable<MidiChordDef> MidiChordDefs
        {
            get
            {
                foreach(IUniqueDef iud in _uniqueDefs)
                {
                    if(iud is MidiChordDef midiChordDef)
                        yield return midiChordDef;
                }
            }
        }
        #endregion

        public override string ToString()
        {
            return ($"Trk: MsDuration={MsDuration} MsPositionReContainer={MsPositionReContainer} Count={Count}");
        }

        /// <summary>
        /// This value is used by Seq.AlignTrkAxes(). It is set by the Permute functions, but can also be set manually.
        /// It is the index of the UniqueDef (in the UniqueDefs list) that will be aligned when calling Seq.AlignTrkAxes().
        /// </summary>
        public int AxisIndex
        {
            get
            {
                return _axisIndex;
            }
            set
            {
                Debug.Assert(_axisIndex < UniqueDefs.Count);
                _axisIndex = value;
            }
        }
        private int _axisIndex = 0;

        /// <summary>
        /// The number of MidiChordDefs and RestDefs in this Trk
        /// </summary>
        public int DurationsCount
        {
            get
            {
                int count = 0;
                foreach(IUniqueDef iud in _uniqueDefs)
                {
                    if(iud is MidiChordDef || iud is MidiRestDef)
                    {
                        count++;
                    }
                }
                return count;
            }
        }
    }
}
