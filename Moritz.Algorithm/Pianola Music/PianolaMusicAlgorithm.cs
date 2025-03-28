using Krystals5ObjectLibrary;

using Moritz.Spec;

using System.Collections.Generic;
using System.Diagnostics;

namespace Moritz.Algorithm.PianolaMusic
{
    public class PianolaMusicAlgorithm : CompositionAlgorithm
    {
        public PianolaMusicAlgorithm()
            : base()
        {
            CheckParameters();
        }

        public override int NumberOfMidiChannels { get { return 6; } }
        public override int NumberOfBars { get { return 8; } }

        // The krystals argument is not used.
        public override List<Bar> DoAlgorithm(List<Krystal> krystals)
        {
            List<Trk> tracks1and6 = GetTracks1and6();
            List<Trk> tracks2and5 = GetTracks2and5();
            List<Trk> tracks3and4 = GetTracks3and4();

            // Add each Trk to trks here, in top to bottom (=channelIndex) order in the score.
            List<Trk> trks = new List<Trk>() { tracks1and6[0], tracks2and5[0], tracks3and4[0], tracks3and4[1], tracks2and5[1], tracks1and6[1] };
            Debug.Assert(trks.Count == NumberOfMidiChannels);

            List<ChannelDef> channelDefs = new List<ChannelDef>();
            foreach(var trk in trks)
            {
                channelDefs.Add(new ChannelDef(new List<Trk>() { trk }));
            }

            Bar singleBar = new Bar(0, channelDefs);

            singleBar.AssertConsistency();  // Trks can only contain MidiChordDefs and RestDefs here

            List<int> barlineMsPositions = GetBalancedBarlineMsPositions(trks, 8);
            List<List<SortedDictionary<int, string>>> clefChangesPerBar = GetClefChangesPerBar(barlineMsPositions.Count, singleBar.ChannelDefs.Count);

            List<Bar> bars = GetBars(singleBar, barlineMsPositions, clefChangesPerBar, null);

            SetPatch0InTheFirstChordInEachVoice(bars[0]);

            return bars;
        }

        /// <summary>
        /// See summary and example code on abstract definition in CompositionAlogorithm.cs
        /// </summary>
        protected override List<List<SortedDictionary<int, string>>> GetClefChangesPerBar(int nBars, int nVoicesPerBar)
        {
            return null;
        }

        // Returns two lists of ints. The first is contains the durations of the upper track, the second the lower.
        private static List<List<int>> TrackDurations(List<int> firstHalfUpperTrack)
        {
            List<int> secondHalfUpperTrack = new List<int>(firstHalfUpperTrack);
            secondHalfUpperTrack.Reverse();

            List<int> lowerTrackDurations = new List<int>(secondHalfUpperTrack);
            lowerTrackDurations.AddRange(firstHalfUpperTrack);

            List<int> upperTrackDurations = new List<int>(firstHalfUpperTrack);
            upperTrackDurations.AddRange(secondHalfUpperTrack);

            List<List<int>> rval = new List<List<int>>
            {
                upperTrackDurations,
                lowerTrackDurations
            };

            return rval;
        }

        private static List<int> TrackPitches(List<int> first24Pitches)
        {
            List<int> pitches = new List<int>(first24Pitches);
            pitches.AddRange(first24Pitches);
            for(int i = 0; i < pitches.Count; ++i)
            {
                pitches[i] += 12;
            }
            List<int> secondHalf = new List<int>(pitches);
            secondHalf.Reverse();
            pitches.AddRange(secondHalf);
            return pitches;
        }

        private List<IUniqueDef> GetMidiChordDefs(List<int> pitches, List<int> durations)
        {
            Debug.Assert(pitches.Count == 96);
            Debug.Assert(durations.Count == 96);

            const int durationFactor = 48;  // the shortest note is 48ms

            List<IUniqueDef> defs = new List<IUniqueDef>();
            List<byte> velocities = new List<byte>() { (byte)127 };
            int msPosition = 0;
            for(int i = 0; i < 96; ++i)
            {
                List<byte> pitchesArg = new List<byte>() { (byte)pitches[i] };
                int msDuration = durations[i] * durationFactor;
                MidiChordDef midiChordDef = new MidiChordDef(pitchesArg, velocities, msDuration, true)
                {
                    MsPositionReFirstUD = msPosition
                };
                defs.Add(midiChordDef);
                msPosition += msDuration;
            }
            return defs;
        }

