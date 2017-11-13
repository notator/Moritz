using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.IO;

using Krystals4ObjectLibrary;
using Moritz.Palettes;
using Moritz.Spec;
using Moritz.Globals;

namespace Moritz.Algorithm.Study1
{
    public class Study1Algorithm : CompositionAlgorithm
	{
		public Study1Algorithm()
            : base()
        {
            CheckParameters();
        }

		public override IReadOnlyList<int> MidiChannelIndexPerOutputVoice { get{ return new List<int>() { 0 }; }}
		public override int NumberOfBars { get{	return 68; }}
		public override int NumberOfInputVoices { get { return 0; } }

		// Neither the krystals, nor the palettes argument is used.
		public override List<Bar> DoAlgorithm(List<Krystal> krystals, List<Palette> palettes)
		{
			byte[] trackChordNumbers = GetTrackChordNumbers();
			byte[] trackRootPitches = GetTrackRootPitches();

			Debug.Assert(trackChordNumbers.GetLength(0) == trackRootPitches.GetLength(0));

			Trk track = GetTrack(trackChordNumbers, trackRootPitches);

			List<int> nChordsPerSystem = GetNChordsPerSystem(NumberOfBars, trackChordNumbers.GetLength(0));

			List<Bar> bars = GetBars(track, nChordsPerSystem);

			SetPatch0InTheFirstChord(bars[0].VoiceDefs[0]);

            InsertClefChanges(bars);

            return bars;
		}

        protected override void InsertClefChanges(List<Bar> bars)
        {
            //VoiceDef voiceDef = bars[0][bars[0].Count - 1];
            //voiceDef.InsertClefDef(5, "b");
        }

        private List<int> GetNChordsPerSystem(int numberOfBars, int nChords)
		{
			int nLocalChords = 0;
			// total number of chords is 1219
			List<int> nChordsInSystemPerSystem = new List<int>();
			for(int i = 0; i < numberOfBars; ++i)
			{
				nChordsInSystemPerSystem.Add(18);
				nLocalChords += 18;
			}
			for(int i = 28; i < 33; ++i)
			{
				nChordsInSystemPerSystem[i] = 17;
				nLocalChords -= 1;

			}

			Debug.Assert(nChordsInSystemPerSystem.Count == numberOfBars);
			Debug.Assert(nLocalChords == nChords);

			return (nChordsInSystemPerSystem);
		}

		private byte[] GetTrackChordNumbers()
		{
			byte[] bytes = File.ReadAllBytes(@"D:\Visual Studio\Projects\Moritz\Moritz.Algorithm\Study 1\A4chordNumbers");
			return bytes;
		}

		private byte[] GetTrackRootPitches()
		{
			byte[] bytes = File.ReadAllBytes(@"D:\Visual Studio\Projects\Moritz\Moritz.Algorithm\Study 1\A4pitches");
			return bytes;
		}

		private Trk GetTrack(byte[] trackChordNumbers, byte[] trackRootPitches)
		{
			Trk track = new Trk(0, 0, new List<IUniqueDef>());
			List<List<byte>> chordIntervals = GetChordIntervals();
			List<byte> chordVelocities = GetChordVelocities();
			List<int> chordDurations = GetChordMsDurations();
			
			int nChords = trackChordNumbers.GetLength(0);
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

			IUniqueDef mcd = new MidiChordDef(pitches, velocities, chordDuration, true);
            mcd.MsPositionReFirstUD = msPosition;

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
			List<List<byte>> chords = new List<List<byte>>();
			// The numbers are the number of semitones between neighbouring notes in the chord.
			chords.Add(new List<byte>() { }); // chordNumber 1 (density 1)
			chords.Add(new List<byte>() { 4 }); // chordNumber 2 (density 2)
			chords.Add(new List<byte>() { 4, 3 }); // chordNumber 3	 (density 3)
			chords.Add(new List<byte>() { 3, 4, 2 }); // chordNumber 4 (density 4)
			chords.Add(new List<byte>() { 3, 2, 4, 1 }); // chordNumber 5 (density 5)
			chords.Add(new List<byte>() { 2, 3, 1, 4, 5 }); // chordNumber 6  (density 6)
			chords.Add(new List<byte>() { 2, 1, 3, 4, 5, 7 }); // chordNumber 7 (density 7)
			chords.Add(new List<byte>() { 1, 2, 3, 4, 5, 7, 12 }); // chordNumber 8 (density 8)

			return chords;
		}

		List<byte> GetChordVelocities()
		{
			List<byte> velocities = new List<byte>();
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
            velocities.Add(M.MaxMidiVelocity[M.Dynamic.fff]);   // 127
            velocities.Add(M.MaxMidiVelocity[M.Dynamic.ff]);    // 113
            velocities.Add(M.MaxMidiVelocity[M.Dynamic.f]);     // 99
            velocities.Add(M.MaxMidiVelocity[M.Dynamic.mf]);    // 85
            velocities.Add(M.MaxMidiVelocity[M.Dynamic.mp]);    // 71
            velocities.Add(M.MaxMidiVelocity[M.Dynamic.p]);     // 57
            velocities.Add(M.MaxMidiVelocity[M.Dynamic.pp]);    // 43
            velocities.Add(M.MaxMidiVelocity[M.Dynamic.ppp]);   // 29

            return velocities;
		}

		private List<int> GetChordMsDurations()
		{
			double baseMsDuration = (1000 * 60) / 250; // 250 beats per minute
			List<int> chordMsDurations = new List<int>();
			chordMsDurations.Add((int)(baseMsDuration)); // chordNumber 1 (density 1)
			chordMsDurations.Add((int)(baseMsDuration * 8 / 9)); // chordNumber 2 (density 2)
			chordMsDurations.Add((int)(baseMsDuration * 8 / 10)); // chordNumber 3 (density 3)
			chordMsDurations.Add((int)(baseMsDuration * 8 / 11)); // chordNumber 4 (density 4)
			chordMsDurations.Add((int)(baseMsDuration * 8 / 12)); // chordNumber 5 (density 5)
			chordMsDurations.Add((int)(baseMsDuration * 8 / 13)); // chordNumber 6 (density 6)
			chordMsDurations.Add((int)(baseMsDuration * 8 / 14)); // chordNumber 7 (density 7)
			chordMsDurations.Add((int)(baseMsDuration * 8 / 15)); // chordNumber 8 (density 8)

			return chordMsDurations;  
		}

		private List<Bar> GetBars(Trk mainTrack, List<int> nChordsPerSystem)
		{
			int mainTrackIndex = 0;
			int absSeqMsPosition = 0;
			IUniqueDef lastUID = null;
			List<Bar> bars = new List<Bar>();
			foreach(int nChords in nChordsPerSystem)
			{
				Trk trk = new Trk(0, 0, new List<IUniqueDef>());
				for(int i = 0; i < nChords; i++)
				{
					lastUID = mainTrack[mainTrackIndex++];
					trk.Add(lastUID);
				}
				Seq seq = new Seq(absSeqMsPosition, new List<Trk>() { trk }, MidiChannelIndexPerOutputVoice);
				Bar bar = new Bar(seq);
				bars.Add(bar);
				absSeqMsPosition = lastUID.MsPositionReFirstUD + lastUID.MsDuration;
			}

			return bars;
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
