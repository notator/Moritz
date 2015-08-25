using System.Collections.Generic;
using System.Diagnostics;

namespace Moritz.Spec
{
	public class TrkOff
	{
		/// <param name="trkMidiChannel">The midi channel of the Trk to be sent a trkOff message</param>
		/// <param name="trkMsPosition">The msPosition (in the score) of the Trk to be sent a trkOff message</param>
		/// <param name="inputControls">Can be null</param>
		public TrkOff(byte trkMidiChannel, int trkMsPosition, InputControls inputControls)
		{
			_midiChannel = trkMidiChannel;
			_trkMsPosition = trkMsPosition;
			_inputControls = inputControls;
		}

		internal void WriteSvg(Xml.SvgWriter w)
		{
			w.WriteStartElement("trkOff");
			w.WriteAttributeString("midiChannel", _midiChannel.ToString());
			w.WriteAttributeString("msPosition", _trkMsPosition.ToString());
			if(_inputControls != null)
			{
				_inputControls.WriteSvg(w);
			}
			w.WriteEndElement(); // trkOff
		}

		private byte _midiChannel;
		private int _trkMsPosition;
		private InputControls _inputControls;
	}
}
