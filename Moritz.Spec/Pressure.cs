using System.Collections.Generic;
using System.Diagnostics;

namespace Moritz.Spec
{
	public class Pressure
	{
		/// <param name="midiChannel">The midi channel of the Track to be sent a pressure message</param>
		/// <param name="inputControls">Can be null</param>
		public Pressure(byte midiChannel, InputControls inputControls)
		{
			_midiChannel = midiChannel;
			_inputControls = inputControls;
		}

		internal void WriteSvg(Xml.SvgWriter w)
		{
			w.WriteStartElement("pressure");
			w.WriteAttributeString("midiChannel", _midiChannel.ToString());
			if(_inputControls != null)
			{
				_inputControls.WriteSvg(w);
			}
			w.WriteEndElement(); // trkOff
		}

		private byte _midiChannel;
		private InputControls _inputControls;
	}
}
