using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Moritz.Spec;

namespace Moritz.Algorithm.Tombeau1
{
     internal abstract class Tombeau1Voice : Trk
    {
		public Tombeau1Voice(int midiChannel)
			: base(midiChannel)
		{
		}

		public IReadOnlyList<IReadOnlyList<MsValues>> GetMsValuesOfModeTrks()
		{
			Debug.Assert(_modeSegments != null && _modeSegments.Count > 0);

			var msValuesListList = new List<List<MsValues>>();
			int msPos = 0;
			for(int i = 0; i < _modeSegments.Count; ++i)
			{
				var msValuesPerModeTrk = new List<MsValues>();
				msValuesListList.Add(msValuesPerModeTrk);
				ModeSegment modeSegment = _modeSegments[i];
				IReadOnlyList<ModeGrpTrk> modeGrpTrks = modeSegment.ModeGrpTrks;
				int modeSegmentMsPosition = modeSegment.MsPositionReContainer;
				for(int j = 0; j < modeGrpTrks.Count; j++)
				{
					Debug.Assert(msPos == modeSegmentMsPosition + modeGrpTrks[j].MsPositionReContainer);

					msValuesPerModeTrk.Add(new MsValues(msPos, modeGrpTrks[j].MsDuration));
					msPos += modeGrpTrks[j].MsDuration;
				}
			}
			return msValuesListList;
		}

		public IReadOnlyList<IReadOnlyList<IReadOnlyList<MsValues>>> GetMsValuesOfIUniqueDefs()
		{
			Debug.Assert(_modeSegments != null && _modeSegments.Count > 0);

			int msPos = 0;
			var msValuesListListList = new List<List<List<MsValues>>>();
			for(int i = 0; i < _modeSegments.Count; ++i)
			{
				var msValuesListList = new List<List<MsValues>>();
				msValuesListListList.Add(msValuesListList);

				ModeSegment modeSegment = _modeSegments[i];
				IReadOnlyList<ModeGrpTrk> modeGrpTrks = modeSegment.ModeGrpTrks;
				int modeSegmentMsPosition = modeSegment.MsPositionReContainer;
				for(int j = 0; j < modeGrpTrks.Count; j++)
				{
					var msValuesList = new List<MsValues>();
					msValuesListList.Add(msValuesList);

					ModeGrpTrk pModeGrpTrk = modeGrpTrks[j];
					int modeTrkMsPosition = modeSegmentMsPosition + pModeGrpTrk.MsPositionReContainer;
					for(int k = 0; k < pModeGrpTrk.Count; k++)
					{
						Debug.Assert(msPos == modeTrkMsPosition + pModeGrpTrk[k].MsPositionReFirstUD);

						msValuesList.Add(new MsValues(msPos, pModeGrpTrk[k].MsDuration));
						msPos += pModeGrpTrk[k].MsDuration;
					}
				}
			}
			return msValuesListListList;
		}

		internal void AddToSeq(Seq seq)
		{
			List<Trk> trkList = ModeSegmentListToTrkList();
			Trk trk = new Trk(trkList[0].MidiChannel);
			foreach(Trk t in trkList)
			{
				trk.AddRange(t);
			}
			seq.SetTrk(trk);
		}

		/// <summary>
		/// The barline msPositions contributed by a specific voice.
		/// </summary>
		/// <returns></returns>
		public abstract List<int> BarlineMsPositions();

		/// <summary>
		/// Returns the smallest ModeGrpTrk msPosition greater than the middle msPosition of the ModeSegment.
		/// </summary>
		protected int MidBarlineMsPos(IReadOnlyList<IReadOnlyList<MsValues>> msValuesListList, int ModeSegmentNumber)
		{
			Debug.Assert(ModeSegmentNumber > 0);
			int thisModeSegmentIndex = ModeSegmentNumber - 1;
			IReadOnlyList<MsValues> thisList = msValuesListList[thisModeSegmentIndex];

			int thisListEndMsPos = thisList[thisList.Count - 1].EndMsPosition;
			int prevListEndMsPos = 0;
			if(ModeSegmentNumber > 1)
			{
				int prevModeIndex = ModeSegmentNumber - 2;
				IReadOnlyList<MsValues> prevList = msValuesListList[prevModeIndex];
				prevListEndMsPos = prevList[prevList.Count - 1].EndMsPosition;
			}

			int midMsPos = prevListEndMsPos + ((thisListEndMsPos - prevListEndMsPos) / 2); // end barline msPosition for ModeSegment 1, divided by 2
			int barlineMsPos = -1;
			foreach(MsValues msValues in thisList)
			{
				if(msValues.MsPosition > midMsPos)
				{
					barlineMsPos = msValues.MsPosition;
					break;
				}
			}
			return barlineMsPos;
		}

		/// <summary>
		/// This function converts the modeSegment list to a list of Trks having the same midiChannel.
		/// </summary>
		private List<Trk> ModeSegmentListToTrkList()
		{
			List<Trk> trkList = new List<Trk>();
			foreach(ModeSegment modeSegment in _modeSegments)
			{
				Trk trk = modeSegment.ToTrk(); 
				trkList.Add(trk);
			}
			return trkList;
		}

		internal List<ModeSegment> ModeSegments { get => _modeSegments; }
		protected List<ModeSegment> _modeSegments = new List<ModeSegment>();
	}
}