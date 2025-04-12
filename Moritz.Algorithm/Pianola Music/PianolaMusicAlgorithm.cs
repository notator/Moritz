using Krystals5ObjectLibrary;

using Moritz.Globals;
using Moritz.Spec;
using Moritz.Symbols;

using System;
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

        public override int NumberOfVoices { get { return 6; } }
        public override int NumberOfBars { get { return 8; } }

        /// <summary>
        /// 12.04.2025: Three new Trks have been added to each voice (with Copilot's help).
        /// The extra Trks are going to be performed during the repeated regions defined in the
        /// new SetScoreRegionsData() implementation defined below.
        /// I've now added a trkIndex parameter to the RegionDef constructor.
        /// The AssistantPerformer is going to be enhanced to play a different interpretation
        /// of the graphic symbols for each Region.
        /// </summary>
        /// <param name="pageFormat"></param>
        /// <param name="krystals"></param>
        /// <returns></returns>
        public override List<Bar> DoAlgorithm(PageFormat pageFormat, List<Krystal> krystals)
        {
            // The pageFormat and krystals arguments are not used.
            _ = pageFormat; // is used by the functions used for inserting clefs (see base class: CompositionAlgorithm.cs)
            _ = krystals;

            // Generate the original Trks for each voice
            List<Trk> tracks1and6 = GetTracks1and6();
            List<Trk> tracks2and5 = GetTracks2and5();
            List<Trk> tracks3and4 = GetTracks3and4();

            // Add each original Trk to trks here, in top to bottom (=channelIndex) order in the score.
            List<Trk> originalTrks = new List<Trk>() { tracks1and6[0], tracks2and5[0], tracks3and4[0], tracks3and4[1], tracks2and5[1], tracks1and6[1] };
            Debug.Assert(originalTrks.Count == NumberOfVoices);

            // Create VoiceDefs with four Trks each, preserving the original Trks in voicedDef.Trks[0]
            List<VoiceDef> voiceDefs = GetFourTrkVoiceDefs(originalTrks);

            AddAccelRitToTrksAtIndex(voiceDefs, 1);
            AddRandomPitchBendToTrksAtIndex(voiceDefs, 2);
            // TrkLevel3 is still TrkLevel0 (not a clone!)

            Debug.Assert(voiceDefs.Count == NumberOfVoices);

            // Create the temporal structure
            TemporalStructure temporalStructure = new TemporalStructure(voiceDefs);

            temporalStructure.AssertConsistency(); // Ensure Trks only contain MidiChordDefs and RestDefs

            // Generate barline positions and create bars
            List<int> barlineMsPositions = GetBalancedBarlineMsPositions(temporalStructure.Trks0, NumberOfBars);
            List<Bar> bars = temporalStructure.GetBars(barlineMsPositions);

            // Set the patch for the first chord in each voice
            SetPatch0InTheFirstChordInEachVoice(bars[0]);

            return bars;
        }

        private void AddRandomPitchBendToTrksAtIndex(List<VoiceDef> voiceDefs, int trksIndex)
        {
            Random random = new Random();

            foreach(var voiceDef in voiceDefs)
            {
                Trk trk = voiceDef.Trks[trksIndex];

                foreach(var midiChordDef in trk.MidiChordDefs)
                {
                    if(midiChordDef.MidiChordControlDef == null)
                    {
                        midiChordDef.MidiChordControlDef = new MidiChordControlDef();
                    }
                    midiChordDef.MidiChordControlDef.PitchWheel = random.Next(128);
                }

                voiceDef.AssertConsistency();
            }
        }

        /// <summary>
        /// This function was produced by Copilot, giving it the following information followed by a few follow-up tweeks (!):
        /// This transformation needs to preserve the absolute order (left-right) of the graphic symbols.
        /// So:
        /// 1. create a flat, ordered list of all the MidiChordDefs in the Trks at trksIndex in all voiceDef.Trks.
        /// 2. create a Dictionary<oldMsPos, newMsPos> implementing accel/rit in the newMsPos values.
        /// 3. use the Dictionary to set the new MsDuratons of all the MidiChordDefs in the Trks at trksIndex in all voiceDef.Trks.
        /// Copilot suggested using Envelope.TimeWarp to create the accel/rit, but originally only created the accel.  
        /// </summary>
        /// <param name="voiceDefs"></param>
        /// <param name="trksIndex"></param>
        private void AddAccelRitToTrksAtIndex(List<VoiceDef> voiceDefs, int trksIndex)
        {
            // Step 1: Create a flat, ordered list of all the MidiChordDefs in the Trks at trksIndex
            List<MidiChordDef> allMidiChordDefs = new List<MidiChordDef>();
            foreach(var voiceDef in voiceDefs)
            {
                Trk trk = voiceDef.Trks[trksIndex];
                allMidiChordDefs.AddRange(trk.MidiChordDefs);
            }

            // Sort the MidiChordDefs by their MsPositionReFirstUD to preserve absolute order
            allMidiChordDefs.Sort((a, b) => a.MsPositionReFirstUD.CompareTo(b.MsPositionReFirstUD));

            if(allMidiChordDefs.Count == 0)
            {
                return; // No MidiChordDefs to process
            }

            // Step 2: Calculate total duration and split into two halves
            var lastMCD = allMidiChordDefs[allMidiChordDefs.Count - 1];
            int totalDuration = lastMCD.MsPositionReFirstUD + lastMCD.MsDuration;
            int midpointDuration = totalDuration / 2;

            // Step 3: Create accel/rit mapping
            Dictionary<int, int> oldToNewMsPositions = new Dictionary<int, int>();
            List<int> originalMsPositions = new List<int>();
            foreach(var midiChordDef in allMidiChordDefs)
            {
                originalMsPositions.Add(midiChordDef.MsPositionReFirstUD);
            }

            // Generate new positions for the first half (accelerando)
            List<int> accelMsPositions = GenerateTimeWarp(originalMsPositions, 0, midpointDuration, true);

            // Generate new positions for the second half (ritardando)
            List<int> ritMsPositions = GenerateTimeWarp(originalMsPositions, midpointDuration, totalDuration, false);

            // Combine accel and rit mappings
            for(int i = 0; i < originalMsPositions.Count; i++)
            {
                if(originalMsPositions[i] <= midpointDuration)
                {
                    oldToNewMsPositions[originalMsPositions[i]] = accelMsPositions[i];
                }
                else
                {
                    oldToNewMsPositions[originalMsPositions[i]] = ritMsPositions[i];
                }
            }

            // Step 4: Update the MsDurations of all the MidiChordDefs
            foreach(var voiceDef in voiceDefs)
            {
                Trk trk = voiceDef.Trks[trksIndex];
                foreach(var midiChordDef in trk.MidiChordDefs)
                {
                    int oldStart = midiChordDef.MsPositionReFirstUD;
                    int oldEnd = oldStart + midiChordDef.MsDuration;

                    // Calculate the new start and end positions
                    int newStart = oldToNewMsPositions[oldStart];
                    int newEnd = oldToNewMsPositions.ContainsKey(oldEnd) ? oldToNewMsPositions[oldEnd] : totalDuration;

                    // Update the MsDuration
                    midiChordDef.MsDuration = newEnd - newStart;
                    midiChordDef.MsPositionReFirstUD = newStart; // Update the start position
                }

                voiceDef.AssertConsistency(); // Ensure the Trk remains consistent
            }
        }

        // Copilot Helper function to generate time-warped positions
        private List<int> GenerateTimeWarp(List<int> originalMsPositions, int startMs, int endMs, bool isAccel)
        {
            List<int> warpedPositions = new List<int>();
            double totalRange = endMs - startMs;
            double factor = isAccel ? 2.0 : 0.5; // Acceleration factor for accel, deceleration for rit

            for(int i = 0; i < originalMsPositions.Count; i++)
            {
                if(originalMsPositions[i] < startMs || originalMsPositions[i] > endMs)
                {
                    warpedPositions.Add(originalMsPositions[i]);
                    continue;
                }

                double normalizedPosition = (originalMsPositions[i] - startMs) / totalRange; // Normalize to [0, 1]
                double warpedValue = isAccel
                    ? Math.Pow(normalizedPosition, factor) // Exponential growth for accel
                    : 1 - Math.Pow(1 - normalizedPosition, factor); // Exponential decay for rit
                int newPosition = startMs + (int)(warpedValue * totalRange);
                warpedPositions.Add(newPosition);
            }

            return warpedPositions;
        }

        private static List<VoiceDef> GetFourTrkVoiceDefs(List<Trk> originalTrks)
        {
            List<VoiceDef> voiceDefs = new List<VoiceDef>();
            foreach(var originalTrk in originalTrks)
            {
                // Preserve the original Trk as Trks[0]
                Trk trk0 = originalTrk;

                // Create three additional Trks by cloning trk0
                Trk trk1 = (Trk)trk0.Clone();
                Trk trk2 = (Trk)trk0.Clone();
                Trk trk3 = (Trk)trk0.Clone();

                // Add all four Trks to the VoiceDef
                voiceDefs.Add(new VoiceDef(new List<Trk>() { trk0, trk1, trk2, trk3 }));
            }

            return voiceDefs;
        }

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
            List<int> velocities = new List<int>() { (int)127 };
            int msPosition = 0;
            for(int i = 0; i < 96; ++i)
            {
                List<int> pitchesArg = new List<int>() { (int)pitches[i] };
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

        /// <summary>
        /// 12.04.2025: This function was originally written by Copilot using the (more complicated) implementation
        /// in the old Tombeau1Algorithm file as an example.
        /// I've now added a trkIndex parameter to the RegionDef constructor, linking Regions to Trks.
        /// </summary>
        /// <param name="bars"></param>
        /// <returns></returns>
        public override ScoreData SetScoreRegionsData(List<Bar> bars)
        {
            // Get barline positions and indices
            Dictionary<int, (int index, int msPosition)> msPosPerBarlineIndexDict = GetMsPosPerBarlineIndexDict(bars);

            // Define regions
            var barline0 = msPosPerBarlineIndexDict[0];
            var finalBarline = msPosPerBarlineIndexDict[msPosPerBarlineIndexDict.Count - 1];

            RegionDef regionA = new RegionDef("A", barline0, finalBarline, 0);
            RegionDef regionB = new RegionDef("B", barline0, finalBarline, 1);
            RegionDef regionC = new RegionDef("C", barline0, finalBarline, 2);
            RegionDef regionD = new RegionDef("D", barline0, finalBarline, 3);

            // Create a region sequence that plays the score four times (with different interpretation of the graphics each time).
            List<RegionDef> regionDefs = new List<RegionDef>() { regionA, regionB, regionC, regionD };
            RegionSequence regionSequence = new RegionSequence(regionDefs, "ABCD");

            // Return the ScoreData
            return new ScoreData(regionSequence);
        }

    }
}
