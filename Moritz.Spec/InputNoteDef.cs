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
							SeqDef noteOnSeqDef, List<TrkOff> noteOnTrkOffs,
							List<byte> notePressureChannels,
							SeqDef noteOffSeqDef, List<TrkOff> noteOffTrkOffs,
							InputControls inputControls)
		{
			Debug.Assert(notatedMidiPitch >= 0 && notatedMidiPitch <= 127);
			// If inputControls is null, the higher level inputControls are used.

			NotatedMidiPitch = notatedMidiPitch;
			NoteOnSeqDef = noteOnSeqDef;
			NoteOnTrkOffs = noteOnTrkOffs;
			NotePressureChannels = notePressureChannels;
			NoteOffSeqDef = noteOffSeqDef; 
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
		public InputNoteDef(byte notatedMidiPitch, SeqDef noteOnSeqDef, List<byte> notePressureChannels, InputControls inputControls)
			: this(notatedMidiPitch, noteOnSeqDef, null, notePressureChannels, null, null, inputControls)
		{
			Debug.Assert(noteOnSeqDef != null);
			foreach(TrkRef trkRef in NoteOnSeqDef.TrkRefs)
			{ 
				TrkOff trkOff = new TrkOff(trkRef.TrkMidiChannel, trkRef.TrkMsPosition, inputControls);
				NoteOffTrkOffs = new List<TrkOff>(){trkOff};
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

			if(NoteOnSeqDef != null || (NoteOnTrkOffs != null && NoteOnTrkOffs.Count > 0))
			{ 
				w.WriteStartElement("noteOn");
				if(NoteOnTrkOffs != null && NoteOnTrkOffs.Count > 0)
				{
					w.WriteStartElement("trkOffs");
					foreach(TrkOff trkOff in NoteOnTrkOffs)
					{
						trkOff.WriteSvg(w);
					}
					w.WriteEndElement();
				}
				if(NoteOnSeqDef != null)
				{
					NoteOnSeqDef.WriteSvg(w);
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

			if(NoteOffSeqDef != null || (NoteOffTrkOffs != null && NoteOffTrkOffs.Count > 0))
			{
				w.WriteStartElement("noteOff");
				if(NoteOffTrkOffs != null && NoteOffTrkOffs.Count > 0)
				{
					w.WriteStartElement("trkOffs");
					foreach(TrkOff trkOff in NoteOffTrkOffs)
					{
						trkOff.WriteSvg(w);
					}
					w.WriteEndElement();
				}
				if(NoteOffSeqDef != null)
				{
					NoteOffSeqDef.WriteSvg(w);
				}
				w.WriteEndElement(); // noteOff
			}

			w.WriteEndElement(); // score:inputNote N.B. This element can be empty!
		}

		public byte NotatedMidiPitch { get { return _notatedMidiPitch; } set {_notatedMidiPitch = M.MidiValue(value); }}
		private byte _notatedMidiPitch;

		public SeqDef NoteOnSeqDef = null;
		public List<TrkOff> NoteOnTrkOffs = null;
		public List<byte> NotePressureChannels = null;
		public SeqDef NoteOffSeqDef = null;
		public List<TrkOff> NoteOffTrkOffs = null;

		public InputControls InputControls = null;
	}
}
