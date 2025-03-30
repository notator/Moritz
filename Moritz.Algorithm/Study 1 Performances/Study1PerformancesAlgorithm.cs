using Krystals5ObjectLibrary;

using Moritz.Globals;
using Moritz.Spec;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Moritz.Algorithm.Study1
{
    public class Study1PerformancesAlgorithm : CompositionAlgorithm
    {
        public Study1PerformancesAlgorithm()
            : base()
        {
            CheckParameters();
        }

        // required as forward reference by AssistantComposerForm.
        public override int NumberOfMidiChannels { get { return 1; } }
        public override int NumberOfBars { get { return 68; } }

        // The krystals argument is not used.
        public override List<Bar> DoAlgorithm(List<Krystal> krystals)
        {
            List<byte> trackChordNumbers = GetTrackChordNumbers();
            List<byte> trackRootPitches = GetTrackRootPitches();

            Debug.Assert(trackChordNumbers.Count == trackRootPitches.Count);

            Trk trk0 = GetTrk0(trackChordNumbers, trackRootPitches);
            // parallel Trks must have the same number and types of contained UniqueDefs
            // but they can have different durations and MIDI definitions
            Trk trk1 = GetTrk1(trk0);                                      
            Trk trk2 = GetTrk2(trk1);
            Trk trk3 = GetTrk3(trk2);

            List<Trk> channelTrks = new List<Trk>() { trk0, trk1, trk2, trk3 }; // all in the same channel
            var channelDef = new ChannelDef(channelTrks);
            List<ChannelDef> channelDefs = new List<ChannelDef>() { channelDef };

            Debug.Assert(channelDefs.Count == NumberOfMidiChannels);

            Bar singleBar = new Bar(0, channelDefs);

            singleBar.AssertConsistency();  // Trks can only contain MidiChordDefs and RestDefs here

            List<int> barlineMsPositions = GetBalancedBarlineMsPositions(singleBar.Trks0, NumberOfBars);

            List<Bar> bars = GetBars(singleBar, barlineMsPositions, null, null);

            foreach(ChannelDef cDef in channelDefs)
            {
                foreach(Trk trk in channelDef.Trks)
                {
                    trk.SetPatch0InTheFirstChord();
                }
            }            

            return bars;  // The Trks in these bars dont contain ClefDefs.
        }

        private Trk GetTrk1(Trk trk0)
        {
            var trk1 = (Trk)trk0.Clone();

            foreach(var midiChordDef in trk1.MidiChordDefs)
            {

            }

            trk1.AssertConsistency();

            return trk1;
        }
        private Trk GetTrk2(Trk trk1)
        {
            var trk2 = (Trk)trk1.Clone();

            foreach(var midiChordDef in trk2.MidiChordDefs)
            {

            }

            trk2.AssertConsistency();

            return trk2;
        }
        private Trk GetTrk3(Trk trk2)
        {
            var trk3 = (Trk)trk2.Clone();

            foreach(var midiChordDef in trk2.MidiChordDefs)
            {

            }

            trk3.AssertConsistency();

            return trk3;
        }

        /// <summary>
        /// See summary and example code on abstract definition in CompositionAlogorithm.cs
        /// </summary>
        protected override List<List<SortedDictionary<int, string>>> GetClefChangesPerBar(int nBars, int nVoicesPerBar)
        {
            return null;
        }

        private List<byte> GetTrackChordNumbers()
        {
            var bytes = new byte[] { };
            
            bytes = File.ReadAllBytes(M.MoritzScoresFolder + @"\Study 1\A4chordNumbers");

            var rval = new List<byte>(bytes);

            return rval;
        }

        private List<byte> GetTrackRootPitches()
        {
            var bytes = new byte[] { };

            bytes = File.ReadAllBytes(M.MoritzScoresFolder + @"\Study 1\A4pitches");

            var rval = new List<byte>(bytes);

            return rval;
        }

        private Trk GetTrk0(List<byte> trackChordNumbers, List<byte> trackRootPitches)
        {
            Trk trk0 = new Trk(new List<IUniqueDef>());
            List<List<byte>> chordIntervals = GetChordIntervals();
            List<byte> chordVelocities = GetChordVelocities();
            List<int> chordDurations = GetChordMsDurations();

            int nChords = trackChordNumbers.Count;
            int chordMsPosition = 0;
            for(int i = 0; i < nChords; ++i)
            {
                byte chordNumber = trackChordNumbers[i];
                byte pitchNumber = trackRootPitches[i];

                IUniqueDef midiChordDef = GetMidiChordDef(chordIntervals[chordNumber - 1], chordVelocities[chordNumber - 1], chordDurations[chordNumber - 1], pitchNumber, chordMsPosition);
                chordMsPosition += midiChordDef.MsDuration;

                trk0.UniqueDefs.Add(midiChordDef);
            }

            trk0.AssertConsistency();

            return trk0;
        }

        private IUniqueDef GetMidiChordDef(List<byte> chordIntervals, byte chordVelocity, int chordDuration, int relativePitch, int msPosition)
        {
            List<byte> pitches = GetPitches(relativePitch, chordIntervals);
            List<byte> velocities = GetVelocities(chordVelocity, chordIntervals.Count() + 1);

            IUniqueDef midiChordDef = new MidiChordDef(pitches, velocities, chordDuration, true)
            {
                MsPositionReFirstUD = msPosition
            };

            return midiChordDef;
        }

        private List<byte> GetPitches(int relativePitch, List<byte> primeIntervals)
        {
            List<byte> pitches = new List<byte>();
            byte rootPitch = (byte)(59 + relativePitch); // C is relativePitch 1
            pitches.Add(rootPitch);

            for(int i = 0; i < primeIntervals.Count; ++i)
            {
                byte newPitch = (byte)(rootPitch + primeIntervals[i]);
                pitches.Add(newPitch);
                rootPitch = newPitch;
            }
            return pitches;
        }

        private List<byte> GetVelocities(byte velocity, int nVelocities)
        {
            List<byte> velocities = new List<byte>();
            for(int i = 0; i < nVelocities; ++i)
            {
                velocities.Add(velocity);
            }

            return velocities;
        }

        List<List<byte>> GetChordIntervals()
        {
            List<List<byte>> chords = new List<List<byte>>
            {
				// The numbers are the number of semitones between neighbouring notes in the chord.
				new List<byte>() { }, // chordNumber 1 (density 1)
				new List<byte>() { 4 }, // chordNumber 2 (density 2)
				new List<byte>() { 4, 3 }, // chordNumber 3	 (density 3)
				new List<byte>() { 3, 4, 2 }, // chordNumber 4 (density 4)
				new List<byte>() { 3, 2, 4, 1 }, // chordNumber 5 (density 5)
				new List<byte>() { 2, 3, 1, 4, 5 }, // chordNumber 6  (density 6)
				new List<byte>() { 2, 1, 3, 4, 5, 7 }, // chordNumber 7 (density 7)
				new List<byte>() { 1, 2, 3, 4, 5, 7, 12 } // chordNumber 8 (density 8)
			};

            return chords;
        }

        List<byte> GetChordVelocities()
        {
            List<byte> velocities = new List<byte>
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