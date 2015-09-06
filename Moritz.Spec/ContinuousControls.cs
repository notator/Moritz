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
		protected ContinuousControls(Dictionary<byte, TrkOptions> trkChannelTrkOptions, TrkOptions trkOptions)
		{
			TrkChannelTrkOptions = trkChannelTrkOptions;
			TrkOptions = trkOptions;
		}

		private void WriteTrkInController(SvgWriter w, byte midiChannel, TrkOptions trkOptions)
		{
			w.WriteStartElement("trk");
			w.WriteAttributeString("midiChannel", midiChannel.ToString());
			if(trkOptions != null)
			{
				trkOptions.WriteSvg(w);
			}
			w.WriteEndElement(); // trk
		}

		protected void WriteSvg(SvgWriter w, string elementName )
		{
			w.WriteStartElement(elementName); // "pressures", "pitchWheels" or "modWheels"
			if(TrkOptions != null)
			{
				TrkOptions.WriteSvg(w);
			}
			foreach(KeyValuePair<byte, TrkOptions> elem in TrkChannelTrkOptions)
			{
				WriteTrkInController(w, elem.Key, elem.Value);
			}
			w.WriteEndElement(); // "pressures", "pitchWheels" or "modWheels"
		}

		public Dictionary<byte, TrkOptions> TrkChannelTrkOptions = null;
		public TrkOptions TrkOptions = null; 
	}

	public class Pressures : ContinuousControls
	{
		public Pressures(Dictionary<byte, TrkOptions> trkChannelTrkOptions, TrkOptions trkOptions)
			: base(trkChannelTrkOptions, trkOptions)
		{ }

		public void WriteSvg(SvgWriter w)
		{
			WriteSvg(w, "pressures");
		}
	}

	public class PitchWheels : ContinuousControls
	{
		public PitchWheels(Dictionary<byte, TrkOptions> trkChannelTrkOptions, TrkOptions trkOptions)
			: base(trkChannelTrkOptions, trkOptions)
		{ }

		public void WriteSvg(SvgWriter w)
		{
			WriteSvg(w, "pitchWheels");
		}
	}

	public class ModWheels : ContinuousControls
	{
		public ModWheels(Dictionary<byte, TrkOptions> trkChannelTrkOptions, TrkOptions trkOptions)
			: base(trkChannelTrkOptions, trkOptions)
		{ }

		public void WriteSvg(SvgWriter w)
		{
			WriteSvg(w, "modWheels");
		}
	}
}
