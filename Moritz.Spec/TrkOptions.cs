using System.Diagnostics;

using Moritz.Globals;
using Moritz.Xml;
using System.Collections.Generic;

namespace Moritz.Spec
{
    /// <summary>
	/// This object defines how Trks react to incoming performed information. 
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
    public sealed class TrkOptions
    {
		public TrkOptions(TrkOption trkOption)
		{
			AddList(new List<TrkOption>() { trkOption });
		}

		public TrkOptions(List<TrkOption> optList)
		{
			AddList(optList);
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
			}
			else if(_pressureVolumeOption == true)
			{
				w.WriteAttributeString("pressure", "volume");
				WriteMaxMinVolume(w);
				isControllingVolume = true;
			}

			if(_pitchWheelOption != PitchWheelOption.undefined)
			{
				w.WriteAttributeString("pitchWheel", _pitchWheelOption.ToString());
				switch(_pitchWheelOption)
				{
					case PitchWheelOption.pitch:
						//(range 0..127)
						Debug.Assert(_pitchWheelDeviationOption != null && _pitchWheelDeviationOption >= 0 && _pitchWheelDeviationOption <= 127);
						w.WriteAttributeString("pitchWheelDeviation", _pitchWheelDeviationOption.ToString());
						break;
					case PitchWheelOption.pan:
						//(range 0..127, centre is 64)
						Debug.Assert(_panOriginOption != null && _panOriginOption >= 0 && _panOriginOption <= 127);
						w.WriteAttributeString("panOrigin", _panOriginOption.ToString());
						break;
					case PitchWheelOption.speed:
						// maximum speed is when durations = durations / speedDeviation
						// minimum speed is when durations = durations * speedDeviation 
						Debug.Assert(_speedDeviationOption != null && _speedDeviationOption > 0);
						w.WriteAttributeString("speedDeviation", ((float)_speedDeviationOption).ToString(M.En_USNumberFormat));
						break;
				}
			}

