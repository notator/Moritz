
using System.Diagnostics;

namespace Moritz.Score.Midi
{
    ///<summary>
    /// A LocalizedMidiDurationDef is a LocalMidiChordDef or LocalMidirestDef with additional MsPositon and msDuration attributes.
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
            if(midiChordDef != null)
            {
                LocalMidiDurationDef = new LocalMidiChordDef(midiChordDef); // a deep clone with a special id string.
            }
            else
            {
                MidiRestDef midiRestDef = midiDurationDef as MidiRestDef;
                Debug.Assert(midiRestDef != null);
                LocalMidiDurationDef = new LocalMidiRestDef(midiRestDef);
            }
            // MsPosition and MsDuration default to 0.
            if(midiDurationDef != null)
            {
                MsDuration = midiDurationDef.MsDuration;
            }
        }

        /// <summary>
        /// This constructor can be used to construct a restDef at MsPosition=0.
        /// The MsPosition and/or MsDuration can be changed later.
        /// </summary>
        /// <param name="msDuration"></param>
        public LocalizedMidiDurationDef(int msDuration)
        {
            Debug.Assert(msDuration > 0);
            LocalMidiDurationDef = new LocalMidiRestDef(new MidiRestDef("", msDuration));
            MsPosition = 0; // can be reset later
            MsDuration = msDuration;
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
        /// Transpose up by the number of semitones given in the argument.
        /// Negative interval values transpose down.
        /// If this is a MidiRestDef, nothing happens and the function returns silently.
        /// It this is a LocalMidiChordDef, is not an error if Midi pitch values would exceed the range 0..127.
        /// In this case, they are silently coerced to 0 or 127 respectively.
        /// </summary>
        /// <param name="interval"></param>
        public void Transpose(int interval)
        {
            LocalMidiChordDef lmcd = LocalMidiDurationDef as LocalMidiChordDef;
            if(lmcd != null)
            {
                // this is not a rest.
                lmcd.Transpose(interval);                
            }
        }

        /// <summary>
        /// A LocalMidiRestDef or a LocalMidiChordDef.
        /// </summary>
        public readonly MidiDurationDef LocalMidiDurationDef = null;

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
    public class LocalizedCautionaryChordDef : LocalizedMidiDurationDef
    {
        public LocalizedCautionaryChordDef(MidiChordDef cautionaryMidiChordDef, int msPosition, int msDuration)
            : base(cautionaryMidiChordDef, msPosition, msDuration)
        {
        }
    }
}
