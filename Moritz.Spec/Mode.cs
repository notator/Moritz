using System;
using System.Collections.Generic;
using System.Diagnostics;
using Moritz.Globals;

namespace Moritz.Spec
{
	/// <summary>
	/// A Mode is a cloneable class, containing:
	///    1. AbsolutePitchWeightDict: an IReadOnlyDictionary whose KeyValuePairs contain
	///       Key: an absolute pitch (in range [0..11] C=0, C#=1, D=2 etc.) and
	///       Value: a weight (in range [1..127]) -- zero weight is not allowed.
	///    2. Gamut: a readonly list of PitchWeight objects (structs), in ascending order of their absolute pitch numbers (range [0..127]).
	///       Each absolute pitch in the Gamut exists at all possible octaves above Mode.Gamut[0].
	///       (Each octave range in the gamut contains the same absolute pitches.)
	///       The Gamut is initially constructed with each instance of an absolute pitch having the weight
	///       it has in the absPitchWeightDict argument.
	///       Gamut is a readonly attribute, but the weights may be changed later using dedicated Mode functions.
	/// Not all absolute pitches need to be included in the Mode.
	/// The Gamut pitches' weights can be used, for example, to determine their relative velocities, durations etc.
	/// </summary>
	public class Mode : ICloneable
	{
		#region constructors
		/// <param name="absPitchWeightDict">Keys must be in range [0..11], Values must be in range [1..127], Count must be in range [1..12]</param>
		public Mode(Dictionary<int, int> absPitchWeightDict)
		{
			Debug.Assert(absPitchWeightDict.Count >= 1 && absPitchWeightDict.Count <= 12);

			foreach(var pitchWeight in absPitchWeightDict)
			{
				var pitch = pitchWeight.Key;
				var weight = pitchWeight.Value;
				if(pitch < 0 || pitch > 127)
				{
					throw new ApplicationException("Illegal pitch.");
				}
				if(weight < 1 || pitch > 127) // weight 0 is not allowed here!
				{
					throw new ApplicationException("Illegal weight.");
				}
			}

			Gamut = GetGamut(absPitchWeightDict);
		}

		public Mode(List<PitchWeight> gamut)
		{
			Gamut = new List<PitchWeight>(gamut);
		}

		public object Clone() => new Mode(Gamut);

		/// <summary>
		/// Called by constructor.
		/// Returns the Mode's original Gamut (which can be changed by other Mode functions).
		/// Sets all relative values of each absolute pitch to the same weight.
		/// </summary>
		/// <returns></returns>
		private List<PitchWeight> GetGamut(IReadOnlyDictionary<int, int> absPitchWeightDict)
		{
			List<PitchWeight> absPitchWeightsByWeight = GetAbsPitchWeightsByWeight(absPitchWeightDict);
			int rootPitch = (int) absPitchWeightsByWeight[0].Pitch;

			List<UInt7> sortedBasePitches = new List<UInt7>();
			foreach(var pitchWeight in absPitchWeightsByWeight)
			{
				sortedBasePitches.Add(pitchWeight.Pitch);
			}
			sortedBasePitches.Sort();

			var gamut = new List<PitchWeight>();
			int rphIndex = 0;
			int octave = 0;
			while(true)
			{
				int pitch = (int) sortedBasePitches[rphIndex++] + (octave * 12);
				if(pitch > 127)
				{
					break;
				}

				if(pitch >= rootPitch)
				{
					int weight = absPitchWeightDict[(pitch % 12)];
					gamut.Add(new PitchWeight(pitch, weight));
				}

				if(rphIndex >= sortedBasePitches.Count)
				{
					rphIndex = 0;
					octave++;
				}
			}

			AssertGamutValidity(gamut);

			return gamut;
		}

		/// <summary>
		/// Returns _absPitchWeightsByWeight (pitchWeights sorted by weight)
		/// Pitches that have the same weight are returned in the unpredictable order returned by the Dictionary.
		/// </summary>
		private List<PitchWeight> GetAbsPitchWeightsByWeight(IReadOnlyDictionary<int, int> absPitchWeightDict)
		{
			List<int> weights = new List<int>();
			foreach(var kv in absPitchWeightDict)
			{
				weights.Add(kv.Value);
			}
			weights.Sort(); // small->large
			weights.Reverse(); // large->small

			var absPitchWeightsByWeight = new List<PitchWeight>();
			List<int> usedKeys = new List<int>();

			for(int i = 0; i < weights.Count; ++i)
			{
				int weight = weights[i];
				foreach(var kv in absPitchWeightDict)
				{
					if(kv.Value == weight && !usedKeys.Contains(kv.Key))
					{
						usedKeys.Add(kv.Key);
						absPitchWeightsByWeight.Add(new PitchWeight(kv.Key, kv.Value));
						break;
					}
				}
			}
			return absPitchWeightsByWeight;
		}

		#endregion constructors

		/// <summary>
		/// Transposes the pitches in this Mode up or down.
		/// The pitches in AbsolutePitchWeightDict are treated Mod 12 (stay in range 0..11)  
		/// Gamut Pitches less than 0 or greater than 127 are silently clipped.
		/// </summary>
		/// <param name="transposition"></param>
		public void Transpose(int transposition)
		{
			var newGamut = new List<PitchWeight>();
			foreach(var pitchWeight in Gamut)
			{
				int newPitch = pitchWeight.Pitch.Int + transposition;
				if(newPitch >= 0 && newPitch <= 127)
				{
					newGamut.Add(new PitchWeight(newPitch, (int) pitchWeight.Weight));
				}				
			}
			Gamut = newGamut;			
		}

		private void AssertGamutValidity()
		{
			AssertGamutValidity(Gamut);
		}

		/// <summary>
		/// Throws an exception if Gamut is invalid for any of the following reasons:
		/// 1. Gamut is null or empty.
		/// 2. All the pitch values must be different, in ascending order, and in range [0..127].
		/// 3. Each absolute pitch exists at all possible octaves above the base pitch.
		/// </summary>
		private void AssertGamutValidity(List<PitchWeight> gamut)
		{
			Debug.Assert(gamut != null && gamut.Count > 0, $"{nameof(gamut)} is null or empty.");

			for(int i = 1; i < gamut.Count; ++i)
			{
				Debug.Assert(gamut[i].Pitch > gamut[i - 1].Pitch, $"{nameof(gamut)} values must be in ascending pitch order.");
			}

			#region check pitch consistency
			List<int> basePitches = new List<int>();
			int pitchWeightIndex = 0;
			UInt7 octaveAboveBasePitch = gamut[0].Pitch + (UInt7)12;
			while(pitchWeightIndex < gamut.Count && gamut[pitchWeightIndex].Pitch < octaveAboveBasePitch)
			{
				basePitches.Add(gamut[pitchWeightIndex++].Pitch.Int);
			}
			int pitchWeightCount = 0;
			foreach(int pitch in basePitches)
			{
				int pitchOctave = pitch;
				while(pitchOctave < 128)
				{
					if(gamut.FindIndex(x => x.Pitch == (UInt7) pitchOctave) < 0)
					{
						throw new ApplicationException($"Missing pitch in {nameof(gamut)}");
					}
					pitchWeightCount += 1;
					pitchOctave += 12;
				}
			}
			Debug.Assert(gamut.Count == pitchWeightCount, $"Unknown pitch in {nameof(gamut)}.");
			#endregion check pitch consistency
		}

		public List<PitchWeight> Gamut { get; set; }
	}
}
