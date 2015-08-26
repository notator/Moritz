using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Collections;

namespace Moritz.Spec
{
	public class Pressures : IEnumerable
	{
		/// <param name="pressures"></param>
		/// <param name="inputControls">Can be null</param>
		public Pressures(List<Pressure> pressures, InputControls inputControls)
		{
			Debug.Assert(pressures != null && pressures.Count > 0);
			_inputControls = inputControls;
			_pressures = pressures;
		}

		internal void WriteSvg(Xml.SvgWriter w)
		{
			w.WriteStartElement("pressures");
			if(_inputControls != null)
			{
				_inputControls.WriteSvg(w);
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

		private InputControls _inputControls;

	}
}
