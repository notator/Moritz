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
		/// <param name="noteOnSeqDef">Can be null or empty</param>
		/// <param name="noteOnTrkOffs">Can be null or empty</param>
		/// <param name="notePressureChannels">Can be null or empty</param>
		/// <param name="noteOffSeqDef">Can be null or empty</param>
		/// <param name="noteOffTrkOffs">Can be null or empty</param>
		/// <param name="inputControls">Can be null</param>
		public InputNoteDef(byte notatedMidiPitch,
							TrkOns noteOnSeqDef, TrkOffs noteOnTrkOffs,
							List<byte> notePressureChannels,
							TrkOns noteOffSeqDef, TrkOffs noteOffTrkOffs,
							InputControls inputControls)
		{
			Debug.Assert(notatedMidiPitch >= 0 && notatedMidiPitch <= 127);
			// If inputControls is null, the higher level inputControls are used.

			NotatedMidiPitch = notatedMidiPitch;
			NoteOnTrkOns = noteOnSeqDef;
			NoteOnTrkOffs = noteOnTrkOffs;
			NotePressureChannels = notePressureChannels;
			NoteOffTrkOns = noteOffSeqDef; 
			NoteOffTrkOffs = noteOffTrkOffs;
			InputControls = inputControls;	
		}

		/// <summary>
		/// This constructs an InputNoteDef that, when the noteOff arrives, turns off all the trks it has turned on. 
		/// </summary>
		/// <param name="notatedMidiPitch">In range 0..127</param>
		/// <param name="noteOnSeqDefs">Must contain at least one SeqDef</param>
		/// <param name="notePressureChannels">Can be null or empty</param>
		/// <param name="inputControls">Can be null</param>
		public InputNoteDef(byte notatedMidiPitch, TrkOns noteOnSeqDef, List<byte> notePressureChannels, InputControls inputControls)
			: this(notatedMidiPitch, noteOnSeqDef, null, notePressureChannels, null, null, inputControls)
		{
			Debug.Assert(noteOnSeqDef != null);
			List<TrkOff> trkOffs = new List<TrkOff>();
			foreach(TrkOn trkOn in NoteOnTrkOns)
			{ 
				TrkOff trkOff = new TrkOff(trkOn.TrkMidiChannel, trkOn.TrkMsPosition, inputControls);
				trkOffs.Add(trkOff);
			}
			NoteOffTrkOffs = new TrkOffs(trkOffs, null);
		}

		private void WriteNoteOnOff(SvgWriter w, TrkOns trkOns, TrkOffs trkOffs)
		{
			if(trkOns != null)
			{
				trkOns.WriteSvg(w);
			}
			if(trkOffs != null)
			{
				trkOffs.WriteSvg(w);
			}
		}

		internal void WriteSvg(SvgWriter w)
		{
			w.WriteStartElement("inputNote");
			w.WriteAttributeString("notatedKey", _notatedMidiPitch.ToString());

			if(InputControls != null)
			{
				InputControls.WriteSvg(w);
			}

			if(NoteOnTrkOns != null || NoteOnTrkOffs != null)
			{ 
				w.WriteStartElement("noteOn");
				WriteNoteOnOff(w, NoteOnTrkOns, NoteOnTrkOffs);
				w.WriteEndElement(); // noteOn
			}

			if(NotePressureChannels != null && NotePressureChannels.Count > 0)
			{
				string midiChannels = M.ByteListToString(NotePressureChannels);
				w.WriteStartElement("pressure");
				w.WriteAttributeString("midiChannels", midiChannels);
				w.WriteEndElement();
			}

			if(NoteOffTrkOns != null || NoteOffTrkOffs != null)
			{
				w.WriteStartElement("noteOff");
				WriteNoteOnOff(w, NoteOffTrkOns, NoteOffTrkOffs);
				w.WriteEndElement(); // noteOff
			}

			w.WriteEndElement(); // score:inputNote N.B. This element can be empty!
		}

		public byte NotatedMidiPitch { get { return _notatedMidiPitch; } set {_notatedMidiPitch = M.MidiValue(value); }}
		private byte _notatedMidiPitch;

		public TrkOns NoteOnTrkOns = null;
		public TrkOffs NoteOnTrkOffs = null;
		public List<byte> NotePressureChannels = null;
		public TrkOns NoteOffTrkOns = null;
		public TrkOffs NoteOffTrkOffs = null;

		public InputControls InputControls = null;
	}
}
