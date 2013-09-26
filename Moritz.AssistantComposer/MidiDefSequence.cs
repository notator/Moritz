
using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections;
using Moritz.Score.Midi;

namespace Moritz.AssistantComposer
{
    /// <summary>
    /// A temporal sequence of LocalizedMidiDurationDef objects.
    /// The objects can define either notes or rests.
    /// (If the LocalizedMidiDurationDef.MidiChordDef is null, the contained object is a rest.)
    /// 
    /// This class is IEnumerable, so that foreach loops can be used.
    /// For example:
    ///     foreach(LocalizedMidiDurationDef lmd in midiDefList)
    ///     {
    ///         ...
    ///     }
    /// It is also indexable, as in:
    ///     LocalizedMidiDurationDef lmdd = midiDefList[index];
    /// </summary>
    public class MidiDefSequence : IEnumerable
    {
        /// <summary>
        /// The argument may not be an empty list.
        /// The MsPositions and MsDurations in the list are checked for consistency.
        /// </summary>
        public MidiDefSequence(List<LocalizedMidiDurationDef> lmdds)
        {
            Debug.Assert(lmdds.Count > 0);
            for(int i = 1; i < lmdds.Count; ++i)
            {
                Debug.Assert(lmdds[i - 1].MsPosition + lmdds[i - 1].MsDuration == lmdds[i].MsPosition);
            }
            _localizedMidiDurationDefs = lmdds;
        }

        /// <summary>
        /// sequence contains a list of values in range 1..paletteDef.MidiDurationDefsCount.
        /// </summary>
        public MidiDefSequence(PaletteDef paletteDef, List<int> sequence)
        {
            int msPosition = 0;

            foreach(int value in sequence)
            {
                Debug.Assert((value >= 1 && value <= paletteDef.MidiDurationDefsCount), "Illegal argument: value out of range in sequence");
                MidiDurationDef midiDurationDef = paletteDef[value - 1];
                LocalizedMidiDurationDef noteDef = new LocalizedMidiDurationDef(midiDurationDef);
                Debug.Assert(midiDurationDef.MsDuration > 0);
                Debug.Assert(noteDef.MsDuration == midiDurationDef.MsDuration);
                noteDef.MsPosition = msPosition;
                msPosition += noteDef.MsDuration;
                _localizedMidiDurationDefs.Add(noteDef);
                //Console.WriteLine("MsPosition=" + noteDef.MsPosition.ToString() + "  MsDuration=" + noteDef.MsDuration.ToString());
            }
        }

        /// <summary>
        /// Constructs a MidiDefList at MsPosition=0, containing the localized sequence of MidiDurationDefs in the PaletteDef.
        /// </summary
        public MidiDefSequence(PaletteDef midiDurationDefs)
        {
            Debug.Assert(midiDurationDefs != null);
            foreach(MidiDurationDef midiDurationDef in midiDurationDefs)
            {
                LocalizedMidiDurationDef lmdd = new LocalizedMidiDurationDef(midiDurationDef);
                _localizedMidiDurationDefs.Add(lmdd);
            }
            MsPosition = _localizedMidiDurationDefs[0].MsPosition; // sets the absolute position of all notes and rests
        }

        /// <summary>
        /// Returns a deep clone of this MidiDefSequence.
        /// </summary>
        public MidiDefSequence Clone()
        {
            List<LocalizedMidiDurationDef> clonedLmdds = new List<LocalizedMidiDurationDef>();
            foreach(LocalizedMidiDurationDef lmdd in this.LocalizedMidiDurationDefs)
            {
                LocalizedMidiDurationDef clonedLmdd = new LocalizedMidiDurationDef(lmdd.LocalMidiDurationDef, lmdd.MsPosition, lmdd.MsDuration);
                clonedLmdds.Add(clonedLmdd);
            }

            return new MidiDefSequence(clonedLmdds);
        }

