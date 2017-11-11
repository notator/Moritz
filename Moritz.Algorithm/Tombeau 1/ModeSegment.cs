using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Moritz.Globals;
using Moritz.Spec;

namespace Moritz.Algorithm.Tombeau1
{
	/// <summary>
	/// A (mutable) List of GamutTrk objects, each of which has the same Mode and MidiChannel.
	/// </summary>
	internal class ModeSegment
	{
		public ModeSegment(int midiChannel)
		{
			MsPositionReContainer = 0;
			MidiChannel = midiChannel;
			_gamutTrks = new List<GamutTrk>();
		}

		public ModeSegment(int midiChannel, int msPositionReContainer, IReadOnlyList<GamutTrk> gamutTrks)
		{
			Debug.Assert(midiChannel >= 0 && midiChannel <= 15);
			Debug.Assert(msPositionReContainer >= 0);
			Debug.Assert(gamutTrks.Count > 0);

			MsPositionReContainer = msPositionReContainer;
			MidiChannel = midiChannel;
			_mode = new Mode(gamutTrks[0].Gamut.Mode.AbsolutePitchHierarchy);

			foreach(GamutTrk gamutTrk in gamutTrks)
			{
				AssertConsistency(MidiChannel, _mode, gamutTrk);
			}

			_gamutTrks = new List<GamutTrk>(gamutTrks);
		}

		/// <summary>
		/// Note that gamuts are not cloned in the contained ModeSegment objects.
		/// </summary>
		/// <returns></returns>
		public ModeSegment Clone()
		{
			ModeSegment clone = new ModeSegment(this.MidiChannel);
			foreach(GamutTrk gamutTrk in _gamutTrks)
			{
				clone.Add(gamutTrk.Clone);
			}
			return clone;
		}

		public GamutTrk this[int i] {	get => _gamutTrks[i]; }

		public int Count { get => _gamutTrks.Count; }

		private void AssertConsistency(int midiChannel, Mode mode, GamutTrk gamutTrk)
		{
			Debug.Assert(mode.Equals(gamutTrk.Gamut.Mode), "All GamutTrks in a ModeSegment must have the same Mode.AbsolutePitchHierarchy");
			Debug.Assert(midiChannel == gamutTrk.MidiChannel, "All GamutTrks in a ModeSegment must have the same MidiChannel");
		}

		public void Add(GamutTrk gamutTrk)
		{
			Debug.Assert(gamutTrk != null);

			if(_gamutTrks.Count > 0) // GamutTrks can also be removed, so check.
			{
				AssertConsistency(MidiChannel, _mode, gamutTrk);
				GamutTrk lastGT = _gamutTrks[_gamutTrks.Count - 1];
				gamutTrk.MsPositionReContainer = lastGT.MsPositionReContainer + lastGT.MsDuration;
			}
			else
			{
				_mode = new Mode(gamutTrk.Gamut.Mode.AbsolutePitchHierarchy);
				gamutTrk.MsPositionReContainer = 0;
			}
			_gamutTrks.Add(gamutTrk);  			
		}

		public void Remove(GamutTrk gamutTrk)
		{
			_gamutTrks.Remove(gamutTrk);
			SetMsPositionsReThisModeSegment();
		}
		public void RemoveAt(int index)
		{
			_gamutTrks.RemoveAt(index);
			SetMsPositionsReThisModeSegment();
		}
		public void RemoveRange(int startIndex, int nItems)
		{
			_gamutTrks.RemoveRange(startIndex, nItems);
			SetMsPositionsReThisModeSegment();
		}

		/// <summary>
		/// Returns a single Trk containing all the IUniqueDefs in the GamtTrks.
		/// </summary>
		internal Trk ToTrk()
		{
			Trk trk = new Trk(this.MidiChannel, this.MsPositionReContainer, new List<IUniqueDef>());
			foreach(GamutTrk gamutTrk in this.GamutTrks)
			{
				trk.AddRange(gamutTrk);
			}

			return trk;
		}

		public void Reverse()
		{
			_gamutTrks.Reverse();
			SetMsPositionsReThisModeSegment();
		}

		public new string ToString()
		{
			int count = _gamutTrks.Count;
			if(count > 0)
			{
				return $"Count={count.ToString()} Mode={_mode.ToString()} MsPositionReContainer={MsPositionReContainer}";
			}
			else
			{
				return $"Count={count.ToString()}";
			}
		}

		public int MsPositionReContainer = 0;
		public IReadOnlyList<GamutTrk> GamutTrks { get => _gamutTrks as IReadOnlyList<GamutTrk>; }
		public int MsDuration
		{ 
			get
			{
				int rval = 0;
				foreach(GamutTrk gamutTrk in _gamutTrks)
				{
					rval += gamutTrk.MsDuration;
				}
				return rval;
			}
			internal set
			{
				Debug.Assert(value > 0);

				int msDuration = value;

				List<int> relativeDurations = new List<int>();
				foreach(GamutTrk gamutTrk in _gamutTrks)
				{
					relativeDurations.Add(gamutTrk.MsDuration);
				}

				List<int> newDurations = M.IntDivisionSizes(msDuration, relativeDurations);

				Debug.Assert(newDurations.Count == relativeDurations.Count);
				int i = 0;
				int newTotal = 0;
				foreach(GamutTrk gamutTrk in _gamutTrks)
				{
					if(gamutTrk.MsDuration > 0)
					{
						gamutTrk.MsDuration = newDurations[i];
						newTotal += gamutTrk.MsDuration;
						++i;
					}
				}

				Debug.Assert(msDuration == newTotal);

				SetMsPositionsReThisModeSegment();
			}
		}

		private void SetMsPositionsReThisModeSegment()
		{
			int msPos = 0;
			foreach(GamutTrk gamutTrk in _gamutTrks)
			{
				gamutTrk.MsPositionReContainer = msPos;
				msPos += gamutTrk.MsDuration;
			}
		}

		private readonly int MidiChannel;
		private List<GamutTrk> _gamutTrks = null;
		private Mode _mode = null;
	}


}