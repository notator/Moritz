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
    /// The default options (which are not written to score files) are:
	///     noteOnMsg="trkOn" -- input midi noteOns send trkOn messages
	///     noteOffMsg="trkOff" -- input midi noteOffs send trkOff messages
	///     trkVel="undefined" -- input midi velocities are ignored (the score uses its own, default velocities)
	///     trkOff="stopNow" -- must be defined if "trkOff" messages are being sent.
    ///     pressure="undefined" -- input channelPressure/aftertouch information is ignored.
    ///     pitchWheel="undefined" -- input pitchWheel information is ignored.
    ///     modulation="undefined" -- input modulation wheel information is ignored.
    ///     speedOption="undefined" -- the default durations (set in the score) are used. 
	/// 
	/// In the AssistantPerformer, new InputControls objects should only be initialized with the _defined_ values:
	///    NoteOnTrkMessage = "trkOn"
	///    NoteOffTrkMessage = "trkOff"
	///    TrkOffOption = "stopNow"
	/// The inputControls object can then be completed by settting/adding the values given in the score.
    /// </summary>
    public class InputControls
    {
        public InputControls()
        {
        }

        public void WriteSvg(SvgWriter w)
        {
            w.WriteStartElement("inputControls");

			if(this.NoteOnTrkMessage != TrkMessageType.trkOn)
			{
				w.WriteAttributeString("noteOnMsg", this.NoteOnTrkMessage.ToString());				
			}

			if(this.NoteOffTrkMessage != TrkMessageType.trkOff)
			{
				w.WriteAttributeString("noteOffMsg", this.NoteOffTrkMessage.ToString());
			}
			
			if(this.TrkVelocityOption != TrkVelocityOption.undefined)
			{
				if(this.MinimumVelocity == null || MinimumVelocity < 1 || MinimumVelocity > 127)
				{
					Debug.Assert(false,
						"If the TrkVelocityOption is being used, then\n" +
						"MinimumVelocity must be set to a value in range [1..127]");
				}
				w.WriteAttributeString("trkVel", this.TrkVelocityOption.ToString());
				w.WriteAttributeString("minVelocity", this.MinimumVelocity.ToString());		
			}

			if(this.TrkOffOption != TrkOffOption.stopNow)
			{
				bool trkOffMessagesAreBeingSent = (this.NoteOffTrkMessage == TrkMessageType.trkOff || this.NoteOnTrkMessage == TrkMessageType.trkOff);
				if(trkOffMessagesAreBeingSent)
				{
					if(this.TrkOffOption == TrkOffOption.undefined)
					{
						Debug.Assert(false, "TrkOffOption must be defined if trkOff messages are being sent.");
					}
				}
				else
				{
					if(this.TrkOffOption != TrkOffOption.undefined)
					{ 
						Debug.Assert(false, "TrkOffOption must be undefined if no trkOff messages are being sent.");
					}				
				}
				
				w.WriteAttributeString("trkOff", this.TrkOffOption.ToString());
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

            w.WriteEndElement(); // score:inputControls
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
		public TrkMessageType NoteOnTrkMessage = TrkMessageType.trkOn;
		public TrkMessageType NoteOffTrkMessage = TrkMessageType.trkOff;
        public TrkVelocityOption TrkVelocityOption = TrkVelocityOption.undefined;
		public TrkOffOption TrkOffOption = TrkOffOption.stopNow; // must be set if trkOff messages are being sent
        public ControllerType PressureOption = ControllerType.undefined;
        public ControllerType PitchWheelOption = ControllerType.undefined;
        public ControllerType ModWheelOption = ControllerType.undefined;
        public SpeedOption SpeedOption = SpeedOption.undefined;
		public byte? MinimumVelocity = null; // must be set if a velocity option is being used
        public byte? MaximumVolume = null; // must be set if the performer is controlling the volume
		public byte? MinimumVolume = null; // must be set if the performer is controlling the volume
        public int? MaxSpeedPercent = null; // must be set to a value > 100 if the performer is controlling the speed (often set to about 400)
    }

	/// <summary>
	/// Message types that can be sent to Trks from incoming NoteOns or NoteOffs
	/// </summary>
	public enum TrkMessageType
	{
		undefined,
		trkOn,
		trkOff
	}

    public enum TrkVelocityOption
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
