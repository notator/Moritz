
using System.Diagnostics;
using Moritz.Xml;

namespace Moritz.Spec
{
    public class TrkRef
    {
		/// <param name="trkMsg">The message to be sent to the referenced Trk</param>
		/// <param name="midiChannel">The referenced Trk's midiChannel</param>
		/// <param name="length">The number of chords and rests in the referenced Trk</param>
		/// <param name="msOffset">The number of milliseconds between the postion of the containing input chord and the beginning of the Trk.</param>
		public TrkRef(TrkMessageType trkMsg, byte midiChannel, int length, int msOffset)
		{
			Debug.Assert(midiChannel >= 0);
			Debug.Assert(length >= 0);
			Debug.Assert(msOffset >= 0);

			_trkMsg = trkMsg;
			_midiChannel = midiChannel;
			_length = length;
			_msOffset = msOffset;
		}

        internal void WriteSvg(SvgWriter w)
        {
            w.WriteStartElement("trkRef");
			w.WriteAttributeString("trkMsg", _trkMsg.ToString());
            w.WriteAttributeString("midiChannel", _midiChannel.ToString());
            w.WriteAttributeString("length", _length.ToString());
            if(_msOffset > 0)
            {
                w.WriteAttributeString("msOffset", _msOffset.ToString());
            }		
			if(_inputControls != null)
			{
				_inputControls.WriteSvg(w);
			}
            w.WriteEndElement(); // trkRef
        }

		public TrkMessageType TrkMsg { get { return _trkMsg; } }
		private TrkMessageType _trkMsg; 

        public byte MidiChannel { get { return _midiChannel; } }
        private byte _midiChannel;

        public int Length { get { return _length; } }
        private int _length;

        public int MsOffset { get { return _msOffset; } }
        private int _msOffset;

		public InputControls InputControls { get { return _inputControls; } set { _inputControls = value; } }
		private InputControls _inputControls;     
    }
}
