using System;
using System.Diagnostics;

using Moritz.Xml;

namespace Moritz.Spec
{
    /// <summary>
    /// This object defines how Trks react to incoming performed information.
    /// An InputControls object can be attached to a TrkRef in an InputChordDef.TrkRefs list.
    /// The values these options can take in the InputControls are defined in enums in this namespace.
    /// (See below, and in the svgScoreExtensions documentation for details.)
    /// Each of these enums has a "dontOverride" member.
    /// The option value applicable to a Trk is the current value of that option stored in the OutputVoice, unless
    /// the TrkRef has an InputControls member, and the InputControls value of that option is something other than
    /// "dontOverride". In this case, the InputControls option value overrides the OutputVoice's option value.
    /// Whether the result of the overriding is temporary or is transferred to the OutputVoice is set using the
    /// InputControls.onlyTrk option. The onlyTrk option itself is not maintained in the OutputVoice.
    /// The default options for an Output Voice are:
    ///     noteOnKey="ignore" -- input midi pitches are ignored (the score uses its own, default pitches)
    ///     noteOnVel="ignore" -- input midi velocities are ignored (the score uses its own, default velocities) 
    ///     noteOff="ignore" -- input noteOffs are ignored.
    ///     pressure="ignore" -- input channelPressure/aftertouch information is ignored.
    ///     pitchWheel="ignore" -- input pitchWheel information is ignored.
    ///     modulation="ignore" -- input modulation wheel information is ignored.
    ///     speedOption="none" -- the default durations (set in the score) are used. 
    /// </summary>
    public class InputControls
    {
        /// <summary>
        /// All options are set to null by default
        /// </summary>
        public InputControls()
        {
        }

        public void WriteSvg(SvgWriter w)
        {
            w.WriteStartElement("inputControls");

            if(this.OnlyTrk == true)
            {
                w.WriteAttributeString("onlyTrk", "1");
            }

            if(this.NoteOnKeyOption != Spec.NoteOnKeyOption.dontOverride)
            {
                w.WriteAttributeString("noteOnKey", this.NoteOnKeyOption.ToString());
            }
            if(this.NoteOnVelocityOption != Spec.NoteOnVelocityOption.dontOverride)
            {
                w.WriteAttributeString("noteOnVel", this.NoteOnVelocityOption.ToString());
            }
            if(this.NoteOffOption != Spec.NoteOffOption.dontOverride)
            {
                w.WriteAttributeString("noteOff", this.NoteOffOption.ToString());
            }
            if(this.NoteOffOption == NoteOffOption.shortFade)
            {
                if(NumberOfObjectsInFade == null)
                    throw new ApplicationException(
                            "\nInputControls Error:\n" +
                            "If the NoteOffOption is set to 'shortFade',\n" +
                            "then the NumberOfObjectsInFade must also be set.");
                w.WriteAttributeString("shortFade", NumberOfObjectsInFade.ToString());
            }
            bool isControllingVolume = false;
            if(this.PressureOption != ControllerOption.dontOverride)
            {
                w.WriteAttributeString("pressure", this.PressureOption.ToString());
                if(this.PressureOption == ControllerOption.volume)
                {
                    WriteMaxMinVolume(w);
                    isControllingVolume = true;
                }
            }

            if(this.PitchWheelOption != ControllerOption.dontOverride)
            {
                w.WriteAttributeString("pitchWheel", this.PitchWheelOption.ToString());
                if(this.PitchWheelOption == ControllerOption.volume)
                {
                    Debug.Assert(isControllingVolume == false);
                    WriteMaxMinVolume(w);
                    isControllingVolume = true;
                }
            }

            if(this.ModWheelOption != ControllerOption.dontOverride)
            {
                w.WriteAttributeString("modulation", this.ModWheelOption.ToString());
                if(this.ModWheelOption == ControllerOption.volume)
                {
                    Debug.Assert(isControllingVolume == false);
                    WriteMaxMinVolume(w);
                    isControllingVolume = true;
                }
            }

            if(this.SpeedOption != Spec.SpeedOption.dontOverride)
            {
                w.WriteAttributeString("speedOption", this.SpeedOption.ToString());
                if(MaxSpeedPercent == null || MaxSpeedPercent < 100)
                {
                    throw new ApplicationException(
                        "\nInputControls Error:\n" +
                        "If the SpeedOption is set, then MaxSpeedPercent must be set\n" +
                        "to a value >= 100.");
                }
                w.WriteAttributeString("maxSpeedPercent", this.MaxSpeedPercent.ToString());
            }
            w.WriteEndElement(); // score:inputControls
        }
        private void WriteMaxMinVolume(SvgWriter w)
        {
            if(MaximumVolume == null || MinimumVolume == null)
            {
                throw new ApplicationException(
                    "\nInputControls Error:\n" +
                    "If any of the continuous controllers is set to control the *volume*,\n" +
                    "then both MaximumVolume and MinimumVolume must also be set.");
            }
            if(MaximumVolume <= MinimumVolume)
            {
                throw new ApplicationException(
                    "\nInputControls Error:\n" +
                    "MaximumVolume must be greater than MinimumVolume.");
            }
            w.WriteAttributeString("maxVolume", MaximumVolume.ToString());
            w.WriteAttributeString("minVolume", MinimumVolume.ToString());
        }

