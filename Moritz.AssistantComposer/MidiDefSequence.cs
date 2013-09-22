
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
    /// </summary>
    public class MidiDefSequence : IEnumerable
    {
        /// <summary>
        /// The argument may not be an empty list.
        /// The MsPositions and MsDurations in the list are checked for consistency.
        /// </summary>
        /// <param name="lmdds"></param>
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
        /// <param name="paletteDef"></param>
        /// <param name="sequence"></param>
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
        /// </summary>
        /// <param name="midiDurationDefs"></param>
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
        /// <param name="midiDefSequence"></param>
        /// <returns></returns>
        public MidiDefSequence Clone()
        {
            List<LocalizedMidiDurationDef> clonedLmdds = new List<LocalizedMidiDurationDef>();
            foreach(LocalizedMidiDurationDef lmdd in this.LocalizedMidiDurationDefs)
            {
                LocalizedMidiDurationDef clonedLmdd = new LocalizedMidiDurationDef(lmdd.LocalMidiChordDef, lmdd.MsPosition, lmdd.MsDuration);
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
