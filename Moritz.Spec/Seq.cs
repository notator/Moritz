using Krystals5ObjectLibrary;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Moritz.Spec
{
    public class Seq : IVoiceDefsContainer
    {
        /// <summary>
        /// This constructor creates the Seq.VoiceDefs field containing a list of VoiceDef objects.
        /// Each VoiceDef has a unique midi channel that is its index in the VoiceDefs list.
        /// Each VoiceDef contains a list of Trk. Each such Trk has the same channel, and is an alternative performance
        /// of the graphics that will eventually be associated with the VoiceDef's graphics. 
        /// </summary>
        /// <param name="absSeqMsPosition">Must be greater than or equal to zero.</param>
        /// <param name="trks">Each Trk must have a constructed UniqueDefs list which is either empty, or contains any
        /// combination of MidiRestDef or MidiChordDef.
        /// More than one trk can have the same midiChannel, but each successive trk in the trks list must have a trk.MidiChannel
        /// that is greater than or equal to the previous trk's. (i.e. the midiChannels must be in contiguous order.)
        /// Each trk.MsPositionReContainer must be 0. All trk.UniqueDef.MsPositionReFirstUD values must be set correctly.
        /// All Trks that have the same channel must have the same trk.DurationsCount. (They are different performances of the same ChordSymbols.)
        /// <para>Not all the Seq's channels need to be given an explicit Trk in the trks argument. The seq will be given empty
        /// (default) Trks for the channels that are missing.
        /// trks.Count must be less than or equal to numberOfMidiChannels.</para>
        /// </param>
        /// <param name="numberOfMidiChannels">Each Voice has one channel index.</param>
        public Seq(int absSeqMsPosition, List<Trk> trks, int numberOfMidiChannels)
        {
            #region conditions
            Debug.Assert(absSeqMsPosition >= 0);
            Debug.Assert(trks != null && trks.Count > 0);
            for(int i = 0; i < trks.Count; ++i)
            {
                Trk trk = trks[i];
                Debug.Assert(trk.MidiChannel < numberOfMidiChannels, "Unknown voice/channel index.");
                Debug.Assert(trk.MsPositionReContainer == 0);
                trk.AssertConsistency();
            }
            Debug.Assert(numberOfMidiChannels > 0 && numberOfMidiChannels <= 16);
            #endregion conditions

            int durationsCount = trks[0].DurationsCount;
            VoiceDef voiceDef = new VoiceDef(trks[0]);
            _voiceDefs.Add(voiceDef);
            for(int i = 0; i < trks.Count - 1; ++i)
            {
                Trk trk = trks[i];
                Trk nextTrk = trks[i+1];
                if(trk.MidiChannel == nextTrk.MidiChannel)
                {
                    Debug.Assert(nextTrk.DurationsCount == durationsCount);
                    voiceDef.Trks.Add(nextTrk);
                }
                else if(nextTrk.MidiChannel == trk.MidiChannel + 1)
                {
                    durationsCount = nextTrk.DurationsCount;
                    voiceDef = new VoiceDef(nextTrk);
                    _voiceDefs.Add(voiceDef);
                }
                else
                {
                    Debug.Assert(false, "Non-contiguous midiChannel.");
                }
            }

            Debug.Assert(VoiceDefs.Count == numberOfMidiChannels, "Wrong number of VoiceDefs.");

            _absMsPosition = absSeqMsPosition;

            AssertConsistency();
        }

        public Seq Clone()
        {
            List<Trk> trks = new List<Trk>();
            foreach(var voiceDef in this.VoiceDefs)
            {
                foreach(var trk in voiceDef.Trks)
                {
                    trks.Add((Trk)trk.Clone());
                }
            }

            Seq clone = new Seq(_absMsPosition, trks, VoiceDefs.Count);

            return clone;
        }

        /// <summary>
        /// Concatenates seq2 to the caller (seq1). Returns a pointer to the caller.
        /// Both Seqs must be normalized before calling this function.
        /// When this function is called, seq2.AbsMsPosition is the earliest position, relative to seq1, at which it can be concatenated.
        /// When it returns, seq2's VoiceDefs will have been concatenated to Seq1, and seq1 is consistent.
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
            Debug.Assert(VoiceDefs.Count == seq2.VoiceDefs.Count);
            Debug.Assert(this.IsNormalized);
            Debug.Assert(seq2.IsNormalized);
            for(int i = 0; i < VoiceDefs.Count; ++i)
            {
                Debug.Assert(VoiceDefs[i].Trks.Count == seq2.VoiceDefs[i].Count);
            }
            AssertChannelConsistency(seq2.VoiceDefs.Count);
            #endregion

            int nVoiceDefs = VoiceDefs.Count;

            #region find concatMsPos
            int absConcatMsPos = seq2.AbsMsPosition;
            if(seq2.AbsMsPosition < (AbsMsPosition + MsDuration))
            {
                for(int i = 0; i < nVoiceDefs; ++i)
                {
                    VoiceDef voiceDef1 = _voiceDefs[i];
                    VoiceDef voiceDef2 = seq2.VoiceDefs[i];
                    int earliestAbsConcatPos = voiceDef1.MsPositionReContainer + voiceDef1.EndMsPositionReFirstIUD - voiceDef2.MsPositionReContainer;
                    absConcatMsPos = (earliestAbsConcatPos > absConcatMsPos) ? earliestAbsConcatPos : absConcatMsPos;
                }
            }
            #endregion

            #region concatenation
            for(int i = 0; i < nVoiceDefs; ++i)
            {
                VoiceDef voiceDef1 = _voiceDefs[i];
                VoiceDef voiceDef2 = seq2.VoiceDefs[i];

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

            foreach(var voiceDef in VoiceDefs)
            {
                foreach(Trk trk in voiceDef.Trks)
                {
                    trk.AgglomerateRests();
                }
            }

            AssertConsistency();

            return this;
        }

        /// <summary>
        /// Replaces (or updates) a VoiceDef having the same channel in the seq.
        /// VoiceDef.Container is set to the Seq.
        /// An exception is thrown if the VoiceDef to replace is not found.
        /// VoiceDef.MsPositionReContainer can have any value, but this function normalizes the seq.
        /// </summary>
        /// <param name="trk"></param>
        public void SetVoiceDef(VoiceDef voiceDef)
        {
            bool found = false;
            for(int i = 0; i < VoiceDefs.Count; ++i)
            {
                if(voiceDef.MidiChannel == _voiceDefs[i].MidiChannel)
                {
                    voiceDef.Container = this;
                    _voiceDefs[i] = voiceDef;
                    found = true;
                    break;
                }
            }

            Debug.Assert(found == true, "Illegal channel");
        }

        /// <summary>
        /// Set empty Trks to contain a single MidiRestDef having the msDuration of the other Trks.
        /// </summary>
        public void PadEmptyTrks()
        {
            int msDuration = MsDuration; // Getting this value checks that all trk msDurations are either 0, or the same.

            foreach(var voiceDef in VoiceDefs)
            {
                foreach(Trk trk in voiceDef.Trks)
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
                foreach(var voiceDef in VoiceDefs)
                {
                    foreach(Trk trk in voiceDef.Trks)
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
        /// Every VoiceDef.MidiChannel and contained Trk MidiChannel is equal to the
        /// index of the VoiceDef in the VoiceDefsList.
        /// </summary>
        private void AssertChannelConsistency(int numberOfMidiChannels)
        {
            Debug.Assert(VoiceDefs != null && VoiceDefs.Count == numberOfMidiChannels);
            for(int channel = 0; channel < numberOfMidiChannels; ++channel)
            {
                var voiceDef = VoiceDefs[channel];
                Debug.Assert(channel == voiceDef.MidiChannel);
                var trks = voiceDef.Trks;
                for(int i = 0; i < trks.Count; ++i)
                { 
                    Debug.Assert(trks[i].MidiChannel == channel);
                }
            }
        }

        /// <summary>
        /// AbsMsPosition is greater than or equal 0.
        /// There is at least one VoiceDef in VoiceDefs
        /// and at least one Trk in each VoiceDef.
        /// All VoiceDef.MidiChannel and Trk.MidiChannel values are equal to the voiceDef's index in VoiceDefs.
        /// All VoiceDef.MsPositionReContainer and Trk.MsPositionReContainer values are 0.
        /// All VoiceDefs and their contained Trks have the same MsDuration.
        /// (?)All VoiceDefs contain Trks that contain only MidiRestDef or MidiChordDef objects.
        /// (?)All Trk.UniqueDef.MsPositionReFirstUD values are set correctly.
        /// </summary>
        public void AssertConsistency()
        {
            Debug.Assert(AbsMsPosition >= 0);
            Debug.Assert(VoiceDefs.Count > 0);
            AssertChannelConsistency(VoiceDefs.Count);

            int nMidiChannels = VoiceDefs.Count;
            foreach(var voiceDef in VoiceDefs)
            {
                Debug.Assert(voiceDef.MsPositionReContainer == 0);
                int msDuration = voiceDef.MsDuration;
                foreach(var trk in voiceDef.Trks)
                {
                    Debug.Assert(trk.MsPositionReContainer == 0);
                    Debug.Assert(trk.MsDuration == msDuration);
                    Debug.Assert(trk.Container == voiceDef);

                    /// (?)All VoiceDefs contain Trks that contain only MidiRestDef or MidiChordDef objects.
                    /// (?)All Trk.UniqueDef.MsPositionReFirstUD values are set correctly.
                    trk.AssertConsistency();
                }
            }
        }

        #region sort functions
        public void SortVelocityIncreasing()
        {
            foreach(var voiceDef in VoiceDefs)
            {
                foreach(Trk trk in voiceDef.Trks)
                {
                    trk.SortVelocityIncreasing();
                }
            }
        }
        public void SortVelocityDecreasing()
        {
            foreach(var voiceDef in VoiceDefs)
            {
                foreach(Trk trk in voiceDef.Trks)
                {
                    trk.SortVelocityDecreasing();
                }
            }
        }
        public void SortRootNotatedPitchAscending()
        {
            foreach(var voiceDef in VoiceDefs)
            {
                foreach(Trk trk in voiceDef.Trks)
                {
                    trk.SortRootNotatedPitchAscending();
                }
            }
        }
        public void SortRootNotatedPitchDescending()
        {
            foreach(var voiceDef in VoiceDefs)
            {
                foreach(Trk trk in voiceDef.Trks)
                {
                    trk.SortRootNotatedPitchDescending();
                }
            }
        }
        #endregion sort functions

        /// <summary>
        /// Aligns the Trk UniqueDefs whose indices are given in the argument.
        /// The argument.Count must be equal to the number of VoiceDefs in the Seq,
        /// and in order of the voiceDefs in the seq's VoiceDefs list.
        /// Each alignmentPosition must be a valid UniqueDef index in each Trk in each VoiceDef.
        /// The Seq is normalized at the end of this function by adding rests at the beginnings and ends of trks as necessary.
		/// The seq's msDuration changes automatically.
        /// </summary>
        public void AlignTrkUniqueDefs(List<int> indicesToAlign)
        {
            Debug.Assert(indicesToAlign.Count == this.VoiceDefs.Count);
            List<int> newMsPositionsReContainer = new List<int>();
            int minMsPositionReContainer = int.MaxValue;
            for(int i = 0; i < VoiceDefs.Count; ++i)
            {
                newMsPositionsReContainer.Add(0); // default value
                VoiceDef voiceDef = VoiceDefs[i];
                foreach(Trk trk in voiceDef.Trks)
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
            for(int i = 0; i < VoiceDefs.Count; ++i)
            {
                VoiceDef voiceDef = VoiceDefs[i];
                foreach(Trk trk in voiceDef.Trks)
                {
                    trk.MsPositionReContainer = newMsPositionsReContainer[i] - minMsPositionReContainer;
                }
            }

            AssertConsistency();
        }


        public void AlignTrkAxes()
        {
            List<int> indicesToAlign = new List<int>();
            foreach(var voiceDef in VoiceDefs)
            {
                foreach(Trk trk in voiceDef.Trks)
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
            Debug.Assert(msShifts.Count == this.VoiceDefs.Count);
            for(int i = 0; i < msShifts.Count; ++i)
            {
                VoiceDef voiceDef = VoiceDefs[i];
                foreach(Trk trk in voiceDef.Trks)
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
                foreach(var voiceDef in VoiceDefs)
                {
                    foreach(var trk in voiceDef.Trks)
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

            foreach(var voiceDef in VoiceDefs)
            {
                foreach(Trk trk in voiceDef.Trks)
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
            foreach(var voiceDef in VoiceDefs)
            {
                foreach(Trk trk in voiceDef.Trks)
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

            foreach(var voiceDef in VoiceDefs)
            {
                foreach(Trk trk in voiceDef.Trks)
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
                Debug.Assert(VoiceDefs != null && VoiceDefs.Count > 0);
                int msDuration = VoiceDefs[0].MsDuration;
                foreach(var voiceDef in VoiceDefs)
                {
                    Debug.Assert(voiceDef.MsDuration == msDuration);
                    foreach(var trk in voiceDef.Trks)
                    {
                        Debug.Assert(trk.MsDuration == msDuration);
                    }
                }
                return msDuration;
            }
            set
            {
                Debug.Assert(VoiceDefs.Count > 0);
                // there is a voiceDef that begins at msPosition==0.
                int currentDuration = MsDuration;
                double factor = ((double)value) / currentDuration;
                foreach(var voiceDef in VoiceDefs)
                {
                    foreach(var trk in voiceDef.Trks)
                    {
                        trk.MsDuration = (int)Math.Round(trk.MsDuration * factor);
                        trk.MsPositionReContainer = (int)Math.Round(trk.MsPositionReContainer * factor);
                    }
                }
                int roundingError = value - MsDuration;
                if(roundingError != 0)
                {
                    foreach(var voiceDef in VoiceDefs)
                    {
                        foreach(var trk in voiceDef.Trks)
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

        public IReadOnlyList<VoiceDef> VoiceDefs
        {
            get => _voiceDefs;
        }
        private List<VoiceDef> _voiceDefs = new List<VoiceDef>();
    }
}
