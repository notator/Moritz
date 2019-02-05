using System;
using System.Collections.Generic;
using System.Diagnostics;

using Krystals4ObjectLibrary;
using Moritz.Globals;
using Moritz.Palettes;
using Moritz.Spec;

namespace Moritz.Algorithm.ErratumMusical
{
    public class ThreeCrashesAlgorithm : CompositionAlgorithm
	{
		public ThreeCrashesAlgorithm()
            : base()
        {
            CheckParameters();
        }

        public override IReadOnlyList<int> MidiChannelPerOutputVoice { get{	return new List<int>() { 0, 1, 2 }; }}
		public override int NumberOfBars { get{	return 8; }}
		public override IReadOnlyList<int> MidiChannelPerInputVoice { get { return null; } }

		// crashes 1-3 contain 85 values each -- not the 89 values described in the online analysis.
		private static readonly IReadOnlyList<IReadOnlyList<byte>> crashAWagons = new List<List<byte>>()
		{
			new List<byte>() // Wagon A1, 17 values
			{10, 13, 17, 27, 29, 37, 47, 49, 54, 57, 58, 62, 64, 66, 68, 73, 80 },
			new List<byte>() // Wagon A2, 17 values
			{19, 20, 24, 25, 32, 34, 36, 38, 40, 55, 56, 59, 74, 78, 79, 82, 85 },
			new List<byte>() // Wagon A3, 17 values
			{ 1,  4,  5, 11, 15, 21, 23, 28, 39, 42, 46, 48, 60, 65, 69, 72, 77 },
			new List<byte>() // Wagon A4, 17 values
			{14, 18, 26, 30, 33, 35, 43, 44, 45, 50, 51, 52, 61, 63, 76, 81, 84 },
			new List<byte>() // Wagon A5, 17 values
			{ 2,  3,  6,  7,  8,  9, 12, 16, 22, 31, 41, 53, 67, 70, 71, 75, 83 },
			new List<byte>() // All A Wagons interspersed, 85 values
			{10, 19,  1, 14,  2, 13, 20,  4, 18,  3,
			 17, 24,  5, 26,  6, 27, 25, 11, 30,  7,
			 29, 32, 15, 33,  8, 37, 34, 21, 35,  9,
			 47, 36, 23, 43, 12, 49, 38, 28, 44, 16,
			 54, 40, 39, 45, 22, 57, 55, 42, 50, 31,
			 58, 56, 46, 51, 41, 62, 59, 48, 52, 53,
			 64, 74, 60, 61, 67, 66, 78, 65, 63, 70,
			 68, 79, 69, 76, 71, 73, 82, 72, 81, 75,
			 80, 85, 77, 84, 83 }
		};
		private static readonly IReadOnlyList<IReadOnlyList<byte>> crashBWagons = new List<List<byte>>()
		{
			new List<byte>() // Wagon B1, 23 values
			{ 2,  4, 15, 20, 26, 32, 35, 37, 39, 41, 46, 48, 49, 52, 56, 60, 63, 65, 67, 77, 78, 82, 83 },
			new List<byte>() // Wagon B2, 20 values
			{ 3,  6,  7, 12, 16, 22, 29, 30, 31, 34, 53, 58, 59, 68, 69, 70, 74, 79, 84, 85 },
			new List<byte>() // Wagon B3, 17 values
			{ 9, 10, 13, 14, 24, 25, 42, 43, 44, 61, 64, 66, 71, 72, 73, 76, 81 },
			new List<byte>() // Wagon B4, 14 values
			{ 1,  8, 17, 19, 21, 28, 36, 50, 51, 54, 57, 62, 75, 80 },
			new List<byte>() // Wagon B5, 11 values
			{ 5, 11, 18, 23, 27, 33, 38, 40, 45, 47, 55 },
			new List<byte>() // All B Wagons interspersed, 85 values
			{ 2,  3,  9,  1,  5,  4,  6, 10,  8, 11,
			 15,  7, 13, 17, 18, 20, 12, 14, 19, 23,
			 26, 16, 24, 21, 27, 32, 22, 25, 28, 33,
			 35, 29, 42, 36, 38, 37, 30, 43, 50, 40,
			 39, 31, 44, 51, 45, 41, 34, 61, 54, 47,
			 46, 53, 64, 57, 55, 48, 58, 66, 62, 49,
			 59, 71, 75, 52, 68, 72, 80, 56, 69, 73,
			 60, 70, 76, 63, 74, 81, 65, 79, 67, 84,
			 77, 85, 78, 82, 83 }
		};
		private static readonly IReadOnlyList<IReadOnlyList<byte>> crashCWagons = new List<List<byte>>()
		{
			new List<byte>() // Wagon C1, 29 values
			{ 1,  4,  5,  7, 14, 20, 30, 32, 34, 35, 38, 40, 41, 46, 50, 53, 57, 61, 63, 66, 68, 69, 72, 74, 75, 76, 77, 78, 82 },
			new List<byte>() // Wagon C2, 23 values
			{ 8,  9, 10, 13, 15, 19, 24, 26, 29, 33, 37, 39, 43, 44, 45, 49, 51, 52, 54, 62, 67, 70, 85 },
			new List<byte>() // Wagon C3, 17 values
			{ 2,  3, 11, 16, 21, 25, 28, 31, 36, 47, 48, 55, 60, 65, 81, 83, 84 },
			new List<byte>() // Wagon C4, 11 values
			{ 6, 12, 17, 18, 56, 58, 59, 64, 71, 79, 80 },
			new List<byte>() // Wagon C5, 5 values
			{ 22, 23, 27, 42, 73 },
			new List<byte>() // All C Wagons interspersed, 85 values
			{ 1,  8,  2,  6, 22,  4,  9,  3, 12, 23,
			  5, 10, 11, 17, 27,  7, 13, 16, 18, 42,
			 14, 15, 21, 56, 73, 20, 19, 25, 58, 30,
			 24, 28, 59, 32, 26, 31, 64, 34, 29, 36,
			 71, 35, 33, 47, 79, 38, 37, 48, 80, 40,
			 39, 55, 41, 43, 60, 46, 44, 65, 50, 45,
			 81, 53, 49, 83, 57, 51, 84, 61, 52, 63,
			 54, 66, 62, 68, 67, 69, 70, 72, 85, 74,
			 75, 76, 77, 78, 82 }
		};

