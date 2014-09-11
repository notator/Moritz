
namespace Moritz.Score.Notation
{
    ///<summary>
    /// IUniqueSplittableChordDef is implemented by InputChordDef and MidiChordDef.
    ///</summary>
    public interface IUniqueSplittableChordDef : IUniqueChordDef
    {
        /// <summary>
        /// This field is set for a chordDef if it crosses a barline. Rests never cross barlines, they are always split.
        /// </summary>
        int? MsDurationToNextBarline { get; set; }
    }
}
