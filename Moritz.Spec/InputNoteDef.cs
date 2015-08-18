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
		public InputNoteDef(byte notatedMidiPitch, int msPositionInScore, InputControls inputControls)
		{
			_notatedMidiPitch = notatedMidiPitch;
			_inputControls = inputControls;
			_seqDef = new SeqDef(msPositionInScore);
		}

		/// When the performer sends a NoteOn to this InputNoteDef, the TrkDef will be sent the trkOnOrTrkOff message. 
		/// <param name="trkDef">The trkDef to which a message is to be sent.</param>
		/// <param name="trkOnOrTrkOff">The message to be sent.</param>
		public void AddNoteOnTrkMsg(TrkDef trkDef, TrkMessageType trkOnOrTrkOff)
		{
			if(trkOnOrTrkOff == TrkMessageType.trkOn || trkOnOrTrkOff == TrkMessageType.trkOff)
			{
				_seqDef.AddNoteOnTrkMsg(trkDef, trkOnOrTrkOff);
			}
			else
			{
				Debug.Assert(false, "Input NoteOn messages can only send TrkOn or TrkOff messages to Trks.");
			}
		}

		/// When the performer sends a NoteOff to this InputNoteDef, the TrkDef will be sent the trkOnOrTrkOff message. 
		/// <param name="trkDef">The trkDef to which a message is to be sent.</param>
		/// <param name="trkOnOrTrkOff">The message to be sent.</param>
		public void AddNoteOffTrkMsg(TrkDef trkDef, TrkMessageType trkOnOrTrkOff)
		{
			if(trkOnOrTrkOff == TrkMessageType.trkOn || trkOnOrTrkOff == TrkMessageType.trkOff)
			{
				_seqDef.AddNoteOffTrkMsg(trkDef, trkOnOrTrkOff);
			}
			else
			{
				Debug.Assert(false, "Input NoteOff messages can only send TrkOn or TrkOff messages to Trks.");
			}
		}

		/// When the performer sends a pressure message (Aftertouch or ChannelPressure) to this InputNoteDef,
		/// the TrkDef will be sent the TrkMessage. 
		/// <param name="trkDef">The trkDef to which a message is to be sent.</param>
		/// <param name="TrkOnOrTrkOff">The message to be sent.</param>
		public void AddPressureTrkMsg(TrkDef trkDef, TrkMessageType controllerMsg)
		{
			if(!(controllerMsg == TrkMessageType.trkOn || controllerMsg == TrkMessageType.trkOff))
			{
				_seqDef.AddPressureTrkMsg(trkDef, controllerMsg);
			}
			else
			{
				Debug.Assert(false, "Input pressure messages can't send trkOn or trkOff messages to Trks.");				
			}
		}

		internal void WriteSvg(SvgWriter w)
		{
			w.WriteStartElement("inputNote");
			w.WriteAttributeString("notatedKey", _notatedMidiPitch.ToString());
			if(_inputControls != null)
			{
				_inputControls.WriteSvg(w);
			}
			_seqDef.WriteSvg(w);
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