        public void Add(LocalizedMidiDurationDef lmdd)
        {
            Debug.Assert(_localizedMidiDurationDefs.Count > 0);
            LocalizedMidiDurationDef lastLmdd = _localizedMidiDurationDefs[_localizedMidiDurationDefs.Count - 1];
            lmdd.MsPosition = lastLmdd.MsPosition + lastLmdd.MsDuration;
            _localizedMidiDurationDefs.Add(lmdd);
        }
        public void Remove(LocalizedMidiDurationDef lmdd)
        {
            Debug.Assert(_localizedMidiDurationDefs.Count > 0);
            Debug.Assert(_localizedMidiDurationDefs.Contains(lmdd));
            _localizedMidiDurationDefs.Remove(lmdd);
            MsPosition = _localizedMidiDurationDefs[0].MsPosition; // sets the absolute position of all notes and rests 
        }
        /// <summary>
        /// The duration of this MidiDefList in milliseconds.
        /// Setting this.MsDuration stretches or compresses all the durations in the list to fit the given total duration.
        /// It does not change this.MsPosition, but does affect this.EndMsPosition. 
        /// </summary>
        public int MsDuration
        {
            get
            {
                int msDuration = 0;
                foreach(LocalizedMidiDurationDef lmdd in _localizedMidiDurationDefs)
                {
                    msDuration += lmdd.MsDuration;
                }
                return msDuration;
            }
            set
            {
                List<int> relativeDurations = new List<int>();
                foreach(LocalizedMidiDurationDef lmdd in _localizedMidiDurationDefs)
                {
                    relativeDurations.Add(lmdd.MsDuration);
                }

                int newMsDuration = value;
                Debug.Assert(newMsDuration > 0);
                List<int> newDurations = MidiChordDef.GetIntDurations(newMsDuration, relativeDurations, relativeDurations.Count);

                int i = 0;
                foreach (LocalizedMidiDurationDef lmdd in _localizedMidiDurationDefs)
                {
                    lmdd.MsDuration = newDurations[i++];
                }

                MsPosition = _localizedMidiDurationDefs[0].MsPosition; // sets the absolute position of all notes and rest definitions
            }
        }
        /// <summary>
        /// The absolute position of the first note or rest in the sequence.
        /// Setting this value sets the absolute position of each note and
        /// rest in the sequence, without changing their durations.
        /// </summary>
        public int MsPosition 
        { 
            get 
            { 
                Debug.Assert(_localizedMidiDurationDefs.Count > 0);
                return _localizedMidiDurationDefs[0].MsPosition;
            } 
            set 
            {
                Debug.Assert(_localizedMidiDurationDefs.Count > 0);
                int absolutePosition = value;
                Debug.Assert(absolutePosition >= 0);
                foreach(LocalizedMidiDurationDef lmdd in _localizedMidiDurationDefs)
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
                Debug.Assert(_localizedMidiDurationDefs.Count > 0);
                LocalizedMidiDurationDef lastLmdd = _localizedMidiDurationDefs[_localizedMidiDurationDefs.Count - 1];
                return lastLmdd.MsPosition + lastLmdd.MsDuration; 
            }
        }
        public int Count { get { return _localizedMidiDurationDefs.Count; } }
        public LocalizedMidiDurationDef this[int i]
        {
            get
            {
                if(i < 0 || i >= _localizedMidiDurationDefs.Count)
                {
                    throw new IndexOutOfRangeException();
                }
                return _localizedMidiDurationDefs[i];
            }
            set
            {
                if(i < 0 || i >= _localizedMidiDurationDefs.Count)
                {
                    throw new IndexOutOfRangeException();
                }
                _localizedMidiDurationDefs[i] = value;
            }
        }
        /// <summary>
        /// Transpose the MidiDefSequence up by the number of semitones given in the argument.
        /// Negative interval values transpose down.
        /// It is not an error if Midi pitch values would exceed the range 0..127.
        /// In this case, they are silently coerced to 0 or 127 respectively.
        /// </summary>
        /// <param name="interval"></param>
        public void Transpose(int interval)
        {
            foreach(LocalizedMidiDurationDef lmdd in _localizedMidiDurationDefs)
            {
                lmdd.Transpose(interval);
            }
        }

