using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Moritz.Spec;

namespace Moritz.Algorithm.Tombeau1
{
	/// <summary>
	/// A (mutable) List of GamutTrk objects, each of which has the same Mode.
	/// </summary>
	internal class ModeSegment
	{
		public ModeSegment()
		{
			_gamutTrks = new List<GamutTrk>();
		}

		public ModeSegment(IReadOnlyList<GamutTrk> gamutTrks)
		{
			Debug.Assert(gamutTrks.Count > 0);

			_mode = new Mode(gamutTrks[0].Gamut.Mode.AbsolutePitchHierarchy);

			foreach(GamutTrk gamutTrk in gamutTrks)
			{
				CheckAbsPitchHierarchy(_mode, gamutTrk.Gamut.Mode);
			}

			_gamutTrks = new List<GamutTrk>(gamutTrks);
		}

		/// <summary>
		/// Note that gamuts are not cloned in the contained ModeSegment objects.
		/// </summary>
		/// <returns></returns>
		public ModeSegment Clone()
		{
			ModeSegment clone = new ModeSegment();
			foreach(GamutTrk gamutTrk in _gamutTrks)
			{
				clone.Add(gamutTrk.Clone);
			}
			return clone;
		}

		public GamutTrk this[int i] {	get => _gamutTrks[i]; }

		public int Count { get => _gamutTrks.Count; }

		private void CheckAbsPitchHierarchy(Mode mode, Mode newMode )
		{
			if(mode.Equals(newMode) == false)
			{
				Debug.Assert(false, "All GamutTrks in a ModeSegment must have the same Mode.AbsolutePitchHierarchy");
			}
		}

		public void Add(GamutTrk gamutTrk)
		{
			Debug.Assert(gamutTrk != null);

			if(_gamutTrks.Count > 0) // GamutTrks can also be removed, so check.
			{
				CheckAbsPitchHierarchy(_mode, gamutTrk.Gamut.Mode);
			}
			else
			{
				_mode = new Mode(gamutTrk.Gamut.Mode.AbsolutePitchHierarchy);
			}
			_gamutTrks.Add(gamutTrk);  			
		}

		public void Remove(GamutTrk gamutTrk)
		{
			_gamutTrks.Remove(gamutTrk);
		}
		public void RemoveAt(int index)
		{
			_gamutTrks.RemoveAt(index);
		}
		public void RemoveRange(int startIndex, int nItems)
		{
			_gamutTrks.RemoveRange(startIndex, nItems);
		}
		public void Reverse()
		{
			_gamutTrks.Reverse();
		}

		public new string ToString()
		{
			int count = _gamutTrks.Count;
			if(count > 0)
			{
				return $"Count={count.ToString()} Mode={_mode.ToString()}";
			}
			else
			{
				return $"Count={count.ToString()}";
			}
		}

		public IReadOnlyList<GamutTrk> GamutTrks { get => _gamutTrks as IReadOnlyList<GamutTrk>; }
		
		private List<GamutTrk> _gamutTrks = null;
		private Mode _mode { get; set; }
	}


}