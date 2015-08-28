using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Collections;

namespace Moritz.Spec
{
	public class TrkOffs : IEnumerable
	{
		/// <param name="trkOffs"></param>
		/// <param name="trkOptions">Can be null</param>
		public TrkOffs(List<TrkOff> trkOffs, TrkOptions trkOptions)
		{
			Debug.Assert(trkOffs != null && trkOffs.Count > 0);
			_trkOptions = trkOptions;
			_trkOffs = trkOffs;
		}

		internal void WriteSvg(Xml.SvgWriter w)
		{
			w.WriteStartElement("trkOffs");
			if(_trkOptions != null)
			{
				_trkOptions.WriteSvg(w);
			}
			Debug.Assert(_trkOffs != null && _trkOffs.Count > 0);
			foreach(TrkOff trkOff in _trkOffs)
			{
				trkOff.WriteSvg(w);
			}
			w.WriteEndElement(); // trkOffs
		}

		#region Enumerable

		public IEnumerator GetEnumerator()
        {
            return new TrkOffEnumerator(_trkOffs);
        }

        // private enumerator class
        // see http://support.microsoft.com/kb/322022/en-us
        private class TrkOffEnumerator : IEnumerator
        {
            public List<TrkOff> _trkOffs;
            int position = -1;
            //constructor
			public TrkOffEnumerator(List<TrkOff> trkOffs)
            {
                _trkOffs = trkOffs;
            }
            private IEnumerator getEnumerator()
            {
                return (IEnumerator)this;
            }
            //IEnumerator
            public bool MoveNext()
            {
                position++;
                return (position < _trkOffs.Count);
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
                        return _trkOffs[position];
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

		//public List<TrkOff> TrkOffs { get{ return _trkOffs;} }
		private List<TrkOff> _trkOffs = null;

		private TrkOptions _trkOptions;

	}
}
