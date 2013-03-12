
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
    /// </summary>
    public class MidiMelodyDef : IEnumerable
    {
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

        public MidiMelodyDef(PaletteDef midiDurationDefs)
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
        /// Clone constructor.
        /// Note that new LocalizedMidiDurationDef objects are created, and that these contain cloned MidiChordDefs.
        /// Changing the content of a MidiChordDef will only change it in the particular MidiMelodyDef instance.
        /// However, LocalizedMidiDurationDef.MsPosition is independent of its MidiChordDef.
        /// LocalizedMidiDurationDef.MsDuration cannot be changed.
        /// </summary>
        /// <param name="mdsd"></param>
        public MidiMelodyDef(MidiMelodyDef mdsd)
        {
            foreach(LocalizedMidiDurationDef lmdd in mdsd)
            {
                MidiChordDef mcd = new MidiChordDef(lmdd.MidiChordDef, lmdd.MsDuration);
                LocalizedMidiDurationDef newLmdd = new LocalizedMidiDurationDef(mcd);
                _localizedMidiDurationDefs.Add(newLmdd);
            }
            MsPosition = _localizedMidiDurationDefs[0].MsPosition; // sets the absolute position of all notes and rest definitions
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
        /// The duration of this MidiMelodyDef in milliseconds.
        /// Setting this.MsDuration does not change this.MsPosition, but moves this.EndMsPosition. 
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

                int msDuration = value;
                float factor = ((float)msDuration / (float)MsDuration);
                MidiChordDef firstMCD = null;
                foreach(LocalizedMidiDurationDef lmdd in _localizedMidiDurationDefs) 
                {
                    firstMCD = lmdd.MidiChordDef;
                    if(firstMCD != null)
                        break;
                }
                Debug.Assert(firstMCD != null);
                List<int> newDurations = firstMCD.GetIntDurations(msDuration, relativeDurations, relativeDurations.Count, factor);

                List<LocalizedMidiDurationDef> newLmdds = new List<LocalizedMidiDurationDef>();
                for(int i = 0; i < relativeDurations.Count; ++i )
                {
                    LocalizedMidiDurationDef lmdd = _localizedMidiDurationDefs[i];
                    MidiChordDef mcd = new MidiChordDef(lmdd.MidiChordDef, newDurations[i]);
                    newLmdds.Add(new LocalizedMidiDurationDef(mcd));
                }
                _localizedMidiDurationDefs = newLmdds;
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
                foreach(LocalizedMidiDurationDef lmdd in _localizedMidiDurationDefs)
                {
                    lmdd.MsPosition = absolutePosition;
                    absolutePosition += lmdd.MsDuration;
                }
            } 
        }
        /// <summary>
        /// The absolute position of the end of the last note or rest in the sequence.
        /// Setting this.EndMsPosition, sets this.MsDuration without changing this.MsPosition.
        /// Setting this.EndMsPosition will fail if EndMsPosition is not greater than MsPosition. 
        /// </summary>
        public int EndMsPosition 
        { 
            get 
            {
                Debug.Assert(_localizedMidiDurationDefs.Count > 0);
                LocalizedMidiDurationDef lastLmdd = _localizedMidiDurationDefs[_localizedMidiDurationDefs.Count - 1];
                return lastLmdd.MsPosition + lastLmdd.MsDuration; 
            }
            set
            {
                int endMsPosition = value; 
                Debug.Assert(endMsPosition > MsPosition);
                int msDuration = endMsPosition - MsPosition;
                MsDuration = msDuration; // sets the durations of all the contained note and rest defs
            }
        }

        public IEnumerator GetEnumerator()
        {
            return new MyEnumerator(_localizedMidiDurationDefs);
        }

        private List<LocalizedMidiDurationDef> _localizedMidiDurationDefs = new List<LocalizedMidiDurationDef>();
    }
}
