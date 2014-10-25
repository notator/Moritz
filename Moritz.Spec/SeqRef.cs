

using Moritz.Xml;

namespace Moritz.Spec
{
    public class SeqRef
    {
        public SeqRef(byte notatedInputMidiPitch, byte voiceID, int length, InputControls inputControls)
        {
            _notatedInputMidiPitch = notatedInputMidiPitch;
            _voiceID = voiceID;
            _length = length;
            _inputControls = inputControls; // can be null
        }

        internal void WriteSvg(SvgWriter w)
        {
            //w.WriteStartElement("score", "seqRef", null);
            w.WriteStartElement("seqRef");
            w.WriteAttributeString("voiceID", _voiceID.ToString());
            w.WriteAttributeString("length", _length.ToString());

            if(_inputControls != null)
            {
                _inputControls.WriteSvg(w);
            }

            w.WriteEndElement(); // score:seqRef
        }

        public byte NotatedInputMidiPitch { get { return _notatedInputMidiPitch; } set { _notatedInputMidiPitch = value; } }
        protected byte _notatedInputMidiPitch;

        public byte VoiceID { get { return _voiceID; } }
        private byte _voiceID;

        public int Length { get { return _length; } }
        private int _length;

        public InputControls InputControls { get { return _inputControls; } set {_inputControls = value; } }
        private InputControls _inputControls = null;

        
    }
}
