using System;
using System.Collections.Generic;

namespace Moritz.Spec
{
	///<summary>
	/// ITrksContainer is implemented by Seq and Bar.
	/// A Seq only contains a list of Trk.
	/// A Bar contains a list of VoiceDef (i.e. both Trk and InputVoiceDef), and implements Trks by traversing that list.
	///</summary>
	public interface ITrksContainer
    {
        int AbsMsPosition { get; set; }
        int MsDuration { get; set; }

        void TimeWarp(Envelope envelope, double distortion);
        void SetPitchWheelSliders(Envelope envelope);

        IReadOnlyList<Trk> Trks { get; }
    }
}
