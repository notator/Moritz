using Krystals5ObjectLibrary;

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Moritz.Spec
{
    public class Bar : IChannelDefsContainer
    {
        public Bar()
        {
        }

        /// <summary>
        /// <para>A Bar contains a list of ChannelDef. Each ChannelDef contains a list of Trk objects.
        /// Bars do not contain barlines. They are implicit, at the beginning and end of the Bar.</para>
        /// <para>Seq.AssertConsistency() must succeed (see its definition).</para>
        /// <para>The Bar's AbsMsPosition is set to the seq's AbsMsPosition.</para>
        /// <para>When complete, this constructor calls the bar.AssertConsistency() function (see that its documentation).</para>
        /// </summary>
        /// <param name="seq">Cannot be null, and must have Trks</param>
        public Bar(Seq seq)
        {
            #region conditions
            seq.AssertConsistency();
            #endregion

            AbsMsPosition = seq.AbsMsPosition;

            int channelIndex = 0;
            foreach(var channelDef in seq.ChannelDefs)
            {
                foreach(var trk in channelDef.Trks)
                {
                    trk.ChannelDefsContainer = this;
                }

                _channelDefs.Add(channelDef);
                channelIndex++;
                
            }

            AssertConsistency();
        }

        #region copied from MainBar (now deleted)
        /// Converts this MainBar to a list of bars, consuming this bar's channelDefs.
        /// Uses the argument barline msPositions as the EndBarlines of the returned bars (which don't contain barlines).
        /// An exception is thrown if:
        ///    1) the first argument value is less than or equal to 0.
        ///    2) the argument contains duplicate msPositions.
        ///    3) the argument is not in ascending order.
        ///    4) a Trk.MsPositionReContainer is not 0.
        ///    5) an msPosition is not the endMsPosition of any IUniqueDef in the seq.
        public List<Bar> GetBars(List<int> barlineMsPositions)
        {
            CheckBarlineMsPositions(barlineMsPositions);

            List<int> barMsDurations = new List<int>();
            int startMsPos = 0;
            for(int i = 0; i < barlineMsPositions.Count; i++)
            {
                int endMsPos = barlineMsPositions[i];
                barMsDurations.Add(endMsPos - startMsPos);
                startMsPos = endMsPos;
            }

            List<Bar> bars = new List<Bar>();
            int totalDurationBeforePop = this.MsDuration;
            Bar remainingBar = (Bar)this;
            foreach(int barMsDuration in barMsDurations)
            {
                Tuple<Bar, Bar> rTuple = PopBar(remainingBar, barMsDuration);
                Bar poppedBar = rTuple.Item1;
                remainingBar = rTuple.Item2; // null after the last pop.

                Debug.Assert(poppedBar.MsDuration == barMsDuration);
                if(remainingBar != null)
                {
                    Debug.Assert(poppedBar.MsDuration + remainingBar.MsDuration == totalDurationBeforePop);
                    totalDurationBeforePop = remainingBar.MsDuration;
                }
                else
                {
                    Debug.Assert(poppedBar.MsDuration == totalDurationBeforePop);
                }

                bars.Add(poppedBar);
            }

            return bars;
        }

        /// <summary>
        /// Returns a Tuple in which Item1 is the popped bar, Item2 is the remaining part of the input bar.
        /// Note that Trks at the same level inside each ChannelDef in each bar have the same duration.
        /// </summary>
        /// <param name ="bar">The bar fron which Item1 is popped.</param>
        /// <param name="poppedBarMsDuration">The duration of the first Trk in each ChannelDef in the popped bar.</param>
        /// <returns>The popped bar and the remaining part of the input bar</returns>
        private Tuple<Bar, Bar> PopBar(Bar bar, int poppedBarMsDuration)
        {
            Debug.Assert(poppedBarMsDuration > 0);

            if(poppedBarMsDuration == bar.MsDuration)
            {
                return new Tuple<Bar, Bar>(bar, null);
            }

            List<ChannelDef> poppedChannelDefs = new List<ChannelDef>();
            List<ChannelDef> remainingChannelDefs = new List<ChannelDef>();

            foreach(ChannelDef channelDef in bar.ChannelDefs)
            {
                Tuple<ChannelDef, ChannelDef> channelDefs = PopChannelDef(channelDef, poppedBarMsDuration);

                poppedChannelDefs.Add(channelDefs.Item1);
                remainingChannelDefs.Add(channelDefs.Item2);
            }

            var poppedSeq = new Seq(bar.AbsMsPosition, poppedChannelDefs);
            var remainingSeq = new Seq(bar.AbsMsPosition, remainingChannelDefs);

            Bar poppedBar = new Bar(poppedSeq);
            Bar remainingBar = new Bar(remainingSeq);

            return new Tuple<Bar, Bar>(poppedBar, remainingBar);
        }

        /// <summary>
        /// Returns two ChannelDefs (each has the same channel index)
        /// Item1.Trks[0] contains the IUniqueDefs that begin within the poppedMsDuration.
        /// Item2.Trks[0] contains the remaining IUniqueDefs from the original channelDef.Trks.
        /// The remaining Trks in Item1 and Item2 are parallel IUniqueDefs (that can have other durations).
        /// The popped IUniqueDefs are removed from the current channelDef before returning it as Item2.
        /// MidiRestDefs and MidiChordDefs are split as necessary to fit the required Trk[0] duration.
        /// </summary>
        /// <param name="channelDef"></param>
        /// <param name="poppedBarMsDuration"></param>
        /// <returns></returns>
        private Tuple<ChannelDef, ChannelDef> PopChannelDef(ChannelDef channelDef, int poppedMsDuration)
        {
            Tuple<Trk, Trk> trks = PopTrk(channelDef.Trks[0], poppedMsDuration);

            Trk poppedTrk0 = trks.Item1;
            Trk remainingTrk0 = trks.Item2;

            List<Trk> poppedTrks = new List<Trk> { poppedTrk0 };
            List<Trk> remainingTrks = new List<Trk> { remainingTrk0 };

            int nUniqueDefs = poppedTrk0.UniqueDefs.Count;
            List<Trk> channelTrks = channelDef.Trks;

            for(int trkIndex = 1; trkIndex < channelTrks.Count; ++trkIndex)
            {
                Trk poppedTrk = new Trk();
                List<IUniqueDef> originalUids = channelTrks[trkIndex].UniqueDefs;
                for(int uidIndex = 0; uidIndex < nUniqueDefs; ++uidIndex)
                {
                    poppedTrk.UniqueDefs.Add(originalUids[0]);
                    originalUids.RemoveAt(0);
                }
                poppedTrks.Add(poppedTrk);
                remainingTrks.Add(new Trk(0, originalUids));
            }

            ChannelDef poppedChannelDef = new ChannelDef(poppedTrks);
            ChannelDef remainingChannelDef = new ChannelDef(remainingTrks);

            return new Tuple<ChannelDef, ChannelDef>(poppedChannelDef, remainingChannelDef);
        }

        private Tuple<Trk, Trk> PopTrk(Trk trk, int poppedMsDuration)
        {
            Trk poppedTrk = new Trk(trk.MsPositionReContainer, new List<IUniqueDef>());
            Trk remainingTrk = new Trk(trk.MsPositionReContainer + poppedMsDuration, new List<IUniqueDef>());

            foreach(IUniqueDef iud in trk.UniqueDefs)
            {
                int iudMsDuration = iud.MsDuration;
                int iudStartPos = iud.MsPositionReFirstUD;
                int iudEndPos = iudStartPos + iudMsDuration;

                if(iudStartPos >= poppedMsDuration)
                {
                    if(iud is ClefDef && iudStartPos == poppedMsDuration)
                    {
                        poppedTrk.UniqueDefs.Add(iud);
                    }
                    else
                    {
                        remainingTrk.UniqueDefs.Add(iud);
                    }
                }
                else if(iudEndPos > poppedMsDuration)
                {
                    int durationBeforeBarline = poppedMsDuration - iudStartPos;
                    int durationAfterBarline = iudEndPos - poppedMsDuration;
                    if(iud is MidiRestDef)
                    {
                        // This is a rest. Split it.
                        MidiRestDef firstRestHalf = new MidiRestDef(iudStartPos, durationBeforeBarline);
                        poppedTrk.UniqueDefs.Add(firstRestHalf);

                        MidiRestDef secondRestHalf = new MidiRestDef(poppedMsDuration, durationAfterBarline);
                        remainingTrk.UniqueDefs.Add(secondRestHalf);
                    }
                    if(iud is CautionaryChordDef)
                    {
                        Debug.Assert(false, "There shouldnt be any cautionary chords here.");
                        // This error can happen if an attempt is made to set barlines too close together,
                        // i.e. (I think) if an attempt is made to create a bar that contains nothing... 
                    }
                    else if(iud is MidiChordDef)
                    {
                        IUniqueSplittableChordDef uniqueChordDef = iud as IUniqueSplittableChordDef;
                        uniqueChordDef.MsDurationToNextBarline = durationBeforeBarline;
                        poppedTrk.UniqueDefs.Add(uniqueChordDef);

                        Debug.Assert(remainingTrk.UniqueDefs.Count == 0);
                        CautionaryChordDef ccd = new CautionaryChordDef(uniqueChordDef, 0, durationAfterBarline);
                        remainingTrk.UniqueDefs.Add(ccd);
                    }
                }
                else
                {
                    Debug.Assert(iudEndPos <= poppedMsDuration && iudStartPos >= 0);
                    poppedTrk.UniqueDefs.Add(iud);
                }
            }

            return new Tuple<Trk, Trk>(poppedTrk, remainingTrk);
        }

        /// <summary>
        /// An exception is thrown if:
        ///    1) the first argument value is less than or equal to 0.
        ///    2) the argument contains duplicate msPositions.
        ///    3) the argument is not in ascending order.
        ///    4) a ChannelDef.MsPositionReContainer is not 0.
        ///    5) if an msPosition is not the endMsPosition of any IUniqueDef in the Trks.
        /// </summary>
        private void CheckBarlineMsPositions(IReadOnlyList<int> barlineMsPositionsReThisBar)
        {
            Debug.Assert(barlineMsPositionsReThisBar[0] > 0, "The first msPosition must be greater than 0.");

            for(int i = 0; i < barlineMsPositionsReThisBar.Count; ++i)
            {
                int msPosition = barlineMsPositionsReThisBar[i];
                Debug.Assert(msPosition <= this.MsDuration);
                for(int j = i + 1; j < barlineMsPositionsReThisBar.Count; ++j)
                {
                    Debug.Assert(msPosition != barlineMsPositionsReThisBar[j], "Error: Duplicate barline msPositions.");
                }
            }

            int currentMsPos = -1;
            foreach(int msPosition in barlineMsPositionsReThisBar)
            {
                Debug.Assert(msPosition > currentMsPos, "Value out of order.");
                currentMsPos = msPosition;
                bool found = false;
                for(int i = ChannelDefs.Count - 1; i >= 0; --i)
                {
                    ChannelDef channelDef = ChannelDefs[i];

                    Debug.Assert(channelDef.MsPositionReContainer == 0);
                    foreach(IUniqueDef iud in channelDef.UniqueDefs)
                    {
                        if(msPosition == (iud.MsPositionReFirstUD + iud.MsDuration))
                        {
                            found = true;
                            break;
                        }
                    }
                }

                Debug.Assert(found, "Error: barline must be at the endMsPosition of at least one IUniqueDef in a Trk.");
            }
        }
        #endregion

        public void Concat(Bar bar2)
        {
            Debug.Assert(ChannelDefs.Count == bar2.ChannelDefs.Count);

            for(int i = 0; i < ChannelDefs.Count; ++i)
            {
                ChannelDef vd1 = ChannelDefs[i];
                ChannelDef vd2 = bar2.ChannelDefs[i];

                vd1.Container = null;
                vd2.Container = null;
                vd1.AddRange(vd2);
                vd1.RemoveDuplicateClefDefs();
                vd1.AgglomerateRests();
                vd1.Container = this;
            }

            AssertConsistency();
        }


        /// <summary>
        /// Trk.AssertConsistency() is called on each Trk in each ChannelDef.
        /// Then the following checks ae also made:
        /// <para>All channelDefs have the same MsDuration.</para>
        /// <para>At least one Trk must start with a MidiChordDef, possibly preceded by a ClefDef.</para>
        /// </summary> 
        public virtual void AssertConsistency()
        {
            #region trk consistent in bar
            foreach(ChannelDef channelDef in ChannelDefs)
            {
                channelDef.AssertConsistency();
                foreach(Trk trk in channelDef.Trks)
                {
                    trk.AssertConsistency();
                }
            }
            #endregion

            int barMsDuration = MsDuration;


            #region All channelDefs have the same MsDuration.
            foreach(ChannelDef channelDef in ChannelDefs)
            {
                Debug.Assert(channelDef.MsDuration == barMsDuration, "All ChannelDefs in a bar must have the same duration.");
            }
            #endregion

            #region 3. At least one Trk must start with a MidiChordDef, possibly preceded by a ClefDef.
            IReadOnlyList<Trk> trks = Trks;
            bool startFound = false;
            foreach(Trk trk in trks)
            {
                List<IUniqueDef> iuds = trk.UniqueDefs;
                IUniqueDef firstIud = iuds[0];
                if(firstIud is MidiChordDef || (iuds.Count > 1 && firstIud is ClefDef && iuds[1] is MidiChordDef))
                {
                    startFound = true;
                    break;
                }
            }
            Debug.Assert(startFound, "MidiChordDef not found at start.");
            #endregion
        }

        #region envelopes
        /// <summary>
        /// This function does not change the MsDuration of the Bar.
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

            foreach(ChannelDef channelDef in ChannelDefs)
            {
                List<IUniqueDef> iuds = channelDef.UniqueDefs;
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

            AssertConsistency();
        }

        /// <summary>
        /// Returns a list containing the msPositions of all IUniqueDefs plus the endMsPosition of the final object.
        /// </summary>
        private List<int> GetMsPositions()
        {
            int originalMsDuration = MsDuration;

            List<int> originalMsPositions = new List<int>();
            foreach(ChannelDef channelDef in ChannelDefs)
            {
                foreach(IUniqueDef iud in channelDef)
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

            foreach(ChannelDef channelDef in ChannelDefs)
            {
                foreach(var trk in channelDef.Trks)
                {
                    trk.SetPitchWheelSliders(pitchWheelValuesPerMsPosition);
                }
            }
        }

        #endregion envelopes

        public void SetMsPositionsReFirstUD()
        {
            foreach(ChannelDef channelDef in ChannelDefs)
            {
                int msPosition = 0;
                foreach(IUniqueDef iud in channelDef)
                {
                    iud.MsPositionReFirstUD = msPosition;
                    msPosition += iud.MsDuration;
                }
            }
        }

        /// <summary>
        /// Setting this value stretches or compresses the msDurations of all the channelDefs and their contained UniqueDefs.
        /// </summary>
        public int MsDuration
        {
            get
            {
                return ChannelDefs[0].MsDuration;
            }
            set
            {
                AssertConsistency();
                int currentDuration = MsDuration;
                double factor = ((double)value) / currentDuration;
                foreach(ChannelDef channelDef in ChannelDefs)
                {
                    channelDef.MsDuration = (int)Math.Round(channelDef.MsDuration * factor);
                }
                int roundingError = value - MsDuration;
                if(roundingError != 0)
                {
                    foreach(ChannelDef channelDef in ChannelDefs)
                    {
                        if((channelDef.EndMsPositionReFirstIUD + roundingError) == value)
                        {
                            channelDef.EndMsPositionReFirstIUD += roundingError;
                        }
                    }
                }
                foreach(ChannelDef channelDef in ChannelDefs)
                {
                    Debug.Assert(channelDef.MsDuration == value);
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
            return $"AbsMsPosition={AbsMsPosition}, MsDuration={MsDuration}, nVoiceDefs={ChannelDefs.Count}";
        }

        public IReadOnlyList<ChannelDef> ChannelDefs { get => _channelDefs; }
        private List<ChannelDef> _channelDefs = new List<ChannelDef>();
    }
}