        public bool OnlyTrk = false;
        public NoteOnKeyOption NoteOnKeyOption = NoteOnKeyOption.dontOverride;
        public NoteOnVelocityOption NoteOnVelocityOption = NoteOnVelocityOption.dontOverride;
        public NoteOffOption NoteOffOption = NoteOffOption.dontOverride;
        public ControllerOption PressureOption = ControllerOption.dontOverride;
        public ControllerOption PitchWheelOption = ControllerOption.dontOverride;
        public ControllerOption ModWheelOption = ControllerOption.dontOverride;
        public SpeedOption SpeedOption = SpeedOption.dontOverride;
        public int? NumberOfObjectsInFade = null; // must be set if the NoteOffOption is "limitedFade".
        public byte? MaximumVolume = null; // must be set if the performer is controlling the volume
        public byte? MinimumVolume = null; // must be set if the performer is controlling the volume
        public int? MaxSpeedPercent = null; // must be set to a value > 100 if the performer is controlling the speed (often set to about 400)
    }

    public enum NoteOnKeyOption
    {
        dontOverride, // Dont write the option to the score file. Use the current voice setting.
        ignore, // any key will start the Trk. It will play using the notated pitches.
        transpose,
        matchExactly // the Trk will not start if the key does not match any of the notated pitchesd.
    };

    public enum NoteOnVelocityOption
    {
        dontOverride, // Dont write the option to the score file. Use the current voice setting.
        ignore,
        scale
    };

    public enum NoteOffOption
    {
        dontOverride, // Dont write the option to the score file. Use the current voice setting.
        ignore,
        stop,
        stopNow,
        fade,
        shortFade
    };

    /// <summary>
    /// These values are in the order of the controlOptions list defined in
    /// Assistant Performer:PerformersOptionsDialog.js.
    /// The int values may be used to index in a menu list. 
    /// </summary>
    public enum ControllerOption
    {
        dontOverride, // Dont write the option to the score file. Use the current voice setting.
        ignore = 0,
        aftertouch = 1,
        channelPressure = 2,
        pitchWheel = 3,
        modulation = 4,
        volume = 5,
        pan = 6,
        expression = 7,
        timbre = 8,
        brightness = 9,
        effects = 10,
        tremolo = 11,
        chorus = 12,
        celeste = 13,
        phaser = 14
    };

    public enum SpeedOption
    {
        dontOverride, // Dont write the option to the score file. Use the current voice setting.
        none, // use the default durations in the score
        noteOnKey,
        noteOnVel,
        pressure,
        pitchWheel,
        modulation
    };
}
