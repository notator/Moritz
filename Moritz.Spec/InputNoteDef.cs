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
		/// <param name="noteOnTrkOns">Can be null or empty</param>
		/// <param name="noteOnTrkOffs">Can be null or empty</param>
		/// <param name="pressures">Can be null or empty</param>
		/// <param name="noteOffTrkOns">Can be null or empty</param>
		/// <param name="noteOffTrkOffs">Can be null or empty</param>
		/// <param name="trkOptions">Can be null</param>
		public InputNoteDef(byte notatedMidiPitch,
							TrkOns noteOnTrkOns, TrkOffs noteOnTrkOffs,
							Pressures pressures,
							TrkOns noteOffTrkOns, TrkOffs noteOffTrkOffs,
							TrkOptions trkOptions)
		{
			Debug.Assert(notatedMidiPitch >= 0 && notatedMidiPitch <= 127);
			// If trkOptions is null, the higher level trkOptions are used.

			NotatedMidiPitch = notatedMidiPitch;
			NoteOnTrkOns = noteOnTrkOns;
			NoteOnTrkOffs = noteOnTrkOffs;
			NotePressures = pressures;
			NoteOffTrkOns = noteOffTrkOns; 
			NoteOffTrkOffs = noteOffTrkOffs;
			TrkOptions = trkOptions;	
		}

		/// <summary>
		/// This constructs an InputNoteDef that, when the noteOff arrives, turns off all the trks it has turned on. 
		/// </summary>
		/// <param name="notatedMidiPitch">In range 0..127</param>
		/// <param name="noteOnSeqDefs">Must contain at least one SeqDef</param>
		/// <param name="pressures">Can be null or empty</param>
		/// <param name="trkOptions">Can be null</param>
		public InputNoteDef(byte notatedMidiPitch, TrkOns noteOnSeqDef, Pressures pressures, TrkOptions trkOptions)
			: this(notatedMidiPitch, noteOnSeqDef, null, pressures, null, null, trkOptions)
		{
			Debug.Assert(noteOnSeqDef != null);
			List<TrkOff> trkOffs = new List<TrkOff>();
			foreach(TrkOn trkOn in NoteOnTrkOns)
			{ 
				TrkOff trkOff = new TrkOff(trkOn.TrkMidiChannel, trkOn.TrkMsPosition, trkOptions);
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

			if(TrkOptions != null)
			{
				TrkOptions.WriteSvg(w);
			}

			if(NoteOnTrkOns != null || NoteOnTrkOffs != null)
			{ 
				w.WriteStartElement("noteOn");
				WriteNoteOnOff(w, NoteOnTrkOns, NoteOnTrkOffs);
				w.WriteEndElement(); // noteOn
			}

			if(NotePressures != null)
			{
				NotePressures.WriteSvg(w);
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
		public Pressures NotePressures = null;
		public TrkOns NoteOffTrkOns = null;
		public TrkOffs NoteOffTrkOffs = null;

		public TrkOptions TrkOptions = null;
	}
}
