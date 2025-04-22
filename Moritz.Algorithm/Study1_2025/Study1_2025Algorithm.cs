using Krystals5ObjectLibrary;

using Moritz.Globals;
using Moritz.Spec;
using Moritz.Symbols;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Moritz.Algorithm.Study1
{
    public class Study1_2025Algorithm : CompositionAlgorithm
    {
        public Study1_2025Algorithm()
            : base()
        {
            CheckParameters();
        }

        // required as forward reference by AssistantComposerForm.
        public override int NumberOfVoices { get { return 1; } }
        public override int NumberOfBars { get { return 68; } }

        public override List<Bar> DoAlgorithm(PageFormat pageFormat, List<Krystal> krystals)
        {
            // The pageFormat and krystals arguments are not used.
            _ = pageFormat; // is used by the functions used for inserting clefs (see base class: CompositionAlgorithm.cs)
            _ = krystals;

            List<int> trackChordNumbers = GetTrackChordNumbers();
            List<int> trackRootPitches = GetTrackRootPitches();

            Debug.Assert(trackChordNumbers.Count == trackRootPitches.Count);

            Trk trk0 = GetTrk0(trackChordNumbers, trackRootPitches);
            // parallel Trks must have the same sequence of contained MidiChordDefs and RestDefs
            // but they can have different durations and MIDI control definitions
            Trk trk1 = (Trk)trk0.Clone();

            List<List<Trk>> interpretations = new List<List<Trk>>() { new List<Trk>() { trk0, trk1 } }; // in the same channel

            Debug.Assert(interpretations.Count == NumberOfVoices);

            AddRandomPitchBendEnvelopeToTrkAtIndex(interpretations, 1);

            List<Trk> mainTrks = GetMainTrks(interpretations);

            Bar singleBar = new Bar(0, mainTrks);

            // Generate barline positions and create bars
            List<int> barlineMsPositions = singleBar.GetBalancedBarlineMsPositions(singleBar.Trks, NumberOfBars);
            List<Bar> bars = singleBar.GetBars(barlineMsPositions);

            SetInitialChordControls(bars[0]);

            // Test function (see below):
            // InsertClefChanges(bars, pageFormat.VoiceIndicesPerStaff);

            return bars;  // The Trks in these bars contain ClefDefs.
        }

        private void AddRandomPitchBendEnvelopeToTrkAtIndex(List<List<Trk>> interpretations, int trksIndex)
        {
            Random random = new Random();

            foreach(var trkList in interpretations)
            {
                Trk trk = trkList[trksIndex];

                foreach(var midiChordDef in trk.MidiChordDefs)
                {
                    int pitchWheel = (int)M.CMD.PITCH_WHEEL_224;
                    int nValues = random.Next(4) + 1; // 1..4
                    List<int> values = new List<int>();
                    for(int i = 0; i < nValues; i++)
                    {
                        values.Add(random.Next(128));  // 0..127
                    }
                    midiChordDef.EnvelopeTypeDef = new Tuple<int, List<int>>(pitchWheel, values);
                }

                trk.AssertConsistency();
            }
        }

        // Inserts ClefDefs at arbitrary positions in the top VoiceDef.Trks[0].UniqueDefs in each Staff.
        // The main clefs at the beginnings of bars are added automatically later, taking these clef changes
        // into account.
        protected override void InsertClefChanges(List<Bar> bars, List<List<int>> voiceIndicesPerStaff)
        {
            var clefChangesPerBarPerStaff = GetEmptyClefChangesPerBarPerStaff(bars, voiceIndicesPerStaff);

            var barIndex = 3;
            var staffIndex = 0;
            SortedDictionary<int, string> dict = clefChangesPerBarPerStaff[barIndex][staffIndex];

            dict.Add(9, "t");
            dict.Add(8, "b2");
            dict.Add(6, "b");
            dict.Add(4, "t2");

            InsertClefChangesInBars(bars, voiceIndicesPerStaff, clefChangesPerBarPerStaff);
        }

        private List<int> GetTrackChordNumbers()
        {
            var bytes = File.ReadAllBytes(M.MoritzScoresFolder + @"\Study 1 (version 2025)\A4chordNumbers"); 

            var rval = new List<int>();
            foreach(var val in bytes)
            {
                rval.Add((int)val);
            }

            return rval;
        }

        private List<int> GetTrackRootPitches()
        {
            var bytes = File.ReadAllBytes(M.MoritzScoresFolder + @"\Study 1 (version 2025)\A4pitches");

            var rval = new List<int>();
            foreach(var val in bytes)
            {
                rval.Add((int)val);
            }            

            return rval;
        }

        private Trk GetTrk0(List<int> trackChordNumbers, List<int> trackRootPitches)
        {
            Trk trk0 = new Trk(new List<IUniqueDef>());
            List<List<int>> chordIntervals = GetChordIntervals();
            List<int> chordVelocities = GetChordVelocities();
            List<int> chordDurations = GetChordMsDurations();

            int nChords = trackChordNumbers.Count;
            int chordMsPosition = 0;
            for(int i = 0; i < nChords; ++i)
            {
                int chordNumber = trackChordNumbers[i];
                int pitchNumber = trackRootPitches[i];

                MidiChordDef midiChordDef = GetMidiChordDef(chordIntervals[chordNumber - 1], chordVelocities[chordNumber - 1], chordDurations[chordNumber - 1], pitchNumber, chordMsPosition);
                chordMsPosition += midiChordDef.MsDuration;

                trk0.UniqueDefs.Add((IUniqueDef)midiChordDef);
            }

            trk0.AssertConsistency();

            return trk0;
        }

        private MidiChordDef GetMidiChordDef(List<int> chordIntervals, int chordVelocity, int chordDuration, int relativePitch, int msPosition)
        {
            List<int> pitches = GetPitches(relativePitch, chordIntervals);
            List<int> velocities = GetVelocities(chordVelocity, chordIntervals.Count() + 1);

            MidiChordDef midiChordDef = new MidiChordDef(pitches, velocities, chordDuration, true)
            {
                MsPositionReFirstUD = msPosition
            };

            return midiChordDef;
        }

        private List<int> GetPitches(int relativePitch, List<int> primeIntervals)
        {
            List<int> pitches = new List<int>();
            int rootPitch = (int)(59 + relativePitch); // C is relativePitch 1
            pitches.Add(rootPitch);

            for(int i = 0; i < primeIntervals.Count; ++i)
            {
                int newPitch = (int)(rootPitch + primeIntervals[i]);
                pitches.Add(newPitch);
                rootPitch = newPitch;
            }
            return pitches;
        }

        private List<int> GetVelocities(int velocity, int nVelocities)
        {
            List<int> velocities = new List<int>();
            for(int i = 0; i < nVelocities; ++i)
            {
                velocities.Add(velocity);
            }

            return velocities;
        }

        List<List<int>> GetChordIntervals()
        {
            List<List<int>> chords = new List<List<int>>
            {
				// The numbers are the number of semitones between neighbouring notes in the chord.
				new List<int>() { }, // chordNumber 1 (density 1)
				new List<int>() { 4 }, // chordNumber 2 (density 2)
				new List<int>() { 4, 3 }, // chordNumber 3	 (density 3)
				new List<int>() { 3, 4, 2 }, // chordNumber 4 (density 4)
				new List<int>() { 3, 2, 4, 1 }, // chordNumber 5 (density 5)
				new List<int>() { 2, 3, 1, 4, 5 }, // chordNumber 6  (density 6)
				new List<int>() { 2, 1, 3, 4, 5, 7 }, // chordNumber 7 (density 7)
				new List<int>() { 1, 2, 3, 4, 5, 7, 12 } // chordNumber 8 (density 8)
			};

            return chords;
        }

        List<int> GetChordVelocities()
        {
            List<int> velocities = new List<int>
            {
				// velocities from the original composition (see About Study 1)
				//velocities.Add(127);
				//velocities.Add(103);
				//velocities.Add(84);
				//velocities.Add(67);
				//velocities.Add(55);
				//velocities.Add(44);
				//velocities.Add(36);
				//velocities.Add(29);

				// Max value per duration symbol (March 2016)
				// See Moritz.Symbols/System Components/Staff Components/VoiceComponents/AnchorageSymbol.cs.AddDynamic(...)
				M.MaxMidiVelocity[M.Dynamic.fff],   // 127
				M.MaxMidiVelocity[M.Dynamic.ff],    // 113
				M.MaxMidiVelocity[M.Dynamic.f],     // 99
				M.MaxMidiVelocity[M.Dynamic.mf],    // 85
				M.MaxMidiVelocity[M.Dynamic.mp],    // 71
				M.MaxMidiVelocity[M.Dynamic.p],     // 57
				M.MaxMidiVelocity[M.Dynamic.pp],    // 43
				M.MaxMidiVelocity[M.Dynamic.ppp]   // 29
			};

            return velocities;
        }

        private List<int> GetChordMsDurations()
        {
            double baseMsDuration = (1000 * 60) / 250; // 250 beats per minute
            List<int> chordMsDurations = new List<int>
            {
                (int)(baseMsDuration), // chordNumber 1 (density 1)
				(int)(baseMsDuration * 8 / 9), // chordNumber 2 (density 2)
				(int)(baseMsDuration * 8 / 10), // chordNumber 3 (density 3)
				(int)(baseMsDuration * 8 / 11), // chordNumber 4 (density 4)
				(int)(baseMsDuration * 8 / 12), // chordNumber 5 (density 5)
				(int)(baseMsDuration * 8 / 13), // chordNumber 6 (density 6)
				(int)(baseMsDuration * 8 / 14), // chordNumber 7 (density 7)
				(int)(baseMsDuration * 8 / 15) // chordNumber 8 (density 8)
			};

            return chordMsDurations;
        }
       
    }
}