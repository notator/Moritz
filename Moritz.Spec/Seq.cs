using Krystals5ObjectLibrary;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Moritz.Spec
{
    public class Seq : IChannelDefsContainer
    {
        /// <summary>
        /// This constructor creates the Seq.ChannelDefs field containing a list of ChannelDef objects.
        /// Each ChannelDef has a unique midi channel that is its index in the ChannelDefs list.
        /// Each ChannelDef contains a list of Trk. Each such Trk has the same channel, and is an alternative performance
        /// of the graphics that will eventually be associated with the ChannelDef's graphics. 
        /// </summary>
        /// <param name="absSeqMsPosition">Must be greater than or equal to zero.</param>
        /// <param name="trks">Each Trk must have a constructed UniqueDefs list which is either empty, or contains any
        /// combination of MidiRestDef or MidiChordDef.
        /// Each trk.MsPositionReContainer must be 0. All trk.UniqueDef.MsPositionReFirstUD values must be set correctly.
        /// All Trks that have the same channel must have the same trk.DurationsCount. (They are different performances of the same ChordSymbols.)
        /// <para>Not all the Seq's channels need to be given an explicit Trk in the trks argument. The seq will be given empty
        /// (default) Trks for the channels that are missing.
        /// trks.Count must be less than or equal to numberOfMidiChannels.</para>
        /// </param>
        /// <param name="numberOfMidiChannels">Each Voice has one channel index.</param>
        public Seq(int absSeqMsPosition, List<ChannelDef> channelDefs)
        {
            #region conditions
            Debug.Assert(absSeqMsPosition >= 0);
            Debug.Assert(channelDefs != null && channelDefs.Count <= 16);
            foreach(var channelDef in ChannelDefs)
            {
                foreach(var trk in channelDef.Trks)
                {
                    trk.ChannelDefsContainer = this;
                    trk.MsPositionReContainer = 0;
                    trk.AssertConsistency();
                }
            }
            #endregion conditions

            _absMsPosition = absSeqMsPosition;

            AssertConsistency();
        }

        public Seq Clone()
        {
            var newMidiChannelDefs = new List<ChannelDef>(); 

            foreach(var oldMidiChannelDef in this.ChannelDefs)
            {
                List<Trk> newTrks = new List<Trk>();
                foreach(var oldTrk in oldMidiChannelDef.Trks)
                {
                    newTrks.Add((Trk)oldTrk.Clone());
                }
                newMidiChannelDefs.Add(new ChannelDef(newTrks));
            }

            Seq clone = new Seq(_absMsPosition, newMidiChannelDefs);

            return clone;
        }

        /// <summary>
        /// Concatenates seq2 to the caller (seq1). Returns a pointer to the caller.
        /// Both Seqs must be normalized before calling this function.
        /// When this function is called, seq2.AbsMsPosition is the earliest position, relative to seq1, at which it can be concatenated.
        /// When it returns, seq2's ChannelDefs will have been concatenated to Seq1, and seq1 is consistent.
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
            Debug.Assert(ChannelDefs.Count == seq2.ChannelDefs.Count);
            Debug.Assert(this.IsNormalized);
            Debug.Assert(seq2.IsNormalized);
            for(int i = 0; i < ChannelDefs.Count; ++i)
            {
                Debug.Assert(ChannelDefs[i].Trks.Count == seq2.ChannelDefs[i].Count);
            }
            #endregion

            int nVoiceDefs = ChannelDefs.Count;

            #region find concatMsPos
            int absConcatMsPos = seq2.AbsMsPosition;
            if(seq2.AbsMsPosition < (AbsMsPosition + MsDuration))
            {
                for(int i = 0; i < nVoiceDefs; ++i)
                {
                    ChannelDef voiceDef1 = _voiceDefs[i];
                    ChannelDef voiceDef2 = seq2.ChannelDefs[i];
                    int earliestAbsConcatPos = voiceDef1.MsPositionReContainer + voiceDef1.EndMsPositionReFirstIUD - voiceDef2.MsPositionReContainer;
                    absConcatMsPos = (earliestAbsConcatPos > absConcatMsPos) ? earliestAbsConcatPos : absConcatMsPos;
                }
            }
            #endregion

            #region concatenation
            for(int i = 0; i < nVoiceDefs; ++i)
            {
                ChannelDef voiceDef1 = _voiceDefs[i];
                ChannelDef voiceDef2 = seq2.ChannelDefs[i];

                int nTrks = voiceDef1.Trks.Count;

                for(int j = 0; j < nTrks; ++j)
                {
                    Trk trk1 = voiceDef1.Trks[j];
                    Trk trk2 = voiceDef2.Trks[j];
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

            foreach(var channelDef in ChannelDefs)
            {
                foreach(Trk trk in channelDef.Trks)
                {
                    trk.AgglomerateRests();
                }
            }

            AssertConsistency();

            return this;
        }

        /// <summary>
        /// Set empty Trks to contain a single MidiRestDef having the msDuration of the other Trks.
        /// </summary>
        public void PadEmptyTrks()
        {
            int msDuration = MsDuration; // Getting this value checks that all trk msDurations are either 0, or the same.

            foreach(var channelDef in ChannelDefs)
            {
                foreach(Trk trk in channelDef.Trks)
                {
                    if(trk.UniqueDefs.Count == 0)
                    {
                        trk.Add(new MidiRestDef(0, msDuration));
                    }
                }
            }
        }

        /// <summary>
        /// An exception is thrown if:
        ///    1) the first argument value is less than or equal to 0.
        ///    2) the argument contains duplicate msPositions.
        ///    3) the argument is not in ascending order.
        ///    4) a Trk.MsPositionReContainer is not 0.
        ///    5) an msPosition is not the endMsPosition of any IUniqueDef in the seq.
        /// </summary>
        private void CheckBarlineMsPositions(IReadOnlyList<int> barlineMsPositionsReSeq)
        {
            Debug.Assert(barlineMsPositionsReSeq[0] > 0, "The first msPosition must be greater than 0.");

            for(int i = 0; i < barlineMsPositionsReSeq.Count; ++i)
            {
                int msPosition = barlineMsPositionsReSeq[i];
                Debug.Assert(msPosition <= this.MsDuration);
                for(int j = i + 1; j < barlineMsPositionsReSeq.Count; ++j)
                {
                    Debug.Assert(msPosition != barlineMsPositionsReSeq[j], "Error: Duplicate barline msPositions.");
                }
            }

            int currentMsPos = -1;
            foreach(int msPosition in barlineMsPositionsReSeq)
            {
                Debug.Assert(msPosition > currentMsPos, "Value out of order.");
                currentMsPos = msPosition;
                bool found = false;
                foreach(var channelDef in ChannelDefs)
                {
                    foreach(Trk trk in channelDef.Trks)
                    {
                        Debug.Assert(trk.MsPositionReContainer == 0);
                        foreach(IUniqueDef iud in trk.UniqueDefs)
                        {
                            if(msPosition == (iud.MsPositionReFirstUD + iud.MsDuration))
                            {
                                found = true;
                                break;
                            }
                        }
                        if(found)
                        {
                            break;
                        }

                    }
                }
                Debug.Assert(found, "Error: barline must be at the endMsPosition of at least one IUniqueDef.");
            }
        }

        /// <summary>
        /// AbsMsPosition is greater than or equal 0.
        /// There is at least one ChannelDef in ChannelDefs and at least one Trk in each ChannelDef.
        /// Trk.AssertConsistency() is called on all Trks.
        /// All ChannelDef.Trks.Count values are the same.
        /// All ChannelDef.MsPositionReContainer values are 0.
        /// All Trks have the same number of events as the Trk at index 0 in the same ChannelDef.
        /// All Trks having the same index in any ChannelDef
        ///     1. have the same MsDuration.
        ///     2. have the same channel event sequence as the channel events at ChannelDefs[0].Trks[0]
        /// (in Seqs) All ChannelDefs contain Trks that contain only MidiRestDef or MidiChordDef objects.
        /// </summary>
        public void AssertConsistency()
        {
            Debug.Assert(AbsMsPosition >= 0);
            Debug.Assert(ChannelDefs.Count > 0);

            foreach(var channelDef in ChannelDefs)
            {
                foreach(var trk in channelDef.Trks)
                {
                    trk.AssertConsistency();
                }
            }

            int nTrks = ChannelDefs[0].Trks.Count;
            for(int channelDefIndex = 1; channelDefIndex < ChannelDefs.Count; ++channelDefIndex)
            {
                var channelDef = ChannelDefs[channelDefIndex];
                Debug.Assert(channelDef.Trks.Count == nTrks);
                Debug.Assert(channelDef.MsPositionReContainer == 0);
                int nChannelDefTrkEvents = channelDef.Trks[0].Count;
                for(int trkIndex = 1; trkIndex < nTrks; ++trkIndex)
                {
                    Debug.Assert(channelDef.Trks[trkIndex].Count == nChannelDefTrkEvents);
                }
            }

            for(int trkIndex = 0; trkIndex < nTrks; ++nTrks)
            {
                int msDuration = ChannelDefs[0].Trks[trkIndex].MsDuration;
                for(int channelDefIndex = 1; channelDefIndex < ChannelDefs.Count; ++channelDefIndex)
                {
                    var channelDef = ChannelDefs[channelDefIndex];
                    Debug.Assert(channelDef.Trks[trkIndex].MsDuration == msDuration);
                }
            }

            List<List<int>> trk0ChannelEventSequence = GetChannelIndicesSequence(ChannelDefs, 0);
            for(int trkIndex = 1; trkIndex < nTrks; ++nTrks)
            {
                List<List<int>> trkChannelIndicesSequence = GetChannelIndicesSequence(ChannelDefs, trkIndex);
                for(int i = 0; i < trkChannelIndicesSequence.Count; ++i )
                {
                    List<int> channelIndices0 = trk0ChannelEventSequence[i];
                    List<int> channelIndices1 = trkChannelIndicesSequence[i];

                    Debug.Assert(channelIndices0.SequenceEqual(channelIndices1));
                }
            }
        }

        private List<List<int>> GetChannelIndicesSequence(IReadOnlyList<ChannelDef> channelDefs, int trkLevel)
        {
            List<Trk> channelTrks = new List<Trk>();
            foreach(var channelDef in channelDefs)
            {
                channelTrks.Add(channelDef.Trks[trkLevel]);
            }
            
            var channelIndicesPosSequence = new List<Tuple<List<int>, int>>();

            for(var channelIndex = 0; channelIndex < channelTrks.Count; ++channelIndex)
            {
                var trk = channelTrks[channelIndex];
                foreach(var uniqueDef in trk.UniqueDefs)
                {
                    var msPos = uniqueDef.MsPositionReFirstUD;
                    channelIndicesPosSequence.Add(new Tuple<List<int>, int>(new List<int>() { channelIndex }, msPos));
                }
            }

            channelIndicesPosSequence.OrderBy(x => x.Item2);

            for(int i = channelIndicesPosSequence.Count - 1; i > 0; i--)
            {
                var mPos2 = channelIndicesPosSequence[i].Item2;
                var mPos1 = channelIndicesPosSequence[i-1].Item2;
                if(mPos2 == mPos1)
                {
                    List<int> channelIndices = channelIndicesPosSequence[i].Item1;
                    channelIndicesPosSequence[i-1].Item1.AddRange(channelIndices);
                    channelIndicesPosSequence[i-1].Item1.Sort();
                    channelIndicesPosSequence.RemoveAt(i);
                }
            }

            List<List<int>> channelIndicesSequence = new List<List<int>>();
            foreach(var tuple in channelIndicesPosSequence)
            {
                channelIndicesSequence.Add(tuple.Item1);
            }
            return channelIndicesSequence;
        }

        #region sort functions
        public void SortVelocityIncreasing()
        {
            foreach(var channelDef in ChannelDefs)
            {
                foreach(Trk trk in channelDef.Trks)
                {
                    trk.SortVelocityIncreasing();
                }
            }
        }
        public void SortVelocityDecreasing()
        {
            foreach(var channelDef in ChannelDefs)
            {
                foreach(Trk trk in channelDef.Trks)
                {
                    trk.SortVelocityDecreasing();
                }
            }
        }
        public void SortRootNotatedPitchAscending()
        {
            foreach(var channelDef in ChannelDefs)
            {
                foreach(Trk trk in channelDef.Trks)
                {
                    trk.SortRootNotatedPitchAscending();
                }
            }
        }
        public void SortRootNotatedPitchDescending()
        {
            foreach(var channelDef in ChannelDefs)
            {
                foreach(Trk trk in channelDef.Trks)
                {
                    trk.SortRootNotatedPitchDescending();
                }
            }
        }
        #endregion sort functions

        /// <summary>
        /// Aligns the Trk UniqueDefs whose indices are given in the argument.
        /// The argument.Count must be equal to the number of ChannelDefs in the Seq,
        /// and in order of the channelDefs in the seq's ChannelDefs list.
        /// Each alignmentPosition must be a valid UniqueDef index in each Trk in each ChannelDef.
        /// The Seq is normalized at the end of this function by adding rests at the beginnings and ends of trks as necessary.
		/// The seq's msDuration changes automatically.
        /// </summary>
        public void AlignTrkUniqueDefs(List<int> indicesToAlign)
        {
            Debug.Assert(indicesToAlign.Count == this.ChannelDefs.Count);
            List<int> newMsPositionsReContainer = new List<int>();
            int minMsPositionReContainer = int.MaxValue;
            for(int i = 0; i < ChannelDefs.Count; ++i)
            {
                newMsPositionsReContainer.Add(0); // default value
                ChannelDef channelDef = ChannelDefs[i];
                foreach(Trk trk in channelDef.Trks)
                {
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
            }
            for(int i = 0; i < ChannelDefs.Count; ++i)
            {
                ChannelDef channelDef = ChannelDefs[i];
                foreach(Trk trk in channelDef.Trks)
                {
                    trk.MsPositionReContainer = newMsPositionsReContainer[i] - minMsPositionReContainer;
                }
            }

            AssertConsistency();
        }


        public void AlignTrkAxes()
        {
            List<int> indicesToAlign = new List<int>();
            foreach(var channelDef in ChannelDefs)
            {
                foreach(Trk trk in channelDef.Trks)
                {
                    indicesToAlign.Add(trk.AxisIndex);
                }
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
            Debug.Assert(msShifts.Count == this.ChannelDefs.Count);
            for(int i = 0; i < msShifts.Count; ++i)
            {
                ChannelDef channelDef = ChannelDefs[i];
                foreach(Trk trk in channelDef.Trks)
                {
                    trk.MsPositionReContainer += msShifts[i];
                }
            }

            AssertConsistency();
        }

        /// <summary>
        /// True if the earliest trk.MsPositionReContainer is 0.
        /// </summary>
        internal bool IsNormalized
        {
            get
            {
                int minMsPositionReContainer = int.MaxValue;
                foreach(var channelDef in ChannelDefs)
                {
                    foreach(var trk in channelDef.Trks)
                    {
                        minMsPositionReContainer = (minMsPositionReContainer < trk.MsPositionReContainer) ? minMsPositionReContainer : trk.MsPositionReContainer;
                    }
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
            AssertConsistency();
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

            foreach(var channelDef in ChannelDefs)
            {
                foreach(Trk trk in channelDef.Trks)
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
            }

            Debug.Assert(originalMsDuration == MsDuration);

            AssertConsistency();
        }


        /// <summary>
        /// returns a list containing the msPositions of all the IUniqueDefs in all Trks
        /// plus the endMsPosition of the final object.
        /// </summary>
        /// <returns></returns>
        private List<int> GetMsPositions()
        {
            int originalMsDuration = MsDuration;

            List<int> originalMsPositions = new List<int>();
            foreach(var channelDef in ChannelDefs)
            {
                foreach(Trk trk in channelDef.Trks)
                {
                    foreach(IUniqueDef iud in trk)
                    {
                        int msPos = iud.MsPositionReFirstUD;
                        if(!originalMsPositions.Contains(msPos))
                        {
                            originalMsPositions.Add(msPos);
                        }
                    }
                }
            }
            originalMsPositions.Sort();
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

            foreach(var channelDef in ChannelDefs)
            {
                foreach(Trk trk in channelDef.Trks)
                {
                    trk.SetPitchWheelSliders(pitchWheelValuesPerMsPosition);
                }
            }
        }
        #endregion Envelopes

        private int _absMsPosition;
        public int AbsMsPosition
        {
            get { return _absMsPosition; }
            set
            {
                Debug.Assert(value >= 0);
                _absMsPosition = value;
            }
        }

        /// <summary>
        /// The duration between the beginning of the first UniqueDef in the Seq and the end of the last UniqueDef in the Seq.
        /// Getting this value checks that all trk msDurations are equal.
        /// Setting this value stretches or compresses the msDurations of all the trks and their contained UniqueDefs.
        /// </summary>
        public virtual int MsDuration
        {
            get
            {
                Debug.Assert(ChannelDefs != null && ChannelDefs.Count > 0);
                int msDuration = ChannelDefs[0].MsDuration;
                foreach(var channelDef in ChannelDefs)
                {
                    Debug.Assert(channelDef.MsDuration == msDuration);
                    foreach(var trk in channelDef.Trks)
                    {
                        Debug.Assert(trk.MsDuration == msDuration);
                    }
                }
                return msDuration;
            }
            set
            {
                Debug.Assert(ChannelDefs.Count > 0);
                // there is a channelDef that begins at msPosition==0.
                int currentDuration = MsDuration;
                double factor = ((double)value) / currentDuration;
                foreach(var channelDef in ChannelDefs)
                {
                    foreach(var trk in channelDef.Trks)
                    {
                        trk.MsDuration = (int)Math.Round(trk.MsDuration * factor);
                        trk.MsPositionReContainer = (int)Math.Round(trk.MsPositionReContainer * factor);
                    }
                }
                int roundingError = value - MsDuration;
                if(roundingError != 0)
                {
                    foreach(var channelDef in ChannelDefs)
                    {
                        foreach(var trk in channelDef.Trks)
                        {
                            if((trk.EndMsPositionReFirstIUD + roundingError) == value)
                            {
                                trk.EndMsPositionReFirstIUD += roundingError;
                            }
                        }
                    }
                }
                Debug.Assert(MsDuration == value);
            }
        }

        public IReadOnlyList<ChannelDef> ChannelDefs
        {
            get => _voiceDefs;
        }
        private List<ChannelDef> _voiceDefs = new List<ChannelDef>();
    }
}
