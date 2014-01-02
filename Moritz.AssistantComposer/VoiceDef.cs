
using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections;
using Moritz.Score.Midi;
using Krystals4ObjectLibrary;

namespace Moritz.AssistantComposer
{
    /// <summary>
    /// A temporal sequence of IUniqueMidiDurationDef objects.
    /// <para>The objects can define either notes or rests.</para>
    /// <para>(IUniqueMidiDurationDef is either a UniqueMidiChordDef or a UniqueMidiRestDef.)</para>
    /// <para></para>
    /// <para>This class is IEnumerable, so that foreach loops can be used.</para>
    /// <para>For example:</para>
    /// <para>foreach(IUniqueMidiDurationDef iumdd in voiceDef) { ... }</para>
    /// <para>It is also indexable, as in:</para>
    /// <para>IUniqueMidiDurationDef iumdd = voiceDef[index];</para>
    /// </summary>
    public class VoiceDef : IEnumerable
    {
        #region constructors
        /// <summary>
        /// A VoiceDef beginning at MsPosition = 0, and containing a single UniqueMidiRestDef having msDuration
        /// </summary>
        /// <param name="msDuration"></param>
        public VoiceDef(int msDuration)
        {
            IUniqueMidiDurationDef lmRestDef = new UniqueMidiRestDef(0, msDuration);
            _uniqueMidiDurationDefs.Add(lmRestDef);
        }

        /// <summary>
        /// <para>If the argument is not empty, the MsPositions and MsDurations in the list are checked for consistency.</para>
        /// <para>The new VoiceDef's UniqueMidiDurationDefs list is simply set to the argument (which is not cloned).</para>
        /// </summary>
        public VoiceDef(List<IUniqueMidiDurationDef> lmdds)
        {
            Debug.Assert(lmdds != null);
            if(lmdds.Count > 0)
            {
                for(int i = 1; i < lmdds.Count; ++i)
                {
                    Debug.Assert(lmdds[i - 1].MsPosition + lmdds[i - 1].MsDuration == lmdds[i].MsPosition);
                }
            }
            _uniqueMidiDurationDefs = lmdds;
        }

        /// <summary>
        /// sequence contains a list of values in range 1..paletteDef.MidiDurationDefsCount.
        /// </summary>
        protected VoiceDef(PaletteDef paletteDef, List<int> sequence)
        {
            int msPosition = 0;
            foreach(int value in sequence)
            {
                Debug.Assert((value >= 1 && value <= paletteDef.MidiDurationDefsCount), "Illegal argument: value out of range in sequence");
                IUniqueMidiDurationDef iumdd = paletteDef[value - 1].CreateUniqueMidiDurationDef();
                Debug.Assert(iumdd.MsDuration > 0);
                iumdd.MsPosition = msPosition;
                msPosition += iumdd.MsDuration;
                _uniqueMidiDurationDefs.Add(iumdd);
            }
        }
 
        /// <summary>
        /// Creates a VoiceDef using the flat sequence of values in the krystal.
        /// </summary>
        public VoiceDef (PaletteDef paletteDef, Krystal krystal)
            : this(paletteDef,krystal.GetValues((uint)1)[0])
        {
        }

        /// <summary>
        /// Constructs a VoiceDef at MsPosition=0, containing the localized sequence of MidiDurationDefs in the PaletteDef.
        /// </summary
        public VoiceDef(PaletteDef midiDurationDefs)
        {
            Debug.Assert(midiDurationDefs != null);
            foreach(MidiDurationDef midiDurationDef in midiDurationDefs)
            {
                IUniqueMidiDurationDef iumdd = midiDurationDef.CreateUniqueMidiDurationDef();
                _uniqueMidiDurationDefs.Add(iumdd);
            }
            SetMsPositions();
            //MsPosition = _uniqueMidiDurationDefs[0].MsPosition; // sets the absolute position of all notes and rests
        }

        /// <summary>
        /// Returns a deep clone of this VoiceDef.
        /// </summary>
        public VoiceDef Clone()
        {
            List<IUniqueMidiDurationDef> clonedLmdds = new List<IUniqueMidiDurationDef>();
            foreach(IUniqueMidiDurationDef iumdd in this._uniqueMidiDurationDefs)
            {
                MidiDurationDef mdd = iumdd as MidiDurationDef;
                IUniqueMidiDurationDef clonedLmdd = mdd.CreateUniqueMidiDurationDef();
                clonedLmdd.MsPosition = iumdd.MsPosition;
                clonedLmdds.Add(clonedLmdd);
            }

            return new VoiceDef(clonedLmdds);
        }
        #endregion constructors

        #region public indexer & enumerator
        /// <summary>
        /// Indexer. Allows individual lmdds to be accessed using array notation on the VoiceDef.
        /// e.g. iumdd = voiceDef[3].
        /// </summary>
        public IUniqueMidiDurationDef this[int i]
        {
            get
            {
                if(i < 0 || i >= _uniqueMidiDurationDefs.Count)
                {
                    throw new IndexOutOfRangeException();
                }
                return _uniqueMidiDurationDefs[i];
            }
            set
            {
                if(i < 0 || i >= _uniqueMidiDurationDefs.Count)
                {
                    throw new IndexOutOfRangeException();
                }
                _uniqueMidiDurationDefs[i] = value;
            }
        }

