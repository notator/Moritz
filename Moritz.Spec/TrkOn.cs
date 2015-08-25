
using System.Diagnostics;
using Moritz.Xml;

namespace Moritz.Spec
{
	/// Describes what happens when a NoteOn, NoteOff or pressure info is received by an InputNote.
    public class TrkOn
    {
		/// <param name="trkDef">The target trk's midi channel</param>
		/// <param name="trkStartPosition">The target trk's startMsPosition</param>
		/// <param name="trkNumMidiObjects">The number of MidiChordDefs and RestDefs in the target trk.</param>
		/// <param name="inputControls">If non-null, this inputControls overrrides the InputControls in the InputNote or InputChord</param>
		public TrkOn(byte trkMidiChannel, int trkStartPosition, int trkNumMidiObjects, InputControls inputControls)
		{
			_trkMidiChannel = trkMidiChannel;
			_trkMsPosition = trkStartPosition;
			_trkNumMidiObjects = trkNumMidiObjects;
			_inputControls = inputControls;
		}

		/// <param name="trkDef">The target trk</param>
		/// <param name="inputControls">If non-null, this inputControls overrrides the InputControls in the InputNote or InputChord</param>
		public TrkOn(TrkDef trkDef, InputControls inputControls)
		{
			_trkMidiChannel = trkDef.MidiChannel;
			_trkMsPosition = trkDef.StartMsPosition;
			_trkNumMidiObjects = trkDef.DurationsCount; // includes MidiChordDef, RestDef
			_inputControls = inputControls;
		}

        internal void WriteSvg(SvgWriter w)
        {
            w.WriteStartElement("trkOn");
			w.WriteAttributeString("midiChannel", _trkMidiChannel.ToString());
			w.WriteAttributeString("msPosition", _trkMsPosition.ToString());
			w.WriteAttributeString("nMidiObjects", _trkNumMidiObjects.ToString());
			if(_inputControls != null)
			{
				_inputControls.WriteSvg(w);
			}
            w.WriteEndElement(); // trkOn
        }

		public byte TrkMidiChannel { get { return _trkMidiChannel; } }
        private byte _trkMidiChannel;

		public int TrkMsPosition { get { return _trkMsPosition; } }
        private int _trkMsPosition;

		private int _trkNumMidiObjects;
		private InputControls _inputControls;
	}
}
