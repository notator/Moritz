using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Collections;

namespace Moritz.Spec
{
	public class Pressures : IEnumerable
	{
		/// <param name="pressures"></param>
		/// <param name="trkOptions">Can be null</param>
		public Pressures(List<Pressure> pressures, TrkOptions trkOptions)
		{
			Debug.Assert(pressures != null && pressures.Count > 0);
			_trkOptions = trkOptions;
			_pressures = pressures;
		}

		internal void WriteSvg(Xml.SvgWriter w)
		{
			w.WriteStartElement("pressures");
			if(_trkOptions != null)
			{
				_trkOptions.WriteSvg(w);
			}
			Debug.Assert(_pressures != null && _pressures.Count > 0);
			foreach(Pressure pressure in _pressures)
			{
				pressure.WriteSvg(w);
			}
			w.WriteEndElement(); // pressures
		}

		#region Enumerable

		public IEnumerator GetEnumerator()
        {
            return new PressureEnumerator(_pressures);
        }

        // private enumerator class
        // see http://support.microsoft.com/kb/322022/en-us
        private class PressureEnumerator : IEnumerator
        {
            public List<Pressure> _pressures;
            int position = -1;
            //constructor
			public PressureEnumerator(List<Pressure> pressures)
            {
                _pressures = pressures;
            }
            private IEnumerator getEnumerator()
            {
                return (IEnumerator)this;
            }
            //IEnumerator
            public bool MoveNext()
            {
                position++;
                return (position < _pressures.Count);
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
                        return _pressures[position];
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

		//public List<Pressure> Pressures { get{ return _pressures;} }
		private List<Pressure> _pressures = null;

		private TrkOptions _trkOptions;

	}
}