        #region Enumerator
        public IEnumerator GetEnumerator()
        {
            return new MyEnumerator(_uniqueMidiDurationDefs);
        }
        // private enumerator class
        // see http://support.microsoft.com/kb/322022/en-us
        private class MyEnumerator : IEnumerator
        {
            public List<IUniqueMidiDurationDef> _localizedMidiDurationDefs;
            int position = -1;
            //constructor
            public MyEnumerator(List<IUniqueMidiDurationDef> localizedMidiDurationDefs)
            {
                _localizedMidiDurationDefs = localizedMidiDurationDefs;
            }
            private IEnumerator getEnumerator()
            {
                return (IEnumerator)this;
            }
            //IEnumerator
            public bool MoveNext()
            {
                position++;
                return (position < _localizedMidiDurationDefs.Count);
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
                        return _localizedMidiDurationDefs[position];
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

        #region internal

        #region internal Count changers
        #region list functions
        /// <summary>
        /// Appends the new iUniqueMidiDurationDef to the end of the list.
        /// </summary>
        /// <param name="iUniqueMidiDurationDef"></param>
        internal void Add(IUniqueMidiDurationDef iUniqueMidiDurationDef)
        {
            Debug.Assert(_uniqueMidiDurationDefs.Count > 0);
            IUniqueMidiDurationDef lastLmdd = _uniqueMidiDurationDefs[_uniqueMidiDurationDefs.Count - 1];
            iUniqueMidiDurationDef.MsPosition = lastLmdd.MsPosition + lastLmdd.MsDuration;
            _uniqueMidiDurationDefs.Add(iUniqueMidiDurationDef);
        }
        /// <summary>
        /// Adds the argument to the end of this VoiceDef.
        /// Sets the MsPositions of the appended UniqueMidiDurationDefs.
        /// </summary>
        internal void AddRange(VoiceDef voiceDef)
        {
            _uniqueMidiDurationDefs.AddRange(voiceDef.UniqueMidiDurationDefs);
            SetMsPositions();
        }
        /// <summary>
        /// Inserts the iUniqueMidiDurationDef in the list at the given index, and then
        /// resets the positions of all the lmdds in the list.
        /// </summary>
        /// <param name="iUniqueMidiDurationDef"></param>
        internal void Insert(int index, IUniqueMidiDurationDef iUniqueMidiDurationDef)
        {
            _uniqueMidiDurationDefs.Insert(index, iUniqueMidiDurationDef);
            SetMsPositions();
        }
        /// <summary>
        /// Inserts the voiceDef in the list at the given index, and then
        /// resets the positions of all the lmdds in the list.
        /// </summary>
        internal void InsertRange(int index, VoiceDef voiceDef)
        {
            _uniqueMidiDurationDefs.InsertRange(index, voiceDef.UniqueMidiDurationDefs);
            SetMsPositions();
        }
        /// <summary>
        /// Creates a new VoiceDef containing just the argument chord,
        /// then calls the other InsertInRest() function with the voiceDef as argument. 
        /// </summary>
        internal void InsertInRest(IUniqueMidiDurationDef chord)
        {
            Debug.Assert(chord is UniqueMidiChordDef);
            List<IUniqueMidiDurationDef> iLmdds = new List<IUniqueMidiDurationDef>() { chord };
            VoiceDef iVoiceDef = new VoiceDef(iLmdds);
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
        internal void InsertInRest(VoiceDef iVoiceDef)
        {
            Debug.Assert(iVoiceDef[0] is UniqueMidiChordDef
                && iVoiceDef[iVoiceDef.Count - 1] is UniqueMidiChordDef);

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
        #region InsertInRest() implementation
        /// <summary>
        /// Returns the index of a rest spanning startMsPos..endMsPos
        /// If there is no such rest, -1 is returned.
        /// </summary>
        /// <param name="startMsPos"></param>
        /// <param name="endMsPos"></param>
        /// <returns></returns>
        private int FindIndexOfSpanningRest(int startMsPos, int endMsPos)
        {
            List<IUniqueMidiDurationDef> lmdds = _uniqueMidiDurationDefs;
            int index = -1, restStartMsPos = -1, restEndMsPos = -1;

            for(int i = 0; i < lmdds.Count; ++i)
            {
                UniqueMidiRestDef umrd = lmdds[i] as UniqueMidiRestDef;
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
            List<IUniqueMidiDurationDef> lmdds = _uniqueMidiDurationDefs;
            IUniqueMidiDurationDef rest = lmdds[restIndex];
            List<IUniqueMidiDurationDef> replacement = GetReplacementList(rest, iVoiceDef);
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
        private List<IUniqueMidiDurationDef> GetReplacementList(IUniqueMidiDurationDef originalRest, VoiceDef iVoiceDef)
        {
            Debug.Assert(originalRest is UniqueMidiRestDef);
            Debug.Assert(iVoiceDef[0] is UniqueMidiChordDef);
            Debug.Assert(iVoiceDef[iVoiceDef.Count - 1] is UniqueMidiChordDef);

            List<IUniqueMidiDurationDef> rList = new List<IUniqueMidiDurationDef>();
            if(iVoiceDef[0].MsPosition > originalRest.MsPosition)
            {
                IUniqueMidiDurationDef rest1 = new UniqueMidiRestDef(originalRest.MsPosition, iVoiceDef[0].MsPosition - originalRest.MsPosition);
                rList.Add(rest1);
            }
            rList.AddRange(iVoiceDef.UniqueMidiDurationDefs);
            int iLmddsEndMsPos = iVoiceDef[iVoiceDef.Count - 1].MsPosition + iVoiceDef[iVoiceDef.Count - 1].MsDuration;
            int originalRestEndMsPos = originalRest.MsPosition + originalRest.MsDuration;
            if(originalRestEndMsPos > iLmddsEndMsPos)
            {
                IUniqueMidiDurationDef rest2 = new UniqueMidiRestDef(iLmddsEndMsPos, originalRestEndMsPos - iLmddsEndMsPos);
                rList.Add(rest2);
            }

            return rList;
        }
        #endregion InsertInRest() implementation
        /// <summary>
        /// removes the iUniqueMidiDurationDef from the list, and then resets the positions of all the lmdds in the list.
        /// </summary>
        /// <param name="iUniqueMidiDurationDef"></param>
        internal void Remove(IUniqueMidiDurationDef iUniqueMidiDurationDef)
        {
            Debug.Assert(_uniqueMidiDurationDefs.Count > 0);
            Debug.Assert(_uniqueMidiDurationDefs.Contains(iUniqueMidiDurationDef));
            _uniqueMidiDurationDefs.Remove(iUniqueMidiDurationDef);
            SetMsPositions();
        }
        /// <summary>
        /// Removes the iUniqueMidiDurationDef at index from the list, and then resets the positions of all the lmdds in the list.
        /// </summary>
        internal void RemoveAt(int index)
        {
            Debug.Assert(index >= 0 && index < _uniqueMidiDurationDefs.Count);
            _uniqueMidiDurationDefs.RemoveAt(index);
            SetMsPositions();
        }
        /// <summary>
        /// Removes count iUniqueMidiDurationDefs from the list, startíng with the iUniqueMidiDurationDef at index.
        /// </summary>
        internal void RemoveRange(int index, int count)
        {
            Debug.Assert(index >= 0 && count >= 0 && ((index + count) <= _uniqueMidiDurationDefs.Count));
            _uniqueMidiDurationDefs.RemoveRange(index, count);
            SetMsPositions();
        }
        /// <summary>
        /// Remove the IUniqueMidiDurationDefs which start between startMsPos and (not including) endMsPos 
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        internal void RemoveBetweenMsPositions(int startMsPos, int endMsPos)
        {
            IUniqueMidiDurationDef iumdd = _uniqueMidiDurationDefs.Find(f => (f.MsPosition >= startMsPos) && (f.MsPosition < endMsPos));
            while(iumdd != null)
            {
                _uniqueMidiDurationDefs.Remove(iumdd);
                iumdd = _uniqueMidiDurationDefs.Find(f => (f.MsPosition >= startMsPos) && (f.MsPosition < endMsPos));
            }
            SetMsPositions();
        }
        /// <summary>
        /// Removes the iUniqueMidiDurationDef at index from the list, and then inserts the replacement at the same index.
        /// </summary>
        internal void Replace(int index, IUniqueMidiDurationDef replacementIumdd)
        {
            Debug.Assert(index >= 0 && index < _uniqueMidiDurationDefs.Count);
            _uniqueMidiDurationDefs.RemoveAt(index);
            _uniqueMidiDurationDefs.Insert(index, replacementIumdd);
            SetMsPositions();
        }

        /// <summary>
        /// From startMsPosition to (not including) endMsPosition,
        /// replace all UniqueMidiChordDefs by UniqueMidiRestDefs, then aglommerate the rests.
        /// </summary>
        internal void Erase(int startMsPosition, int endMsPosition)
        {
            int startIndex = FindIndexAtMsPosition(startMsPosition);
            int endIndex = FindIndexAtMsPosition(endMsPosition);

            for(int i = startIndex; i < endIndex; ++i)
            {
                UniqueMidiChordDef umcd = this[i] as UniqueMidiChordDef;
                if(umcd != null)
                {
                    UniqueMidiRestDef umrd = new UniqueMidiRestDef(umcd.MsPosition, umcd.MsDuration);
                    RemoveAt(i);
                    Insert(i, umrd);
                }
            }

            AgglomerateRests();
        }
        /// <summary>
        /// Extracts nUniqueMidiDurationDefs from the UniqueMidiDurationDefs, and then inserts them again at the toIndex.
        /// </summary>
        internal void Translate(int fromIndex, int nUniqueMidiDurationDefs, int toIndex)
        {
            Debug.Assert((fromIndex + nUniqueMidiDurationDefs) <= _uniqueMidiDurationDefs.Count);
            Debug.Assert(toIndex <= (_uniqueMidiDurationDefs.Count - nUniqueMidiDurationDefs));
            int msPosition = _uniqueMidiDurationDefs[0].MsPosition;
            List<IUniqueMidiDurationDef> extractedLmdds = _uniqueMidiDurationDefs.GetRange(fromIndex, nUniqueMidiDurationDefs);
            _uniqueMidiDurationDefs.RemoveRange(fromIndex, nUniqueMidiDurationDefs);
            _uniqueMidiDurationDefs.InsertRange(toIndex, extractedLmdds);
            SetMsPositions();
        }

        /// <summary>
        /// Returns the index of the IUniqueMidiDurationDef which starts at or is otherwise current at msPosition.
        /// If msPosition is the EndMsPosition, the index of the final IUniqueMidiDurationDef (Count-1) is returned.
        /// If the VoiceDef does not span msPosition, -1 (=error) is returned.
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        internal int FindIndexAtMsPosition(int msPosition)
        {
            int returnedIndex = -1;
            if(msPosition == EndMsPosition)
            {
                returnedIndex = this.Count - 1;
            }
            else if(msPosition >= _uniqueMidiDurationDefs[0].MsPosition && msPosition < EndMsPosition)
            {
                returnedIndex = _uniqueMidiDurationDefs.FindIndex(u => ((u.MsPosition <= msPosition) && ((u.MsPosition + u.MsDuration) > msPosition)));  
            }
            Debug.Assert(returnedIndex != -1);
            return returnedIndex;
        }

        #endregion list functions

        #region VoiceDef duration changers
        /// <summary>
        /// Stretch or compress all the durations in the list to fit the given total duration.
        /// This does not change the VoiceDef's MsPosition, but does affect its EndMsPosition.
        /// </summary>
        /// <param name="msDuration"></param>
        internal void SetMsDuration(int msDuration)
        {
            Debug.Assert(msDuration > 0);

            List<int> relativeDurations = new List<int>();
            foreach(IUniqueMidiDurationDef iumdd in _uniqueMidiDurationDefs)
            {
                relativeDurations.Add(iumdd.MsDuration);
            }

            List<int> newDurations = MidiChordDef.GetIntDurations(msDuration, relativeDurations, relativeDurations.Count);

            int i = 0;
            foreach(IUniqueMidiDurationDef iumdd in _uniqueMidiDurationDefs)
            {
                iumdd.MsDuration = newDurations[i++];
            }

            SetMsPositions();
        }
        /// <summary>
        /// Removes all the rests in this VoiceDef
        /// </summary>
        internal void RemoveRests()
        {
            AdjustMsDurations<UniqueMidiRestDef>(0, _uniqueMidiDurationDefs.Count, 0);
        }
        /// <summary>
        /// Multiplies the MsDuration of each chord and rest from startIndex to (not including) endIndex by factor.
        /// If a chord or rest's MsDuration becomes less than minThreshold, it is removed.
        /// The total duration of this VoiceDef changes accordingly.
        /// </summary>
        internal void AdjustMsDurations(int startIndex, int endIndex, double factor, int minThreshold = 100)
        {
            AdjustMsDurations<MidiDurationDef>(startIndex, endIndex, factor, minThreshold);
        }
        /// <summary>
        /// Multiplies the MsDuration of each chord and rest in the UniqueMidiDurationDefs list by factor.
        /// If a chord or rest's MsDuration becomes less than minThreshold, it is removed.
        /// The total duration of this VoiceDef changes accordingly.
        /// </summary>
        internal void AdjustMsDurations(double factor, int minThreshold = 100)
        {
            AdjustMsDurations<MidiDurationDef>(0, _uniqueMidiDurationDefs.Count, factor, minThreshold);
        }
        /// <summary>
        /// Multiplies the MsDuration of each chord from startIndex to (not including) endIndex by factor.
        /// If a chord's MsDuration becomes less than minThreshold, it is removed.
        /// The total duration of this VoiceDef changes accordingly.
        /// </summary>
        internal void AdjustChordMsDurations(int startIndex, int endIndex, double factor, int minThreshold = 100)
        {
            AdjustMsDurations<UniqueMidiChordDef>(startIndex, endIndex, factor, minThreshold);
        }
        /// <summary>
        /// Multiplies the MsDuration of each chord in the UniqueMidiDurationDefs list by factor.
        /// If a chord's MsDuration becomes less than minThreshold, it is removed.
        /// The total duration of this VoiceDef changes accordingly.
        /// </summary>
        internal void AdjustChordMsDurations(double factor, int minThreshold = 100)
        {
            AdjustMsDurations<UniqueMidiChordDef>(0, _uniqueMidiDurationDefs.Count, factor, minThreshold);
        }
        /// <summary>
        /// Multiplies the MsDuration of each rest from startIndex to (not including) endIndex by factor.
        /// If a rest's MsDuration becomes less than minThreshold, it is removed.
        /// The total duration of this VoiceDef changes accordingly.
        /// </summary>
        internal void AdjustRestMsDurations(int startIndex, int endIndex, double factor, int minThreshold = 100)
        {
            AdjustMsDurations<UniqueMidiRestDef>(startIndex, endIndex, factor, minThreshold);
        }
        /// <summary>
        /// Multiplies the MsDuration of each rest in the UniqueMidiDurationDefs list by factor.
        /// If a rest's MsDuration becomes less than minThreshold, it is removed.
        /// The total duration of this VoiceDef changes accordingly.
        /// </summary>
        internal void AdjustRestMsDurations(double factor, int minThreshold = 100)
        {
            AdjustMsDurations<UniqueMidiRestDef>(0, _uniqueMidiDurationDefs.Count, factor, minThreshold);
        }
        
        /// <summary>
        /// Multiplies the MsDuration of each T from startIndex to (not including) endIndex by factor.
        /// If a MsDuration becomes less than minThreshold, the T (chord or rest) is removed.
        /// The total duration of this VoiceDef changes accordingly.
        /// </summary>
        private void AdjustMsDurations<T>(int startIndex, int endIndex, double factor, int minThreshold = 100)
        {
            CheckIndices(startIndex, endIndex);
            Debug.Assert(factor >= 0);

            for(int i = 0; i < _uniqueMidiDurationDefs.Count; ++i)
            {
                IUniqueMidiDurationDef iumdd = _uniqueMidiDurationDefs[i];
                if(i >= startIndex && i < endIndex && iumdd is T)
                {
                    iumdd.MsDuration = (int)((double)iumdd.MsDuration * factor);
                }
            }

            for(int i = _uniqueMidiDurationDefs.Count - 1; i >= 0; --i)
            {
                IUniqueMidiDurationDef iumdd = _uniqueMidiDurationDefs[i];
                if(iumdd.MsDuration < minThreshold)
                {
                    _uniqueMidiDurationDefs.RemoveAt(i);
                }
            }

            SetMsPositions();
        }

        /// <summary>
        /// Creates an exponential accelerando or decelerando from startIndex to (not including) endIndex.
        /// This function changes the msDuration in the given index range.
        /// endIndex can be equal to this.Count.
        /// </summary>
        internal void CreateAccel(int startIndex, int endIndex, double startEndRatio)
        {
            Debug.Assert(((startIndex + 1) < endIndex) && (startEndRatio >= 0) && (endIndex <= Count));

            double basicIncrement = (startEndRatio - 1) / (endIndex - startIndex);
            double factor = 1.0;
            List<IUniqueMidiDurationDef> lmdds = _uniqueMidiDurationDefs;

            for(int i = startIndex; i < endIndex; ++i)
            {
                lmdds[i].AdjustDuration(factor);
                factor += basicIncrement;
            }

            SetMsPositions();
        }

        #endregion VoiceDef duration changers

        /// <summary>
        /// Combines all consecutive rests.
        /// </summary>
        internal void AgglomerateRests()
        {
            if(_uniqueMidiDurationDefs.Count > 1)
            {
                for(int i = _uniqueMidiDurationDefs.Count - 1; i > 0; --i)
                {
                    IUniqueMidiDurationDef lmdd2 = _uniqueMidiDurationDefs[i];
                    IUniqueMidiDurationDef lmdd1 = _uniqueMidiDurationDefs[i - 1];
                    if(lmdd2 is UniqueMidiRestDef && lmdd1 is UniqueMidiRestDef)
                    {
                        lmdd1.MsDuration += lmdd2.MsDuration;
                        _uniqueMidiDurationDefs.RemoveAt(i);
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
            _uniqueMidiDurationDefs[index - 1].MsDuration += _uniqueMidiDurationDefs[index].MsDuration;
            _uniqueMidiDurationDefs.RemoveAt(index);
        }

        #endregion internal Count changers

        #region internal properties
        /// <summary>
        /// The absolute position of the first note or rest in the sequence.
        /// Setting this value resets all the MsPositions in this VoiceDef,
        /// including the EndMsPosition.
        /// </summary>
        internal int StartMsPosition 
        { 
            get 
            { 
                Debug.Assert(_uniqueMidiDurationDefs.Count > 0);
                return _uniqueMidiDurationDefs[0].MsPosition;
            }
            set
            {
                Debug.Assert(_uniqueMidiDurationDefs.Count > 0);
                _uniqueMidiDurationDefs[0].MsPosition = value;
                SetMsPositions();
            } 
        }
        /// <summary>
        /// The absolute position of the end of the last note or rest in the sequence.
        /// Setting this value changes the msDuration of the final IUniqueMidiDurationDef.
        /// (The EndMsPosition cannot be set if this VoiceDef is empty, or before the last IUniqueMidiDurationDef.) 
        /// </summary>
        internal int EndMsPosition 
        { 
            get 
            {
                int endPosition = 0;
                if(_uniqueMidiDurationDefs.Count > 0)
                {
                    IUniqueMidiDurationDef lastLmdd = _uniqueMidiDurationDefs[_uniqueMidiDurationDefs.Count - 1];
                    endPosition = lastLmdd.MsPosition + lastLmdd.MsDuration;
                }
                return endPosition;
            }
            set
            {
                Debug.Assert(_uniqueMidiDurationDefs.Count > 0);
                IUniqueMidiDurationDef lastLmdd = _uniqueMidiDurationDefs[_uniqueMidiDurationDefs.Count - 1];
                Debug.Assert(value > lastLmdd.MsPosition);
                lastLmdd.MsDuration = value - lastLmdd.MsPosition;
            }
        }
        internal int Count { get { return _uniqueMidiDurationDefs.Count; } }

        #endregion internal properties

        #region internal attribute changers (Transpose etc.)
        /// <summary>
        /// Multiplies each expression value in the UniqueMidiDurationDefs
        /// from startIndex to (not including) endIndex by the argument factor.
        /// </summary>
        internal void AdjustExpression(int startIndex, int endIndex, double factor)
        {
            CheckIndices(startIndex, endIndex);

            for(int i = startIndex; i < endIndex; ++i)
            {
                _uniqueMidiDurationDefs[i].AdjustExpression(factor);
            }
        }
        /// <summary>
        /// Multiplies each expression value in the UniqueMidiDurationDefs by the argument factor.
        /// </summary>
        internal void AdjustExpression(double factor)
        {
            foreach(IUniqueMidiDurationDef iumdd in _uniqueMidiDurationDefs)
            {
                iumdd.AdjustExpression(factor);
            }
        }
        /// <summary>
        /// Multiplies each velocity value in the UniqueMidiDurationDefs
        /// from startIndex to (not including) endIndex by the argument factor.
        /// </summary>
        internal void AdjustVelocities(int startIndex, int endIndex, double factor)
        {
            CheckIndices(startIndex, endIndex);
            for(int i = startIndex; i < endIndex; ++i)
            {
                _uniqueMidiDurationDefs[i].AdjustVelocities(factor);
            }
        }
        /// <summary>
        /// Multiplies each velocity value in the UniqueMidiDurationDefs by the argument factor.
        /// </summary>
        internal void AdjustVelocities(double factor)
        {
            foreach(IUniqueMidiDurationDef iumdd in _uniqueMidiDurationDefs)
            {
                iumdd.AdjustVelocities(factor);
            }
        }
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
        internal void AdjustVelocitiesHairpin(int beginIndex, int endIndex, double finalFactor)
        {
            Debug.Assert(((beginIndex + 1) < endIndex) && (finalFactor >= 0) && (endIndex <= Count));

            double factorIncrement = (finalFactor - 1.0) / (endIndex - beginIndex);
            double factor = 1.0;
            List<IUniqueMidiDurationDef> lmdds = _uniqueMidiDurationDefs;

            for(int i = beginIndex; i < endIndex; ++i)
            {
                lmdds[i].AdjustVelocities(factor);
                factor += factorIncrement;
            }

            for(int i = endIndex; i < lmdds.Count; ++i)
            {
                lmdds[i].AdjustVelocities(factor);
            }
        }

        /// Creates a hairpin in the velocities from startMsPosition to endMsPosition (non-inclusive).
        /// This function does NOT change velocities outside the range given in its arguments.
        /// There must be at least two IUniqueMidiDurationDefs in the msPosition range given in the arguments.
        /// The factors by which the velocities are multiplied change arithmetically:
        /// The velocity of the first IUniqueMidiDurationDefs is multiplied by startFactor, and the velocity
        /// of the last IUniqueMidiDurationDef in range by endFactor.
        /// Can be used to create a diminueno or crescendo.
        internal void AdjustVelocitiesHairpin(int startMsPosition, int endMsPosition, double startFactor, double endFactor)
        {
            int startIndex = FindIndexAtMsPosition(startMsPosition);
            int endIndex = FindIndexAtMsPosition(endMsPosition);

            Debug.Assert(((startIndex + 1) < endIndex) && (startFactor >= 0) && (endFactor >= 0) && (endIndex <= Count));

            double factorIncrement = (endFactor - startFactor) / (endIndex - startIndex);
            double factor = startFactor;
            List<IUniqueMidiDurationDef> lmdds = _uniqueMidiDurationDefs;

            for(int i = startIndex; i < endIndex; ++i)
            {
                lmdds[i].AdjustVelocities(factor);
                factor += factorIncrement;
            }
        }

        /// <summary>
        /// Creates a moving pan from startPanValue at startMsPosition to endPanValue at endMsPosition.
        /// Implemented using one pan value per IUniqueMidiDurationDef.
        /// This function does NOT change pan values outside the position range given in its arguments.
        /// </summary>
        internal void SetPanGliss(int startMsPosition, int endMsPosition, int startPanValue, int endPanValue)
        {
            int startIndex = FindIndexAtMsPosition(startMsPosition);
            int endIndex = FindIndexAtMsPosition(endMsPosition);

            Debug.Assert(((startIndex + 1) < endIndex) && (startPanValue >= 0) && (startPanValue <= 127)
                && (endPanValue >= 0) && (endPanValue <=127) && (endIndex <= Count));

            double increment = ((double)(endPanValue - startPanValue)) / (endIndex - startIndex);
            int panValue = startPanValue;
            List<IUniqueMidiDurationDef> lmdds = _uniqueMidiDurationDefs;

            for(int i = startIndex; i < endIndex; ++i)
            {
                lmdds[i].PanMsbs = new List<byte>() { (byte)panValue };
                panValue += (int)increment;
            }
        }

        /// <summary>
        /// Transpose all the lmdds from startIndex to (not including) endIndex
        /// up by the number of semitones given in the interval argument.
        /// Negative interval values transpose down.
        /// It is not an error if Midi pitch values would exceed the range 0..127.
        /// In this case, they are silently coerced to 0 or 127 respectively.
        /// </summary>
        /// <param name="interval"></param>
        internal void Transpose(int startIndex, int endIndex, int interval)
        {
            CheckIndices(startIndex, endIndex);
            for(int i = startIndex; i < endIndex; ++i)
            {
                _uniqueMidiDurationDefs[i].Transpose(interval);
            }
        }
        /// <summary>
        /// Transpose the whole VoiceDef up by the number of semitones given in the argument.
        /// </summary>
        /// <param name="interval"></param>
        internal void Transpose(int interval)
        {
            foreach(IUniqueMidiDurationDef iumdd in _uniqueMidiDurationDefs)
            {
                iumdd.Transpose(interval);
            }
        }

        /// <summary>
        /// Transposes the UniqueMidiDurationDefs from the startIndex upto (but not including) endIndex
        /// by an equally increasing amount, so that the final IUniqueMidiDurationDef is transposed by glissInterval.
        /// startIndex must be less than endIndex.
        /// glissInterval can be negative.
        /// </summary>
        internal void StepwiseGliss(int startIndex, int endIndex, int glissInterval)
        {
            CheckIndices(startIndex, endIndex);
            Debug.Assert(startIndex < endIndex);

            int nSteps = (endIndex - startIndex);
            double interval = ((double)glissInterval) / nSteps;
            double step = interval;
            for(int index = startIndex; index < endIndex; ++index)
            {
                _uniqueMidiDurationDefs[index].Transpose((int)Math.Round(interval));
                interval += step;
            }
        }

        /// <summary>
        /// Sets the pitchwheelDeviation for chords in the range startIndex to (not including) endindex.
        /// Rests in the range dont change.
        /// </summary>
        internal void SetPitchWheelDeviation(int startIndex, int endIndex, int deviation)
        {
            CheckIndices(startIndex, endIndex);
            Debug.Assert(deviation >= 0 && deviation <= 127);

            for(int i = startIndex; i < endIndex; ++i)
            {
                IUniqueMidiDurationDef iumdd = this[i];
                iumdd.PitchWheelDeviation = deviation;
            }
        }

        /// <summary>
        /// Removes the pitchwheel commands (not the pitchwheelDeviations)
        /// from chords in the range startIndex to (not including) endIndex.
        /// Rests in the range are not changed.
        /// </summary>
        internal void RemoveScorePitchWheelCommands(int startIndex, int endIndex)
        {
            CheckIndices(startIndex, endIndex);

            for(int i = startIndex; i < endIndex; ++i)
            {
                IUniqueMidiDurationDef iumdd = this[i];
                UniqueMidiChordDef umcd = iumdd as UniqueMidiChordDef;
                if(umcd != null)
                {
                    umcd.MidiChordSliderDefs.PitchWheelMsbs = new List<byte>();
                }
            }
        }

        #endregion  internal attribute changers (Transpose etc.)

        #region alignment
        /// <summary>
        /// _localizedMidiDurationDefs[indexToAlign] (=lmddAlign) is moved to toMsPosition, and the surrounding symbols are spread accordingly
        /// between those at anchor1Index and anchor2Index. The symbols at anchor1Index (=lmddA1) and anchor2Index (=lmddA2)do not move.
        /// Note that indexToAlign cannot be 0, and that anchor2Index CAN be equal to _localizedMidiDurationDefs.Count (i.e.on the final barline).
        /// This function checks that 
        ///     1. anchor1Index is in range 0..(indexToAlign - 1),
        ///     2. anchor2Index is in range (indexToAlign + 1).._localizedMidiDurationDefs.Count
        ///     3. toPosition is greater than the msPosition at anchor1Index and less than the msPosition at anchor2Index.
        /// and throws an appropriate exception if there is a problem.
        /// </summary>
        internal void AlignObjectAtIndex(int anchor1Index, int indexToAlign, int anchor2Index, int toMsPosition)
        {
            // throws an exception if there's a problem.
            CheckAlignDefArgs(anchor1Index, indexToAlign, anchor2Index, toMsPosition);

            List<IUniqueMidiDurationDef> lmdds = _uniqueMidiDurationDefs;
            int anchor1MsPosition = lmdds[anchor1Index].MsPosition;
            int fromMsPosition = lmdds[indexToAlign].MsPosition;
            int anchor2MsPosition;
            if(anchor2Index == lmdds.Count) // i.e. anchor2 is on the final barline
            {
                anchor2MsPosition = lmdds[anchor2Index - 1].MsPosition + lmdds[anchor2Index - 1].MsDuration;
            }
            else
            {
                anchor2MsPosition = lmdds[anchor2Index].MsPosition;
            }

            float leftFactor = (float)(((float)(toMsPosition - anchor1MsPosition))/((float)(fromMsPosition - anchor1MsPosition)));
            for(int i = anchor1Index + 1; i < indexToAlign; ++i)
            {
                lmdds[i].MsPosition = anchor1MsPosition + ((int)((lmdds[i].MsPosition - anchor1MsPosition) * leftFactor));           
            }

            lmdds[indexToAlign].MsPosition = toMsPosition;

            float rightFactor = (float)(((float)(anchor2MsPosition - toMsPosition)) / ((float)(anchor2MsPosition - fromMsPosition)));
            for(int i = anchor2Index - 1; i > indexToAlign; --i)
            {
                lmdds[i].MsPosition = anchor2MsPosition - ((int)((anchor2MsPosition - lmdds[i].MsPosition) * rightFactor));
            }

            #region fix MsDurations
            for(int i = anchor1Index + 1; i <= anchor2Index; ++i)
            {
                if(i == lmdds.Count) // possible, when anchor2Index is the final barline
                {
                    lmdds[i - 1].MsDuration = anchor2MsPosition - lmdds[i - 1].MsPosition;
                }
                else
                {
                    lmdds[i - 1].MsDuration = lmdds[i].MsPosition - lmdds[i - 1].MsPosition;
                }
            }
            #endregion
        }
        /// <summary>
        /// Throws an exception if
        ///     1. the index arguments are not in ascending order. (None of them may not be equal either.)
        ///     2. any of the index arguments are out of range (anchor2Index CAN be _localizedMidiDurationDefs.Count, i.e. the final barline)
        ///     3. toPosition is not greater than the msPosition at anchor1Index and less than the msPosition at anchor2Index.
        /// </summary>
        private void CheckAlignDefArgs(int anchor1Index, int indexToAlign, int anchor2Index, int toMsPosition)
        {
            List<IUniqueMidiDurationDef> lmdds = _uniqueMidiDurationDefs; 
            int count = lmdds.Count;
            string msg = "\nError in VoiceDef.cs,\nfunction AlignDefMsPosition()\n\n";
            if(anchor1Index >= indexToAlign || anchor2Index <= indexToAlign)
            {
                throw new Exception(msg + "Index out of order.\n" +
                    "\nanchor1Index=" + anchor1Index.ToString() +
                    "\nindexToAlign=" + indexToAlign.ToString() +
                    "\nanchor2Index=" + anchor2Index.ToString());
            }            
            if((anchor1Index > (count - 2) || indexToAlign > (count - 1) || anchor2Index > count)// anchor2Index can be at the final barline (=count)!
                || (anchor1Index < 0 || indexToAlign < 1 || anchor2Index < 2))
            {
                throw new Exception(msg + "Index out of range.\n" +
                    "\ncount=" + count.ToString() +
                    "\nanchor1Index=" + anchor1Index.ToString() +
                    "\nindexToAlign=" + indexToAlign.ToString() +
                    "\nanchor2Index=" + anchor2Index.ToString());
            }

            int a1MsPos = lmdds[anchor1Index].MsPosition;
            int a2MsPos;
            if(anchor2Index == lmdds.Count)
            {
                a2MsPos = lmdds[anchor2Index - 1].MsPosition + lmdds[anchor2Index - 1].MsDuration;
            }
            else
            {
                a2MsPos = lmdds[anchor2Index].MsPosition;
            }
            if(toMsPosition <= a1MsPos || toMsPosition >= a2MsPos)
            {
                throw new Exception(msg + "Target (msPos) position out of range.\n" +
                    "\nanchor1Index=" + anchor1Index.ToString() +
                    "\nindexToAlign=" + indexToAlign.ToString() +
                    "\nanchor2Index=" + anchor2Index.ToString() +
                    "\ntoMsPosition=" + toMsPosition.ToString());  
            }
        }
        #endregion alignment

        #region internal Permute()
        /// <summary>
        /// Re-orders the UniqueMidiDurationDefs in (part of) this VoiceDef.
        /// <para>1. creates partitions (lists of UniqueMidiDurationDefs) using the startAtIndex and partitionSizes in the first two</para>
        /// <para>-  arguments (see parameter info below).</para>   
        /// <para>2. Sorts the partitions into ascending order of their lowest pitches.
        /// <para>3. Re-orders the partitions according to the contour retrieved (from the static K.Contour[] array) using</para>
        /// <para>-  the contourNumber and axisNumber arguments.</para>
        /// <para>4. Concatenates the re-ordered partitions, re-sets their MsPositions, and replaces the UniqueMidiDurationDefs in</para>
        /// <para>-  the original List with the result.</para>
        /// <para>5. If setLyricsToIndex is true, sets the lyrics to the new indices</para>
        /// <para>SpecialCases:</para>
        /// <para>If a partition contains a single LocalMidiRestDef (the partition is a *rest*, having no 'lowest pitch'), the other</para>
        /// <para>partitions are re-ordered as if the rest-partition was not there, and the rest-partition is subsequently re-inserted</para>
        /// <para>at its original partition position.</para>
        /// <para>If two sub-VoiceDefs have the same initial base pitch, they stay in the same order as they were (not necessarily</para>
        /// <para>together, of course.)</para>
        /// </summary>
        /// <param name="startAtIndex">The index in UniqueMidiDurationDefs at which to start the re-ordering.
        /// </param>
        /// <param name="partitionSizes">The number of UniqueMidiDurationDefs in each sub-voiceDef to be re-ordered.
        /// <para>This partitionSizes list must contain:</para>
        /// <para>    1..7 int sizes.</para>
        /// <para>    sizes which are all greater than 0.</para>
        /// <para>    The sum of all the sizes + startAtIndex must be less than or equal to UniqueMidiDurationDefs.Count.</para>
        /// <para>An Exception is thrown if any of these conditions is not met.</para>
        /// <para>If the partitions list contains only one value, this function returns silently without doing anything.</para>
        /// </param>
        /// <param name="contourNumber">A value greater than or equal to 1, and less than or equal to 12. An exception is thrown if this is not the case.</param>
        /// <param name="axisNumber">A value greater than or equal to 1, and less than or equal to 12 An exception is thrown if this is not the case.</param>
        internal void Permute(int startAtIndex, List<int> partitionSizes, int axisNumber, int contourNumber)
        {
            CheckSetContourArgs(startAtIndex, partitionSizes, axisNumber, contourNumber);

            List<List<IUniqueMidiDurationDef>> partitions = GetPartitions(startAtIndex, partitionSizes);

            // Remove any partitions (from the partitions list) that contain only a single LocalMidiRestDef
            // Store them (the rests), with their original partition indices, in the returned list of KeyValuePairs.
            List<KeyValuePair<int, List<IUniqueMidiDurationDef>>> restPartitions = GetRestPartitions(partitions);

            if(partitions.Count > 1)
            {
                // All the partitions now contain at least one LocalMidiChordDef
                // Sort into ascending order (part 2 of the above comment)
                partitions = SortPartitions(partitions);
                // re-order the partitions (part 3 of the above comment)
                partitions = DoContouring(partitions, axisNumber, contourNumber);
            }

            RestoreRestPartitions(partitions, restPartitions);

            List<IUniqueMidiDurationDef> sortedLmdds = ConvertPartitionsToFlatLmdds(startAtIndex, partitions);

            for(int i = 0; i < sortedLmdds.Count; ++i)
            {
                _uniqueMidiDurationDefs[startAtIndex + i] = sortedLmdds[i];
            }            
        }

        private List<IUniqueMidiDurationDef> ConvertPartitionsToFlatLmdds(int startAtIndex, List<List<IUniqueMidiDurationDef>> partitions)
        {
            List<IUniqueMidiDurationDef> newLmdds = new List<IUniqueMidiDurationDef>();
            int msPosition = _uniqueMidiDurationDefs[startAtIndex].MsPosition;
            foreach(List<IUniqueMidiDurationDef> partition in partitions)
            {
                foreach(IUniqueMidiDurationDef pLmdd in partition)
                {
                    pLmdd.MsPosition = msPosition;
                    msPosition += pLmdd.MsDuration;
                    newLmdds.Add(pLmdd);
                }
            }
            return newLmdds;
        }

        /// <summary>
        /// Re-insert the restPartitions at their original positions
        /// </summary>
        private void RestoreRestPartitions(List<List<IUniqueMidiDurationDef>> partitions, List<KeyValuePair<int, List<IUniqueMidiDurationDef>>> restPartitions)
        {
            for(int i = restPartitions.Count - 1; i >= 0; --i)
            {
                KeyValuePair<int, List<IUniqueMidiDurationDef>> kvp = restPartitions[i];
                partitions.Insert(kvp.Key, kvp.Value);
            }
        }

        private List<List<IUniqueMidiDurationDef>> GetPartitions(int startAtIndex, List<int> partitionSizes)
        {
            List<List<IUniqueMidiDurationDef>> partitions = new List<List<IUniqueMidiDurationDef>>();
            int lmddIndex = startAtIndex;
            foreach(int size in partitionSizes)
            {
                List<IUniqueMidiDurationDef> partition = new List<IUniqueMidiDurationDef>();
                for(int i = 0; i < size; ++i)
                {
                    partition.Add(_uniqueMidiDurationDefs[lmddIndex++]);
                }
                partitions.Add(partition);
            }
            return partitions;
        }

        /// <summary>
        /// Remove any partitions (from partitions) that contain only a single LocalMidiRestDef.
        /// Store them, with their original partition indices, in the returned list of KeyValuePairs.
        /// </summary>
        private List<KeyValuePair<int, List<IUniqueMidiDurationDef>>> GetRestPartitions(List<List<IUniqueMidiDurationDef>> partitions)
        {
            List<List<IUniqueMidiDurationDef>> newPartitions = new List<List<IUniqueMidiDurationDef>>();
            List<KeyValuePair<int, List<IUniqueMidiDurationDef>>> restPartitions = new List<KeyValuePair<int, List<IUniqueMidiDurationDef>>>();
            for(int i = 0; i < partitions.Count; ++i)
            {
                List<IUniqueMidiDurationDef> partition = partitions[i];
                if(partition.Count == 1 && partition[0] is UniqueMidiRestDef)
                {
                    restPartitions.Add(new KeyValuePair<int, List<IUniqueMidiDurationDef>>(i, partition));
                }
                else
                {
                    newPartitions.Add(partition);
                }
            }

            partitions = newPartitions;
            return restPartitions;
        }

        /// <summary>
        /// Returns the result of sorting the partitions into ascending order of their lowest pitches.
        /// <para>All the partitions contain at least one UniqueMidiChordDef</para>
        /// <para>If two partitions have the same lowest pitch, they stay in the same order as they were</para>
        /// <para>(not necessarily together, of course.)</para>    
        /// </summary>
        private List<List<IUniqueMidiDurationDef>> SortPartitions(List<List<IUniqueMidiDurationDef>> partitions)
        {
            List<byte> lowestPitches = GetLowestPitches(partitions);
            List<byte> sortedLowestPitches = new List<byte>(lowestPitches);
            sortedLowestPitches.Sort();

            List<List<IUniqueMidiDurationDef>> sortedPartitions = new List<List<IUniqueMidiDurationDef>>();
            foreach(byte lowestPitch in sortedLowestPitches)
            {
                int pitchIndex = lowestPitches.FindIndex(item => item == lowestPitch);
                sortedPartitions.Add(partitions[pitchIndex]);
                lowestPitches[pitchIndex] = byte.MaxValue;
            }

            return sortedPartitions;
        }

        /// <summary>
        /// Returns a list containing the lowest pitch in each partition
        /// </summary>
        private List<byte> GetLowestPitches(List<List<IUniqueMidiDurationDef>> partitions)
        {
            List<byte> lowestPitches = new List<byte>();
            foreach(List<IUniqueMidiDurationDef> partition in partitions)
            {
                byte lowestPitch = byte.MaxValue;
                foreach(IUniqueMidiDurationDef iumdd in partition)
                {
                    UniqueMidiChordDef umcd = iumdd as UniqueMidiChordDef;
                    if(umcd != null) // partitions can contain rests, which are however ignored here
                    {
                        foreach(BasicMidiChordDef bmcd in umcd.BasicMidiChordDefs)
                        {
                            foreach(byte note in bmcd.Notes)
                            {
                                lowestPitch = (lowestPitch < note) ? lowestPitch : note;
                            }
                        }
                    }
                }
                Debug.Assert(lowestPitch != byte.MaxValue, "There must be at least one UniqueMidiChordDef in the partition.");
                lowestPitches.Add(lowestPitch);
            }
            return lowestPitches;
        }

        /// <summary>
        /// Re-orders the partitions according to the contour retrieved (from the static K.Contour[] array)
        /// <para>using partitions.Count and the contourNumber and axisNumber arguments.</para>
        /// <para>Does not change the inner contents of the partitions themselves.</para>
        /// </summary>
        /// <returns>A re-ordered list of partitions</returns>
        private List<List<IUniqueMidiDurationDef>> DoContouring(List<List<IUniqueMidiDurationDef>> partitions, int axisNumber, int contourNumber)
        {
            List<List<IUniqueMidiDurationDef>> contouredPartitions = new List<List<IUniqueMidiDurationDef>>();
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
        /// Throws an exception if one of the following conditions is not met.
        /// <para>startAtIndex is a valid index in the UniqueMidiDurationDefs list</para>
        /// <para>partitionSizes.Count is greater than 0, and less than 8.</para>
        /// <para>all sizes in partitionSizes are greater then 0.</para>
        /// <para>the sum of startAtIndex plus all the partition sizes is not greater than UniqueMidiDurationDefs.Count</para>
        /// <para>contourNumber is in the range 1..12</para>
        /// <para>axisNumber is in the range 1..12</para>
        /// </summary>
        private void CheckSetContourArgs(int startAtIndex, List<int> partitionSizes, int axisNumber, int contourNumber)
        {
            List<IUniqueMidiDurationDef> lmdds = _uniqueMidiDurationDefs;
            if(startAtIndex < 0 || startAtIndex > lmdds.Count - 1)
            {
                throw new ArgumentException("startAtIndex is out of range.");
            }
            if(partitionSizes.Count < 1 || partitionSizes.Count > 7)
            {
                throw new ArgumentException("partitionSizes.Count must be in range 1..7");
            }

            int totalNumberOfLmdds = startAtIndex;
            foreach(int size in partitionSizes)
            {
                if(size < 1)
                {
                    throw new ArgumentException("partitions must contain at least one IUniqueMidiDurationDef");
                }
                totalNumberOfLmdds += size;
                if(totalNumberOfLmdds > lmdds.Count)
                {
                    throw new ArgumentException("partitions are too big or start too late"); 
                }
            }

            if(contourNumber < 1 || contourNumber > 12)
            {
                throw new ArgumentException("contourNumber out of range 1..12");
            }
            if(axisNumber < 1 || axisNumber > 12)
            {
                throw new ArgumentException("axisNumber out of range 1..12");
            }
        }
        #endregion internal SetContour()

        #region  internal SetLyricsToIndex()

        /// <summary>
        /// Rests dont have lyrics, so their index in the VoiceDef can't be shown as a lyric.
        /// Overridden by Clytemnestra, where the index is inserted before her lyrics.
        /// </summary>
        /// <param name="voiceDef"></param>
        internal virtual void SetLyricsToIndex()
        {
            for(int index = 0; index < _uniqueMidiDurationDefs.Count; ++index)
            {
                UniqueMidiChordDef lmcd = _uniqueMidiDurationDefs[index] as UniqueMidiChordDef;
                if(lmcd != null)
                {
                    lmcd.Lyric = index.ToString();
                }
            }
        }
        #endregion internal SetLyricsToIndex()

        internal List<IUniqueMidiDurationDef> UniqueMidiDurationDefs { get { return _uniqueMidiDurationDefs; } }
        #endregion internal

        /// <summary>
        /// Creates an exponential change (per index) of pitchwheelDeviation from startMsPosition to endMsPosition,
        /// </summary>
        /// <param name="finale"></param>
        protected void AdjustPitchWheelDeviations(int startMsPosition, int endMsPosition, int startPwd, int endPwd)
        {
            double furies1StartPwdValue = startPwd, furies1EndPwdValue = endPwd;
            int startIndex = FindIndexAtMsPosition(startMsPosition);
            int endIndex = FindIndexAtMsPosition(endMsPosition);
 
            double pwdfactor = Math.Pow(furies1EndPwdValue / furies1StartPwdValue, (double)1 / this.Count); // f13.Count'th root of furies1EndPwdValue/furies1StartPwdValue -- the last pwd should be furies1EndPwdValue

            for(int i = startIndex; i < endIndex; ++i)
            {
                this[i].PitchWheelDeviation = (int)(furies1StartPwdValue * (Math.Pow(pwdfactor, i)));
            }
        }

        protected List<IUniqueMidiDurationDef> _uniqueMidiDurationDefs = new List<IUniqueMidiDurationDef>();

        #region private
        /// <summary>
        /// Sets the MsPosition attribute of each IUniqueMidiDurationDef in the _uniqueMidiDurationDefs list.
        /// Uses all the MsDuration attributes, and the MsPosition of the first IUniqueMidiDurationDef as origin.
        /// This function must be called at the end of any function that changes the _uniqueMidiDurationDefs list.
        /// </summary>
        private void SetMsPositions()
        {
            if(_uniqueMidiDurationDefs.Count > 0)
            {
                int currentPosition = _uniqueMidiDurationDefs[0].MsPosition;
                Debug.Assert(currentPosition >= 0);
                foreach(IUniqueMidiDurationDef iumdd in _uniqueMidiDurationDefs)
                {
                    iumdd.MsPosition = currentPosition;
                    currentPosition += iumdd.MsDuration;
                }
            }
        }

        private void CheckIndices(int startIndex, int endIndex)
        {
            Debug.Assert(startIndex >= 0 && startIndex < _uniqueMidiDurationDefs.Count);
            Debug.Assert(endIndex >= 0 && endIndex <= _uniqueMidiDurationDefs.Count);
        }

        #endregion private
    }
}
