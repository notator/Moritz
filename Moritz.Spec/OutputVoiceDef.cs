
using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections;

using Krystals4ObjectLibrary;
using Moritz.Globals;

namespace Moritz.Spec
{
    /// <summary>
    /// A temporal sequence of IUniqueDef objects.
    /// <para>(IUniqueDef is implemented by all Unique...Defs that will eventually be converted to NoteObjects.)</para>
    /// <para></para>
    /// <para>This class is IEnumerable, so that foreach loops can be used.</para>
    /// <para>For example:</para>
    /// <para>foreach(IUniqueDef iumdd in outputVoiceDef) { ... }</para>
    /// <para>An Enumerator for MidiChordDefs is also defined so that</para>
    /// <para>foreach(MidiChordDef mcd in outputVoiceDef.MidiChordDefs) { ... }</para>
    /// <para>can also be used.</para>
    /// <para>This class is also indexable, as in:</para>
    /// <para>IUniqueDef iu = outputVoiceDef[index];</para>
    /// </summary>
    public class OutputVoiceDef : VoiceDef
    {
        #region constructors
        /// <summary>
        /// An empty VoiceDef
        /// </summary>
        /// <param name="msDuration"></param>
        public OutputVoiceDef()
            : base()
        {
        }

        /// <summary>
        /// A VoiceDef beginning at MsPosition = 0, and containing a single RestDef having msDuration
        /// </summary>
        /// <param name="msDuration"></param>
        public OutputVoiceDef(int msDuration)
            : base(msDuration)
        {
        }

        /// <summary>
        /// <para>If the argument is not empty, the MsPositions and MsDurations in the list are checked for consistency.</para>
        /// <para>The new VoiceDef's UniqueDefs list is simply set to the argument (which is not cloned).</para>
        /// </summary>
        public OutputVoiceDef(List<IUniqueDef> iuds) 
            : base(iuds)
        {
        }

        /// <summary>
        /// Returns a deep clone of this OutputVoiceDef.
        /// </summary>
        public OutputVoiceDef DeepClone()
        {
            List<IUniqueDef> clonedLmdds = new List<IUniqueDef>();
            foreach(IUniqueDef iu in this._uniqueDefs)
            {
                IUniqueDef clone = iu.DeepClone();
                clonedLmdds.Add(clone);
            }

            // Clefchange symbols must point at the following object in their own VoiceDef
            for(int i = 0; i < clonedLmdds.Count; ++i)
            {
                ClefChangeDef clone = clonedLmdds[i] as ClefChangeDef;
                if(clone != null)
                {
                    Debug.Assert(i < (clonedLmdds.Count - 1));
                    ClefChangeDef replacement = new ClefChangeDef(clone.ClefType, clonedLmdds[i + 1]);
                    clonedLmdds.RemoveAt(i);
                    clonedLmdds.Insert(i, replacement);
                }
            }

            return new OutputVoiceDef(clonedLmdds);
        }
        #endregion constructors

