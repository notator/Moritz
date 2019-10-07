using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System;

namespace Moritz.Spec
{
	/// <summary>
	/// A helper class storing a Mode and its proximity to another Mode.
	/// </summary>
	public class _oldModeProximity
	{
		public _oldModeProximity(_oldMode mode, int proximity)
		{
			_mode = mode;
			_proximity = proximity;
		}

		public override string ToString()
		{
			return $"Mode: {_mode.ToString()}, proximity={_proximity}";
		}

		public _oldMode Mode { get => _mode; }
		private _oldMode _mode;

		public int Proximity { get => _proximity; }
		private readonly int _proximity = 0;
	}
}