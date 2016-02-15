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
	public class NoteTrigger
	{
		protected NoteTrigger(SeqRef seqRef, List<byte> trkOffs, TrkOptions trkOptions)
		{
			_seqRef = seqRef;
			_trkOffs = trkOffs;
			_trkOptions = trkOptions;
		}

		protected void WriteSvg(SvgWriter w, string elementName)
		{
			w.WriteStartElement(elementName); // "noteOn" or "noteOff"

			if(_trkOptions != null)
			{
				_trkOptions.WriteSvg(w, false);
			}

			if(_seqRef != null)
			{
				_seqRef.WriteSvg(w);
			}

			if(_trkOffs != null && _trkOffs.Count > 0)
			{
				StringBuilder sb = new StringBuilder();
				foreach(byte midiChannel in _trkOffs)
				{
					sb.Append(midiChannel);
					sb.Append(' ');
				}
				sb.Remove(sb.Length - 1, 1);

				w.WriteStartElement("trkOffs");

				w.WriteAttributeString("midiChannels", sb.ToString());

				w.WriteEndElement(); // trkOffs
			}

			w.WriteEndElement(); // noteOn or noteOff
		}

		public SeqRef SeqRef { get { return _seqRef; } }
		private SeqRef _seqRef = null;

		public TrkOptions TrkOptions { get { return _trkOptions; } set { _trkOptions = value; } }
		private TrkOptions _trkOptions = null;

		public List<byte> TrkOffs { get { return _trkOffs; } set {_trkOffs = value;}}
		protected List<byte> _trkOffs = null;
	}

	public class NoteOn : NoteTrigger
	{
		public NoteOn(SeqRef seqRef, List<byte> trkOffs, TrkOptions trkOptions)
			: base(seqRef, trkOffs, trkOptions)
		{
		}

		// a NoteOn that uses no continuous controllers, turns no trks off, and has no trkOptions.
		public NoteOn(SeqRef seqRef)
			: base(seqRef, new List<byte>(), null)
		{

		}

		public void WriteSvg(SvgWriter w)
		{
			WriteSvg(w, "noteOn");
		}
	}

	public class NoteOff : NoteTrigger
	{
		public NoteOff(NoteOn noteOn, SeqRef seqRef, TrkOptions trkOptions)
			: base(seqRef, new List<byte>(), trkOptions)
		{
			foreach(TrkRef tr in noteOn.SeqRef.TrkRefs)
			{
				_trkOffs.Add(tr.MidiChannel);
			}
		}

		/// <summary>
		/// A NoteOff that does nothing except turn off the trks that were turned on by the noteOn argument.
		/// </summary>
		public NoteOff(NoteOn noteOn)
			: this(noteOn, null, null)
		{
		}

		public void WriteSvg(SvgWriter w)
		{
			WriteSvg(w, "noteOff");
		}
	}
}
