using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Xml;
using System.Text;
using System.ComponentModel;

using Moritz.Globals;
using Moritz.Score.Midi;

namespace Moritz.Score.Notation
{
    /// <summary>
    /// The following classes contain a PerformanceControlDef field.
    ///     OutputVoice (OutputVoice.PerformanceControlDef may not be null.)
    ///         Options that affect current OutputChords in this OutputVoice. 
    ///     OutputChord (OutputChord.PerformanceControlDef may be null.)
    ///         if non-null, non-null options in this PerformanceControlDef change the
    ///         current values in the containing OutputVoice.PerformanceControlDef.  
    /// </summary>
    public class PerformanceControlDef
    {
        public PerformanceControlDef(bool nullDefaults)
        {
            if(!nullDefaults)
            {
                this.NoteOnKeyOption = Moritz.Score.Notation.NoteOnKeyOption.ignore;
                this.NoteOnVelocityOption = Moritz.Score.Notation.NoteOnVelocityOption.ignore;
                this.NoteOffOption = Moritz.Score.Notation.NoteOffOption.ignore;
                this.PressureOption = ControllerOption.ignore;
                this.PitchWheelOption = ControllerOption.ignore;
                this.ModWheelOption = ControllerOption.ignore;
            }
        }

        public void WriteSvg(SvgWriter w)
        {
            w.WriteStartElement("score", "performanceControl", null);

            if(NoteOnKeyOption != null)
                w.WriteAttributeString("noteOnKey", this.NoteOnKeyOption.ToString());
            if(NoteOnVelocityOption != null)
                w.WriteAttributeString("noteOnVel", this.NoteOnVelocityOption.ToString());
            if(NoteOffOption != null)
            {
                w.WriteAttributeString("noteOff", this.NoteOffOption.ToString());
                if(this.NoteOffOption == Moritz.Score.Notation.NoteOffOption.limitedFade)
                {
                    Debug.Assert(NumberOfObjectsInFade != null);
                    w.WriteAttributeString("fixedFade", NumberOfObjectsInFade.ToString());
                }
            }

            bool isControllingVolume = false;
            if(PressureOption != null)
            {
                w.WriteAttributeString("pressure", this.PressureOption.ToString());
                if(this.PressureOption == Moritz.Score.Notation.ControllerOption.volume)
                {
                    Debug.Assert(MinimumVolume != null);
                    w.WriteAttributeString("minVolume", MinimumVolume.ToString());
                    isControllingVolume = true;
                }
            }
            if(PitchWheelOption != null)
            {
                w.WriteAttributeString("pitchWheel", this.PitchWheelOption.ToString());
                if(this.PitchWheelOption == Moritz.Score.Notation.ControllerOption.volume)
                {
                    Debug.Assert(isControllingVolume == false);
                    Debug.Assert(MinimumVolume != null);
                    w.WriteAttributeString("minVolume", MinimumVolume.ToString());
                    isControllingVolume = true;
                }
            }
            if(ModWheelOption != null)
            {
                w.WriteAttributeString("modulation", this.ModWheelOption.ToString());
                if(this.ModWheelOption == Moritz.Score.Notation.ControllerOption.volume)
                {
                    Debug.Assert(isControllingVolume == false);
                    Debug.Assert(MinimumVolume != null);
                    w.WriteAttributeString("minVolume", MinimumVolume.ToString());
                }
            }

            if(MasterVolume != null)
            {
                w.WriteAttributeString("masterVolume", MasterVolume.ToString());
            }
           
            w.WriteEndElement(); // score:perfCtl
        }

        public NoteOnKeyOption? NoteOnKeyOption = null; // default will be NoteOnKeyOption.matchExactly;
        public NoteOnVelocityOption? NoteOnVelocityOption = null; // default will be NoteOnVelocityOption.ignore;
        public NoteOffOption? NoteOffOption = null; // default will be NoteOffOption.ignore;

        public ControllerOption? PressureOption = null; // default will be ControllerOption.ignore;
        public ControllerOption? PitchWheelOption = null; // default will be ControllerOption.ignore;
        public ControllerOption? ModWheelOption = null; // default will be ControllerOption.ignore; 

        public int? NumberOfObjectsInFade = null; // default will be 0; // only used in conjunction with NoteOffOption.limitedFade

        public byte? MasterVolume = null; // default is the channel masterVolume; // only used if the performer is controlling the volume
        public byte? MinimumVolume = null; // only used if the performer is controlling the volume
    }

    public enum NoteOnKeyOption
    {
        [Description("ignore")]
        ignore = 0,
        [Description("transpose")]
        transpose = 1,
        [Description("matchExactly")]
        matchExactly = 2
    };

    public enum NoteOnVelocityOption
    {
        [Description("ignore")]
        ignore = 0,
        [Description("scale")]
        scale = 1
    };

    public enum NoteOffOption
    {
        [Description("ignore")]
        ignore = 0,
        [Description("stop")]
        stop = 1,
        [Description("stopNow")]
        stopNow = 2,
        [Description("fade")]
        fade = 3,
        [Description("limitedFade")]
        limitedFade = 4
    };

    /// <summary>
    /// These values are in the order of the controlOptions list defined in
    /// Assistant Performer:PerformersOptionsDialog.js
    /// </summary>
    public enum ControllerOption
    {
        [Description("ignore")]
        ignore = 0,
        [Description("aftertouch")]
        aftertouch = 1,
        [Description("channelPressure")]
        channelPressure = 2,
        [Description("pitchWheel")]
        pitchWheel = 3,
        [Description("modulation")]
        modulation = 4,
        [Description("volume")]
        volume = 5,
        [Description("pan")]
        pan = 6,
        [Description("expression")]
        expression = 7,
        [Description("timbre")]
        timbre = 8,
        [Description("brightness")]
        brightness = 9,
        [Description("effects")]
        effects = 10,
        [Description("tremolo")]
        tremolo = 11,
        [Description("chorus")]
        chorus = 12,
        [Description("celeste")]
        celeste = 13,
        [Description("phaser")]
        phaser = 14
    };
}
