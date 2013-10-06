
using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections;
using Moritz.Score.Midi;
using Krystals4ObjectLibrary;

namespace Moritz.AssistantComposer
{
    /// <summary>
    /// A temporal sequence of LocalMidiDurationDef objects.
    /// <para>The objects can define either notes or rests.</para>
    /// <para>(LocalMidiDurationDef.UniqueMidiDurationDef is either a UniqueMidiChordDef or a UniqueMidiRestDef.)</para>
    /// <para></para>
    /// <para>This class is IEnumerable, so that foreach loops can be used.</para>
    /// <para>For example:</para>
    /// <para>foreach(LocalMidiDurationDef lmd in midiPhrase) { ... }</para>
    /// <para>It is also indexable, as in:</para>
    /// <para>LocalMidiDurationDef lmdd = midiPhrase[index];</para>
    /// </summary>
    public class MidiPhrase : IEnumerable
    {
        /// <summary>
        /// The argument may not be an empty list.
        /// <para>The MsPositions and MsDurations in the list are checked for consistency.</para>
        /// <para>The new MidiPhrase's LocalMidiDurationDefs list is simply set to the argument (which is not cloned).</para>
        /// </summary>
        public MidiPhrase(List<LocalMidiDurationDef> lmdds)
        {
            Debug.Assert(lmdds.Count > 0);
            for(int i = 1; i < lmdds.Count; ++i)
            {
                Debug.Assert(lmdds[i - 1].MsPosition + lmdds[i - 1].MsDuration == lmdds[i].MsPosition);
            }
            _localMidiDurationDefs = lmdds;
        }

        /// <summary>
        /// sequence contains a list of values in range 1..paletteDef.MidiDurationDefsCount.
        /// </summary>
        public MidiPhrase(PaletteDef paletteDef, List<int> sequence)
        {
            int msPosition = 0;

            foreach(int value in sequence)
            {
                Debug.Assert((value >= 1 && value <= paletteDef.MidiDurationDefsCount), "Illegal argument: value out of range in sequence");
                MidiDurationDef midiDurationDef = paletteDef[value - 1];
                LocalMidiDurationDef noteDef = new LocalMidiDurationDef(midiDurationDef);
                Debug.Assert(midiDurationDef.MsDuration > 0);
                Debug.Assert(noteDef.MsDuration == midiDurationDef.MsDuration);
                noteDef.MsPosition = msPosition;
                msPosition += noteDef.MsDuration;
                _localMidiDurationDefs.Add(noteDef);
            }
        }

        /// <summary>
        /// Constructs a MidiPhrase at MsPosition=0, containing the localized sequence of MidiDurationDefs in the PaletteDef.
        /// </summary
        public MidiPhrase(PaletteDef midiDurationDefs)
        {
            Debug.Assert(midiDurationDefs != null);
            foreach(MidiDurationDef midiDurationDef in midiDurationDefs)
            {
                LocalMidiDurationDef lmdd = new LocalMidiDurationDef(midiDurationDef);
                _localMidiDurationDefs.Add(lmdd);
            }
            MsPosition = _localMidiDurationDefs[0].MsPosition; // sets the absolute position of all notes and rests
        }

        /// <summary>
        /// Returns a deep clone of this MidiPhrase.
        /// </summary>
        public MidiPhrase Clone()
        {
            List<LocalMidiDurationDef> clonedLmdds = new List<LocalMidiDurationDef>();
            foreach(LocalMidiDurationDef lmdd in this.LocalMidiDurationDefs)
            {
                LocalMidiDurationDef clonedLmdd = new LocalMidiDurationDef(lmdd.UniqueMidiDurationDef, lmdd.MsPosition, lmdd.MsDuration);
                clonedLmdds.Add(clonedLmdd);
            }

            return new MidiPhrase(clonedLmdds);
        }

