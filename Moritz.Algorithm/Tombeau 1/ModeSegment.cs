using System.Collections.Generic;
using System.Diagnostics;
using Krystals4ObjectLibrary;
using Moritz.Globals;
using Moritz.Spec;

namespace Moritz.Algorithm.Tombeau1
{
	/// <summary>
	/// A (mutable) List of ModeGrpTrk objects, each of which has the same Mode.AbsolutePitchHierarchy and MidiChannel.
	/// </summary>
	internal class ModeSegment
	{
		//public ModeSegment(int midiChannel)
		//{
		//	MsPositionReContainer = 0;
		//	MidiChannel = midiChannel;
		//	_modeGrpTrks = new List<ModeGrpTrk>();
		//}

		public ModeSegment(int midiChannel, int msPositionReContainer, IReadOnlyList<ModeGrpTrk> modeGrpTrks)
		{
			Debug.Assert(midiChannel >= 0 && midiChannel <= 15);
			Debug.Assert(msPositionReContainer >= 0);
			Debug.Assert(modeGrpTrks.Count > 0);

			MsPositionReContainer = msPositionReContainer;
			MidiChannel = midiChannel;
			_mode = modeGrpTrks[0].Mode;

			foreach(ModeGrpTrk ModeGrpTrk in modeGrpTrks)
			{
				AssertConsistency(MidiChannel, _mode, ModeGrpTrk);
			}

			_modeGrpTrks = new List<ModeGrpTrk>(modeGrpTrks);
		}

		internal ModeSegment Clone()
		{
			var clonedModeGrpTrks = new List<ModeGrpTrk>();
			foreach(var modeGrpTrk in _modeGrpTrks)
			{
				ModeGrpTrk mgtClone = modeGrpTrk.Clone();
				clonedModeGrpTrks.Add(mgtClone);
			}
			var modeSegment = new ModeSegment(MidiChannel, MsPositionReContainer, clonedModeGrpTrks);

			return modeSegment;
		}

		private void AssertConsistency(int midiChannel, Mode mode, ModeGrpTrk ModeGrpTrk)
		{
			Debug.Assert(mode.HasSameAbsolutePitchHierarchy(ModeGrpTrk.Mode), "All ModeGrpTrks in a ModeSegment must have the same Mode.AbsolutePitchHierarchy");
			Debug.Assert(midiChannel == ModeGrpTrk.MidiChannel, "All ModeGrpTrks in a ModeSegment must have the same MidiChannel");
		}

		//public void Add(ModeGrpTrk ModeGrpTrk)
		//{
		//	Debug.Assert(ModeGrpTrk != null);

		//	if(_modeGrpTrks.Count > 0) // ModeGrpTrks can also be removed, so check.
		//	{
		//		AssertConsistency(MidiChannel, _mode, ModeGrpTrk);
		//		ModeGrpTrk lastMGT = _modeGrpTrks[_modeGrpTrks.Count - 1];
		//		ModeGrpTrk.MsPositionReContainer = lastMGT.MsPositionReContainer + lastMGT.MsDuration;
		//	}
		//	else
		//	{
		//		_mode = ModeGrpTrk.Mode;
		//		ModeGrpTrk.MsPositionReContainer = 0;
		//	}
		//	_modeGrpTrks.Add(ModeGrpTrk);  			
		//}

		//public void Remove(ModeGrpTrk ModeGrpTrk)
		//{
		//	_ModeGrpTrks.Remove(ModeGrpTrk);
		//	SetMsPositionsReThisModeSegment();
		//}
		//public void RemoveAt(int index)
		//{
		//	_ModeGrpTrks.RemoveAt(index);
		//	SetMsPositionsReThisModeSegment();
		//}
		//public void RemoveRange(int startIndex, int nItems)
		//{
		//	_ModeGrpTrks.RemoveRange(startIndex, nItems);
		//	SetMsPositionsReThisModeSegment();
		//}

		/// <summary>
		/// The TimeWarp is at the level of the IUniqueDefs (treated as a single sequence).
		/// See Trk.TimeWarp(...).
		/// </summary>
		/// <param name="timeWarpPerIUDEnvelope"></param>
		/// <param name="distortion"></param>
		internal void TimeWarpIUDs(Envelope timeWarpPerIUDEnvelope, double distortion)
		{
			Trk tempAllIUDsTrk = this.ToTrk();
			int trkDuration = tempAllIUDsTrk.MsDuration;

			Envelope timeWarpEnvClone = (Envelope)timeWarpPerIUDEnvelope.Clone();

			timeWarpEnvClone.SetCount(tempAllIUDsTrk.Count);
			tempAllIUDsTrk.TimeWarp(timeWarpEnvClone, distortion);

			Debug.Assert(trkDuration == tempAllIUDsTrk.MsDuration);

			ResetRelativeMsPositions();
		}

