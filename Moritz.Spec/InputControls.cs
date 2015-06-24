using System;
using System.Diagnostics;

using Moritz.Xml;

namespace Moritz.Spec
{
    /// <summary>
    /// This object defines how Trks react to incoming performed information.
	/// 
	/// An inputControls element can be contained in the following elements in score files:
	///		inputChord (which contains a list of inputNotes)
	///		inputNote (which contains a list of trkRefs)
	///		trkRef
	///		
	/// See http://james-ingram-act-two.de/open-source/svgScoreExtensions.html for details as to how these inputControls are used.
	/// 	
    /// The values these options can take in the InputControls are defined in enums in this namespace.
    /// In addition to the values stored in scores, each of these enums has a "undefined" member here in Moritz.
    /// The default options for an Input Voice are:
    ///     noteOnVel="undefined" -- input midi velocities are ignored (the score uses its own, default velocities) 
    ///     noteOff="undefined" -- input noteOffs are ignored.
    ///     pressure="undefined" -- input channelPressure/aftertouch information is ignored.
    ///     pitchWheel="undefined" -- input pitchWheel information is ignored.
    ///     modulation="undefined" -- input modulation wheel information is ignored.
    ///     speedOption="undefined" -- the default durations (set in the score) are used. 
    /// </summary>
    public class InputControls
    {
        /// <summary>
        /// All options are set to .undefined by default
        /// </summary>
        public InputControls()
        {
        }

        public void WriteSvg(SvgWriter w)
        {
            w.WriteStartElement("inputControls");

            if(this.NoteOnVelocityOption != NoteOnVelocityOption.undefined)
            {
                w.WriteAttributeString("noteOnVel", this.NoteOnVelocityOption.ToString());
				if(NoteOnVelocityOption != NoteOnVelocityOption.off)
				{ 
					if(this.MinimumVelocity == null || MinimumVelocity < 1 || MinimumVelocity > 127)
					{
						throw new ApplicationException(
							"\nNoteOnVelocityOption Error:\n" +
							"If the NoteOnVelocityOption is being used, then\n" +
							"MinimumVelocity must be set to a value in range [1..127]");
					}
					w.WriteAttributeString("minVelocity", this.MinimumVelocity.ToString());
				}
            }

            if(this.NoteOffOption != NoteOffOption.undefined)
            {
                w.WriteAttributeString("noteOff", this.NoteOffOption.ToString());
            }

            bool isControllingVolume = false;
            if(this.PressureOption != ControllerOption.undefined)
            {
                w.WriteAttributeString("pressure", this.PressureOption.ToString());
                if(this.PressureOption == ControllerOption.volume)
                {
                    WriteMaxMinVolume(w);
                    isControllingVolume = true;
                }
            }

            if(this.PitchWheelOption != ControllerOption.undefined)
            {
                w.WriteAttributeString("pitchWheel", this.PitchWheelOption.ToString());
                if(this.PitchWheelOption == ControllerOption.volume)
                {
                    Debug.Assert(isControllingVolume == false);
                    WriteMaxMinVolume(w);
                    isControllingVolume = true;
                }
            }

            if(this.ModWheelOption != ControllerOption.undefined)
            {
                w.WriteAttributeString("modulation", this.ModWheelOption.ToString());
                if(this.ModWheelOption == ControllerOption.volume)
                {
                    Debug.Assert(isControllingVolume == false);
                    WriteMaxMinVolume(w);
                    isControllingVolume = true;
                }
            }

            if(this.SpeedOption != SpeedOption.undefined)
            {
                w.WriteAttributeString("speedOption", this.SpeedOption.ToString());
				if(SpeedOption != SpeedOption.off)
				{
					if(MaxSpeedPercent == null || MaxSpeedPercent < 100)
					{
						throw new ApplicationException(
							"\nInputControls Error:\n" +
							"If the SpeedOption is set, then MaxSpeedPercent must be set\n" +
							"to a value >= 100.");
					}
					w.WriteAttributeString("maxSpeedPercent", this.MaxSpeedPercent.ToString());
				}
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

        public NoteOnVelocityOption NoteOnVelocityOption = NoteOnVelocityOption.undefined;
        public NoteOffOption NoteOffOption = NoteOffOption.undefined;
        public ControllerOption PressureOption = ControllerOption.undefined;
        public ControllerOption PitchWheelOption = ControllerOption.undefined;
        public ControllerOption ModWheelOption = ControllerOption.undefined;
        public SpeedOption SpeedOption = SpeedOption.undefined;
		public byte? MinimumVelocity = null; // must be set if a velocity option is being used
        public byte? MaximumVolume = null; // must be set if the performer is controlling the volume
		public byte? MinimumVolume = null; // must be set if the performer is controlling the volume
        public int? MaxSpeedPercent = null; // must be set to a value > 100 if the performer is controlling the speed (often set to about 400)
    }

    public enum NoteOnVelocityOption
    {
        undefined, // Dont write the option to the score file. Use the current setting.
		off, // Written to the score file: turn this option off, overriding the current setting
        scaled,
		shared,
		overridden
    };

    public enum NoteOffOption
    {
		undefined, // Dont write the option to the score file. Use the current setting.
		off, // Written to the score file: turn this option off, overriding the current setting
        stopChord, // stop when the current midiChord or midiRest completes
        stopNow, // stop immediately, even inside a midiChord
        fade, // fade velocity to end of trk
		// Both hold options: The Assistant Performer silences the trk when the performer sends a noteOff.
		holdLast, // The Assistant Performer removes noteOff messages from the last moment in the trk to contain any.
		holdAll // The Assistant Performer removes all noteOff messages from the trk.
    };

    /// <summary>
    /// These values are in the order of the controlOptions list defined in
    /// Assistant Performer:PerformersOptionsDialog.js.
    /// The int values may be used to index in a menu list. 
    /// </summary>
    public enum ControllerOption
    {
		undefined = 0, // Dont write the option to the score file. Use the current setting.
		off = 1, // Written to the score file: turn this option off, overriding the current setting
        aftertouch = 2,
        channelPressure = 3,
        pitchWheel = 4,
        modulation = 5,
        volume = 6,
        pan = 7,
        expression = 8,
        timbre = 9,
        brightness = 10,
        effects = 11,
        tremolo = 12,
        chorus = 13,
        celeste = 14,
        phaser = 15
    };

    public enum SpeedOption
    {
		undefined, // Dont write the option to the score file. Use the current setting.
		off, // Written to the score file: turn this option off, overriding the current setting
        noteOnKey,
        noteOnVel,
        pressure,
        pitchWheel,
        modulation
    };
}
