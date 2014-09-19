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
    /// Cascading performance options.
    /// Values in this class that are not null, override the current setting.
    /// The following classes contain a PerformanceControlDef field.
    ///     Score (options that affect all InputVoices and their InputChords). This PerformanceControlDef defines the defaults for the score, and may not be null.
    ///     InputVoice (options that affect all the InputChords in the InputVoice). This PerformanceControlDef may be null. 
    ///     InputChord (options that affect just this InputChord). This PerformanceControlDef may be null. 
    /// </summary>
    public class PerformanceControlDef
    {
        public PerformanceControlDef(bool nullDefaults, int nOutputChannels)
        {
            if(!nullDefaults)
            {
                this.NoteOnKeyOption = Moritz.Score.Notation.NoteOnKeyOption.ignore;
                this.NoteOnVelocityOption = Moritz.Score.Notation.NoteOnVelocityOption.ignore;
                this.NoteOffOption = Moritz.Score.Notation.NoteOffOption.ignore;
                this.PressureOption = ControllerOption.ignore;
                this.PitchWheelOption = ControllerOption.ignore;
                this.ModWheelOption = ControllerOption.ignore;
                MasterVolumes = new List<byte>();
                for(int i = 0; i < nOutputChannels; ++i)
                {
                    MasterVolumes.Add(100);
                }
            }
        }

        public void WriteSvg(SvgWriter w)
        {
            w.WriteStartElement("score", "perfCtl", null);

            if(NoteOnKeyOption != null)
                w.WriteAttributeString("noteOnKey", ((int)this.NoteOnKeyOption).ToString());
            if(NoteOnVelocityOption != null)
                w.WriteAttributeString("noteOnVel", ((int)this.NoteOnVelocityOption).ToString());
            if(NoteOffOption != null)
            {
                w.WriteAttributeString("noteOff", ((int)this.NoteOffOption).ToString());
                if(this.NoteOffOption == Moritz.Score.Notation.NoteOffOption.limitedFade)
                {
                    Debug.Assert(NumberOfObjectsInFade != null);
                    w.WriteAttributeString("fixedFade", NumberOfObjectsInFade.ToString());
                }
            }

            bool isControllingVolume = false;
            if(PressureOption != null)
            {
                w.WriteAttributeString("pressure", ((int)this.PressureOption).ToString());
                if(this.PressureOption == Moritz.Score.Notation.ControllerOption.volume)
                {
                    Debug.Assert(MinimumVolume != null);
                    w.WriteAttributeString("minVolume", MinimumVolume.ToString());
                    isControllingVolume = true;
                }
            }
            if(PitchWheelOption != null)
            {
                w.WriteAttributeString("pitchWheel", ((int)this.PitchWheelOption).ToString());
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
                w.WriteAttributeString("modulation", ((int)this.ModWheelOption).ToString());
                if(this.ModWheelOption == Moritz.Score.Notation.ControllerOption.volume)
                {
                    Debug.Assert(isControllingVolume == false);
                    Debug.Assert(MinimumVolume != null);
                    w.WriteAttributeString("minVolume", MinimumVolume.ToString());
                }
            }

            if(MasterVolumes != null)
            {
                w.WriteAttributeString("masterVolumes", M.ByteListToString(MasterVolumes));
            }
           
            w.WriteEndElement(); // score:perfCtl
        }

        public NoteOnKeyOption? NoteOnKeyOption = null; // default will be NoteOnKeyOption.matchExactly;
        public NoteOnVelocityOption? NoteOnVelocityOption = null; // default will be NoteOnVelocityOption.ignore;
        public NoteOffOption? NoteOffOption = null; // default will be NoteOffOption.ignore;

        public ControllerOption? PressureOption = null; // default will be ControllerOption.ignore;
        public ControllerOption? PitchWheelOption = null; // default will be ControllerOption.ignore;
        public ControllerOption? ModWheelOption = null; // default will be ControllerOption.ignore; 

        public List<byte> MasterVolumes = null; // one value per channel

        public int? NumberOfObjectsInFade = null; // default will be 0; // only used in conjunction with NoteOffOption.limitedFade
        public byte? MinimumVolume = null; // default will be 50; // only used if the performer is controlling the volume
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
