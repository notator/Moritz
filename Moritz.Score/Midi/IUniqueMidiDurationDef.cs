using System.Collections.Generic;
using System.Diagnostics;

namespace Moritz.Score.Midi
{
    ///<summary>
    /// IUniqueMidiDurationDef defines an interface implemented by both MidiChordDef and UniqueMidiRestDef.
    ///</summary>
    public interface IUniqueMidiDurationDef
    {
        string ToString();

        /// <summary>
        /// Transpose up by the number of semitones given in the argument.
        /// Negative interval values transpose down.
        /// If this is a MidiRestDef, nothing happens and the function returns silently.
        /// It this is a MidiChordDef, is not an error if Midi pitch values would exceed the range 0..127.
        /// In this case, they are silently coerced to 0 or 127 respectively.
        /// </summary>
        /// <param name="interval"></param>
        void Transpose(int interval);

        /// <summary>
        /// Multiplies the MsDuration by the given factor.
        /// </summary>
        /// <param name="factor"></param>
        void AdjustDuration(double factor);
        void AdjustVelocities(double factor);
        void AdjustExpression(double factor);
        void AdjustModulationWheel(double factor);
        void AdjustPitchWheel(double factor);

        int? PitchWheelDeviation { get; set; }
        /// <summary>
        /// This field is set if the chord crosses a barline. Rests never cross barlines, they are always split.
        /// </summary>
        int? MsDurationToNextBarline{ get; set; }

        // the msDuration of an IUniqueMidiDurationDef can be changed
        int MsDuration { get; set; }
        int MsPosition { get; set; }

        List<byte> PanMsbs { get; set; }
    }
}