        private List<Trk> GetTrks(int upperChannel, List<int> upperTrackPitches, int lowerChannel, List<int> lowerTrackPitches, List<List<int>> durations)
        {
            List<IUniqueDef> t1MidiChordDefs = GetMidiChordDefs(upperTrackPitches, durations[0]);
            Trk trk1 = new Trk(t1MidiChordDefs);

            List<IUniqueDef> t6MidiChordDefs = GetMidiChordDefs(lowerTrackPitches, durations[1]);
            Trk trk6 = new Trk(t6MidiChordDefs);

            List<Trk> trks = new List<Trk>
            {
                trk1,
                trk6
            };

            return trks;
        }

        // Returns two Trks. The first is track 1, the second is track 6
        private List<Trk> GetTracks1and6()
        {
            #region pitches
            List<int> first24Track1Pitches = new List<int>()
                { 20, 33, 59, 66, 46, 79, 85, 52, 72, 65, 39, 26,
                  26, 85, 59, 52, 72, 39, 33, 66, 46, 65, 79, 20 };
            List<int> t1Pitches = TrackPitches(first24Track1Pitches);
            List<int> first24Track6Pitches = new List<int>()
                { 87, 28, 42, 61, 41, 74, 68, 35, 55, 48, 22, 81,
                  81, 68, 42, 35, 55, 22, 28, 61, 41, 48, 74, 87 };
            List<int> t6Pitches = TrackPitches(first24Track6Pitches);
            #endregion pitches

            #region durations
            List<int> first48Track1Durations = new List<int>()
                { 144, 12, 36, 120, 24, 132, 60, 96, 48, 108, 84, 72,
                  11, 12, 2, 9, 1, 10, 4, 7, 3, 8, 6, 5,
                  9, 12, 18, 3, 15, 6, 24, 33, 21, 36, 30, 27,
                  80, 90, 110, 60, 100, 70, 10, 40, 120, 50, 30, 20 };
            List<List<int>> durations = TrackDurations(first48Track1Durations);
            #endregion durations

            return GetTrks(0, t1Pitches, 5, t6Pitches, durations);
        }

        private List<Trk> GetTracks2and5()
        {
            #region pitches
            List<int> first24Track2Pitches = new List<int>()
            { 70, 57, 31, 24, 44, 83, 89, 50, 18, 37, 63, 76,
              76, 89, 31, 50, 18, 63, 57, 24, 44, 37, 83, 70 };
            List<int> t2Pitches = TrackPitches(first24Track2Pitches);

            List<int> first24Track5Pitches = new List<int>()
            { 37, 24, 70, 63, 83, 50, 44, 89, 57, 76, 18, 31,
              31, 44, 70, 89, 57, 18, 24, 63, 83, 76, 50, 37 };
            List<int> t5Pitches = TrackPitches(first24Track5Pitches);
            #endregion pitches

            #region durations
            List<int> first48Track2Durations = new List<int>()
            { 32, 40, 56, 16, 48, 24, 72, 96, 64, 8, 88, 80,
              35, 40, 50, 25, 45, 30, 60, 15, 55, 20, 10, 5,
              77, 88, 110, 55, 99, 66, 132, 33, 121, 44, 22, 11,
              8, 10, 14, 4, 12, 6, 18, 24, 16, 2, 22, 20 };
            List<List<int>> durations = TrackDurations(first48Track2Durations);
            #endregion durations

            return GetTrks(1, t2Pitches, 4, t5Pitches, durations);
        }

        private List<Trk> GetTracks3and4()
        {
            #region pitches
            List<int> first24Track3Pitches = new List<int>()
            { 42, 55, 81, 28, 68, 41, 35, 74, 22, 87, 61, 48,
              48, 35, 81, 74, 22, 61, 55, 28, 68, 87, 41, 42 };
            List<int> t3Pitches = TrackPitches(first24Track3Pitches);

            List<int> first24Track4Pitches = new List<int>()
            { 65, 66, 20, 39, 79, 52, 46, 85, 33, 26, 72, 59,
              59, 46, 20, 85, 33, 72, 66, 39, 79, 26, 52, 65 };
            List<int> t4Pitches = TrackPitches(first24Track4Pitches);
            #endregion pitches

            #region durations
            List<int> first48Track3Durations = new List<int>()
            { 8, 12, 20, 48, 16, 4, 28, 40, 24, 44, 36, 32,
              81, 90, 108, 63, 99, 72, 18, 45, 9, 54, 36, 27,
              35, 42, 56, 21, 49, 28, 70, 7, 63, 14, 84, 77,
              36, 42, 54, 24, 48, 30, 66, 12, 60, 18, 6, 72 };
            List<List<int>> durations = TrackDurations(first48Track3Durations);
            #endregion durations

            return GetTrks(2, t3Pitches, 3, t4Pitches, durations);
        }
    }
}
