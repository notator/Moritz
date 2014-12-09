
using System.Diagnostics;
using Moritz.Xml;

namespace Moritz.Spec
{
    public class SeqRef
    {
        /// <summary>
        /// The class used by InputNotes to define a referenced Seq.
        /// </summary>
        /// <param name="notatedInputMidiPitch">The notated input midi pitch</param>
        /// <param name="voiceID">The index of the output voice in the algorithm's bar construction</param>
        /// <param name="length">The number of chords and rests in the Seq</param>
        /// <param name="msOffset">The number of milliseconds between the postion of this input chord and the beginning of the Seq.</param>
        /// <param name="inputControls">An InputControls object or null</param>
        public SeqRef(byte notatedInputMidiPitch, byte voiceID, int length, int msOffset, InputControls inputControls)
        {
            Debug.Assert(notatedInputMidiPitch >= 0);
            Debug.Assert(voiceID >= 0);
            Debug.Assert(length >= 0);
            Debug.Assert(msOffset >= 0);

            _notatedInputMidiPitch = notatedInputMidiPitch;
            _voiceID = voiceID;
            _length = length;
            _msOffset = msOffset;
            _inputControls = inputControls; // can be null
        }

        internal void WriteSvg(SvgWriter w)
        {
            //w.WriteStartElement("score", "seqRef", null);
            w.WriteStartElement("seqRef");
            w.WriteAttributeString("voiceID", _voiceID.ToString());
            w.WriteAttributeString("length", _length.ToString());
            if(_msOffset > 0)
            {
                w.WriteAttributeString("msOffset", _msOffset.ToString());
            }
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

        public int MsOffset { get { return _msOffset; } }
        private int _msOffset;

        public InputControls InputControls { get { return _inputControls; } set {_inputControls = value; } }
        private InputControls _inputControls = null;       
    }
}
