using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Moritz.Spec
{
	public class Seq : ITrksContainer
	{
        /// <summary>
        /// trks.Count must be less than or equal to midiChannelIndexPerOutputVoice.Count. (See trks parameter comment.)
        /// </summary>
        /// <param name="absSeqMsPosition">Must be greater than or equal to zero.</param>
        /// <param name="trks">Each Trk must have a constructed UniqueDefs list which is either empty, or contains any
        /// combination of MidiRestDef or MidiChordDef. Each trk.MidiChannel must be unique and present in the
        /// midiChannelIndexPerOutputVoice list. Not all the Seq's channels need to be given an explicit Trk in the trks
        /// argument. The seq will be given empty (default) Trks for the channels that are missing.
        /// The MsPositionReContainer field in each argument trk can have any value, but the Seq is Normalized
        /// by this constructor.</param>
        /// <param name="barlineMsPositionsReSeq">Can be null or empty. All barlineMsPositions must be unique, in ascending order, and less than or equal to the final msDuration of the Seq.</param>
        /// <param name="midiChannelIndexPerOutputVoice">The Seq will contain one trk for each channel in this list.</param>
        public Seq(int absSeqMsPosition, List<Trk> trks, IReadOnlyList<int> midiChannelIndexPerOutputVoice)
        {
            #region conditions
            Debug.Assert(absSeqMsPosition >= 0);
            for(int i = 0; i < trks.Count - 1; ++i)
            {
                for(int j = i + 1; j < trks.Count; ++j)
                {
                    Debug.Assert(trks[i].MidiChannel != trks[j].MidiChannel);
                }
            }
            #endregion conditions

            _absMsPosition = absSeqMsPosition;

            foreach(int channel in midiChannelIndexPerOutputVoice)
            {
                Trk newTrk = null;
                foreach(Trk trk in trks)
                {
                    if(trk.MidiChannel == channel)
                    {
                        newTrk = trk;
                        break;
                    }
                }
                if(newTrk == null)
                {
                    newTrk = new Trk(channel);
                }
                newTrk.Container = this;
                _trks.Add(newTrk);
            }

            Normalize();

            AssertChannelConsistency(midiChannelIndexPerOutputVoice);
            AssertSeqConsistency();
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
        /// Concatenates seq2 to the caller (seq1). Returns a pointer to the caller.
        /// Both Seqs must be normalized before calling this function.
        /// When this function is called, seq2.AbsMsPosition is the earliest position, relative to seq1, at which it can be concatenated.
        /// When it returns, seq2's Trks will have been concatenated to Seq1, and seq1 is consistent.
        /// If Seq2 is needed after calling this function, then it should be cloned first.
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
            Debug.Assert(this.IsNormalized);
            Debug.Assert(seq2.IsNormalized);
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
                        trk1.Add(new MidiRestDef(trk1.EndMsPositionReFirstIUD, trk2AbsStartMsPosition - trk1AbsEndMsPosition));
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

        /// <summary>
        /// Replaces (or updates) a trk having the same channel in the seq. trk.Container is set to the Seq.
        /// An exception is thrown if the trk to replace is not found.
        /// trk.MsPositionReContainer can have any value, but this function normalizes the seq.
        /// </summary>
        /// <param name="trk"></param>
        public void SetTrk(Trk trk)
        {
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

            Normalize();
        }

        /// <summary>
        /// Every Trk.MidiChannel is parallel to the indices in midiChannelIndexPerOutputVoice.
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
                }
            }

            Debug.Assert(nTrks == midiChannelIndexPerOutputVoice.Count);
        }

        /// <summary>
        /// Every Trk in _trks is either empty, or contains any combination of MidiRestDef or MidiChordDef.
        /// There is always a trk having MsPositionReContainer == zero.
        /// </summary>
        private void AssertSeqConsistency()
        {
            Debug.Assert(_trks != null && _trks.Count > 0);
            #region Every Trk in _trks is either empty, or contains any combination of MidiChordDef and MidiRestDef objects.
            foreach(Trk trk in _trks)
            {
                trk.AssertConsistentInSeq();
            }
            #endregion
        }

        /// <summary>
        /// Shifts the Trks so that the earliest trk.MsPositionReContainer is 0.
        /// </summary>
        public void Normalize()
        {
            int minMsPositionReContainer = int.MaxValue;
            foreach(Trk trk in _trks)
            {
                minMsPositionReContainer = (minMsPositionReContainer < trk.MsPositionReContainer) ? minMsPositionReContainer : trk.MsPositionReContainer;
            }
            if(minMsPositionReContainer != 0)
            {
                foreach(Trk trk in _trks)
                {
                    trk.MsPositionReContainer -= minMsPositionReContainer;
                }
            }
        }

        #region sort functions
        public void SortVelocityIncreasing()
        {
            foreach(Trk trk in Trks)
            {
                trk.SortVelocityIncreasing();
            }
        }
        public void SortVelocityDecreasing()
        {
            foreach(Trk trk in Trks)
            {
                trk.SortVelocityDecreasing();
            }
        }
        public void SortRootNotatedPitchAscending()
        {
            foreach(Trk trk in Trks)
            {
                trk.SortRootNotatedPitchAscending();
            }
        }
        public void SortRootNotatedPitchDescending()
        {
            foreach(Trk trk in Trks)
            {
                trk.SortRootNotatedPitchDescending();
            }
        }
        #endregion sort functions

        /// <summary>
        /// Aligns the Trk UniqueDefs whose indices are given in the argument.
        /// The argument.Count must be equal to the number of trks in the Seq, and in order of the trks in the seq's _trks list.
        /// Each alignmentPosition must be a valid UniqueDef index in its trk.
        /// The Seq is normalized at the end of this function. Its msDuration changes automatically.
        /// </summary>
        public void AlignTrkUniqueDefs(List<int> indicesToAlign)
        {
            Debug.Assert(indicesToAlign.Count == this._trks.Count);
            List<int> newMsPositionsReContainer = new List<int>();
            int minMsPositionReContainer = int.MaxValue;
            for(int i = 0; i < _trks.Count; ++i)
            {
                newMsPositionsReContainer.Add(0); // default value
                Trk trk = _trks[i];
                int index = indicesToAlign[i];
                Debug.Assert(index >= 0);

                if(index < trk.UniqueDefs.Count)
                {
                    int alignmentMsPositionReFirstUD = 0;
                    for(int j = 0; j < index; ++j)
                    {
                        alignmentMsPositionReFirstUD += trk.UniqueDefs[j].MsDuration;
                    }

                    int newMsPositionReContainer = trk.MsPositionReContainer - alignmentMsPositionReFirstUD;
                    newMsPositionsReContainer[i] = newMsPositionReContainer;
                    minMsPositionReContainer = (minMsPositionReContainer < newMsPositionReContainer) ? minMsPositionReContainer : newMsPositionReContainer;
                }
            }
            for(int i = 0; i < _trks.Count; ++i)
            {
                Trk trk = _trks[i];
                trk.MsPositionReContainer = newMsPositionsReContainer[i] - minMsPositionReContainer;
            }

            Normalize();

            AssertSeqConsistency();
        }

        public void AlignTrkAxes()
        {
            List<int> indicesToAlign = new List<int>();
            foreach(Trk trk in Trks)
            {
                indicesToAlign.Add(trk.AxisIndex);
            }
            AlignTrkUniqueDefs(indicesToAlign);
        }

        /// <summary>
        /// Shifts each track by adding the corresponding millisecond argument to its MsPositionReContainer attribute.
        /// The argument.Count must be equal to the number of trks in the Seq, and in order of the trks in the seq's _trks list.
        /// The Seq is normalized at the end of this function. Its msDuration changes automatically.
        /// </summary>
        /// <param name="msShifts">The number of milliseconds to shift each trk. (May be negative.)</param>
        public void ShiftTrks(List<int> msShifts)
        {
            Debug.Assert(msShifts.Count == this._trks.Count);
            for(int i = 0; i < msShifts.Count; ++i)
            {
                Trk trk = _trks[i];
                trk.MsPositionReContainer += msShifts[i];
            }
            Normalize();

            AssertSeqConsistency();
        }

        /// <summary>
        /// True if the earliest trk.MsPositionReContainer is 0.
        /// </summary>
        internal bool IsNormalized
        {
            get
            {
                int minMsPositionReContainer = int.MaxValue;
                foreach(Trk trk in _trks)
                {
                    minMsPositionReContainer = (minMsPositionReContainer < trk.MsPositionReContainer) ? minMsPositionReContainer : trk.MsPositionReContainer;
                }
                return (minMsPositionReContainer == 0);
            }
        }

        #region Envelopes
        /// <summary>
        /// This function does not change the MsDuration of the Seq.
        /// See Envelope.TimeWarp() for a description of the arguments.
        /// </summary>
        /// <param name="envelope"></param>
        /// <param name="distortion"></param>
        public void TimeWarp(Envelope envelope, double distortion)
        {
            AssertSeqConsistency();
            int originalMsDuration = MsDuration;
            List<int> originalMsPositions = GetMsPositions();
            Dictionary<int, int> warpDict = new Dictionary<int, int>();
            #region get warpDict
            List<int> newMsPositions = envelope.TimeWarp(originalMsPositions, distortion);

            for(int i = 0; i < newMsPositions.Count; ++i)
            {
                warpDict.Add(originalMsPositions[i], newMsPositions[i]);
            }
            #endregion get warpDict

            foreach(Trk trk in _trks)
            {
                List<IUniqueDef> iuds = trk.UniqueDefs;
                IUniqueDef iud = null;
                int msPos = 0;
                for(int i = 1; i < iuds.Count; ++i)
                {
                    iud = iuds[i - 1];
                    msPos = warpDict[iud.MsPositionReFirstUD];
                    iud.MsPositionReFirstUD = msPos;
                    iud.MsDuration = warpDict[iuds[i].MsPositionReFirstUD] - msPos;
                    msPos += iud.MsDuration;
                }
                iud = iuds[iuds.Count - 1];
                iud.MsPositionReFirstUD = msPos;
                iud.MsDuration = originalMsDuration - msPos;
            }

            Debug.Assert(originalMsDuration == MsDuration);

            AssertSeqConsistency();
        }

        /// <summary>
        /// returns a list containing the msPositions of all the IUniqueDefs plus the endMsPosition of the final object.
        /// </summary>
        /// <returns></returns>
        private List<int> GetMsPositions()
        {
            int originalMsDuration = MsDuration;

            List<int> originalMsPositions = new List<int>();
            foreach(Trk trk in _trks)
            {
                foreach(IUniqueDef iud in trk)
                {
                    int msPos = iud.MsPositionReFirstUD;
                    if(!originalMsPositions.Contains(msPos))
                    {
                        originalMsPositions.Add(msPos);
                    }
                }
                originalMsPositions.Sort();
            }
            originalMsPositions.Add(originalMsDuration);

            return originalMsPositions;
        }

        public void SetPitchWheelSliders(Envelope envelope)
        {
            #region condition
            if(envelope.Domain != 127)
            {
                throw new ArgumentException($"{nameof(envelope.Domain)} must be 127.");
            }
            #endregion condition

            List<int> msPositions = GetMsPositions();

            Dictionary<int, int> pitchWheelValuesPerMsPosition = envelope.GetValuePerMsPosition(msPositions);

            foreach(Trk trk in _trks)
            {
                trk.SetPitchWheelSliders(pitchWheelValuesPerMsPosition);
            }
        }
        #endregion Envelopes

        public IReadOnlyList<Trk> Trks { get { return _trks.AsReadOnly(); } }
        private List<Trk> _trks = new List<Trk>();

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
						int endMsPosReSeq = trk.MsPositionReContainer + lastIUD.MsPositionReFirstUD + lastIUD.MsDuration;
						msDuration = (msDuration < endMsPosReSeq) ? endMsPosReSeq : msDuration;
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
    }
}
