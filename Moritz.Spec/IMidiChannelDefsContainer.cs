using Krystals5ObjectLibrary;

using System.Collections.Generic;

namespace Moritz.Spec
{
    ///<summary>
    /// IMidiChannelDefsContainer is implemented by Seq and Bar.
    /// Both Seq and Bar contain a list of ChannelDef (each ChannelDef contains a list of Trk).
    ///</summary>
    public interface IMidiChannelDefsContainer
    {
        int AbsMsPosition { get; set; }
        int MsDuration { get; set; }

        void TimeWarp(Envelope envelope, double distortion);
        void SetPitchWheelSliders(Envelope envelope);

        IReadOnlyList<ChannelDef> ChannelDefs { get; }
    }
}
