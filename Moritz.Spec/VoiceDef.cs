
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
        public VoiceDef()
        {
        }

        /// <summary>
        /// A VoiceDef beginning at MsPosition = 0, and containing a single UniqueMidiRestDef having msDuration
        /// </summary>
        /// <param name="msDuration"></param>
        public VoiceDef(int msDuration)
        {
            RestDef lmRestDef = new RestDef(0, msDuration);
            _uniqueDefs.Add(lmRestDef);
        }

        /// <summary>
        /// <para>If the argument is not empty, the MsPositions and MsDurations in the list are checked for consistency.</para>
        /// <para>The new VoiceDef's _uniqueDefs list is simply set to the argument (which is not cloned).</para>
        /// </summary>
        public VoiceDef(List<IUniqueDef> iuds)
        {
            Debug.Assert(iuds != null);
            if(iuds.Count > 0)
            {
                for(int i = 1; i < iuds.Count; ++i)
                {
                    Debug.Assert(iuds[i - 1].MsPosition + iuds[i - 1].MsDuration == iuds[i].MsPosition);
                }
            }
            _uniqueDefs = iuds;
        }

        #endregion constructors

        #region public indexer & enumerator
        /// <summary>
        /// Indexer. Allows individual lmdds to be accessed using array notation on the VoiceDef.
        /// e.g. iumdd = voiceDef[3].
        /// </summary>
        public IUniqueDef this[int i]
        {
            get
            {
                if(i < 0 || i >= _uniqueDefs.Count)
                {
                    throw new IndexOutOfRangeException();
                }
                return _uniqueDefs[i];
            }
            set
            {
                if(i < 0 || i >= _uniqueDefs.Count)
                {
                    throw new IndexOutOfRangeException();
                }
                _uniqueDefs[i] = value;
                SetMsPositions();
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

        #region public

        #region Count changers
        #region list functions
        public abstract void Add(IUniqueDef iUniqueDef);
        protected void _Add(IUniqueDef iUniqueDef)
        {
            Debug.Assert(_uniqueDefs.Count > 0);
            IUniqueDef lastIud = _uniqueDefs[_uniqueDefs.Count - 1];
            iUniqueDef.MsPosition = lastIud.MsPosition + lastIud.MsDuration;
            _uniqueDefs.Add(iUniqueDef);
        }
        protected void _AddRange(VoiceDef voiceDef)
        {
            _uniqueDefs.AddRange(voiceDef.UniqueDefs);
            SetMsPositions();
        }
        public abstract void Insert(int index, IUniqueDef iUniqueDef);
        protected void _Insert(int index, IUniqueDef iUniqueDef)
        {
            _uniqueDefs.Insert(index, iUniqueDef);
            SetMsPositions();
        }
        protected void _InsertRange(int index, VoiceDef voiceDef)
        {
            _uniqueDefs.InsertRange(index, voiceDef.UniqueDefs);
            SetMsPositions();
        }
        protected void _InsertInRest(VoiceDef iVoiceDef)
        {
            int iLmddsStartMsPos = iVoiceDef[0].MsPosition;
            int iLmddsEndMsPos = iVoiceDef[iVoiceDef.Count - 1].MsPosition + iVoiceDef[iVoiceDef.Count - 1].MsDuration;

            int restIndex = FindIndexOfSpanningRest(iLmddsStartMsPos, iLmddsEndMsPos);

            // if index >= 0, it is the index of a rest into which the chord will fit.
            if(restIndex >= 0)
            {
                InsertVoiceDefInRest(restIndex, iVoiceDef);
                SetMsPositions(); // just to be sure!
            }
            else
            {
                Debug.Assert(false, "Can't find a rest spanning the chord!");
            }
        }
        #region _InsertInRest() implementation
        /// <summary>
        /// Returns the index of a rest spanning startMsPos..endMsPos
        /// If there is no such rest, -1 is returned.
        /// </summary>
        /// <param name="startMsPos"></param>
        /// <param name="endMsPos"></param>
        /// <returns></returns>
        private int FindIndexOfSpanningRest(int startMsPos, int endMsPos)
        {
            List<IUniqueDef> lmdds = _uniqueDefs;
            int index = -1, restStartMsPos = -1, restEndMsPos = -1;

            for(int i = 0; i < lmdds.Count; ++i)
            {
                RestDef umrd = lmdds[i] as RestDef;
                if(umrd != null)
                {
                    restStartMsPos = lmdds[i].MsPosition;
                    restEndMsPos = lmdds[i].MsPosition + lmdds[i].MsDuration;

                    if(startMsPos >= restStartMsPos && endMsPos <= restEndMsPos)
                    {
                        index = i;
                        break;
                    }
                    if(startMsPos < restStartMsPos)
                    {
                        break;
                    }
                }
            }

            return index;
        }

        private void InsertVoiceDefInRest(int restIndex, VoiceDef iVoiceDef)
        {
            List<IUniqueDef> lmdds = _uniqueDefs;
            IUniqueDef rest = lmdds[restIndex];
            List<IUniqueDef> replacement = GetReplacementList(rest, iVoiceDef);
            int replacementStart = replacement[0].MsPosition;
            int replacementEnd = replacement[replacement.Count - 1].MsPosition + replacement[replacement.Count - 1].MsDuration;
            int restStart = rest.MsPosition;
            int restEnd = rest.MsPosition + rest.MsDuration;
            Debug.Assert(restStart == replacementStart && restEnd == replacementEnd);
            lmdds.RemoveAt(restIndex);
            lmdds.InsertRange(restIndex, replacement);
        }
        /// <summary>
        /// Returns a list having the position and duration of the originalRest.
        /// The iLmdds have been put in(side) the original rest, either at the beginning, middle, or end. 
        /// </summary>
        private List<IUniqueDef> GetReplacementList(IUniqueDef originalRest, VoiceDef iVoiceDef)
        {
            Debug.Assert(originalRest is RestDef);
            Debug.Assert(iVoiceDef[0] is MidiChordDef || iVoiceDef[0] is InputChordDef);
            Debug.Assert(iVoiceDef[iVoiceDef.Count - 1] is MidiChordDef || iVoiceDef[iVoiceDef.Count - 1] is InputChordDef);

            List<IUniqueDef> rList = new List<IUniqueDef>();
            if(iVoiceDef[0].MsPosition > originalRest.MsPosition)
            {
                RestDef rest1 = new RestDef(originalRest.MsPosition, iVoiceDef[0].MsPosition - originalRest.MsPosition);
                rList.Add(rest1);
            }
            rList.AddRange(iVoiceDef.UniqueDefs);
            int iLmddsEndMsPos = iVoiceDef[iVoiceDef.Count - 1].MsPosition + iVoiceDef[iVoiceDef.Count - 1].MsDuration;
            int originalRestEndMsPos = originalRest.MsPosition + originalRest.MsDuration;
            if(originalRestEndMsPos > iLmddsEndMsPos)
            {
                RestDef rest2 = new RestDef(iLmddsEndMsPos, originalRestEndMsPos - iLmddsEndMsPos);
                rList.Add(rest2);
            }

            return rList;
        }
        #endregion InsertInRest() implementation
        /// <summary>
        /// removes the iUniqueDef from the list, and then resets the positions of all the iUniques in the list.
        /// </summary>
        public void Remove(IUniqueDef iUniqueDef)
        {
            Debug.Assert(_uniqueDefs.Count > 0);
            Debug.Assert(_uniqueDefs.Contains(iUniqueDef));
            _uniqueDefs.Remove(iUniqueDef);
            SetMsPositions();
        }
        /// <summary>
        /// Removes the iUniqueMidiDurationDef at index from the list, and then resets the positions of all the lmdds in the list.
        /// </summary>
        public void RemoveAt(int index)
        {
            Debug.Assert(index >= 0 && index < _uniqueDefs.Count);
            _uniqueDefs.RemoveAt(index);
            SetMsPositions();
        }
        /// <summary>
        /// Removes count iUniqueDefs from the list, start�ng with the iUniqueDef at index.
        /// </summary>
        public void RemoveRange(int index, int count)
        {
            Debug.Assert(index >= 0 && count >= 0 && ((index + count) <= _uniqueDefs.Count));
            _uniqueDefs.RemoveRange(index, count);
            SetMsPositions();
        }
        /// <summary>
        /// Remove the IUniques which start between startMsPos and (not including) endMsPos 
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        public void RemoveBetweenMsPositions(int startMsPos, int endMsPos)
        {
            IUniqueDef iumdd = _uniqueDefs.Find(f => (f.MsPosition >= startMsPos) && (f.MsPosition < endMsPos));
            while(iumdd != null)
            {
                _uniqueDefs.Remove(iumdd);
                iumdd = _uniqueDefs.Find(f => (f.MsPosition >= startMsPos) && (f.MsPosition < endMsPos));
            }
            SetMsPositions();
        }
        /// <summary>
        /// Removes the iUniqueMidiDurationDef at index from the list, and then inserts the replacement at the same index.
        /// </summary>
        protected void _Replace(int index, IUniqueDef replacementIUnique)
        {
            Debug.Assert(index >= 0 && index < _uniqueDefs.Count);
            _uniqueDefs.RemoveAt(index);
            _uniqueDefs.Insert(index, replacementIUnique);
            SetMsPositions();
        }
        /// <summary>
        /// From startMsPosition to (not including) endMsPosition,
        /// replace all MidiChordDefs or InputChordDefs by UniqueMidiRestDefs, then aglommerate the rests.
        /// </summary>
        public void Erase(int startMsPosition, int endMsPosition)
        {
            int beginIndex = FindIndexAtMsPosition(startMsPosition);
            int endIndex = FindIndexAtMsPosition(endMsPosition);

            for(int i = beginIndex; i < endIndex; ++i)
            {
                MidiChordDef mcd = this[i] as MidiChordDef;
                InputChordDef icd = this[i] as InputChordDef;
                IUniqueDef iud = (mcd == null) ? (IUniqueDef)icd : (IUniqueDef)mcd;
                if(iud != null)
                {
                    RestDef umrd = new RestDef(iud.MsPosition, iud.MsDuration);
                    RemoveAt(i);
                    Insert(i, umrd);
                }
            }

            AgglomerateRests();
        }
        /// <summary>
        /// Extracts nUniqueDefs from the uniqueDefs, and then inserts them again at the toIndex.
        /// </summary>
        public void Translate(int fromIndex, int nUniqueDefs, int toIndex)
        {
            Debug.Assert((fromIndex + nUniqueDefs) <= _uniqueDefs.Count);
            Debug.Assert(toIndex <= (_uniqueDefs.Count - nUniqueDefs));
            int msPosition = _uniqueDefs[0].MsPosition;
            List<IUniqueDef> extractedLmdds = _uniqueDefs.GetRange(fromIndex, nUniqueDefs);
            _uniqueDefs.RemoveRange(fromIndex, nUniqueDefs);
            _uniqueDefs.InsertRange(toIndex, extractedLmdds);
            SetMsPositions();
        }
        /// <summary>
        /// Returns the index of the IUniqueDef which starts at or is otherwise current at msPosition.
        /// If msPosition is the EndMsPosition, the index of the final IUniqueMidiDurationDef (Count-1) is returned.
        /// If the VoiceDef does not span msPosition, -1 (=error) is returned.
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public int FindIndexAtMsPosition(int msPosition)
        {
            int returnedIndex = -1;
            if(msPosition == EndMsPosition)
            {
                returnedIndex = this.Count - 1;
            }
            else if(msPosition >= _uniqueDefs[0].MsPosition && msPosition < EndMsPosition)
            {
                returnedIndex = _uniqueDefs.FindIndex(u => ((u.MsPosition <= msPosition) && ((u.MsPosition + u.MsDuration) > msPosition)));
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
            AdjustMsDurations<RestDef>(0, _uniqueDefs.Count, 0);
        }
        /// <summary>
        /// Multiplies the MsDuration of each chord and rest from beginIndex to (not including) endIndex by factor.
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
            AdjustMsDurations<DurationDef>(0, _uniqueDefs.Count, factor, minThreshold);
        }
        /// <summary>
        /// Multiplies the MsDuration of each rest from beginIndex to (not including) endIndex by factor.
        /// If a rest's MsDuration becomes less than minThreshold, it is removed.
        /// The total duration of this VoiceDef changes accordingly.
        /// </summary>
        public void AdjustRestMsDurations(int beginIndex, int endIndex, double factor, int minThreshold = 100)
        {
            AdjustMsDurations<RestDef>(beginIndex, endIndex, factor, minThreshold);
        }
        /// <summary>
        /// Multiplies the MsDuration of each rest in the UniqueMidiDurationDefs list by factor.
        /// If a rest's MsDuration becomes less than minThreshold, it is removed.
        /// The total duration of this VoiceDef changes accordingly.
        /// </summary>
        public void AdjustRestMsDurations(double factor, int minThreshold = 100)
        {
            AdjustMsDurations<RestDef>(0, _uniqueDefs.Count, factor, minThreshold);
        }
        
        /// <summary>
        /// Multiplies the MsDuration of each T from beginIndex to (not including) endIndex by factor.
        /// If a MsDuration becomes less than minThreshold, the T (chord or rest) is removed.
        /// The total duration of this VoiceDef changes accordingly.
        /// </summary>
        protected void AdjustMsDurations<T>(int beginIndex, int endIndex, double factor, int minThreshold = 100)
        {
            CheckIndices(beginIndex, endIndex);
            Debug.Assert(factor >= 0);

            for(int i = 0; i < _uniqueDefs.Count; ++i)
            {
                IUniqueDef iumdd = _uniqueDefs[i];
                if(i >= beginIndex && i < endIndex && iumdd is T)
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

            SetMsPositions();
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

            SetMsPositions();
        }

        #endregion VoiceDef duration changers

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
        }

        #endregion Count changers

        #region public properties
        /// <summary>
        /// The absolute position of the first note or rest in the sequence.
        /// Setting this value resets all the MsPositions in this VoiceDef,
        /// including the EndMsPosition.
        /// </summary>
        public int MsPosition 
        { 
            get 
            { 
                Debug.Assert(_uniqueDefs.Count > 0);
                return _uniqueDefs[0].MsPosition;
            }
            set
            {
                Debug.Assert(_uniqueDefs.Count > 0);
                _uniqueDefs[0].MsPosition = value;
                SetMsPositions();
            } 
        }
        /// <summary>
        /// The absolute position of the end of the last note or rest in the sequence.
        /// Setting this value changes the msDuration of the final IUniqueMidiDurationDef.
        /// (The EndMsPosition cannot be set if this VoiceDef is empty, or before the last IUniqueMidiDurationDef.) 
        /// </summary>
        public int EndMsPosition 
        { 
            get 
            {
                int endPosition = 0;
                if(_uniqueDefs.Count > 0)
                {
                    IUniqueDef lastLmdd = _uniqueDefs[_uniqueDefs.Count - 1];
                    endPosition = lastLmdd.MsPosition + lastLmdd.MsDuration;
                }
                return endPosition;
            }
            set
            {
                Debug.Assert(_uniqueDefs.Count > 0);
                IUniqueDef lastLmdd = _uniqueDefs[_uniqueDefs.Count - 1];
                Debug.Assert(value > lastLmdd.MsPosition);
                lastLmdd.MsDuration = value - lastLmdd.MsPosition;
            }
        }
        public int Count { get { return _uniqueDefs.Count; } }

        #endregion public properties

        #region public attribute changers (Transpose etc.)

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
            for(int i = beginIndex; i < endIndex; ++i)
            {
                if(!(_uniqueDefs[i] is MidiChordDef) && !(_uniqueDefs[i] is InputChordDef))
                    nNonMidiChordDefs++;
            }
            return nNonMidiChordDefs;
        }

        /// <summary>
        /// Transpose all the IUniqueChordDefs from beginIndex to (not including) endIndex
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
            for(int i = beginIndex; i < endIndex; ++i)
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

            int currentMsPos = dictPositions[0];
            int j = 0;
            for(int i = 1; i < msPosTranspositionDict.Count; ++i)
            {
                int transposition = msPosTranspositionDict[currentMsPos];
                int nextMsPos = dictPositions[i];
                while(j < _uniqueDefs.Count && _uniqueDefs[j].MsPosition < nextMsPos)
                {
                    if(_uniqueDefs[j].MsPosition >= currentMsPos)
                    {
                        IUniqueChordDef iucd = _uniqueDefs[j] as IUniqueChordDef;
                        if(iucd != null)
                        {
                            iucd.Transpose(transposition);
                        }
                    }
                    ++j;
                }
                currentMsPos = nextMsPos;
            }
        }

        /// <summary>
        /// Transposes the UniqueDefs from the beginIndex upto (but not including) endIndex
        /// by an equally increasing amount, so that the final MidiChordDef or InputChordDef is transposed by glissInterval.
        /// beginIndex must be less than endIndex.
        /// glissInterval can be negative.
        /// </summary>
        public void StepwiseGliss(int beginIndex, int endIndex, int glissInterval)
        {
            CheckIndices(beginIndex, endIndex);
            Debug.Assert(beginIndex < endIndex);

            int nNonMidiChordDefs = GetNumberOfNonMidiOrInputChordDefs(beginIndex, endIndex);

            int nSteps = (endIndex - beginIndex - nNonMidiChordDefs);
            double interval = ((double)glissInterval) / nSteps;
            double step = interval;
            for(int i = beginIndex; i < endIndex; ++i)
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

        #endregion  public attribute changers (Transpose etc.)

        #region  public SetLyricsToIndex()

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
        #endregion public SetLyricsToIndex()

		/// <summary>
		/// For an example of using this function, see SongSixAlgorithm.cs
		/// Note that clef changes must be inserted backwards per voiceDef, so that IUniqueDef indices are correct. 
		/// Inserting a clef change changes the subsequent indices.
		/// Note also that if a ClefChange is defined here on a UniqueMidiRestDef which has no MidiChordDef
		/// to its right on the staff, the resulting ClefSymbol will be placed immediately before the final barline
		/// on the staff.
		/// ClefChanges which would happen at the beginning of a staff are moved to the end of the equivalent staff
		/// in the previous system.
		/// A ClefChange defined here on a MidiChordDef or UniqueMidiRestDef which is eventually preceded
		/// by a barline, are placed to the left of the barline.  
		/// The clefType must be one of the following strings "t", "t1", "t2", "t3", "b", "b1", "b2", "b3"
		/// </summary>
		public void InsertClefChange(int index, string clefType)
		{
			#region check args
			Debug.Assert(index < _uniqueDefs.Count);
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

			ClefChangeDef clefChangeDef = new ClefChangeDef(clefType, _uniqueDefs[index]);
			_uniqueDefs.Insert(index, clefChangeDef);
		}

        public List<IUniqueDef> UniqueDefs { get { return _uniqueDefs; } }

		/// <summary>
		/// Setting this property stretches or compresses all the durations in the UniqueDefs list to fit the given total duration.
		/// This does not change the VoiceDef's MsPosition, but does affect its EndMsPosition.
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

				List<int> newDurations = MidiChordDef.GetIntDurations(msDuration, relativeDurations, relativeDurations.Count);

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

				SetMsPositions();
			}
		}

		public byte MidiChannel = byte.MaxValue; // the MidiChannel will only be valid if set to a value in range [0..15]

        #endregion public

        #region protected
        /// <summary>
        /// Sets the MsPosition attribute of each IUniqueDef in the _uniqueDefs list.
        /// Uses all the MsDuration attributes, and the MsPosition of the first IUniqueDef as origin.
        /// This function must be called at the end of any function that changes the _uniqueDefs list.
        /// </summary>
        protected void SetMsPositions()
        {
            if(_uniqueDefs.Count > 0)
            {
                int currentPosition = _uniqueDefs[0].MsPosition;
                Debug.Assert(currentPosition >= 0);
                foreach(IUniqueDef umcd in _uniqueDefs)
                {
                    umcd.MsPosition = currentPosition;
                    currentPosition += umcd.MsDuration;
                }
            }
        }

        protected void CheckIndices(int beginIndex, int endIndex)
        {
            Debug.Assert(beginIndex >= 0 && beginIndex < _uniqueDefs.Count);
            Debug.Assert(endIndex >= 0 && endIndex <= _uniqueDefs.Count);
        }

		protected List<IUniqueDef> _uniqueDefs = new List<IUniqueDef>();
        #endregion protected
    }
}