        /// <summary>
        /// _localizedMidiDurationDefs[alignIndex] (=lmddAlign) is moved to MsPosition, and the surrounding symbols are spread accordingly
        /// between those at anchor1Index and anchor2Index. The symbols at anchor1Index (=lmddA1) and anchor2Index (=lmddA2)do not move.
        /// An exception is thrown if an attempt is made to move any lmdd which is aligned to a barline.
        /// barMsPositions contains the msPositions of all barlines except the last. (begins with 0)
        /// Note that alignIndex cannot be 0,
        /// and that anchor2Index CAN be equal to _localizedMidiDurationDefs.Count (i.e.on the final barline).
        /// </summary>
        internal void AlignChordOrRest(List<int> barMsPositions, int anchor1Index, int alignIndex, int anchor2Index, int toMsPosition)
        {
            // throws an exception if there's a problem.
            CheckAlignDefArgs(barMsPositions, anchor1Index, alignIndex, anchor2Index, toMsPosition);

            List<LocalizedMidiDurationDef> lmdds = _localizedMidiDurationDefs;
            int anchor1MsPosition = lmdds[anchor1Index].MsPosition;
            int fromMsPosition = lmdds[alignIndex].MsPosition;
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
            for(int i = anchor1Index + 1; i < alignIndex; ++i)
            {
                lmdds[i].MsPosition = anchor1MsPosition + ((int)((lmdds[i].MsPosition - anchor1MsPosition) * leftFactor));           
            }

            lmdds[alignIndex].MsPosition = toMsPosition;

            float rightFactor = (float)(((float)(anchor2MsPosition - toMsPosition)) / ((float)(anchor2MsPosition - fromMsPosition)));
            for(int i = anchor2Index - 1; i > alignIndex; --i)
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

        private void CheckAlignDefArgs(List<int> barMsPositions, int anchor1Index, int alignIndex, int anchor2Index, int toMsPosition)
        {
            List<LocalizedMidiDurationDef> lmdds = _localizedMidiDurationDefs; 
            int count = lmdds.Count;
            string msg = "\nError in MidiDefSequence.cs,\nfunction AlignDefMsPosition()\n\n";
            if(alignIndex == 0)
            {
                throw new Exception(msg + "Can't move chord or rest at index 0.\n" +
                    "\ncount=" + count.ToString() +
                    "\nanchor1Index=" + anchor1Index.ToString() +
                    "\nalignIndex=" + alignIndex.ToString() +
                    "\nanchor2Index=" + anchor2Index.ToString());
            }
            if(anchor1Index >= count || alignIndex >= count || anchor2Index > count) // anchor2Index can be at the final barline (=count)!
            {
                throw new Exception(msg + "Index out of range.\n" +
                    "\ncount=" + count.ToString() +
                    "\nanchor1Index=" + anchor1Index.ToString() +
                    "\nalignIndex=" + alignIndex.ToString() +
                    "\nanchor2Index=" + anchor2Index.ToString());
            }
            if(anchor1Index >= alignIndex || anchor2Index <= alignIndex)
            {
                throw new Exception(msg + "Index out of order.\n" +
                    "\nanchor1Index=" + anchor1Index.ToString() +
                    "\nalignIndex=" + alignIndex.ToString() +
                    "\nanchor2Index=" + anchor2Index.ToString());
            }
            for(int index = anchor1Index + 1; index < anchor2Index; ++index)
            {
                if(index < lmdds.Count && barMsPositions.Contains(lmdds[index].MsPosition))
                {
                    throw new Exception(msg + "Can't move chord or rest attached to a barline.\n" +
                        "\nindex=" + index.ToString());
                }
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
                    "\nalignIndex=" + alignIndex.ToString() +
                    "\nanchor2Index=" + anchor2Index.ToString() +
                    "\ntoMsPosition=" + toMsPosition.ToString());  
            }
        }

        // TODO
        // public void AdjustVelocity(int interval) // like Transpose()

        public List<LocalizedMidiDurationDef> LocalizedMidiDurationDefs { get { return _localizedMidiDurationDefs; } } 
        private List<LocalizedMidiDurationDef> _localizedMidiDurationDefs = new List<LocalizedMidiDurationDef>();

        #region Enumerator
        public IEnumerator GetEnumerator()
        {
            return new MyEnumerator(_localizedMidiDurationDefs);
        }
        // private enumerator class
        // see http://support.microsoft.com/kb/322022/en-us
        private class MyEnumerator : IEnumerator
        {
            public List<LocalizedMidiDurationDef> _localizedMidiDurationDefs;
            int position = -1;
            //constructor
            public MyEnumerator(List<LocalizedMidiDurationDef> localizedMidiDurationDefs)
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
