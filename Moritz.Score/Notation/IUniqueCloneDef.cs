
using System.Diagnostics;

namespace Moritz.Score.Notation
{
    /// <summary>
    /// Implemented by UniqueMidiChordDef, a UniqueInputChordDef or a UniqueRestDef
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
