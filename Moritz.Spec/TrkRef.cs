
using System.Diagnostics;
using Moritz.Xml;

namespace Moritz.Spec
{
    public class TrkRef
    {
        /// <summary>
        /// The class used by InputNotes to define a referenced Seq.
        /// </summary>
        /// <param name="notatedInputMidiPitch">The notated input midi pitch</param>
        /// <param name="midiChannel">The referenced output voice's midiChannel</param>
        /// <param name="length">The number of chords and rests in the Seq</param>
        /// <param name="msOffset">The number of milliseconds between the postion of this input chord and the beginning of the Seq.</param>
        public TrkRef(byte notatedInputMidiPitch, byte midiChannel, int length, int msOffset)
        {
            Debug.Assert(notatedInputMidiPitch >= 0);
            Debug.Assert(midiChannel >= 0);
            Debug.Assert(length >= 0);
            Debug.Assert(msOffset >= 0);

            _notatedInputMidiPitch = notatedInputMidiPitch;
            _midiChannel = midiChannel;
            _length = length;
            _msOffset = msOffset;
        }

        internal void WriteSvg(SvgWriter w)
        {
            w.WriteStartElement("trkRef");
            w.WriteAttributeString("midiChannel", _midiChannel.ToString());
            w.WriteAttributeString("length", _length.ToString());
            if(_msOffset > 0)
            {
                w.WriteAttributeString("msOffset", _msOffset.ToString());
            }
            w.WriteEndElement(); // trkRef
        }

        public byte NotatedInputMidiPitch { get { return _notatedInputMidiPitch; } set { _notatedInputMidiPitch = value; } }
        protected byte _notatedInputMidiPitch;

        public byte MidiChannel { get { return _midiChannel; } }
        private byte _midiChannel;

        public int Length { get { return _length; } }
        private int _length;

        public int MsOffset { get { return _msOffset; } }
        private int _msOffset;     
    }
}
