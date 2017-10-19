using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System;

namespace Moritz.Spec
{
	public class GamutProximity
	{
		public GamutProximity(Gamut gamut, int proximity)
		{
			_gamut = gamut;
			_proximity = proximity;
		}

		public override string ToString()
		{
			return $"Gamut: {_gamut.ToString()} proximity={_proximity}";
		}

		public Gamut Gamut { get => _gamut; }
		private Gamut _gamut;

		public int Proximity { get => _proximity; }
		private int _proximity = 0;
	}
}