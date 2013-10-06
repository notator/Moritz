
using System.Diagnostics;

namespace Moritz.Score.Midi
{
    ///<summary>
    /// A LocalMidiDurationDef contains a UniqueMidiChordDef or UniqueMidiRestDef with additional MsPositon and MsDuration attributes.
    /// <para>Related classes:</para>
    /// <para>1. A UniqueMidiChordDef is a unique MidiChordDef which is saved locally in an SVG file.</para>
    /// <para>2. A PaletteMidiChordDef is a MidiChordDef which is saved in or retrieved from a palette.</para>
    /// <para>PaletteMidiChordDefs can be 'used' in SVG files, but are usually converted to UniqueMidiChordDefs.</para>
    ///</summary>
    public class LocalMidiDurationDef
    {
        /// <summary>
        /// this.UniqueMidiDurationDef is set to either a UniqueMidiChordDef or a UniqueMidiRestDef
        /// <para>depending on the class of the argument (MidiChordDef or MidiRestDef).</para>
        /// <para>this.MsPosition is set to 0;</para>
        /// <para>this.MsDuration is set to the argument's MsDuration attribute.</para> 
        /// </summary>
        /// <param name="midiDurationDef">Either a MidiChordDef or a MidiRestDef</param>
        public LocalMidiDurationDef(MidiDurationDef midiDurationDef)
        {
            Debug.Assert(midiDurationDef != null);

            MidiChordDef midiChordDef = midiDurationDef as MidiChordDef; // null if midiDurationDef is a midiRestDef or null
            if(midiChordDef != null)
            {
                UniqueMidiDurationDef = new UniqueMidiChordDef(midiChordDef); // a deep clone with a special id string.
            }
            else
            {
                MidiRestDef midiRestDef = midiDurationDef as MidiRestDef;
                Debug.Assert(midiRestDef != null);
                UniqueMidiDurationDef = new UniqueMidiRestDef(midiRestDef);
            }
            
            MsPosition = 0;
            MsDuration = midiDurationDef.MsDuration;
        }

        /// <summary>
        /// This constructor can be used to construct a UniqueMidiRestDef at MsPosition=0.
        /// The MsPosition and/or MsDuration can be changed later.
        /// </summary>
        /// <param name="msDuration"></param>
        public LocalMidiDurationDef(int msDuration)
        {
            Debug.Assert(msDuration > 0);
            UniqueMidiDurationDef = new UniqueMidiRestDef(new MidiRestDef("", msDuration));
            MsPosition = 0; // can be reset later
            MsDuration = msDuration;
        }

        /// <summary>
        /// Used to create clones and instances of FinalLMDDInVoice.
        /// </summary>
        public LocalMidiDurationDef(MidiDurationDef midiDurationDef, int msPosition, int msDuration)
            : this(midiDurationDef)
        {
            MsPosition = msPosition;
            MsDuration = msDuration;
        }

        /// <summary>
        /// Transpose up by the number of semitones given in the argument.
        /// Negative interval values transpose down.
        /// If this is a MidiRestDef, nothing happens and the function returns silently.
        /// It this is a UniqueMidiChordDef, is not an error if Midi pitch values would exceed the range 0..127.
        /// In this case, they are silently coerced to 0 or 127 respectively.
        /// </summary>
        /// <param name="interval"></param>
        public void Transpose(int interval)
        {
            UniqueMidiChordDef lmcd = UniqueMidiDurationDef as UniqueMidiChordDef;
            if(lmcd != null)
            {
                // this is not a rest.
                lmcd.Transpose(interval);                
            }
        }

        /// <summary>
        /// A UniqueMidiRestDef or a UniqueMidiChordDef.
        /// </summary>
        public readonly MidiDurationDef UniqueMidiDurationDef = null;

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
    public class LocalizedCautionaryChordDef : LocalMidiDurationDef
    {
        public LocalizedCautionaryChordDef(MidiChordDef cautionaryMidiChordDef, int msPosition, int msDuration)
            : base(cautionaryMidiChordDef, msPosition, msDuration)
        {
        }
    }
}
