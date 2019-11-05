using Moritz.Globals;
using System;

namespace Moritz.Spec
{
	public struct PitchWeight
	{
		/// <summary>
		/// PitchWeight Struct
		/// </summary>
		/// <param name="pitch">Must be in range 0..127</param>
		/// <param name="weight">Must be in range 1..127</param>
		public PitchWeight(int pitch, int weight)
		{
			M.AssertRange0_127(pitch);
			M.AssertRange0_127(weight);

			Pitch = pitch;
			Weight = weight;
		}

		public int Pitch { get; }
		public int Weight { get; }
	}
}
