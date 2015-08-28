using System;
using System.Diagnostics;

using Moritz.Xml;

namespace Moritz.Spec
{
    /// <summary>
    /// Subject to the message types defined in inputNotes,
	/// This object defines how Trks react to incoming performed information.
	/// 
	/// An trkOptions element can be contained in the following elements in score files:
	///		inputChord (which contains a list of inputNotes)
	///		inputNote (which contains a list of trkRefs)
	///		trkRef
	///		
	/// See http://james-ingram-act-two.de/open-source/svgScoreExtensions.html for details as to how these trkOptions are used.
	/// 	
    /// The values these options can take in the TrkOptions are defined in enums in this namespace.
    /// The default options (which are not written to score files) are:
	///     velocity="undefined" -- input midi velocities are ignored (the score uses its own, default velocities)
	///     pressure="undefined" -- input channelPressure/aftertouch information is ignored.
	///     trkOff="undefined" -- input midi noteOff messages ae ignored (trks complete as defined).
    ///     pitchWheel="undefined" -- input pitchWheel information is ignored.
    ///     modulation="undefined" -- input modulation wheel information is ignored.
    ///     speedOption="undefined" -- the default durations (set in the score) are used. 
	/// 
	/// In the AssistantPerformer, new TrkOptions objects should be empty -- contain no defined members.
    /// </summary>
    public class TrkOptions
    {
        public TrkOptions()
        {
        }

        public void WriteSvg(SvgWriter w)
        {
            w.WriteStartElement("trkOptions");
			
			if(this.VelocityOption != VelocityOption.undefined)
			{
				if(this.MinimumVelocity == null || MinimumVelocity < 1 || MinimumVelocity > 127)
				{
					Debug.Assert(false,
						"If the VelocityOption is being used, then\n" +
						"MinimumVelocity must be set to a value in range [1..127]");
				}
				w.WriteAttributeString("velocity", this.VelocityOption.ToString());
				w.WriteAttributeString("minVelocity", this.MinimumVelocity.ToString());		
			}

			bool isControllingVolume = false;
			if(this.PressureOption != ControllerType.undefined)
			{
				w.WriteAttributeString("pressure", this.PressureOption.ToString());
				if(this.PressureOption == ControllerType.volume)
				{
					WriteMaxMinVolume(w);
					isControllingVolume = true;
				}
			}

			if(this.TrkOffOption != TrkOffOption.undefined)
			{
				w.WriteAttributeString("trkOff", this.TrkOffOption.ToString());
			}
		
			if(this.PitchWheelOption != ControllerType.undefined)
			{
				w.WriteAttributeString("pitchWheel", this.PitchWheelOption.ToString());
				if(this.PitchWheelOption == ControllerType.volume)
				{
					Debug.Assert(isControllingVolume == false);
					WriteMaxMinVolume(w);
					isControllingVolume = true;
				}
			}

			if(this.ModWheelOption != ControllerType.undefined)
			{
				w.WriteAttributeString("modulation", this.ModWheelOption.ToString());
				if(this.ModWheelOption == ControllerType.volume)
				{
					Debug.Assert(isControllingVolume == false);
					WriteMaxMinVolume(w);
					isControllingVolume = true;
				}
			}

			if(this.SpeedOption != SpeedOption.undefined)
			{
				if(MaxSpeedPercent == null || MaxSpeedPercent < 100)
				{
					Debug.Assert(false,
						"If the SpeedOption is set, then MaxSpeedPercent must be set to a value >= 100.");
				}
				w.WriteAttributeString("speedOption", this.SpeedOption.ToString());
				w.WriteAttributeString("maxSpeedPercent", this.MaxSpeedPercent.ToString());
			}

            w.WriteEndElement(); // score:trkOptions
        }

        private void WriteMaxMinVolume(SvgWriter w)
        {
            if(MaximumVolume == null || MinimumVolume == null)
            {
                Debug.Assert(false,
                    "If any of the continuous controllers is set to control the *volume*,\n" +
                    "then both MaximumVolume and MinimumVolume must also be set.");
            }
            if(MaximumVolume <= MinimumVolume)
            {
				Debug.Assert(false,
                    "MaximumVolume must be greater than MinimumVolume.");
            }
            w.WriteAttributeString("maxVolume", MaximumVolume.ToString());
            w.WriteAttributeString("minVolume", MinimumVolume.ToString());
        }

		/* 
		 * Default values. These values are not written to score files.
		 */
        public VelocityOption VelocityOption = VelocityOption.undefined;
		public ControllerType PressureOption = ControllerType.undefined;
		public TrkOffOption TrkOffOption = TrkOffOption.undefined;
        public ControllerType PitchWheelOption = ControllerType.undefined;
        public ControllerType ModWheelOption = ControllerType.undefined;
        public SpeedOption SpeedOption = SpeedOption.undefined;
		public byte? MinimumVelocity = null; // must be set if a velocity option is being used
        public byte? MaximumVolume = null; // must be set if the performer is controlling the volume
		public byte? MinimumVolume = null; // must be set if the performer is controlling the volume
        public int? MaxSpeedPercent = null; // must be set to a value > 100 if the performer is controlling the speed (often set to about 400)
    }

    public enum VelocityOption
    {
        undefined,
        scaled,
		shared,
		overridden
    };

    public enum TrkOffOption
    {
		undefined,
        stopChord, // stop when the current midiChord or midiRest completes
        stopNow, // stop immediately, even inside a midiChord
        fade, // fade velocity to end of trk
		// Both hold options: The Assistant Performer silences the trk when the performer sends a noteOff.
		holdLast, // The Assistant Performer removes noteOff messages from the last moment in the trk to contain any.
		holdAll // The Assistant Performer removes all noteOff messages from the trk.
    };

    public enum ControllerType
    {
		undefined,
        aftertouch,
        channelPressure,
        pitchWheel,
        modulation,
        volume,
        pan,
        expression,
        timbre,
        brightness,
        effects,
        tremolo,
        chorus,
        celeste,
        phaser
    };

    public enum SpeedOption
    {
		undefined,
        noteOnKey,
        noteOnVel,
        pressure,
        pitchWheel,
        modulation
    };
}