			if(_modWheelOption != ControllerType.undefined)
			{
				w.WriteAttributeString("modWheel", _modWheelOption.ToString());
			}
			else if(_modWheelVolumeOption == true)
			{
				Debug.Assert(isControllingVolume == false, "Can't control volume with both pressure and modWheel.");
				w.WriteAttributeString("modWheel", "volume");
				WriteMaxMinVolume(w);
				isControllingVolume = true;
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

		public void AddList(List<TrkOption> optList)
		{
			foreach(TrkOption opt in optList)
			{
				PedalControl pto = opt as PedalControl;
				if(pto != null)
				{
					Add(pto);
				}
				TrkOffControl toto = opt as TrkOffControl;
				if(toto != null)
				{
					Add(toto);
				}
				VelocityTrkOption vto = opt as VelocityTrkOption;
				if(vto != null)
				{
					Add(vto);
				}
				PressureControl pcto = opt as PressureControl;
				if(pcto != null)
				{
					Add(pcto);
				}
				ModWheelControl mwcto = opt as ModWheelControl;
				if(mwcto != null)
				{
					Add(mwcto);
				}
				PressureVolumeControl pvto = opt as PressureVolumeControl;
				if(pvto != null)
				{
					Add(pvto);
				}
				ModWheelVolumeControl mwvto = opt as ModWheelVolumeControl;
				if(mwvto != null)
				{
					Add(mwvto);
				}
				PitchWheelPitchControl pwpito = opt as PitchWheelPitchControl;
				if(pwpito != null)
				{
					Add(pwpito);
				}
				PitchWheelPanControl pwpato = opt as PitchWheelPanControl;
				if(pwpato != null)
				{
					Add(pwpato);
				}
				PitchWheelSpeedControl pwsto = opt as PitchWheelSpeedControl;
				if(pwsto != null)
				{
					Add(pwsto);
				}
			}
		}

		public void Add(PedalControl pedalTrkOption)
		{
			_pedalOption = pedalTrkOption.PedalOption;
		}
		public void Add(TrkOffControl trkOffTrkOption)
		{
			_trkOffOption = trkOffTrkOption.TrkOffOption;
		}
		public void Add(VelocityTrkOption velocityTrkOption)
		{
			_velocityOption = velocityTrkOption.VelocityOption;
			_minimumVelocity = velocityTrkOption.MinimumVelocity;
		}
		public void Add(PressureControl pressureControl)
		{
			_pressureOption = pressureControl.ControllerType;
		}
		public void Add(ModWheelControl modWheelControllerTrkOption)
		{
			_modWheelOption = modWheelControllerTrkOption.ControllerType;
		}
		public void Add(PressureVolumeControl pressureVolumeTrkOption)
		{
			_pressureVolumeOption = true;
			_minimumVolume = pressureVolumeTrkOption.MinimumVolume;
			_maximumVolume = pressureVolumeTrkOption.MaximumVolume;

		}
		public void Add(ModWheelVolumeControl modWheelVolumeTrkOption)
		{
			_modWheelVolumeOption = true;
			_minimumVolume = modWheelVolumeTrkOption.MinimumVolume;
			_maximumVolume = modWheelVolumeTrkOption.MaximumVolume;

		}
		public void Add(PitchWheelPitchControl pitchWheelPitchTrkOption)
		{
			_pitchWheelOption = PitchWheelOption.pitch;
			_pitchWheelDeviationOption = pitchWheelPitchTrkOption.PitchWheelDeviation;
		}
		public void Add(PitchWheelPanControl pitchWheelPanTrkOption)
		{
			_pitchWheelOption = PitchWheelOption.pan;
			_panOriginOption = pitchWheelPanTrkOption.PanOriginOption;
		}
		public void Add(PitchWheelSpeedControl pitchWheelSpeedTrkOption)
		{
			_pitchWheelOption = PitchWheelOption.speed;
			_speedDeviationOption = pitchWheelSpeedTrkOption.SpeedDeviationOption;
		}

		/* 
		 * Default values are all null. These values are not written to score files.
		 */
		public PedalOption PedalOption { get { return _pedalOption; } }
		private PedalOption _pedalOption = PedalOption.undefined;

		public TrkOffOption TrkOffOption { get { return _trkOffOption; } }
		private TrkOffOption _trkOffOption = TrkOffOption.undefined;

		public VelocityOption VelocityOption
		{ 
			get { return _velocityOption; }
		}
        private VelocityOption _velocityOption = VelocityOption.undefined;

		public ControllerType PressureOption
		{
			get { return _pressureOption; }
		}
		private ControllerType _pressureOption = ControllerType.undefined;
		public bool PressureVolumeOption { get { return _pressureVolumeOption; } }
		private bool _pressureVolumeOption = false;

		public PitchWheelOption PitchWheelOption
		{
			get { return _pitchWheelOption; }
		}
		private PitchWheelOption _pitchWheelOption = PitchWheelOption.undefined;

		public ControllerType ModWheelOption { get { return _modWheelOption; } }
		private ControllerType _modWheelOption = ControllerType.undefined;
		public bool ModWheelVolumeOption { get { return _modWheelVolumeOption; } }
		private bool _modWheelVolumeOption = false;

		public byte? PitchWheelDeviationOption { get { return _pitchWheelDeviationOption; } }
		private byte? _pitchWheelDeviationOption = null; // should be set if the PitchWheelOption is set to pitchWheel
		public byte? PanOriginOption { get { return _panOriginOption; } }
		private byte? _panOriginOption = null;  // should be set if the PitchWheelOption is set to pan. (range 0..127, centre is 64)
		public float? SpeedDeviationOption { get { return _speedDeviationOption; } }
		private float? _speedDeviationOption = null; // should be set if the PitchWheelOption is set to speed ( < 1 )
		public byte? MinimumVelocity { get { return _minimumVelocity; } }
		private byte? _minimumVelocity = null; // must be set if a velocity option is being used
		public byte? MaximumVolume { get { return _maximumVolume; } }
		private byte? _maximumVolume = null; // must be set if the performer is controlling the volume
		public byte? MinimumVolume { get { return _minimumVolume; } }
		private byte? _minimumVolume = null; // must be set if the performer is controlling the volume
    }

	public class TrkOption
	{
		protected TrkOption() { }
	}

	public enum PedalOption
	{
		undefined,
		holdLast, // remove noteOffs from trk's last moment that contains any, and don't send allNotesOff
		holdAll, // remove all noteOff messages from the track, and don't send allNotesOff
		holdAllStop // like holdAll, but sends AllNotesOff when the track stops (or is stopped)
	};
	public class PedalControl: TrkOption
	{
		public PedalControl(PedalOption pedalOption)
		{
			_pedalOption = pedalOption;
		}
		
		public PedalOption PedalOption {get{return _pedalOption;}}
		private PedalOption _pedalOption;
	}

	public enum TrkOffOption
	{
		undefined,
		stopChord, // stop when the current midiChord or midiRest completes
		stopNow, // stop immediately, even inside a midiChord
		fade // fade velocity to end of trk
	};
	public class TrkOffControl : TrkOption
	{
		public TrkOffControl(TrkOffOption trkOffOption)
		{ 
			_trkOffOption = trkOffOption;
		}

