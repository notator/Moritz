using Krystals5ObjectLibrary;

using Moritz.Spec;

using System;
using System.Collections.Generic;

namespace Moritz.Palettes
{
    /// <summary>
    /// Every Palette contains a private list of objects that can only be retrieved using the protected GetClonedValueAt(int index) function.
    /// The list must be set in the derived constructor using the SetValues(values) function.	
    /// </summary>
    public abstract class Palette<T> where T : ICloneable
    {
        public int Count
        {
            get => _values.Count;
        }

        /// <summary>
        /// This function must be called to set the private _values list. It can only be called once.
        /// </summary>
        protected void SetValues(List<T> values)
        {
            if(_values == null)
            {
                _values = values;
            }
            else
            {
                throw new ApplicationException("_values can only be set once.");
            }
        }

        /// <summary>
        /// Use this protected accessor function in derived classes to retrieve values from the private _values list.
        /// </summary>
        protected T GetClonedValueAt(int index)
        {
            return (T)_values[index].Clone();
        }

        private IReadOnlyList<T> _values = null;
    }

    public class TrkPalette : Palette<Trk>
    {
        public TrkPalette(List<Trk> trks)
        {
            SetValues(trks);
        }
    }

    public class EnvelopePalette : Palette<Envelope>
    {
        public EnvelopePalette(List<Envelope> envelopes)
        {
            SetValues(envelopes);
        }

        /// <summary>
        /// Returns a clone of the envelope at index.
        /// </summary>
        public Envelope GetEnvelope(int index)
        {
            Envelope envelope = GetClonedValueAt(index);

            return envelope;
        }
    }

    public class MidiChordDefPalette : Palette<MidiChordDef>
    {
        public MidiChordDefPalette(List<MidiChordDef> midiChordDefs)
        {
            SetValues(midiChordDefs);
        }

        /// <summary>
        /// Returns a clone of the MidiChordDef at index.
        /// </summary>
        public MidiChordDef GetMidiChordDef(int index)
        {
            MidiChordDef midiChordDef = GetClonedValueAt(index);

            return midiChordDef;
        }
    }
}
