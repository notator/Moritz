using Krystals5ObjectLibrary;

using Moritz.Globals;

using System.Collections.Generic;
using System.Diagnostics;

namespace Moritz.Spec
{
    public class Bar
    {
        /// <summary>
        /// This constructor creates the VoiceDefs field containing a list of VoiceDef objects.
        /// Each VoiceDef has a unique midi channel that is its index in the VoiceDefs list.
        /// Each VoiceDef contains a list of Trk. Each such Trk has the same channel, and is an alternative performance
        /// of the graphics that will eventually be associated with the VoiceDef's graphics. 
        /// </summary>
        /// <param name="absMsPosition">Must be greater than or equal to zero.</param>
        /// <param name="voiceDefs">Each voiceDef has a Trks property containing at least one Trk (performance)
        /// Each Trk must have a constructed UniqueDefs list which is either empty, or contains any combination of RestDef or MidiChordDef.
        /// Each trk.MsPositionReContainer must be 0. All trk.UniqueDef.MsPositionReFirstUD values must be set correctly.
        /// All Trks that have the same channel must have the same trk.DurationsCount. (They are different performances of the same ChordSymbols.)
        public Bar(int absMsPosition, List<VoiceDef> voiceDefs)
        {
            #region conditions
            Debug.Assert(absMsPosition >= 0);
            Debug.Assert(voiceDefs != null && voiceDefs.Count > 0 && voiceDefs.Count <= 16);
            Debug.Assert(voiceDefs[0].Trks != null && voiceDefs[0].Trks.Count > 0);
            foreach(var voiceDef in voiceDefs)
            {
                foreach(var trk in voiceDef.Trks)
                {
                    trk.AssertConsistency();
                }
            }
            #endregion conditions

            _absMsPosition = absMsPosition;
            _voiceDefs = voiceDefs;

            AssertConsistency();
        }

        public Bar Clone()
        {
            var newMidiVoiceDefs = new List<VoiceDef>();

            foreach(var oldMidiVoiceDef in this.VoiceDefs)
            {
                List<Trk> newTrks = new List<Trk>();
                foreach(var oldTrk in oldMidiVoiceDef.Trks)
                {
                    newTrks.Add((Trk)oldTrk.Clone());
                }
                newMidiVoiceDefs.Add(new VoiceDef(newTrks));
            }

            Bar clone = new Bar(_absMsPosition, newMidiVoiceDefs);

            return clone;
        }

        #region old Bar

        public void Concat(Bar bar2)
        {
            Debug.Assert(VoiceDefs.Count == bar2.VoiceDefs.Count);

            for(int i = 0; i < VoiceDefs.Count; ++i)
            {
                VoiceDef voiceDef1 = VoiceDefs[i];
                VoiceDef voiceDef2 = bar2.VoiceDefs[i];

                Debug.Assert(voiceDef1.Trks.Count == voiceDef2.Trks.Count);

                for(int j = 0; j < voiceDef1.Trks.Count; ++j)
                {
                    Trk trk1 = voiceDef1.Trks[j];
                    Trk trk2 = voiceDef2.Trks[j];

                    trk1.AddRange(trk2);
                    trk1.RemoveDuplicateClefDefs();
                    trk1.AgglomerateRests();
                }
            }

            AssertConsistency();
        }

