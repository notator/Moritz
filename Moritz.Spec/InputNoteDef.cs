using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Moritz.Xml;
using Moritz.Globals;

namespace Moritz.Spec
{
	public class InputNoteDef
	{
		public InputNoteDef(byte notatedMidiPitch, InputControls inputControls, List<TrkRef> trkRefs)
		{
			_notatedMidiPitch = notatedMidiPitch;
			_inputControls = inputControls;
			_trkRefs = trkRefs;
		}

		internal void WriteSvg(SvgWriter w)
		{
			w.WriteStartElement("inputNote");
			w.WriteAttributeString("notatedKey", _notatedMidiPitch.ToString());
			if(_inputControls != null)
			{
				_inputControls.WriteSvg(w);
			}
			w.WriteStartElement("trkRefs");
			foreach(TrkRef trkRef in _trkRefs)
			{
				trkRef.WriteSvg(w);
			}
			w.WriteEndElement(); // score:trkRefs
			w.WriteEndElement(); // score:inputNote
		}

		public byte NotatedMidiPitch { get { return _notatedMidiPitch; } set {_notatedMidiPitch = M.MidiValue(value); }}
		private byte _notatedMidiPitch;

		public InputControls InputControls { get { return _inputControls; } set{ _inputControls = value; }}
		private InputControls _inputControls;

		public List<TrkRef> TrkRefs { get { return _trkRefs;}}
		private List<TrkRef> _trkRefs;
	}
}
