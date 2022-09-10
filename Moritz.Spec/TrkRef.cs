
using Moritz.Xml;

using System.Diagnostics;

namespace Moritz.Spec
{
    public class TrkRef
    {
        /// <param name="trkIndex">The trk's index (re the top of the system)</param>
        /// <param name="absTrkStartPosition">The trk's absolute startMsPosition</param>
        /// <param name="trkNumMidiObjects">The number of MidiChordDefs and RestDefs.</param>
        /// <param name="trkOptions">If non-null, this trkOptions overrrides the TrkOptions in the InputNote or InputChord</param>
        public TrkRef(int trkIndex, int absTrkStartPosition, int trkNumMidiObjects, TrkOptions trkOptions)
        {
            TrkIndex = trkIndex;
            _trkMsPosition = absTrkStartPosition;
            _trkNumMidiObjects = trkNumMidiObjects;
            TrkOptions = trkOptions;
        }

        internal void WriteSVG(SvgWriter w)
        {
            w.WriteStartElement("trkRef");
            w.WriteAttributeString("trkIndex", TrkIndex.ToString());
            w.WriteAttributeString("nMidiObjects", _trkNumMidiObjects.ToString());
            if(TrkOptions != null)
            {
                TrkOptions.WriteSVG(w, false);
            }
            w.WriteEndElement(); // trk
        }

        private int _trkIndex = int.MaxValue; // the MidiChannel will only be valid if set to a value in range [0..15]
        public int TrkIndex
        {
            get
            {
                return _trkIndex;
            }
            set
            {
                Debug.Assert(value >= 0 && value <= 15);
                _trkIndex = value;
            }
        }

        public int MsPosition { get { return _trkMsPosition; } }
        private readonly int _trkMsPosition;

        private int _trkNumMidiObjects;
        public TrkOptions TrkOptions = null;
    }
}
