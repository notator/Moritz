using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Collections;

namespace Moritz.Spec
{
	public class TrkOns : IEnumerable
	{
		/// <param name="trkOns"></param>
		/// <param name="trkOptions">Can be null</param>
		public TrkOns(List<TrkOn> trkOns, TrkOptions trkOptions)
		{
			Debug.Assert(trkOns != null && trkOns.Count > 0);
			_trkOptions = trkOptions;
			_trkOns = trkOns;
		}

		internal void WriteSvg(Xml.SvgWriter w)
		{
			w.WriteStartElement("trkOns");
			if(_trkOptions != null)
			{
				_trkOptions.WriteSvg(w);
			}
			Debug.Assert(_trkOns != null && _trkOns.Count > 0);
			foreach(TrkOn trkOn in _trkOns)
			{
				trkOn.WriteSvg(w);
			}
			w.WriteEndElement(); // trkOns
		}

		#region Enumerable

		public IEnumerator GetEnumerator()
		{
			return new TrkOnEnumerator(_trkOns);
		}

		// private enumerator class
		// see http://support.microsoft.com/kb/322022/en-us
		private class TrkOnEnumerator : IEnumerator
		{
			public List<TrkOn> _trkOns;
			int position = -1;
			//constructor
			public TrkOnEnumerator(List<TrkOn> trkOns)
			{
				_trkOns = trkOns;
			}
			private IEnumerator getEnumerator()
			{
				return (IEnumerator)this;
			}
			//IEnumerator
			public bool MoveNext()
			{
				position++;
				return (position < _trkOns.Count);
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
						return _trkOns[position];
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
		private List<TrkOn> _trkOns = null;
		private TrkOptions _trkOptions;

	}
}
