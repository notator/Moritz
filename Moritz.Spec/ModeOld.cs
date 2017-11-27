using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Moritz.Spec
{
	/// <summary>
	/// A ModeOld is an immutable class, containing a list of absolute pitches (C=0, C#=1, D=2 etc.)
	/// in order of importance, whereby not all absolute pitches need to be included.
	/// ModeOlds can used, for example, to determine the loudness of particular pitches.
	/// </summary>
	public class ModeOld
	{
		/// <param name="absolutePitchHierarchy">Count in range [1..12], items in range [0..11]</param>
		public ModeOld(IReadOnlyList<int> absolutePitchHierarchy)
		{
			#region plausibility checks
			int count = absolutePitchHierarchy.Count;
			Debug.Assert(count > 0 && count < 13);
			foreach(int i in absolutePitchHierarchy)
			{
				if(i < 0 || i > 11)
				{
					Debug.Assert(false, "Absolute pitch out of range");
				}
			}
			#endregion

			_absolutePitchHierarchy = new List<int>(absolutePitchHierarchy);
		}

		/// <summary>
		/// ModeOlds are equal if their absolutePitchHierarchies are identical.
		/// </summary>
		public bool Equals(ModeOld otherModeOld)
		{
			bool equals = true;
			IReadOnlyList<int> absH = this.AbsolutePitchHierarchy;
			IReadOnlyList<int> otherAbsH = otherModeOld.AbsolutePitchHierarchy;
			int count = absH.Count;

			if(count != otherAbsH.Count)
			{
				equals = false;
			}
			for(int i = 0; i < count; i++)
			{
				if(absH[i] != otherAbsH[i])
				{
					equals = false;
					break;
				}
			}
			return equals;
		}
		public override string ToString()
		{
			const string nums = "0123456789AB";
			StringBuilder sb = new StringBuilder();
			foreach(int i in _absolutePitchHierarchy)
			{
				sb.Append(nums[i]);
			}
			return sb.ToString();
		}

		public IReadOnlyList<int> AbsolutePitchHierarchy { get => _absolutePitchHierarchy as IReadOnlyList<int>; }
		private List<int> _absolutePitchHierarchy;
	}
}
