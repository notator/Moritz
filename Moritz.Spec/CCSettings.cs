using System.Diagnostics;

using Moritz.Globals;
using Moritz.Xml;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Moritz.Spec
{
	/// <summary>		
	/// In a CCsettings object (attached to an InputChordDef), there is logically one TrackCCSettings object
	/// for each output voice in the score.
	/// If the DefaultSettings are not defined, voices having no TrackCCSettings will be unaffected.
	public sealed class CCSettings
	{
		/// <param name="defaultSettings">TrackCCSettings, that initially apply to all output voices. MidiChannel must be null.</param>
		/// <param name="overrideSettings">A list of TrackCCSettings that override the default setting.\n
		///                                Each MidiChannel must be unique and not null.</param>
		public CCSettings(TrackCCSettings defaultSettings, List<TrackCCSettings> overrideSettings)
		{
			#region check args
			// There must be a _defaultSettings and/or an _overrideSettings
			// Note that if the DefaultSettings are not defined, voices having no TrackCCSettings will be unaffected.
			Debug.Assert(defaultSettings != null || overrideSettings != null);

			List<byte?> channels = new List<byte?>();
			if(defaultSettings != null)
			{ 
				Debug.Assert(defaultSettings.MidiChannel == null);
			}
			if(overrideSettings != null)
			{ 
				foreach(TrackCCSettings tccs in overrideSettings)
				{
					Debug.Assert(tccs.MidiChannel != null);
					Debug.Assert(!channels.Contains(tccs.MidiChannel));
					channels.Add(tccs.MidiChannel);
				}
			}
			#endregion check args

			_defaultSettings = defaultSettings;
			_overrideSettings = overrideSettings;
		}

		public void WriteSvg(SvgWriter w)
		{
			w.WriteStartElement("score", "ccSettings", null);

			if(_defaultSettings != null)
			{
				_defaultSettings.WriteSvg(w, "default");
			}
			if(_overrideSettings != null)
			{
				foreach(TrackCCSettings tccs in _overrideSettings)
				{
					tccs.WriteSvg(w, "track");
				}
			}
			w.WriteEndElement(); // score:ccSettings
		}

		public TrackCCSettings DefaultSettings { get { return _defaultSettings; } }
		private TrackCCSettings _defaultSettings = null;
		public ReadOnlyCollection<TrackCCSettings> OverrideSettings { get { return _overrideSettings.AsReadOnly(); } }
		private List<TrackCCSettings> _overrideSettings = null;
	}
	/// TrackCCSettings are switches that are components of InputChordDef.ccSettings objects.
	/// The Assistant Performer sets these values when the chord with which they are associated is performed.
	/// The setting for each of the three continuous controlers persists until it is changed in a later
	/// InputChordDef.ccSettings object.
	/// TrackCCSettings can have CControllerType values (defined in an enum in this file).
	/// The default options are:
	///     pressure="undefined" -- not written to scores. Means "keep the current setting".
	///     pitchWheel="undefined" -- not written to scores. Means "keep the current setting".
	///     modulation="undefined" -- not written to scores. Means "keep the current setting".
	/// To turn a controller off during a score, use the CControllerType.disabled setting.
	/// The continuous controllers are disabled by default in the Assistant Performer.
	/// 
	/// See also: http://james-ingram-act-two.de/open-source/svgScoreExtensions.html
	/// </summary>
	public sealed class TrackCCSettings
	{
		public TrackCCSettings(byte? midiChannel, CCSetting ccSetting)
		{
			_midiChannel = midiChannel;
			AddList(new List<CCSetting>() { ccSetting });
		}

		public TrackCCSettings(byte? midiChannel, List<CCSetting> optList)
		{
			_midiChannel = midiChannel;
			AddList(optList);
		}

		public void WriteSvg(SvgWriter w, string elementName)
		{
			Debug.Assert((_pressureOption != CControllerType.undefined || _pressureVolumeOption == true)
					|| (_modWheelOption != CControllerType.undefined || _modWheelVolumeOption == true)
					|| (_pitchWheelOption != PitchWheelOption.undefined), "Attempt to write an empty TrackCCSettings element.");

			w.WriteStartElement(elementName);

			if(_midiChannel != null)
			{
				w.WriteAttributeString("midiChannel", _midiChannel.ToString());
			}

			bool isControllingVolume = false;
			if(_pressureOption != CControllerType.undefined)
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

			if(_modWheelOption != CControllerType.undefined)
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

			w.WriteEndElement(); // "default" or "track"
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

		public void AddList(List<CCSetting> optList)
		{
			Debug.Assert(optList.Count < 4, "Each of the three continuous controllers can only be set once!");
			Debug.Assert(optList.Count > 0, "Empty TrackCCSettings list.");
			foreach(CCSetting opt in optList)
			{
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

		public void Add(PressureControl pressureControl)
		{
			Debug.Assert(_pressureOption == CControllerType.undefined && _pressureVolumeOption == false,
				"Illegal attempt to set the PressureOption twice.");

			_pressureOption = pressureControl.ControllerType;
		}
		public void Add(ModWheelControl modWheelControllerCCSetting)
		{
			Debug.Assert(_modWheelOption == CControllerType.undefined && _modWheelVolumeOption == false,
				"Illegal attempt to set the ModWheelOption twice.");

			_modWheelOption = modWheelControllerCCSetting.ControllerType;
		}
		public void Add(PressureVolumeControl pressureVolumeCCSetting)
		{
			Debug.Assert(_pressureOption == CControllerType.undefined && _pressureVolumeOption == false,
				"Illegal attempt to set the PressureOption twice.");

			_pressureVolumeOption = true;
			_minimumVolume = pressureVolumeCCSetting.MinimumVolume;
			_maximumVolume = pressureVolumeCCSetting.MaximumVolume;

		}
		public void Add(ModWheelVolumeControl modWheelVolumeCCSetting)
		{
			Debug.Assert(_modWheelOption == CControllerType.undefined && _modWheelVolumeOption == false,
				"Illegal attempt to set the ModWheelOption twice.");
			_modWheelVolumeOption = true;
			_minimumVolume = modWheelVolumeCCSetting.MinimumVolume;
			_maximumVolume = modWheelVolumeCCSetting.MaximumVolume;
		}
		public void Add(PitchWheelPitchControl pitchWheelPitchCCSetting)
		{
			Debug.Assert(_pitchWheelOption == PitchWheelOption.undefined, "Illegal attempt to set the PitchWheelOption twice.");
			_pitchWheelOption = PitchWheelOption.pitch;
			_pitchWheelDeviationOption = pitchWheelPitchCCSetting.PitchWheelDeviation;
		}
		public void Add(PitchWheelPanControl pitchWheelPanCCSetting)
		{
			Debug.Assert(_pitchWheelOption == PitchWheelOption.undefined, "Illegal attempt to set the PitchWheelOption twice.");
			_pitchWheelOption = PitchWheelOption.pan;
			_panOriginOption = pitchWheelPanCCSetting.PanOriginOption;
		}
		public void Add(PitchWheelSpeedControl pitchWheelSpeedCCSetting)
		{
			Debug.Assert(_pitchWheelOption == PitchWheelOption.undefined, "Illegal attempt to set the PitchWheelOption twice.");
			_pitchWheelOption = PitchWheelOption.speed;
			_speedDeviationOption = pitchWheelSpeedCCSetting.SpeedDeviationOption;
		}

		/* 
		 * These default values are not written to score files.
		 */

		public byte? MidiChannel { get { return _midiChannel; } }
		private byte? _midiChannel = null;

		public CControllerType PressureOption
		{
			get { return _pressureOption; }
		}
		private CControllerType _pressureOption = CControllerType.undefined;
		public bool PressureVolumeOption { get { return _pressureVolumeOption; } }
		private bool _pressureVolumeOption = false;

		public PitchWheelOption PitchWheelOption
		{
			get { return _pitchWheelOption; }
		}
		private PitchWheelOption _pitchWheelOption = PitchWheelOption.undefined;

		public CControllerType ModWheelOption { get { return _modWheelOption; } }
		private CControllerType _modWheelOption = CControllerType.undefined;
		public bool ModWheelVolumeOption { get { return _modWheelVolumeOption; } }
		private bool _modWheelVolumeOption = false;

		public byte? PitchWheelDeviationOption { get { return _pitchWheelDeviationOption; } }
		private byte? _pitchWheelDeviationOption = null; // should be set if the PitchWheelOption is set to pitchWheel
		public byte? PanOriginOption { get { return _panOriginOption; } }
		private byte? _panOriginOption = null;  // should be set if the PitchWheelOption is set to pan. (range 0..127, centre is 64)
		public float? SpeedDeviationOption { get { return _speedDeviationOption; } }
		private float? _speedDeviationOption = null; // should be set if the PitchWheelOption is set to speed ( < 1 )
		public byte? MaximumVolume { get { return _maximumVolume; } }
		private byte? _maximumVolume = null; // must be set if the performer is controlling the volume
		public byte? MinimumVolume { get { return _minimumVolume; } }
		private byte? _minimumVolume = null; // must be set if the performer is controlling the volume
	}

	public class CCSetting
	{
		protected CCSetting() { }
	}

	public enum CControllerType
	{
		undefined, // not written to scores. Means "keep the current setting".
		disabled, // Use this to turn a controller off. This is the Assistant Performer's default setting. 
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
	public class ControllerCCSetting : CCSetting
	{
		protected ControllerCCSetting(CControllerType controllerType)
		{
			_controllerType = controllerType;
		}
		public CControllerType ControllerType { get { return _controllerType; } }
		private CControllerType _controllerType;
	}
	public class PressureControl : ControllerCCSetting
	{
		public PressureControl(CControllerType controllerType)
			: base(controllerType)
		{
		}
	}
	public class ModWheelControl : ControllerCCSetting
	{
		public ModWheelControl(CControllerType controllerType)
			: base(controllerType)
		{
		}
	}

	public class PressureVolumeControl : CCSetting
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
	public class ModWheelVolumeControl : CCSetting
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
	public class PitchWheelControl : CCSetting
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
