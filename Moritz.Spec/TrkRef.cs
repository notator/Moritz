
using System.Diagnostics;
using Moritz.Xml;

namespace Moritz.Spec
{
    public class TrkRef
    {
		/// <param name="trkMidiChannel">The trk's midi channel</param>
		/// <param name="trkStartPosition">The trk's startMsPosition</param>
		/// <param name="trkNumMidiObjects">The number of MidiChordDefs and RestDefs.</param>
		/// <param name="trkOptions">If non-null, this trkOptions overrrides the TrkOptions in the InputNote or InputChord</param>
		public TrkRef(byte trkMidiChannel, int trkStartPosition, int trkNumMidiObjects, TrkOptions trkOptions)
		{
			_trkMidiChannel = trkMidiChannel;
			_trkMsPosition = trkStartPosition;
			_trkNumMidiObjects = trkNumMidiObjects;
			TrkOptions = trkOptions;
		}

		/// <param name="trkDef">The target trk</param>
		/// <param name="trkOptions">If non-null, this trkOptions overrrides the TrkOptions in the InputNote or InputChord</param>
		public TrkRef(Trk trkDef, TrkOptions trkOptions)
		{
			_trkMidiChannel = trkDef.MidiChannel;
			_trkMsPosition = trkDef.StartMsPosition;
			_trkNumMidiObjects = trkDef.DurationsCount; // includes MidiChordDef, RestDef
			TrkOptions = trkOptions;
		}

        internal void WriteSvg(SvgWriter w, bool inSeq)
        {
            w.WriteStartElement("trk");
			w.WriteAttributeString("midiChannel", _trkMidiChannel.ToString());
			w.WriteAttributeString("msPosition", _trkMsPosition.ToString());
			if(inSeq)
			{ 
				w.WriteAttributeString("nMidiObjects", _trkNumMidiObjects.ToString());
				if(TrkOptions != null)
				{
					TrkOptions.WriteSvg(w, false);
				}
			}
            w.WriteEndElement(); // trk
        }

		public byte MidiChannel { get { return _trkMidiChannel; } }
        private byte _trkMidiChannel;

		public int MsPosition { get { return _trkMsPosition; } }
        private int _trkMsPosition;

		private int _trkNumMidiObjects;
		public TrkOptions TrkOptions = null;
	}
}
