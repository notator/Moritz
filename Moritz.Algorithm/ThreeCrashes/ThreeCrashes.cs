using Krystals5ObjectLibrary;
using Moritz.Spec;

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Moritz.Algorithm.ThreeCrashes
{
    public class ThreeCrashesAlgorithm : CompositionAlgorithm
    {
        public ThreeCrashesAlgorithm()
            : base()
        {
            CheckParameters();
        }

        public override int NumberOfMidiChannels { get { return 3; } }
        public override int NumberOfBars { get { return 27; } }

        private static readonly int nKeyboardPitches = 85;
        private readonly int transposition = (int)((127 - nKeyboardPitches) / 2); // 21 -- puts the range in middle of the MIDI range

        // crashes 1-3 contain nKeyboardPitches values each -- not the 89 values described in the online analysis.
        private static readonly IReadOnlyList<IReadOnlyList<int>> crashAWagons = new List<List<int>>()
        {
            new List<int>() // Wagon A1, 17 values
			{10, 13, 17, 27, 29, 37, 47, 49, 54, 57, 58, 62, 64, 66, 68, 73, 80 },
            new List<int>() // Wagon A2, 17 values
			{19, 20, 24, 25, 32, 34, 36, 38, 40, 55, 56, 59, 74, 78, 79, 82, 85 },
            new List<int>() // Wagon A3, 17 values
			{ 1,  4,  5, 11, 15, 21, 23, 28, 39, 42, 46, 48, 60, 65, 69, 72, 77 },
            new List<int>() // Wagon A4, 17 values
			{14, 18, 26, 30, 33, 35, 43, 44, 45, 50, 51, 52, 61, 63, 76, 81, 84 },
            new List<int>() // Wagon A5, 17 values
			{ 2,  3,  6,  7,  8,  9, 12, 16, 22, 31, 41, 53, 67, 70, 71, 75, 83 }
        };
        private static readonly IReadOnlyList<IReadOnlyList<int>> crashBWagons = new List<List<int>>()
        {
            new List<int>() // Wagon B1, 23 values
			{ 2,  4, 15, 20, 26, 32, 35, 37, 39, 41, 46, 48, 49, 52, 56, 60, 63, 65, 67, 77, 78, 82, 83 },
            new List<int>() // Wagon B2, 20 values
			{ 3,  6,  7, 12, 16, 22, 29, 30, 31, 34, 53, 58, 59, 68, 69, 70, 74, 79, 84, 85 },
            new List<int>() // Wagon B3, 17 values
			{ 9, 10, 13, 14, 24, 25, 42, 43, 44, 61, 64, 66, 71, 72, 73, 76, 81 },
            new List<int>() // Wagon B4, 14 values
			{ 1,  8, 17, 19, 21, 28, 36, 50, 51, 54, 57, 62, 75, 80 },
            new List<int>() // Wagon B5, 11 values
			{ 5, 11, 18, 23, 27, 33, 38, 40, 45, 47, 55 }
        };
        private static readonly IReadOnlyList<IReadOnlyList<int>> crashCWagons = new List<List<int>>()
        {
            new List<int>() // Wagon C1, 29 values
			{ 1,  4,  5,  7, 14, 20, 30, 32, 34, 35, 38, 40, 41, 46, 50, 53, 57, 61, 63, 66, 68, 69, 72, 74, 75, 76, 77, 78, 82 },
            new List<int>() // Wagon C2, 23 values
			{ 8,  9, 10, 13, 15, 19, 24, 26, 29, 33, 37, 39, 43, 44, 45, 49, 51, 52, 54, 62, 67, 70, 85 },
            new List<int>() // Wagon C3, 17 values
			{ 2,  3, 11, 16, 21, 25, 28, 31, 36, 47, 48, 55, 60, 65, 81, 83, 84 },
            new List<int>() // Wagon C4, 11 values
			{ 6, 12, 17, 18, 56, 58, 59, 64, 71, 79, 80 },
            new List<int>() // Wagon C5, 5 values
			{ 22, 23, 27, 42, 73 }
        };

        private static readonly IReadOnlyList<IReadOnlyList<int>> random1to85Lists = new List<List<int>>()
        {
            new List<int>()
            { 50,  1, 12, 49, 55, 34, 60,  3, 13,  7, 84,  8, 66, 45, 22, 83, 36, 10, 63, 31, 28, 53,  4, 38, 30,
              62, 11, 64,  6, 85, 74, 54, 61, 41, 15,  2, 79, 78, 73, 47, 65, 39, 24, 42, 29, 27, 76, 25, 21, 46,
              37, 51,  5,  9, 19, 35, 77, 69, 48, 14, 82, 18, 68, 26, 81, 16, 58, 40, 56, 20, 80, 17, 32, 52, 75,
              33, 57, 67, 44, 71, 43, 72, 23, 70, 59 },
            new List<int>()
            { 31, 44, 71, 76, 68, 53, 32, 13,  5, 14, 69, 64, 50, 79, 78, 59, 48, 29, 34,  7, 37, 11, 85,  6, 65,
              43, 39, 57, 30, 27, 15, 16, 61, 52,  9, 33, 10,  4, 77, 81, 41, 60, 49, 20, 62, 47, 22, 83, 75, 72,
              21, 18, 74, 56,  8, 26, 46, 82, 38, 58, 12, 36, 42, 80, 24, 45, 63, 28, 54, 70, 51, 25, 84, 23, 40,
              19,  3, 66, 35,  2, 73, 55, 17,  1, 67 },
            new List<int>()
            { 85, 32, 82, 79, 66,  7, 12,  4,  6, 46, 39, 18, 69, 29, 34, 26, 35, 80, 72,  2, 42, 15,  5, 60, 65,
               8, 28, 63, 43, 84, 36, 44, 70, 61,  1, 45, 73, 40, 83, 20, 71, 22, 52, 74, 57, 11, 38, 76, 51, 17,
              14, 62, 59, 33, 19, 27, 16,  9, 10, 53, 67, 31, 47, 56, 55,  3, 41, 49, 21, 13, 50, 54, 81, 75, 24,
              58, 30, 68, 64, 48, 78, 77, 23, 37, 25 },
            new List<int>()
            { 85, 75, 47, 68, 45, 55, 54, 48, 51, 27, 21, 80, 32, 61, 46, 66, 10, 36, 44, 41,  5, 19, 18, 35, 25,
              20, 81, 29, 17, 50, 64, 65, 71, 43, 63, 38, 69, 56,  4, 16,  3,  1, 15,  6, 53,  9, 42, 23, 34, 40,
              28, 49, 76, 74, 31, 30,  8, 72, 62, 52, 59,  7, 13, 70, 77, 24, 82, 57, 37, 39, 26, 14, 60, 84, 67,
               2, 11, 83, 78, 33, 79, 58, 22, 12, 73 },
            new List<int>()
            { 40, 34, 27, 56, 10, 26,  4, 20, 64, 42,  7, 47, 52, 76,  9, 28, 67, 24,  8, 45, 65, 66, 13, 71, 74,
              61, 38, 62, 50, 33, 84, 75, 22, 82, 53,  5, 12, 48, 14, 49, 68, 23, 31, 30,  3, 70, 69, 29, 51, 18,
              19, 16, 11, 32, 57, 41, 37, 85, 55, 79, 63, 58, 44, 25, 43,  6, 17, 46, 81,  2, 21, 35, 59, 54, 73,
              77, 39, 80, 78,  1, 15, 72, 83, 60, 36 },
            new List<int>()
            { 15, 61, 28, 34, 35, 62, 75, 26, 56, 23, 80, 27, 76, 20, 39,  4, 66, 74, 65, 70, 53, 82,  7, 60, 54,
              79, 31, 36, 59, 13, 10, 38, 43,  1, 52,  2, 14, 19, 68, 77, 33, 16, 58, 50, 69, 11, 40, 45, 81, 63,
              64, 49, 18, 78, 12, 37, 44, 71, 22, 24, 30, 47, 29,  9, 73, 17, 55,  5, 85,  8, 42,  3, 25, 21, 83,
              51, 32, 84, 57,  6, 72, 48, 67, 46, 41 },
            new List<int>()
            { 41, 22, 56, 28, 30, 36, 33, 52, 53, 45, 15, 34, 37, 38, 85, 70, 60, 81, 71, 51, 26, 64, 17, 72, 19,
              43, 78, 82, 59,  3, 31, 83, 66, 50, 76, 27, 18, 58, 14, 77, 80, 84, 65, 67, 44,  7, 32, 69,  2,  8,
              40, 35,  9, 13, 75,  4, 55, 46, 42, 39, 62, 48, 49, 25, 47, 73, 79, 20, 11, 21, 16, 24, 68,  5, 23,
              54, 74,  1, 63, 10, 29, 61, 12,  6, 57 },
            new List<int>()
            { 65, 53, 56, 45, 48, 79, 37, 33, 55, 74, 78, 52, 10, 24,  2, 31, 41, 25, 75, 42, 19, 28, 66, 80, 12,
              62, 50, 81, 18, 51, 47, 13, 26, 73, 39,  8, 11, 14, 49,  7, 30,  6, 20, 27, 71, 15,  4, 57, 40, 70,
              61, 64, 16, 58, 67, 36, 44, 84, 34, 43, 77, 35, 59, 82, 29, 63,  5, 76, 69,  3, 23, 83,  1,  9, 72,
              21, 60, 22, 54, 32, 17, 46, 85, 68, 38 },
            new List<int>()
            { 39, 29, 57, 71, 65,  7, 51, 36, 63, 22, 16, 31, 41, 48, 68, 37, 23, 58, 20, 69, 54, 15, 84, 49, 73,
              47, 76, 46, 80,  4, 34, 85, 66, 78, 17, 28, 50, 27, 61, 42, 70, 26, 19, 83, 38, 62,  5, 13, 59, 56,
              30, 43,  2, 55,  3, 79, 40, 12, 18, 77, 21, 32, 45, 53, 24, 10, 75, 35, 14, 44,  6, 81, 11, 52,  1,
              25, 33, 82,  8,  9, 67, 64, 72, 60, 74 },
            new List<int>()
            { 47, 11, 84, 23, 46, 64, 17, 38, 77, 71, 25, 83, 18, 66, 21, 54,  7, 73, 52, 81, 67, 45, 10, 55, 68,
               2,  9, 35, 12, 37, 72, 26, 30, 33, 69, 61, 50,  4, 24, 80, 43, 79,  5,  8, 51, 22, 82, 65, 14, 63,
              27, 29, 16, 28, 19, 58, 74, 76, 20, 13, 85,  3, 15, 60, 62,  1, 49, 40, 41, 44, 34, 57, 56, 70, 36,
              42, 78, 32, 75, 31, 39, 48,  6, 59, 53 }
        };

        private static readonly List<List<int>> basicVelocityIndexPerWagonPerAngPos = new List<List<int>>()
            {
                new List<int>() {0,2,4,6,8,9,7,5,3,1}, // wagonIndex 0
				new List<int>() {3,3,4,5,7,7,6,5,4,2}, // wagonIndex 1
				new List<int>() {5,5,5,5,5,5,5,5,5,5}, // wagonIndex 2
				new List<int>() {7,7,6,5,3,3,4,5,6,8}, // wagonIndex 3
				new List<int>() {9,7,5,3,1,0,2,4,6,8}  // wagonIndex 4
			};
        // 10 velocities, equally spaced, from 28 to 127
        // c.f. MoritzStatics.cs/MaxMidiVelocity: -- 9 dynamic ranges, equally spaced from [0..15] (=pppp) to [113..127] (=fff) --. 
        private static readonly List<int> basicVelocities = new List<int>() { 28, 39, 50, 61, 72, 83, 94, 105, 116, 127 };

        /****************************************************************************************/

        // The krystals argument is not used.
        public override List<Bar> DoAlgorithm(List<Krystal> krystals)
        {
            ///*********************************************/
            List<Trk> crashATrks = GetElevenCrashTrks(crashAWagons, 0); // angular position in range [0..10]
            List<Trk> crashBTrks = GetElevenCrashTrks(crashBWagons, 3); // angular position in range [0..10]	
            List<Trk> crashCTrks = GetElevenCrashTrks(crashCWagons, 7); // angular position in range [0..10]

            crashATrks = ReverseAlternateTrks(crashATrks, false);
            crashBTrks = ReverseAlternateTrks(crashBTrks, false);
            crashCTrks = ReverseAlternateTrks(crashCTrks, true);

            SetBeamEnds(crashATrks);
            SetBeamEnds(crashBTrks);
            SetBeamEnds(crashCTrks);

            List<IUniqueDef> firstATrkUIDs = GetFirstTrkUIDs(crashATrks); // these IUniqueDefs track the msPosition of the eleven A trks
            List<IUniqueDef> firstBTrkUIDs = GetFirstTrkUIDs(crashBTrks); // these IUniqueDefs track the msPosition of the eleven B trks
            List<IUniqueDef> firstCTrkUIDs = GetFirstTrkUIDs(crashCTrks); // these IUniqueDefs track the msPosition of the eleven C trks

            Trk crashATrk = Concat(crashATrks);
            Trk crashBTrk = Concat(crashBTrks);
            Trk crashCTrk = Concat(crashCTrks);

            int msDuration = crashATrk.MsDuration;
            crashBTrk.MsDuration = msDuration;
            crashCTrk.MsDuration = msDuration;

            ///*******************************************/

            List<Trk> trks = new List<Trk>() { crashATrk, crashBTrk, crashCTrk };
            Debug.Assert(trks.Count == NumberOfMidiChannels);
            List<ChannelDef> channelDefs = new List<ChannelDef>();
            foreach(var trk in trks)
            {
                channelDefs.Add(new ChannelDef(new List<Trk>() { trk }));
            }

            Bar singleBar = new Bar(0, channelDefs);
            singleBar.AssertConsistency();  // Trks can only contain MidiChordDefs and RestDefs here

            //List<int> endBarlinePositions = GetBalancedBarlineMsPositions(trks, null, NumberOfBars);

            List<int> endBarlinePositions = GetEndBarlineMsPositions(firstATrkUIDs, firstBTrkUIDs, firstCTrkUIDs, msDuration);

            Debug.Assert(NumberOfBars == endBarlinePositions.Count); // change NumberOfBars to match endBarlinePositions.Count! 

            List<Bar> bars = GetBars(singleBar, endBarlinePositions);

            SetPatch0InTheFirstChordInEachVoice(bars[0]);

            return bars;
        }

        private List<IUniqueDef> GetMidiChordDefs(List<int> pitches, List<int> velocities, List<int> msDurations)
        {
            Debug.Assert(pitches.Count == velocities.Count);
            Debug.Assert(msDurations.Count == velocities.Count);

            List<IUniqueDef> defs = new List<IUniqueDef>();
            int msPosition = 0;
            for(int i = 0; i < pitches.Count; ++i)
            {
                List<int> pitchesArg = new List<int>() { pitches[i] };
                List<int> velocitiesArg = new List<int>() { velocities[i] };
                int msDuration = msDurations[i];
                MidiChordDef midiChordDef = new MidiChordDef(pitchesArg, velocitiesArg, msDuration, true)
                {
                    MsPositionReFirstUD = msPosition
                };
                defs.Add(midiChordDef);
                msPosition += msDuration;
            }
            return defs;
        }

        #region GetElevenCrashTracks

        /// <summary>
        /// Returns a sequence of eleven Trks, each having nKeyboardPitches notes. The first and last Trks are at angularPosition.
        /// Assumes that the observer is at the centre of the circle around which the crash moves with constant velocity,
        /// so global durations and velocity are the same for each Trk.
        /// As the crash moves round the circle, the pan value and relative velocity of the wagons changes. 
        /// </summary>
        /// <param name="midiChannel"></param>
        /// <param name="crashWagons"></param>
        /// <param name="initialAngularPosition"></param>
        /// <returns></returns>
        private List<Trk> GetElevenCrashTrks(IReadOnlyList<IReadOnlyList<int>> crashWagons, int initialAngularPosition)
        {
            List<Trk> crashTrks = new List<Trk>();

            for(int i = 0; i < 11; ++i)
            {
                int angularPosition = (initialAngularPosition + i) % 10; // in range [0..10]
                List<Trk> wagonTrks = new List<Trk>();

                for(int wagonIndex = 0; wagonIndex < crashWagons.Count; ++wagonIndex)
                {
                    IReadOnlyList<int> wagonValues = crashWagons[wagonIndex];
                    List<int> velocities = GetBasicVelocities(wagonIndex, angularPosition, wagonValues.Count);
                    List<int> pitchDurations = GetBasicPitchDurations(wagonValues);
                    List<int> midiPitches = GetBasicMidiPitches(wagonValues);

                    Trk wagonTrk = GetWagonTrk(midiPitches, velocities, pitchDurations);

                    wagonTrks.Add(wagonTrk);
                }

                Trk crashTrk = Intersperse(wagonTrks, random1to85Lists[angularPosition]);

                crashTrk = SetPan(crashTrk, angularPosition);
                crashTrk = SetVelocities(crashTrk, angularPosition, minFactor: 0.3);
                crashTrk = SetDuration(crashTrk, angularPosition, maxFactor: 3);

                Debug.Assert(crashTrk.Count == nKeyboardPitches);

                crashTrks.Add(crashTrk);
            }
            return crashTrks;
        }

        private List<int> GetBasicVelocities(int wagonIndex, int angularPos, int wagonValuesCount)
        {
            List<int> basicVelocityIndexPerAngPos = basicVelocityIndexPerWagonPerAngPos[wagonIndex];
            int velocityIndex = basicVelocityIndexPerAngPos[angularPos];
            int velocity = basicVelocities[velocityIndex];
            List<int> velocities = new List<int>();
            for(int i = 0; i < wagonValuesCount; ++i)
            {
                velocities.Add(velocity);
            }
            return velocities;
        }

        private List<int> GetBasicPitchDurations(IReadOnlyList<int> wagonValues)
        {
            // nKeyboardPitches durations (longLow to shortHigh)
            IReadOnlyList<int> basicDurations = BasicDurations();

            Debug.Assert(wagonValues.Count <= nKeyboardPitches && basicDurations.Count == nKeyboardPitches);
            List<int> pitchDurations = new List<int>();
            foreach(int pitch in wagonValues)
            {
                Debug.Assert(pitch >= 1 && pitch <= nKeyboardPitches);
                pitchDurations.Add(basicDurations[pitch - 1]);
            }
            return pitchDurations;
        }

        /// <summary>
        /// returns the real (transposed) MIDI pitch values.
        /// </summary>
        private List<int> GetBasicMidiPitches(IReadOnlyList<int> wagonValues)
        {
            List<int> midiPitches = new List<int>();
            foreach(int value in wagonValues)
            {
                midiPitches.Add((int)(value + transposition));
            }
            return midiPitches;
        }

        /// <summary>
        /// returns a list of ms durations by pitch index (in relation nKeyboardPitchesth root of 2).
        /// The top (=last) value is nearly half the bottom (=first) value;
        /// </summary>
        private IReadOnlyList<int> BasicDurations()
        {
            const int longestMsDuration = 350;
            double factor = Math.Pow(2.0, ((double)1 / nKeyboardPitches));

            List<double> dDurations = new List<double>();
            List<int> durations = new List<int>();
            dDurations.Add(longestMsDuration);
            durations.Add(longestMsDuration);
            for(int i = 1; i < nKeyboardPitches; ++i)
            {
                double dDuration = dDurations[dDurations.Count - 1] / factor;
                dDurations.Add(dDuration);
                durations.Add((int)dDuration);
            }
            return durations as IReadOnlyList<int>;
        }

        private Trk GetWagonTrk(List<int> pitches, List<int> velocities, List<int> durations)
        {
            Trk trk = new Trk();
            List<IUniqueDef> midiChordDefs = GetMidiChordDefs(pitches, velocities, durations);
            trk.UniqueDefs.AddRange(midiChordDefs);

            return trk;
        }

        /// <summary>
        /// Adds cloned IUniqueDefs from wagonTrks to the returned Trk.
        /// </summary>
        private Trk Intersperse(List<Trk> wagonTrks, IReadOnlyList<int> random1to85List)
        {
            List<List<int>> iudIndicesList = new List<List<int>>();
            int indexInRandomList = 0;
            foreach(Trk trk in wagonTrks)
            {
                List<int> iudIndices = new List<int>();
                for(int i = 0; i < trk.Count; ++i)
                {
                    iudIndices.Add((int)(random1to85List[indexInRandomList++] - 1));
                }
                iudIndices.Sort();
                iudIndicesList.Add(iudIndices);
            }

            Trk rval = GetInterspersedTrk(wagonTrks, iudIndicesList);

            return rval;
        }

        /// <summary>
        /// The IUniqueDefs in each wagonTrk are in ascending order.
        /// The iudIndices are also in ascending order in each list of int.
        /// </summary>
        /// <param name="wagonTrks"></param>
        /// <param name="iudIndicesList"></param>
        /// <returns></returns>
        private Trk GetInterspersedTrk(List<Trk> wagonTrks, List<List<int>> iudIndicesList)
        {
            Trk rval = new Trk();

            for(int iudIndex = 0; iudIndex < 85; ++iudIndex)
            {
                for(int wtIndex = 0; wtIndex < iudIndicesList.Count; ++wtIndex)
                {
                    List<int> iudIndices = iudIndicesList[wtIndex];
                    int index = iudIndices.IndexOf(iudIndex);
                    if(index >= 0)
                    {
                        rval.Add((IUniqueDef)wagonTrks[wtIndex][index].Clone());
                        break;
                    }
                }
            }
            return rval;
        }

        private Trk SetPan(Trk crashTrk, int angularPosition)
        {
            Debug.Assert(0 <= angularPosition && angularPosition < 11);

            const double angle = Math.PI / 10;

            int left = 0;
            int twoThirdsLeft = 64 - ((int)(Math.Abs(64 * Math.Sin(angle * 3))));
            int thirdLeft = 64 - ((int)(Math.Abs(64 * Math.Sin(angle))));
            int thirdRight = 64 + ((int)(Math.Abs(64 * Math.Sin(angle))));
            int twoThirdsRight = 64 + ((int)(Math.Abs(64 * Math.Sin(angle * 3)))); ;
            int right = 127;

            int startPanValue = 0;
            int endPanValue = 0;
            switch(angularPosition)
            {
                case 0:
                    startPanValue = thirdRight;
                    endPanValue = thirdLeft;
                    break;
                case 1:
                    startPanValue = thirdLeft;
                    endPanValue = twoThirdsLeft;
                    break;
                case 2:
                    startPanValue = twoThirdsLeft;
                    endPanValue = left;
                    break;
                case 3:
                    startPanValue = left;
                    endPanValue = twoThirdsLeft;
                    break;
                case 4:
                    startPanValue = twoThirdsLeft;
                    endPanValue = thirdLeft;
                    break;
                case 5:
                    startPanValue = thirdLeft;
                    endPanValue = thirdRight;
                    break;
                case 6:
                    startPanValue = thirdRight;
                    endPanValue = twoThirdsRight;
                    break;
                case 7:
                    startPanValue = twoThirdsRight;
                    endPanValue = right;
                    break;
                case 8:
                    startPanValue = right;
                    endPanValue = twoThirdsRight;
                    break;
                case 9:
                    startPanValue = twoThirdsRight;
                    endPanValue = thirdRight;
                    break;
                case 10:
                    startPanValue = thirdRight;
                    endPanValue = thirdLeft;
                    break;
            }

            crashTrk.SetPanGliss(0, crashTrk.Count, startPanValue, endPanValue);

            return crashTrk;
        }

        private Trk SetVelocities(Trk crashTrk, int angularPosition, double minFactor)
        {
            Debug.Assert(0 <= angularPosition && angularPosition < 11);
            Debug.Assert(0 < minFactor && minFactor < 1);

            double factor = Math.Pow(minFactor, 0.2);
            double factor0 = 1;
            double factor1 = factor;
            double factor2 = factor1 * factor;
            double factor3 = factor2 * factor;
            double factor4 = factor3 * factor;
            double factor5 = factor4 * factor;

            double warpFactor = 0;

            switch(angularPosition)
            {
                case 0:
                case 10:
                    warpFactor = factor0;
                    break;
                case 1:
                case 9:
                    warpFactor = factor1;
                    break;
                case 2:
                case 8:
                    warpFactor = factor2;
                    break;
                case 3:
                case 7:
                    warpFactor = factor3;
                    break;
                case 4:
                case 6:
                    warpFactor = factor4;
                    break;
                case 5:
                    warpFactor = factor5;
                    break;
            }

            foreach(IUniqueDef iud in crashTrk.UniqueDefs)
            {
                if(iud is MidiChordDef midiChordDef)
                {
                    int originalVelocity = midiChordDef.Velocities[0];
                    int newVelocity = (int)Math.Round(originalVelocity * warpFactor);
                    midiChordDef.Velocities[0] = newVelocity;
                }
            }

            return crashTrk;
        }

        private Trk SetDuration(Trk crashTrk, int angularPosition, double maxFactor)
        {
            Debug.Assert(0 <= angularPosition && angularPosition < 11);

            double factor = Math.Pow(maxFactor, 0.2);
            double factor0 = 1;
            double factor1 = factor;
            double factor2 = factor1 * factor;
            double factor3 = factor2 * factor;
            double factor4 = factor3 * factor;
            double factor5 = factor4 * factor;

            double warpFactor = 0;

            switch(angularPosition)
            {
                case 0:
                case 10:
                    warpFactor = factor0;
                    break;
                case 1:
                case 9:
                    warpFactor = factor1;
                    break;
                case 2:
                case 8:
                    warpFactor = factor2;
                    break;
                case 3:
                case 7:
                    warpFactor = factor3;
                    break;
                case 4:
                case 6:
                    warpFactor = factor4;
                    break;
                case 5:
                    warpFactor = factor5;
                    break;
            }

            crashTrk.MsDuration = (int)(crashTrk.MsDuration * warpFactor);

            return crashTrk;
        }

        #endregion GetElevenCrashTracks

        private List<Trk> ReverseAlternateTrks(List<Trk> crashTrks, bool reverseFirstTrk)
        {
            List<Trk> reversedTrks = new List<Trk>();
            bool doReverse = reverseFirstTrk;

            foreach(Trk trk in crashTrks)
            {
                if(doReverse)
                {
                    Trk newTrk = new Trk();
                    for(int i = trk.UniqueDefs.Count - 1; i >= 0; --i)
                    {
                        newTrk.Add((IUniqueDef)trk.UniqueDefs[i].Clone());
                    }
                    reversedTrks.Add(newTrk);
                }
                else
                {
                    reversedTrks.Add(trk);
                }
                doReverse = !doReverse;
            }

            return reversedTrks;
        }

        private void SetBeamEnds(List<Trk> crashTrks)
        {
            foreach(Trk crashTrk in crashTrks)
            {
                if(crashTrk.UniqueDefs[crashTrk.UniqueDefs.Count - 1] is MidiChordDef lastMCD)
                {
                    lastMCD.BeamContinues = false;
                }
            }
        }

        private List<IUniqueDef> GetFirstTrkUIDs(List<Trk> crashATrks)
        {
            List<IUniqueDef> firstTrkUIDs = new List<IUniqueDef>();
            foreach(Trk trk in crashATrks)
            {
                firstTrkUIDs.Add(trk.UniqueDefs[0]);
            }
            return firstTrkUIDs;
        }

        private Trk Concat(List<Trk> crashTrks)
        {
            Debug.Assert(crashTrks.Count == 11);

            Trk crashTrk = new Trk();

            foreach(Trk trk in crashTrks)
            {
                crashTrk.AddRange(trk);
            }
            return crashTrk;
        }

        /*********************************/

        private List<int> GetEndBarlineMsPositions(List<IUniqueDef> firstATrkUIDs, List<IUniqueDef> firstBTrkUIDs, List<IUniqueDef> firstCTrkUIDs, int msDuration)
        {
            List<int> endBarlinePositions = new List<int>();

            foreach(IUniqueDef iud in firstATrkUIDs)
            {
                if(iud.MsPositionReFirstUD > 0)
                {
                    endBarlinePositions.Add(iud.MsPositionReFirstUD);
                }
            }
            foreach(IUniqueDef iud in firstBTrkUIDs)
            {
                if(iud.MsPositionReFirstUD > 0)
                {
                    endBarlinePositions.Add(iud.MsPositionReFirstUD);
                }
            }
            foreach(IUniqueDef iud in firstCTrkUIDs)
            {
                if(iud.MsPositionReFirstUD > 0)
                {
                    endBarlinePositions.Add(iud.MsPositionReFirstUD);
                }
            }

            endBarlinePositions.Add(msDuration);

            endBarlinePositions.Sort();

            // remove duplicates and barlines that are closer than 1000ms to the previous one
            for(int i = endBarlinePositions.Count - 1; i > 0; --i)
            {
                if(endBarlinePositions[i] - endBarlinePositions[i - 1] <= 1000)
                {
                    endBarlinePositions.RemoveAt(i);
                }
            }
            return endBarlinePositions;
        }
    }
}