        /// <summary>
        /// Appends the new lmdd to the end of the list.
        /// </summary>
        /// <param name="lmdd"></param>
        public void Add(LocalMidiDurationDef lmdd)
        {
            Debug.Assert(_localMidiDurationDefs.Count > 0);
            LocalMidiDurationDef lastLmdd = _localMidiDurationDefs[_localMidiDurationDefs.Count - 1];
            lmdd.MsPosition = lastLmdd.MsPosition + lastLmdd.MsDuration;
            _localMidiDurationDefs.Add(lmdd);
        }
        /// <summary>
        /// removes the lmdd from the list, and then resets the positions of all the lmdds in the list.
        /// </summary>
        /// <param name="lmdd"></param>
        public void Remove(LocalMidiDurationDef lmdd)
        {
            Debug.Assert(_localMidiDurationDefs.Count > 0);
            Debug.Assert(_localMidiDurationDefs.Contains(lmdd));
            _localMidiDurationDefs.Remove(lmdd);
            MsPosition = _localMidiDurationDefs[0].MsPosition; // sets the absolute position of all notes and rests 
        }
        /// <summary>
        /// The total duration of this MidiPhrase in milliseconds.
        /// Setting this.MsDuration stretches or compresses all the durations in the list to fit the given total duration.
        /// It does not change this.MsPosition, but does affect this.EndMsPosition. 
        /// </summary>
        public int MsDuration
        {
            get
            {
                int msDuration = 0;
                foreach(LocalMidiDurationDef lmdd in _localMidiDurationDefs)
                {
                    msDuration += lmdd.MsDuration;
                }
                return msDuration;
            }
            set
            {
                List<int> relativeDurations = new List<int>();
                foreach(LocalMidiDurationDef lmdd in _localMidiDurationDefs)
                {
                    relativeDurations.Add(lmdd.MsDuration);
                }

                int newMsDuration = value;
                Debug.Assert(newMsDuration > 0);
                List<int> newDurations = MidiChordDef.GetIntDurations(newMsDuration, relativeDurations, relativeDurations.Count);

                int i = 0;
                foreach (LocalMidiDurationDef lmdd in _localMidiDurationDefs)
                {
                    lmdd.MsDuration = newDurations[i++];
                }

                MsPosition = _localMidiDurationDefs[0].MsPosition; // sets the absolute position of all notes and rest definitions
            }
        }
        /// <summary>
        /// The absolute position of the first note or rest in the sequence.
        /// Setting this value sets the absolute position of all the lmdds
        /// in the sequence, without changing their durations.
        /// </summary>
        public int MsPosition 
        { 
            get 
            { 
                Debug.Assert(_localMidiDurationDefs.Count > 0);
                return _localMidiDurationDefs[0].MsPosition;
            } 
            set 
            {
                Debug.Assert(_localMidiDurationDefs.Count > 0);
                int absolutePosition = value;
                Debug.Assert(absolutePosition >= 0);
                foreach(LocalMidiDurationDef lmdd in _localMidiDurationDefs)
                {
                    lmdd.MsPosition = absolutePosition;
                    absolutePosition += lmdd.MsDuration;
                }
            } 
        }
        /// <summary>
        /// The absolute position of the end of the last note or rest in the sequence.
        /// </summary>
        public int EndMsPosition 
        { 
            get 
            {
                Debug.Assert(_localMidiDurationDefs.Count > 0);
                LocalMidiDurationDef lastLmdd = _localMidiDurationDefs[_localMidiDurationDefs.Count - 1];
                return lastLmdd.MsPosition + lastLmdd.MsDuration; 
            }
        }
        public int Count { get { return _localMidiDurationDefs.Count; } }
        /// <summary>
        /// Indexer. Allows individual lmdds to be accessed using array notation on the MidiPhrase.
        /// e.g. lmdd = midiPhrase[3].
        /// </summary>
        public LocalMidiDurationDef this[int i]
        {
            get
            {
                if(i < 0 || i >= _localMidiDurationDefs.Count)
                {
                    throw new IndexOutOfRangeException();
                }
                return _localMidiDurationDefs[i];
            }
            set
            {
                if(i < 0 || i >= _localMidiDurationDefs.Count)
                {
                    throw new IndexOutOfRangeException();
                }
                _localMidiDurationDefs[i] = value;
            }
        }
        /// <summary>
        /// Transpose all the lmdds in the MidiPhrase up by the number of semitones given in the argument.
        /// Negative interval values transpose down.
        /// It is not an error if Midi pitch values would exceed the range 0..127.
        /// In this case, they are silently coerced to 0 or 127 respectively.
        /// </summary>
        /// <param name="interval"></param>
        public void Transpose(int interval)
        {
            foreach(LocalMidiDurationDef lmdd in _localMidiDurationDefs)
            {
                lmdd.Transpose(interval);
            }
        }
        /// <summary>
        /// _localizedMidiDurationDefs[indexToAlign] (=lmddAlign) is moved to MsPosition, and the surrounding symbols are spread accordingly
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

