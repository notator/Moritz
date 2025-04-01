using Krystals5ObjectLibrary;

using Moritz.Globals;
using Moritz.Spec;
using Moritz.Symbols;

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Moritz.Algorithm.ErratumMusical
{
    public class ErratumMusicalAlgorithm : CompositionAlgorithm
    {
        public ErratumMusicalAlgorithm()
            : base()
        {
            CheckParameters();
        }

        public override int NumberOfVoices { get { return 1; } }
        public override int NumberOfBars { get { return 8; } }

        private static IReadOnlyList<IReadOnlyList<int>> erratumMusicalGraphPitches = new List<List<int>>()
        {
            new List<int>() // Selection 1
			{ 49, 37, 58, 71, 53,  7, 25, 52, 22, 48,
              69, 39, 61, 46, 56, 15, 27, 84, 77, 76,
              21, 17, 63, 60,  5, 30, 13, 36,  8, 23,
              62, 43, 50, 45, 54, 47, 79, 44, 24,  4,
              42, 78, 38, 12,  2, 34, 18, 16, 72, 80,
              81, 68, 55,  6, 26, 83, 67, 82, 64, 57,
              31, 19, 59, 28, 20, 11,  3, 85,  9, 51,
              1, 32, 73, 66, 70, 29, 74, 75, 40, 14,
              41, 33, 65, 10, 35},
            new List<int>() // Selection 2
			{ 81, 56, 26, 48, 84, 45, 47, 58, 23, 55,
              60, 39, 37, 22, 16,  3, 34, 85,  2, 80,
               7,  6, 74, 29, 28, 20, 73,  5,  4, 69,
              50,  1, 30, 18, 75, 79, 63, 42, 36, 66,
              12, 40, 46, 43, 52, 57, 14, 82, 68, 53,
              83, 11, 25, 76,  8, 19, 33, 71, 38,  9,
              44, 51, 13, 15, 64, 10, 59, 17, 32, 61,
              31, 62, 72, 21, 49, 41, 70, 35, 65, 54,
              27, 77, 67, 78, 24 },
            new List<int>() // Selection 3
			{  1,  7,  2,  8,  9, 10, 13, 16,  3, 14,
              11, 18, 27, 23,  4, 21, 22, 26, 29, 24,
               5,  6, 20, 28, 25, 39, 35, 12, 32, 36,
              38, 31, 30, 15, 33, 48, 41, 47, 42, 17,
              46, 40, 44, 49, 45, 19, 43, 55, 52, 59,
              50, 34, 53, 57, 51, 56, 54, 37, 58, 67,
              66, 63, 65, 68, 60, 64, 61, 69, 62, 77,
              73, 71, 79, 75, 76, 74, 78, 70, 72, 81,
              82, 85, 80, 83, 84 },
            new List<int>() // Selection 4
			{  3,  1,  4,  5,  6,  7,  2,  8,  9, 10,
              11, 12, 13, 16, 14, 15, 17, 19, 21, 18,
              20, 22, 24, 25, 26, 27, 29, 23, 32, 33,
              31, 30, 28, 39, 36, 34, 42, 40, 43, 35,
              41, 45, 46, 48, 49, 37, 44, 50, 51, 53,
              54, 55, 38, 57, 58, 56, 59, 52, 47, 60,
              63, 64, 65, 67, 68, 61, 69, 66, 71, 74,
              77, 62, 70, 72, 76, 79, 78, 82, 83, 81,
              84, 85, 80, 75, 73 },
            new List<int>() // Selection 5
			{  1,  3,  4,  5,  6,  7,  8,  9, 10, 11,
              12, 13,  2, 14, 15, 16, 17, 18, 19, 20,
              21, 23, 24, 25, 22, 27, 28, 29, 30, 31,
              32, 33, 35, 36, 37, 38, 26, 39, 40, 41,
              42, 43, 44, 45, 46, 47, 48, 49, 34, 50,
              51, 52, 53, 54, 55, 57, 58, 59, 60, 61,
              56, 62, 63, 64, 65, 66, 67, 68, 69, 70,
              71, 72, 83, 73, 74, 75, 76, 77, 78, 79,
              80, 81, 82, 84, 85 },
            new List<int>() // Selection 6
			{  1,  3,  4,  6,  7,  9, 10,  2, 11, 12,
              13, 14, 15, 16,  5, 17, 18, 19, 21, 22,
              23,  8, 24, 25, 28, 29, 30, 31, 20, 32,
              33, 34, 35, 36, 37, 26, 38, 39, 40, 41,
              42, 43, 27, 44, 45, 46, 47, 48, 49, 62,
              50, 51, 52, 53, 54, 55, 67, 56, 57, 58,
              59, 60, 61, 72, 63, 64, 65, 66, 68, 69,
              75, 70, 71, 73, 74, 76, 77, 78, 83, 79,
              80, 81, 82, 84, 85 },
            new List<int>() // Selection 7
			{  1,  2,  4,  3,  6,  7,  8,  5,  9, 10,
              11, 19, 12, 13, 23, 14, 15, 16, 28, 17,
              18, 20, 33, 21, 22, 36, 24, 25, 26, 38,
              27, 29, 30, 41, 31, 32, 44, 34, 35, 37,
              48, 39, 40, 42, 51, 43, 45, 46, 52, 47,
              49, 58, 50, 53, 54, 68, 55, 56, 57, 70,
              59, 60, 74, 61, 62, 63, 76, 64, 65, 66,
              79, 67, 69, 80, 71, 72, 73, 84, 75, 77,
              78, 85, 81, 82, 83 },
            new List<int>() // Selection 8
			{  1,  2,  3,  4,  5,  7,  8,  6, 10,  9,
              11, 12, 13, 14, 19, 15, 16, 17, 18, 20,
              21, 22, 23, 24, 25, 26, 27, 28, 29, 43,
              30, 31, 32, 33, 34, 35, 36, 37, 38, 39,
              40, 41, 42, 44, 45, 46, 47, 48, 49, 50,
              51, 52, 53, 65, 54, 55, 56, 73, 57, 58,
              59, 60, 61, 62, 63, 64, 66, 67, 68, 69,
              70, 71, 72, 75, 74, 76, 78, 79, 80, 77,
              82, 81, 84, 85, 83 }
        };
        
        public override List<Bar> DoAlgorithm(PageFormat pageFormat, List<Krystal> krystals)
        {
            // The pageFormat and krystals arguments are not used.
            _ = pageFormat; // is used by the functions used for inserting clefs (see base class: CompositionAlgorithm.cs)
            _ = krystals;

            List<int> endBarlinePositions;
            List<Trk> trks = new List<Trk>() { GetTrack(out endBarlinePositions) };
            List<VoiceDef> voiceDefs = new List<VoiceDef>() { new VoiceDef(trks) };

            M.Assert(voiceDefs.Count == NumberOfVoices);

            TemporalStructure temporalStructure = new TemporalStructure(voiceDefs);

            temporalStructure.AssertConsistency();  // Trks can only contain MidiChordDefs and RestDefs here

            List<Bar> bars = temporalStructure.GetBars(endBarlinePositions);

            SetPatch0InTheFirstChordInEachVoice(bars[0]);

            return bars;
        }

        private List<IUniqueDef> GetMidiChordDefs(List<int> pitches, List<int> velocities, List<int> msDurations)
        {
            M.Assert(pitches.Count == 85);
            M.Assert(velocities.Count == 85);
            M.Assert(msDurations.Count == 85);

            List<IUniqueDef> defs = new List<IUniqueDef>();
            int msPosition = 0;
            for(int i = 0; i < 85; ++i)
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

        private Trk GetTrk(List<int> pitches, List<int> velocities, List<int> durations, int finalRestDuration)
        {
            Trk trk = new Trk();
            List<IUniqueDef> midiChordDefs = GetMidiChordDefs(pitches, velocities, durations);
            trk.UniqueDefs.AddRange(midiChordDefs);

            if(finalRestDuration > 0)
            {
                RestDef midiRestDef = new RestDef(0, finalRestDuration);
                trk.Add(midiRestDef);
            }

            return trk;
        }

        // Returns a Trk containing EM_I to EM_VIII in sequence
        private Trk GetTrack(out List<int> endBarlinePositions)
        {
            // 85 durations (longLow to shortHigh)
            IReadOnlyList<int> durations = Durations();
            IReadOnlyList<List<M.Dynamic>> pitchDynamicPerSelection = GetPitchDynamicPerSelection(erratumMusicalGraphPitches);
            IReadOnlyList<int> finalRestMsDurations = new List<int>() { 2000, 2450, 2200, 1900, 2600, 2400, 2500, 0 };
            M.Assert(finalRestMsDurations.Count == (erratumMusicalGraphPitches.Count));

            endBarlinePositions = new List<int>();
            int endBarlinePosition = 0;
            Trk allSelectionsTrk = new Trk();
            for(int i = 0; i < erratumMusicalGraphPitches.Count; ++i)
            {
                IReadOnlyList<int> graphPitches = erratumMusicalGraphPitches[i];
                List<M.Dynamic> pitchDynamics = pitchDynamicPerSelection[i];
                List<int> velocities = GetVelocities(pitchDynamics); // the values in each sub-selection (=wagon) have the same velocity.
                List<int> pitchDurations = GetPitchDurations(graphPitches, durations);
                List<int> midiPitches = Transposition(graphPitches); // returns the real (transposed) MIDI pitch values.

                Trk selectionTrk = GetTrk(midiPitches, velocities, pitchDurations, finalRestMsDurations[i]);
                endBarlinePosition += selectionTrk.MsDuration;
                endBarlinePositions.Add(endBarlinePosition);

                allSelectionsTrk.AddRange(selectionTrk);
                //RestDef midiRestDef = new RestDef(0, 1000);
                //allSelectionsTrk.Add(midiRestDef);
            }
            //allSelectionsTrk.RemoveAt(allSelectionsTrk.Count - 1);

            return allSelectionsTrk;
        }

        private List<int> GetVelocities(List<M.Dynamic> pitchDynamics)
        {
            List<int> velocities = new List<int>();
            foreach(M.Dynamic dynamic in pitchDynamics)
            {
                velocities.Add(M.MaxMidiVelocity[dynamic]);
            }
            return velocities;
        }

        #region getting dynamics
        private IReadOnlyList<List<M.Dynamic>> GetPitchDynamicPerSelection(IReadOnlyList<IReadOnlyList<int>> erratumMusicalGraphPitches)
        {
            List<List<M.Dynamic>> rval = new List<List<M.Dynamic>>();
            for(int i = 0; i < erratumMusicalGraphPitches.Count; ++i)
            {
                IReadOnlyList<int> selectionGraphPitches = erratumMusicalGraphPitches[i];
                List<M.Dynamic> dynamics = GetDynamics(i + 1, selectionGraphPitches);
                rval.Add(dynamics);
            }

            return rval as IReadOnlyList<List<M.Dynamic>>;
        }

        /// <summary>
        /// The values in each sub-selection (=wagon =colour) have the same dynamic.
        /// The dynamic can be used to determine both the colour and velocity of the pitch
        /// </summary>
        private List<M.Dynamic> GetDynamics(int selectionNumber, IReadOnlyList<int> selectionGraphPitches)
        {
            List<int> redPitches = new List<int>();
            List<int> greenPitches = new List<int>();

            switch(selectionNumber)
            {
                case 1:
                    redPitches.AddRange(new List<int>() { 10, 35 });
                    greenPitches.AddRange(new List<int>() { 83, 67, 82, 64, 57, 31, 19, 59, 28, 20, 11, 3, 85, 9, 51, 1, 32, 73, 66, 70, 29, 74, 75, 40, 14, 41, 33, 65 });
                    break;
                case 2:
                    //redPitches.AddRange(new List<int>() {  });
                    greenPitches.AddRange(new List<int>() { 56, 84, 58, 39, 16, 2, 6, 28, 4, 1, 79, 36, 40, 57, 68, 25, 19, 38, 13, 10, 61, 72, 41, 54, 67 });
                    break;
                case 3:
                    redPitches.AddRange(new List<int>() { 6, 60 });
                    greenPitches.AddRange(new List<int>() { 2, 3, 4, 5, 12, 15, 17, 19, 34, 37, 68, 73, 78, 80 });
                    break;
                case 4:
                    //redPitches.AddRange(new List<int>() { });
                    greenPitches.AddRange(new List<int>() { 2, 16, 18, 24, 28, 35, 37, 38, 47, 61, 62, 83 });
                    break;
                case 5:
                    //redPitches.AddRange(new List<int>() { });
                    greenPitches.AddRange(new List<int>() { 2, 22, 26, 34, 56, 83 });
                    break;
                case 6:
                    //redPitches.AddRange(new List<int>() { });
                    greenPitches.AddRange(new List<int>() { 2, 5, 8, 20, 26, 27, 62, 67, 72, 75, 83 });
                    break;
                case 7:
                    //redPitches.AddRange(new List<int>() { });
                    greenPitches.AddRange(new List<int>() { 3, 5, 19, 23, 28, 33, 36, 38, 41, 44, 48, 51, 52, 58, 68, 70, 74, 76, 79, 80, 84, 85 });
                    break;
                case 8:
                    //redPitches.AddRange(new List<int>() { });
                    greenPitches.AddRange(new List<int>() { 6, 10, 19, 43, 65, 73, 74, 77, 82, 83 });
                    break;
                default:
                    throw new ApplicationException("selectionNumber must be in range [1..8].");
            }

            List<M.Dynamic> dynamics = GetDynamics(selectionGraphPitches, redPitches, greenPitches);

            return dynamics;
        }

        // red pitches have M.Dynamic.fff
        // green pitches have M.Dynamic.f
        // other (=blue) pitches have M.Dynamic.p
        private static List<M.Dynamic> GetDynamics(IReadOnlyList<int> graphPitches, List<int> redPitches, List<int> greenPitches)
        {
            List<M.Dynamic> dynamics = new List<M.Dynamic>();

            foreach(int graphPitch in graphPitches)
            {
                if(redPitches.Contains(graphPitch))
                {
                    dynamics.Add(M.Dynamic.fff);
                }
                else if(greenPitches.Contains(graphPitch))
                {
                    dynamics.Add(M.Dynamic.f);
                }
                else
                {
                    dynamics.Add(M.Dynamic.p); // blue pitches
                }
            }

            return dynamics;
        }
        #endregion

        private List<int> GetPitchDurations(IReadOnlyList<int> graphPitches, IReadOnlyList<int> durations)
        {
            M.Assert(graphPitches.Count == 85 && durations.Count == 85);
            List<int> pitchDurations = new List<int>();
            foreach(int pitch in graphPitches)
            {
                M.Assert(pitch >= 1 && pitch <= 85);
                pitchDurations.Add(durations[pitch - 1]);
            }
            return pitchDurations;
        }

        /// <summary>
        /// returns the real (transposed) MIDI pitch values.
        /// </summary>
        private List<int> Transposition(IReadOnlyList<int> graphPitches)
        {
            int transposition = (int)((127 - 85) / 2); // 21 -- puts the range in middle of the MIDI range
            List<int> midiPitches = new List<int>();
            foreach(int pitch in graphPitches)
            {
                midiPitches.Add((int)(pitch + transposition));
            }
            return midiPitches;
        }

        /// <summary>
        /// returns a list of ms durations by pitch index (in relation 85th root of 2).
        /// The top (=last) value is nearly half the bottom (=first) value;
        /// </summary>
        private IReadOnlyList<int> Durations()
        {
            const int longestMsDuration = 350;
            double factor = Math.Pow(2.0, ((double)1 / 85)); // 1,0081880126197191971720292366177

            List<double> dDurations = new List<double>();
            List<int> durations = new List<int>();
            dDurations.Add(longestMsDuration);
            durations.Add(longestMsDuration);
            for(int i = 1; i < 85; ++i)
            {
                double dDuration = dDurations[dDurations.Count - 1] / factor;
                dDurations.Add(dDuration);
                durations.Add((int)dDuration);
            }
            return durations as IReadOnlyList<int>;
        }
    }
}
