using System;
using System.Diagnostics;

using Moritz.Globals;
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
	///     onSeqPedal -- the Seq triggered by noteOn is played as written in the score
	///     onSeqOff="undefined" -- input midi noteOff messages are ignored (the Seq completes as defined).
	///     offSeqPedal -- the Seq triggered by noteOff is played as written in the score
	///     offSeqOff="undefined" -- input midi noteOff messages are ignored (the Seq completes as defined).
	///     pressure="undefined" -- input channelPressure/aftertouch information is ignored.
    ///     pitchWheel="undefined" -- input pitchWheel information is ignored.
    ///     modulation="undefined" -- input modulation wheel information is ignored.
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
			
			if(VelocityOption != VelocityOption.undefined)
			{
				if(MinimumVelocity == null || MinimumVelocity < 1 || MinimumVelocity > 127)
				{
					Debug.Assert(false,
						"If the VelocityOption is being used, then\n" +
						"MinimumVelocity must be set to a value in range [1..127]");
				}
				w.WriteAttributeString("velocity", VelocityOption.ToString());
				w.WriteAttributeString("minVelocity", MinimumVelocity.ToString());		
			}

			if(OnSeqPedalOption != PedalOption.undefined)
			{
				w.WriteAttributeString("onSeqPedal", OnSeqPedalOption.ToString());
			}
			if(OnSeqOffOption != SeqOffOption.undefined)
			{
				w.WriteAttributeString("onSeqOff", OnSeqOffOption.ToString());
			}

			if(OffSeqPedalOption != PedalOption.undefined)
			{
				w.WriteAttributeString("offSeqPedal", OffSeqPedalOption.ToString());
			}
			if(OffSeqOffOption != SeqOffOption.undefined)
			{
				w.WriteAttributeString("offSeqOff", OffSeqOffOption.ToString());
			}

			bool isControllingVolume = false;
			if(PressureOption != ControllerType.undefined)
			{
				w.WriteAttributeString("pressure", PressureOption.ToString());
				if(PressureOption == ControllerType.volume)
				{
					WriteMaxMinVolume(w);
					isControllingVolume = true;
				}
			}

			if(PitchWheelOption != PitchWheelOption.undefined)
			{
				w.WriteAttributeString("pitchWheel", PitchWheelOption.ToString());
				switch(PitchWheelOption)
				{
					case PitchWheelOption.pitch:
						Debug.Assert(PitchWheelDeviationOption != null);
						//(range 0..127)
						Debug.Assert(PitchWheelDeviationOption >= 0 && PitchWheelDeviationOption <= 127);
						w.WriteAttributeString("pitchWheelDeviation", PitchWheelDeviationOption.ToString());
						break;
					case PitchWheelOption.pan:
						Debug.Assert(PanOriginOption != null);
						//(range 0..127, centre is 64)
						Debug.Assert(PanOriginOption >= 0 && PanOriginOption <= 127);
						w.WriteAttributeString("panOrigin", PanOriginOption.ToString());
						break;
					case PitchWheelOption.speed:
						Debug.Assert(SpeedDeviationOption != null);
						// maximum speed is when durations = durations / speedDeviation
						// minimum speed is when durations = durations * speedDeviation 
						Debug.Assert(SpeedDeviationOption > 1.0F);
						w.WriteAttributeString("speedDeviation", ((float)SpeedDeviationOption).ToString(M.En_USNumberFormat));
						break;
				}
			}

			if(ModWheelOption != ControllerType.undefined)
			{
				w.WriteAttributeString("modulation", ModWheelOption.ToString());
				if(ModWheelOption == ControllerType.volume)
				{
					Debug.Assert(isControllingVolume == false);
					WriteMaxMinVolume(w);
				}
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
		public PedalOption OnSeqPedalOption = PedalOption.undefined;
		public SeqOffOption OnSeqOffOption = SeqOffOption.undefined;
		public PedalOption OffSeqPedalOption = PedalOption.undefined;
		public SeqOffOption OffSeqOffOption = SeqOffOption.undefined;
		public PitchWheelOption PitchWheelOption = PitchWheelOption.undefined;
		public byte? PitchWheelDeviationOption = null; // is set if the PitchWheelOption is set to pitchWheel
		public float? SpeedDeviationOption = null; // is set if the PitchWheelOption is set to speed
		public byte? PanOriginOption = null;  // is set if the PitchWheelOption is set to pan. (range 0..127, centre is 64)
		public ControllerType ModWheelOption = ControllerType.undefined;
		public ControllerType PressureOption = ControllerType.undefined;
		public byte? MinimumVelocity = null; // must be set if a velocity option is being used
		public byte? MaximumVolume = null; // is set if ControllerType.volume is being used
		public byte? MinimumVolume = null; // is set if ControllerType.volume is being used
    }

    public enum VelocityOption
    {
        undefined,
        scaled,
		shared,
		overridden
    };

	public enum PedalOption
	{
		undefined,
		holdLast, // remove noteOffs from trk's last moment that contains any, and don't send allNotesOff
		holdAll, // remove all noteOff messages from the track, and don't send allNotesOff
		holdAllStop // like holdAll, but sends AllNotesOff when the track stops (or is stopped)
	};

    public enum SeqOffOption
    {
		undefined,
        stopChord, // stop when the current midiChord or midiRest completes
        stopNow, // stop immediately, even inside a midiChord
        fade // fade velocity to end of trk
    };

    public enum ControllerType
    {
		undefined,
        aftertouch,
        channelPressure,
        modulation,
		volume,
        expression,
        timbre,
        brightness,
        effects,
        tremolo,
        chorus,
        celeste,
        phaser
    };

    public enum PitchWheelOption
    {
		undefined,
        pitch,
        speed,
		pan
    };
}
