
using System;
using System.Diagnostics;
using System.Collections.Generic;

using Krystals4ObjectLibrary;
using Moritz.Globals;

namespace Moritz.Spec
{
    /// <summary>
    /// In Seqs, a temporal sequence of MidiChordDef and RestDef objects (condition Asserted in Seq).
    /// In Blocks, Trks may also contain CautionaryChordDef objects (condition Asserted in Block);
    /// <para>All VoiceDef objects are IEnumerable, so that foreach loops can be used.</para>
    /// <para>For example:</para>
    /// <para>foreach(IUniqueDef iumdd in trk) { ... }</para>
    /// <para>An Enumerator for MidiChordDefs is also defined so that</para>
    /// <para>foreach(MidiChordDef mcd in trkDef.MidiChordDefs) { ... }</para>
    /// <para>can also be used.</para>
    /// <para>This class is also indexable, as in:</para>
    /// <para>IUniqueDef iu = trk[index];</para>
    /// </summary>
    public class Trk : VoiceDef
    {
        #region constructors
        public Trk(int midiChannel, int msPositionReContainer, List<IUniqueDef> iuds)
            : base(midiChannel, msPositionReContainer, iuds)
        {
            // N.B. msPositionReContainer can be negative here. Seqs are normalised independently.
            AssertConstructionConsistency();
        }

        /// <summary>
        /// A Trk with msPositionReContainer=0 and an empty UniqueDefs list.
        /// This constructor is used by Block.PopBar(...).
        /// </summary>
        public Trk(int midiChannel)
            : base(midiChannel, 0, new List<IUniqueDef>())
        {
        }

        /// <summary>
        /// Returns a deep clone of this Trk.
        /// </summary>
        public Trk Clone()
        {
            List<IUniqueDef> clonedIUDs = GetUniqueDefsClone();
            Trk trk = new Trk(MidiChannel, MsPositionReContainer, clonedIUDs);
            trk.Container = this.Container;

            return trk; 
        }

        /// <summary>
        /// Also used by Clone() functions in subclasses
        /// </summary>
        public List<IUniqueDef> GetUniqueDefsClone()
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

        internal void AssertConstructionConsistency()
        {
            foreach(IUniqueDef iud in UniqueDefs)
            {
                // In blocks, trks can also contain CautionaryChordDefs
                Debug.Assert(iud is MidiChordDef || iud is RestDef || iud is ClefChangeDef);
            }
        }

        internal override void AssertConsistentInBlock()
        {
            foreach(IUniqueDef iud in UniqueDefs)
            {
                // In blocks, trks can also contain ClefChangeDef and CautionaryChordDefs
                Debug.Assert(iud is MidiChordDef || iud is RestDef || iud is ClefChangeDef || iud is CautionaryChordDef);
            }
        }

