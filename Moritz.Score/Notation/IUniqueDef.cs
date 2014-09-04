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

        void AdjustDuration(double factor);

        int MsDuration { get; set; }
        int MsPosition { get; set; }
    }
}
