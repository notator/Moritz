
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
    /// <para>foreach(LocalMidiDurationDef lmd in voiceDef) { ... }</para>
    /// <para>It is also indexable, as in:</para>
    /// <para>LocalMidiDurationDef lmdd = voiceDef[index];</para>
    /// </summary>
    public class VoiceDef : IEnumerable
    {
        /// <summary>
        /// <para>If the argument is not empty, the MsPositions and MsDurations in the list are checked for consistency.</para>
        /// <para>The new VoiceDef's LocalMidiDurationDefs list is simply set to the argument (which is not cloned).</para>
        /// </summary>
        public VoiceDef(List<LocalMidiDurationDef> lmdds)
        {
            Debug.Assert(lmdds != null);
            if(lmdds.Count > 0)
            {
                for(int i = 1; i < lmdds.Count; ++i)
                {
                    Debug.Assert(lmdds[i - 1].MsPosition + lmdds[i - 1].MsDuration == lmdds[i].MsPosition);
                }
            }
            _localMidiDurationDefs = lmdds;
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
                LocalMidiDurationDef lmdd = new LocalMidiDurationDef(midiDurationDef);
                _localMidiDurationDefs.Add(lmdd);
            }
            MsPosition = _localMidiDurationDefs[0].MsPosition; // sets the absolute position of all notes and rests
        }

        /// <summary>
        /// Returns a deep clone of this VoiceDef.
        /// </summary>
        public VoiceDef Clone()
        {
            List<LocalMidiDurationDef> clonedLmdds = new List<LocalMidiDurationDef>();
            foreach(LocalMidiDurationDef lmdd in this._localMidiDurationDefs)
            {
                LocalMidiDurationDef clonedLmdd = new LocalMidiDurationDef(lmdd.UniqueMidiDurationDef, lmdd.MsPosition, lmdd.MsDuration);
                clonedLmdds.Add(clonedLmdd);
            }

            return new VoiceDef(clonedLmdds);
        }

        /// <summary>
        /// Rests dont have lyrics, so their index in the VoiceDef can't be shown as a lyric.
        /// Overridden by Clytemnestra, where the index is inserted before her lyrics.
        /// </summary>
        /// <param name="voiceDef"></param>
        public virtual void SetLyricsToIndex()
        {
            for(int index = 0; index < _localMidiDurationDefs.Count; ++index)
            {
                UniqueMidiChordDef lmcd = _localMidiDurationDefs[index].UniqueMidiDurationDef as UniqueMidiChordDef;
                if(lmcd != null)
                {
                    lmcd.Lyric = index.ToString();
                }
            }
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
        /// The total duration of this VoiceDef in milliseconds.
        /// Setting this.MsDuration stretches or compresses all the durations in the list to fit the given total duration.
        /// It does not change this.MsPosition, but does affect this.EndMsPosition. 
        /// </summary>
        public int MsDuration
        {
            get
            {
                return this.EndMsPosition;
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
        /// Indexer. Allows individual lmdds to be accessed using array notation on the VoiceDef.
        /// e.g. lmdd = voiceDef[3].
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
        /// Transpose all the lmdds from startIndex to (not including) endIndex
        /// up by the number of semitones given in the inteval argument.
        /// Negative interval values transpose down.
        /// It is not an error if Midi pitch values would exceed the range 0..127.
        /// In this case, they are silently coerced to 0 or 127 respectively.
        /// </summary>
        /// <param name="interval"></param>
        public void Transpose(int startIndex, int endIndex, int interval)
        {
            for(int i = startIndex; i < endIndex; ++i)
            {
                LocalMidiDurationDef lmdd = _localMidiDurationDefs[i];
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

        #region SetContour()
        /// <summary>
        /// Re-orders the LocalMidiDurationDefs in (part of) this VoiceDef.
        /// <para>1. creates partitions (lists of LocalMidiDurationDefs) using the startAtIndex and partitionSizes in the first two</para>
        /// <para>-  arguments (see parameter info below).</para>   
        /// <para>2. Sorts the partitions into ascending order of their lowest pitches.
        /// <para>3. Re-orders the partitions according to the contour retrieved (from the static K.Contour[] array) using</para>
        /// <para>-  the contourNumber and axisNumber arguments.</para>
        /// <para>4. Concatenates the re-ordered partitions, re-sets their MsPositions, and replaces the LocalMidiDurationDefs in</para>
        /// <para>-  the original List with the result.</para>
        /// <para>5. If setLyricsToIndex is true, sets the lyrics to the new indices</para>
        /// <para>SpecialCases:</para>
        /// <para>If a partition contains a single LocalMidiRestDef, it stays where it is and the other LocalMidiDurationDefs are</para>
        /// <para>re-ordered around it.</para>
        /// <para>If two sub-VoiceDefs have the same initial base pitch, they stay in the same order as they were (not necessarily</para>
        /// <para>together, of course.)</para>
        /// </summary>
        /// <param name="startAtIndex">The index in LocalMidiDurationDefs at which to start the re-ordering.
        /// </param>
        /// <param name="partitionSizes">The number of LocalMidiDurationDefs in each sub-voiceDef to be re-ordered.
        /// <para>This partitionSizes list must contain:</para>
        /// <para>    1..7 int sizes.</para>
        /// <para>    sizes which are all greater than 0.</para>
        /// <para>    The sum of all the sizes + startAtIndex must be less than or equal to LocalMidiDurationDefs.Count.</para>
        /// <para>An Exception is thrown if any of these conditions is not met.</para>
        /// <para>If the partitions list contains only one value, this function returns silently without doing anything.</para>
        /// </param>
        /// <param name="contourNumber">A value greater than or equal to 1, and less than or equal to 12. An exception is thrown if this is not the case.</param>
        /// <param name="axisNumber">A value greater than or equal to 1, and less than or equal to 12 An exception is thrown if this is not the case.</param>
        internal void SetContour(int startAtIndex, List<int> partitionSizes, int contourNumber, int axisNumber)
        {
            CheckSetContourArgs(startAtIndex, partitionSizes, contourNumber, axisNumber);

            List<List<LocalMidiDurationDef>> partitions = GetPartitions(startAtIndex, partitionSizes);

            // Remove any partitions (from the partitions list) that contain only a single LocalMidiRestDef
            // Store them (the rests), with their original partition indices, in the returned list of KeyValuePairs.
            List<KeyValuePair<int, List<LocalMidiDurationDef>>> restPartitions = GetRestPartitions(partitions);

            if(partitions.Count > 1)
            {
                // All the partitions now contain at least one LocalMidiChordDef
                // Sort into ascending order (part 2 of the above comment)
                partitions = SortPartitions(partitions);
                // re-order the partitions (part 3 of the above comment)
                partitions = DoContouring(partitions, contourNumber, axisNumber);
            }

            RestoreRestPartitions(partitions, restPartitions);

            List<LocalMidiDurationDef> sortedLmdds = ConvertPartitionsToFlatLmdds(startAtIndex, partitions);

            for(int i = 0; i < sortedLmdds.Count; ++i)
            {
                _localMidiDurationDefs[startAtIndex + i] = sortedLmdds[i];
            }            
        }

        private List<LocalMidiDurationDef> ConvertPartitionsToFlatLmdds(int startAtIndex, List<List<LocalMidiDurationDef>> partitions)
        {
            List<LocalMidiDurationDef> newLmdds = new List<LocalMidiDurationDef>();
            int msPosition = _localMidiDurationDefs[startAtIndex].MsPosition;
            foreach(List<LocalMidiDurationDef> partition in partitions)
            {
                foreach(LocalMidiDurationDef pLmdd in partition)
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
        private void RestoreRestPartitions(List<List<LocalMidiDurationDef>> partitions, List<KeyValuePair<int, List<LocalMidiDurationDef>>> restPartitions)
        {
            for(int i = restPartitions.Count - 1; i >= 0; --i)
            {
                KeyValuePair<int, List<LocalMidiDurationDef>> kvp = restPartitions[i];
                partitions.Insert(kvp.Key, kvp.Value);
            }
        }

        private List<List<LocalMidiDurationDef>> GetPartitions(int startAtIndex, List<int> partitionSizes)
        {
            List<List<LocalMidiDurationDef>> partitions = new List<List<LocalMidiDurationDef>>();
            int lmddIndex = startAtIndex;
            foreach(int size in partitionSizes)
            {
                List<LocalMidiDurationDef> partition = new List<LocalMidiDurationDef>();
                for(int i = 0; i < size; ++i)
                {
                    partition.Add(_localMidiDurationDefs[lmddIndex++]);
                }
                partitions.Add(partition);
            }
            return partitions;
        }

        /// <summary>
        /// Remove any partitions (from partitions) that contain only a single LocalMidiRestDef.
        /// Store them, with their original partition indices, in the returned list of KeyValuePairs.
        /// </summary>
        private List<KeyValuePair<int, List<LocalMidiDurationDef>>> GetRestPartitions(List<List<LocalMidiDurationDef>> partitions)
        {
            List<List<LocalMidiDurationDef>> newPartitions = new List<List<LocalMidiDurationDef>>();
            List<KeyValuePair<int, List<LocalMidiDurationDef>>> restPartitions = new List<KeyValuePair<int, List<LocalMidiDurationDef>>>();
            for(int i = 0; i < partitions.Count; ++i)
            {
                List<LocalMidiDurationDef> partition = partitions[i];
                if(partition.Count == 1 && partition[0].UniqueMidiDurationDef is UniqueMidiRestDef)
                {
                    restPartitions.Add(new KeyValuePair<int, List<LocalMidiDurationDef>>(i, partition));
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
        private List<List<LocalMidiDurationDef>> SortPartitions(List<List<LocalMidiDurationDef>> partitions)
        {
            List<byte> lowestPitches = GetLowestPitches(partitions);
            List<byte> sortedLowestPitches = new List<byte>(lowestPitches);
            sortedLowestPitches.Sort();

            List<List<LocalMidiDurationDef>> sortedPartitions = new List<List<LocalMidiDurationDef>>();
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
        private List<byte> GetLowestPitches(List<List<LocalMidiDurationDef>> partitions)
        {
            List<byte> lowestPitches = new List<byte>();
            foreach(List<LocalMidiDurationDef> partition in partitions)
            {
                byte lowestPitch = byte.MaxValue;
                foreach(LocalMidiDurationDef lmdd in partition)
                {
                    UniqueMidiChordDef umcd = lmdd.UniqueMidiDurationDef as UniqueMidiChordDef;
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
        /// <summary>
        /// Throws an exception if one of the following conditions is not met.
        /// <para>startAtIndex is a valid index in the LocalMidiDurationDefs list</para>
        /// <para>partitionSizes.Count is greater than 0, and less than 8.</para>
        /// <para>all sizes in partitionSizes are greater then 0.</para>
        /// <para>the sum of startAtIndex plus all the partition sizes is not greater than LocalMidiDurationDefs.Count</para>
        /// <para>contourNumber is in the range 1..12</para>
        /// <para>axisNumber is in the range 1..12</para>
        /// </summary>
        private void CheckSetContourArgs(int startAtIndex, List<int> partitionSizes, int contourNumber, int axisNumber)
        {
            List<LocalMidiDurationDef> lmdds = _localMidiDurationDefs;
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
                    throw new ArgumentException("partitions must contain at least one LocalMidiDurationDef");
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
        #endregion

        /// <summary>
        /// Extracts nLocalMidiDurationDefs from the LocalMididurationDefs, and then inserts them again at the toIndex.
        /// </summary>
        internal void Translate(int fromIndex, int nLocalMidiDurationDefs, int toIndex)
        {
            Debug.Assert((fromIndex + nLocalMidiDurationDefs) <= _localMidiDurationDefs.Count);
            Debug.Assert(toIndex <= (_localMidiDurationDefs.Count - nLocalMidiDurationDefs));
            int msPosition = _localMidiDurationDefs[0].MsPosition; 
            List<LocalMidiDurationDef> extractedLmdds = _localMidiDurationDefs.GetRange(fromIndex, nLocalMidiDurationDefs);
            _localMidiDurationDefs.RemoveRange(fromIndex, nLocalMidiDurationDefs);
            _localMidiDurationDefs.InsertRange(toIndex, extractedLmdds);
            MsPosition = msPosition; // resets all the positions in this VoiceDef.
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
        /// Transposes the LocalMidiDurationDefs from the startIndex upto (but not including) endIndex
        /// by an equally increasing amount, so that the final LocalMidiDurationDef is transposed by glissInterval.
        /// startIndex must be less than endIndex.
        /// glissInterval can be negative.
        /// </summary>
        internal void StepwiseGliss(int startIndex, int endIndex, int glissInterval)
        {
            Debug.Assert(startIndex < endIndex);

            int nSteps = (endIndex - startIndex);
            double interval = ((double)glissInterval) / nSteps;
            double step = interval;
            for(int index = startIndex; index < endIndex; ++index)
            {
                _localMidiDurationDefs[index].Transpose((int)Math.Round(interval));
                interval += step;
            }
        }

        // TODO
        // public void AdjustVelocity(int interval) // like Transpose()

        public List<LocalMidiDurationDef> LocalMidiDurationDefs { get { return _localMidiDurationDefs; } }
        protected List<LocalMidiDurationDef> _localMidiDurationDefs = new List<LocalMidiDurationDef>();

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