		// Neither the krystals, nor the palettes argument is used.
		public override List<Bar> DoAlgorithm(List<Krystal> krystals, List<Palette> palettes)
		{
			Trk crashATrk = GetCrashTrack(crashAWagons);
			Trk crashBTrk = GetCrashTrack(crashBWagons);
			Trk crashCTrk = GetCrashTrack(crashCWagons);


			List<Trk> trks = new List<Trk>() { crashATrk, crashBTrk, crashCTrk };

			Seq mainSeq = new Seq(0, trks, MidiChannelPerOutputVoice);

			List<InputVoiceDef> inputVoiceDefs = new List<InputVoiceDef>();

			List<int> endBarlinePositions = GetBalancedBarlineMsPositions(trks, null, 10);

			List<List<SortedDictionary<int, string>>> clefChangesPerBar = GetClefChangesPerBar(endBarlinePositions.Count, mainSeq.Trks.Count);

			List<Bar> bars = GetBars(mainSeq, inputVoiceDefs, endBarlinePositions, clefChangesPerBar, null);

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

		private List<IUniqueDef> GetMidiChordDefs(List<byte> pitches, List<byte> velocities, List<int> msDurations)
		{
			Debug.Assert(pitches.Count == 85);
			Debug.Assert(velocities.Count == 85);
			Debug.Assert(msDurations.Count == 85);

			List<IUniqueDef> defs = new List<IUniqueDef>();
			int msPosition = 0;
			for(int i = 0; i < 85; ++i)
			{
				List<byte> pitchesArg = new List<byte>() { pitches[i] };
				List<byte> velocitiesArg = new List<byte>() { velocities[i] };
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

		private Trk GetTrk(List<byte> pitches, List<byte> velocities, List<int> durations, int finalRestDuration)
		{
			Trk trk = new Trk(0);
			List<IUniqueDef> midiChordDefs = GetMidiChordDefs(pitches, velocities, durations);
			trk.UniqueDefs.AddRange(midiChordDefs);

			if(finalRestDuration > 0)
			{
				MidiRestDef midiRestDef = new MidiRestDef(0, finalRestDuration);
				trk.Add(midiRestDef);
			}

			return trk;
		}

		// Returns a Trk constructed from the crashWagons
		private Trk GetCrashTrack(IReadOnlyList<IReadOnlyList<byte>> crashWagons)
		{
			// 85 durations (longLow to shortHigh)
			IReadOnlyList<int> durations = Durations(); 
			IReadOnlyList<List<M.Dynamic>> pitchDynamicPerSelection = GetPitchDynamicPerSelection(crashWagons);
			IReadOnlyList<int> finalRestMsDurations = new List<int>() { 2000, 2450, 2200, 1900, 2600, 2400, 2500, 0 };
			Debug.Assert(finalRestMsDurations.Count == (crashWagons.Count));

			Trk allSelectionsTrk = new Trk(0);
			for(int i = 0; i < crashWagons.Count; ++i)
			{
				IReadOnlyList<byte> graphPitches = crashWagons[i];
				List<M.Dynamic> pitchDynamics = pitchDynamicPerSelection[i];
				List<byte> velocities = GetVelocities(pitchDynamics); // the values in each sub-selection (=wagon) have the same velocity.
				List<int> pitchDurations = GetPitchDurations(graphPitches, durations);
				List<byte> midiPitches = Transposition(graphPitches); // returns the real (transposed) MIDI pitch values.

				Trk selectionTrk = GetTrk(midiPitches, velocities, pitchDurations, finalRestMsDurations[i]);				

				allSelectionsTrk.AddRange(selectionTrk);
				//MidiRestDef midiRestDef = new MidiRestDef(0, 1000);
				//allSelectionsTrk.Add(midiRestDef);
			}
			//allSelectionsTrk.RemoveAt(allSelectionsTrk.Count - 1);

			return allSelectionsTrk;
		}

		private List<byte> GetVelocities(List<M.Dynamic> pitchDynamics)
		{
			List<byte> velocities = new List<byte>();
			foreach(M.Dynamic dynamic in pitchDynamics)
			{
				velocities.Add(M.MaxMidiVelocity[dynamic]);
			}
			return velocities;
		}

		#region getting dynamics
		private IReadOnlyList<List<M.Dynamic>> GetPitchDynamicPerSelection(IReadOnlyList<IReadOnlyList<byte>> erratumMusicalGraphPitches)
		{
			List<List<M.Dynamic>> rval = new List<List<M.Dynamic>>();   
			for(int i = 0; i < erratumMusicalGraphPitches.Count; ++i)
			{
				IReadOnlyList<byte> selectionGraphPitches = erratumMusicalGraphPitches[i];
				List<M.Dynamic> dynamics = GetDynamics(i + 1, selectionGraphPitches);
				rval.Add(dynamics);
			}

			return rval as IReadOnlyList<List<M.Dynamic>>;
		}

		/// <summary>
		/// The values in each sub-selection (=wagon =colour) have the same dynamic.
		/// The dynamic can be used to determine both the colour and velocity of the pitch
		/// </summary>
		private List<M.Dynamic> GetDynamics(int selectionNumber, IReadOnlyList<byte> selectionGraphPitches)
		{
			List<byte> redPitches = new List<byte>();
			List<byte> greenPitches = new List<byte>();

			switch(selectionNumber)
			{
				case 1:
					redPitches.AddRange(new List<byte>() { 10, 35 });
					greenPitches.AddRange(new List<byte>() { 83, 67, 82, 64, 57, 31, 19, 59, 28, 20, 11, 3, 85, 9, 51, 1, 32, 73, 66, 70, 29, 74, 75, 40, 14, 41, 33, 65 });
					break;
				case 2:
					//redPitches.AddRange(new List<byte>() {  });
					greenPitches.AddRange(new List<byte>() { 56, 84, 58, 39, 16, 2, 6, 28, 4, 1, 79, 36, 40, 57, 68, 25, 19, 38, 13, 10, 61, 72, 41, 54, 67 });
					break;
				case 3:
					redPitches.AddRange(new List<byte>() { 6, 60 });
					greenPitches.AddRange(new List<byte>() { 2, 3, 4, 5, 12, 15, 17, 19, 34, 37, 68, 73, 78, 80 });
					break;
				case 4:
					//redPitches.AddRange(new List<byte>() { });
					greenPitches.AddRange(new List<byte>() { 2, 16, 18, 24, 28, 35, 37, 38, 47, 61, 62, 83 });
					break;
				case 5:
					//redPitches.AddRange(new List<byte>() { });
					greenPitches.AddRange(new List<byte>() { 2, 22, 26, 34, 56, 83 });
					break;
				case 6:
					//redPitches.AddRange(new List<byte>() { });
					greenPitches.AddRange(new List<byte>() { 2, 5, 8, 20, 26, 27, 62, 67, 72, 75, 83 });
					break;
				case 7:
					//redPitches.AddRange(new List<byte>() { });
					greenPitches.AddRange(new List<byte>() { 3, 5, 19, 23, 28, 33, 36, 38, 41, 44, 48, 51, 52, 58, 68, 70, 74, 76, 79, 80, 84, 85 });
					break;
				case 8:
					//redPitches.AddRange(new List<byte>() { });
					greenPitches.AddRange(new List<byte>() { 6, 10, 19, 43, 65, 73, 74, 77, 82, 83 });
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
		private static List<M.Dynamic> GetDynamics(IReadOnlyList<byte> graphPitches, List<byte> redPitches, List<byte> greenPitches)
		{
			List<M.Dynamic> dynamics = new List<M.Dynamic>();

			foreach(byte graphPitch in graphPitches)
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

		private List<int> GetPitchDurations(IReadOnlyList<byte> graphPitches, IReadOnlyList<int> durations)
		{
			Debug.Assert(graphPitches.Count == 85 && durations.Count == 85);
			List<int> pitchDurations = new List<int>();
			foreach(byte pitch in graphPitches)
			{
				Debug.Assert(pitch >= 1 && pitch <= 85);
				pitchDurations.Add(durations[pitch - 1]);
			}
			return pitchDurations;
		}

		/// <summary>
		/// returns the real (transposed) MIDI pitch values.
		/// </summary>
		private List<byte> Transposition(IReadOnlyList<byte> graphPitches)
		{
			byte transposition = (byte)((127 - 85) / 2); // 21 -- puts the range in middle of the MIDI range
			List<byte> midiPitches = new List<byte>();
			foreach(byte pitch in graphPitches)
			{
				midiPitches.Add((byte)(pitch + transposition));
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

		/// <summary>
		/// The patch only needs to be set in the first chord in each voice,
		/// since it will be set by shunting if the Assistant Performer starts later.
		/// </summary>
		private void SetPatch0InTheFirstChordInEachVoice(Bar bar1)
		{
			MidiChordDef midiChordDef = null;
			foreach(VoiceDef voiceDef in bar1.VoiceDefs)
			{
				foreach(IUniqueDef iUniqueDef in voiceDef.UniqueDefs)
				{
					midiChordDef = iUniqueDef as MidiChordDef;
					if(midiChordDef != null)
					{
						midiChordDef.Patch = 0;
                        break;
					}
				}
			}
		}
	}
}
