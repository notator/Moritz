using Moritz.Spec;
using System.Collections.Generic;

namespace Moritz.Algorithm.Study4
{
	public partial class Study4Algorithm : CompositionAlgorithm
	{
		private void GetTrksAndBarlines(GamutVector gamutVector, out List<Trk> trks, out List<int> barlineMsPositions)
		{
			const int barChordDuration = 1000; //ms
			const int relativePitch = 60; // added to the pitch values in the PitchWeightVectors to find the pitches in the Trks
			trks = GetTrks(gamutVector.PitchWeightVectors, relativePitch, barChordDuration);

			barlineMsPositions = new List<int>();
			foreach(var uid in trks[0].UniqueDefs)
			{
				barlineMsPositions.Add(uid.MsPositionReFirstUD + barChordDuration);
			}
		}

		/// <summary>
		/// Creates a list of 8 Trks, each of which contains 55 (=each pitchList.Count) MidiChordDefs.
		/// Each MidiChorddef has a duration of 1000ms. There is going to be 1 MidiChordDef per Bar.
		/// The last Trk is a tutti trk: each MidiChordDef contains all the pitches in the above Trks once.
		/// If a pitch would occur more than once in a tutti chord, then it occurs once with the largest
		/// velocity of any duplicated pitch.
		/// </summary>
		/// <param name="pitchVectors"></param>
		/// <returns></returns>
		private List<Trk> GetTrks(IReadOnlyList<PitchWeightVector> pitchVectors, int relativePitch, int barChordDuration)
		{
			List<Trk> trks = new List<Trk>();
			int channel = 0;
			foreach(var pitchVector in pitchVectors)
			{
				Trk trk = new Trk(channel++);
				IReadOnlyList<PitchWeight> pitchWeights = pitchVector.PitchWeights;
				foreach(var def in pitchVector.PitchWeights)
				{
					List<byte> pitches = new List<byte>() { (byte)(def.Pitch + relativePitch) };
					List<byte> velocities = new List<byte>() { (byte)def.Weight };
					IUniqueDef midiChordDef = new MidiChordDef(pitches, velocities, barChordDuration, true);
					trk.Add(midiChordDef);
				}

				trks.Add(trk);
			}
			// now create the bottom, tutti Trk
			int nChords = pitchVectors[0].PitchWeights.Count;
			List<IUniqueDef> tuttiChords = new List<IUniqueDef>();
			for(int i = 0; i < nChords; ++i)
			{
				List<byte> pitches = new List<byte>();
				List<byte> velocities = new List<byte>();

				for(int j= 0; j < pitchVectors.Count; j++)
				{
					PitchWeightVector pitchVector = pitchVectors[j];
					var def = pitchVector.PitchWeights[i];
					byte pitch = (byte)(def.Pitch + relativePitch);
					byte velocity = (byte)def.Weight;
					if(!pitches.Contains(pitch))
					{
						int insertIndex = pitches.Count;
						for(int k = 0; k < pitches.Count; ++k)
						{
							if(pitches[k] > pitch)
							{
								insertIndex = k;
								break;
							}
						}
						pitches.Insert(insertIndex, pitch);
						velocities.Insert(insertIndex, velocity);
					}
					else
					{
						int pitchIndex = pitches.IndexOf(pitch);
						byte existingVelocity = velocities[pitchIndex];
						velocities[pitchIndex] = (existingVelocity > velocity) ? existingVelocity : velocity;
					}
				}
				tuttiChords.Add(new MidiChordDef(pitches, velocities, barChordDuration, true));
			}
			trks.Add(new Trk(channel, 0, tuttiChords));

			return trks;
		}
	}
}
