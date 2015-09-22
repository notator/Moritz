using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Diagnostics;

using Moritz.Xml;
using Moritz.Globals;

namespace Moritz.Spec
{
	public class ContinuousControls
	{
		protected ContinuousControls(Dictionary<byte, TrkOption> trkChannelTrkOption, TrkOption trkOption)
		{
			_trkChannelTrkOption = trkChannelTrkOption;
			_trkOption = trkOption;
		}

		private void WriteTrkInController(SvgWriter w, byte midiChannel, TrkOption trkOption)
		{
			w.WriteStartElement("trk");
			w.WriteAttributeString("midiChannel", midiChannel.ToString());
			if(trkOption != null)
			{
				TrkOptions tos = new TrkOptions(trkOption);
				tos.WriteSvg(w);
			}
			w.WriteEndElement(); // trk
		}

		protected void WriteSvg(SvgWriter w, string elementName )
		{
			w.WriteStartElement(elementName); // "pressures", "pitchWheels" or "modWheels"
			if(_trkOption != null)
			{
				TrkOptions tos = new TrkOptions(_trkOption);
				tos.WriteSvg(w);
			}
			foreach(KeyValuePair<byte, TrkOption> elem in _trkChannelTrkOption)
			{
				WriteTrkInController(w, elem.Key, elem.Value);
			}
			w.WriteEndElement(); // "pressures", "pitchWheels" or "modWheels"
		}

		protected Dictionary<byte, TrkOption> _trkChannelTrkOption = null;
		protected TrkOption _trkOption = null; 
	}

	//public class Pressures : ContinuousControls
	//{
	//	/// <summary>
	//	/// The pressures element (contained in an SVG noteOn element)
	//	/// </summary>
	//	/// <param name="trkChannelTrkOption">The TrkOptions in this dictionary must be either PressureControls, PressureVolumeControls or null.</param>
	//	/// <param name="trkOption">Must be either a PressureControl, a PressureVolumeControl or null.</param>
	//	public Pressures(Dictionary<byte, TrkOption> trkChannelTrkOption, TrkOption trkOption)
	//		: base(trkChannelTrkOption, trkOption)
	//	{
	//		Debug.Assert(trkOption == null);
	//		foreach(TrkOption opt in trkChannelTrkOption.Values)
	//		{
	//			Debug.Assert(opt == null);
	//		}
	//	}

	//	public void WriteSvg(SvgWriter w)
	//	{
	//		WriteSvg(w, "pressures");
	//	}
	//}

	//public class PitchWheels : ContinuousControls
	//{
	//	/// <summary>
	//	/// The pitchWheels element (contained in an SVG noteOn or noteOff element)
	//	/// </summary>
	//	/// <param name="trkChannelTrkOption">The TrkOptions in this dictionary must be either PitchWheelControls or null.</param>
	//	/// <param name="trkOption">Must be either a PitchWheelControl or null.</param>
	//	public PitchWheels(Dictionary<byte, TrkOption> trkChannelTrkOption, TrkOption trkOption)
	//		: base(trkChannelTrkOption, trkOption)
	//	{
	//		Debug.Assert(trkOption == null || trkOption is PitchWheelControl);
	//		foreach(TrkOption opt in trkChannelTrkOption.Values)
	//		{
	//			Debug.Assert(opt == null || opt is PitchWheelControl);
	//		}
	//	}

	//	public void WriteSvg(SvgWriter w)
	//	{
	//		WriteSvg(w, "pitchWheels");
	//	}
	//}

	//public class ModWheels : ContinuousControls
	//{
	//	/// <summary>
	//	/// The modWheels element (contained in an SVG noteOn or noteOff element)
	//	/// </summary>
	//	/// <param name="trkChannelTrkOption">The TrkOptions in this dictionary must be either ModWheelControls, ModWheelVolumeControls or null.</param>
	//	/// <param name="trkOption">Must be either a ModWheelControl, a ModWheelVolumeControl or null.</param>
	//	public ModWheels(Dictionary<byte, TrkOption> trkChannelTrkOption, TrkOption trkOption)
	//		: base(trkChannelTrkOption, trkOption)
	//	{
	//		Debug.Assert(trkOption == null || trkOption is ModWheelControl || trkOption is ModWheelVolumeControl);
	//		foreach(TrkOption opt in trkChannelTrkOption.Values)
	//		{
	//			Debug.Assert(opt == null || opt is ModWheelControl || opt is ModWheelVolumeControl);
	//		}
	//	}

	//	public void WriteSvg(SvgWriter w)
	//	{
	//		WriteSvg(w, "modWheels");
	//	}
	//}
}
