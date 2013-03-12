
using System.Diagnostics;

namespace Moritz.Score.Midi
{
    public class LocalizedMidiDurationDef
    {
        /// <summary>
        /// Use the other constructor to create a rest.
        /// </summary>
        /// <param name="midiDurationDef"></param>
        /// <param name="msDuration"></param>
        public LocalizedMidiDurationDef(MidiDurationDef midiDurationDef)
        {
            MidiChordDef = midiDurationDef as MidiChordDef; // null if midiDurationDef is a midiRestDef
            MsPosition = 0;
            MsDuration = midiDurationDef.MsDuration;
        }

        public LocalizedMidiDurationDef(int msDuration)
        {
            MidiChordDef = null; // null if midiDurationDef is a midiRestDef
            MsPosition = 0; // can be reset later
            MsDuration = msDuration;
            Debug.Assert(MsDuration > 0);
        }

        /// <summary>
        /// Used to create instances of FinalLMDDInVoice.
        /// </summary>
        public LocalizedMidiDurationDef(MidiDurationDef midiDurationDef, int msPosition, int msDuration)
        {
            MidiChordDef = midiDurationDef as MidiChordDef; // null if midiDurationDef is a midiRestDef
            MsPosition = msPosition;
            MsDuration = msDuration;
        }

        /// <summary>
        /// This LocalizedMidiDurationDef represents a rest if MidiChordDef==null.
        /// </summary>
        public readonly MidiChordDef MidiChordDef = null;

        /// <summary>
        /// This field is set if the chord crosses a barline. Rests never cross barlines, they are always split.
        /// </summary>
        public int? MsDurationToNextBarline = null;
        public readonly int MsDuration = 0;
        public int MsPosition { get { return _msPosition; } set { _msPosition = value; } }
        private int _msPosition = 0;
    }

    /// <summary>
    /// Created when a note or rest straddles a barline.
    /// This class is created while splitting systems.
    /// It is used when notating them.
    /// </summary>
    public class OverlapLmddAtStartOfBar : LocalizedMidiDurationDef
    {
        public OverlapLmddAtStartOfBar(int msPosition, int msDuration, MidiChordDef cautionaryMidiChordDef)
            : base(null, msPosition, msDuration)
        {
            CautionaryMidiChordDef = cautionaryMidiChordDef;
        }
        /// <summary>
        /// Used to create cautionary chord at the beginning of a staff,
        /// and to find the heights of extension lines for such a chord.
        /// </summary>
        public readonly MidiChordDef CautionaryMidiChordDef;
    }

    public abstract class MidiDurationDef
    {
        protected MidiDurationDef() { }
        protected MidiDurationDef(int msDuration)
        {
            _msDuration = msDuration;
        }
        protected int _msDuration = 0;
        public int MsDuration { get { return _msDuration; } set { _msDuration = value; } }
    }
}
