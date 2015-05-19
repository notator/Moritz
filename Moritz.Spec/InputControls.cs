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
    /// In addition to the values stored in scores, each of these enums has a "dontOverride" member here in Moritz.
    /// The default options for an Input Voice are:
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
        /// All options are set to .dontOverride by default
        /// </summary>
        public InputControls()
        {
        }

        public void WriteSvg(SvgWriter w)
        {
            w.WriteStartElement("inputControls");

			//if(this.NoteOnKeyOption != Spec.NoteOnKeyOption.dontOverride)
			//{
			//	w.WriteAttributeString("noteOnKey", this.NoteOnKeyOption.ToString());
			//}
            if(this.NoteOnVelocityOption != Spec.NoteOnVelocityOption.dontOverride)
            {
                w.WriteAttributeString("noteOnVel", this.NoteOnVelocityOption.ToString());
            }
            if(this.NoteOffOption != Spec.NoteOffOption.dontOverride)
            {
                w.WriteAttributeString("noteOff", this.NoteOffOption.ToString());
            }
			//if(this.NoteOffOption == NoteOffOption.shortFade)
			//{
			//	if(NumberOfObjectsInFade == null)
			//		throw new ApplicationException(
			//				"\nInputControls Error:\n" +
			//				"If the NoteOffOption is set to 'shortFade',\n" +
			//				"then the NumberOfObjectsInFade must also be set.");
			//	w.WriteAttributeString("shortFade", NumberOfObjectsInFade.ToString());
			//}
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

        //public NoteOnKeyOption NoteOnKeyOption = NoteOnKeyOption.dontOverride;
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

	//public enum NoteOnKeyOption
	//{
	//	dontOverride, // Dont write the option to the score file. Use the current voice setting.
	//	//ignore, // any key will start the Trk. It will play using the notated pitches.
	//	//transpose,
	//	// matchExactly is currently the only way in which noteOns are treated.
	//	matchExactly // the Trk will not start if the key does not match any of the notated pitchesd.
	//};

    public enum NoteOnVelocityOption
    {
        dontOverride, // Dont write the option to the score file. Use the current voice setting.
        ignore,
        scale,
		share,
		overridden
    };

    public enum NoteOffOption
    {
        dontOverride, // Dont write the option to the score file. Use the current setting.
        ignore, // ignore the noteOff and play the trk to completion (as written in the score)
        stopChord, // stop when the current midiChord or midiRest completes
        stopNow, // stop immediately, even inside a midiChord
        fade // fade velocity to end of trk
		// short fade is not currently implemented.
        //shortFade // fade velocity over a fixed number of midiObjects
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
        none, // use the durations in the score
        noteOnKey,
        noteOnVel,
        pressure,
        pitchWheel,
        modulation
    };
}
