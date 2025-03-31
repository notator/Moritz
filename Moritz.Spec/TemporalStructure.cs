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
    /// </summary>
    public class TemporalStructure : Bar
    {
        public TemporalStructure(List<ChannelDef> channelDefs)
            : base(0, channelDefs)
        {
        }



        public override void AssertConsistency()
        {
            foreach(var channelDef in ChannelDefs)
            {
                foreach(var trk in channelDef.Trks)
                {
                    foreach(var uniqueDef in trk.UniqueDefs)
                    {
                        Debug.Assert(uniqueDef is MidiChordDef || uniqueDef is RestDef);
                        Debug.Assert(!(uniqueDef is CautionaryChordDef || uniqueDef is ClefDef));
                    }
                }
            }

            base.AssertConsistency();
        }

        /// Converts this temporalStructure to a list of bars, consuming the temporalStructure's channelDefs.
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
                if(remainingBar != null)
                {
                    int remainingMsDuration = remainingBar.Trks0[0].MsDuration;
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

        /// <summary>
        /// Returns a Tuple in which Item1 is the popped bar, Item2 is the remaining part of the input bar.
        /// Note that Trks at the same level inside each ChannelDef in each bar have the same duration.
        /// and that all Trks in a ChannelDef have the same sequence of MidiChordDef and RestDef (and no ClefDefs).
        /// </summary>
        /// <param name ="bar">The bar fron which Item1 is popped.</param>
        /// <param name="poppedBarMsDuration">The duration of the first Trk in each ChannelDef in the popped bar.</param>
        /// <returns>The popped bar and the remaining part of the input bar</returns>
        public Tuple<Bar, Bar> PopBar(Bar bar, int poppedBarMsDuration)
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
    }
}