            List<LocalMidiDurationDef> lmdds = _localMidiDurationDefs;
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
            List<LocalMidiDurationDef> lmdds = _localMidiDurationDefs; 
            int count = lmdds.Count;
            string msg = "\nError in MidiPhrase.cs,\nfunction AlignDefMsPosition()\n\n";
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

        /// <summary>
        /// Re-orders the LocalMidiDurationDefs in this MidiPhrase.
        /// <para>1. creates sub-midiPhrases using the partitionSizes in the first argument (see parameter info below).</para>   
        /// <para>2. Sorts the sub-midiPhrases into ascending order of the base pitch of their first chord. If a sub-MidiPhrase</para>
        /// <para>    contains no chords, it is put at the beginning of the sort. If two sub-MidiPhrases have the same initial</para>
        /// <para>    base pitch, they stay in the same order as they were.</para>
        /// <para>3. Re-orders the sub-midiPhrases according to the contour retrieved (from the static K.Contour[] array) using</para>
        /// <para>    partitions.Count and the contourNumber and axisNumber arguments.</para>
        /// <para>4. Concatenates the re-ordered sub-MidiPhrases, sets their MsPositions (by setting the starting MsPosition to</para>
        /// <para>    the original MsPosition), and assigns the result to this.LocalMidiDurationDefs.</para>
        /// </summary>
        /// <param name="partitionSizes">Contains the number of LocalMidiDurationDefs in each sub-midiPhrase to be re-ordered.
        /// <para>This partitionSizess list must contain:</para>
        /// <para>    0 or more ints and less than 8 ints.</para>
        /// <para>    ints which are all greater than 0 and less than or equal to LocalMidiDurationDefs.Count.</para>
        /// <para>    The sum of all the ints must be equal to LocalMidiDurationDefs.Count.</para>
        /// <para>An Exception is thrown if any of these conditions is not met.</para>
        /// <para>If the partitions list is empty, or contains only one value (equal to LocalMidiDurationDefs.Count),</para>
        /// <para>the value is ignored, and this function returns silently without doing anything.</para>
        /// </param>
        /// <param name="contourNumber">A value greater than or equal to 1, and less than or equal to 12. An exception is thrown if this is not the case.</param>
        /// <param name="axisNumber">A value greater than or equal to 1, and less than or equal to 12 An exception is thrown if this is not the case.</param>
        internal void SetContour(List<int> partitionSizes, int contourNumber, int axisNumber)
        {
            CheckSetContourArgs(partitionSizes, contourNumber, axisNumber);
            List<LocalMidiDurationDef> oldLmdds = _localMidiDurationDefs;
            List<List<LocalMidiDurationDef>> partitions = new List<List<LocalMidiDurationDef>>();
            int lmddIndex = 0;
            foreach(int size in partitionSizes)
            {
                List<LocalMidiDurationDef> partition = new List<LocalMidiDurationDef>();
                for(int i = 0; i < size; ++i)
                {
                    partition.Add(oldLmdds[lmddIndex++]);
                }
            }
            
            // Sort into ascending order (part 2 of the above comment)
            partitions = SortPartitions(partitions);
            // re-order the partitions (part 3 of the above comment)
            partitions = DoContouring(partitions, contourNumber, axisNumber);

            List<LocalMidiDurationDef> newLmdds = new List<LocalMidiDurationDef>();
            foreach(List<LocalMidiDurationDef> partition in partitions)
            {
                foreach(LocalMidiDurationDef pLmdd in partition)
                {
                    newLmdds.Add(pLmdd);
                }
            }
            int msPosition = this.MsPosition;
            _localMidiDurationDefs = newLmdds;
            this.MsPosition = msPosition; // resets all the msPosition values.
        }

