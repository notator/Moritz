using Krystals5ObjectLibrary;

using Moritz.Globals;
using Moritz.Palettes;
using Moritz.Spec;

using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Moritz.Algorithm.Study1
{
    public class Study1Algorithm : CompositionAlgorithm
    {
        public Study1Algorithm()
            : base()
        {
            CheckParameters();
        }

        public override IReadOnlyList<int> MidiChannelPerOutputVoice { get { return new List<int>() { 0 }; } }
        public override int NumberOfBars { get { return 68; } }
        public override IReadOnlyList<int> MidiChannelPerInputVoice { get { return null; } }

        // Neither the krystals, nor the palettes argument is used.
        public override List<Bar> DoAlgorithm(List<Krystal> krystals, List<Palette> palettes)
        {
            List<byte> trackChordNumbers = GetTrackChordNumbers();
            List<byte> trackRootPitches = GetTrackRootPitches();

            Debug.Assert(trackChordNumbers.Count == trackRootPitches.Count);

            Trk track = GetTrack(trackChordNumbers, trackRootPitches);
            Seq mainSeq = new Seq(0, new List<Trk>() { track }, MidiChannelPerOutputVoice);
            List<int> barlineMsPositions = GetBalancedBarlineMsPositions(mainSeq.Trks, null, NumberOfBars);

            List<Bar> bars = GetBars(mainSeq, null, barlineMsPositions, null, null);

            SetPatch0InTheFirstChord(bars[0].VoiceDefs[0]);

            return bars;
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

        private Trk GetTrack(List<byte> trackChordNumbers, List<byte> trackRootPitches)
        {
            Trk track = new Trk(0, 0, new List<IUniqueDef>());
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

                track.UniqueDefs.Add(midiChordDef);
            }

            return track;
        }

        private IUniqueDef GetMidiChordDef(List<byte> chordIntervals, byte chordVelocity, int chordDuration, int relativePitch, int msPosition)
        {
            List<byte> pitches = GetPitches(relativePitch, chordIntervals);
            List<byte> velocities = GetVelocities(chordVelocity, chordIntervals.Count() + 1);

            IUniqueDef mcd = new MidiChordDef(pitches, velocities, chordDuration, true)
            {
                MsPositionReFirstUD = msPosition
            };

            return mcd;
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
        /// <summary>
        /// The patch only needs to be set in the first chord, since it will be set by shunting if the Assistant Performer starts later.
        /// </summary>
        private void SetPatch0InTheFirstChord(VoiceDef voiceDef)
        {
            MidiChordDef firstMidiChordDef = null;
            foreach(IUniqueDef iUniqueDef in voiceDef.UniqueDefs)
            {
                firstMidiChordDef = iUniqueDef as MidiChordDef;
                if(firstMidiChordDef != null)
                {
                    firstMidiChordDef.Patch = 0;
                    break;
                }
            }
        }
    }
}