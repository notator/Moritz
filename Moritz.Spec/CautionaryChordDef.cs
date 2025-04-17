using Moritz.Globals;

using System.Collections.Generic;
using System.Diagnostics;

namespace Moritz.Spec
{
	/// <summary>
	/// Created when a note or rest straddles a barline.
	/// This class is created while splitting systems.
	/// It is used when notating them. this class has no content!
	/// </summary>
	public class CautionaryChordDef : MidiChordDef
	{
		public CautionaryChordDef(MidiChordDef midiChordDef, int msDurationAfterBarline)
        :base(midiChordDef.Pitches, midiChordDef.Velocities, msDurationAfterBarline, midiChordDef.HasChordOff)
		{
			MsPositionReFirstUD = 0;  // always immediately follows the barline
		}

		public override string ToString()
		{
			return ("CautionaryChordDef: MsDuration=" + MsDuration.ToString());
		}
	}
}
