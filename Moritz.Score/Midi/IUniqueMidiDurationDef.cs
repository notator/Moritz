using System.Collections.Generic;
using System.Diagnostics;

namespace Moritz.Score.Midi
{
    ///<summary>
    /// IUniqueMidiDurationDef defines an interface implemented by both UniqueMidiChordDef and UniqueMidiRestDef.
    ///</summary>
    public interface IUniqueMidiDurationDef
    {
        string ToString();
        //{
        //    return("MsPosition=" + MsPosition.ToString() + " MsDuration=" + MsDuration.ToString());
        //}

        /// <summary>
        /// Transpose up by the number of semitones given in the argument.
        /// Negative interval values transpose down.
        /// If this is a MidiRestDef, nothing happens and the function returns silently.
        /// It this is a UniqueMidiChordDef, is not an error if Midi pitch values would exceed the range 0..127.
        /// In this case, they are silently coerced to 0 or 127 respectively.
        /// </summary>
        /// <param name="interval"></param>
        void Transpose(int interval);
        //{
        //    UniqueMidiChordDef lmcd = UniqueMidiDurationDef as UniqueMidiChordDef;
        //    if(lmcd != null)
        //    {
        //        // this is not a rest.
        //        lmcd.Transpose(interval);                
        //    }
        //}

        /// <summary>
        /// Multiplies the MsDuration by the given factor.
        /// </summary>
        /// <param name="factor"></param>
        void AdjustDuration(double factor);
        //{
        //    _msDuration = (int)(_msDuration * factor);
        //    MsDuration = _msDuration;
        //}

        void AdjustVelocities(double factor);
        //{
        //    foreach(BasicMidiChordDef bmcd in BasicMidiChordDefs)
        //    {
        //        for(int i = 0; i < bmcd.Velocities.Count; ++i)
        //        {
        //            bmcd.Velocities[i] = MidiValue((int)(bmcd.Velocities[i] * factor));
        //        }
        //    }
        //    this._midiVelocity = BasicMidiChordDefs[0].Velocities[0];
        //}

        void AdjustExpression(double factor);
        //{
        //    List<byte> exprs = this.MidiChordSliderDefs.ExpressionMsbs;
        //    for(int i = 0; i < exprs.Count; ++i)
        //    {
        //        exprs[i] = MidiValue((int)(exprs[i] * factor));
        //    }
        //}

        void AdjustModulationWheel(double factor);
        //{
        //    List<byte> modWheels = this.MidiChordSliderDefs.ModulationWheelMsbs;
        //    for(int i = 0; i < modWheels.Count; ++i)
        //    {
        //        modWheels[i] = MidiValue((int)(modWheels[i] * factor));
        //    }
        //}

        void AdjustPitchWheel(double factor);
        //{
        //    List<byte> pitchWheels = this.MidiChordSliderDefs.PitchWheelMsbs;
        //    for(int i = 0; i < pitchWheels.Count; ++i)
        //    {
        //        pitchWheels[i] = MidiValue((int)(pitchWheels[i] * factor));
        //    }
        //}

        int PitchWheelDeviation { get; set; }
        //{
        //    get
        //    {
        //        byte? pwd = null;
        //        UniqueMidiChordDef umcd =  UniqueMidiDurationDef as UniqueMidiChordDef;
        //        if(umcd != null)
        //        {
        //            pwd = umcd.PitchWheelDeviation;
        //        }
        //        int rval = 2;
        //        if(pwd != null)
        //            rval = (int) pwd;
        //        return rval;
        //    }
        //    set
        //    {
        //        UniqueMidiChordDef umcd = UniqueMidiDurationDef as UniqueMidiChordDef;
        //        if(umcd != null)
        //        {
        //            byte pwd = (value > 127) ? (byte) 127 : (byte) value;
        //            pwd = (pwd < 0) ? (byte)0 : pwd; 
        //            umcd.PitchWheelDeviation = pwd;
        //        }
        //    }
        //}

        /// <summary>
        /// This field is set if the chord crosses a barline. Rests never cross barlines, they are always split.
        /// </summary>
        int? MsDurationToNextBarline{ get; set; }
        //public int? MsDurationToNextBarline = null;

        // the msDuration of an IUniqueMidiDurationDef can be changed
        int MsDuration { get; set; }

        int MsPosition { get; set; }

        //public int MsDuration { get { return _msDuration; } set { _msDuration = value; } }
        //private int _msDuration = 0;

        //public int MsPosition { get { return _msPosition; } set { _msPosition = value; } }
        //private int _msPosition = 0;

        List<byte> PanMsbs { get; set; }
    }

    ///// <summary>
    ///// Created when a note or rest straddles a barline.
    ///// This class is created while splitting systems.
    ///// It is used when notating them.
    ///// </summary>
    //public class LocalCautionaryChordDef : IUniqueMidiDurationDef
    //{
    //    public LocalCautionaryChordDef(MidiChordDef cautionaryMidiChordDef, int msPosition, int msDuration)
    //        : base(cautionaryMidiChordDef, msPosition, msDuration)
    //    {
    //    }
    //}
}
