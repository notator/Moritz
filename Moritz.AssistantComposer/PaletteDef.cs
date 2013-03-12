using System.Collections.Generic;
using System.Collections;
using System;

using Moritz.Score.Midi;

namespace Moritz.AssistantComposer
{
    public class PaletteDef : IEnumerable
    {
        private List<MidiDurationDef> _midiDurationDefs = null;
        public PaletteDef(List<MidiDurationDef> midiDurationDefs)
        {
            _midiDurationDefs = new List<MidiDurationDef>();

            foreach(MidiDurationDef midiDurationDef in midiDurationDefs)
            {
                _midiDurationDefs.Add(midiDurationDef);
            }
        }

        public MidiDurationDef this[int i] { get { return _midiDurationDefs[i]; } }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (IEnumerator)GetEnumerator();
        }

        public PaletteEnum GetEnumerator()
        {
            return new PaletteEnum(_midiDurationDefs);
        }
    }

    public class PaletteEnum : IEnumerator
    {
        public List<MidiDurationDef> MidiDurationDefs;

        // Enumerators are positioned before the first element
        // until the first MoveNext() call.
        int position = -1;

        public PaletteEnum(List<MidiDurationDef> midiDurationDefs)
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

        public MidiDurationDef Current
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