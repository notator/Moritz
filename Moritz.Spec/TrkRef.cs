
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
			MidiChannel = trkMidiChannel;
			_trkMsPosition = trkStartPosition;
			_trkNumMidiObjects = trkNumMidiObjects;
			TrkOptions = trkOptions;
		}

		/// <param name="trkDef">The target trk</param>
		/// <param name="trkOptions">If non-null, this trkOptions overrrides the TrkOptions in the InputNote or InputChord</param>
		public TrkRef(Trk trkDef, TrkOptions trkOptions)
		{
			MidiChannel = trkDef.MidiChannel;
			_trkMsPosition = trkDef.MsPosition;
			_trkNumMidiObjects = trkDef.DurationsCount; // includes MidiChordDef, RestDef
			TrkOptions = trkOptions;
		}

        internal void WriteSvg(SvgWriter w)
        {
            w.WriteStartElement("trkRef");
			w.WriteAttributeString("midiChannel", MidiChannel.ToString());
			w.WriteAttributeString("msPosition", _trkMsPosition.ToString());
			w.WriteAttributeString("nMidiObjects", _trkNumMidiObjects.ToString());
			if(TrkOptions != null)
			{
				TrkOptions.WriteSvg(w, false);
			}
            w.WriteEndElement(); // trk
        }

		private int _midiChannel = int.MaxValue; // the MidiChannel will only be valid if set to a value in range [0..15]
		public int MidiChannel
		{
			get
			{
				return _midiChannel;
			}
			set
			{
				Debug.Assert(value >= 0 && value <= 15);
				_midiChannel = value;
			}
		}

		public int MsPosition { get { return _trkMsPosition; } }
        private int _trkMsPosition;

		private int _trkNumMidiObjects;
		public TrkOptions TrkOptions = null;
	}
}