        /// <summary>
        /// Returns the result of sorting the partitions into ascending order of the base pitch of their first chord.
        /// <para>If a sub-MidiPhrase contains no chords, it is put at the beginning of the sort.</para>
        /// <para>If two sub-MidiPhrases have the same initial base pitch, they stay in the same order as they were.</para>
        /// </summary>
        private List<List<LocalMidiDurationDef>> SortPartitions(List<List<LocalMidiDurationDef>> partitions)
        {
            List<List<LocalMidiDurationDef>> sortedPartitions = new List<List<LocalMidiDurationDef>>();

            return sortedPartitions;
        }

        /// <summary>
        /// Re-orders the partitions according to the contour retrieved (from the static K.Contour[] array)
        /// <para>using partitions.Count and the contourNumber and axisNumber arguments.</para>
        /// <para>Does not change the inner contents of the partitions themselves.</para>
        /// </summary>
        /// <returns>A re-ordered list of partitions</returns>
        private List<List<LocalMidiDurationDef>> DoContouring(List<List<LocalMidiDurationDef>> partitions, int contourNumber, int axisNumber)
        {
            List<List<LocalMidiDurationDef>> contouredPartitions = new List<List<LocalMidiDurationDef>>();
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

        private void CheckSetContourArgs(List<int> partitions, int contourNumber, int axisNumber)
        {
            List<LocalMidiDurationDef> lmdds = _localMidiDurationDefs;
            if(partitions.Count < 0 || partitions.Count > 7)
            {
                throw new ArgumentException("partitions.Count must be in range 0..7");
            }
            int sum = 0;
            foreach(int i in partitions)
            {
                if(i < 0 || i > lmdds.Count)
                {
                    throw new ArgumentException("partition size out of range 1.." + lmdds.Count.ToString());
                }
                sum += i;
            }
            if(sum != lmdds.Count)
            {
                throw new ArgumentException("The sum of the partition sizes must be " + lmdds.Count.ToString());
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

        /// <summary>
        /// Returns 0 if msPosition is negative,
        /// Returns the index of the last LocalMidiDurationDef if msPosition is beyond the end of the sequence.
        /// </summary>
        /// <param name="msPosition"></param>
        /// <returns></returns>
        internal int FirstIndexAtOrAfterMsPos(int msPosition)
        {
            int index = _localMidiDurationDefs.Count - 1; // the final index
            for(int i = 0; i < _localMidiDurationDefs.Count; ++i)
            {
                if(_localMidiDurationDefs[i].MsPosition >= msPosition)
                {
                    index = i;
                    break;
                }
            }
            return index;
        }
        /// <summary>
        /// Transposes the LocalMidiDurationDefs from the firstGlissedIndex upto including the lastGlissedIndex
        /// by an equally increasing amount, so that the final LocalMidiDurationDef is transposed by glissInterval.
        /// firstGlissedIndex and lastGlissedIndex can be equal.
        /// glissInterval can be negative.
        /// </summary>
        internal void StepwiseGliss(int firstGlissedIndex, int lastGlissedIndex, int glissInterval)
        {
            Debug.Assert(firstGlissedIndex <= lastGlissedIndex);

            int nSteps = (lastGlissedIndex - firstGlissedIndex) + 1;
            double interval = ((double)glissInterval) / nSteps;
            double step = interval;
            for(int index = firstGlissedIndex; index <= lastGlissedIndex; ++index)
            {
                _localMidiDurationDefs[index].Transpose((int)Math.Round(interval));
                interval += step;
            }
        }

        // TODO
        // public void AdjustVelocity(int interval) // like Transpose()

        public List<LocalMidiDurationDef> LocalMidiDurationDefs { get { return _localMidiDurationDefs; } } 
        private List<LocalMidiDurationDef> _localMidiDurationDefs = new List<LocalMidiDurationDef>();

        #region Enumerator
        public IEnumerator GetEnumerator()
        {
            return new MyEnumerator(_localMidiDurationDefs);
        }
        // private enumerator class
        // see http://support.microsoft.com/kb/322022/en-us
        private class MyEnumerator : IEnumerator
        {
            public List<LocalMidiDurationDef> _localizedMidiDurationDefs;
            int position = -1;
            //constructor
            public MyEnumerator(List<LocalMidiDurationDef> localizedMidiDurationDefs)
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

    }
}