        #region Count changers
        /// <summary>
        /// Appends the new iUniqueDef to the end of the list.
        /// </summary>
        /// <param name="iUniqueDef"></param>
        public override void Add(IUniqueDef iUniqueDef)
        {
            Debug.Assert(!(iUniqueDef is InputChordDef));
            _Add(iUniqueDef);
        }
        /// <summary>
        /// Adds the argument to the end of this VoiceDef.
        /// Sets the MsPositions of the appended UniqueDefs.
        /// </summary>
        public void AddRange(OutputVoiceDef voiceDef)
        {
            _AddRange((VoiceDef)voiceDef);
        }
        /// <summary>
        /// Inserts the iUniqueDef in the list at the given index, and then
        /// resets the positions of all the uniqueDefs in the list.
        /// </summary>
        public override void Insert(int index, IUniqueDef iUniqueDef)
        {
            Debug.Assert(!(iUniqueDef is InputChordDef));
            _Insert(index, iUniqueDef);
        }
        /// <summary>
        /// Inserts the voiceDef in the list at the given index, and then
        /// resets the positions of all the uniqueDefs in the list.
        /// </summary>
        public void InsertRange(int index, OutputVoiceDef voiceDef)
        {
            _InsertRange(index, (VoiceDef)voiceDef);
        }
        /// <summary>
        /// Creates a new OutputVoiceDef containing just the argument midiChordDef,
        /// then calls the other InsertInRest() function with the voiceDef as argument. 
        /// </summary>
        public void InsertInRest(MidiChordDef midiChordDef)
        {
            List<IUniqueDef> iuds = new List<IUniqueDef>() { midiChordDef };
            OutputVoiceDef iVoiceDef = new OutputVoiceDef(iuds);
            InsertInRest(iVoiceDef);
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
        public void InsertInRest(OutputVoiceDef outputVoiceDef)
        {
            Debug.Assert(outputVoiceDef[0] is MidiChordDef && outputVoiceDef[outputVoiceDef.Count - 1] is MidiChordDef);
            _InsertInRest((VoiceDef)outputVoiceDef);
        }
        /// <summary>
        /// Removes the iUniqueDef at index from the list, and then inserts the replacement at the same index.
        /// </summary>
        public void Replace(int index, IUniqueDef replacementIUnique)
        {
            Debug.Assert(!(replacementIUnique is InputChordDef));
            _Replace(index, replacementIUnique);
        }
        #endregion Count changers

        #region OutputVoiceDef duration changers
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
        /// The total duration of this OutputVoiceDef changes accordingly.
        /// </summary>
        public void AdjustChordMsDurations(double factor, int minThreshold = 100)
        {
            AdjustMsDurations<MidiChordDef>(0, _uniqueDefs.Count, factor, minThreshold);
        }
        #endregion OutputVoiceDef duration changers

        #region MidiChordDef attribute changers
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
        /// Multiplies each expression value in the UniqueMidiDurationDefs by the argument factor.
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
        #region deprecated function
        /// <summary>
        /// ACHTUNG: This function is deprecated!! Use the other AdjustVelocitiesHairpin(...).
        /// First creates a hairpin in the velocities from beginIndex to endIndex (non-inclusive),
        /// then adjusts all the remaining velocities in this VoiceDef by the finalFactor.
        /// endIndex must be greater than beginIndex + 1.
        /// The factors by which the velocities are multiplied change arithmetically: The velocities
        /// at beginIndex are multiplied by 1.0, and the velocities from endIndex to the end of the
        /// VoiceDef by finalFactor.
        /// Can be used to create a diminueno or crescendo.
        /// </summary>
        /// <param name="beginDimIndex"></param>
        /// <param name="endDimIndex"></param>
        /// <param name="p"></param>
        public void AdjustVelocitiesHairpin(int beginIndex, int endIndex, double finalFactor)
        {
            Debug.Assert(((beginIndex + 1) < endIndex) && (finalFactor >= 0) && (endIndex <= Count));

            int nNonMidiChordDefs = GetNumberOfNonMidiOrInputChordDefs(beginIndex, endIndex);

            double factorIncrement = (finalFactor - 1.0) / (endIndex - beginIndex - nNonMidiChordDefs);
            double factor = 1.0;

            for(int i = beginIndex; i < endIndex; ++i)
            {
                MidiChordDef iumdd = _uniqueDefs[i] as MidiChordDef;
                if(iumdd != null)
                {
                    iumdd.AdjustVelocities(factor);
                    factor += factorIncrement;
                }
            }

            for(int i = endIndex; i < _uniqueDefs.Count; ++i)
            {
                MidiChordDef iumdd = _uniqueDefs[i] as MidiChordDef;
                if(iumdd != null)
                {
                    iumdd.AdjustVelocities(factor);
                }
            }
        }
        #endregion deprecated function
        /// Creates a hairpin in the velocities from startMsPosition to endMsPosition (non-inclusive).
        /// This function does NOT change velocities outside the range given in its arguments.
        /// There must be at least two IUniqueMidiDurationDefs in the msPosition range given in the arguments.
        /// The factors by which the velocities are multiplied change arithmetically:
        /// The velocity of the first IUniqueMidiDurationDefs is multiplied by startFactor, and the velocity
        /// of the last MidiChordDef in range by endFactor.
        /// Can be used to create a diminueno or crescendo.
        public void AdjustVelocitiesHairpin(int startMsPosition, int endMsPosition, double startFactor, double endFactor)
        {
            int beginIndex = FindIndexAtMsPosition(startMsPosition);
            int endIndex = FindIndexAtMsPosition(endMsPosition);

            Debug.Assert(((beginIndex + 1) < endIndex) && (startFactor >= 0) && (endFactor >= 0) && (endIndex <= Count));

            int nNonMidiChordDefs = GetNumberOfNonMidiOrInputChordDefs(beginIndex, endIndex);

            double factorIncrement = (endFactor - startFactor) / (endIndex - beginIndex - nNonMidiChordDefs);
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
            int beginIndex = FindIndexAtMsPosition(startMsPosition);
            int endIndex = FindIndexAtMsPosition(endMsPosition);

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
        /// Sets the pitchwheelDeviation for MidichordDefs in the range beginIndex to (not including) endindex.
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
            int beginIndex = FindIndexAtMsPosition(startMsPosition);
            int endIndex = FindIndexAtMsPosition(endMsPosition);

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
        #endregion MidiChordDef attribute changers)

        /// <summary>
        /// The composition algorithm must set the MasterVolume (to a value != null)
        /// in every OutputVoiceDef in the first bar of the score.
        /// All other OutputVoiceDefs retain the default value 0. 
        /// </summary>
        public byte? MasterVolume = null; // default value
        /// <summary>
        /// Is set at the same time as the MasterVolume.
        /// </summary>
        public byte MidiChannel = 0;
    }
}
