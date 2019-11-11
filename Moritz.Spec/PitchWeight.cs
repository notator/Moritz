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

		/// <summary>
		/// See https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/statements-expressions-operators/how-to-define-value-equality-for-a-type
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public override bool Equals(object obj)
		{
			if(obj is PitchWeight)
			{
				return this.Equals((PitchWeight)obj);
			}
			return false;
		}

		public bool Equals(PitchWeight pitchWeight)
		{
			return (Pitch == pitchWeight.Pitch) && (Weight == pitchWeight.Weight);
		}

		public override int GetHashCode()
		{
			return Pitch ^ Weight;
		}

		public static bool operator ==(PitchWeight lhs, PitchWeight rhs)
		{
			return lhs.Equals(rhs);
		}

		public static bool operator !=(PitchWeight lhs, PitchWeight rhs)
		{
			return !(lhs.Equals(rhs));
		}

		public int Pitch { get; }
		public int Weight { get; }
	}
}
