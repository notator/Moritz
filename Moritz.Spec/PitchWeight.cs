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
			if(pitch < 0 || pitch > 127)
			{
				throw new ApplicationException("Illegal pitch.");
			}
			if(weight < 1 || pitch > 127)
			{
				throw new ApplicationException("Illegal weight.");
			}
			Pitch = (UInt7)pitch;
			Weight = (UInt7)weight;
		}

		public PitchWeight(UInt7 pitch, UInt7 weight)
		{
			Pitch = pitch;
			Weight = weight;
		}

		public UInt7 Pitch { get; }
		public UInt7 Weight { get; }
	}
}
