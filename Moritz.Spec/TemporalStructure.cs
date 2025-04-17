using Moritz.Globals;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moritz.Spec
{
    /// <summary>
    /// The TemporalStructure is a single Bar without graphic information.
    /// It consists of a List of Trks (one per midi channel), each of which
    /// can contain only DurationDefs (i.e. MidiChordDefs and RestDefs)
    /// The MidiDef properties of the MidiChordDefs and RestDefs have been set to thei alternative interpretations.
    /// </summary>
    public class TemporalStructure : Bar
    {
        public TemporalStructure(List<Trk> trks)
            : base(0, trks)
        {
        }



        public override void AssertConsistency()
        {
            foreach(var trk in Trks)
            {
                foreach(var uniqueDef in trk.UniqueDefs)
                {
                    Debug.Assert(uniqueDef is MidiChordDef || uniqueDef is RestDef);
                    Debug.Assert(!(uniqueDef is CautionaryChordDef || uniqueDef is ClefDef));
                }
            }

            base.AssertConsistency();
        }

        /// Converts this temporalStructure to a list of bars, consuming the temporalStructure's voiceDefs.
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
    }
}
