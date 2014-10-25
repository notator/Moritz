
using System.Collections.Generic;

using Krystals4ObjectLibrary;
using Moritz.Globals;

namespace Moritz.Spec
{
    ///<summary>
    /// IUniqueChordDef is implemented by all IUniqueDef objects that have pitches.
    /// Currently (11.9.2014) these are:
    ///     MidiChordDef
    ///     InputChordDef
    ///     CautionaryChordDef
    ///</summary>
    public interface IUniqueChordDef: IUniqueDef
    {
        /// <summary>
        /// Transpose by the number of semitones given in the argument.
        /// Negative interval values transpose down.
        /// It is not an error if Midi values would exceed the range 0..127.
        /// In this case, they are silently coerced to 0 or 127 respectively.
        /// </summary>
        void Transpose(int interval);

        List<byte> NotatedMidiPitches { get; set; }
    }
}
