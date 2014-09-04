
using System.Diagnostics;

namespace Moritz.Score.Notation
{
    /// <summary>
    /// Implemented by MidiChordDef, a InputChordDef or a RestDef
    /// </summary>
    public interface IUniqueCloneDef
    {
        /// <summary>
        /// Returns a deep clone (a unique object)
        /// </summary>
        /// <returns></returns>
        IUniqueDef DeepClone();
    }
}
