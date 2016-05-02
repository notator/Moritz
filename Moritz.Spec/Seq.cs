using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Moritz.Spec
{
	public class Seq : IVoiceDefContainer
	{
        /// <summary>
        /// <para>Each Trk in trks has a constructed UniqueDefs list which is either empty, or contains any
        /// combination of RestDef or MidiChordDef.</para>
        /// <para>There must be one Trk entry per midiChannelIndexPerOutputVoice. Each tkr.MidiChannel must be
        /// unique and present in the midiChannelIndexPerOutputVoice list.</para>
        /// The Seq will have all the trks corresponding to the midiChannels in midiChannelIndexPerOutputVoice,
        /// but some of them can have an empty UniqueDefs list.
        /// <para>Argument trk.MsPositionReSeq values must be >= 0, and set relative to the start of the Seq.</para>
        /// </summary>
        public Seq(int absSeqMsPosition, List<Trk> trks, IReadOnlyList<int> midiChannelIndexPerOutputVoice)
		{
            Debug.Assert(absSeqMsPosition >= 0);
            _absMsPosition = absSeqMsPosition;

            foreach(Trk trk in trks)
            {
                Debug.Assert(trk.MsPositionReContainer >= 0);
                trk.Container = this;
                _trks.Add(trk);
            }

			AssertChannelConsistency(midiChannelIndexPerOutputVoice);
			AssertSeqConsistency();
		}

        public Seq(int absSeqMsPosition, IReadOnlyList<int> midiChannelIndexPerOutputVoice)
        {
            Debug.Assert(absSeqMsPosition >= 0);
            _absMsPosition = absSeqMsPosition;

            foreach(int channel in midiChannelIndexPerOutputVoice)
            {
                Trk trk = new Trk(channel);
                _trks.Add(trk);
            }

            AssertChannelConsistency(midiChannelIndexPerOutputVoice);
            AssertSeqConsistency();
        }

        /// <summary>
        /// Replaces a trk having the same channel. trk.Container is set to the Seq.
        /// </summary>
        /// <param name="trk"></param>
        public void SetTrk(Trk trk)
        {
            Debug.Assert(trk.MsPositionReContainer >= 0);
            bool found = false;
            for(int i = 0; i < _trks.Count; ++i)
            {
                if(trk.MidiChannel == _trks[i].MidiChannel)
                {
                    trk.Container = this;
                    _trks[i] = trk;
                    found = true;
                    break;
                }
            }
            Debug.Assert(found == true, "Illegal channel");
        }

        public IReadOnlyList<Trk> Trks { get { return _trks.AsReadOnly(); } }

        /// <summary>
        /// Aligns the Trks to the alignmentMsPositions given in the argument, changing the Trks' MsPositionReContainer and width of
        /// the Seq as necessary.
        /// The integer part of each alignmentPosition is a valid UniqueDef index in a trk.
        /// The decimal part of each alignmentPosition, relates to the duration of the indexed UniqueDef.
        /// The argument.Count must be equal to the number of trks in the Seq, and in order of the trks in the seq's _trks list.
        /// </summary>
        public void AlignTrks(List<double> alignmentPositions)
        {
            Debug.Assert(alignmentPositions.Count == this._trks.Count);
            List<int> newMsPositionsReContainer = new List<int>();
            int minMsPositionReContainer = int.MaxValue;
            for(int i = 0; i < _trks.Count; ++i)
            {
                Trk trk = _trks[i];
                double alignmentPosition = alignmentPositions[i];
                Debug.Assert(alignmentPosition >= 0 && alignmentPosition < trk.UniqueDefs.Count);
                int index = (int)Math.Floor(alignmentPosition);
                double fract = alignmentPosition - index;

                int alignmentMsPositionReFirstUD = 0;
                for(int j = 0; j <= index; ++j)
                {
                    alignmentMsPositionReFirstUD += trk.UniqueDefs[j].MsDuration;
                }
                alignmentMsPositionReFirstUD += (int)Math.Floor(trk.UniqueDefs[index].MsDuration * fract);

                int newMsPositionReContainer = trk.MsPositionReContainer - alignmentMsPositionReFirstUD;
                newMsPositionsReContainer.Add(newMsPositionReContainer);
                minMsPositionReContainer = (minMsPositionReContainer < newMsPositionReContainer) ? minMsPositionReContainer : newMsPositionReContainer;
            }
            for(int i = 0; i < _trks.Count; ++i)
            {
                Trk trk = _trks[i];
                trk.MsPositionReContainer = newMsPositionsReContainer[i] - minMsPositionReContainer;
            }
            AssertSeqConsistency();
        }

        public IReadOnlyList<int> MidiChannelIndexPerOutputVoice 
		{
			get
			{
				List<int> channels = new List<int>();
				foreach(Trk trk in _trks)
				{
					channels.Add(trk.MidiChannel);
				}
				return channels;
			}
		}

        /// <summary>
        /// Concatenates seq2 to the caller (seq1). Returns a pointer to the caller.
        /// When this function is called, seq2.AbsMsPosition is the earliest position, relative to seq1, at which it can be concatenated.
        /// When it returns, seq2's Trks will have been concatenated to Seq1, and seq1 is consistent.
        /// If Seq2 is needed after calling thei function, then it should be cloned first.
        /// For example:
        /// If seq2.MsPosition==0, it will be concatenated such that there will be at least one trk concatenation without an
        /// intervening rest.
        /// If seq2.MsPosition == seq1.MsDuration, the seqs will be juxtaposed.
        /// If seq2.MsPosition > seq1.MsDuration, the seqs will be concatenated with an intervening rest.
        /// Redundant clef changes are silently removed.
        /// </summary>
        public Seq Concat(Seq seq2)
		{
            #region assertions
            Debug.Assert(_trks.Count == seq2.Trks.Count);
            AssertChannelConsistency(seq2.MidiChannelIndexPerOutputVoice);
			#endregion

			int nTrks = _trks.Count;

			#region find concatMsPos
			int absConcatMsPos = seq2.AbsMsPosition;
			if(seq2.AbsMsPosition < (AbsMsPosition + MsDuration))
			{
				for(int i = 0; i < nTrks; ++i)
				{
					Trk trk1 = _trks[i];
                    Trk trk2 = seq2.Trks[i];
					int earliestAbsConcatPos = trk1.MsPositionReContainer + trk1.EndMsPositionReFirstIUD - trk2.MsPositionReContainer;
					absConcatMsPos = (earliestAbsConcatPos > absConcatMsPos) ? earliestAbsConcatPos : absConcatMsPos;
				}
			}
			#endregion

			#region concatenation
			for(int i = 0; i < nTrks; ++i)
			{
				Trk trk2 = seq2.Trks[i];
				if(trk2.UniqueDefs.Count > 0)
				{
                    Trk trk1 = _trks[i];
                    int trk1AbsEndMsPosition = AbsMsPosition + trk1.MsPositionReContainer + trk1.EndMsPositionReFirstIUD; 
					int trk2AbsStartMsPosition = absConcatMsPos + trk2.MsPositionReContainer;
					if(trk1AbsEndMsPosition < trk2AbsStartMsPosition)
					{
						trk1.Add(new RestDef(trk1.EndMsPositionReFirstIUD, trk2AbsStartMsPosition - trk1AbsEndMsPosition));
					}
					trk1.AddRange(trk2);
				}
			}
			#endregion

			foreach(Trk trk in Trks)
			{
				trk.AgglomerateRests();
			}

			AssertSeqConsistency();

			return this;
		}

		public Seq Clone()
		{
			List<Trk> trks = new List<Trk>();
			for(int i = 0; i < _trks.Count; ++i)
			{             
                trks.Add(_trks[i].Clone());
			}

			Seq clone = new Seq(_absMsPosition, trks, MidiChannelIndexPerOutputVoice);

			return clone;
		}


        /// <summary>
        /// Every Trk.MidiChannel is unique and is parallel to the indices in midiChannelIndexPerOutputVoice.
        /// </summary>
        private void AssertChannelConsistency(IReadOnlyList<int> midiChannelIndexPerOutputVoice)
		{
			Debug.Assert(_trks != null && _trks.Count > 0);
            int nTrks = 0;		
			for(int i = 0; i < _trks.Count; ++i)
			{
                Trk trk = _trks[i] as Trk;
                if(trk != null)
                {
                    nTrks++;
                    Debug.Assert(trk.MidiChannel == midiChannelIndexPerOutputVoice[i], "All trk.MidiChannels must correspond.");
                    for(int j = i + 1; j < _trks.Count; ++j)
                    {
                        Debug.Assert(trk.MidiChannel != _trks[j].MidiChannel, "All trk.MidiChannels must be unique.");
                    }
                }
			}

            Debug.Assert(nTrks == midiChannelIndexPerOutputVoice.Count);
        }

        /// <summary>
        /// Every Trk in _trks is either empty, or contains any combination of RestDef or MidiChordDef.
        /// There is always a trk having MsPositionReContainer == zero.
        /// </summary>
        public void AssertSeqConsistency()
		{
			Debug.Assert(_trks != null && _trks.Count > 0);
            #region Every Trk in _trks is either empty, or contains any combination of RestDef or MidiChordDef.
            int minMsPositionReContainer = int.MaxValue;
            foreach(Trk trk in _trks)
            {
                trk.AssertConstructionConsistency();
                minMsPositionReContainer = (minMsPositionReContainer < trk.MsPositionReContainer) ? minMsPositionReContainer : trk.MsPositionReContainer;
            }
            Debug.Assert(minMsPositionReContainer == 0, "The minimum trk.MsPositionReContainer must always be 0." );
            #endregion
        }

		private List<Trk> _trks = new List<Trk>();
		//public List<Trk> Trks { get { return _trks; } }

		private int _absMsPosition;

        public int AbsMsPosition
		{	
			get	{ return _absMsPosition; }
			set
			{
				Debug.Assert(value >= 0);
				_absMsPosition = value;
			}
		}

		/// <summary>
		/// The duration between the beginning of the first UniqueDef in the Seq and the end of the last UniqueDef in the Seq.
		/// Setting this value stretches or compresses the msDurations of all the trks and their contained UniqueDefs.
		/// </summary>
		public virtual int MsDuration
		{ 
			get
			{
				AssertSeqConsistency();  // there is a trk that begins at msPosition==0.
				int msDuration = 0;
				foreach(Trk trk in _trks)
				{
					if(trk.UniqueDefs.Count > 0)
					{
						IUniqueDef lastIUD = trk.UniqueDefs[trk.UniqueDefs.Count - 1];
						int endMsPosReFirstIUD = lastIUD.MsPositionReFirstUD + lastIUD.MsDuration;
						msDuration = (msDuration < endMsPosReFirstIUD) ? endMsPosReFirstIUD : msDuration;
					}
				}
				return msDuration;
			}
			set
			{
				Debug.Assert(_trks.Count > 0);
				// there is a trk that begins at msPosition==0.
				int currentDuration = MsDuration;
				double factor = ((double)value) / currentDuration;
				foreach(Trk trk in _trks)
				{
					trk.MsDuration = (int) Math.Round(trk.MsDuration * factor);
					trk.MsPositionReContainer = (int) Math.Round(trk.MsPositionReContainer * factor);
				}
				int roundingError = value - MsDuration;
				if(roundingError != 0)
				{
					foreach(Trk trk in _trks)
					{
						if((trk.EndMsPositionReFirstIUD + roundingError) == value)
						{
							trk.EndMsPositionReFirstIUD += roundingError;
						}
					}
				}
				Debug.Assert(MsDuration == value); 
			}
		}

	}
}
