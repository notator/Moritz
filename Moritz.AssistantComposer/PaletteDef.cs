using System.Collections.Generic;
using System.Collections;
using System;

using Krystals4ObjectLibrary;
using Moritz.Globals;
using Moritz.Krystals;
using Moritz.Score;
using Moritz.Score.Midi;
using Moritz.Score.Notation;
using Moritz.AssistantPerformer;

namespace Moritz.AssistantComposer
{
    public class PaletteDef : IEnumerable
    {
        public PaletteDef(List<DurationDef> midiDurationDefs)
        {
            _midiDurationDefs = new List<DurationDef>();

            foreach(DurationDef midiDurationDef in midiDurationDefs)
            {
                _midiDurationDefs.Add(midiDurationDef);
            }
        }

        public DurationDef this[int i] { get { return _midiDurationDefs[i]; } }
        public int MidiDurationDefsCount { get { return _midiDurationDefs.Count; } }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (IEnumerator)GetEnumerator();
        }

        public PaletteEnum GetEnumerator()
        {
            return new PaletteEnum(_midiDurationDefs);
        }

        private List<DurationDef> _midiDurationDefs = null;
    }

    public class PaletteEnum : IEnumerator
    {
        public List<DurationDef> MidiDurationDefs;

        // Enumerators are positioned before the first element
        // until the first MoveNext() call.
        int position = -1;

        public PaletteEnum(List<DurationDef> midiDurationDefs)
        {
            MidiDurationDefs = midiDurationDefs;
        }

        public bool MoveNext()
        {
            position++;
            return (position < MidiDurationDefs.Count);
        }

        public void Reset()
        {
            position = -1;
        }

        object IEnumerator.Current
        {
            get
            {
                return Current;
            }
        }

        public DurationDef Current
        {
            get
            {
                try
                {
                    return MidiDurationDefs[position];
                }
                catch(IndexOutOfRangeException)
                {
                    throw new InvalidOperationException();
                }
            }
        }
    }
}