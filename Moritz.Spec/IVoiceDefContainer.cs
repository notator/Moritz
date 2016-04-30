using System;
using System.Collections.Generic;

namespace Moritz.Spec
{
    ///<summary>
    /// IUniqueDef is implemented by all objects that can contain VoiceDefs.
    /// Currently (11.9.2014) these are:
    ///     Seq
    ///     Block
    ///</summary>
    public interface IVoiceDefContainer
    {
        int AbsMsPosition { get; set; }
        int MsDuration { get; set; }
        IReadOnlyList<Trk> Trks { get; }
    }
}
