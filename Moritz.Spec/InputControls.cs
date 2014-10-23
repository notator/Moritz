using System;
using System.Diagnostics;

using Moritz.Xml;

namespace Moritz.Spec
{
    /// <summary>
    /// The following classes contain an InputControls field:
    ///     OutputVoice (If the score contains an InputVoice, OutputVoice.InputControls may not be null.)
    ///         Options that affect current OutputChords in this OutputVoice. 
    ///     MidiChordDef (MidiChordDef.InputControls may be null.)
    ///         if non-null, non-null options change the current values in
    ///         the containing OutputVoice.InputControls.  
    /// </summary>
    public class InputControls
    {
        /// <summary>
        /// All options are set to "ignore" by default
        /// </summary>
        public InputControls()
        {
        }

        public void WriteSvg(SvgWriter w)
        {
            w.WriteStartElement("score", "inputControls", null);

            if(this.OnlySeq == true)
            {
                w.WriteAttributeString("onlySeq", "1");
            }

            if(this.NoteOnKeyOption != Spec.NoteOnKeyOption.matchExactly)
            {
                w.WriteAttributeString("noteOnKey", this.NoteOnKeyOption.ToString());
            }
            if(this.NoteOnVelocityOption != Spec.NoteOnVelocityOption.ignore)
            {
                w.WriteAttributeString("noteOnVel", this.NoteOnVelocityOption.ToString());
            }
            if(this.NoteOffOption != Spec.NoteOffOption.stop)
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
            if(this.PressureOption != ControllerOption.ignore)
            {
                w.WriteAttributeString("pressure", this.PressureOption.ToString());
                if(this.PressureOption == ControllerOption.volume)
                {
                    WriteMaxMinVolume(w);
                    isControllingVolume = true;
                }
            }

            if(this.PitchWheelOption != ControllerOption.ignore)
            {
                w.WriteAttributeString("pitchWheel", this.PitchWheelOption.ToString());
                if(this.PitchWheelOption == ControllerOption.volume)
                {
                    Debug.Assert(isControllingVolume == false);
                    WriteMaxMinVolume(w);
                    isControllingVolume = true;
                }
            }

            if(this.ModWheelOption != ControllerOption.ignore)
            {
                w.WriteAttributeString("modulation", this.ModWheelOption.ToString());
                if(this.ModWheelOption == ControllerOption.volume)
                {
                    Debug.Assert(isControllingVolume == false);
                    WriteMaxMinVolume(w);
                    isControllingVolume = true;
                }
            }

            if(this.SpeedOption != Spec.SpeedOption.none)
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

        public NoteOnKeyOption NoteOnKeyOption = NoteOnKeyOption.ignore;
        public NoteOnVelocityOption NoteOnVelocityOption = NoteOnVelocityOption.ignore;
        public NoteOffOption NoteOffOption = NoteOffOption.ignore;

        public ControllerOption PressureOption = ControllerOption.ignore;
        public ControllerOption PitchWheelOption = ControllerOption.ignore;
        public ControllerOption ModWheelOption = ControllerOption.ignore;

        public SpeedOption SpeedOption = SpeedOption.none;

        public int? NumberOfObjectsInFade = null; // must be set if the NoteOffOption is "limitedFade".

        public byte? MaximumVolume = null; // must be set if the performer is controlling the volume (often set to the channel masterVolume)
        public byte? MinimumVolume = null; // must be set if the performer is controlling the volume (often set to about 50)

        public int? MaxSpeedPercent = null; // must be set to a value > 100 if the performer is controlling the speed (often set to about 400)
    }

    public enum NoteOnKeyOption
    {
        ignore, // any key will start the Seq. It will play using the notated pitches.
        transpose,
        matchExactly // the Seq. will not start if the key does not match any of the notated pitchesd.
    };

    public enum NoteOnVelocityOption
    {
        ignore,
        scale
    };

    public enum NoteOffOption
    {
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
        none,
        noteOnKey,
        noteOnVel,
        pressure,
        pitchWheel,
        modulation
    };
}
