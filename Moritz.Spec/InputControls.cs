using System;
using System.Diagnostics;

using Moritz.Xml;

namespace Moritz.Spec
{
    /// <summary>
    /// This object defines how Seqs react to incoming performed information.
    /// An InputControls object can be attached to a SeqRef in an InputChordDef.SeqRefs list.
    /// The values these options can take are defined in enums in this namespace.
    /// The default options for an Output Voice are:
    ///     onlySeq="0" (=false) -- the options become the default options for the Output Voice, when this seq ends.
    ///     noteOnKey="matchExactly" -- wrong midi pitches in the input are ignored
    ///     noteOnVel="ignore" -- input midi velocities are ignored (the score uses its own, default velocities) 
    ///     noteOff="ignore" -- input noteOffs are ignored.
    ///     pressure="ignore" -- input channelPressure/aftertouch information is ignored.
    ///     pitchWheel="ignore" -- input pitchWheel information is ignored.
    ///     modulation="ignore" -- input modulation wheel information is ignored.
    ///     speedOption="none" -- the default durations (set in the score) are used.
    /// These values are inidivdually overridden by InputControls objects during a performance if the new value is
    /// not "dontOverride". Whether the result of the overriding is temporary or permanent in the OutputVoice is set
    /// using the onlySeq option. 
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

            if(this.OnlySeq == true)
            {
                w.WriteAttributeString("onlySeq", "1");
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

        public bool OnlySeq = false;

        public NoteOnKeyOption NoteOnKeyOption = NoteOnKeyOption.dontOverride;
        public NoteOnVelocityOption NoteOnVelocityOption = NoteOnVelocityOption.dontOverride;
        public NoteOffOption NoteOffOption = NoteOffOption.dontOverride;

        public ControllerOption PressureOption = ControllerOption.dontOverride;
        public ControllerOption PitchWheelOption = ControllerOption.dontOverride;
        public ControllerOption ModWheelOption = ControllerOption.dontOverride;

        public SpeedOption SpeedOption = SpeedOption.dontOverride;

        public int? NumberOfObjectsInFade = null; // must be set if the NoteOffOption is "limitedFade".

        public byte? MaximumVolume = null; // must be set if the performer is controlling the volume (often set to the channel masterVolume)
        public byte? MinimumVolume = null; // must be set if the performer is controlling the volume (often set to about 50)

        public int? MaxSpeedPercent = null; // must be set to a value > 100 if the performer is controlling the speed (often set to about 400)
    }

    public enum NoteOnKeyOption
    {
        dontOverride, // Dont write the option to the score file. Use the current voice setting.
        ignore, // any key will start the Seq. It will play using the notated pitches.
        transpose,
        matchExactly // the Seq. will not start if the key does not match any of the notated pitchesd.
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
