using Krystals5ObjectLibrary;

using System.Collections.Generic;
using System.Diagnostics;

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
        /// combination of RestDef or MidiChordDef.
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
            Debug.Assert(channelDefs != null && channelDefs.Count > 0 && channelDefs.Count <= 16);
            Debug.Assert(ChannelDefs[0].Trks != null && ChannelDefs[0].Trks.Count > 0);
            foreach(var channelDef in channelDefs)
            {
                foreach(var trk in channelDef.Trks)
                {
                    trk.AssertConsistency();
                }
            }
            #endregion conditions

            _absMsPosition = absMsPosition;
            _channelDefs = channelDefs;

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
        /// AbsMsPosition is greater than or equal to 0.
        /// There is at least one ChannelDef in ChannelDefs and at least one Trk in each ChannelDef.
        /// Trk.AssertConsistency() is called on all Trks.
        /// All ChannelDef.Trks.Count values are the same.
        /// All ChannelDef.MsPositionReContainer values are 0.
        /// All Trks having the same index in any ChannelDef in this Bar have the same msDuration.
        /// In each ChannelDef:
        ///     1. All Trks have the same number of DurationDefs
        ///     2. In each Trk, DurationDefs at the same index in trk.UniqueDefs are of the same type.
        /// Performance consistency:
        /// (A "performance" consists of all the trks at the same index within their ChannelDef.)
        /// The overall sequence of events (DurationDefs) must be identical in all performances.
        /// </summary>
        public virtual void AssertConsistency()
        {
            Debug.Assert(AbsMsPosition >= 0);
            Debug.Assert(ChannelDefs.Count > 0);
            var trksCount = ChannelDefs[0].Trks.Count;
            Debug.Assert(trksCount > 0);

            int nTrks = ChannelDefs[0].Trks.Count;
            Debug.Assert(nTrks > 0);

            CheckChannelDefConsistency(nTrks);

            CheckPerformanceConsistency(nTrks);
        }

        private void CheckChannelDefConsistency(int nTrks)
        {
            foreach(var channelDef in ChannelDefs)
            {
                Debug.Assert(channelDef.Trks.Count == nTrks);
                Debug.Assert(channelDef.MsPositionReContainer == 0);
                foreach(var trk in channelDef.Trks)
                {
                    trk.AssertConsistency();
                }

                // All Trks having the same index in any ChannelDef in this Bar have the same msDuration.
                for(int trkIndex = 0; trkIndex < nTrks; ++trkIndex)
                {
                    int barMsDuration = ChannelDefs[0].Trks[trkIndex].MsDuration;
                    for(int channelDefIndex = 1; channelDefIndex < ChannelDefs.Count; ++channelDefIndex)
                    {
                        var cDef = ChannelDefs[channelDefIndex];
                        Debug.Assert(cDef.Trks[trkIndex].MsDuration == barMsDuration);
                    }
                }

                // In each ChannelDef:
                //     1. All Trks have the same number of DurationDefs
                //     2. In each Trk, DurationDefs at the same index in trk.UniqueDefs are of the same type.
                List<DurationDef> trk0DurationDefs = channelDef.Trks[0].DurationDefs;
                for(int trkIndex = 1; trkIndex < nTrks; ++trkIndex)
                {
                    List<DurationDef> trkDDs = channelDef.Trks[trkIndex].DurationDefs;
                    Debug.Assert(trkDDs.Count == trk0DurationDefs.Count);
                    for(var ddIndex = 0; ddIndex < trk0DurationDefs.Count; ++ddIndex)
                    {
                        DurationDef dd0 = trk0DurationDefs[ddIndex]; // durationDef in Trk 0
                        DurationDef trkDD = trkDDs[ddIndex];
                        Debug.Assert((dd0 is MidiChordDef && trkDD is MidiChordDef) | (dd0 is RestDef && trkDD is RestDef));
                    }
                }
            }
        }

        private void CheckPerformanceConsistency(int nTrks)
        {
            /// A "performance" consists of all the trks at the same index within their ChannelDef.
            /// The overall sequence of events (DurationDefs) must be identical in all performances.
            List<List<DurationDef>> performances = new List<List<DurationDef>>();
            for(var trkIndex = 0; trkIndex < nTrks; ++trkIndex)
            {
                List<DurationDef> performance = new List<DurationDef>();
                foreach(var channelDef in ChannelDefs)
                {
                    performance.AddRange(channelDef.Trks[trkIndex].DurationDefs);
                }
                performance.Sort((x, y) => x.MsPositionReFirstUD.CompareTo(y.MsPositionReFirstUD));
                performances.Add(performance);
            }
            int nDurationDefs = performances[0].Count;
            for(int pIndex = 1; pIndex < nTrks; ++pIndex)
            {
                Debug.Assert(performances[pIndex - 1].Count == nDurationDefs);
                for(int i = 0; i < nDurationDefs; ++i)
                {
                    DurationDef dd0 = performances[pIndex - 1][i];
                    DurationDef dd1 = performances[pIndex][i];
                    Debug.Assert((dd0 is MidiChordDef && dd1 is MidiChordDef) | (dd0 is RestDef && dd1 is RestDef));
                }
            }
        }

        public int MsDuration { get => ChannelDefs[0].Trks[0].MsDuration; }

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
