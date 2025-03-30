
using System.Collections.Generic;

namespace Moritz.Spec
{
    ///<summary>
    /// IUniqueChordDef is implemented by all IUniqueDef objects that have pitches.
    /// Currently (11.9.2014) these are:
    ///     ChordDef
    ///     InputChordDef
    ///     CautionaryChordDef
    ///</summary>
    public interface IUniqueChordDef : IUniqueDef
    {
        /// <summary>
        /// Transpose by the number of semitones given in the argument.
        /// Negative interval values transpose down.
        /// It is not an error if Midi values would exceed the range 0..127.
        /// In this case, they are silently coerced to 0 or 127 respectively.
        /// </summary>
        void Transpose(int interval);

        List<int> Pitches { get; set; }
        List<int> Velocities { get; set; }
    }
}
