
using System.Diagnostics;

namespace Moritz.Score.Midi
{
    ///<summary>
    /// A LocalizedMidiChordDef is a LocalMidiChordDef with additional MsPositon and msDuration attributes.
    /// Related classes:
    /// 1. A LocalMidiChordDef is a MidiChordDef which is saved locally in an SVG file.
    /// 2. A PaletteMidiChordDef is a MidiChordDef which is saved in or retreived from a palette.
    /// PaletteMidiChordDefs can be 'used' in SVG files, but are usually converted to LocalMidiChordDefs.
    //</summary>
    public class LocalizedMidiDurationDef
    {
        public LocalizedMidiDurationDef(MidiDurationDef midiDurationDef)
        {
            MidiChordDef midiChordDef = midiDurationDef as MidiChordDef; // null if midiDurationDef is a midiRestDef or null
            if(midiChordDef == null)
            {
                LocalMidiChordDef = null;
            }
            else
            {
                LocalMidiChordDef = new LocalMidiChordDef(midiChordDef); // a deep clone with a special id string.
            }
            // MsPosition and MsDuration default to 0.
            if(midiDurationDef != null)
            {
                MsDuration = midiDurationDef.MsDuration;
            }
        }

        /// <summary>
        /// This constructor can be used to construct a rest.
        /// </summary>
        /// <param name="msDuration"></param>
        public LocalizedMidiDurationDef(int msDuration)
        {
            LocalMidiChordDef = null; // null if midiDurationDef is a midiRestDef
            MsPosition = 0; // can be reset later
            MsDuration = msDuration;
            Debug.Assert(MsDuration > 0);
        }

        /// <summary>
        /// Used to create clones and instances of FinalLMDDInVoice.
        /// </summary>
        public LocalizedMidiDurationDef(MidiDurationDef midiDurationDef, int msPosition, int msDuration)
            : this(midiDurationDef)
        {
            MsPosition = msPosition;
            MsDuration = msDuration;
        }

        /// <summary>
        /// This LocalMidiChordDef represents a rest if LocalMidiChordDef==null.
        /// </summary>
        public readonly LocalMidiChordDef LocalMidiChordDef = null;

        /// <summary>
        /// Transpose up by the number of semitones given in the argument.
        /// Negative interval values transpose down.
        /// If this is a MidiRestDef, nothing happens and the function returns silently.
        /// It this is a LocalMidiChordDef, is not an error if Midi pitch values would exceed the range 0..127.
        /// In this case, they are silently coerced to 0 or 127 respectively.
        /// </summary>
        /// <param name="interval"></param>
        public void Transpose(int interval)
        {
            if(LocalMidiChordDef != null)
            {
                // this is not a rest.
                LocalMidiChordDef.Transpose(interval);                
            }
        }

        /// <summary>
        /// This field is set if the chord crosses a barline. Rests never cross barlines, they are always split.
        /// </summary>
        public int? MsDurationToNextBarline = null;

        // can be changed
        public int MsDuration { get { return _msDuration; } set { _msDuration = value; } }
        private int _msDuration = 0;

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
}
