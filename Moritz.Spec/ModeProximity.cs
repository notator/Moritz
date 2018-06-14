using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System;

namespace Moritz.Spec
{
	/// <summary>
	/// A helper class storing a Mode and its proximity to another Mode.
	/// </summary>
	public class ModeProximity
	{
		public ModeProximity(Mode mode, int proximity)
		{
			_mode = mode;
			_proximity = proximity;
		}

		public override string ToString()
		{
			return $"Mode: {_mode.ToString()}, proximity={_proximity}";
		}

		public Mode Mode { get => _mode; }
		private Mode _mode;

		public int Proximity { get => _proximity; }
		private readonly int _proximity = 0;
	}
}