        #region Add, Remove, Insert, Replace objects in the Trk
        /// <summary>
        /// Appends the new MidiChordDef, RestDef, CautionaryChordDef or ClefChangeDef to the end of the list.
        /// Automatically sets the iUniqueDef's msPosition.
        /// Used by Block.PopBar(...), so accepts a CautionaryChordDef argument.
        /// </summary>
        public override void Add(IUniqueDef iUniqueDef)
        {
            Debug.Assert(iUniqueDef is MidiChordDef || iUniqueDef is RestDef || iUniqueDef is CautionaryChordDef || iUniqueDef is ClefChangeDef);
            _Add(iUniqueDef);
        }
        /// <summary>
        /// Adds the argument's UniqueDefs to the end of this Trk.
        /// Sets the MsPositions of the appended UniqueDefs.
        /// </summary>
        public override void AddRange(VoiceDef trk)
        {
            Debug.Assert(trk is Trk);
            _AddRange(trk);
        }
        /// <summary>
        /// Inserts the iUniqueDef in the list at the given index, and then
        /// resets the positions of all the uniqueDefs in the list.
        /// </summary>
        public override void Insert(int index, IUniqueDef iUniqueDef)
        {
            Debug.Assert(iUniqueDef is MidiChordDef || iUniqueDef is RestDef || iUniqueDef is ClefChangeDef);
            _Insert(index, iUniqueDef);
        }
        /// <summary>
        /// Inserts the trk's UniqueDefs in the list at the given index, and then
        /// resets the positions of all the uniqueDefs in the list.
        /// </summary>
        public void InsertRange(int index, Trk trk)
        {
            _InsertRange(index, trk);
        }
        /// <summary>
        /// An attempt is made to insert the argument iVoiceDef in a rest in the VoiceDef.
        /// The rest is found using the iVoiceDef's MsPositon and MsDuration.
        /// The first and last objects in the argument must be chords, not rests.
        /// The argument may contain just one chord.
        /// The inserted iVoiceDef may end up at the beginning, middle or end of the spanning rest (which will
        /// be split as necessary).
        /// If no rest is found spanning the iVoiceDef, the attempt fails and an exception is thrown.
        /// This function does not change the msPositions of any other chords or rests in the containing VoiceDef,
        /// It does, of course, change the indices of the inserted lmdds and the later chords and rests.
        /// </summary>
        public void InsertInRest(Trk trk)
        {
            Debug.Assert(trk[0] is MidiChordDef && trk[trk.Count - 1] is MidiChordDef);
            _InsertInRest(trk);
        }
        /// <summary>
        /// Creates a new TrkDef containing just the argument midiChordDef,
        /// then calls the other InsertInRest() function with the voiceDef as argument. 
        /// </summary>
        public void InsertInRest(MidiChordDef midiChordDef)
        {
            List<IUniqueDef> iuds = new List<IUniqueDef>() { midiChordDef };
            Trk trkDefToInsert = new Trk(this.MidiChannel, 0, iuds);
            InsertInRest(trkDefToInsert);
        }
        /// <summary>
        /// Removes the iUniqueDef at index from the list, and then inserts the replacement at the same index.
        /// </summary>
        public void Replace(int index, IUniqueDef replacementIUnique)
        {
            Debug.Assert(replacementIUnique is MidiChordDef || replacementIUnique is RestDef);
            _Replace(index, replacementIUnique);
        }
        #endregion Add, Remove, Insert, Replace objects in the Trk

        #region Changing the Trk's duration
        /// <summary>
        /// Multiplies the MsDuration of each midiChordDef from beginIndex to (not including) endIndex by factor.
        /// If a midiChordDef's MsDuration becomes less than minThreshold, it is removed.
        /// The total duration of this VoiceDef changes accordingly.
        /// </summary>
        public void AdjustChordMsDurations(int beginIndex, int endIndex, double factor, int minThreshold = 100)
        {
            AdjustMsDurations<MidiChordDef>(beginIndex, endIndex, factor, minThreshold);
        }
        /// <summary>
        /// Multiplies the MsDuration of each midiChordDef in the UniqueDefs list by factor.
        /// If a midiChordDef's MsDuration becomes less than minThreshold, it is removed.
        /// The total duration of this TrkDef changes accordingly.
        /// </summary>
        public void AdjustChordMsDurations(double factor, int minThreshold = 100)
        {
            AdjustMsDurations<MidiChordDef>(0, _uniqueDefs.Count, factor, minThreshold);
        }
        #endregion Changing the Trk's duration

        #region Changing MidiChordDef attributes

