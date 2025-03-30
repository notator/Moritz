using Krystals5ObjectLibrary;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Moritz.Spec
{
    public class Bar
    {
        /// <summary>
        /// This constructor creates the Seq.ChannelDefs field containing a list of ChannelDef objects.
        /// Each ChannelDef has a unique midi channel that is its index in the ChannelDefs list.
        /// Each ChannelDef contains a list of Trk. Each such Trk has the same channel, and is an alternative performance
        /// of the graphics that will eventually be associated with the ChannelDef's graphics. 
        /// </summary>
        /// <param name="absMsPosition">Must be greater than or equal to zero.</param>
        /// <param name="trks">Each Trk must have a constructed UniqueDefs list which is either empty, or contains any
        /// combination of RestDef or ChordDef.
        /// Each trk.MsPositionReContainer must be 0. All trk.UniqueDef.MsPositionReFirstUD values must be set correctly.
        /// All Trks that have the same channel must have the same trk.DurationsCount. (They are different performances of the same ChordSymbols.)
        /// <para>Not all the Seq's channels need to be given an explicit Trk in the trks argument. The seq will be given empty
        /// (default) Trks for the channels that are missing.
        /// trks.Count must be less than or equal to numberOfMidiChannels.</para>
        /// </param>
        /// <param name="numberOfMidiChannels">Each Voice has one channel index.</param>
        public Bar(int absMsPosition, List<ChannelDef> channelDefs)
        {
            #region conditions
            Debug.Assert(absMsPosition >= 0);
            Debug.Assert(channelDefs != null && channelDefs.Count <= 16);
            foreach(var channelDef in ChannelDefs)
            {
                foreach(var trk in channelDef.Trks)
                {
                    trk.AssertConsistency();
                }
            }
            #endregion conditions

            _absMsPosition = absMsPosition;

            AssertConsistency();
        }

        public Bar Clone()
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

            Bar clone = new Bar(_absMsPosition, newMidiChannelDefs);

            return clone;
        }

        #region copied from MainBar (now deleted)
        /// Converts this Bar to a list of bars, consuming this bar's channelDefs.
        /// Uses the argument barline msPositions as the EndBarlines of the returned bars (which don't contain barlines).
        /// An exception is thrown if:
        ///    1) the first argument value is less than or equal to 0.
        ///    2) the argument contains duplicate msPositions.
        ///    3) the argument is not in ascending order.
        ///    4) a Trk.MsPositionReContainer is not 0.
        ///    5) an msPosition is not the endMsPosition of any IUniqueDef in the seq.
        public List<Bar> GetBars(List<int> barlineMsPositionsReTrk0)
        {
            CheckBarlineMsPositionsReTrk0(barlineMsPositionsReTrk0);
            AssertConsistency();

            List<int> barMsDurations = new List<int>();
            int startMsPos = 0;
            for(int i = 0; i < barlineMsPositionsReTrk0.Count; i++)
            {
                int endMsPos = barlineMsPositionsReTrk0[i];
                barMsDurations.Add(endMsPos - startMsPos);
                startMsPos = endMsPos;
            }

            List<Bar> bars = new List<Bar>();
            int totalDurationBeforePop = Trks0[0].MsDuration;  // all Trks0 have the same duration
            Bar remainingBar = (Bar)this;
            foreach(int barMsDuration in barMsDurations)
            {
                Tuple<Bar, Bar> rTuple = PopBar(remainingBar, barMsDuration);

                Bar poppedBar = rTuple.Item1;
                remainingBar = rTuple.Item2; // null after the last pop.

                int poppedMsDuration = poppedBar.Trks0[0].MsDuration;
                int remainingMsDuration = remainingBar.Trks0[0].MsDuration;
                Debug.Assert(poppedMsDuration == barMsDuration);
                if(remainingBar != null)
                {
                    Debug.Assert(poppedMsDuration + remainingMsDuration == totalDurationBeforePop);
                    totalDurationBeforePop = remainingMsDuration;
                }
                else
                {
                    Debug.Assert(poppedMsDuration == totalDurationBeforePop);
                }

                bars.Add(poppedBar);
            }

            return bars;
        }

        /// <summary>
        /// Returns a Tuple in which Item1 is the popped bar, Item2 is the remaining part of the input bar.
        /// Note that Trks at the same level inside each ChannelDef in each bar have the same duration.
        /// and that all Trks in a ChannelDef have the same sequence of ChordDef and RestDef (and no ClefDefs).
        /// </summary>
        /// <param name ="bar">The bar fron which Item1 is popped.</param>
        /// <param name="poppedBarMsDuration">The duration of the first Trk in each ChannelDef in the popped bar.</param>
        /// <returns>The popped bar and the remaining part of the input bar</returns>
        private Tuple<Bar, Bar> PopBar(Bar bar, int poppedBarMsDuration)
        {
            Debug.Assert(poppedBarMsDuration > 0);

            if(poppedBarMsDuration == bar.ChannelDefs[0].Trks[0].MsDuration)
            {
                return new Tuple<Bar, Bar>(bar, null);
            }

            int poppedAbsMsPosition = bar.AbsMsPosition;
            int remainingAbsMsPosition = poppedAbsMsPosition + poppedBarMsDuration;

            List<ChannelDef> poppedChannelDefs = new List<ChannelDef>();
            List<ChannelDef> remainingChannelDefs = new List<ChannelDef>();

            foreach(ChannelDef channelDef in bar.ChannelDefs)
            {
                Tuple<ChannelDef, ChannelDef> channelDefs = channelDef.PopChannelDef(poppedBarMsDuration);

                poppedChannelDefs.Add(channelDefs.Item1);
                remainingChannelDefs.Add(channelDefs.Item2);
            }

            var poppedBar = new Bar(poppedAbsMsPosition, poppedChannelDefs);
            var remainingBar = new Bar(remainingAbsMsPosition, remainingChannelDefs);

            return new Tuple<Bar, Bar>(poppedBar, remainingBar);
        }

        /// <summary>
        /// An exception is thrown if:
        ///    1) the first argument value is less than or equal to 0.
        ///    2) the argument contains duplicate msPositions.
        ///    3) the argument is not in ascending order.
        ///    4) a Trk0.MsPositionReContainer is not 0.
        ///    5) an msPosition is not the endMsPosition of any IUniqueDef in the bar.
        /// </summary>
        private void CheckBarlineMsPositionsReTrk0(IReadOnlyList<int> barlineMsPositionsReTrk0)
        {
            Debug.Assert(barlineMsPositionsReTrk0[0] > 0, "The first msPosition must be greater than 0.");

            for(int i = 0; i < barlineMsPositionsReTrk0.Count; ++i)
            {
                int msPosition = barlineMsPositionsReTrk0[i];
                for(int j = i + 1; j < barlineMsPositionsReTrk0.Count; ++j)
                {
                    Debug.Assert(msPosition != barlineMsPositionsReTrk0[j], "Error: Duplicate barline msPositions.");
                }
            }

            int currentMsPos = -1;
            List<Trk> trks0 = Trks0;

            foreach(int msPosition in barlineMsPositionsReTrk0)
            {
                Debug.Assert(msPosition > currentMsPos, "Value out of order.");
                currentMsPos = msPosition;
                bool found = false;
                foreach(var trk in trks0)
                {
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
                Debug.Assert(found, "Error: barline must be at the endMsPosition of at least one IUniqueDef.");
            }
        }
        #endregion

        #region old Bar

        public void Concat(Bar bar2)
        {
            Debug.Assert(ChannelDefs.Count == bar2.ChannelDefs.Count);

            for(int i = 0; i < ChannelDefs.Count; ++i)
            {
                ChannelDef channelDef1 = ChannelDefs[i];
                ChannelDef channelDef2 = bar2.ChannelDefs[i];

                Debug.Assert(channelDef1.Trks.Count == channelDef2.Trks.Count);

                for(int j = 0; j < channelDef1.Trks.Count; ++j)
                {
                    Trk trk1 = channelDef1.Trks[j];
                    Trk trk2 = channelDef2.Trks[j];

                    trk1.AddRange(trk2);
                    trk1.RemoveDuplicateClefDefs();
                    trk1.AgglomerateRests();
                }
            }

            AssertConsistency();
        }

        #region envelopes
        /// <summary>
        /// Warps the durations in the Trks at trkIndex in all ChannelDefs.
        /// This function does not change the MsDuration of the Trks.
        /// See Envelope.TimeWarp() for a description of the arguments.
        /// </summary>
        /// <param name="envelope"></param>
        /// <param name="distortion"></param>
        public void TimeWarp(int trkIndex, Envelope envelope, double distortion)
        {
            AssertConsistency();
            foreach(var channelDef in ChannelDefs)
            {
                var trk = channelDef.Trks[trkIndex];
                int originalMsDuration = trk.MsDuration;
                List<int> originalMsPositions = trk.GetMsPositions();
                Dictionary<int, int> warpDict = new Dictionary<int, int>();
                #region get warpDict
                List<int> newMsPositions = envelope.TimeWarp(originalMsPositions, distortion);
                for(int i = 0; i < newMsPositions.Count; ++i)
                {
                    warpDict.Add(originalMsPositions[i], newMsPositions[i]);
                }
                #endregion get warpDict
                List<IUniqueDef> iuds = trk.UniqueDefs;
                IUniqueDef iud;
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

                Debug.Assert(originalMsDuration == trk.MsDuration);
            }
            AssertConsistency();
        }

        /// <summary>
        /// Returns a flat list containing
        /// the unique msPositions of IUniqueDefs in Track 0 of all ChannelDefs,
        /// plus the endMsPosition of the final object.
        /// </summary>
        private List<int> GetTrk0MsPositions()
        {
            List<int> originalMsPositions = new List<int>();
            foreach(var channelDef in ChannelDefs)
            {
                var trk = channelDef.Trks[0];
                int originalMsDuration = trk.MsDuration;                
                foreach(IUniqueDef iud in trk.UniqueDefs)
                {
                    int msPos = iud.MsPositionReFirstUD;
                    if(!originalMsPositions.Contains(msPos))
                    {
                        originalMsPositions.Add(msPos);
                    }
                }
                originalMsPositions.Add(originalMsDuration);
            }
            originalMsPositions.Sort();
            return originalMsPositions;
        }

        #endregion envelopes

        public void SetMsPositionsReFirstUD()
        {
            foreach(ChannelDef channelDef in ChannelDefs)
            {
                foreach(var trk in channelDef.Trks)
                {
                    int msPosition = 0;
                    foreach(IUniqueDef iud in trk.UniqueDefs)
                    {
                        iud.MsPositionReFirstUD = msPosition;
                        msPosition += iud.MsDuration;
                    }
                }
            }
        }

        public int AbsMsPosition
        {
            get { return _absMsPosition; }
            set
            {
                Debug.Assert(value >= 0);
                _absMsPosition = value;
            }
        }

        private int _absMsPosition = 0;

        /// <summary>
        /// returns _all_ the trks in the Bar as a flat list.
        /// </summary>
        public IReadOnlyList<Trk> Trks
        {
            get
            {
                List<Trk> trks = new List<Trk>();
                foreach(ChannelDef channelDef in ChannelDefs)
                {
                    foreach(var trk in channelDef.Trks)
                    {
                        trks.Add(trk);
                    }
                }
                return trks.AsReadOnly();
            }
        }

        public override string ToString()
        {
            return $"AbsMsPosition={AbsMsPosition}, nChannelDefs={ChannelDefs.Count}, nTrksPerChannel={ChannelDefs[0].Trks.Count}";
        }

        public IReadOnlyList<ChannelDef> ChannelDefs { get => _channelDefs; }
        private List<ChannelDef> _channelDefs = new List<ChannelDef>();

        /// A list containing only the top Trk in each ChannelDef
        public List<Trk> Trks0
        {
            get 
            {
                List<Trk> trks0 = new List<Trk>();
                foreach(var channelDef in ChannelDefs)
                {
                    trks0.Add(channelDef.Trks[0]);
                }
                return trks0;
            }
        }

        #endregion old Bar

        #region copied from Seq (now deleted)

        /// <summary>
        /// Set empty Trks to contain a single RestDef having the msDuration of the other Trks at the same trkIndex.
        /// </summary>
        public void PadEmptyTrks()
        {
            for(var trkIndex = 0; trkIndex < ChannelDefs[0].Trks.Count; trkIndex++)
            {
                int msDuration = 0;
                foreach(var channelDef in ChannelDefs)
                {
                    var trk = channelDef.Trks[trkIndex];
                    if(trk.UniqueDefs.Count > 0)
                    {
                        msDuration = trk.MsDuration;
                        break;
                    }
                }
                Debug.Assert(msDuration > 0);
                foreach(var channelDef in ChannelDefs)
                {
                    var trk = channelDef.Trks[trkIndex];
                    if(trk.UniqueDefs.Count == 0)
                    {
                        trk.Add(new RestDef(0, msDuration));
                    }
                }
            }
        }

        /// <summary>
        /// AbsMsPosition is greater than or equal 0.
        /// There is at least one ChannelDef in ChannelDefs and at least one Trk in each ChannelDef.
        /// Trk.AssertConsistency(containsClefDefs) is called on all Trks.
        /// All ChannelDef.Trks.Count values are the same.
        /// All ChannelDef.MsPositionReContainer values are 0.
        /// All Trks have the same number of events as the Trk at index 0 in the same ChannelDef.
        /// All Trks having the same index in any ChannelDef
        ///     1. have the same MsDuration.
        ///     2. have the same channel event sequence as the channel events at ChannelDefs[0].Trks[0]
        /// if finalised is false, all Trks can only contain RestDef or ChordDef objects.
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
                for(int i = 0; i < trkChannelIndicesSequence.Count; ++i)
                {
                    List<int> channelIndices0 = trk0ChannelEventSequence[i];
                    List<int> channelIndices1 = trkChannelIndicesSequence[i];

                    Debug.Assert(channelIndices0.SequenceEqual(channelIndices1));
                }
            }
        }

        public int MsDuration { get => ChannelDefs[0].Trks[0].MsDuration; }

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
                var mPos1 = channelIndicesPosSequence[i - 1].Item2;
                if(mPos2 == mPos1)
                {
                    List<int> channelIndices = channelIndicesPosSequence[i].Item1;
                    channelIndicesPosSequence[i - 1].Item1.AddRange(channelIndices);
                    channelIndicesPosSequence[i - 1].Item1.Sort();
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

        #endregion Envelopes
        #endregion
    }
}
