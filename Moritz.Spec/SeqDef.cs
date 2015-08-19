using System.Collections.Generic;
using System.Diagnostics;

namespace Moritz.Spec
{
	public class SeqDef
	{
		public SeqDef(List<TrkRef> trkRefs)
		{
			Debug.Assert(trkRefs != null && trkRefs.Count > 0);
			_trkRefs = trkRefs;
		}

		internal void WriteSvg(Xml.SvgWriter w)
		{
			w.WriteStartElement("seq");
			foreach(TrkRef trkRef in _trkRefs)
			{
				trkRef.WriteSvg(w);
			}
			w.WriteEndElement(); // seq
		}

		public List<TrkRef> TrkRefs { get{ return _trkRefs;} }
		private List<TrkRef> _trkRefs = new List<TrkRef>();

	}
}
