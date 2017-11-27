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
	/// A (mutable) List of ModeGrpTrk objects, each of which has the same Mode and MidiChannel.
	/// </summary>
	internal class ModeSegment
	{
		public ModeSegment(int midiChannel)
		{
			MsPositionReContainer = 0;
			MidiChannel = midiChannel;
			_ModeGrpTrks = new List<ModeGrpTrk>();
		}

		public ModeSegment(int midiChannel, int msPositionReContainer, IReadOnlyList<ModeGrpTrk> ModeGrpTrks)
		{
			Debug.Assert(midiChannel >= 0 && midiChannel <= 15);
			Debug.Assert(msPositionReContainer >= 0);
			Debug.Assert(ModeGrpTrks.Count > 0);

			MsPositionReContainer = msPositionReContainer;
			MidiChannel = midiChannel;
			_mode = ModeGrpTrks[0].Mode;

			foreach(ModeGrpTrk ModeGrpTrk in ModeGrpTrks)
			{
				AssertConsistency(MidiChannel, _mode, ModeGrpTrk);
			}

			_ModeGrpTrks = new List<ModeGrpTrk>(ModeGrpTrks);
		}

		/// <summary>
		/// Note that modes are not cloned in the contained ModeSegment objects.
		/// </summary>
		/// <returns></returns>
		public ModeSegment Clone()
		{
			ModeSegment clone = new ModeSegment(this.MidiChannel);
			foreach(ModeGrpTrk ModeGrpTrk in _ModeGrpTrks)
			{
				clone.Add(ModeGrpTrk.Clone);
			}
			return clone;
		}

		public ModeGrpTrk this[int i] {	get => _ModeGrpTrks[i]; }

		public int Count { get => _ModeGrpTrks.Count; }

		private void AssertConsistency(int midiChannel, Mode mode, ModeGrpTrk ModeGrpTrk)
		{
			Debug.Assert(mode.HasSameAbsolutePitchHierarchy(ModeGrpTrk.Mode), "All ModeGrpTrks in a ModeSegment must have the same Mode.AbsolutePitchHierarchy");
			Debug.Assert(midiChannel == ModeGrpTrk.MidiChannel, "All ModeGrpTrks in a ModeSegment must have the same MidiChannel");
		}

		public void Add(ModeGrpTrk ModeGrpTrk)
		{
			Debug.Assert(ModeGrpTrk != null);

			if(_ModeGrpTrks.Count > 0) // ModeGrpTrks can also be removed, so check.
			{
				AssertConsistency(MidiChannel, _mode, ModeGrpTrk);
				ModeGrpTrk lastGGT = _ModeGrpTrks[_ModeGrpTrks.Count - 1];
				ModeGrpTrk.MsPositionReContainer = lastGGT.MsPositionReContainer + lastGGT.MsDuration;
			}
			else
			{
				_mode = ModeGrpTrk.Mode;
				ModeGrpTrk.MsPositionReContainer = 0;
			}
			_ModeGrpTrks.Add(ModeGrpTrk);  			
		}

		public void Remove(ModeGrpTrk ModeGrpTrk)
		{
			_ModeGrpTrks.Remove(ModeGrpTrk);
			SetMsPositionsReThisModeSegment();
		}
		public void RemoveAt(int index)
		{
			_ModeGrpTrks.RemoveAt(index);
			SetMsPositionsReThisModeSegment();
		}
		public void RemoveRange(int startIndex, int nItems)
		{
			_ModeGrpTrks.RemoveRange(startIndex, nItems);
			SetMsPositionsReThisModeSegment();
		}

		/// <summary>
		/// Returns a single Trk containing all the IUniqueDefs in the GamtTrks.
		/// </summary>
		internal Trk ToTrk()
		{
			Trk trk = new Trk(this.MidiChannel, this.MsPositionReContainer, new List<IUniqueDef>());
			foreach(ModeGrpTrk ModeGrpTrk in this.ModeGrpTrks)
			{
				trk.AddRange(ModeGrpTrk);
			}

			return trk;
		}

		public void Reverse()
		{
			_ModeGrpTrks.Reverse();
			SetMsPositionsReThisModeSegment();
		}

		public new string ToString()
		{
			int count = _ModeGrpTrks.Count;
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
		public IReadOnlyList<ModeGrpTrk> ModeGrpTrks { get => _ModeGrpTrks as IReadOnlyList<ModeGrpTrk>; }
		public int MsDuration
		{ 
			get
			{
				int rval = 0;
				foreach(ModeGrpTrk ModeGrpTrk in _ModeGrpTrks)
				{
					rval += ModeGrpTrk.MsDuration;
				}
				return rval;
			}
			internal set
			{
				Debug.Assert(value > 0);

				int msDuration = value;

				List<int> relativeDurations = new List<int>();
				foreach(ModeGrpTrk ModeGrpTrk in _ModeGrpTrks)
				{
					relativeDurations.Add(ModeGrpTrk.MsDuration);
				}

				List<int> newDurations = M.IntDivisionSizes(msDuration, relativeDurations);

				Debug.Assert(newDurations.Count == relativeDurations.Count);
				int i = 0;
				int newTotal = 0;
				foreach(ModeGrpTrk ModeGrpTrk in _ModeGrpTrks)
				{
					if(ModeGrpTrk.MsDuration > 0)
					{
						ModeGrpTrk.MsDuration = newDurations[i];
						newTotal += ModeGrpTrk.MsDuration;
						++i;
					}
				}

				Debug.Assert(msDuration == newTotal);

				SetMsPositionsReThisModeSegment();
			}
		}

		internal void SetMsPositionsReThisModeSegment()
		{
			int msPos = 0;
			foreach(ModeGrpTrk ModeGrpTrk in _ModeGrpTrks)
			{
				ModeGrpTrk.MsPositionReContainer = msPos;
				msPos += ModeGrpTrk.MsDuration;
			}
		}

		private readonly int MidiChannel;
		private List<ModeGrpTrk> _ModeGrpTrks = null;
		private Mode _mode = null;
	}


}