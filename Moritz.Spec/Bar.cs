using Krystals5ObjectLibrary;

using Moritz.Globals;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml.XPath;

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
        /// <param name="trks">There is one Trk per channel (A Trk's channel is the index in this list.).
        /// Each Trk must have a constructed UniqueDefs list which is either empty, or contains any combination of MidiChordDef or RestDef.
        /// Each trk.MsPositionReContainer must be 0. All trk.UniqueDef.MsPositionReFirstUD values must be set correctly.
        /// each MidiChordDef and RestDef has a MidiDef property containing a list of the alternative interpretations of the symbol. 
        public Bar(int absMsPosition, List<Trk> trks)
        {
            #region conditions
            Debug.Assert(absMsPosition >= 0);
            Debug.Assert(trks != null && trks.Count > 0 && trks.Count <= 16);
            foreach(var trk in trks)
            {
                trk.AssertConsistency();
            }
            #endregion conditions

            AbsMsPosition = absMsPosition;
            Trks = trks;

            AssertConsistency();
        }

        /// Converts this Bar to a list of bars, consuming this bar's trks.
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
            int totalDurationBeforePop = Trks[0].MsDuration;  // all default interpretations have the same duration
            Bar remainingBar = (Bar)this;
            foreach(int barMsDuration in barMsDurations)
            {
                Tuple<Bar, Bar> rTuple = PopBar(remainingBar, barMsDuration);

                Bar poppedBar = rTuple.Item1;
                remainingBar = rTuple.Item2; // null after the last pop.

                int poppedMsDuration = poppedBar.Trks[0].MsDuration;
                if(remainingBar != null)
                {
                    int remainingMsDuration = remainingBar.Trks[0].MsDuration;
                    Debug.Assert(poppedMsDuration == barMsDuration);

                    Debug.Assert(poppedMsDuration + remainingMsDuration == totalDurationBeforePop);
                    totalDurationBeforePop = remainingMsDuration;
                }
                else
                {
                    Debug.Assert(poppedMsDuration == totalDurationBeforePop);
                }

                bars.Add(poppedBar);
            }

            foreach(var bar in bars)
            {
                // Each Trk can begin with a CautionaryChordDef and contains no ClefDefs.
                bar.AssertConsistency();
            }

            return bars;
        }

        /// <summary>
        /// Returns nBars barlineMsPositions.
        /// The Bars are as equal in duration as possible, with each barline being at the end of at least one IUniqueDef in the top Trk of a VoiceDef.
        /// The returned list contains no duplicates (A M.Assertion fails otherwise).
        /// </summary>
        /// <returns></returns>
        /// <param name="trks"></param>
        /// <param name="inputVoiceDefs">Can be null</param>
        /// <param name="nBars"></param>
        /// <returns></returns>
        public List<int> GetBalancedBarlineMsPositions(IReadOnlyList<Trk> trks, int nBars)
        {
            var msDuration = trks[0].MsDuration;
            double approxBarMsDuration = (((double)msDuration) / nBars);
            Debug.Assert(approxBarMsDuration * nBars == msDuration);

            List<int> barlineMsPositions = new List<int>();

            for(int barNumber = 1; barNumber <= nBars; ++barNumber)
            {
                double approxBarMsPosition = approxBarMsDuration * barNumber;
                int barMsPosition = NearestAbsUIDEndMsPosition(trks, approxBarMsPosition);

                Debug.Assert(barlineMsPositions.Contains(barMsPosition) == false);

                barlineMsPositions.Add(barMsPosition);
            }
            Debug.Assert(barlineMsPositions[barlineMsPositions.Count - 1] == msDuration);

            return barlineMsPositions;
        }

        /// <summary>
        /// Barlines will be fit to MidiChordDefs in the trks.
        /// </summary>
        /// <param name="mainSeq"></param>
        /// <param name="inputVoiceDefs">can be null</param>
        /// <param name="approximateBarlineMsPositions"></param>
        /// <returns></returns>
        protected List<int> GetBarlinePositions(IReadOnlyList<Trk> trks0, List<double> approximateBarlineMsPositions)
        {
            List<int> barlineMsPositions = new List<int>();

            foreach(double approxMsPos in approximateBarlineMsPositions)
            {
                int barlineMsPos = NearestAbsUIDEndMsPosition(trks0, approxMsPos);

                barlineMsPositions.Add(barlineMsPos);
            }
            return barlineMsPositions;
        }

        private int NearestAbsUIDEndMsPosition(IReadOnlyList<Trk> trks, double approxAbsMsPosition)
        {
            int nearestAbsUIDEndMsPosition = 0;
            double diff = double.MaxValue;
            foreach(var trk in trks)
            {
                for(int uidIndex = 0; uidIndex < trk.UniqueDefs.Count; ++uidIndex)
                {
                    IUniqueDef iud = trk[uidIndex];
                    int absEndPos = iud.MsPositionReFirstUD + iud.MsDuration;
                    double localDiff = Math.Abs(approxAbsMsPosition - absEndPos);
                    if(localDiff < diff)
                    {
                        diff = localDiff;
                        nearestAbsUIDEndMsPosition = absEndPos;
                    }
                    if(diff == 0)
                    {
                        break;
                    }
                }
                if(diff == 0)
                {
                    break;
                }
            }
            return nearestAbsUIDEndMsPosition;
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

            foreach(int msPosition in barlineMsPositionsReTrk0)
            {
                Debug.Assert(msPosition > currentMsPos, "Value out of order.");
                currentMsPos = msPosition;
                bool found = false;
                foreach(var trk in Trks)
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

        /// <summary>
        /// Returns a Tuple in which Item1 is the popped bar, Item2 is the remaining part of the input bar.
        /// Note that Trks at the same level inside each VoiceDef in each bar have the same duration.
        /// and that all Trks in a VoiceDef have the same sequence of MidiChordDef and RestDef (and no ClefDefs).
        /// </summary>
        /// <param name ="bar">The bar fron which Item1 is popped.</param>
        /// <param name="poppedBarMsDuration">The duration of the first Trk in each VoiceDef in the popped bar.</param>
        /// <returns>The popped bar and the remaining part of the input bar</returns>
        public Tuple<Bar, Bar> PopBar(Bar bar, int poppedBarMsDuration)
        {
            Debug.Assert(poppedBarMsDuration > 0);

            if(poppedBarMsDuration == bar.Trks[0].MsDuration)
            {
                return new Tuple<Bar, Bar>(bar, null);
            }

            int poppedAbsMsPosition = bar.AbsMsPosition;
            int remainingAbsMsPosition = poppedAbsMsPosition + poppedBarMsDuration;

            List<Trk> poppedTrks = new List<Trk>();
            List<Trk> remainingTrks = new List<Trk>();

            foreach(Trk trk in bar.Trks)
            {
                Tuple<Trk, Trk, double?> popTrk = trk.PopTrk(poppedBarMsDuration);

                poppedTrks.Add(popTrk.Item1);
                remainingTrks.Add(popTrk.Item2);
            }

            JustifyMsDurations(poppedTrks);
            JustifyMsDurations(remainingTrks);

            var poppedBar = new Bar(poppedAbsMsPosition, poppedTrks);
            var remainingBar = new Bar(remainingAbsMsPosition, remainingTrks);

            return new Tuple<Bar, Bar>(poppedBar, remainingBar);
        }

        private void JustifyMsDurations(List<Trk> trks)
        {
            int topTrkMsDuration = trks[0].MsDuration;
            for(int trkIndex = 1; trkIndex < trks.Count; trkIndex++)
            {
                trks[trkIndex].MsDuration = topTrkMsDuration;
            }
        }

        #region old Bar

        public void Concat(Bar bar2)
        {
            Debug.Assert(Trks.Count == bar2.Trks.Count);

            for(int i = 0; i < Trks.Count; ++i)
            {
                    var trk1 = Trks[i];
                    var trk2 = bar2.Trks[i];    

                    trk1.AddRange(trk2);
                    trk1.RemoveDuplicateClefDefs();
                    trk1.AgglomerateRests();
            }

            AssertConsistency();
        }

        #region envelopes
        /// <summary>
        /// Warps the durations of a particular interpretation of the Bar without
        /// changing its overall MsDuration.
        /// See Envelope.TimeWarp() for a description of the arguments.
        /// </summary>
        /// <param name="envelope"></param>
        /// <param name="distortion"></param>
        public void TimeWarp(int interpretationIndex, Envelope envelope, double distortion)
        {
            AssertConsistency();
            for(int trkIndex = 0; trkIndex < Trks.Count; trkIndex++)
            {
                var trk = GetInterpretation(trkIndex, interpretationIndex);
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
        /// Returns a flat list containing the unique msPositions of the IUniqueDefs of the main
        /// interpretation of in all Trks in the Bar, plus the endMsPosition of the final object.
        /// </summary>
        private List<int> GetAllMsPositions()
        {
            List<int> originalMsPositions = new List<int>();
            int originalMsDuration = MsDuration;

            foreach(var trk in Trks)
            {                                 
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
            foreach(Trk trk in Trks)
            {
                    int msPosition = 0;
                    foreach(IUniqueDef iud in trk.UniqueDefs)
                    {
                        iud.MsPositionReFirstUD = msPosition;
                        msPosition += iud.MsDuration;
                    }
            }
        }

        public override string ToString()
        {
            return $"AbsMsPosition={AbsMsPosition}, nTrks={Trks.Count}, nInterpretations={Trks[0].InterpretationsCount}";
        }

        public int AbsMsPosition { get; private set; }
        public IReadOnlyList<Trk> Trks { get; private set; } = new List<Trk>();

        #endregion old Bar

        #region copied from Seq (now deleted)

        /// <summary>
        /// AbsMsPosition is greater than or equal to 0.
        /// There is at least one Trk in Trks.
        /// </summary>
        public virtual void AssertConsistency()
        {
            Debug.Assert(AbsMsPosition >= 0);
            Debug.Assert(Trks.Count > 0);
        }

        protected Trk GetInterpretation(int trkIndex, int interpIndex)
        {
            Trk returnTrk = new Trk();
            var mainUniqueDefs = Trks[trkIndex].UniqueDefs;
            for(int i = 0; i < mainUniqueDefs.Count; ++i)
            {
                var uniqueDef = mainUniqueDefs[i];
                if(uniqueDef is MidiChordDef mcd)
                {               
                    returnTrk.Add(mcd.MidiDefs[interpIndex]);
                }
                else if(uniqueDef is RestDef restDef)
                {
                    returnTrk.Add(restDef.MidiDefs[interpIndex]);
                }
            }
            return returnTrk;
        }

        public int MsDuration { get => Trks[0].MsDuration; }

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

            foreach(Trk trk in Trks)
            {
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
            }

            Debug.Assert(originalMsDuration == MsDuration);

            AssertConsistency();
        }


        /// <summary>
        /// returns a list containing the msPositions of all the IUniqueDefs in the default interpretation of all Trks
        /// plus the endMsPosition of the final object.
        /// </summary>
        /// <returns></returns>
        private List<int> GetMsPositions()
        {
            int originalMsDuration = MsDuration;

            List<int> originalMsPositions = new List<int>();
            foreach(Trk trk in Trks)
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
            originalMsPositions.Sort();
            originalMsPositions.Add(originalMsDuration);

            return originalMsPositions;
        }

        #endregion Envelopes
        #endregion
    }
}