        public void TransposeInGamut(int stepsToTranspose)
        {
            foreach(MidiChordDef mcd in MidiChordDefs)
            {
                if(mcd.Gamut != null)
                {
                    mcd.TransposeInGamut(stepsToTranspose);
                }
            }
        }

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
        /// Also used by Trks in Seq and Block
        /// </summary>
        public void SetPitchWheelSliders(Dictionary<int, int> pitchWheelValuesPerMsPosition)
        {
            foreach(IUniqueDef iud in UniqueDefs)
            {
                MidiChordDef mcd = iud as MidiChordDef;
                if(mcd != null)
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
        /// The argument contains a list of 12 velocity values (range [0..127] in order of absolute pitch.
        /// For example: If the MidiChordDef contains one or more C#s, they will be given velocity velocityPerAbsolutePitch[1].
        /// Middle-C is midi pitch 60 (60 % 12 == absolute pitch 0), middle-C# is midi pitch 61 (61 % 12 == absolute pitch 1), etc.
        /// This function applies equally to all the BasicMidiChordDefs in this MidiChordDef. 
        /// </summary>
        /// <param name="velocityPerAbsolutePitch">A list of 12 velocity values (range [0..127] in order of absolute pitch</param>
        public void SetVelocityPerAbsolutePitch(List<byte> velocityPerAbsolutePitch)
        {
            #region conditions
            Debug.Assert(velocityPerAbsolutePitch.Count == 12);
            for(int i = 0; i < 12; ++i)
            {
                int v = velocityPerAbsolutePitch[i];
                Debug.Assert(v >= 0 && v <= 127);
            }
            #endregion conditions
            foreach(IUniqueDef iud in UniqueDefs)
            {
                MidiChordDef mcd = iud as MidiChordDef;
                if(mcd != null)
                {
                    mcd.SetVelocityPerAbsolutePitch(velocityPerAbsolutePitch);
                }
            }
        }
        #endregion SetVelocityPerAbsolutePitch
        #region SetVelocitiesFromDurations
        /// <summary>
        /// Sets the velocity of each MidiChordDef in the Trk (anti-)proportionally to its duration.
        /// N.B 1) Neither velocityForMinMsDuration nor velocityForMaxMsDuration can be zero! -- that would be a NoteOff.
        /// and 2) velocityForMinMsDuration can be less than, equal to, or greater than velocityForMaxMsDuration
        /// </summary>
        /// <param name="velocityForMinMsDuration">in range 1..127</param>
        /// <param name="velocityForMaxMsDuration">in range 1..127</param>
        public void SetVelocitiesFromDurations(byte velocityForMinMsDuration, byte velocityForMaxMsDuration)
        {
            Debug.Assert(velocityForMinMsDuration >= 1 && velocityForMinMsDuration <= 127);
            Debug.Assert(velocityForMaxMsDuration >= 1 && velocityForMaxMsDuration <= 127);

            int msDurationRangeMin = int.MaxValue;
            int msDurationRangeMax = int.MinValue;

            #region find msDurationRangeMin and msDurationRangeMax 
            foreach(IUniqueDef iud in _uniqueDefs)
            {
                MidiChordDef mcd = iud as MidiChordDef;
                if(mcd != null)
                {
                    msDurationRangeMin = (msDurationRangeMin < mcd.MsDuration) ? msDurationRangeMin : mcd.MsDuration;
                    msDurationRangeMax = (msDurationRangeMax > mcd.MsDuration) ? msDurationRangeMax : mcd.MsDuration;
                }
            }
            #endregion find msDurationRangeMin and msDurationRangeMax

            foreach(IUniqueDef iud in _uniqueDefs)
            {
                MidiChordDef mcd = iud as MidiChordDef;
                if(mcd != null)
                {
                    mcd.SetVelocityFromDuration(msDurationRangeMin, msDurationRangeMax, velocityForMinMsDuration, velocityForMaxMsDuration);
                }
            }
        }
        #endregion SetVelocitiesFromDurations
        #region SetVerticalVelocityGradient
        /// <summary>
        /// The arguments are both in range [1..127].
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
                MidiChordDef mcd = iud as MidiChordDef;
                if(mcd != null)
                {
                    mcd.SetVerticalVelocityGradient(rootVelocity, topVelocity);
                }
            }
        }

        #endregion SetVerticalVelocityGradient
        #endregion Envelopes

