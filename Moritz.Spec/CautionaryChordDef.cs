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
		}

		public override string ToString()
		{
			return ($"CautionaryChordDef: MsDuration={MsDuration.ToString()}, MsPositionReFirstUD={MsPositionReFirstUD}");
		}

        public new int MsDuration { get; } // = msDurationAfterBarline
        public new int MsPositionReFirstUD { get; } = 0;
    }
}
