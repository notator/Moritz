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
	/// A (mutable) List of GamutGrpTrk objects, each of which has the same Mode and MidiChannel.
	/// </summary>
	internal class ModeSegment
	{
		public ModeSegment(int midiChannel)
		{
			MsPositionReContainer = 0;
			MidiChannel = midiChannel;
			_gamutGrpTrks = new List<GamutGrpTrk>();
		}

		public ModeSegment(int midiChannel, int msPositionReContainer, IReadOnlyList<GamutGrpTrk> gamutGrpTrks)
		{
			Debug.Assert(midiChannel >= 0 && midiChannel <= 15);
			Debug.Assert(msPositionReContainer >= 0);
			Debug.Assert(gamutGrpTrks.Count > 0);

			MsPositionReContainer = msPositionReContainer;
			MidiChannel = midiChannel;
			_modeOld = new ModeOld(gamutGrpTrks[0].Gamut.ModeOld.AbsolutePitchHierarchy);

			foreach(GamutGrpTrk gamutGrpTrk in gamutGrpTrks)
			{
				AssertConsistency(MidiChannel, _modeOld, gamutGrpTrk);
			}

			_gamutGrpTrks = new List<GamutGrpTrk>(gamutGrpTrks);
		}

		/// <summary>
		/// Note that gamuts are not cloned in the contained ModeSegment objects.
		/// </summary>
		/// <returns></returns>
		public ModeSegment Clone()
		{
			ModeSegment clone = new ModeSegment(this.MidiChannel);
			foreach(GamutGrpTrk gamutGrpTrk in _gamutGrpTrks)
			{
				clone.Add(gamutGrpTrk.Clone);
			}
			return clone;
		}

		public GamutGrpTrk this[int i] {	get => _gamutGrpTrks[i]; }

		public int Count { get => _gamutGrpTrks.Count; }

		private void AssertConsistency(int midiChannel, ModeOld modeOld, GamutGrpTrk gamutGrpTrk)
		{
			Debug.Assert(modeOld.Equals(gamutGrpTrk.Gamut.ModeOld), "All GamutGrpTrks in a ModeSegment must have the same Mode.AbsolutePitchHierarchy");
			Debug.Assert(midiChannel == gamutGrpTrk.MidiChannel, "All GamutGrpTrks in a ModeSegment must have the same MidiChannel");
		}

		public void Add(GamutGrpTrk gamutGrpTrk)
		{
			Debug.Assert(gamutGrpTrk != null);

			if(_gamutGrpTrks.Count > 0) // GamutGrpTrks can also be removed, so check.
			{
				AssertConsistency(MidiChannel, _modeOld, gamutGrpTrk);
				GamutGrpTrk lastGGT = _gamutGrpTrks[_gamutGrpTrks.Count - 1];
				gamutGrpTrk.MsPositionReContainer = lastGGT.MsPositionReContainer + lastGGT.MsDuration;
			}
			else
			{
				_modeOld = new ModeOld(gamutGrpTrk.Gamut.ModeOld.AbsolutePitchHierarchy);
				gamutGrpTrk.MsPositionReContainer = 0;
			}
			_gamutGrpTrks.Add(gamutGrpTrk);  			
		}

		public void Remove(GamutGrpTrk gamutGrpTrk)
		{
			_gamutGrpTrks.Remove(gamutGrpTrk);
			SetMsPositionsReThisModeSegment();
		}
		public void RemoveAt(int index)
		{
			_gamutGrpTrks.RemoveAt(index);
			SetMsPositionsReThisModeSegment();
		}
		public void RemoveRange(int startIndex, int nItems)
		{
			_gamutGrpTrks.RemoveRange(startIndex, nItems);
			SetMsPositionsReThisModeSegment();
		}

		/// <summary>
		/// Returns a single Trk containing all the IUniqueDefs in the GamtTrks.
		/// </summary>
		internal Trk ToTrk()
		{
			Trk trk = new Trk(this.MidiChannel, this.MsPositionReContainer, new List<IUniqueDef>());
			foreach(GamutGrpTrk gamutGrpTrk in this.GamutGrpTrks)
			{
				trk.AddRange(gamutGrpTrk);
			}

			return trk;
		}

		public void Reverse()
		{
			_gamutGrpTrks.Reverse();
			SetMsPositionsReThisModeSegment();
		}

		public new string ToString()
		{
			int count = _gamutGrpTrks.Count;
			if(count > 0)
			{
				return $"Count={count.ToString()} Mode={_modeOld.ToString()} MsPositionReContainer={MsPositionReContainer}";
			}
			else
			{
				return $"Count={count.ToString()}";
			}
		}

		public int MsPositionReContainer = 0;
		public IReadOnlyList<GamutGrpTrk> GamutGrpTrks { get => _gamutGrpTrks as IReadOnlyList<GamutGrpTrk>; }
		public int MsDuration
		{ 
			get
			{
				int rval = 0;
				foreach(GamutGrpTrk gamutGrpTrk in _gamutGrpTrks)
				{
					rval += gamutGrpTrk.MsDuration;
				}
				return rval;
			}
			internal set
			{
				Debug.Assert(value > 0);

				int msDuration = value;

				List<int> relativeDurations = new List<int>();
				foreach(GamutGrpTrk gamutGrpTrk in _gamutGrpTrks)
				{
					relativeDurations.Add(gamutGrpTrk.MsDuration);
				}

				List<int> newDurations = M.IntDivisionSizes(msDuration, relativeDurations);

				Debug.Assert(newDurations.Count == relativeDurations.Count);
				int i = 0;
				int newTotal = 0;
				foreach(GamutGrpTrk gamutGrpTrk in _gamutGrpTrks)
				{
					if(gamutGrpTrk.MsDuration > 0)
					{
						gamutGrpTrk.MsDuration = newDurations[i];
						newTotal += gamutGrpTrk.MsDuration;
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
			foreach(GamutGrpTrk gamutGrpTrk in _gamutGrpTrks)
			{
				gamutGrpTrk.MsPositionReContainer = msPos;
				msPos += gamutGrpTrk.MsDuration;
			}
		}

		private readonly int MidiChannel;
		private List<GamutGrpTrk> _gamutGrpTrks = null;
		private ModeOld _modeOld = null;
	}


}