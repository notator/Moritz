using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Moritz.Spec;

namespace Moritz.Algorithm.Tombeau1
{
	/// <summary>
	/// A (mutable) List of Grp objects, each of which has the same Mode.
	/// </summary>
	internal class ModeSegment
	{
		public ModeSegment()
		{
			_grps = new List<Grp>();
		}

		public ModeSegment(IReadOnlyList<Grp> grps)
		{
			Debug.Assert(grps.Count > 0);

			_mode = new Mode(grps[0].Gamut.Mode.AbsolutePitchHierarchy);

			foreach(Grp grp in grps)
			{
				CheckAbsPitchHierarchy(_mode, grp.Gamut.Mode);
			}

			_grps = new List<Grp>(grps);
		}

		/// <summary>
		/// Note that gamuts are not cloned in the contained ModeSegment objects.
		/// </summary>
		/// <returns></returns>
		public ModeSegment Clone()
		{
			ModeSegment clone = new ModeSegment();
			foreach(Grp grp in _grps)
			{
				clone.Add(grp.Clone);
			}
			return clone;
		}

		public Grp this[int i] {	get => _grps[i]; }

		public int Count { get => _grps.Count; }

		private void CheckAbsPitchHierarchy(Mode mode, Mode newMode )
		{
			if(mode.Equals(newMode) == false)
			{
				Debug.Assert(false, "All Grps in a ModeSegment must have the same Mode.AbsolutePitchHierarchy");
			}
		}

		public void Add(Grp grp)
		{
			Debug.Assert(grp != null);

			if(_grps.Count > 0) // Grps can also be removed, so check.
			{
				CheckAbsPitchHierarchy(_mode, grp.Gamut.Mode);
			}
			else
			{
				_mode = new Mode(grp.Gamut.Mode.AbsolutePitchHierarchy);
			}
			_grps.Add(grp);  			
		}

		public void Remove(Grp grp)
		{
			_grps.Remove(grp);
		}
		public void RemoveAt(int index)
		{
			_grps.RemoveAt(index);
		}
		public void RemoveRange(int startIndex, int nItems)
		{
			_grps.RemoveRange(startIndex, nItems);
		}
		public void Reverse()
		{
			_grps.Reverse();
		}

		public new string ToString()
		{
			int count = _grps.Count;
			if(count > 0)
			{
				return $"Count={count.ToString()} Mode={_mode.ToString()}";
			}
			else
			{
				return $"Count={count.ToString()}";
			}
		}

		public IReadOnlyList<Grp> Grps { get => _grps as IReadOnlyList<Grp>; }
		
		private List<Grp> _grps = null;
		private Mode _mode { get; set; }
	}


}