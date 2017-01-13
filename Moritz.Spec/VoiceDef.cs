
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
    /// <para>foreach(IUniqueDef iumdd in voiceDef) { ... }</para>
    /// <para>An Enumerator for MidiChordDefs is also defined so that</para>
    /// <para>foreach(MidiChordDef mcd in voiceDef.MidiChordDefs) { ... }</para>
    /// <para>can also be used.</para>
    /// <para>This class is also indexable, as in:</para>
    /// <para>IUniqueDef iu = this[index];</para>
    /// </summary>
    public abstract class VoiceDef : IEnumerable
    {
        #region constructors

        protected VoiceDef(int midiChannel, int msPositionReContainer, List<IUniqueDef> iuds)
        {
            Debug.Assert(midiChannel >= 0 && msPositionReContainer >= 0 && iuds != null);

            this.MidiChannel = midiChannel;
            this._msPositionReContainer = msPositionReContainer;
            this._uniqueDefs = iuds;

            SetMsPositionsReFirstUD();

            AssertVoiceDefConsistency();
        }
        #endregion constructors

        #region public indexer & enumerator
        /// <summary>
        /// Indexer. Allows individual UniqueDefs to be accessed using array notation on the VoiceDef.
        /// Automatically resets the MsPositions of all UniqueDefs in the list.
        /// e.g. iumdd = voiceDef[3].
        /// </summary>
        public IUniqueDef this[int i]
        {
            get
            {
                if(i < 0 || i >= _uniqueDefs.Count)
                {
					Debug.Assert(false, "Index out of range");
                }
                return _uniqueDefs[i];
            }
            set
            {
                if(i < 0 || i >= _uniqueDefs.Count)
                {
					Debug.Assert(false, "Index out of range");
				}
                
                Debug.Assert(!((this is Trk && value is InputChordDef) || (this is InputVoiceDef && value is MidiChordDef)));

                _uniqueDefs[i] = value;
                SetMsPositionsReFirstUD();
                AssertVoiceDefConsistency();
            }
        }

        #region Enumerators
        public IEnumerator GetEnumerator()
        {
            return new MyEnumerator(_uniqueDefs);
        }
        // private enumerator class
        // see http://support.microsoft.com/kb/322022/en-us
        private class MyEnumerator : IEnumerator
        {
            public List<IUniqueDef> _uniqueDefs;
            int position = -1;
            //constructor
            public MyEnumerator(List<IUniqueDef> uniqueDefs)
            {
                _uniqueDefs = uniqueDefs;
            }
            private IEnumerator getEnumerator()
            {
                return (IEnumerator)this;
            }
            //IEnumerator
            public bool MoveNext()
            {
                position++;
                return (position < _uniqueDefs.Count);
            }
            //IEnumerator
            public void Reset()
            { position = -1; }
            //IEnumerator
            public object Current
            {
                get
                {
                    try
                    {
                        return _uniqueDefs[position];
                    }
                    catch(IndexOutOfRangeException)
                    {
                        Debug.Assert(false);
                        return null;
                    }
                }
            }
        }  //end nested class
        #endregion
        #endregion public indexer & enumerator

        #region consistency
        protected void CheckIndices(int beginIndex, int endIndex)
        {
            Debug.Assert(endIndex >= 1 && endIndex < _uniqueDefs.Count, "Error: endIndex out of range.");
            Debug.Assert(beginIndex < endIndex, "Error: endIndex must be greater than beginIndex");
        }

        protected void AssertVoiceDefConsistency()
        {
            Debug.Assert(MidiChannel >= 0 && MidiChannel <= 15);
            Debug.Assert(UniqueDefs != null);
            if(UniqueDefs.Count > 0)
            {
                Debug.Assert(UniqueDefs[0].MsPositionReFirstUD == 0);

                for(int i = 1; i < UniqueDefs.Count; ++i)
                {
                    IUniqueDef prevIUD = UniqueDefs[i - 1];
                    int msPosition = prevIUD.MsPositionReFirstUD + prevIUD.MsDuration;
                    Debug.Assert(UniqueDefs[i].MsPositionReFirstUD == msPosition);
                }
            }
        }

        internal abstract void AssertConsistentInBlock();
        #endregion consistency

        #region  miscellaneous
        /// <summary>
        /// Sets the MsPosition attribute of each IUniqueDef in the _uniqueDefs list.
        /// Uses all the MsDuration attributes, and _msPosition as origin.
        /// This function must be called at the end of any function that changes the _uniqueDefs list.
        /// </summary>
        protected void SetMsPositionsReFirstUD()
        {
            if(_uniqueDefs.Count > 0)
            {
                int currentPositionReFirstIUD = 0;
                foreach(IUniqueDef umcd in _uniqueDefs)
                {
                    umcd.MsPositionReFirstUD = currentPositionReFirstIUD;
                    currentPositionReFirstIUD += umcd.MsDuration;
                }
            }
        }
        /// <summary>
        /// Rests dont have lyrics, so their index in the VoiceDef can't be shown as a lyric.
        /// Overridden by Clytemnestra, where the index is inserted before her lyrics.
        /// </summary>
        /// <param name="voiceDef"></param>
        public virtual void SetLyricsToIndex()
        {
            for(int index = 0; index < _uniqueDefs.Count; ++index)
            {
                IUniqueSplittableChordDef lmcd = _uniqueDefs[index] as IUniqueSplittableChordDef;
                if(lmcd != null)
                {
                    lmcd.Lyric = index.ToString();
                }
            }
        }
        /// <summary>
        /// If the index is equal to or greater than the number of chords, rests and clefChanges in the voiceDef,
        /// the ClefChange will be appended to the voiceDef.
        /// Notes: 1) When changing clefs more than once in the same VoiceDef, it is easier to get the indices right if
        ///           they are added backwards.
        ///        2) if a ClefChange is defined here on a RestDef which has no MidiChordDef to its right on the staff,
        ///           the resulting ClefSymbol will be placed immediately before the final barline on the staff.  
        ///        3) The clefType must be one of the following strings "t", "t1", "t2", "t3", "b", "b1", "b2", "b3"
        /// </summary>
        public void InsertClefChange(int index, string clefType)
        {
            #region check args
            Debug.Assert(index >= 0);

            if(String.Equals(clefType, "t") == false
            && String.Equals(clefType, "t1") == false
            && String.Equals(clefType, "t2") == false
            && String.Equals(clefType, "t3") == false
            && String.Equals(clefType, "b") == false
            && String.Equals(clefType, "b1") == false
            && String.Equals(clefType, "b2") == false
            && String.Equals(clefType, "b3") == false)
            {
                Debug.Assert(false, "Unknown clef type.");
            }
            #endregion

            if(index > _uniqueDefs.Count - 1)
            {
                ClefChangeDef clefChangeDef = new ClefChangeDef(clefType, EndMsPositionReFirstIUD);
                _uniqueDefs.Add(clefChangeDef);
            }
            else
            {
                ClefChangeDef clefChangeDef = new ClefChangeDef(clefType, _uniqueDefs[index].MsPositionReFirstUD);
                _uniqueDefs.Insert(index, clefChangeDef);
            }
        }
        #endregion miscellaneous

        #region attribute changers (Transpose etc.)

        /// <summary>
        /// An object is a NonMidiOrInputChordDef if it is not a MidiChordDef and it is not an InputChordDef.
        /// For example: a CautionaryChordDef, a RestDef or ClefChangeDef.
        /// </summary>
        /// <param name="beginIndex"></param>
        /// <param name="endIndex"></param>
        /// <returns></returns>
        protected int GetNumberOfNonMidiOrInputChordDefs(int beginIndex, int endIndex)
        {
            int nNonMidiChordDefs = 0;
            for(int i = beginIndex; i <= endIndex; ++i)
            {
                if(!(_uniqueDefs[i] is MidiChordDef) && !(_uniqueDefs[i] is InputChordDef))
                    nNonMidiChordDefs++;
            }
            return nNonMidiChordDefs;
        }

        #region Envelopes
        /// <summary>
        /// See Envelope.TimeWarp() for a description of the arguments.
        /// </summary>
        /// <param name="envelope"></param>
        /// <param name="distortion"></param>
        public void TimeWarp(Envelope envelope, double distortion)
        {
            #region requirements
            Debug.Assert(distortion >= 1);
            Debug.Assert(_uniqueDefs.Count > 0);
            #endregion

            int originalMsDuration = MsDuration;

            #region 1. create a List of ints containing the msPositions of the DurationDefs plus the end msPosition of the final DurationDef.
            List<DurationDef> durationDefs = new List<DurationDef>();
            List<int> originalPositions = new List<int>();
            int msPos = 0;
            foreach(IUniqueDef iud in UniqueDefs)
            {
                DurationDef dd = iud as DurationDef;
                if(dd != null)
                {
                    durationDefs.Add(dd);
                    originalPositions.Add(msPos);
                    msPos += dd.MsDuration;
                }
            }
            originalPositions.Add(msPos); // end position of duration to warp.
            #endregion
            List<int> newPositions = envelope.TimeWarp(originalPositions, distortion);

            for(int i = 0; i < durationDefs.Count; ++i)
            {
                DurationDef dd = durationDefs[i];
                dd.MsDuration = newPositions[i + 1] - newPositions[i];
            }

            SetMsPositionsReFirstUD();

            AssertVoiceDefConsistency();
        }
        #endregion Envelopes

        #region Transpose
        /// <summary>
        /// Transpose all the IUniqueChordDefs from beginIndex to (including) endIndex
        /// up by the number of semitones given in the interval argument.
        /// IUniqueChordDefs are MidiChordDef, InputChordDef and CautionaryChordDef.
        /// Negative interval values transpose down.
        /// It is not an error if Midi pitch values would exceed the range 0..127.
        /// In this case, they are silently coerced to 0 or 127 respectively.
        /// </summary>
        /// <param name="interval"></param>
        public void Transpose(int beginIndex, int endIndex, int interval)
        {
            CheckIndices(beginIndex, endIndex);

            for(int i = beginIndex; i <= endIndex; ++i)
            {
                IUniqueChordDef iucd = _uniqueDefs[i] as IUniqueChordDef;
                if(iucd != null)
                {
                    iucd.Transpose(interval);
                }
            }
        }
        /// <summary>
        /// Transpose the whole VoiceDef up by the number of semitones given in the argument.
        /// </summary>
        /// <param name="interval"></param>
        public void Transpose(int interval)
        {
            Transpose(0, _uniqueDefs.Count, interval);
        }
        /// <summary>
        /// Transposes all the MidiHeadSymbols in this VoiceDef by the number of semitones in the argument
        /// without changing the sound. Negative arguments transpose downwards.
        /// If the resulting midiHeadSymbol would be less than 0 or greater than 127,
        /// it is silently coerced to 0 or 127 respectively.
        /// </summary>
        /// <param name="p"></param>
        public void TransposeNotation(int semitonesToTranspose)
        {
            foreach(IUniqueDef iud in _uniqueDefs)
            {
                IUniqueChordDef iucd = iud as IUniqueChordDef;
                if(iucd != null)
                {
                    List<byte> midiPitches = iucd.NotatedMidiPitches;
                    for(int i = 0; i < midiPitches.Count; ++i)
                    {
                        midiPitches[i] = M.MidiValue(midiPitches[i] + semitonesToTranspose);
                    }
                }
            }
        }
        /// <summary>
        /// Each IUniqueChordDef is transposed by the interval current at its position.
        /// The argument contains a dictionary[msPosition, transposition].
        /// </summary>
        /// <param name="msPosTranspositionDict"></param>
        public void TransposeToDict(Dictionary<int, int> msPosTranspositionDict)
        {
            List<int> dictPositions = new List<int>(msPosTranspositionDict.Keys);

            int currentMsPosReFirstIUD = dictPositions[0];
            int j = 0;
            for(int i = 1; i < msPosTranspositionDict.Count; ++i)
            {
                int transposition = msPosTranspositionDict[currentMsPosReFirstIUD];
                int nextMsPosReFirstIUD = dictPositions[i];
                while(j < _uniqueDefs.Count && _uniqueDefs[j].MsPositionReFirstUD < nextMsPosReFirstIUD)
                {
                    if(_uniqueDefs[j].MsPositionReFirstUD >= currentMsPosReFirstIUD)
                    {
                        IUniqueChordDef iucd = _uniqueDefs[j] as IUniqueChordDef;
                        if(iucd != null)
                        {
                            iucd.Transpose(transposition);
                        }
                    }
                    ++j;
                }
                currentMsPosReFirstIUD = nextMsPosReFirstIUD;
            }
        }

        /// <summary>
        /// Transposes the UniqueDefs from the beginIndex upto (including) endIndex
        /// by an equally increasing amount, so that the final MidiChordDef or InputChordDef is transposed by glissInterval.
        /// glissInterval can be negative.
        /// </summary>
        public void StepwiseGliss(int beginIndex, int endIndex, int glissInterval)
        {
            CheckIndices(beginIndex, endIndex);

            int nNonMidiChordDefs = GetNumberOfNonMidiOrInputChordDefs(beginIndex, endIndex);

            int nSteps = (endIndex - beginIndex - nNonMidiChordDefs);
            double interval = ((double)glissInterval) / nSteps;
            double step = interval;
            for(int i = beginIndex; i <= endIndex; ++i)
            {
                MidiChordDef mcd = _uniqueDefs[i] as MidiChordDef;
                InputChordDef icd = _uniqueDefs[i] as InputChordDef;
                IUniqueChordDef iucd = (mcd == null) ? (IUniqueChordDef)icd : (IUniqueChordDef)mcd;
                if(iucd != null)
                {
                    iucd.Transpose((int)Math.Round(interval));
                    interval += step;
                }
            }
        }
        #endregion Transpose

        #endregion  attribute changers (Transpose etc.)

        #region Count changers
        #region list functions
        public abstract void Add(IUniqueDef iUniqueDef);
        protected void _Add(IUniqueDef iUniqueDef)
        {
            Debug.Assert(!(Container is Block), "Cannot Add IUniqueDefs inside a Block.");

            if(_uniqueDefs.Count > 0)
			{
				IUniqueDef lastIud = _uniqueDefs[_uniqueDefs.Count - 1];
				iUniqueDef.MsPositionReFirstUD = lastIud.MsPositionReFirstUD + lastIud.MsDuration;
			}
			else
			{
				iUniqueDef.MsPositionReFirstUD = 0;
			}
            _uniqueDefs.Add(iUniqueDef);

            AssertVoiceDefConsistency();
        }
        public abstract void AddRange(VoiceDef voiceDef);
        protected void _AddRange(VoiceDef voiceDef)
        {
            Debug.Assert(!(Container is Block), "Cannot AddRange of VoiceDefs inside a Block.");

            _uniqueDefs.AddRange(voiceDef.UniqueDefs);
            SetMsPositionsReFirstUD();

            AssertVoiceDefConsistency();
        }

        public abstract void Insert(int index, IUniqueDef iUniqueDef);
        protected void _Insert(int index, IUniqueDef iUniqueDef)
        {
            Debug.Assert(!(Container is Block && iUniqueDef.MsDuration > 0), "Cannot Insert IUniqueDefs that have msDuration inside a Block.");

            _uniqueDefs.Insert(index, iUniqueDef);
            SetMsPositionsReFirstUD();

            AssertVoiceDefConsistency();
        }
        protected void _InsertRange(int index, VoiceDef voiceDef)
        {
            Debug.Assert(!(Container is Block), "Cannot Insert range of IUniqueDefs inside a Block.");

            _uniqueDefs.InsertRange(index, voiceDef.UniqueDefs);
            SetMsPositionsReFirstUD();

            AssertVoiceDefConsistency();
        }

        /// <summary>
        /// removes the iUniqueDef from the list, and then resets the positions of all the iUniqueDefs in the list.
        /// </summary>
        public void Remove(IUniqueDef iUniqueDef)
        {
            Debug.Assert(_uniqueDefs.Count > 0);
            Debug.Assert(_uniqueDefs.Contains(iUniqueDef));
            _uniqueDefs.Remove(iUniqueDef);
            SetMsPositionsReFirstUD();

            AssertVoiceDefConsistency();
        }
        /// <summary>
        /// Removes the iUniqueDef at index from the list, and then resets the positions of all the lmdds in the list.
        /// </summary>
        public void RemoveAt(int index)
        {
            Debug.Assert(index >= 0 && index < _uniqueDefs.Count);
            _uniqueDefs.RemoveAt(index);
            SetMsPositionsReFirstUD();

            AssertVoiceDefConsistency();
        }
        /// <summary>
        /// Removes count iUniqueDefs from the list, startíng with the iUniqueDef at index.
        /// </summary>
        public void RemoveRange(int index, int count)
        {
            Debug.Assert(index >= 0 && count >= 0 && ((index + count) <= _uniqueDefs.Count));
            _uniqueDefs.RemoveRange(index, count);
            SetMsPositionsReFirstUD();

            AssertVoiceDefConsistency();
        }

        /// <summary>
        /// Remove the IUniqueDefs which start between startMsPosReFirstIUD and (not including) endMsPosReFirstIUD 
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        public void RemoveBetweenMsPositions(int startMsPosReFirstIUD, int endMsPosReFirstIUD)
        {
            IUniqueDef iumdd = _uniqueDefs.Find(f => (f.MsPositionReFirstUD >= startMsPosReFirstIUD) && (f.MsPositionReFirstUD < endMsPosReFirstIUD));
            while(iumdd != null)
            {
                _uniqueDefs.Remove(iumdd);
                iumdd = _uniqueDefs.Find(f => (f.MsPositionReFirstUD >= startMsPosReFirstIUD) && (f.MsPositionReFirstUD < endMsPosReFirstIUD));
            }
            SetMsPositionsReFirstUD();

            AssertVoiceDefConsistency();
        }
        /// <summary>
        /// Removes the iUniqueMidiDurationDef at index from the list, and then inserts the replacement at the same index.
        /// </summary>
        protected void _Replace(int index, IUniqueDef replacementIUnique)
        {
            Debug.Assert(!(Container is Block), "Cannot Replace IUniqueDefs inside a Block.");

            Debug.Assert(index >= 0 && index < _uniqueDefs.Count);
            _uniqueDefs.RemoveAt(index);
            _uniqueDefs.Insert(index, replacementIUnique);
            SetMsPositionsReFirstUD();

            AssertVoiceDefConsistency();
        }
        /// <summary>
        /// Replace all the IUniqueDefs from startMsPosition to (not including) endMsPosition by a single rest.
        /// </summary>
        public void Erase(int startMsPosition, int endMsPosition)
        {
            int beginIndex = FindIndexAtMsPositionReFirstIUD(startMsPosition);
            int endIndex = FindIndexAtMsPositionReFirstIUD(endMsPosition);

            for(int i = beginIndex; i < endIndex; ++i)
            {
                MidiChordDef mcd = this[i] as MidiChordDef;
                InputChordDef icd = this[i] as InputChordDef;
                IUniqueDef iud = (mcd == null) ? (IUniqueDef)icd : (IUniqueDef)mcd;
                if(iud != null)
                {
                    RestDef restDef;
                    if(this is InputVoiceDef)
                    {
                        restDef = new InputRestDef(iud.MsPositionReFirstUD, iud.MsDuration);
                    }
                    else
                    {
                        restDef = new MidiRestDef(iud.MsPositionReFirstUD, iud.MsDuration);
                    }
                    RemoveAt(i);
                    Insert(i, restDef);
                }
            }

            AgglomerateRests();

            AssertVoiceDefConsistency();
        }
        /// <summary>
        /// Extracts nUniqueDefs from the uniqueDefs, and then inserts them again at the toIndex.
        /// </summary>
        public void Translate(int fromIndex, int nUniqueDefs, int toIndex)
        {
            Debug.Assert((fromIndex + nUniqueDefs) <= _uniqueDefs.Count);
            Debug.Assert(toIndex <= (_uniqueDefs.Count - nUniqueDefs));
            List<IUniqueDef> extractedLmdds = _uniqueDefs.GetRange(fromIndex, nUniqueDefs);
            _uniqueDefs.RemoveRange(fromIndex, nUniqueDefs);
            _uniqueDefs.InsertRange(toIndex, extractedLmdds);
            SetMsPositionsReFirstUD();

            AssertVoiceDefConsistency();
        }
        /// <summary>
        /// Returns the index of the IUniqueDef which starts at or is otherwise current at msPosition.
        /// If msPosition is the EndMsPosition, the index of the final IUniqueDef + 1 (=Count) is returned.
        /// If the VoiceDef does not span msPosition, -1 (=error) is returned.
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public int FindIndexAtMsPositionReFirstIUD(int msPositionReFirstIUD)
        {
            int returnedIndex = -1;
            if(msPositionReFirstIUD == EndMsPositionReFirstIUD)
            {
                returnedIndex = this.Count;
            }
            else if(msPositionReFirstIUD >= _uniqueDefs[0].MsPositionReFirstUD && msPositionReFirstIUD < EndMsPositionReFirstIUD)
            {
                returnedIndex = _uniqueDefs.FindIndex(u => ((u.MsPositionReFirstUD <= msPositionReFirstIUD) && ((u.MsPositionReFirstUD + u.MsDuration) > msPositionReFirstIUD)));
            }
            Debug.Assert(returnedIndex != -1);
            return returnedIndex;
        }
        #endregion list functions

        #region VoiceDef duration changers

        /// <summary>
        /// Removes all the rests in this VoiceDef
        /// </summary>
        public void RemoveRests()
        {
            AdjustMsDurations<RestDef>(0, _uniqueDefs.Count - 1, 0);
        }
        /// <summary>
        /// Multiplies the MsDuration of each chord and rest from beginIndex to endIndex (inclusive) by factor.
        /// If a chord or rest's MsDuration becomes less than minThreshold, it is removed.
        /// The total duration of this VoiceDef changes accordingly.
        /// </summary>
        public void AdjustMsDurations(int beginIndex, int endIndex, double factor, int minThreshold = 100)
        {
            AdjustMsDurations<DurationDef>(beginIndex, endIndex, factor, minThreshold);
        }
        /// <summary>
        /// Multiplies the MsDuration of each chord and rest in the UniqueDefs list by factor.
        /// If a chord or rest's MsDuration becomes less than minThreshold, it is removed.
        /// The total duration of this VoiceDef changes accordingly.
        /// </summary>
        public void AdjustMsDurations(double factor, int minThreshold = 100)
        {
            AdjustMsDurations<DurationDef>(0, _uniqueDefs.Count - 1, factor, minThreshold);
        }
        /// <summary>
        /// Multiplies the MsDuration of each rest from beginIndex to endIndex (inclusive) by factor.
        /// If a rest's MsDuration becomes less than minThreshold, it is removed.
        /// The total duration of this VoiceDef changes accordingly.
        /// </summary>
        public void AdjustRestMsDurations(int beginIndex, int endIndex, double factor, int minThreshold = 100)
        {
            AdjustMsDurations<RestDef>(beginIndex, endIndex, factor, minThreshold);
        }
        /// <summary>
        /// Multiplies the MsDuration of each rest in the UniqueDefs list by factor.
        /// If a rest's MsDuration becomes less than minThreshold, it is removed.
        /// The total duration of this VoiceDef changes accordingly.
        /// </summary>
        public void AdjustRestMsDurations(double factor, int minThreshold = 100)
        {
            AdjustMsDurations<RestDef>(0, _uniqueDefs.Count - 1, factor, minThreshold);
        }

        /// <summary>
        /// Multiplies the MsDuration of each T from beginIndex to endIndex (inclusive) by factor.
        /// If a MsDuration becomes less than minThreshold, the T (chord or rest) is removed.
        /// The total duration of this VoiceDef changes accordingly.
        /// </summary>
        protected void AdjustMsDurations<T>(int beginIndex, int endIndex, double factor, int minThreshold = 100)
        {
            Debug.Assert(!(Container is Block), "Cannot AdjustChordMsDurations inside a Block.");

            CheckIndices(beginIndex, endIndex);
            Debug.Assert(factor >= 0);

            for(int i = 0; i < _uniqueDefs.Count; ++i)
            {
                IUniqueDef iumdd = _uniqueDefs[i];
                if(i >= beginIndex && i <= endIndex && iumdd is T)
                {
                    iumdd.MsDuration = (int)((double)iumdd.MsDuration * factor);
                }
            }

            for(int i = _uniqueDefs.Count - 1; i >= 0; --i)
            {
                IUniqueDef iumdd = _uniqueDefs[i];
                if(iumdd.MsDuration < minThreshold)
                {
                    _uniqueDefs.RemoveAt(i);
                }
            }

            SetMsPositionsReFirstUD();

            AssertVoiceDefConsistency();
        }

        /// <summary>
        /// An object is a NonDurationDef if it is not a DurationDef.
        /// For example: a cautionaryChordDef or a clefChangeDef.
        /// </summary>
        private int GetNumberOfNonDurationDefs(int beginIndex, int endIndex)
        {
            int nNonDurationDefs = 0;
            for(int i = beginIndex; i < endIndex; ++i)
            {
                if(! (_uniqueDefs[i] is DurationDef))
                    nNonDurationDefs++;
            }
            return nNonDurationDefs;
        }

        /// <summary>
        /// Creates an exponential accelerando or decelerando from beginIndex to (not including) endIndex.
        /// This function changes the msDuration in the given index range.
        /// endIndex can be equal to this.Count.
        /// </summary>
        public void CreateAccel(int beginIndex, int endIndex, double startEndRatio)
        {
            Debug.Assert(((beginIndex + 1) < endIndex) && (startEndRatio >= 0) && (endIndex <= Count));

            int nNonDurationDefs = GetNumberOfNonDurationDefs(beginIndex, endIndex);

            double basicIncrement = (startEndRatio - 1) / (endIndex - beginIndex - nNonDurationDefs);
            double factor = 1.0;

            for(int i = beginIndex; i < endIndex; ++i)
            {
                _uniqueDefs[i].AdjustMsDuration(factor);
                factor += basicIncrement;
            }

            SetMsPositionsReFirstUD();

            AssertVoiceDefConsistency();
        }

        #endregion VoiceDef duration changers

        internal void RemoveDuplicateClefChanges()
        {
            if(_uniqueDefs.Count > 1)
            {
                for(int i = _uniqueDefs.Count - 1; i > 0; --i)
                {
                    IUniqueDef iud1 = _uniqueDefs[i];
                    if(iud1 is ClefChangeDef)
                    {
                        for(int j = i - 1; j >= 0; --j)
                        {
                            IUniqueDef iud2 = _uniqueDefs[j];
                            if(iud2 is ClefChangeDef)
                            {
                                if(string.Compare(((ClefChangeDef)iud1).ClefType, ((ClefChangeDef)iud2).ClefType) == 0) 
                                {
                                    _uniqueDefs.RemoveAt(i);
                                }
                                break;
                            }
                        }
                    }
                }
                AssertVoiceDefConsistency();
            }
        }

        /// <summary>
        /// Combines all consecutive rests.
        /// </summary>
        public void AgglomerateRests()
        {
            if(_uniqueDefs.Count > 1)
            {
                for(int i = _uniqueDefs.Count - 1; i > 0; --i)
                {
                    IUniqueDef lmdd2 = _uniqueDefs[i];
                    IUniqueDef lmdd1 = _uniqueDefs[i - 1];
                    if(lmdd2 is RestDef && lmdd1 is RestDef)
                    {
                        lmdd1.MsDuration += lmdd2.MsDuration;
                        _uniqueDefs.RemoveAt(i);
                    }
                }
            }

            AssertVoiceDefConsistency();
        }

        /// <summary>
        /// Removes the rest or chord at index, and extends the previous rest or chord
        /// by the removed duration, so that other msPositions don't change.
        /// </summary>
        /// <param name="p"></param>
        protected void AgglomerateRestOrChordAt(int index)
        {
            Debug.Assert(index > 0 && index < Count);
            _uniqueDefs[index - 1].MsDuration += _uniqueDefs[index].MsDuration;
            _uniqueDefs.RemoveAt(index);

            AssertVoiceDefConsistency();
        }

        #endregion Count changers

        #region Properties

        private int _midiChannel = int.MaxValue; // the MidiChannel will only be valid if set to a value in range [0..15]
        public int MidiChannel
        {
            get
            {
                return _midiChannel;
            }
            set
            {
                Debug.Assert(value >= 0 && value <= 15);
                _midiChannel = value;
            }
        }

        public IVoiceDefContainer Container = null;

        public int Count { get { return _uniqueDefs.Count; } }

        private int _msPositionReContainer = 0;
        /// <summary>
        /// The msPosition of the first note or rest in the UniqueDefs list re the start of the containing Seq or Block.
        /// The msPositions of the IUniqueDefs in the Trk are re the first IUniqueDef in the list, so the first IUniqueDef.MsPositionReFirstUID is always 0;
        /// </summary>
        public virtual int MsPositionReContainer
        {
            get
            {
                return _msPositionReContainer;
            }
            set
            {
                _msPositionReContainer = value;
            }
        }

        /// <summary>
        /// Setting this property stretches or compresses all the durations in the UniqueDefs list to fit the given total duration.
        /// This does not change the VoiceDef's MsPosition, but does affect its EndMsPosition.
        /// See also EndMsPosition.set.
        /// </summary>
        public int MsDuration
        {
            get
            {
                int total = 0;
                foreach(IUniqueDef iud in _uniqueDefs)
                {
                    total += iud.MsDuration;
                }
                return total;
            }
            set
            {
                Debug.Assert(value > 0);

                int msDuration = value;

                List<int> relativeDurations = new List<int>();
                foreach(IUniqueDef iumdd in _uniqueDefs)
                {
                    if(iumdd.MsDuration > 0)
                        relativeDurations.Add(iumdd.MsDuration);
                }

                List<int> newDurations = M.IntDivisionSizes(msDuration, relativeDurations);

                Debug.Assert(newDurations.Count == relativeDurations.Count);
                int i = 0;
                int newTotal = 0;
                foreach(IUniqueDef iumdd in _uniqueDefs)
                {
                    if(iumdd.MsDuration > 0)
                    {
                        iumdd.MsDuration = newDurations[i];
                        newTotal += iumdd.MsDuration;
                        ++i;
                    }
                }

                Debug.Assert(msDuration == newTotal);

                SetMsPositionsReFirstUD();

                AssertVoiceDefConsistency();
            }
        }

        /// <summary>
        /// The position of the end of the last UniqueDef in the list re the first IUniqueDef in the list, or 0 if the list is empty.
        /// Setting this value can only be done if the UniqueDefs list is not empty, and the value
        /// is greater than the position of the final UniqueDef in the list. It then changes
        /// the msDuration of the final IUniqueDef.
        /// See also MsDuration.set.
        /// </summary>
        public int EndMsPositionReFirstIUD
        {
            get
            {
                int endMsPosReFirstUID = 0;
                if(_uniqueDefs.Count > 0)
                {
                    IUniqueDef lastIUD = _uniqueDefs[_uniqueDefs.Count - 1];
                    endMsPosReFirstUID += (lastIUD.MsPositionReFirstUD + lastIUD.MsDuration);
                }
                return endMsPosReFirstUID;
            }
            set
            {
                Debug.Assert(_uniqueDefs.Count > 0);
                Debug.Assert(value > EndMsPositionReFirstIUD);

                IUniqueDef lastLmdd = _uniqueDefs[_uniqueDefs.Count - 1];
                lastLmdd.MsDuration = value - EndMsPositionReFirstIUD;

                AssertVoiceDefConsistency();
            }
        }

        public List<IUniqueDef> UniqueDefs { get { return _uniqueDefs; } }
        protected List<IUniqueDef> _uniqueDefs = new List<IUniqueDef>();
 
        #endregion Properties
    }
}
