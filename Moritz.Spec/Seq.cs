using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Collections;

namespace Moritz.Spec
{
	public class Seq : IEnumerable
	{
		/// <param name="trkRefs"></param>
		/// <param name="trkOptions">Can be null</param>
		public Seq(List<TrkRef> trkRefs, TrkOptions trkOptions)
		{
			Debug.Assert(trkRefs != null && trkRefs.Count > 0);
			TrkOptions = trkOptions;
			TrkRefs = trkRefs;
		}

		internal void WriteSvg(Xml.SvgWriter w)
		{
			w.WriteStartElement("seq");
			if(TrkOptions != null)
			{
				TrkOptions.WriteSvg(w, false);
			}
			Debug.Assert(TrkRefs != null && TrkRefs.Count > 0);
			foreach(TrkRef trkRef in TrkRefs)
			{
				trkRef.WriteSvg(w);
			}
			w.WriteEndElement(); // trks
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
