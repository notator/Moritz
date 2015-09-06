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
	///     pedal="undefined" -- trk noteOffs are performed as written in the score
	///     trkOff="undefined" -- input midi noteOff messages ae ignored (trks complete as defined).
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
			
			if(_velocityOption != VelocityOption.undefined)
			{
				if(_minimumVelocity == null || _minimumVelocity < 1 || _minimumVelocity > 127)
				{
					Debug.Assert(false,
						"If the VelocityOption is being used, then\n" +
						"MinimumVelocity must be set to a value in range [1..127]");
				}
				w.WriteAttributeString("velocity", _velocityOption.ToString());
				w.WriteAttributeString("minVelocity", _minimumVelocity.ToString());		
			}

			if(PedalOption != PedalOption.undefined)
			{
				w.WriteAttributeString("pedal", PedalOption.ToString());
			}

			if(TrkOffOption != TrkOffOption.undefined)
			{
				w.WriteAttributeString("trkOff", TrkOffOption.ToString());
			}

			bool isControllingVolume = false;
			if(_pressureOption != ControllerType.undefined)
			{
				w.WriteAttributeString("pressure", _pressureOption.ToString());
				if(_pressureOption == ControllerType.volume)
				{
					WriteMaxMinVolume(w);
					isControllingVolume = true;
				}
			}

			if(_pitchWheelOption != PitchWheelOption.undefined)
			{
				w.WriteAttributeString("pitchWheel", _pitchWheelOption.ToString());
				switch(_pitchWheelOption)
				{
					case PitchWheelOption.pitch:
						Debug.Assert(_pitchWheelDeviationOption != null);
						//(range 0..127)
						Debug.Assert(_pitchWheelDeviationOption >= 0 && _pitchWheelDeviationOption <= 127);
						w.WriteAttributeString("pitchWheelDeviation", _pitchWheelDeviationOption.ToString());
						break;
					case PitchWheelOption.pan:
						Debug.Assert(_panOriginOption != null);
						//(range 0..127, centre is 64)
						Debug.Assert(_panOriginOption >= 0 && _panOriginOption <= 127);
						w.WriteAttributeString("panOrigin", _panOriginOption.ToString());
						break;
					case PitchWheelOption.speed:
						Debug.Assert(_speedDeviationOption != null);
						// maximum speed is when durations = durations / speedDeviation
						// minimum speed is when durations = durations * speedDeviation 
						Debug.Assert(_speedDeviationOption > 0);
						w.WriteAttributeString("speedDeviation", ((float)_speedDeviationOption).ToString(M.En_USNumberFormat));
						break;
				}
			}

			if(_modWheelOption != ControllerType.undefined)
			{
				w.WriteAttributeString("modWheel", _modWheelOption.ToString());
				if(_modWheelOption == ControllerType.volume)
				{
					Debug.Assert(isControllingVolume == false);
					WriteMaxMinVolume(w);
					isControllingVolume = true;
				}
			}

            w.WriteEndElement(); // score:trkOptions
        }

        private void WriteMaxMinVolume(SvgWriter w)
        {
            if(_maximumVolume == null || _minimumVolume == null)
            {
                Debug.Assert(false,
                    "If any of the continuous controllers is set to control the *volume*,\n" +
                    "then both MaximumVolume and MinimumVolume must also be set.\n\n" +
					"Use either the PressureVolume(...) or ModWheelVolume(...) constructor.");
            }
            if(_maximumVolume <= _minimumVolume)
            {
				Debug.Assert(false,
                    "MaximumVolume must be greater than MinimumVolume.");
            }
            w.WriteAttributeString("maxVolume", _maximumVolume.ToString());
            w.WriteAttributeString("minVolume", _minimumVolume.ToString());
        }

		public void Add(VelocityScaledTrkOption velocityTrkOption)
		{
			_velocityOption = velocityTrkOption.VelocityOption;
			_minimumVelocity = velocityTrkOption.MinimumVelocity;
		}

		public void Add(VelocitySharedTrkOption velocityTrkOption)
		{
			_velocityOption = velocityTrkOption.VelocityOption;
			_minimumVelocity = velocityTrkOption.MinimumVelocity;
		}

		public void Add(VelocityOverriddenTrkOption velocityTrkOption)
		{
			_velocityOption = velocityTrkOption.VelocityOption;
			_minimumVelocity = velocityTrkOption.MinimumVelocity;
		}

		public void Add(PressureControllerTrkOption pressureControllerTrkOption)
		{
			_pressureOption = pressureControllerTrkOption.PressureOption;
		}

		public void Add(PressureVolumeTrkOption pressureVolumeTrkOption)
		{
			_pressureOption = pressureVolumeTrkOption.PressureOption;
			_minimumVolume = pressureVolumeTrkOption.MinimumVolume;
			_maximumVolume = pressureVolumeTrkOption.MaximumVolume;
		}

		public void Add(PitchWheelPitchTrkOption pitchWheelPitchTrkOption)
		{
			_pitchWheelOption = pitchWheelPitchTrkOption.PitchWheelOption;
			_pitchWheelDeviationOption = pitchWheelPitchTrkOption.PitchWheelDeviationOption;
		}

		public void Add(PitchWheelPanTrkOption pitchWheelPanTrkOption)
		{
			_pitchWheelOption = pitchWheelPanTrkOption.PitchWheelOption;
			_panOriginOption = pitchWheelPanTrkOption.PanOriginOption;
		}

		public void Add(PitchWheelSpeedTrkOption pitchWheelSpeedTrkOption)
		{
			_pitchWheelOption = pitchWheelSpeedTrkOption.PitchWheelOption;
			_speedDeviationOption = pitchWheelSpeedTrkOption.SpeedDeviationOption;
		}

		public void Add(ModWheelControllerTrkOption modWheelControllerTrkOption)
		{
			_modWheelOption = modWheelControllerTrkOption.ModWheelOption;
		}
		
		public void Add(ModWheelVolumeTrkOption modWheelVolumeTrkOption)
		{
			_modWheelOption = modWheelVolumeTrkOption.ModWheelOption;
			_minimumVolume = modWheelVolumeTrkOption.MinimumVolume;
			_maximumVolume = modWheelVolumeTrkOption.MaximumVolume;
		}

		/* 
		 * Default values are all null. These values are not written to score files.
		 */
		public PedalOption PedalOption = PedalOption.undefined;
		public TrkOffOption TrkOffOption = TrkOffOption.undefined;

		public VelocityOption VelocityOption
		{ 
			get { return _velocityOption; }
			set
			{ 
				Debug.Assert(false, "Set this option by calling TrkOptions.Add(new VelocityTrkOption(...)).");
			}
		}
        protected VelocityOption _velocityOption = VelocityOption.undefined;

		public ControllerType PressureOption
		{
			get { return _pressureOption; }
			set
			{ 
				if(value == ControllerType.volume)
				{
					Debug.Assert(false, "Set the pressure-volume option by calling TrkOptions.Add(new PressureVolumeTrkOption(...)).");
				}
				else
				{
					_pressureOption = value;
				}
			} 
		}
		protected ControllerType _pressureOption = ControllerType.undefined;

		public PitchWheelOption PitchWheelOption
		{
			get { return _pitchWheelOption; }
			set
			{
				Debug.Assert(false, "Set the pitchWheel option by calling one of the following:\n\n" +
					"TrkOptions.Add(new PitchWheelPitchTrkOption(...)),\n" +
					"TrkOptions.Add(new PitchWheelPanTrkOption(...)),\n" +
					"TrkOptions.Add(new PitchWheelSpeedTrkOption(...)).");
			}
		}
		protected PitchWheelOption _pitchWheelOption = PitchWheelOption.undefined;

		public ControllerType ModWheelOption
		{
			get { return _modWheelOption; }
			set
			{
				if(value == ControllerType.volume)
				{
					Debug.Assert(false, "Set the modWheel-volume option by calling TrkOptions.Add(new ModWheelVolumeTrkOption(...)).");
				}
				else
				{
					_modWheelOption = value;
				}
			}
		}
		protected ControllerType _modWheelOption = ControllerType.undefined;

		public byte? PitchWheelDeviationOption { get { return _pitchWheelDeviationOption; } }
		protected byte? _pitchWheelDeviationOption = null; // should be set if the PitchWheelOption is set to pitchWheel
		public byte? PanOriginOption { get { return _panOriginOption; } }
		protected byte? _panOriginOption = null;  // should be set if the PitchWheelOption is set to pan. (range 0..127, centre is 64)
		public float? SpeedDeviationOption { get { return _speedDeviationOption; } }
		protected float? _speedDeviationOption = null; // should be set if the PitchWheelOption is set to speed ( < 1 )
		public byte? MinimumVelocity { get { return _minimumVelocity; } }
		protected byte? _minimumVelocity = null; // must be set if a velocity option is being used
		public byte? MaximumVolume { get { return _maximumVolume; } }
        protected byte? _maximumVolume = null; // must be set if the performer is controlling the volume
		public byte? MinimumVolume { get { return _minimumVolume; } }
		protected byte? _minimumVolume = null; // must be set if the performer is controlling the volume
    }

	public class VelocityScaledTrkOption : TrkOptions
	{
		public VelocityScaledTrkOption(byte minVelocity)
			: base()
		{
			Debug.Assert(minVelocity > 0 && minVelocity < 128);
			_velocityOption = VelocityOption.scaled;
			_minimumVelocity = minVelocity;
		}
	}
	public class VelocitySharedTrkOption : TrkOptions
	{
		public VelocitySharedTrkOption(byte minVelocity)
			: base()
		{
			Debug.Assert(minVelocity > 0 && minVelocity < 128);
			_velocityOption = VelocityOption.shared;
			_minimumVelocity = minVelocity;
		}
	}
	public class VelocityOverriddenTrkOption : TrkOptions
	{
		public VelocityOverriddenTrkOption(byte minVelocity)
			: base()
		{
			Debug.Assert(minVelocity > 0 && minVelocity < 128);
			_velocityOption = VelocityOption.overridden;
			_minimumVelocity = minVelocity;
		}
	}

	public class PressureControllerTrkOption : TrkOptions
	{
		public PressureControllerTrkOption(ControllerType controller)
		{
			PressureOption = controller; // checks that it is not volume.
		}
	}

	public class PressureVolumeTrkOption : TrkOptions
	{
		public PressureVolumeTrkOption(byte minVolume, byte maxVolume)
			: base()
		{
			Debug.Assert(minVolume < maxVolume);
			Debug.Assert(minVolume > 0 && minVolume < 128);
			Debug.Assert(maxVolume > 0 && maxVolume < 128);
			_pressureOption = ControllerType.volume;
			_minimumVolume = minVolume;
			_maximumVolume = maxVolume;
		}
	}

	#region PitchWheelOptions
	public class PitchWheelPitchTrkOption : TrkOptions
	{
		public PitchWheelPitchTrkOption(byte deviation)
			: base()
		{
			Debug.Assert(deviation > 0 && deviation < 128);
			_pitchWheelOption = PitchWheelOption.pitch;
			_pitchWheelDeviationOption = deviation;
		}
	}

	public class PitchWheelPanTrkOption : TrkOptions
	{
		public PitchWheelPanTrkOption(byte origin)
			: base()
		{
			// pan centre is 64)
			Debug.Assert(origin >= 0 && origin < 128);
			_pitchWheelOption = PitchWheelOption.pan;
			_panOriginOption = origin;
		}
	}

	public class PitchWheelSpeedTrkOption : TrkOptions
	{
		public PitchWheelSpeedTrkOption(float maxFactor)
			: base()
		{
			Debug.Assert(maxFactor > 1.0F);
			_pitchWheelOption = PitchWheelOption.speed;
			_speedDeviationOption = maxFactor;
		}
	}
	#endregion


	public class ModWheelControllerTrkOption : TrkOptions
	{
		public ModWheelControllerTrkOption(ControllerType controller)
		{
			ModWheelOption = controller; // checks that it is not volume.
		}
	}

	public class ModWheelVolumeTrkOption : TrkOptions
	{
		public ModWheelVolumeTrkOption(byte minVolume, byte maxVolume)
			: base()
		{
			Debug.Assert(minVolume < maxVolume);
			Debug.Assert(minVolume > 0 && minVolume < 128);
			Debug.Assert(maxVolume > 0 && maxVolume < 128);
			_modWheelOption = ControllerType.volume;
			_minimumVolume = minVolume;
			_maximumVolume = maxVolume;
		}
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

    public enum TrkOffOption
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