        #region envelopes
        /// <summary>
        /// Warps the durations in the Trks at trkIndex in all VoiceDefs.
        /// This function does not change the MsDuration of the Trks.
        /// See Envelope.TimeWarp() for a description of the arguments.
        /// </summary>
        /// <param name="envelope"></param>
        /// <param name="distortion"></param>
        public void TimeWarp(int trkIndex, Envelope envelope, double distortion)
        {
            AssertConsistency();
            foreach(var voiceDef in VoiceDefs)
            {
                var trk = voiceDef.Trks[trkIndex];
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
        /// the unique msPositions of IUniqueDefs in Track 0 of all VoiceDefs,
        /// plus the endMsPosition of the final object.
        /// </summary>
        private List<int> GetTrk0MsPositions()
        {
            List<int> originalMsPositions = new List<int>();
            foreach(var voiceDef in VoiceDefs)
            {
                var trk = voiceDef.Trks[0];
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
            foreach(VoiceDef voiceDef in VoiceDefs)
            {
                foreach(var trk in voiceDef.Trks)
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

        public override string ToString()
        {
            return $"AbsMsPosition={AbsMsPosition}, nVoiceDefs={VoiceDefs.Count}, nTrksPerVoice={VoiceDefs[0].Trks.Count}";
        }

        public IReadOnlyList<VoiceDef> VoiceDefs { get => _voiceDefs; }
        private List<VoiceDef> _voiceDefs = new List<VoiceDef>();

        /// A list containing only the top Trk in each VoiceDef
        public List<Trk> Trks0
        {
            get 
            {
                List<Trk> trks0 = new List<Trk>();
                foreach(var voiceDef in VoiceDefs)
                {
                    trks0.Add(voiceDef.Trks[0]);
                }
                return trks0;
            }
        }

        #endregion old Bar

        #region copied from Seq (now deleted)

        /// <summary>
        /// AbsMsPosition is greater than or equal to 0.
        /// There is at least one VoiceDef in VoiceDefs and at least one Trk in each VoiceDef.
        /// Trk.AssertConsistency() is called on all Trks.
        /// All VoiceDef.Trks.Count values are the same.
        /// All VoiceDef.MsPositionReContainer values are 0.
        /// All Trks having the same index in any VoiceDef in this Bar have the same msDuration.
        /// In each VoiceDef:
        ///     1. All Trks have the same number of DurationDefs
        ///     2. In each Trk, DurationDefs at the same index in trk.UniqueDefs are of the same type.
        /// Performance consistency:
        /// (A "performance" consists of all the trks at the same index within their VoiceDef.)
        /// The overall sequence of events (DurationDefs) must be identical in all performances.
        /// </summary>
        public virtual void AssertConsistency()
        {
            Debug.Assert(AbsMsPosition >= 0);
            Debug.Assert(VoiceDefs.Count > 0);
            var trksCount = VoiceDefs[0].Trks.Count;
            Debug.Assert(trksCount > 0);

            int nTrks = VoiceDefs[0].Trks.Count;
            Debug.Assert(nTrks > 0);

            CheckVoiceDefConsistency(nTrks);

            CheckPerformanceConsistency(nTrks);
        }

        private void CheckVoiceDefConsistency(int nTrks)
        {
            foreach(var voiceDef in VoiceDefs)
            {
                Debug.Assert(voiceDef.Trks.Count == nTrks);
                Debug.Assert(voiceDef.MsPositionReContainer == 0);
                foreach(var trk in voiceDef.Trks)
                {
                    trk.AssertConsistency();
                }

                // All Trks having the same index in any VoiceDef in this Bar have the same msDuration.
                for(int trkIndex = 0; trkIndex < nTrks; ++trkIndex)
                {
                    int barMsDuration = VoiceDefs[0].Trks[trkIndex].MsDuration;
                    for(int voiceDefIndex = 1; voiceDefIndex < VoiceDefs.Count; ++voiceDefIndex)
                    {
                        var cDef = VoiceDefs[voiceDefIndex];
                        Debug.Assert(cDef.Trks[trkIndex].MsDuration == barMsDuration);
                    }
                }

                // In each VoiceDef:
                //     1. All Trks have the same number of DurationDefs
                //     2. In each Trk, DurationDefs at the same index in trk.UniqueDefs are of the same type.
                List<DurationDef> trk0DurationDefs = voiceDef.Trks[0].DurationDefs;
                for(int trkIndex = 1; trkIndex < nTrks; ++trkIndex)
                {
                    List<DurationDef> trkDDs = voiceDef.Trks[trkIndex].DurationDefs;
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
            /// A "performance" consists of all the trks at the same index within their VoiceDef.
            /// The overall sequence of events (DurationDefs) must be identical in all performances.
            List<List<DurationDef>> performances = new List<List<DurationDef>>();
            for(var trkIndex = 0; trkIndex < nTrks; ++trkIndex)
            {
                List<DurationDef> performance = new List<DurationDef>();
                foreach(var voiceDef in VoiceDefs)
                {
                    performance.AddRange(voiceDef.Trks[trkIndex].DurationDefs);
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

        public int MsDuration { get => VoiceDefs[0].Trks[0].MsDuration; }

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

        #endregion Envelopes
        #endregion
    }
}