		/// <summary>
		/// The TimeWarp is at the level of the ModeGrpTrks (each has its own warp factor).
		/// </summary>
		/// <param name="envelope"></param>
		/// <param name="distortion"></param>
		internal void TimeWarpModeGrpTrks(Envelope envelope, double distortion)
		{
			envelope.SetCount(_modeGrpTrks.Count);  // nModeGrpTrks
			double factor = distortion / envelope.Domain;
			int originalMsDuration = this.MsDuration;

			for(int i = 0; i < _modeGrpTrks.Count; i++)
			{
				ModeGrpTrk mgt = _modeGrpTrks[i];
				int msDuration = ((int)(mgt.MsDuration * (envelope.Original[i] * factor)));
				mgt.MsDuration = msDuration;
			}

			this.MsDuration = originalMsDuration;

			ResetRelativeMsPositions();
		}

		private void ResetRelativeMsPositions()
		{
			SetMsPositionsReThisModeSegment();
			foreach(ModeGrpTrk mgt in _modeGrpTrks)
			{
				mgt.SetMsPositionsReFirstUD();
				mgt.AssertConsistency();
			}
		}

		private List<IUniqueDef> AllIUDs
		{
			get
			{
				List<IUniqueDef> allIUDs = new List<IUniqueDef>();
				foreach(ModeGrpTrk mgt in _modeGrpTrks)
				{
					allIUDs.AddRange(mgt.UniqueDefs);
				}
				return allIUDs;
			}
		}

		public int ShortestIUDMsDuration
		{
			get
			{
				List<IUniqueDef> allIUDs = AllIUDs;
				int shortestMsDuration = int.MaxValue;
				foreach(IUniqueDef iud in allIUDs)
				{
					shortestMsDuration = (shortestMsDuration < iud.MsDuration) ? shortestMsDuration : iud.MsDuration; 
				}
				return shortestMsDuration;
			}
		}

		/// <summary>
		/// Returns a single Trk containing all the IUniqueDefs in the _modeGrpTrks.
		/// </summary>
		internal Trk ToTrk()
		{
			Trk trk = new Trk(this.MidiChannel, this.MsPositionReContainer, new List<IUniqueDef>());
			foreach(ModeGrpTrk mgt in _modeGrpTrks)
			{
				trk.AddRange(mgt);
			}

			return trk;
		}

		public void Reverse()
		{
			_modeGrpTrks.Reverse();
			SetMsPositionsReThisModeSegment();
		}

		public int IUDCount
		{
			get
			{
				int iudCount = 0;
				foreach(ModeGrpTrk mgt in this._modeGrpTrks)
				{
					iudCount += mgt.Count;
				}
				return iudCount;
			}
		}
		internal void SetMsPositionsReThisModeSegment()
		{
			int msPos = 0;
			foreach(ModeGrpTrk mgt in _modeGrpTrks)
			{
				mgt.MsPositionReContainer = msPos;
				msPos += mgt.MsDuration;
			}
		}

		public new string ToString()
		{
			int count = _modeGrpTrks.Count;
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
		public IReadOnlyList<ModeGrpTrk> ModeGrpTrks { get => _modeGrpTrks as IReadOnlyList<ModeGrpTrk>; }
		private List<ModeGrpTrk> _modeGrpTrks = null;
		public int MsDuration
		{ 
			get
			{
				int rval = 0;
				foreach(ModeGrpTrk mgt in _modeGrpTrks)
				{
					rval += mgt.MsDuration;
				}
				return rval;
			}
			internal set
			{
				Debug.Assert(value > 0);

				int msDuration = value;

				List<int> relativeDurations = new List<int>();
				foreach(ModeGrpTrk mgt in _modeGrpTrks)
				{
					relativeDurations.Add(mgt.MsDuration);
				}

				List<int> newDurations = M.IntDivisionSizes(msDuration, relativeDurations);

				Debug.Assert(newDurations.Count == relativeDurations.Count);
				int i = 0;
				int newTotal = 0;
				foreach(ModeGrpTrk mgt in _modeGrpTrks)
				{
					if(mgt.MsDuration > 0)
					{
						mgt.MsDuration = newDurations[i];
						newTotal += mgt.MsDuration;
						++i;
					}
				}

				Debug.Assert(msDuration == newTotal);

				SetMsPositionsReThisModeSegment();
			}
		}

		private readonly int MidiChannel;		
		private Mode _mode = null;
	}
}