		public TrkOffOption TrkOffOption { get { return _trkOffOption; } }
		private TrkOffOption _trkOffOption;
	}

	public enum VelocityOption
	{
		undefined,
		scaled,
		shared,
		overridden
	};
	public class VelocityTrkOption :TrkOption
	{
		protected VelocityTrkOption(VelocityOption velocityOption, byte minVelocity)
		{
			Debug.Assert(minVelocity > 0 && minVelocity < 128);
			_minVelocity = minVelocity;
			_velocityOption = velocityOption;
		}

		public VelocityOption VelocityOption { get { return _velocityOption; } }
		private VelocityOption _velocityOption;
		public byte MinimumVelocity { get { return _minVelocity; } }
		private byte _minVelocity;
	}
	public class VelocityScaledControl : VelocityTrkOption
	{
		public VelocityScaledControl(byte minVelocity)
			: base(VelocityOption.scaled, minVelocity)
		{
		}
	}
	public class VelocitySharedControl : VelocityTrkOption
	{
		public VelocitySharedControl(byte minVelocity)
			: base(VelocityOption.shared, minVelocity)
		{
		}
	}
	public class VelocityOverriddenControl : VelocityTrkOption
	{
		public VelocityOverriddenControl(byte minVelocity)
			: base(VelocityOption.overridden, minVelocity)
		{
		}
	}

	public enum ControllerType
	{
		undefined,
		aftertouch,
		channelPressure,
		modulation,
		expression,
		timbre,
		brightness,
		effects,
		tremolo,
		chorus,
		celeste,
		phaser
	};
	public class ControllerTrkOption : TrkOption
	{
		protected ControllerTrkOption(ControllerType controllerType)
		{
			_controllerType = controllerType;
		}
		public ControllerType ControllerType {get{return _controllerType;}}
		private ControllerType _controllerType;
	}
	public class PressureControl : ControllerTrkOption
	{
		public PressureControl(ControllerType controllerType)
			: base(controllerType)
		{
		}
	}
	public class ModWheelControl : ControllerTrkOption
	{
		public ModWheelControl(ControllerType controllerType)
			: base(controllerType)
		{
		}
	}

	public class PressureVolumeControl : TrkOption
	{
		public PressureVolumeControl(byte minVolume, byte maxVolume)
		{
			Debug.Assert(minVolume < maxVolume);
			Debug.Assert(minVolume > 0 && minVolume < 128);
			Debug.Assert(maxVolume > 0 && maxVolume < 128);
			_minimumVolume = minVolume;
			_maximumVolume = maxVolume;
		}
		public byte MinimumVolume { get { return _minimumVolume; } }
		private byte _minimumVolume;
		public byte MaximumVolume { get { return _maximumVolume; } }
		private byte _maximumVolume;
	}
	public class ModWheelVolumeControl : TrkOption
	{
		public ModWheelVolumeControl(byte minVolume, byte maxVolume)
		{
			Debug.Assert(minVolume < maxVolume);
			Debug.Assert(minVolume > 0 && minVolume < 128);
			Debug.Assert(maxVolume > 0 && maxVolume < 128);
			_minimumVolume = minVolume;
			_maximumVolume = maxVolume;
		}
		public byte MinimumVolume { get { return _minimumVolume; } }
		private byte _minimumVolume;
		public byte MaximumVolume { get { return _maximumVolume; } }
		private byte _maximumVolume;
	}

	public enum PitchWheelOption
	{
		undefined,
		pitch,
		pan,
		speed
	};
	public class PitchWheelControl : TrkOption
	{
	}
	public class PitchWheelPitchControl : PitchWheelControl
	{
		public PitchWheelPitchControl(byte deviation)
		{
			Debug.Assert(deviation > 0 && deviation < 128);
			_pitchWheelDeviation = deviation;
		}
		public byte PitchWheelDeviation { get { return _pitchWheelDeviation; } }
		private byte _pitchWheelDeviation;
	}
	public class PitchWheelPanControl : PitchWheelControl
	{
		public PitchWheelPanControl(byte origin)
		{
			// pan centre is 64)
			Debug.Assert(origin >= 0 && origin < 128);
			_panOriginOption = origin;
		}
		public byte PanOriginOption { get { return _panOriginOption; } }
		private byte _panOriginOption;
	}
	public class PitchWheelSpeedControl : PitchWheelControl
	{
		public PitchWheelSpeedControl(float maxFactor)
		{
			Debug.Assert(maxFactor > 1.0F);
			_speedDeviationOption = maxFactor;
		}
		public float SpeedDeviationOption { get { return _speedDeviationOption; } }
		private float _speedDeviationOption;
	}  
}
