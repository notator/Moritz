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
		protected NoteTrigger(Seq seq, List<byte> trkOffs, TrkOptions trkOptions)
		{
			_seq = seq;
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

			if(_seq != null)
			{
				_seq.WriteSvg(w);
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

		public Seq Seq { get { return _seq; } }
		private Seq _seq = null;

		public TrkOptions TrkOptions { get { return _trkOptions; } set { _trkOptions = value; } }
		private TrkOptions _trkOptions = null;

		public List<byte> TrkOffs { get { return _trkOffs; } set {_trkOffs = value;}}
		protected List<byte> _trkOffs = null;
	}

	public class NoteOn : NoteTrigger
	{
		public NoteOn(Seq seq, List<byte> trkOffs, TrkOptions trkOptions)
			: base(seq, trkOffs, trkOptions)
		{
		}

		// a NoteOn that uses no continuous controllers, turns no trks off, and has no trkOptions.
		public NoteOn(Seq seq)
			: base(seq, new List<byte>(), null)
		{

		}

		public void WriteSvg(SvgWriter w)
		{
			WriteSvg(w, "noteOn");
		}
	}

	public class NoteOff : NoteTrigger
	{
		public NoteOff(NoteOn noteOn, Seq seq, TrkOptions trkOptions)
			: base(seq, new List<byte>(), trkOptions)
		{
			foreach(TrkRef tr in noteOn.Seq.TrkRefs)
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
