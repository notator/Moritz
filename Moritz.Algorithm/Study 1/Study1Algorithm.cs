﻿using System.Collections.Generic;
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
		}

		public override IReadOnlyList<int> MidiChannelIndexPerOutputVoice { get{ return new List<int>() { 0 }; }}
		public override IReadOnlyList<int> MasterVolumePerOutputVoice { get{ return new List<int>() { 127 }; }}
		public override int NumberOfBars { get{	return 68; }}
		public override int NumberOfInputVoices { get { return 0; } }

		// Neither the krystals, nor the palettes argument is used.
		public override List<List<VoiceDef>> DoAlgorithm(List<Krystal> krystals, List<Palette> palettes)
		{
			byte[] trackChordNumbers = GetTrackChordNumbers();
			byte[] trackRootPitches = GetTrackRootPitches();

			Debug.Assert(trackChordNumbers.GetLength(0) == trackRootPitches.GetLength(0));

			Trk track = GetTrack(trackChordNumbers, trackRootPitches);

			List<int> nChordsPerSystem = GetNChordsPerSystem(NumberOfBars, trackChordNumbers.GetLength(0));

			List<List<VoiceDef>> bars = GetBars(track, nChordsPerSystem);

			SetPatch0InAllChords(bars);

			return bars;
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

			IUniqueDef mcd = new MidiChordDef(pitches, velocities, msPosition, chordDuration, true);

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

		private List<List<VoiceDef>> GetBars(Trk track, List<int> nChordsPerSystem)
		{
			List<Trk> consecutiveBars = new List<Trk>();
			// barlineMsPositions will contain both msPos=0 and the position of the final barline
			List<int> barlineMsPositions = new List<int>() { 0 };

			int trackIndex = 0;
			int msPositionReContainer = 0;
			foreach(int nChords in nChordsPerSystem)
			{
				Trk trk = new Trk(0, msPositionReContainer, new List<IUniqueDef>());
				for(int i = 0; i < nChords; ++i)
				{
					trk.Add(track[trackIndex++]);					
				}
				msPositionReContainer = trk.EndMsPositionReFirstIUD;
				barlineMsPositions.Add(msPositionReContainer);
				consecutiveBars.Add(trk);
			}

			List<List<VoiceDef>> bars = new List<List<VoiceDef>>();
			foreach(Trk trk in consecutiveBars)
			{
				List<VoiceDef> bar = new List<VoiceDef>() { trk };
				bars.Add(bar);
			}

			return bars;
		}

		/// <summary>
		/// In other score algorithms, midiChordDef.Patch is always set. This is done here too, for consistency, even
		/// though the patch does not change, and its therefore not necessary to reset the patch when restarting a performance.
		/// If the midiChordDef.Patch is not set on every chord, and the performance starts from somewhere in the middle
		/// after performing another piece, the channels will all have the wrong patch.
		/// </summary>
		/// <param name="bars"></param>
		private void SetPatch0InAllChords(List<List<VoiceDef>> bars)
		{
			MidiChordDef midiChordDef = null;
			foreach(List<VoiceDef> bar in bars)
			{
				foreach(VoiceDef voiceDef in bar)
				{
					foreach(IUniqueDef iUniqueDef in voiceDef.UniqueDefs)
					{
						midiChordDef = iUniqueDef as MidiChordDef;
						if(midiChordDef != null)
						{
							midiChordDef.Patch = 0;
						}
					}
				}
			}
		}
	}
}