        /// <summary>
        /// Multiplies each expression value in the MidiChordDefs
        /// from beginIndex to (not including) endIndex by the argument factor.
        /// </summary>
        public void AdjustExpression(int beginIndex, int endIndex, double factor)
        {
            CheckIndices(beginIndex, endIndex);

            for(int i = beginIndex; i < endIndex; ++i)
            {
                MidiChordDef iumdd = _uniqueDefs[i] as MidiChordDef;
                if(iumdd != null)
                {
                    iumdd.AdjustExpression(factor);
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
        /// Multiplies each velocity value in the MidiChordDefs
        /// from beginIndex to (not including) endIndex by the argument factor.
        /// </summary>
        public void AdjustVelocities(int beginIndex, int endIndex, double factor)
        {
            CheckIndices(beginIndex, endIndex);
            for(int i = beginIndex; i < endIndex; ++i)
            {
                MidiChordDef iumdd = _uniqueDefs[i] as MidiChordDef;
                if(iumdd != null)
                {
                    iumdd.AdjustVelocities(factor);
                }
            }
        }
        /// <summary>
        /// Multiplies each velocity value in the MidiChordDefs by the argument factor.
        /// </summary>
        public void AdjustVelocities(double factor)
        {
            foreach(MidiChordDef mcd in MidiChordDefs)
            {
                mcd.AdjustVelocities(factor);
            }
        }

        /// Creates a hairpin in the velocities from startMsPosition to endMsPosition (non-inclusive).
        /// This function does NOT change velocities outside the range given in its arguments.
        /// There must be at least two IUniqueDefs in the msPosition range given in the arguments.
        /// The factors by which the velocities are multiplied change arithmetically:
        /// The velocity of the first IUniqueDefs is multiplied by startFactor, and the velocity
        /// of the last MidiChordDef in range by endFactor.
        /// Can be used to create a diminueno or crescendo.
        /// <param name="startMsPosition">MsPositionReFirstIUD</param>
        /// <param name="endMsPosition">MsPositionReFirstIUD</param>
        /// <param name="startFactor"></param>
        /// <param name="endFactor"></param>
        public void AdjustVelocitiesHairpin(int startMsPosition, int endMsPosition, double startFactor, double endFactor)
        {
            int beginIndex = FindIndexAtMsPositionReFirstIUD(startMsPosition);
            int endIndex = FindIndexAtMsPositionReFirstIUD(endMsPosition);

            Debug.Assert(((beginIndex + 1) < endIndex) && (startFactor >= 0) && (endFactor >= 0) && (endIndex <= Count));

            int nNonMidiChordDefs = GetNumberOfNonMidiOrInputChordDefs(beginIndex, endIndex);

            double factorIncrement = (endFactor - startFactor) / (endIndex - beginIndex - nNonMidiChordDefs - 1);
            double factor = startFactor;
            List<IUniqueDef> lmdds = _uniqueDefs;

            for(int i = beginIndex; i < endIndex; ++i)
            {
                MidiChordDef iumdd = _uniqueDefs[i] as MidiChordDef;
                if(iumdd != null)
                {
                    iumdd.AdjustVelocities(factor);
                    factor += factorIncrement;
                }
            }
        }
        /// <summary>
        /// Creates a moving pan from startPanValue at startMsPosition to endPanValue at endMsPosition.
        /// Implemented using one pan value per MidiChordDef.
        /// This function does NOT change pan values outside the position range given in its arguments.
        /// </summary>
        public void SetPanGliss(int startMsPosition, int endMsPosition, int startPanValue, int endPanValue)
        {
            int beginIndex = FindIndexAtMsPositionReFirstIUD(startMsPosition);
            int endIndex = FindIndexAtMsPositionReFirstIUD(endMsPosition);

            Debug.Assert(((beginIndex + 1) < endIndex) && (startPanValue >= 0) && (startPanValue <= 127)
                && (endPanValue >= 0) && (endPanValue <=127) && (endIndex <= Count));

            int nNonMidiChordDefs = GetNumberOfNonMidiOrInputChordDefs(beginIndex, endIndex);

            double increment = ((double)(endPanValue - startPanValue)) / (endIndex - beginIndex - nNonMidiChordDefs);
            int panValue = startPanValue;
            List<IUniqueDef> lmdds = _uniqueDefs;

            for(int i = beginIndex; i < endIndex; ++i)
            {
                MidiChordDef iumdd = _uniqueDefs[i] as MidiChordDef;
                if(iumdd != null)
                {
                    iumdd.PanMsbs = new List<byte>() { (byte)panValue };
                    panValue += (int)increment;
                }
            }
        }
        /// <summary>
        /// Sets the pitchwheelDeviation for MidiChordDefs in the range beginIndex to (not including) endindex.
        /// Rests in the range dont change.
        /// </summary>
        public void SetPitchWheelDeviation(int beginIndex, int endIndex, int deviation)
        {
            CheckIndices(beginIndex, endIndex);

            for(int i = beginIndex; i < endIndex; ++i)
            {
                MidiChordDef mcd = this[i] as MidiChordDef;
                if(mcd != null)
                {
                    mcd.PitchWheelDeviation = M.MidiValue(deviation);
                }
            }
        }
        /// <summary>
        /// Removes the pitchwheel commands (not the pitchwheelDeviations)
        /// from chords in the range beginIndex to (not including) endIndex.
        /// Rests in the range are not changed.
        /// </summary>
        public void RemoveScorePitchWheelCommands(int beginIndex, int endIndex)
        {
            CheckIndices(beginIndex, endIndex);

            for(int i = beginIndex; i < endIndex; ++i)
            {
                MidiChordDef iumdd = this[i] as MidiChordDef;
                if(iumdd != null)
                {
                    MidiChordDef umcd = iumdd as MidiChordDef;
                    if(umcd != null)
                    {
                        umcd.MidiChordSliderDefs.PitchWheelMsbs = new List<byte>();
                    }
                }
            }
        }

        /// <summary>
        /// Creates an exponential change (per index) of pitchwheelDeviation from startMsPosition to endMsPosition,
        /// </summary>
        /// <param name="finale"></param>
        protected void AdjustPitchWheelDeviations(int startMsPosition, int endMsPosition, int startPwd, int endPwd)
        {
            double furies1StartPwdValue = startPwd, furies1EndPwdValue = endPwd;
            int beginIndex = FindIndexAtMsPositionReFirstIUD(startMsPosition);
            int endIndex = FindIndexAtMsPositionReFirstIUD(endMsPosition);

            int nNonMidiChordDefs = GetNumberOfNonMidiOrInputChordDefs(beginIndex, endIndex);

            double pwdfactor = Math.Pow(furies1EndPwdValue / furies1StartPwdValue, (double)1 / (endIndex - beginIndex - nNonMidiChordDefs)); // f13.Count'th root of furies1EndPwdValue/furies1StartPwdValue -- the last pwd should be furies1EndPwdValue

            for(int i = beginIndex; i < endIndex; ++i)
            {
                MidiChordDef umc = _uniqueDefs[i] as MidiChordDef;
                if(umc != null)
                {
                    umc.PitchWheelDeviation = M.MidiValue((int)(furies1StartPwdValue * (Math.Pow(pwdfactor, i))));
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

            AssertVoiceDefConsistency();
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
        #endregion alignment

        #region Re-ordering the Trk's UniqueDefs

        #region Permute()
        /// <summary>
        /// This function permutes any number of UniqueDefs in the trk's UniqueDefs list according to the contour retrieved (from
        /// the static K.Contour[] array) using the axisNumber and contourNumber arguments. The trk's AxisIndex property is set.
        /// If there are more than 7 UniqueDefs in the list, 7 partitions are automatically created and permuted recursively
        /// using the same contour.
        /// </summary>
        /// <param name="axisNumber">A value greater than or equal to 1, and less than or equal to 12 An exception is thrown if this is not the case.</param>
        /// <param name="contourNumber">A value greater than or equal to 1, and less than or equal to 12. An exception is thrown if this is not the case.</param>
        public void Permute(int axisNumber, int contourNumber)
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
                AssertVoiceDefConsistency();
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
        public void PermutePartitions(int axisNumber, int contourNumber, List<int> partitionSizes)
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

            AssertVoiceDefConsistency();
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
        private static int[,] axisIndices =
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

        public void SortRootNotatedPitchAscending()
        {
            SortByRootNotatedPitch(true);
        }

        public void SortRootNotatedPitchDescending()
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
        private void SortByRootNotatedPitch(bool ascending)
        {
            Debug.Assert(!(Container is Block), "Cannot sort inside a Block.");

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

        public void SortVelocityIncreasing()
        {
            SortByVelocity(true);
        }

        public void SortVelocityDecreasing()
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
        private void SortByVelocity(bool increasing)
        {
            Debug.Assert(!(Container is Block), "Cannot sort inside a Block.");

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
                if(iuds[i] is RestDef)
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
        private IEnumerable<MidiChordDef> MidiChordDefs
        {
            get
            {
                foreach(IUniqueDef iud in _uniqueDefs)
                {
                    MidiChordDef midiChordDef = iud as MidiChordDef;
                    if(midiChordDef != null)
                        yield return midiChordDef;
                }
            }
        }
        #endregion

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
        /// The number of MidiChordDefs and RestDefs in this TrkDef
        /// </summary>
        public int DurationsCount
		{
			get
			{
				int count = 0;
				foreach(IUniqueDef iud in _uniqueDefs)
				{
					if(iud is MidiChordDef || iud is RestDef)
					{
						count++;
					}
				}
				return count;
			}
		}

        /// <summary>
        /// Setting this Property cannot lead to the minimum MsPositionReContainer in the Seq being greater than zero.
        /// </summary>
        public override int MsPositionReContainer
        {
            get
            {
                return base.MsPositionReContainer;
            }
            set
            {
                Debug.Assert(!(Container is Block), "Cannot set MsPosReContainer inside a Block.");
                base.MsPositionReContainer = value; // can be negative
            }
        }

        /// <summary>
        /// The composition algorithm must set the MasterVolume (to a value != null)
        /// in every TrkDef in the first bar of the score.
        /// All other TrkDef.MasterVolumes retain the default value null. 
        /// </summary>
        public byte? MasterVolume = null; // default value
	}
}
