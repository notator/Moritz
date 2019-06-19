using System;
using System.Diagnostics;
using System.Collections.Generic;

using Krystals4ObjectLibrary;
using Moritz.Globals;

namespace Moritz.Spec
{
	public class ChainTrk : Trk
	{
		/// <summary>
		/// Constructs a Trk having midiChannel and msDuration, containing MidiChordDefs having the relativeDurations.
		/// The constructed MidiChordDefs each have only one BasicMidiChordDef (i.e. they are not ornaments).
		/// All notes are constructed with (default) velocity 100.
		/// (After construction, use Trk functions to set Velocity, PitchWheel, Pan envelopes etc.)
		/// The number of MidiChordDefs will be the number of relativeDurations.
		/// The pitches in the first MidiChordDef will be the unique values in startPitches.
		/// The pitches in the final MidiChordDef will be close to (but not the same as) the targetPitches that are in range [0..127].
		/// The number of values in startPitches and targetPitches must be the same.
		/// Inside each startPitch and targetPitch list, values may repeat and be in any order.
		/// A chain is constructed for each startPitch-targetPitch pair in which target pitch is in range [0..127].
		/// Pitches are normalized in each chord, so that they are in ascending order with no duplicates.
		/// </summary>
		/// <param name="midiChannel"></param>
		/// <param name="msDuration"></param>
		/// <param name="relativeDurations"></param>
		/// <param name="startPitches">Range [0..127], in any order, repeated values are allowed.</param>
		/// <param name="targetPitches">Range [0..127] or int.MaxValue or byte.MaxValue, in any order, repeated values are allowed.</param>
		/// <param name="pitchEnv">Start and end values must be the same, the lowest value must be 0</param>
		public ChainTrk(int midiChannel, int msDuration, List<int> relativeDurations,
			List<int> startPitches, List<int> targetPitches,
			Envelope pitchEnv )
			: base(midiChannel)
		{
			#region local functions
			void CheckEnvelope(Envelope env)
			{
				var orig = env.Original;
				if(orig[0] != orig[orig.Count - 1])
				{
					throw new ApplicationException("pitchEnv's start and end values must be the same.");
				}

				if(orig.Count != (relativeDurations.Count + 1))
				{
					throw new ApplicationException("pitchEnv's Count must be relativeDurations.Count + 1.");
				}

				var zeroFound = false;
				foreach(var val in orig)
				{
					if(val == 0)
					{
						zeroFound = true;
						break;
					}
				}
				if(zeroFound == false)
				{
					throw new ApplicationException("pitchEnv's lowest value must be zero.");
				}

			}
			#endregion

			#region argument checks
			Debug.Assert(startPitches.Count == targetPitches.Count);
			for(int i = 0; i < startPitches.Count; i++)
			{
				Debug.Assert(startPitches[i] >= 0 && startPitches[i] <= 127);
				Debug.Assert((targetPitches[i] >= 0 && targetPitches[i] <= 127) || targetPitches[i] == int.MaxValue || targetPitches[i] == byte.MaxValue);
			}
			CheckEnvelope(pitchEnv);
			#endregion

			var nMidiChordDefs = relativeDurations.Count;

			List<List<byte>> pitchValuesPerMCD = GetPitchesPerMCD(pitchEnv, nMidiChordDefs, startPitches, targetPitches);

			for(int i = 0; i < relativeDurations.Count; i++)
			{
				List<byte> pitches = pitchValuesPerMCD[i];
				var velocities = new List<byte>();
				foreach(var pitch in pitches)
				{
					velocities.Add(100);
				}
				var msDur = relativeDurations[i];
				var mcd = new MidiChordDef(pitches, velocities, msDur, true);
				this.UniqueDefs.Add(mcd);
			}

			this.MsDuration = msDuration; // set to the required duration.

			//this.AgglommerateOrnaments(); // define this in Trk.
		}

		/// <summary>
		/// Returns one list per midiChordDef.
		/// The envelope is first created flat, with its first value equal to 0,
		/// then sheared by the distance between the values in the start and target pitches lists.
		/// The first value does not change.
		/// </summary>
		/// <param name="pitchEnv"></param>
		/// <param name="nMidiChordDefs"></param>
		/// <param name="chainEndsList"></param>
		/// <returns></returns>
		private List<List<byte>> GetPitchesPerMCD(Envelope pitchEnv, int nMidiChordDefs, List<int> startPitches, List<int> targetPitches)
		{
			// remove duplicates and sort
			List<byte> NormalizePitches(List<byte> pitches)
			{
				List<byte> returnedPitches = new List<byte>();
				for(int i = 0; i < pitches.Count; i++)
				{
					if(!returnedPitches.Contains(pitches[i]))
					{
						returnedPitches.Add(pitches[i]);
					}
				}
				returnedPitches.Sort();

				return returnedPitches;
			}

			var envelope = pitchEnv.Original;
			Debug.Assert(envelope.Count == nMidiChordDefs + 1);

			var firstEnvelopeVal = envelope[0];
			for(int i = 0; i < envelope.Count; i++)
			{
				envelope[i] -= firstEnvelopeVal;
			}

			var shearListsPerChain = new List<List<int>>();
			for(int i = 0; i < startPitches.Count; i++)
			{
				if(targetPitches[i] >= 0 && targetPitches[i] <= 127)
				{
					var shearIncrement = ((double)targetPitches[i] - startPitches[i]) / nMidiChordDefs;
					var shearIncrementPerMCD = new List<int>();
					double increment = 0;
					for(int j = 0; j < nMidiChordDefs; j++)
					{
						shearIncrementPerMCD.Add((int)Math.Round(increment));
						increment += shearIncrement;
					}
					shearListsPerChain.Add(shearIncrementPerMCD);
				}
				else
				{
					shearListsPerChain.Add(null);
				}
			}

			List<List<byte>> rval = new List<List<byte>>();
			for(int chordIndex = 0; chordIndex < nMidiChordDefs; chordIndex++)
			{
				var pitches = new List<byte>();

				for(int pitchIndex = 0; pitchIndex < shearListsPerChain.Count; pitchIndex++)
				{
					if(chordIndex == 0)
					{
						pitches.Add((byte)startPitches[pitchIndex]);
					}
					else
					{
						var shearListPerChord = shearListsPerChain[pitchIndex];
						if(shearListPerChord != null)
						{
							var startVal = startPitches[pitchIndex];
							var envVal = envelope[chordIndex];
							double shearIncrement = shearListPerChord[chordIndex];
							byte pitch = M.MidiValue((int)Math.Round(startVal + envVal + shearIncrement));
							pitches.Add(pitch);
						}
					}
				}

				pitches = NormalizePitches(pitches);
				rval.Add(pitches);
			}
			return rval;

		}
	}
}
