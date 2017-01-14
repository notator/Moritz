using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Collections;

namespace Moritz.Spec
{
	public class SeqRef : IEnumerable
	{
		/// <param name="trkRefs"></param>
		/// <param name="trkOptions">Can be null</param>
		public SeqRef(List<TrkRef> trkRefs, TrkOptions trkOptions)
		{
			Debug.Assert(trkRefs != null && trkRefs.Count > 0);
			TrkOptions = trkOptions;
			TrkRefs = trkRefs;
		}

		/// <summary>
		/// This function only writes TrkRefs that refer to Trks stored in external VoiceDefs. 
		/// Note that successive Trks in the same VoiceDef may, in principle, contain common IUniqueDefs...
		/// SVG files contain voice definitions that contain MidiChordDefs and restDefs, but no Trks.
		/// </summary>
		/// <param name="w"></param>
		internal void WriteSVG(Xml.SvgWriter w)
		{
			w.WriteStartElement("seq");
			if(TrkOptions != null)
			{
				TrkOptions.WriteSVG(w, false);
			}
			Debug.Assert(TrkRefs != null && TrkRefs.Count > 0);
			foreach(TrkRef trkRef in TrkRefs)
			{
				trkRef.WriteSVG(w);
			}
			w.WriteEndElement(); // seq
		}

		#region Enumerable

		public IEnumerator GetEnumerator()
		{
			return new TrkOnEnumerator(TrkRefs);
		}

		// private enumerator class
		// see http://support.microsoft.com/kb/322022/en-us
		private class TrkOnEnumerator : IEnumerator
		{
			public List<TrkRef> _trkRefs;
			int position = -1;
			//constructor
			public TrkOnEnumerator(List<TrkRef> trkRefs)
			{
				_trkRefs = trkRefs;
			}
			private IEnumerator getEnumerator()
			{
				return (IEnumerator)this;
			}
			//IEnumerator
			public bool MoveNext()
			{
				position++;
				return (position < _trkRefs.Count);
			}
			//IEnumerator
			public void Reset()
			{ position = -1; }
			//IEnumerator
			public object Current
			{
				get
				{
					try
					{
						return _trkRefs[position];
					}
					catch(IndexOutOfRangeException)
					{
						Debug.Assert(false);
						return null;
					}
				}
			}
		}  //end nested class
		#endregion

		public List<TrkRef> TrkRefs = null;
		public TrkOptions TrkOptions = null;
	}
}
