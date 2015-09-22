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

			if(SpeedOption > 0 && SpeedOption != 1)
			{
				w.WriteAttributeString("speed", SpeedOption.ToString(M.En_USNumberFormat));
			}

			if(TrkOffOption != TrkOffOption.undefined)
			{
				w.WriteAttributeString("trkOff", TrkOffOption.ToString());
			}

            w.WriteEndElement(); // score:trkOptions
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
				SpeedControl sc = opt as SpeedControl;
				if(sc != null)
				{
					Add(sc);
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
			}
		}

		public void Add(PedalControl pedalTrkOption)
		{
			_pedalOption = pedalTrkOption.PedalOption;
		}
		public void Add(SpeedControl speedControl)
		{
			_speedOption = speedControl.SpeedFactor;
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

		/* 
		 * These default values are not written to score files.
		 */
		public PedalOption PedalOption { get { return _pedalOption; } }
		private PedalOption _pedalOption = PedalOption.undefined;

		public float SpeedOption { get { return _speedOption; } }
		private float _speedOption = 1;

		public TrkOffOption TrkOffOption { get { return _trkOffOption; } }
		private TrkOffOption _trkOffOption = TrkOffOption.undefined;

		public VelocityOption VelocityOption
		{ 
			get { return _velocityOption; }
		}
        private VelocityOption _velocityOption = VelocityOption.undefined;
		public byte? MinimumVelocity { get { return _minimumVelocity; } }
		private byte? _minimumVelocity = null; // must be set if a velocity option is being used
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

	public class SpeedControl:TrkOption
	{
		/// <param name="speedFactor">A value greater than zero. Greater values mean greater speed.</param>
		public SpeedControl(float speedFactor)
		{
			Debug.Assert(speedFactor > 0, "Error: speedFactor must be greater than zero.");
			_speedFactor = speedFactor;
		}
		public float SpeedFactor { get { return _speedFactor; } }
		private float _speedFactor;
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


}
