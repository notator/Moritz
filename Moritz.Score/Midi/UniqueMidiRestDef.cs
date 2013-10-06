using System.Collections.Generic;
using System.Diagnostics;

using Multimedia.Midi;

using Moritz.Globals;

namespace Moritz.Score.Midi
{
    ///<summary>
    /// A UniqueMidiRestDef is a unique MidiRestDef which is saved locally in an SVG file.
    /// (This class is necessary, because palettes can also contain rests - which should never be changed.)
    /// Related classes:
    /// 1. A PaletteMidiChordDef is a MidiChordDef which is saved in or retreived from a palette.
    /// PaletteMidiChordDefs can be 'used' in SVG files, but are usually converted to UniqueMidiChordDef.
    /// 2. A LocalizedMidiChordDef is a UniqueMidiChordDef with additional MsPositon and msDuration attributes.
    ///<summary>
    public class UniqueMidiRestDef : MidiRestDef
    {
        /// <summary>
        /// Note that rest IDs in SVG files are of the form "rest"+uniqueNumber, and
        /// that the uniqueNumber is allocated at the time when the SVG file is written.
        /// The null id passed to the base class here should always be ignored.
        /// </summary>
        /// <param name="midiRestDef"></param>
        public UniqueMidiRestDef(MidiRestDef midiRestDef)
            :base(null, midiRestDef.MsDuration)
        {     
        }
    }
}
