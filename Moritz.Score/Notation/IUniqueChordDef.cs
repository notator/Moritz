using System.Collections.Generic;

namespace Moritz.Score.Notation
{
    ///<summary>
    /// IUniqueChordDef is implemented by InputChordDef, MidiChordDef, UniqueCautionaryChordDef.
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

        List<byte> MidiPitches { get; set; }
    }
}
