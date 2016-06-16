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

        void TimeWarp(Envelope envelope, double distortion);
        void SetPitchWheelSliderEnvelope(Envelope envelope);

        IReadOnlyList<Trk> Trks { get; }
    }
}
