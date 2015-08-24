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
		/// <summary>
		/// 
		/// </summary>
		/// <param name="notatedMidiPitch">In range 0..127</param>
		/// <param name="noteOnSeqDefs">Must contain at least one SeqDef</param>
		/// <param name="noteOnTrkOffChannels">Can be null or empty</param>
		/// <param name="notePressureChannels">Can be null or empty</param>
		/// <param name="noteOffSeqDefs">Can be null or empty</param>
		/// <param name="noteOffTrkOffChannels">Can be null or empty</param>
		/// <param name="inputControls">Can be null</param>
		public InputNoteDef(byte notatedMidiPitch,
							List<SeqDef> noteOnSeqDefs, List<byte> noteOnTrkOffChannels,
							List<byte> notePressureChannels,
							List<SeqDef> noteOffSeqDefs, List<byte> noteOffTrkOffChannels,
							InputControls inputControls)
		{
			Debug.Assert(notatedMidiPitch >= 0 && notatedMidiPitch <= 127);
			Debug.Assert(noteOnSeqDefs != null && noteOnSeqDefs.Count > 0);
			// If inputControls is null, the higher level inputControls are used.

			NotatedMidiPitch = notatedMidiPitch;
			NoteOnSeqDefs = noteOnSeqDefs;
			NoteOnTrkOffChannels = noteOnTrkOffChannels;
			NotePressureChannels = notePressureChannels;
			NoteOffSeqDefs = noteOffSeqDefs; 
			NoteOffTrkOffChannels = noteOffTrkOffChannels;
			InputControls = inputControls;	

		}

		/// <param name="notatedMidiPitch">In range 0..127</param>
		/// <param name="noteOnSeqDefs">Must contain at least one SeqDef</param>
		/// <param name="notePressureChannels">Can be null or empty</param>
		/// <param name="noteOffTrkOffChannels">Can be null or empty</param>
		/// <param name="inputControls">Can be null</param>
		public InputNoteDef(byte notatedMidiPitch, List<SeqDef> noteOnSeqDefs, List<byte> notePressureChannels, List<byte> noteOffTrkOffChannels, InputControls inputControls)
			: this(notatedMidiPitch, noteOnSeqDefs, null, notePressureChannels, null, noteOffTrkOffChannels, inputControls)
		{ }

		internal void WriteSvg(SvgWriter w)
		{
			w.WriteStartElement("inputNote");
			w.WriteAttributeString("notatedKey", _notatedMidiPitch.ToString());

			if(InputControls != null)
			{
				InputControls.WriteSvg(w);
			}

			if((NoteOnSeqDefs != null && NoteOnSeqDefs.Count > 0) || (NoteOnTrkOffChannels != null && NoteOnTrkOffChannels.Count > 0))
			{ 
				w.WriteStartElement("noteOn");
				if(NoteOnTrkOffChannels != null && NoteOnTrkOffChannels.Count > 0)
				{
					string midiChannelOffs = M.ByteListToString(NoteOnTrkOffChannels);
					w.WriteAttributeString("midiChannelOffs", midiChannelOffs);
				}
				if(NoteOnSeqDefs != null && NoteOnSeqDefs.Count > 0)
				{
					foreach(SeqDef seqDef in NoteOnSeqDefs)
					{
						seqDef.WriteSvg(w);
					}
				}
				w.WriteEndElement(); // noteOn
			}

			if(NotePressureChannels != null && NotePressureChannels.Count > 0)
			{
				string midiChannels = M.ByteListToString(NotePressureChannels);
				w.WriteStartElement("pressure");
				w.WriteAttributeString("midiChannels", midiChannels);
				w.WriteEndElement();
			}

			if((NoteOffSeqDefs != null && NoteOffSeqDefs.Count > 0) || (NoteOffTrkOffChannels != null && NoteOffTrkOffChannels.Count > 0))
			{
				w.WriteStartElement("noteOff");
				if(NoteOffTrkOffChannels != null && NoteOffTrkOffChannels.Count > 0)
				{
					string midiChannelOffs = M.ByteListToString(NoteOffTrkOffChannels);
					w.WriteAttributeString("midiChannelOffs", midiChannelOffs);
				}
				if(NoteOffSeqDefs != null && NoteOffSeqDefs.Count > 0)
				{
					foreach(SeqDef seqDef in NoteOffSeqDefs)
					{
						seqDef.WriteSvg(w);
					}
				}
				w.WriteEndElement(); // noteOff
			}

			w.WriteEndElement(); // score:inputNote N.B. This element can be empty!
		}

		public byte NotatedMidiPitch { get { return _notatedMidiPitch; } set {_notatedMidiPitch = M.MidiValue(value); }}
		private byte _notatedMidiPitch;

		public List<SeqDef> NoteOnSeqDefs = null;
		public List<byte> NoteOnTrkOffChannels = null;
		public List<byte> NotePressureChannels = null;
		public List<SeqDef> NoteOffSeqDefs = null;
		public List<byte> NoteOffTrkOffChannels = null;

		public InputControls InputControls = null;
	}
}
