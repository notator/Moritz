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
		protected NoteTrigger(Seq seq, List<TrkRef> trkOffs, TrkOptions trkOptions)
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
				_trkOptions.WriteSvg(w);
			}

			if(_seq != null)
			{
				_seq.WriteSvg(w);
			}

			if(_trkOffs != null)
			{
				w.WriteStartElement("trkOffs");
				foreach(TrkRef trkRef in _trkOffs)
				{
					trkRef.WriteSvg(w, false);
				}
				w.WriteEndElement(); // trkOffs
			}

			w.WriteEndElement(); // noteOn or noteOff
		}

		private Seq _seq = null;
		private List<TrkRef> _trkOffs = null;
		private TrkOptions _trkOptions = null;

		public Seq Seq { get { return _seq; } }
		public List<TrkRef> TrkOffs { get { return _trkOffs; } }
		public TrkOptions TrkOptions { get { return _trkOptions; } set { _trkOptions = value; } } 
	}

	public class NoteOn : NoteTrigger
	{
		public NoteOn(Seq seq, List<TrkRef> trkOffs, TrkOptions trkOptions)
			: base(seq, trkOffs, trkOptions)
		{
		}

		// a NoteOn that uses no continuous controllers, turns no trks off, and has no trkOptions.
		public NoteOn(Seq seq)
			: base(seq, null, null)
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
			: base(seq, noteOn.Seq.TrkRefs, trkOptions)
		{

		}

		/// <summary>
		/// A NoteOff that does nothing except turn off the trks that were turned on by the noteOn argument.
		/// </summary>
		/// <param name="noteOn"></param>
		public NoteOff(NoteOn noteOn)
			: base(null, noteOn.Seq.TrkRefs, null)
		{

		}

		public void WriteSvg(SvgWriter w)
		{
			WriteSvg(w, "noteOff");
		}
	}
}
