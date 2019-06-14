using System;
using System.Diagnostics;
using System.Collections.Generic;

using Krystals4ObjectLibrary;
using Moritz.Globals;

namespace Moritz.Spec
{
	class ChainTrk : Trk
	{
		/// <summary>
		/// Constructs a Trk having midiChannel and msDuration, containing MidiChordDefs having the relativeDurations.
		/// The constructed MidiChordDefs each have only one BasicMidiChordDef (i.e. they are not ornaments).
		/// The first MidiChordDef will be startMCD.BasicMidiChordDefs[0].
		/// The final MidiChordDef will be close to (but not the same as) targetMCD.BasicMidiChordDefs[0].
		/// The noteIndexLinks list contains pairs of indices. Each pair defines the start and end points of a chain:
		/// Each pair of indices links a note index in the startMCD with a note index in the targetMCD.
		/// If the noteIndexLinks list is null, then startMCD and targetMCD must have the same
		/// number of notes, and the chains link all the corresponding note indices.
		/// Note: PitchWheel and Pan envelopes can subsequently be set using the existing Trk functions:
		///     SetPitchWheelSliders(Envelope envelope) and SetPanSliders(Envelope envelope)
		/// </summary>
		/// <param name="midiChannel"></param>
		/// <param name="msDuration"></param>
		/// <param name="relativeDurations"></param>
		/// <param name="startMCD"></param>
		/// <param name="targetMCD"></param>
		/// <param name="pitchEnv">Start and end values must be the same, the lowest value must be 0</param>
		/// <param name="velocityEnv">Start and end values must be the same, the lowest value must be 0</param>
		/// <param name="noteIndexLinks">The note indices in the startMCD.BasicMidiChordDefs[0] and targetMCD.BasicMidiChordDefs[0]</param>
		public ChainTrk(int midiChannel, int msDuration, List<int> relativeDurations,
			MidiChordDef startMCD, MidiChordDef targetMCD,
			Envelope pitchEnv, Envelope velocityEnv,			
			List<Tuple<int,int>> noteIndexLinks )
			: base(midiChannel)
		{
			#region local functions
			void CheckEnvelope(Envelope env)
			{
				var orig = env.Original;
				if(orig[0] != orig[orig.Count - 1])
				{
					throw new ApplicationException("The envelope's start and end values must be the same.");
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
					throw new ApplicationException("The lowest envelope value must be zero.");
				}

			}
			void RemoveDuplicatePitches(List<List<byte>> pitchVals, List<List<byte>> velocityVals)
			{
				Debug.Assert(pitchVals.Count == velocityVals.Count);
				for(int i = 0; i < pitchVals.Count; i++)
				{
					var mcdPitches = pitchVals[i];
					var mcdVelocities = velocityVals[i];
					Debug.Assert(mcdPitches.Count == mcdVelocities.Count);
					if(mcdPitches.Count > 1)
					{
						for(int j = mcdPitches.Count - 1; j >= 0; j--)
						{
							for(int k = j - 1; k >= 0; k--)
							{
								if(mcdPitches[j] == mcdPitches[k])
								{
									mcdPitches.RemoveAt(j);
									mcdVelocities.RemoveAt(j);
									break;
								}
							}
						}
					}
				}
			}
			void SortPitches(List<List<byte>> pitchVals, List<List<byte>> velocityVals)
			{
				for(int i = 0; i < pitchVals.Count; i++)
				{
					var mcdPitches = pitchVals[i];
					var mcdVelocities = velocityVals[i];
					if(mcdPitches.Count > 1)
					{
						var unSortedPitches = new List<byte>(mcdPitches);
						mcdPitches.Sort();
						var unsortedVelocities = new List<byte>(mcdVelocities);

						for(int j = 0; j < mcdPitches.Count; j++)
						{
							var originalIndex = unSortedPitches.IndexOf(mcdPitches[j]);
							mcdVelocities[j] = unsortedVelocities[originalIndex];
						}
					}
				}
			}
			#endregion

			#region check pitchEnv and velocityEnv
			CheckEnvelope(pitchEnv);
			CheckEnvelope(velocityEnv);
			#endregion

			var startBMCD = startMCD.BasicMidiChordDefs[0];
			var targetBMCD = targetMCD.BasicMidiChordDefs[0];

			#region check/set noteIndexLinks
			if(noteIndexLinks == null)
			{
				Debug.Assert(startBMCD.Pitches.Count == targetBMCD.Pitches.Count);
				noteIndexLinks = new List<Tuple<int, int>>();
				for(int i = 0; i < startBMCD.Pitches.Count; i++)
				{
					noteIndexLinks.Add(new Tuple<int, int>(i, i));
				}
			}
			else
			{
				foreach(var pair in noteIndexLinks)
				{
					Debug.Assert(pair.Item1 < startBMCD.Pitches.Count);
					Debug.Assert(pair.Item2 < targetBMCD.Pitches.Count);
				}
			}
			#endregion

			var pitchLinks = new Dictionary<int, int>(); // contains <start,target> pairs
			var velocityLinks = new Dictionary<int, int>(); // contains <start,target> pairs
			foreach(var pair in noteIndexLinks)
			{
				pitchLinks.Add(startBMCD.Pitches[pair.Item1], targetBMCD.Pitches[pair.Item2]);
				velocityLinks.Add(startBMCD.Velocities[pair.Item1], targetBMCD.Velocities[pair.Item2]);
			}

			var nMidiChordDefs = relativeDurations.Count;

			List<List<byte>> pitchValues = GetValues(pitchEnv, nMidiChordDefs, pitchLinks);
			List<List<byte>> velocityValues = GetValues(velocityEnv, nMidiChordDefs, velocityLinks);
			RemoveDuplicatePitches(pitchValues, velocityValues);
			SortPitches(pitchValues, velocityValues);

			for(int i = 0; i < relativeDurations.Count; i++)
			{
				var pitches = pitchValues[i];
				var velocities = velocityValues[i];
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
		/// then sheared by the distance between the values in the dictionary.
		/// The first value does not change
		/// </summary>
		/// <param name="paramEnv"></param>
		/// <param name="nMidiChordDefs"></param>
		/// <param name="shearEnds"></param>
		/// <returns></returns>
		private List<List<byte>> GetValues(Envelope paramEnv, int nMidiChordDefs, Dictionary<int, int> shearEnds)
		{
			paramEnv.SetCount(nMidiChordDefs + 1);
			var envelope = paramEnv.Original;
			var firstEnvelopeVal = envelope[0];
			for(int i = 0; i < envelope.Count; i++)
			{
				envelope[i] -= firstEnvelopeVal;
			}

			var shearListsPerChain = new List<List<int>>();
			foreach(var chainEnds in shearEnds)
			{
				var shearIncrement = ((double)chainEnds.Value - chainEnds.Key) / nMidiChordDefs;
				var shearIncrementPerMCD = new List<int>();
				double increment = 0;
				for(int i = 0; i < nMidiChordDefs; i++)
				{
					shearIncrementPerMCD.Add((int)Math.Round(increment));
					increment += shearIncrement;
				}
				shearListsPerChain.Add(shearIncrementPerMCD);
			}

			List<List<byte>> rval = new List<List<byte>>();
			for(int i = 0; i < nMidiChordDefs; i++)
			{
				var mcdValues = new List<byte>();
				var j = 0;
				foreach(var chainEnds in shearEnds)
				{
					var startVal = chainEnds.Key;
					var envVal = envelope[i];
					double shearIncrement = shearListsPerChain[j][i];
					mcdValues.Add((byte)Math.Round(startVal + envVal + shearIncrement));
					j++;
				}
				rval.Add(mcdValues);
			}

			return rval;
		}
	}
}
