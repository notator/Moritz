
using System.Diagnostics;
using Moritz.Xml;

namespace Moritz.Spec
{
	/// Describes what happens when a NoteOn, NoteOff or pressure info is received by an InputNote.
    public class TrkRef
    {
		/// <param name="trkDef">The target trk's midi channel</param>
		/// <param name="trkStartPosition">The target trk's startMsPosition</param>
		/// <param name="trkDurationsCount">The number of MidiChordDefs and RestDefs in the target trk.</param>
		/// <param name="inputControls">If non-null, this inputControls overrrides the InputControls in the InputNote or InputChord</param>
		public TrkRef(byte trkMidiChannel, int trkStartPosition, int trkDurationsCount, InputControls inputControls)
		{
			_trkMidiChannel = trkMidiChannel;
			_trkMsPosition = trkStartPosition;
			_trkDurationsCount = trkDurationsCount;
			_inputControls = inputControls;
		}

		/// <param name="trkDef">The target trk</param>
		/// <param name="inputControls">If non-null, this inputControls overrrides the InputControls in the InputNote or InputChord</param>
		public TrkRef(TrkDef trkDef, InputControls inputControls)
		{
			_trkMidiChannel = trkDef.MidiChannel;
			_trkMsPosition = trkDef.StartMsPosition;
			_trkDurationsCount = trkDef.DurationsCount; // includes MidiChordDef, RestDef
			_inputControls = inputControls;
		}

        internal void WriteSvg(SvgWriter w, int chordMsPosition)
        {
			int msOffset = _trkMsPosition - chordMsPosition;
			Debug.Assert(msOffset >= 0);

            w.WriteStartElement("trkRef");
			w.WriteAttributeString("midiChannel", _trkMidiChannel.ToString());
			if(msOffset > 0)
			{
				w.WriteAttributeString("msOffset", msOffset.ToString());
			}
			w.WriteAttributeString("durationsCount", _trkDurationsCount.ToString());
			if(_inputControls != null)
			{
				_inputControls.WriteSvg(w);
			}
            w.WriteEndElement(); // trkRef
        }

        private byte _trkMidiChannel;
        private int _trkMsPosition;
		private int _trkDurationsCount;
		private InputControls _inputControls;

		public int TrkMsPosition { get{return _trkMsPosition;}}
	}
}
