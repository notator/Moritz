using System.Collections.Generic;
using System.Diagnostics;

namespace Moritz.Score.Notation
{
    ///<summary>
    /// IUniqueDef is implemented by all Unique...Def objects.
    ///</summary>
    public interface IUniqueDef
    {
        string ToString();

        /// <summary>
        /// Returns a deep clone (a unique object)
        /// </summary>
        IUniqueDef DeepClone();

        void AdjustDuration(double factor);

        int MsDuration { get; set; }
        int MsPosition { get; set; }
    }
}
