using System.Collections.Generic;
using System.Diagnostics;

using Multimedia.Midi;

using Moritz.Globals;

namespace Moritz.Score.Midi
{
    ///<summary>
    /// A UniqueMidiRestDef is a unique MidiRestDef which is saved locally in an SVG file.
    /// (This class is necessary, because palettes can also contain rests - which should never be changed.)
    /// Related classes:
    /// 1. A MidiChordDef is saved in or retreived from a palette.
    /// MidiChordDefs can be 'used' in SVG files, but are usually converted to UniqueMidiChordDef.
    /// 2. A UniqueMidiChordDef is a MidiChordDef with an additional MsPositon attribute.
    ///<summary>
    public class UniqueMidiRestDef : MidiRestDef, IUniqueMidiDurationDef
    {
        /// <summary>
        /// Note that rest IDs in SVG files are of the form "rest"+uniqueNumber, and
        /// that the uniqueNumber is allocated at the time when the SVG file is written.
        /// The null id passed to the base class here should always be ignored.
        /// </summary>
        /// <param name="midiRestDef"></param>
        public UniqueMidiRestDef(MidiRestDef midiRestDef)
            :base(null, midiRestDef.MsDuration)
        {
            MsPosition = 0;
        }

        public UniqueMidiRestDef(int msPosition, int msDuration)
            :base(null, msDuration)
        {
            MsPosition = msPosition;
        }

        #region IUniqueMidiDurationDef
        public override string ToString()
        {
            return ("MsPosition=" + MsPosition.ToString() + " MsDuration=" + MsDuration.ToString());
        }
        /// <summary>
        /// Transpose (both notation and sound) by the number of semitones given in the argument.
        /// Negative interval values transpose down.
        /// It is not an error if Midi values would exceed the range 0..127.
        /// In this case, they are silently coerced to 0 or 127 respectively.
        /// </summary>
        public void Transpose(int interval){ }

        /// <summary>
        /// Multiplies the MsDuration by the given factor.
        /// </summary>
        /// <param name="factor"></param>
        public void AdjustDuration(double factor)
        {
            _msDuration = (int)(_msDuration * factor);
        }

        public void AdjustVelocities(double factor) {}
        public void AdjustExpression(double factor){}
        public void AdjustModulationWheel(double factor){}
        public void AdjustPitchWheel(double factor){}
        public byte MidiVelocity { get { return 0; } set {}}
        public int? PitchWheelDeviation { get { return null;} set {}}

        public List<byte> PanMsbs { get { return new List<byte>(); } set { } }

        public int? MsDurationToNextBarline { get { return _msDurationToNextBarline; } set { _msDurationToNextBarline = value; } }
        private int? _msDurationToNextBarline = null;

        public new int MsDuration { get { return _msDuration; } set { _msDuration = value; } }

        public int MsPosition { get { return _msPosition; } set { _msPosition = value; } }
        private int _msPosition = 0;
        #endregion
    }
}
