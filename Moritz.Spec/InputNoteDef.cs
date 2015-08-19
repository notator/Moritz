using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Diagnostics;

using Moritz.Xml;
using Moritz.Globals;

namespace Moritz.Spec
{
	public class InputNoteDef
	{
		
		public InputNoteDef(byte notatedMidiPitch, SeqDef seqDef, InputControls inputControls)
		{
			Debug.Assert(notatedMidiPitch >= 0 && notatedMidiPitch <= 127);	
			Debug.Assert(seqDef != null);
			// If inputControls is null, the higher level inputControls are used.

			_notatedMidiPitch = notatedMidiPitch;
			_seqDef = seqDef;
			_inputControls = inputControls;
		}

		internal void WriteSvg(SvgWriter w, int chordMsPosition)
		{
			w.WriteStartElement("inputNote");
			w.WriteAttributeString("notatedKey", _notatedMidiPitch.ToString());
			_seqDef.WriteSvg(w, chordMsPosition);
			if(_inputControls != null)
			{
				_inputControls.WriteSvg(w);
			}

			w.WriteEndElement(); // score:inputNote
		}

		public byte NotatedMidiPitch { get { return _notatedMidiPitch; } set {_notatedMidiPitch = M.MidiValue(value); }}
		private byte _notatedMidiPitch;

		public InputControls InputControls { get { return _inputControls; } set { _inputControls = value; } }
		private InputControls _inputControls;

		public SeqDef SeqDef { get { return _seqDef; } }
		private SeqDef _seqDef;
	}
}
