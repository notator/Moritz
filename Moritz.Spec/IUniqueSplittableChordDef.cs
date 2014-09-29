
using Krystals4ObjectLibrary;
using Moritz.Globals;

namespace Moritz.Spec
{
    ///<summary>
    /// IUniqueSplittableChordDef is implemented by all IUniqueChordDef objects that can be split across barlines.
    /// Currently (11.9.2014) these are:
    ///     MidiChordDef
    ///     InputChordDef
    /// Note that Rests never cross barlines, they are always split.
    ///</summary>
    public interface IUniqueSplittableChordDef : IUniqueChordDef
    {
        /// <summary>
        /// This field is set if the IUniqueSplittableChordDef crosses a barline.
        /// </summary>
        int? MsDurationToNextBarline { get; set; }
        string Lyric { get; set; }
    }
}
