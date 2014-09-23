using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Xml;
using System.Text;
using System.ComponentModel;

using Moritz.Globals;
using Moritz.Score.Midi;

namespace Moritz.Score.Midi
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

            w.WriteAttributeString("noteOnKey", this.NoteOnKeyOption.ToString());
            w.WriteAttributeString("noteOnVel", this.NoteOnVelocityOption.ToString());
            w.WriteAttributeString("noteOff", this.NoteOffOption.ToString());
            if(this.NoteOffOption == Moritz.Score.Midi.NoteOffOption.limitedFade)
            {
                Debug.Assert(NumberOfObjectsInFade != null,
                            "\nInputControls Error:\n" +
                            "If the NoteOffOption is set to 'limitedFade',\n" +
                            "then the NumberOfObjectsInFade must also be set.");
                w.WriteAttributeString("limitedFade", NumberOfObjectsInFade.ToString());
            }

            bool isControllingVolume = false;
            w.WriteAttributeString("pressure", this.PressureOption.ToString());
            if(this.PressureOption == Moritz.Score.Midi.ControllerOption.volume)
            {
                WriteMaxMinVolume(w);
                isControllingVolume = true;
            }
            w.WriteAttributeString("pitchWheel", this.PitchWheelOption.ToString());
            if(this.PitchWheelOption == Moritz.Score.Midi.ControllerOption.volume)
            {
                Debug.Assert(isControllingVolume == false);
                WriteMaxMinVolume(w);
                isControllingVolume = true;
            }
            w.WriteAttributeString("modulation", this.ModWheelOption.ToString());
            if(this.ModWheelOption == Moritz.Score.Midi.ControllerOption.volume)
            {
                Debug.Assert(isControllingVolume == false);
                WriteMaxMinVolume(w);
            }
           
            w.WriteEndElement(); // score:inputControls
        }

        private void WriteMaxMinVolume(SvgWriter w)
        {
            Debug.Assert(MaximumVolume != null && MinimumVolume != null,
                "\nInputControls Error:\n" +
                "If any of the continuous controllers is set to control the *volume*,\n" +
                "then both MaximumVolume and MinimumVolume must also be set.");
            w.WriteAttributeString("maxVolume", MaximumVolume.ToString());
            w.WriteAttributeString("minVolume", MinimumVolume.ToString());
        }

        public NoteOnKeyOption NoteOnKeyOption = Moritz.Score.Midi.NoteOnKeyOption.ignore;
        public NoteOnVelocityOption NoteOnVelocityOption = Moritz.Score.Midi.NoteOnVelocityOption.ignore;
        public NoteOffOption NoteOffOption = Moritz.Score.Midi.NoteOffOption.ignore;

        public ControllerOption PressureOption = ControllerOption.ignore;
        public ControllerOption PitchWheelOption = ControllerOption.ignore;
        public ControllerOption ModWheelOption = ControllerOption.ignore; 

        public int? NumberOfObjectsInFade = null; // must be set if the NoteOffOption is "limitedFade".

        public byte? MaximumVolume = null; // must be set if the performer is controlling the volume (often set to the channel masterVolume)
        public byte? MinimumVolume = null; // must be set if the performer is controlling the volume (often set to about 50)
    }

    public enum NoteOnKeyOption
    {
        ignore, // any key will start the Seq.
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
        limitedFade
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
}
