using System.Collections.Generic;
using System.Diagnostics;

namespace Moritz.Spec
{
	public class Pressure
	{
		/// <param name="midiChannel">The midi channel of the Track to be sent a pressure message</param>
		/// <param name="trkOptions">Can be null</param>
		public Pressure(byte midiChannel, TrkOptions trkOptions)
		{
			_midiChannel = midiChannel;
			_trkOptions = trkOptions;
		}

		internal void WriteSvg(Xml.SvgWriter w)
		{
			w.WriteStartElement("pressure");
			w.WriteAttributeString("midiChannel", _midiChannel.ToString());
			if(_trkOptions != null)
			{
				_trkOptions.WriteSvg(w);
			}
			w.WriteEndElement(); // trkOff
		}

		private byte _midiChannel;
		private TrkOptions _trkOptions;
	}
}
